using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue;

internal static class RazorVueOperationNormalizer
{
    public static IOperation? Unwrap(IOperation? operation)
    {
        var current = operation;

        while (true)
        {
            switch (current)
            {
                case IConversionOperation conversion when conversion.IsImplicit:
                    current = conversion.Operand;
                    continue;
                case IInvocationOperation invocation when IsRuntimeHelpersTypeCheck(invocation):
                    current = invocation.Arguments.Length == 1
                        ? invocation.Arguments[0].Value
                        : null;
                    continue;
                default:
                    return current;
            }
        }
    }

    private static bool IsRuntimeHelpersTypeCheck(IInvocationOperation invocation)
        => invocation.TargetMethod.Name == "TypeCheck" &&
           invocation.Arguments.Length == 1 &&
           string.Equals(
               invocation.TargetMethod.ContainingType?.ToDisplayString(),
               "Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers",
               StringComparison.Ordinal);
}
