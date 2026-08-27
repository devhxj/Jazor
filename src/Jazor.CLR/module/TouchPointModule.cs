namespace Jazor.CLR;

[ECMAScriptModule("Microsoft/AspNetCore/Components/Web/TouchPointModule.js")]
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Web.TouchPoint", "Touch")]
public static class TouchPointModule
{
	// WebIDL int/double fields are JavaScript Number values, including the
	// CLR long Identifier used by the Blazor event DTO.
	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.TouchPoint.Identifier.get", "__arg1.identifier")]
	public extern static Number _efcde590e86d50b2(Touch instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchPoint.Identifier.set")]
	public extern static void _9ac538aa29d65a06(Touch instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.TouchPoint.ScreenX.get", "__arg1.screenX")]
	public extern static Number _e09e48b761a4d560(Touch instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchPoint.ScreenX.set")]
	public extern static void _2c3c8fdf00ff216a(Touch instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.TouchPoint.ScreenY.get", "__arg1.screenY")]
	public extern static Number _40fdd906a1f08b89(Touch instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchPoint.ScreenY.set")]
	public extern static void _38ccdae12eece287(Touch instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.TouchPoint.ClientX.get", "__arg1.clientX")]
	public extern static Number _2607a73ed54f5ee7(Touch instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchPoint.ClientX.set")]
	public extern static void _2bc329c954c622e0(Touch instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.TouchPoint.ClientY.get", "__arg1.clientY")]
	public extern static Number _28312d3bbf2cce6d(Touch instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchPoint.ClientY.set")]
	public extern static void _d3c1e38ef623c450(Touch instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.TouchPoint.PageX.get", "__arg1.pageX")]
	public extern static Number _9d6d7b2814d17345(Touch instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchPoint.PageX.set")]
	public extern static void _07b968c3c466a9e8(Touch instance, Number value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.TouchPoint.PageY.get", "__arg1.pageY")]
	public extern static Number _058b7e7531165442(Touch instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchPoint.PageY.set")]
	public extern static void _a04dd4ec73187f57(Touch instance, Number value);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.TouchPoint.TouchPoint()")]
	public extern static Touch _cf4570dba5df1d70();
}
