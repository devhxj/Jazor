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
            typeof(RouteMetaValue),
            typeof(HistoryState),
            typeof(LocationQuery),
            typeof(LocationQueryRaw),
            typeof(RouteParams),
            typeof(RouteParamsRaw),
            typeof(RouteLocationRawMaybeRef),
            typeof(RouteBooleanMaybeRef),
            typeof(HistoryStateValue),
            typeof(RouteNavigationResult),
            typeof(RawRouteComponents),
            typeof(RouteComponents),
            typeof(RouteNamedProps),
            typeof(RouteRecordNamedViewProps),
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
        var from = typeof(RouteComponent).GetMethod(nameof(RouteComponent.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(ECMAScript.VueContract.IVueComponent) });
        var fromLoader = typeof(RouteComponent).GetMethod(nameof(RouteComponent.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteComponentLoader) });
        var rawFrom = typeof(RawRouteComponent).GetMethod(nameof(RawRouteComponent.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(ECMAScript.VueContract.IVueComponent) });
        var rawFromLoader = typeof(RawRouteComponent).GetMethod(nameof(RawRouteComponent.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteComponentLoader) });
        var implicitOperators = typeof(RawRouteComponent)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(static method => method.Name == "op_Implicit")
            .ToArray();
        var implicitComponent = implicitOperators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(RouteComponent));
        var implicitLoader = implicitOperators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(RouteComponentLoader));
        var routeComponentOperators = typeof(RouteComponent)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(static method => method.Name == "op_Implicit")
            .ToArray();
        var routeImplicitLoader = routeComponentOperators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(RouteComponentLoader));

        Assert.IsNotNull(from);
        Assert.IsNotNull(fromLoader);
        Assert.IsNotNull(rawFrom);
        Assert.IsNotNull(rawFromLoader);
        Assert.IsNotNull(implicitComponent);
        Assert.IsNotNull(implicitLoader);
        Assert.IsNotNull(routeImplicitLoader);
        Assert.AreEqual(typeof(RouteComponent), from!.ReturnType);
        Assert.AreEqual("__arg1", from.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        CollectionAssert.AreEqual(new[] { typeof(ECMAScript.VueContract.IVueComponent) }, from.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(typeof(RouteComponent), fromLoader!.ReturnType);
        Assert.AreEqual("__arg1", fromLoader.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        CollectionAssert.AreEqual(new[] { typeof(RouteComponentLoader) }, fromLoader.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());

        Assert.AreEqual(typeof(RawRouteComponent), rawFrom!.ReturnType);
        Assert.AreEqual("__arg1", rawFrom.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        CollectionAssert.AreEqual(new[] { typeof(ECMAScript.VueContract.IVueComponent) }, rawFrom.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(typeof(RawRouteComponent), rawFromLoader!.ReturnType);
        Assert.AreEqual("__arg1", rawFromLoader.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        CollectionAssert.AreEqual(new[] { typeof(RouteComponentLoader) }, rawFromLoader.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());

        Assert.AreEqual(typeof(RawRouteComponent), implicitComponent!.ReturnType);
        CollectionAssert.AreEqual(new[] { typeof(RouteComponent) }, implicitComponent.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(typeof(RawRouteComponent), implicitLoader!.ReturnType);
        CollectionAssert.AreEqual(new[] { typeof(RouteComponentLoader) }, implicitLoader.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(typeof(RouteComponent), routeImplicitLoader!.ReturnType);
        CollectionAssert.AreEqual(new[] { typeof(RouteComponentLoader) }, routeImplicitLoader.GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
    }

    [TestMethod]
    public void VueRoute_RoutePropsAndRedirectUnions_ExposeExplicitFactories_AndNamedPropsAddOverloads()
    {
        var routeRecordPropsType = typeof(RouteRecordProps);
        var namedViewPropsType = typeof(RouteRecordNamedViewProps);
        var redirectOptionType = typeof(RouteRedirectOption);
        var namedPropsType = typeof(RouteNamedProps);

        var propsBoolFactory = routeRecordPropsType.GetMethod(nameof(RouteRecordProps.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(bool) });
        var propsObjectFactory = routeRecordPropsType.GetMethod(nameof(RouteRecordProps.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(Vue3.VueProps) });
        var propsResolverFactory = routeRecordPropsType.GetMethod(nameof(RouteRecordProps.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteRecordPropsResolver) });
        var namedViewBoolFactory = namedViewPropsType.GetMethod(nameof(RouteRecordNamedViewProps.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(bool) });
        var namedViewDictionaryFactory = namedViewPropsType.GetMethod(nameof(RouteRecordNamedViewProps.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteNamedProps) });
        var redirectLocationFactory = redirectOptionType.GetMethod(nameof(RouteRedirectOption.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteLocationRaw) });
        var redirectCallbackFactory = redirectOptionType.GetMethod(nameof(RouteRedirectOption.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouteRedirectCallback) });
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
        Assert.AreEqual("__arg1", propsBoolFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", propsObjectFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", propsResolverFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", namedViewBoolFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", namedViewDictionaryFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", redirectLocationFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", redirectCallbackFactory.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
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
        AssertEcmaScriptSupport(typeof(RouteLocation));
        AssertEcmaScriptSupport(typeof(RouteLocationRawMaybeRef));
        AssertEcmaScriptSupport(typeof(RouteBooleanMaybeRef));
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
    public void VueRoute_QueryValueUnions_SupportNullableArrayPayloads_ForOfficialQuerySemantics()
    {
        var locationQueryValueArray = typeof(LocationQueryValue).GetProperty(nameof(LocationQueryValue.AsArray), BindingFlags.Public | BindingFlags.Instance);
        var locationQueryRawArray = typeof(LocationQueryValueRaw).GetProperty(nameof(LocationQueryValueRaw.AsArray), BindingFlags.Public | BindingFlags.Instance);
        var locationQueryValueArrayOperator = typeof(LocationQueryValue)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(static method => method.Name == "op_Implicit" && method.GetParameters().Single().ParameterType == typeof(string[]));
        var locationQueryValueRawMixedArrayOperator = typeof(LocationQueryValueRaw)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(static method => method.Name == "op_Implicit" && method.GetParameters().Single().ParameterType == typeof(LocationQueryValueRaw?[]));

        Assert.IsNotNull(locationQueryValueArray);
        Assert.IsNotNull(locationQueryRawArray);
        Assert.IsNotNull(locationQueryValueArrayOperator);
        Assert.IsNotNull(locationQueryValueRawMixedArrayOperator);
        Assert.AreEqual(typeof(Array<string?>), locationQueryValueArray!.PropertyType);
        Assert.AreEqual(typeof(Array<LocationQueryValueRaw?>), locationQueryRawArray!.PropertyType);
        Assert.AreEqual(typeof(LocationQueryValue), locationQueryValueArrayOperator!.ReturnType);
        Assert.AreEqual(typeof(LocationQueryValueRaw), locationQueryValueRawMixedArrayOperator!.ReturnType);
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
        Assert.AreEqual(typeof(Vue3.VueReadonlyRef<RouteLocationNormalizedLoaded>), currentRoute!.PropertyType);
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

        Assert.AreEqual(9, routerMethods.Length);
        CollectionAssert.AreEquivalent(
            new[]
            {
                typeof(ErrorRouterErrorHandler),
                typeof(NavigationFailureRouterErrorHandler),
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

        Assert.IsTrue(errorType.IsDefined(typeof(ECMAScriptUnionAttribute), inherit: false));

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
        var nullability = new NullabilityInfoContext();
        var routeRecordSingleViewComponent = typeof(RouteRecordSingleView).GetProperty(nameof(RouteRecordSingleView.Component), BindingFlags.Public | BindingFlags.Instance);
        var routeRecordMultipleViewsComponents = typeof(RouteRecordMultipleViews).GetProperty(nameof(RouteRecordMultipleViews.Components), BindingFlags.Public | BindingFlags.Instance);
        var rawRouteComponentsComponentAdd = typeof(RawRouteComponents).GetMethod(nameof(RawRouteComponents.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(string), typeof(ECMAScript.VueContract.IVueComponent) }, modifiers: null);
        var rawRouteComponentsLoaderAdd = typeof(RawRouteComponents).GetMethod(nameof(RawRouteComponents.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(string), typeof(RouteComponentLoader) }, modifiers: null);
        var routeComponentsComponentAdd = typeof(RouteComponents).GetMethod(nameof(RouteComponents.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(string), typeof(ECMAScript.VueContract.IVueComponent) }, modifiers: null);
        var routeComponentsLoaderAdd = typeof(RouteComponents).GetMethod(nameof(RouteComponents.Add), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(string), typeof(RouteComponentLoader) }, modifiers: null);
        var routeRecordRedirectRedirect = typeof(RouteRecordRedirect)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Single(static property => property.Name == nameof(RouteRecordBase.Redirect));
        var routeRecordNormalizedProps = typeof(RouteRecordNormalized).GetProperty(nameof(RouteRecordNormalized.Props), BindingFlags.Public | BindingFlags.Instance);
        var routeLocationMatchedComponents = typeof(RouteLocationMatched)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Single(static property => property.Name == nameof(RouteLocationMatched.Components) && property.PropertyType == typeof(RouteComponents));

        Assert.IsNotNull(routeRecordSingleViewComponent);
        Assert.IsNotNull(routeRecordMultipleViewsComponents);
        Assert.IsNotNull(rawRouteComponentsComponentAdd);
        Assert.IsNotNull(rawRouteComponentsLoaderAdd);
        Assert.IsNotNull(routeComponentsComponentAdd);
        Assert.IsNotNull(routeComponentsLoaderAdd);
        Assert.IsNotNull(routeRecordRedirectRedirect);
        Assert.IsNotNull(routeRecordNormalizedProps);
        Assert.IsNotNull(routeLocationMatchedComponents);
        Assert.AreEqual(typeof(RawRouteComponent), routeRecordSingleViewComponent!.PropertyType);
        Assert.AreEqual(typeof(RawRouteComponents), routeRecordMultipleViewsComponents!.PropertyType);
        Assert.AreEqual(typeof(RouteRedirectOption), routeRecordRedirectRedirect!.PropertyType);
        Assert.AreEqual(typeof(RouteNamedProps), routeRecordNormalizedProps!.PropertyType);
        Assert.AreEqual(typeof(RouteComponents), routeLocationMatchedComponents!.PropertyType);
        Assert.AreEqual(typeof(RouteRecordNormalized), typeof(RouteLocationMatched).BaseType);
        Assert.AreEqual(NullabilityState.NotNull, nullability.Create(routeRecordRedirectRedirect).ReadState);
        Assert.AreEqual(NullabilityState.NotNull, nullability.Create(routeRecordNormalizedProps).ReadState);
    }

    [TestMethod]
    public void VueRoute_RedirectAndGuardDelegates_FollowOfficialRouteContracts()
    {
        var redirectInvoke = typeof(RouteRedirectCallback).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var nextInvoke = typeof(NavigationGuardNext).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        var nextCallbackFactory = typeof(NavigationGuardNextArgument).GetMethod(nameof(NavigationGuardNextArgument.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(NavigationGuardNextCallback) });
        var guardReturnType = typeof(NavigationGuardReturn);
        var guardReturnOperators = guardReturnType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(static method => method.Name == "op_Implicit")
            .ToArray();
        var errorReturn = guardReturnOperators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(Error));
        var nextCallbackReturn = guardReturnOperators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(NavigationGuardNextCallback));
        var guardAsCallback = guardReturnType.GetProperty("AsCallback", BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(redirectInvoke);
        Assert.IsNotNull(nextInvoke);
        Assert.IsNotNull(nextCallbackFactory);
        Assert.IsNotNull(errorReturn);
        Assert.IsNull(nextCallbackReturn);
        Assert.IsNull(guardAsCallback);
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
        Assert.AreEqual(typeof(NavigationGuardReturn), errorReturn!.ReturnType);
    }

    [TestMethod]
    public void VueRoute_RouteNavigationResult_SeparatesNavigationFailureFromVoidNavigationSuccess()
    {
        var nullability = new NullabilityInfoContext();
        var navigationResultType = typeof(RouteNavigationResult);
        var asFailure = navigationResultType.GetProperty(nameof(RouteNavigationResult.AsFailure), BindingFlags.Public | BindingFlags.Instance);
        var failureOperator = navigationResultType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(static method => method.Name == "op_Implicit" && method.GetParameters().Single().ParameterType == typeof(NavigationFailure));
        var push = typeof(Router).GetMethod(nameof(Router.Push), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(RouteLocationRaw) }, modifiers: null);
        var replace = typeof(Router).GetMethod(nameof(Router.Replace), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(RouteLocationRaw) }, modifiers: null);
        var navigateWithoutEvent = typeof(UseLinkReturn).GetMethod(nameof(UseLinkReturn.Navigate), BindingFlags.Public | BindingFlags.Instance, binder: null, types: Type.EmptyTypes, modifiers: null);
        var navigateWithEvent = typeof(UseLinkReturn).GetMethod(nameof(UseLinkReturn.Navigate), BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(MouseEvent) }, modifiers: null);
        var slotNavigate = typeof(RouterLinkSlotScope).GetProperty(nameof(RouterLinkSlotScope.Navigate), BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(asFailure);
        Assert.IsNotNull(failureOperator);
        Assert.IsNotNull(push);
        Assert.IsNotNull(replace);
        Assert.IsNotNull(navigateWithoutEvent);
        Assert.IsNotNull(navigateWithEvent);
        Assert.IsNotNull(slotNavigate);
        Assert.AreEqual(typeof(NavigationFailure), asFailure!.PropertyType);
        Assert.AreEqual(typeof(RouteNavigationResult), failureOperator!.ReturnType);
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

        Assert.IsNotNull(sensitive);
        Assert.IsNotNull(strict);
        Assert.IsNotNull(end);
        Assert.AreEqual(typeof(bool), Nullable.GetUnderlyingType(sensitive!.PropertyType) ?? sensitive.PropertyType);
        Assert.AreEqual(typeof(bool), Nullable.GetUnderlyingType(strict!.PropertyType) ?? strict.PropertyType);
        Assert.AreEqual(typeof(bool), Nullable.GetUnderlyingType(end!.PropertyType) ?? end.PropertyType);
    }

    [TestMethod]
    public void VueRoute_ScrollPositionElement_UsesExplicitSelectorOrDomElementTarget()
    {
        var nullability = new NullabilityInfoContext();
        var targetType = typeof(ScrollPositionTarget);
        var el = typeof(ScrollPositionElement).GetProperty(nameof(ScrollPositionElement.El), BindingFlags.Public | BindingFlags.Instance);
        var asSelector = targetType.GetProperty(nameof(ScrollPositionTarget.AsSelector), BindingFlags.Public | BindingFlags.Instance);
        var asElement = targetType.GetProperty(nameof(ScrollPositionTarget.AsElement), BindingFlags.Public | BindingFlags.Instance);
        var selectorOperator = targetType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(static method => method.Name == "op_Implicit" && method.GetParameters().Single().ParameterType == typeof(string));
        var elementOperator = targetType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(static method => method.Name == "op_Implicit" && method.GetParameters().Single().ParameterType == typeof(Element));

        Assert.IsNotNull(el);
        Assert.IsNotNull(asSelector);
        Assert.IsNotNull(asElement);
        Assert.IsNotNull(selectorOperator);
        Assert.IsNotNull(elementOperator);
        Assert.IsTrue(targetType.IsDefined(typeof(ECMAScriptUnionAttribute), inherit: false));
        Assert.AreEqual(typeof(ScrollPositionTarget), el!.PropertyType);
        Assert.AreEqual(typeof(string), UnwrapNullable(asSelector!.PropertyType));
        Assert.AreEqual(typeof(Element), UnwrapNullable(asElement!.PropertyType));
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(asSelector).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(asElement).ReadState);
        Assert.AreEqual(typeof(ScrollPositionTarget), selectorOperator!.ReturnType);
        Assert.AreEqual(typeof(ScrollPositionTarget), elementOperator!.ReturnType);
    }

    [TestMethod]
    public void VueRoute_RouterScrollHandler_ExposesExplicitFactoryHelpers_ForObjectInitializerAuthoring()
    {
        var syncFactory = typeof(RouterScrollHandler).GetMethod(nameof(RouterScrollHandler.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(RouterScrollBehavior) });
        var asyncFactory = typeof(RouterScrollHandler).GetMethod(nameof(RouterScrollHandler.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(AsyncRouterScrollBehavior) });

        Assert.IsNotNull(syncFactory);
        Assert.IsNotNull(asyncFactory);
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
        Assert.AreEqual(typeof(ScrollPositionNormalized), UnwrapNullable(asNormalized!.PropertyType));
        Assert.AreEqual(NullabilityState.Nullable, nullability.Create(asNormalized).ReadState);
        Assert.AreEqual(typeof(RouterScrollResult), normalizedOperator!.ReturnType);
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
        var routeMaybeRefOperators = typeof(RouteLocationRawMaybeRef)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(static method => method.Name == "op_Implicit")
            .ToArray();
        var boolMaybeRefOperators = typeof(RouteBooleanMaybeRef)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(static method => method.Name == "op_Implicit")
            .ToArray();
        var routeMaybeRefFromRef = typeof(RouteLocationRawMaybeRef).GetMethod(nameof(RouteLocationRawMaybeRef.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(Vue3.IVueRef<RouteLocationAsRelative>) });
        var routeMaybeRefFromReadonly = typeof(RouteLocationRawMaybeRef).GetMethod(nameof(RouteLocationRawMaybeRef.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(Vue3.VueReadonlyRef<RouteLocationAsRelative>) });
        var boolMaybeRefFromReadonly = typeof(RouteBooleanMaybeRef).GetMethod(nameof(RouteBooleanMaybeRef.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(Vue3.VueReadonlyRef<bool>) });
        var routeReadonlyOperator = routeMaybeRefOperators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(Vue3.VueReadonlyRef<RouteLocationAsRelative>));
        var boolReadonlyOperator = boolMaybeRefOperators.SingleOrDefault(static method => method.GetParameters().Single().ParameterType == typeof(Vue3.VueReadonlyRef<bool>));

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
        Assert.IsNotNull(routeReadonlyOperator);
        Assert.IsNotNull(boolReadonlyOperator);
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
        Assert.AreEqual(typeof(Vue3.IVNode[]), routerLinkSlotInvoke!.ReturnType);
        Assert.AreEqual(typeof(Vue3.IVNode[]), routerViewSlotInvoke!.ReturnType);
        Assert.AreEqual("__arg1", routeMaybeRefFromRef!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", routeMaybeRefFromReadonly!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual("__arg1", boolMaybeRefFromReadonly!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
        Assert.AreEqual(typeof(RouteLocationRawMaybeRef), routeReadonlyOperator!.ReturnType);
        Assert.AreEqual(typeof(RouteBooleanMaybeRef), boolReadonlyOperator!.ReturnType);
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
