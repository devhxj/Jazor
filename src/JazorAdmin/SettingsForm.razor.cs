namespace JazorAdmin;

using Microsoft.AspNetCore.Components.Web;

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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var fields = BuildEffectiveFields(Fields?.AsArray);
        var statusText = Text.Normalize(StatusText);

        builder.OpenElement(0, "form");
        builder.AddAttribute(1, "class", RootCssClass);
        builder.AddAttribute(2, "style", CssStyle);
        builder.AddAttribute(4, "onsubmit", EventCallback.Factory.Create(this, OnSubmit));
        WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(builder, 5, "onsubmit", true);

        foreach (var field in fields)
        {
            var source = field.Source;
            var checkbox = source.Kind == SettingsFieldKind.Checkbox;
            builder.OpenElement(4, "div");
            builder.AddAttribute(5, "class", checkbox ? "jazor-admin-settings-form__checkbox-field" : "jazor-admin-settings-form__field");

            if (checkbox)
            {
                builder.OpenElement(6, "input");
                builder.AddAttribute(7, "id", field.InputId);
                builder.AddAttribute(8, "class", "jazor-admin-settings-form__checkbox");
                builder.AddAttribute(9, "data-form-field", field.Key);
                builder.AddAttribute(10, "name", field.Key);
                builder.AddAttribute(11, "type", "checkbox");
                builder.AddAttribute(12, "checked", source.Checked ?? false);
                builder.AddAttribute(13, "required", source.Required ?? false);
                builder.AddAttribute(14, "disabled", source.Disabled ?? false);
                builder.AddAttribute(15, "aria-describedby", field.HelpId);
                builder.AddAttribute(16, "onchange", EventCallback.Factory.Create(this, () => OnCheckboxFieldChanged(field.Key, source.Checked ?? false)));
                builder.CloseElement();
            }

            builder.OpenElement(17, "label");
            builder.AddAttribute(18, "class", "jazor-admin-settings-form__label");
            builder.AddAttribute(19, "for", field.InputId);
            builder.AddContent(20, field.Label);
            builder.CloseElement();

            if (source.Kind == SettingsFieldKind.Text)
            {
                builder.OpenElement(21, "input");
                builder.AddAttribute(22, "id", field.InputId);
                builder.AddAttribute(23, "class", "jazor-admin-settings-form__input");
                builder.AddAttribute(24, "data-form-field", field.Key);
                builder.AddAttribute(25, "name", field.Key);
                builder.AddAttribute(26, "type", source.TextType ?? SettingsTextFieldType.Text);
                builder.AddAttribute(27, "value", source.Value ?? string.Empty);
                builder.AddAttribute(28, "placeholder", Text.Normalize(source.Placeholder));
                builder.AddAttribute(29, "autocomplete", Text.Normalize(source.Autocomplete));
                builder.AddAttribute(30, "required", source.Required ?? false);
                builder.AddAttribute(31, "disabled", source.Disabled ?? false);
                builder.AddAttribute(32, "aria-describedby", field.HelpId);
                builder.AddAttribute(33, "oninput", EventCallback.Factory.Create<string>(this, value => OnTextFieldChanged(field.Key, value)));
                builder.SetUpdatesAttributeName("value");
                builder.CloseElement();
            }
            else if (source.Kind == SettingsFieldKind.Select)
            {
                builder.OpenElement(34, "select");
                builder.AddAttribute(35, "id", field.InputId);
                builder.AddAttribute(36, "class", "jazor-admin-settings-form__select");
                builder.AddAttribute(37, "data-form-field", field.Key);
                builder.AddAttribute(38, "name", field.Key);
                builder.AddAttribute(39, "value", source.Value ?? string.Empty);
                builder.AddAttribute(40, "required", source.Required ?? false);
                builder.AddAttribute(41, "disabled", source.Disabled ?? false);
                builder.AddAttribute(42, "aria-describedby", field.HelpId);
                builder.AddAttribute(43, "onchange", EventCallback.Factory.Create<string>(this, value => OnSelectFieldChanged(field.Key, value)));
                builder.SetUpdatesAttributeName("value");

                foreach (var option in BuildEffectiveOptions(source.Options?.AsArray))
                {
                    builder.OpenElement(44, "option");
                    builder.AddAttribute(45, "value", option.Value);
                    builder.AddAttribute(46, "disabled", option.Disabled ?? false);
                    builder.AddContent(47, Text.Normalize(option.Label) ?? option.Value);
                    builder.CloseElement();
                }

                builder.CloseElement();
            }

            if (field.HelpText is not null)
            {
                builder.OpenElement(48, "small");
                builder.AddAttribute(49, "id", field.HelpId);
                builder.AddAttribute(50, "class", "jazor-admin-settings-form__help");
                builder.AddContent(51, field.HelpText);
                builder.CloseElement();
            }

            builder.CloseElement();
        }

        builder.OpenElement(52, "button");
        builder.AddAttribute(53, "class", "jazor-admin-settings-form__submit");
        builder.AddAttribute(54, "data-form-action", "submit");
        builder.AddAttribute(55, "type", "submit");
        builder.AddAttribute(56, "disabled", SubmitDisabled);
        builder.AddContent(58, Text.Normalize(SubmitText) ?? "Save");
        builder.CloseElement();

        if (statusText is not null)
        {
            builder.OpenElement(59, "p");
            builder.AddAttribute(60, "class", "jazor-admin-settings-form__status");
            builder.AddAttribute(61, "aria-live", "polite");
            builder.AddContent(62, statusText);
            builder.CloseElement();
        }

        builder.CloseElement();
    }

    private Task OnSubmit()
        => Submit.InvokeAsync();

    private Task OnTextFieldChanged(string key, string value)
        => NotifyFieldChanged(key, value);

    private Task OnSelectFieldChanged(string key, string value)
        => NotifyFieldChanged(key, value);

    private Task OnCheckboxFieldChanged(string key, bool checkedValue)
        => NotifyFieldChanged(key, !checkedValue);

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
