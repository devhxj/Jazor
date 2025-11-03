using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;
using OneOf;

namespace ECMAScript.Compiler;

public partial class SemanticWalker
{
	/// <summary>
	/// 处理元组操作
	/// C# 示例：
	/// (1, "hello", true)          // 元组字面量
	/// var tuple = (x, y);         // 元组创建
	/// (double Sum, int Count) t2 = (4.5, 3);// 命名元组创建
	/// 转换结果：{ Item1: 1, Item2: "hello", Item3: true } 或 { Sum: 4.5, Count: 3 } （使用对象模拟）
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitTuple(ITupleOperation operation, Queue<VariableDeclaration> argument)
	{
		var elements = new List<Expression?>();
		var tupleType = (INamedTypeSymbol)operation.NaturalType!;
		for (var index = 0; index < operation.Elements.Length; index++)
		{
			var fieldName = tupleType.TupleElements[index].Name;
			var element = operation.Elements[index];
			var key = new StringLiteral(fieldName, $"'{fieldName}'");
			var value = Translate<Expression>(element, argument);
			var array = new ArrayExpression(NodeList.From<Expression?>(key, value));
			elements.Add(array);
		}

		var obj = new Identifier("Tuple");
		var prop = new Identifier("Create");
		var func = new MemberExpression(obj, prop, false, false);
		var args = new ArrayExpression(NodeList.From(elements));
		var call = new CallExpression(func, NodeList.From<Expression>(args), false);
		return call;
	}

	/// <summary>
	/// 处理解构赋值操作
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitDeconstructionAssignment(IDeconstructionAssignmentOperation operation, Queue<VariableDeclaration> argument)
	{
		// C# 示例：
		// var tuple = (aaa:1,2);
		// (int bbb, int ccc) = tuple;
		// int ddd,eee;
		// (ddd, eee) = tuple;
		// int kkk;
		// (kkk,int qqq) = tuple;
		// (int fff, (int ggg,int hhh)) = (2,tuple);
		// var func = (int x,int y)=>(mmm:x,y);
		// (int zzz,int yyy)= func(2,5);
		// 转换结果：
		// let tuple = {
		//     aaa: 1,
		//     Item2: 2
		// };
		// let bbb = tuple.aaa;
		// let ccc = tuple.Item2;
		// let ddd,eee;
		// ddd = tuple.aaa;
		// eee = tuple.Item2;
		// let kkk;
		// kkk = tuple.aaa;
		// let qqq = tuple.Item2;
		// let fff = 2;
		// let ggg = tuple.aaa;
		// let hhh = tuple.Item2;
		// let func = (x,y)=>{
		//     mmm: x,
		//     Item2: y
		// };
		// const temp = func(2,5);
		// let zzz = temp.aaa;
		// let yyy = temp.Item2;		
		var statements = new List<Statement>();
		Deconstruct(operation.Target, operation.Value.Type!, operation.Value, statements);
		return new StatementGroup(NodeList.From(statements));

		void Deconstruct(IOperation target, ITypeSymbol valueType, object value, List<Statement> states)
		{
			if (valueType.IsTupleType && target is ITupleOperation or IDeclarationExpressionOperation)
			{
				ITupleOperation tupleTarget;
				bool isDeclarationExpr = false;
				if (target is IDeclarationExpressionOperation declarationExpressionOp)
				{
					isDeclarationExpr = true;
					tupleTarget = (ITupleOperation)declarationExpressionOp.Expression;
				}
				else
					tupleTarget = (ITupleOperation)target;

				Expression? idExpr = null;
				if (value is IInvocationOperation invocation)
				{
					// 如果是方法调用，先造一个临时对象存放方法的值
					idExpr = new Identifier(GetUniqueName(invocation));
					var init = Translate<Expression>(invocation, argument);
					var declarator = new VariableDeclarator(idExpr, init);
					var declaration = new VariableDeclaration(VariableDeclarationKind.Const,
						NodeList.From(declarator));
					states.Add(declaration);

				}

				// 如果解构元素是元组类型，递归解构
				for (var index = 0; index < tupleTarget.Elements.Length; index++)
				{
					var element = tupleTarget.Elements[index];
					var field = ((INamedTypeSymbol)valueType).TupleElements[index];
					if (element is IDiscardOperation)
					{
						continue;
					}
					else if (isDeclarationExpr || element is IDeclarationExpressionOperation)
					{
						Expression init;
						if (value is ILocalReferenceOperation localRef)
						{
							var obj = new Identifier(localRef.Local.Name);
							var prop = new Identifier(field.Name);
							init = new MemberExpression(obj, prop, false, false);
						}
						else if (value is IConversionOperation conversion && conversion.Operand is ITupleOperation conversionTuple)
						{
							init = Translate<Expression>(conversionTuple.Elements[index], argument);
						}
						else if (value is IInvocationOperation)
						{
							var prop = new Identifier(field.Name);
							init = new MemberExpression(idExpr!, prop, false, false);
						}
						else if (value is IOperation op)
						{
							init = Translate<Expression>(op, argument);
						}
						else if (value is Expression expr)
						{
							var prop = new Identifier(field.Name);
							init = new MemberExpression(expr, prop, false, false);
						}
						else
						{
							HandleTransformationFailure(target, $"The {target.Kind} operation is not supported in DeconstructionAssignment.");
							return;
						}

						var id = Translate<Node>(element, argument);
						var declarator = new VariableDeclarator(id, init);
						var declaration = new VariableDeclaration(VariableDeclarationKind.Let,
							NodeList.From(declarator));
						states.Add(declaration);
					}
					else if (!isDeclarationExpr && element is ILocalReferenceOperation localRef)
					{
						Expression right;
						if (value is ILocalReferenceOperation valueLocalRef)
						{
							var obj = new Identifier(valueLocalRef.Local.Name);
							var prop = new Identifier(field.Name);
							right = new MemberExpression(obj, prop, false, false);
						}
						else if (value is IInvocationOperation)
						{
							var prop = new Identifier(field.Name);
							right = new MemberExpression(idExpr!, prop, false, false);
						}
						else if (value is IOperation op)
						{
							right = Translate<Expression>(op, argument);
						}
						else if (value is Expression exprr)
						{
							var prop = new Identifier(field.Name);
							right = new MemberExpression(exprr, prop, false, false);
						}
						else
						{
							HandleTransformationFailure(target, $"The {target.Kind} operation is not supported in DeconstructionAssignment.");
							return;
						}

						var left = Translate<Node>(localRef, argument);
						var expr = new AssignmentExpression(Operator.Assignment, left, right);
						var state = new NonSpecialExpressionStatement(expr);
						states.Add(state);
					}
					else if (field.Type.IsTupleType)
					{
						if (value is IConversionOperation conversion && conversion.Operand is ITupleOperation conversionTuple)
						{
							var subValue = conversionTuple.Elements[index];
							Deconstruct(element, subValue.Type!, subValue, states);
						}
						else if (value is ILocalReferenceOperation l1)
						{
							var obj = new Identifier(l1.Local.Name);
							var prop = new Identifier(field.Name);
							var subValue = new MemberExpression(obj, prop, false, false);
							Deconstruct(element, field.Type, subValue, states);
						}
						else if (value is Expression p)
						{
							var prop = new Identifier(field.Name);
							var subValue = new MemberExpression(p, prop, false, false);
							Deconstruct(element, field.Type, subValue, states);
						}
					}
					else
					{
						HandleTransformationFailure(element, $"The {element.Kind} operation is not supported in DeconstructionAssignment.");
						return;
					}
				}
			}
			else if (valueType.TypeKind == TypeKind.Class && value is IOperation expr)
			{
				//自定义解构
				ITupleOperation tupleResult;
				bool isDeclarationExpressionTarget = false;
				if (target is IDeclarationExpressionOperation declarationExpr && declarationExpr.Expression is ITupleOperation t1)
				{
					tupleResult = t1;
					isDeclarationExpressionTarget = true;
				}
				else if (target is ITupleOperation t2)
					tupleResult = t2;
				else
				{
					HandleTransformationFailure(target, $"The {target.Kind} operation is not supported in DeconstructionAssignment.");
					return;
				}

				List<VariableDeclarator> declarators = [];
				List<Expression> args = [];
				List<(int Index, Identifier Id)> nestedRefs = [];
				var tupleType = (INamedTypeSymbol)tupleResult.Type!;
				for (var index = 0; index < tupleResult.Elements.Length; index++)
				{
					var element = tupleResult.Elements[index];
					if (element is ILocalReferenceOperation localRef && isDeclarationExpressionTarget)
					{
						var name = localRef.Local.Name;
						var id = new Identifier(name);
						var declarator = new VariableDeclarator(id, null);

						declarators.Add(declarator);
						args.Add(id);
					}
					else if (element is ITupleOperation subTuple)
					{
						// 如果是一个元组，需要创建一个临时变量，被自定义Deconstruct方法调用后
						// 再解构出元组里面变量定义或引用
						var name = GetUniqueName(subTuple);
						var id = new Identifier(name);
						var declarator = new VariableDeclarator(id, null);

						declarators.Add(declarator);
						args.Add(id);
						nestedRefs.Add((index, id));
					}
					else
					{
						var name = tupleType.TupleElements[index].Name;
						args.Add(new Identifier(name));
					}
				}

				// 处理变量定义
				if (declarators.Count > 0)
				{
					var declaration = new VariableDeclaration(VariableDeclarationKind.Let,
						NodeList.From(declarators));
					states.Add(declaration);
				}

				// 执行 Deconstruct方法
				var obj = Translate<Expression>(expr, argument);
				var prop = new Identifier("Deconstruct");
				var func = new MemberExpression(obj, prop, false, false);
				var call = new CallExpression(func, NodeList.From(args), false);
				states.Add(new NonSpecialExpressionStatement(call));

				IMethodSymbol method;
				// 处理嵌套元组中的解构参数
				if (expr is IInvocationOperation invocation)
				{
					method = invocation.TargetMethod;
				}
				else
				{
					method = (IMethodSymbol)valueType
						.GetMembers()
						.First(x => x.Kind == SymbolKind.Method && x.Name == "Deconstruct");
				}

				foreach (var (index, id) in nestedRefs)
				{
					var parameter = method.Parameters[index];
					var element = tupleResult.Elements[index];
					Deconstruct(element, parameter.Type, id, statements);
				}
			}
			else
			{
				HandleTransformationFailure(target, $"The {target.Kind} operation is not supported in DeconstructionAssignment.");
			}
		}
	}

	/// <summary>
	/// 处理元组二元操作符操作
	/// C# 示例：
	/// (a, b) == (c, d)                    // 元组相等比较
	/// (x, y) != (1, 2)                    // 元组不等比较
	/// (name, age) == ("John", 25)         // 元组与常量比较
	/// tuple1 == tuple2                    // 元组变量比较
	/// 转换结果：
	/// a==c&&b==d
	/// x!=1||y!=2
	/// name=="John"&&age==25
	/// tuple1.Item1 == tuple2.Item1&&tuple1.Item2 == tuple2.Item2
	/// </summary>
	/// <param name="operation">元组二元操作</param>
	/// <param name="argument">当前operation所属的父operation</param>
	/// <returns>JavaScript逻辑表达式</returns>
	public override Acornima.Ast.Node? VisitTupleBinaryOperator(ITupleBinaryOperation operation, Queue<VariableDeclaration> argument)
	{
		// C#本身语法限定左右都必须是同样元素类型和个数的元组
		// 所以此处不用考虑类型和个数不匹配
		Expression? result = null;
		// 递归访问左右操作元，获取它们的表达式。
		var isEq = operation.OperatorKind == BinaryOperatorKind.Equals;

		NestedBuilder(
			(operation.LeftOperand, operation.LeftOperand.Type!),
			(operation.RightOperand, operation.RightOperand.Type!),
			isEq);

		if (result is null)
			return HandleTransformationFailure(operation, "Tuple binary operation could not be translated to JavaScript.");

		return new ParenthesizedExpression(result);

		void NestedBuilder((object Target, ITypeSymbol Type) left, (object Target, ITypeSymbol Type) right, bool isEq)
		{
			Expression? leftExpr = null, rightExpr = null;
			ITupleOperation? tupleLeft = null, tupleRight = null;
			if (left.Target is IInvocationOperation leftInvocation)
			{
				leftExpr = new Identifier(GetUniqueName(leftInvocation));
				var init = Translate<Expression>(leftInvocation, argument);
				var declarator = new VariableDeclarator(leftExpr, init);
				var declaration = new VariableDeclaration(VariableDeclarationKind.Const,
					NodeList.From(declarator));
				argument.Enqueue(declaration);
			}
			else if (left.Target is ITupleOperation leftTuple)
				tupleLeft = leftTuple;

			else if (left.Target is IOperation leftOp)
				leftExpr = Translate<Expression>(leftOp, argument);

			else if (left.Target is Expression leftExp)
				leftExpr = leftExp;

			if (right.Target is IInvocationOperation rightInvocation)
			{
				rightExpr = new Identifier(GetUniqueName(rightInvocation));
				var init = Translate<Expression>(rightInvocation, argument);
				var declarator = new VariableDeclarator(rightExpr, init);
				var declaration = new VariableDeclaration(VariableDeclarationKind.Const,
					NodeList.From(declarator));
				argument.Enqueue(declaration);
			}
			else if (right.Target is ITupleOperation rightTuple)
				tupleRight = rightTuple;

			else if (right.Target is IOperation rightOp)
				rightExpr = Translate<Expression>(rightOp, argument);

			else if (right.Target is Expression rightExp)
				rightExpr = rightExp;

			var leftType = (INamedTypeSymbol)left.Type;
			var rightType = (INamedTypeSymbol)right.Type;
			for (var index = 0; index < leftType.TupleElements.Length; index++)
			{
				var leftField = leftType.TupleElements[index];
				var rightField = rightType.TupleElements[index];

				Expression? exprLeft = null, exprRight = null;
				if (tupleLeft is not null)
					exprLeft = Translate<Expression>(tupleLeft.Elements[index], argument);

				else if (leftExpr is not null)
					exprLeft = new MemberExpression(leftExpr, new Identifier(leftField.Name), false, false);

				if (tupleRight is not null)
					exprRight = Translate<Expression>(tupleRight.Elements[index], argument);

				else if (rightExpr is not null)
					exprRight = new MemberExpression(rightExpr, new Identifier(rightField.Name), false, false);

				if (leftField.Type.IsTupleType)
				{
					object? subLeft = tupleLeft is not null
						? tupleLeft.Elements[index]
						: exprLeft;

					object? subRight = tupleRight is not null
						? tupleRight.Elements[index]
						: exprRight;

					if (subLeft is null || subRight is null)
					{
						HandleTransformationFailure(operation, "Tuple binary operation could not be translated to JavaScript.");
						return;
					}

					NestedBuilder((subLeft, leftField.Type), (subRight, rightField.Type), isEq);
				}
				else
				{
					if (exprLeft is null || exprRight is null)
					{
						HandleTransformationFailure(operation, "Tuple binary operation could not be translated to JavaScript.");
						return;
					}

					var expr = new NonLogicalBinaryExpression(
						isEq ? Operator.StrictEquality : Operator.StrictInequality,
						exprLeft,
						exprRight);

					result = result is null
						? expr
						: new LogicalExpression(isEq ? Operator.LogicalAnd : Operator.LogicalOr, result, expr);
				}
			}
		}
	}

	/// <summary>
	/// 处理丢弃操作（下划线变量）
	/// C# 示例：
	/// _ = SomeMethod();        // 丢弃返回值
	/// (_, var y) = GetTuple(); // 丢弃元组的第一个元素
	/// 转换结果：undefined
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitDiscardOperation(IDiscardOperation operation, Queue<VariableDeclaration> argument)
	{
		// 在解构赋值模式中 (例如: (_, y) = tuple)
		if (operation.Parent is IDeconstructionAssignmentOperation)
		{
			// 在JavaScript的解构模式中，丢弃元素用空槽位表示。
			// 例如: const [, y] = tuple;
			// 但是，AST节点本身不能是“空的”。这里需要特殊处理。
			// 一个常见的策略是返回一个特殊的标记节点，然后在 VisitDeconstructionAssignment 中处理它。
			// 或者，更简单的方法是，让 VisitDeconstructionAssignment 自己处理丢弃操作，
			// 而不是让 VisitDiscardOperation 返回一个有意义的节点。
			// 如果必须返回一个节点，可以返回 null，并在父节点中处理。
			// 最简单直接的方式是返回一个代表“无”的节点，让父节点忽略它。
			// 这里我们返回一个特殊的Identifier，父节点需要识别它。
			// 但更好的做法是让父节点直接检查子操作是否为 IDiscardOperation。
			return null; // 让父节点处理
		}

		// 在简单赋值操作的左侧 (例如: _ = value)
		if (operation.Parent is ISimpleAssignmentOperation assignment && assignment.Target == operation)
		{
			// C# 的 _ = value 意味着“执行 value 但不关心结果”。
			// 这等价于直接执行 value 这个表达式语句。
			// 所以，我们应该返回 value 本身，而不是 undefined。
			// 父节点 VisitSimpleAssignmentOperation 应该检测到目标是丢弃操作，
			// 然后只返回对 operation.Value 的访问结果。
			return null; // 让父节点处理
		}

		// 在其他表达式中作为值使用 (非常罕见，但可能)
		// 例如: var isNull = _ is null; // 这在C#中是编译时错误
		// 或者作为方法参数: SomeMethod(_); // 这也是编译时错误
		// 如果真的遇到了，可以返回 undefined。
		return new Identifier("undefined");
	}
}
