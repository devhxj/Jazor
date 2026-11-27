using Jero.Web.Infrastructure.Storage;
using Jero.Web.Pages.Auth;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Profile;

/// <summary>头像与电子签名的原生 multipart 浏览器边界。</summary>
[ECMAScriptModule("components/profile-browser-actions.js")]
public static class ProfileBrowserActions
{
    public static void ClearSelectedFile(string elementId)
    {
        var input = Document.GetElementById(elementId) as HTMLInputElement;
        if (input is not null)
            input.Value = string.Empty;
    }

    public static IPromise<ProfileUploadOutcome> UploadSelectedImage(FileRef file, string path)
    {
        var form = new FormData();
        form.Append("file", file, file.Name);

        // 浏览器必须自行生成 multipart boundary，手动设置 Content-Type 会破坏 IFormFile 绑定。
        return Promise<Response>.Resolve(Window.Fetch(path, CreateFormPost(form)))
            .Then(ReadResponse);
    }

    private static RequestInit CreateFormPost(FormData form)
    {
        var body = (BodyInit)(XMLHttpRequestBodyInit)form;
        var token = BrowserApiClient.AuthToken;
        return string.IsNullOrEmpty(token)
            ? new RequestInit(Method: "POST", Body: body, Credentials: RequestCredentials.SameOrigin)
            : new RequestInit(
                Method: "POST",
                Headers: (HeadersInit)new string[][] { new[] { "authorization", "Bearer " + token } },
                Body: body,
                Credentials: RequestCredentials.SameOrigin);
    }

    private static IPromise<ProfileUploadOutcome> ReadResponse(Response response)
    {
        ApplySessionHeaders(response);
        return Promise<string>.Resolve(response.Text()).Then(text =>
        {
            var envelope = JSON.Parse(text);
            if (envelope is null)
            {
                return new ProfileUploadOutcome
                {
                    Ok = false,
                    Error = "服务器返回了无效响应。"
                };
            }

            var codeValue = Reflect.Get(envelope, "code");
            var code = codeValue is null ? response.Status : (int)NumberValue(codeValue);
            var messageValue = Reflect.Get(envelope, "message");
            var message = messageValue is null ? string.Empty : StringValue(messageValue);
            if (!response.Ok || code != 200)
            {
                return new ProfileUploadOutcome
                {
                    Ok = false,
                    Error = message.Length == 0 ? "上传失败，状态码 " + code + "。" : message
                };
            }

            var result = Reflect.Get(envelope, "result");
            var id = ReadString(result, "id");
            var bucketName = ReadString(result, "bucketName");
            var filePath = ReadString(result, "filePath");
            var suffix = ReadString(result, "suffix");
            var rawUrl = ReadString(result, "url");
            return new ProfileUploadOutcome
            {
                Ok = true,
                Id = id,
                Url = ResolveImageUrl(bucketName, filePath, id, suffix, rawUrl),
                RawUrl = rawUrl,
                FileName = ReadString(result, "fileName"),
                Suffix = suffix,
                BucketName = bucketName,
                FilePath = filePath
            };
        });
    }

    private static string ResolveImageUrl(
        string bucketName,
        string filePath,
        string id,
        string suffix,
        string url)
    {
        if (bucketName == "Local" && filePath.Length > 0 && id.Length > 0 && suffix.Length > 0)
        {
            var normalizedPath = filePath.Trim().Replace("\\", "/");
            while (normalizedPath.StartsWith('/'))
                normalizedPath = normalizedPath.Substring(1);
            while (normalizedPath.EndsWith('/'))
                normalizedPath = normalizedPath.Substring(0, normalizedPath.Length - 1);
            return "/" + normalizedPath + "/" + id + suffix;
        }

        if (url.Length == 0 || url.StartsWith('/') || url.StartsWith("http://") || url.StartsWith("https://"))
            return url;
        return "/" + url;
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

    private static string ReadString(object? data, string key)
    {
        if (data is null)
            return string.Empty;
        var value = Reflect.Get(data, key);
        if (value is null)
            return string.Empty;
        var text = StringValue(value);
        return text == "null" || text == "undefined" ? string.Empty : text;
    }
}

public sealed record ProfileUploadOutcome
{
    public bool Ok { get; init; }
    public string Id { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string RawUrl { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string Suffix { get; init; } = string.Empty;
    public string BucketName { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public string? Error { get; init; }
}
