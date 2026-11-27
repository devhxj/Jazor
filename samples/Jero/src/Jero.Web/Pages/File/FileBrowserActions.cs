using Jero.Web.Infrastructure.Api.File;
using Jero.Web.Infrastructure.Storage;
using Jero.Web.Pages.Auth;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Pages.File;

/// <summary>用浏览器的 FormData 上传文件，携带会话令牌读取下载流和预览内容。</summary>
[ECMAScriptModule("components/file-browser-actions.js")]
public static class FileBrowserActions
{
    public static void ClearSelectedFile(string elementId)
    {
        var input = Document.GetElementById(elementId) as HTMLInputElement;
        if (input is not null)
            input.Value = string.Empty;
    }

    public static IPromise<FileTransferOutcome> UploadSelectedFile(
        FileRef file,
        string filePath,
        string fileType,
        bool isPublic)
    {
        var form = new FormData();
        form.Append("File", file, file.Name);
        if (filePath.Length > 0)
            form.Append("FilePath", filePath);
        if (fileType.Length > 0)
            form.Append("FileType", fileType);
        form.Append("IsPublic", isPublic ? "true" : "false");

        // fetch 会根据 FormData 生成分隔符，并写入 Content-Type；服务端按这一对值解析表单。
        return Promise<Response>.Resolve(Window.Fetch("/api/sysFile/uploadFile", CreateFormPost(form)))
            .Then(ReadJsonResponse);
    }

    public static IPromise<FileTransferOutcome> Download(string id, string fallbackFileName)
        => SendBinary(
            "/api/sysFile/downloadFile",
            CreateJsonPost(new FileIdInput { Id = id }),
            fallbackFileName,
            true);

    public static IPromise<FileTransferOutcome> Preview(string id)
        => SendBinary(
            "/api/sysFile/preview/" + EncodeURIComponent(id),
            CreateGet(),
            string.Empty,
            false);

    public static string CreatePreviewUrl(Blob blob)
        => URL.CreateObjectURL(blob);

    public static void RevokePreviewUrl(string url)
    {
        // 对象 URL 持有 Blob；关闭预览或离开路由时释放这段文件数据。
        if (url.Length > 0)
            URL.RevokeObjectURL(url);
    }

    private static IPromise<FileTransferOutcome> SendBinary(
        string path,
        RequestInit init,
        string fallbackFileName,
        bool download)
        => Promise<Response>.Resolve(Window.Fetch(path, init)).Then(response =>
        {
            ApplySessionHeaders(response);
            if (IsJson(response))
                return Promise<string>.Resolve(response.Text()).Then(text => ReadJsonOutcome(response, text));

            if (!response.Ok)
            {
                return Promise<FileTransferOutcome>.Resolve(new FileTransferOutcome
                {
                    Ok = false,
                    Error = "文件请求失败，状态码 " + response.Status + "。"
                });
            }

            return Promise<Blob>.Resolve(response.Blob()).Then(blob =>
            {
                var fileName = ResolveFileName(response, fallbackFileName);
                if (download)
                    DownloadBlob(blob, fileName);
                return new FileTransferOutcome
                {
                    Ok = true,
                    Blob = download ? null : blob,
                    FileName = fileName
                };
            });
        });

    private static IPromise<FileTransferOutcome> ReadJsonResponse(Response response)
    {
        ApplySessionHeaders(response);
        return Promise<string>.Resolve(response.Text()).Then(text => ReadJsonOutcome(response, text));
    }

    private static FileTransferOutcome ReadJsonOutcome(Response response, string text)
    {
        var envelope = JSON.Parse(text);
        if (envelope is null)
        {
            return new FileTransferOutcome
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
        return new FileTransferOutcome
        {
            Ok = ok,
            Data = ok ? Reflect.Get(envelope, "result") : null,
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
        // 下载链接触发后即可释放对象 URL，避免反复下载时一直占用文件缓冲区。
        var url = URL.CreateObjectURL(blob);
        var link = (HTMLAnchorElement)Document.CreateElement("a", (DocumentCreateElementOptions?)null);
        link.Href = url;
        link.Download = fileName.Length == 0 ? "download" : fileName;
        link.Click();
        URL.RevokeObjectURL(url);
    }
}

public sealed record FileTransferOutcome
{
    public bool Ok { get; init; }
    public object? Data { get; init; }
    public Blob? Blob { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string? Error { get; init; }
}
