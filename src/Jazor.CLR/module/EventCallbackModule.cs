namespace Jazor.CLR;

// EventCallback dispatch is a RazorVue product protocol. Keep the canonical CLR
// keys here without pretending that EventCallback has a standalone JS object model.
[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.EventCallback")]
public static class EventCallbackModule
{
	[Jazor(Op.Allowed ,"static readonly Microsoft.AspNetCore.Components.EventCallback.Factory")]
	public extern static Object _b22413fc4a9d76c2();

	[Jazor(Op.Discard ,"static readonly Microsoft.AspNetCore.Components.EventCallback.Empty")]
	public extern static Object _39dee5b8d3cbb876();

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.EventCallback.EventCallback(Microsoft.AspNetCore.Components.IHandleEvent, System.MulticastDelegate)")]
	public extern static Object _3948fa4c99f4b6c1(Object? receiver, global::System.MulticastDelegate? @delegate);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.EventCallback.HasDelegate.get")]
	public extern static bool _5c308fc6581c15cd(Object instance);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.EventCallback.InvokeAsync(object)")]
	public extern static global::System.Threading.Tasks.Task _d1214d832985499b(Object instance, object? arg);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.EventCallback.InvokeAsync()")]
	public extern static global::System.Threading.Tasks.Task _0c386d015150c8f9(Object instance);

	[Jazor(Op.Discard ,"override Microsoft.AspNetCore.Components.EventCallback.GetHashCode()")]
	public extern static Number _0365a3fb41563ebf(Object instance);

	[Jazor(Op.Discard ,"override Microsoft.AspNetCore.Components.EventCallback.Equals(object)")]
	public extern static bool _ac07c0acbf2a9a41(Object instance, object? obj);
}
