using Acornima.Ast;
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
    var compilation = CSharpCompilation.Create(
        "TestAssembly",
        syntaxTrees: [CSharpSyntaxTree.ParseText(code)],
        references: Basic.Reference.Assemblies.Net100.References.All,
        options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    var syntaxTree = compilation.SyntaxTrees.First();
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

    Assert.AreEqual(@"{
  label1: Console.WriteLine('Labeled statement');
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
}",script);

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
  let str = 'Hello';
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
  let str = 'Hello';
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
  let result = str ?? 'default';
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
  name ??= 'Default';
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
  let person = { Name: 'John', Age: 30 };
  let newPerson = { ...person, Name: 'Jane' };
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
  let str = 'Hello';
  let flag = true;
  let negX = -x;
  let notFlag = !flag;
  let sum = x + 10;
  let result = flag && !notFlag;
  let value = result ? 1 : 0;
  let nullableStr = null;
  let finalStr = nullableStr ?? 'default';
  let name = null;
  name ??= 'Default';
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
  let text = 'Hello World';
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

    Assert.AreEqual(@"{
  label1: Console.WriteLine('Label1');
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
  let person = { Name: 'John', Age: 30 };
  let newPerson = {
    ...person,
    Name: 'Jane',
    Age: 25
  };
  let name = person.Name;
  let age = person.Age;
  let obj = { Value: 10 };
}", script);

  }
}