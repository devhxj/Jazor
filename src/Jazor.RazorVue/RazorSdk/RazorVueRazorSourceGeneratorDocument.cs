using System.Collections.Immutable;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis.Razor;

namespace Jazor.RazorVue.RazorSdk;

internal sealed record RazorVueRazorSourceGeneratorDocument(
    string HintName,
    RazorCodeDocument CodeDocument,
    RazorCSharpDocument CSharpDocument,
    ImmutableArray<TagHelperDescriptor> TagHelpers,
    DocumentIntermediateNode DocumentNode,
    ImmutableArray<SourceMapping> SourceMappings);
