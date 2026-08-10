using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerLockTest
{
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
            syntaxTrees:
            [
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
            .FirstOrDefault(static method => method.Identifier.ValueText == "TestMethod" && method.Body is not null)
            ?? root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(static method => method.Body is not null);
        if (methodDeclaration?.Body is not null &&
            semanticModel.GetOperation(methodDeclaration.Body) is IBlockOperation operation)
        {
            return operation;
        }

        throw new InvalidOperationException("未找到可分析的操作");
    }

    private static T GetOperationAt<T>(IBlockOperation block, int index = 0)
        where T : class, IOperation
    {
        var operation = block.Operations.Skip(index).First();
        return operation as T ?? throw new InvalidOperationException("未找到可分析的操作");
    }

    private static void AssertScriptEqual(string expected, string? actual)
        => Assert.AreEqual(expected.ReplaceLineEndings("\n"), actual?.ReplaceLineEndings("\n"));

    [TestMethod]
    public void VisitLock_Basic()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    object gate = new object();
                    lock (gate)
                    {
                        Console.WriteLine("ready");
                    }
                }
            }
            """);

        var walker = new SemanticWalker(true);
        var lockOperation = GetOperationAt<ILockOperation>(block, 1);
        var node = walker.VisitLock(lockOperation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
            """
            {
              if (gate == null)
                throw new TypeError("obj");
              {
                console.log("ready");
              }
            }
            """,
            script);
    }

    [TestMethod]
    public void VisitLock_NonSimpleGateExpression_EvaluatesOnce()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                object GetGate() => new object();

                void TestMethod()
                {
                    lock (GetGate())
                    {
                        Console.WriteLine("ready");
                    }
                }
            }
            """);

        var walker = new SemanticWalker(true);
        var lockOperation = GetOperationAt<ILockOperation>(block, 0);
        var node = walker.VisitLock(lockOperation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
            """
            {
              const v$0 = this.GetGate();
              if (v$0 == null)
                throw new TypeError("obj");
              {
                console.log("ready");
              }
            }
            """,
            script);
    }

    [TestMethod]
    public void VisitLock_DispatchThroughVisit()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    object gate = new object();
                    lock (gate)
                    {
                        Console.WriteLine("ready");
                    }
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new())?.ToKnRECMAScript();

        AssertScriptEqual(
            """
            {
              let gate = new Object;
              {
                if (gate == null)
                  throw new TypeError("obj");
                {
                  console.log("ready");
                }
              }
            }
            """,
            script);
    }

    [TestMethod]
    public void VisitLock_InsideTryFinally_PreservesSequentialLowering()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod(object gate)
                {
                    try
                    {
                        lock (gate)
                        {
                            Console.WriteLine("ready");
                        }
                    }
                    finally
                    {
                        Console.WriteLine("cleanup");
                    }
                }
            }
            """);

        var walker = new SemanticWalker(true);
        var tryOperation = GetOperationAt<ITryOperation>(block, 0);
        var node = walker.VisitTry(tryOperation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
            """
            try {
              {
                if (gate == null)
                  throw new TypeError("obj");
                {
                  console.log("ready");
                }
              }
            } finally {
              console.log("cleanup");
            }
            """,
            script);
    }
}
