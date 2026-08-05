namespace ECMAScript.TDesign;

/// <summary>
/// Export surface for the first stable TDesign components.
/// </summary>
[ECMAScript("tdesign-vue-next")]
public static class TComponents
{
    [ECMAScriptName("Button")]
    public extern static ITComponent TButton { get; }

    [ECMAScriptName("Breadcrumb")]
    public extern static ITComponent TBreadcrumb { get; }

    [ECMAScriptName("BreadcrumbItem")]
    public extern static ITComponent TBreadcrumbItem { get; }

    [ECMAScriptName("Layout")]
    public extern static ITComponent TLayout { get; }

    [ECMAScriptName("Aside")]
    public extern static ITComponent TAside { get; }

    [ECMAScriptName("Header")]
    public extern static ITComponent THeader { get; }

    [ECMAScriptName("Content")]
    public extern static ITComponent TContent { get; }

    [ECMAScriptName("Footer")]
    public extern static ITComponent TFooter { get; }

    [ECMAScriptName("Menu")]
    public extern static ITComponent TMenu { get; }

    [ECMAScriptName("HeadMenu")]
    public extern static ITComponent THeadMenu { get; }

    [ECMAScriptName("Submenu")]
    public extern static ITComponent TSubmenu { get; }

    [ECMAScriptName("MenuItem")]
    public extern static ITComponent TMenuItem { get; }

    [ECMAScriptName("MenuGroup")]
    public extern static ITComponent TMenuGroup { get; }

    [ECMAScriptName("Card")]
    public extern static ITComponent TCard { get; }

    [ECMAScriptName("Link")]
    public extern static ITComponent TLink { get; }

    [ECMAScriptName("Tabs")]
    public extern static ITComponent TTabs { get; }

    [ECMAScriptName("TabPanel")]
    public extern static ITComponent TTabPanel { get; }

    [ECMAScriptName("Avatar")]
    public extern static ITComponent TAvatar { get; }

    [ECMAScriptName("AvatarGroup")]
    public extern static ITComponent TAvatarGroup { get; }

    [ECMAScriptName("Badge")]
    public extern static ITComponent TBadge { get; }

    [ECMAScriptName("Space")]
    public extern static ITComponent TSpace { get; }

    [ECMAScriptName("Divider")]
    public extern static ITComponent TDivider { get; }

    [ECMAScriptName("ConfigProvider")]
    public extern static ITComponent TConfigProvider { get; }
}
