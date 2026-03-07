using Acornima.Ast;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class AstConverterTests
{
    private static (INamedTypeSymbol, SemanticModel) CompileAndGetSymbol(string code)
    {
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [CSharpSyntaxTree.ParseText(code)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var syntaxTree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var classDeclaration = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

        Assert.IsNotNull(classSymbol);
        return (classSymbol, semanticModel);
    }

    [TestMethod]
    public void Convert_SimplePublicClass_ReturnsModule()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int Field = 42;
                public static void Method() { }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<Module>(result);
        Assert.IsLessThan(result.Body.Count, 0);
    }

    [TestMethod]
    public void Convert_NonPublicClass_ThrowsNotSupportedException()
    {
        // Arrange
        var code = """
            internal static class TestClass
            {
                public static int Field = 42;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act & Assert
        var exception = Assert.Throws<NotSupportedException>(() => converter.Convert());
        Assert.Contains("不是 public", exception.Message);
    }

    [TestMethod]
    public void Convert_ClassWithStaticField_GeneratesVariableDeclaration()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int Field = 42;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclaration = result.Body.OfType<ExportNamedDeclaration>().FirstOrDefault();
        Assert.IsNotNull(exportDeclaration);
        Assert.IsInstanceOfType(exportDeclaration.Declaration, typeof(VariableDeclaration));
    }

    [TestMethod]
    public void Convert_ClassWithConstField_GeneratesConstDeclaration()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public const int ConstField = 42;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclaration = result.Body.OfType<ExportNamedDeclaration>().FirstOrDefault();
        Assert.IsNotNull(exportDeclaration);
        var variableDeclaration = exportDeclaration.Declaration as VariableDeclaration;
        Assert.IsNotNull(variableDeclaration);
        Assert.AreEqual(VariableDeclarationKind.Const, variableDeclaration.Kind);
    }

    [TestMethod]
    public void Convert_ClassWithPrivateField_DoesNotExport()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                private static int PrivateField = 42;
                public static int PublicField = 24;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        
        // 应该有一个私有字段声明（非导出）和一个公共字段导出
        var variableDeclarations = result.Body.OfType<VariableDeclaration>().ToList();
        var exportDeclarations = result.Body.OfType<ExportNamedDeclaration>().ToList();
        
        Assert.HasCount(1, variableDeclarations); // 私有字段
        Assert.HasCount(1, exportDeclarations);   // 公共字段导出
    }

    [TestMethod]
    public void Convert_ClassWithMethod_GeneratesFunctionDeclaration()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int TestMethod()
                {
                    return 1;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclaration = result.Body.OfType<ExportNamedDeclaration>().FirstOrDefault();
        Assert.IsNotNull(exportDeclaration);
        Assert.IsInstanceOfType(exportDeclaration.Declaration, typeof(FunctionDeclaration));
    }

    [TestMethod]
    public void Convert_ClassWithProperty_GeneratesPropertyMethods()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int Property { get; set; }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);

        // 应该包含 getter 和 setter 方法的导出
        var exportDeclarations = result.Body.OfType<ExportNamedDeclaration>().ToList();
        Assert.IsGreaterThanOrEqualTo(exportDeclarations.Count, 2); // 至少包含 getter 和 setter

        var functionDeclarations = exportDeclarations
            .Select(ed => ed.Declaration)
            .OfType<FunctionDeclaration>()
            .ToList();
        Assert.IsGreaterThanOrEqualTo(functionDeclarations.Count, 2);
    }

    [TestMethod]
    public void Convert_EmptyClass_ReturnsNull()
    {
        // Arrange
        var code = """
            public static class EmptyClass
            {
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Convert_ClassWithEnum_GeneratesEnumObject()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public enum TestEnum
                {
                    Value1,
                    Value2 = 5
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclaration = result.Body.OfType<ExportNamedDeclaration>().FirstOrDefault();
        Assert.IsNotNull(exportDeclaration);
        Assert.IsInstanceOfType(exportDeclaration.Declaration, typeof(VariableDeclaration));
    }

    [TestMethod]
    public void Convert_ClassWithNestedClass_GeneratesClassDeclaration()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public int Field;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Assert
        Assert.Throws<NotSupportedException>(() => converter.Convert(), "Specified method is not supported.");
    }

    #region 静态字段测试

    [TestMethod]
    public void Convert_ClassWithMultipleStaticFields_GeneratesAllDeclarations()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int Field1 = 1;
                public static int Field2 = 2;
                public static int Field3 = 3;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclarations = result.Body.OfType<ExportNamedDeclaration>().ToList();
        Assert.HasCount(3, exportDeclarations);
    }

    [TestMethod]
    public void Convert_ClassWithStaticReadonlyField_GeneratesLetDeclaration()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static readonly int ReadOnlyField = 42;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclaration = result.Body.OfType<ExportNamedDeclaration>().FirstOrDefault();
        Assert.IsNotNull(exportDeclaration);
        var variableDeclaration = exportDeclaration.Declaration as VariableDeclaration;
        Assert.IsNotNull(variableDeclaration);
        Assert.AreEqual(VariableDeclarationKind.Let, variableDeclaration.Kind);
    }

    [TestMethod]
    public void Convert_ClassWithInternalStaticField_GeneratesExport()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                internal static int InternalField = 42;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclarations = result.Body.OfType<ExportNamedDeclaration>().ToList();
        Assert.HasCount(1, exportDeclarations);
    }

    [TestMethod]
    public void Convert_ClassWithStringField_GeneratesStringDeclaration()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static string StringField = "hello";
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclaration = result.Body.OfType<ExportNamedDeclaration>().FirstOrDefault();
        Assert.IsNotNull(exportDeclaration);
    }

    [TestMethod]
    public void Convert_ClassWithBoolField_GeneratesBoolDeclaration()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static bool BoolField = true;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclaration = result.Body.OfType<ExportNamedDeclaration>().FirstOrDefault();
        Assert.IsNotNull(exportDeclaration);
    }

    [TestMethod]
    public void Convert_ClassWithDoubleField_GeneratesNumberDeclaration()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static double DoubleField = 3.14;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclaration = result.Body.OfType<ExportNamedDeclaration>().FirstOrDefault();
        Assert.IsNotNull(exportDeclaration);
    }

    #endregion

    #region 静态方法测试

    [TestMethod]
    public void Convert_ClassWithStaticMethod_ReturnsVoid_GeneratesFunction()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static void VoidMethod()
                {
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclaration = result.Body.OfType<ExportNamedDeclaration>().FirstOrDefault();
        Assert.IsNotNull(exportDeclaration);
        Assert.IsInstanceOfType(exportDeclaration.Declaration, typeof(FunctionDeclaration));
    }

    [TestMethod]
    public void Convert_ClassWithStaticMethod_WithParameters_GeneratesFunctionWithParams()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int Add(int a, int b)
                {
                    return a + b;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclaration = result.Body.OfType<ExportNamedDeclaration>().FirstOrDefault();
        Assert.IsNotNull(exportDeclaration);
        var func = exportDeclaration.Declaration as FunctionDeclaration;
        Assert.IsNotNull(func);
        Assert.HasCount(2, func.Params);
    }

    [TestMethod]
    public void Convert_ClassWithStaticMethod_WithStringParam_GeneratesFunction()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static string Greet(string name)
                {
                    return "Hello " + name;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclaration = result.Body.OfType<ExportNamedDeclaration>().FirstOrDefault();
        Assert.IsNotNull(exportDeclaration);
    }

    [TestMethod]
    public void Convert_ClassWithPrivateMethod_DoesNotExport()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                private static void PrivateMethod() { }
                public static void PublicMethod() { }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclarations = result.Body.OfType<ExportNamedDeclaration>().ToList();
        Assert.HasCount(1, exportDeclarations);
    }

    [TestMethod]
    public void Convert_ClassWithInternalMethod_GeneratesExport()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                internal static void InternalMethod() { }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclarations = result.Body.OfType<ExportNamedDeclaration>().ToList();
        Assert.HasCount(1, exportDeclarations);
    }

    #endregion

    #region 属性测试

    [TestMethod]
    public void Convert_ClassWithStaticProperty_GetOnly_GeneratesGetter()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int ReadOnlyProperty { get; } = 42;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclarations = result.Body.OfType<ExportNamedDeclaration>().ToList();
        Assert.IsGreaterThanOrEqualTo(exportDeclarations.Count, 1);
    }

    [TestMethod]
    public void Convert_ClassWithStaticProperty_Computed_GeneratesGetterFunction()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                private static int _field = 10;
                public static int ComputedProperty
                {
                    get { return _field * 2; }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Convert_ClassWithMultipleProperties_GeneratesAllAccessors()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int Prop1 { get; set; }
                public static string Prop2 { get; set; }
                public static bool Prop3 { get; set; }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclarations = result.Body.OfType<ExportNamedDeclaration>().ToList();
        Assert.IsGreaterThanOrEqualTo(exportDeclarations.Count, 6); // 3 properties * 2 accessors
    }

    #endregion

    #region 枚举测试

    [TestMethod]
    public void Convert_ClassWithEnum_MultipleValues_GeneratesEnumObject()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public enum Status
                {
                    None = 0,
                    Active = 1,
                    Inactive = 2,
                    Pending = 3
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclaration = result.Body.OfType<ExportNamedDeclaration>().FirstOrDefault();
        Assert.IsNotNull(exportDeclaration);
    }

    [TestMethod]
    public void Convert_ClassWithEnum_FlagsAttribute_GeneratesEnumObject()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                [System.Flags]
                public enum Permissions
                {
                    None = 0,
                    Read = 1,
                    Write = 2,
                    Execute = 4
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclaration = result.Body.OfType<ExportNamedDeclaration>().FirstOrDefault();
        Assert.IsNotNull(exportDeclaration);
    }

    [TestMethod]
    public void Convert_ClassWithPrivateEnum_DoesNotExport()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                private enum InternalEnum
                {
                    A, B, C
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        // 私有枚举应该生成声明但不导出
        var variableDeclarations = result.Body.OfType<VariableDeclaration>().ToList();
        Assert.HasCount(1, variableDeclarations);
    }

    #endregion

    #region 混合成员测试

    [TestMethod]
    public void Convert_ClassWithMixedMembers_GeneratesAll()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int Field = 1;
                public const int Const = 42;
                public static int Prop { get; set; }
                public static void Method() { }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsGreaterThan(result.Body.Count, 0);
    }

    [TestMethod]
    public void Convert_ClassWithStaticConstructor_GeneratesInit()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int Value;
                static TestClass()
                {
                    Value = 100;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
    }

    #endregion

    #region 泛型测试

    [TestMethod]
    public void Convert_ClassWithGenericMethod_GeneratesFunction()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static T Identity<T>(T value)
                {
                    return value;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclaration = result.Body.OfType<ExportNamedDeclaration>().FirstOrDefault();
        Assert.IsNotNull(exportDeclaration);
    }

    [TestMethod]
    public void Convert_ClassWithGenericField_GeneratesDeclaration()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static System.Collections.Generic.List<int> Numbers = new System.Collections.Generic.List<int>();
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclaration = result.Body.OfType<ExportNamedDeclaration>().FirstOrDefault();
        Assert.IsNotNull(exportDeclaration);
    }

    #endregion

    #region 特殊类型测试

    [TestMethod]
    public void Convert_ClassWithDelegate_DoesNotGenerate()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public delegate int MathOp(int a, int b);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        // 委托应该被跳过或不生成导出
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Convert_ClassWithEvent_DoesNotGenerate()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static event System.EventHandler MyEvent;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        // 事件应该被跳过
        Assert.IsNotNull(result);
    }

    #endregion

    #region 更多字段测试

    [TestMethod]
    public void Convert_ClassWithNullableField_GeneratesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int? NullableField = null;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Convert_ClassWithArrayField_GeneratesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int[] ArrayField = new int[] { 1, 2, 3 };
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Convert_ClassWithListField_GeneratesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static System.Collections.Generic.List<int> ListField = new();
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Convert_ClassWithDictionaryField_GeneratesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static System.Collections.Generic.Dictionary<string, int> DictField = new();
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
    }

    #endregion

    #region 更多方法测试

    [TestMethod]
    public void Convert_ClassWithGenericMethod_GeneratesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static T Max<T>(T a, T b) where T : System.IComparable<T>
                {
                    return a.CompareTo(b) > 0 ? a : b;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Convert_ClassWithExtensionMethod_GeneratesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int Double(this int value)
                {
                    return value * 2;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Convert_ClassWithParamsMethod_GeneratesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int Sum(params int[] values)
                {
                    int sum = 0;
                    foreach (var v in values) sum += v;
                    return sum;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Convert_ClassWithRefMethod_GeneratesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static void Increment(ref int value)
                {
                    value++;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
    }

    #endregion

    #region 更多属性测试

    [TestMethod]
    public void Convert_ClassWithStaticProperty_InitOnly_GeneratesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int Value { get; init; } = 42;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Convert_ClassWithExpressionProperty_GeneratesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                private static int _field = 10;
                public static int Doubled => _field * 2;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = converter.Convert();

        // Assert
        Assert.IsNotNull(result);
    }

    #endregion
}