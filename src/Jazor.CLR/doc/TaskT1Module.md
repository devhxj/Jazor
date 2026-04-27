# TaskT1Module.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Threading.Tasks.Task<TResult>.Task(System.Func<TResult>)</br>
**签名**：_b67c4cf36519fc4e</br>
**注释**：

```xml
<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task`1" /> with the specified function.</summary>
<param name="function">The delegate that represents the code to execute in the task. When the function has completed, the task's <see cref="P:System.Threading.Tasks.Task`1.Result" /> property will be set to return the result value of the function.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="function" /> argument is <see langword="null" />.</exception>
```

**成员**：System.Threading.Tasks.Task<TResult>.Task(System.Func<TResult>, System.Threading.CancellationToken)</br>
**签名**：_210b47f62903ec68</br>
**注释**：

```xml
<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task`1" /> with the specified function.</summary>
<param name="function">The delegate that represents the code to execute in the task. When the function has completed, the task's <see cref="P:System.Threading.Tasks.Task`1.Result" /> property will be set to return the result value of the function.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to be assigned to this task.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="function" /> argument is <see langword="null" />.</exception>
```

**成员**：System.Threading.Tasks.Task<TResult>.Task(System.Func<TResult>, System.Threading.Tasks.TaskCreationOptions)</br>
**签名**：_ba6457c4d953ecb5</br>
**注释**：

```xml
<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task`1" /> with the specified function and creation options.</summary>
<param name="function">The delegate that represents the code to execute in the task. When the function has completed, the task's <see cref="P:System.Threading.Tasks.Task`1.Result" /> property will be set to return the result value of the function.</param>
<param name="creationOptions">The <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to customize the task's behavior.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskCreationOptions" />.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="function" /> argument is <see langword="null" />.</exception>
```

**成员**：System.Threading.Tasks.Task<TResult>.Task(System.Func<TResult>, System.Threading.CancellationToken, System.Threading.Tasks.TaskCreationOptions)</br>
**签名**：_96777ee6980a57db</br>
**注释**：

```xml
<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task`1" /> with the specified function and creation options.</summary>
<param name="function">The delegate that represents the code to execute in the task. When the function has completed, the task's <see cref="P:System.Threading.Tasks.Task`1.Result" /> property will be set to return the result value of the function.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new task.</param>
<param name="creationOptions">The <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to customize the task's behavior.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskCreationOptions" />.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="function" /> argument is <see langword="null" />.</exception>
```

**成员**：System.Threading.Tasks.Task<TResult>.Task(System.Func<object, TResult>, object)</br>
**签名**：_6207405fd22d16d1</br>
**注释**：

```xml
<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task`1" /> with the specified function and state.</summary>
<param name="function">The delegate that represents the code to execute in the task. When the function has completed, the task's <see cref="P:System.Threading.Tasks.Task`1.Result" /> property will be set to return the result value of the function.</param>
<param name="state">An object representing data to be used by the action.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="function" /> argument is <see langword="null" />.</exception>
```

**成员**：System.Threading.Tasks.Task<TResult>.Task(System.Func<object, TResult>, object, System.Threading.CancellationToken)</br>
**签名**：_1110e77a99517026</br>
**注释**：

```xml
<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task`1" /> with the specified action, state, and options.</summary>
<param name="function">The delegate that represents the code to execute in the task. When the function has completed, the task's <see cref="P:System.Threading.Tasks.Task`1.Result" /> property will be set to return the result value of the function.</param>
<param name="state">An object representing data to be used by the function.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to be assigned to the new task.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="function" /> argument is <see langword="null" />.</exception>
```

**成员**：System.Threading.Tasks.Task<TResult>.Task(System.Func<object, TResult>, object, System.Threading.Tasks.TaskCreationOptions)</br>
**签名**：_74a44809fb4972d8</br>
**注释**：

```xml
<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task`1" /> with the specified action, state, and options.</summary>
<param name="function">The delegate that represents the code to execute in the task. When the function has completed, the task's <see cref="P:System.Threading.Tasks.Task`1.Result" /> property will be set to return the result value of the function.</param>
<param name="state">An object representing data to be used by the function.</param>
<param name="creationOptions">The <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to customize the task's behavior.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskCreationOptions" />.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="function" /> argument is <see langword="null" />.</exception>
```

**成员**：System.Threading.Tasks.Task<TResult>.Task(System.Func<object, TResult>, object, System.Threading.CancellationToken, System.Threading.Tasks.TaskCreationOptions)</br>
**签名**：_d34af741ef3ff2f4</br>
**注释**：

```xml
<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task`1" /> with the specified action, state, and options.</summary>
<param name="function">The delegate that represents the code to execute in the task. When the function has completed, the task's <see cref="P:System.Threading.Tasks.Task`1.Result" /> property will be set to return the result value of the function.</param>
<param name="state">An object representing data to be used by the function.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to be assigned to the new task.</param>
<param name="creationOptions">The <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to customize the task's behavior.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskCreationOptions" />.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="function" /> argument is <see langword="null" />.</exception>
```

**成员**：System.Threading.Tasks.Task<TResult>.Result.get</br>
**签名**：_18af0aa87004bfcc</br>

**成员**：static System.Threading.Tasks.Task<TResult>.Factory.get</br>
**签名**：_4e868d2ddd664410</br>

**成员**：System.Threading.Tasks.Task<TResult>.GetAwaiter()</br>
**签名**：_027217a9621e6f7b</br>
**注释**：

```xml
<summary>Gets an awaiter used to await this <see cref="T:System.Threading.Tasks.Task`1" />.</summary>
<returns>An awaiter instance.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ConfigureAwait(bool)</br>
**签名**：_0e17ea5f64ad914f</br>
**注释**：

```xml
<summary>Configures an awaiter used to await this <see cref="T:System.Threading.Tasks.Task`1" />.</summary>
<param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false.</param>
<returns>An object used to await this task.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ConfigureAwait(System.Threading.Tasks.ConfigureAwaitOptions)</br>
**签名**：_e315c5cff004ed53</br>
**注释**：

```xml
<summary>Configures an awaiter used to await this <see cref="T:System.Threading.Tasks.Task" />.</summary>
<param name="options">Options used to configure how awaits on this task are performed.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="options" /> argument specifies an invalid value.</exception>
<returns>An object used to await this task.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.WaitAsync(System.Threading.CancellationToken)</br>
**签名**：_a5adb3e12ef3a8bb</br>
**注释**：

```xml
<summary>Gets a <see cref="T:System.Threading.Tasks.Task`1" /> that will complete when this <see cref="T:System.Threading.Tasks.Task`1" /> completes or when the specified <see cref="T:System.Threading.CancellationToken" /> has cancellation requested.</summary>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to monitor for a cancellation request.</param>
<returns>The <see cref="T:System.Threading.Tasks.Task`1" /> representing the asynchronous wait. It may or may not be the same instance as the current instance.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan)</br>
**签名**：_408c4a7eefe8214c</br>
**注释**：

```xml
<summary>Gets a <see cref="T:System.Threading.Tasks.Task`1" /> that will complete when this <see cref="T:System.Threading.Tasks.Task`1" /> completes or when the specified timeout expires.</summary>
<param name="timeout">The timeout after which the <see cref="T:System.Threading.Tasks.Task" /> should be faulted with a <see cref="T:System.TimeoutException" /> if it hasn't otherwise completed.</param>
<returns>The <see cref="T:System.Threading.Tasks.Task`1" /> representing the asynchronous wait. It may or may not be the same instance as the current instance.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan, System.TimeProvider)</br>
**签名**：_35ae1f6899303439</br>
**注释**：

```xml
<summary>Gets a <see cref="T:System.Threading.Tasks.Task`1" /> that will complete when this <see cref="T:System.Threading.Tasks.Task`1" /> completes or when the specified timeout expires.</summary>
<param name="timeout">The timeout after which the <see cref="T:System.Threading.Tasks.Task" /> should be faulted with a <see cref="T:System.TimeoutException" /> if it hasn't otherwise completed.</param>
<param name="timeProvider">The <see cref="T:System.TimeProvider" /> with which to interpret <paramref name="timeout" />.</param>
<returns>The <see cref="T:System.Threading.Tasks.Task`1" /> representing the asynchronous wait.  It may or may not be the same instance as the current instance.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan, System.Threading.CancellationToken)</br>
**签名**：_05fbcc037540ba42</br>
**注释**：

```xml
<summary>Gets a <see cref="T:System.Threading.Tasks.Task`1" /> that will complete when this <see cref="T:System.Threading.Tasks.Task`1" /> completes, when the specified timeout expires, or when the specified <see cref="T:System.Threading.CancellationToken" /> has cancellation requested.</summary>
<param name="timeout">The timeout after which the <see cref="T:System.Threading.Tasks.Task" /> should be faulted with a <see cref="T:System.TimeoutException" /> if it hasn't otherwise completed.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to monitor for a cancellation request.</param>
<returns>The <see cref="T:System.Threading.Tasks.Task`1" /> representing the asynchronous wait. It may or may not be the same instance as the current instance.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.WaitAsync(System.TimeSpan, System.TimeProvider, System.Threading.CancellationToken)</br>
**签名**：_4b5b887e2099f8dd</br>
**注释**：

```xml
<summary>Gets a <see cref="T:System.Threading.Tasks.Task`1" /> that will complete when this <see cref="T:System.Threading.Tasks.Task`1" /> completes, when the specified timeout expires, or when the specified <see cref="T:System.Threading.CancellationToken" /> has cancellation requested.</summary>
<param name="timeout">The timeout after which the <see cref="T:System.Threading.Tasks.Task" /> should be faulted with a <see cref="T:System.TimeoutException" /> if it hasn't otherwise completed.</param>
<param name="timeProvider">The <see cref="T:System.TimeProvider" /> with which to interpret <paramref name="timeout" />.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to monitor for a cancellation request.</param>
<returns>The <see cref="T:System.Threading.Tasks.Task`1" /> representing the asynchronous wait.  It may or may not be the same instance as the current instance.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith(System.Action<System.Threading.Tasks.Task<TResult>>)</br>
**签名**：_18ebd732a24d5a8a</br>
**注释**：

```xml
<summary>Creates a continuation that executes asynchronously when the target task completes.</summary>
<param name="continuationAction">An action to run when the antecedent <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is <see langword="null" />.</exception>
<returns>A new continuation task.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith(System.Action<System.Threading.Tasks.Task<TResult>>, System.Threading.CancellationToken)</br>
**签名**：_3338cbe86421b489</br>
**注释**：

```xml
<summary>Creates a cancelable continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
<param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate is passed the completed task as an argument.</param>
<param name="cancellationToken">The cancellation token that is passed to the new continuation task.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.     -or-     The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is <see langword="null" />.</exception>
<returns>A new continuation task.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith(System.Action<System.Threading.Tasks.Task<TResult>>, System.Threading.Tasks.TaskScheduler)</br>
**签名**：_c196f271595e1b79</br>
**注释**：

```xml
<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
<param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
<param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is <see langword="null" />.     -or-     The <paramref name="scheduler" /> argument is <see langword="null" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith(System.Action<System.Threading.Tasks.Task<TResult>>, System.Threading.Tasks.TaskContinuationOptions)</br>
**签名**：_6091b837cc517c80</br>
**注释**：

```xml
<summary>Creates a continuation that executes according the condition specified in <paramref name="continuationOptions" />.</summary>
<param name="continuationAction">An action to according the condition specified in <paramref name="continuationOptions" />. When run, the delegate will be passed the completed task as an argument.</param>
<param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith(System.Action<System.Threading.Tasks.Task<TResult>>, System.Threading.CancellationToken, System.Threading.Tasks.TaskContinuationOptions, System.Threading.Tasks.TaskScheduler)</br>
**签名**：_0ddbe59426be08f7</br>
**注释**：

```xml
<summary>Creates a continuation that executes according the condition specified in <paramref name="continuationOptions" />.</summary>
<param name="continuationAction">An action to run according the condition specified in <paramref name="continuationOptions" />. When run, the delegate will be passed the completed task as an argument.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
<param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
<param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.     -or-     The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is <see langword="null" />.     -or-     The <paramref name="scheduler" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith(System.Action<System.Threading.Tasks.Task<TResult>, object>, object)</br>
**签名**：_9282b08373d72037</br>
**注释**：

```xml
<summary>Creates a continuation that is passed state information and that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
<param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate is   passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation action.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is <see langword="null" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith(System.Action<System.Threading.Tasks.Task<TResult>, object>, object, System.Threading.CancellationToken)</br>
**签名**：_d2072aac313e5e7d</br>
**注释**：

```xml
<summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
<param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation action.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith(System.Action<System.Threading.Tasks.Task<TResult>, object>, object, System.Threading.Tasks.TaskScheduler)</br>
**签名**：_bc25018a37f9cf04</br>
**注释**：

```xml
<summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
<param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation action.</param>
<param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is <see langword="null" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith(System.Action<System.Threading.Tasks.Task<TResult>, object>, object, System.Threading.Tasks.TaskContinuationOptions)</br>
**签名**：_8a7225c0cacf6c33</br>
**注释**：

```xml
<summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
<param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation action.</param>
<param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such  as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith(System.Action<System.Threading.Tasks.Task<TResult>, object>, object, System.Threading.CancellationToken, System.Threading.Tasks.TaskContinuationOptions, System.Threading.Tasks.TaskScheduler)</br>
**签名**：_2dee2cd7dc8b10a0</br>
**注释**：

```xml
<summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
<param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation action.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
<param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as  well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
<param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its  execution.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
<exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith<TNewResult>(System.Func<System.Threading.Tasks.Task<TResult>, TNewResult>)</br>
**签名**：_bec3364575255238</br>
**注释**：

```xml
<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
<param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
<typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is <see langword="null" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith<TNewResult>(System.Func<System.Threading.Tasks.Task<TResult>, TNewResult>, System.Threading.CancellationToken)</br>
**签名**：_c5252913dc3700eb</br>
**注释**：

```xml
<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
<param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new task.</param>
<typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.     -or-     The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is <see langword="null" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith<TNewResult>(System.Func<System.Threading.Tasks.Task<TResult>, TNewResult>, System.Threading.Tasks.TaskScheduler)</br>
**签名**：_e3d3f3a4b57f0d15</br>
**注释**：

```xml
<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
<param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
<param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
<typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is <see langword="null" />.     -or-     The <paramref name="scheduler" /> argument is <see langword="null" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith<TNewResult>(System.Func<System.Threading.Tasks.Task<TResult>, TNewResult>, System.Threading.Tasks.TaskContinuationOptions)</br>
**签名**：_3a223df7acb7dc7f</br>
**注释**：

```xml
<summary>Creates a continuation that executes according the condition specified in <paramref name="continuationOptions" />.</summary>
<param name="continuationFunction">A function to run according the condition specified in <paramref name="continuationOptions" />.     When run, the delegate will be passed the completed task as an argument.</param>
<param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
<typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith<TNewResult>(System.Func<System.Threading.Tasks.Task<TResult>, TNewResult>, System.Threading.CancellationToken, System.Threading.Tasks.TaskContinuationOptions, System.Threading.Tasks.TaskScheduler)</br>
**签名**：_699cdb47464c405c</br>
**注释**：

```xml
<summary>Creates a continuation that executes according the condition specified in <paramref name="continuationOptions" />.</summary>
<param name="continuationFunction">A function to run according the condition specified in <paramref name="continuationOptions" />.     When run, the delegate will be passed as an argument this completed task.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new task.</param>
<param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
<param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
<typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.     -or-     The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is <see langword="null" />.     -or-     The <paramref name="scheduler" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith<TNewResult>(System.Func<System.Threading.Tasks.Task<TResult>, object, TNewResult>, object)</br>
**签名**：_1b81db3c7cbed209</br>
**注释**：

```xml
<summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
<param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation function.</param>
<typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is <see langword="null" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith<TNewResult>(System.Func<System.Threading.Tasks.Task<TResult>, object, TNewResult>, object, System.Threading.CancellationToken)</br>
**签名**：_0d204bef0e39218f</br>
**注释**：

```xml
<summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
<param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation function.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new task.</param>
<typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith<TNewResult>(System.Func<System.Threading.Tasks.Task<TResult>, object, TNewResult>, object, System.Threading.Tasks.TaskScheduler)</br>
**签名**：_fbf66107bc65d1ae</br>
**注释**：

```xml
<summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
<param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation function.</param>
<param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
<typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is <see langword="null" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith<TNewResult>(System.Func<System.Threading.Tasks.Task<TResult>, object, TNewResult>, object, System.Threading.Tasks.TaskContinuationOptions)</br>
**签名**：_f82405b5e8022d61</br>
**注释**：

```xml
<summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
<param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation function.</param>
<param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
<typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：System.Threading.Tasks.Task<TResult>.ContinueWith<TNewResult>(System.Func<System.Threading.Tasks.Task<TResult>, object, TNewResult>, object, System.Threading.CancellationToken, System.Threading.Tasks.TaskContinuationOptions, System.Threading.Tasks.TaskScheduler)</br>
**签名**：_ba5f2a18a0be5ed8</br>
**注释**：

```xml
<summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
<param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation function.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new task.</param>
<param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
<param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
<typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The  <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
<exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

