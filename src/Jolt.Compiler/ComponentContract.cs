using Jazor.Npm;
using Jolt.Npm;
using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace Jolt.Compiler;

internal sealed record ComponentProperty(string Name, string? Type, bool Required, IReadOnlyList<string>? Literals = null, string? Unsupported = null, Microsoft.CodeAnalysis.Text.TextSpan? Source = null)
{
    internal ITypeSymbol? LocalType { get; init; }
    internal IReadOnlyDictionary<string, string>? LiteralExpressions { get; init; }
    internal string? InputDomain { get; init; }
    internal IReadOnlyDictionary<string, string>? InputRecordFields { get; init; }
    internal bool InputNullable { get; init; }
    internal bool HasEventPayload { get; init; }
}
internal sealed record ComponentSlot(string Name, bool Required, IReadOnlyList<ComponentProperty>? Scope = null, string? Unsupported = null);
internal sealed record ComponentTypeParameter(string Name, string Constraint = "", IReadOnlyList<string>? NpmConstraint = null);
internal sealed record LocalModel(string Property, string Callback)
{
    internal string VueProperty => char.ToLowerInvariant(Property[0]) + Property[1..];
    internal string Event => "update:" + VueProperty;
}
internal sealed record ComponentTypeBinding(string[]? Arguments, Microsoft.CodeAnalysis.Text.TextSpan? ErrorSource = null, string? Error = null);

internal sealed record ComponentContract(
    string Name,
    string Path,
    IReadOnlyList<ComponentProperty> Properties,
    IReadOnlyList<ComponentSlot> Slots,
    NpmVueComponent? Npm = null)
{
    internal IReadOnlyList<ComponentTypeParameter> TypeParameters { get; init; } = [];
    internal bool IsVueBuiltIn { get; init; }
    internal string? ReferenceType { get; init; }
    internal IReadOnlyList<LocalModel> LocalModels { get; init; } = [];
    internal IReadOnlyDictionary<string, string> VuePropertyAliases { get; init; } = new Dictionary<string, string>();

    internal static readonly SymbolDisplayFormat TypeDisplay = SymbolDisplayFormat.FullyQualifiedFormat
        .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    internal ComponentContract Specialize(IReadOnlyList<string> types)
    {
        var substitutions = TypeParameters.Select((parameter, index) => (parameter.Name, Type: SyntaxFactory.ParseTypeName(types[index])))
            .ToDictionary(entry => entry.Name, entry => entry.Type, StringComparer.Ordinal);
        ComponentProperty Substitute(ComponentProperty property)
        {
            if (property.Type is null)
                return property;
            var syntax = SyntaxFactory.ParseTypeName(property.Type);
            var matches = syntax.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>()
                .Where(name => substitutions.ContainsKey(name.Identifier.ValueText) && name.Parent is not QualifiedNameSyntax and not AliasQualifiedNameSyntax);
            return property with { Type = syntax.ReplaceNodes(matches, (original, _) =>
            {
                var replacement = substitutions[original.Identifier.ValueText];
                // A consumer can itself supply a nullable type argument for T?.
                if (original.Parent is NullableTypeSyntax && replacement is NullableTypeSyntax nullable)
                    replacement = nullable.ElementType;
                return replacement.WithTriviaFrom(original);
            }).ToString() };
        }
        return this with
        {
            Properties = Properties.Select(Substitute).ToArray(),
            Slots = Slots.Select(slot => slot with { Scope = slot.Scope?.Select(Substitute).ToArray() }).ToArray(),
        };
    }

    internal static ComponentContract FromLocal(Projection projection, INamedTypeSymbol state, Compilation compilation, SemanticModel? model = null)
    {
        var symbols = state.GetMembers().OfType<IPropertySymbol>()
            .Where(property => property is { IsStatic: false, DeclaredAccessibility: Accessibility.Public, GetMethod: not null, SetMethod: not null } &&
                property.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "ParameterAttribute"))
            .ToDictionary(property => property.Name, StringComparer.Ordinal);
        var props = symbols.Values
            .Select(property => new ComponentProperty(
                property.Name,
                property.Type.ToDisplayString(TypeDisplay),
                property.IsRequired || property.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "EditorRequiredAttribute"),
                Source: property.Locations.FirstOrDefault(location => location.IsInSource) is { } location ? projection.Map(location.SourceSpan) : null)
                { LocalType = property.Type })
            .ToArray();

        var slots = SlotOutlets(projection.Document)
            .Select(group => new ComponentSlot(
                group.Key,
                false,
                group.SelectMany(SlotMembers)
                    .DistinctBy(attribute => attribute.Name)
                    .Select(attribute =>
                    {
                        var type = attribute.Value switch
                        {
                            [JoltExpression expression] => model is null ? null : projection.GetExpressionTypeSymbol(expression, model),
                            [] => compilation.GetSpecialType(SpecialType.System_Boolean),
                            _ => compilation.GetSpecialType(SpecialType.System_String),
                        };
                        var display = attribute.Value switch
                        {
                            [] => "bool",
                            [JoltExpression] => type?.ToDisplayString(TypeDisplay) ?? "object",
                            _ => "string",
                        };
                        return new ComponentProperty(attribute.Name, display,
                            false, Source: attribute.NameSpan) { LocalType = type };
                    }).ToArray()))
            .ToArray();

        var aliases = symbols.Keys.Select(name => (Name: name, Alias: char.ToLowerInvariant(name[0]) + name[1..]))
            .Where(pair => !symbols.ContainsKey(pair.Alias))
            .GroupBy(pair => pair.Alias, StringComparer.Ordinal).Where(group => group.Count() == 1)
            .Select(group => group.Single()).ToDictionary(pair => pair.Name, pair => pair.Alias, StringComparer.Ordinal);
        var actionType = compilation.GetTypeByMetadataName("System.Action`1");
        return new ComponentContract(projection.Name, projection.Document.Path, props, slots)
        {
            VuePropertyAliases = aliases,
            LocalModels = symbols.Values
                .Where(property => symbols.TryGetValue(property.Name + "Changed", out var callback) &&
                    callback.Type is INamedTypeSymbol action &&
                    SymbolEqualityComparer.Default.Equals(action.OriginalDefinition, actionType) &&
                    SymbolEqualityComparer.IncludeNullability.Equals(action.TypeArguments[0], property.Type))
                .Select(property => new LocalModel(property.Name, property.Name + "Changed"))
                .Where(model => model.VueProperty == model.Property || aliases.ContainsKey(model.Property))
                .ToArray(),
            ReferenceType = projection.ReferenceType,
            TypeParameters = state.TypeParameters.Select(parameter => new ComponentTypeParameter(parameter.Name, GetConstraint(parameter))).ToArray(),
        };
    }

    internal static IEnumerable<IGrouping<string, JoltElement>> SlotOutlets(JoltDocument document) =>
        JoltTree.Elements(document.Nodes).Where(element => element.Name == "slot")
            .GroupBy(element => Text(element.Attributes.FirstOrDefault(attribute => attribute.Name == "name")) ?? "default");

    internal static IEnumerable<JoltAttribute> SlotMembers(JoltElement element) =>
        element.Attributes.Where(attribute => attribute.Name != "name" && attribute.Kind == JoltAttributeKind.Markup);

    private static string GetConstraint(ITypeParameterSymbol parameter)
    {
        var constraints = new List<string>();
        if (parameter.HasUnmanagedTypeConstraint)
            constraints.Add("unmanaged");
        else if (parameter.HasValueTypeConstraint)
            constraints.Add("struct");
        else if (parameter.HasReferenceTypeConstraint)
            constraints.Add(parameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated ? "class?" : "class");
        else if (parameter.HasNotNullConstraint)
            constraints.Add("notnull");
        constraints.AddRange(parameter.ConstraintTypes.Where(type => type.TypeKind != TypeKind.Error)
            .Select(type => type.ToDisplayString(TypeDisplay)));
        if (parameter.HasConstructorConstraint)
            constraints.Add("new()");
        if (parameter.AllowsRefLikeType)
            constraints.Add("allows ref struct");
        return constraints.Count == 0 ? "" : $" where {parameter.Name} : {string.Join(", ", constraints)}";
    }

    internal static ComponentContract FromNpm(NpmVueComponent component)
    {
        var parameter = component.TypeParameter?.Name;
        var props = component.Props.Select(entry => new ComponentProperty(
            entry.Key, InputType(entry.Value.InputType) is { } input
                ? input + (entry.Value.InputNullable ? "?" : "")
                : ToCSharpType(entry.Value.Type, parameter),
            entry.Value.Required, entry.Value.LiteralValues, entry.Value.Type)
        {
            InputDomain = entry.Value.InputType,
            InputRecordFields = entry.Value.InputRecordFields,
            InputNullable = entry.Value.InputNullable,
        });
        var events = component.Events.Select(entry =>
        {
            var signatures = entry.Value.Signatures;
            var type = signatures.Count == 1 ? DelegateType(signatures[0], parameter) : null;
            return new ComponentProperty(VueEventProperty(entry.Key), type, false,
                Unsupported: signatures.Count == 1 ? Describe(signatures[0]) : "multiple or missing signatures")
                { HasEventPayload = type is not null && signatures[0].Count > 0 && signatures[0] is not [{ UntypedRest: true }] };
        });
        var slots = component.Slots.Select(entry =>
        {
            var signatures = entry.Value.Signatures;
            return signatures switch
            {
                [[var scope]] => GetScope(scope, parameter) is { Count: > 0 } properties
                    ? new ComponentSlot(entry.Key, entry.Value.Required, properties)
                    : new ComponentSlot(entry.Key, entry.Value.Required, Unsupported: scope.Type),
                [[]] => new ComponentSlot(entry.Key, entry.Value.Required),
                _ => new ComponentSlot(entry.Key, entry.Value.Required, Unsupported: "multiple or unsupported signatures"),
            };
        }).ToArray();
        return new ComponentContract(component.LocalPath is { } path ? System.IO.Path.GetFileNameWithoutExtension(path) :
            NpmNames.TagFor(component.ModuleSpecifier, component.ExportName), component.LocalPath ?? component.ModuleSpecifier,
            events.Concat(props).DistinctBy(prop => prop.Name).ToArray(), slots, component)
        {
            TypeParameters = component.TypeParameter is { } type ? [new(type.Name, NpmConstraint: type.PrimitiveKinds)] : [],
        };
    }

    internal static string EventProperty(string name) => name.Length == 0 ? "on" : "on" + char.ToUpperInvariant(name[0]) + name[1..];

    // Vue template listeners on components use toHandlerKey(camelize(event)).
    // A component may inspect vnode.props before deciding whether to emit.
    internal static string VueEventProperty(string name) => EventProperty(
        Regex.Replace(name, "-([a-zA-Z0-9_])", match => match.Groups[1].Value.ToUpperInvariant()));

    internal ModelBinding? FindModel(string name)
    {
        if (Npm is null)
        {
            var local = LocalModels.FirstOrDefault(candidate => candidate.Property == name || candidate.VueProperty == name);
            return local is null ? null : new ModelBinding(local.VueProperty, EventProperty(local.Event),
                Properties.First(property => property.Name == local.Property).Type!, Local: local);
        }
        var models = NpmVueModels.Get(Npm).ToArray();
        var model = models.FirstOrDefault(candidate => candidate.PropName == name);
        if (model is null)
        {
            var matches = models.Where(candidate => string.Equals(candidate.PropName, name, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (matches is [var match])
                model = match;
        }
        return model is null ? null : new ModelBinding(
            model.PropName, VueEventProperty(model.EventName), ToCSharpType(model.Type)!);
    }

    internal static string? Text(JoltAttribute? attribute) => attribute is { Value.Length: 1 } && attribute.Value[0] is JoltText text
        ? text.Text
        : null;

    private static string Describe(IReadOnlyList<NpmVueParameter> parameters) => string.Join(", ", parameters.Select(parameter => parameter.Type));

    private static string? DelegateType(IReadOnlyList<NpmVueParameter> parameters, string? typeParameter)
    {
        // Vue's string-list emits publish names with an any[] rest. A zero-argument
        // listener can safely ignore that payload without asserting an argument type.
        if (parameters.Count == 0 || parameters is [{ UntypedRest: true }])
            return "global::System.Action";
        var types = parameters.Select(parameter => parameter.WebType is { } host
            ? $"global::ECMAScript.{host}"
            : InputType(parameter.ValueDomain) ?? ToCSharpType(parameter.Type, typeParameter)).ToArray();
        if (types.Length > 16 || types.Any(type => type is null))
            return null;
        return "global::System.Action<" + string.Join(", ", types) + ">";
    }

    private static IReadOnlyList<ComponentProperty>? GetScope(NpmVueParameter parameter, string? typeParameter)
        => parameter.Properties?.Select(member =>
            new ComponentProperty(member.Key, ToCSharpType(member.Value, typeParameter), false, Unsupported: member.Value)).ToArray();

    private static string? InputType(string? type) => type switch
    {
        "boolean" => "bool",
        "number" => "double",
        "string" => "string",
        "boolean[]" => "bool[]",
        "number[]" => "double[]",
        "string[]" => "string[]",
        "number|string" => "global::ECMAScript.Vue.VueStringNumberValue",
        "boolean|string" => "global::ECMAScript.Vue.VueBooleanStringValue",
        "boolean|number" => "global::ECMAScript.Vue.VueBooleanNumberValue",
        "boolean|number|string" => "global::ECMAScript.Vue.VueBooleanStringNumberValue",
        _ => null,
    };

    private static string? ToCSharpType(string type, string? typeParameter = null)
    {
        type = type.Trim();
        if (type.StartsWith("readonly ", StringComparison.Ordinal))
            type = type["readonly ".Length..];
        if (typeParameter is not null)
        {
            if (type == typeParameter || type == typeParameter + "[]")
                return type;
            var callback = Regex.Match(type, @"^\(\w+: ([\w]+)\) => ([\w]+)$");
            if (callback.Success && callback.Groups[1].Value == typeParameter && ToCSharpType(callback.Groups[2].Value) is { } result)
                return $"global::System.Func<{typeParameter}, {result}>";
        }
        if (type is ['[', .., ']'] && type.IndexOf('[', 1) < 0)
        {
            var entries = type[1..^1].Split(',', StringSplitOptions.TrimEntries)
                .Select(entry => Regex.Match(entry, @"^(?:[A-Za-z_][A-Za-z0-9_]*\s*:\s*)?([^:]+)$"))
                .Select(entry => entry.Success ? ToCSharpType(entry.Groups[1].Value, typeParameter) : null).ToArray();
            return entries.Length > 1 && entries.All(entry => entry is not null) ? "(" + string.Join(", ", entries) + ")" : null;
        }
        var members = type.Split('|', StringSplitOptions.TrimEntries)
            .Where(member => member is not ("undefined" or "null"))
            .Select(member => member switch
            {
                "boolean" or "true" or "false" => "bool",
                "number" => "double",
                "string" => "string",
                _ when member == typeParameter => member,
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
