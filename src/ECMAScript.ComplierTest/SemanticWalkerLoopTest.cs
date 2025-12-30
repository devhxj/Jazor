using Acornima.Ast;
using ECMAScript.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace ECMAScript.ComplierTest;

[TestClass]
public sealed class SemanticWalkerLoopTest
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
  /// 测试 VisitForEachLoop - ForEach 循环操作
  /// </summary>
  [TestMethod]
  public void Visit_ForEachLoop()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var numbers = new[] { 1, 2, 3, 4, 5 };
                    foreach (var num in numbers)
                    {
                        Console.WriteLine(num);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let numbers = [1, 2, 3, 4, 5];
  for (num of numbers) {
    Console.WriteLine(num);
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitForLoop - For 循环操作（简单循环）
  /// </summary>
  [TestMethod]
  public void Visit_ForLoop_Simple()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Console.WriteLine(i);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  for (let i = 0; i < 10; i++) {
    Console.WriteLine(i);
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitForLoop - For 循环操作（无初始化）
  /// </summary>
  [TestMethod]
  public void Visit_ForLoop_NoInit()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int i = 0;
                    for (; i < 10; i++)
                    {
                        Console.WriteLine(i);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let i = 0;
  for (; i < 10; i++) {
    Console.WriteLine(i);
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitForLoop - For 循环操作（无条件）
  /// </summary>
  [TestMethod]
  public void Visit_ForLoop_NoCondition()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; ; i++)
                    {
                        if (i >= 10)
                            break;
                        Console.WriteLine(i);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  for (let i = 0; ; i++) {
    if (i >= 10)
      break;
    Console.WriteLine(i);
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitForLoop - For 循环操作（无迭代器）
  /// </summary>
  [TestMethod]
  public void Visit_ForLoop_NoUpdate()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; i < 10; )
                    {
                        Console.WriteLine(i);
                        i++;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  for (let i = 0; i < 10; ) {
    Console.WriteLine(i);
    i++;
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitForLoop - For 循环操作（复合赋值）
  /// </summary>
  [TestMethod]
  public void Visit_ForLoop_CompoundAssignment()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; i < 10; i += 2)
                    {
                        Console.WriteLine(i);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  for (let i = 0; i < 10; i += 2) {
    Console.WriteLine(i);
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitWhileLoop - While 循环操作
  /// </summary>
  [TestMethod]
  public void Visit_WhileLoop()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int i = 0;
                    while (i < 10)
                    {
                        Console.WriteLine(i);
                        i++;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let i = 0;
  while (i < 10) {
    Console.WriteLine(i);
    i++;
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitWhileLoop - While 循环操作（复杂条件）
  /// </summary>
  [TestMethod]
  public void Visit_WhileLoop_ComplexCondition()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int i = 0;
                    int j = 0;
                    while (i < 10 && j < 5)
                    {
                        Console.WriteLine(i);
                        i++;
                        j++;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let i = 0;
  let j = 0;
  while (i < 10 && j < 5) {
    Console.WriteLine(i);
    i++;
    j++;
  }
}", script);
  }

  /// <summary>
  /// 整体测试 - 循环组合
  /// </summary>
  [TestMethod]
  public void Visit_LoopCombination()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    // ForEach 循环
                    var numbers = new[] { 1, 2, 3 };
                    foreach (var num in numbers)
                    {
                        Console.WriteLine(num);
                    }

                    // For 循环
                    for (int i = 0; i < 5; i++)
                    {
                        Console.WriteLine(i);
                    }

                    // While 循环
                    int j = 0;
                    while (j < 3)
                    {
                        Console.WriteLine(j);
                        j++;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let numbers = [1, 2, 3];
  for (num of numbers) {
    Console.WriteLine(num);
  }
  for (let i = 0; i < 5; i++) {
    Console.WriteLine(i);
  }
  let j = 0;
  while (j < 3) {
    Console.WriteLine(j);
    j++;
  }
}", script);
  }

  /// <summary>
  /// 整体测试 - 嵌套循环
  /// </summary>
  [TestMethod]
  public void Visit_NestedLoops()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            Console.WriteLine(i * 3 + j);
                        }
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  for (let i = 0; i < 3; i++) {
    for (let j = 0; j < 3; j++) {
      Console.WriteLine(i * 3 + j);
    }
  }
}", script);
  }
}