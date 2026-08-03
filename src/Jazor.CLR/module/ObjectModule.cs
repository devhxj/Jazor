namespace Jazor.CLR;

/// <summary>
/// System.Object 类型模块映射规则
///
/// C# object 与 JavaScript object 的对应关系：
/// - C# object 是所有类型的基类，JavaScript 也有类似概念
/// - typeof 在 JavaScript 中返回类型字符串
/// - Equals 和 ReferenceEquals 都映射为 === 运算符
///
/// Op 类型选择原则：
/// - Inline: JavaScript 有对应操作符（typeof、===）
/// - Alias: JavaScript 有原生方法（toString）
/// - Allowed: 无操作，保持默认行为（Object 构造函数）
/// - Import: 需要共享运行时状态或保留虚分派（GetHashCode）
///
/// 类型映射：
/// - System.Type → object（JavaScript 无类型系统）
/// - int → Number
/// </summary>
[ECMAScriptModule("System/ObjectModule.js")]
[Jazor(Op.Alias, "object", "Object")]
public static class ObjectModule
{
	/// <summary>
	/// C#: obj.GetType()
	/// JS: typeof obj
	/// 注意：JavaScript 的 typeof 返回类型字符串（如 "object", "string", "number" 等）
	/// </summary>
	[Jazor(Op.Inline, "object.GetType()", "typeof __arg1")]
	public extern static string _393ae40d42f17afb(object instance);

	/// <summary>
	/// C#: new object()
	/// JS: {} 或 new Object()
	/// 无操作，JavaScript 对象直接使用字面量创建
	/// </summary>
	[Jazor(Op.Allowed, "object.Object()")]
	public extern static object _4aea088b73a04a68();

	/// <summary>
	/// C#: obj.ToString()
	/// JS: obj.toString()
	/// JavaScript 所有对象都有 toString 方法
	/// </summary>
	[Jazor(Op.Alias, "virtual object.ToString()", "toString")]
	public extern static string? _b43835974ba92ea0(object instance);

	/// <summary>
	/// C#: obj.Equals(other)
	/// JS: obj === other
	/// 对于 object 类型，Equals 语义是引用相等，与 === 一致
	/// </summary>
	[Jazor(Op.Inline, "virtual object.Equals(object)", "(__arg1 === __arg2)")]
	public extern static bool _bfe118282c0f0f45(object instance, object? obj);

	/// <summary>
	/// C#: Object.Equals(objA, objB)
	/// JS: objA === objB
	/// C# 的 Object.Equals 静态方法对于引用类型使用引用相等
	/// </summary>
	[Jazor(Op.Inline, "static object.Equals(object, object)", "(__arg1 === __arg2)")]
	public extern static bool _cfcace6be1500e0f(object? objA, object? objB);

	/// <summary>
	/// C#: Object.ReferenceEquals(objA, objB)
	/// JS: objA === objB
	/// JavaScript 的 === 对于对象比较的是引用
	/// </summary>
	[Jazor(Op.Inline, "static object.ReferenceEquals(object, object)", "(__arg1 === __arg2)")]
	public extern static bool _b7bcdcecb3f79c07(object? objA, object? objB);

	/// <summary>
	/// Gets a deterministic hash code for the current CLR carrier.
	/// </summary>
	/// <remarks>
	/// A source class can override <c>GetHashCode()</c>; compiled classes expose that override as
	/// <c>getHashCode</c>, so preserve virtual dispatch before falling back to the shared carrier
	/// hash. The fallback is centralized in <see cref="RuntimeModule"/> to keep object and
	/// EqualityComparer hash semantics aligned.
	/// </remarks>
	[Jazor(Op.Import, "virtual object.GetHashCode()")]
	public static Number _97891de43f43ceb4(object instance)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");

		var type = TypeOf(instance);
		if (type == "object" || type == "function")
		{
			var customHashCode = Reflect.Get(instance, "getHashCode");
			if (TypeOf(customHashCode) == "function")
				return (Number)Reflect.Apply(customHashCode!, instance, [])!;
		}

		return RuntimeModule.GetObjectHashCode(instance);
	}
}
