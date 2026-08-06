using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ECMAScript.Contract;
using static ECMAScript.Vue3;

namespace ECMAScript;

/// <summary>
/// 路由记录名称联合类型，接受 string 或 Symbol。
/// Route record name union accepting string or Symbol.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouteRecordName(string, Symbol)
{
    /// <summary>
    /// 以字符串形式返回，如果不是字符串变体则返回 default。
    /// Returns as string, or default if not a string variant.
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 以 Symbol 形式返回，如果不是 Symbol 变体则返回 default。
    /// Returns as Symbol, or default if not a Symbol variant.
    /// </summary>
    public Symbol? AsSymbol => Value as Symbol;
}

/// <summary>
/// 路由记录别名联合类型，接受 string 或 string 数组。
/// Route record alias union accepting string or string array.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouteRecordAlias(string, string[])
{
    /// <summary>
    /// 以字符串形式返回，如果不是字符串变体则返回 default。
    /// Returns as string, or default if not a string variant.
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 以字符串数组形式返回，如果不是数组变体则返回 default。
    /// Returns as string array, or default if not an array variant.
    /// </summary>
    public string[]? AsStrings => Value as string[];
}

/// <summary>
/// 路由位置原始值联合类型，接受 string、RouteLocationAsPath 或 RouteLocationAsRelative。
/// Route location raw union accepting string, RouteLocationAsPath, or RouteLocationAsRelative.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouteLocationRaw(string, RouteLocationAsPath, RouteLocationAsRelative)
{
    /// <summary>
    /// 以字符串形式返回，如果不是字符串变体则返回 default。
    /// Returns as string, or default if not a string variant.
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 以路径式路由位置返回，如果不是路径变体则返回 default。
    /// Returns as path-based route location, or default if not a path variant.
    /// </summary>
    public RouteLocationAsPath? AsPath => Value as RouteLocationAsPath;

    /// <summary>
    /// 以相对式路由位置返回，如果不是相对变体则返回 default。
    /// Returns as relative route location, or default if not a relative variant.
    /// </summary>
    public RouteLocationAsRelative? AsRelative => Value as RouteLocationAsRelative;

    /// <summary>
    /// 从 RouteLocationPathRaw 隐式转换，映射为路径式路由位置。
    /// Implicitly converts from RouteLocationPathRaw, mapping to a path-based route location.
    /// </summary>
    /// <param name="value">要转换的路径式原始路由位置。The path-based raw route location to convert.</param>
    public static implicit operator RouteLocationRaw(RouteLocationPathRaw value)
        => new(new RouteLocationAsPath
        {
            Path = value.Path,
            Query = value.Query,
            Hash = value.Hash,
            Replace = value.Replace,
            Force = value.Force,
            State = value.State
        });

    /// <summary>
    /// 从 RouteLocationNamedRaw 隐式转换，映射为相对式路由位置。
    /// Implicitly converts from RouteLocationNamedRaw, mapping to a relative route location.
    /// </summary>
    /// <param name="value">要转换的命名式原始路由位置。The named raw route location to convert.</param>
    public static implicit operator RouteLocationRaw(RouteLocationNamedRaw value)
        => new(new RouteLocationAsRelative
        {
            Name = value.Name,
            Params = value.Params,
            Query = value.Query,
            Hash = value.Hash,
            Replace = value.Replace,
            Force = value.Force,
            State = value.State
        });
}

/// <summary>
/// 路由位置原始值（可能为响应式引用）联合类型，接受值、Ref 或 ReadonlyRef。
/// Route location raw (possibly reactive ref) union accepting value, Ref, or ReadonlyRef.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouteLocationRawMaybeRef(
    RouteLocationRaw,
    Vue3.IVueRef<RouteLocationRaw>,
    Vue3.VueReadonlyRef<RouteLocationRaw>,
    Vue3.IVueRef<string>,
    Vue3.IVueRef<RouteLocationAsPath>,
    Vue3.IVueRef<RouteLocationAsRelative>,
    Vue3.VueReadonlyRef<string>,
    Vue3.VueReadonlyRef<RouteLocationAsPath>,
    Vue3.VueReadonlyRef<RouteLocationAsRelative>)
{
    /// <summary>
    /// 以 RouteLocationRaw 值返回，如果不是值变体则返回 default。
    /// Returns as RouteLocationRaw value, or default if not a value variant.
    /// </summary>
    public RouteLocationRaw? AsValue => Value is RouteLocationRaw value ? value : default(RouteLocationRaw?);

    /// <summary>
    /// 以 Vue Ref 返回，如果不是 Ref 变体则返回 default。
    /// Returns as Vue Ref, or default if not a Ref variant.
    /// </summary>
    public Vue3.IVueRef<RouteLocationRaw>? AsRef => Value as Vue3.IVueRef<RouteLocationRaw>;

    /// <summary>
    /// 以 Vue ReadonlyRef 返回，如果不是 ReadonlyRef 变体则返回 default。
    /// Returns as Vue ReadonlyRef, or default if not a ReadonlyRef variant.
    /// </summary>
    public Vue3.VueReadonlyRef<RouteLocationRaw>? AsReadonlyRef => Value as Vue3.VueReadonlyRef<RouteLocationRaw>;

    /// <summary>
    /// 以字符串 Vue Ref 返回，如果不是字符串 Ref 变体则返回 default。
    /// Returns as string Vue Ref, or default if not a string Ref variant.
    /// </summary>
    public Vue3.IVueRef<string>? AsStringRef => Value as Vue3.IVueRef<string>;

    /// <summary>
    /// 以路径式路由位置 Vue Ref 返回，如果不是路径 Ref 变体则返回 default。
    /// Returns as path-based route location Vue Ref, or default if not a path Ref variant.
    /// </summary>
    public Vue3.IVueRef<RouteLocationAsPath>? AsPathRef => Value as Vue3.IVueRef<RouteLocationAsPath>;

    /// <summary>
    /// 以相对式路由位置 Vue Ref 返回，如果不是相对 Ref 变体则返回 default。
    /// Returns as relative route location Vue Ref, or default if not a relative Ref variant.
    /// </summary>
    public Vue3.IVueRef<RouteLocationAsRelative>? AsRelativeRef => Value as Vue3.IVueRef<RouteLocationAsRelative>;

    /// <summary>
    /// 以字符串 Vue ReadonlyRef 返回，如果不是字符串只读引用变体则返回 default。
    /// Returns as string Vue ReadonlyRef, or default if not a string readonly ref variant.
    /// </summary>
    public Vue3.VueReadonlyRef<string>? AsReadonlyStringRef => Value as Vue3.VueReadonlyRef<string>;

    /// <summary>
    /// 以路径式路由位置 Vue ReadonlyRef 返回，如果不是路径只读引用变体则返回 default。
    /// Returns as path-based route location Vue ReadonlyRef, or default if not a path readonly ref variant.
    /// </summary>
    public Vue3.VueReadonlyRef<RouteLocationAsPath>? AsReadonlyPathRef => Value as Vue3.VueReadonlyRef<RouteLocationAsPath>;

    /// <summary>
    /// 以相对式路由位置 Vue ReadonlyRef 返回，如果不是相对只读引用变体则返回 default。
    /// Returns as relative route location Vue ReadonlyRef, or default if not a relative readonly ref variant.
    /// </summary>
    public Vue3.VueReadonlyRef<RouteLocationAsRelative>? AsReadonlyRelativeRef => Value as Vue3.VueReadonlyRef<RouteLocationAsRelative>;

    /// <summary>
    /// 从字符串隐式转换。
    /// Implicitly converts from a string.
    /// </summary>
    /// <param name="value">要转换的路径字符串。The path string to convert.</param>
    public static implicit operator RouteLocationRawMaybeRef(string value)
        => new((RouteLocationRaw)value);

    /// <summary>
    /// 从路径式路由位置隐式转换。
    /// Implicitly converts from a path-based route location.
    /// </summary>
    /// <param name="value">要转换的路径式路由位置。The path-based route location to convert.</param>
    public static implicit operator RouteLocationRawMaybeRef(RouteLocationAsPath value)
        => new((RouteLocationRaw)value);

    /// <summary>
    /// 从相对式路由位置隐式转换。
    /// Implicitly converts from a relative route location.
    /// </summary>
    /// <param name="value">要转换的相对式路由位置。The relative route location to convert.</param>
    public static implicit operator RouteLocationRawMaybeRef(RouteLocationAsRelative value)
        => new((RouteLocationRaw)value);

    /// <summary>
    /// 从 RouteLocationRaw 的 Vue Ref 创建联合值。
    /// Creates a union value from a Vue Ref of RouteLocationRaw.
    /// </summary>
    /// <param name="value">RouteLocationRaw 的响应式引用。The reactive ref of RouteLocationRaw.</param>
    /// <returns>包含该引用的联合值。The union value containing the ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteLocationRawMaybeRef From(Vue3.IVueRef<RouteLocationRaw> value);

    /// <summary>
    /// 从 RouteLocationRaw 的 Vue ReadonlyRef 创建联合值。
    /// Creates a union value from a Vue ReadonlyRef of RouteLocationRaw.
    /// </summary>
    /// <param name="value">RouteLocationRaw 的只读响应式引用。The readonly ref of RouteLocationRaw.</param>
    /// <returns>包含该引用的联合值。The union value containing the readonly ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteLocationRawMaybeRef From(Vue3.VueReadonlyRef<RouteLocationRaw> value);

    /// <summary>
    /// 从字符串的 Vue Ref 创建联合值。
    /// Creates a union value from a Vue Ref of string.
    /// </summary>
    /// <param name="value">字符串的响应式引用。The reactive ref of string.</param>
    /// <returns>包含该引用的联合值。The union value containing the ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteLocationRawMaybeRef From(Vue3.IVueRef<string> value);

    /// <summary>
    /// 从路径式路由位置的 Vue Ref 创建联合值。
    /// Creates a union value from a Vue Ref of RouteLocationAsPath.
    /// </summary>
    /// <param name="value">路径式路由位置的响应式引用。The reactive ref of path-based route location.</param>
    /// <returns>包含该引用的联合值。The union value containing the ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteLocationRawMaybeRef From(Vue3.IVueRef<RouteLocationAsPath> value);

    /// <summary>
    /// 从相对式路由位置的 Vue Ref 创建联合值。
    /// Creates a union value from a Vue Ref of RouteLocationAsRelative.
    /// </summary>
    /// <param name="value">相对式路由位置的响应式引用。The reactive ref of relative route location.</param>
    /// <returns>包含该引用的联合值。The union value containing the ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteLocationRawMaybeRef From(Vue3.IVueRef<RouteLocationAsRelative> value);

    /// <summary>
    /// 从字符串的 Vue ReadonlyRef 创建联合值。
    /// Creates a union value from a Vue ReadonlyRef of string.
    /// </summary>
    /// <param name="value">字符串的只读响应式引用。The readonly ref of string.</param>
    /// <returns>包含该引用的联合值。The union value containing the readonly ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteLocationRawMaybeRef From(Vue3.VueReadonlyRef<string> value);

    /// <summary>
    /// 从路径式路由位置的 Vue ReadonlyRef 创建联合值。
    /// Creates a union value from a Vue ReadonlyRef of RouteLocationAsPath.
    /// </summary>
    /// <param name="value">路径式路由位置的只读响应式引用。The readonly ref of path-based route location.</param>
    /// <returns>包含该引用的联合值。The union value containing the readonly ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteLocationRawMaybeRef From(Vue3.VueReadonlyRef<RouteLocationAsPath> value);

    /// <summary>
    /// 从相对式路由位置的 Vue ReadonlyRef 创建联合值。
    /// Creates a union value from a Vue ReadonlyRef of RouteLocationAsRelative.
    /// </summary>
    /// <param name="value">相对式路由位置的只读响应式引用。The readonly ref of relative route location.</param>
    /// <returns>包含该引用的联合值。The union value containing the readonly ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteLocationRawMaybeRef From(Vue3.VueReadonlyRef<RouteLocationAsRelative> value);
}

/// <summary>
/// 路由布尔值（可能为响应式引用）联合类型，接受 bool、Ref 或 ReadonlyRef。
/// Route boolean (possibly reactive ref) union accepting bool, Ref, or ReadonlyRef.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouteBooleanMaybeRef(bool, Vue3.IVueRef<bool>, Vue3.VueReadonlyRef<bool>)
{
    /// <summary>
    /// 以布尔值返回，如果不是值变体则返回 default。
    /// Returns as bool, or default if not a value variant.
    /// </summary>
    public bool? AsValue => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 以 Vue Ref 返回，如果不是 Ref 变体则返回 default。
    /// Returns as Vue Ref, or default if not a Ref variant.
    /// </summary>
    public Vue3.IVueRef<bool>? AsRef => Value as Vue3.IVueRef<bool>;

    /// <summary>
    /// 以 Vue ReadonlyRef 返回，如果不是 ReadonlyRef 变体则返回 default。
    /// Returns as Vue ReadonlyRef, or default if not a ReadonlyRef variant.
    /// </summary>
    public Vue3.VueReadonlyRef<bool>? AsReadonlyRef => Value as Vue3.VueReadonlyRef<bool>;

    /// <summary>
    /// 从布尔值的 Vue Ref 创建联合值。
    /// Creates a union value from a Vue Ref of bool.
    /// </summary>
    /// <param name="value">布尔值的响应式引用。The reactive ref of bool.</param>
    /// <returns>包含该引用的联合值。The union value containing the ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteBooleanMaybeRef From(Vue3.IVueRef<bool> value);

    /// <summary>
    /// 从布尔值的 Vue ReadonlyRef 创建联合值。
    /// Creates a union value from a Vue ReadonlyRef of bool.
    /// </summary>
    /// <param name="value">布尔值的只读响应式引用。The readonly ref of bool.</param>
    /// <returns>包含该引用的联合值。The union value containing the readonly ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteBooleanMaybeRef From(Vue3.VueReadonlyRef<bool> value);
}

/// <summary>
/// RouterView 深度值联合类型，接受 Number 或 Number 的 Vue Ref。
/// RouterView depth value union accepting Number or Vue Ref of Number.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouterViewDepthValue(Number, Vue3.IVueRef<Number>)
{
    /// <summary>
    /// 以 Number 返回，如果不是 Number 变体则返回 default。
    /// Returns as Number, or default if not a Number variant.
    /// </summary>
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 以 Vue Ref 返回，如果不是 Ref 变体则返回 default。
    /// Returns as Vue Ref, or default if not a Ref variant.
    /// </summary>
    public Vue3.IVueRef<Number>? AsRef => Value as Vue3.IVueRef<Number>;

    /// <summary>
    /// 从 Number 隐式转换。
    /// Implicitly converts from a Number.
    /// </summary>
    /// <param name="value">要转换的 Number 值。The Number value to convert.</param>
    public static implicit operator RouterViewDepthValue(Number value)
        => new(value);

    /// <summary>
    /// 从 byte 隐式转换。
    /// Implicitly converts from a byte.
    /// </summary>
    /// <param name="value">要转换的 byte 值。The byte value to convert.</param>
    public static implicit operator RouterViewDepthValue(byte value)
        => new(value);

    /// <summary>
    /// 从 sbyte 隐式转换。
    /// Implicitly converts from an sbyte.
    /// </summary>
    /// <param name="value">要转换的 sbyte 值。The sbyte value to convert.</param>
    public static implicit operator RouterViewDepthValue(sbyte value)
        => new(value);

    /// <summary>
    /// 从 short 隐式转换。
    /// Implicitly converts from a short.
    /// </summary>
    /// <param name="value">要转换的 short 值。The short value to convert.</param>
    public static implicit operator RouterViewDepthValue(short value)
        => new(value);

    /// <summary>
    /// 从 ushort 隐式转换。
    /// Implicitly converts from a ushort.
    /// </summary>
    /// <param name="value">要转换的 ushort 值。The ushort value to convert.</param>
    public static implicit operator RouterViewDepthValue(ushort value)
        => new(value);

    /// <summary>
    /// 从 int 隐式转换。
    /// Implicitly converts from an int.
    /// </summary>
    /// <param name="value">要转换的 int 值。The int value to convert.</param>
    public static implicit operator RouterViewDepthValue(int value)
        => new(value);

    /// <summary>
    /// 从 uint 隐式转换。
    /// Implicitly converts from a uint.
    /// </summary>
    /// <param name="value">要转换的 uint 值。The uint value to convert.</param>
    public static implicit operator RouterViewDepthValue(uint value)
        => new(value);

    /// <summary>
    /// 从 float 隐式转换。
    /// Implicitly converts from a float.
    /// </summary>
    /// <param name="value">要转换的 float 值。The float value to convert.</param>
    public static implicit operator RouterViewDepthValue(float value)
        => new(value);

    /// <summary>
    /// 从 double 隐式转换。
    /// Implicitly converts from a double.
    /// </summary>
    /// <param name="value">要转换的 double 值。The double value to convert.</param>
    public static implicit operator RouterViewDepthValue(double value)
        => new(value);

    /// <summary>
    /// 从 decimal 隐式转换。
    /// Implicitly converts from a decimal.
    /// </summary>
    /// <param name="value">要转换的 decimal 值。The decimal value to convert.</param>
    public static implicit operator RouterViewDepthValue(decimal value)
        => new(value);

    /// <summary>
    /// 从 Number 的 Vue Ref 创建联合值。
    /// Creates a union value from a Vue Ref of Number.
    /// </summary>
    /// <param name="value">Number 的响应式引用。The reactive ref of Number.</param>
    /// <returns>包含该引用的联合值。The union value containing the ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouterViewDepthValue From(Vue3.IVueRef<Number> value);

    /// <summary>
    /// 从 byte 的 Vue Ref 创建联合值。
    /// Creates a union value from a Vue Ref of byte.
    /// </summary>
    /// <param name="value">byte 的响应式引用。The reactive ref of byte.</param>
    /// <returns>包含该引用的联合值。The union value containing the ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouterViewDepthValue From(Vue3.IVueRef<byte> value);

    /// <summary>
    /// 从 sbyte 的 Vue Ref 创建联合值。
    /// Creates a union value from a Vue Ref of sbyte.
    /// </summary>
    /// <param name="value">sbyte 的响应式引用。The reactive ref of sbyte.</param>
    /// <returns>包含该引用的联合值。The union value containing the ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouterViewDepthValue From(Vue3.IVueRef<sbyte> value);

    /// <summary>
    /// 从 short 的 Vue Ref 创建联合值。
    /// Creates a union value from a Vue Ref of short.
    /// </summary>
    /// <param name="value">short 的响应式引用。The reactive ref of short.</param>
    /// <returns>包含该引用的联合值。The union value containing the ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouterViewDepthValue From(Vue3.IVueRef<short> value);

    /// <summary>
    /// 从 ushort 的 Vue Ref 创建联合值。
    /// Creates a union value from a Vue Ref of ushort.
    /// </summary>
    /// <param name="value">ushort 的响应式引用。The reactive ref of ushort.</param>
    /// <returns>包含该引用的联合值。The union value containing the ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouterViewDepthValue From(Vue3.IVueRef<ushort> value);

    /// <summary>
    /// 从 int 的 Vue Ref 创建联合值。
    /// Creates a union value from a Vue Ref of int.
    /// </summary>
    /// <param name="value">int 的响应式引用。The reactive ref of int.</param>
    /// <returns>包含该引用的联合值。The union value containing the ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouterViewDepthValue From(Vue3.IVueRef<int> value);

    /// <summary>
    /// 从 uint 的 Vue Ref 创建联合值。
    /// Creates a union value from a Vue Ref of uint.
    /// </summary>
    /// <param name="value">uint 的响应式引用。The reactive ref of uint.</param>
    /// <returns>包含该引用的联合值。The union value containing the ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouterViewDepthValue From(Vue3.IVueRef<uint> value);

    /// <summary>
    /// 从 float 的 Vue Ref 创建联合值。
    /// Creates a union value from a Vue Ref of float.
    /// </summary>
    /// <param name="value">float 的响应式引用。The reactive ref of float.</param>
    /// <returns>包含该引用的联合值。The union value containing the ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouterViewDepthValue From(Vue3.IVueRef<float> value);

    /// <summary>
    /// 从 double 的 Vue Ref 创建联合值。
    /// Creates a union value from a Vue Ref of double.
    /// </summary>
    /// <param name="value">double 的响应式引用。The reactive ref of double.</param>
    /// <returns>包含该引用的联合值。The union value containing the ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouterViewDepthValue From(Vue3.IVueRef<double> value);

    /// <summary>
    /// 从 decimal 的 Vue Ref 创建联合值。
    /// Creates a union value from a Vue Ref of decimal.
    /// </summary>
    /// <param name="value">decimal 的响应式引用。The reactive ref of decimal.</param>
    /// <returns>包含该引用的联合值。The union value containing the ref.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouterViewDepthValue From(Vue3.IVueRef<decimal> value);
}

/// <summary>
/// 历史状态值联合类型，接受 string、Number、bool、HistoryState 或数组。
/// History state value union accepting string, Number, bool, HistoryState, or array.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union HistoryStateValue(string, Number, bool, HistoryState, Array<HistoryStateValue?>)
{
    /// <summary>
    /// 以字符串返回，如果不是字符串变体则返回 default。
    /// Returns as string, or default if not a string variant.
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 以 Number 返回，如果不是 Number 变体则返回 default。
    /// Returns as Number, or default if not a Number variant.
    /// </summary>
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 以布尔值返回，如果不是布尔变体则返回 default。
    /// Returns as bool, or default if not a bool variant.
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 以 HistoryState 对象返回，如果不是对象变体则返回 default。
    /// Returns as HistoryState, or default if not an object variant.
    /// </summary>
    public HistoryState? AsObject => Value as HistoryState;

    /// <summary>
    /// 以数组返回，如果不是数组变体则返回 default。
    /// Returns as array, or default if not an array variant.
    /// </summary>
    public Array<HistoryStateValue?>? AsArray => Value as Array<HistoryStateValue?>;

    /// <summary>
    /// 从可空字符串数组隐式转换。
    /// Implicitly converts from a nullable string array.
    /// </summary>
    /// <param name="value">要转换的可空字符串数组。The nullable string array to convert.</param>
    public static implicit operator HistoryStateValue(string?[] value)
        => new((Array<HistoryStateValue?>)value.Select(static item => item is null ? null : (HistoryStateValue?)item).ToArray());

    /// <summary>
    /// 从 Number 数组隐式转换。
    /// Implicitly converts from a Number array.
    /// </summary>
    /// <param name="value">要转换的 Number 数组。The Number array to convert.</param>
    public static implicit operator HistoryStateValue(Number[] value)
        => new((Array<HistoryStateValue?>)value.Select(static item => (HistoryStateValue?)item).ToArray());

    /// <summary>
    /// 从可空 Number 数组隐式转换。
    /// Implicitly converts from a nullable Number array.
    /// </summary>
    /// <param name="value">要转换的可空 Number 数组。The nullable Number array to convert.</param>
    public static implicit operator HistoryStateValue(Number?[] value)
        => new((Array<HistoryStateValue?>)value.Select(static item => item is null ? null : (HistoryStateValue?)item.Value).ToArray());

    /// <summary>
    /// 从布尔数组隐式转换。
    /// Implicitly converts from a bool array.
    /// </summary>
    /// <param name="value">要转换的布尔数组。The bool array to convert.</param>
    public static implicit operator HistoryStateValue(bool[] value)
        => new((Array<HistoryStateValue?>)value.Select(static item => (HistoryStateValue?)item).ToArray());

    /// <summary>
    /// 从可空布尔数组隐式转换。
    /// Implicitly converts from a nullable bool array.
    /// </summary>
    /// <param name="value">要转换的可空布尔数组。The nullable bool array to convert.</param>
    public static implicit operator HistoryStateValue(bool?[] value)
        => new((Array<HistoryStateValue?>)value.Select(static item => item is null ? null : (HistoryStateValue?)item.Value).ToArray());

    /// <summary>
    /// 从可空 HistoryState 数组隐式转换。
    /// Implicitly converts from a nullable HistoryState array.
    /// </summary>
    /// <param name="value">要转换的可空 HistoryState 数组。The nullable HistoryState array to convert.</param>
    public static implicit operator HistoryStateValue(HistoryState?[] value)
        => new((Array<HistoryStateValue?>)value.Select(static item => item is null ? null : (HistoryStateValue?)item).ToArray());

    /// <summary>
    /// 从 HistoryStateValue 数组隐式转换。
    /// Implicitly converts from an Array of HistoryStateValue.
    /// </summary>
    /// <param name="value">要转换的 HistoryStateValue 数组。The Array of HistoryStateValue to convert.</param>
    public static implicit operator HistoryStateValue(Array<HistoryStateValue?> value)
        => new(value);

    /// <summary>
    /// 从可空 HistoryStateValue 数组隐式转换。
    /// Implicitly converts from a nullable HistoryStateValue array.
    /// </summary>
    /// <param name="value">要转换的可空 HistoryStateValue 数组。The nullable HistoryStateValue array to convert.</param>
    public static implicit operator HistoryStateValue(HistoryStateValue?[] value)
        => new((Array<HistoryStateValue?>)value);
}

/// <summary>
/// 路由错误值联合类型，接受 Error、string、Number、bool、BigInt、Symbol、IObject 或数组。
/// Router error value union accepting Error, string, Number, bool, BigInt, Symbol, IObject, or array.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouterErrorValue(
    Error,
    string,
    Number,
    bool,
    BigInt,
    Symbol,
    IObject,
    Array<RouterErrorValue?>)
{
    public Error? AsError => Value as Error;

    public string? AsString => Value as string;

    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public BigInt? AsBigInt => Value is BigInt value ? value : default(BigInt?);

    public Symbol? AsSymbol => Value as Symbol;

    public IObject? AsObject => Value as IObject;

    public Array<RouterErrorValue?>? AsArray => Value as Array<RouterErrorValue?>;

    /// <summary>
    /// 从可空 RouterErrorValue 数组隐式转换。
    /// Implicitly converts from a nullable RouterErrorValue array.
    /// </summary>
    /// <param name="value">要转换的可空 RouterErrorValue 数组。The nullable RouterErrorValue array to convert.</param>
    public static implicit operator RouterErrorValue(RouterErrorValue?[] value)
        => new((Array<RouterErrorValue?>)value);

    /// <summary>
    /// 从 IObject 创建联合值。
    /// Creates a union value from an IObject.
    /// </summary>
    /// <param name="value">要转换的对象。The object to convert.</param>
    /// <returns>包含该对象的联合值。The union value containing the object.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouterErrorValue From(IObject value);
}

/// <summary>
/// 原始路由组件联合类型，接受 IVueComponent 或 RouteComponentLoader。
/// Raw route component union accepting IVueComponent or RouteComponentLoader.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RawRouteComponent(IVueComponent, RouteComponentLoader)
{

    /// <summary>
    /// 以 IVueComponent 返回，如果不是组件变体则返回 default。
    /// Returns as IVueComponent, or default if not a component variant.
    /// </summary>
    public IVueComponent? AsComponent => Value as IVueComponent;

    /// <summary>
    /// 以 RouteComponentLoader 返回，如果不是加载器变体则返回 default。
    /// Returns as RouteComponentLoader, or default if not a loader variant.
    /// </summary>
    public RouteComponentLoader? AsLoader => Value as RouteComponentLoader;

    /// <summary>
    /// 从 IVueComponent 创建联合值。
    /// Creates a union value from an IVueComponent.
    /// </summary>
    /// <param name="value">Vue 组件。The Vue component.</param>
    /// <returns>包含该组件的联合值。The union value containing the component.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RawRouteComponent From(IVueComponent value);

    /// <summary>
    /// 从 RouteComponentLoader 创建联合值。
    /// Creates a union value from a RouteComponentLoader.
    /// </summary>
    /// <param name="value">组件加载器。The component loader.</param>
    /// <returns>包含该加载器的联合值。The union value containing the loader.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RawRouteComponent From(RouteComponentLoader value);

    /// <summary>
    /// 从 RouteComponent 隐式转换，优先使用加载器变体。
    /// Implicitly converts from a RouteComponent, preferring the loader variant.
    /// </summary>
    /// <param name="value">要转换的路由组件。The route component to convert.</param>
    public static implicit operator RawRouteComponent(RouteComponent value)
        => value.AsLoader is not null ? new(value.AsLoader) : new(value.AsComponent!);
}

/// <summary>
/// 路由组件联合类型，接受 IVueComponent 或 RouteComponentLoader。
/// Route component union accepting IVueComponent or RouteComponentLoader.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouteComponent(IVueComponent, RouteComponentLoader)
{
    /// <summary>
    /// 以 IVueComponent 返回，如果不是组件变体则返回 default。
    /// Returns as IVueComponent, or default if not a component variant.
    /// </summary>
    public IVueComponent? AsComponent => Value as IVueComponent;

    /// <summary>
    /// 以 RouteComponentLoader 返回，如果不是加载器变体则返回 default。
    /// Returns as RouteComponentLoader, or default if not a loader variant.
    /// </summary>
    public RouteComponentLoader? AsLoader => Value as RouteComponentLoader;

    /// <summary>
    /// 从 IVueComponent 创建联合值。
    /// Creates a union value from an IVueComponent.
    /// </summary>
    /// <param name="value">Vue 组件。The Vue component.</param>
    /// <returns>包含该组件的联合值。The union value containing the component.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteComponent From(IVueComponent value);

    /// <summary>
    /// 从 RouteComponentLoader 创建联合值。
    /// Creates a union value from a RouteComponentLoader.
    /// </summary>
    /// <param name="value">组件加载器。The component loader.</param>
    /// <returns>包含该加载器的联合值。The union value containing the loader.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteComponent From(RouteComponentLoader value);

}

/// <summary>
/// 路由记录 Props 联合类型，接受 bool、VueProps 或 RouteRecordPropsResolver。
/// Route record props union accepting bool, VueProps, or RouteRecordPropsResolver.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouteRecordProps(bool, Vue3.VueProps, RouteRecordPropsResolver)
{
    /// <summary>
    /// 以布尔值返回，如果不是布尔变体则返回 default。
    /// Returns as bool, or default if not a bool variant.
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 以 VueProps 返回，如果不是 Props 变体则返回 default。
    /// Returns as VueProps, or default if not a Props variant.
    /// </summary>
    public Vue3.VueProps? AsProps => Value as Vue3.VueProps;

    /// <summary>
    /// 以 RouteRecordPropsResolver 返回，如果不是解析器变体则返回 default。
    /// Returns as RouteRecordPropsResolver, or default if not a resolver variant.
    /// </summary>
    public RouteRecordPropsResolver? AsResolver => Value as RouteRecordPropsResolver;

    /// <summary>
    /// 从布尔值创建联合值。
    /// Creates a union value from a bool.
    /// </summary>
    /// <param name="value">布尔值。The bool value.</param>
    /// <returns>包含该布尔值的联合值。The union value containing the bool.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRecordProps From(bool value);

    /// <summary>
    /// 从 VueProps 创建联合值。
    /// Creates a union value from VueProps.
    /// </summary>
    /// <param name="value">Props 对象。The props object.</param>
    /// <returns>包含该 Props 的联合值。The union value containing the props.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRecordProps From(Vue3.VueProps value);

    /// <summary>
    /// 从 RouteRecordPropsResolver 创建联合值。
    /// Creates a union value from a RouteRecordPropsResolver.
    /// </summary>
    /// <param name="value">Props 解析函数。The props resolver.</param>
    /// <returns>包含该解析器的联合值。The union value containing the resolver.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRecordProps From(RouteRecordPropsResolver value);
}

/// <summary>
/// 路由记录命名视图 Props 联合类型，接受 bool 或 RouteNamedProps。
/// Route record named view props union accepting bool or RouteNamedProps.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouteRecordNamedViewProps(bool, RouteNamedProps)
{
    /// <summary>
    /// 以布尔值返回，如果不是布尔变体则返回 default。
    /// Returns as bool, or default if not a bool variant.
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 以 RouteNamedProps 返回，如果不是命名 Props 变体则返回 default。
    /// Returns as RouteNamedProps, or default if not a named props variant.
    /// </summary>
    public RouteNamedProps? AsNamedProps => Value as RouteNamedProps;

    /// <summary>
    /// 从布尔值创建联合值。
    /// Creates a union value from a bool.
    /// </summary>
    /// <param name="value">布尔值。The bool value.</param>
    /// <returns>包含该布尔值的联合值。The union value containing the bool.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRecordNamedViewProps From(bool value);

    /// <summary>
    /// 从 RouteNamedProps 创建联合值。
    /// Creates a union value from RouteNamedProps.
    /// </summary>
    /// <param name="value">命名视图 Props 映射。The named view props mapping.</param>
    /// <returns>包含该命名 Props 的联合值。The union value containing the named props.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRecordNamedViewProps From(RouteNamedProps value);
}

/// <summary>
/// 导航守卫 next 参数联合类型，接受 bool、RouteLocationRaw、NavigationGuardNextCallback 或 Error。
/// Navigation guard next argument union accepting bool, RouteLocationRaw, NavigationGuardNextCallback, or Error.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union NavigationGuardNextArgument(bool, RouteLocationRaw, NavigationGuardNextCallback, Error)
{
    /// <summary>
    /// 以布尔值返回，如果不是布尔变体则返回 default。
    /// Returns as bool, or default if not a bool variant.
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 以 RouteLocationRaw 返回，如果不是位置变体则返回 default。
    /// Returns as RouteLocationRaw, or default if not a location variant.
    /// </summary>
    public RouteLocationRaw? AsLocation => Value is RouteLocationRaw value ? value : default(RouteLocationRaw?);

    /// <summary>
    /// 以 NavigationGuardNextCallback 返回，如果不是回调变体则返回 default。
    /// Returns as NavigationGuardNextCallback, or default if not a callback variant.
    /// </summary>
    public NavigationGuardNextCallback? AsCallback => Value as NavigationGuardNextCallback;

    /// <summary>
    /// 以 Error 返回，如果不是错误变体则返回 default。
    /// Returns as Error, or default if not an error variant.
    /// </summary>
    public Error? AsError => Value as Error;

    /// <summary>
    /// 从字符串隐式转换，包装为路由位置。
    /// Implicitly converts from a string, wrapping as a route location.
    /// </summary>
    /// <param name="value">要转换的路径字符串。The path string to convert.</param>
    public static implicit operator NavigationGuardNextArgument(string value)
        => new(value);

    /// <summary>
    /// 从路径式路由位置隐式转换。
    /// Implicitly converts from a path-based route location.
    /// </summary>
    /// <param name="value">要转换的路径式路由位置。The path-based route location to convert.</param>
    public static implicit operator NavigationGuardNextArgument(RouteLocationAsPath value)
        => new(value);

    /// <summary>
    /// 从相对式路由位置隐式转换。
    /// Implicitly converts from a relative route location.
    /// </summary>
    /// <param name="value">要转换的相对式路由位置。The relative route location to convert.</param>
    public static implicit operator NavigationGuardNextArgument(RouteLocationAsRelative value)
        => new(value);

    /// <summary>
    /// 从 NavigationGuardNextCallback 隐式转换。
    /// Implicitly converts from a NavigationGuardNextCallback.
    /// </summary>
    /// <param name="value">要转换的回调。The callback to convert.</param>
    /// <summary>
    /// 从 NavigationGuardNextCallback 创建联合值。
    /// Creates a union value from a NavigationGuardNextCallback.
    /// </summary>
    /// <param name="value">导航守卫回调。The navigation guard callback.</param>
    /// <returns>包含该回调的联合值。The union value containing the callback.</returns>
    [ECMAScriptInline("__arg1")]
    [Obsolete("next(vm => ...) is only meaningful for beforeRouteEnter-style component guards. This VueRoute surface does not expose that guard as a recommended authoring path.")]
    public extern static NavigationGuardNextArgument From(NavigationGuardNextCallback value);
}

/// <summary>
/// 导航守卫返回值联合类型，接受 bool、RouteLocationRaw 或 Error。
/// Navigation guard return value union accepting bool, RouteLocationRaw, or Error.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union NavigationGuardReturn(bool, RouteLocationRaw, Error)
{
    /// <summary>
    /// 以布尔值返回，如果不是布尔变体则返回 default。
    /// Returns as bool, or default if not a bool variant.
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 以 RouteLocationRaw 返回，如果不是位置变体则返回 default。
    /// Returns as RouteLocationRaw, or default if not a location variant.
    /// </summary>
    public RouteLocationRaw? AsLocation => Value is RouteLocationRaw value ? value : default(RouteLocationRaw?);

    /// <summary>
    /// 以 Error 返回，如果不是错误变体则返回 default。
    /// Returns as Error, or default if not an error variant.
    /// </summary>
    public Error? AsError => Value as Error;

    /// <summary>
    /// 从字符串隐式转换，包装为路由位置。
    /// Implicitly converts from a string, wrapping as a route location.
    /// </summary>
    /// <param name="value">要转换的路径字符串。The path string to convert.</param>
    public static implicit operator NavigationGuardReturn(string value)
        => new(value);

    /// <summary>
    /// 从路径式路由位置隐式转换。
    /// Implicitly converts from a path-based route location.
    /// </summary>
    /// <param name="value">要转换的路径式路由位置。The path-based route location to convert.</param>
    public static implicit operator NavigationGuardReturn(RouteLocationAsPath value)
        => new(value);

    /// <summary>
    /// 从相对式路由位置隐式转换。
    /// Implicitly converts from a relative route location.
    /// </summary>
    /// <param name="value">要转换的相对式路由位置。The relative route location to convert.</param>
    public static implicit operator NavigationGuardReturn(RouteLocationAsRelative value)
        => new(value);

}

/// <summary>
/// 路由导航结果联合类型，接受 NavigationFailure。
/// Route navigation result union accepting NavigationFailure.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouteNavigationResult(NavigationFailure)
{
    /// <summary>
    /// 以 NavigationFailure 返回，如果不是失败变体则返回 default。
    /// Returns as NavigationFailure, or default if not a failure variant.
    /// </summary>
    public NavigationFailure? AsFailure => Value as NavigationFailure;
}

/// <summary>
/// 导航守卫处理器联合类型，接受同步守卫、异步守卫、遗留同步守卫或遗留异步守卫。
/// Navigation guard handler union accepting sync guard, async guard, legacy sync guard, or legacy async guard.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union NavigationGuardHandler(
    RouteNavigationGuard,
    AsyncRouteNavigationGuard,
    LegacyRouteNavigationGuard,
    LegacyAsyncRouteNavigationGuard)
{
    /// <summary>
    /// 以同步守卫返回，如果不是同步变体则返回 default。
    /// Returns as synchronous guard, or default if not a sync variant.
    /// </summary>
    public RouteNavigationGuard? AsSync => Value as RouteNavigationGuard;

    /// <summary>
    /// 以异步守卫返回，如果不是异步变体则返回 default。
    /// Returns as asynchronous guard, or default if not an async variant.
    /// </summary>
    public AsyncRouteNavigationGuard? AsAsync => Value as AsyncRouteNavigationGuard;

    /// <summary>
    /// 以遗留同步守卫返回，如果不是遗留同步变体则返回 default。
    /// Returns as legacy synchronous guard, or default if not a legacy sync variant.
    /// </summary>
    public LegacyRouteNavigationGuard? AsLegacySync => Value as LegacyRouteNavigationGuard;

    /// <summary>
    /// 以遗留异步守卫返回，如果不是遗留异步变体则返回 default。
    /// Returns as legacy asynchronous guard, or default if not a legacy async variant.
    /// </summary>
    public LegacyAsyncRouteNavigationGuard? AsLegacyAsync => Value as LegacyAsyncRouteNavigationGuard;

    /// <summary>
    /// 从同步导航守卫创建联合值。
    /// Creates a union value from a synchronous navigation guard.
    /// </summary>
    /// <param name="value">同步导航守卫。The synchronous navigation guard.</param>
    /// <returns>包含该守卫的联合值。The union value containing the guard.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static NavigationGuardHandler From(RouteNavigationGuard value);

    /// <summary>
    /// 从异步导航守卫创建联合值。
    /// Creates a union value from an asynchronous navigation guard.
    /// </summary>
    /// <param name="value">异步导航守卫。The asynchronous navigation guard.</param>
    /// <returns>包含该守卫的联合值。The union value containing the guard.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static NavigationGuardHandler From(AsyncRouteNavigationGuard value);

    /// <summary>
    /// 从遗留同步导航守卫创建联合值。
    /// Creates a union value from a legacy synchronous navigation guard.
    /// </summary>
    /// <param name="value">遗留同步导航守卫。The legacy synchronous navigation guard.</param>
    /// <returns>包含该守卫的联合值。The union value containing the guard.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static NavigationGuardHandler From(LegacyRouteNavigationGuard value);

    /// <summary>
    /// 从遗留异步导航守卫创建联合值。
    /// Creates a union value from a legacy asynchronous navigation guard.
    /// </summary>
    /// <param name="value">遗留异步导航守卫。The legacy asynchronous navigation guard.</param>
    /// <returns>包含该守卫的联合值。The union value containing the guard.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static NavigationGuardHandler From(LegacyAsyncRouteNavigationGuard value);
}

/// <summary>
/// 路由记录 beforeEnter 守卫联合类型，接受单个守卫或守卫数组。
/// Route record beforeEnter guard union accepting a single guard or an array of guards.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouteRecordBeforeEnter(NavigationGuardHandler, NavigationGuardHandler[])
{
    /// <summary>
    /// 以单个守卫返回，如果不是单守卫变体则返回 default。
    /// Returns as a single guard, or default if not a single guard variant.
    /// </summary>
    public NavigationGuardHandler? AsGuard => Value is NavigationGuardHandler value ? value : default(NavigationGuardHandler?);

    /// <summary>
    /// 以守卫数组返回，如果不是数组变体则返回 default。
    /// Returns as an array of guards, or default if not an array variant.
    /// </summary>
    public NavigationGuardHandler[]? AsGuards => Value as NavigationGuardHandler[];

    /// <summary>
    /// 从 NavigationGuardHandler 创建联合值。
    /// Creates a union value from a NavigationGuardHandler.
    /// </summary>
    /// <param name="value">导航守卫处理器。The navigation guard handler.</param>
    /// <returns>包含该守卫的联合值。The union value containing the guard.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRecordBeforeEnter From(NavigationGuardHandler value);

    /// <summary>
    /// 从 NavigationGuardHandler 数组创建联合值。
    /// Creates a union value from an array of NavigationGuardHandler.
    /// </summary>
    /// <param name="value">导航守卫处理器数组。The array of navigation guard handlers.</param>
    /// <returns>包含该数组的联合值。The union value containing the array.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRecordBeforeEnter From(NavigationGuardHandler[] value);

    /// <summary>
    /// 从同步导航守卫创建联合值。
    /// Creates a union value from a synchronous navigation guard.
    /// </summary>
    /// <param name="value">同步导航守卫。The synchronous navigation guard.</param>
    /// <returns>包含该守卫的联合值。The union value containing the guard.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRecordBeforeEnter From(RouteNavigationGuard value);

    /// <summary>
    /// 从同步导航守卫数组创建联合值。
    /// Creates a union value from an array of synchronous navigation guards.
    /// </summary>
    /// <param name="value">同步导航守卫数组。The array of synchronous navigation guards.</param>
    /// <returns>包含该数组的联合值。The union value containing the array.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRecordBeforeEnter From(RouteNavigationGuard[] value);

    /// <summary>
    /// 从异步导航守卫创建联合值。
    /// Creates a union value from an asynchronous navigation guard.
    /// </summary>
    /// <param name="value">异步导航守卫。The asynchronous navigation guard.</param>
    /// <returns>包含该守卫的联合值。The union value containing the guard.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRecordBeforeEnter From(AsyncRouteNavigationGuard value);

    /// <summary>
    /// 从异步导航守卫数组创建联合值。
    /// Creates a union value from an array of asynchronous navigation guards.
    /// </summary>
    /// <param name="value">异步导航守卫数组。The array of asynchronous navigation guards.</param>
    /// <returns>包含该数组的联合值。The union value containing the array.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRecordBeforeEnter From(AsyncRouteNavigationGuard[] value);

    /// <summary>
    /// 从遗留同步导航守卫创建联合值。
    /// Creates a union value from a legacy synchronous navigation guard.
    /// </summary>
    /// <param name="value">遗留同步导航守卫。The legacy synchronous navigation guard.</param>
    /// <returns>包含该守卫的联合值。The union value containing the guard.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRecordBeforeEnter From(LegacyRouteNavigationGuard value);

    /// <summary>
    /// 从遗留同步导航守卫数组创建联合值。
    /// Creates a union value from an array of legacy synchronous navigation guards.
    /// </summary>
    /// <param name="value">遗留同步导航守卫数组。The array of legacy synchronous navigation guards.</param>
    /// <returns>包含该数组的联合值。The union value containing the array.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRecordBeforeEnter From(LegacyRouteNavigationGuard[] value);

    /// <summary>
    /// 从遗留异步导航守卫创建联合值。
    /// Creates a union value from a legacy asynchronous navigation guard.
    /// </summary>
    /// <param name="value">遗留异步导航守卫。The legacy asynchronous navigation guard.</param>
    /// <returns>包含该守卫的联合值。The union value containing the guard.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRecordBeforeEnter From(LegacyAsyncRouteNavigationGuard value);

    /// <summary>
    /// 从遗留异步导航守卫数组创建联合值。
    /// Creates a union value from an array of legacy asynchronous navigation guards.
    /// </summary>
    /// <param name="value">遗留异步导航守卫数组。The array of legacy asynchronous navigation guards.</param>
    /// <returns>包含该数组的联合值。The union value containing the array.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRecordBeforeEnter From(LegacyAsyncRouteNavigationGuard[] value);
}

/// <summary>
/// 路由重定向选项联合类型，接受 RouteLocationRaw 或 RouteRedirectCallback。
/// Route redirect option union accepting RouteLocationRaw or RouteRedirectCallback.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouteRedirectOption(RouteLocationRaw, RouteRedirectCallback)
{
    /// <summary>
    /// 以 RouteLocationRaw 返回，如果不是位置变体则返回 default。
    /// Returns as RouteLocationRaw, or default if not a location variant.
    /// </summary>
    public RouteLocationRaw? AsLocation => Value is RouteLocationRaw value ? value : default(RouteLocationRaw?);

    /// <summary>
    /// 以 RouteRedirectCallback 返回，如果不是回调变体则返回 default。
    /// Returns as RouteRedirectCallback, or default if not a callback variant.
    /// </summary>
    public RouteRedirectCallback? AsCallback => Value as RouteRedirectCallback;

    /// <summary>
    /// 从字符串隐式转换，包装为路由位置。
    /// Implicitly converts from a string, wrapping as a route location.
    /// </summary>
    /// <param name="value">要转换的路径字符串。The path string to convert.</param>
    public static implicit operator RouteRedirectOption(string value)
        => new(value);

    /// <summary>
    /// 从路径式路由位置隐式转换。
    /// Implicitly converts from a path-based route location.
    /// </summary>
    /// <param name="value">要转换的路径式路由位置。The path-based route location to convert.</param>
    public static implicit operator RouteRedirectOption(RouteLocationAsPath value)
        => new(value);

    /// <summary>
    /// 从相对式路由位置隐式转换。
    /// Implicitly converts from a relative route location.
    /// </summary>
    /// <param name="value">要转换的相对式路由位置。The relative route location to convert.</param>
    public static implicit operator RouteRedirectOption(RouteLocationAsRelative value)
        => new(value);

    /// <summary>
    /// 从 RouteLocationPathRaw 隐式转换，先转换为 RouteLocationRaw。
    /// Implicitly converts from RouteLocationPathRaw, first converting to RouteLocationRaw.
    /// </summary>
    /// <param name="value">要转换的路径式原始路由位置。The path-based raw route location to convert.</param>
    public static implicit operator RouteRedirectOption(RouteLocationPathRaw value)
        => new((RouteLocationRaw)value);

    /// <summary>
    /// 从 RouteLocationNamedRaw 隐式转换，先转换为 RouteLocationRaw。
    /// Implicitly converts from RouteLocationNamedRaw, first converting to RouteLocationRaw.
    /// </summary>
    /// <param name="value">要转换的命名式原始路由位置。The named raw route location to convert.</param>
    public static implicit operator RouteRedirectOption(RouteLocationNamedRaw value)
        => new((RouteLocationRaw)value);

    /// <summary>
    /// 从 RouteRecordRedirectOption 隐式转换，提取位置或回调。
    /// Implicitly converts from RouteRecordRedirectOption, extracting location or callback.
    /// </summary>
    /// <param name="value">要转换的路由记录重定向选项。The route record redirect option to convert.</param>
    public static implicit operator RouteRedirectOption(RouteRecordRedirectOption value)
    {
        if (value.AsCallback is RouteRedirectCallback callback)
            return new(callback);

        var location = value.AsLocation;
        if (location.HasValue)
            return new(location.GetValueOrDefault());

        throw new InvalidOperationException("RouteRecordRedirectOption must contain either a location or a callback.");
    }

    /// <summary>
    /// 从 RouteLocationRaw 创建联合值。
    /// Creates a union value from a RouteLocationRaw.
    /// </summary>
    /// <param name="value">重定向目标位置。The redirect target location.</param>
    /// <returns>包含该位置的联合值。The union value containing the location.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRedirectOption From(RouteLocationRaw value);

    /// <summary>
    /// 从 RouteRedirectCallback 创建联合值。
    /// Creates a union value from a RouteRedirectCallback.
    /// </summary>
    /// <param name="value">重定向回调函数。The redirect callback function.</param>
    /// <returns>包含该回调的联合值。The union value containing the callback.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRedirectOption From(RouteRedirectCallback value);

    /// <summary>
    /// 从 RouteRecordRedirectOption 创建联合值。
    /// Creates a union value from a RouteRecordRedirectOption.
    /// </summary>
    /// <param name="value">路由记录重定向选项。The route record redirect option.</param>
    /// <returns>包含提取值的联合值。The union value containing the extracted value.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRedirectOption From(RouteRecordRedirectOption value);
}

/// <summary>
/// 路由记录重定向选项联合类型，接受 RouteLocationRaw 或 RouteRedirectCallback。
/// Route record redirect option union accepting RouteLocationRaw or RouteRedirectCallback.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouteRecordRedirectOption(RouteLocationRaw, RouteRedirectCallback)
{
    /// <summary>
    /// 以 RouteLocationRaw 返回，如果不是位置变体则返回 default。
    /// Returns as RouteLocationRaw, or default if not a location variant.
    /// </summary>
    public RouteLocationRaw? AsLocation => Value is RouteLocationRaw value ? value : default(RouteLocationRaw?);

    /// <summary>
    /// 以 RouteRedirectCallback 返回，如果不是回调变体则返回 default。
    /// Returns as RouteRedirectCallback, or default if not a callback variant.
    /// </summary>
    public RouteRedirectCallback? AsCallback => Value as RouteRedirectCallback;

    /// <summary>
    /// 从字符串隐式转换，包装为路由位置。
    /// Implicitly converts from a string, wrapping as a route location.
    /// </summary>
    /// <param name="value">要转换的路径字符串。The path string to convert.</param>
    public static implicit operator RouteRecordRedirectOption(string value)
        => new(value);

    /// <summary>
    /// 从路径式路由位置隐式转换。
    /// Implicitly converts from a path-based route location.
    /// </summary>
    /// <param name="value">要转换的路径式路由位置。The path-based route location to convert.</param>
    public static implicit operator RouteRecordRedirectOption(RouteLocationAsPath value)
        => new(value);

    /// <summary>
    /// 从相对式路由位置隐式转换。
    /// Implicitly converts from a relative route location.
    /// </summary>
    /// <param name="value">要转换的相对式路由位置。The relative route location to convert.</param>
    public static implicit operator RouteRecordRedirectOption(RouteLocationAsRelative value)
        => new(value);

    /// <summary>
    /// 从 RouteLocationPathRaw 隐式转换。
    /// Implicitly converts from a RouteLocationPathRaw.
    /// </summary>
    /// <param name="value">要转换的路径式原始路由位置。The path-based raw route location to convert.</param>
    public static implicit operator RouteRecordRedirectOption(RouteLocationPathRaw value)
        => new(value);

    /// <summary>
    /// 从 RouteLocationNamedRaw 隐式转换。
    /// Implicitly converts from a RouteLocationNamedRaw.
    /// </summary>
    /// <param name="value">要转换的命名式原始路由位置。The named raw route location to convert.</param>
    public static implicit operator RouteRecordRedirectOption(RouteLocationNamedRaw value)
        => new(value);

    /// <summary>
    /// 从 RouteLocationRaw 创建联合值。
    /// Creates a union value from a RouteLocationRaw.
    /// </summary>
    /// <param name="value">重定向目标位置。The redirect target location.</param>
    /// <returns>包含该位置的联合值。The union value containing the location.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRecordRedirectOption From(RouteLocationRaw value);

    /// <summary>
    /// 从 RouteRedirectCallback 创建联合值。
    /// Creates a union value from a RouteRedirectCallback.
    /// </summary>
    /// <param name="value">重定向回调函数。The redirect callback function.</param>
    /// <returns>包含该回调的联合值。The union value containing the callback.</returns>
    [ECMAScriptInline("__arg1")]
    public extern static RouteRecordRedirectOption From(RouteRedirectCallback value);
}

/// <summary>
/// 路由记录原始值联合类型，接受单视图、单视图带子路由、多视图、多视图带子路由或重定向记录。
/// Route record raw union accepting single view, single view with children, multiple views, multiple views with children, or redirect record.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouteRecordRaw(
    RouteRecordSingleView,
    RouteRecordSingleViewWithChildren,
    RouteRecordMultipleViews,
    RouteRecordMultipleViewsWithChildren,
    RouteRecordRedirect)
{
    /// <summary>
    /// 以单视图路由记录返回，如果不是单视图变体则返回 default。
    /// Returns as single view route record, or default if not a single view variant.
    /// </summary>
    public RouteRecordSingleView? AsSingleView => Value as RouteRecordSingleView;

    /// <summary>
    /// 以带子路由的单视图路由记录返回，如果不是该变体则返回 default。
    /// Returns as single view route record with children, or default if not that variant.
    /// </summary>
    public RouteRecordSingleViewWithChildren? AsSingleViewWithChildren => Value as RouteRecordSingleViewWithChildren;

    /// <summary>
    /// 以多视图路由记录返回，如果不是多视图变体则返回 default。
    /// Returns as multiple views route record, or default if not a multiple views variant.
    /// </summary>
    public RouteRecordMultipleViews? AsMultipleViews => Value as RouteRecordMultipleViews;

    /// <summary>
    /// 以带子路由的多视图路由记录返回，如果不是该变体则返回 default。
    /// Returns as multiple views route record with children, or default if not that variant.
    /// </summary>
    public RouteRecordMultipleViewsWithChildren? AsMultipleViewsWithChildren => Value as RouteRecordMultipleViewsWithChildren;

    /// <summary>
    /// 以重定向路由记录返回，如果不是重定向变体则返回 default。
    /// Returns as redirect route record, or default if not a redirect variant.
    /// </summary>
    public RouteRecordRedirect? AsRedirect => Value as RouteRecordRedirect;
}

/// <summary>
/// 匹配器位置原始值联合类型，接受路径式、命名式或相对式匹配器位置。
/// Matcher location raw union accepting path-based, named, or relative matcher location.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union MatcherLocationRaw(MatcherLocationAsPath, MatcherLocationAsName, MatcherLocationAsRelative)
{
    /// <summary>
    /// 以路径式匹配器位置返回，如果不是路径变体则返回 default。
    /// Returns as path-based matcher location, or default if not a path variant.
    /// </summary>
    public MatcherLocationAsPath? AsPath => Value as MatcherLocationAsPath;

    /// <summary>
    /// 以命名式匹配器位置返回，如果不是命名变体则返回 default。
    /// Returns as named matcher location, or default if not a named variant.
    /// </summary>
    public MatcherLocationAsName? AsNamed => Value as MatcherLocationAsName;

    /// <summary>
    /// 以相对式匹配器位置返回，如果不是相对变体则返回 default。
    /// Returns as relative matcher location, or default if not a relative variant.
    /// </summary>
    public MatcherLocationAsRelative? AsRelative => Value as MatcherLocationAsRelative;
}

/// <summary>
/// 路由参数联合类型，接受 string 或 string 数组。
/// Route parameter union accepting string or string array.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouteParam(string, string[])
{
    /// <summary>
    /// 以字符串返回，如果不是字符串变体则返回 default。
    /// Returns as string, or default if not a string variant.
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 以字符串数组返回，如果不是数组变体则返回 default。
    /// Returns as string array, or default if not an array variant.
    /// </summary>
    public string[]? AsStrings => Value as string[];
}

/// <summary>
/// 路由参数原始值联合类型，接受 string、RouteParamRaw 数组或 Number。
/// Route parameter raw union accepting string, RouteParamRaw array, or Number.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouteParamRaw(string, Array<RouteParamRaw>, Number)
{
    /// <summary>
    /// 以字符串返回，如果不是字符串变体则返回 default。
    /// Returns as string, or default if not a string variant.
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 以数组返回，如果不是数组变体则返回 default。
    /// Returns as array, or default if not an array variant.
    /// </summary>
    public Array<RouteParamRaw>? AsArray => Value as Array<RouteParamRaw>;

    /// <summary>
    /// 以 Number 返回，如果不是 Number 变体则返回 default。
    /// Returns as Number, or default if not a Number variant.
    /// </summary>
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 从字符串数组隐式转换，逐项映射为 RouteParamRaw。
    /// Implicitly converts from a string array, mapping each item to RouteParamRaw.
    /// </summary>
    /// <param name="value">要转换的字符串数组。The string array to convert.</param>
    public static implicit operator RouteParamRaw(string[] value)
        => new((Array<RouteParamRaw>)value.Select(static item => (RouteParamRaw)item).ToArray());

    /// <summary>
    /// 从 RouteParamRaw CLR 数组隐式转换。
    /// Implicitly converts from a CLR array of RouteParamRaw.
    /// </summary>
    /// <param name="value">要转换的 RouteParamRaw 数组。The RouteParamRaw array to convert.</param>
    public static implicit operator RouteParamRaw(RouteParamRaw[] value)
        => new((Array<RouteParamRaw>)value);

    /// <summary>
    /// 从 Number 数组隐式转换，逐项映射为 RouteParamRaw。
    /// Implicitly converts from a Number array, mapping each item to RouteParamRaw.
    /// </summary>
    /// <param name="value">要转换的 Number 数组。The Number array to convert.</param>
    public static implicit operator RouteParamRaw(Number[] value)
        => new((Array<RouteParamRaw>)value.Select(static item => (RouteParamRaw)item).ToArray());
}

/// <summary>
/// 位置查询值联合类型，接受 string 或字符串数组。
/// Location query value union accepting string or string array.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union LocationQueryValue(string, Array<string?>)
{
    /// <summary>
    /// 以字符串返回，如果不是字符串变体则返回 default。
    /// Returns as string, or default if not a string variant.
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 以可空字符串数组返回，如果不是数组变体则返回 default。
    /// Returns as nullable string array, or default if not an array variant.
    /// </summary>
    public Array<string?>? AsArray => Value as Array<string?>;

    /// <summary>
    /// 从可空字符串 CLR 数组隐式转换。
    /// Implicitly converts from a CLR array of nullable strings.
    /// </summary>
    /// <param name="value">要转换的可空字符串数组。The nullable string array to convert.</param>
    public static implicit operator LocationQueryValue(string?[] value)
        => new((Array<string?>)value);

}

/// <summary>
/// 位置查询原始值联合类型，接受 string、LocationQueryValueRaw 数组或 Number。
/// Location query raw value union accepting string, LocationQueryValueRaw array, or Number.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union LocationQueryValueRaw(string, Array<LocationQueryValueRaw?>, Number)
{
    /// <summary>
    /// 以字符串返回，如果不是字符串变体则返回 default。
    /// Returns as string, or default if not a string variant.
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 以数组返回，如果不是数组变体则返回 default。
    /// Returns as array, or default if not an array variant.
    /// </summary>
    public Array<LocationQueryValueRaw?>? AsArray => Value as Array<LocationQueryValueRaw?>;

    /// <summary>
    /// 以 Number 返回，如果不是 Number 变体则返回 default。
    /// Returns as Number, or default if not a Number variant.
    /// </summary>
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 从可空字符串数组隐式转换，逐项映射为 LocationQueryValueRaw。
    /// Implicitly converts from a nullable string array, mapping each item to LocationQueryValueRaw.
    /// </summary>
    /// <param name="value">要转换的可空字符串数组。The nullable string array to convert.</param>
    public static implicit operator LocationQueryValueRaw(string?[] value)
        => new((Array<LocationQueryValueRaw?>)value.Select(static item => item is null ? null : (LocationQueryValueRaw?)item).ToArray());

    /// <summary>
    /// 从可空 LocationQueryValueRaw CLR 数组隐式转换。
    /// Implicitly converts from a CLR array of nullable LocationQueryValueRaw.
    /// </summary>
    /// <param name="value">要转换的可空 LocationQueryValueRaw 数组。The nullable LocationQueryValueRaw array to convert.</param>
    public static implicit operator LocationQueryValueRaw(LocationQueryValueRaw?[] value)
        => new((Array<LocationQueryValueRaw?>)value);

    /// <summary>
    /// 从 Number 数组隐式转换，逐项映射为 LocationQueryValueRaw。
    /// Implicitly converts from a Number array, mapping each item to LocationQueryValueRaw.
    /// </summary>
    /// <param name="value">要转换的 Number 数组。The Number array to convert.</param>
    public static implicit operator LocationQueryValueRaw(Number[] value)
        => new((Array<LocationQueryValueRaw?>)value.Select(static item => (LocationQueryValueRaw?)item).ToArray());
}
