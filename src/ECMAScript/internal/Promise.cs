using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace ECMAScript;

[ECMAScript]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IPromise
{
    IPromise Catch(Action<Error> onError);

    IPromise Finally(Action onFinal);

    /// <summary>
    /// Returns a new promise that will be resolved when the passed in action is finished.
    /// </summary>
    /// <param name="onFulfilled">Action to be invoked on resolution</param>
    /// <returns></returns>
    IPromise Then(Action onFulfilled);
    /// <summary>
    /// Returns a new promise that will be resolved when one of the passed in actions is finished.
    /// </summary>
    /// <param name="onFulfilled">Action to be invoked when this promise is resolved</param>
    /// <param name="onRejected">Action to be invoked when this promise is rejected</param>
    /// <returns></returns>
    IPromise Then(Action onFulfilled, Action onRejected);
    /// <summary>
    /// Returns a new promise that will be resolved when one of the passed in actions is finished.
    /// </summary>
    /// <param name="onFulfilled">Action to be invoked when this promise is resolved</param>
    /// <param name="onRejected">Action to be invoked when this promise is rejected</param>
    /// <returns></returns>
    IPromise Then(Action onFufilled, Action<Error> onRejected);

    /// <summary>
    /// Returns a new promise that will be resolved with the return value of the passed
    /// in <see cref="Func{TResult}" />.
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved</param>
    /// <returns></returns>
    IPromise<T> Then<T>(Func<T> onFulfilled);
    /// <summary>
    /// Returns a promise that will be resolved with the return value of the passed in
    /// <see cref="Func{TResult}"/>, or <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    IPromise<T> Then<T>(Func<T> onFulfilled, Action onRejected);
    /// <summary>
    /// Returns a promise that will be resolved with the return value of the passed in
    /// <see cref="Func{TResult}"/>, or <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    IPromise<T> Then<T>(Func<T> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="IPromise{T}"/> returned from the passed in <see cref="Func{TResult}"/>
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved</param>
    /// <returns></returns>
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
    IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="IPromise"/> returned from the passed in <see cref="Func{TResult}"/>
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved</param>
    /// <returns></returns>
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
    IPromise Then(Func<IPromise> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="PromiseResult"/> returned from the passed in <see cref="Func{TResult}"/>
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved</param>
    /// <returns></returns>
    IPromise Then(Func<PromiseResult> onFulfilled);
    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="PromiseResult"/> returned from the passed in <see cref="Func{TResult}"/>, or
    /// <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    IPromise Then(Func<PromiseResult> onFulfilled, Action onRejected);
    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="PromiseResult"/> returned from the passed in <see cref="Func{TResult}"/>, or
    /// <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    IPromise Then(Func<PromiseResult> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="PromiseResult{TResult}"/> returned from the passed in <see cref="Func{TResult}"/>
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved</param>
    /// <returns></returns>
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
    IPromise<T> Then<T>(Func<PromiseResult<T>> onFulfilled, Action<Error> onRejected);
}

[ECMAScript]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IPromise<T> : IPromise
{
    new IPromise<T> Finally(Action onFinal);

    /// <summary>
    /// Returns a new promise that will be resolved when the passed in action is finished.
    /// </summary>
    /// <param name="onFulfilled"><see cref="Action{T}"/> to be invoked on resolution</param>
    /// <returns></returns>
    IPromise Then(Action<T> onFulfilled);
    /// <summary>
    /// Returns a new promise that will be resolved when one of the passed in actions is finished.
    /// </summary>
    /// <param name="onFulfilled"><see cref="Action{T}"/> to be invoked when this promise is resolved</param>
    /// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected</param>
    /// <returns></returns>
    IPromise Then(Action<T> onFulfilled, Action onRejected);

    /// <summary>
    /// Returns a new promise that will be resolved when one of the passed in actions is finished.
    /// </summary>
    /// <param name="onFulfilled"><see cref="Action{T}"/> to be invoked when this promise is resolved</param>
    /// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected</param>
    /// <returns></returns>
    IPromise Then(Action<T> onFulfilled, Action<Error> onRejected);


    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="PromiseResult{TResult}"/> returned from the passed in <see cref="Func{TResult}"/>
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{T, TResult}"/> to be invoked when this promise is resolved</param>
    /// <returns></returns>
    IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled);
    /// <summary>
    /// Returns a promise that will be resolved with the return value of the passed in
    /// <see cref="Func{TResult}"/>, or <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
    /// <param name="onFulfilled"><see cref="Func{T, TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action onRejected);
    /// <summary>
    /// Returns a promise that will be resolved with the return value of the passed in
    /// <see cref="Func{TResult}"/>, or <paramref name="onRejected"/> called when rejected.
    /// </summary>
    /// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
    /// <param name="onFulfilled"><see cref="Func{T, TResult}"/> to be invoked when this promise is resolved.</param>
    /// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected.</param>
    /// <returns></returns>
    IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// Returns a new promise that will be resolved with resolved value of the
    /// <see cref="PromiseResult{TResult}"/> returned from the passed in <see cref="Func{TResult}"/>
    /// </summary>
    /// <param name="onFulfilled"><see cref="Func{T, TResult}"/> to be invoked when this promise is resolved</param>
    /// <returns></returns>
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
    IPromise<TResult> Then<TResult>(Func<T, PromiseResult<TResult>> onFulfilled, Action<Error> onRejected);

    IPromise Then(Func<T, IPromise> onResolve);
    IPromise Then(Func<T, IPromise> onResolve, Action onRejected);
    IPromise Then(Func<T, IPromise> onResolve, Action<Error> onRejected);

    IPromise<TResult> Then<TResult>(Func<T, IPromise<TResult>> onFulfilled);
    IPromise<TResult> Then<TResult>(Func<T, IPromise<TResult>> onFulfilled, Action onRejected);
    IPromise<TResult> Then<TResult>(Func<T, IPromise<TResult>> onFulfilled, Action<Error> onRejected);

    IPromise Then(Func<T, PromiseResult> onFulfilled);
    IPromise Then(Func<T, PromiseResult> onFulfilled, Action onRejected);
    IPromise Then(Func<T, PromiseResult> onFulfilled, Action<Error> onRejected);
}

[ECMAScript]
public sealed class PromiseResult : IAsyncResult
{
    public extern object AsyncState { get; }

    public extern WaitHandle AsyncWaitHandle { get; }

	public extern bool CompletedSynchronously { get; }

	public extern bool IsCompleted { get; }

	public extern static PromiseResult Completed { get; }
}

[ECMAScript]
public sealed class PromiseResult<TResult> : IAsyncResult
{
	public extern object AsyncState { get; }

	public extern WaitHandle AsyncWaitHandle { get; }

	public extern bool CompletedSynchronously { get; }

	public extern bool IsCompleted { get; }

	public extern static PromiseResult<TResult> Completed { get; }
}

[ECMAScript]
[Description("@#promise")]
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
    /// Returns a promise that has already been rejected.
    /// </summary>
    /// <param name="e">The exception with which to reject the promise.</param>
    /// <returns></returns>
    [Description("@#reject")]
    public static extern IPromise Reject(Error e);

    /// <summary>
    /// Returns a promise that will resolve when all promises passed in are resolved.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// </summary>
    /// <param name="promises">Promises to wait on.</param>
    /// <returns></returns>
    [Description("@#all")]
    public static extern IPromise All(params IPromise[] promises);

    /// <summary>
    /// Returns a promise that will resolve when all promises in the Enumerable are resolved.
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
    [Description("@#all")]
    public static extern IPromise All(params PromiseResult[] tasks);

    /// <summary>
    /// Returns a promise that will resolve when all tasks in the Enumerable are resolved.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// </summary>
    /// <param name="tasks"></param>
    /// <returns></returns>
    [Description("@#all")]
    public static extern IPromise All(IEnumerable<PromiseResult> tasks);

    /// <summary>
    /// Returns a promise that will resolve when all promises passed in are resolved.
    /// The final promise will contain the results of the passed in promises. You will
    /// need to cast them to their final types.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// </summary>
    /// <param name="promises"></param>
    /// <returns></returns>
    [Description("@#all")]
    public static extern IPromise<object[]> All(params IPromise<object>[] promises);

    /// <summary>
    /// Returns a promise that will resolve when all promises passed in are resolved.
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
    [Description("@#all")]
    public static extern IPromise<object[]> All(IEnumerable<PromiseResult<object>> tasks);

    /// <summary>
    /// Returns a <see cref="IPromise"/> that resolves if any of the promises resolve. If none do, it
    /// resolves with the first to reject.
    /// </summary>
    /// <param name="promises"><see cref="IPromise"/>s on which to wait</param>
    /// <returns></returns>
    [Description("@#any")]
    public static extern IPromise Any(params IPromise[] promises);

    /// <summary>
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
    [Description("@#any")]
    public static extern IPromise Any(params PromiseResult[] tasks);

    /// <summary>
    /// Returns a <see cref="IPromise"/> that resolves if any of the tasks resolve. If none do, it
    /// resolves with the first to reject.
    /// </summary>
    /// <param name="tasks"><see cref="IEnumerable{T}"/> of <see cref="PromiseResult"/>s on which to wait</param>
    /// <returns></returns>
    [Description("@#any")]
    public static extern IPromise Any(IEnumerable<PromiseResult> tasks);

    /// <summary>
    /// Returns a <see cref="IPromise"/> that resolves if any of the tasks resolve. If none do, it
    /// resolves with the first to reject.
    /// </summary>
    /// <param name="tasks"><see cref="PromiseResult{TResult}"/>s on which to wait</param>
    /// <returns></returns>
    [Description("@#any")]
    public static extern IPromise<object> Any(params PromiseResult<object>[] tasks);

    /// <summary>
    /// Returns a <see cref="IPromise"/> that resolves if any of the tasks resolve. If none do, it
    /// resolves with the first to reject.
    /// </summary>
    /// <param name="tasks"><see cref="IEnumerable{T}"/> of <see cref="PromiseResult{TResult}"/>s on which to wait</param>
    /// <returns></returns>
    [Description("@#any")]
    public static extern IPromise<object> Any(IEnumerable<PromiseResult<object>> tasks);

    /// <summary>
    /// Returns a <see cref="IPromise"/> that resolves if any of the promises resolve. If none do, it
    /// resolves with the first to reject.
    /// </summary>
    /// <param name="promises"><see cref="IPromise{T}"/>s on which to wait</param>
    /// <returns></returns>
    [Description("@#any")]
    public static extern IPromise<object> Any(params IPromise<object>[] promises);

    /// <summary>
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
    /// Returns a <see cref="IPromise"/> that is resolved as soon as any one of the passed in promises resolves.
    /// </summary>
    /// <param name="promises"><see cref="IPromise"/>s on which to wait.</param>
    /// <returns></returns>
    [Description("@#race")]
    public static extern IPromise Race(params IPromise[] promises);

    /// <summary>
    /// Returns a <see cref="IPromise"/> that is resolved as soon as any one of the passed in promises resolves.
    /// </summary>
    /// <param name="tasks"><see cref="PromiseResult"/>s on which to wait.</param>
    /// <returns></returns>
    [Description("@#race")]
    public static extern IPromise Race(IEnumerable<PromiseResult> tasks);

    /// <summary>
    /// Returns a <see cref="IPromise"/> that is resolved as soon as any one of the passed in promises resolves.
    /// </summary>
    /// <param name="tasks"><see cref="PromiseResult"/>s on which to wait.</param>
    /// <returns></returns>
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
    /// Returns a <see cref="IPromise{T}"/> that is resolved as soon as any one of the passed in promises resolves.
    /// </summary>
    /// <param name="promises"><see cref="IPromise{T}"/>s on which to wait.</param>
    /// <returns></returns>
    [Description("@#race")]
    public static extern IPromise<object> Race(params IPromise<object>[] promises);

    /// <summary>
    /// Returns a <see cref="IPromise{T}"/> that is resolved as soon as any one of the passed in promises resolves.
    /// </summary>
    /// <param name="tasks"><see cref="IEnumerable{T}"/> of <see cref="PromiseResult{TResult}"/>s on which to wait.</param>
    /// <returns></returns>
    [Description("@#race")]
    public static extern IPromise<object> Race(IEnumerable<PromiseResult<object>> tasks);

    /// <summary>
    /// Returns a <see cref="IPromise{T}"/> that is resolved as soon as any one of the passed in promises resolves.
    /// </summary>
    /// <param name="tasks"><see cref="PromiseResult{TResult}"/>s on which to wait.</param>
    /// <returns></returns>
    [Description("@#race")]
    public static extern IPromise<object> Race(params PromiseResult<object>[] tasks);

    protected extern Promise();

    /// <summary>
    /// Creates a promise that can be resolved with the passed in callback
    /// </summary>
    /// <param name="callback">Callback that can use the first parameter to resolve the promise.</param>
    public extern Promise(Action<Action> callback);

    /// <summary>
    /// Creates a promise that can be resolved or rejeted with the passed in callback.
    /// </summary>
    /// <param name="callback">Callback that can use the first parameter to resolve the promise,
    /// and the second parameter to reject the promise.</param>
    public extern Promise(Action<Action, Action> callback);

    /// <summary>
    /// Creates a promise that can be resolved or rejeted with the passed in callback.
    /// </summary>
    /// <param name="callback">Callback that can use the first parameter to resolve the promise,
    /// and the second parameter to reject the promise with a given exception.</param>
    public extern Promise(Action<Action, Action<Error>> callback);

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

    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<Promise<T>> onFulfilled);

    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<Promise<T>> onFulfilled, Action onRejected);

    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<Promise<T>> onFulfilled, Action<Error> onRejected);

    [Description("@#then")]
    public extern IPromise Then(Func<PromiseResult> onFulfilled);

    [Description("@#then")]
    public extern IPromise Then(Func<PromiseResult> onFulfilled, Action onRejected);

    [Description("@#then")]
    public extern IPromise Then(Func<PromiseResult> onFulfilled, Action<Error> onRejected);

    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<PromiseResult<T>> onFulfilled);

    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<PromiseResult<T>> onFulfilled, Action onRejected);

    [Description("@#then")]
    public extern IPromise<T> Then<T>(Func<PromiseResult<T>> onFulfilled, Action<Error> onRejected);

    [Description("@#catch")]
    public extern IPromise Catch(Action<Error> onError);

    [Description("@#finally")]
    public extern IPromise Finally(Action onFinal);
}

[ECMAScript]
[Description("@#promise")]  
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

    [Description("@#all")]
    /// <summary>
    /// Returns a promise that will resolve when all promises passed in are resolved.
    /// The final promise will contain the results of the passed in promises.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// </summary>
    /// <param name="promises"></param>
    /// <returns></returns>
    public static extern IPromise<T[]> All(params IPromise<T>[] promises);

    [Description("@#all")]
    /// <summary>
    /// Returns a promise that will resolve when all passed in tasks are resolved.
    /// If any is rejected, it will stop waiting and reject the final promise.
    /// </summary>
    /// <param name="tasks">PromiseResults on which to wait.</param>
    /// <returns></returns>
    public static extern IPromise<T[]> All(params PromiseResult<T>[] tasks);

    [Description("@#any")]
    public static extern IPromise<T> Any(IEnumerable<IPromise<T>> promises);

    [Description("@#any")]
    public static extern IPromise<T> Any(params IPromise<T>[] promises);

    [Description("@#any")]
    public static extern IPromise<T> Any(IEnumerable<PromiseResult<T>> tasks);

    [Description("@#any")]
    public static extern IPromise<T> Any(params PromiseResult<T>[] tasks);

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

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<T, PromiseResult<TResult>> onFulfilled);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<T, PromiseResult<TResult>> onFulfilled, Action onRejected);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<T, PromiseResult<TResult>> onFulfilled, Action<Error> onRejected);

    [Description("@#then")]
    public extern IPromise Then(Func<T, IPromise> onResolve);

    [Description("@#then")]
    public extern IPromise Then(Func<T, IPromise> onResolve, Action onRejected);

    [Description("@#then")]
    public extern IPromise Then(Func<T, IPromise> onResolve, Action<Error> onRejected);

    [Description("@#then")]
    public extern IPromise Then(Func<T, PromiseResult> onFulfilled);

    [Description("@#then")]
    public extern IPromise Then(Func<T, PromiseResult> onFulfilled, Action onRejected);

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

