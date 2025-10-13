using System.ComponentModel;
using System.Data;

namespace ECMAScript;


/// <summary>
/// 创建对象的代理
/// </summary>
/// <typeparam name="TTarget">目标对象类型</typeparam>
[ECMAScript]
[Description("@#Proxy")]
public class Proxy<TTarget> where TTarget : class
{
	/// <summary>
	/// 创建一个新的代理实例
	/// </summary>
	/// <param name="target">目标对象</param>
	/// <param name="handler">代理处理器</param>
	public extern Proxy(TTarget target, ProxyHandler<TTarget> handler);

	/// <summary>
	/// 获取代理的目标对象
	/// </summary>
	[Description("@#target")]
	public extern TTarget Target { get; }

	/// <summary>
	/// 获取代理的处理器对象
	/// </summary>
	[Description("@#handler")]
	public extern ProxyHandler<TTarget> Handler { get; }
}

/// <summary>
/// 代理处理器，定义代理的行为
/// </summary>
/// <typeparam name="TTarget">目标对象类型</typeparam>
[ECMAScript]
public abstract class ProxyHandler<TTarget> where TTarget : class
{
	/// <summary>
	/// 拦截属性读取操作，类似于 handler.get()
	/// </summary>
	/// <param name="target">目标对象</param>
	/// <param name="property">属性名</param>
	/// <param name="receiver">接收对象</param>
	/// <returns></returns>
	[Description("@#get")]
	public extern virtual object Get(TTarget target, string property, object receiver);

	/// <summary>
	/// 拦截属性设置操作，类似于 handler.set()
	/// </summary>
	/// <param name="target">目标对象</param>
	/// <param name="property">属性名</param>
	/// <param name="value">要设置的值</param>
	/// <param name="receiver">接收对象</param>
	/// <returns></returns>
	[Description("@#set")]
	public extern virtual bool Set(TTarget target, string property, object value, object receiver);

	/// <summary>
	/// 拦截 in 操作符，类似于 handler.has()
	/// </summary>
	/// <param name="target">目标对象</param>
	/// <param name="property">属性名</param>
	/// <returns></returns>
	[Description("@#has")]
	public extern virtual bool Has(TTarget target, string property);

	/// <summary>
	/// 拦截函数调用操作，类似于 handler.apply()
	/// </summary>
	/// <param name="target">目标对象</param>
	/// <param name="thisArg">this 值</param>
	/// <param name="argumentsList">参数列表</param>
	/// <returns></returns>
	[Description("@#apply")]
	public extern virtual object Apply(TTarget target, object thisArg, object[] argumentsList);
}

