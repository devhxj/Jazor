namespace Jazor.CLR;

[ECMAScriptModule("System/BooleanModule.js")]
[Jazor(Op.Alias, "bool", "Boolean")]
public static class BooleanModule
{
	///<summary>Represents the Boolean value <see langword="true" /> as a string. This field is read-only.</summary>
	[Jazor(Op.Inline, "static readonly bool.TrueString", "true")]
	public extern static bool _49c57acefc093fcc();

	///<summary>Represents the Boolean value <see langword="false" /> as a string. This field is read-only.</summary>
	[Jazor(Op.Inline, "static readonly bool.FalseString", "false")]
	public extern static bool _19c3bb431dd19e1f();

	[Jazor(Op.Allowed, "bool.Boolean()")]
	public extern static bool _2bd9618624257446();

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Discard, "override bool.GetHashCode()")]
	public extern static Number _80b6c29cc0038969(bool instance);

	///<summary>Converts the value of this instance to its equivalent string representation (either "True" or "False").</summary>
	[Jazor(Op.Alias, "override bool.ToString()", "toString")]
	public extern static string _d48c2d39317daf8f(bool instance);

	///<summary>Converts the value of this instance to its equivalent string representation (either "True" or "False").</summary>
	[Jazor(Op.Discard, "bool.ToString(System.IFormatProvider)")]
	public extern static string _6e30cb91da447de8(bool instance, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current boolean instance into the provided span of characters.</summary>
	[Jazor(Op.Discard, "bool.TryFormat(System.Span<char>, out int)")]
	public extern static Array<object?> _811623fcb5eec2f4(bool instance, string destination, Number charsWritten);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Inline, "override bool.Equals(object)", "(__arg1 === __arg2)")]
	public extern static bool _97cc6572c33639b7(bool instance, object? obj);

	///<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.Boolean" /> object.</summary>
	[Jazor(Op.Inline, "bool.Equals(bool)", "(__arg1 === __arg2)")]
	public extern static bool _22566f8453458531(bool instance, bool obj);

	///<summary>Compares this instance to a specified object and returns an integer that indicates their relationship to one another.</summary>
	[Jazor(Op.Inline, "bool.CompareTo(object)", "(__arg1 === __arg2 ? 0 : (__arg1 ? 1 : -1))")]
	public extern static Number _f877237b160159b0(bool instance, object? obj);

	///<summary>Compares this instance to a specified <see cref="T:System.Boolean" /> object and returns an integer that indicates their relationship to one another.</summary>
	[Jazor(Op.Inline, "bool.CompareTo(bool)", "(__arg1 === __arg2 ? 0 : (__arg1 ? 1 : -1))")]
	public extern static Number _52e94ceda3f9af79(bool instance, bool value);

	///<summary>Converts the specified string representation of a logical value to its <see cref="T:System.Boolean" /> equivalent.</summary>
	[Jazor(Op.Import, "static bool.Parse(string)")]
	public static bool _5dbf54319ebc8dfe(string? value)
	{
		var str = value?.Trim()?.ToLower();
		if (str == "true")
			return true;
		else if (str == "false")
			return false;
		else
			throw new Error($"FormatException: String '{value}' was not recognized as a valid Boolean.");
	}

	///<summary>Converts the specified span representation of a logical value to its <see cref="T:System.Boolean" /> equivalent.</summary>
	[Jazor(Op.Import, "static bool.Parse(System.ReadOnlySpan<char>)")]
	public static bool _c3ccfdf8f687d2bf(string value) => _5dbf54319ebc8dfe(value);

	///<summary>Tries to convert the specified string representation of a logical value to its <see cref="T:System.Boolean" /> equivalent.</summary>
	[Jazor(Op.Import, "static bool.TryParse(string, out bool)")]
	public static Array<object?> _dada4bbdacd7aa19(string? value, bool result)
	{
		var str = value?.Trim()?.ToLower();
		if (str == "true")
			return [true, true];
		else if (str == "false")
			return [true, false];

		return [false, false];
	}

	///<summary>Tries to convert the specified span representation of a logical value to its <see cref="T:System.Boolean" /> equivalent.</summary>
	[Jazor(Op.Import, "static bool.TryParse(System.ReadOnlySpan<char>, out bool)")]
	public static Array<object?> _619c4d1c94319558(string value, bool result) => _dada4bbdacd7aa19(value, result);

	///<summary>Returns the type code for the <see cref="T:System.Boolean" /> value type.</summary>
	[Jazor(Op.Compile, "bool.GetTypeCode()")]
	public extern static System.TypeCode _eb6a23c2a874fdf1(bool instance);
}
