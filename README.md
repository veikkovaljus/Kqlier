# Kqlier

A CLI tool for formatting KQL (Kusto Query Language) files.

## Installation

```bash
dotnet tool install -g kqlier
```

## Usage

```bash
# Format all .kql files recursively in the current directory
kqlier format .

# Format a single file
kqlier format query.kql

# Format all .kql files in a directory
kqlier format ./queries

# Check if files are formatted (exit 1 if any need formatting)
kqlier check .

# Check a single file
kqlier check query.kql

# Format from stdin, output to stdout
echo "StormEvents|where x==1" | kqlier format

# Check stdin content
cat query.kql | kqlier check

# Show version
kqlier --version
```

## Output

Format mode shows how many files were modified:
```
Formatted 3 of 5 files
```

Check mode lists files that need formatting:
```
Checked 5 files, 2 need formatting
Files not formatted:
  /path/to/query1.kql
  /path/to/query2.kql
```

## Exit Codes

- `0` - Success (all files formatted or already formatted)
- `1` - Check mode: one or more files need formatting
- `2` - Runtime, I/O, or usage error, or invalid KQL (e.g. unreadable file, invalid options, syntax errors)

## License

MIT
