using GroupDocs.Signature.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Signature.Mcp.IntegrationTests;

public class SignTests : IClassFixture<McpServerFixture>
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public SignTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Theory]
    [InlineData("text",    "APPROVED")]
    [InlineData("qrcode",  "https://example.com/sig/1")]
    [InlineData("barcode", "INV-2026-001")]
    public async Task Sign_BlankPdf_WritesSignedOutput(string type, string text)
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.Sign.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
                ["type"] = type,
                ["text"] = text,
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"Sign failed for type '{type}': {body}");

        // Tool's [Description] contract: response mentions the signature type and a
        // saved-path / download link. In eval mode it's prefixed with "[Evaluation mode]".
        Assert.Contains("Signed", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(type, body, StringComparison.OrdinalIgnoreCase);

        // Verify the signed output was written to storage.
        var signedFile = Path.Combine(_fixture.StoragePath, "blank_signed.pdf");
        Assert.True(File.Exists(signedFile),
            $"Expected signed file at '{signedFile}'. Response body:\n{body}");
    }

    [Fact]
    public async Task Sign_UnknownType_ReturnsDescriptiveError()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.Sign.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
                ["type"] = "not-a-real-type",
                ["text"] = "irrelevant",
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        // Pitfall #18 — the tool must surface a "Signing failed for" prefix rather
        // than let the MCP SDK wrap it as "An error occurred invoking 'sign'".
        Assert.Contains("Signing failed for", body, StringComparison.Ordinal);
    }

    public static IEnumerable<object[]> RealSignableSamples() => new[]
    {
        new object[] { SampleDocuments.SamplePdf,  "sample_signed.pdf"  },
        new object[] { SampleDocuments.SampleDocx, "sample_signed.docx" },
        new object[] { SampleDocuments.SampleXlsx, "sample_signed.xlsx" },
    };

    [Theory]
    [MemberData(nameof(RealSignableSamples))]
    public async Task Sign_RealSample_WritesSignedOutput(string fileName, string expectedSignedFileName)
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, fileName)))
        {
            _output.WriteLine($"Sample '{fileName}' not present in storage — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.Sign.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = fileName },
                ["type"] = "text",
                ["text"] = "Reviewed",
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"Sign failed for '{fileName}': {body}");

        var signedPath = Path.Combine(_fixture.StoragePath, expectedSignedFileName);
        Assert.True(File.Exists(signedPath),
            $"Expected signed file at '{signedPath}'. Response body:\n{body}");
    }
}
