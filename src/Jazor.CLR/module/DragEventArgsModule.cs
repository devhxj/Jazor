namespace Jazor.CLR;

[ECMAScriptModule("Microsoft/AspNetCore/Components/Web/DragEventArgsModule.js")]
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Web.DragEventArgs", "DragEvent")]
public static class DragEventArgsModule
{
	// DragEventArgs is the browser drag event itself. The nullable browser
	// dataTransfer member remains an opaque native DataTransfer carrier.
	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.DragEventArgs.DataTransfer.get", "__arg1.dataTransfer")]
	public extern static DataTransfer _63d31459412028c3(DragEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.DragEventArgs.DataTransfer.set")]
	public extern static void _0e66833436878f72(DragEvent instance, DataTransfer value);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.DragEventArgs.DragEventArgs()")]
	public extern static DragEvent _e8f526710e676bdb();
}
