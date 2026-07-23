using GroupDocs.Signature.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Signature.Mcp.IntegrationTests;

public class SearchImageSignaturesTests : IClassFixture<McpServerFixture>
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public SearchImageSignaturesTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task SearchImageSignatures_BlankPdf_ReturnsNoneFoundOrEmptyJson()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SearchImageSignatures.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"SearchImageSignatures reported an error: {body}");

        var indicatesEmpty = body.Contains("No image", StringComparison.OrdinalIgnoreCase)
            || body.Contains("\"found\"", StringComparison.Ordinal);
        Assert.True(indicatesEmpty,
            $"Expected empty-result indicator. Response:\n{body}");
    }

    [Fact]
    public async Task SearchImageSignatures_UnknownFile_ReturnsResponse()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SearchImageSignatures.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = "missing.pdf" },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(string.IsNullOrWhiteSpace(body));
    }

    [Fact]
    public async Task SearchImageSignatures_AcceptsPasswordParameter()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SearchImageSignatures.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
                ["password"] = "ignored",
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(string.IsNullOrWhiteSpace(body));
    }
}
