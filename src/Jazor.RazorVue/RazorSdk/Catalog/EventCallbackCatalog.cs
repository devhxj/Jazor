using ECMAScript.Contract;

namespace Jazor.RazorVue.RazorSdk.Catalog;

/// <summary>
/// 将 Blazor EventCallback 及其工厂调用保留为 Jazor 的事件宿主契约。
/// It keeps generated event bindings on the component update path instead of plain JS callbacks.
/// </summary>
/// <remarks>
/// EventCallback 不是普通 delegate：它携带 receiver，并通过 InvokeAsync 进入组件更新协议。
/// 本模块主要提供白名单允许面，不把它误降级为任意 JavaScript 函数或绕过组件宿主。
/// </remarks>
[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.EventCallback")]
public static class EventCallbackCatalog
{
    [Jazor(Op.Allowed, "static readonly Microsoft.AspNetCore.Components.EventCallback.Factory")]
    public extern static object _f486f22f2383a3f9();

    [Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.EventCallback.InvokeAsync()")]
    public extern static System.Threading.Tasks.Task _invokeAsync(object instance);
}

/// <summary>泛型 EventCallback 的类型和调用白名单映射，保留参数类型以便 Razor SG 正确绑定。</summary>
[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.EventCallback<TValue>")]
public static class EventCallbackTCatalog<TValue>
{
    [Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.EventCallback<TValue>.InvokeAsync(TValue)")]
    public extern static System.Threading.Tasks.Task _invokeAsync(object instance, TValue value);
}

/// <summary>EventCallbackFactory 的受支持创建入口白名单映射，供 host 投影 receiver 和回调生命周期。</summary>
[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.EventCallbackFactory")]
public static class EventCallbackFactoryCatalog
{
    [Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.EventCallbackFactory.Create(object, System.Action)")]
    public extern static object _17a24972e4111a8c(object instance, object receiver, object callback);

    [Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.EventCallbackFactory.Create<TValue>(object, System.Action<TValue>)")]
    public extern static object _7e22540eaabef3f4<TValue>(object instance, object receiver, object callback);

    [Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.EventCallbackFactory.Create(object, System.Func<System.Threading.Tasks.Task>)")]
    public extern static object _3a687875967a8b7b(object instance, object receiver, object callback);

    [Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.EventCallbackFactory.Create<TValue>(object, System.Func<TValue, System.Threading.Tasks.Task>)")]
    public extern static object _76e4cf9a2d148858<TValue>(object instance, object receiver, object callback);
}
