using ECMAScript;
using VueProps = ECMAScript.Vue.VueProps;
using Vee = Npm.VeeValidate.Exports;

public static class OverloadOperationConsumer
{
    public sealed record Input(string Email, double Count) : VueProps;
    public sealed record Output(double Length) : VueProps;

    public static void Field1()
    {
        var result = Vee.UseField<string>("email");
        _ = result.Name;
        _ = result.Value.Value;
        result.SetValue("new", false);
    }

    public static void Field2()
    {
        var result = Vee.UseField<string>("email", "required");
        _ = result.Name;
        _ = result.Value.Value;
        result.SetValue("new", false);
    }

    public static void Field3()
    {
        var result = Vee.UseField<string>("email", "required", new() { InitialValue = "" });
        _ = result.Name;
        _ = result.Value.Value;
        result.SetValue("new", false);
    }

    public static void Array()
    {
        var result = Vee.UseFieldArray<string>("emails");
        result.Insert(0, "");
        result.Prepend("");
        result.Push("");
        result.Replace([""]);
        result.Update(0, "");
    }

    public static void Unseeded()
    {
        var result = Vee.UseForm<Input>();
        _ = result.Name;
        _ = result.ErrorBag.Value;
        _ = result.IsSubmitting.Value;
        _ = result.IsValidating.Value;
        _ = result.Meta.Value.Dirty;
        _ = result.SubmitCount.Value;
        _ = result.Values;
        result.DefineField(value => value.Email);
        result.HandleSubmit((ECMAScript.Vue.PartialRecord<Input> value) => { _ = value.Get(item => item.Email); });
        result.ResetForm();
        result.SetErrors(new() { { (Input value) => value.Email, "invalid" } });
        result.SetFieldError(value => value.Email, "invalid");
        result.SetFieldTouched(value => value.Email, true);
        result.SetFieldValue(value => value.Email, "new", false);
        result.SetValues(new Input("new", 0));
        _ = result.ValidateField(value => value.Email);
    }

    public static void Seeded()
    {
        var result = Vee.UseForm<Input, Output>(new() {
            InitialValues = new("", 0),
            ValidationSchema = new() {
                Type = Vee.TypedSchemaDiscriminator.VVTypedSchema,
                Parse = value => Promise<Vee.TypedSchemaResult<Output>>.Resolve(
                    new Vee.TypedSchemaResult<Output> { Value = new((value.Get(input => input.Email) ?? "").Length), Errors = [] }),
            },
        });
        _ = result.Name;
        _ = result.ErrorBag.Value;
        _ = result.IsSubmitting.Value;
        _ = result.IsValidating.Value;
        _ = result.Meta.Value.Dirty;
        _ = result.SubmitCount.Value;
        _ = result.Values;
        result.DefineField(value => value.Email);
        result.HandleSubmit((Output value) => { _ = value.Length; });
        result.ResetForm();
        result.SetErrors(new() { { (Input value) => value.Email, "invalid" } });
        result.SetFieldError(value => value.Email, "invalid");
        result.SetFieldTouched(value => value.Email, true);
        result.SetFieldValue(value => value.Email, "new", false);
        result.SetValues(new Input("new", 0));
        _ = result.ValidateField(value => value.Email);
    }

    public static void Context()
    {
        var result = Vee.UseFormContext<Input, Output>();
        if (result is null) return;
        _ = result.Name;
        _ = result.ErrorBag.Value;
        _ = result.IsSubmitting.Value;
        _ = result.IsValidating.Value;
        _ = result.Meta.Value.Dirty;
        _ = result.SubmitCount.Value;
        _ = result.Values;
        result.DefineField(value => value.Email);
        result.HandleSubmit((ECMAScript.Vue.PartialRecord<Output> value) => { _ = value.GetValue(item => item.Length); });
        result.ResetForm();
        result.SetErrors(new() { { (Input value) => value.Email, "invalid" } });
        result.SetFieldError(value => value.Email, "invalid");
        result.SetFieldTouched(value => value.Email, true);
        result.SetFieldValue(value => value.Email, "new", false);
        result.SetValues(new Input("new", 0));
        _ = result.ValidateField(value => value.Email);
    }

    public static string? SeededInput()
    {
        var result = Vee.UseForm<Input>(new() { InitialValues = new("initial", 0) });
        result.HandleSubmit((ECMAScript.Vue.PartialRecord<Input> value) => { _ = value.Get(item => item.Email); });
        return result.Values.Get(item => item.Email);
    }
}
