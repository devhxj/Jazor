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
        "global using Jazor.Common;",
        "global using ECMAScript;",
        "global using ECMAScript.CSS;",
        "global using ECMAScript.GPUBufferUsage;",
        "global using ECMAScript.WebAssembly;"
    ];

    private static readonly HashSet<string> ExcludedDeclarationNames = new(StringComparer.Ordinal)
    {
        "Console",
    };

    private readonly GeneratorOptions _options;
    private readonly WebIdlTypeMapper _typeMapper = new();
    private readonly Dictionary<string, IReadOnlyList<JsonElement>> _mixinMembersByKey = new(StringComparer.Ordinal);
    private readonly Dictionary<string, InterfaceCache> _interfaceCachesByKey = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string[]> _interfaceKeysByName = new(StringComparer.Ordinal);

    public PreviewBindingEmitter(GeneratorOptions options)
    {
        _options = options;
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

        foreach (var declaration in inventory.Files.SelectMany(file => file.Declarations))
        {
            if (declaration.Kind == "enum" && declaration.Name is not null)
            {
                _typeMapper.RegisterEnum(WebIdlNaming.ToPascalCase(declaration.Name));
            }
            else if (declaration.Kind == "dictionary" && declaration.Name is not null)
            {
                _typeMapper.RegisterDictionary(WebIdlNaming.ToPascalCase(declaration.Name));
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
                        globalUsings.Add(EmitTypedef(declaration, file.Namespace));
                        break;
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
            _mixinMembersByKey[$"{mixinGroup.Key.Namespace}|{mixinGroup.Key.Name}"] = DistinctMembers(members, namespaceName);
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

        await File.WriteAllTextAsync(
            Path.Combine(previewRoot, "GlobalUsings.cs"),
            NormalizeLineEndings(string.Join(Environment.NewLine, globalUsings.Distinct(StringComparer.Ordinal)) + Environment.NewLine),
            cancellationToken);

        await WriteGroupedFilesAsync(previewRoot, "Enums.cs", enumsByNamespace, cancellationToken);
        await WriteGroupedFilesAsync(previewRoot, "Callbacks.cs", callbacksByNamespace, cancellationToken);
        await WriteGroupedFilesAsync(previewRoot, "Dictionaries.cs", dictionariesByNamespace, cancellationToken);
        await WriteGroupedFilesAsync(previewRoot, "Interfaces.cs", interfacesByNamespace, cancellationToken);
        await WriteGroupedFilesAsync(previewRoot, "Namespaces.cs", namespacesByNamespace, cancellationToken);
    }

    private string EmitTypedef(WebIdlDeclarationInventory declaration, string? namespaceName)
    {
        var name = WebIdlNaming.ToPascalCase(declaration.Name ?? throw new InvalidOperationException("Typedef name is required."));
        var aliasTarget = _typeMapper.ToAliasTargetType(declaration.Payload.GetProperty("idlType"), namespaceName);
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
        builder.AppendLine("/// <summary>");
        builder.AppendLine($"/// {enumName}");
        builder.AppendLine("/// </summary>");
        builder.AppendLine($"[Description(\"@#{enumName}\")]");
        builder.AppendLine("[ECMAScript]");
        builder.AppendLine($"public enum {WebIdlNaming.ToPascalCase(enumName)}");
        builder.AppendLine("{");

        for (var index = 0; index < enumValues.Count; index++)
        {
            var value = enumValues[index].GetStringOrNull("value") ?? string.Empty;
            value = value.Replace("\"", string.Empty, StringComparison.Ordinal);
            var enumValueName = WebIdlNaming.ToPascalCase(string.IsNullOrEmpty(value) ? "Empty" : value);
            builder.AppendLine($"    [Description(\"@#{enumValueName}\")]");
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
        var callbackName = WebIdlNaming.ToPascalCase(declaration.Name ?? throw new InvalidOperationException("Callback name is required."));
        var returnType = _typeMapper.ToInlineType(payload.GetProperty("idlType"), namespaceName, "void");
        var parameters = BuildParameterList(payload.GetArray("arguments"), namespaceName);

        var builder = new StringBuilder();
        builder.AppendLine("/// <summary>");
        builder.AppendLine($"/// {declaration.Name}");
        builder.AppendLine("/// </summary>");
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
        var callbackInterfaceName = WebIdlNaming.ToPascalCase(declaration.Name ?? throw new InvalidOperationException("Callback interface name is required."));
        var members = payload.GetArray("members");
        var operations = members.Where(static member => member.GetStringOrNull("type") == "operation").ToArray();
        if (operations.Length != 1)
        {
            throw new InvalidOperationException($"Callback interface '{declaration.Name}' must contain exactly one operation.");
        }

        var operationName = WebIdlNaming.ToPascalCase(operations[0].GetStringOrNull("name") ?? throw new InvalidOperationException("Operation name is required."));
        globalUsings.Add(
            $"global using {callbackInterfaceName} = ECMAScript.Either<{GetQualifiedTypeName(namespaceName, callbackInterfaceName)}Literal, {GetQualifiedTypeName(namespaceName, operationName)}Callback>;");

        var emitted = new List<string>
        {
            EmitCallbackInterfaceDelegate(declaration.Name!, operations[0], namespaceName)
        };

        var partialKeyword = declaration.Partial == true ? " partial" : string.Empty;
        var builder = new StringBuilder();
        builder.AppendLine("/// <summary>");
        builder.AppendLine($"/// {declaration.Name}");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("[ECMAScript]");
        builder.AppendLine("[Description(\"@#\")]");
        builder.AppendLine("[Category(\"literal\")]");
        builder.AppendLine($"public{partialKeyword} sealed class {callbackInterfaceName}Literal");
        builder.AppendLine("{");

        var bodyMembers = new List<string>();
        foreach (var member in members)
        {
            var type = member.GetStringOrNull("type");
            if (type == "operation")
            {
                var memberName = member.GetStringOrNull("name") ?? throw new InvalidOperationException("Operation name is required.");
                var callbackName = WebIdlNaming.ToPascalCase(memberName);
                bodyMembers.Add(
                    "    /// <summary>\n"
                    + $"    /// {memberName}\n"
                    + "    /// </summary>\n"
                    + "    [ECMAScript]\n"
                    + $"    [Description(\"@#{memberName}\")]\n"
                    + $"    public {callbackName}Callback? {callbackName} {{ get; set; }}");
            }
            else if (type == "const")
            {
                bodyMembers.Add(Indent(EmitConst(member, namespaceName), 1));
            }
        }

        builder.Append(string.Join(Environment.NewLine + Environment.NewLine, bodyMembers));
        builder.AppendLine();
        builder.Append('}');

        emitted.Add(builder.ToString());
        return emitted;
    }

    private string EmitCallbackInterfaceDelegate(string interfaceName, JsonElement operation, string? namespaceName)
    {
        var callbackName = WebIdlNaming.ToPascalCase(operation.GetStringOrNull("name") ?? throw new InvalidOperationException("Operation name is required."));
        var returnType = _typeMapper.ToInlineType(operation.GetProperty("idlType"), namespaceName, "void");
        var parameters = BuildParameterList(operation.GetArray("arguments"), namespaceName);

        var builder = new StringBuilder();
        builder.AppendLine("/// <summary>");
        builder.AppendLine($"/// {interfaceName}");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("[ECMAScript]");
        builder.AppendLine("[Description(\"@#\")]");
        builder.AppendLine("[Category(\"literal\")]");
        builder.Append($"public delegate {returnType} {callbackName}Callback({parameters});");
        return builder.ToString();
    }

    private string EmitConst(JsonElement member, string? namespaceName)
    {
        var type = _typeMapper.ToInlineType(member.GetProperty("idlType"), namespaceName);
        var propertyName = WebIdlNaming.ToPascalCase(member.GetStringOrNull("name") ?? throw new InvalidOperationException("Const name is required."));
        var value = _typeMapper.FormatValue(member.GetProperty("value"), member.GetProperty("idlType"), namespaceName);

        return "/// <summary>\n"
            + $"/// {member.GetStringOrNull("name")}\n"
            + "/// </summary>\n"
            + $"[Description(\"@#{member.GetStringOrNull("name")}\")]\n"
            + $"public const {type} {propertyName} = {value};";
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
        return string.Join(Environment.NewLine, normalized.Split('\n').Select(line => prefix + line));
    }

    private string BuildParameterList(IReadOnlyList<JsonElement> arguments, string? namespaceName)
    {
        return string.Join(", ", arguments.Select(argument =>
        {
            var type = _typeMapper.ToInlineType(argument.GetProperty("idlType"), namespaceName);
            var name = WebIdlNaming.ToCamelCase(argument.GetStringOrNull("name") ?? string.Empty);
            return $"{type} {name}";
        }));
    }

    private string EmitDictionary(IReadOnlyList<WebIdlDeclarationInventory> declarations, string? namespaceName)
    {
        var name = declarations[0].Name ?? throw new InvalidOperationException("Dictionary name is required.");
        var recordName = WebIdlNaming.ToPascalCase(name);
        var inheritances = declarations
            .Select(static declaration => declaration.Inheritance)
            .Where(static inheritance => !string.IsNullOrWhiteSpace(inheritance))
            .Select(static inheritance => WebIdlNaming.ToPascalCase(inheritance!))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var inheritanceClause = inheritances.Length > 0 ? $" : {string.Join(", ", inheritances)}" : string.Empty;

        var parameterGroups = declarations
            .Select(declaration => declaration.Payload.GetArray("members")
                .Select(member => EmitDictionaryParameter(member, namespaceName))
                .ToArray())
            .ToArray();

        if (parameterGroups.Length == 1 && parameterGroups[0].Length == 0)
        {
            return "/// <summary>\n"
                + $"/// {name}\n"
                + "/// </summary>\n"
                + "[ECMAScript]\n"
                + $"[Description(\"@#{name}\")]\n"
                + $"public abstract record {recordName}();";
        }

        var builder = new StringBuilder();
        builder.AppendLine("/// <summary>");
        builder.AppendLine($"/// {name}");
        builder.AppendLine("/// </summary>");
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

    private DictionaryParameterEmission EmitDictionaryParameter(JsonElement member, string? namespaceName)
    {
        var memberName = member.GetStringOrNull("name") ?? throw new InvalidOperationException("Dictionary member name is required.");
        var pascalName = WebIdlNaming.ToPascalCase(memberName);
        var type = _typeMapper.ToInlineType(member.GetProperty("idlType"), namespaceName);
        var typeKey = type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type;
        var hasDefault = member.TryGetProperty("default", out var defaultValue) && defaultValue.ValueKind != JsonValueKind.Null;
        var isEnumType = _typeMapper.IsEnumType(typeKey);
        var isNullable = member.GetProperty("idlType").GetBooleanOrNull("nullable") == true;
        var isRequired = member.GetBooleanOrNull("required") == true;

        if (hasDefault)
        {
            var value = _typeMapper.FormatValue(defaultValue, member.GetProperty("idlType"), namespaceName);
            if (isEnumType && (!isNullable || (value.Length > 0 && value is not "default" and not "null")))
            {
                value = $"{typeKey}.{NormalizeEnumValue(value)}";
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
                $"[Description(\"@#{memberName}\")]{type} {WebIdlNaming.ToCamelCase(memberName)} = {value}");
        }

        var propertyType = _typeMapper.IsOptionalPrimitive(typeKey) ? typeKey : $"{typeKey}?";
        return new DictionaryParameterEmission(
            pascalName,
            $"[property: Description(\"@#{memberName}\")]{propertyType} {pascalName} = default",
            $"[Description(\"@#{memberName}\")]{propertyType} {pascalName} = default");
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
        var className = WebIdlNaming.ToPascalCase(originalName);
        var inheritances = declarations
            .Select(static declaration => declaration.Inheritance)
            .Where(static inheritance => !string.IsNullOrWhiteSpace(inheritance))
            .Select(static inheritance => WebIdlNaming.ToPascalCase(inheritance!))
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
                    .Select(member => BuildConstructorCache(member, namespaceName))
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
            EnableInheritance: true);
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
        builder.AppendLine("/// <summary>");
        builder.AppendLine($"/// {originalName}");
        builder.AppendLine("/// </summary>");
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
        var className = WebIdlNaming.ToPascalCase(originalName);
        var members = declarations
            .SelectMany(declaration => declaration.Payload.GetArray("members"))
            .ToArray();
        var distinctMembers = DistinctMembers(members, namespaceName);
        var accessorInfo = BuildAccessorInfo(distinctMembers);
        var emissionContext = new InterfaceEmissionContext(
            OwnerName: originalName,
            NamespaceName: namespaceName,
            InterfaceKey: BuildTypeKey(namespaceName, originalName),
            Cache: null,
            ForceStatic: true,
            EnableInheritance: false);
        var bodyMembers = distinctMembers
            .Select(member => EmitNamespaceMember(member, emissionContext, accessorInfo))
            .Where(static code => !string.IsNullOrWhiteSpace(code))
            .ToList();

        var partialKeyword = declarations.Any(static declaration => declaration.Partial == true) ? " partial" : string.Empty;
        var builder = new StringBuilder();
        builder.AppendLine("/// <summary>");
        builder.AppendLine($"/// {originalName}");
        builder.AppendLine("/// </summary>");
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
        var className = WebIdlNaming.ToPascalCase(originalName);
        return $"global using {aliasName} = {GetQualifiedTypeName(namespaceName, className)};";
    }

    private string? EmitInterfaceMember(JsonElement member, InterfaceEmissionContext context, AccessorInfo accessorInfo)
    {
        var type = member.GetStringOrNull("type");
        return type switch
        {
            "constructor" => EmitConstructor(member, context.OwnerName, context.NamespaceName),
            "attribute" => EmitAttribute(member, context),
            "const" => EmitConst(member, context.NamespaceName),
            "operation" => EmitOperation(member, context, accessorInfo),
            "iterable" => EmitIterableMember(member, context.NamespaceName),
            "maplike" => EmitMaplikeMember(member, context.NamespaceName),
            "setlike" => EmitSetlikeMember(member, context.NamespaceName),
            _ => null,
        };
    }

    private string? EmitNamespaceMember(JsonElement member, InterfaceEmissionContext context, AccessorInfo accessorInfo)
    {
        var type = member.GetStringOrNull("type");
        return type switch
        {
            "attribute" => EmitAttribute(member, context),
            "const" => EmitConst(member, context.NamespaceName),
            "operation" => EmitOperation(member, context, accessorInfo),
            _ => null,
        };
    }

    private string? EmitAttribute(JsonElement attribute, InterfaceEmissionContext context)
    {
        var propertyType = _typeMapper.ToInlineType(attribute.GetProperty("idlType"), context.NamespaceName);
        var originalName = attribute.GetStringOrNull("name") ?? string.Empty;
        var propertyName = GetAttributePropertyName(originalName, context.OwnerName);
        var inheritanceDisposition = context.EnableInheritance
            ? GetPropertyInheritanceDisposition(context, propertyName, propertyType)
            : InheritanceDisposition.None;
        if (inheritanceDisposition == InheritanceDisposition.Skip)
        {
            return null;
        }

        var isStatic = context.ForceStatic || attribute.GetStringOrNull("special") == "static";
        var inheritanceModifier = inheritanceDisposition == InheritanceDisposition.New ? "new " : string.Empty;
        var isReadonly = attribute.GetBooleanOrNull("readonly") == true;

        return "/// <summary>\n"
            + $"/// {originalName}\n"
            + "/// </summary>\n"
            + $"[Description(\"@#{originalName}\")]\n"
            + $"public {inheritanceModifier}{(isStatic ? "static " : string.Empty)}extern {propertyType} {propertyName} {{ get;{(isReadonly ? string.Empty : " set;")} }}";
    }

    private string EmitConstructor(JsonElement constructor, string ownerName, string? namespaceName)
    {
        var arguments = constructor.GetArray("arguments");
        var parameters = new List<string>();
        var comments = new List<string>();

        foreach (var argument in arguments)
        {
            var name = argument.GetStringOrNull("name") ?? string.Empty;
            parameters.Add($"{_typeMapper.ToInlineType(argument.GetProperty("idlType"), namespaceName)} {WebIdlNaming.ToCamelCase(name)}");
            comments.Add($"/// <param name=\"{WebIdlNaming.ToCamelCase(name)}\">{name}</param>");
        }

        return "/// <summary>\n"
            + "/// Constructor \n"
            + "/// </summary>\n"
            + (comments.Count > 0 ? string.Join(Environment.NewLine, comments) + Environment.NewLine : string.Empty)
            + $"public extern {ownerName}({string.Join(", ", parameters)});";
    }

    private string? EmitOperation(JsonElement operation, InterfaceEmissionContext context, AccessorInfo accessorInfo)
    {
        var special = operation.GetStringOrNull("special") ?? string.Empty;
        var operationName = operation.GetStringOrNull("name");
        var returnType = operation.TryGetProperty("idlType", out var operationType)
            ? _typeMapper.ToInlineType(operationType, context.NamespaceName, "void")
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
            var unnamedParameters = BuildMethodParameters(arguments, context.NamespaceName);
            return special switch
            {
                "stringifier" => null,
                "getter" => EmitIndexerGetter(arguments, returnType, accessorInfo, context.NamespaceName, unnamedInheritanceModifier),
                "setter" => EmitIndexerSetter(arguments, context.OwnerName, accessorInfo, context.NamespaceName, unnamedInheritanceModifier),
                "deleter" => $"[Description(\"@#\")]{Environment.NewLine}[Jazor(\"{BuildDeleterInlineTemplate(unnamedParameters)}\")]{Environment.NewLine}public extern {unnamedInheritanceModifier}void Delete({string.Join(", ", unnamedParameters.Select(static parameter => parameter.Signature))});",
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

        var parameters = BuildMethodParameters(arguments, context.NamespaceName);
        var isStatic = context.ForceStatic || special == "static";
        var inheritanceModifier = inheritanceDisposition == InheritanceDisposition.New ? "new " : string.Empty;
        var builder = new StringBuilder();
        builder.AppendLine("/// <summary>");
        builder.AppendLine($"/// {operationName}");
        builder.AppendLine("/// </summary>");
        foreach (var parameter in parameters)
        {
            builder.AppendLine($"/// <param name=\"{parameter.CommentName}\">{parameter.OriginalName}</param>");
        }

        builder.AppendLine($"[Description(\"@#{operationName}\")]");
        builder.Append($"public {inheritanceModifier}{(isStatic ? "static " : string.Empty)}extern {returnType} {methodName}({string.Join(", ", parameters.Select(static parameter => parameter.Signature))});");

        var lastArgument = arguments.LastOrDefault();
        if (lastArgument.ValueKind != JsonValueKind.Undefined
            && lastArgument.TryGetProperty("idlType", out var lastIdlType)
            && lastIdlType.GetBooleanOrNull("union") == true)
        {
            var overloads = EmitUnionTailOverloads(operationName, methodName, returnType, isStatic, inheritanceModifier, arguments, context.NamespaceName);
            if (overloads.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.Append(string.Join(Environment.NewLine + Environment.NewLine, overloads));
            }
        }

        return builder.ToString();
    }

    private string? EmitIndexerGetter(
        IReadOnlyList<JsonElement> arguments,
        string returnType,
        AccessorInfo accessorInfo,
        string? namespaceName,
        string inheritanceModifier)
    {
        var parameters = BuildMethodParameters(arguments, namespaceName);
        return accessorInfo.HasSetter
            ? $"[Description(\"@#\")] {Environment.NewLine}public extern {inheritanceModifier}{returnType} this[{string.Join(", ", parameters.Select(static parameter => parameter.Signature))}] {{ get; set; }}"
            : $"[Description(\"@#\")] {Environment.NewLine}public extern {inheritanceModifier}{returnType} this[{string.Join(", ", parameters.Select(static parameter => parameter.Signature))}] {{ get; }}";
    }

    private string? EmitIndexerSetter(
        IReadOnlyList<JsonElement> arguments,
        string ownerName,
        AccessorInfo accessorInfo,
        string? namespaceName,
        string inheritanceModifier)
    {
        if (accessorInfo.HasGetter)
        {
            return null;
        }

        if (arguments.Count != 2)
        {
            throw new InvalidOperationException($"Setter operation on '{ownerName}' must have exactly two arguments.");
        }

        var indexType = _typeMapper.ToInlineType(arguments[0].GetProperty("idlType"), namespaceName);
        var indexName = WebIdlNaming.ToCamelCase(arguments[0].GetStringOrNull("name") ?? string.Empty);
        var valueType = _typeMapper.ToInlineType(arguments[1].GetProperty("idlType"), namespaceName);
        return $"[Description(\"@#\")] {Environment.NewLine}public extern {inheritanceModifier}{valueType} this[{indexType} {indexName}] {{ set; }}";
    }

    private string EmitIterableMember(JsonElement member, string? namespaceName)
    {
        var types = member.GetArray("idlType");
        var returnType = types.Count == 1
            ? _typeMapper.ToInlineType(types[0], namespaceName)
            : $"({_typeMapper.ToInlineType(types[0], namespaceName)}, {_typeMapper.ToInlineType(types[1], namespaceName)})";

        return $"extern IEnumerator<{returnType}> IEnumerable<{returnType}>.GetEnumerator();{Environment.NewLine}extern IEnumerator IEnumerable.GetEnumerator();";
    }

    private string EmitMaplikeMember(JsonElement member, string? namespaceName)
    {
        var keyType = _typeMapper.ToInlineType(member.GetArray("idlType")[0], namespaceName);
        var valueType = _typeMapper.ToInlineType(member.GetArray("idlType")[1], namespaceName);
        return "#region Dictionary\n"
            + $"extern {valueType} IDictionary<{keyType}, {valueType}>.this[{keyType} key] {{ get; set; }}\n"
            + $"extern ICollection<{keyType}> IDictionary<{keyType}, {valueType}>.Keys {{ get; }}\n"
            + $"extern ICollection<{valueType}> IDictionary<{keyType}, {valueType}>.Values {{ get; }}\n"
            + $"extern int ICollection<KeyValuePair<{keyType}, {valueType}>>.Count {{ get; }}\n"
            + $"extern bool ICollection<KeyValuePair<{keyType}, {valueType}>>.IsReadOnly {{ get; }}\n"
            + $"extern void IDictionary<{keyType}, {valueType}>.Add({keyType} key, {valueType} value);\n"
            + $"extern void ICollection<KeyValuePair<{keyType}, {valueType}>>.Add(KeyValuePair<{keyType}, {valueType}> item);\n"
            + $"extern void ICollection<KeyValuePair<{keyType}, {valueType}>>.Clear();\n"
            + $"extern bool ICollection<KeyValuePair<{keyType}, {valueType}>>.Contains(KeyValuePair<{keyType}, {valueType}> item);\n"
            + $"extern bool IDictionary<{keyType}, {valueType}>.ContainsKey({keyType} key);\n"
            + $"extern void ICollection<KeyValuePair<{keyType}, {valueType}>>.CopyTo(KeyValuePair<{keyType}, {valueType}>[] array, int arrayIndex);\n"
            + $"extern IEnumerator<KeyValuePair<{keyType}, {valueType}>> IEnumerable<KeyValuePair<{keyType}, {valueType}>>.GetEnumerator();\n"
            + $"extern bool IDictionary<{keyType}, {valueType}>.Remove({keyType} key);\n"
            + $"extern bool ICollection<KeyValuePair<{keyType}, {valueType}>>.Remove(KeyValuePair<{keyType}, {valueType}> item);\n"
            + $"extern bool IDictionary<{keyType}, {valueType}>.TryGetValue({keyType} key, [MaybeNullWhen(false)] out {valueType} value);\n"
            + "extern IEnumerator IEnumerable.GetEnumerator();\n"
            + "#endregion";
    }

    private string EmitSetlikeMember(JsonElement member, string? namespaceName)
    {
        var type = _typeMapper.ToInlineType(member.GetArray("idlType")[0], namespaceName);
        return "#region Set\n"
            + $"extern int ICollection<{type}>.Count {{ get; }}\n"
            + $"extern bool ICollection<{type}>.IsReadOnly {{ get; }}\n"
            + $"extern bool ISet<{type}>.Add({type} item);\n"
            + $"extern void ICollection<{type}>.Clear();\n"
            + $"extern bool ICollection<{type}>.Contains({type} item);\n"
            + $"extern void ICollection<{type}>.CopyTo({type}[] array, int arrayIndex);\n"
            + $"extern void ISet<{type}>.ExceptWith(IEnumerable<{type}> other);\n"
            + $"extern IEnumerator<{type}> IEnumerable<{type}>.GetEnumerator();\n"
            + $"extern void ISet<{type}>.IntersectWith(IEnumerable<{type}> other);\n"
            + $"extern bool ISet<{type}>.IsProperSubsetOf(IEnumerable<{type}> other);\n"
            + $"extern bool ISet<{type}>.IsProperSupersetOf(IEnumerable<{type}> other);\n"
            + $"extern bool ISet<{type}>.IsSubsetOf(IEnumerable<{type}> other);\n"
            + $"extern bool ISet<{type}>.IsSupersetOf(IEnumerable<{type}> other);\n"
            + $"extern bool ISet<{type}>.Overlaps(IEnumerable<{type}> other);\n"
            + $"extern bool ICollection<{type}>.Remove({type} item);\n"
            + $"extern bool ISet<{type}>.SetEquals(IEnumerable<{type}> other);\n"
            + $"extern void ISet<{type}>.SymmetricExceptWith(IEnumerable<{type}> other);\n"
            + $"extern void ISet<{type}>.UnionWith(IEnumerable<{type}> other);\n"
            + $"extern void ICollection<{type}>.Add({type} item);\n"
            + "extern IEnumerator IEnumerable.GetEnumerator();\n"
            + "#endregion";
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
            ? _typeMapper.ToInlineType(types[0], namespaceName)
            : $"({_typeMapper.ToInlineType(types[0], namespaceName)}, {_typeMapper.ToInlineType(types[1], namespaceName)})";
        return $"IEnumerable<{returnType}>";
    }

    private string GetMaplikeInterface(JsonElement member, string? namespaceName)
    {
        var keyType = _typeMapper.ToInlineType(member.GetArray("idlType")[0], namespaceName);
        var valueType = _typeMapper.ToInlineType(member.GetArray("idlType")[1], namespaceName);
        return $"IDictionary<{keyType}, {valueType}>";
    }

    private string GetSetlikeInterface(JsonElement member, string? namespaceName)
    {
        var type = _typeMapper.ToInlineType(member.GetArray("idlType")[0], namespaceName);
        return $"ISet<{type}>";
    }

    private IReadOnlyList<MethodParameterEmission> BuildMethodParameters(IReadOnlyList<JsonElement> arguments, string? namespaceName)
    {
        var parameters = new List<MethodParameterEmission>();
        var hasOptionalParameter = false;

        foreach (var argument in arguments)
        {
            var originalName = argument.GetStringOrNull("name") ?? string.Empty;
            var name = WebIdlNaming.ToCamelCase(originalName);
            var idlType = argument.GetProperty("idlType");
            var type = _typeMapper.ToInlineType(idlType, namespaceName);
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
                    Name: name));
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
                Name: name));
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
        string? namespaceName)
    {
        var lastArgument = arguments[^1];
        var lastArgumentName = WebIdlNaming.ToCamelCase(lastArgument.GetStringOrNull("name") ?? string.Empty);
        var priorArguments = arguments.Take(arguments.Count - 1).ToArray();
        var priorParameters = BuildMethodParameters(priorArguments, namespaceName);
        var overloads = new List<string>();

        foreach (var unionType in lastArgument.GetProperty("idlType").GetArray("idlType"))
        {
            var type = _typeMapper.ToInlineType(unionType, namespaceName);
            var typeKey = type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type;
            var parameterType = _typeMapper.IsDictionaryType(typeKey) ? $"{typeKey}?" : type;
            var signatureParts = priorParameters.Select(static parameter => parameter.Signature).ToList();
            signatureParts.Add(_typeMapper.IsDictionaryType(typeKey)
                ? $"{parameterType} {lastArgumentName} = default"
                : $"{parameterType} {lastArgumentName}");

            var builder = new StringBuilder();
            builder.AppendLine("/// <summary>");
            builder.AppendLine($"/// {originalOperationName}");
            builder.AppendLine("/// </summary>");
            foreach (var parameter in priorParameters)
            {
                builder.AppendLine($"/// <param name=\"{parameter.CommentName}\">{parameter.OriginalName} para</param>");
            }

            builder.AppendLine($"/// <param name=\"{lastArgumentName.TrimStart('@')}\">{lastArgument.GetStringOrNull("name")}</param>");
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
                .Select(member => BuildConstructorCache(member, namespaceName))
                .ToArray();
            var className = WebIdlNaming.ToPascalCase(group.Key.Name);
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

        var pascalName = WebIdlNaming.ToPascalCase(typeName);
        var pascalMatches = _interfaceKeysByName
            .Where(pair => WebIdlNaming.ToPascalCase(pair.Key) == pascalName)
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
            $"{argument.GetStringOrNull("name")}:{_typeMapper.ToInlineType(argument.GetProperty("idlType"), context.NamespaceName)}"));
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

    private ConstructorCache BuildConstructorCache(JsonElement constructor, string? namespaceName)
    {
        var arguments = constructor.GetArray("arguments");
        var parameterList = string.Join(", ", arguments.Select(argument =>
        {
            var argType = _typeMapper.ToInlineType(argument.GetProperty("idlType"), namespaceName);
            var argName = WebIdlNaming.ToCamelCase(argument.GetStringOrNull("name") ?? string.Empty);
            return $"{argType} {argName}";
        }));
        var argumentList = string.Join(", ", arguments.Select(argument => WebIdlNaming.ToCamelCase(argument.GetStringOrNull("name") ?? string.Empty)));
        var typeSignature = string.Join("@", arguments.Select(argument => _typeMapper.ToInlineType(argument.GetProperty("idlType"), namespaceName)));
        return new ConstructorCache(arguments.Count, parameterList, argumentList, typeSignature);
    }

    private PropertyCache BuildPropertyCache(JsonElement attribute, string ownerName, string className, string? namespaceName)
    {
        var originalName = attribute.GetStringOrNull("name") ?? string.Empty;
        return new PropertyCache(
            GetAttributePropertyName(originalName, ownerName, className),
            _typeMapper.ToInlineType(attribute.GetProperty("idlType"), namespaceName));
    }

    private OperationCache BuildOperationCache(JsonElement operation, string? namespaceName)
    {
        var special = operation.GetStringOrNull("special") ?? string.Empty;
        var operationName = operation.GetStringOrNull("name");
        var methodName = string.IsNullOrWhiteSpace(operationName)
            ? string.Empty
            : GetOperationMethodName(operationName);
        var argumentSignature = string.Join("@", operation.GetArray("arguments").Select(argument =>
            $"{argument.GetStringOrNull("name")}:{_typeMapper.ToInlineType(argument.GetProperty("idlType"), namespaceName)}"));
        var returnType = operation.TryGetProperty("idlType", out var operationType)
            ? _typeMapper.ToInlineType(operationType, namespaceName, "void")
            : "void";
        return new OperationCache(special, methodName, argumentSignature, returnType);
    }

    private static string GetAttributePropertyName(string originalName, string ownerName, string? className = null)
    {
        var propertyName = WebIdlNaming.ToPascalCase(originalName);
        var resolvedClassName = className ?? WebIdlNaming.ToPascalCase(ownerName);
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
            "constructor" => $"{type}${string.Join("@", member.GetArray("arguments").Select(argument => $"{argument.GetStringOrNull("name")}:{_typeMapper.ToInlineType(argument.GetProperty("idlType"), namespaceName)}"))}",
            "operation" => $"{type}${member.GetStringOrNull("name")}${string.Join("@", member.GetArray("arguments").Select(argument => $"{argument.GetStringOrNull("name")}:{_typeMapper.ToInlineType(argument.GetProperty("idlType"), namespaceName)}"))}{(member.TryGetProperty("idlType", out var operationType) ? _typeMapper.ToInlineType(operationType, namespaceName, "void") : "void")}${member.GetStringOrNull("special")}",
            "attribute" => $"{type}${member.GetStringOrNull("name")}${_typeMapper.ToInlineType(member.GetProperty("idlType"), namespaceName)}",
            "const" => $"{type}${member.GetStringOrNull("name")}${_typeMapper.ToInlineType(member.GetProperty("idlType"), namespaceName)}",
            "iterable" or "maplike" or "setlike" => $"{type}${string.Join(":", member.GetArray("idlType").Select(idlType => _typeMapper.ToInlineType(idlType, namespaceName)))}",
            _ => type,
        };
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
        string ArgumentCode);

    private sealed record MethodParameterEmission(
        string OriginalName,
        string CommentName,
        string Signature,
        string Type,
        string Name);

    private sealed record AccessorInfo(
        bool HasGetter,
        bool HasSetter);

    private sealed record InterfaceEmissionContext(
        string OwnerName,
        string? NamespaceName,
        string InterfaceKey,
        InterfaceCache? Cache,
        bool ForceStatic,
        bool EnableInheritance);

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

    private enum InheritanceDisposition
    {
        None,
        New,
        Skip,
    }
}
