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
    [Description("@#save")]
    Save,

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
    public VuetifyConfirmEditAction[]? AsArray => Value as VuetifyConfirmEditAction[];

    public static implicit operator VuetifyConfirmEditActions(VuetifyConfirmEditAction[] actions)
        => new(actions);

    IEnumerator<VuetifyConfirmEditAction> IEnumerable<VuetifyConfirmEditAction>.GetEnumerator()
        => ((IEnumerable<VuetifyConfirmEditAction>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyConfirmEditAction>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyConfirmEditActionsCollectionBuilder
{
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
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public VuetifyConfirmEditActions? AsActions
        => Value is VuetifyConfirmEditActions value ? value : default(VuetifyConfirmEditActions?);

    public static implicit operator VuetifyConfirmEditDisabled(bool value)
        => new(value);

    public static implicit operator VuetifyConfirmEditDisabled(VuetifyConfirmEditActions value)
        => new(value);

    public static implicit operator VuetifyConfirmEditDisabled(VuetifyConfirmEditAction[] value)
        => new((VuetifyConfirmEditActions)value);
}

public delegate IVNode VConfirmEditActionsCallback(VueProps? props = null);

/// <summary>
/// Vuetify VConfirmEdit 公开的默认插槽上下文。
/// Default slot context exposed by Vuetify VConfirmEdit.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VConfirmEditSlotContext
{
    [Description("@#model")]
    public IVueRef<VueValue?>? Model { get; init; }

    [Description("@#save")]
    public Action? Save { get; init; }

    [Description("@#cancel")]
    public Action? Cancel { get; init; }

    [Description("@#isPristine")]
    public bool IsPristine { get; init; }

    [Description("@#actions")]
    public VConfirmEditActionsCallback? Actions { get; init; }
}
