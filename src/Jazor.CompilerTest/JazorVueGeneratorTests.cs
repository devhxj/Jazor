using System.Collections.Immutable;
using Basic.Reference.Assemblies;
using Jazor.Vue;
using Jazor.Vue.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueGeneratorTests
{
    [TestMethod]
    public void JazorVue_Generator_EmitsArtifactCatalogSourceForAdditionalJazorDocument()
    {
        var compilation = CreateCompilation(
            """
            namespace TestHost;

            public sealed class HostComponent
            {
            }
            """);

        var additionalText = new InMemoryAdditionalText(
            "Features/Counter.jazor",
            """
            @jsimport dayjs from "dayjs"

            <template>
              <div>{{ dayjs() }}</div>
            </template>

            @code {
                [Prop] public string Title { get; set; } = "";
            }
            """);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators:
            [
                new JazorVueGenerator().AsSourceGenerator()
            ],
            additionalTexts:
            [
                additionalText
            ],
            parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        var runResult = driver.GetRunResult();
        var generatedSources = runResult.Results[0].GeneratedSources;
        var generatedSource = generatedSources
            .Single(source => source.HintName == "Jazor.Generated.VueArtifacts.g.cs")
            .SourceText
            .ToString();
        var externalDeclarationsSource = generatedSources
            .Single(source => source.HintName.StartsWith("Jazor.Generated.JazorVueExternals.Counter_", StringComparison.Ordinal))
            .SourceText
            .ToString();
        var analysisSource = generatedSources
            .Single(source => source.HintName.StartsWith("Jazor.Generated.JazorVueAnalysis.Counter_", StringComparison.Ordinal))
            .SourceText
            .ToString();
        var analysisSupportSource = generatedSources
            .Single(source => source.HintName == "Jazor.Generated.JazorVueAnalysisSupport.g.cs")
            .SourceText
            .ToString();

        var errors = outputCompilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static x => x.ToString())));
        StringAssert.Contains(generatedSource, "public static partial class JazorVueArtifacts");
        StringAssert.Contains(generatedSource, "IReadOnlyList<(string Name, string VueText)> GetArtifacts()");
        StringAssert.Contains(generatedSource, "IReadOnlyList<(string Name, string ExternalDeclarationsText)> GetExternalDeclarations()");
        StringAssert.Contains(generatedSource, "IReadOnlyList<(string Name, global::System.Collections.Generic.IReadOnlyList<global::Jazor.Vue.Generated.Analysis.JazorVueImportBindingInfo> Imports)> GetImportInfos()");
        StringAssert.Contains(generatedSource, "(@\"Counter\", @\"");
        StringAssert.Contains(generatedSource, "import dayjs from \"\"dayjs\"\";");
        StringAssert.Contains(generatedSource, "new global::Jazor.Vue.Generated.Analysis.JazorVueImportBindingInfo[]");
        StringAssert.Contains(generatedSource, "JazorVueImportClassification.Value");
        StringAssert.Contains(generatedSource, "const props = defineProps({");
        StringAssert.Contains(generatedSource, "<template>");
        StringAssert.Contains(generatedSource, "<div>{{ dayjs() }}</div>");
        StringAssert.Contains(externalDeclarationsSource, "namespace Jazor.Vue.Generated");
        StringAssert.Contains(externalDeclarationsSource, "public static readonly __JsValueSymbol dayjs;");
        StringAssert.Contains(analysisSource, "using static global::Jazor.Vue.Generated.__JazorVirtualExternals_Counter_");
        StringAssert.Contains(analysisSource, "internal partial class __JazorAnalysis_Counter_");
        StringAssert.Contains(analysisSource, "IReadOnlyList<global::Jazor.Vue.Generated.Analysis.JazorVueImportBindingInfo> __jazorImportInfos");
        StringAssert.Contains(analysisSource, "protected static global::Jazor.Vue.Generated.Analysis.JazorVueImportBindingInfo dayjsInfo => __jazorImportInfos[0];");
        StringAssert.Contains(analysisSource, "JazorVueImportClassification.Value");
        StringAssert.Contains(analysisSource, "__JsValueSymbol dayjsSymbol => global::Jazor.Vue.Generated.__JazorVirtualExternals_Counter_");
        StringAssert.Contains(analysisSource, "protected static dynamic dayjs => dayjsSymbol;");
        StringAssert.Contains(analysisSupportSource, "internal sealed class PropAttribute");
        StringAssert.Contains(analysisSupportSource, "public enum JazorVueImportClassification");
        StringAssert.Contains(analysisSupportSource, "public readonly struct JazorVueImportBindingInfo");
    }

    [TestMethod]
    public void JazorVue_Generator_EmitsAnalysisStubThatBindsImportedSymbolsIntoCompilation()
    {
        var compilation = CreateCompilation(
            """
            namespace TestHost;

            public sealed class HostComponent
            {
            }
            """);

        var additionalText = new InMemoryAdditionalText(
            "Features/Bindings.jazor",
            """
            @jsimport { debounce } from "lodash-es"
            @jsimport * as math from "./math"
            @vueimport UserCard from "./UserCard.vue"

            <template>
              <UserCard />
            </template>

            @code {
                [State] private object current = debounce("x");
                [State] private object pi = math.PI;

                public object Read()
                {
                    return UserCard;
                }
            }
            """);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators:
            [
                new JazorVueGenerator().AsSourceGenerator()
            ],
            additionalTexts:
            [
                additionalText
            ],
            parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        var runResult = driver.GetRunResult();
        var generatedSources = runResult.Results[0].GeneratedSources;
        var externalDeclarationsSource = generatedSources
            .Single(source => source.HintName.StartsWith("Jazor.Generated.JazorVueExternals.Bindings_", StringComparison.Ordinal))
            .SourceText
            .ToString();
        var analysisSource = generatedSources
            .Single(source => source.HintName.StartsWith("Jazor.Generated.JazorVueAnalysis.Bindings_", StringComparison.Ordinal))
            .SourceText
            .ToString();
        var analysisHintName = generatedSources
            .Single(source => source.HintName.StartsWith("Jazor.Generated.JazorVueAnalysis.Bindings_", StringComparison.Ordinal))
            .HintName;
        var errors = outputCompilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        var analysisType = ResolveAnalysisType(outputCompilation, analysisHintName);

        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static x => x.ToString())));
        Assert.IsNotNull(analysisType);
        Assert.IsNotNull(analysisType.GetMembers("debounceInfo").SingleOrDefault());
        Assert.IsNotNull(analysisType.GetMembers("mathInfo").SingleOrDefault());
        Assert.IsNotNull(analysisType.GetMembers("UserCardInfo").SingleOrDefault());
        Assert.IsNotNull(analysisType.GetMembers("debounceSymbol").SingleOrDefault());
        Assert.IsNotNull(analysisType.GetMembers("mathSymbol").SingleOrDefault());
        Assert.IsNotNull(analysisType.GetMembers("UserCardSymbol").SingleOrDefault());
        StringAssert.Contains(generatedSources.Single(source => source.HintName == "Jazor.Generated.VueArtifacts.g.cs").SourceText.ToString(), "JazorVueImportClassification.Callable");
        StringAssert.Contains(generatedSources.Single(source => source.HintName == "Jazor.Generated.VueArtifacts.g.cs").SourceText.ToString(), "JazorVueImportClassification.Namespace");
        StringAssert.Contains(generatedSources.Single(source => source.HintName == "Jazor.Generated.VueArtifacts.g.cs").SourceText.ToString(), "JazorVueImportClassification.Component");
        StringAssert.Contains(externalDeclarationsSource, "public static readonly __JsFunctionSymbol debounce;");
        StringAssert.Contains(externalDeclarationsSource, "public static readonly __JsNamespaceSymbol math;");
        StringAssert.Contains(externalDeclarationsSource, "public static readonly __VueComponentSymbol UserCard;");
        StringAssert.Contains(analysisSource, "IReadOnlyList<global::Jazor.Vue.Generated.Analysis.JazorVueImportBindingInfo> __jazorImportInfos");
        StringAssert.Contains(analysisSource, "protected static global::Jazor.Vue.Generated.Analysis.JazorVueImportBindingInfo debounceInfo => __jazorImportInfos[0];");
        StringAssert.Contains(analysisSource, "JazorVueImportClassification.Callable");
        StringAssert.Contains(analysisSource, "protected static global::Jazor.Vue.Generated.Analysis.JazorVueImportBindingInfo mathInfo => __jazorImportInfos[1];");
        StringAssert.Contains(analysisSource, "JazorVueImportClassification.Namespace");
        StringAssert.Contains(analysisSource, "protected static global::Jazor.Vue.Generated.Analysis.JazorVueImportBindingInfo UserCardInfo => __jazorImportInfos[2];");
        StringAssert.Contains(analysisSource, "JazorVueImportClassification.Component");
        StringAssert.Contains(analysisSource, "__JsFunctionSymbol debounceSymbol => global::Jazor.Vue.Generated.__JazorVirtualExternals_Bindings_");
        StringAssert.Contains(analysisSource, "__JsNamespaceSymbol mathSymbol => global::Jazor.Vue.Generated.__JazorVirtualExternals_Bindings_");
        StringAssert.Contains(analysisSource, "__VueComponentSymbol UserCardSymbol => global::Jazor.Vue.Generated.__JazorVirtualExternals_Bindings_");
        StringAssert.Contains(analysisSource, "protected static dynamic debounce => debounceSymbol;");
        StringAssert.Contains(analysisSource, "protected static dynamic math => mathSymbol;");
        StringAssert.Contains(analysisSource, "protected static dynamic UserCard => UserCardSymbol;");
        StringAssert.Contains(analysisSource, "[State] private object current = debounce(\"x\");");
        StringAssert.Contains(analysisSource, "[State] private object pi = math.PI;");
        StringAssert.Contains(analysisSource, "return UserCard;");
    }

    [TestMethod]
    public void JazorVue_Generator_EmitsExternalDeclarationsIntoCompilation()
    {
        const string documentPath = "Features/Counter.jazor";
        var containerName = JazorVueExternalDeclarationEmitter.CreateContainerName(documentPath);
        var compilation = CreateCompilation(
            $$"""
            namespace TestHost;

            public sealed class HostComponent
            {
                public object JsValue => global::Jazor.Vue.Generated.{{containerName}}.dayjs;

                public object VueComponent => global::Jazor.Vue.Generated.{{containerName}}.UserCard;
            }
            """);

        var additionalText = new InMemoryAdditionalText(
            documentPath,
            """
            @jsimport dayjs from "dayjs"
            @vueimport UserCard from "./UserCard.vue"

            <template>
              <UserCard :title="title" />
            </template>

            @code {
                [Prop] public string Title { get; set; } = "";
            }
            """);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators:
            [
                new JazorVueGenerator().AsSourceGenerator()
            ],
            additionalTexts:
            [
                additionalText
            ],
            parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        var runResult = driver.GetRunResult();
        var generatedSources = runResult.Results[0].GeneratedSources;
        var externalDeclarationsSource = generatedSources
            .Single(source => source.HintName.StartsWith("Jazor.Generated.JazorVueExternals.Counter_", StringComparison.Ordinal))
            .SourceText
            .ToString();
        var errors = outputCompilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        var externalContainer = outputCompilation.GetTypeByMetadataName("Jazor.Vue.Generated." + containerName);

        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static x => x.ToString())));
        Assert.IsNotNull(externalContainer);
        Assert.IsNotNull(externalContainer.GetMembers("dayjs").SingleOrDefault());
        Assert.IsNotNull(externalContainer.GetMembers("UserCard").SingleOrDefault());
        StringAssert.Contains(externalDeclarationsSource, "namespace Jazor.Vue.Generated");
        StringAssert.Contains(externalDeclarationsSource, "public static class " + containerName);
        StringAssert.Contains(externalDeclarationsSource, "public static readonly __JsValueSymbol dayjs;");
        StringAssert.Contains(externalDeclarationsSource, "public static readonly __VueComponentSymbol UserCard;");
    }

    [TestMethod]
    public async Task JazorVue_Analyzer_ReportsDiagnosticWhenNamespaceImportIsInvoked()
    {
        var diagnostics = await GetJazorVueDiagnosticsAsync(
            "Features/InvalidNamespaceInvocation.jazor",
            """
            @jsimport * as math from "./math"

            <template>
              <div />
            </template>

            @code {
                public object Read()
                {
                    return math();
                }
            }
            """);
        var diagnostic = diagnostics.Single(d => d.Id == "JAZORJV001");
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
        StringAssert.Contains(diagnostic.GetMessage(), "math");
        StringAssert.Contains(diagnostic.Location.GetMappedLineSpan().Path, "InvalidNamespaceInvocation.jazor");
    }

    [TestMethod]
    public async Task JazorVue_Analyzer_ReportsDiagnosticWhenComponentImportIsInvoked()
    {
        var diagnostics = await GetJazorVueDiagnosticsAsync(
            "Features/InvalidComponentInvocation.jazor",
            """
            @vueimport UserCard from "./UserCard.vue"

            <template>
              <UserCard />
            </template>

            @code {
                public object Read()
                {
                    return UserCard();
                }
            }
            """);
        var diagnostic = diagnostics.Single(d => d.Id == "JAZORJV002");
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
        StringAssert.Contains(diagnostic.GetMessage(), "UserCard");
        StringAssert.Contains(diagnostic.Location.GetMappedLineSpan().Path, "InvalidComponentInvocation.jazor");
    }

    [TestMethod]
    public async Task JazorVue_Analyzer_DoesNotReportDiagnosticWhenCallableImportIsInvoked()
    {
        var diagnostics = await GetJazorVueDiagnosticsAsync(
            "Features/ValidCallableInvocation.jazor",
            """
            @jsimport { debounce } from "lodash-es"

            <template>
              <div />
            </template>

            @code {
                public object Read()
                {
                    return debounce("x");
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORJV001", "JAZORJV002");
    }

    private static INamedTypeSymbol ResolveAnalysisType(Compilation compilation, string analysisHintName)
    {
        var prefix = "Jazor.Generated.JazorVueAnalysis.";
        var suffix = ".g.cs";
        var identity = analysisHintName.Substring(prefix.Length, analysisHintName.Length - prefix.Length - suffix.Length);
        return compilation.GetTypeByMetadataName("Jazor.Vue.Generated.Analysis.__JazorAnalysis_" + identity)!;
    }

    private static async Task<ImmutableArray<Diagnostic>> GetJazorVueDiagnosticsAsync(string path, string documentText)
    {
        var compilation = CreateCompilation(
            """
            namespace TestHost;

            public sealed class HostComponent
            {
            }
            """);
        var additionalText = new InMemoryAdditionalText(path, documentText);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators:
            [
                new JazorVueGenerator().AsSourceGenerator()
            ],
            additionalTexts:
            [
                additionalText
            ],
            parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        var compileErrors = outputCompilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, compileErrors.Length, string.Join(Environment.NewLine, compileErrors.Select(static x => x.ToString())));

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
            new JazorVueImportUsageAnalyzer());
        return await outputCompilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync();
    }

    private static Compilation CreateCompilation(string source)
    {
        var references = Net100.References.All
            .Cast<MetadataReference>()
            .ToImmutableArray();

        return CSharpCompilation.Create(
            assemblyName: "JazorVue.Generator.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(source)
            ],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static void AssertHasDiagnostic(IEnumerable<Diagnostic> diagnostics, string id)
        => Assert.IsTrue(
            diagnostics.Any(diagnostic => diagnostic.Id == id),
            $"Expected diagnostic {id}, actual: {string.Join(Environment.NewLine, diagnostics.Select(static x => x.ToString()))}");

    private static void AssertNoDiagnostic(IEnumerable<Diagnostic> diagnostics, params string[] ids)
    {
        var unexpected = diagnostics
            .Where(diagnostic => ids.Contains(diagnostic.Id, StringComparer.Ordinal))
            .ToArray();

        Assert.AreEqual(0, unexpected.Length, string.Join(Environment.NewLine, unexpected.Select(static x => x.ToString())));
    }

    private sealed class InMemoryAdditionalText : AdditionalText
    {
        private readonly SourceText _sourceText;

        public InMemoryAdditionalText(string path, string text)
        {
            Path = path;
            _sourceText = SourceText.From(text);
        }

        public override string Path { get; }

        public override SourceText GetText(CancellationToken cancellationToken = default)
            => _sourceText;
    }
}
