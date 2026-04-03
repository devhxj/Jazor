using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace ECMAScript;

/// <summary>
/// JavaScript promise shape used for instance members such as <c>then</c>, <c>catch</c>, and <c>finally</c>.
/// This bridge interface is intentionally hidden so editor completion stays focused on runtime hosts like <see cref="Promise"/>.
/// </summary>
[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IPromise
{
    [Description("@#catch")]
    IPromise Catch(Action<Error> onError);

    [Description("@#finally")]
    IPromise Finally(Action onFinal);

    /// <summary>
    /// Returns a new promise that will be resolved when the passed in action is finished.
    /// </summary>
    /// <param name="onFulfilled">Action to be invoked on resolution</param>
    /// <returns></returns>
    [Description("@#then")]
    IPromise Then(Action onFulfilled);
    /// <summary>
    /// Returns a new promise that will be resolved when one of the passed in actions is finished.
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
/// </summary>
[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IPromise<T> : IPromise
{
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
/// </summary>
[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PromiseResult : IAsyncResult
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public extern object AsyncState { get; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public extern WaitHandle AsyncWaitHandle { get; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern bool CompletedSynchronously { get; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern bool IsCompleted { get; }

	/// <summary>
	/// Bridge-only completed sentinel used by async lowering.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern static PromiseResult Completed { get; }
}

/// <summary>
/// Bridge-only placeholder used by async lowering and generated bindings where JavaScript would normally expose a promise or awaited value.
/// This type is not a JavaScript runtime global and its CLR members do not map to JavaScript instance members.
/// </summary>
[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PromiseResult<TResult> : IAsyncResult
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern object AsyncState { get; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern WaitHandle AsyncWaitHandle { get; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern bool CompletedSynchronously { get; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern bool IsCompleted { get; }

	/// <summary>
	/// Bridge-only completed sentinel used by async lowering.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern static PromiseResult<TResult> Completed { get; }
}

/// <summary>
/// JavaScript object shape returned by <c>Promise.allSettled</c>.
/// The object exposes both <c>value</c> and <c>reason</c> fields in JavaScript, but only one is meaningful for a given <see cref="Status"/>.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class PromiseSettledResult<T>
{
	/// <summary>
	/// Settlement status as reported by JavaScript, typically <c>"fulfilled"</c> or <c>"rejected"</c>.
	/// </summary>
	[Description("@#status")]
	public extern string Status { get; }

	/// <summary>
	/// Fulfillment value when <see cref="Status"/> is <c>"fulfilled"</c>.
	/// JavaScript exposes this as a data property on the result object.
	/// </summary>
	[Description("@#value")]
	public extern T? Value { get; }

	/// <summary>
	/// Rejection reason when <see cref="Status"/> is <c>"rejected"</c>.
	/// JavaScript allows any value here, not just <see cref="Error"/>.
	/// </summary>
	[Description("@#reason")]
	public extern object? Reason { get; }
}

/// <summary>
/// JavaScript object shape returned by <c>Promise.withResolvers()</c>.
/// This stays explicit so the C# surface mirrors the runtime object instead of inventing a CLR-only helper abstraction.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class PromiseWithResolvers
{
	/// <summary>
	/// Promise instance paired with the resolver callbacks returned by JavaScript <c>Promise.withResolvers()</c>.
	/// </summary>
	[Description("@#promise")]
	public extern IPromise Promise { get; }

	/// <summary>
	/// Fulfillment callback paired with <see cref="Promise"/>.
	/// It is modeled as a property because JavaScript returns it as a function-valued field on the result object.
	/// </summary>
	[Description("@#resolve")]
	public extern Action Resolve { get; }

	/// <summary>
	/// JavaScript promises can be rejected with any value, not just <see cref="Error"/>.
	/// </summary>
	[Description("@#reject")]
	public extern Action<object?> Reject { get; }
}

/// <summary>
/// Typed JavaScript object shape returned by <c>Promise.withResolvers()</c> when C# wants to preserve the fulfillment type.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class PromiseWithResolvers<T>
{
	/// <summary>
	/// Typed promise instance paired with the resolver callbacks returned by JavaScript <c>Promise.withResolvers()</c>.
	/// </summary>
	[Description("@#promise")]
	public extern IPromise<T> Promise { get; }

	/// <summary>
	/// Fulfillment callback paired with <see cref="Promise"/>.
	/// It is modeled as a property because JavaScript returns it as a function-valued field on the result object.
	/// </summary>
	[Description("@#resolve")]
	public extern Action<T> Resolve { get; }

	/// <summary>
	/// JavaScript promises can be rejected with any value, not just <see cref="Error"/>.
	/// </summary>
	[Description("@#reject")]
	public extern Action<object?> Reject { get; }
}

[ECMAScript]
[Description("@#Promise")]
public class Promise : IPromise
{
    /// <summary>
    /// Returns a promise that is already resolved.
    /// </summary>
    /// <remarks>This is useful for wrapping code in a promise without having to worry if the callback in
    /// the Promise constructor throws an exception.</remarks>
    /// <returns></returns>
    [Description("@#resolve")]
    public static extern IPromise Resolve();

    /// <summary>
    /// Returns a promise that is resolved with the supplied JavaScript runtime value.
    /// This non-generic host overload keeps the JavaScript <c>Promise.resolve(value)</c> entry point available without forcing a generic type choice in C# first.
    /// </summary>
    [Description("@#resolve")]
    public static extern IPromise<object?> Resolve(object? value);

    /// <summary>
    /// Returns a promise that has already been rejected.
    /// </summary>
    /// <param name="e">The exception with which to reject the promise.</param>
    /// <returns></returns>
    [Description("@#reject")]
    public static extern IPromise Reject(Error e);

    /// <summary>
    /// Returns a promise that has already been rejected with an arbitrary JavaScript reason value.
    /// JavaScript rejection reasons are not limited to <see cref="Error"/> instances.
    /// </summary>
    [Description("@#reject")]
    public static extern IPromise Reject(object? reason);

    /// <summary>
    /// Creates a JavaScript resolver record containing a promise plus its paired resolve and reject callbacks.
    /// </summary>
    [Description("@#withResolvers")]
    public static extern PromiseWithResolvers WithResolvers();

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.all</c> with separate arguments.
    /// JavaScript itself takes a single iterable.
    /// </summary>
    /// <param name="promises">Promises to wait on.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#all")]
    public static extern IPromise All(params IPromise[] promises);

    /// <summary>
    /// C# projection of the JavaScript <c>Promise.all(iterable)</c> overload.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// </summary>
    /// <param name="promises">Promises to wait on.</param>
    /// <returns></returns>
    [Description("@#all")]
    public static extern IPromise All(IEnumerable<IPromise> promises);

    /// <summary>
    /// Returns a promise that will resolve when all passed in tasks are resolved.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// </summary>
    /// <param name="tasks">PromiseResults on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#all")]
    public static extern IPromise All(params PromiseResult[] tasks);

    /// <summary>
    /// Returns a promise that will resolve when all tasks in the Enumerable are resolved.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// </summary>
    /// <param name="tasks"></param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#all")]
    public static extern IPromise All(IEnumerable<PromiseResult> tasks);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.all</c> with separate arguments.
    /// JavaScript itself takes a single iterable.
    /// </summary>
    /// <param name="promises"></param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#all")]
    public static extern IPromise<object[]> All(params IPromise<object>[] promises);

    /// <summary>
    /// C# projection of the JavaScript <c>Promise.all(iterable)</c> overload.
    /// The final promise will contain the results of the passed in promises. You will
    /// need to cast them to their final types.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// </summary>
    /// <param name="promises"></param>
    /// <returns></returns>
    [Description("@#all")]
    public static extern IPromise<object[]> All(IEnumerable<IPromise<object>> promises);

    /// <summary>
    /// Returns a promise that will resolve when all tasks passed in are resolved.
    /// The final promise will contain the results of the passed in tasks. You will
    /// need to cast them to their final types.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// </summary>
    /// <param name="tasks"></param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#all")]
    public static extern IPromise<object[]> All(params PromiseResult<object>[] tasks);

    /// <summary>
    /// Returns a promise that will resolve when all tasks in the Enumerable are resolved.
    /// The final promise will contain the results of the passed in tasks. You will
    /// need to cast them to their final types.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// </summary>
    /// <param name="tasks"><see cref="IEnumerable{T}"/> of <see cref="PromiseResult{TResult}"/>s on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#all")]
    public static extern IPromise<object[]> All(IEnumerable<PromiseResult<object>> tasks);

    /// <summary>
    /// C# projection of JavaScript <c>Promise.allSettled(iterable)</c>.
    /// JavaScript returns settlement records instead of short-circuiting on rejection, so the resulting promise always fulfills.
    /// </summary>
    /// <param name="promises">Promises to observe.</param>
    /// <returns>A promise that fulfills with JavaScript settlement result objects.</returns>
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<object?>[]> AllSettled(IEnumerable<IPromise> promises);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.allSettled</c> with separate arguments.
    /// JavaScript itself takes a single iterable.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<object?>[]> AllSettled(params IPromise[] promises);

    /// <summary>
    /// Compatibility overload used by async lowering. JavaScript itself takes a single iterable.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<object?>[]> AllSettled(IEnumerable<PromiseResult> tasks);

    /// <summary>
    /// Compatibility overload used by async lowering. JavaScript itself takes a single iterable.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<object?>[]> AllSettled(params PromiseResult[] tasks);

    /// <summary>
    /// C# projection of JavaScript <c>Promise.allSettled(iterable)</c> for typed promises.
    /// The promise fulfills with the original fulfillment type preserved on each settlement record.
    /// </summary>
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<T>[]> AllSettled<T>(IEnumerable<IPromise<T>> promises);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.allSettled</c> with separate typed arguments.
    /// JavaScript itself takes a single iterable.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<T>[]> AllSettled<T>(params IPromise<T>[] promises);

    /// <summary>
    /// Compatibility overload used by async lowering for typed promise results.
    /// JavaScript itself takes a single iterable.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<T>[]> AllSettled<T>(IEnumerable<PromiseResult<T>> tasks);

    /// <summary>
    /// Compatibility overload used by async lowering for typed promise results.
    /// JavaScript itself takes a single iterable.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<T>[]> AllSettled<T>(params PromiseResult<T>[] tasks);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.any</c> with separate arguments.
    /// JavaScript itself takes a single iterable.
    /// </summary>
    /// <param name="promises"><see cref="IPromise"/>s on which to wait</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise Any(params IPromise[] promises);

    /// <summary>
    /// C# projection of the JavaScript <c>Promise.any(iterable)</c> overload.
    /// Returns a <see cref="IPromise"/> that resolves if any of the promises resolve. If none do, it
    /// resolves with the first to reject.
    /// </summary>
    /// <param name="promises"><see cref="IEnumerable{T}"/> of <see cref="IPromise"/>s on which to wait</param>
    /// <returns></returns>
    [Description("@#any")]
    public static extern IPromise Any(IEnumerable<IPromise> promises);

    /// <summary>
    /// Returns a <see cref="IPromise"/> that resolves if any of the tasks resolve. If none do, it
    /// resolves with the first to reject.
    /// </summary>
    /// <param name="tasks"><see cref="PromiseResult"/>s on which to wait</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise Any(params PromiseResult[] tasks);

    /// <summary>
    /// Returns a <see cref="IPromise"/> that resolves if any of the tasks resolve. If none do, it
    /// resolves with the first to reject.
    /// </summary>
    /// <param name="tasks"><see cref="IEnumerable{T}"/> of <see cref="PromiseResult"/>s on which to wait</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise Any(IEnumerable<PromiseResult> tasks);

    /// <summary>
    /// Returns a <see cref="IPromise"/> that resolves if any of the tasks resolve. If none do, it
    /// resolves with the first to reject.
    /// </summary>
    /// <param name="tasks"><see cref="PromiseResult{TResult}"/>s on which to wait</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise<object> Any(params PromiseResult<object>[] tasks);

    /// <summary>
    /// Returns a <see cref="IPromise"/> that resolves if any of the tasks resolve. If none do, it
    /// resolves with the first to reject.
    /// </summary>
    /// <param name="tasks"><see cref="IEnumerable{T}"/> of <see cref="PromiseResult{TResult}"/>s on which to wait</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise<object> Any(IEnumerable<PromiseResult<object>> tasks);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.any</c> with separate arguments.
    /// JavaScript itself takes a single iterable.
    /// </summary>
    /// <param name="promises"><see cref="IPromise{T}"/>s on which to wait</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise<object> Any(params IPromise<object>[] promises);

    /// <summary>
    /// C# projection of the JavaScript <c>Promise.any(iterable)</c> overload.
    /// Returns a <see cref="IPromise"/> that resolves if any of the promises resolve. If none do, it
    /// resolves with the first to reject.
    /// </summary>
    /// <param name="promises"><see cref="IEnumerable{T}"/> of <see cref="IPromise{T}"/>s on which to wait</param>
    /// <returns></returns>
    [Description("@#any")]
    public static extern IPromise<object> Any(IEnumerable<IPromise<object>> promises);

    /// <summary>
    /// Returns a promise that fulfills as soon as any of the given promises fulfill,
    /// or rejects if all of them reject.
    /// </summary>
    [Description("@#any")]
    public static extern IPromise<T> Any<T>(IEnumerable<IPromise<T>> promises);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise<T> Any<T>(params IPromise<T>[] promises);

    /// <summary>
    /// Returns a <see cref="IPromise"/> that is resolved as soon as any one of the promises
    /// in the <see cref="IEnumerable{T}"/> resolves.
    /// </summary>
    /// <param name="promises"><see cref="IEnumerable{T}"/> of <see cref="IPromise"/>s on which to wait.</param>
    /// <returns></returns>
    [Description("@#race")]
    public static extern IPromise Race(IEnumerable<IPromise> promises);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.race</c> with separate arguments.
    /// JavaScript itself takes a single iterable.
    /// </summary>
    /// <param name="promises"><see cref="IPromise"/>s on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise Race(params IPromise[] promises);

    /// <summary>
    /// Returns a <see cref="IPromise"/> that is resolved as soon as any one of the passed in promises resolves.
    /// </summary>
    /// <param name="tasks"><see cref="PromiseResult"/>s on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise Race(IEnumerable<PromiseResult> tasks);

    /// <summary>
    /// Returns a <see cref="IPromise"/> that is resolved as soon as any one of the passed in promises resolves.
    /// </summary>
    /// <param name="tasks"><see cref="PromiseResult"/>s on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise Race(params PromiseResult[] tasks);

    /// <summary>
    /// Returns a <see cref="IPromise{T}"/> that is resolved as soon as any one of the passed in promises resolves.
    /// </summary>
    /// <param name="promises"><see cref="IEnumerable{T}"/> of <see cref="IPromise{T}"/>s on which to wait.</param>
    /// <returns></returns>
    [Description("@#race")]
    public static extern IPromise<object> Race(IEnumerable<IPromise<object>> promises);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.race</c> with separate arguments.
    /// JavaScript itself takes a single iterable.
    /// </summary>
    /// <param name="promises"><see cref="IPromise{T}"/>s on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise<object> Race(params IPromise<object>[] promises);

    /// <summary>
    /// Returns a <see cref="IPromise{T}"/> that is resolved as soon as any one of the passed in promises resolves.
    /// </summary>
    /// <param name="tasks"><see cref="IEnumerable{T}"/> of <see cref="PromiseResult{TResult}"/>s on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise<object> Race(IEnumerable<PromiseResult<object>> tasks);

    /// <summary>
    /// Returns a <see cref="IPromise{T}"/> that is resolved as soon as any one of the passed in promises resolves.
    /// </summary>
    /// <param name="tasks"><see cref="PromiseResult{TResult}"/>s on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise<object> Race(params PromiseResult<object>[] tasks);

    protected extern Promise();

    /// <summary>
    /// Creates a promise that can be resolved with the passed in callback
    /// </summary>
    /// <param name="callback">Callback that can use the first parameter to resolve the promise.</param>
    public extern Promise(Action<Action> callback);

    /// <summary>
    /// Creates a promise that can be resolved or rejected with the passed in callback.
    /// </summary>
    /// <param name="callback">Callback that can use the first parameter to resolve the promise,
    /// and the second parameter to reject the promise.</param>
    public extern Promise(Action<Action, Action> callback);

    /// <summary>
    /// Creates a promise that can be resolved or rejected with the passed in callback.
    /// </summary>
    /// <param name="callback">Callback that can use the first parameter to resolve the promise,
    /// and the second parameter to reject the promise with a given exception.</param>
    public extern Promise(Action<Action, Action<Error>> callback);

    /// <summary>
    /// Creates a promise that can be resolved or rejected with the passed in callback.
    /// JavaScript allows the reject callback to receive any runtime value, not only <see cref="Error"/> instances.
    /// </summary>
    public extern Promise(Action<Action, Action<object?>> callback);

    [Description("@#then")]
    public extern IPromise Then(Action onFulfilled);

    [Description("@#then")]
    public extern IPromise Then(Action onFulfilled, Action onRejected);

    [Description("@#then")]
    public extern IPromise Then(Action onFulfilled, Action<Error> onRejected);

    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<T> onFulfilled);

    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<T> onFulfilled, Action onRejected);

    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<T> onFulfilled, Action<Error> onRejected);

    [Description("@#then")]
    public extern IPromise Then(Func<IPromise> onFulfilled);

    [Description("@#then")]
    public extern IPromise Then(Func<IPromise> onFulfilled, Action onRejected);

    [Description("@#then")]
    public extern IPromise Then(Func<IPromise> onFulfilled, Action<Error> onRejected);

    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled);

    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled, Action onRejected);

    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled, Action<Error> onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<Promise<T>> onFulfilled);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<Promise<T>> onFulfilled, Action onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<Promise<T>> onFulfilled, Action<Error> onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<PromiseResult> onFulfilled);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<PromiseResult> onFulfilled, Action onRejected);

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

    [Description("@#catch")]
    public extern IPromise Catch(Action<Error> onError);

    [Description("@#finally")]
    public extern IPromise Finally(Action onFinal);
}

[ECMAScript]
[Description("@#Promise")]  
public sealed class Promise<T> : Promise, IPromise<T>
{
    /// <summary>
    /// Returns a promise that is resolved with the <paramref name="arg"/> value.
    /// </summary>
    /// <param name="arg">Value to use to resolve this promise</param>
    /// <returns></returns>    
    [Description("@#resolve")]
    public static extern IPromise<T> Resolve(T arg);

    [Description("@#reject")]
    /// <summary>
    /// Returns a promise that is rejected with the <paramref name="ex"/> exception.
    /// </summary>
    /// <param name="ex">Exception used to reject this promise.</param>
    /// <returns></returns>
    public static extern new IPromise<T> Reject(Error ex);

    /// <summary>
    /// Returns a promise that is rejected with an arbitrary JavaScript reason value.
    /// JavaScript rejection reasons are not limited to <see cref="Error"/> instances.
    /// </summary>
    [Description("@#reject")]
    public static extern new IPromise<T> Reject(object? reason);

    /// <summary>
    /// Creates a typed JavaScript resolver record containing a promise plus its paired resolve and reject callbacks.
    /// </summary>
    [Description("@#withResolvers")]
    public static extern new PromiseWithResolvers<T> WithResolvers();

    [Description("@#all")]
    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.all</c> with separate arguments.
    /// JavaScript itself takes a single iterable.
    /// </summary>
    /// <param name="promises"></param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static extern IPromise<T[]> All(params IPromise<T>[] promises);

    [Description("@#all")]
    /// <summary>
    /// C# projection of the JavaScript <c>Promise.all(iterable)</c> overload.
    /// Returns a promise that will resolve when all promises in the iterable are resolved.
    /// The final promise will contain the results of the passed in promises.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// </summary>
    /// <param name="promises"></param>
    /// <returns></returns>
    public static extern IPromise<T[]> All(IEnumerable<IPromise<T>> promises);

    [Description("@#all")]
    /// <summary>
    /// Compatibility overload used by async lowering. JavaScript itself takes a single iterable.
    /// Returns a promise that will resolve when all passed in tasks are resolved.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// </summary>
    /// <param name="tasks">PromiseResults on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static extern IPromise<T[]> All(params PromiseResult<T>[] tasks);

    /// <summary>
    /// Compatibility overload used by async lowering. JavaScript itself takes a single iterable.
    /// Returns a promise that will resolve when all passed in tasks are resolved.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// </summary>
    /// <param name="tasks"><see cref="IEnumerable{T}"/> of <see cref="PromiseResult{TResult}"/> values on which to wait.</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#all")]
    public static extern IPromise<T[]> All(IEnumerable<PromiseResult<T>> tasks);

    /// <summary>
    /// C# projection of JavaScript <c>Promise.allSettled(iterable)</c> for the current generic promise host.
    /// This keeps the API surface aligned with JavaScript while preserving the fulfillment type in C#.
    /// </summary>
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<T>[]> AllSettled(IEnumerable<IPromise<T>> promises);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.allSettled</c> with separate typed arguments.
    /// JavaScript itself takes a single iterable.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<T>[]> AllSettled(params IPromise<T>[] promises);

    /// <summary>
    /// Compatibility overload used by async lowering. JavaScript itself takes a single iterable.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<T>[]> AllSettled(IEnumerable<PromiseResult<T>> tasks);

    /// <summary>
    /// Compatibility overload used by async lowering. JavaScript itself takes a single iterable.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#allSettled")]
    public static extern IPromise<PromiseSettledResult<T>[]> AllSettled(params PromiseResult<T>[] tasks);

    [Description("@#any")]
    public static extern IPromise<T> Any(IEnumerable<IPromise<T>> promises);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise<T> Any(params IPromise<T>[] promises);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise<T> Any(IEnumerable<PromiseResult<T>> tasks);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#any")]
    public static extern IPromise<T> Any(params PromiseResult<T>[] tasks);

    /// <summary>
    /// Returns a promise that settles with the first settled promise from the iterable.
    /// This is the typed projection of JavaScript <c>Promise.race(iterable)</c>.
    /// </summary>
    [Description("@#race")]
    public static extern IPromise<T> Race(IEnumerable<IPromise<T>> promises);

    /// <summary>
    /// Compatibility overload that lets C# call <c>Promise.race</c> with separate typed arguments.
    /// JavaScript itself takes a single iterable.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise<T> Race(params IPromise<T>[] promises);

    /// <summary>
    /// Compatibility overload used by async lowering for typed promise results.
    /// JavaScript itself takes a single iterable.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise<T> Race(IEnumerable<PromiseResult<T>> tasks);

    /// <summary>
    /// Compatibility overload used by async lowering for typed promise results.
    /// JavaScript itself takes a single iterable.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#race")]
    public static extern IPromise<T> Race(params PromiseResult<T>[] tasks);

    /// <summary>
    /// Creates a promise that can be resolved with the passed in <see cref="Action{T}"/>.
    /// The value passed to the <see cref="Action{T}"/> will be used as the parameter to any <see cref="Then(Action{T})"/> calls.
    /// </summary>
    /// <param name="callback">Callback that can use the first parameter to resolve the promise.</param>
    public extern Promise(Action<Action<T>> callback);

    /// <summary>
    /// Creates a promise that can be resolved or rejected with the passed in <see cref="Action{T1, T2}"/>.
    /// The value passed to the resolve <see cref="Action{T}"/> will be used as the parameter to any <see cref="Then(Action{T})"/> calls.
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
    /// </summary>
    /// <param name="callback">Callback that can use the first parameter to resolve the promise,
    /// and the second parameter to reject the promise with a given exception.</param>
    public extern Promise(Action<Action<T>, Action<Error>> callback);

    /// <summary>
    /// Creates a promise that can be resolved or rejected with the passed in callback.
    /// JavaScript allows the reject callback to receive any runtime value, not only <see cref="Error"/> instances.
    /// </summary>
    public extern Promise(Action<Action<T>, Action<object?>> callback);

    [Description("@#then")]
    public extern IPromise Then(Action<T> onFulfilled);

    [Description("@#then")]
    public extern IPromise Then(Action<T> onFulfilled, Action onRejected);

    [Description("@#then")]
    public extern IPromise Then(Action<T> onFulfilled, Action<Error> onRejected);

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

    [Description("@#finally")]
    public extern new IPromise<T> Finally(Action onFinal);
}

