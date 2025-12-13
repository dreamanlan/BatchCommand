#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
List Empty Directories Script - Find and list all empty subdirectories
"""

import os
import sys
import argparse
from pathlib import Path


def find_empty_directories(root_dir: Path) -> list:
    """
    Find all empty directories in the given root directory

    Args:
        root_dir: Root directory to search

    Returns:
        List of relative paths to empty directories
    """
    empty_dirs = []

    try:
        # Walk from bottom to top to correctly identify empty directories
        for root, dirs, files in os.walk(root_dir, topdown=False):
            root_path = Path(root)

            # Skip the root directory itself
            if root_path == root_dir:
                continue

            # Check if directory is empty (no files and no subdirectories)
            try:
                if not any(root_path.iterdir()):
                    rel_path = root_path.relative_to(root_dir)
                    empty_dirs.append(str(rel_path))
            except (PermissionError, OSError):
                # Skip directories we can't access
                pass

    except Exception:
        # Silently handle errors during walk
        pass

    return sorted(empty_dirs)


def main():
    parser = argparse.ArgumentParser(
        description='List all empty subdirectories in the specified directory',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog='''
Examples:
  # List all empty directories
  %(prog)s /path/to/directory

  # Save to file
  %(prog)s /path/to/directory > empty_dirs.txt

  # Count empty directories
  %(prog)s /path/to/directory | wc -l
        '''
    )

    parser.add_argument('directory',
                       help='Directory to search for empty subdirectories')

    # Parse arguments
    args = parser.parse_args()

    # Validate directory
    target_dir = Path(args.directory)

    if not target_dir.exists():
        print(f"Error: Directory does not exist: {args.directory}", file=sys.stderr)
        return 1

    if not target_dir.is_dir():
        print(f"Error: Not a directory: {args.directory}", file=sys.stderr)
        return 1

    # Find and print empty directories
    empty_dirs = find_empty_directories(target_dir.resolve())

    for dir_path in empty_dirs:
        print(dir_path)

    return 0


if __name__ == '__main__':
    sys.exit(main())