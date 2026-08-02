namespace JazorAdmin;

[ECMAScriptModule("./components/jazor-admin-settings-page")]
public partial class SettingsPage : AppComponentBase, IVueContainerComponent
{
    [Parameter]
    public SettingsFields? Fields { get; set; }

    [Parameter]
    public string? StatusText { get; set; }

    [Parameter]
    public EventCallback Submit { get; set; }

    [Parameter]
    public EventCallback<SettingsFieldChange> FieldChanged { get; set; }
}
