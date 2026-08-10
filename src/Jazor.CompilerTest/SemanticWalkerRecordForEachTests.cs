using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerRecordForEachTests
{
    [TestMethod]
    public void VisitForEachLoop_NestedRecordDeconstruction_UsesStructuralObjectPatterns()
    {
        var block = GetBlockOperation(
            """
            using System.Collections.Generic;

            record VersionInfo(int Major, int Minor);
            record Release(string Name, VersionInfo Version);

            class TestClass
            {
                void TestMethod(IEnumerable<Release> releases)
                {
                    foreach (var (name, (major, minor)) in releases)
                    {
                        System.Console.WriteLine(name);
                        System.Console.WriteLine(major + minor);
                    }
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(
            script,
            "for (let { Name: name, Version: { Major: major, Minor: minor } } of releases)",
            StringComparison.Ordinal);
        StringAssert.Contains(script, "console.log(name);", StringComparison.Ordinal);
        StringAssert.Contains(script, "console.log(major + minor);", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerRecordForEachTests",
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(
            compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
