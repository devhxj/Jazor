using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue;

internal static class RazorVuePropertyInitializerHelper
{
    public static bool IsNullForgivingPlaceholder(ExpressionSyntax initializerExpression)
    {
        if (initializerExpression is null)
            throw new ArgumentNullException(nameof(initializerExpression));

        var current = initializerExpression;
        var hadNullableSuppression = false;
        while (true)
        {
            switch (current)
            {
                case ParenthesizedExpressionSyntax parenthesized:
                    current = parenthesized.Expression;
                    continue;
                case PostfixUnaryExpressionSyntax postfix
                    when postfix.IsKind(SyntaxKind.SuppressNullableWarningExpression):
                    hadNullableSuppression = true;
                    current = postfix.Operand;
                    continue;
                default:
                    if (!hadNullableSuppression)
                        return false;

                    return current is DefaultExpressionSyntax ||
                           current.IsKind(SyntaxKind.DefaultLiteralExpression) ||
                           current.IsKind(SyntaxKind.NullLiteralExpression);
            }
        }
    }

    public static bool TryGetNormalizedOperation(
        SemanticModel semanticModel,
        ExpressionSyntax initializerExpression,
        out IOperation? operation)
    {
        if (semanticModel is null)
            throw new ArgumentNullException(nameof(semanticModel));
        if (initializerExpression is null)
            throw new ArgumentNullException(nameof(initializerExpression));

        operation = RazorVueOperationNormalizer.Unwrap(
            semanticModel.GetOperation(UnwrapInitializerExpression(initializerExpression)));
        return operation is not null;
    }

    public static ExpressionSyntax UnwrapInitializerExpression(ExpressionSyntax initializerExpression)
    {
        if (initializerExpression is null)
            throw new ArgumentNullException(nameof(initializerExpression));

        var current = initializerExpression;
        while (true)
        {
            switch (current)
            {
                case ParenthesizedExpressionSyntax parenthesized:
                    current = parenthesized.Expression;
                    continue;
                case PostfixUnaryExpressionSyntax postfix
                    when postfix.IsKind(SyntaxKind.SuppressNullableWarningExpression):
                    current = postfix.Operand;
                    continue;
                default:
                    return current;
            }
        }
    }
}
