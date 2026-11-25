namespace Jero.Plugin.GoView.Service;

/// <summary>
/// 登录输出
/// </summary>
public class GoViewLoginOutput
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public GoViewLoginUserInfo Userinfo { get; set; } = new();

    /// <summary>
    /// Token
    /// </summary>
    public GoViewLoginToken Token { get; set; } = new();
}

/// <summary>
/// 登录 Token
/// </summary>
public class GoViewLoginToken
{
    /// <summary>
    /// Token 名
    /// </summary>
    public string TokenName { get; set; } = "Authorization";

    /// <summary>
    /// Token 值
    /// </summary>
    public string TokenValue { get; set; } = string.Empty;
}

/// <summary>
/// 用户信息
/// </summary>
public class GoViewLoginUserInfo
{
    /// <summary>
    /// 用户 Id
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 昵称
    /// </summary>
    public string Nickname { get; set; } = string.Empty;
}
