using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.User;

/// <summary>账号管理浏览器 API。所有 long / 雪花 ID 均以字符串跨越 JSON 边界。</summary>
[ECMAScriptModule("components/user-client.js")]
public static class UserClient
{
    public static IPromise<ApiOutcome> Page(UserPageInput input)
    {
        if (input.tenantId.Length > 0)
            return BrowserApiClient.Post("/api/sysUser/page", input);

        return BrowserApiClient.Post("/api/sysUser/page", new UserPageAllInput
        {
            page = input.page,
            pageSize = input.pageSize,
            account = input.account,
            realName = input.realName,
            posName = input.posName,
            phone = input.phone,
            orgId = input.orgId
        });
    }

    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static IPromise<ApiOutcome> Organizations(string tenantId)
    {
        var path = "/api/sysOrg/tree?Id=0";
        if (tenantId.Length > 0)
            path += "&TenantId=" + EncodeURIComponent(tenantId);
        return BrowserApiClient.Get(path);
    }

    public static IPromise<ApiOutcome> Roles()
        => BrowserApiClient.Get("/api/sysRole/list");

    public static IPromise<ApiOutcome> Positions(string tenantId)
    {
        var path = "/api/sysPos/list";
        if (tenantId.Length > 0)
            path += "?TenantId=" + EncodeURIComponent(tenantId);
        return BrowserApiClient.Get(path);
    }

    public static IPromise<ApiOutcome> Languages()
        => BrowserApiClient.Post("/api/sysLang/dropdownData", new EmptyInput());

    public static IPromise<ApiOutcome> Dict(string code)
        => BrowserApiClient.Get("/api/sysDictType/dataList?code=" + EncodeURIComponent(code));

    public static IPromise<ApiOutcome> OwnRoles(string userId)
        => BrowserApiClient.Get("/api/sysUser/ownRoleList/" + EncodeURIComponent(userId));

    public static IPromise<ApiOutcome> OwnExtOrganizations(string userId)
        => BrowserApiClient.Get("/api/sysUser/ownExtOrgList/" + EncodeURIComponent(userId));

    public static IPromise<ApiOutcome> Add(UserSaveInput input)
        => BrowserApiClient.Post("/api/sysUser/add", new UserAddInput
        {
            tenantId = input.tenantId,
            orgId = input.orgId,
            account = input.account,
            realName = input.realName,
            nickName = input.nickName,
            avatar = input.avatar,
            sex = input.sex,
            age = input.age,
            birthday = input.birthday,
            nation = input.nation,
            phone = input.phone,
            cardType = input.cardType,
            idCardNum = input.idCardNum,
            email = input.email,
            address = input.address,
            cultureLevel = input.cultureLevel,
            politicalOutlook = input.politicalOutlook,
            college = input.college,
            officePhone = input.officePhone,
            emergencyContact = input.emergencyContact,
            emergencyPhone = input.emergencyPhone,
            emergencyAddress = input.emergencyAddress,
            introduction = input.introduction,
            orderNo = input.orderNo,
            status = input.status,
            remark = input.remark,
            accountType = input.accountType,
            managerUserId = input.managerUserId,
            posId = input.posId,
            jobNum = input.jobNum,
            posLevel = input.posLevel,
            posTitle = input.posTitle,
            expertise = input.expertise,
            officeZone = input.officeZone,
            office = input.office,
            joinDate = input.joinDate,
            signature = input.signature,
            langCode = input.langCode,
            homepage = input.homepage,
            domainAccount = input.domainAccount,
            roleIdList = input.roleIdList,
            extOrgIdList = input.extOrgIdList
        });

    public static IPromise<ApiOutcome> Update(UserSaveInput input)
        => BrowserApiClient.Post("/api/sysUser/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysUser/delete", new UserIdInput { id = id });

    public static IPromise<ApiOutcome> SetStatus(string id, int status)
        => BrowserApiClient.Post("/api/sysUser/setStatus", new UserStatusInput
        {
            id = id,
            status = status
        });

    public static IPromise<ApiOutcome> GrantRole(string userId, string[] roleIds)
        => BrowserApiClient.Post("/api/sysUser/grantRole", new UserGrantRoleInput
        {
            userId = userId,
            roleIdList = roleIds
        });

    public static IPromise<ApiOutcome> ResetPassword(string id)
        => BrowserApiClient.Post("/api/sysUser/resetPwd", new UserIdInput { id = id });

    public static IPromise<ApiOutcome> UnlockLogin(string id)
        => BrowserApiClient.Post("/api/sysUser/unlockLogin", new UserIdInput { id = id });

    public static UserView[] ReadUsers(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new UserView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadUser(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static UserTenantOption[] ReadTenants(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new UserTenantOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new UserTenantOption
            {
                value = ReadString(value, "value"),
                label = ReadString(value, "label"),
                host = ReadString(value, "host")
            };
        }
        return result;
    }

    public static UserNamedOption[] ReadRoles(object? data, string tenantId)
        => ReadNamedOptions(data, tenantId);

    public static UserNamedOption[] ReadPositions(object? data, string tenantId)
        => ReadNamedOptions(data, tenantId);

    public static UserOrgOption[] ReadOrganizations(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new UserOrgOption[CountOrganizations(values)];
        FillOrganizations(values, result, 0, string.Empty, 0);
        return result;
    }

    public static UserLanguageOption[] ReadLanguages(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new UserLanguageOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            var code = ReadString(value, "code");
            if (code.Length == 0)
                code = ReadString(value, "value");
            result[index] = new UserLanguageOption
            {
                value = code,
                label = ReadString(value, "label")
            };
        }
        return result;
    }

    public static UserEnumOption[] ReadEnumOptions(object? data, bool numeric)
    {
        var values = data as object?[] ?? [];
        var result = new UserEnumOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            var rawValue = ReadString(value, "value");
            result[index] = new UserEnumOption
            {
                value = numeric ? ((int)NumberValue(rawValue)).ToString() : rawValue,
                number = numeric ? (int)NumberValue(rawValue) : 0,
                label = ReadOptionLabel(value),
                tagType = ReadString(value, "tagType")
            };
        }
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

    public static UserExtOrgInput[] ReadExtOrganizations(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new UserExtOrgInput[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new UserExtOrgInput
            {
                id = ReadOptionalString(value, "id"),
                userId = ReadOptionalString(value, "userId"),
                orgId = ReadString(value, "orgId"),
                posId = ReadString(value, "posId"),
                jobNum = ReadOptionalString(value, "jobNum"),
                posLevel = ReadOptionalString(value, "posLevel"),
                joinDate = ReadDate(value, "joinDate")
            };
        }
        return result;
    }

    public static string ReadPassword(object? data)
        => data is null ? string.Empty : NormalizeString(data);

    private static UserView ReadUser(object value)
        => new()
        {
            id = ReadString(value, "id"),
            tenantId = ReadOptionalString(value, "tenantId"),
            orgId = ReadString(value, "orgId"),
            createTime = ReadString(value, "createTime"),
            updateTime = ReadOptionalString(value, "updateTime"),
            createUserId = ReadOptionalString(value, "createUserId"),
            createUserName = ReadOptionalString(value, "createUserName"),
            updateUserId = ReadOptionalString(value, "updateUserId"),
            updateUserName = ReadOptionalString(value, "updateUserName"),
            account = ReadString(value, "account"),
            realName = ReadString(value, "realName"),
            nickName = ReadOptionalString(value, "nickName"),
            avatar = ReadOptionalString(value, "avatar"),
            sex = ReadInt(value, "sex", 1),
            age = ReadInt(value, "age", 0),
            birthday = ReadDate(value, "birthday"),
            nation = ReadOptionalString(value, "nation"),
            phone = ReadOptionalString(value, "phone"),
            cardType = ReadInt(value, "cardType", 0),
            idCardNum = ReadOptionalString(value, "idCardNum"),
            email = ReadOptionalString(value, "email"),
            address = ReadOptionalString(value, "address"),
            cultureLevel = ReadInt(value, "cultureLevel", 5),
            politicalOutlook = ReadOptionalString(value, "politicalOutlook"),
            college = ReadOptionalString(value, "college"),
            officePhone = ReadOptionalString(value, "officePhone"),
            emergencyContact = ReadOptionalString(value, "emergencyContact"),
            emergencyPhone = ReadOptionalString(value, "emergencyPhone"),
            emergencyAddress = ReadOptionalString(value, "emergencyAddress"),
            introduction = ReadOptionalString(value, "introduction"),
            orderNo = ReadInt(value, "orderNo", 100),
            status = ReadInt(value, "status", 1),
            remark = ReadOptionalString(value, "remark"),
            accountType = ReadInt(value, "accountType", 777),
            managerUserId = ReadOptionalString(value, "managerUserId"),
            posId = ReadString(value, "posId"),
            jobNum = ReadOptionalString(value, "jobNum"),
            posLevel = ReadOptionalString(value, "posLevel"),
            posTitle = ReadOptionalString(value, "posTitle"),
            expertise = ReadOptionalString(value, "expertise"),
            officeZone = ReadOptionalString(value, "officeZone"),
            office = ReadOptionalString(value, "office"),
            joinDate = ReadDate(value, "joinDate"),
            lastLoginIp = ReadOptionalString(value, "lastLoginIp"),
            lastLoginAddress = ReadOptionalString(value, "lastLoginAddress"),
            lastLoginTime = ReadOptionalString(value, "lastLoginTime"),
            lastLoginDevice = ReadOptionalString(value, "lastLoginDevice"),
            signature = ReadOptionalString(value, "signature"),
            langCode = ReadString(value, "langCode"),
            homepage = ReadOptionalString(value, "homepage"),
            orgName = ReadString(value, "orgName"),
            posName = ReadString(value, "posName"),
            roleName = ReadString(value, "roleName"),
            domainAccount = ReadString(value, "domainAccount")
        };

    private static UserNamedOption[] ReadNamedOptions(object? data, string tenantId)
    {
        var values = data as object?[] ?? [];
        var count = 0;
        for (var index = 0; index < values.Length; index++)
            if (ReadString(values[index]!, "id").Length > 0 && MatchesTenant(values[index]!, tenantId))
                count++;

        var result = new UserNamedOption[count];
        var target = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            var id = ReadString(value, "id");
            if (id.Length == 0 || !MatchesTenant(value, tenantId))
                continue;

            result[target] = new UserNamedOption
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
    {
        var optionTenantId = ReadString(value, "tenantId");
        return tenantId.Length == 0 || optionTenantId.Length == 0 || optionTenantId == tenantId;
    }

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
        UserOrgOption[] result,
        int target,
        string parentPath,
        int depth)
    {
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            var name = ReadString(value, "name");
            var path = parentPath.Length == 0 ? name : parentPath + " / " + name;
            result[target] = new UserOrgOption
            {
                id = ReadString(value, "id"),
                tenantId = ReadString(value, "tenantId"),
                name = name,
                path = path,
                depth = depth,
                disabled = ReadBool(value, "disabled")
            };
            target++;

            var children = Reflect.Get(value, "children") as object?[] ?? [];
            target = FillOrganizations(children, result, target, path, depth + 1);
        }
        return target;
    }

    private static string ReadOptionLabel(object value)
    {
        var label = ReadString(value, "label");
        return label.Length > 0 ? label : ReadString(value, "name");
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

    private static string? ReadDate(object data, string key)
    {
        var value = ReadOptionalString(data, key);
        if (value is null)
            return null;
        return value.Length > 10 ? value.Substring(0, 10) : value;
    }

    private static string NormalizeString(object value)
    {
        var text = StringValue(value);
        return text == "null" || text == "undefined" ? string.Empty : text;
    }

    private static int ReadInt(object data, string key, int fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : (int)NumberValue(value);
    }

    private static bool ReadBool(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is not null && BooleanValue(value);
    }
}

public sealed record EmptyInput;

public sealed record UserPageInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 20;
    public string account { get; init; } = string.Empty;
    public string realName { get; init; } = string.Empty;
    public string posName { get; init; } = string.Empty;
    public string phone { get; init; } = string.Empty;
    public string orgId { get; init; } = "-1";
    public string tenantId { get; init; } = string.Empty;
}

public sealed record UserPageAllInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 20;
    public string account { get; init; } = string.Empty;
    public string realName { get; init; } = string.Empty;
    public string posName { get; init; } = string.Empty;
    public string phone { get; init; } = string.Empty;
    public string orgId { get; init; } = "-1";
}

public sealed record UserIdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed record UserStatusInput
{
    public string id { get; init; } = string.Empty;
    public int status { get; init; }
}

public sealed record UserGrantRoleInput
{
    public string userId { get; init; } = string.Empty;
    public string[] roleIdList { get; init; } = [];
}

/// <summary>
/// UpdateUserInput 继承完整 SysUser；更新时必须回传分页行的标量快照，避免未展示字段被默认值覆盖。
/// </summary>
public sealed record UserSaveInput
{
    public string id { get; init; } = string.Empty;
    public string? tenantId { get; init; }
    public string orgId { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string? updateTime { get; init; }
    public string? createUserId { get; init; }
    public string? createUserName { get; init; }
    public string? updateUserId { get; init; }
    public string? updateUserName { get; init; }
    public string account { get; init; } = string.Empty;
    public string realName { get; init; } = string.Empty;
    public string? nickName { get; init; }
    public string? avatar { get; init; }
    public int sex { get; init; } = 1;
    public int age { get; init; }
    public string? birthday { get; init; }
    public string? nation { get; init; }
    public string? phone { get; init; }
    public int cardType { get; init; }
    public string? idCardNum { get; init; }
    public string? email { get; init; }
    public string? address { get; init; }
    public int cultureLevel { get; init; } = 5;
    public string? politicalOutlook { get; init; }
    public string? college { get; init; }
    public string? officePhone { get; init; }
    public string? emergencyContact { get; init; }
    public string? emergencyPhone { get; init; }
    public string? emergencyAddress { get; init; }
    public string? introduction { get; init; }
    public int orderNo { get; init; } = 100;
    public int status { get; init; } = 1;
    public string? remark { get; init; }
    public int accountType { get; init; } = 777;
    public string? managerUserId { get; init; }
    public string posId { get; init; } = string.Empty;
    public string? jobNum { get; init; }
    public string? posLevel { get; init; }
    public string? posTitle { get; init; }
    public string? expertise { get; init; }
    public string? officeZone { get; init; }
    public string? office { get; init; }
    public string? joinDate { get; init; }
    public string? lastLoginIp { get; init; }
    public string? lastLoginAddress { get; init; }
    public string? lastLoginTime { get; init; }
    public string? lastLoginDevice { get; init; }
    public string? signature { get; init; }
    public string langCode { get; init; } = "zh-CN";
    public string? homepage { get; init; }
    public string? domainAccount { get; init; }
    public string[] roleIdList { get; init; } = [];
    public UserExtOrgInput[] extOrgIdList { get; init; } = [];
}

public sealed record UserAddInput
{
    public string? tenantId { get; init; }
    public string orgId { get; init; } = string.Empty;
    public string account { get; init; } = string.Empty;
    public string realName { get; init; } = string.Empty;
    public string? nickName { get; init; }
    public string? avatar { get; init; }
    public int sex { get; init; } = 1;
    public int age { get; init; }
    public string? birthday { get; init; }
    public string? nation { get; init; }
    public string? phone { get; init; }
    public int cardType { get; init; }
    public string? idCardNum { get; init; }
    public string? email { get; init; }
    public string? address { get; init; }
    public int cultureLevel { get; init; } = 5;
    public string? politicalOutlook { get; init; }
    public string? college { get; init; }
    public string? officePhone { get; init; }
    public string? emergencyContact { get; init; }
    public string? emergencyPhone { get; init; }
    public string? emergencyAddress { get; init; }
    public string? introduction { get; init; }
    public int orderNo { get; init; } = 100;
    public int status { get; init; } = 1;
    public string? remark { get; init; }
    public int accountType { get; init; } = 777;
    public string? managerUserId { get; init; }
    public string posId { get; init; } = string.Empty;
    public string? jobNum { get; init; }
    public string? posLevel { get; init; }
    public string? posTitle { get; init; }
    public string? expertise { get; init; }
    public string? officeZone { get; init; }
    public string? office { get; init; }
    public string? joinDate { get; init; }
    public string? signature { get; init; }
    public string langCode { get; init; } = "zh-CN";
    public string? homepage { get; init; }
    public string? domainAccount { get; init; }
    public string[] roleIdList { get; init; } = [];
    public UserExtOrgInput[] extOrgIdList { get; init; } = [];
}

public sealed record UserExtOrgInput
{
    public string? id { get; set; }
    public string? userId { get; set; }
    public string orgId { get; set; } = string.Empty;
    public string posId { get; set; } = string.Empty;
    public string? jobNum { get; set; }
    public string? posLevel { get; set; }
    public string? joinDate { get; set; }
}

public sealed record UserView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string? tenantId { get; init; }
    public string orgId { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string? updateTime { get; init; }
    public string? createUserId { get; init; }
    public string? createUserName { get; init; }
    public string? updateUserId { get; init; }
    public string? updateUserName { get; init; }
    public string account { get; init; } = string.Empty;
    public string realName { get; init; } = string.Empty;
    public string? nickName { get; init; }
    public string? avatar { get; init; }
    public int sex { get; init; }
    public int age { get; init; }
    public string? birthday { get; init; }
    public string? nation { get; init; }
    public string? phone { get; init; }
    public int cardType { get; init; }
    public string? idCardNum { get; init; }
    public string? email { get; init; }
    public string? address { get; init; }
    public int cultureLevel { get; init; }
    public string? politicalOutlook { get; init; }
    public string? college { get; init; }
    public string? officePhone { get; init; }
    public string? emergencyContact { get; init; }
    public string? emergencyPhone { get; init; }
    public string? emergencyAddress { get; init; }
    public string? introduction { get; init; }
    public int orderNo { get; init; }
    public int status { get; init; }
    public string? remark { get; init; }
    public int accountType { get; init; }
    public string? managerUserId { get; init; }
    public string posId { get; init; } = string.Empty;
    public string? jobNum { get; init; }
    public string? posLevel { get; init; }
    public string? posTitle { get; init; }
    public string? expertise { get; init; }
    public string? officeZone { get; init; }
    public string? office { get; init; }
    public string? joinDate { get; init; }
    public string? lastLoginIp { get; init; }
    public string? lastLoginAddress { get; init; }
    public string? lastLoginTime { get; init; }
    public string? lastLoginDevice { get; init; }
    public string? signature { get; init; }
    public string langCode { get; init; } = string.Empty;
    public string? homepage { get; init; }
    public string orgName { get; init; } = string.Empty;
    public string posName { get; init; } = string.Empty;
    public string roleName { get; init; } = string.Empty;
    public string domainAccount { get; init; } = string.Empty;
}

public sealed record UserTenantOption : VueProps
{
    public string value { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string host { get; init; } = string.Empty;
}

public sealed record UserNamedOption : VueProps
{
    public string id { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
}

public sealed record UserOrgOption : VueProps
{
    public string id { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string path { get; init; } = string.Empty;
    public int depth { get; init; }
    public bool disabled { get; init; }
}

public sealed record UserLanguageOption : VueProps
{
    public string value { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
}

public sealed record UserEnumOption : VueProps
{
    public string value { get; init; } = string.Empty;
    public int number { get; init; }
    public string label { get; init; } = string.Empty;
    public string tagType { get; init; } = string.Empty;
}
