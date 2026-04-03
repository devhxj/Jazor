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
	public extern static RegExp RegExp(string value);

	/// <summary>
	/// C# host projection of JavaScript global <c>RegExp(pattern, flags)</c>.
	/// This overload exists only to match the JavaScript constructor/function call surface,
	/// not to introduce a separate CLR regex abstraction.
	/// </summary>
	public extern static RegExp RegExp(string value, string flags);

	/// <summary>
	/// C# host name for JavaScript <c>Number(...)</c>.
	/// The trailing underscore only avoids naming conflicts on the C# side.
	/// </summary>
	[Description("@#Number")]
	public extern static Number Number_(Number value);

	[Description("@#Number")]
	public extern static Number Number_(BigInt value);

	[Description("@#Number")]
	public extern static Number Number_(string value);

	/// <summary>
	/// C# host name for JavaScript <c>BigInt(...)</c>.
	/// The trailing underscore only avoids naming conflicts on the C# side.
	/// </summary>
	[Description("@#BigInt")]
	public extern static BigInt BigInt_(Number value);

	/// <summary>
	/// C# host name for JavaScript <c>BigInt(...)</c>.
	/// The trailing underscore only avoids naming conflicts on the C# side.
	/// </summary>
	[Description("@#BigInt")]
	public extern static BigInt BigInt_(string value);

	/// <summary>
	/// C# host name for JavaScript <c>Symbol(...)</c>.
	/// The trailing underscore only avoids naming conflicts with the <see cref="Symbol"/> type.
	/// </summary>
	[Description("@#Symbol")]
	public extern static Symbol Symbol_(string? description = null);

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
	/// The optional radix matches the JavaScript global function shape.
	/// </summary>
	[Description("@#parseInt")]
	public extern static Number ParseInt(object? value, ushort radix = 10);

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
