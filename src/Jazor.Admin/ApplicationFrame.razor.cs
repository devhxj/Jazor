namespace Jazor.Admin;

[ECMAScriptModule("./components/admin/frame")]
public partial class ApplicationFrame : AdminContentComponentBase, IVueContainerComponent
{
    [Parameter]
    public AdminThemeMode Theme { get; set; }

    [Parameter]
    public string? Language { get; set; }

    [Parameter]
    public bool Grayscale { get; set; }

    private VueClassValue RootCssClass
        => Grayscale
            ? BuildCssClass("jazor-admin-application", GetThemeCssClass(Theme), "jazor-admin-application--grayscale")
            : BuildCssClass("jazor-admin-application", GetThemeCssClass(Theme));

    private VueStyleValue RootStyle
    {
        get
        {
            VueStyleValue cssStyle = CssStyle ?? (VueStyleValue)string.Empty;
            if (!Grayscale)
            {
                return cssStyle;
            }

            VueStyleValue[] styles =
            [
                cssStyle,
                (VueStyleValue)"filter: grayscale(1);"
            ];
            return (VueStyleValue)styles;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        AdminStyleSheet.EnsureLoaded();

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", RootCssClass);
        builder.AddAttribute(2, "style", RootStyle);
        builder.AddAttribute(3, "lang", AdminDisplayTextHelper.Normalize(Language));
        builder.AddAttribute(4, "data-theme", Theme);
        builder.AddAttribute(5, "data-grayscale", Grayscale);
        builder.AddMultipleAttributes(6, AdditionalAttributes);
        builder.AddContent(7, ChildContent);
        builder.CloseElement();
    }

    private static string GetThemeCssClass(AdminThemeMode theme)
        => theme switch
        {
            AdminThemeMode.Light => "jazor-admin-application--light",
            AdminThemeMode.Dark => "jazor-admin-application--dark",
            _ => "jazor-admin-application--system"
        };
}
