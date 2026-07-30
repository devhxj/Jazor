using Acornima;
using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

/// <summary>
/// SemanticWalker 模式匹配功能测试类
///
/// 本测试类验证 C# 模式匹配语法到 JavaScript 的转换功能，涵盖：
/// - IsPattern: 常量模式匹配 (obj is 42, obj is "hello")
/// - IsType: 类型检查 (obj is string, obj is int, obj is DateTime 等)
/// - IsNull/IsNotNull: null 检查 (obj is null, obj is not null)
/// - DiscardPattern: 丢弃模式 (_ 作为默认分支)
/// - NegatedPattern: 取反模式 (obj is not null)
/// - BinaryPattern: 逻辑模式 (and, or)
/// - RelationalPattern: 关系模式 (>, <, >=, <=, ==, !=)
/// - TypePattern: 类型模式
/// - PropertySubpattern: 属性子模式 ({ Name: "John" })
/// - RecursivePattern: 递归模式 (类型+属性、元组模式)
/// - ListPattern: 列表模式 ([1, 2, 3], [..], [var first, .. var rest])
/// - SlicePattern: 切片模式 (列表解构)
/// - DeclarationPattern: 声明模式 (obj is int value)
/// - ConstantPattern: 常量模式
/// - SwitchExpression/CaseClause: switch 表达式和 case 子句
///
/// 测试方法命名约定：
/// - Visit_[PatternType]_[Scenario]: 完整块级转换测试
/// - Visit_[PatternType]_[Scenario]_Direct: 直接调用特定 Visit 方法测试
/// - Visit_ComplexPattern_[Scenario]: 复杂模式组合测试
/// </summary>
[TestClass]
public sealed class SemanticWalkerPatternTest
{
  /// <summary>
  /// 编译代码并获取roslyn代码块
  /// </summary>
  /// <param name="code"></param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  private static IBlockOperation GetBlockOperation(string code)
  {
    var usings = @"
          global using System;
          global using System.Collections.Generic;
          global using System.Linq;
          global using System.Numerics;
          global using ECMAScript;
          global using static ECMAScript.Global;";

    var references = TestMetadataReferences.Net11
      .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location));
    var compilation = CSharpCompilation.Create(
      assemblyName: "TestAssembly",
      syntaxTrees: [
        CSharpSyntaxTree.ParseText(usings),
          CSharpSyntaxTree.ParseText(code)
      ],
      references: references,
      options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    // 输出编译诊断信息
    var diagnostics = compilation.GetDiagnostics();
    var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
    if (errors.Count > 0)
    {
      var errorMessages = string.Join("\n", errors.Select(e => $"{e.Id}: {e.GetMessage()}"));
      throw new InvalidOperationException(errorMessages);
    }

    var syntaxTree = compilation.SyntaxTrees.Last();
    var semanticModel = compilation.GetSemanticModel(syntaxTree);
    var root = syntaxTree.GetRoot();

    // 查找第一个方法体
    var methodDeclaration = root.DescendantNodes()
      .OfType<MethodDeclarationSyntax>()
      .FirstOrDefault(static method => method.Identifier.ValueText == "TestMethod" && method.Body is not null)
      ?? root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(static method => method.Body is not null);
    if (methodDeclaration?.Body is not null)
    {
      var operation = semanticModel.GetOperation(methodDeclaration.Body) as IBlockOperation;
      if (operation is not null)
        return operation;
    }

    throw new InvalidOperationException("未找到可分析的操作");
  }

  /// <summary>
  /// 获取指定索引的操作
  /// </summary>
  private static T GetOperationAt<T>(IBlockOperation block, int index = 0) where T : class, IOperation
  {
    var operation = block.Operations.Skip(index).First();
    return operation as T ?? throw new InvalidOperationException("未找到可分析的操作");
  }

  private static void AssertScriptEqual(string expected, string? actual)
  {
    Assert.AreEqual(ExpectedJsNaming.Normalize(expected).ReplaceLineEndings(), actual?.ReplaceLineEndings());
  }

  private static void AssertContainsCount(string? actual, string expected, int count)
  {
    Assert.IsNotNull(actual);
    expected = ExpectedJsNaming.Normalize(expected);
    var actualCount = actual!.Split([expected], StringSplitOptions.None).Length - 1;
    Assert.AreEqual(count, actualCount, $"Expected '{expected}' to appear {count} time(s), but found {actualCount}.{Environment.NewLine}{actual}");
  }

  private static void AssertStringContainsJsNaming(string? actual, string expected)
  {
    Assert.IsNotNull(actual);
    StringAssert.Contains(actual!, ExpectedJsNaming.Normalize(expected), StringComparison.Ordinal);
  }

  private static void AssertStringContainsJsNaming(string? actual, string expected, StringComparison comparisonType)
  {
    Assert.IsNotNull(actual);
    StringAssert.Contains(actual!, ExpectedJsNaming.Normalize(expected), comparisonType);
  }

  /// <summary>
  /// 测试 Visit - IsPattern 常量模式匹配
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_Constant()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    bool result = obj is 42;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let obj = 42;
  let result = obj === 42;
}", script);

  }

  /// <summary>
  /// 测试 VisitIsPattern - 常量模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_Constant_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    bool result = obj is 42;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var node = walker.VisitIsPattern(isPatternOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("obj === 42", script);
  }

  /// <summary>
  /// 测试 Visit - IsPattern 字符串常量模式
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_StringConstant()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    bool result = obj is ""hello"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let obj = ""hello"";
  let result = obj === ""hello"";
}", script);

  }

  /// <summary>
  /// 测试 VisitIsPattern - 字符串常量模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_StringConstant_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    bool result = obj is ""hello"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var node = walker.VisitIsPattern(isPatternOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"obj === ""hello""", script);
  }

  /// <summary>
  /// 测试 Visit - IsType 字符串类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_String()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    bool result = obj is string;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = ""hello"";
  let result = typeof obj === ""string"";
}", script);
  }

  /// <summary>
  /// 测试 VisitIsType - 字符串类型检查（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_IsType_String_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    bool result = obj is string;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isTypeOperation = declarator.Initializer!.Value as IIsTypeOperation;
    var node = walker.VisitIsType(isTypeOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"typeof obj === ""string""", script);
  }

  /// <summary>
  /// 测试 Visit - IsType 整数类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Int()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    bool result = obj is int;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = 42;
  let result = typeof obj === ""number"";
}", script);
  }

  /// <summary>
  /// 测试 VisitIsType - 整数类型检查（直接调用）
  /// </summary>
  [TestMethod]
  public void VisitIsType_Int_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    bool result = obj is int;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isTypeOperation = declarator.Initializer!.Value as IIsTypeOperation;
    var node = walker.VisitIsType(isTypeOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"typeof obj === ""number""", script);
  }

  /// <summary>
  /// 测试 Visit - IsType 布尔类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Boolean()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = true;
                    bool result = obj is bool;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = true;
  let result = typeof obj === ""boolean"";
}", script);
  }

  /// <summary>
  /// 测试 VisitIsType - 布尔类型检查（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Boolean_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = true;
                    bool result = obj is bool;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isTypeOperation = declarator.Initializer!.Value as IIsTypeOperation;
    var node = walker.VisitIsType(isTypeOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"typeof obj === ""boolean""", script);
  }

  /// <summary>
  /// 测试 Visit - IsType 对象类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Object()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new object();
                    bool result = obj is object;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = new Object;
  let result = obj != null;
}", script);
  }

  /// <summary>
  /// 测试 VisitIsType - 对象类型检查（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Object_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new object();
                    bool result = obj is object;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isTypeOperation = declarator.Initializer!.Value as IIsTypeOperation;
    var node = walker.VisitIsType(isTypeOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"obj != null", script);
  }

  [TestMethod]
  public void Visit_IsType_Object_AllowsErasedPrimitiveValuesByCheckingOnlyNullishness()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(object number, object text)
                {
                    bool numberResult = number is object;
                    bool textResult = text is object;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let numberResult = number != null;
  let textResult = text != null;
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsNull null 检查
  /// </summary>
  [TestMethod]
  public void Visit_IsNull()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = null;
                    bool result = obj is null;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = null;
  let result = obj == null;
}", script);
  }

  /// <summary>
  /// 测试 VisitIsNull - null 检查（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_IsNull_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = null;
                    bool result = obj is null;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var node = walker.Visit(isPatternOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("obj == null", script);
  }

  /// <summary>
  /// 测试 Visit - IsNotNull not null 检查
  /// </summary>
  [TestMethod]
  public void Visit_IsNotNull()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = null;
                    bool result = obj is not null;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = null;
  let result = !(obj == null);
}", script);
  }

  /// <summary>
  /// 测试 VisitIsNotNull -not null 检查（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_IsNotNull_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = null;
                    bool result = obj is not null;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var node = walker.Visit(isPatternOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("!(obj == null)", script);
  }

  /// <summary>
  /// 测试 Visit - DiscardPattern 丢弃模式（在 switch 表达式中）
  /// 丢弃模式 _ 作为 switch 表达式的默认分支，总是匹配
  /// </summary>
  [TestMethod]
  public void Visit_DiscardPattern_SwitchExpression()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 42;
                    string result = value switch
                    {
                        1 => ""one"",
                        2 => ""two"",
                        _ => ""default""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 42;
  let result = (() => {
    const v$0 = value;
    if (v$0 === 1)
      return ""one"";
    if (v$0 === 2)
      return ""two"";
    return ""default"";
  })();
}", script);
  }

  [TestMethod]
  public void Visit_DiscardPattern_WithGuard_ContinuesToFollowingArm()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = -1;
                    string result = value switch
                    {
                        _ when value > 0 => ""positive"",
                        _ => ""fallback""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = -1;
  let result = (() => {
    const v$0 = value;
    if (value > 0)
      return ""positive"";
    return ""fallback"";
  })();
}", script);
  }

  /// <summary>
  /// 测试 VisitDiscardPattern - 丢弃模式（直接调用）
  /// 丢弃模式总是返回 true，表示总是匹配
  /// </summary>
  [TestMethod]
  public void Visit_DiscardPattern_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 42;
                    string result = value switch
                    {
                        _ => ""always matches""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var switchExpressionOperation = declarator.Initializer!.Value as ISwitchExpressionOperation;
    var switchCaseArm = switchExpressionOperation!.Arms.First();
    var discardPatternOperation = (IDiscardPatternOperation)switchCaseArm.Pattern;
    var node = walker.VisitDiscardPattern(discardPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("true", script);
  }

  /// <summary>
  /// 测试 Visit - NegatedPattern 取反模式
  /// </summary>
  [TestMethod]
  public void Visit_NegatedPattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    bool result = obj is not null;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let obj = 42;
  let result = !(obj == null);
}", script);

  }

  /// <summary>
  /// 测试 VisitNegatedPattern - 取反模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_NegatedPattern_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    bool result = obj is not null;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var negatedPatternOperation = (INegatedPatternOperation)isPatternOperation!.Pattern;
    var ctx = new SenseArgument(PatternInput: new Identifier("obj"));
    var node = walker.VisitNegatedPattern(negatedPatternOperation, ctx);
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("!(obj == null)", script);
  }

  /// <summary>
  /// 测试 Visit - BinaryPattern and 模式
  /// </summary>
  [TestMethod]
  public void Visit_BinaryPattern_And()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is > 0 and < 10;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = 5;
  let result = value > 0 && value < 10;
}", script);

  }

  /// <summary>
  /// 测试 VisitBinaryPattern - and 模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_BinaryPattern_And_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is > 0 and < 10;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var binaryPatternOperation = (IBinaryPatternOperation)isPatternOperation!.Pattern;
    var arg = new SenseArgument(PatternInput: new Identifier("value"));
    var node = walker.VisitBinaryPattern(binaryPatternOperation, arg);
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("value > 0 && value < 10", script);
  }

  [TestMethod]
  public void Visit_BinaryPattern_And_CachesInvocationInput()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int GetValue()
                    {
                        return 5;
                    }

                    bool result = GetValue() is > 0 and < 10;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    StringAssert.Contains(script, "let v$0;", StringComparison.Ordinal);
    AssertContainsCount(script, "= GetValue(),", 1);
    StringAssert.Contains(script, "let result = (v$0 = GetValue(), v$0 > 0 && v$0 < 10);", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - BinaryPattern or 模式
  /// </summary>
  [TestMethod]
  public void Visit_BinaryPattern_Or()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is 1 or 2 or 3 or 4 or >8;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = 5;
  let result = value === 1 || value === 2 || value === 3 || value === 4 || value > 8;
}", script);

  }

  /// <summary>
  /// 测试 VisitBinaryPattern - or 模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_BinaryPattern_Or_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is 1 or 2 or 3 or 4 or >8;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var binaryPatternOperation = (IBinaryPatternOperation)isPatternOperation!.Pattern;
    var arg = new SenseArgument(PatternInput: new Identifier("value"));
    var node = walker.VisitBinaryPattern(binaryPatternOperation, arg);
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("value === 1 || value === 2 || value === 3 || value === 4 || value > 8", script);
  }

  /// <summary>
  /// 测试 Visit - RelationalPattern 大于模式
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_GreaterThan()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is > 0;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = 5;
  let result = value > 0;
}", script);

  }

  /// <summary>
  /// 测试 VisitRelationalPattern - 大于模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_GreaterThan_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is > 0;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var relationalPatternOperation = (IRelationalPatternOperation)isPatternOperation!.Pattern;
    var arg = new SenseArgument(PatternInput: new Identifier("value"));
    var node = walker.VisitRelationalPattern(relationalPatternOperation, arg);
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("value > 0", script);
  }

  /// <summary>
  /// 测试 Visit - RelationalPattern 小于模式
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_LessThan()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is < 10;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = value < 10;
}", script);
  }

  /// <summary>
  /// 测试 VisitRelationalPattern - 小于模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_LessThan_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is < 10;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var relationalPatternOperation = (IRelationalPatternOperation)isPatternOperation!.Pattern;
    var arg = new SenseArgument(PatternInput: new Identifier("value"));
    var node = walker.VisitRelationalPattern(relationalPatternOperation, arg);
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("value < 10", script);
  }

  /// <summary>
  /// 测试 Visit - RelationalPattern 大于等于模式
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_GreaterThanOrEqual()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is >= 5;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = 5;
  let result = value >= 5;
}", script);

  }

  /// <summary>
  /// 测试 VisitRelationalPattern - 大于等于模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_GreaterThanOrEqual_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is >= 5;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var relationalPatternOperation = (IRelationalPatternOperation)isPatternOperation!.Pattern;
    var arg = new SenseArgument(PatternInput: new Identifier("value"));
    var node = walker.VisitRelationalPattern(relationalPatternOperation, arg);
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("value >= 5", script);
  }

  /// <summary>
  /// 测试 Visit - RelationalPattern 小于等于模式
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_LessThanOrEqual()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is <= 10;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = 5;
  let result = value <= 10;
}", script);

  }

  /// <summary>
  /// 测试 VisitRelationalPattern - 小于等于模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_LessThanOrEqual_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is <= 10;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var relationalPatternOperation = (IRelationalPatternOperation)isPatternOperation!.Pattern;
    var arg = new SenseArgument(PatternInput: new Identifier("value"));
    var node = walker.VisitRelationalPattern(relationalPatternOperation, arg);
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("value <= 10", script);
  }

  /// <summary>
  /// 测试 Visit - TypePattern 类型模式
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    bool result = obj is string;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = ""hello"";
  let result = typeof obj === ""string"";
}", script);
  }

  /// <summary>
  /// 测试 VisitTypePattern - 类型模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    bool result = obj is string;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isTypeOperation = declarator.Initializer!.Value as IIsTypeOperation;
    var node = walker.VisitIsType(isTypeOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"typeof obj === ""string""", script);
  }

  /// <summary>
  /// 测试 Visit - PropertySubpattern 属性子模式
  /// </summary>
  [TestMethod]
  public void Visit_PropertySubpattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var person = new { Name = ""John"", Age = 30 };
                    bool result = person is { Name: ""John"" };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(
@"{
  let person = { Name: ""John"", Age: 30 };
  let result = person != null && ""Name"" in person && person.Name === ""John"";
}", script);

  }

  /// <summary>
  /// 测试 VisitPropertySubpattern - 属性子模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_PropertySubpattern_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var person = new { Name = ""John"", Age = 30 };
                    bool result = person is { Name: ""John"" };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var recursivePatternOperation = (IRecursivePatternOperation)isPatternOperation!.Pattern;
    var propertySubpatternOperation = recursivePatternOperation.PropertySubpatterns.First();
    var arg = new SenseArgument(PatternInput: new Identifier("person"));
    var node = walker.VisitPropertySubpattern(propertySubpatternOperation, arg);
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"person.Name === ""John""", script);
  }

  /// <summary>
  /// 测试 Visit - RecursivePattern 递归模式（类型+属性）
  /// </summary>
  [TestMethod]
  public void Visit_RecursivePattern_TypeAndProperty()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Name = ""John"", Age = 30 };
                    bool result = obj is { Name: ""John"", Age: > 18 };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(
@"{
  let obj = { Name: ""John"", Age: 30 };
  let result = obj != null && ""Name"" in obj && obj.Name === ""John"" && ""Age"" in obj && obj.Age > 18;
}", script);

  }

  [TestMethod]
  public void Visit_RecursivePattern_InsideDisjunction_EmitsSingleNullGuard()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Name = ""John"", Age = 30 };
                    int c = 1;
                    bool result = obj is null || obj is { Name: ""John"", Age: > 18 } || c > 0;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(
@"{
  let obj = { Name: ""John"", Age: 30 };
  let c = 1;
  let result = obj == null || obj != null && ""Name"" in obj && obj.Name === ""John"" && ""Age"" in obj && obj.Age > 18 || c > 0;
}", script);
  }

  /// <summary>
  /// 测试 VisitRecursivePattern - 递归模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_RecursivePattern_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Name = ""John"", Age = 30 };
                    bool result = obj is { Name: ""John"", Age: > 18 };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var recursivePatternOperation = (IRecursivePatternOperation)isPatternOperation!.Pattern;
    var arg = new SenseArgument(PatternInput: new Identifier("obj"));
    var node = walker.VisitRecursivePattern(recursivePatternOperation, arg);
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(
@"obj != null && ""Name"" in obj && obj.Name === ""John"" && ""Age"" in obj && obj.Age > 18", script);

  }

  /// <summary>
  /// 测试 Visit - ListPattern 列表模式（固定长度）
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_FixedLength()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    bool result = array is [1, 2, 3];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3];
  let result = Array.isArray(array) && array.length === 3 && array[0] === 1 && array[1] === 2 && array[2] === 3;
}", script);
  }

  /// <summary>
  /// 测试 VisitListPattern - 列表模式（固定长度）（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_FixedLength_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    bool result = array is [1, 2, 3];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var listPatternOperation = (IListPatternOperation)isPatternOperation!.Pattern;
    var arg = new SenseArgument(PatternInput: new Identifier("array"));
    var node = walker.VisitListPattern(listPatternOperation, arg);
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("Array.isArray(array) && array.length === 3 && array[0] === 1 && array[1] === 2 && array[2] === 3", script);
  }

  /// <summary>
  /// 测试 Visit - ListPattern 列表模式（带切片）
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_WithSlice()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    bool result = array is [1, 2, ..];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let result = Array.isArray(array) && array.length >= 2 && array[0] === 1 && array[1] === 2;
}", script);
  }

  /// <summary>
  /// 测试 VisitListPattern - 列表模式（带切片）（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_WithSlice_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    bool result = array is [1, 2, ..];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var listPatternOperation = (IListPatternOperation)isPatternOperation!.Pattern;
    var arg = new SenseArgument(PatternInput: new Identifier("array"));
    var node = walker.VisitListPattern(listPatternOperation, arg);
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("Array.isArray(array) && array.length >= 2 && array[0] === 1 && array[1] === 2", script);
  }

  [TestMethod]
  public void Visit_ListPattern_CachesInvocationInput()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] GetValues()
                    {
                        return [1, 2, 3];
                    }

                    bool result = GetValues() is [1, 2, 3];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    StringAssert.Contains(script, "let v$0;", StringComparison.Ordinal);
    AssertContainsCount(script, "= GetValues(),", 1);
    StringAssert.Contains(script, "let result = (v$0 = GetValues(), Array.isArray(v$0) && v$0.length === 3 && v$0[0] === 1 && v$0[1] === 2 && v$0[2] === 3);", StringComparison.Ordinal);
  }

  [TestMethod]
  public void Visit_ListPattern_DeclaredSymbol_AssignsAfterSuccessfulMatch()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    if (array is [1, 2, ..] values)
                    {
                        Console.WriteLine(values.Length);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let values;
  let array = [1, 2, 3];
  if (Array.isArray(array) && array.length >= 2 && array[0] === 1 && array[1] === 2 && (values = array, true)) {
    console.log(values.length);
  }
}", script);
  }

  [TestMethod]
  public void Visit_ListPattern_ListCarrier_UsesWhitelistIndexerHelper()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    List<int> list = [1, 2, 3];
                    bool result = list is [1, 2, 3];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    StringAssert.Contains(script, "let result = Array.isArray(list) && list.length === 3", StringComparison.Ordinal);
    AssertContainsCount(script, "_d389c31d59037b42(list, ", 3);
    AssertContainsCount(script, "list[0]", 0);
    AssertContainsCount(script, "list[1]", 0);
    AssertContainsCount(script, "list[2]", 0);
  }

  [TestMethod]
  public void Visit_ListPattern_ReadOnlyCollectionCarrier_UsesWhitelistIndexerHelper()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new System.Collections.ObjectModel.ReadOnlyCollection<int>(new List<int> { 1, 2, 3 });
                    bool result = list is [1, 2, 3];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    StringAssert.Contains(script, "let result = Array.isArray(list) && list.length === 3", StringComparison.Ordinal);
    AssertContainsCount(script, "_b8c9d0e1f2a3b4c5(list, ", 3);
    AssertContainsCount(script, "list[0]", 0);
    AssertContainsCount(script, "list[1]", 0);
    AssertContainsCount(script, "list[2]", 0);
  }

  [TestMethod]
  public void Visit_ListPattern_StringCarrier_UsesStringRuntimeIndexer()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string text = ""ab"";
                    bool result = text is ['a', 'b'];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    StringAssert.Contains(script, "let result = typeof text === \"string\" && text.length === 2", StringComparison.Ordinal);
    AssertContainsCount(script, "_5ad63706a889c294(text, ", 2);
    AssertContainsCount(script, "text[0]", 0);
    AssertContainsCount(script, "text[1]", 0);
  }

  [TestMethod]
  public void Visit_ListPattern_ListCarrier_WithSliceCapture_UsesSliceSymbol()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    List<int> list = [1, 2, 3, 4];
                    if (list is [var first, .. var rest, var last])
                    {
                        Console.WriteLine(first);
                        Console.WriteLine(rest.Count);
                        Console.WriteLine(last);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    StringAssert.Contains(script, "let first, rest, last;", StringComparison.Ordinal);
    StringAssert.Contains(script, "if (Array.isArray(list) && list.length >= 2 &&", StringComparison.Ordinal);
    StringAssert.Contains(script, "(first = _d389c31d59037b42(list, 0), true)", StringComparison.Ordinal);
    StringAssert.Contains(script, "(rest = list.slice(1, 1 + (list.length - 2)), true)", StringComparison.Ordinal);
    StringAssert.Contains(script, "(last = _d389c31d59037b42(list, list.length - 1), true)", StringComparison.Ordinal);
    AssertContainsCount(script, "list.slice(1, -1)", 0);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 切片模式
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    bool result = array is [..];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let result = Array.isArray(array) && array.length >= 0;
}", script);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 切片带变量捕获（解构赋值）
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_WithVariableCapture()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    if (array is [.. var rest])
                    {
                        Console.WriteLine(rest.Length);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);

    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let rest;
  let array = [1, 2, 3, 4, 5];
  if (Array.isArray(array) && array.length >= 0 && (rest = array.slice(0), true)) {
    console.log(rest.length);
  }
}", script);

  }

  /// <summary>
  /// 测试 VisitSlicePattern - 切片带变量捕获（直接调用）
  /// 验证切片模式中的声明模式会被正确处理，变量名被添加到上下文
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_WithVariableCapture_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    if (array is [.. var rest])
                    {
                        Console.WriteLine(rest.Length);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var ifOperation = GetOperationAt<IConditionalOperation>(block, 1);
    var isPatternOperation = (IIsPatternOperation)ifOperation.Condition;
    var listPatternOperation = (IListPatternOperation)isPatternOperation.Pattern;
    var slicePatternOperation = (ISlicePatternOperation)listPatternOperation.Patterns.First();
    // 注意：直接调用 VisitSlicePattern 无法知道切片位置
    // 实际的 slice 表达式构建在 VisitListPattern 中完成
    // 这里传递的 PatternInput 会直接传给子模式
    var sliceExpr = new CallExpression(
      new MemberExpression(new Identifier("array"), new Identifier("slice"), false, false),
      Acornima.Ast.NodeList.From<Expression>(new NumericLiteral(0, "0")),
      false
    );
    var arg = new SenseArgument(PatternInput: sliceExpr);
    var node = walker.VisitSlicePattern(slicePatternOperation, arg);
    var script = node?.ToECMAScript();

    // 验证生成的表达式（PatternInput 会被传给声明模式）
    Assert.AreEqual(@"rest=array.slice(0),true", script);
    Assert.IsTrue(arg.HasVarDeclarator);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 切片在列表开头
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_AtStart()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    bool result = array is [.., 4, 5];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let result = Array.isArray(array) && array.length >= 2 && array[array.length - 2] === 4 && array[array.length - 1] === 5;
}", script);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 切片在列表中间
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_InMiddle()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    bool result = array is [1, .., 5];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let result = Array.isArray(array) && array.length >= 2 && array[0] === 1 && array[array.length - 1] === 5;
}", script);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 切片在列表末尾（多元素前）
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_AtEnd()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    bool result = array is [1, 2, ..];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let result = Array.isArray(array) && array.length >= 2 && array[0] === 1 && array[1] === 2;
}", script);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 切片带前缀和后缀变量捕获
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_WithPrefixAndSuffix()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    if (array is [var first, .. var rest])
                    {
                        Console.WriteLine(first);
                        Console.WriteLine(rest.Length);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let first, rest;
  let array = [1, 2, 3, 4, 5];
  if (Array.isArray(array) && array.length >= 1 && (first = array[0], true) && (rest = array.slice(1), true)) {
    console.log(first);
    console.log(rest.length);
  }
}", script);

  }

  /// <summary>
  /// 测试 Visit - SlicePattern 切片带前缀、中间和后缀
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_ComplexDestructuring()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    if (array is [var first, .., var last])
                    {
                        Console.WriteLine(first);
                        Console.WriteLine(last);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let first, last;
  let array = [1, 2, 3, 4, 5];
  if (Array.isArray(array) && array.length >= 2 && (first = array[0], true) && (last = array[array.length - 1], true)) {
    console.log(first);
    console.log(last);
  }
}", script);

  }

  /// <summary>
  /// 测试 Visit - SlicePattern 切片与 switch 表达式结合
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_InSwitchExpression()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2];
                    string result = array switch
                    {
                        [..] => ""empty or any"",
                        _ => ""other""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2];
  let result = (() => {
    const v$0 = array;
    if (Array.isArray(v$0) && v$0.length >= 0)
      return ""empty or any"";
    return ""other"";
  })();
}", script);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 空数组匹配
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_EmptyArray()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [];
                    bool result = array is [..];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [];
  let result = Array.isArray(array) && array.length >= 0;
}", script);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 单元素数组匹配
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_SingleElementArray()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [42];
                    bool result = array is [..];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [42];
  let result = Array.isArray(array) && array.length >= 0;
}", script);
  }

  /// <summary>
  /// 测试 Visit - DeclarationPattern 声明模式
  /// </summary>
  [TestMethod]
  public void Visit_DeclarationPattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    if (obj is int value)
                    {
                        Console.WriteLine(value);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value;
  let obj = 42;
  if (typeof obj === ""number"" && (value = obj, true)) {
    console.log(value);
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitDeclarationPattern - 声明模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_DeclarationPattern_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    if (obj is int value)
                    {
                        Console.WriteLine(value);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var conditionalOp = GetOperationAt<IConditionalOperation>(block, 1);
    var isPatternOperation = conditionalOp.Condition as IIsPatternOperation;
    var declarationPatternOperation = (IDeclarationPatternOperation)isPatternOperation!.Pattern;
    var arg = new SenseArgument(PatternInput: new Identifier("obj"));
    var node = walker.VisitDeclarationPattern(declarationPatternOperation, arg);
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"typeof obj === ""number"" && (value = obj, true)", script);
  }

  [TestMethod]
  public void Visit_DeclarationPattern_CachesInvocationInput()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string GetText()
                    {
                        return ""hello"";
                    }

                    bool result = GetText() is string text;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    StringAssert.Contains(script, "let v$0, text;", StringComparison.Ordinal);
    AssertContainsCount(script, "= GetText(),", 1);
    StringAssert.Contains(script, "let result = (v$0 = GetText(), typeof v$0 === \"string\" && (text = v$0, true));", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试复杂模式匹配 - switch 表达式
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_SwitchExpression()
  {
    var block = GetBlockOperation(@"
    class TestClass
    {
      void TestMethod()
      {
        int value = 5;
        string result = Get5(value) switch
        {
          > 0 and < 10 => ""Small"",
          >= 10 => ""Large"",
          _ => ""Unknown""
        };
      }

      static int Get5(int x)
      {
        return x switch
        {
          > 0 and < 10 => 5,
          _ => 0
        };
      }	
    }
    ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let value = 5;
  let result = (() => {
    const v$0 = TestClass.Get5(value);
    if (v$0 > 0 && v$0 < 10)
      return ""Small"";
    if (v$0 >= 10)
      return ""Large"";
    return ""Unknown"";
  })();
}", script);

  }

  /// <summary>
  /// 测试复杂模式匹配 - 属性模式
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_PropertyPattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var person = new { Name = ""John"", Age = 30 };
                    string result = person switch
                    {
                        { Name: ""John"" } => ""Hello John"",
                        { Age: > 18 } => ""Adult"",
                        _ => ""Unknown""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(
@"{
  let person = { Name: ""John"", Age: 30 };
  let result = (() => {
    const v$0 = person;
    if (v$0 != null && ""Name"" in v$0 && v$0.Name === ""John"")
      return ""Hello John"";
    if (v$0 != null && ""Age"" in v$0 && v$0.Age > 18)
      return ""Adult"";
    return ""Unknown"";
  })();
}", script);

  }

  /// <summary>
  /// 测试复杂模式匹配 - 列表解构
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_ListDestructuring()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    if (array is [var first, var second, .. var rest])
                    {
                        Console.WriteLine(first);
                        Console.WriteLine(second);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let first, second, rest;
  let array = [1, 2, 3, 4, 5];
  if (Array.isArray(array) && array.length >= 2 && (first = array[0], true) && (second = array[1], true) && (rest = array.slice(2), true)) {
    console.log(first);
    console.log(second);
  }
}", script);

  }

  /// <summary>
  /// 测试复杂模式匹配 - 嵌套模式
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_NestedPattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var data = new { Inner = new { Value = 42 } };
                    bool result = data is { Inner: { Value: > 0 } };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertStringContainsJsNaming(script, "let data =", StringComparison.Ordinal);
    AssertContainsCount(script, "data.Inner", 1);
    AssertStringContainsJsNaming(script, "(v$0 = data.Inner, v$0 != null && \"Value\" in v$0 && v$0.Value > 0)", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试复杂模式匹配 - 类型模式与属性模式组合
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_TypeAndProperty()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    bool result = obj is string { Length: > 0 };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let obj = ""hello"";
  let result = typeof obj === ""string"" && obj != null && ""length"" in obj && obj.length > 0;
}", script);

  }

  /// <summary>
  /// 测试复杂模式匹配 - 取反模式与关系模式组合
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_NegatedAndRelational()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is not < 0;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = !(value < 0);
}", script);
  }

  /// <summary>
  /// 测试复杂模式匹配 - 多个条件组合
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_MultipleConditions()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is > 0 and < 10 and not 5;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = value > 0 && value < 10 && !(value === 5);
}", script);
  }

  /// <summary>
  /// 测试 VisitConstantPattern - 常量模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_ConstantPattern_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    bool result = obj is 42;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var constantPatternOperation = (IConstantPatternOperation)isPatternOperation!.Pattern;
    var arg = new SenseArgument(PatternInput: new Identifier("obj"));
    var node = walker.VisitConstantPattern(constantPatternOperation, arg);
    var script = node?.ToECMAScript();

    Assert.AreEqual("obj===42", script);
  }

  /// <summary>
  /// 测试 VisitConstantPattern - 字符串常量模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_ConstantPattern_String_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    bool result = obj is ""hello"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var constantPatternOperation = (IConstantPatternOperation)isPatternOperation!.Pattern;
    var arg = new SenseArgument(PatternInput: new Identifier("obj"));
    var node = walker.VisitConstantPattern(constantPatternOperation, arg);
    var script = node?.ToECMAScript();

    Assert.AreEqual(@"obj===""hello""", script);
  }

  /// <summary>
  /// 测试 VisitPatternCaseClause - 模式 case 子句（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_PatternCaseClause_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int obj = 42;
                    switch (obj)
                    {
                        case var x when x>10:
                            break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var switchOperation = GetOperationAt<ISwitchOperation>(block, 1);
    var patternCaseClause = (IPatternCaseClauseOperation)switchOperation.Cases.First()!.Clauses.First()!;
    // 直接调用需要提供 PatternInput
    var arg = new SenseArgument(PatternInput: new Identifier("v$0"));
    var node = walker.VisitPatternCaseClause(patternCaseClause, arg);
    var script = node?.ToECMAScript();

    Assert.AreEqual("(x=v$0,true)&&x>10", script);
  }

  /// <summary>
  /// 测试 Visit - IsType DateTime 类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_DateTime()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = DateTime.Now;
                    bool result = obj is DateTime;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let obj = _ee9dd166a34a2fa5();
  let result = obj instanceof JDateTime;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());

  }

  /// <summary>
  /// 测试 Visit - IsType Task&lt;T&gt; 类型检查（映射为 Promise）
  /// </summary>
  [TestMethod]
  public void Visit_IsType_TaskOfT()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = System.Threading.Tasks.Task.FromResult(42);
                    bool result = obj is System.Threading.Tasks.Task<int>;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let obj = Promise.resolve(42);
  let result = obj instanceof Promise;
}", script);
  }

  /// <summary>
  /// 测试共享 Error 运行时别名的 CLR 异常类型不会在 is-type 中静默退化成 instanceof Error
  /// </summary>
  [TestMethod]
  public void Visit_IsType_InvalidOperationException_SharedRuntimeAlias_Throws()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(object obj)
                {
                    bool result = obj is InvalidOperationException;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    Assert.Throws<OperationTransformationException>(() =>
    {
      _ = walker.Visit(block, new());
    });
  }

  [TestMethod]
  public void Visit_IsType_ConcreteObjectAliasWithoutCarrier_ThrowsActionableDiagnostic()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(object obj)
                {
                    bool result = obj is EqualityComparer<int>;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));

    StringAssert.Contains(exception.Message, "System.Collections.Generic.EqualityComparer<T>");
    StringAssert.Contains(exception.Message, "without an inferred Jazor.CLR runtime carrier");
  }

  [TestMethod]
  public void Visit_IsType_CalendarAndGregorianCalendar_UseSharedInferredCarrier()
  {
    var block = GetBlockOperation(@"
            using System.Globalization;

            class TestClass
            {
                void TestMethod(object obj)
                {
                    bool calendar = obj is Calendar;
                    bool gregorian = obj is GregorianCalendar;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let calendar = obj instanceof JGregorianCalendar;
  let gregorian = obj instanceof JGregorianCalendar;
}", script);
  }

  /// <summary>
  /// 测试共享 Error 运行时别名的声明模式不会静默退化成 instanceof Error
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_InvalidOperationExceptionDeclaration_SharedRuntimeAlias_Throws()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(object obj)
                {
                    bool result = obj is InvalidOperationException ex && ex.Message.Length >= 0;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    Assert.Throws<OperationTransformationException>(() =>
    {
      _ = walker.Visit(block, new());
    });
  }

  /// <summary>
  /// 测试未标记且不在白名单的外部类型不会在 is-type 中静默退化成 instanceof
  /// </summary>
  [TestMethod]
  public void Visit_IsType_UnsupportedExternalType_Throws()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(object obj)
                {
                    bool result = obj is Random;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    Assert.Throws<OperationTransformationException>(() =>
    {
      _ = walker.Visit(block, new());
    });
  }

  /// <summary>
  /// 测试 Visit - IsType long/Int64 类型检查 (BigInt)
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Long()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42L;
                    bool result = obj is long;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = 42n;
  let result = typeof obj === ""bigint"";
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsType 数组类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Array()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new int[] { 1, 2, 3 };
                    bool result = obj is int[];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = [1, 2, 3];
  let result = Array.isArray(obj);
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsType Dictionary/Map 类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Dictionary()
  {
    var block = GetBlockOperation(@"
            using System.Collections.Generic;
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new Dictionary<string, int>();
                    bool result = obj is IDictionary<string, int>;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = new Map;
  let result = obj instanceof Map;
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsType 自定义 Class 类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_CustomClass()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new TestClass();
                    bool result = obj is TestClass;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = new TestClass;
  let result = obj instanceof TestClass;
}", script);
  }

  /// <summary>
  /// 测试 Visit - RecursivePattern 元组模式
  /// </summary>
  [TestMethod]
  public void Visit_RecursivePattern_Tuple()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (1, ""hello"");
                    bool result = tuple is (int x, string s);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertStringContainsJsNaming(script, "let v$0, x, v$1, s;", StringComparison.Ordinal);
    AssertContainsCount(script, "tuple.Item1", 1);
    AssertContainsCount(script, "tuple.Item2", 1);
    AssertStringContainsJsNaming(script, "(v$0 = tuple.Item1, typeof v$0 === \"number\" && (x = v$0, true))", StringComparison.Ordinal);
    AssertStringContainsJsNaming(script, "(v$1 = tuple.Item2, typeof v$1 === \"string\" && (s = v$1, true))", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - RecursivePattern 元组模式带条件
  /// </summary>
  [TestMethod]
  public void Visit_RecursivePattern_TupleWithCondition()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (1, ""hello"");
                    bool result = tuple is (int x, string s) && x > 0;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertStringContainsJsNaming(script, "let v$0, x, v$1, s;", StringComparison.Ordinal);
    AssertContainsCount(script, "tuple.Item1", 1);
    AssertContainsCount(script, "tuple.Item2", 1);
    AssertStringContainsJsNaming(script, "(v$0 = tuple.Item1, typeof v$0 === \"number\" && (x = v$0, true))", StringComparison.Ordinal);
    AssertStringContainsJsNaming(script, "(v$1 = tuple.Item2, typeof v$1 === \"string\" && (s = v$1, true))", StringComparison.Ordinal);
    AssertStringContainsJsNaming(script, "&& x > 0;", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - ListPattern 空列表模式
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_Empty()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [];
                    bool result = array is [];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [];
  let result = Array.isArray(array) && array.length === 0;
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsType 可空类型模式
  /// </summary>
  [TestMethod]
  public void Visit_IsType_NullableInt()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int? value = null;
                    bool result = value is int;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = null;
  let result = typeof value === ""number"";
}", script);
  }

  /// <summary>
  /// 测试 Visit - DeclarationPattern 可空类型带变量声明
  /// </summary>
  [TestMethod]
  public void Visit_DeclarationPattern_Nullable()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int? value = 42;
                    if (value is int v)
                    {
                        Console.WriteLine(v);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let v;
  let value = 42;
  if (typeof value === ""number"" && (v = value, true)) {
    console.log(v);
  }
}", script);
  }

  /// <summary>
  /// 测试 Visit - BinaryPattern 嵌套声明模式
  /// </summary>
  [TestMethod]
  public void Visit_BinaryPattern_NestedDeclaration()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    bool result = obj is int x and int y;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x, y;
  let obj = 42;
  let result = typeof obj === ""number"" && (x = obj, true) && (typeof obj === ""number"" && (y = obj, true));
}", script);
  }

  /// <summary>
  /// 测试 Visit - ListPattern 嵌套声明模式
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_NestedDeclaration()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    if (array is [var a, var b, var c])
                    {
                        Console.WriteLine(a + b + c);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a, b, c;
  let array = [1, 2, 3];
  if (Array.isArray(array) && array.length === 3 && (a = array[0], true) && (b = array[1], true) && (c = array[2], true)) {
    console.log(a + b + c);
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitSwitchExpressionArm - 常量模式 arm（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_SwitchExpressionArm_Constant_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    string result = value switch
                    {
                        1 => ""one"",
                        2 => ""two"",
                        _ => ""default""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var switchExpressionOperation = declarator.Initializer!.Value as ISwitchExpressionOperation;
    if (switchExpressionOperation is null)
      throw new InvalidOperationException("switchExpressionOperation is null");
    var switchCaseArm = switchExpressionOperation.Arms.First();
    // 需要提供 PatternInput（模拟 switch expression 的输入变量）
    var arg = new SenseArgument(PatternInput: new Identifier("v$0"));
    var node = walker.VisitSwitchExpressionArm(switchCaseArm, arg);
    var script = node?.ToECMAScript();

    Assert.AreEqual(@"if(v$0===1)return""one"";", script);
  }

  /// <summary>
  /// 测试 VisitSwitchExpressionArm - 模式 arm 带 when 子句（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_SwitchExpressionArm_WithGuard_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    string result = value switch
                    {
                        var x when x > 0 => ""positive"",
                        _ => ""default""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var switchExpressionOperation = declarator.Initializer!.Value as ISwitchExpressionOperation
      ?? throw new InvalidOperationException("switchExpressionOperation is null");
    var switchCaseArm = switchExpressionOperation.Arms.First();
    var arg = new SenseArgument(PatternInput: new Identifier("v$0"));
    var node = walker.VisitSwitchExpressionArm(switchCaseArm, arg);
    var script = node?.ToECMAScript();

    Assert.AreEqual(@"if((x=v$0,true)&&x>0)return""positive"";", script);
  }

  /// <summary>
  /// 测试 VisitSwitchExpressionArm - 丢弃模式 arm（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_SwitchExpressionArm_Discard_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    string result = value switch
                    {
                        _ => ""default""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var switchExpressionOperation = declarator.Initializer!.Value as ISwitchExpressionOperation
      ?? throw new InvalidOperationException("switchExpressionOperation is null");
    var switchCaseArm = switchExpressionOperation.Arms.First();
    var node = walker.VisitSwitchExpressionArm(switchCaseArm, new());
    var script = node?.ToECMAScript();

    Assert.AreEqual(@"return""default""", script);
  }

  /// <summary>
  /// 测试 VisitPatternCaseClause - 常量模式 case（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_SingleValueCaseClause_Constant_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    switch (value)
                    {
                        case 1:
                            break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var switchOperation = GetOperationAt<ISwitchOperation>(block, 1);
    var patternCaseClause = (ISingleValueCaseClauseOperation)switchOperation.Cases.First()!.Clauses.First()!;
    var node = walker.VisitSingleValueCaseClause(patternCaseClause, new());
    var script = node?.ToECMAScript();

    Assert.AreEqual("1", script);
  }

  /// <summary>
  /// 测试 VisitPatternCaseClause - 关系模式 case（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_PatternCaseClause_Relational_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    switch (value)
                    {
                        case > 0:
                            break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var switchOperation = GetOperationAt<ISwitchOperation>(block, 1);
    var patternCaseClause = (IPatternCaseClauseOperation)switchOperation.Cases.First()!.Clauses.First()!;
    // 直接调用需要提供 PatternInput
    var arg = new SenseArgument(PatternInput: new Identifier("v$0"));
    var node = walker.VisitPatternCaseClause(patternCaseClause, arg);
    var script = node?.ToECMAScript();

    Assert.AreEqual("v$0>0", script);
  }

  /// <summary>
  /// 测试 VisitPatternCaseClause - 复杂模式带 when 子句（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_PatternCaseClause_ComplexWithGuard_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string value = ""hello"";
                    switch (value)
                    {
                        case string s when s.Length > 0:
                            break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var switchOperation = GetOperationAt<ISwitchOperation>(block, 1);
    var patternCaseClause = (IPatternCaseClauseOperation)switchOperation.Cases.First()!.Clauses.First()!;
    // 直接调用需要提供 PatternInput
    var arg = new SenseArgument(PatternInput: new Identifier("v$0"));
    var node = walker.VisitPatternCaseClause(patternCaseClause, arg);
    var script = node?.ToECMAScript();

    Assert.AreEqual(@"typeof v$0===""string""&&(s=v$0,true)&&s.length>0", script);
  }

  /// <summary>
  /// 测试 Visit - IsType DateOnly 类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_DateOnly()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new DateOnly();
                    bool result = obj is DateOnly;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = _5f8053a9657a0844();
  let result = obj instanceof JDateOnly;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
  }

  /// <summary>
  /// 测试 Visit - IsType DateTimeOffset 类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_DateTimeOffset()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new DateTimeOffset();
                    bool result = obj is DateTimeOffset;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let obj = _12b4f3f1dc14bea9();
  let result = obj instanceof JDateTimeOffset;
}", script);
  }

  [TestMethod]
  public void Visit_IsType_DateTimeOffset_CachesInvocationInput()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    DateTimeOffset GetValue()
                    {
                        return new DateTimeOffset();
                    }

                    bool result = GetValue() is DateTimeOffset;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    StringAssert.Contains(script, "let v$0;", StringComparison.Ordinal);
    AssertContainsCount(script, "= GetValue(),", 1);
    StringAssert.Contains(script, "let result = (v$0 = GetValue(), v$0 instanceof JDateTimeOffset);", StringComparison.Ordinal);
  }

  [TestMethod]
  public void Visit_IsType_Queue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new System.Collections.Generic.Queue<int>();
                    bool result = obj is System.Collections.Generic.Queue<int>;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let obj = _ea05a56d08fbd4f9();
  let result = obj instanceof JQueue;
}", script);
  }

  [TestMethod]
  public void Visit_IsType_Stack()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new System.Collections.Generic.Stack<int>();
                    bool result = obj is System.Collections.Generic.Stack<int>;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let obj = _7d15fcc03d17599b();
  let result = obj instanceof JStack;
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsType TimeSpan 类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_TimeSpan()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new TimeSpan();
                    bool result = obj is TimeSpan;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = _5af0f6ad850e6702();
  let result = obj instanceof JTimeSpan;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
  }

  /// <summary>
  /// 测试 Visit - IsType Char 类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Char()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""A"";
                    bool result = obj is char;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = ""A"";
  let result = typeof obj === ""string"";
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsType Decimal 类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Decimal()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 123.45m;
                    bool result = obj is decimal;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = 123.45;
  let result = typeof obj === ""number"";
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsType TimeOnly 类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_TimeOnly()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new TimeOnly();
                    bool result = obj is TimeOnly;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let obj = _9f78f92d0753f4cf();
  let result = obj instanceof JTimeOnly;
}".Replace("\r\n", "\n"),
        script?.Replace("\r\n", "\n"));

  }

  /// <summary>
  /// 测试 Visit - IsType IEnumerable（非 IDictionary）类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_IEnumerable()
  {
    var block = GetBlockOperation(@"
            using System.Collections.Generic;
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new List<int>();
                    bool result = obj is IEnumerable<int>;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let obj = [];
  let result = Array.isArray(obj);
}", script);

  }

  /// <summary>
  /// 测试 Visit - IsType 匿名类型模式检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_AnonymousType()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Name = ""Test"", Value = 42 };
                    bool result = obj is { Name: ""Test"" };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(
@"{
  let obj = { Name: ""Test"", Value: 42 };
  let result = obj != null && ""Name"" in obj && obj.Name === ""Test"";
}", script);

  }

  /// <summary>
  /// 测试 Visit - SlicePattern 切片带常量模式
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_SliceWithConstantPattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    if (array is [.., var last] && last > 0)
                    {
                        Console.WriteLine(last);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let last;
  let array = [1, 2, 3];
  if (Array.isArray(array) && array.length >= 1 && (last = array[array.length - 1], true) && last > 0) {
    console.log(last);
  }
}", script);
  }

  /// <summary>
  /// 测试 Visit - ListPattern 嵌套列表模式
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_NestedList()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var nested = new[] { new[] { 1, 2 }, new[] { 3, 4 } };
                    bool result = nested is [[1, 2], [3, 4]];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertContainsCount(script, "nested[0]", 1);
    AssertContainsCount(script, "nested[1]", 1);
    StringAssert.Contains(script, "(v$0 = nested[0], Array.isArray(v$0) && v$0.length === 2 && v$0[0] === 1 && v$0[1] === 2)", StringComparison.Ordinal);
    StringAssert.Contains(script, "(v$1 = nested[1], Array.isArray(v$1) && v$1.length === 2 && v$1[0] === 3 && v$1[1] === 4)", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - ListPattern 嵌套列表带切片
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_NestedListWithSlice()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var nested = new[] { new[] { 1, 2, 3 }, new[] { 4, 5, 6 } };
                    bool result = nested is [[1, ..], [4, ..]];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertContainsCount(script, "nested[0]", 1);
    AssertContainsCount(script, "nested[1]", 1);
    StringAssert.Contains(script, "(v$0 = nested[0], Array.isArray(v$0) && v$0.length >= 1 && v$0[0] === 1)", StringComparison.Ordinal);
    StringAssert.Contains(script, "(v$1 = nested[1], Array.isArray(v$1) && v$1.length >= 1 && v$1[0] === 4)", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - RecursivePattern 空属性模式
  /// </summary>
  [TestMethod]
  public void Visit_RecursivePattern_EmptyPropertyPattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Name = ""Test"" };
                    bool result = obj is { };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let obj = { Name: ""Test"" };
  let result = obj != null;
}", script);
  }

  [TestMethod]
  public void Visit_RecursivePattern_DeclaredSymbol_AssignsAfterSuccessfulMatch()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    if (obj is int { } value)
                    {
                        Console.WriteLine(value);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let value;
  let obj = 42;
  if (typeof obj === ""number"" && (value = obj, true)) {
    console.log(value);
  }
}", script);
  }

  [TestMethod]
  public void Visit_RecursivePattern_DeclaredSymbol_CachesInvocationInput()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object GetValue()
                    {
                        return 42;
                    }

                    bool result = GetValue() is int { } value;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    StringAssert.Contains(script, "let v$0, value;", StringComparison.Ordinal);
    AssertContainsCount(script, "= GetValue(),", 1);
    StringAssert.Contains(script, "let result = (v$0 = GetValue(), typeof v$0 === \"number\" && (value = v$0, true));", StringComparison.Ordinal);
  }

  [TestMethod]
  public void Visit_RecursivePattern_EmptyPropertyPatternWithDeclaration_AssignsAfterNonNullCheck()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""ready"";
                    if (obj is { } value)
                    {
                        Console.WriteLine(value);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let value;
  let obj = ""ready"";
  if (obj != null && (value = obj, true)) {
    console.log(value);
  }
}", script);
  }

  /// <summary>
  /// 测试 Visit - ListPattern 单元素列表
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_SingleElement()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [42];
                    bool result = array is [42];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [42];
  let result = Array.isArray(array) && array.length === 1 && array[0] === 42;
}", script);
  }

  /// <summary>
  /// 测试 Visit - BinaryPattern 复杂嵌套
  /// </summary>
  [TestMethod]
  public void Visit_BinaryPattern_ComplexNesting()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is (> 0 and < 10) or (>= 100 and <= 200);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = value > 0 && value < 10 || value >= 100 && value <= 200;
}", script);
  }

  /// <summary>
  /// 测试 Visit - RelationalPattern 相等和不等
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_Equality()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is 5;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = value === 5;
}", script);
  }

  /// <summary>
  /// 测试 Visit - RelationalPattern 不等
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_Inequality()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is not 0;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = !(value === 0);
}", script);
  }

  /// <summary>
  /// 测试 Visit - PropertySubpattern 嵌套属性访问
  /// </summary>
  [TestMethod]
  public void Visit_PropertySubpattern_NestedProperty()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var data = new { Inner = new { Value = 42 } };
                    bool result = data is { Inner: { Value: 42 } };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertContainsCount(script, "data.Inner", 1);
    AssertStringContainsJsNaming(script, "\"Inner\" in data && (v$0 = data.Inner, v$0 != null && \"Value\" in v$0 && v$0.Value === 42)", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - Switch 表达式复杂模式组合
  /// </summary>
  [TestMethod]
  public void Visit_SwitchExpression_ComplexPatternCombination()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { X = 1, Y = 2 };
                    string result = obj switch
                    {
                        { X: 1, Y: 2 } => ""Point (1,2)"",
                        { X: var x } when x > 0 => ""Positive X"",
                        _ => ""Other""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(
@"{
  let obj = { X: 1, Y: 2 };
  let result = (() => {
    let x;
    const v$0 = obj;
    if (v$0 != null && ""X"" in v$0 && v$0.X === 1 && ""Y"" in v$0 && v$0.Y === 2)
      return ""Point (1,2)"";
    if (v$0 != null && ""X"" in v$0 && (x = v$0.X, true) && x > 0)
      return ""Positive X"";
    return ""Other"";
  })();
}", script);

  }

  #region 边界情况测试 - 特殊数值

  /// <summary>
  /// 测试 Visit - RelationalPattern NaN 特殊值
  /// NaN 与任何关系比较都返回 false，包括其自身
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_NaN()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double value = double.NaN;
                    bool result = value is > 0;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = NaN;
  let result = value > 0;
}", script);
  }

  /// <summary>
  /// 测试 Visit - RelationalPattern 正无穷大
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_PositiveInfinity()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double value = double.PositiveInfinity;
                    bool result = value is > 0;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = Infinity;
  let result = value > 0;
}", script);
  }

  /// <summary>
  /// 测试 Visit - RelationalPattern 负无穷大
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_NegativeInfinity()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double value = double.NegativeInfinity;
                    bool result = value is < 0;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = -Infinity;
  let result = value < 0;
}", script);
  }

  /// <summary>
  /// 测试 Visit - RelationalPattern 最大值边界
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_MaxValue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = int.MaxValue;
                    bool result = value is > 0 and < int.MaxValue;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 2147483647;
  let result = value > 0 && value < 2147483647;
}", script);
  }

  /// <summary>
  /// 测试 Visit - RelationalPattern 最小值边界
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_MinValue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = int.MinValue;
                    bool result = value is < 0 and > int.MinValue;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = -2147483648;
  let result = value < 0 && value > -2147483648;
}", script);
  }

  #endregion

  #region 字符串模式边界测试

  /// <summary>
  /// 测试 Visit - IsPattern 空字符串
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_EmptyString()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string value = """";
                    bool result = value is """";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = """";
  let result = value === """";
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsPattern 逐字字符串（包含特殊字符）
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_VerbatimString()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string value = ""line1\\nline2"";
                    bool result = value is ""line1\\nline2"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = ""line1\\nline2"";
  let result = value === ""line1\\nline2"";
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsPattern 多行字符串
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_MultiLineString()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string value = @""line1
line2"";
                    bool result = value is @""line1
line2"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = ""line1\nline2"";
  let result = value === ""line1\nline2"";
}", script);
  }

  /// <summary>
  /// 测试 Visit - PropertySubpattern 字符串长度属性
  /// </summary>
  [TestMethod]
  public void Visit_PropertySubpattern_StringLength()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string value = ""hello"";
                    bool result = value is { Length: > 0 and < 10 };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertContainsCount(script, "value.Length", 1);
    StringAssert.Contains(script, "\"length\" in value && (v$0 = value.length, v$0 > 0 && v$0 < 10)", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - PropertySubpattern 空字符串检查
  /// </summary>
  [TestMethod]
  public void Visit_PropertySubpattern_EmptyStringCheck()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string value = """";
                    bool result = value is { Length: 0 };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = """";
  let result = typeof value === ""string"" && value != null && ""length"" in value && value.length === 0;
}", script);

  }

  #endregion

  #region 泛型类型模式测试

  /// <summary>
  /// 测试 Visit - TypePattern 泛型类型 List
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_GenericList()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new List<int>();
                    bool result = obj is List<int>;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let obj = [];
  let result = Array.isArray(obj);
}", script);

  }

  /// <summary>
  /// 测试 Visit - TypePattern 泛型字典类型
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_GenericDictionary()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new Dictionary<string, int>();
                    bool result = obj is Dictionary<string, int>;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = new Map;
  let result = obj instanceof Map;
}", script);
  }

  /// <summary>
  /// 测试 Visit - TypePattern 多接口组合
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_MultipleInterfaces()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new List<int>();
                    bool result = obj is IList<int> or ICollection<int>;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = [];
  let result = Array.isArray(obj) || Array.isArray(obj);
}", script);
  }

  /// <summary>
  /// 测试 Visit - TypePattern 泛型接口在可静态证明场景下直接求值为 true
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_GenericInterface_FoldsToTrue_WhenStaticallyProvable()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""test"";
                    bool result = obj is IComparable<string>;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = ""test"";
  let result = true;
}", script);
  }

  /// <summary>
  /// 测试 Visit - TypePattern 非泛型接口在可静态证明场景下直接求值为 true
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_IComparableNonGeneric_FoldsToTrue_WhenStaticallyProvable()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""test"";
                    bool result = obj is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = ""test"";
  let result = true;
}", script);
  }

  /// <summary>
  /// 测试 Visit - TypePattern 接口在可静态证明不匹配场景下直接求值为 false
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_FoldsToFalse_WhenStaticallyProvable()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""test"";
                    bool result = obj is System.Collections.IEqualityComparer;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = ""test"";
  let result = false;
}", script);
  }

  /// <summary>
  /// 测试 Visit - TypePattern IEqualityComparer 接口在可静态证明“仅与 null 相关”时降级为非空判断
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_IEqualityComparer_FoldsToNonNullCheck()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = System.Collections.Generic.EqualityComparer<int>.Default;
                    bool result = obj is System.Collections.Generic.IEqualityComparer<int>;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = globalThis.__jazorEqualityComparerDefault ??= {};
  let result = obj != null;
}", script);
  }

  /// <summary>
  /// 测试 Visit - DeclarationPattern IEqualityComparer 接口在可静态证明“仅与 null 相关”时降级为非空判断
  /// </summary>
  [TestMethod]
  public void Visit_DeclarationPattern_IEqualityComparer_FoldsToNonNullCheck()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = System.Collections.Generic.EqualityComparer<int>.Default;
                    bool result = obj is System.Collections.Generic.IEqualityComparer<int> comparer && comparer.Equals(1, 1);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let comparer;
  let obj = globalThis.__jazorEqualityComparerDefault ??= {};
  let result = obj != null && (comparer = obj, true) && _dae184550b995be1(comparer, 1, 1);
}", script);
  }

  /// <summary>
  /// 测试 Visit - TypePattern 非泛型 IEqualityComparer 在可静态证明“仅与 null 相关”时降级为非空判断
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_IEqualityComparerNonGeneric_FoldsToNonNullCheck()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = System.Collections.Generic.EqualityComparer<int>.Default;
                    bool result = obj is System.Collections.IEqualityComparer;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = globalThis.__jazorEqualityComparerDefault ??= {};
  let result = obj != null;
}", script);
  }

  /// <summary>
  /// 测试 Visit - DeclarationPattern 非泛型 IEqualityComparer 在可静态证明“仅与 null 相关”时降级为非空判断
  /// </summary>
  [TestMethod]
  public void Visit_DeclarationPattern_IEqualityComparerNonGeneric_FoldsToNonNullCheck()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = System.Collections.Generic.EqualityComparer<int>.Default;
                    bool result = obj is System.Collections.IEqualityComparer comparer && comparer.Equals(1, 1);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let comparer;
  let obj = globalThis.__jazorEqualityComparerDefault ??= {};
  let result = obj != null && (comparer = obj, true) && _eb0a1792ad8b44b7(comparer, 1, 1);
}", script);
  }

  /// <summary>
  /// 测试 Visit - Interface TypePattern 在方法返回可赋值静态类型时降级为非空判断
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_FromAssignableMethodReturn_FoldsToNonNullCheck()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                string? GetValue() => null;

                void TestMethod()
                {
                    object obj = GetValue();
                    bool result = obj is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let obj = this.GetValue();
  let result = obj != null;
}", script);
  }

  [TestMethod]
  public void Visit_TypePattern_Interface_TypeParameterConstraint_FoldsToNonNullCheck()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod<T>(T value)
                    where T : IComparable
                {
                    bool result = value is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let result = value != null;
}", script);
  }

  [TestMethod]
  public void Visit_TypePattern_Interface_StructTypeParameterConstraint_FoldsToTrue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod<T>(T value)
                    where T : struct, IComparable
                {
                    bool result = value is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let result = true;
}", script);
  }

  [TestMethod]
  public void Visit_TypePattern_Interface_DerivedInterfaceConstraint_FoldsToNonNullCheck()
  {
    var block = GetBlockOperation(@"
            interface IComparableContract : IComparable
            {
            }

            class TestClass
            {
                void TestMethod<T>(T value)
                    where T : IComparableContract
                {
                    bool result = value is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let result = value != null;
}", script);
  }

  [TestMethod]
  public void Visit_TypePattern_Interface_ChainedTypeParameterConstraint_FoldsToNonNullCheck()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod<T, U>(T value)
                    where T : U
                    where U : IComparable
                {
                    bool result = value is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let result = value != null;
}", script);
  }

  [TestMethod]
  public void Visit_TypePattern_Interface_UnrelatedTypeParameterConstraint_ThrowsUnsupported()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod<T>(T value)
                    where T : IDisposable
                {
                    bool result = value is IComparable;
                }
            }
            ");

    var exception = Assert.Throws<OperationTransformationException>(() =>
      new SemanticWalker(true).Visit(block, new SenseArgument()));

    StringAssert.Contains(exception.Message, "source static type 'T'");
    StringAssert.Contains(exception.Message, "System.IComparable");
  }

  [TestMethod]
  public void Visit_TypePattern_RuntimeTypeParameter_ThrowsUnsupported()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod<T>(object value)
                {
                    bool result = value is T;
                }
            }
            ");

    var exception = Assert.Throws<OperationTransformationException>(() =>
      new SemanticWalker(true).Visit(block, new SenseArgument()));

    StringAssert.Contains(exception.Message, "Target='T'", StringComparison.Ordinal);
    StringAssert.Contains(exception.Message, "Mapper='Unknown'", StringComparison.Ordinal);
  }

  [TestMethod]
  public void Visit_TypePattern_Interface_DirectInterfaceParameter_FoldsToNonNullCheck()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(IComparable? value)
                {
                    bool result = value is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let result = value != null;
}", script);
  }

  /// <summary>
  /// 测试 Visit - Interface TypePattern 在不可证明场景仍保持显式不支持
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_FromUnknownObject_ThrowsUnsupported()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                object GetValue() => new object();

                void TestMethod()
                {
                    object obj = GetValue();
                    bool result = obj is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var ex = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
    Assert.IsNotNull(ex);
    StringAssert.Contains(ex.Message, "source static type 'object'", StringComparison.Ordinal);
    StringAssert.Contains(ex.Message, "System.IComparable", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - Interface TypePattern 在局部变量发生重赋值时不做静态折叠
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_WithLocalReassignment_ThrowsUnsupported()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(bool pick)
                {
                    object obj = ""a"";
                    if (pick)
                    {
                        obj = 1;
                    }

                    bool result = obj is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    Assert.Throws<OperationTransformationException>(
      () => walker.Visit(block, new()),
      "Unsupported type in is-type operation.");
  }

  /// <summary>
  /// 测试 Visit - Interface TypePattern 折叠为 true 时仍保留被测表达式单次求值
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_FoldTrue_PreservesSingleEvaluation()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                sealed class ComparableValue : IComparable
                {
                    public int CompareTo(object? obj) => 0;
                }

                void TestMethod()
                {
                    bool result = new ComparableValue() is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let result = (v$0 = new ComparableValue, true);", StringComparison.Ordinal);
    AssertContainsCount(script, "new ComparableValue", 1);
  }

  /// <summary>
  /// 测试 Visit - Interface TypePattern 折叠为 false 时仍保留被测表达式单次求值
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_FoldFalse_PreservesSingleEvaluation()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                sealed class NonComparable
                {
                }

                void TestMethod()
                {
                    bool result = new NonComparable() is System.Collections.IEqualityComparer;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let result = (v$0 = new NonComparable, false);", StringComparison.Ordinal);
    AssertContainsCount(script, "new NonComparable", 1);
  }

  [TestMethod]
  public void Visit_TypePattern_Interface_AnonymousObject_FoldsToFalse()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object value = new { Count = 1 };
                    bool result = value is IComparable;
                }
            }
            ");

    var script = new SemanticWalker(true).Visit(block, new())?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let value = { Count: 1 };
  let result = false;
}", script);
  }

  [TestMethod]
  public void Visit_TypePattern_Interface_ArrayCreation_FoldsToFalse()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object value = new int[3];
                    bool result = value is IComparable;
                }
            }
            ");

    var script = new SemanticWalker(true).Visit(block, new())?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let value = new Array(3);
  let result = false;
}", script);
  }

  [TestMethod]
  public void Visit_TypePattern_Interface_ExplicitObjectConversion_PreservesRuntimeType()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    bool result = (object)""value"" is IComparable;
                }
            }
            ");

    var script = new SemanticWalker(true).Visit(block, new())?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let result = true;
}", script);
  }

  /// <summary>
  /// 测试 Visit - Interface TypePattern 对 null 字面量折叠为 false
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_NullLiteral_FoldsToFalse()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = null;
                    bool result = obj is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let obj = null;
  let result = false;
}", script);
  }

  /// <summary>
  /// 测试 Visit - Interface TypePattern 对 default(object) 折叠为 false
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_DefaultObject_FoldsToFalse()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = default(object);
                    bool result = obj is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let obj = null;
  let result = false;
}", script);
  }

  /// <summary>
  /// 测试 Visit - Interface TypePattern 对 default(interface) 折叠为 false
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_DefaultInterface_FoldsToFalse()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = default(System.Collections.IEqualityComparer);
                    bool result = obj is System.Collections.IEqualityComparer;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let obj = null;
  let result = false;
}", script);
  }

  /// <summary>
  /// 测试 Visit - Interface TypePattern 对 default(int) 折叠为 true
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_DefaultInt_FoldsToTrue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = default;
                    bool result = value is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let value = 0;", StringComparison.Ordinal);
    StringAssert.Contains(script, "let result = true;", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - Interface TypePattern 对 default(int?) 折叠为 false
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_DefaultNullableInt_FoldsToFalse()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int? value = default;
                    bool result = value is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let value = null;
  let result = false;
}", script);
  }

  /// <summary>
  /// 测试 Visit - Interface TypePattern 对局部别名链可证明场景折叠为 true
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_LocalAliasChain_FoldsToTrue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object a = ""x"";
                    object b = a;
                    object c = b;
                    bool result = c is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let result = true;", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - Interface TypePattern 对局部别名链不可证明场景保持不支持
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_LocalAliasChain_FromUnknownObject_ThrowsUnsupported()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                object GetValue() => new object();

                void TestMethod()
                {
                    object a = GetValue();
                    object b = a;
                    object c = b;
                    bool result = c is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    Assert.Throws<OperationTransformationException>(
      () => walker.Visit(block, new()),
      "Unsupported type in is-type operation.");
  }

  /// <summary>
  /// 测试 Visit - Interface TypePattern 在 out 参数写入后不做静态折叠
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_WithOutWrite_ThrowsUnsupported()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void Rewrite(out object value)
                {
                    value = new object();
                }

                void TestMethod()
                {
                    object obj = ""x"";
                    Rewrite(out obj);
                    bool result = obj is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    Assert.Throws<OperationTransformationException>(
      () => walker.Visit(block, new()),
      "Unsupported type in is-type operation.");
  }

  /// <summary>
  /// 测试 Visit - Interface TypePattern 在 ref 参数写入后不做静态折叠
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_WithRefWrite_ThrowsUnsupported()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void Rewrite(ref object value)
                {
                    value = new object();
                }

                void TestMethod()
                {
                    object obj = ""x"";
                    Rewrite(ref obj);
                    bool result = obj is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    Assert.Throws<OperationTransformationException>(
      () => walker.Visit(block, new()),
      "Unsupported type in is-type operation.");
  }

  /// <summary>
  /// 测试 Visit - is not 接口判定在可静态证明 true 分支时正确取反
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_NotPattern_FromTrue_FoldsToFalse()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""x"";
                    bool result = obj is not IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let result = !true;", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - is not 接口判定在可静态证明 false 分支时正确取反
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_NotPattern_FromFalse_FoldsToTrue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = null;
                    bool result = obj is not IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let result = !false;", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - Interface TypePattern 在可赋值调用表达式上降级为非空判断并保持单次求值
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_CallExpressionAssignable_FoldsToNonNullCheckWithSingleEvaluation()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                string? GetValue() => null;

                void TestMethod()
                {
                    bool result = GetValue() is IComparable;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    AssertStringContainsJsNaming(script, "let result = (v$0 = this.GetValue(), v$0 != null);", StringComparison.Ordinal);
    AssertContainsCount(script, "this.GetValue()", 1);
  }

  /// <summary>
  /// 测试 Visit - 属性子模式中的接口 TypePattern 使用子模式输入类型静态折叠，而非误用外层 is 输入
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_PropertySubpattern_UsesInnerInputType()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Name = ""x"" };
                    bool result = obj is { Name: IComparable };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    AssertStringContainsJsNaming(script, "\"Name\" in obj", StringComparison.Ordinal);
    AssertStringContainsJsNaming(script, "(v$0 = obj.Name, v$0 != null)", StringComparison.Ordinal);
    Assert.IsFalse(script.Contains("&& false", StringComparison.Ordinal));
    AssertContainsCount(script, "obj.Name", 1);
  }

  /// <summary>
  /// 测试 Visit - 属性子模式接口 TypePattern 在不可证明输入类型时保持显式不支持
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Interface_PropertySubpattern_WithObjectInput_ThrowsUnsupported()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Value = (object)""x"" };
                    bool result = obj is { Value: IComparable };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    Assert.Throws<OperationTransformationException>(
      () => walker.Visit(block, new()),
      "Unsupported type in is-type operation.");
  }

  /// <summary>
  /// 测试 Visit - switch 表达式中的接口 TypePattern 可从 discriminant 源静态折叠
  /// </summary>
  [TestMethod]
  public void Visit_SwitchExpression_InterfaceTypePattern_FromDiscriminant_FoldsToTrue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    string result = obj switch
                    {
                        IComparable => ""yes"",
                        _ => ""no""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "if (true)", StringComparison.Ordinal);
    StringAssert.Contains(script, "return \"yes\";", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - switch case 中的接口 TypePattern 可从 discriminant 源静态折叠
  /// </summary>
  [TestMethod]
  public void Visit_Switch_InterfaceTypePattern_FromDiscriminant_FoldsToTrue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    switch (obj)
                    {
                        case IComparable:
                            Console.WriteLine(""yes"");
                            break;
                        default:
                            Console.WriteLine(""no"");
                            break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "if (true)", StringComparison.Ordinal);
    StringAssert.Contains(script, "console.log(\"yes\");", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - switch 表达式中的接口 TypePattern 在不可证明场景保持显式不支持
  /// </summary>
  [TestMethod]
  public void Visit_SwitchExpression_InterfaceTypePattern_FromUnknownDiscriminant_ThrowsUnsupported()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                object GetValue() => new object();

                void TestMethod()
                {
                    object obj = GetValue();
                    string result = obj switch
                    {
                        IComparable => ""yes"",
                        _ => ""no""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    Assert.Throws<OperationTransformationException>(
      () => walker.Visit(block, new()),
      "Unsupported type in is-type operation.");
  }

  /// <summary>
  /// 测试 Visit - TypePattern IDictionary 接口仍可判定（Map 载体）
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_IDictionaryInterface_RemainsSupported()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new Dictionary<string, int>();
                    bool result = obj is IDictionary<string, int>;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = new Map;
  let result = obj instanceof Map;
}", script);
  }

  [TestMethod]
  public void Visit_TypePattern_TupleTarget_UsesErasedObjectCheck()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(object value)
                {
                    bool result = value is ValueTuple<int, int> tuple;
                }
            }
            ");

    var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
    var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

    Assert.IsNotNull(first);
    Assert.AreEqual(first, second);
    StringAssert.Contains(first, "value !== null && typeof value === \"object\"");
    StringAssert.Contains(first, "tuple = value");
    _ = new Parser().ParseScript(first);
  }

  [TestMethod]
  public void Visit_TypePattern_HashSetTarget_UsesSetCarrier()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(object value)
                {
                    bool result = value is HashSet<int>;
                }
            }
            ");

    var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
    var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

    Assert.IsNotNull(first);
    Assert.AreEqual(first, second);
    StringAssert.Contains(first, "value instanceof Set");
    _ = new Parser().ParseScript(first);
  }

  [TestMethod]
  public void Visit_TypePattern_EcmascriptDateTarget_UsesDateCarrier()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(object value)
                {
                    bool result = value is ECMAScript.Date;
                }
            }
            ");

    var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
    var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

    Assert.IsNotNull(first);
    Assert.AreEqual(first, second);
    StringAssert.Contains(first, "value instanceof Date");
    _ = new Parser().ParseScript(first);
  }

  #endregion

  #region Nullable 和声明模式测试

  /// <summary>
  /// 测试 Visit - DeclarationPattern Nullable 值为 null
  /// </summary>
  [TestMethod]
  public void Visit_DeclarationPattern_NullableValueType_Null()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int? value = null;
                    if (value is int actual)
                    {
                        Console.WriteLine(actual);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let actual;
  let value = null;
  if (typeof value === ""number"" && (actual = value, true)) {
    console.log(actual);
  }
}", script);
  }

  /// <summary>
  /// 测试 Visit - DeclarationPattern Nullable 值不为 null
  /// </summary>
  [TestMethod]
  public void Visit_DeclarationPattern_NullableValueType_HasValue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int? value = 42;
                    if (value is int actual)
                    {
                        Console.WriteLine(actual);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let actual;
  let value = 42;
  if (typeof value === ""number"" && (actual = value, true)) {
    console.log(actual);
  }
}", script);
  }

  /// <summary>
  /// 测试 Visit - DeclarationPattern 嵌套声明作用域
  /// </summary>
  [TestMethod]
  public void Visit_DeclarationPattern_NestedScope_DoubleCapture()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    if (obj is int x)
                    {
                        if (x is > 0 and var y)
                        {
                            Console.WriteLine($""{x}, {y}"");
                        }
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x;
  let obj = 42;
  if (typeof obj === ""number"" && (x = obj, true)) {
    let y;
    if (x > 0 && (y = x, true)) {
      console.log(`${x}, ${y}`);
    }
  }
}", script);
  }

  /// <summary>
  /// 测试 Visit - DeclarationPattern 循环中的变量捕获
  /// </summary>
  [TestMethod]
  public void Visit_DeclarationPattern_InLoop()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object[] items = [1, ""hello"", 3.14];
                    foreach (var item in items)
                    {
                        if (item is int value)
                        {
                            Console.WriteLine(value);
                        }
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let items = [1, ""hello"", 3.14];
  for (let item of items) {
    let value;
    if (typeof item === ""number"" && (value = item, true)) {
      console.log(value);
    }
  }
}", script);
  }

  #endregion

  #region Deconstruct 和记录类型测试

  /// <summary>
  /// 测试 Visit - RecursivePattern 记录类型位置解构
  /// </summary>
  [TestMethod]
  public void Visit_RecursivePattern_PositionalRecord()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var point = new Point(1, 2);
                    bool result = point is (1, 2);
                }

                record Point(int X, int Y);
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let point = { x: 1, y: 2 };
  let result = point.x === 1 && point.y === 2;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
  }

  /// <summary>
  /// 测试 Visit - RecursivePattern 记录类型带关系模式
  /// </summary>
  [TestMethod]
  public void Visit_RecursivePattern_PositionalRecord_WithRelational()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var point = new Point(1, 2);
                    bool result = point is (> 0, > 0);
                }

                record Point(int X, int Y);
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let point = { x: 1, y: 2 };
  let result = point.x > 0 && point.y > 0;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
  }

  /// <summary>
  /// 测试 Visit - RecursivePattern 记录类型变量捕获
  /// </summary>
  [TestMethod]
  public void Visit_RecursivePattern_PositionalRecord_WithCapture()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var point = new Point(1, 2);
                    if (point is (var x, var y))
                    {
                        Console.WriteLine($""({x}, {y})"");
                    }
                }

                record Point(int X, int Y);
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let x, y;
  let point = { x: 1, y: 2 };
  if ((x = point.x, true) && (y = point.y, true)) {
    console.log(`(${x}, ${y})`);
  }
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
  }

  [TestMethod]
  public void Visit_RecursivePattern_PositionalCustomDeconstructClass_UsesDeconstructOutputs()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var point = new Point(1, 2);
                    bool result = point is (2, 3);
                }

                class Point
                {
                    public int X { get; }
                    public int Y { get; }

                    public Point(int x, int y)
                    {
                        X = x;
                        Y = y;
                    }

                    public void Deconstruct(out int x, out int y)
                    {
                        x = X + 1;
                        y = Y + 1;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let v$0;
  let point = new Point(1, 2);
  let result = point instanceof Point && (v$0 = point.Deconstruct(undefined, undefined), true) && v$0[0] === 2 && v$0[1] === 3;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
  }

  #endregion

  #region Switch 表达式高级测试

  /// <summary>
  /// 测试 Visit - SwitchExpressionArm 带 when 子句和复杂逻辑
  /// </summary>
  [TestMethod]
  public void Visit_SwitchExpressionArm_WhenWithComplexLogic()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    string result = value switch
                    {
                        var x when x > 0 && x < 10 => ""Small"",
                        var x when x >= 10 => ""Large"",
                        _ => ""Unknown""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = 5;
  let result = (() => {
    let x;
    const v$0 = value;
    if ((x = v$0, true) && (x > 0 && x < 10))
      return ""Small"";
    if ((x = v$0, true) && x >= 10)
      return ""Large"";
    return ""Unknown"";
  })();
}", script);

  }

  /// <summary>
  /// 测试 Visit - SwitchExpressionArm 嵌套模式带 when
  /// </summary>
  [TestMethod]
  public void Visit_SwitchExpressionArm_NestedPatternWithWhen()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Value = 42 };
                    string result = obj switch
                    {
                        { Value: var v } when v > 0 => ""Positive"",
                        { Value: var v } when v < 0 => ""Negative"",
                        _ => ""Zero""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(
@"{
  let obj = { Value: 42 };
  let result = (() => {
    let v;
    const v$0 = obj;
    if (v$0 != null && ""Value"" in v$0 && (v = v$0.Value, true) && v > 0)
      return ""Positive"";
    if (v$0 != null && ""Value"" in v$0 && (v = v$0.Value, true) && v < 0)
      return ""Negative"";
    return ""Zero"";
  })();
}", script);

  }

  #endregion

  #region 列表模式高级测试

  /// <summary>
  /// 测试 Visit - ListPattern 多维数组
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_MultiDimensionalArray()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[,] matrix = { { 1, 2 }, { 3, 4 } };
                    bool result = matrix is { Length: > 0 };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let matrix = [[1, 2], [3, 4]];
  let result = Array.isArray(matrix) && matrix != null && ""length"" in matrix && matrix.length > 0;
}", script);
  }

  /// <summary>
  /// 测试 Visit - ListPattern 交错数组（锯齿数组）
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_JaggedArray()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[][] jagged = new int[2][];
                    jagged[0] = new[] { 1, 2 };
                    jagged[1] = new[] { 3, 4, 5 };
                    bool result = jagged is [_, ..];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let jagged = new Array(2);
  jagged[0] = [1, 2];
  jagged[1] = [3, 4, 5];
  let result = Array.isArray(jagged) && jagged.length >= 1;
}", script);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 多变量捕获
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_WithMultipleVariableCaptures()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    if (array is [var first, var second, .. var rest, var last])
                    {
                        Console.WriteLine($""{first}, {second}, {rest.Length}, {last}"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let first, second, rest, last;
  let array = [1, 2, 3, 4, 5];
  if (Array.isArray(array) && array.length >= 3 && (first = array[0], true) && (second = array[1], true) && (rest = array.slice(2, -1), true) && (last = array[array.length - 1], true)) {
    console.log(`${first}, ${second}, ${rest.length}, ${last}`);
  }
}", script);

  }

  #endregion

  #region 复杂模式组合测试

  /// <summary>
  /// 测试 Visit - 复杂模式组合 类型、属性、关系
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_TypePropertyRelational()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    bool result = obj is string { Length: > 0 and < 100 };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertContainsCount(script, "obj.Length", 1);
    StringAssert.Contains(script, "\"length\" in obj && (v$0 = obj.length, v$0 > 0 && v$0 < 100)", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - 复杂模式 三层嵌套
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_DeepNesting()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var data = new { Outer = new { Middle = new { Inner = 42 } } };
                    bool result = data is { Outer: { Middle: { Inner: > 0 } } };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertContainsCount(script, "data.Outer", 1);
    AssertContainsCount(script, "v$0.Middle", 1);
    AssertStringContainsJsNaming(script, "(v$0 = data.Outer, v$0 != null && \"Middle\" in v$0 && (v$1 = v$0.Middle, v$1 != null && \"Inner\" in v$1 && v$1.Inner > 0))", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - 复杂模式 所有模式类型组合
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_AllPatternTypes()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Items = new[] { 1, 2, 3 } };
                    bool result = obj is { Items: [var first, ..] and { Length: > 0 } };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertContainsCount(script, "obj.Items", 1);
    AssertStringContainsJsNaming(script, "let v$0, first;", StringComparison.Ordinal);
    AssertStringContainsJsNaming(script, "(v$0 = obj.Items, Array.isArray(v$0) && v$0.Length >= 1 && (first = v$0[0], true)", StringComparison.Ordinal);
    AssertStringContainsJsNaming(script, "\"length\" in v$0 && v$0.Length > 0", StringComparison.Ordinal);
  }

  #endregion

  #region 属性模式高级测试

  /// <summary>
  /// 测试 Visit - PropertySubpattern null 检查
  /// </summary>
  [TestMethod]
  public void Visit_PropertySubpattern_NullChecking()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Value = (string?)null };
                    bool result = obj is { Value: not null };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(
@"{
  let obj = { Value: null };
  let result = obj != null && ""Value"" in obj && !(obj.Value == null);
}", script);

  }

  /// <summary>
  /// 测试 Visit - PropertySubpattern 可选链式属性
  /// </summary>
  [TestMethod]
  public void Visit_PropertySubpattern_OptionalChaining()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Inner = new { Value = 42 } };
                    bool result = obj is { Inner: { Value: var v } };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertStringContainsJsNaming(script, "let v$0, v;", StringComparison.Ordinal);
    AssertContainsCount(script, "obj.Inner", 1);
    AssertStringContainsJsNaming(script, "\"Inner\" in obj && (v$0 = obj.Inner, v$0 != null && \"Value\" in v$0 && (v = v$0.Value, true))", StringComparison.Ordinal);
  }

  #endregion

  #region Switch 语句高级测试

  /// <summary>
  /// 测试 Visit - Switch 语句 default 分支中的声明模式
  /// </summary>
  [TestMethod]
  public void Visit_DeclarationPattern_InSwitch_Default()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    switch (obj)
                    {
                        case string s:
                            Console.WriteLine(s);
                            break;
                        case var x:
                            Console.WriteLine($""Default: {x}"");
                            break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = 42;
  (() => {
    let s, x;
    const v$0 = obj;
    if (typeof v$0 === ""string"" && (s = v$0, true)) {
      console.log(s);
      return;
    }
    if (x = v$0, true) {
      console.log(`Default: ${x}`);
      return;
    }
  })();
}", script);
  }

  #endregion

  #region 三元运算符中的模式测试

  /// <summary>
  /// 测试 Visit - 三元运算符中的类型模式
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_InTernaryOperator()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    string result = obj is string ? ""is string"" : ""not string"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = ""hello"";
  let result = typeof obj === ""string"" ? ""is string"" : ""not string"";
}", script);
  }

  /// <summary>
  /// 测试 Visit - 三元运算符中的 null 检查模式
  /// </summary>
  [TestMethod]
  public void Visit_IsNull_InTernaryOperator()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = null;
                    string result = obj is null ? ""is null"" : ""not null"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = null;
  let result = obj == null ? ""is null"" : ""not null"";
}", script);
  }

  /// <summary>
  /// 测试 Visit - 三元运算符中的关系模式
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_InTernaryOperator()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    string result = value is > 0 ? ""positive"" : ""non-positive"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = value > 0 ? ""positive"" : ""non-positive"";
}", script);
  }

  /// <summary>
  /// 测试 Visit - 嵌套三元运算符中的模式
  /// </summary>
  [TestMethod]
  public void Visit_Pattern_NestedTernary()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    string result = x > 0 ? (x > 10 ? ""large"" : ""small"") : ""negative"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 5;
  let result = x > 0 ? x > 10 ? ""large"" : ""small"" : ""negative"";
}", script);
  }

  #endregion

  #region 控制流语句中的模式测试

  /// <summary>
  /// 测试 Visit - for 语句中的关系模式
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_InForStatement()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (i is > 0 and < 5)
                        {
                            Console.WriteLine(i);
                        }
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  for (let i = 0; i < 10; i++) {
    if (i > 0 && i < 5) {
      console.log(i);
    }
  }
}", script);
  }

  /// <summary>
  /// 测试 Visit - do-while 语句中的关系模式
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_InDoWhile()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    do
                    {
                        value--;
                    } while (value is > 0);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  do {
    value--;
  } while (value > 0);
}", script);
  }

  /// <summary>
  /// 测试 Visit - foreach 语句中的声明模式
  /// </summary>
  [TestMethod]
  public void Visit_DeclarationPattern_InForeach()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object[] items = [1, ""hello"", 3.14, 42];
                    foreach (var item in items)
                    {
                        if (item is int value and > 0)
                        {
                            Console.WriteLine(value);
                        }
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let items = [1, ""hello"", 3.14, 42];
  for (let item of items) {
    let value;
    if (typeof item === ""number"" && (value = item, true) && item > 0) {
      console.log(value);
    }
  }
}", script);

  }

  #endregion

  #region 复杂嵌套模式测试

  /// <summary>
  /// 测试 Visit - 四层嵌套的属性模式
  /// </summary>
  [TestMethod]
  public void Visit_PropertyPattern_FourLevelNesting()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var data = new {
                        Level1 = new {
                            Level2 = new {
                                Level3 = new {
                                    Value = 42
                                }
                            }
                        }
                    };
                    bool result = data is {
                        Level1: {
                            Level2: {
                                Level3: {
                                    Value: > 0
                                }
                            }
                        }
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertStringContainsJsNaming(script, "\"Level1\" in data", StringComparison.Ordinal);
    AssertContainsCount(script, "data.Level1", 1);
    AssertContainsCount(script, "v$0.Level2", 1);
    AssertContainsCount(script, "v$1.Level3", 1);
    AssertStringContainsJsNaming(script, "(v$0 = data.Level1, v$0 != null && \"Level2\" in v$0 && (v$1 = v$0.Level2, v$1 != null && \"Level3\" in v$1 && (v$2 = v$1.Level3, v$2 != null && \"Value\" in v$2 && v$2.Value > 0)))", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - 复杂的类型和属性组合模式
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_TypeAndPropertyWithOr()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""test"";
                    bool result = obj is string { Length: > 0 and < 100 } or int { };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertContainsCount(script, "obj.Length", 1);
    StringAssert.Contains(script, "\"length\" in obj && (v$0 = obj.length, v$0 > 0 && v$0 < 100)", StringComparison.Ordinal);
    StringAssert.Contains(script, "|| typeof obj === \"number\";", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Visit - switch 表达式中的复杂列表模式
  /// </summary>
  [TestMethod]
  public void Visit_SwitchExpression_ComplexListPattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] numbers = [1, 2, 3, 4, 5];
                    string result = numbers switch
                    {
                        [var first, var second, ..] when first > 0 => ""Starts with positive"",
                        [.., var last] when last < 0 => ""Ends with negative"",
                        _ => ""Other""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let numbers = [1, 2, 3, 4, 5];
  let result = (() => {
    let first, second, last;
    const v$0 = numbers;
    if (Array.isArray(v$0) && v$0.length >= 2 && (first = v$0[0], true) && (second = v$0[1], true) && first > 0)
      return ""Starts with positive"";
    if (Array.isArray(v$0) && v$0.length >= 1 && (last = v$0[v$0.length - 1], true) && last < 0)
      return ""Ends with negative"";
    return ""Other"";
  })();
}", script);
  }

  /// <summary>
  /// 测试 Visit - switch 表达式中的复杂列表模式
  /// </summary>
  [TestMethod]
  public void Visit_SwitchExpression_ComplexValue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string result = A.GetNumbers() switch
                    {
                        [var first, var second, ..] when first > 0 => ""Starts with positive"",
                        [.., var last] when last < 0 => ""Ends with negative"",
                        _ => ""Other""
                    };
                }

                static class A
                {
                   public static int[] GetNumbers()=>[1, 2, 3, 4, 5];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(
@"{
  let result = (() => {
    let first, second, last;
    const v$0 = A.GetNumbers();
    if (Array.isArray(v$0) && v$0.length >= 2 && (first = v$0[0], true) && (second = v$0[1], true) && first > 0)
      return ""Starts with positive"";
    if (Array.isArray(v$0) && v$0.length >= 1 && (last = v$0[v$0.length - 1], true) && last < 0)
      return ""Ends with negative"";
    return ""Other"";
  })();
}".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));

  }
  #endregion

  #region 模式与表达式组合测试

  /// <summary>
  /// 测试 Visit - 模式匹配与逻辑运算符结合
  /// </summary>
  [TestMethod]
  public void Visit_Pattern_CombinedWithLogicalOperators()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    bool result = obj is int && obj is > 0 || obj is string { Length: > 0 };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let obj = 42;
  let result = typeof obj === ""number"" && obj > 0 || typeof obj === ""string"" && obj != null && ""length"" in obj && obj.length > 0;
}", script);

  }

  /// <summary>
  /// 测试 Visit - 模式匹配作为方法参数
  /// </summary>
  [TestMethod]
  public void Visit_Pattern_AsMethodArgument()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    Console.WriteLine(obj is int ? ""integer"" : ""not integer"");
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = 42;
  console.log(typeof obj === ""number"" ? ""integer"" : ""not integer"");
}", script);
  }

  #endregion

  #region 边界条件测试

  /// <summary>
  /// 测试超长列表模式（15+ 元素）
  /// C# 示例：list is [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]
  /// 转换结果：JavaScript 长模式匹配表达式
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_ExtraLong()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
                    bool match = list is [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    StringAssert.Contains(script, "let match = Array.isArray(list) && list.length === 15", StringComparison.Ordinal);
    AssertContainsCount(script, "_d389c31d59037b42(list, ", 15);
    AssertContainsCount(script, "list[0] === 1", 0);
    AssertContainsCount(script, "list[14] === 15", 0);
    StringAssert.Contains(script, "_d389c31d59037b42(list, 0) === 1", StringComparison.Ordinal);
    StringAssert.Contains(script, "_d389c31d59037b42(list, 14) === 15", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试深度嵌套属性模式（6+ 层）
  /// C# 示例：obj is { A: { B: { C: { D: { E: { F: 1 } } } } } }
  /// 转换结果：JavaScript 深度嵌套属性检查
  /// </summary>
  [TestMethod]
  public void Visit_PropertySubpattern_SixLevelNesting()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new {
                        A = new {
                            B = new {
                                C = new {
                                    D = new {
                                        E = new {
                                            F = 42
                                        }
                                    }
                                }
                            }
                        }
                    };
                    bool match = obj is {
                        A: {
                            B: {
                                C: {
                                    D: {
                                        E: {
                                            F: 42
                                        }
                                    }
                                }
                            }
                        }
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertContainsCount(script, "obj.A", 1);
    AssertContainsCount(script, "v$0.B", 1);
    AssertContainsCount(script, "v$1.C", 1);
    AssertContainsCount(script, "v$2.D", 1);
    AssertContainsCount(script, "v$3.E", 1);
    AssertStringContainsJsNaming(script, "(v$4 = v$3.E, v$4 != null && \"F\" in v$4 && v$4.F === 42)", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试声明模式边界 - 作用域嵌套
  /// C# 示例：嵌套作用域中的声明模式变量
  /// 转换结果：JavaScript 变量作用域处理
  /// </summary>
  [TestMethod]
  public void Visit_DeclarationPattern_ScopeBoundary()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj1 = ""hello"";
                    if (obj1 is string s && s.Length > 0)
                    {
                        Console.WriteLine(s);
                    }
                    if (obj1 is string s2 && s2.Length > 0)
                    {
                        Console.WriteLine(s2);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    // 验证不同作用域的变量声明
    // 注意：变量可能在同一行或不同行声明
    Assert.AreEqual(
@"{
  let s, s2;
  let obj1 = ""hello"";
  if (typeof obj1 === ""string"" && (s = obj1, true) && s.length > 0) {
    console.log(s);
  }
  if (typeof obj1 === ""string"" && (s2 = obj1, true) && s2.length > 0) {
    console.log(s2);
  }
}", script);

  }

  #endregion

  /// <summary>
  /// 测试综合场景 - 组合多种模式匹配和 switch 语句
  /// 输入: 关系模式 (>, &lt;), 声明模式 (var), 逻辑模式 (and, not), switch case with when 子句
  /// 期望输出: 正确转换所有模式为 JavaScript 语法，包括变量提升和 IIFE 包装的 switch
  /// </summary>
  [TestMethod]
  public void Visit_All()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value > 9 && (value is > 0 and < 10 and not 5) && (value is var x && x < 10);
                    switch (value)
                    {
                      case var s when s > 0:
                        Console.WriteLine("">0"");
                        break;
                      case 1:
                        Console.WriteLine(""1"");
                        break;			
                      case 2:
                        Console.WriteLine(""2"");
                        break;
                      default:
                        Console.WriteLine(""Default"");
                        break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
  @"{
  let x;
  let value = 5;
  let result = value > 9 && (value > 0 && value < 10 && !(value === 5)) && ((x = value, true) && x < 10);
  (() => {
    let s;
    const v$0 = value;
    if ((s = v$0, true) && s > 0) {
      console.log("">0"");
      return;
    }
    if (v$0 === 1) {
      console.log(""1"");
      return;
    }
    if (v$0 === 2) {
      console.log(""2"");
      return;
    }
    console.log(""Default"");
    return;
  })();
}", script);

  }

  #region 扩展测试用例 - 更多类型模式

  /// <summary>
  /// 测试类型模式 - int 可空
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_IntNullable()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int? value = 42;
                    if (value is int v)
                    {
                        Console.WriteLine(v);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let v;
  let value = 42;
  if (typeof value === ""number"" && (v = value, true)) {
    console.log(v);
  }
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
  }

  /// <summary>
  /// 测试类型模式 - double
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_DoubleType()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 3.14;
                    if (obj is double d)
                    {
                        Console.WriteLine(d);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let d;
  let obj = 3.14;
  if (typeof obj === ""number"" && (d = obj, true)) {
    console.log(d);
  }
}", script);
  }

  /// <summary>
  /// 测试类型模式 - bool
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_BoolType()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = true;
                    if (obj is bool b)
                    {
                        Console.WriteLine(b);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let b;
  let obj = true;
  if (typeof obj === ""boolean"" && (b = obj, true)) {
    console.log(b);
  }
}", script);
  }

  #endregion

  #region 扩展测试用例 - 更常量模式

  /// <summary>
  /// 测试常量模式 - 负数
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_NegativeConstant()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = -5;
                    if (value is -5)
                    {
                        Console.WriteLine(""negative five"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = -5;
  if (value === -5) {
    console.log(""negative five"");
  }
}", script);
  }

  /// <summary>
  /// 测试常量模式 - 零
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_ZeroConstant()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 0;
                    if (value is 0)
                    {
                        Console.WriteLine(""zero"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = 0;
  if (value === 0) {
    console.log(""zero"");
  }
}", script);
  }

  /// <summary>
  /// 测试常量模式 - 空字符串
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_EmptyString1()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string s = """";
                    if (s is """")
                    {
                        Console.WriteLine(""empty"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let s = """";
  if (s === """") {
    console.log(""empty"");
  }
}", script);
  }

  #endregion

  #region 扩展测试用例 - 更多关系模式

  /// <summary>
  /// 测试关系模式 - >=
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_GreaterThanOrEqual()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 10;
                    if (value is >= 10)
                    {
                        Console.WriteLine(""ten or more"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = 10;
  if (value >= 10) {
    console.log(""ten or more"");
  }
}", script);
  }

  /// <summary>
  /// 测试关系模式 - <=
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_LessThanOrEqual()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    if (value is <= 10)
                    {
                        Console.WriteLine(""ten or less"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = 5;
  if (value <= 10) {
    console.log(""ten or less"");
  }
}", script);
  }

  #endregion

  #region 扩展测试用例 - 更多逻辑模式

  /// <summary>
  /// 测试逻辑模式 - or
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_OrPattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 1;
                    if (value is 1 or 2 or 3)
                    {
                        Console.WriteLine(""one, two, or three"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = 1;
  if (value === 1 || value === 2 || value === 3) {
    console.log(""one, two, or three"");
  }
}", script);
  }

  /// <summary>
  /// 测试逻辑模式 - 复杂 and/or 混合
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_ComplexAndOr()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 15;
                    if (value is > 0 and < 10 or > 20)
                    {
                        Console.WriteLine(""in range"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let value = 15;
  if (value > 0 && value < 10 || value > 20) {
    console.log(""in range"");
  }
}", script);
  }

  #endregion

  #region 扩展测试用例 - 更多属性模式

  /// <summary>
  /// 测试属性模式 - 嵌套属性
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_NestedProperty()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                class Person { public Address Address { get; set; } }
                class Address { public string City { get; set; } }

                void TestMethod()
                {
                    var person = new Person { Address = new Address { City = ""NYC"" } };
                    if (person is { Address.City: ""NYC"" })
                    {
                        Console.WriteLine(""New Yorker"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertContainsCount(script, "person.Address", 1);
    AssertStringContainsJsNaming(script, "(v$2 = person.Address, v$2 instanceof Address && v$2 != null && \"City\" in v$2 && v$2.City === \"NYC\")", StringComparison.Ordinal);
    AssertStringContainsJsNaming(script, "console.log(\"New Yorker\");", StringComparison.Ordinal);
  }

  [TestMethod]
  public void Visit_IsPattern_NestedProperty_CachesIntermediateMemberInput()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                class Person { public Address Address { get; set; } }
                class Address { public string City { get; set; } public int Zip { get; set; } }

                void TestMethod()
                {
                    var person = new Person { Address = new Address { City = ""NYC"", Zip = 10001 } };
                    if (person is { Address: { City: ""NYC"", Zip: > 0 } })
                    {
                        Console.WriteLine(""New Yorker"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertContainsCount(script, "person.Address", 1);
    AssertStringContainsJsNaming(script, "(v$2 = person.Address, v$2 instanceof Address && v$2 != null", StringComparison.Ordinal);
    AssertStringContainsJsNaming(script, "v$2.City === \"NYC\"", StringComparison.Ordinal);
    AssertStringContainsJsNaming(script, "v$2.Zip > 0", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试属性模式 - 多个属性
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_MultipleProperties()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                class Point { public int X { get; set; } public int Y { get; set; } }

                void TestMethod()
                {
                    var point = new Point { X = 10, Y = 20 };
                    if (point is { X: 10, Y: 20 })
                    {
                        Console.WriteLine(""found"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let point = (() => {
    let v$0 = new Point;
    v$0.X = 10;
    v$0.Y = 20;
    return v$0;
  })();
  if (point instanceof Point && point != null && ""X"" in point && point.X === 10 && ""Y"" in point && point.Y === 20) {
    console.log(""found"");
  }
}", script);
  }

  #endregion

  #region 扩展测试用例 - 更多列表模式

  /// <summary>
  /// 测试列表模式 - 空列表
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_EmptyList()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] arr = new int[0];
                    if (arr is [])
                    {
                        Console.WriteLine(""empty"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let arr = [];
  if (Array.isArray(arr) && arr.length === 0) {
    console.log(""empty"");
  }
}", script);
  }

  /// <summary>
  /// 测试列表模式 - 单元素
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_SingleElementList()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] arr = new int[] { 42 };
                    if (arr is [var single])
                    {
                        Console.WriteLine(single);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let single;
  let arr = [42];
  if (Array.isArray(arr) && arr.length === 1 && (single = arr[0], true)) {
    console.log(single);
  }
}", script);
  }

  /// <summary>
  /// 测试列表模式 - 带切片
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_ListWithSlice()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] arr = new int[] { 1, 2, 3, 4, 5 };
                    if (arr is [var first, .. var rest])
                    {
                        Console.WriteLine(first);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let first, rest;
  let arr = [1, 2, 3, 4, 5];
  if (Array.isArray(arr) && arr.length >= 1 && (first = arr[0], true) && (rest = arr.slice(1), true)) {
    console.log(first);
  }
}", script);
  }

  #endregion

  #region 扩展测试用例 - switch表达式模式

  /// <summary>
  /// 测试 switch 表达式 - 常量模式
  /// </summary>
  [TestMethod]
  public void Visit_SwitchExpression_ConstantPatterns()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 2;
                    string result = value switch
                    {
                        1 => ""one"",
                        2 => ""two"",
                        3 => ""three"",
                        _ => ""other""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = 2;
  let result = (() => {
    const v$0 = value;
    if (v$0 === 1)
      return ""one"";
    if (v$0 === 2)
      return ""two"";
    if (v$0 === 3)
      return ""three"";
    return ""other"";
  })();
}", script);
  }

  /// <summary>
  /// 测试 switch 表达式 - 异步分支应生成 async IIFE
  /// </summary>
  [TestMethod]
  public void Visit_SwitchExpression_AsyncArms_UsesAsyncIife()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                async System.Threading.Tasks.Task TestMethod()
                {
                    int value = 2;
                    int result = value switch
                    {
                        1 => await System.Threading.Tasks.Task.FromResult(10),
                        _ => await System.Threading.Tasks.Task.FromResult(20)
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let result = (async () => {", StringComparison.Ordinal);
    StringAssert.Contains(script, "await Promise.resolve(10);", StringComparison.Ordinal);
    StringAssert.Contains(script, "await Promise.resolve(20);", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 switch 表达式 - 类型模式
  /// </summary>
  [TestMethod]
  public void Visit_SwitchExpression_TypePatterns()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    string result = obj switch
                    {
                        string s => $""string: {s}"",
                        int i => $""int: {i}"",
                        _ => ""other""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let obj = ""hello"";
  let result = (() => {
    let s, i;
    const v$0 = obj;
    if (typeof v$0 === ""string"" && (s = v$0, true))
      return `string: ${s}`;
    if (typeof v$0 === ""number"" && (i = v$0, true))
      return `int: ${i}`;
    return ""other"";
  })();
}", script);
  }

  #endregion

  #region 扩展测试用例 - 模式匹配综合

  /// <summary>
  /// 测试模式匹配在三元表达式中
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_InTernary()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    string result = obj is int i ? $""int: {i}"" : ""not int"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let i;
  let obj = 42;
  let result = typeof obj === ""number"" && (i = obj, true) ? `int: ${i}` : ""not int"";
}", script);
  }

  /// <summary>
  /// 测试模式匹配在循环条件中
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_InLoop()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var items = new object[] { 1, ""two"", 3.0 };
                    foreach (var item in items)
                    {
                        if (item is int n)
                        {
                            Console.WriteLine(n * 2);
                        }
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let items = [1, ""two"", 3];
  for (let item of items) {
    let n;
    if (typeof item === ""number"" && (n = item, true)) {
      console.log(n * 2);
    }
  }
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
  }

  /// <summary>
  /// 测试模式匹配在方法参数中
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_InMethodParameter()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Process(""hello"");
                }

                void Process(object obj)
                {
                    switch (obj)
                    {
                        case string s:
                            Console.WriteLine(s.Length);
                            break;
                        case int n:
                            Console.WriteLine(n * 2);
                            break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  this.Process(""hello"");
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
  }

  #endregion

  #region 扩展测试用例 - 更多声明模式

  /// <summary>
  /// 测试声明模式 - var 声明
  /// </summary>
  [TestMethod]
  public void Visit_DeclarationPattern_Var()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    if (obj is var value)
                    {
                        Console.WriteLine(value);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let value;
  let obj = 42;
  if (value = obj, true) {
    console.log(value);
  }
}", script);
  }

  /// <summary>
  /// 测试声明模式 - 类型声明带变量名
  /// </summary>
  [TestMethod]
  public void Visit_DeclarationPattern_Typed()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    if (obj is string s)
                    {
                        Console.WriteLine(s.Length);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let s;
  let obj = ""hello"";
  if (typeof obj === ""string"" && (s = obj, true)) {
    console.log(s.length);
  }
}", script);
  }

  [TestMethod]
  public void Visit_DeclarationPattern_EcmascriptNumber_UsesTypeOfNumber()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    if (obj is Number n)
                    {
                        Console.WriteLine(n);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let n;
  let obj = 42;
  if (typeof obj === ""number"" && (n = obj, true)) {
    console.log(n);
  }
}", script);
  }

  [TestMethod]
  public void Visit_DeclarationPattern_EcmascriptArray_UsesArrayIsArray()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new string[] { "","" };
                    if (obj is Array<string> items)
                    {
                        Console.WriteLine(items.Length);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let items;
  let obj = ["",""];
  if (Array.isArray(obj) && (items = obj, true)) {
    console.log(items.length);
  }
}", script);
  }

  #endregion

  #region 扩展测试用例 - 更多逻辑模式

  /// <summary>
  /// 测试逻辑模式 - and 模式
  /// </summary>
  [TestMethod]
  public void Visit_LogicalPattern_And()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 15;
                    if (value is > 0 and < 100)
                    {
                        Console.WriteLine(""in range"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = 15;
  if (value > 0 && value < 100) {
    console.log(""in range"");
  }
}", script);
  }

  /// <summary>
  /// 测试逻辑模式 - or 模式
  /// </summary>
  [TestMethod]
  public void Visit_LogicalPattern_Or()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    if (value is 1 or 2 or 3)
                    {
                        Console.WriteLine(""small"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = 5;
  if (value === 1 || value === 2 || value === 3) {
    console.log(""small"");
  }
}", script);
  }

  /// <summary>
  /// 测试逻辑模式 - not 模式
  /// </summary>
  [TestMethod]
  public void Visit_LogicalPattern_Not()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object? obj = null;
                    if (obj is not null)
                    {
                        Console.WriteLine(obj);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let obj = null;
  if (!(obj == null)) {
    console.log(obj);
  }
}", script);
  }

  #endregion

  #region 扩展测试用例 - 更多属性模式

  /// <summary>
  /// 测试属性模式 - 单属性
  /// </summary>
  [TestMethod]
  public void Visit_PropertyPattern_Single()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                class Person { public string Name { get; set; } }

                void TestMethod()
                {
                    var person = new Person { Name = ""John"" };
                    if (person is { Name: ""John"" })
                    {
                        Console.WriteLine(""found"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let person = (() => {
    let v$0 = new Person;
    v$0.Name = ""John"";
    return v$0;
  })();
  if (person instanceof Person && person != null && ""Name"" in person && person.Name === ""John"") {
    console.log(""found"");
  }
}", script);
  }

  /// <summary>
  /// 测试属性模式 - 多属性
  /// </summary>
  [TestMethod]
  public void Visit_PropertyPattern_Multiple()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                class Point { public int X { get; set; } public int Y { get; set; } }

                void TestMethod()
                {
                    var point = new Point { X = 0, Y = 0 };
                    if (point is { X: 0, Y: 0 })
                    {
                        Console.WriteLine(""origin"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let point = (() => {
    let v$0 = new Point;
    v$0.X = 0;
    v$0.Y = 0;
    return v$0;
  })();
  if (point instanceof Point && point != null && ""X"" in point && point.X === 0 && ""Y"" in point && point.Y === 0) {
    console.log(""origin"");
  }
}", script);
  }

  [TestMethod]
  public void Visit_PropertyPattern_Record_UsesStructuralMatchWithoutInstanceOf()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                record Person(string Name, int Age);

                void TestMethod()
                {
                    var person = new Person(""John"", 20);
                    if (person is { Name: ""John"" })
                    {
                        Console.WriteLine(""found"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let person = { name: ""John"", age: 20 };
  if (person != null && ""name"" in person && person.name === ""John"") {
    console.log(""found"");
  }
}", script);
  }

  [TestMethod]
  public void Visit_TypePattern_Record_BareType_Throws()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                record Person(string Name, int Age);

                void TestMethod(object value)
                {
                    bool match = value is Person;
                }
            }
            ");

    var walker = new SemanticWalker(true);

    try
    {
      _ = walker.Visit(block, new());
      Assert.Fail("Expected OperationTransformationException for bare record type pattern.");
    }
    catch (OperationTransformationException exception)
    {
      StringAssert.Contains(exception.Message, "structural lowering");
      StringAssert.Contains(exception.Message, "bare type pattern");
    }
  }

  /// <summary>
  /// 测试白名单运行时属性模式会复用 getter helper，而不是回退成原始成员名
  /// </summary>
  [TestMethod]
  public void Visit_PropertyPattern_DateTimeProperty_UsesRuntimeHelper()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    DateTime now = DateTime.Now;
                    if (now is { Year: 2024 })
                    {
                        Console.WriteLine(""match"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let now = _ee9dd166a34a2fa5();
  if (now instanceof JDateTime && now != null && _9d56b09432f81c05(now) === 2024) {
    console.log(""match"");
  }
}", script);
  }

  /// <summary>
  /// 测试白名单运行时类型上未进入支持表的属性模式不会静默回退
  /// </summary>
  [TestMethod]
  public void Visit_PropertyPattern_UnmappedRuntimeProperty_Throws()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new List<int>();
                    if (list is { Capacity: 0 })
                    {
                        Console.WriteLine(""match"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    Assert.Throws<OperationTransformationException>(() =>
    {
      _ = walker.Visit(block, new());
    });
  }

  #endregion

  #region 扩展测试用例 - 更多关系模式

  /// <summary>
  /// 测试关系模式 - 大于
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_GreaterThan1()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 10;
                    if (value is > 5)
                    {
                        Console.WriteLine(""greater"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = 10;
  if (value > 5) {
    console.log(""greater"");
  }
}", script);
  }

  /// <summary>
  /// 测试关系模式 - 小于
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_LessThan1()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 3;
                    if (value is < 5)
                    {
                        Console.WriteLine(""less"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = 3;
  if (value < 5) {
    console.log(""less"");
  }
}", script);
  }

  /// <summary>
  /// 测试关系模式 - 大于等于
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_GreaterThanOrEqual1()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    if (value is >= 5)
                    {
                        Console.WriteLine(""at least 5"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = 5;
  if (value >= 5) {
    console.log(""at least 5"");
  }
}", script);
  }

  /// <summary>
  /// 测试关系模式 - 小于等于
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_LessThanOrEqual1()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    if (value is <= 5)
                    {
                        Console.WriteLine(""at most 5"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let value = 5;
  if (value <= 5) {
    console.log(""at most 5"");
  }
}", script);
  }

  #endregion

  #region 扩展测试用例 - 更多列表模式

  /// <summary>
  /// 测试列表模式 - 空列表
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_Empty1()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] arr = new int[0];
                    if (arr is [])
                    {
                        Console.WriteLine(""empty"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let arr = [];
  if (Array.isArray(arr) && arr.length === 0) {
    console.log(""empty"");
  }
}", script);
  }

  /// <summary>
  /// 测试列表模式 - 单元素
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_SingleElement1()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] arr = new int[] { 42 };
                    if (arr is [var single])
                    {
                        Console.WriteLine(single);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let single;
  let arr = [42];
  if (Array.isArray(arr) && arr.length === 1 && (single = arr[0], true)) {
    console.log(single);
  }
}", script);
  }

  /// <summary>
  /// 测试列表模式 - 两元素
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_TwoElements()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] arr = new int[] { 1, 2 };
                    if (arr is [var first, var second])
                    {
                        Console.WriteLine(first + second);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let first, second;
  let arr = [1, 2];
  if (Array.isArray(arr) && arr.length === 2 && (first = arr[0], true) && (second = arr[1], true)) {
    console.log(first + second);
  }
}", script);
  }

  #endregion

  #region 扩展测试用例 - 更多丢弃模式

  /// <summary>
  /// 测试丢弃模式 - 在元组解构中
  /// </summary>
  [TestMethod]
  public void Visit_DiscardPattern_Tuple()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var (a, _, c) = (1, 2, 3);
                    Console.WriteLine(a + c);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let a, c;
  a = 1, c = 3;
  console.log(a + c);
}", script);
  }

  /// <summary>
  /// 测试丢弃模式 - 在 switch 中
  /// </summary>
  [TestMethod]
  public void Visit_DiscardPattern_Switch()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    string result = value switch
                    {
                        1 => ""one"",
                        _ => ""other""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let value = 5;
  let result = (() => {
    const v$0 = value;
    if (v$0 === 1)
      return ""one"";
    return ""other"";
  })();
}", script);
  }

  #endregion
}
