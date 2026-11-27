using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Profile;

/// <summary>个人中心 JSON API。雪花 ID 在浏览器边界始终保留为字符串。</summary>
[ECMAScriptModule("components/profile-client.js")]
public static class ProfileClient
{
    public static IPromise<ApiOutcome> BaseInfo()
        => BrowserApiClient.Get("/api/sysUser/baseInfo");

    public static IPromise<ApiOutcome> UpdateBaseInfo(ProfileUser input)
        => BrowserApiClient.Post("/api/sysUser/baseInfo", input);

    public static IPromise<ApiOutcome> ChangePassword(string passwordOld, string passwordNew)
        => BrowserApiClient.Post("/api/sysUser/changePwd", new ChangePasswordInput
        {
            PasswordOld = passwordOld,
            PasswordNew = passwordNew
        });

    public static ProfileUser? ReadUser(object? data)
    {
        if (data is null)
            return null;

        return new ProfileUser
        {
            Id = ReadString(data, "id"),
            TenantId = ReadOptionalString(data, "tenantId"),
            OrgId = ReadString(data, "orgId"),
            CreateTime = ReadString(data, "createTime"),
            UpdateTime = ReadOptionalString(data, "updateTime"),
            CreateUserId = ReadOptionalString(data, "createUserId"),
            CreateUserName = ReadOptionalString(data, "createUserName"),
            UpdateUserId = ReadOptionalString(data, "updateUserId"),
            UpdateUserName = ReadOptionalString(data, "updateUserName"),
            Account = ReadString(data, "account"),
            RealName = ReadString(data, "realName"),
            NickName = ReadOptionalString(data, "nickName"),
            Avatar = ReadOptionalString(data, "avatar"),
            Sex = ReadInt(data, "sex"),
            Age = ReadInt(data, "age"),
            Birthday = ReadOptionalString(data, "birthday"),
            Nation = ReadOptionalString(data, "nation"),
            Phone = ReadOptionalString(data, "phone"),
            CardType = ReadInt(data, "cardType"),
            IdCardNum = ReadOptionalString(data, "idCardNum"),
            Email = ReadOptionalString(data, "email"),
            Address = ReadOptionalString(data, "address"),
            CultureLevel = ReadInt(data, "cultureLevel"),
            PoliticalOutlook = ReadOptionalString(data, "politicalOutlook"),
            College = ReadOptionalString(data, "college"),
            OfficePhone = ReadOptionalString(data, "officePhone"),
            EmergencyContact = ReadOptionalString(data, "emergencyContact"),
            EmergencyPhone = ReadOptionalString(data, "emergencyPhone"),
            EmergencyAddress = ReadOptionalString(data, "emergencyAddress"),
            Introduction = ReadOptionalString(data, "introduction"),
            OrderNo = ReadInt(data, "orderNo"),
            Status = ReadInt(data, "status"),
            Remark = ReadOptionalString(data, "remark"),
            AccountType = ReadInt(data, "accountType"),
            ManagerUserId = ReadOptionalString(data, "managerUserId"),
            PosId = ReadString(data, "posId"),
            JobNum = ReadOptionalString(data, "jobNum"),
            PosLevel = ReadOptionalString(data, "posLevel"),
            PosTitle = ReadOptionalString(data, "posTitle"),
            Expertise = ReadOptionalString(data, "expertise"),
            OfficeZone = ReadOptionalString(data, "officeZone"),
            Office = ReadOptionalString(data, "office"),
            JoinDate = ReadOptionalString(data, "joinDate"),
            LastLoginIp = ReadOptionalString(data, "lastLoginIp"),
            LastLoginAddress = ReadOptionalString(data, "lastLoginAddress"),
            LastLoginTime = ReadOptionalString(data, "lastLoginTime"),
            LastLoginDevice = ReadOptionalString(data, "lastLoginDevice"),
            Signature = ReadOptionalString(data, "signature"),
            LangCode = ReadString(data, "langCode"),
            Homepage = ReadOptionalString(data, "homepage")
        };
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
        return text == "null" || text == "undefined" ? null : text;
    }

    private static string NormalizeString(object value)
    {
        var text = StringValue(value);
        return text == "null" || text == "undefined" ? string.Empty : text;
    }

    private static int ReadInt(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? 0 : (int)NumberValue(value);
    }
}

/// <summary>
/// UpdateBaseInfo 接收 SysUser 实体并更新除少数字段外的整行；因此保存时必须回传已加载的
/// 完整标量快照，不能只发送当前表单的两个字段，否则未展示资料会被默认值覆盖。
/// </summary>
public sealed record ProfileUser
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
    public string RealName { get; set; } = string.Empty;
    [ECMAScriptName("nickName")]
    public string? NickName { get; init; }
    [ECMAScriptName("avatar")]
    public string? Avatar { get; set; }
    [ECMAScriptName("sex")]
    public int Sex { get; init; }
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
    public int CultureLevel { get; init; }
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
    public string? Introduction { get; set; }
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; }
    [ECMAScriptName("status")]
    public int Status { get; init; }
    [ECMAScriptName("remark")]
    public string? Remark { get; init; }
    [ECMAScriptName("accountType")]
    public int AccountType { get; init; }
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
    public string? Signature { get; set; }
    [ECMAScriptName("langCode")]
    public string LangCode { get; init; } = string.Empty;
    [ECMAScriptName("homepage")]
    public string? Homepage { get; init; }
}

public sealed record ChangePasswordInput
{
    [ECMAScriptName("passwordOld")]
    public string PasswordOld { get; init; } = string.Empty;
    [ECMAScriptName("passwordNew")]
    public string PasswordNew { get; init; } = string.Empty;
}
