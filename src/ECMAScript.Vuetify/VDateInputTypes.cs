namespace ECMAScript.Vuetify;

// Defines VDateInput display-format values and actions-slot context.
// 定义 VDateInput 的显示格式值和操作插槽上下文；可擦除值域使用原生 union。

public delegate string VDateInputDisplayFormatCallback(string? value);

/// <summary>
/// Vuetify VDateInput 接受的显示格式值。
/// Display-format value accepted by Vuetify VDateInput.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VDateInputDisplayFormatValue(string, VDateInputDisplayFormatCallback)
{
    public string? AsString => Value as string;

    public VDateInputDisplayFormatCallback? AsCallback => Value as VDateInputDisplayFormatCallback;

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
