#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
File Cleanup Script - Delete files not in whitelist
Support large-scale file processing with performance optimization
"""

import os
import sys
import argparse
import fnmatch
from pathlib import Path
from typing import Set, List, Optional
import logging
from concurrent.futures import ThreadPoolExecutor, as_completed

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.StreamHandler(sys.stdout),
        logging.FileHandler('file_cleanup.log', encoding='utf-8')
    ]
)
logger = logging.getLogger(__name__)


class FileCleanup:
    def __init__(self, target_dir: str, whitelist_file: str, patterns: Optional[List[str]] = None,
                 dry_run: bool = True, workers: int = 4, quiet: bool = False):
        """
        Initialize file cleanup handler

        Args:
            target_dir: Target directory path
            whitelist_file: Whitelist file path
            patterns: Filter patterns list (e.g., ['*.txt', '.log', 'temp_*']), None means all files
            dry_run: Preview only, don't actually delete
            workers: Number of parallel workers
            quiet: Quiet mode (don't show detailed list)
        """
        self.target_dir = Path(target_dir).resolve()
        self.whitelist_file = whitelist_file
        self.patterns = self._normalize_patterns(patterns) if patterns else None
        self.dry_run = dry_run
        self.workers = workers
        self.quiet = quiet
        self.whitelist: Set[str] = set()

        # Statistics
        self.stats = {
            'scanned': 0,
            'deleted': 0,
            'kept': 0,
            'errors': 0,
            'empty_dirs_removed': 0
        }

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

    def load_whitelist(self) -> None:
        """Load whitelist file with memory optimization"""
        logger.info(f"Loading whitelist file: {self.whitelist_file}")
        try:
            with open(self.whitelist_file, 'r', encoding='utf-8') as f:
                # Use set for O(1) lookup performance
                # Store both absolute and relative paths in normalized form
                for line in f:
                    line = line.strip()
                    if line and not line.startswith('#'):
                        # Support both absolute and relative paths
                        self.whitelist.add(line)
                        # Also add normalized absolute path
                        try:
                            abs_path = str(Path(line).resolve())
                            self.whitelist.add(abs_path)
                        except:
                            pass

            logger.info(f"Whitelist loaded successfully, {len(self.whitelist)} entries")
        except Exception as e:
            logger.error(f"Failed to load whitelist file: {e}")
            raise

    def match_patterns(self, filename: str) -> bool:
        """Check if filename matches any filter pattern"""
        # If no patterns specified, match all files
        if self.patterns is None:
            return True

        for pattern in self.patterns:
            if fnmatch.fnmatch(filename, pattern):
                return True
        return False

    def is_in_whitelist(self, file_path: Path) -> bool:
        """Check if file is in whitelist"""
        # Check multiple path formats
        checks = [
            str(file_path),  # Absolute path
            str(file_path.resolve()),  # Resolved absolute path
            file_path.name,  # Filename only
        ]

        # Try relative path to target directory
        try:
            rel_path = str(file_path.relative_to(self.target_dir))
            checks.append(rel_path)
        except ValueError:
            pass

        return any(check in self.whitelist for check in checks)

    def scan_and_collect_files(self) -> List[Path]:
        """Scan directory and collect files to process"""
        logger.info(f"Starting directory scan: {self.target_dir}")
        files_to_process = []

        try:
            # os.walk is faster than Path.rglob for large directories
            for root, dirs, files in os.walk(self.target_dir):
                root_path = Path(root)

                for filename in files:
                    self.stats['scanned'] += 1

                    # Progress indicator (only in non-quiet mode)
                    if not self.quiet and self.stats['scanned'] % 10000 == 0:
                        logger.info(f"Scanned {self.stats['scanned']} files...")

                    # Check if matches filter patterns
                    if not self.match_patterns(filename):
                        continue

                    file_path = root_path / filename
                    files_to_process.append(file_path)

            pattern_info = "all files" if self.patterns is None else f"patterns: {self.patterns}"
            logger.info(f"Scan completed, total {self.stats['scanned']} files, "
                       f"{len(files_to_process)} files match filter ({pattern_info})")
        except Exception as e:
            logger.error(f"Error during directory scan: {e}")
            raise

        return files_to_process

    def process_file(self, file_path: Path) -> bool:
        """Process single file (delete or keep)"""
        try:
            if self.is_in_whitelist(file_path):
                self.stats['kept'] += 1
                return False
            else:
                # File needs to be deleted
                if self.dry_run:
                    if not self.quiet:
                        logger.info(f"[WILL DELETE] {file_path}")
                else:
                    file_path.unlink()
                    if not self.quiet:
                        logger.debug(f"[DELETED] {file_path}")

                self.stats['deleted'] += 1
                return True
        except Exception as e:
            logger.error(f"Failed to process file {file_path}: {e}")
            self.stats['errors'] += 1
            return False

    def process_files_batch(self, files: List[Path]) -> None:
        """Process files in batch with multi-threading"""
        logger.info(f"Starting file processing with {self.workers} workers...")

        if self.workers > 1:
            with ThreadPoolExecutor(max_workers=self.workers) as executor:
                futures = {executor.submit(self.process_file, f): f for f in files}

                for i, future in enumerate(as_completed(futures), 1):
                    try:
                        future.result()
                        # Progress indicator (only in non-quiet mode)
                        if not self.quiet and i % 1000 == 0:
                            logger.info(f"Processed {i}/{len(files)} files "
                                      f"(deleted: {self.stats['deleted']}, "
                                      f"kept: {self.stats['kept']})")
                    except Exception as e:
                        logger.error(f"Error processing file: {e}")
        else:
            # Single-threaded processing
            for i, file_path in enumerate(files, 1):
                self.process_file(file_path)
                if i % 1000 == 0:
                    logger.info(f"Processed {i}/{len(files)} files")

    def remove_empty_directories(self) -> None:
        """Remove empty directories (bottom-up approach)"""
        logger.info("Starting empty directory cleanup...")

        # Collect all directories using BFS from bottom to top
        all_dirs = []
        for root, dirs, files in os.walk(self.target_dir, topdown=False):
            for dirname in dirs:
                dir_path = Path(root) / dirname
                all_dirs.append(dir_path)

        for dir_path in all_dirs:
            try:
                # Check if directory is empty
                if dir_path.exists() and not any(dir_path.iterdir()):
                    if self.dry_run:
                        if not self.quiet:
                            logger.info(f"[WILL DELETE] Empty directory: {dir_path}")
                    else:
                        dir_path.rmdir()
                        if not self.quiet:
                            logger.debug(f"[DELETED] Empty directory: {dir_path}")

                    self.stats['empty_dirs_removed'] += 1
            except Exception as e:
                logger.error(f"Failed to delete empty directory {dir_path}: {e}")

        logger.info(f"Empty directory cleanup completed, removed {self.stats['empty_dirs_removed']} directories")

    def run(self) -> None:
        """Execute cleanup workflow"""
        logger.info("=" * 60)
        logger.info("File cleanup task started")
        logger.info(f"Target directory: {self.target_dir}")
        logger.info(f"Whitelist file: {self.whitelist_file}")
        logger.info(f"Filter patterns: {self.patterns if self.patterns else 'ALL FILES'}")
        logger.info(f"Workers: {self.workers}")
        logger.info(f"Mode: {'PREVIEW (no actual deletion)' if self.dry_run else 'ACTUAL DELETION'}")
        logger.info(f"Quiet mode: {'ON' if self.quiet else 'OFF'}")
        logger.info("=" * 60)

        # 1. Load whitelist
        self.load_whitelist()

        # 2. Scan and collect files
        files_to_process = self.scan_and_collect_files()

        # 3. Process files
        if files_to_process:
            self.process_files_batch(files_to_process)

        # 4. Remove empty directories
        if not self.dry_run:
            self.remove_empty_directories()

        # 5. Print summary
        self.print_summary()

    def print_summary(self) -> None:
        """Print statistics summary"""
        logger.info("=" * 60)
        logger.info("Cleanup task completed")
        logger.info(f"Total files scanned: {self.stats['scanned']}")
        logger.info(f"Files deleted: {self.stats['deleted']}")
        logger.info(f"Files kept: {self.stats['kept']}")
        logger.info(f"Empty directories removed: {self.stats['empty_dirs_removed']}")
        logger.info(f"Errors: {self.stats['errors']}")

        if self.dry_run and self.stats['deleted'] > 0:
            logger.info("\nUse --execute to perform actual deletion")
        elif not self.dry_run and self.stats['deleted'] > 0:
            logger.info("\nDeletion operation completed successfully")

        logger.info("=" * 60)


def main():
    parser = argparse.ArgumentParser(
        description='Clean up directory files based on whitelist',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # Preview mode - show what would be deleted (all files)
  %(prog)s /path/to/target whitelist.txt

  # Preview mode - only check .txt files
  %(prog)s /path/to/target whitelist.txt -p "*.txt"

  # Execute actual deletion of all files not in whitelist
  %(prog)s /path/to/target whitelist.txt --execute

  # Delete only .txt and .log files not in whitelist
  %(prog)s /path/to/target whitelist.txt -p "*.txt" "*.log" -e

  # High performance mode with 8 workers
  %(prog)s /path/to/target whitelist.txt -e -w 8

  # Quiet mode (only show summary)
  %(prog)s /path/to/target whitelist.txt -e -q

  # Combination: specific patterns, 16 workers, execute, quiet
  %(prog)s /path/to/target whitelist.txt -p "*.tmp" "*.log" -e -w 16 -q
        """
    )

    parser.add_argument('target_dir',
                       help='Target directory path')
    parser.add_argument('whitelist_file',
                       help='Whitelist file path (one file path per line)')
    parser.add_argument('-p', '--patterns',
                       nargs='+',
                       default=None,
                       help='File filter patterns (support wildcards), e.g.: *.txt .log temp_* (default: all files)')
    parser.add_argument('-w', '--workers',
                       type=int,
                       default=4,
                       help='Number of parallel workers (default: 4)')
    parser.add_argument('-e', '--execute',
                       action='store_true',
                       help='Execute actual deletion (default is preview only)')
    parser.add_argument('-q', '--quiet',
                       action='store_true',
                       help='Quiet mode, don\'t show detailed list')

    args = parser.parse_args()

    # Validate arguments
    if not os.path.isdir(args.target_dir):
        logger.error(f"Target directory does not exist: {args.target_dir}")
        sys.exit(1)

    if not os.path.isfile(args.whitelist_file):
        logger.error(f"Whitelist file does not exist: {args.whitelist_file}")
        sys.exit(1)

    # Execute cleanup
    try:
        cleaner = FileCleanup(
            target_dir=args.target_dir,
            whitelist_file=args.whitelist_file,
            patterns=args.patterns,
            dry_run=not args.execute,
            workers=args.workers,
            quiet=args.quiet
        )
        cleaner.run()
    except KeyboardInterrupt:
        logger.warning("\nTask interrupted by user")
        sys.exit(1)
    except Exception as e:
        logger.error(f"Execution failed: {e}", exc_info=True)
        sys.exit(1)


if __name__ == '__main__':
    main()