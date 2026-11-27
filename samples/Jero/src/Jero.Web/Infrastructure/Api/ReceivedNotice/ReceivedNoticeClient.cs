using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.ReceivedNotice;

/// <summary>分页查询当前用户收到的通知，并提交已读状态。</summary>
[ECMAScriptModule("components/received-notice-client.js")]
public static class ReceivedNoticeClient
{
    public static IPromise<ApiOutcome> Page(ReceivedNoticePageInput input)
        => BrowserApiClient.Post("/api/sysNotice/pageReceived", input);

    public static IPromise<ApiOutcome> Types()
        => BrowserApiClient.Get("/api/sysDictType/dataList?code=NoticeTypeEnum");

    public static IPromise<ApiOutcome> SetRead(string noticeId)
        => BrowserApiClient.Post("/api/sysNotice/setRead", new ReceivedNoticeIdInput { Id = noticeId });

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
                Value = value,
                Label = label.Length == 0 ? "类型" + value : label,
                TagType = ReadString(item, "tagType")
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
            Id = ReadString(value, "id"),
            NoticeId = noticeId,
            UserId = ReadString(value, "userId"),
            ReadStatus = ReadInt(value, "readStatus", 0),
            ReadTime = ReadString(value, "readTime"),
            Title = notice is null ? string.Empty : ReadString(notice, "title"),
            Content = content,
            ContentSummary = ToPlainText(content),
            Type = notice is null ? 0 : ReadInt(notice, "type", 0),
            PublicUserName = notice is null ? string.Empty : ReadString(notice, "publicUserName"),
            PublicTime = notice is null ? string.Empty : ReadString(notice, "publicTime")
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
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("title")]
    public string Title { get; init; } = string.Empty;
    [ECMAScriptName("type")]
    public int? Type { get; init; }
}

public sealed record ReceivedNoticeIdInput
{
    // 雪花 ID 在浏览器侧保留为字符串，避免转换为 JavaScript 数值时丢失精度。
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}
