namespace ECMAScript.Vuetify;

/// <summary>
/// VMessages 消息插槽的上下文数据。
/// Slot context for the VMessages message slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VMessagesMessageSlotContext
{
    /// <summary>
    /// 当前消息的显示文本。
    /// </summary>
    [Description("@#message")]
    public string? Message { get; init; }
}