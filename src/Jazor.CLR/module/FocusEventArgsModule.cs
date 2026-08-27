namespace Jazor.CLR;

[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Web.FocusEventArgs", "FocusEvent")]
public static class FocusEventArgsModule
{
	[Jazor(Op.Inline ,"Microsoft.AspNetCore.Components.Web.FocusEventArgs.Type.get", "__arg1.type")]
	public extern static string? _cf780dcf203a15ea(FocusEvent instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.FocusEventArgs.Type.set")]
	public extern static void _c9d8d2e7e9689a1b(FocusEvent instance, string? value);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.Web.FocusEventArgs.FocusEventArgs()")]
	public extern static FocusEvent _3cfa503e303c8323();
}
