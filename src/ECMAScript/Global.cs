namespace ECMAScript;

[ECMAScript]
[Description("@#")]
[Jazor]
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

	[Description("@#Number")]
	public extern static Number JNumber(Number value);

	[Description("@#Number")]
	public extern static Number JNumber(BigInt value);

	[Description("@#Number")]
	public extern static Number JNumber(string value);

	[Description("@#BigInt")]
	public extern static BigInt JBigInt(Number value);

	[Description("@#BigInt")]
	public extern static BigInt JBigInt(string value);

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
