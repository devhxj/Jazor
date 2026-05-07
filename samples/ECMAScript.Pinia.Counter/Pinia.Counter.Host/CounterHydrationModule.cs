using System;
using System.ComponentModel;
using ECMAScript;
using ECMAScript.VueContract;
using static ECMAScript.Pinia;
using static ECMAScript.Vue3;

namespace Pinia.Counter.Host;

[ECMAScript]
[Description("@#")]
public sealed record CounterHydrationState : PiniaStateTree
{
    public int Count { get; set; }

    public string Status { get; set; } = "";
}

[ECMAScript]
[Description("@#")]
public sealed record CounterHydrationStatePatch : PiniaStatePatch<CounterHydrationState>
{
    public int? Count { get; init; }

    public string? Status { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record CounterHydrationStore : Vue3.VueProps
{
    public int Count { get; init; }

    public string Status { get; init; } = "";

    public Vue3.IVueRef<string> ClientOnlyNote { get; init; } = default!;

    public Func<bool> CanHydrateClientOnlyNote { get; init; } = default!;

    public Action RefreshClientOnlyNote { get; init; } = default!;
}

[ECMAScript]
[Description("@#")]
public abstract class CounterHydrationOptionStore : Store<CounterHydrationState>
{
    public extern int Count { get; set; }

    public extern string Status { get; set; }
}

[ECMAScriptModule("components/counter-hydration.mjs")]
public static class CounterHydrationModule
{
    public const string HydrationStoreId = "counterHydration";
    public const string HydrationOptionStoreId = "counterHydrationOptions";

    public static StoreDefinition<CounterHydrationStore> UseHydrationStore = DefineStore<CounterHydrationStore>(
        HydrationStoreId,
        SetupHydrationStore);

    public static StoreDefinition<CounterHydrationOptionStore> UseHydrationOptionStore
        = DefineStore<CounterHydrationOptionStore, CounterHydrationState>(
            HydrationOptionStoreId,
            new DefineStoreOptions<CounterHydrationState>
            {
                State = CreateHydrationState,
                Hydrate = HydrateOptionStore
            });

    public static IVueComponent Component = DefineComponent(new VueComponentOptions
    {
        Name = "PiniaCounterHydrationCookbook",
        Setup = Setup
    });

    public static void SeedInitialOptionStoreState(PiniaInstance pinia)
    {
        pinia.State.Value[HydrationOptionStoreId] = new CounterHydrationState
        {
            Count = 12,
            Status = "serialized SSR payload"
        };
    }

    private static CounterHydrationStore SetupHydrationStore(SetupStoreHelpers helpers)
    {
        var clientOnlyNote = SkipHydrate(Ref("client-only note seeded in setup store"));

        return new CounterHydrationStore
        {
            Count = 4,
            Status = "setup-store hydration boundary is ready",
            ClientOnlyNote = clientOnlyNote,
            CanHydrateClientOnlyNote = helpers.Action(() => ShouldHydrate(clientOnlyNote), "canHydrateClientOnlyNote"),
            RefreshClientOnlyNote = helpers.Action(() =>
            {
                clientOnlyNote.Value = "client note refreshed at " + UseHydrationOptionStore.Use().Status;
            }, "refreshClientOnlyNote")
        };
    }

    private static CounterHydrationState CreateHydrationState()
        => new()
        {
            Count = 8,
            Status = "option-store hydration hook waiting"
        };

    private static void HydrateOptionStore(CounterHydrationState storeState, CounterHydrationState initialState)
    {
        storeState.Count = initialState.Count;
        storeState.Status = initialState.Status + " -> hydrate(storeState, initialState)";
    }

    private static VueRenderCallback Setup()
    {
        var setupStore = UseHydrationStore.Use();
        var optionStore = UseHydrationOptionStore.Use();
        var clientHydrates = Computed(setupStore.CanHydrateClientOnlyNote);

        Action reapplyClientNote = setupStore.RefreshClientOnlyNote;
        Action hydrateSnapshot = () =>
        {
            optionStore.Patch(new CounterHydrationStatePatch
            {
                Status = "hydration snapshot captured from client action"
            });
        };

        return () => H("section", new VueObject
        {
            Class = "counter-hydration-shell"
        }, new IVNode[]
        {
            H("h2", "Hydration cookbook"),
            H("p", "skipHydrate()/shouldHydrate() remain explicit setup-store authoring tools, while option-store hydrate(storeState, initialState) stays available for SSR/client boundary repair without hiding runtime semantics."),
            H("ul", new VueObject
            {
                Class = "counter-notes"
            }, new IVNode[]
            {
            H("li", "setup store id: " + HydrationStoreId),
            H("li", "option store id: " + optionStore.Id),
            H("li", "client-only note: " + setupStore.ClientOnlyNote),
            H("li", "should hydrate client-only note: " + clientHydrates.Value),
            H("li", "option-store status: " + optionStore.Status)
            }),
            H("div", new VueObject
            {
                Class = "counter-actions"
            }, new IVNode[]
            {
                CreateActionButton("Refresh client note", "action-button action-button--accent", reapplyClientNote),
                CreateActionButton("Hydration snapshot", "action-button", hydrateSnapshot)
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
