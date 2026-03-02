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
[ECMAScriptModule]
[Jazor(Op.Replace, "System.Exception","Error")]
public static class ExceptionModule
{
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
	[Jazor(Op.Inline, "System.Exception.Exception(string)", "new Error(@#{0})")]
	public extern static Error _2cf200c538022157(string? message);

	/// <summary>
	/// C#: new Exception(string message, Exception innerException)
	/// JS: new Error(message, { cause: innerException }) (ES2022+)
	/// Note: JavaScript Error cause is an optional feature
	/// </summary>
	[Jazor(Op.Discard ,"System.Exception.Exception(string, System.Exception)")]
	public extern static Error _553ffa41c7b954da(string? message, Error? innerException);

	/// <summary>
	/// C#: Exception.Message
	/// JS: error.message
	/// </summary>
	[Jazor(Op.Inline, "virtual System.Exception.Message.get", "@#{0}.message")]
	public extern static string _254136af38922fd7(Error instance);

	[Jazor(Op.Discard ,"virtual System.Exception.Data.get")]
	public extern static System.Collections.IDictionary _72d5829c989f130e(Error instance);

	///<summary>When overridden in a derived class, returns the <see cref="T:System.Exception" /> that is the root cause of one or more subsequent exceptions.</summary>
	[Jazor(Op.Discard ,"virtual System.Exception.GetBaseException()")]
	public extern static System.Exception _f062594f9ecd0366(Error instance);

	[Jazor(Op.Discard ,"System.Exception.InnerException.get")]
	public extern static System.Exception? _463c6b2780b746af(Error instance);

	[Jazor(Op.Discard ,"virtual System.Exception.HelpLink.get")]
	public extern static string? _cbc65d16d0767d67(Error instance);

	[Jazor(Op.Discard ,"virtual System.Exception.HelpLink.set")]
	public extern static void _30c969b3bbd3fa2e(Error instance, string? value);

	[Jazor(Op.Discard ,"virtual System.Exception.Source.get")]
	public extern static string? _21e71d416a10c806(Error instance);

	[Jazor(Op.Discard ,"virtual System.Exception.Source.set")]
	public extern static void _48095d5ec6492dcb(Error instance, string? value);

	///<summary>When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.</summary>
	[Jazor(Op.Discard ,"virtual System.Exception.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)")]
	public extern static void _c4f98e62762f67b4(Error instance, object info, object context);

	/// <summary>
	/// C#: Exception.ToString()
	/// JS: error.toString() 或 error.message
	/// </summary>
	[Jazor(Op.Replace, "override System.Exception.ToString()", "toString")]
	public extern static string _d02b6e28875d5f19(Error instance);

	[Jazor(Op.Discard ,"System.Exception.HResult.get")]
	public extern static Number _f59d814fd0e787cd(Error instance);

	[Jazor(Op.Discard ,"System.Exception.HResult.set")]
	public extern static void _9585e24a6bef548d(Error instance, Number value);

	/// <summary>
	/// C#: Exception.GetType()
	/// JS: error.constructor.name 或 error.name
	/// </summary>
	[Jazor(Op.Inline, "System.Exception.GetType()", "@#{0}.constructor")]
	public extern static System.Type _352db97ff685dc43(Error instance);

	/// <summary>
	/// C#: Exception.StackTrace
	/// JS: error.stack
	/// </summary>
	[Jazor(Op.Inline, "virtual System.Exception.StackTrace.get", "@#{0}.stack")]
	public extern static string? _699c881cddeae353(Error instance);
}
