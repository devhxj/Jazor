namespace ECMAScript.Vuetify;

public delegate string VDateInputDisplayFormatCallback(string? value);

/// <summary>
/// Vuetify VDateInput 接受的显示格式值。
/// Display-format value accepted by Vuetify VDateInput.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VDateInputDisplayFormatValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly VDateInputDisplayFormatCallback? _callback;

    public VDateInputDisplayFormatValue(string value)
    {
        _kind = 1;
        _string = value;
        _callback = default;
    }

    public VDateInputDisplayFormatValue(VDateInputDisplayFormatCallback value)
    {
        _kind = 2;
        _string = default;
        _callback = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public VDateInputDisplayFormatCallback? AsCallback => _kind == 2 ? _callback : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsCallback,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VDateInputDisplayFormatValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VDateInputDisplayFormatValue From(VDateInputDisplayFormatCallback value);

    public static implicit operator VDateInputDisplayFormatValue(string value)
        => new(value);

    public static implicit operator VDateInputDisplayFormatValue(VDateInputDisplayFormatCallback value)
        => new(value);
}

/// <summary>
/// Vuetify VDateInput 操作插槽上下文。
/// Actions slot context exposed by Vuetify VDateInput.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VDateInputActionsSlotContext
{
    [Description("@#save")]
    public Action? Save { get; init; }

    [Description("@#cancel")]
    public Action? Cancel { get; init; }

    [Description("@#isPristine")]
    public bool IsPristine { get; init; }
}
