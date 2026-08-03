namespace Jazor.CLR.Test;

internal static class ClrRuntimeExceptionScenarios
{
    private const string ModulePath = "System/ExceptionModule.js";
    private const string ConstructorMember = "System.Exception.Exception(string, System.Exception)";
    private const string InnerExceptionMember = "System.Exception.InnerException.get";
    private const string GetBaseExceptionMember = "virtual System.Exception.GetBaseException()";
    private const string HelpLinkGetMember = "virtual System.Exception.HelpLink.get";
    private const string HelpLinkSetMember = "virtual System.Exception.HelpLink.set";
    private const string SourceGetMember = "virtual System.Exception.Source.get";
    private const string SourceSetMember = "virtual System.Exception.Source.set";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "exception.cause-constructor.preserves-message-and-inner-shape",
            ConstructorMember,
            [Text("outer"), Error("inner")],
            Error("outer", Error("inner"))),
        Success(
            "exception.inner-exception.returns-direct-cause",
            InnerExceptionMember,
            [Exception("outer", Exception("inner"))],
            Error("inner")),
        Success(
            "exception.inner-exception.without-cause-is-null",
            InnerExceptionMember,
            [Exception("plain")],
            Null()),
        Success(
            "exception.get-base-exception.traverses-root-cause",
            GetBaseExceptionMember,
            [Exception("outer", Exception("middle", Exception("leaf")))],
            Error("leaf")),
        Success(
            "exception.help-link.defaults-null",
            HelpLinkGetMember,
            [Error("metadata")],
            Null()),
        Success(
            "exception.help-link.setter-completes",
            HelpLinkSetMember,
            [Error("metadata"), Text("https://example.test/help")],
            Undefined()),
        Success(
            "exception.source.defaults-null",
            SourceGetMember,
            [Error("metadata")],
            Null()),
        Success(
            "exception.source.preserves-per-error-metadata",
            SourceGetMember,
            [
                Reference("source-error", Error("metadata")),
                Invoke(SourceSetMember, Reference("source-error", Error("ignored")), Text("Jazor.App"))
            ],
            Text("Jazor.App")),
        Failure(
            "exception.inner-exception.rejects-null-receiver",
            InnerExceptionMember,
            [Null()],
            "NullReferenceException"),
        Failure(
            "exception.get-base-exception.rejects-null-receiver",
            GetBaseExceptionMember,
            [Null()],
            "NullReferenceException")
    ];

    private static ClrRuntimeValue Exception(string message, ClrRuntimeValue? cause = null)
        => Invoke(ConstructorMember, Text(message), cause ?? Null());

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        string error)
        => new(id, member, ModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments)
        => ClrRuntimeValue.Invoke(member, arguments);

    private static ClrRuntimeValue Reference(string id, ClrRuntimeValue value)
        => ClrRuntimeValue.Reference(id, value);

    private static ClrRuntimeValue Error(string message, ClrRuntimeValue? cause = null)
        => ClrRuntimeValue.Error(message, cause);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Undefined() => ClrRuntimeValue.Undefined();
}
