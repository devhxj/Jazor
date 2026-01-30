using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("object", WhiteListOp.Allowed, "System/ObjectModule.js")]
public static class ObjectModule
{
    ///<summary>Gets the <see cref="T:System.Type" /> of the current instance.</summary>
    [WhiteList("object.GetType()", WhiteListOp.GetType)]
    public extern static System.Type _393ae40d42f17afb(Object instance);

    ///<summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
    [WhiteList("object.Object()", WhiteListOp.Allowed)]
    public extern static Object _4aea088b73a04a68();

    ///<summary>Returns a string that represents the current object.</summary>
    [WhiteList("virtual object.ToString()", WhiteListOp.ToString)]
    public extern static string? _b43835974ba92ea0(Object instance);

    ///<summary>Determines whether the specified object is equal to the current object.</summary>
    [WhiteList("virtual object.Equals(object)", WhiteListOp.Equals)]
    public extern static bool _bfe118282c0f0f45(Object instance, Object? obj);

    ///<summary>Determines whether the specified object instances are considered equal.</summary>
    [WhiteList("static object.Equals(object, object)", WhiteListOp.Equals)]
    public extern static bool _cfcace6be1500e0f(Object? objA, Object? objB);

    ///<summary>Determines whether the specified <see cref="T:System.Object" /> instances are the same instance.</summary>
    [WhiteList("static object.ReferenceEquals(object, object)", WhiteListOp.Equals)]
    public extern static bool _b7bcdcecb3f79c07(Object? objA, Object? objB);

    ///<summary>Serves as the default hash function.</summary>
    [WhiteList("virtual object.GetHashCode()", WhiteListOp.Discard)]
    public extern static Number _97891de43f43ceb4(Object instance);
}
