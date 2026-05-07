using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jazor.RazorVue.Lowering;

internal static class RazorVueCompilerImportFormatter
{
    public static ImmutableArray<string> CollectImportSources(ImmutableArray<RazorVueCompilerImportBinding> imports)
        => imports
            .Select(static import => import.ModulePath)
            .Distinct(StringComparer.Ordinal)
            .ToImmutableArray();

    public static void AppendImportStatements(StringBuilder builder, ImmutableArray<RazorVueCompilerImportBinding> imports)
    {
        foreach (var group in imports
                     .OrderBy(static import => import.ModulePath, StringComparer.Ordinal)
                     .GroupBy(static import => import.ModulePath, StringComparer.Ordinal))
        {
            var defaultBinding = group
                .Where(static import => import.Kind == RazorVueCompilerImportKind.Default)
                .Select(static import => import.LocalName)
                .FirstOrDefault();
            var namespaceBinding = group
                .Where(static import => import.Kind == RazorVueCompilerImportKind.Namespace)
                .Select(static import => import.LocalName)
                .FirstOrDefault();
            var namedBindings = group
                .Where(static import => import.Kind == RazorVueCompilerImportKind.Named)
                .OrderBy(static import => import.ImportedName, StringComparer.Ordinal)
                .ThenBy(static import => import.LocalName, StringComparer.Ordinal)
                .Select(static import =>
                    string.Equals(import.ImportedName, import.LocalName, StringComparison.Ordinal)
                        ? import.LocalName
                        : import.ImportedName + " as " + import.LocalName)
                .ToArray();

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(defaultBinding))
                parts.Add(defaultBinding!);

            if (!string.IsNullOrWhiteSpace(namespaceBinding))
                parts.Add("* as " + namespaceBinding);

            if (namedBindings.Length > 0)
                parts.Add("{ " + string.Join(", ", namedBindings) + " }");

            if (parts.Count == 0)
                continue;

            builder.Append("import ")
                .Append(string.Join(", ", parts))
                .Append(" from ")
                .Append(ToJavaScriptString(group.Key))
                .AppendLine(";");
        }
    }

    private static string ToJavaScriptString(string value)
        => "\"" + (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n") + "\"";
}
