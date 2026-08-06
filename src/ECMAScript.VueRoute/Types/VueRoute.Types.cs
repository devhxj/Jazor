using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// 路由器选项配置包。
/// Router options configuration bag.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouterOptions : Vue3.VueProps
{
	/// <summary>
	/// 路由器使用的历史实现。
	/// The history implementation used by the router.
	/// </summary>
	[Description("@#history")]
	public RouterHistory History { get; init; } = default!;

	/// <summary>
	/// 初始路由记录列表。
	/// The initial list of route records.
	/// </summary>
	[Description("@#routes")]
	public RouteRecordRaw[] Routes { get; init; } = default!;

	/// <summary>
	/// 是否对路径匹配区分大小写。
	/// Whether path matching should be case-sensitive.
	/// </summary>
	[Description("@#sensitive")]
	public bool? Sensitive { get; init; }

	/// <summary>
	/// 是否允许路径末尾有可选的尾部斜杠。
	/// Whether to allow an optional trailing slash at the end of the path.
	/// </summary>
	[Description("@#strict")]
	public bool? Strict { get; init; }

	/// <summary>
	/// 是否将模式匹配到路径末尾。已弃用，始终为 true。
	/// Whether to match the pattern to the end of the path. Deprecated and always true.
	/// </summary>
	[Description("@#end")]
	[Obsolete("Vue Router 4 documents end as deprecated and always true. Do not author new router options with End.")]
	public bool? End { get; init; }

	/// <summary>
	/// 活跃链接的 CSS 类名。
	/// The CSS class to apply to active links.
	/// </summary>
	[Description("@#linkActiveClass")]
	public string? LinkActiveClass { get; init; }

	/// <summary>
	/// 精确活跃链接的 CSS 类名。
	/// The CSS class to apply to exactly active links.
	/// </summary>
	[Description("@#linkExactActiveClass")]
	public string? LinkExactActiveClass { get; init; }

	/// <summary>
	/// 导航时控制滚动位置的回调。
	/// Callback to control scroll position during navigation.
	/// </summary>
	[Description("@#scrollBehavior")]
	public RouterScrollHandler? ScrollBehavior { get; init; }

	/// <summary>
	/// 自定义查询字符串解析函数。
	/// Custom function to parse a query string.
	/// </summary>
	[Description("@#parseQuery")]
	public RouteQueryParser? ParseQuery { get; init; }

	/// <summary>
	/// 自定义查询字符串序列化函数。
	/// Custom function to stringify a query object.
	/// </summary>
	[Description("@#stringifyQuery")]
	public RouteQueryStringifier? StringifyQuery { get; init; }
}

/// <summary>
/// 可存储在路由元信息中的值的类型包装。
/// Type wrapper for values that can be stored in route metadata.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class RouteMetaValue
{
	private RouteMetaValue()
	{
	}

	/// <summary>
	/// 从字符串值隐式转换为路由元信息值。
	/// Implicit conversion from a string value to a route meta value.
	/// </summary>
	/// <param name="value">要转换的字符串值。The string value to convert.</param>
	public extern static implicit operator RouteMetaValue(string value);

	/// <summary>
	/// 从布尔值隐式转换为路由元信息值。
	/// Implicit conversion from a boolean value to a route meta value.
	/// </summary>
	/// <param name="value">要转换的布尔值。The boolean value to convert.</param>
	public extern static implicit operator RouteMetaValue(bool value);

	/// <summary>
	/// 从 Number 值隐式转换为路由元信息值。
	/// Implicit conversion from a Number value to a route meta value.
	/// </summary>
	/// <param name="value">要转换的 Number 值。The Number value to convert.</param>
	public extern static implicit operator RouteMetaValue(Number value);

	/// <summary>
	/// 从 BigInt 值隐式转换为路由元信息值。
	/// Implicit conversion from a BigInt value to a route meta value.
	/// </summary>
	/// <param name="value">要转换的 BigInt 值。The BigInt value to convert.</param>
	public extern static implicit operator RouteMetaValue(BigInt value);

	/// <summary>
	/// 从 Symbol 值隐式转换为路由元信息值。
	/// Implicit conversion from a Symbol value to a route meta value.
	/// </summary>
	/// <param name="value">要转换的 Symbol 值。The Symbol value to convert.</param>
	public extern static implicit operator RouteMetaValue(Symbol value);

	/// <summary>
	/// 从字符值隐式转换为路由元信息值。
	/// Implicit conversion from a char value to a route meta value.
	/// </summary>
	/// <param name="value">要转换的字符值。The char value to convert.</param>
	public extern static implicit operator RouteMetaValue(char value);

	/// <summary>
	/// 从双精度浮点值隐式转换为路由元信息值。
	/// Implicit conversion from a double value to a route meta value.
	/// </summary>
	/// <param name="value">要转换的双精度浮点值。The double value to convert.</param>
	public extern static implicit operator RouteMetaValue(double value);

	/// <summary>
	/// 从单精度浮点值隐式转换为路由元信息值。
	/// Implicit conversion from a float value to a route meta value.
	/// </summary>
	/// <param name="value">要转换的单精度浮点值。The float value to convert.</param>
	public extern static implicit operator RouteMetaValue(float value);

	/// <summary>
	/// 从 32 位整数值隐式转换为路由元信息值。
	/// Implicit conversion from an int value to a route meta value.
	/// </summary>
	/// <param name="value">要转换的 32 位整数值。The int value to convert.</param>
	public extern static implicit operator RouteMetaValue(int value);

	/// <summary>
	/// 从 64 位整数值隐式转换为路由元信息值。
	/// Implicit conversion from a long value to a route meta value.
	/// </summary>
	/// <param name="value">要转换的 64 位整数值。The long value to convert.</param>
	public extern static implicit operator RouteMetaValue(long value);

	/// <summary>
	/// 从 16 位有符号整数值隐式转换为路由元信息值。
	/// Implicit conversion from a short value to a route meta value.
	/// </summary>
	/// <param name="value">要转换的 16 位有符号整数值。The short value to convert.</param>
	public extern static implicit operator RouteMetaValue(short value);

	/// <summary>
	/// 从 16 位无符号整数值隐式转换为路由元信息值。
	/// Implicit conversion from a ushort value to a route meta value.
	/// </summary>
	/// <param name="value">要转换的 16 位无符号整数值。The ushort value to convert.</param>
	public extern static implicit operator RouteMetaValue(ushort value);

	/// <summary>
	/// 从字节值隐式转换为路由元信息值。
	/// Implicit conversion from a byte value to a route meta value.
	/// </summary>
	/// <param name="value">要转换的字节值。The byte value to convert.</param>
	public extern static implicit operator RouteMetaValue(byte value);

	/// <summary>
	/// 从有符号字节值隐式转换为路由元信息值。
	/// Implicit conversion from an sbyte value to a route meta value.
	/// </summary>
	/// <param name="value">要转换的有符号字节值。The sbyte value to convert.</param>
	public extern static implicit operator RouteMetaValue(sbyte value);

	/// <summary>
	/// 从无符号 32 位整数值隐式转换为路由元信息值。
	/// Implicit conversion from a uint value to a route meta value.
	/// </summary>
	/// <param name="value">要转换的无符号 32 位整数值。The uint value to convert.</param>
	public extern static implicit operator RouteMetaValue(uint value);

	/// <summary>
	/// 从无符号 64 位整数值隐式转换为路由元信息值。
	/// Implicit conversion from a ulong value to a route meta value.
	/// </summary>
	/// <param name="value">要转换的无符号 64 位整数值。The ulong value to convert.</param>
	public extern static implicit operator RouteMetaValue(ulong value);

	/// <summary>
	/// 从十进制值隐式转换为路由元信息值。
	/// Implicit conversion from a decimal value to a route meta value.
	/// </summary>
	/// <param name="value">要转换的十进制值。The decimal value to convert.</param>
	public extern static implicit operator RouteMetaValue(decimal value);

	/// <summary>
	/// 从 Action 委托隐式转换为路由元信息值。
	/// Implicit conversion from an Action delegate to a route meta value.
	/// </summary>
	/// <param name="value">要转换的 Action 委托。The Action delegate to convert.</param>
	public extern static implicit operator RouteMetaValue(Action value);

	/// <summary>
	/// 从 Action 委托创建路由元信息值。
	/// Creates a route meta value from an Action delegate.
	/// </summary>
	/// <param name="value">要包装的 Action 委托。The Action delegate to wrap.</param>
	/// <returns>包装后的路由元信息值。The wrapped route meta value.</returns>
	[ECMAScriptInline("__arg1")]
	public extern static RouteMetaValue From(Action value);

	/// <summary>
	/// 从 Vue 属性对象隐式转换为路由元信息值。
	/// Implicit conversion from a Vue props object to a route meta value.
	/// </summary>
	/// <param name="value">要转换的 Vue 属性对象。The Vue props object to convert.</param>
	public extern static implicit operator RouteMetaValue(Vue3.VueProps value);

	/// <summary>
	/// 从元信息值数组隐式转换为路由元信息值。
	/// Implicit conversion from an array of route meta values.
	/// </summary>
	/// <param name="value">要转换的元信息值数组。The array of route meta values to convert.</param>
	public extern static implicit operator RouteMetaValue(Array<RouteMetaValue?> value);

	/// <summary>
	/// 从可空元信息值数组隐式转换为路由元信息值。
	/// Implicit conversion from a nullable array of route meta values.
	/// </summary>
	/// <param name="value">要转换的可空元信息值数组。The nullable array of route meta values to convert.</param>
	public extern static implicit operator RouteMetaValue(RouteMetaValue?[] value);
}

/// <summary>
/// 路由元信息字典，用于存储附加到路由记录的自定义数据。
/// Route metadata dictionary for storing custom data attached to route records.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouteMeta : Vue3.VueProps, System.Collections.IEnumerable
{
	/// <summary>
	/// 按字符串键获取或设置元信息值。
	/// Gets or sets a meta value by string key.
	/// </summary>
	/// <param name="key">元信息键名。The meta key name.</param>
	public extern RouteMetaValue? this[string key] { get; set; }

	/// <summary>
	/// 按 Number 键获取或设置元信息值。
	/// Gets or sets a meta value by Number key.
	/// </summary>
	/// <param name="key">元信息数字键。The numeric meta key.</param>
	public extern RouteMetaValue? this[Number key] { get; set; }

	/// <summary>
	/// 按 Symbol 键获取或设置元信息值。
	/// Gets or sets a meta value by Symbol key.
	/// </summary>
	/// <param name="key">元信息符号键。The symbol meta key.</param>
	public extern RouteMetaValue? this[Symbol key] { get; set; }

	/// <summary>
	/// 添加具有指定字符串键的元信息值。
	/// Adds a meta value with the specified string key.
	/// </summary>
	/// <param name="key">元信息键名。The meta key name.</param>
	/// <param name="value">要添加的元信息值。The meta value to add.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteMetaValue? value);

	/// <summary>
	/// 添加具有指定字符串键的 Action 值。
	/// Adds an Action value with the specified string key.
	/// </summary>
	/// <param name="key">元信息键名。The meta key name.</param>
	/// <param name="value">要添加的 Action 委托。The Action delegate to add.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, Action value);

	/// <summary>
	/// 添加具有指定 Number 键的元信息值。
	/// Adds a meta value with the specified Number key.
	/// </summary>
	/// <param name="key">元信息数字键。The numeric meta key.</param>
	/// <param name="value">要添加的元信息值。The meta value to add.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(Number key, RouteMetaValue? value);

	/// <summary>
	/// 添加具有指定 Number 键的 Action 值。
	/// Adds an Action value with the specified Number key.
	/// </summary>
	/// <param name="key">元信息数字键。The numeric meta key.</param>
	/// <param name="value">要添加的 Action 委托。The Action delegate to add.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(Number key, Action value);

	/// <summary>
	/// 添加具有指定 Symbol 键的元信息值。
	/// Adds a meta value with the specified Symbol key.
	/// </summary>
	/// <param name="key">元信息符号键。The symbol meta key.</param>
	/// <param name="value">要添加的元信息值。The meta value to add.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(Symbol key, RouteMetaValue? value);

	/// <summary>
	/// 添加具有指定 Symbol 键的 Action 值。
	/// Adds an Action value with the specified Symbol key.
	/// </summary>
	/// <param name="key">元信息符号键。The symbol meta key.</param>
	/// <param name="value">要添加的 Action 委托。The Action delegate to add.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(Symbol key, Action value);

	/// <summary>
	/// 返回遍历元信息条目的枚举器。
	/// Returns an enumerator that iterates through the meta entries.
	/// </summary>
	/// <returns>元信息条目的枚举器。An enumerator for the meta entries.</returns>
	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

/// <summary>
/// 浏览器历史状态条目，用于 history.state 中存储的数据。
/// Browser history state entry for data stored in history.state.
/// </summary>
[ECMAScript]
[Description("@#")]
public record HistoryState : Vue3.VueProps, System.Collections.IEnumerable
{
	/// <summary>
	/// 按字符串键获取或设置历史状态值。
	/// Gets or sets a history state value by string key.
	/// </summary>
	/// <param name="key">状态键名。The state key name.</param>
	public extern HistoryStateValue? this[string key] { get; set; }

	/// <summary>
	/// 按 Number 键获取或设置历史状态值。
	/// Gets or sets a history state value by Number key.
	/// </summary>
	/// <param name="key">状态数字键。The numeric state key.</param>
	public extern HistoryStateValue? this[Number key] { get; set; }

	/// <summary>
	/// 添加具有指定字符串键的历史状态值。
	/// Adds a history state value with the specified string key.
	/// </summary>
	/// <param name="key">状态键名。The state key name.</param>
	/// <param name="value">要添加的历史状态值。The history state value to add.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, HistoryStateValue? value);

	/// <summary>
	/// 添加具有指定 Number 键的历史状态值。
	/// Adds a history state value with the specified Number key.
	/// </summary>
	/// <param name="key">状态数字键。The numeric state key.</param>
	/// <param name="value">要添加的历史状态值。The history state value to add.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(Number key, HistoryStateValue? value);

	/// <summary>
	/// 返回遍历历史状态条目的枚举器。
	/// Returns an enumerator that iterates through the history state entries.
	/// </summary>
	/// <returns>历史状态条目的枚举器。An enumerator for the history state entries.</returns>
	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

/// <summary>
/// 解析后的查询参数字典，值为单个查询值。
/// Parsed query parameters dictionary with single query values.
/// </summary>
[ECMAScript]
[Description("@#")]
public record LocationQuery : Vue3.VueProps, System.Collections.IEnumerable
{
	/// <summary>
	/// 按键名获取或设置查询参数值。
	/// Gets or sets a query parameter value by key name.
	/// </summary>
	/// <param name="key">查询参数键名。The query parameter key name.</param>
	public extern LocationQueryValue? this[string key] { get; set; }

	/// <summary>
	/// 添加具有指定键名的查询参数值。
	/// Adds a query parameter value with the specified key name.
	/// </summary>
	/// <param name="key">查询参数键名。The query parameter key name.</param>
	/// <param name="value">要添加的查询参数值。The query parameter value to add.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, LocationQueryValue? value);

	/// <summary>
	/// 返回遍历查询参数条目的枚举器。
	/// Returns an enumerator that iterates through the query parameter entries.
	/// </summary>
	/// <returns>查询参数条目的枚举器。An enumerator for the query parameter entries.</returns>
	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

/// <summary>
/// 原始查询参数字典，值可以是单个值或值数组。
/// Raw query parameters dictionary where values can be single values or arrays.
/// </summary>
[ECMAScript]
[Description("@#")]
public record LocationQueryRaw : Vue3.VueProps, System.Collections.IEnumerable
{
	/// <summary>
	/// 按字符串键获取或设置原始查询参数值。
	/// Gets or sets a raw query parameter value by string key.
	/// </summary>
	/// <param name="key">查询参数键名。The query parameter key name.</param>
	public extern LocationQueryValueRaw? this[string key] { get; set; }

	/// <summary>
	/// 按 Number 键获取或设置原始查询参数值。
	/// Gets or sets a raw query parameter value by Number key.
	/// </summary>
	/// <param name="key">查询参数数字键。The numeric query parameter key.</param>
	public extern LocationQueryValueRaw? this[Number key] { get; set; }

	/// <summary>
	/// 添加具有指定字符串键的原始查询参数值。
	/// Adds a raw query parameter value with the specified string key.
	/// </summary>
	/// <param name="key">查询参数键名。The query parameter key name.</param>
	/// <param name="value">要添加的原始查询参数值。The raw query parameter value to add.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, LocationQueryValueRaw? value);

	/// <summary>
	/// 添加具有指定 Number 键的原始查询参数值。
	/// Adds a raw query parameter value with the specified Number key.
	/// </summary>
	/// <param name="key">查询参数数字键。The numeric query parameter key.</param>
	/// <param name="value">要添加的原始查询参数值。The raw query parameter value to add.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(Number key, LocationQueryValueRaw? value);

	/// <summary>
	/// 返回遍历原始查询参数条目的枚举器。
	/// Returns an enumerator that iterates through the raw query parameter entries.
	/// </summary>
	/// <returns>原始查询参数条目的枚举器。An enumerator for the raw query parameter entries.</returns>
	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

/// <summary>
/// 解析后的路由参数字典。
/// Parsed route parameters dictionary.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouteParams : Vue3.VueProps, System.Collections.IEnumerable
{
	/// <summary>
	/// 按键名获取或设置路由参数值。
	/// Gets or sets a route parameter value by key name.
	/// </summary>
	/// <param name="key">路由参数键名。The route parameter key name.</param>
	public extern RouteParam? this[string key] { get; set; }

	/// <summary>
	/// 添加具有指定键名的路由参数值。
	/// Adds a route parameter value with the specified key name.
	/// </summary>
	/// <param name="key">路由参数键名。The route parameter key name.</param>
	/// <param name="value">要添加的路由参数值。The route parameter value to add.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteParam value);

	/// <summary>
	/// 返回遍历路由参数条目的枚举器。
	/// Returns an enumerator that iterates through the route parameter entries.
	/// </summary>
	/// <returns>路由参数条目的枚举器。An enumerator for the route parameter entries.</returns>
	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

/// <summary>
/// 原始路由参数字典，参数值可以是单个值或值数组。
/// Raw route parameters dictionary where parameter values can be single values or arrays.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouteParamsRaw : Vue3.VueProps, System.Collections.IEnumerable
{
	/// <summary>
	/// 按键名获取或设置原始路由参数值。
	/// Gets or sets a raw route parameter value by key name.
	/// </summary>
	/// <param name="key">路由参数键名。The route parameter key name.</param>
	public extern RouteParamRaw? this[string key] { get; set; }

	/// <summary>
	/// 添加具有指定键名的原始路由参数值。
	/// Adds a raw route parameter value with the specified key name.
	/// </summary>
	/// <param name="key">路由参数键名。The route parameter key name.</param>
	/// <param name="value">要添加的原始路由参数值。The raw route parameter value to add.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteParamRaw? value);

	/// <summary>
	/// 返回遍历原始路由参数条目的枚举器。
	/// Returns an enumerator that iterates through the raw route parameter entries.
	/// </summary>
	/// <returns>原始路由参数条目的枚举器。An enumerator for the raw route parameter entries.</returns>
	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

/// <summary>
/// 路由位置选项的基类，包含通用导航选项。
/// Base class for route location options containing common navigation options.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract record RouteLocationOptions : Vue3.VueProps
{
	/// <summary>
	/// 是否通过替换当前历史条目进行导航。
	/// Whether to navigate by replacing the current history entry.
	/// </summary>
	[Description("@#replace")]
	public bool? Replace { get; init; }

	/// <summary>
	/// 是否绕过导航守卫强制导航。
	/// Whether to force navigation bypassing navigation guards.
	/// </summary>
	[Description("@#force")]
	public bool? Force { get; init; }

	/// <summary>
	/// 要存储在历史状态中的数据。
	/// Data to store in the history state.
	/// </summary>
	[Description("@#state")]
	public HistoryState? State { get; init; }
}

/// <summary>
/// 路由位置的基类，表示一个已解析或未解析的路由位置。
/// Base class for route locations representing a resolved or unresolved route location.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class RouteLocation
{
	/// <summary>
	/// 初始化 RouteLocation 类的新实例。
	/// Initializes a new instance of the RouteLocation class.
	/// </summary>
	protected RouteLocation()
	{
	}

	/// <summary>
	/// 包含查询和哈希的完整路径。
	/// The full path including query and hash.
	/// </summary>
	[Description("@#fullPath")]
	public extern string FullPath { get; }

	/// <summary>
	/// 路径部分（不包含查询和哈希）。
	/// The path portion (without query and hash).
	/// </summary>
	[Description("@#path")]
	public extern string Path { get; }

	/// <summary>
	/// 解析后的查询参数。
	/// The parsed query parameters.
	/// </summary>
	[Description("@#query")]
	public extern LocationQuery Query { get; }

	/// <summary>
	/// URL 的哈希部分（包含 #）。
	/// The hash portion of the URL (including #).
	/// </summary>
	[Description("@#hash")]
	public extern string Hash { get; }

	/// <summary>
	/// 匹配的路由记录名称。
	/// The name of the matched route record.
	/// </summary>
	[Description("@#name")]
	public extern RouteRecordName? Name { get; }

	/// <summary>
	/// 从路径中提取的路由参数。
	/// The route parameters extracted from the path.
	/// </summary>
	[Description("@#params")]
	public extern RouteParams Params { get; }

	/// <summary>
	/// 附加到匹配路由记录的自定义元数据。
	/// Custom metadata attached to the matched route record.
	/// </summary>
	[Description("@#meta")]
	public extern RouteMeta Meta { get; }

	/// <summary>
	/// 匹配的规范化路由记录数组。
	/// Array of matched normalized route records.
	/// </summary>
	[Description("@#matched")]
	public extern RouteRecordNormalized[] Matched { get; }

	/// <summary>
	/// 发起重定向的原始路由位置。
	/// The original route location that triggered the redirect.
	/// </summary>
	[Description("@#redirectedFrom")]
	public extern RouteLocation? RedirectedFrom { get; }

	/// <summary>
	/// 是否通过替换当前历史条目进行导航。
	/// Whether to navigate by replacing the current history entry.
	/// </summary>
	[Description("@#replace")]
	public extern bool? Replace { get; }

	/// <summary>
	/// 是否绕过导航守卫强制导航。
	/// Whether to force navigation bypassing navigation guards.
	/// </summary>
	[Description("@#force")]
	public extern bool? Force { get; }

	/// <summary>
	/// 与此位置关联的历史状态。
	/// The history state associated with this location.
	/// </summary>
	[Description("@#state")]
	public extern HistoryState? State { get; }
}

/// <summary>
/// 基于路径的原始路由位置的基类，包含查询和哈希。
/// Base class for path-based raw route locations including query and hash.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract record RouteLocationPathRawBase : RouteLocationOptions
{
	/// <summary>
	/// 原始查询参数。
	/// The raw query parameters.
	/// </summary>
	[Description("@#query")]
	public LocationQueryRaw? Query { get; init; }

	/// <summary>
	/// URL 的哈希部分。
	/// The hash portion of the URL.
	/// </summary>
	[Description("@#hash")]
	public string? Hash { get; init; }
}

/// <summary>
/// 基于路径字符串的路由位置（非泛型版本）。
/// Path-string-based route location (non-generic version).
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouteLocationAsPath : RouteLocationPathRawBase
{
	/// <summary>
	/// 路径字符串。
	/// The path string.
	/// </summary>
	[Description("@#path")]
	public string Path { get; init; } = default!;
}

/// <summary>
/// 包含查询和哈希的路由位置基类。
/// Base class for route locations including query and hash.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract record RouteQueryAndHash : Vue3.VueProps
{
	/// <summary>
	/// 原始查询参数。
	/// The raw query parameters.
	/// </summary>
	[Description("@#query")]
	public LocationQueryRaw? Query { get; init; }

	/// <summary>
	/// URL 的哈希部分。
	/// The hash portion of the URL.
	/// </summary>
	[Description("@#hash")]
	public string? Hash { get; init; }
}

/// <summary>
/// 基于路径字符串的原始路由位置。
/// Path-string-based raw route location.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouteLocationPathRaw : RouteLocationPathRawBase
{
	/// <summary>
	/// 路径字符串。
	/// The path string.
	/// </summary>
	[Description("@#path")]
	public string Path { get; init; } = default!;
}

/// <summary>
/// 基于名称的相对原始路由位置的基类。
/// Base class for name-based relative raw route locations.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract record LocationAsRelativeRaw : RouteLocationOptions
{
	/// <summary>
	/// 要导航到的路由记录名称。
	/// The name of the route record to navigate to.
	/// </summary>
	[Description("@#name")]
	public RouteRecordName? Name { get; init; }

	/// <summary>
	/// 要传递的路由参数。
	/// The route parameters to pass.
	/// </summary>
	[Description("@#params")]
	public RouteParamsRaw? Params { get; init; }

	/// <summary>
	/// 原始查询参数。
	/// The raw query parameters.
	/// </summary>
	[Description("@#query")]
	public LocationQueryRaw? Query { get; init; }

	/// <summary>
	/// URL 的哈希部分。
	/// The hash portion of the URL.
	/// </summary>
	[Description("@#hash")]
	public string? Hash { get; init; }

}

/// <summary>
/// 基于名称的相对路由位置（非泛型版本）。
/// Name-based relative route location (non-generic version).
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouteLocationAsRelative : LocationAsRelativeRaw
{
}

/// <summary>
/// 基于名称的原始路由位置。
/// Name-based raw route location.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouteLocationNamedRaw : LocationAsRelativeRaw
{
}

/// <summary>
/// 路径解析器选项，控制路径令牌的匹配行为。
/// Path parser options controlling how path tokens are matched.
/// </summary>
[ECMAScript]
[Description("@#")]
public record PathParserOptions : Vue3.VueProps
{
	/// <summary>
	/// 是否对路径匹配区分大小写。
	/// Whether path matching should be case-sensitive.
	/// </summary>
	[Description("@#sensitive")]
	public bool? Sensitive { get; init; }

	/// <summary>
	/// 是否不允许尾部可选的分隔符。
	/// Whether to disallow optional trailing delimiters.
	/// </summary>
	[Description("@#strict")]
	public bool? Strict { get; init; }

	/// <summary>
	/// 是否将模式匹配到路径末尾。已弃用，始终为 true。
	/// Whether to match the pattern to the end of the path. Deprecated and always true.
	/// </summary>
	[Description("@#end")]
	[Obsolete("Vue Router 4 documents end as deprecated and always true. Do not author new path parser options with End.")]
	public bool? End { get; init; }
}

/// <summary>
/// 表示路径解析器中提取的路径参数键。
/// Represents a path parameter key extracted by the path parser.
/// </summary>
[ECMAScript]
[Description("@#")]
public record PathParserKey : Vue3.VueProps
{
	/// <summary>
	/// 参数名称。
	/// The parameter name.
	/// </summary>
	[Description("@#name")]
	public string Name { get; init; } = default!;

	/// <summary>
	/// 参数是否可重复（即数组参数）。
	/// Whether the parameter is repeatable (i.e. an array parameter).
	/// </summary>
	[Description("@#repeatable")]
	public bool Repeatable { get; init; }

	/// <summary>
	/// 参数是否可选。
	/// Whether the parameter is optional.
	/// </summary>
	[Description("@#optional")]
	public bool Optional { get; init; }
}

/// <summary>
/// 路径解析器的抽象基类，用于将路径字符串与路由模式进行匹配。
/// Abstract base class for path parsers that match path strings against route patterns.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class PathParser
{
	/// <summary>
	/// 初始化 PathParser 类的新实例。
	/// Initializes a new instance of the PathParser class.
	/// </summary>
	protected PathParser()
	{
	}

	/// <summary>
	/// 用于匹配路径的正则表达式。
	/// The regular expression used to match paths.
	/// </summary>
	[Description("@#re")]
	public extern RegExp Re { get; }

	/// <summary>
	/// 路由匹配器的评分数组。
	/// The score array of the route matcher.
	/// </summary>
	[Description("@#score")]
	public extern Array<Array<Number>> Score { get; }

	/// <summary>
	/// 从路径模式中提取的参数键。
	/// The parameter keys extracted from the path pattern.
	/// </summary>
	[Description("@#keys")]
	public extern PathParserKey[] Keys { get; }

	/// <summary>
	/// 将路径字符串解析为路由参数。
	/// Parses a path string into route parameters.
	/// </summary>
	/// <param name="path">要解析的路径字符串。The path string to parse.</param>
	/// <returns>解析后的路由参数，如果路径不匹配则为 null。The parsed route parameters, or null if the path does not match.</returns>
	[Description("@#parse")]
	public extern RouteParams? Parse(string path);

	/// <summary>
	/// 将路由参数序列化为路径字符串。
	/// Stringifies route parameters into a path string.
	/// </summary>
	/// <param name="routeParams">要序列化的路由参数。The route parameters to stringify.</param>
	/// <returns>序列化后的路径字符串。The stringified path string.</returns>
	[Description("@#stringify")]
	public extern string Stringify(RouteParams routeParams);
}

/// <summary>
/// 基于路径的匹配器位置。
/// Path-based matcher location.
/// </summary>
[ECMAScript]
[Description("@#")]
public record MatcherLocationAsPath : Vue3.VueProps
{
	/// <summary>
	/// 路径字符串。
	/// The path string.
	/// </summary>
	[Description("@#path")]
	public string Path { get; init; } = default!;
}

/// <summary>
/// 基于名称的相对匹配器位置。
/// Name-based relative matcher location.
/// </summary>
[ECMAScript]
[Description("@#")]
public record MatcherLocationAsRelative : Vue3.VueProps
{
	/// <summary>
	/// 路由参数。
	/// The route parameters.
	/// </summary>
	[Description("@#params")]
	public RouteParams? Params { get; init; }
}

/// <summary>
/// 具名匹配器位置，通过路由名称定位。
/// Named matcher location, identified by route name.
/// </summary>
[ECMAScript]
[Description("@#")]
public record MatcherLocationAsName : Vue3.VueProps
{
	/// <summary>
	/// 路由记录名称。
	/// The route record name.
	/// </summary>
	[Description("@#name")]
	public RouteRecordName Name { get; init; } = default!;

	/// <summary>
	/// 路由参数。
	/// The route parameters.
	/// </summary>
	[Description("@#params")]
	public RouteParams? Params { get; init; }
}

/// <summary>
/// 匹配器解析后的路由位置。
/// Matcher-resolved route location.
/// </summary>
[ECMAScript]
[Description("@#")]
public record MatcherLocation : Vue3.VueProps
{
	/// <summary>
	/// 路由记录名称。
	/// The route record name.
	/// </summary>
	[Description("@#name")]
	public RouteRecordName? Name { get; init; }

	/// <summary>
	/// 路径字符串。
	/// The path string.
	/// </summary>
	[Description("@#path")]
	public string Path { get; init; } = default!;

	/// <summary>
	/// 解析后的路由参数。
	/// The resolved route parameters.
	/// </summary>
	[Description("@#params")]
	public RouteParams Params { get; init; } = default!;

	/// <summary>
	/// 路由元数据。
	/// The route metadata.
	/// </summary>
	[Description("@#meta")]
	public RouteMeta Meta { get; init; } = default!;

	/// <summary>
	/// 匹配的规范化路由记录数组。
	/// Array of matched normalized route records.
	/// </summary>
	[Description("@#matched")]
	public RouteRecordNormalized[] Matched { get; init; } = default!;
}

/// <summary>
/// 规范化后的路由记录，包含所有解析后的信息。
/// Normalized route record containing all resolved information.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class RouteRecordNormalized
{
	/// <summary>
	/// 初始化 RouteRecordNormalized 类的新实例。
	/// Initializes a new instance of the RouteRecordNormalized class.
	/// </summary>
	protected RouteRecordNormalized()
	{
	}

	/// <summary>
	/// 路由路径模式。
	/// The route path pattern.
	/// </summary>
	[Description("@#path")]
	public extern string Path { get; }

	/// <summary>
	/// 路由记录名称。
	/// The route record name.
	/// </summary>
	[Description("@#name")]
	public extern RouteRecordName? Name { get; }

	/// <summary>
	/// 附加到此路由的自定义元数据。
	/// Custom metadata attached to this route.
	/// </summary>
	[Description("@#meta")]
	public extern RouteMeta Meta { get; }

	/// <summary>
	/// 重定向目标。
	/// The redirect target.
	/// </summary>
	[Description("@#redirect")]
	public extern RouteRedirectOption? Redirect { get; }

	/// <summary>
	/// 路由组件映射。
	/// The route components mapping.
	/// </summary>
	[Description("@#components")]
	public extern RawRouteComponents? Components { get; }

	/// <summary>
	/// 嵌套的子路由记录。
	/// Nested child route records.
	/// </summary>
	[Description("@#children")]
	public extern RouteRecordRaw[] Children { get; }

	/// <summary>
	/// 路由组件的属性配置。
	/// The props configuration for route components.
	/// </summary>
	[Description("@#props")]
	public extern RouteNamedProps Props { get; }

	/// <summary>
	/// 进入路由前执行的导航守卫。
	/// Navigation guard executed before entering the route.
	/// </summary>
	[Description("@#beforeEnter")]
	public extern RouteRecordBeforeEnter? BeforeEnter { get; }

	/// <summary>
	/// 离开路由时执行的守卫集合。
	/// Set of guards executed when leaving the route.
	/// </summary>
	[Description("@#leaveGuards")]
	public extern Set<NavigationGuardHandler> LeaveGuards { get; }

	/// <summary>
	/// 路由更新时执行的守卫集合。
	/// Set of guards executed when the route is updated.
	/// </summary>
	[Description("@#updateGuards")]
	public extern Set<NavigationGuardHandler> UpdateGuards { get; }

	/// <summary>
	/// 进入路由后的回调映射。
	/// Map of callbacks invoked after entering the route.
	/// </summary>
	[Description("@#enterCallbacks")]
	public extern NavigationGuardNextCallbackMap EnterCallbacks { get; }

	/// <summary>
	/// 路由组件实例映射。
	/// Map of route component instances.
	/// </summary>
	[Description("@#instances")]
	public extern RouteComponentInstanceMap Instances { get; }

	/// <summary>
	/// 此路由别名所指向的原始路由记录。
	/// The original route record this alias points to.
	/// </summary>
	[Description("@#aliasOf")]
	public extern RouteRecordNormalized? AliasOf { get; }

	/// <summary>
	/// 路由记录的效应作用域。
	/// The effect scope of the route record.
	/// </summary>
	[Description("@#mods")]
	public extern Vue3.VueDictionary Mods { get; }
}

/// <summary>
/// 匹配到的路由位置记录，扩展了规范化记录并包含已解析的组件。
/// Matched route location record extending the normalized record with resolved components.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class RouteLocationMatched : RouteRecordNormalized
{
	/// <summary>
	/// 初始化 RouteLocationMatched 类的新实例。
	/// Initializes a new instance of the RouteLocationMatched class.
	/// </summary>
	protected RouteLocationMatched()
	{
	}

	/// <summary>
	/// 已解析的路由组件映射。
	/// The resolved route components mapping.
	/// </summary>
	[Description("@#components")]
	public extern new RouteComponents? Components { get; }
}

/// <summary>
/// 路由记录匹配器，扩展了路径解析器并包含匹配器树结构信息。
/// Route record matcher extending the path parser with matcher tree structure information.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class RouteRecordMatcher : PathParser
{
	/// <summary>
	/// 初始化 RouteRecordMatcher 类的新实例。
	/// Initializes a new instance of the RouteRecordMatcher class.
	/// </summary>
	protected RouteRecordMatcher()
	{
	}

	/// <summary>
	/// 关联的规范化路由记录。
	/// The associated normalized route record.
	/// </summary>
	[Description("@#record")]
	public extern RouteRecordNormalized Record { get; }

	/// <summary>
	/// 父级匹配器。
	/// The parent matcher.
	/// </summary>
	[Description("@#parent")]
	public extern RouteRecordMatcher? Parent { get; }

	/// <summary>
	/// 子匹配器数组。
	/// Array of child matchers.
	/// </summary>
	[Description("@#children")]
	public extern RouteRecordMatcher[] Children { get; }

	/// <summary>
	/// 此匹配器的别名匹配器数组。
	/// Array of alias matchers for this matcher.
	/// </summary>
	[Description("@#alias")]
	public extern RouteRecordMatcher[] Alias { get; }
}

/// <summary>
/// 规范化后的路由位置，包含所有解析后的路由信息。
/// Normalized route location containing all resolved route information.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class RouteLocationNormalized
{
	/// <summary>
	/// 初始化 RouteLocationNormalized 类的新实例。
	/// Initializes a new instance of the RouteLocationNormalized class.
	/// </summary>
	protected RouteLocationNormalized()
	{
	}

	/// <summary>
	/// 包含查询和哈希的完整路径。
	/// The full path including query and hash.
	/// </summary>
	[Description("@#fullPath")]
	public extern string FullPath { get; }

	/// <summary>
	/// 路径部分。
	/// The path portion.
	/// </summary>
	[Description("@#path")]
	public extern string Path { get; }

	/// <summary>
	/// 解析后的查询参数。
	/// The parsed query parameters.
	/// </summary>
	[Description("@#query")]
	public extern LocationQuery Query { get; }

	/// <summary>
	/// URL 的哈希部分。
	/// The hash portion of the URL.
	/// </summary>
	[Description("@#hash")]
	public extern string Hash { get; }

	/// <summary>
	/// 匹配的路由记录名称。
	/// The name of the matched route record.
	/// </summary>
	[Description("@#name")]
	public extern RouteRecordName? Name { get; }

	/// <summary>
	/// 解析后的路由参数。
	/// The resolved route parameters.
	/// </summary>
	[Description("@#params")]
	public extern RouteParams Params { get; }

	/// <summary>
	/// 路由元数据。
	/// The route metadata.
	/// </summary>
	[Description("@#meta")]
	public extern RouteMeta Meta { get; }

	/// <summary>
	/// 匹配的规范化路由记录数组。
	/// Array of matched normalized route records.
	/// </summary>
	[Description("@#matched")]
	public extern RouteRecordNormalized[] Matched { get; }

	/// <summary>
	/// 发起重定向的原始路由位置。
	/// The original route location that triggered the redirect.
	/// </summary>
	[Description("@#redirectedFrom")]
	public extern RouteLocation? RedirectedFrom { get; }
}

/// <summary>
/// 已加载组件的规范化路由位置，包含已解析的匹配记录。
/// Normalized route location with loaded components, containing resolved matched records.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class RouteLocationNormalizedLoaded : RouteLocationNormalized
{
	/// <summary>
	/// 初始化 RouteLocationNormalizedLoaded 类的新实例。
	/// Initializes a new instance of the RouteLocationNormalizedLoaded class.
	/// </summary>
	protected RouteLocationNormalizedLoaded()
	{
	}

	/// <summary>
	/// 已加载的匹配路由记录数组。
	/// Array of loaded matched route records.
	/// </summary>
	[Description("@#matched")]
	public extern new RouteLocationMatched[] Matched { get; }
}

/// <summary>
/// 已解析的路由位置，包含可用于生成链接的 href。
/// Resolved route location including the href that can be used to generate links.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class RouteLocationResolved : RouteLocation
{
	/// <summary>
	/// 初始化 RouteLocationResolved 类的新实例。
	/// Initializes a new instance of the RouteLocationResolved class.
	/// </summary>
	protected RouteLocationResolved()
	{
	}

	/// <summary>
	/// 此路由位置的完整 URL 路径。
	/// The full URL path for this route location.
	/// </summary>
	[Description("@#href")]
	public extern string Href { get; }
}

/// <summary>
/// 路由器历史导航信息，描述导航的类型和方向。
/// Router history navigation information describing the type and direction of a navigation.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class RouterHistoryNavigationInformation
{
	/// <summary>
	/// 初始化 RouterHistoryNavigationInformation 类的新实例。
	/// Initializes a new instance of the RouterHistoryNavigationInformation class.
	/// </summary>
	protected RouterHistoryNavigationInformation()
	{
	}

	/// <summary>
	/// 导航类型（如 push、replace、pop）。
	/// The navigation type (e.g. push, replace, pop).
	/// </summary>
	[Description("@#type")]
	public extern RouterHistoryNavigationType Type { get; }

	/// <summary>
	/// 导航方向（前进、后退或未知）。
	/// The navigation direction (forward, back, or unknown).
	/// </summary>
	[Description("@#direction")]
	public extern RouterHistoryNavigationDirection Direction { get; }

	/// <summary>
	/// 导航的步进增量。
	/// The navigation delta step.
	/// </summary>
	[Description("@#delta")]
	public extern Number Delta { get; }
}

/// <summary>
/// 路由器历史管理的抽象基类，封装了浏览器历史 API。
/// Abstract base class for router history management, wrapping the browser history API.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class RouterHistory
{
	/// <summary>
	/// 初始化 RouterHistory 类的新实例。
	/// Initializes a new instance of the RouterHistory class.
	/// </summary>
	protected RouterHistory()
	{
	}

	/// <summary>
	/// 历史管理的基础路径。
	/// The base path for history management.
	/// </summary>
	[Description("@#base")]
	public extern string Base { get; }

	/// <summary>
	/// 当前位置的路径。
	/// The path of the current location.
	/// </summary>
	[Description("@#location")]
	public extern string Location { get; }

	/// <summary>
	/// 当前的历史状态。
	/// The current history state.
	/// </summary>
	[Description("@#state")]
	public extern HistoryState State { get; }

	/// <summary>
	/// 向历史堆栈中推入一个新条目。
	/// Pushes a new entry onto the history stack.
	/// </summary>
	/// <param name="to">目标路径。The target path.</param>
	[Description("@#push")]
	public extern void Push(string to);

	/// <summary>
	/// 向历史堆栈中推入一个新条目并附带状态数据。
	/// Pushes a new entry onto the history stack with associated state data.
	/// </summary>
	/// <param name="to">目标路径。The target path.</param>
	/// <param name="data">要存储的历史状态数据。The history state data to store.</param>
	[Description("@#push")]
	public extern void Push(string to, HistoryState? data);

	/// <summary>
	/// 替换当前历史条目。
	/// Replaces the current history entry.
	/// </summary>
	/// <param name="to">目标路径。The target path.</param>
	[Description("@#replace")]
	public extern void Replace(string to);

	/// <summary>
	/// 替换当前历史条目并附带状态数据。
	/// Replaces the current history entry with associated state data.
	/// </summary>
	/// <param name="to">目标路径。The target path.</param>
	/// <param name="data">要存储的历史状态数据。The history state data to store.</param>
	[Description("@#replace")]
	public extern void Replace(string to, HistoryState? data);

	/// <summary>
	/// 注册历史导航回调监听器。
	/// Registers a history navigation callback listener.
	/// </summary>
	/// <param name="callback">导航回调函数。The navigation callback.</param>
	/// <returns>用于取消监听的函数。A function to remove the listener.</returns>
	[Description("@#listen")]
	public extern Action Listen(RouterHistoryNavigationCallback callback);

	/// <summary>
	/// 根据位置创建完整的 href 字符串。
	/// Creates a full href string from a location.
	/// </summary>
	/// <param name="location">位置字符串。The location string.</param>
	/// <returns>完整的 href 字符串。The full href string.</returns>
	[Description("@#createHref")]
	public extern string CreateHref(string location);

	/// <summary>
	/// 在历史堆栈中前进或后退指定步数。
	/// Moves forward or backward through the history stack by the specified delta.
	/// </summary>
	/// <param name="delta">步进增量，正数前进，负数后退。The delta to move (positive for forward, negative for backward).</param>
	[Description("@#go")]
	public extern void Go(Number delta);

	/// <summary>
	/// 在历史堆栈中前进或后退指定步数，可选择是否触发监听器。
	/// Moves forward or backward through the history stack, optionally triggering listeners.
	/// </summary>
	/// <param name="delta">步进增量。The delta to move.</param>
	/// <param name="triggerListeners">是否触发监听器。Whether to trigger listeners.</param>
	[Description("@#go")]
	public extern void Go(Number delta, bool triggerListeners);

	/// <summary>
	/// 销毁历史实例并清理监听器。
	/// Destroys the history instance and cleans up listeners.
	/// </summary>
	[Description("@#destroy")]
	public extern void Destroy();
}

/// <summary>
/// Vue Router 实例，提供路由导航和状态管理的核心 API。
/// Vue Router instance providing the core API for route navigation and state management.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract record Router : Vue3.VuePlugin
{
	/// <summary>
	/// 当前路由位置的浅响应式引用。
	/// Shallow reactive reference to the current route location.
	/// </summary>
	[Description("@#currentRoute")]
	public extern Vue3.VueShallowRef<RouteLocationNormalizedLoaded> CurrentRoute { get; }

	/// <summary>
	/// 路由器是否正在监听历史变化。
	/// Whether the router is listening to history changes.
	/// </summary>
	[Description("@#listening")]
	public extern bool Listening { get; set; }

	/// <summary>
	/// 路由器的初始选项配置。
	/// The initial options configuration of the router.
	/// </summary>
	[Description("@#options")]
	public extern RouterOptions Options { get; }

	/// <summary>
	/// 添加一条新的路由记录。
	/// Adds a new route record.
	/// </summary>
	/// <param name="route">要添加的路由记录。The route record to add.</param>
	/// <returns>用于移除此路由的函数。A function to remove this route.</returns>
	[Description("@#addRoute")]
	public extern Action AddRoute(RouteRecordRaw route);

	/// <summary>
	/// 在指定父路由下添加一条新的路由记录。
	/// Adds a new route record under the specified parent route.
	/// </summary>
	/// <param name="parentName">父路由的名称。The name of the parent route.</param>
	/// <param name="route">要添加的路由记录。The route record to add.</param>
	/// <returns>用于移除此路由的函数。A function to remove this route.</returns>
	[Description("@#addRoute")]
	public extern Action AddRoute(RouteRecordName parentName, RouteRecordRaw route);

	/// <summary>
	/// 移除指定名称的路由记录。
	/// Removes the route record with the specified name.
	/// </summary>
	/// <param name="routeName">要移除的路由名称。The name of the route to remove.</param>
	[Description("@#removeRoute")]
	public extern void RemoveRoute(RouteRecordName routeName);

	/// <summary>
	/// 检查指定名称的路由是否存在。
	/// Checks whether a route with the specified name exists.
	/// </summary>
	/// <param name="routeName">要检查的路由名称。The name of the route to check.</param>
	/// <returns>路由是否存在。Whether the route exists.</returns>
	[Description("@#hasRoute")]
	public extern bool HasRoute(RouteRecordName routeName);

	/// <summary>
	/// 获取所有已注册的路由记录。
	/// Gets all registered route records.
	/// </summary>
	/// <returns>规范化后的路由记录数组。Array of normalized route records.</returns>
	[Description("@#getRoutes")]
	public extern RouteRecordNormalized[] GetRoutes();

	/// <summary>
	/// 移除所有已注册的路由记录。
	/// Removes all registered route records.
	/// </summary>
	[Description("@#clearRoutes")]
	public extern void ClearRoutes();

	/// <summary>
	/// 解析路由位置为包含 href 的完整路由信息。
	/// Resolves a route location to full route information including href.
	/// </summary>
	/// <param name="to">目标路由位置。The target route location.</param>
	/// <returns>解析后的路由位置。The resolved route location.</returns>
	[Description("@#resolve")]
	public extern RouteLocationResolved Resolve(RouteLocationRaw to);

	/// <summary>
	/// 以指定当前位置为上下文解析路由位置。
	/// Resolves a route location with the specified current location as context.
	/// </summary>
	/// <param name="to">目标路由位置。The target route location.</param>
	/// <param name="currentLocation">当前路由位置。The current route location.</param>
	/// <returns>解析后的路由位置。The resolved route location.</returns>
	[Description("@#resolve")]
	public extern RouteLocationResolved Resolve(RouteLocationRaw to, RouteLocationNormalizedLoaded currentLocation);

	/// <summary>
	/// 通过 push 方式导航到新的路由位置。
	/// Programmatically navigates to a new route by pushing onto the history stack.
	/// </summary>
	/// <param name="to">目标路由位置。The target route location.</param>
	/// <returns>导航结果的 Promise。A promise resolving to the navigation result.</returns>
	[Description("@#push")]
	public extern IPromise<RouteNavigationResult?> Push(RouteLocationRaw to);

	/// <summary>
	/// 通过 replace 方式导航到新的路由位置。
	/// Programmatically navigates to a new route by replacing the current history entry.
	/// </summary>
	/// <param name="to">目标路由位置。The target route location.</param>
	/// <returns>导航结果的 Promise。A promise resolving to the navigation result.</returns>
	[Description("@#replace")]
	public extern IPromise<RouteNavigationResult?> Replace(RouteLocationRaw to);

	/// <summary>
	/// 在历史堆栈中前进或后退指定步数。
	/// Moves forward or backward through the history stack by the specified delta.
	/// </summary>
	/// <param name="delta">步进增量。The delta to move.</param>
	[Description("@#go")]
	public extern void Go(Number delta);

	/// <summary>
	/// 后退到上一个历史条目。
	/// Goes back to the previous history entry.
	/// </summary>
	[Description("@#back")]
	public extern void Back();

	/// <summary>
	/// 前进到下一个历史条目。
	/// Goes forward to the next history entry.
	/// </summary>
	[Description("@#forward")]
	public extern void Forward();

	/// <summary>
	/// 注册全局前置导航守卫。
	/// Registers a global before-each navigation guard.
	/// </summary>
	/// <param name="guard">同步导航守卫函数。The synchronous navigation guard function.</param>
	/// <returns>用于移除此守卫的函数。A function to remove this guard.</returns>
	[Description("@#beforeEach")]
	public extern Action BeforeEach(RouteNavigationGuard guard);

	/// <summary>
	/// 注册全局前置异步导航守卫。
	/// Registers a global async before-each navigation guard.
	/// </summary>
	/// <param name="guard">异步导航守卫函数。The asynchronous navigation guard function.</param>
	/// <returns>用于移除此守卫的函数。A function to remove this guard.</returns>
	[Description("@#beforeEach")]
	public extern Action BeforeEach(AsyncRouteNavigationGuard guard);

	/// <summary>
	/// 注册全局前置遗留导航守卫。
	/// Registers a global legacy before-each navigation guard.
	/// </summary>
	/// <param name="guard">遗留同步导航守卫函数。The legacy synchronous navigation guard function.</param>
	/// <returns>用于移除此守卫的函数。A function to remove this guard.</returns>
	[Description("@#beforeEach")]
	public extern Action BeforeEach(LegacyRouteNavigationGuard guard);

	/// <summary>
	/// 注册全局前置遗留异步导航守卫。
	/// Registers a global legacy async before-each navigation guard.
	/// </summary>
	/// <param name="guard">遗留异步导航守卫函数。The legacy asynchronous navigation guard function.</param>
	/// <returns>用于移除此守卫的函数。A function to remove this guard.</returns>
	[Description("@#beforeEach")]
	public extern Action BeforeEach(LegacyAsyncRouteNavigationGuard guard);

	/// <summary>
	/// 注册全局解析前置导航守卫。
	/// Registers a global before-resolve navigation guard.
	/// </summary>
	/// <param name="guard">同步导航守卫函数。The synchronous navigation guard function.</param>
	/// <returns>用于移除此守卫的函数。A function to remove this guard.</returns>
	[Description("@#beforeResolve")]
	public extern Action BeforeResolve(RouteNavigationGuard guard);

	/// <summary>
	/// 注册全局解析前置异步导航守卫。
	/// Registers a global async before-resolve navigation guard.
	/// </summary>
	/// <param name="guard">异步导航守卫函数。The asynchronous navigation guard function.</param>
	/// <returns>用于移除此守卫的函数。A function to remove this guard.</returns>
	[Description("@#beforeResolve")]
	public extern Action BeforeResolve(AsyncRouteNavigationGuard guard);

	/// <summary>
	/// 注册全局解析前置遗留导航守卫。
	/// Registers a global legacy before-resolve navigation guard.
	/// </summary>
	/// <param name="guard">遗留同步导航守卫函数。The legacy synchronous navigation guard function.</param>
	/// <returns>用于移除此守卫的函数。A function to remove this guard.</returns>
	[Description("@#beforeResolve")]
	public extern Action BeforeResolve(LegacyRouteNavigationGuard guard);

	/// <summary>
	/// 注册全局解析前置遗留异步导航守卫。
	/// Registers a global legacy async before-resolve navigation guard.
	/// </summary>
	/// <param name="guard">遗留异步导航守卫函数。The legacy asynchronous navigation guard function.</param>
	/// <returns>用于移除此守卫的函数。A function to remove this guard.</returns>
	[Description("@#beforeResolve")]
	public extern Action BeforeResolve(LegacyAsyncRouteNavigationGuard guard);

	/// <summary>
	/// 注册全局后置导航钩子。
	/// Registers a global after-each navigation hook.
	/// </summary>
	/// <param name="hook">后置导航钩子函数。The after-navigation hook function.</param>
	/// <returns>用于移除此钩子的函数。A function to remove this hook.</returns>
	[Description("@#afterEach")]
	public extern Action AfterEach(AfterNavigationHook hook);

	/// <summary>
	/// 注册错误处理函数（Error 类型）。
	/// Registers an error handler for Error-type errors.
	/// </summary>
	/// <param name="handler">错误处理函数。The error handler function.</param>
	/// <returns>用于移除此处理函数的函数。A function to remove this handler.</returns>
	[Description("@#onError")]
	public extern Action OnError(ErrorRouterErrorHandler handler);

	/// <summary>
	/// 注册错误处理函数（NavigationFailure 类型）。
	/// Registers an error handler for NavigationFailure-type errors.
	/// </summary>
	/// <param name="handler">错误处理函数。The error handler function.</param>
	/// <returns>用于移除此处理函数的函数。A function to remove this handler.</returns>
	[Description("@#onError")]
	public extern Action OnError(NavigationFailureRouterErrorHandler handler);

	/// <summary>
	/// 注册错误处理函数（NavigationRedirectError 类型）。
	/// Registers an error handler for NavigationRedirectError-type errors.
	/// </summary>
	/// <param name="handler">错误处理函数。The error handler function.</param>
	/// <returns>用于移除此处理函数的函数。A function to remove this handler.</returns>
	[Description("@#onError")]
	public extern Action OnError(NavigationRedirectRouterErrorHandler handler);

	/// <summary>
	/// 注册错误处理函数（String 类型）。
	/// Registers an error handler for string-type errors.
	/// </summary>
	/// <param name="handler">错误处理函数。The error handler function.</param>
	/// <returns>用于移除此处理函数的函数。A function to remove this handler.</returns>
	[Description("@#onError")]
	public extern Action OnError(StringRouterErrorHandler handler);

	/// <summary>
	/// 注册错误处理函数（Number 类型）。
	/// Registers an error handler for Number-type errors.
	/// </summary>
	/// <param name="handler">错误处理函数。The error handler function.</param>
	/// <returns>用于移除此处理函数的函数。A function to remove this handler.</returns>
	[Description("@#onError")]
	public extern Action OnError(NumberRouterErrorHandler handler);

	/// <summary>
	/// 注册错误处理函数（Boolean 类型）。
	/// Registers an error handler for Boolean-type errors.
	/// </summary>
	/// <param name="handler">错误处理函数。The error handler function.</param>
	/// <returns>用于移除此处理函数的函数。A function to remove this handler.</returns>
	[Description("@#onError")]
	public extern Action OnError(BooleanRouterErrorHandler handler);

	/// <summary>
	/// 注册错误处理函数（BigInt 类型）。
	/// Registers an error handler for BigInt-type errors.
	/// </summary>
	/// <param name="handler">错误处理函数。The error handler function.</param>
	/// <returns>用于移除此处理函数的函数。A function to remove this handler.</returns>
	[Description("@#onError")]
	public extern Action OnError(BigIntRouterErrorHandler handler);

	/// <summary>
	/// 注册错误处理函数（Symbol 类型）。
	/// Registers an error handler for Symbol-type errors.
	/// </summary>
	/// <param name="handler">错误处理函数。The error handler function.</param>
	/// <returns>用于移除此处理函数的函数。A function to remove this handler.</returns>
	[Description("@#onError")]
	public extern Action OnError(SymbolRouterErrorHandler handler);

	/// <summary>
	/// 注册错误处理函数（Object 类型）。
	/// Registers an error handler for Object-type errors.
	/// </summary>
	/// <param name="handler">错误处理函数。The error handler function.</param>
	/// <returns>用于移除此处理函数的函数。A function to remove this handler.</returns>
	[Description("@#onError")]
	public extern Action OnError(ObjectRouterErrorHandler handler);

	/// <summary>
	/// 注册错误处理函数（Array 类型）。
	/// Registers an error handler for Array-type errors.
	/// </summary>
	/// <param name="handler">错误处理函数。The error handler function.</param>
	/// <returns>用于移除此处理函数的函数。A function to remove this handler.</returns>
	[Description("@#onError")]
	public extern Action OnError(ArrayRouterErrorHandler handler);

	/// <summary>
	/// 返回路由器初始化完成的 Promise。
	/// Returns a promise that resolves when the router is ready.
	/// </summary>
	/// <returns>路由器就绪后解析的 Promise。A promise that resolves when the router is ready.</returns>
	[Description("@#isReady")]
	public extern IPromise IsReady();
}

/// <summary>
/// 路由匹配器，负责路由记录的注册、移除和解析。
/// Route matcher responsible for registering, removing, and resolving route records.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class RouterMatcher
{
	/// <summary>
	/// 初始化 RouterMatcher 类的新实例。
	/// Initializes a new instance of the RouterMatcher class.
	/// </summary>
	protected RouterMatcher()
	{
	}

	/// <summary>
	/// 添加一条新的路由记录。
	/// Adds a new route record.
	/// </summary>
	/// <param name="record">要添加的路由记录。The route record to add.</param>
	/// <returns>用于移除此路由的函数。A function to remove this route.</returns>
	[Description("@#addRoute")]
	public extern Action AddRoute(RouteRecordRaw record);

	/// <summary>
	/// 在指定父匹配器下添加一条新的路由记录。
	/// Adds a new route record under the specified parent matcher.
	/// </summary>
	/// <param name="record">要添加的路由记录。The route record to add.</param>
	/// <param name="parent">父匹配器。The parent matcher.</param>
	/// <returns>用于移除此路由的函数。A function to remove this route.</returns>
	[Description("@#addRoute")]
	public extern Action AddRoute(RouteRecordRaw record, RouteRecordMatcher parent);

	/// <summary>
	/// 通过匹配器移除路由记录。
	/// Removes a route record by its matcher.
	/// </summary>
	/// <param name="matcher">要移除的匹配器。The matcher to remove.</param>
	[Description("@#removeRoute")]
	public extern void RemoveRoute(RouteRecordMatcher matcher);

	/// <summary>
	/// 通过名称移除路由记录。
	/// Removes a route record by its name.
	/// </summary>
	/// <param name="name">要移除的路由名称。The name of the route to remove.</param>
	[Description("@#removeRoute")]
	public extern void RemoveRoute(RouteRecordName name);

	/// <summary>
	/// 移除所有路由记录。
	/// Removes all route records.
	/// </summary>
	[Description("@#clearRoutes")]
	public extern void ClearRoutes();

	/// <summary>
	/// 获取所有路由记录匹配器。
	/// Gets all route record matchers.
	/// </summary>
	/// <returns>路由记录匹配器数组。Array of route record matchers.</returns>
	[Description("@#getRoutes")]
	public extern RouteRecordMatcher[] GetRoutes();

	/// <summary>
	/// 根据名称获取路由记录匹配器。
	/// Gets a route record matcher by its name.
	/// </summary>
	/// <param name="name">路由记录名称。The route record name.</param>
	/// <returns>匹配器，如果未找到则为 null。The matcher, or null if not found.</returns>
	[Description("@#getRecordMatcher")]
	public extern RouteRecordMatcher? GetRecordMatcher(RouteRecordName name);

	/// <summary>
	/// 解析路由位置到匹配器位置。
	/// Resolves a route location to a matcher location.
	/// </summary>
	/// <param name="location">原始匹配器位置。The raw matcher location.</param>
	/// <param name="currentLocation">当前匹配器位置。The current matcher location.</param>
	/// <returns>解析后的匹配器位置。The resolved matcher location.</returns>
	[Description("@#resolve")]
	public extern MatcherLocation Resolve(MatcherLocationRaw location, MatcherLocation currentLocation);
}

/// <summary>
/// 导航失败错误，包含失败类型和来源/目标路由信息。
/// Navigation failure error containing the failure type and source/target route information.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class NavigationFailure : Error
{
	/// <summary>
	/// 初始化 NavigationFailure 类的新实例。
	/// Initializes a new instance of the NavigationFailure class.
	/// </summary>
	protected NavigationFailure()
	{
	}

	/// <summary>
	/// 导航失败的类型。
	/// The type of navigation failure.
	/// </summary>
	[Description("@#type")]
	public extern NavigationFailureType Type { get; }

	/// <summary>
	/// 导航的目标路由位置。
	/// The target route location of the navigation.
	/// </summary>
	[Description("@#to")]
	public extern RouteLocationNormalized To { get; }

	/// <summary>
	/// 导航的来源路由位置。
	/// The source route location of the navigation.
	/// </summary>
	[Description("@#from")]
	public extern RouteLocationNormalized From { get; }
}

/// <summary>
/// 导航重定向错误，表示在导航守卫中发生了重定向。
/// Navigation redirect error indicating a redirect occurred during navigation guards.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class NavigationRedirectError : Error
{
	/// <summary>
	/// 初始化 NavigationRedirectError 类的新实例。
	/// Initializes a new instance of the NavigationRedirectError class.
	/// </summary>
	protected NavigationRedirectError()
	{
	}

	/// <summary>
	/// 错误的类型。
	/// The error type.
	/// </summary>
	[Description("@#type")]
	public extern ErrorTypes Type { get; }

	/// <summary>
	/// 重定向的目标路由位置。
	/// The target route location of the redirect.
	/// </summary>
	[Description("@#to")]
	public extern RouteLocationRaw To { get; }

	/// <summary>
	/// 重定向的来源路由位置。
	/// The source route location of the redirect.
	/// </summary>
	[Description("@#from")]
	public extern RouteLocationNormalized From { get; }
}

/// <summary>
/// 原始路由组件映射字典，用于多命名视图场景。
/// Raw route components mapping dictionary for multi-named-view scenarios.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RawRouteComponents : Vue3.VueProps, System.Collections.IEnumerable
{
	/// <summary>
	/// 按视图名称获取或设置原始路由组件。
	/// Gets or sets a raw route component by view name.
	/// </summary>
	/// <param name="key">视图名称。The view name.</param>
	public extern RawRouteComponent? this[string key] { get; set; }

	/// <summary>
	/// 添加具有指定视图名称的原始路由组件。
	/// Adds a raw route component with the specified view name.
	/// </summary>
	/// <param name="key">视图名称。The view name.</param>
	/// <param name="value">原始路由组件。The raw route component.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RawRouteComponent value);

	/// <summary>
	/// 添加具有指定视图名称的 Vue 组件。
	/// Adds a Vue component with the specified view name.
	/// </summary>
	/// <param name="key">视图名称。The view name.</param>
	/// <param name="value">Vue 组件。The Vue component.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, ECMAScript.Vue3.IVueComponent value);

	/// <summary>
	/// 添加具有指定视图名称的组件加载器。
	/// Adds a component loader with the specified view name.
	/// </summary>
	/// <param name="key">视图名称。The view name.</param>
	/// <param name="value">组件加载器。The component loader.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteComponentLoader value);

	/// <summary>
	/// 返回遍历原始路由组件条目的枚举器。
	/// Returns an enumerator that iterates through the raw route component entries.
	/// </summary>
	/// <returns>原始路由组件条目的枚举器。An enumerator for the raw route component entries.</returns>
	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

/// <summary>
/// 已解析的路由组件映射字典，用于多命名视图场景。
/// Resolved route components mapping dictionary for multi-named-view scenarios.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouteComponents : Vue3.VueProps, System.Collections.IEnumerable
{
	/// <summary>
	/// 按视图名称获取或设置已解析的路由组件。
	/// Gets or sets a resolved route component by view name.
	/// </summary>
	/// <param name="key">视图名称。The view name.</param>
	public extern RouteComponent? this[string key] { get; set; }

	/// <summary>
	/// 添加具有指定视图名称的路由组件。
	/// Adds a route component with the specified view name.
	/// </summary>
	/// <param name="key">视图名称。The view name.</param>
	/// <param name="value">路由组件。The route component.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteComponent value);

	/// <summary>
	/// 添加具有指定视图名称的 Vue 组件。
	/// Adds a Vue component with the specified view name.
	/// </summary>
	/// <param name="key">视图名称。The view name.</param>
	/// <param name="value">Vue 组件。The Vue component.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, ECMAScript.Vue3.IVueComponent value);

	/// <summary>
	/// 添加具有指定视图名称的组件加载器。
	/// Adds a component loader with the specified view name.
	/// </summary>
	/// <param name="key">视图名称。The view name.</param>
	/// <param name="value">组件加载器。The component loader.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteComponentLoader value);

	/// <summary>
	/// 返回遍历路由组件条目的枚举器。
	/// Returns an enumerator that iterates through the route component entries.
	/// </summary>
	/// <returns>路由组件条目的枚举器。An enumerator for the route component entries.</returns>
	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

/// <summary>
/// 导航守卫 next 回调列表，按索引访问。
/// Navigation guard next callback list, accessible by index.
/// </summary>
[ECMAScript]
[Description("@#")]
public record NavigationGuardNextCallbackList : Vue3.VueProps, System.Collections.IEnumerable
{
	/// <summary>
	/// 按索引获取或设置导航守卫 next 回调。
	/// Gets or sets a navigation guard next callback by index.
	/// </summary>
	/// <param name="index">回调的索引位置。The index of the callback.</param>
	public extern NavigationGuardNextCallback? this[Number index] { get; set; }

	/// <summary>
	/// 向列表中添加一个导航守卫 next 回调。
	/// Adds a navigation guard next callback to the list.
	/// </summary>
	/// <param name="value">要添加的回调。The callback to add.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(NavigationGuardNextCallback value);

	/// <summary>
	/// 返回遍历回调列表的枚举器。
	/// Returns an enumerator that iterates through the callback list.
	/// </summary>
	/// <returns>回调列表的枚举器。An enumerator for the callback list.</returns>
	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

/// <summary>
/// 导航守卫 next 回调的命名映射，按视图名称索引。
/// Named map of navigation guard next callbacks, indexed by view name.
/// </summary>
[ECMAScript]
[Description("@#")]
public record NavigationGuardNextCallbackMap : Vue3.VueProps, System.Collections.IEnumerable
{
	/// <summary>
	/// 按视图名称获取或设置导航守卫 next 回调列表。
	/// Gets or sets a navigation guard next callback list by view name.
	/// </summary>
	/// <param name="key">视图名称。The view name.</param>
	public extern NavigationGuardNextCallbackList? this[string key] { get; set; }

	/// <summary>
	/// 添加具有指定视图名称的回调列表。
	/// Adds a callback list with the specified view name.
	/// </summary>
	/// <param name="key">视图名称。The view name.</param>
	/// <param name="value">回调列表。The callback list.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, NavigationGuardNextCallbackList value);

	/// <summary>
	/// 返回遍历回调映射条目的枚举器。
	/// Returns an enumerator that iterates through the callback map entries.
	/// </summary>
	/// <returns>回调映射条目的枚举器。An enumerator for the callback map entries.</returns>
	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

/// <summary>
/// 路由组件实例映射，按视图名称索引。
/// Route component instance map, indexed by view name.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouteComponentInstanceMap : Vue3.VueProps, System.Collections.IEnumerable
{
	/// <summary>
	/// 按视图名称获取或设置组件公共实例。
	/// Gets or sets a component public instance by view name.
	/// </summary>
	/// <param name="key">视图名称。The view name.</param>
	public extern Vue3.VueComponentPublicInstance? this[string key] { get; set; }

	/// <summary>
	/// 添加具有指定视图名称的组件公共实例。
	/// Adds a component public instance with the specified view name.
	/// </summary>
	/// <param name="key">视图名称。The view name.</param>
	/// <param name="value">组件公共实例。The component public instance.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, Vue3.VueComponentPublicInstance? value);

	/// <summary>
	/// 返回遍历组件实例映射条目的枚举器。
	/// Returns an enumerator that iterates through the component instance map entries.
	/// </summary>
	/// <returns>组件实例映射条目的枚举器。An enumerator for the component instance map entries.</returns>
	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

/// <summary>
/// 按视图名称索引的路由属性配置映射。
/// Map of route props configurations indexed by view name.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouteNamedProps : Vue3.VueProps, System.Collections.IEnumerable
{
	/// <summary>
	/// 按视图名称获取或设置路由属性配置。
	/// Gets or sets route props configuration by view name.
	/// </summary>
	/// <param name="key">视图名称。The view name.</param>
	public extern RouteRecordProps? this[string key] { get; set; }

	/// <summary>
	/// 添加路由属性配置。
	/// Adds a route props configuration.
	/// </summary>
	/// <param name="key">视图名称。The view name.</param>
	/// <param name="value">路由属性配置对象。The route props configuration object.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteRecordProps value);

	/// <summary>
	/// 添加布尔值路由属性配置（true 表示将路由参数作为属性传递）。
	/// Adds a boolean route props configuration (true passes route params as props).
	/// </summary>
	/// <param name="key">视图名称。The view name.</param>
	/// <param name="value">是否将路由参数作为属性传递。Whether to pass route params as props.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, bool value);

	/// <summary>
	/// 添加 Vue 属性对象作为路由属性配置。
	/// Adds a Vue props object as route props configuration.
	/// </summary>
	/// <param name="key">视图名称。The view name.</param>
	/// <param name="value">Vue 属性对象。The Vue props object.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, Vue3.VueProps value);

	/// <summary>
	/// 添加属性解析函数作为路由属性配置。
	/// Adds a props resolver function as route props configuration.
	/// </summary>
	/// <param name="key">视图名称。The view name.</param>
	/// <param name="value">属性解析函数。The props resolver function.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteRecordPropsResolver value);

	/// <summary>
	/// 返回遍历属性映射条目的枚举器。
	/// Returns an enumerator that iterates through the props map entries.
	/// </summary>
	/// <returns>属性映射条目的枚举器。An enumerator for the props map entries.</returns>
	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

/// <summary>
/// 路由记录的基类，包含所有路由记录类型的共享属性。
/// Base class for route records containing shared properties for all route record types.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract record RouteRecordBase : Vue3.VueProps
{
	/// <summary>
	/// 路由记录的名称。
	/// The name of the route record.
	/// </summary>
	[Description("@#name")]
	public RouteRecordName? Name { get; init; }

	/// <summary>
	/// 路由路径模式。
	/// The route path pattern.
	/// </summary>
	[Description("@#path")]
	public string Path { get; init; } = default!;

	/// <summary>
	/// 路由别名。
	/// Aliases for the route.
	/// </summary>
	[Description("@#alias")]
	public RouteRecordAlias? Alias { get; init; }

	/// <summary>
	/// 附加到此路由的自定义元数据。
	/// Custom metadata attached to this route.
	/// </summary>
	[Description("@#meta")]
	public RouteMeta? Meta { get; init; }

	/// <summary>
	/// 进入此路由前执行的导航守卫。
	/// Navigation guard executed before entering this route.
	/// </summary>
	[Description("@#beforeEnter")]
	public RouteRecordBeforeEnter? BeforeEnter { get; init; }

	/// <summary>
	/// 是否对路径匹配区分大小写。
	/// Whether path matching should be case-sensitive.
	/// </summary>
	[Description("@#sensitive")]
	public bool? Sensitive { get; init; }

	/// <summary>
	/// 是否不允许尾部可选的分隔符。
	/// Whether to disallow optional trailing delimiters.
	/// </summary>
	[Description("@#strict")]
	public bool? Strict { get; init; }

	/// <summary>
	/// 是否将模式匹配到路径末尾。已弃用，始终为 true。
	/// Whether to match the pattern to the end of the path. Deprecated and always true.
	/// </summary>
	[Description("@#end")]
	[Obsolete("Vue Router 4 documents end as deprecated and always true. Do not author new route records with End.")]
	public bool? End { get; init; }
}

/// <summary>
/// 单视图路由记录，定义一个默认视图的组件。
/// Single-view route record defining a component for the default view.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouteRecordSingleView : RouteRecordBase
{
	/// <summary>
	/// 默认视图要渲染的组件。
	/// The component to render in the default view.
	/// </summary>
	[Description("@#component")]
	public RawRouteComponent Component { get; init; } = default!;

	/// <summary>
	/// 传递给组件的属性配置。
	/// The props configuration to pass to the component.
	/// </summary>
	[Description("@#props")]
	public RouteRecordProps? Props { get; init; }
}

/// <summary>
/// 带子路由的单视图路由记录。
/// Single-view route record with nested children.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouteRecordSingleViewWithChildren : RouteRecordBase
{
	/// <summary>
	/// 默认视图要渲染的组件。
	/// The component to render in the default view.
	/// </summary>
	[Description("@#component")]
	public RawRouteComponent? Component { get; init; }

	/// <summary>
	/// 嵌套的子路由记录。
	/// Nested child route records.
	/// </summary>
	[Description("@#children")]
	public RouteRecordRaw[] Children { get; init; } = default!;

	/// <summary>
	/// 传递给组件的属性配置。
	/// The props configuration to pass to the component.
	/// </summary>
	[Description("@#props")]
	public RouteRecordProps? Props { get; init; }

	/// <summary>
	/// 重定向目标。
	/// The redirect target.
	/// </summary>
	[Description("@#redirect")]
	public RouteRecordRedirectOption? Redirect { get; init; }
}

/// <summary>
/// 多视图（命名视图）路由记录。
/// Multiple-view (named-view) route record.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouteRecordMultipleViews : RouteRecordBase
{
	/// <summary>
	/// 命名视图的组件映射。
	/// The components mapping for named views.
	/// </summary>
	[Description("@#components")]
	public RawRouteComponents Components { get; init; } = default!;

	/// <summary>
	/// 各命名视图的属性配置映射。
	/// The props configuration mapping for each named view.
	/// </summary>
	[Description("@#props")]
	public RouteRecordNamedViewProps? Props { get; init; }
}

/// <summary>
/// 带子路由的多视图（命名视图）路由记录。
/// Multiple-view (named-view) route record with nested children.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouteRecordMultipleViewsWithChildren : RouteRecordBase
{
	/// <summary>
	/// 命名视图的组件映射。
	/// The components mapping for named views.
	/// </summary>
	[Description("@#components")]
	public RawRouteComponents? Components { get; init; }

	/// <summary>
	/// 嵌套的子路由记录。
	/// Nested child route records.
	/// </summary>
	[Description("@#children")]
	public RouteRecordRaw[] Children { get; init; } = default!;

	/// <summary>
	/// 各命名视图的属性配置映射。
	/// The props configuration mapping for each named view.
	/// </summary>
	[Description("@#props")]
	public RouteRecordNamedViewProps? Props { get; init; }

	/// <summary>
	/// 重定向目标。
	/// The redirect target.
	/// </summary>
	[Description("@#redirect")]
	public RouteRecordRedirectOption? Redirect { get; init; }
}

/// <summary>
/// 重定向路由记录，将路由重定向到其他位置。
/// Redirect route record that redirects to another location.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouteRecordRedirect : RouteRecordBase
{
	/// <summary>
	/// 嵌套的子路由记录。
	/// Nested child route records.
	/// </summary>
	[Description("@#children")]
	public RouteRecordRaw[]? Children { get; init; }

	/// <summary>
	/// 重定向目标。
	/// The redirect target.
	/// </summary>
	[Description("@#redirect")]
	public RouteRecordRedirectOption Redirect { get; init; } = default!;
}

/// <summary>
/// RouterLink 组件 aria-current 属性的允许值枚举。
/// Enum of allowed values for the RouterLink component's aria-current attribute.
/// </summary>
[String]
public enum RouterLinkAriaCurrentValue
{
	/// <summary>
	/// 表示分页上下文中的当前页。
	/// Represents the current page in a pagination context.
	/// </summary>
	[Description("@#page")]
	Page,

	/// <summary>
	/// 表示步骤上下文中的当前步骤。
	/// Represents the current step in a step context.
	/// </summary>
	[Description("@#step")]
	Step,

	/// <summary>
	/// 表示面包屑或流程上下文中的当前位置。
	/// Represents the current location in a breadcrumb or flow context.
	/// </summary>
	[Description("@#location")]
	Location,

	/// <summary>
	/// 表示日期上下文中的当前日期。
	/// Represents the current date in a date context.
	/// </summary>
	[Description("@#date")]
	Date,

	/// <summary>
	/// 表示时间上下文中的当前时间。
	/// Represents the current time in a time context.
	/// </summary>
	[Description("@#time")]
	Time,

	/// <summary>
	/// 布尔值 true，表示当前项。
	/// Boolean true, representing the current item.
	/// </summary>
	[Description("@#true")]
	True,

	/// <summary>
	/// 布尔值 false，表示非当前项。
	/// Boolean false, representing a non-current item.
	/// </summary>
	[Description("@#false")]
	False
}

/// <summary>
/// RouterLink 组件的核心选项。
/// Core options for the RouterLink component.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouterLinkOptions : Vue3.VueProps
{
	/// <summary>
	/// 链接的目标路由位置。
	/// The target route location for the link.
	/// </summary>
	[Description("@#to")]
	public RouteLocationRaw To { get; init; } = default!;

	/// <summary>
	/// 是否在导航时替换当前历史条目而不是推入新条目。
	/// Whether to replace the current history entry instead of pushing a new one during navigation.
	/// </summary>
	[Description("@#replace")]
	public bool? Replace { get; init; }
}

/// <summary>
/// RouterLink 组件的完整属性集。
/// Full props for the RouterLink component.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouterLinkProps : RouterLinkOptions
{

	/// <summary>
	/// 是否禁用默认链接渲染，完全由作用域插槽控制输出。
	/// Whether to disable default link rendering and fully control output via the scoped slot.
	/// </summary>
	[Description("@#custom")]
	public bool? Custom { get; init; }

	/// <summary>
	/// 活跃链接的 CSS 类名。
	/// The CSS class to apply to active links.
	/// </summary>
	[Description("@#activeClass")]
	public string? ActiveClass { get; init; }

	/// <summary>
	/// 精确活跃链接的 CSS 类名。
	/// The CSS class to apply to exactly active links.
	/// </summary>
	[Description("@#exactActiveClass")]
	public string? ExactActiveClass { get; init; }

	/// <summary>
	/// aria-current 属性的值。
	/// The value for the aria-current attribute.
	/// </summary>
	[Description("@#ariaCurrentValue")]
	public RouterLinkAriaCurrentValue? AriaCurrentValue { get; init; }

	/// <summary>
	/// 是否在导航期间启用 View Transition API。
	/// Whether to enable the View Transition API during navigation.
	/// </summary>
	[Description("@#viewTransition")]
	public bool? ViewTransition { get; init; }
}

/// <summary>
/// useLink() 组合式函数的选项。
/// Options for the useLink() composable.
/// </summary>
[ECMAScript]
[Description("@#")]
public record UseLinkOptions : Vue3.VueProps
{
	/// <summary>
	/// <c>useLink()</c> 接受的链接目标。Vue Router 官方同时接受
	/// 普通的路由位置值以及包装这些值的响应式 ref。
	/// Link target accepted by <c>useLink()</c>. Vue Router officially accepts both
	/// plain route-location values and reactive refs wrapping those values.
	/// </summary>
	[Description("@#to")]
	public RouteLocationRawMaybeRef To { get; init; } = default!;

	/// <summary>
	/// <c>useLink()</c> 是否应通过 <c>router.replace()</c> 进行导航。
	/// 在官方 Vue Router API 中，此选项也支持响应式 ref。
	/// Whether <c>useLink()</c> should navigate via <c>router.replace()</c>. This
	/// option also supports reactive refs in the official Vue Router API.
	/// </summary>
	[Description("@#replace")]
	public RouteBooleanMaybeRef? Replace { get; init; }

	/// <summary>
	/// 是否在导航期间启用 View Transition API。
	/// Whether to enable the View Transition API during navigation.
	/// </summary>
	[Description("@#viewTransition")]
	public bool? ViewTransition { get; init; }
}

/// <summary>
/// useLink() 组合式函数的返回值基类。
/// Base class for the return value of the useLink() composable.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class UseLinkReturn
{
	/// <summary>
	/// 初始化 UseLinkReturn 类的新实例。
	/// Initializes a new instance of the UseLinkReturn class.
	/// </summary>
	protected UseLinkReturn()
	{
	}

	/// <summary>
	/// 已解析的路由位置的响应式计算引用。
	/// Reactive computed reference to the resolved route location.
	/// </summary>
	[Description("@#route")]
	public extern Vue3.VueComputedRef<RouteLocationResolved> Route { get; }

	/// <summary>
	/// 链接 href 的响应式计算引用。
	/// Reactive computed reference to the link href.
	/// </summary>
	[Description("@#href")]
	public extern Vue3.VueComputedRef<string> Href { get; }

	/// <summary>
	/// 链接是否活跃（部分匹配）的响应式计算引用。
	/// Reactive computed reference indicating whether the link is active (partial match).
	/// </summary>
	[Description("@#isActive")]
	public extern Vue3.VueComputedRef<bool> IsActive { get; }

	/// <summary>
	/// 链接是否精确活跃（完全匹配）的响应式计算引用。
	/// Reactive computed reference indicating whether the link is exactly active (exact match).
	/// </summary>
	[Description("@#isExactActive")]
	public extern Vue3.VueComputedRef<bool> IsExactActive { get; }

	/// <summary>
	/// 触发链接导航。
	/// Triggers the link navigation.
	/// </summary>
	/// <returns>导航结果的 Promise。A promise resolving to the navigation result.</returns>
	[Description("@#navigate")]
	public extern IPromise<RouteNavigationResult?> Navigate();

	/// <summary>
	/// 在鼠标事件上下文中触发链接导航。
	/// Triggers the link navigation in the context of a mouse event.
	/// </summary>
	/// <param name="event">触发导航的鼠标事件。The mouse event triggering navigation.</param>
	/// <returns>导航结果的 Promise。A promise resolving to the navigation result.</returns>
	[Description("@#navigate")]
	public extern IPromise<RouteNavigationResult?> Navigate(MouseEvent @event);
}

/// <summary>
/// useLink() 组合式函数的完整返回结果。
/// Full return result of the useLink() composable.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class UseLinkResult : UseLinkReturn
{
	/// <summary>
	/// 初始化 UseLinkResult 类的新实例。
	/// Initializes a new instance of the UseLinkResult class.
	/// </summary>
	protected UseLinkResult()
	{
	}
}

/// <summary>
/// 滚动位置坐标，包含水平和垂直偏移量。
/// Scroll position coordinates including horizontal and vertical offsets.
/// </summary>
[ECMAScript]
[Description("@#")]
public record ScrollPositionCoordinates : Vue3.VueProps
{
	/// <summary>
	/// 水平滚动偏移（像素）。
	/// The horizontal scroll offset in pixels.
	/// </summary>
	[Description("@#left")]
	public double? Left { get; init; }

	/// <summary>
	/// 垂直滚动偏移（像素）。
	/// The vertical scroll offset in pixels.
	/// </summary>
	[Description("@#top")]
	public double? Top { get; init; }

	/// <summary>
	/// 滚动行为模式。
	/// The scroll behavior mode.
	/// </summary>
	[Description("@#behavior")]
	public ScrollBehavior? Behavior { get; init; }
}

/// <summary>
/// 基于元素的滚动位置，指定要滚动到的目标元素。
/// Element-based scroll position specifying the target element to scroll to.
/// </summary>
[ECMAScript]
[Description("@#")]
public record ScrollPositionElement : ScrollPositionCoordinates
{
	/// <summary>
	/// 要滚动到的目标元素或 CSS 选择器。
	/// The target element or CSS selector to scroll to.
	/// </summary>
	[Description("@#el")]
	public ScrollPositionTarget El { get; init; } = default!;
}

/// <summary>
/// 规范化后的滚动位置，坐标为确定值。
/// Normalized scroll position with definitive coordinate values.
/// </summary>
[ECMAScript]
[Description("@#")]
public record ScrollPositionNormalized : Vue3.VueProps
{
	/// <summary>
	/// 水平滚动偏移（像素）。
	/// The horizontal scroll offset in pixels.
	/// </summary>
	[Description("@#left")]
	public double Left { get; init; }

	/// <summary>
	/// 垂直滚动偏移（像素）。
	/// The vertical scroll offset in pixels.
	/// </summary>
	[Description("@#top")]
	public double Top { get; init; }

	/// <summary>
	/// 滚动行为模式。
	/// The scroll behavior mode.
	/// </summary>
	[Description("@#behavior")]
	public ScrollBehavior? Behavior { get; init; }
}

/// <summary>
/// 滚动位置目标的联合类型，可以是 CSS 选择器字符串或 DOM 元素。
/// Union type for scroll position targets, which can be a CSS selector string or a DOM element.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ScrollPositionTarget(string, Element)
{
	/// <summary>
	/// 以 CSS 选择器字符串形式获取目标。
	/// Gets the target as a CSS selector string.
	/// </summary>
	public string? AsSelector => Value as string;

	/// <summary>
	/// 以 DOM 元素形式获取目标。
	/// Gets the target as a DOM element.
	/// </summary>
	public Element? AsElement => Value as Element;
}

/// <summary>
/// 路由滚动行为的返回值联合类型，支持多种滚动位置描述形式。
/// Union type for the return value of router scroll behavior, supporting multiple scroll position representations.
/// </summary>
[ECMAScript]
[Union]
[Description("@#")]
public readonly struct RouterScrollResult : IUnion
{
	private readonly byte _kind;
	private readonly bool? _bool;
	private readonly ScrollPositionCoordinates? _coordinates;
	private readonly ScrollPositionElement? _element;
	private readonly ScrollPositionNormalized? _normalized;

	public RouterScrollResult(bool value)
	{
		_kind = 1;
		_bool = value;
		_coordinates = default;
		_element = default;
		_normalized = default;
	}

	public RouterScrollResult(ScrollPositionCoordinates value)
	{
		_kind = 2;
		_bool = default;
		_coordinates = value;
		_element = default;
		_normalized = default;
	}

	public RouterScrollResult(ScrollPositionElement value)
	{
		_kind = 3;
		_bool = default;
		_coordinates = default;
		_element = value;
		_normalized = default;
	}

	public RouterScrollResult(ScrollPositionNormalized value)
	{
		_kind = 4;
		_bool = default;
		_coordinates = default;
		_element = default;
		_normalized = value;
	}

	/// <summary>
	/// 以布尔值形式获取结果（true 表示保持当前位置，false 表示滚动到顶部）。
	/// Gets the result as a boolean (true keeps current position, false scrolls to top).
	/// </summary>
	public bool? AsBool => _kind == 1 ? _bool : default;

	/// <summary>
	/// 以坐标形式获取滚动位置。
	/// Gets the scroll position as coordinates.
	/// </summary>
	public ScrollPositionCoordinates? AsCoordinates => _kind == 2 ? _coordinates : default;

	/// <summary>
	/// 以元素定位形式获取滚动位置。
	/// Gets the scroll position as an element-based position.
	/// </summary>
	public ScrollPositionElement? AsElement => _kind == 3 ? _element : default;

	/// <summary>
	/// 以规范化坐标形式获取滚动位置。
	/// Gets the scroll position as normalized coordinates.
	/// </summary>
	public ScrollPositionNormalized? AsNormalized => _kind == 4 ? _normalized : default;

	/// <summary>
	/// 获取擦除后的 JavaScript 值。
	/// Gets the erased JavaScript value.
	/// </summary>
	public object? Value => _kind switch
	{
		1 => _bool,
		2 => _coordinates,
		3 => _element,
		4 => _normalized,
		_ => default
	};

	/// <summary>
	/// 从布尔值隐式转换为滚动结果。
	/// Implicit conversion from a boolean to a scroll result.
	/// </summary>
	/// <param name="value">布尔值。The boolean value.</param>
	public static implicit operator RouterScrollResult(bool value)
		=> new(value);

	/// <summary>
	/// 从坐标对象隐式转换为滚动结果。
	/// Implicit conversion from coordinates to a scroll result.
	/// </summary>
	/// <param name="value">滚动位置坐标。The scroll position coordinates.</param>
	public static implicit operator RouterScrollResult(ScrollPositionCoordinates value)
		=> new(value);

	/// <summary>
	/// 从元素定位对象隐式转换为滚动结果。
	/// Implicit conversion from an element position to a scroll result.
	/// </summary>
	/// <param name="value">基于元素的滚动位置。The element-based scroll position.</param>
	public static implicit operator RouterScrollResult(ScrollPositionElement value)
		=> new(value);

	/// <summary>
	/// 从规范化坐标对象隐式转换为滚动结果。
	/// Implicit conversion from normalized coordinates to a scroll result.
	/// </summary>
	/// <param name="value">规范化的滚动位置。The normalized scroll position.</param>
	public static implicit operator RouterScrollResult(ScrollPositionNormalized value)
		=> new(value);
}

/// <summary>
/// 路由滚动行为处理器的联合类型，支持同步和异步回调。
/// Union type for the router scroll behavior handler, supporting both sync and async callbacks.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union RouterScrollHandler(RouterScrollBehavior, AsyncRouterScrollBehavior)
{
	/// <summary>
	/// 以同步滚动行为回调形式获取处理器。
	/// Gets the handler as a synchronous scroll behavior callback.
	/// </summary>
	public RouterScrollBehavior? AsSync => Value as RouterScrollBehavior;

	/// <summary>
	/// 以异步滚动行为回调形式获取处理器。
	/// Gets the handler as an asynchronous scroll behavior callback.
	/// </summary>
	public AsyncRouterScrollBehavior? AsAsync => Value as AsyncRouterScrollBehavior;

	/// <summary>
	/// 从同步滚动行为回调创建滚动处理器。
	/// Creates a scroll handler from a synchronous scroll behavior callback.
	/// </summary>
	/// <param name="value">同步滚动行为回调。The synchronous scroll behavior callback.</param>
	/// <returns>滚动处理器。The scroll handler.</returns>
	[ECMAScriptInline("__arg1")]
	public extern static RouterScrollHandler From(RouterScrollBehavior value);

	/// <summary>
	/// 从异步滚动行为回调创建滚动处理器。
	/// Creates a scroll handler from an asynchronous scroll behavior callback.
	/// </summary>
	/// <param name="value">异步滚动行为回调。The asynchronous scroll behavior callback.</param>
	/// <returns>滚动处理器。The scroll handler.</returns>
	[ECMAScriptInline("__arg1")]
	public extern static RouterScrollHandler From(AsyncRouterScrollBehavior value);
}

/// <summary>
/// RouterLink 组件的插槽定义。
/// Slot definitions for the RouterLink component.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouterLinkSlots : Vue3.VueSlots
{
	/// <summary>
	/// 默认作用域插槽，用于自定义链接内容渲染。
	/// Default scoped slot for custom link content rendering.
	/// </summary>
	[Description("@#default")]
	public RouterLinkSlotCallback? Default { get; init; }
}

/// <summary>
/// RouterLink 作用域插槽中暴露的数据对象。
/// Data object exposed in the RouterLink scoped slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouterLinkSlotScope : Vue3.VueProps
{
	/// <summary>
	/// 已解析的路由位置。
	/// The resolved route location.
	/// </summary>
	[Description("@#route")]
	public RouteLocationResolved Route { get; init; } = default!;

	/// <summary>
	/// 链接的 href 属性值。
	/// The href attribute value of the link.
	/// </summary>
	[Description("@#href")]
	public string Href { get; init; } = default!;

	/// <summary>
	/// 链接是否处于活跃状态。
	/// Whether the link is active.
	/// </summary>
	[Description("@#isActive")]
	public bool IsActive { get; init; }

	/// <summary>
	/// 链接是否处于精确活跃状态。
	/// Whether the link is exactly active.
	/// </summary>
	[Description("@#isExactActive")]
	public bool IsExactActive { get; init; }

	/// <summary>
	/// 触发导航的回调函数。
	/// Callback function to trigger navigation.
	/// </summary>
	[Description("@#navigate")]
	public RouterLinkNavigateCallback Navigate { get; init; } = default!;
}

/// <summary>
/// RouterView 组件的属性。
/// Props for the RouterView component.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouterViewProps : Vue3.VueProps
{
	/// <summary>
	/// 要渲染的命名视图名称。
	/// The name of the named view to render.
	/// </summary>
	[Description("@#name")]
	public string? Name { get; init; }

	/// <summary>
	/// 用于解析组件的自定义路由位置（覆盖当前路由）。
	/// Custom route location for resolving the component (overrides the current route).
	/// </summary>
	[Description("@#route")]
	public RouteLocationNormalized? Route { get; init; }
}

/// <summary>
/// RouterView 作用域插槽中暴露的数据对象。
/// Data object exposed in the RouterView scoped slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouterViewSlotScope : Vue3.VueProps
{
	/// <summary>
	/// 匹配的组件的 VNode。
	/// The VNode of the matched component.
	/// </summary>
	[Description("@#Component")]
	public Vue3.IVNode? Component { get; init; }

	/// <summary>
	/// 已加载的规范化路由位置。
	/// The normalized loaded route location.
	/// </summary>
	[Description("@#route")]
	public RouteLocationNormalizedLoaded Route { get; init; } = default!;
}

/// <summary>
/// RouterView 组件的插槽定义。
/// Slot definitions for the RouterView component.
/// </summary>
[ECMAScript]
[Description("@#")]
public record RouterViewSlots : Vue3.VueSlots
{
	/// <summary>
	/// 默认作用域插槽，用于自定义路由视图内容渲染。
	/// Default scoped slot for custom router view content rendering.
	/// </summary>
	[Description("@#default")]
	public RouterViewSlotCallback? Default { get; init; }
}
