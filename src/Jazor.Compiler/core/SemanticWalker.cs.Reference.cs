using Acornima;
using Acornima.Ast;
using Jazor.Common;
using Jazor.Name;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

public partial class SemanticWalker
{
	/// <summary>
	/// 获取初始化器成员的名称，优先检查白名单别名
	/// 对于属性：检查 setter 的白名单别名（初始化器是设置值）
	/// 对于字段：检查字段本身的白名单别名
	/// </summary>
	private static string GetInitializerMemberName(ISymbol symbol)
	{
		// 1. 先检查白名单别名
		ISymbol? whiteListSymbol = symbol;
		if (symbol is IPropertySymbol property && property.SetMethod is not null)
			whiteListSymbol = property.SetMethod;

		var displayString = whiteListSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
		if (WhiteList.Members.TryGetValue(displayString, out var entry) &&
			entry.Op == Op.Alias &&
			!string.IsNullOrEmpty(entry.Value))
			return entry.Value!;

		// 2. 再检查特性配置
		return Util.GetConfigOrSymbolName(symbol);
	}

	/// <summary>
	/// 获取方法的名称，优先检查白名单别名
	/// </summary>
	private static string GetMethodConfigOrWhiteListName(IMethodSymbol method)
	{
		// 1. 先检查白名单别名
		var displayString = method.OriginalDefinition.ToDisplayString(Format.NameFormat);
		if (WhiteList.Members.TryGetValue(displayString, out var entry) &&
			entry.Op == Op.Alias &&
			!string.IsNullOrEmpty(entry.Value))
			return entry.Value!;

		// 2. 再检查特性配置
		return Util.GetConfigOrSymbolName(method);
	}

	private static string? GetTypeConfigOrWhiteListName(ITypeSymbol symbol)
	{
		string? name = null;

		// 先查询白名单
		var displayName = symbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
		if (WhiteList.Types.TryGetValue(displayName, out var entry) &&
			entry.Op == Op.Alias &&
			!string.IsNullOrEmpty(entry.Value))
			name = entry.Value!;

		// 再取特性配置
		if (string.IsNullOrEmpty(name))
		{
			if (Util.HasNameResolutionBoundary(symbol))
				return null;

			name = Util.GetSymbolConfigName(symbol) ?? symbol.Name;
		}

		return name;
	}

	private static string? GetModuleImportPath(ITypeSymbol symbol)
	{
		foreach (var attribute in symbol.GetAttributes())
		{
			if (attribute.AttributeClass?.ToDisplayString() != "ECMAScript.ECMAScriptModuleAttribute")
				continue;

			if (attribute.ConstructorArguments.Length == 1 &&
				attribute.ConstructorArguments[0].Value is string importPath &&
				!string.IsNullOrWhiteSpace(importPath))
				return importPath;
		}

		return null;
	}

	private Expression? BuildFullTypeName(ITypeSymbol symbol, SenseArgument? context = null)
	{
		var queue = new Stack<string>();
		var type = symbol;
		while (type is not null)
		{
			if (_moduleRootType is not null &&
				SymbolEqualityComparer.Default.Equals(type, _moduleRootType))
				break;

			var name = GetTypeConfigOrWhiteListName(type);
			if (string.IsNullOrEmpty(name))
				break;

			var modulePath = GetModuleImportPath(type);
			if (!string.IsNullOrEmpty(modulePath))
			{
				if (_moduleRootType is null || !SymbolEqualityComparer.Default.Equals(type, _moduleRootType))
				{
					context?.MergeImportSpecifier(modulePath!, new ImportSpecifier(new Identifier(name!)));
				}

				queue.Push(name!);
				break;
			}

			queue.Push(name!);

			type = SymbolEqualityComparer.Default.Equals(type, symbol.ContainingType)
				? null : symbol.ContainingType;
		}

		Expression? expr = null;
		if (queue.Count > 0)
		{
			expr = new Identifier(queue.Pop());
			while (queue.Count > 0)
			{
				var property = new Identifier(queue.Pop());
				expr = new MemberExpression(expr, property, computed: false, optional: false);
			}
		}
		return expr;
	}

	private Expression? TryBuildStaticQualifiedMemberFromSyntax(SyntaxNode syntax, string memberName)
	{
		ExpressionSyntax? targetSyntax = syntax switch
		{
			InvocationExpressionSyntax invocation when invocation.Expression is MemberAccessExpressionSyntax memberAccess => memberAccess.Expression,
			MemberAccessExpressionSyntax memberAccess => memberAccess.Expression,
			_ => null
		};

		if (targetSyntax is null)
			return null;

		var target = ConvertFromSyntaxNode(targetSyntax) as Expression;
		if (target is null)
			return null;

		return new MemberExpression(target, new Identifier(memberName), computed: false, optional: false);
	}

	private bool TryBuildImportedModuleMember(ITypeSymbol? containingType, string memberName, SenseArgument? context, out Expression? expression)
	{
		expression = null;
		if (containingType is null)
			return false;

		var modulePath = GetModuleImportPath(containingType);
		if (string.IsNullOrWhiteSpace(modulePath))
			return false;

		if (_moduleRootType is not null &&
			SymbolEqualityComparer.Default.Equals(containingType, _moduleRootType))
			return false;

		context?.MergeImportSpecifier(modulePath!, new ImportSpecifier(new Identifier(memberName)));
		expression = new Identifier(memberName);
		return true;
	}

	private bool TryBuildImportedModulePropertyAccess(IPropertySymbol property, SenseArgument? context, out Expression? expression)
	{
		expression = null;
		if (!property.IsStatic || property.GetMethod is null)
			return false;

		var getterName = GetMethodConfigOrWhiteListName(property.GetMethod);
		if (!TryBuildImportedModuleMember(property.ContainingType, getterName, context, out var getter) ||
			getter is null)
			return false;

		expression = new CallExpression(getter, NodeList.Empty<Expression>(), optional: false);
		return true;
	}

	private Expression GetFieldName(IOperation includeOp, IFieldSymbol symbol)
	{
		if (TryBuildECMAScriptEnumLiteral(symbol, out var enumLiteral))
			return enumLiteral;

		// 检查是否是特殊常量字段（如 double.PositiveInfinity, double.NaN 等）
		if (symbol.ContainingType is not null && symbol.IsConst)
		{
			// 处理特殊常量字段
			return (symbol.Name, symbol.ContainingType.SpecialType) switch
			{
				// 浮点类型特殊常量
				(nameof(double.PositiveInfinity), SpecialType.System_Double) or
				(nameof(float.PositiveInfinity), SpecialType.System_Single) => new Identifier("Infinity"),

				(nameof(double.NegativeInfinity), SpecialType.System_Double) or
				(nameof(float.NegativeInfinity), SpecialType.System_Single) => new Identifier("-Infinity"),

				(nameof(double.NaN), SpecialType.System_Double) or
				(nameof(float.NaN), SpecialType.System_Single) => new Identifier("NaN"),

				(nameof(double.Epsilon), SpecialType.System_Double) or
				(nameof(float.Epsilon), SpecialType.System_Single) =>
					new MemberExpression(
						new Identifier("Number"),
						new Identifier("EPSILON"), computed: false, optional: false),

				// double 的最大/最小值与 JavaScript Number 范围一致
				(nameof(double.MaxValue), SpecialType.System_Double) =>
					new MemberExpression(
						new Identifier("Number"),
						new Identifier("MAX_VALUE"), computed: false, optional: false),
				(nameof(double.MinValue), SpecialType.System_Double) =>
					new NonUpdateUnaryExpression(
						Operator.UnaryNegation,
						new MemberExpression(
							new Identifier("Number"),
							new Identifier("MAX_VALUE"), computed: false, optional: false)),

				// float 的边界值需要保留 C# 单精度语义，不能退化成 JS 的 double 极值
				(nameof(float.MaxValue), SpecialType.System_Single) =>
					new NumericLiteral(float.MaxValue, float.MaxValue.ToString("R", System.Globalization.CultureInfo.InvariantCulture)),
				(nameof(float.MinValue), SpecialType.System_Single) =>
					new NumericLiteral(float.MinValue, float.MinValue.ToString("R", System.Globalization.CultureInfo.InvariantCulture)),

				// long 的边界值在当前映射中属于 bigint
				(nameof(long.MaxValue), SpecialType.System_Int64) =>
					new BigIntLiteral(new System.Numerics.BigInteger(long.MaxValue), $"{long.MaxValue}n"),
				(nameof(long.MinValue), SpecialType.System_Int64) =>
					new BigIntLiteral(new System.Numerics.BigInteger(long.MinValue), $"{long.MinValue}n"),

				// decimal 最大/最小值保持为精确数值字面量
				(nameof(decimal.MaxValue), SpecialType.System_Decimal) =>
					new NumericLiteral((double)decimal.MaxValue, decimal.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture)),
				(nameof(decimal.MinValue), SpecialType.System_Decimal) =>
					new NumericLiteral((double)decimal.MinValue, decimal.MinValue.ToString(System.Globalization.CultureInfo.InvariantCulture)),

				// 其他整数类型（int, short, sbyte 等）保持原样，会作为字面量处理
				_ => symbol.HasConstantValue
					? BuildValueLiteral(symbol.Type, symbol.ConstantValue) ?? Null
					: new Identifier(Util.GetConfigOrSymbolName(symbol))
			};
		}

		return new Identifier(Util.GetConfigOrSymbolName(symbol));
	}

	private static bool TryBuildECMAScriptEnumLiteral(IFieldSymbol symbol, out Expression expression)
	{
		expression = null!;
		if (!symbol.HasConstantValue ||
			symbol.ContainingType?.TypeKind != TypeKind.Enum ||
			symbol.ContainingAssembly?.Name != "ECMAScript")
			return false;

		var alias = Util.GetSymbolConfigName(symbol);
		if (string.IsNullOrEmpty(alias))
			return false;

		expression = new StringLiteral(alias!, $"\"{alias!.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"");
		return true;
	}

	private static bool IsDateLikeType(ITypeSymbol? type)
	{
		if (type is null)
			return false;

		var displayName = type.OriginalDefinition.ToDisplayString(Format.NameFormat);
		return type.SpecialType == SpecialType.System_DateTime ||
			displayName == "System.DateOnly";
	}

	private static bool ShouldInvokeAliasedPropertyGetter(IPropertyReferenceOperation operation, string alias)
	{
		if (operation.Instance is null || operation.Arguments.Length != 0 || string.IsNullOrEmpty(alias))
			return false;

		if (!IsDateLikeType(operation.Instance.Type))
			return false;

		return alias is "getDate" or "getHours" or "getMilliseconds" or "getMinutes" or "getSeconds" or "getFullYear";
	}

	private static Expression BuildAliasedPropertyAccess(Expression instance, string propertyName, bool optional, bool invoke)
	{
		var member = new MemberExpression(instance, new Identifier(propertyName), computed: false, optional: optional);
		if (!invoke)
			return member;

		return new CallExpression(member, NodeList.Empty<Expression>(), optional: false);
	}

	private static Expression BuildArrayFrom(Expression value) =>
		new CallExpression(
			new MemberExpression(new Identifier("Array"), new Identifier("from"), computed: false, optional: false),
			NodeList.From(value),
			optional: false);

	private static Expression BuildInstanceMethodCall(Expression instance, string methodName, params Expression[] arguments) =>
		new CallExpression(
			new MemberExpression(instance, new Identifier(methodName), computed: false, optional: false),
			NodeList.From(arguments),
			optional: false);

	private static bool TryBuildIntrinsicMethodInvocation(IMethodSymbol method, Expression? instance, List<Expression> arguments, out Expression? expression)
	{
		expression = null;
		if (method.ContainingType is null)
			return false;

		var containingType = method.ContainingType.OriginalDefinition.ToDisplayString(Format.NameFormat);
		if (method.ContainingType.SpecialType == SpecialType.System_String || containingType == "string")
		{
			if (method.IsStatic)
			{
				expression = method.Name switch
				{
					"Join" when arguments.Count == 2 =>
						BuildInstanceMethodCall(BuildArrayFrom(arguments[1]), "join", arguments[0]),
					_ => null
				};

				if (expression is not null)
					return true;
			}
			else if (instance is not null)
			{
				expression = method.Name switch
				{
					"Split" when arguments.Count >= 1 =>
						BuildInstanceMethodCall(instance, "split", arguments[0]),
					"PadLeft" when arguments.Count == 1 =>
						BuildInstanceMethodCall(instance, "padStart", arguments[0]),
					"PadLeft" when arguments.Count == 2 =>
						BuildInstanceMethodCall(instance, "padStart", arguments[0], arguments[1]),
					"PadRight" when arguments.Count == 1 =>
						BuildInstanceMethodCall(instance, "padEnd", arguments[0]),
					"PadRight" when arguments.Count == 2 =>
						BuildInstanceMethodCall(instance, "padEnd", arguments[0], arguments[1]),
					"ToCharArray" when arguments.Count == 0 =>
						BuildInstanceMethodCall(instance, "split", new StringLiteral("", "\"\"")),
					"ToCharArray" when arguments.Count == 2 =>
						BuildInstanceMethodCall(
							BuildInstanceMethodCall(
								instance,
								"substring",
								arguments[0],
								new NonLogicalBinaryExpression(Operator.Addition, arguments[0], arguments[1])),
							"split",
							new StringLiteral("", "\"\"")),
					"ToLowerInvariant" when arguments.Count == 0 =>
						BuildInstanceMethodCall(instance, "toLowerCase"),
					"ToUpperInvariant" when arguments.Count == 0 =>
						BuildInstanceMethodCall(instance, "toUpperCase"),
					"Remove" when arguments.Count == 1 =>
						BuildInstanceMethodCall(instance, "slice", new NumericLiteral(0, "0"), arguments[0]),
					"Remove" when arguments.Count == 2 =>
						new NonLogicalBinaryExpression(
							Operator.Addition,
							BuildInstanceMethodCall(instance, "slice", new NumericLiteral(0, "0"), arguments[0]),
							BuildInstanceMethodCall(
								instance,
								"slice",
								new NonLogicalBinaryExpression(Operator.Addition, arguments[0], arguments[1]))),
					"Insert" when arguments.Count == 2 =>
						new NonLogicalBinaryExpression(
							Operator.Addition,
							new NonLogicalBinaryExpression(
								Operator.Addition,
								BuildInstanceMethodCall(instance, "slice", new NumericLiteral(0, "0"), arguments[0]),
								arguments[1]),
							BuildInstanceMethodCall(instance, "slice", arguments[0])),
					_ => null
				};

				if (expression is not null)
					return true;
			}
		}

		if (containingType == "System.Linq.Enumerable")
		{
			expression = method.Name switch
			{
				"Where" when arguments.Count == 2 =>
					new CallExpression(
						new MemberExpression(
							BuildArrayFrom(arguments[0]),
							new Identifier("filter"),
							computed: false,
							optional: false),
						NodeList.From(arguments[1]),
						optional: false),
				"Select" when arguments.Count == 2 =>
					new CallExpression(
						new MemberExpression(
							BuildArrayFrom(arguments[0]),
							new Identifier("map"),
							computed: false,
							optional: false),
						NodeList.From(arguments[1]),
						optional: false),
				"ToList" when arguments.Count == 1 =>
					BuildArrayFrom(arguments[0]),
				_ => null
			};

			if (expression is not null)
				return true;
		}

		if (instance is null)
			return false;

		if (containingType == "System.Numerics.BigInteger")
		{
			expression = method.Name switch
			{
				nameof(System.Numerics.BigInteger.CompareTo) when arguments.Count == 1 =>
					new ConditionalExpression(
						new NonLogicalBinaryExpression(Operator.LessThan, instance, arguments[0]),
						new NumericLiteral(-1, "-1"),
						new ConditionalExpression(
							new NonLogicalBinaryExpression(Operator.GreaterThan, instance, arguments[0]),
							new NumericLiteral(1, "1"),
							new NumericLiteral(0, "0"))),
				nameof(System.Numerics.BigInteger.Equals) when arguments.Count == 1 =>
					new NonLogicalBinaryExpression(Operator.StrictEquality, instance, arguments[0]),
				nameof(object.ToString) when arguments.Count == 0 =>
					new CallExpression(
						new MemberExpression(instance, new Identifier("toString"), computed: false, optional: false),
						NodeList.Empty<Expression>(),
						optional: false),
				_ => null
			};

			if (expression is not null)
				return true;
		}

		return false;
	}

	/// <summary>
	/// 处理数组元素访问操作，不支持多维数组
	/// C# 示例：
	/// array[0]        // 一维数组访问
	/// array[i, j]     // 多维数组访问（不支持）
	/// array[^1]       // 从末尾开始的索引访问
	/// 复杂情况：array[1..^4] 转换为 array.slice(1, array.length - 4)
	/// 转换结果：array[0]/不支持多维数组/array[array.length - 1]
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitArrayElementReference(IArrayElementReferenceOperation operation, SenseArgument argument)
	{
		if (operation.Indices.Length == 0)
			return HandleTransformationFailure<Node>(operation, "Array access requires at least one index.");

		Expression expr = Translate<Expression>(operation.ArrayReference, argument);
		for (var i = 0; i < operation.Indices.Length; i++)
		{
			var indexOperation = operation.Indices[i];
			if (indexOperation is IRangeOperation && i != operation.Indices.Length - 1)
				return HandleTransformationFailure<Node>(operation, "Range indexing is only supported on the final array dimension.");

			expr = BuildArrayIndexAccess(expr, indexOperation);
		}
		return expr;

		Expression BuildArrayIndexAccess(Expression target, IOperation indexOperation)
		{
			if (indexOperation is IUnaryOperation unary && unary.OperatorKind == UnaryOperatorKind.Hat)
			{
				var lengthAccess = new MemberExpression(target, new Identifier("length"), computed: false, optional: false);
				var innerIndex = Translate<Expression>(unary.Operand, argument);
				var indexCalculation = new NonLogicalBinaryExpression(Operator.Subtraction, lengthAccess, innerIndex);
				return new MemberExpression(target, indexCalculation, computed: true, optional: false);
			}
			else if (indexOperation is IImplicitIndexerReferenceOperation implicitIndexer)
			{
				var instance = Translate<Expression>(implicitIndexer.Instance, argument);
				var indexArgument = Translate<Expression>(implicitIndexer.Argument, argument);
				var lengthAccess = new MemberExpression(instance, new Identifier("length"), computed: false, optional: false);
				if (implicitIndexer.Argument is IUnaryOperation indexUnaryOp && indexUnaryOp.OperatorKind == UnaryOperatorKind.Hat)
					indexArgument = Translate<Expression>(indexUnaryOp.Operand, argument);
				var indexCalculation = new NonLogicalBinaryExpression(Operator.Subtraction, lengthAccess, indexArgument);
				return new MemberExpression(instance, indexCalculation, computed: true, optional: false);
			}
			else if (indexOperation is IRangeOperation range)
			{
				var start = range.LeftOperand is IUnaryOperation leftUnary && leftUnary.OperatorKind == UnaryOperatorKind.Hat
					? UnaryHat(target, leftUnary)
					: Translate<Expression>(range.LeftOperand, argument, null);

				var end = range.RightOperand is IUnaryOperation rightUnary && rightUnary.OperatorKind == UnaryOperatorKind.Hat
					? UnaryHat(target, rightUnary)
					: Translate<Expression>(range.RightOperand, argument, null);

				var slice = new MemberExpression(target, new Identifier("slice"), computed: false, optional: false);
				var args = NodeList.Empty<Expression>();
				if (start is not null && end is not null)
				{
					var adjustedEnd = new NonLogicalBinaryExpression(Operator.Addition, end, new NumericLiteral(1, "1"));
					args = NodeList.From(start, adjustedEnd);
				}
				else if (start is not null)
				{
					args = NodeList.From(start);
				}
				else if (end is not null)
				{
					var adjustedEnd = new NonLogicalBinaryExpression(Operator.Addition, end, new NumericLiteral(1, "1"));
					args = NodeList.From<Expression>(new NumericLiteral(0, "0"), adjustedEnd);
				}

				return new CallExpression(slice, args, optional: false);
			}
			else
			{
				var indexCalculation = Translate<Expression>(indexOperation, argument);
				return new MemberExpression(target, indexCalculation, computed: true, optional: false);
			}
		}

		Expression UnaryHat(Expression obj, IUnaryOperation unary)
		{
			var left = new MemberExpression(obj, new Identifier("length"), computed: false, optional: false);
			var right = Translate<Expression>(unary.Operand, argument);
			return new NonLogicalBinaryExpression(Operator.Subtraction, left, right);
		}
	}

	/// <summary>
	/// 处理隐式索引器引用操作
	/// C# 示例：
	/// array[^1]                           // 从末尾开始的索引
	/// array[^n]                           // 从末尾开始的第n个位置
	/// array[^0]                           // 从末尾开始的第0个位置（等同于array.length）
	/// 转换结果：直接生成最简单的 array[array.length - n] 表达式
	/// 利用C#强类型系统，避免不必要的运行时检测，生成高效简洁的代码
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitImplicitIndexerReference(IImplicitIndexerReferenceOperation operation, SenseArgument argument)
	{
		// 隐式索引器引用的直接AST转换，生成最简洁的代码
		var instance = Translate<Expression>(operation.Instance, argument);
		var indexArgument = Translate<Expression>(operation.Argument, argument);
		// 生成 array.length 访问
		var lengthAccess = new MemberExpression(instance, new Identifier("length"), computed: false, optional: false);
		if (operation.Argument is IUnaryOperation indexUnaryOp && indexUnaryOp.OperatorKind == UnaryOperatorKind.Hat)
			indexArgument = Translate<Expression>(indexUnaryOp.Operand, argument);
		// 处理从末尾开始的索引（^n），转换为 length - n
		// 普通索引计算，不是从末尾开始的索引
		// 这种情况可能出现在显式使用 Index.FromEnd() 等场景
		var indexCalculation = new NonLogicalBinaryExpression(Operator.Subtraction, lengthAccess, indexArgument);

		// 直接返回数组访问表达式：array[array.length - n]
		return new MemberExpression(instance, indexCalculation, computed: true, optional: false);
	}

	/// <summary>
	/// 处理局部变量引用操作
	/// C# 示例：
	/// int localVar = 5;
	/// Console.WriteLine(localVar);  // localVar 引用
	/// 转换结果：localVar
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitLocalReference(ILocalReferenceOperation operation, SenseArgument argument)
	{
		return new Identifier(operation.Local.Name);
	}

	/// <summary>
	/// 处理参数引用操作
	/// C# 示例：
	/// void Method(int param) {
	///     Console.WriteLine(param);  // param 引用
	/// }
	/// 转换结果：param
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitParameterReference(IParameterReferenceOperation operation, SenseArgument argument)
	{
		return new Identifier(operation.Parameter.Name);
	}

	/// <summary>
	/// 处理字段引用操作
	/// C# 示例：
	/// obj.field        // 实例字段访问
	/// MyClass.field    // 静态字段访问
	/// 转换结果：obj.field / MyClass.field
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitFieldReference(IFieldReferenceOperation operation, SenseArgument argument)
	{
		// 处理字段的实例对象
		var instance = Translate<Expression>(operation.Instance, argument, null);

		// 检查白名单映射
		// 字段没有 GetMethod/SetMethod，直接使用字段符号进行白名单查询
		var mapperExpr = GetWhiteListExpression(operation.Field, argument, [], instance, out var alias);
		if (mapperExpr is not null)
			return mapperExpr;

		// 对于实例字段访问，需要创建成员访问表达式
		// ImplicitReceiver 指那些语法上不需要、也不能写 this 的隐式实例引用
		if (operation.Instance is IInstanceReferenceOperation instanceReferenceOp &&
			instanceReferenceOp.ReferenceKind == InstanceReferenceKind.ImplicitReceiver)
		{
			// 隐式接收者（如对象初始化器中的字段引用）
			// 如果是常量字段，返回字面量；否则返回字段名
			var fieldExpr = GetFieldName(operation, operation.Field);
			return fieldExpr;
		}

		// 获取字段名称（支持别名）
		var fieldName = string.IsNullOrEmpty(alias)
			? operation.Field.Name
			: alias;

		var property = new Identifier(fieldName!);
		if (instance is not null)
		{
			var optional = operation.Instance is IConditionalAccessInstanceOperation;
			return new MemberExpression(instance, property, false, optional);
		}

		// 静态成员：生成完整的限定名
		// public 静态类带[ECMAScriptModule]是模块类
		if (operation.Field.IsStatic && operation.Field.ContainingType is not null)
		{
			if (TryBuildImportedModuleMember(operation.Field.ContainingType, fieldName!, argument, out var importedMember) &&
				importedMember is not null)
				return importedMember;

			if (operation.Field.IsConst)
				return GetFieldName(operation, operation.Field);

			var containing = BuildFullTypeName(operation.Field.ContainingType, argument);
			if (containing is not null)
				return new MemberExpression(containing, property, computed: false, optional: false);

			var qualified = TryBuildStaticQualifiedMemberFromSyntax(operation.Syntax, fieldName!);
			if (qualified is not null)
				return qualified;
		}

		return operation.Instance is null
			? GetFieldName(operation, operation.Field)
			: property;
	}

	/// <summary>
	/// 处理属性引用操作
	/// C# 示例：
	/// obj.Property     // 实例属性访问
	/// MyClass.Property // 静态属性访问
	/// 转换结果：obj.property / MyClass.property
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitPropertyReference(IPropertyReferenceOperation operation, SenseArgument argument)
	{
		if (operation.Property.Name == "Rank" &&
			operation.Instance?.Type is IArrayTypeSymbol arrayType)
			return new NumericLiteral(arrayType.Rank, arrayType.Rank.ToString());

		// 处理属性调用的实例对象
		var instance = Translate<Expression>(operation.Instance, argument, null);
		var arguments = new List<Expression>(operation.Arguments.Length);
		foreach (var propertyArgument in operation.Arguments)
		{
			var argContext = propertyArgument.Parameter?.RefKind is RefKind.Out
				? argument.With(Sense.OutParameter)
				: argument;
			arguments.Add(Translate<Expression>(propertyArgument.Value, argContext));
		}

		// 检查白名单映射
		var mapperExpr = GetWhiteListExpression(operation.Property.GetMethod!, argument, arguments, instance, out var alias);
		if (mapperExpr is not null)
			return mapperExpr;

		// 获取方法名称
		var propertyName = string.IsNullOrEmpty(alias)
			? Util.GetConfigOrSymbolName(operation.Property)
			: alias;

		var property = new Identifier(propertyName!);
		if (instance is not null)
		{
			var optional = operation.Instance is IConditionalAccessInstanceOperation;
			return BuildAliasedPropertyAccess(instance, propertyName!, optional, ShouldInvokeAliasedPropertyGetter(operation, propertyName!));
		}

		// todo：后续需要清理和白名单整合
		// 静态成员：生成完整的限定名（如 DateTime.Now）
		// 检查属性是否是静态成员
		if (operation.Property.IsStatic && operation.Property.ContainingType is not null)
		{
			if (TryBuildImportedModulePropertyAccess(operation.Property, argument, out var importedProperty) &&
				importedProperty is not null)
				return importedProperty;

			// 生成类型标识符作为对象
			var containing = BuildFullTypeName(operation.Property.ContainingType, argument);
			if (containing is not null)
				return new MemberExpression(containing, property, computed: false, optional: false);
		}

		return property;
	}

	/// <summary>
	/// 处理方法引用操作（不调用）
	/// C# 示例：
	/// Action action = obj.Method;  // 方法引用（委托）
	/// 转换结果：obj.method
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitMethodReference(IMethodReferenceOperation operation, SenseArgument argument)
	{
		// 如果是白名单方法调用，需要生成本地代理方法
		// 生成代理方法参数
		var name = GetUniqueName(operation);
		var count = operation.Method.Parameters.Length + (operation.Method.IsStatic ? 0 : 1);
		var args = Enumerable.Range(0, count)
			.Select(i => new Identifier($"{name}${i}") as Expression)
			.ToList();

		var valueExpr = GetWhiteListExpression(operation.Method, argument, args, out var alias);
		if (valueExpr is not null)
		{
			// 生成箭头函数表达式作为代理方法
			var func = new ArrowFunctionExpression(
				NodeList.From<Node>(args),
				valueExpr,
				expression: false,
				async: false
			);

			// 方法内不含this访问，直接返回箭头函数；否则需要绑定this
			return func;
		}

		var instance = Translate<Expression>(operation.Instance, argument, null);
		var methodName = string.IsNullOrEmpty(alias) ? Util.GetConfigOrSymbolName(operation.Method) : alias;
		var property = new Identifier(methodName!);
		
		Expression callee = property;
		if (instance is null)
		{
			if (operation.Method.IsStatic)
			{
				if (TryBuildImportedModuleMember(operation.Method.ContainingType, methodName!, argument, out var importedMethod) &&
					importedMethod is not null)
					callee = importedMethod;
				else
				{
				var containing = BuildFullTypeName(operation.Method.ContainingType, argument);
				if (containing is not null)
					callee = new MemberExpression(containing, property, computed: false, optional: false);
				else
				{
					var qualified = TryBuildStaticQualifiedMemberFromSyntax(operation.Syntax, methodName!);
					if (qualified is not null)
						callee = qualified;
				}
				}
			}
		}
		else
		{
			callee = operation.Method.MethodKind != MethodKind.DelegateInvoke
				? new MemberExpression(instance, property, computed: false, optional: false)
				: instance;

			// 实例方法组必须绑定到实际接收者，而不是当前 lexical this。
			callee = new CallExpression(
				callee: new MemberExpression(callee, new Identifier("bind"), computed: false, optional: false),
				args: NodeList.From<Expression>(instance),
				false);
		}

		return callee;
	}

	/// <summary>
	/// 处理实例引用操作（this 关键字）
	/// C# 示例：
	/// this.Property   // 引用当前实例
	/// this            // 直接使用 this
	/// 转换结果：this
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitInstanceReference(IInstanceReferenceOperation operation, SenseArgument argument)
	{
		// InstanceReferenceKind
		// ContainingTypeInstance - 语言特性：类实例引用
		// ImplicitReceiver - 语言特性：对象初始化
		// PatternInput - 语言特性：模式匹配
		// InterpolatedStringHandler - 语言特性：内插字符串 

		if (operation.ReferenceKind == InstanceReferenceKind.ContainingTypeInstance)
			return new ThisExpression();

		return null;
	}

	/// <summary>
	/// 处理方法调用操作
	/// C# 示例：
	/// obj.Method(arg1, arg2)      // 实例方法调用
	/// StaticClass.Method(arg)     // 静态方法调用
	/// obj.ExtensionMethod(arg)     // 扩展方法调用
	/// 转换结果：obj.method(arg1, arg2) / staticClass.method(arg) / obj.extensionMethod(arg)
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitInvocation(IInvocationOperation operation, SenseArgument argument)
	{
		// 处理方法调用的实例对象
		var instance = Translate<Expression>(operation.Instance, argument, null);
		var refParas = new List<Expression>();
		var hasReturn = !operation.TargetMethod.ReturnsVoid;

		// 处理方法调用的参数
		var arguments = new List<Expression>();
		foreach (var arg in operation.Arguments)
		{
			// 为 out 参数传递 OutParameter 上下文
			var argContext = arg.Parameter?.RefKind is RefKind.Out
				? argument.With(Sense.OutParameter)
				: argument;
			var right = TranslateTupleForTarget(arg.Value, arg.Parameter?.Type, argContext);
			// ref 引用 或 out 变量引用，记住顺序
			if (arg.Parameter?.RefKind is RefKind.Out or RefKind.Ref)
				refParas.Add(right);

			// 当作普通参数传入
			arguments.Add(right);
		}

		// 检查白名单映射
		var mapperExpr = GetWhiteListExpression(operation.TargetMethod, argument, arguments, instance, out var alias);
		if (mapperExpr is not null)
			return BuildInvExpr(hasReturn, mapperExpr, refParas, argument);

		if (TryBuildIntrinsicMethodInvocation(operation.TargetMethod, instance, arguments, out var intrinsicExpr) &&
			intrinsicExpr is not null)
			return BuildInvExpr(hasReturn, intrinsicExpr, refParas, argument);

		// 判断方法调用的类型
		var methodName = string.IsNullOrEmpty(alias) ? Util.GetConfigOrSymbolName(operation.TargetMethod) : alias;
		var property = new Identifier(methodName!);
		Expression callee = property;
		if (instance is null)
		{
			if (operation.TargetMethod.IsStatic)
			{
				if (TryBuildImportedModuleMember(operation.TargetMethod.ContainingType, methodName!, argument, out var importedMethod) &&
					importedMethod is not null)
					callee = importedMethod;
				else
				{
				var containing = BuildFullTypeName(operation.TargetMethod.ContainingType, argument);
				if (containing is not null)
					callee = new MemberExpression(containing, property, computed: false, optional: false);
				else
				{
					var qualified = TryBuildStaticQualifiedMemberFromSyntax(operation.Syntax, methodName!);
					if (qualified is not null)
						callee = qualified;
				}
				}
			}
		}
		else
		{
			callee = operation.TargetMethod.MethodKind != MethodKind.DelegateInvoke
				? new MemberExpression(instance, property, computed: false, optional: false)
				: instance;
		}

		var callExpr = new CallExpression(callee, NodeList.From(arguments), optional: false);
		return BuildInvExpr(hasReturn, callExpr, refParas, argument);

		Expression BuildInvExpr(bool hasReturns, in Expression expr, in List<Expression> refs, in SenseArgument ctx)
		{
			var expressions = new List<Expression>();
			if (refs.Count > 0)
			{
				// 如果存在ref参数，需要生成逗号表达式，方法调用存临时变量，然后返写参数
				var tempId = new Identifier(GetUniqueName(operation));
				var declarator = new VariableDeclarator(tempId, null);
				ctx.AddVarDeclarator(declarator, _recursionDepth);

				expressions.Add(new AssignmentExpression(Operator.Assignment, tempId, expr));
				for (var i = 0; i < refs.Count; i++)
				{
					var index = hasReturns ? i + 1 : 0;
					var indexer = new NumericLiteral(index, index.ToString());
					var member = new MemberExpression(tempId, indexer, computed: true, optional: false);
					var assignExpr = new AssignmentExpression(Operator.Assignment, refs[i], member);
					expressions.Add(assignExpr);
				}
				// 最后如果有返回调用结果
				if (hasReturns)
				{
					var indexer = new NumericLiteral(0, "0");
					var member = new MemberExpression(tempId, indexer, computed: true, optional: false);
					expressions.Add(member);
				}
				return new SequenceExpression(NodeList.From(expressions));
			}

			return expr;
		}
	}
}
