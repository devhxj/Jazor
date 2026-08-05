// File: BlockComment.cs
// Purpose: Represents a compiler-owned comment node in the Acornima AST stream.
// 仅承载生成产物的辅助标记，不参与用户 C# 语义或 JavaScript 执行行为。
using Acornima;
using Acornima.Ast;
using System.Runtime.CompilerServices;
using static Acornima.JavaScriptTextWriter;

namespace Jazor.Compiler;

/// <summary>
/// 自定义注释节点
/// </summary>
/// <remarks>
/// BlockComment 是 statement-list 专用的 AST 扩展节点，只在最终 JavaScript writer 阶段写出块注释。
/// 它不参与 C# 运行时语义，也不能被普通 AST 优化器当成可执行语句处理。
/// </remarks>
public sealed class BlockComment(in string comment) : Statement(NodeType.Extension)
{
    public string Comment { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; } = comment;

    protected override object? Accept(AstVisitor visitor)
    {
        if (visitor is AstToJavaScriptConverter v)
        {
            v.Writer.WriteBlockComment([Comment], TriviaFlags.TrailingNewLineRequired);
        }

        return this;
    }
}
