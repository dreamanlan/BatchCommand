#!/usr/bin/env python3
"""
Extract specified columns from a CSV file
Lines starting with ; are treated as comments and filtered out
"""

import csv
import argparse
import sys
from pathlib import Path


class CSVExtractor:
    def __init__(self, input_file, output_file, columns, delimiter=',',
                 encoding='utf-8'):
        """
        Initialize CSV extractor

        Args:
            input_file: Input CSV file path
            output_file: Output CSV file path (None for stdout)
            columns: List of column identifiers (index or name)
            delimiter: CSV delimiter (default: comma)
            encoding: File encoding (default: utf-8)
        """
        self.input_file = Path(input_file)
        self.output_file = output_file
        self.columns = columns
        self.delimiter = delimiter
        self.encoding = encoding

        self.header = None
        self.column_indices = []
        self.single_column = len(columns) == 1

    def validate(self):
        """Validate input file"""
        if not self.input_file.exists():
            print(f"Error: Input file does not exist: {self.input_file}",
                  file=sys.stderr)
            return False
        return True

    def filter_comments(self, file_handle):
        """
        Filter out comment lines (starting with ;)

        Args:
            file_handle: File handle

        Yields:
            Non-comment lines
        """
        for line in file_handle:
            stripped = line.strip()
            if stripped and not stripped.startswith(';'):
                yield line

    def parse_column_identifier(self, col_id, header):
        """
        Parse column identifier to index
        Supports both quoted and unquoted column names

        Args:
            col_id: Column identifier (int index or string name)
            header: Header row (already parsed by csv.reader, quotes removed)

        Returns:
            Column index or None if not found
        """
        # Try to parse as integer index
        try:
            idx = int(col_id)
            if 0 <= idx < len(header):
                return idx
            else:
                print(f"Error: Column index {idx} out of range (0-{len(header)-1})",
                      file=sys.stderr)
                return None
        except ValueError:
            # Not an integer, treat as column name
            # The header is already parsed by csv.reader, so quotes are removed
            # We need to match against the parsed column names
            try:
                return header.index(col_id)
            except ValueError:
                print(f"Error: Column name '{col_id}' not found in header",
                      file=sys.stderr)
                print(f"Available columns: {', '.join(repr(h) for h in header)}",
                      file=sys.stderr)
                return None

    def resolve_columns(self):
        """
        Resolve column identifiers to indices

        Returns:
            True if successful, False otherwise
        """
        self.column_indices = []

        for col_id in self.columns:
            idx = self.parse_column_identifier(col_id, self.header)
            if idx is None:
                return False
            self.column_indices.append(idx)

        return True

    def extract_row_values(self, row):
        """
        Extract specified column values from row

        Args:
            row: CSV row

        Returns:
            List of extracted values
        """
        values = []
        for idx in self.column_indices:
            if idx < len(row):
                values.append(row[idx])
            else:
                values.append('')  # Empty value for missing columns
        return values

    def write_single_column(self, output_handle, reader):
        """
        Write single column output (no header, no quotes)

        Args:
            output_handle: Output file handle
            reader: CSV reader object
        """
        for row in reader:
            if row:  # Skip empty rows
                values = self.extract_row_values(row)
                if values:
                    # Write value without quotes
                    output_handle.write(values[0] + '\n')

    def write_multiple_columns(self, output_handle, reader):
        """
        Write multiple columns output (with header, standard CSV format)

        Args:
            output_handle: Output file handle
            reader: CSV reader object
        """
        writer = csv.writer(output_handle, delimiter=self.delimiter)

        # Write header
        header_values = [self.header[idx] for idx in self.column_indices]
        writer.writerow(header_values)

        # Write data rows
        for row in reader:
            if row:  # Skip empty rows
                values = self.extract_row_values(row)
                writer.writerow(values)

    def process(self):
        """Process CSV file and extract columns"""
        if not self.validate():
            return False

        try:
            # Open input file and filter comments
            with open(self.input_file, 'r', encoding=self.encoding, newline='') as infile:
                # Filter comment lines before passing to CSV reader
                filtered_lines = self.filter_comments(infile)
                reader = csv.reader(filtered_lines, delimiter=self.delimiter)

                # Read header (first non-comment line)
                try:
                    self.header = next(reader)
                except StopIteration:
                    print("Error: No header found in CSV file", file=sys.stderr)
                    return False

                if not self.header:
                    print("Error: Empty header in CSV file", file=sys.stderr)
                    return False

                # Resolve column identifiers
                if not self.resolve_columns():
                    return False

                # Open output file or use stdout
                if self.output_file:
                    output_handle = open(self.output_file, 'w', encoding=self.encoding,
                                        newline='')
                else:
                    output_handle = sys.stdout

                try:
                    # Write output based on column count
                    if self.single_column:
                        self.write_single_column(output_handle, reader)
                    else:
                        self.write_multiple_columns(output_handle, reader)

                    # Print summary to stderr if output is stdout
                    if not self.output_file:
                        print(f"\n# Extracted {len(self.column_indices)} column(s)",
                              file=sys.stderr)
                    else:
                        print(f"Successfully extracted {len(self.column_indices)} column(s) "
                              f"to {self.output_file}")

                    return True

                finally:
                    if self.output_file:
                        output_handle.close()

        except Exception as e:
            print(f"Error processing CSV file: {e}", file=sys.stderr)
            import traceback
            traceback.print_exc(file=sys.stderr)
            return False


def main():
    parser = argparse.ArgumentParser(
        description='Extract specified columns from CSV file (lines starting with ; are comments)',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog='''
Column specification:
  - Use integer index (0-based): 0, 1, 2, ...
  - Use column name: Name, Email, Age, "Full Name", ...
  - Mix both: 0 Name 2 Email

Column names with spaces or special characters:
  - Can be specified with or without quotes
  - CSV reader automatically handles quoted column names
  - Example: "Full Name" or Full Name (use quotes in shell if needed)

Single column mode:
  - No header in output
  - Values without quotes
  - One value per line

Multiple columns mode:
  - Header included
  - Standard CSV format
  - Quoted values as needed

Examples:
  # Extract columns 0 and 2 by index
  %(prog)s input.csv -c 0 2 -o output.csv

  # Extract columns by name (simple names)
  %(prog)s input.csv -c Name Email Age -o output.csv

  # Extract columns with spaces in names (use shell quotes)
  %(prog)s input.csv -c "Full Name" "Email Address" -o output.csv

  # Mix index and name
  %(prog)s input.csv -c 0 Name 2 -o output.csv

  # Single column (no header, no quotes)
  %(prog)s input.csv -c Email -o emails.txt

  # Output to stdout
  %(prog)s input.csv -c 0 1

  # Use semicolon as delimiter
  %(prog)s input.csv -c 0 1 -d ";" -o output.csv

  # Specify encoding
  %(prog)s input.csv -c Name -o output.csv -e gbk
        '''
    )

    parser.add_argument('input', help='Input CSV file')
    parser.add_argument('-c', '--columns', nargs='+', required=True,
                        help='Column identifiers (index or name)')
    parser.add_argument('-o', '--output', default=None,
                        help='Output file (default: stdout)')
    parser.add_argument('-d', '--delimiter', default=',',
                        help='CSV delimiter (default: comma)')
    parser.add_argument('-e', '--encoding', default='utf-8',
                        help='File encoding (default: utf-8)')

    args = parser.parse_args()

    extractor = CSVExtractor(
        input_file=args.input,
        output_file=args.output,
        columns=args.columns,
        delimiter=args.delimiter,
        encoding=args.encoding
    )

    success = extractor.process()
    sys.exit(0 if success else 1)


if __name__ == '__main__':
    main()