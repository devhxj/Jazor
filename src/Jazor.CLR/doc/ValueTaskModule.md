# ValueTaskModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Threading.Tasks.ValueTask.ValueTask()</br>
**签名**：_1403cc3779233c2c</br>

**成员**：System.Threading.Tasks.ValueTask.ValueTask(System.Threading.Tasks.Task)</br>
**签名**：_ecb5062deec182c6</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Threading.Tasks.ValueTask" /> class using the supplied task that represents the operation.</summary>
<param name="task">The task that represents the operation.</param>
```

**成员**：System.Threading.Tasks.ValueTask.ValueTask(System.Threading.Tasks.Sources.IValueTaskSource, short)</br>
**签名**：_ac78e4299343644f</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Threading.Tasks.ValueTask" /> class using the supplied <see cref="T:System.Threading.Tasks.Sources.IValueTaskSource" /> object that represents the operation.</summary>
<param name="source">An object that represents the operation.</param>
<param name="token">An opaque value that is passed through to the <see cref="T:System.Threading.Tasks.Sources.IValueTaskSource" />.</param>
```

**成员**：static System.Threading.Tasks.ValueTask.CompletedTask.get</br>
**签名**：_395d253a48bfa9db</br>

**成员**：static System.Threading.Tasks.ValueTask.FromResult<TResult>(TResult)</br>
**签名**：_a9034816209cc796</br>
**注释**：

```xml
<summary>Creates a <see cref="T:System.Threading.Tasks.ValueTask`1" /> that's completed successfully with the specified result.</summary>
<param name="result">The result to store into the completed task.</param>
<typeparam name="TResult">The type of the result returned by the task.</typeparam>
<returns>The successfully completed task.</returns>
```

**成员**：static System.Threading.Tasks.ValueTask.FromCanceled(System.Threading.CancellationToken)</br>
**签名**：_1659e64e8178f1e4</br>
**注释**：

```xml
<summary>Creates a <see cref="T:System.Threading.Tasks.ValueTask" /> that has completed due to cancellation with the specified cancellation token.</summary>
<param name="cancellationToken">The cancellation token with which to complete the task.</param>
<returns>The canceled task.</returns>
```

**成员**：static System.Threading.Tasks.ValueTask.FromCanceled<TResult>(System.Threading.CancellationToken)</br>
**签名**：_dfe745de979b3dec</br>
**注释**：

```xml
<summary>Creates a <see cref="T:System.Threading.Tasks.ValueTask`1" /> that has completed due to cancellation with the specified cancellation token.</summary>
<param name="cancellationToken">The cancellation token with which to complete the task.</param>
<typeparam name="TResult">The type of the result of the returned task.</typeparam>
<returns>The canceled task.</returns>
```

**成员**：static System.Threading.Tasks.ValueTask.FromException(System.Exception)</br>
**签名**：_2190e6b5d3ce645a</br>
**注释**：

```xml
<summary>Creates a <see cref="T:System.Threading.Tasks.ValueTask" /> that has completed with the specified exception.</summary>
<param name="exception">The exception with which to complete the task.</param>
<returns>The faulted task.</returns>
```

**成员**：static System.Threading.Tasks.ValueTask.FromException<TResult>(System.Exception)</br>
**签名**：_a4781d7c683f775b</br>
**注释**：

```xml
<summary>Creates a <see cref="T:System.Threading.Tasks.ValueTask`1" /> that has completed with the specified exception.</summary>
<param name="exception">The exception with which to complete the task.</param>
<typeparam name="TResult">The type of the result of the returned task.</typeparam>
<returns>The faulted task.</returns>
```

**成员**：override System.Threading.Tasks.ValueTask.GetHashCode()</br>
**签名**：_20eb9b6464367d96</br>
**注释**：

```xml
<summary>Returns the hash code for this instance.</summary>
<returns>The hash code for the current object.</returns>
```

**成员**：override System.Threading.Tasks.ValueTask.Equals(object)</br>
**签名**：_a92fa2f2f0247bd2</br>
**注释**：

```xml
<summary>Determines whether the specified object is equal to the current <see cref="T:System.Threading.Tasks.ValueTask" /> instance.</summary>
<param name="obj">The object to compare with the current object.</param>
<returns>  <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Threading.Tasks.ValueTask.Equals(System.Threading.Tasks.ValueTask)</br>
**签名**：_f9a6103151b45ef3</br>
**注释**：

```xml
<summary>Determines whether the specified <see cref="T:System.Threading.Tasks.ValueTask" /> object is equal to the current <see cref="T:System.Threading.Tasks.ValueTask" /> object.</summary>
<param name="other">The object to compare with the current object.</param>
<returns>  <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Threading.Tasks.ValueTask.operator ==(System.Threading.Tasks.ValueTask, System.Threading.Tasks.ValueTask)</br>
**签名**：_adc1860a7ee9024f</br>
**注释**：

```xml
<summary>Compares two <see cref="T:System.Threading.Tasks.ValueTask" /> values for equality.</summary>
<param name="left">The first value to compare.</param>
<param name="right">The second value to compare.</param>
<returns>  <see langword="true" /> if the two <see cref="T:System.Threading.Tasks.ValueTask" /> values are equal; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Threading.Tasks.ValueTask.operator !=(System.Threading.Tasks.ValueTask, System.Threading.Tasks.ValueTask)</br>
**签名**：_fbc37bb5a64ee224</br>
**注释**：

```xml
<summary>Determines whether two <see cref="T:System.Threading.Tasks.ValueTask" /> values are unequal.</summary>
<param name="left">The first value to compare.</param>
<param name="right">The second value to compare.</param>
<returns>  <see langword="true" /> if the two <see cref="T:System.Threading.Tasks.ValueTask" /> values are not equal; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Threading.Tasks.ValueTask.AsTask()</br>
**签名**：_cca39ba1e0874b20</br>
**注释**：

```xml
<summary>Retrieves a <see cref="T:System.Threading.Tasks.Task" /> object that represents this <see cref="T:System.Threading.Tasks.ValueTask" />.</summary>
<returns>The <see cref="T:System.Threading.Tasks.Task" /> object that is wrapped in this <see cref="T:System.Threading.Tasks.ValueTask" /> if one exists, or a new <see cref="T:System.Threading.Tasks.Task" /> object that represents the result.</returns>
```

**成员**：System.Threading.Tasks.ValueTask.Preserve()</br>
**签名**：_318b1fcbe9f077e1</br>
**注释**：

```xml
<summary>Gets a <see cref="T:System.Threading.Tasks.ValueTask" /> that may be used at any point in the future.</summary>
<returns>The preserved <see cref="T:System.Threading.Tasks.ValueTask" />.</returns>
```

**成员**：System.Threading.Tasks.ValueTask.IsCompleted.get</br>
**签名**：_9b4baba665c34c5a</br>

**成员**：System.Threading.Tasks.ValueTask.IsCompletedSuccessfully.get</br>
**签名**：_c08b29883771cc82</br>

**成员**：System.Threading.Tasks.ValueTask.IsFaulted.get</br>
**签名**：_0a3b06794cb6e22d</br>

**成员**：System.Threading.Tasks.ValueTask.IsCanceled.get</br>
**签名**：_cdb5c5b29ee6c441</br>

**成员**：System.Threading.Tasks.ValueTask.GetAwaiter()</br>
**签名**：_d9f56462100b8fab</br>
**注释**：

```xml
<summary>Creates an awaiter for this value.</summary>
<returns>The awaiter.</returns>
```

**成员**：System.Threading.Tasks.ValueTask.ConfigureAwait(bool)</br>
**签名**：_e56a8766d3702b54</br>
**注释**：

```xml
<summary>Configures an awaiter for this value.</summary>
<param name="continueOnCapturedContext">  <see langword="true" /> to attempt to marshal the continuation back to the captured context; otherwise, <see langword="false" />.</param>
<returns>The configured awaiter.</returns>
```
