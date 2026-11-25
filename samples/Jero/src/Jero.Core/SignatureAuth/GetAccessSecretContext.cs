


using Microsoft.AspNetCore.Authentication;
namespace Jero.Core;

/// <summary>
/// 获取 AccessKey 关联 AccessSecret 方法的上下文
/// </summary>
public class GetAccessSecretContext : BaseContext<SignatureAuthenticationOptions>
{
    public GetAccessSecretContext(HttpContext context,
        AuthenticationScheme scheme,
        SignatureAuthenticationOptions options)
        : base(context, scheme, options)
    {
    }

    /// <summary>
    /// 身份标识
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;
}