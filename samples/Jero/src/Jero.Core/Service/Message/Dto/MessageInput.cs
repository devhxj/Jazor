namespace Jero.Core;

public class MessageInput
{
    /// <summary>
    /// 接收者用户Id
    /// </summary>
    public long ReceiveUserId { get; set; }

    /// <summary>
    /// 接收者名称
    /// </summary>
    public string ReceiveUserName { get; set; } = string.Empty;
    /// <summary>
    /// 用户ID列表
    /// </summary>
    public List<long> UserIds { get; set; } = [];
    /// <summary>
    /// 消息标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageTypeEnum MessageType { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// 发送者Id
    /// </summary>
    public string SendUserId { get; set; } = string.Empty;
    /// <summary>
    /// 发送者名称
    /// </summary>
    public string SendUserName { get; set; } = string.Empty;
    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime SendTime { get; set; }
}