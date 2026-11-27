using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Plugin;

/// <summary>维护插件记录，编译 C# 源码或移除程序集。</summary>
[ECMAScriptModule("components/plugin-client.js")]
public static class PluginClient
{
    public static IPromise<ApiOutcome> Page(int page, int pageSize, string name, string tenantId)
        => tenantId.Length == 0
            ? BrowserApiClient.Post("/api/sysPlugin/page", new PluginPageInput
            {
                Page = page,
                PageSize = pageSize,
                Name = name
            })
            : BrowserApiClient.Post("/api/sysPlugin/page", new PluginTenantPageInput
            {
                Page = page,
                PageSize = pageSize,
                Name = name,
                TenantId = tenantId
            });

    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static IPromise<ApiOutcome> Add(PluginAddInput input)
        => BrowserApiClient.Post("/api/sysPlugin/add", input);

    public static IPromise<ApiOutcome> Update(PluginUpdateInput input)
        => BrowserApiClient.Post("/api/sysPlugin/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysPlugin/delete", new PluginIdInput { Id = id });

    public static IPromise<ApiOutcome> CompileAssembly(string csharpCode, string assemblyName)
    {
        var path = "/api/sysPlugin/compileAssembly";
        if (assemblyName.Length > 0)
            path += "?assemblyName=" + EncodeURIComponent(assemblyName);
        // BrowserApiClient 将源码序列化为 JSON 字符串，以匹配服务端 [FromBody] 的绑定类型。
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
                Value = ReadString(value, "value"),
                Label = ReadString(value, "label"),
                Host = ReadString(value, "host")
            };
        }
        return result;
    }

    public static string ReadAssemblyName(object? data)
        => data is null ? string.Empty : StringValue(data);

    private static PluginView ReadItem(object value)
        => new()
        {
            // 雪花 ID 跨浏览器边界时保留为字符串，避免 JavaScript 数值丢失精度。
            Id = ReadString(value, "id"),
            TenantId = ReadString(value, "tenantId"),
            Name = ReadString(value, "name"),
            CsharpCode = ReadString(value, "csharpCode"),
            AssemblyName = ReadString(value, "assemblyName"),
            OrderNo = ReadInt(value, "orderNo", 100),
            Status = ReadInt(value, "status", 1),
            Remark = ReadString(value, "remark"),
            CreateTime = ReadString(value, "createTime"),
            CreateUserId = ReadString(value, "createUserId"),
            CreateUserName = ReadString(value, "createUserName"),
            UpdateTime = ReadString(value, "updateTime"),
            UpdateUserId = ReadString(value, "updateUserId"),
            UpdateUserName = ReadString(value, "updateUserName")
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
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
}

public sealed record PluginTenantPageInput
{
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
}

public sealed record PluginAddInput
{
    [ECMAScriptName("tenantId")]
    public string? TenantId { get; init; }
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("csharpCode")]
    public string CsharpCode { get; init; } = string.Empty;
    [ECMAScriptName("assemblyName")]
    public string? AssemblyName { get; init; }
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("remark")]
    public string? Remark { get; init; }
}

/// <summary>
/// 更新插件时以选中行作为底稿，保留租户和审计字段，避免服务端映射回 SysPlugin 时覆盖表单未编辑的值。
/// </summary>
public sealed record PluginUpdateInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string? TenantId { get; init; }
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("csharpCode")]
    public string CsharpCode { get; init; } = string.Empty;
    [ECMAScriptName("assemblyName")]
    public string? AssemblyName { get; init; }
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("remark")]
    public string? Remark { get; init; }
    [ECMAScriptName("createTime")]
    public string CreateTime { get; init; } = string.Empty;
    [ECMAScriptName("createUserId")]
    public string? CreateUserId { get; init; }
    [ECMAScriptName("createUserName")]
    public string? CreateUserName { get; init; }
    [ECMAScriptName("updateTime")]
    public string? UpdateTime { get; init; }
    [ECMAScriptName("updateUserId")]
    public string? UpdateUserId { get; init; }
    [ECMAScriptName("updateUserName")]
    public string? UpdateUserName { get; init; }
}

public sealed record PluginIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record PluginView : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string CsharpCode { get; init; } = string.Empty;
    public string AssemblyName { get; init; } = string.Empty;
    public int OrderNo { get; init; }
    public int Status { get; init; }
    public string Remark { get; init; } = string.Empty;
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
}

public sealed record PluginTenantOption : VueProps
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
}
