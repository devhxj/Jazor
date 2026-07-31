using Acornima;
using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerOrdinaryTest
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

  private static IMethodBodyOperation GetMethodBodyOperation(string code, string methodName = "TestMethod")
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
    var methodDeclaration = root.DescendantNodes()
      .OfType<MethodDeclarationSyntax>()
      .FirstOrDefault(m => m.Identifier.ValueText == methodName);
    if (methodDeclaration is not null &&
        semanticModel.GetOperation(methodDeclaration) is IMethodBodyOperation operation)
      return operation;

    throw new InvalidOperationException($"未找到方法体操作: {methodName}");
  }

  private static IConstructorBodyOperation GetConstructorBodyOperation(string code)
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
    var constructorDeclaration = root.DescendantNodes().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
    if (constructorDeclaration is not null &&
        semanticModel.GetOperation(constructorDeclaration) is IConstructorBodyOperation operation)
      return operation;

    throw new InvalidOperationException("未找到构造函数体操作");
  }

  private static void AssertScriptEqual(string expected, string? actual)
    => Assert.AreEqual(ExpectedJsNaming.Normalize(expected).ReplaceLineEndings("\n"), actual?.ReplaceLineEndings("\n"));

  /// <summary>
  /// 获取指定索引的操作
  /// </summary>
  private static T GetOperationAt<T>(IBlockOperation block, int index = 0) where T : class, IOperation
  {
    var operation = block.Operations.Skip(index).First() as T;
    return operation ?? throw new InvalidOperationException("未找到可分析的操作");
  }

  /// <summary>
  /// 测试 VisitBlock - 代码块操作
  /// </summary>
  [TestMethod]
  public void Visit_Block_NestedBlockStatement()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    {
                        int x = 5;
                        Console.WriteLine(x);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  {
    let x = 5;
    console.log(x);
  }
}", script);

  }

  [TestMethod]
  public void LegalCSharpParentheses_AreErasedFromOperationTreeAndPreserveInnerSemantics()
  {
    var block = GetBlockOperation("""
            class TestClass
            {
                int TestMethod(int value)
                {
                    int result = (((value + 1)));
                    return result;
                }
            }
            """);

    Assert.IsFalse(block.Descendants().Any(static operation => operation is IParenthesizedOperation));

    var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

    Assert.AreEqual("""
      {
        let result = value + 1;
        return result;
      }
      """.ReplaceLineEndings(), script?.ReplaceLineEndings());
  }

  [TestMethod]
  public void Visit_MethodBody_ExpressionBodyThatNeedsGeneratedTemporaries_MaterializesDeclarationsInsideFunctionBody()
  {
    var methodBody = GetMethodBodyOperation(@"
            class TestClass
            {
                int TestMethod(string input)
                    => int.TryParse(input, out var value) ? value : 0;
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(methodBody, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let value, v$0;");
    StringAssert.Contains(script, "return ");
    Assert.IsFalse(
      script.Contains("return int.TryParse", StringComparison.Ordinal),
      $"Expected the expression-bodied method body to be lowered with materialized temporaries.{Environment.NewLine}{script}");
  }

  [TestMethod]
  public void Visit_ConstructorBody_ExpressionBodyThatNeedsGeneratedTemporaries_MaterializesDeclarationsInsideFunctionBody()
  {
    var constructorBody = GetConstructorBodyOperation(@"
            class TestClass
            {
                int Value;

                TestClass(string input)
                    => Value = int.TryParse(input, out var value) ? value : 0;
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(constructorBody, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let value, v$0;");
    StringAssert.Contains(script, "this.value =");
  }

  /// <summary>
  /// 测试 VisitLabeled - 标签语句操作
  /// </summary>
  [TestMethod]
  public void Visit_LabeledStatement()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    label1:
                        Console.WriteLine(""Labeled statement"");
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  label1: console.log(""Labeled statement"");
}", script);

  }

  /// <summary>
  /// 测试 VisitLabeled - 标签语句应支持语句块目标
  /// </summary>
  [TestMethod]
  public void Visit_LabeledStatement_BlockTarget()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    label1:
                    {
                        Console.WriteLine(""Labeled block"");
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  label1: {
    console.log(""Labeled block"");
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitBranch - Break 操作
  /// </summary>
  [TestMethod]
  public void Visit_Branch_Break()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (i == 5)
                            break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  for (let i = 0; i < 10; i++) {
    if (i === 5)
      break;
  }
}", script);

  }

  /// <summary>
  /// 测试 VisitBranch - Continue 操作
  /// </summary>
  [TestMethod]
  public void Visit_Branch_Continue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (i % 2 == 0)
                            continue;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  for (let i = 0; i < 10; i++) {
    if (i % 2 === 0)
      continue;
  }
}", script);

  }

  /// <summary>
  /// 测试 VisitBranch - Goto 操作应显式拒绝
  /// </summary>
  [TestMethod]
  public void Visit_Branch_Goto_ThrowsOperationTransformationException()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    goto Label1;
                    Label1:
                    Console.WriteLine(""Reached"");
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
    Assert.AreEqual(OperationKind.Branch, exception.Kind);
    StringAssert.Contains(exception.Message ?? string.Empty, "Goto statements are not supported");
  }

  /// <summary>
  /// 测试 VisitEmpty - 空语句操作
  /// </summary>
  [TestMethod]
  public void Visit_EmptyStatement()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    ;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  ;
}", script);

  }

  /// <summary>
  /// 测试 VisitReturn - Return 语句操作
  /// </summary>
  [TestMethod]
  public void Visit_ReturnStatement_WithValue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                int TestMethod()
                {
                    return 42;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  return 42;
}", script);

  }

  /// <summary>
  /// 测试 VisitReturn - Return 无返回值
  /// </summary>
  [TestMethod]
  public void Visit_ReturnStatement_WithoutValue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    return;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  return;
}", script);

  }

  /// <summary>
  /// 测试 VisitLocalFunction - 局部函数操作
  /// </summary>
  [TestMethod]
  public void Visit_LocalFunction()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    void LocalFunction(int param)
                    {
                        Console.WriteLine(param);
                    }
                    LocalFunction(42);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  function LocalFunction(param) {
    console.log(param);
    return;
  }
  LocalFunction(42);
}", script);

  }

  /// <summary>
  /// 测试 VisitLocalFunction - 非 yield 的 IEnumerable 返回值不应被误判为 generator
  /// </summary>
  [TestMethod]
  public void Visit_LocalFunction_IEnumerableReturnWithoutYield_DoesNotBecomeGenerator()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    IEnumerable<int> LocalFunction()
                    {
                        return new int[] { 1, 2 };
                    }

                    var result = LocalFunction();
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  function LocalFunction() {
    return [1, 2];
  }
  let result = LocalFunction();
}", script);
  }

  /// <summary>
  /// 测试 VisitLiteral - 字面量操作
  /// </summary>
  [TestMethod]
  public void Visit_Literal_Integer()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 42;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 42;
}", script);

  }

  /// <summary>
  /// 测试 VisitLiteral - 字符串字面量
  /// </summary>
  [TestMethod]
  public void Visit_Literal_String()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string str = ""Hello"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let str = ""Hello"";
}", script);

  }

  /// <summary>
  /// 测试 VisitLiteral - 布尔字面量
  /// </summary>
  [TestMethod]
  public void Visit_Literal_Boolean()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    bool flag = true;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let flag = true;
}", script);

  }

  /// <summary>
  /// 测试 VisitLiteral - Null 字面量
  /// </summary>
  [TestMethod]
  public void Visit_Literal_Null()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string str = null;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let str = null;
}", script);

  }

  /// <summary>
  /// 测试 VisitConversion - 类型转换操作
  /// </summary>
  [TestMethod]
  public void Visit_Conversion()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double d = 3.14;
                    int i = (int)d;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let d = 3.14;
  let i = d;
}", script);

  }

  [TestMethod]
  public void Visit_Conversion_AsString_EmitsRuntimeTypeGuard()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(object value)
                {
                    string text = value as string;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let text = typeof value === ""string"" ? value : null;
}", script);
  }

  [TestMethod]
  public void Visit_Conversion_AsClass_EmitsNominalRuntimeTypeGuard()
  {
    var block = GetBlockOperation(@"
            [ECMAScript]
            sealed class Customer { }

            class TestClass
            {
                void TestMethod(object value)
                {
                    Customer customer = value as Customer;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let customer = value instanceof Customer ? value : null;
}", script);
  }

  [TestMethod]
  public void Visit_Conversion_AsClass_SideEffectingOperandEvaluatesOnce()
  {
    var block = GetBlockOperation(@"
            [ECMAScript]
            sealed class Customer { }

            class TestClass
            {
                object GetValue() => new Customer();

                void TestMethod()
                {
                    Customer customer = GetValue() as Customer;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let v$0;
  let customer = (v$0 = this.GetValue(), v$0 instanceof Customer ? v$0 : null);
}", script);
  }

  [TestMethod]
  public void Visit_Conversion_AsNullableInt_UsesUnderlyingRuntimeType()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(object value)
                {
                    int? number = value as int?;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let number = typeof value === ""number"" ? value : null;
}", script);
  }

  [TestMethod]
  public void Visit_Conversion_AsNullableDateTime_UsesInferredCarrierGuard()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(object value)
                {
                    DateTime? dateTime = value as DateTime?;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let dateTime = value instanceof JDateTime ? value : null;
}", script);
  }

  [TestMethod]
  public void Visit_Conversion_AsImplicitBaseType_RemainsPassThrough()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(string text)
                {
                    object value = text as object;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let value = text;
}", script);
  }

  /// <summary>
  /// 测试 VisitInvocation - 实例方法调用操作
  /// </summary>
  [TestMethod]
  public void Visit_Invocation_InstanceMethod()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string str = ""Hello"";
                    int length = str.Length;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let str = ""Hello"";
  let length = str.length;
}", script);

  }

  /// <summary>
  /// 测试 VisitInvocation - 静态方法调用操作
  /// </summary>
  [TestMethod]
  public void Visit_Invocation_StaticMethod()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int result = Math.Abs(-5);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let result = Math.abs(-5);
}", script);

  }

  /// <summary>
  /// 测试 VisitConditionalAccess - 条件访问操作（可选链）
  /// </summary>
  [TestMethod]
  public void Visit_ConditionalAccess()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string str = null;
                    int? length = str?.Length;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let str = null;
  let length = str?.length;
}", script);

  }

  [TestMethod]
  public void Visit_VariableDeclaratorWithConditionalAccessSequence_ParenthesizesInitializer()
  {
    var block = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(string? value)
                {
                    var normalized = value?.Trim()?.ToLower();
                }
            }
            """);

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    var body = Assert.IsInstanceOfType<BlockStatement>(node);
    var declaration = Assert.IsInstanceOfType<VariableDeclaration>(body.Body.Last());
    var initializer = declaration.Declarations[0].Init;
    Assert.IsInstanceOfType<SequenceExpression>(initializer);
    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let normalized = (");
    _ = new Parser().ParseScript(script);
  }

  /// <summary>
  /// 测试 VisitUnaryOperator - 一元运算符操作
  /// </summary>
  [TestMethod]
  public void Visit_UnaryOperator_LogicalNot()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    bool flag = true;
                    bool result = !flag;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let flag = true;
  let result = !flag;
}", script);

  }

  /// <summary>
  /// 测试 VisitUnaryOperator - 负号运算符
  /// </summary>
  [TestMethod]
  public void Visit_UnaryOperator_Negation()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    int result = -x;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 5;
  let result = -x;
}", script);

  }

  /// <summary>
  /// 测试 VisitBinaryOperator - 二元运算符操作
  /// </summary>
  [TestMethod]
  public void Visit_BinaryOperator_Addition()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 5;
                    int b = 3;
                    int result = a + b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = 5;
  let b = 3;
  let result = a + b;
}", script);

  }

  /// <summary>
  /// 测试 VisitBinaryOperator - 逻辑与运算符
  /// </summary>
  [TestMethod]
  public void Visit_BinaryOperator_LogicalAnd()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    bool a = true;
                    bool b = false;
                    bool result = a && b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = true;
  let b = false;
  let result = a && b;
}", script);

  }

  /// <summary>
  /// 测试 VisitConditional - 三元运算符操作
  /// </summary>
  [TestMethod]
  public void Visit_Conditional_Ternary()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    bool condition = true;
                    int result = condition ? 1 : 0;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let condition = true;
  let result = condition ? 1 : 0;
}", script);

  }

  /// <summary>
  /// 测试 VisitCoalesce - 空合并运算符操作
  /// </summary>
  [TestMethod]
  public void Visit_Coalesce()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string str = null;
                    string result = str ?? ""default"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let str = null;
  let result = str ?? ""default"";
}", script);

  }

  /// <summary>
  /// 测试 VisitAnonymousFunction - Lambda 表达式操作
  /// </summary>
  [TestMethod]
  public void Visit_AnonymousFunction_Lambda()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var func = (int x, int y) => x + y;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let func = (x, y) => {
    return x + y;
  };
}", script);

  }

  /// <summary>
  /// 测试 VisitAnonymousFunction - Async Lambda 需要保留 async 语义
  /// </summary>
  [TestMethod]
  public void Visit_AnonymousFunction_AsyncLambda_PreservesAsyncModifier()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    System.Func<System.Threading.Tasks.Task> func = async () =>
                    {
                        await System.Threading.Tasks.Task.Delay(1);
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    var declaration = GetOperationAt<IVariableDeclarationGroupOperation>(block);
    var delegateCreation = declaration.Declarations.Single()
      .Declarators.Single()
      .Initializer?.Value as IDelegateCreationOperation;
    Assert.IsNotNull(delegateCreation);

    var anonymousFunction = delegateCreation.Target as IAnonymousFunctionOperation;
    Assert.IsNotNull(anonymousFunction);

    var awaitOperation = anonymousFunction.Body.Operations
      .OfType<IExpressionStatementOperation>()
      .Single();
    Assert.IsNotNull(awaitOperation);
    var awaited = awaitOperation.Operation as IAwaitOperation;
    Assert.IsNotNull(awaited);
    var invocation = awaited.Operation as IInvocationOperation;
    Assert.IsNotNull(invocation);
    AssertScriptEqual(@"{
  let func = async () => {
    await (1 === -1 ? new Promise(() => { }) : new Promise(resolve => setTimeout(resolve, 1)));
    return;
  };
}", script);
  }

  /// <summary>
  /// 测试 VisitAwait - Await 表达式操作
  /// </summary>
  [TestMethod]
  public void Visit_Await()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                async System.Threading.Tasks.Task TestMethod()
                {
                    await System.Threading.Tasks.Task.Delay(100);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    var statement = GetOperationAt<IExpressionStatementOperation>(block);
    var awaitOperation = statement.Operation as IAwaitOperation;
    Assert.IsNotNull(awaitOperation);
    var invocation = awaitOperation.Operation as IInvocationOperation;
    Assert.IsNotNull(invocation);
    AssertScriptEqual(@"{
  await (100 === -1 ? new Promise(() => { }) : new Promise(resolve => setTimeout(resolve, 100)));
}", script);

  }

  /// <summary>
  /// 测试 Task.WhenAll 在 await 下会映射到 Promise.all
  /// </summary>
  [TestMethod]
  public void Visit_Await_TaskWhenAll_UsesPromiseAll()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                async System.Threading.Tasks.Task TestMethod(
                    System.Threading.Tasks.Task first,
                    System.Threading.Tasks.Task second)
                {
                    await System.Threading.Tasks.Task.WhenAll(first, second);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  await Promise.all([first, second]);
}", script);
  }

  /// <summary>
  /// 测试 Task.WhenAny 在 await 下会映射到 Promise.race 包装
  /// </summary>
  [TestMethod]
  public void Visit_Await_TaskWhenAny_UsesPromiseRaceWrapper()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                async System.Threading.Tasks.Task TestMethod(
                    System.Threading.Tasks.Task first,
                    System.Threading.Tasks.Task second)
                {
                    await System.Threading.Tasks.Task.WhenAny(first, second);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  await Promise.race([first, second].map(task => Promise.resolve(task).then(() => task, () => task)));
}", script);
  }

  /// <summary>
  /// 测试 Task.ConfigureAwait 在 await 下会映射为 Promise.resolve
  /// </summary>
  [TestMethod]
  public void Visit_Await_TaskConfigureAwait_UsesPromiseResolve()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                async System.Threading.Tasks.Task TestMethod(System.Threading.Tasks.Task first)
                {
                    await first.ConfigureAwait(false);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  await Promise.resolve(first);
}", script);
  }

  /// <summary>
  /// 测试 Task.WaitAsync(TimeSpan) 在 await 下会映射为 Promise.race 超时语义
  /// </summary>
  [TestMethod]
  public void Visit_Await_TaskWaitAsyncTimeSpan_UsesPromiseRaceTimeout()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                async System.Threading.Tasks.Task TestMethod(
                    System.Threading.Tasks.Task first,
                    System.TimeSpan timeout)
                {
                    await first.WaitAsync(timeout);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "await Promise.race([Promise.resolve(first),", StringComparison.Ordinal);
    StringAssert.Contains(script, "timeout.ticks === -10000n ? new Promise(() => { })", StringComparison.Ordinal);
    StringAssert.Contains(script, "reject(new Error(\"TimeoutException\"))", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Task.ContinueWith(Action&lt;Task&gt;) 会映射为 Promise.then 双分支回调
  /// </summary>
  [TestMethod]
  public void Visit_TaskContinueWithAction_UsesPromiseThen()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                System.Threading.Tasks.Task TestMethod(
                    System.Threading.Tasks.Task first,
                    System.Action<System.Threading.Tasks.Task> continuation)
                {
                    return first.ContinueWith(continuation);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  return Promise.resolve(first).then(() => continuation(first), () => continuation(first));
}", script);
  }

  /// <summary>
  /// 测试 DateTime 到 DateTimeOffset 的隐式转换会绑定到 DateTimeOffsetModule helper
  /// </summary>
  [TestMethod]
  public void Visit_Conversion_DateTimeToDateTimeOffset_UsesHelper()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    DateTime value = DateTime.Now;
                    DateTimeOffset offset = value;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let value = _ee9dd166a34a2fa5();
  let offset = _31bbd12ed57f4f76(value);
}", script);
  }

  /// <summary>
  /// 测试 TimeSpan 一元负号会绑定到 TimeSpanModule helper
  /// </summary>
  [TestMethod]
  public void Visit_UnaryOperator_TimeSpanNegation_UsesTimeSpanHelper()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    TimeSpan value = new TimeSpan(1, 2, 3);
                    TimeSpan neg = -value;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let value = _6f22e268aec62fe7(1, 2, 3);
  let neg = _e8e884a7b14ce4b4(value);
}", script);
  }

  /// <summary>
  /// 测试 TimeSpan 复合赋值会绑定到 TimeSpanModule helper，而不是静默退化成原生 +=
  /// </summary>
  [TestMethod]
  public void Visit_CompoundAssignment_TimeSpan_UsesTimeSpanHelper()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    TimeSpan value = new TimeSpan(1, 2, 3);
                    TimeSpan other = new TimeSpan(0, 1, 0);
                    value += other;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let value = _6f22e268aec62fe7(1, 2, 3);
  let other = _6f22e268aec62fe7(0, 1, 0);
  value = _24670e70abc0feb8(value, other);
}", script);
  }

  /// <summary>
  /// 测试 Task.ContinueWith&lt;TResult&gt;(Func&lt;Task, object, TResult&gt;, state) 会映射为 Promise.then
  /// </summary>
  [TestMethod]
  public void Visit_TaskContinueWithFuncState_UsesPromiseThen()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                System.Threading.Tasks.Task<int> TestMethod(
                    System.Threading.Tasks.Task first,
                    System.Func<System.Threading.Tasks.Task, object, int> continuation,
                    object state)
                {
                    return first.ContinueWith<int>(continuation, state);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  return Promise.resolve(first).then(() => continuation(first, state), () => continuation(first, state));
}", script);
  }

  /// <summary>
  /// 测试 Task.WaitAll(params Task[]) 会映射到 Promise.all
  /// </summary>
  [TestMethod]
  public void Visit_TaskWaitAll_UsesPromiseAll()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(
                    System.Threading.Tasks.Task first,
                    System.Threading.Tasks.Task second)
                {
                    System.Threading.Tasks.Task.WaitAll(first, second);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  Promise.all([first, second]);
}", script);
  }

  /// <summary>
  /// 测试 Task.WaitAll(Task[], TimeSpan) 会映射为基于 ticks 的 Promise.race(true/false)
  /// </summary>
  [TestMethod]
  public void Visit_TaskWaitAll_WithTimeSpanTimeout_UsesTickBasedTimeoutRace()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(
                    System.Threading.Tasks.Task first,
                    System.Threading.Tasks.Task second,
                    System.TimeSpan timeout)
                {
                    var completed = System.Threading.Tasks.Task.WaitAll(new[] { first, second }, timeout);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let completed = Promise.race([Promise.all([first, second]).then(() => true),", StringComparison.Ordinal);
    StringAssert.Contains(script, "timeout.ticks === -10000n ? new Promise(() => { })", StringComparison.Ordinal);
    StringAssert.Contains(script, "setTimeout(() => resolve(false), Number(timeout.ticks / 10000n))", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Task.WaitAny(params Task[]) 会映射到返回索引的 Promise.race
  /// </summary>
  [TestMethod]
  public void Visit_TaskWaitAny_UsesPromiseRaceIndex()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(
                    System.Threading.Tasks.Task first,
                    System.Threading.Tasks.Task second)
                {
                    var index = System.Threading.Tasks.Task.WaitAny(first, second);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let index = Promise.race(Array.from([first, second]).map((task, index) => Promise.resolve(task).then(() => index, () => index)));
}", script);
  }

  /// <summary>
  /// 测试 Task.WaitAny(Task[], int) 会在超时时返回 -1（Promise 语义）
  /// </summary>
  [TestMethod]
  public void Visit_TaskWaitAny_WithMillisecondsTimeout_UsesMinusOneFallback()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(
                    System.Threading.Tasks.Task first,
                    System.Threading.Tasks.Task second)
                {
                    var index = System.Threading.Tasks.Task.WaitAny(new[] { first, second }, 100);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let index = Promise.race([Promise.race(Array.from([first, second]).map((task, index) => Promise.resolve(task).then(() => index, () => index))),", StringComparison.Ordinal);
    StringAssert.Contains(script, "setTimeout(() => resolve(-1), 100)", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Task.WaitAny(Task[], TimeSpan) 会映射为基于 ticks 的 Promise.race(-1 回退)
  /// </summary>
  [TestMethod]
  public void Visit_TaskWaitAny_WithTimeSpanTimeout_UsesTickBasedMinusOneFallback()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(
                    System.Threading.Tasks.Task first,
                    System.Threading.Tasks.Task second,
                    System.TimeSpan timeout)
                {
                    var index = System.Threading.Tasks.Task.WaitAny(new[] { first, second }, timeout);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let index = Promise.race([Promise.race(Array.from([first, second]).map((task, index) => Promise.resolve(task).then(() => index, () => index))),", StringComparison.Ordinal);
    StringAssert.Contains(script, "timeout.ticks === -10000n ? new Promise(() => { })", StringComparison.Ordinal);
    StringAssert.Contains(script, "setTimeout(() => resolve(-1), Number(timeout.ticks / 10000n))", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Task.WhenEach(params Task[]) 会映射为异步生成器 + Promise.race
  /// </summary>
  [TestMethod]
  public void Visit_TaskWhenEach_UsesAsyncGeneratorRace()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(
                    System.Threading.Tasks.Task first,
                    System.Threading.Tasks.Task second)
                {
                    var sequence = System.Threading.Tasks.Task.WhenEach(first, second);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let sequence = (__jz_arg0 => async function*() {", StringComparison.Ordinal);
    StringAssert.Contains(script, "const pending = Array.from(__jz_arg0);", StringComparison.Ordinal);
    StringAssert.Contains(script, "}())([first, second]);", StringComparison.Ordinal);
    StringAssert.Contains(script, "const settled = await Promise.race(", StringComparison.Ordinal);
    StringAssert.Contains(script, "yield settled.task;", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Task.FromException/FromCanceled 会映射到 Promise.reject
  /// </summary>
  [TestMethod]
  public void Visit_TaskFromExceptionAndFromCanceled_UsePromiseReject()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(System.Exception ex, System.Threading.CancellationToken cancellationToken)
                {
                    var faulted = System.Threading.Tasks.Task.FromException(ex);
                    var canceled = System.Threading.Tasks.Task.FromCanceled(cancellationToken);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let faulted = Promise.reject(ex);
  let canceled = Promise.reject(new Error(""TaskCanceledException""));
}", script);
  }

  /// <summary>
  /// 测试 Task.CompletedTask / Task.Yield 会映射为 Promise.resolve()
  /// </summary>
  [TestMethod]
  public void Visit_TaskCompletedTaskAndYield_UsePromiseResolve()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var completed = System.Threading.Tasks.Task.CompletedTask;
                    var yielded = System.Threading.Tasks.Task.Yield();
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let completed = Promise.resolve();
  let yielded = Promise.resolve();
}", script);
  }

  /// <summary>
  /// 测试 new Task(Action) 会映射为延迟启动任务（等待 Start/RunSynchronously 触发）
  /// </summary>
  [TestMethod]
  public void Visit_TaskCtor_Action_UsesPromiseThen()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                System.Threading.Tasks.Task TestMethod(System.Action action)
                {
                    return new System.Threading.Tasks.Task(action);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "globalThis.__jazorTaskStarters ??= new WeakMap", StringComparison.Ordinal);
    StringAssert.Contains(script, "const entry = { started: false, start: null };", StringComparison.Ordinal);
    StringAssert.Contains(script, "Promise.resolve().then(() => action()).then(resolve, reject);", StringComparison.Ordinal);
    StringAssert.Contains(script, "return task;", StringComparison.Ordinal);
    Assert.IsFalse(script.Contains("Promise.resolve().then(action)", StringComparison.Ordinal));
  }

  /// <summary>
  /// 测试 new Task(Action&lt;object&gt;, state) 会映射为延迟启动任务并保留 AsyncState
  /// </summary>
  [TestMethod]
  public void Visit_TaskCtor_ActionWithState_UsesPromiseThen()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                System.Threading.Tasks.Task TestMethod(System.Action<object> action, object state)
                {
                    return new System.Threading.Tasks.Task(action, state);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "globalThis.__jazorTaskStarters ??= new WeakMap", StringComparison.Ordinal);
    StringAssert.Contains(script, "Promise.resolve().then(() => action(state)).then(resolve, reject);", StringComparison.Ordinal);
    StringAssert.Contains(script, "__jazorTaskAsyncStates ??= new WeakMap", StringComparison.Ordinal);
    StringAssert.Contains(script, ".set(task, state)", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 new Task(Action&lt;object&gt;, state) 会将 AsyncState 写入任务状态表
  /// </summary>
  [TestMethod]
  public void Visit_TaskCtor_ActionWithState_WritesAsyncState()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(System.Action<object> action, object state)
                {
                    var task = new System.Threading.Tasks.Task(action, state);
                    var asyncState = task.AsyncState;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let task = (() => {", StringComparison.Ordinal);
    StringAssert.Contains(script, "globalThis.__jazorTaskStarters ??= new WeakMap", StringComparison.Ordinal);
    StringAssert.Contains(script, "Promise.resolve().then(() => action(state)).then(resolve, reject);", StringComparison.Ordinal);
    StringAssert.Contains(script, "__jazorTaskAsyncStates ??= new WeakMap", StringComparison.Ordinal);
    StringAssert.Contains(script, ".set(task, state)", StringComparison.Ordinal);
    StringAssert.Contains(script, "let asyncState = globalThis.__jazorTaskAsyncStates?.get(task) ?? globalThis.__jazorTaskStates?.get(task)?.asyncState ?? null;", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Task.Start / RunSynchronously 会触发延迟任务启动器
  /// </summary>
  [TestMethod]
  public void Visit_TaskStartAndRunSynchronously_UseDeferredStarter()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(System.Threading.Tasks.Task first)
                {
                    first.Start();
                    first.RunSynchronously();
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    const string starterLookup = "globalThis.__jazorTaskStarters?.get(first)";
    var firstIndex = script.IndexOf(starterLookup, StringComparison.Ordinal);
    Assert.IsTrue(firstIndex >= 0);
    var secondIndex = script.IndexOf(starterLookup, firstIndex + 1, StringComparison.Ordinal);
    Assert.IsTrue(secondIndex > firstIndex);
    Assert.IsFalse(script.Contains("Promise.resolve(first)", StringComparison.Ordinal));
    StringAssert.Contains(script, "if (entry && entry.start)", StringComparison.Ordinal);
    StringAssert.Contains(script, "entry.start();", StringComparison.Ordinal);
    StringAssert.Contains(script, "globalThis.__jazorTaskStates?.get(first)", StringComparison.Ordinal);
    StringAssert.Contains(script, "state.status === \"created\"", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 task.Wait() 会映射到 Promise.resolve(task)
  /// </summary>
  [TestMethod]
  public void Visit_TaskWait_UsesPromiseResolve()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(System.Threading.Tasks.Task first)
                {
                    first.Wait();
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  Promise.resolve(first);
}", script);
  }

  /// <summary>
  /// 测试 task.Wait(int) 会映射为 Promise.race(true/false)
  /// </summary>
  [TestMethod]
  public void Visit_TaskWait_WithMillisecondsTimeout_UsesPromiseRaceBool()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(System.Threading.Tasks.Task first)
                {
                    var completed = first.Wait(100);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let completed = Promise.race([Promise.resolve(first).then(() => true),", StringComparison.Ordinal);
    StringAssert.Contains(script, "100 === -1 ? new Promise(() => { })", StringComparison.Ordinal);
    StringAssert.Contains(script, "setTimeout(() => resolve(false), 100)", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 task.Wait(TimeSpan) 会映射为基于 ticks 的 Promise.race(true/false)
  /// </summary>
  [TestMethod]
  public void Visit_TaskWait_WithTimeSpanTimeout_UsesPromiseRaceBool()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(System.Threading.Tasks.Task first, System.TimeSpan timeout)
                {
                    var completed = first.Wait(timeout);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let completed = Promise.race([Promise.resolve(first).then(() => true),", StringComparison.Ordinal);
    StringAssert.Contains(script, "timeout.ticks === -10000n ? new Promise(() => { })", StringComparison.Ordinal);
    StringAssert.Contains(script, "Number(timeout.ticks / 10000n)", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 task.GetAwaiter() 会映射到 Promise.resolve(task)
  /// </summary>
  [TestMethod]
  public void Visit_TaskGetAwaiter_UsesPromiseResolve()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(System.Threading.Tasks.Task first)
                {
                    var awaiter = first.GetAwaiter();
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let awaiter = Promise.resolve(first);
}", script);
  }

  /// <summary>
  /// 测试 Task&lt;T&gt;.GetAwaiter() 会映射到 Promise.resolve(task)
  /// </summary>
  [TestMethod]
  public void Visit_TaskOfTGetAwaiter_UsesPromiseResolve()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(System.Threading.Tasks.Task<int> first)
                {
                    var awaiter = first.GetAwaiter();
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let awaiter = Promise.resolve(first);
}", script);
  }

  /// <summary>
  /// 测试 await Task&lt;T&gt;.ConfigureAwait(false) 会映射到 await Promise.resolve(task)
  /// </summary>
  [TestMethod]
  public void Visit_Await_TaskOfTConfigureAwait_UsesPromiseResolve()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                async System.Threading.Tasks.Task TestMethod(System.Threading.Tasks.Task<int> first)
                {
                    await first.ConfigureAwait(false);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  await Promise.resolve(first);
}", script);
  }

  /// <summary>
  /// 测试 await Task&lt;T&gt;.ConfigureAwait(options) 会映射到 await Promise.resolve(task)
  /// </summary>
  [TestMethod]
  public void Visit_Await_TaskOfTConfigureAwaitOptions_UsesPromiseResolve()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                async System.Threading.Tasks.Task TestMethod(
                    System.Threading.Tasks.Task<int> first,
                    System.Threading.Tasks.ConfigureAwaitOptions options)
                {
                    await first.ConfigureAwait(options);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  await Promise.resolve(first);
}", script);
  }

  /// <summary>
  /// 测试 await Task&lt;T&gt;.WaitAsync(TimeSpan) 会映射为 Promise.race 超时语义
  /// </summary>
  [TestMethod]
  public void Visit_Await_TaskOfTWaitAsyncTimeSpan_UsesPromiseRaceTimeout()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                async System.Threading.Tasks.Task TestMethod(
                    System.Threading.Tasks.Task<int> first,
                    System.TimeSpan timeout)
                {
                    await first.WaitAsync(timeout);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "await Promise.race([Promise.resolve(first),", StringComparison.Ordinal);
    StringAssert.Contains(script, "timeout.ticks === -10000n ? new Promise(() => { })", StringComparison.Ordinal);
    StringAssert.Contains(script, "reject(new Error(\"TimeoutException\"))", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 await Task&lt;T&gt;.WaitAsync(CancellationToken) 会映射到 Promise.resolve(task)
  /// </summary>
  [TestMethod]
  public void Visit_Await_TaskOfTWaitAsyncCancellationToken_UsesPromiseResolve()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                async System.Threading.Tasks.Task TestMethod(
                    System.Threading.Tasks.Task<int> first,
                    System.Threading.CancellationToken cancellationToken)
                {
                    await first.WaitAsync(cancellationToken);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  await Promise.resolve(first);
}", script);
  }

  /// <summary>
  /// 测试 await Task&lt;T&gt;.WaitAsync(TimeSpan, TimeProvider) 会映射为 Promise.race 超时语义
  /// </summary>
  [TestMethod]
  public void Visit_Await_TaskOfTWaitAsyncTimeSpanWithTimeProvider_UsesPromiseRaceTimeout()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                async System.Threading.Tasks.Task TestMethod(
                    System.Threading.Tasks.Task<int> first,
                    System.TimeSpan timeout,
                    System.TimeProvider timeProvider)
                {
                    await first.WaitAsync(timeout, timeProvider);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "await Promise.race([Promise.resolve(first),", StringComparison.Ordinal);
    StringAssert.Contains(script, "timeout.ticks === -10000n ? new Promise(() => { })", StringComparison.Ordinal);
    StringAssert.Contains(script, "reject(new Error(\"TimeoutException\"))", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Task&lt;TResult&gt;.Result 被 Discard 映射拒绝
  /// </summary>
  [TestMethod]
  public void Visit_TaskOfTResult_Discard_Throws()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                int TestMethod(System.Threading.Tasks.Task<int> first)
                {
                    return first.Result;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
  }

  /// <summary>
  /// 测试 task.Dispose() 会降级为 no-op
  /// </summary>
  [TestMethod]
  public void Visit_TaskDispose_UsesUndefinedNoOp()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(System.Threading.Tasks.Task first)
                {
                    first.Dispose();
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  undefined;
}", script);
  }

  /// <summary>
  /// 测试 Global.Undefined&lt;T&gt; 会稳定降级为 JavaScript undefined 字面量
  /// </summary>
  [TestMethod]
  public void Visit_Invocation_GlobalUndefined_InlinesUndefinedLiteral()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                string? TestMethod()
                {
                    string? value = Undefined<string?>();
                    return value;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let value = undefined;
  return value;
}", script);
  }

  /// <summary>
  /// 测试 Task 状态属性会映射到 Promise 状态跟踪 helper
  /// </summary>
  [TestMethod]
  public void Visit_TaskStateProperties_UseStateTrackingHelper()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(System.Threading.Tasks.Task first)
                {
                    var id = first.Id;
                    var status = first.Status;
                    var isCanceled = first.IsCanceled;
                    var isCompleted = first.IsCompleted;
                    var isCompletedSuccessfully = first.IsCompletedSuccessfully;
                    var isFaulted = first.IsFaulted;
                    var exception = first.Exception;
                    var creationOptions = first.CreationOptions;
                    var asyncState = first.AsyncState;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let id = (globalThis.__jazorTaskEnsureState ??=", StringComparison.Ordinal);
    StringAssert.Contains(script, "let status = (s => s.status === \"fulfilled\" ? 5", StringComparison.Ordinal);
    StringAssert.Contains(script, "s.status === \"created\" ? 0", StringComparison.Ordinal);
    StringAssert.Contains(script, "let isCanceled = (s => s.status === \"rejected\" &&", StringComparison.Ordinal);
    StringAssert.Contains(script, "let isCompleted = (s => s.status === \"fulfilled\" || s.status === \"rejected\")", StringComparison.Ordinal);
    StringAssert.Contains(script, "let isCompletedSuccessfully = (globalThis.__jazorTaskEnsureState ??=", StringComparison.Ordinal);
    StringAssert.Contains(script, "let isFaulted = (s => s.status === \"rejected\" &&", StringComparison.Ordinal);
    StringAssert.Contains(script, "let exception = (s => s.status === \"rejected\" ? s.error : null)", StringComparison.Ordinal);
    StringAssert.Contains(script, "const starterEntry = globalThis.__jazorTaskStarters?.get(task);", StringComparison.Ordinal);
    StringAssert.Contains(script, "let creationOptions = 0;", StringComparison.Ordinal);
    StringAssert.Contains(script, "let asyncState = globalThis.__jazorTaskAsyncStates?.get(first) ?? globalThis.__jazorTaskStates?.get(first)?.asyncState ?? null;", StringComparison.Ordinal);
  }

  /// <summary>
  /// 测试 Task.CurrentId / Task.Factory 会映射为 null 回退
  /// </summary>
  [TestMethod]
  public void Visit_TaskStaticProperties_UseNullFallback()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var currentId = System.Threading.Tasks.Task.CurrentId;
                    var factory = System.Threading.Tasks.Task.Factory;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let currentId = null;
  let factory = null;
}", script);
  }

  /// <summary>
  /// 测试 VisitSimpleAssignment - 简单赋值操作
  /// </summary>
  [TestMethod]
  public void Visit_SimpleAssignment()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    x = 10;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 5;
  x = 10;
}", script);

  }

  /// <summary>
  /// 测试 VisitCompoundAssignment - 复合赋值操作
  /// </summary>
  [TestMethod]
  public void Visit_CompoundAssignment_Addition()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    x += 3;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 5;
  x += 3;
}", script);

  }

  /// <summary>
  /// 测试 VisitCoalesceAssignment - 空合并赋值操作
  /// </summary>
  [TestMethod]
  public void Visit_CoalesceAssignment()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string name = null;
                    name ??= ""Default"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let name = null;
  name ??= ""Default"";
}", script);

  }

  /// <summary>
  /// 测试 VisitNameOf - NameOf 表达式操作
  /// </summary>
  [TestMethod]
  public void Visit_NameOf()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string name = nameof(TestMethod);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToECMAScript();

    Assert.AreEqual(@"{let name=""TestMethod""}", script);
  }

  [TestMethod]
  public void Visit_NameOf_PropertyAndTypeMember()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                int Count { get; set; }

                void TestMethod()
                {
                    string propertyName = nameof(Count);
                    string nestedTypeName = nameof(System.Collections.Generic.List<int>.Count);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let propertyName = ""Count"";
  let nestedTypeName = ""Count"";
}", script);
  }

  [TestMethod]
  public void Visit_CheckedUncheckedExpression_ErasesOverflowContext()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 2147483647;
                    int checkedResult = checked(value + 1);
                    int uncheckedResult = unchecked(value + 1);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 2147483647;
  let checkedResult = value + 1;
  let uncheckedResult = value + 1;
}", script);
  }

  /// <summary>
  /// 测试 VisitDefaultValue - 默认值操作
  /// </summary>
  [TestMethod]
  public void Visit_DefaultValue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = default(int);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToECMAScript();

    Assert.AreEqual(@"{let x=0}", script);
  }

  [TestMethod]
  public void Visit_DefaultValue_SpecialValueTypes()
  {
    var block = GetBlockOperation(@"
            using System;
            using System.Numerics;

            class TestClass
            {
                void TestMethod()
                {
                    DateTime dt = default(DateTime);
                    DateTimeOffset dto = default(DateTimeOffset);
                    DateOnly day = default(DateOnly);
                    TimeOnly time = default(TimeOnly);
                    TimeSpan span = default(TimeSpan);
                    BigInteger big = default(BigInteger);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let dt = _bfa8ee5dd46e2005();
  let dto = _12b4f3f1dc14bea9();
  let day = _5f8053a9657a0844();
  let time = _9f78f92d0753f4cf();
  let span = _5af0f6ad850e6702();
  let big = 0n;
}", script);
  }

  [TestMethod]
  public void Visit_DefaultValue_TupleValueType()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    (int Count, string Name, (bool Flag, long Total) Meta) tuple = default((int Count, string Name, (bool Flag, long Total) Meta));
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let tuple = {
    Count: 0,
    Name: null,
    Meta: { Flag: false, Total: 0n }
  };
}", script);
  }

  [TestMethod]
  public void Visit_DefaultValue_Guid()
  {
    var block = GetBlockOperation(@"
            using System;

            class TestClass
            {
                void TestMethod()
                {
                    Guid id = default(Guid);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let id = _0e58e51018e846d2();
}", script);
  }

  [TestMethod]
  public void Visit_DefaultValue_CustomStruct_Throws()
  {
    var block = GetBlockOperation(@"
            public struct Counter
            {
                public int Value;
            }

            class TestClass
            {
                void TestMethod()
                {
                    Counter counter = default(Counter);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
  }

  [TestMethod]
  public void Visit_DefaultValue_UnsupportedExternalConcreteReferenceType_Throws()
  {
    var block = GetBlockOperation(@"
            using System;

            class TestClass
            {
                void TestMethod()
                {
                    Uri uri = default(Uri);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
  }

  /// <summary>
  /// 测试 VisitIncrementOrDecrement - 递增操作
  /// </summary>
  [TestMethod]
  public void Visit_IncrementOrDecrement_Increment()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    x++;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 5;
  x++;
}", script);
  }

  /// <summary>
  /// 测试 VisitIncrementOrDecrement - 前缀递增
  /// </summary>
  [TestMethod]
  public void Visit_IncrementOrDecrement_PrefixIncrement()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    ++x;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 5;
  ++x;
}", script);

  }

  /// <summary>
  /// 测试 VisitWith - With 表达式操作
  /// </summary>
  [TestMethod]
  public void Visit_WithExpression()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var person = new { Name = ""John"", Age = 30 };
                    var newPerson = person with { Name = ""Jane"" };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let person = { Name: ""John"", Age: 30 };
  let newPerson = { ...person, Name: ""Jane"" };
}", script);

  }

  [TestMethod]
  public void Visit_WithExpression_TupleRemapByTargetType()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                record Person((string first, int years) Info);

                void TestMethod()
                {
                    var person = new Person((first: ""John"", years: 30));
                    var newPerson = person with { Info = (name: ""Jane"", age: 40) };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

  AssertScriptEqual(@"{
  let person = { info: { first: ""John"", years: 30 } };
  let newPerson = { ...person, Info: { first: ""Jane"", years: 40 } };
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
  }

  /// <summary>
  /// 测试 VisitCollectionExpression - 集合表达式操作
  /// </summary>
  [TestMethod]
  public void Visit_CollectionExpression()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let array = [1, 2, 3, 4, 5];
}", script);

  }

  [TestMethod]
  public void Visit_CollectionExpression_TupleElements_RemapNames()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    (string first, int years)[] array = [(name: ""John"", age: 30)];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [{ first: ""John"", years: 30 }];
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
  }

  [TestMethod]
  public void Visit_CollectionExpression_ErasedUnsupportedElementType_Allows()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    List<Random> list = [];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let list = [];
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
  }

  /// <summary>
  /// 测试 VisitSpread - 展开操作
  /// </summary>
  [TestMethod]
  public void Visit_Spread()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array1 = [1, 2, 3];
                    int[] array2 = [4, 5, 6];
                    int[] combined = [..array1, ..array2];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array1 = [1, 2, 3];
  let array2 = [4, 5, 6];
  let combined = [...array1, ...array2];
}", script);

  }

  /// <summary>
  /// 整体测试 - 复杂的表达式和语句组合
  /// </summary>
  [TestMethod]
  public void Visit_ComplexExpressionBlock()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    // 字面量和变量
                    int x = 42;
                    string str = ""Hello"";
                    bool flag = true;
                    
                    // 一元运算符
                    int negX = -x;
                    bool notFlag = !flag;
                    
                    // 二元运算符
                    int sum = x + 10;
                    bool result = flag && !notFlag;
                    
                    // 三元运算符
                    int value = result ? 1 : 0;
                    
                    // 空合并运算符
                    string? nullableStr = null;
                    string finalStr = nullableStr ?? ""default"";
                    
                    // 空合并赋值
                    string name = null;
                    name ??= ""Default"";
                    
                    // 复合赋值
                    x += 5;
                    x -= 3;
                    
                    // 递增递减
                    x++;
                    --x;
                    
                    // Lambda 表达式
                    var func = (int a, int b) => a + b;
                    
                    // 集合表达式
                    int[] array = [1, 2, 3, 4, 5];
                    
                    // 展开操作
                    int[] array2 = [..array, 6, 7];
                    
                    // 默认值
                    int defaultVal = default(int);
                    
                    // NameOf
                    string methodName = nameof(TestMethod);
                    
                    // 类型转换
                    double d = 3.14;
                    int i = (int)d;
                    
                    // 条件访问
                    string? testStr = null;
                    int? length = testStr?.Length;
                    
                    // 返回语句
                    return;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let x = 42;
  let str = ""Hello"";
  let flag = true;
  let negX = -x;
  let notFlag = !flag;
  let sum = x + 10;
  let result = flag && !notFlag;
  let value = result ? 1 : 0;
  let nullableStr = null;
  let finalStr = nullableStr ?? ""default"";
  let name = null;
  name ??= ""Default"";
  x += 5;
  x -= 3;
  x++;
  --x;
  let func = (a, b) => {
    return a + b;
  };
  let array = [1, 2, 3, 4, 5];
  let array2 = [...array, 6, 7];
  let defaultVal = 0;
  let methodName = ""TestMethod"";
  let d = 3.14;
  let i = d;
  let testStr = null;
  let length = testStr?.length;
  return;
}", script);

  }

  /// <summary>
  /// 整体测试 - 方法和函数调用
  /// </summary>
  [TestMethod]
  public void Visit_MethodAndFunctionCalls()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    // 静态方法调用
                    int absValue = Math.Abs(-5);
                    
                    // 实例方法调用
                    string text = ""Hello World"";
                    string upperText = text.ToUpper();
                    
                    // 局部函数
                    void LocalFunction(int param)
                    {
                        Console.WriteLine(param);
                    }
                    LocalFunction(42);
                    
                    // Lambda 调用
                    var add = (int a, int b) => a + b;
                    int result = add(3, 4);
                    
                    // 带参数的方法调用
                    string sub = text.Substring(0, 5);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let absValue = Math.abs(-5);
  let text = ""Hello World"";
  let upperText = text.toUpperCase();
  function LocalFunction(param) {
    console.log(param);
    return;
  }
  LocalFunction(42);
  let add = (a, b) => {
    return a + b;
  };
  let result = add(3, 4);
  let sub = text.substring(0, 0 + 5);
}", script);

  }

  /// <summary>
  /// 整体测试 - 控制流语句
  /// </summary>
  [TestMethod]
  public void Visit_ControlFlowStatements()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    // 标签和跳转
                    label1:
                        Console.WriteLine(""Label1"");
                    
                    // 空语句
                    ;
                    
                    // 带标签的跳转（简化测试）
                    for (int i = 0; i < 10; i++)
                    {
                        if (i == 5)
                            break;
                        if (i % 2 == 0)
                            continue;
                    }
                    
                    // 返回语句
                    return;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(
@"{
  label1: console.log(""Label1"");
  ;
  for (let i = 0; i < 10; i++) {
    if (i === 5)
      break;
    if (i % 2 === 0)
      continue;
  }
  return;
}", script);

  }

  /// <summary>
  /// 整体测试 - 对象和属性操作
  /// </summary>
  [TestMethod]
  public void Visit_ObjectAndPropertyOperations()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    // 匿名对象
                    var person = new { Name = ""John"", Age = 30 };
                    
                    // With 表达式
                    var newPerson = person with { Name = ""Jane"", Age = 25 };
                    
                    // 对象属性访问
                    string name = person.Name;
                    int age = person.Age;
                    
                    // 属性赋值
                    var obj = new { Value = 10 };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let person = { Name: ""John"", Age: 30 };
  let newPerson = {
    ...person,
    Name: ""Jane"",
    Age: 25
  };
  let name = person.Name;
  let age = person.Age;
  let obj = { Value: 10 };
}", script);

  }

  #region 复合赋值运算符完整测试

  /// <summary>
  /// 测试 VisitCompoundAssignment - 减法赋值
  /// </summary>
  [TestMethod]
  public void Visit_CompoundAssignment_Subtraction()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 10;
                    x -= 3;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 10;
  x -= 3;
}", script);
  }

  /// <summary>
  /// 测试 VisitCompoundAssignment - 乘法赋值
  /// </summary>
  [TestMethod]
  public void Visit_CompoundAssignment_Multiplication()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    x *= 2;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 5;
  x *= 2;
}", script);
  }

  /// <summary>
  /// 测试 VisitCompoundAssignment - 除法赋值
  /// </summary>
  [TestMethod]
  public void Visit_CompoundAssignment_Division()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 20;
                    x /= 4;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 20;
  x /= 4;
}", script);
  }

  /// <summary>
  /// 测试 VisitCompoundAssignment - 取模赋值
  /// </summary>
  [TestMethod]
  public void Visit_CompoundAssignment_Remainder()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 10;
                    x %= 3;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 10;
  x %= 3;
}", script);
  }

  #endregion

  #region 递减操作测试

  /// <summary>
  /// 测试 VisitIncrementOrDecrement - 后缀递减
  /// </summary>
  [TestMethod]
  public void Visit_IncrementOrDecrement_Decrement()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    x--;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 5;
  x--;
}", script);
  }

  /// <summary>
  /// 测试 VisitIncrementOrDecrement - 前缀递减
  /// </summary>
  [TestMethod]
  public void Visit_IncrementOrDecrement_PrefixDecrement()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    --x;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 5;
  --x;
}", script);
  }

  #endregion

  #region 字面量额外测试

  /// <summary>
  /// 测试 VisitLiteral - 浮点数字面量
  /// </summary>
  [TestMethod]
  public void Visit_Literal_Float()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    float f = 3.14f;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let f = 3.14;
}", script);
  }

  /// <summary>
  /// 测试 VisitLiteral - 字符字面量
  /// </summary>
  [TestMethod]
  public void Visit_Literal_Char()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    char c = 'A';
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let c = ""A"";
}", script);
  }

  /// <summary>
  /// 测试 VisitLiteral - 带转义的字符串字面量
  /// </summary>
  [TestMethod]
  public void Visit_Literal_EscapedString()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string str = ""Line1\nLine2"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let str = ""Line1\nLine2"";
}", script);
  }

  /// <summary>
  /// 测试 VisitLiteral - 双精度浮点数字面量
  /// </summary>
  [TestMethod]
  public void Visit_Literal_Double()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double d = 3.1415926535;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let d = 3.1415926535;
}", script);
  }

  /// <summary>
  /// 测试 VisitLiteral - BigInt 字面量 (long)
  /// </summary>
  [TestMethod]
  public void Visit_Literal_Long()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    long l = 42L;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let l = 42n;
}", script);
  }

  [TestMethod]
  public void Visit_Literal_UserDefinedNumericConstants_PreserveSpecialValuesAndWidths()
  {
    var block = GetBlockOperation("""
            class TestClass
            {
                private const float SingleNaN = float.NaN;
                private const float SinglePositiveInfinity = float.PositiveInfinity;
                private const float SingleNegativeInfinity = float.NegativeInfinity;
                private const double DoubleNaN = double.NaN;
                private const double DoublePositiveInfinity = double.PositiveInfinity;
                private const double DoubleNegativeInfinity = double.NegativeInfinity;
                private const decimal ExactDecimal = 1234567890.123456789m;
                private const long SignedMinimum = long.MinValue;
                private const ulong UnsignedMaximum = ulong.MaxValue;

                void TestMethod()
                {
                    var singleNaN = SingleNaN;
                    var singlePositiveInfinity = SinglePositiveInfinity;
                    var singleNegativeInfinity = SingleNegativeInfinity;
                    var doubleNaN = DoubleNaN;
                    var doublePositiveInfinity = DoublePositiveInfinity;
                    var doubleNegativeInfinity = DoubleNegativeInfinity;
                    var exactDecimal = ExactDecimal;
                    var signedMinimum = SignedMinimum;
                    var unsignedMaximum = UnsignedMaximum;
                }
            }
            """);

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual("""
            {
              let singleNaN = NaN;
              let singlePositiveInfinity = Infinity;
              let singleNegativeInfinity = -Infinity;
              let doubleNaN = NaN;
              let doublePositiveInfinity = Infinity;
              let doubleNegativeInfinity = -Infinity;
              let exactDecimal = 1234567890.123456789;
              let signedMinimum = -9223372036854775808n;
              let unsignedMaximum = 18446744073709551615n;
            }
            """, script);
  }

  #endregion

  #region 类型转换测试

  /// <summary>
  /// 测试 VisitConversion - Number 转 BigInt
  /// </summary>
  [TestMethod]
  public void Visit_Conversion_IntToLong()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int i = 42;
                    long l = (long)i;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let i = 42;
  let l = BigInt(i);
}", script);
  }

  /// <summary>
  /// 测试 VisitConversion - BigInt 转 Number
  /// </summary>
  [TestMethod]
  public void Visit_Conversion_LongToInt()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    long l = 42L;
                    int i = (int)l;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let l = 42n;
  let i = Number(l);
}", script);
  }

  [TestMethod]
  public void Visit_Conversion_CharArithmetic_PreservesUtf16CodeUnitSemantics()
  {
    var block = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(char value)
                {
                    var shifted = value + 32;
                    var lower = (char)shifted;
                }
            }
            """);

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual("""
{
  let shifted = value.charCodeAt(0) + 32;
  let lower = String.fromCharCode(shifted);
}
""", script);
  }

  [TestMethod]
  public void Visit_Conversion_CharToInt64_ConvertsCodeUnitBeforeBigInt()
  {
    var block = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(char value)
                {
                    long codeUnit = value;
                }
            }
            """);

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual("""
{
  let codeUnit = BigInt(value.charCodeAt(0));
}
""", script);
  }

  [TestMethod]
  public void Visit_Conversion_Int64ToChar_ConvertsBigIntThroughNumber()
  {
    var block = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(long codeUnit)
                {
                    var value = (char)codeUnit;
                }
            }
            """);

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual("""
{
  let value = String.fromCharCode(Number(codeUnit));
}
""", script);
  }

  [TestMethod]
  public void Visit_Conversion_CheckedNumericToChar_ReportsUnsupportedCoercion()
  {
    var block = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(int codeUnit)
                {
                    var value = checked((char)codeUnit);
                }
            }
            """);

    var walker = new SemanticWalker(true);
    var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));

    Assert.AreEqual(OperationKind.Conversion, exception.Kind);
    StringAssert.Contains(exception.Message ?? string.Empty, "Checked numeric-to-char conversion is not supported");
  }

  #endregion

  #region 一元运算符额外测试

  /// <summary>
  /// 测试 VisitUnaryOperator - 按位取反
  /// </summary>
  [TestMethod]
  public void Visit_UnaryOperator_BitwiseNot()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    int result = ~x;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 5;
  let result = ~x;
}", script);
  }

  /// <summary>
  /// 测试 VisitUnaryOperator - 正号运算符
  /// </summary>
  [TestMethod]
  public void Visit_UnaryOperator_Plus()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    int result = +x;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 5;
  let result = +x;
}", script);
  }

  /// <summary>
  /// 测试 VisitUnaryOperator - 强制布尔转换
  /// </summary>
  [TestMethod]
  public void Visit_UnaryOperator_True()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    bool result = true;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 5;
  let result = true;
}", script);
  }

  [TestMethod]
  public void Visit_UnaryOperator_SourceCustomOperator_RequiresExplicitMapping()
  {
    var block = GetBlockOperation("""
            public readonly struct Flag
            {
                public static Flag operator -(Flag value) => value;
            }

            class TestClass
            {
                void TestMethod(Flag value)
                {
                    var result = -value;
                }
            }
            """);

    var exception = Assert.Throws<OperationTransformationException>(() =>
      new SemanticWalker(true).Visit(block, new SenseArgument()));

    StringAssert.Contains(exception.Message, "Flag.operator -(Flag)");
    StringAssert.Contains(exception.Message, "requires an explicit whitelist/ECMAScript mapping");
  }

  [TestMethod]
  public void Visit_UnaryOperator_EcmascriptTrueOperator_UsesBooleanCoercion()
  {
    var block = GetBlockOperation("""
            [ECMAScript]
            public readonly struct JsFlag
            {
                public static extern bool operator true(JsFlag value);
                public static extern bool operator false(JsFlag value);
            }

            class TestClass
            {
                void TestMethod(JsFlag value)
                {
                    if (value)
                    {
                    }
                }
            }
            """);

    var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
    var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

    Assert.IsNotNull(first);
    Assert.AreEqual(first, second);
    StringAssert.Contains(first, "if (!(!value))");
    _ = new Parser().ParseScript(first);
  }

  #endregion

  #region 二元运算符完整测试

  /// <summary>
  /// 测试 VisitBinaryOperator - 减法运算符
  /// </summary>
  [TestMethod]
  public void Visit_BinaryOperator_Subtraction()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 10;
                    int b = 3;
                    int result = a - b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let a = 10;
  let b = 3;
  let result = a - b;
}".Replace("\r\n", "\n"),
        script?.Replace("\r\n", "\n"));
  }

  [TestMethod]
  public void Visit_BinaryOperator_TimeOnlySubtraction_UsesWhitelistOperator()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var left = new System.TimeOnly(1, 0, 0);
                    var right = new System.TimeOnly(23, 0, 0);
                    var result = left - right;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let left = _e9a3481b3456aad4(1, 0, 0);
  let right = _e9a3481b3456aad4(23, 0, 0);
  let result = _888a9b439de5e7c1(left, right);
}".Replace("\r\n", "\n"),
        script?.Replace("\r\n", "\n"));
  }

  /// <summary>
  /// 测试 VisitBinaryOperator - 乘法运算符
  /// </summary>
  [TestMethod]
  public void Visit_BinaryOperator_Multiplication()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 5;
                    int b = 3;
                    int result = a * b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = 5;
  let b = 3;
  let result = a * b;
}", script);
  }

  /// <summary>
  /// 测试 VisitBinaryOperator - 除法运算符
  /// </summary>
  [TestMethod]
  public void Visit_BinaryOperator_Division()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 10;
                    int b = 2;
                    int result = a / b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = 10;
  let b = 2;
  let result = a / b;
}", script);
  }

  /// <summary>
  /// 测试 VisitBinaryOperator - 取模运算符
  /// </summary>
  [TestMethod]
  public void Visit_BinaryOperator_Remainder()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 10;
                    int b = 3;
                    int result = a % b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = 10;
  let b = 3;
  let result = a % b;
}", script);
  }

  /// <summary>
  /// 测试 VisitBinaryOperator - 相等运算符
  /// </summary>
  [TestMethod]
  public void Visit_BinaryOperator_Equals()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 5;
                    int b = 5;
                    bool result = a == b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let a = 5;
  let b = 5;
  let result = a === b;
}", script);
  }

  /// <summary>
  /// 测试 VisitBinaryOperator - 不等运算符
  /// </summary>
  [TestMethod]
  public void Visit_BinaryOperator_NotEquals()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 5;
                    int b = 3;
                    bool result = a != b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let a = 5;
  let b = 3;
  let result = a !== b;
}", script);
  }

  /// <summary>
  /// 测试 VisitBinaryOperator - 小于运算符
  /// </summary>
  [TestMethod]
  public void Visit_BinaryOperator_LessThan()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 3;
                    int b = 5;
                    bool result = a < b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = 3;
  let b = 5;
  let result = a < b;
}", script);
  }

  /// <summary>
  /// 测试 VisitBinaryOperator - 大于运算符
  /// </summary>
  [TestMethod]
  public void Visit_BinaryOperator_GreaterThan()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 5;
                    int b = 3;
                    bool result = a > b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = 5;
  let b = 3;
  let result = a > b;
}", script);
  }

  /// <summary>
  /// 测试 VisitBinaryOperator - 小于等于运算符
  /// </summary>
  [TestMethod]
  public void Visit_BinaryOperator_LessThanOrEqual()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 3;
                    int b = 5;
                    bool result = a <= b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = 3;
  let b = 5;
  let result = a <= b;
}", script);
  }

  /// <summary>
  /// 测试 VisitBinaryOperator - 大于等于运算符
  /// </summary>
  [TestMethod]
  public void Visit_BinaryOperator_GreaterThanOrEqual()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 5;
                    int b = 3;
                    bool result = a >= b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = 5;
  let b = 3;
  let result = a >= b;
}", script);
  }

  /// <summary>
  /// 测试 VisitBinaryOperator - 逻辑或运算符
  /// </summary>
  [TestMethod]
  public void Visit_BinaryOperator_LogicalOr()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    bool a = true;
                    bool b = false;
                    bool result = a || b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = true;
  let b = false;
  let result = a || b;
}", script);
  }

  #endregion

  #region 方法调用额外测试

  /// <summary>
  /// 测试 VisitInvocation - 链式方法调用
  /// </summary>
  [TestMethod]
  public void Visit_Invocation_Chained()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string text = ""  Hello  "";
                    string result = text.Trim().ToUpper();
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let text = ""  Hello  "";
  let result = text.trim().toUpperCase();
}", script);

  }

  /// <summary>
  /// 测试 VisitInvocation - 多参数方法调用
  /// </summary>
  [TestMethod]
  public void Visit_Invocation_MultipleArguments()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string text = ""Hello World"";
                    string sub = text.Substring(0, 5);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let text = ""Hello World"";
  let sub = text.substring(0, 0 + 5);
}", script);

  }

  #endregion

  #region Lambda 表达式测试

  /// <summary>
  /// 测试 VisitAnonymousFunction - 单参数 Lambda
  /// </summary>
  [TestMethod]
  public void Visit_AnonymousFunction_SingleParam()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Func<int, int> func = x => x * 2;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let func = x => {
    return x * 2;
  };
}", script);
  }

  /// <summary>
  /// 测试 VisitAnonymousFunction - 无参数 Lambda
  /// </summary>
  [TestMethod]
  public void Visit_AnonymousFunction_NoParams()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Func<int> func = () => 42;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let func = () => {
    return 42;
  };
}", script);
  }

  /// <summary>
  /// 测试 VisitAnonymousFunction - 语句块 Lambda
  /// </summary>
  [TestMethod]
  public void Visit_AnonymousFunction_StatementBody()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var func = (int x) => { return x * 2; };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let func = x => {
    return x * 2;
  };
}", script);
  }

  #endregion

  #region 表达式语句测试

  /// <summary>
  /// 测试表达式语句 - 方法调用作为语句
  /// </summary>
  [TestMethod]
  public void Visit_ExpressionStatement_MethodCall()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Console.WriteLine(""test"");
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  console.log(""test"");
}", script);

  }

  /// <summary>
  /// 测试表达式语句 - 递增作为语句
  /// </summary>
  [TestMethod]
  public void Visit_ExpressionStatement_Increment()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 0;
                    x++;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 0;
  x++;
}", script);
  }

  #endregion

  #region 复杂场景测试

  /// <summary>
  /// 测试复杂的表达式组合
  /// </summary>
  [TestMethod]
  public void Visit_ComplexExpressions()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    // 嵌套三元运算符
                    int x = 5;
                    string result = x > 0 ? x > 10 ? ""large"" : ""small"" : ""negative"";

                    // 空合并链
                    string str = null;
                    string final = str ?? ""default"" ?? ""fallback"";

                    // 复杂的布尔表达式
                    bool a = true;
                    bool b = false;
                    bool c = true;
                    bool complex = a && b || c;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 5;
  let result = x > 0 ? x > 10 ? ""large"" : ""small"" : ""negative"";
  let str = null;
  let final = str ?? (""default"" ?? ""fallback"");
  let a = true;
  let b = false;
  let c = true;
  let complex = a && b || c;
}", script);
  }

  /// <summary>
  /// 测试位运算符组合
  /// </summary>
  [TestMethod]
  public void Visit_BitwiseOperators()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 5;
                    int b = 3;
                    int notResult = ~a;
                    int andResult = a & b;
                    int orResult = a | b;
                    int xorResult = a ^ b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = 5;
  let b = 3;
  let notResult = ~a;
  let andResult = a & b;
  let orResult = a | b;
  let xorResult = a ^ b;
}", script);
  }

  /// <summary>
  /// 测试位移运算符
  /// </summary>
  [TestMethod]
  public void Visit_BitwiseShiftOperators()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 5;
                    int leftShift = a << 2;
                    int rightShift = a >> 1;
                    int unsignedRightShift = a >>> 1;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = 5;
  let leftShift = a << 2;
  let rightShift = a >> 1;
  let unsignedRightShift = a >>> 1;
}", script);
  }

  /// <summary>
  /// 测试位运算符组合表达式
  /// </summary>
  [TestMethod]
  public void Visit_BitwiseComplexExpression()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 5;
                    int b = 3;
                    int c = 2;
                    int result = (a & b) | (c ^ a);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    // 注意：JavaScript 生成器会优化掉不必要的括号
    Assert.AreEqual(@"{
  let a = 5;
  let b = 3;
  let c = 2;
  let result = a & b | c ^ a;
}", script);
  }

  /// <summary>
  /// 测试位运算符与赋值组合
  /// </summary>
  [TestMethod]
  public void Visit_BitwiseWithAssignment()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 5;
                    int b = 3;
                    a &= b;
                    a |= b;
                    a ^= b;
                    a <<= 2;
                    a >>= 1;
                    a >>>= 1;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = 5;
  let b = 3;
  a &= b;
  a |= b;
  a ^= b;
  a <<= 2;
  a >>= 1;
  a >>>= 1;
}", script);
  }

  /// <summary>
  /// 测试赋值运算符的完整场景
  /// </summary>
  [TestMethod]
  public void Visit_AllAssignmentTypes()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    // 简单赋值
                    int a = 10;

                    // 复合赋值
                    int b = 5;
                    b += 3;
                    b -= 2;
                    b *= 2;
                    b /= 2;
                    b %= 3;

                    // 空合并赋值
                    string name = null;
                    name ??= ""Default"";

                    // 递增递减
                    b++;
                    ++b;
                    b--;
                    --b;

                    // 后缀递增在表达式中
                    int c = b++;

                    // 前缀递增在表达式中
                    int d = ++b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = 10;
  let b = 5;
  b += 3;
  b -= 2;
  b *= 2;
  b /= 2;
  b %= 3;
  let name = null;
  name ??= ""Default"";
  b++;
  ++b;
  b--;
  --b;
  let c = b++;
  let d = ++b;
}", script);
  }
  #endregion

  #region Lambda 闭包测试

  /// <summary>
  /// 测试 Lambda 表达式 - 捕获外部变量
  /// C# 示例：int x = 10; Func<int, int> add = y => y + x;
  /// 转换结果：箭头函数捕获外部变量
  /// </summary>
  [TestMethod]
  public void Visit_AnonymousFunction_Closure_CaptureVariable()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 10;
                    Func<int, int> add = y => y + x;
                    int result = add(5);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let x = 10;
  let add = y => {
    return y + x;
  };
  let result = add(5);
}", script);

  }

  /// <summary>
  /// 测试 Lambda 表达式 - 捕获多个外部变量
  /// C# 示例：int a = 1, b = 2; Func<int> sum = () => a + b;
  /// 转换结果：箭头函数捕获多个变量
  /// </summary>
  [TestMethod]
  public void Visit_AnonymousFunction_Closure_MultipleVariables()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 1;
                    int b = 2;
                    Func<int> sum = () => a + b;
                    int result = sum();
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let a = 1;
  let b = 2;
  let sum = () => {
    return a + b;
  };
  let result = sum();
}", script);

  }

  /// <summary>
  /// 测试 Lambda 表达式 - 在循环中创建闭包
  /// C# 示例：for 循环中创建 Lambda 捕获循环变量
  /// 转换结果：箭头函数在循环中捕获变量
  /// </summary>
  [TestMethod]
  public void Visit_AnonymousFunction_Closure_InLoop()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Func<int, int>[] funcs = new Func<int, int>[3];
                    for (int i = 0; i < 3; i++)
                    {
                        int value = i;
                        funcs[i] = x => x + value;
                    }
                    int result = funcs[0](10);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let funcs = new Array(3);
  for (let i = 0; i < 3; i++) {
    let value = i;
    funcs[i] = x => {
      return x + value;
    };
  }
  let result = funcs[0](10);
}", script);

  }

  /// <summary>
  /// 测试 Lambda 表达式 - 嵌套闭包
  /// C# 示例：外层 Lambda 捕获变量，内层 Lambda 捕获外层 Lambda 的参数
  /// 转换结果：嵌套箭头函数
  /// </summary>
  [TestMethod]
  public void Visit_AnonymousFunction_Closure_Nested()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int multiplier = 2;
                    Func<int, Func<int, int>> createAdder = x => y => x * y + multiplier;
                    var add3 = createAdder(3);
                    int result = add3(4);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let multiplier = 2;
  let createAdder = x => {
    return y => {
      return x * y + multiplier;
    };
  };
  let add3 = createAdder(3);
  let result = add3(4);
}", script);

  }

  #endregion

  /// <summary>
  /// 整体测试 - 方法和函数调用
  /// </summary>
  [TestMethod]
  public void Visit_DelegateCalls()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var add = (int a, int b) => a + b;
                    int result = add(3, 4);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
@"{
  let add = (a, b) => {
    return a + b;
  };
  let result = add(3, 4);
}", script);

  }

  #region 扩展测试用例 - 更多二元运算

  /// <summary>
  /// 测试二元运算 - 模运算
  /// </summary>
  [TestMethod]
  public void Visit_Binary_Modulo()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int remainder = 10 % 3;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let remainder = 10 % 3;
}", script);
  }

  /// <summary>
  /// 测试二元运算 - 幂运算
  /// </summary>
  [TestMethod]
  public void Visit_Binary_Power()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double result = System.Math.Pow(2, 10);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let result = Math.pow(2, 10);
}", script);
  }

  /// <summary>
  /// 测试二元运算 - 位移
  /// </summary>
  [TestMethod]
  public void Visit_Binary_Shift()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int left = 1 << 4;
                    int right = 16 >> 2;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let left = 1 << 4;
  let right = 16 >> 2;
}", script);
  }

  /// <summary>
  /// 测试二元运算 - 位运算
  /// </summary>
  [TestMethod]
  public void Visit_Binary_Bitwise()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int and = 0xFF & 0x0F;
                    int or = 0xF0 | 0x0F;
                    int xor = 0xFF ^ 0x0F;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let and = 255 & 15;
  let or = 240 | 15;
  let xor = 255 ^ 15;
}", script);
  }

  #endregion

  #region 扩展测试用例 - 更多一元运算

  /// <summary>
  /// 测试一元运算 - 位取反
  /// </summary>
  [TestMethod]
  public void Visit_Unary_BitwiseNot()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 0x0F;
                    int not = ~value;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let value = 15;
  let not = ~value;
}", script);
  }

  /// <summary>
  /// 测试一元运算 - 正号
  /// </summary>
  [TestMethod]
  public void Visit_Unary_Plus()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = +42;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = +42;
}", script);
  }

  /// <summary>
  /// 测试一元运算 - 前置递增
  /// </summary>
  [TestMethod]
  public void Visit_Unary_PrefixIncrement()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    int y = ++x;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 5;
  let y = ++x;
}", script);
  }

  /// <summary>
  /// 测试一元运算 - 后置递减
  /// </summary>
  [TestMethod]
  public void Visit_Unary_PostfixDecrement()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    int y = x--;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 5;
  let y = x--;
}", script);
  }

  #endregion

  #region 扩展测试用例 - 更多条件表达式

  /// <summary>
  /// 测试条件表达式 - 嵌套三元
  /// </summary>
  [TestMethod]
  public void Visit_Conditional_Nested()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    string result = x < 0 ? ""negative"" : x == 0 ? ""zero"" : ""positive"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 5;
  let result = x < 0 ? ""negative"" : x === 0 ? ""zero"" : ""positive"";
}", script);
  }

  /// <summary>
  /// 测试条件表达式 - 复杂条件
  /// </summary>
  [TestMethod]
  public void Visit_Conditional_ComplexCondition()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 5;
                    int b = 10;
                    int max = a > b ? a : b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = 5;
  let b = 10;
  let max = a > b ? a : b;
}", script);
  }

  /// <summary>
  /// 测试 if/else 语句在分支体为裸表达式语句时仍可稳定转换
  /// </summary>
  [TestMethod]
  public void Visit_Conditional_IfElse_WithBareExpressionStatementBodies()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(bool ready)
                {
                    if (ready)
                        Console.WriteLine(""ready"");
                    else
                        Console.WriteLine(""waiting"");
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  if (ready)
    console.log(""ready"");
  else
    console.log(""waiting"");
}", script);
  }

  #endregion

  #region 扩展测试用例 - 更多赋值运算

  /// <summary>
  /// 测试复合赋值 - %=
  /// </summary>
  [TestMethod]
  public void Visit_Assign_ModuloAssign()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 10;
                    x %= 3;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 10;
  x %= 3;
}", script);
  }

  /// <summary>
  /// 测试复合赋值 - &=
  /// </summary>
  [TestMethod]
  public void Visit_Assign_AndAssign()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 0xFF;
                    x &= 0x0F;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 255;
  x &= 15;
}", script);
  }

  /// <summary>
  /// 测试复合赋值 - |=
  /// </summary>
  [TestMethod]
  public void Visit_Assign_OrAssign()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 0xF0;
                    x |= 0x0F;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 240;
  x |= 15;
}", script);
  }

  /// <summary>
  /// 测试复合赋值 - ^=
  /// </summary>
  [TestMethod]
  public void Visit_Assign_XorAssign()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 0xFF;
                    x ^= 0x0F;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 255;
  x ^= 15;
}", script);
  }

  /// <summary>
  /// 测试复合赋值 - <<=
  /// </summary>
  [TestMethod]
  public void Visit_Assign_LeftShiftAssign()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 1;
                    x <<= 4;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 1;
  x <<= 4;
}", script);
  }

  /// <summary>
  /// 测试复合赋值 - >>=
  /// </summary>
  [TestMethod]
  public void Visit_Assign_RightShiftAssign()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 16;
                    x >>= 2;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 16;
  x >>= 2;
}", script);
  }

  #endregion

  #region 扩展测试用例 - 更多Lambda

  /// <summary>
  /// 测试 Lambda - 无参数
  /// </summary>
  [TestMethod]
  public void Visit_Lambda_NoParams()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Func<int> getNumber = () => 42;
                    int result = getNumber();
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let getNumber = () => {
    return 42;
  };
  let result = getNumber();
}", script);
  }

  /// <summary>
  /// 测试 Lambda - 多语句体
  /// </summary>
  [TestMethod]
  public void Visit_Lambda_StatementBody()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Func<int, int> doubleIt = x =>
                    {
                        int doubled = x * 2;
                        return doubled;
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let doubleIt = x => {
    let doubled = x * 2;
    return doubled;
  };
}", script);
  }

  /// <summary>
  /// 测试 Lambda - Action 类型
  /// </summary>
  [TestMethod]
  public void Visit_Lambda_Action()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Action<string> print = s => Console.WriteLine(s);
                    print(""hello"");
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let print = s => {
    console.log(s);
    return;
  };
  print(""hello"");
}", script);
  }

  #endregion

  #region 扩展测试用例 - 更多比较

  /// <summary>
  /// 测试比较运算 - !=
  /// </summary>
  [TestMethod]
  public void Visit_Comparison_NotEqual()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 1;
                    int b = 2;
                    bool notEqual = a != b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = 1;
  let b = 2;
  let notEqual = a !== b;
}", script);
  }

  /// <summary>
  /// 测试比较运算 - <=
  /// </summary>
  [TestMethod]
  public void Visit_Comparison_LessThanOrEqual()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 1;
                    int b = 2;
                    bool le = a <= b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = 1;
  let b = 2;
  let le = a <= b;
}", script);
  }

  /// <summary>
  /// 测试比较运算 - >=
  /// </summary>
  [TestMethod]
  public void Visit_Comparison_GreaterThanOrEqual()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 2;
                    int b = 1;
                    bool ge = a >= b;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let a = 2;
  let b = 1;
  let ge = a >= b;
}", script);
  }

  #endregion

  #region Nullable<T> 成员访问

  [TestMethod]
  public void Visit_Nullable_HasValue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int? value)
                {
                    var hasValue = value.HasValue;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "value !== null && value !== undefined");
  }

  [TestMethod]
  public void Visit_Nullable_Value()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int? value)
                {
                    var v = value.Value;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "let v = value;");
  }

  [TestMethod]
  public void Visit_Nullable_GetValueOrDefault()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int? value)
                {
                    var d = value.GetValueOrDefault();
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "value ?? 0");
  }

  [TestMethod]
  [DataRow("bool?", "false")]
  [DataRow("char?", "\"\\0\"")]
  [DataRow("long?", "0n")]
  public void Visit_Nullable_GetValueOrDefault_UsesUnderlyingValueTypeDefault(
    string nullableType,
    string expectedDefault)
  {
    var block = GetBlockOperation($$"""
            class TestClass
            {
                void TestMethod({{nullableType}} value)
                {
                    var actual = value.GetValueOrDefault();
                }
            }
            """);

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, $"value ?? {expectedDefault}");
  }

  [TestMethod]
  public void Visit_NullableEnum_GetValueOrDefault_UsesUnderlyingZeroValue()
  {
    var block = GetBlockOperation("""
            enum ReleaseState
            {
                Pending,
                Running,
                Completed
            }

            class TestClass
            {
                void TestMethod(ReleaseState? state)
                {
                    var actual = state.GetValueOrDefault();
                }
            }
            """);

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "state ?? 0");
  }

  [TestMethod]
  public void Visit_GenericNullable_GetValueOrDefault_ReportsErasedDefaultLimitation()
  {
    var block = GetBlockOperation("""
            class TestClass
            {
                void TestMethod<T>(T? value) where T : struct
                {
                    var actual = value.GetValueOrDefault();
                }
            }
            """);

    var walker = new SemanticWalker(true);
    var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));

    Assert.AreEqual(OperationKind.Invocation, exception.Kind);
    StringAssert.Contains(
      exception.Message ?? string.Empty,
      "default(T) is not supported because the runtime type parameter may be a value type");
  }

  [TestMethod]
  public void Visit_Nullable_GetValueOrDefault_WithArg()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int? value)
                {
                    var d = value.GetValueOrDefault(42);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "value ?? 42");
  }

  #endregion
}
