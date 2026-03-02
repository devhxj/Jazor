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
    private LineComment(in string comment) : base(NodeType.Extension) => Comment = comment;

    public string Comment { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected override object? Accept(AstVisitor visitor)
        => visitor is AstToECMAScriptConverter v ? v.VisitLineComment(this) : this;
}

/// <summary>
/// 自定义表达式节点
/// </summary>
public sealed class UnsafeRawExpression : Expression
{
    public UnsafeRawExpression(in string unsafeRaw) : base(NodeType.Extension) => UnsafeRaw = unsafeRaw;

    public string UnsafeRaw { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected override object? Accept(AstVisitor visitor)
        => visitor is AstToECMAScriptConverter v ? v.VisitUnsafeRawExpression(this) : this;
}

public sealed class AstToECMAScriptConverter(JavaScriptTextWriter writer, AstToJavaScriptOptions options)
    : AstToJavaScriptConverter(writer, options)
{
    public object? VisitLineComment(LineComment node)
    {
        Writer.WriteLineComment(node.Comment, TriviaFlags.LeadingNewLineRequired);
        return node;
    }

    public object? VisitUnsafeRawExpression(UnsafeRawExpression node)
    {
		WriteContext.ChangeNodeProperty(nameof(node.UnsafeRaw), static node => node.As<UnsafeRawExpression>().UnsafeRaw);
		Writer.WriteLiteral(node.UnsafeRaw, TokenKind.StringLiteral, ref WriteContext);

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
        => node.ToJavaScript(KnRJavaScriptTextFormatterOptions.Default, AstToECMAScriptOptions.Default);

    public static string ToECMAScript(this Node node)
        => node.ToJavaScript(JavaScriptTextWriterOptions.Default, AstToECMAScriptOptions.Default);
}
