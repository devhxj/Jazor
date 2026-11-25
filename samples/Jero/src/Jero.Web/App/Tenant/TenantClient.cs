using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Tenant;

/// <summary>租户管理浏览器 API。所有 long / 雪花 ID 在 JSON 边界保持字符串。</summary>
[ECMAScriptModule("components/tenant-client.js")]
public static class TenantClient
{
    public static IPromise<ApiOutcome> Page(TenantPageInput input)
        => BrowserApiClient.Post("/api/sysTenant/page", input);

    public static IPromise<ApiOutcome> Add(TenantSaveInput input)
    {
        var payload = ToAddInput(input);
        OmitEmptyLogoFile(payload, input.logoBase64);
        return BrowserApiClient.Post("/api/sysTenant/add", payload);
    }

    public static IPromise<ApiOutcome> Update(TenantSaveInput input)
    {
        OmitEmptyLogoFile(input, input.logoBase64);
        return BrowserApiClient.Post("/api/sysTenant/update", input);
    }

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysTenant/delete", new TenantIdInput { id = id });

    public static IPromise<ApiOutcome> SetStatus(string id, int status)
        => BrowserApiClient.Post("/api/sysTenant/setStatus", new TenantStatusInput { id = id, status = status });

    public static IPromise<ApiOutcome> CreateDatabase(string id)
        => BrowserApiClient.Post("/api/sysTenant/createDb", new TenantIdInput { id = id });

    public static IPromise<ApiOutcome> ResetPassword(string userId)
        => BrowserApiClient.Post("/api/sysTenant/resetPwd", new TenantUserInput { userId = userId });

    public static IPromise<ApiOutcome> ChangeTenant(string id)
        => BrowserApiClient.Post("/api/sysTenant/changeTenant", new TenantIdInput { id = id });

    public static IPromise<ApiOutcome> GoTenant(string id)
        => BrowserApiClient.Post("/api/sysTenant/goTenant", new TenantIdInput { id = id });

    public static IPromise<ApiOutcome> SyncGrantMenu(string id)
        => BrowserApiClient.Post("/api/sysTenant/syncGrantMenu", new TenantIdInput { id = id });

    public static IPromise<ApiOutcome> MenuTree()
        => BrowserApiClient.Get("/api/sysMenu/list");

    public static IPromise<ApiOutcome> OwnMenuList(string id)
        => BrowserApiClient.Get("/api/sysTenant/tenantMenuList?Id=" + EncodeURIComponent(id));

    public static IPromise<ApiOutcome> GrantMenu(string id, string[] menuIds)
        => BrowserApiClient.Post("/api/sysTenant/grantMenu", new TenantGrantMenuInput
        {
            id = id,
            menuIdList = menuIds,
            tenantIdList = []
        });

    public static TenantView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new TenantView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadTenant(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static TenantMenuNode[] ReadMenuTree(object? data)
    {
        var result = new TenantMenuNode[CountTree(data)];
        WriteMenuTree(data, result, 0, 0);
        return result;
    }

    public static string[] ReadIds(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new string[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = values[index] is null ? string.Empty : NormalizeString(values[index]!);
        return result;
    }

    public static string ReadPassword(object? data)
        => data is null ? string.Empty : NormalizeString(data);

    public static TenantLoginResult ReadLoginResult(object? data)
    {
        if (data is null)
            return new TenantLoginResult();
        return new TenantLoginResult
        {
            accessToken = ReadString(data, "accessToken"),
            refreshToken = ReadString(data, "refreshToken"),
            homepage = ReadString(data, "homepage")
        };
    }

    private static TenantAddInput ToAddInput(TenantSaveInput input)
        => new()
        {
            host = input.host,
            tenantType = input.tenantType,
            dbType = input.dbType,
            connection = input.connection,
            configId = input.configId,
            slaveConnections = input.slaveConnections,
            enableReg = input.enableReg,
            regWayId = input.regWayId,
            logo = input.logo,
            logoBase64 = input.logoBase64,
            logoFileName = input.logoFileName,
            title = input.title,
            viceTitle = input.viceTitle,
            viceDesc = input.viceDesc,
            watermark = input.watermark,
            copyright = input.copyright,
            icp = input.icp,
            icpUrl = input.icpUrl,
            orderNo = input.orderNo,
            remark = input.remark,
            status = input.status,
            name = input.name,
            adminAccount = input.adminAccount,
            email = input.email,
            phone = input.phone
        };

    private static void OmitEmptyLogoFile(object payload, string? logoBase64)
    {
        if (!string.IsNullOrEmpty(logoBase64))
            return;

        // Missing file keys mean "keep the current logo"; explicit null/empty values are a
        // different request shape and make route-level contract checks ambiguous.
        Reflect.DeleteProperty(payload, "logoBase64");
        Reflect.DeleteProperty(payload, "logoFileName");
    }

    private static TenantView ReadTenant(object value)
        => new()
        {
            id = ReadString(value, "id"),
            userId = ReadString(value, "userId"),
            orgId = ReadString(value, "orgId"),
            host = ReadString(value, "host"),
            tenantType = ReadInt(value, "tenantType", 0),
            dbType = ReadInt(value, "dbType", 0),
            connection = ReadString(value, "connection"),
            configId = ReadString(value, "configId"),
            slaveConnections = ReadString(value, "slaveConnections"),
            enableReg = ReadNullableInt(value, "enableReg"),
            regWayId = ReadOptionalString(value, "regWayId"),
            logo = ReadString(value, "logo"),
            title = ReadString(value, "title"),
            viceTitle = ReadString(value, "viceTitle"),
            viceDesc = ReadString(value, "viceDesc"),
            watermark = ReadString(value, "watermark"),
            copyright = ReadString(value, "copyright"),
            icp = ReadString(value, "icp"),
            icpUrl = ReadString(value, "icpUrl"),
            orderNo = ReadInt(value, "orderNo", 100),
            remark = ReadString(value, "remark"),
            status = ReadInt(value, "status", 1),
            createTime = ReadString(value, "createTime"),
            createUserId = ReadOptionalString(value, "createUserId"),
            createUserName = ReadOptionalString(value, "createUserName"),
            updateTime = ReadOptionalString(value, "updateTime"),
            updateUserId = ReadOptionalString(value, "updateUserId"),
            updateUserName = ReadOptionalString(value, "updateUserName"),
            name = ReadString(value, "name"),
            adminAccount = ReadString(value, "adminAccount"),
            email = ReadString(value, "email"),
            phone = ReadString(value, "phone")
        };

    private static int CountTree(object? data)
    {
        var values = data as object?[] ?? [];
        var count = values.Length;
        for (var index = 0; index < values.Length; index++)
            count += CountTree(Reflect.Get(values[index]!, "children"));
        return count;
    }

    private static int WriteMenuTree(object? data, TenantMenuNode[] target, int targetIndex, int depth)
    {
        var values = data as object?[] ?? [];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            var children = Reflect.Get(value, "children");
            target[targetIndex] = new TenantMenuNode
            {
                id = ReadString(value, "id"),
                parentId = ReadString(value, "pid"),
                label = ReadString(value, "title"),
                path = ReadString(value, "path"),
                depth = depth,
                hasChildren = (children as object?[] ?? []).Length > 0
            };
            targetIndex++;
            targetIndex = WriteMenuTree(children, target, targetIndex, depth + 1);
        }
        return targetIndex;
    }

    private static string ReadString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? string.Empty : NormalizeString(value);
    }

    private static string? ReadOptionalString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        if (value is null)
            return null;
        var text = NormalizeString(value);
        return text.Length == 0 ? null : text;
    }

    private static int ReadInt(object data, string key, int fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : (int)NumberValue(value);
    }

    private static int? ReadNullableInt(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? null : (int)NumberValue(value);
    }

    private static string NormalizeString(object value)
    {
        var text = StringValue(value);
        return text == "null" || text == "undefined" ? string.Empty : text;
    }
}

public sealed record TenantPageInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 20;
    public string name { get; init; } = string.Empty;
    public string phone { get; init; } = string.Empty;
}

public sealed record TenantIdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed record TenantUserInput
{
    public string userId { get; init; } = string.Empty;
}

public sealed record TenantStatusInput
{
    public string id { get; init; } = string.Empty;
    public int status { get; init; }
}

public sealed record TenantGrantMenuInput
{
    public string id { get; init; } = string.Empty;
    public string[] menuIdList { get; init; } = [];
    public string[] tenantIdList { get; init; } = [];
}

public sealed record TenantAddInput
{
    public string? host { get; init; }
    public int tenantType { get; init; }
    public int dbType { get; init; }
    public string? connection { get; init; }
    public string? configId { get; init; }
    public string? slaveConnections { get; init; }
    public int? enableReg { get; init; }
    public string? regWayId { get; init; }
    public string? logo { get; init; }
    public string? logoBase64 { get; init; }
    public string? logoFileName { get; init; }
    public string? title { get; init; }
    public string? viceTitle { get; init; }
    public string? viceDesc { get; init; }
    public string? watermark { get; init; }
    public string? copyright { get; init; }
    public string? icp { get; init; }
    public string? icpUrl { get; init; }
    public int orderNo { get; init; } = 100;
    public string? remark { get; init; }
    public int status { get; init; } = 1;
    public string name { get; init; } = string.Empty;
    public string adminAccount { get; init; } = string.Empty;
    public string email { get; init; } = string.Empty;
    public string phone { get; init; } = string.Empty;
}

/// <summary>
/// UpdateTenantInput 继承 TenantOutput/SysTenant。更新时回传完整分页行快照，防止隐藏字段被默认值覆盖。
/// </summary>
public sealed record TenantSaveInput
{
    public string id { get; init; } = string.Empty;
    public string userId { get; init; } = string.Empty;
    public string orgId { get; init; } = string.Empty;
    public string? host { get; init; }
    public int tenantType { get; init; }
    public int dbType { get; init; }
    public string? connection { get; init; }
    public string? configId { get; init; }
    public string? slaveConnections { get; init; }
    public int? enableReg { get; init; }
    public string? regWayId { get; init; }
    public string? logo { get; init; }
    public string? logoBase64 { get; init; }
    public string? logoFileName { get; init; }
    public string? title { get; init; }
    public string? viceTitle { get; init; }
    public string? viceDesc { get; init; }
    public string? watermark { get; init; }
    public string? copyright { get; init; }
    public string? icp { get; init; }
    public string? icpUrl { get; init; }
    public int orderNo { get; init; } = 100;
    public string? remark { get; init; }
    public int status { get; init; } = 1;
    public string createTime { get; init; } = string.Empty;
    public string? createUserId { get; init; }
    public string? createUserName { get; init; }
    public string? updateTime { get; init; }
    public string? updateUserId { get; init; }
    public string? updateUserName { get; init; }
    public string name { get; init; } = string.Empty;
    public string adminAccount { get; init; } = string.Empty;
    public string email { get; init; } = string.Empty;
    public string phone { get; init; } = string.Empty;
}

public sealed record TenantView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string userId { get; init; } = string.Empty;
    public string orgId { get; init; } = string.Empty;
    public string host { get; init; } = string.Empty;
    public int tenantType { get; init; }
    public int dbType { get; init; }
    public string connection { get; init; } = string.Empty;
    public string configId { get; init; } = string.Empty;
    public string slaveConnections { get; init; } = string.Empty;
    public int? enableReg { get; init; }
    public string? regWayId { get; init; }
    public string logo { get; init; } = string.Empty;
    public string title { get; init; } = string.Empty;
    public string viceTitle { get; init; } = string.Empty;
    public string viceDesc { get; init; } = string.Empty;
    public string watermark { get; init; } = string.Empty;
    public string copyright { get; init; } = string.Empty;
    public string icp { get; init; } = string.Empty;
    public string icpUrl { get; init; } = string.Empty;
    public int orderNo { get; init; }
    public string remark { get; init; } = string.Empty;
    public int status { get; init; }
    public string createTime { get; init; } = string.Empty;
    public string? createUserId { get; init; }
    public string? createUserName { get; init; }
    public string? updateTime { get; init; }
    public string? updateUserId { get; init; }
    public string? updateUserName { get; init; }
    public string name { get; init; } = string.Empty;
    public string adminAccount { get; init; } = string.Empty;
    public string email { get; init; } = string.Empty;
    public string phone { get; init; } = string.Empty;
}

public sealed record TenantMenuNode
{
    public string id { get; init; } = string.Empty;
    public string parentId { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string path { get; init; } = string.Empty;
    public int depth { get; init; }
    public bool hasChildren { get; init; }
}

public sealed record TenantLoginResult
{
    public string accessToken { get; init; } = string.Empty;
    public string refreshToken { get; init; } = string.Empty;
    public string homepage { get; init; } = string.Empty;
}
