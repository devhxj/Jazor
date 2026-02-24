namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.Nullable","System/NullableModule.js")]
public static class NullableModule
{
	///<summary>Compares the relative values of two <see cref="T:System.Nullable`1" /> objects.</summary>
	[Jazor(Op.Discard ,"static System.Nullable.Compare<T>(T?, T?)")]
	public extern static Number _fcbe94e0f2cfc6f4<T>(object n1, object n2);

	///<summary>Indicates whether two specified <see cref="T:System.Nullable`1" /> objects are equal.</summary>
	[Jazor(Op.Discard ,"static System.Nullable.Equals<T>(T?, T?)")]
	public extern static bool _55d5a6397d48a134<T>(object n1, object n2);

	///<summary>Returns the underlying type argument of the specified nullable type.</summary>
	[Jazor(Op.Discard ,"static System.Nullable.GetUnderlyingType(System.Type)")]
	public extern static System.Type? _c2b1e5fa73eecbd6(object nullableType);

	///<summary>Retrieves a readonly reference to the location in the <see cref="T:System.Nullable`1" /> instance where the value is stored.</summary>
	[Jazor(Op.Discard ,"static System.Nullable.GetValueRefOrDefaultRef<T>(ref readonly T?)")]
	public extern static T _3431dfe868f6e773<T>(ref readonly object nullable);
}
