namespace ECMAScript;

/// <summary>
/// 提供与 JavaScript Reflect 对象类似的功能，用于执行反射操作
/// </summary>
[ECMAScript]
[Description("@#Reflect")]
public static class Reflect
{
	/// <summary>
	/// 调用目标函数
	/// </summary>
	/// <param name="target">要调用的目标函数</param>
	/// <param name="thisArg">函数调用时的 this 值</param>
	/// <param name="argumentsList">调用参数列表</param>
	/// <returns></returns>
	[Description("@#apply")]
	public extern static object Apply(object target, object thisArg, object[] argumentsList);

	/// <summary>
	/// 作为构造函数调用目标函数
	/// </summary>
	/// <param name="target">目标构造函数</param>
	/// <param name="argumentsList">构造函数参数列表</param>
	/// <returns></returns>
	[Description("@#construct")]
	public extern static object Construct(object target, object[] argumentsList);

	/// <summary>
	/// 获取对象属性的值
	/// </summary>
	/// <param name="target">目标对象</param>
	/// <param name="propertyKey">属性键</param>
	/// <returns></returns>
	[Description("@#get")]
	public extern static object Get(object target, object propertyKey);

	/// <summary>
	/// 设置对象属性的值
	/// </summary>
	/// <param name="target">目标对象</param>
	/// <param name="propertyKey">属性键</param>
	/// <param name="value">要设置的值</param>
	/// <returns></returns>
	[Description("@#set")]
	public extern static bool Set(object target, object propertyKey, object value);

	/// <summary>
	/// 检查对象是否具有指定属性
	/// </summary>
	/// <param name="target">目标对象</param>
	/// <param name="propertyKey">属性键</param>
	/// <returns></returns>
	[Description("@#has")]
	public extern static bool Has(object target, object propertyKey);
}

