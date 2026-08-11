using System.ComponentModel;
using ECMAScript;
using static ECMAScript.Pinia;
using static ECMAScript.PiniaTesting;
using static ECMAScript.Vue;

namespace Pinia.Counter.Host;

[ECMAScript]
[Description("@#")]
public sealed record CounterTestingInitialState : TestingInitialState
{
    [ECMAScriptName("counter")]
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
            StubActions = ProjectStubActionPredicate<CounterStore>(ShouldStubAction),
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

    public static TestingPinia CreateTypedTestingRoot()
        => CreateTestingPinia(new TestingOptions<Action, CounterStore>
        {
            InitialState = new CounterTestingInitialState
            {
                Counter = new CounterStatePatch
                {
                    Count = 12,
                    Status = "Seeded from combined typed createTestingPinia()."
                }
            },
            StubActions = ProjectStubActions<CounterStore>(ShouldStubTypedAction),
            WritableComputed = true,
            StubPatch = false,
            StubReset = false,
            FakeApp = true,
            Plugins =
            [
                TypedTestingAuditPlugin
            ],
            CreateSpy = WrapActionSpy
        });

    public static TestingPinia CreateFactoryTestingRoot()
        => CreateTestingPinia(new TestingOptions<Action, CounterStore>
        {
            InitialState = new CounterTestingInitialState
            {
                Counter = new CounterStatePatch
                {
                    Count = 18,
                    Status = "Seeded from combined typed factory createTestingPinia()."
                }
            },
            StubActions = TestingStubActions<CounterStore>.From(ShouldStubFactoryAction),
            WritableComputed = true,
            StubPatch = false,
            StubReset = false,
            FakeApp = true,
            Plugins =
            [
                TypedTestingAuditPlugin
            ],
            CreateSpy = WrapActionSpy
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
            StubActions = new[] { nameof(CounterStore.Increment), nameof(CounterStore.Decrement) },
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

    private static bool ShouldStubAction(string actionName, CounterStore store)
        => actionName == nameof(CounterStore.Decrement) && store.Id == "counter";

    private static bool ShouldStubTypedAction(string actionName, CounterStore store)
        => actionName == nameof(CounterStore.Increment) && store.Id == "counter" && store.Count >= 12;

    private static bool ShouldStubFactoryAction(string actionName, CounterStore store)
        => actionName == nameof(CounterStore.Decrement) && store.Id == "counter" && store.Count >= 18;

    private static Delegate WrapSpy(Delegate? callback)
        => callback ?? ((Action)Noop);

    private static Action WrapActionSpy(Action? callback)
        => callback ?? Noop;

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
