using Acornima.Ast;
using ECMAScript.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScript.ComplierTest;

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
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

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
        Assert.IsInstanceOfType(result, typeof(Module));
        Assert.IsGreaterThan(0, result.Body.Count);
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
        Assert.IsGreaterThanOrEqualTo(2, exportDeclarations.Count); // 至少包含 getter 和 setter
        
        var functionDeclarations = exportDeclarations
            .Select(ed => ed.Declaration)
            .OfType<FunctionDeclaration>()
            .ToList();
        Assert.IsGreaterThanOrEqualTo(2, functionDeclarations.Count);
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
}