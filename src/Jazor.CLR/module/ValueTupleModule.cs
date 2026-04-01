namespace Jazor.CLR;

[ECMAScriptModule("System/ValueTupleModule.js")]
[Jazor(Op.Alias, "System.ValueTuple","Object")]
public static class ValueTupleModule
{
	/// <summary>
	/// C#: new ValueTuple() - empty tuple
	/// JS: null (represents empty tuple)
	/// </summary>
	[Jazor(Op.Inline, "System.ValueTuple.ValueTuple()", "null")]
	public extern static System.ValueTuple _afe5e7b03405c9fc();

	/// <summary>
	/// C#: instance.Equals(obj)
	/// JS: obj === null (empty tuple only equals null)
	/// </summary>
	[Jazor(Op.Inline, "override System.ValueTuple.Equals(object)", "(__arg2 === null)")]
	public extern static bool _f405bb1d41845d0a(System.ValueTuple instance, object? obj);

	/// <summary>
	/// C#: instance.Equals(other)
	/// JS: true (all empty tuples are equal)
	/// </summary>
	[Jazor(Op.Inline, "System.ValueTuple.Equals(System.ValueTuple)", "true")]
	public extern static bool _075aabd97b9153e6(System.ValueTuple instance, object other);

	/// <summary>
	/// C#: instance.CompareTo(other)
	/// JS: 0 (all empty tuples are equal)
	/// </summary>
	[Jazor(Op.Inline, "System.ValueTuple.CompareTo(System.ValueTuple)", "0")]
	public extern static Number _f92b072b1ea77fb3(System.ValueTuple instance, object other);

	/// <summary>
	/// C#: instance.GetHashCode()
	/// JS: 0
	/// </summary>
	[Jazor(Op.Inline, "override System.ValueTuple.GetHashCode()", "0")]
	public extern static Number _79b4fb9a3ea0524a(System.ValueTuple instance);

	/// <summary>
	/// C#: instance.ToString()
	/// JS: "()"
	/// </summary>
	[Jazor(Op.Inline, "override System.ValueTuple.ToString()", "'()'")]
	public extern static string _93b143a10f6cb207(System.ValueTuple instance);

	/// <summary>
	/// C#: ValueTuple.Create()
	/// JS: null
	/// </summary>
	[Jazor(Op.Inline, "static System.ValueTuple.Create()", "null")]
	public extern static System.ValueTuple _b2020d347b181140();

	/// <summary>
	/// C#: ValueTuple.Create(item1)
	/// JS: [item1]
	/// </summary>
	[Jazor(Op.Inline, "static System.ValueTuple.Create<T1>(T1)", "[__arg1]")]
	public extern static System.ValueTuple<T1> _c01432b1ceab8949<T1>(object item1);

	/// <summary>
	/// C#: ValueTuple.Create(item1, item2)
	/// JS: [item1, item2]
	/// </summary>
	[Jazor(Op.Inline, "static System.ValueTuple.Create<T1, T2>(T1, T2)", "[__arg1, __arg2]")]
	public extern static (T1, T2) _3c42e78c6d0ddf68<T1, T2>(object item1, object item2);

	/// <summary>
	/// C#: ValueTuple.Create(item1, item2, item3)
	/// JS: [item1, item2, item3]
	/// </summary>
	[Jazor(Op.Inline, "static System.ValueTuple.Create<T1, T2, T3>(T1, T2, T3)", "[__arg1, __arg2, __arg3]")]
	public extern static (T1, T2, T3) _6462161c42aa6ac1<T1, T2, T3>(object item1, object item2, object item3);

	/// <summary>
	/// C#: ValueTuple.Create(item1, item2, item3, item4)
	/// JS: [item1, item2, item3, item4]
	/// </summary>
	[Jazor(Op.Inline, "static System.ValueTuple.Create<T1, T2, T3, T4>(T1, T2, T3, T4)", "[__arg1, __arg2, __arg3, __arg4]")]
	public extern static (T1, T2, T3, T4) _7d9afb217b6c02e6<T1, T2, T3, T4>(object item1, object item2, object item3, object item4);

	/// <summary>
	/// C#: ValueTuple.Create(item1, item2, item3, item4, item5)
	/// JS: [item1, item2, item3, item4, item5]
	/// </summary>
	[Jazor(Op.Inline, "static System.ValueTuple.Create<T1, T2, T3, T4, T5>(T1, T2, T3, T4, T5)", "[__arg1, __arg2, __arg3, __arg4, __arg5]")]
	public extern static (T1, T2, T3, T4, T5) _4c097ae606bc8905<T1, T2, T3, T4, T5>(object item1, object item2, object item3, object item4, object item5);

	/// <summary>
	/// C#: ValueTuple.Create(item1, item2, item3, item4, item5, item6)
	/// JS: [item1, item2, item3, item4, item5, item6]
	/// </summary>
	[Jazor(Op.Inline, "static System.ValueTuple.Create<T1, T2, T3, T4, T5, T6>(T1, T2, T3, T4, T5, T6)", "[__arg1, __arg2, __arg3, __arg4, __arg5, __arg6]")]
	public extern static (T1, T2, T3, T4, T5, T6) _afec461eabd4d8e5<T1, T2, T3, T4, T5, T6>(object item1, object item2, object item3, object item4, object item5, object item6);

	/// <summary>
	/// C#: ValueTuple.Create(item1, item2, item3, item4, item5, item6, item7)
	/// JS: [item1, item2, item3, item4, item5, item6, item7]
	/// </summary>
	[Jazor(Op.Inline, "static System.ValueTuple.Create<T1, T2, T3, T4, T5, T6, T7>(T1, T2, T3, T4, T5, T6, T7)", "[__arg1, __arg2, __arg3, __arg4, __arg5, __arg6, __arg7]")]
	public extern static (T1, T2, T3, T4, T5, T6, T7) _68093829d7705581<T1, T2, T3, T4, T5, T6, T7>(object item1, object item2, object item3, object item4, object item5, object item6, object item7);

	/// <summary>
	/// C#: ValueTuple.Create(item1, item2, item3, item4, item5, item6, item7, item8)
	/// JS: [item1, item2, item3, item4, item5, item6, item7, item8]
	/// </summary>
	[Jazor(Op.Inline, "static System.ValueTuple.Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1, T2, T3, T4, T5, T6, T7, T8)", "[__arg1, __arg2, __arg3, __arg4, __arg5, __arg6, __arg7, __arg8]")]
	public extern static (T1, T2, T3, T4, T5, T6, T7, T8) _8bc5fa3a3cbbcbc7<T1, T2, T3, T4, T5, T6, T7, T8>(object item1, object item2, object item3, object item4, object item5, object item6, object item7, object item8);
}
