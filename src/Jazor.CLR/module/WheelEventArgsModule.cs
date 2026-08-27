namespace Jazor.CLR;

[ECMAScriptModule("Microsoft/AspNetCore/Components/Web/WheelEventArgsModule.js")]
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Web.WheelEventArgs", "WheelEvent")]
public static class WheelEventArgsModule
{
	// WheelEventArgs extends MouseEventArgs. Its own numeric fields are native DOM numbers,
	// including DeltaMode (WebIDL unsigned long), even though the CLR API uses long there.
	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaX.get", "__arg1.deltaX")]
	public extern static Number _bb4bf13fa2471865(WheelEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaX.set")]
	public extern static void _2d26d519b8973e31(WheelEvent instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaY.get", "__arg1.deltaY")]
	public extern static Number _edf849af8be77808(WheelEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaY.set")]
	public extern static void _e03f6215c531ac16(WheelEvent instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaZ.get", "__arg1.deltaZ")]
	public extern static Number _d35d5b1a1d000ecf(WheelEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaZ.set")]
	public extern static void _b862d942858ae8f0(WheelEvent instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaMode.get", "__arg1.deltaMode")]
	public extern static Number _67505dfb611e0d25(WheelEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaMode.set")]
	public extern static void _09aa9c2167a33f05(WheelEvent instance, Number value);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.WheelEventArgs.WheelEventArgs()")]
	public extern static WheelEvent _87d5ef96a76ccc3c();
}
