using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerDeclarationTest
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
        global using System.ComponentModel;
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
    /// 获取指定索引的操作
    /// </summary>
    private static T GetOperationAt<T>(IBlockOperation block, int index = 0) where T : class, IOperation
    {
        var operation = block.Operations.Skip(index).First();
        return operation as T ?? throw new InvalidOperationException("未找到可分析的操作");
    }

    /// <summary>
    /// 获取元组操作
    /// </summary>
    /// <param name="block"></param>
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

    [TestMethod]
    public void Visit_ArrayInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var intArray = new int[] {1, 2, 3, 4, 5};
                    var stringArray = new string[] {""apple"", ""banana"", ""cherry""};
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let intArray = [1, 2, 3, 4, 5];
  let stringArray = [""apple"", ""banana"", ""cherry""];
}", script);
    }

    [TestMethod]
    public void Visit_VariableInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 10;
                    string name = ""Hello"";
                    double pi = 3.14;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let x = 10;
  let name = ""Hello"";
  let pi = 3.14;
}", script);
    }

    [TestMethod]
    public void Visit_VariableDeclarator()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    string name;
                    bool flag = true;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let x = 5;
  let name;
  let flag = true;
}", script);
    }

    [TestMethod]
    public void Visit_VariableDeclaration()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5, y = 10;
                    string name = $""test{x}{y}"";
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let x = 5, y = 10;
  let name = `test${x}${y}`;
}", script);
    }

    [TestMethod]
    public void Visit_VariableDeclarationGroup()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 1, b = 2, c;
                    string x = ""hello"", y = ""world"";
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let a = 1, b = 2, c;
  let x = ""hello"", y = ""world"";
}", script);

    }

    [TestMethod]
    public void Visit_DeclarationExpression_OutVar()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string input = ""123"";
                    
                    var dict = new System.Collections.Generic.Dictionary<string, int>();
                    if (dict.TryGetValue(""key"", out int value))
                    {
                        Console.WriteLine(value);
                    }

                    int a;
                    if (int.TryParse(input, out a))
                    {
                        Console.WriteLine(a);
                    }

                    if (int.TryParse(input, out var result))
                    {
                        Console.WriteLine(result);
                    }                    
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let input = ""123"";
  let dict = new Map;
  let value, v$0 = {}, v$1;
  if (v$1 = _7db4d9112b4ba3c4(dict, ""key"", v$0), value = v$0.value, v$1) {
    console.log(value);
  }
  let a;
  let v$2 = {}, v$3;
  if (v$3 = _16e2a901535b765e(input, v$2), a = v$2.value, v$3) {
    console.log(a);
  }
  let result, v$4 = {}, v$5;
  if (v$5 = _16e2a901535b765e(input, v$4), result = v$4.value, v$5) {
    console.log(result);
  }
}", script);

    }

    [TestMethod]
    public void Visit_FieldInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new TestClassWithFields();
                }
            }
            
            class TestClassWithFields
            {
                public int Field = 42;
                private string _name = ""default"";
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let obj = new TestClassWithFields;
}", script);

    }

    [TestMethod]
    public void Visit_MixedDeclarationTypes()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    // 简单变量声明
                    int x = 10;
                    
                    // 多变量声明
                    int a = 1, b = 2, c;
                    
                    // 数组初始化
                    var numbers = new int[] {1, 2, 3};
                    
                    // out var 声明
                    string input = ""123"";
                    if (int.TryParse(input, out var result))
                    {
                        Console.WriteLine(result);
                    }

                    int cc;
                    if (int.TryParse(input, out cc))
                    {
                        Console.WriteLine(cc);
                    }                    
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let x = 10;
  let a = 1, b = 2, c;
  let numbers = [1, 2, 3];
  let input = ""123"";
  let result, v$0 = {}, v$1;
  if (v$1 = _16e2a901535b765e(input, v$0), result = v$0.value, v$1) {
    console.log(result);
  }
  let cc;
  let v$2 = {}, v$3;
  if (v$3 = _16e2a901535b765e(input, v$2), cc = v$2.value, v$3) {
    console.log(cc);
  }
}", script);

    }

    [TestMethod]
    public void DirectVisit_ArrayInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new int[] { 1, 2, 3 };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var variableDeclaration = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);
        var variableDeclarator = variableDeclaration.Declarations.First().Declarators.First();
        var arrayCreation = (IArrayCreationOperation)variableDeclarator.Initializer!.Value;
        var result = walker.VisitArrayInitializer(arrayCreation.Initializer!, new());
        var script = result?.ToECMAScript();

        Assert.AreEqual("[1,2,3]", script);
    }

    [TestMethod]
    public void DirectVisit_VariableInitializer()
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
        var variableDeclaration = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);
        var variableDeclarator = variableDeclaration.Declarations.First().Declarators.First();
        var variableInitializer = variableDeclarator.Initializer!;

        var result = walker.VisitVariableInitializer(variableInitializer, new());
        var script = result?.ToKnRECMAScript();

        Assert.AreEqual("42", script);
    }

    [TestMethod]
    public void DirectVisit_VariableDeclarator()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 100;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var variableDeclaration = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);
        var variableDeclarator = variableDeclaration.Declarations.First().Declarators.First();

        var result = walker.VisitVariableDeclarator(variableDeclarator, new());
        var script = result?.ToKnRECMAScript();

        Assert.AreEqual("x = 100", script);
    }

    [TestMethod]
    public void DirectVisit_VariableDeclaration()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5, y = 10;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);
        var variableDeclaration = variableDeclarationGroup.Declarations.First();

        var result = walker.VisitVariableDeclaration(variableDeclaration, new());
        var script = result?.ToKnRECMAScript();

        Assert.AreEqual("let x = 5, y = 10", script);
    }

    [TestMethod]
    public void DirectVisit_VariableDeclarationGroup()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 1, b = 2;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);

        var result = walker.VisitVariableDeclarationGroup(variableDeclarationGroup, new());
        var script = result?.ToKnRECMAScript();

        Assert.AreEqual("let a = 1, b = 2", script);
    }

    [TestMethod]
    public void DirectVisit_DeclarationExpression_OutVar()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    if (int.TryParse(""123"", out var result))
                    {
                        Console.WriteLine(result);
                    }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var ifOperation = GetOperationAt<IConditionalOperation>(block, 0);
        var invocationOperation = (IInvocationOperation)ifOperation.Condition;
        var argumentOperation = invocationOperation.Arguments[1];
        var declarationExpression = (IDeclarationExpressionOperation)argumentOperation.Value;

        var result = walker.VisitDeclarationExpression(declarationExpression, new());
        var script = result?.ToECMAScript();

        Assert.AreEqual("result", script);
    }

    [TestMethod]
    public void DirectVisit_MethodReference()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var a = TestMethod1;
                    a(1,""2"");

                    var ab = int.TryParse(""1"", out var bb);

                    TryParseDelegate cc = int.TryParse;
                    cc(""1"", out var dd);
                }

                [Description(""@#test"")]
                void TestMethod1(int a, string b)
                {

                }        

                delegate bool TryParseDelegate(string s, out int result);        
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let a = this.test.bind(this);
  a(1, ""2"");
  let bb, v$0 = {}, v$1;
  let ab = (v$1 = _16e2a901535b765e(""1"", v$0), bb = v$0.value, v$1);
  let cc = _16e2a901535b765e;
  let dd, v$2 = {}, v$3;
  v$3 = cc(""1"", v$2), dd = v$2.value, v$3;
}", script);

    }
}