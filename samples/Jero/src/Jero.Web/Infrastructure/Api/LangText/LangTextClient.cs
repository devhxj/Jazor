using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.LangText;

/// <summary>查询翻译文本和详情，支持更新和批量删除。</summary>
[ECMAScriptModule("components/lang-text-client.js")]
public static class LangTextClient
{
    public static IPromise<ApiOutcome> Page(LangTextPageInput input)
        => BrowserApiClient.Post("/api/sysLangText/page", input);

    public static IPromise<ApiOutcome> Detail(string id)
        => BrowserApiClient.Get("/api/sysLangText/detail?id=" + EncodeURIComponent(id));

    public static IPromise<ApiOutcome> Add(LangTextSaveInput input)
        => BrowserApiClient.Post("/api/sysLangText/add", new LangTextAddInput
        {
            EntityName = input.EntityName,
            EntityId = input.EntityId,
            FieldName = input.FieldName,
            LangCode = input.LangCode,
            Content = input.Content
        });

    public static IPromise<ApiOutcome> Update(LangTextSaveInput input)
        => BrowserApiClient.Post("/api/sysLangText/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysLangText/delete", new LangTextIdInput { Id = id });

    public static IPromise<ApiOutcome> BatchDelete(string[] ids)
    {
        var input = new LangTextIdInput[ids.Length];
        for (var index = 0; index < ids.Length; index++)
            input[index] = new LangTextIdInput { Id = ids[index] };
        return BrowserApiClient.Post("/api/sysLangText/batchDelete", input);
    }

    public static LangTextView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new LangTextView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadItem(values[index]!);
        return result;
    }

    public static LangTextView? ReadDetail(object? data)
        => data is null ? null : ReadItem(data);

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    private static LangTextView ReadItem(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            EntityName = ReadString(value, "entityName"),
            EntityId = ReadString(value, "entityId"),
            FieldName = ReadString(value, "fieldName"),
            LangCode = ReadString(value, "langCode"),
            Content = ReadString(value, "content"),
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
        if (value is null)
            return string.Empty;

        var result = StringValue(value);
        return result == "null" || result == "undefined" ? string.Empty : result;
    }

    private static int ReadInt(object data, string key, int fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : (int)NumberValue(value);
    }
}

public sealed record LangTextPageInput
{
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 20;
    [ECMAScriptName("field")]
    public string Field { get; init; } = "createTime";
    [ECMAScriptName("order")]
    public string Order { get; init; } = "descending";
    [ECMAScriptName("descStr")]
    public string DescStr { get; init; } = "descending";
    [ECMAScriptName("keyword")]
    public string Keyword { get; init; } = string.Empty;
    [ECMAScriptName("entityName")]
    public string EntityName { get; init; } = string.Empty;
    [ECMAScriptName("entityId")]
    public string? EntityId { get; init; }
    [ECMAScriptName("fieldName")]
    public string FieldName { get; init; } = string.Empty;
    [ECMAScriptName("langCode")]
    public string LangCode { get; init; } = string.Empty;
    [ECMAScriptName("content")]
    public string Content { get; init; } = string.Empty;
    [ECMAScriptName("selectKeyList")]
    public string[] SelectKeyList { get; init; } = [];
}

public sealed record LangTextSaveInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("entityName")]
    public string EntityName { get; init; } = string.Empty;
    [ECMAScriptName("entityId")]
    public string EntityId { get; init; } = string.Empty;
    [ECMAScriptName("fieldName")]
    public string FieldName { get; init; } = string.Empty;
    [ECMAScriptName("langCode")]
    public string LangCode { get; init; } = string.Empty;
    [ECMAScriptName("content")]
    public string Content { get; init; } = string.Empty;
}

public sealed record LangTextAddInput
{
    [ECMAScriptName("entityName")]
    public string EntityName { get; init; } = string.Empty;
    [ECMAScriptName("entityId")]
    public string EntityId { get; init; } = string.Empty;
    [ECMAScriptName("fieldName")]
    public string FieldName { get; init; } = string.Empty;
    [ECMAScriptName("langCode")]
    public string LangCode { get; init; } = string.Empty;
    [ECMAScriptName("content")]
    public string Content { get; init; } = string.Empty;
}

public sealed record LangTextIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}
