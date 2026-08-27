namespace Jazor.CLR;

[ECMAScriptModule("Microsoft/AspNetCore/Components/Web/TouchEventArgsModule.js")]
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Web.TouchEventArgs", "TouchEvent")]
public static class TouchEventArgsModule
{
	// TouchEventArgs is forwarded to the native TouchEvent. TouchList is an
	// indexed, length-bearing browser object and uses the existing array carrier
	// for the erased IReadOnlyList<TouchPoint> contract.
	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.Detail.get", "__arg1.detail")]
	public extern static Number _22d06332ca97dd80(TouchEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.Detail.set")]
	public extern static void _73af4d99a1390cd5(TouchEvent instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.Touches.get", "Array.from(__arg1.touches)")]
	public extern static Array<Touch> _b89afa79297c7788(TouchEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.Touches.set")]
	public extern static void _aec49122c24cc311(TouchEvent instance, Array<Touch> value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.TargetTouches.get", "Array.from(__arg1.targetTouches)")]
	public extern static Array<Touch> _c372677b70d87718(TouchEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.TargetTouches.set")]
	public extern static void _1d817a39d35ba08f(TouchEvent instance, Array<Touch> value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.ChangedTouches.get", "Array.from(__arg1.changedTouches)")]
	public extern static Array<Touch> _5698743dda179b29(TouchEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.ChangedTouches.set")]
	public extern static void _52d836c47c7e715c(TouchEvent instance, Array<Touch> value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.CtrlKey.get", "__arg1.ctrlKey")]
	public extern static bool _5cc3300e783ed848(TouchEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.CtrlKey.set")]
	public extern static void _6339e74980803542(TouchEvent instance, bool value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.ShiftKey.get", "__arg1.shiftKey")]
	public extern static bool _b7966b7e18021d0b(TouchEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.ShiftKey.set")]
	public extern static void _d3e11f6fb6a96f7f(TouchEvent instance, bool value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.AltKey.get", "__arg1.altKey")]
	public extern static bool _7416087ed4e5288f(TouchEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.AltKey.set")]
	public extern static void _bedf50d23f447276(TouchEvent instance, bool value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.MetaKey.get", "__arg1.metaKey")]
	public extern static bool _6f19b0002a5bec79(TouchEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.MetaKey.set")]
	public extern static void _f85f85937e8261eb(TouchEvent instance, bool value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.Type.get", "__arg1.type")]
	public extern static string _3c90f23fa865f302(TouchEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.Type.set")]
	public extern static void _f8d1e94023cfc511(TouchEvent instance, string value);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchEventArgs.TouchEventArgs()")]
	public extern static TouchEvent _3c061308af499d53();
}
