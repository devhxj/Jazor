using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Text.RegularExpressions;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerTryCatchTest
{
    /// <summary>
    /// 编译代码并获取roslyn代码块
    /// </summary>
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
        var operation = block.Operations.Skip(index).First();
        return operation as T ?? throw new InvalidOperationException("未找到可分析的操作");
    }

    private static void AssertScriptEqual(string expected, string? actual)
        => Assert.AreEqual(expected.ReplaceLineEndings("\n"), actual?.ReplaceLineEndings("\n"));

    #region Try-Catch 基础测试

    /// <summary>
    /// 测试 try-catch 语句转换
    /// C# 示例：
    /// try {
    ///     int x = 1;
    /// } catch (Exception ex) {
    ///     int y = 2;
    /// }
    /// 转换结果：try { let x = 1; } catch (ex) { let y = 2; }
    /// </summary>
    [TestMethod]
    public void VisitTry_SingleCatch()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        int x = 1;
                    }
                    catch (Exception ex)
                    {
                        int y = 2;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
            @"try {
  let x = 1;
} catch (ex) {
  let y = 2;
}", script);
    }

    /// <summary>
    /// 测试 try-catch-finally 语句转换
    /// C# 示例：
    /// try {
    ///     int x = 1;
    /// } catch (Exception ex) {
    ///     int y = 2;
    /// } finally {
    ///     int z = 3;
    /// }
    /// 转换结果：try { let x = 1; } catch (ex) { let y = 2; } finally { let z = 3; }
    /// </summary>
    [TestMethod]
    public void VisitTry_WithFinally()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        int x = 1;
                    }
                    catch (Exception ex)
                    {
                        int y = 2;
                    }
                    finally
                    {
                        int z = 3;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
            @"try {
  let x = 1;
} catch (ex) {
  let y = 2;
} finally {
  let z = 3;
}", script);
    }

    /// <summary>
    /// 测试 try-finally 语句转换（无 catch）
    /// C# 示例：
    /// try {
    ///     int x = 1;
    /// } finally {
    ///     int y = 2;
    /// }
    /// 转换结果：try { let x = 1; } finally { let y = 2; }
    /// </summary>
    [TestMethod]
    public void VisitTry_OnlyFinally()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        int x = 1;
                    }
                    finally
                    {
                        int y = 2;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
            @"try {
  let x = 1;
} finally {
  let y = 2;
}", script);
    }

    #endregion

    #region 多 Catch 子句测试

    /// <summary>
    /// 测试多个 catch 子句转换
    /// C# 示例：
    /// try {
    ///     int x = 1;
    /// } catch (ArgumentException ex) {
    ///     int y = 2;
    /// } catch (InvalidOperationException ex) {
    ///     int z = 3;
    /// }
    /// 转换结果：try { let x = 1; } catch (v$0) {
    ///     if (v$0 instanceof ArgumentException) {
    ///         const ex = v$0;
    ///         let y = 2;
    ///     } else if (v$0 instanceof InvalidOperationException) {
    ///         const ex = v$0;
    ///         let z = 3;
    ///     }
    /// }
    /// </summary>
    [TestMethod]
    public void VisitTry_MultipleCatches()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        int x = 1;
                    }
                    catch (ArgumentException ex)
                    {
                        int y = 2;
                    }
                    catch (InvalidOperationException ex)
                    {
                        int z = 3;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"try {
  let x = 1;
} catch (v$0) {
  if (v$0 instanceof ArgumentException) {
    const ex = v$0;
    let y = 2;
  }
  if (v$0 instanceof Error) {
    const ex = v$0;
    let z = 3;
  }
}", script);

    }

    /// <summary>
    /// 测试多个 catch 子句带 finally
    /// C# 示例：
    /// try {
    ///     int x = 1;
    /// } catch (ArgumentException ex) {
    ///     int y = 2;
    /// } catch (Exception ex) {
    ///     int z = 3;
    /// } finally {
    ///     int w = 4;
    /// }
    /// 转换结果：try { let x = 1; } catch (v$0) {
    ///     if (v$0 instanceof ArgumentException) { const ex = v$0; let y = 2; }
    ///     else if (v$0 instanceof Exception) { const ex = v$0; let z = 3; }
    /// } finally { let w = 4; }
    /// </summary>
    [TestMethod]
    public void VisitTry_MultipleCatchesWithFinally()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        int x = 1;
                    }
                    catch (ArgumentException ex)
                    {
                        int y = 2;
                    }
                    catch (Exception ex)
                    {
                        int z = 3;
                    }
                    finally
                    {
                        int w = 4;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"try {
  let x = 1;
} catch (v$0) {
  if (v$0 instanceof ArgumentException) {
    const ex = v$0;
    let y = 2;
  }
  if (v$0 instanceof Error) {
    const ex = v$0;
    let z = 3;
  }
} finally {
  let w = 4;
}", script);

    }

    #endregion

    #region Throw 语句测试

    /// <summary>
    /// 测试 throw 语句转换
    /// C# 示例：
    /// throw new Exception("error");
    /// 转换结果：throw new Exception("error");
    /// </summary>
    [TestMethod]
    public void VisitThrow_WithException()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    throw new Exception(""error"");
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var throwOp = GetOperationAt<IThrowOperation>(block, 0);
        var node = walker.VisitThrow(throwOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
            @"throw new Error(""error"")", script);
    }

    /// <summary>
    /// 测试 throw 语句与字符串字面量
    /// C# 示例：
    /// throw new Exception("test message");
    /// 转换结果：throw new Exception("test message");
    /// </summary>
    [TestMethod]
    public void VisitThrow_StringLiteral()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    throw new Exception(""test message"");
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var throwOp = GetOperationAt<IThrowOperation>(block, 0);
        var node = walker.VisitThrow(throwOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
            @"throw new Error(""test message"")", script);
    }

    /// <summary>
    /// 测试 throw 在 try-catch 中的使用
    /// C# 示例：
    /// try {
    ///     throw new Exception("error");
    /// } catch (Exception ex) {
    ///     int x = 1;
    /// }
    /// 转换结果：try { throw new Exception("error"); } catch (ex) { let x = 1; }
    /// </summary>
    [TestMethod]
    public void VisitTry_WithThrowInBody()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new Exception(""error"");
                    }
                    catch (Exception ex)
                    {
                        int x = 1;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"try {
  throw new Error(""error"");
} catch (ex) {
  let x = 1;
}", script);

    }

    /// <summary>
    /// 测试 catch 块中的 throw
    /// C# 示例：
    /// try {
    ///     int x = 1;
    /// } catch (Exception ex) {
    ///     throw;
    /// }
    /// 转换结果：try { let x = 1; } catch (ex) { throw v$0; }
    /// </summary>
    [TestMethod]
    public void VisitTry_WithThrowInCatch()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        int x = 1;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"try {
  let x = 1;
} catch (ex) {
  throw ex;
}", script);

    }

    #endregion

    #region 嵌套 Try-Catch 测试

    /// <summary>
    /// 测试嵌套 try-catch
    /// C# 示例：
    /// try {
    ///     try {
    ///         int x = 1;
    ///     } catch (Exception ex) {
    ///         int y = 2;
    ///     }
    /// } catch (Exception ex) {
    ///     int z = 3;
    /// }
    /// 转换结果：try { try { let x = 1; } catch (ex) { let y = 2; } } catch (ex) { let z = 3; }
    /// </summary>
    [TestMethod]
    public void VisitTry_NestedTryCatch()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        try
                        {
                            int x = 1;
                        }
                        catch (Exception ex)
                        {
                            int y = 2;
                        }
                    }
                    catch (Exception ex)
                    {
                        int z = 3;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"try {
  try {
    let x = 1;
  } catch (ex) {
    let y = 2;
  }
} catch (ex) {
  let z = 3;
}", script);

    }

    #endregion

    #region VisitCatchClause 单独测试

    /// <summary>
    /// 测试单个 catch 子句转换
    /// C# 示例：
    /// catch (Exception ex) {
    ///     int x = 1;
    /// }
    /// 转换结果：catch (ex) { let x = 1; }
    /// </summary>
    [TestMethod]
    public void VisitCatchClause_Single()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        int a = 0;
                    }
                    catch (Exception ex)
                    {
                        int x = 1;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var catchOp = tryOp.Catches[0];
        var node = walker.VisitCatchClause(catchOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"(ex) {
  let x = 1;
}", script);

    }

    #endregion

    #region 边界情况测试

    /// <summary>
    /// 测试空的 try 块
    /// C# 示例：
    /// try {
    /// } catch (Exception ex) {
    ///     int x = 1;
    /// }
    /// 转换结果：try { } catch (ex) { let x = 1; }
    /// </summary>
    [TestMethod]
    public void VisitTry_EmptyBody()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                    }
                    catch (Exception ex)
                    {
                        int x = 1;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"try { }
catch (ex) {
  let x = 1;
}", script);

    }

    /// <summary>
    /// 测试空的 catch 块
    /// C# 示例：
    /// try {
    ///     int x = 1;
    /// } catch (Exception ex) {
    /// }
    /// 转换结果：try { let x = 1; } catch (ex) { }
    /// </summary>
    [TestMethod]
    public void VisitTry_EmptyCatch()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        int x = 1;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"try {
  let x = 1;
} catch (ex) { }", script);

    }

    /// <summary>
    /// 测试空的 finally 块
    /// C# 示例：
    /// try {
    ///     int x = 1;
    /// } finally {
    /// }
    /// 转换结果：try { let x = 1; } finally { }
    /// </summary>
    [TestMethod]
    public void VisitTry_EmptyFinally()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        int x = 1;
                    }
                    finally
                    {
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"try {
  let x = 1;
} finally { }", script);

    }

    /// <summary>
    /// 测试 catch 块中使用异常变量
    /// C# 示例：
    /// try {
    ///     throw new Exception("error");
    /// } catch (Exception ex) {
    ///     string msg = ex.Message;
    /// }
    /// 转换结果：try { throw new Exception("error"); } catch (ex) { let msg = ex.Message; }
    /// </summary>
    [TestMethod]
    public void VisitTry_UseExceptionVariable()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new Exception(""error"");
                    }
                    catch (Exception ex)
                    {
                        string msg = ex.Message;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"try {
  throw new Error(""error"");
} catch (ex) {
  let msg = ex.message;
}", script);

    }

    #endregion

    #region Catch When 测试

    /// <summary>
    /// 测试 catch when 子句转换
    /// C# 示例：
    /// try {
    ///     throw new Exception("error");
    /// } catch (Exception ex) when (ex.Message.Contains("error")) {
    ///     string msg = ex.Message;
    /// }
    /// 转换结果：try { throw new Exception("error"); } catch (ex) { if (!(ex.Message.contains("error"))) throw ex; let msg = ex.Message; }
    /// </summary>
    [TestMethod]
    public void VisitCatchClause_WithWhenClause()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new Exception(""error"");
                    }
                    catch (Exception ex) when (ex.Message.Contains(""error""))
                    {
                        string msg = ex.Message;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        // Console.WriteLine("=== Actual Output ===");
        // Console.WriteLine(script);
        // Console.WriteLine("=== End Output ===");

        Assert.AreEqual(
@"try {
  throw new Error(""error"");
} catch (ex) {
  if (!ex.message.includes(""error""))
    throw ex;
  let msg = ex.message;
}", script);

    }

    /// <summary>
    /// 测试 catch when 子句带简单条件
    /// C# 示例：
    /// try {
    ///     throw new Exception("error");
    /// } catch (Exception ex) when (ex != null) {
    ///     string msg = ex.Message;
    /// }
    /// 转换结果：try { throw new Exception("error"); } catch (ex) { if (!(ex !== null)) throw ex; let msg = ex.Message; }
    /// </summary>
    [TestMethod]
    public void VisitCatchClause_WithWhenClause_SimpleCondition()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new Exception(""error"");
                    }
                    catch (Exception ex) when (ex != null)
                    {
                        string msg = ex.Message;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        // C# 的 != 被转换为 JavaScript 的 !=
        AssertScriptEqual(
@"try {
  throw new Error(""error"");
} catch (ex) {
  if (!(ex !== null))
    throw ex;
  let msg = ex.message;
}", script);

    }

    /// <summary>
    /// 测试 catch when 子句带逻辑与条件
    /// C# 示例：
    /// try {
    ///     throw new Exception("error");
    /// } catch (Exception ex) when (ex != null && ex.Message.Length > 0) {
    ///     string msg = ex.Message;
    /// }
    /// </summary>
    [TestMethod]
    public void VisitCatchClause_WithWhenClause_LogicalAndCondition()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new Exception(""error"");
                    }
                    catch (Exception ex) when (ex != null && ex.Message.Length > 0)
                    {
                        string msg = ex.Message;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        // C# 的 != 被转换为 JavaScript 的 !=
        // C# 的 Length 属性被转换为 JavaScript 的 Length（由白名单处理）
        AssertScriptEqual(
@"try {
  throw new Error(""error"");
} catch (ex) {
  if (!(ex !== null && ex.message.length > 0))
    throw ex;
  let msg = ex.message;
}", script);

    }

    /// <summary>
    /// 测试不带异常变量的 catch when
    /// C# 示例：
    /// try {
    ///     throw new Exception("error");
    /// } catch (Exception) when (true) {
    ///     string msg = ""caught"";
    /// }
    /// </summary>
    [TestMethod]
    public void VisitCatchClause_WithWhenClause_NoExceptionVariable()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new Exception(""error"");
                    }
                    catch (Exception) when (true)
                    {
                        string msg = ""a"";
                    }
                    catch (RangeError ex) when (true)
                    {
                        string msg = ""b"";
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"try {
  throw new Error(""error"");
} catch (v$0) {
  if (v$0 instanceof Error) {
    if (!true)
      throw v$0;
    let msg = ""a"";
  }
  if (v$0 instanceof RangeError) {
    const ex = v$0;
    if (!true)
      throw v$0;
    let msg = ""b"";
  }
}", script);

    }

    #endregion

    #region 扩展测试用例 - 更多异常类型

    /// <summary>
    /// 测试 throw 预定义异常
    /// </summary>
    [TestMethod]
    public void VisitThrow_DivideByZeroException()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    throw new DivideByZeroException();
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var throwOp = GetOperationAt<IThrowOperation>(block, 0);
        var node = walker.VisitThrow(throwOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(@"throw new Error('DivideByZeroException')", script);
    }

    /// <summary>
    /// 测试 throw InvalidOperationException
    /// </summary>
    [TestMethod]
    public void VisitThrow_InvalidOperationException()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    throw new InvalidOperationException(""Invalid state"");
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var throwOp = GetOperationAt<IThrowOperation>(block, 0);
        var node = walker.VisitThrow(throwOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(@"throw new Error(""Invalid state"")", script);
    }

    /// <summary>
    /// 测试 throw ArgumentNullException
    /// </summary>
    [TestMethod]
    public void VisitThrow_ArgumentNullException()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    throw new ArgumentNullException(""param"");
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var throwOp = GetOperationAt<IThrowOperation>(block, 0);
        var node = walker.VisitThrow(throwOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(@"throw new TypeError(""param"")", script);
    }

    #endregion

    #region 扩展测试用例 - 复杂嵌套

    /// <summary>
    /// 测试三层嵌套 try-catch
    /// </summary>
    [TestMethod]
    public void VisitTry_TripleNested()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        try
                        {
                            try
                            {
                                int x = 1;
                            }
                            catch (Exception ex1)
                            {
                                int y = 2;
                            }
                        }
                        catch (Exception ex2)
                        {
                            int z = 3;
                        }
                    }
                    catch (Exception ex3)
                    {
                        int w = 4;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"try {
  try {
    try {
      let x = 1;
    } catch (ex1) {
      let y = 2;
    }
  } catch (ex2) {
    let z = 3;
  }
} catch (ex3) {
  let w = 4;
}", script);
    }

    /// <summary>
    /// 测试 try-catch 嵌套循环
    /// </summary>
    [TestMethod]
    public void VisitTry_WithLoopInside()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Console.WriteLine(i);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"try {
  for (let i = 0; i < 3; i++) {
    console.log(i);
  }
} catch (ex) {
  console.log(ex.message);
}", script);
    }

    /// <summary>
    /// 测试循环内 try-catch
    /// </summary>
    [TestMethod]
    public void VisitTry_InsideLoop()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            Console.WriteLine(i);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(""error"");
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
  for (let i = 0; i < 3; i++) {
    try {
      console.log(i);
    } catch (ex) {
      console.log(""error"");
    }
  }
}", script);
    }

    #endregion

    #region 扩展测试用例 - Finally复杂场景

    /// <summary>
    /// 测试 finally 中抛出异常
    /// </summary>
    [TestMethod]
    public void VisitTry_ThrowInFinally()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        int x = 1;
                    }
                    finally
                    {
                        throw new Exception(""finally error"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"try {
  let x = 1;
} finally {
  throw new Error(""finally error"");
}", script);
    }

    /// <summary>
    /// 测试 finally 中有 return
    /// </summary>
    [TestMethod]
    public void VisitTry_ReturnInFinally()
    {
        Assert.Throws<InvalidOperationException>(() => GetBlockOperation(@"
            class TestClass
            {
                int TestMethod()
                {
                    try
                    {
                        return 1;
                    }
                    finally
                    {
                        return 2;
                    }
                }
            }
        "));
    }

    /// <summary>
    /// 测试 finally 中有循环
    /// </summary>
    [TestMethod]
    public void VisitTry_LoopInFinally()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        int x = 1;
                    }
                    finally
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Console.WriteLine(i);
                        }
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"try {
  let x = 1;
} finally {
  for (let i = 0; i < 3; i++) {
    console.log(i);
  }
}", script);
    }

    #endregion

    #region 扩展测试用例 - 异常变量使用

    /// <summary>
    /// 测试异常变量在catch中使用
    /// </summary>
    [TestMethod]
    public void VisitTry_ExceptionVariableUsage()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        int x = 1;
                    }
                    catch (Exception ex)
                    {
                        string name = ex.GetType().Name;
                        string msg = ex.Message;
                        string stack = ex.StackTrace;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"try {
  let x = 1;
} catch (ex) {
  let name = ex.constructor.name;
  let msg = ex.message;
  let stack = ex.stack;
}", script);
    }

    /// <summary>
    /// 测试无异常变量catch
    /// </summary>
    [TestMethod]
    public void VisitTry_CatchWithoutVariable()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        int x = 1;
                    }
                    catch
                    {
                        int y = 2;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"try {
  let x = 1;
} catch {
  let y = 2;
}", script);
    }

    #endregion

    #region 扩展测试用例 - When子句变体

    /// <summary>
    /// 测试 when 子句使用属性
    /// </summary>
    [TestMethod]
    public void VisitCatchClause_WhenWithProperty()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new Exception(""test"");
                    }
                    catch (Exception ex) when (ex.Message == ""test"")
                    {
                        Console.WriteLine(""caught"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"try {
  throw new Error(""test"");
} catch (ex) {
  if (!(ex.message === ""test""))
    throw ex;
  console.log(""caught"");
}", script);
    }

    /// <summary>
    /// 测试 when 子句调用方法
    /// </summary>
    [TestMethod]
    public void VisitCatchClause_WhenWithMethodCall()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new Exception(""error message"");
                    }
                    catch (Exception ex) when (ex.Message.Contains(""error""))
                    {
                        Console.WriteLine(""error caught"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"try {
  throw new Error(""error message"");
} catch (ex) {
  if (!ex.message.includes(""error""))
    throw ex;
  console.log(""error caught"");
}", script);
    }

    /// <summary>
    /// 测试多个 catch when 子句
    /// </summary>
    [TestMethod]
    public void VisitTry_MultipleCatchWithWhen()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new Exception(""test"");
                    }
                    catch (Exception ex) when (ex.Message == ""a"")
                    {
                        Console.WriteLine(""a"");
                    }
                    catch (Exception ex) when (ex.Message == ""b"")
                    {
                        Console.WriteLine(""b"");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(""other"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"try {
  throw new Error(""test"");
} catch (v$0) {
  if (v$0 instanceof Error) {
    const ex = v$0;
    if (!(ex.message === ""a""))
      throw v$0;
    console.log(""a"");
  }
  if (v$0 instanceof Error) {
    const ex = v$0;
    if (!(ex.message === ""b""))
      throw v$0;
    console.log(""b"");
  }
  if (v$0 instanceof Error) {
    const ex = v$0;
    console.log(""other"");
  }
}", script);
    }

    #endregion

    #region 扩展测试用例 - 复杂场景

    /// <summary>
    /// 测试 try-catch-finally 完整场景
    /// </summary>
    [TestMethod]
    public void VisitTry_CompleteScenario()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int result = 0;
                    try
                    {
                        result = 1;
                    }
                    catch (ArgumentException ex)
                    {
                        result = 2;
                    }
                    catch (InvalidOperationException ex)
                    {
                        result = 3;
                    }
                    finally
                    {
                        result = 4;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 1);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        Assert.IsTrue(Regex.IsMatch(script, """
^try \{
  result = 1;
\} catch \((?<err>v\$\d+)\) \{
  if \(\k<err> instanceof (ArgumentException|Error)\) \{
    const ex = \k<err>;
    result = 2;
  \}
  if \(\k<err> instanceof Error\) \{
    const ex = \k<err>;
    result = 3;
  \}
\} finally \{
  result = 4;
\}$
""".Replace("\n", "\r?\n")));
    }

    /// <summary>
    /// 测试 try-catch 在表达式中的使用
    /// </summary>
    [TestMethod]
    public void VisitTry_WithExpression()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                int TestMethod()
                {
                    int x = 0;
                    try
                    {
                        x = 10 / 2;
                    }
                    catch
                    {
                        x = -1;
                    }
                    return x;
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let x = 0;
  try {
    x = 10 / 2;
  } catch {
    x = -1;
  }
  return x;
}", script);
    }

    #endregion

    #region 扩展测试用例 - 更多异常类型

    /// <summary>
    /// 测试 catch InvalidOperationException
    /// </summary>
    [TestMethod]
    public void VisitCatch_InvalidOperationException()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new InvalidOperationException();
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"{
  try {
    throw new Error;
  } catch (ex) {
    console.log(ex.message);
  }
}", script);
    }

    /// <summary>
    /// 测试 catch ArgumentNullException
    /// </summary>
    [TestMethod]
    public void VisitCatch_ArgumentNullException()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new ArgumentNullException(""arg"");
                    }
                    catch (ArgumentNullException ex)
                    {
                        Console.WriteLine(ex.ParamName);
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"{
  try {
    throw new TypeError(""arg"");
  } catch (ex) {
    console.log(ex.message);
  }
}", script);
    }

    /// <summary>
    /// 测试 catch FormatException
    /// </summary>
    [TestMethod]
    public void VisitCatch_FormatException()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        int.Parse(""abc"");
                    }
                    catch (FormatException ex)
                    {
                        Console.WriteLine(""Format error"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        Assert.IsTrue(Regex.IsMatch(script, """_[a-f0-9]+\("abc"\)"""));
        Assert.IsTrue(script.Contains("""console.log("Format error")"""));
        Assert.IsTrue(script.Contains("catch"));
    }

    /// <summary>
    /// 测试 catch OverflowException
    /// </summary>
    [TestMethod]
    public void VisitCatch_OverflowException()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = int.MaxValue;
                    try
                    {
                        checked { value += 1; }
                    }
                    catch (OverflowException)
                    {
                        Console.WriteLine(""Overflow"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        // 当前仅验证 checked + OverflowException 的代码路径可完成转换
        Assert.IsNotNull(script);
    }

    #endregion

    #region 扩展测试用例 - 嵌套try-catch

    /// <summary>
    /// 测试双层嵌套 try-catch
    /// </summary>
    [TestMethod]
    public void VisitTry_DoubleNested()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        try
                        {
                            throw new Exception(""inner"");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  try {
    try {
      throw new Error(""inner"");
    } catch (ex) {
      console.log(ex.message);
    }
  } catch (ex) {
    console.log(ex.message);
  }
}", script);
    }

    /// <summary>
    /// 测试三层嵌套 try-catch
    /// </summary>
    [TestMethod]
    public void VisitTry_TripleNested1()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        try
                        {
                            try
                            {
                                throw new Exception(""level3"");
                            }
                            catch (Exception ex3)
                            {
                                Console.WriteLine(ex3.Message);
                            }
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine(ex2.Message);
                        }
                    }
                    catch (Exception ex1)
                    {
                        Console.WriteLine(ex1.Message);
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  try {
    try {
      try {
        throw new Error(""level3"");
      } catch (ex3) {
        console.log(ex3.message);
      }
    } catch (ex2) {
      console.log(ex2.message);
    }
  } catch (ex1) {
    console.log(ex1.message);
  }
}", script);
    }

    #endregion

    #region 扩展测试用例 - finally复杂场景

    /// <summary>
    /// 测试 finally 带 throw
    /// </summary>
    [TestMethod]
    public void VisitFinally_WithThrow()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        Console.WriteLine(""try"");
                    }
                    finally
                    {
                        throw new Exception(""finally"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  try {
    console.log(""try"");
  } finally {
    throw new Error(""finally"");
  }
}", script);
    }

    /// <summary>
    /// 测试 finally 带 return
    /// </summary>
    [TestMethod]
    public void VisitFinally_WithReturn()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                int TestMethod()
                {
                    try
                    {
                        return 1;
                    }
                    finally
                    {
                        Console.WriteLine(""finally"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  try {
    return 1;
  } finally {
    console.log(""finally"");
  }
}", script);
    }

    /// <summary>
    /// 测试 finally 带循环
    /// </summary>
    [TestMethod]
    public void VisitFinally_WithLoop()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        Console.WriteLine(""try"");
                    }
                    finally
                    {
                        for (int i = 0; i < 3; i++)
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

        Assert.AreEqual(
@"{
  try {
    console.log(""try"");
  } finally {
    for (let i = 0; i < 3; i++) {
      console.log(i);
    }
  }
}", script);
    }

    #endregion

    #region 扩展测试用例 - try-catch with控制流

    /// <summary>
    /// 测试 try-catch 带 continue
    /// </summary>
    [TestMethod]
    public void VisitTry_WithContinue()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; i < 5; i++)
                    {
                        try
                        {
                            if (i == 2) continue;
                            Console.WriteLine(i);
                        }
                        catch
                        {
                            Console.WriteLine(""error"");
                        }
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"{
  for (let i = 0; i < 5; i++) {
    try {
      if (i === 2)
        continue;
      console.log(i);
    } catch {
      console.log(""error"");
    }
  }
}", script);
    }

    /// <summary>
    /// 测试 try-catch 带 break
    /// </summary>
    [TestMethod]
    public void VisitTry_WithBreak()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; i < 5; i++)
                    {
                        try
                        {
                            if (i == 3) break;
                            Console.WriteLine(i);
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"{
  for (let i = 0; i < 5; i++) {
    try {
      if (i === 3)
        break;
      console.log(i);
    } catch {
      break;
    }
  }
}", script);
    }

    /// <summary>
    /// 测试 try-catch 带 goto (转换为标签)
    /// </summary>
    [TestMethod]
    public void VisitTry_WithGoto()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        Console.WriteLine(""before"");
                        goto Label;
                        Console.WriteLine(""skipped"");
                        Label:
                        Console.WriteLine(""after"");
                    }
                    catch
                    {
                        Console.WriteLine(""error"");
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

    #region 扩展测试用例 - catch无变量

    /// <summary>
    /// 测试 catch 无异常变量
    /// </summary>
    [TestMethod]
    public void VisitCatch_NoVariable()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new Exception();
                    }
                    catch
                    {
                        Console.WriteLine(""caught"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"{
  try {
    throw new Error;
  } catch {
    console.log(""caught"");
  }
}", script);
    }

    /// <summary>
    /// 测试 catch 无类型无变量
    /// </summary>
    [TestMethod]
    public void VisitCatch_NoTypeNoVariable()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        int x = 1;
                    }
                    catch
                    {
                        int y = 2;
                    }
                    finally
                    {
                        Console.WriteLine(""done"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  try {
    let x = 1;
  } catch {
    let y = 2;
  } finally {
    console.log(""done"");
  }
}", script);
    }

    #endregion

    #region 扩展测试用例 - rethrow

    /// <summary>
    /// 测试 catch 中 rethrow
    /// </summary>
    [TestMethod]
    public void VisitCatch_Rethrow()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new Exception(""test"");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        throw;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  try {
    throw new Error(""test"");
  } catch (ex) {
    console.log(ex.message);
    throw ex;
  }
}", script);
    }

    /// <summary>
    /// 测试 catch 中 throw 新异常
    /// </summary>
    [TestMethod]
    public void VisitCatch_ThrowNew()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new Exception(""original"");
                    }
                    catch (Exception)
                    {
                        throw new Exception(""wrapped"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  try {
    throw new Error(""original"");
  } catch {
    throw new Error(""wrapped"");
  }
}", script);
    }

    #endregion

    #region 扩展测试用例 - 更多异常场景

    /// <summary>
    /// 测试 try-catch-finally 嵌套
    /// </summary>
    [TestMethod]
    public void VisitTry_NestedFinally()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        try
                        {
                            Console.WriteLine(""inner"");
                        }
                        finally
                        {
                            Console.WriteLine(""inner finally"");
                        }
                    }
                    finally
                    {
                        Console.WriteLine(""outer finally"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
    }

    /// <summary>
    /// 测试 catch 中使用 when 条件
    /// </summary>
    [TestMethod]
    public void VisitCatch_WhenCondition()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new Exception(""test"");
                    }
                    catch (Exception ex) when (ex.Message.Contains(""test""))
                    {
                        Console.WriteLine(""caught test exception"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
    }

    /// <summary>
    /// 测试多个 catch 块
    /// </summary>
    [TestMethod]
    public void VisitCatch_MultipleCatchBlocks()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new ArgumentException(""test"");
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine(""argument exception"");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(""general exception"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
    }

    /// <summary>
    /// 测试 try-finally 无 catch
    /// </summary>
    [TestMethod]
    public void VisitTry_FinallyOnly()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        Console.WriteLine(""try block"");
                    }
                    finally
                    {
                        Console.WriteLine(""cleanup"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  try {
    console.log(""try block"");
  } finally {
    console.log(""cleanup"");
  }
}", script);
    }

    /// <summary>
    /// 测试 catch 异常后访问属性
    /// </summary>
    [TestMethod]
    public void VisitCatch_ExceptionProperty()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new Exception(""error message"");
                    }
                    catch (Exception ex)
                    {
                        string msg = ex.Message;
                        string stack = ex.StackTrace;
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
    }

    /// <summary>
    /// 测试 finally 中有 return
    /// </summary>
    [TestMethod]
    public void VisitCatch_FinallyWithReturn()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                int TestMethod()
                {
                    try
                    {
                        return 1;
                    }
                    finally
                    {
                        Console.WriteLine(""finally"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
    }

    /// <summary>
    /// 测试 catch 中调用其他方法
    /// </summary>
    [TestMethod]
    public void VisitCatch_CallMethod()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new Exception(""test"");
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                    }
                }

                void LogError(Exception ex) { }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
    }

    /// <summary>
    /// 测试 try 块中有循环
    /// </summary>
    [TestMethod]
    public void VisitTry_LoopInTry()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            Console.WriteLine(i);
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(""error"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
    }

    /// <summary>
    /// 测试 try 块中有条件判断
    /// </summary>
    [TestMethod]
    public void VisitTry_IfInTry()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(bool flag)
                {
                    try
                    {
                        if (flag)
                        {
                            Console.WriteLine(""flag is true"");
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(""error"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
    }

    /// <summary>
    /// 测试 throw 已声明的异常变量
    /// </summary>
    [TestMethod]
    public void VisitThrow_ExceptionVariable()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var ex = new Exception(""test"");
                    throw ex;
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
    }

    /// <summary>
    /// 测试 throw null 检查
    /// </summary>
    [TestMethod]
    public void VisitThrow_ThrowNullCheck()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(string? value)
                {
                    if (value is null)
                    {
                        throw new ArgumentNullException(nameof(value));
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
    }

    /// <summary>
    /// 测试 catch 中的异常类型检查
    /// </summary>
    [TestMethod]
    public void VisitCatch_ExceptionTypeCheck()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        throw new InvalidOperationException(""test"");
                    }
                    catch (InvalidOperationException)
                    {
                        Console.WriteLine(""invalid operation"");
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(""other exception"");
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
    }

    /// <summary>
    /// 测试 try-catch 中使用 using 声明
    /// </summary>
    [TestMethod]
    public void VisitTry_UsingInTry()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        using var reader = new System.IO.StreamReader(""test.txt"");
                        var content = reader.ReadToEnd();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(""error"");
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

    /// <summary>
    /// 测试 catch 后重新抛出包装异常
    /// </summary>
    [TestMethod]
    public void VisitCatch_WrapException()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        DoSomething();
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(""Wrapped"", ex);
                    }
                }

                void DoSomething() { }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
    }

    /// <summary>
    /// 测试 finally 中有 try-catch
    /// </summary>
    [TestMethod]
    public void VisitTry_TryInFinally()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    try
                    {
                        Console.WriteLine(""try"");
                    }
                    finally
                    {
                        try
                        {
                            Console.WriteLine(""finally try"");
                        }
                        catch
                        {
                            Console.WriteLine(""finally catch"");
                        }
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
    }

    #endregion
}
