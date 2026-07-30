using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Text.RegularExpressions;

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

        var references = TestMetadataReferences.Net11
            .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [
              CSharpSyntaxTree.ParseText(usings, TestMetadataReferences.PreviewParseOptions),
              CSharpSyntaxTree.ParseText(code, TestMetadataReferences.PreviewParseOptions)
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
        var methodDeclaration = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(static method => method.Identifier.ValueText == "TestMethod" && method.Body is not null)
            ?? root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(static method => method.Body is not null);
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

    // 统一脚本断言，既锁定完整 JS 输出风格，也屏蔽平台行尾差异。
    private static void AssertScriptEqual(string expected, string? actual)
    {
        Assert.AreEqual(ExpectedJsNaming.Normalize(expected).ReplaceLineEndings(), actual?.ReplaceLineEndings());
    }

    private static string NormalizeExpectedJsNaming(string expected)
    {
        expected = Regex.Replace(expected, @"\bItem([0-9]+)\b", "item$1");
        expected = Regex.Replace(expected, @"([\{\[,]\s*)([A-Z][A-Za-z0-9_]*)(\s*:)", static m => m.Groups[1].Value + Camel(m.Groups[2].Value) + m.Groups[3].Value);
        expected = Regex.Replace(expected, @"(^\s*)([A-Z][A-Za-z0-9_]*)(\s*:)", static m => m.Groups[1].Value + Camel(m.Groups[2].Value) + m.Groups[3].Value, RegexOptions.Multiline);
        expected = Regex.Replace(expected, @"(\?*\.)([A-Z][A-Za-z0-9_]*)", static m => m.Groups[1].Value + Camel(m.Groups[2].Value));
        expected = Regex.Replace(expected, @"""([A-Z][A-Za-z0-9_]*)""(\s+in\b)", static m => "\"" + Camel(m.Groups[1].Value) + "\"" + m.Groups[2].Value);
        expected = Regex.Replace(expected, @"\[""([A-Z][A-Za-z0-9_]*)""\]", static m => "[\"" + Camel(m.Groups[1].Value) + "\"]");
        return expected;
    }

    private static string Camel(string name)
    {
        if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
            return name;

        if (name.Length == 1)
            return char.ToLowerInvariant(name[0]).ToString();

        var chars = name.ToCharArray();
        chars[0] = char.ToLowerInvariant(chars[0]);
        for (var index = 1; index < chars.Length; index++)
        {
            if (!char.IsUpper(chars[index]))
                break;

            var hasNext = index + 1 < chars.Length;
            if (hasNext && !char.IsUpper(chars[index + 1]))
                break;

            chars[index] = char.ToLowerInvariant(chars[index]);
        }

        return new string(chars);
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

        AssertScriptEqual("new MyClass", script);
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

        AssertScriptEqual(@"new MyClass(42,""test"")", script);
    }

    [TestMethod]
    public void VisitObjectCreation_UnsupportedExternalType_Throws()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var random = new Random();
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);

        Assert.Throws<OperationTransformationException>(() =>
        {
            _ = walker.VisitObjectCreation(operation, new());
        });
    }

    [TestMethod]
    public void VisitObjectCreation_WhitelistContainerWithErasedUnsupportedTypeArgument_Allows()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new List<Random>();
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        AssertScriptEqual("[]", script);
    }

    [TestMethod]
    public void VisitTypeParameterObjectCreation_Throws()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                T Create<T>() where T : new()
                {
                    var value = new T();
                    return value;
                }
            }
            ");

        var operation = GetTypeParameterObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);

        Assert.Throws<OperationTransformationException>(() =>
        {
            _ = walker.VisitTypeParameterObjectCreation(operation, new());
        });
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

        AssertScriptEqual(@"{Name:""John"",Age:30}", script);
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

        AssertScriptEqual("new MyClass", script);
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

        AssertScriptEqual("[1,2,3]", script);
    }

    [TestMethod]
    public void VisitArrayCreation_ErasedUnsupportedElementType_Allows()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var items = new Random[1];
                }
            }
            ");

        var operation = GetArrayCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        AssertScriptEqual("new Array(1)", script);
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

        AssertScriptEqual("new Array(5)", script);
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

        AssertScriptEqual("[]", script);
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

        AssertScriptEqual(@"{Name:""John"",Age:25}", script);
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

        AssertScriptEqual("{Sum:x+y,Product:x*y}", script);
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

        AssertScriptEqual(
@"(() => {
  let v$0 = new MyClass(2, 3);
  v$0.Property1 = ""value1"";
  v$0.Property2 = 42;
  return v$0;
})()", script);

    }

    [TestMethod]
    public void VisitObjectCreation_ObjectInitializer_TupleRemap()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new MyClass { Person = (name: ""John"", age: 30) };
                }

                class MyClass
                {
                    public (string first, int years) Person { get; set; }
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"(() => {
  let v$0 = new MyClass;
  v$0.Person = { first: ""John"", years: 30 };
  return v$0;
})()".ReplaceLineEndings(), script?.ReplaceLineEndings());
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

        AssertScriptEqual(@"v$0.Property1 = ""value1"", v$0.Property2 = 42", script);

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
                    public B A1 { get; } = new();
                    public string? A2 { get; set; }
                }

                class B
                {
                    public B()
                    {
                        B2 = new C();
                    }

                    public string? B1 { get; set; }
                    public C B2 { get; }
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

        AssertScriptEqual(
@"(() => {
  let v$0 = new A;
  v$0.A1.B1 = ""Test"";
  v$0.A1.B2.C1 = ""a"";
  v$0.A1.B2.C2 = 9;
  v$0.A2 = ""value"";
  return v$0;
})()", script);

    }

    [TestMethod]
    public void Visit_ObjectInitializer_NestedUnsupportedExternalProperty_Throws()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var holder = new Holder
                    {
                        StartInfo = { FileName = ""cmd.exe"" }
                    };
                }

                class Holder
                {
                    public System.Diagnostics.ProcessStartInfo StartInfo { get; } = new();
                }
            }
            ");

        var walker = new SemanticWalker(true);
        Assert.Throws<OperationTransformationException>(() =>
        {
            _ = walker.Visit(block, new());
        });
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

        AssertScriptEqual("`Hello, ${name}!`", script);
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

        AssertScriptEqual("`Sum: ${x+y}, Product: ${x*y}`", script);
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

        AssertScriptEqual("`Name: ${name}, Age: ${age}`", script);
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

        AssertScriptEqual("\"Hello, World!\"", script);
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

    AssertScriptEqual(
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
    public void VisitObjectCreation_ECMAScriptArrayHost_PreservesConstructorSemantics()
    {
        var block = GetBlockOperation(@"
            using ECMAScript;

            class TestClass
            {
                void TestMethod(Number size, Number value)
                {
                    var sized = new Array<Number>(size);
                    var singleton = new Array<Number>(value);
                    var empty = new Array<Number>();
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"{
  let sized = new Array(size);
  let singleton = new Array(value);
  let empty = new Array;
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

        AssertScriptEqual("new MyClass", script);
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

        AssertScriptEqual(
@"(() => {
  let v$0 = new Outer;
  v$0.Middle = (() => {
    let v$0 = new Middle;
    v$0.Inner = (() => {
      let v$0 = new Inner;
      v$0.Value = 42;
      return v$0;
    })();
    return v$0;
  })();
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

        AssertScriptEqual(
@"(() => {
  let v$0 = new MyClass(1, 2);
  v$0.Prop1 = ""value1"";
  v$0.Prop2 = 42;
  v$0.Nested = (() => {
    let v$0 = new NestedClass;
    v$0.NestedProp = ""nested"";
    return v$0;
  })();
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

        AssertScriptEqual(@"{Name:""John"",Address:{City:""New York"",Zip:10001}}", script);
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

        AssertScriptEqual(@"{Name:""John"",Scores:[85,92,78]}", script);
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

        AssertScriptEqual("{}", script);
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

        AssertScriptEqual("{Name:name,Age:30}", script);
    }

    [TestMethod]
    public void VisitObjectCreation_RecordPrimaryConstructor_StaticNullLiteral_IsOmitted()
    {
        var block = GetBlockOperation(@"
            using System.ComponentModel;

            class TestClass
            {
                void TestMethod()
                {
                    var obj = new PersonProps(null, 30);
                }

                public sealed record PersonProps(
                    [property: Description(""@#name"")] string? Name,
                    [property: Description(""@#age"")] int Age);
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{ age: 30 }", script);
    }

    [TestMethod]
    public void VisitObjectCreation_RecordObjectInitializer_StaticNullLiteral_IsOmitted()
    {
        var block = GetBlockOperation(@"
            using System.ComponentModel;

            class TestClass
            {
                void TestMethod()
                {
                    var obj = new PersonProps
                    {
                        Name = null,
                        Age = 30
                    };
                }

                public sealed record PersonProps
                {
                    [Description(""@#name"")]
                    public string? Name { get; init; }

                    [Description(""@#age"")]
                    public int Age { get; init; }
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{ age: 30 }", script);
    }

    [TestMethod]
    public void VisitObjectCreation_RecordObjectInitializer_NonConstantNullFlow_IsPreserved()
    {
        var block = GetBlockOperation(@"
            using System.ComponentModel;

            class TestClass
            {
                void TestMethod()
                {
                    string? name = null;
                    var obj = new PersonProps
                    {
                        Name = name,
                        Age = 30
                    };
                }

                public sealed record PersonProps
                {
                    [Description(""@#name"")]
                    public string? Name { get; init; }

                    [Description(""@#age"")]
                    public int Age { get; init; }
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block, 1);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{ name: name, age: 30 }", script);
    }

    [TestMethod]
    public void VisitObjectCreation_RecordWithoutECMAScriptMarker_LowersStructurally()
    {
        var block = GetBlockOperation(@"
            public sealed record PersonProps(string Name, int Age);

            [ECMAScriptModule]
            class TestClass
            {
                void TestMethod()
                {
                    var person = new PersonProps(""Ada"", 37);
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{ name: ""Ada"", age: 37 }", script);
    }

    [TestMethod]
    public void VisitObjectCreation_SourceDataCarrierWithoutOptIn_Throws()
    {
        var block = GetBlockOperation(@"
            public readonly struct ReadyState
            {
                public ReadyState(bool value)
                {
                    Value = value;
                }

                public bool Value { get; }
            }

            public sealed class ReadyEnvelope
            {
                public ReadyEnvelope(ReadyState state)
                {
                    State = state;
                }

                public ReadyState State { get; }
            }

            [ECMAScriptModule]
            class TestClass
            {
                void TestMethod(bool firstRender)
                {
                    var payload = new ReadyEnvelope(new ReadyState(firstRender));
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);

        Assert.Throws<OperationTransformationException>(() =>
        {
            _ = walker.VisitObjectCreation(operation, new());
        });
    }

    [TestMethod]
    public void VisitObjectCreation_ObjectLiteralIndexerOnVuePropsCarrier_StaticNullLiteral_IsPreserved()
    {
        var block = GetBlockOperation(@"
            using ECMAScript;

            class TestClass
            {
                void TestMethod()
                {
                    var query = new LocationQueryRaw
                    {
                        [""empty""] = null,
                        [""page""] = (Number)1
                    };
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{ empty: null, page: 1 }", script);
    }

    [TestMethod]
    public void VisitObjectCreation_ObjectLiteralIndexerOnVuePropsCarrier_StaticUndefinedLiteral_AndMixedArray_ArePreserved()
    {
        var block = GetBlockOperation(@"
            using ECMAScript;
            using static ECMAScript.Global;

            class TestClass
            {
                void TestMethod()
                {
                    var query = new LocationQueryRaw
                    {
                        [""drop""] = Undefined<LocationQueryValueRaw?>(),
                        [""page""] = (Number)1,
                        [""tags""] = new LocationQueryValueRaw?[] { ""a"", Undefined<LocationQueryValueRaw?>(), null, (Number)3 }
                    };
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  drop: undefined,
  page: 1,
  tags: [""a"", undefined, null, 3]
}", script);
    }

    [TestMethod]
    public void VisitObjectCreation_VueDictionaryIndexer_StaticNullLiteral_IsOmitted()
    {
        var block = GetBlockOperation(@"
            using static ECMAScript.Vue3;

            class TestClass
            {
                void TestMethod()
                {
                    var attrs = new VueDictionary
                    {
                        [""title""] = ""hello"",
                        [""skip""] = null
                    };
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{ title: ""hello"" }", script);
    }

    [TestMethod]
    public void VisitObjectCreation_RecordSpreadProperty_FlattensNestedRecord()
    {
        var block = GetBlockOperation(@"
            using System.ComponentModel;
            using ECMAScript;

            class TestClass
            {
                void TestMethod()
                {
                    var obj = new Wrapper
                    {
                        Prefix = ""x"",
                        Child = new ChildProps
                        {
                            Name = ""John"",
                            Age = 30
                        }
                    };
                }

                public sealed record ChildProps
                {
                    [Description(""@#name"")]
                    public string? Name { get; init; }

                    [Description(""@#age"")]
                    public int Age { get; init; }
                }

                public sealed record Wrapper
                {
                    [Description(""@#prefix"")]
                    public string? Prefix { get; init; }

                    [Spread]
                    public ChildProps? Child { get; init; }
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"{
  prefix: ""x"",
  name: ""John"",
  age: 30
}", script);
    }

    [TestMethod]
    public void VisitObjectCreation_RecordPrimaryConstructorSpreadProperty_NonLiteralValue_UsesSpreadElement()
    {
        var block = GetBlockOperation(@"
            using System.ComponentModel;
            using ECMAScript;

            class TestClass
            {
                void TestMethod()
                {
                    var child = new ChildProps
                    {
                        Name = ""John"",
                        Age = 30
                    };
                    var obj = new Wrapper(""x"", child);
                }

                public sealed record ChildProps
                {
                    [Description(""@#name"")]
                    public string? Name { get; init; }

                    [Description(""@#age"")]
                    public int Age { get; init; }
                }

                public sealed record Wrapper(
                    [property: Description(""@#prefix"")] string? Prefix,
                    [property: Spread] ChildProps? Child);
            }
            ");

        var operation = GetObjectCreationOperationAt(block, 1);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{ prefix: ""x"", ...child }", script);
    }

    [TestMethod]
    public void VisitObjectCreation_RecordObjectInitializerSpreadProperty_NonLiteralValue_UsesSpreadElement()
    {
        var block = GetBlockOperation(@"
            using System.ComponentModel;
            using ECMAScript;

            class TestClass
            {
                void TestMethod()
                {
                    var child = new ChildProps
                    {
                        Name = ""John"",
                        Age = 30
                    };
                    var obj = new Wrapper
                    {
                        Prefix = ""x"",
                        Child = child
                    };
                }

                public sealed record ChildProps
                {
                    [Description(""@#name"")]
                    public string? Name { get; init; }

                    [Description(""@#age"")]
                    public int Age { get; init; }
                }

                public sealed record Wrapper
                {
                    [Description(""@#prefix"")]
                    public string? Prefix { get; init; }

                    [Spread]
                    public ChildProps? Child { get; init; }
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block, 1);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{ prefix: ""x"", ...child }", script);
    }

    [TestMethod]
    public void VisitObjectCreation_RecordSpecialPropertyNames_AreQuotedWhenRequired()
    {
        var block = GetBlockOperation(@"
            using System.ComponentModel;

            class TestClass
            {
                void TestMethod()
                {
                    var obj = new SpecialProps
                    {
                        Selector = ""some-name"",
                        DataUserId = ""42"",
                        Width = ""100"",
                        Class = ""foo""
                    };
                }

                public sealed record SpecialProps
                {
                    [Description(""@#.name"")]
                    public string? Selector { get; init; }

                    [Description(""@#data-user-id"")]
                    public string? DataUserId { get; init; }

                    [Description(""@#^width"")]
                    public string? Width { get; init; }

                    [Description(""@#class"")]
                    public string? Class { get; init; }
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"{
  "".name"": ""some-name"",
  ""data-user-id"": ""42"",
  ""^width"": ""100"",
  class: ""foo""
}", script);
    }

    [TestMethod]
    public void VisitObjectCreation_RecordObjectInitializer_SymbolIndexer_UsesComputedProperty()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var countKey = SymbolFn(""count"");
                    var obj = new Bag
                    {
                        [countKey] = 1
                    };
                }

                public sealed record Bag
                {
                    public int this[Symbol key]
                    {
                        get => 0;
                        set { }
                    }
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block, 1);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{ [countKey]: 1 }", script);
    }

    [TestMethod]
    public void VisitObjectCreation_RecordCollectionInitializer_SymbolAdd_UsesComputedProperty()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var countKey = SymbolFn(""count"");
                    var obj = new Bag
                    {
                        { countKey, 1 }
                    };
                }

                public sealed record Bag : System.Collections.IEnumerable
                {
                    public void Add(Symbol key, int value)
                    {
                    }

                    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                        => throw new NotImplementedException();
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block, 1);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{ [countKey]: 1 }", script);
    }

    [TestMethod]
    public void VisitObjectCreation_ObjectLiteralAddOnVuePropsCarrier_StaticNullLiteral_IsPreserved()
    {
        var block = GetBlockOperation(@"
            using ECMAScript;

            class TestClass
            {
                void TestMethod()
                {
                    var query = new LocationQueryRaw
                    {
                        { ""empty"", null },
                        { ""page"", (Number)1 }
                    };
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{ empty: null, page: 1 }", script);
    }

    [TestMethod]
    public void VisitObjectCreation_ObjectLiteralAddOnVuePropsCarrier_StaticUndefinedLiteral_IsPreserved()
    {
        var block = GetBlockOperation(@"
            using ECMAScript;
            using static ECMAScript.Global;

            class TestClass
            {
                void TestMethod()
                {
                    var query = new LocationQueryRaw
                    {
                        { ""drop"", Undefined<LocationQueryValueRaw?>() },
                        { ""page"", (Number)1 }
                    };
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{ drop: undefined, page: 1 }", script);
    }

    [TestMethod]
    public void VisitObjectCreation_ObjectLiteralAddOnVuePropsCarrier_NumericKey_UsesNumericProperty()
    {
        var block = GetBlockOperation(@"
            using ECMAScript;

            class TestClass
            {
                void TestMethod()
                {
                    var state = new HistoryState
                    {
                        { (Number)7, (Number)9 }
                    };
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{ 7: 9 }", script);
    }

    [TestMethod]
    public void VisitObjectCreation_VueDictionaryAdd_StaticNullLiteral_IsOmitted()
    {
        var block = GetBlockOperation(@"
            using static ECMAScript.Vue3;

            class TestClass
            {
                void TestMethod()
                {
                    var attrs = new VueDictionary
                    {
                        { ""title"", ""hello"" },
                        { ""skip"", null }
                    };
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{ title: ""hello"" }", script);
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

        var node = walker.VisitArrayCreation(operation, new());
        var script = node?.ToECMAScript();

        AssertScriptEqual("new Array(5).fill().map(()=>new Array(5))", script);
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

        AssertScriptEqual(@"[""Hello"",""World""]", script);
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

        AssertScriptEqual(@"[42,""test"",true]", script);
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

        AssertScriptEqual("[[1,2],[3,4]]", script);
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

        AssertScriptEqual("this.MyMethod.bind(this)", script);
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

        AssertScriptEqual("x=>{return x*2}", script);
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

        AssertScriptEqual(@"v$0.push(1), v$0.push(2), v$0.push(3)", script);
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

        AssertScriptEqual(@"v$0.push([1]), v$0.push([2, 4]), v$0.push([3])".ReplaceLineEndings(), script?.ReplaceLineEndings());

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

        AssertScriptEqual(
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

        AssertScriptEqual("[42]", script);
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

        AssertScriptEqual("{Flag:true,Count:100,Price:19.99}", script);
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
        AssertScriptEqual("BigInt(42)", script);
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
        AssertScriptEqual(
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
        AssertScriptEqual("this.MyMethod.bind(this)", script);
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

        AssertScriptEqual("x=>{return x*2}", script);
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
        AssertScriptEqual(
@"{
  let list = [];
  list.push((() => {
    let v$0 = new Outer;
    v$0.Inner = (() => {
      let v$1 = new Inner;
      v$1.Value = 42;
      return v$1;
    })();
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
        AssertScriptEqual("new Array(3)", script);
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

        AssertScriptEqual(@"new MyClass(42,""hello"",""world"")", script);
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

        AssertScriptEqual(@"{Level1:{Level2:{Level3:""deep""}}}", script);
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

        AssertScriptEqual(@"[""hello"",null,""world""]", script);
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
        AssertScriptEqual(@"`The value is {value}: ${value}`", script);
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
        AssertScriptEqual(
@"(() => {
  let v$0 = new A;
  v$0.B = (() => {
    let v$0 = new B;
    v$0.C = (() => {
      let v$0 = new C;
      v$0.D = (() => {
        let v$0 = new D;
        v$0.Value = 999;
        return v$0;
      })();
      return v$0;
    })();
    return v$0;
  })();
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

        AssertScriptEqual("_4cb33a818161a3e1(2024,1,1)", script);
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

        AssertScriptEqual("_8c5a25d777626c6c(2024,1,1)", script);
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

        AssertScriptEqual("_e9a3481b3456aad4(12,30,0)", script);
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

        AssertScriptEqual("_6f22e268aec62fe7(1,2,3)", script);
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
        AssertScriptEqual("[]", script);
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
        AssertScriptEqual("new Map", script);
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
        AssertScriptEqual("new Set", script);
    }

    [TestMethod]
    public void VisitObjectCreation_DictionaryWithRecordKey_Throws()
    {
        var block = GetBlockOperation(@"
            record Key(int Id);

            class TestClass
            {
                void TestMethod()
                {
                    var dict = new System.Collections.Generic.Dictionary<Key, string>();
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);

        var exception = Assert.Throws<OperationTransformationException>(() =>
        {
            _ = walker.VisitObjectCreation(operation, new());
        });

        StringAssert.Contains(exception.Message, "JS-stable default equality");
        StringAssert.Contains(exception.Message, "System.Collections.Generic.Dictionary<Key, string>");
        StringAssert.Contains(exception.Message, "Key");
    }

    [TestMethod]
    public void VisitObjectCreation_DictionaryWithPlainReferenceIdentityKey_Allows()
    {
        var block = GetBlockOperation(@"
            class Key
            {
                public int Id { get; set; }
            }

            class TestClass
            {
                void TestMethod()
                {
                    var dict = new System.Collections.Generic.Dictionary<Key, string>();
                }
            }
            ");

        var operation = GetObjectCreationOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitObjectCreation(operation, new());
        var script = node?.ToECMAScript();

        AssertScriptEqual("new Map", script);
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
        AssertScriptEqual(
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
        AssertScriptEqual(
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

        AssertScriptEqual(
@"{
  let list = [];
  list.push((new Outer).Value);
}", script);

    }

    [TestMethod]
    public void Visit_ObjectCreation_WithAsyncObjectInitializer_UsesAsyncIife()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                async System.Threading.Tasks.Task TestMethod()
                {
                    var obj = new MyClass { Value = await System.Threading.Tasks.Task.FromResult(42) };
                }

                class MyClass
                {
                    public int Value { get; set; }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let obj = (async () => {", StringComparison.Ordinal);
        StringAssert.Contains(script, "await Promise.resolve(42);", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ObjectCreation_WithAsyncLambdaInitializer_DoesNotPromoteOuterIifeToAsync()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new MyClass
                    {
                        Callback = async () => await System.Threading.Tasks.Task.CompletedTask
                    };
                }

                class MyClass
                {
                    public System.Func<System.Threading.Tasks.Task>? Callback { get; set; }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let obj = (() => {", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("let obj = (async () => {", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "v$0.callback = async () => {", StringComparison.Ordinal);
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

        // 大小为 0 的数组会压缩为空数组字面量
        AssertScriptEqual("[]", script);
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

        AssertScriptEqual("new MyClass(x+y,x*y)", script);
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

        AssertScriptEqual("_d90dce0e1d2f06e4(2024,1,1,0,0,0,_e5548fcde33957a6())", script);
    }

    #region 扩展测试用例 - 数组创建变体

    /// <summary>
    /// 测试隐式类型数组
    /// </summary>
    [TestMethod]
    public void Visit_ArrayCreation_ImplicitlyTyped()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new[] { 1, 2, 3, 4, 5 };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let arr = [1, 2, 3, 4, 5];
}", script);
    }

    /// <summary>
    /// 测试字符串数组创建
    /// </summary>
    [TestMethod]
    public void Visit_ArrayCreation_StringArray()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new[] { ""a"", ""b"", ""c"" };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let arr = [""a"", ""b"", ""c""];
}", script);
    }

    /// <summary>
    /// 测试多维数组创建
    /// </summary>
    [TestMethod]
    public void Visit_ArrayCreation_Multidimensional()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var matrix = new int[2, 3];
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let matrix = new Array(2).fill().map(() => new Array(3));
}", script);
    }

    /// <summary>
    /// 测试交错数组创建
    /// </summary>
    [TestMethod]
    public void Visit_ArrayCreation_Jagged()
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

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let jagged = new Array(3);
}", script);
    }

    /// <summary>
    /// 测试带初始化的多维数组
    /// </summary>
    [TestMethod]
    public void Visit_ArrayCreation_MultidimensionalWithInit()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var matrix = new int[,] { { 1, 2, 3 }, { 4, 5, 6 } };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let matrix = [[1, 2, 3], [4, 5, 6]];
}", script);
    }

    #endregion

    #region 扩展测试用例 - 对象创建变体

    /// <summary>
    /// 测试无参数对象创建
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_Parameterless()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new object();
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let obj = new Object;
}", script);
    }

    /// <summary>
    /// 测试带参数对象创建
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_WithParameters()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var exception = new System.Exception(""Error message"");
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let exception = new Error(""Error message"");
}", script);
    }

    [TestMethod]
    public void Visit_ObjectCreation_OmitsTrailingDefaultArguments()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new MyClass(42);
                }

                class MyClass
                {
                    public MyClass(int number, string text = ""test"") { }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let obj = new MyClass(42);
}", script);
    }

    /// <summary>
    /// 测试对象初始化器
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_WithInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new TestClass { Name = ""Test"", Value = 42 };
                }

                public string Name { get; set; }
                public int Value { get; set; }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let obj = (() => {
    let v$0 = new TestClass;
    v$0.Name = ""Test"";
    v$0.Value = 42;
    return v$0;
  })();
}", script);
    }

    /// <summary>
    /// 测试嵌套对象创建
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_Nested()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                class OuterClass
                {
                    public InnerClass Inner { get; set; }
                }

                class InnerClass
                {
                    public int Value { get; set; }
                }

                void TestMethod()
                {
                    var outer = new OuterClass { Inner = new InnerClass { Value = 100 } };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let outer = (() => {
    let v$0 = new OuterClass;
    v$0.Inner = (() => {
      let v$1 = new InnerClass;
      v$1.Value = 100;
      return v$1;
    })();
    return v$0;
  })();
}", script);
    }

    /// <summary>
    /// 测试匿名对象创建
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_Anonymous()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var anonymous = new { Name = ""Test"", Value = 42 };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let anonymous = { Name: ""Test"", Value: 42 };
}", script);
    }

    #endregion

    #region 扩展测试用例 - 集合初始化器

    /// <summary>
    /// 测试List初始化器
    /// </summary>
    [TestMethod]
    public void Visit_CollectionInitializer_List()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new System.Collections.Generic.List<int> { 1, 2, 3, 4, 5 };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let list = [1, 2, 3, 4, 5];
}", script);
    }

    [TestMethod]
    public void Visit_CollectionInitializer_ListTupleElements_RemapNames()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new System.Collections.Generic.List<(string first, int years)>
                    {
                        (name: ""John"", age: 30)
                    };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let list = [{ first: ""John"", years: 30 }];
}", script);
    }

    /// <summary>
    /// 测试Dictionary初始化器
    /// </summary>
    [TestMethod]
    public void Visit_CollectionInitializer_Dictionary()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dict = new System.Collections.Generic.Dictionary<string, int>
                    {
                        { ""one"", 1 },
                        { ""two"", 2 },
                        { ""three"", 3 }
                    };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let dict = new Map([[""one"", 1], [""two"", 2], [""three"", 3]]);
}", script);
    }

    [TestMethod]
    public void Visit_CollectionInitializer_DictionaryTupleValues_RemapNames()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dict = new System.Collections.Generic.Dictionary<string, (string first, int years)>
                    {
                        [""one""] = (name: ""John"", age: 30)
                    };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let dict = new Map([[""one"", { first: ""John"", years: 30 }]]);
}", script);
    }

    [TestMethod]
    public void Visit_ArrayCreation_TupleElements_RemapNames()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    (string first, int years)[] values = new (string first, int years)[]
                    {
                        (name: ""John"", age: 30)
                    };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let values = [{ first: ""John"", years: 30 }];
}", script);
    }

    /// <summary>
    /// 测试HashSet初始化器
    /// </summary>
    [TestMethod]
    public void Visit_CollectionInitializer_HashSet()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var set = new System.Collections.Generic.HashSet<int> { 1, 2, 3 };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let set = new Set([1, 2, 3]);
}", script);
    }

    #endregion

    #region 扩展测试用例 - 特殊类型创建

    /// <summary>
    /// 测试DateTime创建
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_DateTime()
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

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();
        
        AssertScriptEqual(@"{
  let date = _4cb33a818161a3e1(2024, 1, 1);
}", script);
    }

    /// <summary>
    /// 测试BigInteger创建
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_BigInteger()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var big = new System.Numerics.BigInteger(12345);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let big = BigInt(12345);
}", script);
    }

    /// <summary>
    /// 测试StringBuilder创建
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_StringBuilder()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let sb = [];
}", script);
    }

    [TestMethod]
    public void Visit_ObjectCreation_StringBuilder_WithString_UsesInlineSplit()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(string text)
                {
                    var sb = new System.Text.StringBuilder(text);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let sb = (text ?? '').split('');
}", script);
    }

    /// <summary>
    /// 测试 new Guid() 走运行时 helper，而不是直接内联到 crypto.randomUUID()
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_Guid()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var guid = new System.Guid();
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let guid = _0e58e51018e846d2();
}", script);
    }

    /// <summary>
    /// 测试 Guid.NewGuid() 直接内联到浏览器运行时 API
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_GuidNewGuid()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var guid = System.Guid.NewGuid();
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let guid = globalThis.crypto.randomUUID();
}", script);
    }

    #region 扩展测试用例 - 更多对象创建

    /// <summary>
    /// 测试对象创建 - 带参数构造函数
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_WithArgs()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                class Point
                {
                    public int X { get; }
                    public int Y { get; }
                    public Point(int x, int y) { X = x; Y = y; }
                }

                void TestMethod()
                {
                    var point = new Point(10, 20);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let point = new Point(10, 20);
}", script);
    }

    [TestMethod]
    public void Visit_ObjectCreation_Record_LowersStructurally()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                record Point(int X, int Y);

                void TestMethod()
                {
                    var point = new Point(10, 20);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let point = { x: 10, y: 20 };
}", script);
    }

    /// <summary>
    /// 测试对象创建 - 链式构造
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_Chained()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                class OuterClass { public InnerClass Inner { get; set; } }
                class InnerClass { public int Value { get; set; } }

                void TestMethod()
                {
                    var obj = new OuterClass { Inner = new InnerClass { Value = 42 } };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let obj = (() => {
    let v$0 = new OuterClass;
    v$0.Inner = (() => {
      let v$1 = new InnerClass;
      v$1.Value = 42;
      return v$1;
    })();
    return v$0;
  })();
}", script);
    }

    /// <summary>
    /// 测试对象创建 - DateTime
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_DateTime1()
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

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();
        AssertScriptEqual(@"{
  let date = _4cb33a818161a3e1(2024, 1, 1);
}", script);
    }

    /// <summary>
    /// 测试对象创建 - TimeSpan
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_TimeSpan()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var duration = new System.TimeSpan(1, 2, 3);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();
        AssertScriptEqual(@"{
  let duration = _6f22e268aec62fe7(1, 2, 3);
}", script);
    }

    /// <summary>
    /// 测试对象创建 - 数组初始化器简写
    /// </summary>
    [TestMethod]
    public void Visit_ArrayCreation_Shorthand()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] arr = { 1, 2, 3 };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let arr = [1, 2, 3];
}", script);
    }

    /// <summary>
    /// 测试对象创建 - 空数组
    /// </summary>
    [TestMethod]
    public void Visit_ArrayCreation_Empty()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] empty = new int[0];
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();
        AssertScriptEqual(@"{
  let empty = [];
}", script);
    }

    /// <summary>
    /// 测试对象创建 - 匿名对象
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_Anonymous1()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Name = ""Test"", Value = 42 };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let obj = { Name: ""Test"", Value: 42 };
}", script);
    }

    /// <summary>
    /// 测试对象创建 - 嵌套匿名对象
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_NestedAnonymous()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Outer = new { Inner = 1 } };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let obj = { Outer: { Inner: 1 } };
}", script);
    }

    /// <summary>
    /// 测试对象创建 - 集合初始化器
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_CollectionInitializer()
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

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let list = [1, 2, 3];
}", script);
    }

    /// <summary>
    /// 测试对象创建 - 字典初始化器
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_DictionaryInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dict = new System.Collections.Generic.Dictionary<string, int>
                    {
                        { ""one"", 1 },
                        { ""two"", 2 }
                    };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();
        AssertScriptEqual(@"{
  let dict = new Map([[""one"", 1], [""two"", 2]]);
}", script);
    }

    /// <summary>
    /// 测试对象创建 - HashSet
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_HashSetWithValues()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var set = new System.Collections.Generic.HashSet<int> { 1, 2, 3 };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let set = new Set([1, 2, 3]);
}", script);
    }

    /// <summary>
    /// 测试对象创建 - 栈
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_Stack()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var stack = new System.Collections.Generic.Stack<int>();
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();
        AssertScriptEqual(@"{
  let stack = _7d15fcc03d17599b();
}", script);
    }

    /// <summary>
    /// 测试对象创建 - 队列
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_Queue()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var queue = new System.Collections.Generic.Queue<string>();
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();
        AssertScriptEqual(@"{
  let queue = _ea05a56d08fbd4f9();
}", script);
    }

    [TestMethod]
    public void Visit_ObjectCreation_Queue_WithCapacity()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var queue = new System.Collections.Generic.Queue<int>(4);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();
        AssertScriptEqual(@"{
  let queue = _7fc2b76467c43db9(4);
}", script);
    }

    [TestMethod]
    public void Visit_ObjectCreation_Stack_FromEnumerable()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var stack = new System.Collections.Generic.Stack<int>(new[] { 1, 2, 3 });
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();
        AssertScriptEqual(@"{
  let stack = _60d564060ac5fb0f([1, 2, 3]);
}", script);
    }

    /// <summary>
    /// 测试对象创建 - 多个对象
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_Multiple()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var a = new object();
                    var b = new object();
                    var c = new object();
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let a = new Object;
  let b = new Object;
  let c = new Object;
}", script);
    }

    /// <summary>
    /// 测试对象创建 - 对象在表达式中
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_InExpression()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    bool result = new object() != null;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();
        AssertScriptEqual(@"{
  let result = new Object !== null;
}", script);
    }

    #endregion

    #region 扩展测试用例 - 更多集合创建

    /// <summary>
    /// 测试集合创建 - 列表带初始值
    /// </summary>
    [TestMethod]
    public void Visit_CollectionCreation_ListWithValues()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new System.Collections.Generic.List<int> { 1, 2, 3, 4, 5 };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let list = [1, 2, 3, 4, 5];
}", script);
    }

    /// <summary>
    /// 测试集合创建 - 字典带初始值
    /// </summary>
    [TestMethod]
    public void Visit_CollectionCreation_DictionaryWithValues()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dict = new System.Collections.Generic.Dictionary<string, int>
                    {
                        [""one""] = 1,
                        [""two""] = 2
                    };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();
        AssertScriptEqual(@"{
  let dict = new Map([[""one"", 1], [""two"", 2]]);
}", script);
    }

    /// <summary>
    /// 测试集合创建 - HashSet带初始值
    /// </summary>
    [TestMethod]
    public void Visit_CollectionCreation_HashSetWithValues()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var set = new System.Collections.Generic.HashSet<string> { ""a"", ""b"", ""c"" };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let set = new Set([""a"", ""b"", ""c""]);
}", script);
    }

    /// <summary>
    /// 测试数组创建 - 隐式类型
    /// </summary>
    [TestMethod]
    public void Visit_ArrayCreation_ImplicitlyTyped1()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new[] { 1, 2, 3 };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let arr = [1, 2, 3];
}", script);
    }

    /// <summary>
    /// 测试数组创建 - 字符串数组
    /// </summary>
    [TestMethod]
    public void Visit_ArrayCreation_StringArray1()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new[] { ""a"", ""b"", ""c"" };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let arr = [""a"", ""b"", ""c""];
}", script);
    }

    /// <summary>
    /// 测试数组创建 - 双精度数组
    /// </summary>
    [TestMethod]
    public void Visit_ArrayCreation_DoubleArray()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new[] { 1.1, 2.2, 3.3 };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let arr = [1.1, 2.2, 3.3];
}", script);
    }

    #endregion

    #region 扩展测试用例 - 更多对象初始化器

    /// <summary>
    /// 测试对象初始化器 - 多属性
    /// </summary>
    [TestMethod]
    public void Visit_ObjectInitializer_MultipleProperties()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                class Person
                {
                    public string Name { get; set; }
                    public int Age { get; set; }
                }

                void TestMethod()
                {
                    var person = new Person { Name = ""John"", Age = 30 };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();
        AssertScriptEqual(@"{
  let person = (() => {
    let v$0 = new Person;
    v$0.Name = ""John"";
    v$0.Age = 30;
    return v$0;
  })();
}", script);
    }

    /// <summary>
    /// 测试对象初始化器 - 嵌套
    /// </summary>
    [TestMethod]
    public void Visit_ObjectInitializer_Nested()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                class Outer { public Inner Inner { get; set; } }
                class Inner { public int Value { get; set; } }

                void TestMethod()
                {
                    var outer = new Outer { Inner = new Inner { Value = 42 } };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();
        AssertScriptEqual(@"{
  let outer = (() => {
    let v$0 = new Outer;
    v$0.Inner = (() => {
      let v$1 = new Inner;
      v$1.Value = 42;
      return v$1;
    })();
    return v$0;
  })();
}", script);
    }

    [TestMethod]
    public void Visit_ObjectInitializer_WithGeneratedTemporary_KeepsTemporaryInsideIifeBody()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                class Box
                {
                    public int Value { get; set; }
                }

                void TestMethod()
                {
                    var obj = new Box { Value = GetTuple() == (1, 2) ? 1 : 0 };
                }

                (int, int) GetTuple() => (1, 2);
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let obj = (() => {");
        StringAssert.Contains(script, "let v$1;");
        StringAssert.Contains(script, "let v$0 = new Box;");
        StringAssert.Contains(script, "v$1 = this.getTuple()");
        Assert.IsFalse(
            script.Contains("{\r\n  let v$1;\r\n  let obj = (() => {", StringComparison.Ordinal)
            || script.Contains("{\n  let v$1;\n  let obj = (() => {", StringComparison.Ordinal),
            $"Expected the generated tuple cache to stay inside the object-initializer IIFE.{Environment.NewLine}{script}");

        var iifeIndex = script.IndexOf("let obj = (() => {", StringComparison.Ordinal);
        var innerTempIndex = script.IndexOf("let v$1;", StringComparison.Ordinal);
        var objectTempIndex = script.IndexOf("let v$0 = new Box;", StringComparison.Ordinal);
        Assert.IsTrue(
            iifeIndex >= 0 && innerTempIndex > iifeIndex && objectTempIndex > innerTempIndex,
            $"Expected the generated tuple cache declaration to live inside the IIFE body before object initialization.{Environment.NewLine}{script}");
    }

    #endregion

    #region 扩展测试用例 - 特殊类型创建

    /// <summary>
    /// 测试 DateOnly 创建
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_DateOnly()
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

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();
        AssertScriptEqual(@"{
  let date = _8c5a25d777626c6c(2024, 1, 1);
}", script);
    }

    /// <summary>
    /// 测试 TimeOnly 创建
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_TimeOnly()
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

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertScriptEqual(
@"{
  let time = _e9a3481b3456aad4(12, 30, 0);
}", script);
    }

    /// <summary>
    /// 测试 DateTimeOffset 创建
    /// </summary>
    [TestMethod]
    public void Visit_ObjectCreation_DateTimeOffset()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dto = new System.DateTimeOffset(2024, 1, 1, 12, 0, 0, System.TimeSpan.Zero);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();
        AssertScriptEqual(@"{
  let dto = _d90dce0e1d2f06e4(2024, 1, 1, 12, 0, 0, _e5548fcde33957a6());
}", script);
    }

    #endregion
}
#endregion
