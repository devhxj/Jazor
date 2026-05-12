namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VCounter 公开的默认插槽上下文。
/// Default slot context exposed by Vuetify VCounter.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VCounterDefaultSlotContext
{
    [Description("@#counter")]
    public string? Counter { get; init; }

    [Description("@#max")]
    public VueStringNumberValue? Max { get; init; }

    [Description("@#value")]
    public VueStringNumberValue? Value { get; init; }
}
