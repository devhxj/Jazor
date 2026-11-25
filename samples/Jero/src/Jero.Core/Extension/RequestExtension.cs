namespace Jero.Core;

public static class RequestExtension
{
    /// <summary>
    /// 获取请求地址源
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static string GetOrigin(this HttpRequest request)
    {
        string scheme = request.Scheme;
        string host = request.Host.Host;
        int port = request.Host.Port ?? (-1);

        string url = $"{scheme}://{host}";
        if (port != 80 && port != 443 && port != -1) url += $":{port}";

        return url;
    }
}