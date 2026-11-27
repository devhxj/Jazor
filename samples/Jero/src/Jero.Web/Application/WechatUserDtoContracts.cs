using static ECMAScript.Vue;

namespace Jero.Web.Application;

public sealed record WechatUserView : VueProps
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    [ECMAScriptName("platformType")]
    public int PlatformType { get; init; }
    public string OpenId { get; init; } = string.Empty;
    public string SessionKey { get; init; } = string.Empty;
    [ECMAScriptName("unionId")]
    public string UnionId { get; init; } = string.Empty;
    [ECMAScriptName("nickName")]
    public string NickName { get; init; } = string.Empty;
    [ECMAScriptName("avatar")]
    public string Avatar { get; init; } = string.Empty;
    [ECMAScriptName("mobile")]
    public string Mobile { get; init; } = string.Empty;
    [ECMAScriptName("sex")]
    public int? Sex { get; init; }
    public string Language { get; init; } = string.Empty;
    [ECMAScriptName("city")]
    public string City { get; init; } = string.Empty;
    [ECMAScriptName("province")]
    public string Province { get; init; } = string.Empty;
    [ECMAScriptName("country")]
    public string Country { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public int? ExpiresIn { get; init; }
    public string Scope { get; init; } = string.Empty;
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
}

public sealed record WechatUserPlatformOption : VueProps
{
    public int Value { get; init; }
    public string Label { get; init; } = string.Empty;
    public string TagType { get; init; } = string.Empty;
}
