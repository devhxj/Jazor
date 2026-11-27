using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Template;

/// <summary>维护消息模板，并用给定数据渲染模板。</summary>
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
            Name = input.Name,
            Code = input.Code,
            Type = input.Type,
            GroupName = input.GroupName,
            Content = input.Content,
            OrderNo = input.OrderNo,
            Remark = input.Remark
        });

    public static IPromise<ApiOutcome> Update(TemplateSaveInput input)
        => BrowserApiClient.Post("/api/sysTemplate/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysTemplate/delete", new TemplateIdInput { Id = id });

    public static IPromise<ApiOutcome> Render(string content, VueDictionary<string> data)
        => BrowserApiClient.Post("/api/sysTemplate/render", new TemplateRenderInput
        {
            Content = content,
            Data = data
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
                Value = value,
                Label = label.Length == 0 ? "类型" + value : label,
                TagType = ReadString(values[index]!, "tagType")
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
            Id = ReadString(value, "id"),
            Name = ReadString(value, "name"),
            Code = ReadString(value, "code"),
            Type = ReadInt(value, "type", 1),
            GroupName = ReadString(value, "groupName"),
            Content = ReadString(value, "content"),
            OrderNo = ReadInt(value, "orderNo", 100),
            Remark = ReadString(value, "remark"),
            CreateTime = ReadString(value, "createTime"),
            CreateUserId = ReadString(value, "createUserId"),
            CreateUserName = ReadString(value, "createUserName"),
            UpdateTime = ReadString(value, "updateTime"),
            UpdateUserId = ReadString(value, "updateUserId"),
            UpdateUserName = ReadString(value, "updateUserName")
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
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("field")]
    public string Field { get; init; } = "orderNo";
    [ECMAScriptName("order")]
    public string Order { get; init; } = "ascending";
    [ECMAScriptName("descStr")]
    public string DescStr { get; init; } = "descending";
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("type")]
    public int? Type { get; init; }
    [ECMAScriptName("groupName")]
    public string GroupName { get; init; } = string.Empty;
}

public sealed record TemplateSaveInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("type")]
    public int Type { get; init; } = 1;
    [ECMAScriptName("groupName")]
    public string GroupName { get; init; } = string.Empty;
    [ECMAScriptName("content")]
    public string Content { get; init; } = string.Empty;
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
}

public sealed record TemplateAddInput
{
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("type")]
    public int Type { get; init; } = 1;
    [ECMAScriptName("groupName")]
    public string GroupName { get; init; } = string.Empty;
    [ECMAScriptName("content")]
    public string Content { get; init; } = string.Empty;
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
}

public sealed record TemplateIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record TemplateRenderInput
{
    [ECMAScriptName("content")]
    public string Content { get; init; } = string.Empty;
    [ECMAScriptName("data")]
    public VueDictionary<string> Data { get; init; } = new();
}


