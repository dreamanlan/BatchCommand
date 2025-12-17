#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
File and Directory Copy Script - Copy files and create directories based on file list
Read paths from file list, create directories or copy files accordingly
Support large-scale processing with performance optimization
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
import sys

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.StreamHandler(sys.stdout),
        logging.FileHandler('copy_create.log', encoding='utf-8')
    ]
)
logger = logging.getLogger(__name__)


class FileDirectoryCopier:
    def __init__(self, source_dir: str, target_dir: str, file_list: str,
                 file_patterns: Optional[List[str]] = None,
                 dir_patterns: Optional[List[str]] = None,
                 dry_run: bool = True, quiet: bool = False, workers: int = 4,
                 overwrite: bool = False):
        """
        Initialize file and directory copier

        Args:
            source_dir: Source directory path
            target_dir: Target directory path
            file_list: Path to text file containing paths (absolute, relative, or from source/target)
            file_patterns: File filter patterns (e.g., ['*.txt', 'log', '.dat']), None means all files
            dir_patterns: Directory filter patterns (e.g., ['images', 'temp_*']), None means all directories
            dry_run: Preview only, don't actually copy/create
            quiet: Quiet mode, don't show detailed list
            workers: Number of parallel workers for copying
            overwrite: Overwrite existing files in target
        """
        self.source_dir = Path(source_dir).resolve()
        self.target_dir = Path(target_dir).resolve()
        self.file_list = Path(file_list).resolve()
        self.file_patterns = self._normalize_file_patterns(file_patterns) if file_patterns else None
        self.dir_patterns = self._normalize_dir_patterns(dir_patterns) if dir_patterns else None
        self.dry_run = dry_run
        self.quiet = quiet
        self.workers = workers
        self.overwrite = overwrite

        # Statistics
        self.stats = {
            'total_lines': 0,
            'files_filtered': 0,
            'dirs_filtered': 0,
            'dirs_to_create': 0,
            'dirs_created': 0,
            'dirs_existed': 0,
            'dirs_failed': 0,
            'files_to_copy': 0,
            'files_copied': 0,
            'files_skipped': 0,
            'files_failed': 0,
            'not_found': 0,
            'bytes_copied': 0
        }
        self.lock = threading.Lock()

    def _normalize_file_patterns(self, patterns: List[str]) -> List[str]:
        """
        Normalize file filter patterns

        Rules:
        - If starts with '.': convert to '*.ext' (e.g., '.txt' -> '*.txt')
        - If no wildcards and no '.': treat as extension (e.g., 'txt' -> '*.txt')
        - Otherwise: keep as is (e.g., '*.log', 'config_*.json')
        """
        normalized = []
        for pattern in patterns:
            if pattern.startswith('.'):
                # .txt -> *.txt
                normalized.append(f'*{pattern}')
            elif not any(c in pattern for c in ['*', '?', '[']):
                # No wildcards
                if '.' not in pattern:
                    # txt -> *.txt (treat as extension)
                    normalized.append(f'*.{pattern}')
                else:
                    # file.txt -> file.txt (exact match with extension)
                    normalized.append(pattern)
            else:
                # Has wildcards: *.log, config_*.json, etc.
                normalized.append(pattern)
        return normalized

    def _normalize_dir_patterns(self, patterns: List[str]) -> List[str]:
        """
        Normalize directory filter patterns

        Rules:
        - If starts with '.': convert to '*.ext' (for consistency, though rare for dirs)
        - If no wildcards and no '.': treat as exact directory name (e.g., 'images' -> 'images')
        - Otherwise: keep as is (e.g., 'temp_*', '*_backup')
        """
        normalized = []
        for pattern in patterns:
            if pattern.startswith('.'):
                # .hidden -> *.hidden (rare case)
                normalized.append(f'*{pattern}')
            elif not any(c in pattern for c in ['*', '?', '[']):
                # No wildcards
                if '.' not in pattern:
                    # images -> images (exact directory name)
                    normalized.append(pattern)
                else:
                    # dir.name -> dir.name (exact match, rare but possible)
                    normalized.append(pattern)
            else:
                # Has wildcards: temp_*, *_backup, etc.
                normalized.append(pattern)
        return normalized

    def _convert_to_relative_path(self, path_str: str) -> Optional[str]:
        """
        Convert path to relative path

        Try to convert absolute or source/target-based paths to relative paths.

        Args:
            path_str: Path string from file list

        Returns:
            Relative path string, or None if path cannot be resolved
        """
        path_str = path_str.strip()

        # If empty, return None
        if not path_str:
            return None

        try:
            # Try to resolve as Path
            path = Path(path_str)

            # If it's already a relative path and exists in source, use it directly
            if not path.is_absolute():
                source_path = self.source_dir / path
                if source_path.exists():
                    return str(path)
                # If not exists in source, still return it (will be handled later)
                return str(path)

            # If it's an absolute path, try to get relative path from source or target
            abs_path = path.resolve()

            # Try to get relative path from source directory
            try:
                rel_from_source = abs_path.relative_to(self.source_dir)
                return str(rel_from_source)
            except ValueError:
                pass

            # Try to get relative path from target directory
            try:
                rel_from_target = abs_path.relative_to(self.target_dir)
                return str(rel_from_target)
            except ValueError:
                pass

            # If path is not under source or target, return None
            logger.warning(f"Path is not under source or target directory: {path_str}")
            return None

        except Exception as e:
            logger.warning(f"Failed to process path '{path_str}': {e}")
            return None

    def match_patterns(self, name: str, patterns: Optional[List[str]]) -> bool:
        """Check if name matches any pattern"""
        # If no patterns specified, match all
        if patterns is None:
            return True

        for pattern in patterns:
            if fnmatch.fnmatch(name, pattern):
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

        if not self.file_list.is_file():
            logger.error(f"File list is not a file: {self.file_list}")
            return False

        # Check if source and target are the same
        if self.source_dir == self.target_dir:
            logger.error("Source and target directories cannot be the same")
            return False

        return True

    def read_file_list(self) -> Generator[Tuple[str, str], None, None]:
        """
        Read file list and apply pattern filters

        Yields:
            Tuple of (path_type, rel_path_str)
            path_type: 'file', 'dir', or 'unknown'
        """
        logger.info(f"Reading file list: {self.file_list}")
        total_count = 0
        file_filtered = 0
        dir_filtered = 0
        skipped_count = 0

        try:
            with open(self.file_list, 'r', encoding='utf-8') as f:
                for line_num, line in enumerate(f, 1):
                    line = line.strip()

                    # Skip empty lines and comments
                    if not line or line.startswith('#'):
                        continue

                    total_count += 1

                    # Convert to relative path
                    rel_path_str = self._convert_to_relative_path(line)

                    if rel_path_str is None:
                        skipped_count += 1
                        continue

                    # Check if it exists in source and determine type
                    source_path = self.source_dir / rel_path_str

                    if not source_path.exists():
                        # Path doesn't exist in source
                        yield ('unknown', rel_path_str)
                        continue

                    # Get the name (basename) for pattern matching
                    name = Path(rel_path_str).name

                    if source_path.is_dir():
                        # Apply directory pattern filter
                        if self.match_patterns(name, self.dir_patterns):
                            dir_filtered += 1
                            yield ('dir', rel_path_str)
                    elif source_path.is_file():
                        # Apply file pattern filter
                        if self.match_patterns(name, self.file_patterns):
                            file_filtered += 1
                            yield ('file', rel_path_str)

            file_pattern_info = "all files" if self.file_patterns is None else f"file patterns: {self.file_patterns}"
            dir_pattern_info = "all directories" if self.dir_patterns is None else f"dir patterns: {self.dir_patterns}"

            logger.info(f"File list loaded: {total_count} paths total, {skipped_count} skipped")
            logger.info(f"  Files matched: {file_filtered} ({file_pattern_info})")
            logger.info(f"  Directories matched: {dir_filtered} ({dir_pattern_info})")

            # Update stats
            with self.lock:
                self.stats['total_lines'] = total_count
                self.stats['files_filtered'] = file_filtered
                self.stats['dirs_filtered'] = dir_filtered

        except UnicodeDecodeError as e:
            logger.error(f"Failed to read file list (encoding error): {e}")
            logger.info("Try saving the file list with UTF-8 encoding")
            raise
        except Exception as e:
            logger.error(f"Failed to read file list: {e}")
            raise

    def create_directory(self, rel_path_str: str) -> Tuple[str, str]:
        """
        Create directory in target if it doesn't exist

        Args:
            rel_path_str: Relative path string from file list

        Returns:
            Tuple of (status, message)
            status: 'created', 'existed', 'not_found', 'failed'
        """
        try:
            rel_path = Path(rel_path_str)
            source_dir = self.source_dir / rel_path
            target_dir = self.target_dir / rel_path

            # Check if source directory exists
            if not source_dir.exists():
                return ('not_found', 'not found in source')

            if not source_dir.is_dir():
                return ('not_found', 'not a directory in source')

            # Check if target directory already exists
            if target_dir.exists():
                if target_dir.is_dir():
                    return ('existed', 'already exists in target')
                else:
                    return ('failed', 'path exists in target but is not a directory')

            if self.dry_run:
                return ('created', 'would be created')

            # Create directory
            target_dir.mkdir(parents=True, exist_ok=True)

            return ('created', 'created successfully')

        except PermissionError as e:
            return ('failed', f'permission denied: {e}')
        except OSError as e:
            return ('failed', f'OS error: {e}')
        except Exception as e:
            return ('failed', f'unexpected error: {e}')

    def copy_file(self, rel_path_str: str) -> Tuple[str, str, int]:
        """
        Copy file to target

        Args:
            rel_path_str: Relative path string from file list

        Returns:
            Tuple of (status, message, bytes_copied)
            status: 'copied', 'skipped', 'not_found', 'failed'
        """
        try:
            rel_path = Path(rel_path_str)
            source_file = self.source_dir / rel_path
            target_file = self.target_dir / rel_path

            # Check if source file exists
            if not source_file.exists():
                return ('not_found', 'not found in source', 0)

            if not source_file.is_file():
                return ('not_found', 'not a file in source', 0)

            # Check if target file already exists
            if target_file.exists() and not self.overwrite:
                return ('skipped', 'already exists in target', 0)

            # Get file size
            file_size = source_file.stat().st_size

            if self.dry_run:
                return ('copied', 'would be copied', file_size)

            # Create parent directory if needed
            target_file.parent.mkdir(parents=True, exist_ok=True)

            # Copy file with metadata
            shutil.copy2(source_file, target_file)

            return ('copied', 'copied successfully', file_size)

        except PermissionError as e:
            return ('failed', f'permission denied: {e}', 0)
        except OSError as e:
            return ('failed', f'OS error: {e}', 0)
        except Exception as e:
            return ('failed', f'unexpected error: {e}', 0)

    def update_stats(self, category: str, status: str, bytes_copied: int = 0) -> None:
        """Thread-safe statistics update"""
        with self.lock:
            if category == 'dir':
                self.stats[f'dirs_{status}'] += 1
            elif category == 'file':
                self.stats[f'files_{status}'] += 1
                if status == 'copied':
                    self.stats['bytes_copied'] += bytes_copied

            if status == 'not_found':
                self.stats['not_found'] += 1

    def format_size(self, bytes_size: int) -> str:
        """Format bytes to human readable size"""
        for unit in ['B', 'KB', 'MB', 'GB', 'TB']:
            if bytes_size < 1024.0:
                return f"{bytes_size:.2f} {unit}"
            bytes_size /= 1024.0
        return f"{bytes_size:.2f} PB"

    def run(self) -> None:
        """Execute the copy and create operation"""
        if not self.validate_directories():
            return

        logger.info("=" * 60)
        logger.info("File and directory copy task started")
        logger.info(f"Source directory:    {self.source_dir}")
        logger.info(f"Target directory:    {self.target_dir}")
        logger.info(f"File list:           {self.file_list}")
        logger.info(f"File patterns:       {self.file_patterns if self.file_patterns else 'ALL FILES'}")
        logger.info(f"Directory patterns:  {self.dir_patterns if self.dir_patterns else 'ALL DIRECTORIES'}")
        logger.info(f"Workers:             {self.workers}")
        logger.info(f"Mode:                {'PREVIEW (no actual copy/create)' if self.dry_run else 'ACTUAL COPY/CREATE'}")
        logger.info(f"Overwrite:           {'YES' if self.overwrite else 'NO'}")
        logger.info(f"Quiet mode:          {'ON' if self.quiet else 'OFF'}")
        logger.info("=" * 60)

        # Separate directories and files
        dirs_to_create = []
        files_to_copy = []

        for path_type, rel_path in self.read_file_list():
            if path_type == 'dir':
                dirs_to_create.append(rel_path)
                self.stats['dirs_to_create'] += 1
            elif path_type == 'file':
                files_to_copy.append(rel_path)
                self.stats['files_to_copy'] += 1
            else:
                # Unknown type (not found in source)
                with self.lock:
                    self.stats['not_found'] += 1
                if not self.quiet:
                    logger.warning(f"[NOT FOUND] {rel_path}")

        if not dirs_to_create and not files_to_copy:
            logger.warning("No valid directories or files to process after filtering")
            return

        # Process directories first (create them)
        if dirs_to_create:
            logger.info(f"Processing {len(dirs_to_create)} directories...")

            for rel_path in dirs_to_create:
                status, message = self.create_directory(rel_path)
                self.update_stats('dir', status)

                if not self.quiet:
                    if status == 'created':
                        prefix = "[WILL CREATE]" if self.dry_run else "[CREATED]"
                        logger.info(f"{prefix} {rel_path}/")
                    elif status == 'existed':
                        logger.debug(f"[EXISTS] {rel_path}/")
                    elif status == 'not_found':
                        logger.warning(f"[NOT FOUND] {rel_path}: {message}")
                    elif status == 'failed':
                        logger.error(f"[FAILED] {rel_path}/: {message}")

        # Process files (copy them)
        if files_to_copy:
            logger.info(f"Processing {len(files_to_copy)} files with {self.workers} workers...")

            with ThreadPoolExecutor(max_workers=self.workers) as executor:
                # Submit all tasks
                futures = {
                    executor.submit(self.copy_file, rel_path): rel_path
                    for rel_path in files_to_copy
                }

                # Process results as they complete
                for i, future in enumerate(as_completed(futures), 1):
                    try:
                        rel_path = futures[future]
                        status, message, bytes_copied = future.result()
                        self.update_stats('file', status, bytes_copied)

                        # Show detailed output if not quiet
                        if not self.quiet:
                            if status == 'copied':
                                prefix = "[WILL COPY]" if self.dry_run else "[COPIED]"
                                size_info = f" ({self.format_size(bytes_copied)})" if bytes_copied > 0 else ""
                                logger.info(f"{prefix} {rel_path}{size_info}")
                            elif status == 'not_found':
                                logger.warning(f"[NOT FOUND] {rel_path}: {message}")
                            elif status == 'failed':
                                logger.error(f"[FAILED] {rel_path}: {message}")
                            # Don't print skipped files to reduce noise

                        # Progress indicator
                        if i % 1000 == 0 or i == len(files_to_copy):
                            logger.info(f"Processed {i}/{len(files_to_copy)} files "
                                      f"(copied: {self.stats['files_copied']}, "
                                      f"skipped: {self.stats['files_skipped']}, "
                                      f"failed: {self.stats['files_failed']})")
                    except Exception as e:
                        logger.error(f"Error processing file: {e}")

        # Print summary
        self.print_summary()

    def print_summary(self) -> None:
        """Print operation summary"""
        logger.info("=" * 60)
        logger.info("Task completed")
        logger.info(f"Total paths in list:     {self.stats['total_lines']}")
        logger.info(f"Not found in source:     {self.stats['not_found']}")
        logger.info("")
        logger.info("Directories:")
        logger.info(f"  Matched by filter:     {self.stats['dirs_filtered']}")
        logger.info(f"  To create:             {self.stats['dirs_to_create']}")
        logger.info(f"  Created:               {self.stats['dirs_created']}")
        logger.info(f"  Already existed:       {self.stats['dirs_existed']}")
        logger.info(f"  Failed:                {self.stats['dirs_failed']}")
        logger.info("")
        logger.info("Files:")
        logger.info(f"  Matched by filter:     {self.stats['files_filtered']}")
        logger.info(f"  To copy:               {self.stats['files_to_copy']}")
        logger.info(f"  Copied:                {self.stats['files_copied']}")
        logger.info(f"  Skipped:               {self.stats['files_skipped']}")
        logger.info(f"  Failed:                {self.stats['files_failed']}")

        if self.stats['bytes_copied'] > 0:
            logger.info(f"  Total size copied:     {self.format_size(self.stats['bytes_copied'])}")

        if self.dry_run and (self.stats['files_copied'] > 0 or self.stats['dirs_created'] > 0):
            logger.info("\nUse --execute to perform actual copy/create")
        elif not self.dry_run and (self.stats['files_copied'] > 0 or self.stats['dirs_created'] > 0):
            logger.info("\nOperation completed successfully")

        logger.info("=" * 60)


def main():
    parser = argparse.ArgumentParser(
        description='Copy files and create directories based on file list',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog='''
File list format (one path per line, supports multiple formats):
  # Relative paths
  subdir/file1.txt
  images/

  # Absolute paths from source directory
  /source/project/docs/readme.txt
  /source/project/images/

  # Absolute paths from target directory
  /target/backup/old/
  /target/data.log

  # Comments
  # This is a comment

Path resolution:
  1. If path is relative: use as-is (relative to source)
  2. If path is absolute and under source directory: convert to relative path from source
  3. If path is absolute and under target directory: convert to relative path from target
  4. Otherwise: skip the path with warning

Examples:
  # Preview mode - process all paths in list
  %(prog)s /source /target paths.txt

  # Copy .txt and .jpg files, create "images" and "docs" directories
  %(prog)s /source /target paths.txt -f txt jpg -d images docs

  # Using different pattern styles
  %(prog)s /source /target paths.txt -f "*.log" ".dat" json -d "temp_*" "backup"

  # Execute actual copy/create with 8 workers
  %(prog)s /source /target paths.txt -f jpg png -d images --execute -w 8

  # Process only files (all directories will be filtered out)
  %(prog)s /source /target paths.txt -f jpg png txt -e

  # Process only directories (all files will be filtered out)
  %(prog)s /source /target paths.txt -d images docs backup -e

  # Overwrite existing files
  %(prog)s /source /target paths.txt -f conf json -e --overwrite

  # High performance: 16 workers, quiet mode
  %(prog)s /source /target paths.txt -f dat bin -d "data_*" -e -w 16 -q

Pattern examples:

  File patterns (-f):
    txt            -> *.txt           (extension: matches file.txt, data.txt)
    .log           -> *.log           (extension with dot)
    *.dat          -> *.dat           (wildcard: matches any .dat file)
    config_*.json  -> config_*.json   (wildcard: matches config_dev.json, config_prod.json)
    readme.txt     -> readme.txt      (exact match: only matches readme.txt)

  Directory patterns (-d):
    images         -> images          (exact name: only matches "images")
    temp_*         -> temp_*          (wildcard: matches temp_2024, temp_backup)
    *_backup       -> *_backup        (wildcard: matches data_backup, old_backup)
    .git           -> *.git           (rare: hidden directories)

File list examples:

  Example 1 - Mixed relative and absolute paths:
    project/images
    /source/project/docs
    /target/backup/old
    project/data.txt
    /source/config.json

  Example 2 - All relative paths:
    images/
    docs/
    images/photo.jpg
    docs/readme.txt
    config.json

  Example 3 - Generated from find command:
    # find /source -name "*.jpg" > files.txt
    /source/project/images/photo1.jpg
    /source/project/images/photo2.jpg
    /source/gallery/pic.jpg

How it works:
  1. Read each line from the file list
  2. Convert to relative path (from source or target directory)
  3. Check if path exists in source directory
  4. Determine if it's a file or directory
  5. Apply corresponding pattern filter
  6. If it's a directory and matches: create it in target (if not exists)
  7. If it's a file and matches: copy it to target (with same relative path)
        '''
    )

    parser.add_argument('source',
                       help='Source directory path')
    parser.add_argument('target',
                       help='Target directory path')
    parser.add_argument('filelist',
                       help='Text file containing paths (relative or absolute, one per line)')
    parser.add_argument('-f', '--file-patterns',
                       nargs='+',
                       default=None,
                       help='File filter patterns. Without wildcards/dot: treated as extension (e.g., txt -> *.txt). '
                            'Examples: txt, .log, *.dat, config_*.json (default: all files)')
    parser.add_argument('-d', '--dir-patterns',
                       nargs='+',
                       default=None,
                       help='Directory filter patterns. Without wildcards/dot: treated as exact name (e.g., images -> images). '
                            'Examples: images, temp_*, *_backup (default: all directories)')
    parser.add_argument('-e', '--execute',
                       action='store_true',
                       help='Execute actual copy/create (default is preview only)')
    parser.add_argument('-q', '--quiet',
                       action='store_true',
                       help='Quiet mode, don\'t show detailed list')
    parser.add_argument('-w', '--workers',
                       type=int,
                       default=4,
                       help='Number of parallel workers for file copying (default: 4)')
    parser.add_argument('--overwrite',
                       action='store_true',
                       help='Overwrite existing files in target directory')

    args = parser.parse_args()

    try:
        copier = FileDirectoryCopier(
            source_dir=args.source,
            target_dir=args.target,
            file_list=args.filelist,
            file_patterns=args.file_patterns,
            dir_patterns=args.dir_patterns,
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