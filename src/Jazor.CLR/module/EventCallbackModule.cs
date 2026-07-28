namespace Jazor.CLR;

[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.EventCallback")]
public static class EventCallbackModule
{
    [Jazor(Op.Allowed, "static readonly Microsoft.AspNetCore.Components.EventCallback.Factory")]
    public extern static object _f486f22f2383a3f9();
}

[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.EventCallbackFactory")]
public static class EventCallbackFactoryModule
{
    [Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.EventCallbackFactory.Create(object, System.Action)")]
    public extern static object _17a24972e4111a8c(object instance, object receiver, object callback);

    [Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.EventCallbackFactory.Create<TValue>(object, System.Action<TValue>)")]
    public extern static object _7e22540eaabef3f4<TValue>(object instance, object receiver, object callback);
}
