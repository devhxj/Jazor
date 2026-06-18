using System.Collections.Immutable;
using System.Text;
using Jazor.RazorVue.Descriptor;

namespace Jazor.RazorVue.Lowering;

internal sealed partial class RazorVueArtifactFactory
{
    internal static ImmutableArray<string> BuildImportsForCanonicalization(
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        ImmutableArray<RazorVueCompilerImportBinding> compilerImports)
        => BuildImports(resolvedComponents, compilerImports);

    internal static ImmutableArray<string> BuildStylesForCanonicalization(
        VueComponentDescriptor descriptor,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
        => BuildStyles(descriptor, resolvedComponents);

    internal static ImmutableArray<string> BuildPluginRequirementsForCanonicalization(
        VueComponentDescriptor descriptor,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
        => BuildPluginRequirements(descriptor, resolvedComponents);

    internal static string NormalizeRelativePathForCanonicalization(string relativePath)
        => NormalizeRelativePath(relativePath);

    private static ImmutableArray<string> BuildImports(
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        ImmutableArray<RazorVueCompilerImportBinding> compilerImports)
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        builder.Add("vue");
        builder.AddRange(RazorVueCompilerImportFormatter.CollectImportSources(compilerImports));

        // Host-facing artifacts should carry declared dependency specifiers rather
        // than local alias names generated during lowering.
        builder.AddRange(
            resolvedComponents.Values
                .Where(static descriptor => !VueCascadingValueProviderDescriptor.IsProviderDescriptor(descriptor))
                .Select(static descriptor => descriptor.ImportSpecifier)
                .Where(static importSpecifier => !string.Equals(importSpecifier, "vue", StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal));
        return builder.Distinct(StringComparer.Ordinal).ToImmutableArray();
    }

    private static ImmutableArray<string> BuildStyles(
        VueComponentDescriptor descriptor,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var style in descriptor.StyleDependencies)
        {
            if (!string.IsNullOrWhiteSpace(style) && seen.Add(style))
                builder.Add(style);
        }

        foreach (var component in resolvedComponents.Values)
        {
            foreach (var style in component.StyleDependencies)
            {
                if (!string.IsNullOrWhiteSpace(style) && seen.Add(style))
                    builder.Add(style);
            }
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<string> BuildPluginRequirements(
        VueComponentDescriptor descriptor,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var requirement in descriptor.PluginRequirements)
        {
            if (!string.IsNullOrWhiteSpace(requirement) && seen.Add(requirement))
                builder.Add(requirement);
        }

        foreach (var component in resolvedComponents.Values)
        {
            foreach (var requirement in component.PluginRequirements)
            {
                if (!string.IsNullOrWhiteSpace(requirement) && seen.Add(requirement))
                    builder.Add(requirement);
            }
        }

        return builder.ToImmutable();
    }

    private static void AppendComponentImports(StringBuilder builder, ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        var groups = resolvedComponents
            .Where(static pair => !VueCascadingValueProviderDescriptor.IsProviderDescriptor(pair.Value) &&
                                  !string.Equals(pair.Value.ImportSpecifier, "vue", StringComparison.Ordinal))
            .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
            .GroupBy(static pair => pair.Value.ImportSpecifier, StringComparer.Ordinal);

        foreach (var group in groups)
        {
            AppendGroupedComponentImports(builder, group.Key, group.ToImmutableArray());
        }
    }

    private static void AppendGroupedComponentImports(
        StringBuilder builder,
        string importSpecifier,
        ImmutableArray<KeyValuePair<string, VueComponentDescriptor>> components)
    {
        var namedImports = components
            .Where(static item => item.Value.SourceKind == VueComponentSourceKind.LibraryComponent &&
                                  !string.Equals(item.Value.ExportName, "default", StringComparison.Ordinal))
            .Select(static item => item.Value.ExportName + " as " + CreateComponentAlias(item.Key))
            .ToImmutableArray();

        foreach (var item in components)
        {
            if (item.Value.SourceKind == VueComponentSourceKind.LibraryComponent &&
                !string.Equals(item.Value.ExportName, "default", StringComparison.Ordinal))
            {
                continue;
            }

            AppendDefaultComponentImport(builder, item.Key, importSpecifier);
        }

        if (!namedImports.IsDefaultOrEmpty)
        {
            // Aggregate named library exports from the same package into one import
            // so generated modules stay compact while preserving local aliases.
            builder.Append("import { ");
            builder.Append(string.Join(", ", namedImports));
            builder.Append(" } from ");
            builder.Append(ToJavaScriptString(importSpecifier));
            builder.AppendLine(";");
        }
    }

    private static void AppendDefaultComponentImport(
        StringBuilder builder,
        string componentName,
        string importSpecifier)
    {
        var alias = CreateComponentAlias(componentName);

        builder.Append("import ");
        builder.Append(alias);
        builder.Append(" from ");
        builder.Append(ToJavaScriptString(importSpecifier));
        builder.AppendLine(";");
    }


    private static string NormalizeRelativePath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized.Substring(2);

        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidOperationException("RazorVue artifact relative path cannot be empty.");

        if (!normalized.EndsWith(".js", StringComparison.OrdinalIgnoreCase) &&
            !normalized.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase))
        {
            normalized += ".mjs";
        }

        return normalized;
    }

    private static string ToLowerCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length == 1)
            return char.ToLowerInvariant(value[0]).ToString();

        if (char.IsUpper(value[0]) && char.IsUpper(value[1]))
            return value;

        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }
}
