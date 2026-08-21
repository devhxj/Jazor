namespace Jazor.CLR;

/// <summary>
/// 将非泛型 System.Threading.Tasks.Task 的常用创建、等待和组合 API 投影为 Promise。
/// </summary>
/// <remarks>
/// Promise 本身通常立即拥有异步结果，但 C# Task 的部分创建 API 还具有延迟启动语义；
/// 本模块通过 runtime WeakMap 保存 starter 状态，不能简单把所有 Task.Factory 调用替换为 Promise.resolve。
/// </remarks>
[ECMAScriptModule("System/Threading/Tasks/TaskModule.js")]
[Jazor(Op.Alias, "System.Threading.Tasks.Task", "Promise")]
public static class TaskModule
{
	///<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Task(System.Action)", TaskInlineTemplates.ColdTask)]
	public extern static System.Threading.Tasks.Task _54056395d4c60189(object action);

	///<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action and <see cref="T:System.Threading.CancellationToken" />.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Task(System.Action, System.Threading.CancellationToken)", TaskInlineTemplates.ColdTaskCancellation)]
	public extern static System.Threading.Tasks.Task _85cc61f0768e2467(object action, AbortSignal cancellationToken);

	///<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action and creation options.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Task(System.Action, System.Threading.Tasks.TaskCreationOptions)", TaskInlineTemplates.ColdTask)]
	public extern static System.Threading.Tasks.Task _eff8e21064439c38(object action, object creationOptions);

	///<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action and creation options.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Task(System.Action, System.Threading.CancellationToken, System.Threading.Tasks.TaskCreationOptions)", TaskInlineTemplates.ColdTaskCancellation)]
	public extern static System.Threading.Tasks.Task _cec1128f4e8dc68a(object action, AbortSignal cancellationToken, object creationOptions);

	///<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action and state.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Task(System.Action<object>, object)", TaskInlineTemplates.ColdTaskWithState)]
	public extern static System.Threading.Tasks.Task _0be51a2dc3255844(object action, object? state);

	///<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action, state, and <see cref="T:System.Threading.CancellationToken" />.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Task(System.Action<object>, object, System.Threading.CancellationToken)", TaskInlineTemplates.ColdTaskWithStateCancellation)]
	public extern static System.Threading.Tasks.Task _9fcd22dde0dcd8a7(object action, object? state, AbortSignal cancellationToken);

	///<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action, state, and options.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Task(System.Action<object>, object, System.Threading.Tasks.TaskCreationOptions)", TaskInlineTemplates.ColdTaskWithState)]
	public extern static System.Threading.Tasks.Task _751384169b9f00a5(object action, object? state, object creationOptions);

	///<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action, state, and options.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Task(System.Action<object>, object, System.Threading.CancellationToken, System.Threading.Tasks.TaskCreationOptions)", TaskInlineTemplates.ColdTaskWithStateCancellation)]
	public extern static System.Threading.Tasks.Task _1e1dc0b6a7d9ae5a(object action, object? state, AbortSignal cancellationToken, object creationOptions);

	///<summary>Starts the <see cref="T:System.Threading.Tasks.Task" />, scheduling it for execution to the current <see cref="T:System.Threading.Tasks.TaskScheduler" />.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Start()", "((entry => { if (entry && entry.start) { entry.start(); const state = globalThis.__jazorTaskStates?.get(__arg1); if (state && state.status === \"created\") { state.status = \"pending\"; } } return undefined; })(globalThis.__jazorTaskStarters?.get(__arg1)))")]
	public extern static void _571f6c3f73cde8c3(System.Threading.Tasks.Task instance);

	///<summary>Starts the <see cref="T:System.Threading.Tasks.Task" />, scheduling it for execution to the specified <see cref="T:System.Threading.Tasks.TaskScheduler" />.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Start(System.Threading.Tasks.TaskScheduler)", "((entry => { if (entry && entry.start) { entry.start(); const state = globalThis.__jazorTaskStates?.get(__arg1); if (state && state.status === \"created\") { state.status = \"pending\"; } } return undefined; })(globalThis.__jazorTaskStarters?.get(__arg1)))")]
	public extern static void _5393d9342c25e912(System.Threading.Tasks.Task instance, object scheduler);

	///<summary>Runs the <see cref="T:System.Threading.Tasks.Task" /> synchronously on the current <see cref="T:System.Threading.Tasks.TaskScheduler" />.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.RunSynchronously()", "((entry => { if (entry && entry.start) { entry.start(); const state = globalThis.__jazorTaskStates?.get(__arg1); if (state && state.status === \"created\") { state.status = \"pending\"; } } return undefined; })(globalThis.__jazorTaskStarters?.get(__arg1)))")]
	public extern static void _1f6e131527687ab7(System.Threading.Tasks.Task instance);

	///<summary>Runs the <see cref="T:System.Threading.Tasks.Task" /> synchronously on the <see cref="T:System.Threading.Tasks.TaskScheduler" /> provided.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.RunSynchronously(System.Threading.Tasks.TaskScheduler)", "((entry => { if (entry && entry.start) { entry.start(); const state = globalThis.__jazorTaskStates?.get(__arg1); if (state && state.status === \"created\") { state.status = \"pending\"; } } return undefined; })(globalThis.__jazorTaskStarters?.get(__arg1)))")]
	public extern static void _930596f5e09d6af6(System.Threading.Tasks.Task instance, object scheduler);

	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Id.get", "((globalThis.__jazorTaskEnsureState ??= (task => { const states = globalThis.__jazorTaskStates ??= new WeakMap(); let state = states.get(task); if (!state) { const starterEntry = globalThis.__jazorTaskStarters?.get(task); state = { id: (globalThis.__jazorTaskNextId = (globalThis.__jazorTaskNextId ?? 0) + 1), status: (starterEntry && !starterEntry.started) ? \"created\" : \"pending\", error: null, asyncState: null }; states.set(task, state); Promise.resolve(task).then(() => { state.status = \"fulfilled\"; state.error = null; }, (error) => { state.status = \"rejected\"; state.error = error; }); } return state; }))(__arg1).id)")]
	public extern static Number _631607ea76b1f24d(System.Threading.Tasks.Task instance);

	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.CurrentId.get", "null")]
	public extern static Number? _77f2902849fd5781();

	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Exception.get", "((s => s.status === \"rejected\" ? s.error : null)((globalThis.__jazorTaskEnsureState ??= (task => { const states = globalThis.__jazorTaskStates ??= new WeakMap(); let state = states.get(task); if (!state) { const starterEntry = globalThis.__jazorTaskStarters?.get(task); state = { id: (globalThis.__jazorTaskNextId = (globalThis.__jazorTaskNextId ?? 0) + 1), status: (starterEntry && !starterEntry.started) ? \"created\" : \"pending\", error: null, asyncState: null }; states.set(task, state); Promise.resolve(task).then(() => { state.status = \"fulfilled\"; state.error = null; }, (error) => { state.status = \"rejected\"; state.error = error; }); } return state; }))(__arg1)))")]
	public extern static System.AggregateException? _3ffef6d50b7844eb(System.Threading.Tasks.Task instance);

	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Status.get", "((s => s.status === \"fulfilled\" ? 5 : (s.status === \"created\" ? 0 : (s.status === \"pending\" ? 3 : ((s.error?.message === \"TaskCanceledException\" || s.error?.name === \"TaskCanceledException\") ? 6 : 7))))((globalThis.__jazorTaskEnsureState ??= (task => { const states = globalThis.__jazorTaskStates ??= new WeakMap(); let state = states.get(task); if (!state) { const starterEntry = globalThis.__jazorTaskStarters?.get(task); state = { id: (globalThis.__jazorTaskNextId = (globalThis.__jazorTaskNextId ?? 0) + 1), status: (starterEntry && !starterEntry.started) ? \"created\" : \"pending\", error: null, asyncState: null }; states.set(task, state); Promise.resolve(task).then(() => { state.status = \"fulfilled\"; state.error = null; }, (error) => { state.status = \"rejected\"; state.error = error; }); } return state; }))(__arg1)))")]
	public extern static System.Threading.Tasks.TaskStatus _56ab2a84bfd1008c(System.Threading.Tasks.Task instance);

	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.IsCanceled.get", "((s => s.status === \"rejected\" && (s.error?.message === \"TaskCanceledException\" || s.error?.name === \"TaskCanceledException\"))((globalThis.__jazorTaskEnsureState ??= (task => { const states = globalThis.__jazorTaskStates ??= new WeakMap(); let state = states.get(task); if (!state) { const starterEntry = globalThis.__jazorTaskStarters?.get(task); state = { id: (globalThis.__jazorTaskNextId = (globalThis.__jazorTaskNextId ?? 0) + 1), status: (starterEntry && !starterEntry.started) ? \"created\" : \"pending\", error: null, asyncState: null }; states.set(task, state); Promise.resolve(task).then(() => { state.status = \"fulfilled\"; state.error = null; }, (error) => { state.status = \"rejected\"; state.error = error; }); } return state; }))(__arg1)))")]
	public extern static bool _674d95dbc0c2bec9(System.Threading.Tasks.Task instance);

	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.IsCompleted.get", "((s => s.status === \"fulfilled\" || s.status === \"rejected\")((globalThis.__jazorTaskEnsureState ??= (task => { const states = globalThis.__jazorTaskStates ??= new WeakMap(); let state = states.get(task); if (!state) { const starterEntry = globalThis.__jazorTaskStarters?.get(task); state = { id: (globalThis.__jazorTaskNextId = (globalThis.__jazorTaskNextId ?? 0) + 1), status: (starterEntry && !starterEntry.started) ? \"created\" : \"pending\", error: null, asyncState: null }; states.set(task, state); Promise.resolve(task).then(() => { state.status = \"fulfilled\"; state.error = null; }, (error) => { state.status = \"rejected\"; state.error = error; }); } return state; }))(__arg1)))")]
	public extern static bool _753caf2a29c3dd56(System.Threading.Tasks.Task instance);

	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.IsCompletedSuccessfully.get", "((globalThis.__jazorTaskEnsureState ??= (task => { const states = globalThis.__jazorTaskStates ??= new WeakMap(); let state = states.get(task); if (!state) { const starterEntry = globalThis.__jazorTaskStarters?.get(task); state = { id: (globalThis.__jazorTaskNextId = (globalThis.__jazorTaskNextId ?? 0) + 1), status: (starterEntry && !starterEntry.started) ? \"created\" : \"pending\", error: null, asyncState: null }; states.set(task, state); Promise.resolve(task).then(() => { state.status = \"fulfilled\"; state.error = null; }, (error) => { state.status = \"rejected\"; state.error = error; }); } return state; }))(__arg1).status === \"fulfilled\")")]
	public extern static bool _5f5f52d8162e3c67(System.Threading.Tasks.Task instance);

	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.CreationOptions.get", "0")]
	public extern static System.Threading.Tasks.TaskCreationOptions _84c3a581e703f638(System.Threading.Tasks.Task instance);

	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.AsyncState.get", "((globalThis.__jazorTaskAsyncStates?.get(__arg1)) ?? (globalThis.__jazorTaskStates?.get(__arg1)?.asyncState) ?? null)")]
	public extern static object? _929848e3cc78ca86(System.Threading.Tasks.Task instance);

	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.Factory.get", "null")]
	public extern static System.Threading.Tasks.TaskFactory _424d6d3b6efd4c35();

	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.CompletedTask.get", "Promise.resolve()")]
	public extern static System.Threading.Tasks.Task _d46fb3cd9d40f3df();

	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.IsFaulted.get", "((s => s.status === \"rejected\" && !(s.error?.message === \"TaskCanceledException\" || s.error?.name === \"TaskCanceledException\"))((globalThis.__jazorTaskEnsureState ??= (task => { const states = globalThis.__jazorTaskStates ??= new WeakMap(); let state = states.get(task); if (!state) { const starterEntry = globalThis.__jazorTaskStarters?.get(task); state = { id: (globalThis.__jazorTaskNextId = (globalThis.__jazorTaskNextId ?? 0) + 1), status: (starterEntry && !starterEntry.started) ? \"created\" : \"pending\", error: null, asyncState: null }; states.set(task, state); Promise.resolve(task).then(() => { state.status = \"fulfilled\"; state.error = null; }, (error) => { state.status = \"rejected\"; state.error = error; }); } return state; }))(__arg1)))")]
	public extern static bool _11b6c79f7ac7b231(System.Threading.Tasks.Task instance);

	///<summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.Tasks.Task" /> class.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Dispose()", "undefined")]
	public extern static void _f256cd4ac83f870c(System.Threading.Tasks.Task instance);

	///<summary>Gets an awaiter used to await this <see cref="T:System.Threading.Tasks.Task" />.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.GetAwaiter()", "Promise.resolve(__arg1)")]
	public extern static System.Runtime.CompilerServices.TaskAwaiter _552e4961aa6b5315(System.Threading.Tasks.Task instance);

	///<summary>Configures an awaiter used to await this <see cref="T:System.Threading.Tasks.Task" />.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ConfigureAwait(bool)", "Promise.resolve(__arg1)")]
	public extern static System.Runtime.CompilerServices.ConfiguredTaskAwaitable _9fd66975446401cf(System.Threading.Tasks.Task instance, bool continueOnCapturedContext);

	///<summary>Configures an awaiter used to await this <see cref="T:System.Threading.Tasks.Task" />.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ConfigureAwait(System.Threading.Tasks.ConfigureAwaitOptions)", "Promise.resolve(__arg1)")]
	public extern static System.Runtime.CompilerServices.ConfiguredTaskAwaitable _e9268008488e3309(System.Threading.Tasks.Task instance, object options);

	///<summary>Creates an awaitable task that asynchronously yields back to the current context when awaited.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.Yield()", "Promise.resolve()")]
	public extern static System.Runtime.CompilerServices.YieldAwaitable _f4e403764ad42836();

	///<summary>Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Wait()", "Promise.resolve(__arg1)")]
	public extern static void _1594f07e6f31cc00(System.Threading.Tasks.Task instance);

	///<summary>Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution within a specified time interval.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Wait(System.TimeSpan)", TaskInlineTemplates.WaitTimeSpan)]
	public extern static bool _591f7e80884826c4(System.Threading.Tasks.Task instance, RuntimeModule.JTimeSpan timeout);

	///<summary>Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Wait(System.TimeSpan, System.Threading.CancellationToken)", TaskInlineTemplates.WaitTimeSpanCancellation)]
	public extern static bool _f5ac6969a7868bed(System.Threading.Tasks.Task instance, RuntimeModule.JTimeSpan timeout, AbortSignal cancellationToken);

	///<summary>Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution. The wait terminates if a cancellation token is canceled before the task completes.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Wait(System.Threading.CancellationToken)", TaskInlineTemplates.WaitCancellation)]
	public extern static void _0ae24698cd349db7(System.Threading.Tasks.Task instance, AbortSignal cancellationToken);

	///<summary>Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution within a specified number of milliseconds.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Wait(int)", TaskInlineTemplates.WaitMilliseconds)]
	public extern static bool _31c9338e14c100f0(System.Threading.Tasks.Task instance, Number millisecondsTimeout);

	///<summary>Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution. The wait terminates if a timeout interval elapses or a cancellation token is canceled before the task completes.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.Wait(int, System.Threading.CancellationToken)", TaskInlineTemplates.WaitMillisecondsCancellation)]
	public extern static bool _3abcae6b9f17598c(System.Threading.Tasks.Task instance, Number millisecondsTimeout, AbortSignal cancellationToken);

	///<summary>Gets a <see cref="T:System.Threading.Tasks.Task" /> that will complete when this <see cref="T:System.Threading.Tasks.Task" /> completes or when the specified <see cref="P:System.Threading.CancellationToken" /> has cancellation requested.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.WaitAsync(System.Threading.CancellationToken)", TaskInlineTemplates.WaitAsyncCancellation)]
	public extern static System.Threading.Tasks.Task _ad9afc914886a128(System.Threading.Tasks.Task instance, AbortSignal cancellationToken);

	///<summary>Gets a <see cref="T:System.Threading.Tasks.Task" /> that will complete when this <see cref="T:System.Threading.Tasks.Task" /> completes or when the specified timeout expires.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.WaitAsync(System.TimeSpan)", TaskInlineTemplates.WaitAsyncTimeSpan)]
	public extern static System.Threading.Tasks.Task _f579ca933233a01c(System.Threading.Tasks.Task instance, RuntimeModule.JTimeSpan timeout);

	///<summary>Gets a <see cref="T:System.Threading.Tasks.Task" /> that will complete when this <see cref="T:System.Threading.Tasks.Task" /> completes or when the specified timeout expires.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.WaitAsync(System.TimeSpan, System.TimeProvider)", TaskInlineTemplates.WaitAsyncTimeSpan)]
	public extern static System.Threading.Tasks.Task _263b4b628e4d1a20(System.Threading.Tasks.Task instance, RuntimeModule.JTimeSpan timeout, object timeProvider);

	///<summary>Gets a <see cref="T:System.Threading.Tasks.Task" /> that will complete when this <see cref="T:System.Threading.Tasks.Task" /> completes, when the specified timeout expires, or when the specified <see cref="P:System.Threading.CancellationToken" /> has cancellation requested.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.WaitAsync(System.TimeSpan, System.Threading.CancellationToken)", TaskInlineTemplates.WaitAsyncTimeSpanCancellation)]
	public extern static System.Threading.Tasks.Task _d36be122fd9a52dd(System.Threading.Tasks.Task instance, RuntimeModule.JTimeSpan timeout, AbortSignal cancellationToken);

	///<summary>Gets a <see cref="T:System.Threading.Tasks.Task" /> that will complete when this <see cref="T:System.Threading.Tasks.Task" /> completes, when the specified timeout expires, or when the specified <see cref="T:System.Threading.CancellationToken" /> has cancellation requested.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.WaitAsync(System.TimeSpan, System.TimeProvider, System.Threading.CancellationToken)", TaskInlineTemplates.WaitAsyncTimeSpanProviderCancellation)]
	public extern static System.Threading.Tasks.Task _c5cedb48e708d62d(System.Threading.Tasks.Task instance, RuntimeModule.JTimeSpan timeout, object timeProvider, AbortSignal cancellationToken);

	///<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task>)", "Promise.resolve(__arg1).then(() => __arg2(__arg1), () => __arg2(__arg1))")]
	public extern static System.Threading.Tasks.Task _42870c69dd0eb9d8(System.Threading.Tasks.Task instance, object continuationAction);

	///<summary>Creates a continuation that receives a cancellation token and executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task>, System.Threading.CancellationToken)", TaskInlineTemplates.ContinueWithCancellation)]
	public extern static System.Threading.Tasks.Task _f6aaa640c4977029(System.Threading.Tasks.Task instance, object continuationAction, AbortSignal cancellationToken);

	///<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes. The continuation uses a specified scheduler.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task>, System.Threading.Tasks.TaskScheduler)", "Promise.resolve(__arg1).then(() => __arg2(__arg1), () => __arg2(__arg1))")]
	public extern static System.Threading.Tasks.Task _31fe4c9b6470785b(System.Threading.Tasks.Task instance, object continuationAction, object scheduler);

	///<summary>Creates a continuation that executes when the target task completes according to the specified <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task>, System.Threading.Tasks.TaskContinuationOptions)", "Promise.resolve(__arg1).then(() => __arg2(__arg1), () => __arg2(__arg1))")]
	public extern static System.Threading.Tasks.Task _e479b4b2988a20a4(System.Threading.Tasks.Task instance, object continuationAction, object continuationOptions);

	///<summary>Creates a continuation that executes when the target task competes according to the specified <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />. The continuation receives a cancellation token and uses a specified scheduler.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task>, System.Threading.CancellationToken, System.Threading.Tasks.TaskContinuationOptions, System.Threading.Tasks.TaskScheduler)", TaskInlineTemplates.ContinueWithCancellation)]
	public extern static System.Threading.Tasks.Task _6798878bd9396e39(System.Threading.Tasks.Task instance, object continuationAction, AbortSignal cancellationToken, object continuationOptions, object scheduler);

	///<summary>Creates a continuation that receives caller-supplied state information and executes when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task, object>, object)", "Promise.resolve(__arg1).then(() => __arg2(__arg1, __arg3), () => __arg2(__arg1, __arg3))")]
	public extern static System.Threading.Tasks.Task _c0b1f1737fb5274e(System.Threading.Tasks.Task instance, object continuationAction, object? state);

	///<summary>Creates a continuation that receives caller-supplied state information and a cancellation token and that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task, object>, object, System.Threading.CancellationToken)", TaskInlineTemplates.ContinueWithStateCancellation)]
	public extern static System.Threading.Tasks.Task _a1c3856bf9ec7f94(System.Threading.Tasks.Task instance, object continuationAction, object? state, AbortSignal cancellationToken);

	///<summary>Creates a continuation that receives caller-supplied state information and executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes. The continuation uses a specified scheduler.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task, object>, object, System.Threading.Tasks.TaskScheduler)", "Promise.resolve(__arg1).then(() => __arg2(__arg1, __arg3), () => __arg2(__arg1, __arg3))")]
	public extern static System.Threading.Tasks.Task _c98db2d4923664cc(System.Threading.Tasks.Task instance, object continuationAction, object? state, object scheduler);

	///<summary>Creates a continuation that receives caller-supplied state information and executes when the target <see cref="T:System.Threading.Tasks.Task" /> completes. The continuation executes based on a set of specified conditions.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task, object>, object, System.Threading.Tasks.TaskContinuationOptions)", "Promise.resolve(__arg1).then(() => __arg2(__arg1, __arg3), () => __arg2(__arg1, __arg3))")]
	public extern static System.Threading.Tasks.Task _6276124cb311c12a(System.Threading.Tasks.Task instance, object continuationAction, object? state, object continuationOptions);

	///<summary>Creates a continuation that receives caller-supplied state information and a cancellation token and that executes when the target <see cref="T:System.Threading.Tasks.Task" /> completes. The continuation executes based on a set of specified conditions and uses a specified scheduler.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task, object>, object, System.Threading.CancellationToken, System.Threading.Tasks.TaskContinuationOptions, System.Threading.Tasks.TaskScheduler)", TaskInlineTemplates.ContinueWithStateCancellation)]
	public extern static System.Threading.Tasks.Task _bf9404373dee65a3(System.Threading.Tasks.Task instance, object continuationAction, object? state, AbortSignal cancellationToken, object continuationOptions, object scheduler);

	///<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes and returns a value.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, TResult>)", "Promise.resolve(__arg1).then(() => __arg2(__arg1), () => __arg2(__arg1))")]
	public extern static System.Threading.Tasks.Task<TResult> _7d7b67122a4ac6c2<TResult>(System.Threading.Tasks.Task instance, object continuationFunction);

	///<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes and returns a value. The continuation receives a cancellation token.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, TResult>, System.Threading.CancellationToken)", TaskInlineTemplates.ContinueWithCancellation)]
	public extern static System.Threading.Tasks.Task<TResult> _27c27506d65c32ef<TResult>(System.Threading.Tasks.Task instance, object continuationFunction, AbortSignal cancellationToken);

	///<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes and returns a value. The continuation uses a specified scheduler.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, TResult>, System.Threading.Tasks.TaskScheduler)", "Promise.resolve(__arg1).then(() => __arg2(__arg1), () => __arg2(__arg1))")]
	public extern static System.Threading.Tasks.Task<TResult> _27b8beeb6791105d<TResult>(System.Threading.Tasks.Task instance, object continuationFunction, object scheduler);

	///<summary>Creates a continuation that executes according to the specified continuation options and returns a value.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, TResult>, System.Threading.Tasks.TaskContinuationOptions)", "Promise.resolve(__arg1).then(() => __arg2(__arg1), () => __arg2(__arg1))")]
	public extern static System.Threading.Tasks.Task<TResult> _ca92ad467c5ad377<TResult>(System.Threading.Tasks.Task instance, object continuationFunction, object continuationOptions);

	///<summary>Creates a continuation that executes according to the specified continuation options and returns a value. The continuation is passed a cancellation token and uses a specified scheduler.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, TResult>, System.Threading.CancellationToken, System.Threading.Tasks.TaskContinuationOptions, System.Threading.Tasks.TaskScheduler)", TaskInlineTemplates.ContinueWithCancellation)]
	public extern static System.Threading.Tasks.Task<TResult> _a91194cd6fe4a804<TResult>(System.Threading.Tasks.Task instance, object continuationFunction, AbortSignal cancellationToken, object continuationOptions, object scheduler);

	///<summary>Creates a continuation that receives caller-supplied state information and executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes and returns a value.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, object, TResult>, object)", "Promise.resolve(__arg1).then(() => __arg2(__arg1, __arg3), () => __arg2(__arg1, __arg3))")]
	public extern static System.Threading.Tasks.Task<TResult> _c90ac65203d1352e<TResult>(System.Threading.Tasks.Task instance, object continuationFunction, object? state);

	///<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes and returns a value. The continuation receives caller-supplied state information and a cancellation token.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, object, TResult>, object, System.Threading.CancellationToken)", TaskInlineTemplates.ContinueWithStateCancellation)]
	public extern static System.Threading.Tasks.Task<TResult> _68bee76bd94d95ee<TResult>(System.Threading.Tasks.Task instance, object continuationFunction, object? state, AbortSignal cancellationToken);

	///<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes. The continuation receives caller-supplied state information and uses a specified scheduler.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, object, TResult>, object, System.Threading.Tasks.TaskScheduler)", "Promise.resolve(__arg1).then(() => __arg2(__arg1, __arg3), () => __arg2(__arg1, __arg3))")]
	public extern static System.Threading.Tasks.Task<TResult> _a7f062d93de2ed93<TResult>(System.Threading.Tasks.Task instance, object continuationFunction, object? state, object scheduler);

	///<summary>Creates a continuation that executes based on the specified task continuation options when the target <see cref="T:System.Threading.Tasks.Task" /> completes. The continuation receives caller-supplied state information.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, object, TResult>, object, System.Threading.Tasks.TaskContinuationOptions)", "Promise.resolve(__arg1).then(() => __arg2(__arg1, __arg3), () => __arg2(__arg1, __arg3))")]
	public extern static System.Threading.Tasks.Task<TResult> _81acb4f27ed5b790<TResult>(System.Threading.Tasks.Task instance, object continuationFunction, object? state, object continuationOptions);

	///<summary>Creates a continuation that executes based on the specified task continuation options when the target <see cref="T:System.Threading.Tasks.Task" /> completes and returns a value. The continuation receives caller-supplied state information and a cancellation token and uses the specified scheduler.</summary>
	[Jazor(Op.Inline ,"System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, object, TResult>, object, System.Threading.CancellationToken, System.Threading.Tasks.TaskContinuationOptions, System.Threading.Tasks.TaskScheduler)", TaskInlineTemplates.ContinueWithStateCancellation)]
	public extern static System.Threading.Tasks.Task<TResult> _e31e78776c233392<TResult>(System.Threading.Tasks.Task instance, object continuationFunction, object? state, AbortSignal cancellationToken, object continuationOptions, object scheduler);

	///<summary>Waits for all of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WaitAll(params System.Threading.Tasks.Task[])", "Promise.all(__arg1)")]
	public extern static void _41e1c022a07a165c( object tasks);

	///<summary>Waits for all of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WaitAll(params System.ReadOnlySpan<System.Threading.Tasks.Task>)", "Promise.all(__arg1)")]
	public extern static void _950ed2cc45523925( object tasks);

	///<summary>Waits for all of the provided cancellable <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified time interval.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WaitAll(System.Threading.Tasks.Task[], System.TimeSpan)", TaskInlineTemplates.WaitAllTimeSpan)]
	public extern static bool _f8fce6748b855ce2(object tasks, RuntimeModule.JTimeSpan timeout);

	///<summary>Waits for all of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified number of milliseconds.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WaitAll(System.Threading.Tasks.Task[], int)", TaskInlineTemplates.WaitAllMilliseconds)]
	public extern static bool _daa1f706f69a1f60(object tasks, Number millisecondsTimeout);

	///<summary>Waits for all of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution unless the wait is cancelled.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WaitAll(System.Threading.Tasks.Task[], System.Threading.CancellationToken)", TaskInlineTemplates.WaitAllCancellation)]
	public extern static void _8f55779be329115b(object tasks, AbortSignal cancellationToken);

	///<summary>Waits for all of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified number of milliseconds or until the wait is cancelled.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WaitAll(System.Threading.Tasks.Task[], int, System.Threading.CancellationToken)", TaskInlineTemplates.WaitAllMillisecondsCancellation)]
	public extern static bool _d7522c9a3480bafa(object tasks, Number millisecondsTimeout, AbortSignal cancellationToken);

	///<summary>Waits for all of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution unless the wait is cancelled.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WaitAll(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task>, System.Threading.CancellationToken)", TaskInlineTemplates.WaitAllEnumerableCancellation)]
	public extern static void _6bcdad547747a518(object tasks, AbortSignal cancellationToken);

	///<summary>Waits for any of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WaitAny(params System.Threading.Tasks.Task[])", "Promise.race(Array.from(__arg1).map((task, index) => Promise.resolve(task).then(() => index, () => index)))")]
	public extern static Number _a7f38153597cbfe4( object tasks);

	///<summary>Waits for any of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified time interval.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WaitAny(System.Threading.Tasks.Task[], System.TimeSpan)", TaskInlineTemplates.WaitAnyTimeSpan)]
	public extern static Number _4aa06494e0b5a7e1(object tasks, RuntimeModule.JTimeSpan timeout);

	///<summary>Waits for any of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution unless the wait is cancelled.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WaitAny(System.Threading.Tasks.Task[], System.Threading.CancellationToken)", TaskInlineTemplates.WaitAnyCancellation)]
	public extern static Number _d6006967fd3ff1ae(object tasks, AbortSignal cancellationToken);

	///<summary>Waits for any of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified number of milliseconds.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WaitAny(System.Threading.Tasks.Task[], int)", TaskInlineTemplates.WaitAnyMilliseconds)]
	public extern static Number _2291d9e80a279f88(object tasks, Number millisecondsTimeout);

	///<summary>Waits for any of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified number of milliseconds or until a cancellation token is cancelled.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WaitAny(System.Threading.Tasks.Task[], int, System.Threading.CancellationToken)", TaskInlineTemplates.WaitAnyMillisecondsCancellation)]
	public extern static Number _a2afaebb710c2e05(object tasks, Number millisecondsTimeout, AbortSignal cancellationToken);

	///<summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that's completed successfully with the specified result.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.FromResult<TResult>(TResult)", "Promise.resolve(__arg1)")]
	public extern static System.Threading.Tasks.Task<TResult> _76486886fd6b2143<TResult>(object result);

	///<summary>Creates a <see cref="T:System.Threading.Tasks.Task" /> that has completed with a specified exception.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.FromException(System.Exception)", "Promise.reject(__arg1)")]
	public extern static System.Threading.Tasks.Task _681f263276bb77fd(object exception);

	///<summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that's completed with a specified exception.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.FromException<TResult>(System.Exception)", "Promise.reject(__arg1)")]
	public extern static System.Threading.Tasks.Task<TResult> _f14ed013f26abbfe<TResult>(object exception);

	///<summary>Creates a <see cref="T:System.Threading.Tasks.Task" /> that's completed due to cancellation with a specified cancellation token.</summary>
	// token 在这里被有意忽略：结果 Task 一开始就是 Canceled，取消已经发生完了，signal 不再驱动任何状态转换。
	// CLR 下 token 唯一可观察的去处是 OperationCanceledException.CancellationToken，那个面没有映射；
	// "token 必须已取消"是调用方保证的前置条件，不在运行时复查。
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.FromCanceled(System.Threading.CancellationToken)", "Promise.reject(new Error(\"TaskCanceledException\"))")]
	public extern static System.Threading.Tasks.Task _2a2b8d828dc4e32b(AbortSignal cancellationToken);

	///<summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that's completed due to cancellation with a specified cancellation token.</summary>
	// 同上：已取消的结果不需要再监听 signal。
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.FromCanceled<TResult>(System.Threading.CancellationToken)", "Promise.reject(new Error(\"TaskCanceledException\"))")]
	public extern static System.Threading.Tasks.Task<TResult> _84bf39167a494585<TResult>(AbortSignal cancellationToken);

	///<summary>Queues the specified work to run on the thread pool and returns a <see cref="T:System.Threading.Tasks.Task" /> object that represents that work.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.Run(System.Action)", "Promise.resolve().then(__arg1)")]
	public extern static System.Threading.Tasks.Task _da51a19b5762a1f4(object action);

	///<summary>Queues the specified work to run on the thread pool and returns a <see cref="T:System.Threading.Tasks.Task" /> object that represents that work. A cancellation token allows the work to be cancelled if it has not yet started.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.Run(System.Action, System.Threading.CancellationToken)", TaskInlineTemplates.RunCancellation)]
	public extern static System.Threading.Tasks.Task _a3df9536862f3937(object action, AbortSignal cancellationToken);

	///<summary>Queues the specified work to run on the thread pool and returns a <see cref="T:System.Threading.Tasks.Task`1" /> object that represents that work. A cancellation token allows the work to be cancelled if it has not yet started.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.Run<TResult>(System.Func<TResult>)", "Promise.resolve().then(__arg1)")]
	public extern static System.Threading.Tasks.Task<TResult> _d928ffeaf8804ba2<TResult>(object function);

	///<summary>Queues the specified work to run on the thread pool and returns a <see langword="Task(TResult)" /> object that represents that work.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.Run<TResult>(System.Func<TResult>, System.Threading.CancellationToken)", TaskInlineTemplates.RunCancellation)]
	public extern static System.Threading.Tasks.Task<TResult> _38b8d80dd098c8e1<TResult>(object function, AbortSignal cancellationToken);

	///<summary>Queues the specified work to run on the thread pool and returns a proxy for the task returned by <paramref name="function" />.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.Run(System.Func<System.Threading.Tasks.Task>)", "Promise.resolve().then(__arg1)")]
	public extern static System.Threading.Tasks.Task _62a7e2b729db2d93(object function);

	///<summary>Queues the specified work to run on the thread pool and returns a proxy for the task returned by <paramref name="function" />. A cancellation token allows the work to be cancelled if it has not yet started.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.Run(System.Func<System.Threading.Tasks.Task>, System.Threading.CancellationToken)", TaskInlineTemplates.RunCancellation)]
	public extern static System.Threading.Tasks.Task _cdbfa5101a0dad37(object function, AbortSignal cancellationToken);

	///<summary>Queues the specified work to run on the thread pool and returns a proxy for the <see langword="Task(TResult)" /> returned by <paramref name="function" />. A cancellation token allows the work to be cancelled if it has not yet started.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.Run<TResult>(System.Func<System.Threading.Tasks.Task<TResult>>)", "Promise.resolve().then(__arg1)")]
	public extern static System.Threading.Tasks.Task<TResult> _452c2b887d5a1fc3<TResult>(object function);

	///<summary>Queues the specified work to run on the thread pool and returns a proxy for the <see langword="Task(TResult)" /> returned by <paramref name="function" />.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.Run<TResult>(System.Func<System.Threading.Tasks.Task<TResult>>, System.Threading.CancellationToken)", TaskInlineTemplates.RunCancellation)]
	public extern static System.Threading.Tasks.Task<TResult> _da50521c9500efbd<TResult>(object function, AbortSignal cancellationToken);

	///<summary>Creates a task that completes after a specified time interval.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.Delay(System.TimeSpan)", TaskInlineTemplates.DelayTimeSpan)]
	public extern static System.Threading.Tasks.Task _ff4ca8df194f90bf(RuntimeModule.JTimeSpan delay);

	///<summary>Creates a task that completes after a specified time interval.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.Delay(System.TimeSpan, System.TimeProvider)", TaskInlineTemplates.DelayTimeSpan)]
	public extern static System.Threading.Tasks.Task _c515b64b763bdb72(RuntimeModule.JTimeSpan delay, object timeProvider);

	///<summary>Creates a cancellable task that completes after a specified time interval.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.Delay(System.TimeSpan, System.Threading.CancellationToken)", TaskInlineTemplates.DelayTimeSpanCancellation)]
	public extern static System.Threading.Tasks.Task _1dd519d143fccf61(RuntimeModule.JTimeSpan delay, AbortSignal cancellationToken);

	///<summary>Creates a cancellable task that completes after a specified time interval.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.Delay(System.TimeSpan, System.TimeProvider, System.Threading.CancellationToken)", TaskInlineTemplates.DelayTimeSpanProviderCancellation)]
	public extern static System.Threading.Tasks.Task _c16542532f5bf55f(RuntimeModule.JTimeSpan delay, object timeProvider, AbortSignal cancellationToken);

	///<summary>Creates a task that completes after a specified number of milliseconds.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.Delay(int)", TaskInlineTemplates.DelayMilliseconds)]
	public extern static System.Threading.Tasks.Task _3da1cdb174644ada(Number millisecondsDelay);

	///<summary>Creates a cancellable task that completes after a specified number of milliseconds.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.Delay(int, System.Threading.CancellationToken)", TaskInlineTemplates.DelayMillisecondsCancellation)]
	public extern static System.Threading.Tasks.Task _34c332c06d4d985b(Number millisecondsDelay, AbortSignal cancellationToken);

	///<summary>Creates a task that will complete when all of the <see cref="T:System.Threading.Tasks.Task" /> objects in an enumerable collection have completed.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenAll(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task>)", "Promise.all(__arg1)")]
	public extern static System.Threading.Tasks.Task _cb0c072793c59334(object tasks);

	///<summary>Creates a task that will complete when all of the <see cref="T:System.Threading.Tasks.Task" /> objects in an array have completed.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenAll(params System.Threading.Tasks.Task[])", "Promise.all(__arg1)")]
	public extern static System.Threading.Tasks.Task _5bdce56e38e4b97c( object tasks);

	///<summary>Creates a task that will complete when all of the supplied tasks have completed.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenAll(params System.ReadOnlySpan<System.Threading.Tasks.Task>)", "Promise.all(__arg1)")]
	public extern static System.Threading.Tasks.Task _d62721be70a65388( object tasks);

	///<summary>Creates a task that will complete when all of the <see cref="T:System.Threading.Tasks.Task`1" /> objects in an enumerable collection have completed.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenAll<TResult>(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<TResult>>)", "Promise.all(__arg1)")]
	public extern static System.Threading.Tasks.Task<TResult[]> _cfb648f6d9ec34c8<TResult>(object tasks);

	///<summary>Creates a task that will complete when all of the <see cref="T:System.Threading.Tasks.Task`1" /> objects in an array have completed.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenAll<TResult>(params System.Threading.Tasks.Task<TResult>[])", "Promise.all(__arg1)")]
	public extern static System.Threading.Tasks.Task<TResult[]> _a54b67fbb4ccb6bc<TResult>( object tasks);

	///<summary>Creates a task that will complete when all of the supplied tasks have completed.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenAll<TResult>(params System.ReadOnlySpan<System.Threading.Tasks.Task<TResult>>)", "Promise.all(__arg1)")]
	public extern static System.Threading.Tasks.Task<TResult[]> _d8cf2ec1f7803bff<TResult>( object tasks);

	///<summary>Creates a task that will complete when any of the supplied tasks have completed.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenAny(params System.Threading.Tasks.Task[])", "Promise.race(Array.from(__arg1).map((task) => Promise.resolve(task).then(() => task, () => task)))")]
	public extern static System.Threading.Tasks.Task<System.Threading.Tasks.Task> _ddf19fd1d97f0cd2( object tasks);

	///<summary>Creates a task that will complete when any of the supplied tasks have completed.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenAny(params System.ReadOnlySpan<System.Threading.Tasks.Task>)", "Promise.race(Array.from(__arg1).map((task) => Promise.resolve(task).then(() => task, () => task)))")]
	public extern static System.Threading.Tasks.Task<System.Threading.Tasks.Task> _e7c954aa77999183( object tasks);

	///<summary>Creates a task that will complete when either of the supplied tasks have completed.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenAny(System.Threading.Tasks.Task, System.Threading.Tasks.Task)", "Promise.race([__arg1, __arg2].map((task) => Promise.resolve(task).then(() => task, () => task)))")]
	public extern static System.Threading.Tasks.Task<System.Threading.Tasks.Task> _cc30f99c4d488ed9(object task1, object task2);

	///<summary>Creates a task that will complete when any of the supplied tasks have completed.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenAny(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task>)", "Promise.race(Array.from(__arg1).map((task) => Promise.resolve(task).then(() => task, () => task)))")]
	public extern static System.Threading.Tasks.Task<System.Threading.Tasks.Task> _717dc2ba16f86618(object tasks);

	///<summary>Creates a task that will complete when any of the supplied tasks have completed.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenAny<TResult>(params System.Threading.Tasks.Task<TResult>[])", "Promise.race(Array.from(__arg1).map((task) => Promise.resolve(task).then(() => task, () => task)))")]
	public extern static System.Threading.Tasks.Task<System.Threading.Tasks.Task<TResult>> _e1fbf4daaee01944<TResult>( object tasks);

	///<summary>Creates a task that will complete when any of the supplied tasks have completed.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenAny<TResult>(params System.ReadOnlySpan<System.Threading.Tasks.Task<TResult>>)", "Promise.race(Array.from(__arg1).map((task) => Promise.resolve(task).then(() => task, () => task)))")]
	public extern static System.Threading.Tasks.Task<System.Threading.Tasks.Task<TResult>> _8106e2961a122fe0<TResult>( object tasks);

	///<summary>Creates a task that will complete when either of the supplied tasks have completed.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenAny<TResult>(System.Threading.Tasks.Task<TResult>, System.Threading.Tasks.Task<TResult>)", "Promise.race([__arg1, __arg2].map((task) => Promise.resolve(task).then(() => task, () => task)))")]
	public extern static System.Threading.Tasks.Task<System.Threading.Tasks.Task<TResult>> _592d4633f4f24c38<TResult>(object task1, object task2);

	///<summary>Creates a task that will complete when any of the supplied tasks have completed.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenAny<TResult>(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<TResult>>)", "Promise.race(Array.from(__arg1).map((task) => Promise.resolve(task).then(() => task, () => task)))")]
	public extern static System.Threading.Tasks.Task<System.Threading.Tasks.Task<TResult>> _cf1b91bc49523a2b<TResult>(object tasks);

	///<summary>Creates an <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" /> that will yield the supplied tasks as those tasks complete.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenEach(params System.Threading.Tasks.Task[])", "(async function*(){ const pending = Array.from(__arg1); while (pending.length) { const settled = await Promise.race(pending.map((task, index) => Promise.resolve(task).then(() => ({ task, index }), () => ({ task, index })))); yield settled.task; pending.splice(settled.index, 1); } })()")]
	public extern static System.Collections.Generic.IAsyncEnumerable<System.Threading.Tasks.Task> _2ad9e7d43f12d14d( object tasks);

	///<summary>Creates an <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.IAsyncEnumerable`1"></xref> that will yield the supplied tasks as those tasks complete.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenEach(params System.ReadOnlySpan<System.Threading.Tasks.Task>)", "(async function*(){ const pending = Array.from(__arg1); while (pending.length) { const settled = await Promise.race(pending.map((task, index) => Promise.resolve(task).then(() => ({ task, index }), () => ({ task, index })))); yield settled.task; pending.splice(settled.index, 1); } })()")]
	public extern static System.Collections.Generic.IAsyncEnumerable<System.Threading.Tasks.Task> _2df0fee75892f471( object tasks);

	///<summary>Creates an <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.IAsyncEnumerable`1"></xref> that will yield the supplied tasks as those tasks complete.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenEach(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task>)", "(async function*(){ const pending = Array.from(__arg1); while (pending.length) { const settled = await Promise.race(pending.map((task, index) => Promise.resolve(task).then(() => ({ task, index }), () => ({ task, index })))); yield settled.task; pending.splice(settled.index, 1); } })()")]
	public extern static System.Collections.Generic.IAsyncEnumerable<System.Threading.Tasks.Task> _b06f770db773a3a0(object tasks);

	///<summary>Creates an <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.IAsyncEnumerable`1"></xref> that will yield the supplied tasks as those tasks complete.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenEach<TResult>(params System.Threading.Tasks.Task<TResult>[])", "(async function*(){ const pending = Array.from(__arg1); while (pending.length) { const settled = await Promise.race(pending.map((task, index) => Promise.resolve(task).then(() => ({ task, index }), () => ({ task, index })))); yield settled.task; pending.splice(settled.index, 1); } })()")]
	public extern static System.Collections.Generic.IAsyncEnumerable<System.Threading.Tasks.Task<TResult>> _287e334b00da970c<TResult>( object tasks);

	///<summary>Creates an <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.IAsyncEnumerable`1"></xref> that will yield the supplied tasks as those tasks complete.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenEach<TResult>(params System.ReadOnlySpan<System.Threading.Tasks.Task<TResult>>)", "(async function*(){ const pending = Array.from(__arg1); while (pending.length) { const settled = await Promise.race(pending.map((task, index) => Promise.resolve(task).then(() => ({ task, index }), () => ({ task, index })))); yield settled.task; pending.splice(settled.index, 1); } })()")]
	public extern static System.Collections.Generic.IAsyncEnumerable<System.Threading.Tasks.Task<TResult>> _4cca3bf88970e2ff<TResult>( object tasks);

	///<summary>Creates an <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.IAsyncEnumerable`1"></xref> that will yield the supplied tasks as those tasks complete.</summary>
	[Jazor(Op.Inline ,"static System.Threading.Tasks.Task.WhenEach<TResult>(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<TResult>>)", "(async function*(){ const pending = Array.from(__arg1); while (pending.length) { const settled = await Promise.race(pending.map((task, index) => Promise.resolve(task).then(() => ({ task, index }), () => ({ task, index })))); yield settled.task; pending.splice(settled.index, 1); } })()")]
	public extern static System.Collections.Generic.IAsyncEnumerable<System.Threading.Tasks.Task<TResult>> _0fb3578fab4c3d87<TResult>(object tasks);
}
