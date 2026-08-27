namespace Jazor.CLR;

[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.RenderFragment")]
public static class RenderFragmentModule
{
	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.RenderFragment.RenderFragment(object, nint)")]
	public extern static Object _330ee5c4c9c476fa(object @object, nint @method);

	[Jazor(Op.Allowed ,"virtual Microsoft.AspNetCore.Components.RenderFragment.Invoke(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]
	public extern static void _cb20f9d98b0a787b(Object instance, Object builder);

	[Jazor(Op.Discard ,"virtual Microsoft.AspNetCore.Components.RenderFragment.BeginInvoke(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder, System.AsyncCallback, object)")]
	public extern static global::System.IAsyncResult _c1211d3f8a275393(Object instance, Object builder, global::System.AsyncCallback callback, object @object);

	[Jazor(Op.Discard ,"virtual Microsoft.AspNetCore.Components.RenderFragment.EndInvoke(System.IAsyncResult)")]
	public extern static void _312fb5992d73ab17(Object instance, global::System.IAsyncResult result);
}
