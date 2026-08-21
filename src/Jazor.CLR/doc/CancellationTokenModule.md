# CancellationTokenModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Threading.CancellationToken.CancellationToken()</br>
**签名**：_f21ba4033b40a8aa</br>

**成员**：static System.Threading.CancellationToken.None.get</br>
**签名**：_39130b6163fb1960</br>

**成员**：System.Threading.CancellationToken.IsCancellationRequested.get</br>
**签名**：_d304e669ec364248</br>

**成员**：System.Threading.CancellationToken.CanBeCanceled.get</br>
**签名**：_f343b545e3147cce</br>

**成员**：System.Threading.CancellationToken.WaitHandle.get</br>
**签名**：_8f00231516910f63</br>

**成员**：System.Threading.CancellationToken.CancellationToken(bool)</br>
**签名**：_c5634ecc2859098c</br>
**注释**：

```xml
<summary>Initializes the <see cref="T:System.Threading.CancellationToken" />.</summary>
<param name="canceled">The canceled state for the token.</param>
```

**成员**：System.Threading.CancellationToken.Register(System.Action)</br>
**签名**：_72a0106915493c44</br>
**注释**：

```xml
<summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
<param name="callback">The delegate to be executed when the <see cref="T:System.Threading.CancellationToken" /> is canceled.</param>
<exception cref="T:System.ObjectDisposedException">The associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">  <paramref name="callback" /> is null.</exception>
<returns>The <see cref="T:System.Threading.CancellationTokenRegistration" /> instance that can be used to unregister the callback.</returns>
```

**成员**：System.Threading.CancellationToken.Register(System.Action, bool)</br>
**签名**：_2424f34aae18aa06</br>
**注释**：

```xml
<summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
<param name="callback">The delegate to be executed when the <see cref="T:System.Threading.CancellationToken" /> is canceled.</param>
<param name="useSynchronizationContext">A value that indicates whether to capture the current <see cref="T:System.Threading.SynchronizationContext" /> and use it when invoking the <paramref name="callback" />.</param>
<exception cref="T:System.ObjectDisposedException">The associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">  <paramref name="callback" /> is null.</exception>
<returns>The <see cref="T:System.Threading.CancellationTokenRegistration" /> instance that can be used to unregister the callback.</returns>
```

**成员**：System.Threading.CancellationToken.Register(System.Action<object>, object)</br>
**签名**：_eb49f18acb077ff1</br>
**注释**：

```xml
<summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
<param name="callback">The delegate to be executed when the <see cref="T:System.Threading.CancellationToken" /> is canceled.</param>
<param name="state">The state to pass to the <paramref name="callback" /> when the delegate is invoked. This may be null.</param>
<exception cref="T:System.ObjectDisposedException">The associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">  <paramref name="callback" /> is null.</exception>
<returns>The <see cref="T:System.Threading.CancellationTokenRegistration" /> instance that can be used to unregister the callback.</returns>
```

**成员**：System.Threading.CancellationToken.Register(System.Action<object, System.Threading.CancellationToken>, object)</br>
**签名**：_11a6b73058ddd45e</br>
**注释**：

```xml
<summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken">CancellationToken</see> is canceled.</summary>
<param name="callback">The delegate to be executed when the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> is canceled.</param>
<param name="state">The state to pass to the <paramref name="callback" /> when the delegate is invoked.  This may be <see langword="null" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="callback" /> is <see langword="null" />.</exception>
<returns>The <see cref="T:System.Threading.CancellationTokenRegistration" /> instance that can be used to unregister the callback.</returns>
```

**成员**：System.Threading.CancellationToken.Register(System.Action<object>, object, bool)</br>
**签名**：_f55770dedf931292</br>
**注释**：

```xml
<summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
<param name="callback">The delegate to be executed when the <see cref="T:System.Threading.CancellationToken" /> is canceled.</param>
<param name="state">The state to pass to the <paramref name="callback" /> when the delegate is invoked. This may be null.</param>
<param name="useSynchronizationContext">A Boolean value that indicates whether to capture the current <see cref="T:System.Threading.SynchronizationContext" /> and use it when invoking the <paramref name="callback" />.</param>
<exception cref="T:System.ObjectDisposedException">The associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
<exception cref="T:System.ArgumentNullException">  <paramref name="callback" /> is null.</exception>
<returns>The <see cref="T:System.Threading.CancellationTokenRegistration" /> instance that can be used to unregister the callback.</returns>
```

**成员**：System.Threading.CancellationToken.UnsafeRegister(System.Action<object>, object)</br>
**签名**：_54049b6fbd22e813</br>
**注释**：

```xml
<summary>Registers a delegate that is called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
<param name="callback">The delegate to execute when the <see cref="T:System.Threading.CancellationToken" /> is canceled.</param>
<param name="state">The state to pass to the <paramref name="callback" /> when the delegate is invoked.  This may be <see langword="null" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="callback" /> is null.</exception>
<returns>An object that can             be used to unregister the callback.</returns>
```

**成员**：System.Threading.CancellationToken.UnsafeRegister(System.Action<object, System.Threading.CancellationToken>, object)</br>
**签名**：_bd3fc6b3035e6a60</br>
**注释**：

```xml
<summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken">CancellationToken</see> is canceled.</summary>
<param name="callback">The delegate to be executed when the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> is canceled.</param>
<param name="state">The state to pass to the <paramref name="callback" /> when the delegate is invoked.  This may be <see langword="null" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="callback" /> is <see langword="null" />.</exception>
<returns>The <see cref="T:System.Threading.CancellationTokenRegistration" /> instance that can be used to unregister the callback.</returns>
```

**成员**：System.Threading.CancellationToken.Equals(System.Threading.CancellationToken)</br>
**签名**：_1164f03605d2c4fa</br>
**注释**：

```xml
<summary>Determines whether the current <see cref="T:System.Threading.CancellationToken" /> instance is equal to the specified token.</summary>
<param name="other">The other <see cref="T:System.Threading.CancellationToken" /> to compare with this instance.</param>
<returns>  <see langword="true" /> if the instances are equal; otherwise, <see langword="false" />. See the Remarks section for more information.</returns>
```

**成员**：override System.Threading.CancellationToken.Equals(object)</br>
**签名**：_1a6a42d621ec0494</br>
**注释**：

```xml
<summary>Determines whether the current <see cref="T:System.Threading.CancellationToken" /> instance is equal to the specified <see cref="T:System.Object" />.</summary>
<param name="other">The other object to compare with this instance.</param>
<exception cref="T:System.ObjectDisposedException">An associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
<returns>  <see langword="true" /> if <paramref name="other" /> is a <see cref="T:System.Threading.CancellationToken" /> and if the two instances are equal; otherwise, <see langword="false" />. See the Remarks section for more information.</returns>
```

**成员**：override System.Threading.CancellationToken.GetHashCode()</br>
**签名**：_35888e21bae24e5c</br>
**注释**：

```xml
<summary>Serves as a hash function for a <see cref="T:System.Threading.CancellationToken" />.</summary>
<returns>A hash code for the current <see cref="T:System.Threading.CancellationToken" /> instance.</returns>
```

**成员**：static System.Threading.CancellationToken.operator ==(System.Threading.CancellationToken, System.Threading.CancellationToken)</br>
**签名**：_20bdabf51c432a6d</br>
**注释**：

```xml
<summary>Determines whether two <see cref="T:System.Threading.CancellationToken" /> instances are equal.</summary>
<param name="left">The first instance.</param>
<param name="right">The second instance.</param>
<exception cref="T:System.ObjectDisposedException">An associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
<returns>  <see langword="true" /> if the instances are equal; otherwise, <see langword="false" /> See the Remarks section for more information.</returns>
```

**成员**：static System.Threading.CancellationToken.operator !=(System.Threading.CancellationToken, System.Threading.CancellationToken)</br>
**签名**：_0b54f5c239fec8ac</br>
**注释**：

```xml
<summary>Determines whether two <see cref="T:System.Threading.CancellationToken" /> instances are not equal.</summary>
<param name="left">The first instance.</param>
<param name="right">The second instance.</param>
<exception cref="T:System.ObjectDisposedException">An associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
<returns>  <see langword="true" /> if the instances are not equal; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Threading.CancellationToken.ThrowIfCancellationRequested()</br>
**签名**：_93a52990613703a6</br>
**注释**：

```xml
<summary>Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</summary>
<exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
```
