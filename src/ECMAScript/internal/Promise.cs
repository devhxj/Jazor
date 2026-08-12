using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace ECMAScript;

/// <summary>
/// JavaScript promise shape used for instance members such as <c>then</c>, <c>catch</c>, and <c>finally</c>.
/// This bridge interface is intentionally hidden so editor completion stays focused on runtime hosts like <see cref="Promise"/>.
/// JavaScript Promise 的实例形状，用于 <c>then</c>、<c>catch</c> 和 <c>finally</c>；该桥接接口被刻意隐藏，
/// 使编辑器补全聚焦于 <see cref="Promise"/> 等运行时宿主。
/// </summary>
[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IPromise
{
    /// <summary>Returns a chained promise after observing rejection. 观察 rejection 后返回链式 Promise。</summary>
    [Description("@#catch")]
    IPromise Catch(Action<Error> onError);

    /// <summary>Runs a final callback without changing fulfillment unless it throws or rejects. 执行最终回调；除非回调抛出或拒绝，否则不改变兑现结果。</summary>
    [Description("@#finally")]
    IPromise Finally(Action onFinal);

    /// <summary>
    /// Returns a new promise that will be resolved when the passed in action is finished.
    /// 当前 Promise 兑现后执行回调并返回新 Promise；调度遵循 JavaScript Promise 微任务语义。
    /// </summary>
    /// <param name="onFulfilled">Action to be invoked on resolution</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise Then(Action onFulfilled);
    /// <summary>
    /// Returns a new promise that will be resolved when one of the passed in actions is finished.
    /// 在兑现或拒绝路径运行对应回调，并按 JavaScript Promise resolution procedure 返回新 Promise。
    /// </summary>
    /// <param name="onFulfilled">Action to be invoked when this promise is resolved</param>
    /// <param name="onRejected">Action to be invoked when this promise is rejected</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise Then(Action onFulfilled, Action onRejected);
    /// <summary>
    /// Returns a new promise that will be resolved when one of the passed in actions is finished.
    /// </summary>
    /// <param name="onFulfilled">Action to be invoked when this promise is resolved</param>
    /// <param name="onRejected">Action to be invoked when this promise is rejected</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise Then(Action onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// Returns a new promise that will be resolved with the return value of the passed
    /// in <see cref="Func{TResult}" />.
    /// 兑现后执行映射回调，并将返回值按 JavaScript Promise 解析规则包装到新 Promise。
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise<T> Then<T>(Func<T> onFulfilled);
    /// <summary>
    /// Returns a promise that will be resolved with the return value of the passed in
    /// <see cref="Func{TResult}"/>, or <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise<T> Then<T>(Func<T> onFulfilled, Action onRejected);
    /// <summary>
    /// Returns a promise that will be resolved with the return value of the passed in
    /// <see cref="Func{TResult}"/>, or <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise<T> Then<T>(Func<T> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="IPromise{T}"/> returned from the passed in <see cref="Func{TResult}"/>
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled);
    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="IPromise{T}"/> returned from the passed in <see cref="Func{TResult}"/>, or
    /// <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled, Action onRejected);
    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="IPromise{T}"/> returned from the passed in <see cref="Func{TResult}"/>, or
    /// <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="IPromise"/> returned from the passed in <see cref="Func{TResult}"/>
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise Then(Func<IPromise> onFulfilled);
    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="IPromise"/> returned from the passed in <see cref="Func{TResult}"/>, or
    /// <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise Then(Func<IPromise> onFulfilled, Action onRejected);
    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="IPromise"/> returned from the passed in <see cref="Func{TResult}"/>, or
    /// <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise Then(Func<IPromise> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// Returns a new promise that will be resolved with the value carried by the
    /// bridge-only <see cref="PromiseResult"/> returned from the passed in <see cref="Func{TResult}"/>.
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    IPromise Then(Func<PromiseResult> onFulfilled);
    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="PromiseResult"/> returned from the passed in <see cref="Func{TResult}"/>, or
    /// <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    IPromise Then(Func<PromiseResult> onFulfilled, Action onRejected);
    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="PromiseResult"/> returned from the passed in <see cref="Func{TResult}"/>, or
    /// <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    IPromise Then(Func<PromiseResult> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// Returns a new promise that will be resolved with the value carried by the
    /// bridge-only <see cref="PromiseResult{TResult}"/> returned from the passed in <see cref="Func{TResult}"/>.
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    IPromise<T> Then<T>(Func<PromiseResult<T>> onFulfilled);
    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="PromiseResult{TResult}"/> returned from the passed in <see cref="Func{TResult}"/>, or
    /// <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    IPromise<T> Then<T>(Func<PromiseResult<T>> onFulfilled, Action onRejected);
    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="PromiseResult{TResult}"/> returned from the passed in <see cref="Func{TResult}"/>, or
    /// <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    IPromise<T> Then<T>(Func<PromiseResult<T>> onFulfilled, Action<Error> onRejected);
}

/// <summary>
/// Typed JavaScript promise shape used for instance members such as <c>then</c> and <c>finally</c>.
/// This is a bridge contract, so it stays hidden from normal editor completion.
/// 带兑现值类型标注的 JavaScript Promise 实例形状；它是桥接契约，故对正常编辑器补全隐藏。
/// </summary>
[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IPromise<T> : IPromise
{
    /// <summary>Runs a final callback and preserves the typed promise result unless it throws or rejects. 执行最终回调并保留类型化兑现结果，除非回调抛出或拒绝。</summary>
    [Description("@#finally")]
    new IPromise<T> Finally(Action onFinal);

    /// <summary>
    /// Returns a new promise that will be resolved when the passed in action is finished.
    /// </summary>
    /// <param name="onFulfilled"><see cref="Action{T}"/> to be invoked on resolution</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise Then(Action<T> onFulfilled);
    /// <summary>
    /// Returns a new promise that will be resolved when one of the passed in actions is finished.
    /// </summary>
    /// <param name="onFulfilled"><see cref="Action{T}"/> to be invoked when this promise is resolved</param>
    /// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise Then(Action<T> onFulfilled, Action onRejected);

    /// <summary>
    /// Returns a new promise that will be resolved when one of the passed in actions is finished.
    /// </summary>
    /// <param name="onFulfilled"><see cref="Action{T}"/> to be invoked when this promise is resolved</param>
    /// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise Then(Action<T> onFulfilled, Action<Error> onRejected);


    /// <summary>
    /// Returns a new promise that will be resolved with the return value of the passed in
    /// <see cref="Func{TResult}"/>.
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{T, TResult}"/> to be invoked when this promise is resolved</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled);
    /// <summary>
    /// Returns a promise that will be resolved with the return value of the passed in
    /// <see cref="Func{TResult}"/>, or <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
    /// <param name="onFulfilled"><see cref="Func{T, TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action onRejected);
    /// <summary>
    /// Returns a promise that will be resolved with the return value of the passed in
    /// <see cref="Func{TResult}"/>, or <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
    /// <param name="onFulfilled"><see cref="Func{T, TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// Returns a new promise that will be resolved with the value carried by the
    /// bridge-only <see cref="PromiseResult{TResult}"/> returned from the passed in <see cref="Func{TResult}"/>.
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{T, TResult}"/> to be invoked when this promise is resolved</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    IPromise<TResult> Then<TResult>(Func<T, PromiseResult<TResult>> onFulfilled);
    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="PromiseResult{TResult}"/> returned from the passed in <see cref="Func{TResult}"/>, or
    /// <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <typeparam name="TResult">Return type of the <see cref="Func{TResult}"/></typeparam>
    /// <param name="onFulfilled"><see cref="Func{T, TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    IPromise<TResult> Then<TResult>(Func<T, PromiseResult<TResult>> onFulfilled, Action onRejected);
    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="PromiseResult{TResult}"/> returned from the passed in <see cref="Func{TResult}"/>, or
    /// <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <typeparam name="TResult">Return type of the <see cref="Func{TResult}"/></typeparam>
    /// <param name="onFulfilled"><see cref="Func{T, TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    IPromise<TResult> Then<TResult>(Func<T, PromiseResult<TResult>> onFulfilled, Action<Error> onRejected);

    [Description("@#then")]
    IPromise Then(Func<T, IPromise> onResolve);
    [Description("@#then")]
    IPromise Then(Func<T, IPromise> onResolve, Action onRejected);
    [Description("@#then")]
    IPromise Then(Func<T, IPromise> onResolve, Action<Error> onRejected);

    [Description("@#then")]
    IPromise<TResult> Then<TResult>(Func<T, IPromise<TResult>> onFulfilled);
    [Description("@#then")]
    IPromise<TResult> Then<TResult>(Func<T, IPromise<TResult>> onFulfilled, Action onRejected);
    [Description("@#then")]
    IPromise<TResult> Then<TResult>(Func<T, IPromise<TResult>> onFulfilled, Action<Error> onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    IPromise Then(Func<T, PromiseResult> onFulfilled);
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    IPromise Then(Func<T, PromiseResult> onFulfilled, Action onRejected);
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    IPromise Then(Func<T, PromiseResult> onFulfilled, Action<Error> onRejected);
}

/// <summary>
/// Bridge-only placeholder used by async lowering and generated bindings where JavaScript would normally expose a promise or awaited value.
/// This type is not a JavaScript runtime global and its CLR members do not map to JavaScript instance members.
/// async lowering 和生成绑定使用的桥接占位符；它不是 JavaScript 运行时全局对象，其 CLR 成员也不映射为 JavaScript 实例成员。
/// </summary>
[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
/// <summary>
/// Awaiter adapter carrier for a non-generic Promise result.
/// 非泛型 Promise 结果的异步等待适配载体。
/// </summary>
/// <remarks>It connects JavaScript Promise values to C# await/awaiter authoring and does not imply synchronous Promise execution.
/// 它用于把 JavaScript Promise 接入 C# await/awaiter 编写，不代表同步执行 Promise。</remarks>
public sealed class PromiseResult : IAsyncResult
{
    /// <summary>CLR async bridge state; not a JavaScript Promise property. CLR 异步桥接状态，不是 JavaScript Promise 属性。</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public extern object AsyncState { get; }

    /// <summary>CLR async bridge wait handle; not a JavaScript Promise property. CLR 异步桥接等待句柄，不是 JavaScript Promise 属性。</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public extern WaitHandle AsyncWaitHandle { get; }

	/// <summary>CLR async bridge completion flag. CLR 异步桥接完成标记。</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern bool CompletedSynchronously { get; }

	/// <summary>CLR async bridge completion state. CLR 异步桥接完成状态。</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern bool IsCompleted { get; }

	/// <summary>
	/// Bridge-only completed sentinel used by async lowering.
	/// async lowering 使用的已完成桥接哨兵值，不对应 JavaScript 运行时成员。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern static PromiseResult Completed { get; }
}

/// <summary>
/// Bridge-only placeholder used by async lowering and generated bindings where JavaScript would normally expose a promise or awaited value.
/// This type is not a JavaScript runtime global and its CLR members do not map to JavaScript instance members.
/// async lowering 和生成绑定使用的带结果桥接占位符；它不是 JavaScript 运行时全局对象，其 CLR 成员也不映射为 JavaScript 实例成员。
/// </summary>
[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
/// <summary>Awaiter adapter carrier for a typed Promise result. 带结果值的 Promise await 适配载体。</summary>
public sealed class PromiseResult<TResult> : IAsyncResult
{
	/// <summary>CLR async bridge state; not a JavaScript Promise property. CLR 异步桥接状态，不是 JavaScript Promise 属性。</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern object AsyncState { get; }

	/// <summary>CLR async bridge wait handle; not a JavaScript Promise property. CLR 异步桥接等待句柄，不是 JavaScript Promise 属性。</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern WaitHandle AsyncWaitHandle { get; }

	/// <summary>CLR async bridge completion flag. CLR 异步桥接完成标记。</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern bool CompletedSynchronously { get; }

	/// <summary>CLR async bridge completion state. CLR 异步桥接完成状态。</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern bool IsCompleted { get; }

	/// <summary>
	/// Bridge-only completed sentinel used by async lowering.
	/// async lowering 使用的已完成桥接哨兵值，不对应 JavaScript 运行时成员。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern static PromiseResult<TResult> Completed { get; }
}

/// <summary>
/// JavaScript object shape returned by <c>Promise.allSettled</c>.
/// The object exposes both <c>value</c> and <c>reason</c> fields in JavaScript, but only one is meaningful for a given <see cref="Status"/>.
/// JavaScript <c>Promise.allSettled</c> 返回的对象形状。JavaScript 同时允许对象具有 <c>value</c> 与 <c>reason</c> 字段，
/// 但对于给定 <see cref="Status"/>，只有其中一个具有业务含义。
/// </summary>
[ECMAScript]
[Description("@#")]
/// <summary>Fulfilled/rejected union-like result shape returned by <c>Promise.allSettled</c>. Promise.allSettled 返回的 fulfilled/rejected 结果联合形状。</summary>
public sealed class PromiseSettledResult<T>
{
	/// <summary>
	/// Settlement status as reported by JavaScript, typically <c>"fulfilled"</c> or <c>"rejected"</c>.
	/// JavaScript 报告的结算状态，通常为 <c>"fulfilled"</c> 或 <c>"rejected"</c>。
	/// </summary>
	[Description("@#status")]
	public extern string Status { get; }

	/// <summary>
	/// Fulfillment value when <see cref="Status"/> is <c>"fulfilled"</c>.
	/// JavaScript exposes this as a data property on the result object.
	/// 当 <see cref="Status"/> 为 <c>"fulfilled"</c> 时的兑现值；JavaScript 将其作为结果对象的数据属性公开。
	/// </summary>
	[Description("@#value")]
	public extern T? Value { get; }

	/// <summary>
	/// Rejection reason when <see cref="Status"/> is <c>"rejected"</c>.
	/// JavaScript allows any value here, not just <see cref="Error"/>.
	/// 当 <see cref="Status"/> 为 <c>"rejected"</c> 时的拒绝原因；JavaScript 允许任意值，不限于 <see cref="Error"/>。
	/// </summary>
	[Description("@#reason")]
	public extern object? Reason { get; }
}

/// <summary>
/// JavaScript object shape returned by <c>Promise.withResolvers()</c>.
/// This stays explicit so the C# surface mirrors the runtime object instead of inventing a CLR-only helper abstraction.
/// JavaScript <c>Promise.withResolvers()</c> 返回的对象形状。保持显式类型，使 C# 表面镜像运行时对象而不发明 CLR 专用辅助抽象。
/// </summary>
[ECMAScript]
[Description("@#")]
/// <summary>Promise plus resolve/reject functions returned by <c>Promise.withResolvers</c>. Promise.withResolvers 返回的 Promise 与 resolve/reject 函数集合。</summary>
public sealed class PromiseWithResolvers
{
	/// <summary>
	/// Promise instance paired with the resolver callbacks returned by JavaScript <c>Promise.withResolvers()</c>.
	/// The non-generic host still carries arbitrary JavaScript fulfillment values, so the promise surface remains value-bearing here.
	/// JavaScript <c>Promise.withResolvers()</c> 返回的 Promise；非泛型宿主仍可承载任意 JavaScript 兑现值。
	/// </summary>
	[Description("@#promise")]
	public extern IPromise<object?> Promise { get; }

	/// <summary>
	/// Fulfillment callback paired with <see cref="Promise"/>.
	/// It is modeled as a property because JavaScript returns it as a function-valued field on the result object.
	/// The non-generic host still accepts an arbitrary JavaScript fulfillment value.
	/// 与 <see cref="Promise"/> 配对的兑现回调；JavaScript 结果对象将它作为函数值字段公开，非泛型版本接受任意兑现值。
	/// </summary>
	[Description("@#resolve")]
	public extern Action<object?> Resolve { get; }

	/// <summary>
	/// JavaScript promises can be rejected with any value, not just <see cref="Error"/>.
	/// 拒绝回调；JavaScript Promise 可使用任意值拒绝，不限于 <see cref="Error"/>。
	/// </summary>
	[Description("@#reject")]
	public extern Action<object?> Reject { get; }
}

/// <summary>
/// Typed JavaScript object shape returned by <c>Promise.withResolvers()</c> when C# wants to preserve the fulfillment type.
/// 当 C# 需要保留兑现值类型时，JavaScript <c>Promise.withResolvers()</c> 返回的类型化对象形状。
/// </summary>
[ECMAScript]
[Description("@#")]
/// <summary>Typed resolver record returned by <c>Promise.withResolvers</c>. 泛型 Promise.withResolvers 返回的结果集合。</summary>
public sealed class PromiseWithResolvers<T>
{
	/// <summary>
	/// Typed promise instance paired with the resolver callbacks returned by JavaScript <c>Promise.withResolvers()</c>.
	/// 与 JavaScript <c>Promise.withResolvers()</c> 回调配对的类型化 Promise 实例。
	/// </summary>
	[Description("@#promise")]
	public extern IPromise<T> Promise { get; }

	/// <summary>
	/// Fulfillment callback paired with <see cref="Promise"/>.
	/// It is modeled as a property because JavaScript returns it as a function-valued field on the result object.
	/// 与 <see cref="Promise"/> 配对的类型化兑现回调；使用属性是因为 JavaScript 在结果对象上以函数值字段返回它。
	/// </summary>
	[Description("@#resolve")]
	public extern Action<T> Resolve { get; }

	/// <summary>
	/// JavaScript promises can be rejected with any value, not just <see cref="Error"/>.
	/// 拒绝回调；JavaScript Promise 可使用任意值拒绝，不限于 <see cref="Error"/>。
	/// </summary>
	[Description("@#reject")]
	public extern Action<object?> Reject { get; }
}

[ECMAScript]
[Description("@#Promise")]
/// <summary>
/// Host binding for the JavaScript <c>Promise</c> constructor, static combinators, and instance methods.
/// JavaScript <c>Promise</c> 构造器、静态组合方法和实例方法的宿主绑定。
/// </summary>
/// <remarks>
/// Promise execution, microtask scheduling, and rejection propagation are owned by the JavaScript runtime; this type provides only a strongly typed authoring surface.
/// CLR-specific semantics such as delayed task startup are handled separately by Jazor.CLR.
/// Promise 的执行、微任务调度和异常传播由 JavaScript runtime 负责；本类型只提供强类型
/// authoring surface。Task 映射中的延迟启动等 CLR 特殊语义由 Jazor.CLR 另行处理。
/// </remarks>
public class Promise : IPromise
{
    /// <summary>
    /// JavaScript <c>Promise.prototype</c> object.
    /// This stays on the non-generic constructor host so the public surface still reads like the JavaScript runtime.
    /// JavaScript <c>Promise.prototype</c> 对象；保留在非泛型构造器宿主上，使公开表面保持 JavaScript 运行时形状。
    /// </summary>
    [Description("@#prototype")]
    public static extern Promise Prototype { get; }

    /// <summary>
    /// Returns a promise that is already resolved.
    /// 返回已兑现的 Promise；即便已兑现，后续处理器仍按 JavaScript 微任务队列安排。
    /// </summary>
    /// <remarks>This is useful for wrapping code in a promise without having to worry if the callback in
    /// the Promise constructor throws an exception.</remarks>
    /// <returns></returns>
    [Description("@#resolve")]
    public static extern IPromise Resolve();

    /// <summary>
    /// Returns a promise that is resolved with the supplied JavaScript runtime value.
    /// This non-generic host overload keeps the JavaScript <c>Promise.resolve(value)</c> entry point available without forcing a generic type choice in C# first.
    /// 返回以提供的 JavaScript 运行时值兑现的 Promise；非泛型重载保留 <c>Promise.resolve(value)</c>，无需 C# 先选择泛型类型。
    /// </summary>
    [Description("@#resolve")]
    public static extern IPromise<object?> Resolve(object? value);

    /// <summary>
    /// Returns a promise that adopts the supplied JavaScript promise-like value.
    /// This overload preserves the non-generic promise host in C# when the input is already promise-shaped.
    /// 接管提供的 JavaScript Promise-like 值；输入已经是 Promise 形状时，在 C# 中保留非泛型 Promise 宿主。
    /// </summary>
    [Description("@#resolve")]
    public static extern IPromise Resolve(IPromise value);

    /// <summary>
    /// Bridge-only overload for compiler-lowered async results that should follow JavaScript <c>Promise.resolve</c> assimilation.
    /// 编译器 lowering 专用的异步结果桥接重载；运行时遵循 JavaScript <c>Promise.resolve</c> 的接管语义。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#resolve")]
    public static extern IPromise Resolve(PromiseResult value);

    /// <summary>
    /// Returns a promise that has already been rejected.
    /// 返回已使用 <see cref="Error"/> 拒绝的 Promise；处理器仍按 JavaScript 微任务语义安排。
    /// </summary>
    /// <param name="e">The exception with which to reject the promise.</param>
    /// <returns></returns>
    [Description("@#reject")]
    public static extern IPromise Reject(Error e);

    /// <summary>
    /// Returns a promise that has already been rejected with an arbitrary JavaScript reason value.
    /// JavaScript rejection reasons are not limited to <see cref="Error"/> instances.
    /// 返回已使用任意 JavaScript 原因值拒绝的 Promise；拒绝原因不限于 <see cref="Error"/> 实例。
    /// </summary>
    [Description("@#reject")]
    public static extern IPromise Reject(object? reason);

    /// <summary>
    /// Creates a JavaScript resolver record containing a promise plus its paired resolve and reject callbacks.
    /// 创建包含 Promise 及其成对 resolve/reject 回调的 JavaScript resolver record。
    /// </summary>
    [Description("@#withResolvers")]
    public static extern PromiseWithResolvers WithResolvers();

    /// <summary>
    /// Calls the callback immediately and wraps its completion in a JavaScript promise.
    /// This is the direct projection of <c>Promise.try(callback)</c> for callbacks that do not return a value.
    /// 立即调用回调并将完成结果包装到 JavaScript Promise；这是无返回值回调的 <c>Promise.try(callback)</c> 直接投影。
    /// </summary>
    [Description("@#try")]
    public static extern IPromise Try(Action callback);

    /// <summary>
    /// Calls the callback immediately and wraps the returned JavaScript promise in the usual promise assimilation rules.
    /// This keeps the host close to <c>Promise.try(callback)</c> without forcing callers through CLR task abstractions.
    /// 立即调用回调并按 JavaScript 接管规则包装其 Promise 返回值，不强迫调用方经过 CLR Task 抽象。
    /// </summary>
    [Description("@#try")]
    public static extern IPromise Try(Func<IPromise> callback);

    /// <summary>
    /// Bridge-only overload for compiler-lowered async callbacks that surface <see cref="PromiseResult"/> instead of an explicit promise object.
    /// 编译器 lowering 专用的异步回调桥接重载，回调以 <see cref="PromiseResult"/> 而非显式 Promise 对象表达结果。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#try")]
    public static extern IPromise Try(Func<PromiseResult> callback);

    /// <summary>
    /// Calls the callback immediately and resolves the returned value through JavaScript promise semantics.
    /// This is the direct projection of <c>Promise.try(callback)</c> for typed fulfillment values.
    /// 立即调用回调，并按 JavaScript Promise 语义解析类型化兑现值；这是 <c>Promise.try(callback)</c> 的直接投影。
    /// </summary>
    [Description("@#try")]
    public static extern IPromise<T> Try<T>(Func<T> callback);

    /// <summary>
    /// Calls the callback immediately and adopts the returned typed JavaScript promise.
    /// 立即调用回调并接管返回的类型化 JavaScript Promise。
    /// </summary>
    [Description("@#try")]
    public static extern IPromise<T> Try<T>(Func<IPromise<T>> callback);

    /// <summary>
    /// Bridge-only overload for compiler-lowered async callbacks that surface <see cref="PromiseResult{T}"/> instead of an explicit promise object.
    /// 编译器 lowering 专用的类型化异步回调桥接重载。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#try")]
    public static extern IPromise<T> Try<T>(Func<PromiseResult<T>> callback);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.all</c> with separate arguments.
    /// JavaScript itself takes a single iterable.
    /// Nullable element types are used because non-generic JavaScript promises may fulfill with <see langword="null" />.
    /// C# 兼容重载，允许以分散参数调用 <c>Promise.all</c>；JavaScript 本身只接受一个 iterable。非泛型 Promise 可兑现为 <see langword="null"/>，故元素类型可空。
    /// </summary>
    /// <param name="promises">Promises to wait on.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#all")]
    public static extern IPromise All(params IPromise[] promises);

    /// <summary>
    /// C# projection of the JavaScript <c>Promise.all(iterable)</c> overload.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// JavaScript <c>Promise.all(iterable)</c> 的 C# 投影；任一输入拒绝时立即拒绝最终 Promise，所有输入兑现后才兑现。
    /// </summary>
    /// <param name="promises">Promises to wait on.</param>
    /// <returns></returns>
    [Description("@#all")]
    public static extern IPromise All(IEnumerable<IPromise> promises);

    /// <summary>
    /// Returns a promise that will resolve when all passed in tasks are resolved.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// async lowering 专用桥接重载；等待全部 <see cref="PromiseResult"/> 完成，任一拒绝时立即拒绝最终 Promise。
    /// </summary>
    /// <param name="tasks">PromiseResults on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#all")]
    public static extern IPromise All(params PromiseResult[] tasks);

    /// <summary>
    /// Returns a promise that will resolve when all tasks in the Enumerable are resolved.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// async lowering 专用 iterable 桥接重载；拒绝短路规则与 JavaScript <c>Promise.all</c> 一致。
    /// </summary>
    /// <param name="tasks"></param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#all")]
    public static extern IPromise All(IEnumerable<PromiseResult> tasks);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.all</c> with separate arguments.
    /// JavaScript itself takes a single iterable.
    /// C# 兼容重载，允许以分散参数调用带值 Promise 的 <c>Promise.all</c>；最终兑现值保持为运行时 <see cref="object"/> 数组。
    /// </summary>
    /// <param name="promises"></param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#all")]
    public static extern IPromise<object?[]> All(params IPromise<object?>[] promises);

    /// <summary>
    /// C# projection of the JavaScript <c>Promise.all(iterable)</c> overload.
    /// The final promise will contain the results of the passed in promises. You will
    /// need to cast them to their final types.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// JavaScript <c>Promise.all(iterable)</c> 的非泛型值承载投影；任一拒绝时立即拒绝，全部兑现时以 <see cref="object"/> 数组兑现。
    /// </summary>
    /// <param name="promises"></param>
    /// <returns></returns>
    [Description("@#all")]
    public static extern IPromise<object?[]> All(IEnumerable<IPromise<object?>> promises);

    /// <summary>
    /// Returns a promise that will resolve when all tasks passed in are resolved.
    /// The final promise will contain the results of the passed in tasks. You will
    /// need to cast them to their final types.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// async lowering 专用桥接重载；最终兑现值为 <see cref="object"/> 数组，拒绝时短路。
    /// </summary>
    /// <param name="tasks"></param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#all")]
    public static extern IPromise<object?[]> All(params PromiseResult<object?>[] tasks);

    /// <summary>
    /// Returns a promise that will resolve when all tasks in the Enumerable are resolved.
    /// The final promise will contain the results of the passed in tasks. You will
    /// need to cast them to their final types.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// async lowering 专用 iterable 桥接重载；最终兑现值为 <see cref="object"/> 数组，拒绝时短路。
    /// </summary>
    /// <param name="tasks"><see cref="IEnumerable{T}"/> of <see cref="PromiseResult{TResult}"/>s on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#all")]
    public static extern IPromise<object?[]> All(IEnumerable<PromiseResult<object?>> tasks);

    /// <summary>
    /// C# projection of JavaScript <c>Promise.allSettled(iterable)</c>.
    /// JavaScript returns settlement records instead of short-circuiting on rejection, so the resulting promise always fulfills.
    /// JavaScript <c>Promise.allSettled(iterable)</c> 的 C# 投影；它返回所有结算记录而非在拒绝时短路，因此结果 Promise 总会兑现。
    /// </summary>
    /// <param name="promises">Promises to observe.</param>
    /// <returns>A promise that fulfills with JavaScript settlement result objects.</returns>
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<object?>[]> AllSettled(IEnumerable<IPromise> promises);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.allSettled</c> with separate arguments.
    /// JavaScript itself takes a single iterable.
    /// C# 兼容重载，允许以分散参数调用 <c>Promise.allSettled</c>；JavaScript 本身只接受一个 iterable。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<object?>[]> AllSettled(params IPromise[] promises);

    /// <summary>
    /// Compatibility overload used by async lowering. JavaScript itself takes a single iterable.
    /// async lowering 使用的 iterable 兼容重载。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<object?>[]> AllSettled(IEnumerable<PromiseResult> tasks);

    /// <summary>
    /// Compatibility overload used by async lowering. JavaScript itself takes a single iterable.
    /// async lowering 使用的分散参数兼容重载。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<object?>[]> AllSettled(params PromiseResult[] tasks);

    /// <summary>
    /// C# projection of JavaScript <c>Promise.allSettled(iterable)</c> for typed promises.
    /// The promise fulfills with the original fulfillment type preserved on each settlement record.
    /// 类型化 JavaScript <c>Promise.allSettled(iterable)</c> 投影；每个结算记录保留原兑现值类型标注。
    /// </summary>
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<T>[]> AllSettled<T>(IEnumerable<IPromise<T>> promises);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.allSettled</c> with separate typed arguments.
    /// JavaScript itself takes a single iterable.
    /// C# 兼容重载，允许以分散类型化参数调用 <c>Promise.allSettled</c>。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<T>[]> AllSettled<T>(params IPromise<T>[] promises);

    /// <summary>
    /// Compatibility overload used by async lowering for typed promise results.
    /// JavaScript itself takes a single iterable.
    /// async lowering 使用的类型化 iterable 兼容重载。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<T>[]> AllSettled<T>(IEnumerable<PromiseResult<T>> tasks);

    /// <summary>
    /// Compatibility overload used by async lowering for typed promise results.
    /// JavaScript itself takes a single iterable.
    /// async lowering 使用的类型化分散参数兼容重载。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<T>[]> AllSettled<T>(params PromiseResult<T>[] tasks);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.any</c> with separate arguments.
    /// JavaScript itself takes a single iterable.
    /// C# 兼容重载，允许以分散参数调用 <c>Promise.any</c>；JavaScript 本身只接受一个 iterable。
    /// </summary>
    /// <param name="promises"><see cref="IPromise"/>s on which to wait</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise Any(params IPromise[] promises);

    /// <summary>
    /// C# projection of the JavaScript <c>Promise.any(iterable)</c> overload.
    /// Returns a promise that fulfills with the first fulfilled input, or rejects with JavaScript AggregateError semantics when all inputs reject.
    /// JavaScript <c>Promise.any(iterable)</c> 的 C# 投影；第一个兑现的输入决定结果，若全部拒绝则以 <c>AggregateError</c> 语义拒绝，而不是使用第一个拒绝原因。
    /// </summary>
    /// <param name="promises"><see cref="IEnumerable{T}"/> of <see cref="IPromise"/>s on which to wait</param>
    /// <returns></returns>
    [Description("@#any")]
    public static extern IPromise Any(IEnumerable<IPromise> promises);

    /// <summary>
    /// Returns a promise that fulfills with the first fulfilled input, or rejects with JavaScript AggregateError semantics when all inputs reject.
    /// Nullable is used because the winning JavaScript fulfillment value may be <see langword="null" />.
    /// async lowering 专用桥接重载；第一个兑现结果获胜，全部拒绝时按 JavaScript <c>Promise.any</c> 拒绝语义处理。
    /// </summary>
    /// <param name="tasks"><see cref="PromiseResult"/>s on which to wait</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise Any(params PromiseResult[] tasks);

    /// <summary>
    /// Returns a promise that fulfills with the first fulfilled input, or rejects with JavaScript AggregateError semantics when all inputs reject.
    /// async lowering 专用 iterable 桥接重载；第一个兑现结果获胜。
    /// </summary>
    /// <param name="tasks"><see cref="IEnumerable{T}"/> of <see cref="PromiseResult"/>s on which to wait</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise Any(IEnumerable<PromiseResult> tasks);

    /// <summary>
    /// <summary>Bridge overload of <c>Promise.any</c> for typed async results. 面向类型化异步结果的 <c>Promise.any</c> 桥接重载。</summary>
    /// </summary>
    /// <param name="tasks"><see cref="PromiseResult{TResult}"/>s on which to wait</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise<object?> Any(params PromiseResult<object?>[] tasks);

    /// <summary>
    /// <summary>Iterable bridge overload of <c>Promise.any</c> for typed async results. 面向类型化异步结果的 iterable <c>Promise.any</c> 桥接重载。</summary>
    /// </summary>
    /// <param name="tasks"><see cref="IEnumerable{T}"/> of <see cref="PromiseResult{TResult}"/>s on which to wait</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise<object?> Any(IEnumerable<PromiseResult<object?>> tasks);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.any</c> with separate arguments.
    /// JavaScript itself takes a single iterable.
    /// C# 兼容重载，允许以分散带值 Promise 参数调用 <c>Promise.any</c>。
    /// </summary>
    /// <param name="promises"><see cref="IPromise{T}"/>s on which to wait</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise<object?> Any(params IPromise<object?>[] promises);

    /// <summary>
    /// C# projection of the JavaScript <c>Promise.any(iterable)</c> overload.
    /// It fulfills with the first fulfilled input, or rejects with AggregateError semantics when all inputs reject.
    /// JavaScript <c>Promise.any(iterable)</c> 的 C# 投影；第一个兑现输入决定结果，全部拒绝时按 AggregateError 语义拒绝。
    /// </summary>
    /// <param name="promises"><see cref="IEnumerable{T}"/> of <see cref="IPromise{T}"/>s on which to wait</param>
    /// <returns></returns>
    [Description("@#any")]
    public static extern IPromise<object?> Any(IEnumerable<IPromise<object?>> promises);

    /// <summary>
    /// Returns a promise that fulfills as soon as any of the given promises fulfill,
    /// or rejects if all of them reject.
    /// 类型化 <c>Promise.any</c> 投影；第一个兑现值保留类型标注，全部拒绝时按 AggregateError 语义拒绝。
    /// </summary>
    [Description("@#any")]
    public static extern IPromise<T> Any<T>(IEnumerable<IPromise<T>> promises);

    /// <summary>C# params convenience overload for typed <c>Promise.any</c>. 类型化 <c>Promise.any</c> 的 C# 分散参数便利重载。</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise<T> Any<T>(params IPromise<T>[] promises);

    /// <summary>
    /// Returns a <see cref="IPromise"/> that is resolved as soon as any one of the promises
    /// in the <see cref="IEnumerable{T}"/> resolves.
    /// JavaScript <c>Promise.race(iterable)</c> 的 C# 投影；第一个结算的输入（兑现或拒绝）决定最终 Promise。
    /// </summary>
    /// <param name="promises"><see cref="IEnumerable{T}"/> of <see cref="IPromise"/>s on which to wait.</param>
    /// <returns></returns>
    [Description("@#race")]
    public static extern IPromise Race(IEnumerable<IPromise> promises);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.race</c> with separate arguments.
    /// JavaScript itself takes a single iterable.
    /// C# 兼容重载，允许以分散参数调用 <c>Promise.race</c>。
    /// </summary>
    /// <param name="promises"><see cref="IPromise"/>s on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise Race(params IPromise[] promises);

    /// <summary>
    /// Returns a <see cref="IPromise"/> that is resolved as soon as any one of the passed in promises resolves.
    /// async lowering 专用 iterable 桥接重载；第一个结算结果（兑现或拒绝）决定最终 Promise。
    /// </summary>
    /// <param name="tasks"><see cref="PromiseResult"/>s on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise Race(IEnumerable<PromiseResult> tasks);

    /// <summary>
    /// Returns a <see cref="IPromise"/> that is resolved as soon as any one of the passed in promises resolves.
    /// async lowering 专用分散参数桥接重载；第一个结算结果决定最终 Promise。
    /// </summary>
    /// <param name="tasks"><see cref="PromiseResult"/>s on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise Race(params PromiseResult[] tasks);

    /// <summary>
    /// Returns a <see cref="IPromise{T}"/> that is resolved as soon as any one of the passed in promises resolves.
    /// Nullable is used because the winning JavaScript fulfillment value may be <see langword="null" />.
    /// 带值 <c>Promise.race</c> 投影；第一个结算输入决定结果，兑现值可为 <see langword="null"/>。
    /// </summary>
    /// <param name="promises"><see cref="IEnumerable{T}"/> of <see cref="IPromise{T}"/>s on which to wait.</param>
    /// <returns></returns>
    [Description("@#race")]
    public static extern IPromise<object?> Race(IEnumerable<IPromise<object?>> promises);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.race</c> with separate arguments.
    /// JavaScript itself takes a single iterable.
    /// C# 兼容重载，允许以分散带值 Promise 参数调用 <c>Promise.race</c>。
    /// </summary>
    /// <param name="promises"><see cref="IPromise{T}"/>s on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise<object?> Race(params IPromise<object?>[] promises);

    /// <summary>
    /// Returns a <see cref="IPromise{T}"/> that is resolved as soon as any one of the passed in promises resolves.
    /// </summary>
    /// <param name="tasks"><see cref="IEnumerable{T}"/> of <see cref="PromiseResult{TResult}"/>s on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise<object?> Race(IEnumerable<PromiseResult<object?>> tasks);

    /// <summary>
    /// Returns a <see cref="IPromise{T}"/> that is resolved as soon as any one of the passed in promises resolves.
    /// </summary>
    /// <param name="tasks"><see cref="PromiseResult{TResult}"/>s on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise<object?> Race(params PromiseResult<object?>[] tasks);

    protected extern Promise();

    /// <summary>
    /// Creates a promise whose executor can resolve with an arbitrary JavaScript fulfillment value.
    /// The non-generic host uses <see cref="Action{T}"/> so C# can express the standard JavaScript <c>resolve(value)</c> shape directly.
    /// 创建可用任意 JavaScript 兑现值 resolve 的 Promise；非泛型宿主使用 <see cref="Action{T}"/>，使 C# 可直接表达标准 <c>resolve(value)</c> 形状。
    /// </summary>
    /// <param name="callback">Callback that can use the first parameter to resolve the promise.</param>
    public extern Promise(Action<Action<object?>> callback);

    /// <summary>
    /// Creates a promise that can be resolved or rejected with the passed in callback.
    /// The resolve callback accepts arbitrary JavaScript fulfillment values.
    /// 创建可通过回调 resolve 或 reject 的 Promise；resolve 回调接受任意 JavaScript 兑现值。
    /// </summary>
    /// <param name="callback">Callback that can use the first parameter to resolve the promise,
    /// and the second parameter to reject the promise.</param>
    public extern Promise(Action<Action<object?>, Action> callback);

    /// <summary>
    /// Creates a promise that can be resolved or rejected with the passed in callback.
    /// The resolve callback accepts arbitrary JavaScript fulfillment values.
    /// 创建可通过回调 resolve 或使用 <see cref="Error"/> reject 的 Promise；resolve 回调接受任意 JavaScript 兑现值。
    /// </summary>
    /// <param name="callback">Callback that can use the first parameter to resolve the promise,
    /// and the second parameter to reject the promise with a given exception.</param>
    public extern Promise(Action<Action<object?>, Action<Error>> callback);

    /// <summary>
    /// Creates a promise that can be resolved or rejected with the passed in callback.
    /// JavaScript allows the reject callback to receive any runtime value, not only <see cref="Error"/> instances.
    /// 创建可通过回调 resolve 或 reject 的 Promise；JavaScript reject 可接收任意运行时值，不限于 <see cref="Error"/>。
    /// </summary>
    public extern Promise(Action<Action<object?>, Action<object?>> callback);

    /// <summary>Chains a fulfillment action. 链接一个兑现处理回调。</summary>
    [Description("@#then")]
    public extern IPromise Then(Action onFulfilled);

    /// <summary>Chains fulfillment and rejection actions. 链接兑现与拒绝处理回调。</summary>
    [Description("@#then")]
    public extern IPromise Then(Action onFulfilled, Action onRejected);

    /// <summary>Chains fulfillment and Error-projected rejection actions. 链接兑现与 Error 投影的拒绝处理回调。</summary>
    [Description("@#then")]
    public extern IPromise Then(Action onFulfilled, Action<Error> onRejected);

    /// <summary>Chains a fulfillment mapping callback. 链接一个兑现映射回调。</summary>
    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<T> onFulfilled);

    /// <summary>Chains fulfillment mapping and rejection actions. 链接兑现映射与拒绝处理回调。</summary>
    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<T> onFulfilled, Action onRejected);

    /// <summary>Chains fulfillment mapping and an Error-projected rejection action. 链接兑现映射与 Error 投影的拒绝处理回调。</summary>
    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<T> onFulfilled, Action<Error> onRejected);

    /// <summary>Chains a callback whose returned promise is automatically adopted. 链接一个返回 Promise 且会被自动接管的回调。</summary>
    [Description("@#then")]
    public extern IPromise Then(Func<IPromise> onFulfilled);

    /// <summary>Chains a returned Promise with a rejection action. 链接返回 Promise 的回调及拒绝处理。</summary>
    [Description("@#then")]
    public extern IPromise Then(Func<IPromise> onFulfilled, Action onRejected);

    /// <summary>Chains a returned Promise with an Error-projected rejection action. 链接返回 Promise 的回调及 Error 投影的拒绝处理。</summary>
    [Description("@#then")]
    public extern IPromise Then(Func<IPromise> onFulfilled, Action<Error> onRejected);

    /// <summary>Chains a callback whose typed returned Promise is automatically adopted. 链接一个类型化返回 Promise 且会被自动接管的回调。</summary>
    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled);

    /// <summary>Chains a typed returned Promise with a rejection action. 链接类型化返回 Promise 的回调及拒绝处理。</summary>
    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled, Action onRejected);

    /// <summary>Chains a typed returned Promise with an Error-projected rejection action. 链接类型化返回 Promise 的回调及 Error 投影的拒绝处理。</summary>
    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled, Action<Error> onRejected);

    /// <summary>CLR convenience bridge for a concrete <see cref="Promise{T}"/> return; JavaScript still adopts a normal promise-like value. 返回具体 <see cref="Promise{T}"/> 的 CLR 便利桥接；JavaScript 仍正常接管 Promise-like 值。</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<Promise<T>> onFulfilled);

    /// <summary>CLR bridge for a concrete Promise return with rejection handling. 带拒绝处理的具体 Promise 返回 CLR 桥接。</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<Promise<T>> onFulfilled, Action onRejected);

    /// <summary>CLR bridge for a concrete Promise return with Error-projected rejection handling. 带 Error 投影拒绝处理的具体 Promise 返回 CLR 桥接。</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<Promise<T>> onFulfilled, Action<Error> onRejected);

    /// <summary>Compiler-only async bridge returning <see cref="PromiseResult"/>. 返回 <see cref="PromiseResult"/> 的 compiler-only 异步桥接。</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<PromiseResult> onFulfilled);

    /// <summary>Compiler-only async bridge with rejection handling. 带拒绝处理的 compiler-only 异步桥接。</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<PromiseResult> onFulfilled, Action onRejected);

    /// <summary>Compiler-only async bridge with Error-projected rejection handling. 带 Error 投影拒绝处理的 compiler-only 异步桥接。</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<PromiseResult> onFulfilled, Action<Error> onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<PromiseResult<T>> onFulfilled);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<PromiseResult<T>> onFulfilled, Action onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<PromiseResult<T>> onFulfilled, Action<Error> onRejected);

    /// <summary>Observes rejection and returns a chained Promise. 观察拒绝并返回链式 Promise。</summary>
    [Description("@#catch")]
    public extern IPromise Catch(Action<Error> onError);

    /// <summary>Runs a final callback without changing fulfillment unless it throws or rejects. 执行最终回调；除非回调抛出或拒绝，否则不改变兑现结果。</summary>
    [Description("@#finally")]
    public extern IPromise Finally(Action onFinal);
}

[ECMAScript]
[Description("@#Promise")]  
/// <summary>
/// JavaScript <c>Promise</c> host binding with a compile-time fulfillment type annotation.
/// 带编译期兑现结果类型标注的 JavaScript <c>Promise</c> 宿主绑定。
/// </summary>
public sealed class Promise<T> : Promise, IPromise<T>
{
    /// <summary>
    /// Returns a promise that is resolved with the <paramref name="arg"/> value.
    /// 使用 <paramref name="arg"/> 兑现 Promise；泛型只保留 C# 侧的兑现值类型标注。
    /// </summary>
    /// <param name="arg">Value to use to resolve this promise</param>
    /// <returns></returns>    
    [Description("@#resolve")]
    public static extern IPromise<T> Resolve(T arg);

    /// <summary>
    /// Returns a promise that adopts the supplied typed JavaScript promise-like value.
    /// This keeps the fulfillment type visible in C# while matching JavaScript <c>Promise.resolve</c> assimilation semantics.
    /// 接管提供的类型化 Promise-like 值；在 C# 中保留兑现类型，同时匹配 JavaScript <c>Promise.resolve</c> 接管语义。
    /// </summary>
    [Description("@#resolve")]
    public static extern IPromise<T> Resolve(IPromise<T> arg);

    /// <summary>
    /// Bridge-only overload for compiler-lowered async results that should preserve the typed fulfillment shape.
    /// 编译器 lowering 专用桥接重载，用于保留类型化兑现形状。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#resolve")]
    public static extern IPromise<T> Resolve(PromiseResult<T> arg);

    /// <summary>
    /// Returns a promise that is rejected with the <paramref name="ex"/> exception.
    /// 返回以 <paramref name="ex"/> 拒绝的类型化 Promise；拒绝值不会携带 <typeparamref name="T"/>。
    /// </summary>
    /// <param name="ex">Exception used to reject this promise.</param>
    /// <returns></returns>
    [Description("@#reject")]
    public static extern new IPromise<T> Reject(Error ex);

    /// <summary>
    /// Returns a promise that is rejected with an arbitrary JavaScript reason value.
    /// JavaScript rejection reasons are not limited to <see cref="Error"/> instances.
    /// 返回以任意 JavaScript 原因值拒绝的类型化 Promise；拒绝原因不限于 <see cref="Error"/>。
    /// </summary>
    [Description("@#reject")]
    public static extern new IPromise<T> Reject(object? reason);

    /// <summary>
    /// Creates a typed JavaScript resolver record containing a promise plus its paired resolve and reject callbacks.
    /// 创建包含类型化 Promise 及其 resolve/reject 回调的 JavaScript resolver record。
    /// </summary>
    [Description("@#withResolvers")]
    public static extern new PromiseWithResolvers<T> WithResolvers();

    /// <summary>
    /// Calls the callback immediately and resolves the returned value through JavaScript promise semantics.
    /// This typed host keeps the fulfillment type visible in C# while still mapping to <c>Promise.try(callback)</c>.
    /// 立即调用回调并按 JavaScript Promise 规则解析返回值；泛型宿主仅在 C# 中保留兑现类型。
    /// </summary>
    [Description("@#try")]
    public static extern IPromise<T> Try(Func<T> callback);

    /// <summary>
    /// Calls the callback immediately and adopts the returned typed JavaScript promise.
    /// 立即调用回调并接管返回的类型化 JavaScript Promise。
    /// </summary>
    [Description("@#try")]
    public static extern IPromise<T> Try(Func<IPromise<T>> callback);

    /// <summary>
    /// Bridge-only overload for compiler-lowered async callbacks that surface <see cref="PromiseResult{T}"/> instead of an explicit promise object.
    /// 编译器 lowering 专用异步桥接重载，回调以 <see cref="PromiseResult{T}"/> 表达结果。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#try")]
    public static extern IPromise<T> Try(Func<PromiseResult<T>> callback);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.all</c> with separate arguments.
    /// JavaScript itself takes a single iterable.
    /// C# 兼容重载，允许以分散参数调用类型化 <c>Promise.all</c>。
    /// </summary>
    /// <param name="promises"></param>
    /// <returns></returns>
    [Description("@#all")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static extern IPromise<T[]> All(params IPromise<T>[] promises);

    /// <summary>
    /// C# projection of the JavaScript <c>Promise.all(iterable)</c> overload.
    /// Returns a promise that will resolve when all promises in the iterable are resolved.
    /// The final promise will contain the results of the passed in promises.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// JavaScript <c>Promise.all(iterable)</c> 的类型化投影；所有输入兑现时以 <typeparamref name="T"/> 数组兑现，任一拒绝时短路。
    /// </summary>
    /// <param name="promises"></param>
    /// <returns></returns>
    [Description("@#all")]
    public static extern IPromise<T[]> All(IEnumerable<IPromise<T>> promises);

    [Description("@#all")]
    /// <summary>
    /// Compatibility overload used by async lowering. JavaScript itself takes a single iterable.
    /// Returns a promise that will resolve when all passed in tasks are resolved.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// async lowering 专用类型化桥接重载；任一任务拒绝时短路，全部完成后兑现 <typeparamref name="T"/> 数组。
    /// </summary>
    /// <param name="tasks">PromiseResults on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static extern IPromise<T[]> All(params PromiseResult<T>[] tasks);

    /// <summary>
    /// Compatibility overload used by async lowering. JavaScript itself takes a single iterable.
    /// Returns a promise that will resolve when all passed in tasks are resolved.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// async lowering 专用类型化 iterable 桥接重载；拒绝短路规则与 JavaScript <c>Promise.all</c> 一致。
    /// </summary>
    /// <param name="tasks"><see cref="IEnumerable{T}"/> of <see cref="PromiseResult{TResult}"/> values on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#all")]
    public static extern IPromise<T[]> All(IEnumerable<PromiseResult<T>> tasks);

    /// <summary>
    /// C# projection of JavaScript <c>Promise.allSettled(iterable)</c> for the current generic promise host.
    /// This keeps the API surface aligned with JavaScript while preserving the fulfillment type in C#.
    /// 与 JavaScript API 对齐，同时在 C# 中保留每个结算记录的兑现类型。
    /// </summary>
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<T>[]> AllSettled(IEnumerable<IPromise<T>> promises);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.allSettled</c> with separate typed arguments.
    /// JavaScript itself takes a single iterable.
    /// C# 兼容重载，允许以分散类型化参数调用 <c>Promise.allSettled</c>；所有结果均记录，不因拒绝短路。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<T>[]> AllSettled(params IPromise<T>[] promises);

    /// <summary>
    /// Compatibility overload used by async lowering. JavaScript itself takes a single iterable.
    /// async lowering 使用的类型化 iterable 兼容重载；结果始终为结算记录数组。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<T>[]> AllSettled(IEnumerable<PromiseResult<T>> tasks);

    /// <summary>
    /// Compatibility overload used by async lowering. JavaScript itself takes a single iterable.
    /// async lowering 使用的类型化分散参数兼容重载；结果始终为结算记录数组。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<T>[]> AllSettled(params PromiseResult<T>[] tasks);

    /// <summary>Returns the first fulfilled typed promise, or rejects when all inputs reject. 返回首个兑现的类型化 Promise；全部拒绝时拒绝。</summary>
    [Description("@#any")]
    public static extern IPromise<T> Any(IEnumerable<IPromise<T>> promises);

    /// <summary>Params convenience overload for typed <c>Promise.any</c>. 类型化 <c>Promise.any</c> 的分散参数便利重载。</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise<T> Any(params IPromise<T>[] promises);

    /// <summary>Async bridge overload for typed <c>Promise.any</c>. 类型化 <c>Promise.any</c> 的异步桥接重载。</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise<T> Any(IEnumerable<PromiseResult<T>> tasks);

    /// <summary>Params async bridge overload for typed <c>Promise.any</c>. 类型化 <c>Promise.any</c> 的分散参数异步桥接重载。</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise<T> Any(params PromiseResult<T>[] tasks);

    /// <summary>
    /// Returns a promise that settles with the first settled promise from the iterable.
    /// This is the typed projection of JavaScript <c>Promise.race(iterable)</c>.
    /// 第一个输入结算（兑现或拒绝）即决定最终 Promise，而不是等待全部输入。
    /// </summary>
    [Description("@#race")]
    public static extern IPromise<T> Race(IEnumerable<IPromise<T>> promises);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.race</c> with separate typed arguments.
    /// JavaScript itself takes a single iterable.
    /// C# 兼容重载，允许以分散类型化参数调用 <c>Promise.race</c>。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise<T> Race(params IPromise<T>[] promises);

    /// <summary>
    /// Compatibility overload used by async lowering for typed promise results.
    /// JavaScript itself takes a single iterable.
    /// async lowering 使用的类型化 iterable 桥接重载；第一个结算结果决定最终 Promise。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise<T> Race(IEnumerable<PromiseResult<T>> tasks);

    /// <summary>
    /// Compatibility overload used by async lowering for typed promise results.
    /// JavaScript itself takes a single iterable.
    /// async lowering 使用的类型化分散参数桥接重载；第一个结算结果决定最终 Promise。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise<T> Race(params PromiseResult<T>[] tasks);

    /// <summary>
    /// Creates a promise that can be resolved with the passed in <see cref="Action{T}"/>.
    /// The value passed to the <see cref="Action{T}"/> will be used as the parameter to any <see cref="Then(Action{T})"/> calls.
    /// 创建类型化 Promise；resolve 回调传入的值会成为后续 <see cref="Then(Action{T})"/> 的兑现参数。
    /// </summary>
    /// <param name="callback">Callback that can use the first parameter to resolve the promise.</param>
    public extern Promise(Action<Action<T>> callback);

    /// <summary>
    /// Creates a promise that can be resolved or rejected with the passed in <see cref="Action{T1, T2}"/>.
    /// The value passed to the resolve <see cref="Action{T}"/> will be used as the parameter to any <see cref="Then(Action{T})"/> calls.
    /// 创建可 resolve/reject 的类型化 Promise；resolve 值会沿后续 then 链传递。
    /// </summary>
    /// <param name="callback">Callback that can use the first parameter to resolve the promise,
    /// and the second parameter to reject the promise.</param>
    public extern Promise(Action<Action<T>, Action> callback);

    /// <summary>
    /// Creates a promise that can be resolved or rejected with the passed in <see cref="Action{T1, T2}"/>.
    /// The value passed to the resolve <see cref="Action{T}"/> will be used as the parameter to any <see cref="Then(Action{T})"/> calls.
    /// The value passed to the reject <see cref="Action{T}"/> will be used as the parameter to any
    /// <see cref="Then(Action{T}, Action{Error})"/> calls, or any <see cref="IPromise.Catch(Action{Error})"/>
    /// calls.
    /// reject 回调可接收 Error 投影；JavaScript 实际仍允许任意拒绝原因值。
    /// </summary>
    /// <param name="callback">Callback that can use the first parameter to resolve the promise,
    /// and the second parameter to reject the promise with a given exception.</param>
    public extern Promise(Action<Action<T>, Action<Error>> callback);

    /// <summary>
    /// Creates a promise that can be resolved or rejected with the passed in callback.
    /// JavaScript allows the reject callback to receive any runtime value, not only <see cref="Error"/> instances.
    /// 创建可携带任意 JavaScript 拒绝原因的类型化 Promise。
    /// </summary>
    public extern Promise(Action<Action<T>, Action<object?>> callback);

    /// <summary>Chains a typed fulfillment action. 链接类型化兑现处理回调。</summary>
    [Description("@#then")]
    public extern IPromise Then(Action<T> onFulfilled);

    /// <summary>Chains typed fulfillment and rejection actions. 链接类型化兑现与拒绝处理回调。</summary>
    [Description("@#then")]
    public extern IPromise Then(Action<T> onFulfilled, Action onRejected);

    /// <summary>Chains typed fulfillment and Error-projected rejection actions. 链接类型化兑现与 Error 投影的拒绝处理回调。</summary>
    [Description("@#then")]
    public extern IPromise Then(Action<T> onFulfilled, Action<Error> onRejected);

    /// <summary>Maps a typed fulfillment value to a new promise result. 将类型化兑现值映射为新的 Promise 结果。</summary>
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action onRejected);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action<Error> onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<T, PromiseResult<TResult>> onFulfilled);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<T, PromiseResult<TResult>> onFulfilled, Action onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<T, PromiseResult<TResult>> onFulfilled, Action<Error> onRejected);

    [Description("@#then")]
    public extern IPromise Then(Func<T, IPromise> onResolve);

    [Description("@#then")]
    public extern IPromise Then(Func<T, IPromise> onResolve, Action onRejected);

    [Description("@#then")]
    public extern IPromise Then(Func<T, IPromise> onResolve, Action<Error> onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<T, PromiseResult> onFulfilled);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<T, PromiseResult> onFulfilled, Action onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<T, PromiseResult> onFulfilled, Action<Error> onRejected);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<T, IPromise<TResult>> onFulfilled);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<T, IPromise<TResult>> onFulfilled, Action onRejected);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<T, IPromise<TResult>> onFulfilled, Action<Error> onRejected);

    /// <summary>Runs a final callback while preserving the typed fulfillment value unless it throws or rejects. 执行最终回调并保留类型化兑现值，除非回调抛出或拒绝。</summary>
    [Description("@#finally")]
    public extern new IPromise<T> Finally(Action onFinal);
}
