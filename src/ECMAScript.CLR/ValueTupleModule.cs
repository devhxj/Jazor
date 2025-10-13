using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.ValueTuple")]
public static class ValueTupleModule
{
    [WhiteList("System.ValueTuple.ValueTuple()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.ValueTuple ValueTupleNew();

    ///<summary>Returns a value that indicates whether the current <see cref="T:System.ValueTuple" /> instance is equal to a specified object.</summary>
    ///<param name="obj">The object to compare to the current instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="obj" /> is a <see cref="T:System.ValueTuple" /> instance; otherwise, <see langword="false" />.</returns>    [WhiteList("override System.ValueTuple.Equals(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ValueTupleEquals(System.ValueTuple instance, Object? obj);

    ///<summary>Determines whether two <see cref="T:System.ValueTuple" /> instances are equal. This method always returns <see langword="true" />.</summary>
    ///<param name="other">The value tuple to compare with the current instance.</param>
    ///<returns>This method always returns <see langword="true" />.</returns>    [WhiteList("System.ValueTuple.Equals(System.ValueTuple)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ValueTupleEquals2(System.ValueTuple instance, System.ValueTuple other);

    ///<summary>Compares the current <see cref="T:System.ValueTuple" /> instance to a specified <see cref="T:System.ValueTuple" /> instance.</summary>
    ///<param name="other">The object to compare with the current instance.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="other" /> is not a <see cref="T:System.ValueTuple" /> instance.</exception>
    ///<returns>This method always returns 0.</returns>    [WhiteList("System.ValueTuple.CompareTo(System.ValueTuple)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ValueTupleCompareTo(System.ValueTuple instance, System.ValueTuple other);

    ///<summary>Returns the hash code for the current <see cref="T:System.ValueTuple" /> instance.</summary>
    ///<returns>This method always return 0.</returns>    [WhiteList("override System.ValueTuple.GetHashCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ValueTupleGetHashCode(System.ValueTuple instance);

    ///<summary>Returns the string representation of this <see cref="T:System.ValueTuple" /> instance.</summary>
    ///<returns>This method always returns "()".</returns>    [WhiteList("override System.ValueTuple.ToString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string ValueTupleToString(System.ValueTuple instance);

    ///<summary>Creates a new value tuple with zero components.</summary>
    ///<returns>A new value tuple with no components.</returns>    [WhiteList("static System.ValueTuple.Create()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.ValueTuple ValueTupleCreate();

    ///<summary>Creates a new value tuple with 1 component (a singleton).</summary>
    ///<param name="item1">The value of the value tuple's only component.</param>
    ///<typeparam name="T1">The type of the value tuple's only component.</typeparam>
    ///<returns>A value tuple with 1 component.</returns>    [WhiteList("static System.ValueTuple.Create<T1>(T1)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.ValueTuple<T1> ValueTupleCreate2<T1>(T1 item1);

    ///<summary>Creates a new value tuple with 2 components (a pair).</summary>
    ///<param name="item1">The value of the value tuple's first component.</param>
    ///<param name="item2">The value of the value tuple's second component.</param>
    ///<typeparam name="T1">The type of the value tuple's first component.</typeparam>
    ///<typeparam name="T2">The type of the value tuple's second component.</typeparam>
    ///<returns>A value tuple with 2 components.</returns>    [WhiteList("static System.ValueTuple.Create<T1, T2>(T1, T2)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static (T1, T2) ValueTupleCreate3<T1, T2>(T1 item1, T2 item2);

    ///<summary>Creates a new value tuple with 3 components (a triple).</summary>
    ///<param name="item1">The value of the value tuple's first component.</param>
    ///<param name="item2">The value of the value tuple's second component.</param>
    ///<param name="item3">The value of the value tuple's third component.</param>
    ///<typeparam name="T1">The type of the value tuple's first component.</typeparam>
    ///<typeparam name="T2">The type of the value tuple's second component.</typeparam>
    ///<typeparam name="T3">The type of the value tuple's third component.</typeparam>
    ///<returns>A value tuple with 3 components.</returns>    [WhiteList("static System.ValueTuple.Create<T1, T2, T3>(T1, T2, T3)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static (T1, T2, T3) ValueTupleCreate4<T1, T2, T3>(T1 item1, T2 item2, T3 item3);

    ///<summary>Creates a new value tuple with 4 components (a quadruple).</summary>
    ///<param name="item1">The value of the value tuple's first component.</param>
    ///<param name="item2">The value of the value tuple's second component.</param>
    ///<param name="item3">The value of the value tuple's third component.</param>
    ///<param name="item4">The value of the value tuple's fourth component.</param>
    ///<typeparam name="T1">The type of the value tuple's first component.</typeparam>
    ///<typeparam name="T2">The type of the value tuple's second component.</typeparam>
    ///<typeparam name="T3">The type of the value tuple's third component.</typeparam>
    ///<typeparam name="T4">The type of the value tuple's fourth component.</typeparam>
    ///<returns>A value tuple with 4 components.</returns>    [WhiteList("static System.ValueTuple.Create<T1, T2, T3, T4>(T1, T2, T3, T4)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static (T1, T2, T3, T4) ValueTupleCreate5<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4);

    ///<summary>Creates a new value tuple with 5 components (a quintuple).</summary>
    ///<param name="item1">The value of the value tuple's first component.</param>
    ///<param name="item2">The value of the value tuple's second component.</param>
    ///<param name="item3">The value of the value tuple's third component.</param>
    ///<param name="item4">The value of the value tuple's fourth component.</param>
    ///<param name="item5">The value of the value tuple's fifth component.</param>
    ///<typeparam name="T1">The type of the value tuple's first component.</typeparam>
    ///<typeparam name="T2">The type of the value tuple's second component.</typeparam>
    ///<typeparam name="T3">The type of the value tuple's third component.</typeparam>
    ///<typeparam name="T4">The type of the value tuple's fourth component.</typeparam>
    ///<typeparam name="T5">The type of the value tuple's fifth component.</typeparam>
    ///<returns>A value tuple with 5 components.</returns>    [WhiteList("static System.ValueTuple.Create<T1, T2, T3, T4, T5>(T1, T2, T3, T4, T5)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static (T1, T2, T3, T4, T5) ValueTupleCreate6<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5);

    ///<summary>Creates a new value tuple with 6 components (a sexuple).</summary>
    ///<param name="item1">The value of the value tuple's first component.</param>
    ///<param name="item2">The value of the value tuple's second component.</param>
    ///<param name="item3">The value of the value tuple's third component.</param>
    ///<param name="item4">The value of the value tuple's fourth component.</param>
    ///<param name="item5">The value of the value tuple's fifth component.</param>
    ///<param name="item6">The value of the value tuple's sixth component.</param>
    ///<typeparam name="T1">The type of the value tuple's first component.</typeparam>
    ///<typeparam name="T2">The type of the value tuple's second component.</typeparam>
    ///<typeparam name="T3">The type of the value tuple's third component.</typeparam>
    ///<typeparam name="T4">The type of the value tuple's fourth component.</typeparam>
    ///<typeparam name="T5">The type of the value tuple's fifth component.</typeparam>
    ///<typeparam name="T6">The type of the value tuple's sixth component.</typeparam>
    ///<returns>A value tuple with 6 components.</returns>    [WhiteList("static System.ValueTuple.Create<T1, T2, T3, T4, T5, T6>(T1, T2, T3, T4, T5, T6)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static (T1, T2, T3, T4, T5, T6) ValueTupleCreate7<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6);

    ///<summary>Creates a new value tuple with 7 components (a septuple).</summary>
    ///<param name="item1">The value of the value tuple's first component.</param>
    ///<param name="item2">The value of the value tuple's second component.</param>
    ///<param name="item3">The value of the value tuple's third component.</param>
    ///<param name="item4">The value of the value tuple's fourth component.</param>
    ///<param name="item5">The value of the value tuple's fifth component.</param>
    ///<param name="item6">The value of the value tuple's sixth component.</param>
    ///<param name="item7">The value of the value tuple's seventh component.</param>
    ///<typeparam name="T1">The type of the value tuple's first component.</typeparam>
    ///<typeparam name="T2">The type of the value tuple's second component.</typeparam>
    ///<typeparam name="T3">The type of the value tuple's third component.</typeparam>
    ///<typeparam name="T4">The type of the value tuple's fourth component.</typeparam>
    ///<typeparam name="T5">The type of the value tuple's fifth component.</typeparam>
    ///<typeparam name="T6">The type of the value tuple's sixth component.</typeparam>
    ///<typeparam name="T7">The type of the value tuple's seventh component.</typeparam>
    ///<returns>A value tuple with 7 components.</returns>    [WhiteList("static System.ValueTuple.Create<T1, T2, T3, T4, T5, T6, T7>(T1, T2, T3, T4, T5, T6, T7)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static (T1, T2, T3, T4, T5, T6, T7) ValueTupleCreate8<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7);

    ///<summary>Creates a new value tuple with 8 components (an octuple).</summary>
    ///<param name="item1">The value of the value tuple's first component.</param>
    ///<param name="item2">The value of the value tuple's second component.</param>
    ///<param name="item3">The value of the value tuple's third component.</param>
    ///<param name="item4">The value of the value tuple's fourth component.</param>
    ///<param name="item5">The value of the value tuple's fifth component.</param>
    ///<param name="item6">The value of the value tuple's sixth component.</param>
    ///<param name="item7">The value of the value tuple's seventh component.</param>
    ///<param name="item8">The value of the value tuple's eighth component.</param>
    ///<typeparam name="T1">The type of the value tuple's first component.</typeparam>
    ///<typeparam name="T2">The type of the value tuple's second component.</typeparam>
    ///<typeparam name="T3">The type of the value tuple's third component.</typeparam>
    ///<typeparam name="T4">The type of the value tuple's fourth component.</typeparam>
    ///<typeparam name="T5">The type of the value tuple's fifth component.</typeparam>
    ///<typeparam name="T6">The type of the value tuple's sixth component.</typeparam>
    ///<typeparam name="T7">The type of the value tuple's seventh component.</typeparam>
    ///<typeparam name="T8">The type of the value tuple's eighth component.</typeparam>
    ///<returns>A value tuple with 8 components.</returns>    [WhiteList("static System.ValueTuple.Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1, T2, T3, T4, T5, T6, T7, T8)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static (T1, T2, T3, T4, T5, T6, T7, T8) ValueTupleCreate9<T1, T2, T3, T4, T5, T6, T7, T8>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8);
}
