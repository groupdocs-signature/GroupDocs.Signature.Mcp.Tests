using ModelContextProtocol.Client;

namespace GroupDocs.Signature.Mcp.IntegrationTests.Fixtures;

/// Resolves tool names by keyword. The server-side attribute [McpServerTool] uses
/// the method name verbatim today (PascalCase: Sign, Verify, SearchTextSignatures,
/// SearchBarcodes, SearchQrCodes, SearchDigitalSignatures, SearchImageSignatures,
/// GetDocumentInfo), but keyword-based resolution keeps tests robust against
/// future renames / casing convention changes.
internal sealed class ToolCatalog
{
    private readonly IReadOnlyList<McpClientTool> _tools;

    private ToolCatalog(IReadOnlyList<McpClientTool> tools) => _tools = tools;

    public static async Task<ToolCatalog> LoadAsync(McpClient client, CancellationToken ct = default)
    {
        var tools = await client.ListToolsAsync(cancellationToken: ct);
        return new ToolCatalog(tools.ToList());
    }

    public IReadOnlyList<McpClientTool> All => _tools;

    public McpClientTool Sign                    => Resolve("sign", excluding: new[] { "search", "digital", "image" });
    public McpClientTool Verify                  => Resolve("verify");
    public McpClientTool SearchTextSignatures    => Resolve("text", required: "search");
    public McpClientTool SearchBarcodes          => Resolve("barcode", required: "search");
    public McpClientTool SearchQrCodes           => Resolve("qr", required: "search");
    public McpClientTool SearchDigitalSignatures => Resolve("digital", required: "search");
    public McpClientTool SearchImageSignatures   => Resolve("image", required: "search");
    public McpClientTool GetDocumentInfo         => Resolve("document");

    private McpClientTool Resolve(string keyword, string? required = null, string[]? excluding = null)
    {
        var ex = excluding ?? Array.Empty<string>();
        bool Matches(McpClientTool t)
        {
            var name = t.Name;
            if (!name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                return false;
            if (required is not null && !name.Contains(required, StringComparison.OrdinalIgnoreCase))
                return false;
            foreach (var x in ex)
                if (name.Contains(x, StringComparison.OrdinalIgnoreCase))
                    return false;
            return true;
        }

        return _tools.FirstOrDefault(Matches)
            ?? throw new InvalidOperationException(
                $"No tool with name containing '{keyword}'{(required is not null ? $" and '{required}'" : "")}{(ex.Length > 0 ? $" but excluding [{string.Join(',', ex)}]" : "")}. " +
                $"Found: {string.Join(", ", _tools.Select(t => t.Name))}");
    }
}
