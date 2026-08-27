namespace Jazor.CLR;

[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.RenderFragment<TValue>")]
public static class RenderFragmentT1Module<TValue>
{
	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.RenderFragment<TValue>.RenderFragment(object, nint)")]
	public extern static Object _feb1584b69cf93fc(object @object, nint @method);

	[Jazor(Op.Allowed ,"virtual Microsoft.AspNetCore.Components.RenderFragment<TValue>.Invoke(TValue)")]
	public extern static Object _c6d7ce32bd55f3fc(Object instance, TValue value);

	[Jazor(Op.Discard ,"virtual Microsoft.AspNetCore.Components.RenderFragment<TValue>.BeginInvoke(TValue, System.AsyncCallback, object)")]
	public extern static global::System.IAsyncResult _03ff4f1870312482(Object instance, TValue value, global::System.AsyncCallback callback, object @object);

	[Jazor(Op.Discard ,"virtual Microsoft.AspNetCore.Components.RenderFragment<TValue>.EndInvoke(System.IAsyncResult)")]
	public extern static Object _4fb4e2bb5e868f8b(Object instance, global::System.IAsyncResult result);
}
