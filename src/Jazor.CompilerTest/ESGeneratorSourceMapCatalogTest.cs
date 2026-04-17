using System.Reflection;
using System.Linq;
using Acornima.Ast;
using Basic.Reference.Assemblies;
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
        StringAssert.Contains(moduleCatalog, "public static partial class ModuleCatalog");
        Assert.AreEqual(-1, moduleCatalog.IndexOf("SourceMapRelativePath", StringComparison.Ordinal));
        Assert.AreEqual(-1, moduleCatalog.IndexOf("SourceMapContent", StringComparison.Ordinal));
        Assert.AreEqual(-1, moduleCatalog.IndexOf("MapHash", StringComparison.Ordinal));

        var sourceMapCatalog = GetGeneratedSource(runResult, "Jazor.Generated.ModuleSourceMapCatalog.g.cs");
        StringAssert.Contains(sourceMapCatalog, "public static partial class ModuleSourceMapCatalog");
        StringAssert.Contains(sourceMapCatalog, "sourceMapRelativePath:");
        StringAssert.Contains(sourceMapCatalog, "modules/math.mjs.map");
        StringAssert.Contains(sourceMapCatalog, "Demo/MathModule.cs");
    }

    [TestMethod]
    [DoNotParallelize]
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

    private static Compilation CreateCompilation(string assemblyName, string source, string sourcePath)
        => CSharpCompilation.Create(
            assemblyName: assemblyName,
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(source, path: sourcePath)
            ],
            references: Net100.References.All.Cast<MetadataReference>(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static (Compilation OutputCompilation, GeneratorDriverRunResult RunResult) RunGeneratorWithResult(Compilation compilation)
    {
        ISourceGenerator[] generators =
        [
            new ESGenerator().AsSourceGenerator()
        ];

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generators);
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
        var field = typeof(ESGenerator).GetField("SourceMapArtifactFactory", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(field, "Expected ESGenerator.SourceMapArtifactFactory field.");
        var original = field.GetValue(null);
        field.SetValue(null, factory);
        return new RestoreStaticFieldScope(field, original);
    }

    private sealed class RestoreStaticFieldScope(FieldInfo field, object? value) : IDisposable
    {
        private readonly FieldInfo _field = field;
        private readonly object? _value = value;

        public void Dispose()
        {
            _field.SetValue(null, _value);
        }
    }
}
