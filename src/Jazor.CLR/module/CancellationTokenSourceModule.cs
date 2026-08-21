namespace Jazor.CLR;

/// <summary>
/// System.Threading.CancellationTokenSource 映射到浏览器的 AbortController。
/// </summary>
/// <remarks>
/// CLR 的 source/token 分工与 AbortController/AbortSignal 完全对应：source 持有取消权，token 只是只读视图，
/// 因此 <c>Token</c> 直接就是 <c>controller.signal</c>，<c>Cancel()</c> 直接就是 <c>abort()</c>，
/// 不引入额外包装对象。
/// <para>
/// <c>Cancel()</c> 不传 reason：运行时统一以 <c>Error("&lt;ExceptionName&gt;: &lt;message&gt;")</c> 表达失败，
/// 而 abort reason 默认是 DOMException，catch 侧无法按该协议识别。取消原因由消费方（Task 系列模板、
/// <c>ThrowIfCancellationRequested</c>）各自构造。
/// </para>
/// <para>
/// 延迟取消（延迟构造函数与 <c>CancelAfter</c>）需要"替换上一次延迟"和"Dispose 时清除"两种行为，
/// 因此定时器 id 记录在模块级 <see cref="PendingCancelTimers"/> 中，而不是给宿主 AbortController
/// 挂私有属性。<c>TryReset()</c> 保持 unsupported：AbortController 一旦 abort 就不可复位。
/// </para>
/// </remarks>
[ECMAScriptModule("System/Threading/CancellationTokenSourceModule.js")]
[Jazor(Op.Alias, "System.Threading.CancellationTokenSource", "AbortController")]
public static class CancellationTokenSourceModule
{
	// 待触发的延迟取消定时器。CancelAfter 会替换上一次的延迟，Dispose 会清除它；
	// 用 WeakMap 保证 controller 被回收时记录一起消失。
	private static readonly WeakMap<AbortController, Number> PendingCancelTimers = new();

	/// <summary>
	/// 排定（或撤销）一次延迟取消，供延迟构造函数与 <c>CancelAfter</c> 共用。
	/// </summary>
	/// <remarks>
	/// CLR 只接受 -1（Timeout.Infinite）或 0..Int32.MaxValue 毫秒。超出范围时 setTimeout 会静默钳位，
	/// 把"永不自动取消"变成"下一 tick 就取消"，属于可观察的语义偏移，因此必须在这里显式失败。
	/// </remarks>
	private static void ScheduleCancel(AbortController controller, Number millisecondsDelay)
	{
		if (millisecondsDelay < -1 || millisecondsDelay > 2147483647)
			throw new Error("ArgumentOutOfRangeException: The delay must be -1 or between 0 and Int32.MaxValue milliseconds.");

		ClearPendingCancel(controller);

		// -1 表示"永不自动取消"，不排定定时器；此前的延迟已在上面清除。
		if (millisecondsDelay == -1)
			return;

		PendingCancelTimers.Set(controller, SetTimeout(() => controller.Abort(), millisecondsDelay));
	}

	// WeakMap.Get 对值类型 TValue 无法区分"缺失"与"0"（缺失的 undefined 只在引用类型上投影为 null），
	// 因此存在性判断走 Has。
	private static void ClearPendingCancel(AbortController controller)
	{
		if (PendingCancelTimers.Has(controller))
		{
			ClearTimeout(PendingCancelTimers.Get(controller));
			PendingCancelTimers.Delete(controller);
		}
	}

	// TimeSpan carrier 以 tick 保存；CLR 走 (long)delay.TotalMilliseconds，同样向零截断。
	private static Number ToMillisecondsDelay(RuntimeModule.JTimeSpan delay)
		=> NumberValue(delay.Ticks / BigIntValue(10000));

	private static AbortController CreateWithMillisecondsDelay(Number millisecondsDelay)
	{
		var controller = new AbortController();
		ScheduleCancel(controller, millisecondsDelay);
		return controller;
	}

	/// <summary>
	/// 把若干输入 token 链接成一个仍可独立 Cancel 的新 source。
	/// </summary>
	/// <remarks>
	/// <c>AbortSignal.any</c> 只产出 signal，而 CLR 要求返回一个可 <c>Cancel()</c> 的 source，
	/// 因此新建 controller 并把聚合信号的 abort 转发过去。输入已取消时 any 的结果也已 abort，
	/// 此时 abort 事件不会再派发，必须立刻转发一次。
	/// </remarks>
	private static AbortController CreateLinked(AbortSignal[] tokens)
	{
		var linked = new AbortController();
		var source = AbortSignal.Any(tokens);
		if (source.Aborted)
			linked.Abort();
		else
			source.AddEventListener("abort", (HandleEventCallback)(_ => linked.Abort()), false);

		return linked;
	}

	[Jazor(Op.Inline, "System.Threading.CancellationTokenSource.IsCancellationRequested.get", "__arg1.signal.aborted")]
	public extern static bool _7bce90ebe75fba7d(AbortController instance);

	[Jazor(Op.Alias, "System.Threading.CancellationTokenSource.Token.get", "signal")]
	public extern static AbortSignal _c6beb3ac47585eb0(AbortController instance);

	///<summary>Initializes a new instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class.</summary>
	[Jazor(Op.Inline, "System.Threading.CancellationTokenSource.CancellationTokenSource()", "new AbortController()")]
	public extern static AbortController _c93a8dffcc42e84b();

	///<summary>Initializes a new instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class that will be canceled after the specified time span.</summary>
	[Jazor(Op.Import, "System.Threading.CancellationTokenSource.CancellationTokenSource(System.TimeSpan)", "createWithDelay")]
	public static AbortController _cbe063f9fd0c2719(RuntimeModule.JTimeSpan delay)
		=> CreateWithMillisecondsDelay(ToMillisecondsDelay(delay));

	///<summary>Initializes a new instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class that will be canceled after the specified <see cref="T:System.TimeSpan" />.</summary>
	// TimeProvider 未映射；用宿主时钟替代它会静默丢掉调用方自带的时间源。
	[Jazor(Op.Discard, "System.Threading.CancellationTokenSource.CancellationTokenSource(System.TimeSpan, System.TimeProvider)")]
	public extern static AbortController _1c33ef293564b460(RuntimeModule.JTimeSpan delay, global::System.TimeProvider timeProvider);

	///<summary>Initializes a new instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class that will be canceled after the specified delay in milliseconds.</summary>
	[Jazor(Op.Import, "System.Threading.CancellationTokenSource.CancellationTokenSource(int)", "createWithMillisecondsDelay")]
	public static AbortController _99cb96f8cd1386b9(Number millisecondsDelay)
		=> CreateWithMillisecondsDelay(millisecondsDelay);

	///<summary>Communicates a request for cancellation.</summary>
	[Jazor(Op.Alias, "System.Threading.CancellationTokenSource.Cancel()", "abort")]
	public extern static void _7b1e80c48df4a4a1(AbortController instance);

	///<summary>Communicates a request for cancellation, and specifies whether remaining callbacks and cancelable operations should be processed if an exception occurs.</summary>
	// throwOnFirstException 描述 CLR 如何聚合回调抛出的异常；abort 派发不聚合 listener 异常，
	// 没有可观察的对应行为，因此忽略该开关。
	[Jazor(Op.Inline, "System.Threading.CancellationTokenSource.Cancel(bool)", "__arg1.abort()")]
	public extern static void _b528c1e73ac70627(AbortController instance, bool throwOnFirstException);

	///<summary>Communicates a request for cancellation asynchronously.</summary>
	// abort() 是同步的；CancelAsync 的可观察差异只是"结果以 Task 形式返回"。
	[Jazor(Op.Inline, "System.Threading.CancellationTokenSource.CancelAsync()", "Promise.resolve(__arg1.abort())")]
	public extern static Promise _d6c75d8a27eec714(AbortController instance);

	///<summary>Schedules a cancel operation on this <see cref="T:System.Threading.CancellationTokenSource" /> after the specified time span.</summary>
	[Jazor(Op.Import, "System.Threading.CancellationTokenSource.CancelAfter(System.TimeSpan)", "cancelAfterDelay")]
	public static void _142b2ab0f86b3788(AbortController instance, RuntimeModule.JTimeSpan delay)
		=> ScheduleCancel(instance, ToMillisecondsDelay(delay));

	///<summary>Schedules a cancel operation on this <see cref="T:System.Threading.CancellationTokenSource" /> after the specified number of milliseconds.</summary>
	[Jazor(Op.Import, "System.Threading.CancellationTokenSource.CancelAfter(int)", "cancelAfter")]
	public static void _054ea7e5f7fdad80(AbortController instance, Number millisecondsDelay)
		=> ScheduleCancel(instance, millisecondsDelay);

	///<summary>Attempts to reset the <see cref="T:System.Threading.CancellationTokenSource" /> to be used for an unrelated operation.</summary>
	// AbortController/AbortSignal 的 abort 是单向终态，没有复位入口。
	[Jazor(Op.Discard, "System.Threading.CancellationTokenSource.TryReset()")]
	public extern static bool _b73d00b710a1dde2(AbortController instance);

	///<summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class.</summary>
	// Dispose 不取消，只释放资源；这里唯一的资源是尚未触发的延迟取消定时器。
	[Jazor(Op.Import, "System.Threading.CancellationTokenSource.Dispose()", "dispose")]
	public static void _2168e1dc84c34975(AbortController instance)
		=> ClearPendingCancel(instance);

	///<summary>Creates a <see cref="T:System.Threading.CancellationTokenSource" /> that will be in the canceled state when any of the source tokens are in the canceled state.</summary>
	[Jazor(Op.Import, "static System.Threading.CancellationTokenSource.CreateLinkedTokenSource(System.Threading.CancellationToken, System.Threading.CancellationToken)", "createLinkedTokenSourceFromPair")]
	public static AbortController _00350dc2979ca5c5(AbortSignal token1, AbortSignal token2)
		=> CreateLinked([token1, token2]);

	///<summary>Creates a <see cref="T:System.Threading.CancellationTokenSource" /> that will be in the canceled state when the supplied token is in the canceled state.</summary>
	[Jazor(Op.Import, "static System.Threading.CancellationTokenSource.CreateLinkedTokenSource(System.Threading.CancellationToken)", "createLinkedTokenSource")]
	public static AbortController _b01498d7103a5db2(AbortSignal token)
		=> CreateLinked([token]);

	///<summary>Creates a <see cref="T:System.Threading.CancellationTokenSource" /> that will be in the canceled state when any of the source tokens in the specified array are in the canceled state.</summary>
	// 长尾重载：常用路径是一到两个 token，params 数组/span 形态等有明确需求时再支持。
	[Jazor(Op.Discard, "static System.Threading.CancellationTokenSource.CreateLinkedTokenSource(params System.Threading.CancellationToken[])")]
	public extern static AbortController _943ce2954d0f9210(Array<AbortSignal> tokens);

	///<summary>Creates a <see cref="T:System.Threading.CancellationTokenSource" /> that will be in the canceled state when any of the source tokens are in the canceled state.</summary>
	[Jazor(Op.Discard, "static System.Threading.CancellationTokenSource.CreateLinkedTokenSource(params System.ReadOnlySpan<System.Threading.CancellationToken>)")]
	public extern static AbortController _a9302f782f58fc4e(Array<AbortSignal> tokens);
}
