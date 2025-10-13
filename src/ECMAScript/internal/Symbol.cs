namespace ECMAScript;

[ECMAScript]
[Description("@#symbol")]
public sealed class Symbol
{
	// Well-known Symbols
	public extern static Symbol HasInstance { get; }
	public extern static Symbol IsConcatSpreadable { get; }
	public extern static Symbol Iterator { get; }
	public extern static Symbol Match { get; }
	public extern static Symbol MatchAll { get; }
	public extern static Symbol Replace { get; }
	public extern static Symbol Search { get; }
	public extern static Symbol Species { get; }
	public extern static Symbol Split { get; }
	public extern static Symbol ToPrimitive { get; }
	public extern static Symbol ToStringTag { get; }
	public extern static Symbol Unscopables { get; }

	public extern override string ToString();

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override bool Equals(object? obj);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override int GetHashCode();

	/// <summary>
	/// Creates a new unique symbol.
	/// </summary>
	public extern static Symbol Create(string? description = null);

	/// <summary>
	/// Retrieves or creates a symbol from the global registry.
	/// </summary>
	public extern static Symbol For(string key);

	/// <summary>
	/// Returns the key associated with the given symbol in the global registry.
	/// </summary>
	public extern static string? KeyFor(Symbol sym);

	/// <summary>
	/// Allow implicit conversion to string for easier logging and debugging
	/// </summary>
	/// <param name="symbol"></param>
	public extern static implicit operator string?(Symbol symbol);
}
