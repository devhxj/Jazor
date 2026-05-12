namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 对齐方式枚举。
/// Vuetify justify alignment enumeration.
/// </summary>
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
/// Vuetify VEmptyState 操作插槽暴露的属性对象。
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
/// Vuetify VEmptyState 作用域操作插槽上下文。
/// Scoped actions slot context exposed by Vuetify VEmptyState.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VEmptyStateActionsSlotContext
{
    [Description("@#props")]
    public VEmptyStateActionsProps? Props { get; init; }
}
