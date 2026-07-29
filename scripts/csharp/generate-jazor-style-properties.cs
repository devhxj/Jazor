#!/usr/bin/env dotnet run

using System.Text;
using System.Text.Json;

var repoRoot = FindRepositoryRoot();
var inventoryPath = Path.Combine(repoRoot, "src", "ECMAScript", "webidl", "webidl.inventory.json");
var outputPath = Path.Combine(repoRoot, "src", "Jazor.Style", "CssDeclarations.Properties.g.cs");
var checkOnly = args.Any(static argument => string.Equals(argument, "--check", StringComparison.Ordinal));

using var document = JsonDocument.Parse(File.ReadAllText(inventoryPath));
var root = document.RootElement;
var schemaVersion = root.GetProperty("schemaVersion").GetInt32();
var webrefCssVersion = root.GetProperty("source").GetProperty("webrefCss").GetString()
    ?? throw new InvalidDataException("The inventory source.webrefCss value is missing.");

var propertiesByCssName = new Dictionary<string, CssProperty>(StringComparer.Ordinal);
foreach (var file in root.GetProperty("files").EnumerateArray())
{
    foreach (var declaration in file.GetProperty("declarations").EnumerateArray())
    {
        if (!HasStringValue(declaration, "kind", "interface") ||
            !HasStringValue(declaration, "name", "CSSStyleDeclaration"))
        {
            continue;
        }

        var payload = declaration.GetProperty("payload");
        foreach (var member in payload.GetProperty("members").EnumerateArray())
        {
            if (!HasStringValue(member, "type", "attribute") ||
                member.GetProperty("readonly").GetBoolean())
            {
                continue;
            }

            var idlType = member.GetProperty("idlType").GetProperty("idlType");
            if (idlType.ValueKind != JsonValueKind.String ||
                !string.Equals(idlType.GetString(), "CSSOMString", StringComparison.Ordinal))
            {
                continue;
            }

            var idlName = member.GetProperty("name").GetString()
                ?? throw new InvalidDataException("A CSSStyleDeclaration attribute has no name.");
            if (string.Equals(idlName, "cssText", StringComparison.Ordinal))
                continue;

            var property = CreateProperty(idlName);
            if (propertiesByCssName.TryGetValue(property.CssName, out var existing))
            {
                if (!string.Equals(existing.MemberName, property.MemberName, StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        $"CSS property '{property.CssName}' maps to both '{existing.MemberName}' and '{property.MemberName}'.");
                }

                continue;
            }

            propertiesByCssName.Add(property.CssName, property);
        }
    }
}

var reservedMemberNames = new HashSet<string>(StringComparer.Ordinal)
{
    "Additional",
    "Children"
};
var duplicateMember = propertiesByCssName.Values
    .GroupBy(static property => property.MemberName, StringComparer.Ordinal)
    .FirstOrDefault(static group => group.Count() > 1);
if (duplicateMember is not null)
{
    throw new InvalidDataException(
        $"Generated member '{duplicateMember.Key}' maps to multiple CSS properties: " +
        string.Join(", ", duplicateMember.Select(static property => property.CssName)));
}

var reservedConflict = propertiesByCssName.Values.FirstOrDefault(property => reservedMemberNames.Contains(property.MemberName));
if (reservedConflict is not null)
{
    throw new InvalidDataException(
        $"Generated member '{reservedConflict.MemberName}' conflicts with a hand-authored CssDeclarations member.");
}

var output = BuildOutput(
    schemaVersion,
    webrefCssVersion,
    propertiesByCssName.Values.OrderBy(static property => property.CssName, StringComparer.Ordinal));

if (checkOnly)
{
    if (!File.Exists(outputPath) || !string.Equals(File.ReadAllText(outputPath), output, StringComparison.Ordinal))
    {
        Console.Error.WriteLine("Jazor.Style generated properties are out of date. Run:");
        Console.Error.WriteLine("  dotnet run --file scripts/csharp/generate-jazor-style-properties.cs");
        Environment.ExitCode = 1;
        return;
    }

    Console.WriteLine($"Jazor.Style properties are current: {propertiesByCssName.Count} properties ({webrefCssVersion}).");
    return;
}

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, output, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
Console.WriteLine($"Generated {propertiesByCssName.Count} Jazor.Style properties from {webrefCssVersion}.");

static bool HasStringValue(JsonElement element, string propertyName, string expected)
    => element.TryGetProperty(propertyName, out var property) &&
       property.ValueKind == JsonValueKind.String &&
       string.Equals(property.GetString(), expected, StringComparison.Ordinal);

static CssProperty CreateProperty(string idlName)
{
    var normalizedName = idlName switch
    {
        "cssFloat" or "_float" => "float",
        _ => idlName
    };
    var memberName = char.ToUpperInvariant(normalizedName[0]) + normalizedName[1..];
    var cssName = ToKebabCase(normalizedName);

    if (HasVendorPrefix(normalizedName, "webkit") ||
        HasVendorPrefix(normalizedName, "moz") ||
        HasVendorPrefix(normalizedName, "ms") ||
        HasVendorPrefix(normalizedName, "o"))
    {
        cssName = "-" + cssName;
    }

    return new CssProperty(cssName, memberName);
}

static bool HasVendorPrefix(string value, string prefix)
    => value.Length > prefix.Length &&
       value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
       char.IsUpper(value[prefix.Length]);

static string ToKebabCase(string value)
{
    var builder = new StringBuilder(value.Length + 8);
    for (var index = 0; index < value.Length; index++)
    {
        var character = value[index];
        if (char.IsUpper(character))
        {
            if (index > 0)
                builder.Append('-');

            builder.Append(char.ToLowerInvariant(character));
        }
        else
        {
            builder.Append(character);
        }
    }

    return builder.ToString();
}

static string BuildOutput(int schemaVersion, string webrefCssVersion, IEnumerable<CssProperty> properties)
{
    var builder = new StringBuilder();
    builder.AppendLine("// <auto-generated/>");
    builder.Append("// Source: webidl.inventory.json schema ").Append(schemaVersion)
        .Append(", ").Append(webrefCssVersion).AppendLine();
    builder.AppendLine("#nullable enable");
    builder.AppendLine();
    builder.AppendLine("namespace Jazor.Style;");
    builder.AppendLine();
    builder.AppendLine("public partial record CssDeclarations");
    builder.AppendLine("{");
    foreach (var property in properties)
    {
        builder.Append("    [global::System.ComponentModel.Description(\"@#");
        builder.Append(property.CssName.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal));
        builder.AppendLine("\")]");
        builder.Append("    public string? ").Append(property.MemberName).AppendLine(" { get; init; }");
        builder.AppendLine();
    }

    builder.AppendLine("}");
    return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
}

static string FindRepositoryRoot()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;

        directory = directory.Parent;
    }

    throw new DirectoryNotFoundException("Repository root containing Jazor.slnx was not found.");
}

internal sealed record CssProperty(string CssName, string MemberName);
