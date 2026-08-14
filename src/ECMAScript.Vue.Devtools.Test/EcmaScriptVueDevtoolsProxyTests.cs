using System.Reflection;
using RuntimeDescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace ECMAScript.VueDevtoolsTest;

#pragma warning disable CA1416

[TestClass]
public sealed class EcmaScriptVueDevtoolsProxyTests
{
    [TestMethod]
    public void VueDevtools_ImportHost_UsesOfficialPackage()
    {
        var runtime = typeof(VueDevtools).GetCustomAttribute<ECMAScriptAttribute>();

        Assert.IsNotNull(runtime);
        Assert.AreEqual("@vue/devtools-api", runtime!.Import);
        Assert.IsNull(typeof(VueDevtools).GetCustomAttribute<ECMAScriptModuleAttribute>());
    }

    [TestMethod]
    public void VueDevtools_StaticApi_ExposesOfficialPluginAuthoringSurface()
    {
        var methods = typeof(VueDevtools)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .ToArray();

        RequireMethod(methods, nameof(VueDevtools.SetupPlugin), 2, typeof(VueDevtools.PluginDescriptor), typeof(DevtoolsPluginSetupCallback));
        RequireMethod(methods, nameof(VueDevtools.SetupPlugin), 2, typeof(VueDevtools.PluginDescriptor<>), typeof(DevtoolsPluginSetupCallback<>));
        RequireMethod(methods, nameof(VueDevtools.AddCustomTab), 1, typeof(VueDevtools.CustomTab));
        RequireMethod(methods, nameof(VueDevtools.AddCustomCommand), 1, typeof(VueDevtools.CustomCommand));
        RequireMethod(methods, nameof(VueDevtools.RemoveCustomCommand), 1, typeof(string));
        RequireMethod(methods, nameof(VueDevtools.OnDevToolsConnected), 1, typeof(DevtoolsConnectionCallback));
        RequireMethod(methods, nameof(VueDevtools.OnDevToolsClientConnected), 1, typeof(DevtoolsConnectionCallback));

        AssertRuntimeName(typeof(VueDevtools), nameof(VueDevtools.SetupPlugin), "@#setupDevToolsPlugin");
        AssertRuntimeName(typeof(VueDevtools), nameof(VueDevtools.AddCustomTab), "@#addCustomTab");
        AssertRuntimeName(typeof(VueDevtools), nameof(VueDevtools.AddCustomCommand), "@#addCustomCommand");
        AssertRuntimeName(typeof(VueDevtools), nameof(VueDevtools.RemoveCustomCommand), "@#removeCustomCommand");
        AssertRuntimeName(typeof(VueDevtools), nameof(VueDevtools.OnDevToolsConnected), "@#onDevToolsConnected");
        AssertRuntimeName(typeof(VueDevtools), nameof(VueDevtools.OnDevToolsClientConnected), "@#onDevToolsClientConnected");
    }

    [TestMethod]
    public void VueDevtools_PluginApi_ContainsInspectorTimelineComponentAndSettingsOperations()
    {
        var api = typeof(VueDevtools.PluginApi);
        var expected = new[]
        {
            "notifyComponentUpdate",
            "addInspector",
            "sendInspectorTree",
            "sendInspectorState",
            "selectInspectorNode",
            "visitComponentTree",
            "now",
            "addTimelineLayer",
            "addTimelineEvent",
            "getSettings",
            "getComponentInstances",
            "getComponentBounds",
            "getComponentName",
            "highlightElement",
            "unhighlightElement"
        };

        foreach (var member in expected)
        {
            Assert.IsTrue(
                api.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Any(candidate => candidate.GetCustomAttribute<RuntimeDescriptionAttribute>()?.Description == "@#" + member),
                $"PluginApi must map '{member}'.");
        }

        var getSettings = api.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(VueDevtools.PluginApi.GetSettings))
            .ToArray();
        Assert.AreEqual(4, getSettings.Length);
        Assert.IsTrue(getSettings.Any(static method => !method.IsGenericMethod && method.GetParameters().Length == 0));
        Assert.IsTrue(getSettings.Any(static method => !method.IsGenericMethod && method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) })));
        Assert.IsTrue(getSettings.Any(static method => method.IsGenericMethodDefinition && method.GetParameters().Length == 0));
        Assert.IsTrue(getSettings.Any(static method => method.IsGenericMethodDefinition && method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) })));

        var hooks = typeof(VueDevtools.PluginHooks)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(static method => method.GetCustomAttribute<RuntimeDescriptionAttribute>()?.Description)
            .ToArray();
        CollectionAssert.AreEquivalent(
            new[]
            {
                "@#visitComponentTree",
                "@#inspectComponent",
                "@#editComponentState",
                "@#getInspectorTree",
                "@#getInspectorState",
                "@#editInspectorState",
                "@#inspectTimelineEvent",
                "@#timelineCleared",
                "@#setPluginSettings"
            },
            hooks!);
    }

    [TestMethod]
    public void VueDevtools_PublicRuntimeShapes_DoNotExposeObjectCatchAlls()
    {
        var runtimeTypes = new[]
        {
            typeof(VueDevtools.PluginDescriptor),
            typeof(VueDevtools.PluginDescriptor<TestSettings>),
            typeof(VueDevtools.PluginSettings),
            typeof(VueDevtools.PluginSettingsValues),
            typeof(VueDevtools.BooleanPluginSettingOptions),
            typeof(VueDevtools.BooleanPluginSetting),
            typeof(VueDevtools.ChoicePluginSettingOptions),
            typeof(VueDevtools.ChoicePluginSetting),
            typeof(VueDevtools.TextPluginSettingOptions),
            typeof(VueDevtools.TextPluginSetting),
            typeof(VueDevtools.PluginApi),
            typeof(VueDevtools.PluginApi<TestSettings>),
            typeof(VueDevtools.PluginHooks),
            typeof(VueDevtools.InspectorOptions),
            typeof(VueDevtools.InspectorNode),
            typeof(VueDevtools.InspectorStateEntry),
            typeof(VueDevtools.ComponentStateEntry),
            typeof(VueDevtools.EditStatePayload),
            typeof(VueDevtools.GetInspectorTreePayload),
            typeof(VueDevtools.GetInspectorStatePayload),
            typeof(VueDevtools.SetPluginSettingsPayload),
            typeof(VueDevtools.SetPluginSettingsPayload<TestSettings>),
            typeof(VueDevtools.ComponentTreeNode),
            typeof(VueDevtools.InspectedComponentData),
            typeof(VueDevtools.VisitComponentTreePayload),
            typeof(VueDevtools.InspectComponentPayload),
            typeof(VueDevtools.TimelineLayerOptions<TestTimelineData, TestTimelineMeta>),
            typeof(VueDevtools.TimelineEventOptions<TestTimelineData, TestTimelineMeta>),
            typeof(VueDevtools.ScreenshotOverlayEvent<TestTimelineData, TestTimelineMeta>),
            typeof(VueDevtools.InspectTimelineEventPayload<TestTimelineData, TestTimelineMeta>),
            typeof(VueDevtools.CustomTab),
            typeof(VueDevtools.IframeViewOptions),
            typeof(VueDevtools.VNodeViewOptions),
            typeof(VueDevtools.SfcViewOptions),
            typeof(VueDevtools.CustomCommand),
            typeof(VueDevtools.CustomCommandChild),
            typeof(VueDevtools.CustomCommandUrlActionOptions),
            typeof(VueDevtools.CustomCommandUrlAction)
        };

        foreach (var type in runtimeTypes)
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (IsUnionValueProperty(property))
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
        }
    }

    [TestMethod]
    public void VueDevtools_ErasedValueUnions_UseNativeNet11Contracts()
    {
        AssertNet11UnionContract(typeof(VueDevtools.ChoiceValue), typeof(string), typeof(Number));
        AssertNet11UnionContract(typeof(VueDevtools.DevtoolsValue), typeof(string), typeof(bool), typeof(Number), typeof(BigInt), typeof(Vue.VueProps), typeof(Array<VueDevtools.DevtoolsValue?>));
        AssertNet11UnionContract(typeof(VueDevtools.StateChange), typeof(VueDevtools.EditStateValue), typeof(VueDevtools.RemoveState));
        AssertNet11UnionContract(typeof(VueDevtools.ComponentIdentifier), typeof(string), typeof(Number));
        AssertNet11UnionContract(typeof(VueDevtools.TimelineGroupId), typeof(string), typeof(Number));
        AssertNet11UnionContract(typeof(VueDevtools.ModuleView), typeof(VueDevtools.IframeView), typeof(VueDevtools.VNodeView), typeof(VueDevtools.SfcView));
        AssertNet11UnionContract(typeof(VueDevtools.ScreenshotOverlayRenderResult), typeof(HTMLElement), typeof(string), typeof(bool));

        var none = typeof(VueDevtools.ScreenshotOverlayRenderResult).GetMethod(nameof(VueDevtools.ScreenshotOverlayRenderResult.None), BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(none);
        Assert.AreEqual("false", none!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
    }

    [TestMethod]
    public void VueDevtools_CustomCommandsRemainSingleLevel_AndUseOfficialUrlAction()
    {
        var child = typeof(VueDevtools.CustomCommandChild);
        Assert.IsNull(child.GetProperty("Children", BindingFlags.Public | BindingFlags.Instance));

        AssertFactoryInline(typeof(VueDevtools.BooleanPluginSetting), typeof(VueDevtools.BooleanPluginSettingOptions), "boolean");
        AssertFactoryInline(typeof(VueDevtools.ChoicePluginSetting), typeof(VueDevtools.ChoicePluginSettingOptions), "choice");
        AssertFactoryInline(typeof(VueDevtools.TextPluginSetting), typeof(VueDevtools.TextPluginSettingOptions), "text");
        AssertFactoryInline(typeof(VueDevtools.IframeView), typeof(VueDevtools.IframeViewOptions), "iframe");
        AssertFactoryInline(typeof(VueDevtools.VNodeView), typeof(VueDevtools.VNodeViewOptions), "vnode");
        AssertFactoryInline(typeof(VueDevtools.SfcView), typeof(VueDevtools.SfcViewOptions), "sfc");
        AssertFactoryInline(typeof(VueDevtools.CustomCommandUrlAction), typeof(VueDevtools.CustomCommandUrlActionOptions), "url");
    }

    private static void RequireMethod(IReadOnlyList<MethodInfo> methods, string name, int parameterCount, params Type[] parameterTypes)
    {
        Assert.IsTrue(
            methods.Any(method =>
                method.Name == name &&
                method.GetParameters().Length == parameterCount &&
                method.GetParameters().Select(static parameter => parameter.ParameterType.IsGenericType ? parameter.ParameterType.GetGenericTypeDefinition() : parameter.ParameterType)
                    .SequenceEqual(parameterTypes)),
            $"Missing {name}({string.Join(", ", parameterTypes.Select(static type => type.Name))}).");
    }

    private static void AssertRuntimeName(Type type, string methodName, string expectedRuntimeName)
    {
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => method.Name == methodName)
            .ToArray();
        Assert.IsTrue(methods.Length > 0);
        Assert.IsTrue(methods.All(method => method.GetCustomAttribute<RuntimeDescriptionAttribute>()?.Description == expectedRuntimeName));
    }

    private static void AssertFactoryInline(Type resultType, Type optionsType, string discriminator)
    {
        var create = resultType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(create, resultType.FullName);
        Assert.AreEqual(resultType, create!.ReturnType);
        CollectionAssert.AreEqual(new[] { optionsType }, create.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(
            $"Object.assign({{ type: \"{discriminator}\" }}, __arg1)",
            create.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
    }

    private static void AssertNotObject(Type type, string message)
    {
        Assert.AreNotEqual(typeof(object), Nullable.GetUnderlyingType(type) ?? type, message);

        if (type.IsArray)
        {
            AssertNotObject(type.GetElementType()!, message);
            return;
        }

        if (!type.IsGenericType)
            return;

        foreach (var argument in type.GetGenericArguments())
        {
            if (!argument.IsGenericParameter)
                AssertNotObject(argument, message);
        }
    }

    private static void AssertNet11UnionContract(Type unionType, params Type[] constructorBranchTypes)
    {
        Assert.IsNotNull(unionType.GetCustomAttribute<System.Runtime.CompilerServices.UnionAttribute>(), unionType.FullName);
        Assert.IsTrue(typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(unionType), unionType.FullName);

        CollectionAssert.AreEquivalent(
            constructorBranchTypes,
            unionType
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Select(static constructor => constructor.GetParameters().SingleOrDefault()?.ParameterType)
                .Where(static type => type is not null)
                .ToArray(),
            unionType.FullName);
    }

    private static bool IsUnionValueProperty(PropertyInfo property)
        => property.Name == nameof(System.Runtime.CompilerServices.IUnion.Value) &&
           property.DeclaringType is not null &&
           typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(property.DeclaringType);
}

public sealed record TestSettings : Vue.VueProps
{
    public bool Verbose { get; init; }
}

public sealed record TestTimelineData : Vue.VueProps
{
    public string Name { get; init; } = "";
}

public sealed record TestTimelineMeta : Vue.VueProps
{
    public int Sequence { get; init; }
}

#pragma warning restore CA1416
