namespace Jazor.CLR;

// RazorVue's @ref lifecycle produces the native HTMLElement carrier directly.
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.ElementReference", "HTMLElement")]
public static class ElementReferenceModule
{
	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.ElementReference.Id.get")]
	public extern static string _bf83f0cb19d54be6(HTMLElement instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.ElementReference.Context.get")]
	public extern static Object? _3d3a4eb22020be49(HTMLElement instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.ElementReference.ElementReference(string, Microsoft.AspNetCore.Components.ElementReferenceContext)")]
	public extern static HTMLElement _15171a45639a345e(string id, Object? context);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.ElementReference.ElementReference(string)")]
	public extern static HTMLElement _cf5694d1ebdbd8ef(string id);
}
