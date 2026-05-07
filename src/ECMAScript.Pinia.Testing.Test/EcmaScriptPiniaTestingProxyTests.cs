using System.Reflection;
using ECMAScript.Contract;

namespace ECMAScript.PiniaTestingTests;

#pragma warning disable CA1416
#pragma warning disable CS0626

[TestClass]
public sealed class EcmaScriptPiniaTestingProxyTests
{
	[TestMethod]
	public void PiniaTesting_ImportHost_UsesBareTestingPackageImport()
	{
		AssertEcmaScriptImport(typeof(PiniaTesting), "@pinia/testing");
	}

	[TestMethod]
	public void PiniaTesting_CoreRuntimeShapes_DoNotExposeObject()
	{
		var runtimeTypes = new[]
		{
			typeof(PiniaTesting),
			typeof(PiniaTesting.TestingPinia),
			typeof(PiniaTesting.TestingOptions),
			typeof(PiniaTesting.TestingInitialState),
			typeof(PiniaTesting.TestingStubActions),
			typeof(PiniaTestingSpyFactory),
			typeof(PiniaTestingSpyFactory<>).MakeGenericType(typeof(Action)),
			typeof(PiniaTestingStubActionPredicate),
			typeof(PiniaTestingStubActionPredicate<>).MakeGenericType(typeof(Pinia.StoreGeneric))
		};

		foreach (var type in runtimeTypes)
		{
			foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
				AssertNotObject(property.PropertyType, $"{type.Name}.{property.Name}");

			foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
						 .Where(static method => !method.IsSpecialName)
						 .Where(static method =>
							 method.Name is not ("Equals" or "GetHashCode" or "ToString" or "PrintMembers" or "<Clone>$" or "Invoke" or "BeginInvoke" or "EndInvoke")))
			{
				AssertNotObject(method.ReturnType, $"{type.Name}.{method.Name} return");
				foreach (var parameter in method.GetParameters())
					AssertNotObject(parameter.ParameterType, $"{type.Name}.{method.Name}({parameter.Name})");
			}
		}
	}

	[TestMethod]
	public void PiniaTesting_StaticApi_ExposesCreateTestingPiniaOverloads()
	{
		var methods = typeof(PiniaTesting)
			.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
			.Where(static method => method.Name == nameof(PiniaTesting.CreateTestingPinia))
			.OrderBy(static method => method.GetParameters().Length)
			.ToArray();

		Assert.AreEqual(2, methods.Length);
		Assert.AreEqual(typeof(PiniaTesting.TestingPinia), methods[0].ReturnType);
		Assert.AreEqual(0, methods[0].GetParameters().Length);
		Assert.AreEqual(typeof(PiniaTesting.TestingPinia), methods[1].ReturnType);
		Assert.AreEqual(typeof(PiniaTesting.TestingOptions), methods[1].GetParameters()[0].ParameterType);
	}

	[TestMethod]
	public void PiniaTesting_StaticApi_ExposesTypedPluginProjectionOverloads()
	{
		var methods = typeof(PiniaTesting)
			.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
			.Where(static method => method.Name == nameof(PiniaTesting.ProjectPlugin))
			.OrderBy(static method => method.GetGenericArguments().Length)
			.ToArray();

		Assert.AreEqual(5, methods.Length);
		Assert.IsTrue(methods.All(static method => method.ReturnType == typeof(PiniaPlugin)));
		Assert.IsTrue(methods.All(static method => method.GetParameters().Length == 1));
		Assert.IsTrue(methods.Any(static method => method.GetGenericArguments().Length == 1));
		Assert.IsTrue(methods.Any(static method => method.GetGenericArguments().Length == 2));
		Assert.IsTrue(methods.Any(static method => method.GetGenericArguments().Length == 3));
		Assert.IsTrue(methods.Any(static method => method.GetGenericArguments().Length == 4));
		Assert.IsTrue(methods.Any(static method => method.GetGenericArguments().Length == 5));
	}

	[TestMethod]
	public void PiniaTesting_StaticApi_ExposesTypedStubActionPredicateProjection()
	{
		var methods = typeof(PiniaTesting)
			.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
			.Where(static method => method.Name == nameof(PiniaTesting.ProjectStubActionPredicate))
			.ToArray();

		Assert.AreEqual(1, methods.Length);
		Assert.AreEqual(typeof(PiniaTestingStubActionPredicate), methods[0].ReturnType);
		Assert.AreEqual(1, methods[0].GetGenericArguments().Length);
		Assert.AreEqual(1, methods[0].GetParameters().Length);
		Assert.IsTrue(methods[0].GetParameters()[0].ParameterType.IsGenericType);
		Assert.AreEqual(typeof(PiniaTestingStubActionPredicate<>), methods[0].GetParameters()[0].ParameterType.GetGenericTypeDefinition());
	}

	[TestMethod]
	public void PiniaTesting_TestingOptions_ExposeProductionGradeConfigurationSurface()
	{
		var optionsType = typeof(PiniaTesting.TestingOptions);
		var initialState = optionsType.GetProperty(nameof(PiniaTesting.TestingOptions.InitialState), BindingFlags.Public | BindingFlags.Instance);
		var plugins = optionsType.GetProperty(nameof(PiniaTesting.TestingOptions.Plugins), BindingFlags.Public | BindingFlags.Instance);
		var stubActions = optionsType.GetProperty(nameof(PiniaTesting.TestingOptions.StubActions), BindingFlags.Public | BindingFlags.Instance);
		var writableComputed = optionsType.GetProperty(nameof(PiniaTesting.TestingOptions.WritableComputed), BindingFlags.Public | BindingFlags.Instance);
		var stubPatch = optionsType.GetProperty(nameof(PiniaTesting.TestingOptions.StubPatch), BindingFlags.Public | BindingFlags.Instance);
		var stubReset = optionsType.GetProperty(nameof(PiniaTesting.TestingOptions.StubReset), BindingFlags.Public | BindingFlags.Instance);
		var fakeApp = optionsType.GetProperty(nameof(PiniaTesting.TestingOptions.FakeApp), BindingFlags.Public | BindingFlags.Instance);
		var createSpy = optionsType.GetProperty(nameof(PiniaTesting.TestingOptions.CreateSpy), BindingFlags.Public | BindingFlags.Instance);

		Assert.IsNotNull(initialState);
		Assert.IsNotNull(plugins);
		Assert.IsNotNull(stubActions);
		Assert.IsNotNull(writableComputed);
		Assert.IsNotNull(stubPatch);
		Assert.IsNotNull(stubReset);
		Assert.IsNotNull(fakeApp);
		Assert.IsNotNull(createSpy);
		Assert.AreEqual(typeof(PiniaTesting.TestingInitialState), initialState!.PropertyType);
		Assert.AreEqual(typeof(PiniaPlugin[]), plugins!.PropertyType);
		Assert.AreEqual(typeof(PiniaTesting.TestingStubActions?), stubActions!.PropertyType);
		Assert.AreEqual(typeof(bool?), writableComputed!.PropertyType);
		Assert.AreEqual(typeof(bool?), stubPatch!.PropertyType);
		Assert.AreEqual(typeof(bool?), stubReset!.PropertyType);
		Assert.AreEqual(typeof(bool?), fakeApp!.PropertyType);
		Assert.AreEqual(typeof(PiniaTestingSpyFactory), createSpy!.PropertyType);
		Assert.IsTrue(typeof(Vue3.VueProps).IsAssignableFrom(typeof(PiniaTesting.TestingOptions)));
		Assert.IsTrue(typeof(Vue3.VueProps).IsAssignableFrom(typeof(PiniaTesting.TestingInitialState)));
		Assert.IsTrue(typeof(Pinia.PiniaInstance).IsAssignableFrom(typeof(PiniaTesting.TestingPinia)));
	}

	[TestMethod]
	public void PiniaTesting_TypedTestingOptions_ExposeTypedCreateSpyProjection()
	{
		var optionsType = typeof(PiniaTesting.TestingOptions<Action<int>>);
		var createSpy = optionsType.GetProperty(nameof(PiniaTesting.TestingOptions.CreateSpy), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

		Assert.IsNotNull(createSpy);
		Assert.AreEqual(typeof(PiniaTestingSpyFactory<Action<int>>), createSpy!.PropertyType);
		Assert.IsTrue(typeof(PiniaTesting.TestingOptions).IsAssignableFrom(optionsType));
		Assert.IsTrue(typeof(Vue3.VueProps).IsAssignableFrom(optionsType));
	}

	[TestMethod]
	public void PiniaTesting_TestingPinia_ExposesFakeAppBoundary()
	{
		var app = typeof(PiniaTesting.TestingPinia)
			.GetProperty(nameof(PiniaTesting.TestingPinia.App), BindingFlags.Public | BindingFlags.Instance);

		Assert.IsNotNull(app);
		Assert.AreEqual(typeof(Vue3.VueApp), app!.PropertyType);
	}

	[TestMethod]
	public void PiniaTesting_StubActions_ModelsBooleanNamedListOrPredicateConfiguration()
	{
		var stubActionsType = typeof(PiniaTesting.TestingStubActions);
		var asBoolean = stubActionsType.GetProperty(nameof(PiniaTesting.TestingStubActions.AsBoolean), BindingFlags.Public | BindingFlags.Instance);
		var asNames = stubActionsType.GetProperty(nameof(PiniaTesting.TestingStubActions.AsNames), BindingFlags.Public | BindingFlags.Instance);
		var asPredicate = stubActionsType.GetProperty(nameof(PiniaTesting.TestingStubActions.AsPredicate), BindingFlags.Public | BindingFlags.Instance);
		var implicitOperators = stubActionsType
			.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Where(static method => method.Name == "op_Implicit")
			.ToArray();

		Assert.IsNotNull(asBoolean);
		Assert.IsNotNull(asNames);
		Assert.IsNotNull(asPredicate);
		Assert.AreEqual(typeof(bool?), asBoolean!.PropertyType);
		Assert.AreEqual(typeof(string[]), asNames!.PropertyType);
		Assert.AreEqual(typeof(PiniaTestingStubActionPredicate), asPredicate!.PropertyType);
		Assert.IsTrue(implicitOperators.Any(static method => method.GetParameters()[0].ParameterType == typeof(bool)));
		Assert.IsTrue(implicitOperators.Any(static method => method.GetParameters()[0].ParameterType == typeof(string[])));
		Assert.IsTrue(implicitOperators.Any(static method => method.GetParameters()[0].ParameterType == typeof(PiniaTestingStubActionPredicate)));
	}

	[TestMethod]
	public void PiniaTesting_RuntimeSupportTypes_UseEcmaScriptMarkers()
	{
		AssertEcmaScriptSupport(typeof(PiniaTesting.TestingPinia));
		AssertEcmaScriptSupport(typeof(PiniaTesting.TestingInitialState));
		AssertEcmaScriptSupport(typeof(PiniaTesting.TestingStubActions));
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
}

#pragma warning restore CA1416
#pragma warning restore CS0626
