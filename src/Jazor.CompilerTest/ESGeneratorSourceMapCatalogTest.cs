using System.Reflection;
using System.Linq;
using System.Threading;
using Acornima.Ast;
using Jazor.ComplierTest;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class ESGeneratorSourceMapCatalogTest
{
    [TestMethod]
    public void RunGenerator_StaticModule_EmitsSourceMapInTheModuleCatalog()
    {
        var compilation = CreateCompilation(
            "SourceMapCatalog.Generated",
            """
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Modules
            {
                [ECMAScript.ECMAScriptModule("./modules/math")]
                public static class MathModule
                {
                    public static int Add(int left, int right)
                    {
                        var sum = left + right;
                        return sum;
                    }
                }
            }
            """,
            "Demo/MathModule.cs");

        var (_, runResult) = RunGeneratorWithResult(compilation);
        var diagnostics = runResult.Results
            .SelectMany(static result => result.Diagnostics)
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.AreEqual(0, diagnostics.Length, string.Join("\n", diagnostics.Select(static item => item.ToString())));

        var hintNames = runResult.Results
            .SelectMany(static result => result.GeneratedSources)
            .Select(static source => source.HintName)
            .ToArray();
        CollectionAssert.Contains(hintNames, "Jazor.Generated.ModuleCatalog.g.cs");
        CollectionAssert.AreEquivalent(
            new[] { "Jazor.Generated.ModuleCatalog.g.cs" },
            hintNames);

        var moduleCatalog = GetGeneratedSource(runResult, "Jazor.Generated.ModuleCatalog.g.cs");
        StringAssert.Contains(moduleCatalog, "internal static partial class ModuleCatalog");
        StringAssert.Contains(moduleCatalog, "SourceMapRelativePath");
        StringAssert.Contains(moduleCatalog, "SourceMapContent");
        StringAssert.Contains(moduleCatalog, "MapHash");
        StringAssert.Contains(moduleCatalog, "sourceMapRelativePath:");
        StringAssert.Contains(moduleCatalog, "modules/math.mjs.map");
        StringAssert.Contains(moduleCatalog, "Demo/MathModule.cs");
    }

    [TestMethod]
    public void RunGenerator_SourceContentLookup_DistinguishesSameNamedFilesByNormalizedTreePath()
    {
        const string attributeSource = """
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }
            """;
        const string alphaSource = """
            [ECMAScript.ECMAScriptModule("modules/alpha")]
            public static class AlphaModule
            {
                public static int Read() => 11;
            }
            """;
        const string betaSource = """
            [ECMAScript.ECMAScriptModule("modules/beta")]
            public static class BetaModule
            {
                public static int Read() => 22;
            }
            """;

        var sourceRoot = Path.Combine(Path.GetTempPath(), "jazor-esgenerator-source-content");
        var attributePath = Path.Combine(sourceRoot, "Contracts", "ECMAScriptModuleAttribute.cs");
        var alphaPath = Path.Combine(sourceRoot, "Alpha", "SharedModule.cs");
        var betaPath = Path.Combine(sourceRoot, "Beta", "SharedModule.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "SourceMapCatalog.DuplicateFileNames.Generated",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(attributeSource, TestMetadataReferences.PreviewParseOptions, path: attributePath),
                CSharpSyntaxTree.ParseText(alphaSource, TestMetadataReferences.PreviewParseOptions, path: alphaPath),
                CSharpSyntaxTree.ParseText(betaSource, TestMetadataReferences.PreviewParseOptions, path: betaPath)
            ],
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var probes = new Dictionary<string, SourceContentProbe>(StringComparer.Ordinal);

        using var _ = OverrideSourceMapArtifactFactoryForTest(
            (_, generatedFileName, _, observedSourceRoot, readSourceContent) =>
            {
                probes.Add(
                    generatedFileName,
                    new SourceContentProbe(
                        observedSourceRoot,
                        readSourceContent!(alphaPath),
                        readSourceContent(betaPath.Replace('\\', '/')),
                        readSourceContent("SharedModule.cs"),
                        readSourceContent(" ")));
                return new GeneratedJavaScriptArtifact("export {};", "{}", "js-hash", "map-hash");
            });

        var (outputCompilation, runResult) = RunGeneratorWithResult(compilation);

        Assert.HasCount(2, probes);
        foreach (var probe in probes.Values)
        {
            Assert.AreEqual(Path.GetFullPath(sourceRoot), probe.SourceRoot);
            Assert.AreEqual(alphaSource, probe.AlphaContent);
            Assert.AreEqual(betaSource, probe.BetaContent);
            Assert.IsNull(probe.AmbiguousFileNameContent);
            Assert.IsNull(probe.BlankPathContent);
        }

        var diagnostics = runResult.Results.SelectMany(static result => result.Diagnostics).ToArray();
        Assert.HasCount(0, diagnostics, string.Join(Environment.NewLine, diagnostics.Select(static item => item.ToString())));
        var errors = outputCompilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static item => item.ToString())));
    }

    [TestMethod]
    public void RunGenerator_StaticModule_WhenSourceMapGenerationFails_ReportsWarningAndKeepsTheModuleCatalog()
    {
        var compilation = CreateCompilation(
            "SourceMapCatalog.Fallback.Generated",
            """
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Modules
            {
                [ECMAScript.ECMAScriptModule("./modules/math")]
                public static class MathModule
                {
                    public static int Add(int left, int right)
                    {
                        var sum = left + right;
                        return sum;
                    }
                }
            }
            """,
            "Demo/MathModule.cs");

        using var _ = OverrideSourceMapArtifactFactoryForTest(
            static (_, _, _, _, _) => throw new InvalidOperationException("boom-map"));

        var (_, runResult) = RunGeneratorWithResult(compilation);
        var diagnostics = runResult.Results
            .SelectMany(static result => result.Diagnostics)
            .ToArray();
        var errors = diagnostics
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        var sourceMapWarnings = diagnostics
            .Where(static diagnostic => diagnostic.Id == "JAZORG002")
            .ToArray();

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(static item => item.ToString())));
        Assert.AreEqual(1, sourceMapWarnings.Length, string.Join("\n", diagnostics.Select(static item => item.ToString())));
        StringAssert.Contains(sourceMapWarnings[0].GetMessage(), "boom-map");

        var hintNames = runResult.Results
            .SelectMany(static result => result.GeneratedSources)
            .Select(static source => source.HintName)
            .ToArray();
        CollectionAssert.Contains(hintNames, "Jazor.Generated.ModuleCatalog.g.cs");
        CollectionAssert.AreEquivalent(
            new[] { "Jazor.Generated.ModuleCatalog.g.cs" },
            hintNames);

        var moduleCatalog = GetGeneratedSource(runResult, "Jazor.Generated.ModuleCatalog.g.cs");
        StringAssert.Contains(moduleCatalog, "modules/math.mjs");
    }

    [TestMethod]
    public void RunGenerator_InternalStaticModule_EmitsCatalogModule()
    {
        var compilation = CreateCompilation(
            "InternalModule.Generated",
            """
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Modules
            {
                [ECMAScript.ECMAScriptModule("modules/text-helper")]
                internal static class TextHelper
                {
                    public static string Normalize(string value)
                    {
                        return value.Trim();
                    }
                }
            }
            """,
            "Demo/TextHelper.cs");

        var (_, runResult) = RunGeneratorWithResult(compilation);
        var diagnostics = runResult.Results
            .SelectMany(static result => result.Diagnostics)
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.AreEqual(0, diagnostics.Length, string.Join("\n", diagnostics.Select(static item => item.ToString())));

        var moduleCatalog = GetGeneratedSource(runResult, "Jazor.Generated.ModuleCatalog.g.cs");
        StringAssert.Contains(moduleCatalog, "modules/text-helper.mjs");
        StringAssert.Contains(moduleCatalog, "function Normalize(value)");
    }

    private static Compilation CreateCompilation(string assemblyName, string source, string sourcePath)
        => CSharpCompilation.Create(
            assemblyName: assemblyName,
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions, path: sourcePath)
            ],
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static (Compilation OutputCompilation, GeneratorDriverRunResult RunResult) RunGeneratorWithResult(Compilation compilation)
    {
        ISourceGenerator[] generators =
        [
            new ESGenerator().AsSourceGenerator()
        ];

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators,
            parseOptions: (CSharpParseOptions?)compilation.SyntaxTrees.FirstOrDefault()?.Options);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        var runResult = driver.GetRunResult();
        return (outputCompilation, runResult);
    }

    private static string GetGeneratedSource(GeneratorDriverRunResult runResult, string hintName)
        => runResult.Results
            .SelectMany(static result => result.GeneratedSources)
            .Single(source => source.HintName == hintName)
            .SourceText
            .ToString();

    private static IDisposable OverrideSourceMapArtifactFactoryForTest(
        Func<Node, string, bool, string?, Func<string, string?>?, GeneratedJavaScriptArtifact> factory)
    {
        var field = typeof(ESGenerator).GetField("SourceMapArtifactFactoryOverride", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(field, "Expected ESGenerator.SourceMapArtifactFactoryOverride field.");
        var asyncLocal = field.GetValue(null) as AsyncLocal<Func<Node, string, bool, string?, Func<string, string?>?, GeneratedJavaScriptArtifact>?>;
        Assert.IsNotNull(asyncLocal, "Expected ESGenerator.SourceMapArtifactFactoryOverride to be AsyncLocal.");
        var original = asyncLocal.Value;
        asyncLocal.Value = factory;
        return new RestoreAsyncLocalScope(asyncLocal, original);
    }

    private sealed class RestoreAsyncLocalScope(
        AsyncLocal<Func<Node, string, bool, string?, Func<string, string?>?, GeneratedJavaScriptArtifact>?> asyncLocal,
        Func<Node, string, bool, string?, Func<string, string?>?, GeneratedJavaScriptArtifact>? value) : IDisposable
    {
        private readonly AsyncLocal<Func<Node, string, bool, string?, Func<string, string?>?, GeneratedJavaScriptArtifact>?> _asyncLocal = asyncLocal;
        private readonly Func<Node, string, bool, string?, Func<string, string?>?, GeneratedJavaScriptArtifact>? _value = value;

        public void Dispose()
        {
            _asyncLocal.Value = _value;
        }
    }

    private sealed record SourceContentProbe(
        string? SourceRoot,
        string? AlphaContent,
        string? BetaContent,
        string? AmbiguousFileNameContent,
        string? BlankPathContent);
}
