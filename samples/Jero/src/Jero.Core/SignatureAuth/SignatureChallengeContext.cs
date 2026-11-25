


using Microsoft.AspNetCore.Authentication;
namespace Jero.Core;

/// <summary>
/// Signature 身份验证质询上下文
/// </summary>
public class SignatureChallengeContext : PropertiesContext<SignatureAuthenticationOptions>
{
    public SignatureChallengeContext(HttpContext context,
        AuthenticationScheme scheme,
        SignatureAuthenticationOptions options,
        AuthenticationProperties properties)
        : base(context, scheme, options, properties)
    {
    }

    /// <summary>
    /// 在认证期间出现的异常
    /// </summary>
    public Exception? AuthenticateFailure { get; set; }

    /// <summary>
    /// 指定是否已被处理，如果已处理，则跳过默认认证逻辑
    /// </summary>
    public bool Handled { get; private set; }
}
