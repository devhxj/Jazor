namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "object","System/ObjectModule.js")]
public static class ObjectModule
{
	///<summary>Gets the <see cref="T:System.Type" /> of the current instance.</summary>
	[Jazor(Op.Discard ,"object.GetType()")]
	public extern static System.Type _393ae40d42f17afb(object instance);

	///<summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
	[Jazor(Op.Discard ,"object.Object()")]
	public extern static object _4aea088b73a04a68();

	///<summary>Returns a string that represents the current object.</summary>
	[Jazor(Op.Discard ,"virtual object.ToString()")]
	public extern static string? _b43835974ba92ea0(object instance);

	///<summary>Determines whether the specified object is equal to the current object.</summary>
	[Jazor(Op.Discard ,"virtual object.Equals(object)")]
	public extern static bool _bfe118282c0f0f45(object instance, object? obj);

	///<summary>Determines whether the specified object instances are considered equal.</summary>
	[Jazor(Op.Discard ,"static object.Equals(object, object)")]
	public extern static bool _cfcace6be1500e0f(object? objA, object? objB);

	///<summary>Determines whether the specified <see cref="T:System.Object" /> instances are the same instance.</summary>
	[Jazor(Op.Discard ,"static object.ReferenceEquals(object, object)")]
	public extern static bool _b7bcdcecb3f79c07(object? objA, object? objB);

	///<summary>Serves as the default hash function.</summary>
	[Jazor(Op.Discard ,"virtual object.GetHashCode()")]
	public extern static Number _97891de43f43ceb4(object instance);
}
