using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vben.TDesign;

[ECMAScriptModule("./components/vben-tdesign-page-container")]
public partial class VbenTDesignPageContainer : VbenContentComponentBase
{
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public VbenBreadcrumbItem[]? BreadcrumbItems { get; set; }

    [Parameter]
    public VbenPageAction[]? Actions { get; set; }

    [Parameter]
    public RenderFragment? Extra { get; set; }

    private VueClassValue RootCssClass
        => BuildCssClass("vben-tdesign-page-container");

    private static TDesignButtonTheme? MapTheme(VbenPageActionKind? kind) => kind switch
    {
        VbenPageActionKind.Primary => TDesignButtonTheme.Primary,
        VbenPageActionKind.Danger => TDesignButtonTheme.Danger,
        VbenPageActionKind.Secondary => TDesignButtonTheme.Default,
        VbenPageActionKind.Link => TDesignButtonTheme.Default,
        _ => TDesignButtonTheme.Default
    };

    private static TDesignMenuRouteTarget? MapRoute(VbenRouteLocation? route)
    {
        if (route is null)
            return null;

        return new TDesignMenuRoute
        {
            Path = route.Path,
            Name = route.Name,
            Hash = route.Hash
        };
    }
}
