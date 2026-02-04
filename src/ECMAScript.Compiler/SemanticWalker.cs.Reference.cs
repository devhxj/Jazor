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
	/// <param name="symbol"></param>
	/// <returns></returns>
	private static string? GetSymbolConfigName(ISymbol symbol)
	{
		var useDescription = true;
		string? configName = null, description = null;
		foreach (var attr in symbol.GetAttributes())
		{
			if (attr.ConstructorArguments.Length == 0)
				continue;

			//ECMAScriptNameAttribute 优先级最高，找到后直接返回
			if (attr.AttributeClass?.Name == "ECMAScriptNameAttribute")
			{
				useDescription = false;
				configName = attr.ConstructorArguments[0].Value?.ToString()?.Trim();
				break;
			}
			else if (attr.AttributeClass?.Name == "DescriptionAttribute")
			{
				var desc = attr.ConstructorArguments[0].Value?.ToString()?.Trim();
				if (desc?.StartsWith("@#") == true)
					description = desc.Substring(2);
			}
		}

		return useDescription ? description : configName;
	}

	private static string GetConfigOrSymbolName(ISymbol symbol)
	{
		var name = GetSymbolConfigName(symbol);
		return string.IsNullOrEmpty(name) ? symbol.Name : name!;
	}

	private static string? GetTypeConfigOrWhiteListName(ITypeSymbol symbol)
	{
		string? name = null;

		// 先查询白名单
		var displayName = symbol.OriginalDefinition.ToDisplayString(Util.NameFormat);
		if (WhiteList.Types.TryGetValue(displayName, out var entry) &&
			entry.Op == WhiteListOp.Replace &&
			!string.IsNullOrEmpty(entry.Value))
			name = entry.Value!;

		// 再取特性配置
		if (string.IsNullOrEmpty(name))
		{
			// 注意 name 为空字符串表示跳过名称，只有为null时才使用symbol name
			name = GetSymbolConfigName(symbol);
			name ??= symbol.Name;
		}

		return name;
	}

	private static Expression? BuildFullTypeName(ITypeSymbol symbol)
	{
		var queue = new Stack<string>();
		var type = symbol;
		while (type is not null)
		{
			var name = GetTypeConfigOrWhiteListName(type);
			if (string.IsNullOrEmpty(name))
				break;
			else
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
		var displayName = operation.Property.GetMethod!.ToDisplayString(Util.NameFormat);
		if (WhiteList.Members.TryGetValue(displayName, out var entry))
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
			var containing = BuildFullTypeName(operation.Property.ContainingType);
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
		var instance = Translate<Expression>(operation.Instance, argument, null);
		string? methodName = null;
		// 检查白名单映射
		var displayName = operation.Method.OriginalDefinition.ToDisplayString(Util.NameFormat);
		if (WhiteList.Members.TryGetValue(displayName, out var entry))
		{
			if (entry.Op == WhiteListOp.Allowed)
				methodName = operation.Method.Name;

			else if (entry.Op == WhiteListOp.Replace)
				methodName = entry.Value;

			else if (entry.Op == WhiteListOp.Import)
			{
				if (string.IsNullOrEmpty(entry.Value))
					return HandleTransformationFailure<Node>(operation, "Import mapping requires a module path.");

				// 生成导入调用
				var tempId = new Identifier(entry.Hash);
				argument.MergeImportSpecifier(entry.Value!, new ImportSpecifier(tempId));
				return tempId;
			}

			else if (entry.Op == WhiteListOp.Equals)
			{
				return new MemberExpression(
					obj: new Identifier("Object"),
					property: new Identifier("is"), computed: false, optional: false);
			}

			else if (entry.Op == WhiteListOp.CompareTo)
			{
				/*
				var functionBody = new FunctionBody(NodeList.From(statements), strict: true);
				var arrowFunction = new ArrowFunctionExpression(
					NodeList.From<Node>(),
					functionBody,
					expression: false,
					async: false
				);*/
			}			
		}
		else
			methodName = GetConfigOrSymbolName(operation.Method);

		if (string.IsNullOrEmpty(methodName))
			return HandleTransformationFailure<Node>(operation, "Method name cannot be determined for invocation.");

		var property = new Identifier(methodName!);
		Expression callee = property;
		if (instance is null)
		{
			if (operation.Method.IsStatic)
			{
				var containing = BuildFullTypeName(operation.Method.ContainingType);
				if (containing is not null)
					callee = new MemberExpression(containing, property, computed: false, optional: false);
			}
		}
		else
		{
			callee = operation.Method.MethodKind != MethodKind.DelegateInvoke
				? new MemberExpression(instance, property, computed: false, optional: false)
				: instance;

			// 若方法内含this访问，需绑定
			callee = new CallExpression(
				callee: new MemberExpression(callee, new Identifier("bind"), computed: false, optional: false),
				args: NodeList.From<Expression>(new ThisExpression()),
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
		var refParas = new Dictionary<MemberExpression, Expression>();
		var hasReturn = !operation.TargetMethod.ReturnsVoid;

		// 处理方法调用的参数
		var arguments = new List<Expression>();
		foreach (var arg in operation.Arguments)
		{
			var right = Translate<Expression>(arg.Value, argument);
			// ref 引用 或 out 变量引用，外部定义一个临时空对象来中转
			if (arg.Parameter?.RefKind is RefKind.Out or RefKind.Ref)
			{
				var temp = new Identifier(GetUniqueName(arg));
				var left = new MemberExpression(temp, new Identifier("value"), false, false);
				// ref 要多一步 ref.value赋值
				var properties = arg.Parameter.RefKind == RefKind.Ref
					? NodeList.From<Node>(new ObjectProperty(
						kind: PropertyKind.Init,
						key: new Identifier("value"),
						value: right,
						computed: false,
						shorthand: false,
						method: false))
					: NodeList.Empty<Node>();
				var init = new ObjectExpression(properties);
				var declarator = new VariableDeclarator(temp, init);
				argument.AddVarDeclarator(declarator, _recursionDepth);
				refParas.Add(left, right);
				arguments.Add(temp);
			}
			else
				arguments.Add(right);
		}

		string? methodName = null;
		// 检查白名单映射
		var displayName = operation.TargetMethod.OriginalDefinition.ToDisplayString(Util.NameFormat);
		if (WhiteList.Members.TryGetValue(displayName, out var entry))
		{
			if (entry.Op == WhiteListOp.Allowed)
				methodName = operation.TargetMethod.Name;

			else if (entry.Op == WhiteListOp.Replace)
				methodName = entry.Value;

			else if (entry.Op == WhiteListOp.Import)
			{
				if (string.IsNullOrEmpty(entry.Value))
					return HandleTransformationFailure<Node>(operation, "Import mapping requires a module path.");

				// 生成导入调用
				var tempId = new Identifier(entry.Hash);
				argument.MergeImportSpecifier(entry.Value!, new ImportSpecifier(tempId));

				// 如果是实例方法调用，插入实例作为第一个参数
				if (instance is not null)
					arguments.Insert(0, instance);

				var temp = new CallExpression(tempId, NodeList.From(arguments), optional: false);
				return BuildInvExpr(hasReturn, temp, refParas, argument);
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

		if (string.IsNullOrEmpty(methodName))
			return HandleTransformationFailure<Node>(operation, "Method name cannot be determined for invocation.");

		// 判断方法调用的类型
		var property = new Identifier(methodName!);
		Expression callee = property;
		if (instance is null)
		{
			if (operation.TargetMethod.IsStatic)
			{
				var containing = BuildFullTypeName(operation.TargetMethod.ContainingType);
				if (containing is not null)
					callee = new MemberExpression(containing, property, computed: false, optional: false);
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


		Expression BuildInvExpr(bool hasReturns, in Expression expr, in Dictionary<MemberExpression, Expression> paras,
			in WalkerArgument argument)
		{
			var expressions = new List<Expression>();
			if (paras.Count > 0)
			{
				// 如果存在ref参数，需要生成逗号表达式，方法调用存临时变量，然后返写参数
				var tempId = new Identifier(GetUniqueName(operation));
				var declarator = new VariableDeclarator(tempId, null);
				argument.AddVarDeclarator(declarator, _recursionDepth);
				expressions.Add(new AssignmentExpression(Operator.Assignment, tempId, expr));
				foreach (var pair in paras)
					expressions.Add(new AssignmentExpression(Operator.Assignment, pair.Value, pair.Key));
				// 最后如果有返回调用结果
				if (hasReturns)
					expressions.Add(tempId);
				return new SequenceExpression(NodeList.From(expressions));
			}

			return expr;
		}
	}
}
