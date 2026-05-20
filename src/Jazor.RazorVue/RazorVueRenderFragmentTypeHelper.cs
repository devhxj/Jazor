using System;
using Jazor.Common;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue;

internal static class RazorVueRenderFragmentTypeHelper
{
    // Under Format.NameFormat, delegate canonical names include the full signature,
    // not just the delegate type name.
    internal const string RenderFragmentMetadataName = "Microsoft.AspNetCore.Components.RenderFragment(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)";
    internal const string ParameterizedRenderFragmentMetadataName = "Microsoft.AspNetCore.Components.RenderFragment<TValue>(TValue)";

    public static bool IsRenderFragmentType(ITypeSymbol? typeSymbol)
        => TryGetOriginalDefinitionDisplayName(typeSymbol, out var displayName) &&
           (string.Equals(displayName, RenderFragmentMetadataName, StringComparison.Ordinal) ||
            string.Equals(displayName, ParameterizedRenderFragmentMetadataName, StringComparison.Ordinal));

    public static bool IsUntypedRenderFragmentType(ITypeSymbol? typeSymbol)
        => TryGetOriginalDefinitionDisplayName(typeSymbol, out var displayName) &&
           string.Equals(displayName, RenderFragmentMetadataName, StringComparison.Ordinal);

    public static bool IsParameterizedRenderFragmentType(ITypeSymbol? typeSymbol)
        => TryGetOriginalDefinitionDisplayName(typeSymbol, out var displayName) &&
           string.Equals(displayName, ParameterizedRenderFragmentMetadataName, StringComparison.Ordinal);

    private static bool TryGetOriginalDefinitionDisplayName(ITypeSymbol? typeSymbol, out string displayName)
    {
        displayName = string.Empty;
        if (typeSymbol is null)
            return false;

        if (typeSymbol is INamedTypeSymbol namedType &&
            namedType.IsGenericType &&
            namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
        {
            typeSymbol = namedType.TypeArguments[0];
        }

        if (typeSymbol is not INamedTypeSymbol renderFragmentType)
            return false;

        displayName = renderFragmentType.OriginalDefinition.ToDisplayString(Format.NameFormat);
        return true;
    }
}
