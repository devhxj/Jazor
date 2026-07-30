using System.Reflection;
using ECMAScript.Contract;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class NumericWidthModuleWhitelistTests
{
	[TestMethod]
	public void HalfMappings_UseNumberCarrierAndHalfRuntimeModule()
	{
		AssertTypeAlias(typeof(Jazor.CLR.HalfModule), "System.Half", "Number");
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.NaN.get", Op.Inline);
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.operator +(System.Half, System.Half)", Op.Allowed);
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.Parse(string)", Op.Import, "System/HalfModule.js");
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.MaxMagnitude(System.Half, System.Half)", Op.Import, "System/HalfModule.js");
	}

	[TestMethod]
	public void Int128Mappings_UseBigIntCarrierAndSignedRuntimeModule()
	{
		AssertTypeAlias(typeof(Jazor.CLR.Int128Module), "System.Int128", "BigInt");
		AssertMember(typeof(Jazor.CLR.Int128Module), "static System.Int128.MaxValue.get", Op.Inline);
		AssertMember(typeof(Jazor.CLR.Int128Module), "static System.Int128.operator +(System.Int128, System.Int128)", Op.Inline);
		AssertMember(typeof(Jazor.CLR.Int128Module), "static System.Int128.operator /(System.Int128, System.Int128)", Op.Import, "System/Int128Module.js");
		AssertMember(typeof(Jazor.CLR.Int128Module), "static System.Int128.Parse(string)", Op.Import, "System/Int128Module.js");
		AssertMember(typeof(Jazor.CLR.Int128Module), "static System.Int128.RotateLeft(System.Int128, int)", Op.Import, "System/Int128Module.js");
	}

	[TestMethod]
	public void UInt128Mappings_UseBigIntCarrierAndUnsignedRuntimeModule()
	{
		AssertTypeAlias(typeof(Jazor.CLR.UInt128Module), "System.UInt128", "BigInt");
		AssertMember(typeof(Jazor.CLR.UInt128Module), "static System.UInt128.MaxValue.get", Op.Inline);
		AssertMember(typeof(Jazor.CLR.UInt128Module), "static System.UInt128.operator -(System.UInt128, System.UInt128)", Op.Inline);
		AssertMember(typeof(Jazor.CLR.UInt128Module), "static System.UInt128.operator %(System.UInt128, System.UInt128)", Op.Import, "System/UInt128Module.js");
		AssertMember(typeof(Jazor.CLR.UInt128Module), "static System.UInt128.Parse(string)", Op.Import, "System/UInt128Module.js");
		AssertMember(typeof(Jazor.CLR.UInt128Module), "static System.UInt128.RotateRight(System.UInt128, int)", Op.Import, "System/UInt128Module.js");
	}

	private static void AssertTypeAlias(Type type, string member, string alias)
	{
		var attribute = type.GetCustomAttribute<JazorAttribute>();
		Assert.IsNotNull(attribute);
		Assert.AreEqual(Op.Alias, attribute.Op);
		Assert.AreEqual(member, attribute.Member);
		Assert.AreEqual(alias, attribute.Value);
	}

	private static void AssertMember(Type type, string member, Op op, string? modulePath = null)
	{
		var attribute = type
			.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Select(static method => method.GetCustomAttribute<JazorAttribute>())
			.OfType<JazorAttribute>()
			.Single(candidate => candidate.Member == member);

		Assert.AreEqual(op, attribute.Op, member);
		if (modulePath is not null)
			Assert.AreEqual(modulePath, ClrRuntimeMappingCatalog.GetImport(member).ModulePath, member);
	}
}
