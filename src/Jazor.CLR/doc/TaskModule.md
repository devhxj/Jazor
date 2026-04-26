# TaskModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Threading.Tasks.Task.Task(System.Action)</br>
**签名**：_54056395d4c60189</br>
**注释**：

```xml
<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action.</summary>
<param name="action">The delegate that represents the code to execute in the task.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="action" /> argument is <see langword="null" />.</exception>
```

**成员**：System.Threading.Tasks.Task.Task(System.Action, System.Threading.CancellationToken)</br>
**签名**：_85cc61f0768e2467</br>
**注释**：

```xml
<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action and <see cref="T:System.Threading.CancellationToken" />.</summary>
<param name="action">The delegate that represents the code to execute in the task.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that the new  task will observe.</param>
<exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="action" /> argument is null.</exception>
```

**成员**：System.Threading.Tasks.Task.Task(System.Action, System.Threading.Tasks.TaskCreationOptions)</br>
**签名**：_eff8e21064439c38</br>
**注释**：

```xml
<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action and creation options.</summary>
<param name="action">The delegate that represents the code to execute in the task.</param>
<param name="creationOptions">The <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to customize the task's behavior.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="action" /> argument is null.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskCreationOptions" />.</exception>
```

**成员**：System.Threading.Tasks.Task.Task(System.Action, System.Threading.CancellationToken, System.Threading.Tasks.TaskCreationOptions)</br>
**签名**：_cec1128f4e8dc68a</br>
**注释**：

```xml
<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action and creation options.</summary>
<param name="action">The delegate that represents the code to execute in the task.</param>
<param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that the new task will observe.</param>
<param name="creationOptions">The <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to customize the task's behavior.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="action" /> argument is null.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskCreationOptions" />.</exception>
```

**成员**：System.Threading.Tasks.Task.Task(System.Action<object>, object)</br>
**签名**：_0be51a2dc3255844</br>
**注释**：

```xml
<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action and state.</summary>
<param name="action">The delegate that represents the code to execute in the task.</param>
<param name="state">An object representing data to be used by the action.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="action" /> argument is null.</exception>
```

**成员**：System.Threading.Tasks.Task.Task(System.Action<object>, object, System.Threading.CancellationToken)</br>
**签名**：_9fcd22dde0dcd8a7</br>
**注释**：

```xml
<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action, state, and <see cref="T:System.Threading.CancellationToken" />.</summary>
<param name="action">The delegate that represents the code to execute in the task.</param>
<param name="state">An object representing data to be used by the action.</param>
<param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that the new task will observe.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="action" /> argument is null.</exception>
```

**成员**：System.Threading.Tasks.Task.Task(System.Action<object>, object, System.Threading.Tasks.TaskCreationOptions)</br>
**签名**：_751384169b9f00a5</br>
**注释**：

```xml
<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action, state, and options.</summary>
<param name="action">The delegate that represents the code to execute in the task.</param>
<param name="state">An object representing data to be used by the action.</param>
<param name="creationOptions">The <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to customize the task's behavior.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="action" /> argument is null.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskCreationOptions" />.</exception>
```

**成员**：System.Threading.Tasks.Task.Task(System.Action<object>, object, System.Threading.CancellationToken, System.Threading.Tasks.TaskCreationOptions)</br>
**签名**：_1e1dc0b6a7d9ae5a</br>
**注释**：

```xml
<summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action, state, and options.</summary>
<param name="action">The delegate that represents the code to execute in the task.</param>
<param name="state">An object representing data to be used by the action.</param>
<param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that the new task will observe.</param>
<param name="creationOptions">The <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to customize the task's behavior.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="action" /> argument is null.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskCreationOptions" />.</exception>
```

**成员**：System.Threading.Tasks.Task.Start()</br>
**签名**：_571f6c3f73cde8c3</br>
**注释**：

```xml
<summary>Starts the <see cref="T:System.Threading.Tasks.Task" />, scheduling it for execution to the current <see cref="T:System.Threading.Tasks.TaskScheduler" />.</summary>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> instance has been disposed.</exception>
<exception cref="T:System.InvalidOperationException">The <see cref="T:System.Threading.Tasks.Task" /> is not in a valid state to be started. It may have already been started, executed, or canceled, or it may have been created in a manner that doesn't support direct scheduling.</exception>
```

**成员**：System.Threading.Tasks.Task.Start(System.Threading.Tasks.TaskScheduler)</br>
**签名**：_5393d9342c25e912</br>
**注释**：

```xml
<summary>Starts the <see cref="T:System.Threading.Tasks.Task" />, scheduling it for execution to the specified <see cref="T:System.Threading.Tasks.TaskScheduler" />.</summary>
<param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> with which to associate and execute this task.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.InvalidOperationException">The <see cref="T:System.Threading.Tasks.Task" /> is not in a valid state to be started. It may have already been started, executed, or canceled, or it may have been created in a manner that doesn't support direct scheduling.</exception>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> instance has been disposed.</exception>
<exception cref="T:System.Threading.Tasks.TaskSchedulerException">The scheduler was unable to queue this task.</exception>
```

**成员**：System.Threading.Tasks.Task.RunSynchronously()</br>
**签名**：_1f6e131527687ab7</br>
**注释**：

```xml
<summary>Runs the <see cref="T:System.Threading.Tasks.Task" /> synchronously on the current <see cref="T:System.Threading.Tasks.TaskScheduler" />.</summary>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> instance has been disposed.</exception>
<exception cref="T:System.InvalidOperationException">The <see cref="T:System.Threading.Tasks.Task" /> is not in a valid state to be started. It may have already been started, executed, or canceled, or it may have been created in a manner that doesn't support direct scheduling.</exception>
```

**成员**：System.Threading.Tasks.Task.RunSynchronously(System.Threading.Tasks.TaskScheduler)</br>
**签名**：_930596f5e09d6af6</br>
**注释**：

```xml
<summary>Runs the <see cref="T:System.Threading.Tasks.Task" /> synchronously on the <see cref="T:System.Threading.Tasks.TaskScheduler" /> provided.</summary>
<param name="scheduler">The scheduler on which to attempt to run this task inline.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> instance has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.InvalidOperationException">The <see cref="T:System.Threading.Tasks.Task" /> is not in a valid state to be started. It may have already been started, executed, or canceled, or it may have been created in a manner that doesn't support direct scheduling.</exception>
```

**成员**：System.Threading.Tasks.Task.Id.get</br>
**签名**：_631607ea76b1f24d</br>

**成员**：static System.Threading.Tasks.Task.CurrentId.get</br>
**签名**：_77f2902849fd5781</br>

**成员**：System.Threading.Tasks.Task.Exception.get</br>
**签名**：_3ffef6d50b7844eb</br>

**成员**：System.Threading.Tasks.Task.Status.get</br>
**签名**：_56ab2a84bfd1008c</br>

**成员**：System.Threading.Tasks.Task.IsCanceled.get</br>
**签名**：_674d95dbc0c2bec9</br>

**成员**：System.Threading.Tasks.Task.IsCompleted.get</br>
**签名**：_753caf2a29c3dd56</br>

**成员**：System.Threading.Tasks.Task.IsCompletedSuccessfully.get</br>
**签名**：_5f5f52d8162e3c67</br>

**成员**：System.Threading.Tasks.Task.CreationOptions.get</br>
**签名**：_84c3a581e703f638</br>

**成员**：System.Threading.Tasks.Task.AsyncState.get</br>
**签名**：_929848e3cc78ca86</br>

**成员**：static System.Threading.Tasks.Task.Factory.get</br>
**签名**：_424d6d3b6efd4c35</br>

**成员**：static System.Threading.Tasks.Task.CompletedTask.get</br>
**签名**：_d46fb3cd9d40f3df</br>

**成员**：System.Threading.Tasks.Task.IsFaulted.get</br>
**签名**：_11b6c79f7ac7b231</br>

**成员**：System.Threading.Tasks.Task.Dispose()</br>
**签名**：_f256cd4ac83f870c</br>
**注释**：

```xml
<summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.Tasks.Task" /> class.</summary>
<exception cref="T:System.InvalidOperationException">The task is not in one of the final states: <see cref="F:System.Threading.Tasks.TaskStatus.RanToCompletion" />, <see cref="F:System.Threading.Tasks.TaskStatus.Faulted" />, or <see cref="F:System.Threading.Tasks.TaskStatus.Canceled" />.</exception>
```

**成员**：System.Threading.Tasks.Task.GetAwaiter()</br>
**签名**：_552e4961aa6b5315</br>
**注释**：

```xml
<summary>Gets an awaiter used to await this <see cref="T:System.Threading.Tasks.Task" />.</summary>
<returns>An awaiter instance.</returns>
```

**成员**：System.Threading.Tasks.Task.ConfigureAwait(bool)</br>
**签名**：_9fd66975446401cf</br>
**注释**：

```xml
<summary>Configures an awaiter used to await this <see cref="T:System.Threading.Tasks.Task" />.</summary>
<param name="continueOnCapturedContext">  <see langword="true" /> to attempt to marshal the continuation back to the original context captured; otherwise, <see langword="false" />.</param>
<returns>An object used to await this task.</returns>
```

**成员**：System.Threading.Tasks.Task.ConfigureAwait(System.Threading.Tasks.ConfigureAwaitOptions)</br>
**签名**：_e9268008488e3309</br>
**注释**：

```xml
<summary>Configures an awaiter used to await this <see cref="T:System.Threading.Tasks.Task" />.</summary>
<param name="options">Options used to configure how awaits on this task are performed.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="options" /> argument specifies an invalid value.</exception>
<returns>An object used to await this task.</returns>
```

**成员**：static System.Threading.Tasks.Task.Yield()</br>
**签名**：_f4e403764ad42836</br>
**注释**：

```xml
<summary>Creates an awaitable task that asynchronously yields back to the current context when awaited.</summary>
<returns>A context that, when awaited, will asynchronously transition back into the current context at the time of the await. If the current <see cref="T:System.Threading.SynchronizationContext" /> is non-null, it is treated as the current context. Otherwise, the task scheduler that is associated with the currently executing task is treated as the current context.</returns>
```

**成员**：System.Threading.Tasks.Task.Wait()</br>
**签名**：_1594f07e6f31cc00</br>
**注释**：

```xml
<summary>Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution.</summary>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
<exception cref="T:System.AggregateException">The task was canceled. The <see cref="P:System.AggregateException.InnerExceptions" /> collection contains a <see cref="T:System.Threading.Tasks.TaskCanceledException" /> object. -or- An exception was thrown during the execution of the task. The <see cref="P:System.AggregateException.InnerExceptions" /> collection contains information about the exception or exceptions.</exception>
```

**成员**：System.Threading.Tasks.Task.Wait(System.TimeSpan)</br>
**签名**：_591f7e80884826c4</br>
**注释**：

```xml
<summary>Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution within a specified time interval.</summary>
<param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an infinite time-out. -or- <paramref name="timeout" /> is greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.</exception>
<exception cref="T:System.AggregateException">The task was canceled. The <see cref="P:System.AggregateException.InnerExceptions" /> collection contains a <see cref="T:System.Threading.Tasks.TaskCanceledException" /> object. -or- An exception was thrown during the execution of the task. The <see cref="P:System.AggregateException.InnerExceptions" /> collection contains information about the exception or exceptions.</exception>
<returns>  <see langword="true" /> if the <see cref="T:System.Threading.Tasks.Task" /> completed execution within the allotted time; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Threading.Tasks.Task.Wait(System.TimeSpan, System.Threading.CancellationToken)</br>
**签名**：_f5ac6969a7868bed</br>
**注释**：

```xml
<summary>Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution.</summary>
<param name="timeout">The time to wait, or <see cref="F:System.Threading.Timeout.InfiniteTimeSpan" /> to wait indefinitely</param>
<param name="cancellationToken">A <see cref="P:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
<exception cref="T:System.AggregateException">The <see cref="T:System.Threading.Tasks.Task" /> was canceled-or-an exception was thrown during the execution of the <see cref="T:System.Threading.Tasks.Task" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an            infinite time-out-or-timeout is greater than            <see cref="F:System.Int32.MaxValue" />.</exception>
<exception cref="T:System.OperationCanceledException">The <paramref name="cancellationToken" /> was canceled.</exception>
<returns>  <see langword="true" /> if the <see cref="T:System.Threading.Tasks.Task" /> completed execution within the allotted time; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Threading.Tasks.Task.Wait(System.Threading.CancellationToken)</br>
**签名**：_0ae24698cd349db7</br>
**注释**：

```xml
<summary>Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution. The wait terminates if a cancellation token is canceled before the task completes.</summary>
<param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
<exception cref="T:System.OperationCanceledException">The <paramref name="cancellationToken" /> was canceled.</exception>
<exception cref="T:System.ObjectDisposedException">The task has been disposed.</exception>
<exception cref="T:System.AggregateException">The task was canceled. The <see cref="P:System.AggregateException.InnerExceptions" /> collection contains a <see cref="T:System.Threading.Tasks.TaskCanceledException" /> object. -or- An exception was thrown during the execution of the task. The <see cref="P:System.AggregateException.InnerExceptions" /> collection contains information about the exception or exceptions.</exception>
```

**成员**：System.Threading.Tasks.Task.Wait(int)</br>
**签名**：_31c9338e14c100f0</br>
**注释**：

```xml
<summary>Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution within a specified number of milliseconds.</summary>
<param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
<exception cref="T:System.AggregateException">The task was canceled. The <see cref="P:System.AggregateException.InnerExceptions" /> collection contains a <see cref="T:System.Threading.Tasks.TaskCanceledException" /> object. -or- An exception was thrown during the execution of the task. The <see cref="P:System.AggregateException.InnerExceptions" /> collection contains information about the exception or exceptions.</exception>
<returns>  <see langword="true" /> if the <see cref="T:System.Threading.Tasks.Task" /> completed execution within the allotted time; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Threading.Tasks.Task.Wait(int, System.Threading.CancellationToken)</br>
**签名**：_3abcae6b9f17598c</br>
**注释**：

```xml
<summary>Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution. The wait terminates if a timeout interval elapses or a cancellation token is canceled before the task completes.</summary>
<param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
<param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
<exception cref="T:System.OperationCanceledException">The <paramref name="cancellationToken" /> was canceled.</exception>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
<exception cref="T:System.AggregateException">The task was canceled. The <see cref="P:System.AggregateException.InnerExceptions" /> collection contains a <see cref="T:System.Threading.Tasks.TaskCanceledException" /> object. -or- An exception was thrown during the execution of the task. The <see cref="P:System.AggregateException.InnerExceptions" /> collection contains information about the exception or exceptions.</exception>
<returns>  <see langword="true" /> if the <see cref="T:System.Threading.Tasks.Task" /> completed execution within the allotted time; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Threading.Tasks.Task.WaitAsync(System.Threading.CancellationToken)</br>
**签名**：_ad9afc914886a128</br>
**注释**：

```xml
<summary>Gets a <see cref="T:System.Threading.Tasks.Task" /> that will complete when this <see cref="T:System.Threading.Tasks.Task" /> completes or when the specified <see cref="P:System.Threading.CancellationToken" /> has cancellation requested.</summary>
<param name="cancellationToken">The <see cref="P:System.Threading.CancellationToken" /> to monitor for a cancellation request.</param>
<exception cref="T:System.OperationCanceledException">The cancellation token was canceled. This exception is stored into the returned task.</exception>
<returns>The <see cref="T:System.Threading.Tasks.Task" /> representing the asynchronous wait. It may or may not be the same instance as the current instance.</returns>
```

**成员**：System.Threading.Tasks.Task.WaitAsync(System.TimeSpan)</br>
**签名**：_f579ca933233a01c</br>
**注释**：

```xml
<summary>Gets a <see cref="T:System.Threading.Tasks.Task" /> that will complete when this <see cref="T:System.Threading.Tasks.Task" /> completes or when the specified timeout expires.</summary>
<param name="timeout">The timeout after which the <see cref="T:System.Threading.Tasks.Task" /> should be faulted with a <see cref="T:System.TimeoutException" /> if it hasn't otherwise completed.</param>
<returns>The <see cref="T:System.Threading.Tasks.Task" /> representing the asynchronous wait. It may or may not be the same instance as the current instance.</returns>
```

**成员**：System.Threading.Tasks.Task.WaitAsync(System.TimeSpan, System.TimeProvider)</br>
**签名**：_263b4b628e4d1a20</br>
**注释**：

```xml
<summary>Gets a <see cref="T:System.Threading.Tasks.Task" /> that will complete when this <see cref="T:System.Threading.Tasks.Task" /> completes or when the specified timeout expires.</summary>
<param name="timeout">The timeout after which the <see cref="T:System.Threading.Tasks.Task" /> should be faulted with a <see cref="T:System.TimeoutException" /> if it hasn't otherwise completed.</param>
<param name="timeProvider">The <see cref="T:System.TimeProvider" /> with which to interpret <paramref name="timeout" />.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="timeProvider" /> argument is <see langword="null" />.</exception>
<returns>The <see cref="T:System.Threading.Tasks.Task" /> representing the asynchronous wait.  It may or may not be the same instance as the current instance.</returns>
```

**成员**：System.Threading.Tasks.Task.WaitAsync(System.TimeSpan, System.Threading.CancellationToken)</br>
**签名**：_d36be122fd9a52dd</br>
**注释**：

```xml
<summary>Gets a <see cref="T:System.Threading.Tasks.Task" /> that will complete when this <see cref="T:System.Threading.Tasks.Task" /> completes, when the specified timeout expires, or when the specified <see cref="P:System.Threading.CancellationToken" /> has cancellation requested.</summary>
<param name="timeout">The timeout after which the <see cref="T:System.Threading.Tasks.Task" /> should be faulted with a <see cref="T:System.TimeoutException" /> if it hasn't otherwise completed.</param>
<param name="cancellationToken">The <see cref="P:System.Threading.CancellationToken" /> to monitor for a cancellation request.</param>
<exception cref="T:System.OperationCanceledException">The cancellation token was canceled. This exception is stored into the returned task.</exception>
<returns>The <see cref="T:System.Threading.Tasks.Task" /> representing the asynchronous wait. It may or may not be the same instance as the current instance.</returns>
```

**成员**：System.Threading.Tasks.Task.WaitAsync(System.TimeSpan, System.TimeProvider, System.Threading.CancellationToken)</br>
**签名**：_c5cedb48e708d62d</br>
**注释**：

```xml
<summary>Gets a <see cref="T:System.Threading.Tasks.Task" /> that will complete when this <see cref="T:System.Threading.Tasks.Task" /> completes, when the specified timeout expires, or when the specified <see cref="T:System.Threading.CancellationToken" /> has cancellation requested.</summary>
<param name="timeout">The timeout after which the <see cref="T:System.Threading.Tasks.Task" /> should be faulted with a <see cref="T:System.TimeoutException" /> if it hasn't otherwise completed.</param>
<param name="timeProvider">The <see cref="T:System.TimeProvider" /> with which to interpret <paramref name="timeout" />.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to monitor for a cancellation request.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="timeProvider" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.OperationCanceledException">The cancellation token was canceled. This exception is stored into the returned task.</exception>
<returns>The <see cref="T:System.Threading.Tasks.Task" /> representing the asynchronous wait.  It may or may not be the same instance as the current instance.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task>)</br>
**签名**：_42870c69dd0eb9d8</br>
**注释**：

```xml
<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
<param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is <see langword="null" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task>, System.Threading.CancellationToken)</br>
**签名**：_f6aaa640c4977029</br>
**注释**：

```xml
<summary>Creates a continuation that receives a cancellation token and executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
<param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
<param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to the new continuation task.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> that created the token has already been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task>, System.Threading.Tasks.TaskScheduler)</br>
**签名**：_31fe4c9b6470785b</br>
**注释**：

```xml
<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes. The continuation uses a specified scheduler.</summary>
<param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
<param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is <see langword="null" />. -or- The <paramref name="scheduler" /> argument is null.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task>, System.Threading.Tasks.TaskContinuationOptions)</br>
**签名**：_e479b4b2988a20a4</br>
**注释**：

```xml
<summary>Creates a continuation that executes when the target task completes according to the specified <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</summary>
<param name="continuationAction">An action to run according to the specified <paramref name="continuationOptions" />. When run, the delegate will be passed the completed task as an argument.</param>
<param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task>, System.Threading.CancellationToken, System.Threading.Tasks.TaskContinuationOptions, System.Threading.Tasks.TaskScheduler)</br>
**签名**：_6798878bd9396e39</br>
**注释**：

```xml
<summary>Creates a continuation that executes when the target task competes according to the specified <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />. The continuation receives a cancellation token and uses a specified scheduler.</summary>
<param name="continuationAction">An action to run according to the specified <paramref name="continuationOptions" />. When run, the delegate will be passed the completed task as an argument.</param>
<param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to the new continuation task.</param>
<param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
<param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> that created the token has already been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null. -or- The <paramref name="scheduler" /> argument is null.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task, object>, object)</br>
**签名**：_c0b1f1737fb5274e</br>
**注释**：

```xml
<summary>Creates a continuation that receives caller-supplied state information and executes when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
<param name="continuationAction">An action to run when the task completes. When run, the delegate is passed the completed task and a caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation action.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is <see langword="null" />.</exception>
<returns>A new continuation task.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task, object>, object, System.Threading.CancellationToken)</br>
**签名**：_a1c3856bf9ec7f94</br>
**注释**：

```xml
<summary>Creates a continuation that receives caller-supplied state information and a cancellation token and that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
<param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation action.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task, object>, object, System.Threading.Tasks.TaskScheduler)</br>
**签名**：_c98db2d4923664cc</br>
**注释**：

```xml
<summary>Creates a continuation that receives caller-supplied state information and executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes. The continuation uses a specified scheduler.</summary>
<param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task" /> completes.  When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation action.</param>
<param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is <see langword="null" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task, object>, object, System.Threading.Tasks.TaskContinuationOptions)</br>
**签名**：_6276124cb311c12a</br>
**注释**：

```xml
<summary>Creates a continuation that receives caller-supplied state information and executes when the target <see cref="T:System.Threading.Tasks.Task" /> completes. The continuation executes based on a set of specified conditions.</summary>
<param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation action.</param>
<param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith(System.Action<System.Threading.Tasks.Task, object>, object, System.Threading.CancellationToken, System.Threading.Tasks.TaskContinuationOptions, System.Threading.Tasks.TaskScheduler)</br>
**签名**：_bf9404373dee65a3</br>
**注释**：

```xml
<summary>Creates a continuation that receives caller-supplied state information and a cancellation token and that executes when the target <see cref="T:System.Threading.Tasks.Task" /> completes. The continuation executes based on a set of specified conditions and uses a specified scheduler.</summary>
<param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation action.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
<param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
<param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its  execution.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
<exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, TResult>)</br>
**签名**：_7d7b67122a4ac6c2</br>
**注释**：

```xml
<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes and returns a value.</summary>
<param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
<typeparam name="TResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
<returns>A new continuation task.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, TResult>, System.Threading.CancellationToken)</br>
**签名**：_27c27506d65c32ef</br>
**注释**：

```xml
<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes and returns a value. The continuation receives a cancellation token.</summary>
<param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
<param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to the new continuation task.</param>
<typeparam name="TResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed. -or- The <see cref="T:System.Threading.CancellationTokenSource" /> that created the token has already been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, TResult>, System.Threading.Tasks.TaskScheduler)</br>
**签名**：_27b8beeb6791105d</br>
**注释**：

```xml
<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes and returns a value. The continuation uses a specified scheduler.</summary>
<param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
<param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
<typeparam name="TResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null. -or- The <paramref name="scheduler" /> argument is null.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, TResult>, System.Threading.Tasks.TaskContinuationOptions)</br>
**签名**：_ca92ad467c5ad377</br>
**注释**：

```xml
<summary>Creates a continuation that executes according to the specified continuation options and returns a value.</summary>
<param name="continuationFunction">A function to run according to the condition specified in <paramref name="continuationOptions" />. When run, the delegate will be passed the completed task as an argument.</param>
<param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
<typeparam name="TResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, TResult>, System.Threading.CancellationToken, System.Threading.Tasks.TaskContinuationOptions, System.Threading.Tasks.TaskScheduler)</br>
**签名**：_a91194cd6fe4a804</br>
**注释**：

```xml
<summary>Creates a continuation that executes according to the specified continuation options and returns a value. The continuation is passed a cancellation token and uses a specified scheduler.</summary>
<param name="continuationFunction">A function to run according to the specified <c>continuationOptions.</c> When run, the delegate will be passed the completed task as an argument.</param>
<param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to the new continuation task.</param>
<param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
<param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
<typeparam name="TResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed. -or- The <see cref="T:System.Threading.CancellationTokenSource" /> that created the token has already been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null. -or- The <paramref name="scheduler" /> argument is null.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, object, TResult>, object)</br>
**签名**：_c90ac65203d1352e</br>
**注释**：

```xml
<summary>Creates a continuation that receives caller-supplied state information and executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes and returns a value.</summary>
<param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation function.</param>
<typeparam name="TResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is <see langword="null" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, object, TResult>, object, System.Threading.CancellationToken)</br>
**签名**：_68bee76bd94d95ee</br>
**注释**：

```xml
<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes and returns a value. The continuation receives caller-supplied state information and a cancellation token.</summary>
<param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation function.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
<typeparam name="TResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, object, TResult>, object, System.Threading.Tasks.TaskScheduler)</br>
**签名**：_a7f062d93de2ed93</br>
**注释**：

```xml
<summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes. The continuation receives caller-supplied state information and uses a specified scheduler.</summary>
<param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task" /> completes.  When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation function.</param>
<param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
<typeparam name="TResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is <see langword="null" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, object, TResult>, object, System.Threading.Tasks.TaskContinuationOptions)</br>
**签名**：_81acb4f27ed5b790</br>
**注释**：

```xml
<summary>Creates a continuation that executes based on the specified task continuation options when the target <see cref="T:System.Threading.Tasks.Task" /> completes. The continuation receives caller-supplied state information.</summary>
<param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation function.</param>
<param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
<typeparam name="TResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：System.Threading.Tasks.Task.ContinueWith<TResult>(System.Func<System.Threading.Tasks.Task, object, TResult>, object, System.Threading.CancellationToken, System.Threading.Tasks.TaskContinuationOptions, System.Threading.Tasks.TaskScheduler)</br>
**签名**：_e31e78776c233392</br>
**注释**：

```xml
<summary>Creates a continuation that executes based on the specified task continuation options when the target <see cref="T:System.Threading.Tasks.Task" /> completes and returns a value. The continuation receives caller-supplied state information and a cancellation token and uses the specified scheduler.</summary>
<param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
<param name="state">An object representing data to be used by the continuation function.</param>
<param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
<param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
<param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its  execution.</param>
<typeparam name="TResult">The type of the result produced by the continuation.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
<exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
<returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
```

**成员**：static System.Threading.Tasks.Task.WaitAll(params System.Threading.Tasks.Task[])</br>
**签名**：_41e1c022a07a165c</br>
**注释**：

```xml
<summary>Waits for all of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution.</summary>
<param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
<exception cref="T:System.ObjectDisposedException">One or more of the <see cref="T:System.Threading.Tasks.Task" /> objects in <paramref name="tasks" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
<exception cref="T:System.AggregateException">At least one of the <see cref="T:System.Threading.Tasks.Task" /> instances was canceled. If a task was canceled, the <see cref="T:System.AggregateException" /> exception contains an <see cref="T:System.OperationCanceledException" /> exception in its <see cref="P:System.AggregateException.InnerExceptions" /> collection. -or- An exception was thrown during the execution of at least one of the <see cref="T:System.Threading.Tasks.Task" /> instances.</exception>
```

**成员**：static System.Threading.Tasks.Task.WaitAll(params System.ReadOnlySpan<System.Threading.Tasks.Task>)</br>
**签名**：_950ed2cc45523925</br>
**注释**：

```xml
<summary>Waits for all of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution.</summary>
<param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument contains a <see langword="null" /> element.</exception>
<exception cref="T:System.AggregateException">At least one of the <see cref="T:System.Threading.Tasks.Task" /> instances was canceled.-or-An exception was thrown during            the execution of at least one of the <see cref="T:System.Threading.Tasks.Task" /> instances.</exception>
```

**成员**：static System.Threading.Tasks.Task.WaitAll(System.Threading.Tasks.Task[], System.TimeSpan)</br>
**签名**：_f8fce6748b855ce2</br>
**注释**：

```xml
<summary>Waits for all of the provided cancellable <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified time interval.</summary>
<param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
<param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
<exception cref="T:System.ObjectDisposedException">One or more of the <see cref="T:System.Threading.Tasks.Task" /> objects in <paramref name="tasks" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.AggregateException">At least one of the <see cref="T:System.Threading.Tasks.Task" /> instances was canceled. If a task was canceled, the <see cref="T:System.AggregateException" /> contains an <see cref="T:System.OperationCanceledException" /> in its <see cref="P:System.AggregateException.InnerExceptions" /> collection. -or- An exception was thrown during the execution of at least one of the <see cref="T:System.Threading.Tasks.Task" /> instances.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an infinite time-out. -or- <paramref name="timeout" /> is greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
<returns>  <see langword="true" /> if all of the <see cref="T:System.Threading.Tasks.Task" /> instances completed execution within the allotted time; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Threading.Tasks.Task.WaitAll(System.Threading.Tasks.Task[], int)</br>
**签名**：_daa1f706f69a1f60</br>
**注释**：

```xml
<summary>Waits for all of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified number of milliseconds.</summary>
<param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
<param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
<exception cref="T:System.ObjectDisposedException">One or more of the <see cref="T:System.Threading.Tasks.Task" /> objects in <paramref name="tasks" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.AggregateException">At least one of the <see cref="T:System.Threading.Tasks.Task" /> instances was canceled. If a task was canceled, the <see cref="T:System.AggregateException" /> contains an <see cref="T:System.OperationCanceledException" /> in its <see cref="P:System.AggregateException.InnerExceptions" /> collection. -or- An exception was thrown during the execution of at least one of the <see cref="T:System.Threading.Tasks.Task" /> instances.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
<returns>  <see langword="true" /> if all of the <see cref="T:System.Threading.Tasks.Task" /> instances completed execution within the allotted time; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Threading.Tasks.Task.WaitAll(System.Threading.Tasks.Task[], System.Threading.CancellationToken)</br>
**签名**：_8f55779be329115b</br>
**注释**：

```xml
<summary>Waits for all of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution unless the wait is cancelled.</summary>
<param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
<param name="cancellationToken">A <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> to observe while waiting for the tasks to complete.</param>
<exception cref="T:System.OperationCanceledException">The <paramref name="cancellationToken" /> was canceled.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.AggregateException">At least one of the <see cref="T:System.Threading.Tasks.Task" /> instances was canceled. If a task was canceled, the <see cref="T:System.AggregateException" /> contains an <see cref="T:System.OperationCanceledException" /> in its <see cref="P:System.AggregateException.InnerExceptions" /> collection. -or- An exception was thrown during the execution of at least one of the <see cref="T:System.Threading.Tasks.Task" /> instances.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
<exception cref="T:System.ObjectDisposedException">One or more of the <see cref="T:System.Threading.Tasks.Task" /> objects in <paramref name="tasks" /> has been disposed.</exception>
```

**成员**：static System.Threading.Tasks.Task.WaitAll(System.Threading.Tasks.Task[], int, System.Threading.CancellationToken)</br>
**签名**：_d7522c9a3480bafa</br>
**注释**：

```xml
<summary>Waits for all of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified number of milliseconds or until the wait is cancelled.</summary>
<param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
<param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
<param name="cancellationToken">A <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> to observe while waiting for the tasks to complete.</param>
<exception cref="T:System.ObjectDisposedException">One or more of the <see cref="T:System.Threading.Tasks.Task" /> objects in <paramref name="tasks" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.AggregateException">At least one of the <see cref="T:System.Threading.Tasks.Task" /> instances was canceled. If a task was canceled, the <see cref="T:System.AggregateException" /> contains an <see cref="T:System.OperationCanceledException" /> in its <see cref="P:System.AggregateException.InnerExceptions" /> collection. -or- An exception was thrown during the execution of at least one of the <see cref="T:System.Threading.Tasks.Task" /> instances.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
<exception cref="T:System.OperationCanceledException">The <paramref name="cancellationToken" /> was canceled.</exception>
<returns>  <see langword="true" /> if all of the <see cref="T:System.Threading.Tasks.Task" /> instances completed execution within the allotted time; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Threading.Tasks.Task.WaitAll(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task>, System.Threading.CancellationToken)</br>
**签名**：_6bcdad547747a518</br>
**注释**：

```xml
<summary>Waits for all of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution unless the wait is cancelled.</summary>
<param name="tasks">A collection of tasks on which to wait.</param>
<param name="cancellationToken">A token to observe while waiting for the tasks to complete.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a <see langword="null" /> element.</exception>
<exception cref="T:System.ObjectDisposedException">One or more of the <see cref="T:System.Threading.Tasks.Task" /> objects in tasks has been disposed.</exception>
<exception cref="T:System.OperationCanceledException">The <paramref name="cancellationToken" /> was canceled.</exception>
<exception cref="T:System.AggregateException">At least one of the <see cref="T:System.Threading.Tasks.Task" /> instances was canceled. If a task was canceled, the <see cref="T:System.AggregateException" />            contains an <see cref="T:System.OperationCanceledException" /> in its <see cref="P:System.AggregateException.InnerExceptions" /> collection.</exception>
```

**成员**：static System.Threading.Tasks.Task.WaitAny(params System.Threading.Tasks.Task[])</br>
**签名**：_a7f38153597cbfe4</br>
**注释**：

```xml
<summary>Waits for any of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution.</summary>
<param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
<returns>The index of the completed <see cref="T:System.Threading.Tasks.Task" /> object in the <paramref name="tasks" /> array.</returns>
```

**成员**：static System.Threading.Tasks.Task.WaitAny(System.Threading.Tasks.Task[], System.TimeSpan)</br>
**签名**：_4aa06494e0b5a7e1</br>
**注释**：

```xml
<summary>Waits for any of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified time interval.</summary>
<param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
<param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <see cref="P:System.TimeSpan.TotalMilliseconds" /> property of the <paramref name="timeout" /> argument is a negative number other than -1, which represents an infinite time-out. -or- The <see cref="P:System.TimeSpan.TotalMilliseconds" /> property of the <paramref name="timeout" /> argument is greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
<returns>The index of the completed task in the <paramref name="tasks" /> array argument, or -1 if the timeout occurred.</returns>
```

**成员**：static System.Threading.Tasks.Task.WaitAny(System.Threading.Tasks.Task[], System.Threading.CancellationToken)</br>
**签名**：_d6006967fd3ff1ae</br>
**注释**：

```xml
<summary>Waits for any of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution unless the wait is cancelled.</summary>
<param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
<param name="cancellationToken">A <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> to observe while waiting for a task to complete.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
<exception cref="T:System.OperationCanceledException">The <paramref name="cancellationToken" /> was canceled.</exception>
<returns>The index of the completed task in the <paramref name="tasks" /> array argument.</returns>
```

**成员**：static System.Threading.Tasks.Task.WaitAny(System.Threading.Tasks.Task[], int)</br>
**签名**：_2291d9e80a279f88</br>
**注释**：

```xml
<summary>Waits for any of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified number of milliseconds.</summary>
<param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
<param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
<returns>The index of the completed task in the <paramref name="tasks" /> array argument, or -1 if the timeout occurred.</returns>
```

**成员**：static System.Threading.Tasks.Task.WaitAny(System.Threading.Tasks.Task[], int, System.Threading.CancellationToken)</br>
**签名**：_a2afaebb710c2e05</br>
**注释**：

```xml
<summary>Waits for any of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified number of milliseconds or until a cancellation token is cancelled.</summary>
<param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
<param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
<param name="cancellationToken">A <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> to observe while waiting for a task to complete.</param>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
<exception cref="T:System.OperationCanceledException">The <paramref name="cancellationToken" /> was canceled.</exception>
<returns>The index of the completed task in the <paramref name="tasks" /> array argument, or -1 if the timeout occurred.</returns>
```

**成员**：static System.Threading.Tasks.Task.FromResult<TResult>(TResult)</br>
**签名**：_76486886fd6b2143</br>
**注释**：

```xml
<summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that's completed successfully with the specified result.</summary>
<param name="result">The result to store into the completed task.</param>
<typeparam name="TResult">The type of the result returned by the task.</typeparam>
<returns>The successfully completed task.</returns>
```

**成员**：static System.Threading.Tasks.Task.FromException(System.Exception)</br>
**签名**：_681f263276bb77fd</br>
**注释**：

```xml
<summary>Creates a <see cref="T:System.Threading.Tasks.Task" /> that has completed with a specified exception.</summary>
<param name="exception">The exception with which to complete the task.</param>
<returns>The faulted task.</returns>
```

**成员**：static System.Threading.Tasks.Task.FromException<TResult>(System.Exception)</br>
**签名**：_f14ed013f26abbfe</br>
**注释**：

```xml
<summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that's completed with a specified exception.</summary>
<param name="exception">The exception with which to complete the task.</param>
<typeparam name="TResult">The type of the result returned by the task.</typeparam>
<returns>The faulted task.</returns>
```

**成员**：static System.Threading.Tasks.Task.FromCanceled(System.Threading.CancellationToken)</br>
**签名**：_2a2b8d828dc4e32b</br>
**注释**：

```xml
<summary>Creates a <see cref="T:System.Threading.Tasks.Task" /> that's completed due to cancellation with a specified cancellation token.</summary>
<param name="cancellationToken">The cancellation token with which to complete the task.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Cancellation has not been requested for <paramref name="cancellationToken" />; its <see cref="P:System.Threading.CancellationToken.IsCancellationRequested" /> property is <see langword="false" />.</exception>
<returns>The canceled task.</returns>
```

**成员**：static System.Threading.Tasks.Task.FromCanceled<TResult>(System.Threading.CancellationToken)</br>
**签名**：_84bf39167a494585</br>
**注释**：

```xml
<summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that's completed due to cancellation with a specified cancellation token.</summary>
<param name="cancellationToken">The cancellation token with which to complete the task.</param>
<typeparam name="TResult">The type of the result returned by the task.</typeparam>
<exception cref="T:System.ArgumentOutOfRangeException">Cancellation has not been requested for <paramref name="cancellationToken" />; its <see cref="P:System.Threading.CancellationToken.IsCancellationRequested" /> property is <see langword="false" />.</exception>
<returns>The canceled task.</returns>
```

**成员**：static System.Threading.Tasks.Task.Run(System.Action)</br>
**签名**：_da51a19b5762a1f4</br>
**注释**：

```xml
<summary>Queues the specified work to run on the thread pool and returns a <see cref="T:System.Threading.Tasks.Task" /> object that represents that work.</summary>
<param name="action">The work to execute asynchronously.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="action" /> parameter was <see langword="null" />.</exception>
<returns>A task that represents the work queued to execute in the ThreadPool.</returns>
```

**成员**：static System.Threading.Tasks.Task.Run(System.Action, System.Threading.CancellationToken)</br>
**签名**：_a3df9536862f3937</br>
**注释**：

```xml
<summary>Queues the specified work to run on the thread pool and returns a <see cref="T:System.Threading.Tasks.Task" /> object that represents that work. A cancellation token allows the work to be cancelled if it has not yet started.</summary>
<param name="action">The work to execute asynchronously.</param>
<param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started. <see cref="M:System.Threading.Tasks.Task.Run(System.Action,System.Threading.CancellationToken)" /> does not pass <paramref name="cancellationToken" /> to <paramref name="action" />.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="action" /> parameter was <see langword="null" />.</exception>
<exception cref="T:System.OperationCanceledException">The task has been canceled. This exception is stored into the returned task.</exception>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> associated with <paramref name="cancellationToken" /> was disposed.</exception>
<exception cref="T:System.Threading.Tasks.TaskCanceledException">The task has been canceled.</exception>
<returns>A task that represents the work queued to execute in the thread pool.</returns>
```

**成员**：static System.Threading.Tasks.Task.Run<TResult>(System.Func<TResult>)</br>
**签名**：_d928ffeaf8804ba2</br>
**注释**：

```xml
<summary>Queues the specified work to run on the thread pool and returns a <see cref="T:System.Threading.Tasks.Task`1" /> object that represents that work. A cancellation token allows the work to be cancelled if it has not yet started.</summary>
<param name="function">The work to execute asynchronously.</param>
<typeparam name="TResult">The return type of the task.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="function" /> parameter is <see langword="null" />.</exception>
<returns>A task object that represents the work queued to execute in the thread pool.</returns>
```

**成员**：static System.Threading.Tasks.Task.Run<TResult>(System.Func<TResult>, System.Threading.CancellationToken)</br>
**签名**：_38b8d80dd098c8e1</br>
**注释**：

```xml
<summary>Queues the specified work to run on the thread pool and returns a <see langword="Task(TResult)" /> object that represents that work.</summary>
<param name="function">The work to execute asynchronously.</param>
<param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started. <see cref="M:System.Threading.Tasks.Task.Run``1(System.Func{``0},System.Threading.CancellationToken)" /> does not pass <paramref name="cancellationToken" /> to <paramref name="action" />.</param>
<typeparam name="TResult">The result type of the task.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="function" /> parameter is <see langword="null" />.</exception>
<exception cref="T:System.OperationCanceledException">The task has been canceled. This exception is stored into the returned task.</exception>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> associated with <paramref name="cancellationToken" /> was disposed.</exception>
<exception cref="T:System.Threading.Tasks.TaskCanceledException">The task has been canceled.</exception>
<returns>A <see langword="Task(TResult)" /> that represents the work queued to execute in the thread pool.</returns>
```

**成员**：static System.Threading.Tasks.Task.Run(System.Func<System.Threading.Tasks.Task>)</br>
**签名**：_62a7e2b729db2d93</br>
**注释**：

```xml
<summary>Queues the specified work to run on the thread pool and returns a proxy for the task returned by <paramref name="function" />.</summary>
<param name="function">The work to execute asynchronously.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="function" /> parameter was <see langword="null" />.</exception>
<returns>A task that represents a proxy for the task returned by <paramref name="function" />.</returns>
```

**成员**：static System.Threading.Tasks.Task.Run(System.Func<System.Threading.Tasks.Task>, System.Threading.CancellationToken)</br>
**签名**：_cdbfa5101a0dad37</br>
**注释**：

```xml
<summary>Queues the specified work to run on the thread pool and returns a proxy for the task returned by <paramref name="function" />. A cancellation token allows the work to be cancelled if it has not yet started.</summary>
<param name="function">The work to execute asynchronously.</param>
<param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started. <see cref="M:System.Threading.Tasks.Task.Run(System.Func{System.Threading.Tasks.Task},System.Threading.CancellationToken)" /> does not pass <paramref name="cancellationToken" /> to <paramref name="action" />.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="function" /> parameter was <see langword="null" />.</exception>
<exception cref="T:System.OperationCanceledException">The task has been canceled. This exception is stored into the returned task.</exception>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> associated with <paramref name="cancellationToken" /> was disposed.</exception>
<exception cref="T:System.Threading.Tasks.TaskCanceledException">The task has been canceled.</exception>
<returns>A task that represents a proxy for the task returned by <paramref name="function" />.</returns>
```

**成员**：static System.Threading.Tasks.Task.Run<TResult>(System.Func<System.Threading.Tasks.Task<TResult>>)</br>
**签名**：_452c2b887d5a1fc3</br>
**注释**：

```xml
<summary>Queues the specified work to run on the thread pool and returns a proxy for the <see langword="Task(TResult)" /> returned by <paramref name="function" />. A cancellation token allows the work to be cancelled if it has not yet started.</summary>
<param name="function">The work to execute asynchronously.</param>
<typeparam name="TResult">The type of the result returned by the proxy task.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="function" /> parameter was <see langword="null" />.</exception>
<returns>A <see langword="Task(TResult)" /> that represents a proxy for the <see langword="Task(TResult)" /> returned by <paramref name="function" />.</returns>
```

**成员**：static System.Threading.Tasks.Task.Run<TResult>(System.Func<System.Threading.Tasks.Task<TResult>>, System.Threading.CancellationToken)</br>
**签名**：_da50521c9500efbd</br>
**注释**：

```xml
<summary>Queues the specified work to run on the thread pool and returns a proxy for the <see langword="Task(TResult)" /> returned by <paramref name="function" />.</summary>
<param name="function">The work to execute asynchronously.</param>
<param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started. <see cref="M:System.Threading.Tasks.Task.Run``1(System.Func{System.Threading.Tasks.Task{``0}},System.Threading.CancellationToken)" /> does not pass <paramref name="cancellationToken" /> to <paramref name="action" />.</param>
<typeparam name="TResult">The type of the result returned by the proxy task.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="function" /> parameter was <see langword="null" />.</exception>
<exception cref="T:System.OperationCanceledException">The task has been canceled. This exception is stored into the returned task.</exception>
<exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> associated with <paramref name="cancellationToken" /> was disposed.</exception>
<exception cref="T:System.Threading.Tasks.TaskCanceledException">The task has been canceled.</exception>
<returns>A <see langword="Task(TResult)" /> that represents a proxy for the <see langword="Task(TResult)" /> returned by <paramref name="function" />.</returns>
```

**成员**：static System.Threading.Tasks.Task.Delay(System.TimeSpan)</br>
**签名**：_ff4ca8df194f90bf</br>
**注释**：

```xml
<summary>Creates a task that completes after a specified time interval.</summary>
<param name="delay">The time span to wait before completing the returned task, or <see langword="Timeout.InfiniteTimeSpan" /> to wait indefinitely.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="delay" /> represents a negative time interval other than <see langword="Timeout.InfiniteTimeSpan" />. -or- The <paramref name="delay" /> argument's <see cref="P:System.TimeSpan.TotalMilliseconds" /> property is greater than 4294967294 on .NET 6 and later versions, or <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see> on all previous versions.</exception>
<returns>A task that represents the time delay.</returns>
```

**成员**：static System.Threading.Tasks.Task.Delay(System.TimeSpan, System.TimeProvider)</br>
**签名**：_c515b64b763bdb72</br>
**注释**：

```xml
<summary>Creates a task that completes after a specified time interval.</summary>
<param name="delay">The <see cref="T:System.TimeSpan" /> to wait before completing the returned task, or <see cref="F:System.Threading.Timeout.InfiniteTimeSpan" /> to wait indefinitely.</param>
<param name="timeProvider">The <see cref="T:System.TimeProvider" /> with which to interpret <paramref name="delay" />.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <para>    <paramref name="delay" /> represents a negative time interval other than <see cref="F:System.Threading.Timeout.InfiniteTimeSpan" />.</para>  <para>-or-</para>  <para>    <paramref name="delay" />'s <see cref="P:System.TimeSpan.TotalMilliseconds" /> property is greater than 4294967294.</para></exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="timeProvider" /> argument is <see langword="null" />.</exception>
<returns>A task that represents the time delay.</returns>
```

**成员**：static System.Threading.Tasks.Task.Delay(System.TimeSpan, System.Threading.CancellationToken)</br>
**签名**：_1dd519d143fccf61</br>
**注释**：

```xml
<summary>Creates a cancellable task that completes after a specified time interval.</summary>
<param name="delay">The time span to wait before completing the returned task, or <see langword="Timeout.InfiniteTimeSpan" /> to wait indefinitely.</param>
<param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="delay" /> represents a negative time interval other than <see langword="Timeout.InfiniteTimeSpan" />. -or- The <paramref name="delay" /> argument's <see cref="P:System.TimeSpan.TotalMilliseconds" /> property is greater than 4294967294 on .NET 6 and later versions, or <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see> on all previous versions.</exception>
<exception cref="T:System.OperationCanceledException">The task has been canceled. This exception is stored into the returned task.</exception>
<exception cref="T:System.ObjectDisposedException">The provided <paramref name="cancellationToken" /> has already been disposed.</exception>
<exception cref="T:System.Threading.Tasks.TaskCanceledException">The task has been canceled.</exception>
<returns>A task that represents the time delay.</returns>
```

**成员**：static System.Threading.Tasks.Task.Delay(System.TimeSpan, System.TimeProvider, System.Threading.CancellationToken)</br>
**签名**：_c16542532f5bf55f</br>
**注释**：

```xml
<summary>Creates a cancellable task that completes after a specified time interval.</summary>
<param name="delay">The <see cref="T:System.TimeSpan" /> to wait before completing the returned task, or <see cref="F:System.Threading.Timeout.InfiniteTimeSpan" /> to wait indefinitely.</param>
<param name="timeProvider">The <see cref="T:System.TimeProvider" /> with which to interpret <paramref name="delay" />.</param>
<param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <para>    <paramref name="delay" /> represents a negative time interval other than <see cref="F:System.Threading.Timeout.InfiniteTimeSpan" />.</para>  <para>-or-</para>  <para>    <paramref name="delay" />'s <see cref="P:System.TimeSpan.TotalMilliseconds" /> property is greater than 4294967294.</para></exception>
<exception cref="T:System.ArgumentNullException">The <paramref name="timeProvider" /> argument is <see langword="null" />.</exception>
<exception cref="T:System.OperationCanceledException">The cancellation token was canceled. This exception is stored into the returned task.</exception>
<returns>A task that represents the time delay.</returns>
```

**成员**：static System.Threading.Tasks.Task.Delay(int)</br>
**签名**：_3da1cdb174644ada</br>
**注释**：

```xml
<summary>Creates a task that completes after a specified number of milliseconds.</summary>
<param name="millisecondsDelay">The number of milliseconds to wait before completing the returned task, or -1 to wait indefinitely.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="millisecondsDelay" /> argument is less than -1.</exception>
<returns>A task that represents the time delay.</returns>
```

**成员**：static System.Threading.Tasks.Task.Delay(int, System.Threading.CancellationToken)</br>
**签名**：_34c332c06d4d985b</br>
**注释**：

```xml
<summary>Creates a cancellable task that completes after a specified number of milliseconds.</summary>
<param name="millisecondsDelay">The number of milliseconds to wait before completing the returned task, or -1 to wait indefinitely.</param>
<param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="millisecondsDelay" /> argument is less than -1.</exception>
<exception cref="T:System.OperationCanceledException">The task has been canceled. This exception is stored into the returned task.</exception>
<exception cref="T:System.ObjectDisposedException">The provided <paramref name="cancellationToken" /> has already been disposed.</exception>
<exception cref="T:System.Threading.Tasks.TaskCanceledException">The task has been canceled.</exception>
<returns>A task that represents the time delay.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenAll(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task>)</br>
**签名**：_cb0c072793c59334</br>
**注释**：

```xml
<summary>Creates a task that will complete when all of the <see cref="T:System.Threading.Tasks.Task" /> objects in an enumerable collection have completed.</summary>
<param name="tasks">The tasks to wait on for completion.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument was <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> collection contained a <see langword="null" /> task.</exception>
<returns>A task that represents the completion of all of the supplied tasks.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenAll(params System.Threading.Tasks.Task[])</br>
**签名**：_5bdce56e38e4b97c</br>
**注释**：

```xml
<summary>Creates a task that will complete when all of the <see cref="T:System.Threading.Tasks.Task" /> objects in an array have completed.</summary>
<param name="tasks">The tasks to wait on for completion.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument was <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contained a <see langword="null" /> task.</exception>
<returns>A task that represents the completion of all of the supplied tasks.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenAll(params System.ReadOnlySpan<System.Threading.Tasks.Task>)</br>
**签名**：_d62721be70a65388</br>
**注释**：

```xml
<summary>Creates a task that will complete when all of the supplied tasks have completed.</summary>
<param name="tasks">The tasks to wait on for completion.</param>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a <see langword="null" /> task.</exception>
<returns>A task that represents the completion of all of the supplied tasks.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenAll<TResult>(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<TResult>>)</br>
**签名**：_cfb648f6d9ec34c8</br>
**注释**：

```xml
<summary>Creates a task that will complete when all of the <see cref="T:System.Threading.Tasks.Task`1" /> objects in an enumerable collection have completed.</summary>
<param name="tasks">The tasks to wait on for completion.</param>
<typeparam name="TResult">The type of the completed task.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument was <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> collection contained a <see langword="null" /> task.</exception>
<returns>A task that represents the completion of all of the supplied tasks.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenAll<TResult>(params System.Threading.Tasks.Task<TResult>[])</br>
**签名**：_a54b67fbb4ccb6bc</br>
**注释**：

```xml
<summary>Creates a task that will complete when all of the <see cref="T:System.Threading.Tasks.Task`1" /> objects in an array have completed.</summary>
<param name="tasks">The tasks to wait on for completion.</param>
<typeparam name="TResult">The type of the completed task.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument was <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contained a <see langword="null" /> task.</exception>
<returns>A task that represents the completion of all of the supplied tasks.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenAll<TResult>(params System.ReadOnlySpan<System.Threading.Tasks.Task<TResult>>)</br>
**签名**：_d8cf2ec1f7803bff</br>
**注释**：

```xml
<summary>Creates a task that will complete when all of the supplied tasks have completed.</summary>
<param name="tasks">The tasks to wait on for completion.</param>
<typeparam name="TResult">The type of the result returned by the tasks.</typeparam>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a <see langword="null" /> task.</exception>
<returns>A task that represents the completion of all of the supplied tasks.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenAny(params System.Threading.Tasks.Task[])</br>
**签名**：_ddf19fd1d97f0cd2</br>
**注释**：

```xml
<summary>Creates a task that will complete when any of the supplied tasks have completed.</summary>
<param name="tasks">The tasks to wait on for completion.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument was null.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contained a null task, or was empty.</exception>
<returns>A task that represents the completion of one of the supplied tasks.  The return task's Result is the task that completed.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenAny(params System.ReadOnlySpan<System.Threading.Tasks.Task>)</br>
**签名**：_e7c954aa77999183</br>
**注释**：

```xml
<summary>Creates a task that will complete when any of the supplied tasks have completed.</summary>
<param name="tasks">The tasks to wait on for completion.</param>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a <see langword="null" /> task, or is empty.</exception>
<returns>A task that represents the completion of one of the supplied tasks.  The return Task's Result is the task that completed.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenAny(System.Threading.Tasks.Task, System.Threading.Tasks.Task)</br>
**签名**：_cc30f99c4d488ed9</br>
**注释**：

```xml
<summary>Creates a task that will complete when either of the supplied tasks have completed.</summary>
<param name="task1">The first task to wait on for completion.</param>
<param name="task2">The second task to wait on for completion.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="task1" /> or <paramref name="task2" /> was <see langword="null" />.</exception>
<returns>A new task that represents the completion of one of the supplied tasks. Its <see langword="Result" /> is the task that completed first.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenAny(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task>)</br>
**签名**：_717dc2ba16f86618</br>
**注释**：

```xml
<summary>Creates a task that will complete when any of the supplied tasks have completed.</summary>
<param name="tasks">The tasks to wait on for completion.</param>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument was <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contained a null task, or was empty.</exception>
<returns>A task that represents the completion of one of the supplied tasks.  The return task's Result is the task that completed.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenAny<TResult>(params System.Threading.Tasks.Task<TResult>[])</br>
**签名**：_e1fbf4daaee01944</br>
**注释**：

```xml
<summary>Creates a task that will complete when any of the supplied tasks have completed.</summary>
<param name="tasks">The tasks to wait on for completion.</param>
<typeparam name="TResult">The type of the completed task.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument was null.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contained a null task, or was empty.</exception>
<returns>A task that represents the completion of one of the supplied tasks.  The return task's Result is the task that completed.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenAny<TResult>(params System.ReadOnlySpan<System.Threading.Tasks.Task<TResult>>)</br>
**签名**：_8106e2961a122fe0</br>
**注释**：

```xml
<summary>Creates a task that will complete when any of the supplied tasks have completed.</summary>
<param name="tasks">The tasks to wait on for completion.</param>
<typeparam name="TResult">The type of the result returned by the tasks.</typeparam>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a <see langword="null" /> task, or is empty.</exception>
<returns>A task that represents the completion of one of the supplied tasks.  The return Task's Result is the task that completed.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenAny<TResult>(System.Threading.Tasks.Task<TResult>, System.Threading.Tasks.Task<TResult>)</br>
**签名**：_592d4633f4f24c38</br>
**注释**：

```xml
<summary>Creates a task that will complete when either of the supplied tasks have completed.</summary>
<param name="task1">The first task to wait on for completion.</param>
<param name="task2">The second task to wait on for completion.</param>
<typeparam name="TResult">The type of the result of the returned task.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="task1" /> or <paramref name="task2" /> was <see langword="null" />.</exception>
<returns>A task that represents the completion of one of the supplied tasks. The returned task's <typeparamref name="TResult" /> is the task that completed first.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenAny<TResult>(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<TResult>>)</br>
**签名**：_cf1b91bc49523a2b</br>
**注释**：

```xml
<summary>Creates a task that will complete when any of the supplied tasks have completed.</summary>
<param name="tasks">The tasks to wait on for completion.</param>
<typeparam name="TResult">The type of the completed task.</typeparam>
<exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument was <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contained a null task, or was empty.</exception>
<returns>A task that represents the completion of one of the supplied tasks.  The return task's Result is the task that completed.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenEach(params System.Threading.Tasks.Task[])</br>
**签名**：_2ad9e7d43f12d14d</br>
**注释**：

```xml
<summary>Creates an <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" /> that will yield the supplied tasks as those tasks complete.</summary>
<param name="tasks">The task to iterate through when completed.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="tasks" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="tasks" /> contains a <see langword="null" />.</exception>
<returns>An <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" /> for iterating through the supplied tasks.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenEach(params System.ReadOnlySpan<System.Threading.Tasks.Task>)</br>
**签名**：_2df0fee75892f471</br>
**注释**：

```xml
<summary>Creates an <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.IAsyncEnumerable`1"></xref> that will yield the supplied tasks as those tasks complete.</summary>
<param name="tasks">The tasks to iterate through as they complete.</param>
<returns>An <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.IAsyncEnumerable`1"></xref> for iterating through the supplied tasks.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenEach(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task>)</br>
**签名**：_b06f770db773a3a0</br>
**注释**：

```xml
<summary>Creates an <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.IAsyncEnumerable`1"></xref> that will yield the supplied tasks as those tasks complete.</summary>
<param name="tasks">The tasks to iterate through as they complete.</param>
<returns>An <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.IAsyncEnumerable`1"></xref> for iterating through the supplied tasks.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenEach<TResult>(params System.Threading.Tasks.Task<TResult>[])</br>
**签名**：_287e334b00da970c</br>
**注释**：

```xml
<summary>Creates an <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.IAsyncEnumerable`1"></xref> that will yield the supplied tasks as those tasks complete.</summary>
<param name="tasks">The tasks to iterate through as they complete.</param>
<typeparam name="TResult">The type of the result returned by the tasks.</typeparam>
<returns>An <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.IAsyncEnumerable`1"></xref> for iterating through the supplied tasks.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenEach<TResult>(params System.ReadOnlySpan<System.Threading.Tasks.Task<TResult>>)</br>
**签名**：_4cca3bf88970e2ff</br>
**注释**：

```xml
<summary>Creates an <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.IAsyncEnumerable`1"></xref> that will yield the supplied tasks as those tasks complete.</summary>
<param name="tasks">The tasks to iterate through as they complete.</param>
<typeparam name="TResult">The type of the result returned by the tasks.</typeparam>
<returns>An <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.IAsyncEnumerable`1"></xref> for iterating through the supplied tasks.</returns>
```

**成员**：static System.Threading.Tasks.Task.WhenEach<TResult>(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<TResult>>)</br>
**签名**：_0fb3578fab4c3d87</br>
**注释**：

```xml
<summary>Creates an <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.IAsyncEnumerable`1"></xref> that will yield the supplied tasks as those tasks complete.</summary>
<param name="tasks">The tasks to iterate through as they complete.</param>
<typeparam name="TResult">The type of the result returned by the tasks.</typeparam>
<returns>An <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.IAsyncEnumerable`1"></xref> for iterating through the supplied tasks.</returns>
```

