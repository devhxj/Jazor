using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Exception", WhiteListOp.Allowed, null,"System/ExceptionModule.js")]
public static class ExceptionModule
{
	[WhiteList("System.Exception.TargetSite.get", WhiteListOp.Discard)]
	public extern static System.Reflection.MethodBase? _1645aa9cae2c858e(Error instance);

	///<summary>Initializes a new instance of the <see cref="T:System.Exception" /> class.</summary>
	[WhiteList("System.Exception.Exception()", WhiteListOp.Discard)]
	public extern static Error _984704ccb6ce2252();

	///<summary>Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message.</summary>
	[WhiteList("System.Exception.Exception(string)", WhiteListOp.Discard)]
	public extern static Error _2cf200c538022157(object message);

	///<summary>Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	[WhiteList("System.Exception.Exception(string, System.Exception)", WhiteListOp.Discard)]
	public extern static Error _553ffa41c7b954da(object message, object innerException);

	[WhiteList("virtual System.Exception.Message.get", WhiteListOp.Discard)]
	public extern static string _254136af38922fd7(Error instance);

	[WhiteList("virtual System.Exception.Data.get", WhiteListOp.Discard)]
	public extern static System.Collections.IDictionary _72d5829c989f130e(Error instance);

	///<summary>When overridden in a derived class, returns the <see cref="T:System.Exception" /> that is the root cause of one or more subsequent exceptions.</summary>
	[WhiteList("virtual System.Exception.GetBaseException()", WhiteListOp.Discard)]
	public extern static System.Exception _f062594f9ecd0366(Error instance);

	[WhiteList("System.Exception.InnerException.get", WhiteListOp.Discard)]
	public extern static System.Exception? _463c6b2780b746af(Error instance);

	[WhiteList("virtual System.Exception.HelpLink.get", WhiteListOp.Discard)]
	public extern static string? _cbc65d16d0767d67(Error instance);

	[WhiteList("virtual System.Exception.HelpLink.set", WhiteListOp.Discard)]
	public extern static void _30c969b3bbd3fa2e(Error instance, object value);

	[WhiteList("virtual System.Exception.Source.get", WhiteListOp.Discard)]
	public extern static string? _21e71d416a10c806(Error instance);

	[WhiteList("virtual System.Exception.Source.set", WhiteListOp.Discard)]
	public extern static void _48095d5ec6492dcb(Error instance, object value);

	///<summary>When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.</summary>
	[WhiteList("virtual System.Exception.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)", WhiteListOp.Discard)]
	public extern static void _c4f98e62762f67b4(Error instance, object info, object context);

	///<summary>Creates and returns a string representation of the current exception.</summary>
	[WhiteList("override System.Exception.ToString()", WhiteListOp.Discard)]
	public extern static string _d02b6e28875d5f19(Error instance);

	[WhiteList("System.Exception.HResult.get", WhiteListOp.Discard)]
	public extern static Number _f59d814fd0e787cd(Error instance);

	[WhiteList("System.Exception.HResult.set", WhiteListOp.Discard)]
	public extern static void _9585e24a6bef548d(Error instance, Number value);

	///<summary>Gets the runtime type of the current instance.</summary>
	[WhiteList("System.Exception.GetType()", WhiteListOp.Discard)]
	public extern static System.Type _352db97ff685dc43(Error instance);

	[WhiteList("virtual System.Exception.StackTrace.get", WhiteListOp.Discard)]
	public extern static string? _699c881cddeae353(Error instance);
}
