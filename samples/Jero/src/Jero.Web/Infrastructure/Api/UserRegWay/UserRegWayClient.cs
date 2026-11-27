using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.UserRegWay;

/// <summary>维护用户注册方式，并加载租户、角色、组织及岗位选项。</summary>
[ECMAScriptModule("components/user-reg-way-client.js")]
public static class UserRegWayClient
{
    public static IPromise<ApiOutcome> List(UserRegWayListInput input)
    {
        if (input.TenantId.Length > 0)
            return BrowserApiClient.Post("/api/sysUserRegWay/list", input);

        return BrowserApiClient.Post("/api/sysUserRegWay/list", new UserRegWayListAllInput
        {
            Keyword = input.Keyword,
            Name = input.Name,
            Field = input.Field,
            Order = input.Order,
            DescStr = input.DescStr
        });
    }

    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static IPromise<ApiOutcome> AccountTypes()
        => BrowserApiClient.Get("/api/sysDictType/dataList?code=AccountTypeEnum");

    public static IPromise<ApiOutcome> Roles()
        => BrowserApiClient.Get("/api/sysRole/list");

    public static IPromise<ApiOutcome> Organizations(string tenantId)
        => BrowserApiClient.Get("/api/sysOrg/tree?Id=0&TenantId=" + EncodeURIComponent(tenantId));

    public static IPromise<ApiOutcome> Positions(string tenantId)
        => BrowserApiClient.Get("/api/sysPos/list?TenantId=" + EncodeURIComponent(tenantId));

    public static IPromise<ApiOutcome> Add(UserRegWaySaveInput input)
        => BrowserApiClient.Post("/api/sysUserRegWay/add", new UserRegWayAddInput
        {
            TenantId = input.TenantId,
            Name = input.Name,
            AccountType = input.AccountType,
            RoleId = input.RoleId,
            OrgId = input.OrgId,
            PosId = input.PosId,
            OrderNo = input.OrderNo,
            Remark = input.Remark
        });

    public static IPromise<ApiOutcome> Update(UserRegWaySaveInput input)
        => BrowserApiClient.Post("/api/sysUserRegWay/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysUserRegWay/delete", new UserRegWayIdInput { Id = id });

    public static UserRegWayView[] ReadItems(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new UserRegWayView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadItem(values[index]!);
        return result;
    }

    public static UserRegWayTenantOption[] ReadTenants(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new UserRegWayTenantOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new UserRegWayTenantOption
            {
                Value = ReadString(value, "value"),
                Label = ReadString(value, "label"),
                Host = ReadString(value, "host")
            };
        }
        return result;
    }

    public static UserRegWayNamedOption[] ReadRoles(object? data, string tenantId)
        => ReadNamedOptions(data, tenantId);

    public static UserRegWayNamedOption[] ReadPositions(object? data, string tenantId)
        => ReadNamedOptions(data, tenantId);

    public static UserRegWayNamedOption[] ReadOrganizations(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new UserRegWayNamedOption[CountOrganizations(values)];
        FillOrganizations(values, result, 0, string.Empty);
        return result;
    }

    public static UserRegWayAccountTypeOption[] ReadAccountTypes(object? data)
    {
        var values = data as object?[] ?? [];
        var validCount = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var accountType = ReadInt(values[index]!, "value", 0);
            if (accountType > 0 && accountType != 888 && accountType != 999)
                validCount++;
        }

        var result = new UserRegWayAccountTypeOption[validCount];
        var target = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            var accountType = ReadInt(value, "value", 0);
            if (accountType <= 0 || accountType == 888 || accountType == 999)
                continue;

            var label = ReadString(value, "label");
            if (label.Length == 0)
                label = ReadString(value, "name");

            result[target] = new UserRegWayAccountTypeOption
            {
                Value = accountType,
                Label = label.Length == 0 ? "类型" + accountType : label,
                TagType = ReadString(value, "tagType")
            };
            target++;
        }
        return result;
    }

    private static UserRegWayNamedOption[] ReadNamedOptions(object? data, string tenantId)
    {
        var values = data as object?[] ?? [];
        var validCount = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            if (ReadString(value, "id").Length > 0 && MatchesTenant(value, tenantId))
                validCount++;
        }

        var result = new UserRegWayNamedOption[validCount];
        var target = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            var id = ReadString(value, "id");
            if (id.Length == 0 || !MatchesTenant(value, tenantId))
                continue;

            result[target] = new UserRegWayNamedOption
            {
                Id = id,
                TenantId = ReadString(value, "tenantId"),
                Name = ReadString(value, "name")
            };
            target++;
        }
        return result;
    }

    private static bool MatchesTenant(object value, string tenantId)
        => tenantId.Length == 0 || ReadString(value, "tenantId") == tenantId;

    private static int CountOrganizations(object?[] values)
    {
        var count = values.Length;
        for (var index = 0; index < values.Length; index++)
        {
            var children = Reflect.Get(values[index]!, "children") as object?[] ?? [];
            count += CountOrganizations(children);
        }
        return count;
    }

    private static int FillOrganizations(
        object?[] values,
        UserRegWayNamedOption[] result,
        int target,
        string parentPath)
    {
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            var name = ReadString(value, "name");
            var path = parentPath.Length == 0 ? name : parentPath + " / " + name;
            result[target] = new UserRegWayNamedOption
            {
                Id = ReadString(value, "id"),
                TenantId = ReadString(value, "tenantId"),
                Name = path
            };
            target++;

            var children = Reflect.Get(value, "children") as object?[] ?? [];
            target = FillOrganizations(children, result, target, path);
        }
        return target;
    }

    private static UserRegWayView ReadItem(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            TenantId = ReadString(value, "tenantId"),
            Name = ReadString(value, "name"),
            AccountType = ReadInt(value, "accountType", 777),
            RoleId = ReadString(value, "roleId"),
            RoleName = ReadString(value, "roleName"),
            OrgId = ReadString(value, "orgId"),
            OrgName = ReadString(value, "orgName"),
            PosId = ReadString(value, "posId"),
            PosName = ReadString(value, "posName"),
            OrderNo = ReadInt(value, "orderNo", 100),
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
        return value is null ? string.Empty : StringValue(value);
    }

    private static int ReadInt(object data, string key, int fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : (int)NumberValue(value);
    }
}

public sealed record UserRegWayListInput
{
    [ECMAScriptName("keyword")]
    public string Keyword { get; init; } = string.Empty;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
    [ECMAScriptName("field")]
    public string Field { get; init; } = "orderNo";
    [ECMAScriptName("order")]
    public string Order { get; init; } = "ascending";
    [ECMAScriptName("descStr")]
    public string DescStr { get; init; } = "descending";
}

public sealed record UserRegWayListAllInput
{
    [ECMAScriptName("keyword")]
    public string Keyword { get; init; } = string.Empty;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("field")]
    public string Field { get; init; } = "orderNo";
    [ECMAScriptName("order")]
    public string Order { get; init; } = "ascending";
    [ECMAScriptName("descStr")]
    public string DescStr { get; init; } = "descending";
}

public sealed record UserRegWaySaveInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("accountType")]
    public int AccountType { get; init; } = 777;
    [ECMAScriptName("roleId")]
    public string RoleId { get; init; } = string.Empty;
    [ECMAScriptName("orgId")]
    public string OrgId { get; init; } = string.Empty;
    [ECMAScriptName("posId")]
    public string PosId { get; init; } = string.Empty;
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
}

public sealed record UserRegWayAddInput
{
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("accountType")]
    public int AccountType { get; init; } = 777;
    [ECMAScriptName("roleId")]
    public string RoleId { get; init; } = string.Empty;
    [ECMAScriptName("orgId")]
    public string OrgId { get; init; } = string.Empty;
    [ECMAScriptName("posId")]
    public string PosId { get; init; } = string.Empty;
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
}

public sealed record UserRegWayIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record UserRegWayView : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int AccountType { get; init; }
    public string RoleId { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
    public string OrgId { get; init; } = string.Empty;
    public string OrgName { get; init; } = string.Empty;
    public string PosId { get; init; } = string.Empty;
    public string PosName { get; init; } = string.Empty;
    public int OrderNo { get; init; }
    public string Remark { get; init; } = string.Empty;
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
}

public sealed record UserRegWayTenantOption : VueProps
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
}

public sealed record UserRegWayNamedOption : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

public sealed record UserRegWayAccountTypeOption : VueProps
{
    public int Value { get; init; }
    public string Label { get; init; } = string.Empty;
    public string TagType { get; init; } = string.Empty;
}
