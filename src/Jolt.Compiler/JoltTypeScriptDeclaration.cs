using System.Text;
using System.Text.Json;
using Jazor.Compiler;
using Jazor.Common.SourceMaps;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using static Jolt.Compiler.JoltTypeScriptType;

namespace Jolt.Compiler;

internal static class JoltTypeScriptDeclaration
{
    internal static (string Content, string SourceMap) Emit(ComponentContract contract, Projection projection,
        INamedTypeSymbol module, INamedTypeSymbol state, IReadOnlyList<IMethodSymbol> exposed)
    {
        var text = new StringBuilder();
        var segments = new List<SourceMapSegment>();
        int line = 0;
        void Line(string value)
        {
            text.Append(value).Append('\n');
            line++;
        }
        void Mark(int column, TextSpan? span)
        {
            if (span is not { } source)
                return;
            var position = projection.Source.Lines.GetLinePosition(source.Start);
            segments.Add(new SourceMapSegment(line, column, 0, position.Line, position.Character));
        }
        TextSpan? Authored(ISymbol symbol) => symbol.Locations.FirstOrDefault(location => location.IsInSource) is { } location
            ? projection.Map(location.SourceSpan)
            : null;

        var parameters = state.TypeParameters;
        var generic = TypeParameters(parameters);
        var arguments = parameters.IsEmpty ? "" : $"<{string.Join(", ", parameters.Select(TypeParameterName))}>";
        var models = contract.LocalModels.ToDictionary(model => model.Property, StringComparer.Ordinal);
        var modelCallbacks = contract.LocalModels.Select(model => model.Callback).ToHashSet(StringComparer.Ordinal);
        var slotOutlets = ComponentContract.SlotOutlets(projection.Document).ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        Line("import type { AllowedComponentProps, VNode, VNodeChild, VNodeProps } from 'vue';");
        Line("");
        Line($"type Slots{generic} = {{");
        foreach (var slot in contract.Slots)
        {
            var scope = slot.Scope ?? [];
            var outlet = slotOutlets[slot.Name];
            Mark(4, outlet.Attributes.FirstOrDefault(attribute => attribute.Name == "name")?.Value.FirstOrDefault()?.Span ?? outlet.NameSpan);
            if (scope.Count == 0)
            {
                Line($"    {Quote(slot.Name)}?(): VNodeChild;");
                continue;
            }
            Line($"    {Quote(slot.Name)}?(scope: {{");
            foreach (var property in scope)
            {
                Mark(8, property.Source);
                Line($"        {Quote(property.Name)}: {Map(property.LocalType)};");
            }
            Line("    }): VNodeChild;");
        }
        Line("};");

        Line($"type Props{generic} = VNodeProps & AllowedComponentProps & {{");
        var requiredModelAliases = new List<(string Name, string Alias, string Type, TextSpan? Source)>();
        var callbackRequirements = new List<(string Text, TextSpan? Source)>();
        var events = new List<(string Name, string Type)>();
        foreach (var property in contract.Properties)
        {
            var type = Map(property.LocalType);
            var alias = contract.VuePropertyAliases.GetValueOrDefault(property.Name);
            var required = property.Required && !modelCallbacks.Contains(property.Name);

            // The Vue camelCase spelling and the authored spelling share a prop.
            if (alias is not null && required)
            {
                requiredModelAliases.Add((property.Name, alias, type, property.Source));
            }
            else
            {
                Mark(4, property.Source);
                Line($"    {Quote(property.Name)}{(required ? "" : "?")}: {type};");
                if (alias is not null)
                {
                    Mark(4, property.Source);
                    Line($"    {Quote(alias)}?: {type};");
                }
            }
            if (!models.TryGetValue(property.Name, out var model))
                continue;

            var listener = ComponentContract.EventProperty(model.Event);
            Mark(4, property.Source);
            Line($"    {Quote(listener)}?: (value: {type}) => void;");
            Mark(4, property.Source);
            Line($"    {Quote(listener + "Once")}?: (value: {type}) => void;");
            events.Add((model.Event, type));
            var callbackProperty = contract.Properties.Single(candidate => candidate.Name == model.Callback);
            if (callbackProperty.Required)
            {
                // The authored callback can be supplied directly or by a Vue listener.
                var callbackType = Map(callbackProperty.LocalType);
                var listenerType = $"(value: {type}) => void";
                var callbackAlternatives = new List<string> { $"{{ {Quote(model.Callback)}: {callbackType} }}" };
                if (contract.VuePropertyAliases.GetValueOrDefault(model.Callback) is { } vueCallback)
                    callbackAlternatives.Add($"{{ {Quote(vueCallback)}: {callbackType} }}");
                callbackAlternatives.Add($"{{ {Quote(listener)}: {listenerType} }}");
                callbackAlternatives.Add($"{{ {Quote(listener + "Once")}: {listenerType} }}");
                callbackRequirements.Add(("    & (" + string.Join(" | ", callbackAlternatives) + ")", callbackProperty.Source));
            }
        }
        if (contract.Slots.Count > 0)
            Line($"    'v-slots'?: Slots{arguments};");
        var requirementsRemaining = requiredModelAliases.Count + callbackRequirements.Count;
        Line(requirementsRemaining == 0 ? "};" : "}");
        foreach (var (name, alias, type, source) in requiredModelAliases)
        {
            var authoredName = Quote(name);
            var vueName = Quote(alias);
            var authoredBranch = $"    & ({{ {authoredName}: {type}; ";
            Mark(9, source);
            Mark(authoredBranch.Length, source);
            Line(authoredBranch + $"{vueName}?: {type} }}");
            var vueBranch = $"       | {{ {authoredName}?: {type}; ";
            Mark(11, source);
            Mark(vueBranch.Length, source);
            Line(vueBranch + $"{vueName}: {type} }})" + (--requirementsRemaining == 0 ? ";" : ""));
        }
        foreach (var (requirement, source) in callbackRequirements)
        {
            Mark(9, source);
            Line(requirement + (--requirementsRemaining == 0 ? ";" : ""));
        }

        Line($"type Exposed{generic} = {{");
        foreach (var method in exposed)
        {
            Mark(4, Authored(method));
            Line($"    {Quote(method.Name)}{TypeParameters(method.TypeParameters)}({Parameters(method)}): {ReturnType(method)};");
        }
        Line("};");
        Line($"type Emit{generic} = {{");
        foreach (var (name, type) in events)
            Line($"    (event: {Quote(name)}, value: {type}): void;");
        Line("};");
        Line("");

        Mark("declare const ".Length, default(TextSpan));
        if (parameters.IsEmpty)
            Line("declare const component: new () => Exposed & { $props: Props; $slots: Slots; $emit: Emit };");
        else
        {
            // Vue language-tools reads the return context after inferring the
            // component's type arguments from the template props.
            Line($"declare const component: {generic}(props: Props{arguments}, context?: {{");
            Line($"    slots: Slots{arguments};");
            Line($"    emit: Emit{arguments};");
            Line($"    expose: (value: Exposed{arguments}) => void;");
            Line("}) => VNode & { __ctx?: {");
            Line($"    props: Props{arguments};");
            Line($"    slots: Slots{arguments};");
            Line($"    emit: Emit{arguments};");
            Line($"    expose: (value: Exposed{arguments}) => void;");
            Line("} };");
        }
        Line("export default component;");

        // Match AstConverter's standard module policy for authored static members.
        // Local names avoid collisions with declaration helpers and JS keywords.
        int exportIndex = 0;
        foreach (var symbol in module.GetMembers().Where(symbol => symbol is IFieldSymbol or IMethodSymbol { MethodKind: MethodKind.Ordinary } &&
            symbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal))
        {
            string local = $"$export{exportIndex++}";
            var source = Authored(symbol);
            if (symbol is IMethodSymbol method)
            {
                Mark("declare function ".Length, source);
                Line($"declare function {local}{TypeParameters(method.TypeParameters)}({Parameters(method)}): {ReturnType(method)};");
            }
            else if (symbol is IFieldSymbol field)
            {
                var keyword = field.IsConst || field.IsReadOnly ? "const" : "let";
                Mark($"declare {keyword} ".Length, source);
                Line($"declare {keyword} {local}: {Map(field.Type)};");
            }
            Mark($"export {{ {local} as ".Length, source);
            Line($"export {{ {local} as {Quote(Util.GetConfigOrSymbolName(symbol))} }};");
        }

        // TypeScript skips declaration maps with embedded source content. It reads
        // the .jazor file the caller places at the map's relative source path.
        var map = new SourceMapDocument("", [new SourceMapSource(contract.Path, null)], segments);
        return (text.ToString(), new SourceMapWriter().Write(map));
    }

    private static string Quote(string value) => JsonSerializer.Serialize(value);

    private static string TypeParameters(IReadOnlyList<ITypeParameterSymbol> parameters) => parameters.Count == 0 ? "" :
        $"<{string.Join(", ", parameters.Select(parameter =>
        {
            var constraints = parameter.ConstraintTypes.Select(Map).Where(type => type != "unknown").ToList();
            if (parameter.HasNotNullConstraint || parameter.HasValueTypeConstraint ||
                parameter.HasReferenceTypeConstraint && parameter.ReferenceTypeConstraintNullableAnnotation != NullableAnnotation.Annotated)
                constraints.Add("NonNullable<unknown>");
            return TypeParameterName(parameter) + (constraints.Count == 0 ? "" : " extends " + string.Join(" & ", constraints));
        }))}>";
}
