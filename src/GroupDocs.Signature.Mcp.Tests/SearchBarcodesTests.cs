using GroupDocs.Signature.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Signature.Mcp.IntegrationTests;

[Collection(McpServerCollection.Name)]
public class SearchBarcodesTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public SearchBarcodesTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task SearchBarcodes_BlankPdf_ReturnsNoneFoundOrEmptyJson()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SearchBarcodes.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"SearchBarcodes reported an error: {body}");

        var indicatesEmpty = body.Contains("No barcode", StringComparison.OrdinalIgnoreCase)
            || body.Contains("\"found\"", StringComparison.Ordinal);
        Assert.True(indicatesEmpty,
            $"Expected empty-result indicator. Response:\n{body}");
    }

    [Fact]
    public async Task SearchBarcodes_AcceptsOptionalTextFilter()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SearchBarcodes.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
                ["text"] = "INV-",
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"SearchBarcodes with text filter reported an error: {body}");
    }

    [Fact]
    public async Task SearchBarcodes_UnknownFile_ReturnsResponse()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SearchBarcodes.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = "missing.pdf" },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(string.IsNullOrWhiteSpace(body));
    }
}
