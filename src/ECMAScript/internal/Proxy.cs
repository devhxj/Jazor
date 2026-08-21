using System.ComponentModel;
using PropertyKey = ECMAScript.JazorPropertyKey;

namespace ECMAScript;

/// <summary>
/// JavaScript object shape returned by <c>Proxy.revocable()</c>.
/// This stays explicit because JavaScript returns a record-like object containing both the proxy and its paired revoke callback.
/// JavaScript <c>Proxy.revocable()</c> 返回的对象形状；它同时包含代理和配对的 revoke 回调，因此保持显式模型。
/// </summary>
/// <typeparam name="TTarget">Static CLR view of the proxied target object. 被代理目标对象的静态 CLR 视图。</typeparam>
[ECMAScript]
[Description("@#")]
public sealed class RevocableProxy<TTarget> where TTarget : class
{
	/// <summary>
	/// Gets the revocable JavaScript proxy instance.
	/// 获取可撤销的 JavaScript 代理实例。
	/// </summary>
	[Description("@#proxy")]
	public extern Proxy<TTarget> Proxy { get; }

	/// <summary>
	/// Gets the action that revokes the proxy so future operations fail according to JavaScript proxy semantics.
	/// 获取撤销代理的操作；撤销后续操作会按 JavaScript 代理语义失败。
	/// </summary>
	[Description("@#revoke")]
	public extern Action Revoke { get; }
}

/// <summary>
/// Static JavaScript <c>Proxy</c> host members.
/// This stays separate from <see cref="Proxy{TTarget}"/> so the runtime static API can be modeled without inventing a CLR-only wrapper type.
/// JavaScript <c>Proxy</c> 静态宿主成员；与 <see cref="Proxy{TTarget}"/> 分离，可建模运行时静态 API 而不引入仅 CLR 的包装类型。
/// </summary>
[ECMAScript]
[Description("@#Proxy")]
public static class Proxy
{
	/// <summary>
	/// Creates a revocable JavaScript proxy together with its paired revoke callback.
	/// This models JavaScript <c>Proxy.revocable(target, handler)</c> directly on the <c>Proxy</c> host.
	/// 创建可撤销 JavaScript 代理及其配对 revoke 回调；直接在 <c>Proxy</c> 宿主上映射 <c>Proxy.revocable(target, handler)</c>。
	/// </summary>
	[Description("@#revocable")]
	public extern static RevocableProxy<TTarget> Revocable<TTarget>(TTarget target, ProxyHandler<TTarget> handler) where TTarget : class;
}

/// <summary>
/// Projection of JavaScript's <c>Proxy</c> constructor.
/// JavaScript <c>Proxy</c> 构造器投影。
/// </summary>
/// <typeparam name="TTarget">Static CLR view of the proxied target object. 被代理目标对象的静态 CLR 视图。</typeparam>
[ECMAScript]
[Description("@#Proxy")]
public sealed class Proxy<TTarget> where TTarget : class
{
	/// <summary>
	/// Creates a JavaScript proxy for the supplied target and handler.
	/// Proxy traps must preserve JavaScript proxy invariants for non-configurable and non-extensible target state.
	/// 使用给定目标和 handler 创建 JavaScript 代理；代理 trap 必须保持 JavaScript 对不可配置及不可扩展目标状态的约束。
	/// </summary>
	public extern Proxy(TTarget target, ProxyHandler<TTarget> handler);

	/// <summary>
	/// Creates a JavaScript proxy with an object-shaped mutation handler.
	/// This form avoids external host inheritance when callers only need to guard writes.
	/// 使用对象形状的修改 handler 创建 JavaScript 代理；调用方仅需控制写入时，此形式避免外部宿主继承。
	/// </summary>
	public extern Proxy(TTarget target, ProxyMutationHandler<TTarget> handler);
}

/// <summary>
/// Strongly typed object-shaped subset of JavaScript Proxy traps for write interception.
/// This is useful when a host needs mutation policy but no read or invocation interception.
/// 用于写入拦截的 JavaScript Proxy trap 强类型对象子集；适合只需要修改策略而不拦截读取或调用的宿主。
/// </summary>
/// <typeparam name="TTarget">Static CLR view of the proxied target object. 被代理目标对象的静态 CLR 视图。</typeparam>
[ECMAScript]
[Description("@#")]
public sealed class ProxyMutationHandler<TTarget> where TTarget : class
{
	/// <summary>
	/// Gets the optional trap for property reads so mutation-only handlers remain compact.
	/// 获取可选的属性读取 trap，使仅修改的 handler 保持精简。
	/// </summary>
	[Description("@#get")]
	public Func<TTarget, JazorPropertyKey, object, object?>? Get { get; init; }

	/// <summary>Gets the optional trap for property writes. 获取可选的属性写入 trap。</summary>
	[Description("@#set")]
	public Func<TTarget, JazorPropertyKey, object?, object, bool>? Set { get; init; }

	/// <summary>Gets the optional trap for deleting an own property. 获取可选的删除自身属性 trap。</summary>
	[Description("@#deleteProperty")]
	public Func<TTarget, JazorPropertyKey, bool>? DeleteProperty { get; init; }

	/// <summary>Gets the optional trap for defining or reconfiguring an own property. 获取可选的定义或重新配置自身属性 trap。</summary>
	[Description("@#defineProperty")]
	public Func<TTarget, JazorPropertyKey, JazorPropertyDescriptor, bool>? DefineProperty { get; init; }
}

/// <summary>
/// Bridge type used to declare JavaScript proxy traps.
/// This type itself is not a JavaScript runtime host.
/// 用于声明 JavaScript 代理 trap 的桥接类型；类型自身不是 JavaScript 运行时宿主。
/// </summary>
/// <typeparam name="TTarget">Static CLR view of the proxied target object. 被代理目标对象的静态 CLR 视图。</typeparam>
[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class ProxyHandler<TTarget> where TTarget : class
{
	/// <summary>
	/// Trap for property reads. 属性读取 trap。
	/// </summary>
	[Description("@#get")]
	public extern virtual object? Get(TTarget target, JazorPropertyKey property, object receiver);

	/// <summary>
	/// Trap for property writes. 属性写入 trap。
	/// </summary>
	[Description("@#set")]
	public extern virtual bool Set(TTarget target, JazorPropertyKey property, object? value, object receiver);

	/// <summary>
	/// Trap for deleting an own property. 删除自身属性 trap。
	/// </summary>
	[Description("@#deleteProperty")]
	public extern virtual bool DeleteProperty(TTarget target, JazorPropertyKey property);

	/// <summary>
	/// Trap for defining or reconfiguring an own property. 定义或重新配置自身属性 trap。
	/// </summary>
	[Description("@#defineProperty")]
	public extern virtual bool DefineProperty(TTarget target, JazorPropertyKey property, JazorPropertyDescriptor attributes);

	/// <summary>
	/// Trap for reading an own property descriptor. 读取自身属性描述符 trap。
	/// </summary>
	[Description("@#getOwnPropertyDescriptor")]
	public extern virtual JazorPropertyDescriptor? GetOwnPropertyDescriptor(TTarget target, JazorPropertyKey property);

	/// <summary>
	/// Trap for enumerating own property keys, including symbols. 枚举自身属性键（包括 Symbol）的 trap。
	/// </summary>
	[Description("@#ownKeys")]
	public extern virtual Array<JazorPropertyKey> OwnKeys(TTarget target);

	/// <summary>
	/// Trap for reading the proxy target prototype. 读取代理目标原型 trap。
	/// </summary>
	[Description("@#getPrototypeOf")]
	public extern virtual IObject? GetPrototypeOf(TTarget target);

	/// <summary>
	/// Trap for updating the proxy target prototype. 更新代理目标原型 trap。
	/// </summary>
	[Description("@#setPrototypeOf")]
	public extern virtual bool SetPrototypeOf(TTarget target, object? prototype);

	/// <summary>
	/// Trap for checking whether the target remains extensible. 检查目标是否仍可扩展的 trap。
	/// </summary>
	[Description("@#isExtensible")]
	public extern virtual bool IsExtensible(TTarget target);

	/// <summary>
	/// Trap for preventing extensions on the target. 阻止目标扩展的 trap。
	/// </summary>
	[Description("@#preventExtensions")]
	public extern virtual bool PreventExtensions(TTarget target);

	/// <summary>
	/// Trap for the JavaScript <c>in</c> operator. JavaScript <c>in</c> 运算符的 trap。
	/// </summary>
	[Description("@#has")]
	public extern virtual bool Has(TTarget target, JazorPropertyKey property);

	/// <summary>
	/// Trap for function invocation.
	/// The receiver and argument list stay nullable because JavaScript call sites may supply any runtime values there.
	/// 函数调用 trap；receiver 和参数列表保持可空，因为 JavaScript 调用点可传入任意运行时值。
	/// </summary>
	[Description("@#apply")]
	public extern virtual object? Apply(TTarget target, object? thisArg, object?[] argumentsList);

	/// <summary>
	/// Trap for constructor invocation with <c>new</c>.
	/// The argument list stays nullable because JavaScript constructor calls may supply any runtime values there.
	/// 通过 <c>new</c> 调用构造器的 trap；参数列表保持可空，因为 JavaScript 构造器调用可传入任意运行时值。
	/// </summary>
	[Description("@#construct")]
	public extern virtual object? Construct(TTarget target, object?[] argumentsList, object newTarget);
}
