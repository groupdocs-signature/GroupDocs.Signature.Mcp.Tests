using System.Text.Json;
using GroupDocs.Signature.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Signature.Mcp.IntegrationTests;

[Collection(McpServerCollection.Name)]
public class GetDocumentInfoTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public GetDocumentInfoTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task GetDocumentInfo_BlankPdf_ReturnsJsonWithPageCount()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.GetDocumentInfo.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"GetDocumentInfo reported an error: {body}");

        // Tool returns JSON (Pitfall #16 — raw JSON, no TruncateText) — parse and
        // sanity-check at least one well-known field. Don't assert exact field name
        // (PageCount vs pageCount), just look for a numeric page count somewhere.
        using var doc = JsonDocument.Parse(body);
        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);

        // A 1-page blank PDF should report exactly 1 page somewhere in the response.
        Assert.Contains("1", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetDocumentInfo_Jpeg_ReturnsJson()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.GetDocumentInfo.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.PlainJpeg },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"GetDocumentInfo for JPEG reported an error: {body}");

        using var doc = JsonDocument.Parse(body);
        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
    }

    public static IEnumerable<object[]> RealSamples() => new[]
    {
        new object[] { SampleDocuments.SamplePdf  },
        new object[] { SampleDocuments.SampleDocx },
        new object[] { SampleDocuments.SampleXlsx },
        new object[] { SampleDocuments.SampleJpeg },
        new object[] { SampleDocuments.SamplePng  },
    };

    [Theory]
    [MemberData(nameof(RealSamples))]
    public async Task GetDocumentInfo_RealSample_ReturnsJson(string fileName)
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, fileName)))
        {
            _output.WriteLine($"Sample '{fileName}' not present in storage — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.GetDocumentInfo.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = fileName },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"GetDocumentInfo failed for '{fileName}': {body}");

        using var doc = JsonDocument.Parse(body);
        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
    }
}
