using Acornima;
using Acornima.Ast;
using System.Runtime.CompilerServices;

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
