namespace ECMAScript;

[ECMAScript]
[Description("@#")]
public static partial class Global
{
	/// <summary>
	/// Returns the JavaScript type of the value.
	/// </summary>
	[SpecialCompile]
	public extern static string TypeOf(object? value);

	/// <summary>
	/// Returns the string representation of the value.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[SpecialCompile]
	public extern static RegExp RegExp(string value);

	public extern static Number Number(Number value);

	public extern static Number Number(BigInt value);

	public extern static Number Number(string value);

	public extern static BigInt BigInt(Number value);

	public extern static BigInt BigInt(string value);

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
