namespace Jazor.CLR;

[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.EventCallback<TValue>")]
public static class EventCallbackT1Module<TValue>
{
	[Jazor(Op.Discard ,"static readonly Microsoft.AspNetCore.Components.EventCallback<TValue>.Empty")]
	public extern static Object _3b318d176dba15f8();

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.EventCallback<TValue>.EventCallback(Microsoft.AspNetCore.Components.IHandleEvent, System.MulticastDelegate)")]
	public extern static Object _e8385a8026f90990(Object? receiver, global::System.MulticastDelegate? @delegate);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.EventCallback<TValue>.HasDelegate.get")]
	public extern static bool _e694c901cc293d0e(Object instance);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.EventCallback<TValue>.InvokeAsync(TValue)")]
	public extern static global::System.Threading.Tasks.Task _57e2038522c641fb(Object instance, TValue? arg);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.EventCallback<TValue>.InvokeAsync()")]
	public extern static global::System.Threading.Tasks.Task _49f47d6d1be5edfa(Object instance);

	[Jazor(Op.Discard ,"override Microsoft.AspNetCore.Components.EventCallback<TValue>.GetHashCode()")]
	public extern static Number _bdffb20a4557c4b1(Object instance);

	[Jazor(Op.Discard ,"override Microsoft.AspNetCore.Components.EventCallback<TValue>.Equals(object)")]
	public extern static bool _44fce1ac0b109220(Object instance, object? obj);
}
