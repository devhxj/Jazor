namespace ECMAScript.Vuetify;

/// <summary>
/// VMessages 消息插槽的上下文数据。
/// Slot context for the VMessages message slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VMessagesMessageSlotContext
{
    [Description("@#message")]
    public string? Message { get; init; }
}
