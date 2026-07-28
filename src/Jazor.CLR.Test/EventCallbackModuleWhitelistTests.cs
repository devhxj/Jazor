using System.Reflection;
using ECMAScript.Contract;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EventCallbackModuleWhitelistTests
{
	[TestMethod]
	public void EventCallbackFactoryField_IsAllowedMapped()
	{
		var attribute = typeof(Jazor.CLR.EventCallbackModule)
			.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Select(method => method.GetCustomAttribute<JazorAttribute>())
			.OfType<JazorAttribute>()
			.Single(attribute => attribute.Member == "static readonly Microsoft.AspNetCore.Components.EventCallback.Factory");

		Assert.AreEqual(Op.Allowed, attribute.Op);
	}

	[TestMethod]
	public void EventCallbackFactoryCreate_MapsSyncAndAsyncHandlerFamilies()
	{
		var members = typeof(Jazor.CLR.EventCallbackFactoryModule)
			.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Select(method => method.GetCustomAttribute<JazorAttribute>())
			.OfType<JazorAttribute>()
			.ToDictionary(attribute => attribute.Member!, attribute => attribute.Op);

		Assert.AreEqual(Op.Allowed, members["Microsoft.AspNetCore.Components.EventCallbackFactory.Create(object, System.Action)"]);
		Assert.AreEqual(Op.Allowed, members["Microsoft.AspNetCore.Components.EventCallbackFactory.Create<TValue>(object, System.Action<TValue>)"]);
		Assert.AreEqual(Op.Allowed, members["Microsoft.AspNetCore.Components.EventCallbackFactory.Create(object, System.Func<System.Threading.Tasks.Task>)"]);
		Assert.AreEqual(Op.Allowed, members["Microsoft.AspNetCore.Components.EventCallbackFactory.Create<TValue>(object, System.Func<TValue, System.Threading.Tasks.Task>)"]);
	}

	[TestMethod]
	public void EventCallbackInvokeAsync_MapsNonGenericAndGenericCallbackContracts()
	{
		var nonGeneric = typeof(Jazor.CLR.EventCallbackModule)
			.GetCustomAttribute<JazorAttribute>();
		var generic = typeof(Jazor.CLR.EventCallbackTModule<>)
			.GetCustomAttribute<JazorAttribute>();
		var members = typeof(Jazor.CLR.EventCallbackModule)
			.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Concat(typeof(Jazor.CLR.EventCallbackTModule<>).GetMethods(BindingFlags.Public | BindingFlags.Static))
			.Select(method => method.GetCustomAttribute<JazorAttribute>())
			.OfType<JazorAttribute>()
			.ToDictionary(attribute => attribute.Member!, attribute => attribute.Op);

		Assert.IsNotNull(nonGeneric);
		Assert.AreEqual("Microsoft.AspNetCore.Components.EventCallback", nonGeneric.Member);
		Assert.IsNotNull(generic);
		Assert.AreEqual("Microsoft.AspNetCore.Components.EventCallback<TValue>", generic.Member);
		Assert.AreEqual(Op.Allowed, members["Microsoft.AspNetCore.Components.EventCallback.InvokeAsync()"]);
		Assert.AreEqual(Op.Allowed, members["Microsoft.AspNetCore.Components.EventCallback<TValue>.InvokeAsync(TValue)"]);
	}
}
