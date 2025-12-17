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
            [CSharpSyntaxTree.ParseText(code)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

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
    private static T GetOperationAt<T>(string code, int index = 0) where T : class, IOperation
    {
        var block = GetBlockOperation(code);
        var operation = block.Operations.Skip(index).First() as T;

        return operation ?? throw new InvalidOperationException("未找到可分析的操作");
    }

    /// <summary>
    /// 获取对象创建操作
    /// </summary>
    /// <param name="code"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private static IObjectCreationOperation GetObjectCreationOperationAt(string code, int index = 0)
    {
        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(code, index);
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
    private static IArrayCreationOperation GetArrayCreationOperationAt(string code, int index = 0)
    {
        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(code, index);
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
    private static IAnonymousObjectCreationOperation GetAnonymousObjectCreationOperationAt(string code, int index = 0)
    {
        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(code, index);
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
    private static IInterpolatedStringOperation GetInterpolatedStringOperationAt(string code, int index = 0)
    {
        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(code, index);
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
    private static ITypeParameterObjectCreationOperation GetTypeParameterObjectCreationOperationAt(string code, int index = 0)
    {
        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(code, index);
        var variableDeclaration = variableDeclarationGroup!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (ITypeParameterObjectCreationOperation)initializer!.Value;
        return operation;
    }

    [TestMethod]
    public void VisitObjectCreation_SimpleConstructor()
    {
        var code = @"
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
            ";

        var operation = GetObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("new MyClass", script);
    }

    [TestMethod]
    public void VisitObjectCreation_ConstructorWithParameters()
    {
        var code = @"
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
            ";

        var operation = GetObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("new MyClass(42,'test')", script);
    }

    [TestMethod]
    public void VisitObjectCreation_AnonymousType()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Name = ""John"", Age = 30 };
                }
            }
            ";

        var operation = GetAnonymousObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitAnonymousObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{Name:'John',Age:30}", script);
    }

    [TestMethod]
    public void VisitObjectCreation_GenericType()
    {
        var code = @"
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
            ";

        var operation = GetObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("new MyClass", script);
    }

    [TestMethod]
    public void VisitArrayCreation_WithInitializer()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new int[] { 1, 2, 3 };
                }
            }
            ";

        var operation = GetArrayCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("[1,2,3]", script);
    }

    [TestMethod]
    public void VisitArrayCreation_WithSize()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new int[5];
                }
            }
            ";

        var operation = GetArrayCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("new Array(5)", script);
    }

    [TestMethod]
    public void VisitArrayCreation_EmptyArray()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new int[] { };
                }
            }
            ";

        var operation = GetArrayCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("[]", script);
    }

    [TestMethod]
    public void VisitAnonymousObjectCreation_SimpleProperties()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Name = ""John"", Age = 25 };
                }
            }
            ";

        var operation = GetAnonymousObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitAnonymousObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{Name:'John',Age:25}", script);
    }

    [TestMethod]
    public void VisitAnonymousObjectCreation_WithExpressions()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 10, y = 20;
                    var obj = new { Sum = x + y, Product = x * y };
                }
            }
            ";

        var operation = GetAnonymousObjectCreationOperationAt(code, 1);
        var walker = new SemanticWalker(true);
        var node = walker.VisitAnonymousObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{Sum:x+y,Product:x*y}", script);
    }

    [TestMethod]
    public void VisitObjectCreation_ObjectInitializer()
    {
        var code = @"
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
            ";

        var operation = GetObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var left = new Identifier("obj");
        var node = walker.VisitObjectCreation(operation, (left, Scene.Any, []));
        var script = node?.ToECMAScript();

        Assert.AreEqual("new MyClass(2,3);obj.Property1='value1';obj.Property2=42;", script);
    }

    [TestMethod]
    public void VisitObjectOrCollectionInitializer_ObjectInitializer()
    {
        var code = @"
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
            ";

        var operation = GetObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var left = new Identifier("obj");
        var node = walker.VisitObjectOrCollectionInitializer(operation.Initializer!, (left, Scene.Any, []));
        var script = node?.ToECMAScript();

        Assert.AreEqual("obj.Property1='value1';obj.Property2=42;", script);
    }

    [TestMethod]
    public void VisitMemberInitializer_PropertyAssignment()
    {
        var code = @"
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
            ";

        var walker = new SemanticWalker(true);
        var operation = GetObjectCreationOperationAt(code);
        var left = new Identifier("obj");
        //var memberInitializer = (IMemberInitializerOperation)operation.Initializer!.Initializers.First();
        //var node = walker.VisitMemberInitializer(memberInitializer, new());
        var node = walker.VisitObjectCreation(operation, (left, Scene.Any, []));
        var script = node?.ToECMAScript();

        Assert.AreEqual("new A;obj.A1={B1:'Test',B2:{C1:'a',C2:9}};obj.A2='value';", script);
    }

    [TestMethod]
    public void VisitInterpolatedString_SimpleInterpolation()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    string name = ""John"";
                    var message = $""Hello, {name}!"";
                }
            }
            ";

        var operation = GetInterpolatedStringOperationAt(code, 1);
        var walker = new SemanticWalker(true);
        var node = walker.VisitInterpolatedString(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("`Hello, ${name}!`", script);
    }

    [TestMethod]
    public void VisitInterpolatedString_WithExpressions()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5, y = 10;
                    var message = $""Sum: {x + y}, Product: {x * y}"";
                }
            }
            ";

        var operation = GetInterpolatedStringOperationAt(code, 1);
        var walker = new SemanticWalker(true);
        var node = walker.VisitInterpolatedString(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("`Sum: ${x+y}, Product: ${x*y}`", script);
    }

    [TestMethod]
    public void VisitInterpolatedString_MultipleInterpolations()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    string name = ""John"";
                    int age = 30;
                    var message = $""Name: {name}, Age: {age}"";
                }
            }
            ";

        var operation = GetInterpolatedStringOperationAt(code, 2);
        var walker = new SemanticWalker(true);
        var node = walker.VisitInterpolatedString(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("`Name: ${name}, Age: ${age}`", script);
    }

    [TestMethod]
    public void VisitInterpolatedString_WithoutInterpolation()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var message = $""Hello, World!"";
                }
            }
            ";

        var operation = GetInterpolatedStringOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitInterpolatedString(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("'Hello, World!'", script);
    }

    [TestMethod]
    public void VisitObjectCreation_BlockCode()
    {
        var code = @"
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
            ";

        var block = GetBlockOperation(code);
        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(@"{
  let simpleObj = new MyClass;
  let paramObj = new MyClass(42, 'test');
  let anonymousObj = { Name: 'John', Age: 30 };
  let array1 = [1, 2, 3];
  let array2 = new Array(5);
  let name = 'John';
  let message = `Hello, ${name}!`;
}", script);
    }

    [TestMethod]
    public void VisitObjectCreation_EmptyObjectInitializer()
    {
        var code = @"
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
            ";

        var operation = GetObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("new MyClass;", script);
    }

    [TestMethod]
    public void VisitObjectCreation_MultipleLevelsOfNesting()
    {
        var code = @"
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
            ";

        var operation = GetObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var left = new Identifier("obj");
        var node = walker.VisitObjectCreation(operation, (left, Scene.Any, []));
        var script = node?.ToECMAScript();

        Assert.AreEqual("new Outer;obj.Middle=new Middle;obj.Middle.Inner=new Inner;obj.Middle.Inner.Value=42;", script);
    }

    [TestMethod]
    public void VisitObjectCreation_MixedInitializers()
    {
        var code = @"
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
            ";

        var operation = GetObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var left = new Identifier("obj");
        var node = walker.VisitObjectCreation(operation, (left, Scene.Any, []));
        var script = node?.ToECMAScript();

        Assert.AreEqual("new MyClass(1,2);obj.Prop1='value1';obj.Prop2=42;obj.Nested=new NestedClass;obj.Nested.NestedProp='nested';", script);
    }

    [TestMethod]
    public void VisitAnonymousObjectCreation_NestedAnonymousObject()
    {
        var code = @"
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
            ";

        var operation = GetAnonymousObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitAnonymousObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{Name:'John',Address:{City:'New York',Zip:10001}}", script);
    }

    [TestMethod]
    public void VisitAnonymousObjectCreation_WithArray()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Name = ""John"", Scores = new[] { 85, 92, 78 } };
                }
            }
            ";

        var operation = GetAnonymousObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitAnonymousObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{Name:'John',Scores:[85,92,78]}", script);
    }

    [TestMethod]
    public void VisitAnonymousObjectCreation_EmptyAnonymousObject()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { };
                }
            }
            ";

        var operation = GetAnonymousObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitAnonymousObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{}", script);
    }

    [TestMethod]
    public void VisitAnonymousObjectCreation_WithNullValue()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    string? name = null;
                    var obj = new { Name = name, Age = 30 };
                }
            }
            ";

        var operation = GetAnonymousObjectCreationOperationAt(code, 1);
        var walker = new SemanticWalker(true);
        var node = walker.VisitAnonymousObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{Name:name,Age:30}", script);
    }

    [TestMethod]
    public void VisitArrayCreation_WithMultipleDimensions_ShouldHandleGracefully()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new int[5, 5];
                }
            }
            ";

        var operation = GetArrayCreationOperationAt(code);
        var walker = new SemanticWalker(true);

        // 多维数组应该转换失败
        Assert.Throws<OperationTransformationException>(() => walker.VisitArrayCreation(operation, new()));
    }

    [TestMethod]
    public void VisitArrayCreation_StringArray()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new string[] { ""Hello"", ""World"" };
                }
            }
            ";

        var operation = GetArrayCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("['Hello','World']", script);
    }

    [TestMethod]
    public void VisitArrayCreation_MixedTypeArray()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new object[] { 42, ""test"", true };
                }
            }
            ";

        var operation = GetArrayCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("[42,'test',true]", script);
    }

    [TestMethod]
    public void VisitArrayCreation_NestedArrays()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new int[][] { new[] { 1, 2 }, new[] { 3, 4 } };
                }
            }
            ";

        var operation = GetArrayCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("[[1,2],[3,4]]", script);
    }

    [TestMethod]
    public void VisitConversionOperation_MethodGroup()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    Action action = MyMethod;
                }

                void MyMethod() { }
            }
            ";

        var operation = GetOperationAt<IVariableDeclarationGroupOperation>(code);
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
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    Func<int, int> func = x => x * 2;
                }
            }
            ";

        var operation = GetOperationAt<IVariableDeclarationGroupOperation>(code);
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
        var code = @"
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
            ";

        var operation = GetObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var left = new Identifier("obj");
        var node = walker.VisitObjectOrCollectionInitializer(operation.Initializer!, (left, Scene.Any, []));
        var script = node?.ToECMAScript();

        Assert.AreEqual("", script);
    }

    [TestMethod]
    public void VisitObjectOrCollectionInitializer_CollectionInitializer()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new System.Collections.Generic.List<int> { 1, 2, 3 };
                }
            }
            ";

        var operation = GetObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectOrCollectionInitializer(operation.Initializer!,
            (new Identifier("list"), Scene.Any, new()));
        var script = node?.ToECMAScript();

        Assert.AreEqual("list.Add(1);list.Add(2);list.Add(3);", script);
    }

    [TestMethod]
    public void VisitObjectOrCollectionInitializer_ComplexCollectionInitializer()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new System.Collections.Generic.List<List<int>> {
                        new(){1},new(){2,4},new(){3},
                    };
                }
            }
            ";

        var operation = GetObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectOrCollectionInitializer(operation.Initializer!,
            (new Identifier("list"), Scene.Any, new()));
        var script = node?.ToECMAScript();

        Assert.AreEqual("list.Add(1);list.Add(2);list.Add(3);", script);
    }    

    [TestMethod]
    public void VisitMemberInitializer_FieldAssignment()
    {
        var code = @"
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
            ";

        var operation = GetObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var left = new Identifier("obj");
        var node = walker.VisitObjectCreation(operation, (left, Scene.Any, []));
        var script = node?.ToECMAScript();

        Assert.AreEqual("new MyClass;obj.Field1=42;obj.Field2='test';", script);
    }

    [TestMethod]
    public void VisitArrayCreation_SingleElement()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new int[] { 42 };
                }
            }
            ";

        var operation = GetArrayCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("[42]", script);
    }

    [TestMethod]
    public void VisitAnonymousObjectCreation_BooleanAndNumericValues()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Flag = true, Count = 100, Price = 19.99 };
                }
            }
            ";

        var operation = GetAnonymousObjectCreationOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitAnonymousObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{Flag:true,Count:100,Price:19.99}", script);
    }
}
