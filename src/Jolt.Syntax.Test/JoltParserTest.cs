using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jolt.Syntax.Test;

[TestClass]
public sealed class JoltParserTest
{
    [TestMethod]
    public void Parse_PreservesStaticMarkupAndDirectiveSpans()
    {
        const string source = """
            @using System;
            <article><p title="&lt;tag&gt;" disabled empty="">Hi &amp; @@friend</p><Icon /></article>
            @code /* body */ { int Count = 1; }
            """;
        var document = JoltParser.Parse(source, "Demo.jazor");

        Assert.IsEmpty(document.Diagnostics);
        Assert.AreEqual("Demo.jazor", document.Path);
        var import = document.Nodes.OfType<JoltUsing>().Single();
        Assert.AreEqual("@using System;", Slice(source, import.Span));
        Assert.AreEqual("System", Slice(source, import.CodeSpan));
        var code = document.Nodes.OfType<JoltCode>().Single();
        Assert.AreEqual("@code /* body */ { int Count = 1; }", Slice(source, code.Span));
        Assert.AreEqual(" int Count = 1; ", code.Code);

        var article = document.Nodes.OfType<JoltElement>().Single();
        Assert.AreEqual("article", Slice(source, article.NameSpan));
        var paragraph = (JoltElement)article.Children[0];
        Assert.AreEqual("<Icon />", Slice(source, article.Children[1].Span));
        Assert.AreEqual("title", Slice(source, paragraph.Attributes[0].NameSpan));
        Assert.AreEqual("<tag>", ((JoltText)paragraph.Attributes[0].Value.Single()).Text);
        Assert.IsEmpty(paragraph.Attributes[1].Value);
        Assert.AreEqual("", ((JoltText)paragraph.Attributes[2].Value.Single()).Text);
        Assert.AreEqual("Hi & @friend", string.Concat(paragraph.Children.OfType<JoltText>().Select(node => node.Text)));
        Assert.AreEqual("Hi &amp; @@friend", string.Concat(paragraph.Children.Select(node => Slice(source, node.Span))));
    }

    [TestMethod]
    public void Parse_MapsExpressionsEventHandlersAndMixedAttributes()
    {
        const string source = """<button @onclick="() => Run(1)" title="Hi @@team @Name @(Count + 1)!">@Count @(Count + 2)</button>""";
        var document = JoltParser.Parse(source);
        Assert.IsEmpty(document.Diagnostics);
        var button = document.Nodes.OfType<JoltElement>().Single();
        var handler = (JoltExpression)button.Attributes[0].Value.Single();
        Assert.AreEqual("@onclick", Slice(source, button.Attributes[0].NameSpan));
        Assert.AreEqual("() => Run(1)", Slice(source, handler.CodeSpan));

        var title = button.Attributes[1].Value;
        CollectionAssert.AreEqual(new[] { "Hi @team ", "Name", " ", "Count + 1", "!" }, title.Select(Value).ToArray());
        CollectionAssert.AreEqual(new[] { "@Count", "@(Count + 2)" },
            button.Children.OfType<JoltExpression>().Select(node => Slice(source, node.Span)).ToArray());
    }

    [TestMethod]
    public void Parse_BindsNestedBranchesAndForeachToTheirAuthoredBodies()
    {
        const string source = """
            @if (Check(")") && Model is { Count: > 0 }) {
                @foreach (var (a, b) in Pairs) {
                    <span>@a</span>
                }
            } else if (Count == 0) {
                <b>Zero</b>
            } else {
                <i>Other</i>
            }
            """;
        var document = JoltParser.Parse(source);
        Assert.IsEmpty(document.Diagnostics);
        var condition = document.Nodes.OfType<JoltIf>().Single();
        Assert.AreEqual(source, Slice(source, condition.Span));
        Assert.AreEqual("""Check(")") && Model is { Count: > 0 }""", Slice(source, condition.ConditionSpan));
        var loop = condition.Then.OfType<JoltForEach>().Single();
        Assert.AreEqual("var (a, b) in Pairs", Slice(source, loop.HeaderSpan));
        Assert.AreEqual("span", loop.Children.OfType<JoltElement>().Single().Name);
        var alternate = condition.Else.OfType<JoltIf>().Single();
        Assert.AreEqual("b", alternate.Then.OfType<JoltElement>().Single().Name);
        Assert.AreEqual("i", alternate.Else.OfType<JoltElement>().Single().Name);
    }

    [TestMethod]
    [DataRow("<section>")]
    [DataRow("<SwitchRoot ModelValue=\"@(Count >")]
    [DataRow("@code { private int Count = 1;")]
    [DataRow("@if (Count > 0) { <p>Partial")]
    public void Parse_IncompleteEditsRetainEarlierMarkup(string edit)
    {
        var document = JoltParser.Parse("<p>Retained @Count</p>\n" + edit);
        Assert.IsTrue(document.Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        var retained = document.Nodes.OfType<JoltElement>().First();
        Assert.AreEqual("Count", retained.Children.OfType<JoltExpression>().Single().Code);
    }

    [TestMethod]
    public void Parse_AcceptsEachIntermediateEdit()
    {
        const string source = """
            @using System
            <div>
                @if (Count is > 0) {
                    <Widget Name="Hello @Name" @onclick="() => Pick(1)" />
                } else {
                    @foreach (var row in Rows) {
                        <p>@(row.Name)</p>
                    }
                }
            </div>
            @code { private int Count; }
            """;
        for (var end = 0; end <= source.Length; end++)
        {
            var document = JoltParser.Parse(source[..end]);
            Assert.IsTrue(document.Nodes.All(node => node.Span.End <= end), $"Prefix {end}");
        }
    }

    private static string Value(JoltNode node) => node switch
    {
        JoltText text => text.Text,
        JoltExpression expression => expression.Code,
        _ => throw new AssertFailedException()
    };

    [TestMethod]
    public void Parse_PreservesRawTextDuplicateTagsAndGenericExpressions()
    {
        const string source = """
            <style>.a::after { content: "&amp;"; }</style>
            <script>const spelling = "&lt;";</script>
            <p>@(Pick<Dictionary<string, int>>(Nested(1)))</p><p>@(Pick<int>(2))</p>
            <button @onclick="Handler.OnClick" />
            """;
        var document = JoltParser.Parse(source, "Demo.razor");
        Assert.IsEmpty(document.Diagnostics);
        var elements = document.Nodes.OfType<JoltElement>().ToArray();
        Assert.AreEqual(""".a::after { content: "&amp;"; }""", string.Concat(elements[0].Children.OfType<JoltText>().Select(item => item.Text)));
        Assert.AreEqual("""const spelling = "&lt;";""", string.Concat(elements[1].Children.OfType<JoltText>().Select(item => item.Text)));
        CollectionAssert.AreEqual(new[] { "style", "script", "p", "p", "button" }, elements.Select(item => item.Name).ToArray());
        Assert.AreEqual("<p>@(Pick<Dictionary<string, int>>(Nested(1)))</p>", Slice(source, elements[2].Span));
        var nested = elements[2].Children.OfType<JoltExpression>().Single();
        Assert.AreEqual("Pick<Dictionary<string, int>>(Nested(1))", Slice(source, nested.CodeSpan));
        Assert.AreEqual("@(Pick<int>(2))", Slice(source, elements[3].Children.Single().Span));
        Assert.AreEqual("Handler.OnClick", ((JoltExpression)elements[4].Attributes.Single().Value.Single()).Code);
    }

    private static string Slice(string source, TextSpan span) => source.Substring(span.Start, span.Length);
}
