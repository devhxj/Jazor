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
}
