using System;
using System.ComponentModel;
using ECMAScript;
using ECMAScript.VueContract;
using static ECMAScript.Pinia;
using static ECMAScript.Vue;

namespace Pinia.Counter.Host;

[ECMAScript]
[Description("@#")]
public sealed record CounterOptionsComputed : VueProps
{
    public Func<int> Count { get; init; } = default!;

    public Func<string> Status { get; init; } = default!;

    public Func<int> DoubleCount { get; init; } = default!;

    public Func<int> TripleCount { get; init; } = default!;

    public Func<string> AuditTag { get; init; } = default!;
}

[ECMAScript]
[Description("@#")]
public sealed record CounterOptionsMethods : VueProps
{
    public Action Increment { get; init; } = default!;

    public Action Decrement { get; init; } = default!;
}

[ECMAScriptModule("components/counter-cookbook.mjs")]
public static class CounterCookbookModule
{
    public static IVueComponent Component = DefineComponent(new VueComponentOptions
    {
        Name = "PiniaCounterCookbook",
        Computed = CreateComputed(),
        Methods = CreateMethods(),
        Setup = Setup
    });

    private static CounterOptionsComputed CreateComputed()
        => MapState<CounterOptionsComputed, ProjectedStore<CounterStore, CounterPluginExtensions, CounterPluginState>>(
            CounterStoreModule.UseProjectedCounterStore,
            new PiniaStateMapper<ProjectedStore<CounterStore, CounterPluginExtensions, CounterPluginState>>
            {
                { nameof(CounterOptionsComputed.Count), PiniaStateMapValue<ProjectedStore<CounterStore, CounterPluginExtensions, CounterPluginState>>.From(nameof(CounterStore.Count)) },
                { nameof(CounterOptionsComputed.Status), PiniaStateMapValue<ProjectedStore<CounterStore, CounterPluginExtensions, CounterPluginState>>.From(nameof(CounterStore.Status)) },
                { nameof(CounterOptionsComputed.DoubleCount), PiniaStateMapValue<ProjectedStore<CounterStore, CounterPluginExtensions, CounterPluginState>>.From(nameof(CounterStore.DoubleCount)) },
                { nameof(CounterOptionsComputed.TripleCount), PiniaStateMapValue<ProjectedStore<CounterStore, CounterPluginExtensions, CounterPluginState>>.From(ReadTripleCount) },
                { nameof(CounterOptionsComputed.AuditTag), PiniaStateMapValue<ProjectedStore<CounterStore, CounterPluginExtensions, CounterPluginState>>.From(nameof(CounterPluginExtensions.AuditTag)) }
            });

    private static CounterOptionsMethods CreateMethods()
        => MapActions<CounterOptionsMethods, ProjectedStore<CounterStore, CounterPluginExtensions, CounterPluginState>>(
            CounterStoreModule.UseProjectedCounterStore,
            [nameof(CounterStore.Increment), nameof(CounterStore.Decrement)]);

    private static VueRenderCallback Setup()
    {
        var projectedStore = CounterStoreModule.UseProjectedCounterStore.Use();
        var refs = CounterStoreModule.UseProjectedCounterStoreRefs(projectedStore);
        var baseStore = projectedStore.AsStore();
        var customState = projectedStore.AsCustomState();

        customState.PersistedAt = "component:" + baseStore.Id;

        return () => H("section", new VueObject
        {
            Class = "counter-cookbook-shell"
        }, new IVNode[]
        {
            H("h2", "Projected plugin cookbook"),
            H("p", "Projected store definitions flow through storeToRefs(), Options API helpers, and direct custom-property/custom-state projections without inventing a separate runtime object."),
            H("ul", new IVNode[]
            {
                H("li", "auditTag: " + projectedStore.AsCustomProperties().AuditTag),
                H("li", "persistedAt: " + projectedStore.AsCustomState().PersistedAt),
                H("li", "countRef: " + refs[nameof(CounterStore.Count)]!.Value),
                H("li", "statusRef: " + refs[nameof(CounterStore.Status)]!.Value),
                H("li", "doubleCount: " + projectedStore.AsStore().DoubleCount),
                H("li", "tripleCount: " + ReadTripleCount(projectedStore))
            }),
            H("div", new VueObject
            {
                Class = "counter-actions"
            }, new IVNode[]
            {
                CreateActionButton("Projected increment", "action-button action-button--accent", baseStore.Increment),
                CreateActionButton("Projected decrement", "action-button", baseStore.Decrement)
            }),
            H("p", new VueObject
            {
                Class = "counter-status"
            }, "Options API helpers are configured through CreateComputed()/CreateMethods(); the live card shows the projected store + projected refs path.")
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

    private static PiniaValue ReadTripleCount(ProjectedStore<CounterStore, CounterPluginExtensions, CounterPluginState> store)
        => store.AsStore().Count * 3;
}
