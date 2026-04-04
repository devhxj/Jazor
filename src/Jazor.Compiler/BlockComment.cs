using Acornima;
using Acornima.Ast;
using System.Runtime.CompilerServices;
using static Acornima.JavaScriptTextWriter;

namespace Jazor.Compiler;

/// <summary>
/// 自定义注释节点
/// </summary>
public sealed class BlockComment(in string comment) : Expression(NodeType.Extension)
{
    public string Comment { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; } = comment;

    protected override object? Accept(AstVisitor visitor)
    {
        if (visitor is AstToJavaScriptConverter v)
        {
            v.Writer.WriteBlockComment([Comment], TriviaFlags.None);
        }

        return this;
    }
}
