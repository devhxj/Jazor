namespace Jero.Core.Service;

/// <summary>
/// 生成网页授权Url
/// </summary>
public class GenAuthUrlInput
{
    /// <summary>
    /// RedirectUrl
    /// </summary>
    public string RedirectUrl { get; set; } = string.Empty;
    /// <summary>
    /// Scope
    /// </summary>
    public string Scope { get; set; } = string.Empty;
    /// <summary>
    /// State
    /// </summary>
    public string State { get; set; } = string.Empty;
}

/// <summary>
/// 获取微信用户OpenId
/// </summary>
public class WechatOAuth2Input
{
    /// <summary>
    /// Code
    /// </summary>
    [Required(ErrorMessage = "Code不能为空"), MinLength(10, ErrorMessage = "Code错误")]
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// 微信用户登录
/// </summary>
public class WechatUserLogin
{
    /// <summary>
    /// OpenId
    /// </summary>
    [Required(ErrorMessage = "微信标识不能为空"), MinLength(10, ErrorMessage = "微信标识长错误")]
    public string OpenId { get; set; } = string.Empty;
}

/// <summary>
/// 获取配置签名
/// </summary>
public class SignatureInput
{
    /// <summary>
    /// Url
    /// </summary>
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// 获取消息模板列表
/// </summary>
public class MessageTemplateSendInput
{
    /// <summary>
    /// 订阅模板Id
    /// </summary>
    [Required(ErrorMessage = "订阅模板Id不能为空")]
    public string TemplateId { get; set; } = string.Empty;
    /// <summary>
    /// 接收者的OpenId
    /// </summary>
    [Required(ErrorMessage = "接收者的OpenId不能为空")]
    public string ToUserOpenId { get; set; } = string.Empty;
    /// <summary>
    /// 模板数据，格式形如 { "key1": { "value": any }, "key2": { "value": any } }
    /// </summary>
    [Required(ErrorMessage = "模板数据不能为空")]
    public Dictionary<string, CgibinMessageSubscribeSendRequest.Types.DataItem> Data { get; set; } = [];
    /// <summary>
    /// 模板跳转链接
    /// </summary>
    public string Url { get; set; } = string.Empty;
    /// <summary>
    /// 所需跳转到小程序的具体页面路径，支持带参数,（示例index?foo=bar）
    /// </summary>
    public string MiniProgramPagePath { get; set; } = string.Empty;
}

/// <summary>
/// 删除消息模板
/// </summary>
public class DeleteMessageTemplateInput
{
    /// <summary>
    /// 订阅模板Id
    /// </summary>
    [Required(ErrorMessage = "订阅模板Id不能为空")]
    public string TemplateId { get; set; } = string.Empty;
}

public class UploadAvatarInput
{
    /// <summary>
    /// 小程序用户身份标识
    /// </summary>
    [Required(ErrorMessage = "OpenId不能为空")]
    public string OpenId { get; set; } = string.Empty;
    /// <summary>
    /// 文件
    /// </summary>
    [Required]
    public IFormFile? File { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    public string FileType { get; set; } = string.Empty;
    /// <summary>
    /// 文件路径
    /// </summary>
    public string Path { get; set; } = string.Empty;
}

public class SetNickNameInput
{
    /// <summary>
    /// 小程序用户身份标识
    /// </summary>
    [Required(ErrorMessage = "OpenId不能为空")]
    public string OpenId { get; set; } = string.Empty;
    /// <summary>
    /// 昵称
    /// </summary>
    [Required(ErrorMessage = "昵称不能为空")]
    public string NickName { get; set; } = string.Empty;
}
