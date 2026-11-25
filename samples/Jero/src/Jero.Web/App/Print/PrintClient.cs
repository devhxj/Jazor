using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Print;

/// <summary>Typed browser boundary for print template management.</summary>
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
        => BrowserApiClient.Post("/api/sysPrint/delete", new PrintIdInput { id = id });

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
                value = ReadString(value, "value"),
                label = ReadString(value, "label"),
                host = ReadString(value, "host")
            };
        }
        return result;
    }

    private static PrintView ReadItem(object value)
        => new()
        {
            // Snowflake identifiers must remain strings across the JavaScript boundary.
            id = ReadString(value, "id"),
            tenantId = ReadString(value, "tenantId"),
            name = ReadString(value, "name"),
            template = ReadString(value, "template"),
            printType = ReadInt(value, "printType", 1),
            clientServiceAddress = ReadString(value, "clientServiceAddress"),
            printParam = ReadString(value, "printParam"),
            orderNo = ReadInt(value, "orderNo", 100),
            status = ReadInt(value, "status", 1),
            remark = ReadString(value, "remark"),
            printDataDemo = ReadString(value, "printDataDemo"),
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

public sealed record PrintPageInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string name { get; init; } = string.Empty;
    public string tenantId { get; init; } = "0";
}

public sealed record PrintAddInput
{
    public string? tenantId { get; init; }
    public string name { get; init; } = string.Empty;
    public string template { get; init; } = string.Empty;
    public int printType { get; init; } = 1;
    public string? clientServiceAddress { get; init; }
    public string? printParam { get; init; }
    public int orderNo { get; init; } = 100;
    public int status { get; init; } = 1;
    public string? remark { get; init; }
    public string? printDataDemo { get; init; }
}

/// <summary>
/// Full row snapshot for SysPrintService.UpdatePrint. The backend adapts this DTO to the
/// complete entity, so tenant and audit fields from the selected row cannot be dropped.
/// </summary>
public sealed record PrintUpdateInput
{
    public string id { get; init; } = string.Empty;
    public string? tenantId { get; init; }
    public string name { get; init; } = string.Empty;
    public string template { get; init; } = string.Empty;
    public int printType { get; init; } = 1;
    public string? clientServiceAddress { get; init; }
    public string? printParam { get; init; }
    public int orderNo { get; init; } = 100;
    public int status { get; init; } = 1;
    public string? remark { get; init; }
    public string? printDataDemo { get; init; }
    public string createTime { get; init; } = string.Empty;
    public string? createUserId { get; init; }
    public string? createUserName { get; init; }
    public string? updateTime { get; init; }
    public string? updateUserId { get; init; }
    public string? updateUserName { get; init; }
}

public sealed record PrintIdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed record PrintView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string template { get; init; } = string.Empty;
    public int printType { get; init; }
    public string clientServiceAddress { get; init; } = string.Empty;
    public string printParam { get; init; } = string.Empty;
    public int orderNo { get; init; }
    public int status { get; init; }
    public string remark { get; init; } = string.Empty;
    public string printDataDemo { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
}

public sealed record PrintTenantOption : VueProps
{
    public string value { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string host { get; init; } = string.Empty;
}
