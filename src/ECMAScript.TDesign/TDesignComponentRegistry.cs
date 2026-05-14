namespace ECMAScript.TDesign;

/// <summary>
/// Registry of first-slice TDesign components.
/// </summary>
[ECMAScript]
[Description("@#TDesignComponentRegistry")]
public sealed record TDesignComponentRegistry : VueComponentRegistry
{
    [Description("@#Button")]
    public ITDesignComponent? TButton { get; init; }

    [Description("@#Breadcrumb")]
    public ITDesignComponent? TBreadcrumb { get; init; }

    [Description("@#BreadcrumbItem")]
    public ITDesignComponent? TBreadcrumbItem { get; init; }

    [Description("@#Layout")]
    public ITDesignComponent? TLayout { get; init; }

    [Description("@#Aside")]
    public ITDesignComponent? TAside { get; init; }

    [Description("@#Header")]
    public ITDesignComponent? THeader { get; init; }

    [Description("@#Content")]
    public ITDesignComponent? TContent { get; init; }

    [Description("@#Footer")]
    public ITDesignComponent? TFooter { get; init; }

    [Description("@#Menu")]
    public ITDesignComponent? TMenu { get; init; }

    [Description("@#HeadMenu")]
    public ITDesignComponent? THeadMenu { get; init; }

    [Description("@#Submenu")]
    public ITDesignComponent? TSubmenu { get; init; }

    [Description("@#MenuItem")]
    public ITDesignComponent? TMenuItem { get; init; }

    [Description("@#MenuGroup")]
    public ITDesignComponent? TMenuGroup { get; init; }

    [Description("@#Card")]
    public ITDesignComponent? TCard { get; init; }

    [Description("@#Link")]
    public ITDesignComponent? TLink { get; init; }

    [Description("@#Tabs")]
    public ITDesignComponent? TTabs { get; init; }

    [Description("@#TabPanel")]
    public ITDesignComponent? TTabPanel { get; init; }

    [Description("@#Avatar")]
    public ITDesignComponent? TAvatar { get; init; }

    [Description("@#AvatarGroup")]
    public ITDesignComponent? TAvatarGroup { get; init; }

    [Description("@#Badge")]
    public ITDesignComponent? TBadge { get; init; }

    [Description("@#Space")]
    public ITDesignComponent? TSpace { get; init; }

    [Description("@#Divider")]
    public ITDesignComponent? TDivider { get; init; }

    [Description("@#ConfigProvider")]
    public ITDesignComponent? TConfigProvider { get; init; }
}
