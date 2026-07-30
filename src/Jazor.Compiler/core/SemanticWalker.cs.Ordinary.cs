using Acornima;
using Acornima.Ast;
using ECMAScript.Contract;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

/// <summary>
/// 处理常规语句、表达式和调用的 operation visitor 实现。
/// </summary>
/// <remarks>
/// 这是大多数 C# 语义的基础路径；需要协议模拟或结构擦除的特性应转交对应分片，
/// 不要在这里增加只针对某个宿主 API 的特殊分支，以免破坏宿主语义边界。
/// </remarks>
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
		var scopedArgument = EnsureScopeContext(operation, argument);
		var ctx = scopedArgument.ScopeContext is not null && ReferenceEquals(scopedArgument.ScopeContext.Anchor, operation)
			? scopedArgument
			: scopedArgument.EnterScope(operation, ScopeSite.NestedBlock());
		var pendingStatements = TranslateOperationsToStatements(operation.Operations, ctx);

		// 所有 stmt 处理完后，将 out/pattern 变量声明集中提升到块顶
		// C# 编译器保证同一作用域内不会有同名变量，提升是安全的
		var statements = MaterializeScopedStatements(ctx, pendingStatements);

		// 根据上下文判断返回不同类型的语句块
		if (scopedArgument.Sense == Sense.FunctionBody)
			return new FunctionBody(NodeList.From(statements), strict: true);

		if (scopedArgument.Sense == Sense.StaticBlock)
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
		{
			var bodyArg = EnsureScopeContext(operation, argument).EnterScope(operation.BlockBody, ScopeSite.FunctionBody()).With(Sense.FunctionBody);
			return Visit(operation.BlockBody, bodyArg);
		}

		// 如果有表达式体，转换为返回语句
		if (operation.ExpressionBody is not null)
		{
			var bodyArg = EnsureScopeContext(operation, argument, ScopeSite.FunctionBody()).With(Sense.FunctionBody);
			if (operation.ExpressionBody is IBlockOperation blockExpression)
				return Visit(blockExpression, bodyArg);

			var expr = Translate<Expression>(operation.ExpressionBody, bodyArg);
			var returnStmt = new ReturnStatement(expr);
			var statements = MaterializeScopedStatements(bodyArg, [returnStmt]);
			return new FunctionBody(NodeList.From(statements), strict: true);
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
		{
			var bodyArg = EnsureScopeContext(operation, argument).EnterScope(operation.BlockBody, ScopeSite.FunctionBody()).With(Sense.FunctionBody);
			return Visit(operation.BlockBody, bodyArg);
		}

		if (operation.ExpressionBody is not null)
		{
			var bodyArg = EnsureScopeContext(operation, argument, ScopeSite.FunctionBody()).With(Sense.FunctionBody);
			if (operation.ExpressionBody is IBlockOperation blockExpression)
				return Visit(blockExpression, bodyArg);

			var expr = Translate<Expression>(operation.ExpressionBody, bodyArg);
			var statement = new NonSpecialExpressionStatement(expr);
			var statements = MaterializeScopedStatements(bodyArg, [statement]);
			return new FunctionBody(NodeList.From(statements), strict: true);
		}

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
			var node = Visit(operation.Operation, argument);
			statement = node switch
			{
				Statement stmt => stmt,
				Expression expr => new NonSpecialExpressionStatement(expr),
				_ => HandleTransformationFailure<Statement>(operation.Operation, "Labeled statement target could not be translated to JavaScript.")
			};
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
			_ => HandleTransformationFailure<Node>(operation, $"Unsupported branch kind: {operation.BranchKind}.")
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
		if (operation.Kind == OperationKind.YieldReturn)
		{
			var value = Translate<Expression>(operation.ReturnedValue!, argument);
			return WithOrigin(new NonSpecialExpressionStatement(new YieldExpression(value, @delegate: false)), operation);
		}

		if (operation.Kind == OperationKind.YieldBreak)
			return WithOrigin(new ReturnStatement(null), operation);

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
		var node = Visit(operation.Operation, argument);
		return node switch
		{
			Statement statement => statement,
			SequenceExpression { Expressions.Count: 0 } sequenceExpression => sequenceExpression,
			Expression expression => WithOrigin(new NonSpecialExpressionStatement(expression), operation),
			_ => HandleTransformationFailure<Node>(operation, "Expression statement could not be translated to JavaScript.")
		};
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
		if (Host?.ShouldSkipLocalFunctionDeclaration(operation, argument) == true)
			return WithOriginIfMissing(new SequenceExpression(NodeList.Empty<Expression>()), operation);

		var id = new Identifier(operation.Symbol.Name);
		var parameters = new List<Node>();
		var refParameters = new List<Expression>();
		foreach (var param in operation.Symbol.Parameters)
		{
			var parameter = new Identifier(param.Name);
			parameters.Add(parameter);
			if (param.RefKind is RefKind.Out or RefKind.Ref)
				refParameters.Add(parameter);
		}

		// 函数边界：隔离 _declarators，共享 _specifiers（import 需跨函数边界传播）
		var bodyCtx = EnsureScopeContext(operation, argument).EnterScope(operation, ScopeSite.LocalFunctionBody());
		var pendingStatements = operation.Body is not null
			? TranslateOperationsToStatements(operation.Body.Operations, bodyCtx)
			: [];

		// 将函数体内的变量声明提升到函数体顶部
		var bodyStatements = MaterializeScopedStatements(bodyCtx, pendingStatements);

		var body = new FunctionBody(NodeList.From(bodyStatements), strict: true);
		if (refParameters.Count > 0)
			body = RefOutReturnProtocol.Apply(body, refParameters, !operation.Symbol.ReturnsVoid);

		// 检查函数是否为async或generator
		var isAsync = operation.Symbol.IsAsync;
		var isGenerator = ContainsYieldOperation(operation.Body);

		return new FunctionDeclaration(id,
			NodeList.From(parameters),
			body,
			generator: isGenerator,
			@async: isAsync);
	}

	private static bool ContainsAwaitOperation(IOperation? operation)
		=> operation is not null &&
		   (ContainsOperation(operation, static op =>
				op.Kind == OperationKind.Await ||
				op is IUsingOperation { IsAsynchronous: true } ||
				op is IUsingDeclarationOperation { IsAsynchronous: true }) ||
			ContainsAwaitSyntax(operation.Syntax));

	private static bool ContainsYieldOperation(IOperation? operation)
		=> ContainsOperation(operation, static op => op.Kind is OperationKind.YieldReturn or OperationKind.YieldBreak);

	private static bool ContainsOperation(IOperation? operation, Func<IOperation, bool> predicate)
	{
		if (operation is null)
			return false;

		if (predicate(operation))
			return true;

		foreach (var child in EnumerateContainedOperations(operation))
		{
			// 嵌套函数边界内的 yield 不应影响当前函数的 generator 语义。
			if (child is IAnonymousFunctionOperation or ILocalFunctionOperation)
				continue;

			if (ContainsOperation(child, predicate))
				return true;
		}

		return false;
	}

	private static IEnumerable<IOperation> EnumerateContainedOperations(IOperation operation)
	{
		foreach (var child in operation.ChildOperations)
		{
			if (child is not null)
				yield return child;
		}

		switch (operation)
		{
			case ISwitchExpressionOperation switchExpression:
				foreach (var arm in switchExpression.Arms)
					yield return arm;
				break;

			case ISwitchExpressionArmOperation arm:
				yield return arm.Pattern;
				if (arm.Guard is not null)
					yield return arm.Guard;
				yield return arm.Value;
				break;

			case ISwitchOperation switchOperation:
				if (switchOperation.Value is not null)
					yield return switchOperation.Value;
				foreach (var @case in switchOperation.Cases)
					yield return @case;
				break;

			case ISwitchCaseOperation switchCase:
				foreach (var clause in switchCase.Clauses)
					yield return clause;
				foreach (var body in switchCase.Body)
					yield return body;
				break;
		}
	}

	private static bool ContainsAwaitSyntax(SyntaxNode syntax)
	{
		if (syntax is AwaitExpressionSyntax)
			return true;

		return syntax.DescendantNodes(descendIntoChildren: static node =>
				node is not AnonymousFunctionExpressionSyntax
				and not AnonymousMethodExpressionSyntax
				and not SimpleLambdaExpressionSyntax
				and not ParenthesizedLambdaExpressionSyntax
				and not LocalFunctionStatementSyntax)
			.Any(static node => node is AwaitExpressionSyntax);
	}

	private Expression? BuildValueLiteral(ITypeSymbol? type, object? value)
	{
		// 处理 null 值（必须在类型检查之前，因为 null 没有特定的类型信息）
		if (value is null)
			return Null;

		// 类型信息缺失时报告错误
		if (type is null)
			return null;

		if (type.TypeKind == TypeKind.Enum &&
			type is INamedTypeSymbol enumType)
		{
			if (TryBuildStringEnumValueLiteral(enumType, value, out var stringEnumLiteral))
				return stringEnumLiteral;

			if (enumType.EnumUnderlyingType is not null)
				return BuildValueLiteral(enumType.EnumUnderlyingType, value);
		}

		var valueStr = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
		var (mapper, _) = GetMapperType(type);

		// 布尔值字面量：true / false
		if (mapper == TypeMapper.Boolean)
			return new BooleanLiteral((bool)value, ((bool)value).ToString().ToLowerInvariant());

		// 字符串和字符字面量
		else if (mapper == TypeMapper.String)
		{
			return CreateStringLiteral(valueStr ?? string.Empty);
		}

		//  数值字面量：42 / 3.14
		else if (mapper == TypeMapper.Number)
		{
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
	/// 2. C# as 转换必须保留运行时 try-cast 语义
	///    - 匹配目标运行时类型时返回原值
	///    - 不匹配时返回 null
	///    - 可能有副作用的操作数只求值一次
	/// 3. 其他类型转换在 JavaScript 中可以安全忽略，因为 JS 是动态类型语言
	///    - 装箱/拆箱: JS 无需区分值类型和引用类型
	///    - 引用类型转换: JS 运行时动态检查
	///
	/// C# 示例 → JavaScript 转换结果：
	///   (long)42           → BigInt(42)      // int 转 long，需要显式转换
	///   (long)x            → BigInt(x)       // 变量转 BigInt
	///   (int)someLong      → Number(someLong) // long 转 int，需要显式转换
	///   (ulong)count       → BigInt(count)   // uint 转 ulong
	///   (int)3.14          → 3.14            // float 转 int，忽略转换
	///   obj as string      → typeof obj === "string" ? obj : null
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
		if (Host?.RewriteConversionPreorder(operation, argument) is Expression preorderHostExpression)
			return WithOriginIfMissing(preorderHostExpression, operation);

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

		if (operation.IsTryCast)
			return TranslateTryCast(operation, argument, expr);

		if (operation.OperatorMethod is not null)
		{
			var mapped = GetWhiteListExpression(operation.OperatorMethod, argument, [expr], out _);
			if (mapped is not null)
				return mapped;

			if (!IsPassThroughCustomOperatorFallbackAllowed(operation.OperatorMethod) &&
				!CanPassThroughIntrinsicConversion(operation))
				return HandleTransformationFailure<Node>(
					operation,
					$"Conversion operator '{operation.OperatorMethod.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' requires an explicit whitelist/ECMAScript mapping and cannot fall back to raw JavaScript conversion.");
		}

		// 处理 Number 与 BigInt 之间的显式转换
		if (operation.Type is not null && operation.Operand.Type is not null)
		{
			var targetType = GetMapperType(operation.Type);
			var operandType = GetMapperType(operation.Operand.Type);
			var targetIsChar = operation.Type.SpecialType == SpecialType.System_Char;
			var operandIsChar = operation.Operand.Type.SpecialType == SpecialType.System_Char;

			if (operandIsChar && targetType.Mapper is TypeMapper.Number or TypeMapper.BigInt)
			{
				var codeUnit = new CallExpression(
					new MemberExpression(expr, new Identifier("charCodeAt"), computed: false, optional: false),
					NodeList.From<Expression>(new NumericLiteral(0, "0")),
					optional: false);
				return targetType.Mapper == TypeMapper.BigInt
					? new CallExpression(new Identifier("BigInt"), NodeList.From<Expression>(codeUnit), optional: false)
					: codeUnit;
			}

			if (targetIsChar && operandType.Mapper is TypeMapper.Number or TypeMapper.BigInt)
			{
				if (operation.IsChecked)
				{
					return HandleTransformationFailure<Node>(
						operation,
						"Checked numeric-to-char conversion is not supported because JavaScript String.fromCharCode applies unchecked UInt16 coercion.");
				}

				var codeUnit = operandType.Mapper == TypeMapper.BigInt
					? new CallExpression(new Identifier("Number"), NodeList.From(expr), optional: false)
					: expr;
				return new CallExpression(
					new MemberExpression(new Identifier("String"), new Identifier("fromCharCode"), computed: false, optional: false),
					NodeList.From(codeUnit),
					optional: false);
			}

			// Number → BigInt: (long)1 → BigInt(1)
			if (operandType.Mapper == TypeMapper.Number && targetType.Mapper == TypeMapper.BigInt)
				return new CallExpression(new Identifier("BigInt"), NodeList.From(expr), optional: false);

			// BigInt → Number: (int)someLong → Number(someLong)
			if (operandType.Mapper == TypeMapper.BigInt && targetType.Mapper == TypeMapper.Number)
				return new CallExpression(new Identifier("Number"), NodeList.From(expr), optional: false);
		}

		// 其他情况：直接返回操作数（JavaScript 是动态类型）
		// 包括：装箱/拆箱、引用类型强制转换等
		return expr;
	}

	private Expression TranslateTryCast(
		IConversionOperation operation,
		SenseArgument argument,
		Expression operand)
	{
		if (operation.Type is null)
			return HandleTransformationFailure<Expression>(operation, "Try-cast conversion is missing its target type.");

		// Roslyn has already proven implicit and identity conversions cannot fail.
		if (operation.Conversion.IsImplicit)
			return operand;

		var targetType = UnwrapNullableValueType(operation.Type);
		Expression input = operand;
		Expression? initialization = null;
		if (NeedsSingleEvaluationCaching(operand))
		{
			var tempId = new Identifier(AllocateUniqueName(operation, argument, LoweringSite.TryCastInput()));
			argument.AddVarDeclarator(new VariableDeclarator(tempId, null), _recursionDepth);
			initialization = new AssignmentExpression(Operator.Assignment, tempId, operand);
			input = tempId;
		}

		var match = CreateTypeMatchExpr(operation, targetType, input, context: argument);
		var result = new ConditionalExpression(match, input, Null);
		return initialization is null
			? result
			: new SequenceExpression(NodeList.From<Expression>(initialization, result));
	}

	private bool CanPassThroughIntrinsicConversion(IConversionOperation operation)
	{
		if (operation.Type is null || operation.Operand.Type is null)
			return false;

		if (IsSystemIndexType(operation.Type) ||
			IsSystemIndexType(operation.Operand.Type) ||
			IsSystemRangeType(operation.Type) ||
			IsSystemRangeType(operation.Operand.Type))
			return true;

		var targetType = GetMapperType(operation.Type);
		var operandType = GetMapperType(operation.Operand.Type);
		if (targetType.Mapper == operandType.Mapper)
			return targetType.Mapper is not TypeMapper.Class and not TypeMapper.Unknown;

		return (operandType.Mapper == TypeMapper.Number && targetType.Mapper == TypeMapper.BigInt) ||
			(operandType.Mapper == TypeMapper.BigInt && targetType.Mapper == TypeMapper.Number);
	}

	private static bool IsSystemIndexType(ITypeSymbol? type)
		=> type?.OriginalDefinition is INamedTypeSymbol namedType &&
			namedType.Name == "Index" &&
			namedType.ContainingNamespace?.ToDisplayString() == "System";

	private static bool IsSystemRangeType(ITypeSymbol? type)
		=> type?.OriginalDefinition is INamedTypeSymbol namedType &&
			namedType.Name == "Range" &&
			namedType.ContainingNamespace?.ToDisplayString() == "System";

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
		var operand = Translate<Expression>(operation.Operation, argument);

		if (TryBuildConditionalAccessNullishGuard(operation, argument, operand, out var guardedExpression) &&
			guardedExpression is not null)
			return guardedExpression;

		// 先转换 Operation，然后通过 PatternInput 传递给 WhenNotNull
		var whenNotNullArg = argument.WithPatternInput(operand);
		var whenNotNull = Translate<Expression>(operation.WhenNotNull, whenNotNullArg);
		if (whenNotNull is SequenceExpression)
			return whenNotNull;

		return new ChainExpression(whenNotNull);
	}

	private bool TryBuildConditionalAccessNullishGuard(
		IConditionalAccessOperation operation,
		SenseArgument argument,
		Expression operand,
		out Expression? expression)
	{
		expression = null;

		switch (operation.WhenNotNull)
		{
			case IPropertyReferenceOperation propertyReference when RequiresConditionalAccessNullishGuard(propertyReference):
				break;

			case IInvocationOperation invocation when invocation.Instance is IConditionalAccessInstanceOperation:
				break;

			case IImplicitIndexerReferenceOperation implicitIndexer when implicitIndexer.Instance is IConditionalAccessInstanceOperation:
				break;

			case IArrayElementReferenceOperation arrayElementReference when arrayElementReference.ArrayReference is IConditionalAccessInstanceOperation:
				break;

			default:
				return false;
		}

		var tempId = new Identifier(AllocateUniqueName(operation, argument, LoweringSite.ConditionalAccessInput()));
		argument.AddVarDeclarator(new VariableDeclarator(tempId, null), _recursionDepth);

		var whenNotNullArg = argument.WithPatternInput(tempId);
		var whenNotNull = Translate<Expression>(operation.WhenNotNull, whenNotNullArg);
		var assign = new AssignmentExpression(Operator.Assignment, tempId, operand);
		var isNullish = new NonLogicalBinaryExpression(Operator.Equality, tempId, Null);
		var guarded = new ConditionalExpression(isNullish, Undefined, whenNotNull);
		expression = new SequenceExpression(NodeList.From<Expression>(assign, guarded));
		return true;
	}

	private bool RequiresConditionalAccessNullishGuard(IPropertyReferenceOperation operation)
	{
		if (operation.Instance is not IConditionalAccessInstanceOperation)
			return false;

		var getter = operation.Property.GetMethod;
		if (getter is null)
			return false;

		if (TryGetWhiteListValue(_whiteListCompiles, getter, out _, out _))
			return true;

		if (!TryGetWhiteListValue(WhiteList.Members, getter, out _, out var entry))
			return false;

		return !(entry.Op == Op.Alias && operation.Arguments.Length == 0);
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
		// '^' needs the containing indexer/range to supply Length/Count. Reaching the
		// unary visitor directly means C# is materializing a standalone System.Index value.
		if (operation.OperatorKind == UnaryOperatorKind.Hat)
		{
			return HandleTransformationFailure<Node>(
				operation,
				"Standalone System.Index values are not supported in JavaScript conversion. Use '^' directly in an array, string, or supported indexer access.");
		}

		var operand = Translate<Expression>(operation.Operand, argument);
		if (operation.OperatorMethod is not null)
		{
			var mapped = GetWhiteListExpression(operation.OperatorMethod, argument, [operand], out _);
			if (mapped is not null)
				return mapped;

			if (!IsPassThroughCustomOperatorFallbackAllowed(operation.OperatorMethod))
				return HandleTransformationFailure<Node>(
					operation,
					$"Unary operator '{operation.OperatorMethod.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' requires an explicit whitelist/ECMAScript mapping and cannot fall back to raw JavaScript unary semantics.");
		}

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
			var mapped = GetWhiteListExpression(operation.OperatorMethod, argument, [left, right], out _, operation);
			if (mapped is not null)
				return mapped;

			if (!IsPassThroughCustomOperatorFallbackAllowed(operation.OperatorMethod))
				return HandleTransformationFailure<Node>(
					operation,
					$"Binary operator '{operation.OperatorMethod.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' requires an explicit whitelist/ECMAScript mapping and cannot fall back to raw JavaScript binary semantics.");
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
		var bodyCtx = EnsureScopeContext(operation, argument).EnterScope(operation, ScopeSite.LambdaBody());
		var pendingStatements = TranslateOperationsToStatements(operation.Body.Operations, bodyCtx);

		// 将函数体内的变量声明提升到函数体顶部
		var bodyStatements = MaterializeScopedStatements(bodyCtx, pendingStatements);

		var body = new FunctionBody(NodeList.From(bodyStatements), strict: true);

		// 创建箭头函数
		return new ArrowFunctionExpression(
			NodeList.From(parameters), body,
			@async: operation.Symbol.IsAsync,
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
		if (Host?.RewriteSimpleAssignmentPreorder(operation, argument) is Expression preorderHostExpression)
			return WithOriginIfMissing(preorderHostExpression, operation);

		if (operation.Target is IDiscardOperation)
		{
			// tuple 赋值不依赖 Roslyn 恰好插入 conversion。
			// 只要目标静态类型是另一套 tuple 视图，这里就按目标协议主动重映射。
			// 这样：
			//   target = source;
			// 不会因为 IOperation 树里缺少显式 conversion 而漏掉 tuple remap。
			var discardValue = TranslateTupleForTarget(operation.Value, operation.Target.Type, argument);
			return WithOriginIfMissing(discardValue, operation);
		}

		if (operation.Target is IFieldReferenceOperation importedFieldReference &&
			IsImportedModuleStaticFieldMutation(importedFieldReference, argument))
		{
			return HandleTransformationFailure<Node>(
				operation,
				$"Cross-module static field mutation '{importedFieldReference.Field.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' is not supported because ECMAScript imported bindings are read-only. Expose a property setter or helper method on the module host instead.");
		}

		// tuple 赋值不依赖 Roslyn 恰好插入 conversion。
		// 只要目标静态类型是另一套 tuple 视图，这里就按目标协议主动重映射。
		// 这样：
		//   target = source;
		// 不会因为 IOperation 树里缺少显式 conversion 而漏掉 tuple remap。
		var value = TranslateTupleForTarget(operation.Value, operation.Target.Type, argument);

		if (operation.Target is IPropertyReferenceOperation autoPropertyReference &&
			TryBuildCurrentModuleAutoPropertyBackingFieldAssignment(autoPropertyReference, value, out var backingFieldAssignment))
		{
			return WithOriginIfMissing(backingFieldAssignment, operation);
		}

		if (operation.Target is IPropertyReferenceOperation propertyReference &&
			propertyReference.Property.SetMethod is not null)
		{
			var instance = Translate<Expression>(propertyReference.Instance, argument, null);
			var propertyArguments = new List<Expression>(propertyReference.Arguments.Length);
			foreach (var propertyArgument in propertyReference.Arguments)
			{
				var argContext = propertyArgument.Parameter?.RefKind is RefKind.Out
					? argument.With(Sense.OutParameter)
					: argument;
				propertyArguments.Add(Translate<Expression>(propertyArgument.Value, argContext));
			}

			return WithOriginIfMissing(
				BuildPropertySetterAssignment(propertyReference, argument, instance, propertyArguments, value),
				operation);
		}

		if (operation.Target is IImplicitIndexerReferenceOperation implicitIndexer)
		{
			PrepareImplicitIndexerSetterAccess(
				implicitIndexer,
				operation,
				argument,
				cacheForRepeatedReadWrite: false,
				out var implicitInitializations,
				out var indexerInstance,
				out var indexerArguments,
				out var indexerProperty,
				out _);

			var assignment = BuildImplicitIndexerSetterAssignment(
				implicitIndexer,
				argument,
				indexerProperty,
				indexerInstance,
				indexerArguments,
				value);
			if (implicitInitializations.Count == 0)
				return WithOrigin(assignment, operation);

			var implicitExpressions = new List<Expression>(implicitInitializations.Count + 1);
			implicitExpressions.AddRange(implicitInitializations);
			implicitExpressions.Add(assignment);
			return WithOrigin(new SequenceExpression(NodeList.From(implicitExpressions)), operation);
		}

		if (operation.Target is IArrayElementReferenceOperation arrayReference)
		{
			var arrayInitializations = new List<Expression>();
			var arrayTarget = BuildArrayElementMutationTarget(arrayReference, argument, arrayInitializations);
			var assignment = new AssignmentExpression(Operator.Assignment, arrayTarget, value);
			if (arrayInitializations.Count == 0)
				return WithOrigin(assignment, operation);

			var arrayExpressions = new List<Expression>(arrayInitializations.Count + 1);
			arrayExpressions.AddRange(arrayInitializations);
			arrayExpressions.Add(assignment);
			return WithOrigin(new SequenceExpression(NodeList.From(arrayExpressions)), operation);
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
		if (operation.Target is IFieldReferenceOperation importedFieldReference &&
			IsImportedModuleStaticFieldMutation(importedFieldReference, argument))
		{
			return HandleTransformationFailure<Node>(
				operation,
				$"Cross-module static field mutation '{importedFieldReference.Field.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' is not supported because ECMAScript imported bindings are read-only. Expose a property setter or helper method on the module host instead.");
		}

		if (operation.Target is IPropertyReferenceOperation propertyReference &&
			TryPreparePropertyMutationAccess(propertyReference, operation, argument, out var initializations, out var readExpression, out var propertyInstance, out var propertyArguments))
		{
			var rhsExpression = Translate<Expression>(operation.Value, argument);
			if (!TryGetCompoundAssignmentOperators(operation.OperatorKind, out _, out _))
				return HandleTransformationFailure<Node>(operation, $"Compound assignment operator {operation.OperatorKind} is not supported");

			var currentId = CreatePropertyMutationTemp(operation, argument, "current");
			var expressions = new List<Expression>(initializations.Count + 3);
			expressions.AddRange(initializations);
			expressions.Add(new AssignmentExpression(
				Operator.Assignment,
				currentId,
				BuildCompoundAssignmentValue(operation, argument, readExpression, rhsExpression)));
			expressions.Add(BuildPropertySetterAssignment(propertyReference, argument, propertyInstance, propertyArguments, currentId));
			expressions.Add(currentId);
			return WithOrigin(new SequenceExpression(NodeList.From(expressions)), operation);
		}

		if (operation.Target is IImplicitIndexerReferenceOperation implicitIndexer)
		{
			PrepareImplicitIndexerMutationAccess(
				implicitIndexer,
				operation,
				argument,
				out var implicitInitializations,
				out var implicitReadExpression,
				out var indexerInstance,
				out var indexerArguments,
				out var indexerProperty);

			var rhsExpression = Translate<Expression>(operation.Value, argument);
			if (!TryGetCompoundAssignmentOperators(operation.OperatorKind, out _, out _))
				return HandleTransformationFailure<Node>(operation, $"Compound assignment operator {operation.OperatorKind} is not supported");

			var currentId = CreatePropertyMutationTemp(operation, argument, "current");
			var implicitExpressions = new List<Expression>(implicitInitializations.Count + 3);
			implicitExpressions.AddRange(implicitInitializations);
			implicitExpressions.Add(new AssignmentExpression(
				Operator.Assignment,
				currentId,
				BuildCompoundAssignmentValue(operation, argument, implicitReadExpression, rhsExpression)));
			implicitExpressions.Add(BuildImplicitIndexerSetterAssignment(
				implicitIndexer,
				argument,
				indexerProperty,
				indexerInstance,
				indexerArguments,
				currentId));
			implicitExpressions.Add(currentId);
			return WithOrigin(new SequenceExpression(NodeList.From(implicitExpressions)), operation);
		}

		List<Expression>? targetInitializations = null;
		Expression left;
		if (operation.Target is IArrayElementReferenceOperation arrayReference)
		{
			targetInitializations = [];
			left = BuildArrayElementMutationTarget(
				arrayReference,
				argument,
				targetInitializations,
				cacheForRepeatedReadWrite: operation.OperatorMethod is not null);
		}
		else
		{
			left = Translate<Expression>(operation.Target, argument);
		}

		var right = Translate<Expression>(operation.Value, argument);
		Expression WrapTarget(Expression expression)
		{
			if (targetInitializations is null || targetInitializations.Count == 0)
				return expression;

			var expressions = new List<Expression>(targetInitializations.Count + 1);
			expressions.AddRange(targetInitializations);
			expressions.Add(expression);
			return new SequenceExpression(NodeList.From(expressions));
		}

		if (operation.OperatorMethod is not null)
		{
			var mapped = GetWhiteListExpression(operation.OperatorMethod, argument, [left, right], out _, operation);
			if (mapped is not null)
			{
				if (!CanDuplicateReadWriteTarget(left))
					return HandleTransformationFailure<Node>(
						operation,
						"Compound assignment with a custom operator requires a stable assignable target. Use a local/field/property target without side-effecting receiver/index expressions.");

				return WithOrigin(WrapTarget(new AssignmentExpression(Operator.Assignment, left, mapped)), operation);
			}

			if (!IsPassThroughCustomOperatorFallbackAllowed(operation.OperatorMethod))
				return HandleTransformationFailure<Node>(
					operation,
					$"Compound assignment operator '{operation.OperatorMethod.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' requires an explicit whitelist mapping and cannot fall back to raw JavaScript compound semantics.");
		}

		if (!TryGetCompoundAssignmentOperators(operation.OperatorKind, out var @operator, out _))
			return HandleTransformationFailure<Node>(operation, $"Compound assignment operator {operation.OperatorKind} is not supported");

		return WithOrigin(WrapTarget(new AssignmentExpression(@operator, left, right)), operation);
	}

	private static bool TryGetCompoundAssignmentOperators(BinaryOperatorKind operatorKind, out Operator assignmentOperator, out Operator binaryOperator)
	{
		switch (operatorKind)
		{
			case BinaryOperatorKind.Add:
				assignmentOperator = Operator.AdditionAssignment;
				binaryOperator = Operator.Addition;
				return true;
			case BinaryOperatorKind.Subtract:
				assignmentOperator = Operator.SubtractionAssignment;
				binaryOperator = Operator.Subtraction;
				return true;
			case BinaryOperatorKind.Multiply:
				assignmentOperator = Operator.MultiplicationAssignment;
				binaryOperator = Operator.Multiplication;
				return true;
			case BinaryOperatorKind.Divide:
				assignmentOperator = Operator.DivisionAssignment;
				binaryOperator = Operator.Division;
				return true;
			case BinaryOperatorKind.Remainder:
				assignmentOperator = Operator.RemainderAssignment;
				binaryOperator = Operator.Remainder;
				return true;
			case BinaryOperatorKind.And:
				assignmentOperator = Operator.BitwiseAndAssignment;
				binaryOperator = Operator.BitwiseAnd;
				return true;
			case BinaryOperatorKind.Or:
				assignmentOperator = Operator.BitwiseOrAssignment;
				binaryOperator = Operator.BitwiseOr;
				return true;
			case BinaryOperatorKind.ExclusiveOr:
				assignmentOperator = Operator.BitwiseXorAssignment;
				binaryOperator = Operator.BitwiseXor;
				return true;
			case BinaryOperatorKind.LeftShift:
				assignmentOperator = Operator.LeftShiftAssignment;
				binaryOperator = Operator.LeftShift;
				return true;
			case BinaryOperatorKind.RightShift:
				assignmentOperator = Operator.RightShiftAssignment;
				binaryOperator = Operator.RightShift;
				return true;
			case BinaryOperatorKind.UnsignedRightShift:
				assignmentOperator = Operator.UnsignedRightShiftAssignment;
				binaryOperator = Operator.UnsignedRightShift;
				return true;
			default:
				assignmentOperator = Operator.Unknown;
				binaryOperator = Operator.Unknown;
				return false;
		}
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
		if (operation.Target is IFieldReferenceOperation importedFieldReference &&
			IsImportedModuleStaticFieldMutation(importedFieldReference, argument))
		{
			return HandleTransformationFailure<Node>(
				operation,
				$"Cross-module static field mutation '{importedFieldReference.Field.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' is not supported because ECMAScript imported bindings are read-only. Expose a property setter or helper method on the module host instead.");
		}

		if (operation.Target is IPropertyReferenceOperation propertyReference &&
			TryPreparePropertyMutationAccess(propertyReference, operation, argument, out var initializations, out var readExpression, out var propertyInstance, out var propertyArguments))
		{
			var rhsExpression = Translate<Expression>(operation.Value, argument);
			var currentId = CreatePropertyMutationTemp(operation, argument, "current");
			var assignIfNull = new SequenceExpression(NodeList.From<Expression>(
				new AssignmentExpression(Operator.Assignment, currentId, rhsExpression),
				BuildPropertySetterAssignment(propertyReference, argument, propertyInstance, propertyArguments, currentId),
				currentId));

			var expressions = new List<Expression>(initializations.Count + 2);
			expressions.AddRange(initializations);
			expressions.Add(new AssignmentExpression(Operator.Assignment, currentId, readExpression));
			expressions.Add(new ConditionalExpression(
				new NonLogicalBinaryExpression(Operator.Equality, currentId, Null),
				assignIfNull,
				currentId));

			return WithOrigin(new SequenceExpression(NodeList.From(expressions)), operation);
		}

		if (operation.Target is IImplicitIndexerReferenceOperation implicitIndexer)
		{
			PrepareImplicitIndexerMutationAccess(
				implicitIndexer,
				operation,
				argument,
				out var implicitInitializations,
				out var implicitReadExpression,
				out var indexerInstance,
				out var indexerArguments,
				out var indexerProperty);

			var rhsExpression = Translate<Expression>(operation.Value, argument);
			var currentId = CreatePropertyMutationTemp(operation, argument, "current");
			var assignIfNull = new SequenceExpression(NodeList.From<Expression>(
				new AssignmentExpression(Operator.Assignment, currentId, rhsExpression),
				BuildImplicitIndexerSetterAssignment(
					implicitIndexer,
					argument,
					indexerProperty,
					indexerInstance,
					indexerArguments,
					currentId),
				currentId));

			var implicitExpressions = new List<Expression>(implicitInitializations.Count + 2);
			implicitExpressions.AddRange(implicitInitializations);
			implicitExpressions.Add(new AssignmentExpression(Operator.Assignment, currentId, implicitReadExpression));
			implicitExpressions.Add(new ConditionalExpression(
				new NonLogicalBinaryExpression(Operator.Equality, currentId, Null),
				assignIfNull,
				currentId));

			return WithOrigin(new SequenceExpression(NodeList.From(implicitExpressions)), operation);
		}

		List<Expression>? targetInitializations = null;
		Expression left;
		if (operation.Target is IArrayElementReferenceOperation arrayReference)
		{
			targetInitializations = [];
			left = BuildArrayElementMutationTarget(arrayReference, argument, targetInitializations);
		}
		else
		{
			left = Translate<Expression>(operation.Target, argument);
		}

		var right = Translate<Expression>(operation.Value, argument);
		if (targetInitializations is null || targetInitializations.Count == 0)
			return WithOrigin(new AssignmentExpression(Operator.NullishCoalescingAssignment, left, right), operation);

		var wrappedExpressions = new List<Expression>(targetInitializations.Count + 1);
		wrappedExpressions.AddRange(targetInitializations);
		wrappedExpressions.Add(new AssignmentExpression(Operator.NullishCoalescingAssignment, left, right));
		return WithOrigin(new SequenceExpression(NodeList.From(wrappedExpressions)), operation);
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
		return new ParenthesizedExpression(exp);
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

		return CreateStringLiteral(name);
	}

	/// <summary>
	/// 处理 typeof 运算符操作。
	/// 当前支持的是稳定运行时类型令牌，而不是完整 CLR System.Type 反射对象。
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">当前 lowering 上下文</param>
	/// <returns>JavaScript 运行时类型令牌表达式</returns>
	public override Node? VisitTypeOf(ITypeOfOperation operation, SenseArgument argument)
	{
		var typeOperand = operation.TypeOperand;
		if (typeOperand is null)
			return HandleTransformationFailure<Node>(operation, "typeof operation must have a target type.");

		var typeToken = BuildRuntimeTypeTokenExpression(operation, typeOperand, argument);
		return WithOriginIfMissing(typeToken, operation);
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
			return WithOrigin(Null, operation);

		var expression = BuildDefaultValueExpression(operation, type, argument);
		return WithOrigin(expression, operation);
	}

	private Expression BuildDefaultValueExpression(IOperation operation, ITypeSymbol type, SenseArgument argument)
	{
		RejectUnsupportedDefaultValueTypeIfNeeded(operation, type);

		if (type is ITypeParameterSymbol typeParameter)
		{
			if (typeParameter.HasReferenceTypeConstraint)
				return Null;

			return HandleTransformationFailure<Expression>(
				operation,
				$"default({typeParameter.Name}) is not supported because the runtime type parameter may be a value type and Jazor cannot synthesize CLR default semantics safely.");
		}

		if (type.TypeKind == TypeKind.Enum)
		{
			if (type is INamedTypeSymbol enumType)
			{
				if (Util.IsStringEnumType(enumType))
				{
					var zeroValue = enumType.EnumUnderlyingType is not null
						? CreateEnumUnderlyingZeroValue(enumType.EnumUnderlyingType)
						: 0;
					if (TryBuildStringEnumValueLiteral(enumType, zeroValue, out var stringEnumDefault))
						return stringEnumDefault;

					return HandleTransformationFailure<Expression>(
						operation,
						$"default({enumType.ToDisplayString(Format.NameFormat)}) is not supported because string enums require a declared zero-valued member mapping.");
				}

				if (enumType.EnumUnderlyingType is not null)
				{
					var zeroValue = CreateEnumUnderlyingZeroValue(enumType.EnumUnderlyingType);
					return BuildValueLiteral(enumType.EnumUnderlyingType, zeroValue) ?? new NumericLiteral(0, "0");
				}
			}

			return new NumericLiteral(0, "0");
		}

		if (!type.IsValueType)
			return Null;

		if (type is INamedTypeSymbol tupleType && tupleType.IsTupleType)
			return BuildTupleDefaultValueExpression(operation, tupleType, argument);

		if (type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
			return Null;

		if (IsSystemHalfType(type))
			return new NumericLiteral(0, "0");

		return type.SpecialType switch
		{
			SpecialType.System_Boolean => new BooleanLiteral(false, "false"),
			SpecialType.System_Char => CreateStringLiteral("\0"),
			SpecialType.System_String => Null,
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
			_ => GetDefaultValueTypeExpression(operation, type, argument)
		};
	}

	private static object CreateEnumUnderlyingZeroValue(ITypeSymbol underlyingType)
	{
		return underlyingType.SpecialType switch
		{
			SpecialType.System_SByte => (sbyte)0,
			SpecialType.System_Byte => (byte)0,
			SpecialType.System_Int16 => (short)0,
			SpecialType.System_UInt16 => (ushort)0,
			SpecialType.System_Int32 => 0,
			SpecialType.System_UInt32 => 0u,
			SpecialType.System_Int64 => 0L,
			SpecialType.System_UInt64 => 0UL,
			_ => 0
		};
	}

	private static StringLiteral CreateStringLiteral(string value)
		=> JavaScriptAstFactory.CreateStringLiteral(value);

	private void RejectUnsupportedDefaultValueTypeIfNeeded(IOperation operation, ITypeSymbol type)
	{
		if (type is ITypeParameterSymbol or IArrayTypeSymbol)
			return;

		var (mapper, _) = GetMapperType(type);
		if (mapper is TypeMapper.Number or TypeMapper.BigInt or TypeMapper.Boolean or TypeMapper.String)
			return;

		if (type.IsTupleType ||
			type.IsAnonymousType ||
			type.TypeKind is TypeKind.Interface or TypeKind.Delegate)
			return;

		if (!type.IsValueType && type.IsAbstract)
			return;

		RejectUnsupportedTypeFallback(operation, type, "default value");
	}

	private Expression GetDefaultValueTypeExpression(IOperation operation, ITypeSymbol type, SenseArgument argument)
	{
		var (mapper, _) = GetMapperType(type);
		if (mapper == TypeMapper.Number)
			return new NumericLiteral(0, "0");

		if (mapper == TypeMapper.BigInt)
			return new BigIntLiteral(new System.Numerics.BigInteger(0), "0n");

		if (TryBuildKnownDefaultConstructorExpression(type, argument, out var knownDefault) &&
			knownDefault is not null)
			return knownDefault;

		return HandleTransformationFailure<Expression>(
			operation,
			$"Value type '{type.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' does not have a safe JavaScript lowering for default(...). Only intrinsic value types, tuples, nullable values, and known CLR runtime shims can be emitted without changing CLR semantics.");
	}

	private static bool IsKnownDefaultConstructorType(ITypeSymbol type)
	{
		var displayName = type.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat);
		return displayName is
			"System.DateOnly" or
			"System.DateTime" or
			"System.DateTimeOffset" or
			"System.TimeOnly" or
			"System.TimeSpan" or
			"System.Guid";
	}

	private bool TryBuildKnownDefaultConstructorExpression(ITypeSymbol type, SenseArgument argument, out Expression? expression)
	{
		expression = null;
		if (!IsKnownDefaultConstructorType(type) ||
			type is not INamedTypeSymbol namedType)
			return false;

		var ctor = namedType.InstanceConstructors.FirstOrDefault(static x => x.Parameters.Length == 0);
		if (ctor is null)
			return false;

		expression = GetWhiteListExpression(ctor, argument, [], out _);
		return expression is not null;
	}

	private Expression BuildTupleDefaultValueExpression(IOperation operation, INamedTypeSymbol tupleType, SenseArgument argument)
	{
		var nodes = new List<Node>(tupleType.TupleElements.Length);
		for (var index = 0; index < tupleType.TupleElements.Length; index++)
		{
			var element = tupleType.TupleElements[index];
			nodes.Add(new ObjectProperty(
				PropertyKind.Init,
				key: CreateObjectPropertyKey(Util.GetConfigOrSymbolName(element)),
				value: BuildDefaultValueExpression(operation, element.Type, argument),
				computed: false,
				shorthand: false,
				method: false));
		}

		return new ObjectExpression(NodeList.From(nodes));
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
		if (operation.Target is IFieldReferenceOperation importedFieldReference &&
			IsImportedModuleStaticFieldMutation(importedFieldReference, argument))
		{
			return HandleTransformationFailure<Node>(
				operation,
				$"Cross-module static field mutation '{importedFieldReference.Field.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' is not supported because ECMAScript imported bindings are read-only. Expose a property setter or helper method on the module host instead.");
		}

		if (operation.Target is IPropertyReferenceOperation propertyReference &&
			TryPreparePropertyMutationAccess(propertyReference, operation, argument, out var initializations, out var readExpression, out var propertyInstance, out var propertyArguments))
		{
			var currentId = CreatePropertyMutationTemp(operation, argument, "current");
			var expressions = new List<Expression>(initializations.Count + 3);
			expressions.AddRange(initializations);

			if (operation.IsPostfix)
			{
				expressions.Add(new AssignmentExpression(Operator.Assignment, currentId, readExpression));
				expressions.Add(BuildPropertySetterAssignment(
					propertyReference,
					argument,
					propertyInstance,
					propertyArguments,
					BuildIncrementOrDecrementValue(operation, argument, currentId)));
				expressions.Add(currentId);
			}
			else
			{
				expressions.Add(new AssignmentExpression(
					Operator.Assignment,
					currentId,
					BuildIncrementOrDecrementValue(operation, argument, readExpression)));
				expressions.Add(BuildPropertySetterAssignment(propertyReference, argument, propertyInstance, propertyArguments, currentId));
				expressions.Add(currentId);
			}

			return WithOrigin(new SequenceExpression(NodeList.From(expressions)), operation);
		}

		if (operation.Target is IImplicitIndexerReferenceOperation implicitIndexer)
		{
			PrepareImplicitIndexerMutationAccess(
				implicitIndexer,
				operation,
				argument,
				out var implicitInitializations,
				out var implicitReadExpression,
				out var indexerInstance,
				out var indexerArguments,
				out var indexerProperty);

			var currentId = CreatePropertyMutationTemp(operation, argument, "current");
			var implicitExpressions = new List<Expression>(implicitInitializations.Count + 3);
			implicitExpressions.AddRange(implicitInitializations);

			if (operation.IsPostfix)
			{
				implicitExpressions.Add(new AssignmentExpression(Operator.Assignment, currentId, implicitReadExpression));
				implicitExpressions.Add(BuildImplicitIndexerSetterAssignment(
					implicitIndexer,
					argument,
					indexerProperty,
					indexerInstance,
					indexerArguments,
					BuildIncrementOrDecrementValue(operation, argument, currentId)));
				implicitExpressions.Add(currentId);
			}
			else
			{
				implicitExpressions.Add(new AssignmentExpression(
					Operator.Assignment,
					currentId,
					BuildIncrementOrDecrementValue(operation, argument, implicitReadExpression)));
				implicitExpressions.Add(BuildImplicitIndexerSetterAssignment(
					implicitIndexer,
					argument,
					indexerProperty,
					indexerInstance,
					indexerArguments,
					currentId));
				implicitExpressions.Add(currentId);
			}

			return WithOrigin(new SequenceExpression(NodeList.From(implicitExpressions)), operation);
		}

		List<Expression>? targetInitializations = null;
		Expression preparedTarget;
		if (operation.Target is IArrayElementReferenceOperation arrayReference)
		{
			targetInitializations = [];
			preparedTarget = BuildArrayElementMutationTarget(
				arrayReference,
				argument,
				targetInitializations,
				cacheForRepeatedReadWrite: operation.OperatorMethod is not null);
		}
		else
		{
			preparedTarget = Translate<Expression>(operation.Target, argument);
		}

		Expression WrapTarget(Expression expression)
		{
			if (targetInitializations is null || targetInitializations.Count == 0)
				return expression;

			var expressions = new List<Expression>(targetInitializations.Count + 1);
			expressions.AddRange(targetInitializations);
			expressions.Add(expression);
			return new SequenceExpression(NodeList.From(expressions));
		}

		if (operation.OperatorMethod is not null)
		{
			var assignmentTarget = preparedTarget;
			var mapped = GetWhiteListExpression(operation.OperatorMethod, argument, [assignmentTarget], out _);
			if (mapped is not null)
			{
				if (!CanDuplicateReadWriteTarget(assignmentTarget))
					return HandleTransformationFailure<Node>(
						operation,
						"Increment/decrement with a custom operator requires a stable assignable target. Use a local/field/property target without side-effecting receiver/index expressions.");

				var currentId = CreatePropertyMutationTemp(operation, argument, "current");
				var expressions = new List<Expression>(3);
				if (operation.IsPostfix)
				{
					expressions.Add(new AssignmentExpression(Operator.Assignment, currentId, assignmentTarget));
					expressions.Add(new AssignmentExpression(Operator.Assignment, assignmentTarget, GetWhiteListExpression(operation.OperatorMethod, argument, [currentId], out _) ?? mapped));
					expressions.Add(currentId);
				}
				else
				{
					expressions.Add(new AssignmentExpression(Operator.Assignment, currentId, GetWhiteListExpression(operation.OperatorMethod, argument, [assignmentTarget], out _) ?? mapped));
					expressions.Add(new AssignmentExpression(Operator.Assignment, assignmentTarget, currentId));
					expressions.Add(currentId);
				}

				return WithOrigin(WrapTarget(new SequenceExpression(NodeList.From(expressions))), operation);
			}

			if (!IsPassThroughCustomOperatorFallbackAllowed(operation.OperatorMethod) &&
				!CanPassThroughIntrinsicIncrementOrDecrement(operation))
				return HandleTransformationFailure<Node>(
					operation,
					$"Increment/decrement operator '{operation.OperatorMethod.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' requires an explicit whitelist mapping and cannot fall back to raw JavaScript update semantics.");
		}

		var @operator = operation.Kind == OperationKind.Increment
			? Operator.Increment
			: Operator.Decrement;
		var prefix = !operation.IsPostfix; // 前缀当IsPostfix为false时

		return WithOrigin(WrapTarget(new UpdateExpression(@operator, preparedTarget, prefix: prefix)), operation);
	}

	private Expression BuildCompoundAssignmentValue(
		ICompoundAssignmentOperation operation,
		SenseArgument argument,
		Expression left,
		Expression right)
	{
		if (operation.OperatorMethod is not null)
		{
			var mapped = GetWhiteListExpression(operation.OperatorMethod, argument, [left, right], out _);
			if (mapped is not null)
				return mapped;

			if (!IsPassThroughCustomOperatorFallbackAllowed(operation.OperatorMethod))
				return HandleTransformationFailure<Expression>(
					operation,
					$"Compound assignment operator '{operation.OperatorMethod.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' requires an explicit whitelist/ECMAScript mapping and cannot fall back to raw JavaScript compound semantics.");
		}

		if (!TryGetCompoundAssignmentOperators(operation.OperatorKind, out _, out var binaryOperator))
			return HandleTransformationFailure<Expression>(operation, $"Compound assignment operator {operation.OperatorKind} is not supported");

		return new NonLogicalBinaryExpression(binaryOperator, left, right);
	}

	private Expression BuildIncrementOrDecrementValue(
		IIncrementOrDecrementOperation operation,
		SenseArgument argument,
		Expression operand)
	{
		if (operation.OperatorMethod is not null)
		{
			var mapped = GetWhiteListExpression(operation.OperatorMethod, argument, [operand], out _);
			if (mapped is not null)
				return mapped;

			if (!IsPassThroughCustomOperatorFallbackAllowed(operation.OperatorMethod) &&
				!CanPassThroughIntrinsicIncrementOrDecrement(operation))
				return HandleTransformationFailure<Expression>(
					operation,
					$"Increment/decrement operator '{operation.OperatorMethod.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' requires an explicit whitelist/ECMAScript mapping and cannot fall back to raw JavaScript update semantics.");
		}

		var targetType = operation.Target.Type ?? operation.Type;
		Expression one = targetType is not null && GetMapperType(targetType).Mapper == TypeMapper.BigInt
			? new BigIntLiteral(System.Numerics.BigInteger.One, "1n")
			: new NumericLiteral(1, "1");
		return new NonLogicalBinaryExpression(
			operation.Kind == OperationKind.Increment ? Operator.Addition : Operator.Subtraction,
			operand,
			one);
	}

	private static bool CanPassThroughIntrinsicIncrementOrDecrement(IIncrementOrDecrementOperation operation)
	{
		var targetType = operation.Target.Type ?? operation.Type;
		return targetType is not null &&
			GetMapperType(targetType).Mapper is TypeMapper.Number or TypeMapper.BigInt;
	}

	private bool TryPreparePropertyMutationAccess(
		IPropertyReferenceOperation propertyReference,
		IOperation ownerOperation,
		SenseArgument argument,
		out List<Expression> initializations,
		out Expression readExpression,
		out Expression? instance,
		out List<Expression> arguments)
	{
		initializations = [];
		readExpression = null!;
		instance = null;
		arguments = [];

		if (!RequiresPropertyMutationBridge(propertyReference) ||
			propertyReference.Property.GetMethod is null)
			return false;

		instance = MaterializePropertyMutationOperand(
			Translate<Expression>(propertyReference.Instance, argument, null),
			ownerOperation,
			argument,
			initializations,
			"instance");

		for (var i = 0; i < propertyReference.Arguments.Length; i++)
		{
			var propertyArgument = propertyReference.Arguments[i];
			var argContext = propertyArgument.Parameter?.RefKind is RefKind.Out
				? argument.With(Sense.OutParameter)
				: argument;
			var rawArgument = Translate<Expression>(propertyArgument.Value, argContext);
			arguments.Add(MaterializePropertyMutationOperand(rawArgument, ownerOperation, argument, initializations, $"arg{i}"));
		}

		var getterExpression = GetWhiteListExpression(propertyReference.Property.GetMethod, argument, arguments, instance, out _, propertyReference);
		if (getterExpression is null)
			return false;

		readExpression = getterExpression;
		return true;
	}

	private bool RequiresPropertyMutationBridge(IPropertyReferenceOperation propertyReference)
	{
		if (propertyReference.Property.SetMethod is null ||
			propertyReference.Property.GetMethod is null)
			return false;

		if (TryGetWhiteListValue(_whiteListCompiles, propertyReference.Property.GetMethod, out _, out _))
			return true;

		if (!TryGetWhiteListValue(WhiteList.Members, propertyReference.Property.GetMethod, out _, out var entry))
			return false;

		if (entry.Op != Op.Alias)
			return true;

		return propertyReference.Arguments.Length > 0;
	}

	private Expression MaterializePropertyMutationOperand(
		Expression? expression,
		IOperation ownerOperation,
		SenseArgument argument,
		List<Expression> initializations,
		string slot)
	{
		if (expression is null)
			return null!;

		if (!NeedsSingleEvaluationCaching(expression))
			return expression;

		var tempId = CreatePropertyMutationTemp(ownerOperation, argument, slot);
		initializations.Add(new AssignmentExpression(Operator.Assignment, tempId, expression));
		return tempId;
	}

	private static bool NeedsSingleEvaluationCaching(Expression expression)
		=> expression is not Identifier
			and not Literal
			and not ThisExpression
			and not Super;

	private static bool CanDuplicateReadWriteTarget(Expression expression)
		=> expression switch
		{
			Identifier or ThisExpression or Super => true,
			MemberExpression member when !member.Optional &&
				CanDuplicateReadWriteTarget((Expression)member.Object) &&
				((!member.Computed && member.Property is Identifier) ||
				 (member.Computed && member.Property is Identifier or Literal)) => true,
			_ => false
		};

	private bool IsImportedModuleStaticFieldMutation(IFieldReferenceOperation fieldReference, SenseArgument argument)
	{
		if (!fieldReference.Field.IsStatic ||
			fieldReference.Instance is not null ||
			fieldReference.Field.ContainingType is null)
			return false;

		var fieldName = Util.GetConfigOrSymbolName(fieldReference.Field);
		return TryBuildImportedModuleMember(fieldReference.Field.ContainingType, fieldName, argument, out var importedMember) &&
			importedMember is not null;
	}

	private Identifier CreatePropertyMutationTemp(IOperation ownerOperation, SenseArgument argument, string slot)
	{
		var tempId = new Identifier(AllocateUniqueName(ownerOperation, argument, LoweringSite.PropertyMutationTemp(slot)));
		argument.AddVarDeclarator(new VariableDeclarator(tempId, null), _recursionDepth);
		return tempId;
	}

	private Expression BuildPropertySetterAssignment(
		IPropertyReferenceOperation propertyReference,
		SenseArgument argument,
		Expression? instance,
		List<Expression> arguments,
		Expression value)
	{
		RejectUnsupportedNativeMapSetEqualityBoundaryIfNeeded(
			propertyReference,
			propertyReference.Instance?.Type ?? propertyReference.Property.ContainingType,
			"property assignment");

		if (propertyReference.Property.SetMethod is not null)
		{
			var setterArguments = new List<Expression>(arguments.Count + 1);
			setterArguments.AddRange(arguments);
			setterArguments.Add(value);

			var mapperExpr = GetWhiteListExpression(propertyReference.Property.SetMethod, argument, setterArguments, instance, out var setterAlias, propertyReference);
			if (mapperExpr is not null)
				return mapperExpr;

			if (TryBuildCurrentModulePropertySetterCall(propertyReference.Property, value, out var currentModuleSetterCall) &&
				currentModuleSetterCall is not null)
				return currentModuleSetterCall;

			if (TryBuildImportedModulePropertySetterCall(propertyReference.Property, argument, value, out var importedSetterCall) &&
				importedSetterCall is not null)
				return importedSetterCall;

			if (string.IsNullOrEmpty(setterAlias))
				RejectUnsupportedRuntimeFallback(propertyReference, propertyReference.Property.SetMethod, "property assignment", propertyReference.Instance?.Type ?? propertyReference.Property.ContainingType);
		}

		var target = BuildPropertyWriteTarget(propertyReference, argument, instance, arguments);
		return new AssignmentExpression(Operator.Assignment, target, value);
	}

	private Expression BuildPropertyWriteTarget(
		IPropertyReferenceOperation propertyReference,
		SenseArgument argument,
		Expression? instance,
		List<Expression> arguments)
	{
		if (instance is not null &&
			arguments.Count > 0 &&
			(propertyReference.Property.IsIndexer || propertyReference.Property.Parameters.Length > 0))
		{
			if (arguments.Count != 1)
				return HandleTransformationFailure<Expression>(propertyReference, "JavaScript fallback for indexers only supports a single translated index argument.");

			return new MemberExpression(instance, arguments[0], computed: true, optional: false);
		}

		var propertyName = GetInitializerMemberName(propertyReference.Property);
		var property = new Identifier(propertyName!);
		if (instance is not null)
			return new MemberExpression(instance, property, computed: false, optional: false);

		if (propertyReference.Property.IsStatic && propertyReference.Property.ContainingType is not null)
		{
			if (TryBuildPreferredRuntimeStaticMemberAccess(propertyReference.Property, propertyReference.Syntax, propertyReference.SemanticModel, propertyName!, out var preferredStaticProperty) &&
				preferredStaticProperty is not null)
				return preferredStaticProperty;

			var containing = BuildFullTypeName(propertyReference.Property.ContainingType, argument);
			if (containing is not null)
				return new MemberExpression(containing, property, computed: false, optional: false);
		}

		return property;
	}

	private bool TryBuildCurrentModuleAutoPropertyBackingFieldAssignment(
		IPropertyReferenceOperation propertyReference,
		Expression value,
		out Expression assignment)
	{
		assignment = null!;

		if (propertyReference.Property.IsStatic ||
			propertyReference.Property.IsIndexer ||
			propertyReference.Property.Parameters.Length > 0)
			return false;

		if (propertyReference.Property.SetMethod is { IsInitOnly: false })
			return false;

		if (!TryGetCurrentModuleDeclaredName(propertyReference.Property.ContainingType, out _))
			return false;

		var backingField = propertyReference.Property.ContainingType
			.GetMembers($"<{propertyReference.Property.Name}>k__BackingField")
			.OfType<IFieldSymbol>()
			.FirstOrDefault(field =>
				field.IsImplicitlyDeclared &&
				SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol?.OriginalDefinition, propertyReference.Property.OriginalDefinition));

		if (backingField is null)
			return false;

		if (propertyReference.Instance is not IInstanceReferenceOperation instanceReference ||
			instanceReference.ReferenceKind != InstanceReferenceKind.ContainingTypeInstance)
			return false;

		var backingFieldName = Format.HashName(propertyReference.Property.OriginalDefinition.ToDisplayString(Format.NameFormat));
		assignment = new AssignmentExpression(
			Operator.Assignment,
			new MemberExpression(
				new ThisExpression(),
				new PrivateIdentifier(backingFieldName),
				computed: false,
				optional: false),
			value);
		return true;
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
		var operand = Translate<Expression>(operation.Operand, argument);
		var properties = new List<Node>
		{
            new SpreadElement(operand)
		};

		foreach (var initializer in operation.Initializer.Initializers)
		{
			if (initializer is not ISimpleAssignmentOperation assignment)
			{
				return HandleTransformationFailure<Node>(
					operation.Initializer,
					"With initializer must bind to a property or field assignment.");
			}

			var member = GetObjectInitializerMemberSymbol(assignment);
			var targetType = member switch
			{
				IPropertySymbol property => property.Type,
				IFieldSymbol field => field.Type,
				_ => null
			};
			if (member is null || targetType is null)
				return HandleTransformationFailure<Node>(operation.Initializer, "With initializer target could not be resolved.");

			var memberName = ResolveInitializerAssignmentMemberName(
				assignment,
				member,
				"with-expression member assignment",
				member.ContainingType);
			var value = TranslateTupleForTarget(assignment.Value, targetType, argument);
			properties.Add(new ObjectProperty(
				kind: PropertyKind.Init,
				key: CreateObjectPropertyKey(memberName),
				value: value,
				computed: false,
				shorthand: false,
				method: false));
		}

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
	/// 构造参数和命名初始化器都直接从绑定的 IObjectCreationOperation 转换
	/// </remarks>
	public override Node? VisitAttribute(IAttributeOperation operation, SenseArgument argument)
	{
		if (operation.Operation is not IObjectCreationOperation creationOp)
			return null;

		if (creationOp.Type?.AllInterfaces.Any(static interfaceType =>
			string.Equals(interfaceType.ToDisplayString(Format.NameFormat), "ECMAScript.IECMAScript", StringComparison.Ordinal)) != true)
			return null;

		var attributeName = Util.GetConfigOrSymbolName(creationOp.Type);
		if (attributeName.EndsWith("Attribute", StringComparison.Ordinal))
			attributeName = attributeName.Substring(0, attributeName.Length - 9);

		var positionalArgs = new List<Expression>();
		var namedProps = new List<ObjectProperty>();
		foreach (var constructorArgument in creationOp.Arguments)
		{
			if (Translate<Expression>(constructorArgument.Value, argument) is not { } value)
				return HandleTransformationFailure<Node>(operation, "Attribute constructor argument could not be translated to JavaScript.");

			positionalArgs.Add(value);
		}

		if (creationOp.Initializer is not null)
		{
			foreach (var initializer in creationOp.Initializer.Initializers)
			{
				if (initializer is not ISimpleAssignmentOperation assignment ||
					GetObjectInitializerMemberSymbol(assignment) is not { } member)
				{
					return HandleTransformationFailure<Node>(
						operation,
						"Attribute named argument must bind to a property or field assignment.");
				}

				var memberType = member switch
				{
					IPropertySymbol property => property.Type,
					IFieldSymbol field => field.Type,
					_ => null
				};
				if (memberType is null)
					return HandleTransformationFailure<Node>(operation, "Attribute named argument target type could not be resolved.");

				var memberName = GetCurrentModuleDeclaredOrConfigName(member);
				var memberValue = TranslateTupleForTarget(assignment.Value, memberType, argument);
				namedProps.Add(new ObjectProperty(
					kind: PropertyKind.Init,
					key: CreateObjectPropertyKey(memberName),
					value: memberValue,
					computed: false,
					shorthand: false,
					method: false));
			}
		}

		Expression decorator = (positionalArgs.Count, namedProps.Count) switch
		{
			(0, 0) => new Identifier(attributeName),
			(_, 0) => new CallExpression(new Identifier(attributeName), NodeList.From(positionalArgs), optional: false),
			(0, _) => CreateDecoratorWithProps(attributeName, namedProps),
			_ => CreateDecoratorWithArgsAndProps(attributeName, positionalArgs, namedProps)
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
		var elementTargetType = GetCollectionElementTargetType(operation.Type);
		if (operation.Type is not null)
		{
			if (!IsCollectionExpressionCarrierType(operation.Type))
			{
				RejectUnsupportedTypeFallback(operation, operation.Type, "collection expression");
			}
		}

		var elements = new List<Expression?>();
		foreach (var element in operation.Elements)
		{
			elements.Add(TranslateTupleForTarget(element, elementTargetType, argument));
		}
		return new ArrayExpression(NodeList.From(elements));
	}

	private static bool IsCollectionExpressionCarrierType(ITypeSymbol? typeSymbol)
	{
		if (typeSymbol is not INamedTypeSymbol namedType)
			return false;

		var displayName = namedType.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat);
		return displayName is "System.ReadOnlySpan<T>" or "System.Span<T>";
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
