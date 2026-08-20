namespace ECMAScript;

/// <summary>
/// Projection of JavaScript's <c>JSON</c> host object.
/// This remains a dedicated static host because JavaScript exposes <c>JSON</c>
/// as its own runtime object rather than as part of <c>globalThis</c>.
/// JavaScript <c>JSON</c> 宿主对象的投影；它是独立运行时对象，不属于 <c>globalThis</c> 的普通函数集合。
/// </summary>
[ECMAScript]
[Description("@#JSON")]
/// <remarks>
/// JSON.parse/stringify 的回调和缺失值行为由 JavaScript runtime 决定；binding 只提供强类型
/// authoring 入口，不把 JSON 值域收窄成 CLR 对象图，也不额外引入序列化协议。
/// JSON.parse/stringify callbacks and missing-value behavior are owned by the JavaScript runtime. This binding only provides typed authoring entry points;
/// it does not narrow JSON values into a CLR object graph or add a serialization protocol.
/// </remarks>
public static class JSON
{
	/// <summary>
	/// JavaScript object shape passed as the third argument to <c>JSON.parse</c> revivers.
	/// This is not a global host; it models the runtime context object directly.
	/// 作为 <c>JSON.parse</c> reviver 第三个参数传入的 JavaScript 对象形状；不是全局宿主，直接建模运行时 context 对象。
	/// </summary>
	[Description("@#")]
	public sealed class ParseContext
	{
		/// <summary>
		/// Source text for the current primitive parse node when JavaScript provides it.
		/// When the runtime leaves this field absent, the C# projection surfaces that absence as <see langword="null" />.
		/// JavaScript 提供时，获取当前原始解析节点的 source 文本；运行时缺少字段时投影为 <see langword="null"/>。
		/// </summary>
		[Description("@#source")]
		public extern string? Source { get; }
	}

	/// <summary>
	/// JavaScript object shape returned by <c>JSON.rawJSON</c>.
	/// This is not a global host; it models the frozen runtime object directly.
	/// JavaScript <c>JSON.rawJSON</c> 返回的对象形状；不是全局宿主，直接建模冻结的运行时对象。
	/// </summary>
	[Description("@#")]
	public sealed class RawValue
	{
		/// <summary>
		/// Underlying raw JSON text carried by the runtime object.
		/// 运行时 raw JSON 对象承载的原始 JSON 文本。
		/// </summary>
		[Description("@#rawJSON")]
		public extern string RawJSON { get; }
	}

	/// <summary>
	/// Projection of JavaScript <c>JSON.parse(text)</c>.
	/// The result is modeled as <see cref="object"/> because the runtime value can be any JSON-compatible shape.
	/// JavaScript <c>JSON.parse(text)</c> 投影；结果可为任意 JSON 兼容形状，故保持 <see cref="object"/>。
	/// </summary>
	[Description("@#parse")]
	public extern static object? Parse(string text);

	/// <summary>
	/// Projection of JavaScript <c>JSON.parse(text, reviver)</c>.
	/// The reviver stays callback-shaped so the host surface matches JavaScript rather than introducing a CLR serializer abstraction.
	/// 带 reviver 的 <c>JSON.parse</c> 投影；保持回调形状以贴近 JavaScript，而不引入 CLR serializer 抽象。reviver 返回 <c>undefined</c> 时会删除属性。
	/// </summary>
	[Description("@#parse")]
	public extern static object? Parse(string text, Func<string, object?, object?> reviver);

	/// <summary>
	/// Projection of JavaScript <c>JSON.parse(text, reviver)</c> with the current reviver context object.
	/// JavaScript passes <c>(key, value, context)</c>, where <c>context.source</c> is exposed for supported primitive parse nodes.
	/// 带 context 的 <c>JSON.parse</c> reviver 投影；JavaScript 传入 <c>(key, value, context)</c>，支持的 primitive parse 节点可在 <c>context.source</c> 读取原始文本。
	/// </summary>
	[Description("@#parse")]
	public extern static object? Parse(string text, Func<string, object?, ParseContext, object?> reviver);

	/// <summary>
	/// Returns whether the supplied value is a JavaScript raw-JSON wrapper produced by <c>JSON.rawJSON</c>.
	/// 检查提供值是否为 <c>JSON.rawJSON</c> 创建的 JavaScript raw JSON wrapper。
	/// </summary>
	[Description("@#isRawJSON")]
	public extern static bool IsRawJSON(object? value);

	/// <summary>
	/// Creates the JavaScript raw-JSON wrapper object used by <c>JSON.stringify</c>.
	/// The returned object is a real runtime value, not a CLR-only helper.
	/// 创建供 <c>JSON.stringify</c> 使用的 JavaScript raw JSON wrapper；返回真实运行时值，不是 CLR-only helper。
	/// </summary>
	[Description("@#rawJSON")]
	public extern static RawValue RawJSON(string text);

	/// <summary>
	/// Projection of JavaScript <c>JSON.stringify(value)</c>.
	/// Nullable is used because JavaScript may return <c>undefined</c> for unsupported top-level inputs,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// JavaScript <c>JSON.stringify(value)</c> 投影；不支持的顶层输入可能得到 <c>undefined</c>，在 C# 中投影为 <see langword="null"/>。
	/// </summary>
	[Description("@#stringify")]
	public extern static string? Stringify(object? value);

	/// <summary>
	/// Projection of JavaScript <c>JSON.stringify(value, replacer, space)</c>.
	/// The host signature stays close to JavaScript runtime shape rather than introducing a CLR-specific serializer abstraction.
	/// The nullable return still exists because JavaScript may produce <c>undefined</c>, which this C# projection surfaces as <see langword="null" />.
	/// 使用任意 replacer 和数值缩进的 <c>JSON.stringify</c> 投影；公开签名贴近 JavaScript，返回 <c>undefined</c> 时投影为 <see langword="null"/>。
	/// </summary>
	[Description("@#stringify")]
	public extern static string? Stringify(object? value, object? replacer, Number? space = null);

	/// <summary>
	/// Projection of JavaScript <c>JSON.stringify(value, replacer, space)</c> with a replacer function.
	/// JavaScript calls the replacer with <c>(key, value)</c> for each visited property or element.
	/// 使用 replacer 函数的 <c>JSON.stringify</c> 投影；JavaScript 为每个访问属性/元素调用 <c>(key, value)</c>。
	/// </summary>
	[Description("@#stringify")]
	public extern static string? Stringify(object? value, Func<string, object?, object?> replacer, Number? space = null);

	/// <summary>
	/// Projection of JavaScript <c>JSON.stringify(value, replacer, space)</c> with a property-list replacer.
	/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for the JavaScript array-like inclusion list.
	/// Runtime coercion still follows JavaScript rules: string and number-like entries become property names, and unsupported entries are ignored.
	/// 使用 property-list replacer 的 <c>JSON.stringify</c> 投影；<see cref="IEnumerable{T}"/> 表达 JavaScript inclusion list，字符串和 number-like 项会成为属性名，其余项被忽略。
	/// </summary>
	[Description("@#stringify")]
	public extern static string? Stringify(object? value, IEnumerable<object?> replacer, Number? space = null);

	/// <summary>
	/// Projection of JavaScript <c>JSON.stringify(value, replacer, space)</c> with string indentation.
	/// JavaScript accepts either a number or a string for <c>space</c>, so both shapes are modeled explicitly.
	/// The nullable return still exists because JavaScript may produce <c>undefined</c>, which this C# projection surfaces as <see langword="null" />.
	/// 使用字符串缩进的 <c>JSON.stringify</c> 投影；JavaScript 的 <c>space</c> 可为 number 或 string，顶层结果为 <c>undefined</c> 时投影为 <see langword="null"/>。
	/// </summary>
	[Description("@#stringify")]
	public extern static string? Stringify(object? value, object? replacer, string? space);

	/// <summary>
	/// Projection of JavaScript <c>JSON.stringify(value, replacer, space)</c> with a replacer function and string indentation.
	/// 使用 replacer 函数与字符串缩进的 <c>JSON.stringify</c> 投影。
	/// </summary>
	[Description("@#stringify")]
	public extern static string? Stringify(object? value, Func<string, object?, object?> replacer, string? space);

	/// <summary>
	/// Projection of JavaScript <c>JSON.stringify(value, replacer, space)</c> with a property-list replacer and string indentation.
	/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for the JavaScript array-like inclusion list.
	/// 使用 property-list replacer 与字符串缩进的 <c>JSON.stringify</c> 投影；<see cref="IEnumerable{T}"/> 是 inclusion list 的通用 C# 输入表面。
	/// </summary>
	[Description("@#stringify")]
	public extern static string? Stringify(object? value, IEnumerable<object?> replacer, string? space);
}
