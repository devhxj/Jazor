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
    /// <summary>
    /// 当前条目是否已选中。
    /// </summary>
    [Description("@#isSelected")]
    public bool? IsSelected { get; init; }

    /// <summary>
    /// 条目被选中时使用的 CSS 类名。
    /// </summary>
    [Description("@#selectedClass")]
    public VChipSelectedClassValue? SelectedClass { get; init; }

    /// <summary>
    /// 根据传入的目标和布尔值更新选择状态。
    /// </summary>
    [Description("@#select")]
    public VChipSelectCallback? Select { get; init; }

    /// <summary>
    /// 切换当前条目的选中状态。
    /// </summary>
    [Description("@#toggle")]
    public Action? Toggle { get; init; }

    /// <summary>
    /// 此条目关联的模型值，用于渲染和更新回调。
    /// </summary>
    [Description("@#value")]
    public VuetifyGroupModelValue? Value { get; init; }

    /// <summary>
    /// 是否禁止此项的用户交互。
    /// </summary>
    [Description("@#disabled")]
    public bool Disabled { get; init; }
}

/// <summary>
/// 用于 VChipDefaultSlotContext.Select 的回调签名。
/// 根据传入的目标和布尔值更新选择状态。
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
public delegate void VChipSelectCallback(bool value);

/// <summary>
/// Vuetify VChip 默认插槽 selectedClass 使用的值类型。
/// Value shape used by Vuetify VChip default slot selectedClass.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VChipSelectedClassValue(bool, string?[])
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 string?[] 分支；不属于该分支时返回 null。
    /// </summary>
    public string?[]? AsClasses => Value as string?[];

    /// <summary>
    /// 将 bool 值转换为 VChipSelectedClassValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VChipSelectedClassValue(bool value)
        => new(value);

    /// <summary>
    /// 将 string?[] 值转换为 VChipSelectedClassValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VChipSelectedClassValue(string?[] value)
        => new(value);
}