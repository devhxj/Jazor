namespace ECMAScript;

[ECMAScript]
[Description("@#")]
public static partial class Global
{
	/// <summary>
	/// Checks if the value is strictly null.
	/// </summary>
	[ECMAScriptLiteral("{0} === null")]
	public extern static bool IsNull(object? value);

	/// <summary>
	/// Checks if the value is strictly undefined.
	/// </summary>
	[ECMAScriptLiteral("{0} === undefined")]
	public extern static bool IsUndefined(object? value);

	/// <summary>
	/// Checks if the value is either null or undefined.
	/// </summary>
	[ECMAScriptLiteral("{0} === null || {0} === undefined")]
	public extern static bool IsNullOrUndefined(object? value);

	/// <summary>
	/// Returns the value if not null or undefined, otherwise returns undefined.
	/// </summary>
	[ECMAScriptLiteral("{0} ?? undefined")]
	public extern static T? OrUndefined<T>(T? value);

	/// <summary>
	/// Returns the JavaScript type of the value.
	/// </summary>
	[ECMAScriptLiteral("typeof {0}")]
	public extern static string TypeOf(object? value);

	[ECMAScriptLiteral("{0}")]
	public extern static RegExp RegExp(string value);

	[Description("@#Number")]
	public extern static Number Number(Number value);

	[Description("@#Number")]
	public extern static Number Number(BigInt value);

	[Description("@#Number")]
	public extern static Number Number(string value);

	[Description("@#BigInt")]
	public extern static BigInt BigInt(Number value);

	[Description("@#BigInt")]
	public extern static BigInt BigInt(string value);

	[Description("@#document")]
	public extern static Document document { get; }

	[Description("@#window")]
	public extern static WindowProxy window { get; }

	[Description("@#parseFloat")]
	public extern static Number ParseFloat(object? value, ushort radix = 10);

	[Description("@#parseInt")]
	public extern static Number ParseInt(object? value, ushort radix = 10);

	[Description("@#isNaN")]
	public extern static bool IsNaN(object? value);
}
