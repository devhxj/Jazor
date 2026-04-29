using System.Reflection;
using ECMAScript;
using ECMAScript.Vuetify;
using static ECMAScript.Vue;

namespace Jazor.ComplierTest;

#pragma warning disable CA1416

[TestClass]
public sealed class EcmaScriptVueProxyTests
{
    [TestMethod]
    public void Vue_CoreProxyMethods_DoNotExposeObject()
    {
        var proxyTypes = new[] { typeof(Vue), typeof(VueApp), typeof(VueSetupContext) };

        foreach (var method in proxyTypes.SelectMany(static type =>
            type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(static method => !method.IsSpecialName)))
        {
            AssertNotObject(method.ReturnType, $"{method.DeclaringType?.Name}.{method.Name} return");
            foreach (var parameter in method.GetParameters())
                AssertNotObject(parameter.ParameterType, $"{method.DeclaringType?.Name}.{method.Name}({parameter.Name})");
        }
    }

    [TestMethod]
    public void Vuetify_CreateVuetify_UsesStronglyTypedOptionsAndPlugin()
    {
        var overloads = typeof(Vuetify)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Vuetify.CreateVuetify))
            .OrderBy(static method => method.GetParameters().Length)
            .ToArray();

        Assert.AreEqual(2, overloads.Length);
        Assert.IsTrue(overloads.All(static method => method.ReturnType == typeof(VuetifyPlugin)));

        CollectionAssert.AreEqual(
            Array.Empty<Type>(),
            overloads[0].GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(
            new[] { typeof(VuetifyOptions) },
            overloads[1].GetParameters().Select(static parameter => parameter.ParameterType).ToArray());

        Assert.IsTrue(typeof(VuePlugin).IsAssignableFrom(typeof(VuetifyPlugin)));
        Assert.IsTrue(typeof(VuePluginOptions).IsAssignableFrom(typeof(VuetifyOptions)));
    }

    [TestMethod]
    public void Vue_ComponentOptions_UseNamedRenderAndSetupDelegates()
    {
        var setup = typeof(VueComponentOptions).GetProperty(nameof(VueComponentOptions.Setup), BindingFlags.Public | BindingFlags.Instance);
        var render = typeof(VueComponentOptions).GetProperty(nameof(VueComponentOptions.Render), BindingFlags.Public | BindingFlags.Instance);
        var emits = typeof(VueComponentOptions).GetProperty(nameof(VueComponentOptions.EmitNames), BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(setup);
        Assert.IsNotNull(render);
        Assert.IsNotNull(emits);
        Assert.AreEqual(typeof(VueSetupCallback), setup.PropertyType);
        Assert.AreEqual(typeof(VueRenderCallback), render.PropertyType);
        Assert.AreEqual(typeof(string[]), emits.PropertyType);
    }

    [TestMethod]
    public void Vue_GenericComponentOptions_UseTypedSetupAndExplicitContracts()
    {
        var componentOptions = typeof(VueComponentOptions<>).MakeGenericType(typeof(TestVueProps));
        var setup = componentOptions.GetProperty("Setup", BindingFlags.Public | BindingFlags.Instance);
        var propNames = componentOptions.GetProperty("PropNames", BindingFlags.Public | BindingFlags.Instance);
        var emitNames = componentOptions.GetProperty("EmitNames", BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(setup);
        Assert.IsNotNull(propNames);
        Assert.IsNotNull(emitNames);
        Assert.AreEqual(typeof(VueTypedSetupCallback<TestVueProps>), setup.PropertyType);
        Assert.AreEqual(typeof(string[]), propNames.PropertyType);
        Assert.AreEqual(typeof(string[]), emitNames.PropertyType);
        var contractAssembly = typeof(ECMAScript.Contract.IUIComponent).Assembly;
        Assert.AreEqual(
            "ECMAScript.Contract.RecordLiteralContractAttribute",
            contractAssembly.GetType("ECMAScript.Contract.PropsAttribute")?.BaseType?.FullName);
        Assert.AreEqual(
            "ECMAScript.Contract.RecordLiteralContractAttribute",
            contractAssembly.GetType("ECMAScript.Contract.EmitsAttribute")?.BaseType?.FullName);
        CollectionAssert.Contains(
            propNames.CustomAttributes.Select(static attribute => attribute.AttributeType.FullName).ToArray(),
            "ECMAScript.Contract.PropsAttribute");
        CollectionAssert.Contains(
            emitNames.CustomAttributes.Select(static attribute => attribute.AttributeType.FullName).ToArray(),
            "ECMAScript.Contract.EmitsAttribute");
    }

    [TestMethod]
    public void Vue_GenericComponentOptionsWithSlots_BindTypedComponentAndContextContracts()
    {
        var componentOptions = typeof(VueComponentOptions<,>).MakeGenericType(typeof(TestVueProps), typeof(TestVueSlots));
        var setup = componentOptions.GetProperty("Setup", BindingFlags.Public | BindingFlags.Instance);
        var setupContext = typeof(VueSetupContext<>).MakeGenericType(typeof(TestVueSlots));
        var slots = setupContext.GetProperty("Slots", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var typedComponent = typeof(ECMAScript.Vue.IVueComponent<,>).MakeGenericType(typeof(TestVueProps), typeof(TestVueSlots));
        var defineComponentOverload = typeof(Vue)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Single(static method =>
                method.Name == nameof(Vue.DefineComponent) &&
                method.IsGenericMethodDefinition &&
                method.GetGenericArguments().Length == 2);

        Assert.IsNotNull(setup);
        Assert.IsNotNull(slots);
        Assert.AreEqual(typeof(VueTypedSetupCallback<TestVueProps, TestVueSlots>), setup.PropertyType);
        Assert.AreEqual(typeof(TestVueSlots), slots.PropertyType);
        Assert.IsTrue(typeof(ECMAScript.Vue.IVueComponent<TestVueProps>).IsAssignableFrom(typedComponent));
        Assert.IsTrue(typeof(ECMAScript.Vue.IVueComponent).IsAssignableFrom(typedComponent));

        var parameters = defineComponentOverload.GetParameters();
        Assert.AreEqual(1, parameters.Length);
        Assert.AreEqual(typeof(VueComponentOptions<,>), parameters[0].ParameterType.GetGenericTypeDefinition());
        Assert.AreEqual(typeof(ECMAScript.Vue.IVueComponent<,>), defineComponentOverload.ReturnType.GetGenericTypeDefinition());
    }

    [TestMethod]
    public void Vue_SlotComponentOptions_BindTypedSlotOnlyContracts()
    {
        var componentOptions = typeof(VueSlotComponentOptions<>).MakeGenericType(typeof(TestVueSlots));
        var setup = componentOptions.GetProperty("Setup", BindingFlags.Public | BindingFlags.Instance);
        var slotComponent = typeof(ECMAScript.Vue.IVueSlotComponent<>).MakeGenericType(typeof(TestVueSlots));
        var defineComponentOverload = typeof(Vue)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Single(static method =>
                method.Name == nameof(Vue.DefineComponent) &&
                method.IsGenericMethodDefinition &&
                method.GetGenericArguments().Length == 1 &&
                method.GetParameters()[0].ParameterType.IsGenericType &&
                method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueSlotComponentOptions<>));

        Assert.IsNotNull(setup);
        Assert.AreEqual(typeof(VueTypedSlotSetupCallback<TestVueSlots>), setup.PropertyType);
        Assert.IsTrue(typeof(ECMAScript.Vue.IVueComponent).IsAssignableFrom(slotComponent));

        var parameters = defineComponentOverload.GetParameters();
        Assert.AreEqual(1, parameters.Length);
        Assert.AreEqual(typeof(VueSlotComponentOptions<>), parameters[0].ParameterType.GetGenericTypeDefinition());
        Assert.AreEqual(typeof(ECMAScript.Vue.IVueSlotComponent<>), defineComponentOverload.ReturnType.GetGenericTypeDefinition());
    }

    [TestMethod]
    public void Vue_H_UsesTypedComponentSlotContracts()
    {
        var slotInvoke = typeof(VueSlotCallback).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var scopedSlotInvoke = typeof(VueSlotCallback<>)
            .MakeGenericType(typeof(string))
            .GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var slotOverloads = typeof(Vue)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Vue.H))
            .ToArray();

        Assert.IsNotNull(slotInvoke);
        Assert.IsNotNull(scopedSlotInvoke);
        Assert.AreEqual(typeof(IVNode), slotInvoke.ReturnType);
        Assert.AreEqual(typeof(IVNode), scopedSlotInvoke.ReturnType);
        CollectionAssert.AreEqual(
            new[] { typeof(string) },
            scopedSlotInvoke.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.IsTrue(typeof(VueSlots).IsAbstract);
        Assert.IsTrue(slotOverloads.Any(static method =>
        {
            var parameters = method.GetParameters();
            return !method.IsGenericMethodDefinition &&
                   parameters.Length == 2 &&
                   parameters[0].ParameterType == typeof(ECMAScript.Vue.IVueComponent) &&
                   parameters[1].ParameterType == typeof(VueSlots);
        }));
        Assert.IsTrue(slotOverloads.Any(static method =>
        {
            var parameters = method.GetParameters();
            return !method.IsGenericMethodDefinition &&
                   parameters.Length == 3 &&
                   parameters[0].ParameterType == typeof(ECMAScript.Vue.IVueComponent) &&
                   parameters[1].ParameterType == typeof(VueProps) &&
                   parameters[2].ParameterType == typeof(VueSlots);
        }));
        Assert.IsTrue(slotOverloads.Any(static method =>
        {
            if (!method.IsGenericMethodDefinition || method.GetGenericArguments().Length != 1)
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 2 &&
                   parameters[0].ParameterType.IsGenericType &&
                   parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue.IVueSlotComponent<>) &&
                   parameters[1].ParameterType.IsGenericParameter;
        }));
        Assert.IsTrue(slotOverloads.Any(static method =>
        {
            if (!method.IsGenericMethodDefinition || method.GetGenericArguments().Length != 2)
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 2 &&
                   parameters[0].ParameterType.IsGenericType &&
                   parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue.IVueComponent<,>) &&
                   parameters[1].ParameterType.IsGenericParameter;
        }));
        Assert.IsTrue(slotOverloads.Any(static method =>
        {
            if (!method.IsGenericMethodDefinition || method.GetGenericArguments().Length != 2)
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 3 &&
                   parameters[0].ParameterType.IsGenericType &&
                   parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue.IVueComponent<,>) &&
                   parameters[1].ParameterType.IsGenericParameter &&
                   parameters[2].ParameterType.IsGenericParameter;
        }));
    }

    [TestMethod]
    public void Vue_H_ExposesSingleVNodeChildOverloads()
    {
        var overloads = typeof(Vue)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Vue.H) && !method.IsGenericMethodDefinition)
            .Select(static method => method.GetParameters().Select(static parameter => parameter.ParameterType).ToArray())
            .ToArray();

        Assert.IsTrue(overloads.Any(static parameters =>
            parameters.Length == 2 &&
            parameters[0] == typeof(string) &&
            parameters[1] == typeof(IVNode)));
        Assert.IsTrue(overloads.Any(static parameters =>
            parameters.Length == 3 &&
            parameters[0] == typeof(string) &&
            parameters[1] == typeof(VueProps) &&
            parameters[2] == typeof(IVNode)));
        Assert.IsTrue(overloads.Any(static parameters =>
            parameters.Length == 2 &&
            parameters[0] == typeof(ECMAScript.Vue.IVueComponent) &&
            parameters[1] == typeof(IVNode)));
        Assert.IsTrue(overloads.Any(static parameters =>
            parameters.Length == 3 &&
            parameters[0] == typeof(ECMAScript.Vue.IVueComponent) &&
            parameters[1] == typeof(VueProps) &&
            parameters[2] == typeof(IVNode)));
    }

    [TestMethod]
    public void Vue_H_ExposesTypedDefaultSlotChildOverloads()
    {
        var overloads = typeof(Vue)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Vue.H) && method.IsGenericMethodDefinition)
            .ToArray();

        Assert.IsTrue(overloads.Any(static method =>
        {
            if (method.GetGenericArguments().Length != 1)
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 2 &&
                   parameters[0].ParameterType.IsGenericType &&
                   parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue.IVueSlotComponent<>) &&
                   parameters[1].ParameterType == typeof(IVNode);
        }));
        Assert.IsTrue(overloads.Any(static method =>
        {
            if (method.GetGenericArguments().Length != 1)
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 2 &&
                   parameters[0].ParameterType.IsGenericType &&
                   parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue.IVueSlotComponent<>) &&
                   parameters[1].ParameterType.IsGenericType &&
                   parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(Either<,,,,>);
        }));
        Assert.IsTrue(overloads.Any(static method =>
        {
            if (method.GetGenericArguments().Length != 2)
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 2 &&
                   parameters[0].ParameterType.IsGenericType &&
                   parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue.IVueComponent<,>) &&
                   parameters[1].ParameterType == typeof(IVNode);
        }));
        Assert.IsTrue(overloads.Any(static method =>
        {
            if (method.GetGenericArguments().Length != 2)
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 2 &&
                   parameters[0].ParameterType.IsGenericType &&
                   parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue.IVueComponent<,>) &&
                   parameters[1].ParameterType.IsGenericType &&
                   parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(Either<,,,,>);
        }));
        Assert.IsTrue(overloads.Any(static method =>
        {
            if (method.GetGenericArguments().Length != 2)
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 3 &&
                   parameters[0].ParameterType.IsGenericType &&
                   parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue.IVueComponent<,>) &&
                   parameters[1].ParameterType.IsGenericParameter &&
                   parameters[2].ParameterType == typeof(IVNode);
        }));
        Assert.IsTrue(overloads.Any(static method =>
        {
            if (method.GetGenericArguments().Length != 2)
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 3 &&
                   parameters[0].ParameterType.IsGenericType &&
                   parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue.IVueComponent<,>) &&
                   parameters[1].ParameterType.IsGenericParameter &&
                   parameters[2].ParameterType.IsGenericType &&
                   parameters[2].ParameterType.GetGenericTypeDefinition() == typeof(Either<,,,,>);
        }));
    }

    [TestMethod]
    public void Vuetify_ComponentExports_AreConcreteComponentTypes()
    {
        var exportedComponents = typeof(VuetifyComponents)
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .OrderBy(static property => property.Name, StringComparer.Ordinal)
            .ToArray();
        var registryProperties = typeof(VuetifyComponentRegistry)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(static property => property.Name, StringComparer.Ordinal);

        Assert.IsTrue(exportedComponents.Length > 0);
        foreach (var property in exportedComponents)
        {
            Assert.IsTrue(typeof(ECMAScript.Vue.IVueComponent).IsAssignableFrom(property.PropertyType), property.Name);
            Assert.IsTrue(typeof(IVuetifyComponent).IsAssignableFrom(property.PropertyType), property.Name);
            Assert.AreEqual(property.PropertyType, registryProperties[property.Name].PropertyType.UnwrapNullable(), property.Name);
        }
    }

    [TestMethod]
    public void Vuetify_DirectiveExports_AreConcreteDirectiveTypes()
    {
        var exportedDirectives = typeof(VuetifyDirectives)
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .OrderBy(static property => property.Name, StringComparer.Ordinal)
            .ToArray();
        var registryProperties = typeof(VuetifyDirectiveRegistry)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(static property => property.Name, StringComparer.Ordinal);

        Assert.IsTrue(exportedDirectives.Length > 0);
        foreach (var property in exportedDirectives)
        {
            Assert.IsTrue(typeof(VuetifyDirective).IsAssignableFrom(property.PropertyType), property.Name);
            Assert.IsTrue(typeof(VueDirective).IsAssignableFrom(property.PropertyType), property.Name);
            Assert.AreEqual(property.PropertyType, registryProperties[property.Name].PropertyType.UnwrapNullable(), property.Name);
        }
    }

    [TestMethod]
    public void Vuetify_OptionsAndRegistries_DoNotExposeObjectProperties()
    {
        var optionTypes = typeof(VuetifyOptions).Assembly
            .GetTypes()
            .Where(static type =>
                type.Namespace == "ECMAScript.Vuetify" &&
                (type.Name.EndsWith("Options", StringComparison.Ordinal) ||
                 type.Name.EndsWith("Registry", StringComparison.Ordinal)))
            .OrderBy(static type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.IsTrue(optionTypes.Length > 0);
        foreach (var property in optionTypes.SelectMany(static type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)))
        {
            Assert.AreNotEqual(typeof(object), property.PropertyType.UnwrapNullable(), $"{property.DeclaringType?.Name}.{property.Name}");
        }
    }

    private static void AssertNotObject(Type type, string message)
    {
        Assert.AreNotEqual(typeof(object), type.UnwrapNullable(), message);

        if (!type.IsGenericType)
            return;

        foreach (var argument in type.GetGenericArguments())
            AssertNotObject(argument, message);
    }

    private static void AssertModule(Type type, string expectedModule, string expectedName)
    {
        var module = type.GetCustomAttribute<ECMAScriptModuleAttribute>();
        var name = type.GetCustomAttribute<ECMAScriptNameAttribute>();

        Assert.IsNotNull(module, type.FullName);
        Assert.IsNotNull(name, type.FullName);
        Assert.AreEqual(expectedModule, module.Export, type.FullName);
        Assert.AreEqual(expectedName, name.Name, type.FullName);
    }
}

internal static class TypeTestExtensions
{
    public static Type UnwrapNullable(this Type type)
        => Nullable.GetUnderlyingType(type) ?? type;
}

public sealed record TestVueProps : VueProps;

public sealed record TestVueSlots : VueSlots;

#pragma warning restore CA1416
