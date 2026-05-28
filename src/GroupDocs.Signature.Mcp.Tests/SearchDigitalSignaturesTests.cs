using GroupDocs.Signature.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Signature.Mcp.IntegrationTests;

[Collection(McpServerCollection.Name)]
public class SearchDigitalSignaturesTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public SearchDigitalSignaturesTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task SearchDigitalSignatures_BlankPdf_ReturnsNoneFoundOrEmptyJson()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SearchDigitalSignatures.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"SearchDigitalSignatures reported an error: {body}");

        var indicatesEmpty = body.Contains("No digital", StringComparison.OrdinalIgnoreCase)
            || body.Contains("\"found\"", StringComparison.Ordinal);
        Assert.True(indicatesEmpty,
            $"Expected empty-result indicator. Response:\n{body}");
    }

    [Fact]
    public async Task SearchDigitalSignatures_UnknownFile_ReturnsResponse()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SearchDigitalSignatures.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = "missing.pdf" },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(string.IsNullOrWhiteSpace(body));
    }

    [Fact]
    public async Task SearchDigitalSignatures_AcceptsPasswordParameter()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SearchDigitalSignatures.Name,
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
