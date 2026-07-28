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
    public void RunGenerator_StaticModule_EmitsDedicatedSourceMapCatalogWithoutChangingModuleCatalogShape()
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
        CollectionAssert.Contains(hintNames, "Jazor.Generated.ModuleSourceMapCatalog.g.cs");

        var moduleCatalog = GetGeneratedSource(runResult, "Jazor.Generated.ModuleCatalog.g.cs");
        StringAssert.Contains(moduleCatalog, "internal static partial class ModuleCatalog");
        Assert.AreEqual(-1, moduleCatalog.IndexOf("SourceMapRelativePath", StringComparison.Ordinal));
        Assert.AreEqual(-1, moduleCatalog.IndexOf("SourceMapContent", StringComparison.Ordinal));
        Assert.AreEqual(-1, moduleCatalog.IndexOf("MapHash", StringComparison.Ordinal));

        var sourceMapCatalog = GetGeneratedSource(runResult, "Jazor.Generated.ModuleSourceMapCatalog.g.cs");
        StringAssert.Contains(sourceMapCatalog, "internal static partial class ModuleSourceMapCatalog");
        StringAssert.Contains(sourceMapCatalog, "sourceMapRelativePath:");
        StringAssert.Contains(sourceMapCatalog, "modules/math.mjs.map");
        StringAssert.Contains(sourceMapCatalog, "Demo/MathModule.cs");
    }

    [TestMethod]
    public void RunGenerator_StaticModule_WhenSourceMapGenerationFails_ReportsWarningAndFallsBackToJsOnlyCatalog()
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
        CollectionAssert.DoesNotContain(hintNames, "Jazor.Generated.ModuleSourceMapCatalog.g.cs");

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
        StringAssert.Contains(moduleCatalog, "function normalize(value)");
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
}
