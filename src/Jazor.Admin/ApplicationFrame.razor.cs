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
            ? BuildCssClass("ja-application", GetThemeCssClass(Theme), "ja-application--grayscale")
            : BuildCssClass("ja-application", GetThemeCssClass(Theme));

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
        EnsureStylesRegistered();

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", RootCssClass);
        builder.AddAttribute(2, "style", RootStyle);
        builder.AddAttribute(3, "lang", LanguageTag);
        builder.AddAttribute(4, "data-theme", Theme);
        builder.AddAttribute(5, "data-grayscale", Grayscale);
        builder.AddMultipleAttributes(6, AdditionalAttributes);
        builder.AddContent(7, ChildContent);
        builder.CloseElement();
    }

    // Keep the display-text host call in a member position: render-position references
    // alone make RazorVue import collection emit a phantom class-name binding that the
    // helper module does not export (components/admin/display-text.mjs exports Normalize only).
    // 让 display-text 宿主调用保持在成员位置：仅有渲染位引用时，导入收集会额外发出
    // helper 模块并未导出的类名绑定，导致浏览器模块链接失败。
    private string? LanguageTag
        => AdminDisplayTextHelper.Normalize(Language);

    private static void EnsureStylesRegistered()
    {
        // styles.mjs 只导出 EnsureLoaded；渲染位的 AdminStyleSheet 限定引用同样会触发
        // phantom 类名导入，因此经由本方法间接调用。
        AdminStyleSheet.EnsureLoaded();
    }

    private static string GetThemeCssClass(AdminThemeMode theme)
        => theme switch
        {
            AdminThemeMode.Light => "ja-application--light",
            AdminThemeMode.Dark => "ja-application--dark",
            _ => "ja-application--system"
        };
}
