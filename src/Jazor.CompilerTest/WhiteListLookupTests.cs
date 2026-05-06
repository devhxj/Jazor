using Basic.Reference.Assemblies;
using Jazor.Common;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class WhiteListLookupTests
{
	[TestMethod]
	public void TryGetValue_SourceExternAccessor_UsesExactNameFormatKey()
	{
		var getter = CompileGetterSymbol("""
			namespace Demo;

			public abstract class JsBigInt
			{
			    public extern static JsBigInt Zero { get; }
			}
			""");

		var rawDisplay = getter.OriginalDefinition.ToDisplayString(Format.NameFormat);
		Assert.IsTrue(rawDisplay.Contains("extern", StringComparison.Ordinal), "Exact NameFormat-based key should retain the extern modifier.");

		var mappings = new Dictionary<string, int>(StringComparer.Ordinal)
		{
			[rawDisplay] = 42
		};

		var matched = InvokeTryGetValue(mappings, getter, out var displayString, out var value);

		Assert.IsTrue(matched);
		Assert.AreEqual(rawDisplay, displayString);
		Assert.AreEqual(42, value);
	}

	[TestMethod]
	public void WhiteListMembers_ImplicitExternAccessorKey_RetainsExternModifier()
	{
		var members = GetWhiteListMembers();
		var zeroKey = members.Keys
			.Cast<object>()
			.Select(static key => key?.ToString())
			.Single(static key => string.Equals(key, "static ECMAScript.BigInt.Zero.get", StringComparison.Ordinal) || key?.EndsWith("ECMAScript.BigInt.Zero.get", StringComparison.Ordinal) == true);

		Assert.IsNotNull(zeroKey);
		Assert.IsTrue(zeroKey.Contains("extern", StringComparison.Ordinal), "Implicit ECMAScript host accessor keys must retain the exact NameFormat contract, including 'extern'.");
		Assert.IsFalse(members.Contains("static ECMAScript.BigInt.Zero.get"), "Legacy generator-side stripping of 'extern' must not remain in persisted whitelist keys.");
	}

	private static IMethodSymbol CompileGetterSymbol(string code)
	{
		var compilation = CSharpCompilation.Create(
			"WhiteListLookupTests",
			[CSharpSyntaxTree.ParseText(code)],
			Net100.References.All,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var diagnostics = compilation.GetDiagnostics()
			.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
			.ToArray();
		Assert.IsFalse(
			diagnostics.Length > 0,
			string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));

		var syntaxTree = compilation.SyntaxTrees.Single();
		var semanticModel = compilation.GetSemanticModel(syntaxTree);
		var propertyDeclaration = syntaxTree.GetRoot()
			.DescendantNodes()
			.OfType<PropertyDeclarationSyntax>()
			.Single();
		var property = semanticModel.GetDeclaredSymbol(propertyDeclaration);

		Assert.IsNotNull(property);
		Assert.IsNotNull(property.GetMethod);
		return property.GetMethod;
	}

	private static bool InvokeTryGetValue<T>(Dictionary<string, T> mappings, ISymbol symbol, out string displayString, out T value)
		where T : notnull
	{
		var lookupType = typeof(SemanticWalker).Assembly.GetType("Jazor.Compiler.WhiteListLookup", throwOnError: true)!;
		var method = lookupType
			.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
			.Single(static candidate =>
			{
				if (!string.Equals(candidate.Name, "TryGetValue", StringComparison.Ordinal))
					return false;

				var parameters = candidate.GetParameters();
				return parameters.Length == 4 && parameters[1].ParameterType == typeof(ISymbol);
			})
			.MakeGenericMethod(typeof(T));

		object?[] arguments = [mappings, symbol, null, null];
		var matched = (bool)method.Invoke(null, arguments)!;
		displayString = (string)arguments[2]!;
		value = (T)arguments[3]!;
		return matched;
	}

	private static System.Collections.IDictionary GetWhiteListMembers()
	{
		var whiteListType = typeof(SemanticWalker).Assembly.GetType("Jazor.Compiler.WhiteList", throwOnError: true)!;
		var membersField = whiteListType.GetField("Members", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

		Assert.IsNotNull(membersField);
		var members = membersField.GetValue(null) as System.Collections.IDictionary;
		Assert.IsNotNull(members);
		return members;
	}
}
