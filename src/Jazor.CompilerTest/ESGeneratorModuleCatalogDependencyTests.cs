using System.Collections.Immutable;
using Jazor.ComplierTest;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class ESGeneratorModuleCatalogDependencyTests
{
    [TestMethod]
    public void RunGenerator_SeparatesReferencedModuleCatalogDependenciesFromJsResourceImports()
    {
        var catalogLibrary = EmitGeneratedLibraryReference(
            "Sample.Features",
            """
            using ECMAScript;

            namespace Sample.Features;

            [ECMAScriptModule("features/greeter.mjs")]
            public static class GreeterModule
            {
                public static int Greet(int value) => value + 1;
            }
            """);
        var resourceLibrary = EmitLibraryReference(
            "Sample.Resources",
            """
            using ECMAScript;

            namespace Sample.Resources;

            [ECMAScriptModule("resources/widget.mjs")]
            public static class WidgetModule
            {
                public static int Double(int value) => value * 2;
            }
            """);
        var hostCompilation = CreateCompilation(
            "Sample.Host",
            """
            using ECMAScript;
            using Sample.Features;
            using Sample.Resources;

            namespace Sample.Host;

            [ECMAScriptModule("host/app.mjs")]
            public static class AppModule
            {
                public static int Boot() => GreeterModule.Greet(WidgetModule.Double(2));
            }
            """,
            catalogLibrary,
            resourceLibrary);

        AssertNoCompilationErrors(hostCompilation, "host input");

        var (outputCompilation, runResult) = RunGenerator(hostCompilation);
        AssertNoGeneratorErrors(runResult, "host generation");
        AssertNoCompilationErrors(outputCompilation, "host generated output");

        var catalog = GetGeneratedSource(runResult);
        StringAssert.Contains(catalog, "relativePath: \"host/app.mjs\"");
        StringAssert.Contains(catalog, "packageImports: new string[] { \"resources/widget.mjs\" },");
        StringAssert.Contains(catalog, "dependencies: new string[] { \"features/greeter.mjs\" },");
        Assert.IsFalse(
            catalog.Contains("packageImports: new string[] { \"features/greeter.mjs\"", StringComparison.Ordinal),
            catalog);
    }

    private static MetadataReference EmitGeneratedLibraryReference(string assemblyName, string source)
    {
        var inputCompilation = CreateCompilation(assemblyName, source);
        AssertNoCompilationErrors(inputCompilation, assemblyName + " input");

        var (outputCompilation, runResult) = RunGenerator(inputCompilation);
        AssertNoGeneratorErrors(runResult, assemblyName + " generation");
        AssertNoCompilationErrors(outputCompilation, assemblyName + " generated output");
        return EmitReference(outputCompilation, assemblyName);
    }

    private static MetadataReference EmitLibraryReference(string assemblyName, string source)
    {
        var compilation = CreateCompilation(assemblyName, source);
        AssertNoCompilationErrors(compilation, assemblyName + " input");
        return EmitReference(compilation, assemblyName);
    }

    private static MetadataReference EmitReference(Compilation compilation, string description)
    {
        using var image = new MemoryStream();
        var result = compilation.Emit(image);
        Assert.IsTrue(
            result.Success,
            description + Environment.NewLine + string.Join(
                Environment.NewLine,
                result.Diagnostics.Select(static diagnostic => diagnostic.ToString())));
        return MetadataReference.CreateFromImage(ImmutableArray.CreateRange(image.ToArray()));
    }

    private static CSharpCompilation CreateCompilation(
        string assemblyName,
        string source,
        params MetadataReference[] additionalReferences)
    {
        var references = new List<MetadataReference>(TestMetadataReferences.Net11)
        {
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location)
        };
        references.AddRange(additionalReferences);

        return CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions, assemblyName + ".cs")],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static (Compilation OutputCompilation, GeneratorDriverRunResult RunResult) RunGenerator(
        CSharpCompilation compilation)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new ESGenerator().AsSourceGenerator()],
            parseOptions: (CSharpParseOptions?)compilation.SyntaxTrees.First().Options);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        return (outputCompilation, driver.GetRunResult());
    }

    private static string GetGeneratedSource(GeneratorDriverRunResult runResult)
        => runResult.Results
            .SelectMany(static result => result.GeneratedSources)
            .Single(source => source.HintName == "Jazor.Generated.ModuleCatalog.g.cs")
            .SourceText
            .ToString();

    private static void AssertNoGeneratorErrors(GeneratorDriverRunResult runResult, string stage)
    {
        var errors = runResult.Results
            .SelectMany(static result => result.Diagnostics)
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, stage + Environment.NewLine + string.Join(
            Environment.NewLine,
            errors.Select(static diagnostic => diagnostic.ToString())));
    }

    private static void AssertNoCompilationErrors(Compilation compilation, string stage)
    {
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, stage + Environment.NewLine + string.Join(
            Environment.NewLine,
            errors.Select(static diagnostic => diagnostic.ToString())));
    }
}
