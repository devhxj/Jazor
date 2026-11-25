using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Plugin;

/// <summary>Typed browser boundary for dynamic plugin management.</summary>
[ECMAScriptModule("components/plugin-client.js")]
public static class PluginClient
{
    public static IPromise<ApiOutcome> Page(int page, int pageSize, string name, string tenantId)
        => tenantId.Length == 0
            ? BrowserApiClient.Post("/api/sysPlugin/page", new PluginPageInput
            {
                page = page,
                pageSize = pageSize,
                name = name
            })
            : BrowserApiClient.Post("/api/sysPlugin/page", new PluginTenantPageInput
            {
                page = page,
                pageSize = pageSize,
                name = name,
                tenantId = tenantId
            });

    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static IPromise<ApiOutcome> Add(PluginAddInput input)
        => BrowserApiClient.Post("/api/sysPlugin/add", input);

    public static IPromise<ApiOutcome> Update(PluginUpdateInput input)
        => BrowserApiClient.Post("/api/sysPlugin/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysPlugin/delete", new PluginIdInput { id = id });

    public static IPromise<ApiOutcome> CompileAssembly(string csharpCode, string assemblyName)
    {
        var path = "/api/sysPlugin/compileAssembly";
        if (assemblyName.Length > 0)
            path += "?assemblyName=" + EncodeURIComponent(assemblyName);
        // BrowserApiClient JSON-stringifies the string, which is the [FromBody] JSON string contract.
        return BrowserApiClient.Post(path, csharpCode);
    }

    public static IPromise<ApiOutcome> RemoveAssembly(string assemblyName)
        => BrowserApiClient.Post("/api/sysPlugin/removeAssembly/" + EncodeURIComponent(assemblyName));

    public static PluginView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new PluginView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadItem(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static PluginTenantOption[] ReadTenants(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new PluginTenantOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new PluginTenantOption
            {
                value = ReadString(value, "value"),
                label = ReadString(value, "label"),
                host = ReadString(value, "host")
            };
        }
        return result;
    }

    public static string ReadAssemblyName(object? data)
        => data is null ? string.Empty : StringValue(data);

    private static PluginView ReadItem(object value)
        => new()
        {
            // Snowflake identifiers must never cross the browser boundary as JavaScript numbers.
            id = ReadString(value, "id"),
            tenantId = ReadString(value, "tenantId"),
            name = ReadString(value, "name"),
            csharpCode = ReadString(value, "csharpCode"),
            assemblyName = ReadString(value, "assemblyName"),
            orderNo = ReadInt(value, "orderNo", 100),
            status = ReadInt(value, "status", 1),
            remark = ReadString(value, "remark"),
            createTime = ReadString(value, "createTime"),
            createUserId = ReadString(value, "createUserId"),
            createUserName = ReadString(value, "createUserName"),
            updateTime = ReadString(value, "updateTime"),
            updateUserId = ReadString(value, "updateUserId"),
            updateUserName = ReadString(value, "updateUserName")
        };

    private static string ReadString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        if (value is null)
            return string.Empty;
        var result = StringValue(value);
        return result == "null" || result == "undefined" ? string.Empty : result;
    }

    private static int ReadInt(object data, string key, int fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : (int)NumberValue(value);
    }
}

public sealed record PluginPageInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string name { get; init; } = string.Empty;
}

public sealed record PluginTenantPageInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string name { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
}

public sealed record PluginAddInput
{
    public string? tenantId { get; init; }
    public string name { get; init; } = string.Empty;
    public string csharpCode { get; init; } = string.Empty;
    public string? assemblyName { get; init; }
    public int orderNo { get; init; } = 100;
    public int status { get; init; } = 1;
    public string? remark { get; init; }
}

/// <summary>
/// Full selected-row snapshot for UpdatePlugin. Keep the tenant and audit fields so adapting the
/// DTO back to SysPlugin cannot erase values that are outside the visible edit controls.
/// </summary>
public sealed record PluginUpdateInput
{
    public string id { get; init; } = string.Empty;
    public string? tenantId { get; init; }
    public string name { get; init; } = string.Empty;
    public string csharpCode { get; init; } = string.Empty;
    public string? assemblyName { get; init; }
    public int orderNo { get; init; } = 100;
    public int status { get; init; } = 1;
    public string? remark { get; init; }
    public string createTime { get; init; } = string.Empty;
    public string? createUserId { get; init; }
    public string? createUserName { get; init; }
    public string? updateTime { get; init; }
    public string? updateUserId { get; init; }
    public string? updateUserName { get; init; }
}

public sealed record PluginIdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed record PluginView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string csharpCode { get; init; } = string.Empty;
    public string assemblyName { get; init; } = string.Empty;
    public int orderNo { get; init; }
    public int status { get; init; }
    public string remark { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
}

public sealed record PluginTenantOption : VueProps
{
    public string value { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string host { get; init; } = string.Empty;
}
