using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using ECMAScript.Common;
using System.Linq;
using System.Collections.Generic;

namespace ECMAScript.Compiler;

public partial class SemanticWalker
{

	/// <summary>
	/// 获取ISymbol的 JavaScript 名称
	/// 优先级：
	/// 1. ECMAScriptNameAttribute
	/// 2. DescriptionAttribute (以 @# 开头)
	/// </summary>
	private static string? GetSymbolConfigName(ISymbol symbol)
	{
		string? configName = null;
		foreach (var attr in symbol.GetAttributes())
		{
			if (attr.ConstructorArguments.Length > 0)
			{
				if (attr.AttributeClass?.Name == "ECMAScriptNameAttribute")
				{
					configName = attr.ConstructorArguments[0].Value?.ToString();
					break;//ECMAScriptNameAttribute 优先级最高，找到后直接返回
				}
				else if (attr.AttributeClass?.Name == "DescriptionAttribute")
				{
					var desc = attr.ConstructorArguments[0].Value?.ToString();
					if (desc?.StartsWith("@#") == true)
						return desc.Substring(2);
				}
			}
		}
		
		return configName;
	}

	private static string GetConfigOrSymbolName(ISymbol symbol)
		=> GetSymbolConfigName(symbol) ?? symbol.Name;

	private static string? GetTypeName(ITypeSymbol symbol)
	{
		// 先取特性配置
		var name = GetSymbolConfigName(symbol);

		// 再查询白名单
		if (string.IsNullOrEmpty(name))
		{
			var displayName = symbol.ToDisplayString(Util.NameFormat);
			if (WhiteList.Types.TryGetValue(displayName, out var entry) &&
				entry.Op == WhiteListOp.Replace &&
				!string.IsNullOrEmpty(entry.Value))
				name = entry.Value!;
		}

		return name;
	}

	private static Expression? BuildTypeName(ITypeSymbol symbol)
	{
		var queue = new Stack<string>();

		var type = symbol;
		while (type is not null)
		{
			var name = GetTypeName(type);
			if (string.IsNullOrEmpty(name))
				break;
			else
				queue.Push(name!);

			type = symbol.ContainingType;
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


	private Expression GetFieldName(IOperation includeOp, IFieldSymbol symbol)
	{
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

				// 浮点类型的最大/最小值
				(nameof(double.MaxValue), SpecialType.System_Double) or
				(nameof(float.MaxValue), SpecialType.System_Single) =>
					new MemberExpression(
						new Identifier("Number"),
						new Identifier("MAX_VALUE"), computed: false, optional: false),
				(nameof(double.MinValue), SpecialType.System_Double) or
				(nameof(float.MinValue), SpecialType.System_Single) =>
					new MemberExpression(
						new Identifier("Number"),
						new Identifier("MIN_VALUE"), computed: false, optional: false),

				// 整数类型的最大/最小值 - 使用安全整数常量或字面量
				// long 类型超出 JavaScript 安全整数范围，使用 MAX/MIN_SAFE_INTEGER
				(nameof(long.MaxValue), SpecialType.System_Int64) =>
					new MemberExpression(
						new Identifier("Number"),
						new Identifier("MAX_SAFE_INTEGER"), computed: false, optional: false),
				(nameof(long.MinValue), SpecialType.System_Int64) =>
					new MemberExpression(
						new Identifier("Number"),
						new Identifier("MIN_SAFE_INTEGER"), computed: false, optional: false),

				// 其他整数类型（int, short, sbyte 等）保持原样，会作为字面量处理
				_ => symbol.HasConstantValue
					? BuildValueLiteral(symbol.ContainingType, symbol.ConstantValue) ?? Null
					: new Identifier(symbol.Name)
			};
		}

		return new Identifier(symbol.Name);
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
	public override Node? VisitArrayElementReference(IArrayElementReferenceOperation operation, WalkerArgument argument)
	{
		if (operation.Indices.Length != 1)
			return HandleTransformationFailure<Node>(operation,
				operation.Indices.Length > 1
					? "Multi-dimensional array access is not supported in JavaScript conversion"
					: "Array access requires at least one index.");

		var expr = Translate<Expression>(operation.ArrayReference, argument);
		var indexOperation = operation.Indices[0];

		// 检查是否是从末尾开始的索引（^n）
		// 处理从末尾开始的索引，转换为 array[array.length - n]
		// 生成 array.length 访问
		if (indexOperation is IUnaryOperation unary && unary.OperatorKind == UnaryOperatorKind.Hat)
		{
			var lengthAccess = new MemberExpression(expr, new Identifier("length"), computed: false, optional: false);
			var innerIndex = Translate<Expression>(unary.Operand, argument);
			var indexCalculation = new NonLogicalBinaryExpression(Operator.Subtraction, lengthAccess, innerIndex);
			return new MemberExpression(expr, indexCalculation, computed: true, optional: false);
		}
		else if (indexOperation is IImplicitIndexerReferenceOperation implicitIndexer)
		{
			// 处理隐式索引器引用（另一种可能的表示方式）
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
			// 处理普通范围操作，转换为 Array.slice
			// 获取范围的起始和结束值
			// 检查起始值是否是从末尾开始的索引（^n）
			var start = range.LeftOperand is IUnaryOperation leftUnary && leftUnary.OperatorKind == UnaryOperatorKind.Hat
				? UnaryHat(expr, leftUnary)
				: Translate<Expression>(range.LeftOperand, argument, null);

			var end = range.RightOperand is IUnaryOperation rightUnary && rightUnary.OperatorKind == UnaryOperatorKind.Hat
				? UnaryHat(expr, rightUnary)
				: Translate<Expression>(range.RightOperand, argument, null);

			// 创建 slice 方法调用
			var slice = new MemberExpression(expr, new Identifier("slice"), computed: false, optional: false);
			var args = NodeList.Empty<Expression>();// 空范围：array[..] -> array.slice() (复制整个数组)

			// 处理不同的范围情况
			if (start is not null && end is not null)
			{
				// 完整范围：array[start..end] -> array.slice(start, end + 1)
				// C# 范围包含结束位置，JavaScript slice 不包含，所以需要 +1
				var adjustedEnd = new NonLogicalBinaryExpression(Operator.Addition, end, new NumericLiteral(1, "1"));
				args = NodeList.From(start, adjustedEnd);
			}
			else if (start is not null)
			{
				// 只有起始：array[start..] -> array.slice(start)
				args = NodeList.From(start);
			}
			else if (end is not null)
			{
				// 只有结束：array[..end] -> array.slice(0, end + 1)
				var adjustedEnd = new NonLogicalBinaryExpression(Operator.Addition, end, new NumericLiteral(1, "1"));
				args = NodeList.From<Expression>(new NumericLiteral(0, "0"), adjustedEnd);
			}

			return new CallExpression(slice, args, optional: false);
		}
		// 注意：步长范围操作（如 array[1..^4..2]）在当前的 Roslyn 操作模型中可能不直接支持
		// 这种情况可能需要通过自定义操作或语法节点处理
		// 如果需要支持，可以在 VisitInvalid 方法中处理特殊的语法节点
		else
		{
			// 普通索引访问
			var indexCalculation = Translate<Expression>(indexOperation, argument);
			return new MemberExpression(expr, indexCalculation, computed: true, optional: false);
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
	public override Node? VisitImplicitIndexerReference(IImplicitIndexerReferenceOperation operation, WalkerArgument argument)
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
	public override Node? VisitLocalReference(ILocalReferenceOperation operation, WalkerArgument argument)
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
	public override Node? VisitParameterReference(IParameterReferenceOperation operation, WalkerArgument argument)
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
	public override Node? VisitFieldReference(IFieldReferenceOperation operation, WalkerArgument argument)
	{
		// 对于静态常量字段（无实例），GetFieldName 返回的是常量表达式
		if (operation.Instance is null)
			return GetFieldName(operation, operation.Field);

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

		// 普通实例字段访问：obj.field
		var expr = Translate<Expression>(operation.Instance, argument);
		var fieldName = operation.Field.Name;
		var property = new Identifier(fieldName);
		return new MemberExpression(expr, property, computed: false, optional: false);
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
	public override Node? VisitPropertyReference(IPropertyReferenceOperation operation, WalkerArgument argument)
	{
		// 处理属性调用的实例对象
		var instance = Translate<Expression>(operation.Instance, argument, null);

		// 获取方法名称
		string? propertyName = null;

		// 检查白名单映射
		var key = operation.Property.GetMethod!.ToDisplayString(Util.NameFormat);
		if (WhiteList.Members.TryGetValue(key, out var entry))
		{
			if (entry.Op == WhiteListOp.Allowed)
				propertyName = operation.Property.Name;

			else if (entry.Op == WhiteListOp.Replace)
				propertyName = entry.Value;

			else if (entry.Op == WhiteListOp.Import)
			{
				if (string.IsNullOrEmpty(entry.Value))
					return HandleTransformationFailure<Node>(operation,
						"Import mapping requires a module path.");

				// 生成导入调用
				var id = new Identifier(entry.Hash);
				argument.MergeImportSpecifier(entry.Value!, new ImportSpecifier(id));

				// 如果是实例属性调用，插入实例作为第一个参数
				var arguments = new List<Expression>();
				if (instance is not null)
					arguments.Add(instance);

				return new CallExpression(id, NodeList.From(arguments), optional: false);
			}
		}
		else
			propertyName = GetConfigOrSymbolName(operation.Property);

		if (string.IsNullOrEmpty(propertyName))
			return HandleTransformationFailure<Node>(operation, "");
				
		var property = new Identifier(propertyName!);
		if (instance is not null)
		{
			var optional = operation.Instance is IConditionalAccessInstanceOperation;
			return new MemberExpression(instance, property, false, optional);
		}

		// todo：后续需要清理和白名单整合
		// 静态成员：生成完整的限定名（如 DateTime.Now）
		// 检查属性是否是静态成员
		if (operation.Property.IsStatic && operation.Property.ContainingType is not null)
		{
			// 生成类型标识符作为对象
			var containing = BuildTypeName(operation.Property.ContainingType);
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
	public override Node? VisitMethodReference(IMethodReferenceOperation operation, WalkerArgument argument)
	{
		// 获取属性名称（支持白名单映射）
		var configName = GetConfigOrSymbolName(operation.Method);
		if (string.IsNullOrEmpty(configName))
		{
			// GetMethod肯定不为null
			// 检查白名单映射
			var display = operation.Method.ToDisplayString(Util.NameFormat);
			if (WhiteList.Members.TryGetValue(display, out var entry))
			{
				if (entry.Op == WhiteListOp.Import)
				{
					// todo：处理导入映射
					// entry.Value 是导入模块路径
					configName = entry.Hash;
				}
				else if (entry.Op == WhiteListOp.Allowed)
					configName = operation.Method.Name;

				else if (entry.Op == WhiteListOp.Replace)
					configName = entry.Value;
			}
		}
		var property = new Identifier(configName ?? operation.Method.Name);
		if (operation.Instance is not null)
		{
			var expr = Translate<Expression>(operation.Instance, argument);
			return new MemberExpression(expr, property, computed: false, optional: false);
		}

		// 静态方法：生成完整的限定名（如 Math.Abs）
		if (operation.Method.IsStatic && operation.Method.ContainingType is not null)
		{
			var typeName = new Identifier(operation.Method.ContainingType.Name);
			return new MemberExpression(typeName, property, computed: false, optional: false);
		}

		return property;
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
	public override Node? VisitInstanceReference(IInstanceReferenceOperation operation, WalkerArgument argument)
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
	public override Node? VisitInvocation(IInvocationOperation operation, WalkerArgument argument)
	{
		// 处理方法调用的实例对象
		var instance = Translate<Expression>(operation.Instance, argument, null);

		// 处理方法调用的参数
		var arguments = new List<Expression>();
		foreach (var arg in operation.Arguments)
		{
			Translate(arguments, arg.Value, argument);
		}

		// 获取方法名称
		string? methodName = null;

		// 检查白名单映射
		var key = operation.TargetMethod.ToDisplayString(Util.NameFormat);
		if (WhiteList.Members.TryGetValue(key, out var entry))
		{
			if (entry.Op == WhiteListOp.Allowed)
				methodName = operation.TargetMethod.Name;
			else if (entry.Op == WhiteListOp.Replace)
				methodName = entry.Value;
			else if (entry.Op == WhiteListOp.Import)
			{
				if (string.IsNullOrEmpty(entry.Value))
					return HandleTransformationFailure<Node>(operation,
						"Import mapping requires a module path.");

				// 生成导入调用
				var id = new Identifier(entry.Hash);
				argument.MergeImportSpecifier(entry.Value!, new ImportSpecifier(id));

				// 如果是实例方法调用，插入实例作为第一个参数
				if (instance is not null)
					arguments.Insert(0, instance);
				return new CallExpression(id, NodeList.From(arguments), optional: false);
			}
			else if (entry.Op == WhiteListOp.Equals || entry.Op == WhiteListOp.CompareTo)
			{
				Expression left, right;
				if (instance is null && arguments.Count == 2)
				{
					left = arguments[0];
					right = arguments[1];
				}
				else if (instance is not null && arguments.Count == 1)
				{
					left = instance;
					right = arguments[0];
				}
				else
					return HandleTransformationFailure<Node>(operation,
						"Equals operation requires an instance and at least one argument.");

				var test = new NonLogicalBinaryExpression(Operator.StrictEquality, left, right);
				return entry.Op == WhiteListOp.CompareTo
					? new ConditionalExpression(
						test: test,
						consequent: new NumericLiteral(0, "0"),
						alternate: new ConditionalExpression(
							test: new NonLogicalBinaryExpression(Operator.GreaterThan, left, right),
							consequent: new NumericLiteral(1, "1"),
							alternate: new NumericLiteral(-1, "-1")
						)
					) : test;
			}
		}
		else
			methodName = GetConfigOrSymbolName(operation.TargetMethod);

		if(string.IsNullOrEmpty(methodName))
			return HandleTransformationFailure<Node>(operation,
				"Method name cannot be determined for invocation.");
		
		var property = new Identifier(methodName!);
		// 判断方法调用的类型
		Expression callee;
		if (instance is null)
		{
			// todo:
			// 可能需要完善多次嵌套类的静态方法调用			
			// 静态方法调用
			if (operation.TargetMethod.IsStatic)
			{
				// 静态方法调用：StaticClass.Method()
				// 生成类型标识符作为对象
				var containing = BuildTypeName(operation.TargetMethod.ContainingType);
				callee = containing is not null
					? new MemberExpression(containing, property, computed: false, optional: false)
					: property;
			}
			else
				callee = property;// 扩展方法调用：ExtensionMethod(arg)

		}
		else
		{
			callee = new MemberExpression(
				instance,
				property,
				computed: false,
				optional: false
			);
		}

		return new CallExpression(callee, NodeList.From(arguments), optional: false);
	}

}
