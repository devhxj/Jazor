namespace ECMAScript;

[ECMAScript]
[Description("@#")]
[Jazor]
/// <summary>
/// Host surface for JavaScript runtime globals as exposed to C#.
/// The public API aims to stay as close to JavaScript runtime shape as C# allows,
/// ideally differing only by casing. When C# syntax or BCL naming conflicts force
/// a deviation, that deviation is a host-language escape hatch rather than a different
/// runtime model.
/// 这是向 C# 暴露的 JavaScript 全局宿主表面。除大小写或 C# 命名冲突导致的必要调整外，
/// API 应尽量保持 JavaScript 运行时形状；这些调整只是宿主语言适配，不代表另一套运行时模型。
/// </summary>
public static partial class Global
{
	/// <summary>
	/// Returns the JavaScript undefined value.
	/// 返回 JavaScript 的 <c>undefined</c> 值；泛型参数只提供调用点的静态类型投影。
	/// </summary>
	[Jazor("undefined")]
	public extern static T Undefined<T>();

	/// <summary>
	/// Returns the JavaScript type of the value.
	/// 返回 JavaScript <c>typeof</c> 运算符产生的运行时类型字符串，而不是 CLR 类型名称。
	/// </summary>
	[Jazor]
	public extern static string TypeOf(object? value);

	/// <summary>
	/// C# host projection of JavaScript global <c>RegExp(pattern)</c>.
	/// This stays on <see cref="Global"/> because JavaScript exposes it on
	/// <c>globalThis</c> rather than as a member on another host object.
	/// 这是 JavaScript 全局 <c>RegExp(pattern)</c> 的 C# 投影。它属于 <see cref="Global"/>，
	/// 因为该构造/函数入口由 <c>globalThis</c> 提供，而不是其他宿主对象的成员。
	/// </summary>
	[Description("@#RegExp")]
	public extern static RegExp RegExp(string value);

	/// <summary>
	/// C# host projection of JavaScript global <c>RegExp(pattern, flags)</c>.
	/// This overload exists only to match the JavaScript constructor/function call surface,
	/// not to introduce a separate CLR regex abstraction.
	/// 投影 JavaScript 全局 <c>RegExp(pattern, flags)</c>。此重载仅匹配 JavaScript 调用形状，
	/// 不引入独立的 CLR 正则表达式抽象。
	/// </summary>
	[Description("@#RegExp")]
	public extern static RegExp RegExp(string value, string flags);

	/// <summary>
	/// C# host projection of JavaScript global <c>RegExp(existingRegExp)</c>.
	/// This stays on <see cref="Global"/> because JavaScript allows the global constructor function to be called with an existing regular expression value.
	/// 投影 JavaScript 全局 <c>RegExp(existingRegExp)</c>。JavaScript 允许用已有正则值调用全局构造函数，
	/// 因此该入口保留在 <see cref="Global"/>。
	/// </summary>
	[Description("@#RegExp")]
	public extern static RegExp RegExp(RegExp value);

	/// <summary>
	/// C# host projection of JavaScript global <c>RegExp(existingRegExp, flags)</c>.
	/// This overload exists only to match the JavaScript constructor/function call surface,
	/// not to introduce a separate CLR regex abstraction.
	/// 用已有正则值和替换后的 flags 创建 JavaScript <c>RegExp</c>；该重载只用于表达原生调用形状。
	/// </summary>
	[Description("@#RegExp")]
	public extern static RegExp RegExp(RegExp value, string flags);

	/// <summary>
	/// C# host name for JavaScript <c>Number(...)</c>.
	/// The Fn suffix only avoids naming conflicts on the C# side.
	/// JavaScript <c>Number()</c> 的 C# 宿主名称；<c>J</c> 前缀只用于避免 C# 侧命名冲突。
	/// </summary>
	[Description("@#Number")]
	public extern static Number NumberValue();

	/// <summary>
	/// C# host name for JavaScript <c>Number(...)</c>.
	/// This overload keeps JavaScript's value-coercion entry point available for arbitrary runtime values.
	/// The Fn suffix only avoids naming conflicts on the C# side.
	/// JavaScript <c>Number(value)</c> 的 C# 宿主名称，保留 JavaScript 的值转换语义；<c>J</c> 前缀仅用于 C# 命名冲突。
	/// </summary>
	[Description("@#Number")]
	public extern static Number NumberValue(object? value);

	/// <summary>Converts an existing JavaScript number through <c>Number(...)</c>. 将已有 JavaScript Number 通过 <c>Number(...)</c> 转换。</summary>
	[Description("@#Number")]
	public extern static Number NumberValue(Number value);

	/// <summary>Converts a JavaScript bigint through <c>Number(...)</c>. 通过 <c>Number(...)</c> 将 JavaScript BigInt 转换为 Number，精度规则遵循 JavaScript。</summary>
	[Description("@#Number")]
	public extern static Number NumberValue(BigInt value);

	/// <summary>Converts text through JavaScript <c>Number(...)</c>. 通过 JavaScript <c>Number(...)</c> 将文本转换为数值。</summary>
	[Description("@#Number")]
	public extern static Number NumberValue(string value);

	/// <summary>
	/// C# host name for JavaScript <c>String()</c>.
	/// The Fn suffix only avoids naming conflicts on the C# side.
	/// JavaScript <c>String()</c> 的 C# 宿主名称；<c>J</c> 前缀只用于避免 C# 侧命名冲突。
	/// </summary>
	[Description("@#String")]
	public extern static string StringValue();

	/// <summary>
	/// C# host name for JavaScript <c>String(...)</c>.
	/// This overload keeps JavaScript's value-to-string coercion entry point available for arbitrary runtime values.
	/// The Fn suffix only avoids naming conflicts on the C# side.
	/// JavaScript <c>String(value)</c> 的 C# 宿主名称，保留 JavaScript 的值转文本语义；<c>J</c> 前缀仅用于 C# 命名冲突。
	/// </summary>
	[Description("@#String")]
	public extern static string StringValue(object? value);

	/// <summary>
	/// C# host name for JavaScript <c>Boolean()</c>.
	/// The Fn suffix only avoids naming conflicts on the C# side.
	/// JavaScript <c>Boolean()</c> 的 C# 宿主名称；<c>J</c> 前缀只用于避免 C# 侧命名冲突。
	/// </summary>
	[Description("@#Boolean")]
	public extern static bool BooleanValue();

	/// <summary>
	/// C# host name for JavaScript <c>Boolean(...)</c>.
	/// This overload keeps JavaScript's truthiness conversion entry point available for arbitrary runtime values.
	/// The Fn suffix only avoids naming conflicts on the C# side.
	/// JavaScript <c>Boolean(value)</c> 的 C# 宿主名称，按 JavaScript truthiness 规则转换；<c>J</c> 前缀仅用于 C# 命名冲突。
	/// </summary>
	[Description("@#Boolean")]
	public extern static bool BooleanValue(object? value);

	/// <summary>
	/// C# host name for JavaScript <c>BigInt(...)</c>.
	/// The Fn suffix only avoids naming conflicts on the C# side.
	/// JavaScript <c>BigInt(value)</c> 的 C# 宿主名称；<c>J</c> 前缀只用于避免 C# 侧命名冲突。
	/// </summary>
	[Description("@#BigInt")]
	public extern static BigInt BigIntValue(Number value);

	/// <summary>
	/// C# host name for JavaScript <c>BigInt(...)</c>.
	/// The Fn suffix only avoids naming conflicts on the C# side.
	/// 通过 JavaScript <c>BigInt(string)</c> 解析整数文本；<c>J</c> 前缀只用于避免 C# 侧命名冲突。
	/// </summary>
	[Description("@#BigInt")]
	public extern static BigInt BigIntValue(string value);

	/// <summary>
	/// C# host name for JavaScript <c>BigInt(...)</c>.
	/// This overload keeps JavaScript's bigint conversion entry point available for arbitrary runtime values.
	/// The Fn suffix only avoids naming conflicts on the C# side.
	/// Runtime failures still follow JavaScript <c>BigInt</c> conversion semantics.
	/// 通过 JavaScript <c>BigInt(value)</c> 执行通用 bigint 转换；转换失败仍按 JavaScript <c>BigInt</c> 规则在运行时发生。
	/// </summary>
	[Description("@#BigInt")]
	public extern static BigInt BigIntValue(object? value);

	/// <summary>
	/// C# host name for JavaScript <c>Symbol(...)</c>.
	/// The Fn suffix only avoids naming conflicts with the <see cref="Symbol"/> type.
	/// JavaScript <c>Symbol(description)</c> 的 C# 宿主名称；<c>J</c> 前缀用于避免与 <see cref="Symbol"/> 类型冲突。
	/// </summary>
	[Description("@#Symbol")]
	public extern static Symbol SymbolValue(string? description = null);

	/// <summary>
	/// C# host name for JavaScript <c>Symbol(...)</c>.
	/// JavaScript accepts any description value and stringifies it at runtime when it is not <c>undefined</c>.
	/// JavaScript <c>Symbol(description)</c> 接受任意 description 值，并在其不是 <c>undefined</c> 时按运行时规则转为文本。
	/// </summary>
	[Description("@#Symbol")]
	public extern static Symbol SymbolValue(object? description);

	/// <summary>Gets the browser <c>document</c> global. 获取浏览器全局对象 <c>document</c>。</summary>
	[Description("@#document")]
	public extern static JazorDocument Document { get; }

	/// <summary>Gets the browser <c>window</c> proxy. 获取浏览器 <c>window</c> 代理对象。</summary>
	[Description("@#window")]
	public extern static WindowProxy Window { get; }

	/// <summary>
	/// C# host projection of JavaScript global <c>parseFloat</c>.
	/// This remains on <see cref="Global"/> because JavaScript exposes it on
	/// <c>globalThis</c> rather than on the <c>Number</c> constructor.
	/// JavaScript 全局 <c>parseFloat</c> 的 C# 投影，保留在 <see cref="Global"/> 而不是 <c>Number</c> 宿主上。
	/// </summary>
	[Description("@#parseFloat")]
	public extern static Number ParseFloat(object? value);

	/// <summary>
	/// C# host projection of JavaScript global <c>parseInt</c>.
	/// This overload preserves JavaScript's omitted-radix behavior instead of forcing a CLR-side default value.
	/// JavaScript 全局 <c>parseInt</c> 的 C# 投影。未传 radix 时保留 JavaScript 的省略参数行为，不在 CLR 侧强加默认值。
	/// </summary>
	[Description("@#parseInt")]
	public extern static Number ParseInt(object? value);

	/// <summary>
	/// C# host projection of JavaScript global <c>parseInt</c> with an explicit radix.
	/// Nullable is used so the public host can still represent JavaScript's "argument omitted" shape when needed.
	/// 带显式 radix 的 JavaScript 全局 <c>parseInt</c> 投影；可空值保留 JavaScript 的“参数省略”调用形状。
	/// </summary>
	[Description("@#parseInt")]
	public extern static Number ParseInt(object? value, Number? radix);

	/// <summary>
	/// C# host projection of JavaScript global <c>isNaN</c>.
	/// This is the global function variant, so it intentionally stays distinct from
	/// <c>Number.isNaN</c>.
	/// JavaScript 全局 <c>isNaN</c> 的 C# 投影；它不同于 <c>Number.isNaN</c>，会先遵循全局函数的转换语义。
	/// </summary>
	[Description("@#isNaN")]
	public extern static bool IsNaN(object? value);

	/// <summary>
	/// C# host projection of JavaScript global <c>isFinite</c>.
	/// This stays on <see cref="Global"/> because JavaScript exposes it on
	/// <c>globalThis</c> rather than on the <c>Number</c> constructor.
	/// JavaScript 全局 <c>isFinite</c> 的 C# 投影，保留在 <see cref="Global"/>，并遵循全局函数的值转换语义。
	/// </summary>
	[Description("@#isFinite")]
	public extern static bool IsFinite(object? value);

	/// <summary>
	/// C# host projection of JavaScript global <c>eval</c>.
	/// The result stays as <see cref="object"/> because JavaScript can evaluate to any runtime value shape.
	/// JavaScript 全局 <c>eval</c> 的 C# 投影。结果保持为 <see cref="object"/>，因为 JavaScript 可计算出任意运行时值形状。
	/// </summary>
	[Description("@#eval")]
	public extern static object? Eval(string source);

	/// <summary>
	/// Global JavaScript <c>NaN</c> value.
	/// This is kept on <see cref="Global"/> because JavaScript exposes it directly on <c>globalThis</c> in addition to <c>Number.NaN</c>.
	/// JavaScript 全局 <c>NaN</c> 值；除 <c>Number.NaN</c> 外它也直接存在于 <c>globalThis</c>，因此保留在 <see cref="Global"/>。
	/// </summary>
	[Description("@#NaN")]
	public extern static Number NaN { get; }

	/// <summary>
	/// Global JavaScript <c>Infinity</c> value.
	/// This is kept on <see cref="Global"/> because JavaScript exposes it directly on <c>globalThis</c>.
	/// JavaScript 全局 <c>Infinity</c> 值；它直接由 <c>globalThis</c> 提供，因此保留在 <see cref="Global"/>。
	/// </summary>
	[Description("@#Infinity")]
	public extern static Number Infinity { get; }

	/// <summary>
	/// C# host projection of JavaScript global <c>encodeURI</c>.
	/// This remains on <see cref="Global"/> because JavaScript exposes it as a global function rather than on another host object.
	/// JavaScript 全局 <c>encodeURI</c> 的 C# 投影；它是全局函数，不属于其他运行时宿主对象。
	/// </summary>
	[Description("@#encodeURI")]
	public extern static string EncodeURI(string uri);

	/// <summary>
	/// C# host projection of JavaScript global <c>decodeURI</c>.
	/// JavaScript 全局 <c>decodeURI</c> 的 C# 投影。
	/// </summary>
	[Description("@#decodeURI")]
	public extern static string DecodeURI(string encodedURI);

	/// <summary>
	/// C# host projection of JavaScript global <c>encodeURIComponent</c>.
	/// This stays distinct from <see cref="EncodeURI"/> because JavaScript applies different escaping rules.
	/// JavaScript 全局 <c>encodeURIComponent</c> 的 C# 投影；它与 <see cref="EncodeURI"/> 的转义规则不同，不能互换。
	/// </summary>
	[Description("@#encodeURIComponent")]
	public extern static string EncodeURIComponent(string uriComponent);

	/// <summary>
	/// C# host projection of JavaScript global <c>decodeURIComponent</c>.
	/// JavaScript 全局 <c>decodeURIComponent</c> 的 C# 投影。
	/// </summary>
	[Description("@#decodeURIComponent")]
	public extern static string DecodeURIComponent(string encodedURIComponent);

	/// <summary>
	/// Legacy JavaScript global <c>escape</c>.
	/// This remains on <see cref="Global"/> because JavaScript exposes it on <c>globalThis</c> for web compatibility.
	/// 已废弃的 JavaScript 全局 <c>escape</c>；仅为 Web 兼容性保留，新的 URI 编码应使用 <see cref="EncodeURI"/> 或 <see cref="EncodeURIComponent"/>。
	/// </summary>
	[Description("@#escape")]
	public extern static string Escape(string text);

	/// <summary>
	/// Legacy JavaScript global <c>unescape</c>.
	/// This remains on <see cref="Global"/> because JavaScript exposes it on <c>globalThis</c> for web compatibility.
	/// 已废弃的 JavaScript 全局 <c>unescape</c>；仅为 Web 兼容性保留，新的 URI 解码应使用 <see cref="DecodeURI"/> 或 <see cref="DecodeURIComponent"/>。
	/// </summary>
	[Description("@#unescape")]
	public extern static string Unescape(string text);

	/// <summary>
	/// C# host projection of JavaScript global <c>queueMicrotask</c>.
	/// This stays on <see cref="Global"/> because JavaScript exposes it directly on <c>globalThis</c>.
	/// JavaScript 全局 <c>queueMicrotask</c> 的 C# 投影；回调会按 JavaScript 微任务队列语义安排，不是 CLR 线程调度 API。
	/// </summary>
	[Description("@#queueMicrotask")]
	public extern static void QueueMicrotask(Action callback);

	/// <summary>
	/// C# host projection of JavaScript global <c>structuredClone</c>.
	/// Nullable is used because JavaScript can clone <c>undefined</c>, and the C# projection maps that absence to <see langword="null" />.
	/// JavaScript 全局 <c>structuredClone</c> 的 C# 投影。可空返回值表示 JavaScript 的 <c>undefined</c> 缺失值，并不表示 CLR 深拷贝。
	/// </summary>
	[Description("@#structuredClone")]
	public extern static object? StructuredClone(object? value, StructuredSerializeOptions? options = default);
}
