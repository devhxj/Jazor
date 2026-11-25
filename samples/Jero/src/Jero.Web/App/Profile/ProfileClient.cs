using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Profile;

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
            passwordOld = passwordOld,
            passwordNew = passwordNew
        });

    public static ProfileUser? ReadUser(object? data)
    {
        if (data is null)
            return null;

        return new ProfileUser
        {
            id = ReadString(data, "id"),
            tenantId = ReadOptionalString(data, "tenantId"),
            orgId = ReadString(data, "orgId"),
            createTime = ReadString(data, "createTime"),
            updateTime = ReadOptionalString(data, "updateTime"),
            createUserId = ReadOptionalString(data, "createUserId"),
            createUserName = ReadOptionalString(data, "createUserName"),
            updateUserId = ReadOptionalString(data, "updateUserId"),
            updateUserName = ReadOptionalString(data, "updateUserName"),
            account = ReadString(data, "account"),
            realName = ReadString(data, "realName"),
            nickName = ReadOptionalString(data, "nickName"),
            avatar = ReadOptionalString(data, "avatar"),
            sex = ReadInt(data, "sex"),
            age = ReadInt(data, "age"),
            birthday = ReadOptionalString(data, "birthday"),
            nation = ReadOptionalString(data, "nation"),
            phone = ReadOptionalString(data, "phone"),
            cardType = ReadInt(data, "cardType"),
            idCardNum = ReadOptionalString(data, "idCardNum"),
            email = ReadOptionalString(data, "email"),
            address = ReadOptionalString(data, "address"),
            cultureLevel = ReadInt(data, "cultureLevel"),
            politicalOutlook = ReadOptionalString(data, "politicalOutlook"),
            college = ReadOptionalString(data, "college"),
            officePhone = ReadOptionalString(data, "officePhone"),
            emergencyContact = ReadOptionalString(data, "emergencyContact"),
            emergencyPhone = ReadOptionalString(data, "emergencyPhone"),
            emergencyAddress = ReadOptionalString(data, "emergencyAddress"),
            introduction = ReadOptionalString(data, "introduction"),
            orderNo = ReadInt(data, "orderNo"),
            status = ReadInt(data, "status"),
            remark = ReadOptionalString(data, "remark"),
            accountType = ReadInt(data, "accountType"),
            managerUserId = ReadOptionalString(data, "managerUserId"),
            posId = ReadString(data, "posId"),
            jobNum = ReadOptionalString(data, "jobNum"),
            posLevel = ReadOptionalString(data, "posLevel"),
            posTitle = ReadOptionalString(data, "posTitle"),
            expertise = ReadOptionalString(data, "expertise"),
            officeZone = ReadOptionalString(data, "officeZone"),
            office = ReadOptionalString(data, "office"),
            joinDate = ReadOptionalString(data, "joinDate"),
            lastLoginIp = ReadOptionalString(data, "lastLoginIp"),
            lastLoginAddress = ReadOptionalString(data, "lastLoginAddress"),
            lastLoginTime = ReadOptionalString(data, "lastLoginTime"),
            lastLoginDevice = ReadOptionalString(data, "lastLoginDevice"),
            signature = ReadOptionalString(data, "signature"),
            langCode = ReadString(data, "langCode"),
            homepage = ReadOptionalString(data, "homepage")
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
    public string realName { get; set; } = string.Empty;
    public string? nickName { get; init; }
    public string? avatar { get; set; }
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
    public string? introduction { get; set; }
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
    public string? signature { get; set; }
    public string langCode { get; init; } = string.Empty;
    public string? homepage { get; init; }
}

public sealed record ChangePasswordInput
{
    public string passwordOld { get; init; } = string.Empty;
    public string passwordNew { get; init; } = string.Empty;
}
