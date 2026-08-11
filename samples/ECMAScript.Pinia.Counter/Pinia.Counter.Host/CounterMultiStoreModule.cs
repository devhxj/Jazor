using System;
using System.ComponentModel;
using ECMAScript;
using ECMAScript.VueContract;
using static ECMAScript.Pinia;
using static ECMAScript.Vue;

namespace Pinia.Counter.Host;

[ECMAScript]
[Description("@#")]
public sealed record CounterMultiStoreComputed : Vue.VueProps
{
    [ECMAScriptName("counter")]
    public CounterStore Counter { get; init; } = default!;

    [ECMAScriptName("activity")]
    public ActivityStore Activity { get; init; } = default!;
}

[ECMAScript]
[Description("@#")]
public abstract class CounterMultiStoreThis
{
    [ECMAScriptName("counter")]
    public extern CounterStore Counter { get; }

    [ECMAScriptName("activity")]
    public extern ActivityStore Activity { get; }
}

[ECMAScriptModule("components/counter-multi-store.mjs")]
public static class CounterMultiStoreModule
{
    private static Vue.IVueRef<string> OptionsApiSnapshot = Ref("mapStores() snapshot will be captured after mount.");
    private static Vue.IVueRef<string> OptionsApiStoreIds = Ref("store ids pending");

    public static IVueComponent Component = DefineComponent(new VueComponentOptions
    {
        Name = "PiniaCounterMultiStore",
        Computed = CreateComputed(),
        Mounted = BindThis<CounterMultiStoreThis>(CaptureMappedStores),
        Setup = Setup
    });

    private static CounterMultiStoreComputed CreateComputed()
    {
        SetMapStoreSuffix("");
        return MapStores<CounterMultiStoreComputed>(
            CounterStoreModule.UseCounterStore,
            ActivityStoreModule.UseActivityStore);
    }

    private static VueRenderCallback Setup()
    {
        var counter = CounterStoreModule.UseCounterStore.Use();
        var activity = ActivityStoreModule.UseActivityStore.Use();

        Action incrementAndCapture = () =>
        {
            counter.Increment();
            activity.Capture("increment");
        };
        Action queueReview = activity.QueueReview;
        var liveSummary = Computed(() => counter.Status + " | " + activity.Summary);
        var combinedScore = Computed(() => counter.Count + activity.CompletedActions + activity.PendingReviews);

        return () => H("section", new VueObject
        {
            Class = "counter-multi-store-shell"
        }, new IVNode[]
        {
            H("h2", "Multi-store cookbook"),
            H("p", "SetMapStoreSuffix(\"\") keeps the component-instance fields aligned with store ids while mapStores() projects both stores through one Options API entry point."),
            H("div", new VueObject
            {
                Class = "counter-summary-grid"
            }, new IVNode[]
            {
                CreateMetricCard("counter.count", counter.Count, "metric-card metric-card--primary"),
                CreateMetricCard("activity.done", activity.CompletedActions, "metric-card metric-card--secondary"),
                CreateMetricCard("activity.pending", activity.PendingReviews, "metric-card metric-card--neutral"),
                CreateMetricCard("combined", combinedScore.Value, "metric-card metric-card--accent")
            }),
            H("p", new VueObject
            {
                Class = "counter-status"
            }, liveSummary.Value),
            H("div", new VueObject
            {
                Class = "counter-actions"
            }, new IVNode[]
            {
                CreateActionButton("Increment + capture", "action-button action-button--accent", incrementAndCapture),
                CreateActionButton("Queue review", "action-button", queueReview)
            }),
            H("ul", new VueObject
            {
                Class = "counter-notes"
            }, new IVNode[]
            {
                H("li", "mounted snapshot via mapStores(): " + OptionsApiSnapshot.Value),
                H("li", "mapped component store ids: " + OptionsApiStoreIds.Value),
                H("li", "direct setup render keeps the live surface readable while Options API captures the heterogeneous store projection.")
            })
        });
    }

    private static void CaptureMappedStores(CounterMultiStoreThis self)
    {
        OptionsApiSnapshot.Value = self.Counter.Status + " | " + self.Activity.Summary;
        OptionsApiStoreIds.Value = self.Counter.Id + " + " + self.Activity.Id;
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
