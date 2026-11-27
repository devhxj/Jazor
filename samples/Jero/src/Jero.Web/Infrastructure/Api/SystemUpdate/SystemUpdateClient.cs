using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.SystemUpdate;

/// <summary>提交系统更新和备份恢复请求，读取或清理更新日志。</summary>
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
        => BrowserApiClient.Post("/api/sysUpdate/restore", new SystemRestoreInput { FileName = fileName });

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
                FileName = ReadString(value, "fileName"),
                CreateTime = ReadString(value, "createTime")
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
    [ECMAScriptName("fileName")]
    public string FileName { get; init; } = string.Empty;
}
