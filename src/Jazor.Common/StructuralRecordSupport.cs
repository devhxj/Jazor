using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Common;

public static class StructuralRecordSupport
{
	public delegate bool SourceDataCarrierTypePredicate(INamedTypeSymbol typeSymbol);

	public static bool IsStructuralRecordType(ITypeSymbol? typeSymbol)
		=> typeSymbol is INamedTypeSymbol { IsRecord: true };

	public static bool IsStructuralType(
		ITypeSymbol? typeSymbol,
		SourceDataCarrierTypePredicate? sourceDataCarrierPredicate = null)
		=> typeSymbol is INamedTypeSymbol namedType &&
		   (namedType.IsRecord || sourceDataCarrierPredicate?.Invoke(namedType) == true);

	public static bool IsStructuralRecordMember(ISymbol? symbol)
		=> IsStructuralMember(symbol);

	public static bool IsStructuralMember(
		ISymbol? symbol,
		SourceDataCarrierTypePredicate? sourceDataCarrierPredicate = null)
		=> symbol switch
		{
			IPropertySymbol property => IsStructuralProperty(property, sourceDataCarrierPredicate),
			IFieldSymbol { IsStatic: false, ContainingType: { } containingType } =>
				IsStructuralType(containingType, sourceDataCarrierPredicate),
			IMethodSymbol
			{
				MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet,
				AssociatedSymbol: IPropertySymbol property
			} => IsStructuralMember(property, sourceDataCarrierPredicate),
			_ => false
		};

	private static bool IsStructuralRecordProperty(IPropertySymbol property)
		=> IsStructuralProperty(property);

	private static bool IsStructuralProperty(
		IPropertySymbol property,
		SourceDataCarrierTypePredicate? sourceDataCarrierPredicate = null)
	{
		if (property is not { IsStatic: false, Parameters.Length: 0, ContainingType: { } containingType } ||
			!IsStructuralType(containingType, sourceDataCarrierPredicate))
		{
			return false;
		}

		if (property.IsAbstract)
			return true;

		if (!IsSourceDeclaredProperty(property) &&
			IsMetadataStructuralSettableProperty(property))
		{
			return true;
		}

		foreach (var member in containingType.GetMembers())
		{
			if (member is IFieldSymbol { IsStatic: false } field &&
				IsBackingFieldForProperty(field, property))
			{
				return true;
			}
		}

		return false;
	}

	private static bool IsSourceDeclaredProperty(IPropertySymbol property)
		=> property.Locations.Any(static location => location.IsInSource);

	public static bool IsSourceDeclaredAutoPropertyCandidate(IPropertySymbol property)
	{
		if (property is not { IsStatic: false, Parameters.Length: 0, ContainingType: { } } ||
			!IsSourceDeclaredProperty(property))
		{
			return false;
		}

		foreach (var member in property.ContainingType.GetMembers())
		{
			if (member is IFieldSymbol { IsStatic: false } field &&
				IsBackingFieldForProperty(field, property))
			{
				return true;
			}
		}

		return false;
	}

	private static bool IsMetadataStructuralSettableProperty(IPropertySymbol property)
	{
		var setMethod = property.SetMethod;
		return setMethod is not null &&
			   !setMethod.IsExtern &&
			   property.Parameters.Length == 0;
	}

	private static bool IsBackingFieldForProperty(IFieldSymbol field, IPropertySymbol property)
	{
		if (SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol, property))
			return true;

		// Metadata-only auto-properties can lose the AssociatedSymbol link. Keep this
		// fallback narrow so computed record properties do not become structural data.
		return string.Equals(
			field.Name,
			$"<{property.Name}>k__BackingField",
			StringComparison.Ordinal);
	}

	public static bool IsNonStructuralRecordRuntimeMember(ISymbol? symbol, ITypeSymbol? hostType = null)
		=> IsNonStructuralRuntimeMember(symbol, hostType);

	public static bool IsNonStructuralRuntimeMember(
		ISymbol? symbol,
		ITypeSymbol? hostType = null,
		SourceDataCarrierTypePredicate? sourceDataCarrierPredicate = null)
	{
		if (symbol is null ||
			IsStructuralMember(symbol, sourceDataCarrierPredicate) ||
			IsExtensionMethod(symbol))
		{
			return false;
		}

		if (IsStructuralType(symbol.ContainingType, sourceDataCarrierPredicate) ||
			IsStructuralType(symbol.OriginalDefinition.ContainingType, sourceDataCarrierPredicate))
		{
			return true;
		}

		return IsStructuralType(hostType, sourceDataCarrierPredicate);
	}

	public static bool IsStructuralRecordRuntimeSemanticInvocation(IInvocationOperation? invocation)
		=> IsStructuralRuntimeSemanticInvocation(invocation);

	public static bool IsStructuralRuntimeSemanticInvocation(
		IInvocationOperation? invocation,
		SourceDataCarrierTypePredicate? sourceDataCarrierPredicate = null)
	{
		if (invocation is null ||
			!IsRecordRuntimeSemanticMember(invocation.TargetMethod))
		{
			return false;
		}

		if (IsStructuralOperand(invocation.Instance, sourceDataCarrierPredicate))
			return true;

		foreach (var argument in invocation.Arguments)
		{
			if (IsStructuralOperand(argument.Value, sourceDataCarrierPredicate))
				return true;
		}

		return false;
	}

	public static bool IsRecordRuntimeSemanticMember(ISymbol? symbol)
	{
		if (symbol is not IMethodSymbol method)
			return false;

		if (method.Name is not "Equals" and not "GetHashCode" and not "ToString")
			return false;

		var containingType = method.ContainingType?.OriginalDefinition.ToDisplayString(Format.NameFormat);
		return containingType is "object"
			or "System.Collections.Generic.EqualityComparer<T>"
			or "System.Collections.Generic.IEqualityComparer<T>"
			or "System.Collections.IEqualityComparer";
	}

	private static bool IsStructuralRecordOperand(IOperation? operation)
		=> IsStructuralOperand(operation);

	private static bool IsStructuralOperand(
		IOperation? operation,
		SourceDataCarrierTypePredicate? sourceDataCarrierPredicate = null)
	{
		for (var current = operation; current is not null;)
		{
			if (IsStructuralType(current.Type, sourceDataCarrierPredicate))
				return true;

			current = current switch
			{
				IConversionOperation conversion => conversion.Operand,
				IParenthesizedOperation parenthesized => parenthesized.Operand,
				_ => null
			};
		}

		return false;
	}

	private static bool IsExtensionMethod(ISymbol symbol)
		=> symbol is IMethodSymbol method &&
		   (method.IsExtensionMethod || method.ReducedFrom is not null);
}
