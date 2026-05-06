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
			typeof(PiniaTesting.TestingOptions),
			typeof(PiniaTesting.TestingInitialState),
			typeof(PiniaTestingSpyFactory),
			typeof(PiniaTestingSpyFactory<>).MakeGenericType(typeof(Action))
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
		Assert.AreEqual(typeof(Pinia.PiniaInstance), methods[0].ReturnType);
		Assert.AreEqual(0, methods[0].GetParameters().Length);
		Assert.AreEqual(typeof(Pinia.PiniaInstance), methods[1].ReturnType);
		Assert.AreEqual(typeof(PiniaTesting.TestingOptions), methods[1].GetParameters()[0].ParameterType);
	}

	[TestMethod]
	public void PiniaTesting_TestingOptions_ExposeProductionGradeConfigurationSurface()
	{
		var optionsType = typeof(PiniaTesting.TestingOptions);
		var initialState = optionsType.GetProperty(nameof(PiniaTesting.TestingOptions.InitialState), BindingFlags.Public | BindingFlags.Instance);
		var plugins = optionsType.GetProperty(nameof(PiniaTesting.TestingOptions.Plugins), BindingFlags.Public | BindingFlags.Instance);
		var stubActions = optionsType.GetProperty(nameof(PiniaTesting.TestingOptions.StubActions), BindingFlags.Public | BindingFlags.Instance);
		var stubPatch = optionsType.GetProperty(nameof(PiniaTesting.TestingOptions.StubPatch), BindingFlags.Public | BindingFlags.Instance);
		var stubReset = optionsType.GetProperty(nameof(PiniaTesting.TestingOptions.StubReset), BindingFlags.Public | BindingFlags.Instance);
		var fakeApp = optionsType.GetProperty(nameof(PiniaTesting.TestingOptions.FakeApp), BindingFlags.Public | BindingFlags.Instance);
		var createSpy = optionsType.GetProperty(nameof(PiniaTesting.TestingOptions.CreateSpy), BindingFlags.Public | BindingFlags.Instance);

		Assert.IsNotNull(initialState);
		Assert.IsNotNull(plugins);
		Assert.IsNotNull(stubActions);
		Assert.IsNotNull(stubPatch);
		Assert.IsNotNull(stubReset);
		Assert.IsNotNull(fakeApp);
		Assert.IsNotNull(createSpy);
		Assert.AreEqual(typeof(PiniaTesting.TestingInitialState), initialState!.PropertyType);
		Assert.AreEqual(typeof(PiniaPlugin[]), plugins!.PropertyType);
		Assert.AreEqual(typeof(bool?), stubActions!.PropertyType);
		Assert.AreEqual(typeof(bool?), stubPatch!.PropertyType);
		Assert.AreEqual(typeof(bool?), stubReset!.PropertyType);
		Assert.AreEqual(typeof(bool?), fakeApp!.PropertyType);
		Assert.AreEqual(typeof(PiniaTestingSpyFactory), createSpy!.PropertyType);
		Assert.IsTrue(typeof(Vue3.VueProps).IsAssignableFrom(typeof(PiniaTesting.TestingOptions)));
		Assert.IsTrue(typeof(Vue3.VueProps).IsAssignableFrom(typeof(PiniaTesting.TestingInitialState)));
	}

	[TestMethod]
	public void PiniaTesting_RuntimeSupportTypes_UseEcmaScriptMarkers()
	{
		AssertEcmaScriptSupport(typeof(PiniaTesting.TestingInitialState));
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
