using Jolt.Npm;
using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text.RegularExpressions;

namespace Jolt.Compiler;

internal sealed record ComponentProperty(string Name, string? Type, bool Required, IReadOnlyList<string>? Literals = null, string? Unsupported = null, Microsoft.CodeAnalysis.Text.TextSpan? Source = null);
internal sealed record ComponentSlot(string Name, bool Required, IReadOnlyList<ComponentProperty>? Scope = null, string? Unsupported = null);

internal sealed record ComponentContract(
    string Name,
    string Path,
    IReadOnlyList<ComponentProperty> Properties,
    IReadOnlyList<ComponentSlot> Slots,
    NpmVueComponent? Npm = null)
{
    internal static ComponentContract FromLocal(Projection projection, INamedTypeSymbol state, SemanticModel? model = null)
    {
        var props = state.GetMembers().OfType<IPropertySymbol>()
            .Where(property => property is { IsStatic: false, DeclaredAccessibility: Accessibility.Public, GetMethod: not null, SetMethod: not null } &&
                property.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "ParameterAttribute"))
            .Select(property => new ComponentProperty(
                property.Name,
                property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                property.IsRequired || property.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "EditorRequiredAttribute"),
                Source: property.Locations.FirstOrDefault(location => location.IsInSource) is { } location ? projection.Map(location.SourceSpan) : null))
            .ToArray();

        var slots = Descendants(projection.Document.Nodes).Where(element => element.Name == "slot")
            .GroupBy(element => Text(element.Attributes.FirstOrDefault(attribute => attribute.Name == "name")) ?? "default")
            .Select(group => new ComponentSlot(
                group.Key,
                false,
                group.SelectMany(element => element.Attributes.Where(attribute => attribute.Name != "name"))
                    .DistinctBy(attribute => attribute.Name)
                    .Select(attribute => new ComponentProperty(attribute.Name, attribute.Value switch
                    {
                        [JoltExpression expression] when model is not null => projection.GetExpressionType(expression, model) ?? "object",
                        [JoltText] => "string",
                        [] => "bool",
                        _ => "object"
                    }, false)).ToArray()))
            .ToArray();

        return new ComponentContract(projection.Name, projection.Document.Path, props, slots);
    }

    internal static ComponentContract FromNpm(NpmVueComponent component)
    {
        var props = component.Props.Select(entry => new ComponentProperty(
            entry.Key, ToCSharpType(entry.Value.Type), entry.Value.Required, entry.Value.LiteralValues, entry.Value.Type));
        var events = component.Events.Select(entry =>
        {
            var signatures = entry.Value.Signatures;
            var type = signatures.Count == 1 ? DelegateType(signatures[0]) : null;
            return new ComponentProperty(EventProperty(entry.Key), type, false,
                Unsupported: signatures.Count == 1 ? Describe(signatures[0]) : "multiple or missing signatures");
        });
        var slots = component.Slots.Select(entry =>
        {
            var signatures = entry.Value.Signatures;
            return signatures switch
            {
                [[var scope]] => GetScope(scope) is { Count: > 0 } properties
                    ? new ComponentSlot(entry.Key, entry.Value.Required, properties)
                    : new ComponentSlot(entry.Key, entry.Value.Required, Unsupported: scope.Type),
                [[]] => new ComponentSlot(entry.Key, entry.Value.Required),
                _ => new ComponentSlot(entry.Key, entry.Value.Required, Unsupported: "multiple or unsupported signatures"),
            };
        }).ToArray();
        return new ComponentContract(NpmNames.TagFor(component.ModuleSpecifier, component.ExportName), component.ModuleSpecifier,
            events.Concat(props).DistinctBy(prop => prop.Name).ToArray(), slots, component);
    }

    internal static string EventProperty(string name) => name.Length == 0 ? "on" : "on" + char.ToUpperInvariant(name[0]) + name[1..];

    internal static string? Text(JoltAttribute? attribute) => attribute is { Value.Length: 1 } && attribute.Value[0] is JoltText text
        ? text.Text
        : null;

    internal static IEnumerable<JoltElement> Descendants(IEnumerable<JoltNode> nodes)
    {
        foreach (var node in nodes)
        {
            switch (node)
            {
                case JoltElement element:
                    yield return element;
                    foreach (var child in Descendants(element.Children))
                        yield return child;
                    break;
                case JoltIf conditional:
                    foreach (var child in Descendants(conditional.Then.Concat(conditional.Else)))
                        yield return child;
                    break;
                case JoltForEach loop:
                    foreach (var child in Descendants(loop.Children))
                        yield return child;
                    break;
            }
        }
    }

    private static string Describe(IReadOnlyList<NpmVueParameter> parameters) => string.Join(", ", parameters.Select(parameter => parameter.Type));

    private static string? DelegateType(IReadOnlyList<NpmVueParameter> parameters)
    {
        if (parameters.Count == 0)
            return "global::System.Action";
        var types = parameters.Select(parameter => ToCSharpType(parameter.Type)).ToArray();
        if (types.Length > 16 || types.Any(type => type is null))
            return null;
        return "global::System.Action<" + string.Join(", ", types) + ">";
    }

    private static IReadOnlyList<ComponentProperty>? GetScope(NpmVueParameter parameter)
        => parameter.Properties?.Select(member =>
            new ComponentProperty(member.Key, ToCSharpType(member.Value), false, Unsupported: member.Value)).ToArray();

    private static string? ToCSharpType(string type)
    {
        type = type.Trim();
        if (type.StartsWith("readonly ", StringComparison.Ordinal))
            type = type["readonly ".Length..];
        if (type is ['[', .., ']'] && type.IndexOf('[', 1) < 0)
        {
            var entries = type[1..^1].Split(',', StringSplitOptions.TrimEntries)
                .Select(entry => Regex.Match(entry, @"^(?:[A-Za-z_][A-Za-z0-9_]*\s*:\s*)?([^:]+)$"))
                .Select(entry => entry.Success ? ToCSharpType(entry.Groups[1].Value) : null).ToArray();
            return entries.Length > 1 && entries.All(entry => entry is not null) ? "(" + string.Join(", ", entries) + ")" : null;
        }
        var members = type.Split('|', StringSplitOptions.TrimEntries)
            .Where(member => member is not ("undefined" or "null"))
            .Select(member => member switch
            {
                "boolean" or "true" or "false" => "bool",
                "number" => "double",
                "string" => "string",
                _ when member.Length > 1 && member[0] is '\'' or '"' => "string",
                _ => null,
            })
            .Distinct()
            .ToArray();
        var name = members.Length == 1 ? members[0] : null;
        return name is "bool" or "double" or "string" && type.Split('|', StringSplitOptions.TrimEntries).Any(member => member is "undefined" or "null")
            ? name + "?"
            : name;
    }
}
