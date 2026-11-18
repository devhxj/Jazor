using System.Text.Json;
using System.Text.RegularExpressions;
using Jazor.Compiler;
using Jazor.ComponentProbe.Npm;
using JazorComponentProbe.Razor;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

if (args.Length != 3)
{
    Console.Error.WriteLine("Usage: JazorComponentProbe <input.jazor> <npm-project> <output.mjs>");
    return 2;
}

var path = Path.GetFullPath(args[0]);
var source = SourceText.From(await File.ReadAllTextAsync(path));
var parsed = RazorProbe.Parse(source.ToString(), path);
foreach (var diagnostic in parsed.Diagnostics.Where(d => d.Severity == RazorDiagnosticSeverity.Error))
    Console.Error.WriteLine(diagnostic);
if (parsed.Diagnostics.Any(d => d.Severity == RazorDiagnosticSeverity.Error))
    return 1;

var method = parsed.Document.FindPrimaryMethod()!;
var elements = method.Children.OfType<MarkupElementIntermediateNode>().ToArray();
var imports = parsed.Document.FindPrimaryNamespace()!.Children.OfType<UsingDirectiveIntermediateNode>().ToArray();
var members = parsed.Document.FindPrimaryClass()!.Children.OfType<CSharpCodeIntermediateNode>().ToArray();
if (elements.Length != 1 || imports.Length != 1 || members.Length != 1 ||
    method.Children.Any(child => child != elements[0] && !IsWhitespace(child)) ||
    elements[0].Body.Any(child => !IsWhitespace(child)) || elements[0].Attributes.Count() != 1)
    throw new NotSupportedException("This probe expects one @using, one component with one expression prop and no children, and one @code.");

var element = elements[0];
var attribute = element.Attributes.Single();
var expressionNode = attribute.Children.OfType<CSharpExpressionAttributeValueIntermediateNode>().SingleOrDefault();
if (attribute.Children.Count != 1 || expressionNode?.Children.Count != 1 || expressionNode.Children[0].Source is not { } expressionSpan)
    throw new NotSupportedException("This probe requires a single C# expression in the attribute.");

using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(args[1], "package.json")));
var packages = manifest.RootElement.GetProperty("dependencies").EnumerateObject()
    .Where(dep => NamespaceFor(dep.Name) == imports[0].Content).Select(dep => dep.Name).ToArray();
if (packages.Length != 1)
    return Report(imports[0].Source!.Value.AbsoluteIndex, $"Expected one npm dependency matching {imports[0].Content}.");

using var metadata = JsonDocument.Parse(await NpmComponentProbe.ReadAsync(args[1], packages[0], element.TagName));
var props = metadata.RootElement.GetProperty("props");
var matching = props.EnumerateObject()
    .Where(prop => prop.Name == attribute.AttributeName || char.ToUpperInvariant(prop.Name[0]) + prop.Name[1..] == attribute.AttributeName)
    .ToArray();
if (matching.Length != 1)
    return Report(attribute.Source!.Value.AbsoluteIndex, $"Cannot resolve prop {attribute.AttributeName} on {element.TagName}.");
var prop = matching[0];
foreach (var required in props.EnumerateObject())
    if (required.Name != prop.Name && required.Value.GetProperty("required").GetBoolean())
        return Report(element.Source!.Value.AbsoluteIndex, $"Missing required prop {required.Name} on {element.TagName}.");

var expectedType = prop.Value.GetProperty("type").GetString() switch
{
    "boolean" => "bool",
    "string" => "string",
    "number" => "double",
    var other => throw new NotSupportedException($"This probe cannot project {element.TagName}.{prop.Name} ({other})."),
};
var memberSpan = members[0].Source!.Value;
const string header = "public static class ComponentModule { public sealed class ComponentState {\n";
var memberText = Slice(memberSpan);
var expressionHeader = header + memberText + "\npublic " + expectedType + " ReadProp() => ";
var expression = Slice(expressionSpan);
var tree = CSharpSyntaxTree.ParseText(expressionHeader + expression + ";\n} }");
var compilation = CSharpCompilation.Create("ComponentProbe", [tree],
    [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
foreach (var diagnostic in errors)
{
    // These two projected regions are verbatim text, so offsets remain UTF-16 offsets.
    var offset = diagnostic.Location.SourceSpan.Start;
    if (offset >= expressionHeader.Length && offset <= expressionHeader.Length + expression.Length)
        Report(expressionSpan.AbsoluteIndex + offset - expressionHeader.Length, $"{diagnostic.Id}: {diagnostic.GetMessage()}");
    else if (offset >= header.Length && offset <= header.Length + memberText.Length)
        Report(memberSpan.AbsoluteIndex + offset - header.Length, $"{diagnostic.Id}: {diagnostic.GetMessage()}");
    else
        Console.Error.WriteLine(diagnostic);
}
if (errors.Length > 0)
    return 1;

var moduleSymbol = compilation.GetTypeByMetadataName("ComponentModule")!;
var converter = new AstConverter(moduleSymbol, compilation.GetSemanticModel(tree));
var module = await converter.Convert() ?? throw new InvalidOperationException("No JavaScript module produced.");
var output = $$"""
    import { defineComponent as defineVueComponent, h as createVNode } from "vue";
    import { {{element.TagName}} as ImportedComponent } from {{JsonSerializer.Serialize(packages[0])}};
    {{module.ToKnRECMAScript()}}
    export default defineVueComponent({
        setup() {
            const state = new ComponentState();
            return () => createVNode(ImportedComponent, { [{{JsonSerializer.Serialize(prop.Name)}}]: state.ReadProp() });
        }
    });
    """;
await File.WriteAllTextAsync(args[2], output);
return 0;

string Slice(SourceSpan span) => source.ToString(new TextSpan(span.AbsoluteIndex, span.Length));

int Report(int offset, string message)
{
    var position = source.Lines.GetLinePosition(offset);
    Console.Error.WriteLine($"{path}({position.Line + 1},{position.Character + 1}): {message}");
    return 1;
}

bool IsWhitespace(IntermediateNode node) => node is HtmlContentIntermediateNode && node.Source is { } span && string.IsNullOrWhiteSpace(Slice(span));

static string NamespaceFor(string package) => "Npm." + string.Concat(Regex.Split(package, "[^A-Za-z0-9]+")
    .Where(part => part.Length != 0).Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
