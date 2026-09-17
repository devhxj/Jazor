using System.Reflection;

using ECMAScript;
using ECMAScript.Contract;

#pragma warning disable CA1416

namespace ECMAScriptVeeValidateTest;

/// <summary>
/// Reflection checks for the vee-validate host surface: import host, strongly typed shapes, and
/// the repository rule that public parameters and returns never degrade to object.
/// 宿主表面反射检查：import 宿主、强类型形状与禁用 object 万能参数的仓库规则。
/// </summary>
[TestClass]
public sealed class VeeValidateProxyTests
{
    [TestMethod]
    public void VeeValidate_ImportHost_UsesTheAuthorEntry()
    {
        var runtime = typeof(VeeValidate).GetCustomAttribute<ECMAScriptAttribute>();
        Assert.IsNotNull(runtime);
        Assert.AreEqual("vee-validate", runtime!.Import);
        Assert.AreEqual(Transform.Import, runtime.Transform);
    }

    [TestMethod]
    public void VeeValidate_RuntimeShapes_DoNotExposeObject()
    {
        var runtimeTypes = new[]
        {
            typeof(VeeValidate), typeof(VeeValidateValidationResult), typeof(VeeValidateFieldMeta),
            typeof(VeeValidateFormMeta), typeof(VeeValidateInvalidSubmitContext),
            typeof(VeeValidateFormOptions<string>), typeof(VeeValidateFieldOptions<string>),
            typeof(VeeValidateConfig), typeof(VeeValidateFormReturn<string>),
            typeof(VeeValidateFieldReturn<string>), typeof(VeeValidateFieldArrayReturn<string>),
            typeof(VeeValidateFieldArrayEntry<string>), typeof(VeeValidateFormResetState),
            typeof(VeeValidateFormResetState<string>)
        };

        foreach (var type in runtimeTypes)
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                // An erased-value union's IUnion.Value is intentionally object-typed; the typed
                // branch surface carries the contract. 联合的 IUnion.Value 按契约就是 object。
                if (property.Name == nameof(System.Runtime.CompilerServices.IUnion.Value))
                    continue;
                AssertNotObject(property.PropertyType, $"{type.Name}.{property.Name}");
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         .Where(static method => !method.IsSpecialName)
                         .Where(static method => method.Name is not ("Equals" or "GetHashCode" or "ToString" or "PrintMembers" or "<Clone>$")))
            {
                AssertNotObject(method.ReturnType, $"{type.Name}.{method.Name} return");
                foreach (var parameter in method.GetParameters())
                    AssertNotObject(parameter.ParameterType, $"{type.Name}.{method.Name}({parameter.Name})");
            }

            foreach (var constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                foreach (var parameter in constructor.GetParameters())
                    AssertNotObject(parameter.ParameterType, $"{type.Name}..ctor({parameter.Name})");
            }
        }
    }

    [TestMethod]
    public void VeeValidate_StaticApi_ExposesTheFirstSliceSurface()
    {
        var methods = typeof(VeeValidate)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .ToArray();

        // 泛型方法的返回类型是开放构造类型（含方法泛型参数），因此比较泛型类型定义而非封闭构造类型。
        RequiredStatic(methods, nameof(VeeValidate.UseForm), static method =>
            method.IsGenericMethodDefinition &&
            IsGenericReturnOf(method, typeof(VeeValidateFormReturn<>)) &&
            method.GetParameters().Length == 1);
        RequiredStatic(methods, nameof(VeeValidate.UseFormContext), static method =>
            method.IsGenericMethodDefinition &&
            IsGenericReturnOf(method, typeof(VeeValidateFormReturn<>)) &&
            method.GetParameters().Length == 0);
        RequiredStatic(methods, nameof(VeeValidate.UseField), static method =>
            method.GetParameters().Length == 3 && method.IsGenericMethodDefinition &&
            IsGenericReturnOf(method, typeof(VeeValidateFieldReturn<>)));
        RequiredStatic(methods, nameof(VeeValidate.UseFieldArray), static method =>
            method.IsGenericMethodDefinition &&
            IsGenericReturnOf(method, typeof(VeeValidateFieldArrayReturn<>)) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }));
        RequiredStatic(methods, nameof(VeeValidate.Configure), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(VeeValidateConfig) }));

        // 表单/字段状态读取全部返回强类型引用，不使用 object 兜底。
        RequiredStatic(methods, nameof(VeeValidate.UseFormErrors), static method =>
            method.ReturnType == typeof(Vue.VueComputedRef<Vue.VueDictionary<string>>));
        RequiredStatic(methods, nameof(VeeValidate.UseIsFormValid), static method =>
            method.ReturnType == typeof(Vue.VueComputedRef<bool>));
        RequiredStatic(methods, nameof(VeeValidate.UseIsSubmitting), static method =>
            method.ReturnType == typeof(Vue.VueComputedRef<bool>));
        RequiredStatic(methods, nameof(VeeValidate.UseSubmitCount), static method =>
            method.ReturnType == typeof(Vue.VueComputedRef<Number>));
        RequiredStatic(methods, nameof(VeeValidate.UseSetFormErrors), static method =>
            method.ReturnType == typeof(VeeValidateSetFormErrorsHandler));
        RequiredStatic(methods, nameof(VeeValidate.UseResetForm), static method =>
            method.ReturnType == typeof(VeeValidateResetFormHandler));
        RequiredStatic(methods, nameof(VeeValidate.UseValidateForm), static method =>
            method.ReturnType == typeof(VeeValidateValidateHandler));

        // useSubmitForm 同时提供同步与异步两个强类型重载。
        Assert.AreEqual(2, methods.Count(static method => method.Name == nameof(VeeValidate.UseSubmitForm)),
            "useSubmitForm must expose both the sync and async callback overloads.");
    }

    [TestMethod]
    public void VeeValidate_FormReturn_ExposesMetaSubmitAndWriteEntries()
    {
        var formReturn = typeof(VeeValidateFormReturn<string>);
        Assert.AreEqual(typeof(Vue.VueComputedRef<Vue.VueDictionary<string>>), formReturn.GetProperty("Errors")!.PropertyType);
        Assert.AreEqual(typeof(Vue.VueComputedRef<VeeValidateFormMeta>), formReturn.GetProperty("Meta")!.PropertyType);
        Assert.AreEqual(typeof(Vue.VueComputedRef<bool>), formReturn.GetProperty("IsSubmitting")!.PropertyType);
        Assert.AreEqual(typeof(PromiseResult<VeeValidateValidationResult>), formReturn.GetMethod("Validate")!.ReturnType);

        // handleSubmit 是同步/异步重载族，按首参类型区分，不以无参 GetMethod 取。
        var handleSubmit = formReturn.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(static method => method.Name == "HandleSubmit")
            .ToArray();
        Assert.AreEqual(2, handleSubmit.Length, "handleSubmit must expose the sync and async overload families.");
        foreach (var overload in handleSubmit)
            Assert.AreEqual(typeof(VeeValidateSubmitHandler), overload.ReturnType);
    }

    [TestMethod]
    public void VeeValidate_FieldReturn_ExposesWritableValueAndErrors()
    {
        var fieldReturn = typeof(VeeValidateFieldReturn<string>);
        Assert.AreEqual(typeof(Vue.IVueRef<string>), fieldReturn.GetProperty("Value")!.PropertyType);
        Assert.AreEqual(typeof(Vue.VueComputedRef<string>), fieldReturn.GetProperty("ErrorMessage")!.PropertyType);
        Assert.AreEqual(typeof(Vue.VueComputedRef<VeeValidateFieldMeta>), fieldReturn.GetProperty("Meta")!.PropertyType);
        Assert.AreEqual(typeof(PromiseResult<VeeValidateValidationResult>), fieldReturn.GetMethod("Validate")!.ReturnType);
    }

    [TestMethod]
    public void VeeValidate_FieldArrayReturn_UsesStableEntryKeys()
    {
        var arrayReturn = typeof(VeeValidateFieldArrayReturn<string>);
        var entry = typeof(VeeValidateFieldArrayEntry<string>);

        Assert.AreEqual(typeof(Number), entry.GetProperty("Key")!.PropertyType);
        Assert.AreEqual(typeof(Vue.IVueRef<string>), entry.GetProperty("Value")!.PropertyType);
        Assert.AreEqual(typeof(bool), entry.GetProperty("IsFirst")!.PropertyType);
        Assert.AreEqual(typeof(Vue.VueShallowRef<VeeValidateFieldArrayEntry<string>[]>), arrayReturn.GetProperty("Fields")!.PropertyType);
        Assert.AreEqual(typeof(Vue.VueComputedRef<string>), arrayReturn.GetProperty("Path")!.PropertyType);
    }

    private static bool IsGenericReturnOf(MethodInfo method, Type openGeneric)
        => method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == openGeneric;

    private static void RequiredStatic(MethodInfo[] methods, string name, Func<MethodInfo, bool> predicate)
    {
        var matches = methods.Where(method => method.Name == name && predicate(method)).ToArray();
        Assert.IsTrue(matches.Length >= 1, $"Expected a strongly typed '{name}' overload, found {matches.Length}.");
    }

    private static void AssertNotObject(Type type, string location)
        => Assert.IsFalse(type == typeof(object), $"{location} must stay strongly typed; object is not permitted on this surface.");
}
