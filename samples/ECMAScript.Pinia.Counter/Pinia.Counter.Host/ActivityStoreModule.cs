using System.ComponentModel;
using ECMAScript;
using static ECMAScript.Pinia;
using static ECMAScript.Vue;

namespace Pinia.Counter.Host;

[ECMAScript]
[Description("@#")]
public sealed record ActivityState : PiniaStateTree
{
    public int CompletedActions { get; set; }

    public int PendingReviews { get; set; }

    public string Highlight { get; set; } = "";
}

[ECMAScript]
[Description("@#")]
public abstract class ActivityStore : Store<ActivityState>
{
    public extern int CompletedActions { get; set; }

    public extern int PendingReviews { get; set; }

    public extern string Highlight { get; set; }

    public extern string Summary { get; }

    public extern void Capture(string source);

    public extern void QueueReview();
}

[ECMAScript]
[Description("@#")]
public sealed record ActivityGetters : Vue.VueProps
{
    public Func<string> Summary { get; init; } = default!;
}

[ECMAScript]
[Description("@#")]
public sealed record ActivityActions : Vue.VueProps
{
    public Action<string> Capture { get; init; } = default!;

    public Action QueueReview { get; init; } = default!;
}

[ECMAScriptModule("stores/activity-store.mjs")]
public static class ActivityStoreModule
{
    private const string ActivityStoreId = "activity";

    public static StoreDefinition<ActivityStore> UseActivityStore = DefineStore<ActivityStore, ActivityState>(
        ActivityStoreId,
        new DefineStoreOptions<ActivityState>
        {
            State = CreateState,
            Getters = new ActivityGetters
            {
                Summary = BindThis<ActivityStore, string>(ReadSummary)
            },
            Actions = new ActivityActions
            {
                Capture = BindThis<ActivityStore, string>(Capture),
                QueueReview = BindThis<ActivityStore>(QueueReview)
            }
        });

    private static ActivityState CreateState()
        => new()
        {
            CompletedActions = 1,
            PendingReviews = 2,
            Highlight = "Waiting for the next workflow capture."
        };

    private static string ReadSummary(ActivityStore self)
        => self.Highlight
            + " (done: "
            + self.CompletedActions
            + ", pending: "
            + self.PendingReviews
            + ")";

    private static void Capture(ActivityStore self, string source)
    {
        self.CompletedActions += 1;

        if (self.PendingReviews > 0)
        {
            self.PendingReviews -= 1;
        }

        self.Highlight = "capture(" + source + ") updated the activity store.";
    }

    private static void QueueReview(ActivityStore self)
    {
        self.PendingReviews += 1;
        self.Highlight = "queueReview() recorded another follow-up item.";
    }
}
