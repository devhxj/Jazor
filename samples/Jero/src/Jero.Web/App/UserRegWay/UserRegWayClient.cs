using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.UserRegWay;

/// <summary>Typed browser boundary for user registration plan management.</summary>
[ECMAScriptModule("components/user-reg-way-client.js")]
public static class UserRegWayClient
{
    public static IPromise<ApiOutcome> List(UserRegWayListInput input)
    {
        if (input.tenantId.Length > 0)
            return BrowserApiClient.Post("/api/sysUserRegWay/list", input);

        return BrowserApiClient.Post("/api/sysUserRegWay/list", new UserRegWayListAllInput
        {
            keyword = input.keyword,
            name = input.name,
            field = input.field,
            order = input.order,
            descStr = input.descStr
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
            tenantId = input.tenantId,
            name = input.name,
            accountType = input.accountType,
            roleId = input.roleId,
            orgId = input.orgId,
            posId = input.posId,
            orderNo = input.orderNo,
            remark = input.remark
        });

    public static IPromise<ApiOutcome> Update(UserRegWaySaveInput input)
        => BrowserApiClient.Post("/api/sysUserRegWay/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysUserRegWay/delete", new UserRegWayIdInput { id = id });

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
                value = ReadString(value, "value"),
                label = ReadString(value, "label"),
                host = ReadString(value, "host")
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
                value = accountType,
                label = label.Length == 0 ? "类型" + accountType : label,
                tagType = ReadString(value, "tagType")
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
                id = id,
                tenantId = ReadString(value, "tenantId"),
                name = ReadString(value, "name")
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
                id = ReadString(value, "id"),
                tenantId = ReadString(value, "tenantId"),
                name = path
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
            id = ReadString(value, "id"),
            tenantId = ReadString(value, "tenantId"),
            name = ReadString(value, "name"),
            accountType = ReadInt(value, "accountType", 777),
            roleId = ReadString(value, "roleId"),
            roleName = ReadString(value, "roleName"),
            orgId = ReadString(value, "orgId"),
            orgName = ReadString(value, "orgName"),
            posId = ReadString(value, "posId"),
            posName = ReadString(value, "posName"),
            orderNo = ReadInt(value, "orderNo", 100),
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
    public string keyword { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
    public string field { get; init; } = "orderNo";
    public string order { get; init; } = "ascending";
    public string descStr { get; init; } = "descending";
}

public sealed record UserRegWayListAllInput
{
    public string keyword { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string field { get; init; } = "orderNo";
    public string order { get; init; } = "ascending";
    public string descStr { get; init; } = "descending";
}

public sealed record UserRegWaySaveInput
{
    public string id { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public int accountType { get; init; } = 777;
    public string roleId { get; init; } = string.Empty;
    public string orgId { get; init; } = string.Empty;
    public string posId { get; init; } = string.Empty;
    public int orderNo { get; init; } = 100;
    public string remark { get; init; } = string.Empty;
}

public sealed record UserRegWayAddInput
{
    public string tenantId { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public int accountType { get; init; } = 777;
    public string roleId { get; init; } = string.Empty;
    public string orgId { get; init; } = string.Empty;
    public string posId { get; init; } = string.Empty;
    public int orderNo { get; init; } = 100;
    public string remark { get; init; } = string.Empty;
}

public sealed record UserRegWayIdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed record UserRegWayView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public int accountType { get; init; }
    public string roleId { get; init; } = string.Empty;
    public string roleName { get; init; } = string.Empty;
    public string orgId { get; init; } = string.Empty;
    public string orgName { get; init; } = string.Empty;
    public string posId { get; init; } = string.Empty;
    public string posName { get; init; } = string.Empty;
    public int orderNo { get; init; }
    public string remark { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
}

public sealed record UserRegWayTenantOption : VueProps
{
    public string value { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string host { get; init; } = string.Empty;
}

public sealed record UserRegWayNamedOption : VueProps
{
    public string id { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
}

public sealed record UserRegWayAccountTypeOption : VueProps
{
    public int value { get; init; }
    public string label { get; init; } = string.Empty;
    public string tagType { get; init; } = string.Empty;
}
