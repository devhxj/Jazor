/*
IPatternOperation  (base: InputType, NarrowedType)
 ├─ 常见的上下文（pattern 可直接出现在这些位置）
 │   ├─ IIsPatternOperation
 │   │   ├─ Value : IOperation
 │   │   └─ Pattern : IPatternOperation
 │   ├─ IPatternCaseClauseOperation  (switch/case 语句的 case)
 │   │   ├─ Pattern : IPatternOperation
 │   │   ├─ Guard : IOperation? 
 │   │   └─ Label : ILabelSymbol?
 │   ├─ ISwitchExpressionArmOperation (switch expression 的 arm)
 │   │   ├─ Pattern : IPatternOperation
 │   │   ├─ Guard : IOperation?
 │   │   └─ Value : IOperation (右侧表达式)
 │   ├─ IRangeCaseClauseOperation (range case; 相关于 pattern 的分支 VB特有)
 │   │   ├─ MinimumValue : IOperation?
 │   │   ├─ MaximumValue : IOperation?
 │   │   └─ (可能与 pattern 组合的其它成员)
 │   └─ 其它位置（如分析/推断 API 中直接使用 IPatternOperation）
 ├─ 具体的 pattern 类型（均为 IPatternOperation 的实现）
 │   ├─ IConstantPatternOperation
 │   │   └─ Value : IOperation (ILiteralOperation / IConversionOperation / IFieldReferenceOperation / ...)
 │   ├─ IDeclarationPatternOperation
 │   │   ├─ DeclaredSymbol : ISymbol  (绑定到的新变量)
 │   │   ├─ MatchesNull : bool
 │   │   └─ MatchedType : ITypeSymbol
 │   ├─ IDiscardPatternOperation
 │   │   └─ (表示 `_`)  (仅 InputType / NarrowedType)
 │   ├─ ITypePatternOperation
 │   │   ├─ MatchedType : ITypeSymbol
 │   │   └─ (InputType / NarrowedType)
 │   ├─ IRelationalPatternOperation
 │   │   ├─ OperatorKind : BinaryOperatorKind (LessThan / GreaterThan / Equals / NotEquals / ...)
 │   │   └─ Value : IOperation (Literal / FieldRef / Conversion / ...)
 │   ├─ INegatedPatternOperation
 │   │   └─ Pattern : IPatternOperation
 │   ├─ IBinaryPatternOperation
 │   │   ├─ OperatorKind : BinaryOperatorKind (And / Or)
 │   │   ├─ LeftPattern : IPatternOperation
 │   │   └─ RightPattern : IPatternOperation
 │   ├─ IRecursivePatternOperation
 │   │   ├─ MatchedType : ITypeSymbol
 │   │   ├─ DeconstructSymbol : ISymbol? (Deconstruct 方法符号，若有)
 │   │   ├─ DeclaredSymbol : ISymbol? (模式绑定的符号，如命名元组/变量)
 │   │   ├─ DeconstructionSubpatterns[] : ImmutableArray<IPatternOperation>  (位置式 / positional 子模式)
 │   │   └─ PropertySubpatterns[] : ImmutableArray<IPropertySubpatternOperation>
 │   │       └─ IPropertySubpatternOperation
 │   │           ├─ Member : ISymbol (属性/字段/成员符号)
 │   │           └─ Pattern : IPatternOperation (可递归)
 │   ├─ IListPatternOperation (list / list pattern)
 │   │   ├─ patterns[] : ImmutableArray<IPatternOperation> (按索引的子模式)
 │   │   ├─ LengthSymbol : ISymbol? (Length/Count property，可能为 null)
 │   │   ├─ IndexerSymbol : ISymbol? (indexer 或隐式索引器)
 │   │   └─ DeclaredSymbol : ISymbol? (若列表模式绑定一个变量)
 │   ├─ ISlicePatternOperation
 │   │   ├─ SliceSymbol : ISymbol? (索引/切片相关符号，可能为 null)
 │   │   ├─ Pattern : IPatternOperation? (切片内子模式，或 null 表示省略)
 │   │   └─ (InputType / NarrowedType)
 │   └─ 可能的扩展/语言特性 pattern（比如组合/新的子类型）
 └─ 递归与嵌套说明
     ├─ 任一 IPatternOperation 都可以出现在另一个 pattern 的子位置（例如 Binary/Negated/Recursive 的子 Pattern）
     ├─ Pattern 可作为 switch/case/arm 的顶层（无需通过 IIsPatternOperation 包裹）
     └─ IIsPatternOperation 只是表达式层面 (expr is pattern) 的容器，而非 pattern 的唯一父节点

IIsPatternOperation
 ├─ Value : IOperation (任意表达式)
 └─ Pattern : IPatternOperation
     ├─ IConstantPatternOperation
     │   └─ Value : IOperation (常见：ILiteralOperation / IConversionOperation / IFieldReferenceOperation / ...)
     ├─ IDeclarationPatternOperation
     │   ├─ DeclaredSymbol : ISymbol (绑定到的新变量)
     │   ├─ MatchesNull : bool
     │   └─ MatchedType : ITypeSymbol
     ├─ IDiscardPatternOperation
     │   └─ (表示 `_`) (继承 IPatternOperation 的 InputType / NarrowedType)
     ├─ ITypePatternOperation
     │   ├─ MatchedType : ITypeSymbol
     │   └─ (InputType, NarrowedType 来自 IPatternOperation)
     ├─ IRelationalPatternOperation
     │   ├─ OperatorKind : BinaryOperatorKind (LessThan / GreaterThan / Equals / NotEquals / ...)
     │   └─ Value : IOperation (Literal / FieldRef / Conversion / ...)
     ├─ INegatedPatternOperation
     │   └─ Pattern : IPatternOperation
     ├─ IBinaryPatternOperation
     │   ├─ OperatorKind : BinaryOperatorKind (And / Or)
     │   ├─ LeftPattern : IPatternOperation
     │   └─ RightPattern : IPatternOperation
     ├─ IRecursivePatternOperation
     │   ├─ MatchedType : ITypeSymbol
     │   ├─ DeconstructSymbol : ISymbol? (Deconstruct 方法符号，若有)
     │   ├─ DeclaredSymbol : ISymbol? (绑定到的变量/命名元组等)
     │   ├─ DeconstructionSubpatterns[] : ImmutableArray<IPatternOperation>  (位置式/析构子模式 / positional subpatterns)
     │   └─ PropertySubpatterns[] : ImmutableArray<IPropertySubpatternOperation>
     │       └─ IPropertySubpatternOperation
     │           ├─ Member : ISymbol (属性/字段/成员符号)
     │           └─ Pattern : IPatternOperation (可递归：Constant/Declaration/Recursive/...)
     ├─ IListPatternOperation (list / list pattern)
     │   ├─ patterns[] : ImmutableArray<IPatternOperation> (按索引的子模式)
     │   ├─ LengthSymbol : ISymbol? (Length/Count property, 可为 null 表示不可用)
     │   ├─ IndexerSymbol : ISymbol? (indexer 或隐式索引器)
     │   └─ DeclaredSymbol : ISymbol? (若列表模式绑定一个变量)
     ├─ ISlicePatternOperation
     │   ├─ SliceSymbol : ISymbol? (索引/切片相关符号，可能为 null)
     │   ├─ Pattern : IPatternOperation? (切片内的子模式，或 null 表示省略)
     │   └─ (InputType / NarrowedType)
     └─ 其他相关/组合节点
         ├─ IPropertySubpatternOperation (见上)
         ├─ IPatternCaseClauseOperation / ISwitchExpressionArmOperation
         │   ├─ Pattern : IPatternOperation
         │   ├─ Guard : IOperation? (可选守卫)
         │   └─ Label : ILabelSymbol?
         └─ IPatternOperation（基接口）
             ├─ InputType : ITypeSymbol (模式的输入类型)
             └─ NarrowedType : ITypeSymbol (匹配成功后被“缩窄”的类型)
*/
using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ECMAScript.Compiler;

public partial class SemanticWalker
{
	private static readonly NullLiteral NullExpr = new("null");

	private static readonly MemberExpression IsArrayExpr = new(new Identifier("Array"), new Identifier("isArray"), computed: false, optional: false);

	private static bool IsNullableType(ITypeSymbol? type)
		=> type is INamedTypeSymbol namedType && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;

	/// <summary>
	/// 提取模式操作中引用对象名称
	/// </summary>
	/// <param name="operation">模式相关操作</param>
	/// <returns>引用对象名称</returns>
	private Expression? ExtractPatternRefrence(IOperation? operation)
	{
		if (operation is null)
			return null;

		var visited = new HashSet<IOperation>();
		var current = operation;
		IOperation? reference = null;
		Queue<(Identifier, bool)> members = [];
		while (current is not null)
		{
			if (!visited.Add(current))
				break;
			else if (current is IPropertySubpatternOperation propertySubpatternOp)
			{
				if (propertySubpatternOp.Member is IFieldReferenceOperation fieldRef)
				{
					var member = new Identifier(fieldRef.Field.Name);
					var optional = IsNullableType(fieldRef.Field.Type);
					members.Enqueue((member, optional));
				}
				else if (propertySubpatternOp.Member is IPropertyReferenceOperation propRef)
				{
					var member = new Identifier(propRef.Property.Name);
					var optional = IsNullableType(propRef.Property.Type);
					members.Enqueue((member, optional));
				}
			}
			else if (current is IIsTypeOperation isTypeOp)
			{
				reference = isTypeOp.ValueOperand;
				break;
			}
			else if (current is IIsPatternOperation isPatternOp)
			{
				reference = isPatternOp.Value;
				break;
			}
			else if (current is ISwitchExpressionOperation switchExpressionOp)
			{
				reference = switchExpressionOp.Value;
				break;
			}

			// 继续向上
			current = current.Parent;
		}

		if (reference is null)
			return null;

		var expr = Translate<Expression>(reference, []);
		while (members.Count > 0)
		{
			var (member, optional) = members.Dequeue();
			expr = new MemberExpression(expr, member, computed: false, optional);
		}

		return expr;
	}

	/// <summary>
	/// 提取模式操作中引用对象名称
	/// </summary>
	/// <param name="operation">模式相关操作</param>
	/// <returns>引用对象名称</returns>
	private Expression GetPatternRefrence(IOperation operation)
	{
		var expr = ExtractPatternRefrence(operation);
		if (expr is null)
		{
			var location = operation.Syntax.GetLocation();
			var message = $"无法提取模式引用对象名称，操作类型：{operation.Kind}。";
			_report?.Invoke(location, message);

			throw new OperationTransformationException(operation, message);
		}

		return expr;
	}

	private Expression CreateTypeMatchExpr(IOperation operation, ITypeSymbol typeSymbol, Expression value, bool? nullable = null)
	{
		Expression? result = null;
		var typeName = typeSymbol.Name;
		var fullTypeName = typeSymbol.ToDisplayString();

		// 类型映射
		// object -> js object
		// string -> js string
		// byte、sbyte、short、ushort、int、uint、decimal、double、float -> js Number
		// long、timestamp -> BigInt
		// DateOnly、TimeOnly、DateTime、DateTimeOffset -> js Date
		// Array -> js array
		// IDictionary -> js Map
		// IEnumerable(非IDictionary) -> js Set
		// 其他 class -> js class

		// 使用 SpecialType 进行基础类型检查，更加类型安全和高效
		switch (typeSymbol.SpecialType)
		{
			case SpecialType.System_Char:
			case SpecialType.System_String:
				result = TypeOfExpr(value, new StringLiteral("string", "'string'"));
				break;
			case SpecialType.System_SByte:
			case SpecialType.System_Byte:
			case SpecialType.System_Int16:
			case SpecialType.System_UInt16:
			case SpecialType.System_Int32:
			case SpecialType.System_UInt32:
			case SpecialType.System_Single:
			case SpecialType.System_Double:
			case SpecialType.System_Decimal:
				result = TypeOfExpr(value, new StringLiteral("number", "'number'"));
				break;
			case SpecialType.System_Boolean:
				result = TypeOfExpr(value, new StringLiteral("boolean", "'boolean'"));
				break;
			case SpecialType.System_Object:
				result = TypeOfExpr(value, new StringLiteral("object", "'object'"));
				break;
			case SpecialType.System_Int64:
			case SpecialType.System_UInt64:
				result = TypeOfExpr(value, new StringLiteral("bigint", "'bigint'"));
				break;
			case SpecialType.System_DateTime:
				result = InstanceOfExpr(value, new Identifier("Date"));
				break;
			default:
				{
					// 元组类型
					if (typeSymbol.IsTupleType) { }

					// 匿名类型检查为 object
					else if (typeSymbol.IsAnonymousType)
						result = TypeOfExpr(value, new StringLiteral("object", "'object'"));

					// 大整数类型检查（long、timestamp等）
					else if (typeName.Equals("timestamp", StringComparison.OrdinalIgnoreCase))
						result = TypeOfExpr(value, new StringLiteral("bigint", "'bigint'"));

					// 日期类型检查
					else if (typeName.Equals("DateOnly", StringComparison.OrdinalIgnoreCase) ||
							typeName.Equals("TimeOnly", StringComparison.OrdinalIgnoreCase) ||
							typeName.Equals("DateTime", StringComparison.OrdinalIgnoreCase) ||
							typeName.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase))
						result = InstanceOfExpr(value, new Identifier("Date"));

					// 数组类型检查
					else if (typeName.Equals("Array", StringComparison.OrdinalIgnoreCase) ||
						fullTypeName.Contains("[]"))
						result = new CallExpression(IsArrayExpr, NodeList.From(value), optional: false);

					// 字典类型检查
					else if (typeName.Equals("IDictionary", StringComparison.OrdinalIgnoreCase) ||
						(typeSymbol is INamedTypeSymbol namedType &&
						 namedType.AllInterfaces.Any(i => i.Name.Equals("IDictionary", StringComparison.OrdinalIgnoreCase))))
						result = InstanceOfExpr(value, new Identifier("Map"));

					// 集合类型检查（非字典）
					else if (typeName.Equals("IEnumerable", StringComparison.OrdinalIgnoreCase) ||
						(typeSymbol is INamedTypeSymbol enumType &&
						 enumType.AllInterfaces.Any(i => i.Name.Equals("IEnumerable", StringComparison.OrdinalIgnoreCase)) &&
						 !enumType.AllInterfaces.Any(i => i.Name.Equals("IDictionary", StringComparison.OrdinalIgnoreCase))))
						result = InstanceOfExpr(value, new Identifier("Map"));

					// 对于自定义类型，使用instanceof检查
					else if (typeSymbol.TypeKind == TypeKind.Class)
					{
						var right = new Identifier(typeSymbol.Name);
						result = InstanceOfExpr(value, right);
					}
				}
				break;
		}

		// 判断可空
		if (nullable ?? IsNullableType(typeSymbol))
		{
			var expr = new NonLogicalBinaryExpression(Operator.StrictEquality, value, NullExpr);
			result = result is null ? expr : new LogicalExpression(Operator.LogicalOr, result, expr);
		}

		if (result is null)
			return HandleTransformationFailure<Expression>(operation, "Unsupported type in is-type operation.");

		return result;

		static NonLogicalBinaryExpression TypeOfExpr(Expression target, Literal literal)
		{
			return new NonLogicalBinaryExpression(
				Operator.StrictEquality,
				new NonUpdateUnaryExpression(Operator.TypeOf, target),
				literal
			);
		}

		static NonLogicalBinaryExpression InstanceOfExpr(Expression target, Expression expr)
		{
			return new NonLogicalBinaryExpression(Operator.InstanceOf, target, expr);
		}
	}
	
	/// <summary>
	/// 处理类型检查操作（is 运算符）
	/// C# 示例：
	/// obj is string   // 检查对象是否为特定类型
	/// typeof obj === 'string'
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitIsType(IIsTypeOperation operation, Context argument)
	{
		var value = Translate<Expression>(operation.ValueOperand, argument);
		var result = CreateTypeMatchExpr(operation, operation.TypeOperand, value);
		if (operation.IsNegated)
			return new NonUpdateUnaryExpression(Operator.LogicalNot, result);
		
		return result;
	}

	/// <summary>
	/// 处理 null 检查操作
	/// C# 示例：
	/// obj is null             // 检查是否为 null
	/// value == null           // 直接 null 比较
	/// 转换结果：obj === null
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitIsNull(IIsNullOperation operation, Context argument)
	{
		// null检查转换为 === null 比较
		var operand = Translate<Expression>(operation.Operand, argument);

		return new NonLogicalBinaryExpression(Operator.StrictEquality, operand, NullExpr);
	}

	/// <summary>
	/// 处理 is 模式匹配操作
	/// C# 示例：
	/// obj is int value                    // 模式匹配并声明变量
	/// x is > 0 and < 10                   // 关系模式匹配
	/// input is "hello"                    // 常量模式匹配
	/// 转换结果：对于复杂模式，替换占位符并返回条件表达式
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitIsPattern(IIsPatternOperation operation, Context argument)
	{
		return Translate<Expression>(operation.Pattern, argument);
	}

	/// <summary>
	/// 处理模式 case 子句操作
	/// C# 示例：
	/// switch (obj) {
	///     case string s when s.Length > 0:
	///         Console.WriteLine(s);
	///         break;
	/// }
	/// 转换结果：转换为条件表达式
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitPatternCaseClause(IPatternCaseClauseOperation operation, Context argument)
	{
		// 模式 case 子句转换为条件表达式
		return Visit(operation.Pattern, argument);
	}

	/// <summary>
	/// 处理 switch 表达式分支操作
	/// 根据上下文返回SwitchCase（传统switch）或Statement（模式匹配）
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitSwitchExpressionArm(ISwitchExpressionArmOperation operation, Context argument)
	{
		var pattern = Translate<Expression>(operation.Pattern, argument);
		var guard = Translate<Expression>(operation.Guard, argument, null);
		var value = Translate<Expression>(operation.Value, argument);

		// 检查是否为传统的常量模式（无when子句）
		bool isTraditionalPattern = (operation.Pattern.Kind == OperationKind.ConstantPattern ||
								   operation.Pattern.Kind == OperationKind.DiscardPattern) &&
								   operation.Guard == null;

		if (isTraditionalPattern)
		{
			// 生成SwitchCase用于传统switch语句
			Expression? test = null;

			if (operation.Pattern.Kind == OperationKind.ConstantPattern)
				test = pattern;

			// DiscardPattern的test为null（默认情况）
			// SwitchCase中不能直接使用ReturnStatement，应该使用break
			// 对于switch表达式转换为switch语句的场景，需要在外层包装函数来处理返回值
			var breakStatement = new BreakStatement(null);

			return new SwitchCase(
				test,
				NodeList.From<Statement>(new NonSpecialExpressionStatement(value), breakStatement)
			);
		}
		else
		{
			// 生成Statement用于模式匹配IIFE
			if (operation.Pattern.Kind == OperationKind.DiscardPattern)
			{
				// 默认情况，直接返回
				return new ReturnStatement(value);
			}
			else if (pattern is not null)
			{
				Expression condition;

				if (operation.Pattern.Kind == OperationKind.ConstantPattern)
				{
					// 从父operation获取switch目标名称并构建表达式
					var target = GetPatternRefrence(operation);
					condition = new LogicalExpression(Operator.StrictEquality, target, pattern);
				}
				else
				{
					// 复杂模式，直接使用模式表达式（已经包含实际目标）
					condition = pattern;
				}

				// 处理when子句
				if (guard is not null)
				{
					condition = new LogicalExpression(Operator.LogicalAnd, condition, guard);
				}

				return new IfStatement(condition, new ReturnStatement(value), null);
			}
		}

		return HandleTransformationFailure(operation, "Switch expression arm could not be translated to JavaScript.");
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
	public override Acornima.Ast.Node? VisitRecursivePattern(IRecursivePatternOperation operation, Context argument)
	{
		// 递归模式的条件判断转换
		// C# 示例：obj is Person { Name: "John", Age: > 18 }
		// 转换结果：生成 (obj instanceof Person) && (obj.Name === "John") && (obj.Age > 18)
		// 递归模式由多个子模式组成，需要用 && 连接所有条件

		var exprs = new List<Expression>();

		// 如果不存在子模式，处理类型模式
		if (operation.PropertySubpatterns.Length > 0)
		{
			foreach (var propertySubpattern in operation.PropertySubpatterns)
				Translate(exprs, propertySubpattern, argument);
		}
		else
		{
			// 根据获取的名称构建目标表达式
			var obj = GetPatternRefrence(operation);
			var expr = CreateTypeMatchExpr(operation,operation.MatchedType, obj);
			exprs.Add(expr);
		}

		// 声明模式不影响条件判断，只是绑定变量
		// 在模式匹配中，我们只关心条件判断部分
		// operation.DeclaredSymbol

		// 组合所有条件
		Expression result;

		// 如果没有条件，返回true（空模式总是匹配）
		if (exprs.Count == 0)
			result = new BooleanLiteral(true, "true");
		// 只有一个条件，直接返回
		else if (exprs.Count == 1)
			result = exprs[0];
		else
		{
			// 多个条件，用 && 连接
			result = exprs[0];
			for (int i = 1; i < exprs.Count; i++)
			{
				result = new LogicalExpression(Operator.LogicalAnd, result, exprs[i]);
			}
		}

		return result;
	}

	/// <summary>
	/// 处理常量模式操作
	/// C# 示例：
	/// obj is 42              // 常量模式匹配
	/// obj is "hello"         // 字符串常量模式
	/// obj is null            // null 常量模式
	/// 转换结果：返回常量字面量进行比较
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitConstantPattern(IConstantPatternOperation operation, Context argument)
	{
		var expr = Translate<Expression>(operation.Value, argument);

		// 对于常量模式，直接比较
		if (operation.Parent is
			IIsPatternOperation or
			IBinaryPatternOperation or
			INegatedPatternOperation or
			IPropertySubpatternOperation)
		{
			var obj = GetPatternRefrence(operation.Parent);
			return new NonLogicalBinaryExpression(Operator.StrictEquality, obj, expr);
		}

		return Translate<Expression>(operation.Value, argument);
	}

	/// <summary>
	/// 处理声明模式操作
	/// </summary>
	/// <param name="operation">声明模式操作</param>
	/// <param name="value">赋值对象</param>
	/// <param name="argument"></param>
	/// <returns></returns>
	private Expression VisitDeclarationPattern(IDeclarationPatternOperation operation, Expression? value, Context argument)
	{
		/* 
		有效 - 显式类型声明，MatchedType 非空，DeclaredSymbol 非空：if (obj is string s)，显式指定类型并声明变量
		有效 - 推断类型声明，MatchedType null，DeclaredSymbol 非空：if (obj is var s)，类型推断，声明变量
		有效 - 类型检查，MatchedType 非空，DeclaredSymbol null：if (obj is string)，仅检查类型，不声明变量
		无效，MatchedType null，DeclaredSymbol null：if (obj is )，语法错误：未指定类型，未声明变量
		*/

		if (operation.DeclaredSymbol is null && operation.MatchedType is null)
			return HandleTransformationFailure<Expression>(operation, "Declaration pattern must have either a declared symbol or a matched type.");

		var obj = GetPatternRefrence(operation);

		Expression? typeMatchExpr = null, declaredExpr = null;

		if (operation.MatchedType is not null)
		{
			typeMatchExpr = CreateTypeMatchExpr(operation, operation.MatchedType, obj);
		}

		if (operation.DeclaredSymbol is not null)
		{
			// 声明模式转换为变量声明
			var id = new Identifier(operation.DeclaredSymbol.Name);
			var declarator = new VariableDeclarator(id, null);
			var declaration = new VariableDeclaration(VariableDeclarationKind.Let, NodeList.From(declarator));
			argument.Enqueue(declaration);

			Expression? assignValueExpr = null;
			if (value is not null)
				assignValueExpr = value;

			else if (operation.Parent is ISlicePatternOperation slicePatternOp && slicePatternOp.SliceSymbol is null)
				assignValueExpr = obj;

			else if (operation.Parent is IIsPatternOperation)
				assignValueExpr = obj;

			if (assignValueExpr is null)
				return HandleTransformationFailure<Expression>(operation, "Cannot determine value to assign in declaration pattern.");

			var assignmentExpr = new AssignmentExpression(Operator.Assignment, id, assignValueExpr);
			var exprs = NodeList.From<Expression>(assignmentExpr, new BooleanLiteral(true, "true"));

			declaredExpr = new SequenceExpression(exprs);
		}

		if (typeMatchExpr is not null && declaredExpr is not null)
			return new LogicalExpression(Operator.LogicalAnd, typeMatchExpr, declaredExpr);
		else if (typeMatchExpr is not null)
			return typeMatchExpr;
		else
			return declaredExpr!;
	}

	/// <summary>
	/// 处理声明模式操作
	/// C# 示例：
	/// obj is int value        // 类型模式声明
	/// obj is string { Length: > 0 } str // 属性模式声明
	/// 转换结果：转换为变量声明 let value / let str
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitDeclarationPattern(IDeclarationPatternOperation operation, Context argument)
		=> VisitDeclarationPattern(operation, null, argument);
	
	/// <summary>
	/// 处理丢弃模式操作
	/// C# 示例：
	/// value switch {
	///     _ => "Default",              // 丢弃模式，总是匹配
	///     var _ => "Any value",        // 丢弃模式变量声明
	/// }
	/// obj is _                         // 丢弃模式类型检查
	/// 转换结果：转换为JavaScript的true条件表达式
	/// 丢弃模式表示"总是匹配"，在条件判断中等价于true
	/// </summary>
	/// <param name="operation">丢弃模式操作</param>
	/// <param name="argument">当前operation所属的父operation</param>
	/// <returns>JavaScript布尔字面量true</returns>
	public override Acornima.Ast.Node? VisitDiscardPattern(IDiscardPatternOperation operation, Context argument)
	{
		// 丢弃模式的条件判断转换
		// C# 示例：_ 表示"总是匹配"的模式
		// 转换结果：生成JavaScript的true字面量
		// 因为丢弃模式在任何情况下都应该匹配成功

		// 丢弃模式总是匹配，返回true字面量
		return new BooleanLiteral(true, "true");
	}

	/// <summary>
	/// 处理属性子模式操作
	/// C# 示例：
	/// obj is { Name: "John" }             // 属性模式中的 Name: "John" 部分
	/// person is { Age: > 18 }             // 属性条件模式
	/// item is { Length: var len }         // 属性声明模式
	/// data is { Count: not 0 }            // 属性取反模式
	/// 转换结果：转换为JavaScript的属性访问和比较表达式
	/// Name: "John" 转换为 obj.Name === "John"
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitPropertySubpattern(IPropertySubpatternOperation operation, Context argument)
	{
		// 属性子模式的条件判断转换
		// C# 示例：obj is { Name: "John" } 中的 Name: "John" 部分
		//         转换为 obj.Name === "John" 的JavaScript表达式
		// 转换结果：生成属性访问和比较的组合表达式
		// 访问属性模式并转换为表达式
		return Translate<Expression>(operation.Pattern, argument);
	}

	/// <summary>
	/// 处理取反模式操作
	/// C# 示例：
	/// obj is not null         // not 模式
	/// value is not 0          // 取反常量模式
	/// item is not { IsValid: false } // 取反属性模式
	/// 转换结果：转换为JavaScript的逻辑非操作符（!）
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitNegatedPattern(INegatedPatternOperation operation, Context argument)
	{
		// 取反模式的条件判断转换
		// C# 示例：obj is not null 是一个布尔条件表达式
		//         value is not 0 检查值是否不等于0
		// 转换结果：生成JavaScript的逻辑非表达式

		// 访问内部模式并转换为表达式
		var expr = Translate<Expression>(operation.Pattern, argument);

		// 使用NonUpdateUnaryExpression处理逻辑非操作
		return new NonUpdateUnaryExpression(Operator.LogicalNot, expr);
	}

	/// <summary>
	/// 处理二元模式操作
	/// C# 示例：
	/// value is > 0 and < 100              // and 模式（组合条件）
	/// obj is string or int                // or 模式（类型选择）
	/// item is not null and [1, 2, 3]     // 复杂and模式
	/// data is { Length: > 5 } or null    // 属性模式与or组合
	/// 转换结果：转换为JavaScript的逻辑表达式（&amp;&amp; 或 ||）
	/// and 模式转换为 (left) &amp;&amp; (right)，or 模式转换为 (left) || (right)
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitBinaryPattern(IBinaryPatternOperation operation, Context argument)
	{
		// 二元模式的条件判断转换
		// C# 示例：value is > 0 and < 100 是一个布尔条件表达式
		//         obj is string or int 检查对象是否为指定类型中的任意一种
		// 转换结果：生成相应的JavaScript逻辑表达式

		// 访问左右两个子模式
		var left = Translate<Expression>(operation.LeftPattern, argument);
		var right = Translate<Expression>(operation.RightPattern, argument);

		// 检查模式的类型来确定操作符
		var @operator = operation.OperatorKind switch
		{
			BinaryOperatorKind.And => Operator.LogicalAnd,
			BinaryOperatorKind.Or => Operator.LogicalOr,
			_ => Operator.Unknown
		};

		if (@operator == Operator.Unknown)
			return HandleTransformationFailure(operation, "Unsupported binary operator in pattern.");

		return new LogicalExpression(@operator, left, right);
	}

	/// <summary>
	/// 处理关系模式操作
	/// C# 示例：
	/// value is > 0            // 大于模式
	/// age is >= 18 and <= 65  // 组合关系模式
	/// score is < 60           // 小于模式
	/// 转换结果：转换为JavaScript的关系比较表达式
	/// value > 0, age >= 18, score < 60
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitRelationalPattern(IRelationalPatternOperation operation, Context argument)
	{
		// 关系模式的条件判断转换
		// C# 示例：value is > 0 是一个布尔条件表达式
		//         age is >= 18 检查年龄是否满足条件
		// 转换结果：生成相应的JavaScript关系比较表达式
		// 从参考操作中提取名称构建目标表达式
		var left = GetPatternRefrence(operation);

		// 获取右操作数（比较值）
		var right = Translate<Expression>(operation.Value, argument);

		// 根据编译时优化原则，直接生成最简洁的JavaScript关系比较表达式
		// 将C#的关系操作符映射到JavaScript的操作符
		// 如果在取反模式中，需要反转操作符（如 Equals 变为 StrictInequality）
		var @operator = operation.OperatorKind switch
		{
			BinaryOperatorKind.GreaterThan => Operator.GreaterThan,
			BinaryOperatorKind.GreaterThanOrEqual => Operator.GreaterThanOrEqual,
			BinaryOperatorKind.LessThan => Operator.LessThan,
			BinaryOperatorKind.LessThanOrEqual => Operator.LessThanOrEqual,
			BinaryOperatorKind.Equals => Operator.StrictEquality,
			BinaryOperatorKind.NotEquals => Operator.StrictInequality,
			_ => Operator.Unknown
		};

		if (@operator == Operator.Unknown)
			return HandleTransformationFailure(operation, "Unsupported relational operator in pattern.");

		// 根据AST节点构造规范，使用LogicalExpression表示比较操作
		// 使用实际的目标表达式而不是占位符
		return new NonLogicalBinaryExpression(@operator, left, right);
	}

	/// <summary>
	/// 处理列表模式操作
	/// C# 示例：
	///   list is [1, 2, ..]            // 长度 ≥2 即可
	///   list is [var a, .. var rest]  // 长度 ≥1，rest 运行时就是 slice(1)
	/// 转换结果：
	///   Array.isArray(list) &&
	///   list.length >= 2 &&
	///   list[0] === 1
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitListPattern(IListPatternOperation operation, Context argument)
	{
		if (operation.Patterns.IsEmpty) 
			return null;

		// 获取目标名称，在节点内构建表达式
		var obj = GetPatternRefrence(operation);

		// 检查是数组 Array.isArray(target)
		Expression result = new CallExpression(
			callee: new MemberExpression(
				obj: new Identifier("Array"),
				property: new Identifier("isArray"),
				computed: false,
				optional: false),
			args: NodeList.From(obj),
			optional: false
		);

		// 如果有切片则需要判断长度
		var hasSlice = false;
		var sliceIndex = -1; // 记录切片模式的位置
		for (int i = 0; i < operation.Patterns.Length; i++)
		{
			if (operation.Patterns[i].Kind == OperationKind.SlicePattern)
			{
				hasSlice = true;
				sliceIndex = i;
				break;
			}
		}

		// 长度检查，有切片就用 >=，没有就用 ===
		var fixedLen = hasSlice ? operation.Patterns.Length - 1 : operation.Patterns.Length;
		var lengthProp = new Identifier("length");
		var lengthExpr = new MemberExpression(obj, lengthProp, computed: false, optional: false);
		var lengthCheck = new NonLogicalBinaryExpression(
			hasSlice ? Operator.GreaterThanOrEqual : Operator.StrictEquality,
			lengthExpr,
			new NumericLiteral(fixedLen, fixedLen.ToString())
		);
		result = new LogicalExpression(Operator.LogicalAnd, result, lengthCheck);

		// 模式处理
		for (int i = 0; i < operation.Patterns.Length; i++)
		{
			var pattern = operation.Patterns[i];
			if (pattern.Kind == OperationKind.DiscardPattern)
				continue;// 弃元模式忽略
			else if (pattern.Kind == OperationKind.ConstantPattern || pattern.Kind == OperationKind.DeclarationPattern)
			{
				// 切片前直接使用索引，切片后需要计算反向索引
				Expression prop = new NumericLiteral(i, i.ToString());
				if (hasSlice && i > sliceIndex)
				{
					var offset = operation.Patterns.Length - i;
					var subExpr = new NumericLiteral(offset, offset.ToString());
					prop = new NonLogicalBinaryExpression(Operator.Subtraction, lengthExpr, subExpr);
				}

				var indexAccess = new MemberExpression(obj, prop, computed: true, optional: false);
				Expression? expr;
				if (pattern is IDeclarationPatternOperation declarationPatternOp)
					expr = VisitDeclarationPattern(declarationPatternOp, indexAccess, argument);
				else
				{
					var value = Translate<Expression>(pattern, argument);
					expr = new NonLogicalBinaryExpression(Operator.StrictEquality, indexAccess, value);
				}

				if (expr is not null)
					result = new LogicalExpression(Operator.LogicalAnd, result, expr);
			}
			else if (pattern.Kind == OperationKind.SlicePattern)
			{
				// 切片模式可能返回空
				var expr = Translate<Expression>(pattern, argument, null);
				if (expr is not null)
					result = new LogicalExpression(Operator.LogicalAnd, result, expr);
			}
			else
			{
				var expr = Translate<Expression>(pattern, argument);
				result = new LogicalExpression(Operator.LogicalAnd, result, expr);
			}
		}

		return result;
	}

	/// <summary>
	/// 处理切片模式操作
	/// C# 示例：
	/// array is [1, .., 5]                 // 切片模式匹配（条件表达式）
	/// list is [var first, .. var middle, var last] // 切片解构条件
	/// 转换结果：生成 Array.isArray 条件检查，返回布尔值
	/// 符合模式匹配语义：判断是否匹配，而不是提取数据
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitSlicePattern(ISlicePatternOperation operation, Context argument)
	{
		if (operation.Pattern is null)
			return null;

		return Visit(operation.Pattern, argument);
	}

	/// <summary>
	/// 在模式匹配上下文中声明一个新变量（如 s）或进行类型测试。
	/// is string s 中的 string s 部分，或 case string s: 中的模式，是构成一个模式匹配的组件。
	/// 作为 IIsPatternOperation.Pattern 或 ICaseClauseOperation.Pattern 的子节点出现。
	/// C# 示例：
	/// obj is string s    // 类型模式
	/// value is int s            // 值类型模式
	/// item is MyClass s        // 自定义类型模式
	/// case string s:		  // switch case 类型模式
	/// 转换结果：根据类型生成相应的JavaScript类型检查条件
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitTypePattern(ITypePatternOperation operation, Context argument)
	{
		// 类型模式的条件判断转换
		// C# 示例：obj is string 是一个布尔条件表达式
		//         value is MyClass 检查对象是否为指定类型
		// 转换结果：根据类型生成对应的JavaScript检查条件

		var inputType = operation.InputType;
		// 根据获取的名称构建目标表达式
		var targetExpression = GetPatternRefrence(operation);

		// 根据编译时优化原则和强弱类型转换优化原则
		// 利用C#的编译时类型信息，生成最简洁的JavaScript类型检查

		var typeName = inputType.Name;

		// 对于基本类型，使用typeof检查
		return typeName.ToLowerInvariant() switch
		{
			"string" => new LogicalExpression(
								Operator.StrictEquality,
								new UpdateExpression(Operator.TypeOf, targetExpression, prefix: true),
								new StringLiteral("string", "'string'")
							),
			"number" or "int32" or "int64" or "double" or "float" or "decimal" => new LogicalExpression(
								Operator.StrictEquality,
								new UpdateExpression(Operator.TypeOf, targetExpression, prefix: true),
								new StringLiteral("number", "'number'")
							),
			"boolean" => new LogicalExpression(
								Operator.StrictEquality,
								new UpdateExpression(Operator.TypeOf, targetExpression, prefix: true),
								new StringLiteral("boolean", "'boolean'")
							),
			"object" => new LogicalExpression(
								Operator.LogicalAnd,
								new LogicalExpression(Operator.StrictInequality, targetExpression, NullExpr),
								new LogicalExpression(
									Operator.StrictEquality,
									new UpdateExpression(Operator.TypeOf, targetExpression, prefix: true),
									new StringLiteral("object", "'object'")
								)
							),// 对于对象类型，检查是否不为null且为object
			_ => new LogicalExpression(
								Operator.InstanceOf,
								targetExpression,
								new Identifier(typeName)
							),// 对于自定义类型，使用instanceof检查
		};
	}

}
