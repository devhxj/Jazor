namespace ECMAScript.Vuetify;

[ECMAScript]
[Description("@#")]
public sealed record VProgressCircularDefaultSlotContext
{
    [Description("@#value")]
    public Number Value { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record VProgressLinearDefaultSlotContext
{
    [Description("@#value")]
    public Number Value { get; init; }

    [Description("@#buffer")]
    public Number Buffer { get; init; }
}
