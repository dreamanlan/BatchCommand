#!/usr/bin/env python3
"""
Compare two directories and delete files/directories in dir2 that don't exist in dir1
"""

import os
import shutil
import argparse
from pathlib import Path


def compare_and_clean(dir1, dir2, dry_run=True, verbose=True):
    """
    Compare two directories and delete items in dir2 that don't exist in dir1

    Args:
        dir1: Reference directory (baseline to keep)
        dir2: Target directory to clean
        dry_run: Preview only, don't actually delete
        verbose: Show detailed information
    """
    dir1_path = Path(dir1).resolve()
    dir2_path = Path(dir2).resolve()

    if not dir1_path.exists():
        print(f"Error: Directory 1 does not exist: {dir1_path}")
        return

    if not dir2_path.exists():
        print(f"Error: Directory 2 does not exist: {dir2_path}")
        return

    print(f"\n{'=' * 60}")
    print(f"Directory 1 (reference): {dir1_path}")
    print(f"Directory 2 (to clean):  {dir2_path}")
    print(f"{'=' * 60}\n")

    if dry_run:
        print("*** DRY RUN MODE - Preview only ***\n")

    deleted_files = 0
    deleted_dirs = 0
    failed_count = 0

    # Walk through dir2 in pre-order (topdown=True allows modifying dirs list)
    for root, dirs, files in os.walk(dir2_path, topdown=True):
        root_path = Path(root)
        rel_root = root_path.relative_to(dir2_path)

        # Check and delete files
        for f in files:
            rel_path = rel_root / f
            corresponding_path = dir1_path / rel_path

            if not corresponding_path.exists():
                full_path = dir2_path / rel_path

                if dry_run:
                    if verbose:
                        print(f"[FILE] {rel_path}")
                    deleted_files += 1
                else:
                    try:
                        full_path.unlink()
                        if verbose:
                            print(f"Deleted file: {rel_path}")
                        deleted_files += 1
                    except Exception as e:
                        print(f"Failed to delete file {rel_path}: {e}")
                        failed_count += 1

        # Check directories and modify dirs list in-place to skip deleted ones
        # Use list() to create a copy for iteration to avoid modification during iteration
        for d in list(dirs):
            rel_path = rel_root / d
            corresponding_path = dir1_path / rel_path

            if not corresponding_path.exists():
                full_path = dir2_path / rel_path

                if dry_run:
                    if verbose:
                        print(f"[DIR]  {rel_path}")
                    deleted_dirs += 1
                    # Remove from dirs to prevent descending (even in dry-run for accurate preview)
                    dirs.remove(d)
                else:
                    try:
                        shutil.rmtree(full_path)
                        if verbose:
                            print(f"Deleted directory: {rel_path}")
                        deleted_dirs += 1
                        # Remove from dirs to prevent descending into deleted directory
                        dirs.remove(d)
                    except Exception as e:
                        print(f"Failed to delete directory {rel_path}: {e}")
                        failed_count += 1
                        # Don't remove from dirs if deletion failed, continue checking subdirs

    # Summary
    total = deleted_files + deleted_dirs

    print(f"\n{'=' * 60}")
    if total == 0:
        print("No files or directories to delete")
    else:
        print(f"Files:       {deleted_files}")
        print(f"Directories: {deleted_dirs}")
        print(f"Total:       {total}")
        if not dry_run and failed_count > 0:
            print(f"Failed:      {failed_count}")

    if dry_run and total > 0:
        print("\nUse --execute to perform actual deletion")
    elif not dry_run and total > 0:
        print("\nDeletion completed")
    print(f"{'=' * 60}")


def main():
    parser = argparse.ArgumentParser(
        description='Compare two directories and delete files/directories in dir2 that don\'t exist in dir1',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog='''
Examples:
  # Preview mode (no actual deletion)
  %(prog)s /path/to/dir1 /path/to/dir2

  # Execute actual deletion
  %(prog)s /path/to/dir1 /path/to/dir2 --execute

  # Quiet mode (show statistics only)
  %(prog)s /path/to/dir1 /path/to/dir2 --execute --quiet
        '''
    )

    parser.add_argument('dir1', help='Reference directory (baseline to keep)')
    parser.add_argument('dir2', help='Target directory to clean')
    parser.add_argument('--execute', '-e', action='store_true',
                        help='Execute actual deletion (default is preview only)')
    parser.add_argument('--quiet', '-q', action='store_true',
                        help='Quiet mode, don\'t show detailed list')

    args = parser.parse_args()

    compare_and_clean(
        args.dir1,
        args.dir2,
        dry_run=not args.execute,
        verbose=not args.quiet
    )


if __name__ == '__main__':
    main()