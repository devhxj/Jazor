namespace Jero.Core;

/// <summary>
/// 第三方登录授权配置选项
/// </summary>
public sealed class OAuthOptions : IConfigurableOptions
{
    /// <summary>
    /// Weixin配置
    /// </summary>
    public Microsoft.AspNetCore.Authentication.OAuth.OAuthOptions Weixin { get; set; } = new();
    /// <summary>
    /// Gitee配置
    /// </summary>
    public Microsoft.AspNetCore.Authentication.OAuth.OAuthOptions Gitee { get; set; } = new();
}