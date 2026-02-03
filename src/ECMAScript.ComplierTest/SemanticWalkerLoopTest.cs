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
    console.log(num);
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
  for (num of numbers) {
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
  for (item of list) {
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
  for (num of numbers) {
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
  for (name of names) {
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

    Assert.AreEqual(@"{
  for (let i = 0; i < 10; i++) {
    if (i == 5)
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

    Assert.AreEqual(@"{
  for (let i = 0; i < 10; i++) {
    if (i % 2 == 0)
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

    Assert.AreEqual(@"{
  for (let i = 0; i < 10; i++) {
    if (i == 5)
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

    Assert.AreEqual(@"{
  for (let i = 0; i < 3; i++) {
    for (let j = 0; j < 3; j++) {
      if (i == j)
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

    Assert.AreEqual(@"{
  for (let i = 0; i < 3; i++) {
    for (let j = 0; j < 3; j++) {
      if (i == j)
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
  for (row of matrix) {
    for (let j = 0; j < row.Length; j++) {
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

    Assert.AreEqual(@"{
  let items = [1, 2, 3];
  for (item of items) {
    console.log(item);
  }
  for (let i = 0; i < 5; i++) {
    console.log(i);
  }
  for (let j = 10; j >= 0; j--) {
    if (j == 5)
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
}