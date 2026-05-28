using GroupDocs.Signature.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Signature.Mcp.IntegrationTests;

[Collection(McpServerCollection.Name)]
public class SearchQrCodesTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public SearchQrCodesTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task SearchQrCodes_BlankPdf_ReturnsNoneFoundOrEmptyJson()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SearchQrCodes.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"SearchQrCodes reported an error: {body}");

        var indicatesEmpty = body.Contains("No QR", StringComparison.OrdinalIgnoreCase)
            || body.Contains("\"found\"", StringComparison.Ordinal);
        Assert.True(indicatesEmpty,
            $"Expected empty-result indicator. Response:\n{body}");
    }

    [Fact]
    public async Task SearchQrCodes_AcceptsOptionalTextFilter()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SearchQrCodes.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
                ["text"] = "https://",
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"SearchQrCodes with text filter reported an error: {body}");
    }

    [Fact]
    public async Task SearchQrCodes_UnknownFile_ReturnsResponse()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SearchQrCodes.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = "missing.pdf" },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(string.IsNullOrWhiteSpace(body));
    }
}
