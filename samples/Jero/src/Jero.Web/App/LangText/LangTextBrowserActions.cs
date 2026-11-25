using Jero.Web.App.Auth;
using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.LangText;

/// <summary>Typed WebIDL boundary for translation Excel upload and download flows.</summary>
[ECMAScriptModule("components/lang-text-browser-actions.js")]
public static class LangTextBrowserActions
{
    private static string importResultUrl = string.Empty;
    private static string importResultFileName = string.Empty;

    public static FileRef? GetSelectedFile(string elementId)
    {
        var input = Document.GetElementById(elementId) as HTMLInputElement;
        return input?.Files?.GetItem(0);
    }

    public static void ClearSelectedFile(string elementId)
    {
        var input = Document.GetElementById(elementId) as HTMLInputElement;
        if (input is not null)
            input.Value = string.Empty;
    }

    public static IPromise<LangTextFileOutcome> DownloadExport(LangTextPageInput input, string fallbackFileName)
        => SendDownload("/api/sysLangText/export", CreateJsonPost(input), fallbackFileName);

    public static IPromise<LangTextFileOutcome> DownloadTemplate(string fallbackFileName)
        => SendDownload("/api/sysLangText/import", CreateGet(), fallbackFileName);

    public static IPromise<LangTextFileOutcome> ImportSelectedFile(FileRef file)
    {
        var form = new FormData();
        form.Append("file", file, file.Name);
        var init = CreateFormPost(form);
        return Promise<Response>.Resolve(Window.Fetch("/api/sysLangText/import", init)).Then(ReadImportResponse);
    }

    public static void DownloadImportResult()
    {
        if (importResultUrl.Length == 0)
            return;

        var link = (HTMLAnchorElement)Document.CreateElement("a", (DocumentCreateElementOptions?)null);
        link.Href = importResultUrl;
        link.Download = importResultFileName.Length == 0 ? "翻译表导入结果.xlsx" : importResultFileName;
        link.Click();
    }

    public static void ResetImportResult()
    {
        if (importResultUrl.Length > 0)
            URL.RevokeObjectURL(importResultUrl);
        importResultUrl = string.Empty;
        importResultFileName = string.Empty;
    }

    private static IPromise<LangTextFileOutcome> SendDownload(string path, RequestInit init, string fallbackFileName)
        => Promise<Response>.Resolve(Window.Fetch(path, init)).Then(response =>
        {
            ApplySessionHeaders(response);
            if (IsJson(response))
                return Promise<string>.Resolve(response.Text()).Then(text => ReadJsonOutcome(response, text));

            if (!response.Ok)
            {
                return Promise<LangTextFileOutcome>.Resolve(new LangTextFileOutcome
                {
                    Ok = false,
                    Error = "下载失败，状态码 " + response.Status + "。"
                });
            }

            return Promise<Blob>.Resolve(response.Blob()).Then(blob =>
            {
                DownloadBlob(blob, ResolveFileName(response, fallbackFileName));
                return new LangTextFileOutcome { Ok = true };
            });
        });

    private static IPromise<LangTextFileOutcome> ReadImportResponse(Response response)
    {
        ApplySessionHeaders(response);
        if (IsJson(response))
            return Promise<string>.Resolve(response.Text()).Then(text => ReadJsonOutcome(response, text));

        if (!response.Ok)
        {
            return Promise<LangTextFileOutcome>.Resolve(new LangTextFileOutcome
            {
                Ok = false,
                Error = "导入失败，状态码 " + response.Status + "。"
            });
        }

        return Promise<Blob>.Resolve(response.Blob()).Then(blob =>
        {
            ResetImportResult();
            importResultUrl = URL.CreateObjectURL(blob);
            importResultFileName = ResolveFileName(response, "翻译表导入结果.xlsx");
            return new LangTextFileOutcome
            {
                Ok = true,
                HasErrorFile = true,
                Message = "导入完成，但存在错误记录。",
                FileName = importResultFileName
            };
        });
    }

    private static LangTextFileOutcome ReadJsonOutcome(Response response, string text)
    {
        var envelope = JSON.Parse(text);
        if (envelope is null)
        {
            return new LangTextFileOutcome
            {
                Ok = false,
                Error = "服务器返回了无效响应。"
            };
        }

        var codeValue = Reflect.Get(envelope, "code");
        var code = codeValue is null ? response.Status : (int)NumberValue(codeValue);
        var messageValue = Reflect.Get(envelope, "message");
        var message = messageValue is null ? string.Empty : StringValue(messageValue);
        var ok = response.Ok && code == 200;
        return new LangTextFileOutcome
        {
            Ok = ok,
            Message = ok ? (message.Length == 0 ? "操作成功。" : message) : string.Empty,
            Error = ok ? null : (message.Length == 0 ? "请求失败，状态码 " + code + "。" : message)
        };
    }

    private static bool IsJson(Response response)
        => (response.Headers.Get("content-type") ?? string.Empty).IndexOf("application/json") >= 0;

    private static RequestInit CreateGet()
    {
        var token = BrowserApiClient.AuthToken;
        return string.IsNullOrEmpty(token)
            ? new RequestInit(Method: "GET", Credentials: RequestCredentials.SameOrigin)
            : new RequestInit(Method: "GET", Headers: AuthHeaders(token!), Credentials: RequestCredentials.SameOrigin);
    }

    private static RequestInit CreateJsonPost(object body)
    {
        var json = JSON.Stringify(body) ?? "{}";
        var token = BrowserApiClient.AuthToken;
        return string.IsNullOrEmpty(token)
            ? new RequestInit(
                Method: "POST",
                Headers: JsonHeaders(),
                Body: (BodyInit)(XMLHttpRequestBodyInit)json,
                Credentials: RequestCredentials.SameOrigin)
            : new RequestInit(
                Method: "POST",
                Headers: JsonAndAuthHeaders(token!),
                Body: (BodyInit)(XMLHttpRequestBodyInit)json,
                Credentials: RequestCredentials.SameOrigin);
    }

    private static RequestInit CreateFormPost(FormData form)
    {
        var body = (BodyInit)(XMLHttpRequestBodyInit)form;
        var token = BrowserApiClient.AuthToken;
        return string.IsNullOrEmpty(token)
            ? new RequestInit(Method: "POST", Body: body, Credentials: RequestCredentials.SameOrigin)
            : new RequestInit(
                Method: "POST",
                Headers: AuthHeaders(token!),
                Body: body,
                Credentials: RequestCredentials.SameOrigin);
    }

    private static HeadersInit AuthHeaders(string token)
        => (HeadersInit)new string[][] { new[] { "authorization", "Bearer " + token } };

    private static HeadersInit JsonHeaders()
        => (HeadersInit)new string[][] { new[] { "content-type", "application/json" } };

    private static HeadersInit JsonAndAuthHeaders(string token)
        => (HeadersInit)new string[][]
        {
            new[] { "content-type", "application/json" },
            new[] { "authorization", "Bearer " + token }
        };

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
        if (encoded.Length == 0)
            return fallbackFileName;
        return DecodeURIComponent(encoded);
    }

    private static void DownloadBlob(Blob blob, string fileName)
    {
        var url = URL.CreateObjectURL(blob);
        var link = (HTMLAnchorElement)Document.CreateElement("a", (DocumentCreateElementOptions?)null);
        link.Href = url;
        link.Download = fileName;
        link.Click();
        URL.RevokeObjectURL(url);
    }
}

public sealed record LangTextFileOutcome
{
    public bool Ok { get; init; }
    public bool HasErrorFile { get; init; }
    public string Message { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string? Error { get; init; }
}
