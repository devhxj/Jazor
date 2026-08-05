namespace ECMAScript.TDesign;

/// <summary>
/// Registry of first-slice TDesign components.
/// </summary>
[ECMAScript]
[Description("@#TComponentRegistry")]
public sealed record TComponentRegistry : VueComponentRegistry
{
    [Description("@#Button")]
    public ITComponent? TButton { get; init; }

    [Description("@#Breadcrumb")]
    public ITComponent? TBreadcrumb { get; init; }

    [Description("@#BreadcrumbItem")]
    public ITComponent? TBreadcrumbItem { get; init; }

    [Description("@#Layout")]
    public ITComponent? TLayout { get; init; }

    [Description("@#Aside")]
    public ITComponent? TAside { get; init; }

    [Description("@#Header")]
    public ITComponent? THeader { get; init; }

    [Description("@#Content")]
    public ITComponent? TContent { get; init; }

    [Description("@#Footer")]
    public ITComponent? TFooter { get; init; }

    [Description("@#Menu")]
    public ITComponent? TMenu { get; init; }

    [Description("@#HeadMenu")]
    public ITComponent? THeadMenu { get; init; }

    [Description("@#Submenu")]
    public ITComponent? TSubmenu { get; init; }

    [Description("@#MenuItem")]
    public ITComponent? TMenuItem { get; init; }

    [Description("@#MenuGroup")]
    public ITComponent? TMenuGroup { get; init; }

    [Description("@#Card")]
    public ITComponent? TCard { get; init; }

    [Description("@#Link")]
    public ITComponent? TLink { get; init; }

    [Description("@#Tabs")]
    public ITComponent? TTabs { get; init; }

    [Description("@#TabPanel")]
    public ITComponent? TTabPanel { get; init; }

    [Description("@#Avatar")]
    public ITComponent? TAvatar { get; init; }

    [Description("@#AvatarGroup")]
    public ITComponent? TAvatarGroup { get; init; }

    [Description("@#Badge")]
    public ITComponent? TBadge { get; init; }

    [Description("@#Space")]
    public ITComponent? TSpace { get; init; }

    [Description("@#Divider")]
    public ITComponent? TDivider { get; init; }

    [Description("@#ConfigProvider")]
    public ITComponent? TConfigProvider { get; init; }
}
