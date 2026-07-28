using System.Collections.Immutable;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgComponentCandidateMatrixTests
{
    private static readonly Lazy<CandidateFixture> Fixture = new(CreateFixture);

    public static IEnumerable<TestDataRow<ComponentCandidateCase>> Cases
        => ComponentCandidateCase.All.Select(static testCase => new TestDataRow<ComponentCandidateCase>(testCase)
        {
            DisplayName = "Candidate_" + testCase.Id
        });

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Discover_ClassifiesCurrentTailAndHandwrittenComponents(ComponentCandidateCase testCase)
    {
        var fixture = Fixture.Value;
        var symbol = fixture.Compilation.GetTypeByMetadataName(testCase.MetadataName);
        Assert.IsNotNull(symbol, testCase.MetadataName);

        Assert.AreEqual(testCase.IsCurrent, fixture.Current.Contains(symbol!, SymbolEqualityComparer.Default));
        Assert.AreEqual(testCase.IsTailRequired, fixture.TailRequired.Contains(symbol!, SymbolEqualityComparer.Default));
        Assert.AreEqual(testCase.IsHandwritten, fixture.Handwritten.Contains(symbol!, SymbolEqualityComparer.Default));
        Assert.AreEqual(
            testCase.HasHandwrittenMethod,
            RazorSgComponentCandidateSelector.FindHandwrittenBuildRenderTreeMethod(symbol!) is not null);
    }

    private static CandidateFixture CreateFixture()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var trees = ComponentCandidateCase.All
            .Select(testCase => CSharpSyntaxTree.ParseText(
                BuildSource(testCase),
                parseOptions,
                testCase.SourcePath))
            .ToArray();
        var compilation = CSharpCompilation.Create(
            "RazorVue.ComponentCandidate.Matrix",
            trees,
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        return new CandidateFixture(
            compilation,
            RazorSgComponentCandidateSelector.DiscoverCurrentComponents(compilation),
            RazorSgComponentCandidateSelector.DiscoverTailRequiredComponents(compilation),
            RazorSgComponentCandidateSelector.DiscoverHandwrittenComponents(compilation));
    }

    private static string BuildSource(ComponentCandidateCase testCase)
    {
        var module = testCase.HasModule
            ? "[ECMAScriptModule(\"./candidates/" + testCase.Id + "\")]"
            : string.Empty;
        var marker = testCase.HasVueMarker ? ", IVueComponent" : string.Empty;
        var buildRenderTree = testCase.HasBuildRenderTree
            ? """
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "candidate");
                }
                """
            : string.Empty;
        var declaration = $$"""
            {{module}}
            public sealed partial class {{testCase.TypeName}} : ComponentBase{{marker}}
            {
                {{buildRenderTree}}
            }
            """;
        if (testCase.ContainerName is not null)
        {
            declaration = $$"""
                public static class {{testCase.ContainerName}}
                {
                    {{declaration}}
                }
                """;
        }

        return $$"""
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue3;

            namespace RazorVue.Candidates;

            {{declaration}}
            """;
    }

    private sealed record CandidateFixture(
        Compilation Compilation,
        ImmutableArray<INamedTypeSymbol> Current,
        ImmutableArray<INamedTypeSymbol> TailRequired,
        ImmutableArray<INamedTypeSymbol> Handwritten);
}

public sealed record ComponentCandidateCase(
    string Id,
    string TypeName,
    string MetadataName,
    string SourcePath,
    string? ContainerName,
    bool HasModule,
    bool HasVueMarker,
    bool HasBuildRenderTree,
    bool IsCurrent,
    bool IsTailRequired,
    bool IsHandwritten,
    bool HasHandwrittenMethod)
{
    public static IReadOnlyList<ComponentCandidateCase> All { get; } = Create();

    private static IReadOnlyList<ComponentCandidateCase> Create()
    {
        var cases = new List<ComponentCandidateCase>(40);
        string[] sourceExtensions = [".razor.cs", ".razor.g.cs", ".cs", ".g.cs"];
        foreach (var extension in sourceExtensions)
        {
            foreach (var hasModule in new[] { false, true })
            {
                foreach (var hasVueMarker in new[] { false, true })
                {
                    foreach (var hasBuildRenderTree in new[] { false, true })
                    {
                        Add(
                            cases,
                            "top_level_" + NormalizeExtension(extension) +
                            "_module_" + Flag(hasModule) +
                            "_marker_" + Flag(hasVueMarker) +
                            "_render_" + Flag(hasBuildRenderTree),
                            extension,
                            hasBuildRenderTree,
                            hasModule,
                            hasVueMarker,
                            nested: false);
                    }
                }
            }
        }

        Add(cases, "nested_razor_tail", ".razor.cs", hasBuildRenderTree: false, hasModule: true, hasVueMarker: true, nested: true);
        Add(cases, "nested_razor_handwritten", ".razor.cs", hasBuildRenderTree: true, hasModule: true, hasVueMarker: true, nested: true);
        Add(cases, "nested_generated_tail", ".razor.g.cs", hasBuildRenderTree: true, hasModule: true, hasVueMarker: true, nested: true);
        Add(cases, "nested_plain_authored", ".cs", hasBuildRenderTree: false, hasModule: true, hasVueMarker: true, nested: true);
        Add(cases, "nested_plain_handwritten", ".cs", hasBuildRenderTree: true, hasModule: true, hasVueMarker: true, nested: true);
        Add(cases, "nested_missing_module", ".razor.cs", hasBuildRenderTree: true, hasModule: false, hasVueMarker: true, nested: true);
        Add(cases, "nested_missing_marker", ".razor.cs", hasBuildRenderTree: true, hasModule: true, hasVueMarker: false, nested: true);
        Add(cases, "nested_generated_non_razor", ".generated.cs", hasBuildRenderTree: true, hasModule: true, hasVueMarker: true, nested: true);
        return cases;
    }

    private static void Add(
        List<ComponentCandidateCase> cases,
        string id,
        string extension,
        bool hasBuildRenderTree,
        bool hasModule,
        bool hasVueMarker,
        bool nested)
    {
        var suffix = cases.Count.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
        var typeName = "Candidate" + suffix;
        var containerName = nested ? "Container" + suffix : null;
        var metadataName = containerName is null
            ? "RazorVue.Candidates." + typeName
            : "RazorVue.Candidates." + containerName + "+" + typeName;
        var isCurrent = hasModule && hasVueMarker;
        var isGeneratedSource = extension.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ||
                                extension.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase) ||
                                extension.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase);
        var hasHandwrittenMethod = hasBuildRenderTree && !isGeneratedSource;
        var isRazorAuthored = extension.EndsWith(".razor.cs", StringComparison.OrdinalIgnoreCase) ||
                              extension.EndsWith(".razor.g.cs", StringComparison.OrdinalIgnoreCase);
        var isHandwritten = isCurrent && hasHandwrittenMethod;
        var isTailRequired = isCurrent && isRazorAuthored && !hasHandwrittenMethod;
        cases.Add(new ComponentCandidateCase(
            id,
            typeName,
            metadataName,
            "Candidates/" + typeName + extension,
            containerName,
            hasModule,
            hasVueMarker,
            hasBuildRenderTree,
            isCurrent,
            isTailRequired,
            isHandwritten,
            hasHandwrittenMethod));
    }

    private static string NormalizeExtension(string extension)
        => extension.TrimStart('.').Replace('.', '_');

    private static string Flag(bool value)
        => value ? "yes" : "no";
}
