namespace Jero.Core;

/// <summary>
/// 密码配置选项
/// </summary>
public sealed class CryptogramOptions : IConfigurableOptions
{
    /// <summary>
    /// 是否开启密码强度验证
    /// </summary>
    public bool StrongPassword { get; set; }

    /// <summary>
    /// 密码强度验证正则表达式
    /// </summary>
    public string PasswordStrengthValidation { get; set; } = string.Empty;
    /// <summary>
    /// 密码强度验证提示
    /// </summary>
    public string PasswordStrengthValidationMsg { get; set; } = string.Empty;
    /// <summary>
    /// 密码类型
    /// </summary>
    public string CryptoType { get; set; } = string.Empty;
    /// <summary>
    /// 公钥
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;
    /// <summary>
    /// 私钥
    /// </summary>
    public string PrivateKey { get; set; } = string.Empty;
}