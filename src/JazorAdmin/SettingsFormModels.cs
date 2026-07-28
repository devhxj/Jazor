namespace JazorAdmin;

[String]
public enum SettingsFieldKind
{
    [Description("@#text")]
    Text,

    [Description("@#select")]
    Select,

    [Description("@#checkbox")]
    Checkbox
}

[String]
public enum SettingsTextFieldType
{
    [Description("@#text")]
    Text,

    [Description("@#email")]
    Email,

    [Description("@#password")]
    Password,

    [Description("@#search")]
    Search,

    [Description("@#url")]
    Url
}

[ECMAScript]
[Description("@#")]
public sealed record SettingsField : VueProps
{
    [Description("@#key")]
    public string Key { get; init; } = string.Empty;

    [Description("@#kind")]
    public SettingsFieldKind Kind { get; init; }

    [Description("@#label")]
    public string? Label { get; init; }

    [Description("@#helpText")]
    public string? HelpText { get; init; }

    [Description("@#required")]
    public bool? Required { get; init; }

    [Description("@#disabled")]
    public bool? Disabled { get; init; }

    [Description("@#value")]
    public string? Value { get; init; }

    [Description("@#checked")]
    public bool? Checked { get; init; }

    [Description("@#placeholder")]
    public string? Placeholder { get; init; }

    [Description("@#autocomplete")]
    public string? Autocomplete { get; init; }

    [Description("@#textType")]
    public SettingsTextFieldType? TextType { get; init; }

    [Description("@#options")]
    public SettingsOptions? Options { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record SettingsOption : VueProps
{
    [Description("@#value")]
    public string Value { get; init; } = string.Empty;

    [Description("@#label")]
    public string? Label { get; init; }

    [Description("@#disabled")]
    public bool? Disabled { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union SettingsValue(string, bool)
{
    public string? AsString => Value as string;

    public bool? AsBoolean => Value is bool value ? value : default(bool?);
}

[ECMAScript]
[Description("@#")]
public sealed record SettingsFieldChange : VueProps
{
    [Description("@#key")]
    public string Key { get; init; } = string.Empty;

    [Description("@#value")]
    public SettingsValue Value { get; init; }
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(SettingsFieldsCollectionBuilder), nameof(SettingsFieldsCollectionBuilder.Create))]
public readonly union SettingsFields(SettingsField[]) : IEnumerable<SettingsField>
{
    public SettingsField[]? AsArray => Value as SettingsField[];

    IEnumerator<SettingsField> IEnumerable<SettingsField>.GetEnumerator()
        => ((IEnumerable<SettingsField>)(AsArray ?? Array.Empty<SettingsField>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<SettingsField>)this).GetEnumerator();
}

public static class SettingsFieldsCollectionBuilder
{
    public static SettingsFields Create(ReadOnlySpan<SettingsField> values)
        => values.ToArray();
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(SettingsOptionsCollectionBuilder), nameof(SettingsOptionsCollectionBuilder.Create))]
public readonly union SettingsOptions(SettingsOption[]) : IEnumerable<SettingsOption>
{
    public SettingsOption[]? AsArray => Value as SettingsOption[];

    IEnumerator<SettingsOption> IEnumerable<SettingsOption>.GetEnumerator()
        => ((IEnumerable<SettingsOption>)(AsArray ?? Array.Empty<SettingsOption>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<SettingsOption>)this).GetEnumerator();
}

public static class SettingsOptionsCollectionBuilder
{
    public static SettingsOptions Create(ReadOnlySpan<SettingsOption> values)
        => values.ToArray();
}
