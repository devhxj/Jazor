namespace Jero.Plugin.DingTalk;

public class DingTalkSendInteractiveCardsInput
{
    /// <summary>
    /// 互动卡片的消息模板Id
    /// </summary>
    [Required(ErrorMessage = "互动卡片的消息模板Id必填!")]
    public string? CardTemplateId { get; set; }

    /// <summary>
    /// 群Id
    /// </summary>
    /// <remarks>
    /// 1、基于群模板创建的群。
    /// 企业内部应用，调用创建群接口获取open_conversation_id参数值。
    /// 2、安装群聊酷应用的群。
    /// 企业内部应用，通过群内安装酷应用事件获取回调参数OpenConversationId参数值。
    /// </remarks>
    public string OpenConversationId { get; set; } = string.Empty;

    /// <summary>
    /// 接收人userId列表
    /// </summary>
    /// <remarks>
    /// 单聊：receiverUserIdList填写用户ID，最大值20。
    /// 群聊：receiverUserIdList填写用户ID，表示当前对应ID的群内用户可见
    /// receiverUserIdList参数不填写，表示当前群内所有用户可见
    /// </remarks>
    [Required(ErrorMessage = "接收人userId列表必填!")]
    public List<string>? ReceiverUserIdList { get; set; }

    /// <summary>
    /// 唯一标示卡片的外部编码
    /// </summary>
    [Required(ErrorMessage = "唯一标示卡片的外部编码必填!")]
    public string? OutTrackId { get; set; }

    /// <summary>
    /// 机器人的编码
    /// </summary>
    public string RobotCode { get; set; } = string.Empty;

    /// <summary>
    /// 发送的会话类型
    /// </summary>
    [Required(ErrorMessage = "会话类型必填!")]
    public DingTalkConversationTypeEnum? ConversationType { get; set; }

    /// <summary>
    /// 卡片回调时的路由Key，用于查询注册的callbackUrl
    /// </summary>
    public string CallbackRouteKey { get; set; } = string.Empty;

    /// <summary>
    /// 卡片公有数据
    /// </summary>
    [Required(ErrorMessage = "卡片公有数据必填!")]
    public DingTalkCardData? CardData { get; set; }
}

public class GetDingTalkCardMessageReadStatusInput
{
    /// <summary>
    /// 机器人的编码
    /// </summary>
    public string RobotCode { set; get; } = string.Empty;

    /// <summary>
    /// 消息唯一标识，可通过批量发送人与机器人会话中机器人消息接口返回参数中processQueryKey字段获取。
    /// </summary>
    public string ProcessQueryKey { set; get; } = string.Empty;
}

public class GetDingTalkCardMessageReadStatusOutput
{
    /// <summary>
    /// 消息发送状态，SUCCESS：成功、RECALLED：已撤回、PROCESSING： 处理中
    /// </summary>
    public string SendStatus { get; set; } = string.Empty;

    /// <summary>
    ///
    /// </summary>
    public DingTalkCardMessageReadInfoList MessageReadInfoList { get; set; } = new();
}

/// <summary>
/// 钉钉卡片消息已读情况
/// </summary>
public class DingTalkCardMessageReadInfoList
{
    /// <summary>
    /// 消息接收者名称
    /// </summary>
    public string Name { set; get; } = string.Empty;

    /// <summary>
    /// 消息接收者的userId
    /// </summary>
    public string UserId { set; get; } = string.Empty;

    /// <summary>
    /// 已读状态，READ：已读、UNREAD：未读
    /// </summary>
    public string ReadStatus { set; get; } = string.Empty;
}
