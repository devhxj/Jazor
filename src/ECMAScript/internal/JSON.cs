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
	/// Projection of JavaScript <c>JSON.stringify(value, replacer, space)</c> with string indentation.
	/// JavaScript accepts either a number or a string for <c>space</c>, so both shapes are modeled explicitly.
	/// The nullable return still exists because JavaScript may produce <c>undefined</c>, which this C# projection surfaces as <see langword="null" />.
	/// </summary>
	[Description("@#stringify")]
	public extern static string? Stringify(object? value, object? replacer, string? space);
}
