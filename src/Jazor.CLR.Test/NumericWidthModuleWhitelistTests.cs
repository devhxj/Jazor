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
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.Round(System.Half, int)", Op.Import, "System/HalfModule.js");
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.Round(System.Half, System.MidpointRounding)", Op.Import, "System/HalfModule.js");
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.Round(System.Half, int, System.MidpointRounding)", Op.Import, "System/HalfModule.js");
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.Ieee754Remainder(System.Half, System.Half)", Op.Import, "System/HalfModule.js");
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.ILogB(System.Half)", Op.Import, "System/HalfModule.js");
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.BitIncrement(System.Half)", Op.Import, "System/HalfModule.js");
		AssertMember(typeof(Jazor.CLR.HalfModule), "static System.Half.BitDecrement(System.Half)", Op.Import, "System/HalfModule.js");
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
		AssertMember(typeof(Jazor.CLR.Int128Module), "static System.Int128.explicit operator float(System.Int128)", Op.Inline);
		AssertMember(typeof(Jazor.CLR.Int128Module), "static System.Int128.operator /(System.Int128, System.Int128)", Op.Import, "System/Int128Module.js");
		AssertMember(typeof(Jazor.CLR.Int128Module), "static System.Int128.Parse(string)", Op.Import, "System/Int128Module.js");
		AssertMember(typeof(Jazor.CLR.Int128Module), "static System.Int128.RotateLeft(System.Int128, int)", Op.Import, "System/Int128Module.js");
		AssertMember(typeof(Jazor.CLR.Int128Module), "static System.Int128.Abs(System.Int128)", Op.Import, "System/Int128Module.js");
		AssertMember(typeof(Jazor.CLR.Int128Module), "static System.Int128.CopySign(System.Int128, System.Int128)", Op.Import, "System/Int128Module.js");
		AssertMember(typeof(Jazor.CLR.Int128Module), "static System.Int128.Clamp(System.Int128, System.Int128, System.Int128)", Op.Import, "System/Int128Module.js");
		AssertMember(typeof(Jazor.CLR.Int128Module), "static System.Int128.Log10(System.Int128)", Op.Import, "System/Int128Module.js");
		AssertMember(typeof(Jazor.CLR.Int128Module), "static System.Int128.BigMul(System.Int128, System.Int128, out System.Int128)", Op.Import, "System/Int128Module.js");
		AssertRecoveredInt128CheckedAndSpanMembers(typeof(Jazor.CLR.Int128Module), "System.Int128", "System/Int128Module.js");
	}

	[TestMethod]
	public void UInt128Mappings_UseBigIntCarrierAndUnsignedRuntimeModule()
	{
		AssertTypeAlias(typeof(Jazor.CLR.UInt128Module), "System.UInt128", "BigInt");
		AssertMember(typeof(Jazor.CLR.UInt128Module), "static System.UInt128.MaxValue.get", Op.Inline);
		AssertMember(typeof(Jazor.CLR.UInt128Module), "static System.UInt128.operator -(System.UInt128, System.UInt128)", Op.Inline);
		AssertMember(typeof(Jazor.CLR.UInt128Module), "static System.UInt128.explicit operator float(System.UInt128)", Op.Inline);
		AssertMember(typeof(Jazor.CLR.UInt128Module), "static System.UInt128.operator %(System.UInt128, System.UInt128)", Op.Import, "System/UInt128Module.js");
		AssertMember(typeof(Jazor.CLR.UInt128Module), "static System.UInt128.Parse(string)", Op.Import, "System/UInt128Module.js");
		AssertMember(typeof(Jazor.CLR.UInt128Module), "static System.UInt128.RotateRight(System.UInt128, int)", Op.Import, "System/UInt128Module.js");
		AssertMember(typeof(Jazor.CLR.UInt128Module), "static System.UInt128.Clamp(System.UInt128, System.UInt128, System.UInt128)", Op.Import, "System/UInt128Module.js");
		AssertMember(typeof(Jazor.CLR.UInt128Module), "static System.UInt128.Log10(System.UInt128)", Op.Import, "System/UInt128Module.js");
		AssertMember(typeof(Jazor.CLR.UInt128Module), "static System.UInt128.BigMul(System.UInt128, System.UInt128, out System.UInt128)", Op.Import, "System/UInt128Module.js");
		AssertRecoveredInt128CheckedAndSpanMembers(typeof(Jazor.CLR.UInt128Module), "System.UInt128", "System/UInt128Module.js");
	}

	[TestMethod]
	public void FixedWidthIntegerReadOnlyCharSpanTryParse_UsesTypedRuntimeImports()
	{
		AssertMember(typeof(Jazor.CLR.ByteModule), "static byte.TryParse(System.ReadOnlySpan<char>, out byte)", Op.Import, "System/ByteModule.js");
		AssertMember(typeof(Jazor.CLR.SByteModule), "static sbyte.TryParse(System.ReadOnlySpan<char>, out sbyte)", Op.Import, "System/SByteModule.js");
		AssertMember(typeof(Jazor.CLR.Int16Module), "static short.TryParse(System.ReadOnlySpan<char>, out short)", Op.Import, "System/Int16Module.js");
		AssertMember(typeof(Jazor.CLR.UInt16Module), "static ushort.TryParse(System.ReadOnlySpan<char>, out ushort)", Op.Import, "System/UInt16Module.js");
		AssertMember(typeof(Jazor.CLR.Int32Module), "static int.TryParse(System.ReadOnlySpan<char>, out int)", Op.Import, "System/Int32Module.js");
		AssertMember(typeof(Jazor.CLR.UInt32Module), "static uint.TryParse(System.ReadOnlySpan<char>, out uint)", Op.Import, "System/UInt32Module.js");
		AssertMember(typeof(Jazor.CLR.Int64Module), "static long.TryParse(System.ReadOnlySpan<char>, out long)", Op.Import, "System/Int64Module.js");
		AssertMember(typeof(Jazor.CLR.UInt64Module), "static ulong.TryParse(System.ReadOnlySpan<char>, out ulong)", Op.Import, "System/UInt64Module.js");
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
	public void Int128RecoveredConversions_DeclareWidthSpecificInlineAndImportContracts()
	{
		AssertMembers(
			typeof(Jazor.CLR.Int128Module),
			Op.Inline,
			null,
			"static System.Int128.explicit operator byte(System.Int128)",
			"static System.Int128.explicit operator double(System.Int128)",
			"static System.Int128.explicit operator float(System.Int128)",
			"static System.Int128.explicit operator short(System.Int128)",
			"static System.Int128.explicit operator int(System.Int128)",
			"static System.Int128.explicit operator long(System.Int128)",
			"static System.Int128.explicit operator sbyte(System.Int128)",
			"static System.Int128.explicit operator ushort(System.Int128)",
			"static System.Int128.explicit operator uint(System.Int128)",
			"static System.Int128.explicit operator ulong(System.Int128)",
			"static System.Int128.explicit operator System.UInt128(System.Int128)",
			"static System.Int128.implicit operator System.Int128(byte)",
			"static System.Int128.implicit operator System.Int128(short)",
			"static System.Int128.implicit operator System.Int128(int)",
			"static System.Int128.implicit operator System.Int128(long)",
			"static System.Int128.implicit operator System.Int128(sbyte)",
			"static System.Int128.implicit operator System.Int128(ushort)",
			"static System.Int128.implicit operator System.Int128(uint)",
			"static System.Int128.implicit operator System.Int128(ulong)");

		AssertMembers(
			typeof(Jazor.CLR.Int128Module),
			Op.Import,
			"System/Int128Module.js",
			"static System.Int128.explicit operator checked byte(System.Int128)",
			"static System.Int128.explicit operator decimal(System.Int128)",
			"static System.Int128.explicit operator checked short(System.Int128)",
			"static System.Int128.explicit operator checked int(System.Int128)",
			"static System.Int128.explicit operator checked long(System.Int128)",
			"static System.Int128.explicit operator checked sbyte(System.Int128)",
			"static System.Int128.explicit operator checked ushort(System.Int128)",
			"static System.Int128.explicit operator checked uint(System.Int128)",
			"static System.Int128.explicit operator checked ulong(System.Int128)",
			"static System.Int128.explicit operator checked System.UInt128(System.Int128)",
			"static System.Int128.explicit operator System.Int128(decimal)");
	}

	[TestMethod]
	public void UInt128RecoveredConversions_DeclareWidthSpecificInlineAndImportContracts()
	{
		AssertMembers(
			typeof(Jazor.CLR.UInt128Module),
			Op.Inline,
			null,
			"static System.UInt128.explicit operator byte(System.UInt128)",
			"static System.UInt128.explicit operator double(System.UInt128)",
			"static System.UInt128.explicit operator float(System.UInt128)",
			"static System.UInt128.explicit operator short(System.UInt128)",
			"static System.UInt128.explicit operator int(System.UInt128)",
			"static System.UInt128.explicit operator long(System.UInt128)",
			"static System.UInt128.explicit operator System.Int128(System.UInt128)",
			"static System.UInt128.explicit operator sbyte(System.UInt128)",
			"static System.UInt128.explicit operator ushort(System.UInt128)",
			"static System.UInt128.explicit operator uint(System.UInt128)",
			"static System.UInt128.explicit operator ulong(System.UInt128)",
			"static System.UInt128.explicit operator System.UInt128(short)",
			"static System.UInt128.explicit operator System.UInt128(int)",
			"static System.UInt128.explicit operator System.UInt128(long)",
			"static System.UInt128.explicit operator System.UInt128(sbyte)",
			"static System.UInt128.implicit operator System.UInt128(byte)",
			"static System.UInt128.implicit operator System.UInt128(ushort)",
			"static System.UInt128.implicit operator System.UInt128(uint)",
			"static System.UInt128.implicit operator System.UInt128(ulong)");

		AssertMembers(
			typeof(Jazor.CLR.UInt128Module),
			Op.Import,
			"System/UInt128Module.js",
			"static System.UInt128.explicit operator checked byte(System.UInt128)",
			"static System.UInt128.explicit operator decimal(System.UInt128)",
			"static System.UInt128.explicit operator checked short(System.UInt128)",
			"static System.UInt128.explicit operator checked int(System.UInt128)",
			"static System.UInt128.explicit operator checked long(System.UInt128)",
			"static System.UInt128.explicit operator checked System.Int128(System.UInt128)",
			"static System.UInt128.explicit operator checked sbyte(System.UInt128)",
			"static System.UInt128.explicit operator checked ushort(System.UInt128)",
			"static System.UInt128.explicit operator checked uint(System.UInt128)",
			"static System.UInt128.explicit operator checked ulong(System.UInt128)",
			"static System.UInt128.explicit operator System.UInt128(decimal)",
			"static System.UInt128.explicit operator checked System.UInt128(short)",
			"static System.UInt128.explicit operator checked System.UInt128(int)",
			"static System.UInt128.explicit operator checked System.UInt128(long)",
			"static System.UInt128.explicit operator checked System.UInt128(sbyte)");
	}

	[TestMethod]
	public void BigIntegerToSingleConversion_UsesBigIntCarrierAndInlineSinglePrecisionProjection()
	{
		AssertTypeAlias(typeof(Jazor.CLR.BigIntegerModule), "System.Numerics.BigInteger", "BigInt");
		AssertMember(
			typeof(Jazor.CLR.BigIntegerModule),
			"static System.Numerics.BigInteger.explicit operator float(System.Numerics.BigInteger)",
			Op.Inline);
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

	private static void AssertRecoveredInt128CheckedAndSpanMembers(Type moduleType, string typeName, string modulePath)
	{
		foreach (var member in new[]
		{
			$"static {typeName}.TryParse(System.ReadOnlySpan<char>, out {typeName})",
			$"static {typeName}.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)",
			$"static {typeName}.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out {typeName})",
			$"static {typeName}.operator checked +({typeName}, {typeName})",
			$"static {typeName}.operator checked --({typeName})",
			$"static {typeName}.operator checked /({typeName}, {typeName})",
			$"static {typeName}.operator checked ++({typeName})",
			$"static {typeName}.operator checked *({typeName}, {typeName})",
			$"static {typeName}.operator checked -({typeName}, {typeName})",
			$"static {typeName}.operator checked -({typeName})"
		})
		{
			AssertMember(moduleType, member, Op.Import, modulePath);
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

	private static void AssertMembers(Type type, Op op, string? modulePath, params string[] members)
	{
		foreach (var member in members)
			AssertMember(type, member, op, modulePath);
	}

	private static string GetClrTypeName(Type moduleType)
		=> moduleType == typeof(Jazor.CLR.HalfModule)
			? "System.Half"
			: moduleType == typeof(Jazor.CLR.Int128Module)
				? "System.Int128"
				: "System.UInt128";
}
