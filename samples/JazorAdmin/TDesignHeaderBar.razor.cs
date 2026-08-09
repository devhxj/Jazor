using Microsoft.AspNetCore.Components;

namespace JazorAdmin;

[ECMAScriptModule("./components/tdesign/header")]
public partial class TDesignHeaderBar : AdminComponentBase
{
    [Parameter]
    public AdminThemeMode Theme { get; set; } = AdminThemeMode.Light;

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public RenderFragment? Logo { get; set; }

    [Parameter]
    public bool ShowLogo { get; set; } = true;

    [Parameter]
    public RenderFragment? Leading { get; set; }

    [Parameter]
    public RenderFragment? Actions { get; set; }

    [Parameter]
    public RenderFragment? Navigation { get; set; }

    [Parameter]
    public TMenuValue? NavigationValue { get; set; }

    [Parameter]
    public TMenuValue[]? NavigationExpanded { get; set; }

    [Parameter]
    public EventCallback<TMenuValue> OnNavigationChange { get; set; }

    [Parameter]
    public EventCallback<TMenuValue[]> OnNavigationExpand { get; set; }

    [Parameter]
    public RenderFragment? UserRegion { get; set; }

    private const string RootCssClass = "ja-tdesign-header";

    private THeadMenuThemeValue MenuTheme
        => Theme == AdminThemeMode.Dark ? THeadMenuThemeValue.Dark : THeadMenuThemeValue.Light;
}
