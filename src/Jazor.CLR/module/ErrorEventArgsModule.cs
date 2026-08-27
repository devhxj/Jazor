namespace Jazor.CLR;

[ECMAScriptModule("Microsoft/AspNetCore/Components/Web/ErrorEventArgsModule.js")]
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Web.ErrorEventArgs", "ErrorEvent")]
public static class ErrorEventArgsModule
{
	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Message.get", "__arg1.message")]
	public extern static string? _f43b039c45cf19d1(ErrorEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Message.set")]
	public extern static void _3a817e94648fc562(ErrorEvent instance, string? value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Filename.get", "__arg1.filename")]
	public extern static string? _06868d945c4037cf(ErrorEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Filename.set")]
	public extern static void _d794873ee2ce1ab3(ErrorEvent instance, string? value);

	// WebIDL unsigned long values are Number carriers, not CLR BigInt values.
	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Lineno.get", "__arg1.lineno")]
	public extern static Number _56ff0806567b8858(ErrorEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Lineno.set")]
	public extern static void _911288992dd8f1c7(ErrorEvent instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Colno.get", "__arg1.colno")]
	public extern static Number _b862ee9431a7866b(ErrorEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Colno.set")]
	public extern static void _dbfaed6d0859b0f1(ErrorEvent instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Type.get", "__arg1.type")]
	public extern static string? _286976c96b6c44cf(ErrorEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Type.set")]
	public extern static void _bc6e147c2f8039f1(ErrorEvent instance, string? value);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.ErrorEventArgs()")]
	public extern static ErrorEvent _45c9d400f0c619a0();
}
