using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

namespace ECMAScript.Compiler;

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
	public override Acornima.Ast.Node? VisitBlock(IBlockOperation operation, Context argument)
	{
		var statements = new List<Statement>();
		foreach (var stmt in operation.Operations)
		{
			var node = Visit(stmt, argument);
			if (node is Statement statement)
				statements.Add(statement);
			else if (node is Expression expr)
				statements.Add(new NonSpecialExpressionStatement(expr));
			else
				HandleTransformationFailure(stmt, $"{stmt.Kind} could not be translated to JavaScript.");
		}

		// 根据上下文判断返回不同类型的语句块
		// 如果父节点是方法或函数，返回 FunctionBody
		if (operation.Parent is IMethodBodyOperation ||
			operation.Parent is ILocalFunctionOperation ||
			operation.Parent is IAnonymousFunctionOperation ||
			operation.Parent is IConstructorBodyOperation)
		{
			return new FunctionBody(NodeList.From(statements), strict: true);
		}

		// 如果父节点是类型或类定义的静态初始化块，返回 StaticBlock
		if (operation.Parent is IFieldInitializerOperation &&
			operation.Parent is IFieldReferenceOperation fieldRef &&
			fieldRef.Field.IsStatic)
		{
			return new StaticBlock(NodeList.From(statements));
		}

		// 默认情况返回 NestedBlockStatement
		return new NestedBlockStatement(NodeList.From(statements));
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
	public override Acornima.Ast.Node? VisitLabeled(ILabeledOperation operation, Context argument)
	{
		var label = new Identifier(operation.Label.Name);

		Statement statement;
		if (operation.Operation is null)
			statement = new EmptyStatement();
		else
			statement = Translate<Statement>(operation.Operation, argument);

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
	public override Acornima.Ast.Node? VisitBranch(IBranchOperation operation, Context argument)
	{
		var label = new Identifier(operation.Target.Name);

		return operation.BranchKind switch
		{
			BranchKind.Break => new BreakStatement(label),
			BranchKind.Continue => new ContinueStatement(label),
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
	public override Acornima.Ast.Node? VisitEmpty(IEmptyOperation operation, Context argument)
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
	public override Acornima.Ast.Node? VisitReturn(IReturnOperation operation, Context argument)
	{
		if (operation.ReturnedValue is null)
			return new ReturnStatement(null);

		var exp = Translate<Expression>(operation.ReturnedValue, argument);
		return new ReturnStatement(exp);
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
	public override Acornima.Ast.Node? VisitExpressionStatement(IExpressionStatementOperation operation, Context argument)
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
	public override Acornima.Ast.Node? VisitLocalFunction(ILocalFunctionOperation operation, Context argument)
	{
		var id = new Identifier(operation.Symbol.Name);
		var parameters = new List<Node>();
		foreach (var param in operation.Symbol.Parameters)
			parameters.Add(new Identifier(param.Name));

		var bodyStatements = new List<Statement>();
		if (operation.Body is not null)
		{
			foreach (var stmt in operation.Body.Operations)
			{
				var node = Visit(stmt, argument);
				if (node is Statement statement)
					bodyStatements.Add(statement);
				else if (node is Expression expr)
					bodyStatements.Add(new NonSpecialExpressionStatement(expr));
				else
					HandleTransformationFailure(stmt, "Local function statement could not be translated to JavaScript.");
			}
		}

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

	/// <summary>
	/// 处理字面量操作
	/// C# 示例：
	/// 42              // 整数字面量
	/// "Hello"         // 字符串字面量
	/// true            // 布尔字面量
	/// 'A'             // 字符字面量
	/// null            // 空字面量
	/// 转换结果：42 / "Hello" / true / "A" / null
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitLiteral(ILiteralOperation operation, Context argument)
	{
		if (operation.ConstantValue.Value is null)
			return new NullLiteral("null");

		var value = operation.ConstantValue.Value;
		var raw = value.ToString() ?? "null";

		return operation.Type?.SpecialType switch
		{
			SpecialType.System_Boolean => new BooleanLiteral((bool)value, raw.ToLower()),
			SpecialType.System_String => new StringLiteral((string)value, $"'{raw}'"),
			SpecialType.System_Char => new StringLiteral(value.ToString() ?? "", $"'{raw}'"),
			SpecialType.System_SByte or
			SpecialType.System_Byte or
			SpecialType.System_Int16 or
			SpecialType.System_UInt16 or
			SpecialType.System_Int32 or
			SpecialType.System_UInt32 or
			SpecialType.System_Int64 or
			SpecialType.System_UInt64 or
			SpecialType.System_Single or
			SpecialType.System_Double or
			SpecialType.System_Decimal => new NumericLiteral(System.Convert.ToDouble(value), raw),
			_ => new NullLiteral("null")
		};
	}

	/// <summary>
	/// 处理类型转换操作
	/// C# 示例：
	/// (int)3.14       // 显式类型转换
	/// obj as Type     // 安全类型转换
	/// 转换结果：直接返回操作数（JavaScript 是动态类型）
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitConversion(IConversionOperation operation, Context argument)
	{
		return Visit(operation.Operand, argument);
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
	public override Acornima.Ast.Node? VisitInvocation(IInvocationOperation operation, Context argument)
	{
		var arguments = new List<Expression>();

		foreach (var arg in operation.Arguments)
		{
			Translate(arguments, arg.Value, argument);
		}

		// 判断方法调用的类型
		Expression callee;

		if (operation.Instance is null)
		{
			// 静态方法调用或扩展方法调用
			if (operation.TargetMethod.IsStatic == true)
			{
				// 静态方法调用：StaticClass.Method()
				var className = operation.TargetMethod.ContainingType.Name;
				var methodName = operation.TargetMethod.Name;
				callee = new MemberExpression(
					new Identifier(className),
					new Identifier(methodName),
					computed: false,
					optional: false
				);
			}
			else
			{
				// 扩展方法调用：ExtensionMethod(arg)
				var methodName = operation.TargetMethod.Name;
				callee = new Identifier(methodName);
			}
		}
		else
		{
			// 实例方法调用：obj.Method()
			var instance = Translate<Expression>(operation.Instance, argument);
			var methodName = operation.TargetMethod.Name;

			callee = new MemberExpression(
				instance,
				new Identifier(methodName),
				computed: false,
				optional: false
			);
		}

		return new CallExpression(callee, NodeList.From(arguments), optional: false);
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
	public override Acornima.Ast.Node? VisitConditionalAccess(IConditionalAccessOperation operation, Context argument)
	{
		// 不需要处理 Operation，会在 WhenNotNull中递归回来处理
		//var operand = VisitTo<Expression>(operation.Operation, argument);
		var whenNotNull = Translate<Expression>(operation.WhenNotNull, argument);
		return new ChainExpression(whenNotNull);
	}

	/// <summary>
	/// IConditionalAccessInstanceOperation 是一个轻量级的、无子操作的、作为语义占位符的叶子节点。
	/// 它被专门设计用于在 IOperation 树中，作为空条件访问表达式（?.）右侧成员操作的 Instance。
	/// 它的唯一目的是提供类型信息，从而将运行时的短路求值逻辑（由 IConditionalAccessOperation 控制）与编译时的静态语义分析（由成员操作自身完成）完美解耦。
	/// C# 示例：
	/// obj?.Property中的obj?
	/// 转换方式：递归向上找到IConditionalAccessOperation，提取真实的 Operation
	/// 转换结果：obj?
	/// </summary>
	public override Acornima.Ast.Node? VisitConditionalAccessInstance(IConditionalAccessInstanceOperation operation, Context argument)
	{
		var parent = operation.Parent;
		while (parent is not null)
		{
			if (parent is IConditionalAccessOperation access)
			{
				return Translate<Expression>(access.Operation, argument);
			}
			parent = parent.Parent;
		}

		return HandleTransformationFailure(operation, "Could not find parent ConditionalAccessOperation.");
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
	public override Acornima.Ast.Node? VisitUnaryOperator(IUnaryOperation operation, Context argument)
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

		return HandleTransformationFailure(operation.Operand, "Unary operator operand could not be translated to JavaScript.");
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
	/// 转换结果：相同的 JavaScript 运算符
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitBinaryOperator(IBinaryOperation operation, Context argument)
	{
		var left = Translate<Expression>(operation.LeftOperand, argument);
		var right = Translate<Expression>(operation.RightOperand, argument);
		var @operator = operation.OperatorKind switch
		{
			BinaryOperatorKind.Add => Operator.Addition,
			BinaryOperatorKind.Subtract => Operator.Subtraction,
			BinaryOperatorKind.Multiply => Operator.Multiplication,
			BinaryOperatorKind.Divide => Operator.Division,
			BinaryOperatorKind.Remainder => Operator.Remainder,
			BinaryOperatorKind.Equals => Operator.Equality,
			BinaryOperatorKind.NotEquals => Operator.Inequality,
			BinaryOperatorKind.LessThan => Operator.LessThan,
			BinaryOperatorKind.GreaterThan => Operator.GreaterThan,
			BinaryOperatorKind.LessThanOrEqual => Operator.LessThanOrEqual,
			BinaryOperatorKind.GreaterThanOrEqual => Operator.GreaterThanOrEqual,
			BinaryOperatorKind.ConditionalAnd => Operator.LogicalAnd,
			BinaryOperatorKind.ConditionalOr => Operator.LogicalOr,
			_ => Operator.Unknown
		};

		// 逻辑运算符 → LogicalExpression
		if (@operator is Operator.LogicalAnd or Operator.LogicalOr)
			return new LogicalExpression(@operator, left, right);

		else if (@operator == Operator.Unknown)
			return HandleTransformationFailure(operation, "Binary operator could not be translated to JavaScript.");

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
	public override Acornima.Ast.Node? VisitConditional(IConditionalOperation operation, Context argument)
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

		return HandleTransformationFailure(operation, "Conditional operator could not be translated to JavaScript.");
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
	public override Acornima.Ast.Node? VisitCoalesce(ICoalesceOperation operation, Context argument)
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
	public override Acornima.Ast.Node? VisitAnonymousFunction(IAnonymousFunctionOperation operation, Context argument)
	{
		var parameters = new List<Node>();
		foreach (var param in operation.Symbol.Parameters)
		{
			var paramName = param.Name;
			parameters.Add(new Identifier(paramName));
		}

		var statements = new List<Statement>();
		foreach (var stmt in operation.Body.Operations)
		{
			var node = Visit(stmt, argument);
			if (node is Statement statement)
				statements.Add(statement);
			else if (node is Expression expr)
				statements.Add(new NonSpecialExpressionStatement(expr));
			else
				return HandleTransformationFailure(stmt, "Anonymous function body statement could not be translated to JavaScript.");
		}

		var body = new FunctionBody(NodeList.From(statements), strict: true);

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
	public override Acornima.Ast.Node? VisitAwait(IAwaitOperation operation, Context argument)
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
	public override Acornima.Ast.Node? VisitSimpleAssignment(ISimpleAssignmentOperation operation, Context argument)
	{
		var value = Translate<Expression>(operation.Value, (null, AstType.Expression, argument.Vars));
		var target = Translate<Expression>(operation.Target, (null, AstType.Expression, argument.Vars));
		if (argument.Out == AstType.ObjectProperty)
		{
			return new ObjectProperty(
				PropertyKind.Init,
				key: target,
				value: value,
				computed: false,
				shorthand: false,
				method: false
			);
		}
				
		var left = target;
		if (argument.Left is not null)
		{
			left = new MemberExpression(
				argument.Left,
				target,
				computed: false,
				optional: false
			);
		}

		return new AssignmentExpression(Operator.Assignment, left, value);
	}

	/// <summary>
	/// 处理复合赋值操作
	/// C# 示例：
	/// x += 5          // 加法赋值
	/// x -= 3          // 减法赋值
	/// x *= 2          // 乘法赋值
	/// x /= 4          // 除法赋值
	/// x %= 7          // 取模赋值
	/// 转换结果：x += 5 / x -= 3 / x *= 2 / x /= 4 / x %= 7
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitCompoundAssignment(ICompoundAssignmentOperation operation, Context argument)
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
			_ => Operator.Unknown
		};

		if (@operator == Operator.Unknown)
			return HandleTransformationFailure(operation, $"Compound assignment operator {operation.OperatorKind} is not supported");

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
	public override Acornima.Ast.Node? VisitCoalesceAssignment(ICoalesceAssignmentOperation operation, Context argument)
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
	public override Acornima.Ast.Node? VisitParenthesized(IParenthesizedOperation operation, Context argument)
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
	public override Acornima.Ast.Node? VisitNameOf(INameOfOperation operation, Context argument)
	{
		string? name = null;
		if (operation.Argument.ConstantValue.HasValue)
			name = operation.Argument.ConstantValue.Value?.ToString();

		else if (operation.ConstantValue.HasValue)
			name = operation.ConstantValue.Value?.ToString();

		if (string.IsNullOrEmpty(name))
			return HandleTransformationFailure(operation.Argument, "NameOf expression could not be translated to JavaScript.");

		return new StringLiteral(name, $"'{name}'");
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
	public override Acornima.Ast.Node? VisitDefaultValue(IDefaultValueOperation operation, Context argument)
	{
		// default(T) 转换为适当的默认值
		return operation.Type?.SpecialType switch
		{
			SpecialType.System_Boolean => new BooleanLiteral(false, "false"),
			SpecialType.System_String => new StringLiteral("", "''"),
			SpecialType.System_SByte or
			SpecialType.System_Byte or
			SpecialType.System_Int16 or
			SpecialType.System_UInt16 or
			SpecialType.System_Int32 or
			SpecialType.System_UInt32 or
			SpecialType.System_Int64 or
			SpecialType.System_UInt64 or
			SpecialType.System_Single or
			SpecialType.System_Double or
			SpecialType.System_Decimal => new NumericLiteral(0, "0"),
			_ => new NullLiteral("null")
		};
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
	public override Acornima.Ast.Node? VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation, Context argument)
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
	public override Acornima.Ast.Node? VisitOmittedArgument(IOmittedArgumentOperation operation, Context argument)
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
	public override Acornima.Ast.Node? VisitArgument(IArgumentOperation operation, Context argument)
	{
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
	public override Acornima.Ast.Node? VisitWith(IWithOperation operation, Context argument)
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
				// 获取成员名称
				string memberName;
				if (memberInit.InitializedMember is IFieldSymbol f)
					memberName = f.Name;
				else if (memberInit.InitializedMember is IPropertySymbol p)
					memberName = p.Name;
				else
					return HandleTransformationFailure(operation.Initializer, "With initializer could not be translated to JavaScript.");

				// 获取初始化值
				var initValue = Translate<Expression>(memberInit.Initializer, argument);
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
					return HandleTransformationFailure(operation, "With initializer could not be translated to JavaScript.");

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
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitAttribute(IAttributeOperation operation, Context argument)
	{
		// 通过语法节点获取特性信息
		if (operation.Syntax is not AttributeSyntax attributeSyntax)
			return HandleTransformationFailure(operation, "Attribute syntax node is not available");

		// 获取特性名称
		var attributeName = attributeSyntax.Name?.ToString();
		if (string.IsNullOrEmpty(attributeName))
			return HandleTransformationFailure(operation, "Cannot determine attribute name");

		// 移除常见的 C# 特性后缀
		if (attributeName.EndsWith("Attribute"))
			attributeName = attributeName.Substring(0, attributeName.Length - 9);

		// 处理特性参数
		var arguments = new List<Expression>();
		if (attributeSyntax.ArgumentList?.Arguments is not null)
		{
			foreach (var arg in attributeSyntax.ArgumentList.Arguments)
			{
				if (arg.Expression is not null)
				{
					var expr = ConvertFromSyntaxNode(arg.Expression);
					if (expr is Expression convertedExpr)
						arguments.Add(convertedExpr);
					else
						return HandleTransformationFailure(operation, "Failed to convert attribute argument");
				}
			}
		}

		// 处理命名参数
		var properties = new List<Node>();
		if (attributeSyntax.ArgumentList?.Arguments is not null)
		{
			foreach (var arg in attributeSyntax.ArgumentList.Arguments)
			{
				if (arg.NameEquals is not null)
				{
					// 命名参数：PropertyName = value
					var key = new Identifier(arg.NameEquals.Name.Identifier.Text);
					if (arg.Expression is not null)
					{
						var value = ConvertFromSyntaxNode(arg.Expression);
						if (value is Expression valueExpr)
						{
							properties.Add(new ObjectProperty(
								kind: PropertyKind.Init,
								key: key,
								value: valueExpr,
								computed: false,
								shorthand: false,
								method: false
							));
						}
						else
							return HandleTransformationFailure(operation, "Failed to convert named argument value");
					}
				}
			}
		}

		// 创建装饰器表达式
		Expression decorator;
		if (arguments.Count == 0 && properties.Count == 0)
		{
			// 无参数装饰器：@Decorator
			decorator = new Identifier(attributeName);
		}
		else if (arguments.Count > 0 && properties.Count == 0)
		{
			// 只有位置参数：@Decorator(args...)
			decorator = new CallExpression(
				new Identifier(attributeName),
				NodeList.From(arguments),
				optional: false
			);
		}
		else
		{
			// 有命名参数，使用对象字面量：@Decorator({ ...props })
			var propsObject = new ObjectExpression(NodeList.From(properties));
			if (arguments.Count > 0)
			{
				// 既有位置参数又有命名参数：@Decorator(args..., { ...props })
				var allArgs = new List<Expression>(arguments)
				{
					propsObject
				};
				decorator = new CallExpression(
					new Identifier(attributeName),
					NodeList.From(allArgs),
					optional: false
				);
			}
			else
			{
				// 只有命名参数：@Decorator({ ...props })
				decorator = new CallExpression(
					new Identifier(attributeName),
					NodeList.From<Expression>(propsObject),
					optional: false
				);
			}
		}

		// 返回装饰器节点
		return new Decorator(decorator);
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
	public override Acornima.Ast.Node? VisitCollectionExpression(ICollectionExpressionOperation operation, Context argument)
	{
		var elements = new List<Expression?>();
		foreach (var element in operation.Elements)
		{
			Translate(elements, element, argument, null);
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
	public override Acornima.Ast.Node? VisitSpread(ISpreadOperation operation, Context argument)
	{
		var operand = Translate<Expression>(operation.Operand, argument);
		return new SpreadElement(operand);
	}
}
