using GroupDocs.Signature.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Signature.Mcp.IntegrationTests;

public class SearchTextSignaturesTests : IClassFixture<McpServerFixture>
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public SearchTextSignaturesTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task SearchTextSignatures_BlankPdf_ReturnsNoneFoundOrEmptyJson()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SearchTextSignatures.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"SearchTextSignatures reported an error: {body}");

        // Tool returns either a "No text signatures found in '<file>'" plain-text
        // line, or a JSON object with a `found` field. Both are valid empty results.
        var indicatesEmpty = body.Contains("No text signatures", StringComparison.OrdinalIgnoreCase)
            || body.Contains("\"found\"", StringComparison.Ordinal);
        Assert.True(indicatesEmpty,
            $"Expected empty-result indicator. Response:\n{body}");
    }

    [Fact]
    public async Task SearchTextSignatures_AcceptsOptionalTextFilter()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SearchTextSignatures.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
                ["text"] = "APPROVED",
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"SearchTextSignatures with text filter reported an error: {body}");
    }

    [Fact]
    public async Task SearchTextSignatures_UnknownFile_ReturnsDescriptiveError()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SearchTextSignatures.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = "missing.pdf" },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        // Pitfall #18: a descriptive prefix or an MCP error — both acceptable;
        // what we forbid is the response being empty.
        Assert.False(string.IsNullOrWhiteSpace(body),
            "Expected a non-empty error or hint response for missing file.");
    }
}
