namespace Jazor.CLR;

[ECMAScriptModule("System/Threading/Tasks/TaskModule.js")]
[Jazor(Op.Alias, "System.Threading.Tasks.Task<TResult>", "Promise")]
public static class TaskModule<TResult>
{
	///<summary>Gets the result value of this <see cref="T:System.Threading.Tasks.Task`1" />.</summary>
	[Jazor(Op.Discard, "System.Threading.Tasks.Task<TResult>.Result.get")]
	public extern static TResult _18af0aa87004bfcc(System.Threading.Tasks.Task<TResult> instance);

	///<summary>Gets an awaiter used to await this <see cref="T:System.Threading.Tasks.Task`1" />.</summary>
	[Jazor(Op.Inline, "System.Threading.Tasks.Task<TResult>.GetAwaiter()", "Promise.resolve(__arg1)")]
	public extern static System.Runtime.CompilerServices.TaskAwaiter<TResult> _027217a9621e6f7b(System.Threading.Tasks.Task<TResult> instance);

	///<summary>Configures an awaiter used to await this <see cref="T:System.Threading.Tasks.Task`1" />.</summary>
	[Jazor(Op.Inline, "System.Threading.Tasks.Task<TResult>.ConfigureAwait(bool)", "Promise.resolve(__arg1)")]
	public extern static System.Runtime.CompilerServices.ConfiguredTaskAwaitable<TResult> _0e17ea5f64ad914f(System.Threading.Tasks.Task<TResult> instance, bool continueOnCapturedContext);

	///<summary>Configures an awaiter used to await this <see cref="T:System.Threading.Tasks.Task`1" />.</summary>
	[Jazor(Op.Inline, "System.Threading.Tasks.Task<TResult>.ConfigureAwait(System.Threading.Tasks.ConfigureAwaitOptions)", "Promise.resolve(__arg1)")]
	public extern static System.Runtime.CompilerServices.ConfiguredTaskAwaitable<TResult> _e315c5cff004ed53(System.Threading.Tasks.Task<TResult> instance, object options);

	///<summary>Gets a task that completes when this task completes or when cancellation is requested.</summary>
	[Jazor(Op.Inline, "System.Threading.Tasks.Task<TResult>.WaitAsync(System.Threading.CancellationToken)", "Promise.resolve(__arg1)")]
	public extern static System.Threading.Tasks.Task<TResult> _a5adb3e12ef3a8bb(System.Threading.Tasks.Task<TResult> instance, object cancellationToken);

	///<summary>Gets a task that completes when this task completes or when the specified timeout expires.</summary>
	[Jazor(Op.Inline, "System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan)", TaskInlineTemplates.WaitAsyncTimeSpan)]
	public extern static System.Threading.Tasks.Task<TResult> _408c4a7eefe8214c(System.Threading.Tasks.Task<TResult> instance, RuntimeModule.JTimeSpan timeout);

	///<summary>Gets a task that completes when this task completes or when the specified timeout expires.</summary>
	[Jazor(Op.Inline, "System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan, System.TimeProvider)", TaskInlineTemplates.WaitAsyncTimeSpan)]
	public extern static System.Threading.Tasks.Task<TResult> _35ae1f6899303439(System.Threading.Tasks.Task<TResult> instance, RuntimeModule.JTimeSpan timeout, object timeProvider);

	///<summary>Gets a task that completes when this task completes, when timeout expires, or when cancellation is requested.</summary>
	[Jazor(Op.Inline, "System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan, System.Threading.CancellationToken)", TaskInlineTemplates.WaitAsyncTimeSpan)]
	public extern static System.Threading.Tasks.Task<TResult> _05fbcc037540ba42(System.Threading.Tasks.Task<TResult> instance, RuntimeModule.JTimeSpan timeout, object cancellationToken);

	///<summary>Gets a task that completes when this task completes, when timeout expires, or when cancellation is requested.</summary>
	[Jazor(Op.Inline, "System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan, System.TimeProvider, System.Threading.CancellationToken)", TaskInlineTemplates.WaitAsyncTimeSpan)]
	public extern static System.Threading.Tasks.Task<TResult> _4b5b887e2099f8dd(System.Threading.Tasks.Task<TResult> instance, RuntimeModule.JTimeSpan timeout, object timeProvider, object cancellationToken);
}
