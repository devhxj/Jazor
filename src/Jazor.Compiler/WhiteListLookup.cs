using Jazor.Common;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jazor.Compiler;

internal static class WhiteListLookup
{
	public static bool TryGetValue<T>(Dictionary<string, T> mappings, string lookupKey, out string displayString, out T value)
		where T : notnull
	{
		displayString = lookupKey;
		if (mappings.TryGetValue(lookupKey, out value))
			return true;

		if (TryGetGenericParameterEquivalentWhiteListValue(mappings, lookupKey, out var matchedKey, out value))
		{
			displayString = matchedKey;
			return true;
		}

		value = default!;
		return false;
	}

	public static bool TryGetValue<T>(Dictionary<string, T> mappings, ISymbol symbol, out string displayString, out T value)
		where T : notnull
	{
		foreach (var candidate in EnumerateWhiteListLookupSymbols(symbol))
		{
			var rawDisplayString = candidate.OriginalDefinition.ToDisplayString(Format.NameFormat);
			string? staticExtensionKey = null;

			foreach (var lookupKey in EnumerateWhiteListLookupKeys(rawDisplayString))
			{
				if (TryGetValue(mappings, lookupKey, out displayString, out value))
					return true;
			}

			if (candidate is IMethodSymbol method &&
				(method.IsExtensionMethod || method.ReducedFrom is not null))
			{
				var extensionSource = method.ReducedFrom?.OriginalDefinition ?? method.OriginalDefinition;
				staticExtensionKey = extensionSource.OriginalDefinition.ToDisplayString(Format.StaticExtensionNameFormat);
				foreach (var lookupKey in EnumerateWhiteListLookupKeys(staticExtensionKey))
				{
					if (TryGetValue(mappings, lookupKey, out displayString, out value))
						return true;
				}
			}

			if (candidate is IMethodSymbol supplementalMethod)
			{
				var synthesizedStaticKey = TryBuildMethodWhiteListKey(supplementalMethod);
				if (!string.IsNullOrEmpty(synthesizedStaticKey) &&
					!string.Equals(synthesizedStaticKey, rawDisplayString, StringComparison.Ordinal) &&
					!string.Equals(synthesizedStaticKey, staticExtensionKey, StringComparison.Ordinal))
				{
					foreach (var lookupKey in EnumerateWhiteListLookupKeys(synthesizedStaticKey!))
					{
						if (TryGetValue(mappings, lookupKey, out displayString, out value))
							return true;
					}
				}
			}
		}

		displayString = symbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
		value = default!;
		return false;
	}

	private static bool TryGetGenericParameterEquivalentWhiteListValue<T>(Dictionary<string, T> mappings, string lookupKey, out string matchedKey, out T value)
		where T : notnull
	{
		matchedKey = null!;
		value = default!;

		if (!TryBuildGenericParameterOrdinalMap(lookupKey, out var lookupGenericParameters))
			return false;

		foreach (var candidate in mappings)
		{
			if (!TryBuildGenericParameterOrdinalMap(candidate.Key, out var candidateGenericParameters))
				continue;

			if (!string.Equals(
					RewriteDeclaredGenericParameters(lookupKey, lookupGenericParameters),
					RewriteDeclaredGenericParameters(candidate.Key, candidateGenericParameters),
					StringComparison.Ordinal))
				continue;

			matchedKey = candidate.Key;
			value = candidate.Value;
			return true;
		}

		return false;
	}

	private static bool TryBuildGenericParameterOrdinalMap(string text, out Dictionary<string, int> genericParameters)
	{
		genericParameters = new Dictionary<string, int>(StringComparer.Ordinal);
		var declarationSegment = GetGenericParameterDeclarationSegment(text);
		if (string.IsNullOrEmpty(declarationSegment))
			return false;

		for (var index = 0; index < declarationSegment.Length;)
		{
			if (!IsIdentifierStart(declarationSegment[index]))
			{
				index++;
				continue;
			}

			var tokenStart = index++;
			while (index < declarationSegment.Length && IsIdentifierPart(declarationSegment[index]))
				index++;

			var token = declarationSegment.Substring(tokenStart, index - tokenStart);
			var previous = GetPreviousMeaningfulChar(declarationSegment, tokenStart - 1);
			var next = GetNextMeaningfulChar(declarationSegment, index);
			if ((previous is '<' or ',') &&
				next is '>' or ',')
			{
				if (!genericParameters.ContainsKey(token))
					genericParameters[token] = genericParameters.Count;
			}
		}

		return genericParameters.Count > 0;
	}

	private static string RewriteDeclaredGenericParameters(string text, IReadOnlyDictionary<string, int> genericParameters)
	{
		var builder = new StringBuilder(text.Length);
		for (var index = 0; index < text.Length;)
		{
			if (!IsIdentifierStart(text[index]))
			{
				builder.Append(text[index]);
				index++;
				continue;
			}

			var tokenStart = index++;
			while (index < text.Length && IsIdentifierPart(text[index]))
				index++;

			var token = text.Substring(tokenStart, index - tokenStart);
			var previous = GetPreviousMeaningfulChar(text, tokenStart - 1);
			var next = GetNextMeaningfulChar(text, index);
			if (genericParameters.TryGetValue(token, out var ordinal) &&
				previous != '.' &&
				next != '.')
				builder.Append("{generic_parameter_").Append(ordinal).Append('}');
			else
				builder.Append(token);
		}

		return builder.ToString();
	}

	private static string GetGenericParameterDeclarationSegment(string text)
	{
		var end = text.IndexOf('(');
		if (end < 0)
			end = text.Length;

		foreach (var accessor in new[] { ".get", ".set", ".add", ".remove" })
		{
			var accessorIndex = text.LastIndexOf(accessor, StringComparison.Ordinal);
			if (accessorIndex >= 0 && accessorIndex < end)
				end = accessorIndex;
		}

		return end <= 0 ? string.Empty : text.Substring(0, end);
	}

	private static bool IsIdentifierStart(char ch)
		=> char.IsLetter(ch) || ch == '_';

	private static bool IsIdentifierPart(char ch)
		=> char.IsLetterOrDigit(ch) || ch == '_';

	private static char? GetPreviousMeaningfulChar(string text, int index)
	{
		for (var current = index; current >= 0; current--)
		{
			if (!char.IsWhiteSpace(text[current]))
				return text[current];
		}

		return null;
	}

	private static char? GetNextMeaningfulChar(string text, int index)
	{
		for (var current = index; current < text.Length; current++)
		{
			if (!char.IsWhiteSpace(text[current]))
				return text[current];
		}

		return null;
	}

	private static string? TryBuildMethodWhiteListKey(IMethodSymbol method)
	{
		var source = method.ReducedFrom?.OriginalDefinition ?? method.OriginalDefinition;
		if (source.ContainingType is null)
			return null;

		var builder = new StringBuilder();
		if (source.IsExtern)
			builder.Append("extern ");

		if (source.IsStatic)
			builder.Append("static ");

		builder.Append(source.ContainingType.OriginalDefinition.ToDisplayString(Format.NameFormat));
		builder.Append('.');
		builder.Append(source.Name);

		if (source.TypeParameters.Length > 0)
		{
			builder.Append('<');
			for (var i = 0; i < source.TypeParameters.Length; i++)
			{
				if (i > 0)
					builder.Append(", ");

				builder.Append(source.TypeParameters[i].Name);
			}

			builder.Append('>');
		}

		builder.Append('(');
		for (var i = 0; i < source.Parameters.Length; i++)
		{
			if (i > 0)
				builder.Append(", ");

			var parameter = source.Parameters[i];
			if (parameter.RefKind == RefKind.Ref)
				builder.Append("ref ");
			else if (parameter.RefKind == RefKind.Out)
				builder.Append("out ");
			else if (parameter.RefKind == RefKind.In)
				builder.Append("in ");

			if (parameter.IsParams)
				builder.Append("params ");

			builder.Append(parameter.Type.OriginalDefinition.ToDisplayString(Format.NameFormat));
		}

		builder.Append(')');
		return builder.ToString();
	}

	private static IEnumerable<ISymbol> EnumerateWhiteListLookupSymbols(ISymbol symbol)
	{
		var seen = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

		foreach (var candidate in EnumerateWithOverrideFallback(symbol.OriginalDefinition))
		{
			if (seen.Add(candidate))
				yield return candidate;
		}

		foreach (var candidate in EnumerateContainingTypeImplementationCandidates(symbol))
		{
			foreach (var fallback in EnumerateWithOverrideFallback(candidate))
			{
				if (seen.Add(fallback))
					yield return fallback;
			}
		}
	}

	private static IEnumerable<ISymbol> EnumerateWithOverrideFallback(ISymbol symbol)
	{
		for (ISymbol? current = symbol; current is not null; current = GetFallbackSymbol(current))
			yield return current;
	}

	private static IEnumerable<ISymbol> EnumerateContainingTypeImplementationCandidates(ISymbol symbol)
	{
		if (symbol.ContainingType is null)
			yield break;

		if (symbol is IMethodSymbol method)
		{
			foreach (var candidate in symbol.ContainingType.GetMembers(method.Name).OfType<IMethodSymbol>())
			{
				if (!IsCompatibleMethodCandidate(method, candidate))
					continue;

				yield return candidate.OriginalDefinition;
			}

			yield break;
		}

		if (symbol is IPropertySymbol property)
		{
			foreach (var candidate in symbol.ContainingType.GetMembers(property.Name).OfType<IPropertySymbol>())
			{
				if (!IsCompatiblePropertyCandidate(property, candidate))
					continue;

				yield return candidate.OriginalDefinition;
			}
		}
	}

	private static bool IsCompatibleMethodCandidate(IMethodSymbol source, IMethodSymbol candidate)
	{
		if (source.MethodKind != candidate.MethodKind ||
			source.Name != candidate.Name ||
			source.IsStatic != candidate.IsStatic ||
			source.Arity != candidate.Arity ||
			source.Parameters.Length != candidate.Parameters.Length)
			return false;

		for (var i = 0; i < source.Parameters.Length; i++)
		{
			if (source.Parameters[i].RefKind != candidate.Parameters[i].RefKind ||
				source.Parameters[i].IsParams != candidate.Parameters[i].IsParams)
				return false;

			if (!SymbolEqualityComparer.Default.Equals(
					source.Parameters[i].Type.OriginalDefinition,
					candidate.Parameters[i].Type.OriginalDefinition))
				return false;
		}

		return true;
	}

	private static bool IsCompatiblePropertyCandidate(IPropertySymbol source, IPropertySymbol candidate)
	{
		if (source.Name != candidate.Name ||
			source.IsStatic != candidate.IsStatic ||
			source.Parameters.Length != candidate.Parameters.Length)
			return false;

		for (var i = 0; i < source.Parameters.Length; i++)
		{
			if (source.Parameters[i].RefKind != candidate.Parameters[i].RefKind)
				return false;

			if (!SymbolEqualityComparer.Default.Equals(
					source.Parameters[i].Type.OriginalDefinition,
					candidate.Parameters[i].Type.OriginalDefinition))
				return false;
		}

		return SymbolEqualityComparer.Default.Equals(
			source.Type.OriginalDefinition,
			candidate.Type.OriginalDefinition);
	}

	public static ISymbol? GetFallbackSymbol(ISymbol symbol)
		=> symbol switch
		{
			IMethodSymbol { ReducedFrom: not null } method => method.ReducedFrom.OriginalDefinition,
			IMethodSymbol { OverriddenMethod: not null } method => method.OverriddenMethod.OriginalDefinition,
			IPropertySymbol { OverriddenProperty: not null } property => property.OverriddenProperty.OriginalDefinition,
			IEventSymbol { OverriddenEvent: not null } @event => @event.OverriddenEvent.OriginalDefinition,
			_ => null
		};

	private static IEnumerable<string> EnumerateWhiteListLookupKeys(string displayString)
	{
		yield return displayString;

		var normalizedConstFieldDisplay = NormalizeConstFieldDisplay(displayString);
		if (normalizedConstFieldDisplay is { Length: > 0 } &&
			!string.Equals(normalizedConstFieldDisplay, displayString, StringComparison.Ordinal))
			yield return normalizedConstFieldDisplay;

		var normalizedExtensionDisplay = NormalizeExtensionThisParameterDisplay(displayString);
		if (normalizedExtensionDisplay is { Length: > 0 } &&
			!string.Equals(normalizedExtensionDisplay, displayString, StringComparison.Ordinal))
			yield return normalizedExtensionDisplay;

		var normalizedStaticDisplay = NormalizeStaticAbstractLikeDisplay(displayString);
		if (normalizedStaticDisplay is { Length: > 0 } &&
			!string.Equals(normalizedStaticDisplay, displayString, StringComparison.Ordinal))
			yield return normalizedStaticDisplay;

		const string virtualPrefix = "virtual ";
		const string overridePrefix = "override ";
		const string abstractPrefix = "abstract ";

		if (displayString.StartsWith(virtualPrefix, StringComparison.Ordinal))
		{
			yield return displayString.Substring(virtualPrefix.Length);
			yield break;
		}

		if (displayString.StartsWith(overridePrefix, StringComparison.Ordinal))
		{
			yield return displayString.Substring(overridePrefix.Length);
			yield return virtualPrefix + displayString.Substring(overridePrefix.Length);
			yield break;
		}

		if (displayString.StartsWith(abstractPrefix, StringComparison.Ordinal))
		{
			yield return displayString.Substring(abstractPrefix.Length);
			yield return virtualPrefix + displayString.Substring(abstractPrefix.Length);
			yield break;
		}

		yield return virtualPrefix + displayString;
		yield return overridePrefix + displayString;
		yield return abstractPrefix + displayString;

		static string? NormalizeExtensionThisParameterDisplay(string text)
		{
			var normalized = text
				.Replace("(this ", "(")
				.Replace(", this ", ", ");

			return string.Equals(normalized, text, StringComparison.Ordinal) ? null : normalized;
		}

		static string? NormalizeConstFieldDisplay(string text)
		{
			const string constPrefix = "const ";
			if (!text.StartsWith(constPrefix, StringComparison.Ordinal))
				return null;

			var end = text.IndexOf(" = ", StringComparison.Ordinal);
			var withoutInitializer = end >= 0 ? text.Substring(0, end) : text;
			return "static " + withoutInitializer.Substring(constPrefix.Length);
		}

		static string? NormalizeStaticAbstractLikeDisplay(string text)
		{
			const string staticAbstractPrefix = "static abstract ";
			const string staticVirtualPrefix = "static virtual ";
			const string staticOverridePrefix = "static override ";
			const string staticSealedPrefix = "static sealed ";

			if (text.StartsWith(staticAbstractPrefix, StringComparison.Ordinal))
				return "static " + text.Substring(staticAbstractPrefix.Length);
			if (text.StartsWith(staticVirtualPrefix, StringComparison.Ordinal))
				return "static " + text.Substring(staticVirtualPrefix.Length);
			if (text.StartsWith(staticOverridePrefix, StringComparison.Ordinal))
				return "static " + text.Substring(staticOverridePrefix.Length);
			if (text.StartsWith(staticSealedPrefix, StringComparison.Ordinal))
				return "static " + text.Substring(staticSealedPrefix.Length);

			return null;
		}
	}
}
