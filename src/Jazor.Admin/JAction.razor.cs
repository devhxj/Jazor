namespace Jazor.Admin;

/// <summary>
/// Renders one page action using the shared RouteTarget, Href, and button contract.
/// </summary>
[ECMAScriptModule("./components/admin/page-action")]
public partial class JAction : JComponentBase, IVueContainerComponent
{
    [Parameter]
    public AdminPageAction Action { get; set; } = new();

    private string? Text => AdminDisplayTextHelper.Normalize(Action.Text);

    private bool IsDisabled => Action.Disabled ?? false;

    private bool AriaDisabled => IsDisabled && Target.IsNavigable;

    // Keep target resolution in one place so page actions and breadcrumbs follow the same
    // RouteTarget-first navigation rule.
    private AdminNavigationTargetResolver.ResolvedNavigationTarget Target
        => AdminNavigationTargetResolver.Resolve(Action.Href, Action.RouteTarget);

    private string CssClassValue => BuildActionCssClass(Action);

    private static string BuildActionCssClass(AdminPageAction action)
    {
        var classes = new List<string>(3)
        {
            "ja-page__action",
            $"ja-page__action--{MapActionKindSuffix(action.Kind)}"
        };

        if (action.Disabled ?? false)
        {
            classes.Add("is-disabled");
        }

        return string.Join(" ", classes);
    }

    private static string MapActionKindSuffix(AdminPageActionKind? kind) => kind switch
    {
        AdminPageActionKind.Primary => "primary",
        AdminPageActionKind.Secondary => "secondary",
        AdminPageActionKind.Link => "link",
        AdminPageActionKind.Danger => "danger",
        _ => "default"
    };
}
