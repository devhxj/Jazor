using System.ComponentModel;

namespace Jero.Application.Modules.DingTalk;

/// <summary>
/// 钉钉发送的会话类型枚举
/// </summary>
[Description("钉钉发送的会话类型枚举")]
public enum DingTalkConversationTypeEnum
{
    /// <summary>
    /// 单聊
    /// </summary>
    [Description("单聊")]
    SingleChat = 0,

    /// <summary>
    /// 群聊
    /// </summary>
    [Description("群聊")]
    GroupChat = 1
}