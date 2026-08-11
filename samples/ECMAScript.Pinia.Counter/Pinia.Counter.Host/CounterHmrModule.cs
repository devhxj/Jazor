using System;
using ECMAScript;
using ECMAScript.VueContract;
using static ECMAScript.Pinia;
using static ECMAScript.Vue;

namespace Pinia.Counter.Host;

[ECMAScriptModule("components/counter-hmr.mjs")]
public static class CounterHmrModule
{
    public static IVueComponent Component = DefineComponent(new VueComponentOptions
    {
        Name = "PiniaCounterHmrCookbook",
        Setup = Setup
    });

    public static PiniaHotUpdateHandler CreateCounterHotHandler(IObject hot)
        => AcceptHMRUpdate(CounterStoreModule.UseCounterStore, hot);

    public static PiniaHotUpdateHandler CreateProjectedCounterHotHandler(IObject hot)
        => AcceptHMRUpdate(CounterStoreModule.UseProjectedCounterStore, hot);

    public static CounterStore ResolveCounterStore(PiniaInstance pinia, StoreGeneric hot)
        => CounterStoreModule.UseCounterStore.Use(pinia, hot);

    public static ProjectedStore<CounterStore, CounterPluginExtensions, CounterPluginState> ResolveProjectedCounterStore(
        PiniaInstance pinia,
        StoreGeneric hot)
        => CounterStoreModule.UseProjectedCounterStore.Use(pinia, hot);

    private static VueRenderCallback Setup()
    {
        var store = CounterStoreModule.UseCounterStore.Use();
        var projectedStore = CounterStoreModule.UseProjectedCounterStore.Use();
        var customState = projectedStore.AsCustomState();
        Action installHotSnapshot = () => customState.PersistedAt = "hmr:" + projectedStore.AsStore().Id;

        return () => H("section", new VueObject
        {
            Class = "counter-hmr-shell"
        }, new IVNode[]
        {
            H("h2", "HMR cookbook"),
            H("p", "acceptHMRUpdate(useStore, hot) and storeDefinition.Use(pinia, hot) stay explicit in C# so Vite/Jolt hot-module wiring can remain a host concern instead of hidden compiler magic."),
            H("ul", new VueObject
            {
                Class = "counter-notes"
            }, new IVNode[]
            {
                H("li", "store id: " + store.Id),
                H("li", "auditTag: " + projectedStore.AsCustomProperties().AuditTag),
                H("li", "persistedAt: " + customState.PersistedAt),
                H("li", "consumer bridge calls import.meta.hot.accept(createCounterHotHandler(import.meta.hot))")
            }),
            H("div", new VueObject
            {
                Class = "counter-actions"
            }, new IVNode[]
            {
                CreateActionButton("Prime HMR snapshot", "action-button action-button--accent", installHotSnapshot)
            })
        });
    }

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
