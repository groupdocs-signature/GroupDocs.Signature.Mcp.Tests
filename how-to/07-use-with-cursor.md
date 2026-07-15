# Use with Cursor

Connect the MCP server to [Cursor](https://cursor.com) so you can ask its Agent
to sign, verify, and search for signatures in your documents.

## Prerequisites

- Cursor installed and updated (MCP support is in **Settings → Tools & MCP**).
- One of:
  - [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (for the `dnx` route — recommended), or
  - [Docker](https://www.docker.com/products/docker-desktop) (for the container route).

## Config file location

Cursor uses the **`mcpServers`** key (like Claude Desktop) — **not** `servers`
as in VS Code. Two scopes:

| Scope | Path |
|---|---|
| Global (all projects) | `~/.cursor/mcp.json` (macOS/Linux) · `%USERPROFILE%\.cursor\mcp.json` (Windows) |
| Project-only | `.cursor/mcp.json` in the workspace root |

Create the file if it doesn't exist.

## Option A — dnx (recommended)

```json
{
  "mcpServers": {
    "groupdocs-signature": {
      "command": "dnx",
      "args": ["GroupDocs.Signature.Mcp@26.7.0", "--yes"],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "/Users/you/Documents"
      }
    }
  }
}
```

- Replace the storage path with an **absolute path** to the folder Cursor should
  operate on. On Windows use `"C:\\Users\\you\\Documents"` (double-escaped) or
  forward slashes.
- Omit `@26.7.0` to always pull the latest stable.
- Add `"GROUPDOCS_LICENSE_PATH": "…/GroupDocs.Total.lic"` to `env` to run
  `sign` without the evaluation watermark (signing calls `Save()`, so in
  evaluation mode the output may carry a watermark). The read-only tools
  (`verify`, the `search_*` tools, `get_document_info`) work in evaluation mode.

Copy-paste starter: [examples/cursor-mcp.json](../examples/cursor-mcp.json).

## Option B — Windows: full path to `dotnet.exe` (SSL / timeout workaround)

On Windows, Cursor launching `dnx` can fail with an **SSL / ~30 s timeout** on
the first package probe. Bypass `dnx` by running the already-cached tool DLL
directly with `dotnet.exe`:

```json
{
  "mcpServers": {
    "groupdocs-signature": {
      "command": "C:\\Program Files\\dotnet\\dotnet.exe",
      "args": [
        "C:\\Users\\you\\.nuget\\packages\\groupdocs.signature.mcp\\26.7.0\\tools\\net10.0\\any\\GroupDocs.Signature.Mcp.dll"
      ],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "C:\\Users\\you\\Documents"
      }
    }
  }
}
```

Populate the cache first by running `dnx GroupDocs.Signature.Mcp@26.7.0 --yes` once
in a terminal, then point `args[0]` at the resulting
`…\.nuget\packages\groupdocs.signature.mcp\<version>\tools\net10.0\any\GroupDocs.Signature.Mcp.dll`.

## Option C — Docker

```json
{
  "mcpServers": {
    "groupdocs-signature": {
      "command": "docker",
      "args": [
        "run", "--rm", "-i",
        "-v", "/Users/you/Documents:/data",
        "ghcr.io/groupdocs-signature/signature-net-mcp:26.7.0"
      ]
    }
  }
}
```

## Reload and verify

1. Save `mcp.json`.
2. **Settings → Tools & MCP** → find `groupdocs-signature` → toggle it on (or hit
   the reload icon). A green dot means it connected.
3. Expand it — you should see `sign`, `verify`, `search_text_signatures`,
   `search_barcodes`, `search_qr_codes`, `search_digital_signatures`,
   `search_image_signatures`, and `get_document_info`.

## Example prompts (Agent mode)

```
Sign contract.pdf with a QR code containing "Reviewed by Alice".

Does agreement.docx have any valid digital signatures?

Search invoice.pdf for barcodes and show me the decoded text.

How many pages does report.pdf have?
```

The Agent will call `sign` / `verify` / the `search_*` tools / `get_document_info`
and compose its answer from the results.

## Troubleshooting

| Symptom | Fix |
|---|---|
| Server greyed out / won't start on Windows | `dnx` SSL/timeout — use **Option B** (full `dotnet.exe` path + cached DLL). |
| Server not listed | JSON typo — Cursor silently drops unparseable entries. Validate with `jq . mcp.json`. Confirm the key is `mcpServers`, not `servers`. |
| Signed output has a watermark | Expected in evaluation mode. Add `GROUPDOCS_LICENSE_PATH` to sign without it. `verify` / `search_*` / `get_document_info` are unaffected. |
| `DllNotFoundException: libgdiplus` (macOS/Linux) | Install native deps — `brew install mono-libgdiplus` (macOS) / `apt-get install libgdiplus libfontconfig1` (Linux), or use the Docker option. |

## Next steps

- [04 — Use with Claude Desktop](04-use-with-claude-desktop.md)
- [05 — Use with VS Code / Copilot](05-use-with-vscode-copilot.md)
