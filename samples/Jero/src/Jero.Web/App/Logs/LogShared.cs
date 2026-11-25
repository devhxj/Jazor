using Jero.Web.App.Auth;
using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Logs;

/// <summary>Shared browser contracts used by the four system log pages.</summary>
[ECMAScriptModule("components/log-shared.js")]
public static class LogShared
{
    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static LogTenantOption[] ReadTenants(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new LogTenantOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new LogTenantOption
            {
                value = ReadString(value, "value"),
                label = ReadString(value, "label"),
                host = ReadString(value, "host")
            };
        }
        return result;
    }

    public static IPromise<LogFileOutcome> Download(string path, LogExportInput input, string fallbackFileName)
    {
        var json = JSON.Stringify(input) ?? "{}";
        var token = BrowserApiClient.AuthToken;
        var headers = string.IsNullOrEmpty(token)
            ? (HeadersInit)new string[][] { new[] { "content-type", "application/json" } }
            : (HeadersInit)new string[][]
            {
                new[] { "content-type", "application/json" },
                new[] { "authorization", "Bearer " + token }
            };
        var init = new RequestInit(
            Method: "POST",
            Headers: headers,
            Body: (BodyInit)(XMLHttpRequestBodyInit)json,
            Credentials: RequestCredentials.SameOrigin);

        return Promise<Response>.Resolve(Window.Fetch(path, init)).Then(response =>
        {
            ApplySessionHeaders(response);
            if (!response.Ok)
            {
                return Promise<LogFileOutcome>.Resolve(new LogFileOutcome
                {
                    Ok = false,
                    Error = "导出失败，状态码 " + response.Status + "。"
                });
            }

            var contentType = response.Headers.Get("content-type") ?? string.Empty;
            if (contentType.IndexOf("application/json") >= 0)
            {
                return Promise<string>.Resolve(response.Text()).Then(text =>
                {
                    var envelope = JSON.Parse(text);
                    var messageValue = envelope is null ? null : Reflect.Get(envelope, "message");
                    var message = messageValue is null ? "导出失败。" : StringValue(messageValue);
                    return new LogFileOutcome { Ok = false, Error = message };
                });
            }

            return Promise<Blob>.Resolve(response.Blob()).Then(blob =>
            {
                var fileName = ResolveFileName(response, fallbackFileName);
                var url = URL.CreateObjectURL(blob);
                var link = (HTMLAnchorElement)Document.CreateElement("a", (DocumentCreateElementOptions?)null);
                link.Href = url;
                link.Download = fileName;
                link.Click();
                URL.RevokeObjectURL(url);
                return new LogFileOutcome { Ok = true, FileName = fileName };
            });
        });
    }

    private static void ApplySessionHeaders(Response response)
    {
        var accessToken = response.Headers.Get("access-token");
        var refreshToken = response.Headers.Get("x-access-token");
        if (accessToken == "invalid_token" || response.Status == 401)
        {
            BrowserSessionStore.Clear();
            return;
        }

        if (!string.IsNullOrEmpty(accessToken))
            BrowserApiClient.AuthToken = accessToken;
        if (!string.IsNullOrEmpty(refreshToken))
            BrowserApiClient.RefreshToken = refreshToken;
    }

    private static string ResolveFileName(Response response, string fallbackFileName)
    {
        var header = response.Headers.Get("content-disposition") ?? string.Empty;
        if (header.Length == 0)
            return fallbackFileName;

        var marker = "filename*=UTF-8''";
        var index = header.IndexOf(marker);
        if (index >= 0)
            return DecodeFileName(header.Substring(index + marker.Length), fallbackFileName);

        marker = "filename=";
        index = header.IndexOf(marker);
        return index < 0
            ? fallbackFileName
            : DecodeFileName(header.Substring(index + marker.Length), fallbackFileName);
    }

    private static string DecodeFileName(string value, string fallbackFileName)
    {
        var separator = value.IndexOf(';');
        var encoded = (separator < 0 ? value : value.Substring(0, separator)).Trim().Replace("\"", string.Empty);
        return encoded.Length == 0 ? fallbackFileName : DecodeURIComponent(encoded);
    }

    private static string ReadString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? string.Empty : StringValue(value);
    }
}

public sealed record LogTenantOption : VueProps
{
    public string value { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string host { get; init; } = string.Empty;
}

public sealed record LogExportInput
{
    public string? startTime { get; init; }
    public string? endTime { get; init; }
}

public sealed record LogFileOutcome
{
    public bool Ok { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string? Error { get; init; }
}
