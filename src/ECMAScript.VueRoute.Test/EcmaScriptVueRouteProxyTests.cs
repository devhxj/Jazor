using System.Reflection;
using ECMAScript;

namespace ECMAScriptVueRouteTest;

#pragma warning disable CA1416

[TestClass]
public sealed class EcmaScriptVueRouteProxyTests
{
    [TestMethod]
    public void VueRoute_ImportHost_UsesVueRouterImport()
    {
        AssertEcmaScriptImport(typeof(VueRoute), "npm:vue-router@4");
    }

    [TestMethod]
    public void VueRoute_CoreRuntimeShapes_DoNotExposeObject()
    {
        var runtimeTypes = new[]
        {
            typeof(VueRoute),
            typeof(Router),
            typeof(RouterHistory),
            typeof(RouterOptions),
            typeof(RouteLocationNormalizedLoaded),
            typeof(RouteLocationResolved),
            typeof(RouteLocationMatched),
            typeof(RouteLocationAsPath),
            typeof(RouteLocationAsRelative),
            typeof(RouteRecordBase),
            typeof(RouteRecordSingleView),
            typeof(RouteRecordMultipleViews),
            typeof(RouteRecordRedirect),
            typeof(UseLinkOptions),
            typeof(UseLinkResult),
            typeof(RouterLinkProps),
            typeof(RouterViewProps),
            typeof(RouterViewSlotScope),
            typeof(RouteMeta),
            typeof(LocationQuery),
            typeof(LocationQueryRaw),
            typeof(RouteParams),
            typeof(RouteParamsRaw),
            typeof(RouteComponents),
            typeof(RouteNamedProps),
            typeof(NavigationFailure)
        };

        foreach (var type in runtimeTypes)
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                AssertNotObject(property.PropertyType, $"{type.Name}.{property.Name}");

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
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
    public void VueRoute_StaticApi_ExposesExpectedHighFrequencySurface()
    {
        var methods = typeof(VueRoute)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .ToArray();

        RequiredStatic(methods, nameof(VueRoute.CreateRouter), static method =>
            method.ReturnType == typeof(Router) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouterOptions) }));
        RequiredStatic(methods, nameof(VueRoute.CreateWebHistory), static method =>
            method.ReturnType == typeof(RouterHistory) &&
            method.GetParameters().Length == 0);
        RequiredStatic(methods, nameof(VueRoute.CreateWebHistory), static method =>
            method.ReturnType == typeof(RouterHistory) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }));
        RequiredStatic(methods, nameof(VueRoute.CreateWebHashHistory), static method =>
            method.ReturnType == typeof(RouterHistory) &&
            method.GetParameters().Length == 0);
        RequiredStatic(methods, nameof(VueRoute.CreateMemoryHistory), static method =>
            method.ReturnType == typeof(RouterHistory) &&
            method.GetParameters().Length == 0);
        RequiredStatic(methods, nameof(VueRoute.UseRouter), static method =>
            method.ReturnType == typeof(Router) &&
            method.GetParameters().Length == 0);
        RequiredStatic(methods, nameof(VueRoute.UseRoute), static method =>
            method.ReturnType == typeof(RouteLocationNormalizedLoaded) &&
            method.GetParameters().Length == 0);
        RequiredStatic(methods, nameof(VueRoute.UseLink), static method =>
            method.ReturnType == typeof(UseLinkResult) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(UseLinkOptions) }));
        RequiredStatic(methods, nameof(VueRoute.OnBeforeRouteLeave), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(NavigationGuardHandler) }));
        RequiredStatic(methods, nameof(VueRoute.OnBeforeRouteUpdate), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(NavigationGuardHandler) }));

        var routerLink = typeof(VueRoute).GetProperty(nameof(VueRoute.RouterLink), BindingFlags.Public | BindingFlags.Static);
        var routerView = typeof(VueRoute).GetProperty(nameof(VueRoute.RouterView), BindingFlags.Public | BindingFlags.Static);

        Assert.IsNotNull(routerLink);
        Assert.IsNotNull(routerView);
        Assert.AreEqual(typeof(Vue3.IVueComponent<RouterLinkProps, RouterLinkSlots>), routerLink!.PropertyType);
        Assert.AreEqual(typeof(Vue3.IVueComponent<RouterViewProps, RouterViewSlots>), routerView!.PropertyType);
    }

    [TestMethod]
    public void VueRoute_RouteComponent_UsesInlineFactory_ForConcreteComponents_AndImplicitLoaderUnion()
    {
        var from = typeof(RouteComponent).GetMethod(nameof(RouteComponent.From), BindingFlags.Public | BindingFlags.Static);
        var implicitLoader = typeof(RouteComponent)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(static method => method.Name == "op_Implicit");

        Assert.IsNotNull(from);
        Assert.AreEqual(typeof(RouteComponent), from!.ReturnType);
        Assert.AreEqual("__arg1", from.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        CollectionAssert.AreEqual(new[] { typeof(ECMAScript.VueContract.IVueComponent) }, from.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());

        Assert.AreEqual(typeof(RouteComponent), implicitLoader.ReturnType);
        CollectionAssert.AreEqual(new[] { typeof(RouteComponentLoader) }, implicitLoader.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
    }

    [TestMethod]
    public void VueRoute_RuntimeSupportTypes_UseEcmaScriptMarkers()
    {
        AssertEcmaScriptSupport(typeof(RouteMeta));
        AssertEcmaScriptSupport(typeof(LocationQuery));
        AssertEcmaScriptSupport(typeof(LocationQueryRaw));
        AssertEcmaScriptSupport(typeof(RouteParams));
        AssertEcmaScriptSupport(typeof(RouteParamsRaw));
        AssertEcmaScriptSupport(typeof(RouteComponents));
        AssertEcmaScriptSupport(typeof(RouteNamedProps));
        AssertEcmaScriptSupport(typeof(RouteRecordName));
        AssertEcmaScriptSupport(typeof(RouteRecordAlias));
        AssertEcmaScriptSupport(typeof(RouteLocationRaw));
        AssertEcmaScriptSupport(typeof(RouteComponent));
        AssertEcmaScriptSupport(typeof(RouteRecordProps));
        AssertEcmaScriptSupport(typeof(NavigationGuardReturn));
        AssertEcmaScriptSupport(typeof(NavigationGuardHandler));
        AssertEcmaScriptSupport(typeof(RouteRecordBeforeEnter));
        AssertEcmaScriptSupport(typeof(RouteRedirectOption));
        AssertEcmaScriptSupport(typeof(RouteRecordRaw));
        AssertEcmaScriptSupport(typeof(RouteParam));
        AssertEcmaScriptSupport(typeof(RouteParamRaw));
        AssertEcmaScriptSupport(typeof(LocationQueryValue));
        AssertEcmaScriptSupport(typeof(LocationQueryValueRaw));
    }

    private static MethodInfo RequiredStatic(MethodInfo[] methods, string name, Func<MethodInfo, bool> predicate)
    {
        var match = methods.SingleOrDefault(method => method.Name == name && predicate(method));
        Assert.IsNotNull(match, $"Missing expected static method: {name}");
        return match!;
    }

    private static void AssertNotObject(Type type, string message)
    {
        Assert.AreNotEqual(typeof(object), UnwrapNullable(type), message);

        if (!type.IsGenericType)
            return;

        foreach (var argument in type.GetGenericArguments())
            AssertNotObject(argument, message);
    }

    private static Type UnwrapNullable(Type type)
        => Nullable.GetUnderlyingType(type) ?? type;

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

#pragma warning restore CA1416
