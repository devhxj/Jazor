namespace Jazor.Analyzer.RazorVue.Generation;

internal readonly record struct RazorSourceGeneratorDocumentOutput(
    string HintName,
    object CodeDocument,
    object CSharpDocument);
