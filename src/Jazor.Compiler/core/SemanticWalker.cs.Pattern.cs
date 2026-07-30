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
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

/// <summary>
/// 负责把 Roslyn pattern operation 转换为 JavaScript 条件表达式和分支条件。
/// </summary>
/// <remarks>
/// 模式匹配优先使用编译期可证明的信息折叠结果；无法证明时才生成显式 AST 检查。
/// 复杂模式可能需要缓存输入值，以保证源表达式只求值一次并保持原有副作用顺序。
/// </remarks>
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
	public override Node? VisitIsType(IIsTypeOperation operation, SenseArgument argument)
	{
		var value = Translate<Expression>(operation.ValueOperand, argument);
		var result = CreateTypeMatchExpr(operation, operation.TypeOperand, value, context: argument);
		if (operation.IsNegated)
			return new NonUpdateUnaryExpression(Operator.LogicalNot, result);

		return result;
	}

	/// <summary>
	/// 处理 null 检查操作
	/// C# 示例：
	/// obj is null             // 检查是否为 null
	/// value == null           // 直接 null 比较
	/// 转换结果：obj == null
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitIsNull(IIsNullOperation operation, SenseArgument argument)
	{
		// 在 JS host 语义里，null-pattern 需要把 undefined 也视为缺失值，
		// 否则可空 prop / erased union 缺失分支会在 not-null guard 后继续解引用。
		var operand = Translate<Expression>(operation.Operand, argument);

		return new NonLogicalBinaryExpression(Operator.Equality, operand, Null);
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
	public override Node? VisitIsPattern(IIsPatternOperation operation, SenseArgument argument)
	{
		// 获取被测试的值作为 PatternInput
		var inputValue = Translate<Expression>(operation.Value, argument);
		var patternArg = argument.WithPatternInput(inputValue);
		var expr = Translate<Expression>(operation.Pattern, patternArg);
		return Optimizer.OptimizeLogical(expr);
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
	public override Node? VisitPatternCaseClause(IPatternCaseClauseOperation operation, SenseArgument argument)
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
	public override Node? VisitSwitchExpression(ISwitchExpressionOperation operation, SenseArgument argument)
	{
		// 至少有一个分支
		if (operation.Arms.Length < 1)
			return HandleTransformationFailure<Node>(operation, "Switch expression must have at least one arm.");

		var input = Translate<Expression>(operation.Value, argument);

		// 复杂模式匹配switch，生成健全的IIFE保证副作用顺序
		// 采用分层判断：先模式匹配，后when条件，确保求值节拍与C#一致
		var iifeArg = EnsureScopeContext(operation, argument).EnterEmissionScope(operation, ScopeSite.SwitchExpressionIife());
		var statements = new List<Statement>();

		// input 可能是方法调用或一个复杂表达式，此处定义一个中间变量存储其值
		var id = new Identifier(AllocateUniqueName(operation.Value, iifeArg, LoweringSite.SwitchExpressionInput()));
		var inputVar = new VariableDeclaration(
			VariableDeclarationKind.Const,
			NodeList.From(new VariableDeclarator(id, input))
		);

		// 设置 PatternInput 为输入变量，传递给所有 arm
		var armArg = iifeArg.WithPatternInput(id);

		// 处理所有模式，采用嵌套if确保副作用顺序
		foreach (var arm in operation.Arms)
			Translate(statements, arm, armArg);

		statements.Insert(0, inputVar);
		var functionBody = new FunctionBody(NodeList.From(MaterializeScopedStatements(iifeArg, statements)), strict: true);
		var arrowFunction = new ArrowFunctionExpression(
			NodeList.From<Node>(),
			functionBody,
			expression: false,
			async: ContainsAwaitOperation(operation)
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
	public override Node? VisitSwitchExpressionArm(ISwitchExpressionArmOperation operation, SenseArgument argument)
	{
		var value = Translate<Expression>(operation.Value, argument);

		// Discard only becomes unconditional when it has no guard.
		if (operation.Pattern.Kind == OperationKind.DiscardPattern)
		{
			if (operation.Guard is null)
				return new ReturnStatement(value);

			var guard = Translate<Expression>(operation.Guard, argument);
			return new IfStatement(guard, new ReturnStatement(value), null);
		}

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
	/// // 属性模式（显式 class，保留 nominal runtime type check）
	/// obj is Person { Name: "John", Age: > 18 }
	/// // → obj instanceof Person && obj.Name === "John" && obj.Age > 18
	///
	/// // 位置式元组模式
	/// value is (int x, string y)
	/// // → typeof value === "object" && value.Item1 !== undefined && value.Item2 !== undefined
	///
	/// // 位置式 record 模式（structural lowering）
	/// obj is Person("John", 18)
	/// // → obj.name === "John" && obj.age === 18
	///
	/// // 混合模式（显式 class）
	/// data is Point(int x, int y) { Z: > 0 }
	/// // → data instanceof Point && positional match && data.Z > 0
	/// </code>
	/// </remarks>
	/// <param name="operation">递归模式操作</param>
	/// <param name="argument">用于存放临时变量定义的上下文</param>
	/// <returns>JavaScript组合条件表达式（使用&amp;&amp;连接所有条件）</returns>
	public override Node? VisitRecursivePattern(IRecursivePatternOperation operation, SenseArgument argument)
	{
		var conditions = new List<Expression>();
		var targetExpr = GetPatternRefrence(operation, argument);
		var patternArgument = argument;
		targetExpr = StabilizePatternExpression(operation, targetExpr, argument, "recursive", out var targetInitialization);
		if (targetInitialization is not null)
			patternArgument = argument.WithPatternInput(targetExpr);

		// 类型匹配条件（排除匿名类型、元组类型、object）
		if (!operation.MatchedType.IsAnonymousType &&
			!operation.MatchedType.IsTupleType &&
			!(operation.MatchedType is INamedTypeSymbol matchedNamedType && IsStructuralType(matchedNamedType)) &&
			operation.MatchedType.SpecialType != SpecialType.System_Object &&
			!IsRecursivePatternTypeMatchStaticallyTrue(operation))
		{
			var typeCheck = CreateTypeMatchExpr(operation, operation.MatchedType, targetExpr, context: patternArgument);
			conditions.Add(typeCheck);
		}

		// 属性子模式（命名属性，如 { Name: "John" }）
		if (operation.PropertySubpatterns.Length > 0)
		{
			conditions.Add(new NonLogicalBinaryExpression(Operator.Inequality, targetExpr, Null));

			foreach (var propertySubpattern in operation.PropertySubpatterns)
			{
				if (propertySubpattern.Member is IMemberReferenceOperation m)
				{
					var propertyAccess = BuildPatternMemberAccess(m, targetExpr, patternArgument, out var existencePropertyName);
					var propertyArg = patternArgument.WithPatternInput(propertyAccess);
					var right = Translate<Expression>(propertySubpattern.Pattern, propertyArg);
					if (!string.IsNullOrEmpty(existencePropertyName))
					{
						var exists = new NonLogicalBinaryExpression(
							Operator.In,
							CreateStringLiteral(existencePropertyName!),
							targetExpr);
						conditions.Add(exists);
					}

					conditions.Add(right);
				}
				else
					conditions.Add(Translate<Expression>(propertySubpattern, patternArgument.WithPatternInput(targetExpr)));
			}
		}

		// 位置式解构子模式（如 (int x, string y) 或 Person("John", 18)）
		if (operation.DeconstructionSubpatterns.Length > 0)
		{
			if (operation.InputType is not INamedTypeSymbol namedType)
				return HandleTransformationFailure<Node>(operation, $"Input type '{operation.InputType}' is not a named type for deconstruction pattern.");

			ProcessPositionalSubpatterns(operation, namedType, targetExpr, conditions, patternArgument);
		}

		if (conditions.Count == 0)
			conditions.Add(BuildRecursivePatternFallbackMatch(operation, targetExpr));

		if (operation.DeclaredSymbol is not null)
			conditions.Add(BuildPatternDeclaredSymbolAssignment(operation.DeclaredSymbol, targetExpr, operation, argument));

		var result = conditions[0];
		for (int i = 1; i < conditions.Count; i++)
			result = new LogicalExpression(Operator.LogicalAnd, result, conditions[i]);
		return PrependEvaluation(targetInitialization, result);
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
	public override Node? VisitConstantPattern(IConstantPatternOperation operation, SenseArgument argument)
	{
		var expr = Translate<Expression>(operation.Value, argument);

		// 如果有 PatternInput，生成比较表达式
		if (argument.PatternInput is not null)
		{
			var @operator = expr is Literal literal && literal.Kind == TokenKind.NullLiteral
				? Operator.Equality
				: Operator.StrictEquality;
			return new NonLogicalBinaryExpression(@operator, argument.PatternInput, expr);
		}

		// 如果没有 PatternInput，直接返回值表达式（可能在某些特殊场景下）
		return expr;
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
	public override Node? VisitDeclarationPattern(IDeclarationPatternOperation operation, SenseArgument argument)
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
	public override Node? VisitDiscardPattern(IDiscardPatternOperation operation, SenseArgument argument)
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
	public override Node? VisitPropertySubpattern(IPropertySubpatternOperation operation, SenseArgument argument)
	{
		var obj = GetPatternRefrence(operation, argument);
		if (operation.Member is not IMemberReferenceOperation memberReference)
		{
			return HandleTransformationFailure<Node>(
				operation,
				$"属性子模式的成员不是有效的成员引用：{operation.Member?.Kind}");
		}

		var propertyAccess = BuildPatternMemberAccess(memberReference, obj, argument, out _);
		var patternArg = argument.WithPatternInput(propertyAccess);
		return Translate<Expression>(operation.Pattern, patternArg);
	}

	private Expression BuildPatternMemberAccess(
		IMemberReferenceOperation memberReference,
		Expression targetExpr,
		SenseArgument argument,
		out string? existencePropertyName)
	{
		existencePropertyName = null;
		var symbol = GetWhiteListSymbol(memberReference);
		var mapperExpr = GetWhiteListExpression(symbol, argument, [], targetExpr, out var alias, memberReference);
		if (mapperExpr is not null)
			return mapperExpr;

		if (!string.IsNullOrEmpty(alias))
		{
			if (memberReference is IPropertyReferenceOperation)
			{
				existencePropertyName = alias!;
				return BuildAliasedPropertyAccess(targetExpr, alias!, optional: false);
			}

			existencePropertyName = alias!;
			return new MemberExpression(targetExpr, new Identifier(alias!), computed: false, optional: false);
		}

		RejectUnsupportedRuntimeFallback(memberReference, symbol, "pattern property access", memberReference.Instance?.Type ?? memberReference.Member.ContainingType);
		existencePropertyName = Util.GetConfigOrSymbolName(memberReference.Member);
		return new MemberExpression(targetExpr, new Identifier(existencePropertyName), computed: false, optional: false);
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
	public override Node? VisitNegatedPattern(INegatedPatternOperation operation, SenseArgument argument)
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
	public override Node? VisitBinaryPattern(IBinaryPatternOperation operation, SenseArgument argument)
	{
		// 二元模式的条件判断转换
		// C# 示例：value is > 0 and < 100 是一个布尔条件表达式
		//         obj is string or int 检查对象是否为指定类型中的任意一种
		// 转换结果：生成相应的JavaScript逻辑表达式

		// 访问左右两个子模式
		var patternArgument = argument;
		Expression? patternInitialization = null;
		if (argument.PatternInput is not null)
		{
			var stabilizedInput = StabilizePatternExpression(operation, argument.PatternInput, argument, "binary", out patternInitialization);
			if (patternInitialization is not null)
				patternArgument = argument.WithPatternInput(stabilizedInput);
		}

		var left = Translate<Expression>(operation.LeftPattern, patternArgument);
		var right = Translate<Expression>(operation.RightPattern, patternArgument);

		// 检查模式的类型来确定操作符
		var @operator = operation.OperatorKind switch
		{
			BinaryOperatorKind.And => Operator.LogicalAnd,
			BinaryOperatorKind.Or => Operator.LogicalOr,
			_ => Operator.Unknown
		};

		if (@operator == Operator.Unknown)
			return HandleTransformationFailure<Node>(operation, "Unsupported binary operator in pattern.");

		return PrependEvaluation(patternInitialization, new LogicalExpression(@operator, left, right));
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
		public override Node? VisitRelationalPattern(IRelationalPatternOperation operation, SenseArgument argument)
	{
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

		// 稳定化 PatternInput 以避免副作用表达式重复求值
		var targetExpr = GetPatternRefrence(operation, argument);
		targetExpr = StabilizePatternExpression(
			operation,
			targetExpr,
			argument,
			"relational",
			out var initialization,
			cacheMemberAccess: false);

		// 获取右操作数（比较值）
		var right = Translate<Expression>(operation.Value, argument);

		var result = new NonLogicalBinaryExpression(@operator, targetExpr, right);
		return PrependEvaluation(initialization, result);
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
	public override Node? VisitListPattern(IListPatternOperation operation, SenseArgument argument)
	{
		var obj = GetPatternRefrence(operation, argument);
		obj = StabilizePatternExpression(operation, obj, argument, "list", out var listInitialization);
		var hostType = operation.InputType ?? operation.NarrowedType;
		var isIntrinsicArrayCarrier = hostType?.TypeKind == TypeKind.Array;
		Expression result = BuildListPatternCarrierCheck(operation, obj, argument);
		var lengthExpr = BuildListPatternLengthAccess(operation, obj, argument, isIntrinsicArrayCarrier, hostType);
		var usesLengthMultipleTimes = ListPatternUsesLengthMultipleTimes(operation);
		var shouldCacheLength = usesLengthMultipleTimes && !IsPureListPatternLengthAccess(operation, isIntrinsicArrayCarrier);
		Expression? lengthInitialization;
		if (shouldCacheLength)
			lengthExpr = StabilizePatternExpression(operation, lengthExpr, argument, "listlen", out lengthInitialization);
		else
			lengthInitialization = null;

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
					Expression indexExpr = new NumericLiteral(i, i.ToString());
					if (hasSlice && i > sliceIndex)
					{
						var offset = operation.Patterns.Length - i;
						var subExpr = new NumericLiteral(offset, offset.ToString());
						indexExpr = new NonLogicalBinaryExpression(Operator.Subtraction, lengthExpr, subExpr);
					}

					var indexAccess = BuildListPatternIndexerAccess(operation, obj, indexExpr, argument, isIntrinsicArrayCarrier, hostType);
					Expression? expr;
					if (pattern is IDeclarationPatternOperation declarationPatternOp)
						expr = BuildDeclarationPattern(declarationPatternOp, indexAccess, argument);
					else
					{
						// 为子模式传递 indexAccess 作为 PatternInput
						var patternArg = argument.WithPatternInput(indexAccess);
						expr = Translate<Expression>(pattern, patternArg);
					}

					if (expr is not null)
						result = new LogicalExpression(Operator.LogicalAnd, result, expr);
				}
				else if (pattern is ISlicePatternOperation slicePattern)
				{
					// 切片模式需要构建 slice 表达式
					// slice(startIndex, endIndex)
					// - 如果切片在最前面 [.. var rest]: slice(0)
					// - 如果切片在中间 [var a, .. var rest]: slice(1)
					// - 如果切片在中间且后面还有元素 [var a, .. var rest, var last]: slice(1, -1)

					if (slicePattern.Pattern is null)
						continue;

					var sliceExpr = BuildListPatternSliceAccess(
						operation,
						slicePattern,
						obj,
						lengthExpr,
						sliceIndex,
						argument,
						isIntrinsicArrayCarrier,
						hostType);

					// 处理切片模式的子模式（通常是声明模式）
					Expression? expr;
					if (slicePattern.Pattern is IDeclarationPatternOperation declarationPatternOp)
					{
						expr = BuildDeclarationPattern(declarationPatternOp, sliceExpr, argument);
					}
					else
					{
						// 其他情况：传递切片表达式作为 PatternInput
						var patternArg = argument.WithPatternInput(sliceExpr);
						expr = Translate<Expression>(slicePattern.Pattern, patternArg);
					}

					if (expr is not null)
						result = new LogicalExpression(Operator.LogicalAnd, result, expr);
				}
				else
				{
					// 嵌套列表模式或其他模式
					// 计算索引访问表达式作为 PatternInput
					Expression indexExpr = new NumericLiteral(i, i.ToString());
					if (hasSlice && i > sliceIndex)
					{
						var offset = operation.Patterns.Length - i;
						var subExpr = new NumericLiteral(offset, offset.ToString());
						indexExpr = new NonLogicalBinaryExpression(Operator.Subtraction, lengthExpr, subExpr);
					}

					var indexAccess = BuildListPatternIndexerAccess(operation, obj, indexExpr, argument, isIntrinsicArrayCarrier, hostType);
					var patternArg = argument.WithPatternInput(indexAccess);
					var expr = Translate<Expression>(pattern, patternArg);
					if (expr is not null)
						result = new LogicalExpression(Operator.LogicalAnd, result, expr);
				}
			}
		}

		if (operation.DeclaredSymbol is not null)
			result = new LogicalExpression(
				Operator.LogicalAnd,
				result,
				BuildPatternDeclaredSymbolAssignment(operation.DeclaredSymbol, obj, operation, argument));

		return PrependEvaluation(listInitialization, PrependEvaluation(lengthInitialization, result));
	}

	private static Expression BuildRecursivePatternFallbackMatch(IRecursivePatternOperation operation, Expression targetExpr)
	{
		if (operation.MatchedType?.IsValueType == true &&
			operation.MatchedType.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
		{
			return new BooleanLiteral(true, "true");
		}

		return new NonLogicalBinaryExpression(Operator.Inequality, targetExpr, Null);
	}

	private static bool IsRecursivePatternTypeMatchStaticallyTrue(IRecursivePatternOperation operation)
	{
		var inputType = operation.InputType;
		// Exact non-nullable value inputs cannot fail the C# type test. Carrier-backed values still
		// need their inferred runtime discriminator because the carrier is part of the CLR mapping contract.
		return inputType?.IsValueType == true &&
			!IsNullableType(inputType) &&
			!TryGetWhiteListRuntimeValueCarrier(inputType, out _) &&
			SymbolEqualityComparer.Default.Equals(inputType, operation.MatchedType);
	}

	private Expression BuildListPatternCarrierCheck(IListPatternOperation operation, Expression targetExpr, SenseArgument argument)
	{
		var carrierType = operation.InputType ?? operation.NarrowedType;
		if (carrierType is null)
			return new NonLogicalBinaryExpression(Operator.Inequality, targetExpr, Null);

		var mapper = GetMapperType(carrierType).Mapper;
		if (mapper is TypeMapper.Array or TypeMapper.String)
			return CreateTypeMatchExpr(operation, carrierType, targetExpr, context: argument);

		return new NonLogicalBinaryExpression(Operator.Inequality, targetExpr, Null);
	}

	private static bool ListPatternUsesLengthMultipleTimes(IListPatternOperation operation)
	{
		var sliceIndex = -1;
		for (var i = 0; i < operation.Patterns.Length; i++)
		{
			if (operation.Patterns[i].Kind == OperationKind.SlicePattern)
			{
				sliceIndex = i;
				break;
			}
		}

		if (sliceIndex < 0)
			return false;

		if (sliceIndex < operation.Patterns.Length - 1)
			return true;

		var slicePattern = (ISlicePatternOperation)operation.Patterns[sliceIndex];
		return slicePattern.Pattern is not null &&
			slicePattern.Pattern.Kind != OperationKind.DiscardPattern;
	}

	private bool IsPureListPatternLengthAccess(IListPatternOperation operation, bool isIntrinsicArrayCarrier)
	{
		if (isIntrinsicArrayCarrier)
			return true;

		if (operation.LengthSymbol is null)
			return false;

		var lookupSymbol = operation.LengthSymbol is IPropertySymbol { GetMethod: not null } property
			? (ISymbol)property.GetMethod!
			: operation.LengthSymbol;

		return TryGetWhiteListValue(WhiteList.Members, lookupSymbol, out _, out var entry) &&
			entry.Op == ECMAScript.Contract.Op.Alias &&
			string.Equals(entry.Value, "length", StringComparison.Ordinal);
	}

	private Expression BuildListPatternLengthAccess(
		IListPatternOperation operation,
		Expression targetExpr,
		SenseArgument argument,
		bool isIntrinsicArrayCarrier,
		ITypeSymbol? hostType)
	{
		if (isIntrinsicArrayCarrier)
			return new MemberExpression(targetExpr, new Identifier("length"), computed: false, optional: false);

		if (operation.LengthSymbol is null)
		{
			return HandleTransformationFailure<Expression>(
				operation,
				$"List pattern on '{hostType?.OriginalDefinition.ToDisplayString(Format.NameFormat) ?? "<unknown>"}' requires a supported length/count symbol.");
		}

		return BuildListPatternBoundAccess(
			operation,
			operation.LengthSymbol,
			targetExpr,
			[],
			argument,
			"list pattern length access",
			hostType ?? operation.LengthSymbol.ContainingType);
	}

	private Expression BuildListPatternIndexerAccess(
		IListPatternOperation operation,
		Expression targetExpr,
		Expression indexExpr,
		SenseArgument argument,
		bool isIntrinsicArrayCarrier,
		ITypeSymbol? hostType)
	{
		if (isIntrinsicArrayCarrier)
			return new MemberExpression(targetExpr, indexExpr, computed: true, optional: false);

		if (operation.IndexerSymbol is null)
		{
			return HandleTransformationFailure<Expression>(
				operation,
				$"List pattern on '{hostType?.OriginalDefinition.ToDisplayString(Format.NameFormat) ?? "<unknown>"}' requires a supported indexer symbol.");
		}

		return BuildListPatternBoundAccess(
			operation,
			operation.IndexerSymbol,
			targetExpr,
			[indexExpr],
			argument,
			"list pattern index access",
			hostType ?? operation.IndexerSymbol.ContainingType);
	}

	private Expression BuildListPatternSliceAccess(
		IListPatternOperation listPattern,
		ISlicePatternOperation slicePattern,
		Expression targetExpr,
		Expression lengthExpr,
		int sliceIndex,
		SenseArgument argument,
		bool isIntrinsicArrayCarrier,
		ITypeSymbol? hostType)
	{
		var startExpr = new NumericLiteral(sliceIndex, sliceIndex.ToString());
		var elementsAfterSlice = listPattern.Patterns.Length - sliceIndex - 1;

		if (isIntrinsicArrayCarrier)
			return BuildIntrinsicArraySliceAccess(targetExpr, startExpr, elementsAfterSlice);

		if (slicePattern.SliceSymbol is null)
		{
			return HandleTransformationFailure<Expression>(
				slicePattern,
				$"Slice pattern on '{hostType?.OriginalDefinition.ToDisplayString(Format.NameFormat) ?? "<unknown>"}' requires a supported slice symbol.");
		}

		if (slicePattern.SliceSymbol is IPropertySymbol sliceProperty)
		{
			return HandleTransformationFailure<Expression>(
				slicePattern,
				$"Range-based slice property '{sliceProperty.OriginalDefinition.ToDisplayString(Format.NameFormat)}' is not supported in list pattern lowering. Expose a Slice(int, int) member or configure a whitelist mapping.");
		}

		if (slicePattern.SliceSymbol is not IMethodSymbol sliceMethod)
		{
			return HandleTransformationFailure<Expression>(
				slicePattern,
				$"Unsupported slice symbol kind '{slicePattern.SliceSymbol.Kind}' in list pattern.");
		}

		var sliceArguments = BuildListPatternSliceMethodArguments(
			slicePattern,
			sliceMethod,
			startExpr,
			lengthExpr,
			sliceIndex,
			elementsAfterSlice);
		return BuildMethodCallExpression(
			slicePattern,
			sliceMethod,
			slicePattern.Syntax,
			slicePattern.SemanticModel,
			targetExpr,
			sliceArguments,
			argument,
			hostType ?? sliceMethod.ContainingType);
	}

	private static Expression BuildIntrinsicArraySliceAccess(Expression targetExpr, Expression startExpr, int elementsAfterSlice)
	{
		var sliceMethod = new MemberExpression(targetExpr, new Identifier("slice"), computed: false, optional: false);
		if (elementsAfterSlice == 0)
		{
			return new CallExpression(
				sliceMethod,
				NodeList.From(startExpr),
				optional: false);
		}

		var endExpr = new NumericLiteral(-elementsAfterSlice, (-elementsAfterSlice).ToString());
		return new CallExpression(
			sliceMethod,
			NodeList.From<Expression>(startExpr, endExpr),
			optional: false);
	}

	private List<Expression> BuildListPatternSliceMethodArguments(
		IOperation ownerOperation,
		IMethodSymbol method,
		Expression startExpr,
		Expression lengthExpr,
		int sliceIndex,
		int elementsAfterSlice)
	{
		if (method.Parameters.Length != 2 ||
			method.Parameters[0].Type.OriginalDefinition.SpecialType != SpecialType.System_Int32 ||
			method.Parameters[1].Type.OriginalDefinition.SpecialType != SpecialType.System_Int32)
		{
			return HandleUnsupportedSliceArguments(
				ownerOperation,
				$"Slice method '{method.OriginalDefinition.ToDisplayString(Format.NameFormat)}' must expose Slice(int, int) semantics for list pattern lowering.");
		}

		return
		[
			startExpr,
			BuildListPatternSliceLengthExpression(lengthExpr, sliceIndex, elementsAfterSlice)
		];
	}

	private Expression BuildListPatternSliceLengthExpression(Expression lengthExpr, int sliceIndex, int elementsAfterSlice)
	{
		var fixedElements = sliceIndex + elementsAfterSlice;
		if (fixedElements == 0)
			return lengthExpr;

		var fixedExpr = new NumericLiteral(fixedElements, fixedElements.ToString());
		return new NonLogicalBinaryExpression(Operator.Subtraction, lengthExpr, fixedExpr);
	}

	private Expression BuildListPatternBoundAccess(
		IOperation ownerOperation,
		ISymbol symbol,
		Expression targetExpr,
		List<Expression> arguments,
		SenseArgument argument,
		string usage,
		ITypeSymbol? hostType)
	{
		var lookupSymbol = symbol is IPropertySymbol { GetMethod: not null } propertyForLookup
			? (ISymbol)propertyForLookup.GetMethod!
			: symbol;
		var mapperExpr = GetWhiteListExpression(lookupSymbol, argument, arguments, targetExpr, out var alias, ownerOperation, hostType);
		if (mapperExpr is not null)
			return mapperExpr;

		if (lookupSymbol is IMethodSymbol { AssociatedSymbol: IPropertySymbol associatedProperty } accessorMethod)
		{
			return BuildListPatternPropertyAccess(
				ownerOperation,
				accessorMethod,
				associatedProperty,
				targetExpr,
				arguments,
				alias,
				usage,
				hostType ?? associatedProperty.ContainingType);
		}

		// 有效 list pattern 的 Length/Count 与 indexer 都是属性；上面已把它们归一化为 getter。
		// 这里剩余的 method 形态由 Roslyn 的绑定结果直接提供，不接受 field/raw-property 猜测。
		if (symbol is IMethodSymbol method)
		{
			return BuildMethodCallExpression(
				ownerOperation,
				method,
				ownerOperation.Syntax,
				ownerOperation.SemanticModel,
				targetExpr,
				arguments,
				argument,
				hostType ?? method.ContainingType);
		}

		return HandleTransformationFailure<Expression>(
			ownerOperation,
			$"Unsupported symbol kind '{symbol.Kind}' for {usage}.");
	}

	private Expression BuildListPatternPropertyAccess(
		IOperation ownerOperation,
		ISymbol lookupSymbol,
		IPropertySymbol property,
		Expression targetExpr,
		List<Expression> arguments,
		string? alias,
		string usage,
		ITypeSymbol? hostType)
	{
		if (property.IsIndexer || property.Parameters.Length > 0)
		{
			if (arguments.Count != 1)
			{
				return HandleTransformationFailure<Expression>(
					ownerOperation,
					$"{usage} cannot fall back to raw JavaScript member access because '{property.OriginalDefinition.ToDisplayString(Format.NameFormat)}' requires {arguments.Count} translated arguments.");
			}

			if (string.IsNullOrEmpty(alias))
				RejectUnsupportedRuntimeFallback(ownerOperation, lookupSymbol, usage, hostType ?? property.ContainingType);

			return new MemberExpression(targetExpr, arguments[0], computed: true, optional: false);
		}

		if (string.IsNullOrEmpty(alias))
			RejectUnsupportedRuntimeFallback(ownerOperation, lookupSymbol, usage, hostType ?? property.ContainingType);

		var propertyName = string.IsNullOrEmpty(alias)
			? Util.GetConfigOrSymbolName(property)
			: alias;
		return new MemberExpression(targetExpr, new Identifier(propertyName!), computed: false, optional: false);
	}

	private List<Expression> HandleUnsupportedSliceArguments(IOperation operation, string message)
	{
		HandleTransformationFailure<Node>(operation, message);
		return [];
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
	public override Node? VisitSlicePattern(ISlicePatternOperation operation, SenseArgument argument)
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
	public override Node? VisitTypePattern(ITypePatternOperation operation, SenseArgument argument)
	{
		// ITypePatternOperation 只进行类型检查，不声明变量
		// 使用 MatchedType 而非 InputType，因为我们要检查的是目标类型
		var matchedType = operation.MatchedType;
		var targetExpr = GetPatternRefrence(operation, argument);

		// 稳定化 PatternInput 以避免副作用表达式重复求值
		targetExpr = StabilizePatternExpression(operation, targetExpr, argument, "typepattern", out var initialization);

		// 复用已有的类型匹配表达式生成方法
		// CreateTypeMatchExpr 已正确处理：
		//   - 基本类型映射（string/number/boolean/bigint）
		//   - 引用类型映射（Date/Map/Set/Class）
		//   - 数组类型检查（Array.isArray）
		//   - 可空类型处理（nullable 包含 null 检查）
		var result = CreateTypeMatchExpr(operation, matchedType, targetExpr, context: argument);
		return PrependEvaluation(initialization, result);
	}

	private static bool IsNullableType(ITypeSymbol? type)
		=> type is INamedTypeSymbol namedType && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;

	/// <summary>
	/// 获取模式匹配的目标表达式。
	/// 必须通过 SenseArgument.PatternInput 提供目标表达式。
	/// </summary>
	/// <param name="operation">模式相关操作</param>
	/// <param name="context">上下文信息（必须包含 PatternInput）</param>
	/// <returns>目标表达式</returns>
	/// <exception cref="InvalidOperationException">当 PatternInput 未提供时抛出</exception>
	private Expression GetPatternRefrence(IOperation operation, SenseArgument context)
	{
		// 必须提供 PatternInput
		if (context.PatternInput is null)
		{
			var location = operation.Syntax.GetLocation();
			var message = $"模式匹配需要 PatternInput，但未提供。操作类型：{operation.Kind}。请检查调用点是否正确传递了 PatternInput。";
			_report?.Invoke(location, message);
			throw new InvalidOperationException(message);
		}

		return context.PatternInput;
	}

	private static bool IsPurePropertyAccessChain(Expression expression)
		=> expression switch
		{
			Identifier or ThisExpression or Super => true,
			MemberExpression member when !member.Optional &&
				IsPurePropertyAccessChain((Expression)member.Object) &&
				((!member.Computed && member.Property is Identifier) ||
				 (member.Computed && member.Property is Literal)) => true,
			_ => false
		};

	private Expression StabilizePatternExpression(
		IOperation ownerOperation,
		Expression expression,
		SenseArgument argument,
		string slot,
		out Expression? initialization,
		bool cacheMemberAccess = true)
	{
		initialization = null;
		// Member access can invoke a C# getter or a JavaScript Proxy/getter. Only a caller
		// that emits exactly one read may opt out of caching a syntactically stable chain.
		if (!NeedsSingleEvaluationCaching(expression) ||
			(!cacheMemberAccess && IsPurePropertyAccessChain(expression)))
			return expression;

		var tempId = new Identifier(AllocateUniqueName(ownerOperation, argument, LoweringSite.PatternInputCache(slot)));
		argument.AddVarDeclarator(new VariableDeclarator(tempId, null), _recursionDepth);
		initialization = new AssignmentExpression(Operator.Assignment, tempId, expression);
		return tempId;
	}

	private static Expression PrependEvaluation(Expression? initialization, Expression expression)
		=> initialization is null
			? expression
			: new SequenceExpression(NodeList.From<Expression>(initialization, expression));

	/// <summary>
	/// 
	/// </summary>
	/// <param name="operation"></param>
	/// <param name="typeSymbol"></param>
	/// <param name="value"></param>
	/// <param name="context"></param>
	/// <returns></returns>
	private Expression CreateTypeMatchExpr(IOperation operation, ITypeSymbol typeSymbol, Expression value, SenseArgument context)
	{
		value = StabilizePatternExpression(operation, value, context, "type", out var initialization);

		Expression? result;
		if (typeSymbol.IsTupleType || typeSymbol.IsAnonymousType)
			result = new LogicalExpression(
				Operator.LogicalAnd,
				new NonLogicalBinaryExpression(Operator.StrictInequality, value, Null),
				TypeOfExpr(value, CreateStringLiteral("object")));

		else if (TryGetWhiteListRuntimeValueCarrier(typeSymbol, out var runtimeValueCarrier))
		{
			var carrierConstructor = context.BindImportSpecifier(runtimeValueCarrier.Path, runtimeValueCarrier.Name);
			result = InstanceOfExpr(value, carrierConstructor);
		}
		else
		{
			var (mapper, typeName) = GetMapperType(typeSymbol);

			if (typeSymbol.SpecialType == SpecialType.System_Object)
			{
				result = new NonLogicalBinaryExpression(Operator.Inequality, value, Null);
			}
			// Interface types aliased to Object do not carry a reliable runtime discriminator in JS.
			// For these cases, only fold at compile-time when Roslyn metadata can prove the outcome.
			// If not provable, keep the explicit unsupported boundary instead of producing unsound runtime checks.
			else if (typeSymbol.TypeKind == TypeKind.Interface && mapper == TypeMapper.Object)
			{
				if (TryEvaluateCompileTimeErasedInterfaceIsTypeCheck(operation, typeSymbol, out var folded))
				{
					result = folded switch
					{
						InterfaceTypeCheckFold.AlwaysTrue => new BooleanLiteral(true, "true"),
						InterfaceTypeCheckFold.AlwaysFalse => new BooleanLiteral(false, "false"),
						InterfaceTypeCheckFold.NonNullOnly => new NonLogicalBinaryExpression(Operator.Inequality, value, Null),
						_ => null
					};
				}
				else
					return HandleTransformationFailure<Expression>(operation, BuildUnsupportedErasedInterfaceIsTypeCheckMessage(operation, typeSymbol));
			}
			else if (mapper == TypeMapper.Object && TryGetWhiteListTypeAlias(typeSymbol, out var runtimeAlias) && runtimeAlias == "Object")
			{
				return HandleTransformationFailure<Expression>(
					operation,
					$"Type '{typeSymbol.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' uses an erased Object alias without an inferred Jazor.CLR runtime carrier, so its runtime type test cannot be lowered soundly.");
			}
			else
			{
				result = mapper switch
				{
					TypeMapper.String => TypeOfExpr(value, CreateStringLiteral("string")),
					TypeMapper.Number => TypeOfExpr(value, CreateStringLiteral("number")),
					TypeMapper.BigInt => TypeOfExpr(value, CreateStringLiteral("bigint")),
					TypeMapper.Object => new LogicalExpression(
						Operator.LogicalAnd,
						new NonLogicalBinaryExpression(Operator.Inequality, value, Null),
						TypeOfExpr(value, CreateStringLiteral("object"))),
					TypeMapper.Boolean => TypeOfExpr(value, CreateStringLiteral("boolean")),
					TypeMapper.Date => InstanceOfExpr(value, new Identifier("Date")),
					TypeMapper.Map => InstanceOfExpr(value, new Identifier("Map")),
					TypeMapper.Set => InstanceOfExpr(value, new Identifier("Set")),
					TypeMapper.Array => new CallExpression(IsArrayExpr, NodeList.From(value), optional: false),
					TypeMapper.Class => BuildClassTypeMatch(operation, typeSymbol, value, typeName, context),
					_ => null
				};
			}
		}

		if (result is null)
		{
			var mapped = GetMapperType(typeSymbol);
			return HandleTransformationFailure<Expression>(
				operation,
				$"Unsupported type in is-type operation. Target='{typeSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat)}', Mapper='{mapped.Mapper}', RuntimeName='{mapped.TypeName}'.");
		}

		return PrependEvaluation(initialization, result);

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

	private enum InterfaceTypeCheckFold
	{
		AlwaysTrue,
		AlwaysFalse,
		NonNullOnly
	}

	private bool TryEvaluateCompileTimeErasedInterfaceIsTypeCheck(IOperation operation, ITypeSymbol interfaceType, out InterfaceTypeCheckFold result)
	{
		result = InterfaceTypeCheckFold.AlwaysFalse;
		if (interfaceType.TypeKind != TypeKind.Interface)
			return false;

		var sourceOperation = ResolveIsTypeSourceOperation(operation);
		var resolvedSource = sourceOperation is null
			? null
			: ResolveSingleAssignmentValueSource(sourceOperation, operation);

		if (resolvedSource is not null &&
			TryResolveDeterministicRuntimeValue(resolvedSource, out var runtimeType, out var definitelyNonNull))
		{
			// null is never an instance of interface types.
			if (runtimeType is null)
			{
				result = InterfaceTypeCheckFold.AlwaysFalse;
				return true;
			}

			var assignable = IsRuntimeTypeAssignableToInterface(runtimeType, interfaceType);
			if (assignable)
				result = definitelyNonNull
					? InterfaceTypeCheckFold.AlwaysTrue
					: InterfaceTypeCheckFold.NonNullOnly;
			else if (definitelyNonNull)
				result = InterfaceTypeCheckFold.AlwaysFalse;
			else
				return false;

			return true;
		}

		// If deterministic runtime value is unavailable, fall back to statically-known
		// input type only. Unprovable scenarios stay explicit unsupported.
		var staticType = resolvedSource?.Type ?? ResolvePatternInputStaticType(operation);
		if (staticType is null || !IsRuntimeTypeAssignableToInterface(staticType, interfaceType))
			return false;

		result = staticType.IsValueType && !IsNullableType(staticType)
			? InterfaceTypeCheckFold.AlwaysTrue
			: InterfaceTypeCheckFold.NonNullOnly;
		return true;
	}

	private static bool IsRuntimeTypeAssignableToInterface(ITypeSymbol runtimeType, ITypeSymbol interfaceType)
	{
		if (interfaceType.TypeKind != TypeKind.Interface)
			return false;

		if (SymbolEqualityComparer.Default.Equals(runtimeType, interfaceType))
			return true;

		if (runtimeType is INamedTypeSymbol namedRuntime)
			return namedRuntime.AllInterfaces.Any(candidate =>
				SymbolEqualityComparer.Default.Equals(candidate, interfaceType));

		if (runtimeType is not ITypeParameterSymbol typeParameter)
			return false;

		// Generic constraints are the only runtime contract available for T. Follow T : U
		// chains as Roslyn symbols and stop on cycles instead of guessing from emitted values.
		var pending = new Stack<ITypeParameterSymbol>();
		var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
		pending.Push(typeParameter);
		while (pending.Count > 0)
		{
			foreach (var constraint in pending.Pop().ConstraintTypes)
			{
				if (!visited.Add(constraint))
					continue;

				if (SymbolEqualityComparer.Default.Equals(constraint, interfaceType))
					return true;

				if (constraint is INamedTypeSymbol namedConstraint &&
					namedConstraint.AllInterfaces.Any(candidate =>
						SymbolEqualityComparer.Default.Equals(candidate, interfaceType)))
				{
					return true;
				}

				if (constraint is ITypeParameterSymbol chainedTypeParameter)
					pending.Push(chainedTypeParameter);
			}
		}

		return false;
	}

	private IOperation ResolveSingleAssignmentValueSource(IOperation sourceOperation, IOperation useSiteOperation)
	{
		var current = UnwrapImplicitConversions(sourceOperation);
		var localCycleGuard = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);

		while (current is ILocalReferenceOperation localReference)
		{
			var local = localReference.Local;
			if (local is null || !localCycleGuard.Add(local))
				break;

			if (!TryResolveSingleAssignmentLocalInitializer(localReference, useSiteOperation, out var initializerValue))
				break;

			current = UnwrapImplicitConversions(initializerValue);
		}

		return current;
	}

	private static IOperation? ResolveIsTypeSourceOperation(IOperation operation)
	{
		if (operation is IIsTypeOperation isTypeOperation)
			return isTypeOperation.ValueOperand;
		if (operation is IConversionOperation { IsTryCast: true } conversionOperation)
			return conversionOperation.Operand;

		// Only walk through wrappers that preserve the same pattern input (`not` / `and` / `or`).
		// Do not cross recursive/property/list/positional pattern boundaries because they can
		// change the effective input value for child patterns.
		for (var current = operation.Parent; current is not null; current = current.Parent)
		{
			if (current is INegatedPatternOperation or IBinaryPatternOperation)
				continue;

			if (current is IIsPatternOperation isPatternOperation)
				return isPatternOperation.Value;

			// switch-expression arms and switch statement case-clauses also have a single
			// discriminant source that can be used for compile-time provability.
			if (current is ISwitchExpressionArmOperation switchExpressionArm)
				return switchExpressionArm.Parent is ISwitchExpressionOperation switchExpression
					? switchExpression.Value
					: null;

			if (current is IPatternCaseClauseOperation patternCaseClause)
				return patternCaseClause.Parent is ISwitchCaseOperation switchCase &&
					   switchCase.Parent is ISwitchOperation switchOperation
					? switchOperation.Value
					: null;

			return null;
		}

		return null;
	}

	private static ITypeSymbol? ResolvePatternInputStaticType(IOperation operation)
	{
		return operation switch
		{
			IPatternOperation patternOperation => patternOperation.InputType,
			IIsTypeOperation isTypeOperation => isTypeOperation.ValueOperand.Type,
			_ => operation.Type
		};
	}

	private string BuildUnsupportedErasedInterfaceIsTypeCheckMessage(IOperation operation, ITypeSymbol interfaceType)
	{
		var sourceOperation = ResolveIsTypeSourceOperation(operation);
		var resolvedSource = sourceOperation is null
			? null
			: ResolveSingleAssignmentValueSource(sourceOperation, operation);
		var sourceType = resolvedSource?.Type ?? ResolvePatternInputStaticType(operation);
		var interfaceTypeName = interfaceType.ToDisplayString(Jazor.Common.Format.NameFormat);

		if (sourceType is null)
			return $"Unsupported interface is-type operation: cannot statically prove source assignability to '{interfaceTypeName}' because the source type is unknown.";

		var sourceTypeName = sourceType.ToDisplayString(Jazor.Common.Format.NameFormat);
		return $"Unsupported interface is-type operation: source static type '{sourceTypeName}' cannot be statically proven assignable to '{interfaceTypeName}'.";
	}

	private bool TryResolveDeterministicRuntimeValue(
		IOperation sourceOperation,
		out ITypeSymbol? runtimeType,
		out bool definitelyNonNull)
	{
		runtimeType = null;
		definitelyNonNull = false;
		var operation = UnwrapImplicitConversions(sourceOperation);

		// Compile-time null constants (null literal / default of reference-like targets).
		if (operation.ConstantValue.HasValue && operation.ConstantValue.Value is null)
			return true;

		switch (operation)
		{
			case IObjectCreationOperation objectCreation:
				runtimeType = objectCreation.Type ?? objectCreation.Constructor?.ContainingType;
				definitelyNonNull = runtimeType is not null;
				return runtimeType is not null;

			case IAnonymousObjectCreationOperation anonymousObjectCreation:
				runtimeType = anonymousObjectCreation.Type;
				definitelyNonNull = runtimeType is not null;
				return runtimeType is not null;

			case IArrayCreationOperation arrayCreation:
				runtimeType = arrayCreation.Type;
				definitelyNonNull = runtimeType is not null;
				return runtimeType is not null;

			case ILiteralOperation literal when literal.ConstantValue.HasValue:
				runtimeType = literal.Type;
				definitelyNonNull = literal.ConstantValue.Value is not null && runtimeType is not null;
				return runtimeType is not null;

			case IDefaultValueOperation defaultValue:
				if (defaultValue.Type is null)
					return false;

				// default(reference-like) -> null
				if (defaultValue.Type.IsReferenceType || IsNullableType(defaultValue.Type))
					return true;

				// default(non-nullable value-type) keeps concrete runtime type.
				runtimeType = defaultValue.Type;
				definitelyNonNull = true;
				return true;

			case IConversionOperation conversion:
				return TryResolveDeterministicRuntimeValue(conversion.Operand, out runtimeType, out definitelyNonNull);
		}

		return false;
	}

	private static bool TryResolveSingleAssignmentLocalInitializer(
		ILocalReferenceOperation localReference,
		IOperation useSiteOperation,
		out IOperation initializerValue)
	{
		initializerValue = null!;

		var local = localReference.Local;
		var semanticModel = localReference.SemanticModel;
		if (local is null ||
			semanticModel is null ||
			local.DeclaringSyntaxReferences.Length != 1)
			return false;

		if (IsLocalReassignedBeforeUse(local, useSiteOperation))
			return false;

		var declarationSyntax = local.DeclaringSyntaxReferences[0].GetSyntax();
		if (semanticModel.GetOperation(declarationSyntax) is not IVariableDeclaratorOperation { Initializer.Value: IOperation initializer } declarator)
			return false;

		initializerValue = declarator.Initializer.Value;
		return true;
	}

	private static bool IsLocalReassignedBeforeUse(ILocalSymbol local, IOperation useSiteOperation)
	{
		var root = useSiteOperation;
		while (root.Parent is not null)
			root = root.Parent;

		var usePosition = useSiteOperation.Syntax.SpanStart;
		foreach (var operation in root.Descendants())
		{
			if (operation.Syntax.SpanStart >= usePosition)
				continue;

			if (WritesToLocal(operation, local))
				return true;
		}

		return false;
	}

	private static bool WritesToLocal(IOperation operation, ILocalSymbol local)
	{
		static bool IsSameLocal(IOperation? target, ILocalSymbol localSymbol)
			=> target is ILocalReferenceOperation localReference &&
			   SymbolEqualityComparer.Default.Equals(localReference.Local, localSymbol);

		return operation switch
		{
			ISimpleAssignmentOperation simpleAssignment => IsSameLocal(simpleAssignment.Target, local),
			ICompoundAssignmentOperation compoundAssignment => IsSameLocal(compoundAssignment.Target, local),
			IIncrementOrDecrementOperation incrementOrDecrement => IsSameLocal(incrementOrDecrement.Target, local),
			IDeconstructionAssignmentOperation deconstructionAssignment => IsSameLocal(deconstructionAssignment.Target, local),
			IArgumentOperation argument when argument.Parameter?.RefKind is RefKind.Out or RefKind.Ref => IsSameLocal(argument.Value, local),
			_ => false
		};
	}

	private Expression BuildClassTypeMatch(
		IOperation operation,
		ITypeSymbol typeSymbol,
		Expression value,
		string typeName,
		SenseArgument? context)
	{
		if (typeSymbol is INamedTypeSymbol namedType && IsStructuralType(namedType))
		{
			return HandleTransformationFailure<Expression>(
				operation,
				$"Structural type '{namedType.ToDisplayString(Format.NameFormat)}' uses structural lowering and does not support nominal runtime type checks. Use property/positional patterns instead of a bare type pattern.");
		}

		RejectUnsupportedTypeFallback(operation, typeSymbol, "type checks");
		RejectAmbiguousRuntimeTypeFilter(operation, typeSymbol, "type checks");
		var runtimeType = BuildFullTypeName(typeSymbol, context) ?? new Identifier(typeName);
		return new NonLogicalBinaryExpression(Operator.InstanceOf, value, runtimeType);
	}

	/// <summary>
	/// 处理位置式解构子模式
	/// </summary>
	private void ProcessPositionalSubpatterns(
		IRecursivePatternOperation operation,
		INamedTypeSymbol namedType,
		Expression targetExpr,
		List<Expression> conditions,
		SenseArgument argument)
	{
		Identifier? deconstructResultId = null;
		if (!namedType.IsTupleType &&
			!IsStructuralType(namedType) &&
			operation.DeconstructSymbol is IMethodSymbol deconstructMethod)
		{
			deconstructResultId = new Identifier(AllocateUniqueName(operation, argument, LoweringSite.DeconstructResult()));
			argument.AddVarDeclarator(new VariableDeclarator(deconstructResultId, null), _recursionDepth);

			var deconstructArguments = new List<Expression>(deconstructMethod.Parameters.Length);
			for (var i = 0; i < deconstructMethod.Parameters.Length; i++)
				deconstructArguments.Add(new Identifier("undefined"));

			var callExpr = BuildMethodCallExpression(
				operation,
				deconstructMethod,
				operation.Syntax,
				operation.SemanticModel,
				targetExpr,
				deconstructArguments,
				argument,
				operation.InputType ?? deconstructMethod.ContainingType);

			conditions.Add(new SequenceExpression(NodeList.From<Expression>(
				new AssignmentExpression(Operator.Assignment, deconstructResultId, callExpr),
				new BooleanLiteral(true, "true"))));
		}

		for (int i = 0; i < operation.DeconstructionSubpatterns.Length; i++)
		{
			var subpattern = operation.DeconstructionSubpatterns[i];

			// 跳过弃元模式 _
			if (subpattern.Kind == OperationKind.DiscardPattern)
				continue;

			Expression? propertyExpr = deconstructResultId is not null
				? new MemberExpression(
					deconstructResultId,
					new NumericLiteral(i, i.ToString(System.Globalization.CultureInfo.InvariantCulture)),
					computed: true,
					optional: false)
				: GetPositionalPropertyExpression(operation, namedType, targetExpr, i);
			if (propertyExpr is null)
				{
					HandleTransformationFailure<Node>(operation,
						$"Cannot resolve positional property at index {i} for type '{namedType.ToDisplayString()}'.");
					return;
				}

			// 处理子模式，传递 propertyExpr 作为 PatternInput
			var subpatternArg = argument.WithPatternInput(propertyExpr);
			Expression? condition;
			if (subpattern is IDeclarationPatternOperation declarationPattern)
			{
				// 声明模式：变量声明（如 var x）
				condition = BuildDeclarationPattern(declarationPattern, propertyExpr, argument);
			}
			else
			{
				// 其他模式：常量、关系等（如 1, > 0）
				// 传递更新后的 PatternInput，让子模式自己处理比较
				condition = Translate<Expression>(subpattern, subpatternArg);
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
			// 元组类型同样遵循统一的 runtime naming 规则，而不是直接透传 CLR 成员名。
			propertyName = Util.GetConfigOrSymbolName(namedType.TupleElements[index]);
		}
		else if (IsStructuralType(namedType))
		{
			// Structural-lowered types bind positional patterns directly to their structural property keys.
			propertyName = GetStructuralPositionPropertyName(operation, namedType, index);
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
	private string? GetStructuralPositionPropertyName(
		IRecursivePatternOperation operation,
		INamedTypeSymbol namedType,
		int index)
	{
		if (StructuralRecordSupport.IsStructuralRecordType(namedType))
		{
			// Record 统一走 structural lowering，位置模式必须绑定到运行时结构属性键。
			// 主构造函数参数名只用于定位对应属性，最终仍取属性的运行时名。
			var constructor = FindMatchingConstructor(namedType, operation.DeconstructionSubpatterns.Length);
			if (constructor is not null && constructor.Parameters.Length > index)
			{
				var parameter = constructor.Parameters[index];
				var property = EnumerateNamedTypeHierarchyBaseFirst(namedType)
					.SelectMany(static current => current.GetMembers().OfType<IPropertySymbol>())
					.FirstOrDefault(member =>
						!member.IsStatic &&
						string.Equals(member.Name, parameter.Name, StringComparison.OrdinalIgnoreCase));
				return property is null
					? parameter.Name
					: Util.GetConfigOrSymbolName(property);
			}
		}

		var message = $"Cannot determine property name for structural type '{namedType.Name}' at position {index}. " +
			$"Ensure the type has a matching structural member shape.";
		var location = operation.Syntax.GetLocation();
		_report?.Invoke(location, message);
		throw new OperationTransformationException(operation.Kind, message);
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
	private CallExpression VisitSwitchPatternMatching(ISwitchOperation operation, SenseArgument argument)
	{
		if (Visit(operation.Value, argument) is not Expression discriminant)
			return HandleTransformationFailure<CallExpression>(operation.Value, "Switch discriminant could not be translated to JavaScript.");

		var iifeArg = EnsureScopeContext(operation, argument).EnterEmissionScope(operation, ScopeSite.PatternIife());

		// 创建唯一名称存储 switch 值
		var inputId = new Identifier(AllocateUniqueName(operation.Value, iifeArg, LoweringSite.SwitchPatternInput()));
		var inputVar = new VariableDeclaration(
			VariableDeclarationKind.Const,
			NodeList.From(new VariableDeclarator(inputId, discriminant))
		);

		// 设置 PatternInput 为输入变量
		var caseArg = iifeArg.WithPatternInput(inputId);

		var statements = new List<Statement>();
		for (var caseIndex = 0; caseIndex < operation.Cases.Length; caseIndex++)
		{
			var switchCase = operation.Cases[caseIndex];
			var hasDefault = false;
			// 收集所有条件表达式
			var conditions = new List<Expression>();
			foreach (var clause in switchCase.Clauses)
			{
				if (clause.CaseKind == CaseKind.Default)
					hasDefault = true;
				else
				{
					// 兼容常量 null 模式
					var expr = Translate<Expression>(clause, caseArg);
					if ((clause.CaseKind == CaseKind.SingleValue) ||
						(expr is Literal literal && literal.Kind == TokenKind.NullLiteral))
						expr = new NonLogicalBinaryExpression(Operator.StrictEquality, inputId, expr);

					conditions.Add(expr);
				}
			}

			// 处理 case 体：隔离 scope，变量声明留在 case 块内
			var caseCtx = caseArg.EnterScope(switchCase, ScopeSite.SwitchCaseBody());
			var casePending = TranslatePatternSwitchCaseBodyStatements(switchCase.Body, caseCtx);

			var bodyStatements = MaterializeScopedStatements(caseCtx, casePending);

			if (conditions.Count > 0)
			{
				Expression combinedCondition = conditions[0];
				for (int i = 1; i < conditions.Count; i++)
				{
					combinedCondition = new LogicalExpression(Operator.LogicalOr, combinedCondition, conditions[i]);
				}

				var ifStmt = new IfStatement(combinedCondition, new NestedBlockStatement(NodeList.From(bodyStatements)), null);
				statements.Add(ifStmt);
			}
			else if (hasDefault)
			{
				statements.AddRange(bodyStatements);
			}
		}

		// 构造 IIFE
		statements.Insert(0, inputVar);
		var functionBody = new FunctionBody(NodeList.From(MaterializeScopedStatements(iifeArg, statements)), strict: true);
		var arrowFunction = new ArrowFunctionExpression(
			NodeList.From<Node>(),
			functionBody,
			expression: false,
			async: ContainsAwaitOperation(operation)
		);

		return new CallExpression(arrowFunction, NodeList.From<Expression>(), optional: false);
	}

	private List<Statement> TranslatePatternSwitchCaseBodyStatements(
		IReadOnlyList<IOperation> operations,
		SenseArgument context)
	{
		var pendingStatements = new List<Statement>();
		for (var index = 0; index < operations.Count; index++)
		{
			var operation = operations[index];
			if (operation is IBranchOperation branchOperation)
			{
				switch (branchOperation.BranchKind)
				{
					case BranchKind.Break:
						pendingStatements.Add(new ReturnStatement(null));
						continue;

					case BranchKind.Continue:
						HandleTransformationFailure<Node>(branchOperation, "Continue statements inside pattern-matching switch are not supported (IIFE boundary).");
						continue;
				}
			}

			var node = Visit(operation, context);
			if (node is Statement statement)
				pendingStatements.Add(statement);
			else if (node is Expression expr)
				pendingStatements.Add(new NonSpecialExpressionStatement(expr));
			else
				HandleTransformationFailure<Node>(operation, $"{operation.Kind} could not be translated to JavaScript.");
		}

		return pendingStatements;
	}

	/// <summary>
	/// 处理声明模式操作
	/// </summary>
	/// <param name="operation">声明模式操作</param>
	/// <param name="value">赋值对象（可选，如果提供则用于类型检查）</param>
	/// <param name="argument">语义上下文（必须包含 PatternInput）</param>
	/// <returns>JavaScript 表达式</returns>
	private Expression BuildDeclarationPattern(IDeclarationPatternOperation operation, Expression? value, SenseArgument argument)
	{
		/*
		有效 - 显式类型声明，MatchedType 非空，DeclaredSymbol 非空：if (obj is string s)，显式指定类型并声明变量
		有效 - 推断类型声明，MatchedType null，DeclaredSymbol 非空：if (obj is var s)，类型推断，声明变量
		有效 - 类型检查，MatchedType 非空，DeclaredSymbol null：if (obj is string)，仅检查类型，不声明变量
		无效，MatchedType null，DeclaredSymbol null：if (obj is )，语法错误：未指定类型，未声明变量
		*/

		if (operation.DeclaredSymbol is null && operation.MatchedType is null)
			return HandleTransformationFailure<Expression>(operation, "Declaration pattern must have either a declared symbol or a matched type.");

		// 必须有 PatternInput
		var obj = GetPatternRefrence(operation, argument);

		Expression? typeMatchExpr = null, declaredExpr = null;
		var assignValueExpr = value ?? obj;
		Expression? assignmentInitialization = null;
		if (operation.MatchedType is not null && operation.DeclaredSymbol is not null)
			assignValueExpr = StabilizePatternExpression(operation, assignValueExpr, argument, "declaration", out assignmentInitialization);

		// 存在赋值对象使用赋值对象
		if (operation.MatchedType is not null)
			typeMatchExpr = CreateTypeMatchExpr(operation, operation.MatchedType, assignValueExpr, context: argument);

		if (operation.DeclaredSymbol is not null)
		{
			// 声明模式转换为变量声明
			declaredExpr = BuildPatternDeclaredSymbolAssignment(operation.DeclaredSymbol, assignValueExpr, operation, argument);
		}

		if (typeMatchExpr is not null && declaredExpr is not null)
			return PrependEvaluation(assignmentInitialization, new LogicalExpression(Operator.LogicalAnd, typeMatchExpr, declaredExpr));
		else if (typeMatchExpr is not null)
			return PrependEvaluation(assignmentInitialization, typeMatchExpr);
		else
			return PrependEvaluation(assignmentInitialization, declaredExpr!);
	}

	private Expression BuildPatternDeclaredSymbolAssignment(
		ISymbol declaredSymbol,
		Expression value,
		IOperation operation,
		SenseArgument argument)
	{
		var id = CreatePatternDeclaredSymbolIdentifier(declaredSymbol, operation, argument);
		var assignmentExpr = new AssignmentExpression(Operator.Assignment, id, value);
		return new SequenceExpression(NodeList.From<Expression>(assignmentExpr, new BooleanLiteral(true, "true")));
	}

	private Identifier CreatePatternDeclaredSymbolIdentifier(
		ISymbol declaredSymbol,
		IOperation operation,
		SenseArgument argument)
	{
		var id = declaredSymbol is ILocalSymbol local
			? Host?.RewriteLocalDeclarationIdentifier(local, operation, argument) ?? new Identifier(declaredSymbol.Name)
			: new Identifier(declaredSymbol.Name);
		argument.AddVarDeclarator(new VariableDeclarator(id, null), _recursionDepth);
		return id;
	}

}
