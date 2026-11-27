using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Cache;

/// <summary>查看缓存键和值，支持按键、前缀或全量清理。</summary>
[ECMAScriptModule("components/cache-client.js")]
public static class CacheClient
{
    public static IPromise<ApiOutcome> Keys()
        => BrowserApiClient.Get("/api/sysCache/keyList");

    public static IPromise<ApiOutcome> Value(string key)
        => BrowserApiClient.Get("/api/sysCache/value/" + EncodeURIComponent(key));

    public static IPromise<ApiOutcome> Delete(string key)
        => BrowserApiClient.Post("/api/sysCache/delete/" + EncodeURIComponent(key));

    public static IPromise<ApiOutcome> DeletePrefix(string prefixKey)
        => BrowserApiClient.Post("/api/sysCache/deleteByPreKey/" + EncodeURIComponent(prefixKey));

    public static IPromise<ApiOutcome> Clear()
        => BrowserApiClient.Post("/api/sysCache/clear");

    public static string[] ReadKeys(object? data)
    {
        var values = data as object?[] ?? [];
        var validCount = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var key = values[index] is null ? string.Empty : StringValue(values[index]).Trim();
            if (key.Length > 0)
                validCount++;
        }

        var result = new string[validCount];
        var target = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var key = values[index] is null ? string.Empty : StringValue(values[index]).Trim();
            if (key.Length == 0)
                continue;
            result[target] = key;
            target++;
        }
        return result;
    }

    public static CacheValuePreview DescribeValue(object? value)
    {
        if (value is null)
            return new CacheValuePreview { TypeLabel = "null", DisplayText = "null" };

        if (value is object?[] values)
            return new CacheValuePreview
            {
                TypeLabel = "array",
                SizeLabel = values.Length + " 项",
                DisplayText = JSON.Stringify(value) ?? "[]"
            };

        if (value is string text)
        {
            var normalized = text.Trim();
            if (LooksLikeJson(normalized))
            {
                try
                {
                    var parsed = JSON.Parse(normalized);
                    return new CacheValuePreview
                    {
                        TypeLabel = normalized.StartsWith('[') ? "json-array" : "json-object",
                        DisplayText = JSON.Stringify(parsed) ?? text
                    };
                }
                catch
                {
                    // 结构化解析失败时，保留原始缓存文本。
                }
            }

            return new CacheValuePreview
            {
                TypeLabel = "string",
                SizeLabel = text.Length + " 字符",
                DisplayText = text
            };
        }

        var json = JSON.Stringify(value);
        return new CacheValuePreview
        {
            TypeLabel = "value",
            DisplayText = json ?? StringValue(value)
        };
    }

    private static bool LooksLikeJson(string value)
        => value.Length >= 2 &&
           ((value.StartsWith('{') && value.EndsWith('}')) ||
            (value.StartsWith('[') && value.EndsWith(']')));
}
