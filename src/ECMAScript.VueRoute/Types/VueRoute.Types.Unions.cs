using System;
using System.ComponentModel;
using System.Linq;
using ECMAScript.Contract;
using static ECMAScript.Vue3;

namespace ECMAScript;

/// <summary>
/// 路由记录名称联合类型，接受 string 或 Symbol。
/// Route record name union accepting string or Symbol.
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteRecordName
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly Symbol? _symbol;

    /// <summary>
    /// 从字符串值初始化。
    /// Initializes from a string value.
    /// </summary>
    /// <param name="value">路由记录名称。The route record name.</param>
    private RouteRecordName(string value)
    {
        _kind = 1;
        _string = value;
        _symbol = default;
    }

    /// <summary>
    /// 从 Symbol 值初始化。
    /// Initializes from a Symbol value.
    /// </summary>
    /// <param name="value">路由记录名称的 Symbol 值。The Symbol value for the route record name.</param>
    private RouteRecordName(Symbol value)
    {
        _kind = 2;
        _string = default;
        _symbol = value;
    }

    /// <summary>
    /// 以字符串形式返回，如果不是字符串变体则返回 default。
    /// Returns as string, or default if not a string variant.
    /// </summary>
    public string? AsString => _kind == 1 ? _string : default;

    /// <summary>
    /// 以 Symbol 形式返回，如果不是 Symbol 变体则返回 default。
    /// Returns as Symbol, or default if not a Symbol variant.
    /// </summary>
    public Symbol? AsSymbol => _kind == 2 ? _symbol : default;

    /// <summary>
    /// 从字符串隐式转换。
    /// Implicitly converts from a string.
    /// </summary>
    /// <param name="value">要转换的字符串值。The string value to convert.</param>
    public static implicit operator RouteRecordName(string value)
        => new(value);

    /// <summary>
    /// 从 Symbol 隐式转换。
    /// Implicitly converts from a Symbol.
    /// </summary>
    /// <param name="value">要转换的 Symbol 值。The Symbol value to convert.</param>
    public static implicit operator RouteRecordName(Symbol value)
        => new(value);
}

/// <summary>
/// 路由记录别名联合类型，接受 string 或 string 数组。
/// Route record alias union accepting string or string array.
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteRecordAlias
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly string[]? _strings;

    /// <summary>
    /// 从字符串值初始化。
    /// Initializes from a string value.
    /// </summary>
    /// <param name="value">路由别名。The route alias.</param>
    private RouteRecordAlias(string value)
    {
        _kind = 1;
        _string = value;
        _strings = default;
    }

    /// <summary>
    /// 从字符串数组初始化。
    /// Initializes from a string array.
    /// </summary>
    /// <param name="value">路由别名数组。The route alias array.</param>
    private RouteRecordAlias(string[] value)
    {
        _kind = 2;
        _string = default;
        _strings = value;
    }

    /// <summary>
    /// 以字符串形式返回，如果不是字符串变体则返回 default。
    /// Returns as string, or default if not a string variant.
    /// </summary>
    public string? AsString => _kind == 1 ? _string : default;

    /// <summary>
    /// 以字符串数组形式返回，如果不是数组变体则返回 default。
    /// Returns as string array, or default if not an array variant.
    /// </summary>
    public string[]? AsStrings => _kind == 2 ? _strings : default;

    /// <summary>
    /// 从字符串隐式转换。
    /// Implicitly converts from a string.
    /// </summary>
    /// <param name="value">要转换的字符串值。The string value to convert.</param>
    public static implicit operator RouteRecordAlias(string value)
        => new(value);

    /// <summary>
    /// 从字符串数组隐式转换。
    /// Implicitly converts from a string array.
    /// </summary>
    /// <param name="value">要转换的字符串数组。The string array to convert.</param>
    public static implicit operator RouteRecordAlias(string[] value)
        => new(value);
}

/// <summary>
/// 路由位置原始值联合类型，接受 string、RouteLocationAsPath 或 RouteLocationAsRelative。
/// Route location raw union accepting string, RouteLocationAsPath, or RouteLocationAsRelative.
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteLocationRaw
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly RouteLocationAsPath? _path;
    private readonly RouteLocationAsRelative? _relative;

    /// <summary>
    /// 从字符串值初始化。
    /// Initializes from a string value.
    /// </summary>
    /// <param name="value">路由路径字符串。The route path string.</param>
    private RouteLocationRaw(string value)
    {
        _kind = 1;
        _string = value;
        _path = default;
        _relative = default;
    }

    /// <summary>
    /// 从路径式路由位置初始化。
    /// Initializes from a path-based route location.
    /// </summary>
    /// <param name="value">路径式路由位置。The path-based route location.</param>
    private RouteLocationRaw(RouteLocationAsPath value)
    {
        _kind = 2;
        _string = default;
        _path = value;
        _relative = default;
    }

    /// <summary>
    /// 从相对式路由位置初始化。
    /// Initializes from a relative route location.
    /// </summary>
    /// <param name="value">相对式路由位置。The relative route location.</param>
    private RouteLocationRaw(RouteLocationAsRelative value)
    {
        _kind = 3;
        _string = default;
        _path = default;
        _relative = value;
    }

    /// <summary>
    /// 以字符串形式返回，如果不是字符串变体则返回 default。
    /// Returns as string, or default if not a string variant.
    /// </summary>
    public string? AsString => _kind == 1 ? _string : default;

    /// <summary>
    /// 以路径式路由位置返回，如果不是路径变体则返回 default。
    /// Returns as path-based route location, or default if not a path variant.
    /// </summary>
    public RouteLocationAsPath? AsPath => _kind == 2 ? _path : default;

    /// <summary>
    /// 以相对式路由位置返回，如果不是相对变体则返回 default。
    /// Returns as relative route location, or default if not a relative variant.
    /// </summary>
    public RouteLocationAsRelative? AsRelative => _kind == 3 ? _relative : default;

    /// <summary>
    /// 从字符串隐式转换。
    /// Implicitly converts from a string.
    /// </summary>
    /// <param name="value">要转换的路径字符串。The path string to convert.</param>
    public static implicit operator RouteLocationRaw(string value)
        => new(value);

    /// <summary>
    /// 从路径式路由位置隐式转换。
    /// Implicitly converts from a path-based route location.
    /// </summary>
    /// <param name="value">要转换的路径式路由位置。The path-based route location to convert.</param>
    public static implicit operator RouteLocationRaw(RouteLocationAsPath value)
        => new(value);

    /// <summary>
    /// 从相对式路由位置隐式转换。
    /// Implicitly converts from a relative route location.
    /// </summary>
    /// <param name="value">要转换的相对式路由位置。The relative route location to convert.</param>
    public static implicit operator RouteLocationRaw(RouteLocationAsRelative value)
        => new(value);

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteLocationRawMaybeRef
{
    private readonly byte _kind;
    private readonly RouteLocationRaw? _value;
    private readonly Vue3.IVueRef<RouteLocationRaw>? _ref;
    private readonly Vue3.VueReadonlyRef<RouteLocationRaw>? _readonlyRef;
    private readonly Vue3.IVueRef<string>? _stringRef;
    private readonly Vue3.IVueRef<RouteLocationAsPath>? _pathRef;
    private readonly Vue3.IVueRef<RouteLocationAsRelative>? _relativeRef;
    private readonly Vue3.VueReadonlyRef<string>? _readonlyStringRef;
    private readonly Vue3.VueReadonlyRef<RouteLocationAsPath>? _readonlyPathRef;
    private readonly Vue3.VueReadonlyRef<RouteLocationAsRelative>? _readonlyRelativeRef;

    /// <summary>
    /// 从 RouteLocationRaw 值初始化。
    /// Initializes from a RouteLocationRaw value.
    /// </summary>
    /// <param name="value">路由位置原始值。The raw route location value.</param>
    private RouteLocationRawMaybeRef(RouteLocationRaw value)
    {
        _kind = 1;
        _value = value;
        _ref = default;
        _readonlyRef = default;
        _stringRef = default;
        _pathRef = default;
        _relativeRef = default;
        _readonlyStringRef = default;
        _readonlyPathRef = default;
        _readonlyRelativeRef = default;
    }

    /// <summary>
    /// 从 RouteLocationRaw 的 Vue Ref 初始化。
    /// Initializes from a Vue Ref of RouteLocationRaw.
    /// </summary>
    /// <param name="value">RouteLocationRaw 的响应式引用。The reactive ref of RouteLocationRaw.</param>
    private RouteLocationRawMaybeRef(Vue3.IVueRef<RouteLocationRaw> value)
    {
        _kind = 2;
        _value = default;
        _ref = value;
        _readonlyRef = default;
        _stringRef = default;
        _pathRef = default;
        _relativeRef = default;
        _readonlyStringRef = default;
        _readonlyPathRef = default;
        _readonlyRelativeRef = default;
    }

    /// <summary>
    /// 从 RouteLocationRaw 的 Vue ReadonlyRef 初始化。
    /// Initializes from a Vue ReadonlyRef of RouteLocationRaw.
    /// </summary>
    /// <param name="value">RouteLocationRaw 的只读响应式引用。The readonly ref of RouteLocationRaw.</param>
    private RouteLocationRawMaybeRef(Vue3.VueReadonlyRef<RouteLocationRaw> value)
    {
        _kind = 3;
        _value = default;
        _ref = default;
        _readonlyRef = value;
        _stringRef = default;
        _pathRef = default;
        _relativeRef = default;
        _readonlyStringRef = default;
        _readonlyPathRef = default;
        _readonlyRelativeRef = default;
    }

    /// <summary>
    /// 从字符串的 Vue Ref 初始化。
    /// Initializes from a Vue Ref of string.
    /// </summary>
    /// <param name="value">字符串的响应式引用。The reactive ref of string.</param>
    private RouteLocationRawMaybeRef(Vue3.IVueRef<string> value)
    {
        _kind = 4;
        _value = default;
        _ref = default;
        _readonlyRef = default;
        _stringRef = value;
        _pathRef = default;
        _relativeRef = default;
        _readonlyStringRef = default;
        _readonlyPathRef = default;
        _readonlyRelativeRef = default;
    }

    /// <summary>
    /// 从 RouteLocationAsPath 的 Vue Ref 初始化。
    /// Initializes from a Vue Ref of RouteLocationAsPath.
    /// </summary>
    /// <param name="value">路径式路由位置的响应式引用。The reactive ref of path-based route location.</param>
    private RouteLocationRawMaybeRef(Vue3.IVueRef<RouteLocationAsPath> value)
    {
        _kind = 5;
        _value = default;
        _ref = default;
        _readonlyRef = default;
        _stringRef = default;
        _pathRef = value;
        _relativeRef = default;
        _readonlyStringRef = default;
        _readonlyPathRef = default;
        _readonlyRelativeRef = default;
    }

    /// <summary>
    /// 从 RouteLocationAsRelative 的 Vue Ref 初始化。
    /// Initializes from a Vue Ref of RouteLocationAsRelative.
    /// </summary>
    /// <param name="value">相对式路由位置的响应式引用。The reactive ref of relative route location.</param>
    private RouteLocationRawMaybeRef(Vue3.IVueRef<RouteLocationAsRelative> value)
    {
        _kind = 6;
        _value = default;
        _ref = default;
        _readonlyRef = default;
        _stringRef = default;
        _pathRef = default;
        _relativeRef = value;
        _readonlyStringRef = default;
        _readonlyPathRef = default;
        _readonlyRelativeRef = default;
    }

    /// <summary>
    /// 从字符串的 Vue ReadonlyRef 初始化。
    /// Initializes from a Vue ReadonlyRef of string.
    /// </summary>
    /// <param name="value">字符串的只读响应式引用。The readonly ref of string.</param>
    private RouteLocationRawMaybeRef(Vue3.VueReadonlyRef<string> value)
    {
        _kind = 7;
        _value = default;
        _ref = default;
        _readonlyRef = default;
        _stringRef = default;
        _pathRef = default;
        _relativeRef = default;
        _readonlyStringRef = value;
        _readonlyPathRef = default;
        _readonlyRelativeRef = default;
    }

    /// <summary>
    /// 从 RouteLocationAsPath 的 Vue ReadonlyRef 初始化。
    /// Initializes from a Vue ReadonlyRef of RouteLocationAsPath.
    /// </summary>
    /// <param name="value">路径式路由位置的只读响应式引用。The readonly ref of path-based route location.</param>
    private RouteLocationRawMaybeRef(Vue3.VueReadonlyRef<RouteLocationAsPath> value)
    {
        _kind = 8;
        _value = default;
        _ref = default;
        _readonlyRef = default;
        _stringRef = default;
        _pathRef = default;
        _relativeRef = default;
        _readonlyStringRef = default;
        _readonlyPathRef = value;
        _readonlyRelativeRef = default;
    }

    /// <summary>
    /// 从 RouteLocationAsRelative 的 Vue ReadonlyRef 初始化。
    /// Initializes from a Vue ReadonlyRef of RouteLocationAsRelative.
    /// </summary>
    /// <param name="value">相对式路由位置的只读响应式引用。The readonly ref of relative route location.</param>
    private RouteLocationRawMaybeRef(Vue3.VueReadonlyRef<RouteLocationAsRelative> value)
    {
        _kind = 9;
        _value = default;
        _ref = default;
        _readonlyRef = default;
        _stringRef = default;
        _pathRef = default;
        _relativeRef = default;
        _readonlyStringRef = default;
        _readonlyPathRef = default;
        _readonlyRelativeRef = value;
    }

    /// <summary>
    /// 以 RouteLocationRaw 值返回，如果不是值变体则返回 default。
    /// Returns as RouteLocationRaw value, or default if not a value variant.
    /// </summary>
    public RouteLocationRaw? AsValue => _kind == 1 ? _value : default;

    /// <summary>
    /// 以 Vue Ref 返回，如果不是 Ref 变体则返回 default。
    /// Returns as Vue Ref, or default if not a Ref variant.
    /// </summary>
    public Vue3.IVueRef<RouteLocationRaw>? AsRef => _kind == 2 ? _ref : default;

    /// <summary>
    /// 以 Vue ReadonlyRef 返回，如果不是 ReadonlyRef 变体则返回 default。
    /// Returns as Vue ReadonlyRef, or default if not a ReadonlyRef variant.
    /// </summary>
    public Vue3.VueReadonlyRef<RouteLocationRaw>? AsReadonlyRef => _kind == 3 ? _readonlyRef : default;

    /// <summary>
    /// 以字符串 Vue Ref 返回，如果不是字符串 Ref 变体则返回 default。
    /// Returns as string Vue Ref, or default if not a string Ref variant.
    /// </summary>
    public Vue3.IVueRef<string>? AsStringRef => _kind == 4 ? _stringRef : default;

    /// <summary>
    /// 以路径式路由位置 Vue Ref 返回，如果不是路径 Ref 变体则返回 default。
    /// Returns as path-based route location Vue Ref, or default if not a path Ref variant.
    /// </summary>
    public Vue3.IVueRef<RouteLocationAsPath>? AsPathRef => _kind == 5 ? _pathRef : default;

    /// <summary>
    /// 以相对式路由位置 Vue Ref 返回，如果不是相对 Ref 变体则返回 default。
    /// Returns as relative route location Vue Ref, or default if not a relative Ref variant.
    /// </summary>
    public Vue3.IVueRef<RouteLocationAsRelative>? AsRelativeRef => _kind == 6 ? _relativeRef : default;

    /// <summary>
    /// 以字符串 Vue ReadonlyRef 返回，如果不是字符串只读引用变体则返回 default。
    /// Returns as string Vue ReadonlyRef, or default if not a string readonly ref variant.
    /// </summary>
    public Vue3.VueReadonlyRef<string>? AsReadonlyStringRef => _kind == 7 ? _readonlyStringRef : default;

    /// <summary>
    /// 以路径式路由位置 Vue ReadonlyRef 返回，如果不是路径只读引用变体则返回 default。
    /// Returns as path-based route location Vue ReadonlyRef, or default if not a path readonly ref variant.
    /// </summary>
    public Vue3.VueReadonlyRef<RouteLocationAsPath>? AsReadonlyPathRef => _kind == 8 ? _readonlyPathRef : default;

    /// <summary>
    /// 以相对式路由位置 Vue ReadonlyRef 返回，如果不是相对只读引用变体则返回 default。
    /// Returns as relative route location Vue ReadonlyRef, or default if not a relative readonly ref variant.
    /// </summary>
    public Vue3.VueReadonlyRef<RouteLocationAsRelative>? AsReadonlyRelativeRef => _kind == 9 ? _readonlyRelativeRef : default;

    /// <summary>
    /// 从 RouteLocationRaw 隐式转换。
    /// Implicitly converts from a RouteLocationRaw.
    /// </summary>
    /// <param name="value">要转换的路由位置原始值。The raw route location to convert.</param>
    public static implicit operator RouteLocationRawMaybeRef(RouteLocationRaw value)
        => new(value);

    /// <summary>
    /// 从字符串隐式转换。
    /// Implicitly converts from a string.
    /// </summary>
    /// <param name="value">要转换的路径字符串。The path string to convert.</param>
    public static implicit operator RouteLocationRawMaybeRef(string value)
        => new(value);

    /// <summary>
    /// 从路径式路由位置隐式转换。
    /// Implicitly converts from a path-based route location.
    /// </summary>
    /// <param name="value">要转换的路径式路由位置。The path-based route location to convert.</param>
    public static implicit operator RouteLocationRawMaybeRef(RouteLocationAsPath value)
        => new(value);

    /// <summary>
    /// 从相对式路由位置隐式转换。
    /// Implicitly converts from a relative route location.
    /// </summary>
    /// <param name="value">要转换的相对式路由位置。The relative route location to convert.</param>
    public static implicit operator RouteLocationRawMaybeRef(RouteLocationAsRelative value)
        => new(value);

    /// <summary>
    ///从 RouteLocationRaw 的 Vue ReadonlyRef 隐式转换。
    /// Implicitly converts from a Vue ReadonlyRef of RouteLocationRaw.
    /// </summary>
    /// <param name="value">要转换的只读响应式引用。The readonly ref to convert.</param>
    public static implicit operator RouteLocationRawMaybeRef(Vue3.VueReadonlyRef<RouteLocationRaw> value)
        => new(value);

    /// <summary>
    /// 从字符串的 Vue ReadonlyRef 隐式转换。
    /// Implicitly converts from a Vue ReadonlyRef of string.
    /// </summary>
    /// <param name="value">要转换的字符串只读响应式引用。The string readonly ref to convert.</param>
    public static implicit operator RouteLocationRawMaybeRef(Vue3.VueReadonlyRef<string> value)
        => new(value);

    /// <summary>
    /// 从路径式路由位置的 Vue ReadonlyRef 隐式转换。
    /// Implicitly converts from a Vue ReadonlyRef of RouteLocationAsPath.
    /// </summary>
    /// <param name="value">要转换的路径式只读响应式引用。The path-based readonly ref to convert.</param>
    public static implicit operator RouteLocationRawMaybeRef(Vue3.VueReadonlyRef<RouteLocationAsPath> value)
        => new(value);

    /// <summary>
    /// 从相对式路由位置的 Vue ReadonlyRef 隐式转换。
    /// Implicitly converts from a Vue ReadonlyRef of RouteLocationAsRelative.
    /// </summary>
    /// <param name="value">要转换的相对式只读响应式引用。The relative readonly ref to convert.</param>
    public static implicit operator RouteLocationRawMaybeRef(Vue3.VueReadonlyRef<RouteLocationAsRelative> value)
        => new(value);

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteBooleanMaybeRef
{
    private readonly byte _kind;
    private readonly bool? _value;
    private readonly Vue3.IVueRef<bool>? _ref;
    private readonly Vue3.VueReadonlyRef<bool>? _readonlyRef;

    /// <summary>
    /// 从布尔值初始化。
    /// Initializes from a boolean value.
    /// </summary>
    /// <param name="value">布尔值。The boolean value.</param>
    private RouteBooleanMaybeRef(bool value)
    {
        _kind = 1;
        _value = value;
        _ref = default;
        _readonlyRef = default;
    }

    /// <summary>
    /// 从布尔值的 Vue Ref 初始化。
    /// Initializes from a Vue Ref of bool.
    /// </summary>
    /// <param name="value">布尔值的响应式引用。The reactive ref of bool.</param>
    private RouteBooleanMaybeRef(Vue3.IVueRef<bool> value)
    {
        _kind = 2;
        _value = default;
        _ref = value;
        _readonlyRef = default;
    }

    /// <summary>
    /// 从布尔值的 Vue ReadonlyRef 初始化。
    /// Initializes from a Vue ReadonlyRef of bool.
    /// </summary>
    /// <param name="value">布尔值的只读响应式引用。The readonly ref of bool.</param>
    private RouteBooleanMaybeRef(Vue3.VueReadonlyRef<bool> value)
    {
        _kind = 3;
        _value = default;
        _ref = default;
        _readonlyRef = value;
    }

    /// <summary>
    /// 以布尔值返回，如果不是值变体则返回 default。
    /// Returns as bool, or default if not a value variant.
    /// </summary>
    public bool? AsValue => _kind == 1 ? _value : default;

    /// <summary>
    /// 以 Vue Ref 返回，如果不是 Ref 变体则返回 default。
    /// Returns as Vue Ref, or default if not a Ref variant.
    /// </summary>
    public Vue3.IVueRef<bool>? AsRef => _kind == 2 ? _ref : default;

    /// <summary>
    /// 以 Vue ReadonlyRef 返回，如果不是 ReadonlyRef 变体则返回 default。
    /// Returns as Vue ReadonlyRef, or default if not a ReadonlyRef variant.
    /// </summary>
    public Vue3.VueReadonlyRef<bool>? AsReadonlyRef => _kind == 3 ? _readonlyRef : default;

    /// <summary>
    /// 从布尔值隐式转换。
    /// Implicitly converts from a bool.
    /// </summary>
    /// <param name="value">要转换的布尔值。The bool value to convert.</param>
    public static implicit operator RouteBooleanMaybeRef(bool value)
        => new(value);

    /// <summary>
    /// 从布尔值的 Vue ReadonlyRef 隐式转换。
    /// Implicitly converts from a Vue ReadonlyRef of bool.
    /// </summary>
    /// <param name="value">要转换的只读响应式引用。The readonly ref to convert.</param>
    public static implicit operator RouteBooleanMaybeRef(Vue3.VueReadonlyRef<bool> value)
        => new(value);

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouterViewDepthValue
{
    private readonly byte _kind;
    private readonly Number? _number;
    private readonly Vue3.IVueRef<Number>? _ref;

    /// <summary>
    /// 从 Number 值初始化。
    /// Initializes from a Number value.
    /// </summary>
    /// <param name="value">深度数值。The depth number value.</param>
    private RouterViewDepthValue(Number value)
    {
        _kind = 1;
        _number = value;
        _ref = default;
    }

    /// <summary>
    /// 从 Number 的 Vue Ref 初始化。
    /// Initializes from a Vue Ref of Number.
    /// </summary>
    /// <param name="value">Number 的响应式引用。The reactive ref of Number.</param>
    private RouterViewDepthValue(Vue3.IVueRef<Number> value)
    {
        _kind = 2;
        _number = default;
        _ref = value;
    }

    /// <summary>
    /// 以 Number 返回，如果不是 Number 变体则返回 default。
    /// Returns as Number, or default if not a Number variant.
    /// </summary>
    public Number? AsNumber => _kind == 1 ? _number : default;

    /// <summary>
    /// 以 Vue Ref 返回，如果不是 Ref 变体则返回 default。
    /// Returns as Vue Ref, or default if not a Ref variant.
    /// </summary>
    public Vue3.IVueRef<Number>? AsRef => _kind == 2 ? _ref : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct HistoryStateValue
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly Number? _number;
    private readonly bool? _bool;
    private readonly HistoryState? _object;
    private readonly Array<HistoryStateValue?>? _array;

    /// <summary>
    /// 从字符串值初始化。
    /// Initializes from a string value.
    /// </summary>
    /// <param name="value">状态字符串值。The state string value.</param>
    private HistoryStateValue(string value)
    {
        _kind = 1;
        _string = value;
        _number = default;
        _bool = default;
        _object = default;
        _array = default;
    }

    /// <summary>
    /// 从 Number 值初始化。
    /// Initializes from a Number value.
    /// </summary>
    /// <param name="value">状态数值。The state number value.</param>
    private HistoryStateValue(Number value)
    {
        _kind = 2;
        _string = default;
        _number = value;
        _bool = default;
        _object = default;
        _array = default;
    }

    /// <summary>
    /// 从布尔值初始化。
    /// Initializes from a bool value.
    /// </summary>
    /// <param name="value">状态布尔值。The state bool value.</param>
    private HistoryStateValue(bool value)
    {
        _kind = 3;
        _string = default;
        _number = default;
        _bool = value;
        _object = default;
        _array = default;
    }

    /// <summary>
    /// 从 HistoryState 对象初始化。
    /// Initializes from a HistoryState object.
    /// </summary>
    /// <param name="value">历史状态对象。The history state object.</param>
    private HistoryStateValue(HistoryState value)
    {
        _kind = 4;
        _string = default;
        _number = default;
        _bool = default;
        _object = value;
        _array = default;
    }

    /// <summary>
    /// 从 HistoryStateValue 数组初始化。
    /// Initializes from an array of HistoryStateValue.
    /// </summary>
    /// <param name="value">状态值数组。The array of state values.</param>
    private HistoryStateValue(Array<HistoryStateValue?> value)
    {
        _kind = 5;
        _string = default;
        _number = default;
        _bool = default;
        _object = default;
        _array = value;
    }

    /// <summary>
    /// 以字符串返回，如果不是字符串变体则返回 default。
    /// Returns as string, or default if not a string variant.
    /// </summary>
    public string? AsString => _kind == 1 ? _string : default;

    /// <summary>
    /// 以 Number 返回，如果不是 Number 变体则返回 default。
    /// Returns as Number, or default if not a Number variant.
    /// </summary>
    public Number? AsNumber => _kind == 2 ? _number : default;

    /// <summary>
    /// 以布尔值返回，如果不是布尔变体则返回 default。
    /// Returns as bool, or default if not a bool variant.
    /// </summary>
    public bool? AsBool => _kind == 3 ? _bool : default;

    /// <summary>
    /// 以 HistoryState 对象返回，如果不是对象变体则返回 default。
    /// Returns as HistoryState, or default if not an object variant.
    /// </summary>
    public HistoryState? AsObject => _kind == 4 ? _object : default;

    /// <summary>
    /// 以数组返回，如果不是数组变体则返回 default。
    /// Returns as array, or default if not an array variant.
    /// </summary>
    public Array<HistoryStateValue?>? AsArray => _kind == 5 ? _array : default;

    /// <summary>
    /// 从字符串隐式转换。
    /// Implicitly converts from a string.
    /// </summary>
    /// <param name="value">要转换的字符串值。The string value to convert.</param>
    public static implicit operator HistoryStateValue(string value)
        => new(value);

    /// <summary>
    /// 从 Number 隐式转换。
    /// Implicitly converts from a Number.
    /// </summary>
    /// <param name="value">要转换的 Number 值。The Number value to convert.</param>
    public static implicit operator HistoryStateValue(Number value)
        => new(value);

    /// <summary>
    /// 从布尔值隐式转换。
    /// Implicitly converts from a bool.
    /// </summary>
    /// <param name="value">要转换的布尔值。The bool value to convert.</param>
    public static implicit operator HistoryStateValue(bool value)
        => new(value);

    /// <summary>
    /// 从 HistoryState 对象隐式转换。
    /// Implicitly converts from a HistoryState object.
    /// </summary>
    /// <param name="value">要转换的历史状态对象。The HistoryState to convert.</param>
    public static implicit operator HistoryStateValue(HistoryState value)
        => new(value);

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouterErrorValue
{
    private readonly byte _kind;
    private readonly Error? _error;
    private readonly string? _string;
    private readonly Number? _number;
    private readonly bool? _bool;
    private readonly BigInt? _bigInt;
    private readonly Symbol? _symbol;
    private readonly IObject? _object;
    private readonly Array<RouterErrorValue?>? _array;

    /// <summary>
    /// 从 Error 值初始化。
    /// Initializes from an Error value.
    /// </summary>
    /// <param name="value">错误对象。The error object.</param>
    private RouterErrorValue(Error value)
    {
        _kind = 1;
        _error = value;
        _string = default;
        _number = default;
        _bool = default;
        _bigInt = default;
        _symbol = default;
        _object = default;
        _array = default;
    }

    /// <summary>
    /// 从字符串值初始化。
    /// Initializes from a string value.
    /// </summary>
    /// <param name="value">错误字符串。The error string.</param>
    private RouterErrorValue(string value)
    {
        _kind = 2;
        _error = default;
        _string = value;
        _number = default;
        _bool = default;
        _bigInt = default;
        _symbol = default;
        _object = default;
        _array = default;
    }

    /// <summary>
    /// 从 Number 值初始化。
    /// Initializes from a Number value.
    /// </summary>
    /// <param name="value">错误数值。The error number value.</param>
    private RouterErrorValue(Number value)
    {
        _kind = 3;
        _error = default;
        _string = default;
        _number = value;
        _bool = default;
        _bigInt = default;
        _symbol = default;
        _object = default;
        _array = default;
    }

    /// <summary>
    /// 从布尔值初始化。
    /// Initializes from a bool value.
    /// </summary>
    /// <param name="value">错误布尔值。The error bool value.</param>
    private RouterErrorValue(bool value)
    {
        _kind = 4;
        _error = default;
        _string = default;
        _number = default;
        _bool = value;
        _bigInt = default;
        _symbol = default;
        _object = default;
        _array = default;
    }

    /// <summary>
    /// 从 BigInt 值初始化。
    /// Initializes from a BigInt value.
    /// </summary>
    /// <param name="value">错误 BigInt 值。The error BigInt value.</param>
    private RouterErrorValue(BigInt value)
    {
        _kind = 5;
        _error = default;
        _string = default;
        _number = default;
        _bool = default;
        _bigInt = value;
        _symbol = default;
        _object = default;
        _array = default;
    }

    /// <summary>
    /// 从 Symbol 值初始化。
    /// Initializes from a Symbol value.
    /// </summary>
    /// <param name="value">错误 Symbol 值。The error Symbol value.</param>
    private RouterErrorValue(Symbol value)
    {
        _kind = 6;
        _error = default;
        _string = default;
        _number = default;
        _bool = default;
        _bigInt = default;
        _symbol = value;
        _object = default;
        _array = default;
    }

    /// <summary>
    /// 从 IObject 值初始化。
    /// Initializes from an IObject value.
    /// </summary>
    /// <param name="value">错误对象。The error object.</param>
    private RouterErrorValue(IObject value)
    {
        _kind = 7;
        _error = default;
        _string = default;
        _number = default;
        _bool = default;
        _bigInt = default;
        _symbol = default;
        _object = value;
        _array = default;
    }

    /// <summary>
    /// 从 RouterErrorValue 数组初始化。
    /// Initializes from an array of RouterErrorValue.
    /// </summary>
    /// <param name="value">错误值数组。The array of error values.</param>
    private RouterErrorValue(Array<RouterErrorValue?> value)
    {
        _kind = 8;
        _error = default;
        _string = default;
        _number = default;
        _bool = default;
        _bigInt = default;
        _symbol = default;
        _object = default;
        _array = value;
    }

    /// <summary>
    /// 以 Error 返回，如果不是 Error 变体则返回 default。
    /// Returns as Error, or default if not an Error variant.
    /// </summary>
    public Error? AsError => _kind == 1 ? _error : default;

    /// <summary>
    /// 以字符串返回，如果不是字符串变体则返回 default。
    /// Returns as string, or default if not a string variant.
    /// </summary>
    public string? AsString => _kind == 2 ? _string : default;

    /// <summary>
    /// 以 Number 返回，如果不是 Number 变体则返回 default。
    /// Returns as Number, or default if not a Number variant.
    /// </summary>
    public Number? AsNumber => _kind == 3 ? _number : default;

    /// <summary>
    /// 以布尔值返回，如果不是布尔变体则返回 default。
    /// Returns as bool, or default if not a bool variant.
    /// </summary>
    public bool? AsBool => _kind == 4 ? _bool : default;

    /// <summary>
    /// 以 BigInt 返回，如果不是 BigInt 变体则返回 default。
    /// Returns as BigInt, or default if not a BigInt variant.
    /// </summary>
    public BigInt? AsBigInt => _kind == 5 ? _bigInt : default;

    /// <summary>
    /// 以 Symbol 返回，如果不是 Symbol 变体则返回 default。
    /// Returns as Symbol, or default if not a Symbol variant.
    /// </summary>
    public Symbol? AsSymbol => _kind == 6 ? _symbol : default;

    /// <summary>
    /// 以 IObject 返回，如果不是 IObject 变体则返回 default。
    /// Returns as IObject, or default if not an IObject variant.
    /// </summary>
    public IObject? AsObject => _kind == 7 ? _object : default;

    /// <summary>
    /// 以数组返回，如果不是数组变体则返回 default。
    /// Returns as array, or default if not an array variant.
    /// </summary>
    public Array<RouterErrorValue?>? AsArray => _kind == 8 ? _array : default;

    /// <summary>
    /// 从 Error 隐式转换。
    /// Implicitly converts from an Error.
    /// </summary>
    /// <param name="value">要转换的错误对象。The Error to convert.</param>
    public static implicit operator RouterErrorValue(Error value)
        => new(value);

    /// <summary>
    /// 从字符串隐式转换。
    /// Implicitly converts from a string.
    /// </summary>
    /// <param name="value">要转换的字符串值。The string value to convert.</param>
    public static implicit operator RouterErrorValue(string value)
        => new(value);

    /// <summary>
    /// 从 Number 隐式转换。
    /// Implicitly converts from a Number.
    /// </summary>
    /// <param name="value">要转换的 Number 值。The Number value to convert.</param>
    public static implicit operator RouterErrorValue(Number value)
        => new(value);

    /// <summary>
    /// 从布尔值隐式转换。
    /// Implicitly converts from a bool.
    /// </summary>
    /// <param name="value">要转换的布尔值。The bool value to convert.</param>
    public static implicit operator RouterErrorValue(bool value)
        => new(value);

    /// <summary>
    /// 从 BigInt 隐式转换。
    /// Implicitly converts from a BigInt.
    /// </summary>
    /// <param name="value">要转换的 BigInt 值。The BigInt value to convert.</param>
    public static implicit operator RouterErrorValue(BigInt value)
        => new(value);

    /// <summary>
    /// 从 Symbol 隐式转换。
    /// Implicitly converts from a Symbol.
    /// </summary>
    /// <param name="value">要转换的 Symbol 值。The Symbol value to convert.</param>
    public static implicit operator RouterErrorValue(Symbol value)
        => new(value);

    /// <summary>
    /// 从 RouterErrorValue 数组隐式转换。
    /// Implicitly converts from an Array of RouterErrorValue.
    /// </summary>
    /// <param name="value">要转换的 RouterErrorValue 数组。The Array of RouterErrorValue to convert.</param>
    public static implicit operator RouterErrorValue(Array<RouterErrorValue?> value)
        => new(value);

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RawRouteComponent
{
    private readonly byte _kind;
    private readonly IVueComponent? _component;
    private readonly RouteComponentLoader? _loader;

    /// <summary>
    /// 从 IVueComponent 初始化。
    /// Initializes from an IVueComponent.
    /// </summary>
    /// <param name="value">Vue 组件。The Vue component.</param>
    private RawRouteComponent(IVueComponent value)
    {
        _kind = 1;
        _component = value;
        _loader = default;
    }

    /// <summary>
    /// 从 RouteComponentLoader 初始化。
    /// Initializes from a RouteComponentLoader.
    /// </summary>
    /// <param name="value">组件加载器。The component loader.</param>
    private RawRouteComponent(RouteComponentLoader value)
    {
        _kind = 2;
        _component = default;
        _loader = value;
    }

    /// <summary>
    /// 以 IVueComponent 返回，如果不是组件变体则返回 default。
    /// Returns as IVueComponent, or default if not a component variant.
    /// </summary>
    public IVueComponent? AsComponent => _kind == 1 ? _component : default;

    /// <summary>
    /// 以 RouteComponentLoader 返回，如果不是加载器变体则返回 default。
    /// Returns as RouteComponentLoader, or default if not a loader variant.
    /// </summary>
    public RouteComponentLoader? AsLoader => _kind == 2 ? _loader : default;

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
    /// 从 RouteComponentLoader 隐式转换。
    /// Implicitly converts from a RouteComponentLoader.
    /// </summary>
    /// <param name="value">要转换的组件加载器。The component loader to convert.</param>
    public static implicit operator RawRouteComponent(RouteComponentLoader value)
        => new(value);

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteComponent
{
    private readonly byte _kind;
    private readonly IVueComponent? _component;
    private readonly RouteComponentLoader? _loader;

    /// <summary>
    /// 从 IVueComponent 初始化。
    /// Initializes from an IVueComponent.
    /// </summary>
    /// <param name="value">Vue 组件。The Vue component.</param>
    private RouteComponent(IVueComponent value)
    {
        _kind = 1;
        _component = value;
        _loader = default;
    }

    /// <summary>
    /// 从 RouteComponentLoader 初始化。
    /// Initializes from a RouteComponentLoader.
    /// </summary>
    /// <param name="value">组件加载器。The component loader.</param>
    private RouteComponent(RouteComponentLoader value)
    {
        _kind = 2;
        _component = default;
        _loader = value;
    }

    /// <summary>
    /// 以 IVueComponent 返回，如果不是组件变体则返回 default。
    /// Returns as IVueComponent, or default if not a component variant.
    /// </summary>
    public IVueComponent? AsComponent => _kind == 1 ? _component : default;

    /// <summary>
    /// 以 RouteComponentLoader 返回，如果不是加载器变体则返回 default。
    /// Returns as RouteComponentLoader, or default if not a loader variant.
    /// </summary>
    public RouteComponentLoader? AsLoader => _kind == 2 ? _loader : default;

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

    /// <summary>
    /// 从 RouteComponentLoader 隐式转换。
    /// Implicitly converts from a RouteComponentLoader.
    /// </summary>
    /// <param name="value">要转换的组件加载器。The component loader to convert.</param>
    public static implicit operator RouteComponent(RouteComponentLoader value)
        => new(value);
}

/// <summary>
/// 路由记录 Props 联合类型，接受 bool、VueProps 或 RouteRecordPropsResolver。
/// Route record props union accepting bool, VueProps, or RouteRecordPropsResolver.
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteRecordProps
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly Vue3.VueProps? _props;
    private readonly RouteRecordPropsResolver? _resolver;

    /// <summary>
    /// 从布尔值初始化。
    /// Initializes from a bool value.
    /// </summary>
    /// <param name="value">是否将路由参数作为 props 传递。Whether to pass route params as props.</param>
    private RouteRecordProps(bool value)
    {
        _kind = 1;
        _bool = value;
        _props = default;
        _resolver = default;
    }

    /// <summary>
    /// 从 VueProps 初始化。
    /// Initializes from VueProps.
    /// </summary>
    /// <param name="value">Props 对象。The props object.</param>
    private RouteRecordProps(Vue3.VueProps value)
    {
        _kind = 2;
        _bool = default;
        _props = value;
        _resolver = default;
    }

    /// <summary>
    /// 从 RouteRecordPropsResolver 初始化。
    /// Initializes from a RouteRecordPropsResolver.
    /// </summary>
    /// <param name="value">Props 解析函数。The props resolver function.</param>
    private RouteRecordProps(RouteRecordPropsResolver value)
    {
        _kind = 3;
        _bool = default;
        _props = default;
        _resolver = value;
    }

    /// <summary>
    /// 以布尔值返回，如果不是布尔变体则返回 default。
    /// Returns as bool, or default if not a bool variant.
    /// </summary>
    public bool? AsBool => _kind == 1 ? _bool : default;

    /// <summary>
    /// 以 VueProps 返回，如果不是 Props 变体则返回 default。
    /// Returns as VueProps, or default if not a Props variant.
    /// </summary>
    public Vue3.VueProps? AsProps => _kind == 2 ? _props : default;

    /// <summary>
    /// 以 RouteRecordPropsResolver 返回，如果不是解析器变体则返回 default。
    /// Returns as RouteRecordPropsResolver, or default if not a resolver variant.
    /// </summary>
    public RouteRecordPropsResolver? AsResolver => _kind == 3 ? _resolver : default;

    /// <summary>
    /// 从布尔值隐式转换。
    /// Implicitly converts from a bool.
    /// </summary>
    /// <param name="value">要转换的布尔值。The bool value to convert.</param>
    public static implicit operator RouteRecordProps(bool value)
        => new(value);

    /// <summary>
    /// 从 VueProps 隐式转换。
    /// Implicitly converts from VueProps.
    /// </summary>
    /// <param name="value">要转换的 Props 对象。The VueProps to convert.</param>
    public static implicit operator RouteRecordProps(Vue3.VueProps value)
        => new(value);

    /// <summary>
    /// 从 RouteRecordPropsResolver 隐式转换。
    /// Implicitly converts from a RouteRecordPropsResolver.
    /// </summary>
    /// <param name="value">要转换的 Props 解析函数。The props resolver to convert.</param>
    public static implicit operator RouteRecordProps(RouteRecordPropsResolver value)
        => new(value);

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteRecordNamedViewProps
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly RouteNamedProps? _namedProps;

    /// <summary>
    /// 从布尔值初始化。
    /// Initializes from a bool value.
    /// </summary>
    /// <param name="value">是否将路由参数作为 props 传递。Whether to pass route params as props.</param>
    private RouteRecordNamedViewProps(bool value)
    {
        _kind = 1;
        _bool = value;
        _namedProps = default;
    }

    /// <summary>
    /// 从 RouteNamedProps 初始化。
    /// Initializes from RouteNamedProps.
    /// </summary>
    /// <param name="value">命名视图 Props 映射。The named view props mapping.</param>
    private RouteRecordNamedViewProps(RouteNamedProps value)
    {
        _kind = 2;
        _bool = default;
        _namedProps = value;
    }

    /// <summary>
    /// 以布尔值返回，如果不是布尔变体则返回 default。
    /// Returns as bool, or default if not a bool variant.
    /// </summary>
    public bool? AsBool => _kind == 1 ? _bool : default;

    /// <summary>
    /// 以 RouteNamedProps 返回，如果不是命名 Props 变体则返回 default。
    /// Returns as RouteNamedProps, or default if not a named props variant.
    /// </summary>
    public RouteNamedProps? AsNamedProps => _kind == 2 ? _namedProps : default;

    /// <summary>
    /// 从布尔值隐式转换。
    /// Implicitly converts from a bool.
    /// </summary>
    /// <param name="value">要转换的布尔值。The bool value to convert.</param>
    public static implicit operator RouteRecordNamedViewProps(bool value)
        => new(value);

    /// <summary>
    /// 从 RouteNamedProps 隐式转换。
    /// Implicitly converts from RouteNamedProps.
    /// </summary>
    /// <param name="value">要转换的命名 Props。The named props to convert.</param>
    public static implicit operator RouteRecordNamedViewProps(RouteNamedProps value)
        => new(value);

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct NavigationGuardNextArgument
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly RouteLocationRaw? _location;
    private readonly NavigationGuardNextCallback? _callback;
    private readonly Error? _error;

    /// <summary>
    /// 从布尔值初始化。
    /// Initializes from a bool value.
    /// </summary>
    /// <param name="value">是否允许导航。Whether to allow navigation.</param>
    private NavigationGuardNextArgument(bool value)
    {
        _kind = 1;
        _bool = value;
        _location = default;
        _callback = default;
        _error = default;
    }

    /// <summary>
    /// 从 RouteLocationRaw 初始化。
    /// Initializes from a RouteLocationRaw.
    /// </summary>
    /// <param name="value">要重定向到的路由位置。The route location to redirect to.</param>
    private NavigationGuardNextArgument(RouteLocationRaw value)
    {
        _kind = 2;
        _bool = default;
        _location = value;
        _callback = default;
        _error = default;
    }

    /// <summary>
    /// 从 NavigationGuardNextCallback 初始化。
    /// Initializes from a NavigationGuardNextCallback.
    /// </summary>
    /// <param name="value">导航守卫回调。The navigation guard callback.</param>
    private NavigationGuardNextArgument(NavigationGuardNextCallback value)
    {
        _kind = 3;
        _bool = default;
        _location = default;
        _callback = value;
        _error = default;
    }

    /// <summary>
    /// 从 Error 初始化。
    /// Initializes from an Error.
    /// </summary>
    /// <param name="value">要中止导航的错误。The error to abort navigation with.</param>
    private NavigationGuardNextArgument(Error value)
    {
        _kind = 4;
        _bool = default;
        _location = default;
        _callback = default;
        _error = value;
    }

    /// <summary>
    /// 以布尔值返回，如果不是布尔变体则返回 default。
    /// Returns as bool, or default if not a bool variant.
    /// </summary>
    public bool? AsBool => _kind == 1 ? _bool : default;

    /// <summary>
    /// 以 RouteLocationRaw 返回，如果不是位置变体则返回 default。
    /// Returns as RouteLocationRaw, or default if not a location variant.
    /// </summary>
    public RouteLocationRaw? AsLocation => _kind == 2 ? _location : default;

    /// <summary>
    /// 以 NavigationGuardNextCallback 返回，如果不是回调变体则返回 default。
    /// Returns as NavigationGuardNextCallback, or default if not a callback variant.
    /// </summary>
    public NavigationGuardNextCallback? AsCallback => _kind == 3 ? _callback : default;

    /// <summary>
    /// 以 Error 返回，如果不是错误变体则返回 default。
    /// Returns as Error, or default if not an error variant.
    /// </summary>
    public Error? AsError => _kind == 4 ? _error : default;

    /// <summary>
    /// 从布尔值隐式转换。
    /// Implicitly converts from a bool.
    /// </summary>
    /// <param name="value">要转换的布尔值。The bool value to convert.</param>
    public static implicit operator NavigationGuardNextArgument(bool value)
        => new(value);

    /// <summary>
    /// 从 RouteLocationRaw 隐式转换。
    /// Implicitly converts from a RouteLocationRaw.
    /// </summary>
    /// <param name="value">要转换的路由位置。The route location to convert.</param>
    public static implicit operator NavigationGuardNextArgument(RouteLocationRaw value)
        => new(value);

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
    public static implicit operator NavigationGuardNextArgument(NavigationGuardNextCallback value)
        => new(value);

    /// <summary>
    /// 从 Error 隐式转换。
    /// Implicitly converts from an Error.
    /// </summary>
    /// <param name="value">要转换的错误。The error to convert.</param>
    public static implicit operator NavigationGuardNextArgument(Error value)
        => new(value);

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct NavigationGuardReturn
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly RouteLocationRaw? _location;
    private readonly Error? _error;

    /// <summary>
    /// 从布尔值初始化。
    /// Initializes from a bool value.
    /// </summary>
    /// <param name="value">是否允许导航。Whether to allow navigation.</param>
    private NavigationGuardReturn(bool value)
    {
        _kind = 1;
        _bool = value;
        _location = default;
        _error = default;
    }

    /// <summary>
    /// 从 RouteLocationRaw 初始化。
    /// Initializes from a RouteLocationRaw.
    /// </summary>
    /// <param name="value">要重定向到的路由位置。The route location to redirect to.</param>
    private NavigationGuardReturn(RouteLocationRaw value)
    {
        _kind = 2;
        _bool = default;
        _location = value;
        _error = default;
    }

    /// <summary>
    /// 从 Error 初始化。
    /// Initializes from an Error.
    /// </summary>
    /// <param name="value">要中止导航的错误。The error to abort navigation with.</param>
    private NavigationGuardReturn(Error value)
    {
        _kind = 3;
        _bool = default;
        _location = default;
        _error = value;
    }

    /// <summary>
    /// 以布尔值返回，如果不是布尔变体则返回 default。
    /// Returns as bool, or default if not a bool variant.
    /// </summary>
    public bool? AsBool => _kind == 1 ? _bool : default;

    /// <summary>
    /// 以 RouteLocationRaw 返回，如果不是位置变体则返回 default。
    /// Returns as RouteLocationRaw, or default if not a location variant.
    /// </summary>
    public RouteLocationRaw? AsLocation => _kind == 2 ? _location : default;

    /// <summary>
    /// 以 Error 返回，如果不是错误变体则返回 default。
    /// Returns as Error, or default if not an error variant.
    /// </summary>
    public Error? AsError => _kind == 3 ? _error : default;

    /// <summary>
    /// 从布尔值隐式转换。
    /// Implicitly converts from a bool.
    /// </summary>
    /// <param name="value">要转换的布尔值。The bool value to convert.</param>
    public static implicit operator NavigationGuardReturn(bool value)
        => new(value);

    /// <summary>
    /// 从 RouteLocationRaw 隐式转换。
    /// Implicitly converts from a RouteLocationRaw.
    /// </summary>
    /// <param name="value">要转换的路由位置。The route location to convert.</param>
    public static implicit operator NavigationGuardReturn(RouteLocationRaw value)
        => new(value);

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

    /// <summary>
    /// 从 Error 隐式转换。
    /// Implicitly converts from an Error.
    /// </summary>
    /// <param name="value">要转换的错误。The error to convert.</param>
    public static implicit operator NavigationGuardReturn(Error value)
        => new(value);
}

/// <summary>
/// 路由导航结果联合类型，接受 NavigationFailure。
/// Route navigation result union accepting NavigationFailure.
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteNavigationResult
{
    private readonly byte _kind;
    private readonly NavigationFailure? _failure;

    /// <summary>
    /// 从 NavigationFailure 初始化。
    /// Initializes from a NavigationFailure.
    /// </summary>
    /// <param name="value">导航失败信息。The navigation failure.</param>
    private RouteNavigationResult(NavigationFailure value)
    {
        _kind = 1;
        _failure = value;
    }

    /// <summary>
    /// 以 NavigationFailure 返回，如果不是失败变体则返回 default。
    /// Returns as NavigationFailure, or default if not a failure variant.
    /// </summary>
    public NavigationFailure? AsFailure => _kind == 1 ? _failure : default;

    /// <summary>
    /// 从 NavigationFailure 隐式转换。
    /// Implicitly converts from a NavigationFailure.
    /// </summary>
    /// <param name="value">要转换的导航失败。The navigation failure to convert.</param>
    public static implicit operator RouteNavigationResult(NavigationFailure value)
        => new(value);
}

/// <summary>
/// 导航守卫处理器联合类型，接受同步守卫、异步守卫、遗留同步守卫或遗留异步守卫。
/// Navigation guard handler union accepting sync guard, async guard, legacy sync guard, or legacy async guard.
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct NavigationGuardHandler
{
    private readonly byte _kind;
    private readonly RouteNavigationGuard? _sync;
    private readonly AsyncRouteNavigationGuard? _async;
    private readonly LegacyRouteNavigationGuard? _legacySync;
    private readonly LegacyAsyncRouteNavigationGuard? _legacyAsync;

    /// <summary>
    /// 从同步导航守卫初始化。
    /// Initializes from a synchronous navigation guard.
    /// </summary>
    /// <param name="value">同步导航守卫。The synchronous navigation guard.</param>
    private NavigationGuardHandler(RouteNavigationGuard value)
    {
        _kind = 1;
        _sync = value;
        _async = default;
        _legacySync = default;
        _legacyAsync = default;
    }

    /// <summary>
    /// 从异步导航守卫初始化。
    /// Initializes from an asynchronous navigation guard.
    /// </summary>
    /// <param name="value">异步导航守卫。The asynchronous navigation guard.</param>
    private NavigationGuardHandler(AsyncRouteNavigationGuard value)
    {
        _kind = 2;
        _sync = default;
        _async = value;
        _legacySync = default;
        _legacyAsync = default;
    }

    /// <summary>
    /// 从遗留同步导航守卫初始化。
    /// Initializes from a legacy synchronous navigation guard.
    /// </summary>
    /// <param name="value">遗留同步导航守卫。The legacy synchronous navigation guard.</param>
    private NavigationGuardHandler(LegacyRouteNavigationGuard value)
    {
        _kind = 3;
        _sync = default;
        _async = default;
        _legacySync = value;
        _legacyAsync = default;
    }

    /// <summary>
    /// 从遗留异步导航守卫初始化。
    /// Initializes from a legacy asynchronous navigation guard.
    /// </summary>
    /// <param name="value">遗留异步导航守卫。The legacy asynchronous navigation guard.</param>
    private NavigationGuardHandler(LegacyAsyncRouteNavigationGuard value)
    {
        _kind = 4;
        _sync = default;
        _async = default;
        _legacySync = default;
        _legacyAsync = value;
    }

    /// <summary>
    /// 以同步守卫返回，如果不是同步变体则返回 default。
    /// Returns as synchronous guard, or default if not a sync variant.
    /// </summary>
    public RouteNavigationGuard? AsSync => _kind == 1 ? _sync : default;

    /// <summary>
    /// 以异步守卫返回，如果不是异步变体则返回 default。
    /// Returns as asynchronous guard, or default if not an async variant.
    /// </summary>
    public AsyncRouteNavigationGuard? AsAsync => _kind == 2 ? _async : default;

    /// <summary>
    /// 以遗留同步守卫返回，如果不是遗留同步变体则返回 default。
    /// Returns as legacy synchronous guard, or default if not a legacy sync variant.
    /// </summary>
    public LegacyRouteNavigationGuard? AsLegacySync => _kind == 3 ? _legacySync : default;

    /// <summary>
    /// 以遗留异步守卫返回，如果不是遗留异步变体则返回 default。
    /// Returns as legacy asynchronous guard, or default if not a legacy async variant.
    /// </summary>
    public LegacyAsyncRouteNavigationGuard? AsLegacyAsync => _kind == 4 ? _legacyAsync : default;

    /// <summary>
    /// 从同步导航守卫隐式转换。
    /// Implicitly converts from a synchronous navigation guard.
    /// </summary>
    /// <param name="value">要转换的同步守卫。The sync guard to convert.</param>
    public static implicit operator NavigationGuardHandler(RouteNavigationGuard value)
        => new(value);

    /// <summary>
    /// 从异步导航守卫隐式转换。
    /// Implicitly converts from an asynchronous navigation guard.
    /// </summary>
    /// <param name="value">要转换的异步守卫。The async guard to convert.</param>
    public static implicit operator NavigationGuardHandler(AsyncRouteNavigationGuard value)
        => new(value);

    /// <summary>
    /// 从遗留同步导航守卫隐式转换。
    /// Implicitly converts from a legacy synchronous navigation guard.
    /// </summary>
    /// <param name="value">要转换的遗留同步守卫。The legacy sync guard to convert.</param>
    public static implicit operator NavigationGuardHandler(LegacyRouteNavigationGuard value)
        => new(value);

    /// <summary>
    /// 从遗留异步导航守卫隐式转换。
    /// Implicitly converts from a legacy asynchronous navigation guard.
    /// </summary>
    /// <param name="value">要转换的遗留异步守卫。The legacy async guard to convert.</param>
    public static implicit operator NavigationGuardHandler(LegacyAsyncRouteNavigationGuard value)
        => new(value);

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteRecordBeforeEnter
{
    private readonly byte _kind;
    private readonly NavigationGuardHandler? _guard;
    private readonly NavigationGuardHandler[]? _guards;

    /// <summary>
    /// 从单个导航守卫初始化。
    /// Initializes from a single navigation guard.
    /// </summary>
    /// <param name="value">导航守卫处理器。The navigation guard handler.</param>
    private RouteRecordBeforeEnter(NavigationGuardHandler value)
    {
        _kind = 1;
        _guard = value;
        _guards = default;
    }

    /// <summary>
    /// 从导航守卫数组初始化。
    /// Initializes from an array of navigation guards.
    /// </summary>
    /// <param name="value">导航守卫处理器数组。The array of navigation guard handlers.</param>
    private RouteRecordBeforeEnter(NavigationGuardHandler[] value)
    {
        _kind = 2;
        _guard = default;
        _guards = value;
    }

    /// <summary>
    /// 以单个守卫返回，如果不是单守卫变体则返回 default。
    /// Returns as a single guard, or default if not a single guard variant.
    /// </summary>
    public NavigationGuardHandler? AsGuard => _kind == 1 ? _guard : default;

    /// <summary>
    /// 以守卫数组返回，如果不是数组变体则返回 default。
    /// Returns as an array of guards, or default if not an array variant.
    /// </summary>
    public NavigationGuardHandler[]? AsGuards => _kind == 2 ? _guards : default;

    /// <summary>
    /// 从单个导航守卫隐式转换。
    /// Implicitly converts from a single navigation guard.
    /// </summary>
    /// <param name="value">要转换的守卫。The guard to convert.</param>
    public static implicit operator RouteRecordBeforeEnter(NavigationGuardHandler value)
        => new(value);

    /// <summary>
    /// 从导航守卫数组隐式转换。
    /// Implicitly converts from an array of navigation guards.
    /// </summary>
    /// <param name="value">要转换的守卫数组。The guard array to convert.</param>
    public static implicit operator RouteRecordBeforeEnter(NavigationGuardHandler[] value)
        => new(value);

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteRedirectOption
{
    private readonly byte _kind;
    private readonly RouteLocationRaw? _location;
    private readonly RouteRedirectCallback? _callback;

    /// <summary>
    /// 从 RouteLocationRaw 初始化。
    /// Initializes from a RouteLocationRaw.
    /// </summary>
    /// <param name="value">重定向目标位置。The redirect target location.</param>
    private RouteRedirectOption(RouteLocationRaw value)
    {
        _kind = 1;
        _location = value;
        _callback = default;
    }

    /// <summary>
    /// 从 RouteRedirectCallback 初始化。
    /// Initializes from a RouteRedirectCallback.
    /// </summary>
    /// <param name="value">重定向回调函数。The redirect callback function.</param>
    private RouteRedirectOption(RouteRedirectCallback value)
    {
        _kind = 2;
        _location = default;
        _callback = value;
    }

    /// <summary>
    /// 以 RouteLocationRaw 返回，如果不是位置变体则返回 default。
    /// Returns as RouteLocationRaw, or default if not a location variant.
    /// </summary>
    public RouteLocationRaw? AsLocation => _kind == 1 ? _location : default;

    /// <summary>
    /// 以 RouteRedirectCallback 返回，如果不是回调变体则返回 default。
    /// Returns as RouteRedirectCallback, or default if not a callback variant.
    /// </summary>
    public RouteRedirectCallback? AsCallback => _kind == 2 ? _callback : default;

    /// <summary>
    /// 从 RouteLocationRaw 隐式转换。
    /// Implicitly converts from a RouteLocationRaw.
    /// </summary>
    /// <param name="value">要转换的路由位置。The route location to convert.</param>
    public static implicit operator RouteRedirectOption(RouteLocationRaw value)
        => new(value);

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
    /// 从 RouteRedirectCallback 隐式转换。
    /// Implicitly converts from a RouteRedirectCallback.
    /// </summary>
    /// <param name="value">要转换的重定向回调。The redirect callback to convert.</param>
    public static implicit operator RouteRedirectOption(RouteRedirectCallback value)
        => new(value);

    /// <summary>
    /// 从 RouteRecordRedirectOption 隐式转换，提取位置或回调。
    /// Implicitly converts from RouteRecordRedirectOption, extracting location or callback.
    /// </summary>
    /// <param name="value">要转换的路由记录重定向选项。The route record redirect option to convert.</param>
    public static implicit operator RouteRedirectOption(RouteRecordRedirectOption value)
        => value.AsCallback is RouteRedirectCallback callback
            ? new(callback)
            : value.AsLocation is RouteLocationRaw location
                ? new(location)
                : throw new InvalidOperationException("RouteRecordRedirectOption must contain either a location or a callback.");

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteRecordRedirectOption
{
    private readonly byte _kind;
    private readonly RouteLocationRaw? _location;
    private readonly RouteRedirectCallback? _callback;

    /// <summary>
    /// 从 RouteLocationRaw 初始化。
    /// Initializes from a RouteLocationRaw.
    /// </summary>
    /// <param name="value">重定向目标位置。The redirect target location.</param>
    private RouteRecordRedirectOption(RouteLocationRaw value)
    {
        _kind = 1;
        _location = value;
        _callback = default;
    }

    /// <summary>
    /// 从 RouteRedirectCallback 初始化。
    /// Initializes from a RouteRedirectCallback.
    /// </summary>
    /// <param name="value">重定向回调函数。The redirect callback function.</param>
    private RouteRecordRedirectOption(RouteRedirectCallback value)
    {
        _kind = 2;
        _location = default;
        _callback = value;
    }

    /// <summary>
    /// 以 RouteLocationRaw 返回，如果不是位置变体则返回 default。
    /// Returns as RouteLocationRaw, or default if not a location variant.
    /// </summary>
    public RouteLocationRaw? AsLocation => _kind == 1 ? _location : default;

    /// <summary>
    /// 以 RouteRedirectCallback 返回，如果不是回调变体则返回 default。
    /// Returns as RouteRedirectCallback, or default if not a callback variant.
    /// </summary>
    public RouteRedirectCallback? AsCallback => _kind == 2 ? _callback : default;

    /// <summary>
    /// 从 RouteLocationRaw 隐式转换。
    /// Implicitly converts from a RouteLocationRaw.
    /// </summary>
    /// <param name="value">要转换的路由位置。The route location to convert.</param>
    public static implicit operator RouteRecordRedirectOption(RouteLocationRaw value)
        => new(value);

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
    /// 从 RouteRedirectCallback 隐式转换。
    /// Implicitly converts from a RouteRedirectCallback.
    /// </summary>
    /// <param name="value">要转换的重定向回调。The redirect callback to convert.</param>
    public static implicit operator RouteRecordRedirectOption(RouteRedirectCallback value)
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteRecordRaw
{
    private readonly byte _kind;
    private readonly RouteRecordSingleView? _singleView;
    private readonly RouteRecordSingleViewWithChildren? _singleViewWithChildren;
    private readonly RouteRecordMultipleViews? _multipleViews;
    private readonly RouteRecordMultipleViewsWithChildren? _multipleViewsWithChildren;
    private readonly RouteRecordRedirect? _redirect;

    /// <summary>
    /// 从单视图路由记录初始化。
    /// Initializes from a single view route record.
    /// </summary>
    /// <param name="value">单视图路由记录。The single view route record.</param>
    private RouteRecordRaw(RouteRecordSingleView value)
    {
        _kind = 1;
        _singleView = value;
        _singleViewWithChildren = default;
        _multipleViews = default;
        _multipleViewsWithChildren = default;
        _redirect = default;
    }

    /// <summary>
    /// 从带子路由的单视图路由记录初始化。
    /// Initializes from a single view route record with children.
    /// </summary>
    /// <param name="value">带子路由的单视图路由记录。The single view route record with children.</param>
    private RouteRecordRaw(RouteRecordSingleViewWithChildren value)
    {
        _kind = 2;
        _singleView = default;
        _singleViewWithChildren = value;
        _multipleViews = default;
        _multipleViewsWithChildren = default;
        _redirect = default;
    }

    /// <summary>
    /// 从多视图路由记录初始化。
    /// Initializes from a multiple views route record.
    /// </summary>
    /// <param name="value">多视图路由记录。The multiple views route record.</param>
    private RouteRecordRaw(RouteRecordMultipleViews value)
    {
        _kind = 3;
        _singleView = default;
        _singleViewWithChildren = default;
        _multipleViews = value;
        _multipleViewsWithChildren = default;
        _redirect = default;
    }

    /// <summary>
    /// 从带子路由的多视图路由记录初始化。
    /// Initializes from a multiple views route record with children.
    /// </summary>
    /// <param name="value">带子路由的多视图路由记录。The multiple views route record with children.</param>
    private RouteRecordRaw(RouteRecordMultipleViewsWithChildren value)
    {
        _kind = 4;
        _singleView = default;
        _singleViewWithChildren = default;
        _multipleViews = default;
        _multipleViewsWithChildren = value;
        _redirect = default;
    }

    /// <summary>
    /// 从重定向路由记录初始化。
    /// Initializes from a redirect route record.
    /// </summary>
    /// <param name="value">重定向路由记录。The redirect route record.</param>
    private RouteRecordRaw(RouteRecordRedirect value)
    {
        _kind = 5;
        _singleView = default;
        _singleViewWithChildren = default;
        _multipleViews = default;
        _multipleViewsWithChildren = default;
        _redirect = value;
    }

    /// <summary>
    /// 以单视图路由记录返回，如果不是单视图变体则返回 default。
    /// Returns as single view route record, or default if not a single view variant.
    /// </summary>
    public RouteRecordSingleView? AsSingleView => _kind == 1 ? _singleView : default;

    /// <summary>
    /// 以带子路由的单视图路由记录返回，如果不是该变体则返回 default。
    /// Returns as single view route record with children, or default if not that variant.
    /// </summary>
    public RouteRecordSingleViewWithChildren? AsSingleViewWithChildren => _kind == 2 ? _singleViewWithChildren : default;

    /// <summary>
    /// 以多视图路由记录返回，如果不是多视图变体则返回 default。
    /// Returns as multiple views route record, or default if not a multiple views variant.
    /// </summary>
    public RouteRecordMultipleViews? AsMultipleViews => _kind == 3 ? _multipleViews : default;

    /// <summary>
    /// 以带子路由的多视图路由记录返回，如果不是该变体则返回 default。
    /// Returns as multiple views route record with children, or default if not that variant.
    /// </summary>
    public RouteRecordMultipleViewsWithChildren? AsMultipleViewsWithChildren => _kind == 4 ? _multipleViewsWithChildren : default;

    /// <summary>
    /// 以重定向路由记录返回，如果不是重定向变体则返回 default。
    /// Returns as redirect route record, or default if not a redirect variant.
    /// </summary>
    public RouteRecordRedirect? AsRedirect => _kind == 5 ? _redirect : default;

    /// <summary>
    /// 从单视图路由记录隐式转换。
    /// Implicitly converts from a single view route record.
    /// </summary>
    /// <param name="value">要转换的单视图路由记录。The single view route record to convert.</param>
    public static implicit operator RouteRecordRaw(RouteRecordSingleView value)
        => new(value);

    /// <summary>
    /// 从带子路由的单视图路由记录隐式转换。
    /// Implicitly converts from a single view route record with children.
    /// </summary>
    /// <param name="value">要转换的带子路由的单视图路由记录。The single view with children record to convert.</param>
    public static implicit operator RouteRecordRaw(RouteRecordSingleViewWithChildren value)
        => new(value);

    /// <summary>
    /// 从多视图路由记录隐式转换。
    /// Implicitly converts from a multiple views route record.
    /// </summary>
    /// <param name="value">要转换的多视图路由记录。The multiple views route record to convert.</param>
    public static implicit operator RouteRecordRaw(RouteRecordMultipleViews value)
        => new(value);

    /// <summary>
    /// 从带子路由的多视图路由记录隐式转换。
    /// Implicitly converts from a multiple views route record with children.
    /// </summary>
    /// <param name="value">要转换的带子路由的多视图路由记录。The multiple views with children record to convert.</param>
    public static implicit operator RouteRecordRaw(RouteRecordMultipleViewsWithChildren value)
        => new(value);

    /// <summary>
    /// 从重定向路由记录隐式转换。
    /// Implicitly converts from a redirect route record.
    /// </summary>
    /// <param name="value">要转换的重定向路由记录。The redirect route record to convert.</param>
    public static implicit operator RouteRecordRaw(RouteRecordRedirect value)
        => new(value);
}

/// <summary>
/// 匹配器位置原始值联合类型，接受路径式、命名式或相对式匹配器位置。
/// Matcher location raw union accepting path-based, named, or relative matcher location.
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct MatcherLocationRaw
{
    private readonly byte _kind;
    private readonly MatcherLocationAsPath? _path;
    private readonly MatcherLocationAsName? _named;
    private readonly MatcherLocationAsRelative? _relative;

    /// <summary>
    /// 从路径式匹配器位置初始化。
    /// Initializes from a path-based matcher location.
    /// </summary>
    /// <param name="value">路径式匹配器位置。The path-based matcher location.</param>
    private MatcherLocationRaw(MatcherLocationAsPath value)
    {
        _kind = 1;
        _path = value;
        _named = default;
        _relative = default;
    }

    /// <summary>
    /// 从命名式匹配器位置初始化。
    /// Initializes from a named matcher location.
    /// </summary>
    /// <param name="value">命名式匹配器位置。The named matcher location.</param>
    private MatcherLocationRaw(MatcherLocationAsName value)
    {
        _kind = 2;
        _path = default;
        _named = value;
        _relative = default;
    }

    /// <summary>
    /// 从相对式匹配器位置初始化。
    /// Initializes from a relative matcher location.
    /// </summary>
    /// <param name="value">相对式匹配器位置。The relative matcher location.</param>
    private MatcherLocationRaw(MatcherLocationAsRelative value)
    {
        _kind = 3;
        _path = default;
        _named = default;
        _relative = value;
    }

    /// <summary>
    /// 以路径式匹配器位置返回，如果不是路径变体则返回 default。
    /// Returns as path-based matcher location, or default if not a path variant.
    /// </summary>
    public MatcherLocationAsPath? AsPath => _kind == 1 ? _path : default;

    /// <summary>
    /// 以命名式匹配器位置返回，如果不是命名变体则返回 default。
    /// Returns as named matcher location, or default if not a named variant.
    /// </summary>
    public MatcherLocationAsName? AsNamed => _kind == 2 ? _named : default;

    /// <summary>
    /// 以相对式匹配器位置返回，如果不是相对变体则返回 default。
    /// Returns as relative matcher location, or default if not a relative variant.
    /// </summary>
    public MatcherLocationAsRelative? AsRelative => _kind == 3 ? _relative : default;

    /// <summary>
    /// 从路径式匹配器位置隐式转换。
    /// Implicitly converts from a path-based matcher location.
    /// </summary>
    /// <param name="value">要转换的路径式匹配器位置。The path-based matcher location to convert.</param>
    public static implicit operator MatcherLocationRaw(MatcherLocationAsPath value)
        => new(value);

    /// <summary>
    /// 从命名式匹配器位置隐式转换。
    /// Implicitly converts from a named matcher location.
    /// </summary>
    /// <param name="value">要转换的命名式匹配器位置。The named matcher location to convert.</param>
    public static implicit operator MatcherLocationRaw(MatcherLocationAsName value)
        => new(value);

    /// <summary>
    /// 从相对式匹配器位置隐式转换。
    /// Implicitly converts from a relative matcher location.
    /// </summary>
    /// <param name="value">要转换的相对式匹配器位置。The relative matcher location to convert.</param>
    public static implicit operator MatcherLocationRaw(MatcherLocationAsRelative value)
        => new(value);
}

/// <summary>
/// 路由参数联合类型，接受 string 或 string 数组。
/// Route parameter union accepting string or string array.
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteParam
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly string[]? _strings;

    /// <summary>
    /// 从字符串值初始化。
    /// Initializes from a string value.
    /// </summary>
    /// <param name="value">路由参数值。The route parameter value.</param>
    private RouteParam(string value)
    {
        _kind = 1;
        _string = value;
        _strings = default;
    }

    /// <summary>
    /// 从字符串数组初始化。
    /// Initializes from a string array.
    /// </summary>
    /// <param name="value">路由参数值数组。The route parameter value array.</param>
    private RouteParam(string[] value)
    {
        _kind = 2;
        _string = default;
        _strings = value;
    }

    /// <summary>
    /// 以字符串返回，如果不是字符串变体则返回 default。
    /// Returns as string, or default if not a string variant.
    /// </summary>
    public string? AsString => _kind == 1 ? _string : default;

    /// <summary>
    /// 以字符串数组返回，如果不是数组变体则返回 default。
    /// Returns as string array, or default if not an array variant.
    /// </summary>
    public string[]? AsStrings => _kind == 2 ? _strings : default;

    /// <summary>
    /// 从字符串隐式转换。
    /// Implicitly converts from a string.
    /// </summary>
    /// <param name="value">要转换的字符串值。The string value to convert.</param>
    public static implicit operator RouteParam(string value)
        => new(value);

    /// <summary>
    /// 从字符串数组隐式转换。
    /// Implicitly converts from a string array.
    /// </summary>
    /// <param name="value">要转换的字符串数组。The string array to convert.</param>
    public static implicit operator RouteParam(string[] value)
        => new(value);
}

/// <summary>
/// 路由参数原始值联合类型，接受 string、RouteParamRaw 数组或 Number。
/// Route parameter raw union accepting string, RouteParamRaw array, or Number.
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouteParamRaw
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly Array<RouteParamRaw>? _array;
    private readonly Number? _number;

    /// <summary>
    /// 从字符串值初始化。
    /// Initializes from a string value.
    /// </summary>
    /// <param name="value">路由参数原始字符串值。The raw route parameter string value.</param>
    private RouteParamRaw(string value)
    {
        _kind = 1;
        _string = value;
        _array = default;
        _number = default;
    }

    /// <summary>
    /// 从 RouteParamRaw 数组初始化。
    /// Initializes from an array of RouteParamRaw.
    /// </summary>
    /// <param name="value">路由参数原始值数组。The array of raw route parameters.</param>
    private RouteParamRaw(Array<RouteParamRaw> value)
    {
        _kind = 2;
        _string = default;
        _array = value;
        _number = default;
    }

    /// <summary>
    /// 从 Number 值初始化。
    /// Initializes from a Number value.
    /// </summary>
    /// <param name="value">路由参数数值。The route parameter number value.</param>
    private RouteParamRaw(Number value)
    {
        _kind = 3;
        _string = default;
        _array = default;
        _number = value;
    }

    /// <summary>
    /// 以字符串返回，如果不是字符串变体则返回 default。
    /// Returns as string, or default if not a string variant.
    /// </summary>
    public string? AsString => _kind == 1 ? _string : default;

    /// <summary>
    /// 以数组返回，如果不是数组变体则返回 default。
    /// Returns as array, or default if not an array variant.
    /// </summary>
    public Array<RouteParamRaw>? AsArray => _kind == 2 ? _array : default;

    /// <summary>
    /// 以 Number 返回，如果不是 Number 变体则返回 default。
    /// Returns as Number, or default if not a Number variant.
    /// </summary>
    public Number? AsNumber => _kind == 3 ? _number : default;

    /// <summary>
    /// 从字符串隐式转换。
    /// Implicitly converts from a string.
    /// </summary>
    /// <param name="value">要转换的字符串值。The string value to convert.</param>
    public static implicit operator RouteParamRaw(string value)
        => new(value);

    /// <summary>
    /// 从字符串数组隐式转换，逐项映射为 RouteParamRaw。
    /// Implicitly converts from a string array, mapping each item to RouteParamRaw.
    /// </summary>
    /// <param name="value">要转换的字符串数组。The string array to convert.</param>
    public static implicit operator RouteParamRaw(string[] value)
        => new((Array<RouteParamRaw>)value.Select(static item => (RouteParamRaw)item).ToArray());

    /// <summary>
    /// 从 Number 隐式转换。
    /// Implicitly converts from a Number.
    /// </summary>
    /// <param name="value">要转换的 Number 值。The Number value to convert.</param>
    public static implicit operator RouteParamRaw(Number value)
        => new(value);

    /// <summary>
    /// 从 RouteParamRaw 数组隐式转换。
    /// Implicitly converts from an Array of RouteParamRaw.
    /// </summary>
    /// <param name="value">要转换的 RouteParamRaw 数组。The Array of RouteParamRaw to convert.</param>
    public static implicit operator RouteParamRaw(Array<RouteParamRaw> value)
        => new(value);

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct LocationQueryValue
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly Array<string?>? _array;

    /// <summary>
    /// 从字符串值初始化。
    /// Initializes from a string value.
    /// </summary>
    /// <param name="value">查询字符串值。The query string value.</param>
    private LocationQueryValue(string value)
    {
        _kind = 1;
        _string = value;
        _array = default;
    }

    /// <summary>
    /// 从可空字符串数组初始化。
    /// Initializes from an array of nullable strings.
    /// </summary>
    /// <param name="value">查询字符串数组。The query string array.</param>
    private LocationQueryValue(Array<string?> value)
    {
        _kind = 2;
        _string = default;
        _array = value;
    }

    /// <summary>
    /// 以字符串返回，如果不是字符串变体则返回 default。
    /// Returns as string, or default if not a string variant.
    /// </summary>
    public string? AsString => _kind == 1 ? _string : default;

    /// <summary>
    /// 以可空字符串数组返回，如果不是数组变体则返回 default。
    /// Returns as nullable string array, or default if not an array variant.
    /// </summary>
    public Array<string?>? AsArray => _kind == 2 ? _array : default;

    /// <summary>
    /// 从字符串隐式转换。
    /// Implicitly converts from a string.
    /// </summary>
    /// <param name="value">要转换的字符串值。The string value to convert.</param>
    public static implicit operator LocationQueryValue(string value)
        => new(value);

    /// <summary>
    /// 从可空字符串 CLR 数组隐式转换。
    /// Implicitly converts from a CLR array of nullable strings.
    /// </summary>
    /// <param name="value">要转换的可空字符串数组。The nullable string array to convert.</param>
    public static implicit operator LocationQueryValue(string?[] value)
        => new((Array<string?>)value);

    /// <summary>
    /// 从可空字符串 Array 隐式转换。
    /// Implicitly converts from an Array of nullable strings.
    /// </summary>
    /// <param name="value">要转换的可空字符串 Array。The Array of nullable strings to convert.</param>
    public static implicit operator LocationQueryValue(Array<string?> value)
        => new(value);
}

/// <summary>
/// 位置查询原始值联合类型，接受 string、LocationQueryValueRaw 数组或 Number。
/// Location query raw value union accepting string, LocationQueryValueRaw array, or Number.
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct LocationQueryValueRaw
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly Array<LocationQueryValueRaw?>? _array;
    private readonly Number? _number;

    /// <summary>
    /// 从字符串值初始化。
    /// Initializes from a string value.
    /// </summary>
    /// <param name="value">查询原始字符串值。The raw query string value.</param>
    private LocationQueryValueRaw(string value)
    {
        _kind = 1;
        _string = value;
        _array = default;
        _number = default;
    }

    /// <summary>
    /// 从 LocationQueryValueRaw 数组初始化。
    /// Initializes from an array of LocationQueryValueRaw.
    /// </summary>
    /// <param name="value">查询原始值数组。The array of raw query values.</param>
    private LocationQueryValueRaw(Array<LocationQueryValueRaw?> value)
    {
        _kind = 2;
        _string = default;
        _array = value;
        _number = default;
    }

    /// <summary>
    /// 从 Number 值初始化。
    /// Initializes from a Number value.
    /// </summary>
    /// <param name="value">查询数值。The query number value.</param>
    private LocationQueryValueRaw(Number value)
    {
        _kind = 3;
        _string = default;
        _array = default;
        _number = value;
    }

    /// <summary>
    /// 以字符串返回，如果不是字符串变体则返回 default。
    /// Returns as string, or default if not a string variant.
    /// </summary>
    public string? AsString => _kind == 1 ? _string : default;

    /// <summary>
    /// 以数组返回，如果不是数组变体则返回 default。
    /// Returns as array, or default if not an array variant.
    /// </summary>
    public Array<LocationQueryValueRaw?>? AsArray => _kind == 2 ? _array : default;

    /// <summary>
    /// 以 Number 返回，如果不是 Number 变体则返回 default。
    /// Returns as Number, or default if not a Number variant.
    /// </summary>
    public Number? AsNumber => _kind == 3 ? _number : default;

    /// <summary>
    /// 从字符串隐式转换。
    /// Implicitly converts from a string.
    /// </summary>
    /// <param name="value">要转换的字符串值。The string value to convert.</param>
    public static implicit operator LocationQueryValueRaw(string value)
        => new(value);

    /// <summary>
    /// 从可空字符串数组隐式转换，逐项映射为 LocationQueryValueRaw。
    /// Implicitly converts from a nullable string array, mapping each item to LocationQueryValueRaw.
    /// </summary>
    /// <param name="value">要转换的可空字符串数组。The nullable string array to convert.</param>
    public static implicit operator LocationQueryValueRaw(string?[] value)
        => new((Array<LocationQueryValueRaw?>)value.Select(static item => item is null ? null : (LocationQueryValueRaw?)item).ToArray());

    /// <summary>
    /// 从 Number 隐式转换。
    /// Implicitly converts from a Number.
    /// </summary>
    /// <param name="value">要转换的 Number 值。The Number value to convert.</param>
    public static implicit operator LocationQueryValueRaw(Number value)
        => new(value);

    /// <summary>
    /// 从 LocationQueryValueRaw 数组隐式转换。
    /// Implicitly converts from an Array of LocationQueryValueRaw.
    /// </summary>
    /// <param name="value">要转换的 LocationQueryValueRaw 数组。The Array of LocationQueryValueRaw to convert.</param>
    public static implicit operator LocationQueryValueRaw(Array<LocationQueryValueRaw?> value)
        => new(value);

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
