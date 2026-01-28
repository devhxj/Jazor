using ECMAScript.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace ECMAScript.ComplierTest;

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
        global using System.Linq;";

    var compilation = CSharpCompilation.Create(
        "TestAssembly",
        syntaxTrees: [
          CSharpSyntaxTree.ParseText(usings),
          CSharpSyntaxTree.ParseText(code)
        ],
        references: Basic.Reference.Assemblies.Net100.References.All,
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
    var methodDeclaration = root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
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
    Console.WriteLine(x);
  }
}", script);

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
  label1: Console.WriteLine(""Labeled statement"");
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

    Assert.AreEqual(@"{
  for (let i = 0; i < 10; i++) {
    if (i == 5)
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

    Assert.AreEqual(@"{
  for (let i = 0; i < 10; i++) {
    if (i % 2 == 0)
      continue;
  }
}", script);

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
    Console.WriteLine(param);
    return;
  }
  LocalFunction(42);
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
  let length = str.Length;
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
  let result = Math.Abs(-5);
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
  let length = str?.Length;
}", script);

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

    Assert.AreEqual(@"{
  await Task.Delay(100);
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

    Assert.AreEqual(@"{let name='TestMethod'}", script);
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

    Assert.AreEqual(@"{
  let person = { Name: ""John"", Age: 30 };
  let newPerson = { ...person, Name: ""Jane"" };
}", script);

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

    Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
}", script);

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

    Assert.AreEqual(@"{
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
  let methodName = 'TestMethod';
  let d = 3.14;
  let i = d;
  let testStr = null;
  let length = testStr?.Length;
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

    Assert.AreEqual(@"{
  let absValue = Math.Abs(-5);
  let text = ""Hello World"";
  let upperText = text.ToUpper();
  function LocalFunction(param) {
    Console.WriteLine(param);
    return;
  }
  LocalFunction(42);
  let add = (a, b) => {
    return a + b;
  };
  let result = add.Invoke(3, 4);
  let sub = text.Substring(0, 5);
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

    Assert.AreEqual(
@"{
  label1: Console.WriteLine(""Label1"");
  ;
  for (let i = 0; i < 10; i++) {
    if (i == 5)
      break;
    if (i % 2 == 0)
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

    Assert.AreEqual(@"{
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

    Assert.AreEqual(@"{
  let a = 10;
  let b = 3;
  let result = a - b;
}", script);
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

    Assert.AreEqual(@"{
  let a = 5;
  let b = 5;
  let result = a == b;
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

    Assert.AreEqual(@"{
  let a = 5;
  let b = 3;
  let result = a != b;
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

    Assert.AreEqual(@"{
  let text = ""  Hello  "";
  let result = text.Trim().ToUpper();
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

    Assert.AreEqual(@"{
  let text = ""Hello World"";
  let sub = text.Substring(0, 5);
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

    Assert.AreEqual(@"{
  Console.WriteLine(""test"");
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
  let result = add.Invoke(5);
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
  let result = sum.Invoke();
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
  let result = funcs[0].Invoke(10);
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
  let add3 = createAdder.Invoke(3);
  let result = add3.Invoke(4);
}", script);

  }

  #endregion
}