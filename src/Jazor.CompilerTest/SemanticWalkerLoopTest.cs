using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Text.RegularExpressions;

namespace Jazor.ComplierTest;

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
    var usings = @"
          global using System;
          global using System.Collections.Generic;
          global using System.Linq;
          global using System.Numerics;
          global using ECMAScript;
          global using static ECMAScript.Global;";

    var references = Basic.Reference.Assemblies.Net100.References.All
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
    var methodDeclaration = root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
    if (methodDeclaration?.Body is not null)
    {
      var operation = semanticModel.GetOperation(methodDeclaration.Body) as IBlockOperation;
      if (operation is not null)
        return operation;
    }

  throw new InvalidOperationException("未找到可分析的操作");
  }

  private static void AssertScriptEqual(string expected, string? actual)
    => Assert.AreEqual(expected.ReplaceLineEndings("\n"), actual?.ReplaceLineEndings("\n"));
  

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
  for (let num of numbers) {
    console.log(num);
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitForEachLoop - await foreach 应保留异步枚举语义
  /// </summary>
  [TestMethod]
  public void Visit_ForEachLoop_AsyncEnumerable_UsesForAwait()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                async System.Threading.Tasks.Task TestMethod()
                {
                    await foreach (var num in GetNumbers())
                    {
                        Console.WriteLine(num);
                    }
                }

                async IAsyncEnumerable<int> GetNumbers()
                {
                    yield return 1;
                    await System.Threading.Tasks.Task.Yield();
                    yield return 2;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.IsNotNull(script);
    StringAssert.Contains(script, "for await (let num of", StringComparison.Ordinal);
    StringAssert.Contains(script, "console.log(num);", StringComparison.Ordinal);
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
    console.log(i);
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
    console.log(i);
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitForLoop - 表达式初始化器不应被丢失
  /// </summary>
  [TestMethod]
  public void Visit_ForLoop_ExpressionInitializer()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int i;
                    for (i = 0; i < 3; i++)
                    {
                        Console.WriteLine(i);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let i;
  for (i = 0; i < 3; i++) {
    console.log(i);
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitForLoop - 多表达式初始化器应保留顺序
  /// </summary>
  [TestMethod]
  public void Visit_ForLoop_MultipleExpressionInitializers()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int i;
                    int j;
                    for (i = 0, j = 10; i < j; i++, j--)
                    {
                        Console.WriteLine(i + j);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let i;
  let j;
  for (i = 0, j = 10; i < j; i++, j--) {
    console.log(i + j);
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
    console.log(i);
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
    console.log(i);
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
    console.log(i);
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
    console.log(i);
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
    console.log(i);
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
  for (let num of numbers) {
    console.log(num);
  }
  for (let i = 0; i < 5; i++) {
    console.log(i);
  }
  let j = 0;
  while (j < 3) {
    console.log(j);
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
      console.log(i * 3 + j);
    }
  }
}", script);
  }

  #region VisitForEachLoop - 额外测试用例

  /// <summary>
  /// 测试 VisitForEachLoop - 使用 List 集合
  /// C# 示例：foreach (var item in list)
  /// 转换结果：for (let item of list)
  /// </summary>
  [TestMethod]
  public void Visit_ForEachLoop_List()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new[] { 10, 20, 30 };
                    foreach (var item in list)
                    {
                        Console.WriteLine(item);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let list = [10, 20, 30];
  for (let item of list) {
    console.log(item);
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitForEachLoop - 带类型的循环变量
  /// C# 示例：foreach (int num in numbers)
  /// 转换结果：for (let num of numbers)
  /// </summary>
  [TestMethod]
  public void Visit_ForEachLoop_TypedVariable()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var numbers = new[] { 1, 2, 3 };
                    foreach (int num in numbers)
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
  let numbers = [1, 2, 3];
  for (let num of numbers) {
    console.log(num);
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitForEachLoop - 字符串数组
  /// C# 示例：foreach (var name in names)
  /// 转换结果：for (let name of names)
  /// </summary>
  [TestMethod]
  public void Visit_ForEachLoop_StringArray()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var names = new[] { ""Alice"", ""Bob"", ""Charlie"" };
                    foreach (var name in names)
                    {
                        Console.WriteLine(name);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let names = [""Alice"", ""Bob"", ""Charlie""];
  for (let name of names) {
    console.log(name);
  }
}", script);
  }

  #endregion

  #region VisitForLoop - 额外测试用例

  /// <summary>
  /// 测试 VisitForLoop - 递减循环
  /// C# 示例：for (int i = 10; i >= 0; i--)
  /// 转换结果：for (let i = 10; i >= 0; i--)
  /// </summary>
  [TestMethod]
  public void Visit_ForLoop_Decrement()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 10; i >= 0; i--)
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
  for (let i = 10; i >= 0; i--) {
    console.log(i);
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitForLoop - 步长为 2
  /// C# 示例：for (int i = 0; i < 10; i += 2)
  /// 转换结果：for (let i = 0; i < 10; i += 2)
  /// </summary>
  [TestMethod]
  public void Visit_ForLoop_StepTwo()
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
    console.log(i);
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitForLoop - 复杂的更新表达式
  /// C# 示例：for (int i = 0; i < 10; i = i * 2 + 1)
  /// 转换结果：for (let i = 0; i < 10; i = i * 2 + 1)
  /// </summary>
  [TestMethod]
  public void Visit_ForLoop_ComplexUpdate()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; i < 10; i = i * 2 + 1)
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
  for (let i = 0; i < 10; i = i * 2 + 1) {
    console.log(i);
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitForLoop - 完全空的 for 循环
  /// C# 示例：for (;;) { ... }
  /// 转换结果：for (;;) { ... }
  /// </summary>
  [TestMethod]
  public void Visit_ForLoop_Empty()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (;;)
                    {
                        break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  for (; ; ) {
    break;
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitForLoop - 多个初始化变量（只保留一个）
  /// C# 中的 for 循环只能有一个初始化语句
  /// 转换结果：JavaScript 也只保留一个初始化
  /// </summary>
  [TestMethod]
  public void Visit_ForLoop_SingleInit()
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
    console.log(i);
  }
}", script);
  }

  #endregion

  #region VisitWhileLoop - 额外测试用例

  /// <summary>
  /// 测试 VisitWhileLoop - 简单条件（使用变量）
  /// C# 示例：while (x > 0)
  /// 转换结果：while (x > 0)
  /// </summary>
  [TestMethod]
  public void Visit_WhileLoop_VariableCondition()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    while (x > 0)
                    {
                        Console.WriteLine(x);
                        x--;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 5;
  while (x > 0) {
    console.log(x);
    x--;
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitWhileLoop - 逻辑或条件
  /// C# 示例：while (i < 10 || j < 5)
  /// 转换结果：while (i < 10 || j < 5)
  /// </summary>
  [TestMethod]
  public void Visit_WhileLoop_OrCondition()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int i = 0;
                    int j = 0;
                    while (i < 10 || j < 5)
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
  while (i < 10 || j < 5) {
    console.log(i);
    i++;
    j++;
  }
}", script);
  }

  #endregion

  #region 循环控制语句

  /// <summary>
  /// 测试循环中的 break 语句
  /// C# 示例：在循环中使用 break
  /// 转换结果：在循环中使用 break
  /// </summary>
  [TestMethod]
  public void Visit_Loop_WithBreak()
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
                        Console.WriteLine(i);
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
    console.log(i);
  }
}", script);
  }

  /// <summary>
  /// 测试循环中的 continue 语句
  /// C# 示例：在循环中使用 continue
  /// 转换结果：在循环中使用 continue
  /// </summary>
  [TestMethod]
  public void Visit_Loop_WithContinue()
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
                        Console.WriteLine(i);
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
    console.log(i);
  }
}", script);
  }

  /// <summary>
  /// 测试循环中的 return 语句
  /// C# 示例：在循环中使用 return
  /// 转换结果：在循环中使用 return
  /// </summary>
  [TestMethod]
  public void Visit_Loop_WithReturn()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                int TestMethod()
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (i == 5)
                            return i;
                    }
                    return -1;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  for (let i = 0; i < 10; i++) {
    if (i === 5)
      return i;
  }
  return -1;
}", script);
  }

  #endregion

  #region 复杂嵌套场景

  /// <summary>
  /// 测试嵌套循环中的 break
  /// C# 示例：嵌套循环中使用 break 跳出内层循环
  /// 转换结果：嵌套循环中使用 break 跳出内层循环
  /// </summary>
  [TestMethod]
  public void Visit_NestedLoops_WithBreak()
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
                            if (i == j)
                                break;
                            Console.WriteLine(i * 3 + j);
                        }
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  for (let i = 0; i < 3; i++) {
    for (let j = 0; j < 3; j++) {
      if (i === j)
        break;
      console.log(i * 3 + j);
    }
  }
}", script);
  }

  /// <summary>
  /// 测试嵌套循环中的 continue
  /// C# 示例：嵌套循环中使用 continue
  /// 转换结果：嵌套循环中使用 continue
  /// </summary>
  [TestMethod]
  public void Visit_NestedLoops_WithContinue()
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
                            if (i == j)
                                continue;
                            Console.WriteLine(i * 3 + j);
                        }
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

AssertScriptEqual(@"{
  for (let i = 0; i < 3; i++) {
    for (let j = 0; j < 3; j++) {
      if (i === j)
        continue;
      console.log(i * 3 + j);
    }
  }
}", script);
  }

  /// <summary>
  /// 测试 foreach 和 for 的嵌套
  /// C# 示例：外层 foreach，内层 for
  /// 转换结果：外层 for-of，内层 for
  /// </summary>
  [TestMethod]
  public void Visit_NestedForEachAndFor()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var matrix = new[] { new[] { 1, 2 }, new[] { 3, 4 } };
                    foreach (var row in matrix)
                    {
                        for (int j = 0; j < row.Length; j++)
                        {
                            Console.WriteLine(row[j]);
                        }
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let matrix = [[1, 2], [3, 4]];
  for (let row of matrix) {
    for (let j = 0; j < row.length; j++) {
      console.log(row[j]);
    }
  }
}", script);
  }

  #endregion

  #region 综合场景测试

  /// <summary>
  /// 测试循环中的变量作用域
  /// C# 示例：循环变量在循环结束后不可访问
  /// 转换结果：JavaScript 中 let 作用域也遵循相同规则
  /// </summary>
  [TestMethod]
  public void Visit_Loop_VariableScope()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Console.WriteLine(i);
                    }
                    // i 在这里不可访问
                    int j = 0;
                    Console.WriteLine(j);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  for (let i = 0; i < 3; i++) {
    console.log(i);
  }
  let j = 0;
  console.log(j);
}", script);
  }

  /// <summary>
  /// 测试循环的多种组合
  /// C# 示例：包含 foreach、for、while 的复杂场景
  /// 转换结果：对应的 JavaScript 循环组合
  /// </summary>
  [TestMethod]
  public void Visit_Loop_ComplexCombination()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    // foreach 遍历
                    var items = new[] { 1, 2, 3 };
                    foreach (var item in items)
                    {
                        Console.WriteLine(item);
                    }

                    // for 递增循环
                    for (int i = 0; i < 5; i++)
                    {
                        Console.WriteLine(i);
                    }

                    // for 递减循环
                    for (int j = 10; j >= 0; j--)
                    {
                        if (j == 5)
                            break;
                        Console.WriteLine(j);
                    }

                    // while 循环
                    int k = 0;
                    while (k < 3)
                    {
                        Console.WriteLine(k);
                        k++;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let items = [1, 2, 3];
  for (let item of items) {
    console.log(item);
  }
  for (let i = 0; i < 5; i++) {
    console.log(i);
  }
  for (let j = 10; j >= 0; j--) {
    if (j === 5)
      break;
    console.log(j);
  }
  let k = 0;
  while (k < 3) {
    console.log(k);
    k++;
  }
}", script);
  }

  /// <summary>
  /// 测试 do-while 循环
  /// C# 示例：
  /// do {
  ///     i++;
  /// } while (i < 10);
  /// 转换结果：do { i++; } while (i < 10);
  /// </summary>
  [TestMethod]
  public void Visit_DoWhileLoop_Simple()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int i = 0;
                    do
                    {
                        i++;
                    } while (i < 10);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let i = 0;
  do {
    i++;
  } while (i < 10);
}", script);
  }

  /// <summary>
  /// 测试 do-while 循环 - 复杂条件
  /// C# 示例：
  /// do {
  ///     i++;
  /// } while (i < 10 && j > 0);
  /// 转换结果：do { i++; } while (i < 10 && j > 0);
  /// </summary>
  [TestMethod]
  public void Visit_DoWhileLoop_ComplexCondition()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int i = 0;
                    int j = 5;
                    do
                    {
                        i++;
                    } while (i < 10 && j > 0);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let i = 0;
  let j = 5;
  do {
    i++;
  } while (i < 10 && j > 0);
}", script);
  }

  /// <summary>
  /// 测试 do-while 循环 - 嵌套
  /// C# 示例：
  /// do {
  ///     do {
  ///         i++;
  ///     } while (i < 3);
  ///     j++;
  /// } while (j < 5);
  /// 转换结果：嵌套的 do-while 循环
  /// </summary>
  [TestMethod]
  public void Visit_DoWhileLoop_Nested()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int i = 0;
                    int j = 0;
                    do
                    {
                        do
                        {
                            i++;
                        } while (i < 3);
                        j++;
                    } while (j < 5);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let i = 0;
  let j = 0;
  do {
    do {
      i++;
    } while (i < 3);
    j++;
  } while (j < 5);
}", script);
  }

  #endregion

  #region 扩展测试用例 - ForEach变体

  /// <summary>
  /// 测试 foreach 遍历字符串
  /// </summary>
  [TestMethod]
  public void Visit_ForEachLoop_String()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string text = ""hello"";
                    foreach (char c in text)
                    {
                        Console.WriteLine(c);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let text = ""hello"";
  for (let c of text) {
    console.log(c);
  }
}", script);
  }

  /// <summary>
  /// 测试 foreach 带索引访问
  /// </summary>
  [TestMethod]
  public void Visit_ForEachLoop_WithIndex()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var items = new[] { ""a"", ""b"", ""c"" };
                    int index = 0;
                    foreach (var item in items)
                    {
                        Console.WriteLine(index + "": "" + item);
                        index++;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let items = [""a"", ""b"", ""c""];
  let index = 0;
  for (let item of items) {
    console.log(index + "": "" + item);
    index++;
  }
}", script);
  }

  /// <summary>
  /// 测试 foreach 嵌套
  /// </summary>
  [TestMethod]
  public void Visit_ForEachLoop_Nested()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var matrix = new[] { new[] { 1, 2 }, new[] { 3, 4 } };
                    foreach (var row in matrix)
                    {
                        foreach (var col in row)
                        {
                            Console.WriteLine(col);
                        }
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let matrix = [[1, 2], [3, 4]];
  for (let row of matrix) {
    for (let col of row) {
      console.log(col);
    }
  }
}", script);
  }

  #endregion

  #region 扩展测试用例 - For变体

  /// <summary>
  /// 测试 for 循环多变量初始化
  /// </summary>
  [TestMethod]
  public void Visit_ForLoop_MultipleVariables()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0, j = 10; i < j; i++, j--)
                    {
                        Console.WriteLine(i + j);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  for (let i = 0, j = 10; i < j; i++, j--) {
    console.log(i + j);
  }
}", script);
  }

  /// <summary>
  /// 测试 for 循环复杂条件
  /// </summary>
  [TestMethod]
  public void Visit_ForLoop_ComplexCondition()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int limit = 10;
                    for (int i = 0; i < limit && limit > 0; i++)
                    {
                        limit--;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let limit = 10;
  for (let i = 0; i < limit && limit > 0; i++) {
    limit--;
  }
}", script);
  }

  /// <summary>
  /// 测试 for 循环多更新表达式
  /// </summary>
  [TestMethod]
  public void Visit_ForLoop_MultipleUpdates()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0, j = 10; i < 10; i++, j -= 2)
                    {
                        Console.WriteLine(i * j);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  for (let i = 0, j = 10; i < 10; i++, j -= 2) {
    console.log(i * j);
  }
}", script);
  }

  /// <summary>
  /// 测试 for 循环递减步长
  /// </summary>
  [TestMethod]
  public void Visit_ForLoop_DecrementStep()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 100; i >= 0; i -= 10)
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
  for (let i = 100; i >= 0; i -= 10) {
    console.log(i);
  }
}", script);
  }

  /// <summary>
  /// 测试 for 循环乘法更新
  /// </summary>
  [TestMethod]
  public void Visit_ForLoop_MultiplyUpdate()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 1; i < 1000; i *= 2)
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
  for (let i = 1; i < 1000; i *= 2) {
    console.log(i);
  }
}", script);
  }

  #endregion

  #region 扩展测试用例 - While变体

  /// <summary>
  /// 测试 while 循环无限循环
  /// </summary>
  [TestMethod]
  public void Visit_WhileLoop_Infinite()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    while (true)
                    {
                        break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  while (true) {
    break;
  }
}", script);
  }

  /// <summary>
  /// 测试 while 循环带计数器
  /// </summary>
  [TestMethod]
  public void Visit_WhileLoop_WithCounter()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int count = 0;
                    int sum = 0;
                    while (count < 10)
                    {
                        sum += count;
                        count++;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let count = 0;
  let sum = 0;
  while (count < 10) {
    sum += count;
    count++;
  }
}", script);
  }

  /// <summary>
  /// 测试 while 循环前条件检查
  /// </summary>
  [TestMethod]
  public void Visit_WhileLoop_PreCondition()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 0;
                    while (x > 0)
                    {
                        x--;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 0;
  while (x > 0) {
    x--;
  }
}", script);
  }

  #endregion

  #region 扩展测试用例 - DoWhile变体

  /// <summary>
  /// 测试 do-while 循环至少执行一次
  /// </summary>
  [TestMethod]
  public void Visit_DoWhileLoop_ExecuteOnce()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 0;
                    do
                    {
                        x++;
                    } while (false);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 0;
  do {
    x++;
  } while (false);
}", script);
  }

  /// <summary>
  /// 测试 do-while 循环带break
  /// </summary>
  [TestMethod]
  public void Visit_DoWhileLoop_WithBreak()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int i = 0;
                    do
                    {
                        i++;
                        if (i > 5)
                            break;
                    } while (i < 100);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let i = 0;
  do {
    i++;
    if (i > 5)
      break;
  } while (i < 100);
}", script);
  }

  /// <summary>
  /// 测试 do-while 循环带continue
  /// </summary>
  [TestMethod]
  public void Visit_DoWhileLoop_WithContinue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int i = 0;
                    int count = 0;
                    do
                    {
                        i++;
                        if (i % 2 == 0)
                            continue;
                        count++;
                    } while (i < 10);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

AssertScriptEqual(@"{
  let i = 0;
  let count = 0;
  do {
    i++;
    if (i % 2 === 0)
      continue;
    count++;
  } while (i < 10);
}", script);
  }

  #endregion

  #region 扩展测试用例 - 循环控制

  /// <summary>
  /// 测试 break 跳出多层循环
  /// </summary>
  [TestMethod]
  public void Visit_Loop_BreakOuter()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    bool found = false;
                    for (int i = 0; i < 10 && !found; i++)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            if (i * j == 25)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

AssertScriptEqual(@"{
  let found = false;
  for (let i = 0; i < 10 && !found; i++) {
    for (let j = 0; j < 10; j++) {
      if (i * j === 25) {
        found = true;
        break;
      }
    }
  }
}", script);
  }

  /// <summary>
  /// 测试 continue 在嵌套循环中
  /// </summary>
  [TestMethod]
  public void Visit_Loop_ContinueNested()
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
                            if (j == 1)
                                continue;
                            Console.WriteLine(i + "","" + j);
                        }
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

AssertScriptEqual(@"{
  for (let i = 0; i < 3; i++) {
    for (let j = 0; j < 3; j++) {
      if (j === 1)
        continue;
      console.log(i + "","" + j);
    }
  }
}", script);
  }

  /// <summary>
  /// 测试循环中的 return
  /// </summary>
  [TestMethod]
  public void Visit_Loop_ReturnInLoop()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                int TestMethod()
                {
                    for (int i = 0; i < 100; i++)
                    {
                        if (i == 50)
                            return i;
                    }
                    return -1;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

AssertScriptEqual(@"{
  for (let i = 0; i < 100; i++) {
    if (i === 50)
      return i;
  }
  return -1;
}", script);
  }

  /// <summary>
  /// 测试无限循环 while(true)
  /// </summary>
  [TestMethod]
  public void Visit_While_Infinite()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int count = 0;
                    while (true)
                    {
                        count++;
                        if (count > 10)
                            break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let count = 0;
  while (true) {
    count++;
    if (count > 10)
      break;
  }
}", script);
  }

  /// <summary>
  /// 测试 do-while 至少执行一次
  /// </summary>
  [TestMethod]
  public void Visit_DoWhile_ExecuteOnce()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 0;
                    do
                    {
                        x++;
                    } while (false);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let x = 0;
  do {
    x++;
  } while (false);
}", script);
  }

  /// <summary>
  /// 测试 for 循环多变量
  /// </summary>
  [TestMethod]
  public void Visit_For_MultiVariable()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0, j = 10; i < j; i++, j--)
                    {
                        Console.WriteLine(i + "","" + j);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  for (let i = 0, j = 10; i < j; i++, j--) {
    console.log(i + "","" + j);
  }
}", script);
  }

  /// <summary>
  /// 测试 foreach 带索引计算
  /// </summary>
  [TestMethod]
  public void Visit_Foreach_WithIndex()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var items = new int[] { 10, 20, 30 };
                    int index = 0;
                    foreach (var item in items)
                    {
                        Console.WriteLine(index + "": "" + item);
                        index++;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let items = [10, 20, 30];
  let index = 0;
  for (let item of items) {
    console.log(index + "": "" + item);
    index++;
  }
}", script);
  }

  /// <summary>
  /// 测试嵌套 for 循环带条件 break
  /// </summary>
  [TestMethod]
  public void Visit_NestedFor_ConditionalBreak()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    bool found = false;
                    for (int i = 0; i < 5 && !found; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            if (i * j == 12)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

AssertScriptEqual(@"{
  let found = false;
  for (let i = 0; i < 5 && !found; i++) {
    for (let j = 0; j < 5; j++) {
      if (i * j === 12) {
        found = true;
        break;
      }
    }
  }
}", script);
  }

  /// <summary>
  /// 测试 while 循环带计数器
  /// </summary>
  [TestMethod]
  public void Visit_While_WithCounter()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int count = 0;
                    int sum = 0;
                    while (count < 10)
                    {
                        sum += count;
                        count++;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let count = 0;
  let sum = 0;
  while (count < 10) {
    sum += count;
    count++;
  }
}", script);
  }

  /// <summary>
  /// 测试 do-while 带 continue
  /// </summary>
  [TestMethod]
  public void Visit_DoWhile_WithContinue()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int i = 0;
                    do
                    {
                        i++;
                        if (i == 5)
                            continue;
                        Console.WriteLine(i);
                    } while (i < 10);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

AssertScriptEqual(@"{
  let i = 0;
  do {
    i++;
    if (i === 5)
      continue;
    console.log(i);
  } while (i < 10);
}", script);
  }

  /// <summary>
  /// 测试 for 循环空初始化
  /// </summary>
  [TestMethod]
  public void Visit_For_EmptyInit()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int i = 0;
                    for (; i < 5; i++)
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
  for (; i < 5; i++) {
    console.log(i);
  }
}", script);
  }

  /// <summary>
  /// 测试 for 循环空条件
  /// </summary>
  [TestMethod]
  public void Visit_For_EmptyCondition()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; ; i++)
                    {
                        if (i >= 10) break;
                        Console.WriteLine(i);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  for (let i = 0; ; i++) {
    if (i >= 10)
      break;
    console.log(i);
  }
}", script);
  }

  /// <summary>
  /// 测试 for 循环空迭代器
  /// </summary>
  [TestMethod]
  public void Visit_For_EmptyIterator()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; i < 5; )
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
  for (let i = 0; i < 5; ) {
    console.log(i);
    i++;
  }
}", script);
  }

  /// <summary>
  /// 测试 foreach 遍历字符串
  /// </summary>
  [TestMethod]
  public void Visit_Foreach_String()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string text = ""Hello"";
                    foreach (char c in text)
                    {
                        Console.WriteLine(c);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  let text = ""Hello"";
  for (let c of text) {
    console.log(c);
  }
}", script);
  }

  /// <summary>
  /// 测试三重嵌套循环
  /// </summary>
  [TestMethod]
  public void Visit_TripleNestedLoop()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            for (int k = 0; k < 2; k++)
                            {
                                Console.WriteLine(i + "","" + j + "","" + k);
                            }
                        }
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  for (let i = 0; i < 2; i++) {
    for (let j = 0; j < 2; j++) {
      for (let k = 0; k < 2; k++) {
        console.log(i + "","" + j + "","" + k);
      }
    }
  }
}", script);
  }

  /// <summary>
  /// 测试循环带复杂条件
  /// </summary>
  [TestMethod]
  public void Visit_For_ComplexCondition()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 10;
                    int y = 20;
                    for (int i = 0; i < x && i < y; i++)
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
  let x = 10;
  let y = 20;
  for (let i = 0; i < x && i < y; i++) {
    console.log(i);
  }
}", script);
  }

  /// <summary>
  /// 测试循环带递增/递减运算符
  /// </summary>
  [TestMethod]
  public void Visit_For_IncDecOperators()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; i < 10; i++)
                    {
                        int j = i++;
                        int k = ++i;
                        Console.WriteLine(j + "" "" + k);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    AssertScriptEqual(@"{
  for (let i = 0; i < 10; i++) {
    let j = i++;
    let k = ++i;
    console.log(j + "" "" + k);
  }
}", script);
  }

  /// <summary>
  /// 测试 while 循环带提前退出
  /// </summary>
  [TestMethod]
  public void Visit_While_EarlyExit()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                int TestMethod()
                {
                    int i = 0;
                    while (i < 100)
                    {
                        if (i == 42)
                            return i;
                        i++;
                    }
                    return -1;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

AssertScriptEqual(@"{
  let i = 0;
  while (i < 100) {
    if (i === 42)
      return i;
    i++;
  }
  return -1;
}", script);
  }

  /// <summary>
  /// 测试循环中使用 out 参数
  /// </summary>
  [TestMethod]
  public void Visit_Loop_WithOutParameter()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (int.TryParse(""123"", out int result))
                        {
                            Console.WriteLine(result);
                        }
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  for (let i = 0; i < 5; i++) {
    let result, v$0;
    if (v$0 = _16e2a901535b765e(""123"", result), result = v$0[1], v$0[0]) {
      console.log(result);
    }
  }
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
  }

  #endregion
}
