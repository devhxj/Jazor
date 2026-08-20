using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Generation;

/// <summary>
/// Produces the browser route registry from the same final Razor SG symbols that produce
/// component artifacts. Page authors keep <c>@page</c>/<c>@layout</c>; the generated module is
/// an implementation detail consumed by the standard Router adapter.
/// 从 Razor SG 最终符号生成浏览器路由表，页面作者只保留标准 @page/@layout 形状。
/// </summary>
internal static class RazorVueRouteCatalogBuilder
{
    internal const string RelativePath = "@jazor/vue-runtime/routes.mjs";

    private const string RouteAttributeMetadataName = "Microsoft.AspNetCore.Components.RouteAttribute";
    private const string LayoutAttributeMetadataName = "Microsoft.AspNetCore.Components.LayoutAttribute";
    private const string SupplyParameterFromQueryAttributeMetadataName = "Microsoft.AspNetCore.Components.SupplyParameterFromQueryAttribute";
    private static readonly Regex RouteParameterPattern = new(
        "\\{(?<name>[^}:?]+)(?::[^}?]+)?\\??\\}",
        RegexOptions.CultureInvariant);

    internal static VueModuleArtifact Build(
        GeneratedCSharpBinding binding,
        ImmutableArray<VueModuleArtifact> componentArtifacts)
    {
        var artifactsByComponentId = componentArtifacts.ToDictionary(
            static artifact => artifact.ComponentId,
            StringComparer.Ordinal);
        var routes = new List<RouteDefinition>();
        foreach (var component in binding.Components
                     .OrderBy(static candidate => candidate.ComponentSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), StringComparer.Ordinal))
        {
            var templates = GetRouteTemplates(component.ComponentSymbol);
            if (templates.IsDefaultOrEmpty)
                continue;

            if (!artifactsByComponentId.TryGetValue(component.ComponentSymbol.ToDisplayString(), out var pageArtifact))
            {
                throw new InvalidOperationException(
                    "RazorVue route page '" +
                    component.ComponentSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) +
                    "' has no generated component artifact.");
            }

            var layoutType = GetLayoutType(component.ComponentSymbol);
            VueModuleArtifact? layoutArtifact = null;
            if (layoutType is not null &&
                !artifactsByComponentId.TryGetValue(layoutType.ToDisplayString(), out layoutArtifact))
            {
                throw new InvalidOperationException(
                    "RazorVue route page '" +
                    component.ComponentSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) +
                    "' declares layout '" +
                    layoutType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) +
                    "', but that layout has no generated component artifact.");
            }

            foreach (var template in templates)
            {
                routes.Add(new RouteDefinition(
                    template,
                    pageArtifact,
                    layoutArtifact,
                    BuildRouteParameters(component.ComponentSymbol, template),
                    BuildQueryParameters(component.ComponentSymbol)));
            }
        }

        routes.Sort(RouteDefinitionComparer.Instance);
        var moduleText = BuildModuleText(routes);
        var sourceMapContent = "{\"version\":3,\"file\":\"routes.mjs\",\"sources\":[],\"names\":[],\"mappings\":\"\"}";
        var contentHash = ComputeContentHash(moduleText);
        return new VueModuleArtifact(
            "Jazor.Generated.RazorVue.RouteCatalog",
            RelativePath,
            moduleText,
            contentHash,
            RelativePath + ".map",
            sourceMapContent,
            ComputeContentHash(sourceMapContent),
            ImmutableArray<string>.Empty,
            ImmutableArray<VueAsset>.Empty,
            new VueHmrMetadata(
                "jazor-route-catalog:" + RelativePath,
                contentHash,
                contentHash,
                contentHash,
                VueHmrBoundaryKind.FullReloadRequired));
    }

    private static string BuildModuleText(IReadOnlyList<RouteDefinition> routes)
    {
        var imports = routes
            .SelectMany(static route => route.LayoutArtifact is null
                ? new[] { route.PageArtifact }
                : new[] { route.PageArtifact, route.LayoutArtifact! })
            .GroupBy(static artifact => artifact.RelativePath, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .OrderBy(static artifact => artifact.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static artifact => artifact.ComponentId, StringComparer.Ordinal)
            .ToArray();
        var aliases = imports
            .Select(static (artifact, index) => new KeyValuePair<string, string>(
                artifact.RelativePath,
                "routeComponent" + index.ToString(System.Globalization.CultureInfo.InvariantCulture)))
            .ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated/> RazorVue standard Blazor route catalog.");
        foreach (var artifact in imports)
        {
            builder.Append("import ")
                .Append(aliases[artifact.RelativePath])
                .Append(" from ")
                .Append(JavaScriptString(GetRelativeImport(RelativePath, artifact.RelativePath)))
                .AppendLine(";");
        }

        if (imports.Length > 0)
            builder.AppendLine();

        builder.AppendLine("export const routes = [");
        foreach (var route in routes)
        {
            builder.Append("  { template: ")
                .Append(JavaScriptString(route.Template))
                .Append(", component: ")
                .Append(aliases[route.PageArtifact.RelativePath])
                .Append(", layout: ")
                .Append(route.LayoutArtifact is null ? "null" : aliases[route.LayoutArtifact.RelativePath])
                .Append(", parameters: ");
            AppendRouteParameters(builder, route.Parameters);
            builder.Append(", queries: ");
            AppendRouteParameters(builder, route.Queries);
            builder.AppendLine(" },");
        }

        builder.AppendLine("];\n");
        return builder.ToString();
    }

    private static void AppendRouteParameters(StringBuilder builder, ImmutableArray<RouteParameter> parameters)
    {
        builder.Append('[');
        for (var index = 0; index < parameters.Length; index++)
        {
            if (index > 0)
                builder.Append(", ");

            var parameter = parameters[index];
            builder.Append("{ name: ")
                .Append(JavaScriptString(parameter.Name))
                .Append(", prop: ")
                .Append(JavaScriptString(parameter.PropName))
                .Append(", kind: ")
                .Append(JavaScriptString(parameter.Kind))
                .Append(" }");
        }

        builder.Append(']');
    }

    private static ImmutableArray<string> GetRouteTemplates(INamedTypeSymbol component)
        => component.GetAttributes()
            .Where(static attribute => string.Equals(
                attribute.AttributeClass?.OriginalDefinition.ToDisplayString(),
                RouteAttributeMetadataName,
                StringComparison.Ordinal))
            .Select(static attribute => attribute.ConstructorArguments.Length > 0
                ? attribute.ConstructorArguments[0].Value as string
                : null)
            .Where(static template => !string.IsNullOrWhiteSpace(template))
            .Select(static template => template!)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static template => template, StringComparer.Ordinal)
            .ToImmutableArray();

    private static INamedTypeSymbol? GetLayoutType(INamedTypeSymbol component)
    {
        var attribute = component.GetAttributes().FirstOrDefault(static candidate => string.Equals(
            candidate.AttributeClass?.OriginalDefinition.ToDisplayString(),
            LayoutAttributeMetadataName,
            StringComparison.Ordinal));
        return attribute is { ConstructorArguments.Length: > 0 } &&
               attribute.ConstructorArguments[0].Value is INamedTypeSymbol layout
            ? layout
            : null;
    }

    private static ImmutableArray<RouteParameter> BuildRouteParameters(
        INamedTypeSymbol component,
        string template)
    {
        var properties = LibraryComponentConventions
            .GetEffectiveParameterProperties(component)
            .ToDictionary(static property => property.Name, StringComparer.OrdinalIgnoreCase);
        var parameters = ImmutableArray.CreateBuilder<RouteParameter>();
        var claimedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in RouteParameterPattern.Matches(template))
        {
            var routeName = match.Groups["name"].Value.TrimStart('*');
            if (routeName.Length == 0 || !claimedNames.Add(routeName) ||
                !properties.TryGetValue(routeName, out var property))
            {
                continue;
            }

            parameters.Add(new RouteParameter(
                routeName,
                LibraryComponentConventions.GetPropRuntimeName(property),
                GetRouteValueKind(property.Type)));
        }

        return parameters.ToImmutable();
    }

    private static ImmutableArray<RouteParameter> BuildQueryParameters(INamedTypeSymbol component)
    {
        var parameters = ImmutableArray.CreateBuilder<RouteParameter>();
        var claimedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var current = component; current is not null; current = current.BaseType)
        {
            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (!claimedNames.Add(property.Name))
                    continue;

                var attribute = property.GetAttributes().FirstOrDefault(static candidate => string.Equals(
                    candidate.AttributeClass?.OriginalDefinition.ToDisplayString(),
                    SupplyParameterFromQueryAttributeMetadataName,
                    StringComparison.Ordinal));
                if (attribute is null)
                    continue;

                var queryName = GetNamedString(attribute, "Name") ?? property.Name;
                parameters.Add(new RouteParameter(
                    queryName,
                    LibraryComponentConventions.GetPropRuntimeName(property),
                    GetRouteValueKind(property.Type)));
            }
        }

        return parameters
            .OrderBy(static parameter => parameter.Name, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    private static string? GetNamedString(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.Ordinal) && argument.Value.Value is string value)
                return value;
        }

        return null;
    }

    private static string GetRouteValueKind(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
            type = nullable.TypeArguments[0];

        return type.SpecialType switch
        {
            SpecialType.System_SByte or
            SpecialType.System_Byte or
            SpecialType.System_Int16 or
            SpecialType.System_UInt16 or
            SpecialType.System_Int32 or
            SpecialType.System_UInt32 or
            SpecialType.System_Int64 or
            SpecialType.System_UInt64 or
            SpecialType.System_Single or
            SpecialType.System_Double or
            SpecialType.System_Decimal => "number",
            SpecialType.System_Boolean => "boolean",
            _ when type.TypeKind == TypeKind.Enum => "number",
            _ => "string"
        };
    }

    private static string GetRelativeImport(string fromPath, string targetPath)
    {
        var source = new Uri("https://jazor.invalid/" + fromPath.Replace('\\', '/'), UriKind.Absolute);
        var target = new Uri("https://jazor.invalid/" + targetPath.Replace('\\', '/'), UriKind.Absolute);
        var relative = Uri.UnescapeDataString(source.MakeRelativeUri(target).ToString());
        return relative.StartsWith(".", StringComparison.Ordinal) ? relative : "./" + relative;
    }

    private static string JavaScriptString(string value)
    {
        var builder = new StringBuilder(value.Length + 2);
        builder.Append('"');
        foreach (var character in value)
        {
            builder.Append(character switch
            {
                '\\' => "\\\\",
                '"' => "\\\"",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                _ when character < ' ' => "\\u" + ((int)character).ToString("x4", System.Globalization.CultureInfo.InvariantCulture),
                _ => character.ToString()
            });
        }

        builder.Append('"');
        return builder.ToString();
    }

    private static string ComputeContentHash(string content)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        var builder = new StringBuilder(hash.Length * 2 + 7);
        builder.Append("sha256:");
        foreach (var value in hash)
            builder.Append(value.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
        return builder.ToString();
    }

    private sealed record RouteDefinition(
        string Template,
        VueModuleArtifact PageArtifact,
        VueModuleArtifact? LayoutArtifact,
        ImmutableArray<RouteParameter> Parameters,
        ImmutableArray<RouteParameter> Queries);

    private sealed record RouteParameter(string Name, string PropName, string Kind);

    private sealed class RouteDefinitionComparer : IComparer<RouteDefinition>
    {
        internal static readonly RouteDefinitionComparer Instance = new();

        public int Compare(RouteDefinition? left, RouteDefinition? right)
        {
            if (ReferenceEquals(left, right))
                return 0;
            if (left is null)
                return -1;
            if (right is null)
                return 1;

            var template = string.Compare(left.Template, right.Template, StringComparison.Ordinal);
            return template != 0
                ? template
                : string.Compare(left.PageArtifact.ComponentId, right.PageArtifact.ComponentId, StringComparison.Ordinal);
        }
    }
}
