using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue;

internal static class RazorVueSlotParameterNames
{
    private const string ImplicitDefaultSlotParameterBaseName = "__jazorSlotContext";

    public static string CreateImplicitDefaultSlotParameterName(
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        var usedNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var symbol in allowedLocalSymbols)
            usedNames.Add(symbol.Name);
        foreach (var symbol in allowedParameterSymbols)
            usedNames.Add(symbol.Name);

        if (!usedNames.Contains(ImplicitDefaultSlotParameterBaseName))
            return ImplicitDefaultSlotParameterBaseName;

        for (var index = 1; ; index++)
        {
            var candidate = ImplicitDefaultSlotParameterBaseName + index.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (!usedNames.Contains(candidate))
                return candidate;
        }
    }
}
