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
		var right = BuildForEachLoopCollection(operation, argument);
		var body = Translate<Statement>(operation.Body, argument);

		return new ForOfStatement(left, right, body, @await: operation.IsAsynchronous);
	}

	private Expression BuildForEachLoopCollection(IForEachLoopOperation operation, SenseArgument argument)
	{
		var collection = Translate<Expression>(operation.Collection, argument);
		if (operation.Collection.Type is not INamedTypeSymbol
			{ SpecialType: SpecialType.System_String } stringType)
		{
			return collection;
		}

		// JavaScript `for...of` iterates Unicode code points, while C# foreach over string yields
		// UTF-16 char units. Reuse the CLR ToCharArray mapping instead of encoding split semantics here.
		var toCharArray = stringType.GetMembers(nameof(string.ToCharArray))
			.OfType<IMethodSymbol>()
			.Single(static method => !method.IsStatic && method.Parameters.Length == 0);
		return BuildMethodCallExpression(
			operation.Collection,
			toCharArray,
			operation.Collection.Syntax,
			semanticModel: null,
			collection,
			[],
			argument,
			hostType: stringType);
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

		var elementType = (INamedTypeSymbol)GetForEachElementType(operation);
		if (CanLowerForEachDeconstructionSource(elementType))
		{
			var pattern = BuildForEachDeconstructionPattern(targetTuple, elementType, argument);
			return CreateForEachLoopBinding(pattern);
		}

		var elementDisplayName = elementType.ToDisplayString(Jazor.Common.Format.NameFormat);
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

	private static ITypeSymbol GetForEachElementType(IForEachLoopOperation operation)
		=> operation.SemanticModel!
			.GetForEachStatementInfo((CommonForEachStatementSyntax)operation.Syntax)
			.ElementType!;

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

			// Roslyn binds the deconstruction arity before this structural record path is selected.
			// Every target slot therefore has a corresponding tuple/record runtime property.
			GetForEachSourceSlot(sourceType, index, out var propertyName, out var propertyType);

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

	private void GetForEachSourceSlot(
		INamedTypeSymbol sourceType,
		int index,
		out string propertyName,
		out ITypeSymbol propertyType)
	{
		if (sourceType.IsTupleType)
		{
			var field = sourceType.TupleElements[index];
			propertyName = GetTupleRuntimeFieldName(field);
			propertyType = field.Type;
			return;
		}

		// The caller admits this route only after CanLowerForEachDeconstructionSource() has
		// established a structural source. Roslyn has already validated the deconstruction arity.
		_ = TryGetStructuralRuntimeProperty(sourceType, index, out propertyName, out propertyType);
	}

	private static Node CreateForEachLoopBinding(Node loopControl)
	{
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
			test = Translate<Expression>(operation.Condition, argument);
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

				// This branch starts with more than one AtLoopBottom operation and every bound
				// operation yields one expression, so the sequence always has at least two items.
				updateExpression = expressions[0];
				for (int i = 1; i < expressions.Count; i++)
				{
					updateExpression = new SequenceExpression(
						NodeList.From([updateExpression, expressions[i]])
					);
				}
			}
		}

		var body = Translate<Statement>(operation.Body, argument);
		if (init is VariableDeclaration declaration && HasCapturedForControlVariable(operation))
		{
			// C# for declares one control-variable binding for the whole loop. JavaScript `let` in a
			// for initializer instead creates a fresh binding for each iteration, so callbacks would
			// observe different values. Keep the declaration in an equivalent lexical block and leave
			// the JS for initializer empty whenever Roslyn proves a nested function captures it.
			return new NestedBlockStatement(NodeList.From<Statement>(
				declaration,
				new ForStatement(null, test, updateExpression, body)));
		}

		return new ForStatement(init, test, updateExpression, body);
	}

	private static bool HasCapturedForControlVariable(IForLoopOperation operation)
	{
		foreach (var localReference in operation.Descendants().OfType<ILocalReferenceOperation>())
		{
			if (!operation.Locals.Any(local => SymbolEqualityComparer.Default.Equals(local, localReference.Local)))
				continue;

			for (IOperation? ancestor = localReference.Parent;
				 ancestor is not null && !ReferenceEquals(ancestor, operation);
				 ancestor = ancestor.Parent)
			{
				if (ancestor is IAnonymousFunctionOperation or ILocalFunctionOperation)
					return true;
			}
		}

		return false;
	}

	private Expression TranslateForLoopUpdateExpression(IOperation operation, SenseArgument argument)
		// IForLoopOperation.AtLoopBottom contains expression statements in valid C# input.
		=> Translate<NonSpecialExpressionStatement>(operation, argument).Expression;

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
		var test = Translate<Expression>(operation.Condition!, argument);
		var body = Translate<Statement>(operation.Body, argument);

		// ConditionIsTop: true = while (条件在顶部), false = do-while (条件在底部)
		if (!operation.ConditionIsTop)
			return new DoWhileStatement(body, test);
		else
			return new WhileStatement(test, body);
	}

}
