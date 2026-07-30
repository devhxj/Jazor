using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Common;

/// <summary>
/// 识别 structural record，并统一判断其成员是否参与结构化 lowering。
/// </summary>
/// <remarks>
/// 该工具只做编译期形状判断，不发射 runtime 类型，也不负责构造对象 AST。
/// compiler、analyzer 和宿主投影必须使用一致的判断规则，避免同一个 record 在不同阶段被当成
/// nominal class 或 structural value。
/// </remarks>
public static class StructuralRecordSupport
{
	public static bool IsStructuralRecordType(ITypeSymbol? typeSymbol)
		=> typeSymbol is INamedTypeSymbol { IsRecord: true };

	public static bool IsStructuralRecordMember(ISymbol? symbol)
		=> symbol switch
		{
			IPropertySymbol property => IsStructuralRecordProperty(property),
			IFieldSymbol { IsStatic: false, ContainingType: { } containingType } =>
				IsStructuralRecordType(containingType),
			IMethodSymbol
			{
				MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet,
				AssociatedSymbol: IPropertySymbol property
			} => IsStructuralRecordMember(property),
			_ => false
		};

	private static bool IsStructuralRecordProperty(IPropertySymbol property)
	{
		if (property is not { IsStatic: false, Parameters.Length: 0, ContainingType: { } containingType } ||
			!IsStructuralRecordType(containingType))
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
	{
		if (symbol is null ||
			IsStructuralRecordMember(symbol) ||
			IsExtensionMethod(symbol))
		{
			return false;
		}

		if (IsStructuralRecordType(symbol.ContainingType) ||
			IsStructuralRecordType(symbol.OriginalDefinition.ContainingType))
		{
			return true;
		}

		return IsStructuralRecordType(hostType);
	}

	public static bool IsStructuralRecordRuntimeSemanticInvocation(IInvocationOperation? invocation)
	{
		if (invocation is null ||
			!IsRecordRuntimeSemanticMember(invocation.TargetMethod))
		{
			return false;
		}

		if (IsStructuralRecordOperand(invocation.Instance))
			return true;

		foreach (var argument in invocation.Arguments)
		{
			if (IsStructuralRecordOperand(argument.Value))
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
	{
		for (var current = operation; current is not null;)
		{
			if (IsStructuralRecordType(current.Type))
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
