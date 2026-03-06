using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerCreationTest
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
    /// 获取指定索引的操作
    /// </summary>
    private static T GetOperationAt<T>(IBlockOperation block, int index = 0) where T : class, IOperation
    {
        var operation = block.Operations.Skip(index).First() as T;
        return operation ?? throw new InvalidOperationException("未找到可分析的操作");
    }

    /// <summary>
    /// 获取对象创建操作
    /// </summary>
    /// <param name="code"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private static IObjectCreationOperation GetObjectCreationOperationAt(IBlockOperation block, int index = 0)
    {
        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(block, index);
        var variableDeclaration = variableDeclarationGroup!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (IObjectCreationOperation)initializer!.Value;
        return operation;
    }

    /// <summary>
    /// 获取数组创建操作
    /// </summary>
    /// <param name="code"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private static IArrayCreationOperation GetArrayCreationOperationAt(IBlockOperation block, int index = 0)
    {
        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(block, index);
        var variableDeclaration = variableDeclarationGroup!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (IArrayCreationOperation)initializer!.Value;
        return operation;
    }

    /// <summary>
    /// 获取匿名对象创建操作
    /// </summary>
    /// <param name="code"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private static IAnonymousObjectCreationOperation GetAnonymousObjectCreationOperationAt(IBlockOperation block, int index = 0)
    {
        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(block, index);
        var variableDeclaration = variableDeclarationGroup!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (IAnonymousObjectCreationOperation)initializer!.Value;
        return operation;
    }

    /// <summary>
    /// 获取插值字符串操作
    /// </summary>
    /// <param name="code"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private static IInterpolatedStringOperation GetInterpolatedStringOperationAt(IBlockOperation block, int index = 0)
    {
        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(block, index);
        var variableDeclaration = variableDeclarationGroup!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (IInterpolatedStringOperation)initializer!.Value;
        return operation;
    }

    /// <summary>
    /// 获取泛型类型参数对象创建操作
    /// </summary>
    /// <param name="code"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private static ITypeParameterObjectCreationOperation GetTypeParameterObjectCreationOperationAt(IBlockOperation block, int index = 0)
    {
        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(block, index);
        var variableDeclaration = variableDeclarationGroup!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (ITypeParameterObjectCreationOperation)initializer!.Value;
        return operation;
    }

    [TestMethod]
    public void VisitObjectCreation_SimpleConstructor()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new MyClass();
                }

                class MyClass
                {
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("new MyClass", script);
    }

    [TestMethod]
    public void VisitObjectCreation_ConstructorWithParameters()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new MyClass(42, ""test"");
                }

                class MyClass
                {
                    public MyClass(int number, string text) { }
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual(@"new MyClass(42,""test"")", script);
    }

    [TestMethod]
    public void VisitObjectCreation_AnonymousType()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Name = ""John"", Age = 30 };
                }
            }
            ");

        var operation = GetAnonymousObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitAnonymousObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual(@"{Name:""John"",Age:30}", script);
    }

    [TestMethod]
    public void VisitObjectCreation_GenericType()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod<T>()
                {
                    var obj = new MyClass<int>();
                }

                class MyClass<T>
                {
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("new MyClass", script);
    }

    [TestMethod]
    public void VisitArrayCreation_WithInitializer()
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

        var operation = GetArrayCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("[1,2,3]", script);
    }

    [TestMethod]
    public void VisitArrayCreation_WithSize()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new int[5];
                }
            }
            ");

        var operation = GetArrayCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("new Array(5)", script);
    }

    [TestMethod]
    public void VisitArrayCreation_EmptyArray()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new int[] { };
                }
            }
            ");

        var operation = GetArrayCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("[]", script);
    }

    [TestMethod]
    public void VisitAnonymousObjectCreation_SimpleProperties()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Name = ""John"", Age = 25 };
                }
            }
            ");

        var operation = GetAnonymousObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitAnonymousObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual(@"{Name:""John"",Age:25}", script);
    }

    [TestMethod]
    public void VisitAnonymousObjectCreation_WithExpressions()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 10, y = 20;
                    var obj = new { Sum = x + y, Product = x * y };
                }
            }
            ");

        var operation = GetAnonymousObjectCreationOperationAt(block, 1);
        var walker = new SemanticWalker(true);
        var node = walker.VisitAnonymousObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{Sum:x+y,Product:x*y}", script);
    }

    [TestMethod]
    public void VisitObjectCreation_ObjectInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new MyClass(2,3) { Property1 = ""value1"", Property2 = 42 };
                }

                class MyClass(int x,int y)
                {
                    public int X{get;} =x;
                    public int Y{get;} =y;
                    public string Property1 { get; set; }
                    public int Property2 { get; set; }
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"(() => {
  let v$0 = new MyClass(2, 3);
  v$0.Property1 = ""value1"";
  v$0.Property2 = 42;
  return v$0;
})()", script);

    }

    [TestMethod]
    public void VisitObjectOrCollectionInitializer_ObjectInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new MyClass { Property1 = ""value1"", Property2 = 42 };
                }

                class MyClass
                {
                    public string Property1 { get; set; }
                    public int Property2 { get; set; }
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectOrCollectionInitializer(operation.Initializer!, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(@"v$0.Property1 = ""value1"", v$0.Property2 = 42", script);

    }

    [TestMethod]
    public void VisitMemberInitializer_PropertyAssignment()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new A { A1 = { B1 = ""Test"", B2 = { C1 = ""a"", C2 = 9 } }, A2 = ""value"" };
                }

                class A
                {
                    public B? A1 { get; set; }
                    public string? A2 { get; set; }
                }

                class B
                {
                    public string? B1 { get; set; }
                    public C? B2 { get; set; }
                }

                class C
                {
                    public string? C1 { get; set; }
                    public int C2 { get; set; }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var operation = GetObjectCreationOperationAt(block);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"(() => {
  let v$0 = new A;
  v$0.A1 = { B1: ""Test"", B2: { C1: ""a"", C2: 9 } };
  v$0.A2 = ""value"";
  return v$0;
})()", script);

    }

    [TestMethod]
    public void VisitInterpolatedString_SimpleInterpolation()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string name = ""John"";
                    var message = $""Hello, {name}!"";
                }
            }
            ");

        var operation = GetInterpolatedStringOperationAt(block, 1);
        var walker = new SemanticWalker(true);
        var node = walker.VisitInterpolatedString(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("`Hello, ${name}!`", script);
    }

    [TestMethod]
    public void VisitInterpolatedString_WithExpressions()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5, y = 10;
                    var message = $""Sum: {x + y}, Product: {x * y}"";
                }
            }
            ");

        var operation = GetInterpolatedStringOperationAt(block, 1);
        var walker = new SemanticWalker(true);
        var node = walker.VisitInterpolatedString(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("`Sum: ${x+y}, Product: ${x*y}`", script);
    }

    [TestMethod]
    public void VisitInterpolatedString_MultipleInterpolations()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string name = ""John"";
                    int age = 30;
                    var message = $""Name: {name}, Age: {age}"";
                }
            }
            ");

        var operation = GetInterpolatedStringOperationAt(block, 2);
        var walker = new SemanticWalker(true);
        var node = walker.VisitInterpolatedString(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("`Name: ${name}, Age: ${age}`", script);
    }

    [TestMethod]
    public void VisitInterpolatedString_WithoutInterpolation()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var message = $""Hello, World!"";
                }
            }
            ");

        var operation = GetInterpolatedStringOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitInterpolatedString(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("'Hello, World!'", script);
    }

    [TestMethod]
    public void VisitObjectCreation_BlockCode()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var simpleObj = new MyClass();
                    var paramObj = new MyClass(42, ""test"");
                    var anonymousObj = new { Name = ""John"", Age = 30 };
                    var array1 = new int[] { 1, 2, 3 };
                    var array2 = new int[5];
                    string name = ""John"";
                    var message = $""Hello, {name}!"";
                }

                class MyClass
                {
                    public MyClass() { }
                    public MyClass(int number, string text) { }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let simpleObj = new MyClass;
  let paramObj = new MyClass(42, ""test"");
  let anonymousObj = { Name: ""John"", Age: 30 };
  let array1 = [1, 2, 3];
  let array2 = new Array(5);
  let name = ""John"";
  let message = `Hello, ${name}!`;
}", script);
    }

    [TestMethod]
    public void VisitObjectCreation_EmptyObjectInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new MyClass { };
                }

                class MyClass
                {
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("new MyClass", script);
    }

    [TestMethod]
    public void VisitObjectCreation_MultipleLevelsOfNesting()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new Outer
                    {
                        Middle = new Middle
                        {
                            Inner = new Inner { Value = 42 }
                        }
                    };
                }

                class Outer
                {
                    public Middle? Middle { get; set; }
                }

                class Middle
                {
                    public Inner? Inner { get; set; }
                }

                class Inner
                {
                    public int Value { get; set; }
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"(() => {
  let v$0 = new Outer;
  v$0.Middle = new Middle;
  v$0.Middle.Inner = new Inner;
  v$0.Middle.Inner.Value = 42;
  return v$0;
})()", script);

    }

    [TestMethod]
    public void VisitObjectCreation_MixedInitializers()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new MyClass(1, 2)
                    {
                        Prop1 = ""value1"",
                        Prop2 = 42,
                        Nested = new NestedClass { NestedProp = ""nested"" }
                    };
                }

                class MyClass(int a, int b)
                {
                    public int A { get; } = a;
                    public int B { get; } = b;
                    public string? Prop1 { get; set; }
                    public int Prop2 { get; set; }
                    public NestedClass? Nested { get; set; }
                }

                class NestedClass
                {
                    public string? NestedProp { get; set; }
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"(() => {
  let v$0 = new MyClass(1, 2);
  v$0.Prop1 = ""value1"";
  v$0.Prop2 = 42;
  v$0.Nested = new NestedClass;
  v$0.Nested.NestedProp = ""nested"";
  return v$0;
})()", script);

    }

    [TestMethod]
    public void VisitAnonymousObjectCreation_NestedAnonymousObject()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new
                    {
                        Name = ""John"",
                        Address = new { City = ""New York"", Zip = 10001 }
                    };
                }
            }
            ");

        var operation = GetAnonymousObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitAnonymousObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual(@"{Name:""John"",Address:{City:""New York"",Zip:10001}}", script);
    }

    [TestMethod]
    public void VisitAnonymousObjectCreation_WithArray()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Name = ""John"", Scores = new[] { 85, 92, 78 } };
                }
            }
            ");

        var operation = GetAnonymousObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitAnonymousObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual(@"{Name:""John"",Scores:[85,92,78]}", script);
    }

    [TestMethod]
    public void VisitAnonymousObjectCreation_EmptyAnonymousObject()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { };
                }
            }
            ");

        var operation = GetAnonymousObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitAnonymousObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{}", script);
    }

    [TestMethod]
    public void VisitAnonymousObjectCreation_WithNullValue()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string? name = null;
                    var obj = new { Name = name, Age = 30 };
                }
            }
            ");

        var operation = GetAnonymousObjectCreationOperationAt(block, 1);
        var walker = new SemanticWalker(true);
        var node = walker.VisitAnonymousObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{Name:name,Age:30}", script);
    }

    [TestMethod]
    public void VisitArrayCreation_WithMultipleDimensions_ShouldHandleGracefully()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new int[5, 5];
                }
            }
            ");

        var operation = GetArrayCreationOperationAt(block);
        var walker = new SemanticWalker(true);

        // 多维数组应该转换失败
        Assert.Throws<OperationTransformationException>(() => walker.VisitArrayCreation(operation, new()));
    }

    [TestMethod]
    public void VisitArrayCreation_StringArray()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new string[] { ""Hello"", ""World"" };
                }
            }
            ");

        var operation = GetArrayCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual(@"[""Hello"",""World""]", script);
    }

    [TestMethod]
    public void VisitArrayCreation_MixedTypeArray()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new object[] { 42, ""test"", true };
                }
            }
            ");

        var operation = GetArrayCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual(@"[42,""test"",true]", script);
    }

    [TestMethod]
    public void VisitArrayCreation_NestedArrays()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new int[][] { new[] { 1, 2 }, new[] { 3, 4 } };
                }
            }
            ");

        var operation = GetArrayCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("[[1,2],[3,4]]", script);
    }

    [TestMethod]
    public void VisitDelegateCreation_MethodGroup()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Action action = MyMethod;
                }

                void MyMethod() { }
            }
            ");

        var operation = GetOperationAt<IVariableDeclarationGroupOperation>(block);
        var variableDeclaration = operation.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer!;
        var delegateCreationOp = (IDelegateCreationOperation)initializer.Value;

        var walker = new SemanticWalker(true);
        var node = walker.VisitDelegateCreation(delegateCreationOp, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("this.MyMethod.bind(this)", script);
    }

    [TestMethod]
    public void VisitDelegateCreation_Lambda()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Func<int, int> func = x => x * 2;
                }
            }
            ");

        var operation = GetOperationAt<IVariableDeclarationGroupOperation>(block);
        var variableDeclaration = operation.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer!;
        var delegateCreationOp = (IDelegateCreationOperation)initializer.Value;

        var watcher = new SemanticWalker(true);
        var node = watcher.VisitDelegateCreation(delegateCreationOp, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("x=>{return x*2}", script);
    }

    [TestMethod]
    public void VisitObjectOrCollectionInitializer_EmptyInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new MyClass { };
                }

                class MyClass
                {
                    public string? Prop1 { get; set; }
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectOrCollectionInitializer(operation.Initializer!, new());
        var script = node?.ToECMAScript();

        Assert.IsNull(script);
    }

    [TestMethod]
    public void VisitObjectOrCollectionInitializer_CollectionInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new System.Collections.Generic.List<int> { 1, 2, 3 };
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectOrCollectionInitializer(operation.Initializer!, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(@"v$0.Add(1), v$0.Add(2), v$0.Add(3)", script);
    }

    [TestMethod]
    public void VisitObjectOrCollectionInitializer_ComplexCollectionInitializer()
    {
        var block = GetBlockOperation(@"
            using System.Collections.Generic;
            class TestClass
            {
                void TestMethod()
                {
                    var list = new List<List<int>> {
                        new(){1},new(){2,4},new(){3},
                    };
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectOrCollectionInitializer(operation.Initializer!, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"v$0.Add((() => {
  let v$1 = [];
  v$1.Add(1);
  return v$1;
})()), v$0.Add((() => {
  let v$2 = [];
  v$2.Add(2);
  v$2.Add(4);
  return v$2;
})()), v$0.Add((() => {
  let v$3 = [];
  v$3.Add(3);
  return v$3;
})())", script);

    }

    [TestMethod]
    public void VisitMemberInitializer_FieldAssignment()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new MyClass { Field1 = 42, Field2 = ""test"" };
                }

                class MyClass
                {
                    public int Field1;
                    public string? Field2;
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"(() => {
  let v$0 = new MyClass;
  v$0.Field1 = 42;
  v$0.Field2 = ""test"";
  return v$0;
})()", script);

    }

    [TestMethod]
    public void VisitArrayCreation_SingleElement()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new int[] { 42 };
                }
            }
            ");

        var operation = GetArrayCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("[42]", script);
    }

    [TestMethod]
    public void VisitAnonymousObjectCreation_BooleanAndNumericValues()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Flag = true, Count = 100, Price = 19.99 };
                }
            }
            ");

        var operation = GetAnonymousObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitAnonymousObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{Flag:true,Count:100,Price:19.99}", script);
    }

    [TestMethod]
    public void VisitObjectCreation_BigIntType()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var num = new System.Numerics.BigInteger(42);
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        // BigInt 类型使用 CallExpression 而非 NewExpression
        // GetMapperType 将 BigInteger 类型名称映射为 "BigInt"
        Assert.AreEqual("BigInt(42)", script);
    }

    [TestMethod]
    public void VisitObjectCreation_AsArgument()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    StaticMethod(new MyClass());
                }

                static void StaticMethod(MyClass obj) { }

                class MyClass
                {
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        // 当对象创建作为方法参数时，会直接内联创建
        // 静态方法调用使用类名前缀
        Assert.AreEqual(
@"{
  TestClass.StaticMethod(new MyClass);
}", script);

    }

    [TestMethod]
    public void VisitDelegateCreation_ExplicitDelegateCreation()
    {
        var block = GetBlockOperation(@"
            using System;
            class TestClass
            {
                void TestMethod()
                {
                    Action action = new Action(MyMethod);
                }

                void MyMethod() { }
            }
            ");

        var operation = GetOperationAt<IVariableDeclarationGroupOperation>(block);
        var variableDeclaration = operation.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer!;
        var delegateCreationOp = (IDelegateCreationOperation)initializer.Value;

        var walker = new SemanticWalker(true);
        var node = walker.VisitDelegateCreation(delegateCreationOp, new());
        var script = node?.ToECMAScript();

        // 实例方法会带有 this 前缀
        Assert.AreEqual("this.MyMethod.bind(this)", script);
    }

    [TestMethod]
    public void VisitDelegateCreation_WithLambda()
    {
        var block = GetBlockOperation(@"
            using System;
            class TestClass
            {
                void TestMethod()
                {
                    Func<int, int> func = new Func<int, int>(x => x * 2);
                }
            }
            ");

        var operation = GetOperationAt<IVariableDeclarationGroupOperation>(block);
        var variableDeclaration = operation.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer!;
        var delegateCreationOp = (IDelegateCreationOperation)initializer.Value;

        var walker = new SemanticWalker(true);
        var node = walker.VisitDelegateCreation(delegateCreationOp, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("x=>{return x*2}", script);
    }

    [TestMethod]
    public void VisitObjectCreation_NestedArgument()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new System.Collections.Generic.List<int>();
                    // 嵌套对象创建作为参数
                    list.Add((new Outer { Inner = new Inner { Value = 42 } }).Inner.Value);
                }

                class Outer
                {
                    public Inner? Inner { get; set; }
                }

                class Inner
                {
                    public int Value { get; set; }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        // 验证嵌套对象创建作为参数时的处理
        // 当对象创建没有初始化器时，会直接内联创建
        Assert.AreEqual(
@"{
  let list = [];
  list.push((() => {
    let v$0 = new Outer;
    v$0.Inner = new Inner;
    v$0.Inner.Value = 42;
    return v$0;
  })().Inner.Value);
}", script);

    }

    [TestMethod]
    public void VisitArrayCreation_JaggedArray()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var jagged = new int[3][];
                }
            }
            ");

        var operation = GetArrayCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        // 交错数组（数组的数组）应该创建指定大小的数组
        Assert.AreEqual("new Array(3)", script);
    }

    [TestMethod]
    public void VisitObjectCreation_WithComplexArguments()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new MyClass(
                        42,
                        ""hello"",
                        ""world""
                    );
                }

                class MyClass(int a, string b, string c) { }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual(@"new MyClass(42,""hello"",""world"")", script);
    }

    [TestMethod]
    public void VisitAnonymousObjectCreation_DeeplyNested()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new {
                        Level1 = new {
                            Level2 = new {
                                Level3 = ""deep""
                            }
                        }
                    };
                }
            }
            ");

        var operation = GetAnonymousObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitAnonymousObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual(@"{Level1:{Level2:{Level3:""deep""}}}", script);
    }

    [TestMethod]
    public void VisitArrayCreation_WithNullElements()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new string?[] { ""hello"", null, ""world"" };
                }
            }
            ");

        var operation = GetArrayCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual(@"[""hello"",null,""world""]", script);
    }

    [TestMethod]
    public void VisitInterpolatedString_WithEscapedBraces()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 42;
                    var message = $""The value is {{value}}: {value}"";
                }
            }
            ");

        var operation = GetInterpolatedStringOperationAt(block, 1);
        var walker = new SemanticWalker(true);
        var node = walker.VisitInterpolatedString(operation, new());
        var script = node?.ToECMAScript();

        // 双大括号转义为单大括号
        Assert.AreEqual(@"`The value is {value}: ${value}`", script);
    }

    [TestMethod]
    public void VisitObjectCreation_MultipleNestedLevels()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new A {
                        B = new B {
                            C = new C {
                                D = new D {
                                    Value = 999
                                }
                            }
                        }
                    };
                }

                class A { public B? B { get; set; } }
                class B { public C? C { get; set; } }
                class C { public D? D { get; set; } }
                class D { public int Value { get; set; } }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        // 多层嵌套会逐级创建对象
        Assert.AreEqual(
@"(() => {
  let v$0 = new A;
  v$0.B = new B;
  v$0.B.C = new C;
  v$0.B.C.D = new D;
  v$0.B.C.D.Value = 999;
  return v$0;
})()", script);

    }

    // ========== 补充未覆盖的高优先级测试用例 ==========

    [TestMethod]
    public void VisitObjectCreation_DateType()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var date = new System.DateTime(2024, 1, 1);
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        // DateTime 映射为 Date
        Assert.AreEqual("new Date(2024,1-1,1)", script);
    }

    [TestMethod]
    public void VisitObjectCreation_DateOnlyType()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var date = new System.DateOnly(2024, 1, 1);
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        // DateOnly 映射为 Date
        Assert.AreEqual("new Date(2024,1-1,1)", script);
    }

    [TestMethod]
    public void VisitObjectCreation_TimeOnlyType()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var time = new System.TimeOnly(12, 30, 0);
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        // TimeOnly 映射为 Number秒
        Assert.AreEqual("12*3600000+30*60000+0*1000", script);
    }

    [TestMethod]
    public void VisitObjectCreation_TimeSpanType()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var span = new System.TimeSpan(1, 2, 3);
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        // TimeSpan 映射为 BigInt，使用 CallExpression
        Assert.AreEqual("BigInt(1,2,3)", script);
    }

    [TestMethod]
    public void VisitObjectCreation_ListType()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new System.Collections.Generic.List<int>();
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        // List 映射为 Array
        Assert.AreEqual("[]", script);
    }

    [TestMethod]
    public void VisitObjectCreation_DictionaryType()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dict = new System.Collections.Generic.Dictionary<int, string>();
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        // Dictionary 映射为 Map
        Assert.AreEqual("new Map", script);
    }

    [TestMethod]
    public void VisitObjectCreation_HashSetType()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var set = new System.Collections.Generic.HashSet<int>();
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        // HashSet 映射为 Set
        Assert.AreEqual("new Set", script);
    }

    [TestMethod]
    public void VisitObjectCreation_AsArgument_WithInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    // IArgumentOperation 分支设计用于处理：M(new Derived()) 这种语法
                    // 操作链：IObjectCreationOperation -> IConversionOperation -> IArgumentOperation
                    // 目的：当对象创建直接作为参数时，如有初始化器，需要先创建临时变量
                    ProcessObject(new MyClass { Value = 42 });
                }

                void ProcessObject(MyClass obj) { }

                class MyClass
                {
                    public int Value { get; set; }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        // 当前实际行为：对象初始化器在作为参数时被忽略
        // IArgumentOperation 分支可能因以下原因未被触发：
        // 1. Roslyn 操作树结构与预期不同
        // 2. 需要特定的类型转换场景才包含 IConversionOperation
        // 3. 条件检查 operation.Parent?.Parent 可能需要调整
        Assert.AreEqual(
@"{
  this.ProcessObject((() => {
    let v$0 = new MyClass;
    v$0.Value = 42;
    return v$0;
  })());
}", script);

    }

    [TestMethod]
    public void VisitObjectCreation_AsArgument_WithInitializer1()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    ProcessObject(new MyClass { Value = 42 });
                }

                void ProcessObject(object obj) { }

                class MyClass
                {
                    public int Value { get; set; }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        // 当前实际行为：对象初始化器在作为参数时被忽略
        // IArgumentOperation 分支可能因以下原因未被触发：
        // 1. Roslyn 操作树结构与预期不同
        // 2. 需要特定的类型转换场景才包含 IConversionOperation
        // 3. 条件检查 operation.Parent?.Parent 可能需要调整
        Assert.AreEqual(
@"{
  this.ProcessObject((() => {
    let v$0 = new MyClass;
    v$0.Value = 42;
    return v$0;
  })());
}", script);


    }

    [TestMethod]
    public void VisitObjectOrCollectionInitializer_WithComplexObjectCreation()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new System.Collections.Generic.List<int>();
                    // 集合初始化器中，参数是复杂对象创建
                    list.Add(new Outer().Value);
                }

                class Outer
                {
                    public int Value { get; set; } = 100;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let list = [];
  list.push((new Outer).Value);
}", script);

    }

    [TestMethod]
    public void VisitArrayCreation_ZeroSize()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new int[0];
                }
            }
            ");

        var operation = GetArrayCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        // 大小为 0 的数组
        Assert.AreEqual("new Array(0)", script);
    }

    [TestMethod]
    public void VisitObjectCreation_WithExpressionArguments()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 10, y = 20;
                    var obj = new MyClass(x + y, x * y);
                }

                class MyClass(int a, int b) { }
            }
            ");

        var operation = GetObjectCreationOperationAt(block, 1);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("new MyClass(x+y,x*y)", script);
    }

    [TestMethod]
    public void VisitObjectCreation_DateTimeOffsetType()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var offset = new System.DateTimeOffset(2024, 1, 1, 0, 0, 0, System.TimeSpan.Zero);
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        // TimeSpan.Zero 被白名单内联转换为 0n（BigInt 零）
        // DateTimeOffset 被正确映射为 Date
        Assert.AreEqual("new Date(2024,1,1,0,0,0,0n)", script);
    }
}
