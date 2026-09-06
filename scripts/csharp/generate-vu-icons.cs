#!/usr/bin/env dotnet run

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

var options = GeneratorOptions.Parse(args);
var sourceRoot = Path.GetFullPath(options.Source);
var outputRoot = Path.GetFullPath(options.Output);
var packagePath = Path.Combine(sourceRoot, "package.json");
var iconListPath = Path.Combine(sourceRoot, "dist", "icons.json");
var iconDataPath = Path.Combine(sourceRoot, "dist", "icons-data.js");
var webTypesPath = Path.Combine(sourceRoot, "dist", "web-types.json");

foreach (var path in new[] { packagePath, iconListPath, iconDataPath, webTypesPath, Path.Combine(sourceRoot, "LICENSE") })
{
    if (!File.Exists(path))
        throw new FileNotFoundException("vu-icons source artifact is incomplete.", path);
}

using var package = JsonDocument.Parse(File.ReadAllText(packagePath));
var version = package.RootElement.GetProperty("version").GetString()
    ?? throw new InvalidOperationException("vu-icons package.json does not contain a version.");
var componentNames = ReadComponentNames(iconListPath);
var iconData = ReadIconData(iconDataPath);
var documentation = ReadDocumentation(webTypesPath);

if (componentNames.Length != componentNames.Distinct(StringComparer.Ordinal).Count())
    throw new InvalidOperationException("vu-icons component names must be unique.");

var icons = componentNames
    .Select(componentName => ReadIcon(sourceRoot, componentName, iconData, documentation))
    .OrderBy(static icon => icon.ComponentName, StringComparer.Ordinal)
    .ToArray();
var usedIconNames = icons.Select(static icon => icon.IconName).ToHashSet(StringComparer.Ordinal);
if (!usedIconNames.SetEquals(iconData.Keys))
    throw new InvalidOperationException("Wrapper components and iconData entries must have an exact one-to-one mapping.");

var componentsDirectory = Path.Combine(outputRoot, "dist", "components");
Directory.CreateDirectory(componentsDirectory);
Directory.CreateDirectory(Path.Combine(outputRoot, "licenses"));
foreach (var path in Directory.EnumerateFiles(componentsDirectory, "*.mjs"))
    File.Delete(path);

WriteFile(Path.Combine(outputRoot, "Types", "VuIconName.generated.cs"), GenerateIconNames(version, icons));
WriteFile(Path.Combine(outputRoot, "Components", "VuIcons.generated.cs"), GenerateComponents(icons));
foreach (var icon in icons)
    WriteFile(Path.Combine(componentsDirectory, icon.ComponentName + ".mjs"), GenerateStaticIconModule(icon));

WriteFile(Path.Combine(outputRoot, "dist", "jazor-vu-icon-runtime.mjs"), GetRuntimeModule());
WriteFile(Path.Combine(outputRoot, "dist", "jazor-vu-icon.mjs"), GetDynamicIconModule());
WriteFile(Path.Combine(outputRoot, "dist", "jazor-vu-icon.css"), GetIconStyleSheet());
File.Copy(iconDataPath, Path.Combine(outputRoot, "dist", "icons-data.js"), overwrite: true);
File.Copy(Path.Combine(sourceRoot, "LICENSE"), Path.Combine(outputRoot, "licenses", "VU-ICONS-LICENSE"), overwrite: true);
WriteFile(Path.Combine(outputRoot, "manifest.json"), GenerateManifest(version, icons, outputRoot));

Console.WriteLine($"Generated {icons.Length} vu-icons bindings for {version}.");

static IReadOnlyDictionary<string, IconData> ReadIconData(string path)
{
    var content = File.ReadAllText(path);
    var entries = Regex.Matches(
        content,
        """^\s*"(?<name>[^"]+)": \{ viewBox: "(?<viewBox>[^"]+)", content: '(?<content>(?:\\.|[^'])*)', stroke: (?<stroke>true|false) \},?$""",
        RegexOptions.Multiline | RegexOptions.CultureInvariant);

    var result = new Dictionary<string, IconData>(StringComparer.Ordinal);
    foreach (Match entry in entries)
    {
        var name = entry.Groups["name"].Value;
        if (!result.TryAdd(name, new IconData(entry.Groups["viewBox"].Value, entry.Groups["content"].Value)))
            throw new InvalidOperationException($"Duplicate iconData entry '{name}'.");
    }

    if (result.Count == 0)
        throw new InvalidOperationException("Could not parse vu-icons iconData entries.");

    return result;
}

static string[] ReadComponentNames(string path)
{
    using var document = JsonDocument.Parse(File.ReadAllText(path));
    if (document.RootElement.ValueKind != JsonValueKind.Array)
        throw new InvalidOperationException("vu-icons component names must be a JSON array.");

    return document.RootElement
        .EnumerateArray()
        .Select(static item => item.GetString()
            ?? throw new InvalidOperationException("vu-icons component names must be strings."))
        .ToArray();
}

static IReadOnlyDictionary<string, IconDocumentation> ReadDocumentation(string path)
{
    using var document = JsonDocument.Parse(File.ReadAllText(path));
    var result = new Dictionary<string, IconDocumentation>(StringComparer.Ordinal);
    var tags = document.RootElement.GetProperty("contributions").GetProperty("html").GetProperty("tags");
    foreach (var tag in tags.EnumerateArray())
    {
        var name = tag.GetProperty("name").GetString()
            ?? throw new InvalidOperationException("vu-icons web-types tag has no name.");
        var description = tag.TryGetProperty("description", out var descriptionElement)
            ? descriptionElement.GetString()
            : null;
        var props = new Dictionary<string, string>(StringComparer.Ordinal);
        if (tag.TryGetProperty("props", out var propElements))
        {
            foreach (var prop in propElements.EnumerateArray())
            {
                var propName = prop.GetProperty("name").GetString();
                var propDescription = prop.TryGetProperty("description", out var propDescriptionElement)
                    ? propDescriptionElement.GetString()
                    : null;
                if (!string.IsNullOrWhiteSpace(propName) && !string.IsNullOrWhiteSpace(propDescription))
                    props[propName] = propDescription!;
            }
        }

        result[name] = new IconDocumentation(description, props);
    }

    return result;
}

static Icon ReadIcon(
    string sourceRoot,
    string componentName,
    IReadOnlyDictionary<string, IconData> iconData,
    IReadOnlyDictionary<string, IconDocumentation> documentation)
{
    if (!componentName.StartsWith("Vu", StringComparison.Ordinal) || componentName.Length == 2)
        throw new InvalidOperationException($"Unexpected vu-icons component name '{componentName}'.");

    var sourcePath = Path.Combine(sourceRoot, "dist", "vue3", componentName + ".vue");
    if (!File.Exists(sourcePath))
        throw new FileNotFoundException($"Could not find wrapper source for {componentName}.", sourcePath);

    var source = File.ReadAllText(sourcePath);
    var iconName = Regex.Match(source, "icon=\"(?<name>[^\"]+)\"", RegexOptions.CultureInvariant).Groups["name"].Value;
    if (string.IsNullOrWhiteSpace(iconName) || !iconData.TryGetValue(iconName, out var data))
        throw new InvalidOperationException($"Could not resolve icon data for {componentName}.");

    if (!documentation.TryGetValue(componentName, out var docs))
        throw new InvalidOperationException($"Could not resolve web-types documentation for {componentName}.");

    return new Icon(componentName, componentName[2..], iconName, data, docs);
}

static string GenerateIconNames(string version, IReadOnlyList<Icon> icons)
{
    var builder = new StringBuilder();
    builder.AppendLine("// <auto-generated />");
    builder.AppendLine("namespace ECMAScript.VuIcons;");
    builder.AppendLine();
    builder.AppendLine($"/// <summary>vu-icons {version} 的全部 icon 名称。Runtime token 由 Description 固定。</summary>");
    builder.AppendLine("[String]");
    builder.AppendLine("public enum VuIconName");
    builder.AppendLine("{");

    for (var index = 0; index < icons.Count; index++)
    {
        var icon = icons[index];
        builder.Append("    [Description(\"@#").Append(icon.IconName).AppendLine("\")]");
        builder.Append("    ").Append(icon.EnumMember);
        builder.AppendLine(index == icons.Count - 1 ? string.Empty : ",");
        if (index != icons.Count - 1)
            builder.AppendLine();
    }

    builder.AppendLine("}");
    return builder.ToString();
}

static string GenerateComponents(IReadOnlyList<Icon> icons)
{
    var builder = new StringBuilder();
    builder.AppendLine("// <auto-generated />");
    builder.AppendLine("namespace ECMAScript.VuIcons;");
    builder.AppendLine();
    builder.AppendLine("// One descriptor per upstream wrapper keeps static icon usage on a one-icon ESM path.");

    for (var index = 0; index < icons.Count; index++)
    {
        var icon = icons[index];
        builder.Append("/// <summary>").Append(XmlText(icon.Documentation.Description ?? icon.ComponentName)).AppendLine("</summary>");
        builder.Append("/// <remarks>按需 static renderer for upstream <c>")
            .Append(icon.ComponentName)
            .AppendLine("</c>; only its SVG module is materialized.</remarks>");
        builder.Append("[ECMAScript(\"vu-icons/").Append(icon.ComponentName).Append("\", Transform.Component, \"").Append(icon.ComponentName).AppendLine("\")]");
        builder.Append("public sealed class ").Append(icon.ComponentName).AppendLine(" : VuIconComponentBase;");
        if (index != icons.Count - 1)
            builder.AppendLine();
    }

    return builder.ToString();
}

static string GenerateStaticIconModule(Icon icon)
    => "import { createVuIcon } from \"../jazor-vu-icon-runtime.mjs\";\n\n" +
       "export const " + icon.ComponentName + " = createVuIcon(\"" + icon.ComponentName + "\", \"" + icon.Data.ViewBox + "\", '" + icon.Data.Content + "');\n";

static string GenerateManifest(string version, IReadOnlyList<Icon> icons, string outputRoot)
{
    using var stream = new MemoryStream();
    using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
    {
        // Stable writer order keeps the package manifest deterministic without reflection serialization.
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", 2);
        writer.WriteString("libraryId", "vu-icons");
        writer.WriteString("version", version);
        writer.WritePropertyName("imports");
        writer.WriteStartObject();
        WriteManifestImport(
            writer,
            "vu-icons",
            "dist/jazor-vu-icon.mjs",
            outputRoot,
            ["vue"],
            ["dist/icons-data.js", "dist/jazor-vu-icon-runtime.mjs"],
            [
                ("dist/icons-data.js", "module"),
                ("dist/jazor-vu-icon-runtime.mjs", "module")
            ]);
        foreach (var icon in icons)
        {
            WriteManifestImport(
                writer,
                "vu-icons/" + icon.ComponentName,
                "dist/components/" + icon.ComponentName + ".mjs",
                outputRoot,
                ["vue"],
                ["dist/jazor-vu-icon-runtime.mjs"],
                [("dist/jazor-vu-icon-runtime.mjs", "module")]);
        }

        writer.WriteEndObject();
        writer.WritePropertyName("requires");
        writer.WriteStartObject();
        writer.WriteString("vue3", "^3.2.0");
        writer.WriteEndObject();
        writer.WritePropertyName("styles");
        writer.WriteStartArray();
        writer.WriteStartObject();
        writer.WriteString("type", "style");
        writer.WriteString("path", "dist/jazor-vu-icon.css");
        writer.WriteString("hash", ComputeHash(outputRoot, "dist/jazor-vu-icon.css"));
        writer.WriteEndObject();
        writer.WriteEndArray();
        writer.WritePropertyName("files");
        writer.WriteStartArray();
        writer.WriteStartObject();
        writer.WriteString("type", "license");
        writer.WriteString("path", "licenses/VU-ICONS-LICENSE");
        writer.WriteString("hash", ComputeHash(outputRoot, "licenses/VU-ICONS-LICENSE"));
        writer.WriteEndObject();
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    return Encoding.UTF8.GetString(stream.ToArray()) + Environment.NewLine;
}

static void WriteManifestImport(
    Utf8JsonWriter writer,
    string importSpecifier,
    string entryPath,
    string outputRoot,
    IReadOnlyList<string> packageDependencies,
    IReadOnlyList<string> moduleDependencies,
    IReadOnlyList<(string Path, string Type)> files)
{
    writer.WriteStartObject(importSpecifier);
    writer.WriteString("type", "module");
    writer.WriteString("development", entryPath);
    writer.WriteString("production", entryPath);
    writer.WriteString("developmentHash", ComputeHash(outputRoot, entryPath));
    writer.WriteString("productionHash", ComputeHash(outputRoot, entryPath));
    writer.WritePropertyName("developmentDependencies");
    writer.WriteStartArray();
    foreach (var dependency in packageDependencies)
        writer.WriteStringValue(dependency);
    writer.WriteEndArray();
    writer.WritePropertyName("productionDependencies");
    writer.WriteStartArray();
    foreach (var dependency in packageDependencies)
        writer.WriteStringValue(dependency);
    writer.WriteEndArray();
    writer.WritePropertyName("developmentModuleDependencies");
    writer.WriteStartArray();
    foreach (var dependency in moduleDependencies)
        writer.WriteStringValue(dependency);
    writer.WriteEndArray();
    writer.WritePropertyName("productionModuleDependencies");
    writer.WriteStartArray();
    foreach (var dependency in moduleDependencies)
        writer.WriteStringValue(dependency);
    writer.WriteEndArray();
    writer.WritePropertyName("files");
    writer.WriteStartArray();
    foreach (var file in files)
    {
        writer.WriteStartObject();
        writer.WriteString("type", file.Type);
        writer.WriteString("path", file.Path);
        writer.WriteString("hash", ComputeHash(outputRoot, file.Path));
        if (file.Type is "module" or "source-map")
            writer.WriteString("moduleId", file.Path);
        writer.WriteEndObject();
    }
    writer.WriteEndArray();
    writer.WriteEndObject();
}

static string ComputeHash(string root, string relativePath)
    => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(
        root,
        relativePath.Replace('/', Path.DirectorySeparatorChar))))).ToLowerInvariant();

static void WriteFile(string path, string content)
{
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, content.Replace("\r\n", "\n"));
}

static string GetRuntimeModule()
    => """
import { defineComponent, h } from "vue";

function toPixelSize(size) {
    return typeof size === "number" || !isNaN(Number(size)) ? Number(size) : 24;
}

function toMaskData(viewBox, content) {
    const svgContent = content.replace(/currentColor/g, "#000000");
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="${viewBox}">${svgContent}</svg>`;
    return "data:image/svg+xml," + encodeURIComponent(svg);
}

export function createIconStyle(size, color, maskData) {
    const pixelSize = toPixelSize(size) + "px";
    return {
        "--vu-icon-size": pixelSize,
        "--vu-icon-color": color === "currentColor" ? "#333333" : color,
        "--vu-icon-mask": "url(" + maskData + ")"
    };
}

export function createVuIcon(componentName, viewBox, content) {
    const maskData = toMaskData(viewBox, content);

    return defineComponent({
        name: componentName,
        inheritAttrs: false,
        props: {
            size: { type: [Number, String], default: 24 },
            color: { type: String, default: "currentColor" },
            className: { type: String, default: "" },
            spin: { type: Boolean, default: false }
        },
        setup(props, { attrs }) {
            return () => h("div", {
                ...attrs,
                class: [attrs.class, props.className, "vu-icon", props.spin && "vu-icon-spin"],
                style: [attrs.style, createIconStyle(props.size, props.color, maskData)]
            });
        }
    });
}
""";

static string GetDynamicIconModule()
    => """
import { computed, defineComponent, h } from "vue";
import { iconData } from "./icons-data.js";
import { createIconStyle } from "./jazor-vu-icon-runtime.mjs";

export const VuIcon = defineComponent({
    name: "VuIcon",
    inheritAttrs: false,
    props: {
        name: { type: String, default: "" },
        icon: { type: String, default: "" },
        size: { type: [Number, String], default: 24 },
        color: { type: String, default: "currentColor" },
        spin: { type: Boolean, default: false }
    },
    setup(props, { attrs }) {
        const iconName = computed(() => props.name || props.icon);
        const maskData = computed(() => {
            const data = iconData[iconName.value];
            if (!data) return "";

            const svgContent = data.content.replace(/currentColor/g, "#000000");
            const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="${data.viewBox}">${svgContent}</svg>`;
            return "data:image/svg+xml," + encodeURIComponent(svg);
        });

        return () => h("div", {
            ...attrs,
            class: [attrs.class, "vu-icon", props.spin && "vu-icon-spin"],
            style: [attrs.style, createIconStyle(props.size, props.color, maskData.value)]
        });
    }
});
""";

static string GetIconStyleSheet()
    => """
.vu-icon {
    display: inline-block;
    vertical-align: middle;
    width: var(--vu-icon-size, 24px);
    height: var(--vu-icon-size, 24px);
    background-color: var(--vu-icon-color, #333333);
    -webkit-mask-image: var(--vu-icon-mask, none);
    mask-image: var(--vu-icon-mask, none);
    -webkit-mask-size: 100% 100%;
    mask-size: 100% 100%;
    -webkit-mask-repeat: no-repeat;
    mask-repeat: no-repeat;
    -webkit-mask-position: center;
    mask-position: center;
}

.vu-icon-spin {
    animation: vu-spin 1s linear infinite;
}

@keyframes vu-spin {
    from { transform: rotate(0deg); }
    to { transform: rotate(360deg); }
}
""";

static string XmlText(string value)
    => value.Replace("&", "&amp;", StringComparison.Ordinal)
        .Replace("<", "&lt;", StringComparison.Ordinal)
        .Replace(">", "&gt;", StringComparison.Ordinal);

internal sealed record IconData(string ViewBox, string Content);

internal sealed record IconDocumentation(string? Description, IReadOnlyDictionary<string, string> Props);

internal sealed record Icon(string ComponentName, string EnumMember, string IconName, IconData Data, IconDocumentation Documentation);

internal sealed record GeneratorOptions(string Source, string Output)
{
    public static GeneratorOptions Parse(string[] args)
    {
        string? source = null;
        string? output = null;

        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            switch (argument)
            {
                case "--source":
                    source = ReadValue(args, ref index, argument);
                    break;
                case "--output":
                    output = ReadValue(args, ref index, argument);
                    break;
                default:
                    throw new InvalidOperationException("Unknown argument: " + argument);
            }
        }

        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(output))
        {
            throw new InvalidOperationException(
                "Usage: dotnet run --file scripts/csharp/generate-vu-icons.cs -- --source <unpacked-vu-icons> --output <ECMAScript.VuIcons-project>.");
        }

        return new GeneratorOptions(source, output);
    }

    private static string ReadValue(string[] args, ref int index, string argument)
    {
        if (++index >= args.Length)
            throw new InvalidOperationException("Missing value for " + argument);

        return args[index];
    }
}
