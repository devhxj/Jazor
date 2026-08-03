namespace Jazor.CLR;

/// <summary>
/// System.Exception 类型模块映射规则
///
/// C# Exception 与 JavaScript Error 的对应关系：
/// - C# Exception 是所有异常的基类
/// - JavaScript Error 是所有错误的基类
/// - 大部分属性可以直接映射
///
/// Op 类型选择原则：
/// - Inline: 简单属性访问（如 message、stack）
/// - Import: 需要完整实现的构造函数
/// - Discard: 不支持或极少使用的功能
/// </summary>
[ECMAScriptModule("System/ExceptionModule.js")]
[Jazor(Op.Alias, "System.Exception","Error")]
public static class ExceptionModule
{
	// Error has no CLR HelpLink equivalent. Keep metadata external so the Error carrier remains native.
	private static readonly WeakMap<Error, string?> HelpLinks = new();
	// Source is CLR metadata rather than a JavaScript Error field. Keep it per carrier for the
	// same reason as HelpLink: mutating Source must not alter the native Error shape.
	private static readonly WeakMap<Error, string?> Sources = new();

	private static void EnsureInstance(Error instance)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");
	}

	private static Error? GetInnerExceptionCore(Error instance)
	{
		EnsureInstance(instance);
		return instance.Cause as Error;
	}

	[Jazor(Op.Discard ,"System.Exception.TargetSite.get")]
	public extern static System.Reflection.MethodBase? _1645aa9cae2c858e(Error instance);

	/// <summary>
	/// C#: new Exception()
	/// JS: new Error()
	/// </summary>
	[Jazor(Op.Inline, "System.Exception.Exception()", "new Error()")]
	public extern static Error _984704ccb6ce2252();

	/// <summary>
	/// C#: new Exception(string message)
	/// JS: new Error(message)
	/// </summary>
	[Jazor(Op.Inline, "System.Exception.Exception(string)", "new Error(__arg1)")]
	public extern static Error _2cf200c538022157(string? message);

	/// <summary>
	/// C#: new Exception(string message, Exception innerException)
	/// JS: new Error(message, { cause: innerException }) (ES2022+)
	/// Note: JavaScript Error cause is an optional feature
	/// </summary>
	[Jazor(Op.Import, "System.Exception.Exception(string, System.Exception)")]
	public static Error _553ffa41c7b954da(string? message, Error? innerException)
		=> new(message ?? "", new ErrorOptions { Cause = innerException });

	/// <summary>
	/// C#: Exception.Message
	/// JS: error.message
	/// </summary>
	[Jazor(Op.Inline, "virtual System.Exception.Message.get", "__arg1.message")]
	public extern static string _254136af38922fd7(Error instance);

	/// <summary>
	/// C#: ArgumentException.Message
	/// JS: error.message
	/// </summary>
	[Jazor(Op.Inline, "virtual System.ArgumentException.Message.get", "__arg1.message")]
	public extern static string _b3ef5ec5ac6d412a(Error instance);

	[Jazor(Op.Discard ,"virtual System.Exception.Data.get")]
	public extern static System.Collections.IDictionary _72d5829c989f130e(Error instance);

	///<summary>When overridden in a derived class, returns the <see cref="T:System.Exception" /> that is the root cause of one or more subsequent exceptions.</summary>
	[Jazor(Op.Import, "virtual System.Exception.GetBaseException()")]
	public static Error _f062594f9ecd0366(Error instance)
	{
		EnsureInstance(instance);
		var current = instance;
		var inner = GetInnerExceptionCore(current);
		while (inner != null)
		{
			current = inner;
			inner = GetInnerExceptionCore(current);
		}

		return current;
	}

	[Jazor(Op.Import, "System.Exception.InnerException.get")]
	public static Error? _463c6b2780b746af(Error instance)
		=> GetInnerExceptionCore(instance);

	[Jazor(Op.Import, "virtual System.Exception.HelpLink.get")]
	public static string? _cbc65d16d0767d67(Error instance)
	{
		EnsureInstance(instance);
		return HelpLinks.Has(instance) ? HelpLinks.Get(instance) : null;
	}

	[Jazor(Op.Import, "virtual System.Exception.HelpLink.set")]
	public static void _30c969b3bbd3fa2e(Error instance, string? value)
	{
		EnsureInstance(instance);
		HelpLinks.Set(instance, value);
	}

	[Jazor(Op.Import, "virtual System.Exception.Source.get")]
	public static string? _21e71d416a10c806(Error instance)
	{
		EnsureInstance(instance);
		return Sources.Has(instance) ? Sources.Get(instance) : null;
	}

	[Jazor(Op.Import, "virtual System.Exception.Source.set")]
	public static void _48095d5ec6492dcb(Error instance, string? value)
	{
		EnsureInstance(instance);
		Sources.Set(instance, value);
	}

	///<summary>When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.</summary>
	[Jazor(Op.Discard ,"virtual System.Exception.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)")]
	public extern static void _c4f98e62762f67b4(Error instance, object info, object context);

	/// <summary>
	/// C#: Exception.ToString()
	/// JS: error.toString() 或 error.message
	/// </summary>
	[Jazor(Op.Alias, "override System.Exception.ToString()", "toString")]
	public extern static string _d02b6e28875d5f19(Error instance);

	[Jazor(Op.Discard ,"System.Exception.HResult.get")]
	public extern static Number _f59d814fd0e787cd(Error instance);

	[Jazor(Op.Discard ,"System.Exception.HResult.set")]
	public extern static void _9585e24a6bef548d(Error instance, Number value);

	/// <summary>
	/// C#: Exception.GetType()
	/// JS: error.constructor.name 或 error.name
	/// </summary>
	[Jazor(Op.Inline, "System.Exception.GetType()", "__arg1.constructor")]
	public extern static System.Type _352db97ff685dc43(Error instance);

	/// <summary>
	/// C#: Exception.StackTrace
	/// JS: error.stack
	/// </summary>
	[Jazor(Op.Inline, "virtual System.Exception.StackTrace.get", "__arg1.stack")]
	public extern static string? _699c881cddeae353(Error instance);

	/// <summary>
	/// C#: ArgumentException.ParamName
	/// JS: error.message
	/// </summary>
	[Jazor(Op.Inline, "virtual System.ArgumentException.ParamName.get", "__arg1.message")]
	public extern static string? _0dbd1c9e0d1f4e3a(Error instance);

	/// <summary>
	/// C#: new DivideByZeroException()
	/// JS: new Error("DivideByZeroException")
	/// </summary>
	[Jazor(Op.Inline, "System.DivideByZeroException.DivideByZeroException()", "new Error('DivideByZeroException')")]
	public extern static Error _d1f4c6c8e9474d37();

	/// <summary>
	/// C#: new InvalidOperationException()
	/// JS: new Error()
	/// </summary>
	[Jazor(Op.Inline, "System.InvalidOperationException.InvalidOperationException()", "new Error()")]
	public extern static Error _e2850b70fbe24075();

	/// <summary>
	/// C#: new InvalidOperationException(string)
	/// JS: new Error(message)
	/// </summary>
	[Jazor(Op.Inline, "System.InvalidOperationException.InvalidOperationException(string)", "new Error(__arg1)")]
	public extern static Error _5c8e0e76e3ba42db(string? message);

	/// <summary>
	/// C#: new ArgumentNullException(string)
	/// JS: new TypeError(paramName)
	/// </summary>
	[Jazor(Op.Inline, "System.ArgumentNullException.ArgumentNullException(string)", "new TypeError(__arg1)")]
	public extern static TypeError _d6f57ff44fd24ef5(string? paramName);

	/// <summary>
	/// C#: ArgumentNullException.ThrowIfNull(argument, paramName)
	/// JS: throw TypeError when the argument is null.
	/// </summary>
	[Jazor(Op.Import, "static System.ArgumentNullException.ThrowIfNull(object, string)")]
	public static void _c80ae10aa1d0d795(object? argument, string? paramName)
	{
		if (argument == null)
			throw new TypeError(paramName ?? "Value cannot be null.");
	}
}

/// <summary>
/// 衍生异常类型别名映射。
/// </summary>
[Jazor(Op.Alias, "System.InvalidOperationException", "Error")]
public static class InvalidOperationExceptionModule
{
}

/// <summary>
/// ArgumentNullException 映射到 JavaScript TypeError。
/// </summary>
[Jazor(Op.Alias, "System.ArgumentNullException", "TypeError")]
public static class ArgumentNullExceptionModule
{
}

/// <summary>
/// DivideByZeroException 映射到 JavaScript Error。
/// </summary>
[Jazor(Op.Alias, "System.DivideByZeroException", "Error")]
public static class DivideByZeroExceptionModule
{
}
