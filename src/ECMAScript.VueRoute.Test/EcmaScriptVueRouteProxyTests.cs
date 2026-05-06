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
            typeof(RouteLocationNormalized),
            typeof(RouteLocationNormalizedLoaded),
            typeof(RouteLocationResolved),
            typeof(RouteLocationMatched),
            typeof(RouteRecordNormalized),
            typeof(RouteLocation),
            typeof(RouteLocationOptions),
            typeof(RouteLocationAsPath),
            typeof(RouteLocationAsRelative),
            typeof(RouteRecordBase),
            typeof(RouteRecordSingleView),
            typeof(RouteRecordMultipleViews),
            typeof(RouteRecordRedirect),
            typeof(UseLinkOptions),
            typeof(UseLinkReturn),
            typeof(UseLinkResult),
            typeof(RouterLinkOptions),
            typeof(RouterLinkProps),
            typeof(RouterLinkSlotScope),
            typeof(RouterViewProps),
            typeof(RouterViewSlotScope),
            typeof(RouteMeta),
            typeof(HistoryState),
            typeof(LocationQuery),
            typeof(LocationQueryRaw),
            typeof(RouteParams),
            typeof(RouteParamsRaw),
            typeof(RouteLocationRawMaybeRef),
            typeof(RouteBooleanMaybeRef),
            typeof(HistoryStateValue),
            typeof(RawRouteComponents),
            typeof(RouteComponents),
            typeof(RouteNamedProps),
            typeof(NavigationFailure),
            typeof(RawRouteComponent),
            typeof(RouterHistoryNavigationInformation)
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
            method.ReturnType == typeof(UseLinkReturn) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(UseLinkOptions) }));
        RequiredStatic(methods, nameof(VueRoute.OnBeforeRouteLeave), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(NavigationGuardHandler) }));
        RequiredStatic(methods, nameof(VueRoute.OnBeforeRouteUpdate), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(NavigationGuardHandler) }));
        RequiredStatic(methods, nameof(VueRoute.IsNavigationFailure), static method =>
            method.ReturnType == typeof(bool) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Error) }));
        RequiredStatic(methods, nameof(VueRoute.IsNavigationFailure), static method =>
            method.ReturnType == typeof(bool) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Error), typeof(NavigationFailureType) }));
        RequiredStatic(methods, nameof(VueRoute.ParseQuery), static method =>
            method.ReturnType == typeof(LocationQuery) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }));
        RequiredStatic(methods, nameof(VueRoute.StringifyQuery), static method =>
            method.ReturnType == typeof(string) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(LocationQueryRaw) }));
        RequiredStatic(methods, nameof(VueRoute.LoadRouteLocation), static method =>
            method.ReturnType == typeof(IPromise<RouteLocationNormalizedLoaded>) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouteLocationNormalized) }));
        RequiredStatic(methods, nameof(VueRoute.LoadRouteLocation), static method =>
            method.ReturnType == typeof(IPromise<RouteLocationNormalizedLoaded>) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouteLocationResolved) }));

        var routerLink = typeof(VueRoute).GetProperty(nameof(VueRoute.RouterLink), BindingFlags.Public | BindingFlags.Static);
        var routerView = typeof(VueRoute).GetProperty(nameof(VueRoute.RouterView), BindingFlags.Public | BindingFlags.Static);
        var startLocation = typeof(VueRoute).GetProperty(nameof(VueRoute.START_LOCATION), BindingFlags.Public | BindingFlags.Static);

        Assert.IsNotNull(routerLink);
        Assert.IsNotNull(routerView);
        Assert.IsNotNull(startLocation);
        Assert.AreEqual(typeof(Vue3.IVueComponent<RouterLinkProps, RouterLinkSlots>), routerLink!.PropertyType);
        Assert.AreEqual(typeof(Vue3.IVueComponent<RouterViewProps, RouterViewSlots>), routerView!.PropertyType);
        Assert.AreEqual(typeof(RouteLocationNormalizedLoaded), startLocation!.PropertyType);
    }

    [TestMethod]
    public void VueRoute_NavigationFailureType_UsesOfficialBitFlags()
    {
        Assert.AreEqual(1, typeof(NavigationFailureType).GetCustomAttributes(typeof(FlagsAttribute), inherit: false).Length);
        CollectionAssert.AreEquivalent(
            new[]
            {
                nameof(NavigationFailureType.Aborted),
                nameof(NavigationFailureType.Cancelled),
                nameof(NavigationFailureType.Duplicated)
            },
            Enum.GetNames<NavigationFailureType>());
        CollectionAssert.AreEquivalent(
            new[] { 4, 8, 16 },
            Enum.GetValues<NavigationFailureType>().Select(static value => (int)value).ToArray());
    }

    [TestMethod]
    public void VueRoute_RouteComponent_UsesInlineFactory_AndRawRouteComponent_HandlesLazyLoaderUnion()
    {
        var from = typeof(RouteComponent).GetMethod(nameof(RouteComponent.From), BindingFlags.Public | BindingFlags.Static);
        var rawFrom = typeof(RawRouteComponent).GetMethod(nameof(RawRouteComponent.From), BindingFlags.Public | BindingFlags.Static);
        var implicitOperators = typeof(RawRouteComponent)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(static method => method.Name == "op_Implicit")
            .ToArray();
        var implicitComponent = implicitOperators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(RouteComponent));
        var implicitLoader = implicitOperators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(RouteComponentLoader));

        Assert.IsNotNull(from);
        Assert.IsNotNull(rawFrom);
        Assert.IsNotNull(implicitComponent);
        Assert.IsNotNull(implicitLoader);
        Assert.AreEqual(typeof(RouteComponent), from!.ReturnType);
        Assert.AreEqual("__arg1", from.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        CollectionAssert.AreEqual(new[] { typeof(ECMAScript.VueContract.IVueComponent) }, from.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());

        Assert.AreEqual(typeof(RawRouteComponent), rawFrom!.ReturnType);
        Assert.AreEqual("__arg1", rawFrom.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        CollectionAssert.AreEqual(new[] { typeof(ECMAScript.VueContract.IVueComponent) }, rawFrom.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());

        Assert.AreEqual(typeof(RawRouteComponent), implicitComponent!.ReturnType);
        CollectionAssert.AreEqual(new[] { typeof(RouteComponent) }, implicitComponent.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());

        Assert.AreEqual(typeof(RawRouteComponent), implicitLoader!.ReturnType);
        CollectionAssert.AreEqual(new[] { typeof(RouteComponentLoader) }, implicitLoader.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
    }

    [TestMethod]
    public void VueRoute_RuntimeSupportTypes_UseEcmaScriptMarkers()
    {
        AssertEcmaScriptSupport(typeof(RouteMeta));
        AssertEcmaScriptSupport(typeof(HistoryState));
        AssertEcmaScriptSupport(typeof(LocationQuery));
        AssertEcmaScriptSupport(typeof(LocationQueryRaw));
        AssertEcmaScriptSupport(typeof(RouteParams));
        AssertEcmaScriptSupport(typeof(RouteParamsRaw));
        AssertEcmaScriptSupport(typeof(RouteLocationOptions));
        AssertEcmaScriptSupport(typeof(RouteLocation));
        AssertEcmaScriptSupport(typeof(RouteLocationRawMaybeRef));
        AssertEcmaScriptSupport(typeof(RouteBooleanMaybeRef));
        AssertEcmaScriptSupport(typeof(HistoryStateValue));
        AssertEcmaScriptSupport(typeof(RawRouteComponents));
        AssertEcmaScriptSupport(typeof(RouteComponents));
        AssertEcmaScriptSupport(typeof(RouteNamedProps));
        AssertEcmaScriptSupport(typeof(RouteRecordName));
        AssertEcmaScriptSupport(typeof(RouteRecordAlias));
        AssertEcmaScriptSupport(typeof(RouteLocationRaw));
        AssertEcmaScriptSupport(typeof(UseLinkReturn));
        AssertEcmaScriptSupport(typeof(RouterLinkOptions));
        AssertEcmaScriptSupport(typeof(RouterLinkSlotScope));
        AssertEcmaScriptSupport(typeof(RouteComponent));
        AssertEcmaScriptSupport(typeof(RawRouteComponent));
        AssertEcmaScriptSupport(typeof(RouteRecordProps));
        AssertEcmaScriptSupport(typeof(NavigationGuardNextArgument));
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

    [TestMethod]
    public void VueRoute_RouterRuntimeSurface_ExposesProductionNavigationControls()
    {
        var routerType = typeof(Router);
        var listening = routerType.GetProperty(nameof(Router.Listening), BindingFlags.Public | BindingFlags.Instance);
        var methods = routerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => !method.IsSpecialName)
            .ToArray();

        Assert.IsNotNull(listening);
        Assert.AreEqual(typeof(bool), listening!.PropertyType);
        Assert.IsTrue(listening.CanRead);
        Assert.IsTrue(listening.CanWrite);

        RequiredInstance(methods, nameof(Router.GetRoutes), static method =>
            method.ReturnType == typeof(RouteRecordNormalized[]) &&
            method.GetParameters().Length == 0);
        RequiredInstance(methods, nameof(Router.ClearRoutes), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Length == 0);
        RequiredInstance(methods, nameof(Router.OnError), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouterErrorHandler) }));
        RequiredInstance(methods, nameof(Router.Resolve), static method =>
            method.ReturnType == typeof(RouteLocationResolved) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouteLocationRaw), typeof(RouteLocationNormalizedLoaded) }));
    }

    [TestMethod]
    public void VueRoute_RouterHistorySurface_ExposesTypedStateAndNavigationCallbacks()
    {
        var historyType = typeof(RouterHistory);
        var methods = historyType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => !method.IsSpecialName)
            .ToArray();
        var state = historyType.GetProperty(nameof(RouterHistory.State), BindingFlags.Public | BindingFlags.Instance);
        var pathState = typeof(RouteLocationAsPath).GetProperty(nameof(RouteLocationAsPath.State), BindingFlags.Public | BindingFlags.Instance);
        var relativeState = typeof(RouteLocationAsRelative).GetProperty(nameof(RouteLocationAsRelative.State), BindingFlags.Public | BindingFlags.Instance);
        var historyInfoType = typeof(RouterHistoryNavigationInformation);

        Assert.IsNotNull(state);
        Assert.IsNotNull(pathState);
        Assert.IsNotNull(relativeState);
        Assert.AreEqual(typeof(HistoryState), state!.PropertyType);
        Assert.AreEqual(typeof(HistoryState), pathState!.PropertyType);
        Assert.AreEqual(typeof(HistoryState), relativeState!.PropertyType);

        RequiredInstance(methods, nameof(RouterHistory.Push), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), typeof(HistoryState) }));
        RequiredInstance(methods, nameof(RouterHistory.Replace), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), typeof(HistoryState) }));
        RequiredInstance(methods, nameof(RouterHistory.Listen), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouterHistoryNavigationCallback) }));
        RequiredInstance(methods, nameof(RouterHistory.Go), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Number), typeof(bool) }));

        Assert.AreEqual(typeof(RouterHistoryNavigationType), historyInfoType.GetProperty(nameof(RouterHistoryNavigationInformation.Type))!.PropertyType);
        Assert.AreEqual(typeof(RouterHistoryNavigationDirection), historyInfoType.GetProperty(nameof(RouterHistoryNavigationInformation.Direction))!.PropertyType);
        Assert.AreEqual(typeof(Number), historyInfoType.GetProperty(nameof(RouterHistoryNavigationInformation.Delta))!.PropertyType);
        Assert.IsTrue(typeof(RouterHistoryNavigationType).IsDefined(typeof(StringAttribute), inherit: false));
        Assert.IsTrue(typeof(RouterHistoryNavigationDirection).IsDefined(typeof(StringAttribute), inherit: false));
        Assert.AreEqual(string.Empty, typeof(RouterHistoryNavigationDirection).GetField(nameof(RouterHistoryNavigationDirection.Unknown))!
            .GetCustomAttribute<ECMAScriptNameAttribute>()?.Name);
    }

    [TestMethod]
    public void VueRoute_RouteRecordAndMatchedContracts_DifferentiateRawAndLoadedComponents()
    {
        var routeRecordSingleViewComponent = typeof(RouteRecordSingleView).GetProperty(nameof(RouteRecordSingleView.Component), BindingFlags.Public | BindingFlags.Instance);
        var routeRecordMultipleViewsComponents = typeof(RouteRecordMultipleViews).GetProperty(nameof(RouteRecordMultipleViews.Components), BindingFlags.Public | BindingFlags.Instance);
        var routeLocationMatchedComponents = typeof(RouteLocationMatched)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Single(static property => property.Name == nameof(RouteLocationMatched.Components) && property.PropertyType == typeof(RouteComponents));

        Assert.IsNotNull(routeRecordSingleViewComponent);
        Assert.IsNotNull(routeRecordMultipleViewsComponents);
        Assert.IsNotNull(routeLocationMatchedComponents);
        Assert.AreEqual(typeof(RawRouteComponent), routeRecordSingleViewComponent!.PropertyType);
        Assert.AreEqual(typeof(RawRouteComponents), routeRecordMultipleViewsComponents!.PropertyType);
        Assert.AreEqual(typeof(RouteComponents), routeLocationMatchedComponents!.PropertyType);
        Assert.AreEqual(typeof(RouteRecordNormalized), typeof(RouteLocationMatched).BaseType);
    }

    [TestMethod]
    public void VueRoute_UseLinkAndRouterViewContracts_FollowOfficialReactiveAndSlotShapes()
    {
        var useLinkTo = typeof(UseLinkOptions).GetProperty(nameof(UseLinkOptions.To), BindingFlags.Public | BindingFlags.Instance);
        var useLinkReplace = typeof(UseLinkOptions).GetProperty(nameof(UseLinkOptions.Replace), BindingFlags.Public | BindingFlags.Instance);
        var useLinkMethod = typeof(VueRoute).GetMethod(
            nameof(VueRoute.UseLink),
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: new[] { typeof(UseLinkOptions) },
            modifiers: null);
        var useLinkReturnRoute = typeof(UseLinkReturn).GetProperty(nameof(UseLinkReturn.Route), BindingFlags.Public | BindingFlags.Instance);
        var routerLinkTo = typeof(RouterLinkOptions).GetProperty(nameof(RouterLinkOptions.To), BindingFlags.Public | BindingFlags.Instance);
        var routerLinkReplace = typeof(RouterLinkOptions).GetProperty(nameof(RouterLinkOptions.Replace), BindingFlags.Public | BindingFlags.Instance);
        var routerLinkScopeRoute = typeof(RouterLinkSlotScope).GetProperty(nameof(RouterLinkSlotScope.Route), BindingFlags.Public | BindingFlags.Instance);
        var routerLinkScopeHref = typeof(RouterLinkSlotScope).GetProperty(nameof(RouterLinkSlotScope.Href), BindingFlags.Public | BindingFlags.Instance);
        var routerLinkScopeNavigate = typeof(RouterLinkSlotScope).GetProperty(nameof(RouterLinkSlotScope.Navigate), BindingFlags.Public | BindingFlags.Instance);
        var routeProp = typeof(RouterViewProps).GetProperty(nameof(RouterViewProps.Route), BindingFlags.Public | BindingFlags.Instance);
        var slotComponent = typeof(RouterViewSlotScope).GetProperty(nameof(RouterViewSlotScope.Component), BindingFlags.Public | BindingFlags.Instance);
        var routeMaybeRefFromRef = typeof(RouteLocationRawMaybeRef).GetMethod(nameof(RouteLocationRawMaybeRef.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(Vue3.IVueRef<RouteLocationAsRelative>) });
        var routeMaybeRefFromReadonly = typeof(RouteLocationRawMaybeRef).GetMethod(nameof(RouteLocationRawMaybeRef.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(Vue3.VueReadonlyRef<RouteLocationAsRelative>) });
        var boolMaybeRefFromReadonly = typeof(RouteBooleanMaybeRef).GetMethod(nameof(RouteBooleanMaybeRef.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(Vue3.VueReadonlyRef<bool>) });

        Assert.IsNotNull(useLinkTo);
        Assert.IsNotNull(useLinkReplace);
        Assert.IsNotNull(useLinkMethod);
        Assert.IsNotNull(useLinkReturnRoute);
        Assert.IsNotNull(routerLinkTo);
        Assert.IsNotNull(routerLinkReplace);
        Assert.IsNotNull(routerLinkScopeRoute);
        Assert.IsNotNull(routerLinkScopeHref);
        Assert.IsNotNull(routerLinkScopeNavigate);
        Assert.IsNotNull(routeProp);
        Assert.IsNotNull(slotComponent);
        Assert.IsNotNull(routeMaybeRefFromRef);
        Assert.IsNotNull(routeMaybeRefFromReadonly);
        Assert.IsNotNull(boolMaybeRefFromReadonly);
        Assert.AreEqual(typeof(RouteLocationRawMaybeRef), useLinkTo!.PropertyType);
        Assert.AreEqual(typeof(RouteBooleanMaybeRef), Nullable.GetUnderlyingType(useLinkReplace!.PropertyType) ?? useLinkReplace.PropertyType);
        Assert.AreEqual(typeof(UseLinkReturn), useLinkMethod!.ReturnType);
        Assert.AreEqual(typeof(Vue3.VueReadonlyRef<RouteLocationResolved>), useLinkReturnRoute!.PropertyType);
        Assert.AreEqual(typeof(RouteLocationRaw), routerLinkTo!.PropertyType);
        Assert.AreEqual(typeof(bool), Nullable.GetUnderlyingType(routerLinkReplace!.PropertyType) ?? routerLinkReplace.PropertyType);
        Assert.AreEqual(typeof(RouteLocationResolved), routerLinkScopeRoute!.PropertyType);
        Assert.AreEqual(typeof(string), routerLinkScopeHref!.PropertyType);
        Assert.AreEqual(typeof(RouterLinkNavigateCallback), routerLinkScopeNavigate!.PropertyType);
        Assert.AreEqual(typeof(RouteLocationNormalized), routeProp!.PropertyType);
        Assert.AreEqual(typeof(Vue3.IVNode), slotComponent!.PropertyType);
        Assert.AreEqual("__arg1", routeMaybeRefFromRef!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", routeMaybeRefFromReadonly!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", boolMaybeRefFromReadonly!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
    }

    [TestMethod]
    public void VueRoute_RouterLinkAriaCurrentValue_UsesOfficialLiteralUnion()
    {
        var property = typeof(RouterLinkProps).GetProperty(nameof(RouterLinkProps.AriaCurrentValue), BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(property);
        Assert.AreEqual(typeof(RouterLinkAriaCurrentValue), Nullable.GetUnderlyingType(property!.PropertyType) ?? property.PropertyType);
        Assert.IsTrue(typeof(RouterLinkAriaCurrentValue).IsDefined(typeof(StringAttribute), inherit: false));
        CollectionAssert.AreEqual(
            new[]
            {
                nameof(RouterLinkAriaCurrentValue.Page),
                nameof(RouterLinkAriaCurrentValue.Step),
                nameof(RouterLinkAriaCurrentValue.Location),
                nameof(RouterLinkAriaCurrentValue.Date),
                nameof(RouterLinkAriaCurrentValue.Time),
                nameof(RouterLinkAriaCurrentValue.True),
                nameof(RouterLinkAriaCurrentValue.False)
            },
            Enum.GetNames<RouterLinkAriaCurrentValue>());
    }

    [TestMethod]
    public void VueRoute_LocationContracts_SeparateAuthoringOptions_FromNormalizedRuntimeShapes()
    {
        var routeLocationBase = typeof(RouteLocation);
        var normalized = typeof(RouteLocationNormalized);
        var resolved = typeof(RouteLocationResolved);
        var options = typeof(RouteLocationOptions);
        var failureFrom = typeof(NavigationFailure).GetProperty(nameof(NavigationFailure.From), BindingFlags.Public | BindingFlags.Instance);

        Assert.AreEqual(typeof(RouteLocationOptions), typeof(RouteLocationAsPath).BaseType);
        Assert.AreEqual(typeof(RouteLocationOptions), typeof(RouteLocationAsRelative).BaseType);
        Assert.AreEqual(typeof(RouteLocation), resolved.BaseType);
        Assert.AreEqual(typeof(RouteLocationNormalized), typeof(RouteLocationNormalizedLoaded).BaseType);
        Assert.IsTrue(routeLocationBase.GetProperty(nameof(RouteLocation.Replace), BindingFlags.Public | BindingFlags.Instance) is not null);
        Assert.IsTrue(routeLocationBase.GetProperty(nameof(RouteLocation.State), BindingFlags.Public | BindingFlags.Instance) is not null);
        Assert.IsNull(normalized.GetProperty(nameof(RouteLocation.Replace), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.IsNotNull(normalized.GetProperty(nameof(RouteLocationNormalized.RedirectedFrom), BindingFlags.Public | BindingFlags.Instance));
        Assert.AreEqual(typeof(RouteLocation), normalized.GetProperty(nameof(RouteLocationNormalized.RedirectedFrom), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.IsNotNull(failureFrom);
        Assert.AreEqual(typeof(RouteLocationNormalized), failureFrom!.PropertyType);
        Assert.IsTrue(options.IsAbstract);
    }

    private static MethodInfo RequiredStatic(MethodInfo[] methods, string name, Func<MethodInfo, bool> predicate)
    {
        var match = methods.SingleOrDefault(method => method.Name == name && predicate(method));
        Assert.IsNotNull(match, $"Missing expected static method: {name}");
        return match!;
    }

    private static MethodInfo RequiredInstance(MethodInfo[] methods, string name, Func<MethodInfo, bool> predicate)
    {
        var match = methods.SingleOrDefault(method => method.Name == name && predicate(method));
        Assert.IsNotNull(match, $"Missing expected instance method: {name}");
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
