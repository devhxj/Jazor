namespace Jazor.CLR;

[ECMAScriptModule("System/NullableT1Module.js")]
[Jazor(Op.Allowed, "System.Nullable<T>")]
public static class NullableT1Module<T>
{
	/// <summary>
	/// C#: nullable.HasValue
	/// JS: value !== null && value !== undefined
	/// </summary>
	[Jazor(Op.Inline, "System.Nullable<T>.HasValue.get", "(__arg1 !== null && __arg1 !== undefined)")]
	public extern static bool _hasValue(T? instance);

	/// <summary>
	/// C#: nullable.Value
	/// JS: value (throws if null)
	/// </summary>
	[Jazor(Op.Inline, "System.Nullable<T>.Value.get", "__arg1")]
	public extern static T _value(T? instance);

	/// <summary>
	/// C#: nullable.GetValueOrDefault()
	/// JS: value ?? default(T)
	/// </summary>
	[Jazor(Op.Inline, "System.Nullable<T>.GetValueOrDefault()", "(__arg1 ?? null)")]
	public extern static T? _getValueOrDefault(T? instance);

	/// <summary>
	/// C#: nullable.GetValueOrDefault(defaultValue)
	/// JS: value ?? defaultValue
	/// </summary>
	[Jazor(Op.Inline, "System.Nullable<T>.GetValueOrDefault(T)", "(__arg1 ?? __arg2)")]
	public extern static T _getValueOrDefaultWithDefault(T? instance, T defaultValue);

	///<summary>Compares the relative values of two <see cref="T:System.Nullable`1" /> objects.</summary>
	[Jazor(Op.Discard ,"static System.Nullable.Compare<T>(T?, T?)")]
	public extern static Number _fcbe94e0f2cfc6f4(object n1, object n2);

	///<summary>Indicates whether two specified <see cref="T:System.Nullable`1" /> objects are equal.</summary>
	[Jazor(Op.Discard ,"static System.Nullable.Equals<T>(T?, T?)")]
	public extern static bool _55d5a6397d48a134(object n1, object n2);

	///<summary>Returns the underlying type argument of the specified nullable type.</summary>
	[Jazor(Op.Discard ,"static System.Nullable.GetUnderlyingType(System.Type)")]
	public extern static System.Type? _c2b1e5fa73eecbd6(object nullableType);

	///<summary>Retrieves a readonly reference to the location in the <see cref="T:System.Nullable`1" /> instance where the value is stored.</summary>
	[Jazor(Op.Discard ,"static System.Nullable.GetValueRefOrDefaultRef<T>(ref readonly T?)")]
	public extern static T _3431dfe868f6e773(ref readonly object nullable);
}
