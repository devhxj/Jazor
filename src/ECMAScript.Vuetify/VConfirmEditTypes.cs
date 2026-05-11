using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

[String]
public enum VuetifyConfirmEditAction
{
    [Description("@#save")]
    Save,

    [Description("@#cancel")]
    Cancel
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyConfirmEditActionsCollectionBuilder), nameof(VuetifyConfirmEditActionsCollectionBuilder.Create))]
public readonly struct VuetifyConfirmEditActions : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifyConfirmEditAction>
{
    private readonly VuetifyConfirmEditAction[]? _actions;

    private VuetifyConfirmEditActions(VuetifyConfirmEditAction[] actions)
    {
        _actions = actions;
    }

    public VuetifyConfirmEditAction[]? AsArray => _actions;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyConfirmEditActions From(VuetifyConfirmEditAction[] actions);

    public static implicit operator VuetifyConfirmEditActions(VuetifyConfirmEditAction[] actions)
        => new(actions);

    IEnumerator<VuetifyConfirmEditAction> IEnumerable<VuetifyConfirmEditAction>.GetEnumerator()
        => ((IEnumerable<VuetifyConfirmEditAction>)(_actions ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyConfirmEditAction>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyConfirmEditActionsCollectionBuilder
{
    public static VuetifyConfirmEditActions Create(ReadOnlySpan<VuetifyConfirmEditAction> actions)
        => actions.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyConfirmEditDisabled : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyConfirmEditActions? _actions;

    private VuetifyConfirmEditDisabled(bool value)
    {
        _kind = 1;
        _bool = value;
        _actions = default;
    }

    private VuetifyConfirmEditDisabled(VuetifyConfirmEditActions value)
    {
        _kind = 2;
        _bool = default;
        _actions = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyConfirmEditActions? AsActions => _kind == 2 ? _actions : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsActions,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyConfirmEditDisabled From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyConfirmEditDisabled From(VuetifyConfirmEditActions value);

    public static implicit operator VuetifyConfirmEditDisabled(bool value)
        => new(value);

    public static implicit operator VuetifyConfirmEditDisabled(VuetifyConfirmEditActions value)
        => new(value);

    public static implicit operator VuetifyConfirmEditDisabled(VuetifyConfirmEditAction[] value)
        => new((VuetifyConfirmEditActions)value);
}

public delegate IVNode VConfirmEditActionsCallback(VueProps? props = null);

/// <summary>
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
