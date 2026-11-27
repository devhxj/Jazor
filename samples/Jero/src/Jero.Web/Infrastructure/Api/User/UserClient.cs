using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.User;

/// <summary>账号管理浏览器 API。所有 long / 雪花 ID 均以字符串跨越 JSON 边界。</summary>
[ECMAScriptModule("components/user-client.js")]
public static class UserClient
{
    public static IPromise<ApiOutcome> Page(UserPageInput input)
    {
        if (input.TenantId.Length > 0)
            return BrowserApiClient.Post("/api/sysUser/page", input);

        return BrowserApiClient.Post("/api/sysUser/page", new UserPageAllInput
        {
            Page = input.Page,
            PageSize = input.PageSize,
            Account = input.Account,
            RealName = input.RealName,
            PosName = input.PosName,
            Phone = input.Phone,
            OrgId = input.OrgId
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
            TenantId = input.TenantId,
            OrgId = input.OrgId,
            Account = input.Account,
            RealName = input.RealName,
            NickName = input.NickName,
            Avatar = input.Avatar,
            Sex = input.Sex,
            Age = input.Age,
            Birthday = input.Birthday,
            Nation = input.Nation,
            Phone = input.Phone,
            CardType = input.CardType,
            IdCardNum = input.IdCardNum,
            Email = input.Email,
            Address = input.Address,
            CultureLevel = input.CultureLevel,
            PoliticalOutlook = input.PoliticalOutlook,
            College = input.College,
            OfficePhone = input.OfficePhone,
            EmergencyContact = input.EmergencyContact,
            EmergencyPhone = input.EmergencyPhone,
            EmergencyAddress = input.EmergencyAddress,
            Introduction = input.Introduction,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            AccountType = input.AccountType,
            ManagerUserId = input.ManagerUserId,
            PosId = input.PosId,
            JobNum = input.JobNum,
            PosLevel = input.PosLevel,
            PosTitle = input.PosTitle,
            Expertise = input.Expertise,
            OfficeZone = input.OfficeZone,
            Office = input.Office,
            JoinDate = input.JoinDate,
            Signature = input.Signature,
            LangCode = input.LangCode,
            Homepage = input.Homepage,
            DomainAccount = input.DomainAccount,
            RoleIdList = input.RoleIdList,
            ExtOrgIdList = input.ExtOrgIdList
        });

    public static IPromise<ApiOutcome> Update(UserSaveInput input)
        => BrowserApiClient.Post("/api/sysUser/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysUser/delete", new UserIdInput { Id = id });

    public static IPromise<ApiOutcome> SetStatus(string id, int status)
        => BrowserApiClient.Post("/api/sysUser/setStatus", new UserStatusInput
        {
            Id = id,
            Status = status
        });

    public static IPromise<ApiOutcome> GrantRole(string userId, string[] roleIds)
        => BrowserApiClient.Post("/api/sysUser/grantRole", new UserGrantRoleInput
        {
            UserId = userId,
            RoleIdList = roleIds
        });

    public static IPromise<ApiOutcome> ResetPassword(string id)
        => BrowserApiClient.Post("/api/sysUser/resetPwd", new UserIdInput { Id = id });

    public static IPromise<ApiOutcome> UnlockLogin(string id)
        => BrowserApiClient.Post("/api/sysUser/unlockLogin", new UserIdInput { Id = id });

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
                Value = ReadString(value, "value"),
                Label = ReadString(value, "label"),
                Host = ReadString(value, "host")
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
                Value = code,
                Label = ReadString(value, "label")
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
                Value = numeric ? ((int)NumberValue(rawValue)).ToString() : rawValue,
                Number = numeric ? (int)NumberValue(rawValue) : 0,
                Label = ReadOptionLabel(value),
                TagType = ReadString(value, "tagType")
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
                Id = ReadOptionalString(value, "id"),
                UserId = ReadOptionalString(value, "userId"),
                OrgId = ReadString(value, "orgId"),
                PosId = ReadString(value, "posId"),
                JobNum = ReadOptionalString(value, "jobNum"),
                PosLevel = ReadOptionalString(value, "posLevel"),
                JoinDate = ReadDate(value, "joinDate")
            };
        }
        return result;
    }

    public static string ReadPassword(object? data)
        => data is null ? string.Empty : NormalizeString(data);

    private static UserView ReadUser(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            TenantId = ReadOptionalString(value, "tenantId"),
            OrgId = ReadString(value, "orgId"),
            CreateTime = ReadString(value, "createTime"),
            UpdateTime = ReadOptionalString(value, "updateTime"),
            CreateUserId = ReadOptionalString(value, "createUserId"),
            CreateUserName = ReadOptionalString(value, "createUserName"),
            UpdateUserId = ReadOptionalString(value, "updateUserId"),
            UpdateUserName = ReadOptionalString(value, "updateUserName"),
            Account = ReadString(value, "account"),
            RealName = ReadString(value, "realName"),
            NickName = ReadOptionalString(value, "nickName"),
            Avatar = ReadOptionalString(value, "avatar"),
            Sex = ReadInt(value, "sex", 1),
            Age = ReadInt(value, "age", 0),
            Birthday = ReadDate(value, "birthday"),
            Nation = ReadOptionalString(value, "nation"),
            Phone = ReadOptionalString(value, "phone"),
            CardType = ReadInt(value, "cardType", 0),
            IdCardNum = ReadOptionalString(value, "idCardNum"),
            Email = ReadOptionalString(value, "email"),
            Address = ReadOptionalString(value, "address"),
            CultureLevel = ReadInt(value, "cultureLevel", 5),
            PoliticalOutlook = ReadOptionalString(value, "politicalOutlook"),
            College = ReadOptionalString(value, "college"),
            OfficePhone = ReadOptionalString(value, "officePhone"),
            EmergencyContact = ReadOptionalString(value, "emergencyContact"),
            EmergencyPhone = ReadOptionalString(value, "emergencyPhone"),
            EmergencyAddress = ReadOptionalString(value, "emergencyAddress"),
            Introduction = ReadOptionalString(value, "introduction"),
            OrderNo = ReadInt(value, "orderNo", 100),
            Status = ReadInt(value, "status", 1),
            Remark = ReadOptionalString(value, "remark"),
            AccountType = ReadInt(value, "accountType", 777),
            ManagerUserId = ReadOptionalString(value, "managerUserId"),
            PosId = ReadString(value, "posId"),
            JobNum = ReadOptionalString(value, "jobNum"),
            PosLevel = ReadOptionalString(value, "posLevel"),
            PosTitle = ReadOptionalString(value, "posTitle"),
            Expertise = ReadOptionalString(value, "expertise"),
            OfficeZone = ReadOptionalString(value, "officeZone"),
            Office = ReadOptionalString(value, "office"),
            JoinDate = ReadDate(value, "joinDate"),
            LastLoginIp = ReadOptionalString(value, "lastLoginIp"),
            LastLoginAddress = ReadOptionalString(value, "lastLoginAddress"),
            LastLoginTime = ReadOptionalString(value, "lastLoginTime"),
            LastLoginDevice = ReadOptionalString(value, "lastLoginDevice"),
            Signature = ReadOptionalString(value, "signature"),
            LangCode = ReadString(value, "langCode"),
            Homepage = ReadOptionalString(value, "homepage"),
            OrgName = ReadString(value, "orgName"),
            PosName = ReadString(value, "posName"),
            RoleName = ReadString(value, "roleName"),
            DomainAccount = ReadString(value, "domainAccount")
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
                Id = id,
                TenantId = ReadString(value, "tenantId"),
                Name = ReadString(value, "name")
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
                Id = ReadString(value, "id"),
                TenantId = ReadString(value, "tenantId"),
                Name = name,
                Path = path,
                Depth = depth,
                Disabled = ReadBool(value, "disabled")
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
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 20;
    [ECMAScriptName("account")]
    public string Account { get; init; } = string.Empty;
    [ECMAScriptName("realName")]
    public string RealName { get; init; } = string.Empty;
    [ECMAScriptName("posName")]
    public string PosName { get; init; } = string.Empty;
    [ECMAScriptName("phone")]
    public string Phone { get; init; } = string.Empty;
    [ECMAScriptName("orgId")]
    public string OrgId { get; init; } = "-1";
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
}

public sealed record UserPageAllInput
{
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 20;
    [ECMAScriptName("account")]
    public string Account { get; init; } = string.Empty;
    [ECMAScriptName("realName")]
    public string RealName { get; init; } = string.Empty;
    [ECMAScriptName("posName")]
    public string PosName { get; init; } = string.Empty;
    [ECMAScriptName("phone")]
    public string Phone { get; init; } = string.Empty;
    [ECMAScriptName("orgId")]
    public string OrgId { get; init; } = "-1";
}

public sealed record UserIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record UserStatusInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("status")]
    public int Status { get; init; }
}

public sealed record UserGrantRoleInput
{
    [ECMAScriptName("userId")]
    public string UserId { get; init; } = string.Empty;
    [ECMAScriptName("roleIdList")]
    public string[] RoleIdList { get; init; } = [];
}

/// <summary>
/// UpdateUserInput 继承完整 SysUser；更新时必须回传分页行的标量快照，避免未展示字段被默认值覆盖。
/// </summary>
public sealed record UserSaveInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string? TenantId { get; init; }
    [ECMAScriptName("orgId")]
    public string OrgId { get; init; } = string.Empty;
    [ECMAScriptName("createTime")]
    public string CreateTime { get; init; } = string.Empty;
    [ECMAScriptName("updateTime")]
    public string? UpdateTime { get; init; }
    [ECMAScriptName("createUserId")]
    public string? CreateUserId { get; init; }
    [ECMAScriptName("createUserName")]
    public string? CreateUserName { get; init; }
    [ECMAScriptName("updateUserId")]
    public string? UpdateUserId { get; init; }
    [ECMAScriptName("updateUserName")]
    public string? UpdateUserName { get; init; }
    [ECMAScriptName("account")]
    public string Account { get; init; } = string.Empty;
    [ECMAScriptName("realName")]
    public string RealName { get; init; } = string.Empty;
    [ECMAScriptName("nickName")]
    public string? NickName { get; init; }
    [ECMAScriptName("avatar")]
    public string? Avatar { get; init; }
    [ECMAScriptName("sex")]
    public int Sex { get; init; } = 1;
    [ECMAScriptName("age")]
    public int Age { get; init; }
    [ECMAScriptName("birthday")]
    public string? Birthday { get; init; }
    [ECMAScriptName("nation")]
    public string? Nation { get; init; }
    [ECMAScriptName("phone")]
    public string? Phone { get; init; }
    [ECMAScriptName("cardType")]
    public int CardType { get; init; }
    [ECMAScriptName("idCardNum")]
    public string? IdCardNum { get; init; }
    [ECMAScriptName("email")]
    public string? Email { get; init; }
    [ECMAScriptName("address")]
    public string? Address { get; init; }
    [ECMAScriptName("cultureLevel")]
    public int CultureLevel { get; init; } = 5;
    [ECMAScriptName("politicalOutlook")]
    public string? PoliticalOutlook { get; init; }
    [ECMAScriptName("college")]
    public string? College { get; init; }
    [ECMAScriptName("officePhone")]
    public string? OfficePhone { get; init; }
    [ECMAScriptName("emergencyContact")]
    public string? EmergencyContact { get; init; }
    [ECMAScriptName("emergencyPhone")]
    public string? EmergencyPhone { get; init; }
    [ECMAScriptName("emergencyAddress")]
    public string? EmergencyAddress { get; init; }
    [ECMAScriptName("introduction")]
    public string? Introduction { get; init; }
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("remark")]
    public string? Remark { get; init; }
    [ECMAScriptName("accountType")]
    public int AccountType { get; init; } = 777;
    [ECMAScriptName("managerUserId")]
    public string? ManagerUserId { get; init; }
    [ECMAScriptName("posId")]
    public string PosId { get; init; } = string.Empty;
    [ECMAScriptName("jobNum")]
    public string? JobNum { get; init; }
    [ECMAScriptName("posLevel")]
    public string? PosLevel { get; init; }
    [ECMAScriptName("posTitle")]
    public string? PosTitle { get; init; }
    [ECMAScriptName("expertise")]
    public string? Expertise { get; init; }
    [ECMAScriptName("officeZone")]
    public string? OfficeZone { get; init; }
    [ECMAScriptName("office")]
    public string? Office { get; init; }
    [ECMAScriptName("joinDate")]
    public string? JoinDate { get; init; }
    [ECMAScriptName("lastLoginIp")]
    public string? LastLoginIp { get; init; }
    [ECMAScriptName("lastLoginAddress")]
    public string? LastLoginAddress { get; init; }
    [ECMAScriptName("lastLoginTime")]
    public string? LastLoginTime { get; init; }
    [ECMAScriptName("lastLoginDevice")]
    public string? LastLoginDevice { get; init; }
    [ECMAScriptName("signature")]
    public string? Signature { get; init; }
    [ECMAScriptName("langCode")]
    public string LangCode { get; init; } = "zh-CN";
    [ECMAScriptName("homepage")]
    public string? Homepage { get; init; }
    [ECMAScriptName("domainAccount")]
    public string? DomainAccount { get; init; }
    [ECMAScriptName("roleIdList")]
    public string[] RoleIdList { get; init; } = [];
    [ECMAScriptName("extOrgIdList")]
    public UserExtOrgInput[] ExtOrgIdList { get; init; } = [];
}

public sealed record UserAddInput
{
    [ECMAScriptName("tenantId")]
    public string? TenantId { get; init; }
    [ECMAScriptName("orgId")]
    public string OrgId { get; init; } = string.Empty;
    [ECMAScriptName("account")]
    public string Account { get; init; } = string.Empty;
    [ECMAScriptName("realName")]
    public string RealName { get; init; } = string.Empty;
    [ECMAScriptName("nickName")]
    public string? NickName { get; init; }
    [ECMAScriptName("avatar")]
    public string? Avatar { get; init; }
    [ECMAScriptName("sex")]
    public int Sex { get; init; } = 1;
    [ECMAScriptName("age")]
    public int Age { get; init; }
    [ECMAScriptName("birthday")]
    public string? Birthday { get; init; }
    [ECMAScriptName("nation")]
    public string? Nation { get; init; }
    [ECMAScriptName("phone")]
    public string? Phone { get; init; }
    [ECMAScriptName("cardType")]
    public int CardType { get; init; }
    [ECMAScriptName("idCardNum")]
    public string? IdCardNum { get; init; }
    [ECMAScriptName("email")]
    public string? Email { get; init; }
    [ECMAScriptName("address")]
    public string? Address { get; init; }
    [ECMAScriptName("cultureLevel")]
    public int CultureLevel { get; init; } = 5;
    [ECMAScriptName("politicalOutlook")]
    public string? PoliticalOutlook { get; init; }
    [ECMAScriptName("college")]
    public string? College { get; init; }
    [ECMAScriptName("officePhone")]
    public string? OfficePhone { get; init; }
    [ECMAScriptName("emergencyContact")]
    public string? EmergencyContact { get; init; }
    [ECMAScriptName("emergencyPhone")]
    public string? EmergencyPhone { get; init; }
    [ECMAScriptName("emergencyAddress")]
    public string? EmergencyAddress { get; init; }
    [ECMAScriptName("introduction")]
    public string? Introduction { get; init; }
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("remark")]
    public string? Remark { get; init; }
    [ECMAScriptName("accountType")]
    public int AccountType { get; init; } = 777;
    [ECMAScriptName("managerUserId")]
    public string? ManagerUserId { get; init; }
    [ECMAScriptName("posId")]
    public string PosId { get; init; } = string.Empty;
    [ECMAScriptName("jobNum")]
    public string? JobNum { get; init; }
    [ECMAScriptName("posLevel")]
    public string? PosLevel { get; init; }
    [ECMAScriptName("posTitle")]
    public string? PosTitle { get; init; }
    [ECMAScriptName("expertise")]
    public string? Expertise { get; init; }
    [ECMAScriptName("officeZone")]
    public string? OfficeZone { get; init; }
    [ECMAScriptName("office")]
    public string? Office { get; init; }
    [ECMAScriptName("joinDate")]
    public string? JoinDate { get; init; }
    [ECMAScriptName("signature")]
    public string? Signature { get; init; }
    [ECMAScriptName("langCode")]
    public string LangCode { get; init; } = "zh-CN";
    [ECMAScriptName("homepage")]
    public string? Homepage { get; init; }
    [ECMAScriptName("domainAccount")]
    public string? DomainAccount { get; init; }
    [ECMAScriptName("roleIdList")]
    public string[] RoleIdList { get; init; } = [];
    [ECMAScriptName("extOrgIdList")]
    public UserExtOrgInput[] ExtOrgIdList { get; init; } = [];
}

public sealed record UserExtOrgInput
{
    [ECMAScriptName("id")]
    public string? Id { get; set; }
    [ECMAScriptName("userId")]
    public string? UserId { get; set; }
    [ECMAScriptName("orgId")]
    public string OrgId { get; set; } = string.Empty;
    [ECMAScriptName("posId")]
    public string PosId { get; set; } = string.Empty;
    [ECMAScriptName("jobNum")]
    public string? JobNum { get; set; }
    [ECMAScriptName("posLevel")]
    public string? PosLevel { get; set; }
    [ECMAScriptName("joinDate")]
    public string? JoinDate { get; set; }
}

public sealed record UserView : VueProps
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    public string? TenantId { get; init; }
    public string OrgId { get; init; } = string.Empty;
    public string CreateTime { get; init; } = string.Empty;
    public string? UpdateTime { get; init; }
    public string? CreateUserId { get; init; }
    public string? CreateUserName { get; init; }
    public string? UpdateUserId { get; init; }
    public string? UpdateUserName { get; init; }
    [ECMAScriptName("account")]
    public string Account { get; init; } = string.Empty;
    [ECMAScriptName("realName")]
    public string RealName { get; init; } = string.Empty;
    [ECMAScriptName("nickName")]
    public string? NickName { get; init; }
    [ECMAScriptName("avatar")]
    public string? Avatar { get; init; }
    public int Sex { get; init; }
    public int Age { get; init; }
    public string? Birthday { get; init; }
    public string? Nation { get; init; }
    public string? Phone { get; init; }
    public int CardType { get; init; }
    public string? IdCardNum { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public int CultureLevel { get; init; }
    public string? PoliticalOutlook { get; init; }
    public string? College { get; init; }
    public string? OfficePhone { get; init; }
    public string? EmergencyContact { get; init; }
    public string? EmergencyPhone { get; init; }
    public string? EmergencyAddress { get; init; }
    public string? Introduction { get; init; }
    public int OrderNo { get; init; }
    [ECMAScriptName("status")]
    public int Status { get; init; }
    public string? Remark { get; init; }
    [ECMAScriptName("accountType")]
    public int AccountType { get; init; }
    public string? ManagerUserId { get; init; }
    public string PosId { get; init; } = string.Empty;
    public string? JobNum { get; init; }
    public string? PosLevel { get; init; }
    public string? PosTitle { get; init; }
    public string? Expertise { get; init; }
    public string? OfficeZone { get; init; }
    public string? Office { get; init; }
    public string? JoinDate { get; init; }
    public string? LastLoginIp { get; init; }
    public string? LastLoginAddress { get; init; }
    public string? LastLoginTime { get; init; }
    public string? LastLoginDevice { get; init; }
    public string? Signature { get; init; }
    public string LangCode { get; init; } = string.Empty;
    public string? Homepage { get; init; }
    public string OrgName { get; init; } = string.Empty;
    public string PosName { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
    public string DomainAccount { get; init; } = string.Empty;
}

public sealed record UserTenantOption : VueProps
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
}

public sealed record UserNamedOption : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

public sealed record UserOrgOption : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public int Depth { get; init; }
    public bool Disabled { get; init; }
}

public sealed record UserLanguageOption : VueProps
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
}

public sealed record UserEnumOption : VueProps
{
    public string Value { get; init; } = string.Empty;
    public int Number { get; init; }
    public string Label { get; init; } = string.Empty;
    public string TagType { get; init; } = string.Empty;
}
