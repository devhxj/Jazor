namespace Jero.Application.Modules.DingTalk;

/// <summary>
/// 发送钉钉互动卡片返回
/// </summary>
public class DingTalkSendInteractiveCardsOutput
{
    /// <summary>
    /// 返回结果
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 创建卡片结果
    /// </summary>
    public DingTalkSendInteractiveCardsResult Result { get; set; } = new();
}
