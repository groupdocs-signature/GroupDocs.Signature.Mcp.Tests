using System.Text.Json;
using GroupDocs.Signature.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Signature.Mcp.IntegrationTests;

public class VerifyTests : IClassFixture<McpServerFixture>
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public VerifyTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Verify_BlankPdf_ReturnsValidityJson()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.Verify.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"Verify reported an error: {body}");

        // Tool returns a raw JSON object with isValid / succeeded / failed (Pitfall #16).
        using var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.TryGetProperty("isValid", out _),
            "Verify response should include 'isValid' property.");
        Assert.True(doc.RootElement.TryGetProperty("succeeded", out _),
            "Verify response should include 'succeeded' property.");
        Assert.True(doc.RootElement.TryGetProperty("failed", out _),
            "Verify response should include 'failed' property.");
    }

    [Theory]
    [InlineData("text")]
    [InlineData("qrcode")]
    [InlineData("barcode")]
    [InlineData("digital")]
    [InlineData("all")]
    public async Task Verify_TypeParameter_IsAcceptedAndReturnsJson(string type)
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.Verify.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
                ["type"] = type,
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"Verify with type '{type}' reported an error: {body}");

        using var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.TryGetProperty("isValid", out _),
            $"Verify response for type '{type}' should include 'isValid' property.");
    }

    [Fact]
    public async Task Verify_UnknownType_ReturnsDescriptiveError()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.Verify.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
                ["type"] = "not-a-real-type",
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        // Pitfall #18 — descriptive prefix instead of MCP's opaque wrapper.
        Assert.Contains("Verification failed for", body, StringComparison.Ordinal);
    }
}
