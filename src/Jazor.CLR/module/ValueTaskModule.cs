namespace Jazor.CLR;

/// <summary>
/// 将非泛型 System.Threading.Tasks.ValueTask 投影为 Promise。
/// </summary>
/// <remarks>
/// ValueTask 与 Task 共用同一个 Promise carrier，因此这里只覆盖 await/返回值编写路径上真正会用到的
/// 创建与等待成员（默认构造、CompletedTask、Task 包装、AsTask/Preserve、awaiter 配置）。
/// 状态检查成员（IsCompleted 等）依赖 Task 模块的 __jazorTaskStates 协议，长尾需求出现时应复用同一套
/// 状态模板而不是在此另起一套；返回 ValueTask&lt;TResult&gt; 的静态工厂在泛型 carrier 映射就绪前保持 unsupported。
/// <para>
/// 共用 carrier 的代价是运行时过滤：装箱后的 ValueTask 在 JS 下同样是 Promise，因此
/// <c>obj is Task</c> / <c>obj is Task&lt;T&gt;</c> 这类精确类型检查不再可判定，编译器会显式失败
/// 而不是发射假阳性的 <c>instanceof Promise</c>（与共用 Error carrier 的异常家族同一规则）。
/// </para>
/// </remarks>
[ECMAScriptModule("System/Threading/Tasks/ValueTaskModule.js")]
[Jazor(Op.Alias, "System.Threading.Tasks.ValueTask", "Promise")]
public static class ValueTaskModule
{
	// default(ValueTask) 与 new ValueTask() 都表示"已成功完成且无结果"。
	[Jazor(Op.Inline ,"System.Threading.Tasks.ValueTask.ValueTask()", "Promise.resolve()")]
	public extern static Promise _1403cc3779233c2c();

	///<summary>Initializes a new instance of the <see cref="T:System.Threading.Tasks.ValueTask" /> class using the supplied task that represents the operation.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.ValueTask.ValueTask(System.Threading.Tasks.Task)", "Promise.resolve(__arg1)")]
	public extern static Promise _ecb5062deec182c6(global::System.Threading.Tasks.Task task);

	///<summary>Initializes a new instance of the <see cref="T:System.Threading.Tasks.ValueTask" /> class using the supplied <see cref="T:System.Threading.Tasks.Sources.IValueTaskSource" /> object that represents the operation.</summary>
	// IValueTaskSource 是 CLR 池化协议，没有对应的浏览器 carrier。
	[Jazor(Op.Discard ,"System.Threading.Tasks.ValueTask.ValueTask(System.Threading.Tasks.Sources.IValueTaskSource, short)")]
	public extern static Promise _ac78e4299343644f(global::System.Threading.Tasks.Sources.IValueTaskSource source, Number token);

	[Jazor(Op.Inline ,"static System.Threading.Tasks.ValueTask.CompletedTask.get", "Promise.resolve()")]
	public extern static global::System.Threading.Tasks.ValueTask _395d253a48bfa9db();

	///<summary>Creates a <see cref="T:System.Threading.Tasks.ValueTask`1" /> that's completed successfully with the specified result.</summary>
	// ValueTask<TResult> 尚未映射，这里返回的值将没有可用成员面，故保持 unsupported。
	[Jazor(Op.Discard ,"static System.Threading.Tasks.ValueTask.FromResult<TResult>(TResult)")]
	public extern static global::System.Threading.Tasks.ValueTask<TResult> _a9034816209cc796<TResult>(TResult result);

	///<summary>Creates a <see cref="T:System.Threading.Tasks.ValueTask" /> that has completed due to cancellation with the specified cancellation token.</summary>
	// 取消原因与 TaskModule 保持同一个约定，供 IsCanceled/Status 复用同一识别规则。
	// token 被有意忽略，理由与 Task.FromCanceled 相同：结果一开始就是 Canceled，signal 不再驱动状态转换。
	[Jazor(Op.Inline ,"static System.Threading.Tasks.ValueTask.FromCanceled(System.Threading.CancellationToken)", "Promise.reject(new Error(\"TaskCanceledException\"))")]
	public extern static global::System.Threading.Tasks.ValueTask _1659e64e8178f1e4(AbortSignal cancellationToken);

	///<summary>Creates a <see cref="T:System.Threading.Tasks.ValueTask`1" /> that has completed due to cancellation with the specified cancellation token.</summary>
	[Jazor(Op.Discard ,"static System.Threading.Tasks.ValueTask.FromCanceled<TResult>(System.Threading.CancellationToken)")]
	public extern static global::System.Threading.Tasks.ValueTask<TResult> _dfe745de979b3dec<TResult>(AbortSignal cancellationToken);

	///<summary>Creates a <see cref="T:System.Threading.Tasks.ValueTask" /> that has completed with the specified exception.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.ValueTask.FromException(System.Exception)", "Promise.reject(__arg1)")]
	public extern static global::System.Threading.Tasks.ValueTask _2190e6b5d3ce645a(global::System.Exception exception);

	///<summary>Creates a <see cref="T:System.Threading.Tasks.ValueTask`1" /> that has completed with the specified exception.</summary>
	[Jazor(Op.Discard ,"static System.Threading.Tasks.ValueTask.FromException<TResult>(System.Exception)")]
	public extern static global::System.Threading.Tasks.ValueTask<TResult> _a4781d7c683f775b<TResult>(global::System.Exception exception);

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Discard ,"override System.Threading.Tasks.ValueTask.GetHashCode()")]
	public extern static Number _20eb9b6464367d96(Promise instance);

	///<summary>Determines whether the specified object is equal to the current <see cref="T:System.Threading.Tasks.ValueTask" /> instance.</summary>
	[Jazor(Op.Discard ,"override System.Threading.Tasks.ValueTask.Equals(object)")]
	public extern static bool _a92fa2f2f0247bd2(Promise instance, object? obj);

	///<summary>Determines whether the specified <see cref="T:System.Threading.Tasks.ValueTask" /> object is equal to the current <see cref="T:System.Threading.Tasks.ValueTask" /> object.</summary>
	[Jazor(Op.Discard ,"System.Threading.Tasks.ValueTask.Equals(System.Threading.Tasks.ValueTask)")]
	public extern static bool _f9a6103151b45ef3(Promise instance, global::System.Threading.Tasks.ValueTask other);

	///<summary>Compares two <see cref="T:System.Threading.Tasks.ValueTask" /> values for equality.</summary>
	// CLR 比较的是内部 obj/token 字段；擦除到 Promise 之后只剩引用同一性，
	// 两个各自完成的 ValueTask 在 CLR 下相等而在 JS 下是不同对象，因此不发射 === 启发式。
	[Jazor(Op.Discard ,"static System.Threading.Tasks.ValueTask.operator ==(System.Threading.Tasks.ValueTask, System.Threading.Tasks.ValueTask)")]
	public extern static bool _adc1860a7ee9024f(global::System.Threading.Tasks.ValueTask left, global::System.Threading.Tasks.ValueTask right);

	///<summary>Determines whether two <see cref="T:System.Threading.Tasks.ValueTask" /> values are unequal.</summary>
	[Jazor(Op.Discard ,"static System.Threading.Tasks.ValueTask.operator !=(System.Threading.Tasks.ValueTask, System.Threading.Tasks.ValueTask)")]
	public extern static bool _fbc37bb5a64ee224(global::System.Threading.Tasks.ValueTask left, global::System.Threading.Tasks.ValueTask right);

	///<summary>Retrieves a <see cref="T:System.Threading.Tasks.Task" /> object that represents this <see cref="T:System.Threading.Tasks.ValueTask" />.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.ValueTask.AsTask()", "Promise.resolve(__arg1)")]
	public extern static global::System.Threading.Tasks.Task _cca39ba1e0874b20(Promise instance);

	///<summary>Gets a <see cref="T:System.Threading.Tasks.ValueTask" /> that may be used at any point in the future.</summary>
	// Promise 本身可以重复 await，Preserve() 不需要额外的复制语义。
	[Jazor(Op.Inline ,"System.Threading.Tasks.ValueTask.Preserve()", "Promise.resolve(__arg1)")]
	public extern static global::System.Threading.Tasks.ValueTask _318b1fcbe9f077e1(Promise instance);

	[Jazor(Op.Discard ,"System.Threading.Tasks.ValueTask.IsCompleted.get")]
	public extern static bool _9b4baba665c34c5a(Promise instance);

	[Jazor(Op.Discard ,"System.Threading.Tasks.ValueTask.IsCompletedSuccessfully.get")]
	public extern static bool _c08b29883771cc82(Promise instance);

	[Jazor(Op.Discard ,"System.Threading.Tasks.ValueTask.IsFaulted.get")]
	public extern static bool _0a3b06794cb6e22d(Promise instance);

	[Jazor(Op.Discard ,"System.Threading.Tasks.ValueTask.IsCanceled.get")]
	public extern static bool _cdb5c5b29ee6c441(Promise instance);

	///<summary>Creates an awaiter for this value.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.ValueTask.GetAwaiter()", "Promise.resolve(__arg1)")]
	public extern static global::System.Runtime.CompilerServices.ValueTaskAwaiter _d9f56462100b8fab(Promise instance);

	///<summary>Configures an awaiter for this value.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.ValueTask.ConfigureAwait(bool)", "Promise.resolve(__arg1)")]
	public extern static global::System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable _e56a8766d3702b54(Promise instance, bool continueOnCapturedContext);
}
