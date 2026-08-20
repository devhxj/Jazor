using System.Text;
using System.Text.Json;

namespace ECMAScript.WebIDL.Generator;

internal sealed class PreviewBindingEmitter
{
    private static readonly string[] CommonUsings =
    [
        "global using System;",
        "global using System.ComponentModel;",
        "global using System.Collections;",
        "global using System.Collections.Generic;",
        "global using System.Collections.ObjectModel;",
        "global using System.Numerics;",
        "global using System.Collections.Frozen;",
        "global using System.Threading.Tasks;",
        "global using System.Diagnostics.CodeAnalysis;",
		"global using ECMAScript.Contract;",
        "global using ECMAScript;",
        "global using ECMAScript.CSS;",
        "global using ECMAScript.GPUBufferUsage;",
        "global using ECMAScript.WebAssembly;",
        // SVG 2 retains these legacy names in several signatures while its
        // extracted IDL no longer declares the equivalent interfaces.
        "global using SVGPoint = ECMAScript.DOMPoint;",
        "global using SVGRect = ECMAScript.DOMRect;",
        "global using SVGMatrix = ECMAScript.DOMMatrix;"
    ];

    private static readonly HashSet<string> ExcludedDeclarationNames = new(StringComparer.Ordinal)
    {
        "Console",
    };

    private readonly GeneratorOptions _options;
    private readonly WebIdlTypeMapper _typeMapper = new();
    private readonly Dictionary<string, IReadOnlyList<JsonElement>> _mixinMembersByKey = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IReadOnlyDictionary<string, WebIdlMemberDocumentation>> _mixinMemberDocumentationByKey = new(StringComparer.Ordinal);
    private readonly Dictionary<string, InterfaceCache> _interfaceCachesByKey = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string[]> _interfaceKeysByName = new(StringComparer.Ordinal);
    private readonly Dictionary<string, GeneratedUnionDefinition> _generatedUnionDefinitionsByQualifiedName = new(StringComparer.Ordinal);
    private readonly Dictionary<string, HashSet<string>> _occupiedTypeNamesByNamespace = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _resolvedUnionTypeNameByIdentity = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<string>> _resolvedUnionTypeNamesByShape = new(StringComparer.Ordinal);

    public PreviewBindingEmitter(GeneratorOptions options)
    {
        _options = options;
        _typeMapper.NamedUnionResolver = ResolveNamedUnionType;
        _typeMapper.NamedUnionTypeFormatter = (idlType, namespaceName, baseName) =>
            ResolveInlineType(idlType, namespaceName, baseName);
    }

    public async Task EmitAsync(WebIdlInventory inventory, CancellationToken cancellationToken)
    {
        var previewRoot = Path.Combine(_options.OutputDirectory, "generate");
        if (Directory.Exists(previewRoot))
        {
            Directory.Delete(previewRoot, recursive: true);
        }

        Directory.CreateDirectory(previewRoot);

        var globalUsings = new List<string>(CommonUsings);
        var enumsByNamespace = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);
        var callbacksByNamespace = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);
        var dictionariesByNamespace = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);
        var interfacesByNamespace = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);
        var namespacesByNamespace = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);
        var unionsByNamespace = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);
        _generatedUnionDefinitionsByQualifiedName.Clear();
        _occupiedTypeNamesByNamespace.Clear();
        _resolvedUnionTypeNameByIdentity.Clear();
        _resolvedUnionTypeNamesByShape.Clear();

        foreach (var item in inventory.Files.SelectMany(file => file.Declarations.Select(declaration => new
                 {
                     file.Namespace,
                     Declaration = declaration,
                 })))
        {
            var declaration = item.Declaration;
            RegisterOccupiedDeclarationName(item.Namespace, declaration);

            if (declaration.Kind == "enum" && declaration.Name is not null)
            {
                _typeMapper.RegisterEnum(
                    WebIdlNaming.ToTypeName(declaration.Name),
                    declaration.Payload
                        .GetArray("values")
                        .Select(static value => value.GetStringOrNull("value") ?? string.Empty));
            }
            else if (declaration.Kind == "dictionary" && declaration.Name is not null)
            {
                _typeMapper.RegisterDictionary(WebIdlNaming.ToTypeName(declaration.Name));
            }
        }

        foreach (var file in inventory.Files)
        {
            foreach (var declaration in file.Declarations)
            {
                if (IsExcludedDeclarationName(declaration.Name))
                {
                    continue;
                }

                switch (declaration.Kind)
                {
                    case "typedef":
                    {
                        var typedefCode = EmitTypedef(declaration, file.Namespace);
                        if (!string.IsNullOrWhiteSpace(typedefCode))
                        {
                            globalUsings.Add(typedefCode);
                        }

                        break;
                    }
                    case "enum":
                        AddByNamespace(enumsByNamespace, file.Namespace, EmitEnum(declaration));
                        break;
                    case "callback":
                        AddByNamespace(callbacksByNamespace, file.Namespace, EmitCallback(declaration, file.Namespace));
                        break;
                    case "callback interface":
                        foreach (var code in EmitCallbackInterface(declaration, file.Namespace, globalUsings))
                        {
                            AddByNamespace(callbacksByNamespace, file.Namespace, code);
                        }

                        break;
                }
            }
        }

        foreach (var dictionaryGroup in inventory.Files
                     .SelectMany(file => file.Declarations
                         .Where(static declaration => declaration.Kind == "dictionary" && !IsExcludedDeclarationName(declaration.Name))
                         .Select(declaration => new { file.Namespace, Declaration = declaration }))
                     .GroupBy(item => new
                     {
                         Namespace = item.Namespace ?? string.Empty,
                         Name = item.Declaration.Name ?? string.Empty,
                     }))
        {
            var namespaceName = string.IsNullOrWhiteSpace(dictionaryGroup.Key.Namespace) ? null : dictionaryGroup.Key.Namespace;
            AddByNamespace(dictionariesByNamespace, namespaceName, EmitDictionary(dictionaryGroup.Select(item => item.Declaration).ToArray(), namespaceName));
        }

        _mixinMembersByKey.Clear();
        _mixinMemberDocumentationByKey.Clear();
        foreach (var mixinGroup in inventory.Files
                     .SelectMany(file => file.Declarations
                         .Where(static declaration => declaration.Kind == "interface mixin")
                         .Select(declaration => new { file.Namespace, Declaration = declaration }))
                     .GroupBy(item => new
                     {
                         Namespace = item.Namespace ?? string.Empty,
                         Name = item.Declaration.Name ?? string.Empty,
                     }))
        {
            var namespaceName = string.IsNullOrWhiteSpace(mixinGroup.Key.Namespace) ? null : mixinGroup.Key.Namespace;
            var members = mixinGroup
                .SelectMany(item => item.Declaration.Payload.GetArray("members"))
                .ToArray();
            var mixinKey = $"{mixinGroup.Key.Namespace}|{mixinGroup.Key.Name}";
            _mixinMembersByKey[mixinKey] = DistinctMembers(members, namespaceName);
            _mixinMemberDocumentationByKey[mixinKey] = BuildMemberDocumentationByKey(
                mixinGroup.Select(item => item.Declaration),
                namespaceName);
        }

        var includesByTarget = inventory.Files
            .SelectMany(file => file.Declarations
                .Where(static declaration => declaration.Kind == "includes")
                .Select(declaration => new
                {
                    Namespace = file.Namespace ?? string.Empty,
                    Target = declaration.Target ?? string.Empty,
                    Include = declaration.Includes ?? string.Empty,
                }))
            .GroupBy(item => $"{item.Namespace}|{item.Target}", StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item => item.Include).Distinct(StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);

        BuildInterfaceCaches(inventory, includesByTarget);

        var inheritedTypeKeys = inventory.Files
            .SelectMany(file => file.Declarations
                .Where(static declaration => declaration.Kind == "interface" && !string.IsNullOrWhiteSpace(declaration.Inheritance))
                .Select(declaration => $"{file.Namespace ?? string.Empty}|{declaration.Inheritance}"))
            .ToHashSet(StringComparer.Ordinal);

        foreach (var interfaceGroup in inventory.Files
                     .SelectMany(file => file.Declarations
                         .Where(static declaration => declaration.Kind == "interface" && !IsExcludedDeclarationName(declaration.Name))
                         .Select(declaration => new { file.Namespace, Declaration = declaration }))
                     .GroupBy(item => new
                     {
                         Namespace = item.Namespace ?? string.Empty,
                         Name = item.Declaration.Name ?? string.Empty,
                     }))
        {
            var namespaceName = string.IsNullOrWhiteSpace(interfaceGroup.Key.Namespace) ? null : interfaceGroup.Key.Namespace;
            var inheritedKey = $"{interfaceGroup.Key.Namespace}|{interfaceGroup.Key.Name}";
            AddByNamespace(
                interfacesByNamespace,
                namespaceName,
                EmitInterface(
                    interfaceGroup.Select(item => item.Declaration).ToArray(),
                    namespaceName,
                    inheritedTypeKeys.Contains(inheritedKey),
                    includesByTarget));
        }

        foreach (var namespaceGroup in inventory.Files
                     .SelectMany(file => file.Declarations
                         .Where(static declaration => declaration.Kind == "namespace" && !IsExcludedDeclarationName(declaration.Name))
                         .Select(declaration => new { file.Namespace, Declaration = declaration }))
                     .GroupBy(item => new
                     {
                         Namespace = item.Namespace ?? string.Empty,
                         Name = item.Declaration.Name ?? string.Empty,
                     }))
        {
            var namespaceName = string.IsNullOrWhiteSpace(namespaceGroup.Key.Namespace) ? null : namespaceGroup.Key.Namespace;
            AddByNamespace(
                namespacesByNamespace,
                namespaceName,
                EmitNamespace(namespaceGroup.Select(item => item.Declaration).ToArray(), namespaceName));

            globalUsings.Add(EmitNamespaceAlias(namespaceGroup.Key.Name, namespaceName));
        }

        foreach (var unionDefinition in _generatedUnionDefinitionsByQualifiedName.Values.OrderBy(static item => item.QualifiedTypeName, StringComparer.Ordinal))
        {
            AddByNamespace(unionsByNamespace, unionDefinition.NamespaceName, EmitUnion(unionDefinition));
        }

        await File.WriteAllTextAsync(
            Path.Combine(previewRoot, "GlobalUsings.cs"),
            NormalizeLineEndings(string.Join(Environment.NewLine, globalUsings.Distinct(StringComparer.Ordinal)) + Environment.NewLine),
            cancellationToken);

        await WriteGroupedFilesAsync(previewRoot, "Enums.cs", enumsByNamespace, cancellationToken);
        await WriteGroupedFilesAsync(previewRoot, "Callbacks.cs", callbacksByNamespace, cancellationToken);
        await WriteGroupedFilesAsync(previewRoot, "Dictionaries.cs", dictionariesByNamespace, cancellationToken);
        await WriteGroupedFilesAsync(previewRoot, "Interfaces.cs", interfacesByNamespace, cancellationToken);
        await WriteGroupedFilesAsync(previewRoot, "Namespaces.cs", namespacesByNamespace, cancellationToken);
        await WriteGroupedFilesAsync(previewRoot, "Unions.cs", unionsByNamespace, cancellationToken);
    }

    private string EmitTypedef(WebIdlDeclarationInventory declaration, string? namespaceName)
    {
        var name = WebIdlNaming.ToTypeName(declaration.Name ?? throw new InvalidOperationException("Typedef name is required."));
        var idlType = declaration.Payload.GetProperty("idlType");
        var aliasTarget = ResolveAliasTargetType(idlType, namespaceName, name, preferRequestedNameForFirstUnion: true);
        if (idlType.GetBooleanOrNull("union") == true
            && string.Equals(aliasTarget, GetQualifiedTypeName(namespaceName, name), StringComparison.Ordinal))
        {
            return string.Empty;
        }

        var exists = _typeMapper.TryResolveAliasValue(aliasTarget, out var existingValue);
        var value = exists ? existingValue : aliasTarget;
        if (value.EndsWith("?", StringComparison.Ordinal))
        {
            value = value[..^1];
        }

        _typeMapper.RegisterAlias(name, value);
        var mark = exists ? $"/*{aliasTarget}*/" : string.Empty;
        return $"global using {name} = {value};{mark}";
    }

    private string EmitEnum(WebIdlDeclarationInventory declaration)
    {
        var payload = declaration.Payload;
        var enumName = declaration.Name ?? throw new InvalidOperationException("Enum name is required.");
        var enumValues = payload.GetArray("values");
        var builder = new StringBuilder();
        AppendDocumentation(builder, declaration.Documentation);
        builder.AppendLine($"[Description(\"@#{enumName}\")]");
        builder.AppendLine("[ECMAScript]");
        builder.AppendLine("[String]");
        builder.AppendLine($"public enum {WebIdlNaming.ToTypeName(enumName)}");
        builder.AppendLine("{");

        for (var index = 0; index < enumValues.Count; index++)
        {
            var value = enumValues[index].GetStringOrNull("value") ?? string.Empty;
            var enumValueName = WebIdlNaming.ToPascalCase(string.IsNullOrEmpty(value) ? "Empty" : value);
            // WebIDL enum tokens are wire values. The C# name is only an authoring projection.
            AppendDocumentation(builder, GetMemberDocumentation(declaration, index)?.Documentation, 1);
            builder.AppendLine($"    [Description({JsonSerializer.Serialize($"@#{value}")})]");
            builder.Append($"    {enumValueName} = {index}");
            if (index < enumValues.Count - 1)
            {
                builder.AppendLine(",");
                builder.AppendLine();
            }
            else
            {
                builder.AppendLine();
            }
        }

        builder.Append('}');
        return builder.ToString();
    }

    private string EmitCallback(WebIdlDeclarationInventory declaration, string? namespaceName)
    {
        var payload = declaration.Payload;
        var callbackName = WebIdlNaming.ToTypeName(declaration.Name ?? throw new InvalidOperationException("Callback name is required."));
        var returnType = ResolveInlineType(payload.GetProperty("idlType"), namespaceName, callbackName, "void");
        var parameters = BuildParameterList(payload.GetArray("arguments"), namespaceName);

        var builder = new StringBuilder();
        AppendDocumentation(builder, declaration.Documentation);
        builder.AppendLine("[ECMAScript]");
        builder.AppendLine("[Description(\"@#\")]");
        builder.AppendLine("[Category(\"literal\")]");
        builder.Append($"public delegate {returnType} {callbackName}({parameters});");
        return builder.ToString();
    }

    private IReadOnlyList<string> EmitCallbackInterface(
        WebIdlDeclarationInventory declaration,
        string? namespaceName,
        ICollection<string> globalUsings)
    {
        var payload = declaration.Payload;
        var callbackInterfaceName = WebIdlNaming.ToTypeName(declaration.Name ?? throw new InvalidOperationException("Callback interface name is required."));
        var members = payload.GetArray("members");
        var operations = members.Where(static member => member.GetStringOrNull("type") == "operation").ToArray();
        if (operations.Length != 1)
        {
            throw new InvalidOperationException($"Callback interface '{declaration.Name}' must contain exactly one operation.");
        }

        var operationName = WebIdlNaming.ToPascalCase(operations[0].GetStringOrNull("name") ?? throw new InvalidOperationException("Operation name is required."));
        var aliasUnionType = ResolveInlineType(
            CreateUnionJson(
                CreateNamedTypeJson($"{callbackInterfaceName}Literal"),
                CreateNamedTypeJson($"{operationName}Callback")),
            namespaceName,
            callbackInterfaceName);
        globalUsings.Add($"global using {callbackInterfaceName} = {GetQualifiedTypeName(namespaceName, aliasUnionType)};");

        var operationIndex = Array.FindIndex(members.ToArray(), member => member.GetStringOrNull("type") == "operation");
        var emitted = new List<string>
        {
            EmitCallbackInterfaceDelegate(
                declaration.Name!,
                operations[0],
                namespaceName,
                declaration.Documentation,
                GetMemberDocumentation(declaration, operationIndex))
        };

        var partialKeyword = declaration.Partial == true ? " partial" : string.Empty;
        var builder = new StringBuilder();
        AppendDocumentation(builder, declaration.Documentation);
        builder.AppendLine("[ECMAScript]");
        builder.AppendLine("[Description(\"@#\")]");
        builder.AppendLine("[Category(\"literal\")]");
        builder.AppendLine($"public{partialKeyword} sealed class {callbackInterfaceName}Literal");
        builder.AppendLine("{");

        var bodyMembers = new List<string>();
        for (var memberIndex = 0; memberIndex < members.Count; memberIndex++)
        {
            var member = members[memberIndex];
            var memberDocumentation = GetMemberDocumentation(declaration, memberIndex);
            var type = member.GetStringOrNull("type");
            if (type == "operation")
            {
                var memberName = member.GetStringOrNull("name") ?? throw new InvalidOperationException("Operation name is required.");
                var callbackName = WebIdlNaming.ToPascalCase(memberName);
                var callbackBuilder = new StringBuilder();
                AppendDocumentation(callbackBuilder, memberDocumentation?.Documentation);
                callbackBuilder.AppendLine("[ECMAScript]");
                callbackBuilder.AppendLine($"[Description(\"@#{memberName}\")]" );
                callbackBuilder.Append($"public {callbackName}Callback? {callbackName} {{ get; set; }}");
                bodyMembers.Add(Indent(callbackBuilder.ToString(), 1));
            }
            else if (type == "const")
            {
                bodyMembers.Add(Indent(EmitConst(member, namespaceName, memberDocumentation?.Documentation), 1));
            }
        }

        builder.Append(string.Join(Environment.NewLine + Environment.NewLine, bodyMembers));
        builder.AppendLine();
        builder.Append('}');

        emitted.Add(builder.ToString());
        return emitted;
    }

    private string EmitCallbackInterfaceDelegate(
        string interfaceName,
        JsonElement operation,
        string? namespaceName,
        WebIdlDocumentation? interfaceDocumentation,
        WebIdlMemberDocumentation? operationDocumentation)
    {
        var callbackName = WebIdlNaming.ToPascalCase(operation.GetStringOrNull("name") ?? throw new InvalidOperationException("Operation name is required."));
        var returnType = ResolveInlineType(operation.GetProperty("idlType"), namespaceName, callbackName, "void");
        var parameters = BuildParameterList(operation.GetArray("arguments"), namespaceName);

        var builder = new StringBuilder();
        AppendDocumentation(builder, operationDocumentation?.Documentation ?? interfaceDocumentation);
        builder.AppendLine("[ECMAScript]");
        builder.AppendLine("[Description(\"@#\")]");
        builder.AppendLine("[Category(\"literal\")]");
        builder.Append($"public delegate {returnType} {callbackName}Callback({parameters});");
        return builder.ToString();
    }

    private string EmitConst(JsonElement member, string? namespaceName, WebIdlDocumentation? documentation = null)
    {
        var memberName = member.GetStringOrNull("name") ?? throw new InvalidOperationException("Const name is required.");
        var type = ResolveInlineType(member.GetProperty("idlType"), namespaceName, WebIdlNaming.ToPascalCase(memberName));
        var propertyName = WebIdlNaming.ToPascalCase(member.GetStringOrNull("name") ?? throw new InvalidOperationException("Const name is required."));
        var value = _typeMapper.FormatValue(member.GetProperty("value"), member.GetProperty("idlType"), namespaceName);

        var builder = new StringBuilder();
        AppendDocumentation(builder, documentation);
        builder.AppendLine($"[Description(\"@#{member.GetStringOrNull("name")}\")]" );
        builder.Append($"public const {type} {propertyName} = {value};");
        return builder.ToString();
    }

    private static void AddByNamespace(IDictionary<string, List<string>> target, string? namespaceName, string code)
    {
        var key = namespaceName ?? string.Empty;
        if (!target.TryGetValue(key, out var list))
        {
            list = [];
            target[key] = list;
        }

        list.Add(code);
    }

    private static async Task WriteGroupedFilesAsync(
        string previewRoot,
        string fileName,
        IReadOnlyDictionary<string, List<string>> grouped,
        CancellationToken cancellationToken)
    {
        foreach (var pair in grouped)
        {
            var directory = string.IsNullOrWhiteSpace(pair.Key)
                ? previewRoot
                : Path.Combine(previewRoot, WebIdlNaming.ToPascalCase(pair.Key));
            Directory.CreateDirectory(directory);

            var namespaceLine = string.IsNullOrWhiteSpace(pair.Key)
                ? "namespace ECMAScript;"
                : $"namespace ECMAScript.{WebIdlNaming.ToPascalCase(pair.Key)};";

            var content = namespaceLine
                + Environment.NewLine
                + Environment.NewLine
                + string.Join(Environment.NewLine + Environment.NewLine, pair.Value.OrderBy(static item => item, StringComparer.Ordinal))
                + Environment.NewLine;
            await File.WriteAllTextAsync(Path.Combine(directory, fileName), NormalizeLineEndings(content), cancellationToken);
        }
    }

    private static string GetQualifiedTypeName(string? namespaceName, string typeName)
    {
        return string.IsNullOrWhiteSpace(namespaceName)
            ? $"ECMAScript.{typeName}"
            : $"ECMAScript.{WebIdlNaming.ToPascalCase(namespaceName)}.{typeName}";
    }

    private static bool IsExcludedDeclarationName(string? declarationName)
    {
        return !string.IsNullOrWhiteSpace(declarationName)
            && ExcludedDeclarationNames.Contains(WebIdlNaming.ToPascalCase(declarationName));
    }

    private static string NormalizeLineEndings(string text)
    {
        return text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Replace("\n", "\r\n", StringComparison.Ordinal);
    }

    private static string Indent(string text, int level)
    {
        var prefix = new string(' ', level * 4);
        var normalized = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        // Keep separator lines truly empty. Prefixing them would create trailing whitespace in
        // every generated member block, especially now that XML documentation adds blank lines.
        // 空的分隔行必须保持为空；否则 XML 文档生成后会在每个成员块之间产生行尾空白。
        return string.Join(Environment.NewLine, normalized.Split('\n').Select(line => line.Length == 0 ? string.Empty : prefix + line));
    }

    private static void AppendDocumentation(StringBuilder builder, WebIdlDocumentation? documentation, int level = 0)
    {
        if (documentation is null)
        {
            return;
        }

        var indent = new string(' ', level * 4);
        builder.Append(indent).AppendLine("/// <summary>");
        builder.Append(indent).Append("/// ");
        if (string.IsNullOrWhiteSpace(documentation.Prose))
        {
            AppendDocumentationLink(builder, documentation);
        }
        else
        {
            builder.Append(EscapeXmlDocumentation(documentation.Prose));
        }

        builder.AppendLine();
        builder.Append(indent).AppendLine("/// </summary>");

        if (!string.IsNullOrWhiteSpace(documentation.Prose))
        {
            builder.Append(indent).AppendLine("/// <remarks>");
            builder.Append(indent).Append("/// ");
            AppendDocumentationLink(builder, documentation);
            builder.AppendLine();
            builder.Append(indent).AppendLine("/// </remarks>");
        }

        if (!string.IsNullOrWhiteSpace(documentation.Usage))
        {
            builder.Append(indent).AppendLine("/// <example>");
            builder.Append(indent).Append("/// <code>")
                .Append(EscapeXmlDocumentation(documentation.Usage))
                .AppendLine("</code>");
            builder.Append(indent).AppendLine("/// </example>");
        }
    }

    private static void AppendParameterDocumentation(StringBuilder builder, IEnumerable<MethodParameterEmission> parameters, int level = 0)
    {
        var indent = new string(' ', level * 4);
        foreach (var parameter in parameters)
        {
            if (parameter.Documentation is null)
            {
                continue;
            }

            builder.Append(indent).Append("/// <param name=\"")
                .Append(EscapeXmlDocumentation(parameter.CommentName))
                .Append("\">");
            if (!string.IsNullOrWhiteSpace(parameter.Documentation.Prose))
            {
                builder.Append(EscapeXmlDocumentation(parameter.Documentation.Prose)).Append(' ');
            }

            AppendDocumentationLink(builder, parameter.Documentation);
            builder.AppendLine("</param>");
        }
    }

    private static void AppendDictionaryParameterDocumentation(
        StringBuilder builder,
        IEnumerable<DictionaryParameterEmission> parameters,
        int level = 0)
    {
        var indent = new string(' ', level * 4);
        foreach (var parameter in parameters)
        {
            if (parameter.Documentation is null)
            {
                continue;
            }

            builder.Append(indent).Append("/// <param name=\"")
                .Append(EscapeXmlDocumentation(parameter.PascalName))
                .Append("\">");
            if (!string.IsNullOrWhiteSpace(parameter.Documentation.Prose))
            {
                builder.Append(EscapeXmlDocumentation(parameter.Documentation.Prose)).Append(' ');
            }

            AppendDocumentationLink(builder, parameter.Documentation);
            builder.AppendLine("</param>");
        }
    }

    private static void AppendDocumentationLink(StringBuilder builder, WebIdlDocumentation documentation)
    {
        var label = string.IsNullOrWhiteSpace(documentation.Heading)
            ? documentation.SpecificationTitle
            : $"{documentation.SpecificationTitle}: {documentation.Heading}";
        builder.Append("<see href=\"")
            .Append(EscapeXmlDocumentation(documentation.Href))
            .Append("\">")
            .Append(EscapeXmlDocumentation(label))
            .Append("</see>");
    }

    private static string EscapeXmlDocumentation(string value)
        => value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("'", "&apos;", StringComparison.Ordinal);

    private string BuildParameterList(IReadOnlyList<JsonElement> arguments, string? namespaceName)
    {
        return string.Join(", ", arguments.Select(argument =>
        {
            var argumentName = argument.GetStringOrNull("name") ?? string.Empty;
            var type = ResolveInlineType(
                argument.GetProperty("idlType"),
                namespaceName,
                CombineUnionBaseName("Parameter", WebIdlNaming.ToPascalCase(argumentName)));
            var name = WebIdlNaming.ToCamelCase(argument.GetStringOrNull("name") ?? string.Empty);
            return $"{type} {name}";
        }));
    }

    private string EmitDictionary(IReadOnlyList<WebIdlDeclarationInventory> declarations, string? namespaceName)
    {
        var name = declarations[0].Name ?? throw new InvalidOperationException("Dictionary name is required.");
        var recordName = WebIdlNaming.ToTypeName(name);
        var inheritances = declarations
            .Select(static declaration => declaration.Inheritance)
            .Where(static inheritance => !string.IsNullOrWhiteSpace(inheritance))
            .Select(static inheritance => WebIdlNaming.ToTypeName(inheritance!))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var inheritanceClause = inheritances.Length > 0 ? $" : {string.Join(", ", inheritances)}" : string.Empty;

        var parameterGroups = declarations
            .Select(declaration => declaration.Payload.GetArray("members")
                .Select((member, memberIndex) => EmitDictionaryParameter(
                    member,
                    recordName,
                    namespaceName,
                    GetMemberDocumentation(declaration, memberIndex)?.Documentation))
                .ToArray())
            .ToArray();

        if (parameterGroups.Length == 1 && parameterGroups[0].Length == 0)
        {
            var emptyDictionary = new StringBuilder();
            AppendDocumentation(emptyDictionary, SelectDocumentation(declarations));
            emptyDictionary.AppendLine("[ECMAScript]");
            emptyDictionary.AppendLine($"[Description(\"@#{name}\")]");
            emptyDictionary.Append($"public abstract record {recordName}();");
            return emptyDictionary.ToString();
        }

        var builder = new StringBuilder();
        AppendDocumentation(builder, SelectDocumentation(declarations));
        AppendDictionaryParameterDocumentation(builder, parameterGroups.SelectMany(static group => group));
        builder.AppendLine("[ECMAScript]");
        builder.AppendLine($"[Description(\"@#{name}\")]");
        builder.AppendLine($"public record {recordName}(");
        builder.Append("    ");
        builder.Append(string.Join("," + Environment.NewLine + "    ", parameterGroups.SelectMany(static group => group).Select(static parameter => parameter.Code)));
        builder.Append($"){inheritanceClause}");

        if (parameterGroups.Length == 1)
        {
            builder.Append(';');
            return builder.ToString();
        }

        builder.AppendLine();
        builder.AppendLine("{");
        for (var index = 0; index < parameterGroups.Length; index++)
        {
            if (index > 0)
            {
                builder.AppendLine();
            }

            var group = parameterGroups[index];
            var methodNameSuffix = group.Length > 3
                ? $"{string.Concat(group.Take(3).Select(static item => item.PascalName))}{group.Length}"
                : string.Concat(group.Select(static item => item.PascalName));
            builder.AppendLine("    [Category(\"optional\")]");
            builder.Append($"    public extern static {recordName} Optional{methodNameSuffix}(");
            if (group.Length == 0)
            {
                builder.AppendLine(");");
                continue;
            }

            builder.AppendLine();
            builder.Append("        ");
            builder.Append(string.Join("," + Environment.NewLine + "        ", group.Select(static parameter => parameter.ArgumentCode)));
            builder.AppendLine(");");
        }

        builder.Append('}');
        return builder.ToString();
    }

    private DictionaryParameterEmission EmitDictionaryParameter(
        JsonElement member,
        string ownerName,
        string? namespaceName,
        WebIdlDocumentation? documentation)
    {
        var memberName = member.GetStringOrNull("name") ?? throw new InvalidOperationException("Dictionary member name is required.");
        var pascalName = WebIdlNaming.ToPascalCase(memberName);
        var type = ResolveInlineType(member.GetProperty("idlType"), namespaceName, CombineUnionBaseName(ownerName, pascalName));
        var typeKey = type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type;
        var hasDefault = member.TryGetProperty("default", out var defaultValue) && defaultValue.ValueKind != JsonValueKind.Null;
        var isEnumType = _typeMapper.IsEnumType(typeKey);
        var isNullable = member.GetProperty("idlType").GetBooleanOrNull("nullable") == true;
        var isRequired = member.GetBooleanOrNull("required") == true;

        if (hasDefault)
        {
            var value = _typeMapper.FormatValue(defaultValue, member.GetProperty("idlType"), namespaceName);
            if (isEnumType)
            {
                var enumValue = NormalizeEnumValue(value);
                // Specs occasionally retain an older wire-token default after
                // removing that token from the enum. Preserve compilability and
                // let the all-zero C# default represent the omitted value.
                value = _typeMapper.HasEnumValue(typeKey, enumValue)
                    ? $"{typeKey}.{enumValue}"
                    : "default";
            }

            if (member.GetProperty("idlType").GetBooleanOrNull("union") == true)
            {
                value = isNullable ? "default" : $"new()/*{value}*/";
            }

            value = AddNumericSuffix(type, value);

            if (!isRequired && !isNullable && !_typeMapper.IsOptionalPrimitive(typeKey) && !isEnumType)
            {
                type = $"{typeKey}?";
                value = "default";
            }

            return new DictionaryParameterEmission(
                pascalName,
                $"[property: Description(\"@#{memberName}\")]{type} {pascalName} = {value}",
                $"[Description(\"@#{memberName}\")]{type} {WebIdlNaming.ToCamelCase(memberName)} = {value}",
                documentation);
        }

        var propertyType = _typeMapper.IsOptionalPrimitive(typeKey) ? typeKey : $"{typeKey}?";
        return new DictionaryParameterEmission(
            pascalName,
            $"[property: Description(\"@#{memberName}\")]{propertyType} {pascalName} = default",
            $"[Description(\"@#{memberName}\")]{propertyType} {pascalName} = default",
            documentation);
    }

    private static string NormalizeEnumValue(string value)
    {
        var unwrapped = value.Trim('"');
        return WebIdlNaming.ToPascalCase(string.IsNullOrEmpty(unwrapped) ? "Empty" : unwrapped);
    }

    private static string AddNumericSuffix(string type, string value)
    {
        return type switch
        {
            "double" => $"{value}d",
            "float" => $"{value}f",
            _ => value,
        };
    }

    private static bool CanUseLiteralOptionalParameterDefault(string typeKey, bool isEnumType)
    {
        if (isEnumType)
        {
            return true;
        }

        return typeKey switch
        {
            "string" or
            "bool" or
            "sbyte" or
            "byte" or
            "short" or
            "ushort" or
            "int" or
            "uint" or
            "long" or
            "ulong" or
            "float" or
            "double" => true,
            _ => false,
        };
    }

    private string EmitInterface(
        IReadOnlyList<WebIdlDeclarationInventory> declarations,
        string? namespaceName,
        bool isInheritedByOtherType,
        IReadOnlyDictionary<string, string[]> includesByTarget)
    {
        var originalName = declarations[0].Name ?? throw new InvalidOperationException("Interface name is required.");
        var className = WebIdlNaming.ToTypeName(originalName);
        var inheritances = declarations
            .Select(static declaration => declaration.Inheritance)
            .Where(static inheritance => !string.IsNullOrWhiteSpace(inheritance))
            .Select(static inheritance => WebIdlNaming.ToTypeName(inheritance!))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (inheritances.Length > 1)
        {
            throw new InvalidOperationException($"Interface '{originalName}' has multiple inheritance entries, which is not supported yet.");
        }

        var members = declarations
            .SelectMany(declaration => declaration.Payload.GetArray("members"))
            .ToArray();
        var distinctMembers = DistinctMembers(members, namespaceName);
        var memberDocumentationByKey = new Dictionary<string, WebIdlMemberDocumentation>(
            BuildMemberDocumentationByKey(declarations, namespaceName),
            StringComparer.Ordinal);
        var effectiveMembers = GetEffectiveInterfaceMembers(declarations, namespaceName, includesByTarget);
        var interfaceKey = BuildTypeKey(namespaceName, originalName);
        _interfaceCachesByKey.TryGetValue(interfaceKey, out var interfaceCache);
        var inheritanceParts = new List<string>();
        var primaryConstructor = string.Empty;
        if (inheritances.Length == 1)
        {
            var parentName = inheritances[0];
            var parentKey = ResolveInterfaceKey(
                namespaceName,
                declarations.Select(static declaration => declaration.Inheritance).FirstOrDefault(static inheritance => !string.IsNullOrWhiteSpace(inheritance)));
            if (parentKey is not null && _interfaceCachesByKey.TryGetValue(parentKey, out var parentCache))
            {
                var noParameterConstructors = parentCache.Constructors.Where(static ctor => ctor.ParameterCount == 0).ToArray();
                var childConstructors = distinctMembers
                    .Where(static member => member.GetStringOrNull("type") == "constructor")
                    .Select(member => BuildConstructorCache(member, originalName, namespaceName))
                    .ToArray();
                if (parentCache.Constructors.Count > 0 && noParameterConstructors.Length == 0)
                {
                    var hasSameParameterConstructor = childConstructors.Any(childCtor =>
                        parentCache.Constructors.Any(parentCtor => parentCtor.TypeSignature == childCtor.TypeSignature));
                    if (hasSameParameterConstructor)
                    {
                        inheritanceParts.Add(parentName);
                    }
                    else
                    {
                        var parentConstructor = parentCache.Constructors[0];
                        primaryConstructor = $"({parentConstructor.ParameterList})";
                        inheritanceParts.Add($"{parentName}({parentConstructor.ArgumentList})");
                    }
                }
                else
                {
                    inheritanceParts.Add(parentName);
                }
            }
            else
            {
                inheritanceParts.Add(parentName);
            }
        }

        var iterableInterface = distinctMembers.FirstOrDefault(static member => member.GetStringOrNull("type") == "iterable");
        if (iterableInterface.ValueKind != JsonValueKind.Undefined)
        {
            inheritanceParts.Add(GetIterableInterface(iterableInterface, namespaceName));
        }

        var maplikeInterface = distinctMembers.FirstOrDefault(static member => member.GetStringOrNull("type") == "maplike");
        if (maplikeInterface.ValueKind != JsonValueKind.Undefined)
        {
            inheritanceParts.Add(GetMaplikeInterface(maplikeInterface, namespaceName));
        }

        var setlikeInterface = distinctMembers.FirstOrDefault(static member => member.GetStringOrNull("type") == "setlike");
        if (setlikeInterface.ValueKind != JsonValueKind.Undefined)
        {
            inheritanceParts.Add(GetSetlikeInterface(setlikeInterface, namespaceName));
        }

        var inheritanceClause = inheritanceParts.Count > 0 ? $" : {string.Join(", ", inheritanceParts.Distinct(StringComparer.Ordinal))}" : string.Empty;
        var accessorInfo = BuildAccessorInfo(effectiveMembers);
        var emissionContext = new InterfaceEmissionContext(
            OwnerName: originalName,
            NamespaceName: namespaceName,
            InterfaceKey: interfaceKey,
            Cache: interfaceCache,
            ForceStatic: false,
            EnableInheritance: true,
            MemberDocumentationByKey: memberDocumentationByKey);
        var bodyMembers = distinctMembers
            .Select(member => EmitInterfaceMember(member, emissionContext, accessorInfo))
            .Where(static code => !string.IsNullOrWhiteSpace(code))
            .ToList();
        var emittedMemberKeys = new HashSet<string>(distinctMembers.Select(member => BuildMemberKey(member, namespaceName)), StringComparer.Ordinal);

        var includeKey = $"{namespaceName ?? string.Empty}|{originalName}";
        if (includesByTarget.TryGetValue(includeKey, out var includeMixins))
        {
            foreach (var mixinName in includeMixins)
            {
                var mixinKey = $"{namespaceName ?? string.Empty}|{mixinName}";
                if (_mixinMembersByKey.TryGetValue(mixinKey, out var mixinMembers))
                {
                    if (_mixinMemberDocumentationByKey.TryGetValue(mixinKey, out var mixinDocumentation))
                    {
                        foreach (var (key, documentation) in mixinDocumentation)
                        {
                            memberDocumentationByKey.TryAdd(key, documentation);
                        }
                    }

                    var regionMembers = new List<JsonElement>();
                    foreach (var mixinMember in mixinMembers)
                    {
                        if (emittedMemberKeys.Add(BuildMemberKey(mixinMember, namespaceName)))
                        {
                            regionMembers.Add(mixinMember);
                        }
                    }

                    var mixinBody = string.Join(
                        Environment.NewLine + Environment.NewLine,
                        regionMembers
                            .Select(member => EmitInterfaceMember(member, emissionContext, accessorInfo))
                            .Where(static code => !string.IsNullOrWhiteSpace(code)));
                    if (!string.IsNullOrWhiteSpace(mixinBody))
                    {
                        bodyMembers.Add($"#region mixin {mixinName}{Environment.NewLine}{mixinBody}{Environment.NewLine}#endregion");
                    }
                }
            }
        }

        var partialKeyword = declarations.Any(static declaration => declaration.Partial == true) ? " partial" : string.Empty;
        var abstractKeyword = inheritances.Length == 0 && bodyMembers.Count == 0 && isInheritedByOtherType ? " abstract" : string.Empty;
        var builder = new StringBuilder();
        AppendDocumentation(builder, SelectDocumentation(declarations));
        builder.AppendLine("[ECMAScript]");
        builder.AppendLine($"[Description(\"@#{originalName}\")]");
        builder.AppendLine($"public{abstractKeyword}{partialKeyword} class {className}{primaryConstructor}{inheritanceClause}");
        builder.AppendLine("{");
        if (bodyMembers.Count > 0)
        {
            builder.AppendLine(string.Join(Environment.NewLine + Environment.NewLine, bodyMembers.OfType<string>().Select(static member => Indent(member, 1))));
        }

        builder.Append('}');
        return builder.ToString();
    }

    private string EmitNamespace(
        IReadOnlyList<WebIdlDeclarationInventory> declarations,
        string? namespaceName)
    {
        var originalName = declarations[0].Name ?? throw new InvalidOperationException("Namespace name is required.");
        var className = WebIdlNaming.ToTypeName(originalName);
        var members = declarations
            .SelectMany(declaration => declaration.Payload.GetArray("members"))
            .ToArray();
        var distinctMembers = DistinctMembers(members, namespaceName);
        var memberDocumentationByKey = BuildMemberDocumentationByKey(declarations, namespaceName);
        var accessorInfo = BuildAccessorInfo(distinctMembers);
        var emissionContext = new InterfaceEmissionContext(
            OwnerName: originalName,
            NamespaceName: namespaceName,
            InterfaceKey: BuildTypeKey(namespaceName, originalName),
            Cache: null,
            ForceStatic: true,
            EnableInheritance: false,
            MemberDocumentationByKey: memberDocumentationByKey);
        var bodyMembers = distinctMembers
            .Select(member => EmitNamespaceMember(member, emissionContext, accessorInfo))
            .Where(static code => !string.IsNullOrWhiteSpace(code))
            .ToList();

        var partialKeyword = declarations.Any(static declaration => declaration.Partial == true) ? " partial" : string.Empty;
        var builder = new StringBuilder();
        AppendDocumentation(builder, SelectDocumentation(declarations));
        builder.AppendLine("[ECMAScript]");
        builder.AppendLine($"[Description(\"@#{originalName}\")]");
        builder.AppendLine($"public static{partialKeyword} class {className}");
        builder.AppendLine("{");
        if (bodyMembers.Count > 0)
        {
            builder.AppendLine(string.Join(Environment.NewLine + Environment.NewLine, bodyMembers.OfType<string>().Select(static member => Indent(member, 1))));
        }

        builder.Append('}');
        return builder.ToString();
    }

    private static string EmitNamespaceAlias(string originalName, string? namespaceName)
    {
        var aliasName = EscapeIdentifier(originalName);
        var className = WebIdlNaming.ToTypeName(originalName);
        return $"global using {aliasName} = {GetQualifiedTypeName(namespaceName, className)};";
    }

    private string? EmitInterfaceMember(JsonElement member, InterfaceEmissionContext context, AccessorInfo accessorInfo)
    {
        var documentation = GetMemberDocumentation(context, member);
        var type = member.GetStringOrNull("type");
        return type switch
        {
            "constructor" => EmitConstructor(member, context.OwnerName, context.NamespaceName, documentation),
            "attribute" => EmitAttribute(member, context, documentation?.Documentation),
            "const" => EmitConst(member, context.NamespaceName, documentation?.Documentation),
            "operation" => EmitOperation(member, context, accessorInfo, documentation),
            "iterable" => EmitIterableMember(member, context.NamespaceName, documentation?.Documentation),
            "maplike" => EmitMaplikeMember(member, context.NamespaceName, documentation?.Documentation),
            "setlike" => EmitSetlikeMember(member, context.NamespaceName, documentation?.Documentation),
            _ => null,
        };
    }

    private string? EmitNamespaceMember(JsonElement member, InterfaceEmissionContext context, AccessorInfo accessorInfo)
    {
        var documentation = GetMemberDocumentation(context, member);
        var type = member.GetStringOrNull("type");
        return type switch
        {
            "attribute" => EmitAttribute(member, context, documentation?.Documentation),
            "const" => EmitConst(member, context.NamespaceName, documentation?.Documentation),
            "operation" => EmitOperation(member, context, accessorInfo, documentation),
            _ => null,
        };
    }

    private string? EmitAttribute(
        JsonElement attribute,
        InterfaceEmissionContext context,
        WebIdlDocumentation? documentation)
    {
        var originalName = attribute.GetStringOrNull("name") ?? string.Empty;
        var propertyName = GetAttributePropertyName(originalName, context.OwnerName);
        var propertyType = ResolveInlineType(
            attribute.GetProperty("idlType"),
            context.NamespaceName,
            CombineUnionBaseName(context.OwnerName, propertyName));
        var inheritanceDisposition = context.EnableInheritance
            ? GetPropertyInheritanceDisposition(context, propertyName, propertyType)
            : InheritanceDisposition.None;
        if (inheritanceDisposition == InheritanceDisposition.Skip)
        {
            return null;
        }

        var isStatic = context.ForceStatic || attribute.GetStringOrNull("special") == "static";
        var inheritanceModifier = inheritanceDisposition == InheritanceDisposition.New ? "new " : string.Empty;
        var builder = new StringBuilder();
        AppendDocumentation(builder, documentation);
        builder.AppendLine($"[Description(\"@#{originalName}\")]");
        builder.Append($"public {inheritanceModifier}{(isStatic ? "static " : string.Empty)}extern {propertyType} {propertyName} {{ get;{(attribute.GetBooleanOrNull("readonly") == true ? string.Empty : " set;")} }}");
        return builder.ToString();
    }

    private string EmitConstructor(
        JsonElement constructor,
        string ownerName,
        string? namespaceName,
        WebIdlMemberDocumentation? documentation)
    {
        var arguments = constructor.GetArray("arguments");
        var parameters = BuildMethodParameters(arguments, namespaceName, ownerName, documentation);

        var builder = new StringBuilder();
        AppendDocumentation(builder, documentation?.Documentation);
        AppendParameterDocumentation(builder, parameters);
        builder.Append($"public extern {WebIdlNaming.ToTypeName(ownerName)}({string.Join(", ", parameters.Select(static parameter => parameter.Signature))});");
        return builder.ToString();
    }

    private string? EmitOperation(
        JsonElement operation,
        InterfaceEmissionContext context,
        AccessorInfo accessorInfo,
        WebIdlMemberDocumentation? documentation)
    {
        var special = operation.GetStringOrNull("special") ?? string.Empty;
        var operationName = operation.GetStringOrNull("name");
        var returnType = operation.TryGetProperty("idlType", out var operationType)
            ? ResolveInlineType(
                operationType,
                context.NamespaceName,
                CombineUnionBaseName(
                    CombineUnionBaseName(context.OwnerName, WebIdlNaming.ToPascalCase(operationName ?? special)),
                    "Result"),
                "void")
            : "void";
        var arguments = operation.GetArray("arguments");

        if (string.IsNullOrWhiteSpace(operationName))
        {
            var unnamedDisposition = GetOperationInheritanceDisposition(context, special, string.Empty, arguments, returnType);
            if (unnamedDisposition == InheritanceDisposition.Skip)
            {
                return null;
            }

            var unnamedInheritanceModifier = unnamedDisposition == InheritanceDisposition.New ? "new " : string.Empty;
            var unnamedParameters = BuildMethodParameters(
                arguments,
                context.NamespaceName,
                CombineUnionBaseName(context.OwnerName, WebIdlNaming.ToPascalCase(special)),
                documentation);
            return special switch
            {
                "stringifier" => null,
                "getter" => EmitIndexerGetter(arguments, returnType, accessorInfo, context.NamespaceName, unnamedInheritanceModifier, documentation),
                "setter" => EmitIndexerSetter(arguments, context.OwnerName, accessorInfo, context.NamespaceName, unnamedInheritanceModifier, documentation),
                "deleter" => EmitDeleter(unnamedParameters, unnamedInheritanceModifier, documentation?.Documentation),
                _ => null,
            };
        }

        var methodName = GetOperationMethodName(operationName);
        var inheritanceDisposition = context.EnableInheritance
            ? GetOperationInheritanceDisposition(context, special, methodName, arguments, returnType)
            : InheritanceDisposition.None;
        if (inheritanceDisposition == InheritanceDisposition.Skip)
        {
            return null;
        }

        var parameters = BuildMethodParameters(
            arguments,
            context.NamespaceName,
            CombineUnionBaseName(context.OwnerName, methodName),
            documentation);
        var isStatic = context.ForceStatic || special == "static";
        var inheritanceModifier = inheritanceDisposition == InheritanceDisposition.New ? "new " : string.Empty;
        var builder = new StringBuilder();
        AppendDocumentation(builder, documentation?.Documentation);
        AppendParameterDocumentation(builder, parameters);

        builder.AppendLine($"[Description(\"@#{operationName}\")]");
        builder.Append($"public {inheritanceModifier}{(isStatic ? "static " : string.Empty)}extern {returnType} {methodName}({string.Join(", ", parameters.Select(static parameter => parameter.Signature))});");

        var lastArgument = arguments.LastOrDefault();
        if (lastArgument.ValueKind != JsonValueKind.Undefined
            && lastArgument.TryGetProperty("idlType", out var lastIdlType)
            && lastIdlType.GetBooleanOrNull("union") == true
            && ShouldEmitUnionTailOverloads(
                lastIdlType,
                context.NamespaceName,
                methodName,
                WebIdlNaming.ToPascalCase(lastArgument.GetStringOrNull("name") ?? string.Empty)))
        {
            var overloads = EmitUnionTailOverloads(
                operationName,
                methodName,
                returnType,
                isStatic,
                inheritanceModifier,
                arguments,
                context.NamespaceName,
                CombineUnionBaseName(context.OwnerName, methodName),
                documentation);
            if (overloads.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.Append(string.Join(Environment.NewLine + Environment.NewLine, overloads));
            }
        }

        return builder.ToString();
    }

    private bool ShouldEmitUnionTailOverloads(
        JsonElement unionIdlType,
        string? namespaceName,
        string methodName,
        string lastArgumentName)
    {
        var resolvedUnionType = ResolveInlineType(
            unionIdlType,
            namespaceName,
            CombineUnionBaseName(methodName, lastArgumentName));

        return !string.IsNullOrWhiteSpace(resolvedUnionType);
    }

    private string? EmitIndexerGetter(
        IReadOnlyList<JsonElement> arguments,
        string returnType,
        AccessorInfo accessorInfo,
        string? namespaceName,
        string inheritanceModifier,
        WebIdlMemberDocumentation? documentation)
    {
        var parameters = BuildMethodParameters(arguments, namespaceName, "Indexer", documentation);
        var builder = new StringBuilder();
        AppendDocumentation(builder, documentation?.Documentation);
        AppendParameterDocumentation(builder, parameters);
        builder.AppendLine("[Description(\"@#\")]");
        builder.Append(accessorInfo.HasSetter
            ? $"public extern {inheritanceModifier}{returnType} this[{string.Join(", ", parameters.Select(static parameter => parameter.Signature))}] {{ get; set; }}"
            : $"public extern {inheritanceModifier}{returnType} this[{string.Join(", ", parameters.Select(static parameter => parameter.Signature))}] {{ get; }}");
        return builder.ToString();
    }

    private string? EmitIndexerSetter(
        IReadOnlyList<JsonElement> arguments,
        string ownerName,
        AccessorInfo accessorInfo,
        string? namespaceName,
        string inheritanceModifier,
        WebIdlMemberDocumentation? documentation)
    {
        if (accessorInfo.HasGetter)
        {
            return null;
        }

        if (arguments.Count != 2)
        {
            throw new InvalidOperationException($"Setter operation on '{ownerName}' must have exactly two arguments.");
        }

        var indexType = ResolveInlineType(arguments[0].GetProperty("idlType"), namespaceName, "IndexerKey");
        var indexName = WebIdlNaming.ToCamelCase(arguments[0].GetStringOrNull("name") ?? string.Empty);
        var valueType = ResolveInlineType(arguments[1].GetProperty("idlType"), namespaceName, "IndexerValue");
        var builder = new StringBuilder();
        AppendDocumentation(builder, documentation?.Documentation);
        builder.AppendLine("[Description(\"@#\")]");
        builder.Append($"public extern {inheritanceModifier}{valueType} this[{indexType} {indexName}] {{ set; }}");
        return builder.ToString();
    }

    private static string EmitDeleter(
        IReadOnlyList<MethodParameterEmission> parameters,
        string inheritanceModifier,
        WebIdlDocumentation? documentation)
    {
        var builder = new StringBuilder();
        AppendDocumentation(builder, documentation);
        AppendParameterDocumentation(builder, parameters);
        builder.AppendLine("[Description(\"@#\")]");
        builder.AppendLine($"[Jazor(\"{BuildDeleterInlineTemplate(parameters)}\")]");
        builder.Append($"public extern {inheritanceModifier}void Delete({string.Join(", ", parameters.Select(static parameter => parameter.Signature))});");
        return builder.ToString();
    }

    private string EmitIterableMember(JsonElement member, string? namespaceName, WebIdlDocumentation? documentation)
    {
        var types = member.GetArray("idlType");
        var returnType = types.Count == 1
            ? ResolveInlineType(types[0], namespaceName, "IterableItem")
            : $"({ResolveInlineType(types[0], namespaceName, "IterableKey")}, {ResolveInlineType(types[1], namespaceName, "IterableValue")})";

        var builder = new StringBuilder();
        AppendDocumentation(builder, documentation);
        builder.Append($"extern IEnumerator<{returnType}> IEnumerable<{returnType}>.GetEnumerator();{Environment.NewLine}extern IEnumerator IEnumerable.GetEnumerator();");
        return builder.ToString();
    }

    private string EmitMaplikeMember(JsonElement member, string? namespaceName, WebIdlDocumentation? documentation)
    {
        var keyType = ResolveInlineType(member.GetArray("idlType")[0], namespaceName, "MaplikeKey");
        var valueType = ResolveInlineType(member.GetArray("idlType")[1], namespaceName, "MaplikeValue");
        var builder = new StringBuilder();
        AppendDocumentation(builder, documentation);
        builder.Append("#region Dictionary\n")
            .Append($"extern {valueType} IDictionary<{keyType}, {valueType}>.this[{keyType} key] {{ get; set; }}\n")
            .Append($"extern ICollection<{keyType}> IDictionary<{keyType}, {valueType}>.Keys {{ get; }}\n")
            .Append($"extern ICollection<{valueType}> IDictionary<{keyType}, {valueType}>.Values {{ get; }}\n")
            .Append($"extern int ICollection<KeyValuePair<{keyType}, {valueType}>>.Count {{ get; }}\n")
            .Append($"extern bool ICollection<KeyValuePair<{keyType}, {valueType}>>.IsReadOnly {{ get; }}\n")
            .Append($"extern void IDictionary<{keyType}, {valueType}>.Add({keyType} key, {valueType} value);\n")
            .Append($"extern void ICollection<KeyValuePair<{keyType}, {valueType}>>.Add(KeyValuePair<{keyType}, {valueType}> item);\n")
            .Append($"extern void ICollection<KeyValuePair<{keyType}, {valueType}>>.Clear();\n")
            .Append($"extern bool ICollection<KeyValuePair<{keyType}, {valueType}>>.Contains(KeyValuePair<{keyType}, {valueType}> item);\n")
            .Append($"extern bool IDictionary<{keyType}, {valueType}>.ContainsKey({keyType} key);\n")
            .Append($"extern void ICollection<KeyValuePair<{keyType}, {valueType}>>.CopyTo(KeyValuePair<{keyType}, {valueType}>[] array, int arrayIndex);\n")
            .Append($"extern IEnumerator<KeyValuePair<{keyType}, {valueType}>> IEnumerable<KeyValuePair<{keyType}, {valueType}>>.GetEnumerator();\n")
            .Append($"extern bool IDictionary<{keyType}, {valueType}>.Remove({keyType} key);\n")
            .Append($"extern bool ICollection<KeyValuePair<{keyType}, {valueType}>>.Remove(KeyValuePair<{keyType}, {valueType}> item);\n")
            .Append($"extern bool IDictionary<{keyType}, {valueType}>.TryGetValue({keyType} key, [MaybeNullWhen(false)] out {valueType} value);\n")
            .Append("extern IEnumerator IEnumerable.GetEnumerator();\n")
            .Append("#endregion");
        return builder.ToString();
    }

    private string EmitSetlikeMember(JsonElement member, string? namespaceName, WebIdlDocumentation? documentation)
    {
        var type = ResolveInlineType(member.GetArray("idlType")[0], namespaceName, "SetlikeItem");
        var builder = new StringBuilder();
        AppendDocumentation(builder, documentation);
        builder.Append("#region Set\n")
            .Append($"extern int ICollection<{type}>.Count {{ get; }}\n")
            .Append($"extern bool ICollection<{type}>.IsReadOnly {{ get; }}\n")
            .Append($"extern bool ISet<{type}>.Add({type} item);\n")
            .Append($"extern void ICollection<{type}>.Clear();\n")
            .Append($"extern bool ICollection<{type}>.Contains({type} item);\n")
            .Append($"extern void ICollection<{type}>.CopyTo({type}[] array, int arrayIndex);\n")
            .Append($"extern void ISet<{type}>.ExceptWith(IEnumerable<{type}> other);\n")
            .Append($"extern IEnumerator<{type}> IEnumerable<{type}>.GetEnumerator();\n")
            .Append($"extern void ISet<{type}>.IntersectWith(IEnumerable<{type}> other);\n")
            .Append($"extern bool ISet<{type}>.IsProperSubsetOf(IEnumerable<{type}> other);\n")
            .Append($"extern bool ISet<{type}>.IsProperSupersetOf(IEnumerable<{type}> other);\n")
            .Append($"extern bool ISet<{type}>.IsSubsetOf(IEnumerable<{type}> other);\n")
            .Append($"extern bool ISet<{type}>.IsSupersetOf(IEnumerable<{type}> other);\n")
            .Append($"extern bool ISet<{type}>.Overlaps(IEnumerable<{type}> other);\n")
            .Append($"extern bool ICollection<{type}>.Remove({type} item);\n")
            .Append($"extern bool ISet<{type}>.SetEquals(IEnumerable<{type}> other);\n")
            .Append($"extern void ISet<{type}>.SymmetricExceptWith(IEnumerable<{type}> other);\n")
            .Append($"extern void ISet<{type}>.UnionWith(IEnumerable<{type}> other);\n")
            .Append($"extern void ICollection<{type}>.Add({type} item);\n")
            .Append("extern IEnumerator IEnumerable.GetEnumerator();\n")
            .Append("#endregion");
        return builder.ToString();
    }

    private static string BuildDeleterInlineTemplate(IReadOnlyList<MethodParameterEmission> parameters)
    {
        return parameters.Count switch
        {
            1 => "delete (__arg1)[__arg2]",
            _ => throw new InvalidOperationException($"Deleter operation must have exactly one key argument, but found {parameters.Count}."),
        };
    }

    private string GetIterableInterface(JsonElement member, string? namespaceName)
    {
        var types = member.GetArray("idlType");
        var returnType = types.Count == 1
            ? ResolveInlineType(types[0], namespaceName, "IterableItem")
            : $"({ResolveInlineType(types[0], namespaceName, "IterableKey")}, {ResolveInlineType(types[1], namespaceName, "IterableValue")})";
        return $"IEnumerable<{returnType}>";
    }

    private string GetMaplikeInterface(JsonElement member, string? namespaceName)
    {
        var keyType = ResolveInlineType(member.GetArray("idlType")[0], namespaceName, "MaplikeKey");
        var valueType = ResolveInlineType(member.GetArray("idlType")[1], namespaceName, "MaplikeValue");
        return $"IDictionary<{keyType}, {valueType}>";
    }

    private string GetSetlikeInterface(JsonElement member, string? namespaceName)
    {
        var type = ResolveInlineType(member.GetArray("idlType")[0], namespaceName, "SetlikeItem");
        return $"ISet<{type}>";
    }

    private IReadOnlyList<MethodParameterEmission> BuildMethodParameters(
        IReadOnlyList<JsonElement> arguments,
        string? namespaceName,
        string ownerBaseName,
        WebIdlMemberDocumentation? documentation = null)
    {
        var parameters = new List<MethodParameterEmission>();
        var hasOptionalParameter = false;

        for (var argumentIndex = 0; argumentIndex < arguments.Count; argumentIndex++)
        {
            var argument = arguments[argumentIndex];
            var argumentDocumentation = GetArgumentDocumentation(documentation, argumentIndex);
            var originalName = argument.GetStringOrNull("name") ?? string.Empty;
            var name = WebIdlNaming.ToCamelCase(originalName);
            var idlType = argument.GetProperty("idlType");
            var type = ResolveInlineType(
                idlType,
                namespaceName,
                CombineUnionBaseName(ownerBaseName, WebIdlNaming.ToPascalCase(originalName)));
            var typeKey = type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type;
            var isOptional = argument.GetBooleanOrNull("optional") == true;
            var isVariadic = argument.GetBooleanOrNull("variadic") == true;
            string? defaultValue = null;

            if (isVariadic)
            {
                var variadicType = type.EndsWith("[]", StringComparison.Ordinal) ? type : $"{type}[]";
                parameters.Add(new MethodParameterEmission(
                    OriginalName: originalName,
                    CommentName: name.TrimStart('@'),
                    Signature: $"params {variadicType} {name}",
                    Type: variadicType,
                    Name: name,
                    Documentation: argumentDocumentation));
                continue;
            }

            if (argument.TryGetProperty("default", out var argumentDefault) && argumentDefault.ValueKind != JsonValueKind.Null)
            {
                var value = _typeMapper.FormatValue(argumentDefault, idlType, namespaceName);
                var isEnumType = _typeMapper.IsEnumType(typeKey);
                if (isEnumType && (idlType.GetBooleanOrNull("nullable") != true || (value.Length > 0 && value is not "default" and not "null")))
                {
                    value = $"{typeKey}.{NormalizeEnumValue(value)}";
                }

                if (value == "null")
                {
                    type = MakeOptionalParameterType(type);
                    defaultValue = "default";
                }
                else if (!CanUseLiteralOptionalParameterDefault(typeKey, isEnumType))
                {
                    type = MakeOptionalParameterType(type);
                    defaultValue = "default";
                }
                else
                {
                    value = AddNumericSuffix(type, value);
                    defaultValue = value;
                }

                hasOptionalParameter = true;
            }
            else if (isOptional || hasOptionalParameter)
            {
                type = MakeOptionalParameterType(type);
                defaultValue = "default";
                hasOptionalParameter = true;
            }

            parameters.Add(new MethodParameterEmission(
                OriginalName: originalName,
                CommentName: name.TrimStart('@'),
                Signature: $"{type} {name}{(defaultValue is null ? string.Empty : $" = {defaultValue}")}",
                Type: type,
                Name: name,
                Documentation: argumentDocumentation));
        }

        return parameters;
    }

    private IReadOnlyList<string> EmitUnionTailOverloads(
        string originalOperationName,
        string methodName,
        string returnType,
        bool isStatic,
        string inheritanceModifier,
        IReadOnlyList<JsonElement> arguments,
        string? namespaceName,
        string ownerBaseName,
        WebIdlMemberDocumentation? documentation)
    {
        var lastArgument = arguments[^1];
        var lastArgumentName = WebIdlNaming.ToCamelCase(lastArgument.GetStringOrNull("name") ?? string.Empty);
        var priorArguments = arguments.Take(arguments.Count - 1).ToArray();
        var priorParameters = BuildMethodParameters(priorArguments, namespaceName, ownerBaseName, documentation);
        var overloads = new List<string>();

        foreach (var unionType in lastArgument.GetProperty("idlType").GetArray("idlType"))
        {
            var type = ResolveInlineType(
                unionType,
                namespaceName,
                CombineUnionBaseName(methodName, WebIdlNaming.ToPascalCase(lastArgumentName)));
            var typeKey = type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type;
            var parameterType = _typeMapper.IsDictionaryType(typeKey) ? $"{typeKey}?" : type;
            var signatureParts = priorParameters.Select(static parameter => parameter.Signature).ToList();
            signatureParts.Add(_typeMapper.IsDictionaryType(typeKey)
                ? $"{parameterType} {lastArgumentName} = default"
                : $"{parameterType} {lastArgumentName}");

            var builder = new StringBuilder();
            AppendDocumentation(builder, documentation?.Documentation);
            AppendParameterDocumentation(builder, priorParameters);
            var lastArgumentDocumentation = GetArgumentDocumentation(documentation, arguments.Count - 1);
            if (lastArgumentDocumentation is not null)
            {
                builder.Append("/// <param name=\"")
                    .Append(EscapeXmlDocumentation(lastArgumentName.TrimStart('@')))
                    .Append("\">");
                if (!string.IsNullOrWhiteSpace(lastArgumentDocumentation.Prose))
                {
                    builder.Append(EscapeXmlDocumentation(lastArgumentDocumentation.Prose)).Append(' ');
                }

                AppendDocumentationLink(builder, lastArgumentDocumentation);
                builder.AppendLine("</param>");
            }

            builder.AppendLine($"[Description(\"@#{originalOperationName}\")]");
            builder.Append($"public {inheritanceModifier}{(isStatic ? "static " : string.Empty)}extern {returnType} {methodName}({string.Join(", ", signatureParts)});");
            overloads.Add(builder.ToString());
        }

        return overloads;
    }

    private void BuildInterfaceCaches(WebIdlInventory inventory, IReadOnlyDictionary<string, string[]> includesByTarget)
    {
        _interfaceCachesByKey.Clear();
        _interfaceKeysByName.Clear();

        var groupedInterfaces = inventory.Files
            .SelectMany(file => file.Declarations
                .Where(static declaration => declaration.Kind == "interface")
                .Select(declaration => new { file.Namespace, Declaration = declaration }))
            .GroupBy(item => new
            {
                Namespace = item.Namespace ?? string.Empty,
                Name = item.Declaration.Name ?? string.Empty,
            })
            .ToArray();

        foreach (var group in groupedInterfaces)
        {
            var key = $"{group.Key.Namespace}|{group.Key.Name}";
            if (_interfaceKeysByName.TryGetValue(group.Key.Name, out var existingKeys))
            {
                _interfaceKeysByName[group.Key.Name] = [.. existingKeys, key];
            }
            else
            {
                _interfaceKeysByName[group.Key.Name] = [key];
            }
        }

        foreach (var group in groupedInterfaces)
        {
            var namespaceName = string.IsNullOrWhiteSpace(group.Key.Namespace) ? null : group.Key.Namespace;
            var declarations = group.Select(item => item.Declaration).ToArray();
            var effectiveMembers = GetEffectiveInterfaceMembers(declarations, namespaceName, includesByTarget);
            var parentName = declarations
                .Select(static declaration => declaration.Inheritance)
                .FirstOrDefault(static inheritance => !string.IsNullOrWhiteSpace(inheritance));
            var parentKey = ResolveInterfaceKey(namespaceName, parentName);
            var constructors = effectiveMembers
                .Where(static member => member.GetStringOrNull("type") == "constructor")
                .Select(member => BuildConstructorCache(member, group.Key.Name, namespaceName))
                .ToArray();
            var className = WebIdlNaming.ToTypeName(group.Key.Name);
            var properties = effectiveMembers
                .Where(static member => member.GetStringOrNull("type") == "attribute")
                .Select(member => BuildPropertyCache(member, group.Key.Name, className, namespaceName))
                .ToArray();
            var operations = effectiveMembers
                .Where(static member => member.GetStringOrNull("type") == "operation")
                .Select(member => BuildOperationCache(member, namespaceName))
                .ToArray();

            _interfaceCachesByKey[$"{group.Key.Namespace}|{group.Key.Name}"] = new InterfaceCache(
                Constructors: constructors,
                Properties: properties,
                Operations: operations,
                ParentKey: parentKey);
        }
    }

    private IReadOnlyList<JsonElement> GetEffectiveInterfaceMembers(
        IReadOnlyList<WebIdlDeclarationInventory> declarations,
        string? namespaceName,
        IReadOnlyDictionary<string, string[]> includesByTarget)
    {
        var members = declarations
            .SelectMany(declaration => declaration.Payload.GetArray("members"))
            .ToArray();
        var effectiveMembers = DistinctMembers(members, namespaceName).ToList();
        var seen = new HashSet<string>(effectiveMembers.Select(member => BuildMemberKey(member, namespaceName)), StringComparer.Ordinal);
        var includeKey = $"{namespaceName ?? string.Empty}|{declarations[0].Name}";
        if (!includesByTarget.TryGetValue(includeKey, out var includeMixins))
        {
            return effectiveMembers;
        }

        foreach (var mixinName in includeMixins)
        {
            var mixinKey = $"{namespaceName ?? string.Empty}|{mixinName}";
            if (!_mixinMembersByKey.TryGetValue(mixinKey, out var mixinMembers))
            {
                continue;
            }

            foreach (var mixinMember in mixinMembers)
            {
                if (seen.Add(BuildMemberKey(mixinMember, namespaceName)))
                {
                    effectiveMembers.Add(mixinMember);
                }
            }
        }

        return effectiveMembers;
    }

    private string? ResolveInterfaceKey(string? namespaceName, string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return null;
        }

        var namespacedKey = BuildTypeKey(namespaceName, typeName);
        if (_interfaceKeysByName.Values.Any(keys => keys.Contains(namespacedKey, StringComparer.Ordinal)))
        {
            return namespacedKey;
        }

        if (_interfaceKeysByName.TryGetValue(typeName, out var keys) && keys.Length == 1)
        {
            return keys[0];
        }

        var typeNameProjection = WebIdlNaming.ToTypeName(typeName);
        var pascalMatches = _interfaceKeysByName
            .Where(pair => WebIdlNaming.ToTypeName(pair.Key) == typeNameProjection)
            .SelectMany(static pair => pair.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return pascalMatches.Length == 1 ? pascalMatches[0] : null;
    }

    private InheritanceDisposition GetPropertyInheritanceDisposition(InterfaceEmissionContext context, string propertyName, string propertyType)
    {
        var hasMatchingName = false;
        foreach (var ancestor in EnumerateAncestors(context.Cache))
        {
            if (ancestor.Properties.Any(property => property.Name == propertyName && property.Type == propertyType))
            {
                return InheritanceDisposition.Skip;
            }

            if (ancestor.Properties.Any(property => property.Name == propertyName))
            {
                hasMatchingName = true;
            }
        }

        return hasMatchingName ? InheritanceDisposition.New : InheritanceDisposition.None;
    }

    private InheritanceDisposition GetOperationInheritanceDisposition(
        InterfaceEmissionContext context,
        string special,
        string methodName,
        IReadOnlyList<JsonElement> arguments,
        string returnType)
    {
        var argumentSignature = string.Join("@", arguments.Select(argument =>
            $"{argument.GetStringOrNull("name")}:{GetStructuralCacheType(argument.GetProperty("idlType"), context.NamespaceName)}"));
        var hasMatchingNameAndParameters = false;
        foreach (var ancestor in EnumerateAncestors(context.Cache))
        {
            if (ancestor.Operations.Any(operation =>
                    operation.Special == special
                    && operation.Name == methodName
                    && operation.ArgumentSignature == argumentSignature
                    && operation.ReturnType == returnType))
            {
                return InheritanceDisposition.Skip;
            }

            if (ancestor.Operations.Any(operation =>
                    operation.Special == special
                    && operation.Name == methodName
                    && operation.ArgumentSignature == argumentSignature))
            {
                hasMatchingNameAndParameters = true;
            }
        }

        return hasMatchingNameAndParameters ? InheritanceDisposition.New : InheritanceDisposition.None;
    }

    private IEnumerable<InterfaceCache> EnumerateAncestors(InterfaceCache? cache)
    {
        var parentKey = cache?.ParentKey;
        while (!string.IsNullOrWhiteSpace(parentKey) && _interfaceCachesByKey.TryGetValue(parentKey, out var parentCache))
        {
            yield return parentCache;
            parentKey = parentCache.ParentKey;
        }
    }

    private ConstructorCache BuildConstructorCache(JsonElement constructor, string ownerName, string? namespaceName)
    {
        var arguments = constructor.GetArray("arguments");
        var parameterList = string.Join(", ", arguments.Select(argument =>
        {
            var originalArgName = argument.GetStringOrNull("name") ?? string.Empty;
            var argType = ResolveInlineType(
                argument.GetProperty("idlType"),
                namespaceName,
                CombineUnionBaseName(ownerName, WebIdlNaming.ToPascalCase(originalArgName)));
            var argName = WebIdlNaming.ToCamelCase(originalArgName);
            return $"{argType} {argName}";
        }));
        var argumentList = string.Join(", ", arguments.Select(argument => WebIdlNaming.ToCamelCase(argument.GetStringOrNull("name") ?? string.Empty)));
        var typeSignature = string.Join("@", arguments.Select(argument =>
        {
            var argName = argument.GetStringOrNull("name") ?? string.Empty;
            return GetStructuralCacheType(argument.GetProperty("idlType"), namespaceName);
        }));
        return new ConstructorCache(arguments.Count, parameterList, argumentList, typeSignature);
    }

    private PropertyCache BuildPropertyCache(JsonElement attribute, string ownerName, string className, string? namespaceName)
    {
        var originalName = attribute.GetStringOrNull("name") ?? string.Empty;
        return new PropertyCache(
            GetAttributePropertyName(originalName, ownerName, className),
            GetStructuralCacheType(attribute.GetProperty("idlType"), namespaceName));
    }

    private OperationCache BuildOperationCache(JsonElement operation, string? namespaceName)
    {
        var special = operation.GetStringOrNull("special") ?? string.Empty;
        var operationName = operation.GetStringOrNull("name");
        var methodName = string.IsNullOrWhiteSpace(operationName)
            ? string.Empty
            : GetOperationMethodName(operationName);
        var argumentSignature = string.Join("@", operation.GetArray("arguments").Select(argument =>
            $"{argument.GetStringOrNull("name")}:{GetStructuralCacheType(argument.GetProperty("idlType"), namespaceName)}"));
        var returnType = operation.TryGetProperty("idlType", out var operationType)
            ? GetStructuralCacheType(operationType, namespaceName, "void")
            : "void";
        return new OperationCache(special, methodName, argumentSignature, returnType);
    }

    private string GetStructuralCacheType(JsonElement idlType, string? namespaceName, string defaultValue = "object")
        => GetStructuralCacheType(idlType, namespaceName, "StructuralCache", defaultValue);

    private string GetStructuralCacheType(JsonElement idlType, string? namespaceName, string baseName, string defaultValue = "object")
        => idlType.GetBooleanOrNull("union") == true
            ? ResolveInlineType(idlType, namespaceName, baseName, defaultValue)
            : _typeMapper.ToInlineType(idlType, namespaceName, defaultValue, CreateUnionNameContext(baseName, idlType, preferRequestedNameForFirstUnion: false));

    private static string CombineUnionBaseName(string baseName, string segment)
    {
        if (string.IsNullOrWhiteSpace(baseName))
        {
            return segment;
        }

        if (string.IsNullOrWhiteSpace(segment) || baseName.EndsWith(segment, StringComparison.Ordinal))
        {
            return baseName;
        }

        return baseName + segment;
    }

    private static string GetAttributePropertyName(string originalName, string ownerName, string? className = null)
    {
        var propertyName = WebIdlNaming.ToPascalCase(originalName);
        var resolvedClassName = className ?? WebIdlNaming.ToTypeName(ownerName);
        if (propertyName == resolvedClassName)
        {
            propertyName += "_";
        }

        if (originalName.Contains('-', StringComparison.Ordinal))
        {
            propertyName = string.Join(
                "_",
                originalName.Split('-', StringSplitOptions.RemoveEmptyEntries)
                    .Select(WebIdlNaming.ToPascalCase));
        }

        return propertyName;
    }

    private static string GetOperationMethodName(string operationName)
    {
        var methodName = WebIdlNaming.ToPascalCase(operationName);
        return methodName == "Item" ? "GetItem" : methodName;
    }

    private static string EscapeIdentifier(string value)
    {
        return value switch
        {
            "abstract" or "as" or "base" or "bool" or "break" or "byte" or "case" or "catch" or "char" or "checked" or
            "class" or "const" or "continue" or "decimal" or "default" or "delegate" or "do" or "double" or "else" or
            "enum" or "event" or "explicit" or "extern" or "false" or "finally" or "fixed" or "float" or "for" or
            "foreach" or "goto" or "if" or "implicit" or "in" or "int" or "interface" or "internal" or "is" or "lock" or
            "long" or "namespace" or "new" or "null" or "object" or "operator" or "out" or "override" or "params" or
            "private" or "protected" or "public" or "readonly" or "ref" or "return" or "sbyte" or "sealed" or "short" or
            "sizeof" or "stackalloc" or "static" or "string" or "struct" or "switch" or "this" or "throw" or "true" or
            "try" or "typeof" or "uint" or "ulong" or "unchecked" or "unsafe" or "ushort" or "using" or "virtual" or
            "void" or "volatile" or "while" => $"@{value}",
            _ => value,
        };
    }

    private static string MakeOptionalParameterType(string type)
    {
        return type.EndsWith("?", StringComparison.Ordinal) ? type : $"{type}?";
    }

    private static string BuildTypeKey(string? namespaceName, string? name)
    {
        return $"{namespaceName ?? string.Empty}|{name ?? string.Empty}";
    }

    private static WebIdlDocumentation? SelectDocumentation(IEnumerable<WebIdlDeclarationInventory> declarations)
    {
        return declarations
            .Select(static declaration => declaration.Documentation)
            .Where(static documentation => documentation is not null)
            .Cast<WebIdlDocumentation>()
            .OrderByDescending(static documentation => !string.IsNullOrWhiteSpace(documentation.Prose))
            .ThenBy(static documentation => documentation.Href, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private WebIdlMemberDocumentation? GetMemberDocumentation(WebIdlDeclarationInventory declaration, int memberIndex)
    {
        return declaration.MemberDocumentation?
            .FirstOrDefault(documentation => documentation.MemberIndex == memberIndex);
    }

    private WebIdlMemberDocumentation? GetMemberDocumentation(InterfaceEmissionContext context, JsonElement member)
    {
        return context.MemberDocumentationByKey.TryGetValue(BuildMemberKey(member, context.NamespaceName), out var documentation)
            ? documentation
            : null;
    }

    private static WebIdlDocumentation? GetArgumentDocumentation(WebIdlMemberDocumentation? documentation, int argumentIndex)
    {
        return documentation?.Arguments?
            .FirstOrDefault(argumentDocumentation => argumentDocumentation.ArgumentIndex == argumentIndex)
            ?.Documentation;
    }

    private IReadOnlyDictionary<string, WebIdlMemberDocumentation> BuildMemberDocumentationByKey(
        IEnumerable<WebIdlDeclarationInventory> declarations,
        string? namespaceName)
    {
        var result = new Dictionary<string, WebIdlMemberDocumentation>(StringComparer.Ordinal);
        foreach (var declaration in declarations)
        {
            if (declaration.MemberDocumentation is null)
            {
                continue;
            }

            var members = declaration.Payload.GetArray("members");
            foreach (var documentation in declaration.MemberDocumentation)
            {
                if (documentation.MemberIndex < 0 || documentation.MemberIndex >= members.Count)
                {
                    continue;
                }

                var key = BuildMemberKey(members[documentation.MemberIndex], namespaceName);
                if (!result.TryGetValue(key, out var existing)
                    || PreferDocumentation(documentation.Documentation, existing.Documentation))
                {
                    result[key] = documentation;
                }
            }
        }

        return result;
    }

    private static bool PreferDocumentation(WebIdlDocumentation? candidate, WebIdlDocumentation? existing)
    {
        if (candidate is null)
        {
            return false;
        }

        if (existing is null)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(candidate.Prose)
            && string.IsNullOrWhiteSpace(existing.Prose);
    }

    private IReadOnlyList<JsonElement> DistinctMembers(IReadOnlyList<JsonElement> members, string? namespaceName)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var distinct = new List<JsonElement>();
        foreach (var member in members)
        {
            var key = BuildMemberKey(member, namespaceName);
            if (seen.Add(key))
            {
                distinct.Add(member);
            }
        }

        return distinct;
    }

    private string BuildMemberKey(JsonElement member, string? namespaceName)
    {
        var type = member.GetStringOrNull("type") ?? string.Empty;
        return type switch
        {
            "constructor" => $"{type}${string.Join("@", member.GetArray("arguments").Select(argument => $"{argument.GetStringOrNull("name")}:{GetStructuralCacheType(argument.GetProperty("idlType"), namespaceName)}"))}",
            "operation" => $"{type}${member.GetStringOrNull("name")}${string.Join("@", member.GetArray("arguments").Select(argument => $"{argument.GetStringOrNull("name")}:{GetStructuralCacheType(argument.GetProperty("idlType"), namespaceName)}"))}{(member.TryGetProperty("idlType", out var operationType) ? GetStructuralCacheType(operationType, namespaceName, "void") : "void")}${member.GetStringOrNull("special")}",
            "attribute" => $"{type}${member.GetStringOrNull("name")}${GetStructuralCacheType(member.GetProperty("idlType"), namespaceName)}",
            "const" => $"{type}${member.GetStringOrNull("name")}${GetStructuralCacheType(member.GetProperty("idlType"), namespaceName)}",
            "field" => $"{type}${member.GetStringOrNull("name")}${GetStructuralCacheType(member.GetProperty("idlType"), namespaceName)}",
            "iterable" or "maplike" or "setlike" => $"{type}${string.Join(":", member.GetArray("idlType").Select(idlType => GetStructuralCacheType(idlType, namespaceName)))}",
            _ => type,
        };
    }

    private string ResolveAliasTargetType(
        JsonElement idlType,
        string? namespaceName,
        string unionBaseName,
        string defaultValue = "object",
        bool preferRequestedNameForFirstUnion = false)
        => _typeMapper.ToAliasTargetType(idlType, namespaceName, defaultValue, CreateUnionNameContext(unionBaseName, idlType, preferRequestedNameForFirstUnion));

    private string ResolveInlineType(
        JsonElement idlType,
        string? namespaceName,
        string unionBaseName,
        string defaultValue = "object",
        bool preferRequestedNameForFirstUnion = false)
        => _typeMapper.ToInlineType(idlType, namespaceName, defaultValue, CreateUnionNameContext(unionBaseName, idlType, preferRequestedNameForFirstUnion));

    private string ResolveNamedUnionType(NamedUnionRequest request)
    {
        var analysis = AnalyzeUnion(request);

        var identity = BuildUnionIdentity(request.NamespaceName, request.Name, request.IdlType);
        if (!_resolvedUnionTypeNameByIdentity.TryGetValue(identity, out var resolvedName))
        {
            var shapeKey = BuildUnionShapeKey(request.NamespaceName, request.IdlType);
            resolvedName = TryGetRelatedResolvedUnionTypeName(shapeKey, request.Name, out var existingName)
                ? existingName
                : GetAvailableUnionTypeName(request.NamespaceName, request.Name, analysis.Branches, request.PreferRequestedName);
            _resolvedUnionTypeNameByIdentity[identity] = resolvedName;

            if (!_resolvedUnionTypeNamesByShape.TryGetValue(shapeKey, out var names))
            {
                names = [];
                _resolvedUnionTypeNamesByShape[shapeKey] = names;
            }

            if (!names.Contains(resolvedName, StringComparer.Ordinal))
            {
                names.Add(resolvedName);
            }
        }

        var qualifiedTypeName = GetQualifiedTypeName(request.NamespaceName, resolvedName);
        if (!_generatedUnionDefinitionsByQualifiedName.ContainsKey(qualifiedTypeName))
        {
            _generatedUnionDefinitionsByQualifiedName[qualifiedTypeName] = new GeneratedUnionDefinition(
                resolvedName,
                request.NamespaceName,
                qualifiedTypeName,
                analysis.Branches,
                analysis.CollectionBranch,
                analysis.CollectionElementType);
            RegisterOccupiedTypeName(request.NamespaceName, resolvedName);
        }

        return request.QualifyForAlias ? qualifiedTypeName : resolvedName;
    }

    private UnionAnalysis AnalyzeUnion(NamedUnionRequest request)
    {
        var rawBranchParts = request.IdlType.GetArray("idlType");
        var nestedContext = new UnionNameContext(request.Name, useBaseNameForFirstUnion: false, preferRequestedNameForFirstUnion: false);
        var branches = rawBranchParts
            .Select(part =>
            {
                var mappedType = _typeMapper.ToInlineType(part, request.NamespaceName, "object", nestedContext);
                return new { Part = part, MappedType = mappedType };
            })
            .Where(item => !WebIdlTypeMapper.IsVoidLikeType(item.Part, item.MappedType))
            .GroupBy(item => item.MappedType, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();

        var branchDefinitions = branches
            .Select((item, index) => new GeneratedUnionBranch(
                item.MappedType,
                $"As{BuildTypeMemberSuffix(item.MappedType)}",
                index + 1,
                SupportsImplicitConversion(item.MappedType)))
            .ToArray();

        var collectionBranch = branchDefinitions.FirstOrDefault(static branch => branch.Type.EndsWith("[]", StringComparison.Ordinal));
        var collectionElementType = collectionBranch is null
            ? null
            : collectionBranch.Type[..^2];
        return new UnionAnalysis(branchDefinitions, collectionBranch, collectionElementType);
    }

    private string EmitUnion(GeneratedUnionDefinition union)
    {
        var supportsNativeUnionSyntax = SupportsNativeUnionSyntax(union.NamespaceName, union.Branches);
        var builder = new StringBuilder();
        builder.AppendLine("[ECMAScript]");
        if (union.SupportsSystemUnionContract)
        {
            builder.AppendLine("[System.Runtime.CompilerServices.Union]");
        }

        builder.AppendLine("[Description(\"@#\")]");
        if (union.CollectionElementType is not null)
        {
            builder.AppendLine($"[System.Runtime.CompilerServices.CollectionBuilder(typeof({union.Name}CollectionBuilder), nameof({union.Name}CollectionBuilder.Create))]");
        }

        var interfaces = new List<string>();
        if (union.SupportsSystemUnionContract)
        {
            interfaces.Add("System.Runtime.CompilerServices.IUnion");
        }

        if (union.CollectionElementType is not null)
        {
            interfaces.Add($"IEnumerable<{union.CollectionElementType}>");
        }

        if (union.SupportsSystemUnionContract && supportsNativeUnionSyntax)
        {
            builder.Append($"public readonly union {union.Name}(");
            builder.Append(string.Join(", ", union.Branches.Select(static branch => branch.Type)));
            builder.Append(')');
            if (union.CollectionElementType is not null)
            {
                builder.Append($" : IEnumerable<{union.CollectionElementType}>");
            }

            builder.AppendLine();
            builder.AppendLine("{");

            foreach (var branch in union.Branches)
            {
                builder.AppendLine();
                builder.AppendLine($"    public {ToOptionalType(branch.Type)} {branch.AccessorName} => Value {BuildNativeUnionAccessorExpression(branch.Type)};");
            }

            foreach (var branch in union.Branches)
            {
                builder.AppendLine();
                builder.AppendLine($"    public static implicit operator {union.Name}({branch.Type} value)");
                builder.AppendLine("        => new(value);");
            }

            AppendUnionCollectionMembers(builder, union);
            builder.Append('}');
            AppendUnionCollectionBuilder(builder, union);
            return builder.ToString();
        }

        builder.AppendLine(interfaces.Count == 0
            ? $"public readonly struct {union.Name}"
            : $"public readonly struct {union.Name} : {string.Join(", ", interfaces)}");
        builder.AppendLine("{");
        builder.AppendLine("    private readonly byte _kind;");
        foreach (var branch in union.Branches)
        {
            builder.AppendLine($"    private readonly {ToOptionalType(branch.Type)} _value{branch.Kind};");
        }

        foreach (var branch in union.Branches)
        {
            builder.AppendLine();
            builder.AppendLine($"    {GetUnionConstructorAccessibility(union)} {union.Name}({branch.Type} value)");
            builder.AppendLine("    {");
            builder.AppendLine($"        _kind = {branch.Kind};");
            foreach (var fieldBranch in union.Branches)
            {
                builder.AppendLine($"        _value{fieldBranch.Kind} = {(fieldBranch.Kind == branch.Kind ? "value" : "default")};");
            }

            builder.AppendLine("    }");
        }

        foreach (var branch in union.Branches)
        {
            builder.AppendLine();
            builder.AppendLine($"    public {ToOptionalType(branch.Type)} {branch.AccessorName} => _kind == {branch.Kind} ? _value{branch.Kind} : default;");
        }

        if (union.SupportsSystemUnionContract)
        {
            builder.AppendLine();
            builder.AppendLine("    public object? Value => _kind switch");
            builder.AppendLine("    {");
            foreach (var branch in union.Branches)
            {
                builder.AppendLine($"        {branch.Kind} => _value{branch.Kind},");
            }

            builder.AppendLine("        _ => default");
            builder.AppendLine("    };");
        }

        foreach (var branch in union.Branches)
        {
            builder.AppendLine();
            if (branch.SupportsImplicitConversion)
            {
                builder.AppendLine($"    public static implicit operator {union.Name}({branch.Type} value)");
                builder.AppendLine("        => new(value);");
            }
            else
            {
                builder.AppendLine($"    public static {union.Name} From{branch.AccessorName[2..]}({branch.Type} value)");
                builder.AppendLine("        => new(value);");

                foreach (var concreteType in GetForwardingImplicitConversionTypes(branch.Type))
                {
                    builder.AppendLine();
                    builder.AppendLine($"    public static implicit operator {union.Name}({concreteType} value)");
                    builder.AppendLine("        => new(value);");
                }
            }
        }

        AppendUnionCollectionMembers(builder, union);
        builder.Append('}');
        AppendUnionCollectionBuilder(builder, union);

        return builder.ToString();
    }

    private static void AppendUnionCollectionMembers(StringBuilder builder, GeneratedUnionDefinition union)
    {
        if (union.CollectionElementType is not null && union.CollectionBranch is not null)
        {
            builder.AppendLine();
            builder.AppendLine($"    IEnumerator<{union.CollectionElementType}> IEnumerable<{union.CollectionElementType}>.GetEnumerator()");
            builder.AppendLine($"        => ((IEnumerable<{union.CollectionElementType}>)({union.CollectionBranch.AccessorName} ?? Array.Empty<{union.CollectionElementType}>())).GetEnumerator();");
            builder.AppendLine();
            builder.AppendLine("    IEnumerator IEnumerable.GetEnumerator()");
            builder.AppendLine($"        => ((IEnumerable<{union.CollectionElementType}>)this).GetEnumerator();");
        }
    }

    private static void AppendUnionCollectionBuilder(StringBuilder builder, GeneratedUnionDefinition union)
    {
        if (union.CollectionElementType is not null)
        {
            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine("[EditorBrowsable(EditorBrowsableState.Never)]");
            builder.AppendLine($"public static class {union.Name}CollectionBuilder");
            builder.AppendLine("{");
            builder.AppendLine($"    public static {union.Name} Create(ReadOnlySpan<{union.CollectionElementType}> items)");
            builder.AppendLine("        => items.ToArray();");
            builder.Append('}');
        }
    }

    private static string BuildNativeUnionAccessorExpression(string type)
        => $"is {type} value ? value : default({ToOptionalType(type)})";

    private static UnionNameContext CreateUnionNameContext(string baseName, JsonElement idlType, bool preferRequestedNameForFirstUnion)
        => new(
            WebIdlNaming.ToPascalCase(string.IsNullOrWhiteSpace(baseName) ? "Union" : baseName),
            idlType.GetBooleanOrNull("union") == true,
            preferRequestedNameForFirstUnion);

    private static string BuildTypeMemberSuffix(string typeName)
    {
        // The qualification prevents the WebCrypto typedef named BigInteger from capturing
        // the primitive mapping, but it is an implementation detail in a public union projection.
        var displayTypeName = typeName.Replace("System.Numerics.", string.Empty, StringComparison.Ordinal);
        var builder = new StringBuilder();
        for (var index = 0; index < displayTypeName.Length; index++)
        {
            if (index + 1 < displayTypeName.Length && displayTypeName[index] == '[' && displayTypeName[index + 1] == ']')
            {
                builder.Append("Array");
                index++;
                continue;
            }

            if (!char.IsLetterOrDigit(displayTypeName[index]))
            {
                continue;
            }

            var start = index;
            while (index < displayTypeName.Length && char.IsLetterOrDigit(displayTypeName[index]))
            {
                index++;
            }

            var segment = displayTypeName[start..index];
            // `JazorFile` is the C# authoring projection of the browser's singular
            // File type. Keep generated union accessors as AsFile so the member
            // still describes the runtime value rather than the C# collision fix.
            // `JazorFile` 是浏览器单数 File 类型的 C# 作者侧投影。联合类型访问器仍应
            // 使用 AsFile，描述运行时值本身，而不能泄露为规避命名冲突而做的 C# 改名。
            builder.Append(segment == "JazorFile" ? "File" : WebIdlNaming.ToPascalCase(segment));
            index--;
        }

        return builder.Length == 0 ? "Value" : builder.ToString();
    }

    private static string ToOptionalType(string type)
        => type.EndsWith("?", StringComparison.Ordinal) ? type : $"{type}?";

    private static bool SupportsSystemUnionContract(IReadOnlyList<GeneratedUnionBranch> branches)
        => branches.All(static branch => branch.SupportsImplicitConversion);

    private bool SupportsNativeUnionSyntax(string? namespaceName, IReadOnlyList<GeneratedUnionBranch> branches)
        => branches.All(static branch => !branch.Type.Contains('?', StringComparison.Ordinal))
           && !HasAssignableGeneratedInterfaceBranches(namespaceName, branches);

    private bool HasAssignableGeneratedInterfaceBranches(string? namespaceName, IReadOnlyList<GeneratedUnionBranch> branches)
    {
        for (var leftIndex = 0; leftIndex < branches.Count; leftIndex++)
        {
            for (var rightIndex = leftIndex + 1; rightIndex < branches.Count; rightIndex++)
            {
                if (IsGeneratedInterfaceAssignable(namespaceName, branches[leftIndex].Type, branches[rightIndex].Type)
                    || IsGeneratedInterfaceAssignable(namespaceName, branches[rightIndex].Type, branches[leftIndex].Type))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsGeneratedInterfaceAssignable(string? namespaceName, string sourceType, string targetType)
    {
        var sourceIsArray = sourceType.EndsWith("[]", StringComparison.Ordinal);
        var targetIsArray = targetType.EndsWith("[]", StringComparison.Ordinal);
        if (sourceIsArray || targetIsArray)
        {
            // An element is never assignable to its array; compare covariance only for two array branches.
            return sourceIsArray
                && targetIsArray
                && IsGeneratedInterfaceAssignable(
                    namespaceName,
                    sourceType[..^2],
                    targetType[..^2]);
        }

        var sourceKey = ResolveInterfaceKey(namespaceName, sourceType);
        var targetKey = ResolveInterfaceKey(namespaceName, targetType);
        if (sourceKey is null || targetKey is null)
        {
            return false;
        }

        while (true)
        {
            if (string.Equals(sourceKey, targetKey, StringComparison.Ordinal))
            {
                return true;
            }

            if (!_interfaceCachesByKey.TryGetValue(sourceKey, out var sourceCache)
                || string.IsNullOrWhiteSpace(sourceCache.ParentKey))
            {
                return false;
            }

            sourceKey = sourceCache.ParentKey;
        }
    }

    private static string GetUnionConstructorAccessibility(GeneratedUnionDefinition union)
        => union.SupportsSystemUnionContract ? "public" : "private";

    private static bool SupportsImplicitConversion(string type)
    {
        if (string.Equals(type, "object", StringComparison.Ordinal))
        {
            return false;
        }

        var simpleTypeName = GetSimpleTypeName(type);
        return !(simpleTypeName.Length > 1
                 && simpleTypeName[0] == 'I'
                 && char.IsUpper(simpleTypeName[1]));
    }

    private static IReadOnlyList<string> GetForwardingImplicitConversionTypes(string type)
    {
        return type switch
        {
            "IBufferSource" =>
            [
                "ArrayBuffer",
                "DataView",
                "Uint8Array",
                "Uint8ClampedArray",
                "Int8Array",
                "Int16Array",
                "Uint16Array",
                "Int32Array",
                "Uint32Array",
                "Float16Array",
                "Float32Array",
                "Float64Array",
                "BigInt64Array",
                "BigUint64Array",
            ],
            "IArrayBufferView" =>
            [
                "DataView",
                "Uint8Array",
                "Uint8ClampedArray",
                "Int8Array",
                "Int16Array",
                "Uint16Array",
                "Int32Array",
                "Uint32Array",
                "Float16Array",
                "Float32Array",
                "Float64Array",
                "BigInt64Array",
                "BigUint64Array",
            ],
            "IAllowSharedBufferSource" =>
            [
                "ArrayBuffer",
                "SharedArrayBuffer",
                "DataView",
                "Uint8Array",
                "Uint8ClampedArray",
                "Int8Array",
                "Int16Array",
                "Uint16Array",
                "Int32Array",
                "Uint32Array",
                "Float16Array",
                "Float32Array",
                "Float64Array",
                "BigInt64Array",
                "BigUint64Array",
            ],
            _ => Array.Empty<string>(),
        };
    }

    private static string BuildUnionIdentity(string? namespaceName, string requestedName, JsonElement idlType)
        => $"{namespaceName ?? string.Empty}|{requestedName}|{GetNormalizedJsonKey(idlType)}";

    private static string BuildUnionShapeKey(string? namespaceName, JsonElement idlType)
        => $"{namespaceName ?? string.Empty}|{GetNormalizedJsonKey(idlType)}";

    private static string GetNormalizedJsonKey(JsonElement idlType)
        => JsonSerializer.Serialize(idlType);

    private bool TryGetRelatedResolvedUnionTypeName(string shapeKey, string requestedName, out string resolvedName)
    {
        if (_resolvedUnionTypeNamesByShape.TryGetValue(shapeKey, out var names))
        {
            foreach (var name in names.OrderBy(static item => item.Length))
            {
                if (AreRelatedUnionNames(name, requestedName))
                {
                    resolvedName = name;
                    return true;
                }
            }
        }

        resolvedName = string.Empty;
        return false;
    }

    private static bool AreRelatedUnionNames(string existingName, string requestedName)
    {
        if (string.Equals(existingName, requestedName, StringComparison.Ordinal))
        {
            return true;
        }

        const string suffix = "Value";
        if (requestedName.EndsWith(suffix, StringComparison.Ordinal))
        {
            return string.Equals(existingName, requestedName[..^suffix.Length], StringComparison.Ordinal);
        }

        return false;
    }

    private void RegisterOccupiedDeclarationName(string? namespaceName, WebIdlDeclarationInventory declaration)
    {
        if (!string.IsNullOrWhiteSpace(declaration.Name))
        {
            RegisterOccupiedTypeName(namespaceName, WebIdlNaming.ToTypeName(declaration.Name));
        }
    }

    private void RegisterOccupiedTypeName(string? namespaceName, string typeName)
    {
        var key = namespaceName ?? string.Empty;
        if (!_occupiedTypeNamesByNamespace.TryGetValue(key, out var set))
        {
            set = new HashSet<string>(StringComparer.Ordinal);
            _occupiedTypeNamesByNamespace[key] = set;
        }

        set.Add(typeName);
    }

    private string GetAvailableUnionTypeName(string? namespaceName, string requestedName, IReadOnlyList<GeneratedUnionBranch> branches, bool preferRequestedName)
    {
        var key = namespaceName ?? string.Empty;
        _occupiedTypeNamesByNamespace.TryGetValue(key, out var occupiedNames);
        occupiedNames ??= [];

        var candidate = requestedName;
        var suffix = 0;
        while (((!preferRequestedName) || suffix > 0) && occupiedNames.Contains(candidate)
               || branches.Any(branch => string.Equals(GetSimpleTypeName(branch.Type), candidate, StringComparison.Ordinal)))
        {
            suffix++;
            candidate = suffix == 1 ? $"{requestedName}Value" : $"{requestedName}Value{suffix}";
        }

        return candidate;
    }

    private static string GetSimpleTypeName(string typeName)
    {
        var span = typeName.AsSpan().TrimEnd('?');
        var genericIndex = span.IndexOf('<');
        if (genericIndex >= 0)
        {
            span = span[..genericIndex];
        }

        var arrayIndex = span.IndexOf('[');
        if (arrayIndex >= 0)
        {
            span = span[..arrayIndex];
        }

        var lastDotIndex = span.LastIndexOf('.');
        if (lastDotIndex >= 0)
        {
            span = span[(lastDotIndex + 1)..];
        }

        return span.ToString();
    }

    private static AccessorInfo BuildAccessorInfo(IReadOnlyList<JsonElement> members)
    {
        return new AccessorInfo(
            HasGetter: members.Any(static member => member.GetStringOrNull("type") == "operation" && member.GetStringOrNull("special") == "getter"),
            HasSetter: members.Any(static member => member.GetStringOrNull("type") == "operation" && member.GetStringOrNull("special") == "setter"));
    }

    private sealed record DictionaryParameterEmission(
        string PascalName,
        string Code,
        string ArgumentCode,
        WebIdlDocumentation? Documentation);

    private sealed record MethodParameterEmission(
        string OriginalName,
        string CommentName,
        string Signature,
        string Type,
        string Name,
        WebIdlDocumentation? Documentation);

    private sealed record AccessorInfo(
        bool HasGetter,
        bool HasSetter);

    private sealed record InterfaceEmissionContext(
        string OwnerName,
        string? NamespaceName,
        string InterfaceKey,
        InterfaceCache? Cache,
        bool ForceStatic,
        bool EnableInheritance,
        IReadOnlyDictionary<string, WebIdlMemberDocumentation> MemberDocumentationByKey);

    private sealed record InterfaceCache(
        IReadOnlyList<ConstructorCache> Constructors,
        IReadOnlyList<PropertyCache> Properties,
        IReadOnlyList<OperationCache> Operations,
        string? ParentKey);

    private sealed record ConstructorCache(
        int ParameterCount,
        string ParameterList,
        string ArgumentList,
        string TypeSignature);

    private sealed record PropertyCache(
        string Name,
        string Type);

    private sealed record OperationCache(
        string Special,
        string Name,
        string ArgumentSignature,
        string ReturnType);

    private sealed record GeneratedUnionDefinition(
        string Name,
        string? NamespaceName,
        string QualifiedTypeName,
        IReadOnlyList<GeneratedUnionBranch> Branches,
        GeneratedUnionBranch? CollectionBranch,
        string? CollectionElementType)
    {
        public bool SupportsSystemUnionContract { get; } = PreviewBindingEmitter.SupportsSystemUnionContract(Branches);
    }

    private sealed record GeneratedUnionBranch(
        string Type,
        string AccessorName,
        int Kind,
        bool SupportsImplicitConversion);

    private sealed record UnionAnalysis(
        IReadOnlyList<GeneratedUnionBranch> Branches,
        GeneratedUnionBranch? CollectionBranch,
        string? CollectionElementType);

    private static JsonElement CreateUnionJson(params JsonElement[] branches)
    {
        using var document = JsonDocument.Parse($$"""
            {
              "union": true,
              "idlType": [
                {{string.Join("," + Environment.NewLine, branches.Select(static branch => branch.GetRawText()))}}
              ]
            }
            """);
        return document.RootElement.Clone();
    }

    private static JsonElement CreateNamedTypeJson(string typeName)
    {
        using var document = JsonDocument.Parse($$"""
            {
              "idlType": "{{typeName}}"
            }
            """);
        return document.RootElement.Clone();
    }

    private enum InheritanceDisposition
    {
        None,
        New,
        Skip,
    }
}
