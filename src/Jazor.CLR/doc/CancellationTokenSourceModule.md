# CancellationTokenSourceModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Threading.CancellationTokenSource.IsCancellationRequested.get</br>
**签名**：_7bce90ebe75fba7d</br>

**成员**：System.Threading.CancellationTokenSource.Token.get</br>
**签名**：_c6beb3ac47585eb0</br>

**成员**：System.Threading.CancellationTokenSource.CancellationTokenSource()</br>
**签名**：_c93a8dffcc42e84b</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class.</summary>
```

**成员**：System.Threading.CancellationTokenSource.CancellationTokenSource(System.TimeSpan)</br>
**签名**：_cbe063f9fd0c2719</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class that will be canceled after the specified time span.</summary>
<param name="delay">The time interval to wait before canceling this <see cref="T:System.Threading.CancellationTokenSource" />.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="delay" />.<see cref="P:System.TimeSpan.TotalMilliseconds" /> is less than -1 or greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see> (or <see cref="F:System.UInt32.MaxValue">UInt32.MaxValue</see> - 1 on some versions of .NET). Note that this upper bound is more restrictive than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>.</exception>
```

**成员**：System.Threading.CancellationTokenSource.CancellationTokenSource(System.TimeSpan, System.TimeProvider)</br>
**签名**：_1c33ef293564b460</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class that will be canceled after the specified <see cref="T:System.TimeSpan" />.</summary>
<param name="delay">The time interval to wait before canceling this <see cref="T:System.Threading.CancellationTokenSource" />.</param>
<param name="timeProvider">The <see cref="T:System.TimeProvider" /> with which to interpret the <paramref name="delay" />.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="delay" />'s <see cref="P:System.TimeSpan.TotalMilliseconds" /> is less than -1 or greater than <see cref="F:System.UInt32.MaxValue" /> - 1.</exception>
<exception cref="T:System.ArgumentNullException">  <paramref name="timeProvider" /> is <see langword="null" />.</exception>
```

**成员**：System.Threading.CancellationTokenSource.CancellationTokenSource(int)</br>
**签名**：_99cb96f8cd1386b9</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class that will be canceled after the specified delay in milliseconds.</summary>
<param name="millisecondsDelay">The time interval in milliseconds to wait before canceling this <see cref="T:System.Threading.CancellationTokenSource" />.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="millisecondsDelay" /> is less than -1.</exception>
```

**成员**：System.Threading.CancellationTokenSource.Cancel()</br>
**签名**：_7b1e80c48df4a4a1</br>
**注释**：

```xml
<summary>Communicates a request for cancellation.</summary>
<exception cref="T:System.ObjectDisposedException">This <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
<exception cref="T:System.AggregateException">An aggregate exception containing all the exceptions thrown by the registered callbacks on the associated <see cref="T:System.Threading.CancellationToken" />.</exception>
```

**成员**：System.Threading.CancellationTokenSource.Cancel(bool)</br>
**签名**：_b528c1e73ac70627</br>
**注释**：

```xml
<summary>Communicates a request for cancellation, and specifies whether remaining callbacks and cancelable operations should be processed if an exception occurs.</summary>
<param name="throwOnFirstException">  <see langword="true" /> if exceptions should immediately propagate; otherwise, <see langword="false" />.</param>
<exception cref="T:System.ObjectDisposedException">This <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
<exception cref="T:System.AggregateException">An aggregate exception containing all the exceptions thrown by the registered callbacks on the associated <see cref="T:System.Threading.CancellationToken" />.</exception>
```

**成员**：System.Threading.CancellationTokenSource.CancelAsync()</br>
**签名**：_d6c75d8a27eec714</br>
**注释**：

```xml
<summary>Communicates a request for cancellation asynchronously.</summary>
<exception cref="T:System.ObjectDisposedException">This <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
<returns>A task that will complete after cancelable operations and callbacks registered with the associated <see cref="T:System.Threading.CancellationToken" /> have completed.</returns>
```

**成员**：System.Threading.CancellationTokenSource.CancelAfter(System.TimeSpan)</br>
**签名**：_142b2ab0f86b3788</br>
**注释**：

```xml
<summary>Schedules a cancel operation on this <see cref="T:System.Threading.CancellationTokenSource" /> after the specified time span.</summary>
<param name="delay">The time span to wait before canceling this <see cref="T:System.Threading.CancellationTokenSource" />.</param>
<exception cref="T:System.ObjectDisposedException">The exception thrown when this <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="delay" />.<see cref="P:System.TimeSpan.TotalMilliseconds" /> is less than -1 or greater than Int32.MaxValue (or UInt32.MaxValue - 1 on some versions of .NET). Note that this upper bound is more restrictive than TimeSpan.MaxValue.</exception>
```

**成员**：System.Threading.CancellationTokenSource.CancelAfter(int)</br>
**签名**：_054ea7e5f7fdad80</br>
**注释**：

```xml
<summary>Schedules a cancel operation on this <see cref="T:System.Threading.CancellationTokenSource" /> after the specified number of milliseconds.</summary>
<param name="millisecondsDelay">The time span to wait before canceling this <see cref="T:System.Threading.CancellationTokenSource" />.</param>
<exception cref="T:System.ObjectDisposedException">The exception thrown when this <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The exception thrown when <paramref name="millisecondsDelay" /> is less than -1.</exception>
```

**成员**：System.Threading.CancellationTokenSource.TryReset()</br>
**签名**：_b73d00b710a1dde2</br>
**注释**：

```xml
<summary>Attempts to reset the <see cref="T:System.Threading.CancellationTokenSource" /> to be used for an unrelated operation.</summary>
<returns>  <see langword="true" /> if the <see cref="T:System.Threading.CancellationTokenSource" /> has not had cancellation requested and could have its state reset to be reused for a subsequent operation; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Threading.CancellationTokenSource.Dispose()</br>
**签名**：_2168e1dc84c34975</br>
**注释**：

```xml
<summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class.</summary>
```

**成员**：static System.Threading.CancellationTokenSource.CreateLinkedTokenSource(System.Threading.CancellationToken, System.Threading.CancellationToken)</br>
**签名**：_00350dc2979ca5c5</br>
**注释**：

```xml
<summary>Creates a <see cref="T:System.Threading.CancellationTokenSource" /> that will be in the canceled state when any of the source tokens are in the canceled state.</summary>
<param name="token1">The first cancellation token to observe.</param>
<param name="token2">The second cancellation token to observe.</param>
<exception cref="T:System.ObjectDisposedException">A <see cref="T:System.Threading.CancellationTokenSource" /> associated with one of the source tokens has been disposed.</exception>
<returns>A <see cref="T:System.Threading.CancellationTokenSource" /> that is linked to the source tokens.</returns>
```

**成员**：static System.Threading.CancellationTokenSource.CreateLinkedTokenSource(System.Threading.CancellationToken)</br>
**签名**：_b01498d7103a5db2</br>
**注释**：

```xml
<summary>Creates a <see cref="T:System.Threading.CancellationTokenSource" /> that will be in the canceled state when the supplied token is in the canceled state.</summary>
<param name="token">The cancellation token to observe.</param>
<returns>An object that's linked to the source token.</returns>
```

**成员**：static System.Threading.CancellationTokenSource.CreateLinkedTokenSource(params System.Threading.CancellationToken[])</br>
**签名**：_943ce2954d0f9210</br>
**注释**：

```xml
<summary>Creates a <see cref="T:System.Threading.CancellationTokenSource" /> that will be in the canceled state when any of the source tokens in the specified array are in the canceled state.</summary>
<param name="tokens">An array that contains the cancellation token instances to observe.</param>
<exception cref="T:System.ObjectDisposedException">A <see cref="T:System.Threading.CancellationTokenSource" /> associated with one of the source tokens has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">  <paramref name="tokens" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="tokens" /> is empty.</exception>
<returns>A <see cref="T:System.Threading.CancellationTokenSource" /> that is linked to the source tokens.</returns>
```

**成员**：static System.Threading.CancellationTokenSource.CreateLinkedTokenSource(params System.ReadOnlySpan<System.Threading.CancellationToken>)</br>
**签名**：_a9302f782f58fc4e</br>
**注释**：

```xml
<summary>Creates a <see cref="T:System.Threading.CancellationTokenSource" /> that will be in the canceled state when any of the source tokens are in the canceled state.</summary>
<param name="tokens">The <see cref="T:System.Threading.CancellationToken">CancellationToken</see> instances to observe.</param>
<returns>A <see cref="T:System.Threading.CancellationTokenSource" /> that is linked to the source tokens.</returns>
```
