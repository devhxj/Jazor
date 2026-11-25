using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Template;

/// <summary>Typed browser boundary for message template management.</summary>
[ECMAScriptModule("components/template-client.js")]
public static class TemplateClient
{
    public static IPromise<ApiOutcome> Page(TemplatePageInput input)
        => BrowserApiClient.Post("/api/sysTemplate/page", input);

    public static IPromise<ApiOutcome> Groups()
        => BrowserApiClient.Get("/api/sysTemplate/groupList");

    public static IPromise<ApiOutcome> Types()
        => BrowserApiClient.Get("/api/sysDictType/dataList?code=TemplateTypeEnum");

    public static IPromise<ApiOutcome> Add(TemplateSaveInput input)
        => BrowserApiClient.Post("/api/sysTemplate/add", new TemplateAddInput
        {
            name = input.name,
            code = input.code,
            type = input.type,
            groupName = input.groupName,
            content = input.content,
            orderNo = input.orderNo,
            remark = input.remark
        });

    public static IPromise<ApiOutcome> Update(TemplateSaveInput input)
        => BrowserApiClient.Post("/api/sysTemplate/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysTemplate/delete", new TemplateIdInput { id = id });

    public static IPromise<ApiOutcome> Render(string content, VueDictionary<string> data)
        => BrowserApiClient.Post("/api/sysTemplate/render", new TemplateRenderInput
        {
            content = content,
            data = data
        });

    public static TemplateView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new TemplateView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadItem(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static string[] ReadGroups(object? data)
    {
        var values = data as object?[] ?? [];
        var count = 0;
        for (var index = 0; index < values.Length; index++)
            if (values[index] is not null && StringValue(values[index]!).Trim().Length > 0)
                count++;

        var result = new string[count];
        var target = 0;
        for (var index = 0; index < values.Length; index++)
        {
            if (values[index] is null)
                continue;
            var value = StringValue(values[index]!).Trim();
            if (value.Length == 0)
                continue;
            result[target] = value;
            target++;
        }
        return result;
    }

    public static TemplateTypeOption[] ReadTypes(object? data)
    {
        var values = data as object?[] ?? [];
        var validCount = 0;
        for (var index = 0; index < values.Length; index++)
            if (ReadInt(values[index]!, "value", 0) > 0)
                validCount++;

        var result = new TemplateTypeOption[validCount];
        var target = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var value = ReadInt(values[index]!, "value", 0);
            if (value <= 0)
                continue;

            var label = ReadString(values[index]!, "name");
            if (label.Length == 0)
                label = ReadString(values[index]!, "label");

            result[target] = new TemplateTypeOption
            {
                value = value,
                label = label.Length == 0 ? "类型" + value : label,
                tagType = ReadString(values[index]!, "tagType")
            };
            target++;
        }
        return result;
    }

    public static string ReadRenderedHtml(object? data)
        => data is null ? string.Empty : StringValue(data);

    private static TemplateView ReadItem(object value)
        => new()
        {
            id = ReadString(value, "id"),
            name = ReadString(value, "name"),
            code = ReadString(value, "code"),
            type = ReadInt(value, "type", 1),
            groupName = ReadString(value, "groupName"),
            content = ReadString(value, "content"),
            orderNo = ReadInt(value, "orderNo", 100),
            remark = ReadString(value, "remark"),
            createTime = ReadString(value, "createTime"),
            createUserId = ReadString(value, "createUserId"),
            createUserName = ReadString(value, "createUserName"),
            updateTime = ReadString(value, "updateTime"),
            updateUserId = ReadString(value, "updateUserId"),
            updateUserName = ReadString(value, "updateUserName")
        };

    private static string ReadString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? string.Empty : StringValue(value);
    }

    private static int ReadInt(object data, string key, int fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : (int)NumberValue(value);
    }
}

public sealed record TemplatePageInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string field { get; init; } = "orderNo";
    public string order { get; init; } = "ascending";
    public string descStr { get; init; } = "descending";
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public int? type { get; init; }
    public string groupName { get; init; } = string.Empty;
}

public sealed record TemplateSaveInput
{
    public string id { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public int type { get; init; } = 1;
    public string groupName { get; init; } = string.Empty;
    public string content { get; init; } = string.Empty;
    public int orderNo { get; init; } = 100;
    public string remark { get; init; } = string.Empty;
}

public sealed record TemplateAddInput
{
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public int type { get; init; } = 1;
    public string groupName { get; init; } = string.Empty;
    public string content { get; init; } = string.Empty;
    public int orderNo { get; init; } = 100;
    public string remark { get; init; } = string.Empty;
}

public sealed record TemplateIdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed record TemplateRenderInput
{
    public string content { get; init; } = string.Empty;
    public VueDictionary<string> data { get; init; } = new();
}


