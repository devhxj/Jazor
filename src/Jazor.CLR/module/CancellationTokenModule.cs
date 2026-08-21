namespace Jazor.CLR;

/// <summary>
/// System.Threading.CancellationToken 映射到浏览器的 AbortSignal。
/// </summary>
/// <remarks>
/// CancellationToken 在 CLR 下是围绕 CancellationTokenSource 的只读视图，AbortSignal 与 AbortController 的关系完全对应，
/// 因此 token 直接擦除为 signal，不引入额外包装对象。
/// <para>
/// 身份约定：CLR 保证 <c>default(CancellationToken) == CancellationToken.None</c>，且两者都永远不会被取消。
/// 这里用一个模块级的、永不 abort 的 signal 单例承载这个事实，因此 <c>None</c>、无参构造和
/// <c>default</c> 都会得到同一个引用，<c>==</c> / <c>Equals</c> 可以直接落成 <c>===</c>，而
/// <c>CanBeCanceled</c> 正好就是"不是这个单例"。单例的引用稳定性是这一组语义的前提，不要改成每次新建。
/// </para>
/// <para>
/// 回调注册（<c>Register</c> / <c>UnsafeRegister</c>）返回 <c>CancellationTokenRegistration</c>，
/// 它擦除为 <see cref="RuntimeModule.JCancellationTokenRegistration"/>，只保存解除订阅所需的
/// (signal, listener) 二元组；注册与撤销的实际语义集中在 <see cref="RuntimeModule.RegisterCancellationCallback"/>
/// 和 <see cref="RuntimeModule.UnregisterCancellationCallback"/>，各重载只负责把 CLR 的
/// state/token 形参适配成一个零参回调。
/// </para>
/// </remarks>
[ECMAScriptModule("System/Threading/CancellationTokenModule.js")]
[Jazor(Op.Alias, "System.Threading.CancellationToken", "AbortSignal")]
public static class CancellationTokenModule
{
	// 永不 abort 的 signal 单例：它的 controller 从不被引用，因此 abort() 无从触发。
	// None / default / new CancellationToken() 共用它，身份即语义，不能替换成 AbortSignal.timeout 之类会自行完成的信号。
	private static readonly AbortSignal NoneSignal = new AbortController().Signal;

	// 其他模块（例如 LocationChangingContext 的默认 token）必须拿到同一个单例，
	// 否则 default(CancellationToken) == CancellationToken.None 的身份约定会破裂。
	internal static AbortSignal GetNoneSignal() => NoneSignal;

	[Jazor(Op.Import, "System.Threading.CancellationToken.CancellationToken()", "createDefaultToken")]
	public static AbortSignal _f21ba4033b40a8aa() => NoneSignal;

	[Jazor(Op.Import, "static System.Threading.CancellationToken.None.get", "getNone")]
	public static AbortSignal _39130b6163fb1960() => NoneSignal;

	[Jazor(Op.Alias, "System.Threading.CancellationToken.IsCancellationRequested.get", "aborted")]
	public extern static bool _d304e669ec364248(AbortSignal instance);

	// CLR 语义是"这个 token 是否可能被取消"，等价于它不是那个永不取消的单例。
	[Jazor(Op.Import, "System.Threading.CancellationToken.CanBeCanceled.get", "getCanBeCanceled")]
	public static bool _f343b545e3147cce(AbortSignal instance) => instance != NoneSignal;

	// WaitHandle 是 CLR 内核同步对象，浏览器没有对等物。
	[Jazor(Op.Discard, "System.Threading.CancellationToken.WaitHandle.get")]
	public extern static global::System.Threading.WaitHandle _8f00231516910f63(AbortSignal instance);

	///<summary>Initializes the <see cref="T:System.Threading.CancellationToken" />.</summary>
	[Jazor(Op.Import, "System.Threading.CancellationToken.CancellationToken(bool)", "createToken")]
	public static AbortSignal _c5634ecc2859098c(bool canceled)
		=> canceled ? AbortSignal.Abort() : NoneSignal;

	///<summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
	[Jazor(Op.Import, "System.Threading.CancellationToken.Register(System.Action)", "register")]
	public static RuntimeModule.JCancellationTokenRegistration _72a0106915493c44(AbortSignal instance, global::System.Action callback)
		=> RuntimeModule.RegisterCancellationCallback(instance, callback);

	///<summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
	// useSynchronizationContext 描述回调回到哪个同步上下文执行；浏览器只有一个事件循环，
	// abort listener 本身就在它上面跑，因此这个开关无可观察差异，直接忽略。
	[Jazor(Op.Import, "System.Threading.CancellationToken.Register(System.Action, bool)", "registerWithSynchronizationContext")]
	public static RuntimeModule.JCancellationTokenRegistration _2424f34aae18aa06(AbortSignal instance, global::System.Action callback, bool useSynchronizationContext)
		=> RuntimeModule.RegisterCancellationCallback(instance, callback);

	///<summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
	[Jazor(Op.Import, "System.Threading.CancellationToken.Register(System.Action<object>, object)", "registerWithState")]
	public static RuntimeModule.JCancellationTokenRegistration _eb49f18acb077ff1(AbortSignal instance, global::System.Action<object?> callback, object? state)
		=> RuntimeModule.RegisterCancellationCallback(instance, () => callback(state));

	///<summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken">CancellationToken</see> is canceled.</summary>
	// 第二个实参是"触发这次取消的 token"，擦除后就是 instance 自身这个 signal。
	[Jazor(Op.Import, "System.Threading.CancellationToken.Register(System.Action<object, System.Threading.CancellationToken>, object)", "registerWithStateAndToken")]
	public static RuntimeModule.JCancellationTokenRegistration _11a6b73058ddd45e(AbortSignal instance, global::System.Action<object?, AbortSignal> callback, object? state)
		=> RuntimeModule.RegisterCancellationCallback(instance, () => callback(state, instance));

	///<summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
	[Jazor(Op.Import, "System.Threading.CancellationToken.Register(System.Action<object>, object, bool)", "registerWithStateAndSynchronizationContext")]
	public static RuntimeModule.JCancellationTokenRegistration _f55770dedf931292(AbortSignal instance, global::System.Action<object?> callback, object? state, bool useSynchronizationContext)
		=> RuntimeModule.RegisterCancellationCallback(instance, () => callback(state));

	///<summary>Registers a delegate that is called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
	// UnsafeRegister 只是不捕获 ExecutionContext；浏览器没有 ExecutionContext，与 Register 同路。
	[Jazor(Op.Import, "System.Threading.CancellationToken.UnsafeRegister(System.Action<object>, object)", "unsafeRegisterWithState")]
	public static RuntimeModule.JCancellationTokenRegistration _54049b6fbd22e813(AbortSignal instance, global::System.Action<object?> callback, object? state)
		=> RuntimeModule.RegisterCancellationCallback(instance, () => callback(state));

	///<summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken">CancellationToken</see> is canceled.</summary>
	[Jazor(Op.Import, "System.Threading.CancellationToken.UnsafeRegister(System.Action<object, System.Threading.CancellationToken>, object)", "unsafeRegisterWithStateAndToken")]
	public static RuntimeModule.JCancellationTokenRegistration _bd3fc6b3035e6a60(AbortSignal instance, global::System.Action<object?, AbortSignal> callback, object? state)
		=> RuntimeModule.RegisterCancellationCallback(instance, () => callback(state, instance));

	///<summary>Determines whether the current <see cref="T:System.Threading.CancellationToken" /> instance is equal to the specified token.</summary>
	// CLR 比较的是内部 source 引用；擦除到 signal 之后同一个 source 就是同一个 signal，引用相等即可。
	[Jazor(Op.Inline, "System.Threading.CancellationToken.Equals(System.Threading.CancellationToken)", "__arg1 === __arg2")]
	public extern static bool _1164f03605d2c4fa(AbortSignal instance, AbortSignal other);

	///<summary>Determines whether the current <see cref="T:System.Threading.CancellationToken" /> instance is equal to the specified <see cref="T:System.Object" />.</summary>
	[Jazor(Op.Inline, "override System.Threading.CancellationToken.Equals(object)", "__arg1 === __arg2")]
	public extern static bool _1a6a42d621ec0494(AbortSignal instance, object? other);

	///<summary>Serves as a hash function for a <see cref="T:System.Threading.CancellationToken" />.</summary>
	// signal 没有稳定的数值身份，发射任何近似值都会破坏 Equals/GetHashCode 一致性。
	[Jazor(Op.Discard, "override System.Threading.CancellationToken.GetHashCode()")]
	public extern static Number _35888e21bae24e5c(AbortSignal instance);

	///<summary>Determines whether two <see cref="T:System.Threading.CancellationToken" /> instances are equal.</summary>
	// 默认 lowering 就是 === / !==，与上面的 Equals 同一套引用身份规则。
	[Jazor(Op.Allowed, "static System.Threading.CancellationToken.operator ==(System.Threading.CancellationToken, System.Threading.CancellationToken)")]
	public extern static bool _20bdabf51c432a6d(AbortSignal left, AbortSignal right);

	///<summary>Determines whether two <see cref="T:System.Threading.CancellationToken" /> instances are not equal.</summary>
	[Jazor(Op.Allowed, "static System.Threading.CancellationToken.operator !=(System.Threading.CancellationToken, System.Threading.CancellationToken)")]
	public extern static bool _0b54f5c239fec8ac(AbortSignal left, AbortSignal right);

	///<summary>Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</summary>
	// 不用 signal.throwIfAborted()：那会抛出 DOMException，与运行时统一的
	// "Error(\"<ExceptionName>: <message>\")" 失败协议不一致，catch 侧无法识别。
	[Jazor(Op.Import, "System.Threading.CancellationToken.ThrowIfCancellationRequested()", "throwIfCancellationRequested")]
	public static void _93a52990613703a6(AbortSignal instance)
	{
		if (instance.Aborted)
			throw new Error("OperationCanceledException: The operation was canceled.");
	}
}
