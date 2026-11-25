namespace Jero.Core.Service;

/// <summary>
/// 开放接口身份输入参数
/// </summary>
public class OpenAccessInput : BasePageInput
{
    /// <summary>
    /// 身份标识
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;
}

public class AddOpenAccessInput : SysOpenAccess
{
    /// <summary>
    /// 身份标识
    /// </summary>
    [Required(ErrorMessage = "身份标识不能为空")]
    public override string AccessKey { get; set; } = string.Empty;
    /// <summary>
    /// 密钥
    /// </summary>
    [Required(ErrorMessage = "密钥不能为空")]
    public override string AccessSecret { get; set; } = string.Empty;
    /// <summary>
    /// 绑定用户Id
    /// </summary>
    [Required(ErrorMessage = "绑定用户不能为空")]
    public override long BindUserId { get; set; }
}

public class UpdateOpenAccessInput : AddOpenAccessInput
{
}

public class DeleteOpenAccessInput : BaseIdInput
{
}

public class GenerateSignatureInput
{
    /// <summary>
    /// 身份标识
    /// </summary>
    [Required(ErrorMessage = "身份标识不能为空")]
    public string AccessKey { get; set; } = string.Empty;
    /// <summary>
    /// 密钥
    /// </summary>
    [Required(ErrorMessage = "密钥不能为空")]
    public string AccessSecret { get; set; } = string.Empty;
    /// <summary>
    /// 请求方法
    /// </summary>
    public HttpMethodEnum Method { get; set; }

    /// <summary>
    /// 请求接口地址
    /// </summary>
    [Required(ErrorMessage = "请求接口地址不能为空")]
    public string Url { get; set; } = string.Empty;
    /// <summary>
    /// 时间戳
    /// </summary>
    [Required(ErrorMessage = "时间戳不能为空")]
    public long Timestamp { get; set; }

    /// <summary>
    /// 随机数
    /// </summary>
    [Required(ErrorMessage = "随机数不能为空")]
    public string Nonce { get; set; } = string.Empty;
}