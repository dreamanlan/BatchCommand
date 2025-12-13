import os
import sys
from pathlib import Path
from typing import Optional, List
import fnmatch

def delete_file_direct(file_path: str) -> bool:
    """
    Delete a file directly without checking existence

    Args:
        file_path: Path to the file to delete

    Returns:
        True if successful, False otherwise
    """
    try:
        Path(file_path).unlink(missing_ok=True)  # Python 3.8+
        return True
    except Exception as e:
        return False

def write_files_to_txt(output_file: str, directory: str, *extensions: str) -> int:
    """
    Write relative paths of files to a text file (append mode)

    Args:
        output_file: Output text file path
        directory: Directory path to scan
        *extensions: Optional extension parameters, supports formats:
                    - 'txt' (extension name)
                    - '.txt' (.extension)
                    - '*.txt' (with wildcard)
                    - '*.py*' (wildcard matching)

    Returns:
        Number of files written

    Examples:
        write_files_to_txt('output.txt', '.')  # Write all files
        write_files_to_txt('output.txt', '.', 'txt', 'py')  # Write txt and py files
        write_files_to_txt('output.txt', '.', '.txt', '.py')  # Same as above
        write_files_to_txt('output.txt', '.', '*.txt', '*.py')  # Same as above
        write_files_to_txt('output.txt', '.', '*.py*')  # Match .py, .pyx, .pyc, etc.
    """

    # Normalize directory path
    base_path = Path(directory)

    if not base_path.exists():
        return 0

    if not base_path.is_dir():
        return 0

    # Normalize extensions
    normalized_patterns = []
    if extensions:
        for ext in extensions:
            ext = ext.strip()
            if not ext:
                continue

            # If no wildcard, standardize it
            if '*' not in ext and '?' not in ext:
                # Ensure it starts with a dot
                if not ext.startswith('.'):
                    ext = '.' + ext
                # Convert to wildcard pattern
                ext = '*' + ext
            # If already wildcard form but doesn't start with *
            elif not ext.startswith('*'):
                ext = '*' + ext

            normalized_patterns.append(ext.lower())

    # Traverse directory and write to file
    file_count = 0
    try:
        with open(output_file, 'a', encoding='utf-8') as f:
            for root, dirs, files in os.walk(directory):
                for filename in files:
                    # Get full path
                    full_path = Path(root) / filename

                    # Calculate relative path
                    try:
                        relative_path = full_path.relative_to(base_path)
                    except ValueError:
                        relative_path = full_path

                    # If no extensions specified, write all files
                    if not normalized_patterns:
                        f.write(str(relative_path) + '\n')
                        file_count += 1
                    else:
                        # Check if file matches any extension pattern
                        filename_lower = filename.lower()
                        for pattern in normalized_patterns:
                            if fnmatch.fnmatch(filename_lower, pattern):
                                f.write(str(relative_path) + '\n')
                                file_count += 1
                                break
    except Exception as e:
        return file_count

    return file_count

def print_files(directory: str, *extensions: str) -> int:
    """
    Print relative paths of files in a directory

    Args:
        directory: Directory path
        *extensions: Optional extension parameters, supports formats:
                    - 'txt' (extension name)
                    - '.txt' (.extension)
                    - '*.txt' (with wildcard)
                    - '*.py*' (wildcard matching)

    Examples:
        print_files('.')  # Print all files
        print_files('.', 'txt', 'py')  # Print txt and py files
        print_files('.', '.txt', '.py')  # Same as above
        print_files('.', '*.txt', '*.py')  # Same as above
        print_files('.', '*.py*')  # Match .py, .pyx, .pyc, etc.
    """

    # Normalize directory path
    base_path = Path(directory)

    if not base_path.exists():
        return 0

    if not base_path.is_dir():
        return 0

    # Normalize extensions
    normalized_patterns = []
    if extensions:
        for ext in extensions:
            ext = ext.strip()
            if not ext:
                continue

            # If no wildcard, standardize it
            if '*' not in ext and '?' not in ext:
                # Ensure it starts with a dot
                if not ext.startswith('.'):
                    ext = '.' + ext
                # Convert to wildcard pattern
                ext = '*' + ext
            # If already wildcard form but doesn't start with *
            elif not ext.startswith('*'):
                ext = '*' + ext

            normalized_patterns.append(ext.lower())

    # Traverse directory
    file_count = 0
    for root, dirs, files in os.walk(directory):
        for filename in files:
            # Get full path
            full_path = Path(root) / filename

            # Calculate relative path
            try:
                relative_path = full_path.relative_to(base_path)
            except ValueError:
                relative_path = full_path

            # If no extensions specified, print all files
            if not normalized_patterns:
                print(relative_path)
                file_count += 1
            else:
                # Check if file matches any extension pattern
                filename_lower = filename.lower()
                for pattern in normalized_patterns:
                    if fnmatch.fnmatch(filename_lower, pattern):
                        print(relative_path)
                        file_count += 1
                        break

    return file_count

def get_files(directory: str, *extensions: str) -> List[Path]:
    """
    Get list of relative file paths in a directory

    Args:
        directory: Directory path
        *extensions: Optional extension parameters, supports formats:
                    - 'txt' (extension name)
                    - '.txt' (.extension)
                    - '*.txt' (with wildcard)
                    - '*.py*' (wildcard matching)

    Returns:
        List of relative file paths

    Examples:
        files = get_files('.')  # Get all files
        files = get_files('.', 'txt', 'py')  # Get txt and py files
        files = get_files('.', '.txt', '.py')  # Same as above
        files = get_files('.', '*.txt', '*.py')  # Same as above
        files = get_files('.', '*.py*')  # Match .py, .pyx, .pyc, etc.
    """
    base_path = Path(directory)

    if not base_path.exists() or not base_path.is_dir():
        return []

    # Normalize extensions
    normalized_patterns = []
    if extensions:
        for ext in extensions:
            ext = ext.strip()
            if not ext:
                continue
            if '*' not in ext and '?' not in ext:
                if not ext.startswith('.'):
                    ext = '.' + ext
                ext = '*' + ext
            elif not ext.startswith('*'):
                ext = '*' + ext
            normalized_patterns.append(ext.lower())

    result = []
    for root, dirs, files in os.walk(directory):
        for filename in files:
            full_path = Path(root) / filename

            try:
                relative_path = full_path.relative_to(base_path)
            except ValueError:
                continue

            if not normalized_patterns:
                result.append(relative_path)
            else:
                filename_lower = filename.lower()
                for pattern in normalized_patterns:
                    if fnmatch.fnmatch(filename_lower, pattern):
                        result.append(relative_path)
                        break

    return result

if __name__ == "__main__":
    # Check if the path is provided via command line arguments
    if len(sys.argv) > 1:
        target_dir = sys.argv[1]
    else:
        # Otherwise, prompt the user to input the path
        target_dir = input("Please enter the root directory path: ")

    print_files(target_dir)