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

    private static (SemanticModel SemanticModel, SyntaxNode Root) GetSemanticModelAndRoot(string code)
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

        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        if (errors.Count > 0)
        {
            var errorMessages = string.Join("\n", errors.Select(e => $"{e.Id}: {e.GetMessage()}"));
            throw new InvalidOperationException(errorMessages);
        }

        var syntaxTree = compilation.SyntaxTrees.Last();
        return (compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot());
    }

    private static IFieldInitializerOperation GetFieldInitializerOperation(string code)
    {
        var (semanticModel, root) = GetSemanticModelAndRoot(code);
        var fieldDeclarator = root.DescendantNodes().OfType<VariableDeclaratorSyntax>().First();
        var operation = semanticModel.GetOperation(fieldDeclarator.Initializer!) as IFieldInitializerOperation;
        return operation ?? throw new InvalidOperationException("未找到字段初始化器操作");
    }

    private static IPropertyInitializerOperation GetPropertyInitializerOperation(string code)
    {
        var (semanticModel, root) = GetSemanticModelAndRoot(code);
        var propertyDeclaration = root.DescendantNodes().OfType<PropertyDeclarationSyntax>()
            .First(x => x.Initializer is not null);
        var operation = semanticModel.GetOperation(propertyDeclaration.Initializer!) as IPropertyInitializerOperation;
        return operation ?? throw new InvalidOperationException("未找到属性初始化器操作");
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
  let value, v$0, v$1, result, v$2;
  let input = ""123"";
  let dict = new Map;
  if (v$0 = _7db4d9112b4ba3c4(dict, ""key"", value), value = v$0[1], v$0[0]) {
    console.log(value);
  }
  let a;
  if (v$1 = _16e2a901535b765e(input, a), a = v$1[1], v$1[0]) {
    console.log(a);
  }
  if (v$2 = _16e2a901535b765e(input, result), result = v$2[1], v$2[0]) {
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
    public void DirectVisit_FieldInitializer_TupleRemapByTargetType()
    {
        var operation = GetFieldInitializerOperation(@"
            class TestClass
            {
                private (string first, int years) _person = (name: ""John"", age: 30);
            }
            ");

        var walker = new SemanticWalker(true);
        var result = walker.VisitFieldInitializer(operation, new());
        var script = result?.ToKnRECMAScript();

        AssertScriptEqual(@"{ first: ""John"", years: 30 }", script);
    }

    [TestMethod]
    public void DirectVisit_PropertyInitializer_TupleRemapByTargetType()
    {
        var operation = GetPropertyInitializerOperation(@"
            class TestClass
            {
                public (string first, int years) Person { get; set; } = (name: ""John"", age: 30);
            }
            ");

        var walker = new SemanticWalker(true);
        var result = walker.VisitPropertyInitializer(operation, new());
        var script = result?.ToKnRECMAScript();

        AssertScriptEqual(@"{ first: ""John"", years: 30 }", script);
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
  let result, v$0, v$1;
  let x = 10;
  let a = 1, b = 2, c;
  let numbers = [1, 2, 3];
  let input = ""123"";
  if (v$0 = _16e2a901535b765e(input, result), result = v$0[1], v$0[0]) {
    console.log(result);
  }
  let cc;
  if (v$1 = _16e2a901535b765e(input, cc), cc = v$1[1], v$1[0]) {
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
    public void DirectVisit_VariableDeclarator_TupleRemapByTargetType()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    (string name, int age) source = (""John"", 30);
                    (string first, int years) target = source;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
        var variableDeclarator = variableDeclarationGroup.Declarations.First().Declarators.First();

        var result = walker.VisitVariableDeclarator(variableDeclarator, new());
        var script = result?.ToKnRECMAScript();

        AssertScriptEqual(@"target = { first: source.name, years: source.age }", script);
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
        var result = walker.VisitDeclarationExpression(declarationExpression, new SenseArgument());
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
  let bb, v$1, dd, v$3;
  let a = this.test.bind(this);
  a(1, ""2"");
  let ab = (v$1 = _16e2a901535b765e(""1"", bb), bb = v$1[1], v$1[0]);
  let cc = (v$2$0, v$2$1) => _16e2a901535b765e(v$2$0, v$2$1);
  v$3 = cc(""1"", dd), dd = v$3[1], v$3[0];
}", script);

    }

    #region 数组声明测试

    [TestMethod]
    public void Visit_ArrayDeclaration_Empty()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] empty = new int[0];
                    string[] emptyStr = new string[0];
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let empty = [];
  let emptyStr = [];
}", script);
    }

    [TestMethod]
    public void Visit_ArrayDeclaration_Size()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] arr = new int[5];
                    double[] doubles = new double[10];
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let arr = new Array(5);
  let doubles = new Array(10);
}", script);
    }

    [TestMethod]
    public void Visit_ArrayDeclaration_Implicit()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new[] { 1, 2, 3 };
                    var strs = new[] { ""a"", ""b"" };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let arr = [1, 2, 3];
  let strs = [""a"", ""b""];
}", script);
    }

    [TestMethod]
    public void Visit_ArrayDeclaration_Nested()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[][] nested = new int[][] { new int[] { 1, 2 }, new int[] { 3, 4, 5 } };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let nested = [[1, 2], [3, 4, 5]];
}", script);
    }

    #endregion

    #region 多变量声明测试

    [TestMethod]
    public void Visit_MultiVarDeclaration_SameType()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 1, b = 2, c = 3, d = 4;
                    string x = ""x"", y = ""y"", z = ""z"";
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let a = 1, b = 2, c = 3, d = 4;
  let x = ""x"", y = ""y"", z = ""z"";
}", script);
    }

    [TestMethod]
    public void Visit_MultiVarDeclaration_MixedInit()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 1, b, c = 3, d;
                    string x, y = ""y"", z;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let a = 1, b, c = 3, d;
  let x, y = ""y"", z;
}", script);
    }

    [TestMethod]
    public void Visit_MultiVarDeclaration_Expressions()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 1 + 2, b = 3 * 4, c = 5 - 6;
                    bool x = true, y = false, z = a > b;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let a = 1 + 2, b = 3 * 4, c = 5 - 6;
  let x = true, y = false, z = a > b;
}", script);
    }

    #endregion

    #region 变量初始化测试

    [TestMethod]
    public void Visit_VariableInit_Null()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string s = null;
                    object obj = null;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let s = null;
  let obj = null;
}", script);
    }

    [TestMethod]
    public void Visit_VariableInit_Default()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = default(int);
                    bool b = default(bool);
                    string s = default(string);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let x = 0;
  let b = false;
  let s = null;
}", script);
    }

    [TestMethod]
    public void Visit_VariableInit_NewObject()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new object();
                    var list = new System.Collections.Generic.List<int>();
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let obj = new Object;
  let list = [];
}", script);
    }

    [TestMethod]
    public void Visit_VariableInit_ComplexExpression()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int result = (1 + 2) * 3 - 4 / 2;
                    bool condition = (5 > 3) && (2 < 4) || false;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let result = (1 + 2) * 3 - 4 / 2;
  let condition = 5 > 3 && 2 < 4 || false;
}", script);
    }

    [TestMethod]
    public void Visit_VariableInit_MethodCall()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string upper = ""hello"".ToUpper();
                    int length = ""world"".Length;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let upper = ""hello"".toUpperCase();
  let length = ""world"".length;
}", script);
    }

    #endregion

    #region Const声明测试

    [TestMethod]
    public void Visit_ConstDeclaration_Basic()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    const int x = 10;
                    const string name = ""test"";
                    const bool flag = true;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let x = 10;
  let name = ""test"";
  let flag = true;
}", script);
    }

    [TestMethod]
    public void Visit_ConstDeclaration_Expression()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    const int sum = 1 + 2 + 3;
                    const double pi = 3.14159;
                    const int product = 2 * 3 * 4;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let sum = 1 + 2 + 3;
  let pi = 3.14159;
  let product = 2 * 3 * 4;
}", script);
    }

    #endregion

    #region 局部函数声明测试

    [TestMethod]
    public void Visit_LocalFunction_NoParams()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int GetNumber()
                    {
                        return 42;
                    }
                    var result = GetNumber();
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"{
  function GetNumber() {
    return 42;
  }
  let result = GetNumber();
}", script);
    }

    [TestMethod]
    public void Visit_LocalFunction_WithParams()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int Add(int a, int b)
                    {
                        return a + b;
                    }
                    var sum = Add(1, 2);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"{
  function Add(a, b) {
    return a + b;
  }
  let sum = Add(1, 2);
}", script);
    }

    [TestMethod]
    public void Visit_LocalFunction_Static()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    static int Multiply(int a, int b)
                    {
                        return a * b;
                    }
                    var result = Multiply(3, 4);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"{
  function Multiply(a, b) {
    return a * b;
  }
  let result = TestClass.Multiply(3, 4);
}", script);
    }

    #endregion

    #region out参数声明测试

    [TestMethod]
    public void Visit_OutDeclaration_IntTryParse()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    if (int.TryParse(""123"", out int number))
                    {
                        Console.WriteLine(number);
                    }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let number, v$0;
  if (v$0 = _16e2a901535b765e(""123"", number), number = v$0[1], v$0[0]) {
    console.log(number);
  }
}", script);
    }

    [TestMethod]
    public void Visit_OutDeclaration_DoubleTryParse()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    if (double.TryParse(""3.14"", out double value))
                    {
                        Console.WriteLine(value);
                    }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let value, v$0;
  if (v$0 = _a29d389185c5e37d(""3.14"", value), value = v$0[1], v$0[0]) {
    console.log(value);
  }
}", script);
    }

    [TestMethod]
    public void Visit_OutDeclaration_DateOnlyTryParse()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    if (System.DateOnly.TryParse(""2024-01-02"", out var value))
                    {
                        Console.WriteLine(value.ToString());
                    }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let value, v$0;
  if (v$0 = _b14e4d5a572477d0(""2024-01-02"", value), value = v$0[1], v$0[0]) {
    console.log(value.toString());
  }
}", script);
    }

    [TestMethod]
    public void Visit_OutDeclaration_TimeOnlyTryParse()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    if (System.TimeOnly.TryParse(""12:30:00"", out var value))
                    {
                        Console.WriteLine(value.ToString());
                    }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let value, v$0;
  if (v$0 = _ee7de3e005ab6751(""12:30:00"", value), value = v$0[1], v$0[0]) {
    console.log(value.toString());
  }
}", script);
    }

    [TestMethod]
    public void Visit_OutDeclaration_TimeSpanTryParse()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    if (System.TimeSpan.TryParse(""01:02:03"", out var value))
                    {
                        Console.WriteLine(value.ToString());
                    }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let value, v$0;
  if (v$0 = _6fb85ef4d11b9143(""01:02:03"", value), value = v$0[1], v$0[0]) {
    console.log(value.toString());
  }
}", script);
    }

    [TestMethod]
    public void Visit_OutDeclaration_DateTimeOffsetTryParse()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    if (System.DateTimeOffset.TryParse(""2024-01-02T03:04:05+08:00"", out var value))
                    {
                        Console.WriteLine(value.ToString());
                    }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let value, v$0;
  if (v$0 = _2fd90dc37b274014(""2024-01-02T03:04:05+08:00"", value), value = v$0[1], v$0[0]) {
    console.log(value.toString());
  }
}", script);
    }

    [TestMethod]
    public void Visit_OutDeclaration_DecimalTryParse()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    if (decimal.TryParse(""123.45"", out var value))
                    {
                        Console.WriteLine(value.ToString());
                    }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let value, v$0;
  if (v$0 = _e96278809bb50e35(""123.45"", value), value = v$0[1], v$0[0]) {
    console.log(_65a0e4fe8ccdd829(value));
  }
}", script);
    }

    [TestMethod]
    public void Visit_OutDeclaration_MultipleOut()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dict = new System.Collections.Generic.Dictionary<string, int>();
                    dict.TryGetValue(""a"", out int a);
                    dict.TryGetValue(""b"", out int b);
                    Console.WriteLine(a + b);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let a, v$0, b, v$1;
  let dict = new Map;
  v$0 = _7db4d9112b4ba3c4(dict, ""a"", a), a = v$0[1], v$0[0];
  v$1 = _7db4d9112b4ba3c4(dict, ""b"", b), b = v$1[1], v$1[0];
  console.log(a + b);
}", script);
    }

    #endregion

    #region 嵌套声明测试

    [TestMethod]
    public void Visit_NestedDeclaration_Block()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    {
                        int inner = 42;
                        Console.WriteLine(inner);
                    }
                    int outer = 24;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  {
    let inner = 42;
    console.log(inner);
  }
  let outer = 24;
}", script);
    }

    [TestMethod]
    public void Visit_NestedDeclaration_If()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    if (true)
                    {
                        int inIf = 1;
                        Console.WriteLine(inIf);
                    }
                    int afterIf = 2;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  if (true) {
    let inIf = 1;
    console.log(inIf);
  }
  let afterIf = 2;
}", script);
    }

    [TestMethod]
    public void Visit_NestedDeclaration_For()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    for (int i = 0; i < 10; i++)
                    {
                        int loopVar = i * 2;
                        Console.WriteLine(loopVar);
                    }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  for (let i = 0; i < 10; i++) {
    let loopVar = i * 2;
    console.log(loopVar);
  }
}", script);
    }

    #endregion

    #region 使用声明测试

    [TestMethod]
    public void Visit_UsingDeclaration_Basic()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    using var disposable = new TestDisposable();
                    Console.WriteLine(""test"");
                }
            }

            class TestDisposable : System.IDisposable
            {
                public void Dispose() { }
            }
            ");

        var walker = new SemanticWalker(true);
        Assert.Throws<OperationTransformationException>(() =>
        {
            _ = walker.Visit(block, new());
        });
    }

    #endregion

    #region 元组声明测试

    [TestMethod]
    public void Visit_TupleDeclaration_Basic()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    (int a, int b) tuple = (1, 2);
                    Console.WriteLine(tuple.a);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let tuple = { a: 1, b: 2 };
  console.log(tuple.a);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    [TestMethod]
    public void Visit_TupleDeclaration_Named()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    (string Name, int Age) person = (""John"", 30);
                    Console.WriteLine(person.Name);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let person = { Name: ""John"", Age: 30 };
  console.log(person.Name);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    #endregion

    #region 声明表达式测试

    [TestMethod]
    public void Visit_DeclarationExpression_InLambda()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new System.Collections.Generic.List<int> { 1, 2, 3 };
                    var filtered = list.Where(x => x > 1).ToList();
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"{
  let list = [1, 2, 3];
  let filtered = Array.from(Array.from(list).filter(x => {
    return x > 1;
  }));
}", script);
    }

    #endregion

    #region 赋值声明测试

    [TestMethod]
    public void Visit_AssignmentDeclaration_Simple()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x;
                    x = 10;
                    Console.WriteLine(x);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let x;
  x = 10;
  console.log(x);
}", script);
    }

    [TestMethod]
    public void Visit_AssignmentDeclaration_Chained()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a, b, c;
                    a = b = c = 5;
                    Console.WriteLine(a + b + c);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let a, b, c;
  a = b = c = 5;
  console.log(a + b + c);
}", script);
    }

    #endregion

    #region 扩展测试用例 - 更多变量声明

    /// <summary>
    /// 测试变量声明 - 隐式类型
    /// </summary>
    [TestMethod]
    public void Visit_VarDeclaration_InferredType()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var number = 42;
                    var text = ""hello"";
                    var flag = true;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let number = 42;
  let text = ""hello"";
  let flag = true;
}", script);
    }

    /// <summary>
    /// 测试变量声明 - 可空类型
    /// </summary>
    [TestMethod]
    public void Visit_NullableDeclaration()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int? nullable = null;
                    string? maybeNull = ""test"";
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let nullable = null;
  let maybeNull = ""test"";
}", script);
    }

    /// <summary>
    /// 测试变量声明 - 初始化为表达式
    /// </summary>
    [TestMethod]
    public void Visit_ExpressionInitialization()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int sum = 1 + 2 + 3;
                    string greeting = ""Hello"" + "" "" + ""World"";
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let sum = 1 + 2 + 3;
  let greeting = ""Hello"" + "" "" + ""World"";
}", script);
    }

    /// <summary>
    /// 测试变量声明 - 初始化为方法结果
    /// </summary>
    [TestMethod]
    public void Visit_MethodResultInitialization()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int result = GetValue();
                }

                int GetValue() => 42;
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let result = this.GetValue();
}", script);
    }

    #endregion

    #region 扩展测试用例 - 更多数组声明

    /// <summary>
    /// 测试数组声明 - 指定大小
    /// </summary>
    [TestMethod]
    public void Visit_ArrayWithSize()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] arr = new int[5];
                    string[] strs = new string[10];
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let arr = new Array(5);
  let strs = new Array(10);
}", script);
    }

    /// <summary>
    /// 测试数组声明 - 多维数组
    /// </summary>
    [TestMethod]
    public void Visit_MultiDimensionalArray()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[,] matrix = new int[3, 4];
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"{
  let matrix = new Array(3).fill().map(() => new Array(4));
}", script);
    }

    /// <summary>
    /// 测试数组声明 - 锯齿数组
    /// </summary>
    [TestMethod]
    public void Visit_JaggedArray()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[][] jagged = new int[3][];
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"{
  let jagged = new Array(3);
}", script);
    }

    #endregion

    #region 扩展测试用例 - 更多局部函数

    /// <summary>
    /// 测试局部函数 - 带默认参数
    /// </summary>
    [TestMethod]
    public void Visit_LocalFunction_DefaultParam()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int Add(int a, int b = 1)
                    {
                        return a + b;
                    }
                    var result = Add(5);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"{
  function Add(a, b) {
    return a + b;
  }
  let result = Add(5, 1);
}", script);
    }

    /// <summary>
    /// 测试局部函数 - 递归调用
    /// </summary>
    [TestMethod]
    public void Visit_LocalFunction_Recursive()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int Factorial(int n)
                    {
                        return n <= 1 ? 1 : n * Factorial(n - 1);
                    }
                    var result = Factorial(5);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"{
  function Factorial(n) {
    return n <= 1 ? 1 : n * Factorial(n - 1);
  }
  let result = Factorial(5);
}", script);
    }

    #endregion

    #region 扩展测试用例 - 更多out参数

    /// <summary>
    /// 测试out参数 - 多个out
    /// </summary>
    [TestMethod]
    public void Visit_MultipleOutParameters()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    if (int.TryParse(""1"", out int a) && int.TryParse(""2"", out int b))
                    {
                        Console.WriteLine(a + b);
                    }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let a, v$0, b, v$1;
  if ((v$0 = _16e2a901535b765e(""1"", a), a = v$0[1], v$0[0]) && (v$1 = _16e2a901535b765e(""2"", b), b = v$1[1], v$1[0])) {
    console.log(a + b);
  }
}", script);
    }

    #endregion
}
