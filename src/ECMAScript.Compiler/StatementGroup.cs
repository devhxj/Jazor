using System.Runtime.CompilerServices;
using Acornima;
using Acornima.Ast;

namespace ECMAScript.Compiler;

public sealed class StatementGroup(in NodeList<Statement> elements)
    : Statement(NodeType.ExpressionStatement)
{
    private readonly NodeList<Statement> _elements = elements;

    public ref readonly NodeList<Statement> Elements
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _elements;
    }

    protected override object? Accept(AstVisitor visitor)
    {
        if (visitor is AstToECMAScriptConverter v)
        {
            return v.VisitStatementGroup(this);
        }
        else
        {
            for (var i = 0; i < _elements.Count; i++)
            {
                var elementsItem = _elements[i];
                if (elementsItem is not null)
                {
                    visitor.Visit(elementsItem);
                }
            }
            return this;
        }
    }
}

public sealed class AstToECMAScriptConverter(JavaScriptTextWriter writer, AstToJavaScriptOptions options)
    : AstToJavaScriptConverter(writer, options)
{
    public object? VisitStatementGroup(StatementGroup node)
    {
        VisitStatementList(in node.Elements, (_, _, _, _) => StatementFlags.NeedsSemicolon);
        return node;
    }
}

public record class AstToECMAScriptOptions : AstToJavaScriptOptions
{
    protected override AstToJavaScriptConverter CreateConverter(JavaScriptTextWriter writer)
        => new AstToECMAScriptConverter(writer, this);

    public static new readonly AstToJavaScriptOptions Default = new AstToECMAScriptOptions();
}

public static class AstToECMAScript
{
    public static string ToKnRECMAScript(this Node node)
    {
        return node.ToJavaScript(KnRJavaScriptTextFormatterOptions.Default, AstToECMAScriptOptions.Default);
    }

    public static string ToECMAScript(this Node node)
    {
        return node.ToJavaScript(JavaScriptTextWriterOptions.Default, AstToECMAScriptOptions.Default);
    }
}
