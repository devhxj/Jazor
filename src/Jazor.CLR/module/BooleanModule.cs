namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "bool","System/BooleanModule.js")]
public static class BooleanModule
{
	///<summary>Represents the Boolean value <see langword="true" /> as a string. This field is read-only.</summary>
	[Jazor(Op.Discard ,"static readonly bool.TrueString")]
	public extern static Boolean _49c57acefc093fcc();

	///<summary>Represents the Boolean value <see langword="false" /> as a string. This field is read-only.</summary>
	[Jazor(Op.Discard ,"static readonly bool.FalseString")]
	public extern static Boolean _19c3bb431dd19e1f();

	[Jazor(Op.Discard ,"bool.Boolean()")]
	public extern static Boolean _2bd9618624257446();

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Discard ,"override bool.GetHashCode()")]
	public extern static Number _80b6c29cc0038969(Boolean instance);

	///<summary>Converts the value of this instance to its equivalent string representation (either "True" or "False").</summary>
	[Jazor(Op.Discard ,"override bool.ToString()")]
	public extern static string _d48c2d39317daf8f(Boolean instance);

	///<summary>Converts the value of this instance to its equivalent string representation (either "True" or "False").</summary>
	[Jazor(Op.Discard ,"bool.ToString(System.IFormatProvider)")]
	public extern static string _6e30cb91da447de8(Boolean instance, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current boolean instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"bool.TryFormat(System.Span<char>, out int)")]
	public extern static Array<object?> _811623fcb5eec2f4(Boolean instance, Uint32Array destination, out Number charsWritten);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Discard ,"override bool.Equals(object)")]
	public extern static bool _97cc6572c33639b7(Boolean instance, object? obj);

	///<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.Boolean" /> object.</summary>
	[Jazor(Op.Discard ,"bool.Equals(bool)")]
	public extern static bool _22566f8453458531(Boolean instance, object obj);

	///<summary>Compares this instance to a specified object and returns an integer that indicates their relationship to one another.</summary>
	[Jazor(Op.Discard ,"bool.CompareTo(object)")]
	public extern static Number _f877237b160159b0(Boolean instance, object? obj);

	///<summary>Compares this instance to a specified <see cref="T:System.Boolean" /> object and returns an integer that indicates their relationship to one another.</summary>
	[Jazor(Op.Discard ,"bool.CompareTo(bool)")]
	public extern static Number _52e94ceda3f9af79(Boolean instance, object value);

	///<summary>Converts the specified string representation of a logical value to its <see cref="T:System.Boolean" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static bool.Parse(string)")]
	public extern static bool _5dbf54319ebc8dfe(string value);

	///<summary>Converts the specified span representation of a logical value to its <see cref="T:System.Boolean" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static bool.Parse(System.ReadOnlySpan<char>)")]
	public extern static bool _c3ccfdf8f687d2bf(Uint32Array value);

	///<summary>Tries to convert the specified string representation of a logical value to its <see cref="T:System.Boolean" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static bool.TryParse(string, out bool)")]
	public extern static Array<object?> _dada4bbdacd7aa19(string? value, out object result);

	///<summary>Tries to convert the specified span representation of a logical value to its <see cref="T:System.Boolean" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static bool.TryParse(System.ReadOnlySpan<char>, out bool)")]
	public extern static Array<object?> _619c4d1c94319558(Uint32Array value, out object result);

	///<summary>Returns the type code for the <see cref="T:System.Boolean" /> value type.</summary>
	[Jazor(Op.Discard ,"bool.GetTypeCode()")]
	public extern static System.TypeCode _eb6a23c2a874fdf1(Boolean instance);
}
