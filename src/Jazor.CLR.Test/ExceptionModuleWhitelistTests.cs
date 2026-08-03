using System.Reflection;
using ECMAScript.Contract;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class ExceptionModuleWhitelistTests
{
	[TestMethod]
	public void ArgumentNullExceptionThrowIfNull_IsImportMapped()
	{
		var attribute = typeof(Jazor.CLR.ExceptionModule)
			.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Select(method => method.GetCustomAttribute<JazorAttribute>())
			.OfType<JazorAttribute>()
			.Single(attribute => attribute.Member == "static System.ArgumentNullException.ThrowIfNull(object, string)");

		Assert.AreEqual(Op.Import, attribute.Op);
	}

	[TestMethod]
	public void HelpLinkAccessors_AreImportMapped()
	{
		var mappings = typeof(Jazor.CLR.ExceptionModule)
			.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Select(method => method.GetCustomAttribute<JazorAttribute>())
			.OfType<JazorAttribute>()
			.ToDictionary(attribute => attribute.Member, StringComparer.Ordinal);

		Assert.AreEqual(Op.Import, mappings["virtual System.Exception.HelpLink.get"].Op);
		Assert.AreEqual(Op.Import, mappings["virtual System.Exception.HelpLink.set"].Op);
	}
}
