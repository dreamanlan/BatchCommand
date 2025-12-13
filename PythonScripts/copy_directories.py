#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Directory Copy Script - Copy specified directories from source to target
Support large-scale directory processing with performance optimization
"""

import os
import shutil
import argparse
from pathlib import Path
from typing import List, Set, Optional, Dict
from concurrent.futures import ThreadPoolExecutor, as_completed
import threading
import logging

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.StreamHandler(),
        logging.FileHandler('dir_copy.log', encoding='utf-8')
    ]
)
logger = logging.getLogger(__name__)


class DirectoryCopier:
    def __init__(self, source_dir: str, target_dir: str,
                 dir_names: Optional[List[str]] = None,
                 dry_run: bool = True, quiet: bool = False,
                 workers: int = 4, overwrite: bool = False):
        """
        Initialize directory copier

        Args:
            source_dir: Source directory path
            target_dir: Target directory path
            dir_names: List of directory names to copy (None means copy all)
            dry_run: Preview only, don't actually copy
            quiet: Quiet mode, don't show detailed list
            workers: Number of parallel workers for copying
            overwrite: Overwrite existing files in target
        """
        self.source_dir = Path(source_dir).resolve()
        self.target_dir = Path(target_dir).resolve()
        self.dir_names = set(dir_names) if dir_names else None
        self.dry_run = dry_run
        self.quiet = quiet
        self.workers = workers
        self.overwrite = overwrite

        # Statistics
        self.stats = {
            'dirs_scanned': 0,
            'dirs_matched': 0,
            'dirs_copied': 0,
            'files_copied': 0,
            'files_skipped': 0,
            'files_failed': 0,
            'bytes_copied': 0
        }
        self.lock = threading.Lock()

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

        # Check if source and target are the same
        if self.source_dir == self.target_dir:
            logger.error("Source and target directories cannot be the same")
            return False

        # Check if target is subdirectory of source
        try:
            self.target_dir.relative_to(self.source_dir)
            logger.error("Target directory cannot be a subdirectory of source")
            return False
        except ValueError:
            pass

        return True

    def scan_directories(self) -> Dict[Path, Path]:
        """
        Scan source directory and collect all directories matching the filter

        Returns:
            Dictionary mapping source directory paths to their relative paths
        """
        logger.info(f"Scanning source directory: {self.source_dir}")
        dirs_to_copy = {}

        try:
            for root, dirs, files in os.walk(self.source_dir):
                root_path = Path(root)

                self.stats['dirs_scanned'] += 1

                # Progress indicator
                if not self.quiet and self.stats['dirs_scanned'] % 10000 == 0:
                    logger.info(f"Scanned {self.stats['dirs_scanned']} directories...")

                # Get relative path from source
                try:
                    rel_path = root_path.relative_to(self.source_dir)
                except ValueError:
                    continue

                # Skip the root directory itself
                if rel_path == Path('.'):
                    continue

                # Get the directory name (basename)
                dir_name = root_path.name

                # Check if this directory matches the filter
                if self.dir_names is None or dir_name in self.dir_names:
                    dirs_to_copy[root_path] = rel_path
                    self.stats['dirs_matched'] += 1

                    if not self.quiet:
                        logger.debug(f"Matched: {rel_path}")

                    # Don't traverse into matched directories
                    # (we'll copy the whole tree later)
                    dirs[:] = []

            filter_info = "all directories" if self.dir_names is None else f"matching names: {self.dir_names}"
            logger.info(f"Scan completed: {self.stats['dirs_scanned']} directories scanned, "
                       f"{self.stats['dirs_matched']} directories matched ({filter_info})")

            if self.dir_names is not None and not dirs_to_copy:
                logger.warning(f"No directories found matching: {self.dir_names}")
                logger.info("Listing some directories in source for reference:")
                count = 0
                for root, dirs, files in os.walk(self.source_dir):
                    if count >= 20:  # Show first 20 directories
                        logger.info("  ... (more directories exist)")
                        break
                    root_path = Path(root)
                    try:
                        rel = root_path.relative_to(self.source_dir)
                        if rel != Path('.'):
                            logger.info(f"  - {rel}")
                            count += 1
                    except ValueError:
                        pass

        except Exception as e:
            logger.error(f"Error during directory scan: {e}")
            raise

        return dirs_to_copy

    def copy_file(self, src_file: Path, dst_file: Path) -> tuple:
        """
        Copy a single file

        Returns:
            Tuple of (status, bytes_copied, error_message)
        """
        try:
            # Check if target exists
            if dst_file.exists() and not self.overwrite:
                return ('skipped', 0, 'already exists')

            # Get file size
            file_size = src_file.stat().st_size

            if self.dry_run:
                return ('copied', file_size, None)

            # Create parent directory if needed
            dst_file.parent.mkdir(parents=True, exist_ok=True)

            # Copy file with metadata
            shutil.copy2(src_file, dst_file)

            return ('copied', file_size, None)

        except PermissionError as e:
            return ('failed', 0, f'permission denied: {e}')
        except OSError as e:
            return ('failed', 0, f'OS error: {e}')
        except Exception as e:
            return ('failed', 0, f'unexpected error: {e}')

    def copy_directory_tree(self, src_dir: Path, rel_path: Path) -> None:
        """
        Copy a directory tree and all its contents

        Args:
            src_dir: Source directory absolute path
            rel_path: Relative path from source root (where to place in target)
        """
        try:
            dst_dir = self.target_dir / rel_path

            if not self.quiet:
                logger.info(f"[PROCESSING] {rel_path}")

            # Create target directory
            if not self.dry_run:
                dst_dir.mkdir(parents=True, exist_ok=True)

            with self.lock:
                self.stats['dirs_copied'] += 1

            # Walk through the entire directory tree
            for root, dirs, files in os.walk(src_dir):
                root_path = Path(root)

                # Calculate the relative path within this directory tree
                try:
                    sub_rel = root_path.relative_to(src_dir)
                except ValueError:
                    continue

                # The full relative path in target
                if sub_rel == Path('.'):
                    current_rel = rel_path
                else:
                    current_rel = rel_path / sub_rel

                # Create subdirectories in target
                if sub_rel != Path('.'):
                    target_subdir = self.target_dir / current_rel
                    if not self.dry_run:
                        target_subdir.mkdir(parents=True, exist_ok=True)

                # Copy all files in current directory
                for filename in files:
                    src_file = root_path / filename
                    dst_file = self.target_dir / current_rel / filename

                    status, bytes_copied, error_msg = self.copy_file(src_file, dst_file)

                    with self.lock:
                        if status == 'copied':
                            self.stats['files_copied'] += 1
                            self.stats['bytes_copied'] += bytes_copied
                        elif status == 'skipped':
                            self.stats['files_skipped'] += 1
                        elif status == 'failed':
                            self.stats['files_failed'] += 1

                    if not self.quiet:
                        if status == 'copied':
                            prefix = "[WILL COPY]" if self.dry_run else "[COPIED]"
                            logger.debug(f"{prefix} {current_rel / filename}")
                        elif status == 'failed':
                            logger.error(f"[FAILED] {current_rel / filename}: {error_msg}")

        except Exception as e:
            logger.error(f"Error copying directory tree {src_dir}: {e}")
            with self.lock:
                self.stats['files_failed'] += 1

    def copy_all_content(self) -> None:
        """
        Copy all content from source to target (when no dir_names specified)
        This is optimized for copying entire directory trees
        """
        logger.info("Copying all content from source to target...")

        try:
            for root, dirs, files in os.walk(self.source_dir):
                root_path = Path(root)
                rel_path = root_path.relative_to(self.source_dir)
                dst_path = self.target_dir / rel_path

                self.stats['dirs_scanned'] += 1

                # Progress indicator
                if not self.quiet and self.stats['dirs_scanned'] % 1000 == 0:
                    logger.info(f"Processed {self.stats['dirs_scanned']} directories, "
                              f"{self.stats['files_copied']} files copied...")

                # Create target directory
                if not self.dry_run:
                    dst_path.mkdir(parents=True, exist_ok=True)

                with self.lock:
                    self.stats['dirs_copied'] += 1

                # Copy all files
                for filename in files:
                    src_file = root_path / filename
                    dst_file = dst_path / filename

                    status, bytes_copied, error_msg = self.copy_file(src_file, dst_file)

                    with self.lock:
                        if status == 'copied':
                            self.stats['files_copied'] += 1
                            self.stats['bytes_copied'] += bytes_copied
                        elif status == 'skipped':
                            self.stats['files_skipped'] += 1
                        elif status == 'failed':
                            self.stats['files_failed'] += 1

                    if not self.quiet and status == 'failed':
                        logger.error(f"[FAILED] {rel_path / filename}: {error_msg}")

        except Exception as e:
            logger.error(f"Error during copy operation: {e}")
            raise

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
        logger.info("Directory copy task started")
        logger.info(f"Source directory: {self.source_dir}")
        logger.info(f"Target directory: {self.target_dir}")
        logger.info(f"Directory filter: {self.dir_names if self.dir_names else 'ALL (copy everything)'}")
        logger.info(f"Workers:          {self.workers}")
        logger.info(f"Mode:             {'PREVIEW (no actual copy)' if self.dry_run else 'ACTUAL COPY'}")
        logger.info(f"Overwrite:        {'YES' if self.overwrite else 'NO'}")
        logger.info(f"Quiet mode:       {'ON' if self.quiet else 'OFF'}")
        logger.info("=" * 60)

        # If no directory filter, copy everything
        if self.dir_names is None:
            self.copy_all_content()
        else:
            # Scan and collect directories to copy
            dirs_to_copy = self.scan_directories()

            if not dirs_to_copy:
                logger.warning("No directories found matching the filter")
                return

            logger.info(f"Found {len(dirs_to_copy)} directories to copy")

            # Copy directories in parallel
            logger.info(f"Starting copy operation with {self.workers} workers...")

            if self.workers > 1:
                with ThreadPoolExecutor(max_workers=self.workers) as executor:
                    futures = {
                        executor.submit(self.copy_directory_tree, src, rel): (src, rel)
                        for src, rel in dirs_to_copy.items()
                    }

                    for i, future in enumerate(as_completed(futures), 1):
                        try:
                            future.result()

                            # Progress indicator
                            if i % 100 == 0 or i == len(dirs_to_copy):
                                logger.info(f"Processed {i}/{len(dirs_to_copy)} directories "
                                          f"(files: {self.stats['files_copied']} copied, "
                                          f"{self.stats['files_skipped']} skipped)")
                        except Exception as e:
                            logger.error(f"Error processing directory: {e}")
            else:
                # Single-threaded processing
                for i, (src, rel) in enumerate(dirs_to_copy.items(), 1):
                    self.copy_directory_tree(src, rel)

                    if i % 100 == 0 or i == len(dirs_to_copy):
                        logger.info(f"Processed {i}/{len(dirs_to_copy)} directories")

        # Print summary
        self.print_summary()

    def print_summary(self) -> None:
        """Print operation summary"""
        logger.info("=" * 60)
        logger.info("Copy task completed")
        logger.info(f"Directories scanned:     {self.stats['dirs_scanned']}")
        if self.dir_names is not None:
            logger.info(f"Directories matched:     {self.stats['dirs_matched']}")
        logger.info(f"Directories copied:      {self.stats['dirs_copied']}")
        logger.info(f"Files copied:            {self.stats['files_copied']}")
        logger.info(f"Files skipped:           {self.stats['files_skipped']}")
        logger.info(f"Files failed:            {self.stats['files_failed']}")

        if self.stats['bytes_copied'] > 0:
            logger.info(f"Total size copied:       {self.format_size(self.stats['bytes_copied'])}")

        if self.dry_run and self.stats['files_copied'] > 0:
            logger.info("\nUse --execute to perform actual copy")
        elif not self.dry_run and self.stats['files_copied'] > 0:
            logger.info("\nCopy operation completed successfully")

        logger.info("=" * 60)


def main():
    parser = argparse.ArgumentParser(
        description='Copy specified directories from source to target',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog='''
Examples:
  # Preview mode - copy all directories and files
  %(prog)s /source /target

  # Preview mode - copy all directories named "images" (at any level)
  %(prog)s /source /target images

  # Copy all "logs" and "data" directories found anywhere in source tree
  %(prog)s /source /target logs data -e

  # High performance mode with 8 workers
  %(prog)s /source /target images docs -e -w 8

  # Quiet mode (show summary only)
  %(prog)s /source /target -e -q

  # Overwrite existing files
  %(prog)s /source /target images -e --overwrite

Directory structure example:
  Source:
    /source/project1/images/photo1.jpg
    /source/project2/images/photo2.jpg
    /source/backup/project1/images/photo3.jpg
    /source/docs/readme.txt

  Command: %(prog)s /source /target images -e

  Result (all "images" directories copied with their relative paths):
    /target/project1/images/photo1.jpg
    /target/project2/images/photo2.jpg
    /target/backup/project1/images/photo3.jpg
        '''
    )

    parser.add_argument('source',
                       help='Source directory path')
    parser.add_argument('target',
                       help='Target directory path')
    parser.add_argument('directories',
                       nargs='*',
                       help='Directory names to copy (searches at all levels, if not specified copy all)')
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
        copier = DirectoryCopier(
            source_dir=args.source,
            target_dir=args.target,
            dir_names=args.directories if args.directories else None,
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