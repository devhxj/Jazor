namespace Jazor.CLR;

[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Web.MouseEventArgs", "MouseEvent")]
public static class MouseEventArgsModule
{
	// MouseEvent fields are native WebIDL long values and stay on the Number carrier.
	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.Detail.get", "__arg1.detail")]
	public extern static Number _5be2fbd2d2769159(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.Detail.set")]
	public extern static void _4ba47fe259efe30c(MouseEvent instance, Number value);

	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.ScreenX.get", "__arg1.screenX")]
	public extern static Number _452c2c07ed3da850(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.ScreenX.set")]
	public extern static void _149a9094c0e30050(MouseEvent instance, Number value);

	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.ScreenY.get", "__arg1.screenY")]
	public extern static Number _9ae6827f6703851e(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.ScreenY.set")]
	public extern static void _be242fa8bc7f0d03(MouseEvent instance, Number value);

	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.ClientX.get", "__arg1.clientX")]
	public extern static Number _91196032e2c6d388(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.ClientX.set")]
	public extern static void _399750c057a1b992(MouseEvent instance, Number value);

	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.ClientY.get", "__arg1.clientY")]
	public extern static Number _713b9b0fd6680a73(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.ClientY.set")]
	public extern static void _6c44f815c56466c9(MouseEvent instance, Number value);

	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.OffsetX.get", "__arg1.offsetX")]
	public extern static Number _fc35f6015c690edf(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.OffsetX.set")]
	public extern static void _23f6ddfc81949cfc(MouseEvent instance, Number value);

	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.OffsetY.get", "__arg1.offsetY")]
	public extern static Number _bbbbcb0edd93db22(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.OffsetY.set")]
	public extern static void _8ba2e77ca8dae73c(MouseEvent instance, Number value);

	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.PageX.get", "__arg1.pageX")]
	public extern static Number _fd29a7788fef40f3(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.PageX.set")]
	public extern static void _b9093248d662b54b(MouseEvent instance, Number value);

	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.PageY.get", "__arg1.pageY")]
	public extern static Number _39546249a98e9471(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.PageY.set")]
	public extern static void _2c2104f5023e16a7(MouseEvent instance, Number value);

	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.MovementX.get", "__arg1.movementX")]
	public extern static Number _bb271a0f1a6cf1a4(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.MovementX.set")]
	public extern static void _668f18f95a499940(MouseEvent instance, Number value);

	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.MovementY.get", "__arg1.movementY")]
	public extern static Number _e7704ed7409ce4fa(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.MovementY.set")]
	public extern static void _ac0a40d9782de2f5(MouseEvent instance, Number value);

	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.Button.get", "__arg1.button")]
	public extern static Number _12796ef3ef6c6179(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.Button.set")]
	public extern static void _b61988ba001b3237(MouseEvent instance, Number value);

	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.Buttons.get", "__arg1.buttons")]
	public extern static Number _ed6ae4720c495606(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.Buttons.set")]
	public extern static void _f8e06b0f0073336f(MouseEvent instance, Number value);

	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.CtrlKey.get", "__arg1.ctrlKey")]
	public extern static bool _0d77e34b81e04ff6(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.CtrlKey.set")]
	public extern static void _3acbf83245c5c99e(MouseEvent instance, bool value);

	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.ShiftKey.get", "__arg1.shiftKey")]
	public extern static bool _2d0787df3d22b947(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.ShiftKey.set")]
	public extern static void _e6a6ef0dd45c9538(MouseEvent instance, bool value);

	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.AltKey.get", "__arg1.altKey")]
	public extern static bool _79385d39a9d44c17(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.AltKey.set")]
	public extern static void _8a4445913ebc4220(MouseEvent instance, bool value);

	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.MetaKey.get", "__arg1.metaKey")]
	public extern static bool _59a375715b9021f2(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.MetaKey.set")]
	public extern static void _824d247e3c86bb62(MouseEvent instance, bool value);

	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.Type.get", "__arg1.type")]
	public extern static string _ba8b345dcf635e9b(MouseEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.Type.set")]
	public extern static void _3bf480a9c40e6236(MouseEvent instance, string value);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.MouseEventArgs.MouseEventArgs()")]
	public extern static MouseEvent _8a5fa4630061ed3f();
}
