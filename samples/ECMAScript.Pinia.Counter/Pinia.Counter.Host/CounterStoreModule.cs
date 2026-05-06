using System.ComponentModel;
using ECMAScript;
using static ECMAScript.Pinia;
using static ECMAScript.Vue3;

namespace Pinia.Counter.Host;

[ECMAScript]
[Description("@#")]
public sealed record CounterState : PiniaStateTree
{
    public int Count { get; set; }

    public string Status { get; set; } = "";
}

[ECMAScript]
[Description("@#")]
public abstract class CounterStore : Store<CounterState>
{
    public extern int Count { get; set; }

    public extern string Status { get; set; }

    public extern int DoubleCount { get; }

    public extern void Increment();

    public extern void Decrement();
}

[ECMAScript]
[Description("@#")]
public abstract class CounterStoreRefs : StoreRefs<CounterStore>
{
    public extern Vue3.IVueRef<int> Count { get; }

    public extern Vue3.IVueRef<string> Status { get; }

    public extern Vue3.VueReadonlyRef<int> DoubleCount { get; }
}

[ECMAScript]
[Description("@#")]
public record CounterGetters : Vue3.VueProps
{
    public Func<int> DoubleCount { get; init; } = default!;
}

[ECMAScript]
[Description("@#")]
public record CounterActions : Vue3.VueProps
{
    public Action Increment { get; init; } = default!;

    public Action Decrement { get; init; } = default!;
}

[ECMAScriptModule("stores/counter-store.mjs")]
public static class CounterStoreModule
{
    private const int SeedCount = 2;

    public static StoreDefinition<CounterStore> UseCounterStore = DefineStore<CounterStore, CounterState>(
        "counter",
        new DefineStoreOptions<CounterState>
        {
            State = CreateState,
            Getters = new CounterGetters
            {
                DoubleCount = BindThis<CounterStore, int>(ReadDoubleCount)
            },
            Actions = new CounterActions
            {
                Increment = BindThis<CounterStore>(Increment),
                Decrement = BindThis<CounterStore>(Decrement)
            }
        });

    public static CounterStoreRefs UseCounterStoreRefs(CounterStore store)
        => StoreToRefs<CounterStoreRefs, CounterStore>(store);

    private static CounterState CreateState()
        => new()
        {
            Count = SeedCount,
            Status = "Store seeded through defineStore()."
        };

    private static int ReadDoubleCount(CounterStore self)
        => self.Count * 2;

    private static void Increment(CounterStore self)
    {
        self.Count += 1;
        self.Status = "increment() updated the store.";
    }

    private static void Decrement(CounterStore self)
    {
        if (self.Count > 0)
        {
            self.Count -= 1;
            self.Status = "decrement() updated the store.";
            return;
        }

        self.Status = "decrement() is clamped at zero.";
    }
}
