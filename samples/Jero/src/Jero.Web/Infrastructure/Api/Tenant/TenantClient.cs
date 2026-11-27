using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Tenant;

/// <summary>租户管理浏览器 API。所有 long / 雪花 ID 在 JSON 边界保持字符串。</summary>
[ECMAScriptModule("components/tenant-client.js")]
public static class TenantClient
{
    public static IPromise<ApiOutcome> Page(TenantPageInput input)
        => BrowserApiClient.Post("/api/sysTenant/page", input);

    public static IPromise<ApiOutcome> Add(TenantSaveInput input)
    {
        var payload = ToAddInput(input);
        OmitEmptyLogoFile(payload, input.LogoBase64);
        return BrowserApiClient.Post("/api/sysTenant/add", payload);
    }

    public static IPromise<ApiOutcome> Update(TenantSaveInput input)
    {
        OmitEmptyLogoFile(input, input.LogoBase64);
        return BrowserApiClient.Post("/api/sysTenant/update", input);
    }

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysTenant/delete", new TenantIdInput { Id = id });

    public static IPromise<ApiOutcome> SetStatus(string id, int status)
        => BrowserApiClient.Post("/api/sysTenant/setStatus", new TenantStatusInput { Id = id, Status = status });

    public static IPromise<ApiOutcome> CreateDatabase(string id)
        => BrowserApiClient.Post("/api/sysTenant/createDb", new TenantIdInput { Id = id });

    public static IPromise<ApiOutcome> ResetPassword(string userId)
        => BrowserApiClient.Post("/api/sysTenant/resetPwd", new TenantUserInput { UserId = userId });

    public static IPromise<ApiOutcome> ChangeTenant(string id)
        => BrowserApiClient.Post("/api/sysTenant/changeTenant", new TenantIdInput { Id = id });

    public static IPromise<ApiOutcome> GoTenant(string id)
        => BrowserApiClient.Post("/api/sysTenant/goTenant", new TenantIdInput { Id = id });

    public static IPromise<ApiOutcome> SyncGrantMenu(string id)
        => BrowserApiClient.Post("/api/sysTenant/syncGrantMenu", new TenantIdInput { Id = id });

    public static IPromise<ApiOutcome> MenuTree()
        => BrowserApiClient.Get("/api/sysMenu/list");

    public static IPromise<ApiOutcome> OwnMenuList(string id)
        => BrowserApiClient.Get("/api/sysTenant/tenantMenuList?Id=" + EncodeURIComponent(id));

    public static IPromise<ApiOutcome> GrantMenu(string id, string[] menuIds)
        => BrowserApiClient.Post("/api/sysTenant/grantMenu", new TenantGrantMenuInput
        {
            Id = id,
            MenuIdList = menuIds,
            TenantIdList = []
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
            AccessToken = ReadString(data, "accessToken"),
            RefreshToken = ReadString(data, "refreshToken"),
            Homepage = ReadString(data, "homepage")
        };
    }

    private static TenantAddInput ToAddInput(TenantSaveInput input)
        => new()
        {
            Host = input.Host,
            TenantType = input.TenantType,
            DbType = input.DbType,
            Connection = input.Connection,
            ConfigId = input.ConfigId,
            SlaveConnections = input.SlaveConnections,
            EnableReg = input.EnableReg,
            RegWayId = input.RegWayId,
            Logo = input.Logo,
            LogoBase64 = input.LogoBase64,
            LogoFileName = input.LogoFileName,
            Title = input.Title,
            ViceTitle = input.ViceTitle,
            ViceDesc = input.ViceDesc,
            Watermark = input.Watermark,
            Copyright = input.Copyright,
            Icp = input.Icp,
            IcpUrl = input.IcpUrl,
            OrderNo = input.OrderNo,
            Remark = input.Remark,
            Status = input.Status,
            Name = input.Name,
            AdminAccount = input.AdminAccount,
            Email = input.Email,
            Phone = input.Phone
        };

    private static void OmitEmptyLogoFile(object payload, string? logoBase64)
    {
        if (!string.IsNullOrEmpty(logoBase64))
            return;

        // 未选择新 Logo 时省略两个上传字段，以保留当前图片，并保持约定的请求结构。
        Reflect.DeleteProperty(payload, "logoBase64");
        Reflect.DeleteProperty(payload, "logoFileName");
    }

    private static TenantView ReadTenant(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            UserId = ReadString(value, "userId"),
            OrgId = ReadString(value, "orgId"),
            Host = ReadString(value, "host"),
            TenantType = ReadInt(value, "tenantType", 0),
            DbType = ReadInt(value, "dbType", 0),
            Connection = ReadString(value, "connection"),
            ConfigId = ReadString(value, "configId"),
            SlaveConnections = ReadString(value, "slaveConnections"),
            EnableReg = ReadNullableInt(value, "enableReg"),
            RegWayId = ReadOptionalString(value, "regWayId"),
            Logo = ReadString(value, "logo"),
            Title = ReadString(value, "title"),
            ViceTitle = ReadString(value, "viceTitle"),
            ViceDesc = ReadString(value, "viceDesc"),
            Watermark = ReadString(value, "watermark"),
            Copyright = ReadString(value, "copyright"),
            Icp = ReadString(value, "icp"),
            IcpUrl = ReadString(value, "icpUrl"),
            OrderNo = ReadInt(value, "orderNo", 100),
            Remark = ReadString(value, "remark"),
            Status = ReadInt(value, "status", 1),
            CreateTime = ReadString(value, "createTime"),
            CreateUserId = ReadOptionalString(value, "createUserId"),
            CreateUserName = ReadOptionalString(value, "createUserName"),
            UpdateTime = ReadOptionalString(value, "updateTime"),
            UpdateUserId = ReadOptionalString(value, "updateUserId"),
            UpdateUserName = ReadOptionalString(value, "updateUserName"),
            Name = ReadString(value, "name"),
            AdminAccount = ReadString(value, "adminAccount"),
            Email = ReadString(value, "email"),
            Phone = ReadString(value, "phone")
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
                Id = ReadString(value, "id"),
                ParentId = ReadString(value, "pid"),
                Label = ReadString(value, "title"),
                Path = ReadString(value, "path"),
                Depth = depth,
                HasChildren = (children as object?[] ?? []).Length > 0
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
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 20;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("phone")]
    public string Phone { get; init; } = string.Empty;
}

public sealed record TenantIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record TenantUserInput
{
    [ECMAScriptName("userId")]
    public string UserId { get; init; } = string.Empty;
}

public sealed record TenantStatusInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("status")]
    public int Status { get; init; }
}

public sealed record TenantGrantMenuInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("menuIdList")]
    public string[] MenuIdList { get; init; } = [];
    [ECMAScriptName("tenantIdList")]
    public string[] TenantIdList { get; init; } = [];
}

public sealed record TenantAddInput
{
    [ECMAScriptName("host")]
    public string? Host { get; init; }
    [ECMAScriptName("tenantType")]
    public int TenantType { get; init; }
    [ECMAScriptName("dbType")]
    public int DbType { get; init; }
    [ECMAScriptName("connection")]
    public string? Connection { get; init; }
    [ECMAScriptName("configId")]
    public string? ConfigId { get; init; }
    [ECMAScriptName("slaveConnections")]
    public string? SlaveConnections { get; init; }
    [ECMAScriptName("enableReg")]
    public int? EnableReg { get; init; }
    [ECMAScriptName("regWayId")]
    public string? RegWayId { get; init; }
    [ECMAScriptName("logo")]
    public string? Logo { get; init; }
    [ECMAScriptName("logoBase64")]
    public string? LogoBase64 { get; init; }
    [ECMAScriptName("logoFileName")]
    public string? LogoFileName { get; init; }
    [ECMAScriptName("title")]
    public string? Title { get; init; }
    [ECMAScriptName("viceTitle")]
    public string? ViceTitle { get; init; }
    [ECMAScriptName("viceDesc")]
    public string? ViceDesc { get; init; }
    [ECMAScriptName("watermark")]
    public string? Watermark { get; init; }
    [ECMAScriptName("copyright")]
    public string? Copyright { get; init; }
    [ECMAScriptName("icp")]
    public string? Icp { get; init; }
    [ECMAScriptName("icpUrl")]
    public string? IcpUrl { get; init; }
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("remark")]
    public string? Remark { get; init; }
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("adminAccount")]
    public string AdminAccount { get; init; } = string.Empty;
    [ECMAScriptName("email")]
    public string Email { get; init; } = string.Empty;
    [ECMAScriptName("phone")]
    public string Phone { get; init; } = string.Empty;
}

/// <summary>
/// UpdateTenantInput 继承 TenantOutput/SysTenant。更新时回传完整分页行快照，防止隐藏字段被默认值覆盖。
/// </summary>
public sealed record TenantSaveInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("userId")]
    public string UserId { get; init; } = string.Empty;
    [ECMAScriptName("orgId")]
    public string OrgId { get; init; } = string.Empty;
    [ECMAScriptName("host")]
    public string? Host { get; init; }
    [ECMAScriptName("tenantType")]
    public int TenantType { get; init; }
    [ECMAScriptName("dbType")]
    public int DbType { get; init; }
    [ECMAScriptName("connection")]
    public string? Connection { get; init; }
    [ECMAScriptName("configId")]
    public string? ConfigId { get; init; }
    [ECMAScriptName("slaveConnections")]
    public string? SlaveConnections { get; init; }
    [ECMAScriptName("enableReg")]
    public int? EnableReg { get; init; }
    [ECMAScriptName("regWayId")]
    public string? RegWayId { get; init; }
    [ECMAScriptName("logo")]
    public string? Logo { get; init; }
    [ECMAScriptName("logoBase64")]
    public string? LogoBase64 { get; init; }
    [ECMAScriptName("logoFileName")]
    public string? LogoFileName { get; init; }
    [ECMAScriptName("title")]
    public string? Title { get; init; }
    [ECMAScriptName("viceTitle")]
    public string? ViceTitle { get; init; }
    [ECMAScriptName("viceDesc")]
    public string? ViceDesc { get; init; }
    [ECMAScriptName("watermark")]
    public string? Watermark { get; init; }
    [ECMAScriptName("copyright")]
    public string? Copyright { get; init; }
    [ECMAScriptName("icp")]
    public string? Icp { get; init; }
    [ECMAScriptName("icpUrl")]
    public string? IcpUrl { get; init; }
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("remark")]
    public string? Remark { get; init; }
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
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
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("adminAccount")]
    public string AdminAccount { get; init; } = string.Empty;
    [ECMAScriptName("email")]
    public string Email { get; init; } = string.Empty;
    [ECMAScriptName("phone")]
    public string Phone { get; init; } = string.Empty;
}

public sealed record TenantView : VueProps
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string OrgId { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
    [ECMAScriptName("tenantType")]
    public int TenantType { get; init; }
    [ECMAScriptName("dbType")]
    public int DbType { get; init; }
    public string Connection { get; init; } = string.Empty;
    public string ConfigId { get; init; } = string.Empty;
    public string SlaveConnections { get; init; } = string.Empty;
    public int? EnableReg { get; init; }
    public string? RegWayId { get; init; }
    [ECMAScriptName("logo")]
    public string Logo { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string ViceTitle { get; init; } = string.Empty;
    public string ViceDesc { get; init; } = string.Empty;
    public string Watermark { get; init; } = string.Empty;
    public string Copyright { get; init; } = string.Empty;
    public string Icp { get; init; } = string.Empty;
    public string IcpUrl { get; init; } = string.Empty;
    public int OrderNo { get; init; }
    public string Remark { get; init; } = string.Empty;
    [ECMAScriptName("status")]
    public int Status { get; init; }
    public string CreateTime { get; init; } = string.Empty;
    public string? CreateUserId { get; init; }
    public string? CreateUserName { get; init; }
    public string? UpdateTime { get; init; }
    public string? UpdateUserId { get; init; }
    public string? UpdateUserName { get; init; }
    public string Name { get; init; } = string.Empty;
    public string AdminAccount { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
}

public sealed record TenantMenuNode
{
    public string Id { get; init; } = string.Empty;
    public string ParentId { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public int Depth { get; init; }
    public bool HasChildren { get; init; }
}

public sealed record TenantLoginResult
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public string Homepage { get; init; } = string.Empty;
}
