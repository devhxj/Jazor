using System.Diagnostics;
using Acornima.Ast;
using Jazor.Compiler;
using Jazor.Name;
using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class AstConverterTests
{
    private static string ImportBindingName(string modulePath, string importedName)
        => $"i${Format.HashName($"{modulePath}\0{importedName}").TrimStart('_')}";

    private static (INamedTypeSymbol, SemanticModel) CompileAndGetSymbol(string code)
    {
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [CSharpSyntaxTree.ParseText(code)],
            Net100.References.All,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var syntaxTree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var classDeclaration = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

        Assert.IsNotNull(classSymbol);
        return (classSymbol, semanticModel);
    }

    [TestMethod]
    public async Task Convert_SimplePublicClass_ReturnsModule()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let Field = 42;
export function Method() { }
", script);
        
    }

    [TestMethod]
    public async Task Convert_NonPublicClass_ThrowsNotSupportedException()
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
        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);
        Assert.Contains("不是 public", exception.Message);
    }

    [TestMethod]
    public async Task Convert_ClassWithStaticField_GeneratesVariableDeclaration()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let Field = 42;
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithConstField_GeneratesConstDeclaration()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export const ConstField = 42;
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithPrivateField_DoesNotExport()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"let PrivateField = 42;
export let PublicField = 24;
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithMethod_GeneratesFunctionDeclaration()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export function TestMethod() {
  return 1;
}
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithProperty_GeneratesPropertyMethods()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"let _38ee328c86b9b067;
export function get_Property() {
  return _38ee328c86b9b067;
}
export function set_Property(value) {
  _38ee328c86b9b067 = value;
}
", script);

    }

    [TestMethod]
    public async Task Convert_EmptyClass_ReturnsEmpty()
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
        var result = await converter.Convert();

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task Convert_ClassWithEnum_GeneratesEnumObject()
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
        var result = await converter.Convert();

        // Assert
        Assert.IsNotNull(result);
        var exportDeclaration = result.Body.OfType<ExportNamedDeclaration>().FirstOrDefault();
        Assert.IsNotNull(exportDeclaration);
        Assert.IsInstanceOfType(exportDeclaration.Declaration, typeof(VariableDeclaration));
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClass_GeneratesClassDeclaration()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public int Field;

                    public NestedClass(int value)
                    {
                        Field = value;
                    }

                    public int Double()
                    {
                        return Field * 2;
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export class NestedClass {
  Field;
  constructor(value) {
    this.Field = value;
  }
  Double() {
    return this.Field * 2;
  }
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    #region 静态字段测试

    [TestMethod]
    public async Task Convert_ClassWithMultipleStaticFields_GeneratesAllDeclarations()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let Field1 = 1;
export let Field2 = 2;
export let Field3 = 3;
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithStaticReadonlyField_GeneratesLetDeclaration()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let ReadOnlyField = 42;
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithInternalStaticField_GeneratesExport()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let InternalField = 42;
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithStringField_GeneratesStringDeclaration()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let StringField = ""hello"";
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithBoolField_GeneratesBoolDeclaration()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let BoolField = true;
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithDoubleField_GeneratesNumberDeclaration()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let DoubleField = 3.14;
", script);

    }

    #endregion

    #region 静态方法测试

    [TestMethod]
    public async Task Convert_ClassWithStaticMethod_ReturnsVoid_GeneratesFunction()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export function VoidMethod() { }
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithStaticMethod_WithParameters_GeneratesFunctionWithParams()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export function Add(a, b) {
  return a + b;
}
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithStaticMethod_WithStringParam_GeneratesFunction()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export function Greet(name) {
  return ""Hello "" + name;
}
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithPrivateMethod_DoesNotExport()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"function PrivateMethod() { }
export function PublicMethod() { }
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithInternalMethod_GeneratesExport()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export function InternalMethod() { }
", script);

    }

    #endregion

    #region 属性测试

    [TestMethod]
    public async Task Convert_ClassWithStaticProperty_GetOnly_GeneratesGetter()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"let _3d9336660801cacd = 42;
export function get_ReadOnlyProperty() {
  return _3d9336660801cacd;
}
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithStaticProperty_Computed_GeneratesGetterFunction()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"let _field = 10;
export function get_ComputedProperty() {
  return _field * 2;
}
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithMultipleProperties_GeneratesAllAccessors()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"let _6f335f402aa64190;
export function get_Prop1() {
  return _6f335f402aa64190;
}
export function set_Prop1(value) {
  _6f335f402aa64190 = value;
}
let _5bc1888a0261866a;
export function get_Prop2() {
  return _5bc1888a0261866a;
}
export function set_Prop2(value) {
  _5bc1888a0261866a = value;
}
let _2695ecdb6d62ea86;
export function get_Prop3() {
  return _2695ecdb6d62ea86;
}
export function set_Prop3(value) {
  _2695ecdb6d62ea86 = value;
}
", script);

    }

    #endregion

    #region 枚举测试

    [TestMethod]
    public async Task Convert_ClassWithEnum_MultipleValues_GeneratesEnumObject()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export const Status = Object.freeze({
  None: 0,
  Active: 1,
  Inactive: 2,
  Pending: 3
});
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithEnum_FlagsAttribute_GeneratesEnumObject()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export const Permissions = Object.freeze({
  None: 0,
  Read: 1,
  Write: 2,
  Execute: 4
});
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithPrivateEnum_DoesNotExport()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"const InternalEnum = Object.freeze({
  A: 0,
  B: 1,
  C: 2
});
", script);

    }

    #endregion

    #region 混合成员测试

    [TestMethod]
    public async Task Convert_ClassWithMixedMembers_GeneratesAll()
    {
        // Arrange
        var code = """
            public static class TestClass
            {                
                public static int A = 1;
                public static string B = "456";
                public const int C = 42;
                public static int P1 { get; set; }
                public static int P2 { get; }
                public static int P3
                {
                    get { return P1; }
                    set { }
                }

                public static int P4 => P1;

                public static string P5
                {
                    get => B;
                    set => B = value;
                }

                public static string? P6
                {
                    get => field;
                    set => field = value;
                }

                public static string? P7
                {
                    get;
                    set => field = value?.Trim();
                }

                public static string P8
                {
                    get => B;
                    set => B = value.Trim();
                }

                public static void Method() { }

                public static void Method(int a) { }

                public static int Method(int a, int b) => a + b;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var result = await converter.Convert();
        var script = result?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let A = 1;
export let B = ""456"";
export const C = 42;
let _81c4b3c96dabee42;
export function get_P1() {
  return _81c4b3c96dabee42;
}
export function set_P1(value) {
  _81c4b3c96dabee42 = value;
}
let _f616cc6f43cd37b6;
export function get_P2() {
  return _f616cc6f43cd37b6;
}
export function get_P3() {
  return P1;
}
export function set_P3(value) { }
export function get_P4() {
  return P1;
}
export function get_P5() {
  return B;
}
export function set_P5(value) {
  B = value;
}
let _57556f0916b4200d;
export function get_P6() {
  return _57556f0916b4200d;
}
export function set_P6(value) {
  _57556f0916b4200d = value;
}
let _aa3181446f60dc6e;
export function get_P7() {
  return _aa3181446f60dc6e;
}
export function set_P7(value) {
  _aa3181446f60dc6e = value.trim();
}
export function get_P8() {
  return B;
}
export function set_P8(value) {
  B = value.trim();
}
export function Method_a604b94929b691c0() { }
export function Method_d389d2b826e42edb(a) { }
export function Method_04bbed0f7a07bb40(a, b) {
  return a + b;
}
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithStaticConstructor_GeneratesInit()
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

        // Assert
        await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);

    }

    #endregion

    #region 泛型测试

    [TestMethod]
    public async Task Convert_ClassWithGenericMethod_GeneratesFunction()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export function Identity(value) {
  return value;
}
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithGenericField_GeneratesDeclaration()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let Numbers = [];
", script);

    }

    #endregion

    #region 特殊类型测试

    [TestMethod]
    public async Task Convert_ClassWithDelegate_ReturnsNull()
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

        // Assert
        await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);

    }

    [TestMethod]
    public async Task Convert_ClassWithEvent_ThrowsNotSupportedException()
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

        // Act & Assert - 事件的 add/remove 方法不支持转换
        await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);
    }

    #endregion

    #region 更多字段测试

    [TestMethod]
    public async Task Convert_ClassWithNullableField_GeneratesCorrectly()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let NullableField = null;
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithArrayField_GeneratesCorrectly()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let ArrayField = [1, 2, 3];
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithListField_GeneratesCorrectly()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let ListField = [];
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithDictionaryField_GeneratesCorrectly()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let DictField = new Map;
", script);

    }

    #endregion

    #region 更多方法测试

    [TestMethod]
    public async Task Convert_ClassWithGenericMethod_GeneratesCorrectly()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export function Max(a, b) {
  return a.CompareTo(b) > 0 ? a : b;
}
".ReplaceLineEndings(), script?.ReplaceLineEndings());

    }

    [TestMethod]
    public async Task Convert_ClassWithExtensionMethod_GeneratesCorrectly()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        // todo:扩展函数还需要增加调用测试
        Assert.AreEqual(
@"export function Double(value) {
  return value * 2;
}
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithParamsMethod_GeneratesCorrectly()
    {
        // Arrange - params 参数作为数组参数处理
        var code = """
            public static class TestClass
            {
                public static int Sum(params int[] values)
                {
                    return values.Length;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        // todo:需要增加 params 参数方法调用测试
        Assert.AreEqual(
@"export function Sum(values) {
  return values.length;
}
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithRefMethod_GeneratesCorrectly()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export function Increment(value) {
  value++;
  return [value];
}
".ReplaceLineEndings(), script?.ReplaceLineEndings());

    }

    [TestMethod]
    public async Task Convert_ClassWithNonVoidRefMethod_GeneratesTupleStyleReturn()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int IncrementAndReturn(ref int value)
                {
                    value++;
                    return value + 10;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export function IncrementAndReturn(value) {
  value++;
  return [value + 10, value];
}
".ReplaceLineEndings(), script?.ReplaceLineEndings());

    }

    [TestMethod]
    public async Task Convert_ClassWithRefMethod_EarlyReturn_PreservesProtocolOnAllPaths()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static void Normalize(ref int value)
                {
                    if (value < 0)
                        return;

                    value++;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export function Normalize(value) {
  if (value < 0)
    return [value];
  value++;
  return [value];
}
".ReplaceLineEndings(), script?.ReplaceLineEndings());

    }

    #endregion

    #region 更多属性测试

    [TestMethod]
    public async Task Convert_ClassWithStaticProperty_InitOnly_GeneratesCorrectly()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"const _9d512e1fd4b4d93c = 42;
export function get_Value() {
  return _9d512e1fd4b4d93c;
}
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithExpressionProperty_GeneratesCorrectly()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"let _field = 10;
export function get_Doubled() {
  return _field * 2;
}
", script);

    }

    #endregion

    #region 边界值测试

    [TestMethod]
    public async Task Convert_ClassWithLongField_GeneratesBigIntLiteral()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static long LongField = 9223372036854775807L;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let LongField = 9223372036854775807n;
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithULongField_GeneratesBigIntLiteral()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static ulong ULongField = 18446744073709551615UL;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let ULongField = 18446744073709551615n;
".ReplaceLineEndings(), script?.ReplaceLineEndings());

    }

    [TestMethod]
    public async Task Convert_ClassWithDoubleMaxValue_GeneratesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static double MaxDouble = double.MaxValue;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let MaxDouble = Number.MAX_VALUE;
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithDecimalField_GeneratesNumberLiteral()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static decimal DecimalField = 123.456m;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let DecimalField = 123.456;
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithSpecialString_EscapesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static string SpecialString = "Hello\nWorld\t!";
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let SpecialString = ""Hello\nWorld\t!"";
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithEmptyString_GeneratesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static string EmptyString = "";
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let EmptyString = """";
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithUnicodeString_GeneratesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static string UnicodeString = "你好世界🌍";
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let UnicodeString = ""你好世界🌍"";
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithQuoteString_EscapesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static string QuoteString = "He said \"Hello\"";
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let QuoteString = ""He said \""Hello\"""";
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithCharField_GeneratesStringLiteral()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static char CharField = 'A';
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let CharField = ""A"";
", script);

    }

    #endregion

    #region 参数默认值测试

    [TestMethod]
    public async Task Convert_MethodWithDefaultParameter_GeneratesDefault()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int Add(int a, int b = 10)
                {
                    return a + b;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export function Add(a, b = 10) {
  return a + b;
}
", script);

    }

    [TestMethod]
    public async Task Convert_MethodWithMultipleDefaultParameters_GeneratesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static string Greet(string name = "World", int age = 0)
                {
                    return $"Hello {name}, age {age}";
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export function Greet(name = ""World"", age = 0) {
  return `Hello ${name}, age ${age}`;
}
", script);

    }

    [TestMethod]
    public async Task Convert_MethodWithNullableDefaultParameter_ThrowsNotSupportedException()
    {
        // Arrange - null 默认值当前不支持
        var code = """
            public static class TestClass
            {
                public static void Process(string? name = null)
                {
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act & Assert - null 默认值当前不支持
        await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);
    }

    #endregion

    #region 方法重载测试

    [TestMethod]
    public async Task Convert_ClassWithOverloadedMethods_GeneratesAllMethods()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static void DoWork() { }
                public static void DoWork(int value) { }
                public static void DoWork(string value) { }
                public static int DoWork(int a, int b) => a + b;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export function DoWork_7bf2b889f48863c7() { }
export function DoWork_6b6f7943743f9c5d(value) { }
export function DoWork_53280513e48ce038(value) { }
export function DoWork_90a9f2ec5e6402a1(a, b) {
  return a + b;
}
", script);

    }

    #endregion

    #region 表达式体方法测试

    [TestMethod]
    public async Task Convert_ClassWithExpressionBodyMethod_GeneratesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int Square(int x) => x * x;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export function Square(x) {
  return x * x;
}
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithVoidExpressionBodyMethod_GeneratesCorrectly()
    {
        // Arrange - 使用简单的表达式体方法
        var code = """
            public static class TestClass
            {
                private static int _counter = 0;
                public static void Increment() => _counter++;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"let _counter = 0;
export function Increment() {
  _counter++;
}
", script);

    }

    #endregion

    #region 复杂场景测试

    [TestMethod]
    public async Task Convert_ClassWithNestedGenerics_GeneratesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>> NestedGenerics = new();
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export let NestedGenerics = new Map;
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithComplexMethodBody_GeneratesCorrectly()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static int Fibonacci(int n)
                {
                    if (n <= 1) return n;
                    return Fibonacci(n - 1) + Fibonacci(n - 2);
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export function Fibonacci(n) {
  if (n <= 1)
    return n;
  return Fibonacci(n - 1) + Fibonacci(n - 2);
}
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithMultipleEnumValues_GeneratesCorrectOrder()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public enum Days
                {
                    Monday = 1,
                    Tuesday,
                    Wednesday,
                    Thursday,
                    Friday,
                    Saturday,
                    Sunday
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export const Days = Object.freeze({
  Monday: 1,
  Tuesday: 2,
  Wednesday: 3,
  Thursday: 4,
  Friday: 5,
  Saturday: 6,
  Sunday: 7
});
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithImportWhitelistMembers_GeneratesMergedImports()
    {
        // Arrange
        var code = """
            using System.Numerics;

            public static class TestClass
            {
                public static BigInteger Value = BigInteger.Parse("33");

                public static double LogValue() => BigInteger.Log(BigInteger.Parse("44"));
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"import { _155212572c9a3297, _fb5a811e7a32a324 } from ""System/Numerics/BigIntegerModule.js"";
export let Value = _155212572c9a3297(""33"");
export function LogValue() {
  return _fb5a811e7a32a324(_155212572c9a3297(""44""));
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleStaticReference_GeneratesModuleImport()
    {
        // Arrange
        var code = """
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo
            {
                [ECMAScript.ECMAScriptModule("System/RuntimeModule.js")]
                public static class RuntimeModule
                {
                    public static int Make() => 42;
                }

                [ECMAScript.ECMAScriptModule("System/ConsumerModule.js")]
                public static class ConsumerModule
                {
                    public static int Create() => RuntimeModule.Make();
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code);
        var consumer = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "ConsumerModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();
        var converter = new AstConverter(consumer, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"import { Make } from ""System/RuntimeModule.js"";
export function Create() {
  return Make();
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleStaticPropertyReference_GeneratesGetterImport()
    {
        // Arrange
        var code = """
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo
            {
                [ECMAScript.ECMAScriptModule("System/RuntimeModule.js")]
                public static class RuntimeModule
                {
                    public static int Value { get; } = 42;
                }

                [ECMAScript.ECMAScriptModule("System/ConsumerModule.js")]
                public static class ConsumerModule
                {
                    public static int Create() => RuntimeModule.Value;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code);
        var consumer = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "ConsumerModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();
        var converter = new AstConverter(consumer, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"import { get_Value } from ""System/RuntimeModule.js"";
export function Create() {
  return get_Value();
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleStaticReferenceViaAlias_GeneratesModuleImport()
    {
        // Arrange
        var code = """
            using System;
            using Runtime = Demo.RuntimeModule;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo
            {
                [ECMAScript.ECMAScriptModule("System/RuntimeModule.js")]
                public static class RuntimeModule
                {
                    public static int Make() => 42;
                }

                [ECMAScript.ECMAScriptModule("System/ConsumerModule.js")]
                public static class ConsumerModule
                {
                    public static int Create() => Runtime.Make();
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code);
        var consumer = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "ConsumerModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();
        var converter = new AstConverter(consumer, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"import { Make } from ""System/RuntimeModule.js"";
export function Create() {
  return Make();
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleMethodReference_GeneratesModuleImport()
    {
        // Arrange
        var code = """
            using System;
            using Runtime = Demo.RuntimeModule;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo
            {
                [ECMAScript.ECMAScriptModule("System/RuntimeModule.js")]
                public static class RuntimeModule
                {
                    public static int Make() => 42;
                }

                [ECMAScript.ECMAScriptModule("System/ConsumerModule.js")]
                public static class ConsumerModule
                {
                    public static int Create()
                    {
                        Func<int> factory = Runtime.Make;
                        return factory();
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code);
        var consumer = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "ConsumerModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();
        var converter = new AstConverter(consumer, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"import { Make } from ""System/RuntimeModule.js"";
export function Create() {
  let factory = Make;
  return factory();
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleStaticPropertyReferenceViaAlias_GeneratesGetterImport()
    {
        // Arrange
        var code = """
            using System;
            using Runtime = Demo.RuntimeModule;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo
            {
                [ECMAScript.ECMAScriptModule("System/RuntimeModule.js")]
                public static class RuntimeModule
                {
                    public static int Value { get; } = 42;
                }

                [ECMAScript.ECMAScriptModule("System/ConsumerModule.js")]
                public static class ConsumerModule
                {
                    public static int Create() => Runtime.Value;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code);
        var consumer = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "ConsumerModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();
        var converter = new AstConverter(consumer, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"import { get_Value } from ""System/RuntimeModule.js"";
export function Create() {
  return get_Value();
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleStaticPropertyReferenceViaGlobalAlias_GeneratesGetterImport()
    {
        // Arrange
        var code = """
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo
            {
                [ECMAScript.ECMAScriptModule("System/RuntimeModule.js")]
                public static class RuntimeModule
                {
                    public static int Value { get; } = 42;
                }

                [ECMAScript.ECMAScriptModule("System/ConsumerModule.js")]
                public static class ConsumerModule
                {
                    public static int Create() => global::Demo.RuntimeModule.Value;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code);
        var consumer = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "ConsumerModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();
        var converter = new AstConverter(consumer, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"import { get_Value } from ""System/RuntimeModule.js"";
export function Create() {
  return get_Value();
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleNestedClassStaticReference_GeneratesNestedTypeImport()
    {
        // Arrange
        var code = """
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo
            {
                [ECMAScript.ECMAScriptModule("System/RuntimeModule.js")]
                public static class RuntimeModule
                {
                    public class Helpers
                    {
                        public static int Make() => 42;
                    }
                }

                [ECMAScript.ECMAScriptModule("System/ConsumerModule.js")]
                public static class ConsumerModule
                {
                    public static int Create() => RuntimeModule.Helpers.Make();
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code);
        var consumer = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "ConsumerModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();
        var converter = new AstConverter(consumer, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"import { Helpers } from ""System/RuntimeModule.js"";
export function Create() {
  return Helpers.Make();
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleImportShadowedByLocalVariable_UsesAliasedImport()
    {
        var code = """
            using System;
            using Runtime = Demo.RuntimeModule;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo
            {
                [ECMAScript.ECMAScriptModule("System/RuntimeModule.js")]
                public static class RuntimeModule
                {
                    public static int Make() => 42;
                }

                [ECMAScript.ECMAScriptModule("System/ConsumerModule.js")]
                public static class ConsumerModule
                {
                    public static int Create()
                    {
                        var Make = 1;
                        return Runtime.Make() + Make;
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code);
        var consumer = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "ConsumerModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();
        var converter = new AstConverter(consumer, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        var makeId = ImportBindingName("System/RuntimeModule.js", "Make");
        Assert.AreEqual(
$@"import {{ Make as {makeId} }} from ""System/RuntimeModule.js"";
export function Create() {{
  let Make = 1;
  return {makeId}() + Make;
}}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleDuplicateImportNames_UsesDistinctAliasedImports()
    {
        var code = """
            using System;
            using Left = Demo.LeftModule;
            using Right = Demo.RightModule;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo
            {
                [ECMAScript.ECMAScriptModule("System/LeftModule.js")]
                public static class LeftModule
                {
                    public static int Make() => 1;
                }

                [ECMAScript.ECMAScriptModule("System/RightModule.js")]
                public static class RightModule
                {
                    public static int Make() => 2;
                }

                [ECMAScript.ECMAScriptModule("System/ConsumerModule.js")]
                public static class ConsumerModule
                {
                    public static int Create() => Left.Make() + Right.Make();
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code);
        var consumer = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "ConsumerModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();
        var converter = new AstConverter(consumer, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        var rightMakeId = ImportBindingName("System/RightModule.js", "Make");
        Assert.AreEqual(
$@"import {{ Make }} from ""System/LeftModule.js"";
import {{ Make as {rightMakeId} }} from ""System/RightModule.js"";
export function Create() {{
  return Make() + {rightMakeId}();
}}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithInlineWhitelistAnonymousObject_PreservesObjectLiteralArguments()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public static bool Check() => object.Equals(new { Value = 1 }, new { Value = 1 });
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.AreEqual(
@"export function Check() {
  return { Value: 1 } === { Value: 1 };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    #endregion
}
