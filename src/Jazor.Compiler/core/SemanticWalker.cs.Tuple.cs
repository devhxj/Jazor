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

		/// <summary>
		/// 从 value 获取元组字段的值表达式
		/// </summary>
		/// <param name="value">值来源（IOperation 或 Expression）</param>
		/// <param name="fieldName">字段名</param>
		/// <param name="index">字段索引（用于 conversion 和 invocation 场景）</param>
		/// <param name="tempVar">临时变量（用于 invocation 场景）</param>
		/// <param name="argument">上下文参数</param>
		/// <returns>字段值表达式，失败返回 null</returns>
		Expression? GetTupleFieldValue(object value, string fieldName, int index, Identifier? tempVar, SenseArgument argument)
		{
			if (value is ILocalReferenceOperation localRef)
			{
				var obj = new Identifier(localRef.Local.Name);
				return new MemberExpression(obj, new Identifier(fieldName), false, false);
			}
			if (value is ITupleOperation tupleOp)
				return Translate<Expression>(tupleOp.Elements[index], argument);
			if (value is IConversionOperation conversion && conversion.Operand is ITupleOperation conversionTuple)
			{
				return Translate<Expression>(conversionTuple.Elements[index], argument);
			}
			if (value is IInvocationOperation && tempVar is not null)
			{
				return new MemberExpression(tempVar, new Identifier(fieldName), false, false);
			}
			if (value is IOperation op)
			{
				return Translate<Expression>(op, argument);
			}
			if (value is Expression expr)
			{
				return new MemberExpression(expr, new Identifier(fieldName), false, false);
			}
			return null;
		}

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

				Identifier? tempVar = null;
				if (value is IInvocationOperation invocation)
				{
					// 如果是方法调用，先创建临时变量存放方法的值
					tempVar = new Identifier(GetUniqueName(invocation));
					var init = Translate<Expression>(invocation, argument);
					var declarator = new VariableDeclarator(tempVar, null);
					argument.AddVarDeclarator(declarator, _recursionDepth);
					exprs.Add(new AssignmentExpression(Operator.Assignment, tempVar, init));
				}

				// 遍历元组元素进行解构
				for (var index = 0; index < tupleTarget.Elements.Length; index++)
				{
					var element = tupleTarget.Elements[index];
					var field = ((INamedTypeSymbol)valueType).TupleElements[index];

					// 跳过丢弃操作
					if (element is IDiscardOperation)
						continue;

					// 处理声明表达式（新变量声明）
					if (isDeclarationExpr || element is IDeclarationExpressionOperation)
					{
						var init = GetTupleFieldValue(value, field.Name, index, tempVar, argument);
						if (init is null)
						{
							HandleTransformationFailure<Node>(target, $"The {target.Kind} operation is not supported in DeconstructionAssignment.");
							return;
						}

						var id = Translate<Node>(element, argument);
						var declarator = new VariableDeclarator(id, null);
						argument.AddVarDeclarator(declarator, _recursionDepth);
						exprs.Add(new AssignmentExpression(Operator.Assignment, id, init));
					}
					// 处理已有变量引用
					else if (!isDeclarationExpr && element is ILocalReferenceOperation localRef)
					{
						var right = GetTupleFieldValue(value, field.Name, index, tempVar, argument);
						if (right is null)
						{
							HandleTransformationFailure<Node>(target, $"The {target.Kind} operation is not supported in DeconstructionAssignment.");
							return;
						}

						var left = Translate<Node>(localRef, argument);
						exprs.Add(new AssignmentExpression(Operator.Assignment, left, right));
					}
					// 处理嵌套元组
					else if (field.Type.IsTupleType)
					{
						if (value is IConversionOperation conversion && conversion.Operand is ITupleOperation conversionTuple)
						{
							var subValue = conversionTuple.Elements[index];
							Deconstruct(element, subValue.Type!, subValue, exprs);
						}
						else
						{
							var subValue = GetTupleFieldValue(value, field.Name, index, tempVar, argument);
							if (subValue is null)
							{
								HandleTransformationFailure<Node>(element, $"The {element.Kind} operation is not supported in DeconstructionAssignment.");
								return;
							}
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
				// 自定义解构
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
	/// <remarks>
	/// 注意：VisitSimpleAssignment 和 VisitDeconstructionAssignment 已经直接处理了 IDiscardOperation，
	/// 不会调用此方法。此方法主要作为兜底处理其他可能的场景（罕见）。
	/// </remarks>
	public override Node? VisitDiscardOperation(IDiscardOperation operation, SenseArgument argument)
	{
		// 返回 undefined 作为兜底
		// 注意：大多数情况下，此方法不会被调用，因为：
		// 1. _ = value 由 VisitSimpleAssignment 直接处理（检查 Target is IDiscardOperation）
		// 2. (_, y) = tuple 由 VisitDeconstructionAssignment 直接处理（检查 element is IDiscardOperation）
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
	/// <summary>
	/// 元组比较操作数的处理结果
	/// </summary>
	/// <param name="Expression">转换后的表达式（用于成员访问）</param>
	/// <param name="TupleOperation">如果是元组字面量，保留原始操作</param>
	private readonly record struct TupleOperandResult(Expression? Expression, ITupleOperation? TupleOperation);

	/// <summary>
	/// 处理元组比较操作数
	/// </summary>
	private TupleOperandResult ProcessTupleOperand(object target, SenseArgument argument)
	{
		if (target is IInvocationOperation invocation)
		{
			// 方法调用需要创建临时变量
			var id = new Identifier(GetUniqueName(invocation));
			var init = Translate<Expression>(invocation, argument);
			var declarator = new VariableDeclarator(id, init);
			argument.AddVarDeclarator(declarator, _recursionDepth);
			return new TupleOperandResult(id, null);
		}
		if (target is ITupleOperation tuple)
			return new TupleOperandResult(null, tuple);
		if (target is IOperation op)
			return new TupleOperandResult(Translate<Expression>(op, argument), null);
		if (target is Expression expr)
			return new TupleOperandResult(expr, null);

		return default;
	}

	/// <summary>
	/// 获取元组元素的表达式
	/// </summary>
	private Expression? GetTupleElementExpression(
		in TupleOperandResult operand,
		IFieldSymbol field,
		int index,
		SenseArgument argument)
	{
		if (operand.TupleOperation is not null)
			return Translate<Expression>(operand.TupleOperation.Elements[index], argument);
		if (operand.Expression is not null)
			return new MemberExpression(operand.Expression, new Identifier(field.Name), false, false);
		return null;
	}

	private Expression? BuildTupleBinaryExpression(
		(object Target, ITypeSymbol Type) left,
		(object Target, ITypeSymbol Type) right,
		bool isEq,
		SenseArgument argument)
	{
		// 类型防御性检查
		if (left.Type is not INamedTypeSymbol leftType || right.Type is not INamedTypeSymbol rightType)
			return null;

		// 处理左右操作数
		var leftResult = ProcessTupleOperand(left.Target, argument);
		var rightResult = ProcessTupleOperand(right.Target, argument);

		Expression? result = null;

		// 遍历元组元素
		for (var index = 0; index < leftType.TupleElements.Length; index++)
		{
			var leftField = leftType.TupleElements[index];
			var rightField = rightType.TupleElements[index];

			// 处理嵌套元组
			if (leftField.Type.IsTupleType)
			{
				var subLeft = leftResult.TupleOperation is not null
					? (object)leftResult.TupleOperation.Elements[index]
					: GetTupleElementExpression(leftResult, leftField, index, argument)!;
				var subRight = rightResult.TupleOperation is not null
					? (object)rightResult.TupleOperation.Elements[index]
					: GetTupleElementExpression(rightResult, rightField, index, argument)!;

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
				var exprLeft = GetTupleElementExpression(leftResult, leftField, index, argument);
				var exprRight = GetTupleElementExpression(rightResult, rightField, index, argument);

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
