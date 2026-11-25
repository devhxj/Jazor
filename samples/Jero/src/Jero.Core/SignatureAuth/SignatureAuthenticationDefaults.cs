namespace Jero.Core;

/// <summary>
/// Signature 身份验证处理程序相关的默认值
/// </summary>
public static class SignatureAuthenticationDefaults
{
    /// <summary>
    /// SignatureAuthenticationOptions.AuthenticationScheme 使用的默认值
    /// </summary>
    public const string AuthenticationScheme = "Signature";

    /// <summary>
    /// 附加在 HttpContext Item 中验证失败消息的 Key
    /// </summary>
    public const string AuthenticateFailMsgKey = "SignatureAuthenticateFailMsg";
}