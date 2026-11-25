using Basic.Reference.Assemblies;
using ECMAScript;
using Jazor.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jazor.EmitTest;

[TestClass]
public sealed class JavaScriptNameAnalyzerTests
{
    [TestMethod]
    public async Task Analyze_UnusedBindings_ReportsEachEmptyAliasAtItsAttribute()
    {
        const string source = """
            using System.ComponentModel;
            using ECMAScript;
            using Alias = ECMAScript.ECMAScriptNameAttribute;

            [Boundary]
            public static class Bindings
            {
                private const string Empty = "";
                [Alias(Empty), Description("@#" + Empty)] public class Invalid;
                [Alias("  ")] public static void InvalidMethod() { }
                [Alias(null)] public class Missing;
                [Description(""), Alias("ok")] public sealed record Data(int Value);
                [Description("@#wire")] public static int Valid { get; }
            }
            [String] enum Wire
            {
                [Alias("")] Empty,
                [Description("@#")] Invalid,
                [Alias(null)] Missing,
            }
            enum Numeric { [Alias("")] Invalid }
            [Other.ECMAScriptName("")] class Unrelated;
            namespace Other
            {
                public sealed class ECMAScriptNameAttribute(string value) : System.Attribute;
            }
            """;
        var tree = CSharpSyntaxTree.ParseText(source, path: "Bindings.cs");
        var compilation = CSharpCompilation.Create("Bindings", [tree],
            Net110.References.All.Cast<MetadataReference>().Concat([
                MetadataReference.CreateFromFile(typeof(BoundaryAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScriptNameAttribute).Assembly.Location)
            ]), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var csharpErrors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.IsEmpty(csharpErrors, string.Join(Environment.NewLine, csharpErrors.Select(d => d.ToString())));

        var diagnostics = await compilation.WithAnalyzers([new JavaScriptNameAnalyzer()]).GetAnalyzerDiagnosticsAsync();
        CollectionAssert.AreEqual(
            new[] { "Alias(Empty)", "Description(\"@#\" + Empty)", "Alias(\"  \")", "Alias(null)",
                "Description(\"@#\")", "Alias(null)", "Alias(\"\")" },
            diagnostics.OrderBy(d => d.Location.SourceSpan.Start).Select(d =>
            {
                Assert.AreEqual("JAZOR009", d.Id);
                Assert.AreEqual(DiagnosticSeverity.Error, d.Severity);
                return source.Substring(d.Location.SourceSpan.Start, d.Location.SourceSpan.Length);
            }).ToArray());
    }
}
