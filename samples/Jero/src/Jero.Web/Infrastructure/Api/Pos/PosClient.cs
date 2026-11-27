using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Pos;

/// <summary>按租户查询和维护岗位。</summary>
[ECMAScriptModule("components/pos-client.js")]
public static class PosClient
{
    public static IPromise<ApiOutcome> List(PosListInput input)
    {
        var path = "/api/sysPos/list?Code=" + EncodeURIComponent(input.Code) +
                   "&Name=" + EncodeURIComponent(input.Name);
        if (input.TenantId.Length > 0)
            path += "&TenantId=" + EncodeURIComponent(input.TenantId);
        return BrowserApiClient.Get(path);
    }

    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static IPromise<ApiOutcome> Add(PosSaveInput input)
        => BrowserApiClient.Post("/api/sysPos/add", new PosAddInput
        {
            Name = input.Name,
            Code = input.Code,
            OrderNo = input.OrderNo,
            Remark = input.Remark,
            Status = input.Status,
            TenantId = input.TenantId
        });

    public static IPromise<ApiOutcome> Update(PosSaveInput input)
        => BrowserApiClient.Post("/api/sysPos/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysPos/delete", new PosIdInput { Id = id });

    public static PosView[] ReadPositions(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new PosView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadPosition(values[index]!);
        return result;
    }

    public static TenantOption[] ReadTenants(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new TenantOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new TenantOption
            {
                Value = ReadString(value, "value"),
                Label = ReadString(value, "label"),
                Host = ReadString(value, "host")
            };
        }
        return result;
    }

    private static PosView ReadPosition(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            TenantId = ReadString(value, "tenantId"),
            Name = ReadString(value, "name"),
            Code = ReadString(value, "code"),
            OrderNo = ReadInt(value, "orderNo", 100),
            Remark = ReadString(value, "remark"),
            Status = ReadInt(value, "status", 1),
            CreateTime = ReadString(value, "createTime"),
            CreateUserId = ReadString(value, "createUserId"),
            CreateUserName = ReadString(value, "createUserName"),
            UpdateTime = ReadString(value, "updateTime"),
            UpdateUserId = ReadString(value, "updateUserId"),
            UpdateUserName = ReadString(value, "updateUserName"),
            UserList = ReadUsers(Reflect.Get(value, "userList"))
        };

    private static PosUserView[] ReadUsers(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new PosUserView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new PosUserView
            {
                Id = ReadString(value, "id"),
                Account = ReadString(value, "account"),
                RealName = ReadString(value, "realName")
            };
        }
        return result;
    }

    private static string ReadString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? string.Empty : StringValue(value);
    }

    private static int ReadInt(object data, string key, int fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : (int)NumberValue(value);
    }
}

public sealed record PosListInput
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
}

public sealed record PosSaveInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
}

public sealed record PosAddInput
{
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
}

public sealed record PosIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record PosView : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public int OrderNo { get; init; }
    public string Remark { get; init; } = string.Empty;
    public int Status { get; init; }
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
    public PosUserView[] UserList { get; init; } = [];
}

public sealed record PosUserView : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string Account { get; init; } = string.Empty;
    public string RealName { get; init; } = string.Empty;
}

public sealed record TenantOption : VueProps
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
}
