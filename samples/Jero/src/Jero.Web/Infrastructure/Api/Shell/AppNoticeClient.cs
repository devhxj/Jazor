using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Shell;

/// <summary>读取应用外壳显示的未读通知，并标记已读。</summary>
[ECMAScriptModule("components/app-notice-client.js")]
public static class AppNoticeClient
{
    public static IPromise<ApiOutcome> UnreadList()
        => BrowserApiClient.Get("/api/sysNotice/unReadList");

    public static IPromise<ApiOutcome> SetRead(string id)
        => BrowserApiClient.Post("/api/sysNotice/setRead", new AppNoticeIdInput { Id = id });

    public static AppUnreadNotice[] ReadUnread(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new AppUnreadNotice[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var item = values[index]!;
            result[index] = new AppUnreadNotice
            {
                Id = ReadString(item, "id"),
                Title = ReadString(item, "title"),
                Content = ReadString(item, "content"),
                Type = ReadInt(item, "type"),
                PublicUserName = ReadString(item, "publicUserName"),
                PublicTime = ReadString(item, "publicTime")
            };
        }
        return result;
    }

    private static string ReadString(object value, string key)
    {
        var field = Reflect.Get(value, key);
        if (field is null)
            return string.Empty;
        var result = StringValue(field);
        return result == "null" || result == "undefined" ? string.Empty : result;
    }

    private static int ReadInt(object value, string key)
    {
        var field = Reflect.Get(value, key);
        return field is null ? 0 : (int)NumberValue(field);
    }
}

public sealed record AppNoticeIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}
