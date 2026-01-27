using ECMAScript.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace ECMAScript.ComplierTest;

[TestClass]
public sealed class SemanticWalkerInvalidTest
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
  /// 获取元组操作
  /// </summary>
  /// <param name="code"></param>
  /// <param name="index"></param>
  /// <returns></returns>
  private static ITupleOperation GetTupleOperationAt(IBlockOperation block, int index = 0)
  {
    var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(block, index);
    var variableDeclaration = variableDeclarationGroup!.Declarations.First();
    var initializer = variableDeclaration.Declarators.First().Initializer;
    var operation = (ITupleOperation)initializer!.Value;
    return operation;
  }

  /// <summary>
  /// 这只是一个整体测试例子
  /// </summary>
  [TestMethod]
  public void Visit_TupleBlockCode()
  {
    var block = GetBlockOperation(@"
            
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(
  @"{


}", script);

  }
}