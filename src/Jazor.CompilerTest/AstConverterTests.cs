using System.Diagnostics;
using System.Threading;
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

    private static void AssertScriptEqual(string expected, string? actual)
        => Assert.AreEqual(expected.ReplaceLineEndings("\n"), actual?.ReplaceLineEndings("\n"));

    private static string PropertyBackingFieldName(INamedTypeSymbol containingType, string propertyName)
    {
        var property = containingType.GetMembers(propertyName).OfType<IPropertySymbol>().Single();
        return Format.HashName(property.OriginalDefinition.ToDisplayString(Format.NameFormat));
    }

    private static string ConstructorHelperName(INamedTypeSymbol containingType, int parameterCount)
    {
        var constructor = containingType.InstanceConstructors
            .Single(ctor => !ctor.IsImplicitlyDeclared && ctor.Parameters.Length == parameterCount);
        return $"$ctor_{Format.HashName(constructor.OriginalDefinition.ToDisplayString(Format.NameFormat)).TrimStart('_')}";
    }

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

    private static (INamedTypeSymbol, SemanticModel) CompileAndGetSymbol(string code, string className)
    {
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [CSharpSyntaxTree.ParseText(code)],
            Net100.References.All,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var syntaxTree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var classDeclaration = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single(node => node.Identifier.Text == className);
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

        Assert.IsNotNull(classSymbol);
        return (classSymbol, semanticModel);
    }

    private static async Task AssertCrossModuleStaticFieldMutationThrowsAsync(string fieldDeclaration, string statement)
    {
        var code = $$"""
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
                    {{fieldDeclaration}}
                }

                [ECMAScript.ECMAScriptModule("System/ConsumerModule.js")]
                public static class ConsumerModule
                {
                    public static void Set()
                    {
                        {{statement}}
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code, "ConsumerModule");
        var consumer = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "ConsumerModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();
        var converter = new AstConverter(consumer, semanticModel);

        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "Cross-module static field mutation");
        StringAssert.Contains(exception.Message, "read-only");
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

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code, "TestClass");
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

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code, "TestClass");
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);
        Assert.AreEqual("类 TestClass 不是 public，无法转换", exception.Message);
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
    public async Task Convert_ClassWithAsyncMethod_GeneratesAsyncFunctionDeclaration()
    {
        var code = """
            public static class TestClass
            {
                public static async System.Threading.Tasks.Task TestMethodAsync()
                {
                    await System.Threading.Tasks.Task.CompletedTask;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export async function TestMethodAsync() {
  await Promise.resolve();
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
    public async Task Convert_ClassWithOnlyEnum_ErasesDeclaration()
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
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        Assert.IsNull(module);
        Assert.IsNull(script);
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

    [TestMethod]
    public async Task Convert_ClassWithNestedClassImplementingInterface_GeneratesClassWithoutInterfaceArtifact()
    {
        var code = """
            public interface IMarker
            {
            }

            public static class TestClass
            {
                public class NestedClass : IMarker
                {
                    public int Field;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"export class NestedClass {
  Field;
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_NestedClassSymbol_ThrowsNotSupportedException()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code, "NestedClass");
        var converter = new AstConverter(classSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);

        Assert.AreEqual("嵌套类 NestedClass 需要扁平化处理", exception.Message);
    }

    [TestMethod]
    public async Task Convert_ClassWithStaticNestedClass_ThrowsNotSupportedException()
    {
        var code = """
            public static class TestClass
            {
                public static class NestedHelpers
                {
                    public static int Value = 1;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);

        Assert.AreEqual("Jazor 模块类中不支持静态成员类NestedHelpers。", exception.Message);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedInterface_ErasesDeclaration()
    {
        var code = """
            public static class TestClass
            {
                public interface IMarker
                {
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNull(module);
        Assert.IsNull(script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedStruct_ThrowsNotSupportedException()
    {
        var code = """
            public static class TestClass
            {
                public struct Data
                {
                    public int Value;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);

        Assert.AreEqual("Jazor 模块类不支持NamedType:Data。", exception.Message);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassEvent_ThrowsNotSupportedException()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public event System.EventHandler? Changed;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);

        Assert.AreEqual("Jazor member class does not support Event:Changed.", exception.Message);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassStaticConstructor_ThrowsNotSupportedException()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    static NestedClass()
                    {
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);

        Assert.AreEqual("Jazor member class does not support static constructor .cctor.", exception.Message);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassConstructorInitializer_ThrowsNotSupportedException()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public NestedClass() : this(1)
                    {
                    }

                    public NestedClass(int value)
                    {
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);

        Assert.AreEqual("Jazor member class does not support constructor initializer on .ctor.", exception.Message);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassMultipleInstanceConstructors_GeneratesDispatcher()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public int Value;

                    public NestedClass()
                    {
                    }

                    public NestedClass(int value)
                    {
                        Value = value;
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var nestedClass = classSymbol.GetTypeMembers("NestedClass").Single();
        var ctor0 = ConstructorHelperName(nestedClass, 0);
        var ctor1 = ConstructorHelperName(nestedClass, 1);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
$@"export class NestedClass {{
  Value;
  constructor() {{
    let $args = arguments;
    if ($args.length === 0) {{
      this.{ctor0}();
      return;
    }}
    if ($args.length === 1) {{
      let value = $args[0];
      this.{ctor1}(value);
      return;
    }}
    throw new Error(""No matching constructor overload for NestedClass."");
  }}
  {ctor0}() {{ }}
  {ctor1}(value) {{
    this.Value = value;
  }}
}}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassConstructorOverloadsWithSameArity_ThrowsNotSupportedException()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public NestedClass(int value)
                    {
                    }

                    public NestedClass(string value)
                    {
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);

        StringAssert.Contains(exception.Message, "Jazor member class constructor overloads are not uniquely dispatchable by argument count NestedClass.");
        StringAssert.Contains(exception.Message, "Conflict at argument count 1");
        StringAssert.Contains(exception.Message, "NestedClass(int value) [args:1]");
        StringAssert.Contains(exception.Message, "NestedClass(string value) [args:1]");
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassConstructorOverloadsWithOptionalParameterOverlap_ThrowsNotSupportedException()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public NestedClass(int value)
                    {
                    }

                    public NestedClass(int value, int increment = 1)
                    {
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);

        StringAssert.Contains(exception.Message, "Jazor member class constructor overloads are not uniquely dispatchable by argument count NestedClass.");
        StringAssert.Contains(exception.Message, "Conflict at argument count 1");
        StringAssert.Contains(exception.Message, "NestedClass(int value) [args:1]");
        StringAssert.Contains(exception.Message, "NestedClass(int value, int increment = <default>) [args:1..2]");
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassInheritance_GeneratesExtendsClause()
    {
        var code = """
            public static class TestClass
            {
                public class BaseClass
                {
                }

                public class NestedClass : BaseClass
                {
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export class BaseClass { }
export class NestedClass extends BaseClass {
  constructor() {
    super();
  }
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassInheritance_DerivedBeforeBase_EmitsBaseFirst()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass : BaseClass
                {
                }

                public class BaseClass
                {
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export class BaseClass { }
export class NestedClass extends BaseClass {
  constructor() {
    super();
  }
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassThreeLevelInheritanceOutOfOrder_EmitsBaseChainFirst()
    {
        var code = """
            public static class TestClass
            {
                public class Level3 : Level2
                {
                }

                public class Level1
                {
                }

                public class Level2 : Level1
                {
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export class Level1 { }
export class Level2 extends Level1 {
  constructor() {
    super();
  }
}
export class Level3 extends Level2 {
  constructor() {
    super();
  }
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassBaseConstructorInitializer_GeneratesSuperCall()
    {
        var code = """
            public static class TestClass
            {
                public class BaseClass
                {
                    public int Field;

                    public BaseClass(int value)
                    {
                        Field = value;
                    }
                }

                public class NestedClass : BaseClass
                {
                    public NestedClass(int value) : base(value + 1)
                    {
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export class BaseClass {
  Field;
  constructor(value) {
    this.Field = value;
  }
}
export class NestedClass extends BaseClass {
  constructor(value) {
    super(value + 1);
  }
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassMultipleConstructorsAndBaseInitializers_GeneratesDispatcher()
    {
        var code = """
            public static class TestClass
            {
                public class BaseClass
                {
                    public int Field;

                    public BaseClass(int value)
                    {
                        Field = value;
                    }
                }

                public class NestedClass : BaseClass
                {
                    public NestedClass() : base(1)
                    {
                    }

                    public NestedClass(int value) : base(value + 1)
                    {
                        Field = value * 2;
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var nestedClass = classSymbol.GetTypeMembers("NestedClass").Single();
        var ctor0 = ConstructorHelperName(nestedClass, 0);
        var ctor1 = ConstructorHelperName(nestedClass, 1);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
$@"export class BaseClass {{
  Field;
  constructor(value) {{
    this.Field = value;
  }}
}}
export class NestedClass extends BaseClass {{
  constructor() {{
    let $args = arguments;
    if ($args.length === 0) {{
      super(1);
      this.{ctor0}();
      return;
    }}
    if ($args.length === 1) {{
      let value = $args[0];
      super(value + 1);
      this.{ctor1}(value);
      return;
    }}
    throw new Error(""No matching constructor overload for NestedClass."");
  }}
  {ctor0}() {{ }}
  {ctor1}(value) {{
    this.Field = value * 2;
  }}
}}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassNamedBaseConstructorInitializer_ThrowsNotSupportedException()
    {
        var code = """
            public static class TestClass
            {
                public class BaseClass
                {
                    public BaseClass(int first, int second)
                    {
                    }
                }

                public class NestedClass : BaseClass
                {
                    public NestedClass() : base(second: 2, first: 1)
                    {
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);

        Assert.AreEqual("Jazor member class does not support named constructor initializer arguments.", exception.Message);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassBaseMethodCall_GeneratesSuperInvocation()
    {
        var code = """
            public static class TestClass
            {
                public class BaseClass
                {
                    public virtual int Value()
                    {
                        return 1;
                    }
                }

                public class NestedClass : BaseClass
                {
                    public override int Value()
                    {
                        return base.Value() + 1;
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export class BaseClass {
  Value() {
    return 1;
  }
}
export class NestedClass extends BaseClass {
  constructor() {
    super();
  }
  Value() {
    return super.Value() + 1;
  }
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassBasePropertyReadWrite_GeneratesSuperPropertyAccess()
    {
        var code = """
            public static class TestClass
            {
                public class BaseClass
                {
                    public virtual int Value { get; set; }
                }

                public class NestedClass : BaseClass
                {
                    public int Read()
                    {
                        return base.Value;
                    }

                    public void Write(int value)
                    {
                        base.Value = value + 1;
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var baseClass = classSymbol.GetTypeMembers("BaseClass").Single();
        var backingFieldName = PropertyBackingFieldName(baseClass, "Value");
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
$@"export class BaseClass {{
  #{backingFieldName};
  get Value() {{
    return this.#{backingFieldName};
  }}
  set Value(value) {{
    this.#{backingFieldName} = value;
  }}
}}
export class NestedClass extends BaseClass {{
  constructor() {{
    super();
  }}
  Read() {{
    return super.Value;
  }}
  Write(value) {{
    super.Value = value + 1;
  }}
}}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassBaseMethodReference_GeneratesSuperForwarder()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public class BaseClass
                {
                    public virtual int Value(int value)
                    {
                        return value + 1;
                    }
                }

                public class NestedClass : BaseClass
                {
                    public Func<int, int> Get()
                    {
                        return base.Value;
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export class BaseClass {
  Value(value) {
    return value + 1;
  }
}
export class NestedClass extends BaseClass {
  constructor() {
    super();
  }
  Get() {
    return value => super.Value(value);
  }
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassBaseFieldAccess_ThrowsOperationTransformationException()
    {
        var code = """
            public static class TestClass
            {
                public class BaseClass
                {
                    public int Value;
                }

                public class NestedClass : BaseClass
                {
                    public int Read()
                    {
                        return base.Value;
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);

        Assert.AreEqual("Base field access 'Value' is not supported because member-class fields lower to instance-owned state rather than prototype members. Use a property or method seam instead.", exception.Message);
    }

    [TestMethod]
    public async Task Convert_ClassWithExternalBaseType_ThrowsNotSupportedException()
    {
        var code = """
            public class ExternalBase
            {
            }

            public static class TestClass
            {
                public class NestedClass : ExternalBase
                {
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code, "TestClass");
        var converter = new AstConverter(classSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);

        Assert.AreEqual("Jazor member class does not support inheritance NestedClass : ExternalBase.", exception.Message);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassNestedInterface_ErasesDeclaration()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public interface IMarker
                    {
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export class NestedClass { }
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedAbstractMethod_ThrowsNotSupportedException()
    {
        var code = """
            public static class TestClass
            {
                public abstract class NestedClass
                {
                    public abstract int Compute();
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);

        Assert.AreEqual("Jazor member class does not support abstract method Compute.", exception.Message);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedAbstractProperty_ThrowsNotSupportedException()
    {
        var code = """
            public static class TestClass
            {
                public abstract class NestedClass
                {
                    public abstract int Value { get; }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);

        Assert.AreEqual("Jazor member class does not support abstract property Value.", exception.Message);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassNestedEnum_ErasesDeclaration()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public enum Kind
                    {
                        One
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export class NestedClass { }
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedExternMethod_ThrowsNotSupportedException()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public extern int Native();
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);

        Assert.AreEqual("Jazor member class method Native requires a body.", exception.Message);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassFieldInitializer_PreservesInitializer()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public int Value = 42;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export class NestedClass {
  Value = 42;
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedAutoPropertyInitializer_GeneratesHashedBackingField()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public int Value { get; set; } = 42;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var nestedClass = classSymbol.GetTypeMembers("NestedClass").Single();
        var backingFieldName = PropertyBackingFieldName(nestedClass, "Value");
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
$@"export class NestedClass {{
  #{backingFieldName} = 42;
  get Value() {{
    return this.#{backingFieldName};
  }}
  set Value(value) {{
    this.#{backingFieldName} = value;
  }}
}}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedInitOnlyPropertyInitializer_GeneratesGetterOnly()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public int Value { get; init; } = 42;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var nestedClass = classSymbol.GetTypeMembers("NestedClass").Single();
        var backingFieldName = PropertyBackingFieldName(nestedClass, "Value");
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
$@"export class NestedClass {{
  #{backingFieldName} = 42;
  get Value() {{
    return this.#{backingFieldName};
  }}
}}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedStaticAutoPropertyInitializer_GeneratesStaticHashedBackingField()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public static int Value { get; set; } = 42;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var nestedClass = classSymbol.GetTypeMembers("NestedClass").Single();
        var backingFieldName = PropertyBackingFieldName(nestedClass, "Value");
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
$@"export class NestedClass {{
  static #{backingFieldName} = 42;
  static get Value() {{
    return this.#{backingFieldName};
  }}
  static set Value(value) {{
    this.#{backingFieldName} = value;
  }}
}}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedStaticPropertyDateTimeInitializer_MergesImports()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public class NestedClass
                {
                    public static DateTime Value { get; } = new DateTime(2024, 1, 2);
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var nestedClass = classSymbol.GetTypeMembers("NestedClass").Single();
        var backingFieldName = PropertyBackingFieldName(nestedClass, "Value");
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
$@"import {{ _4cb33a818161a3e1 }} from ""System/DateTimeModule.js"";
export class NestedClass {{
  static #{backingFieldName} = _4cb33a818161a3e1(2024, 1, 2);
  static get Value() {{
    return this.#{backingFieldName};
  }}
}}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassExplicitGetterProperty_GeneratesAccessorMethods()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public int Value
                    {
                        get { return 1; }
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export class NestedClass {
  get Value() {
    return 1;
  }
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassExpressionBodyMethod_GeneratesMethod()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public int Square(int x) => x * x;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export class NestedClass {
  Square(x) {
    return x * x;
  }
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassAsyncMethod_GeneratesAsyncMethod()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public async System.Threading.Tasks.Task LoadAsync()
                    {
                        await System.Threading.Tasks.Task.CompletedTask;
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export class NestedClass {
  async LoadAsync() {
    await Promise.resolve();
  }
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassArrowProperty_GeneratesGetterMethod()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public int Value => 1;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export class NestedClass {
  get Value() {
    return 1;
  }
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassExpressionBodyAccessors_GeneratesAccessorMethods()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public int current = 1;

                    public int Value
                    {
                        get => current;
                        set => current = value;
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export class NestedClass {
  current = 1;
  get Value() {
    return this.current;
  }
  set Value(value) {
    this.current = value;
  }
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassExpressionBodyConstructor_GeneratesConstructor()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public int Value;

                    public NestedClass() => Value = 1;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export class NestedClass {
  Value;
  constructor() {
    this.Value = 1;
  }
}
", script);
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
    public async Task Convert_ClassWithOnlyEnum_MultipleValues_ErasesDeclaration()
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
        Assert.IsNull(module);
        Assert.IsNull(script);

    }

    [TestMethod]
    public async Task Convert_ClassWithOnlyFlagsEnum_ErasesDeclaration()
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
        Assert.IsNull(module);
        Assert.IsNull(script);

    }

    [TestMethod]
    public async Task Convert_ClassWithPrivateEnum_ErasesDeclaration()
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
        Assert.IsNull(module);
        Assert.IsNull(script);

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
  let __cacc$db228d45b7e701d8a374d195;
  _aa3181446f60dc6e = (__cacc$db228d45b7e701d8a374d195 = value, __cacc$db228d45b7e701d8a374d195 == null ? undefined : __cacc$db228d45b7e701d8a374d195.trim());
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
    public async Task Convert_ClassWithStaticConstructor_ThrowsNotSupportedException()
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
        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);
        Assert.AreEqual("Jazor 模块类.cctor不支持静态构造函数。", exception.Message);

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
    public async Task Convert_ClassWithDelegate_ThrowsNotSupportedException()
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
        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);
        Assert.AreEqual("Jazor 模块类不支持NamedType:MathOp。", exception.Message);

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

        // Act & Assert - 事件声明本身当前不支持导出
        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);
        Assert.AreEqual("Jazor 模块类不支持Event:MyEvent。", exception.Message);
    }

    [TestMethod]
    public async Task Convert_ClassWithExternMethod_ThrowsNotSupportedException()
    {
        var code = """
            public static class TestClass
            {
                public static extern int Native();
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);

        Assert.AreEqual("Jazor 不支持转换方法 Native，无法从操作生成函数体。", exception.Message);
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
        AssertScriptEqual(
@"export let NullableField = null;
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithConstNullStringField_GeneratesNullDeclaration()
    {
        // Arrange
        var code = """
            public static class TestClass
            {
                public const string? Missing = null;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        AssertScriptEqual(
@"export const Missing = null;
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
@"import { _797b5246c9b12c8d } from ""System/IComparableT1Module.js"";
export function Max(a, b) {
  return _797b5246c9b12c8d(a, b) > 0 ? a : b;
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
        AssertScriptEqual(
@"export let CharField = ""A"";
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithNameOfFieldInitializer_UsesDoubleQuotedStringLiteral()
    {
        var code = """
            public static class TestClass
            {
                public static string ClassName = nameof(TestClass);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export let ClassName = ""TestClass"";
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithDefaultDateTimeField_UsesDefaultConstructorHelper()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static DateTime Value = default(DateTime);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { _bfa8ee5dd46e2005 } from ""System/DateTimeModule.js"";
export let Value = _bfa8ee5dd46e2005();
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithDefaultDateTimeOffsetField_UsesDefaultConstructorHelper()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static DateTimeOffset Value = default(DateTimeOffset);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { _12b4f3f1dc14bea9 } from ""System/DateTimeOffsetModule.js"";
export let Value = _12b4f3f1dc14bea9();
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithDefaultTimeSpanField_UsesDefaultConstructorHelper()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static TimeSpan Value = default(TimeSpan);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { _5af0f6ad850e6702 } from ""System/TimeSpanModule.js"";
export let Value = _5af0f6ad850e6702();
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithDefaultDateOnlyField_UsesDefaultConstructorHelper()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static DateOnly Value = default(DateOnly);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { _5f8053a9657a0844 } from ""System/DateOnlyModule.js"";
export let Value = _5f8053a9657a0844();
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithDefaultTimeOnlyField_UsesDefaultConstructorHelper()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static TimeOnly Value = default(TimeOnly);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { _9f78f92d0753f4cf } from ""System/TimeOnlyModule.js"";
export let Value = _9f78f92d0753f4cf();
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithDefaultBigIntegerField_GeneratesZeroBigInt()
    {
        var code = """
            using System.Numerics;

            public static class TestClass
            {
                public static BigInteger Value = default(BigInteger);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export let Value = 0n;
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithDefaultHalfField_GeneratesZeroNumber()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static Half Value = default(Half);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export let Value = 0;
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithDefaultEnumField_GeneratesZero()
    {
        var code = """
            public static class TestClass
            {
                public enum Kind
                {
                    None = 0,
                    One = 1
                }

                public static Kind Value = default(Kind);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export let Value = 0;
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithDefaultLongBackedEnumField_GeneratesBigIntZero()
    {
        var code = """
            public static class TestClass
            {
                public enum Kind : long
                {
                    None = 0L,
                    One = 9007199254740993L
                }

                public static Kind Value = default(Kind);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export let Value = 0n;
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithDefaultCharField_GeneratesNullCharacterString()
    {
        var code = """
            public static class TestClass
            {
                public static char Value = default(char);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export let Value = ""\0"";
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithConstEnumField_GeneratesNumericLiteral()
    {
        var code = """
            public static class TestClass
            {
                public enum Kind
                {
                    None = 0,
                    One = 1
                }

                public const Kind Value = Kind.One;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export const Value = 1;
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithConstLongBackedEnumField_GeneratesBigIntLiteral()
    {
        var code = """
            public static class TestClass
            {
                public enum Kind : long
                {
                    None = 0L,
                    One = 9007199254740993L
                }

                public const Kind Value = Kind.One;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export const Value = 9007199254740993n;
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
    public async Task Convert_MethodWithNullableDefaultParameter_GeneratesNullDefaultLiteral()
    {
        // Arrange
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

        // Act
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        // Assert
        AssertScriptEqual(
@"export function Process(name = null) { }
", script);
    }

    [TestMethod]
    public async Task Convert_MethodWithEnumDefaultParameter_GeneratesNumericDefaultLiteral()
    {
        var code = """
            public static class TestClass
            {
                public enum Kind
                {
                    None = 0,
                    One = 1
                }

                public static int Check(Kind value = Kind.One)
                {
                    return (int)value;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export function Check(value = 1) {
  return value;
}
", script);
    }

    [TestMethod]
    public async Task Convert_MethodWithHalfDefaultParameter_GeneratesNumericDefaultLiteral()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static int Check(Half value = (Half)1.5f)
                {
                    return 0;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export function Check(value = 1.5) {
  return 0;
}
", script);
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

    [TestMethod]
    public async Task Convert_ClassWithExpressionBodyMethodThatNeedsGeneratedTemporaries_EmitsThemInsideFunctionBody()
    {
        var code = """
            public static class TestClass
            {
                public static int ParseOrZero(string input)
                    => int.TryParse(input, out var value) ? value : 0;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "export function ParseOrZero(input) {");
        StringAssert.Contains(script, "return ");
        Assert.IsTrue(
            System.Text.RegularExpressions.Regex.IsMatch(script, @"let\s+value,\s+__ref\$[0-9a-f]+;")
            || System.Text.RegularExpressions.Regex.IsMatch(script, @"let\s+__ref\$[0-9a-f]+,\s+value;"),
            $"Expected generated temporaries to be materialized inside the function body.{Environment.NewLine}{script}");
    }

    [TestMethod]
    public async Task Convert_ClassWithTupleBinaryFieldInitializerThatNeedsTemporary_WrapsInitializerInIife()
    {
        var code = """
            public static class TestClass
            {
                public static bool IsOrigin = Create() == (0, 0);

                private static (int left, int right) Create()
                    => (1, 2);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "export let IsOrigin = (() => {");
        Assert.IsTrue(
            System.Text.RegularExpressions.Regex.IsMatch(script, @"let\s+__tbin\$[0-9a-f]+;"),
            $"Expected tuple-comparison temporary to be declared inside an initializer IIFE.{Environment.NewLine}{script}");
        StringAssert.Contains(script, "return ");
        StringAssert.Contains(script, "})();");
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
    public async Task Convert_ClassWithOnlyEnum_AutoIncrementValues_ErasesDeclaration()
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
        Assert.IsNull(module);
        Assert.IsNull(script);

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
    public async Task Convert_ClassWithLinqWhereAndToListOnIEnumerable_UsesArrayMethodsWithoutImport()
    {
        var code = """
            using System.Collections.Generic;
            using System.Linq;

            public static class TestClass
            {
                public static List<int> Filter(IEnumerable<int> source) => source.Where(x => x > 1).ToList();
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        Assert.IsFalse(script.Contains("System/Linq/EnumerableModule.js", StringComparison.Ordinal));
        StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: source is null\");");
        StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: predicate is null\");");
        StringAssert.Contains(script, "return Array.from(__src).filter(__callback);");
    }

    [TestMethod]
    public async Task Convert_ClassWithLinqSelectIndexAndToArrayOnIEnumerable_UsesArrayMethodsWithoutImport()
    {
        var code = """
            using System.Collections.Generic;
            using System.Linq;

            public static class TestClass
            {
                public static int[] Project(IEnumerable<int> source) => source.Select((x, index) => x + index).ToArray();
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        Assert.IsFalse(script.Contains("System/Linq/EnumerableModule.js", StringComparison.Ordinal));
        StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: source is null\");");
        StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: selector is null\");");
        StringAssert.Contains(script, "return Array.from(__src).map(__callback);");
    }

    [TestMethod]
    public async Task Convert_ClassWithLinqWhereAndToListOnList_UsesArrayMethodsWithoutImport()
    {
        var code = """
            using System.Collections.Generic;
            using System.Linq;

            public static class TestClass
            {
                public static List<int> Filter(List<int> source) => source.Where(x => x > 1).ToList();
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        Assert.IsFalse(script.Contains("System/Linq/EnumerableModule.js", StringComparison.Ordinal));
        StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: source is null\");");
        StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: predicate is null\");");
        StringAssert.Contains(script, "return __src.filter(__callback);");
    }

    [TestMethod]
    public async Task Convert_ClassWithLinqSelectIndexAndToArrayOnList_UsesArrayMethodsWithoutImport()
    {
        var code = """
            using System.Collections.Generic;
            using System.Linq;

            public static class TestClass
            {
                public static int[] Project(List<int> source) => source.Select((x, index) => x + index).ToArray();
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        Assert.IsFalse(script.Contains("System/Linq/EnumerableModule.js", StringComparison.Ordinal));
        StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: source is null\");");
        StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: selector is null\");");
        StringAssert.Contains(script, "return __src.map(__callback);");
    }

    [TestMethod]
    public async Task Convert_ClassWithLinqWhereAndToListOnIList_UsesArrayMethodsWithoutImport()
    {
        var code = """
            using System.Collections.Generic;
            using System.Linq;

            public static class TestClass
            {
                public static List<int> Filter(IList<int> source) => source.Where(x => x > 1).ToList();
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        Assert.IsFalse(script.Contains("System/Linq/EnumerableModule.js", StringComparison.Ordinal));
        StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: source is null\");");
        StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: predicate is null\");");
        StringAssert.Contains(script, "return __src.filter(__callback);");
    }

    [TestMethod]
    public async Task Convert_ClassWithLinqSelectAndToArrayOnICollection_UsesArrayFromFastPath()
    {
        var code = """
            using System.Collections.Generic;
            using System.Linq;

            public static class TestClass
            {
                public static int[] Project(ICollection<int> source) => source.Select(x => x * 2).ToArray();
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        Assert.IsFalse(script.Contains("System/Linq/EnumerableModule.js", StringComparison.Ordinal));
        StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: source is null\");");
        StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: selector is null\");");
        StringAssert.Contains(script, "return Array.from(__src).map(__callback);");
    }

    [TestMethod]
    public async Task Convert_ClassWithDateOnlyParseAndDefaultToString_ImportsOnlyParseHelper()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static string Format() => DateOnly.Parse("2024-01-02").ToString();
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _e2640560d207afce } from ""System/DateOnlyModule.js"";
export function Format() {
  return _e2640560d207afce(""2024-01-02"").toString();
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithDateTimeOffsetParseAndFormattedToString_ImportsOnlyNeededHelpers()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static string Format() => DateTimeOffset.Parse("2024-01-02T03:04:05+08:00").ToString("O", null);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _25187a24d190d864, _e856edbfd7db0646 } from ""System/DateTimeOffsetModule.js"";
export function Format() {
  return _e856edbfd7db0646(_25187a24d190d864(""2024-01-02T03:04:05+08:00""), ""O"", null);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithCultureInfoNameAndToString_ImportsOnlyConstructorAndToStringHelper()
    {
        var code = """
            using System.Globalization;

            public static class TestClass
            {
                public static string Format()
                {
                    var culture = new CultureInfo("en-US");
                    return culture.Name + "|" + culture.ToString();
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _559b27327f84f1af, _b7486264ae338f27 } from ""System/Globalization/CultureInfoModule.js"";
export function Format() {
  let culture = _b7486264ae338f27(""en-US"");
  return culture + ""|"" + _559b27327f84f1af(culture);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithDateOnlyStringConcat_ImportsOnlyParseHelper()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static string Format() => "date=" + DateOnly.Parse("2024-01-02");
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _e2640560d207afce } from ""System/DateOnlyModule.js"";
export function Format() {
  return ""date="" + _e2640560d207afce(""2024-01-02"");
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithDateTimeOffsetStringConcat_ImportsOnlyParseHelper()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static string Format() => "dto=" + DateTimeOffset.Parse("2024-01-02T03:04:05+08:00");
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _25187a24d190d864 } from ""System/DateTimeOffsetModule.js"";
export function Format() {
  return ""dto="" + _25187a24d190d864(""2024-01-02T03:04:05+08:00"");
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithDateTimeStringConcat_ImportsOnlyConstructorHelper()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static string Format() => "dt=" + new DateTime(2024, 1, 2);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _4cb33a818161a3e1 } from ""System/DateTimeModule.js"";
export function Format() {
  return ""dt="" + _4cb33a818161a3e1(2024, 1, 2);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithTimeOnlyStringConcat_ImportsOnlyConstructorHelper()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static string Format() => "time=" + new TimeOnly(12, 30, 0);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _e9a3481b3456aad4 } from ""System/TimeOnlyModule.js"";
export function Format() {
  return ""time="" + _e9a3481b3456aad4(12, 30, 0);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithTimeSpanStringConcat_ImportsOnlyConstructorHelper()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static string Format() => "span=" + new TimeSpan(1, 2, 3);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _6f22e268aec62fe7 } from ""System/TimeSpanModule.js"";
export function Format() {
  return ""span="" + _6f22e268aec62fe7(1, 2, 3);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithIntegerCopySign_UsesInlineWithoutHelperImports()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static float FloatCopy(float value, float sign) => float.CopySign(value, sign);

                public static int IntCopy(int value, int sign) => int.CopySign(value, sign);

                public static long LongCopy(long value, long sign) => long.CopySign(value, sign);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"export function FloatCopy(value, sign) {
  return sign < 0 || Object.is(sign, -0) ? -Math.abs(value) : Math.abs(value);
}
export function IntCopy(value, sign) {
  return sign < 0 ? -Math.abs(value) : Math.abs(value);
}
export function LongCopy(value, sign) {
  return sign < 0n ? value < 0n ? value : -value : value < 0n ? -value : value;
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithInt32Intrinsics_UsesInlineWithoutHelperImports()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static int SignedCopy(int value, int sign) => int.CopySign(value, sign);

                public static int SignedClamp(int value, int min, int max) => int.Clamp(value, min, max);

                public static int SignedMeta(int value)
                    => int.Sign(value)
                    + int.Abs(value)
                    + int.Log2(value)
                    + int.LeadingZeroCount(value)
                    + int.TrailingZeroCount(value)
                    + (int.IsEvenInteger(value) ? 1 : 0)
                    + (int.IsNegative(value) ? 1 : 0)
                    + (int.IsOddInteger(value) ? 1 : 0)
                    + (int.IsPositive(value) ? 1 : 0)
                    + (int.IsPow2(value) ? 1 : 0);

                public static int SignedBounds(int left, int right) => int.Max(left, right) - int.Min(left, right);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"export function SignedCopy(value, sign) {
  return sign < 0 ? -Math.abs(value) : Math.abs(value);
}
export function SignedClamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}
export function SignedMeta(value) {
  return (value > 0 ? 1 : value < 0 ? -1 : 0) + Math.abs(value) + Math.floor(Math.log2(value)) + Math.clz32(value) + (value === 0 ? 32 : 31 - Math.clz32(value & -value)) + ((value & 1) === 0 ? 1 : 0) + (value < 0 ? 1 : 0) + ((value & 1) !== 0 ? 1 : 0) + (value > 0 ? 1 : 0) + (value > 0 && (value & value - 1) === 0 ? 1 : 0);
}
export function SignedBounds(left, right) {
  return Math.max(left, right) - Math.min(left, right);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithPrimitiveGetTypeCode_UsesInlineConstants()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static int Sum(byte b, int i, string s, DateTime dt)
                    => b.GetTypeCode() + i.GetTypeCode() + s.GetTypeCode() + dt.GetTypeCode();
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export function Sum(b, i, s, dt) {
  return 6 + 9 + 18 + 16;
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithInt16AndUInt16Intrinsics_UsesInlineWithoutHelperImports()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static short SignedCopy(short value, short sign) => short.CopySign(value, sign);

                public static short SignedClamp(short value, short min, short max) => short.Clamp(value, min, max);

                public static int SignedMeta(short value)
                    => short.Sign(value)
                    + short.Abs(value)
                    + short.Log2(value)
                    + short.LeadingZeroCount(value)
                    + short.TrailingZeroCount(value)
                    + short.RotateLeft(value, 3)
                    + short.RotateRight(value, 5)
                    + (short.IsEvenInteger(value) ? 1 : 0)
                    + (short.IsNegative(value) ? 1 : 0)
                    + (short.IsOddInteger(value) ? 1 : 0)
                    + (short.IsPositive(value) ? 1 : 0)
                    + (short.IsPow2(value) ? 1 : 0);

                public static int SignedBounds(short left, short right) => short.Max(left, right) - short.Min(left, right);

                public static ushort UnsignedClamp(ushort value, ushort min, ushort max) => ushort.Clamp(value, min, max);

                public static int UnsignedMeta(ushort value)
                    => ushort.Sign(value)
                    + ushort.Log2(value)
                    + ushort.LeadingZeroCount(value)
                    + ushort.TrailingZeroCount(value)
                    + ushort.RotateLeft(value, 3)
                    + ushort.RotateRight(value, 5)
                    + (ushort.IsEvenInteger(value) ? 1 : 0)
                    + (ushort.IsOddInteger(value) ? 1 : 0)
                    + (ushort.IsPow2(value) ? 1 : 0);

                public static int UnsignedBounds(ushort left, ushort right) => ushort.Max(left, right) - ushort.Min(left, right);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"export function SignedCopy(value, sign) {
  return sign < 0 ? -Math.abs(value) : Math.abs(value);
}
export function SignedClamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}
export function SignedMeta(value) {
  return (value > 0 ? 1 : value < 0 ? -1 : 0) + Math.abs(value) + Math.floor(Math.log2(value)) + (value === 0 ? 16 : Math.clz32(value & 0xFFFF) - 16) + (value === 0 ? 16 : Math.floor(Math.log2(value & 0xFFFF & -(value & 0xFFFF)))) + ((((value & 0xFFFF) << (3 & 15) | (value & 0xFFFF) >>> 16 - (3 & 15)) & 0xFFFF) << 16 >> 16) + ((((value & 0xFFFF) >>> (5 & 15) | (value & 0xFFFF) << 16 - (5 & 15)) & 0xFFFF) << 16 >> 16) + ((value & 1) === 0 ? 1 : 0) + (value < 0 ? 1 : 0) + ((value & 1) !== 0 ? 1 : 0) + (value > 0 ? 1 : 0) + (value > 0 && (value & value - 1) === 0 ? 1 : 0);
}
export function SignedBounds(left, right) {
  return Math.max(left, right) - Math.min(left, right);
}
export function UnsignedClamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}
export function UnsignedMeta(value) {
  return (value === 0 ? 0 : 1) + Math.floor(Math.log2(value)) + (value === 0 ? 16 : Math.clz32(value & 0xFFFF) - 16) + (value === 0 ? 16 : Math.floor(Math.log2(value & 0xFFFF & -(value & 0xFFFF)))) + ((value << (3 & 15) | value >>> 16 - (3 & 15)) & 0xFFFF) + ((value >>> (5 & 15) | value << 16 - (5 & 15)) & 0xFFFF) + ((value & 1) === 0 ? 1 : 0) + ((value & 1) !== 0 ? 1 : 0) + (value > 0 && (value & value - 1) === 0 ? 1 : 0);
}
export function UnsignedBounds(left, right) {
  return Math.max(left, right) - Math.min(left, right);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithByteIntrinsics_UsesInlineWithoutHelperImports()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static byte UnsignedClamp(byte value, byte min, byte max) => byte.Clamp(value, min, max);

                public static int UnsignedMeta(byte value)
                    => byte.Sign(value)
                    + byte.Log2(value)
                    + (byte.IsEvenInteger(value) ? 1 : 0)
                    + (byte.IsOddInteger(value) ? 1 : 0)
                    + (byte.IsPow2(value) ? 1 : 0);

                public static int UnsignedCounts(byte value)
                    => byte.LeadingZeroCount(value)
                    + byte.PopCount(value)
                    + byte.TrailingZeroCount(value);

                public static int UnsignedBounds(byte left, byte right) => byte.Max(left, right) - byte.Min(left, right);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"export function UnsignedClamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}
export function UnsignedMeta(value) {
  return (value === 0 ? 0 : 1) + Math.floor(Math.log2(value)) + ((value & 1) === 0 ? 1 : 0) + ((value & 1) !== 0 ? 1 : 0) + (value > 0 && (value & value - 1) === 0 ? 1 : 0);
}
export function UnsignedCounts(value) {
  return (value === 0 ? 8 : Math.clz32(value & 0xFF) - 24) + ((value & 1) + (value >> 1 & 1) + (value >> 2 & 1) + (value >> 3 & 1) + (value >> 4 & 1) + (value >> 5 & 1) + (value >> 6 & 1) + (value >> 7 & 1)) + (value === 0 ? 8 : Math.floor(Math.log2(value & 0xFF & -(value & 0xFF))));
}
export function UnsignedBounds(left, right) {
  return Math.max(left, right) - Math.min(left, right);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithUInt16AndUInt32DivRem_UsesImportHelpers()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static int Sum(ushort ushortLeft, ushort ushortRight, uint uintLeft, uint uintRight)
                {
                    var ushortPair = ushort.DivRem(ushortLeft, ushortRight);
                    var uintPair = uint.DivRem(uintLeft, uintRight);
                    return ushortPair.Quotient
                        + ushortPair.Remainder
                        + (int)uintPair.Quotient
                        + (int)uintPair.Remainder;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _80e78c0aa0b98fef } from ""System/UInt16Module.js"";
import { _8a073d758132b5bb } from ""System/UInt32Module.js"";
export function Sum(ushortLeft, ushortRight, uintLeft, uintRight) {
  let ushortPair = _80e78c0aa0b98fef(ushortLeft, ushortRight);
  let uintPair = _8a073d758132b5bb(uintLeft, uintRight);
  return ushortPair.Quotient + ushortPair.Remainder + uintPair.Quotient + uintPair.Remainder;
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithInt16DivRemAndPopCount_UsesImportHelpers()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static int Sum(short left, short right, short value)
                {
                    var pair = short.DivRem(left, right);
                    return pair.Quotient + pair.Remainder + short.PopCount(value);
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _1636c956519f95fa, _b2c1f15fae072110 } from ""System/Int16Module.js"";
export function Sum(left, right, value) {
  let pair = _b2c1f15fae072110(left, right);
  return pair.Quotient + pair.Remainder + _1636c956519f95fa(value);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithUInt16AndUInt32PopCount_UsesImportHelpers()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static ulong Sum(ushort ushortValue, uint uintValue)
                    => (ulong)ushort.PopCount(ushortValue) + (ulong)uint.PopCount(uintValue);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _2ea0cab4f3f489d9 } from ""System/UInt16Module.js"";
import { _96cd49e102b39e5b } from ""System/UInt32Module.js"";
export function Sum(ushortValue, uintValue) {
  return BigInt(_2ea0cab4f3f489d9(ushortValue)) + BigInt(_96cd49e102b39e5b(uintValue));
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithSByteUInt32AndUInt64Intrinsics_UsesInlineWithoutHelperImports()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static sbyte SignedCopy(sbyte value, sbyte sign) => sbyte.CopySign(value, sign);

                public static sbyte SignedClamp(sbyte value, sbyte min, sbyte max) => sbyte.Clamp(value, min, max);

                public static int SignedMeta(sbyte value)
                    => sbyte.Sign(value)
                    + sbyte.Abs(value)
                    + sbyte.Log2(value)
                    + (sbyte.IsEvenInteger(value) ? 1 : 0)
                    + (sbyte.IsNegative(value) ? 1 : 0)
                    + (sbyte.IsOddInteger(value) ? 1 : 0)
                    + (sbyte.IsPositive(value) ? 1 : 0)
                    + (sbyte.IsPow2(value) ? 1 : 0);

                public static int SignedBounds(sbyte left, sbyte right) => sbyte.Max(left, right) - sbyte.Min(left, right);

                public static uint UnsignedClamp(uint value, uint min, uint max) => uint.Clamp(value, min, max);

                public static ulong UnsignedMeta(uint value)
                    => uint.Sign(value)
                    + uint.Log2(value)
                    + uint.LeadingZeroCount(value)
                    + uint.TrailingZeroCount(value)
                    + uint.RotateLeft(value, 3)
                    + uint.RotateRight(value, 5)
                    + (uint.IsEvenInteger(value) ? 1 : 0)
                    + (uint.IsOddInteger(value) ? 1 : 0)
                    + (uint.IsPow2(value) ? 1 : 0);

                public static uint UnsignedBounds(uint left, uint right) => uint.Max(left, right) - uint.Min(left, right);

                public static ulong UnsignedLongClamp(ulong value, ulong min, ulong max) => ulong.Clamp(value, min, max);

                public static int UnsignedLongSign(ulong value) => ulong.Sign(value);

                public static bool UnsignedLongEven(ulong value) => ulong.IsEvenInteger(value);

                public static bool UnsignedLongOdd(ulong value) => ulong.IsOddInteger(value);

                public static bool UnsignedLongPow2(ulong value) => ulong.IsPow2(value);

                public static ulong UnsignedLongLog2(ulong value) => ulong.Log2(value);

                public static ulong UnsignedLongMax(ulong left, ulong right) => ulong.Max(left, right);

                public static ulong UnsignedLongMin(ulong left, ulong right) => ulong.Min(left, right);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"export function SignedCopy(value, sign) {
  return sign < 0 ? -Math.abs(value) : Math.abs(value);
}
export function SignedClamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}
export function SignedMeta(value) {
  return (value > 0 ? 1 : value < 0 ? -1 : 0) + Math.abs(value) + Math.floor(Math.log2(value)) + ((value & 1) === 0 ? 1 : 0) + (value < 0 ? 1 : 0) + ((value & 1) !== 0 ? 1 : 0) + (value > 0 ? 1 : 0) + (value > 0 && (value & value - 1) === 0 ? 1 : 0);
}
export function SignedBounds(left, right) {
  return Math.max(left, right) - Math.min(left, right);
}
export function UnsignedClamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}
export function UnsignedMeta(value) {
  return BigInt(value === 0 ? 0 : 1) + BigInt(Math.floor(Math.log2(value))) + BigInt(Math.clz32(value)) + BigInt(value === 0 ? 32 : 31 - Math.clz32(value >>> 0 & -(value >>> 0))) + BigInt((value << (3 & 31) | value >>> 32 - (3 & 31)) >>> 0) + BigInt((value >>> (5 & 31) | value << 32 - (5 & 31)) >>> 0) + BigInt((value & 1) === 0 ? 1 : 0) + BigInt((value & 1) !== 0 ? 1 : 0) + BigInt(value > 0 && (value & value - 1) === 0 ? 1 : 0);
}
export function UnsignedBounds(left, right) {
  return Math.max(left, right) - Math.min(left, right);
}
export function UnsignedLongClamp(value, min, max) {
  return value < min ? min : value > max ? max : value;
}
export function UnsignedLongSign(value) {
  return value === 0n ? 0 : 1;
}
export function UnsignedLongEven(value) {
  return value % 2n === 0n;
}
export function UnsignedLongOdd(value) {
  return value % 2n !== 0n;
}
export function UnsignedLongPow2(value) {
  return value > 0n && (value & value - 1n) === 0n;
}
export function UnsignedLongLog2(value) {
  return value === 0n ? 0n : BigInt(value.toString(2).length - 1);
}
export function UnsignedLongMax(left, right) {
  return left > right ? left : right;
}
export function UnsignedLongMin(left, right) {
  return left < right ? left : right;
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithInt64AndMathBigIntIntrinsics_UsesInlineWithoutHelperImports()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static long LongClamp(long value, long min, long max) => long.Clamp(value, min, max);

                public static int LongSign(long value) => long.Sign(value);

                public static long MathLongClamp(long value, long min, long max) => Math.Clamp(value, min, max);

                public static int MathLongSign(long value) => Math.Sign(value);

                public static long MathLongMax(long left, long right) => Math.Max(left, right);

                public static long MathLongMin(long left, long right) => Math.Min(left, right);

                public static ulong MathUnsignedLongClamp(ulong value, ulong min, ulong max) => Math.Clamp(value, min, max);

                public static ulong MathUnsignedLongMax(ulong left, ulong right) => Math.Max(left, right);

                public static ulong MathUnsignedLongMin(ulong left, ulong right) => Math.Min(left, right);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"export function LongClamp(value, min, max) {
  return value < min ? min : value > max ? max : value;
}
export function LongSign(value) {
  return value > 0n ? 1 : value < 0n ? -1 : 0;
}
export function MathLongClamp(value, min, max) {
  return value < min ? min : value > max ? max : value;
}
export function MathLongSign(value) {
  return value > 0n ? 1 : value < 0n ? -1 : 0;
}
export function MathLongMax(left, right) {
  return left > right ? left : right;
}
export function MathLongMin(left, right) {
  return left < right ? left : right;
}
export function MathUnsignedLongClamp(value, min, max) {
  return value < min ? min : value > max ? max : value;
}
export function MathUnsignedLongMax(left, right) {
  return left > right ? left : right;
}
export function MathUnsignedLongMin(left, right) {
  return left < right ? left : right;
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithBigIntegerSimpleIntrinsics_UsesInlineWithoutHelperImports()
    {
        var code = """
            using System.Numerics;

            public static class TestClass
            {
                public static BigInteger Abs(BigInteger value) => BigInteger.Abs(value);

                public static BigInteger Add(BigInteger left, BigInteger right) => BigInteger.Add(left, right);

                public static BigInteger Copy(BigInteger value, BigInteger sign) => BigInteger.CopySign(value, sign);

                public static int Compare(BigInteger left, BigInteger right) => BigInteger.Compare(left, right);

                public static int CompareTo(BigInteger left, BigInteger right) => left.CompareTo(right);

                public static BigInteger Divide(BigInteger left, BigInteger right) => BigInteger.Divide(left, right);

                public static bool EqualsValue(BigInteger left, BigInteger right) => left.Equals(right);

                public static BigInteger Max(BigInteger left, BigInteger right) => BigInteger.Max(left, right);

                public static BigInteger Min(BigInteger left, BigInteger right) => BigInteger.Min(left, right);

                public static bool Even(BigInteger value) => BigInteger.IsEvenInteger(value);

                public static bool Negative(BigInteger value) => BigInteger.IsNegative(value);

                public static BigInteger Negate(BigInteger value) => BigInteger.Negate(value);

                public static bool Odd(BigInteger value) => BigInteger.IsOddInteger(value);

                public static bool Positive(BigInteger value) => BigInteger.IsPositive(value);

                public static BigInteger Remainder(BigInteger left, BigInteger right) => BigInteger.Remainder(left, right);

                public static BigInteger Subtract(BigInteger left, BigInteger right) => BigInteger.Subtract(left, right);

                public static BigInteger Multiply(BigInteger left, BigInteger right) => BigInteger.Multiply(left, right);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"export function Abs(value) {
  return value < 0n ? -value : value;
}
export function Add(left, right) {
  return left + right;
}
export function Copy(value, sign) {
  return sign < 0n ? value < 0n ? value : -value : value < 0n ? -value : value;
}
export function Compare(left, right) {
  return left < right ? -1 : left > right ? 1 : 0;
}
export function CompareTo(left, right) {
  return left < right ? -1 : left > right ? 1 : 0;
}
export function Divide(left, right) {
  return left / right;
}
export function EqualsValue(left, right) {
  return left === right;
}
export function Max(left, right) {
  return left > right ? left : right;
}
export function Min(left, right) {
  return left < right ? left : right;
}
export function Even(value) {
  return value % 2n === 0n;
}
export function Negative(value) {
  return value < 0n;
}
export function Negate(value) {
  return -value;
}
export function Odd(value) {
  return value % 2n !== 0n;
}
export function Positive(value) {
  return value > 0n;
}
export function Remainder(left, right) {
  return left % right;
}
export function Subtract(left, right) {
  return left - right;
}
export function Multiply(left, right) {
  return left * right;
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithBigIntegerMagnitudeHelpers_UsesRuntimeImports()
    {
        var code = """
            using System.Numerics;

            public static class TestClass
            {
                public static BigInteger Max(BigInteger left, BigInteger right) => BigInteger.MaxMagnitude(left, right);

                public static BigInteger Min(BigInteger left, BigInteger right) => BigInteger.MinMagnitude(left, right);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _d305de2c64e85995, _fef56ccd17b22e88 } from ""System/Numerics/BigIntegerModule.js"";
export function Max(left, right) {
  return _d305de2c64e85995(left, right);
}
export function Min(left, right) {
  return _fef56ccd17b22e88(left, right);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithInt64MagnitudeHelpers_UsesRuntimeImports()
    {
        var code = """
            public static class TestClass
            {
                public static long Max(long left, long right) => long.MaxMagnitude(left, right);

                public static long Min(long left, long right) => long.MinMagnitude(left, right);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _9618dc0d855ee729, _bfad1ee52075b36e } from ""System/Int64Module.js"";
export function Max(left, right) {
  return _9618dc0d855ee729(left, right);
}
export function Min(left, right) {
  return _bfad1ee52075b36e(left, right);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithSingleMagnitudeHelpers_UsesRuntimeImports()
    {
        var code = """
            public static class TestClass
            {
                public static float Max(float left, float right) => float.MaxMagnitude(left, right);

                public static float MaxNumber(float left, float right) => float.MaxMagnitudeNumber(left, right);

                public static float Min(float left, float right) => float.MinMagnitude(left, right);

                public static float MinNumber(float left, float right) => float.MinMagnitudeNumber(left, right);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _4a2ec5d010e27cb1, _7c146ff0a50e958f, _b7b1d7781578b7e0, _e5a7b14f707c69f7 } from ""System/SingleModule.js"";
export function Max(left, right) {
  return _7c146ff0a50e958f(left, right);
}
export function MaxNumber(left, right) {
  return _b7b1d7781578b7e0(left, right);
}
export function Min(left, right) {
  return _e5a7b14f707c69f7(left, right);
}
export function MinNumber(left, right) {
  return _4a2ec5d010e27cb1(left, right);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithSingleMathIntrinsics_UsesDirectMathWithoutImports()
    {
        var code = """
            public static class TestClass
            {
                public static double Run(float value, float left, float right, float third)
                {
                    var log2 = float.Log2(value);
                    var expM1 = float.ExpM1(value);
                    var ceil = float.Ceiling(value);
                    var floor = float.Floor(value);
                    var round = float.Round(value);
                    var trunc = float.Truncate(value);
                    var atan2Pi = float.Atan2Pi(left, right);
                    var fused = float.FusedMultiplyAdd(left, right, third);
                    var ieee = float.Ieee754Remainder(left, right);
                    var lerp = float.Lerp(left, right, third);
                    var reciprocal = float.ReciprocalEstimate(value);
                    var acosh = float.Acosh(value);
                    var logBase = float.Log(left, right);
                    var clamp = float.Clamp(value, left, right);
                    var max = float.Max(left, right);
                    var abs = float.Abs(value);
                    var even = float.IsEvenInteger(value);
                    var integer = float.IsInteger(value);
                    var positive = float.IsPositive(value);
                    var real = float.IsRealNumber(value);
                    var pow = float.Pow(left, right);
                    var sqrt = float.Sqrt(value);
                    var acosPi = float.AcosPi(value);
                    var cosPi = float.CosPi(value);
                    var deg = float.DegreesToRadians(value);
                    var sin = float.Sin(value);
                    var tanPi = float.TanPi(value);

                    return log2 + expM1 + ceil + floor + round + trunc + atan2Pi + fused + ieee + lerp + reciprocal + acosh + logBase + clamp + max + abs
                        + (even ? 1 : 0) + (integer ? 1 : 0) + (positive ? 1 : 0) + (real ? 1 : 0)
                        + pow + sqrt + acosPi + cosPi + deg + sin + tanPi;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"export function Run(value, left, right, third) {
  let log2 = Math.log2(value);
  let expM1 = Math.exp(value) - 1;
  let ceil = Math.ceil(value);
  let floor = Math.floor(value);
  let round = Math.round(value);
  let trunc = Math.trunc(value);
  let atan2Pi = Math.atan2(left, right) / Math.PI;
  let fused = left * right + third;
  let ieee = left - right * Math.round(left / right);
  let lerp = left + (right - left) * third;
  let reciprocal = 1 / value;
  let acosh = Math.acosh(value);
  let logBase = Math.log(left) / Math.log(right);
  let clamp = Math.max(left, Math.min(value, right));
  let max = Math.max(left, right);
  let abs = Math.abs(value);
  let even = value % 2 === 0;
  let integer = Number.isInteger(value);
  let positive = value > 0 || Object.is(value, 0);
  let real = !isNaN(value) && value !== Infinity && value !== -Infinity;
  let pow = Math.pow(left, right);
  let sqrt = Math.sqrt(value);
  let acosPi = Math.acos(value) / Math.PI;
  let cosPi = Math.cos(value * Math.PI);
  let deg = value * Math.PI / 180;
  let sin = Math.sin(value);
  let tanPi = Math.tan(value * Math.PI);
  return log2 + expM1 + ceil + floor + round + trunc + atan2Pi + fused + ieee + lerp + reciprocal + acosh + logBase + clamp + max + abs + (even ? 1 : 0) + (integer ? 1 : 0) + (positive ? 1 : 0) + (real ? 1 : 0) + pow + sqrt + acosPi + cosPi + deg + sin + tanPi;
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithDoubleMathIntrinsics_UsesMathHostWithoutImports()
    {
        var code = """
            public static class TestClass
            {
                public static double Run(double value, double left, double right)
                {
                    var log2 = double.Log2(value);
                    var exp = double.Exp(value);
                    var max = double.Max(left, right);
                    var abs = double.Abs(value);
                    var pow = double.Pow(left, right);
                    var sqrt = double.Sqrt(value);

                    return log2 + exp + max + abs + pow + sqrt;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"export function Run(value, left, right) {
  let log2 = Math.log2(value);
  let exp = Math.exp(value);
  let max = Math.max(left, right);
  let abs = Math.abs(value);
  let pow = Math.pow(left, right);
  let sqrt = Math.sqrt(value);
  return log2 + exp + max + abs + pow + sqrt;
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithFloatingPointMaxMinNumber_UsesInlineNaNFallbackWithoutImports()
    {
        var code = """
            public static class TestClass
            {
                public static double Run(float fleft, float fright, double dleft, double dright)
                {
                    var fmax = float.MaxNumber(fleft, fright);
                    var fmin = float.MinNumber(fleft, fright);
                    var dmax = double.MaxNumber(dleft, dright);
                    var dmin = double.MinNumber(dleft, dright);

                    return fmax + fmin + dmax + dmin;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"export function Run(fleft, fright, dleft, dright) {
  let fmax = isNaN(fleft) ? fright : isNaN(fright) ? fleft : Math.max(fleft, fright);
  let fmin = isNaN(fleft) ? fright : isNaN(fright) ? fleft : Math.min(fleft, fright);
  let dmax = isNaN(dleft) ? dright : isNaN(dright) ? dleft : Math.max(dleft, dright);
  let dmin = isNaN(dleft) ? dright : isNaN(dright) ? dleft : Math.min(dleft, dright);
  return fmax + fmin + dmax + dmin;
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithFloatingPointSignAndPow2_UsesRuntimeImports()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static int FloatSign(float value) => float.Sign(value);

                public static bool FloatPow2(float value) => float.IsPow2(value);

                public static int DoubleSign(double value) => double.Sign(value);

                public static bool DoublePow2(double value) => double.IsPow2(value);

                public static int MathFloatSign(float value) => Math.Sign(value);

                public static int MathDoubleSign(double value) => Math.Sign(value);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _0f9f49a802919a8f, _eee146c74a9bc322 } from ""System/DoubleModule.js"";
import { _9a554cfca79bdc59, _c0668680ba7ef96e } from ""System/MathModule.js"";
import { _0dcf89ab5d6bd60c, _323a6b94e62b2729 } from ""System/SingleModule.js"";
export function FloatSign(value) {
  return _323a6b94e62b2729(value);
}
export function FloatPow2(value) {
  return _0dcf89ab5d6bd60c(value);
}
export function DoubleSign(value) {
  return _eee146c74a9bc322(value);
}
export function DoublePow2(value) {
  return _0f9f49a802919a8f(value);
}
export function MathFloatSign(value) {
  return _c0668680ba7ef96e(value);
}
export function MathDoubleSign(value) {
  return _9a554cfca79bdc59(value);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithFloatingPointNormalClassification_UsesInlineThresholdChecks()
    {
        var code = """
            public static class TestClass
            {
                public static bool FloatNormal(float value) => float.IsNormal(value);

                public static bool FloatSubnormal(float value) => float.IsSubnormal(value);

                public static bool DoubleNormal(double value) => double.IsNormal(value);

                public static bool DoubleSubnormal(double value) => double.IsSubnormal(value);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"export function FloatNormal(value) {
  return isFinite(value) && value !== 0 && Math.abs(value) >= 1.17549435e-38;
}
export function FloatSubnormal(value) {
  return isFinite(value) && value !== 0 && Math.abs(value) < 1.17549435e-38;
}
export function DoubleNormal(value) {
  return isFinite(value) && value !== 0 && Math.abs(value) >= 2.2250738585072014e-308;
}
export function DoubleSubnormal(value) {
  return isFinite(value) && value !== 0 && Math.abs(value) < 2.2250738585072014e-308;
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithSingleSinCos_UsesRuntimeImports()
    {
        var code = """
            public static class TestClass
            {
                public static (float Sin, float Cos) Pair(float value) => float.SinCos(value);

                public static (float SinPi, float CosPi) PairPi(float value) => float.SinCosPi(value);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _2c792a5d6ef88cd1, _9905e3952bca67bc } from ""System/SingleModule.js"";
export function Pair(value) {
  return _9905e3952bca67bc(value);
}
export function PairPi(value) {
  return _2c792a5d6ef88cd1(value);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithDoubleMagnitudeHelpers_UsesRuntimeImports()
    {
        var code = """
            public static class TestClass
            {
                public static double Max(double left, double right) => double.MaxMagnitude(left, right);

                public static double MaxNumber(double left, double right) => double.MaxMagnitudeNumber(left, right);

                public static double Min(double left, double right) => double.MinMagnitude(left, right);

                public static double MinNumber(double left, double right) => double.MinMagnitudeNumber(left, right);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _315c6cdfa11efcf2, _7f7b38b043f3f42f, _b6202851542d164c, _bb1daa880a2ad14e } from ""System/DoubleModule.js"";
export function Max(left, right) {
  return _b6202851542d164c(left, right);
}
export function MaxNumber(left, right) {
  return _7f7b38b043f3f42f(left, right);
}
export function Min(left, right) {
  return _bb1daa880a2ad14e(left, right);
}
export function MinNumber(left, right) {
  return _315c6cdfa11efcf2(left, right);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithInt16MagnitudeHelpers_UsesRuntimeImports()
    {
        var code = """
            public static class TestClass
            {
                public static short Max(short left, short right) => short.MaxMagnitude(left, right);

                public static short Min(short left, short right) => short.MinMagnitude(left, right);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { _63d3d54252a49e29, _ea75510d32bc8099 } from ""System/Int16Module.js"";
export function Max(left, right) {
  return _ea75510d32bc8099(left, right);
}
export function Min(left, right) {
  return _63d3d54252a49e29(left, right);
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
    public async Task Convert_ClassWithTopLevelSiblingSourceHelper_Throws()
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
                [ECMAScript.ECMAScriptModule("System/ConsumerModule.js")]
                public static class ConsumerModule
                {
                    public static int Create() => Helper.Make();
                }

                public static class Helper
                {
                    public static int Make() => 42;
                }
            }
            """;

        var (consumer, semanticModel) = CompileAndGetSymbol(code, "ConsumerModule");
        var converter = new AstConverter(consumer, semanticModel);

        // Act & Assert
        await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
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
    public async Task Convert_ClassWithCrossModuleStaticPropertyAssignment_GeneratesSetterImport()
    {
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
                    public static int Value { get; set; }
                }

                [ECMAScript.ECMAScriptModule("System/ConsumerModule.js")]
                public static class ConsumerModule
                {
                    public static void Set() => RuntimeModule.Value = 7;
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

        Assert.AreEqual(
@"import { set_Value } from ""System/RuntimeModule.js"";
export function Set() {
  set_Value(7);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleStaticFieldAssignment_Throws()
    {
        await AssertCrossModuleStaticFieldMutationThrowsAsync(
            "public static int Value;",
            "RuntimeModule.Value = 7;");
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleStaticFieldAssignment_PrioritizesMutationGuard()
    {
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
                    public static int Value;
                }

                [ECMAScript.ECMAScriptModule("System/ConsumerModule.js")]
                public static class ConsumerModule
                {
                    public static void Set() => RuntimeModule.Value = Helper.Make();
                }

                public static class Helper
                {
                    public static int Make() => 42;
                }
            }
            """;

        var (consumer, semanticModel) = CompileAndGetSymbol(code, "ConsumerModule");
        var converter = new AstConverter(consumer, semanticModel);

        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "Cross-module static field mutation");
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleStaticFieldCompoundAssignment_Throws()
    {
        await AssertCrossModuleStaticFieldMutationThrowsAsync(
            "public static int Value;",
            "RuntimeModule.Value += 1;");
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleStaticFieldCoalesceAssignment_Throws()
    {
        await AssertCrossModuleStaticFieldMutationThrowsAsync(
            "public static string? Value;",
            "RuntimeModule.Value ??= \"fallback\";");
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleStaticFieldIncrement_Throws()
    {
        await AssertCrossModuleStaticFieldMutationThrowsAsync(
            "public static int Value;",
            "RuntimeModule.Value++;");
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleStaticFieldDeconstructionAssignment_Throws()
    {
        await AssertCrossModuleStaticFieldMutationThrowsAsync(
            "public static int Value;",
            "int local = 0; (RuntimeModule.Value, local) = (1, 2);");
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
    public async Task Convert_ClassWithCrossModuleImportsEncounteredOutOfOrder_EmitsSortedImportDeclarations()
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
                    public static int Create() => Right.Make() + Left.Make();
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

        Assert.IsNotNull(script);
        var importLines = script
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(static line => line.StartsWith("import ", StringComparison.Ordinal))
            .ToArray();
        Assert.AreEqual(2, importLines.Length);
        StringAssert.Contains(importLines[0], "from \"System/LeftModule.js\";");
        StringAssert.Contains(importLines[1], "from \"System/RightModule.js\";");

        static string ParseLocalBinding(string importLine)
        {
            var leftBrace = importLine.IndexOf('{');
            var rightBrace = importLine.IndexOf('}');
            var specifier = importLine[(leftBrace + 1)..rightBrace].Trim();
            if (specifier.StartsWith("Make as ", StringComparison.Ordinal))
                return specifier.Substring("Make as ".Length).Trim();

            return specifier;
        }

        var leftLocal = ParseLocalBinding(importLines[0]);
        var rightLocal = ParseLocalBinding(importLines[1]);
        StringAssert.Contains(script, $"return {rightLocal}() + {leftLocal}();");
    }

    [TestMethod]
    public async Task Convert_ClassWithEqualityComparerConcreteAndInterface_EmitsStableDedupedImports()
    {
        var code = """
            using System.Collections.Generic;

            public static class TestClass
            {
                public static bool EqualsConcrete(int left, int right)
                    => EqualityComparer<int>.Default.Equals(left, right);

                public static bool EqualsInterface(int left, int right)
                {
                    IEqualityComparer<int> comparer = EqualityComparer<int>.Default;
                    return comparer.Equals(left, right);
                }

                public static int HashConcrete(int value)
                    => EqualityComparer<int>.Default.GetHashCode(value);

                public static int HashInterface(int value)
                {
                    IEqualityComparer<int> comparer = EqualityComparer<int>.Default;
                    return comparer.GetHashCode(value);
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "globalThis.__jazorEqualityComparerDefault ??= {}");
        StringAssert.Contains(script, "System/Collections/Generic/EqualityComparerT1Module.js");
        StringAssert.Contains(script, "System/Collections/Generic/IEqualityComparerT1Module.js");

        var importLines = script
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(static line => line.StartsWith("import ", StringComparison.Ordinal))
            .ToArray();
        Assert.AreEqual(2, importLines.Length);

        var modulePaths = importLines
            .Select(static line =>
            {
                var fromStart = line.IndexOf("from \"", StringComparison.Ordinal);
                var pathStart = fromStart + 6;
                var pathEnd = line.LastIndexOf("\";", StringComparison.Ordinal);
                return line[pathStart..pathEnd];
            })
            .ToArray();
        CollectionAssert.AreEqual(
            new[]
            {
                "System/Collections/Generic/EqualityComparerT1Module.js",
                "System/Collections/Generic/IEqualityComparerT1Module.js"
            },
            modulePaths);

        foreach (var importLine in importLines)
        {
            var leftBrace = importLine.IndexOf('{');
            var rightBrace = importLine.IndexOf('}');
            Assert.IsTrue(leftBrace >= 0 && rightBrace > leftBrace, importLine);

            var specifiers = importLine[(leftBrace + 1)..rightBrace]
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Assert.AreEqual(specifiers.Length, specifiers.Distinct(StringComparer.Ordinal).Count());
        }
    }

    [TestMethod]
    public async Task Convert_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        var code = """
            public static class TestClass
            {
                public static int Sum(int left, int right) => left + right;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => converter.Convert(cts.Token));
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
