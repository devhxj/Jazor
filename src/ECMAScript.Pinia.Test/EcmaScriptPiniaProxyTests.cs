using System.Reflection;
using ECMAScript.Contract;

namespace ECMAScript.PiniaTests;

#pragma warning disable CA1416
#pragma warning disable CS0626

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
		AssertEcmaScriptSupport(typeof(Pinia.StoreDefinition));

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
			typeof(Pinia.PiniaPluginContext<>).MakeGenericType(typeof(TestPiniaStore)),
			typeof(Pinia.PiniaPluginContext<,>).MakeGenericType(typeof(TestPiniaStore), typeof(TestPiniaPluginOptions)),
			typeof(Pinia.PiniaPluginContext<,,>).MakeGenericType(typeof(TestPiniaStore), typeof(TestPiniaPluginOptions), typeof(TestPiniaPluginExtensions)),
			typeof(Pinia.PiniaPluginContext<,,,>).MakeGenericType(typeof(TestPiniaStore), typeof(TestPiniaPluginOptions), typeof(TestPiniaPluginExtensions), typeof(TestPiniaPluginStateExtensions)),
			typeof(Pinia.StoreProperties),
			typeof(Pinia.StoreGeneric),
			typeof(Pinia.StoreDefinition),
			typeof(Pinia.Store<>).MakeGenericType(typeof(TestPiniaState)),
			typeof(Pinia.SubscriptionMutation<>).MakeGenericType(typeof(TestPiniaState)),
			typeof(Pinia.SubscriptionMutationDirect<>).MakeGenericType(typeof(TestPiniaState)),
			typeof(Pinia.SubscriptionMutationPatchFunction<>).MakeGenericType(typeof(TestPiniaState)),
			typeof(Pinia.SubscriptionMutationPatchObject<>).MakeGenericType(typeof(TestPiniaState)),
			typeof(Pinia.SubscriptionMutationEvents),
			typeof(Pinia.StoreActionListenerContext),
			typeof(Pinia.StoreActionListenerContext<>).MakeGenericType(typeof(Pinia.StoreGeneric)),
			typeof(Pinia.StoreDefinition<>).MakeGenericType(typeof(Pinia.StoreGeneric)),
			typeof(Pinia.ProjectedStore<,>).MakeGenericType(typeof(TestPiniaStore), typeof(TestPiniaPluginExtensions)),
			typeof(Pinia.ProjectedStore<,,>).MakeGenericType(typeof(TestPiniaStore), typeof(TestPiniaPluginExtensions), typeof(TestPiniaPluginStateExtensions)),
			typeof(Pinia.ProjectedStoreDefinition<,>).MakeGenericType(typeof(TestPiniaStore), typeof(TestPiniaPluginExtensions)),
			typeof(Pinia.ProjectedStoreDefinition<,,>).MakeGenericType(typeof(TestPiniaStore), typeof(TestPiniaPluginExtensions), typeof(TestPiniaPluginStateExtensions)),
			typeof(Pinia.StoreRefs<>).MakeGenericType(typeof(Pinia.StoreGeneric)),
			typeof(Pinia.DefineStoreOptions<>).MakeGenericType(typeof(TestPiniaState)),
			typeof(Pinia.DefineStoreOptionsInPlugin),
			typeof(Pinia.DefineStoreOptionsInPlugin<>).MakeGenericType(typeof(TestPiniaState)),
			typeof(Pinia.DefineStoreOptionsInPlugin<,,>).MakeGenericType(typeof(TestPiniaState), typeof(TestPiniaGetters), typeof(TestPiniaActions)),
			typeof(Pinia.DefineSetupStoreOptions),
			typeof(Pinia.DefineSetupStoreOptions<>).MakeGenericType(typeof(TestPiniaActions)),
			typeof(Pinia.PiniaStatePatch<>).MakeGenericType(typeof(TestPiniaState)),
			typeof(Pinia.SetupStoreHelpers),
			typeof(Pinia.PiniaKeyMapper),
			typeof(Pinia.PiniaStateMapper<>).MakeGenericType(typeof(Pinia.StoreGeneric)),
			typeof(Pinia.PiniaStateMapValue<>).MakeGenericType(typeof(Pinia.StoreGeneric)),
			typeof(Pinia.PiniaValue)
		};

		foreach (var type in runtimeTypes)
		{
			foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
			{
				if (IsUnionValueProperty(property))
					continue;

				AssertNotObject(property.PropertyType, $"{type.Name}.{property.Name}");
			}

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
		Assert.AreEqual(typeof(Vue3.VueWatchOptions), subscribeOptions.BaseType);

		var detached = subscribeOptions.GetProperty(nameof(Pinia.SubscribeOptions.Detached), BindingFlags.Public | BindingFlags.Instance);
		var flush = subscribeOptions.GetProperty(nameof(Pinia.SubscribeOptions.Flush), BindingFlags.Public | BindingFlags.Instance);
		var immediate = subscribeOptions.GetProperty(nameof(Vue3.VueWatchOptions.Immediate), BindingFlags.Public | BindingFlags.Instance);
		var deep = subscribeOptions.GetProperty(nameof(Vue3.VueWatchOptions.Deep), BindingFlags.Public | BindingFlags.Instance);
		var once = subscribeOptions.GetProperty(nameof(Vue3.VueWatchOptions.Once), BindingFlags.Public | BindingFlags.Instance);
		var onTrack = subscribeOptions.GetProperty(nameof(Vue3.VueWatchEffectOptions.OnTrack), BindingFlags.Public | BindingFlags.Instance);
		var onTrigger = subscribeOptions.GetProperty(nameof(Vue3.VueWatchEffectOptions.OnTrigger), BindingFlags.Public | BindingFlags.Instance);

		Assert.IsNotNull(detached);
		Assert.IsNotNull(flush);
		Assert.IsNotNull(immediate);
		Assert.IsNotNull(deep);
		Assert.IsNotNull(once);
		Assert.IsNotNull(onTrack);
		Assert.IsNotNull(onTrigger);
		Assert.AreEqual(typeof(bool?), detached!.PropertyType);
		Assert.AreEqual(typeof(Vue3.VueWatchFlush?), flush!.PropertyType);
		Assert.AreEqual(typeof(bool?), immediate!.PropertyType);
		Assert.AreEqual(typeof(Vue3.VueWatchDeep?), deep!.PropertyType);
		Assert.AreEqual(typeof(bool?), once!.PropertyType);
		Assert.AreEqual(typeof(global::ECMAScript.VueDebuggerCallback), onTrack!.PropertyType);
		Assert.AreEqual(typeof(global::ECMAScript.VueDebuggerCallback), onTrigger!.PropertyType);
	}

	[TestMethod]
	public void Pinia_SubscriptionMutationSurface_ModelsOfficialMutationVariants()
	{
		CollectionAssert.Contains(
			typeof(Pinia.MutationType).CustomAttributes.Select(static attribute => attribute.AttributeType.Name).ToArray(),
			"StringAttribute");

		var baseMutation = typeof(Pinia.SubscriptionMutation<>).MakeGenericType(typeof(TestPiniaState));
		var directMutation = typeof(Pinia.SubscriptionMutationDirect<>).MakeGenericType(typeof(TestPiniaState));
		var patchFunctionMutation = typeof(Pinia.SubscriptionMutationPatchFunction<>).MakeGenericType(typeof(TestPiniaState));
		var patchObjectMutation = typeof(Pinia.SubscriptionMutationPatchObject<>).MakeGenericType(typeof(TestPiniaState));

		Assert.IsTrue(baseMutation.IsAssignableFrom(directMutation));
		Assert.IsTrue(baseMutation.IsAssignableFrom(patchFunctionMutation));
		Assert.IsTrue(baseMutation.IsAssignableFrom(patchObjectMutation));

		var baseEvents = baseMutation.GetProperty(nameof(Pinia.SubscriptionMutation<TestPiniaState>.Events), BindingFlags.Public | BindingFlags.Instance);
		var directEvents = directMutation.GetProperty(nameof(Pinia.SubscriptionMutationDirect<TestPiniaState>.Events), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		var patchFunctionEvents = patchFunctionMutation.GetProperty(nameof(Pinia.SubscriptionMutationPatchFunction<TestPiniaState>.Events), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		var patchObjectEvents = patchObjectMutation.GetProperty(nameof(Pinia.SubscriptionMutationPatchObject<TestPiniaState>.Events), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		var patchObjectPayload = patchObjectMutation.GetProperty(nameof(Pinia.SubscriptionMutationPatchObject<TestPiniaState>.Payload), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

		Assert.IsNotNull(baseEvents);
		Assert.IsNotNull(directEvents);
		Assert.IsNotNull(patchFunctionEvents);
		Assert.IsNotNull(patchObjectEvents);
		Assert.IsNotNull(patchObjectPayload);
		Assert.AreEqual(typeof(Pinia.SubscriptionMutationEvents?), baseEvents!.PropertyType);
		Assert.AreEqual(typeof(Vue3.VueDebuggerEvent), directEvents!.PropertyType);
		Assert.AreEqual(typeof(Vue3.VueDebuggerEvent[]), patchFunctionEvents!.PropertyType);
		Assert.AreEqual(typeof(Vue3.VueDebuggerEvent[]), patchObjectEvents!.PropertyType);
		Assert.AreEqual(typeof(Pinia.PiniaStatePatch<TestPiniaState>), patchObjectPayload!.PropertyType);
	}

	[TestMethod]
	public void Pinia_StatePatchSurface_UsesExplicitTypedPatchContracts()
	{
		var storeType = typeof(Pinia.Store<>).MakeGenericType(typeof(TestPiniaState));
		var patchMethods = storeType
			.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
			.Where(static method => method.Name == nameof(Pinia.Store<TestPiniaState>.Patch))
			.OrderBy(static method => method.GetParameters()[0].ParameterType.Name)
			.ToArray();

		Assert.AreEqual(2, patchMethods.Length);
		Assert.AreEqual(typeof(Pinia.PiniaStatePatch<TestPiniaState>), patchMethods[0].GetParameters()[0].ParameterType);
		Assert.AreEqual(typeof(PiniaStatePatchCallback<TestPiniaState>), patchMethods[1].GetParameters()[0].ParameterType);

		var patchContract = typeof(Pinia.PiniaStatePatch<>).MakeGenericType(typeof(TestPiniaState));
		Assert.IsTrue(typeof(Vue3.VueProps).IsAssignableFrom(patchContract));
	}

	[TestMethod]
	public void Pinia_SetupStoreSurface_ExposesHelperAwareOverloads()
	{
		var setupDefineStoreMethods = typeof(Pinia)
			.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
			.Where(static method => method.Name == nameof(Pinia.DefineStore))
			.Where(static method => method.GetGenericArguments().Length == 1)
			.Where(static method => method.GetParameters().Length >= 2)
			.Where(static method => method.GetParameters()[0].ParameterType == typeof(string))
			.ToArray();

		Assert.IsTrue(setupDefineStoreMethods.Any(static method =>
			method.GetParameters().Length == 2 &&
			method.GetParameters()[1].ParameterType.IsGenericType &&
			method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<>)));
		Assert.IsTrue(setupDefineStoreMethods.Any(static method =>
			method.GetParameters().Length == 2 &&
			method.GetParameters()[1].ParameterType.IsGenericType &&
			method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(PiniaSetupStoreFactory<>)));
		Assert.IsTrue(setupDefineStoreMethods.Any(static method =>
			method.GetParameters().Length == 3 &&
			method.GetParameters()[1].ParameterType.IsGenericType &&
			method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<>) &&
			method.GetParameters()[2].ParameterType == typeof(Pinia.DefineSetupStoreOptions)));
		Assert.IsTrue(setupDefineStoreMethods.Any(static method =>
			method.GetParameters().Length == 3 &&
			method.GetParameters()[1].ParameterType.IsGenericType &&
			method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(PiniaSetupStoreFactory<>) &&
			method.GetParameters()[2].ParameterType == typeof(Pinia.DefineSetupStoreOptions)));

		var helperType = typeof(Pinia.SetupStoreHelpers);
		var helperActionMethods = helperType
			.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
			.Where(static method => method.Name == nameof(Pinia.SetupStoreHelpers.Action))
			.ToArray();

		Assert.AreEqual(34, helperActionMethods.Length);
		Assert.IsTrue(helperActionMethods.All(static method => method.GetParameters().Length == 2));
		Assert.IsTrue(helperActionMethods.All(static method => method.GetParameters()[1].ParameterType == typeof(string)));
		Assert.IsTrue(helperActionMethods.Any(static method => method.ReturnType == typeof(Action)));
		Assert.IsTrue(helperActionMethods.Any(static method =>
			method.ReturnType.IsGenericType &&
			method.ReturnType.GetGenericTypeDefinition() == typeof(Func<>)));
		Assert.IsTrue(helperActionMethods.Any(static method =>
			method.ReturnType.IsGenericType &&
			method.ReturnType.GetGenericTypeDefinition() == typeof(Action<,,,,,,,,,,,,,,,>)));
		Assert.IsTrue(helperActionMethods.Any(static method =>
			method.ReturnType.IsGenericType &&
			method.ReturnType.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,,,,,,,,,>)));

		var setupActions = typeof(Pinia.DefineSetupStoreOptions)
			.GetProperty(nameof(Pinia.DefineSetupStoreOptions.Actions), BindingFlags.Public | BindingFlags.Instance);
		var typedSetupActions = typeof(Pinia.DefineSetupStoreOptions<TestPiniaActions>)
			.GetProperty(nameof(Pinia.DefineSetupStoreOptions<TestPiniaActions>.Actions), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

		Assert.IsNotNull(setupActions);
		Assert.IsNotNull(typedSetupActions);
		Assert.AreEqual(typeof(Vue3.VueProps), setupActions!.PropertyType);
		Assert.AreEqual(typeof(TestPiniaActions), typedSetupActions!.PropertyType);
	}

	[TestMethod]
	public void Pinia_ActionAndPluginProxySurface_ExposeTypedContracts()
	{
		var storeGeneric = typeof(Pinia.StoreGeneric);
		var onActionMethods = storeGeneric
			.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
			.Where(static method => method.Name == nameof(Pinia.StoreGeneric.OnAction))
			.OrderBy(static method => method.GetParameters().Length)
			.ThenBy(static method => method.IsGenericMethodDefinition)
			.ToArray();

		Assert.AreEqual(4, onActionMethods.Length);
		Assert.IsFalse(onActionMethods[0].IsGenericMethodDefinition);
		Assert.IsTrue(onActionMethods[1].IsGenericMethodDefinition);
		Assert.IsFalse(onActionMethods[2].IsGenericMethodDefinition);
		Assert.IsTrue(onActionMethods[3].IsGenericMethodDefinition);
		Assert.AreEqual(typeof(PiniaStoreActionListener), onActionMethods[0].GetParameters()[0].ParameterType);
		Assert.AreEqual(typeof(PiniaStoreActionListener<>), onActionMethods[1].GetParameters()[0].ParameterType.GetGenericTypeDefinition());
		Assert.AreEqual(typeof(PiniaStoreActionListener), onActionMethods[2].GetParameters()[0].ParameterType);
		Assert.AreEqual(typeof(PiniaStoreActionListener<>), onActionMethods[3].GetParameters()[0].ParameterType.GetGenericTypeDefinition());
		Assert.AreEqual(typeof(bool), onActionMethods[2].GetParameters()[1].ParameterType);
		Assert.AreEqual(typeof(bool), onActionMethods[3].GetParameters()[1].ParameterType);

		var afterMethods = typeof(Pinia.StoreActionListenerContext)
			.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
			.Where(static method => method.Name == nameof(Pinia.StoreActionListenerContext.After))
			.OrderBy(static method => method.GetParameters()[0].ParameterType.Name)
			.ThenBy(static method => method.IsGenericMethodDefinition)
			.ToArray();

		Assert.AreEqual(3, afterMethods.Length);
		Assert.AreEqual(typeof(Action), afterMethods[0].GetParameters()[0].ParameterType);
		Assert.AreEqual(typeof(Action<Pinia.PiniaValue?>), afterMethods[1].GetParameters()[0].ParameterType);
		Assert.IsTrue(afterMethods[2].IsGenericMethodDefinition);
		Assert.AreEqual(typeof(Action<>), afterMethods[2].GetParameters()[0].ParameterType.GetGenericTypeDefinition());

		var onErrorMethods = typeof(Pinia.StoreActionListenerContext)
			.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
			.Where(static method => method.Name == nameof(Pinia.StoreActionListenerContext.OnError))
			.ToArray();
		var onAnyError = typeof(Pinia.StoreActionListenerContext)
			.GetMethod(nameof(Pinia.StoreActionListenerContext.OnAnyError), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

		Assert.AreEqual(2, onErrorMethods.Length);
		Assert.IsTrue(onErrorMethods.Any(static method =>
			!method.IsGenericMethodDefinition &&
			method.GetParameters()[0].ParameterType == typeof(Action<Error>)));
		Assert.IsTrue(onErrorMethods.Any(static method =>
			method.IsGenericMethodDefinition &&
			method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Action<>)));
		Assert.IsNotNull(onAnyError);
		Assert.AreEqual(typeof(Action<Pinia.PiniaValue?>), onAnyError!.GetParameters()[0].ParameterType);

		var projectActionContextMethods = typeof(Pinia)
			.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
			.Where(static method => method.Name == nameof(Pinia.ProjectActionContext))
			.OrderBy(static method => method.GetGenericArguments().Length)
			.ToArray();
		var tryProjectActionContextMethods = typeof(Pinia)
			.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
			.Where(static method => method.Name == nameof(Pinia.TryProjectActionContext))
			.OrderBy(static method => method.GetGenericArguments().Length)
			.ToArray();
		var actionArgsViewType = typeof(Pinia.ActionArgsView<int, string, bool, double>);
		var actionArgsViewBaseType = typeof(Pinia.ActionArgsView);
		var actionContextProjectionType = typeof(Pinia.ProjectedActionContext<,,>).MakeGenericType(typeof(TestPiniaStore), typeof(string), actionArgsViewType);
		var projectedActionName = actionContextProjectionType.GetProperty(nameof(Pinia.ProjectedActionContext<TestPiniaStore, string, Pinia.ActionArgsView<int, string, bool, double>>.ActionName), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		var projectedActionArgs = actionContextProjectionType.GetProperty(nameof(Pinia.ProjectedActionContext<TestPiniaStore, string, Pinia.ActionArgsView<int, string, bool, double>>.ActionArgs), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		var actionArgsFirst = actionArgsViewType.GetProperty(nameof(Pinia.ActionArgsView<int, string, bool, double>.Arg0), BindingFlags.Public | BindingFlags.Instance);
		var actionArgsSecond = actionArgsViewType.GetProperty(nameof(Pinia.ActionArgsView<int, string, bool, double>.Arg1), BindingFlags.Public | BindingFlags.Instance);
		var actionArgsThird = actionArgsViewType.GetProperty(nameof(Pinia.ActionArgsView<int, string, bool, double>.Arg2), BindingFlags.Public | BindingFlags.Instance);
		var actionArgsFourth = actionArgsViewType.GetProperty(nameof(Pinia.ActionArgsView<int, string, bool, double>.Arg3), BindingFlags.Public | BindingFlags.Instance);
		var actionArgsLength = actionArgsViewBaseType.GetProperty(nameof(Pinia.ActionArgsView.Length), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		var actionArgsViewHighType = typeof(Pinia.ActionArgsView<int, string, bool, double, long, decimal, float, byte, short, uint, ulong, char, DateTime, Guid, TimeSpan, object>);
		var actionArgsHighLast = actionArgsViewHighType.GetProperty(nameof(Pinia.ActionArgsView<int, string, bool, double, long, decimal, float, byte, short, uint, ulong, char, DateTime, Guid, TimeSpan, object>.Arg15), BindingFlags.Public | BindingFlags.Instance);
		var actionArgsArities = new[]
		{
			typeof(Pinia.ActionArgsView),
			typeof(Pinia.ActionArgsView<>),
			typeof(Pinia.ActionArgsView<,>),
			typeof(Pinia.ActionArgsView<,,>),
			typeof(Pinia.ActionArgsView<,,,>),
			typeof(Pinia.ActionArgsView<,,,,>),
			typeof(Pinia.ActionArgsView<,,,,,>),
			typeof(Pinia.ActionArgsView<,,,,,,>),
			typeof(Pinia.ActionArgsView<,,,,,,,>),
			typeof(Pinia.ActionArgsView<,,,,,,,,>),
			typeof(Pinia.ActionArgsView<,,,,,,,,,>),
			typeof(Pinia.ActionArgsView<,,,,,,,,,,>),
			typeof(Pinia.ActionArgsView<,,,,,,,,,,,>),
			typeof(Pinia.ActionArgsView<,,,,,,,,,,,,>),
			typeof(Pinia.ActionArgsView<,,,,,,,,,,,,,>),
			typeof(Pinia.ActionArgsView<,,,,,,,,,,,,,,>),
			typeof(Pinia.ActionArgsView<,,,,,,,,,,,,,,,>)
		};

		Assert.AreEqual(1, projectActionContextMethods.Length);
		Assert.AreEqual(1, tryProjectActionContextMethods.Length);
		Assert.AreEqual(3, projectActionContextMethods[0].GetGenericArguments().Length);
		Assert.AreEqual(3, tryProjectActionContextMethods[0].GetGenericArguments().Length);
		Assert.AreEqual(typeof(Pinia.StoreActionListenerContext<>), projectActionContextMethods[0].GetParameters()[0].ParameterType.GetGenericTypeDefinition());
		Assert.AreEqual(typeof(Pinia.StoreActionListenerContext<>), tryProjectActionContextMethods[0].GetParameters()[0].ParameterType.GetGenericTypeDefinition());
		Assert.AreEqual(typeof(string), tryProjectActionContextMethods[0].GetParameters()[1].ParameterType);
		Assert.AreEqual(typeof(Pinia.ProjectedActionContext<,,>), projectActionContextMethods[0].ReturnType.GetGenericTypeDefinition());
		Assert.AreEqual(typeof(Pinia.ProjectedActionContext<,,>), Nullable.GetUnderlyingType(tryProjectActionContextMethods[0].ReturnType)?.GetGenericTypeDefinition() ?? tryProjectActionContextMethods[0].ReturnType.GetGenericTypeDefinition());
		Assert.AreEqual("__arg1", projectActionContextMethods[0].GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
		Assert.AreEqual("(__arg1.name === __arg2 ? __arg1 : null)", tryProjectActionContextMethods[0].GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
		CollectionAssert.AreEqual(
			new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 },
			actionArgsArities.Select(static type => type.GetGenericArguments().Length).ToArray());
		Assert.IsTrue(actionArgsArities.Skip(1).All(type => actionArgsViewBaseType.IsAssignableFrom(type)));
		Assert.IsNotNull(projectedActionName);
		Assert.IsNotNull(projectedActionArgs);
		Assert.AreEqual(typeof(string), projectedActionName!.PropertyType);
		Assert.AreEqual(actionArgsViewType, projectedActionArgs!.PropertyType);
		Assert.IsNotNull(actionArgsFirst);
		Assert.IsNotNull(actionArgsSecond);
		Assert.IsNotNull(actionArgsThird);
		Assert.IsNotNull(actionArgsFourth);
		Assert.IsNotNull(actionArgsLength);
		Assert.AreEqual(typeof(int), actionArgsFirst!.PropertyType);
		Assert.AreEqual(typeof(string), actionArgsSecond!.PropertyType);
		Assert.AreEqual(typeof(bool), actionArgsThird!.PropertyType);
		Assert.AreEqual(typeof(double), actionArgsFourth!.PropertyType);
		Assert.AreEqual(typeof(int), actionArgsLength!.PropertyType);
		Assert.IsNotNull(actionArgsHighLast);
		Assert.AreEqual(typeof(object), actionArgsHighLast!.PropertyType);

		var pluginContextOptions = typeof(Pinia.PiniaPluginContext)
			.GetProperty(nameof(Pinia.PiniaPluginContext.Options), BindingFlags.Public | BindingFlags.Instance);
		var typedPluginContextStore = typeof(Pinia.PiniaPluginContext<TestPiniaStore>)
			.GetProperty(nameof(Pinia.PiniaPluginContext<TestPiniaStore>.Store), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		var typedPluginContextOptions = typeof(Pinia.PiniaPluginContext<TestPiniaStore, TestPiniaPluginOptions>)
			.GetProperty(nameof(Pinia.PiniaPluginContext<TestPiniaStore, TestPiniaPluginOptions>.Options), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		var projectedPluginContextStore = typeof(Pinia.PiniaPluginContext<TestPiniaStore, TestPiniaPluginOptions, TestPiniaPluginExtensions>)
			.GetProperty(nameof(Pinia.PiniaPluginContext<TestPiniaStore, TestPiniaPluginOptions, TestPiniaPluginExtensions>.Store), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		var projectedPluginContextStoreWithState = typeof(Pinia.PiniaPluginContext<TestPiniaStore, TestPiniaPluginOptions, TestPiniaPluginExtensions, TestPiniaPluginStateExtensions>)
			.GetProperty(nameof(Pinia.PiniaPluginContext<TestPiniaStore, TestPiniaPluginOptions, TestPiniaPluginExtensions, TestPiniaPluginStateExtensions>.Store), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

		Assert.IsNotNull(pluginContextOptions);
		Assert.IsNotNull(typedPluginContextStore);
		Assert.IsNotNull(typedPluginContextOptions);
		Assert.IsNotNull(projectedPluginContextStore);
		Assert.IsNotNull(projectedPluginContextStoreWithState);
		Assert.AreEqual(typeof(Pinia.DefineStoreOptionsInPlugin), pluginContextOptions!.PropertyType);
		Assert.AreEqual(typeof(TestPiniaStore), typedPluginContextStore!.PropertyType);
		Assert.AreEqual(typeof(TestPiniaPluginOptions), typedPluginContextOptions!.PropertyType);
		Assert.AreEqual(typeof(Pinia.ProjectedStore<TestPiniaStore, TestPiniaPluginExtensions>), projectedPluginContextStore!.PropertyType);
		Assert.AreEqual(typeof(Pinia.ProjectedStore<TestPiniaStore, TestPiniaPluginExtensions, TestPiniaPluginStateExtensions>), projectedPluginContextStoreWithState!.PropertyType);

		var useMethods = typeof(Pinia.PiniaInstance)
			.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
			.Where(static method => method.Name == nameof(Pinia.PiniaInstance.Use))
			.ToArray();

		Assert.IsTrue(useMethods.Any(static method =>
			!method.IsGenericMethodDefinition &&
			method.GetParameters().Length == 1 &&
			method.GetParameters()[0].ParameterType == typeof(PiniaPlugin)));
		Assert.IsTrue(useMethods.Any(static method =>
			method.IsGenericMethodDefinition &&
			method.GetGenericArguments().Length == 1 &&
			method.GetParameters().Length == 1 &&
			method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(PiniaPlugin<>)));
		Assert.IsTrue(useMethods.Any(static method =>
			method.IsGenericMethodDefinition &&
			method.GetGenericArguments().Length == 2 &&
			method.GetParameters().Length == 1 &&
			method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(PiniaPlugin<,>)));
		Assert.IsTrue(useMethods.Any(static method =>
			method.IsGenericMethodDefinition &&
			method.GetGenericArguments().Length == 3 &&
			method.GetParameters().Length == 1 &&
			method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(PiniaPlugin<,,>)));
		Assert.IsTrue(useMethods.Any(static method =>
			method.IsGenericMethodDefinition &&
			method.GetGenericArguments().Length == 4 &&
			method.GetParameters().Length == 1 &&
			method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(PiniaPlugin<,,,>)));
		Assert.IsTrue(useMethods.Any(static method =>
			method.IsGenericMethodDefinition &&
			method.GetGenericArguments().Length == 5 &&
			method.GetParameters().Length == 1 &&
			method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(PiniaPlugin<,,,,>)));
	}

	[TestMethod]
	public void Pinia_ProjectedPluginSurface_ExposesExplicitStoreAndDefinitionViews()
	{
		var projectedStore = typeof(Pinia.ProjectedStore<,>).MakeGenericType(typeof(TestPiniaStore), typeof(TestPiniaPluginExtensions));
		var projectedStoreWithState = typeof(Pinia.ProjectedStore<,,>).MakeGenericType(typeof(TestPiniaStore), typeof(TestPiniaPluginExtensions), typeof(TestPiniaPluginStateExtensions));
		var projectedDefinition = typeof(Pinia.ProjectedStoreDefinition<,>).MakeGenericType(typeof(TestPiniaStore), typeof(TestPiniaPluginExtensions));
		var projectedDefinitionWithState = typeof(Pinia.ProjectedStoreDefinition<,,>).MakeGenericType(typeof(TestPiniaStore), typeof(TestPiniaPluginExtensions), typeof(TestPiniaPluginStateExtensions));

		var asStore = projectedStore.GetMethod(nameof(Pinia.ProjectedStore<TestPiniaStore, TestPiniaPluginExtensions>.AsStore), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		var asCustomProperties = projectedStore.GetMethod(nameof(Pinia.ProjectedStore<TestPiniaStore, TestPiniaPluginExtensions>.AsCustomProperties), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		var asCustomState = projectedStoreWithState.GetMethod(nameof(Pinia.ProjectedStore<TestPiniaStore, TestPiniaPluginExtensions, TestPiniaPluginStateExtensions>.AsCustomState), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		var asDefinition = projectedDefinition.GetMethod(nameof(Pinia.ProjectedStoreDefinition<TestPiniaStore, TestPiniaPluginExtensions>.AsDefinition), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		var useProjected = projectedDefinitionWithState
			.GetMethods(BindingFlags.Public | BindingFlags.Instance)
			.Where(static method => method.Name == nameof(Pinia.ProjectedStoreDefinition<TestPiniaStore, TestPiniaPluginExtensions, TestPiniaPluginStateExtensions>.Use))
			.OrderBy(static method => method.GetParameters().Length)
			.ToArray();

		Assert.IsNotNull(asStore);
		Assert.IsNotNull(asCustomProperties);
		Assert.IsNotNull(asCustomState);
		Assert.IsNotNull(asDefinition);
		Assert.AreEqual(typeof(TestPiniaStore), asStore!.ReturnType);
		Assert.AreEqual(typeof(TestPiniaPluginExtensions), asCustomProperties!.ReturnType);
		Assert.AreEqual(typeof(TestPiniaPluginStateExtensions), asCustomState!.ReturnType);
		Assert.AreEqual(typeof(Pinia.StoreDefinition<TestPiniaStore>), asDefinition!.ReturnType);
		Assert.AreEqual("__arg1", asStore.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
		Assert.AreEqual("__arg1", asCustomProperties.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
		Assert.AreEqual("__arg1.$state", asCustomState.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
		Assert.AreEqual("__arg1", asDefinition.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);

		CollectionAssert.AreEqual(
			new[] { 0, 1, 2 },
			useProjected.Select(static method => method.GetParameters().Length).ToArray());
		Assert.AreEqual(typeof(Pinia.ProjectedStore<TestPiniaStore, TestPiniaPluginExtensions, TestPiniaPluginStateExtensions>), useProjected[0].ReturnType);
		Assert.AreEqual("__arg1()", useProjected[0].GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
		Assert.AreEqual("__arg1(__arg2)", useProjected[1].GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
		Assert.AreEqual("__arg1(__arg2, __arg3)", useProjected[2].GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);

		var projectStoreMethods = typeof(Pinia)
			.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
			.Where(static method => method.Name == nameof(Pinia.ProjectStore))
			.OrderBy(static method => method.GetGenericArguments().Length)
			.ToArray();
		var projectDefinitionMethods = typeof(Pinia)
			.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
			.Where(static method => method.Name == nameof(Pinia.ProjectStoreDefinition))
			.OrderBy(static method => method.GetGenericArguments().Length)
			.ToArray();

		Assert.AreEqual(2, projectStoreMethods.Length);
		Assert.AreEqual(2, projectDefinitionMethods.Length);
		Assert.IsTrue(projectStoreMethods.All(static method => method.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode == "__arg1"));
		Assert.IsTrue(projectDefinitionMethods.All(static method => method.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode == "__arg1"));
		Assert.AreEqual(typeof(Pinia.ProjectedStore<,>), projectStoreMethods[0].ReturnType.GetGenericTypeDefinition());
		Assert.AreEqual(typeof(Pinia.ProjectedStore<,,>), projectStoreMethods[1].ReturnType.GetGenericTypeDefinition());
		Assert.AreEqual(typeof(Pinia.ProjectedStoreDefinition<,>), projectDefinitionMethods[0].ReturnType.GetGenericTypeDefinition());
		Assert.AreEqual(typeof(Pinia.ProjectedStoreDefinition<,,>), projectDefinitionMethods[1].ReturnType.GetGenericTypeDefinition());
		Assert.IsTrue(typeof(Pinia.StoreDefinition<Pinia.ProjectedStore<TestPiniaStore, TestPiniaPluginExtensions>>).IsAssignableFrom(projectedDefinition));
		Assert.IsTrue(typeof(Pinia.StoreDefinition<Pinia.ProjectedStore<TestPiniaStore, TestPiniaPluginExtensions, TestPiniaPluginStateExtensions>>).IsAssignableFrom(projectedDefinitionWithState));
	}

	[TestMethod]
	public void Pinia_HelperApiSurface_UsesTypedMappers_And_NonGenericStoreDefinitionBase()
	{
		var helperNames = typeof(Pinia)
			.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
			.Select(static method => method.Name)
			.ToHashSet(StringComparer.Ordinal);

		CollectionAssert.IsSubsetOf(
			new[]
			{
				nameof(Pinia.MapState),
				nameof(Pinia.MapGetters),
				nameof(Pinia.MapWritableState),
				nameof(Pinia.MapActions),
				nameof(Pinia.MapStores),
				nameof(Pinia.SetMapStoreSuffix)
			},
			helperNames.ToArray());

		var mapStores = typeof(Pinia)
			.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
			.Single(static method => method.Name == nameof(Pinia.MapStores) && method.GetGenericArguments().Length == 0);

		var mapStoresParams = mapStores.GetParameters();
		Assert.AreEqual(1, mapStoresParams.Length);
		Assert.AreEqual(typeof(Pinia.StoreDefinition[]), mapStoresParams[0].ParameterType);

		Assert.IsTrue(typeof(Vue3.VueDictionary<string>).IsAssignableFrom(typeof(Pinia.PiniaKeyMapper)));
		Assert.IsTrue(typeof(Vue3.VueDictionary<Pinia.PiniaStateMapValue<Pinia.StoreGeneric>>)
			.IsAssignableFrom(typeof(Pinia.PiniaStateMapper<Pinia.StoreGeneric>)));

		var mapValueType = typeof(Pinia.PiniaStateMapValue<Pinia.StoreGeneric>);
		var fromKey = mapValueType.GetMethod(nameof(Pinia.PiniaStateMapValue<Pinia.StoreGeneric>.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(string) });
		var fromSelector = mapValueType.GetMethod(nameof(Pinia.PiniaStateMapValue<Pinia.StoreGeneric>.From), BindingFlags.Public | BindingFlags.Static, new[] { typeof(PiniaMapStateSelector<Pinia.StoreGeneric>) });

		Assert.IsNotNull(fromKey);
		Assert.IsNotNull(fromSelector);
		Assert.AreEqual(typeof(Pinia.PiniaStateMapValue<Pinia.StoreGeneric>), fromKey!.ReturnType);
		Assert.AreEqual(typeof(Pinia.PiniaStateMapValue<Pinia.StoreGeneric>), fromSelector!.ReturnType);
		Assert.AreEqual("__arg1", fromKey.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
		Assert.AreEqual("__arg1", fromSelector.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
	}

	[TestMethod]
	public void Pinia_SubscriptionMutationEvents_UsesNet11UnionContract()
	{
		AssertNet11UnionContract(
			typeof(Pinia.SubscriptionMutationEvents),
			typeof(Vue3.VueDebuggerEvent),
			typeof(Vue3.VueDebuggerEvent[]));
	}

	[TestMethod]
	public void Pinia_RootLifecycleAndHydrationHelpers_ExposeOfficialContracts()
	{
		var getActivePinia = typeof(Pinia)
			.GetMethod(nameof(Pinia.GetActivePinia), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
		var setActivePinia = typeof(Pinia)
			.GetMethod(nameof(Pinia.SetActivePinia), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
		var clearActivePinia = typeof(Pinia)
			.GetMethod(nameof(Pinia.ClearActivePinia), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
		var disposePinia = typeof(Pinia)
			.GetMethod(nameof(Pinia.DisposePinia), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
		var skipHydrate = typeof(Pinia)
			.GetMethod(nameof(Pinia.SkipHydrate), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
		var shouldHydrate = typeof(Pinia)
			.GetMethod(nameof(Pinia.ShouldHydrate), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
		var hydrate = typeof(Pinia.DefineStoreOptions<TestPiniaState>)
			.GetProperty(nameof(Pinia.DefineStoreOptions<TestPiniaState>.Hydrate), BindingFlags.Public | BindingFlags.Instance);
		var pluginHydrate = typeof(Pinia.DefineStoreOptionsInPlugin<TestPiniaState>)
			.GetProperty(nameof(Pinia.DefineStoreOptionsInPlugin<TestPiniaState>.Hydrate), BindingFlags.Public | BindingFlags.Instance);

		Assert.IsNotNull(getActivePinia);
		Assert.IsNotNull(setActivePinia);
		Assert.IsNotNull(clearActivePinia);
		Assert.IsNotNull(disposePinia);
		Assert.IsNotNull(skipHydrate);
		Assert.IsNotNull(shouldHydrate);
		Assert.IsNotNull(hydrate);
		Assert.IsNotNull(pluginHydrate);

		Assert.AreEqual(typeof(Pinia.PiniaInstance), setActivePinia!.ReturnType);
		Assert.AreEqual(typeof(Pinia.PiniaInstance), setActivePinia.GetParameters()[0].ParameterType);
		Assert.AreEqual(typeof(Pinia.PiniaInstance), Nullable.GetUnderlyingType(clearActivePinia!.ReturnType) ?? clearActivePinia.ReturnType);
		Assert.AreEqual(0, clearActivePinia.GetParameters().Length);
		Assert.AreEqual("setActivePinia(undefined)", clearActivePinia.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
		Assert.AreEqual(typeof(Pinia.PiniaInstance), Nullable.GetUnderlyingType(getActivePinia!.ReturnType) ?? getActivePinia.ReturnType);
		Assert.AreEqual(typeof(Pinia.PiniaInstance), disposePinia!.GetParameters()[0].ParameterType);
		Assert.IsTrue(skipHydrate!.IsGenericMethodDefinition);
		Assert.AreEqual(skipHydrate.GetGenericArguments().Single(), skipHydrate.GetParameters()[0].ParameterType);
		Assert.AreEqual(skipHydrate.GetGenericArguments().Single(), skipHydrate.ReturnType);
		Assert.AreEqual(typeof(bool), shouldHydrate!.ReturnType);
		Assert.IsTrue(shouldHydrate.IsGenericMethodDefinition);
		Assert.AreEqual(shouldHydrate.GetGenericArguments().Single(), shouldHydrate.GetParameters()[0].ParameterType);
		Assert.AreEqual(typeof(PiniaHydrateCallback<TestPiniaState>), hydrate!.PropertyType);
		Assert.AreEqual(typeof(PiniaHydrateCallback<TestPiniaState>), pluginHydrate!.PropertyType);
	}

	[TestMethod]
	public void Pinia_RuntimeSupportTypes_UseEcmaScriptMarkers()
	{
		AssertEcmaScriptSupport(typeof(Pinia.StoreDefinition));
		AssertEcmaScriptSupport(typeof(Pinia.StoreDefinition<>));
		AssertEcmaScriptSupport(typeof(Pinia.ProjectedActionContext<,,>));
		AssertEcmaScriptSupport(typeof(Pinia.ActionArgsView));
		AssertEcmaScriptSupport(typeof(Pinia.ActionArgsView<,>));
		AssertEcmaScriptSupport(typeof(Pinia.ActionArgsView<,,,,,,,,,,,,,,,>));
		AssertEcmaScriptSupport(typeof(Pinia.ProjectedStore<,>));
		AssertEcmaScriptSupport(typeof(Pinia.ProjectedStore<,,>));
		AssertEcmaScriptSupport(typeof(Pinia.ProjectedStoreDefinition<,>));
		AssertEcmaScriptSupport(typeof(Pinia.ProjectedStoreDefinition<,,>));
		AssertEcmaScriptSupport(typeof(Pinia.StoreRefs<>));
		AssertEcmaScriptSupport(typeof(Pinia.PiniaStatePatch<>));
		AssertEcmaScriptSupport(typeof(Pinia.PiniaStateMapValue<>));
		AssertEcmaScriptSupport(typeof(Pinia.SubscriptionMutationEvents));
		AssertEcmaScriptSupport(typeof(Pinia.SetupStoreHelpers));
		AssertEcmaScriptSupport(typeof(Pinia.PiniaValue));
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

	private static void AssertNet11UnionContract(Type unionType, params Type[] constructorBranchTypes)
	{
		Assert.IsNull(unionType.GetCustomAttribute<ECMAScriptUnionAttribute>(), unionType.FullName);
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
				$"{unionType.FullName} cannot use native union because branch {right.FullName} is assignable to {left.FullName}; keep a tagged ECMAScriptUnion wrapper to preserve exact AsX projections.");
		}
	}

	private static bool IsUnionValueProperty(PropertyInfo property)
		=> property.Name == nameof(System.Runtime.CompilerServices.IUnion.Value) &&
		   typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(property.DeclaringType);
}

public sealed record TestPiniaState : Pinia.PiniaStateTree
{
	public int Count { get; init; }
}

public abstract class TestPiniaStore : Pinia.Store<TestPiniaState>
{
	public extern int DoubleCount { get; }

	public extern void Increment();
}

public sealed record TestPiniaGetters : Vue3.VueProps
{
	public Func<int> DoubleCount { get; init; } = default!;
}

public sealed record TestPiniaActions : Vue3.VueProps
{
	public Action Increment { get; init; } = default!;
}

public sealed record TestPiniaPluginExtensions : Vue3.VueProps
{
	public string AuditTag { get; init; } = "";
}

public sealed record TestPiniaPluginStateExtensions : Pinia.PiniaStateTree
{
	public string PersistedAt { get; init; } = "";
}

public sealed record TestPiniaPluginOptions : Pinia.DefineStoreOptionsInPlugin<TestPiniaState, TestPiniaGetters, TestPiniaActions>
{
	public bool? Persist { get; init; }
}

#pragma warning restore CA1416
#pragma warning restore CS0626
