using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 覆盖层偏移值集合。
/// Vuetify overlay offset value collection.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyOverlayOffsetValuesCollectionBuilder), nameof(VuetifyOverlayOffsetValuesCollectionBuilder.Create))]
public readonly union VuetifyOverlayOffsetValues(Number[]) : IEnumerable<Number>
{
    /// <summary>
    /// 读取当前值的 Number[] 分支；不属于该分支时返回 null。
    /// </summary>
    public Number[]? AsArray => Value as Number[];

    /// <summary>
    /// 将 Number[] 值转换为 VuetifyOverlayOffsetValues，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayOffsetValues(Number[] values)
        => new(values);

    /// <summary>
    /// 将 int[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayOffsetValues(int[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    /// <summary>
    /// 将 double[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayOffsetValues(double[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    IEnumerator<Number> IEnumerable<Number>.GetEnumerator()
        => ((IEnumerable<Number>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<Number>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyOverlayOffsetValuesCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    public static VuetifyOverlayOffsetValues Create(ReadOnlySpan<Number> values)
        => values.ToArray();
}

/// <summary>
/// 用于 VSnackbar.ActivatorTarget 的参数类型。
/// 激活器目标元素。
/// Activator target element.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyOverlayActivatorTarget(Element, VueComponentPublicInstance, string)
{
    /// <summary>
    /// 读取当前值的 Element 分支；不属于该分支时返回 null。
    /// </summary>
    public Element? AsElement => Value as Element;

    /// <summary>
    /// 读取当前值的 VueComponentPublicInstance 分支；不属于该分支时返回 null。
    /// </summary>
    public VueComponentPublicInstance? AsComponent => Value as VueComponentPublicInstance;

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 将父元素作为浮层激活目标，传入上游 parent 标识。
    /// </summary>
    [ECMAScriptInline("'parent'")]
    public extern static VuetifyOverlayActivatorTarget Parent();

    /// <summary>
    /// 将 Element 值转换为 VuetifyOverlayActivatorTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayActivatorTarget(Element value)
        => new(value);

    /// <summary>
    /// 将 VueComponentPublicInstance 值转换为 VuetifyOverlayActivatorTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayActivatorTarget(VueComponentPublicInstance value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyOverlayActivatorTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayActivatorTarget(string value)
        => new(value);
}

/// <summary>
/// 用于 VDialog.Offset、VSnackbar.Offset、VSnackbarQueue.Offset、VSpeedDial.Offset 的参数类型。
/// Increases distance from the target. When passed as a pair of numbers, the second value shifts anchor along the side and away from the target.
/// 消息条相对于锚点的偏移量。
/// Offset of the snackbar relative to its anchor.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyOverlayOffsetValue(string, Number, VuetifyOverlayOffsetValues)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 VuetifyOverlayOffsetValues 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyOverlayOffsetValues? AsValues
        => Value is VuetifyOverlayOffsetValues value ? value : default(VuetifyOverlayOffsetValues?);

    /// <summary>
    /// 将 string 值转换为 VuetifyOverlayOffsetValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayOffsetValue(string value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifyOverlayOffsetValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayOffsetValue(Number value)
        => new(value);

    /// <summary>
    /// 将 VuetifyOverlayOffsetValues 值转换为 VuetifyOverlayOffsetValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayOffsetValue(VuetifyOverlayOffsetValues value)
        => new(value);

    /// <summary>
    /// 将 byte 值转换为 VuetifyOverlayOffsetValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayOffsetValue(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifyOverlayOffsetValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayOffsetValue(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifyOverlayOffsetValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayOffsetValue(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifyOverlayOffsetValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayOffsetValue(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifyOverlayOffsetValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayOffsetValue(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifyOverlayOffsetValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayOffsetValue(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifyOverlayOffsetValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayOffsetValue(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifyOverlayOffsetValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayOffsetValue(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifyOverlayOffsetValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayOffsetValue(decimal value)
        => new((Number)value);
}

/// <summary>
/// 浮层过渡原点的定位方式。
/// </summary>
[String]
public enum VuetifyOriginMode
{
    /// <summary>
    /// 自动选择；上游取值为 “auto”。
    /// </summary>
    [Description("@#auto")]
    Auto,

    /// <summary>
    /// 过渡原点与浮层重叠定位；上游取值为 “overlap”。
    /// </summary>
    [Description("@#overlap")]
    Overlap
}

/// <summary>
/// 用于 VDialog.Origin、VSnackbar.Origin、VSnackbarQueue.Origin、VSpeedDial.Origin 的参数类型。
/// Sets the anchor point on the overlay content that aligns to the `location` anchor on the target. `auto` uses the opposing side of `location`; `overlap` uses the same anchor, causing the overlay to cover the target. Also sets the CSS `transform-origin` for enter/leave transitions.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyOriginValue(VuetifyLocation, VuetifyOriginMode, string)
{
    /// <summary>
    /// 读取当前值的 VuetifyLocation 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyLocation? AsLocation
        => Value is VuetifyLocation value ? value : default(VuetifyLocation?);

    /// <summary>
    /// 读取当前值的 VuetifyOriginMode 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyOriginMode? AsMode
        => Value is VuetifyOriginMode value ? value : default(VuetifyOriginMode?);

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsCustom => Value as string;

    /// <summary>
    /// 将 VuetifyLocation 值转换为 VuetifyOriginValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOriginValue(VuetifyLocation value)
        => new(value);

    /// <summary>
    /// 将 VuetifyOriginMode 值转换为 VuetifyOriginValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOriginValue(VuetifyOriginMode value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyOriginValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOriginValue(string value)
        => new(value);
}

/// <summary>
/// 使用横向和纵向坐标指定浮层定位点；保存为 [x, y] 数组。
/// </summary>
[ECMAScript]
[Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyOverlayCoordinateTargetCollectionBuilder), nameof(VuetifyOverlayCoordinateTargetCollectionBuilder.Create))]
public readonly struct VuetifyOverlayCoordinateTarget : IUnion, IEnumerable<Number>
{
    private readonly Number[]? _values;

    /// <summary>
    /// 保存恰好两个坐标组成的 [x, y] 数组；数组长度不为 2 时抛出参数异常。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    public VuetifyOverlayCoordinateTarget(Number[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length != 2)
            throw new ArgumentException("Vuetify overlay coordinate targets require exactly two items.", nameof(values));

        _values = values;
    }

    /// <summary>
    /// 读取当前值的 Number[] 分支；不属于该分支时返回 null。
    /// </summary>
    public Number[]? AsArray => _values;

    /// <summary>
    /// 定位目标的横向坐标。
    /// </summary>
    public Number? X => _values is { Length: > 0 } values ? values[0] : default(Number?);

    /// <summary>
    /// 定位目标的纵向坐标。
    /// </summary>
    public Number? Y => _values is { Length: > 1 } values ? values[1] : default(Number?);

    /// <summary>
    /// 读取当前联合分支保存的原始值，供宿主 API 传递；不会转换到其他分支。
    /// </summary>
    public object? Value => _values;

    /// <summary>
    /// 将 Number[] 值转换为 VuetifyOverlayCoordinateTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayCoordinateTarget(Number[] values)
        => new(values);

    /// <summary>
    /// 将 int[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayCoordinateTarget(int[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    /// <summary>
    /// 将 double[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayCoordinateTarget(double[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    IEnumerator<Number> IEnumerable<Number>.GetEnumerator()
        => ((IEnumerable<Number>)(_values ?? Array.Empty<Number>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<Number>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyOverlayCoordinateTargetCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    public static VuetifyOverlayCoordinateTarget Create(ReadOnlySpan<Number> values)
        => values.ToArray();
}

/// <summary>
/// 用于 VSnackbar.Target 的参数类型。
/// For locationStrategy=&quot;connected&quot;, specify an element or array of x,y coordinates that the overlay should position itself relative to. This will be the activator element by default.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyOverlayTarget(
    Element,
    VueComponentPublicInstance,
    string,
    VuetifyOverlayCoordinateTarget)
{
    /// <summary>
    /// 读取当前值的 Element 分支；不属于该分支时返回 null。
    /// </summary>
    public Element? AsElement => Value as Element;

    /// <summary>
    /// 读取当前值的 VueComponentPublicInstance 分支；不属于该分支时返回 null。
    /// </summary>
    public VueComponentPublicInstance? AsComponent => Value as VueComponentPublicInstance;

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 VuetifyOverlayCoordinateTarget 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyOverlayCoordinateTarget? AsCoordinates
        => Value is VuetifyOverlayCoordinateTarget value ? value : default(VuetifyOverlayCoordinateTarget?);

    /// <summary>
    /// 以最近指针位置定位浮层，传入上游 cursor 标识。
    /// </summary>
    [ECMAScriptInline("'cursor'")]
    public extern static VuetifyOverlayTarget Cursor();

    /// <summary>
    /// 以父元素定位浮层，传入上游 parent 标识。
    /// </summary>
    [ECMAScriptInline("'parent'")]
    public extern static VuetifyOverlayTarget Parent();

    /// <summary>
    /// 将 Element 值转换为 VuetifyOverlayTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayTarget(Element value)
        => new(value);

    /// <summary>
    /// 将 VueComponentPublicInstance 值转换为 VuetifyOverlayTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayTarget(VueComponentPublicInstance value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyOverlayTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayTarget(string value)
        => new(value);

    /// <summary>
    /// 将 VuetifyOverlayCoordinateTarget 值转换为 VuetifyOverlayTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyOverlayTarget(VuetifyOverlayCoordinateTarget value)
        => new(value);
}

/// <summary>
/// 用于 VDialog.Target 的参数类型。
/// For locationStrategy=&quot;connected&quot;, specify an element or array of x,y coordinates that the overlay should position itself relative to. This will be the activator element by default.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyDialogTarget(
    Element,
    VueComponentPublicInstance,
    string,
    VuetifyOverlayCoordinateTarget)
{
    /// <summary>
    /// 读取当前值的 Element 分支；不属于该分支时返回 null。
    /// </summary>
    public Element? AsElement => Value as Element;

    /// <summary>
    /// 读取当前值的 VueComponentPublicInstance 分支；不属于该分支时返回 null。
    /// </summary>
    public VueComponentPublicInstance? AsComponent => Value as VueComponentPublicInstance;

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 VuetifyOverlayCoordinateTarget 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyOverlayCoordinateTarget? AsCoordinates
        => Value is VuetifyOverlayCoordinateTarget value ? value : default(VuetifyOverlayCoordinateTarget?);

    /// <summary>
    /// 将最近指针位置作为对话框定位目标，传入上游 cursor 标识。
    /// </summary>
    [ECMAScriptInline("'cursor'")]
    public extern static VuetifyDialogTarget Cursor();

    /// <summary>
    /// 将父元素作为对话框定位目标，传入上游 parent 标识。
    /// </summary>
    [ECMAScriptInline("'parent'")]
    public extern static VuetifyDialogTarget Parent();

    /// <summary>
    /// 将 Element 值转换为 VuetifyDialogTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDialogTarget(Element value)
        => new(value);

    /// <summary>
    /// 将 VueComponentPublicInstance 值转换为 VuetifyDialogTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDialogTarget(VueComponentPublicInstance value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyDialogTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDialogTarget(string value)
        => new(value);

    /// <summary>
    /// 将 VuetifyOverlayCoordinateTarget 值转换为 VuetifyDialogTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDialogTarget(VuetifyOverlayCoordinateTarget value)
        => new(value);
}

/// <summary>
/// 用于 VDialog.ActivatorTarget 的参数类型。
/// 激活器目标元素。
/// Activator target element.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyDialogActivatorTarget(Element, VueComponentPublicInstance, string)
{
    /// <summary>
    /// 读取当前值的 Element 分支；不属于该分支时返回 null。
    /// </summary>
    public Element? AsElement => Value as Element;

    /// <summary>
    /// 读取当前值的 VueComponentPublicInstance 分支；不属于该分支时返回 null。
    /// </summary>
    public VueComponentPublicInstance? AsComponent => Value as VueComponentPublicInstance;

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 将对话框组件的父元素作为激活目标，传入上游 parent 标识。
    /// </summary>
    [ECMAScriptInline("'parent'")]
    public extern static VuetifyDialogActivatorTarget Parent();

    /// <summary>
    /// 将 Element 值转换为 VuetifyDialogActivatorTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDialogActivatorTarget(Element value)
        => new(value);

    /// <summary>
    /// 将 VueComponentPublicInstance 值转换为 VuetifyDialogActivatorTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDialogActivatorTarget(VueComponentPublicInstance value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyDialogActivatorTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDialogActivatorTarget(string value)
        => new(value);
}