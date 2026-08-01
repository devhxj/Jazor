using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

/// <summary>
/// 处理 for、foreach、while、do 等循环 operation 的 lowering。
/// </summary>
/// <remarks>
/// 循环转换必须保留初始化、条件、迭代和 body 的执行顺序，并正确处理 continue/break 的
/// 控制流目标。集合遍历只使用当前宿主已声明的可调用协议，不假设所有对象都天然可迭代。
/// </remarks>
public partial class SemanticWalker
{
	/// <summary>
	/// 处理 foreach 循环操作
	/// C# 示例：
	/// foreach (var item in collection) {
	///     Console.WriteLine(item);
	/// }
	/// 转换结果：for (let item of collection) { console.log(item); }
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitForEachLoop(IForEachLoopOperation operation, SenseArgument argument)
	{
		var left = BuildForEachLoopBinding(operation, argument);
		var right = Translate<Expression>(operation.Collection, argument);
		var body = Translate<Statement>(operation.Body, argument);

		return new ForOfStatement(left, right, body, @await: operation.IsAsynchronous);
	}

	/// <summary>
	/// Builds the ESTree binding for a Roslyn foreach loop without lowering its collection or body.
	/// Product hosts can reuse this when they own the surrounding loop artifact but must retain
	/// the compiler's structural tuple, KeyValuePair, and record deconstruction contract.
	/// </summary>
	public Node BuildForEachLoopBinding(IForEachLoopOperation operation, SenseArgument argument)
	{
		var targetTuple = GetForEachTargetTuple(operation.LoopControlVariable);
		if (targetTuple is null)
			return CreateForEachLoopBinding(Translate<Node>(operation.LoopControlVariable, argument));

		var elementType = GetForEachElementType(operation);
		if (elementType is INamedTypeSymbol namedElementType &&
			CanLowerForEachDeconstructionSource(namedElementType))
		{
			var pattern = BuildForEachDeconstructionPattern(targetTuple, namedElementType, argument);
			return CreateForEachLoopBinding(pattern);
		}

		var elementDisplayName = elementType?.ToDisplayString(Jazor.Common.Format.NameFormat) ?? "<unknown>";
		return HandleTransformationFailure<Node>(
			operation,
			$"For-each deconstruction source type '{elementDisplayName}' does not have a compiler-known structural runtime shape. " +
			"Use an ordinary loop variable and deconstruct it inside the loop body.");
	}

	private static ITupleOperation? GetForEachTargetTuple(IOperation loopControlVariable)
		=> loopControlVariable switch
		{
			IDeclarationExpressionOperation { Expression: ITupleOperation tuple } => tuple,
			ITupleOperation tuple => tuple,
			_ => null
		};

	private static ITypeSymbol? GetForEachElementType(IForEachLoopOperation operation)
	{
		if (operation.SemanticModel is null ||
			operation.Syntax is not CommonForEachStatementSyntax syntax)
		{
			return null;
		}

		return operation.SemanticModel.GetForEachStatementInfo(syntax).ElementType;
	}

	private Node BuildForEachDeconstructionPattern(
		ITupleOperation targetTuple,
		INamedTypeSymbol sourceType,
		SenseArgument argument)
		=> IsKeyValuePairType(sourceType)
			? BuildForEachKeyValuePairPattern(targetTuple, sourceType, argument)
			: BuildForEachObjectPattern(targetTuple, sourceType, argument);

	private ObjectPattern BuildForEachObjectPattern(
		ITupleOperation targetTuple,
		INamedTypeSymbol sourceType,
		SenseArgument argument)
	{
		var properties = new List<Node>();
		for (var index = 0; index < targetTuple.Elements.Length; index++)
		{
			var targetElement = targetTuple.Elements[index];
			if (targetElement is IDiscardOperation)
				continue;

			if (!TryGetForEachSourceSlot(sourceType, index, out var propertyName, out var propertyType))
			{
				return HandleTransformationFailure<ObjectPattern>(
					targetTuple,
					$"For-each deconstruction source type '{sourceType.ToDisplayString(Jazor.Common.Format.NameFormat)}' " +
					$"does not expose structural slot {index}.");
			}

			var value = BuildForEachDeconstructionValue(
				targetElement,
				propertyType,
				sourceType,
				index,
				argument);

			properties.Add(new AssignmentProperty(
				key: CreateObjectPropertyKey(propertyName),
				value: value,
				computed: false,
				shorthand: false));
		}

		return WithOrigin(new ObjectPattern(NodeList.From(properties)), targetTuple);
	}

	private ArrayPattern BuildForEachKeyValuePairPattern(
		ITupleOperation targetTuple,
		INamedTypeSymbol sourceType,
		SenseArgument argument)
	{
		if (targetTuple.Elements.Length != sourceType.TypeArguments.Length)
		{
			return HandleTransformationFailure<ArrayPattern>(
				targetTuple,
				$"For-each KeyValuePair deconstruction requires {sourceType.TypeArguments.Length} target slots, " +
				$"but found {targetTuple.Elements.Length}.");
		}

		var elements = new List<Node?>();
		for (var index = 0; index < targetTuple.Elements.Length; index++)
		{
			var targetElement = targetTuple.Elements[index];
			if (targetElement is IDiscardOperation)
			{
				elements.Add(null);
				continue;
			}

			elements.Add(BuildForEachDeconstructionValue(
				targetElement,
				sourceType.TypeArguments[index],
				sourceType,
				index,
				argument));
		}

		return WithOrigin(new ArrayPattern(NodeList.From(elements)), targetTuple);
	}

	private Node BuildForEachDeconstructionValue(
		IOperation targetElement,
		ITypeSymbol sourceSlotType,
		INamedTypeSymbol sourceType,
		int index,
		SenseArgument argument)
	{
		var nestedTarget = GetForEachTargetTuple(targetElement);
		if (nestedTarget is null)
			return Translate<Expression>(targetElement, argument);

		if (sourceSlotType is INamedTypeSymbol nestedSourceType &&
			CanLowerForEachDeconstructionSource(nestedSourceType))
		{
			return BuildForEachDeconstructionPattern(nestedTarget, nestedSourceType, argument);
		}

		return HandleTransformationFailure<Node>(
			targetElement,
			$"Nested for-each deconstruction slot {index} on source type " +
			$"'{sourceType.ToDisplayString(Jazor.Common.Format.NameFormat)}' does not have a structural runtime shape.");
	}

	private bool CanLowerForEachDeconstructionSource(INamedTypeSymbol sourceType)
		=> sourceType.IsTupleType ||
		   IsKeyValuePairType(sourceType) ||
		   ShouldLowerStructurally(sourceType);

	private static bool IsKeyValuePairType(INamedTypeSymbol sourceType)
		=> sourceType.OriginalDefinition.MetadataName == "KeyValuePair`2" &&
		   sourceType.OriginalDefinition.ContainingNamespace.ToDisplayString() == "System.Collections.Generic";

	private bool TryGetForEachSourceSlot(
		INamedTypeSymbol sourceType,
		int index,
		out string propertyName,
		out ITypeSymbol propertyType)
	{
		if (sourceType.IsTupleType && index < sourceType.TupleElements.Length)
		{
			var field = sourceType.TupleElements[index];
			propertyName = GetTupleRuntimeFieldName(field);
			propertyType = field.Type;
			return true;
		}

		if (ShouldLowerStructurally(sourceType) &&
			TryGetStructuralRuntimeProperty(sourceType, index, out propertyName, out propertyType))
		{
			return true;
		}

		propertyName = null!;
		propertyType = null!;
		return false;
	}

	private static Node CreateForEachLoopBinding(Node loopControl)
	{
		if (loopControl is VariableDeclaration)
			return loopControl;

		var declarator = loopControl as VariableDeclarator
			?? new VariableDeclarator(loopControl, null);

		return new VariableDeclaration(
			VariableDeclarationKind.Let,
			NodeList.From([declarator]));
	}

	/// <summary>
	/// 处理 for 循环操作
	/// C# 示例：
	/// for (int i = 0; i < 10; i++) {
	///     Console.WriteLine(i);
	/// }
	/// 转换结果：for (let i = 0; i < 10; i++) { console.log(i); }
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitForLoop(IForLoopOperation operation, SenseArgument argument)
	{
		StatementOrExpression? init = CreateForLoopInitializer(operation.Before, argument);
		Expression? test = null;

		if (operation.Condition is not null)
		{
			test = TranslateExpression(operation.Condition, argument);
		}

		// 处理多个 AtLoopBottom 操作的情况
		// IForLoopOperation.AtLoopBottom 出现“多个”运算并不代表 C# 支持写多个迭代表达式，
		// 而是 Roslyn 在 lowering 阶段把源码里的一个迭代表达式拆成多条中间指令。
		// 常见场景：
		// 1. 复合赋值  i += x + y  →  先算临时变量，再执行加法赋值
		// 2. 方法调用  M(out var tmp)  →  调用 + 丢弃返回值
		// 3. 异步/迭代器状态机生成
		// 遍历列表时按顺序依次输出即可，源代码层面仍只有一段“迭代表达式”。        
		Expression? updateExpression = null;
		if (operation.AtLoopBottom.Length > 0)
		{
			// 如果只有一个操作，直接使用
			if (operation.AtLoopBottom.Length == 1)
			{
				updateExpression = TranslateForLoopUpdateExpression(operation.AtLoopBottom[0], argument);
			}
			else
			{
				// 如果有多个操作，将它们组合成一个逗号表达式
				var expressions = new List<Expression>();
				foreach (var atLoopBottomOp in operation.AtLoopBottom)
				{
					var expr = TranslateForLoopUpdateExpression(atLoopBottomOp, argument);
					expressions.Add(expr);
				}

				// 如果只有一个有效表达式，直接使用
				if (expressions.Count == 1)
					updateExpression = expressions[0];

				// 如果有多个有效表达式，使用逗号表达式组合
				else if (expressions.Count > 1)
				{
					updateExpression = expressions[0];
					for (int i = 1; i < expressions.Count; i++)
					{
						updateExpression = new SequenceExpression(
							NodeList.From([updateExpression, expressions[i]])
						);
					}
				}
			}
		}

		var body = Translate<Statement>(operation.Body, argument);
		return new ForStatement(init, test, updateExpression, body);
	}

	private Expression TranslateForLoopUpdateExpression(IOperation operation, SenseArgument argument)
	{
		var node = Visit(operation, argument);
		return node switch
		{
			Expression expression => expression,
			NonSpecialExpressionStatement statement => statement.Expression,
			_ => HandleTransformationFailure<Expression>(
				operation,
				"For loop update expression could not be translated to JavaScript.")
		};
	}

	private StatementOrExpression? CreateForLoopInitializer(IEnumerable<IOperation> beforeOperations, SenseArgument argument)
	{
		var declarations = new List<VariableDeclarator>();
		var expressions = new List<Expression>();

		foreach (var before in beforeOperations)
		{
			var node = Visit(before, argument);
			switch (node)
			{
				case VariableDeclaration declaration:
					if (declaration.Declarations.Count > 0)
						declarations.AddRange(declaration.Declarations);
					break;

				case NonSpecialExpressionStatement statement:
					expressions.Add(statement.Expression);
					break;

				default:
					return HandleTransformationFailure<StatementOrExpression>(
						before,
						"For loop initializer could not be translated to JavaScript.");
			}
		}

		if (declarations.Count > 0)
			return new VariableDeclaration(VariableDeclarationKind.Let, NodeList.From(declarations));

		if (expressions.Count == 0)
			return null;

		if (expressions.Count == 1)
			return expressions[0];

		return new SequenceExpression(NodeList.From(expressions));
	}

	/// <summary>
	/// 处理 while 和 do-while 循环操作
	/// C# 示例：
	/// while (condition) { ... }        → while (condition) { ... }
	/// do { ... } while (condition);    → do { ... } while (condition);
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitWhileLoop(IWhileLoopOperation operation, SenseArgument argument)
	{
		if (operation.Condition is null)
			return null;

		var test = Translate<Expression>(operation.Condition, argument);
		var body = Translate<Statement>(operation.Body, argument);

		// ConditionIsTop: true = while (条件在顶部), false = do-while (条件在底部)
		if (!operation.ConditionIsTop)
			return new DoWhileStatement(body, test);
		else
			return new WhileStatement(test, body);
	}

}
