using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Notice;

/// <summary>维护和发布系统公告，读取公告类型及状态选项。</summary>
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
        => BrowserApiClient.Post("/api/sysNotice/delete", new NoticeIdInput { Id = id });

    public static IPromise<ApiOutcome> Publish(string id)
        => BrowserApiClient.Post("/api/sysNotice/public", new NoticeIdInput { Id = id });

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
                Value = value,
                Label = label.Length == 0 ? "选项" + value : label,
                TagType = ReadString(item, "tagType")
            };
            target++;
        }
        return result;
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

    private static NoticeView ReadItem(object value)
    {
        var content = ReadString(value, "content");
        return new NoticeView
        {
            Id = ReadString(value, "id"),
            Title = ReadString(value, "title"),
            Content = content,
            ContentSummary = ToPlainText(content),
            Type = ReadInt(value, "type", 1),
            Status = ReadInt(value, "status", 0),
            PublicUserId = ReadString(value, "publicUserId"),
            PublicUserName = ReadString(value, "publicUserName"),
            PublicOrgId = ReadString(value, "publicOrgId"),
            PublicOrgName = ReadString(value, "publicOrgName"),
            PublicTime = ReadString(value, "publicTime"),
            CancelTime = ReadString(value, "cancelTime"),
            CreateTime = ReadString(value, "createTime"),
            CreateUserId = ReadString(value, "createUserId"),
            CreateUserName = ReadString(value, "createUserName"),
            UpdateTime = ReadString(value, "updateTime"),
            UpdateUserId = ReadString(value, "updateUserId"),
            UpdateUserName = ReadString(value, "updateUserName")
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
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("title")]
    public string Title { get; init; } = string.Empty;
    [ECMAScriptName("type")]
    public int? Type { get; init; }
}

public sealed record NoticeAddInput
{
    [ECMAScriptName("title")]
    public string Title { get; init; } = string.Empty;
    [ECMAScriptName("content")]
    public string Content { get; init; } = string.Empty;
    [ECMAScriptName("type")]
    public int Type { get; init; } = 1;
    [ECMAScriptName("status")]
    public int Status { get; init; }
}

/// <summary>
/// SysNoticeService 会更新适配后的整条公告记录。提交更新时需保留选中行的审计、发布和创建人字段。
/// </summary>
public sealed record NoticeSaveInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("title")]
    public string Title { get; init; } = string.Empty;
    [ECMAScriptName("content")]
    public string Content { get; init; } = string.Empty;
    [ECMAScriptName("type")]
    public int Type { get; init; } = 1;
    [ECMAScriptName("status")]
    public int Status { get; init; }
    [ECMAScriptName("publicUserId")]
    public string PublicUserId { get; init; } = "0";
    [ECMAScriptName("publicUserName")]
    public string? PublicUserName { get; init; }
    [ECMAScriptName("publicOrgId")]
    public string PublicOrgId { get; init; } = "0";
    [ECMAScriptName("publicOrgName")]
    public string? PublicOrgName { get; init; }
    [ECMAScriptName("publicTime")]
    public string? PublicTime { get; init; }
    [ECMAScriptName("cancelTime")]
    public string? CancelTime { get; init; }
    [ECMAScriptName("createTime")]
    public string CreateTime { get; init; } = string.Empty;
    [ECMAScriptName("createUserId")]
    public string CreateUserId { get; init; } = string.Empty;
    [ECMAScriptName("createUserName")]
    public string? CreateUserName { get; init; }
    [ECMAScriptName("updateTime")]
    public string? UpdateTime { get; init; }
    [ECMAScriptName("updateUserId")]
    public string? UpdateUserId { get; init; }
    [ECMAScriptName("updateUserName")]
    public string? UpdateUserName { get; init; }
}

public sealed record NoticeIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record NoticeView : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string ContentSummary { get; init; } = "-";
    public int Type { get; init; }
    public int Status { get; init; }
    public string PublicUserId { get; init; } = string.Empty;
    public string PublicUserName { get; init; } = string.Empty;
    public string PublicOrgId { get; init; } = string.Empty;
    public string PublicOrgName { get; init; } = string.Empty;
    public string PublicTime { get; init; } = string.Empty;
    public string CancelTime { get; init; } = string.Empty;
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
}

public sealed record NoticeOption : VueProps
{
    public int Value { get; init; }
    public string Label { get; init; } = string.Empty;
    public string TagType { get; init; } = string.Empty;
}
