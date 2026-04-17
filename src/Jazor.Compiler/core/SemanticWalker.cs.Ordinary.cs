using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

public partial class SemanticWalker
{
	/// <summary>
	/// 处理代码块操作
	/// C# 示例：
	/// {
	///     int x = 5;
	///     Console.WriteLine(x);
	/// }
	/// 转换结果：根据上下文返回 NestedBlockStatement、FunctionBody 或 StaticBlock
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitBlock(IBlockOperation operation, SenseArgument argument)
	{
		var ctx = argument.WithNewScope();
		var pendingStatements = new List<Statement>();
		foreach (var stmt in operation.Operations)
		{
			var node = Visit(stmt, ctx);

			if (node is Statement statement)
				pendingStatements.Add(statement);

			else if (node is Expression expr)
			{
				// 剔除等于0的情况，因为它在JavaScript中没有副作用，不需要生成语句
				if (expr is SequenceExpression seqExpr)
				{
					if (seqExpr.Expressions.Count == 1)
						pendingStatements.Add(new NonSpecialExpressionStatement(seqExpr.Expressions[0]));

					else if (seqExpr.Expressions.Count > 1)
						pendingStatements.Add(new NonSpecialExpressionStatement(expr));
				}
				else
					pendingStatements.Add(new NonSpecialExpressionStatement(expr));
			}

			else
				HandleTransformationFailure<Node>(stmt, $"{stmt.Kind} could not be translated to JavaScript.");
		}

		// 所有 stmt 处理完后，将 out/pattern 变量声明集中提升到块顶
		// C# 编译器保证同一作用域内不会有同名变量，提升是安全的
		var statements = new List<Statement>();
		if (ctx.HasVarDeclarator)
		{
			var declarators = ctx.FlushVarDeclarator();
			statements.Add(new VariableDeclaration(VariableDeclarationKind.Let, declarators));
		}
		statements.AddRange(pendingStatements);

		// 根据上下文判断返回不同类型的语句块
		if (argument.Sense == Sense.FunctionBody)
			return new FunctionBody(NodeList.From(statements), strict: true);

		if (argument.Sense == Sense.StaticBlock)
			return new StaticBlock(NodeList.From(statements));

		return new NestedBlockStatement(NodeList.From(statements));
	}

	/// <summary>
	/// 处理方法体操作
	/// C# 示例：
	/// void Method() { /* 方法体 */ }
	/// int GetValue() => 42;  // 表达式体
	/// 转换结果：转换为 FunctionBody
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitMethodBodyOperation(IMethodBodyOperation operation, SenseArgument argument)
	{
		// 如果有块体，直接访问块
		if (operation.BlockBody is not null)
			return Visit(operation.BlockBody, argument);

		// 如果有表达式体，转换为返回语句
		if (operation.ExpressionBody is not null)
		{
			if (operation.ExpressionBody is IBlockOperation blockExpression)
				return Visit(blockExpression, argument);

			var expr = Translate<Expression>(operation.ExpressionBody, argument);
			var returnStmt = new ReturnStatement(expr);
			return new FunctionBody(NodeList.From<Statement>(returnStmt), strict: true);
		}

		return HandleTransformationFailure<Node>(operation, "Method body has neither block nor expression body.");
	}

	/// <summary>
	/// 处理构造函数体操作
	/// C# 示例：
	/// class MyClass {
	///     MyClass() { /* 构造函数体 */ }
	/// }
	/// 转换结果：转换为 FunctionBody
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitConstructorBodyOperation(IConstructorBodyOperation operation, SenseArgument argument)
	{
		// 如果有块体，直接访问块
		if (operation.BlockBody is not null)
			return Visit(operation.BlockBody, argument);

		return HandleTransformationFailure<Node>(operation, "Constructor body has no block body.");
	}

	/// <summary>
	/// 处理标签语句操作
	/// C# 示例：
	/// label1:
	///     Console.WriteLine("Labeled statement");
	/// goto label1;
	/// 转换结果：label1: console.log("Labeled statement");
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitLabeled(ILabeledOperation operation, SenseArgument argument)
	{
		var label = new Identifier(operation.Label.Name);

		Statement statement;
		if (operation.Operation is null)
			statement = new EmptyStatement();
		else
		{
			var expr = TranslateExpression(operation.Operation, argument);
			statement = new NonSpecialExpressionStatement(expr);
		}

		return new LabeledStatement(label, statement);
	}

	/// <summary>
	/// 处理分支操作（break/continue）
	/// C# 示例：
	/// break;           // 跳出循环
	/// continue;        // 继续下一次循环
	/// break label1;    // 跳出到指定标签
	/// 转换结果：break; / continue; / break label1;
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitBranch(IBranchOperation operation, SenseArgument argument)
	{
		return operation.BranchKind switch
		{
			BranchKind.Break => new BreakStatement(null),
			BranchKind.Continue => new ContinueStatement(null),
			BranchKind.GoTo => HandleTransformationFailure<Node>(operation, "Goto statements are not supported in JavaScript."),
			_ => null
		};
	}

	/// <summary>
	/// 处理空语句操作
	/// C# 示例：; // 空语句
	/// 转换结果：; // JavaScript 空语句
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitEmpty(IEmptyOperation operation, SenseArgument argument)
		=> new EmptyStatement();

	/// <summary>
	/// 处理 return 语句操作
	/// C# 示例：
	/// return;          // 无返回值
	/// return value;    // 返回值
	/// 转换结果：return; / return value;
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitReturn(IReturnOperation operation, SenseArgument argument)
	{
		if (operation.ReturnedValue is null)
			return WithOrigin(new ReturnStatement(null), operation);

		// return 也是 tuple 运行时协议切换边界。
		// 如果返回表达式当前 tuple 视图和函数声明返回类型不同，
		// 这里需要按目标返回类型显式 remap，而不是直接返回源对象。
		var exp = TranslateTupleForTarget(operation.ReturnedValue, GetTupleReturnTargetType(operation), argument);
		return WithOrigin(new ReturnStatement(exp), operation);
	}

	/// <summary>
	/// 处理表达式语句操作
	/// C# 示例：
	/// Method();        // 方法调用表达式语句
	/// x++;            // 自增表达式语句
	/// 转换结果：method(); / x++;
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitExpressionStatement(IExpressionStatementOperation operation, SenseArgument argument)
	{
		return Translate<Node>(operation.Operation, argument);
	}

	/// <summary>
	/// 处理局部函数操作
	/// C# 示例：
	/// void LocalFunction(int param) {
	///     Console.WriteLine(param);
	/// }
	/// LocalFunction(42);
	/// 转换结果：function localFunction(param) { console.log(param); }
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitLocalFunction(ILocalFunctionOperation operation, SenseArgument argument)
	{
		var id = new Identifier(operation.Symbol.Name);
		var parameters = new List<Node>();
		foreach (var param in operation.Symbol.Parameters)
			parameters.Add(new Identifier(param.Name));

		// 函数边界：隔离 _declarators，共享 _specifiers（import 需跨函数边界传播）
		var bodyCtx = argument.WithNewScope();
		var pendingStatements = new List<Statement>();
		if (operation.Body is not null)
		{
			foreach (var stmt in operation.Body.Operations)
			{
				var node = Visit(stmt, bodyCtx);
				if (node is Statement statement)
					pendingStatements.Add(statement);
				else if (node is Expression expr)
					pendingStatements.Add(new NonSpecialExpressionStatement(expr));
				else
					HandleTransformationFailure<Node>(stmt, "Local function statement could not be translated to JavaScript.");
			}
		}

		// 将函数体内的变量声明提升到函数体顶部
		var bodyStatements = new List<Statement>();
		if (bodyCtx.HasVarDeclarator)
		{
			var declarators = bodyCtx.FlushVarDeclarator();
			bodyStatements.Add(new VariableDeclaration(VariableDeclarationKind.Let, declarators));
		}
		bodyStatements.AddRange(pendingStatements);

		var body = new FunctionBody(NodeList.From(bodyStatements), strict: true);

		// 检查函数是否为async或generator
		var isAsync = operation.Symbol.IsAsync;

		// 检查是否返回IEnumerable类型（可能是迭代器）
		var returnTypeName = operation.Symbol.ReturnType.Name;
		var isGenerator = returnTypeName.Contains("IEnumerable") || returnTypeName.Contains("IEnumerator");

		return new FunctionDeclaration(id,
			NodeList.From(parameters),
			body,
			generator: isGenerator,
			@async: isAsync);
	}

	private Expression? BuildValueLiteral(ITypeSymbol? type, object? value)
	{
		// 处理 null 值（必须在类型检查之前，因为 null 没有特定的类型信息）
		if (value is null)
			return Null;

		// 类型信息缺失时报告错误
		if (type is null)
			return null;

		var valueStr = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
		var (mapper, _) = GetMapperType(type);

		// 布尔值字面量：true / false
		if (mapper == TypeMapper.Boolean)
			return new BooleanLiteral((bool)value, ((bool)value).ToString().ToLowerInvariant());

		// 字符串和字符字面量
		else if (mapper == TypeMapper.String)
		{
			if (string.IsNullOrEmpty(valueStr))
				return new StringLiteral("", "\"\"");

			// 为 JavaScript 字符串字面量进行正确的转义处理
			// JavaScript 字符串中需要转义的特殊字符：
			// - 控制字符：\0 (null), \b (backspace), \f (form feed), \n (newline), \r (carriage return), \t (tab), \v (vertical tab)
			// - 特殊字符：\\ (backslash), \" (double quote), \' (single quote)
			// - Unicode 字符大于 0x7F 的可以保留原样（现代 JS 支持 UTF-8）
			var escaped = new System.Text.StringBuilder();
			foreach (char c in valueStr)
			{
				switch (c)
				{
					case '\0':
						escaped.Append("\\0");
						break;
					case '\b':
						escaped.Append("\\b");
						break;
					case '\f':
						escaped.Append("\\f");
						break;
					case '\n':
						escaped.Append("\\n");
						break;
					case '\r':
						escaped.Append("\\r");
						break;
					case '\t':
						escaped.Append("\\t");
						break;
					case '\v':
						escaped.Append("\\v");
						break;
					case '\\':
						escaped.Append("\\\\");
						break;
					case '"':
						escaped.Append("\\\"");
						break;
					// 单引号在双引号字符串中不需要转义，但为了一致性可以保留
					// case '\'':
					//     escaped.Append("\\'");
					//     break;
					default:
						// Unicode 字符可以直接保留（UTF-8 编码）
						escaped.Append(c);
						break;
				}
			}

			// 使用双引号包裹
			var formatted = $"\"{escaped}\"";

			return new StringLiteral(valueStr, formatted);
		}

		//  数值字面量：42 / 3.14
		else if (mapper == TypeMapper.Number)
		{
			if (IsSystemHalfType(type))
			{
				var halfValue = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);

				if (double.IsNaN(halfValue))
					return new Identifier("NaN");
				if (double.IsPositiveInfinity(halfValue))
					return new Identifier("Infinity");
				if (double.IsNegativeInfinity(halfValue))
					return new NonUpdateUnaryExpression(Operator.UnaryNegation, new Identifier("Infinity"));

				return new NumericLiteral(
					halfValue,
					valueStr ?? halfValue.ToString("R", System.Globalization.CultureInfo.InvariantCulture)
				);
			}

			// 对于 decimal 类型，需要处理精度损失问题
			// decimal 范围约为 ±7.9×10²⁸，double 范围约为 ±1.7×10³⁰⁸
			// double 范围更大，不会溢出，但 decimal 有 28-29 位精度，double 只有 15-16 位
			if (type.SpecialType == SpecialType.System_Decimal)
			{
				var decimalValue = Convert.ToDecimal(value);
				return new NumericLiteral(
					(double)decimalValue,
					decimalValue.ToString(System.Globalization.CultureInfo.InvariantCulture)
				);
			}

			// 对于 float 类型，处理特殊值并使用 float 转换以保持单精度格式
			else if (type.SpecialType == SpecialType.System_Single)
			{
				var floatValue = Convert.ToSingle(value);

				// 处理特殊浮点值
				if (float.IsNaN(floatValue))
					return new Identifier("NaN");
				if (float.IsPositiveInfinity(floatValue))
					return new Identifier("Infinity");
				if (float.IsNegativeInfinity(floatValue))
					return new NonUpdateUnaryExpression(Operator.UnaryNegation, new Identifier("Infinity"));

				return new NumericLiteral(
					floatValue,
					floatValue.ToString("R", System.Globalization.CultureInfo.InvariantCulture)
				);
			}

			// 对于 double 类型，处理特殊值并直接使用
			else if (type.SpecialType == SpecialType.System_Double)
			{
				var doubleValue = Convert.ToDouble(value);

				// 处理特殊浮点值
				if (double.IsNaN(doubleValue))
					return new Identifier("NaN");
				if (double.IsPositiveInfinity(doubleValue))
					return new Identifier("Infinity");
				if (double.IsNegativeInfinity(doubleValue))
					return new NonUpdateUnaryExpression(Operator.UnaryNegation, new Identifier("Infinity"));

				return new NumericLiteral(
					doubleValue,
					doubleValue.ToString("R", System.Globalization.CultureInfo.InvariantCulture)
				);
			}

			// 对于整数类型，使用原始字符串格式
			var numberValue = Convert.ToDouble(value);
			if (!string.IsNullOrEmpty(valueStr))
				return new NumericLiteral(numberValue, valueStr);
		}

		// BigInt 字面量：42L / 100UL / -42L -> 42n / 100n / -42n
		else if (mapper == TypeMapper.BigInt)
		{
			// 处理 ulong 类型（无符号 64 位整数）
			if (type.SpecialType == SpecialType.System_UInt64)
			{
				var ulongValue = Convert.ToUInt64(value);
				var bigInt = new System.Numerics.BigInteger(ulongValue);
				return new BigIntLiteral(bigInt, $"{ulongValue}n");
			}

			// 处理 long 类型（有符号 64 位整数）
			else if (type.SpecialType == SpecialType.System_Int64)
			{
				var longValue = Convert.ToInt64(value);
				var bigInt = new System.Numerics.BigInteger(longValue);
				return new BigIntLiteral(bigInt, $"{longValue}n");
			}

			// 处理 Int128 和 UInt128 类型（128 位整数）
			// 这些类型的值会被装箱为 object，需要特殊处理
			else if (value is IFormattable formattable)
			{
				// 使用 InvariantCulture 确保格式一致性
				var invariantStr = formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture);
				if (System.Numerics.BigInteger.TryParse(invariantStr, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var bigIntValue))
					return new BigIntLiteral(bigIntValue, $"{invariantStr}n");
			}

			// 尝试直接解析为 BigInteger（使用 InvariantCulture 确保健壮性）
			else if (System.Numerics.BigInteger.TryParse(valueStr, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var bigIntValue))
				return new BigIntLiteral(bigIntValue, $"{valueStr}n");
		}

		// 不支持的类型报告错误 或 其他类型返回 null（如 Object、Array、Date 等不应出现在字面量中）
		return null;
	}

	/// <summary>
	/// 处理 C# 字面量操作，将其转换为 JavaScript 字面量表达式。
	///
	/// 转换策略说明：
	/// 1. null 值：直接返回 null（必须优先处理，因为 null 没有类型信息）
	/// 2. Boolean: true/false 直接映射
	/// 3. String/Char: 统一转换为双引号字符串，并对特殊字符进行转义
	///    - 控制字符：\0, \b, \f, \n, \r, \t, \v
	///    - 特殊字符：\\, \"
	///    - Unicode 字符直接保留（UTF-8）
	/// 4. Number: 根据 C# 类型进行差异化处理
	///    - 整数类型（byte, short, int, uint）: 保持原格式
	///    - float/double: 使用 "R" 格式符避免精度丢失，处理 NaN/Infinity
	///    - decimal: 转为 double，注意精度损失（28-29 位 → 15-16 位）
	/// 5. BigInt: 64 位及以上整数，添加 n 后缀
	///    - long/ulong: 直接加 n
	///    - Int128/UInt128/BigInteger: 解析后加 n
	///
	/// C# 示例 → JavaScript 转换结果：
	///   null              → null
	///   true              → true
	///   false             → false
	///   "Hello"           → "Hello"
	///   "Line1\nLine2"    → "Line1\nLine2"
	///   "C:\\Path"        → "C:\\Path"
	///   'A'               → "A"
	///   ""                → ""
	///   42                → 42
	///   3.14              → 3.14
	///   3.14f             → 3.14
	///   3.1415926535m     → 3.1415926535 (注意精度损失)
	///   float.NaN         → NaN
	///   float.PositiveInfinity → Infinity
	///   float.NegativeInfinity → -Infinity
	///   42L               → 42n
	///   42UL              → 42n
	///   -42L              → -42n
	///   (long)42          → 42n
	/// </summary>
	/// <param name="operation">当前访问的 ILiteralOperation 操作，包含字面量值和类型信息</param>
	/// <param name="argument">用于存放当前操作内部需要的全局变量定义的上下文（字面量通常不需要）</param>
	/// <returns>转换后的 JavaScript 字面量 Node（BooleanLiteral, StringLiteral, NumericLiteral, BigIntLiteral, NullLiteral 或 Identifier）</returns>
	public override Node? VisitLiteral(ILiteralOperation operation, SenseArgument argument)
	{
		var expr = BuildValueLiteral(operation.Type, operation.ConstantValue.Value);
		if (expr is null)
			// 不支持的类型报告错误 或 其他类型返回 null（如 Object、Array、Date 等不应出现在字面量中）
			return HandleTransformationFailure<Node>(operation, $"Literal type '{operation.Type?.Name}' ({operation.Kind}) cannot be directly translated to JavaScript literal.");

		return WithOrigin(expr, operation);
	}

	/// <summary>
	/// 处理 C# 类型转换操作，将其转换为 JavaScript 表达式。
	///
	/// 转换策略说明：
	/// 1. Number 与 BigInt 之间的显式转换需要生成转换函数调用
	///    - Number → BigInt: 生成 BigInt(value) 调用
	///    - BigInt → Number: 生成 Number(value) 调用
	/// 2. 其他类型转换在 JavaScript 中可以安全忽略，因为 JS 是动态类型语言
	///    - 装箱/拆箱: JS 无需区分值类型和引用类型
	///    - 引用类型转换: JS 运行时动态检查
	///    - as 转换: 语义等价于直接访问
	///
	/// C# 示例 → JavaScript 转换结果：
	///   (long)42           → BigInt(42)      // int 转 long，需要显式转换
	///   (long)x            → BigInt(x)       // 变量转 BigInt
	///   (int)someLong      → Number(someLong) // long 转 int，需要显式转换
	///   (ulong)count       → BigInt(count)   // uint 转 ulong
	///   (int)3.14          → 3.14            // float 转 int，忽略转换
	///   obj as string      → obj             // as 转换，直接返回
	///   (BaseType)derived  → derived         // 引用类型强转，直接返回
	///   object o = 42      → 42              // 装箱，忽略
	///   int i = (int)o     → o               // 拆箱，忽略
	///
	/// 特殊情况：方法组转换为委托类型（如 Action a = MyMethod）
	///           此时 OperationKind 为 None，需要通过语法节点直接处理
	/// </summary>
	/// <param name="operation">当前访问的 IConversionOperation 操作</param>
	/// <param name="argument">用于存放当前操作内部需要的全局变量定义的上下文</param>
	/// <returns>转换后的 JavaScript ESTree Node</returns>
	public override Node? VisitConversion(IConversionOperation operation, SenseArgument argument)
	{
		// tuple 在这里做“边界重映射”：
		// 语义仍按位置对应，但一旦目标静态视图名字不同，就显式生成新的对象协议。
		// 这里只处理 Roslyn 明确表示出来的 conversion；其他边界（参数、赋值、初始化器）
		// 还会通过 TranslateTupleForTarget 主动套同一套规则。
		var tupleProjection = TryTranslateTupleConversion(operation, argument);
		if (tupleProjection is not null)
			return tupleProjection;

		// 处理特殊情况：方法组转换为委托类型
		// class TestClass
		// {
		//     void TestMethod() { Action action = MyMethod; }
		//     void MyMethod() { }
		// }
		var expr = Translate<Expression>(operation.Operand, argument);

		// 处理 Number 与 BigInt 之间的显式转换
		if (operation.Type is not null && operation.Operand.Type is not null)
		{
			var targetType = GetMapperType(operation.Type);
			var operandType = GetMapperType(operation.Operand.Type);

			// Number → BigInt: (long)1 → BigInt(1)
			if (operandType.Mapper == TypeMapper.Number && targetType.Mapper == TypeMapper.BigInt)
				return new CallExpression(new Identifier("BigInt"), NodeList.From(expr), optional: false);

			// BigInt → Number: (int)someLong → Number(someLong)
			if (operandType.Mapper == TypeMapper.BigInt && targetType.Mapper == TypeMapper.Number)
				return new CallExpression(new Identifier("Number"), NodeList.From(expr), optional: false);
		}

		// 其他情况：直接返回操作数（JavaScript 是动态类型）
		// 包括：装箱/拆箱、引用类型转换、as 转换等
		return expr;
	}

	/// <summary>
	/// 处理条件访问操作（可选链操作符）
	/// C# 示例：
	/// obj?.Property               // 属性可选访问
	/// obj?.Method()               // 方法可选调用
	/// 转换结果：obj?.property
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitConditionalAccess(IConditionalAccessOperation operation, SenseArgument argument)
	{
		// 先转换 Operation，然后通过 PatternInput 传递给 WhenNotNull
		var operand = Translate<Expression>(operation.Operation, argument);
		var whenNotNullArg = argument.WithPatternInput(operand);
		var whenNotNull = Translate<Expression>(operation.WhenNotNull, whenNotNullArg);
		return new ChainExpression(whenNotNull);
	}

	/// <summary>
	/// IConditionalAccessInstanceOperation 是一个轻量级的、无子操作的、作为语义占位符的叶子节点。
	/// 它被专门设计用于在 IOperation 树中，作为空条件访问表达式（?.）右侧成员操作的 Instance。
	/// 它的唯一目的是提供类型信息，从而将运行时的短路求值逻辑（由 IConditionalAccessOperation 控制）与编译时的静态语义分析（由成员操作自身完成）完美解耦。
	/// C# 示例：
	/// obj?.Property中的obj?
	/// 转换方式：从 PatternInput 获取（由 VisitConditionalAccess 传递）
	/// 转换结果：obj?
	/// </summary>
	public override Node? VisitConditionalAccessInstance(IConditionalAccessInstanceOperation operation, SenseArgument argument)
	{
		// 从 PatternInput 获取（由 VisitConditionalAccess 传递）
		if (argument.PatternInput is not null)
			return argument.PatternInput;

		return HandleTransformationFailure<Node>(operation, "ConditionalAccessInstance requires PatternInput context from VisitConditionalAccess.");
	}

	/// <summary>
	/// 处理一元运算符操作
	/// C# 示例：
	/// +x              // 正号运算符
	/// -x              // 负号运算符
	/// !condition      // 逻辑非运算符
	/// ~value          // 按位取反运算符
	/// 转换结果：+x / -x / !condition / ~value
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitUnaryOperator(IUnaryOperation operation, SenseArgument argument)
	{
		var operand = Translate<Expression>(operation.Operand, argument);
		if (operation.OperatorKind == UnaryOperatorKind.BitwiseNegation ||
			operation.OperatorKind == UnaryOperatorKind.Not ||
			operation.OperatorKind == UnaryOperatorKind.Plus ||
			operation.OperatorKind == UnaryOperatorKind.Minus)
		{
			// 一元运算
			var @operator = operation.OperatorKind switch
			{
				UnaryOperatorKind.BitwiseNegation => Operator.BitwiseNot,
				UnaryOperatorKind.Not => Operator.LogicalNot,
				UnaryOperatorKind.Plus => Operator.UnaryPlus,
				UnaryOperatorKind.Minus => Operator.UnaryNegation,
				_ => Operator.Unknown
			};
			return new NonUpdateUnaryExpression(@operator, operand);
		}
		else if (operation.OperatorKind == UnaryOperatorKind.True ||
				 operation.OperatorKind == UnaryOperatorKind.False)
		{
			// 将操作数强制转换为布尔值，应该转换为!!(operand) 或 Boolean(operand)
			var innerOperand = new NonUpdateUnaryExpression(Operator.LogicalNot, operand);
			return new NonUpdateUnaryExpression(Operator.LogicalNot, innerOperand);
		}
		else if (operation.OperatorKind == UnaryOperatorKind.Hat)
		{
			// 需要根据上下文语义来生成内容
		}
		else if (operation.OperatorKind == UnaryOperatorKind.None)
		{
			// 对应语义()
			return new ParenthesizedExpression(operand);
		}

		return HandleTransformationFailure<Node>(operation.Operand, "Unary operator operand could not be translated to JavaScript.");
	}

	/// <summary>
	/// 处理二元运算符操作
	/// C# 示例：
	/// a + b           // 加法运算
	/// a - b           // 减法运算
	/// a * b           // 乘法运算
	/// a / b           // 除法运算
	/// a % b           // 取模运算
	/// a == b          // 相等比较
	/// a != b          // 不等比较
	/// a < b           // 小于比较
	/// a > b           // 大于比较
	/// a <= b          // 小于等于比较
	/// a >= b          // 大于等于比较
	/// a &amp;&amp; b          // 逻辑与运算
	/// a || b          // 逻辑或运算
	/// a &amp; b           // 按位与运算
	/// a | b           // 按位或运算
	/// a ^ b           // 按位异或运算
	/// a &lt;&lt; b          // 左移运算
	/// a >> b          // 右移运算
	/// a >>> b         // 无符号右移运算
	/// 转换结果：相同的 JavaScript 运算符
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitBinaryOperator(IBinaryOperation operation, SenseArgument argument)
	{
		var left = Translate<Expression>(operation.LeftOperand, argument);
		var right = Translate<Expression>(operation.RightOperand, argument);

		if (operation.OperatorMethod is not null)
		{
			var mapped = GetWhiteListExpression(operation.OperatorMethod, argument, [left, right], out _);
			if (mapped is not null)
				return mapped;
		}

		var @operator = operation.OperatorKind switch
		{
			BinaryOperatorKind.Add => Operator.Addition,
			BinaryOperatorKind.Subtract => Operator.Subtraction,
			BinaryOperatorKind.Multiply => Operator.Multiplication,
			BinaryOperatorKind.Divide => Operator.Division,
			BinaryOperatorKind.Remainder => Operator.Remainder,
			BinaryOperatorKind.Equals => Operator.StrictEquality,
			BinaryOperatorKind.NotEquals => Operator.StrictInequality,
			BinaryOperatorKind.LessThan => Operator.LessThan,
			BinaryOperatorKind.GreaterThan => Operator.GreaterThan,
			BinaryOperatorKind.LessThanOrEqual => Operator.LessThanOrEqual,
			BinaryOperatorKind.GreaterThanOrEqual => Operator.GreaterThanOrEqual,
			BinaryOperatorKind.ConditionalAnd => Operator.LogicalAnd,
			BinaryOperatorKind.ConditionalOr => Operator.LogicalOr,
			BinaryOperatorKind.And => Operator.BitwiseAnd,
			BinaryOperatorKind.Or => Operator.BitwiseOr,
			BinaryOperatorKind.ExclusiveOr => Operator.BitwiseXor,
			BinaryOperatorKind.LeftShift => Operator.LeftShift,
			BinaryOperatorKind.RightShift => Operator.RightShift,
			BinaryOperatorKind.UnsignedRightShift => Operator.UnsignedRightShift,
			_ => Operator.Unknown
		};

		// 逻辑运算符 → LogicalExpression
		if (@operator is Operator.LogicalAnd or Operator.LogicalOr)
			return new LogicalExpression(@operator, left, right);

		else if (@operator == Operator.Unknown)
			return HandleTransformationFailure<Node>(operation, "Binary operator could not be translated to JavaScript.");

		// 其余 → BinaryExpression
		return new NonLogicalBinaryExpression(@operator, left, right);
	}

	/// <summary>
	/// 处理条件运算符操作（三元运算符）
	/// C# 示例：
	/// condition ? trueValue : falseValue
	/// 转换结果：condition ? trueValue : falseValue
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitConditional(IConditionalOperation operation, SenseArgument argument)
	{
		var alternate = Visit(operation.WhenFalse, argument);
		var consequent = Translate<Node>(operation.WhenTrue, argument);
		var test = Translate<Expression>(operation.Condition, argument);
		if (operation.Syntax is ConditionalExpressionSyntax &&
			consequent is Expression expConsequent &&
			alternate is Expression expAlternate)
		{
			// 这是三元表达式 a ? b : c
			// 生成 JavaScript 的三元表达式
			return new ConditionalExpression(test, expConsequent, expAlternate);
		}
		else if (operation.Syntax is IfStatementSyntax &&
			consequent is Statement ifConsequent)
		{
			// 这是 if 语句
			// 生成 JavaScript 的 if...else 语句
			return new IfStatement(test, ifConsequent, alternate as Statement);
		}

		return HandleTransformationFailure<Node>(operation, "Conditional operator could not be translated to JavaScript.");
	}

	/// <summary>
	/// 处理空合并运算符操作
	/// C# 示例：
	/// value ?? defaultValue
	/// 转换结果：value ?? defaultValue
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitCoalesce(ICoalesceOperation operation, SenseArgument argument)
	{
		var left = Translate<Expression>(operation.Value, argument);
		var right = Translate<Expression>(operation.WhenNull, argument);
		return new LogicalExpression(Operator.NullishCoalescing, left, right);
	}

	/// <summary>
	/// 处理匿名函数操作（Lambda 表达式）
	/// C# 示例：
	/// (x, y) => x + y               // 多参数 Lambda
	/// x => x * 2                   // 单参数 Lambda
	/// () => Console.WriteLine("Hi") // 无参数 Lambda
	/// 转换结果：(x, y) => { return x + y; } / x => { return x * 2; }
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitAnonymousFunction(IAnonymousFunctionOperation operation, SenseArgument argument)
	{
		var parameters = new List<Node>();
		foreach (var param in operation.Symbol.Parameters)
		{
			var paramName = param.Name;
			parameters.Add(new Identifier(paramName));
		}

		// 函数边界：隔离 _declarators，共享 _specifiers（import 需跨函数边界传播）
		var bodyCtx = argument.WithNewScope();
		var pendingStatements = new List<Statement>();
		foreach (var stmt in operation.Body.Operations)
		{
			var node = Visit(stmt, bodyCtx);
			if (node is Statement statement)
				pendingStatements.Add(statement);
			else if (node is Expression expr)
				pendingStatements.Add(new NonSpecialExpressionStatement(expr));
			else
				return HandleTransformationFailure<Node>(stmt, "Anonymous function body statement could not be translated to JavaScript.");
		}

		// 将函数体内的变量声明提升到函数体顶部
		var bodyStatements = new List<Statement>();
		if (bodyCtx.HasVarDeclarator)
		{
			var declarators = bodyCtx.FlushVarDeclarator();
			bodyStatements.Add(new VariableDeclaration(VariableDeclarationKind.Let, declarators));
		}
		bodyStatements.AddRange(pendingStatements);

		var body = new FunctionBody(NodeList.From(bodyStatements), strict: true);

		// 创建箭头函数
		return new ArrowFunctionExpression(
			NodeList.From(parameters), body,
			@async: false,
			expression: false);
	}

	/// <summary>
	/// 处理 await 表达式操作
	/// C# 示例：
	/// await SomeAsyncMethod()     // 等待异步操作完成
	/// 转换结果：await someAsyncMethod()
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitAwait(IAwaitOperation operation, SenseArgument argument)
	{
		var awaitedExpression = Translate<Expression>(operation.Operation, argument);
		return new AwaitExpression(awaitedExpression);
	}

	/// <summary>
	/// 处理简单赋值操作
	/// C# 示例：
	/// x = 5           // 基本赋值
	/// obj.prop = val  // 属性赋值
	/// arr[0] = item   // 数组元素赋值
	/// 转换结果：x = 5 / obj.prop = val / arr[0] = item
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitSimpleAssignment(ISimpleAssignmentOperation operation, SenseArgument argument)
	{
		// tuple 赋值不依赖 Roslyn 恰好插入 conversion。
		// 只要目标静态类型是另一套 tuple 视图，这里就按目标协议主动重映射。
		// 这样：
		//   target = source;
		// 不会因为 IOperation 树里缺少显式 conversion 而漏掉 tuple remap。
		var value = TranslateTupleForTarget(operation.Value, operation.Target.Type, argument);
		if (operation.Target is IDiscardOperation)
			return WithOriginIfMissing(value, operation);

		if (operation.Target is IPropertyReferenceOperation propertyReference &&
			propertyReference.Property.SetMethod is not null)
		{
			var instance = Translate<Expression>(propertyReference.Instance, argument, null);
			var setterArguments = new List<Expression>(propertyReference.Arguments.Length + 1);
			foreach (var propertyArgument in propertyReference.Arguments)
			{
				var argContext = propertyArgument.Parameter?.RefKind is RefKind.Out
					? argument.With(Sense.OutParameter)
					: argument;
				setterArguments.Add(Translate<Expression>(propertyArgument.Value, argContext));
			}
			setterArguments.Add(value);

			var mapperExpr = GetWhiteListExpression(propertyReference.Property.SetMethod, argument, setterArguments, instance, out _);
			if (mapperExpr is not null)
				return WithOriginIfMissing(mapperExpr, operation);
		}

		var target = Translate<Expression>(operation.Target, argument);
		return WithOrigin(new AssignmentExpression(Operator.Assignment, target, value), operation);
	}

	/// <summary>
	/// 处理复合赋值操作
	/// C# 示例：
	/// x += 5          // 加法赋值
	/// x -= 3          // 减法赋值
	/// x *= 2          // 乘法赋值
	/// x /= 4          // 除法赋值
	/// x %= 7          // 取模赋值
	/// x &amp;= 3          // 按位与赋值
	/// x |= 2          // 按位或赋值
	/// x ^= 1          // 按位异或赋值
	/// x &lt;&lt;= 2         // 左移赋值
	/// x >>= 1         // 右移赋值
	/// x >>>= 1        // 无符号右移赋值
	/// 转换结果：x += 5 / x -= 3 / x *= 2 / x /= 4 / x %= 7 / x &amp;= 3 / x |= 2 / x ^= 1 / x &lt;&lt;= 2 / x >>= 1 / x >>>= 1
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitCompoundAssignment(ICompoundAssignmentOperation operation, SenseArgument argument)
	{
		var left = Translate<Expression>(operation.Target, argument);
		var right = Translate<Expression>(operation.Value, argument);
		var @operator = operation.OperatorKind switch
		{
			BinaryOperatorKind.Add => Operator.AdditionAssignment,
			BinaryOperatorKind.Subtract => Operator.SubtractionAssignment,
			BinaryOperatorKind.Multiply => Operator.MultiplicationAssignment,
			BinaryOperatorKind.Divide => Operator.DivisionAssignment,
			BinaryOperatorKind.Remainder => Operator.RemainderAssignment,
			BinaryOperatorKind.And => Operator.BitwiseAndAssignment,
			BinaryOperatorKind.Or => Operator.BitwiseOrAssignment,
			BinaryOperatorKind.ExclusiveOr => Operator.BitwiseXorAssignment,
			BinaryOperatorKind.LeftShift => Operator.LeftShiftAssignment,
			BinaryOperatorKind.RightShift => Operator.RightShiftAssignment,
			BinaryOperatorKind.UnsignedRightShift => Operator.UnsignedRightShiftAssignment,
			_ => Operator.Unknown
		};

		if (@operator == Operator.Unknown)
			return HandleTransformationFailure<Node>(operation, $"Compound assignment operator {operation.OperatorKind} is not supported");

		return new AssignmentExpression(@operator, left, right);
	}

	/// <summary>
	/// 处理空合并赋值操作
	/// C# 示例：
	/// name ??= "Default";     // 如果 name 为 null，则赋值 "Default"
	/// value ??= 0;            // 如果 value 为 null，则赋值 0
	/// 转换结果：name ??= "Default" / value ??= 0
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitCoalesceAssignment(ICoalesceAssignmentOperation operation, SenseArgument argument)
	{
		var left = Translate<Expression>(operation.Target, argument);
		var right = Translate<Expression>(operation.Value, argument);
		return new AssignmentExpression(Operator.NullishCoalescingAssignment, left, right);
	}

	/// <summary>
	/// 处理括号表达式操作
	/// C# 示例：
	/// (x + y)         // 括号表达式
	/// 转换结果：直接返回内部表达式（JavaScript 中括号由解析器处理）
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitParenthesized(IParenthesizedOperation operation, SenseArgument argument)
	{
		var exp = Translate<Expression>(operation.Operand, argument);
		return new SequenceExpression(NodeList.From(exp));
	}

	/// <summary>
	/// 处理 nameof 表达式操作
	/// C# 示例：
	/// nameof(variable)            // 获取变量名称
	/// nameof(MyClass.Property)    // 获取属性名称
	/// 转换结果："variable" / "Property"
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitNameOf(INameOfOperation operation, SenseArgument argument)
	{
		string? name = null;
		if (operation.Argument.ConstantValue.HasValue)
			name = operation.Argument.ConstantValue.Value?.ToString();

		else if (operation.ConstantValue.HasValue)
			name = operation.ConstantValue.Value?.ToString();

		if (string.IsNullOrEmpty(name) || name is null)
			return HandleTransformationFailure<Node>(operation.Argument, "NameOf expression could not be translated to JavaScript.");

		return new StringLiteral(name, $"\"{name}\"");
	}

	/// <summary>
	/// 处理默认值操作
	/// C# 示例：
	/// default(int)                        // 0
	/// default(string)                     // null
	/// default(bool)                       // false
	/// default(T)                          // 泛型类型的默认值
	/// 转换结果：0 / "" / false / null（根据类型）
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitDefaultValue(IDefaultValueOperation operation, SenseArgument argument)
	{
		var type = operation.Type;
		if (type is null)
			return new NullLiteral("null");

		if (type.TypeKind == TypeKind.Enum)
			return new NumericLiteral(0, "0");

		if (!type.IsValueType)
			return new NullLiteral("null");

		if (IsSystemHalfType(type))
			return new NumericLiteral(0, "0");

		return type.SpecialType switch
		{
			SpecialType.System_Boolean => new BooleanLiteral(false, "false"),
			SpecialType.System_Char => new StringLiteral("\0", "\"\\0\""),
			SpecialType.System_String => new NullLiteral("null"),
			SpecialType.System_SByte or
			SpecialType.System_Byte or
			SpecialType.System_Int16 or
			SpecialType.System_UInt16 or
			SpecialType.System_Int32 or
			SpecialType.System_UInt32 or
			SpecialType.System_Single or
			SpecialType.System_Double or
			SpecialType.System_Decimal => new NumericLiteral(0, "0"),
			SpecialType.System_Int64 or
			SpecialType.System_UInt64 => new BigIntLiteral(new System.Numerics.BigInteger(0), "0n"),
			_ => GetDefaultValueTypeExpression(type, argument)
				?? new NullLiteral("null")
		};
	}

	private Expression? GetDefaultValueTypeExpression(ITypeSymbol type, SenseArgument argument)
	{
		var (mapper, _) = GetMapperType(type);
		if (mapper == TypeMapper.Number)
			return new NumericLiteral(0, "0");

		if (mapper == TypeMapper.BigInt)
			return new BigIntLiteral(new System.Numerics.BigInteger(0), "0n");

		if (type is not INamedTypeSymbol namedType)
			return null;

		var ctor = namedType.InstanceConstructors.FirstOrDefault(static x => x.Parameters.Length == 0);
		if (ctor is null)
			return null;

		return GetWhiteListExpression(ctor, argument, [], out _);
	}

	/// <summary>
	/// 处理递增递减操作
	/// C# 示例：
	/// x++                                 // 后缀递增
	/// ++x                                 // 前缀递增
	/// x--                                 // 后缀递减
	/// --x                                 // 前缀递减
	/// 转换结果：x++ / ++x / x-- / --x
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation, SenseArgument argument)
	{
		var target = Translate<Expression>(operation.Target, argument);
		var @operator = operation.Kind == OperationKind.Increment
			? Operator.Increment
			: Operator.Decrement;
		var prefix = !operation.IsPostfix; // 前缀当IsPostfix为false时

		return new UpdateExpression(@operator, target, prefix: prefix);
	}

	/// <summary>
	/// 处理省略参数操作
	/// C# 示例：
	/// SomeMethod(arg1, , arg3);           // 省略中间参数
	/// Optional(a: 1, c: 3);               // 命名参数中省略了 b
	/// 转换结果：undefined
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitOmittedArgument(IOmittedArgumentOperation operation, SenseArgument argument)
	{
		// 省略的参数返回 undefined
		return new Identifier("undefined");
	}

	/// <summary>
	/// 处理参数操作
	/// C# 示例：
	/// Method(arg1, ref arg2, out arg3)    // 方法参数
	/// Constructor(param1, param2)         // 构造函数参数
	/// 转换结果：直接返回参数值表达式
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitArgument(IArgumentOperation operation, SenseArgument argument)
	{
		// 如果是 out 参数，传递 OutParameter 上下文
		if (operation.Parameter?.RefKind == RefKind.Out)
		{
			var outArg = argument.With(Sense.OutParameter);
			return Visit(operation.Value, outArg);
		}
		return Visit(operation.Value, argument);
	}

	/// <summary>
	/// 处理 with 表达式操作（记录类型的复制修改）
	/// C# 示例：
	/// var newPerson = person with { Name = "John" }; // 记录类型复制修改
	/// var updated = point with { X = 10 };          // 部分属性更新
	/// 转换结果：转换为JavaScript的对象展开语法
	/// { ...person, Name: "John" } / { ...point, X: 10 }
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitWith(IWithOperation operation, SenseArgument argument)
	{
		// with表达式的直接AST转换
		// C# 示例：person with { Name = "John" } 表示创建一个新对象，复制原对象并修改指定属性
		//         point with { X = 10 } 表示复制point对象并更新X属性
		// 转换结果：生成JavaScript的对象展开语法 { ...original, property: newValue }
		// 语义等价：C# record的with表达式在运行时语义上等同于JavaScript的对象展开

		// 获取原始对象
		var operand = Translate<Expression>(operation.Operand, argument);

		// 处理初始化器（要修改的属性）
		var properties = new List<Node>
		{
            // 添加展开元素（复制原始对象的所有属性）
            new SpreadElement(operand)
		};

		// 处理初始化器中的新属性
		// 获取初始化器中的所有初始化操作
		foreach (var initializer in operation.Initializer.Initializers)
		{
			if (initializer is IMemberInitializerOperation memberInit)
			{
				// 获取成员名称（支持特性别名）
				string memberName;
				ITypeSymbol? targetType;
				if (memberInit.InitializedMember is IFieldSymbol f)
				{
					memberName = Util.GetConfigOrSymbolName(f);
					targetType = f.Type;
				}
				else if (memberInit.InitializedMember is IPropertySymbol p)
				{
					memberName = Util.GetConfigOrSymbolName(p);
					targetType = p.Type;
				}
				else
					return HandleTransformationFailure<Node>(operation.Initializer, "With initializer could not be translated to JavaScript.");

				// 获取初始化值
				var initValue = TranslateTupleForTarget(memberInit.Initializer, targetType, argument);
				// 根据AST节点构造规范，使用PropertyDefinition创建对象属性
				// 确保生成正确的属性语法：{ ...original, propertyName: value }
				properties.Add(new ObjectProperty(
					kind: PropertyKind.Init,
					key: new Identifier(memberName),
					value: initValue,
					computed: false,
					shorthand: false,
					method: false
				));
			}
			else
			{
				// 对于其他类型的初始化器，需要确保生成正确的属性语法
				var initNode = Visit(initializer, argument);

				// 如果初始化器不是PropertyDefinition，需要包装成PropertyDefinition
				// 这样可以确保生成正确的JavaScript对象属性语法
				if (initNode is PropertyDefinition)
					properties.Add(initNode);
				else if (initNode is AssignmentExpression assignment)
				{
					// 如果是赋值表达式，提取左侧作为属性名，右侧作为值
					var key = assignment.Left switch
					{
						Identifier i => i,
						MemberExpression m => m.Property as Identifier,
						_ => null
					};

					if (key is not null)
					{
						properties.Add(new ObjectProperty(
							kind: PropertyKind.Init,
							key: key,
							value: assignment.Right,
							computed: false,
							shorthand: false,
							method: false
						));
					}
				}
				else
					return HandleTransformationFailure<Node>(operation, "With initializer could not be translated to JavaScript.");

			}
		}


		// 根据编译时优化原则，直接生成最简洁的JavaScript对象字面量
		// 返回对象表达式：{ ...original, property: value }
		return new ObjectExpression(NodeList.From<Node>(properties));
	}

	/// <summary>
	/// 处理特性操作
	/// C# 示例：
	/// [Obsolete("Use NewMethod instead")]  // 特性应用
	/// [JsonPropertyName("custom_name")]    // 序列化特性
	/// 转换结果：使用 JavaScript Decorator 实现
	/// @Obsolete("Use NewMethod instead")
	/// @JsonPropertyName("custom_name")
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node，如果特性未实现 IECMAScript 接口则返回 null</returns>
	/// <remarks>
	/// 特性参数限制：必须是编译时常量（基本类型、字符串、枚举、typeof、null、数组）
	/// 只处理实现了 IECMAScript 接口的特性，其他特性忽略
	/// 使用 operation.Operation 获取 IObjectCreationOperation，通过 IOperation 转换参数值
	/// 通过语法节点的 NameEquals 判断是否命名参数
	/// </remarks>
	public override Node? VisitAttribute(IAttributeOperation operation, SenseArgument argument)
	{
		// 只处理实现了 IECMAScript 接口的特性,IECMAScript是一个约定，所以写死名称
		if (operation.Operation is not IObjectCreationOperation creationOp)
			return null;

		if(creationOp.Type?.AllInterfaces.Any(i => i.Name == "IECMAScript") != true)
			return null;

		if (operation.Syntax is not AttributeSyntax attributeSyntax)
			return HandleTransformationFailure<Node>(operation, "Attribute syntax node is not available");

		// 获取特性名称（移除 Attribute 后缀）
		var attributeName = attributeSyntax.Name?.ToString();
		if (string.IsNullOrEmpty(attributeName))
			return HandleTransformationFailure<Node>(operation, "Cannot determine attribute name");

		if (attributeName!.EndsWith("Attribute"))
			attributeName = attributeName.Substring(0, attributeName.Length - 9);

		var positionalArgs = new List<Expression>();
		var namedProps = new List<ObjectProperty>();

		// 通过 IObjectCreationOperation 的 Arguments 获取参数
		var syntaxArgs = attributeSyntax.ArgumentList?.Arguments;
		if (syntaxArgs is not null)
		{
			for (int i = 0; i < creationOp.Arguments.Length && i < syntaxArgs.Value.Count; i++)
			{
				var arg = creationOp.Arguments[i];
				var syntaxArg = syntaxArgs.Value[i];

                if (Visit(arg.Value, argument) is not Expression valueExpr)
                    return HandleTransformationFailure<Node>(operation, "Failed to convert attribute argument");

                // 通过语法判断是否命名参数（NameEquals 表示 PropertyName = value）
                if (syntaxArg.NameEquals is not null)
				{
					var key = new Identifier(syntaxArg.NameEquals.Name.Identifier.Text);
					namedProps.Add(new ObjectProperty(
						kind: PropertyKind.Init,
						key: key,
						value: valueExpr,
						computed: false,
						shorthand: false,
						method: false
					));
				}
				else
				{
					positionalArgs.Add(valueExpr);
				}
			}
		}

		// 构建装饰器表达式
		Expression decorator = (positionalArgs.Count, namedProps.Count) switch
		{
			(0, 0) => new Identifier(attributeName),                                    // @Decorator
			(_, 0) => new CallExpression(new Identifier(attributeName), NodeList.From(positionalArgs), optional: false),  // @Decorator(args...)
			(0, _) => CreateDecoratorWithProps(attributeName, namedProps),              // @Decorator({ props })
			_ => CreateDecoratorWithArgsAndProps(attributeName, positionalArgs, namedProps)  // @Decorator(args..., { props })
		};

		return new Decorator(decorator);
	}

	private static CallExpression CreateDecoratorWithProps(string name, List<ObjectProperty> props)
	{
		var propsObject = new ObjectExpression(NodeList.From<Node>(props));
		return new CallExpression(new Identifier(name), NodeList.From<Expression>(propsObject), optional: false);
	}

	private static CallExpression CreateDecoratorWithArgsAndProps(string name, List<Expression> args, List<ObjectProperty> props)
	{
		var propsObject = new ObjectExpression(NodeList.From<Node>(props));
		var allArgs = new List<Expression>(args) { propsObject };
		return new CallExpression(new Identifier(name), NodeList.From(allArgs), optional: false);
	}

	/// <summary>
	/// 处理集合表达式操作
	/// C# 示例：
	/// int[] array = [1, 2, 3, 4, 5];      // 集合表达式语法
	/// List<string> list = ["a", "b", "c"]; // 集合表达式初始化
	/// 转换结果：[1, 2, 3, 4, 5] / ["a", "b", "c"]
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitCollectionExpression(ICollectionExpressionOperation operation, SenseArgument argument)
	{
		var elements = new List<Expression?>();
		var elementTargetType = GetCollectionElementTargetType(operation.Type);
		foreach (var element in operation.Elements)
		{
			elements.Add(TranslateTupleForTarget(element, elementTargetType, argument));
		}
		return new ArrayExpression(NodeList.From(elements));
	}

	/// <summary>
	/// 处理展开操作（扩展运算符）
	/// C# 示例：
	/// int[] combined = [..array1, ..array2]; // 数组展开
	/// Method(..args);                      // 参数展开
	/// 转换结果：...array1 / ...args（JavaScript 扩展运算符）
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitSpread(ISpreadOperation operation, SenseArgument argument)
	{
		var operand = Translate<Expression>(operation.Operand, argument);
		return new SpreadElement(operand);
	}
}
