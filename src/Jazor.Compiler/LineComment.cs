using Acornima;
using Acornima.Ast;
using System.Runtime.CompilerServices;
using static Acornima.JavaScriptTextWriter;

namespace Jazor.Compiler;

/// <summary>
/// 自定义注释节点
/// </summary>
public sealed class LineComment : Statement
{
    public LineComment(in string comment) : base(NodeType.Extension) => Comment = comment;

    public string Comment { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected override object? Accept(AstVisitor visitor)
    {
        if (visitor is AstToJavaScriptConverter v)
        {
            v.Writer.WriteLineComment(Comment, TriviaFlags.LeadingNewLineRequired);
        }

        return this;
    }
}
