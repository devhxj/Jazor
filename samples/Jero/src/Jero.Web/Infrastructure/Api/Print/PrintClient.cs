using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Print;

/// <summary>维护打印模板，并按名称查询打印配置。</summary>
[ECMAScriptModule("components/print-client.js")]
public static class PrintClient
{
    public static IPromise<ApiOutcome> Page(PrintPageInput input)
        => BrowserApiClient.Post("/api/sysPrint/page", input);

    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static IPromise<ApiOutcome> GetByName(string name)
        => BrowserApiClient.Get("/api/sysPrint/print/" + EncodeURIComponent(name));

    public static IPromise<ApiOutcome> Add(PrintAddInput input)
        => BrowserApiClient.Post("/api/sysPrint/add", input);

    public static IPromise<ApiOutcome> Update(PrintUpdateInput input)
        => BrowserApiClient.Post("/api/sysPrint/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysPrint/delete", new PrintIdInput { Id = id });

    public static PrintView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new PrintView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadItem(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static PrintView? ReadSingle(object? data)
        => data is null ? null : ReadItem(data);

    public static PrintTenantOption[] ReadTenants(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new PrintTenantOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new PrintTenantOption
            {
                Value = ReadString(value, "value"),
                Label = ReadString(value, "label"),
                Host = ReadString(value, "host")
            };
        }
        return result;
    }

    private static PrintView ReadItem(object value)
        => new()
        {
            // 雪花 ID 跨浏览器边界时保留为字符串，避免 JavaScript 数值丢失精度。
            Id = ReadString(value, "id"),
            TenantId = ReadString(value, "tenantId"),
            Name = ReadString(value, "name"),
            Template = ReadString(value, "template"),
            PrintType = ReadInt(value, "printType", 1),
            ClientServiceAddress = ReadString(value, "clientServiceAddress"),
            PrintParam = ReadString(value, "printParam"),
            OrderNo = ReadInt(value, "orderNo", 100),
            Status = ReadInt(value, "status", 1),
            Remark = ReadString(value, "remark"),
            PrintDataDemo = ReadString(value, "printDataDemo"),
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

public sealed record PrintPageInput
{
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = "0";
}

public sealed record PrintAddInput
{
    [ECMAScriptName("tenantId")]
    public string? TenantId { get; init; }
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("template")]
    public string Template { get; init; } = string.Empty;
    [ECMAScriptName("printType")]
    public int PrintType { get; init; } = 1;
    [ECMAScriptName("clientServiceAddress")]
    public string? ClientServiceAddress { get; init; }
    [ECMAScriptName("printParam")]
    public string? PrintParam { get; init; }
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("remark")]
    public string? Remark { get; init; }
    [ECMAScriptName("printDataDemo")]
    public string? PrintDataDemo { get; init; }
}

/// <summary>
/// 更新打印模板时提交完整的行数据。服务端映射为整条实体，需保留选中行的租户和审计字段。
/// </summary>
public sealed record PrintUpdateInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string? TenantId { get; init; }
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("template")]
    public string Template { get; init; } = string.Empty;
    [ECMAScriptName("printType")]
    public int PrintType { get; init; } = 1;
    [ECMAScriptName("clientServiceAddress")]
    public string? ClientServiceAddress { get; init; }
    [ECMAScriptName("printParam")]
    public string? PrintParam { get; init; }
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("remark")]
    public string? Remark { get; init; }
    [ECMAScriptName("printDataDemo")]
    public string? PrintDataDemo { get; init; }
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

public sealed record PrintIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record PrintView : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Template { get; init; } = string.Empty;
    public int PrintType { get; init; }
    public string ClientServiceAddress { get; init; } = string.Empty;
    public string PrintParam { get; init; } = string.Empty;
    public int OrderNo { get; init; }
    public int Status { get; init; }
    public string Remark { get; init; } = string.Empty;
    public string PrintDataDemo { get; init; } = string.Empty;
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
}

public sealed record PrintTenantOption : VueProps
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
}
