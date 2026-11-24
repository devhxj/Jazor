using Jolt.Compiler;
using Jolt.Syntax;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class AttributesDirectiveTests
{
    [TestMethod]
    public void Parser_PreservesSplatExpressionRanges()
    {
        const string markup = """<button @attributes="Bag()" @attributes="@(OtherBag())" />""";
        var parsed = JoltParser.Parse(markup, "Card.jazor");
        Assert.IsEmpty(parsed.Diagnostics);
        var button = parsed.Nodes.OfType<JoltElement>().Single();
        Assert.IsTrue(button.Attributes.All(attribute => attribute.Kind == JoltAttributeKind.Attributes));
        CollectionAssert.AreEqual(new[] { "Bag()", "OtherBag()" }, button.Attributes
            .Select(attribute => ((JoltExpression)attribute.Value.Single()).CodeSpan)
            .Select(span => markup.Substring(span.Start, span.Length)).ToArray());
    }

    [TestMethod]
    public async Task Compile_SplatsEvaluateOnceInOrderAndComposeWithBindingAndComponents()
    {
        var result = await JoltProjectCompiler.CompileAsync([
            new("Parent.jazor", """
                @using static ECMAScript.Vue
                <button class="@Mark(1)" @attributes="Bag(2)" title="@Mark(3)" @attributes="Bag(4)">Go</button>
                <input @attributes="Bag(5)" @bind="Name" />
                <Child @attributes="Bag(6)" Caption="@Name" />
                @code {
                    private string Name = "sample";
                    private string Mark(int marker) => marker.ToString();
                    private VueDictionary<string> Bag(int marker) => new() { ["title"] = Mark(marker) };
                }
                """),
            new("Child.jazor", """
                <b>@Caption</b>
                @code { [Parameter, EditorRequired] public string Caption { get; set; } = ""; }
                """),
        ]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single(module => module.Name == "Parent").Content;
        StringAssert.Contains(js, "mergeProps as $joltMergeProps");
        StringAssert.Contains(js, "vModelText as $joltModelText");
        Assert.DoesNotContain("@attributes", js);
        var cursor = js.IndexOf("$joltMergeProps(", StringComparison.Ordinal);
        Assert.IsTrue(cursor >= 0, js);
        for (int marker = 1; marker <= 6; marker++)
        {
            string call = $"this.{(marker is 1 or 3 ? "Mark" : "Bag")}({marker})";
            var next = js.IndexOf(call, cursor, StringComparison.Ordinal);
            Assert.IsTrue(next > cursor, js);
            Assert.AreEqual(next, js.LastIndexOf(call, StringComparison.Ordinal), js);
            cursor = next;
        }
    }

    [TestMethod]
    public async Task Compile_ComponentVNodeAttributesUseVueValueTypesAndPreserveDeclaredProps()
    {
        var child = new JoltSource("Tile.jazor", "<article />");
        var result = await JoltProjectCompiler.CompileAsync([
            child,
            new("Page.jazor", """<Tile class="highlight" style="padding:0" />"""),
        ]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single(module => module.Name == "Page").Content;
        StringAssert.Contains(js, "\"class\": \"highlight\"");
        StringAssert.Contains(js, "\"style\": \"padding:0\"");

        foreach (var (markup, component, diagnostic) in new[]
        {
            ("""<Tile style="@(42)" />""", child, "CS1503"),
            ("""<Tile clas="typo" />""", child, "JOLT1004"),
            ("""<Tile class="highlight" />""", new JoltSource("Tile.jazor",
                "<article />\n@code { [Parameter] public bool Class { get; set; } }"), "CS1503"),
        })
        {
            var analysis = JoltProjectCompiler.Analyze([component, new("Page.jazor", markup)]);
            Assert.IsTrue(analysis.Diagnostics.Any(item => item.Id == diagnostic),
                markup + Environment.NewLine + string.Join(Environment.NewLine, analysis.Diagnostics));
        }
    }

    [TestMethod]
    public async Task Compile_AcceptsForwardedVueAttributeBag()
    {
        var result = await JoltProjectCompiler.CompileAsync([
            new("Item.jazor", """
                @using static ECMAScript.Vue
                <div @attributes="Bag" />
                @code { [Parameter] public VueAttributeBag Bag { get; set; } = null!; }
                """),
        ]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules.Single().Content, "$joltMergeProps(");
    }

    [TestMethod]
    [DataRow("<div @attributes />", "JOLT1030")]
    [DataRow("<div @attributes='prefix @Bag' />", "JOLT1030")]
    [DataRow("<div @attributes='new Dictionary<string, string>()' />", "CS1503")]
    [DataRow("<slot @attributes='Bag' />", "JOLT1022")]
    [DataRow("<Child @attributes='Bag' />", "JOLT1005")]
    public void Analyze_InvalidSplat_ReportsAtTheAuthorLocation(string markup, string expected)
    {
        var result = JoltProjectCompiler.Analyze([
            new("Item.jazor", markup + """
                @code {
                    private ECMAScript.Vue.VueDictionary<string> Bag = new() { ["Name"] = "sample" };
                }
                """),
            new("Child.jazor", """
                @code { [Parameter, EditorRequired] public string Name { get; set; } = ""; }
                """),
        ]);
        Assert.IsTrue(result.Diagnostics.Any(diagnostic => diagnostic.Id == expected &&
            diagnostic.Location.GetLineSpan().Path == "Item.jazor" && diagnostic.Location.SourceSpan.Length > 0),
            string.Join(Environment.NewLine, result.Diagnostics));
    }
}
