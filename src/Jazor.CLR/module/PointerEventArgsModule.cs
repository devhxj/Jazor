namespace Jazor.CLR;

[ECMAScriptModule("Microsoft/AspNetCore/Components/Web/PointerEventArgsModule.js")]
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Web.PointerEventArgs", "PointerEvent")]
public static class PointerEventArgsModule
{
	// PointerEventArgs is a read-only view over the native PointerEvent carrier.
	// WebIDL long/int values stay on the browser Number carrier; no CLR object is materialized.
	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.PointerEventArgs.PointerId.get", "__arg1.pointerId")]
	public extern static Number _7f278ce4fcbb268e(PointerEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.PointerEventArgs.PointerId.set")]
	public extern static void _712e8a66478b60a6(PointerEvent instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.PointerEventArgs.Width.get", "__arg1.width")]
	public extern static Number _9844b865b0810a88(PointerEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.PointerEventArgs.Width.set")]
	public extern static void _e666c66e87bfc445(PointerEvent instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.PointerEventArgs.Height.get", "__arg1.height")]
	public extern static Number _f2d219e4216ec87e(PointerEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.PointerEventArgs.Height.set")]
	public extern static void _d33807bea7d26e78(PointerEvent instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.PointerEventArgs.Pressure.get", "__arg1.pressure")]
	public extern static Number _e45b62cddb067b06(PointerEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.PointerEventArgs.Pressure.set")]
	public extern static void _aaee27296aa5a4b2(PointerEvent instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.PointerEventArgs.TiltX.get", "__arg1.tiltX")]
	public extern static Number _4686499cc911f913(PointerEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.PointerEventArgs.TiltX.set")]
	public extern static void _0850770c1e071282(PointerEvent instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.PointerEventArgs.TiltY.get", "__arg1.tiltY")]
	public extern static Number _cb73aec9e496833b(PointerEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.PointerEventArgs.TiltY.set")]
	public extern static void _13ca33c1b48fe548(PointerEvent instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.PointerEventArgs.PointerType.get", "__arg1.pointerType")]
	public extern static string _7279f944dfce4bb7(PointerEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.PointerEventArgs.PointerType.set")]
	public extern static void _a7eed857be633dfd(PointerEvent instance, string value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.PointerEventArgs.IsPrimary.get", "__arg1.isPrimary")]
	public extern static bool _62322c2e23723f11(PointerEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.PointerEventArgs.IsPrimary.set")]
	public extern static void _92c2b75562fc3562(PointerEvent instance, bool value);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.PointerEventArgs.PointerEventArgs()")]
	public extern static PointerEvent _584cec81e10e13be();
}
