


using Microsoft.AspNetCore.Authentication;
namespace Jero.Core;

/// <summary>
/// Signature 身份验证扩展
/// </summary>
public static class SignatureAuthenticationExtensions
{
    /// <summary>
    /// 注册 Signature 身份验证处理模块
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static AuthenticationBuilder AddSignatureAuthentication(this AuthenticationBuilder builder)
    {
        return builder.AddSignatureAuthentication(options => { });
    }

    /// <summary>
    /// 注册 Signature 身份验证处理模块
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static AuthenticationBuilder AddSignatureAuthentication(this AuthenticationBuilder builder, Action<SignatureAuthenticationOptions> options)
    {
        return builder.AddScheme<SignatureAuthenticationOptions, SignatureAuthenticationHandler>(SignatureAuthenticationDefaults.AuthenticationScheme, options);
    }
}