namespace ECMAScript;

/// <summary>
/// Projection of JavaScript's <c>JSON</c> host object.
/// This remains a dedicated static host because JavaScript exposes <c>JSON</c>
/// as its own runtime object rather than as part of <c>globalThis</c>.
/// </summary>
[ECMAScript]
[Description("@#JSON")]
public static class JSON
{
	/// <summary>
	/// JavaScript object shape passed as the third argument to <c>JSON.parse</c> revivers.
	/// This is not a global host; it models the runtime context object directly.
	/// </summary>
	[Description("@#")]
	public sealed class ParseContext
	{
		/// <summary>
		/// Source text for the current primitive parse node when JavaScript provides it.
		/// When the runtime leaves this field absent, the C# projection surfaces that absence as <see langword="null" />.
		/// </summary>
		[Description("@#source")]
		public extern string? Source { get; }
	}

	/// <summary>
	/// JavaScript object shape returned by <c>JSON.rawJSON</c>.
	/// This is not a global host; it models the frozen runtime object directly.
	/// </summary>
	[Description("@#")]
	public sealed class RawValue
	{
		/// <summary>
		/// Underlying raw JSON text carried by the runtime object.
		/// </summary>
		[Description("@#rawJSON")]
		public extern string RawJSON { get; }
	}

	/// <summary>
	/// Projection of JavaScript <c>JSON.parse(text)</c>.
	/// The result is modeled as <see cref="object"/> because the runtime value can be any JSON-compatible shape.
	/// </summary>
	[Description("@#parse")]
	public extern static object? Parse(string text);

	/// <summary>
	/// Projection of JavaScript <c>JSON.parse(text, reviver)</c>.
	/// The reviver stays callback-shaped so the host surface matches JavaScript rather than introducing a CLR serializer abstraction.
	/// </summary>
	[Description("@#parse")]
	public extern static object? Parse(string text, Func<string, object?, object?> reviver);

	/// <summary>
	/// Projection of JavaScript <c>JSON.parse(text, reviver)</c> with the current reviver context object.
	/// JavaScript passes <c>(key, value, context)</c>, where <c>context.source</c> is exposed for supported primitive parse nodes.
	/// </summary>
	[Description("@#parse")]
	public extern static object? Parse(string text, Func<string, object?, ParseContext, object?> reviver);

	/// <summary>
	/// Returns whether the supplied value is a JavaScript raw-JSON wrapper produced by <c>JSON.rawJSON</c>.
	/// </summary>
	[Description("@#isRawJSON")]
	public extern static bool IsRawJSON(object? value);

	/// <summary>
	/// Creates the JavaScript raw-JSON wrapper object used by <c>JSON.stringify</c>.
	/// The returned object is a real runtime value, not a CLR-only helper.
	/// </summary>
	[Description("@#rawJSON")]
	public extern static RawValue RawJSON(string text);

	/// <summary>
	/// Projection of JavaScript <c>JSON.stringify(value)</c>.
	/// Nullable is used because JavaScript may return <c>undefined</c> for unsupported top-level inputs,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// </summary>
	[Description("@#stringify")]
	public extern static string? Stringify(object? value);

	/// <summary>
	/// Projection of JavaScript <c>JSON.stringify(value, replacer, space)</c>.
	/// The host signature stays close to JavaScript runtime shape rather than introducing a CLR-specific serializer abstraction.
	/// The nullable return still exists because JavaScript may produce <c>undefined</c>, which this C# projection surfaces as <see langword="null" />.
	/// </summary>
	[Description("@#stringify")]
	public extern static string? Stringify(object? value, object? replacer, Number? space = null);

	/// <summary>
	/// Projection of JavaScript <c>JSON.stringify(value, replacer, space)</c> with a replacer function.
	/// JavaScript calls the replacer with <c>(key, value)</c> for each visited property or element.
	/// </summary>
	[Description("@#stringify")]
	public extern static string? Stringify(object? value, Func<string, object?, object?> replacer, Number? space = null);

	/// <summary>
	/// Projection of JavaScript <c>JSON.stringify(value, replacer, space)</c> with a property-list replacer.
	/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for the JavaScript array-like inclusion list.
	/// Runtime coercion still follows JavaScript rules: string and number-like entries become property names, and unsupported entries are ignored.
	/// </summary>
	[Description("@#stringify")]
	public extern static string? Stringify(object? value, IEnumerable<object?> replacer, Number? space = null);

	/// <summary>
	/// Projection of JavaScript <c>JSON.stringify(value, replacer, space)</c> with string indentation.
	/// JavaScript accepts either a number or a string for <c>space</c>, so both shapes are modeled explicitly.
	/// The nullable return still exists because JavaScript may produce <c>undefined</c>, which this C# projection surfaces as <see langword="null" />.
	/// </summary>
	[Description("@#stringify")]
	public extern static string? Stringify(object? value, object? replacer, string? space);

	/// <summary>
	/// Projection of JavaScript <c>JSON.stringify(value, replacer, space)</c> with a replacer function and string indentation.
	/// </summary>
	[Description("@#stringify")]
	public extern static string? Stringify(object? value, Func<string, object?, object?> replacer, string? space);

	/// <summary>
	/// Projection of JavaScript <c>JSON.stringify(value, replacer, space)</c> with a property-list replacer and string indentation.
	/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for the JavaScript array-like inclusion list.
	/// </summary>
	[Description("@#stringify")]
	public extern static string? Stringify(object? value, IEnumerable<object?> replacer, string? space);
}
