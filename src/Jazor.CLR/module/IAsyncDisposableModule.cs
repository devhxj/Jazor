namespace Jazor.CLR;

/// <summary>
/// System.IAsyncDisposable 模块映射规则
///
/// 当前仅开放接口释放调度入口；await using lowering 由 compiler 侧单独决定是否启用。
/// </summary>
[ECMAScriptModule("System/IAsyncDisposableModule.js")]
[Jazor(Op.Alias, "System.IAsyncDisposable", "Object")]
public static class IAsyncDisposableModule
{
	internal static void EnsureDisposableInstance(object instance)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");
	}

	/// <summary>
	/// C#: disposable.DisposeAsync()
	/// JS: 运行时优先调用实例的 disposeAsync 方法；没有该方法则返回已完成 Promise。
	/// </summary>
	[Jazor(Op.Import, "System.IAsyncDisposable.DisposeAsync()")]
	public static object _d17f7fbf9eb14eef(object instance)
	{
		EnsureDisposableInstance(instance);

		if (Reflect.Has(instance, "disposeAsync"))
		{
			var disposeAsync = Reflect.Get(instance, "disposeAsync");
			var result = Reflect.Apply(disposeAsync!, instance, []);
			return Promise.Resolve(result);
		}

		return Promise.Resolve();
	}
}
