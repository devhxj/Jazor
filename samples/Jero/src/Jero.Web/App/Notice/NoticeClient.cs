using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Notice;

/// <summary>Typed browser boundary for notice management.</summary>
[ECMAScriptModule("components/notice-client.js")]
public static class NoticeClient
{
    public static IPromise<ApiOutcome> Page(NoticePageInput input)
        => BrowserApiClient.Post("/api/sysNotice/page", input);

    public static IPromise<ApiOutcome> Types()
        => BrowserApiClient.Get("/api/sysDictType/dataList?code=NoticeTypeEnum");

    public static IPromise<ApiOutcome> Statuses()
        => BrowserApiClient.Get("/api/sysDictType/dataList?code=NoticeStatusEnum");

    public static IPromise<ApiOutcome> Add(NoticeAddInput input)
        => BrowserApiClient.Post("/api/sysNotice/add", input);

    public static IPromise<ApiOutcome> Update(NoticeSaveInput input)
        => BrowserApiClient.Post("/api/sysNotice/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysNotice/delete", new NoticeIdInput { id = id });

    public static IPromise<ApiOutcome> Publish(string id)
        => BrowserApiClient.Post("/api/sysNotice/public", new NoticeIdInput { id = id });

    public static NoticeView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new NoticeView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadItem(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static NoticeOption[] ReadTypeOptions(object? data)
        => ReadOptions(data, false);

    public static NoticeOption[] ReadStatusOptions(object? data)
        => ReadOptions(data, true);

    private static NoticeOption[] ReadOptions(object? data, bool includeZero)
    {
        var values = data as object?[] ?? [];
        var validCount = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var value = ReadInt(values[index]!, "value", -1);
            if (includeZero ? value >= 0 : value > 0)
                validCount++;
        }

        var result = new NoticeOption[validCount];
        var target = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var item = values[index]!;
            var value = ReadInt(item, "value", -1);
            if (!(includeZero ? value >= 0 : value > 0))
                continue;

            var label = ReadString(item, "label");
            if (label.Length == 0)
                label = ReadString(item, "name");

            result[target] = new NoticeOption
            {
                value = value,
                label = label.Length == 0 ? "选项" + value : label,
                tagType = ReadString(item, "tagType")
            };
            target++;
        }
        return result;
    }

    private static NoticeView ReadItem(object value)
    {
        var content = ReadString(value, "content");
        return new NoticeView
        {
            id = ReadString(value, "id"),
            title = ReadString(value, "title"),
            content = content,
            contentSummary = NoticeBrowserActions.ToPlainText(content),
            type = ReadInt(value, "type", 1),
            status = ReadInt(value, "status", 0),
            publicUserId = ReadString(value, "publicUserId"),
            publicUserName = ReadString(value, "publicUserName"),
            publicOrgId = ReadString(value, "publicOrgId"),
            publicOrgName = ReadString(value, "publicOrgName"),
            publicTime = ReadString(value, "publicTime"),
            cancelTime = ReadString(value, "cancelTime"),
            createTime = ReadString(value, "createTime"),
            createUserId = ReadString(value, "createUserId"),
            createUserName = ReadString(value, "createUserName"),
            updateTime = ReadString(value, "updateTime"),
            updateUserId = ReadString(value, "updateUserId"),
            updateUserName = ReadString(value, "updateUserName")
        };
    }

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

public sealed record NoticePageInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string title { get; init; } = string.Empty;
    public int? type { get; init; }
}

public sealed record NoticeAddInput
{
    public string title { get; init; } = string.Empty;
    public string content { get; init; } = string.Empty;
    public int type { get; init; } = 1;
    public int status { get; init; }
}

/// <summary>
/// Full update payload. SysNoticeService updates the adapted entity as a whole, so audit,
/// publication and creator fields must be retained from the selected row.
/// </summary>
public sealed record NoticeSaveInput
{
    public string id { get; init; } = string.Empty;
    public string title { get; init; } = string.Empty;
    public string content { get; init; } = string.Empty;
    public int type { get; init; } = 1;
    public int status { get; init; }
    public string publicUserId { get; init; } = "0";
    public string? publicUserName { get; init; }
    public string publicOrgId { get; init; } = "0";
    public string? publicOrgName { get; init; }
    public string? publicTime { get; init; }
    public string? cancelTime { get; init; }
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string? createUserName { get; init; }
    public string? updateTime { get; init; }
    public string? updateUserId { get; init; }
    public string? updateUserName { get; init; }
}

public sealed record NoticeIdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed record NoticeView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string title { get; init; } = string.Empty;
    public string content { get; init; } = string.Empty;
    public string contentSummary { get; init; } = "-";
    public int type { get; init; }
    public int status { get; init; }
    public string publicUserId { get; init; } = string.Empty;
    public string publicUserName { get; init; } = string.Empty;
    public string publicOrgId { get; init; } = string.Empty;
    public string publicOrgName { get; init; } = string.Empty;
    public string publicTime { get; init; } = string.Empty;
    public string cancelTime { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
}

public sealed record NoticeOption : VueProps
{
    public int value { get; init; }
    public string label { get; init; } = string.Empty;
    public string tagType { get; init; } = string.Empty;
}
