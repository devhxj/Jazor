using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.RazorSdk;

internal sealed record RazorVueRazorSourceGeneratorDocument(
    string HintName,
    Jazor.RazorVue.RazorVueRazorDocument PrimaryDocument,
    ImmutableArray<Jazor.RazorVue.RazorVueRazorDocument> ImportDocuments,
    SourceText CSharpText,
    ImmutableArray<RazorVueRazorSourceMapping> SourceMappings,
    RazorVueRazorIrNode DocumentNode);
