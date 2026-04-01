using System.IO;
using System.Linq;
using Basic.Reference.Assemblies;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class ESGeneratorTests
{
    [TestMethod]
    public void GenerateCatalog_WithReferencedModuleAssembly_DoesNotProduceTypeConflictWarnings()
    {
        var upstreamCompilation = CreateCompilation(
            "Upstream.Modules",
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

            [ECMAScript.ECMAScriptModule("shared/upstream.mjs")]
            public static class UpstreamModule
            {
                public static int Value = 1;
            }
            """);

        var upstreamOutput = RunGenerator(upstreamCompilation, out _);
        using var upstreamImage = new MemoryStream();
        var upstreamEmit = upstreamOutput.Emit(upstreamImage);
        Assert.IsTrue(
            upstreamEmit.Success,
            string.Join("\n", upstreamEmit.Diagnostics.Select(static x => x.ToString())));

        var downstreamCompilation = CreateCompilation(
            "Downstream.Modules",
            """
            [ECMAScript.ECMAScriptModule("features/downstream.mjs")]
            public static class DownstreamModule
            {
                public static int Value = 2;
            }
            """,
            MetadataReference.CreateFromImage(upstreamImage.ToArray()));

        var downstreamOutput = RunGenerator(downstreamCompilation, out var generatedSource);
        var conflicts = downstreamOutput.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Id == "CS0436")
            .ToArray();

        Assert.AreEqual(0, conflicts.Length, string.Join("\n", conflicts.Select(static x => x.ToString())));
        StringAssert.Contains(generatedSource, "public static global::System.Collections.IEnumerable GetModules()");
        StringAssert.Contains(generatedSource, "private sealed class GeneratedModule");
        Assert.IsFalse(generatedSource.Contains("public sealed class GeneratedModule", System.StringComparison.Ordinal));
    }

    private static Compilation CreateCompilation(string assemblyName, string source, params MetadataReference[] extraReferences)
    {
        var references = Net100.References.All
            .Cast<MetadataReference>()
            .ToList();
        references.AddRange(extraReferences);

        return CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static Compilation RunGenerator(Compilation compilation, out string generatedSource)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ESGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        var runResult = driver.GetRunResult();
        generatedSource = runResult.Results
            .SelectMany(static result => result.GeneratedSources)
            .Single(static source => source.HintName == "Jazor.Generated.ModuleCatalog.g.cs")
            .SourceText
            .ToString();

        return outputCompilation;
    }
}
