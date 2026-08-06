#:package TreeSitter.DotNet@1.3.0

using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;
using TreeSitter;

const string version = "1.20.5";
var check = args is ["--check"];
if (!check && args.Length != 0)
    throw new ArgumentException("Supported arguments: --check.");
var repoRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
var snapshotRoot = Path.Combine(repoRoot, "src", "ECMAScript.TDesign", "upstream", "tdesign-vue-next", version);
var inventoryPath = Path.Combine(snapshotRoot, "components.json");
var outputPath = Path.Combine(snapshotRoot, "bindings.json");
var contractsPath = Path.Combine(snapshotRoot, "contracts.json");

if (!File.Exists(inventoryPath))
    throw new InvalidOperationException($"Missing TDesign component inventory: {inventoryPath}");

using var typeScript = new Language("TypeScript");
var declarations = Directory.GetFiles(Path.Combine(snapshotRoot, "es"), "*.d.ts", SearchOption.AllDirectories)
    .SelectMany(path => TypeScriptDeclarations.Read(path, snapshotRoot, typeScript))
    .OrderBy(static declaration => declaration.Module, StringComparer.Ordinal)
    .ThenBy(static declaration => declaration.Name, StringComparer.Ordinal)
    .ToArray();

using var inventory = JsonDocument.Parse(File.ReadAllText(inventoryPath));
var components = inventory.RootElement.GetProperty("components").EnumerateArray()
    .Select(ComponentMetadata.Read)
    .Where(IsCurrentComponent)
    .Select(component => BindComponent(component, snapshotRoot, declarations))
    .OrderBy(static component => component.Tag, StringComparer.Ordinal)
    .ToArray();

var bindingJson = RenderBindings(components, declarations);
var contracts = ComponentContracts.Read(components, declarations, snapshotRoot, typeScript);
var contractsJson = RenderContracts(contracts);

var unresolved = components.Where(static component => component.Props.Length > 0 && component.PropsDeclaration is null).ToArray();
var missingProperties = contracts.Where(static contract => contract.MissingProperties.Length > 0).ToArray();
Console.WriteLine($"Indexed {declarations.Length} exported TypeScript declarations.");
Console.WriteLine($"Bound {components.Length - unresolved.Length}/{components.Length} documented components to props declarations.");
if (unresolved.Length > 0)
{
    foreach (var component in unresolved)
        Console.Error.WriteLine($"Unresolved component props: {component.Tag} ({component.RuntimeExport})");

    Environment.ExitCode = 1;
}
else if (missingProperties.Length > 0)
{
    foreach (var contract in missingProperties)
        Console.Error.WriteLine($"Undeclared component props: {contract.Tag} ({string.Join(", ", contract.MissingProperties)})");

    Environment.ExitCode = 1;
}
else if (check)
{
    if (!File.Exists(outputPath) || !string.Equals(File.ReadAllText(outputPath), bindingJson, StringComparison.Ordinal))
        throw new InvalidOperationException("TDesign component bindings are stale. Run scripts/csharp/generate-tdesign-bindings.cs.");
    if (!File.Exists(contractsPath) || !string.Equals(File.ReadAllText(contractsPath), contractsJson, StringComparison.Ordinal))
        throw new InvalidOperationException("TDesign component contracts are stale. Run scripts/csharp/generate-tdesign-bindings.cs.");

    Console.WriteLine("TDesign component bindings and contracts are current.");
}
else
{
    File.WriteAllText(outputPath, bindingJson);
    File.WriteAllText(contractsPath, contractsJson);
}

static ComponentBinding BindComponent(
    ComponentMetadata component,
    string snapshotRoot,
    IReadOnlyList<TsDeclaration> declarations)
{
    var runtimeExport = component.RuntimeExport;
    var module = ResolveComponentModule(component.Tag);
    var indexPath = Path.Combine(snapshotRoot, "es", module, "index.d.ts");
    var propsName = ResolvePropsName(component.Tag) ??
        ResolveExportedPropsName(indexPath, runtimeExport) ??
        "Td" + runtimeExport + "Props";
    var declaration = declarations.FirstOrDefault(candidate =>
        string.Equals(candidate.Module, module, StringComparison.Ordinal) &&
        string.Equals(candidate.Name, propsName, StringComparison.Ordinal));

    if (declaration is null && runtimeExport != component.SourceExport)
    {
        propsName = ResolveExportedPropsName(indexPath, component.SourceExport) ?? "Td" + component.SourceExport + "Props";
        declaration = declarations.FirstOrDefault(candidate =>
            string.Equals(candidate.Module, module, StringComparison.Ordinal) &&
            string.Equals(candidate.Name, propsName, StringComparison.Ordinal));
    }

    return new ComponentBinding(
        component.Tag,
        component.SourceExport,
        runtimeExport,
        "T" + runtimeExport,
        module,
        declaration is null ? null : declaration.Module + ":" + declaration.Name,
        declaration?.SourcePath,
        component.Props,
        component.PropTypes,
        component.Slots,
        component.Events);
}

static string ResolveComponentModule(string tag)
{
    var name = tag.StartsWith("t-", StringComparison.Ordinal) ? tag[2..] : tag;
    return name switch
    {
        "icon-font" => "icon",
        "anchor-item" or "anchor-target" => "anchor",
        "avatar-group" => "avatar",
        "breadcrumb-item" => "breadcrumb",
        "cascader-panel" => "cascader",
        "check-tag" or "check-tag-group" => "tag",
        "checkbox-group" => "checkbox",
        "collapse-panel" => "collapse",
        "color-picker-panel" => "color-picker",
        "date-picker-panel" or "date-range-picker" or "date-range-picker-panel" => "date-picker",
        "descriptions-item" => "descriptions",
        "dialog-card" => "dialog",
        "dropdown-item" => "dropdown",
        "form-item" => "form",
        "head-menu" or "menu-group" or "menu-item" or "submenu" => "menu",
        "aside" or "content" or "footer" or "header" => "layout",
        "list-item" or "list-item-meta" => "list",
        "option" or "option-group" => "select",
        "pagination-mini" => "pagination",
        "radio-button" or "radio-group" => "radio",
        "range-input-popup" => "range-input",
        "input-group" => "input",
        "base-table" or "enhanced-table" or "primary-table" => "table",
        "col" or "row" => "grid",
        "step-item" => "steps",
        "sticky-item" => "sticky-tool",
        "swiper-item" => "swiper",
        "tab-panel" => "tabs",
        "time-range-picker" => "time-picker",
        "timeline-item" => "timeline",
        "tree-select" => "tree-select",
        "typography" or "typography-paragraph" or "typography-text" or "typography-title" => "typography",
        _ => name
    };
}

static string? ResolvePropsName(string tag)
    => tag switch
    {
        "icon-font" => "TdIconfontProps",
        "t-base-table" => "BaseTableProps",
        "t-primary-table" => "PrimaryTableProps",
        "t-enhanced-table" => "EnhancedTableProps",
        "t-form-item" => "TdFormItemProps",
        "t-input-group" => "TdInputGroupProps",
        "t-col" => "TdColProps",
        "t-row" => "TdRowProps",
        _ => null
    };

static bool IsCurrentComponent(ComponentMetadata component)
    // web-types still publishes these retired tags, but 1.20.5 does not export a
    // corresponding browser component. Do not generate imports that cannot run.
    => component.Tag is not "t-search" and not "t-tooltip-lite";

static string? ResolveExportedPropsName(string indexPath, string exportName)
{
    if (!File.Exists(indexPath))
        return null;

    var content = File.ReadAllText(indexPath);
    var match = Regex.Match(
        content,
        $@"export\s+type\s+{Regex.Escape(exportName)}Props(?:\s*<[^;=]+>)?\s*=\s*(?<name>[A-Za-z_$][A-Za-z0-9_$]*)",
        RegexOptions.CultureInvariant);
    return match.Success ? match.Groups["name"].Value : null;
}

static string RenderBindings(
    IReadOnlyList<ComponentBinding> components,
    IReadOnlyList<TsDeclaration> declarations)
{
    using var stream = new MemoryStream();
    using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
    writer.WriteStartObject();
    writer.WriteString("package", "tdesign-vue-next");
    writer.WriteString("version", version);
    writer.WriteNumber("componentCount", components.Count);
    writer.WriteNumber("declarationCount", declarations.Count);
    writer.WriteStartArray("components");
    foreach (var component in components)
    {
        writer.WriteStartObject();
        writer.WriteString("tag", component.Tag);
        writer.WriteString("sourceExport", component.SourceExport);
        writer.WriteString("runtimeExport", component.RuntimeExport);
        writer.WriteString("authoringType", component.AuthoringType);
        writer.WriteString("module", component.Module);
        writer.WriteString("propsDeclaration", component.PropsDeclaration);
        writer.WriteString("propsSource", component.PropsSource);
        WriteStrings(writer, "props", component.Props);
        WriteStrings(writer, "slots", component.Slots);
        WriteStrings(writer, "events", component.Events);
        writer.WriteEndObject();
    }

    writer.WriteEndArray();
    writer.WriteEndObject();
    writer.Flush();
    return Encoding.UTF8.GetString(stream.ToArray()) + Environment.NewLine;
}

static string RenderContracts(IReadOnlyList<ComponentContract> contracts)
{
    using var stream = new MemoryStream();
    using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
    writer.WriteStartObject();
    writer.WriteString("package", "tdesign-vue-next");
    writer.WriteString("version", version);
    writer.WriteNumber("componentCount", contracts.Count);
    writer.WriteStartArray("components");
    foreach (var contract in contracts)
    {
        writer.WriteStartObject();
        writer.WriteString("tag", contract.Tag);
        writer.WriteString("authoringType", contract.AuthoringType);
        writer.WriteString("propsDeclaration", contract.PropsDeclaration);
        writer.WriteStartArray("props");
        foreach (var property in contract.Properties)
        {
            writer.WriteStartObject();
            writer.WriteString("name", property.Name);
            writer.WriteString("type", property.Type);
            writer.WriteString("declaredBy", property.DeclaredBy);
            writer.WriteString("source", property.SourcePath);
            writer.WriteBoolean("optional", property.Optional);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteStartArray("events");
        foreach (var @event in contract.Events)
        {
            writer.WriteStartObject();
            writer.WriteString("name", @event.Name);
            writer.WriteString("prop", @event.Property);
            writer.WriteString("type", @event.Type);
            writer.WriteString("declaredBy", @event.DeclaredBy);
            writer.WriteString("source", @event.SourcePath);
            writer.WriteBoolean("optional", @event.Optional);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteStartArray("slots");
        foreach (var slot in contract.Slots)
        {
            writer.WriteStartObject();
            writer.WriteString("name", slot.Name);
            writer.WriteString("prop", slot.Property);
            writer.WriteString("type", slot.Type);
            writer.WriteString("declaredBy", slot.DeclaredBy);
            writer.WriteString("source", slot.SourcePath);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        WriteStrings(writer, "missingProperties", contract.MissingProperties);
        WriteStrings(writer, "missingSlots", contract.MissingSlots);
        writer.WriteEndObject();
    }

    writer.WriteEndArray();
    writer.WriteEndObject();
    writer.Flush();
    return Encoding.UTF8.GetString(stream.ToArray()) + Environment.NewLine;
}

static void WriteStrings(Utf8JsonWriter writer, string name, IReadOnlyList<string> values)
{
    writer.WriteStartArray(name);
    foreach (var value in values)
        writer.WriteStringValue(value);
    writer.WriteEndArray();
}

static string FindRepositoryRoot(string startDirectory)
{
    for (var directory = new DirectoryInfo(Path.GetFullPath(startDirectory)); directory is not null; directory = directory.Parent)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;
    }

    throw new InvalidOperationException("Unable to locate Jazor.slnx.");
}

file sealed record ComponentMetadata(
    string Tag,
    string SourceExport,
    string RuntimeExport,
    string[] Props,
    IReadOnlyDictionary<string, string> PropTypes,
    string[] Slots,
    string[] Events)
{
    public static ComponentMetadata Read(JsonElement component)
        => new(
            component.GetProperty("tag").GetString()!,
            component.TryGetProperty("sourceExport", out var sourceExport)
                ? sourceExport.GetString()!
                : component.GetProperty("export").GetString()!,
            component.GetProperty("export").GetString()!,
            component.GetProperty("props").EnumerateArray().Select(static item => item.GetProperty("name").GetString()!).ToArray(),
            component.GetProperty("props").EnumerateArray()
                .GroupBy(static item => item.GetProperty("name").GetString()!, StringComparer.Ordinal)
                .ToDictionary(
                    static group => group.Key,
                    static group => group.Select(static item => item.TryGetProperty("type", out var type) ? type.GetString() : null)
                        .FirstOrDefault(static type => !string.IsNullOrWhiteSpace(type)) ?? "unknown",
                    StringComparer.Ordinal),
            component.GetProperty("slots").EnumerateArray().Select(static item => item.GetProperty("name").GetString()!).ToArray(),
            component.GetProperty("events").EnumerateArray().Select(static item => item.GetProperty("name").GetString()!).ToArray());
}

file sealed record ComponentBinding(
    string Tag,
    string SourceExport,
    string RuntimeExport,
    string AuthoringType,
    string Module,
    string? PropsDeclaration,
    string? PropsSource,
    string[] Props,
    IReadOnlyDictionary<string, string> PropTypes,
    string[] Slots,
    string[] Events);

file sealed record TsDeclaration(
    string Module,
    string Name,
    string Kind,
    string SourcePath,
    TsTypeParameter[] TypeParameters);
file sealed record TsTypeParameter(string Name, string? DefaultType);

file sealed record ComponentContract(
    string Tag,
    string AuthoringType,
    string? PropsDeclaration,
    TsProperty[] Properties,
    TsEvent[] Events,
    TsSlot[] Slots,
    string[] MissingProperties,
    string[] MissingSlots);

file sealed record TsProperty(string Name, string Type, string DeclaredBy, string SourcePath, bool Optional);
file sealed record TsEvent(string Name, string Property, string Type, string DeclaredBy, string SourcePath, bool Optional);
file sealed record TsSlot(string Name, string? Property, string Type, string DeclaredBy, string SourcePath);

file static class ComponentContracts
{
    public static ComponentContract[] Read(
        IReadOnlyList<ComponentBinding> components,
        IReadOnlyList<TsDeclaration> declarations,
        string snapshotRoot,
        Language typeScript)
    {
        var reader = new DeclarationReader(declarations, snapshotRoot, typeScript);
        return components.Select(component =>
        {
            if (component.PropsSource is null || component.PropsDeclaration is null)
                return new ComponentContract(
                    component.Tag,
                    component.AuthoringType,
                    null,
                    [],
                    [],
                    [],
                    component.Props,
                    component.Slots);

            var declarationName = component.PropsDeclaration[(component.PropsDeclaration.IndexOf(':') + 1)..];
            // A component Props declaration is its public generic surface. Keep its
            // parameters open here; inherited declarations then receive either that
            // parameter or their own upstream default instead of an unbound `T`.
            var properties = reader.Read(component.PropsSource, declarationName, preserveParameters: true);
            var byName = properties.ToDictionary(static property => property.Name, StringComparer.Ordinal);
            var selected = new List<TsProperty>();
            var missing = new List<string>();
            foreach (var propertyName in component.Props)
            {
                var sourceName = ToCamelCase(propertyName);
                if (byName.TryGetValue(sourceName, out var property))
                    selected.Add(property);
                else if (component.PropTypes.TryGetValue(propertyName, out var type))
                    selected.Add(new TsProperty(sourceName, type, "web-types", "helper/web-types.json", Optional: true));
                else
                    missing.Add(propertyName);
            }

            // The actual event surface is the exported Props declaration. web-types
            // can retain stale documentation entries that are not runtime listeners.
            var events = properties
                .Where(static property => property.Name.StartsWith("on", StringComparison.Ordinal) &&
                                          property.Name.Length > 2 &&
                                          property.Type.Contains("=>", StringComparison.Ordinal))
                .Select(property => new TsEvent(
                    ToKebabCase(property.Name[2..]),
                    property.Name,
                    property.Type,
                    property.DeclaredBy,
                    property.SourcePath,
                    property.Optional))
                .ToArray();

            var slots = new List<TsSlot>();
            var missingSlots = new List<string>();
            foreach (var slotName in component.Slots.DistinctBy(NormalizeName, StringComparer.Ordinal))
            {
                if (string.Equals(slotName, "default", StringComparison.Ordinal))
                {
                    slots.Add(new TsSlot(slotName, null, "TNode", "slot", component.PropsSource));
                    continue;
                }

                var source = selected.FirstOrDefault(property =>
                    string.Equals(NormalizeName(property.Name), NormalizeName(slotName), StringComparison.Ordinal));
                if (source is not null)
                    slots.Add(new TsSlot(slotName, source.Name, source.Type, source.DeclaredBy, source.SourcePath));
                else
                    missingSlots.Add(slotName);
            }

            return new ComponentContract(
                component.Tag,
                component.AuthoringType,
                component.PropsDeclaration,
                selected.ToArray(),
                events.ToArray(),
                slots.ToArray(),
                missing.ToArray(),
                missingSlots.ToArray());
        }).ToArray();
    }

    private static string ToCamelCase(string name)
    {
        var builder = new StringBuilder(name.Length);
        var upper = false;
        foreach (var character in name)
        {
            if (character == '-')
            {
                upper = true;
                continue;
            }

            builder.Append(upper ? char.ToUpperInvariant(character) : character);
            upper = false;
        }

        return builder.ToString();
    }

    private static string ToKebabCase(string name)
    {
        var builder = new StringBuilder(name.Length + 4);
        foreach (var character in name)
        {
            if (char.IsUpper(character))
                builder.Append('-');
            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString().TrimStart('-');
    }

    private static string NormalizeName(string name)
        => string.Concat(name.Where(char.IsLetterOrDigit)).ToUpperInvariant();

    private sealed class DeclarationReader(
        IReadOnlyList<TsDeclaration> declarations,
        string snapshotRoot,
        Language typeScript)
    {
        private readonly Dictionary<(string Path, string Name, string Arguments), TsProperty[]> _cache = [];

        public TsProperty[] Read(
            string sourcePath,
            string name,
            IReadOnlyList<string>? typeArguments = null,
            bool preserveParameters = false)
        {
            var declarationInfo = ResolveDeclaration(sourcePath, name)
                ?? throw new InvalidOperationException($"Unable to resolve TypeScript declaration {name} in {sourcePath}.");
            var bindings = BindTypeParameters(declarationInfo, typeArguments, preserveParameters);
            var key = (
                declarationInfo.SourcePath,
                declarationInfo.Name,
                string.Join("\0", declarationInfo.TypeParameters.Select(parameter => bindings[parameter.Name])));
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            // Break recursive declarations before following inheritance aliases.
            _cache[key] = [];
            var path = Path.Combine(snapshotRoot, declarationInfo.SourcePath.Replace('/', Path.DirectorySeparatorChar));
            using var parser = new Parser(typeScript);
            using var tree = parser.Parse(File.ReadAllText(path))
                ?? throw new InvalidOperationException($"Unable to parse TypeScript contract: {path}");
            var declaration = tree.RootNode.NamedChildren
                .Where(static node => node.Type == "export_statement")
                .Select(static node => node.GetChildForField("declaration"))
                .FirstOrDefault(node => node?.GetChildForField("name")?.Text == declarationInfo.Name)
                ?? throw new InvalidOperationException($"Missing TypeScript declaration {declarationInfo.Name} in {declarationInfo.SourcePath}.");

            var properties = new Dictionary<string, TsProperty>(StringComparer.Ordinal);
            foreach (var parent in ReadParents(declaration, bindings))
            {
                var parentDeclaration = ResolveDeclaration(declarationInfo.SourcePath, parent.Name);
                if (parentDeclaration is null)
                    throw new InvalidOperationException($"Unable to resolve base type {parent.Name} for {declarationInfo.Name} in {declarationInfo.SourcePath}.");

                var inherited = Read(parentDeclaration.SourcePath, parentDeclaration.Name, parent.TypeArguments);
                foreach (var property in inherited)
                {
                    if (parent.Pick.Count > 0 && !parent.Pick.Contains(property.Name))
                        continue;
                    if (parent.Omit.Contains(property.Name))
                        continue;
                    properties[property.Name] = property;
                }
            }

            var body = declaration.GetChildForField("body");
            if (body is not null)
            {
                foreach (var property in body.NamedChildren.Where(static node => node.Type == "property_signature"))
                {
                    var propertyName = property.GetChildForField("name")?.Text.Trim('"', '\'');
                    var annotation = property.GetChildForField("type")?.Text;
                    if (string.IsNullOrWhiteSpace(propertyName) || string.IsNullOrWhiteSpace(annotation))
                        continue;

                    properties[propertyName] = new TsProperty(
                        propertyName,
                        SubstituteTypeParameters(annotation.TrimStart().TrimStart(':').TrimStart(), bindings),
                        declarationInfo.Name,
                        declarationInfo.SourcePath,
                        property.Children.Any(static child => child.Type == "?"));
                }
            }

            var result = properties.Values.ToArray();
            _cache[key] = result;
            return result;
        }

        private static IReadOnlyDictionary<string, string> BindTypeParameters(
            TsDeclaration declaration,
            IReadOnlyList<string>? typeArguments,
            bool preserveParameters)
        {
            typeArguments ??= [];
            if (typeArguments.Count > declaration.TypeParameters.Length)
            {
                throw new InvalidOperationException(
                    $"TypeScript declaration {declaration.Name} expects {declaration.TypeParameters.Length} type arguments, received {typeArguments.Count}.");
            }

            var bindings = new Dictionary<string, string>(StringComparer.Ordinal);
            for (var index = 0; index < declaration.TypeParameters.Length; index++)
            {
                var parameter = declaration.TypeParameters[index];
                if (index < typeArguments.Count)
                {
                    bindings.Add(parameter.Name, typeArguments[index]);
                    continue;
                }

                if (preserveParameters)
                {
                    bindings.Add(parameter.Name, parameter.Name);
                    continue;
                }

                if (parameter.DefaultType is null)
                {
                    throw new InvalidOperationException(
                        $"TypeScript declaration {declaration.Name} requires an explicit type argument for {parameter.Name}.");
                }

                bindings.Add(parameter.Name, SubstituteTypeParameters(parameter.DefaultType, bindings));
            }

            return bindings;
        }

        private TsDeclaration? ResolveDeclaration(string sourcePath, string name)
        {
            var exact = declarations.FirstOrDefault(candidate =>
                candidate.SourcePath == sourcePath && candidate.Name == name);
            if (exact is not null)
                return exact;

            var module = sourcePath.StartsWith("es/", StringComparison.Ordinal)
                ? sourcePath[3..].Split('/')[0]
                : "common";
            var local = declarations.Where(candidate => candidate.Module == module && candidate.Name == name).ToArray();
            if (local.Length == 1)
                return local[0];

            var global = declarations.Where(candidate => candidate.Name == name).ToArray();
            return global.Length == 1 ? global[0] : null;
        }

        private static IEnumerable<ParentType> ReadParents(
            Node declaration,
            IReadOnlyDictionary<string, string> bindings)
        {
            if (declaration.Type == "type_alias_declaration")
            {
                // A generic default such as <T = Row> is not the alias assignment.
                // Only the top-level '=' starts the alias definition.
                var equals = IndexOfTopLevel(declaration.Text, '=');
                if (equals >= 0)
                {
                    var definition = declaration.Text[(equals + 1)..].Trim().TrimEnd(';');
                    foreach (var parent in SplitTopLevel(definition, '&')
                                 .Select(part => ParentType.Parse(SubstituteTypeParameters(part, bindings))))
                        yield return parent;
                }
                yield break;
            }

            foreach (var clause in declaration.NamedChildren.Where(static node => node.Type == "extends_type_clause"))
            {
                var text = clause.Text.StartsWith("extends", StringComparison.Ordinal)
                    ? clause.Text["extends".Length..].Trim()
                    : clause.Text;
                foreach (var parent in SplitTopLevel(text, ','))
                    yield return ParentType.Parse(SubstituteTypeParameters(parent, bindings));
            }
        }

        // Replace only TypeScript identifier tokens. A regular expression would also
        // rewrite quoted string-literal domains such as 'T', changing the contract.
        private static string SubstituteTypeParameters(string source, IReadOnlyDictionary<string, string> bindings)
        {
            if (bindings.Count == 0)
                return source;

            var builder = new StringBuilder(source.Length);
            var quote = '\0';
            for (var index = 0; index < source.Length;)
            {
                var current = source[index];
                if (quote != '\0')
                {
                    builder.Append(current);
                    if (current == '\\' && index + 1 < source.Length)
                    {
                        builder.Append(source[index + 1]);
                        index += 2;
                        continue;
                    }

                    if (current == quote)
                        quote = '\0';
                    index++;
                    continue;
                }

                if (current is '\'' or '"' or '`')
                {
                    quote = current;
                    builder.Append(current);
                    index++;
                    continue;
                }

                if (char.IsLetter(current) || current is '_' or '$')
                {
                    var start = index++;
                    while (index < source.Length && (char.IsLetterOrDigit(source[index]) || source[index] is '_' or '$'))
                        index++;
                    var identifier = source[start..index];
                    builder.Append(bindings.TryGetValue(identifier, out var replacement) ? replacement : identifier);
                    continue;
                }

                builder.Append(current);
                index++;
            }

            return builder.ToString();
        }

        private static IEnumerable<string> SplitTopLevel(string text, char delimiter)
        {
            var start = 0;
            var depth = 0;
            for (var index = 0; index < text.Length; index++)
            {
                depth += text[index] switch { '<' or '(' or '[' or '{' => 1, '>' or ')' or ']' or '}' => -1, _ => 0 };
                if (text[index] != delimiter || depth != 0)
                    continue;
                yield return text[start..index].Trim();
                start = index + 1;
            }
            yield return text[start..].Trim();
        }

        private static int IndexOfTopLevel(string text, char value)
        {
            var depth = 0;
            for (var index = 0; index < text.Length; index++)
            {
                switch (text[index])
                {
                    case '<':
                    case '(':
                    case '[':
                    case '{':
                        depth++;
                        break;
                    case '>':
                    case ')':
                    case ']':
                    case '}':
                        depth--;
                        break;
                    default:
                        if (text[index] == value && depth == 0)
                            return index;
                        break;
                }
            }

            return -1;
        }

        private sealed record ParentType(
            string Name,
            string[] TypeArguments,
            HashSet<string> Pick,
            HashSet<string> Omit)
        {
            public static ParentType Parse(string text)
            {
                var generic = ReadGeneric(text);
                if (generic is { Name: "Pick" or "Omit", TypeArguments.Length: > 0 })
                {
                    var target = Parse(generic.TypeArguments[0]);
                    var names = generic.TypeArguments.Skip(1)
                        .SelectMany(argument => Regex.Matches(argument, "['\\\"](?<name>[A-Za-z_$][A-Za-z0-9_$]*)['\\\"]")
                            .Select(static match => match.Groups["name"].Value))
                        .ToHashSet(StringComparer.Ordinal);
                    return generic.Name == "Pick"
                        ? target with { Pick = names }
                        : target with { Omit = names };
                }

                if (generic is not null)
                    return new ParentType(generic.Name, generic.TypeArguments, [], []);

                var name = Regex.Match(text, @"^[A-Za-z_$][A-Za-z0-9_$]*$").Value;
                if (name.Length == 0)
                    throw new InvalidOperationException($"Unable to read TypeScript base type: {text}");
                return new ParentType(name, [], [], []);
            }

            private static ParentType? ReadGeneric(string text)
            {
                var name = Regex.Match(text.Trim(), @"^(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\s*<");
                if (!name.Success)
                    return null;

                var open = text.IndexOf('<', name.Index + name.Length - 1);
                if (open < 0 || !text.TrimEnd().EndsWith('>'))
                    return null;

                var inner = text[(open + 1)..text.LastIndexOf('>')];
                return new ParentType(
                    name.Groups["name"].Value,
                    SplitTopLevel(inner, ',').ToArray(),
                    [],
                    []);
            }
        }
    }
}

file static class TypeScriptDeclarations
{
    public static IEnumerable<TsDeclaration> Read(string path, string snapshotRoot, Language typeScript)
    {
        var relativePath = Path.GetRelativePath(Path.Combine(snapshotRoot, "es"), path).Replace('\\', '/');
        var slash = relativePath.IndexOf('/');
        var module = slash < 0 ? "common" : relativePath[..slash];
        using var parser = new Parser(typeScript);
        using var tree = parser.Parse(File.ReadAllText(path))
            ?? throw new InvalidOperationException($"Unable to parse TypeScript declaration: {path}");

        foreach (var statement in tree.RootNode.NamedChildren)
        {
            if (statement.Type != "export_statement")
                continue;

            var declaration = statement.GetChildForField("declaration");
            if (declaration is null || declaration.Type is not ("interface_declaration" or "type_alias_declaration"))
                continue;

            var name = declaration.GetChildForField("name")?.Text;
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException($"TypeScript export is missing a name: {path}");

            yield return new TsDeclaration(
                module,
                name,
                declaration.Type == "interface_declaration" ? "interface" : "type",
                Path.GetRelativePath(snapshotRoot, path).Replace('\\', '/'),
                ReadTypeParameters(declaration));
        }
    }

    private static TsTypeParameter[] ReadTypeParameters(Node declaration)
    {
        var parameters = declaration.NamedChildren.FirstOrDefault(static node => node.Type == "type_parameters");
        if (parameters is null)
            return [];

        return parameters.NamedChildren
            .Where(static node => node.Type == "type_parameter")
            .Select(parameter =>
            {
                var name = parameter.NamedChildren.FirstOrDefault(static node => node.Type == "type_identifier")?.Text;
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidOperationException($"TypeScript type parameter has no name: {parameter.Text}");

                var equals = IndexOfTopLevel(parameter.Text, '=');
                return new TsTypeParameter(name, equals < 0 ? null : parameter.Text[(equals + 1)..].Trim());
            })
            .ToArray();
    }

    private static int IndexOfTopLevel(string text, char value)
    {
        var depth = 0;
        for (var index = 0; index < text.Length; index++)
        {
            depth += text[index] switch
            {
                '<' or '(' or '[' or '{' => 1,
                '>' or ')' or ']' or '}' => -1,
                _ => 0
            };
            if (text[index] == value && depth == 0)
                return index;
        }

        return -1;
    }
}
