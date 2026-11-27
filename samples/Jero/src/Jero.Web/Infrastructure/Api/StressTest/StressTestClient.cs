using Jero.Web.Application;
using Jero.Web.Infrastructure.Storage;
using static ECMAScript.Global;
using Jero.Web.Infrastructure.Http;

namespace Jero.Web.Infrastructure.Api.StressTest;

/// <summary>发现 Swagger 接口、读取接口文档并提交压测请求。</summary>
[ECMAScriptModule("components/stress-test-client.js")]
public static class StressTestClient
{
    public static IPromise<ApiOutcome> Resources()
        => GetRaw("/swagger/swagger-resources");

    public static IPromise<ApiOutcome> Document(string url)
        => GetRaw(url);

    public static IPromise<ApiOutcome> Run(StressTestInput input)
        => BrowserApiClient.Post("/api/sysCommon/stressTest", input);

    public static string ResolveRequestUri(string path)
    {
        var normalized = path.Trim();
        if (normalized.Length == 0 || normalized.StartsWith("http://") || normalized.StartsWith("https://"))
            return normalized;
        return Window.Origin + (normalized.StartsWith('/') ? normalized : "/" + normalized);
    }

    public static StressSwaggerResource[] ReadResources(object? data)
    {
        var values = data as object?[] ?? [];
        var result = Array.Empty<StressSwaggerResource>();
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index];
            if (value is null)
                continue;
            var url = NormalizeSwaggerUrl(ReadString(value, "url"));
            var name = ReadString(value, "name");
            if (url.Length == 0 || url.ToLower().Contains("all%20groups") || name.ToLower().Contains("all groups"))
                continue;
            result = Append(result, new StressSwaggerResource
            {
                Name = name.Length == 0 ? url : DecodeURIComponent(name),
                Url = url
            });
        }
        return result;
    }

    public static StressApiGroup[] ReadGroups(object? data, string keywords)
    {
        if (data is null)
            return [];

        var paths = Reflect.Get(data, "paths");
        if (paths is null)
            return [];

        var nodes = Array.Empty<StressApiNode>();
        var pathKeys = Reflect.OwnKeys(paths);
        for (var pathIndex = 0; pathIndex < pathKeys.Length; pathIndex++)
        {
            var path = StringValue(pathKeys[pathIndex]);
            var operations = Reflect.Get(paths, pathKeys[pathIndex]);
            if (operations is null)
                continue;

            var methodKeys = Reflect.OwnKeys(operations);
            for (var methodIndex = 0; methodIndex < methodKeys.Length; methodIndex++)
            {
                var method = StringValue(methodKeys[methodIndex]).ToUpper();
                if (method != "GET" && method != "POST" && method != "PUT" && method != "DELETE")
                    continue;
                var operation = Reflect.Get(operations, methodKeys[methodIndex]);
                if (operation is null)
                    continue;

                var summary = ReadString(operation, "summary");
                var tagName = ReadFirstString(Reflect.Get(operation, "tags"));
                if (summary.Length == 0)
                    summary = path;
                if (tagName.Length == 0)
                    tagName = "未分组接口";

                var searchable = (summary + " " + path + " " + method + " " + tagName).ToLower();
                var keyword = keywords.Trim().ToLower();
                if (keyword.Length > 0 && !searchable.Contains(keyword))
                    continue;

                nodes = Append(nodes, new StressApiNode
                {
                    Id = method + ":" + path,
                    Method = method,
                    Path = path,
                    Summary = summary,
                    TagName = tagName,
                    Headers = ExtractParameterRows(operation, "header"),
                    PathParameters = ExtractParameterRows(operation, "path"),
                    QueryParameters = ExtractParameterRows(operation, "query"),
                    RequestParameters = ExtractBodyRows(operation)
                });
            }
        }

        var groups = Array.Empty<StressApiGroup>();
        for (var index = 0; index < nodes.Length; index++)
        {
            var groupIndex = FindGroup(groups, nodes[index].TagName);
            if (groupIndex < 0)
            {
                groups = Append(groups, new StressApiGroup
                {
                    Name = nodes[index].TagName,
                    Summary = ReadTagDescription(data, nodes[index].TagName),
                    Children = [nodes[index]]
                });
                continue;
            }

            groups[groupIndex].Children = Append(groups[groupIndex].Children, nodes[index]);
        }
        return groups;
    }

    public static StressTestResult ReadResult(object? data)
    {
        if (data is null)
            return new StressTestResult();
        return new StressTestResult
        {
            TotalRequests = ReadString(data, "totalRequests"),
            SuccessfulRequests = ReadString(data, "successfulRequests"),
            FailedRequests = ReadString(data, "failedRequests"),
            TotalTimeInSeconds = ReadNumber(data, "totalTimeInSeconds"),
            QueriesPerSecond = ReadNumber(data, "queriesPerSecond"),
            MinResponseTime = ReadNumber(data, "minResponseTime"),
            MaxResponseTime = ReadNumber(data, "maxResponseTime"),
            AverageResponseTime = ReadNumber(data, "averageResponseTime"),
            Percentile10ResponseTime = ReadNumber(data, "percentile10ResponseTime"),
            Percentile25ResponseTime = ReadNumber(data, "percentile25ResponseTime"),
            Percentile50ResponseTime = ReadNumber(data, "percentile50ResponseTime"),
            Percentile75ResponseTime = ReadNumber(data, "percentile75ResponseTime"),
            Percentile90ResponseTime = ReadNumber(data, "percentile90ResponseTime"),
            Percentile99ResponseTime = ReadNumber(data, "percentile99ResponseTime"),
            Percentile999ResponseTime = ReadNumber(data, "percentile999ResponseTime")
        };
    }

    private static IPromise<ApiOutcome> GetRaw(string path)
    {
        var accessToken = BrowserSessionStore.AccessToken;
        var refreshToken = BrowserSessionStore.RefreshToken;
        RequestInit init;
        if (string.IsNullOrEmpty(accessToken) && string.IsNullOrEmpty(refreshToken))
        {
            init = new RequestInit(Method: "GET", Credentials: RequestCredentials.SameOrigin);
        }
        else if (string.IsNullOrEmpty(refreshToken))
        {
            var headers = new string[][] { new[] { "authorization", "Bearer " + accessToken } };
            init = new RequestInit(Method: "GET", Headers: (HeadersInit)headers, Credentials: RequestCredentials.SameOrigin);
        }
        else if (string.IsNullOrEmpty(accessToken))
        {
            var headers = new string[][] { new[] { "x-authorization", "Bearer " + refreshToken } };
            init = new RequestInit(Method: "GET", Headers: (HeadersInit)headers, Credentials: RequestCredentials.SameOrigin);
        }
        else
        {
            var headers = new string[][]
            {
                new[] { "authorization", "Bearer " + accessToken },
                new[] { "x-authorization", "Bearer " + refreshToken }
            };
            init = new RequestInit(Method: "GET", Headers: (HeadersInit)headers, Credentials: RequestCredentials.SameOrigin);
        }

        return Promise<Response>.Resolve(Window.Fetch(path, init)).Then(response =>
            Promise<object>.Resolve(response.Json()).Then(payload =>
            {
                var result = Reflect.Get(payload, "result");
                var message = ReadString(payload, "message");
                return new ApiOutcome
                {
                    Ok = response.Ok,
                    Code = response.Status,
                    Data = result is null ? payload : result,
                    Error = response.Ok ? null : (message.Length == 0 ? "加载 Swagger 数据失败。" : message)
                };
            }));
    }

    private static string NormalizeSwaggerUrl(string value)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0 || normalized.StartsWith("http://") || normalized.StartsWith("https://") || normalized.StartsWith('/'))
            return normalized;
        return "/" + normalized;
    }

    private static StressPair[] ExtractParameterRows(object operation, string location)
    {
        var values = Reflect.Get(operation, "parameters") as object?[] ?? [];
        var result = Array.Empty<StressPair>();
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index];
            if (value is null || ReadString(value, "in").ToLower() != location)
                continue;
            var name = ReadString(value, "name").Trim();
            if (name.Length == 0)
                continue;
            result = Append(result, new StressPair { Key = name, Value = ReadParameterValue(value) });
        }
        return result;
    }

    private static StressPair[] ExtractBodyRows(object operation)
    {
        var requestBody = Reflect.Get(operation, "requestBody");
        var content = requestBody is null ? null : Reflect.Get(requestBody, "content");
        if (content is null)
            return [];

        var schema = ReadSchema(content, "application/json") ??
                     ReadSchema(content, "application/x-www-form-urlencoded") ??
                     ReadSchema(content, "multipart/form-data");
        var properties = schema is null ? null : Reflect.Get(schema, "properties");
        if (properties is null)
            return [];

        var keys = Reflect.OwnKeys(properties);
        var result = new StressPair[keys.Length];
        for (var index = 0; index < keys.Length; index++)
            result[index] = new StressPair { Key = StringValue(keys[index]), Value = string.Empty };
        return result;
    }

    private static object? ReadSchema(object content, string contentType)
    {
        var item = Reflect.Get(content, contentType);
        return item is null ? null : Reflect.Get(item, "schema");
    }

    private static string ReadParameterValue(object parameter)
    {
        var example = Reflect.Get(parameter, "example");
        if (example is not null)
            return StringValue(example);
        var schema = Reflect.Get(parameter, "schema");
        if (schema is null)
            return string.Empty;
        example = Reflect.Get(schema, "example");
        if (example is not null)
            return StringValue(example);
        var fallback = Reflect.Get(schema, "default");
        return fallback is null ? string.Empty : StringValue(fallback);
    }

    private static string ReadTagDescription(object document, string name)
    {
        var tags = Reflect.Get(document, "tags") as object?[] ?? [];
        for (var index = 0; index < tags.Length; index++)
        {
            var tag = tags[index];
            if (tag is not null && ReadString(tag, "name") == name)
            {
                var description = ReadString(tag, "description");
                return description.Length == 0 ? name : description;
            }
        }
        return name;
    }

    private static string ReadFirstString(object? value)
    {
        var values = value as object?[] ?? [];
        return values.Length == 0 || values[0] is null ? string.Empty : StringValue(values[0]!);
    }

    private static string ReadString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? string.Empty : StringValue(value);
    }

    private static double ReadNumber(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? 0 : (double)NumberValue(value);
    }

    private static int FindGroup(StressApiGroup[] groups, string name)
    {
        for (var index = 0; index < groups.Length; index++)
        {
            if (groups[index].Name == name)
                return index;
        }
        return -1;
    }

    private static StressSwaggerResource[] Append(StressSwaggerResource[] source, StressSwaggerResource item)
    {
        var result = new StressSwaggerResource[source.Length + 1];
        for (var index = 0; index < source.Length; index++) result[index] = source[index];
        result[source.Length] = item;
        return result;
    }

    private static StressApiNode[] Append(StressApiNode[] source, StressApiNode item)
    {
        var result = new StressApiNode[source.Length + 1];
        for (var index = 0; index < source.Length; index++) result[index] = source[index];
        result[source.Length] = item;
        return result;
    }

    private static StressApiGroup[] Append(StressApiGroup[] source, StressApiGroup item)
    {
        var result = new StressApiGroup[source.Length + 1];
        for (var index = 0; index < source.Length; index++) result[index] = source[index];
        result[source.Length] = item;
        return result;
    }

    private static StressPair[] Append(StressPair[] source, StressPair item)
    {
        var result = new StressPair[source.Length + 1];
        for (var index = 0; index < source.Length; index++) result[index] = source[index];
        result[source.Length] = item;
        return result;
    }
}

public sealed record StressSwaggerResource : VueProps
{
    public string Name { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}

public sealed record StressApiGroup : VueProps
{
    public string Name { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public StressApiNode[] Children { get; set; } = [];
}

public sealed record StressApiNode : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string TagName { get; init; } = string.Empty;
    public StressPair[] Headers { get; init; } = [];
    public StressPair[] RequestParameters { get; init; } = [];
    public StressPair[] PathParameters { get; init; } = [];
    public StressPair[] QueryParameters { get; init; } = [];
}

public sealed record StressKeyValueInput
{
    [ECMAScriptName("key")]
    public string Key { get; init; } = string.Empty;
    [ECMAScriptName("value")]
    public string Value { get; init; } = string.Empty;
}

public sealed record StressTestInput
{
    [ECMAScriptName("requestUri")]
    public string RequestUri { get; init; } = string.Empty;
    [ECMAScriptName("requestMethod")]
    public string RequestMethod { get; init; } = "GET";
    [ECMAScriptName("numberOfRequests")]
    public int NumberOfRequests { get; init; }
    [ECMAScriptName("numberOfRounds")]
    public int NumberOfRounds { get; init; }
    [ECMAScriptName("maxDegreeOfParallelism")]
    public int MaxDegreeOfParallelism { get; init; }
    [ECMAScriptName("requestParameters")]
    public StressKeyValueInput[] RequestParameters { get; init; } = [];
    [ECMAScriptName("headers")]
    public VueDictionary<string> Headers { get; init; } = new();
    [ECMAScriptName("pathParameters")]
    public VueDictionary<string> PathParameters { get; init; } = new();
    [ECMAScriptName("queryParameters")]
    public VueDictionary<string> QueryParameters { get; init; } = new();
}

public sealed record StressPair
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
