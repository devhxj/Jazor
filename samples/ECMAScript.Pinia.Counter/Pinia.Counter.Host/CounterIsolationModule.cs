using System;
using ECMAScript;
using ECMAScript.VueContract;
using static ECMAScript.Pinia;
using static ECMAScript.Vue;

namespace Pinia.Counter.Host;

[ECMAScriptModule("components/counter-isolation.mjs")]
public static class CounterIsolationModule
{
    public static IVueComponent Component = DefineComponent(new VueComponentOptions
    {
        Name = "PiniaCounterIsolationCookbook",
        Setup = Setup
    });

    public static string CompareIsolatedRoots()
    {
        var leftPinia = CreateInstalledConfiguredPinia();
        var rightPinia = CreateInstalledConfiguredPinia();
        try
        {
            var leftStore = CounterStoreModule.UseCounterStore.Use(leftPinia);
            var leftProjected = CounterStoreModule.UseProjectedCounterStore.Use(leftPinia);
            var rightStore = CounterStoreModule.UseCounterStore.Use(rightPinia);
            var rightProjected = CounterStoreModule.UseProjectedCounterStore.Use(rightPinia);

            leftStore.Increment();
            leftProjected.AsCustomState().PersistedAt = "isolated:left:" + leftStore.Id;

            return leftStore.Count
                + "|"
                + rightStore.Count
                + "|"
                + leftProjected.AsCustomProperties().AuditTag
                + "|"
                + rightProjected.AsCustomProperties().AuditTag
                + "|"
                + leftProjected.AsCustomState().PersistedAt
                + "|"
                + rightProjected.AsCustomState().PersistedAt;
        }
        finally
        {
            DisposePinia(leftPinia);
            DisposePinia(rightPinia);
        }
    }

    private static VueRenderCallback Setup()
    {
        var leftPinia = CreateInstalledConfiguredPinia();
        var rightPinia = CreateInstalledConfiguredPinia();
        var leftStore = CounterStoreModule.UseCounterStore.Use(leftPinia);
        var leftProjected = CounterStoreModule.UseProjectedCounterStore.Use(leftPinia);
        var rightStore = CounterStoreModule.UseCounterStore.Use(rightPinia);
        var rightProjected = CounterStoreModule.UseProjectedCounterStore.Use(rightPinia);

        var snapshot = Ref(DescribeSnapshot(leftStore, rightStore, leftProjected, rightProjected));

        Action incrementLeftOnly = () =>
        {
            leftStore.Increment();
            leftProjected.AsCustomState().PersistedAt = "isolated:left:" + leftStore.Count;
            snapshot.Value = DescribeSnapshot(leftStore, rightStore, leftProjected, rightProjected);
        };

        Action incrementRightOnly = () =>
        {
            rightStore.Increment();
            rightProjected.AsCustomState().PersistedAt = "isolated:right:" + rightStore.Count;
            snapshot.Value = DescribeSnapshot(leftStore, rightStore, leftProjected, rightProjected);
        };

        OnUnmounted(() =>
        {
            DisposePinia(leftPinia);
            DisposePinia(rightPinia);
        });

        return () => H("section", new VueObject
        {
            Class = "counter-isolation-shell"
        }, new IVNode[]
        {
            H("h2", "Root isolation cookbook"),
            H("p", "Explicit StoreDefinition.Use(pinia) resolution keeps multiple Pinia roots isolated even when they reuse the same generated store definition and plugin projection contract."),
            H("ul", new VueObject
            {
                Class = "counter-notes"
            }, new IVNode[]
            {
                H("li", "snapshot: " + snapshot.Value),
                H("li", "left persistedAt: " + leftProjected.AsCustomState().PersistedAt),
                H("li", "right persistedAt: " + rightProjected.AsCustomState().PersistedAt)
            }),
            H("div", new VueObject
            {
                Class = "counter-actions"
            }, new IVNode[]
            {
                CreateActionButton("Increment left root", "action-button action-button--accent", incrementLeftOnly),
                CreateActionButton("Increment right root", "action-button", incrementRightOnly)
            })
        });
    }

    private static PiniaInstance CreateInstalledConfiguredPinia()
    {
        var pinia = AppModule.CreateConfiguredPinia();
        AppModule.CreatePiniaInstallationApp(pinia);
        return pinia;
    }

    private static string DescribeSnapshot(
        CounterStore leftStore,
        CounterStore rightStore,
        ProjectedStore<CounterStore, CounterPluginExtensions, CounterPluginState> leftProjected,
        ProjectedStore<CounterStore, CounterPluginExtensions, CounterPluginState> rightProjected)
        => "left=" + leftStore.Count
            + ", right=" + rightStore.Count
            + ", leftAudit=" + leftProjected.AsCustomProperties().AuditTag
            + ", rightAudit=" + rightProjected.AsCustomProperties().AuditTag;

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
