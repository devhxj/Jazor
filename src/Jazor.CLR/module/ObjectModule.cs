namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "object", "System/ObjectModule.js")]
public static class ObjectModule
{
	[Jazor(Op.Discard, "object.GetType()")]
	public extern static Object _393ae40d42f17afb();

	[Jazor(Op.Discard, "object.Object()")]
	public extern static Object _4aea088b73a04a68();

	[Jazor(Op.Discard, "virtual object.ToString()")]
	public extern static String _b43835974ba92ea0();

	[Jazor(Op.Compile, "virtual object.Equals(object)")]
	public extern static Boolean _bfe118282c0f0f45(Object instance, Object obj);

	[Jazor(Op.Compile, "static object.Equals(object, object)")]
	public extern static Boolean _cfcace6be1500e0f(Object obj, Object obj1);

	[Jazor(Op.Compile, "static object.ReferenceEquals(object, object)")]
	public extern static Boolean _b7bcdcecb3f79c07(Object obj, Object obj1);

	[Jazor(Op.Discard, "virtual object.GetHashCode()")]
	public extern static Number _97891de43f43ceb4();

}