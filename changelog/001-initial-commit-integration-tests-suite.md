---
id: 001
date: 2026-05-28
package-under-test: 26.5.0
type: feature
---

# Initial integration test suite for GroupDocs.Signature.Mcp

## What changed

- xUnit test project targeting `net10.0`, referencing only the published
  `ModelContextProtocol` 1.1.0 NuGet — no project reference to the server source.
- `McpServerFixture` launches the published `GroupDocs.Signature.Mcp@26.5.0`
  package via `dnx` as a child process, wires an MCP stdio client, and seeds a
  temporary storage folder with synthetic + real sample documents.
- `SampleDocuments` builds a minimal valid blank PDF and a baseline 1×1 JPEG
  from byte arrays at runtime, and copies real samples from `Files/` (committed
  PDF / DOCX / XLSX / JPG / PNG from the upstream
  GroupDocs.Signature-for-.NET examples repo — see [Files/README.md](../Files/README.md)
  for provenance).
- Nine test classes covering all 8 tools advertised by the server:
  - `ToolDiscoveryTests` (3) — server info, exactly-8-tools, schema validation.
  - `SignTests` (5) — text / qrcode / barcode theories on the blank PDF;
    unknown-type error path (Pitfall #18 prefix); real-sample theory across
    PDF / DOCX / XLSX.
  - `VerifyTests` (7) — JSON shape; per-type parameter theory (text / qrcode /
    barcode / digital / all); unknown-type error path.
  - `SearchTextSignaturesTests` (3) — empty-result shape, text-filter
    acceptance, unknown-file response.
  - `SearchBarcodesTests` (3) — same shape.
  - `SearchQrCodesTests` (3) — same shape.
  - `SearchDigitalSignaturesTests` (3) — empty-result, unknown-file, password.
  - `SearchImageSignaturesTests` (3) — empty-result, unknown-file, password.
  - `GetDocumentInfoTests` (2 + theory) — JSON shape on PDF / JPEG; per-format
    theory across the 5 real samples.
  - `ErrorHandlingTests` (3) — unknown file, corrupted bytes, password parameter.
- GitHub Actions workflow `.github/workflows/integration.yml`:
  - Matrix: `ubuntu-latest`, `windows-latest`, `macos-latest`.
  - Linux step installs `libgdiplus` + `libfontconfig1` + `ttf-mscorefonts-installer`
    (with debconf EULA accept + `fc-cache`) because the engine rasterises
    signature glyphs onto pages and needs real fonts.
  - macOS step `brew install mono-libgdiplus` and copies `libgdiplus.dylib`
    into the .NET shared-framework directory so dnx's child process can
    `dlopen` it.
  - Triggers: push, PR, nightly cron, `workflow_dispatch` (with `package_version`
    input), `repository_dispatch` (`nuget-published` event for release smoke).
  - Optional `GROUPDOCS_LICENSE` repo secret auto-decoded into `$RUNNER_TEMP`
    and exported as `GROUPDOCS_LICENSE_PATH` to drop the `sign` watermark.
- `examples/` — ready-to-use `claude-desktop.json`, `vscode-mcp.json`,
  `docker-compose.yml` copy-paste configs.
- `AGENTS.md` + `llms.txt` for AI coding agent orientation.
- `how-to/` guides covering every deployment channel (NuGet via dnx / dotnet
  tool, Docker, MCP registry, Claude Desktop, VS Code / GitHub Copilot, plus
  running this test suite).

## Why

Closes the release-validation gap: the main repo's unit tests mock
`IFileResolver` / `ILicenseManager` and validate tool logic, but nothing
previously exercised the **shipped** NuGet end-to-end. Every release now has
a cross-platform smoke check against live nuget.org before users hit it.

## Migration / impact

First release of this repository — no migration. To wire the release-smoke
trigger, add a `gh api repos/.../dispatches -f event_type=nuget-published -f
'client_payload[package_version]=…'` step to the main repo's publish workflow
after `dotnet nuget push` succeeds. See `how-to/06-run-integration-tests.md`.
