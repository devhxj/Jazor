using ECMAScript;
using Jazor.Admin;
using Microsoft.AspNetCore.Components;

namespace JazorAdmin;

[ECMAScriptModule("./components/starter-settings")]
public partial class StarterSettings : AdminComponentBase
{
    private static readonly string[] BrandColors =
    [
        "#0052D9", "#0594FA", "#00A870", "#EBB105",
        "#ED7B2F", "#E34D59", "#ED49B4", "#834EC2"
    ];

    [Parameter]
    public bool Visible { get; set; }

    [Parameter]
    public EventCallback<bool> VisibleChanged { get; set; }

    [Parameter]
    public string ThemeKey { get; set; } = "light";

    [Parameter]
    public EventCallback<string> ThemeKeyChanged { get; set; }

    [Parameter]
    public string BrandTheme { get; set; } = "#0052D9";

    [Parameter]
    public EventCallback<string> BrandThemeChanged { get; set; }

    [Parameter]
    public string LayoutKey { get; set; } = "side";

    [Parameter]
    public EventCallback<string> LayoutKeyChanged { get; set; }

    [Parameter]
    public bool SplitMenu { get; set; }

    [Parameter]
    public EventCallback<bool> SplitMenuChanged { get; set; }

    [Parameter]
    public string SideModeKey { get; set; } = "light";

    [Parameter]
    public EventCallback<string> SideModeKeyChanged { get; set; }

    [Parameter]
    public bool IsSidebarFixed { get; set; }

    [Parameter]
    public EventCallback<bool> IsSidebarFixedChanged { get; set; }

    [Parameter]
    public bool ShowHeader { get; set; }

    [Parameter]
    public EventCallback<bool> ShowHeaderChanged { get; set; }

    [Parameter]
    public bool ShowBreadcrumb { get; set; }

    [Parameter]
    public EventCallback<bool> ShowBreadcrumbChanged { get; set; }

    [Parameter]
    public bool ShowFooter { get; set; }

    [Parameter]
    public EventCallback<bool> ShowFooterChanged { get; set; }

    [Parameter]
    public bool IsUseTabsRouter { get; set; }

    [Parameter]
    public EventCallback<bool> IsUseTabsRouterChanged { get; set; }

    [Parameter]
    public bool MenuAutoCollapsed { get; set; }

    [Parameter]
    public EventCallback<bool> MenuAutoCollapsedChanged { get; set; }

    [Parameter]
    public AdminLanguage Language { get; set; }

    private bool colorPickerVisible;

    private static readonly string[] EmptySwatchColors = [];
    private static readonly VueClassValue ColorGroupCssClass = "setting-layout-color-group";
    private static readonly VueClassValue DynamicColorCssClass = new[] { "setting-layout-color-group", "dynamic-color-btn" };
    private static readonly VueClassValue SideModeRadioCssClass = "side-mode-radio";
    private VueStyleValue? MixOnlyStyle
    {
        get
        {
            if (LayoutKey == "mix")
                return null;

            return (VueStyleValue)"display: none;";
        }
    }
    private string CustomColorLabel => L("自定义颜色", "Custom color");
    private string SideLayoutLabel => L("侧边栏", "Side");
    private string TopLayoutLabel => L("顶部", "Top");
    private string MixedLayoutLabel => L("混合", "Mix");
    private string SplitMenuLabel => L("分割菜单", "Split Menu");
    private string FixedSidebarLabel => L("固定侧边栏", "Fixed Sidebar");
    private string SideModeLabel => L("侧栏主题", "Sidebar Theme");
    private string LightLabel => L("明亮", "Light");
    private string DarkLabel => L("暗色", "Dark");
    private string ShowHeaderLabel => L("显示顶部栏", "Show Header");
    private string ShowBreadcrumbLabel => L("显示面包屑", "Show Breadcrumb");
    private string ShowFooterLabel => L("显示页脚", "Show Footer");
    private string UseTabsLabel => L("启用标签页", "Use Tag Tabs");
    private string MenuAutoCollapsedLabel => L("菜单自动收起", "Auto Collapse Menu");
    private string CopyConfigLabel => L("复制配置", "Copy Config");

    private Task CloseAsync() => VisibleChanged.InvokeAsync(false);

    private void CopyConfig()
    {
        try
        {
            var clipboard = Global.Window.Navigator.Clipboard;
            if (clipboard is not null)
                _ = Promise.Resolve(clipboard.WriteText(ConfigJson));
        }
        catch
        {
        }
    }

    private string ConfigJson =>
        "{\n" +
        "    \"showFooter\": " + BoolJson(ShowFooter) + ",\n" +
        "    \"isSidebarCompact\": false,\n" +
        "    \"showBreadcrumb\": " + BoolJson(ShowBreadcrumb) + ",\n" +
        "    \"menuAutoCollapsed\": " + BoolJson(MenuAutoCollapsed) + ",\n" +
        "    \"mode\": \"" + ThemeKey + "\",\n" +
        "    \"layout\": \"" + LayoutKey + "\",\n" +
        "    \"splitMenu\": " + BoolJson(SplitMenu) + ",\n" +
        "    \"sideMode\": \"" + SideModeKey + "\",\n" +
        "    \"isSidebarFixed\": " + BoolJson(IsSidebarFixed) + ",\n" +
        "    \"isHeaderFixed\": true,\n" +
        "    \"isUseTabsRouter\": " + BoolJson(IsUseTabsRouter) + ",\n" +
        "    \"showHeader\": " + BoolJson(ShowHeader) + ",\n" +
        "    \"brandTheme\": \"" + BrandTheme + "\"\n" +
        "}";

    private static string BoolJson(bool value) => value ? "true" : "false";

    private Task ChangeThemeAsync(string value) => ThemeKeyChanged.InvokeAsync(value);

    private Task ChangeBrandAsync(string value) => BrandThemeChanged.InvokeAsync(value);

    private Task ChangeLayoutAsync(string value) => LayoutKeyChanged.InvokeAsync(value);

    private Task ChangeCustomColorAsync(string value) => BrandThemeChanged.InvokeAsync(value);

    private Task ChangeColorPickerVisibilityAsync(bool visible)
    {
        colorPickerVisible = visible;
        return Task.CompletedTask;
    }

    private Task ChangeSplitMenuAsync(bool value) => SplitMenuChanged.InvokeAsync(value);

    private Task ChangeSidebarFixedAsync(bool value) => IsSidebarFixedChanged.InvokeAsync(value);

    private Task ChangeSideModeAsync(string value) => SideModeKeyChanged.InvokeAsync(value);

    private Task ChangeShowHeaderAsync(bool value) => ShowHeaderChanged.InvokeAsync(value);

    private Task ChangeShowBreadcrumbAsync(bool value) => ShowBreadcrumbChanged.InvokeAsync(value);

    private Task ChangeShowFooterAsync(bool value) => ShowFooterChanged.InvokeAsync(value);

    private Task ChangeUseTabsAsync(bool value) => IsUseTabsRouterChanged.InvokeAsync(value);

    private Task ChangeMenuAutoCollapsedAsync(bool value) => MenuAutoCollapsedChanged.InvokeAsync(value);

    private string L(string chinese, string english) => Language == AdminLanguage.Chinese ? chinese : english;
}
