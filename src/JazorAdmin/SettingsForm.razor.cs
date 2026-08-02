namespace JazorAdmin;

[ECMAScriptModule("./components/jazor-admin-settings-form")]
public partial class SettingsForm : AppComponentBase, IVueContainerComponent
{
    [Parameter]
    public SettingsFields? Fields { get; set; }

    [Parameter]
    public string? SubmitText { get; set; }

    [Parameter]
    public bool SubmitDisabled { get; set; }

    [Parameter]
    public string? StatusText { get; set; }

    [Parameter]
    public EventCallback Submit { get; set; }

    [Parameter]
    public EventCallback<SettingsFieldChange> FieldChanged { get; set; }

    private VueClassValue RootCssClass
        => BuildCssClass("jazor-admin-settings-form");

    private Task OnSubmit()
        => Submit.InvokeAsync();

    private Task OnTextFieldChanged(string key, string? value)
        => NotifyFieldChanged(key, value ?? string.Empty);

    private Task OnSelectFieldChanged(string key, string? value)
        => NotifyFieldChanged(key, value ?? string.Empty);

    private Task OnCheckboxFieldChanged(string key, bool checkedValue)
        => NotifyFieldChanged(key, checkedValue);

    private Task NotifyFieldChanged(string key, SettingsValue value)
        => FieldChanged.InvokeAsync(new SettingsFieldChange
        {
            Key = key,
            Value = value
        });

    private static EffectiveField[] BuildEffectiveFields(SettingsField[]? fields)
    {
        if (fields is not { Length: > 0 })
        {
            return Array.Empty<EffectiveField>();
        }

        var effectiveFields = new List<EffectiveField>(fields.Length);
        var usedKeys = new HashSet<string>();
        foreach (var field in fields)
        {
            var key = Text.Normalize(field.Key);
            var label = Text.Normalize(field.Label);
            if (key is null || label is null || !usedKeys.Add(key))
            {
                continue;
            }

            effectiveFields.Add(new EffectiveField(
                field,
                key,
                label,
                Text.Normalize(field.HelpText)));
        }

        return effectiveFields.ToArray();
    }

    private static SettingsOption[] BuildEffectiveOptions(SettingsOption[]? options)
    {
        if (options is not { Length: > 0 })
        {
            return Array.Empty<SettingsOption>();
        }

        var effectiveOptions = new List<SettingsOption>(options.Length);
        var usedValues = new HashSet<string>();
        foreach (var option in options)
        {
            var value = Text.Normalize(option.Value);
            if (value is null || !usedValues.Add(value))
            {
                continue;
            }

            effectiveOptions.Add(option with { Value = value });
        }

        return effectiveOptions.ToArray();
    }

    private sealed class EffectiveField
    {
        public EffectiveField(SettingsField source, string key, string label, string? helpText)
        {
            Source = source;
            Key = key;
            Label = label;
            HelpText = helpText;
        }

        public SettingsField Source { get; }

        public string Key { get; }

        public string Label { get; }

        public string? HelpText { get; }

        public string InputId => "jazor-admin-settings-form-field-" + Key;

        public string? HelpId => HelpText is null ? null : InputId + "-help";
    }
}
