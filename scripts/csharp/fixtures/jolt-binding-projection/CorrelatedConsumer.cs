using ECMAScript;
using Query = Npm.Tanstack.VueQuery.Exports;
using Vee = Npm.VeeValidate.Exports;
using VueUse = Npm.Vueuse.Core.Exports;

[ECMAScriptModule("correlated-consumer.mjs")]
public static class CorrelatedConsumer
{
    public static string ReadStorage()
    {
        var configured = VueUse.UseStorage("named-storage", "configured", options: new() { WriteDefaults = false });
        var defaults = VueUse.UseStorage("default-storage", "default");
        return configured.Value + "/" + defaults.Value;
    }

    public sealed record Contact([property: ECMAScriptName("address")] string EmailAddress, double Count);
    public sealed record Input([property: ECMAScriptName("contact")] Contact? Details) : Vue.VueProps;
    public sealed record Page(string Label);
    public sealed record SelectedPage(double Cursor, string Caption);

    public static string WriteForm()
    {
        var form = Vee.UseForm<Input, Input>(new() {
            InitialValues = new(new("initial", 1)),
            ValidationSchema = new() {
                Type = Vee.TypedSchemaDiscriminator.VVTypedSchema,
                Parse = values => Promise<Vee.TypedSchemaResult<Input>>.Resolve(
                    new Vee.TypedSchemaResult<Input> { Value = values, Errors = [] }),
            },
        });
        form.SetFieldValue(value => value.Details!.EmailAddress, "updated", false);
        form.SetFieldValue(value => value.Details!.Count, 0, false);
        return form.Values.Details!.EmailAddress + "/" + form.Values.Details.Count;
    }

    public static void ReadVariables(System.Action<double?> observe)
    {
        var mutation = Query.UseMutation(new Query.UseMutationBooleanValueOptionsInput<double> {
            MutationFn = value => Promise<bool>.Resolve(value > 0),
        });
        double? variables = mutation.Variables.Value;
        observe(variables);
        Vue.OnServerPrefetch(() => mutation.MutateAsync(0).Then(_ => observe(mutation.Variables.Value)));
    }

    public static void ReadPageParams(System.Action<double, string> observe, System.Action<bool, bool> observeStatus)
    {
        var infinite = Query.UseInfiniteQuery(new Query.UseInfiniteQueryNumberSelectedOptionsInput<Page, SelectedPage> {
            QueryKey = ["correlated-consumer"],
            InitialPageParam = 0,
            QueryFn = context => Promise<Page>.Resolve(new Page("first")),
            GetNextPageParam = (lastPage, pages, lastPageParam, pageParams) => null,
            Select = data => {
                double pageParam = data.PageParams[0];
                return new SelectedPage(pageParam, data.Pages[0].Label);
            },
        });
        observeStatus(infinite.IsPending.Value, infinite.IsSuccess.Value);
        Vue.OnServerPrefetch(() => infinite.Refetch().Then(() => {
            SelectedPage result = infinite.Data.Value!;
            observe(result.Cursor, result.Caption);
            observeStatus(infinite.IsPending.Value, infinite.IsSuccess.Value);
        }));
    }
}
