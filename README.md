# Kqlier

A CLI tool for formatting KQL (Kusto Query Language) files.

## Installation

```bash
dotnet tool install -g kqlier
```

## Usage

```bash
# Format all .kql files recursively in the current directory
kqlier

# Format a single file
kqlier --file query.kql

# Check if files are formatted
kqlier --check

# Check a single file
kqlier --check --file query.kql
```

## Exit Codes

- `0` - Success (all files formatted or already formatted)
- `1` - Check mode: one or more files need formatting

## License

MIT
