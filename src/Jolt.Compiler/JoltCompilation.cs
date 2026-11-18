using Jolt.Npm;
using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.Compiler;

public sealed record JoltSource(string Path, string Content);

public sealed record JoltCompileOptions(
    string RootNamespace = "App",
    IReadOnlyList<NpmVueComponent>? NpmComponents = null,
    IReadOnlyList<MetadataReference>? References = null);

public sealed record JoltModule(string Name, string Path, string Content, string SourceMap)
{
    public IReadOnlyList<string> ClrImports { get; init; } = [];
}

public sealed record JoltCompileResult(IReadOnlyList<JoltModule> Modules, IReadOnlyList<Diagnostic> Diagnostics);

public sealed record JoltProjectionSpan(TextSpan Source, TextSpan Generated);

public sealed record JoltLocalProperty(string Name, string? Type, bool Required, TextSpan? Source);
public sealed record JoltLocalComponent(string Name, string Path, IReadOnlyList<JoltLocalProperty> Properties, IReadOnlyList<string> Slots);

public sealed record JoltProjectedDocument(
    JoltDocument Document,
    SemanticModel SemanticModel,
    IReadOnlyList<JoltProjectionSpan> Spans,
    IReadOnlyList<JoltLocalComponent> LocalComponents);

public sealed record JoltAnalysis(IReadOnlyList<JoltProjectedDocument> Documents, IReadOnlyList<Diagnostic> Diagnostics);
