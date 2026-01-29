using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("object", "object", WhiteListOp.Allowed)]
public static class ObjectModule
{
    ///<summary>Gets the <see cref="T:System.Type" /> of the current instance.</summary>
    [WhiteList("_393ae40d42f17afb", "object.GetType()", WhiteListOp.Literal, "typeof @#{0}")]
    public extern static System.Type _393ae40d42f17afb(Object instance);

    ///<summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
    [WhiteList("_4aea088b73a04a68", "object.Object()", WhiteListOp.Allowed)]
    public extern static Object _4aea088b73a04a68();

    ///<summary>Returns a string that represents the current object.</summary>
    [WhiteList("_b43835974ba92ea0", "virtual object.ToString()", WhiteListOp.Replace, "toString")]
    public extern static string? _b43835974ba92ea0(Object instance);

    ///<summary>Determines whether the specified object is equal to the current object.</summary>
    [WhiteList("_bfe118282c0f0f45", "virtual object.Equals(object)", WhiteListOp.Literal, "@#{0} === @#{1}")]
    public extern static bool _bfe118282c0f0f45(Object instance, Object? obj);

    ///<summary>Determines whether the specified object instances are considered equal.</summary>
    [WhiteList("_cfcace6be1500e0f", "static object.Equals(object, object)", WhiteListOp.Literal, "@#{0} === @#{1}")]
    public extern static bool _cfcace6be1500e0f(Object? objA, Object? objB);

    ///<summary>Determines whether the specified <see cref="T:System.Object" /> instances are the same instance.</summary>
    [WhiteList("_b7bcdcecb3f79c07", "static object.ReferenceEquals(object, object)", WhiteListOp.Literal, "@#{0} === @#{1}")]
    public extern static bool _b7bcdcecb3f79c07(Object? objA, Object? objB);

    ///<summary>Serves as the default hash function.</summary>
    [WhiteList("_97891de43f43ceb4", "virtual object.GetHashCode()", WhiteListOp.Discard)]
    public extern static Number _97891de43f43ceb4(Object instance);
}
