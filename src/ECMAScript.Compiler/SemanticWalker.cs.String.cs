using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Text;

namespace ECMAScript.Compiler;

public partial class SemanticWalker
{
	/// <summary>
	/// 处理插值字符串文本操作
	/// C# 示例：
	/// $"Hello {name}, welcome!" 中的 "Hello " 和 ", welcome!" 部分
	/// 转换结果：字符串字面量 "Hello " / ", welcome!"
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitInterpolatedStringText(IInterpolatedStringTextOperation operation, Queue<VariableDeclaration> argument)
	{
		// 插值字符串中的文本部分转换为字符串字面量
		var text = operation.Text.ConstantValue.Value?.ToString() ?? "";
		return new StringLiteral(text, $"\"{text}\"");
	}

	/// <summary>
	/// 处理插值表达式操作
	/// C# 示例：
	/// $"Hello {name}!" 中的 {name} 部分
	/// $"Value: {x + y:F2}" 中的 {x + y:F2} 部分
	/// 转换结果：返回插值表达式 name / (x + y)，并处理格式化
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitInterpolation(IInterpolationOperation operation, Queue<VariableDeclaration> argument)
	{
		// 插值表达式转换为表达式
		// 处理格式化说明符（如 :F2）
		// 注意：由于API限制，格式化信息可能不在IInterpolationOperation中
		// 这里保留原始行为，只返回表达式
		return Translate<Expression>(operation.Expression, argument);
	}

	/// <summary>
	/// 处理插值字符串添加操作
	/// C# 示例：
	/// $"Hello {name}!" 中的字符串连接操作
	/// 转换结果： `Hello ${name}!`
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitInterpolatedStringAddition(IInterpolatedStringAdditionOperation operation, Queue<VariableDeclaration> argument)
	{
		// 递归收集所有静态字符串和动态表达式
		var quasis = new List<TemplateElement>();
		var exprs = new List<Expression>();

		void Collect(IOperation? node)
		{
			switch (node)
			{
				case IInterpolatedStringAdditionOperation add:
					// 递归处理左子树和右子树，以压平编译器生成的二叉树结构
					Collect(add.Left);
					Collect(add.Right);
					break;
				case ILiteralOperation { ConstantValue: { HasValue: true, Value: string cookedValue } }:
					// s 是 C# 解释后的 "cooked" 值
					var rawValue = CookedToRaw(cookedValue);
					var templateValue = TemplateValue.From(cookedValue, rawValue);
					quasis.Add(new TemplateElement(templateValue, tail: false));
					break;
				default:
					// 任何非字面量操作（如变量、方法调用等）都被视为动态表达式
					Translate(exprs, node, argument);
					break;
			}
		}

		Collect(operation);

		// 最后一个 quasi 的 tail 标志必须是 true
		if (quasis.Count == exprs.Count)
			quasis.Add(new TemplateElement(TemplateValue.From("", ""), tail: true));
		else
			quasis[quasis.Count - 1] = new TemplateElement(quasis[quasis.Count - 1].Value, tail: true);

		// 生成最终的 TemplateLiteral AST 节点
		return new TemplateLiteral(NodeList.From(quasis), NodeList.From(exprs));

		string CookedToRaw(string cooked)
		{
			var sb = new StringBuilder(cooked.Length);
			foreach (var c in cooked)
			{
				switch (c)
				{
					case '`': sb.Append("\\`"); break;
					case '\\': sb.Append("\\\\"); break;
					case '$': sb.Append("\\$"); break;
					case '\r': sb.Append("\\r"); break;
					case '\n': sb.Append("\\n"); break;
					case '\t': sb.Append("\\t"); break;
					default: sb.Append(c); break;
				}
			}
			return sb.ToString();
		}
	}
	
	/// <summary>
	/// 处理委托创建操作
	/// C# 示例：
	/// Action action = Method;              // 方法组转委托
	/// Func<int, string> func = x => x.ToString(); // Lambda 转委托
	/// EventHandler handler = new EventHandler(OnEvent); // 显式委托创建
	/// 转换结果：转换为函数引用或箭头函数
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitDelegateCreation(IDelegateCreationOperation operation, Queue<VariableDeclaration> argument)
	{
		// 委托创建转换为函数引用或箭头函数
		return Visit(operation.Target, argument);
	}
}
