namespace ECMAScript.Vuetify;

/// <summary>
/// Scoped default slot context exposed by Vuetify VChip.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VChipDefaultSlotContext
{
    [Description("@#isSelected")]
    public bool? IsSelected { get; init; }

    [Description("@#selectedClass")]
    public VChipSelectedClassValue? SelectedClass { get; init; }

    [Description("@#select")]
    public VChipSelectCallback? Select { get; init; }

    [Description("@#toggle")]
    public Action? Toggle { get; init; }

    [Description("@#value")]
    public VuetifyGroupModelValue? Value { get; init; }

    [Description("@#disabled")]
    public bool Disabled { get; init; }
}

public delegate void VChipSelectCallback(bool value);

/// <summary>
/// Value shape used by Vuetify VChip default slot selectedClass.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VChipSelectedClassValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly string?[]? _classes;

    private VChipSelectedClassValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _classes = default;
    }

    private VChipSelectedClassValue(string?[] value)
    {
        _kind = 2;
        _bool = default;
        _classes = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public string?[]? AsClasses => _kind == 2 ? _classes : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsClasses,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VChipSelectedClassValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VChipSelectedClassValue From(string?[] value);

    public static implicit operator VChipSelectedClassValue(bool value)
        => new(value);

    public static implicit operator VChipSelectedClassValue(string?[] value)
        => new(value);
}
