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
	/// <summary>
	/// 处理类型检查操作（is 运算符）
	/// C# 示例：
	/// obj is string   // 检查对象是否为特定类型
	/// typeof obj === 'string'
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitIsType(IIsTypeOperation operation, WalkerArgument argument)
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
	public override Node? VisitIsNull(IIsNullOperation operation, WalkerArgument argument)
	{
		// null检查转换为 === null 比较
		var operand = Translate<Expression>(operation.Operand, argument);

		return new NonLogicalBinaryExpression(Operator.StrictEquality, operand, Null);
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
	public override Node? VisitIsPattern(IIsPatternOperation operation, WalkerArgument argument)
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
	/// 转换结果：转换为条件表达式（用于模式匹配的 switch 语句）
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitPatternCaseClause(IPatternCaseClauseOperation operation, WalkerArgument argument)
	{
		// IPatternCaseClauseOperation 没有 Value 属性
		// 只返回条件表达式（模式检查 + when 守卫）
		// 实际的语句体由 VisitSwitch 的 Body 部分处理
		var condition = Translate<Expression>(operation.Pattern, argument);

		// 处理when子句
		if (operation.Guard is not null)
		{
			var guard = Translate<Expression>(operation.Guard, argument);
			condition = new LogicalExpression(Operator.LogicalAnd, condition, guard);
		}

		return condition;
	}

	/// <summary>
	/// 处理 switch 表达式操作
	/// C# 示例：
	/// var result = value switch {
	///     1 => "One",              // 常量模式
	///     string s => $"String: {s}", // 类型模式
	///     { Length: > 5 } => "Long",   // 属性模式
	///     var x when x > 0 => "Positive", // when 子句
	///     _ => "Other"             // 默认模式
	/// };
	/// 转换结果：根据模式复杂度转换为嵌套条件表达式或函数调用
	/// <summary>
	/// 将C# switch表达式转换为JavaScript switch语句或IIFE
	/// 非模式匹配switch转换为switch语句，模式匹配switch转换为IIFE
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitSwitchExpression(ISwitchExpressionOperation operation, WalkerArgument argument)
	{
		// 至少有一个分支
		if (operation.Arms.Length < 1)
			return HandleTransformationFailure<Node>(operation, "Switch expression must have at least one arm.");

		var input = Translate<Expression>(operation.Value, argument);

		// 复杂模式匹配switch，生成健全的IIFE保证副作用顺序
		// 采用分层判断：先模式匹配，后when条件，确保求值节拍与C#一致
		var statements = new List<Statement>();

		// input 可能是方法调用或一个复杂表达式，此处定义一个中间变量存储其值
		var id = new Identifier(GetUniqueName(operation.Value.Syntax));
		var inputVar = new VariableDeclaration(
			VariableDeclarationKind.Const,
			NodeList.From(new VariableDeclarator(id, input))
		);

		// 处理所有模式，采用嵌套if确保副作用顺序
		foreach (var arm in operation.Arms)
			Translate(statements, arm, argument);

		statements.Insert(0, inputVar);
		var functionBody = new FunctionBody(NodeList.From(statements), strict: true);
		var arrowFunction = new ArrowFunctionExpression(
			NodeList.From<Node>(),
			functionBody,
			expression: false,
			async: false
		);

		// 立即调用箭头函数
		return new CallExpression(arrowFunction, NodeList.From<Expression>(), optional: false);
	}

	/// <summary>
	/// 处理 switch 表达式分支操作
	/// 根据上下文返回SwitchCase（传统switch）或Statement（模式匹配）
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitSwitchExpressionArm(ISwitchExpressionArmOperation operation, WalkerArgument argument)
	{
		var value = Translate<Expression>(operation.Value, argument);

		// 默认情况，直接返回
		if (operation.Pattern.Kind == OperationKind.DiscardPattern)
			return new ReturnStatement(value);

		else
		{
			var condition = Translate<Expression>(operation.Pattern, argument);

			// 处理when子句
			if (operation.Guard is not null)
			{
				var guard = Translate<Expression>(operation.Guard, argument);
				condition = new LogicalExpression(Operator.LogicalAnd, condition, guard);
			}

			return new IfStatement(condition, new ReturnStatement(value), null);
		}
	}

	/// <summary>
	/// 处理递归模式操作
	/// </summary>
	/// <remarks>
	/// C# 示例：
	/// <code>
	/// // 属性模式
	/// obj is Person { Name: "John", Age: > 18 }
	/// // → obj instanceof Person && obj.Name === "John" && obj.Age > 18
	///
	/// // 位置式元组模式
	/// value is (int x, string y)
	/// // → typeof value === "object" && value.Item1 !== undefined && value.Item2 !== undefined
	///
	/// // 位置式 record 模式
	/// obj is Person("John", 18)
	/// // → obj instanceof Person && obj.Name === "John" && obj.Age === 18
	///
	/// // 混合模式
	/// data is Point(int x, int y) { Z: > 0 }
	/// // → data instanceof Point && data.X !== undefined && data.Y !== undefined && data.Z > 0
	/// </code>
	/// </remarks>
	/// <param name="operation">递归模式操作</param>
	/// <param name="argument">用于存放临时变量定义的上下文</param>
	/// <returns>JavaScript组合条件表达式（使用&amp;&amp;连接所有条件）</returns>
	public override Node? VisitRecursivePattern(IRecursivePatternOperation operation, WalkerArgument argument)
	{
		var conditions = new List<Expression>();
		var targetExpr = GetPatternRefrence(operation);

		// 类型匹配条件（排除匿名类型、元组类型、object）
		if (!operation.MatchedType.IsAnonymousType &&
			!operation.MatchedType.IsTupleType &&
			operation.MatchedType.SpecialType != SpecialType.System_Object)
		{
			var typeCheck = CreateTypeMatchExpr(operation, operation.MatchedType, targetExpr);
			conditions.Add(typeCheck);
		}

		// 属性子模式（命名属性，如 { Name: "John" }）
		if (operation.PropertySubpatterns.Length > 0)
		{
			foreach (var propertySubpattern in operation.PropertySubpatterns)
			{
				var right = Translate<Expression>(propertySubpattern, argument);

				// todo：需要完善判断是否检测属性存在，比如有些固定属性不需要检测
				if (propertySubpattern.Member is IFieldReferenceOperation fieldRef)
				{
					var name = fieldRef.Field.Name;
					var left = BuildHasOwnProperty(targetExpr, name);
					var condition = new LogicalExpression(Operator.LogicalAnd, left, right);
					conditions.Add(condition);
				}
				else if (propertySubpattern.Member is IPropertyReferenceOperation propRef)
				{
					var name = propRef.Property.Name;
					var left = BuildHasOwnProperty(targetExpr, name);
					var condition = new LogicalExpression(Operator.LogicalAnd, left, right);
					conditions.Add(condition);
				}
				else
					conditions.Add(right);
			}
		}

		// 位置式解构子模式（如 (int x, string y) 或 Person("John", 18)）
		if (operation.DeconstructionSubpatterns.Length > 0)
		{
			if (operation.InputType is not INamedTypeSymbol namedType)
				return HandleTransformationFailure<Node>(operation, $"Input type '{operation.InputType}' is not a named type for deconstruction pattern.");

			ProcessPositionalSubpatterns(operation, namedType, targetExpr, conditions, argument);
		}

		// 组合所有条件
		if (conditions.Count > 0)
		{
			var result = conditions[0];
			for (int i = 1; i < conditions.Count; i++)
				result = new LogicalExpression(Operator.LogicalAnd, result, conditions[i]);
			return result;
		}

		// 空模式总是匹配
		return new BooleanLiteral(true, "true");

		static CallExpression BuildHasOwnProperty(Expression obj,string name)
		{
			var member = new StringLiteral(name, $"\"{name}\"");
			var property = new Identifier("hasOwnProperty");
			var hasPropcallee = new MemberExpression(obj, property, computed: false, optional: false);
			return new CallExpression(
				callee: hasPropcallee,
				args: NodeList.From<Expression>(member),
				optional: false
			);
		}
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
	public override Node? VisitConstantPattern(IConstantPatternOperation operation, WalkerArgument argument)
	{
		var expr = Translate<Expression>(operation.Value, argument);

		// 对于常量模式，直接比较
		if (operation.Parent is
			IIsPatternOperation or
			IBinaryPatternOperation or
			INegatedPatternOperation or
			IPropertySubpatternOperation or
			ISwitchExpressionArmOperation)
		{
			var obj = GetPatternRefrence(operation.Parent);
			return new NonLogicalBinaryExpression(Operator.StrictEquality, obj, expr);
		}

		return Translate<Expression>(operation.Value, argument);
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
	public override Node? VisitDeclarationPattern(IDeclarationPatternOperation operation, WalkerArgument argument)
		=> BuildDeclarationPattern(operation, null, argument);

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
	public override Node? VisitDiscardPattern(IDiscardPatternOperation operation, WalkerArgument argument)
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
	public override Node? VisitPropertySubpattern(IPropertySubpatternOperation operation, WalkerArgument argument)
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
	public override Node? VisitNegatedPattern(INegatedPatternOperation operation, WalkerArgument argument)
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
	public override Node? VisitBinaryPattern(IBinaryPatternOperation operation, WalkerArgument argument)
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
			return HandleTransformationFailure<Node>(operation, "Unsupported binary operator in pattern.");

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
	public override Node? VisitRelationalPattern(IRelationalPatternOperation operation, WalkerArgument argument)
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
			return HandleTransformationFailure<Node>(operation, "Unsupported relational operator in pattern.");

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
	public override Node? VisitListPattern(IListPatternOperation operation, WalkerArgument argument)
	{
		// 获取目标名称，在节点内构建表达式
		var obj = GetPatternRefrence(operation);
		var lengthProp = new Identifier("length");
		var lengthExpr = new MemberExpression(obj, lengthProp, computed: false, optional: false);

		// 检查是数组 Array.isArray(target)
		Expression result = new CallExpression(
			callee: IsArrayExpr,
			args: NodeList.From(obj),
			optional: false
		);

		if (operation.Patterns.IsEmpty)
		{
			var lengthCheck = new NonLogicalBinaryExpression(
				Operator.StrictEquality,
				lengthExpr,
				new NumericLiteral(0, "0")
			);
			result = new LogicalExpression(Operator.LogicalAnd, result, lengthCheck);
		}
		else
		{
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
						expr = BuildDeclarationPattern(declarationPatternOp, indexAccess, argument);
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
	public override Node? VisitSlicePattern(ISlicePatternOperation operation, WalkerArgument argument)
	{
		if (operation.Pattern is null)
			return null;

		return Visit(operation.Pattern, argument);
	}

	/// <summary>
	/// 处理类型模式操作
	/// ITypePatternOperation 是一种仅检查类型的模式，不声明变量。
	/// 结构：
	///   - MatchedType : ITypeSymbol - 要匹配的目标类型
	///   - InputType : ITypeSymbol - 输入类型（继承自 IPatternOperation）
	///   - NarrowedType : ITypeSymbol - 匹配成功后缩窄的类型
	///
	/// 与 IDeclarationPatternOperation 的区别：
	///   - TypePattern: 只检查类型，不声明变量（如 obj is string）
	///   - DeclarationPattern: 检查类型并声明变量（如 obj is string s）
	///
	/// C# 示例：
	///   obj is string           // 类型模式，只检查不声明
	///   value switch { int => ... }  // 类型模式在 switch 表达式中
	/// 转换结果：根据类型生成相应的 JavaScript 类型检查条件
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitTypePattern(ITypePatternOperation operation, WalkerArgument argument)
	{
		// ITypePatternOperation 只进行类型检查，不声明变量
		// 使用 MatchedType 而非 InputType，因为我们要检查的是目标类型
		var matchedType = operation.MatchedType;
		var targetExpr = GetPatternRefrence(operation);

		// 复用已有的类型匹配表达式生成方法
		// CreateTypeMatchExpr 已正确处理：
		//   - 基本类型映射（string/number/boolean/bigint）
		//   - 引用类型映射（Date/Map/Set/Class）
		//   - 数组类型检查（Array.isArray）
		//   - 可空类型处理（nullable 包含 null 检查）
		return CreateTypeMatchExpr(operation, matchedType, targetExpr);
	}

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

		var isInSwitch = false;
		var visited = new HashSet<IOperation>();
		var current = operation;
		IOperation? reference = null;
		Stack<Func<Expression, Expression>> members = [];
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
					members.Push((x) => new MemberExpression(x, member, computed: false, optional));
				}
				else if (propertySubpatternOp.Member is IPropertyReferenceOperation propRef)
				{
					var member = new Identifier(propRef.Property.Name);
					var optional = IsNullableType(propRef.Property.Type);
					members.Push((x) => new MemberExpression(x, member, computed: false, optional));
				}
			}
			else if (
				current is IPatternOperation patternOp &&
				current.Parent is IListPatternOperation listPatternOp)
			{
				// 判断是否有切片和切片位置
				var hasSlice = false;
				var sliceIndex = -1;
				for (int i = 0; i < listPatternOp.Patterns.Length; i++)
				{
					if (listPatternOp.Patterns[i].Kind == OperationKind.SlicePattern)
					{
						hasSlice = true;
						sliceIndex = i;
						break;
					}
				}

				var fixedLen = hasSlice ? listPatternOp.Patterns.Length - 1 : listPatternOp.Patterns.Length;
				var currentIndex = listPatternOp.Patterns.IndexOf(patternOp);

				members.Push((x) =>
				{
					// 切片前直接使用索引，切片后需要计算反向索引
					Expression prop = new NumericLiteral(currentIndex, currentIndex.ToString());
					if (hasSlice)
					{
						if (currentIndex > sliceIndex)
						{
							var offset = listPatternOp.Patterns.Length - currentIndex;
							var subExpr = new NumericLiteral(offset, offset.ToString());
							var lengthId = new Identifier("length");
							var lengthExpr = new MemberExpression(x, lengthId, computed: false, optional: false);
							prop = new NonLogicalBinaryExpression(Operator.Subtraction, lengthExpr, subExpr);
						}
						else if (currentIndex == sliceIndex)
						{
							var afterSlice = listPatternOp.Patterns.Length - currentIndex - 1;
							var sliceId = new Identifier("slice");
							var sliceExpr = new MemberExpression(x, sliceId, computed: false, optional: false);
							if (afterSlice == 0)
							{
								// 切片在末尾，如 [var first, .. var rest]
								// JavaScript: obj.slice(index)
								return new CallExpression(
									sliceExpr,
									NodeList.From<Expression>(new NumericLiteral(currentIndex, currentIndex.ToString())),
									optional: false
								);
							}
							else
							{
								// 切片在中间或开头，需要排除后面的 elementsAfterSlice 个元素
								// 如 [var first, .. var middle, var last] -> obj.slice(1, -1)
								// 如 [.. var rest, var last] -> obj.slice(0, -1)
								// 如 [var a, .. var middle, var b, var c] -> obj.slice(1, -2)
								return new CallExpression(
									sliceExpr,
									NodeList.From<Expression>(
										new NumericLiteral(currentIndex, currentIndex.ToString()),
										new NumericLiteral(-afterSlice, (-afterSlice).ToString())
									),
									optional: false
								);
							}
						}
					}
					return new MemberExpression(x, prop, computed: true, false);
				});
			}
			else if (current is IIsTypeOperation isTypeOp)
			{
				reference = isTypeOp.ValueOperand;
				break;
			}
			else if (current is ISwitchOperation switchOp)
			{
				isInSwitch = true;
				reference = switchOp.Value;
				break;
			}
			else if (current is IIsPatternOperation isPatternOp)
			{
				reference = isPatternOp.Value;
				break;
			}
			else if (current is ISwitchExpressionOperation switchExpressionOp)
			{
				isInSwitch = true;
				reference = switchExpressionOp.Value;
				break;
			}

			// 继续向上
			current = current.Parent;
		}

		if (reference is null)
			return null;


		Expression expr;
		if (isInSwitch)
		{
			//switch的目标值可能是一个复杂表达式，需要创建一个中间变量
			var id = GetUniqueName(reference.Syntax);
			expr = new Identifier(id);
		}
		else
			expr = Translate<Expression>(reference, new());

		while (members.Count > 0)
			expr = members.Pop()(expr);

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

	/// <summary>
	/// 
	/// </summary>
	/// <param name="operation"></param>
	/// <param name="typeSymbol"></param>
	/// <param name="value"></param>
	/// <param name="nullable"></param>
	/// <returns></returns>
	private Expression CreateTypeMatchExpr(IOperation operation, ITypeSymbol typeSymbol, Expression value, bool? nullable = null)
	{
		Expression? result;
		if (typeSymbol.IsTupleType || typeSymbol.IsAnonymousType)
			result = TypeOfExpr(value, new StringLiteral("object", "'object'"));

		else
		{
			var mapper = GetMapperType(typeSymbol, out _);
			result = mapper switch
			{
				TypeMapper.String => TypeOfExpr(value, new StringLiteral("string", "\"string\"")),
				TypeMapper.Number => TypeOfExpr(value, new StringLiteral("number", "\"number\"")),
				TypeMapper.BigInt => TypeOfExpr(value, new StringLiteral("bigint", "\"bigint\"")),
				TypeMapper.Object => TypeOfExpr(value, new StringLiteral("object", "\"object\"")),
				TypeMapper.Boolean => TypeOfExpr(value, new StringLiteral("boolean", "\"boolean\"")),
				TypeMapper.Date => InstanceOfExpr(value, new Identifier("Date")),
				TypeMapper.Map => InstanceOfExpr(value, new Identifier("Map")),
				TypeMapper.Set => InstanceOfExpr(value, new Identifier("Set")),
				TypeMapper.Class => InstanceOfExpr(value, new Identifier(typeSymbol.Name)),
				TypeMapper.Array => new CallExpression(IsArrayExpr, NodeList.From(value), optional: false),
				_ => null
			};
		}

		// 判断可空
		if (nullable ?? IsNullableType(typeSymbol))
		{
			var expr = new NonLogicalBinaryExpression(Operator.StrictEquality, value, Null);
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
	/// 处理位置式解构子模式
	/// </summary>
	private void ProcessPositionalSubpatterns(
		IRecursivePatternOperation operation,
		INamedTypeSymbol namedType,
		Expression targetExpr,
		List<Expression> conditions,
		WalkerArgument argument)
	{
		for (int i = 0; i < operation.DeconstructionSubpatterns.Length; i++)
		{
			var subpattern = operation.DeconstructionSubpatterns[i];

			// 跳过弃元模式 _
			if (subpattern.Kind == OperationKind.DiscardPattern)
				continue;

			// 获取位置对应的属性表达式
			var propertyExpr = GetPositionalPropertyExpression(operation, namedType, targetExpr, i);
			if (propertyExpr is null)
				return;

			// 处理子模式
			Expression? condition;
			if (subpattern is IDeclarationPatternOperation declarationPattern)
			{
				// 声明模式：变量声明（如 var x）
				condition = BuildDeclarationPattern(declarationPattern, propertyExpr, argument);
			}
			else
			{
				// 其他模式：常量、关系等（如 1, > 0）
				var patternResult = Translate<Expression>(subpattern, argument);
				condition = new NonLogicalBinaryExpression(Operator.StrictEquality, propertyExpr, patternResult);
			}

			conditions.Add(condition);
		}
	}

	/// <summary>
	/// 获取位置式解构中指定索引对应的属性访问表达式
	/// </summary>
	private MemberExpression? GetPositionalPropertyExpression(
		IRecursivePatternOperation operation,
		INamedTypeSymbol namedType,
		Expression targetExpr,
		int index)
	{
		string? propertyName = null;

		if (namedType.IsTupleType)
		{
			// 元组类型：使用 TupleElements[index].Name（如 Item1, Item2）
			propertyName = namedType.TupleElements[index].Name;
		}
		else if (namedType.IsRecord)
		{
			// Record 类型：使用 Deconstruct 输出参数名或主构造函数参数名
			propertyName = GetRecordPositionPropertyName(operation, namedType, index);
		}

		if (propertyName is null)
			return null;

		return new MemberExpression(targetExpr, new Identifier(propertyName), computed: false, optional: false);
	}

	/// <summary>
	/// 获取 record 类型位置式解构中指定索引对应的属性名
	/// </summary>
	/// <param name="operation"></param>
	/// <param name="namedType"></param>
	/// <param name="index"></param>
	/// <returns></returns>
	/// <exception cref="OperationTransformationException"></exception>
	private string? GetRecordPositionPropertyName(
		IRecursivePatternOperation operation,
		INamedTypeSymbol namedType,
		int index)
	{
		// 1. 优先使用 Deconstruct 方法的输出参数名
		if (operation.DeconstructSymbol is IMethodSymbol deconstructMethod &&
			deconstructMethod.Parameters.Length > index)
		{
			return deconstructMethod.Parameters[index].Name;
		}

		// 2. 回退到主构造函数参数名
		var constructor = FindMatchingConstructor(namedType, operation.DeconstructionSubpatterns.Length);
		if (constructor is not null && constructor.Parameters.Length > index)
		{
			return constructor.Parameters[index].Name;
		}

		var message = $"Cannot determine property name for record type '{namedType.Name}' at position {index}. " +
			$"Ensure the record has a Deconstruct method or a matching constructor.";
		var location = operation.Syntax.GetLocation();
		_report?.Invoke(location, message);
		throw new OperationTransformationException(operation, message);
	}

	/// <summary>
	/// 查找匹配指定参数数量的构造函数
	/// </summary>
	private IMethodSymbol? FindMatchingConstructor(INamedTypeSymbol namedType, int parameterCount)
	{
		// 优先选择参数数量精确匹配的构造函数
		foreach (var ctor in namedType.Constructors)
		{
			if (!ctor.IsStatic && ctor.Parameters.Length == parameterCount)
				return ctor;
		}

		// 回退到第一个实例构造函数
		return namedType.Constructors.FirstOrDefault(c => !c.IsStatic);
	}

	/// <summary>
	/// 处理包含模式匹配的 switch 语句，转换为 IIFE + if-else 链
	/// C# 示例：
	/// switch (obj) {
	///     case string s when s.Length > 0:
	///         Console.WriteLine(s);
	///         break;
	///     case int i when i > 0:
	///         Console.WriteLine(i);
	///         break;
	///     default:
	///         Console.WriteLine("Default");
	///         break;
	/// }
	/// 转换结果：IIFE + if-else 链
	/// 注意：不支持 goto 语句，如需共享逻辑请提取为方法
	/// </summary>
	private CallExpression VisitSwitchPatternMatching(ISwitchOperation operation, WalkerArgument argument)
	{
		if (Visit(operation.Value, argument) is not Expression discriminant)
			return HandleTransformationFailure<CallExpression>(operation.Value, "Switch discriminant could not be translated to JavaScript.");

		// 创建唯一名称存储 switch 值
		var inputId = new Identifier(GetUniqueName(operation.Value.Syntax));
		var inputVar = new VariableDeclaration(
			VariableDeclarationKind.Const,
			NodeList.From(new VariableDeclarator(inputId, discriminant))
		);

		var statements = new List<Statement>();
		foreach (var switchCase in operation.Cases)
		{
			var hasDefault = false;
			// 收集所有条件表达式
			var conditions = new List<Expression>();
			foreach (var clause in switchCase.Clauses)
			{
				if (clause.CaseKind == CaseKind.Default)
					hasDefault = true;
				else
				{

					var expr = Translate<Expression>(clause, argument);
					if (clause.CaseKind == CaseKind.SingleValue)
						expr = new NonLogicalBinaryExpression(Operator.StrictEquality, inputId, expr);

					conditions.Add(expr);
				}
			}

			// 处理 case 体
			var bodyStatements = new List<Statement>();
			foreach (var bodyOp in switchCase.Body)
			{
				// 此处会下沉检测是否使用了goto
				var node = Visit(bodyOp, argument);
				if (bodyOp.Kind == OperationKind.Branch)
					bodyStatements.Add(new ReturnStatement(null));

				else if (node is Statement stmt)
					bodyStatements.Add(stmt);

				else if (node is Expression expr)
					bodyStatements.Add(new NonSpecialExpressionStatement(expr));
			}

			// 如果有条件
			if (conditions.Count > 0)
			{
				// 组合所有条件（同一个 switchCase 的多个 clause 是 OR 关系）
				Expression combinedCondition = conditions[0];
				for (int i = 1; i < conditions.Count; i++)
				{
					combinedCondition = new LogicalExpression(Operator.LogicalOr, combinedCondition, conditions[i]);
				}

				// 创建 if 语句
				var ifStmt = new IfStatement(combinedCondition, new NestedBlockStatement(NodeList.From(bodyStatements)), null);
				statements.Add(ifStmt);
			}

			if (hasDefault)
				statements.AddRange(bodyStatements);
		}

		// 构造 IIFE
		statements.Insert(0, inputVar);
		var functionBody = new FunctionBody(NodeList.From(statements), strict: true);
		var arrowFunction = new ArrowFunctionExpression(
			NodeList.From<Node>(),
			functionBody,
			expression: false,
			async: false
		);

		return new CallExpression(arrowFunction, NodeList.From<Expression>(), optional: false);
	}

	/// <summary>
	/// 处理声明模式操作
	/// </summary>
	/// <param name="operation">声明模式操作</param>
	/// <param name="value">赋值对象</param>
	/// <param name="argument"></param>
	/// <returns></returns>
	private Expression BuildDeclarationPattern(IDeclarationPatternOperation operation, Expression? value, WalkerArgument argument)
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

		// 存在赋值对象使用赋值对象
		if (operation.MatchedType is not null)
			typeMatchExpr = CreateTypeMatchExpr(operation, operation.MatchedType, value ?? obj);

		if (operation.DeclaredSymbol is not null)
		{
			// 声明模式转换为变量声明
			var id = new Identifier(operation.DeclaredSymbol.Name);
			var declarator = new VariableDeclarator(id, null);
			argument.AddVarDeclarator(declarator, _recursionDepth);

			Expression? assignValueExpr = null;
			if (value is not null)
				assignValueExpr = value;

			else if (operation.Parent is IIsPatternOperation
				or IPatternCaseClauseOperation
				or ISwitchExpressionArmOperation
				or IBinaryPatternOperation
				or ISlicePatternOperation
				or IPropertySubpatternOperation)
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

}
