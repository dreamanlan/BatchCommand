#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
File Copy Script - Copy files from source to target based on file list
Only copy files that don't exist in target directory
Support large-scale file processing with performance optimization
"""

import os
import shutil
import argparse
import fnmatch
from pathlib import Path
from typing import Generator, Tuple, List, Optional
from concurrent.futures import ThreadPoolExecutor, as_completed
import threading
import logging

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.StreamHandler(),
        logging.FileHandler('file_copy.log', encoding='utf-8')
    ]
)
logger = logging.getLogger(__name__)


class FileCopier:
    def __init__(self, source_dir: str, target_dir: str, file_list: str,
                 patterns: Optional[List[str]] = None,
                 dry_run: bool = True, quiet: bool = False, workers: int = 4,
                 overwrite: bool = False):
        """
        Initialize file copier

        Args:
            source_dir: Source directory path
            target_dir: Target directory path
            file_list: Path to text file containing relative file paths
            patterns: Filter patterns list (e.g., ['*.txt', '.log', 'temp_*']), None means all files
            dry_run: Preview only, don't actually copy
            quiet: Quiet mode, don't show detailed list
            workers: Number of parallel workers for copying
            overwrite: Overwrite existing files in target
        """
        self.source_dir = Path(source_dir).resolve()
        self.target_dir = Path(target_dir).resolve()
        self.file_list = Path(file_list).resolve()
        self.patterns = self._normalize_patterns(patterns) if patterns else None
        self.dry_run = dry_run
        self.quiet = quiet
        self.workers = workers
        self.overwrite = overwrite

        # Statistics
        self.stats = {
            'total': 0,
            'filtered': 0,
            'copied': 0,
            'skipped': 0,
            'failed': 0,
            'bytes_copied': 0
        }
        self.lock = threading.Lock()

    def _normalize_patterns(self, patterns: List[str]) -> List[str]:
        """Normalize filter patterns"""
        normalized = []
        for pattern in patterns:
            # Convert extension-only patterns (e.g., 'txt' or '.txt') to '*.txt'
            if pattern.startswith('.'):
                normalized.append(f'*{pattern}')
            elif not any(c in pattern for c in ['*', '?', '[']):
                normalized.append(f'*.{pattern}')
            else:
                normalized.append(pattern)
        return normalized

    def match_patterns(self, filename: str) -> bool:
        """Check if filename matches any filter pattern"""
        # If no patterns specified, match all files
        if self.patterns is None:
            return True

        for pattern in self.patterns:
            if fnmatch.fnmatch(filename, pattern):
                return True
        return False

    def validate_directories(self) -> bool:
        """Validate source and target directories"""
        if not self.source_dir.exists():
            logger.error(f"Source directory does not exist: {self.source_dir}")
            return False

        if not self.source_dir.is_dir():
            logger.error(f"Source path is not a directory: {self.source_dir}")
            return False

        if not self.target_dir.exists():
            logger.error(f"Target directory does not exist: {self.target_dir}")
            return False

        if not self.target_dir.is_dir():
            logger.error(f"Target path is not a directory: {self.target_dir}")
            return False

        if not self.file_list.exists():
            logger.error(f"File list does not exist: {self.file_list}")
            return False

        # Check if source and target are the same
        if self.source_dir == self.target_dir:
            logger.error("Source and target directories cannot be the same")
            return False

        return True

    def read_file_list(self) -> Generator[str, None, None]:
        """
        Read file list from text file and apply pattern filters
        Returns generator to avoid loading all files into memory

        Yields:
            Relative file path strings that match the patterns
        """
        logger.info(f"Reading file list: {self.file_list}")
        total_count = 0
        filtered_count = 0

        try:
            with open(self.file_list, 'r', encoding='utf-8') as f:
                for line_num, line in enumerate(f, 1):
                    line = line.strip()

                    # Skip empty lines and comments
                    if not line or line.startswith('#'):
                        continue

                    total_count += 1

                    # Apply pattern filter
                    filename = Path(line).name
                    if not self.match_patterns(filename):
                        continue

                    filtered_count += 1
                    yield line

            pattern_info = "all files" if self.patterns is None else f"patterns: {self.patterns}"
            logger.info(f"File list loaded: {total_count} files total, "
                       f"{filtered_count} files match filter ({pattern_info})")

            # Update stats
            with self.lock:
                self.stats['total'] = total_count
                self.stats['filtered'] = filtered_count

        except Exception as e:
            logger.error(f"Failed to read file list: {e}")
            raise

    def process_file(self, rel_path_str: str) -> Tuple[str, str, str, int]:
        """
        Process a single file: check if exists in target, copy if needed

        Args:
            rel_path_str: Relative path string from file list

        Returns:
            Tuple of (status, rel_path_str, message, bytes_copied)
            status: 'copied', 'skipped', 'failed'
        """
        try:
            rel_path = Path(rel_path_str)
            source_file = self.source_dir / rel_path
            target_file = self.target_dir / rel_path

            # Check if source file exists
            if not source_file.exists():
                return ('failed', rel_path_str, 'not found in source', 0)

            # Check if source is actually a file
            if not source_file.is_file():
                return ('failed', rel_path_str, 'not a file in source', 0)

            # Check if target file already exists
            if target_file.exists() and not self.overwrite:
                return ('skipped', rel_path_str, 'already exists in target', 0)

            # Get file size
            file_size = source_file.stat().st_size

            if self.dry_run:
                return ('copied', rel_path_str, 'would be copied', file_size)

            # Create target directory if needed
            target_file.parent.mkdir(parents=True, exist_ok=True)

            # Copy file with metadata
            shutil.copy2(source_file, target_file)

            return ('copied', rel_path_str, 'copied successfully', file_size)

        except PermissionError as e:
            return ('failed', rel_path_str, f'permission denied: {e}', 0)
        except OSError as e:
            return ('failed', rel_path_str, f'OS error: {e}', 0)
        except Exception as e:
            return ('failed', rel_path_str, f'unexpected error: {e}', 0)

    def update_stats(self, status: str, bytes_copied: int = 0) -> None:
        """Thread-safe statistics update"""
        with self.lock:
            self.stats[status] += 1
            if status == 'copied':
                self.stats['bytes_copied'] += bytes_copied

    def format_size(self, bytes_size: int) -> str:
        """Format bytes to human readable size"""
        for unit in ['B', 'KB', 'MB', 'GB', 'TB']:
            if bytes_size < 1024.0:
                return f"{bytes_size:.2f} {unit}"
            bytes_size /= 1024.0
        return f"{bytes_size:.2f} PB"

    def run(self) -> None:
        """Execute the copy operation"""
        if not self.validate_directories():
            return

        logger.info("=" * 60)
        logger.info("File copy task started")
        logger.info(f"Source directory: {self.source_dir}")
        logger.info(f"Target directory: {self.target_dir}")
        logger.info(f"File list:        {self.file_list}")
        logger.info(f"Filter patterns:  {self.patterns if self.patterns else 'ALL FILES'}")
        logger.info(f"Workers:          {self.workers}")
        logger.info(f"Mode:             {'DRY RUN (preview only)' if self.dry_run else 'ACTUAL COPY'}")
        logger.info(f"Overwrite:        {'YES' if self.overwrite else 'NO'}")
        logger.info(f"Quiet mode:       {'ON' if self.quiet else 'OFF'}")
        logger.info("=" * 60)

        # Process files in parallel
        with ThreadPoolExecutor(max_workers=self.workers) as executor:
            # Submit all tasks
            futures = {}
            for rel_path in self.read_file_list():
                future = executor.submit(self.process_file, rel_path)
                futures[future] = rel_path

            # Process results as they complete
            for i, future in enumerate(as_completed(futures), 1):
                status, rel_path, message, bytes_copied = future.result()
                self.update_stats(status, bytes_copied)

                # Show detailed output if not quiet
                if not self.quiet:
                    if status == 'copied':
                        prefix = "[WILL COPY]" if self.dry_run else "[COPIED]"
                        size_info = f" ({self.format_size(bytes_copied)})" if bytes_copied > 0 else ""
                        logger.info(f"{prefix} {rel_path}{size_info}")
                    elif status == 'failed':
                        logger.error(f"[FAILED] {rel_path}: {message}")
                    # Don't print skipped files to reduce noise

                # Progress indicator
                if i % 1000 == 0:
                    logger.info(f"Processed {i}/{self.stats['filtered']} files "
                              f"(copied: {self.stats['copied']}, "
                              f"skipped: {self.stats['skipped']}, "
                              f"failed: {self.stats['failed']})")

        # Print summary
        self.print_summary()

    def print_summary(self) -> None:
        """Print operation summary"""
        logger.info("=" * 60)
        logger.info("Copy task completed")
        logger.info(f"Total files in list:     {self.stats['total']}")
        logger.info(f"Files matching filter:   {self.stats['filtered']}")
        logger.info(f"Copied:                  {self.stats['copied']}")
        logger.info(f"Skipped:                 {self.stats['skipped']}")
        logger.info(f"Failed:                  {self.stats['failed']}")

        if self.stats['bytes_copied'] > 0:
            logger.info(f"Total size copied:       {self.format_size(self.stats['bytes_copied'])}")

        if self.dry_run and self.stats['copied'] > 0:
            logger.info("\nUse --execute to perform actual copy")
        elif not self.dry_run and self.stats['copied'] > 0:
            logger.info("\nCopy operation completed successfully")

        logger.info("=" * 60)


def main():
    parser = argparse.ArgumentParser(
        description='Copy files from source to target based on file list',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog='''
File list format (one relative path per line):
  subdir/file1.txt
  another/path/file2.dat
  # This is a comment
  docs/readme.md

Examples:
  # Preview mode - copy all files in list
  %(prog)s /source /target files.txt

  # Copy only .txt files
  %(prog)s /source /target files.txt -p "*.txt"

  # Copy multiple file types
  %(prog)s /source /target files.txt -p "*.txt" "*.log" ".dat"

  # Execute actual copy with 8 workers
  %(prog)s /source /target files.txt --execute -w 8

  # Copy only .jpg and .png files, quiet mode
  %(prog)s /source /target files.txt -p "*.jpg" "*.png" -e -q

  # Overwrite existing files
  %(prog)s /source /target files.txt --execute --overwrite

  # High performance: specific patterns, 16 workers, quiet mode
  %(prog)s /source /target files.txt -p "*.dat" "*.bin" -e -w 16 -q
        '''
    )

    parser.add_argument('source',
                       help='Source directory path')
    parser.add_argument('target',
                       help='Target directory path')
    parser.add_argument('filelist',
                       help='Text file containing relative file paths (one per line)')
    parser.add_argument('-p', '--patterns',
                       nargs='+',
                       default=None,
                       help='File filter patterns (support wildcards), e.g.: *.txt .log temp_* (default: all files)')
    parser.add_argument('-e', '--execute',
                       action='store_true',
                       help='Execute actual copy (default is preview only)')
    parser.add_argument('-q', '--quiet',
                       action='store_true',
                       help='Quiet mode, don\'t show detailed list')
    parser.add_argument('-w', '--workers',
                       type=int,
                       default=4,
                       help='Number of parallel workers (default: 4)')
    parser.add_argument('--overwrite',
                       action='store_true',
                       help='Overwrite existing files in target directory')

    args = parser.parse_args()

    try:
        copier = FileCopier(
            source_dir=args.source,
            target_dir=args.target,
            file_list=args.filelist,
            patterns=args.patterns,
            dry_run=not args.execute,
            quiet=args.quiet,
            workers=args.workers,
            overwrite=args.overwrite
        )
        copier.run()
    except KeyboardInterrupt:
        logger.warning("\nTask interrupted by user")
        return 1
    except Exception as e:
        logger.error(f"Execution failed: {e}", exc_info=True)
        return 1

    return 0


if __name__ == '__main__':
    exit(main())