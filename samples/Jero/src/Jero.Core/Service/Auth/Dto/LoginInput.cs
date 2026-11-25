namespace Jero.Core.Service;

/// <summary>
/// 用户登录参数
/// </summary>
public class LoginInput
{
    /// <summary>
    /// 账号
    /// </summary>
    /// <example>admin</example>
    [Required(ErrorMessage = "账号不能为空"), MinLength(2, ErrorMessage = "账号不能少于2个字符")]
    public string Account { get; set; } = string.Empty;
    /// <summary>
    /// 密码
    /// </summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "密码不能为空"), MinLength(3, ErrorMessage = "密码不能少于3个字符")]
    public string Password { get; set; } = string.Empty;
    /// <summary>
    /// 租户
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    /// 验证码Id
    /// </summary>
    public long CodeId { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    public string Code { get; set; } = string.Empty;
}

public class LoginPhoneInput
{
    /// <summary>
    /// 手机号码
    /// </summary>
    /// <example>admin</example>
    [Required(ErrorMessage = "手机号码不能为空")]
    [DataValidation(ValidationTypes.PhoneNumber, ErrorMessage = "手机号码不正确")]
    public string Phone { get; set; } = string.Empty;
    /// <summary>
    /// 验证码
    /// </summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "验证码不能为空"), MinLength(4, ErrorMessage = "验证码不能少于4个字符")]
    public string Code { get; set; } = string.Empty;
    /// <summary>
    /// 租户
    /// </summary>
    [Required(ErrorMessage = "租户不能为空")]
    public long? TenantId { get; set; }
}

/// <summary>
/// 用户注册输入参数
/// </summary>
public class UserRegistrationInput
{
    /// <summary>
    /// 真实姓名
    /// </summary>
    [Required(ErrorMessage = "真实姓名不能为空"), MinLength(2, ErrorMessage = "真实姓名不能少于2个字符")]
    public string RealName { get; set; } = string.Empty;
    /// <summary>
    /// 账号
    /// </summary>
    [Required(ErrorMessage = "账号不能为空"), MinLength(6, ErrorMessage = "账号不能少于6个字符")]
    public string Account { get; set; } = string.Empty;
    /// <summary>
    /// 手机号码
    /// </summary>
    /// <example>admin</example>
    [Required(ErrorMessage = "手机号码不能为空")]
    [DataValidation(ValidationTypes.PhoneNumber, ErrorMessage = "手机号码不正确")]
    public string Phone { get; set; } = string.Empty;
    /// <summary>
    /// 验证码
    /// </summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "验证码不能为空")]
    public string Code { get; set; } = string.Empty;
    /// <summary>
    /// 验证码Id
    /// </summary>
    public long CodeId { get; set; }

    /// <summary>
    /// 租户
    /// </summary>
    [Required(ErrorMessage = "租户不能为空")]
    public long TenantId { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = string.Empty;
    /// <summary>
    /// 注册方案
    /// </summary>
    public long WayId { get; set; }
}