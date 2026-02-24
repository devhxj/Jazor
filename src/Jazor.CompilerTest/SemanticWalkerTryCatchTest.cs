using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

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

        Assert.AreEqual(
            @"try {
  let x = 1;
} catch (v$0) {
  if (v$0 instanceof ArgumentException) {
    const ex = v$0;
    let y = 2;
  } else if (v$0 instanceof InvalidOperationException) {
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
  } else if (v$0 instanceof Exception) {
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
            @"throw new Exception(""error"")", script);
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
            @"throw new Exception(""test message"")", script);
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
  throw new Exception(""error"");
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
  throw v$0;
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
  throw new Exception(""error"");
} catch (ex) {
  let msg = ex.Message;
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
  throw new Exception(""error"");
} catch (ex) {
  if (!ex.Message.Contains(""error""))
    throw ex;
  let msg = ex.Message;
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
        Assert.AreEqual(
            @"try {
  throw new Exception(""error"");
} catch (ex) {
  if (!(ex != null))
    throw ex;
  let msg = ex.Message;
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
        Assert.AreEqual(
            @"try {
  throw new Exception(""error"");
} catch (ex) {
  if (!(ex != null && ex.Message.Length > 0))
    throw ex;
  let msg = ex.Message;
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
                        string msg = ""caught"";
                    }
                }
            }
        ");

        var walker = new SemanticWalker(true);
        var tryOp = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOp, new());
        var script = node?.ToKnRECMAScript();

        // 没有 catch 参数时，重新抛出使用唯一标识符
        // 注意：if 语句主体只有一个语句时，不会生成花括号
        Assert.AreEqual(
            @"try {
  throw new Exception(""error"");
} catch {
  if (!true)
    throw v$0;
  let msg = ""caught"";
}", script);
    }

    #endregion
}
