namespace Jazor.CLR;

[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.EventCallbackFactory")]
public static class EventCallbackFactoryModule
{
	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.EventCallbackFactory.Create(object, Microsoft.AspNetCore.Components.EventCallback)")]
	public extern static Object _09b675e364e49c78(Object instance, object receiver, Object callback);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.EventCallbackFactory.Create(object, System.Action)")]
	public extern static Object _762814517b76c7d0(Object instance, object receiver, global::System.Action callback);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.EventCallbackFactory.Create(object, System.Action<object>)")]
	public extern static Object _54fc5c35c094cc69(Object instance, object receiver, global::System.Action<object> callback);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.EventCallbackFactory.Create(object, System.Func<System.Threading.Tasks.Task>)")]
	public extern static Object _3850fb831be86066(Object instance, object receiver, global::System.Func<global::System.Threading.Tasks.Task> callback);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.EventCallbackFactory.Create(object, System.Func<object, System.Threading.Tasks.Task>)")]
	public extern static Object _f9d9aadf03b7e804(Object instance, object receiver, global::System.Func<object, global::System.Threading.Tasks.Task> callback);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.EventCallbackFactory.Create<TValue>(object, Microsoft.AspNetCore.Components.EventCallback)")]
	public extern static Object _2ef15e5d9151e5cd<TValue>(Object instance, object receiver, Object callback);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.EventCallbackFactory.Create<TValue>(object, Microsoft.AspNetCore.Components.EventCallback<TValue>)")]
	public extern static Object _968a25c433f735d7<TValue>(Object instance, object receiver, Object callback);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.EventCallbackFactory.Create<TValue>(object, System.Action)")]
	public extern static Object _0e7122411c83074c<TValue>(Object instance, object receiver, global::System.Action callback);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.EventCallbackFactory.Create<TValue>(object, System.Action<TValue>)")]
	public extern static Object _471f75f35bf4d5d9<TValue>(Object instance, object receiver, global::System.Action<TValue> callback);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.EventCallbackFactory.Create<TValue>(object, System.Func<System.Threading.Tasks.Task>)")]
	public extern static Object _8d69d9d3720ae18a<TValue>(Object instance, object receiver, global::System.Func<global::System.Threading.Tasks.Task> callback);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.EventCallbackFactory.Create<TValue>(object, System.Func<TValue, System.Threading.Tasks.Task>)")]
	public extern static Object _236a25b95303a508<TValue>(Object instance, object receiver, global::System.Func<TValue, global::System.Threading.Tasks.Task> callback);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.EventCallbackFactory.CreateInferred<TValue>(object, System.Action<TValue>, TValue)")]
	public extern static Object _ccd38badf5e7f1df<TValue>(Object instance, object receiver, global::System.Action<TValue> callback, TValue value);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.EventCallbackFactory.CreateInferred<TValue>(object, System.Func<TValue, System.Threading.Tasks.Task>, TValue)")]
	public extern static Object _d3d746194b3138a6<TValue>(Object instance, object receiver, global::System.Func<TValue, global::System.Threading.Tasks.Task> callback, TValue value);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.EventCallbackFactory.EventCallbackFactory()")]
	public extern static Object _9bdafcddae492c47();
}
