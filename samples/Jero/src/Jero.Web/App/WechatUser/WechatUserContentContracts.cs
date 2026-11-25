using ECMAScript;
using static ECMAScript.Vue;

namespace Jero.Web.App.WechatUser;

[ECMAScript]
public interface IWechatUserContentState
{
    string AvatarSourceForView(VueProps? row);
    bool CanDelete { get; }
    bool CanEdit { get; }
    void Delete(VueProps? value);
    string deletingId { get; }
    string DisplayRowValueForView(VueProps? row, string key);
    string GenderTagForView(VueProps? row);
    string GenderTextForView(VueProps? row);
    bool HasActionColumn { get; }
    bool IsDeletingRow(VueProps? row);
    void OpenAudit(VueProps? value);
    void OpenEdit(VueProps? value);
    string PlatformLabel(VueProps? row);
    string PlatformTag(VueProps? row);
    WechatUserView[] rows { get; }
}

public sealed record WechatUserView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string userId { get; init; } = string.Empty;
    public int platformType { get; init; }
    public string openId { get; init; } = string.Empty;
    public string sessionKey { get; init; } = string.Empty;
    public string unionId { get; init; } = string.Empty;
    public string nickName { get; init; } = string.Empty;
    public string avatar { get; init; } = string.Empty;
    public string mobile { get; init; } = string.Empty;
    public int? sex { get; init; }
    public string language { get; init; } = string.Empty;
    public string city { get; init; } = string.Empty;
    public string province { get; init; } = string.Empty;
    public string country { get; init; } = string.Empty;
    public string accessToken { get; init; } = string.Empty;
    public string refreshToken { get; init; } = string.Empty;
    public int? expiresIn { get; init; }
    public string scope { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
}

public sealed record WechatUserPlatformOption : VueProps
{
    public int value { get; init; }
    public string label { get; init; } = string.Empty;
    public string tagType { get; init; } = string.Empty;
}
