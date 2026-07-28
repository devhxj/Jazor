namespace JazorAdmin;

[String]
public enum ActionNoticeKind
{
    [Description("@#info")]
    Info,

    [Description("@#success")]
    Success,

    [Description("@#warning")]
    Warning,

    [Description("@#error")]
    Error
}

[ECMAScriptModule("./components/jazor-admin-action-notice")]
public partial class ActionNotice : AppComponentBase, IVueContainerComponent
{
    [Parameter]
    public ActionNoticeKind Kind { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public bool Dismissible { get; set; }

    [Parameter]
    public EventCallback Dismissed { get; set; }

    private VueClassValue RootCssClass
        => BuildCssClass("jazor-admin-action-notice", GetKindCssClass(Kind));

    private string? NormalizedTitle => Text.Normalize(Title);

    private string? NormalizedDescription => Text.Normalize(Description);

    private bool HasContent => NormalizedTitle is not null || NormalizedDescription is not null;

    private bool IsAssertive
        => Kind == ActionNoticeKind.Warning || Kind == ActionNoticeKind.Error;

    private Task OnDismissed()
        => Dismissed.InvokeAsync();

    private static string GetKindCssClass(ActionNoticeKind kind)
        => kind switch
        {
            ActionNoticeKind.Success => "jazor-admin-action-notice--success",
            ActionNoticeKind.Warning => "jazor-admin-action-notice--warning",
            ActionNoticeKind.Error => "jazor-admin-action-notice--error",
            _ => "jazor-admin-action-notice--info"
        };
}
