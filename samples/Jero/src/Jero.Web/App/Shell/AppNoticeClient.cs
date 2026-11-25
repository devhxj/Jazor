using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Shell;

/// <summary>Loads the current user's unread notices for the persistent application shell.</summary>
[ECMAScriptModule("components/app-notice-client.js")]
public static class AppNoticeClient
{
    public static IPromise<ApiOutcome> UnreadList()
        => BrowserApiClient.Get("/api/sysNotice/unReadList");

    public static IPromise<ApiOutcome> SetRead(string id)
        => BrowserApiClient.Post("/api/sysNotice/setRead", new AppNoticeIdInput { id = id });

    public static AppUnreadNotice[] ReadUnread(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new AppUnreadNotice[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var item = values[index]!;
            result[index] = new AppUnreadNotice
            {
                id = ReadString(item, "id"),
                title = ReadString(item, "title"),
                content = ReadString(item, "content"),
                type = ReadInt(item, "type"),
                publicUserName = ReadString(item, "publicUserName"),
                publicTime = ReadString(item, "publicTime")
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
    public string id { get; init; } = string.Empty;
}

public sealed partial record AppUnreadNotice : VueProps;
