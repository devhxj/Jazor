using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class VueUnwrappedRecordTests
{
    private const string Source = """
        #nullable enable
        using ECMAScript;

        public static class TestModule
        {
            public sealed record State(
                [property: ECMAScriptName("form.count")] Vue.IVueRef<double> Count,
                [property: ECMAScriptName("title")] Vue.IVueRef<string> Title,
                [property: ECMAScriptName("enabled")] Vue.IVueRef<bool> Enabled,
                Vue.IVueRef<double>? MissingRef,
                Vue.IVueRef<string?> NullableTitle,
                Vue.IVueRef<int> OtherNumber,
                Vue.VueComputedRef<double> ReadonlyCount,
                [property: ECMAScriptName("form.child")] Nested Child,
                [property: ECMAScriptOptional] Nested MissingChild,
                Nested? NullableChild);

            public sealed record Nested([property: ECMAScriptName("nested.count")] Vue.IVueRef<double> Count);

            public static void Run(Vue.VueUnwrappedRecord<State> model)
            {
                Vue.IVueRef<double> count = Vue.ToRef(model, row => row.Count);
                Vue.IVueRef<string> title = Vue.ToRef(model, row => row.Title);
                Vue.IVueRef<bool> enabled = Vue.ToRef(model, row => row.Enabled);
                count.Value = 0d;
                title.Value = "";
                enabled.Value = false;
            }
        }
        """;

    [TestMethod]
    public async Task Convert_UnwrappedRecordToRef_UsesVueAndEmitsLiteralKeys()
    {
        var script = await Convert(CreateCompilation(Source));
        StringAssert.Contains(script, "from \"vue\"");
        StringAssert.Contains(script, """toRef(model, "form.count")""");
        StringAssert.Contains(script, """toRef(model, "title")""");
        StringAssert.Contains(script, """toRef(model, "enabled")""");
        StringAssert.Contains(script, "count.value = 0");
        StringAssert.Contains(script, "enabled.value = false");
    }

    [TestMethod]
    public async Task Convert_UnwrappedRecordSelect_UsesOriginalNestedObjectAndEvaluatesReceiverOnce()
    {
        var source = Source.Replace("public static void Run(Vue.VueUnwrappedRecord<State> model)",
            "public static void Run(Vue.VueUnwrappedRecord<State> model, System.Func<Vue.VueUnwrappedRecord<State>> getModel)")
            .Replace("count.Value = 0d;", "Vue.ToRef(getModel().Select(row => row.Child), row => row.Count).Value = 0d;");
        var script = await Convert(CreateCompilation(source));
        StringAssert.Contains(script, """toRef(getModel()["form.child"], "nested.count").value = 0""");
        Assert.AreEqual(1, System.Text.RegularExpressions.Regex.Matches(script, @"\bgetModel\(\)").Count);
    }

    [TestMethod]
    [DataRow("MissingChild", "Read optional property 'MissingChild' directly")]
    [DataRow("NullableChild", "requires a non-null structural record")]
    [DataRow("Count", "requires a non-null structural record")]
    public async Task Convert_UnwrappedRecordSelect_RejectsUnsupportedNestedObjects(string member, string diagnostic)
    {
        var source = Source.Replace("count.Value = 0d;", $"var child = model.Select(row => row.{member});");
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(() => Convert(CreateCompilation(source)));
        StringAssert.Contains(exception.Message, diagnostic);
    }

    [TestMethod]
    [DataRow("row.Child.Count", "selecting exactly one structural record data property")]
    [DataRow("row.MissingRef", "the call expects")]
    [DataRow("row.NullableTitle", "the call expects")]
    public async Task Convert_UnwrappedRecordToRef_RejectsNestedOrNullableKeys(string selector, string diagnostic)
    {
        // Discarding the result lets C# bind the intended overload from the selector alone.
        var source = Source.Replace("Vue.IVueRef<double> count = Vue.ToRef(model, row => row.Count);",
            $"var count = Vue.ToRef(model, row => {selector});").Replace("count.Value = 0d;", "");
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(() => Convert(CreateCompilation(source)));
        StringAssert.Contains(exception.Message, diagnostic);
    }

    [TestMethod]
    [DataRow("row.OtherNumber")]
    [DataRow("row.ReadonlyCount")]
    [DataRow("row.Count.Value")]
    public void Bind_UnwrappedRecordToRef_RejectsUnsupportedFieldTypes(string selector)
    {
        var source = Source.Replace("row => row.Count", $"row => {selector}");
        var diagnostics = CreateCompilation(source).GetDiagnostics();
        Assert.IsTrue(diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error &&
            diagnostic.Location is { IsInSource: true, SourceTree: { } tree } &&
            tree.GetText().Lines.GetLineFromPosition(diagnostic.Location.SourceSpan.Start).ToString().Contains(selector)),
            string.Join(Environment.NewLine, diagnostics));
    }

    private static CSharpCompilation CreateCompilation(string source) =>
        CSharpCompilation.Create("UnwrappedRecord", [CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions)],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static async Task<string> Convert(CSharpCompilation compilation)
    {
        var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        var converter = new AstConverter(compilation.GetTypeByMetadataName("TestModule")!,
            compilation.GetSemanticModel(compilation.SyntaxTrees.Single()));
        return (await converter.Convert())!.ToKnRECMAScript();
    }
}
