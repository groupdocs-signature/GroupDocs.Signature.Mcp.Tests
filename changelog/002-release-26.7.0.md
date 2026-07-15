---
id: 002
date: 2026-07-15
package-under-test: 26.7.0
type: maintenance
---

# Target GroupDocs.Signature.Mcp 26.7.0

## What changed

- Bumped the package-under-test to `GroupDocs.Signature.Mcp@26.7.0` everywhere:
  `Directory.Build.props` (`<McpPackageVersion>` default), the
  `.github/workflows/integration.yml` `package_version` input default and the
  computed env default, and every `@26.7.0` / `:26.7.0` doc pin across
  `how-to/*`, `examples/*`, `README.md`, and `AGENTS.md`.
- Added a Cursor how-to guide (`how-to/07-use-with-cursor.md`) plus a
  ready-to-paste `examples/cursor-mcp.json`. Cursor uses the `mcpServers` key
  (like Claude Desktop); the guide documents the Windows `dnx` SSL/timeout
  workaround (full `dotnet.exe` path + cached DLL).
- Corrected the stale tool list and license note in `how-to/README.md` — the
  server exposes eight tools (`sign`, `verify`, five `search_*`,
  `get_document_info`), and it is `sign` (not `verify`) that calls `Save()` and
  benefits from a license.

## Why

Keep the integration suite pinned to the current released MCP package and add
the standard Cursor integration guide/config that ships with every product's
Tests repo.

## Migration / impact

- The tool surface is unchanged (still eight tools); `ToolDiscoveryTests` already
  asserts a count of 8 and name-checks each tool — no test changes required.
- Integration tests exercise the published `26.7.0` package via `dnx`; they run
  in CI post-publish. Local build is compile-only.
