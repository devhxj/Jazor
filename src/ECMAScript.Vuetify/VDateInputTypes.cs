namespace ECMAScript.Vuetify;

// Defines VDateInput display-format values and actions-slot context.
// 定义 VDateInput 的显示格式值和操作插槽上下文；可擦除值域使用原生 union。

/// <summary>
/// 用于 VDateInputDisplayFormatValue.AsCallback 的回调签名。
/// 读取当前值的 VDateInputDisplayFormatCallback 分支；不属于该分支时返回 null。
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
public delegate string VDateInputDisplayFormatCallback(string? value);

/// <summary>
/// Vuetify VDateInput 接受的显示格式值。
/// Display-format value accepted by Vuetify VDateInput.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VDateInputDisplayFormatValue(string, VDateInputDisplayFormatCallback)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 VDateInputDisplayFormatCallback 分支；不属于该分支时返回 null。
    /// </summary>
    public VDateInputDisplayFormatCallback? AsCallback => Value as VDateInputDisplayFormatCallback;

    /// <summary>
    /// 将 string 值转换为 VDateInputDisplayFormatValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VDateInputDisplayFormatValue(string value)
        => new(value);

    /// <summary>
    /// 将 VDateInputDisplayFormatCallback 值转换为 VDateInputDisplayFormatValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
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
    /// <summary>
    /// 提交当前待确认的编辑值。
    /// </summary>
    [Description("@#save")]
    public Action? Save { get; init; }

    /// <summary>
    /// 放弃待确认的编辑，将编辑值恢复到已确认模型。
    /// </summary>
    [Description("@#cancel")]
    public Action? Cancel { get; init; }

    /// <summary>
    /// 当前编辑或校验状态是否尚未被用户修改或触发。
    /// </summary>
    [Description("@#isPristine")]
    public bool IsPristine { get; init; }
}