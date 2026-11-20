using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jolt.Syntax.Test;

[TestClass]
public sealed class JoltParserTest
{
    [TestMethod]
    public void Parse_PreservesInlineTemplateMarkupAndAuthorCodeExtent()
    {
        const string source = """
            @code { private RenderFragment<string> Label = value => @<strong>@value</strong>; }
            @{ RenderFragment local = () => @<em>Local</em>; }
            """;
        var document = JoltParser.Parse(source);
        Assert.IsEmpty(document.Diagnostics);
        var templates = document.InlineTemplates;
        CollectionAssert.AreEqual(new[] { "@<strong>@value</strong>", "@<em>Local</em>" },
            templates.Select(template => Slice(source, template.Span)).ToArray());
        Assert.IsTrue(document.Nodes.OfType<JoltCode>().Single().CodeSpan.Contains(templates[0].Span));
        Assert.IsTrue(document.Nodes.OfType<JoltStatement>().Single().CodeSpan.Contains(templates[1].Span));
        for (var end = 0; end <= source.Length; end++)
        {
            var incomplete = JoltParser.Parse(source[..end]);
            Assert.IsTrue(incomplete.InlineTemplates.All(template => template.Span.End <= end), $"Prefix {end}");
        }
    }

    [TestMethod]
    public void Parse_ControlFlow()
    {
        const string source = """
            @{ var count = 3; }
            @for (var i = 0; i < count; i++) {
                var label = i.ToString();
                switch (i) {
                    case > 0 when (i != 2 ? true : false):
                    case 0:
                        if (i > 0) {
                            <p>@label</p>
                        }
                        break;
                    default:
                        continue;
                }
            }
            @switch (count) {
                case 3: <p>Three</p> break;
                default: break;
            }
            """;
        var document = JoltParser.Parse(source);
        Assert.IsEmpty(document.Diagnostics);
        var declaration = document.Nodes.OfType<JoltStatement>().Single();
        Assert.AreEqual("var count = 3;", Slice(source, declaration.CodeSpan));
        var loop = document.Nodes.OfType<JoltFor>().Single();
        Assert.AreEqual("var i = 0; i < count; i++", Slice(source, loop.HeaderSpan));
        Assert.IsTrue(Slice(source, loop.Span).StartsWith("@for", StringComparison.Ordinal));
        Assert.AreEqual("var label = i.ToString();", loop.Children.OfType<JoltStatement>().Single().Code);

        var selection = loop.Children.OfType<JoltSwitch>().Single();
        Assert.AreEqual("i", Slice(source, selection.ExpressionSpan));
        CollectionAssert.AreEqual(new[] { "case > 0 when (i != 2 ? true : false):", "case 0:" },
            selection.Sections[0].Labels.Select(label => Slice(source, label.CodeSpan)).ToArray());
        Assert.AreEqual("break;", selection.Sections[0].Children.OfType<JoltStatement>().Single().Code);
        Assert.AreEqual("default:", selection.Sections[1].Labels.Single().Code);
        Assert.AreEqual("continue;", selection.Sections[1].Children.OfType<JoltStatement>().Single().Code);
        var topLevelSelection = document.Nodes.OfType<JoltSwitch>().Single();
        Assert.AreEqual("case 3:", topLevelSelection.Sections[0].Labels.Single().Code);
        Assert.AreEqual("label", JoltTree.Elements(loop.Children).Single().Children.OfType<JoltExpression>().Single().Code);

        for (var end = 0; end <= source.Length; end++)
        {
            var incomplete = JoltParser.Parse(source[..end]);
            Assert.IsTrue(JoltTree.Descendants(incomplete.Nodes).All(node => node.Span.End <= end), $"Prefix {end}");
        }
    }

    [TestMethod]
    public void Parse_PreservesStatementOrderAndLexicalBlocks()
    {
        const string source = """
            @foreach (var row in Rows) {
                var label = row.Name;
                var title = label.ToUpper();
                {
                    var caption = title + "!";
                    <p>@caption</p>
                }
                <span>@title</span>
            }
            """;
        var document = JoltParser.Parse(source);
        Assert.IsEmpty(document.Diagnostics, string.Join("\n", document.Diagnostics));
        var loop = document.Nodes.OfType<JoltForEach>().Single();
        var declarations = loop.Children.OfType<JoltStatement>().ToArray();
        CollectionAssert.AreEqual(new[] { "var label = row.Name;", "var title = label.ToUpper();" },
            declarations.Select(statement => Slice(source, statement.CodeSpan)).ToArray());
        var block = loop.Children.OfType<JoltBlock>().Single();
        Assert.AreEqual("var caption = title + \"!\";", block.Children.OfType<JoltStatement>().Single().Code);
        CollectionAssert.AreEqual(new[] { "p", "span" }, JoltTree.Elements(document.Nodes).Select(element => element.Name).ToArray());
    }
    [TestMethod]
    public void Parse_MapsDirectiveKindsAndAuthoredSpans()
    {
        const string source = """
            <input type="text" @bind=" name " @bind:event="oninput" />
            <li @key="row.Id" />
            <button @onclick="Handle" @onclick:preventDefault @onclick:stopPropagation="enabled" />
            <CollapsibleRoot @bind-Open="@(state.Open)" @onupdate:open="Set" />
            <input @bind:get="Name" @bind:set="SetName" />
            <Editor @bind-Value:after="Saved" @bind-Open:get="Open" @bind-Open:set="SetOpen" />
            """;
        var document = JoltParser.Parse(source, "Form.jazor");
        Assert.IsEmpty(document.Diagnostics);
        var elements = document.Nodes.OfType<JoltElement>().ToArray();

        var input = elements[0];
        Assert.AreEqual(JoltAttributeKind.Markup, input.Attributes[0].Kind);
        Assert.AreEqual("text", ((JoltText)input.Attributes[0].Value.Single()).Text);
        AssertDirective(input.Attributes[1], JoltAttributeKind.Bind, "@bind", "name");
        Assert.AreEqual(JoltAttributeKind.BindEvent, input.Attributes[2].Kind);
        Assert.AreEqual("@bind:event", Slice(source, input.Attributes[2].NameSpan));
        Assert.AreEqual("oninput", ((JoltText)input.Attributes[2].Value.Single()).Text);

        AssertDirective(elements[1].Attributes.Single(), JoltAttributeKind.Key, "@key", "row.Id");
        var button = elements[2];
        AssertDirective(button.Attributes[0], JoltAttributeKind.Event, "@onclick", "Handle", "click");
        Assert.AreEqual(JoltAttributeKind.EventModifier, button.Attributes[1].Kind);
        Assert.AreEqual("@onclick:preventDefault", Slice(source, button.Attributes[1].NameSpan));
        Assert.AreEqual("click", button.Attributes[1].Target);
        Assert.AreEqual("preventDefault", button.Attributes[1].Modifier);
        Assert.IsEmpty(button.Attributes[1].Value);
        AssertDirective(button.Attributes[2], JoltAttributeKind.EventModifier, "@onclick:stopPropagation", "enabled", "click");
        Assert.AreEqual("stopPropagation", button.Attributes[2].Modifier);

        AssertDirective(elements[3].Attributes[0], JoltAttributeKind.Bind, "@bind-Open", "state.Open", "Open");
        AssertDirective(elements[3].Attributes[1], JoltAttributeKind.Event, "@onupdate:open", "Set", "update:open");
        AssertDirective(elements[4].Attributes[0], JoltAttributeKind.Bind, "@bind:get", "Name");
        AssertDirective(elements[4].Attributes[1], JoltAttributeKind.Bind, "@bind:set", "SetName");
        AssertDirective(elements[5].Attributes[0], JoltAttributeKind.Bind, "@bind-Value:after", "Saved", "Value");
        AssertDirective(elements[5].Attributes[1], JoltAttributeKind.Bind, "@bind-Open:get", "Open", "Open");
        AssertDirective(elements[5].Attributes[2], JoltAttributeKind.Bind, "@bind-Open:set", "SetOpen", "Open");
        CollectionAssert.AreEqual(new[] { "get", "set", "after", "get", "set" },
            elements.Skip(4).SelectMany(element => element.Attributes).Select(attribute => attribute.Modifier).ToArray());

        void AssertDirective(JoltAttribute attribute, JoltAttributeKind kind, string name, string code, string? target = null)
        {
            Assert.AreEqual(kind, attribute.Kind);
            Assert.AreEqual(name, attribute.Name);
            Assert.AreEqual(name, Slice(source, attribute.NameSpan));
            Assert.AreEqual(target, attribute.Target);
            var expression = (JoltExpression)attribute.Value.Single();
            Assert.AreEqual(code, expression.Code);
            Assert.AreEqual(code, Slice(source, expression.CodeSpan));
        }
    }

    [TestMethod]
    public void Parse_ReportsUnsupportedDirectivesAtTheirNames()
    {
        const string source = """
            @page "/demo"
            @rendermode InteractiveAuto
            @inject IService Service
            @namespace Demo
            @attribute [Demo]
            @implements IFoo
            @inherits Base
            @layout Layout
            @preservewhitespace true
            @using System
            @typeparam T
            @code { private string value = "@page @ref"; }
            @* @implements IFoo *@
            <p title="@@ref">@@page</p>
            <input @ref="field" @attributes="more" @bind:culture="culture" />
            """;
        var document = JoltParser.Parse(source, "Form.jazor");
        CollectionAssert.AreEqual(
            new[] { "@page", "@rendermode", "@inject", "@namespace", "@attribute", "@implements", "@inherits",
                "@layout", "@preservewhitespace", "@bind:culture" },
            document.Diagnostics.Select(diagnostic => Slice(source, diagnostic.Span)).ToArray(),
            string.Join("\n", document.Diagnostics));
        Assert.IsTrue(document.Diagnostics.All(diagnostic => diagnostic is { Id: "JOLT1022", Severity: DiagnosticSeverity.Error }));
        Assert.IsTrue(document.Diagnostics[^1].Message.Contains("explicit property and event handler", StringComparison.Ordinal));
    }
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
                    <input @bind="Name" @bind:event="oninput" @key="Id" />
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
