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
/// - Replace: JavaScript 有原生方法（toString）
/// - Allowed: 无操作，保持默认行为（Object 构造函数）
/// - Discard: JavaScript 无对应概念（GetHashCode、Type 类型）
///
/// 类型映射：
/// - System.Type → object（JavaScript 无类型系统）
/// - int → Number
/// </summary>
[ECMAScriptModule]
[Jazor(Op.Import, "object", "System/ObjectModule.js")]
public static class ObjectModule
{
	/// <summary>
	/// C#: obj.GetType()
	/// JS: typeof obj
	/// 注意：JavaScript 的 typeof 返回类型字符串（如 "object", "string", "number" 等）
	/// </summary>
	[Jazor(Op.Inline, "object.GetType()", "typeof @#{0}")]
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
	[Jazor(Op.Replace, "virtual object.ToString()", "toString")]
	public extern static string? _b43835974ba92ea0(object instance);

	/// <summary>
	/// C#: obj.Equals(other)
	/// JS: obj === other
	/// 对于 object 类型，Equals 语义是引用相等，与 === 一致
	/// </summary>
	[Jazor(Op.Inline, "virtual object.Equals(object)", "(@#{0} === @#{1})")]
	public extern static bool _bfe118282c0f0f45(object instance, object? obj);

	/// <summary>
	/// C#: Object.Equals(objA, objB)
	/// JS: objA === objB
	/// C# 的 Object.Equals 静态方法对于引用类型使用引用相等
	/// </summary>
	[Jazor(Op.Inline, "static object.Equals(object, object)", "(@#{0} === @#{1})")]
	public extern static bool _cfcace6be1500e0f(object? objA, object? objB);

	/// <summary>
	/// C#: Object.ReferenceEquals(objA, objB)
	/// JS: objA === objB
	/// JavaScript 的 === 对于对象比较的是引用
	/// </summary>
	[Jazor(Op.Inline, "static object.ReferenceEquals(object, object)", "(@#{0} === @#{1})")]
	public extern static bool _b7bcdcecb3f79c07(object? objA, object? objB);

	/// <summary>
	/// JavaScript 没有统一的 GetHashCode 机制
	/// Map 和 Set 使用引用相等或值相等，不需要哈希码
	/// </summary>
	[Jazor(Op.Discard, "virtual object.GetHashCode()")]
	public extern static Number _97891de43f43ceb4(object instance);
}