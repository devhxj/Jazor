namespace Jazor.CLR;

/// <summary>
/// Task / Task&lt;TResult&gt; 家族共用的 inline 模板片段。
/// </summary>
/// <remarks>
/// 模板中的参数占位符由 SemanticWalker 实例化；这里不能写入依赖局部 C# 名称的代码。
/// 复杂控制流或需要独立 helper 的 Task 行为应放到 Import runtime method 中。
/// <para>
/// 取消、延时与冷启动这三段语义跨十多个重载复用，而 inline 模板必须自包含（无法 import 同模块的 helper），
/// 因此沿用本模块既有的 <c>globalThis.__jazor* ??= (...)</c> 惰性安装约定：模板文本里重复出现，
/// 运行时只定义一次；C# 侧靠 const 拼接保证同一段语义只写一遍。
/// </para>
/// </remarks>
internal static class TaskInlineTemplates
{
	// 取消原因分两类，不能混用。
	// 产出"已取消的 Task"的路径必须落在这个精确载荷上：Status / IsCanceled / IsFaulted 按字符串识别取消。
	private const string CanceledTaskReason = "\"TaskCanceledException\"";

	// 阻塞式 Wait / WaitAll / WaitAny 在 CLR 下直接抛 OperationCanceledException，不产出 Task，
	// 因此走运行时统一的 "<ExceptionName>: <message>" 失败格式。
	private const string CanceledWaitReason = "\"OperationCanceledException: The operation was canceled.\"";

	private const string TaskCore = "Promise.resolve(__arg1)";

	private const string AllCore = "Promise.all(__arg1)";

	private const string AllEnumerableCore = "Promise.all(Array.from(__arg1))";

	private const string AnyIndexCore =
		"Promise.race(Array.from(__arg1).map((task, index) => Promise.resolve(task).then(() => index, () => index)))";

	// TimeSpan carrier 以 tick 保存；-10000n（-1 毫秒）就是 Timeout.InfiniteTimeSpan，表示不排定定时器。
	private const string TimeSpanRejectArm =
		"(__arg2.ticks === -10000n ? new Promise(() => {}) : new Promise((_, reject) => setTimeout(() => reject(new Error(\"TimeoutException\")), Number(__arg2.ticks / 10000n))))";

	private const string TimeSpanFalseArm =
		"(__arg2.ticks === -10000n ? new Promise(() => {}) : new Promise((resolve) => setTimeout(() => resolve(false), Number(__arg2.ticks / 10000n))))";

	private const string TimeSpanMinusOneArm =
		"(__arg2.ticks === -10000n ? new Promise(() => {}) : new Promise((resolve) => setTimeout(() => resolve(-1), Number(__arg2.ticks / 10000n))))";

	private const string MillisecondsFalseArm =
		"(__arg2 === -1 ? new Promise(() => {}) : new Promise((resolve) => setTimeout(() => resolve(false), __arg2)))";

	private const string MillisecondsMinusOneArm =
		"(__arg2 === -1 ? new Promise(() => {}) : new Promise((resolve) => setTimeout(() => resolve(-1), __arg2)))";

	public const string WaitAsyncTimeSpan = "Promise.race([" + TaskCore + ", " + TimeSpanRejectArm + "])";

	public const string WaitTimeSpan = "Promise.race([" + TaskCore + ".then(() => true), " + TimeSpanFalseArm + "])";

	public const string WaitMilliseconds = "Promise.race([" + TaskCore + ".then(() => true), " + MillisecondsFalseArm + "])";

	public const string WaitAllTimeSpan = "Promise.race([" + AllCore + ".then(() => true), " + TimeSpanFalseArm + "])";

	public const string WaitAllMilliseconds = "Promise.race([" + AllCore + ".then(() => true), " + MillisecondsFalseArm + "])";

	public const string WaitAnyTimeSpan = "Promise.race([" + AnyIndexCore + ", " + TimeSpanMinusOneArm + "])";

	public const string WaitAnyMilliseconds = "Promise.race([" + AnyIndexCore + ", " + MillisecondsMinusOneArm + "])";

	// 取消侧只 reject、永不 resolve，因此 race 会把被等待方的结果原样透出。
	// 被等待方先落定时必须撤下 listener：default(CancellationToken) 与 CancellationToken.None 共用同一个
	// never-abort 单例，不撤销会让 listener 在这个全局单例上跨调用无上限累积。
	private const string WithCancellationOpen =
		"(globalThis.__jazorTaskWithCancellation ??= ((task, signal, reason) => { let fail = null; const cancellation = new Promise((_, reject) => { fail = () => reject(new Error(reason)); }); if (signal.aborted) { fail(); } else { signal.addEventListener(\"abort\", fail, { once: true }); } return Promise.race([Promise.resolve(task).finally(() => signal.removeEventListener(\"abort\", fail)), cancellation]); }))(";

	private const string CancelTask2 = ", __arg2, " + CanceledTaskReason + ")";

	private const string CancelTask3 = ", __arg3, " + CanceledTaskReason + ")";

	private const string CancelTask4 = ", __arg4, " + CanceledTaskReason + ")";

	private const string CancelWait2 = ", __arg2, " + CanceledWaitReason + ")";

	private const string CancelWait3 = ", __arg3, " + CanceledWaitReason + ")";

	public const string WaitCancellation = WithCancellationOpen + TaskCore + CancelWait2;

	public const string WaitTimeSpanCancellation = WithCancellationOpen + WaitTimeSpan + CancelWait3;

	public const string WaitMillisecondsCancellation = WithCancellationOpen + WaitMilliseconds + CancelWait3;

	public const string WaitAsyncCancellation = WithCancellationOpen + TaskCore + CancelTask2;

	public const string WaitAsyncTimeSpanCancellation = WithCancellationOpen + WaitAsyncTimeSpan + CancelTask3;

	public const string WaitAsyncTimeSpanProviderCancellation = WithCancellationOpen + WaitAsyncTimeSpan + CancelTask4;

	public const string WaitAllCancellation = WithCancellationOpen + AllCore + CancelWait2;

	public const string WaitAllEnumerableCancellation = WithCancellationOpen + AllEnumerableCore + CancelWait2;

	public const string WaitAllMillisecondsCancellation = WithCancellationOpen + WaitAllMilliseconds + CancelWait3;

	public const string WaitAnyCancellation = WithCancellationOpen + AnyIndexCore + CancelWait2;

	public const string WaitAnyMillisecondsCancellation = WithCancellationOpen + WaitAnyMilliseconds + CancelWait3;

	// 前继落定之前取消 => 延续不执行，延续任务直接进入 Canceled；延续一旦开始执行就不再受 token 影响
	// （与 CLR 一致），因此取消只与"前继落定"竞速，而不与延续本身竞速。
	// 前继的成败被折叠成同一个哨兵值：ContinueWith 的延续在两种情况下都要跑，只是不能吞掉 race 的拒绝。
	public const string ContinueWithCancellation =
		WithCancellationOpen + "Promise.resolve(__arg1).then(() => 0, () => 0)" + CancelTask3 + ".then(() => __arg2(__arg1))";

	public const string ContinueWithStateCancellation =
		WithCancellationOpen + "Promise.resolve(__arg1).then(() => 0, () => 0)" + CancelTask4 + ".then(() => __arg2(__arg1, __arg3))";

	// Task.Run 的 token 只影响"尚未开始执行"的工作：进入调度微任务时若已取消就不调用委托，
	// 已经开始执行之后 token 不再有可观察作用。
	public const string RunCancellation =
		"Promise.resolve().then(() => { if (__arg2.aborted) { throw new Error(" + CanceledTaskReason + "); } return __arg1(); })";

	public const string DelayTimeSpan =
		"(__arg1.ticks === -10000n ? new Promise(() => {}) : new Promise((resolve) => setTimeout(resolve, Number(__arg1.ticks / 10000n))))";

	public const string DelayMilliseconds =
		"(__arg1 === -1 ? new Promise(() => {}) : new Promise((resolve) => setTimeout(resolve, __arg1)))";

	// Delay 是唯一必须撤掉定时器的取消路径：定时器本身就是这次操作，被取消后继续持有它会把整段延时
	// 挂在事件循环上。其余竞速模板里的超时定时器与 CLR 的 timer 生命周期无关，沿用既有行为不额外清理。
	private const string DelayInstaller =
		"(globalThis.__jazorTaskDelay ??= ((milliseconds, signal) => new Promise((resolve, reject) => { const fail = () => reject(new Error(" + CanceledTaskReason + ")); if (signal.aborted) { fail(); return; } const id = milliseconds === -1 ? undefined : setTimeout(() => { signal.removeEventListener(\"abort\", onAbort); resolve(); }, milliseconds); const onAbort = () => { clearTimeout(id); fail(); }; signal.addEventListener(\"abort\", onAbort, { once: true }); })))";

	public const string DelayTimeSpanCancellation = DelayInstaller + "(Number(__arg1.ticks / 10000n), __arg2)";

	public const string DelayTimeSpanProviderCancellation = DelayInstaller + "(Number(__arg1.ticks / 10000n), __arg3)";

	public const string DelayMillisecondsCancellation = DelayInstaller + "(__arg1, __arg2)";

	// 冷启动 Task：构造时只登记 starter，Start() / RunSynchronously() 从 __jazorTaskStarters 取回并触发。
	private const string ColdTaskOpen =
		"(() => { const starters = globalThis.__jazorTaskStarters ??= new WeakMap(); const entry = { started: false, start: null }; const task = new Promise((resolve, reject) => { entry.start = () => { if (entry.started) { return; } entry.started = true; starters.delete(task); Promise.resolve().then(() => ";

	private const string ColdTaskStartEnd = ").then(resolve, reject); }; ";

	private const string ColdTaskRegister = "}); starters.set(task, entry); ";

	private const string ColdTaskAsyncState = "(globalThis.__jazorTaskAsyncStates ??= new WeakMap()).set(task, __arg2); ";

	private const string ColdTaskClose = "return task; })()";

	// token 取消一个尚未 Start 的 Task 时，CLR 让它直接进入 Canceled；已经开始执行后 token 不再生效，
	// 所以 cancel 与 start 抢同一个 entry.started 闸门。这里刻意不 delete starter 条目：
	// entry 由 WeakMap 以 task 为键持有，随 task 一起回收，而闸门已经足以让后续 Start() 变成 no-op。
	private const string ColdTaskCancelOpen =
		"const cancel = () => { if (entry.started) { return; } entry.started = true; reject(new Error(" + CanceledTaskReason + ")); }; if (";

	private const string ColdTaskCancelElse = ".aborted) { cancel(); } else { ";

	private const string ColdTaskCancelClose = ".addEventListener(\"abort\", cancel, { once: true }); } ";

	public const string ColdTask = ColdTaskOpen + "__arg1()" + ColdTaskStartEnd + ColdTaskRegister + ColdTaskClose;

	public const string ColdTaskCancellation =
		ColdTaskOpen + "__arg1()" + ColdTaskStartEnd
		+ ColdTaskCancelOpen + "__arg2" + ColdTaskCancelElse + "__arg2" + ColdTaskCancelClose
		+ ColdTaskRegister + ColdTaskClose;

	public const string ColdTaskWithState =
		ColdTaskOpen + "__arg1(__arg2)" + ColdTaskStartEnd + ColdTaskRegister + ColdTaskAsyncState + ColdTaskClose;

	public const string ColdTaskWithStateCancellation =
		ColdTaskOpen + "__arg1(__arg2)" + ColdTaskStartEnd
		+ ColdTaskCancelOpen + "__arg3" + ColdTaskCancelElse + "__arg3" + ColdTaskCancelClose
		+ ColdTaskRegister + ColdTaskAsyncState + ColdTaskClose;
}
