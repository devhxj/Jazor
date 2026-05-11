namespace ECMAScript.Vuetify;

[String]
public enum VuetifyJustify
{
    [Description("@#start")]
    Start,

    [Description("@#center")]
    Center,

    [Description("@#end")]
    End
}

/// <summary>
/// Props object exposed by the Vuetify VEmptyState actions slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VEmptyStateActionsProps
{
    [Description("@#onClick")]
    public Action<Event>? OnClick { get; init; }
}

/// <summary>
/// Scoped actions slot context exposed by Vuetify VEmptyState.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VEmptyStateActionsSlotContext
{
    [Description("@#props")]
    public VEmptyStateActionsProps? Props { get; init; }
}
