namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VDialog 激活器插槽上下文。
/// Activator slot context exposed by Vuetify VDialog.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VDialogActivatorContext
{
    [Description("@#isActive")]
    public bool IsActive { get; init; }

    [Description("@#props")]
    public VueProps? Props { get; init; }

    [Description("@#targetRef")]
    public VueValue? TargetRef { get; init; }
}
