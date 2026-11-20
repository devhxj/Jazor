using Jolt.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.Compiler;

internal sealed partial class Projection
{
    private string _dynamicComponentPath = "";

    private void EmitDynamicComponent(JoltElement element)
    {
        var selectors = element.Attributes.Where(attribute => attribute is { Kind: JoltAttributeKind.Markup, Name: "is" }).ToArray();
        if (selectors.Length != 1 || selectors[0].Value is not [JoltExpression expression])
        {
            Report("JOLT1042", selectors.LastOrDefault()?.NameSpan ?? element.NameSpan,
                "<component> requires one is attribute with a C# expression that selects visible local components.");
            Write("null!");
            return;
        }

        var selector = SyntaxFactory.ParseExpression(expression.Code);
        if (selector.ContainsDiagnostics)
        {
            Report("JOLT1042", expression.CodeSpan, "Invalid dynamic component selector.");
            Write("null!");
            return;
        }

        var attributes = element.Attributes.Remove(selectors[0]);
        EmitChoice(selector);

        TextSpan SourceSpan(ExpressionSyntax syntax) => new(expression.CodeSpan.Start + syntax.SpanStart, syntax.Span.Length);

        void EmitChoice(ExpressionSyntax choice)
        {
            switch (choice)
            {
                case ParenthesizedExpressionSyntax group:
                    EmitChoice(group.Expression);
                    return;
                case ConditionalExpressionSyntax conditional:
                    // The conditional evaluates its condition once and renders only the
                    // chosen vnode. Roslyn still checks each candidate's props and slots.
                    Write("(");
                    int start = _code.Length;
                    var condition = SourceSpan(conditional.Condition);
                    Write("(");
                    Copy(condition);
                    Write(")");
                    _anchors.Add((TextSpan.FromBounds(start, _code.Length), condition));
                    Write(" ? ");
                    EmitChoice(conditional.WhenTrue);
                    Write(" : ");
                    EmitChoice(conditional.WhenFalse);
                    Write(")");
                    return;
                case IdentifierNameSyntax identifier:
                    var name = identifier.Identifier.ValueText;
                    var span = SourceSpan(identifier);
                    var candidates = _components.Where(component => component.Name == name).ToArray();
                    if (candidates.Length == 1 && !candidates[0].IsVueBuiltIn &&
                        (candidates[0].Npm is null || candidates[0].Npm!.LocalPath is not null))
                    {
                        // A shared child can appear under different scoped slot types.
                        // Its generic binding follows the candidate path as well as its source span.
                        var path = _dynamicComponentPath;
                        _dynamicComponentPath = $"{path}/{span.Start}";
                        EmitElement(element with { Name = name, NameSpan = span, Attributes = attributes, ClosingNameSpan = null });
                        _dynamicComponentPath = path;
                        return;
                    }
                    Report("JOLT1042", span, candidates.Length > 1
                        ? $"Dynamic component candidate '{name}' is ambiguous."
                        : $"Dynamic component candidate '{name}' must name a visible local .jazor or .vue component.");
                    break;
                default:
                    Report("JOLT1042", SourceSpan(choice), "Use local component names as the branches of a dynamic component selector.");
                    break;
            }
            Write("null!");
        }
    }
}
