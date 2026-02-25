namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.ValueTuple","System/ValueTupleModule.js")]
public static class ValueTupleModule
{
	[Jazor(Op.Discard ,"System.ValueTuple.ValueTuple()")]
	public extern static System.ValueTuple _afe5e7b03405c9fc();

	///<summary>Returns a value that indicates whether the current <see cref="T:System.ValueTuple" /> instance is equal to a specified object.</summary>
	[Jazor(Op.Discard ,"override System.ValueTuple.Equals(object)")]
	public extern static bool _f405bb1d41845d0a(System.ValueTuple instance, object? obj);

	///<summary>Determines whether two <see cref="T:System.ValueTuple" /> instances are equal. This method always returns <see langword="true" />.</summary>
	[Jazor(Op.Discard ,"System.ValueTuple.Equals(System.ValueTuple)")]
	public extern static bool _075aabd97b9153e6(System.ValueTuple instance, object other);

	///<summary>Compares the current <see cref="T:System.ValueTuple" /> instance to a specified <see cref="T:System.ValueTuple" /> instance.</summary>
	[Jazor(Op.Discard ,"System.ValueTuple.CompareTo(System.ValueTuple)")]
	public extern static Number _f92b072b1ea77fb3(System.ValueTuple instance, object other);

	///<summary>Returns the hash code for the current <see cref="T:System.ValueTuple" /> instance.</summary>
	[Jazor(Op.Discard ,"override System.ValueTuple.GetHashCode()")]
	public extern static Number _79b4fb9a3ea0524a(System.ValueTuple instance);

	///<summary>Returns the string representation of this <see cref="T:System.ValueTuple" /> instance.</summary>
	[Jazor(Op.Discard ,"override System.ValueTuple.ToString()")]
	public extern static string _93b143a10f6cb207(System.ValueTuple instance);

	///<summary>Creates a new value tuple with zero components.</summary>
	[Jazor(Op.Discard ,"static System.ValueTuple.Create()")]
	public extern static System.ValueTuple _b2020d347b181140();

	///<summary>Creates a new value tuple with 1 component (a singleton).</summary>
	[Jazor(Op.Discard ,"static System.ValueTuple.Create<T1>(T1)")]
	public extern static System.ValueTuple<T1> _c01432b1ceab8949<T1>(object item1);

	///<summary>Creates a new value tuple with 2 components (a pair).</summary>
	[Jazor(Op.Discard ,"static System.ValueTuple.Create<T1, T2>(T1, T2)")]
	public extern static (T1, T2) _3c42e78c6d0ddf68<T1, T2>(object item1, object item2);

	///<summary>Creates a new value tuple with 3 components (a triple).</summary>
	[Jazor(Op.Discard ,"static System.ValueTuple.Create<T1, T2, T3>(T1, T2, T3)")]
	public extern static (T1, T2, T3) _6462161c42aa6ac1<T1, T2, T3>(object item1, object item2, object item3);

	///<summary>Creates a new value tuple with 4 components (a quadruple).</summary>
	[Jazor(Op.Discard ,"static System.ValueTuple.Create<T1, T2, T3, T4>(T1, T2, T3, T4)")]
	public extern static (T1, T2, T3, T4) _7d9afb217b6c02e6<T1, T2, T3, T4>(object item1, object item2, object item3, object item4);

	///<summary>Creates a new value tuple with 5 components (a quintuple).</summary>
	[Jazor(Op.Discard ,"static System.ValueTuple.Create<T1, T2, T3, T4, T5>(T1, T2, T3, T4, T5)")]
	public extern static (T1, T2, T3, T4, T5) _4c097ae606bc8905<T1, T2, T3, T4, T5>(object item1, object item2, object item3, object item4, object item5);

	///<summary>Creates a new value tuple with 6 components (a sexuple).</summary>
	[Jazor(Op.Discard ,"static System.ValueTuple.Create<T1, T2, T3, T4, T5, T6>(T1, T2, T3, T4, T5, T6)")]
	public extern static (T1, T2, T3, T4, T5, T6) _afec461eabd4d8e5<T1, T2, T3, T4, T5, T6>(object item1, object item2, object item3, object item4, object item5, object item6);

	///<summary>Creates a new value tuple with 7 components (a septuple).</summary>
	[Jazor(Op.Discard ,"static System.ValueTuple.Create<T1, T2, T3, T4, T5, T6, T7>(T1, T2, T3, T4, T5, T6, T7)")]
	public extern static (T1, T2, T3, T4, T5, T6, T7) _68093829d7705581<T1, T2, T3, T4, T5, T6, T7>(object item1, object item2, object item3, object item4, object item5, object item6, object item7);

	///<summary>Creates a new value tuple with 8 components (an octuple).</summary>
	[Jazor(Op.Discard ,"static System.ValueTuple.Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1, T2, T3, T4, T5, T6, T7, T8)")]
	public extern static (T1, T2, T3, T4, T5, T6, T7, T8) _8bc5fa3a3cbbcbc7<T1, T2, T3, T4, T5, T6, T7, T8>(object item1, object item2, object item3, object item4, object item5, object item6, object item7, object item8);
}
