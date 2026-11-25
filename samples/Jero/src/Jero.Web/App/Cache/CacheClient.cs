using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Cache;

/// <summary>Typed browser boundary for cache inspection and removal.</summary>
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
            return new CacheValuePreview { typeLabel = "null", displayText = "null" };

        if (value is object?[] values)
            return new CacheValuePreview
            {
                typeLabel = "array",
                sizeLabel = values.Length + " 项",
                displayText = JSON.Stringify(value) ?? "[]"
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
                        typeLabel = normalized.StartsWith('[') ? "json-array" : "json-object",
                        displayText = JSON.Stringify(parsed) ?? text
                    };
                }
                catch
                {
                    // Invalid structured strings remain ordinary cache text.
                }
            }

            return new CacheValuePreview
            {
                typeLabel = "string",
                sizeLabel = text.Length + " 字符",
                displayText = text
            };
        }

        var json = JSON.Stringify(value);
        return new CacheValuePreview
        {
            typeLabel = "value",
            displayText = json ?? StringValue(value)
        };
    }

    private static bool LooksLikeJson(string value)
        => value.Length >= 2 &&
           ((value.StartsWith('{') && value.EndsWith('}')) ||
            (value.StartsWith('[') && value.EndsWith(']')));
}
