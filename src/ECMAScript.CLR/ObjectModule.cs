using System.Collections;
using static ECMAScript.CLRModule;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("object","object", "object")]
public static class ObjectModule
{
    ///<summary>Gets the <see cref="T:System.Type" /> of the current instance.</summary>
    ///<returns>The exact runtime type of the current instance.</returns>
    [WhiteList("_393ae40d42f17afb","object.GetType()", "_393ae40d42f17afb")]
	[ECMAScriptLiteral(@"typeof @#{0}")]
	public extern static System.Type _393ae40d42f17afb(Object instance);

    ///<summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
    [WhiteList("_4aea088b73a04a68","object.Object()", "_4aea088b73a04a68")]
	public extern static Object _4aea088b73a04a68();

    ///<summary>Returns a string that represents the current object.</summary>
    ///<returns>A string that represents the current object.</returns>
    [WhiteList("_b43835974ba92ea0", "virtual object.ToString()", "_b43835974ba92ea0")]
	public extern static string? _b43835974ba92ea0(Object instance);

    ///<summary>Determines whether the specified object is equal to the current object.</summary>
    ///<param name="obj">The object to compare with the current object.</param>
    ///<returns>  <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
    [WhiteList("_bfe118282c0f0f45", "virtual object.Equals(object)", "_bfe118282c0f0f45")]
	[ECMAScriptLiteral("@#{0} === @#{1}")]
	public extern static bool _bfe118282c0f0f45(Object instance, Object? obj);

    ///<summary>Determines whether the specified object instances are considered equal.</summary>
    ///<param name="objA">The first object to compare.</param>
    ///<param name="objB">The second object to compare.</param>
    ///<returns>  <see langword="true" /> if the objects are considered equal; otherwise, <see langword="false" />. If both <paramref name="objA" /> and <paramref name="objB" /> are null, the method returns <see langword="true" />.</returns>
    [WhiteList("_cfcace6be1500e0f", "static object.Equals(object, object)", "_cfcace6be1500e0f")]
	[ECMAScriptLiteral("@#{0} === @#{1}")]
	public extern static bool _cfcace6be1500e0f(Object? objA, Object? objB);

    ///<summary>Determines whether the specified <see cref="T:System.Object" /> instances are the same instance.</summary>
    ///<param name="objA">The first object to compare.</param>
    ///<param name="objB">The second object  to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="objA" /> is the same instance as <paramref name="objB" /> or if both are null; otherwise, <see langword="false" />.</returns>
    [WhiteList("_b7bcdcecb3f79c07", "static object.ReferenceEquals(object, object)", "_b7bcdcecb3f79c07")]
	[ECMAScriptLiteral("@#{0} === @#{1}")]
	public extern static bool _b7bcdcecb3f79c07(Object? objA, Object? objB);

    ///<summary>Serves as the default hash function.</summary>
    ///<returns>A hash code for the current object.</returns>
    [WhiteList("_97891de43f43ceb4", "virtual object.GetHashCode()", "_97891de43f43ceb4")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _97891de43f43ceb4(Object instance);
}
