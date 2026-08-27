namespace Jazor.CLR;

[ECMAScriptModule("Microsoft/AspNetCore/Components/Web/ClipboardEventArgsModule.js")]
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Web.ClipboardEventArgs", "ClipboardEvent")]
public static class ClipboardEventArgsModule
{
	// The framework event argument exposes the native event type. Clipboard
	// payload access remains outside this CLR DTO slice and must use typed WebIDL.
	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.ClipboardEventArgs.Type.get", "__arg1.type")]
	public extern static string _fb72b7b890c36924(ClipboardEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.ClipboardEventArgs.Type.set")]
	public extern static void _90025c0225e61bd6(ClipboardEvent instance, string value);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.ClipboardEventArgs.ClipboardEventArgs()")]
	public extern static ClipboardEvent _7d238a713c8bf970();
}
