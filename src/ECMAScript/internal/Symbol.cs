namespace ECMAScript;

/// <summary>
/// Projection of JavaScript's <c>Symbol</c> constructor host and its well-known symbols.
/// Members stay on this runtime host instead of being redistributed into CLR helper types.
/// </summary>
[ECMAScript]
[Description("@#Symbol")]
public sealed class Symbol
{
	// Well-known Symbols
	[Description("@#hasInstance")]
	public extern static Symbol HasInstance { get; }
	[Description("@#isConcatSpreadable")]
	public extern static Symbol IsConcatSpreadable { get; }
	[Description("@#asyncIterator")]
	public extern static Symbol AsyncIterator { get; }
	[Description("@#asyncDispose")]
	public extern static Symbol AsyncDispose { get; }
	[Description("@#dispose")]
	public extern static Symbol Dispose { get; }
	[Description("@#iterator")]
	public extern static Symbol Iterator { get; }
	[Description("@#match")]
	public extern static Symbol Match { get; }
	[Description("@#matchAll")]
	public extern static Symbol MatchAll { get; }
	[Description("@#replace")]
	public extern static Symbol Replace { get; }
	[Description("@#search")]
	public extern static Symbol Search { get; }
	[Description("@#species")]
	public extern static Symbol Species { get; }
	[Description("@#split")]
	public extern static Symbol Split { get; }
	[Description("@#toPrimitive")]
	public extern static Symbol ToPrimitive { get; }
	[Description("@#toStringTag")]
	public extern static Symbol ToStringTag { get; }
	[Description("@#unscopables")]
	public extern static Symbol Unscopables { get; }

	/// <summary>
	/// Optional description carried by the JavaScript symbol.
	/// Nullable is used because symbols may be created without a description.
	/// </summary>
	[Description("@#description")]
	public extern string? Description { get; }

	[Description("@#toString")]
	public extern override string ToString();

	/// <summary>
	/// Returns the primitive symbol value carried by this host projection.
	/// </summary>
	[Description("@#valueOf")]
	public extern Symbol ValueOf();

	/// <summary>
	/// Retrieves or creates a symbol from the global registry.
	/// </summary>
	[Description("@#for")]
	public extern static Symbol For(string key);

	/// <summary>
	/// Returns the key associated with the given symbol in the global registry.
	/// </summary>
	[Description("@#keyFor")]
	public extern static string? KeyFor(Symbol sym);
}
