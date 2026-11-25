using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.SystemUpdate;

/// <summary>Typed browser boundary for system update management.</summary>
[ECMAScriptModule("components/system-update-client.js")]
public static class SystemUpdateClient
{
    public static IPromise<ApiOutcome> Backups()
        => BrowserApiClient.Post("/api/sysUpdate/list");

    public static IPromise<ApiOutcome> Logs()
        => BrowserApiClient.Get("/api/sysUpdate/logs");

    public static IPromise<ApiOutcome> Update()
        => BrowserApiClient.Post("/api/sysUpdate/update");

    public static IPromise<ApiOutcome> Restore(string fileName)
        => BrowserApiClient.Post("/api/sysUpdate/restore", new SystemRestoreInput { fileName = fileName });

    public static IPromise<ApiOutcome> ClearLogs()
        => BrowserApiClient.Get("/api/sysUpdate/clear");

    public static IPromise<ApiOutcome> WebHookKey()
        => BrowserApiClient.Get("/api/sysUpdate/webHookKey");

    public static SystemBackupView[] ReadBackups(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new SystemBackupView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new SystemBackupView
            {
                fileName = ReadString(value, "fileName"),
                createTime = ReadString(value, "createTime")
            };
        }
        return result;
    }

    public static string[] ReadLogs(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new string[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = values[index] is null ? string.Empty : StringValue(values[index]);
        return result;
    }

    public static string ReadText(object? data)
    {
        if (data is null)
            return string.Empty;
        var result = StringValue(data);
        return result == "null" || result == "undefined" ? string.Empty : result;
    }

    private static string ReadString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        if (value is null)
            return string.Empty;
        var result = StringValue(value);
        return result == "null" || result == "undefined" ? string.Empty : result;
    }
}

public sealed record SystemRestoreInput
{
    public string fileName { get; init; } = string.Empty;
}

public sealed partial record SystemBackupView : VueProps;
