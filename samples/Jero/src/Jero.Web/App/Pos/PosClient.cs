using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Pos;

/// <summary>Typed browser boundary for position management.</summary>
[ECMAScriptModule("components/pos-client.js")]
public static class PosClient
{
    public static IPromise<ApiOutcome> List(PosListInput input)
    {
        var path = "/api/sysPos/list?Code=" + EncodeURIComponent(input.code) +
                   "&Name=" + EncodeURIComponent(input.name);
        if (input.tenantId.Length > 0)
            path += "&TenantId=" + EncodeURIComponent(input.tenantId);
        return BrowserApiClient.Get(path);
    }

    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static IPromise<ApiOutcome> Add(PosSaveInput input)
        => BrowserApiClient.Post("/api/sysPos/add", new PosAddInput
        {
            name = input.name,
            code = input.code,
            orderNo = input.orderNo,
            remark = input.remark,
            status = input.status,
            tenantId = input.tenantId
        });

    public static IPromise<ApiOutcome> Update(PosSaveInput input)
        => BrowserApiClient.Post("/api/sysPos/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysPos/delete", new PosIdInput { id = id });

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
                value = ReadString(value, "value"),
                label = ReadString(value, "label"),
                host = ReadString(value, "host")
            };
        }
        return result;
    }

    private static PosView ReadPosition(object value)
        => new()
        {
            id = ReadString(value, "id"),
            tenantId = ReadString(value, "tenantId"),
            name = ReadString(value, "name"),
            code = ReadString(value, "code"),
            orderNo = ReadInt(value, "orderNo", 100),
            remark = ReadString(value, "remark"),
            status = ReadInt(value, "status", 1),
            createTime = ReadString(value, "createTime"),
            createUserId = ReadString(value, "createUserId"),
            createUserName = ReadString(value, "createUserName"),
            updateTime = ReadString(value, "updateTime"),
            updateUserId = ReadString(value, "updateUserId"),
            updateUserName = ReadString(value, "updateUserName"),
            userList = ReadUsers(Reflect.Get(value, "userList"))
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
                id = ReadString(value, "id"),
                account = ReadString(value, "account"),
                realName = ReadString(value, "realName")
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
    public string code { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
}

public sealed record PosSaveInput
{
    public string id { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public int orderNo { get; init; } = 100;
    public string remark { get; init; } = string.Empty;
    public int status { get; init; } = 1;
    public string tenantId { get; init; } = string.Empty;
}

public sealed record PosAddInput
{
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public int orderNo { get; init; } = 100;
    public string remark { get; init; } = string.Empty;
    public int status { get; init; } = 1;
    public string tenantId { get; init; } = string.Empty;
}

public sealed record PosIdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed record PosView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public int orderNo { get; init; }
    public string remark { get; init; } = string.Empty;
    public int status { get; init; }
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
    public PosUserView[] userList { get; init; } = [];
}

public sealed record PosUserView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string account { get; init; } = string.Empty;
    public string realName { get; init; } = string.Empty;
}

public sealed record TenantOption : VueProps
{
    public string value { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string host { get; init; } = string.Empty;
}
