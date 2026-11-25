using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.LangText;

/// <summary>Typed browser boundary for translation text management.</summary>
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
            entityName = input.entityName,
            entityId = input.entityId,
            fieldName = input.fieldName,
            langCode = input.langCode,
            content = input.content
        });

    public static IPromise<ApiOutcome> Update(LangTextSaveInput input)
        => BrowserApiClient.Post("/api/sysLangText/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysLangText/delete", new LangTextIdInput { id = id });

    public static IPromise<ApiOutcome> BatchDelete(string[] ids)
    {
        var input = new LangTextIdInput[ids.Length];
        for (var index = 0; index < ids.Length; index++)
            input[index] = new LangTextIdInput { id = ids[index] };
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
            id = ReadString(value, "id"),
            entityName = ReadString(value, "entityName"),
            entityId = ReadString(value, "entityId"),
            fieldName = ReadString(value, "fieldName"),
            langCode = ReadString(value, "langCode"),
            content = ReadString(value, "content"),
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
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 20;
    public string field { get; init; } = "createTime";
    public string order { get; init; } = "descending";
    public string descStr { get; init; } = "descending";
    public string keyword { get; init; } = string.Empty;
    public string entityName { get; init; } = string.Empty;
    public string? entityId { get; init; }
    public string fieldName { get; init; } = string.Empty;
    public string langCode { get; init; } = string.Empty;
    public string content { get; init; } = string.Empty;
    public string[] selectKeyList { get; init; } = [];
}

public sealed record LangTextSaveInput
{
    public string id { get; init; } = string.Empty;
    public string entityName { get; init; } = string.Empty;
    public string entityId { get; init; } = string.Empty;
    public string fieldName { get; init; } = string.Empty;
    public string langCode { get; init; } = string.Empty;
    public string content { get; init; } = string.Empty;
}

public sealed record LangTextAddInput
{
    public string entityName { get; init; } = string.Empty;
    public string entityId { get; init; } = string.Empty;
    public string fieldName { get; init; } = string.Empty;
    public string langCode { get; init; } = string.Empty;
    public string content { get; init; } = string.Empty;
}

public sealed record LangTextIdInput
{
    public string id { get; init; } = string.Empty;
}
