using ECMAScript;
using Query = Npm.Tanstack.VueQuery.Exports;
using Vee = Npm.VeeValidate.Exports;
using VueUse = Npm.Vueuse.Core.Exports;

[ECMAScriptModule("correlated-consumer.mjs")]
public static class CorrelatedConsumer
{
    public static UnknownValue?[] ReadVueUseEmit()
    {
        UnknownValue?[] captured = [];
        var model = VueUse.UseVModel(new UnmountInput(4d, false), p => p.Count, (name, payload) =>
        {
            if (name == "update:count")
                captured = payload;
        });
        model.Value = 0d;
        return captured;
    }

    public static string ReadStorage()
    {
        var configured = VueUse.UseStorage("named-storage", "configured", options: new() { WriteDefaults = false });
        var defaults = VueUse.UseStorage("default-storage", "default");
        return configured.Value + "/" + defaults.Value;
    }

    public sealed record Contact([property: ECMAScriptName("address")] string EmailAddress, double Count);
    public sealed record Input([property: ECMAScriptName("contact")] Contact? Details,
        [property: ECMAScriptName("contacts")] Contact[] Contacts) : Vue.VueProps;
    public sealed record Page(string Label);
    public sealed record SelectedPage(double Cursor, string Caption);
    public sealed record EmptyInput(double Count, bool Enabled) : Vue.VueProps;
    public sealed record FieldProps(
        [property: ECMAScriptName("readout")] double Readout,
        [property: ECMAScriptName("onBlur")] string OnBlur) : Vue.VueProps;

    public static IPromise<string> WriteEmptyForm()
    {
        var form = Vee.UseForm<EmptyInput>();
        var (count, props) = form.DefineField<double, FieldProps>(value => value.Count,
            state => new() {
                Props = new FieldProps(state.Value?.Value ?? -1, "lazy-blur"),
                ValidateOnBlur = false,
            });
        bool missing = form.Values.GetValue(value => value.Count) is null && count.Value is null &&
            props.Value.Readout == -1 && props.Value.OnBlur == "lazy-blur";
        form.SetFieldValue(value => value.Count, 0, false);
        double? zero = count.Value?.Value;
        double zeroReadout = props.Value.Readout;
        count.WriteValue = 1;
        bool submitted = false;
        var submit = form.HandleSubmit(values => {
            submitted = values.GetValue(value => value.Count) == 1 &&
                values.GetValue(value => value.Enabled) is null;
        });
        return submit().Then(() => {
            form.SetValues(new EmptyInput(2, false), false);
            return form.ValidateField(value => value.Count).Then(result =>
                missing && zero == 0 && zeroReadout == 0 && submitted && result.Valid &&
                form.ErrorBag.Value["missing"] is null && form.Values.GetValue(value => value.Count) == 2 &&
                form.Values.GetValue(value => value.Enabled) == false ? "missing/0/2" : "wrong");
        });
    }

    public static string WriteForm()
    {
        var form = Vee.UseForm<Input, Input>(new() {
            InitialValues = new(new("initial", 1), [new("row", 2)]),
            ValidationSchema = new() {
                Type = Vee.TypedSchemaDiscriminator.VVTypedSchema,
                Parse = values => Promise<Vee.TypedSchemaResult<Input>>.Resolve(
                    new Vee.TypedSchemaResult<Input> {
                        Value = new(values.Get(value => value.Details), values.Get(value => value.Contacts) ?? []),
                        Errors = [],
                    }),
            },
        });
        form.SetFieldValue(value => value.Details!.EmailAddress, "updated", false);
        form.SetFieldValue(value => value.Details!.Count, 0, false);
        var (count, props) = form.DefineField(value => value.Contacts[0].Count,
            new() { Label = "Contact count", ValidateOnBlur = false, ValidateOnModelUpdate = false });
        count.WriteValue = 3;
        props.Value.OnBlur();
        var (configured, customProps) = form.DefineField<double, FieldProps>(value => value.Contacts[0].Count,
            new() { Props = state => new FieldProps(state.Value?.Value ?? -1, "custom-blur") });
        double initialReadout = customProps.Value.Readout;
        configured.WriteValue = 4;
        return form.Values.Get(value => value.Details!.EmailAddress) + "/" +
            form.Values.GetValue(value => value.Details!.Count) + "/" +
            initialReadout + "/" + customProps.Value.Readout + "/" + customProps.Value.OnBlur;
    }

    public static IPromise<string> WriteSeededForm()
    {
        var form = Vee.UseForm<EmptyInput>(new() { InitialValues = new(0, false) });
        var initial = form.Values;
        bool seeded = initial.GetValue(value => value.Count) == 0 && initial.GetValue(value => value.Enabled) == false;
        form.SetFieldValue(value => value.Count, 5, false);
        var result = "missing";
        var submit = form.HandleSubmit((Vue.PartialRecord<EmptyInput> values) => {
            result = seeded && values.GetValue(value => value.Count) == 5 &&
                values.GetValue(value => value.Enabled) == false ? "0/false/5" : "wrong";
        });
        return submit().Then(() => result);
    }

    public sealed record UnmountInput([property: ECMAScriptName("count")] double Count, bool Enabled) : Vue.VueProps;

    public static System.Func<IPromise<string>> TrackSeededField()
    {
        var form = Vee.UseForm<UnmountInput>(new() { InitialValues = new(0, false) });
        return () => {
            var count = form.Values.GetValue(value => value.Count);
            var enabled = form.Values.GetValue(value => value.Enabled);
            var submitted = false;
            var submit = form.HandleSubmit(values => {
                submitted = values.GetValue(value => value.Count) == count &&
                    values.GetValue(value => value.Enabled) == false;
            });
            return submit().Then(() => submitted && enabled == false
                ? count == 0 ? "0/false" : count is null ? "missing/false" : "wrong count"
                : "wrong submission");
        };
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
