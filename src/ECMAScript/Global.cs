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
	/// Returns the string representation of the value.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[Jazor]
	public extern static RegExp RegExp(string value);

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

	[Description("@#BigInt")]
	public extern static BigInt BigInt_(string value);

	[Description("@#document")]
	public extern static Document Document { get; }

	[Description("@#window")]
	public extern static WindowProxy Window { get; }

	[Description("@#parseFloat")]
	public extern static Number ParseFloat(object? value, ushort radix = 10);

	[Description("@#parseInt")]
	public extern static Number ParseInt(object? value, ushort radix = 10);

	[Description("@#isNaN")]
	public extern static bool IsNaN(object? value);
}
