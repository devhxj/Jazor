using System;
using System.Numerics;
using Acornima.Ast;
using Microsoft.CodeAnalysis;

namespace Jazor.Compiler;

/// <summary>
/// Creates C# default values for RazorVue current-component state slots.
/// This keeps CLR default-value semantics in the compiler layer while the
/// RazorVue artifact builder owns only Vue render-function framing.
/// </summary>
public static class CurrentComponentStateDefaultInitializer
{
    public static Expression CreateExpression(ITypeSymbol type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (type is ITypeParameterSymbol)
            throw CreateUnsupportedException(type);

        if (!type.IsValueType ||
            type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return new NullLiteral("null");
        }

        if (type.TypeKind == TypeKind.Enum)
            return CreateEnumZeroExpression(type);

        if (IsSystemHalfType(type))
            return new NumericLiteral(0, "0");

        return type.SpecialType switch
        {
            SpecialType.System_Boolean => new BooleanLiteral(false, "false"),
            SpecialType.System_Char => JavaScriptAstFactory.CreateStringLiteral("\0"),
            SpecialType.System_SByte or
            SpecialType.System_Byte or
            SpecialType.System_Int16 or
            SpecialType.System_UInt16 or
            SpecialType.System_Int32 or
            SpecialType.System_UInt32 or
            SpecialType.System_Single or
            SpecialType.System_Double or
            SpecialType.System_Decimal => new NumericLiteral(0, "0"),
            SpecialType.System_Int64 or
            SpecialType.System_UInt64 => new BigIntLiteral(BigInteger.Zero, "0n"),
            _ => IsBigIntMappedValueType(type)
                ? new BigIntLiteral(BigInteger.Zero, "0n")
                : throw CreateUnsupportedException(type)
        };
    }

    private static Expression CreateEnumZeroExpression(ITypeSymbol type)
    {
        if (Util.IsStringEnumType(type))
            throw CreateUnsupportedException(type);

        if (type is INamedTypeSymbol { EnumUnderlyingType: { } underlyingType } &&
            underlyingType.SpecialType is SpecialType.System_Int64 or SpecialType.System_UInt64)
        {
            return new BigIntLiteral(BigInteger.Zero, "0n");
        }

        return new NumericLiteral(0, "0");
    }

    private static bool IsSystemHalfType(ITypeSymbol type)
        => type.OriginalDefinition is { Name: "Half" } original &&
           original.ContainingNamespace?.ToDisplayString() == "System";

    private static bool IsBigIntMappedValueType(ITypeSymbol type)
    {
        var displayName = type.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat);
        return displayName is "System.Int128" or "System.UInt128" or "System.Numerics.BigInteger";
    }

    private static NotSupportedException CreateUnsupportedException(ITypeSymbol type)
        => new(
            "Current-component state member type '" +
            type.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat) +
            "' requires an explicit initializer because RazorVue current-component state default lowering v1 only supports primitive scalar defaults.");
}
