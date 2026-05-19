namespace Jazor.CLR;

/// <summary>
/// System.IDisposable 模块映射规则
///
/// 目标支持面：
/// - IDisposable.Dispose()
/// </summary>
[ECMAScriptModule("System/IDisposableModule.js")]
[Jazor(Op.Alias, "System.IDisposable", "Object")]
public static class IDisposableModule
{
	internal static void EnsureDisposableInstance(object instance)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");
	}

	/// <summary>
	/// C#: disposable.Dispose()
	/// JS: 运行时优先调用实例的 dispose 方法；没有该方法则视为 no-op。
	/// 说明：using lowering 只会在编译时已满足 IDisposable 约束的资源上发射此调用，
	/// 因此这里不再引入额外 runtime 类型筛选，只保留空接收者语义和稳定方法探测。
	/// </summary>
	[Jazor(Op.Import, "System.IDisposable.Dispose()")]
	public static void _6f97d94b6f2e4bc1(object instance)
	{
		EnsureDisposableInstance(instance);

		if (Reflect.Has(instance, "dispose"))
		{
			var dispose = Reflect.Get(instance, "dispose");
			Reflect.Apply(dispose!, instance, []);
		}
	}
}
