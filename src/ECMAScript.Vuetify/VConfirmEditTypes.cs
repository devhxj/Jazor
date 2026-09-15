using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

// Defines VConfirmEdit action values and scoped-slot context contracts.
// 定义 VConfirmEdit 的操作值和作用域插槽上下文合同；可擦除值域使用原生 union。

/// <summary>
/// Vuetify 确认编辑操作枚举。
/// Vuetify confirm edit action enum.
/// </summary>
[String]
public enum VuetifyConfirmEditAction
{
    /// <summary>
    /// 显示保存操作；上游取值为 “save”。
    /// </summary>
    [Description("@#save")]
    Save,

    /// <summary>
    /// 显示取消操作；上游取值为 “cancel”。
    /// </summary>
    [Description("@#cancel")]
    Cancel
}

/// <summary>
/// Vuetify 确认编辑操作集合。
/// Collection of Vuetify confirm edit actions.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyConfirmEditActionsCollectionBuilder), nameof(VuetifyConfirmEditActionsCollectionBuilder.Create))]
public readonly union VuetifyConfirmEditActions(VuetifyConfirmEditAction[]) : IEnumerable<VuetifyConfirmEditAction>
{
    /// <summary>
    /// 读取当前值的 VuetifyConfirmEditAction[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyConfirmEditAction[]? AsArray => Value as VuetifyConfirmEditAction[];

    /// <summary>
    /// 将 VuetifyConfirmEditAction[] 值转换为 VuetifyConfirmEditActions，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyConfirmEditActions(VuetifyConfirmEditAction[] actions)
        => new(actions);

    IEnumerator<VuetifyConfirmEditAction> IEnumerable<VuetifyConfirmEditAction>.GetEnumerator()
        => ((IEnumerable<VuetifyConfirmEditAction>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyConfirmEditAction>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyConfirmEditActionsCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    public static VuetifyConfirmEditActions Create(ReadOnlySpan<VuetifyConfirmEditAction> actions)
        => actions.ToArray();
}

/// <summary>
/// Vuetify 确认编辑禁用状态值，支持布尔值或操作数组。
/// Vuetify confirm edit disabled state value, supporting boolean or action array.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyConfirmEditDisabled(bool, VuetifyConfirmEditActions)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VuetifyConfirmEditActions 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyConfirmEditActions? AsActions
        => Value is VuetifyConfirmEditActions value ? value : default(VuetifyConfirmEditActions?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyConfirmEditDisabled，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyConfirmEditDisabled(bool value)
        => new(value);

    /// <summary>
    /// 将 VuetifyConfirmEditActions 值转换为 VuetifyConfirmEditDisabled，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyConfirmEditDisabled(VuetifyConfirmEditActions value)
        => new(value);

    /// <summary>
    /// 将 VuetifyConfirmEditAction[] 值转换为 VuetifyConfirmEditDisabled，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyConfirmEditDisabled(VuetifyConfirmEditAction[] value)
        => new((VuetifyConfirmEditActions)value);
}

/// <summary>
/// 用于 VConfirmEditSlotContext.Actions 的回调签名。
/// 渲染确认编辑控件的操作区域。
/// </summary>
public delegate IVNode VConfirmEditActionsCallback(VueProps? props = null);

/// <summary>
/// Vuetify VConfirmEdit 公开的默认插槽上下文。
/// Default slot context exposed by Vuetify VConfirmEdit.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VConfirmEditSlotContext
{
    /// <summary>
    /// 控件内部正在编辑的模型引用；自定义插槽应与此引用同步值。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#model")]
    public IVueRef<VueValue?>? Model { get; init; }

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

    /// <summary>
    /// 渲染确认编辑控件的操作区域。
    /// </summary>
    [Description("@#actions")]
    public VConfirmEditActionsCallback? Actions { get; init; }
}