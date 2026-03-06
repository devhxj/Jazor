using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Linq;

namespace Jazor.Compiler;

public partial class SemanticWalker
{
	/// <summary>
	/// 处理 IInvalidOperation，它包装了在当前上下文中无法用单一类型表示其结果的操作。
	/// 此方法会尝试解包结合语法节点和子操作实现语义的精准匹配（不支持dynamic，不用考虑这个）。
	/// 在诊断器没有异常的情况下，理论上不会触发这个方法
	/// </summary>
	/// <param name="operation">当前访问的 IInvalidOperation。</param>
	/// <param name="argument">当前访问的 operation 的根 operation。</param>
	/// <returns>转换后的 Acornima AST Node。</returns>
	public override Node? VisitInvalid(IInvalidOperation operation, SenseArgument argument)
		=> ConvertFromSyntaxNode(operation.Syntax);

	/// <summary>
	/// 核心转换器，基于 C# 语法节点类型进行模式匹配，转换为 Acornima AST 节点。
	/// </summary>
	/// <param name="node">要转换的 C# 语法节点。</param>
	/// <returns>转换后的 Acornima AST Node。</returns>
	/// <exception cref="ArgumentNullException">当 syn 参数为 null 时抛出。</exception>
	/// <exception cref="NotSupportedException">当遇到不支持的语法节点类型时抛出。</exception>
	private Node ConvertFromSyntaxNode(SyntaxNode node)
	{					
		var result = node switch
		{
			// 基础表达式和字面量
			LiteralExpressionSyntax lit => lit.Token.Value switch
			{
				null => Null,
				bool b => new BooleanLiteral(b, b.ToString().ToLower()),
				char c => new StringLiteral(c.ToString(), $"'{c}'"),
				string s => new StringLiteral(s, $"'{s}'"),
				sbyte sb => new NumericLiteral(sb, sb.ToString()),
				byte b => new NumericLiteral(b, b.ToString()),
				short s => new NumericLiteral(s, s.ToString()),
				ushort us => new NumericLiteral(us, us.ToString()),
				int i => new NumericLiteral(i, i.ToString()),
				uint ui => new NumericLiteral(ui, ui.ToString()),
				long l => new NumericLiteral(l, l.ToString()),
				ulong ul => new NumericLiteral(ul, ul.ToString()),
				double d => new NumericLiteral(d, d.ToString()),
				float f => new NumericLiteral(f, f.ToString()),
				decimal dec => new NumericLiteral(System.Convert.ToDouble(dec), dec.ToString()),
				_ => null
			},
			IdentifierNameSyntax id => new Identifier(id.Identifier.Text),
			DefaultExpressionSyntax _ => Null,

			ParenthesizedExpressionSyntax pe => ConvertFromSyntaxNode(pe.Expression),

			// 调用、创建与访问
			InvocationExpressionSyntax ie => new CallExpression(
				(Expression)ConvertFromSyntaxNode(ie.Expression),
				NodeList.From(ie.ArgumentList.Arguments.Select(a => (Expression)ConvertFromSyntaxNode(a.Expression))),
				optional: false),

			ObjectCreationExpressionSyntax oc => new NewExpression(
				(Expression)ConvertFromSyntaxNode(oc.Type),
				NodeList.From(oc.ArgumentList?.Arguments.Select(a => (Expression)ConvertFromSyntaxNode(a.Expression)) ?? [])),

			MemberAccessExpressionSyntax ma => new MemberExpression(
				(Expression)ConvertFromSyntaxNode(ma.Expression),
				new Identifier(ma.Name.Identifier.Text),
				computed: false,
				optional: false),

			ConditionalAccessExpressionSyntax ca => new ConditionalExpression(
				new LogicalExpression(
					Operator.LogicalAnd,
					(Expression)ConvertFromSyntaxNode(ca.Expression),
					new LogicalExpression(
						Operator.StrictInequality,
						(Expression)ConvertFromSyntaxNode(ca.Expression),
						Null
					)
				),
				(Expression)ConvertFromSyntaxNode(ca.WhenNotNull),
				Undefined),

			ElementAccessExpressionSyntax ea => new MemberExpression(
				(Expression)ConvertFromSyntaxNode(ea.Expression),
				ea.ArgumentList.Arguments.Count > 0
				? (Expression)ConvertFromSyntaxNode(ea.ArgumentList.Arguments[0].Expression)
				: Undefined,
				computed: true,
				optional: false),

			// 赋值与运算符
			AssignmentExpressionSyntax ae => new AssignmentExpression(
				Operator.Assignment,
				(Expression)ConvertFromSyntaxNode(ae.Left),
				(Expression)ConvertFromSyntaxNode(ae.Right)),

			ConditionalExpressionSyntax ce => new ConditionalExpression(
				(Expression)ConvertFromSyntaxNode(ce.Condition),
				(Expression)ConvertFromSyntaxNode(ce.WhenTrue),
				(Expression)ConvertFromSyntaxNode(ce.WhenFalse)),

			BinaryExpressionSyntax be => be.OperatorToken.Kind() switch
			{
				SyntaxKind.PlusToken => new NonLogicalBinaryExpression(Operator.Addition, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
				SyntaxKind.MinusToken => new NonLogicalBinaryExpression(Operator.Subtraction, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
				SyntaxKind.AsteriskToken => new NonLogicalBinaryExpression(Operator.Multiplication, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
				SyntaxKind.SlashToken => new NonLogicalBinaryExpression(Operator.Division, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
				SyntaxKind.EqualsEqualsToken => new NonLogicalBinaryExpression(Operator.Equality, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
				SyntaxKind.ExclamationEqualsToken => new NonLogicalBinaryExpression(Operator.Inequality, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
				SyntaxKind.GreaterThanToken => new NonLogicalBinaryExpression(Operator.GreaterThan, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
				SyntaxKind.LessThanToken => new NonLogicalBinaryExpression(Operator.LessThan, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
				SyntaxKind.AmpersandAmpersandToken => new LogicalExpression(Operator.LogicalAnd, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
				SyntaxKind.BarBarToken => new LogicalExpression(Operator.LogicalOr, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
				_ => null
			},

			PrefixUnaryExpressionSyntax pu => pu.OperatorToken.Kind() switch
			{
				SyntaxKind.MinusToken => new NonUpdateUnaryExpression(Operator.UnaryNegation, (Expression)ConvertFromSyntaxNode(pu.Operand)),
				SyntaxKind.PlusPlusToken => new UpdateExpression(Operator.Increment, (Expression)ConvertFromSyntaxNode(pu.Operand), prefix: true),
				SyntaxKind.MinusMinusToken => new UpdateExpression(Operator.Decrement, (Expression)ConvertFromSyntaxNode(pu.Operand), prefix: true),
				SyntaxKind.ExclamationToken => new NonUpdateUnaryExpression(Operator.LogicalNot, (Expression)ConvertFromSyntaxNode(pu.Operand)),
				SyntaxKind.PlusToken => new NonUpdateUnaryExpression(Operator.UnaryPlus, (Expression)ConvertFromSyntaxNode(pu.Operand)),
				SyntaxKind.TildeToken => new NonUpdateUnaryExpression(Operator.BitwiseNot, (Expression)ConvertFromSyntaxNode(pu.Operand)),
				_ => null
			},
			PostfixUnaryExpressionSyntax po when po.OperatorToken.IsKind(SyntaxKind.PlusPlusToken) || po.OperatorToken.IsKind(SyntaxKind.MinusMinusToken) =>
				new UpdateExpression(
					po.OperatorToken.IsKind(SyntaxKind.PlusPlusToken) ? Operator.Increment : Operator.Decrement,
					(Expression)ConvertFromSyntaxNode(po.Operand),
					prefix: false),

			CastExpressionSyntax cs => (Expression)ConvertFromSyntaxNode(cs.Expression),

			AwaitExpressionSyntax aw => new AwaitExpression((Expression)ConvertFromSyntaxNode(aw.Expression)),

			TupleExpressionSyntax te => new SequenceExpression(NodeList.From(te.Arguments.Select(a => (Expression)ConvertFromSyntaxNode(a.Expression)))),

			ExpressionStatementSyntax es => new NonSpecialExpressionStatement((Expression)ConvertFromSyntaxNode(es.Expression)),

			_ => null
		};

		return result ?? HandleTransformationFailure(node, $"Unsupported syntax node kind: {node.Kind()}.");
	}
}
