using System.Text.Json;

namespace ECMAScript.WebIDL.Generator;

/// <summary>
/// Complements specification prose with frozen MDN original text. C# bridge notes
/// describe only facts in the IDL contract; they never invent browser behavior.
/// </summary>
internal sealed class BindingDocumentationCatalog
{
    private readonly Dictionary<string, Entry> _entries = new(StringComparer.Ordinal);
    private sealed record Entry(string Summary, string Href, Dictionary<string, string> Parameters, string? Returns);

    public BindingDocumentationCatalog(string repositoryRoot)
    {
        var path = Path.Combine(repositoryRoot, "src", "ECMAScript.WebIDL.Generator", "documentation", "mdn.json");
        if (!File.Exists(path)) return;
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        foreach (var row in document.RootElement.GetProperty("descriptions").EnumerateObject())
            _entries[row.Name] = row.Value.Deserialize<Entry>()!;
    }

    public WebIdlInventory Apply(WebIdlInventory inventory)
        => inventory with { Files = inventory.Files.Select(file => file with
        {
            Declarations = file.Declarations.Select(declaration => Enrich(file, declaration)).ToArray()
        }).ToArray() };

    private WebIdlDeclarationInventory Enrich(WebIdlFileInventory file, WebIdlDeclarationInventory declaration)
    {
        if (declaration.Name is not { } owner) return declaration;
        if (file.Source is null && file.FileName == "jazor.webref-css-properties.idl")
            file = file with { Source = new("CSS Object Model", "https://drafts.csswg.org/cssom-1/#the-cssstyledeclaration-interface") };
        if (file.Source is null) return declaration;
        var documentation = Resolve(declaration.Documentation, owner,
            $"WebIDL {declaration.Kind} {owner}。定义于 {file.Source.Title}。{Exposure(declaration.Payload)}", file.Source);
        var members = declaration.Payload.GetArray(declaration.Kind == "enum" ? "values" : "members");
        var result = new List<WebIdlMemberDocumentation>();
        for (var i = 0; i < members.Count; i++)
        {
            var member = members[i];
            var previous = declaration.MemberDocumentation?.FirstOrDefault(item => item.MemberIndex == i);
            var kind = member.GetStringOrNull("type") ?? string.Empty;
            var name = member.GetStringOrNull("name") ?? member.GetStringOrNull("value") ?? kind;
            if (kind == "constructor") name = owner;
            var key = owner + "." + name;
            var catalogKey = kind == "const" && _entries.ContainsKey("WebGL." + name) ? "WebGL." + name : key;
            var summary = Resolve(previous?.Documentation, catalogKey, DescribeContract(owner, member), file.Source);
            var arguments = member.GetArray("arguments");
            var parameterDocs = new List<WebIdlArgumentDocumentation>();
            _entries.TryGetValue(key, out var original);
            for (var j = 0; j < arguments.Count; j++)
            {
                var argument = arguments[j];
                var parameterName = argument.GetStringOrNull("name") ?? "argument";
                var authored = previous?.Arguments?.FirstOrDefault(item => item.ArgumentIndex == j)?.Documentation;
                if (original?.Parameters.TryGetValue(parameterName, out var text) == true && string.IsNullOrWhiteSpace(authored?.Prose))
                    authored = new(original.Href, "MDN Web Docs", parameterName, null, text);
                authored ??= new(summary.Href, summary.SpecificationTitle, parameterName, null,
                    $"WebIDL 参数 {parameterName}：{IdlType(argument)}。{Optionality(argument)}");
                parameterDocs.Add(new(j, authored));
            }
            result.Add(new(i, summary, parameterDocs, original?.Returns ?? previous?.Returns));
        }
        return declaration with { Documentation = documentation, MemberDocumentation = members.Count == 0 ? declaration.MemberDocumentation : result };
    }

    private WebIdlDocumentation Resolve(WebIdlDocumentation? current, string key, string contract, WebIdlSpecificationSource source)
    {
        if (!string.IsNullOrWhiteSpace(current?.Prose)) return current;
        if (_entries.TryGetValue(key, out var entry))
            return new(entry.Href, "MDN Web Docs", key, null, entry.Summary);
        return current is not null ? current with { Prose = contract }
            : new(source.Url, source.Title, key, null, contract);
    }

    private static string DescribeContract(string owner, JsonElement member)
    {
        var name = member.GetStringOrNull("name") ?? string.Empty;
        var kind = member.GetStringOrNull("type");
        return kind switch
        {
            "enum-value" => $"JavaScript 字符串取值 “{member.GetStringOrNull("value")}”；属于 {owner} 的规范取值域。",
            "const" => $"{owner} 的规范常量 {name}，WebIDL 类型为 {IdlType(member)}，值为 {member.GetProperty("value").GetStringOrNull("value") ?? member.GetProperty("value").GetRawText()}。",
            "field" => $"{owner} 字典中的 {name} 成员，WebIDL 类型为 {IdlType(member)}。{Optionality(member)}",
            "attribute" => $"JavaScript 属性 {owner}.{name}：{IdlType(member)}。{(member.GetBooleanOrNull("readonly") == true ? "只读；由宿主 API 提供当前值。" : "可读取或赋值；赋值按 WebIDL 类型转换规则处理。")}",
            "constructor" => $"构造浏览器提供的 {owner} 对象；参数按对应 WebIDL 构造签名传递。{Exposure(member)}",
            "operation" => $"JavaScript {owner}.{name}({string.Join(", ", member.GetArray("arguments").Select(a => a.GetStringOrNull("name")))}) 的强类型绑定，WebIDL 返回类型为 {IdlType(member)}。{Exposure(member)}",
            "iterable" => $"遍历 {owner} 中由宿主定义顺序的条目；迭代元素类型为 {IdlType(member)}。",
            "maplike" => $"{owner} 的 WebIDL 映射集合接口；键和值类型为 {IdlType(member)}。",
            "setlike" => $"{owner} 的 WebIDL 集合接口；元素类型为 {IdlType(member)}。",
            _ => $"{owner} 的 WebIDL {kind} 声明。"
        };
    }

    private static string Optionality(JsonElement value)
    {
        var requirement = value.GetBooleanOrNull("required") == true ? "必须提供该成员。" :
            value.GetBooleanOrNull("optional") == true || value.GetStringOrNull("type") == "field" ? "可省略。" : "调用时必须传入。";
        if (value.TryGetProperty("default", out var defaultValue) && defaultValue.ValueKind == JsonValueKind.Object)
            requirement += "WebIDL 默认值：" + (defaultValue.GetStringOrNull("value") ?? defaultValue.GetRawText()) + "。";
        return requirement;
    }

    private static string IdlType(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String) return element.GetString()!;
        if (element.ValueKind == JsonValueKind.Array) return string.Join(", ", element.EnumerateArray().Select(IdlType));
        if (element.ValueKind != JsonValueKind.Object) return "undefined";
        if (!element.TryGetProperty("idlType", out var type)) return "undefined";
        if (type.ValueKind == JsonValueKind.String) return type.GetString()!;
        if (type.ValueKind == JsonValueKind.Array)
            return string.Join(", ", type.EnumerateArray().Select(IdlType));
        var generic = type.GetStringOrNull("generic");
        var inner = IdlType(type);
        if (!string.IsNullOrWhiteSpace(generic)) inner = generic + "<" + inner + ">";
        return inner + (type.GetBooleanOrNull("nullable") == true ? "?" : "");
    }

    private static string Exposure(JsonElement element)
        => element.GetArray("extAttrs").Any(attribute => attribute.GetStringOrNull("name") == "SecureContext")
            ? "该 API 仅暴露于安全上下文。" : string.Empty;
}