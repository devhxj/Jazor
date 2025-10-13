using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Nullable")]
public static class NullableModule
{
    ///<summary>Compares the relative values of two <see cref="T:System.Nullable`1" /> objects.</summary>
    ///<param name="n1">A <see cref="T:System.Nullable`1" /> object.</param>
    ///<param name="n2">A <see cref="T:System.Nullable`1" /> object.</param>
    ///<typeparam name="T">The underlying value type of the <paramref name="n1" /> and <paramref name="n2" /> parameters.</typeparam>
    ///<returns>An integer that indicates the relative values of the <paramref name="n1" /> and <paramref name="n2" /> parameters. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> The <see cref="P:System.Nullable`1.HasValue" /> property for <paramref name="n1" /> is <see langword="false" />, and the <see cref="P:System.Nullable`1.HasValue" /> property for <paramref name="n2" /> is <see langword="true" />, or the <see cref="P:System.Nullable`1.HasValue" /> properties for <paramref name="n1" /> and <paramref name="n2" /> are <see langword="true" />, and the value of the <see cref="P:System.Nullable`1.Value" /> property for <paramref name="n1" /> is less than the value of the <see cref="P:System.Nullable`1.Value" /> property for <paramref name="n2" />.</description></item><item><term> Zero</term><description> The <see cref="P:System.Nullable`1.HasValue" /> properties for <paramref name="n1" /> and <paramref name="n2" /> are <see langword="false" />, or the <see cref="P:System.Nullable`1.HasValue" /> properties for <paramref name="n1" /> and <paramref name="n2" /> are <see langword="true" />, and the value of the <see cref="P:System.Nullable`1.Value" /> property for <paramref name="n1" /> is equal to the value of the <see cref="P:System.Nullable`1.Value" /> property for <paramref name="n2" />.</description></item><item><term> Greater than zero</term><description> The <see cref="P:System.Nullable`1.HasValue" /> property for <paramref name="n1" /> is <see langword="true" />, and the <see cref="P:System.Nullable`1.HasValue" /> property for <paramref name="n2" /> is <see langword="false" />, or the <see cref="P:System.Nullable`1.HasValue" /> properties for <paramref name="n1" /> and <paramref name="n2" /> are <see langword="true" />, and the value of the <see cref="P:System.Nullable`1.Value" /> property for <paramref name="n1" /> is greater than the value of the <see cref="P:System.Nullable`1.Value" /> property for <paramref name="n2" />.</description></item></list></returns>    [WhiteList("static System.Nullable.Compare<T>(T?, T?)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number NullableCompare<T>(T? n1, T? n2);

    ///<summary>Indicates whether two specified <see cref="T:System.Nullable`1" /> objects are equal.</summary>
    ///<param name="n1">A <see cref="T:System.Nullable`1" /> object.</param>
    ///<param name="n2">A <see cref="T:System.Nullable`1" /> object.</param>
    ///<typeparam name="T">The underlying value type of the <paramref name="n1" /> and <paramref name="n2" /> parameters.</typeparam>
    ///<returns>  <see langword="true" /> if the <paramref name="n1" /> parameter is equal to the <paramref name="n2" /> parameter; otherwise, <see langword="false" />. The return value depends on the <see cref="P:System.Nullable`1.HasValue" /> and <see cref="P:System.Nullable`1.Value" /> properties of the two parameters that are compared. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term><see langword="true" /></term><description> The <see cref="P:System.Nullable`1.HasValue" /> properties for <paramref name="n1" /> and <paramref name="n2" /> are <see langword="false" />, or the <see cref="P:System.Nullable`1.HasValue" /> properties for <paramref name="n1" /> and <paramref name="n2" /> are <see langword="true" />, and the <see cref="P:System.Nullable`1.Value" /> properties of the parameters are equal.</description></item><item><term><see langword="false" /></term><description> The <see cref="P:System.Nullable`1.HasValue" /> property is <see langword="true" /> for one parameter and <see langword="false" /> for the other parameter, or the <see cref="P:System.Nullable`1.HasValue" /> properties for <paramref name="n1" /> and <paramref name="n2" /> are <see langword="true" />, and the <see cref="P:System.Nullable`1.Value" /> properties of the parameters are unequal.</description></item></list></returns>    [WhiteList("static System.Nullable.Equals<T>(T?, T?)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool NullableEquals<T>(T? n1, T? n2);

    ///<summary>Returns the underlying type argument of the specified nullable type.</summary>
    ///<param name="nullableType">A <see cref="T:System.Type" /> object that describes a closed generic nullable type.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="nullableType" /> is <see langword="null" />.</exception>
    ///<returns>The type argument of the <paramref name="nullableType" /> parameter, if the <paramref name="nullableType" /> parameter is a closed generic nullable type; otherwise, <see langword="null" />.</returns>    [WhiteList("static System.Nullable.GetUnderlyingType(System.Type)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Type? NullableGetUnderlyingType(System.Type nullableType);

    ///<summary>Retrieves a readonly reference to the location in the <see cref="T:System.Nullable`1" /> instance where the value is stored.</summary>
    ///<param name="nullable">The readonly reference to the input <see cref="T:System.Nullable`1" /> value.</param>
    ///<typeparam name="T">The underlying value type of the <see cref="T:System.Nullable`1" /> generic type.</typeparam>
    ///<returns>A readonly reference to the location where the instance's <typeparamref name="T" /> value is stored. If the instance's <see cref="P:System.Nullable`1.HasValue" /> is <see langword="false" />, the current value at that location may be the default value.</returns>    [WhiteList("static System.Nullable.GetValueRefOrDefaultRef<T>(ref readonly T?)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static T NullableGetValueRefOrDefaultRef<T>(ref readonly T? nullable);
}
