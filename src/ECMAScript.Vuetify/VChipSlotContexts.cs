namespace ECMAScript.Vuetify;

// Defines VChip scoped-slot contexts and the selected-class value domain.
// 定义 VChip 作用域插槽上下文和 selected-class 值域；可擦除值域使用原生 union。

/// <summary>
/// Vuetify VChip 公开的作用域默认插槽上下文。
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
/// Vuetify VChip 默认插槽 selectedClass 使用的值类型。
/// Value shape used by Vuetify VChip default slot selectedClass.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VChipSelectedClassValue(bool, string?[])
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public string?[]? AsClasses => Value as string?[];

    public static implicit operator VChipSelectedClassValue(bool value)
        => new(value);

    public static implicit operator VChipSelectedClassValue(string?[] value)
        => new(value);
}
