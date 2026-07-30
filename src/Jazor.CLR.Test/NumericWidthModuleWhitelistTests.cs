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
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.operator +(System.Half, System.Half)", Op.Inline);
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.Parse(string)", Op.Import, "System/HalfModule.js");
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.Round(System.Half)", Op.Import, "System/HalfModule.js");
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.Ieee754Remainder(System.Half, System.Half)", Op.Import, "System/HalfModule.js");
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.ILogB(System.Half)", Op.Import, "System/HalfModule.js");
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.Clamp(System.Half, System.Half, System.Half)", Op.Import, "System/HalfModule.js");
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.RootN(System.Half, int)", Op.Import, "System/HalfModule.js");
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
		AssertMember(typeof(Jazor.CLR.Int128Module), "static System.Int128.Abs(System.Int128)", Op.Import, "System/Int128Module.js");
		AssertMember(typeof(Jazor.CLR.Int128Module), "static System.Int128.CopySign(System.Int128, System.Int128)", Op.Import, "System/Int128Module.js");
		AssertMember(typeof(Jazor.CLR.Int128Module), "static System.Int128.Clamp(System.Int128, System.Int128, System.Int128)", Op.Import, "System/Int128Module.js");
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
		AssertMember(typeof(Jazor.CLR.UInt128Module), "static System.UInt128.Clamp(System.UInt128, System.UInt128, System.UInt128)", Op.Import, "System/UInt128Module.js");
	}

	[TestMethod]
	public void GeneratedNumericScaffolds_PreserveStrongConversionAndGenericOperands()
	{
		foreach (var moduleType in new[]
		{
			typeof(Jazor.CLR.HalfModule),
			typeof(Jazor.CLR.Int128Module),
			typeof(Jazor.CLR.UInt128Module)
		})
		{
			var methods = moduleType.GetMethods(BindingFlags.Public | BindingFlags.Static);
			var conversions = methods.Where(method =>
			{
				var member = method.GetCustomAttribute<JazorAttribute>()?.Member;
				return member?.Contains(".implicit operator ", StringComparison.Ordinal) == true ||
					member?.Contains(".explicit operator ", StringComparison.Ordinal) == true;
			});

			Assert.IsNotEmpty(conversions, moduleType.Name);
			foreach (var conversion in conversions)
				Assert.HasCount(1, conversion.GetParameters(), conversion.Name);

			foreach (var methodName in new[] { "CreateChecked", "CreateSaturating", "CreateTruncating" })
			{
				var method = methods.Single(candidate =>
					candidate.GetCustomAttribute<JazorAttribute>()?.Member.Contains($".{methodName}<", StringComparison.Ordinal) == true);
				Assert.IsTrue(method.GetParameters()[0].ParameterType.IsGenericParameter, methodName);
				var genericParameter = method.GetGenericArguments().Single();
				Assert.IsTrue(
					genericParameter.GetGenericParameterConstraints().Any(constraint =>
						constraint.IsGenericType &&
						constraint.GetGenericTypeDefinition() == typeof(System.Numerics.INumberBase<>)),
					methodName);
			}
		}
	}

	[TestMethod]
	public void GeneratedNumericScaffolds_PreserveNumberStylesType()
	{
		foreach (var moduleType in new[]
		{
			typeof(Jazor.CLR.HalfModule),
			typeof(Jazor.CLR.Int128Module),
			typeof(Jazor.CLR.UInt128Module)
		})
		{
			var parse = moduleType
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Single(method => method.GetCustomAttribute<JazorAttribute>()?.Member ==
					$"static {GetClrTypeName(moduleType)}.Parse(string, System.Globalization.NumberStyles)");

			Assert.AreEqual(typeof(System.Globalization.NumberStyles), parse.GetParameters()[1].ParameterType);
		}
	}

	[TestMethod]
	public void GeneratedNumericScaffolds_PreserveParseNullabilityContract()
	{
		var nullability = new NullabilityInfoContext();

		foreach (var moduleType in new[]
		{
			typeof(Jazor.CLR.HalfModule),
			typeof(Jazor.CLR.Int128Module),
			typeof(Jazor.CLR.UInt128Module)
		})
		{
			var clrTypeName = GetClrTypeName(moduleType);
			var methods = moduleType.GetMethods(BindingFlags.Public | BindingFlags.Static);
			foreach (var member in new[]
			{
				$"static {clrTypeName}.Parse(string)",
				$"static {clrTypeName}.Parse(string, System.IFormatProvider)"
			})
			{
				var method = methods.Single(candidate =>
					candidate.GetCustomAttribute<JazorAttribute>()?.Member == member);
				var text = method.GetParameters()[0];
				Assert.AreEqual(NullabilityState.NotNull, nullability.Create(text).ReadState, member);
			}
		}
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

	private static string GetClrTypeName(Type moduleType)
		=> moduleType == typeof(Jazor.CLR.HalfModule)
			? "System.Half"
			: moduleType == typeof(Jazor.CLR.Int128Module)
				? "System.Int128"
				: "System.UInt128";
}
