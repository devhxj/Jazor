using ECMAScript;
using static ECMAScript.Pinia;
using static ECMAScript.Vue;

namespace Pinia.Counter.Host;

[ECMAScriptModule("host/app.mjs")]
public static class AppModule
{
    public static VueApp CreateConfiguredApp()
    {
        var app = CreateApp(DefineComponent(new VueComponentOptions
        {
            Name = "PiniaCounterRoot",
            Render = RenderRoot
        }));
        var pinia = CreateConfiguredPinia();

        app.Use(pinia);
        app.OnUnmount(() => DisposePinia(pinia));
        return app;
    }

    public static PiniaInstance CreateConfiguredPinia()
    {
        var pinia = CreatePinia()
            .Use(CounterStoreModule.InstallAuditPlugin);

        CounterHydrationModule.SeedInitialOptionStoreState(pinia);
        return pinia;
    }

    public static VueApp CreatePiniaInstallationApp(PiniaInstance pinia)
    {
        var app = CreateApp(DefineComponent(new VueComponentOptions
        {
            Name = "PiniaConfiguredRootShell",
            Render = RenderPiniaInstallationShell
        }));

        app.Use(pinia);
        return app;
    }

    public static PiniaInstance? ClearConfiguredActivePinia()
        => ClearActivePinia();

    public static void Boot(string selector)
    {
        var app = CreateConfiguredApp();
        app.Mount(selector);
    }

    private static IVNode RenderPiniaInstallationShell()
        => H("div");

    private static IVNode RenderRoot()
        => H("main", new VueObject
        {
            Class = "counter-root"
        }, new IVNode[]
        {
            H("section", new VueObject
            {
                Class = "counter-hero"
            }, new IVNode[]
            {
                H("p", new VueObject
                {
                    Class = "counter-kicker"
                }, "ECMAScript.Pinia production sample"),
                H("h1", new VueObject
                {
                    Class = "counter-title"
                }, "Typed Pinia stores, projected plugins, multi-store helpers, subscriptions, and testing"),
                H("p", new VueObject
                {
                    Class = "counter-copy"
                }, "The sample keeps Pinia as a normal external runtime while exercising authoring paths that matter in production code: defineStore(), storeToRefs(), plugin projections, mapStores(), $subscribe(), acceptHMRUpdate(), and createTestingPinia().")
            }),
            H("div", new VueObject
            {
                Class = "counter-stack"
            }, new IVNode[]
            {
                H(CounterAppModule.Component),
                H(CounterCookbookModule.Component),
                H(CounterMultiStoreModule.Component),
                H(CounterSubscriptionModule.Component),
                H(CounterHydrationModule.Component),
                H(CounterIsolationModule.Component),
                H(CounterHmrModule.Component)
            })
        });
}
