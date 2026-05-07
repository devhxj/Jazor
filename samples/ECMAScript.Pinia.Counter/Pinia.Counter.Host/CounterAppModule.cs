using System;
using System.ComponentModel;
using ECMAScript;
using ECMAScript.VueContract;
using static ECMAScript.Pinia;
using static ECMAScript.Vue3;

namespace Pinia.Counter.Host;

[ECMAScript]
[Description("@#")]
public sealed record CounterStatePatch : PiniaStatePatch<CounterState>
{
    public int? Count { get; init; }

    public string? Status { get; init; }
}

[ECMAScriptModule("components/counter-app.mjs")]
public static class CounterAppModule
{
    public static IVueComponent Component = DefineComponent(new VueComponentOptions
    {
        Name = "PiniaCounterApp",
        Setup = Setup
    });

    private static VueRenderCallback Setup()
    {
        var store = CounterStoreModule.UseCounterStore.Use();
        var refs = CounterStoreModule.UseCounterStoreRefs(store);

        Action patchPlusFive = () => store.Patch(new CounterStatePatch
        {
            Count = store.Count + 5,
            Status = "Applied $patch({ ... }) from the component."
        });
        Action resetStore = store.Reset;

        return () => H("section", new VueObject
        {
            Class = "counter-shell"
        }, new IVNode[]
        {
            H("p", new VueObject
            {
                Class = "counter-kicker"
            }, "ECMAScript.Pinia sample"),
            H("h1", new VueObject
            {
                Class = "counter-title"
            }, "Typed Pinia store authored in C#"),
            H("p", new VueObject
            {
                Class = "counter-copy"
            }, "The store comes from defineStore(), is resolved through StoreDefinition.Use(), and is read via storeToRefs()."),
            H("div", new VueObject
            {
                Class = "counter-grid"
            }, new IVNode[]
            {
                CreateMetricCard("count", refs.Count.Value, "metric-card metric-card--primary"),
                CreateMetricCard("doubleCount", refs.DoubleCount.Value, "metric-card metric-card--secondary")
            }),
            H("p", new VueObject
            {
                Class = "counter-status"
            }, refs.Status.Value),
            H("div", new VueObject
            {
                Class = "counter-actions"
            }, new IVNode[]
            {
                CreateActionButton("Increment", "action-button action-button--accent", store.Increment),
                CreateActionButton("Decrement", "action-button", store.Decrement),
                CreateActionButton("Patch +5", "action-button", patchPlusFive),
                CreateActionButton("Reset", "action-button action-button--ghost", resetStore)
            }),
            H("ul", new VueObject
            {
                Class = "counter-notes"
            }, new IVNode[]
            {
                H("li", "createPinia() stays a normal external runtime import."),
                H("li", "StoreDefinition<TStore>.Use() keeps the callable store factory explicit in C#."),
                H("li", "storeToRefs() returns typed refs for both state and getters.")
            })
        });
    }

    private static IVNode CreateMetricCard(string label, VueChild value, string className)
        => H("article", new VueObject
        {
            Class = className
        }, new IVNode[]
        {
            H("span", new VueObject
            {
                Class = "metric-label"
            }, label),
            H("strong", new VueObject
            {
                Class = "metric-value"
            }, value)
        });

    private static IVNode CreateActionButton(string label, string className, Action handler)
        => H("button", new VueObject
        {
            Type = "button",
            Class = className,
            Events = new VueEventHandlers
            {
                ["onClick"] = handler
            }
        }, label);
}
