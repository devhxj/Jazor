using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.ReceivedNotice;

/// <summary>Typed browser boundary for the current user's received notices.</summary>
[ECMAScriptModule("components/received-notice-client.js")]
public static class ReceivedNoticeClient
{
    public static IPromise<ApiOutcome> Page(ReceivedNoticePageInput input)
        => BrowserApiClient.Post("/api/sysNotice/pageReceived", input);

    public static IPromise<ApiOutcome> Types()
        => BrowserApiClient.Get("/api/sysDictType/dataList?code=NoticeTypeEnum");

    public static IPromise<ApiOutcome> SetRead(string noticeId)
        => BrowserApiClient.Post("/api/sysNotice/setRead", new ReceivedNoticeIdInput { id = noticeId });

    public static ReceivedNoticeView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new ReceivedNoticeView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadItem(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static ReceivedNoticeOption[] ReadTypeOptions(object? data)
    {
        var values = data as object?[] ?? [];
        var validCount = 0;
        for (var index = 0; index < values.Length; index++)
            if (ReadInt(values[index]!, "value", 0) > 0)
                validCount++;

        var result = new ReceivedNoticeOption[validCount];
        var target = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var item = values[index]!;
            var value = ReadInt(item, "value", 0);
            if (value <= 0)
                continue;

            var label = ReadString(item, "label");
            if (label.Length == 0)
                label = ReadString(item, "name");

            result[target] = new ReceivedNoticeOption
            {
                value = value,
                label = label.Length == 0 ? "类型" + value : label,
                tagType = ReadString(item, "tagType")
            };
            target++;
        }
        return result;
    }

    private static ReceivedNoticeView ReadItem(object value)
    {
        var notice = Reflect.Get(value, "sysNotice");
        var content = notice is null ? string.Empty : ReadString(notice, "content");
        var noticeId = ReadString(value, "noticeId");
        if (noticeId.Length == 0 && notice is not null)
            noticeId = ReadString(notice, "id");

        return new ReceivedNoticeView
        {
            id = ReadString(value, "id"),
            noticeId = noticeId,
            userId = ReadString(value, "userId"),
            readStatus = ReadInt(value, "readStatus", 0),
            readTime = ReadString(value, "readTime"),
            title = notice is null ? string.Empty : ReadString(notice, "title"),
            content = content,
            contentSummary = ToPlainText(content),
            type = notice is null ? 0 : ReadInt(notice, "type", 0),
            publicUserName = notice is null ? string.Empty : ReadString(notice, "publicUserName"),
            publicTime = notice is null ? string.Empty : ReadString(notice, "publicTime")
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

    private static string ToPlainText(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return "-";

        var element = (HTMLElement)Document.CreateElement("div", (DocumentCreateElementOptions?)null);
        element.InnerHTML = html;
        var text = element.InnerText.Trim();
        return text.Length == 0 ? "-" : text;
    }
}

public sealed record ReceivedNoticePageInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string title { get; init; } = string.Empty;
    public int? type { get; init; }
}

public sealed record ReceivedNoticeIdInput
{
    // Snowflake IDs cross the browser boundary as strings to avoid JavaScript precision loss.
    public string id { get; init; } = string.Empty;
}
