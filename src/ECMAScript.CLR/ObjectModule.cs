using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Global;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("object")]
public static class ObjectModule
{
	///<summary>Gets the <see cref="T:System.Type" /> of the current instance.</summary>
	///<returns>The exact runtime type of the current instance.</returns>
	[WhiteList("object.GetType()")]
	[Obsolete("Not Support in Jazor", true)]
	public extern static Type ObjectGetType(object instance);

	///<summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
	[WhiteList("object.Object()")]
	public extern static object ObjectNew();

	///<summary>Returns a string that represents the current object.</summary>
	///<returns>A string that represents the current object.</returns>
	[WhiteList("virtual object.ToString()")]
	public extern static string? ObjectToString(object instance);

	///<summary>Determines whether the specified object is equal to the current object.</summary>
	///<param name="obj">The object to compare with the current object.</param>
	///<returns>  <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
	[WhiteList("virtual object.Equals(object)")]
	[WhiteList("static object.Equals(object, object)")]
	[WhiteList("static object.ReferenceEquals(object, object)")]
	[ECMAScriptLiteral("{0} === {1}")]
	public extern static bool ObjectEquals(object instance, object? obj);

    ///<summary>Serves as the default hash function.</summary>
    ///<returns>A hash code for the current object.</returns>
    [WhiteList("virtual object.GetHashCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public static Number ObjectGetHashCode(object instance)
	{
		//待补充实现
		throw new Error("NotImplemented");
	}

	//public Number hashStringToInt(string str)
	//{
	//	var hash = 0;
	//	for (var i = 0; i < str.Length; i++)
	//	{
	//		var code = str.CharCodeAt(i);
	//		hash = (hash << 5) - hash + code;
	//		hash = hash & hash; // 将hash转换为32位整数
	//	}
	//	return hash;
	//}

	//private static Number ObjectHash(object? obj, WeakMap<object, Number>? visited)
	//{
	//	if (obj is null)
	//		return 0;

	//	if (TypeOf(obj) == "object")
	//	{
	//		visited ??= new();
	//		if (visited.Has(obj))
	//			return visited.Get(obj);

	//		Number hash;
	//		if (IArray.IsArray(obj))
	//		{
	//			hash = ((Array<object?>)obj).Map(item => ObjectHash(item, visited)).Join('|');
	//		}
	//		else
	//		{
	//			var keys = object.Keys(obj).Sort();
	//			hash = keys.Map(key => `${ key}:${ objectHash(obj[key], visited)}`).join('|');
	//		}
	//		visited.Set(obj, hash);
	//		return hashStringToInt(hash);
	//	}
	//	else if (typeof obj === 'function')
	//	{
	//		return hashStringToInt(obj.toString());
	//	}
	//	else
	//	{
	//		return hashStringToInt(String(obj));
	//	}
	//}
}
