using System.ComponentModel;
using ECMAScript;
using static ECMAScript.Pinia;
using static ECMAScript.PiniaTesting;
using static ECMAScript.Vue3;

namespace Pinia.Counter.Host;

[ECMAScript]
[Description("@#")]
public sealed record CounterTestingInitialState : TestingInitialState
{
    public CounterStatePatch Counter { get; init; } = default!;
}

[ECMAScriptModule("tests/counter-testing.mjs")]
public static class CounterTestingModule
{
    public static PiniaPlugin TypedTestingAuditPlugin
        = ProjectPlugin<CounterStore, DefineStoreOptionsInPlugin<CounterState, CounterGetters, CounterActions>, CounterPluginExtensions, CounterPluginState, CounterPluginExtensions>(
            InstallTypedTestingAuditPlugin);

    public static TestingPinia CreateTestingRoot()
        => CreateTestingPinia(new TestingOptions
        {
            InitialState = new CounterTestingInitialState
            {
                Counter = new CounterStatePatch
                {
                    Count = 9,
                    Status = "Seeded from createTestingPinia()."
                }
            },
            StubActions = (PiniaTestingStubActionPredicate)ShouldStubAction,
            WritableComputed = true,
            StubPatch = false,
            StubReset = false,
            FakeApp = true,
            Plugins =
            [
                TypedTestingAuditPlugin
            ],
            CreateSpy = WrapSpy
        });

    public static TestingPinia CreateStrictTestingRoot()
        => CreateTestingPinia(new TestingOptions
        {
            InitialState = new CounterTestingInitialState
            {
                Counter = new CounterStatePatch
                {
                    Count = 15,
                    Status = "Seeded from strict createTestingPinia()."
                }
            },
            StubActions = new[] { "increment", "decrement" },
            WritableComputed = true,
            StubPatch = true,
            StubReset = true,
            FakeApp = true,
            Plugins =
            [
                TypedTestingAuditPlugin
            ],
            CreateSpy = WrapSpy
        });

    private static bool ShouldStubAction(string actionName, StoreGeneric store)
        => actionName == "decrement" && store.Id == "counter";

    private static Delegate WrapSpy(Delegate? callback)
        => callback ?? ((Action)Noop);

    private static void Noop()
    {
    }

    private static CounterPluginExtensions? InstallTypedTestingAuditPlugin(
        PiniaPluginContext<CounterStore, DefineStoreOptionsInPlugin<CounterState, CounterGetters, CounterActions>, CounterPluginExtensions, CounterPluginState> context)
    {
        var projectedStore = context.Store;
        var customState = projectedStore.AsCustomState();
        var options = context.Options;
        var increment = options.Actions.Increment;

        customState.PersistedAt = "testing:" + projectedStore.AsStore().Id + ":" + (increment == null ? "missing" : "typed");

        return new CounterPluginExtensions
        {
            AuditTag = projectedStore.AsStore().Id + ":testing"
        };
    }
}
