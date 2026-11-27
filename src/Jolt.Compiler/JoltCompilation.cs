using Jazor.Npm;
using Jolt.Npm;
using Jolt.Syntax;
using Jazor.Common.SourceMaps;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.Compiler;

public sealed record JoltSource(string Path, string Content);

public sealed record JoltCompileOptions(
    string RootNamespace = "App",
    IReadOnlyList<NpmVueComponent>? NpmComponents = null,
    IReadOnlyList<MetadataReference>? References = null)
{
    public IReadOnlyList<NpmValueExport> NpmValues { get; init; } = [];
    public IReadOnlyList<JoltSource> CSharpSources { get; init; } = [];
    internal IReadOnlyList<JoltSource> ProjectSources { get; init; } = [];
    internal IReadOnlyList<string> ProjectDefines { get; init; } = [];
    public IReadOnlyList<NpmVueComponent> VueComponents { get; init; } = [];
    public string VueSourceRootRelative { get; init; } = ".";
    public bool Router { get; init; }
}

public sealed record JoltModule(string Name, string Path, string Content, string SourceMap)
{
    public IReadOnlyList<string> ClrImports { get; init; } = [];
    public IReadOnlyList<string> LocalVueImports { get; init; } = [];
    public string? ScopedCss { get; init; }
    public string TypeScriptDeclaration { get; init; } = "";
    internal string? TypeScriptSourceMap { get; init; }

    public string? CreateTypeScriptSourceMap(string declarationFileName, string relativeSourcePath)
    {
        if (TypeScriptSourceMap is not { } encodedMap)
            return null;
        var map = new SourceMapReader().Read(encodedMap);
        return new SourceMapWriter().Write(map with
        {
            File = declarationFileName,
            Sources = [map.Sources[0] with { Path = relativeSourcePath.Replace('\\', '/') }],
        });
    }
}

public sealed record JoltCompileResult(IReadOnlyList<JoltModule> Modules, IReadOnlyList<Diagnostic> Diagnostics)
{
    public const string RouterModulePath = "_routes.mjs";
    public string? RouterModule { get; init; }
}

public sealed record JoltProjectionSpan(TextSpan Source, TextSpan Generated);

public sealed record JoltLocalProperty(string Name, string? Type, bool Required, TextSpan? Source)
{
    internal string? DocumentationXml { get; init; }
}
public sealed record JoltLocalModel(string Property, string Type);
public enum JoltMarkupKind { Component, Property, Slot }
public sealed record JoltMarkupSymbol(string ComponentPath, JoltMarkupKind Kind, string Name = "");
public sealed record JoltMarkupBinding(JoltMarkupSymbol Symbol, TextSpan Span, bool IsDeclaration = false);

public sealed record JoltLocalComponent(string Name, string Path, IReadOnlyList<JoltLocalProperty> Properties, IReadOnlyList<string> Slots)
{
    public string Namespace { get; init; } = "";
    public bool HasReferenceContract { get; init; }
    public IReadOnlyList<JoltLocalModel> Models { get; init; } = [];
}

public sealed record JoltProjectedDocument(
    JoltDocument Document,
    SemanticModel SemanticModel,
    IReadOnlyList<JoltProjectionSpan> Spans,
    IReadOnlyList<JoltLocalComponent> LocalComponents)
{
    public IReadOnlyList<JoltMarkupBinding> MarkupBindings { get; init; } = [];
    public IReadOnlyList<string> Imports { get; init; } = [];
    public IReadOnlyList<NpmVueComponent> VueComponents { get; init; } = [];
    public IReadOnlyDictionary<(string Scope, string Member), (string Path, TextSpan Span)> SlotMemberOrigins { get; init; }
        = new Dictionary<(string, string), (string, TextSpan)>();
    public IReadOnlyDictionary<(string Scope, string Member), NpmDefinition> ExternalSlotMemberOrigins { get; init; }
        = new Dictionary<(string, string), NpmDefinition>();
}

public sealed record JoltAnalysis(IReadOnlyList<JoltProjectedDocument> Documents, IReadOnlyList<Diagnostic> Diagnostics)
{
    public IReadOnlyList<JoltLocalComponent> LocalComponents { get; init; } = [];
    public IReadOnlyDictionary<ISymbol, NpmDefinition> NpmValueDefinitions { get; init; }
        = new Dictionary<ISymbol, NpmDefinition>(SymbolEqualityComparer.Default);
}
