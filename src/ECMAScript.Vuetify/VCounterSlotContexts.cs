namespace ECMAScript.Vuetify;

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
