namespace Jero.Core;

/// <summary>
/// 消息模板类型枚举
/// </summary>
[Description("消息模板类型枚举")]
public enum TemplateTypeEnum
{
    /// <summary>
    /// 通知公告
    /// </summary>
    [Description("通知")]
    Notice = 1,

    /// <summary>
    /// 短信
    /// </summary>
    [Description("短信")]
    SMS = 2,

    /// <summary>
    /// 邮件
    /// </summary>
    [Description("邮件")]
    Email = 3,

    /// <summary>
    /// 微信
    /// </summary>
    [Description("微信")]
    Wechat = 4,

    /// <summary>
    /// 钉钉
    /// </summary>
    [Description("钉钉")]
    DingTalk = 5,

    /// <summary>
    /// 企业微信
    /// </summary>
    [Description("企业微信")]
    WeChatWork = 7
}