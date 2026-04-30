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
/// </summary>
public static partial class Global
{
	/// <summary>
	/// Returns the JavaScript type of the value.
	/// </summary>
	[Jazor]
	public extern static string TypeOf(object? value);

	/// <summary>
	/// C# host projection of JavaScript global <c>RegExp(pattern)</c>.
	/// This stays on <see cref="Global"/> because JavaScript exposes it on
	/// <c>globalThis</c> rather than as a member on another host object.
	/// </summary>
	[Description("@#RegExp")]
	public extern static RegExp RegExp(string value);

	/// <summary>
	/// C# host projection of JavaScript global <c>RegExp(pattern, flags)</c>.
	/// This overload exists only to match the JavaScript constructor/function call surface,
	/// not to introduce a separate CLR regex abstraction.
	/// </summary>
	[Description("@#RegExp")]
	public extern static RegExp RegExp(string value, string flags);

	/// <summary>
	/// C# host projection of JavaScript global <c>RegExp(existingRegExp)</c>.
	/// This stays on <see cref="Global"/> because JavaScript allows the global constructor function to be called with an existing regular expression value.
	/// </summary>
	[Description("@#RegExp")]
	public extern static RegExp RegExp(RegExp value);

	/// <summary>
	/// C# host projection of JavaScript global <c>RegExp(existingRegExp, flags)</c>.
	/// This overload exists only to match the JavaScript constructor/function call surface,
	/// not to introduce a separate CLR regex abstraction.
	/// </summary>
	[Description("@#RegExp")]
	public extern static RegExp RegExp(RegExp value, string flags);

	/// <summary>
	/// C# host name for JavaScript <c>Number(...)</c>.
	/// The trailing underscore only avoids naming conflicts on the C# side.
	/// </summary>
	[Description("@#Number")]
	public extern static Number NumberFn();

	/// <summary>
	/// C# host name for JavaScript <c>Number(...)</c>.
	/// This overload keeps JavaScript's value-coercion entry point available for arbitrary runtime values.
	/// The trailing underscore only avoids naming conflicts on the C# side.
	/// </summary>
	[Description("@#Number")]
	public extern static Number NumberFn(object? value);

	[Description("@#Number")]
	public extern static Number NumberFn(Number value);

	[Description("@#Number")]
	public extern static Number NumberFn(BigInt value);

	[Description("@#Number")]
	public extern static Number NumberFn(string value);

	/// <summary>
	/// C# host name for JavaScript <c>String()</c>.
	/// The trailing underscore only avoids naming conflicts on the C# side.
	/// </summary>
	[Description("@#String")]
	public extern static string StringFn();

	/// <summary>
	/// C# host name for JavaScript <c>String(...)</c>.
	/// This overload keeps JavaScript's value-to-string coercion entry point available for arbitrary runtime values.
	/// The trailing underscore only avoids naming conflicts on the C# side.
	/// </summary>
	[Description("@#String")]
	public extern static string StringFn(object? value);

	/// <summary>
	/// C# host name for JavaScript <c>Boolean()</c>.
	/// The trailing underscore only avoids naming conflicts on the C# side.
	/// </summary>
	[Description("@#Boolean")]
	public extern static bool BooleanFn();

	/// <summary>
	/// C# host name for JavaScript <c>Boolean(...)</c>.
	/// This overload keeps JavaScript's truthiness conversion entry point available for arbitrary runtime values.
	/// The trailing underscore only avoids naming conflicts on the C# side.
	/// </summary>
	[Description("@#Boolean")]
	public extern static bool BooleanFn(object? value);

	/// <summary>
	/// C# host name for JavaScript <c>BigInt(...)</c>.
	/// The trailing underscore only avoids naming conflicts on the C# side.
	/// </summary>
	[Description("@#BigInt")]
	public extern static BigInt BigIntFn(Number value);

	/// <summary>
	/// C# host name for JavaScript <c>BigInt(...)</c>.
	/// The trailing underscore only avoids naming conflicts on the C# side.
	/// </summary>
	[Description("@#BigInt")]
	public extern static BigInt BigIntFn(string value);

	/// <summary>
	/// C# host name for JavaScript <c>BigInt(...)</c>.
	/// This overload keeps JavaScript's bigint conversion entry point available for arbitrary runtime values.
	/// The trailing underscore only avoids naming conflicts on the C# side.
	/// Runtime failures still follow JavaScript <c>BigInt</c> conversion semantics.
	/// </summary>
	[Description("@#BigInt")]
	public extern static BigInt BigIntFn(object? value);

	/// <summary>
	/// C# host name for JavaScript <c>Symbol(...)</c>.
	/// The trailing underscore only avoids naming conflicts with the <see cref="Symbol"/> type.
	/// </summary>
	[Description("@#Symbol")]
	public extern static Symbol SymbolFn(string? description = null);

	/// <summary>
	/// C# host name for JavaScript <c>Symbol(...)</c>.
	/// JavaScript accepts any description value and stringifies it at runtime when it is not <c>undefined</c>.
	/// </summary>
	[Description("@#Symbol")]
	public extern static Symbol SymbolFn(object? description);

	[Description("@#document")]
	public extern static Document Document { get; }

	[Description("@#window")]
	public extern static WindowProxy Window { get; }

	/// <summary>
	/// C# host projection of JavaScript global <c>parseFloat</c>.
	/// This remains on <see cref="Global"/> because JavaScript exposes it on
	/// <c>globalThis</c> rather than on the <c>Number</c> constructor.
	/// </summary>
	[Description("@#parseFloat")]
	public extern static Number ParseFloat(object? value);

	/// <summary>
	/// C# host projection of JavaScript global <c>parseInt</c>.
	/// This overload preserves JavaScript's omitted-radix behavior instead of forcing a CLR-side default value.
	/// </summary>
	[Description("@#parseInt")]
	public extern static Number ParseInt(object? value);

	/// <summary>
	/// C# host projection of JavaScript global <c>parseInt</c> with an explicit radix.
	/// Nullable is used so the public host can still represent JavaScript's "argument omitted" shape when needed.
	/// </summary>
	[Description("@#parseInt")]
	public extern static Number ParseInt(object? value, Number? radix);

	/// <summary>
	/// C# host projection of JavaScript global <c>isNaN</c>.
	/// This is the global function variant, so it intentionally stays distinct from
	/// <c>Number.isNaN</c>.
	/// </summary>
	[Description("@#isNaN")]
	public extern static bool IsNaN(object? value);

	/// <summary>
	/// C# host projection of JavaScript global <c>isFinite</c>.
	/// This stays on <see cref="Global"/> because JavaScript exposes it on
	/// <c>globalThis</c> rather than on the <c>Number</c> constructor.
	/// </summary>
	[Description("@#isFinite")]
	public extern static bool IsFinite(object? value);

	/// <summary>
	/// C# host projection of JavaScript global <c>eval</c>.
	/// The result stays as <see cref="object"/> because JavaScript can evaluate to any runtime value shape.
	/// </summary>
	[Description("@#eval")]
	public extern static object? Eval(string source);

	/// <summary>
	/// Global JavaScript <c>NaN</c> value.
	/// This is kept on <see cref="Global"/> because JavaScript exposes it directly on <c>globalThis</c> in addition to <c>Number.NaN</c>.
	/// </summary>
	[Description("@#NaN")]
	public extern static Number NaN { get; }

	/// <summary>
	/// Global JavaScript <c>Infinity</c> value.
	/// This is kept on <see cref="Global"/> because JavaScript exposes it directly on <c>globalThis</c>.
	/// </summary>
	[Description("@#Infinity")]
	public extern static Number Infinity { get; }

	/// <summary>
	/// C# host projection of JavaScript global <c>encodeURI</c>.
	/// This remains on <see cref="Global"/> because JavaScript exposes it as a global function rather than on another host object.
	/// </summary>
	[Description("@#encodeURI")]
	public extern static string EncodeURI(string uri);

	/// <summary>
	/// C# host projection of JavaScript global <c>decodeURI</c>.
	/// </summary>
	[Description("@#decodeURI")]
	public extern static string DecodeURI(string encodedURI);

	/// <summary>
	/// C# host projection of JavaScript global <c>encodeURIComponent</c>.
	/// This stays distinct from <see cref="EncodeURI"/> because JavaScript applies different escaping rules.
	/// </summary>
	[Description("@#encodeURIComponent")]
	public extern static string EncodeURIComponent(string uriComponent);

	/// <summary>
	/// C# host projection of JavaScript global <c>decodeURIComponent</c>.
	/// </summary>
	[Description("@#decodeURIComponent")]
	public extern static string DecodeURIComponent(string encodedURIComponent);

	/// <summary>
	/// Legacy JavaScript global <c>escape</c>.
	/// This remains on <see cref="Global"/> because JavaScript exposes it on <c>globalThis</c> for web compatibility.
	/// </summary>
	[Description("@#escape")]
	public extern static string Escape(string text);

	/// <summary>
	/// Legacy JavaScript global <c>unescape</c>.
	/// This remains on <see cref="Global"/> because JavaScript exposes it on <c>globalThis</c> for web compatibility.
	/// </summary>
	[Description("@#unescape")]
	public extern static string Unescape(string text);

	/// <summary>
	/// C# host projection of JavaScript global <c>queueMicrotask</c>.
	/// This stays on <see cref="Global"/> because JavaScript exposes it directly on <c>globalThis</c>.
	/// </summary>
	[Description("@#queueMicrotask")]
	public extern static void QueueMicrotask(Action callback);

	/// <summary>
	/// C# host projection of JavaScript global <c>structuredClone</c>.
	/// Nullable is used because JavaScript can clone <c>undefined</c>, and the C# projection maps that absence to <see langword="null" />.
	/// </summary>
	[Description("@#structuredClone")]
	public extern static object? StructuredClone(object? value, StructuredSerializeOptions? options = default);
}
