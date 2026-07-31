using System.Reflection;
using ECMAScript.Contract;
using Jazor.RazorVue.RazorSdk.Catalog;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class EventCallbackCatalogWhitelistTests
{
	[TestMethod]
	public void EventCallbackFactoryField_IsAllowedMapped()
	{
		var attribute = typeof(EventCallbackCatalog)
			.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Select(method => method.GetCustomAttribute<JazorAttribute>())
			.OfType<JazorAttribute>()
			.Single(attribute => attribute.Member == "static readonly Microsoft.AspNetCore.Components.EventCallback.Factory");

		Assert.AreEqual(Op.Allowed, attribute.Op);
	}

	[TestMethod]
	public void EventCallbackFactoryCreate_MapsSyncAndAsyncHandlerFamilies()
	{
		var members = typeof(EventCallbackFactoryCatalog)
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
		var nonGeneric = typeof(EventCallbackCatalog)
			.GetCustomAttribute<JazorAttribute>();
		var generic = typeof(EventCallbackTCatalog<>)
			.GetCustomAttribute<JazorAttribute>();
		var members = typeof(EventCallbackCatalog)
			.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Concat(typeof(EventCallbackTCatalog<>).GetMethods(BindingFlags.Public | BindingFlags.Static))
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
