using System.Text.Json;

namespace ECMAScript.WebIDL.Generator;

internal sealed class WebIdlTypeMapper
{
    private static readonly HashSet<string> OptionalPrimitiveTypes =
    [
        "int", "float", "double", "bool", "long",
        "short", "ushort", "uint", "ulong", "byte", "sbyte"
    ];

    private static readonly IReadOnlyDictionary<string, string> PrimitiveTypeMap = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["bigint"] = "BigInteger",
        ["DOMString"] = "string",
        ["USVString"] = "string",
        ["CSSOMString"] = "string",
        ["HTMLString"] = "string",
        ["ScriptString"] = "string",
        ["ScriptURLString"] = "string",
        ["ByteString"] = "byte[]",
        ["boolean"] = "bool",
        ["byte"] = "sbyte",
        ["octet"] = "byte",
        ["short"] = "short",
        ["unsigned short"] = "ushort",
        ["long"] = "int",
        ["unsigned long"] = "uint",
        ["long long"] = "long",
        ["unsigned long long"] = "ulong",
        ["float"] = "float",
        ["unrestricted float"] = "float",
        ["double"] = "double",
        ["unrestricted double"] = "double",
        ["DOMHighResTimeStamp"] = "double",
        ["any"] = "object",
        ["object"] = "object",
        ["void"] = "void",
        ["DOMTokenList"] = "List<string>",
        ["BufferSource"] = "IBufferSource",
        ["ArrayBufferView"] = "IArrayBufferView",
        ["Function"] = "Delegate",
        ["VoidFunction"] = "Action",
        ["AllowSharedBufferSource"] = "IAllowSharedBufferSource",
    };

    private static readonly IReadOnlyDictionary<string, string> FullyQualifiedTypeMap = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["Delegate"] = "System.Delegate",
        ["BigInteger"] = "System.Numerics.BigInteger",
        ["ArrayBuffer"] = "ECMAScript.ArrayBuffer",
        ["IBufferSource"] = "ECMAScript.IBufferSource",
        ["IArrayBufferView"] = "ECMAScript.IArrayBufferView",
        ["ReadableStream"] = "ECMAScript.ReadableStream",
        ["ImageBitmap"] = "ECMAScript.ImageBitmap",
        ["ImageData"] = "ECMAScript.ImageData",
        ["HTMLImageElement"] = "ECMAScript.HTMLImageElement",
        ["HTMLVideoElement"] = "ECMAScript.HTMLVideoElement",
        ["VideoFrame"] = "ECMAScript.VideoFrame",
        ["HTMLCanvasElement"] = "ECMAScript.HTMLCanvasElement",
        ["OffscreenCanvas"] = "ECMAScript.OffscreenCanvas",
        ["GPUCanvasContext"] = "ECMAScript.GPUBufferUsage.GPUCanvasContext",
        ["IAllowSharedBufferSource"] = "ECMAScript.IAllowSharedBufferSource",
    };

    private readonly Dictionary<string, string> _typedefValueByName = new(StringComparer.Ordinal);
    private readonly HashSet<string> _enumTypeNames = new(StringComparer.Ordinal);
    private readonly HashSet<string> _dictionaryTypeNames = new(StringComparer.Ordinal);

    public string ToInlineType(JsonElement idlType, string? namespaceName, string defaultValue = "object")
    {
        return ToSharpType(idlType, namespaceName, qualifyForAlias: false, defaultValue);
    }

    public string ToAliasTargetType(JsonElement idlType, string? namespaceName, string defaultValue = "object")
    {
        return ToSharpType(idlType, namespaceName, qualifyForAlias: true, defaultValue);
    }

    public void RegisterAlias(string aliasName, string value)
    {
        _typedefValueByName[aliasName] = value;
    }

    public void RegisterEnum(string enumName)
    {
        _enumTypeNames.Add(enumName);
    }

    public void RegisterDictionary(string dictionaryName)
    {
        _dictionaryTypeNames.Add(dictionaryName);
    }

    public bool TryResolveAliasValue(string aliasName, out string value)
    {
        return _typedefValueByName.TryGetValue(aliasName, out value!);
    }

    public bool IsEnumType(string typeName)
    {
        return _enumTypeNames.Contains(typeName);
    }

    public bool IsOptionalPrimitive(string typeName)
    {
        return OptionalPrimitiveTypes.Contains(typeName);
    }

    public bool IsDictionaryType(string typeName)
    {
        return _dictionaryTypeNames.Contains(typeName);
    }

    public string FormatValue(JsonElement value, JsonElement? parentIdlType, string? namespaceName)
    {
        var kind = value.GetStringOrNull("type");
        return kind switch
        {
            "string" => $"\"{value.GetStringOrNull("value")}\"",
            "number" => value.GetStringOrNull("value") ?? "0",
            "boolean" => value.GetStringOrNull("value") ?? "false",
            "null" => "null",
            "Infinity" => value.GetBooleanOrNull("negative") == true ? "double.NegativeInfinity" : "double.PositiveInfinity",
            "NaN" => "null",
            "sequence" when parentIdlType is JsonElement idlType => $"new {ToInlineType(idlType, namespaceName)}()",
            "dictionary" => "new()",
            _ => string.Empty,
        };
    }

    private string ToSharpType(JsonElement idlType, string? namespaceName, bool qualifyForAlias, string defaultValue)
    {
        var generic = idlType.GetStringOrNull("generic") ?? string.Empty;
        var nullable = idlType.GetBooleanOrNull("nullable") == true;
        string sharpType;

        switch (generic)
        {
            case "":
            {
                if (idlType.GetBooleanOrNull("union") == true)
                {
                    var parts = idlType.GetArray("idlType")
                        .Select(part => new
                        {
                            Type = part,
                            MappedType = ToSharpType(part, namespaceName, qualifyForAlias, defaultValue),
                        })
                        .ToArray();

                    var concreteParts = parts
                        .Where(part => !IsVoidLikeType(part.Type, part.MappedType))
                        .Select(static part => part.MappedType)
                        .ToArray();
                    var hasVoidLikePart = parts.Length != concreteParts.Length;
                    sharpType = concreteParts.Length switch
                    {
                        0 => defaultValue,
                        1 when hasVoidLikePart => concreteParts[0].EndsWith("?", StringComparison.Ordinal)
                            ? concreteParts[0]
                            : $"{concreteParts[0]}?",
                        _ => $"{GetEitherTypePrefix(qualifyForAlias)}<{string.Join(", ", concreteParts)}>",
                    };
                }
                else
                {
                    var idlTypeName = idlType.GetStringOrNull("idlType") ?? defaultValue;
                    var defaultType = PrimitiveTypeMap.TryGetValue(idlTypeName, out var mappedType)
                        ? mappedType
                        : idlTypeName;

                    if (qualifyForAlias)
                    {
                        if (_typedefValueByName.TryGetValue(defaultType, out var aliasValue))
                        {
                            sharpType = $"{aliasValue}/*{defaultType}*/";
                        }
                        else if (FullyQualifiedTypeMap.TryGetValue(defaultType, out var fullyQualifiedType))
                        {
                            sharpType = fullyQualifiedType;
                        }
                        else if (PrimitiveTypeMap.TryGetValue(idlTypeName, out var primitiveType) && FullyQualifiedTypeMap.TryGetValue(primitiveType, out var qualifiedPrimitive))
                        {
                            sharpType = qualifiedPrimitive;
                        }
                        else if (PrimitiveTypeMap.TryGetValue(idlTypeName, out var primitiveInline))
                        {
                            sharpType = primitiveInline;
                        }
                        else
                        {
                            sharpType = $"{GetNamespacePrefix(namespaceName)}.{idlTypeName}";
                        }
                    }
                    else
                    {
                        sharpType = defaultType;
                    }
                }

                break;
            }
            case "sequence":
            {
                var subType = ToSharpType(idlType.GetArray("idlType")[0], namespaceName, qualifyForAlias, defaultValue);
                sharpType = $"{subType}[]";
                break;
            }
            case "Promise":
            {
                var promiseResultType = idlType.GetArray("idlType")[0];
                var subType = ToSharpType(promiseResultType, namespaceName, qualifyForAlias, defaultValue);
                var prefix = qualifyForAlias ? "ECMAScript.PromiseResult" : "PromiseResult";
                sharpType = IsVoidLikeType(promiseResultType, subType) ? prefix : $"{prefix}<{subType}>";
                break;
            }
            case "record":
            {
                var keyType = ToSharpType(idlType.GetArray("idlType")[0], namespaceName, qualifyForAlias, defaultValue);
                var valueType = ToSharpType(idlType.GetArray("idlType")[1], namespaceName, qualifyForAlias, defaultValue);
                sharpType = qualifyForAlias
                    ? $"System.Collections.Generic.Dictionary<{keyType}, {valueType}>"
                    : $"Dictionary<{keyType}, {valueType}>";
                break;
            }
            case "FrozenArray":
            {
                var subType = ToSharpType(idlType.GetArray("idlType")[0], namespaceName, qualifyForAlias, defaultValue);
                sharpType = $"FrozenSet<{subType}>";
                break;
            }
            case "ObservableArray":
            {
                var subType = ToSharpType(idlType.GetArray("idlType")[0], namespaceName, qualifyForAlias, defaultValue);
                sharpType = $"ObservableCollection<{subType}>";
                break;
            }
            default:
                sharpType = defaultValue;
                break;
        }

        if (nullable)
        {
            sharpType += "?";
        }

        return sharpType == "undefined" ? defaultValue : sharpType;
    }

    private static string GetEitherTypePrefix(bool qualifyForAlias)
    {
        return qualifyForAlias ? "ECMAScript.Either" : "Either";
    }

    private static bool IsVoidLikeType(JsonElement idlType, string mappedType)
    {
        if (mappedType is "void" or "undefined")
        {
            return true;
        }

        if ((idlType.GetStringOrNull("generic") ?? string.Empty) != string.Empty
            || idlType.GetBooleanOrNull("union") == true)
        {
            return false;
        }

        return idlType.GetStringOrNull("idlType") is "void" or "undefined";
    }

    private static string GetNamespacePrefix(string? namespaceName)
    {
        return string.IsNullOrWhiteSpace(namespaceName)
            ? "ECMAScript"
            : $"ECMAScript.{WebIdlNaming.ToPascalCase(namespaceName)}";
    }
}
