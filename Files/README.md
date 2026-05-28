# Files/ — integration test samples

Real document samples used by the per-format `[Theory]` tests
(`Sign_RealSample_WritesSignedOutput`, `GetDocumentInfo_RealSample_ReturnsJson`,
etc.). The fixtures (`SampleDocuments.CopyRealSamples`) stage them into the
per-test temp storage folder; tests skip themselves when a sample is missing,
so this folder may be safely pruned.

## Provenance

All files are copied verbatim from the upstream
[GroupDocs.Signature for .NET examples repo](https://github.com/groupdocs-signature/GroupDocs.Signature-for-.NET),
under `Examples/GroupDocs.Signature.Examples.CSharp/Resources/SampleFiles/`.

| File         | Source path                              | Used by                          |
|--------------|------------------------------------------|----------------------------------|
| `sample.pdf` | `SampleFiles/sample.pdf`                 | Sign, GetDocumentInfo theories   |
| `sample.docx`| `SampleFiles/sample.docx`                | Sign, GetDocumentInfo theories   |
| `sample.xlsx`| `SampleFiles/sample.xlsx`                | Sign, GetDocumentInfo theories   |
| `sample.png` | `SampleFiles/sample.png`                 | GetDocumentInfo theory           |
| `sample.jpg` | `SampleFiles/Images/signature_handwrite.jpg` | GetDocumentInfo theory       |

## License

Examples-repo content is published under the MIT license (see the upstream
repo's LICENSE) — the same license as this Tests repo.

## Adding more

If you need a sample for a new format:

1. Pick the smallest representative file from the upstream examples repo
   (or generate a minimal valid file).
2. Drop it under `Files/`.
3. Add a `public const string Sample…` to
   `src/GroupDocs.Signature.Mcp.Tests/Fixtures/SampleDocuments.cs` and
   include it in `RealSamples`.
4. Reference it from the relevant theory `MemberData`.
5. Update the provenance table above.
