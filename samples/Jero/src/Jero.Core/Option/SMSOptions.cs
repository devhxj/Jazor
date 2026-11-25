namespace Jero.Core;

/// <summary>
/// 短信配置选项
/// </summary>
public sealed class SMSOptions : IConfigurableOptions
{
    /// <summary>
    /// 验证码缓存过期时间（秒）
    /// 默认: 60秒
    /// </summary>
    public int VerifyCodeExpireSeconds { get; set; } = 60;

    /// <summary>
    /// Aliyun
    /// </summary>
    public SMSSettings Aliyun { get; set; } = new();
    /// <summary>
    /// Tencentyun
    /// </summary>
    public SMSSettings Tencentyun { get; set; } = new();
    /// <summary>
    /// Custom 自定义短信接口
    /// </summary>
    public CustomSMSSettings Custom { get; set; } = new();
}

public sealed class SMSSettings
{
    /// <summary>
    /// SdkAppId
    /// </summary>
    public string SdkAppId { get; set; } = string.Empty;
    /// <summary>
    /// AccessKey ID
    /// </summary>
    public string AccessKeyId { get; set; } = string.Empty;
    /// <summary>
    /// AccessKey Secret
    /// </summary>
    public string AccessKeySecret { get; set; } = string.Empty;
    /// <summary>
    /// Templates
    /// </summary>
    public List<SmsTemplate> Templates { get; set; } = [];
    /// <summary>
    /// GetTemplate
    /// </summary>
    public SmsTemplate? GetTemplate(string id = "0")
    {
        foreach (var template in Templates)
        {
            if (template.Id == id) { return template; }
        }
        return null;
    }
}

public class SmsTemplate
{
    public string Id { get; set; } = string.Empty;
    public string SignName { get; set; } = string.Empty;
    public string TemplateCode { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// 自定义短信配置
/// </summary>
public sealed class CustomSMSSettings
{
    /// <summary>
    /// 是否启用自定义短信接口
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// API 接口地址模板
    /// <para>支持占位符: {mobile} - 手机号, {content} - 短信内容, {code} - 验证码</para>
    /// </summary>
    /// <remarks>示例: https://api.xxxx.com/sms?u=xxxx&amp;key=59e03f49c3dbb5033&amp;m={mobile}&amp;c={content}</remarks>
    public string ApiUrl { get; set; } = string.Empty;
    /// <summary>
    /// 请求方法 (GET/POST)
    /// </summary>
    public string Method { get; set; } = "GET";

    /// <summary>
    /// POST 请求的 Content-Type (application/json 或 application/x-www-form-urlencoded)
    /// 默认: application/x-www-form-urlencoded
    /// </summary>
    public string ContentType { get; set; } = "application/x-www-form-urlencoded";

    /// <summary>
    /// POST 请求的数据模板（支持占位符）
    /// </summary>
    /// <remarks>
    /// JSON 格式示例: {"mobile":"{mobile}","content":"{content}","apikey":"your_key"} <br />
    /// Form 格式示例: mobile={mobile}&amp;content={content}&amp;apikey=your_key
    /// </remarks>
    public string PostData { get; set; } = string.Empty;
    /// <summary>
    /// 成功响应标识（用于判断发送是否成功）
    /// 如果响应内容包含此字符串，则认为发送成功
    /// </summary>
    public string SuccessFlag { get; set; } = "0";

    /// <summary>
    /// 短信模板列表
    /// </summary>
    public List<SmsTemplate> Templates { get; set; } = [];
    /// <summary>
    /// 获取模板
    /// </summary>
    public SmsTemplate? GetTemplate(string id = "0")
    {
        foreach (var template in Templates)
        {
            if (template.Id == id) { return template; }
        }
        return null;
    }
}
