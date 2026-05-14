#!/usr/bin/env dotnet run

using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Collections.Generic;

internal static class Program
{
    private const string CssClassPropertyName = "CssClass";
    private const string CssStylePropertyName = "CssStyle";
    private const string AdditionalAttributesPropertyName = "AdditionalAttributes";
    private const string ChildContentPropertyName = "ChildContent";

    private static void Main()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var packageRoot = Path.Combine(repositoryRoot, "src", "ECMAScript.ElementPlus");
        var webTypesPath = Path.Combine(repositoryRoot, ".tmp", "elementplus-inspect", "package", "web-types.json");

        if (!File.Exists(webTypesPath))
            throw new InvalidOperationException("Missing Element Plus web-types metadata: " + webTypesPath);

        using var webTypes = JsonDocument.Parse(File.ReadAllText(webTypesPath));
        var html = webTypes.RootElement.GetProperty("contributions").GetProperty("html");
        var components = html.GetProperty("vue-components")
            .EnumerateArray()
            .Select(ElementPlusComponentMetadata.FromJson)
            .GroupBy(static component => component.ExportName, StringComparer.Ordinal)
            .Select(static group => ElementPlusComponentMetadata.Merge(group))
            .OrderBy(static component => component.ClassName, StringComparer.Ordinal)
            .ToArray();
        var directives = html.GetProperty("attributes")
            .EnumerateArray()
            .Select(ElementPlusDirectiveMetadata.FromJson)
            .GroupBy(static directive => directive.ExportName, StringComparer.Ordinal)
            .Select(static group => group.First())
            .OrderBy(static directive => directive.ExportName, StringComparer.Ordinal)
            .ToArray();

        WriteFile(
            Path.Combine(packageRoot, "ElementPlusComponentExports.cs"),
            RenderComponentExports(components));

        WriteFile(
            Path.Combine(packageRoot, "ElementPlusComponentRegistry.cs"),
            RenderComponentRegistry(components));

        WriteFile(
            Path.Combine(packageRoot, "ElementPlus.Components.generated.cs"),
            RenderComponentDefinitions(components));

        WriteFile(
            Path.Combine(packageRoot, "ElementPlusDirectiveExports.cs"),
            RenderDirectiveExports(directives));

        WriteFile(
            Path.Combine(packageRoot, "ElementPlusDirectiveRegistry.cs"),
            RenderDirectiveRegistry(directives));

        Console.WriteLine($"Generated {components.Length} Element Plus components and {directives.Length} directives.");
    }

    private static string RenderComponentExports(ElementPlusComponentMetadata[] components)
    {
        var builder = new StringBuilder();
        builder.AppendLine("namespace ECMAScript.ElementPlus;");
        builder.AppendLine();
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// Export surface for generated Element Plus components.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("[ECMAScript(\"element-plus\")]");
        builder.AppendLine("public static class ElementPlusComponents");
        builder.AppendLine("{");

        foreach (var component in components)
        {
            builder.AppendLine($"    [ECMAScriptName(\"{component.ExportName}\")]");
            builder.AppendLine($"    public extern static IElementPlusComponent {component.ClassName} {{ get; }}");
            builder.AppendLine();
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string RenderComponentRegistry(ElementPlusComponentMetadata[] components)
    {
        var builder = new StringBuilder();
        builder.AppendLine("namespace ECMAScript.ElementPlus;");
        builder.AppendLine();
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// Registry of generated Element Plus components.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("[ECMAScript]");
        builder.AppendLine("[Description(\"@#ElementPlusComponentRegistry\")]");
        builder.AppendLine("public sealed record ElementPlusComponentRegistry : VueComponentRegistry");
        builder.AppendLine("{");

        foreach (var component in components)
        {
            builder.AppendLine($"    [Description(\"@#{component.ExportName}\")]");
            builder.AppendLine($"    public IElementPlusComponent? {component.ClassName} {{ get; init; }}");
            builder.AppendLine();
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string RenderComponentDefinitions(ElementPlusComponentMetadata[] components)
    {
        var builder = new StringBuilder();
        builder.AppendLine("using Microsoft.AspNetCore.Components;");
        builder.AppendLine();
        builder.AppendLine("namespace ECMAScript.ElementPlus;");
        builder.AppendLine();
        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("// Generated from .tmp/elementplus-inspect/package/web-types.json");
        builder.AppendLine();

        foreach (var component in components)
        {
            builder.AppendLine("/// <summary>");
            builder.AppendLine($"/// {EscapeXml(component.Description ?? component.TagName)}");
            builder.AppendLine("/// </summary>");
            builder.AppendLine($"[VueLibraryComponent(\"element-plus\", \"{component.ExportName}\")]");
            builder.AppendLine("[VueLibraryStyle(\"element-plus/dist/index.css\")]");
            builder.AppendLine("[VueLibraryPluginRequirement(\"element-plus\")]");
            builder.AppendLine("[VueProp(nameof(CssClass), Name = \"class\")]");
            builder.AppendLine("[VueProp(nameof(CssStyle), Name = \"style\")]");

            foreach (var slot in component.Slots)
            {
                if (slot.IsDefault)
                {
                    builder.AppendLine("[VueSlot(nameof(ChildContent), IsDefault = true)]");
                }
                else
                {
                    builder.AppendLine($"[VueSlot(nameof({slot.PropertyName}), Name = \"{slot.RuntimeName}\")]");
                }
            }

            foreach (var emit in component.Emits)
            {
                builder.AppendLine($"[VueLibraryEmit(nameof({emit.PropertyName}), Name = \"{emit.RuntimeName}\")]");
            }

            builder.AppendLine($"public sealed class {component.ClassName} : {(component.HasDefaultSlot ? "ElementPlusContentComponentBase" : "ElementPlusComponentBase")}");
            builder.AppendLine("{");

            foreach (var prop in component.Props)
            {
                if (prop.IsSkipped)
                    continue;

                builder.AppendLine("    [Parameter]");
                builder.AppendLine($"    public {prop.TypeName} {prop.PropertyName} {{ get; set; }}");
                builder.AppendLine();
            }

            foreach (var slot in component.Slots.Where(static slot => !slot.IsDefault))
            {
                builder.AppendLine("    [Parameter]");
                builder.AppendLine($"    public RenderFragment? {slot.PropertyName} {{ get; set; }}");
                builder.AppendLine();
            }

            foreach (var emit in component.Emits)
            {
                builder.AppendLine("    [Parameter]");
                builder.AppendLine($"    public EventCallback {emit.PropertyName} {{ get; set; }}");
                builder.AppendLine();
            }

            builder.AppendLine("}");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string RenderDirectiveExports(ElementPlusDirectiveMetadata[] directives)
    {
        var builder = new StringBuilder();
        builder.AppendLine("namespace ECMAScript.ElementPlus;");
        builder.AppendLine();
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// Export surface for Element Plus directives.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("[ECMAScript(\"element-plus\")]");
        builder.AppendLine("public static class ElementPlusDirectives");
        builder.AppendLine("{");

        foreach (var directive in directives)
        {
            builder.AppendLine($"    [ECMAScriptName(\"{directive.ExportName}\")]");
            builder.AppendLine($"    public extern static {directive.TypeName} {directive.PropertyName} {{ get; }}");
            builder.AppendLine();
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string RenderDirectiveRegistry(ElementPlusDirectiveMetadata[] directives)
    {
        var builder = new StringBuilder();
        builder.AppendLine("namespace ECMAScript.ElementPlus;");
        builder.AppendLine();
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// Registry of Element Plus directives.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("[ECMAScript]");
        builder.AppendLine("[Description(\"@#ElementPlusDirectiveRegistry\")]");
        builder.AppendLine("public sealed record ElementPlusDirectiveRegistry : VueDirectiveRegistry");
        builder.AppendLine("{");

        foreach (var directive in directives)
        {
            builder.AppendLine($"    [Description(\"@#{directive.PropertyName}\")]");
            builder.AppendLine($"    public {directive.TypeName}? {directive.PropertyName} {{ get; init; }}");
            builder.AppendLine();
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

    private static void WriteFile(string path, string content)
    {
        var normalized = content.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\n", Environment.NewLine, StringComparison.Ordinal);
        File.WriteAllText(path, normalized, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static string ResolveRepositoryRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root from current directory.");
    }

    private static string EscapeXml(string value)
        => value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);

    private static string ToPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var sanitized = value.Trim();
        sanitized = sanitized.Replace("@", "At", StringComparison.Ordinal);
        sanitized = sanitized.Replace(":", " ", StringComparison.Ordinal);
        sanitized = sanitized.Replace("-", " ", StringComparison.Ordinal);
        sanitized = sanitized.Replace("_", " ", StringComparison.Ordinal);
        sanitized = sanitized.Replace("/", " ", StringComparison.Ordinal);
        sanitized = sanitized.Replace(".", " ", StringComparison.Ordinal);
        sanitized = sanitized.Replace("[", " ", StringComparison.Ordinal);
        sanitized = sanitized.Replace("]", " ", StringComparison.Ordinal);

        var parts = sanitized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var builder = new StringBuilder();
        foreach (var part in parts)
        {
            if (part.Length == 0)
                continue;

            if (part.Length == 1)
            {
                builder.Append(char.ToUpperInvariant(part[0]));
                continue;
            }

            if (char.IsDigit(part[0]))
            {
                builder.Append('_').Append(part);
                continue;
            }

            builder.Append(char.ToUpperInvariant(part[0]));
            builder.Append(part[1..]);
        }

        return builder.ToString();
    }

    private static string ToEventPropertyName(string runtimeName)
    {
        var pascalName = ToPascalCase(runtimeName);
        if (pascalName.StartsWith("Update", StringComparison.Ordinal))
            return pascalName + "Event";

        return "On" + pascalName;
    }

    private sealed record ElementPlusComponentMetadata(
        string TagName,
        string ClassName,
        string ExportName,
        string? Description,
        ElementPlusPropMetadata[] Props,
        ElementPlusSlotMetadata[] Slots,
        ElementPlusEmitMetadata[] Emits)
    {
        public bool HasDefaultSlot => Slots.Any(static slot => slot.IsDefault);

        public static ElementPlusComponentMetadata FromJson(JsonElement element)
        {
            var tagName = element.GetProperty("name").GetString()
                          ?? throw new InvalidOperationException("Element Plus component is missing tag name.");
            var exportName = element.TryGetProperty("source", out var source) &&
                             source.TryGetProperty("symbol", out var symbol) &&
                             symbol.ValueKind == JsonValueKind.String
                ? symbol.GetString()!
                : ToPascalCase(tagName);
            var className = exportName;
            var description = element.TryGetProperty("description", out var descriptionElement) && descriptionElement.ValueKind == JsonValueKind.String
                ? descriptionElement.GetString()
                : null;

            var props = element.TryGetProperty("props", out var propsElement)
                ? propsElement.EnumerateArray().Select(ElementPlusPropMetadata.FromJson).ToArray()
                : [];
            var slots = element.TryGetProperty("slots", out var slotsElement)
                ? slotsElement.EnumerateArray().Select(ElementPlusSlotMetadata.FromJson).ToArray()
                : [];
            var emits = element.TryGetProperty("js", out var jsElement) &&
                        jsElement.TryGetProperty("events", out var eventsElement)
                ? eventsElement.EnumerateArray().Select(ElementPlusEmitMetadata.FromJson).ToArray()
                : [];

            return new ElementPlusComponentMetadata(tagName, className, exportName, description, props, slots, emits);
        }

        public static ElementPlusComponentMetadata Merge(IEnumerable<ElementPlusComponentMetadata> components)
        {
            var items = components.ToArray();
            if (items.Length == 0)
                throw new InvalidOperationException("Element Plus component merge received no items.");

            var first = items[0];
            var description = items
                .Select(static item => item.Description)
                .FirstOrDefault(static item => !string.IsNullOrWhiteSpace(item));
            var props = items
                .SelectMany(static item => item.Props)
                .GroupBy(static item => item.RuntimeName, StringComparer.Ordinal)
                .Select(static group => group.First())
                .ToArray();
            var emits = items
                .SelectMany(static item => item.Emits)
                .GroupBy(static item => item.RuntimeName, StringComparer.Ordinal)
                .Select(static group => group.First())
                .ToArray();
            var slots = ResolveSlotPropertyNames(
                props,
                items.SelectMany(static item => item.Slots)
                    .GroupBy(static item => item.RuntimeName, StringComparer.Ordinal)
                    .Select(static group => group.First())
                    .ToArray(),
                emits);

            return new ElementPlusComponentMetadata(
                first.TagName,
                first.ClassName,
                first.ExportName,
                description,
                props,
                slots,
                emits);
        }

        private static ElementPlusSlotMetadata[] ResolveSlotPropertyNames(
            ElementPlusPropMetadata[] props,
            ElementPlusSlotMetadata[] slots,
            ElementPlusEmitMetadata[] emits)
        {
            var occupiedNames = new HashSet<string>(StringComparer.Ordinal)
            {
                CssClassPropertyName,
                CssStylePropertyName,
                AdditionalAttributesPropertyName,
                ChildContentPropertyName
            };

            foreach (var prop in props)
            {
                if (!prop.IsSkipped)
                    occupiedNames.Add(prop.PropertyName);
            }

            foreach (var emit in emits)
                occupiedNames.Add(emit.PropertyName);

            var resolved = new List<ElementPlusSlotMetadata>(slots.Length);
            foreach (var slot in slots)
            {
                if (slot.IsDefault)
                {
                    resolved.Add(slot with { PropertyName = ChildContentPropertyName });
                    continue;
                }

                var basePropertyName = ToPascalCase(slot.RuntimeName);
                var propertyName = GetUniqueSlotPropertyName(basePropertyName, occupiedNames);
                occupiedNames.Add(propertyName);
                resolved.Add(slot with { PropertyName = propertyName });
            }

            return resolved.ToArray();
        }

        private static string GetUniqueSlotPropertyName(string basePropertyName, HashSet<string> occupiedNames)
        {
            var normalizedBaseName = string.IsNullOrWhiteSpace(basePropertyName)
                ? "Slot"
                : basePropertyName;
            if (!occupiedNames.Contains(normalizedBaseName))
                return normalizedBaseName;

            var slotName = normalizedBaseName.EndsWith("Slot", StringComparison.Ordinal)
                ? normalizedBaseName
                : normalizedBaseName + "Slot";
            if (!occupiedNames.Contains(slotName))
                return slotName;

            for (var suffix = 2; ; suffix++)
            {
                var candidate = slotName + suffix;
                if (!occupiedNames.Contains(candidate))
                    return candidate;
            }
        }
    }

    private sealed record ElementPlusPropMetadata(
        string RuntimeName,
        string PropertyName,
        string TypeName,
        bool IsSkipped)
    {
        public static ElementPlusPropMetadata FromJson(JsonElement element)
        {
            var runtimeName = element.GetProperty("name").GetString()
                              ?? throw new InvalidOperationException("Element Plus prop is missing name.");

            if (string.Equals(runtimeName, "class", StringComparison.Ordinal) ||
                string.Equals(runtimeName, "style", StringComparison.Ordinal))
            {
                return new ElementPlusPropMetadata(runtimeName, ToPascalCase(runtimeName), "VueValue?", true);
            }

            if (runtimeName.StartsWith("[", StringComparison.Ordinal) ||
                runtimeName.Contains('/', StringComparison.Ordinal))
            {
                return new ElementPlusPropMetadata(runtimeName, ToPascalCase(runtimeName), "VueValue?", true);
            }

            var propertyName = ToPascalCase(runtimeName);
            var typeName = MapType(element.TryGetProperty("type", out var typeElement) ? typeElement : default);
            return new ElementPlusPropMetadata(runtimeName, propertyName, typeName, false);
        }

        private static string MapType(JsonElement typeElement)
        {
            if (typeElement.ValueKind != JsonValueKind.Array || typeElement.GetArrayLength() == 0)
                return "VueValue?";

            var options = typeElement.EnumerateArray()
                .Select(NormalizeTypeToken)
                .Where(static token => !string.IsNullOrWhiteSpace(token))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (options.Length == 0)
                return "VueValue?";

            if (options.All(static token => token == "boolean"))
                return "bool";

            if (options.All(static token => token == "number"))
                return "Number?";

            if (options.All(static token => token == "string"))
                return "string?";

            if (options.All(static token => token == "Component") ||
                options.SequenceEqual(["string", "Component"], StringComparer.Ordinal) ||
                options.SequenceEqual(["Component", "string"], StringComparer.Ordinal))
            {
                return "VueStringComponentValue?";
            }

            if (options.SequenceEqual(["string", "number"], StringComparer.Ordinal) ||
                options.SequenceEqual(["number", "string"], StringComparer.Ordinal))
            {
                return "ElementPlusStringNumberValue?";
            }

            if (options.All(static token => token is "HTMLElement" or "CSSSelector") &&
                options.Length <= 2)
            {
                return "VueTeleportTarget?";
            }

            if (options.All(static token => token == "string[]"))
                return "ElementPlusStringArray?";

            if (options.Length == 1)
                return MapSingleType(options[0]);

            if (options.Any(static token => token.Contains("=>", StringComparison.Ordinal)))
                return "Delegate?";

            if (options.All(static token => token.StartsWith("'", StringComparison.Ordinal) || token == "string"))
                return "string?";

            if (options.Any(static token => token == "boolean") && options.Any(static token => token == "string"))
                return "VueBooleanStringValue?";

            if (options.Any(static token => token == "VueProps") || options.Any(static token => token == "object"))
                return "VueProps?";

            return "VueValue?";
        }

        private static string MapSingleType(string typeToken)
            => typeToken switch
            {
                "boolean" => "bool",
                "number" => "Number?",
                "string" => "string?",
                "string[]" => "ElementPlusStringArray?",
                "VueProps" => "VueProps?",
                "CSSProperties" => "VueStyleValue?",
                "CSSSelector" => "string?",
                "HTMLElement" => "HTMLElement?",
                "Component" => "IVueComponent?",
                "RouteLocationRaw" => "VueValue?",
                "Headers" => "Headers?",
                "XMLHttpRequest" => "XMLHttpRequest?",
                "File" => "File?",
                "Blob" => "Blob?",
                "Error" => "Error?",
                "Function" => "Delegate?",
                _ when typeToken.Contains("=>", StringComparison.Ordinal) => "Delegate?",
                _ when typeToken.StartsWith("Array<", StringComparison.Ordinal) => MapArrayType(typeToken),
                _ when typeToken.StartsWith("[", StringComparison.Ordinal) => MapTupleType(typeToken),
                _ when typeToken.StartsWith("Record<", StringComparison.Ordinal) => "VueDictionary?",
                _ when typeToken.Contains("Record<", StringComparison.Ordinal) => "VueDictionary?",
                _ when typeToken.Contains("Awaitable", StringComparison.Ordinal) => "VueValue?",
                _ when typeToken.Contains("any", StringComparison.OrdinalIgnoreCase) => "VueValue?",
                _ when typeToken.Contains("object", StringComparison.OrdinalIgnoreCase) => "VueProps?",
                _ => "VueValue?"
            };

        private static string MapArrayType(string typeToken)
        {
            if (string.Equals(typeToken, "Array<string | number>", StringComparison.Ordinal) ||
                string.Equals(typeToken, "Array<number | string>", StringComparison.Ordinal))
            {
                return "VueValue?";
            }

            if (string.Equals(typeToken, "Array<string>", StringComparison.Ordinal))
                return "ElementPlusStringArray?";

            return "VueValue?";
        }

        private static string MapTupleType(string typeToken)
        {
            if (string.Equals(typeToken, "[number, number]", StringComparison.Ordinal))
                return "Number[]?";

            if (string.Equals(typeToken, "[Font]", StringComparison.Ordinal))
                return "VueProps?";

            return "VueValue?";
        }

        private static string NormalizeTypeToken(JsonElement element)
            => element.ValueKind switch
            {
                JsonValueKind.String => NormalizeRawTypeToken(element.GetString() ?? string.Empty),
                JsonValueKind.Object => element.TryGetProperty("name", out var name)
                    ? NormalizeRawTypeToken(name.GetString() ?? string.Empty)
                    : "VueValue",
                _ => "VueValue"
            };

        private static string NormalizeRawTypeToken(string value)
        {
            var token = value.Trim();
            if (string.IsNullOrWhiteSpace(token))
                return string.Empty;

            token = token.Replace(" | ", "|", StringComparison.Ordinal);
            token = Regex.Replace(token, @"\s+", " ");

            return token switch
            {
                "object" => "object",
                "Array" => "Array",
                "Function" => "Function",
                "CSSProperties" => "CSSProperties",
                "Component" => "Component",
                "HTMLElement" => "HTMLElement",
                "CSSSelector" => "CSSSelector",
                "RouteLocationRaw" => "RouteLocationRaw",
                "Headers" => "Headers",
                "XMLHttpRequest" => "XMLHttpRequest",
                "File" => "File",
                "Blob" => "Blob",
                "Error" => "Error",
                _ when token.StartsWith("Array<", StringComparison.Ordinal) => token,
                _ when token.StartsWith("[", StringComparison.Ordinal) => token,
                _ when token.Contains("=>", StringComparison.Ordinal) => token,
                _ when token.StartsWith("Record<", StringComparison.Ordinal) => token,
                _ when token.Contains("Record<", StringComparison.Ordinal) => token,
                _ when token.Contains("Awaitable", StringComparison.Ordinal) => token,
                _ => token
            };
        }
    }

    private sealed record ElementPlusSlotMetadata(
        string RuntimeName,
        string PropertyName,
        bool IsDefault)
    {
        public static ElementPlusSlotMetadata FromJson(JsonElement element)
        {
            var runtimeName = element.GetProperty("name").GetString()
                              ?? throw new InvalidOperationException("Element Plus slot is missing name.");
            var isDefault = string.Equals(runtimeName, "default", StringComparison.Ordinal);
            return new ElementPlusSlotMetadata(
                runtimeName,
                isDefault ? "ChildContent" : ToPascalCase(runtimeName),
                isDefault);
        }
    }

    private sealed record ElementPlusEmitMetadata(
        string RuntimeName,
        string PropertyName)
    {
        public static ElementPlusEmitMetadata FromJson(JsonElement element)
        {
            var runtimeName = element.GetProperty("name").GetString()
                              ?? throw new InvalidOperationException("Element Plus event is missing name.");
            return new ElementPlusEmitMetadata(runtimeName, ToEventPropertyName(runtimeName));
        }
    }

    private sealed record ElementPlusDirectiveMetadata(
        string ExportName,
        string PropertyName,
        string TypeName)
    {
        public static ElementPlusDirectiveMetadata FromJson(JsonElement element)
        {
            var exportName = element.TryGetProperty("source", out var source) &&
                             source.TryGetProperty("symbol", out var symbol) &&
                             symbol.ValueKind == JsonValueKind.String
                ? symbol.GetString()!
                : throw new InvalidOperationException("Element Plus directive is missing export symbol.");

            return exportName switch
            {
                "ElInfiniteScroll" => new ElementPlusDirectiveMetadata(exportName, "InfiniteScroll", "ElementPlusDirective"),
                "ElLoading" => new ElementPlusDirectiveMetadata("ElLoadingDirective", "Loading", "VueDirective<ElementPlusDirectiveValue>"),
                _ => new ElementPlusDirectiveMetadata(exportName, exportName, "ElementPlusDirective")
            };
        }
    }
}
