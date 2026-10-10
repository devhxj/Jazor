using Acornima;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class AstConverterPartialCompilationTests
{
    [TestMethod]
    public async Task Convert_PartialCodeBehindIsStableAndUsesNewCompilation()
    {
        foreach (var seed in new[] { 17, 29 })
        {
            var root = CSharpSyntaxTree.ParseText(
                "public static partial class Module { public static int First() => Read(); public static int Second() => Read() + 1; }",
                TestMetadataReferences.PreviewParseOptions, "module.cs");
            var codeBehind = CSharpSyntaxTree.ParseText(
                $"public static partial class Module {{ private const int Seed = {seed}; private static int Read() => Seed; }}",
                TestMetadataReferences.PreviewParseOptions, "module.partial.cs");
            var compilation = CSharpCompilation.Create("PartialModule", [root, codeBehind], TestMetadataReferences.Net11,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            Assert.IsEmpty(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray());
            var symbol = compilation.GetTypeByMetadataName("Module")!;
            var model = compilation.GetSemanticModel(root);
            var first = (await new AstConverter(symbol, model).Convert())!.ToKnRECMAScript();
            var repeated = (await new AstConverter(symbol, model).Convert())!.ToKnRECMAScript();
            Assert.AreEqual(first, repeated);
            StringAssert.Contains(first, $"return {seed};");
            StringAssert.Contains(first, "export function First()");
            StringAssert.Contains(first, "export function Second()");
            _ = new Parser().ParseModule(first);
        }
    }
}
