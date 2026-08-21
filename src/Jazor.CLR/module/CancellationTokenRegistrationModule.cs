namespace Jazor.CLR;

/// <summary>
/// System.Threading.CancellationTokenRegistration 映射到 <see cref="RuntimeModule.JCancellationTokenRegistration"/> carrier。
/// </summary>
/// <remarks>
/// registration 在 CLR 下是"如何撤下这个回调"的凭据，浏览器没有对等类型，因此擦除为一个只保存
/// (signal, listener) 的内部 carrier。撤销语义集中在 <see cref="RuntimeModule.UnregisterCancellationCallback"/>：
/// <c>Unregister</c> 返回是否真的撤下了一个尚未执行的回调，<c>Dispose</c> 是它的忽略返回值版本。
/// <para>
/// 身份按引用比较：同一个 <c>Register</c> 调用得到同一个 carrier 对象，因此 <c>==</c> / <c>Equals</c>
/// 直接落成 <c>===</c>；carrier 没有稳定数值身份，<c>GetHashCode</c> 保持 unsupported 以免破坏
/// Equals/GetHashCode 一致性。
/// </para>
/// <para>
/// 无参构造（<c>default(CancellationTokenRegistration)</c>）保持 unsupported：它表示一个空注册，
/// 会把 carrier 的 signal 变成可空，从而给整条撤销路径引入一个仅为占位值存在的分支。
/// 常用路径是 <c>Register</c> 返回的真实注册，等有明确需求时再支持。
/// </para>
/// </remarks>
[ECMAScriptModule("System/Threading/CancellationTokenRegistrationModule.js")]
[Jazor(Op.Alias, "System.Threading.CancellationTokenRegistration", "Object")]
public static class CancellationTokenRegistrationModule
{
	[Jazor(Op.Discard, "System.Threading.CancellationTokenRegistration.CancellationTokenRegistration()")]
	public extern static RuntimeModule.JCancellationTokenRegistration _956101a413714c9c();

	///<summary>Disposes of the registration and unregisters the target callback from the associated <see cref="T:System.Threading.CancellationToken" />.</summary>
	// Dispose 与 Unregister 在 JS 下完全同路：CLR 的差别只是 Dispose 还要等待并发执行中的回调，
	// 单线程事件循环里不存在那个窗口。
	[Jazor(Op.Import, "System.Threading.CancellationTokenRegistration.Dispose()", "dispose")]
	public static void _ddfca4a87505c8d8(RuntimeModule.JCancellationTokenRegistration instance)
		=> RuntimeModule.UnregisterCancellationCallback(instance);

	///<summary>Disposes of the registration and unregisters the target callback from the associated             <see cref="T:System.Threading.CancellationToken" />.</summary>
	[Jazor(Op.Import, "System.Threading.CancellationTokenRegistration.DisposeAsync()", "disposeAsync")]
	public static Promise _5ab177c632de03e2(RuntimeModule.JCancellationTokenRegistration instance)
	{
		RuntimeModule.UnregisterCancellationCallback(instance);
		// Promise.Resolve() 的宿主签名是 IPromise，而 ValueTask 的 carrier 是 Promise；
		// 这里的引用转换在发射时被擦除，不产生运行时检查。
		return (Promise)Promise.Resolve();
	}

	// carrier 的 signal 就是注册时的那个 token。
	[Jazor(Op.Alias, "System.Threading.CancellationTokenRegistration.Token.get", "signal")]
	public extern static AbortSignal _3eb82b1d809eca7f(RuntimeModule.JCancellationTokenRegistration instance);

	///<summary>Disposes of the registration and unregisters the target callback from the associated             <see cref="T:System.Threading.CancellationToken" />.</summary>
	[Jazor(Op.Import, "System.Threading.CancellationTokenRegistration.Unregister()", "unregister")]
	public static bool _3f92a31b30a1bf31(RuntimeModule.JCancellationTokenRegistration instance)
		=> RuntimeModule.UnregisterCancellationCallback(instance);

	///<summary>Determines whether two <see cref="T:System.Threading.CancellationTokenRegistration" /> instances are equal.</summary>
	// 默认 lowering 就是 === / !==，与下面的 Equals 同一套 carrier 引用身份规则。
	[Jazor(Op.Allowed, "static System.Threading.CancellationTokenRegistration.operator ==(System.Threading.CancellationTokenRegistration, System.Threading.CancellationTokenRegistration)")]
	public extern static bool _acc1375b1abd6520(RuntimeModule.JCancellationTokenRegistration left, RuntimeModule.JCancellationTokenRegistration right);

	///<summary>Determines whether two <see cref="T:System.Threading.CancellationTokenRegistration" /> instances are not equal.</summary>
	[Jazor(Op.Allowed, "static System.Threading.CancellationTokenRegistration.operator !=(System.Threading.CancellationTokenRegistration, System.Threading.CancellationTokenRegistration)")]
	public extern static bool _5cfb509ee9a8aab9(RuntimeModule.JCancellationTokenRegistration left, RuntimeModule.JCancellationTokenRegistration right);

	///<summary>Determines whether the current <see cref="T:System.Threading.CancellationTokenRegistration" /> instance is equal to the specified <see cref="T:System.Threading.CancellationTokenRegistration" />.</summary>
	[Jazor(Op.Inline, "override System.Threading.CancellationTokenRegistration.Equals(object)", "__arg1 === __arg2")]
	public extern static bool _6d73eb424acc37d5(RuntimeModule.JCancellationTokenRegistration instance, object? obj);

	///<summary>Determines whether the current <see cref="T:System.Threading.CancellationTokenRegistration" /> instance is equal to the specified <see cref="T:System.Threading.CancellationTokenRegistration" />.</summary>
	[Jazor(Op.Inline, "System.Threading.CancellationTokenRegistration.Equals(System.Threading.CancellationTokenRegistration)", "__arg1 === __arg2")]
	public extern static bool _330c3c06bd34b9e4(RuntimeModule.JCancellationTokenRegistration instance, RuntimeModule.JCancellationTokenRegistration other);

	///<summary>Serves as a hash function for a <see cref="T:System.Threading.CancellationTokenRegistration" />.</summary>
	// carrier 没有稳定的数值身份，发射任何近似值都会破坏 Equals/GetHashCode 一致性。
	[Jazor(Op.Discard, "override System.Threading.CancellationTokenRegistration.GetHashCode()")]
	public extern static Number _9fb481fab54c6699(RuntimeModule.JCancellationTokenRegistration instance);
}
