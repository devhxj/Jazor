namespace ECMAScript.Vuetify;

/// <summary>
/// Close slot context exposed by Vuetify VAlert.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VAlertCloseSlotContext
{
    [Description("@#props")]
    public VueProps? Props { get; init; }
}
