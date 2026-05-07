using System;
using ECMAScript;
using ECMAScript.VueContract;
using static ECMAScript.Pinia;
using static ECMAScript.Vue3;

namespace Pinia.Counter.Host;

[ECMAScriptModule("components/counter-subscription.mjs")]
public static class CounterSubscriptionModule
{
    public static IVueComponent Component = DefineComponent(new VueComponentOptions
    {
        Name = "PiniaCounterSubscriptionCookbook",
        Setup = Setup
    });

    private static VueRenderCallback Setup()
    {
        var store = CounterStoreModule.UseCounterStore.Use();
        var mutationKind = Ref("No mutations observed yet.");
        var storeId = Ref(store.Id);
        var statusSnapshot = Ref(store.Status);
        var countSnapshot = Ref(store.Count);
        var payloadSnapshot = Ref("payload appears only for $patch({ ... }) mutations.");
        var eventShape = Ref("Debugger events are dev-only and may be unavailable.");
        var notificationCount = Ref(0);
        PiniaDetachCallback? detach = null;

        PiniaSubscriptionCallback<CounterState> handleMutation = (mutation, state) =>
        {
            notificationCount.Value += 1;
            mutationKind.Value = DescribeMutationType(mutation.Type);
            storeId.Value = mutation.StoreId;
            countSnapshot.Value = state.Count;
            statusSnapshot.Value = state.Status;
            payloadSnapshot.Value = ReadMutationSummary(mutation);
            eventShape.Value = DescribeEvents(mutation.Events);
        };

        Action applyDirectMutation = () =>
        {
            store.Count += 1;
            store.Status = "Direct assignment updated the counter store.";
        };
        Action applyObjectPatch = () => store.Patch(new CounterStatePatch
        {
            Count = store.Count + 3,
            Status = "Object patch updated the counter store."
        });
        Action applyFunctionPatch = () => store.Patch(state =>
        {
            state.Count += 2;
            state.Status = "Function patch updated the counter store.";
        });

        OnMounted(() =>
        {
            detach = store.Subscribe(handleMutation, new SubscribeOptions
            {
                Detached = true,
                Flush = VueWatchFlush.Sync
            });
        });

        OnUnmounted(() =>
        {
            if (detach != null)
            {
                detach();
            }
        });

        return () => H("section", new VueObject
        {
            Class = "counter-subscription-shell"
        }, new IVNode[]
        {
            H("h2", "Subscription cookbook"),
            H("p", "$subscribe() is registered with detached + sync options so the sample can inspect direct mutations, object patches, and function patches from one stable callback. Direct assignments may report multiple sync notifications when several fields change back-to-back."),
            H("div", new VueObject
            {
                Class = "counter-actions"
            }, new IVNode[]
            {
                CreateActionButton("Direct +1", "action-button action-button--accent", applyDirectMutation),
                CreateActionButton("Object patch", "action-button", applyObjectPatch),
                CreateActionButton("Function patch", "action-button", applyFunctionPatch)
            }),
            H("ul", new VueObject
            {
                Class = "counter-notes"
            }, new IVNode[]
            {
                H("li", "mutation kind: " + mutationKind.Value),
                H("li", "store id: " + storeId.Value),
                H("li", "count snapshot: " + countSnapshot.Value),
                H("li", "status snapshot: " + statusSnapshot.Value),
                H("li", "payload summary: " + payloadSnapshot.Value),
                H("li", "events shape: " + eventShape.Value),
                H("li", "notifications seen: " + notificationCount.Value)
            })
        });
    }

    private static string DescribeMutationType(MutationType type)
        => type switch
        {
            MutationType.Direct => "direct assignment",
            MutationType.PatchObject => "$patch({ ... }) object merge",
            MutationType.PatchFunction => "$patch((state) => ...) callback",
            _ => "unknown mutation"
        };

    private static string ReadMutationSummary(SubscriptionMutation<CounterState> mutation)
    {
        if (mutation.Type == MutationType.PatchObject)
        {
            var patchMutation = (SubscriptionMutationPatchObject<CounterState>)mutation;
            var payload = (CounterStatePatch)patchMutation.Payload;

            if (payload.Status != null)
            {
                return "payload.status = " + payload.Status;
            }

            if (payload.Count != null)
            {
                return "payload.count = " + payload.Count;
            }

            return "object patch payload captured without known fields";
        }

        if (mutation.Type == MutationType.PatchFunction)
        {
            return "function patch metadata captured without a payload object";
        }

        return "direct assignments do not expose a payload object";
    }

    private static string DescribeEvents(SubscriptionMutationEvents? events)
        => events == null ? "not provided" : "reported";

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
