using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Jolt.Syntax;
using Microsoft.CodeAnalysis;

namespace Jolt.Compiler;

internal static class JoltRouter
{
    private static readonly Regex Literal = new(@"\A[A-Za-z0-9._~-]+\z", RegexOptions.CultureInvariant);
    private static readonly Regex Parameter = new(@"\A\{([A-Za-z_][A-Za-z0-9_]*)(:int)?\}\z", RegexOptions.CultureInvariant);
    private sealed record Route(string Path, string Page, string? Layout, IReadOnlyList<RouteProp> Props);
    private sealed record RouteProp(string Parameter, string Property, bool Integer);

    internal static string Build(IReadOnlyList<(Projection Projection, ComponentContract Contract)> documents, List<Diagnostic> diagnostics)
    {
        var routes = new List<Route>();
        // Vue Router defaults to case-insensitive matching and gives constrained
        // parameters precedence over unconstrained parameters.
        var patterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (projection, contract) in documents)
        {
            var directives = projection.Document.Nodes.OfType<JoltRouteDirective>().ToArray();
            var pages = directives.Where(directive => directive.Name == "page").ToArray();
            var layouts = directives.Where(directive => directive.Name == "layout").ToArray();
            var page = VueLowering.ModulePath(contract.Path);
            if (page.Equals(JoltCompileResult.RouterModulePath, StringComparison.OrdinalIgnoreCase))
                diagnostics.Add(JoltProjectCompiler.Error("JOLT1043", projection.Location(default),
                    $"'{page}' is reserved for the Jolt router adapter."));

            foreach (var duplicate in layouts.Skip(1))
                Report(duplicate, "A routed component can select one @layout.");
            string? layoutPath = null;
            if (layouts.FirstOrDefault() is { } layout)
            {
                if (pages.Length == 0)
                {
                    Report(layout, "@layout requires an @page directive on the same component.");
                }
                else
                {
                    var name = layout.Argument;
                    var imports = projection.Imports.Select(import => import.Code).ToHashSet(StringComparer.Ordinal);
                    var candidates = documents.Where(item => name == $"{item.Projection.Namespace}.{item.Contract.Name}" ||
                        name == item.Contract.Name && JoltProjectCompiler.IsVisible(projection, item.Projection.Namespace, imports)).ToArray();
                    if (candidates is not [{ Projection: var layoutProjection, Contract: var layoutContract }])
                        Report(layout, $"@layout '{name}' must resolve to one local .jazor component. Use a qualified name if it is ambiguous.");
                    else if (layoutProjection.Document.Nodes.OfType<JoltRouteDirective>().Any() ||
                        layoutContract.TypeParameters.Count != 0 || layoutContract.Properties.Any(property => property.Required) ||
                        !layoutContract.Slots.Any(slot => slot.Name == "default" && slot.Scope is not { Count: > 0 }))
                        Report(layout, "A layout must be a non-generic local component with an unscoped default <slot />, no required parameters, and no route directives.");
                    else
                        layoutPath = VueLowering.ModulePath(layoutContract.Path);
                }
            }

            foreach (var directive in pages)
            {
                if (contract.TypeParameters.Count != 0)
                {
                    Report(directive, "@page requires a non-generic component.");
                    continue;
                }
                var argument = directive.Argument;
                if (argument.Length < 3 || argument[0] != '"' || argument[^1] != '"' || argument[1] != '/')
                {
                    Report(directive, "Use a quoted absolute path in @page.");
                    continue;
                }

                var authored = argument[1..^1];
                var path = new StringBuilder();
                var pattern = new StringBuilder();
                var props = new List<RouteProp>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                bool valid = true;
                foreach (var segment in authored == "/" ? [] : authored[1..].Split('/'))
                {
                    path.Append('/');
                    pattern.Append('/');
                    if (Literal.IsMatch(segment) && segment is not "." and not "..")
                    {
                        path.Append(segment);
                        pattern.Append(segment);
                        continue;
                    }
                    var match = Parameter.Match(segment);
                    if (!match.Success)
                    {
                        Report(directive, $"Unsupported route segment '{segment}'. Use literal segments, {{name}}, or {{name:int}}.");
                        valid = false;
                        continue;
                    }
                    var name = match.Groups[1].Value;
                    bool integer = match.Groups[2].Success;
                    var candidates = contract.Properties.Where(property =>
                        property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray();
                    if (!seen.Add(name))
                    {
                        Report(directive, $"Route parameter '{name}' is repeated.");
                        valid = false;
                    }
                    else if (candidates is not [{ LocalType.SpecialType: var type } property] ||
                        type != (integer ? SpecialType.System_Int32 : SpecialType.System_String))
                    {
                        var expected = integer ? "int" : "string";
                        Report(directive, $"Route parameter '{name}' requires one public [Parameter] property of type '{expected}'.");
                        valid = false;
                    }
                    else
                    {
                        props.Add(new RouteProp(name, property.Name, integer));
                    }
                    path.Append(':').Append(name);
                    if (integer)
                        path.Append("([+-]?[0-9]+)");
                    pattern.Append(integer ? "{int}" : "{}");
                }
                if (!valid)
                    continue;
                var missing = contract.Properties.Where(property => property.Required && !props.Any(prop => prop.Property == property.Name))
                    .Select(property => property.Name).ToArray();
                if (missing.Length != 0)
                {
                    Report(directive, $"The route does not supply required parameters: {string.Join(", ", missing)}.");
                    continue;
                }
                if (!patterns.Add(pattern.ToString()))
                {
                    Report(directive, $"Route '{authored}' duplicates another route's matching pattern.");
                    continue;
                }
                routes.Add(new Route(path.Length == 0 ? "/" : path.ToString(), page, layoutPath, props));
            }

            void Report(JoltRouteDirective directive, string message) =>
                diagnostics.Add(JoltProjectCompiler.Error("JOLT1043",
                    projection.Location(directive.ArgumentSpan.IsEmpty ? directive.NameSpan : directive.ArgumentSpan), message));
        }

        return Emit(routes);
    }

    private static string Emit(IReadOnlyList<Route> routes)
    {
        var output = new StringBuilder();
        if (routes.Any(route => route.Layout is not null))
            output.Append("import { defineComponent, h } from \"vue\";\n");
        var aliases = routes.SelectMany(route => new[] { route.Page, route.Layout }).OfType<string>()
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)
            .Select((path, index) => (path, alias: $"component{index}"))
            .ToDictionary(pair => pair.path, pair => pair.alias, StringComparer.Ordinal);
        foreach (var (path, alias) in aliases)
            output.Append($"import {alias} from {Quote("./" + path)};\n");
        if (routes.Any(route => route.Props.Any(prop => prop.Integer)))
        {
            // Vue Router decodes the param before computing props. Validate the decoded
            // value before it reaches the C# component as a JavaScript Number.
            output.Append("""
                function routeInt(value) {
                    const number = Number(value);
                    if (!/^[+-]?[0-9]+$/.test(value) || number < -2147483648 || number > 2147483647)
                        throw new RangeError("Invalid Int32 route parameter: " + value);
                    return number || 0;
                }

                """);
        }
        output.Append("export const routes = [\n");
        foreach (var route in routes)
        {
            var component = aliases[route.Page];
            output.Append($"    {{ path: {Quote(route.Path)}, component: ");
            if (route.Layout is { } layout)
                output.Append($"defineComponent({{ inheritAttrs: false, setup(_, {{ attrs }}) {{ return () => h({aliases[layout]}, null, {{ default: () => h({component}, attrs) }}); }} }})");
            else
                output.Append(component);
            if (route.Props.Count != 0)
            {
                var assignments = route.Props.Select(prop =>
                {
                    var value = $"route.params[{Quote(prop.Parameter)}]";
                    return $"[{Quote(prop.Property)}]: {(prop.Integer ? $"routeInt({value})" : value)}";
                });
                output.Append($", props: route => ({{ {string.Join(", ", assignments)} }})");
            }
            output.Append(" },\n");
        }
        return output.Append("];\n").ToString();
    }

    private static string Quote(string value) => JsonSerializer.Serialize(value);
}
