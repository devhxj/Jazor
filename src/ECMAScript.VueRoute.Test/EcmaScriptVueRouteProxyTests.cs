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
            typeof(RouteRecordMatcher),
            typeof(RouteLocation),
            typeof(RouteLocationOptions),
            typeof(RouteQueryAndHash),
            typeof(RouteLocationAsPath),
            typeof(RouteLocationPathRaw),
            typeof(RouteLocationAsRelative),
            typeof(RouteLocationNamedRaw),
            typeof(MatcherLocation),
            typeof(MatcherLocationAsPath),
            typeof(MatcherLocationAsName),
            typeof(MatcherLocationAsRelative),
            typeof(LocationAsRelativeRaw),
            typeof(PathParserOptions),
            typeof(PathParserKey),
            typeof(PathParser),
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
            typeof(RouteMetaValue),
            typeof(HistoryState),
            typeof(LocationQuery),
            typeof(LocationQueryRaw),
            typeof(RouteParams),
            typeof(RouteParamsRaw),
            typeof(RouteLocationRawMaybeRef),
            typeof(MatcherLocationRaw),
            typeof(RouteBooleanMaybeRef),
            typeof(RouterViewDepthValue),
            typeof(HistoryStateValue),
            typeof(RouteNavigationResult),
            typeof(RawRouteComponents),
            typeof(RouteComponents),
            typeof(RouteNamedProps),
            typeof(RouteRecordNamedViewProps),
            typeof(NavigationFailure),
            typeof(NavigationRedirectError),
            typeof(RouterMatcher),
            typeof(RawRouteComponent),
            typeof(RouterHistoryNavigationInformation)
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
        RequiredStatic(methods, nameof(VueRoute.CreateRouterMatcher), static method =>
            method.ReturnType == typeof(RouterMatcher) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouteRecordRaw[]), typeof(PathParserOptions) }));
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
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouteNavigationGuard) }));
        RequiredStatic(methods, nameof(VueRoute.OnBeforeRouteLeave), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(AsyncRouteNavigationGuard) }));
        RequiredStatic(methods, nameof(VueRoute.OnBeforeRouteLeave), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(LegacyRouteNavigationGuard) }));
        RequiredStatic(methods, nameof(VueRoute.OnBeforeRouteLeave), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(LegacyAsyncRouteNavigationGuard) }));
        RequiredStatic(methods, nameof(VueRoute.OnBeforeRouteUpdate), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouteNavigationGuard) }));
        RequiredStatic(methods, nameof(VueRoute.OnBeforeRouteUpdate), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(AsyncRouteNavigationGuard) }));
        RequiredStatic(methods, nameof(VueRoute.OnBeforeRouteUpdate), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(LegacyRouteNavigationGuard) }));
        RequiredStatic(methods, nameof(VueRoute.OnBeforeRouteUpdate), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(LegacyAsyncRouteNavigationGuard) }));
        RequiredStatic(methods, nameof(VueRoute.IsNavigationFailure), static method =>
            method.ReturnType == typeof(bool) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Error) }));
        RequiredStatic(methods, nameof(VueRoute.IsNavigationFailure), static method =>
            method.ReturnType == typeof(bool) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Error), typeof(NavigationFailureType) }));
        RequiredStatic(methods, nameof(VueRoute.IsNavigationFailure), static method =>
            method.ReturnType == typeof(bool) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Error), typeof(ErrorTypes) }));
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
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouteLocation) }));

        var routerLink = typeof(VueRoute).GetProperty(nameof(VueRoute.RouterLink), BindingFlags.Public | BindingFlags.Static);
        var routerView = typeof(VueRoute).GetProperty(nameof(VueRoute.RouterView), BindingFlags.Public | BindingFlags.Static);
        var startLocation = typeof(VueRoute).GetProperty(nameof(VueRoute.START_LOCATION), BindingFlags.Public | BindingFlags.Static);
        var routerKey = typeof(VueRoute).GetProperty(nameof(VueRoute.RouterKey), BindingFlags.Public | BindingFlags.Static);
        var routeLocationKey = typeof(VueRoute).GetProperty(nameof(VueRoute.RouteLocationKey), BindingFlags.Public | BindingFlags.Static);
        var routerViewLocationKey = typeof(VueRoute).GetProperty(nameof(VueRoute.RouterViewLocationKey), BindingFlags.Public | BindingFlags.Static);
        var matchedRouteKey = typeof(VueRoute).GetProperty(nameof(VueRoute.MatchedRouteKey), BindingFlags.Public | BindingFlags.Static);
        var viewDepthKey = typeof(VueRoute).GetProperty(nameof(VueRoute.ViewDepthKey), BindingFlags.Public | BindingFlags.Static);

        Assert.IsNotNull(routerLink);
        Assert.IsNotNull(routerView);
        Assert.IsNotNull(startLocation);
        Assert.IsNotNull(routerKey);
        Assert.IsNotNull(routeLocationKey);
        Assert.IsNotNull(routerViewLocationKey);
        Assert.IsNotNull(matchedRouteKey);
        Assert.IsNotNull(viewDepthKey);
        Assert.AreEqual(typeof(Vue3.IVueComponent<RouterLinkProps, RouterLinkSlots>), routerLink!.PropertyType);
        Assert.AreEqual(typeof(Vue3.IVueComponent<RouterViewProps, RouterViewSlots>), routerView!.PropertyType);
        Assert.AreEqual(typeof(RouteLocationNormalizedLoaded), startLocation!.PropertyType);
        Assert.AreEqual(typeof(Vue3.VueInjectionKey<Router>), routerKey!.PropertyType);
        Assert.AreEqual(typeof(Vue3.VueInjectionKey<RouteLocationNormalizedLoaded>), routeLocationKey!.PropertyType);
        Assert.AreEqual(typeof(Vue3.VueInjectionKey<Vue3.IVueRef<RouteLocationNormalizedLoaded>>), routerViewLocationKey!.PropertyType);
        Assert.AreEqual(typeof(Vue3.VueInjectionKey<Vue3.VueComputedRef<RouteRecordNormalized?>>), matchedRouteKey!.PropertyType);
        Assert.AreEqual(typeof(Vue3.VueInjectionKey<RouterViewDepthValue>), viewDepthKey!.PropertyType);
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
    public void VueRoute_ErasedValueUnions_UseNet11UnionContract()
    {
        AssertNet11UnionContract(typeof(RouteRecordName), typeof(string), typeof(Symbol));
        AssertNet11UnionContract(typeof(RouteRecordAlias), typeof(string), typeof(string[]));
        AssertNet11UnionContract(typeof(RouteLocationRaw), typeof(string), typeof(RouteLocationAsPath), typeof(RouteLocationAsRelative));
        AssertNet11UnionContract(
            typeof(RouteLocationRawMaybeRef),
            typeof(RouteLocationRaw),
            typeof(Vue3.IVueRef<RouteLocationRaw>),
            typeof(Vue3.VueReadonlyRef<RouteLocationRaw>),
            typeof(Vue3.IVueRef<string>),
            typeof(Vue3.IVueRef<RouteLocationAsPath>),
            typeof(Vue3.IVueRef<RouteLocationAsRelative>),
            typeof(Vue3.VueReadonlyRef<string>),
            typeof(Vue3.VueReadonlyRef<RouteLocationAsPath>),
            typeof(Vue3.VueReadonlyRef<RouteLocationAsRelative>));
        AssertNet11UnionContract(typeof(RouteBooleanMaybeRef), typeof(bool), typeof(Vue3.IVueRef<bool>), typeof(Vue3.VueReadonlyRef<bool>));
        AssertNet11UnionContract(typeof(RouterViewDepthValue), typeof(Number), typeof(Vue3.IVueRef<Number>));
        AssertNet11UnionContract(typeof(RawRouteComponent), typeof(ECMAScript.Vue3.IVueComponent), typeof(RouteComponentLoader));
        AssertNet11UnionContract(typeof(RouteComponent), typeof(ECMAScript.Vue3.IVueComponent), typeof(RouteComponentLoader));
        AssertNet11UnionContract(typeof(RouteRecordProps), typeof(bool), typeof(Vue3.VueProps), typeof(RouteRecordPropsResolver));
        AssertNet11UnionContract(typeof(RouteRecordNamedViewProps), typeof(bool), typeof(RouteNamedProps));
        AssertNet11UnionContract(typeof(NavigationGuardNextArgument), typeof(bool), typeof(RouteLocationRaw), typeof(NavigationGuardNextCallback), typeof(Error));
        AssertNet11UnionContract(typeof(NavigationGuardReturn), typeof(bool), typeof(RouteLocationRaw), typeof(Error));
        AssertNet11UnionContract(typeof(RouteNavigationResult), typeof(NavigationFailure));
        AssertNet11UnionContract(typeof(NavigationGuardHandler), typeof(RouteNavigationGuard), typeof(AsyncRouteNavigationGuard), typeof(LegacyRouteNavigationGuard), typeof(LegacyAsyncRouteNavigationGuard));
        AssertNet11UnionContract(typeof(RouteRecordBeforeEnter), typeof(NavigationGuardHandler), typeof(NavigationGuardHandler[]));
        AssertNet11UnionContract(typeof(RouteRedirectOption), typeof(RouteLocationRaw), typeof(RouteRedirectCallback));
        AssertNet11UnionContract(typeof(RouteRecordRedirectOption), typeof(RouteLocationRaw), typeof(RouteRedirectCallback));
        AssertNet11UnionContract(typeof(HistoryStateValue), typeof(string), typeof(Number), typeof(bool), typeof(HistoryState), typeof(Array<HistoryStateValue?>));
        AssertNet11UnionContract(typeof(ScrollPositionTarget), typeof(string), typeof(Element));
        AssertNet11UnionContract(typeof(RouteRecordRaw), typeof(RouteRecordSingleView), typeof(RouteRecordSingleViewWithChildren), typeof(RouteRecordMultipleViews), typeof(RouteRecordMultipleViewsWithChildren), typeof(RouteRecordRedirect));
        AssertNet11UnionContract(typeof(MatcherLocationRaw), typeof(MatcherLocationAsPath), typeof(MatcherLocationAsName), typeof(MatcherLocationAsRelative));
        AssertNet11UnionContract(typeof(RouteParam), typeof(string), typeof(string[]));
        AssertNet11UnionContract(typeof(RouteParamRaw), typeof(string), typeof(Array<RouteParamRaw>), typeof(Number));
        AssertNet11UnionContract(typeof(LocationQueryValue), typeof(string), typeof(Array<string>));
        AssertNet11UnionContract(typeof(LocationQueryValueRaw), typeof(string), typeof(Array<LocationQueryValueRaw?>), typeof(Number));
    }

    [TestMethod]
    public void VueRoute_ErrorTypes_UsesOfficialInternalBitFlags()
    {
        Assert.AreEqual(1, typeof(ErrorTypes).GetCustomAttributes(typeof(FlagsAttribute), inherit: false).Length);
        CollectionAssert.AreEquivalent(
            new[]
            {
                nameof(ErrorTypes.MATCHER_NOT_FOUND),
                nameof(ErrorTypes.NAVIGATION_GUARD_REDIRECT),
                nameof(ErrorTypes.NAVIGATION_ABORTED),
                nameof(ErrorTypes.NAVIGATION_CANCELLED),
                nameof(ErrorTypes.NAVIGATION_DUPLICATED)
            },
            Enum.GetNames<ErrorTypes>());
        CollectionAssert.AreEquivalent(
            new[] { 1, 2, 4, 8, 16 },
            Enum.GetValues<ErrorTypes>().Select(static value => (int)value).ToArray());
    }

    [TestMethod]
    public void VueRoute_RouteComponent_UsesInlineFactory_AndRawRouteComponent_HandlesLazyLoaderUnion()
    {
        var from = typeof(RouteComponent).GetMethod(nameof(RouteComponent.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(ECMAScript.Vue3.IVueComponent) });
        var fromLoader = typeof(RouteComponent).GetMethod(nameof(RouteComponent.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteComponentLoader) });
        var rawFrom = typeof(RawRouteComponent).GetMethod(nameof(RawRouteComponent.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(ECMAScript.Vue3.IVueComponent) });
        var rawFromLoader = typeof(RawRouteComponent).GetMethod(nameof(RawRouteComponent.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteComponentLoader) });
        var implicitOperators = typeof(RawRouteComponent)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(static method => method.Name == "op_Implicit")
            .ToArray();
        var implicitComponent = implicitOperators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(RouteComponent));

        Assert.IsNotNull(from);
        Assert.IsNotNull(fromLoader);
        Assert.IsNotNull(rawFrom);
        Assert.IsNotNull(rawFromLoader);
        Assert.IsNotNull(implicitComponent);
        Assert.AreEqual(typeof(RouteComponent), from!.ReturnType);
        Assert.AreEqual("__arg1", from.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        CollectionAssert.AreEqual(new[] { typeof(ECMAScript.Vue3.IVueComponent) }, from.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(typeof(RouteComponent), fromLoader!.ReturnType);
        Assert.AreEqual("__arg1", fromLoader.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        CollectionAssert.AreEqual(new[] { typeof(RouteComponentLoader) }, fromLoader.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());

        Assert.AreEqual(typeof(RawRouteComponent), rawFrom!.ReturnType);
        Assert.AreEqual("__arg1", rawFrom.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        CollectionAssert.AreEqual(new[] { typeof(ECMAScript.Vue3.IVueComponent) }, rawFrom.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(typeof(RawRouteComponent), rawFromLoader!.ReturnType);
        Assert.AreEqual("__arg1", rawFromLoader.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        CollectionAssert.AreEqual(new[] { typeof(RouteComponentLoader) }, rawFromLoader.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());

        Assert.AreEqual(typeof(RawRouteComponent), implicitComponent!.ReturnType);
        CollectionAssert.AreEqual(new[] { typeof(RouteComponent) }, implicitComponent.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
    }

    [TestMethod]
    public void VueRoute_RoutePropsAndRedirectUnions_ExposeExplicitFactories_AndNamedPropsAddOverloads()
    {
        var routeRecordPropsType = typeof(RouteRecordProps);
        var namedViewPropsType = typeof(RouteRecordNamedViewProps);
        var redirectOptionType = typeof(RouteRedirectOption);
        var recordRedirectOptionType = typeof(RouteRecordRedirectOption);
        var namedPropsType = typeof(RouteNamedProps);

        var propsBoolFactory = routeRecordPropsType.GetMethod(nameof(RouteRecordProps.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(bool) });
        var propsObjectFactory = routeRecordPropsType.GetMethod(nameof(RouteRecordProps.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(Vue3.VueProps) });
        var propsResolverFactory = routeRecordPropsType.GetMethod(nameof(RouteRecordProps.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteRecordPropsResolver) });
        var namedViewBoolFactory = namedViewPropsType.GetMethod(nameof(RouteRecordNamedViewProps.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(bool) });
        var namedViewDictionaryFactory = namedViewPropsType.GetMethod(nameof(RouteRecordNamedViewProps.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteNamedProps) });
        var redirectLocationFactory = redirectOptionType.GetMethod(nameof(RouteRedirectOption.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteLocationRaw) });
        var redirectCallbackFactory = redirectOptionType.GetMethod(nameof(RouteRedirectOption.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteRedirectCallback) });
        var redirectRecordFactory = redirectOptionType.GetMethod(nameof(RouteRedirectOption.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteRecordRedirectOption) });
        var recordRedirectLocationFactory = recordRedirectOptionType.GetMethod(nameof(RouteRecordRedirectOption.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteLocationRaw) });
        var recordRedirectCallbackFactory = recordRedirectOptionType.GetMethod(nameof(RouteRecordRedirectOption.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteRedirectCallback) });
        var addBool = namedPropsType.GetMethod(nameof(RouteNamedProps.Add), BindingFlags.Public | BindingFlags.Instance, new[] { typeof(string), typeof(bool) });
        var addProps = namedPropsType.GetMethod(nameof(RouteNamedProps.Add), BindingFlags.Public | BindingFlags.Instance, new[] { typeof(string), typeof(Vue3.VueProps) });
        var addResolver = namedPropsType.GetMethod(nameof(RouteNamedProps.Add), BindingFlags.Public | BindingFlags.Instance, new[] { typeof(string), typeof(RouteRecordPropsResolver) });

        Assert.IsNotNull(propsBoolFactory);
        Assert.IsNotNull(propsObjectFactory);
        Assert.IsNotNull(propsResolverFactory);
        Assert.IsNotNull(namedViewBoolFactory);
        Assert.IsNotNull(namedViewDictionaryFactory);
        Assert.IsNotNull(redirectLocationFactory);
        Assert.IsNotNull(redirectCallbackFactory);
        Assert.IsNotNull(redirectRecordFactory);
        Assert.IsNotNull(recordRedirectLocationFactory);
        Assert.IsNotNull(recordRedirectCallbackFactory);
        Assert.IsNotNull(addBool);
        Assert.IsNotNull(addProps);
        Assert.IsNotNull(addResolver);
        Assert.AreEqual(typeof(RouteRecordProps), propsBoolFactory!.ReturnType);
        Assert.AreEqual(typeof(RouteRecordProps), propsObjectFactory!.ReturnType);
        Assert.AreEqual(typeof(RouteRecordProps), propsResolverFactory!.ReturnType);
        Assert.AreEqual(typeof(RouteRecordNamedViewProps), namedViewBoolFactory!.ReturnType);
        Assert.AreEqual(typeof(RouteRecordNamedViewProps), namedViewDictionaryFactory!.ReturnType);
        Assert.AreEqual(typeof(RouteRedirectOption), redirectLocationFactory!.ReturnType);
        Assert.AreEqual(typeof(RouteRedirectOption), redirectCallbackFactory!.ReturnType);
        Assert.AreEqual(typeof(RouteRedirectOption), redirectRecordFactory!.ReturnType);
        Assert.AreEqual(typeof(RouteRecordRedirectOption), recordRedirectLocationFactory!.ReturnType);
        Assert.AreEqual(typeof(RouteRecordRedirectOption), recordRedirectCallbackFactory!.ReturnType);
        Assert.AreEqual("__arg1", propsBoolFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", propsObjectFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", propsResolverFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", namedViewBoolFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", namedViewDictionaryFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", redirectLocationFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", redirectCallbackFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", redirectRecordFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", recordRedirectLocationFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", recordRedirectCallbackFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.IsNull(redirectOptionType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(static method =>
                method.Name == "op_Implicit" &&
                method.GetParameters().Single().ParameterType == typeof(Func<RouteLocation, RouteLocationNormalizedLoaded, RouteLocationRaw>)));
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
        AssertEcmaScriptSupport(typeof(RouteQueryAndHash));
        AssertEcmaScriptSupport(typeof(RouteLocation));
        AssertEcmaScriptSupport(typeof(MatcherLocation));
        AssertEcmaScriptSupport(typeof(MatcherLocationAsPath));
        AssertEcmaScriptSupport(typeof(MatcherLocationAsName));
        AssertEcmaScriptSupport(typeof(MatcherLocationAsRelative));
        AssertEcmaScriptSupport(typeof(LocationAsRelativeRaw));
        AssertEcmaScriptSupport(typeof(PathParserOptions));
        AssertEcmaScriptSupport(typeof(PathParserKey));
        AssertEcmaScriptSupport(typeof(PathParser));
        AssertEcmaScriptSupport(typeof(RouteLocationPathRaw));
        AssertEcmaScriptSupport(typeof(RouteLocationNamedRaw));
        AssertEcmaScriptSupport(typeof(RouteLocationRawMaybeRef));
        AssertEcmaScriptSupport(typeof(MatcherLocationRaw));
        AssertEcmaScriptSupport(typeof(RouteBooleanMaybeRef));
        AssertEcmaScriptSupport(typeof(RouterViewDepthValue));
        AssertEcmaScriptSupport(typeof(HistoryStateValue));
        AssertEcmaScriptSupport(typeof(RouteNavigationResult));
        AssertEcmaScriptSupport(typeof(RawRouteComponents));
        AssertEcmaScriptSupport(typeof(RouteComponents));
        AssertEcmaScriptSupport(typeof(RouteNamedProps));
        AssertEcmaScriptSupport(typeof(RouteRecordNamedViewProps));
        AssertEcmaScriptSupport(typeof(RouteRecordName));
        AssertEcmaScriptSupport(typeof(RouteRecordAlias));
        AssertEcmaScriptSupport(typeof(RouteLocationRaw));
        AssertEcmaScriptSupport(typeof(UseLinkReturn));
        AssertEcmaScriptSupport(typeof(RouterLinkOptions));
        AssertEcmaScriptSupport(typeof(RouterLinkSlotScope));
        AssertEcmaScriptSupport(typeof(RouteComponent));
        AssertEcmaScriptSupport(typeof(RawRouteComponent));
        AssertEcmaScriptSupport(typeof(RouteMetaValue));
        AssertEcmaScriptSupport(typeof(RouteRecordProps));
        AssertEcmaScriptSupport(typeof(RouteRecordMatcher));
        AssertEcmaScriptSupport(typeof(RouterMatcher));
        AssertEcmaScriptSupport(typeof(NavigationGuardNextArgument));
        AssertEcmaScriptSupport(typeof(NavigationGuardReturn));
        AssertEcmaScriptSupport(typeof(NavigationGuardHandler));
        AssertEcmaScriptSupport(typeof(RouteRecordBeforeEnter));
        AssertEcmaScriptSupport(typeof(RouteRedirectOption));
        AssertEcmaScriptSupport(typeof(RouteRecordRedirectOption));
        AssertEcmaScriptSupport(typeof(RouteRecordRaw));
        AssertEcmaScriptSupport(typeof(RouteParam));
        AssertEcmaScriptSupport(typeof(RouteParamRaw));
        AssertEcmaScriptSupport(typeof(LocationQueryValue));
        AssertEcmaScriptSupport(typeof(LocationQueryValueRaw));
    }

    [TestMethod]
    public void VueRoute_QueryValueUnions_SupportNullableArrayPayloads_ForOfficialQuerySemantics()
    {
        var nullability = new NullabilityInfoContext();
        var locationQueryValueArray = typeof(LocationQueryValue).GetProperty(nameof(LocationQueryValue.AsArray), BindingFlags.Public | BindingFlags.Instance);
        var locationQueryRawArray = typeof(LocationQueryValueRaw).GetProperty(nameof(LocationQueryValueRaw.AsArray), BindingFlags.Public | BindingFlags.Instance);
        var locationQueryValueArrayOperator = typeof(LocationQueryValue)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(static method => method.Name == "op_Implicit" && method.GetParameters().Single().ParameterType == typeof(string[]));
        var locationQueryValueRawNullableStringArrayOperator = typeof(LocationQueryValueRaw)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(static method => method.Name == "op_Implicit" && method.GetParameters().Single().ParameterType == typeof(string[]));
        var locationQueryValueRawMixedArrayOperator = typeof(LocationQueryValueRaw)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(static method => method.Name == "op_Implicit" && method.GetParameters().Single().ParameterType == typeof(LocationQueryValueRaw?[]));

        Assert.IsNotNull(locationQueryValueArray);
        Assert.IsNotNull(locationQueryRawArray);
        Assert.IsNotNull(locationQueryValueArrayOperator);
        Assert.IsNotNull(locationQueryValueRawNullableStringArrayOperator);
        Assert.IsNotNull(locationQueryValueRawMixedArrayOperator);
        Assert.AreEqual(typeof(Array<string?>), locationQueryValueArray!.PropertyType);
        Assert.AreEqual(typeof(Array<LocationQueryValueRaw?>), locationQueryRawArray!.PropertyType);
        Assert.AreEqual(typeof(LocationQueryValue), locationQueryValueArrayOperator!.ReturnType);
        Assert.AreEqual(typeof(LocationQueryValueRaw), locationQueryValueRawNullableStringArrayOperator!.ReturnType);
        Assert.AreEqual(typeof(LocationQueryValueRaw), locationQueryValueRawMixedArrayOperator!.ReturnType);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(locationQueryValueArrayOperator.GetParameters()[0]).ElementType!.ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(locationQueryValueRawNullableStringArrayOperator.GetParameters()[0]).ElementType!.ReadState);
    }

    [TestMethod]
    public void VueRoute_HistoryStateValue_SupportsRecursiveTypedArrayAuthoring_ForOfficialHistoryStateSemantics()
    {
        var nullability = new NullabilityInfoContext();
        var historyStateValueType = typeof(HistoryStateValue);
        var asArray = historyStateValueType.GetProperty(nameof(HistoryStateValue.AsArray), BindingFlags.Public | BindingFlags.Instance);
        var operators = historyStateValueType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(static method => method.Name == "op_Implicit")
            .ToArray();
        var nullableStringArrayOperator = operators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(string[]));
        var boolArrayOperator = operators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(bool[]));
        var nullableBoolArrayOperator = operators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(bool?[]));
        var numberArrayOperator = operators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(Number[]));
        var nullableNumberArrayOperator = operators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(Number?[]));
        var historyStateArrayOperator = operators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(HistoryState[]));
        var mixedArrayOperator = operators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(HistoryStateValue?[]));

        Assert.IsNotNull(asArray);
        Assert.IsNotNull(nullableStringArrayOperator);
        Assert.IsNotNull(boolArrayOperator);
        Assert.IsNotNull(nullableBoolArrayOperator);
        Assert.IsNotNull(numberArrayOperator);
        Assert.IsNotNull(nullableNumberArrayOperator);
        Assert.IsNotNull(historyStateArrayOperator);
        Assert.IsNotNull(mixedArrayOperator);
        Assert.AreEqual(typeof(Array<HistoryStateValue?>), asArray!.PropertyType);
        Assert.AreEqual(typeof(HistoryStateValue), nullableStringArrayOperator!.ReturnType);
        Assert.AreEqual(typeof(HistoryStateValue), boolArrayOperator!.ReturnType);
        Assert.AreEqual(typeof(HistoryStateValue), nullableBoolArrayOperator!.ReturnType);
        Assert.AreEqual(typeof(HistoryStateValue), numberArrayOperator!.ReturnType);
        Assert.AreEqual(typeof(HistoryStateValue), nullableNumberArrayOperator!.ReturnType);
        Assert.AreEqual(typeof(HistoryStateValue), historyStateArrayOperator!.ReturnType);
        Assert.AreEqual(typeof(HistoryStateValue), mixedArrayOperator!.ReturnType);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(asArray).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(nullableStringArrayOperator.GetParameters()[0]).ElementType!.ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(nullableBoolArrayOperator.GetParameters()[0]).ElementType!.ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(nullableNumberArrayOperator.GetParameters()[0]).ElementType!.ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(historyStateArrayOperator.GetParameters()[0]).ElementType!.ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(mixedArrayOperator.GetParameters()[0]).ElementType!.ReadState);
    }

    [TestMethod]
    public void VueRoute_RouteParamRaw_UsesMixedScalarArrayContract_ForOfficialParamSemantics()
    {
        var routeParamRawArray = typeof(RouteParamRaw).GetProperty(nameof(RouteParamRaw.AsArray), BindingFlags.Public | BindingFlags.Instance);
        var routeParamRawMixedArrayOperator = typeof(RouteParamRaw)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(static method => method.Name == "op_Implicit" && method.GetParameters().Single().ParameterType == typeof(RouteParamRaw[]));
        var routeParamRawStringArrayOperator = typeof(RouteParamRaw)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(static method => method.Name == "op_Implicit" && method.GetParameters().Single().ParameterType == typeof(string[]));
        var routeParamRawNumberArrayOperator = typeof(RouteParamRaw)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(static method => method.Name == "op_Implicit" && method.GetParameters().Single().ParameterType == typeof(Number[]));

        Assert.IsNotNull(routeParamRawArray);
        Assert.IsNotNull(routeParamRawMixedArrayOperator);
        Assert.IsNotNull(routeParamRawStringArrayOperator);
        Assert.IsNotNull(routeParamRawNumberArrayOperator);
        Assert.AreEqual(typeof(Array<RouteParamRaw>), routeParamRawArray!.PropertyType);
        Assert.AreEqual(typeof(RouteParamRaw), routeParamRawMixedArrayOperator!.ReturnType);
        Assert.AreEqual(typeof(RouteParamRaw), routeParamRawStringArrayOperator!.ReturnType);
        Assert.AreEqual(typeof(RouteParamRaw), routeParamRawNumberArrayOperator!.ReturnType);
    }

    [TestMethod]
    public void VueRoute_LocationQueryRaw_AndHistoryState_SupportNumericAuthoringKeys()
    {
        var queryNumberIndexer = typeof(LocationQueryRaw).GetProperty("Item", new[] { typeof(Number) });
        var queryNullableStringAdd = typeof(LocationQueryRaw).GetMethod(nameof(LocationQueryRaw.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(string), typeof(LocationQueryValueRaw?) }, modifiers: null);
        var queryNullableNumberAdd = typeof(LocationQueryRaw).GetMethod(nameof(LocationQueryRaw.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(Number), typeof(LocationQueryValueRaw?) }, modifiers: null);
        var stateNumberIndexer = typeof(HistoryState).GetProperty("Item", new[] { typeof(Number) });
        var stateNullableStringAdd = typeof(HistoryState).GetMethod(nameof(HistoryState.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(string), typeof(HistoryStateValue?) }, modifiers: null);
        var stateNullableNumberAdd = typeof(HistoryState).GetMethod(nameof(HistoryState.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(Number), typeof(HistoryStateValue?) }, modifiers: null);

        Assert.IsNotNull(queryNumberIndexer);
        Assert.IsNotNull(queryNullableStringAdd);
        Assert.IsNotNull(queryNullableNumberAdd);
        Assert.IsNotNull(stateNumberIndexer);
        Assert.IsNotNull(stateNullableStringAdd);
        Assert.IsNotNull(stateNullableNumberAdd);
        Assert.AreEqual(typeof(LocationQueryValueRaw?), queryNumberIndexer!.PropertyType);
        Assert.AreEqual(typeof(HistoryStateValue?), stateNumberIndexer!.PropertyType);
    }

    [TestMethod]
    public void VueRoute_NullableObjectLiteralHosts_ExposeNullableAddOverloads_ForCollectionInitializerAuthoring()
    {
        var nullability = new NullabilityInfoContext();
        var locationQueryAdd = typeof(LocationQuery).GetMethod(nameof(LocationQuery.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(string), typeof(LocationQueryValue?) }, modifiers: null);
        var routeParamsRawAdd = typeof(RouteParamsRaw).GetMethod(nameof(RouteParamsRaw.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(string), typeof(RouteParamRaw?) }, modifiers: null);
        var routeComponentInstanceMapAdd = typeof(RouteComponentInstanceMap).GetMethod(nameof(RouteComponentInstanceMap.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(string), typeof(Vue3.VueComponentPublicInstance) }, modifiers: null);

        Assert.IsNotNull(locationQueryAdd);
        Assert.IsNotNull(routeParamsRawAdd);
        Assert.IsNotNull(routeComponentInstanceMapAdd);
        Assert.AreEqual(typeof(Vue3.VueComponentPublicInstance), routeComponentInstanceMapAdd!.GetParameters()[1].ParameterType);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(routeComponentInstanceMapAdd.GetParameters()[1]).ReadState);
    }

    [TestMethod]
    public void VueRoute_RouteMeta_UsesPropertyKeyIndices_AndRecursiveNullableValueContracts()
    {
        var nullability = new NullabilityInfoContext();
        var routeMetaType = typeof(RouteMeta);
        var routeMetaValueType = typeof(RouteMetaValue);
        var stringIndexer = routeMetaType.GetProperty("Item", new[] { typeof(string) });
        var numberIndexer = routeMetaType.GetProperty("Item", new[] { typeof(Number) });
        var symbolIndexer = routeMetaType.GetProperty("Item", new[] { typeof(Symbol) });
        var stringAdd = routeMetaType.GetMethod(nameof(RouteMeta.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(string), typeof(RouteMetaValue) }, modifiers: null);
        var numberAdd = routeMetaType.GetMethod(nameof(RouteMeta.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(Number), typeof(RouteMetaValue) }, modifiers: null);
        var symbolAdd = routeMetaType.GetMethod(nameof(RouteMeta.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(Symbol), typeof(RouteMetaValue) }, modifiers: null);
        var actionStringAdd = routeMetaType.GetMethod(nameof(RouteMeta.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(string), typeof(Action) }, modifiers: null);
        var actionNumberAdd = routeMetaType.GetMethod(nameof(RouteMeta.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(Number), typeof(Action) }, modifiers: null);
        var actionSymbolAdd = routeMetaType.GetMethod(nameof(RouteMeta.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(Symbol), typeof(Action) }, modifiers: null);
        var actionFactory = routeMetaValueType.GetMethod(nameof(RouteMetaValue.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(Action) });
        var operators = routeMetaValueType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(static method => method.Name == "op_Implicit")
            .ToArray();
        var symbolOperator = operators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(Symbol));
        var vuePropsOperator = operators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(Vue3.VueProps));
        var mixedArrayOperator = operators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(RouteMetaValue[]));

        Assert.IsNotNull(stringIndexer);
        Assert.IsNotNull(numberIndexer);
        Assert.IsNotNull(symbolIndexer);
        Assert.IsNotNull(stringAdd);
        Assert.IsNotNull(numberAdd);
        Assert.IsNotNull(symbolAdd);
        Assert.IsNotNull(actionStringAdd);
        Assert.IsNotNull(actionNumberAdd);
        Assert.IsNotNull(actionSymbolAdd);
        Assert.IsNotNull(actionFactory);
        Assert.IsNotNull(symbolOperator);
        Assert.IsNotNull(vuePropsOperator);
        Assert.IsNotNull(mixedArrayOperator);
        Assert.AreEqual(typeof(RouteMetaValue), stringIndexer!.PropertyType);
        Assert.AreEqual(typeof(RouteMetaValue), numberIndexer!.PropertyType);
        Assert.AreEqual(typeof(RouteMetaValue), symbolIndexer!.PropertyType);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(stringIndexer).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(numberIndexer).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(symbolIndexer).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(stringAdd!.GetParameters()[1]).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(numberAdd!.GetParameters()[1]).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(symbolAdd!.GetParameters()[1]).ReadState);
        Assert.AreEqual("__arg1", actionFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual(typeof(RouteMetaValue), symbolOperator!.ReturnType);
        Assert.AreEqual(typeof(RouteMetaValue), vuePropsOperator!.ReturnType);
        Assert.AreEqual(typeof(RouteMetaValue), mixedArrayOperator!.ReturnType);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(mixedArrayOperator.GetParameters()[0]).ElementType!.ReadState);
    }

    [TestMethod]
    public void VueRoute_RouterRuntimeSurface_ExposesProductionNavigationControls()
    {
        var routerType = typeof(Router);
        var listening = routerType.GetProperty(nameof(Router.Listening), BindingFlags.Public | BindingFlags.Instance);
        var currentRoute = routerType.GetProperty(nameof(Router.CurrentRoute), BindingFlags.Public | BindingFlags.Instance);
        var methods = routerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => !method.IsSpecialName)
            .ToArray();

        Assert.IsNotNull(listening);
        Assert.IsNotNull(currentRoute);
        Assert.AreEqual(typeof(bool), listening!.PropertyType);
        Assert.AreEqual(typeof(Vue3.VueShallowRef<RouteLocationNormalizedLoaded>), currentRoute!.PropertyType);
        Assert.IsTrue(listening.CanRead);
        Assert.IsTrue(listening.CanWrite);

        RequiredInstance(methods, nameof(Router.GetRoutes), static method =>
            method.ReturnType == typeof(RouteRecordNormalized[]) &&
            method.GetParameters().Length == 0);
        RequiredInstance(methods, nameof(Router.ClearRoutes), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Length == 0);
        RequiredInstance(methods, nameof(Router.BeforeEach), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouteNavigationGuard) }));
        RequiredInstance(methods, nameof(Router.BeforeEach), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(AsyncRouteNavigationGuard) }));
        RequiredInstance(methods, nameof(Router.BeforeEach), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(LegacyRouteNavigationGuard) }));
        RequiredInstance(methods, nameof(Router.BeforeEach), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(LegacyAsyncRouteNavigationGuard) }));
        RequiredInstance(methods, nameof(Router.BeforeResolve), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouteNavigationGuard) }));
        RequiredInstance(methods, nameof(Router.BeforeResolve), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(AsyncRouteNavigationGuard) }));
        RequiredInstance(methods, nameof(Router.BeforeResolve), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(LegacyRouteNavigationGuard) }));
        RequiredInstance(methods, nameof(Router.BeforeResolve), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(LegacyAsyncRouteNavigationGuard) }));
        RequiredInstance(methods, nameof(Router.OnError), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(ErrorRouterErrorHandler) }));
        RequiredInstance(methods, nameof(Router.OnError), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(NavigationFailureRouterErrorHandler) }));
        RequiredInstance(methods, nameof(Router.OnError), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(NavigationRedirectRouterErrorHandler) }));
        RequiredInstance(methods, nameof(Router.OnError), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(StringRouterErrorHandler) }));
        RequiredInstance(methods, nameof(Router.OnError), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(NumberRouterErrorHandler) }));
        RequiredInstance(methods, nameof(Router.OnError), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(BooleanRouterErrorHandler) }));
        RequiredInstance(methods, nameof(Router.OnError), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(BigIntRouterErrorHandler) }));
        RequiredInstance(methods, nameof(Router.OnError), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(SymbolRouterErrorHandler) }));
        RequiredInstance(methods, nameof(Router.OnError), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(ObjectRouterErrorHandler) }));
        RequiredInstance(methods, nameof(Router.OnError), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(ArrayRouterErrorHandler) }));
        RequiredInstance(methods, nameof(Router.Resolve), static method =>
            method.ReturnType == typeof(RouteLocationResolved) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouteLocationRaw), typeof(RouteLocationNormalizedLoaded) }));
    }

    [TestMethod]
    public void VueRoute_RouterOnError_UsesExplicitStronglyTypedOverloadFamilies_WithoutObjectFallback()
    {
        var routerMethods = typeof(Router)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Router.OnError))
            .ToArray();
        var delegateTypes = routerMethods
            .Select(static method => method.GetParameters().Single().ParameterType)
            .ToArray();

        Assert.AreEqual(10, routerMethods.Length);
        CollectionAssert.AreEquivalent(
            new[]
            {
                typeof(ErrorRouterErrorHandler),
                typeof(NavigationFailureRouterErrorHandler),
                typeof(NavigationRedirectRouterErrorHandler),
                typeof(StringRouterErrorHandler),
                typeof(NumberRouterErrorHandler),
                typeof(BooleanRouterErrorHandler),
                typeof(BigIntRouterErrorHandler),
                typeof(SymbolRouterErrorHandler),
                typeof(ObjectRouterErrorHandler),
                typeof(ArrayRouterErrorHandler)
            },
            delegateTypes);
        Assert.IsFalse(delegateTypes.Any(static type => type.Name.StartsWith("RouterErrorHandler", StringComparison.Ordinal)));
        Assert.IsNull(typeof(Router).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .SingleOrDefault(static method => method.Name == nameof(Router.OnError) && method.IsGenericMethodDefinition));
    }

    [TestMethod]
    public void VueRoute_NavigationGuardHostSurfaces_ExposeExplicitFactories_AndDoNotRelyOnUnionOnlyMethodParameters()
    {
        var handlerType = typeof(NavigationGuardHandler);
        var beforeEnterType = typeof(RouteRecordBeforeEnter);
        var syncFactory = handlerType.GetMethod(nameof(NavigationGuardHandler.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteNavigationGuard) });
        var asyncFactory = handlerType.GetMethod(nameof(NavigationGuardHandler.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(AsyncRouteNavigationGuard) });
        var legacySyncFactory = handlerType.GetMethod(nameof(NavigationGuardHandler.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(LegacyRouteNavigationGuard) });
        var legacyAsyncFactory = handlerType.GetMethod(nameof(NavigationGuardHandler.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(LegacyAsyncRouteNavigationGuard) });
        var beforeEnterGuardFactory = beforeEnterType.GetMethod(nameof(RouteRecordBeforeEnter.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(NavigationGuardHandler) });
        var beforeEnterSyncFactory = beforeEnterType.GetMethod(nameof(RouteRecordBeforeEnter.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteNavigationGuard) });
        var beforeEnterAsyncFactory = beforeEnterType.GetMethod(nameof(RouteRecordBeforeEnter.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(AsyncRouteNavigationGuard) });
        var beforeEnterLegacySyncFactory = beforeEnterType.GetMethod(nameof(RouteRecordBeforeEnter.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(LegacyRouteNavigationGuard) });
        var beforeEnterLegacyAsyncFactory = beforeEnterType.GetMethod(nameof(RouteRecordBeforeEnter.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(LegacyAsyncRouteNavigationGuard) });
        var beforeEnterGuardArrayFactory = beforeEnterType.GetMethod(nameof(RouteRecordBeforeEnter.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(NavigationGuardHandler[]) });
        var beforeEnterSyncArrayFactory = beforeEnterType.GetMethod(nameof(RouteRecordBeforeEnter.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteNavigationGuard[]) });
        var beforeEnterAsyncArrayFactory = beforeEnterType.GetMethod(nameof(RouteRecordBeforeEnter.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(AsyncRouteNavigationGuard[]) });
        var beforeEnterLegacySyncArrayFactory = beforeEnterType.GetMethod(nameof(RouteRecordBeforeEnter.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(LegacyRouteNavigationGuard[]) });
        var beforeEnterLegacyAsyncArrayFactory = beforeEnterType.GetMethod(nameof(RouteRecordBeforeEnter.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(LegacyAsyncRouteNavigationGuard[]) });

        Assert.IsNotNull(syncFactory);
        Assert.IsNotNull(asyncFactory);
        Assert.IsNotNull(legacySyncFactory);
        Assert.IsNotNull(legacyAsyncFactory);
        Assert.IsNotNull(beforeEnterGuardFactory);
        Assert.IsNotNull(beforeEnterSyncFactory);
        Assert.IsNotNull(beforeEnterAsyncFactory);
        Assert.IsNotNull(beforeEnterLegacySyncFactory);
        Assert.IsNotNull(beforeEnterLegacyAsyncFactory);
        Assert.IsNotNull(beforeEnterGuardArrayFactory);
        Assert.IsNotNull(beforeEnterSyncArrayFactory);
        Assert.IsNotNull(beforeEnterAsyncArrayFactory);
        Assert.IsNotNull(beforeEnterLegacySyncArrayFactory);
        Assert.IsNotNull(beforeEnterLegacyAsyncArrayFactory);
        Assert.AreEqual("__arg1", syncFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", asyncFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", legacySyncFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", legacyAsyncFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", beforeEnterGuardFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", beforeEnterSyncFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", beforeEnterAsyncFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", beforeEnterLegacySyncFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", beforeEnterLegacyAsyncFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", beforeEnterGuardArrayFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", beforeEnterSyncArrayFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", beforeEnterAsyncArrayFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", beforeEnterLegacySyncArrayFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", beforeEnterLegacyAsyncArrayFactory!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
    }

    [TestMethod]
    public void VueRoute_RouterErrorValue_ProvidesStronglyTypedScalarObjectAndArrayProjections()
    {
        var nullability = new NullabilityInfoContext();
        var errorType = typeof(RouterErrorValue);

        Assert.IsNotNull(errorType.GetCustomAttribute<System.Runtime.CompilerServices.UnionAttribute>());
        Assert.IsTrue(typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(errorType));

        var asError = errorType.GetProperty(nameof(RouterErrorValue.AsError), BindingFlags.Public | BindingFlags.Instance);
        var asString = errorType.GetProperty(nameof(RouterErrorValue.AsString), BindingFlags.Public | BindingFlags.Instance);
        var asNumber = errorType.GetProperty(nameof(RouterErrorValue.AsNumber), BindingFlags.Public | BindingFlags.Instance);
        var asBool = errorType.GetProperty(nameof(RouterErrorValue.AsBool), BindingFlags.Public | BindingFlags.Instance);
        var asBigInt = errorType.GetProperty(nameof(RouterErrorValue.AsBigInt), BindingFlags.Public | BindingFlags.Instance);
        var asSymbol = errorType.GetProperty(nameof(RouterErrorValue.AsSymbol), BindingFlags.Public | BindingFlags.Instance);
        var asObject = errorType.GetProperty(nameof(RouterErrorValue.AsObject), BindingFlags.Public | BindingFlags.Instance);
        var asArray = errorType.GetProperty(nameof(RouterErrorValue.AsArray), BindingFlags.Public | BindingFlags.Instance);
        var objectFactory = errorType.GetMethod(nameof(RouterErrorValue.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(IObject) });
        var arrayOperator = errorType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(static method => method.Name == "op_Implicit" && method.GetParameters().Single().ParameterType == typeof(RouterErrorValue?[]));

        Assert.IsNotNull(asError);
        Assert.IsNotNull(asString);
        Assert.IsNotNull(asNumber);
        Assert.IsNotNull(asBool);
        Assert.IsNotNull(asBigInt);
        Assert.IsNotNull(asSymbol);
        Assert.IsNotNull(asObject);
        Assert.IsNotNull(asArray);
        Assert.IsNotNull(objectFactory);
        Assert.IsNotNull(arrayOperator);
        Assert.AreEqual(typeof(Error), UnwrapNullable(asError!.PropertyType));
        Assert.AreEqual(typeof(string), UnwrapNullable(asString!.PropertyType));
        Assert.AreEqual(typeof(Number), UnwrapNullable(asNumber!.PropertyType));
        Assert.AreEqual(typeof(bool), UnwrapNullable(asBool!.PropertyType));
        Assert.AreEqual(typeof(BigInt), UnwrapNullable(asBigInt!.PropertyType));
        Assert.AreEqual(typeof(Symbol), UnwrapNullable(asSymbol!.PropertyType));
        Assert.AreEqual(typeof(IObject), UnwrapNullable(asObject!.PropertyType));
        Assert.AreEqual(typeof(Array<RouterErrorValue?>), UnwrapNullable(asArray!.PropertyType));
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(asError).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(asString).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(asNumber).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(asBool).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(asBigInt).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(asSymbol).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(asObject).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(asArray).ReadState);
        Assert.AreEqual(typeof(RouterErrorValue), objectFactory!.ReturnType);
        Assert.AreEqual("__arg1", objectFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual(typeof(RouterErrorValue), arrayOperator!.ReturnType);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(arrayOperator.GetParameters()[0]).ElementType!.ReadState);
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
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }));
        RequiredInstance(methods, nameof(RouterHistory.Push), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), typeof(HistoryState) }));
        RequiredInstance(methods, nameof(RouterHistory.Replace), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }));
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
    public void VueRoute_ViewDepthInjectionKey_UsesStronglyTypedNumberOrWritableRefContract()
    {
        var nullability = new NullabilityInfoContext();
        var viewDepthType = typeof(RouterViewDepthValue);
        var asNumber = viewDepthType.GetProperty(nameof(RouterViewDepthValue.AsNumber), BindingFlags.Public | BindingFlags.Instance);
        var asRef = viewDepthType.GetProperty(nameof(RouterViewDepthValue.AsRef), BindingFlags.Public | BindingFlags.Instance);
        var numberOperator = viewDepthType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(static method => method.Name == "op_Implicit" && method.GetParameters().Single().ParameterType == typeof(Number));
        var intOperator = viewDepthType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(static method => method.Name == "op_Implicit" && method.GetParameters().Single().ParameterType == typeof(int));
        var refFactory = viewDepthType.GetMethod(nameof(RouterViewDepthValue.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(Vue3.IVueRef<Number>) });
        var intRefFactory = viewDepthType.GetMethod(nameof(RouterViewDepthValue.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(Vue3.IVueRef<int>) });

        Assert.IsNotNull(viewDepthType.GetCustomAttribute<System.Runtime.CompilerServices.UnionAttribute>());
        Assert.IsTrue(typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(viewDepthType));
        Assert.IsNotNull(asNumber);
        Assert.IsNotNull(asRef);
        Assert.IsNotNull(numberOperator);
        Assert.IsNotNull(intOperator);
        Assert.IsNotNull(refFactory);
        Assert.IsNotNull(intRefFactory);
        Assert.AreEqual(typeof(Number), UnwrapNullable(asNumber!.PropertyType));
        Assert.AreEqual(typeof(Vue3.IVueRef<Number>), UnwrapNullable(asRef!.PropertyType));
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(asNumber).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(asRef).ReadState);
        Assert.AreEqual(typeof(RouterViewDepthValue), numberOperator!.ReturnType);
        Assert.AreEqual(typeof(RouterViewDepthValue), intOperator!.ReturnType);
        Assert.AreEqual(typeof(RouterViewDepthValue), refFactory!.ReturnType);
        Assert.AreEqual(typeof(RouterViewDepthValue), intRefFactory!.ReturnType);
        Assert.AreEqual("__arg1", refFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", intRefFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
    }

    [TestMethod]
    public void VueRoute_RouteRecordAndMatchedContracts_DifferentiateRawAndLoadedComponents()
    {
        var nullability = new NullabilityInfoContext();
        var routeRecordSingleViewComponent = typeof(RouteRecordSingleView).GetProperty(nameof(RouteRecordSingleView.Component), BindingFlags.Public | BindingFlags.Instance);
        var routeRecordMultipleViewsComponents = typeof(RouteRecordMultipleViews).GetProperty(nameof(RouteRecordMultipleViews.Components), BindingFlags.Public | BindingFlags.Instance);
        var routeRecordSingleViewRedirect = typeof(RouteRecordSingleView).GetProperty(nameof(RouteRecordRedirect.Redirect), BindingFlags.Public | BindingFlags.Instance);
        var routeRecordSingleViewChildren = typeof(RouteRecordSingleView).GetProperty(nameof(RouteRecordRedirect.Children), BindingFlags.Public | BindingFlags.Instance);
        var routeRecordMultipleViewsRedirect = typeof(RouteRecordMultipleViews).GetProperty(nameof(RouteRecordRedirect.Redirect), BindingFlags.Public | BindingFlags.Instance);
        var routeRecordMultipleViewsChildren = typeof(RouteRecordMultipleViews).GetProperty(nameof(RouteRecordRedirect.Children), BindingFlags.Public | BindingFlags.Instance);
        var routeRecordSingleViewWithChildrenRedirect = typeof(RouteRecordSingleViewWithChildren).GetProperty(nameof(RouteRecordRedirect.Redirect), BindingFlags.Public | BindingFlags.Instance);
        var routeRecordMultipleViewsWithChildrenRedirect = typeof(RouteRecordMultipleViewsWithChildren).GetProperty(nameof(RouteRecordRedirect.Redirect), BindingFlags.Public | BindingFlags.Instance);
        var routeRecordRedirectChildren = typeof(RouteRecordRedirect).GetProperty(nameof(RouteRecordRedirect.Children), BindingFlags.Public | BindingFlags.Instance);
        var rawRouteComponentsComponentAdd = typeof(RawRouteComponents).GetMethod(nameof(RawRouteComponents.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(string), typeof(ECMAScript.Vue3.IVueComponent) }, modifiers: null);
        var rawRouteComponentsLoaderAdd = typeof(RawRouteComponents).GetMethod(nameof(RawRouteComponents.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(string), typeof(RouteComponentLoader) }, modifiers: null);
        var routeComponentsComponentAdd = typeof(RouteComponents).GetMethod(nameof(RouteComponents.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(string), typeof(ECMAScript.Vue3.IVueComponent) }, modifiers: null);
        var routeComponentsLoaderAdd = typeof(RouteComponents).GetMethod(nameof(RouteComponents.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(string), typeof(RouteComponentLoader) }, modifiers: null);
        var routeRecordRedirectRedirect = typeof(RouteRecordRedirect)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Single(static property => property.Name == nameof(RouteRecordRedirect.Redirect));
        var routeRecordNormalizedProps = typeof(RouteRecordNormalized).GetProperty(nameof(RouteRecordNormalized.Props), BindingFlags.Public | BindingFlags.Instance);
        var routeLocationMatchedComponents = typeof(RouteLocationMatched)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Single(static property => property.Name == nameof(RouteLocationMatched.Components) && property.PropertyType == typeof(RouteComponents));

        Assert.IsNotNull(routeRecordSingleViewComponent);
        Assert.IsNotNull(routeRecordMultipleViewsComponents);
        Assert.IsNull(routeRecordSingleViewRedirect);
        Assert.IsNull(routeRecordSingleViewChildren);
        Assert.IsNull(routeRecordMultipleViewsRedirect);
        Assert.IsNull(routeRecordMultipleViewsChildren);
        Assert.IsNotNull(routeRecordSingleViewWithChildrenRedirect);
        Assert.IsNotNull(routeRecordMultipleViewsWithChildrenRedirect);
        Assert.IsNotNull(routeRecordRedirectChildren);
        Assert.IsNotNull(rawRouteComponentsComponentAdd);
        Assert.IsNotNull(rawRouteComponentsLoaderAdd);
        Assert.IsNotNull(routeComponentsComponentAdd);
        Assert.IsNotNull(routeComponentsLoaderAdd);
        Assert.IsNotNull(routeRecordRedirectRedirect);
        Assert.IsNotNull(routeRecordNormalizedProps);
        Assert.IsNotNull(routeLocationMatchedComponents);
        Assert.AreEqual(typeof(RawRouteComponent), routeRecordSingleViewComponent!.PropertyType);
        Assert.AreEqual(typeof(RawRouteComponents), routeRecordMultipleViewsComponents!.PropertyType);
        Assert.AreEqual(typeof(RouteRecordRedirectOption), routeRecordRedirectRedirect!.PropertyType);
        Assert.AreEqual(typeof(RouteRecordRedirectOption), Nullable.GetUnderlyingType(routeRecordSingleViewWithChildrenRedirect!.PropertyType) ?? routeRecordSingleViewWithChildrenRedirect.PropertyType);
        Assert.AreEqual(typeof(RouteRecordRedirectOption), Nullable.GetUnderlyingType(routeRecordMultipleViewsWithChildrenRedirect!.PropertyType) ?? routeRecordMultipleViewsWithChildrenRedirect.PropertyType);
        Assert.AreEqual(typeof(RouteRecordRaw[]), Nullable.GetUnderlyingType(routeRecordRedirectChildren!.PropertyType) ?? routeRecordRedirectChildren.PropertyType);
        Assert.AreEqual(typeof(RouteNamedProps), routeRecordNormalizedProps!.PropertyType);
        Assert.AreEqual(typeof(RouteComponents), routeLocationMatchedComponents!.PropertyType);
        Assert.AreEqual(typeof(RouteRecordNormalized), typeof(RouteLocationMatched).BaseType);
        Assert.AreEqual(NullabilityState.NotNull, nullability.Create(routeRecordRedirectRedirect).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(routeRecordSingleViewWithChildrenRedirect).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(routeRecordMultipleViewsWithChildrenRedirect).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(routeRecordRedirectChildren).ReadState);
        Assert.AreEqual(NullabilityState.NotNull, nullability.Create(routeRecordNormalizedProps).ReadState);
    }

    [TestMethod]
    public void VueRoute_RedirectAndGuardDelegates_FollowOfficialRouteContracts()
    {
        var redirectInvoke = typeof(RouteRedirectCallback).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var nextInvoke = typeof(NavigationGuardNext).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var nextCallbackFactory = typeof(NavigationGuardNextArgument).GetMethod(nameof(NavigationGuardNextArgument.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(NavigationGuardNextCallback) });
        var nextObsolete = typeof(NavigationGuardNext).GetCustomAttribute<ObsoleteAttribute>();
        var nextCallbackObsolete = typeof(NavigationGuardNextCallback).GetCustomAttribute<ObsoleteAttribute>();
        var legacyGuardObsolete = typeof(LegacyRouteNavigationGuard).GetCustomAttribute<ObsoleteAttribute>();
        var legacyAsyncGuardObsolete = typeof(LegacyAsyncRouteNavigationGuard).GetCustomAttribute<ObsoleteAttribute>();
        var nextCallbackFactoryObsolete = nextCallbackFactory?.GetCustomAttribute<ObsoleteAttribute>();
        var guardReturnType = typeof(NavigationGuardReturn);
        var errorConstructor = guardReturnType.GetConstructor(new[] { typeof(Error) });
        var nextCallbackConstructor = guardReturnType.GetConstructor(new[] { typeof(NavigationGuardNextCallback) });
        var guardAsCallback = guardReturnType.GetProperty("AsCallback", BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(redirectInvoke);
        Assert.IsNotNull(nextInvoke);
        Assert.IsNotNull(nextCallbackFactory);
        Assert.IsNotNull(nextObsolete);
        Assert.IsNotNull(nextCallbackObsolete);
        Assert.IsNotNull(legacyGuardObsolete);
        Assert.IsNotNull(legacyAsyncGuardObsolete);
        Assert.IsNotNull(nextCallbackFactoryObsolete);
        Assert.IsNotNull(errorConstructor);
        Assert.IsNull(nextCallbackConstructor);
        Assert.IsNull(guardAsCallback);
        StringAssert.Contains(nextObsolete!.Message, "return-based navigation guards");
        StringAssert.Contains(nextCallbackObsolete!.Message, "return-based navigation guards");
        StringAssert.Contains(legacyGuardObsolete!.Message, "backward compatibility");
        StringAssert.Contains(legacyAsyncGuardObsolete!.Message, "backward compatibility");
        StringAssert.Contains(nextCallbackFactoryObsolete!.Message, "beforeRouteEnter");
        CollectionAssert.AreEqual(
            new[] { typeof(RouteLocation), typeof(RouteLocationNormalizedLoaded) },
            redirectInvoke!.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(
            new[] { typeof(NavigationGuardNextArgument) },
            nextInvoke!.GetParameters().Select(static parameter => Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType).ToArray());
        Assert.IsTrue(nextInvoke.GetParameters().Single().HasDefaultValue);
        Assert.IsNull(nextInvoke.GetParameters().Single().DefaultValue);
        Assert.AreEqual(typeof(NavigationGuardNextArgument), nextCallbackFactory!.ReturnType);
        Assert.AreEqual("__arg1", nextCallbackFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual(typeof(RouteLocationRaw), redirectInvoke.ReturnType);
        Assert.AreEqual(typeof(NavigationGuardReturn), errorConstructor!.DeclaringType);
    }

    [TestMethod]
    public void VueRoute_RouteNavigationResult_SeparatesNavigationFailureFromVoidNavigationSuccess()
    {
        var nullability = new NullabilityInfoContext();
        var navigationResultType = typeof(RouteNavigationResult);
        var asFailure = navigationResultType.GetProperty(nameof(RouteNavigationResult.AsFailure), BindingFlags.Public | BindingFlags.Instance);
        var failureConstructor = navigationResultType.GetConstructor(new[] { typeof(NavigationFailure) });
        var push = typeof(Router).GetMethod(nameof(Router.Push), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(RouteLocationRaw) }, modifiers: null);
        var replace = typeof(Router).GetMethod(nameof(Router.Replace), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(RouteLocationRaw) }, modifiers: null);
        var navigateWithoutEvent = typeof(UseLinkReturn).GetMethod(nameof(UseLinkReturn.Navigate), BindingFlags.Public | BindingFlags.Instance, binder: null, types: Type.EmptyTypes, modifiers: null);
        var navigateWithEvent = typeof(UseLinkReturn).GetMethod(nameof(UseLinkReturn.Navigate), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(MouseEvent) }, modifiers: null);
        var slotNavigate = typeof(RouterLinkSlotScope).GetProperty(nameof(RouterLinkSlotScope.Navigate), BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(asFailure);
        Assert.IsNotNull(failureConstructor);
        Assert.IsNotNull(push);
        Assert.IsNotNull(replace);
        Assert.IsNotNull(navigateWithoutEvent);
        Assert.IsNotNull(navigateWithEvent);
        Assert.IsNotNull(slotNavigate);
        Assert.AreEqual(typeof(NavigationFailure), asFailure!.PropertyType);
        Assert.AreEqual(typeof(RouteNavigationResult), failureConstructor!.DeclaringType);
        Assert.AreEqual(typeof(IPromise<RouteNavigationResult?>), push!.ReturnType);
        Assert.AreEqual(typeof(IPromise<RouteNavigationResult?>), replace!.ReturnType);
        Assert.AreEqual(typeof(IPromise<RouteNavigationResult?>), navigateWithoutEvent!.ReturnType);
        Assert.AreEqual(typeof(IPromise<RouteNavigationResult?>), navigateWithEvent!.ReturnType);
        Assert.AreEqual(typeof(RouterLinkNavigateCallback), slotNavigate!.PropertyType);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(push.ReturnParameter).GenericTypeArguments.Single().ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(replace.ReturnParameter).GenericTypeArguments.Single().ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(navigateWithoutEvent.ReturnParameter).GenericTypeArguments.Single().ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(navigateWithEvent.ReturnParameter).GenericTypeArguments.Single().ReadState);
    }

    [TestMethod]
    public void VueRoute_RouterOptions_ExposeGlobalPathParserFlags()
    {
        var routerOptionsType = typeof(RouterOptions);
        var sensitive = routerOptionsType.GetProperty(nameof(RouteRecordBase.Sensitive), BindingFlags.Public | BindingFlags.Instance);
        var strict = routerOptionsType.GetProperty(nameof(RouteRecordBase.Strict), BindingFlags.Public | BindingFlags.Instance);
        var end = routerOptionsType.GetProperty(nameof(RouteRecordBase.End), BindingFlags.Public | BindingFlags.Instance);
        var pathParserEnd = typeof(PathParserOptions).GetProperty(nameof(PathParserOptions.End), BindingFlags.Public | BindingFlags.Instance);
        var routeRecordBaseEnd = typeof(RouteRecordBase).GetProperty(nameof(RouteRecordBase.End), BindingFlags.Public | BindingFlags.Instance);
        var routerEndObsolete = end?.GetCustomAttribute<ObsoleteAttribute>();
        var pathParserEndObsolete = pathParserEnd?.GetCustomAttribute<ObsoleteAttribute>();
        var routeRecordEndObsolete = routeRecordBaseEnd?.GetCustomAttribute<ObsoleteAttribute>();

        Assert.IsNotNull(sensitive);
        Assert.IsNotNull(strict);
        Assert.IsNotNull(end);
        Assert.IsNotNull(pathParserEnd);
        Assert.IsNotNull(routeRecordBaseEnd);
        Assert.IsNotNull(routerEndObsolete);
        Assert.IsNotNull(pathParserEndObsolete);
        Assert.IsNotNull(routeRecordEndObsolete);
        Assert.AreEqual(typeof(bool), Nullable.GetUnderlyingType(sensitive!.PropertyType) ?? sensitive.PropertyType);
        Assert.AreEqual(typeof(bool), Nullable.GetUnderlyingType(strict!.PropertyType) ?? strict.PropertyType);
        Assert.AreEqual(typeof(bool), Nullable.GetUnderlyingType(end!.PropertyType) ?? end.PropertyType);
        StringAssert.Contains(routerEndObsolete!.Message, "deprecated", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(routerEndObsolete.Message, "always true", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(pathParserEndObsolete!.Message, "deprecated", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(pathParserEndObsolete.Message, "always true", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(routeRecordEndObsolete!.Message, "deprecated", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(routeRecordEndObsolete.Message, "always true", StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public void VueRoute_ReactiveRefContracts_PreserveOfficialComputedAndShallowRefSemantics()
    {
        var vue3Type = typeof(Vue3);
        var computedGetter = RequiredStatic(
                vue3Type.GetMethods(BindingFlags.Public | BindingFlags.Static),
                nameof(Vue3.Computed),
                static method =>
                    method.IsGenericMethodDefinition &&
                    method.GetGenericArguments().Length == 1 &&
                    method.ReturnType.IsGenericType &&
                    method.ReturnType.GetGenericTypeDefinition() == typeof(Vue3.VueComputedRef<>) &&
                    method.GetParameters() is var parameters &&
                    parameters.Length == 1 &&
                    parameters[0].ParameterType.IsGenericType &&
                    parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(Func<>))
            .MakeGenericMethod(typeof(bool));
        var shallowRef = RequiredStatic(
                vue3Type.GetMethods(BindingFlags.Public | BindingFlags.Static),
                nameof(Vue3.ShallowRef),
                static method =>
                    method.IsGenericMethodDefinition &&
                    method.GetGenericArguments().Length == 1 &&
                    method.GetParameters() is var parameters &&
                    parameters.Length == 1 &&
                    parameters[0].ParameterType.IsGenericParameter)
            .MakeGenericMethod(typeof(RouteLocationNormalizedLoaded));
        var triggerRef = RequiredStatic(
                vue3Type.GetMethods(BindingFlags.Public | BindingFlags.Static),
                nameof(Vue3.TriggerRef),
                static method =>
                    method.IsGenericMethodDefinition &&
                    method.GetGenericArguments().Length == 1 &&
                    method.GetParameters() is var parameters &&
                    parameters.Length == 1 &&
                    parameters[0].ParameterType.IsGenericType &&
                    parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(Vue3.VueShallowRef<>))
            .MakeGenericMethod(typeof(RouteLocationNormalizedLoaded));
        var toRefGetter = RequiredStatic(
                vue3Type.GetMethods(BindingFlags.Public | BindingFlags.Static),
                nameof(Vue3.ToRef),
                static method =>
                    method.IsGenericMethodDefinition &&
                    method.GetGenericArguments().Length == 1 &&
                    method.ReturnType.IsGenericType &&
                    method.ReturnType.GetGenericTypeDefinition() == typeof(Vue3.VueComputedRef<>) &&
                    method.GetParameters() is var parameters &&
                    parameters.Length == 1 &&
                    parameters[0].ParameterType.IsGenericType &&
                    parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(Func<>))
            .MakeGenericMethod(typeof(RouteLocationAsRelative));
        var useLinkHref = typeof(UseLinkReturn).GetProperty(nameof(UseLinkReturn.Href), BindingFlags.Public | BindingFlags.Instance);
        var useLinkIsActive = typeof(UseLinkReturn).GetProperty(nameof(UseLinkReturn.IsActive), BindingFlags.Public | BindingFlags.Instance);
        var useLinkIsExactActive = typeof(UseLinkReturn).GetProperty(nameof(UseLinkReturn.IsExactActive), BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(computedGetter);
        Assert.IsNotNull(shallowRef);
        Assert.IsNotNull(triggerRef);
        Assert.IsNotNull(toRefGetter);
        Assert.IsNotNull(useLinkHref);
        Assert.IsNotNull(useLinkIsActive);
        Assert.IsNotNull(useLinkIsExactActive);
        Assert.AreEqual(typeof(Vue3.VueComputedRef<bool>), computedGetter!.ReturnType);
        Assert.AreEqual(typeof(Vue3.VueShallowRef<RouteLocationNormalizedLoaded>), shallowRef!.ReturnType);
        Assert.AreEqual(typeof(void), triggerRef!.ReturnType);
        Assert.AreEqual(typeof(Vue3.VueComputedRef<RouteLocationAsRelative>), toRefGetter!.ReturnType);
        Assert.AreEqual(typeof(Vue3.VueComputedRef<string>), useLinkHref!.PropertyType);
        Assert.AreEqual(typeof(Vue3.VueComputedRef<bool>), useLinkIsActive!.PropertyType);
        Assert.AreEqual(typeof(Vue3.VueComputedRef<bool>), useLinkIsExactActive!.PropertyType);
    }

    [TestMethod]
    public void VueRoute_RouteLocationNamedAndPathRaw_Surfaces_AlignWithOfficialQueryHashContracts()
    {
        var queryAndHash = typeof(RouteQueryAndHash);
        var relativeRaw = typeof(LocationAsRelativeRaw);
        var pathBase = typeof(RouteLocationPathRawBase);
        var asPath = typeof(RouteLocationAsPath);
        var pathRaw = typeof(RouteLocationPathRaw);
        var asRelative = typeof(RouteLocationAsRelative);
        var namedRaw = typeof(RouteLocationNamedRaw);
        var query = queryAndHash.GetProperty(nameof(RouteLocationAsPath.Query), BindingFlags.Public | BindingFlags.Instance);
        var hash = queryAndHash.GetProperty(nameof(RouteLocationAsPath.Hash), BindingFlags.Public | BindingFlags.Instance);
        var path = pathRaw.GetProperty(nameof(RouteLocationPathRaw.Path), BindingFlags.Public | BindingFlags.Instance);
        var name = relativeRaw.GetProperty(nameof(RouteLocationNamedRaw.Name), BindingFlags.Public | BindingFlags.Instance);
        var @params = relativeRaw.GetProperty(nameof(RouteLocationNamedRaw.Params), BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(query);
        Assert.IsNotNull(hash);
        Assert.IsNotNull(path);
        Assert.IsNotNull(name);
        Assert.IsNotNull(@params);
        Assert.AreEqual(typeof(LocationQueryRaw), Nullable.GetUnderlyingType(query!.PropertyType) ?? query.PropertyType);
        Assert.AreEqual(typeof(string), Nullable.GetUnderlyingType(hash!.PropertyType) ?? hash.PropertyType);
        Assert.AreEqual(typeof(string), path!.PropertyType);
        Assert.AreEqual(typeof(RouteRecordName), Nullable.GetUnderlyingType(name!.PropertyType) ?? name.PropertyType);
        Assert.AreEqual(typeof(RouteParamsRaw), Nullable.GetUnderlyingType(@params!.PropertyType) ?? @params.PropertyType);
        Assert.AreEqual(typeof(Vue3.VueProps), queryAndHash.BaseType);
        Assert.AreEqual(typeof(RouteLocationOptions), relativeRaw.BaseType);
        Assert.AreEqual(typeof(RouteLocationOptions), pathBase.BaseType);
        Assert.AreEqual(typeof(RouteLocationPathRawBase), asPath.BaseType);
        Assert.AreEqual(typeof(RouteLocationPathRawBase), pathRaw.BaseType);
        Assert.AreEqual(typeof(LocationAsRelativeRaw), asRelative.BaseType);
        Assert.AreEqual(typeof(LocationAsRelativeRaw), namedRaw.BaseType);
    }

    [TestMethod]
    public void VueRoute_RouterMatcherSurface_ExposesOfficialLowLevelMatcherContracts()
    {
        var matcherApi = typeof(VueRoute).GetMethod(nameof(VueRoute.CreateRouterMatcher), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteRecordRaw[]), typeof(PathParserOptions) });
        var matcherType = typeof(RouterMatcher);
        var parserType = typeof(PathParser);
        var recordMatcherType = typeof(RouteRecordMatcher);
        var matcherMethods = matcherType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => !method.IsSpecialName)
            .ToArray();
        var parserMethods = parserType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static method => !method.IsSpecialName)
            .ToArray();
        var parserKeys = parserType.GetProperty(nameof(PathParser.Keys), BindingFlags.Public | BindingFlags.Instance);
        var parserScore = parserType.GetProperty(nameof(PathParser.Score), BindingFlags.Public | BindingFlags.Instance);
        var record = recordMatcherType.GetProperty(nameof(RouteRecordMatcher.Record), BindingFlags.Public | BindingFlags.Instance);
        var parent = recordMatcherType.GetProperty(nameof(RouteRecordMatcher.Parent), BindingFlags.Public | BindingFlags.Instance);
        var children = recordMatcherType.GetProperty(nameof(RouteRecordMatcher.Children), BindingFlags.Public | BindingFlags.Instance);
        var alias = recordMatcherType.GetProperty(nameof(RouteRecordMatcher.Alias), BindingFlags.Public | BindingFlags.Instance);
        var matcherLocationMatched = typeof(MatcherLocation).GetProperty(nameof(MatcherLocation.Matched), BindingFlags.Public | BindingFlags.Instance);
        var matcherAsPath = typeof(MatcherLocationRaw)
            .GetProperty(nameof(MatcherLocationRaw.AsPath), BindingFlags.Public | BindingFlags.Instance);
        var matcherAsNamed = typeof(MatcherLocationRaw)
            .GetProperty(nameof(MatcherLocationRaw.AsNamed), BindingFlags.Public | BindingFlags.Instance);
        var matcherAsRelative = typeof(MatcherLocationRaw)
            .GetProperty(nameof(MatcherLocationRaw.AsRelative), BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(matcherApi);
        Assert.IsNotNull(parserKeys);
        Assert.IsNotNull(parserScore);
        Assert.IsNotNull(record);
        Assert.IsNotNull(parent);
        Assert.IsNotNull(children);
        Assert.IsNotNull(alias);
        Assert.IsNotNull(matcherLocationMatched);
        Assert.IsNotNull(matcherAsPath);
        Assert.IsNotNull(matcherAsNamed);
        Assert.IsNotNull(matcherAsRelative);
        Assert.AreEqual(typeof(RouterMatcher), matcherApi!.ReturnType);
        Assert.AreEqual(typeof(PathParserKey[]), parserKeys!.PropertyType);
        Assert.AreEqual(typeof(Array<Array<Number>>), parserScore!.PropertyType);
        Assert.AreEqual(typeof(RouteRecordNormalized), record!.PropertyType);
        Assert.AreEqual(typeof(RouteRecordMatcher), Nullable.GetUnderlyingType(parent!.PropertyType) ?? parent.PropertyType);
        Assert.AreEqual(typeof(RouteRecordMatcher[]), children!.PropertyType);
        Assert.AreEqual(typeof(RouteRecordMatcher[]), alias!.PropertyType);
        Assert.AreEqual(typeof(RouteRecordNormalized[]), matcherLocationMatched!.PropertyType);
        Assert.AreEqual(typeof(MatcherLocationAsPath), Nullable.GetUnderlyingType(matcherAsPath!.PropertyType) ?? matcherAsPath.PropertyType);
        Assert.AreEqual(typeof(MatcherLocationAsName), Nullable.GetUnderlyingType(matcherAsNamed!.PropertyType) ?? matcherAsNamed.PropertyType);
        Assert.AreEqual(typeof(MatcherLocationAsRelative), Nullable.GetUnderlyingType(matcherAsRelative!.PropertyType) ?? matcherAsRelative.PropertyType);

        RequiredInstance(parserMethods, nameof(PathParser.Parse), static method =>
            method.ReturnType == typeof(RouteParams) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }));
        RequiredInstance(parserMethods, nameof(PathParser.Stringify), static method =>
            method.ReturnType == typeof(string) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouteParams) }));
        RequiredInstance(matcherMethods, nameof(RouterMatcher.AddRoute), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouteRecordRaw) }));
        RequiredInstance(matcherMethods, nameof(RouterMatcher.AddRoute), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouteRecordRaw), typeof(RouteRecordMatcher) }));
        RequiredInstance(matcherMethods, nameof(RouterMatcher.RemoveRoute), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouteRecordMatcher) }));
        RequiredInstance(matcherMethods, nameof(RouterMatcher.RemoveRoute), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouteRecordName) }));
        RequiredInstance(matcherMethods, nameof(RouterMatcher.GetRoutes), static method =>
            method.ReturnType == typeof(RouteRecordMatcher[]) &&
            method.GetParameters().Length == 0);
        RequiredInstance(matcherMethods, nameof(RouterMatcher.GetRecordMatcher), static method =>
            method.ReturnType == typeof(RouteRecordMatcher) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(RouteRecordName) }));
        RequiredInstance(matcherMethods, nameof(RouterMatcher.Resolve), static method =>
            method.ReturnType == typeof(MatcherLocation) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(MatcherLocationRaw), typeof(MatcherLocation) }));
    }

    [TestMethod]
    public void VueRoute_ScrollPositionElement_UsesExplicitSelectorOrDomElementTarget()
    {
        var nullability = new NullabilityInfoContext();
        var targetType = typeof(ScrollPositionTarget);
        var el = typeof(ScrollPositionElement).GetProperty(nameof(ScrollPositionElement.El), BindingFlags.Public | BindingFlags.Instance);
        var asSelector = targetType.GetProperty(nameof(ScrollPositionTarget.AsSelector), BindingFlags.Public | BindingFlags.Instance);
        var asElement = targetType.GetProperty(nameof(ScrollPositionTarget.AsElement), BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(el);
        Assert.IsNotNull(asSelector);
        Assert.IsNotNull(asElement);
        AssertNet11UnionContract(targetType, typeof(string), typeof(Element));
        Assert.AreEqual(typeof(ScrollPositionTarget), el!.PropertyType);
        Assert.AreEqual(typeof(string), UnwrapNullable(asSelector!.PropertyType));
        Assert.AreEqual(typeof(Element), UnwrapNullable(asElement!.PropertyType));
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(asSelector).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(asElement).ReadState);
    }

    [TestMethod]
    public void VueRoute_RouterScrollHandler_ExposesExplicitFactoryHelpers_ForObjectInitializerAuthoring()
    {
        var syncFactory = typeof(RouterScrollHandler).GetMethod(nameof(RouterScrollHandler.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouterScrollBehavior) });
        var asyncFactory = typeof(RouterScrollHandler).GetMethod(nameof(RouterScrollHandler.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(AsyncRouterScrollBehavior) });

        Assert.IsNotNull(syncFactory);
        Assert.IsNotNull(asyncFactory);
        Assert.IsNotNull(typeof(RouterScrollHandler).GetCustomAttribute<System.Runtime.CompilerServices.UnionAttribute>());
        Assert.IsTrue(typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(typeof(RouterScrollHandler)));
        Assert.AreEqual(typeof(RouterScrollHandler), syncFactory!.ReturnType);
        Assert.AreEqual(typeof(RouterScrollHandler), asyncFactory!.ReturnType);
        Assert.AreEqual("__arg1", syncFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", asyncFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
    }

    [TestMethod]
    public void VueRoute_RouterScrollResult_IncludesNormalizedSavedPositionContract()
    {
        var nullability = new NullabilityInfoContext();
        var asNormalized = typeof(RouterScrollResult).GetProperty(nameof(RouterScrollResult.AsNormalized), BindingFlags.Public | BindingFlags.Instance);
        var normalizedOperator = typeof(RouterScrollResult)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(static method => method.Name == "op_Implicit" && method.GetParameters().Single().ParameterType == typeof(ScrollPositionNormalized));

        Assert.IsNotNull(asNormalized);
        Assert.IsNotNull(normalizedOperator);
        Assert.IsNotNull(typeof(RouterScrollResult).GetCustomAttribute<System.Runtime.CompilerServices.UnionAttribute>());
        Assert.IsTrue(typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(typeof(RouterScrollResult)));
        Assert.AreEqual(typeof(ScrollPositionNormalized), UnwrapNullable(asNormalized!.PropertyType));
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(asNormalized).ReadState);
        Assert.AreEqual(typeof(RouterScrollResult), normalizedOperator!.ReturnType);
    }

    [TestMethod]
    public void VueRoute_RouterScrollResult_PreservesPreciseProjectionForOverlappingPositionTypes()
    {
        RouterScrollResult coordinates = new ScrollPositionCoordinates();
        RouterScrollResult element = new ScrollPositionElement();

        Assert.IsNotNull(coordinates.AsCoordinates);
        Assert.IsNull(coordinates.AsElement);
        Assert.IsNull(element.AsCoordinates);
        Assert.IsNotNull(element.AsElement);
    }

    [TestMethod]
    public void VueRoute_WithChildrenRouteRecords_ExposeOptionalShellComponentContracts()
    {
        var nullability = new NullabilityInfoContext();
        var singleViewWithChildrenType = typeof(RouteRecordSingleViewWithChildren);
        var multipleViewsWithChildrenType = typeof(RouteRecordMultipleViewsWithChildren);
        var singleViewComponent = singleViewWithChildrenType.GetProperty(nameof(RouteRecordSingleViewWithChildren.Component), BindingFlags.Public | BindingFlags.Instance);
        var singleViewChildren = singleViewWithChildrenType.GetProperty(nameof(RouteRecordSingleViewWithChildren.Children), BindingFlags.Public | BindingFlags.Instance);
        var multipleViewsComponents = multipleViewsWithChildrenType.GetProperty(nameof(RouteRecordMultipleViewsWithChildren.Components), BindingFlags.Public | BindingFlags.Instance);
        var multipleViewsChildren = multipleViewsWithChildrenType.GetProperty(nameof(RouteRecordMultipleViewsWithChildren.Children), BindingFlags.Public | BindingFlags.Instance);

        Assert.AreEqual(typeof(RouteRecordBase), singleViewWithChildrenType.BaseType);
        Assert.AreEqual(typeof(RouteRecordBase), multipleViewsWithChildrenType.BaseType);
        Assert.IsNotNull(singleViewComponent);
        Assert.IsNotNull(singleViewChildren);
        Assert.IsNotNull(multipleViewsComponents);
        Assert.IsNotNull(multipleViewsChildren);
        Assert.AreEqual(typeof(RawRouteComponent?), singleViewComponent!.PropertyType);
        Assert.AreEqual(typeof(RouteRecordRaw[]), singleViewChildren!.PropertyType);
        Assert.AreEqual(typeof(RawRouteComponents), multipleViewsComponents!.PropertyType);
        Assert.AreEqual(typeof(RouteRecordRaw[]), multipleViewsChildren!.PropertyType);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(singleViewComponent).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(multipleViewsComponents!).ReadState);
    }

    [TestMethod]
    public void VueRoute_RouteRecordAuthoringSurface_EncodesOfficialMutualExclusionConstraints_InCSharpTypes()
    {
        var singleViewType = typeof(RouteRecordSingleView);
        var multipleViewsType = typeof(RouteRecordMultipleViews);
        var redirectType = typeof(RouteRecordRedirect);
        var singleViewWithChildrenType = typeof(RouteRecordSingleViewWithChildren);
        var multipleViewsWithChildrenType = typeof(RouteRecordMultipleViewsWithChildren);

        Assert.IsNull(singleViewType.GetProperty(nameof(RouteRecordRedirect.Redirect), BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(singleViewType.GetProperty(nameof(RouteRecordRedirect.Children), BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(multipleViewsType.GetProperty(nameof(RouteRecordRedirect.Redirect), BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(multipleViewsType.GetProperty(nameof(RouteRecordRedirect.Children), BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(redirectType.GetProperty(nameof(RouteRecordSingleView.Component), BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(redirectType.GetProperty(nameof(RouteRecordMultipleViews.Components), BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNull(redirectType.GetProperty(nameof(RouteRecordSingleView.Props), BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNotNull(singleViewWithChildrenType.GetProperty(nameof(RouteRecordRedirect.Redirect), BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNotNull(multipleViewsWithChildrenType.GetProperty(nameof(RouteRecordRedirect.Redirect), BindingFlags.Public | BindingFlags.Instance));
        Assert.IsNotNull(redirectType.GetProperty(nameof(RouteRecordRedirect.Children), BindingFlags.Public | BindingFlags.Instance));
    }

    [TestMethod]
    public void VueRoute_MultipleViewRouteRecordProps_SupportGlobalBooleanOrNamedDictionaryContracts()
    {
        var multipleViewsProps = typeof(RouteRecordMultipleViews).GetProperty(nameof(RouteRecordMultipleViews.Props), BindingFlags.Public | BindingFlags.Instance);
        var multipleViewsWithChildrenProps = typeof(RouteRecordMultipleViewsWithChildren).GetProperty(nameof(RouteRecordMultipleViewsWithChildren.Props), BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(multipleViewsProps);
        Assert.IsNotNull(multipleViewsWithChildrenProps);
        Assert.AreEqual(typeof(RouteRecordNamedViewProps), Nullable.GetUnderlyingType(multipleViewsProps!.PropertyType) ?? multipleViewsProps.PropertyType);
        Assert.AreEqual(typeof(RouteRecordNamedViewProps), Nullable.GetUnderlyingType(multipleViewsWithChildrenProps!.PropertyType) ?? multipleViewsWithChildrenProps.PropertyType);
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
        var routerLinkSlotInvoke = typeof(RouterLinkSlotCallback).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var routerViewSlotInvoke = typeof(RouterViewSlotCallback).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var routeMaybeRefFromRef = typeof(RouteLocationRawMaybeRef).GetMethod(nameof(RouteLocationRawMaybeRef.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(Vue3.IVueRef<RouteLocationAsRelative>) });
        var routeMaybeRefFromReadonly = typeof(RouteLocationRawMaybeRef).GetMethod(nameof(RouteLocationRawMaybeRef.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(Vue3.VueReadonlyRef<RouteLocationAsRelative>) });
        var boolMaybeRefFromReadonly = typeof(RouteBooleanMaybeRef).GetMethod(nameof(RouteBooleanMaybeRef.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(Vue3.VueReadonlyRef<bool>) });
        var routeReadonlyConstructor = typeof(RouteLocationRawMaybeRef).GetConstructor(new[] { typeof(Vue3.VueReadonlyRef<RouteLocationAsRelative>) });
        var boolReadonlyConstructor = typeof(RouteBooleanMaybeRef).GetConstructor(new[] { typeof(Vue3.VueReadonlyRef<bool>) });

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
        Assert.IsNotNull(routerLinkSlotInvoke);
        Assert.IsNotNull(routerViewSlotInvoke);
        Assert.IsNotNull(routeMaybeRefFromRef);
        Assert.IsNotNull(routeMaybeRefFromReadonly);
        Assert.IsNotNull(boolMaybeRefFromReadonly);
        Assert.IsNotNull(routeReadonlyConstructor);
        Assert.IsNotNull(boolReadonlyConstructor);
        Assert.AreEqual(typeof(RouteLocationRawMaybeRef), useLinkTo!.PropertyType);
        Assert.AreEqual(typeof(RouteBooleanMaybeRef), Nullable.GetUnderlyingType(useLinkReplace!.PropertyType) ?? useLinkReplace.PropertyType);
        Assert.AreEqual(typeof(UseLinkReturn), useLinkMethod!.ReturnType);
        Assert.AreEqual(typeof(Vue3.VueComputedRef<RouteLocationResolved>), useLinkReturnRoute!.PropertyType);
        Assert.AreEqual(typeof(RouteLocationRaw), routerLinkTo!.PropertyType);
        Assert.AreEqual(typeof(bool), Nullable.GetUnderlyingType(routerLinkReplace!.PropertyType) ?? routerLinkReplace.PropertyType);
        Assert.AreEqual(typeof(RouteLocationResolved), routerLinkScopeRoute!.PropertyType);
        Assert.AreEqual(typeof(string), routerLinkScopeHref!.PropertyType);
        Assert.AreEqual(typeof(RouterLinkNavigateCallback), routerLinkScopeNavigate!.PropertyType);
        Assert.AreEqual(typeof(RouteLocationNormalized), routeProp!.PropertyType);
        Assert.AreEqual(typeof(Vue3.IVNode), slotComponent!.PropertyType);
        Assert.AreEqual(typeof(Vue3.IVNode[]), routerLinkSlotInvoke!.ReturnType);
        Assert.AreEqual(typeof(Vue3.IVNode[]), routerViewSlotInvoke!.ReturnType);
        Assert.AreEqual("__arg1", routeMaybeRefFromRef!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", routeMaybeRefFromReadonly!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", boolMaybeRefFromReadonly!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual(typeof(RouteLocationRawMaybeRef), routeReadonlyConstructor!.DeclaringType);
        Assert.AreEqual(typeof(RouteBooleanMaybeRef), boolReadonlyConstructor!.DeclaringType);
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
        var redirectErrorType = typeof(NavigationRedirectError);
        var redirectErrorTo = redirectErrorType.GetProperty(nameof(NavigationRedirectError.To), BindingFlags.Public | BindingFlags.Instance);
        var redirectErrorFrom = redirectErrorType.GetProperty(nameof(NavigationRedirectError.From), BindingFlags.Public | BindingFlags.Instance);
        var redirectErrorKind = redirectErrorType.GetProperty(nameof(NavigationRedirectError.Type), BindingFlags.Public | BindingFlags.Instance);

        Assert.AreEqual(typeof(RouteLocationPathRawBase), typeof(RouteLocationAsPath).BaseType);
        Assert.AreEqual(typeof(LocationAsRelativeRaw), typeof(RouteLocationAsRelative).BaseType);
        Assert.AreEqual(typeof(RouteLocation), resolved.BaseType);
        Assert.AreEqual(typeof(RouteLocationNormalized), typeof(RouteLocationNormalizedLoaded).BaseType);
        Assert.IsTrue(routeLocationBase.GetProperty(nameof(RouteLocation.Replace), BindingFlags.Public | BindingFlags.Instance) is not null);
        Assert.IsTrue(routeLocationBase.GetProperty(nameof(RouteLocation.State), BindingFlags.Public | BindingFlags.Instance) is not null);
        Assert.IsNull(normalized.GetProperty(nameof(RouteLocation.Replace), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.IsNotNull(normalized.GetProperty(nameof(RouteLocationNormalized.RedirectedFrom), BindingFlags.Public | BindingFlags.Instance));
        Assert.AreEqual(typeof(RouteLocation), normalized.GetProperty(nameof(RouteLocationNormalized.RedirectedFrom), BindingFlags.Public | BindingFlags.Instance)!.PropertyType);
        Assert.IsNotNull(failureFrom);
        Assert.AreEqual(typeof(RouteLocationNormalized), failureFrom!.PropertyType);
        Assert.AreEqual(typeof(Error), redirectErrorType.BaseType);
        Assert.IsNotNull(redirectErrorTo);
        Assert.IsNotNull(redirectErrorFrom);
        Assert.IsNotNull(redirectErrorKind);
        Assert.AreEqual(typeof(RouteLocationRaw), redirectErrorTo!.PropertyType);
        Assert.AreEqual(typeof(RouteLocationNormalized), redirectErrorFrom!.PropertyType);
        Assert.AreEqual(typeof(ErrorTypes), redirectErrorKind!.PropertyType);
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

	private static void AssertNet11UnionContract(Type unionType, params Type[] constructorBranchTypes)
	{
        Assert.IsNotNull(unionType.GetCustomAttribute<System.Runtime.CompilerServices.UnionAttribute>(), unionType.FullName);
        Assert.IsTrue(typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(unionType), unionType.FullName);

        var value = unionType.GetProperty(nameof(System.Runtime.CompilerServices.IUnion.Value), BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(value, unionType.FullName);
        Assert.AreEqual(typeof(object), value!.PropertyType);

        CollectionAssert.AreEquivalent(
            constructorBranchTypes,
            unionType
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Select(static constructor => constructor.GetParameters().SingleOrDefault()?.ParameterType)
                .Where(static type => type is not null)
                .ToArray(),
            unionType.FullName);

        AssertNoAssignableBranchOverlap(unionType, constructorBranchTypes);
    }

    private static void AssertNoAssignableBranchOverlap(Type unionType, Type[] constructorBranchTypes)
    {
        foreach (var left in constructorBranchTypes)
        foreach (var right in constructorBranchTypes)
        {
            if (left == right)
                continue;

            Assert.IsFalse(
                left.IsAssignableFrom(right),
                $"{unionType.FullName} cannot use native union because branch {right.FullName} is assignable to {left.FullName}; keep a tagged [Union] + IUnion wrapper to preserve exact AsX projections.");
        }
    }

    private static bool IsUnionValueProperty(PropertyInfo property)
        => property.Name == nameof(System.Runtime.CompilerServices.IUnion.Value) &&
           typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(property.DeclaringType);
}

#pragma warning restore CA1416
