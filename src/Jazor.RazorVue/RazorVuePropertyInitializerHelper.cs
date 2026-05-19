using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue;

internal static class RazorVuePropertyInitializerHelper
{
    public static bool TryGetPropertyValueOperation(
        SemanticModel semanticModel,
        PropertyDeclarationSyntax declaration,
        out IOperation? operation)
    {
        if (semanticModel is null)
            throw new ArgumentNullException(nameof(semanticModel));
        if (declaration is null)
            throw new ArgumentNullException(nameof(declaration));

        if (declaration.ExpressionBody?.Expression is { } propertyExpressionBody &&
            TryGetNormalizedOperation(semanticModel, propertyExpressionBody, out operation))
        {
            return true;
        }

        if (declaration.Initializer?.Value is { } propertyInitializer &&
            TryGetNormalizedOperation(semanticModel, propertyInitializer, out operation))
        {
            return true;
        }

        if (declaration.AccessorList is null)
        {
            operation = null;
            return false;
        }

        var getter = declaration.AccessorList.Accessors
            .FirstOrDefault(static accessor => accessor.IsKind(SyntaxKind.GetAccessorDeclaration));
        if (getter?.ExpressionBody?.Expression is { } getterExpressionBody &&
            TryGetNormalizedOperation(semanticModel, getterExpressionBody, out operation))
        {
            return true;
        }

        if (getter?.Body is not null &&
            getter.Body.Statements.Count == 1 &&
            getter.Body.Statements[0] is ReturnStatementSyntax { Expression: { } returnExpression } &&
            TryGetNormalizedOperation(semanticModel, returnExpression, out operation))
        {
            return true;
        }

        operation = null;
        return false;
    }

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
