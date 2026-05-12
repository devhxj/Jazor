namespace ECMAScript.Vuetify;

/// <summary>
/// VAlert 关闭插槽的上下文对象。
/// Close slot context exposed by Vuetify VAlert.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VAlertCloseSlotContext
{
    [Description("@#props")]
    public VueProps? Props { get; init; }
}
