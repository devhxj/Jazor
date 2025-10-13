using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Exception")]
public static class ExceptionModule
{
    [WhiteList("System.Exception.TargetSite.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Reflection.MethodBase? ExceptionGetTargetSite(Error instance);

    ///<summary>Initializes a new instance of the <see cref="T:System.Exception" /> class.</summary>    [WhiteList("System.Exception.Exception()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Error ExceptionNew();

    ///<summary>Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message.</summary>
    ///<param name="message">The message that describes the error.</param>    [WhiteList("System.Exception.Exception(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Error ExceptionNew2(string? message);

    ///<summary>Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
    ///<param name="message">The error message that explains the reason for the exception.</param>
    ///<param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>    [WhiteList("System.Exception.Exception(string, System.Exception)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Error ExceptionNew3(string? message, System.Exception? innerException);

    [WhiteList("virtual System.Exception.Message.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string ExceptionGetMessage(Error instance);

    [WhiteList("virtual System.Exception.Data.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.IDictionary ExceptionGetData(Error instance);

    ///<summary>When overridden in a derived class, returns the <see cref="T:System.Exception" /> that is the root cause of one or more subsequent exceptions.</summary>
    ///<returns>The first exception thrown in a chain of exceptions. If the <see cref="P:System.Exception.InnerException" /> property of the current exception is a null reference (<see langword="Nothing" /> in Visual Basic), this property returns the current exception.</returns>    [WhiteList("virtual System.Exception.GetBaseException()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Exception ExceptionGetBaseException(Error instance);

    [WhiteList("System.Exception.InnerException.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Exception? ExceptionGetInnerException(Error instance);

    [WhiteList("virtual System.Exception.HelpLink.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string? ExceptionGetHelpLink(Error instance);

    [WhiteList("virtual System.Exception.HelpLink.set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ExceptionSetHelpLink(Error instance, string? value);

    [WhiteList("virtual System.Exception.Source.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string? ExceptionGetSource(Error instance);

    [WhiteList("virtual System.Exception.Source.set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ExceptionSetSource(Error instance, string? value);

    ///<summary>When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.</summary>
    ///<param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
    ///<param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
    ///<exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is a null reference (<see langword="Nothing" /> in Visual Basic).</exception>    [WhiteList("virtual System.Exception.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ExceptionGetObjectData(Error instance, System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context);

    ///<summary>Creates and returns a string representation of the current exception.</summary>
    ///<returns>A string representation of the current exception.</returns>    [WhiteList("override System.Exception.ToString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string ExceptionToString(Error instance);

    [WhiteList("System.Exception.HResult.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ExceptionGetHResult(Error instance);

    [WhiteList("System.Exception.HResult.set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ExceptionSetHResult(Error instance, Number value);

    ///<summary>Gets the runtime type of the current instance.</summary>
    ///<returns>A <see cref="T:System.Type" /> object that represents the exact runtime type of the current instance.</returns>    [WhiteList("System.Exception.GetType()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Type ExceptionGetType(Error instance);

    [WhiteList("virtual System.Exception.StackTrace.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string? ExceptionGetStackTrace(Error instance);
}
