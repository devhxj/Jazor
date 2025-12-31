using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ECMAScript.Compiler;

public partial class SemanticWalker
{
	/// <summary>
	/// 提取模式操作中引用对象名称
	/// </summary>
	/// <param name="operation">模式相关操作</param>
	/// <returns>引用对象名称</returns>
	private string ExtractPatternValName(IOperation? operation)
	{
		if (operation is null)
			return string.Empty;

		var op = FindValueOperation(operation);
		var name = op switch
		{
			ILocalReferenceOperation localRef => localRef.Local.Name,
			IParameterReferenceOperation paramRef => paramRef.Parameter.Name,
			IFieldReferenceOperation fieldRef => fieldRef.Field.Name,
			IPropertyReferenceOperation propRef => propRef.Property.Name,
			IDiscardOperation => "_",
			IInstanceReferenceOperation => "this",
			_ => string.Empty
		};

		if (string.IsNullOrEmpty(name))
		{
			var location = operation.Syntax.GetLocation();
			var message = $"cannot extract reference name from {operation.Kind}.";
			_report?.Invoke(location, message);
		}

		return name;

		static IOperation? FindValueOperation(IOperation? startOp)
		{
			var visited = new HashSet<IOperation>();
			var currentOp = startOp;
			while (currentOp is not null)
			{
				// 防止在损坏的 IOperation 树中出现无限循环
				if (!visited.Add(currentOp))
					return null; // 检测到循环

				// 这是关键的判断：我们正在寻找的“容器”操作
				switch (currentOp)
				{
					case IIsPatternOperation isPattern:
						return isPattern.Value; // is e
					case ISwitchExpressionOperation switchExpr:
						return switchExpr.Value; // switch (e) { ... }
					case ISwitchOperation switchStmt:
						return switchStmt.Value; // switch (e) { ... }
				}
				// 如果不是容器操作，则向父级继续搜索
				currentOp = currentOp.Parent;
			}

			return null; // 到达根节点仍未找到
		}
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
		var pattern = Translate<Expression>(operation.Pattern, argument);

		// 对于常量模式，直接比较
		if (operation.Pattern.Kind == OperationKind.ConstantPattern)
		{
			// is 模式转换，支持复杂模式匹配
			var value = Translate<Expression>(operation.Value, argument);
			return new NonLogicalBinaryExpression(Operator.StrictEquality, value, pattern);
		}

		// 对于复杂模式，直接使用模式表达式（已经包含实际目标）
		return pattern;
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
		var valueOperand = Translate<Expression>(operation.ValueOperand, argument);
		var targetType = operation.TypeOperand;
		var typeName = targetType.Name;
		var fullTypeName = targetType.ToDisplayString();

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
		switch (targetType.SpecialType)
		{
			case SpecialType.System_String:
				return new NonLogicalBinaryExpression(
					Operator.StrictEquality,
					new NonUpdateUnaryExpression(Operator.TypeOf, valueOperand),
					new StringLiteral("string", "'string'")
				);
			case SpecialType.System_SByte:
			case SpecialType.System_Byte:
			case SpecialType.System_Int16:
			case SpecialType.System_UInt16:
			case SpecialType.System_Int32:
			case SpecialType.System_UInt32:
			case SpecialType.System_Single:
			case SpecialType.System_Double:
			case SpecialType.System_Decimal:
				return new NonLogicalBinaryExpression(
					Operator.StrictEquality,
					new NonUpdateUnaryExpression(Operator.TypeOf, valueOperand),
					new StringLiteral("number", "'number'")
				);
			case SpecialType.System_Boolean:
				return new NonLogicalBinaryExpression(
					Operator.StrictEquality,
					new NonUpdateUnaryExpression(Operator.TypeOf, valueOperand),
					new StringLiteral("boolean", "'boolean'")
				);
			case SpecialType.System_Object:
				return new LogicalExpression(
					Operator.LogicalAnd,
					new NonLogicalBinaryExpression(
						Operator.StrictEquality,
						new NonUpdateUnaryExpression(Operator.TypeOf, valueOperand),
						new StringLiteral("object", "'object'")
					),
					new NonLogicalBinaryExpression(Operator.Inequality, valueOperand, new NullLiteral("null"))
				);
		}

		// 对于非基础类型，使用字符串名称进行判断
		// 大整数类型检查（long、timestamp等）
		if (targetType.SpecialType == SpecialType.System_Int64 ||
			targetType.SpecialType == SpecialType.System_UInt64 ||
			typeName.Equals("timestamp", StringComparison.OrdinalIgnoreCase))
		{
			return new LogicalExpression(
				Operator.LogicalAnd,
				new NonLogicalBinaryExpression(
					Operator.StrictEquality,
					new NonUpdateUnaryExpression(Operator.TypeOf, valueOperand),
					new StringLiteral("bigint", "'bigint'")
				),
				new NonLogicalBinaryExpression(Operator.Inequality, valueOperand, new NullLiteral("null"))
			);
		}

		// 日期类型检查
		if (typeName.Equals("DateOnly", StringComparison.OrdinalIgnoreCase) ||
			typeName.Equals("TimeOnly", StringComparison.OrdinalIgnoreCase) ||
			typeName.Equals("DateTime", StringComparison.OrdinalIgnoreCase) ||
			typeName.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase))
		{
			return new LogicalExpression(
				Operator.LogicalAnd,
				new NonLogicalBinaryExpression(Operator.InstanceOf, valueOperand, new Identifier("Date")),
				new NonLogicalBinaryExpression(Operator.Inequality, valueOperand, new NullLiteral("null"))
			);
		}

		// 数组类型检查
		if (typeName.Equals("Array", StringComparison.OrdinalIgnoreCase) ||
			fullTypeName.Contains("[]"))
		{
			return new LogicalExpression(
				Operator.LogicalAnd,
				new NonLogicalBinaryExpression(Operator.Inequality, valueOperand, new NullLiteral("null")),
				new CallExpression(
					new MemberExpression(new Identifier("Array"), new Identifier("isArray"), computed: false, optional: false),
					NodeList.From(valueOperand),
					optional: false
				)
			);
		}

		// 字典类型检查
		if (typeName.Equals("IDictionary", StringComparison.OrdinalIgnoreCase) ||
			(targetType is INamedTypeSymbol namedType &&
			 namedType.AllInterfaces.Any(i => i.Name.Equals("IDictionary", StringComparison.OrdinalIgnoreCase))))
		{
			return new LogicalExpression(
				Operator.LogicalAnd,
				new NonLogicalBinaryExpression(Operator.InstanceOf, valueOperand, new Identifier("Map")),
				new NonLogicalBinaryExpression(Operator.Inequality, valueOperand, new NullLiteral("null"))
			);
		}

		// 集合类型检查（非字典）
		if (typeName.Equals("IEnumerable", StringComparison.OrdinalIgnoreCase) ||
			(targetType is INamedTypeSymbol enumType &&
			 enumType.AllInterfaces.Any(i => i.Name.Equals("IEnumerable", StringComparison.OrdinalIgnoreCase)) &&
			 !enumType.AllInterfaces.Any(i => i.Name.Equals("IDictionary", StringComparison.OrdinalIgnoreCase))))
		{
			return new LogicalExpression(
				Operator.LogicalAnd,
				new NonLogicalBinaryExpression(Operator.InstanceOf, valueOperand, new Identifier("Set")),
				new NonLogicalBinaryExpression(Operator.Inequality, valueOperand, new NullLiteral("null"))
			);
		}

		// 其他自定义类类型检查
		return new LogicalExpression(
			Operator.LogicalAnd,
			new NonLogicalBinaryExpression(Operator.InstanceOf, valueOperand, new Identifier(typeName)),
			new NonLogicalBinaryExpression(Operator.Inequality, valueOperand, new NullLiteral("null"))
		);
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
		// 常量模式转换为字面量比较
		return Visit(operation.Value, argument);
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
	{
		if (operation.DeclaredSymbol is null)
			return null;

		// 声明模式转换为变量声明
		var identifier = new Identifier(operation.DeclaredSymbol.Name);
		return new VariableDeclaration(VariableDeclarationKind.Let,
			NodeList.From(new VariableDeclarator(identifier, null)));
	}

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

		return new LogicalExpression(Operator.StrictEquality, operand, new NullLiteral("null"));
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

		// 从父operation获取目标名称，在节点内构建表达式
		var targetName = ExtractPatternValName(operation.Parent);
		var targetExpression = new Identifier(targetName);

		// 从Member中获取名称
		var propertyName = operation.Member switch
		{
			IFieldReferenceOperation fieldRef => fieldRef.Field.Name,
			IPropertyReferenceOperation propRef => propRef.Property.Name,
			_ => null
		};

		// 访问属性模式并转换为表达式
		var patternExpression = Translate<Expression>(operation.Pattern, argument);
		if (propertyName is null)
			return HandleTransformationFailure(operation, "Unsupported member type in property subpattern.");

		// 根据AST节点构造规范，生成属性访问表达式
		// 使用实际的目标表达式而不是占位符
		var propertyAccess = new MemberExpression(
			targetExpression,
			new Identifier(propertyName),
			computed: false,
			optional: false
		);

		// 根据模式类型生成不同的比较表达式
		// 对于常量模式，生成 === 比较
		// 对于其他模式，直接返回模式表达式（如关系比较等）
		return new NonLogicalBinaryExpression(Operator.StrictEquality, propertyAccess, patternExpression);
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
		var innerExpression = Translate<Expression>(operation.Pattern, argument);

		// 使用UpdateExpression处理逻辑非操作
		// 生成 !(innerExpression) 的JavaScript表达式
		return new UpdateExpression(Operator.LogicalNot, innerExpression, prefix: true);
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
	/// 处理类型模式操作
	/// C# 示例：
	/// obj is string           // 类型模式
	/// value is int            // 值类型模式
	/// item is MyClass         // 自定义类型模式
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
		// 从父operation获取目标名称，在节点内构建表达式
		var targetName = ExtractPatternValName(operation.Parent);
		// 根据获取的名称构建目标表达式
		var targetExpression = new Identifier(targetName);

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
								new LogicalExpression(Operator.StrictInequality, targetExpression, new NullLiteral("null")),
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

		// 获取参考操作
		IOperation? refOperation = operation.Parent switch
		{
			IIsPatternOperation isPattern => isPattern.Value,
			ISwitchExpressionOperation switchExpr => switchExpr.Value,
			INegatedPatternOperation negatedPattern => negatedPattern.Parent switch
			{
				IIsPatternOperation nestedIsPattern => nestedIsPattern.Value,
				ISwitchExpressionOperation nestedSwitchExpr => nestedSwitchExpr.Value,
				_ => null
			},
			_ => null
		};

		// 从参考操作中提取名称
		var targetName = refOperation switch
		{
			ILocalReferenceOperation localRef => localRef.Local.Name,
			IParameterReferenceOperation paramRef => paramRef.Parameter.Name,
			IFieldReferenceOperation fieldRef when fieldRef.Instance is null => fieldRef.Field.Name,
			IPropertyReferenceOperation propRef when propRef.Instance is null => propRef.Property.Name,
			IInstanceReferenceOperation => "this",
			_ => "value"
		};

		// 根据获取的名称构建目标表达式
		var targetExpression = new Identifier(targetName);

		// 获取右操作数（比较值）
		var value = Translate<Expression>(operation.Value, argument);

		// 检查是否在取反模式中（使用简化的检查逻辑）
		bool isInNegatedPattern = operation.OperatorKind == BinaryOperatorKind.NotEquals ||
								  operation.Parent is INegatedPatternOperation ||
								  (operation.Parent is IIsPatternOperation isPatternOp &&
								   isPatternOp.Pattern is INegatedPatternOperation) ||
								  (operation.Parent is IIsPatternOperation parentPattern &&
								   parentPattern.Parent is INegatedPatternOperation) ||
								  (operation.OperatorKind == BinaryOperatorKind.Equals &&
								   operation.Parent is IIsPatternOperation patternOp &&
								   patternOp.Parent is INegatedPatternOperation);

		// 根据编译时优化原则，直接生成最简洁的JavaScript关系比较表达式
		// 将C#的关系操作符映射到JavaScript的操作符
		// 如果在取反模式中，需要反转操作符（如 Equals 变为 StrictInequality）
		var @operator = (operation.OperatorKind, isInNegatedPattern) switch
		{
			(BinaryOperatorKind.GreaterThan, false) => Operator.GreaterThan,
			(BinaryOperatorKind.GreaterThan, true) => Operator.LessThanOrEqual,
			(BinaryOperatorKind.GreaterThanOrEqual, false) => Operator.GreaterThanOrEqual,
			(BinaryOperatorKind.GreaterThanOrEqual, true) => Operator.LessThan,
			(BinaryOperatorKind.LessThan, false) => Operator.LessThan,
			(BinaryOperatorKind.LessThan, true) => Operator.GreaterThanOrEqual,
			(BinaryOperatorKind.LessThanOrEqual, false) => Operator.LessThanOrEqual,
			(BinaryOperatorKind.LessThanOrEqual, true) => Operator.GreaterThan,
			(BinaryOperatorKind.Equals, false) => Operator.StrictEquality,
			(BinaryOperatorKind.Equals, true) => Operator.StrictInequality,
			(BinaryOperatorKind.NotEquals, false) => Operator.StrictInequality,
			(BinaryOperatorKind.NotEquals, true) => Operator.StrictEquality,
			_ => Operator.Unknown
		};

		if (@operator == Operator.Unknown)
			return HandleTransformationFailure(operation, "Unsupported relational operator in pattern.");

		// 根据AST节点构造规范，使用LogicalExpression表示比较操作
		// 使用实际的目标表达式而不是占位符
		return new LogicalExpression(@operator, targetExpression, value);
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
		// 从父operation获取目标名称，在节点内构建表达式
		var targetName = ExtractPatternValName(operation.Parent);
		var patterns = operation.Patterns;
		if (patterns == null || patterns.IsEmpty) return null;

		Expression targetExpr = new Identifier(targetName);

		/* 1. Array.isArray(target) */
		// 修正 CallExpression 的构造：callee 和 arguments 分开
		var arrayCheck = new CallExpression(
			callee: new MemberExpression(new Identifier("Array"), new Identifier("isArray"), computed: false, optional: false),
			args: NodeList.From(targetExpr),
			optional: false
		);

		/* 2. 统计"固定头"长度，并检测有没有 .. */
		int fixedLen = 0;
		bool hasSlice = false;
		int sliceIndex = -1; // 记录切片模式的位置
		for (int i = 0; i < patterns.Length; i++)
		{
			if (patterns[i] is ISlicePatternOperation)
			{
				hasSlice = true;
				sliceIndex = i;
				break;
			}
			fixedLen++;
		}

		/* 3. 长度检查：有切片就用 >=，没有就用 === */
		var lengthCheck = new NonLogicalBinaryExpression(
			hasSlice ? Operator.GreaterThanOrEqual : Operator.StrictEquality,
			new MemberExpression(targetExpr, new Identifier("length"), computed: false, optional: false),
			new NumericLiteral(fixedLen, fixedLen.ToString())
		);

		/* 4. 固定头元素检查 */
		Expression? elemChecks = null;
		for (int i = 0; i < fixedLen; i++)
		{
			var pattern = patterns[i];
			// 创建对 target[i] 的访问表达式
			var indexAccess = new MemberExpression(targetExpr,
				new NumericLiteral(i, i.ToString()), computed: true, optional: false);

			Expression? subCondition = null;

			// 【核心修正】直接在此处处理子模式，而不是递归调用 Visit
			// 因为我们不能改变 argument，所以子模式必须由父模式"解释"和"生成"
			switch (pattern)
			{
				case IConstantPatternOperation cpo:
					// 递归访问常量表达式，这是安全的，因为它不依赖于 argument
					var valueExpr = Translate<Expression>(cpo.Value, argument)!;
					subCondition = new NonLogicalBinaryExpression(Operator.StrictEquality, indexAccess, valueExpr);
					break;

				case IDeclarationPatternOperation declPattern:
					// 声明模式：将变量名添加到 argument 队列，由上层统一生成 const 语句
					if (declPattern.DeclaredSymbol is not null)
					{
						var variableName = declPattern.DeclaredSymbol.Name;
						argument.Enqueue(new VariableDeclaration(
							VariableDeclarationKind.Const,
							NodeList.From(new VariableDeclarator(new Identifier(variableName), indexAccess))
						));
					}
					// 声明模式总是匹配，不增加布尔条件
					subCondition = null;
					break;

				case IDiscardPatternOperation:
					// 弃元模式忽略，不增加条件
					subCondition = null;
					break;

					// 可以根据需要添加其他模式类型的处理
					// default:
					//     return HandleTransformationFailure(pattern, "Unsupported pattern type in list.");
			}

			if (subCondition is not null)
			{
				elemChecks = elemChecks is null
					? subCondition
					: new LogicalExpression(Operator.LogicalAnd, elemChecks, subCondition);
			}
		}

		/* 6. 处理切片模式（如果有） */
		if (hasSlice && sliceIndex >= 0)
		{
			var slicePattern = patterns[sliceIndex] as ISlicePatternOperation;
			if (slicePattern?.Pattern is IDeclarationPatternOperation sliceDeclPattern &&
				sliceDeclPattern.DeclaredSymbol is not null)
			{
				// 处理切片模式中的变量声明，如 .. var rest
				var variableName = sliceDeclPattern.DeclaredSymbol.Name;

				// 创建切片表达式：target.slice(fixedLen)
				var sliceCall = new CallExpression(
					new MemberExpression(targetExpr, new Identifier("slice"), computed: false, optional: false),
					NodeList.From<Expression>(new NumericLiteral(fixedLen, fixedLen.ToString())),
					optional: false
				);

				// 将变量名添加到 argument 队列，由上层统一生成 const 语句
				argument.Enqueue(new VariableDeclaration(
					VariableDeclarationKind.Const,
					NodeList.From(new VariableDeclarator(new Identifier(variableName), sliceCall))
				));
			}
		}

		/* 7. 拼总表达式 */
		Expression result = new LogicalExpression(Operator.LogicalAnd, arrayCheck, lengthCheck);
		if (elemChecks != null)
			result = new LogicalExpression(Operator.LogicalAnd, result, elemChecks);

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
		// 切片模式的条件判断转换
		// C# 示例：array is [.., var lastPart] 是一个布尔条件表达式
		// 转换结果：Array.isArray(array) && array.length >= minLength

		var pattern = operation.Pattern;
		if (pattern is null)
			return null;

		// 从父operation获取目标名称，在节点内构建表达式
		var targetName = ExtractPatternValName(operation.Parent);
		var targetExpression = new Identifier(targetName);

		// 1. 首先生成 Array.isArray(targetExpression) 检查
		var arrayCheck = new CallExpression(
			new MemberExpression(new Identifier("Array"), new Identifier("isArray"), computed: false, optional: false),
			NodeList.From<Expression>(targetExpression),
			optional: false
		);

		// 2. 处理切片模式内部的模式（如果有）
		// 切片模式本身可以包含一个子模式，如 .. var rest 或 .. 5

		// 对于声明模式（如 .. var rest），需要将变量名添加到 argument 队列
		if (pattern is IDeclarationPatternOperation declPattern && declPattern.DeclaredSymbol is not null)
		{
			// 在解构上下文中，将变量名添加到 argument 队列，由上层统一生成 Let 语句
			var variableName = declPattern.DeclaredSymbol.Name;
			argument.Enqueue(new VariableDeclaration(
				VariableDeclarationKind.Let,
				NodeList.From(new VariableDeclarator(new Identifier(variableName), null))
			));

			// 声明模式总是匹配，不增加额外条件
			return arrayCheck;
		}

		// 对于常量模式（如 .. 5），需要生成条件检查
		if (pattern is IConstantPatternOperation)
		{
			// 对于常量模式，需要检查切片部分是否为空
			// 例如：array is [1, .., 5] 中的 .. 部分不能为空
			// 但在纯条件判断中，我们只需要确保数组类型正确
			// 实际的长度检查由 VisitListPattern 处理
			return arrayCheck;
		}

		// 对于丢弃模式（如 ..），总是匹配
		if (pattern is IDiscardPatternOperation)
		{
			return arrayCheck;
		}

		// 3. 如果没有子模式，只返回数组检查
		return arrayCheck;
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
						new StringLiteral("string", "'string'")
					),
				"number" or "int32" or "int64" or "double" or "float" or "decimal" =>
							 new NonLogicalBinaryExpression(
						Operator.StrictEquality,
						new NonUpdateUnaryExpression(Operator.TypeOf, target),
						new StringLiteral("number", "'number'")
					),
				"boolean" => new NonLogicalBinaryExpression(
						Operator.StrictEquality,
						new NonUpdateUnaryExpression(Operator.TypeOf, target),
						new StringLiteral("boolean", "'boolean'")
					),
				"object" => new NonLogicalBinaryExpression(
						Operator.StrictEquality,
						new NonUpdateUnaryExpression(Operator.TypeOf, target),
						new StringLiteral("object", "'object'")
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
