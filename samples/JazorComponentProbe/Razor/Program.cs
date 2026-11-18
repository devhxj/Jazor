using JazorComponentProbe.Razor;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

const string source = """
    @using Npm.RekaUi

    <section><SwitchRoot ModelValue="@(Count > 0)" /><span>Count @Count</span></section>

    @code { private int Count = 1; }
    """;
var path = args.Length == 1 ? args[0] : "Counter.jazor";
if (args.Length == 0)
    Verify(source, path);

var input = args.Length == 1 ? File.ReadAllText(path) : source;
var result = RazorProbe.Parse(input, path);
Print(result.Document, 0);
foreach (var diagnostic in result.Diagnostics)
    Console.WriteLine(diagnostic);

void Print(IntermediateNode node, int depth)
{
    var range = node.Source is { } span
        ? $"{span.AbsoluteIndex}+{span.Length} {System.Text.Json.JsonSerializer.Serialize(input.Substring(span.AbsoluteIndex, span.Length))}"
        : "";
    Console.WriteLine($"{new string(' ', depth)}{node.GetType().Name} {range}");
    foreach (var child in node.Children)
        Print(child, depth + 1);
}

static void Verify(string source, string path)
{
    var result = RazorProbe.Parse(source, path);
    Require(result.Diagnostics.All(d => d.Severity != RazorDiagnosticSeverity.Error), "valid input diagnostics");
    var root = result.Document.FindPrimaryMethod()!.Children.OfType<MarkupElementIntermediateNode>().Single();
    Require(root.TagName == "section", "nested markup root");
    var children = root.Body.OfType<MarkupElementIntermediateNode>().ToArray();
    Require(children.Length == 2 && children[0].TagName == "SwitchRoot" && children[1].TagName == "span", "nested elements in order");
    Require(Slice(children[0]) == "<SwitchRoot ModelValue=\"@(Count > 0)\" />", "component span");

    var attribute = children[0].Attributes.Single();
    Require(attribute.AttributeName == "ModelValue" && Slice(attribute) == " ModelValue=\"@(Count > 0)\"", "attribute name and span");
    var expression = attribute.Children.OfType<CSharpExpressionAttributeValueIntermediateNode>().Single();
    Require(Slice(expression) == "@(Count > 0)" && Slice(expression.Children.Single()) == "Count > 0", "attribute transition and C# token spans");
    Require(Slice(children[1].Body.OfType<HtmlContentIntermediateNode>().Single()) == "Count ", "mixed text span");
    Require(Slice(children[1].Body.OfType<CSharpExpressionIntermediateNode>().Single()) == "Count", "mixed implicit expression span");
    Require(Slice(result.Document.FindPrimaryNamespace()!.Children.OfType<UsingDirectiveIntermediateNode>().Single()) == "using Npm.RekaUi", "using directive span");
    Require(Slice(result.Document.FindPrimaryClass()!.Children.OfType<CSharpCodeIntermediateNode>().Single()) == " private int Count = 1; ", "code directive body span");

    var staticMarkup = RazorProbe.Parse("<p>Static</p>", path);
    Require(staticMarkup.Document.FindPrimaryMethod()!.Children.OfType<MarkupBlockIntermediateNode>().Single().Content == "<p>Static</p>", "static markup coalescing");

    foreach (var incomplete in new[]
    {
        "<p>Retained @Count</p>\n\n@code { private int Count = 1;",
        "<p>Retained @Count</p>\n\n<section>",
        "<p>Retained @Count</p>\n\n<SwitchRoot ModelValue=\"@(Count >",
    })
    {
        var recovered = RazorProbe.Parse(incomplete, path);
        Require(recovered.Diagnostics.Any(d => d.Severity == RazorDiagnosticSeverity.Error), "incomplete input diagnostic");
        Require(recovered.Document.FindPrimaryMethod()!.Children.OfType<MarkupElementIntermediateNode>().Any(element => element.TagName == "p"), "preceding element recovery");
    }

    Console.WriteLine("Verified nested markup, directives, exact source slices, and incomplete input.");
    return;

    string Slice(IntermediateNode node) => node.Source is { } span ? source.Substring(span.AbsoluteIndex, span.Length) : "";

    static void Require(bool condition, string check)
    {
        if (!condition)
            throw new InvalidOperationException($"Razor probe check failed ({check}).");
    }
}
