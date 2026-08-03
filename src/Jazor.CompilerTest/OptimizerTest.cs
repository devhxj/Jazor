using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jazor.ComplierTest;

/// <summary>
/// Optimizer 单元测试
/// 测试逻辑表达式优化器的去重和简化功能
/// 命名规范：[方法名]_[测试场景]_[期望行为]
/// </summary>
[TestClass]
public sealed class OptimizerTest
{
    #region 基本去重测试

    /// <summary>
    /// 测试简单重复: A && A -> A
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_SimpleAndDuplicate_ReturnsSingleOperand()
    {
        // Arrange
        var a = new Identifier("a");
        var expr = new LogicalExpression(Operator.LogicalAnd, a, a);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert
        Assert.IsInstanceOfType<Identifier>(result);
        Assert.AreEqual("a", ((Identifier)result).Name);
    }

    /// <summary>
    /// 测试简单重复: A || A -> A
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_SimpleOrDuplicate_ReturnsSingleOperand()
    {
        // Arrange
        var a = new Identifier("a");
        var expr = new LogicalExpression(Operator.LogicalOr, a, a);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert
        Assert.IsInstanceOfType<Identifier>(result);
        Assert.AreEqual("a", ((Identifier)result).Name);
    }

    /// <summary>
    /// 测试无重复: A && B 保持不变
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_NoDuplicate_ReturnsSameStructure()
    {
        // Arrange
        var a = new Identifier("a");
        var b = new Identifier("b");
        var expr = new LogicalExpression(Operator.LogicalAnd, a, b);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var logical = (LogicalExpression)result;
        Assert.AreEqual(Operator.LogicalAnd, logical.Operator);
        Assert.IsInstanceOfType<Identifier>(logical.Left);
        Assert.IsInstanceOfType<Identifier>(logical.Right);
        Assert.AreEqual("a", ((Identifier)logical.Left).Name);
        Assert.AreEqual("b", ((Identifier)logical.Right).Name);
    }

    [TestMethod]
    public void OptimizeLogical_NullishDisjunctionWithStableIdentifier_ElidesImpliedNonNullGuard()
    {
        var value = new Identifier("value");
        var nullBranch = new NonLogicalBinaryExpression(Operator.Equality, value, new NullLiteral("null"));
        var nonNullGuard = new NonLogicalBinaryExpression(Operator.Inequality, value, new NullLiteral("null"));
        var matches = new CallExpression(new Identifier("matches"), NodeList.Empty<Expression>(), optional: false);
        var expression = new LogicalExpression(
            Operator.LogicalOr,
            nullBranch,
            new LogicalExpression(Operator.LogicalAnd, nonNullGuard, matches));

        var result = Optimizer.OptimizeLogical(expression);

        Assert.AreEqual("value == null || matches()", result.ToKnRECMAScript());
    }

    [TestMethod]
    public void OptimizeLogical_NullishDisjunctionWithMemberRead_PreservesNonNullGuard()
    {
        var value = new MemberExpression(
            new Identifier("source"),
            new Identifier("current"),
            computed: false,
            optional: false);
        var nullBranch = new NonLogicalBinaryExpression(Operator.Equality, value, new NullLiteral("null"));
        var nonNullGuard = new NonLogicalBinaryExpression(Operator.Inequality, value, new NullLiteral("null"));
        var expression = new LogicalExpression(
            Operator.LogicalOr,
            nullBranch,
            new LogicalExpression(Operator.LogicalAnd, nonNullGuard, new Identifier("matches")));

        var result = Optimizer.OptimizeLogical(expression);

        Assert.AreEqual("source.current == null || source.current != null && matches", result.ToKnRECMAScript());
    }

    #endregion

    #region 多操作数去重测试

    /// <summary>
    /// 测试三个操作数去重: A && B && A -> A && B
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_ThreeOperandsWithDuplicate_RemovesDuplicate()
    {
        // Arrange: (A && B) && A
        var a = new Identifier("a");
        var b = new Identifier("b");
        var inner = new LogicalExpression(Operator.LogicalAnd, a, b);
        var expr = new LogicalExpression(Operator.LogicalAnd, inner, a);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 应该优化为 A && B
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var logical = (LogicalExpression)result;
        Assert.AreEqual(Operator.LogicalAnd, logical.Operator);

        // 验证结果包含 a 和 b
        var operands = CollectOperands(logical, Operator.LogicalAnd);
        Assert.HasCount(2, operands);
        Assert.IsTrue(operands.Any(op => op is Identifier id && id.Name == "a"));
        Assert.IsTrue(operands.Any(op => op is Identifier id && id.Name == "b"));
    }

    /// <summary>
    /// 测试多个重复: A && A && A -> A
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_MultipleDuplicates_ReturnsSingleOperand()
    {
        // Arrange: ((A && A) && A)
        var a = new Identifier("a");
        var inner = new LogicalExpression(Operator.LogicalAnd, a, a);
        var expr = new LogicalExpression(Operator.LogicalAnd, inner, a);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert
        Assert.IsInstanceOfType<Identifier>(result);
        Assert.AreEqual("a", ((Identifier)result).Name);
    }

    /// <summary>
    /// 测试四个操作数去重: A && B && C && A -> A && B && C
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_FourOperandsWithDuplicate_RemovesDuplicate()
    {
        // Arrange: ((A && B) && (C && A))
        var a = new Identifier("a");
        var b = new Identifier("b");
        var c = new Identifier("c");
        var left = new LogicalExpression(Operator.LogicalAnd, a, b);
        var right = new LogicalExpression(Operator.LogicalAnd, c, a);
        var expr = new LogicalExpression(Operator.LogicalAnd, left, right);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 应该优化为包含 3 个唯一操作数的树
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var logical = (LogicalExpression)result;
        var operands = CollectOperands(logical, Operator.LogicalAnd);
        Assert.HasCount(3, operands);
    }

    #endregion

    #region 不同运算符混合测试

    /// <summary>
    /// 测试不同运算符不混淆: A && B || A && B
    /// 注意：由于两个 && 子表达式完全相同，会被去重成一个
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_IdenticalSubExprWithDifferentOps_Deduplicated()
    {
        // Arrange: (A && B) || (A && B)
        // 由于两个子表达式完全相同，应该优化为 (A && B)
        var a = new Identifier("a");
        var b = new Identifier("b");
        var left = new LogicalExpression(Operator.LogicalAnd, a, b);
        var right = new LogicalExpression(Operator.LogicalAnd, a, b);
        var expr = new LogicalExpression(Operator.LogicalOr, left, right);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 由于两个 && 表达式相同，应该被优化为单个 A && B
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var logical = (LogicalExpression)result;
        Assert.AreEqual(Operator.LogicalAnd, logical.Operator);
    }

    /// <summary>
    /// 测试不同运算符且子表达式不同时保持结构: A && B || A && C
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_DifferentSubExprWithDifferentOps_PreservesOrStructure()
    {
        // Arrange: (A && B) || (A && C)
        var a = new Identifier("a");
        var b = new Identifier("b");
        var c = new Identifier("c");
        var left = new LogicalExpression(Operator.LogicalAnd, a, b);
        var right = new LogicalExpression(Operator.LogicalAnd, a, c);
        var expr = new LogicalExpression(Operator.LogicalOr, left, right);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: || 结构应该保持
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var logical = (LogicalExpression)result;
        Assert.AreEqual(Operator.LogicalOr, logical.Operator);
    }

    /// <summary>
    /// 测试嵌套不同运算符: A && (B || A) 不应该去掉 A
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_NestedDifferentOperators_PreservesStructure()
    {
        // Arrange: A && (B || A)
        var a = new Identifier("a");
        var b = new Identifier("b");
        var inner = new LogicalExpression(Operator.LogicalOr, b, a);
        var expr = new LogicalExpression(Operator.LogicalAnd, a, inner);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: A 和 (B || A) 应该保持
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var logical = (LogicalExpression)result;
        Assert.AreEqual(Operator.LogicalAnd, logical.Operator);
        Assert.IsInstanceOfType<Identifier>(logical.Left);
        Assert.IsInstanceOfType<LogicalExpression>(logical.Right);
    }

    #endregion

    #region 嵌套表达式测试

    /// <summary>
    /// 测试复杂嵌套表达式
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_ComplexNestedExpression_OptimizesCorrectly()
    {
        // Arrange: (A && B && C) && (D && A && E)
        // 期望: A && B && C && D && E (去重 A)
        var a = new Identifier("a");
        var b = new Identifier("b");
        var c = new Identifier("c");
        var d = new Identifier("d");
        var e = new Identifier("e");

        var leftInner = new LogicalExpression(Operator.LogicalAnd, a, b);
        var left = new LogicalExpression(Operator.LogicalAnd, leftInner, c);

        var rightInner = new LogicalExpression(Operator.LogicalAnd, d, a);
        var right = new LogicalExpression(Operator.LogicalAnd, rightInner, e);

        var expr = new LogicalExpression(Operator.LogicalAnd, left, right);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var logical = (LogicalExpression)result;
        var operands = CollectOperands(logical, Operator.LogicalAnd);

        // 应该有 5 个唯一操作数：a, b, c, d, e
        Assert.HasCount(5, operands);
    }

    #endregion

    #region 边界条件测试

    /// <summary>
    /// 测试非逻辑表达式直接返回
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_NonLogicalExpression_ReturnsSame()
    {
        // Arrange
        var a = new Identifier("a");

        // Act
        var result = Optimizer.OptimizeLogical(a);

        // Assert
        Assert.AreSame(a, result);
    }

    /// <summary>
    /// 测试布尔字面量去重
    /// </summary>
    [TestMethod]
    public void OptimizeLiteral_BooleanDuplicate_ReturnsSingleLiteral()
    {
        // Arrange: true && true
        var lit = new BooleanLiteral(true, "true");
        var expr = new LogicalExpression(Operator.LogicalAnd, lit, lit);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert
        Assert.IsInstanceOfType<BooleanLiteral>(result);
        Assert.IsTrue(((BooleanLiteral)result).Value);
    }

    /// <summary>
    /// 测试字符串字面量去重
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_StringLiteralDuplicate_ReturnsSingleLiteral()
    {
        // Arrange: "hello" || "hello"
        var lit = new StringLiteral("hello", "\"hello\"");
        var expr = new LogicalExpression(Operator.LogicalOr, lit, lit);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert
        Assert.IsInstanceOfType<StringLiteral>(result);
        Assert.AreEqual("hello", ((StringLiteral)result).Value);
    }

    #endregion

    #region 递归深度测试

    /// <summary>
    /// 测试深层嵌套表达式不会栈溢出
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_DeepNestedExpression_NoStackOverflow()
    {
        // Arrange: 构建深度嵌套的 a && a && a && ...
        var a = new Identifier("a");
        Expression expr = a;

        // 构建深度为 50 的嵌套
        for (int i = 0; i < 50; i++)
        {
            expr = new LogicalExpression(Operator.LogicalAnd, expr, a);
        }

        // Act & Assert: 不应抛出异常
        var result = Optimizer.OptimizeLogical(expr);

        // 应该优化为单个 a
        Assert.IsInstanceOfType<Identifier>(result);
        Assert.AreEqual("a", ((Identifier)result).Name);
    }

    #endregion

    #region Nullish Coalescing 测试

    /// <summary>
    /// 测试空值合并运算符去重
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_NullishCoalescingDuplicate_ReturnsSingleOperand()
    {
        // Arrange: a ?? a
        var a = new Identifier("a");
        var expr = new LogicalExpression(Operator.NullishCoalescing, a, a);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert
        Assert.IsInstanceOfType<Identifier>(result);
        Assert.AreEqual("a", ((Identifier)result).Name);
    }

    #endregion

    #region 副作用检测测试

    /// <summary>
    /// 测试函数调用不去重: foo() && foo() 保持不变
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_FunctionCallDuplicate_PreservesBothCalls()
    {
        // Arrange: foo() && foo()
        var foo = new Identifier("foo");
        var call = new CallExpression(foo, NodeList.From<Expression>(), optional: false);
        var expr = new LogicalExpression(Operator.LogicalAnd, call, call);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 应该保持原样，因为函数调用有副作用
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var logical = (LogicalExpression)result;
        Assert.AreEqual(Operator.LogicalAnd, logical.Operator);
        Assert.IsInstanceOfType<CallExpression>(logical.Left);
        Assert.IsInstanceOfType<CallExpression>(logical.Right);
    }

    /// <summary>
    /// 测试 new 表达式不去重
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_NewExpressionDuplicate_PreservesBoth()
    {
        // Arrange: new Foo() && new Foo()
        var foo = new Identifier("Foo");
        var newExpr = new NewExpression(foo, NodeList.From<Expression>());
        var expr = new LogicalExpression(Operator.LogicalAnd, newExpr, newExpr);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 应该保持原样
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var logical = (LogicalExpression)result;
        Assert.AreEqual(Operator.LogicalAnd, logical.Operator);
    }

    /// <summary>
    /// 测试赋值表达式不去重
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_AssignmentDuplicate_PreservesBoth()
    {
        // Arrange: (a = 1) && (a = 1)
        var a = new Identifier("a");
        var one = new NumericLiteral(1, "1");
        var assign = new AssignmentExpression(Operator.Assignment, a, one);
        var expr = new LogicalExpression(Operator.LogicalAnd, assign, assign);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 应该保持原样
        Assert.IsInstanceOfType<LogicalExpression>(result);
    }

    /// <summary>
    /// 测试更新表达式不去重: a++ && a++
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_UpdateExpressionDuplicate_PreservesBoth()
    {
        // Arrange: a++ && a++
        var a = new Identifier("a");
        var update = new UpdateExpression(Operator.Increment, a, prefix: false);
        var expr = new LogicalExpression(Operator.LogicalAnd, update, update);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 应该保持原样
        Assert.IsInstanceOfType<LogicalExpression>(result);
    }

    /// <summary>
    /// 测试混合副作用：纯表达式和函数调用混合
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_MixedSideEffect_PreservesStructure()
    {
        // Arrange: a && foo() && a
        // 由于 foo() 有副作用，整个表达式不应该去重
        var a = new Identifier("a");
        var foo = new Identifier("foo");
        var call = new CallExpression(foo, NodeList.From<Expression>(), optional: false);
        var inner = new LogicalExpression(Operator.LogicalAnd, a, call);
        var expr = new LogicalExpression(Operator.LogicalAnd, inner, a);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 应该保持原样，因为包含函数调用
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var operands = CollectOperands((LogicalExpression)result, Operator.LogicalAnd);
        Assert.HasCount(3, operands); // a, foo(), a 都保留
    }

    /// <summary>
    /// 测试纯表达式可以正确去重
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_PureExpression_DeduplicatesCorrectly()
    {
        // Arrange: (typeof a === "string") && (typeof a === "string")
        var a = new Identifier("a");
        var typeofA = new NonUpdateUnaryExpression(Operator.TypeOf, a);
        var stringType = new StringLiteral("string", "\"string\"");
        var check = new NonLogicalBinaryExpression(Operator.StrictEquality, typeofA, stringType);
        var expr = new LogicalExpression(Operator.LogicalAnd, check, check);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 应该去重
        Assert.IsInstanceOfType<NonLogicalBinaryExpression>(result);
    }

    /// <summary>
    /// 测试成员访问中的副作用
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_MemberAccessWithSideEffect_PreservesStructure()
    {
        // Arrange: foo().bar && foo().bar
        // foo() 有副作用，不应去重
        var foo = new Identifier("foo");
        var call = new CallExpression(foo, NodeList.From<Expression>(), optional: false);
        var bar = new Identifier("bar");
        var member = new MemberExpression(call, bar, computed: false, optional: false);
        var expr = new LogicalExpression(Operator.LogicalAnd, member, member);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 应该保持原样
        Assert.IsInstanceOfType<LogicalExpression>(result);
    }

    #endregion

    #region 顺序保持测试

    /// <summary>
    /// 测试操作数顺序保持: (B && A) && C && A -> B && A && C
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_PreservesOperandOrder_Basic()
    {
        // Arrange: (B && A) && C && A  -> 期望 B && A && C
        var a = new Identifier("a");
        var b = new Identifier("b");
        var c = new Identifier("c");

        var inner = new LogicalExpression(Operator.LogicalAnd, b, a);
        var middle = new LogicalExpression(Operator.LogicalAnd, inner, c);
        var expr = new LogicalExpression(Operator.LogicalAnd, middle, a);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 顺序应该是 B, A, C
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var operands = CollectOperands((LogicalExpression)result, Operator.LogicalAnd);
        Assert.HasCount(3, operands);
        Assert.AreEqual("b", ((Identifier)operands[0]).Name);
        Assert.AreEqual("a", ((Identifier)operands[1]).Name);
        Assert.AreEqual("c", ((Identifier)operands[2]).Name);
    }

    /// <summary>
    /// 测试操作数顺序保持：复杂属性模式匹配表达式去重
    /// 输入: obj != null && ("Name" in obj && obj.Name === "John") && (obj != null && ("Age" in obj && obj.Age > 18))
    /// 期望: obj != null && "Name" in obj && obj.Name === "John" && "Age" in obj && obj.Age > 18
    /// 说明: obj != null 在表达式中出现两次，应该去重为一个，并保持在最前面的位置
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_ComplexPatternMatch_PreservesPotentialProxyEffects()
    {
        // Arrange: 构建类似 C# 属性模式匹配的表达式
        // C# 原始代码可能类似于: obj is { Name: "John" } && obj is { Age: > 18 }
        // 转换后会生成重复的 null 检查
        var expr = new Parser()
            .ParseExpression($"obj != null && (\"Name\" in obj && obj.Name === \"John\") && (obj != null && (\"Age\" in obj && obj.Age > 18))");

        // Act
        var result = Optimizer.OptimizeLogical(expr);
        var script = result.ToKnRECMAScript();

        // Assert: `in` and member access may invoke proxy/getter behavior, so the
        // optimizer must preserve both null checks and the original evaluation order.
        Assert.AreEqual(
@"obj != null && (""Name"" in obj && obj.Name === ""John"") && (obj != null && (""Age"" in obj && obj.Age > 18))", script);
    }

    /// <summary>
    /// 测试操作数顺序保持：中间重复的情况
    /// 输入: A && B && A && C
    /// 期望: A && B && C (第一个 A 保留，第二个 A 去重)
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_PreservesOperandOrder_DuplicateInMiddle()
    {
        // Arrange: A && B && A && C
        var a = new Identifier("a");
        var b = new Identifier("b");
        var c = new Identifier("c");

        var inner = new LogicalExpression(Operator.LogicalAnd, a, b);
        var middle = new LogicalExpression(Operator.LogicalAnd, inner, a);
        var expr = new LogicalExpression(Operator.LogicalAnd, middle, c);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 顺序应该是 A, B, C
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var operands = CollectOperands((LogicalExpression)result, Operator.LogicalAnd);
        Assert.HasCount(3, operands);
        Assert.AreEqual("a", ((Identifier)operands[0]).Name);
        Assert.AreEqual("b", ((Identifier)operands[1]).Name);
        Assert.AreEqual("c", ((Identifier)operands[2]).Name);
    }

    /// <summary>
    /// 测试操作数顺序保持：末尾重复的情况
    /// 输入: A && B && C && A
    /// 期望: A && B && C (第一个 A 保留，末尾 A 去重)
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_PreservesOperandOrder_DuplicateAtEnd()
    {
        // Arrange: A && B && C && A
        var a = new Identifier("a");
        var b = new Identifier("b");
        var c = new Identifier("c");

        var inner = new LogicalExpression(Operator.LogicalAnd, a, b);
        var middle = new LogicalExpression(Operator.LogicalAnd, inner, c);
        var expr = new LogicalExpression(Operator.LogicalAnd, middle, a);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 顺序应该是 A, B, C
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var operands = CollectOperands((LogicalExpression)result, Operator.LogicalAnd);
        Assert.HasCount(3, operands);
        Assert.AreEqual("a", ((Identifier)operands[0]).Name);
        Assert.AreEqual("b", ((Identifier)operands[1]).Name);
        Assert.AreEqual("c", ((Identifier)operands[2]).Name);
    }

    /// <summary>
    /// 测试操作数顺序保持：多个重复操作数
    /// 输入: C && A && B && A && C && B
    /// 期望: C && A && B (首次出现的顺序保持)
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_PreservesOperandOrder_MultipleDuplicates()
    {
        // Arrange: C && A && B && A && C && B
        var a = new Identifier("a");
        var b = new Identifier("b");
        var c = new Identifier("c");

        // 构建树: ((((C && A) && B) && A) && C) && B
        Expression expr = new LogicalExpression(Operator.LogicalAnd, c, a);
        expr = new LogicalExpression(Operator.LogicalAnd, expr, b);
        expr = new LogicalExpression(Operator.LogicalAnd, expr, a);
        expr = new LogicalExpression(Operator.LogicalAnd, expr, c);
        expr = new LogicalExpression(Operator.LogicalAnd, expr, b);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 顺序应该是 C, A, B (首次出现的顺序)
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var operands = CollectOperands((LogicalExpression)result, Operator.LogicalAnd);
        Assert.HasCount(3, operands);
        Assert.AreEqual("c", ((Identifier)operands[0]).Name);
        Assert.AreEqual("a", ((Identifier)operands[1]).Name);
        Assert.AreEqual("b", ((Identifier)operands[2]).Name);
    }

    /// <summary>
    /// 测试 || 运算符的操作数顺序保持
    /// 输入: A || B || A || C
    /// 期望: A || B || C
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_PreservesOperandOrder_OrOperator()
    {
        // Arrange: A || B || A || C
        var a = new Identifier("a");
        var b = new Identifier("b");
        var c = new Identifier("c");

        var inner = new LogicalExpression(Operator.LogicalOr, a, b);
        var middle = new LogicalExpression(Operator.LogicalOr, inner, a);
        var expr = new LogicalExpression(Operator.LogicalOr, middle, c);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 顺序应该是 A, B, C
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var operands = CollectOperands((LogicalExpression)result, Operator.LogicalOr);
        Assert.HasCount(3, operands);
        Assert.AreEqual("a", ((Identifier)operands[0]).Name);
        Assert.AreEqual("b", ((Identifier)operands[1]).Name);
        Assert.AreEqual("c", ((Identifier)operands[2]).Name);
    }

    /// <summary>
    /// 测试混合 || 和 && 的复杂表达式
    /// 输入: (obj == null) || ((obj != null && ("Name" in obj && obj.Name === "John")) && (obj != null && ("Age" in obj && obj.Age > 18))) || c > 0
    /// 期望: 首个 <c>obj != null</c> 会由前面的 null 分支蕴含；属性读取后的
    /// <c>obj != null</c> 仍保留，因为 Proxy trap 可能在两次读取之间改变局部值。
    /// 注意：由于 && 优先级高于 ||，括号可以省略
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_MixedOrAndAnd_PreservesPotentialProxyEffects()
    {
        // Arrange: 混合 || 和 && 的复杂表达式
        var expr = new Parser()
            .ParseExpression($"(obj == null) || ((obj != null && (\"Name\" in obj && obj.Name === \"John\")) && (obj != null && (\"Age\" in obj && obj.Age > 18))) || c > 0");

        // Act
        var result = Optimizer.OptimizeLogical(expr);
        var script = result.ToKnRECMAScript();

        // Assert: the proxy-sensitive `in` and member operations keep their
        // surrounding checks and evaluation order intact.
        Assert.AreEqual(
@"obj == null || ""Name"" in obj && obj.Name === ""John"" && obj != null && ""Age"" in obj && obj.Age > 18 || c > 0", script);
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 收集逻辑表达式的所有操作数
    /// </summary>
    private static List<Expression> CollectOperands(Expression expr, Operator op)
    {
        var operands = new List<Expression>();
        CollectOperandsCore(expr, op, operands);
        return operands;
    }

    private static void CollectOperandsCore(Expression expr, Operator op, List<Expression> operands)
    {
        if (expr is LogicalExpression le && le.Operator == op)
        {
            CollectOperandsCore(le.Left, op, operands);
            CollectOperandsCore(le.Right, op, operands);
        }
        else
        {
            operands.Add(expr);
        }
    }

    #endregion

    #region 更多基本去重测试

    /// <summary>
    /// 测试五个操作数去重
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_FiveOperandsWithDuplicate_RemovesDuplicate()
    {
        // Arrange: A && B && C && D && A -> A && B && C && D
        var a = new Identifier("a");
        var b = new Identifier("b");
        var c = new Identifier("c");
        var d = new Identifier("d");
        var left = new LogicalExpression(Operator.LogicalAnd, a, b);
        var middle = new LogicalExpression(Operator.LogicalAnd, left, c);
        var right = new LogicalExpression(Operator.LogicalAnd, d, a);
        var expr = new LogicalExpression(Operator.LogicalAnd, middle, right);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var operands = CollectOperands((LogicalExpression)result, Operator.LogicalAnd);
        Assert.HasCount(4, operands);
    }

    /// <summary>
    /// 测试无重复时保持结构
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_NoDuplicates_PreservesAll()
    {
        // Arrange: A && B && C && D
        var a = new Identifier("a");
        var b = new Identifier("b");
        var c = new Identifier("c");
        var d = new Identifier("d");
        var left = new LogicalExpression(Operator.LogicalAnd, a, b);
        var right = new LogicalExpression(Operator.LogicalAnd, c, d);
        var expr = new LogicalExpression(Operator.LogicalAnd, left, right);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var operands = CollectOperands((LogicalExpression)result, Operator.LogicalAnd);
        Assert.HasCount(4, operands);
    }

    #endregion

    #region Nullish Coalescing 更多测试

    /// <summary>
    /// 测试空值合并运算符嵌套
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_NullishCoalescingNested_Optimizes()
    {
        // Arrange: a ?? b ?? a
        var a = new Identifier("a");
        var b = new Identifier("b");
        var inner = new LogicalExpression(Operator.NullishCoalescing, a, b);
        var expr = new LogicalExpression(Operator.NullishCoalescing, inner, a);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 应该优化为 a ?? b
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var logical = (LogicalExpression)result;
        Assert.AreEqual(Operator.NullishCoalescing, logical.Operator);
    }

    /// <summary>
    /// 测试空值合并与 || 混合
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_NullishCoalescingMixedOr_PreservesStructure()
    {
        // Arrange: (a ?? b) || (a ?? b)
        var a = new Identifier("a");
        var b = new Identifier("b");
        var nullish = new LogicalExpression(Operator.NullishCoalescing, a, b);
        var expr = new LogicalExpression(Operator.LogicalOr, nullish, nullish);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 由于表达式相同，应该去重
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var logical = (LogicalExpression)result;
        Assert.AreEqual(Operator.NullishCoalescing, logical.Operator);
    }

    #endregion

    #region 更多副作用测试

    /// <summary>
    /// 测试数组访问副作用
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_ArrayAccessWithSideEffect_PreservesStructure()
    {
        // Arrange: arr[0] && arr[0]
        var arr = new Identifier("arr");
        var zero = new NumericLiteral(0, "0");
        var access = new MemberExpression(arr, zero, computed: true, optional: false);
        var expr = new LogicalExpression(Operator.LogicalAnd, access, access);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: 数组访问可能有副作用，不应该去重
        Assert.IsInstanceOfType<LogicalExpression>(result);
    }

    /// <summary>
    /// 测试对象属性访问副作用
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_PropertyAccess_PreservesPotentialGetterEffects()
    {
        // Arrange: obj.prop && obj.prop
        var obj = new Identifier("obj");
        var prop = new Identifier("prop");
        var access = new MemberExpression(obj, prop, computed: false, optional: false);
        var expr = new LogicalExpression(Operator.LogicalAnd, access, access);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert: property access can execute a getter or proxy trap.
        Assert.IsInstanceOfType<LogicalExpression>(result);
    }

    #endregion

    #region 数值字面量测试

    /// <summary>
    /// 测试数值字面量去重
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_NumericLiteralDuplicate_ReturnsSingleLiteral()
    {
        // Arrange: 42 && 42
        var lit = new NumericLiteral(42, "42");
        var expr = new LogicalExpression(Operator.LogicalAnd, lit, lit);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert
        Assert.IsInstanceOfType<NumericLiteral>(result);
        Assert.AreEqual(42, ((NumericLiteral)result).Value);
    }

    /// <summary>
    /// 测试不同数值不去重
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_DifferentNumericLiterals_PreservesBoth()
    {
        // Arrange: 1 && 2
        var one = new NumericLiteral(1, "1");
        var two = new NumericLiteral(2, "2");
        var expr = new LogicalExpression(Operator.LogicalAnd, one, two);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert
        Assert.IsInstanceOfType<LogicalExpression>(result);
    }

    #endregion

    #region 复杂嵌套测试

    /// <summary>
    /// 测试深度嵌套表达式
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_DeepNestedNoDuplicates_PreservesAll()
    {
        // Arrange: 构建深度为10的表达式
        Expression expr = new Identifier("a");
        for (int i = 0; i < 10; i++)
        {
            expr = new LogicalExpression(Operator.LogicalAnd, expr, new Identifier($"b{i}"));
        }

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert
        Assert.IsInstanceOfType<LogicalExpression>(result);
        var operands = CollectOperands((LogicalExpression)result, Operator.LogicalAnd);
        Assert.HasCount(11, operands); // a + b0..b9
    }

    /// <summary>
    /// 测试混合运算符深度嵌套
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_MixedDeepNested_PreservesStructure()
    {
        // Arrange: ((a && b) || c) && (d || (e && f))
        var a = new Identifier("a");
        var b = new Identifier("b");
        var c = new Identifier("c");
        var d = new Identifier("d");
        var e = new Identifier("e");
        var f = new Identifier("f");

        var and1 = new LogicalExpression(Operator.LogicalAnd, a, b);
        var or1 = new LogicalExpression(Operator.LogicalOr, and1, c);
        var and2 = new LogicalExpression(Operator.LogicalAnd, e, f);
        var or2 = new LogicalExpression(Operator.LogicalOr, d, and2);
        var expr = new LogicalExpression(Operator.LogicalAnd, or1, or2);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert
        Assert.IsInstanceOfType<LogicalExpression>(result);
    }

    #endregion

    #region 边界情况测试

    /// <summary>
    /// 测试 null 值处理
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_NullLiteral_HandlesCorrectly()
    {
        // Arrange: null && null
        var nullLit = new NullLiteral("null");
        var expr = new LogicalExpression(Operator.LogicalAnd, nullLit, nullLit);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert
        Assert.IsInstanceOfType<NullLiteral>(result);
    }

    /// <summary>
    /// 测试 this 表达式
    /// </summary>
    [TestMethod]
    public void OptimizeLogical_ThisExpression_HandlesCorrectly()
    {
        // Arrange: this && this
        var thisExpr = new ThisExpression();
        var expr = new LogicalExpression(Operator.LogicalAnd, thisExpr, thisExpr);

        // Act
        var result = Optimizer.OptimizeLogical(expr);

        // Assert
        Assert.IsInstanceOfType<ThisExpression>(result);
    }

    #endregion
}
