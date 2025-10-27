using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;

namespace ECMAScript.Compiler;

public partial class SemanticWalker
{
	/// <summary>
	/// 处理解构赋值操作
	/// C# 示例：
	/// var tuple = (aaa:1,2);
	/// (int bbb, int ccc) = tuple;
	/// int ddd,eee;
	/// (ddd, eee) = tuple;
	/// int kkk;
	/// (kkk,int qqq) = tuple;
	/// (int fff, (int ggg,int hhh)) = (2,tuple);
	/// var func = (int x,int y)=>(mmm:x,y);
	/// (int zzz,int yyy)= func(2,5);
	/// 转换结果：
	/// let tuple = {
	///     aaa: 1,
	///     Item2: 2
	/// };
	/// let bbb = tuple.aaa;
	/// let ccc = tuple.Item2;
	/// let ddd,eee;
	/// ddd = tuple.aaa;
	/// eee = tuple.Item2;
	/// let kkk;
	/// kkk = tuple.aaa;
	/// let qqq = tuple.Item2;
	/// let fff = 2;
	/// let ggg = tuple.aaa;
	/// let hhh = tuple.Item2;
	/// let func = (x,y)=>{
	///     mmm: x,
	///     Item2: y
	/// };
	/// const temp = func(2,5);
	/// let zzz = temp.aaa;
	/// let yyy = temp.Item2;
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitDeconstructionAssignment(IDeconstructionAssignmentOperation operation, Queue<VariableDeclaration> argument)
	{
		var statements = new List<Statement>();
		Deconstruct(operation.Target, operation.Value, statements);
		return new StatementGroup(NodeList.From(statements));

		void Deconstruct(IOperation target, IOperation value, List<Statement> states)
		{
			if (value.Type is null || target is not ITupleOperation tupleTarget)
			{
				// 解构语法中 target肯定是元组，value.Type 不能为空
				HandleTransformationFailure(value, $"{value.Kind} in DeconstructionAssignment cannot be null.");
				return;
			}

			Identifier? tempId = null;
			if (value is IInvocationOperation invocation)
			{
				// 如果是方法调用
				// 先造一个临时对象存放方法的值
				tempId = new Identifier(GetUniqueName(invocation));
				var init = Translate<Expression>(invocation, argument);
				var declarator = new VariableDeclarator(tempId, init);
				var declaration = new VariableDeclaration(VariableDeclarationKind.Const,
					NodeList.From(declarator));
				states.Add(declaration);

			}
			else if (value is ILocalReferenceOperation localRef)
			{
				// 如果是本地变量引用，直接使用本地变量名
				tempId = new Identifier(localRef.Local.Name);
				////自定义解构
				//foreach (var member in value.Type.GetMembers())
				//{
				//    if(member is IMethodSymbol method && 
				//       method.Name == "Deconstruct" &&
				//       method.Parameters.Length == tupleTarget.Elements.Length &&
				//       method.Parameters.Count(p=>p.RefKind != RefKind.Out) == 0)
				//    {

				//        tempId = new Identifier(method.Name);
				//        break;
				//    }
				//}  
			}
			else if (value is IConversionOperation)
			{

			}
			else
			{
				HandleTransformationFailure(value, $"{value.Kind} in DeconstructionAssignment cannot be null.");
				return;
			}


			for (var index = 0; index < tupleTarget.Elements.Length; index++)
			{
				var element = tupleTarget.Elements[index];
				// 如果解构元素是元组类型，递归解构
				if (value.Type.IsTupleType)
				{
					var field = ((INamedTypeSymbol)value.Type).TupleElements[index];
					if (element is IDeclarationExpressionOperation decl)
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
							init = new MemberExpression(tempId!, prop, false, false);
						}
						else
						{
							init = Translate<Expression>(value, argument);
						}

						var id = Translate<Node>(decl, argument);
						var declarator = new VariableDeclarator(id, init);
						var declaration = new VariableDeclaration(VariableDeclarationKind.Let,
							NodeList.From(declarator));
						states.Add(declaration);
					}
					else if (element is ILocalReferenceOperation localRef)
					{
						var left = Translate<Node>(localRef, argument);
						Expression right;
						if (value is ILocalReferenceOperation)
						{
							var obj = Translate<Expression>(value, argument);
							var prop = new Identifier(field.Name);
							right = new MemberExpression(obj, prop, false, false);
						}
						else if (value is IInvocationOperation)
						{
							var prop = new Identifier(field.Name);
							right = new MemberExpression(tempId!, prop, false, false);
						}
						else
						{
							right = Translate<Expression>(value, argument);
						}

						var expr = new AssignmentExpression(Operator.Assignment, left, right);
						var state = new NonSpecialExpressionStatement(expr);
						states.Add(state);
					}
					else if (field.Type.IsTupleType)
					{
						var subValue = value;
						if (value is IConversionOperation conversion)
						{
							if (conversion.Operand is ITupleOperation conversionTuple)
								subValue = conversionTuple.Elements[index];
						}
						Deconstruct(element, subValue, states);
					}
				}
			}
		}
	}

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
	/// 处理元组二元操作符操作
	/// C# 示例：
	/// (a, b) == (c, d)                    // 元组相等比较
	/// (x, y) != (1, 2)                    // 元组不等比较
	/// (name, age) == ("John", 25)         // 元组与常量比较
	/// tuple1 == tuple2                    // 元组变量比较
	/// 转换结果：利用编译时信息生成最简洁的比较代码
	/// </summary>
	/// <param name="operation">元组二元操作</param>
	/// <param name="argument">当前operation所属的父operation</param>
	/// <returns>JavaScript逻辑表达式</returns>
	public override Acornima.Ast.Node? VisitTupleBinaryOperator(ITupleBinaryOperation operation, Queue<VariableDeclaration> argument)
	{
		var isEq = operation.OperatorKind == BinaryOperatorKind.Equals;

		// 处理空元组比较：() == () 为 true, () == (1,) 为 false。
		// 空元组的类型没有 TupleElements。
		if (operation.Type is not INamedTypeSymbol { TupleElements.Length: > 0 } tupleType)
		{
			// 对于空元组，相等性仅取决于操作符本身。
			return new BooleanLiteral(isEq, isEq ? "true" : "false");
		}

		// 递归访问左右操作元，获取它们的表达式。
		var left = Translate<Expression>(operation.LeftOperand, argument);
		var right = Translate<Expression>(operation.RightOperand, argument);

		Acornima.Ast.Expression? result = null;

		// 遍历元组的每个元素，为每个元素生成比较表达式。
		foreach (var field in tupleType.TupleElements)
		{
			// 创建访问元组元素的表达式，如 left.Item1 或 left.Name。
			// field.Name 会正确解析为 "Item1" 或自定义名称如 "Name"。
			var leftMember = new MemberExpression(left, new Identifier(field.Name), false, false);
			var rightMember = new MemberExpression(right, new Identifier(field.Name), false, false);

			// 为当前元素创建严格相等/不相等比较。
			// 使用严格相等 (===) 更贴近C#的强类型比较语义。
			var currentComparison = new NonLogicalBinaryExpression(
				isEq ? Operator.StrictEquality : Operator.StrictInequality,
				leftMember,
				rightMember);

			// 将当前比较与之前的结果用逻辑运算符组合。
			// 相等要求所有元素都相等 (&&)，不等要求至少一个元素不等 (||)。
			if (result is null)
			{
				result = currentComparison;
			}
			else
			{
				result = new LogicalExpression(
					isEq ? Operator.LogicalAnd : Operator.LogicalOr,
					result,
					currentComparison);
			}
		}

		if (result is null)
			return HandleTransformationFailure(operation, "Tuple binary operation could not be translated to JavaScript.");

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

	/// <summary>
	/// 处理递归模式操作
	/// C# 示例：
	/// obj is Person { Name: "John", Age: > 18 }        // 类型模式 + 属性模式组合
	/// value is { Length: > 0, Count: var c }          // 属性模式组合
	/// data is MyClass { Prop1: 1, Prop2: { X: 2 } }   // 嵌套递归模式
	/// item is (int x, string s) when x > 0            // 元组模式 + when子句
	/// 转换结果：转换为JavaScript的组合条件表达式
	/// 递归模式是多个模式的组合，生成 &amp;&amp;连接的条件判断
	/// </summary>
	/// <param name="operation">递归模式操作</param>
	/// <param name="argument">当前operation所属的父operation</param>
	/// <returns>JavaScript组合条件表达式</returns>
	public override Acornima.Ast.Node? VisitRecursivePattern(IRecursivePatternOperation operation, Queue<VariableDeclaration> argument)
	{
		// 递归模式的条件判断转换
		// C# 示例：obj is Person { Name: "John", Age: > 18 }
		// 转换结果：生成 (obj instanceof Person) && (obj.Name === "John") && (obj.Age > 18)
		// 递归模式由多个子模式组成，需要用 && 连接所有条件

		var conditions = new List<Expression>();

		// 1. 处理类型模式（如果存在）
		if (operation.MatchedType is not null)
		{
			// 从父operation获取目标名称，在节点内构建表达式
			var targetName = ExtractPatternValName(operation.Parent);
			// 根据获取的名称构建目标表达式
			var target = new Identifier(targetName);
			var typeName = operation.MatchedType.IsAnonymousType
				? "Object"
				: operation.MatchedType.Name;
			Expression condition = typeName.ToLowerInvariant() switch
			{
				"string" => new NonLogicalBinaryExpression(
						Operator.StrictEquality,
						new NonUpdateUnaryExpression(Operator.TypeOf, target),
						new StringLiteral("string", "\"string\"")
					),
				"number" or "int32" or "int64" or "double" or "float" or "decimal" =>
							 new NonLogicalBinaryExpression(
						Operator.StrictEquality,
						new NonUpdateUnaryExpression(Operator.TypeOf, target),
						new StringLiteral("number", "\"number\"")
					),
				"boolean" => new NonLogicalBinaryExpression(
						Operator.StrictEquality,
						new NonUpdateUnaryExpression(Operator.TypeOf, target),
						new StringLiteral("boolean", "\"boolean\"")
					),
				"object" => new NonLogicalBinaryExpression(
						Operator.StrictEquality,
						new NonUpdateUnaryExpression(Operator.TypeOf, target),
						new StringLiteral("object", "\"object\"")
					),// 对于对象类型，检查是否不为null且为object
				_ => new NonLogicalBinaryExpression(Operator.InstanceOf, target, new Identifier(typeName)),// 对于自定义类型，使用instanceof检查
			};
			conditions.Add(condition);
		}

		// 2. 处理属性子模式（如果存在）
		if (operation.PropertySubpatterns.Length > 0)
		{
			foreach (var propertySubpattern in operation.PropertySubpatterns)
			{
				// 根据AST转换器方法复用原则，argument是父节点，这里传递当前的递归模式操作
				Translate(conditions, propertySubpattern, argument);
			}
		}

		// 3. 处理声明模式（变量声明）
		if (operation.DeclaredSymbol is not null)
		{
			// 声明模式不影响条件判断，只是绑定变量
			// 在模式匹配中，我们只关心条件判断部分
		}

		// 4. 组合所有条件
		if (conditions.Count == 0)
		{
			// 如果没有条件，返回true（空模式总是匹配）
			return new BooleanLiteral(true, "true");
		}
		else if (conditions.Count == 1)
		{
			// 只有一个条件，直接返回
			return conditions[0];
		}
		else
		{
			// 多个条件，用 && 连接
			Expression result = conditions[0];
			for (int i = 1; i < conditions.Count; i++)
			{
				result = new LogicalExpression(Operator.LogicalAnd, result, conditions[i]);
			}
			return result;
		}
	}

}
