using Jazor.Npm;
using Microsoft.CodeAnalysis.CSharp;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class NpmDocumentationTests
{
    [TestMethod]
    public void GenerateSource_EmitsEscapedDocumentationOnlyForProjectedParameters()
    {
        var definition = new NpmDefinition("index.d.ts", 0, 1)
        {
            Documentation = new(
                Summary: "Check <value> & \"quotes\".\nNext line.",
                Parameters: new Dictionary<string, string> { ["label"] = "The label.", ["hidden"] = "Omitted." },
                TypeParameters: new Dictionary<string, string> { ["T"] = "A type.", ["Unused"] = "Omitted." },
                Returns: "A value.", Examples: ["read(x);"], Deprecated: "Use another call."),
        };
        var signature = new NpmValueSignature(
            [new("label", new("string"), false), new("hidden", new("void"), false, FixedUndefined: true)],
            new("typeParameter", Name: "T"), definition, TypeParameters: ["T"]);
        var export = new NpmValueExport("doc-package", "read", "doc-package", "1.0.0", "index.mjs",
            "index.d.ts", definition, [signature], null);

        var source = NpmBindings.GenerateSource([], [export]);
        var parsed = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(documentationMode: Microsoft.CodeAnalysis.DocumentationMode.Diagnose));
        Assert.IsEmpty(parsed.GetDiagnostics());
        StringAssert.Contains(source, "Check &lt;value&gt; &amp; &quot;quotes&quot;.\n/// Next line.");
        StringAssert.Contains(source, """<param name="label">The label.</param>""");
        StringAssert.Contains(source, """<typeparam name="T">A type.</typeparam>""");
        StringAssert.Contains(source, "<returns>A value.</returns>");
        StringAssert.Contains(source, "<example>read(x);</example>");
        StringAssert.Contains(source, "<remarks>Deprecated: Use another call.</remarks>");
        Assert.DoesNotContain("Omitted.", source);
        Assert.AreEqual(definition, definition with { Documentation = new("Changed") });
    }

    [TestMethod]
    public void GenerateSource_OverloadDoesNotInheritSiblingDocumentation()
    {
        var first = new NpmDefinition("index.d.ts", 0, 1) { Documentation = new("First only.") };
        var second = new NpmDefinition("index.d.ts", 20, 1);
        NpmValueSignature[] signatures = [
            new([], new("void"), first),
            new([new("label", new("string"), false)], new("void"), second),
        ];
        var export = new NpmValueExport("doc-package", "read", "doc-package", "1.0.0", "index.mjs",
            "index.d.ts", first, signatures, null);
        var source = NpmBindings.GenerateSource([], [export]);
        Assert.AreEqual(1, source.Split("First only.", StringSplitOptions.None).Length - 1);

        var owner = first with { DocumentationFromCallableOwner = true, Documentation = new("Callable owner.") };
        source = NpmBindings.GenerateSource([], [export with { Definition = owner }]);
        StringAssert.Contains(source, "Callable owner.");
    }
}
