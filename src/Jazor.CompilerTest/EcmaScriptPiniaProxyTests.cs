using System.Reflection;
using ECMAScript;
using ECMAScript.Contract;

namespace Jazor.ComplierTest;

#pragma warning disable CA1416

[TestClass]
public sealed class EcmaScriptPiniaProxyTests
{
    [TestMethod]
    public void Pinia_ImportHost_UsesBarePiniaImport()
    {
        AssertEcmaScriptImport(typeof(Pinia), "pinia");
    }

    [TestMethod]
    public void Pinia_CreatePinia_ReturnsVuePluginCompatibleRootInstance()
    {
        var createPinia = typeof(Pinia)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Single(static method => method.Name == nameof(Pinia.CreatePinia));

        Assert.AreEqual(typeof(Pinia.PiniaInstance), createPinia.ReturnType);
        Assert.IsTrue(typeof(Vue3.VuePlugin).IsAssignableFrom(typeof(Pinia.PiniaInstance)));
    }

    [TestMethod]
    public void Pinia_StoreDefinition_UsesExplicitUseWrappers_ForCallableStoreFactories()
    {
        var storeDefinition = typeof(Pinia.StoreDefinition<>).MakeGenericType(typeof(Pinia.StoreGeneric));
        var methods = storeDefinition
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Pinia.StoreDefinition<Pinia.StoreGeneric>.Use))
            .OrderBy(static method => method.GetParameters().Length)
            .ToArray();

        CollectionAssert.AreEqual(
            new[] { 0, 1, 2 },
            methods.Select(static method => method.GetParameters().Length).ToArray());

        Assert.AreEqual("__arg1()", methods[0].GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1(__arg2)", methods[1].GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1(__arg2, __arg3)", methods[2].GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);

        var id = storeDefinition.GetProperty(nameof(Pinia.StoreDefinition<Pinia.StoreGeneric>.Id), BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(id);
        Assert.AreEqual(typeof(string), id!.PropertyType);
    }

    [TestMethod]
    public void Pinia_CoreRuntimeShapes_DoNotExposeObject()
    {
        var runtimeTypes = new Type[]
        {
            typeof(Pinia.PiniaInstance),
            typeof(Pinia.PiniaPluginContext),
            typeof(Pinia.StoreProperties),
            typeof(Pinia.StoreGeneric),
            typeof(Pinia.Store<>).MakeGenericType(typeof(TestPiniaState)),
            typeof(Pinia.SubscriptionMutation<>).MakeGenericType(typeof(TestPiniaState)),
            typeof(Pinia.StoreActionListenerContext),
            typeof(Pinia.StoreActionListenerContext<>).MakeGenericType(typeof(Pinia.StoreGeneric)),
            typeof(Pinia.StoreDefinition<>).MakeGenericType(typeof(Pinia.StoreGeneric)),
            typeof(Pinia.StoreRefs<>).MakeGenericType(typeof(Pinia.StoreGeneric)),
            typeof(Pinia.DefineStoreOptions<>).MakeGenericType(typeof(TestPiniaState)),
            typeof(Pinia.DefineSetupStoreOptions),
            typeof(Pinia.PiniaValue)
        };

        foreach (var type in runtimeTypes)
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                AssertNotObject(property.PropertyType, $"{type.Name}.{property.Name}");

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                         .Where(static method => !method.IsSpecialName)
                         .Where(static method =>
                             method.Name is not ("Equals" or "GetHashCode" or "ToString" or "PrintMembers" or "<Clone>$")))
            {
                AssertNotObject(method.ReturnType, $"{type.Name}.{method.Name} return");
                foreach (var parameter in method.GetParameters())
                    AssertNotObject(parameter.ParameterType, $"{type.Name}.{method.Name}({parameter.Name})");
            }
        }
    }

    [TestMethod]
    public void Pinia_StoreRuntimeSurface_IsTyped_And_UsesVueBridges()
    {
        var piniaInstanceState = typeof(Pinia.PiniaInstance).GetProperty(nameof(Pinia.PiniaInstance.State), BindingFlags.Public | BindingFlags.Instance);
        var storeState = typeof(Pinia.Store<>).MakeGenericType(typeof(TestPiniaState))
            .GetProperty(nameof(Pinia.Store<TestPiniaState>.State), BindingFlags.Public | BindingFlags.Instance);
        var subscribeOptions = typeof(Pinia.SubscribeOptions);

        Assert.IsNotNull(piniaInstanceState);
        Assert.IsNotNull(storeState);
        Assert.AreEqual(typeof(Vue3.IVueRef<Vue3.VueDictionary<Pinia.PiniaStateTree>>), piniaInstanceState!.PropertyType);
        Assert.AreEqual(typeof(TestPiniaState), storeState!.PropertyType);

        var detached = subscribeOptions.GetProperty(nameof(Pinia.SubscribeOptions.Detached), BindingFlags.Public | BindingFlags.Instance);
        var flush = subscribeOptions.GetProperty(nameof(Pinia.SubscribeOptions.Flush), BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(detached);
        Assert.IsNotNull(flush);
        Assert.AreEqual(typeof(bool?), detached!.PropertyType);
        Assert.AreEqual(typeof(Vue3.VueWatchFlush?), flush!.PropertyType);
    }

    [TestMethod]
    public void Pinia_RuntimeSupportTypes_UseEcmaScriptMarkers()
    {
        AssertEcmaScriptSupport(typeof(Pinia.StoreDefinition<>));
        AssertEcmaScriptSupport(typeof(Pinia.StoreRefs<>));
        AssertEcmaScriptSupport(typeof(Pinia.PiniaValue));
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
}

public sealed record TestPiniaState : Pinia.PiniaStateTree
{
    public int Count { get; init; }
}

#pragma warning restore CA1416
