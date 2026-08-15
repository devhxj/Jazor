namespace JazorAdmin;

[ECMAScriptModule("./components/help-dialog")]
public partial class HelpDialog : AppComponentBase
{
    [Parameter]
    public bool Visible { get; set; }

    [Parameter]
    public EventCallback<bool> VisibleChanged { get; set; }

    private Task Close()
        => VisibleChanged.InvokeAsync(false);
}
