# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-10-15

First stable release.

### Added

- `format` and `check` commands, each taking an optional file or directory path.
- Reporting of KQL syntax errors: the file path and diagnostics are written to
  stderr, the run exits with code `2`, and the file is left unchanged.
- `--version` output, derived from the assembly version.

### Changed

- **Breaking:** exit codes now follow the `0` / `1` / `2` convention — `0`
  success, `1` files need formatting, `2` runtime/IO/usage error. Previously
  errors also exited with `1`.
- **Breaking:** the CLI is command-based:
  - `kqlier format [path]` formats a file or directory; reads stdin when no path
    is given.
  - `kqlier check [path]` checks a file or directory; checks stdin when no path
    is given.
- `.kql` discovery is now case-insensitive on every platform.
- Hidden files and directories are skipped during recursive discovery.

### Removed

- **Breaking:** the `--file`, `--check`, and `--stdin` options, replaced by the
  `format` / `check` commands.

### Fixed

- Writes are atomic and preserve permissions: the temporary file is created next
  to the target and renamed over it in place, `FileAttributes` (and, on Unix, the
  file mode) are preserved, and temporary files are cleaned up on failure. This
  fixes the previous loss of Unix permissions (e.g. `0644` becoming `0600`) and
  the non-atomic cross-volume replace.
- Standard input and output are always UTF-8 without a BOM, avoiding Windows
  console code-page corruption.
- Single-file failures are reported the same way as the all-files run.
- Unreadable directories are skipped instead of aborting the whole run.

### Internal

- Build treats warnings as errors.
- CI runs on Ubuntu, Windows, and macOS, checks formatting with CSharpier, and
  runs an install smoke test (`dotnet pack` + `dotnet tool install --local` +
  `kqlier --version`).
- Releases publish to NuGet on `v*` tags using trusted publishing (OIDC).

[1.0.0]: https://github.com/veikkovaljus/Kqlier/releases/tag/v1.0.0
