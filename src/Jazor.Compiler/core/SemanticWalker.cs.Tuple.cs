using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

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
	public override Node? VisitTuple(ITupleOperation operation, SenseArgument argument)
	{
		/*
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
		*/
		
		var nodes = new List<Node>();
		var tupleType = (INamedTypeSymbol)operation.NaturalType!;
		for (var index = 0; index < operation.Elements.Length; index++)
		{
			var fieldName = tupleType.TupleElements[index].Name;
			var element = operation.Elements[index];
			var key = new Identifier(fieldName);
			var value = Translate<Expression>(element, argument);
			nodes.Add(new ObjectProperty(
				PropertyKind.Init,
				key: key,
				value: value,
				computed: false,
				shorthand: false,
				method: false
			));
		}

		return new ObjectExpression(NodeList.From(nodes));		
	}

	/// <summary>
	/// 处理解构赋值操作
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitDeconstructionAssignment(IDeconstructionAssignmentOperation operation, SenseArgument argument)
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
		var expressions = new List<Expression>();
		Deconstruct(operation.Target, operation.Value.Type!, operation.Value, expressions);
		return new SequenceExpression(NodeList.From(expressions));

		void Deconstruct(IOperation target, ITypeSymbol valueType, object value, List<Expression> exprs)
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
					var declarator = new VariableDeclarator(idExpr, null);
					argument.AddVarDeclarator(declarator, _recursionDepth);
					exprs.Add(new AssignmentExpression(Operator.Assignment, idExpr, init));
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
							HandleTransformationFailure<Node>(target, $"The {target.Kind} operation is not supported in DeconstructionAssignment.");
							return;
						}

						var id = Translate<Node>(element, argument);
						var declarator = new VariableDeclarator(id, null);
						argument.AddVarDeclarator(declarator, _recursionDepth);
						exprs.Add(new AssignmentExpression(Operator.Assignment, id, init));
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
							HandleTransformationFailure<Node>(target, $"The {target.Kind} operation is not supported in DeconstructionAssignment.");
							return;
						}

						var left = Translate<Node>(localRef, argument);
						var expr = new AssignmentExpression(Operator.Assignment, left, right);
						exprs.Add(expr);
					}
					else if (field.Type.IsTupleType)
					{
						if (value is IConversionOperation conversion && conversion.Operand is ITupleOperation conversionTuple)
						{
							var subValue = conversionTuple.Elements[index];
							Deconstruct(element, subValue.Type!, subValue, exprs);
						}
						else if (value is ILocalReferenceOperation l1)
						{
							var obj = new Identifier(l1.Local.Name);
							var prop = new Identifier(field.Name);
							var subValue = new MemberExpression(obj, prop, false, false);
							Deconstruct(element, field.Type, subValue, exprs);
						}
						else if (value is Expression p)
						{
							var prop = new Identifier(field.Name);
							var subValue = new MemberExpression(p, prop, false, false);
							Deconstruct(element, field.Type, subValue, exprs);
						}
					}
					else
					{
						HandleTransformationFailure<Node>(element, $"The {element.Kind} operation is not supported in DeconstructionAssignment.");
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
					HandleTransformationFailure<Node>(target, $"The {target.Kind} operation is not supported in DeconstructionAssignment.");
					return;
				}

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

						args.Add(id);
						argument.AddVarDeclarator(declarator, _recursionDepth);
					}
					else if (element is ITupleOperation subTuple)
					{
						// 如果是一个元组，需要创建一个临时变量，被自定义Deconstruct方法调用后
						// 再解构出元组里面变量定义或引用
						var name = GetUniqueName(subTuple);
						var id = new Identifier(name);
						var declarator = new VariableDeclarator(id, null);

						args.Add(id);
						nestedRefs.Add((index, id));
						argument.AddVarDeclarator(declarator, _recursionDepth);
					}
					else
					{
						var name = tupleType.TupleElements[index].Name;
						var id = new Identifier(name);
						// 处理声明表达式
						if (element is IDeclarationExpressionOperation)
						{
							var declarator = new VariableDeclarator(id, null);
							argument.AddVarDeclarator(declarator, _recursionDepth);
						}
						args.Add(id);
					}
				}
				
				// Deconstruct方法参数是out参数且无返回值，但js不支持out/ref
				// 所以编译器会把Deconstruct方法编成普通参数，然后返回数组对象输出本该由out参数输出的值	
				var obj = Translate<Expression>(expr, argument);
				var prop = new Identifier("Deconstruct");
				var func = new MemberExpression(obj, prop, false, false);
				var call = new CallExpression(func, NodeList.From(args), false);
				var deconstructName = GetUniqueName(operation);
				var deconstructId = new Identifier(deconstructName);
				var deconstructDecl = new VariableDeclarator(deconstructId, null);
				argument.AddVarDeclarator(deconstructDecl, _recursionDepth);
				exprs.Add(new AssignmentExpression(Operator.Assignment, deconstructId, call));

				// 从数组中取值赋给目标值
				for (var i = 0; i < args.Count; i++)
				{
					var indexer = new NumericLiteral(i, i.ToString());
					var member = new MemberExpression(deconstructId, indexer, computed: true, optional: false);
					var assignExpr = new AssignmentExpression(Operator.Assignment, args[i], member);
					exprs.Add(assignExpr);
				}

				IMethodSymbol method;
				// 处理嵌套元组中的解构参数
				if (expr is IInvocationOperation invocation)
					method = invocation.TargetMethod;
				else
					method = (IMethodSymbol)valueType
						.GetMembers()
						.First(x => x.Kind == SymbolKind.Method && x.Name == "Deconstruct");

				foreach (var (index, id) in nestedRefs)
				{
					var parameter = method.Parameters[index];
					var element = tupleResult.Elements[index];
					Deconstruct(element, parameter.Type, id, expressions);
				}
			}
			else
			{
				HandleTransformationFailure<Node>(target, $"The {target.Kind} operation is not supported in DeconstructionAssignment.");
			}
		}
	}

	/// <summary>
	/// 处理元组二元操作符操作（相等/不等比较）
	/// <para/>
	/// 支持的运算符：
	/// - == (相等): 所有元素都相等时返回 true
	/// - != (不等): 任意元素不等时返回 true
	/// <para/>
	/// C# 示例及转换结果：
	/// <code>
	/// // 简单元组比较
	/// (a, b) == (c, d)              → a===c&&b===d
	/// (x, y) != (1, 2)              → x!==1||y!==2
	///
	/// // 命名元组比较
	/// (name, age) == ("John", 25)   → name==="John"&&age===25
	///
	/// // 元组变量比较
	/// tuple1 == tuple2              → tuple1.Item1===tuple2.Item1&&tuple1.Item2===tuple2.Item2
	///
	/// // 嵌套元组比较
	/// ((a, b), c) == ((d, e), f)    → (a===d&&b===e)&&c===f
	///
	/// // 方法调用结果比较
	/// GetTuple() == (1, 2)          → const temp=GetTuple(); temp.Item1===1&&temp.Item2===2
	/// </code>
	/// <para/>
	/// 实现说明：
	/// - C# 编译器保证左右元组元素类型和个数必须相同
	/// - 递归处理嵌套元组
	/// - 使用严格相等 (===) 和严格不等 (!==) 运算符
	/// - 对于 IInvocationOperation，会创建临时变量缓存结果
	/// </summary>
	/// <param name="operation">元组二元操作</param>
	/// <param name="argument">上下文参数，用于存放临时变量定义</param>
	/// <returns>带括号的 JavaScript 逻辑表达式，转换失败返回 null</returns>
	public override Node? VisitTupleBinaryOperator(ITupleBinaryOperation operation, SenseArgument argument)
	{
		// C#本身语法限定左右都必须是同样元素类型和个数的元组
		// 所以此处不用考虑类型和个数不匹配
		var isEq = operation.OperatorKind == BinaryOperatorKind.Equals;

		var result = BuildTupleBinaryExpression(
			(operation.LeftOperand, operation.LeftOperand.Type!),
			(operation.RightOperand, operation.RightOperand.Type!),
			isEq,
			argument);

		if (result is null)
			return HandleTransformationFailure<Node>(operation, "Tuple binary operation could not be translated to JavaScript.");

		return new ParenthesizedExpression(result);
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
	public override Node? VisitDiscardOperation(IDiscardOperation operation, SenseArgument argument)
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

	/// <summary>
	/// 构建元组二元比较表达式（递归辅助方法）
	/// </summary>
	/// <param name="left">左操作数及其类型</param>
	/// <param name="right">右操作数及其类型</param>
	/// <param name="isEq">是否为相等比较（true: ==, false: !=）</param>
	/// <param name="argument">上下文参数</param>
	/// <returns>构建的逻辑表达式，失败返回 null</returns>
	private Expression? BuildTupleBinaryExpression(
		(object Target, ITypeSymbol Type) left,
		(object Target, ITypeSymbol Type) right,
		bool isEq,
		SenseArgument argument)
	{
		// 类型防御性检查
		if (left.Type is not INamedTypeSymbol leftType || right.Type is not INamedTypeSymbol rightType)
			return null;

		Expression? leftExpr = null, rightExpr = null;
		ITupleOperation? tupleLeft = null, tupleRight = null;

		// 处理左操作数
		if (left.Target is IInvocationOperation leftInvocation)
		{
			leftExpr = new Identifier(GetUniqueName(leftInvocation));
			var init = Translate<Expression>(leftInvocation, argument);
			var declarator = new VariableDeclarator(leftExpr, init);
			argument.AddVarDeclarator(declarator,_recursionDepth);
		}
		else if (left.Target is ITupleOperation leftTuple)
			tupleLeft = leftTuple;
		else if (left.Target is IOperation leftOp)
			leftExpr = Translate<Expression>(leftOp, argument);
		else if (left.Target is Expression leftExp)
			leftExpr = leftExp;

		// 处理右操作数
		if (right.Target is IInvocationOperation rightInvocation)
		{
			rightExpr = new Identifier(GetUniqueName(rightInvocation));
			var init = Translate<Expression>(rightInvocation, argument);
			var declarator = new VariableDeclarator(rightExpr, init);
			argument.AddVarDeclarator(declarator,_recursionDepth);
		}
		else if (right.Target is ITupleOperation rightTuple)
			tupleRight = rightTuple;
		else if (right.Target is IOperation rightOp)
			rightExpr = Translate<Expression>(rightOp, argument);
		else if (right.Target is Expression rightExp)
			rightExpr = rightExp;

		Expression? result = null;

		// 遍历元组元素
		for (var index = 0; index < leftType.TupleElements.Length; index++)
		{
			var leftField = leftType.TupleElements[index];
			var rightField = rightType.TupleElements[index];

			Expression? exprLeft = null, exprRight = null;

			// 获取左元素表达式
			if (tupleLeft is not null)
				exprLeft = Translate<Expression>(tupleLeft.Elements[index], argument);
			else if (leftExpr is not null)
				exprLeft = new MemberExpression(leftExpr, new Identifier(leftField.Name), false, false);

			// 获取右元素表达式
			if (tupleRight is not null)
				exprRight = Translate<Expression>(tupleRight.Elements[index], argument);
			else if (rightExpr is not null)
				exprRight = new MemberExpression(rightExpr, new Identifier(rightField.Name), false, false);

			// 处理嵌套元组
			if (leftField.Type.IsTupleType)
			{
				object? subLeft = tupleLeft is not null ? tupleLeft.Elements[index] : exprLeft;
				object? subRight = tupleRight is not null ? tupleRight.Elements[index] : exprRight;

				if (subLeft is null || subRight is null)
					return null;

				var subResult = BuildTupleBinaryExpression(
					(subLeft, leftField.Type),
					(subRight, rightField.Type),
					isEq,
					argument);

				if (subResult is null)
					return null;

				result = result is null
					? subResult
					: new LogicalExpression(isEq ? Operator.LogicalAnd : Operator.LogicalOr, result, subResult);
			}
			else
			{
				if (exprLeft is null || exprRight is null)
					return null;

				var expr = new NonLogicalBinaryExpression(
					isEq ? Operator.StrictEquality : Operator.StrictInequality,
					exprLeft,
					exprRight);

				result = result is null
					? expr
					: new LogicalExpression(isEq ? Operator.LogicalAnd : Operator.LogicalOr, result, expr);
			}
		}

		return result;
	}
}
