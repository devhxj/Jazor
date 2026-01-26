using Acornima.Ast;
using ECMAScript.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace ECMAScript.ComplierTest;

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
        var script = node?.ToECMAScript();

        Assert.AreEqual(@"new MyClass(2,3);obj.Property1=""value1"";obj.Property2=42;", script);
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
        var left = new Identifier("obj");
        var node = walker.VisitObjectOrCollectionInitializer(operation.Initializer!, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual(@"obj.Property1=""value1"";obj.Property2=42;", script);
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
        var left = new Identifier("obj");
        //var memberInitializer = (IMemberInitializerOperation)operation.Initializer!.Initializers.First();
        //var node = walker.VisitMemberInitializer(memberInitializer, new());
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual(@"new A;obj.A1={B1:""Test"",B2:{C1:""a"",C2:9}};obj.A2=""value"";", script);
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
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(@"{
  let simpleObj = new MyClass;
  let paramObj = new MyClass(42, 'test');
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

        Assert.AreEqual("new MyClass;", script);
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
        var script = node?.ToECMAScript();

        Assert.AreEqual("new Outer;obj.Middle=new Middle;obj.Middle.Inner=new Inner;obj.Middle.Inner.Value=42;", script);
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
        var left = new Identifier("obj");
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(@"new MyClass(1, 2);
obj.Prop1 = ""value1"";
obj.Prop2 = 42;
obj.Nested = new NestedClass;
obj.Nested.NestedProp = ""nested"";
", script);
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
    public void VisitConversionOperation_MethodGroup()
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
        var conversionOp = (IConversionOperation)initializer.Value;

        var walker = new SemanticWalker(true);
        var node = walker.VisitConversion(conversionOp, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("MyMethod", script);
    }

    [TestMethod]
    public void VisitConversionOperation_Lambda()
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
        var conversionOp = (IConversionOperation)initializer.Value;

        var watcher = new SemanticWalker(true);
        var node = watcher.VisitConversion(conversionOp, new());
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
        var left = new Identifier("obj");
        var node = walker.VisitObjectOrCollectionInitializer(operation.Initializer!, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("", script);
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
        var node = walker.VisitObjectOrCollectionInitializer(operation.Initializer!,new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("list.Add(1);list.Add(2);list.Add(3);", script);
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
        var node = walker.VisitObjectOrCollectionInitializer(operation.Initializer!,new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"v$test = new Array;
v$test.Add(1);
list.Add(v$test);
v$test = new Array;
v$test.Add(2);
v$test.Add(4);
list.Add(v$test);
v$test = new Array;
v$test.Add(3);
list.Add(v$test);
", script);
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
        var left = new Identifier("obj");
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual(@"new MyClass;obj.Field1=42;obj.Field2=""test"";", script);
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
                    // 对象创建作为方法参数（不带初始化器）
                    StaticMethod(new MyClass());
                }

                static void StaticMethod(MyClass obj) { }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        // 当对象创建作为方法参数时，会直接内联创建
        // 静态方法调用使用类名前缀
        Assert.AreEqual(@"{
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
        Assert.AreEqual("this.MyMethod", script);
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
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        // 验证嵌套对象创建作为参数时的处理
        // 当对象创建没有初始化器时，会直接内联创建
        Assert.AreEqual(@"{
  let list = new Array;
  list.Add((new Outer).Inner.Value);
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
        var script = node?.ToECMAScript();

        // 多层嵌套会逐级创建对象
        Assert.AreEqual(@"new A;obj.B=new B;obj.B.C=new C;obj.B.C.D=new D;obj.B.C.D.Value=999;", script);
    }
}
