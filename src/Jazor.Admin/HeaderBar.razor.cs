namespace Jazor.Admin;

[ECMAScriptModule("./components/jazor-admin-header-bar")]
public partial class HeaderBar : AdminComponentBase, IVueContainerComponent
{
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public RenderFragment? Logo { get; set; }

    [Parameter]
    public RenderFragment? Navigation { get; set; }

    [Parameter]
    public RenderFragment? Actions { get; set; }

    [Parameter]
    public RenderFragment? UserRegion { get; set; }

    private string? NormalizedTitle
        => AdminDisplayTextHelper.Normalize(Title);

    private string? NormalizedSubtitle
        => AdminDisplayTextHelper.Normalize(Subtitle);

    private bool HasTitles
        => NormalizedTitle is not null || NormalizedSubtitle is not null;

    private VueClassValue RootCssClass
        => BuildCssClass("jazor-admin-header");

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var logo = Logo;
        var navigation = Navigation;
        var actions = Actions;
        var userRegion = UserRegion;
        var hasMainRegion = logo is not null || HasTitles;
        var hasRightRegion = actions is not null || userRegion is not null;
        if (!hasMainRegion && navigation is null && !hasRightRegion)
        {
            return;
        }

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", RootCssClass);
        builder.AddAttribute(2, "style", CssStyle);
        builder.AddMultipleAttributes(3, AdditionalAttributes);

        if (hasMainRegion)
        {
            builder.OpenElement(4, "div");
            builder.AddAttribute(5, "class", "jazor-admin-header__main");

            if (logo is not null)
            {
                builder.OpenElement(6, "div");
                builder.AddAttribute(7, "class", "jazor-admin-header__logo");
                builder.AddContent(8, logo);
                builder.CloseElement();
            }

            if (HasTitles)
            {
                builder.OpenElement(9, "div");
                builder.AddAttribute(10, "class", "jazor-admin-header__titles");
                if (NormalizedTitle is not null)
                {
                    builder.OpenElement(11, "div");
                    builder.AddAttribute(12, "class", "jazor-admin-header__title");
                    builder.AddContent(13, NormalizedTitle);
                    builder.CloseElement();
                }

                if (NormalizedSubtitle is not null)
                {
                    builder.OpenElement(14, "div");
                    builder.AddAttribute(15, "class", "jazor-admin-header__subtitle");
                    builder.AddContent(16, NormalizedSubtitle);
                    builder.CloseElement();
                }

                builder.CloseElement();
            }

            builder.CloseElement();
        }

        if (navigation is not null)
        {
            builder.OpenElement(17, "div");
            builder.AddAttribute(18, "class", "jazor-admin-header__navigation");
            builder.AddContent(19, navigation);
            builder.CloseElement();
        }

        if (hasRightRegion)
        {
            builder.OpenElement(20, "div");
            builder.AddAttribute(21, "class", "jazor-admin-header__actions");

            if (actions is not null)
            {
                builder.OpenElement(22, "div");
                builder.AddAttribute(23, "class", "jazor-admin-header__toolbar");
                builder.AddContent(24, actions);
                builder.CloseElement();
            }

            if (userRegion is not null)
            {
                builder.OpenElement(25, "div");
                builder.AddAttribute(26, "class", "jazor-admin-header__user-region");
                builder.AddContent(27, userRegion);
                builder.CloseElement();
            }

            builder.CloseElement();
        }

        builder.CloseElement();
    }
}
