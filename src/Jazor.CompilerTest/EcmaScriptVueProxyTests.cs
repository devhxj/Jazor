using System.Reflection;
using ECMAScript;
using ECMAScript.Contract;
using ECMAScript.Vuetify;
using static ECMAScript.Vue3;

namespace Jazor.ComplierTest;

#pragma warning disable CA1416

[TestClass]
public sealed class EcmaScriptVueProxyTests
{
    [TestMethod]
    public void Vue_CoreProxyMethods_DoNotExposeObject()
    {
        var proxyTypes = new[] { typeof(Vue3), typeof(VueApp), typeof(VueSetupContext) };

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
    public void Vuetify_ImportHosts_UseEcmaScriptImports_InsteadOfModuleEntryMarkers()
    {
        AssertEcmaScriptImport(typeof(Vuetify), "npm:vuetify");
        AssertEcmaScriptImport(typeof(VuetifyComponents), "vuetify/components");
        AssertEcmaScriptImport(typeof(VuetifyDirectives), "vuetify/directives");
    }

    [TestMethod]
    public void Vuetify_RuntimeShapes_UseEcmaScriptSupportMarkers_WithoutModuleGeneration()
    {
        var runtimeShapes = new[]
        {
            typeof(IVuetifyComponent),
            typeof(VuetifyPlugin),
            typeof(VuetifyOptions),
            typeof(VuetifyThemeOptions),
            typeof(VuetifyThemeVariationOptions),
            typeof(VuetifyDisplayOptions),
            typeof(VuetifyDisplayThresholds),
            typeof(VuetifyIconOptions),
            typeof(VuetifyLocaleOptions),
            typeof(VuetifyDateOptions),
            typeof(VuetifyComponentRegistry),
            typeof(VuetifyDirectiveRegistry),
            typeof(VuetifyDirective)
        };

        foreach (var type in runtimeShapes)
            AssertEcmaScriptSupport(type);
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
    public void Vue_SpreadAttribute_And_VueObject_Surface_ArePublicAndTyped()
    {
        var spreadUsage = typeof(SpreadAttribute).GetCustomAttribute<AttributeUsageAttribute>();
        var vueObjectType = typeof(VueObject);
        var typedVueObjectType = typeof(VueObject<>);
        var vueDictionaryType = typeof(VueDictionary<>);
        var attrs = typeof(VueObject).GetProperty(nameof(VueObject.Attrs), BindingFlags.Public | BindingFlags.Instance);
        var dataset = typeof(VueObject).GetProperty(nameof(VueObject.Dataset), BindingFlags.Public | BindingFlags.Instance);
        var raw = typeof(VueObject).GetProperty(nameof(VueObject.Raw), BindingFlags.Public | BindingFlags.Instance);
        var @class = typeof(VueObject).GetProperty(nameof(VueObject.Class), BindingFlags.Public | BindingFlags.Instance);
        var props = typeof(VueObject<>).GetProperty("Props", BindingFlags.Public | BindingFlags.Instance);
        var indexer = vueDictionaryType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(spreadUsage);
        Assert.AreEqual(AttributeTargets.Property, spreadUsage.ValidOn);
        Assert.AreEqual(false, spreadUsage.AllowMultiple);
        Assert.AreEqual("ECMAScript", typeof(SpreadAttribute).Namespace);
        Assert.IsFalse(vueObjectType.IsAbstract);
        Assert.IsFalse(typedVueObjectType.IsAbstract);
        Assert.IsFalse(vueDictionaryType.IsAbstract);
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(vueObjectType));
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(typedVueObjectType));
        Assert.IsTrue(typeof(VueProps).IsAssignableFrom(vueDictionaryType));
        Assert.IsNotNull(attrs);
        Assert.IsNotNull(dataset);
        Assert.IsNotNull(raw);
        Assert.IsNotNull(@class);
        Assert.IsNotNull(props);
        Assert.IsNotNull(indexer);
        CollectionAssert.AreEqual(new[] { typeof(string) }, indexer.GetIndexParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.Contains(
            attrs.CustomAttributes.Select(static attribute => attribute.AttributeType).ToArray(),
            typeof(SpreadAttribute));
        CollectionAssert.Contains(
            dataset.CustomAttributes.Select(static attribute => attribute.AttributeType).ToArray(),
            typeof(SpreadAttribute));
        CollectionAssert.Contains(
            raw.CustomAttributes.Select(static attribute => attribute.AttributeType).ToArray(),
            typeof(SpreadAttribute));
        CollectionAssert.Contains(
            props.CustomAttributes.Select(static attribute => attribute.AttributeType).ToArray(),
            typeof(SpreadAttribute));
        Assert.IsTrue(@class.PropertyType.UnwrapNullable().IsGenericType);
        Assert.AreEqual(typeof(Either<,,,>), @class.PropertyType.UnwrapNullable().GetGenericTypeDefinition());
    }

    [TestMethod]
    public void Vue_DictionaryAndValueHelpers_DoNotExposeObject()
    {
        AssertNotObject(typeof(VueDictionary<string>), nameof(VueDictionary<string>));
        AssertNotObject(typeof(VueDictionary<Either<string, string[]>>), "VueDictionary<Either<string, string[]>>");
        AssertNotObject(typeof(VueStyleValue), nameof(VueStyleValue));
        AssertNotObject(typeof(VueClassItem), nameof(VueClassItem));
    }

    [TestMethod]
    public void Vue_CreateApp_And_CreateSsrApp_ExposeTypedRootPropsOverloads()
    {
        var methods = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name is nameof(Vue3.CreateApp) or nameof(Vue3.CreateSsrApp))
            .ToArray();

        static bool HasTypedRootPropsOverload(MethodInfo[] methods, string methodName, int genericArity, Func<ParameterInfo[], bool> predicate)
            => methods.Any(method =>
                method.Name == methodName &&
                method.IsGenericMethodDefinition &&
                method.GetGenericArguments().Length == genericArity &&
                predicate(method.GetParameters()));

        Assert.IsTrue(HasTypedRootPropsOverload(
            methods,
            nameof(Vue3.CreateApp),
            1,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType.IsGenericParameter));

        Assert.IsTrue(HasTypedRootPropsOverload(
            methods,
            nameof(Vue3.CreateApp),
            1,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>)));

        Assert.IsTrue(HasTypedRootPropsOverload(
            methods,
            nameof(Vue3.CreateApp),
            2,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericParameter));

        Assert.IsTrue(HasTypedRootPropsOverload(
            methods,
            nameof(Vue3.CreateApp),
            2,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>)));

        Assert.IsTrue(HasTypedRootPropsOverload(
            methods,
            nameof(Vue3.CreateSsrApp),
            1,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType.IsGenericParameter));

        Assert.IsTrue(HasTypedRootPropsOverload(
            methods,
            nameof(Vue3.CreateSsrApp),
            1,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>)));

        Assert.IsTrue(HasTypedRootPropsOverload(
            methods,
            nameof(Vue3.CreateSsrApp),
            2,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericParameter));

        Assert.IsTrue(HasTypedRootPropsOverload(
            methods,
            nameof(Vue3.CreateSsrApp),
            2,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>)));
    }

    [TestMethod]
    public void Vue_GenericComponentOptions_UseTypedSetupAndExplicitContracts()
    {
        var componentOptions = typeof(VueComponentOptions<>).MakeGenericType(typeof(TestVueProps));
        var setup = componentOptions.GetProperty("Setup", BindingFlags.Public | BindingFlags.Instance);
        var propNames = componentOptions.GetProperty("PropNames", BindingFlags.Public | BindingFlags.Instance);
        var emitNames = componentOptions.GetProperty("EmitNames", BindingFlags.Public | BindingFlags.Instance);
        var propsUsage = typeof(PropsAttribute).GetCustomAttribute<AttributeUsageAttribute>();
        var emitsUsage = typeof(EmitsAttribute).GetCustomAttribute<AttributeUsageAttribute>();
        var propsDefaults = new PropsAttribute();
        var emitsDefaults = new EmitsAttribute();

        Assert.IsNotNull(setup);
        Assert.IsNotNull(propNames);
        Assert.IsNotNull(emitNames);
        Assert.AreEqual(typeof(VueTypedSetupCallback<TestVueProps>), setup.PropertyType);
        Assert.AreEqual(typeof(string[]), propNames.PropertyType);
        Assert.AreEqual(typeof(string[]), emitNames.PropertyType);
        Assert.IsNotNull(propsUsage);
        Assert.IsNotNull(emitsUsage);
        Assert.AreEqual(AttributeTargets.Property, propsUsage.ValidOn);
        Assert.AreEqual(AttributeTargets.Property, emitsUsage.ValidOn);
        Assert.AreEqual(false, propsUsage.AllowMultiple);
        Assert.AreEqual(false, emitsUsage.AllowMultiple);
        Assert.AreEqual(PropsAttribute.DefaultTypeArgumentIndex, propsDefaults.TypeArgumentIndex);
        Assert.AreEqual(EmitsAttribute.DefaultSourceMemberName, emitsDefaults.SourceMemberName);
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
        var typedComponent = typeof(ECMAScript.Vue3.IVueComponent<,>).MakeGenericType(typeof(TestVueProps), typeof(TestVueSlots));
        var defineComponentOverload = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Single(static method =>
                method.Name == nameof(Vue3.DefineComponent) &&
                method.IsGenericMethodDefinition &&
                method.GetGenericArguments().Length == 2);

        Assert.IsNotNull(setup);
        Assert.IsNotNull(slots);
        Assert.AreEqual(typeof(VueTypedSetupCallback<TestVueProps, TestVueSlots>), setup.PropertyType);
        Assert.AreEqual(typeof(TestVueSlots), slots.PropertyType);
        Assert.IsTrue(typeof(ECMAScript.Vue3.IVueComponent<TestVueProps>).IsAssignableFrom(typedComponent));
        Assert.IsTrue(typeof(ECMAScript.Vue3.IVueComponent).IsAssignableFrom(typedComponent));

        var parameters = defineComponentOverload.GetParameters();
        Assert.AreEqual(1, parameters.Length);
        Assert.AreEqual(typeof(VueComponentOptions<,>), parameters[0].ParameterType.GetGenericTypeDefinition());
        Assert.AreEqual(typeof(ECMAScript.Vue3.IVueComponent<,>), defineComponentOverload.ReturnType.GetGenericTypeDefinition());
    }

    [TestMethod]
    public void Vue_SlotComponentOptions_BindTypedSlotOnlyContracts()
    {
        var componentOptions = typeof(VueSlotComponentOptions<>).MakeGenericType(typeof(TestVueSlots));
        var setup = componentOptions.GetProperty("Setup", BindingFlags.Public | BindingFlags.Instance);
        var slotComponent = typeof(ECMAScript.Vue3.IVueSlotComponent<>).MakeGenericType(typeof(TestVueSlots));
        var defineComponentOverload = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Single(static method =>
                method.Name == nameof(Vue3.DefineComponent) &&
                method.IsGenericMethodDefinition &&
                method.GetGenericArguments().Length == 1 &&
                method.GetParameters()[0].ParameterType.IsGenericType &&
                method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(VueSlotComponentOptions<>));

        Assert.IsNotNull(setup);
        Assert.AreEqual(typeof(VueTypedSlotSetupCallback<TestVueSlots>), setup.PropertyType);
        Assert.IsTrue(typeof(ECMAScript.Vue3.IVueComponent).IsAssignableFrom(slotComponent));

        var parameters = defineComponentOverload.GetParameters();
        Assert.AreEqual(1, parameters.Length);
        Assert.AreEqual(typeof(VueSlotComponentOptions<>), parameters[0].ParameterType.GetGenericTypeDefinition());
        Assert.AreEqual(typeof(ECMAScript.Vue3.IVueSlotComponent<>), defineComponentOverload.ReturnType.GetGenericTypeDefinition());
    }

    [TestMethod]
    public void Vue_H_UsesTypedComponentSlotContracts()
    {
        var slotInvoke = typeof(VueSlotCallback).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var scopedSlotInvoke = typeof(VueSlotCallback<>)
            .MakeGenericType(typeof(string))
            .GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var slotOverloads = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Vue3.H))
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
                   parameters[0].ParameterType == typeof(ECMAScript.Vue3.IVueComponent) &&
                   parameters[1].ParameterType == typeof(VueSlots);
        }));
        Assert.IsTrue(slotOverloads.Any(static method =>
        {
            var parameters = method.GetParameters();
            return !method.IsGenericMethodDefinition &&
                   parameters.Length == 3 &&
                   parameters[0].ParameterType == typeof(ECMAScript.Vue3.IVueComponent) &&
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
                   parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueSlotComponent<>) &&
                   parameters[1].ParameterType.IsGenericParameter;
        }));
        Assert.IsTrue(slotOverloads.Any(static method =>
        {
            if (!method.IsGenericMethodDefinition || method.GetGenericArguments().Length != 2)
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 2 &&
                   parameters[0].ParameterType.IsGenericType &&
                   parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                   parameters[1].ParameterType.IsGenericParameter;
        }));
        Assert.IsTrue(slotOverloads.Any(static method =>
        {
            if (!method.IsGenericMethodDefinition || method.GetGenericArguments().Length != 2)
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 3 &&
                   parameters[0].ParameterType.IsGenericType &&
                   parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                   parameters[1].ParameterType.IsGenericParameter &&
                   parameters[2].ParameterType.IsGenericParameter;
        }));
    }

    [TestMethod]
    public void Vue_H_ExposesChildOverloads()
    {
        var overloads = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Vue3.H) && !method.IsGenericMethodDefinition)
            .Select(static method => method.GetParameters().Select(static parameter => parameter.ParameterType).ToArray())
            .ToArray();

        static bool HasOverload(Type[][] overloads, params Type[] signature)
            => overloads.Any(parameters => parameters.SequenceEqual(signature));

        var childTypes = new[]
        {
            typeof(IVNode),
            typeof(string),
            typeof(Number),
            typeof(bool),
            typeof(IVNode[]),
        };

        foreach (var childType in childTypes)
        {
            Assert.IsTrue(HasOverload(overloads, typeof(string), childType));
            Assert.IsTrue(HasOverload(overloads, typeof(string), typeof(VueProps), childType));
            Assert.IsTrue(HasOverload(overloads, typeof(ECMAScript.Vue3.IVueComponent), childType));
            Assert.IsTrue(HasOverload(overloads, typeof(ECMAScript.Vue3.IVueComponent), typeof(VueProps), childType));
        }

        Assert.IsFalse(overloads.Any(static parameters =>
            parameters.Any(static parameter =>
                parameter.IsGenericType &&
                parameter.GetGenericTypeDefinition() == typeof(Either<,,,,>))));
    }

    [TestMethod]
    public void Vue_H_ExposesTypedDefaultSlotChildOverloads()
    {
        var overloads = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Vue3.H) && method.IsGenericMethodDefinition)
            .ToArray();

        static bool HasGenericOverload(MethodInfo[] methods, int genericArity, params Func<ParameterInfo[], bool>[] predicates)
            => methods.Any(method =>
            {
                if (method.GetGenericArguments().Length != genericArity)
                    return false;

                var parameters = method.GetParameters();
                return predicates.Any(predicate => predicate(parameters));
            });

        var childTypes = new[]
        {
            typeof(IVNode),
            typeof(string),
            typeof(Number),
            typeof(bool),
            typeof(IVNode[]),
        };

        foreach (var childType in childTypes)
        {
            Assert.IsTrue(HasGenericOverload(
                overloads,
                1,
                parameters => parameters.Length == 2 &&
                              parameters[0].ParameterType.IsGenericType &&
                              parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueSlotComponent<>) &&
                              parameters[1].ParameterType == childType));

            Assert.IsTrue(HasGenericOverload(
                overloads,
                2,
                parameters => parameters.Length == 2 &&
                              parameters[0].ParameterType.IsGenericType &&
                              parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                              parameters[1].ParameterType == childType,
                parameters => parameters.Length == 3 &&
                              parameters[0].ParameterType.IsGenericType &&
                              parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                              parameters[1].ParameterType.IsGenericParameter &&
                              parameters[2].ParameterType == childType));
        }

        Assert.IsFalse(overloads.Any(static method =>
            method.GetParameters().Any(static parameter =>
                parameter.ParameterType.IsGenericType &&
                parameter.ParameterType.GetGenericTypeDefinition() == typeof(Either<,,,,>))));
    }

    [TestMethod]
    public void Vue_H_ExposesTypedVueObjectPropsOverloads()
    {
        var overloads = typeof(Vue3)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Vue3.H) && method.IsGenericMethodDefinition)
            .ToArray();

        static bool HasGenericOverload(MethodInfo[] methods, int genericArity, Func<ParameterInfo[], bool> predicate)
            => methods.Any(method =>
                method.GetGenericArguments().Length == genericArity &&
                predicate(method.GetParameters()));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            1,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            2,
            parameters => parameters.Length == 2 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            2,
            parameters => parameters.Length == 3 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>) &&
                          parameters[2].ParameterType == typeof(IVNode)));

        Assert.IsTrue(HasGenericOverload(
            overloads,
            2,
            parameters => parameters.Length == 3 &&
                          parameters[0].ParameterType.IsGenericType &&
                          parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>) &&
                          parameters[1].ParameterType.IsGenericType &&
                          parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(VueObject<>) &&
                          parameters[2].ParameterType.IsGenericParameter));
    }

    [TestMethod]
    public void Vue_DefaultSlotSugar_DoesNotDependOnJazorAttributes()
    {
        var vueType = typeof(Vue3);
        var hOverloads = vueType
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Vue3.H))
            .ToArray();

        Assert.IsFalse(HasAttribute(vueType, "JazorAttribute"));

        foreach (var method in hOverloads.Where(IsVueDefaultSlotSugarOverload))
            Assert.IsFalse(HasAttribute(method, "JazorAttribute"), method.ToString());

        static bool IsVueDefaultSlotSugarOverload(MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (parameters.Length is not (2 or 3))
                return false;

            var receiverType = parameters[0].ParameterType;
            var childType = parameters[^1].ParameterType;
            if (receiverType != typeof(ECMAScript.Vue3.IVueComponent) &&
                !(receiverType.IsGenericType &&
                  (receiverType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueSlotComponent<>) ||
                   receiverType.GetGenericTypeDefinition() == typeof(ECMAScript.Vue3.IVueComponent<,>))))
            {
                return false;
            }

            return childType == typeof(IVNode) ||
                   childType == typeof(string) ||
                   childType == typeof(Number) ||
                   childType == typeof(bool) ||
                   childType == typeof(IVNode[]);
        }
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
            Assert.IsTrue(typeof(ECMAScript.Vue3.IVueComponent).IsAssignableFrom(property.PropertyType), property.Name);
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

    private static void AssertEcmaScriptImport(Type type, string expectedImport)
    {
        var runtime = type.GetCustomAttribute<ECMAScriptAttribute>();
        var module = type.GetCustomAttribute<ECMAScriptModuleAttribute>();

        Assert.IsNotNull(runtime, type.FullName);
        Assert.IsNull(module, type.FullName);
        Assert.AreEqual(expectedImport, runtime!.Import, type.FullName);
    }

    private static void AssertEcmaScriptSupport(Type type)
    {
        var runtime = type.GetCustomAttribute<ECMAScriptAttribute>();
        var module = type.GetCustomAttribute<ECMAScriptModuleAttribute>();

        Assert.IsNotNull(runtime, type.FullName);
        Assert.IsNull(module, type.FullName);
        Assert.IsNull(runtime!.Import, type.FullName);
    }

    private static bool HasAttribute(MemberInfo member, string attributeTypeName)
        => member.GetCustomAttributesData().Any(attribute => attribute.AttributeType.Name == attributeTypeName);
}

internal static class TypeTestExtensions
{
    public static Type UnwrapNullable(this Type type)
        => Nullable.GetUnderlyingType(type) ?? type;
}

public sealed record TestVueProps : VueProps;

public sealed record TestVueSlots : VueSlots;

#pragma warning restore CA1416
