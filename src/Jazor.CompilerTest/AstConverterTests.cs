using System.Diagnostics;
using System.Threading;
using Acornima.Ast;
using Jazor.Compiler;
using Jazor.Common;
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

    private static void AssertContainsCount(string actual, string expected, int count)
    {
        var actualCount = actual.Split([expected], StringSplitOptions.None).Length - 1;
        Assert.AreEqual(count, actualCount, $"Expected '{expected}' to appear {count} time(s), but found {actualCount}.{Environment.NewLine}{actual}");
    }

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

    private static string ConstructorHelperName(INamedTypeSymbol containingType, params string[] parameterTypes)
    {
        var constructor = containingType.InstanceConstructors
            .Single(ctor =>
                !ctor.IsImplicitlyDeclared &&
                ctor.Parameters
                    .Select(static parameter => parameter.Type.ToDisplayString(Format.NameFormat))
                    .SequenceEqual(parameterTypes));
        return $"$ctor_{Format.HashName(constructor.OriginalDefinition.ToDisplayString(Format.NameFormat)).TrimStart('_')}";
    }

    private static bool ContainsVue3Reference(IEnumerable<MetadataReference> references)
        => references.Any(static reference => string.Equals(reference.Display, typeof(ECMAScript.Vue3).Assembly.Location, StringComparison.OrdinalIgnoreCase));

    private static MetadataReference[] BuildCompilationReferences(IEnumerable<MetadataReference>? additionalReferences = null)
    {
        var references = TestMetadataReferences.Net11.ToList();
        if (additionalReferences is not null)
            references.AddRange(additionalReferences);

        if (ContainsVue3Reference(references) &&
            !references.Any(static reference => string.Equals(reference.Display, typeof(ECMAScript.VueContract.VueLibraryComponentAttribute).Assembly.Location, StringComparison.OrdinalIgnoreCase)))
        {
            references.Add(MetadataReference.CreateFromFile(typeof(ECMAScript.VueContract.VueLibraryComponentAttribute).Assembly.Location));
        }

        if (ContainsVue3Reference(references) &&
            !references.Any(static reference => string.Equals(reference.Display, typeof(ECMAScript.Vue3.IVueComponent).Assembly.Location, StringComparison.OrdinalIgnoreCase)))
        {
            references.Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3.IVueComponent).Assembly.Location));
        }

        return references.ToArray();
    }

    private static SyntaxTree[] BuildSyntaxTrees(string code, IEnumerable<MetadataReference>? additionalReferences = null)
    {
        var syntaxTrees = new List<SyntaxTree>();
        if (additionalReferences is not null && ContainsVue3Reference(additionalReferences))
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(
                "global using ECMAScript.VueContract;",
                TestMetadataReferences.PreviewParseOptions,
                path: "__TestGlobalUsings.cs"));
        }

        syntaxTrees.Add(CSharpSyntaxTree.ParseText(code, TestMetadataReferences.PreviewParseOptions));
        return syntaxTrees.ToArray();
    }

    private static SyntaxTree[] BuildSyntaxTrees((string Path, string Text)[] sources, IEnumerable<MetadataReference>? additionalReferences = null)
    {
        var syntaxTrees = new List<SyntaxTree>();
        if (additionalReferences is not null && ContainsVue3Reference(additionalReferences))
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(
                "global using ECMAScript.VueContract;",
                TestMetadataReferences.PreviewParseOptions,
                path: "__TestGlobalUsings.cs"));
        }

        syntaxTrees.AddRange(sources.Select(static source => CSharpSyntaxTree.ParseText(
            source.Text,
            TestMetadataReferences.PreviewParseOptions,
            path: source.Path)));
        return syntaxTrees.ToArray();
    }

    private static (INamedTypeSymbol, SemanticModel) CompileAndGetSymbol(string code)
    {
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            BuildSyntaxTrees(code),
            BuildCompilationReferences(),
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
            BuildSyntaxTrees(code),
            BuildCompilationReferences(),
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

    private static (INamedTypeSymbol, SemanticModel) CompileAndGetSymbol(
        string code,
        string className,
        params MetadataReference[] additionalReferences)
    {
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            BuildSyntaxTrees(code, additionalReferences),
            BuildCompilationReferences(additionalReferences),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsFalse(diagnostics.Length > 0, string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var classDeclaration = syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(node => node.Identifier.Text == className);

            if (classDeclaration is null)
                continue;

            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            Assert.IsNotNull(classSymbol);
            return (classSymbol, semanticModel);
        }

        throw new InvalidOperationException($"Class '{className}' was not found.");
    }

    private static (INamedTypeSymbol, SemanticModel) CompileAndGetSymbol(
        (string Path, string Text)[] sources,
        string className,
        params MetadataReference[] additionalReferences)
    {
        var syntaxTrees = BuildSyntaxTrees(sources, additionalReferences);
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees,
            BuildCompilationReferences(additionalReferences),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsFalse(diagnostics.Length > 0, string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var classDeclaration = syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(node => node.Identifier.Text == className);
            if (classDeclaration is null)
                continue;

            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            Assert.IsNotNull(classSymbol);
            return (classSymbol, semanticModel);
        }

        throw new InvalidOperationException($"Class '{className}' was not found.");
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
@"export let field = 42;
export function method() { }
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
@"export let field = 42;
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
@"export const constField = 42;
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
@"let privateField = 42;
export let publicField = 24;
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
@"export function testMethod() {
  return 1;
}
", script);

    }

    [TestMethod]
    public async Task Convert_ClassMembersWithoutExplicitNameConfig_UseJsNamingFallback()
    {
        var code = """
            public static class TestClass
            {
                public static int PascalField = 1;

                public static int PascalMethod()
                    => PascalField;

                public sealed class NestedClass
                {
                    public int PascalValue = 2;

                    public int ReadValue()
                        => PascalValue;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export let pascalField = 1;
export function pascalMethod() {
  return pascalField;
}
export class NestedClass {
  pascalValue = 2;
  readValue() {
    return this.pascalValue;
  }
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
@"export async function testMethodAsync() {
  await Promise.resolve();
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithOptionalModuleFunctionParameters_GeneratesJavaScriptDefaults()
    {
        var code = """
            public static class TestClass
            {
                public enum ReleaseMode
                {
                    DryRun = 1
                }

                public static string CreateRelease(
                    string name = "release",
                    bool enabled = true,
                    int retryCount = 3,
                    ReleaseMode mode = ReleaseMode.DryRun,
                    string? note = null)
                    => name;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export function createRelease(name = ""release"", enabled = true, retryCount = 3, mode = 1, note = null) {
  return name;
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
  field = 0;
  constructor(value) {
    this.field = value;
  }
  double() {
    return this.field * 2;
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
  field = 0;
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public void ConvertRuntimeClass_StaticClass_GeneratesClassDeclarationWithStaticMembers()
    {
        var code = """
            public static class TestClass
            {
                public static class NestedHelpers
                {
                    public static string Label = "helper";

                    public static string Describe()
                    {
                        return Label;
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code, "NestedHelpers");
        var converter = new AstConverter(classSymbol, semanticModel);

        var declaration = converter.ConvertRuntimeClass(classSymbol);
        var script = declaration.ToKnRECMAScript();

Assert.AreEqual(
@"class NestedHelpers {
  static label = ""helper"";
  static describe() {
    return NestedHelpers.label;
  }
}".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
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
  value = 0;
  constructor() {{
    let $args = arguments;
    let $ctor = $args[0];
    if ($ctor === ""{ctor0}"") {{
      this.{ctor0}();
      return;
    }}
    if ($ctor === ""{ctor1}"") {{
      let value = $args[1];
      this.{ctor1}(value);
      return;
    }}
    throw new Error(""No matching constructor overload for NestedClass."");
  }}
  {ctor0}() {{ }}
  {ctor1}(value) {{
    this.value = value;
  }}
}}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassConstructorOverloadsWithSameArity_GeneratesSelectorDispatcher()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public string Value;

                    public NestedClass(int value)
                    {
                        Value = "int";
                    }

                    public NestedClass(string value)
                    {
                        Value = value;
                    }
                }

                public static NestedClass CreateInt()
                {
                    return new NestedClass(1);
                }

                public static NestedClass CreateString()
                {
                    return new NestedClass("text");
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var nestedClass = classSymbol.GetTypeMembers("NestedClass").Single();
        var ctorInt = ConstructorHelperName(nestedClass, "int");
        var ctorString = ConstructorHelperName(nestedClass, "string");
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

AssertScriptEqual(
$@"export class NestedClass {{
  value = null;
  constructor() {{
    let $args = arguments;
    let $ctor = $args[0];
    if ($ctor === ""{ctorInt}"") {{
      let value = $args[1];
      this.{ctorInt}(value);
      return;
    }}
    if ($ctor === ""{ctorString}"") {{
      let value = $args[1];
      this.{ctorString}(value);
      return;
    }}
    throw new Error(""No matching constructor overload for NestedClass."");
  }}
  {ctorInt}(value) {{
    this.value = ""int"";
  }}
  {ctorString}(value) {{
    this.value = value;
  }}
}}
export function createInt() {{
  return new NestedClass(""{ctorInt}"", 1);
}}
export function createString() {{
  return new NestedClass(""{ctorString}"", ""text"");
}}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassConstructorOverloadsWithOptionalParameterOverlap_GeneratesSelectorDefaults()
    {
        var code = """
            public static class TestClass
            {
                public class NestedClass
                {
                    public int Value;

                    public NestedClass(int value)
                    {
                        Value = value;
                    }

                    public NestedClass(int value, int increment = 1)
                    {
                        Value = value + increment;
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var nestedClass = classSymbol.GetTypeMembers("NestedClass").Single();
        var ctor1 = ConstructorHelperName(nestedClass, 1);
        var ctor2 = ConstructorHelperName(nestedClass, 2);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

AssertScriptEqual(
$@"export class NestedClass {{
  value = 0;
  constructor() {{
    let $args = arguments;
    let $ctor = $args[0];
    if ($ctor === ""{ctor1}"") {{
      let value = $args[1];
      this.{ctor1}(value);
      return;
    }}
    if ($ctor === ""{ctor2}"") {{
      let value = $args[1], increment = $args.length > 2 ? $args[2] : 1;
      this.{ctor2}(value, increment);
      return;
    }}
    throw new Error(""No matching constructor overload for NestedClass."");
  }}
  {ctor1}(value) {{
    this.value = value;
  }}
  {ctor2}(value, increment) {{
    this.value = value + increment;
  }}
}}
", script);
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
  field = 0;
  constructor(value) {
    this.field = value;
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
  field = 0;
  constructor(value) {{
    this.field = value;
  }}
}}
export class NestedClass extends BaseClass {{
  constructor() {{
    let $args = arguments;
    let $ctor = $args[0];
    if ($ctor === ""{ctor0}"") {{
      super(1);
      this.{ctor0}();
      return;
    }}
    if ($ctor === ""{ctor1}"") {{
      let value = $args[1];
      super(value + 1);
      this.{ctor1}(value);
      return;
    }}
    throw new Error(""No matching constructor overload for NestedClass."");
  }}
  {ctor0}() {{ }}
  {ctor1}(value) {{
    this.field = value * 2;
  }}
}}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedClassBaseConstructorOverloadsWithSameArity_GeneratesSuperSelector()
    {
        var code = """
            public static class TestClass
            {
                public class BaseClass
                {
                    public string Field;

                    public BaseClass(int value)
                    {
                        Field = "int";
                    }

                    public BaseClass(string value)
                    {
                        Field = value;
                    }
                }

                public class NestedClass : BaseClass
                {
                    public NestedClass() : base("text")
                    {
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var baseClass = classSymbol.GetTypeMembers("BaseClass").Single();
        var baseCtorInt = ConstructorHelperName(baseClass, "int");
        var baseCtorString = ConstructorHelperName(baseClass, "string");
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

AssertScriptEqual(
$@"export class BaseClass {{
  field = null;
  constructor() {{
    let $args = arguments;
    let $ctor = $args[0];
    if ($ctor === ""{baseCtorInt}"") {{
      let value = $args[1];
      this.{baseCtorInt}(value);
      return;
    }}
    if ($ctor === ""{baseCtorString}"") {{
      let value = $args[1];
      this.{baseCtorString}(value);
      return;
    }}
    throw new Error(""No matching constructor overload for BaseClass."");
  }}
  {baseCtorInt}(value) {{
    this.field = ""int"";
  }}
  {baseCtorString}(value) {{
    this.field = value;
  }}
}}
export class NestedClass extends BaseClass {{
  constructor() {{
    super(""{baseCtorString}"", ""text"");
  }}
}}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassWithImplicitDerivedConstructorAndBaseOverloads_GeneratesSuperSelector()
    {
        var code = """
            public static class TestClass
            {
                public class BaseClass
                {
                    public int Field;

                    public BaseClass()
                    {
                        Field = 1;
                    }

                    public BaseClass(int value)
                    {
                        Field = value;
                    }
                }

                public class NestedClass : BaseClass
                {
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var baseClass = classSymbol.GetTypeMembers("BaseClass").Single();
        var baseCtor0 = ConstructorHelperName(baseClass, 0);
        var baseCtor1 = ConstructorHelperName(baseClass, 1);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

AssertScriptEqual(
$@"export class BaseClass {{
  field = 0;
  constructor() {{
    let $args = arguments;
    let $ctor = $args[0];
    if ($ctor === ""{baseCtor0}"") {{
      this.{baseCtor0}();
      return;
    }}
    if ($ctor === ""{baseCtor1}"") {{
      let value = $args[1];
      this.{baseCtor1}(value);
      return;
    }}
    throw new Error(""No matching constructor overload for BaseClass."");
  }}
  {baseCtor0}() {{
    this.field = 1;
  }}
  {baseCtor1}(value) {{
    this.field = value;
  }}
}}
export class NestedClass extends BaseClass {{
  constructor() {{
    super(""{baseCtor0}"");
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
  value() {
    return 1;
  }
}
export class NestedClass extends BaseClass {
  constructor() {
    super();
  }
  value() {
    return super.value() + 1;
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
  #{backingFieldName} = 0;
  get value() {{
    return this.#{backingFieldName};
  }}
  set value(value) {{
    this.#{backingFieldName} = value;
  }}
}}
export class NestedClass extends BaseClass {{
  constructor() {{
    super();
  }}
  read() {{
    return super.value;
  }}
  write(value) {{
    super.value = value + 1;
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
  value(value) {
    return value + 1;
  }
}
export class NestedClass extends BaseClass {
  constructor() {
    super();
  }
  get() {
    return value => super.value(value);
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
  value = 42;
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
  get value() {{
    return this.#{backingFieldName};
  }}
  set value(value) {{
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
  get value() {{
    return this.#{backingFieldName};
  }}
}}
", script);
    }

    [TestMethod]
    public async Task Convert_InternalModuleNestedGetterOnlyPropertyConstructorAssignment_WritesBackingField()
    {
        var code = """
            using ECMAScript;

            [ECMAScriptModule("components/helper")]
            internal static class Helper
            {
                public sealed class Item
                {
                    public Item(string value)
                    {
                        Value = value;
                    }

                    public string Value { get; }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "Helper",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var nestedClass = classSymbol.GetTypeMembers("Item").Single();
        var backingFieldName = PropertyBackingFieldName(nestedClass, "Value");
        var converter = new AstConverter(classSymbol, semanticModel, new AstConverterOptions(AstConverterProfile.ClrRuntime));

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
$@"export class Item {{
  constructor(value) {{
    this.#{backingFieldName} = value;
  }}
  #{backingFieldName} = null;
  get value() {{
    return this.#{backingFieldName};
  }}
}}
", script);
    }

    [TestMethod]
    public async Task Convert_InternalModuleNestedManualInitPropertyConstructorAssignment_UsesSetter()
    {
        var code = """
            using ECMAScript;

            [ECMAScriptModule("components/helper")]
            internal static class Helper
            {
                public sealed class Item
                {
                    public Item(string value)
                    {
                        Value = value;
                    }

                    public string Value
                    {
                        get => "ready";
                        init => Observe(value);
                    }

                    private void Observe(string value) { }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "Helper",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel, new AstConverterOptions(AstConverterProfile.ClrRuntime));

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "constructor(value) {\n    this.value = value;\n  }", StringComparison.Ordinal);
        StringAssert.Contains(script, "set value(value) {\n    this.observe(value);\n  }", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("this.#", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public async Task Convert_InternalModuleNestedBlockInitPropertyConstructorAssignment_UsesSetterBody()
    {
        var code = """
            using ECMAScript;

            [ECMAScriptModule("components/helper")]
            internal static class Helper
            {
                public sealed class Item
                {
                    public Item(string value)
                    {
                        Value = value;
                    }

                    public string Value
                    {
                        get => "ready";
                        init
                        {
                            Observe(value.Trim());
                        }
                    }

                    private void Observe(string value) { }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "Helper",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel, new AstConverterOptions(AstConverterProfile.ClrRuntime));

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "constructor(value) {\n    this.value = value;\n  }", StringComparison.Ordinal);
        StringAssert.Contains(script, "set value(value) {\n    this.observe(value.trim());\n  }", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("this.#", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public async Task Convert_InternalModuleNestedFieldBackedInitPropertyConstructorAssignment_InvokesSetter()
    {
        var code = """
            using ECMAScript;

            [ECMAScriptModule("components/helper")]
            internal static class Helper
            {
                public sealed class Item
                {
                    public Item(string value)
                    {
                        Value = value;
                    }

                    public string Value
                    {
                        get => field;
                        init => field = value.Trim();
                    }
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "Helper",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var itemSymbol = classSymbol.GetTypeMembers("Item").Single();
        var backingFieldName = PropertyBackingFieldName(itemSymbol, "Value");
        var converter = new AstConverter(classSymbol, semanticModel, new AstConverterOptions(AstConverterProfile.ClrRuntime));

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "constructor(value) {\n    this.value = value;\n  }", StringComparison.Ordinal);
        StringAssert.Contains(script, $"get value() {{\n    return this.#{backingFieldName};\n  }}", StringComparison.Ordinal);
        StringAssert.Contains(script, $"set value(value) {{\n    this.#{backingFieldName} = value.trim();\n  }}", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task Convert_InternalModuleNestedPrivateFields_UsePrivateNamesForDeclarationsAndReferences()
    {
        var code = """
            using ECMAScript;

            [ECMAScriptModule("components/counter")]
            internal static class CounterModule
            {
                public sealed class Counter
                {
                    private int _value;
                    private static int _total;

                    public Counter(int value)
                    {
                        _value = value;
                        _total += value;
                    }

                    public int Read() => _value + _total;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "CounterModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel, new AstConverterOptions(AstConverterProfile.ClrRuntime));

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "#_value = 0;", StringComparison.Ordinal);
        StringAssert.Contains(script, "static #_total = 0;", StringComparison.Ordinal);
        StringAssert.Contains(script, "this.#_value = value;", StringComparison.Ordinal);
        StringAssert.Contains(script, "Counter.#_total += value;", StringComparison.Ordinal);
        StringAssert.Contains(script, "return this.#_value + Counter.#_total;", StringComparison.Ordinal);
        _ = new Acornima.Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_ModulePolicyComputedPropertyGetter_UsesConfiguredRuntimeName()
    {
        var code = """
            public static class Counter
            {
                private static bool IsReady => true;

                public static void Build()
                {
                    var ready = IsReady;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(
            classSymbol,
            semanticModel,
            new AstConverterOptions(
                AstConverterProfile.Standard,
                ModulePolicy: ConfiguredPropertyGetterModulePolicy.Instance));

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "function isReady()", StringComparison.Ordinal);
        StringAssert.Contains(script, "let ready = isReady();", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("function get_IsReady()", StringComparison.Ordinal), script);
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
  static get value() {{
    return this.#{backingFieldName};
  }}
  static set value(value) {{
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
  static get value() {{
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
  get value() {
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
  square(x) {
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
  async loadAsync() {
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
  get value() {
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
  get value() {
    return this.current;
  }
  set value(value) {
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
  value = 0;
  constructor() {
    this.value = 1;
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
@"export let field1 = 1;
export let field2 = 2;
export let field3 = 3;
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
@"export let readOnlyField = 42;
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
@"export let internalField = 42;
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
@"export let stringField = ""hello"";
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
@"export let boolField = true;
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
@"export let doubleField = 3.14;
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
@"export function voidMethod() { }
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
@"export function add(a, b) {
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
@"export function greet(name) {
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
@"function privateMethod() { }
export function publicMethod() { }
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
@"export function internalMethod() { }
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
@"let A = 1;
export { A as a };
let B = ""456"";
export { B as b };
export const c = 42;
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
  return get_P1();
}
export function set_P3(value) { }
export function get_P4() {
  return get_P1();
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
  let __cacc$7089c50d965b6db792c99228;
  _aa3181446f60dc6e = (__cacc$7089c50d965b6db792c99228 = value, __cacc$7089c50d965b6db792c99228 == null ? undefined : __cacc$7089c50d965b6db792c99228.trim());
}
export function get_P8() {
  return B;
}
export function set_P8(value) {
  B = value.trim();
}
export function method_a604b94929b691c0() { }
export function method_d389d2b826e42edb(a) { }
export function method_04bbed0f7a07bb40(a, b) {
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
@"export function identity(value) {
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
@"export let numbers = [];
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
@"export let nullableField = null;
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
@"export const missing = null;
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
@"export let arrayField = [1, 2, 3];
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
@"export let listField = [];
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
@"export let dictField = new Map;
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
export function max(a, b) {
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
@"export function double(value) {
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
@"export function sum(values) {
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
@"export function increment(value) {
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
@"export function incrementAndReturn(value) {
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
@"export function normalize(value) {
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
@"export let longField = 9223372036854775807n;
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
@"export let uLongField = 18446744073709551615n;
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
@"export let maxDouble = Number.MAX_VALUE;
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
@"export let decimalField = 123.456;
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
@"export let specialString = ""Hello\nWorld\t!"";
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
@"export let emptyString = """";
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
@"export let unicodeString = ""你好世界🌍"";
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
@"export let quoteString = ""He said \""Hello\"""";
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
@"export let charField = ""A"";
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
@"export let className = ""TestClass"";
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
export let value = _bfa8ee5dd46e2005();
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
export let value = _12b4f3f1dc14bea9();
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
export let value = _5af0f6ad850e6702();
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
export let value = _5f8053a9657a0844();
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
export let value = _9f78f92d0753f4cf();
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
@"export let value = 0n;
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
@"export let value = 0;
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
@"export let value = 0;
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
@"export let value = 0n;
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
@"export let value = ""\0"";
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
@"export const value = 1;
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
@"export const value = 9007199254740993n;
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
@"export function add(a, b = 10) {
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
@"export function greet(name = ""World"", age = 0) {
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
@"export function process(name = null) { }
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
@"export function check(value = 1) {
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
@"export function check(value = 1.5) {
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
@"export function doWork_7bf2b889f48863c7() { }
export function doWork_6b6f7943743f9c5d(value) { }
export function doWork_53280513e48ce038(value) { }
export function doWork_90a9f2ec5e6402a1(a, b) {
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
@"export function square(x) {
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
export function increment() {
  _counter++;
}
", script);

    }

    [TestMethod]
    public async Task Convert_ClassWithExpressionBodiedThrow_PreservesThrowStatement()
    {
        var code = """
            public static class TestClass
            {
                public static int Fail(string message) => throw new System.Exception(message);
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var module = await new AstConverter(classSymbol, semanticModel).Convert();

        Assert.IsNotNull(module);
        var export = module.Body.OfType<ExportNamedDeclaration>().Single();
        var function = Assert.IsInstanceOfType<FunctionDeclaration>(export.Declaration);
        Assert.HasCount(1, function.Body.Body);
        Assert.IsInstanceOfType<ThrowStatement>(function.Body.Body[0]);

        var script = module.ToKnRECMAScript();
        StringAssert.Contains(script, "throw new Error(message);", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("return throw", StringComparison.Ordinal));
        _ = new Acornima.Parser().ParseModule(script);
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
        StringAssert.Contains(script, "export function parseOrZero(input) {");
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
        StringAssert.Contains(script, "export let isOrigin = (() => {");
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
@"export let nestedGenerics = new Map;
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
@"export function fibonacci(n) {
  if (n <= 1)
    return n;
  return fibonacci(n - 1) + fibonacci(n - 2);
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
export let value = _155212572c9a3297(""33"");
export function logValue() {
  return _fb5a811e7a32a324(_155212572c9a3297(""44""));
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithArgumentNullExceptionThrowIfNull_ImportsNullGuard()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static void Check(object? value)
                {
                    ArgumentNullException.ThrowIfNull(value, nameof(value));
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "import { _c80ae10aa1d0d795 } from \"System/ExceptionModule.js\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "_c80ae10aa1d0d795(value, \"value\");", StringComparison.Ordinal);
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
        StringAssert.Contains(script, "throw new TypeError(\"source\");");
        StringAssert.Contains(script, "throw new TypeError(\"predicate\");");
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
        StringAssert.Contains(script, "throw new TypeError(\"source\");");
        StringAssert.Contains(script, "throw new TypeError(\"selector\");");
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
        StringAssert.Contains(script, "throw new TypeError(\"source\");");
        StringAssert.Contains(script, "throw new TypeError(\"predicate\");");
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
        StringAssert.Contains(script, "throw new TypeError(\"source\");");
        StringAssert.Contains(script, "throw new TypeError(\"selector\");");
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
        StringAssert.Contains(script, "throw new TypeError(\"source\");");
        StringAssert.Contains(script, "throw new TypeError(\"predicate\");");
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
        StringAssert.Contains(script, "throw new TypeError(\"source\");");
        StringAssert.Contains(script, "throw new TypeError(\"selector\");");
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
export function format() {
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
export function format() {
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
export function format() {
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
export function format() {
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
export function format() {
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
export function format() {
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
export function format() {
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
export function format() {
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
@"export function floatCopy(value, sign) {
  return sign < 0 || Object.is(sign, -0) ? -Math.abs(value) : Math.abs(value);
}
export function intCopy(value, sign) {
  return sign < 0 ? -Math.abs(value) : Math.abs(value);
}
export function longCopy(value, sign) {
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
@"export function signedCopy(value, sign) {
  return sign < 0 ? -Math.abs(value) : Math.abs(value);
}
export function signedClamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}
export function signedMeta(value) {
  return (value > 0 ? 1 : value < 0 ? -1 : 0) + Math.abs(value) + Math.floor(Math.log2(value)) + Math.clz32(value) + (value === 0 ? 32 : 31 - Math.clz32(value & -value)) + ((value & 1) === 0 ? 1 : 0) + (value < 0 ? 1 : 0) + ((value & 1) !== 0 ? 1 : 0) + (value > 0 ? 1 : 0) + (value > 0 && (value & value - 1) === 0 ? 1 : 0);
}
export function signedBounds(left, right) {
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
@"export function sum(b, i, s, dt) {
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
@"export function signedCopy(value, sign) {
  return sign < 0 ? -Math.abs(value) : Math.abs(value);
}
export function signedClamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}
export function signedMeta(value) {
  return (value > 0 ? 1 : value < 0 ? -1 : 0) + Math.abs(value) + Math.floor(Math.log2(value)) + (value === 0 ? 16 : Math.clz32(value & 0xFFFF) - 16) + (value === 0 ? 16 : Math.floor(Math.log2(value & 0xFFFF & -(value & 0xFFFF)))) + ((((value & 0xFFFF) << (3 & 15) | (value & 0xFFFF) >>> 16 - (3 & 15)) & 0xFFFF) << 16 >> 16) + ((((value & 0xFFFF) >>> (5 & 15) | (value & 0xFFFF) << 16 - (5 & 15)) & 0xFFFF) << 16 >> 16) + ((value & 1) === 0 ? 1 : 0) + (value < 0 ? 1 : 0) + ((value & 1) !== 0 ? 1 : 0) + (value > 0 ? 1 : 0) + (value > 0 && (value & value - 1) === 0 ? 1 : 0);
}
export function signedBounds(left, right) {
  return Math.max(left, right) - Math.min(left, right);
}
export function unsignedClamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}
export function unsignedMeta(value) {
  return (value === 0 ? 0 : 1) + Math.floor(Math.log2(value)) + (value === 0 ? 16 : Math.clz32(value & 0xFFFF) - 16) + (value === 0 ? 16 : Math.floor(Math.log2(value & 0xFFFF & -(value & 0xFFFF)))) + ((value << (3 & 15) | value >>> 16 - (3 & 15)) & 0xFFFF) + ((value >>> (5 & 15) | value << 16 - (5 & 15)) & 0xFFFF) + ((value & 1) === 0 ? 1 : 0) + ((value & 1) !== 0 ? 1 : 0) + (value > 0 && (value & value - 1) === 0 ? 1 : 0);
}
export function unsignedBounds(left, right) {
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
@"export function unsignedClamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}
export function unsignedMeta(value) {
  return (value === 0 ? 0 : 1) + Math.floor(Math.log2(value)) + ((value & 1) === 0 ? 1 : 0) + ((value & 1) !== 0 ? 1 : 0) + (value > 0 && (value & value - 1) === 0 ? 1 : 0);
}
export function unsignedCounts(value) {
  return (value === 0 ? 8 : Math.clz32(value & 0xFF) - 24) + ((value & 1) + (value >> 1 & 1) + (value >> 2 & 1) + (value >> 3 & 1) + (value >> 4 & 1) + (value >> 5 & 1) + (value >> 6 & 1) + (value >> 7 & 1)) + (value === 0 ? 8 : Math.floor(Math.log2(value & 0xFF & -(value & 0xFF))));
}
export function unsignedBounds(left, right) {
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
export function sum(ushortLeft, ushortRight, uintLeft, uintRight) {
  let ushortPair = _80e78c0aa0b98fef(ushortLeft, ushortRight);
  let uintPair = _8a073d758132b5bb(uintLeft, uintRight);
  return ushortPair.quotient + ushortPair.remainder + uintPair.quotient + uintPair.remainder;
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
export function sum(left, right, value) {
  let pair = _b2c1f15fae072110(left, right);
  return pair.quotient + pair.remainder + _1636c956519f95fa(value);
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
export function sum(ushortValue, uintValue) {
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
@"export function signedCopy(value, sign) {
  return sign < 0 ? -Math.abs(value) : Math.abs(value);
}
export function signedClamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}
export function signedMeta(value) {
  return (value > 0 ? 1 : value < 0 ? -1 : 0) + Math.abs(value) + Math.floor(Math.log2(value)) + ((value & 1) === 0 ? 1 : 0) + (value < 0 ? 1 : 0) + ((value & 1) !== 0 ? 1 : 0) + (value > 0 ? 1 : 0) + (value > 0 && (value & value - 1) === 0 ? 1 : 0);
}
export function signedBounds(left, right) {
  return Math.max(left, right) - Math.min(left, right);
}
export function unsignedClamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}
export function unsignedMeta(value) {
  return BigInt(value === 0 ? 0 : 1) + BigInt(Math.floor(Math.log2(value))) + BigInt(Math.clz32(value)) + BigInt(value === 0 ? 32 : 31 - Math.clz32(value >>> 0 & -(value >>> 0))) + BigInt((value << (3 & 31) | value >>> 32 - (3 & 31)) >>> 0) + BigInt((value >>> (5 & 31) | value << 32 - (5 & 31)) >>> 0) + BigInt((value & 1) === 0 ? 1 : 0) + BigInt((value & 1) !== 0 ? 1 : 0) + BigInt(value > 0 && (value & value - 1) === 0 ? 1 : 0);
}
export function unsignedBounds(left, right) {
  return Math.max(left, right) - Math.min(left, right);
}
export function unsignedLongClamp(value, min, max) {
  return value < min ? min : value > max ? max : value;
}
export function unsignedLongSign(value) {
  return value === 0n ? 0 : 1;
}
export function unsignedLongEven(value) {
  return value % 2n === 0n;
}
export function unsignedLongOdd(value) {
  return value % 2n !== 0n;
}
export function unsignedLongPow2(value) {
  return value > 0n && (value & value - 1n) === 0n;
}
export function unsignedLongLog2(value) {
  return value === 0n ? 0n : BigInt(value.toString(2).length - 1);
}
export function unsignedLongMax(left, right) {
  return left > right ? left : right;
}
export function unsignedLongMin(left, right) {
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
@"export function longClamp(value, min, max) {
  return value < min ? min : value > max ? max : value;
}
export function longSign(value) {
  return value > 0n ? 1 : value < 0n ? -1 : 0;
}
export function mathLongClamp(value, min, max) {
  return value < min ? min : value > max ? max : value;
}
export function mathLongSign(value) {
  return value > 0n ? 1 : value < 0n ? -1 : 0;
}
export function mathLongMax(left, right) {
  return left > right ? left : right;
}
export function mathLongMin(left, right) {
  return left < right ? left : right;
}
export function mathUnsignedLongClamp(value, min, max) {
  return value < min ? min : value > max ? max : value;
}
export function mathUnsignedLongMax(left, right) {
  return left > right ? left : right;
}
export function mathUnsignedLongMin(left, right) {
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
@"export function abs(value) {
  return value < 0n ? -value : value;
}
export function add(left, right) {
  return left + right;
}
export function copy(value, sign) {
  return sign < 0n ? value < 0n ? value : -value : value < 0n ? -value : value;
}
export function compare(left, right) {
  return left < right ? -1 : left > right ? 1 : 0;
}
export function compareTo(left, right) {
  return left < right ? -1 : left > right ? 1 : 0;
}
export function divide(left, right) {
  return left / right;
}
export function equalsValue(left, right) {
  return left === right;
}
export function max(left, right) {
  return left > right ? left : right;
}
export function min(left, right) {
  return left < right ? left : right;
}
export function even(value) {
  return value % 2n === 0n;
}
export function negative(value) {
  return value < 0n;
}
export function negate(value) {
  return -value;
}
export function odd(value) {
  return value % 2n !== 0n;
}
export function positive(value) {
  return value > 0n;
}
export function remainder(left, right) {
  return left % right;
}
export function subtract(left, right) {
  return left - right;
}
export function multiply(left, right) {
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
export function max(left, right) {
  return _d305de2c64e85995(left, right);
}
export function min(left, right) {
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
export function max(left, right) {
  return _9618dc0d855ee729(left, right);
}
export function min(left, right) {
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
export function max(left, right) {
  return _7c146ff0a50e958f(left, right);
}
export function maxNumber(left, right) {
  return _b7b1d7781578b7e0(left, right);
}
export function min(left, right) {
  return _e5a7b14f707c69f7(left, right);
}
export function minNumber(left, right) {
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
@"export function run(value, left, right, third) {
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
@"export function run(value, left, right) {
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
@"export function run(fleft, fright, dleft, dright) {
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
export function floatSign(value) {
  return _323a6b94e62b2729(value);
}
export function floatPow2(value) {
  return _0dcf89ab5d6bd60c(value);
}
export function doubleSign(value) {
  return _eee146c74a9bc322(value);
}
export function doublePow2(value) {
  return _0f9f49a802919a8f(value);
}
export function mathFloatSign(value) {
  return _c0668680ba7ef96e(value);
}
export function mathDoubleSign(value) {
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
@"export function floatNormal(value) {
  return isFinite(value) && value !== 0 && Math.abs(value) >= 1.17549435e-38;
}
export function floatSubnormal(value) {
  return isFinite(value) && value !== 0 && Math.abs(value) < 1.17549435e-38;
}
export function doubleNormal(value) {
  return isFinite(value) && value !== 0 && Math.abs(value) >= 2.2250738585072014e-308;
}
export function doubleSubnormal(value) {
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
export function pair(value) {
  return _9905e3952bca67bc(value);
}
export function pairPi(value) {
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
export function max(left, right) {
  return _b6202851542d164c(left, right);
}
export function maxNumber(left, right) {
  return _7f7b38b043f3f42f(left, right);
}
export function min(left, right) {
  return _bb1daa880a2ad14e(left, right);
}
export function minNumber(left, right) {
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
export function max(left, right) {
  return _ea75510d32bc8099(left, right);
}
export function min(left, right) {
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
@"import { make } from ""System/RuntimeModule.js"";
export function create() {
  return make();
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingEcmaScriptVueProxy_GeneratesVueImportsFromNameAttributes()
    {
        var code = """
            using System;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static VueComponentPublicInstance Mount(IVueComponent component)
                    {
                        var app = Vue3.CreateApp(component);
                        return app.Mount("#app");
                    }

                    public static int ReadRef()
                    {
                        var count = Vue3.Ref(1);
                        return count.Value;
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { createApp, ref } from ""npm:vue@3"";
export function mount(component) {
  let app = createApp(component);
  return app.mount(""#app"");
}
export function readRef() {
  let count = ref(1);
  return count.value;
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_PartialEcmaScriptModuleClassAcrossSyntaxTrees_UsesCorrectSemanticModelPerDeclaration()
    {
        var sources = new[]
        {
            ("AppModule.Entry.cs", """
                using System;
                using ECMAScript;

                namespace Demo
                {
                    [ECMAScriptModule("app/main.mjs")]
                    public static partial class AppModule
                    {
                        public static int Read() => Sum(1, 2);
                    }
                }
                """),
            ("AppModule.Math.cs", """
                namespace Demo
                {
                    public static partial class AppModule
                    {
                        public static int Sum(int a, int b)
                        {
                            return a + b;
                        }
                    }
                }
                """)
        };

        var (_, semanticModel) = CompileAndGetSymbol(
            sources,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "export function read()");
        StringAssert.Contains(script, "return sum(1, 2);");
        StringAssert.Contains(script, "export function sum(a, b)");
        StringAssert.Contains(script, "return a + b;");
    }

    [TestMethod]
    public async Task Convert_ClassUsingDerivedVueProps_GeneratesObjectLiteral()
    {
        var code = """
            using ECMAScript;
            using System;
            using System.ComponentModel;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record RootProps : VueProps
                {
                    [Description("@#message")]
                    public string? Message { get; init; }
                }

                [ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static VueApp Boot(IVueComponent component)
                    {
                        return Vue3.CreateApp(component, new RootProps { Message = "Hello" });
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { createApp } from ""npm:vue@3"";
export function boot(component) {
  return createApp(component, { message: ""Hello"" });
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueObjectRootProps_FlattensIntoCreateAppArgument()
    {
        var code = """
            using ECMAScript;
            using ECMAScript.Contract;
            using System.ComponentModel;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record RootProps : VueProps
                {
                    [Description("@#message")]
                    public string? Message { get; init; }
                }

                [ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static VueApp Boot(IVueComponent component)
                    {
                        return Vue3.CreateApp(component, new VueObject<RootProps>
                        {
                            Props = new RootProps { Message = "Hello" }
                        });
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { createApp } from ""npm:vue@3"";
export function boot(component) {
  return createApp(component, { message: ""Hello"" });
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueObjectElementProps_FlattensCommonMembersAndBags()
    {
        var code = """
            using ECMAScript.Contract;
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record StyleBag : VueProps
                {
                    [Description("@#color")]
                    public string? Color { get; init; }
                }

                public sealed record AttrBag : VueProps
                {
                    [Description("@#.name")]
                    public string? NameSelector { get; init; }
                }

                public sealed record DatasetBag : VueProps
                {
                    [Description("@#data-user-id")]
                    public string? UserId { get; init; }
                }

                public sealed record RawBag : VueProps
                {
                    [Description("@#^width")]
                    public string? Width { get; init; }
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVNode Render()
                        => H("div", new VueObject
                        {
                            Key = 42,
                            Ref = "panel",
                            Id = null,
                            Title = "hero",
                            Class = new VueValue[] { "foo", new VueDictionary { ["bar"] = true } },
                            Style = new StyleBag { Color = "red" },
                            Attrs = new AttrBag { NameSelector = "some-name" },
                            Dataset = new DatasetBag { UserId = "42" },
                            Raw = new RawBag { Width = "100" },
                            ["role"] = "banner"
                        });
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { h } from ""npm:vue@3"";
export function render() {
  return h(""div"", {
    key: 42,
    ref: ""panel"",
    title: ""hero"",
    class: [""foo"", { bar: true }],
    style: { color: ""red"" },
    "".name"": ""some-name"",
    ""data-user-id"": ""42"",
    ""^width"": ""100"",
    role: ""banner""
  });
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueObjectDictionarySurface_FlattensIntoObjectLiteralMembers()
    {
        var code = """
            using ECMAScript.Contract;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVNode Render()
                        => H("div", new VueObject
                        {
                            Style = new VueDictionary
                            {
                                ["color"] = "red",
                                ["width"] = "100px",
                                ["skip"] = null
                            },
                            Class = new VueValue[]
                            {
                                "foo",
                                new VueDictionary { ["bar"] = true }
                            },
                            Attrs = new VueDictionary
                            {
                                [".name"] = "some-name"
                            },
                            Dataset = new VueDictionary
                            {
                                ["data-user-id"] = "42"
                            },
                            Raw = new VueDictionary
                            {
                                ["^width"] = "100"
                            }
                        });
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { h } from ""npm:vue@3"";
export function render() {
  return h(""div"", {
    style: { color: ""red"", width: ""100px"" },
    class: [""foo"", { bar: true }],
    "".name"": ""some-name"",
    ""data-user-id"": ""42"",
    ""^width"": ""100""
  });
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueObjectSpreadOrderingAndStaticNullOmission_PreservesObservableOrder()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record BaseProps : VueProps
                {
                    [Description("@#title")]
                    public string? Title { get; init; }

                    [Description("@#count")]
                    public int? Count { get; init; }
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVNode Render(VueProps raw)
                        => H("div", new VueObject<BaseProps>
                        {
                            Id = "before",
                            Props = new BaseProps
                            {
                                Title = "from-props",
                                Count = 1
                            },
                            Title = "from-local",
                            Attrs = new VueDictionary
                            {
                                ["title"] = "from-attrs",
                                ["data-a"] = "a"
                            },
                            Dataset = null,
                            Raw = raw,
                            ["title"] = "from-indexer"
                        });
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { h } from ""npm:vue@3"";
export function render(raw) {
  return h(""div"", {
    id: ""before"",
    title: ""from-props"",
    count: 1,
    title: ""from-local"",
    title: ""from-attrs"",
    ""data-a"": ""a"",
    ...raw,
    title: ""from-indexer""
  });
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueObjectWithDynamicIndexerKey_ThrowsOperationTransformationException()
    {
        var code = """
            using ECMAScript.Contract;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVNode Render()
                    {
                        var key = "role";
                        return H("div", new VueObject
                        {
                            [key] = "banner"
                        });
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "unsupported dynamic object key");
        StringAssert.Contains(exception.Message, "ECMAScript.Vue3.VueObject");
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueDictionaryWithDynamicIndexerKey_ThrowsOperationTransformationException()
    {
        var code = """
            using ECMAScript.Contract;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVNode Render()
                    {
                        var key = "role";
                        return H("div", new VueObject
                        {
                            Attrs = new VueDictionary
                            {
                                [key] = "banner"
                            }
                        });
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "unsupported dynamic object key");
        StringAssert.Contains(exception.Message, "ECMAScript.Vue3.VueDictionary");
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueObjectEventHandlers_FlattensEventListeners()
    {
        var code = """
            using ECMAScript.Contract;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVNode Render()
                        => H("button", new VueObject
                        {
                            Events = new VueEventHandlers
                            {
                                ["onClick"] = OnClick,
                                ["onFocus"] = Vue3.WithModifiers(OnFocus, "stop")
                            }
                        }, "Save");

                    public static IVNode RenderTracked()
                        => H("button", new VueObject
                        {
                            Events = new VueEventHandlers<MouseEvent>
                            {
                                ["onMousemove"] = OnMouseMove
                            }
                        }, "Move");

                    private static void OnClick()
                    {
                    }

                    private static void OnFocus()
                    {
                    }

                    private static void OnMouseMove(MouseEvent mouseEvent)
                    {
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { h, withModifiers } from ""npm:vue@3"";
export function render() {
  return h(""button"", { onClick: onClick, onFocus: withModifiers(onFocus, [""stop""]) }, ""Save"");
}
export function renderTracked() {
  return h(""button"", { onMousemove: onMouseMove }, ""Move"");
}
function onClick() { }
function onFocus() { }
function onMouseMove(mouseEvent) { }
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueObjectHtmlConvenienceMembers_GeneratesFinalAttributeKeys()
    {
        var code = """
            using ECMAScript.Contract;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVNode RenderLabel()
                        => H("label", new VueObject
                        {
                            For = "cs-input",
                            Class = "editor-label"
                        }, "C# Input");

                    public static IVNode RenderTextArea()
                        => H("textarea", new VueObject
                        {
                            Id = "cs-input",
                            Class = "editor-input",
                            Spellcheck = false,
                            Rows = 18,
                            Value = "demo",
                            Events = new VueEventHandlers<Event>
                            {
                                ["onInput"] = OnInput
                            }
                        });

                    private static void OnInput(Event @event)
                    {
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { h } from ""npm:vue@3"";
export function renderLabel() {
  return h(""label"", { for: ""cs-input"", class: ""editor-label"" }, ""C# Input"");
}
export function renderTextArea() {
  return h(""textarea"", {
    id: ""cs-input"",
    class: ""editor-input"",
    spellcheck: false,
    rows: 18,
    value: ""demo"",
    onInput: onInput
  });
}
function onInput(event) { }
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueObjectAdditionalHtmlConvenienceMembers_GeneratesFinalAttributeKeys()
    {
        var code = """
            using ECMAScript.Contract;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVNode RenderInput()
                        => H("input", new VueObject
                        {
                            Name = "source",
                            Type = "text",
                            Placeholder = "Type here",
                            Disabled = true,
                            Readonly = true,
                            Required = true,
                            Tabindex = 2
                        });

                    public static IVNode RenderCheckbox()
                        => H("input", new VueObject
                        {
                            Type = "checkbox",
                            Checked = true
                        });

                    public static IVNode RenderLink()
                        => H("a", new VueObject
                        {
                            Href = "/docs"
                        }, "Docs");

                    public static IVNode RenderImage()
                        => H("img", new VueObject
                        {
                            Src = "/logo.svg",
                            Alt = "Logo"
                        });
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { h } from ""npm:vue@3"";
export function renderInput() {
  return h(""input"", {
    name: ""source"",
    type: ""text"",
    placeholder: ""Type here"",
    disabled: true,
    readonly: true,
    required: true,
    tabindex: 2
  });
}
export function renderCheckbox() {
  return h(""input"", { type: ""checkbox"", checked: true });
}
export function renderLink() {
  return h(""a"", { href: ""/docs"" }, ""Docs"");
}
export function renderImage() {
  return h(""img"", { src: ""/logo.svg"", alt: ""Logo"" });
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueObjectFormAndLinkConvenienceMembers_GeneratesFinalAttributeKeys()
    {
        var code = """
            using ECMAScript.Contract;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVNode RenderForm()
                        => H("form", new VueObject
                        {
                            Action = "/submit",
                            Method = "post",
                            Autocomplete = "on"
                        },
                        [
                            H("textarea", new VueObject
                            {
                                Name = "notes",
                                Autofocus = true,
                                Rows = 4,
                                Cols = 32
                            }),
                            H("select", new VueObject
                            {
                                Multiple = true
                            },
                            [
                                H("option", new VueObject
                                {
                                    Selected = true,
                                    Value = "a"
                                }, "Alpha")
                            ])
                        ]);

                    public static IVNode RenderLink()
                        => H("a", new VueObject
                        {
                            Href = "/docs",
                            Target = "_blank",
                            Rel = "noopener"
                        }, "Docs");

                    public static IVNode RenderRegion()
                        => H("div", new VueObject
                        {
                            Role = "button"
                        }, "Pseudo button");
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { h } from ""npm:vue@3"";
export function renderForm() {
  return h(""form"", {
    action: ""/submit"",
    method: ""post"",
    autocomplete: ""on""
  }, [h(""textarea"", {
    name: ""notes"",
    autofocus: true,
    rows: 4,
    cols: 32
  }), h(""select"", { multiple: true }, [h(""option"", { selected: true, value: ""a"" }, ""Alpha"")])]);
}
export function renderLink() {
  return h(""a"", {
    href: ""/docs"",
    target: ""_blank"",
    rel: ""noopener""
  }, ""Docs"");
}
export function renderRegion() {
  return h(""div"", { role: ""button"" }, ""Pseudo button"");
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingWebIdlUnionCollectionExpressionArguments_GeneratesNativeUnionShapes()
    {
        var code = """
            using ECMAScript;

            namespace Demo
            {
                [ECMAScriptModule("webidl/either-collections.mjs")]
                public static class WebIdlUnionModule
                {
                    public static WebSocket CreateSocket()
                        => new WebSocket("wss://example.test/socket", ["chat", "superchat"]);

                    public static URLSearchParams CreateSearchParams()
                        => new URLSearchParams([["q", "term"], ["page", "1"]]);

                    public static IntersectionObserver CreateObserver()
                        => new IntersectionObserver(HandleIntersections, new IntersectionObserverInit
                        {
                            Threshold = [0.25, 0.5],
                            RootMargin = "10px"
                        });

                    private static void HandleIntersections(IntersectionObserverEntry[] entries, IntersectionObserver observer)
                    {
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "WebIdlUnionModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "WebIdlUnionModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export function createSocket() {
  return new WebSocket(""wss://example.test/socket"", [""chat"", ""superchat""]);
}
export function createSearchParams() {
  return new URLSearchParams([[""q"", ""term""], [""page"", ""1""]]);
}
export function createObserver() {
  return new IntersectionObserver(handleIntersections, { threshold: [0.25, 0.5], rootMargin: ""10px"" });
}
function handleIntersections(entries, observer) { }
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingWebIdlUnionProperties_GeneratesExpectedObjectLiterals()
    {
        var code = """
            using ECMAScript;

            namespace Demo
            {
                [ECMAScriptModule("webidl/either-properties.mjs")]
                public static class WebIdlUnionPropertyModule
                {
                    public static MediaStreamConstraints CreateConstraints()
                        => new MediaStreamConstraints
                        {
                            Video = true,
                            Audio = new MediaTrackConstraints
                            {
                                Advanced = []
                            }
                        };

                    public static ConstrainDOMStringParameters CreateFacingMode()
                        => new ConstrainDOMStringParameters
                        {
                            Exact = ["user", "environment"]
                        };
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "WebIdlUnionPropertyModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "WebIdlUnionPropertyModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export function createConstraints() {
  return { video: true, audio: { advanced: [] } };
}
export function createFacingMode() {
  return { exact: [""user"", ""environment""] };
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingNamedWebIdlUnionMethodParameters_GeneratesNativeArguments()
    {
        var code = """
            using ECMAScript;

            namespace Demo
            {
                [ECMAScriptModule("webidl/named-union-method-parameters.mjs")]
                public static class WebIdlNamedUnionMethodModule
                {
                    public static void UpdateValue(ElementInternals internals)
                    {
                        internals.SetFormValue("draft");
                        internals.SetFormValue("published", new FormData());
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "WebIdlNamedUnionMethodModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "WebIdlNamedUnionMethodModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export function updateValue(internals) {
  internals.setFormValue(""draft"");
  internals.setFormValue(""published"", new FormData);
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingWebIdlNamedUnionDictionaryProperty_GeneratesNativeValue()
    {
        var code = """
            using ECMAScript;

            namespace Demo
            {
                [ECMAScriptModule("webidl/named-union-dictionary-property.mjs")]
                public static class WebIdlNamedUnionDictionaryPropertyModule
                {
                    public static PushSubscriptionOptionsInit CreateOptions(Uint8Array key)
                        => new PushSubscriptionOptionsInit
                        {
                            UserVisibleOnly = true,
                            ApplicationServerKey = key
                        };
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "WebIdlNamedUnionDictionaryPropertyModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "WebIdlNamedUnionDictionaryPropertyModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export function createOptions(key) {
  return { userVisibleOnly: true, applicationServerKey: key };
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingWebIdlNamedUnionInterfaceArgument_GeneratesNativeValue()
    {
        var code = """
            using ECMAScript;

            namespace Demo
            {
                [ECMAScriptModule("webidl/named-union-interface-argument.mjs")]
                public static class WebIdlNamedUnionInterfaceArgumentModule
                {
                    public static PromiseResult<CryptoKey> ImportJwk(SubtleCrypto crypto, JsonWebKey jwk, KeyUsage[] usages)
                    {
                        return crypto.ImportKey(KeyFormat.Jwk, jwk, "RSA-PSS", true, usages);
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "WebIdlNamedUnionInterfaceArgumentModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "WebIdlNamedUnionInterfaceArgumentModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export function importJwk(crypto, jwk, usages) {
  return crypto.importKey(""Jwk"", jwk, ""RSA-PSS"", true, usages);
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingWebIdlNamedUnionProjection_GeneratesNativeValue()
    {
        var code = """
            using ECMAScript;

            namespace Demo
            {
                [ECMAScriptModule("webidl/named-union-projection.mjs")]
                public static class WebIdlNamedUnionProjectionModule
                {
                    public static string? ReadValue(FormDataEntryValue value)
                        => value.AsString;

                    public static File? ReadFile(FormDataEntryValue value)
                        => value.AsFile;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "WebIdlNamedUnionProjectionModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "WebIdlNamedUnionProjectionModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export function readValue(value) {
  return value;
}
export function readFile(value) {
  return value;
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingSystemUnionMarkerProjection_GeneratesNativeValue()
    {
        var code = """
            using System;
            using ECMAScript;

            namespace System.Runtime.CompilerServices
            {
                [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
                public sealed class UnionAttribute : Attribute
                {
                }

                public interface IUnion
                {
                    object? Value { get; }
                }
            }

            namespace Demo
            {
                [ECMAScriptModule("webidl/system-union-marker-projection.mjs")]
                public static class SystemUnionMarkerProjectionModule
                {
                    public static string? ReadString(SystemUnionMarker value)
                        => value.AsString;

                    public static object? ReadValue(SystemUnionMarker value)
                        => value.Value;
                }

                [ECMAScript]
                [System.Runtime.CompilerServices.Union]
                public readonly struct SystemUnionMarker : System.Runtime.CompilerServices.IUnion
                {
                    public SystemUnionMarker(string value)
                    {
                    }

                    public string? AsString => default;

                    public object? Value => default;

                    public static implicit operator SystemUnionMarker(string value) => default;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "SystemUnionMarkerProjectionModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "SystemUnionMarkerProjectionModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export function readString(value) {
  return value;
}
export function readValue(value) {
  return value;
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingSourceNativeUnionProjection_GeneratesNativeValue()
    {
        var code = """
            using System;
            using System.Runtime.CompilerServices;
            using ECMAScript;

            namespace Demo
            {
                [ECMAScriptModule("app/native-union-projection.mjs")]
                public static class SourceNativeUnionProjectionModule
                {
                    public static string[]? ReadArray(NativeValues? values)
                        => values?.AsArray;
                }

                [ECMAScript]
                public readonly union NativeValues(string[]) : IUnion
                {
                    public string[]? AsArray => Value as string[];
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "SourceNativeUnionProjectionModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "SourceNativeUnionProjectionModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export function readArray(values) {
  return values;
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingRuntimeIUnionWithoutSystemUnionAttribute_ThrowsUnsupportedExternalPropertyAccess()
    {
        var code = """
            using System;
            using ECMAScript;

            namespace System.Runtime.CompilerServices
            {
                public interface IUnion
                {
                    object? Value { get; }
                }
            }

            namespace Demo
            {
                [ECMAScriptModule("webidl/runtime-iunion-without-union-attribute-projection.mjs")]
                public static class RuntimeIUnionProjectionModule
                {
                    public static object? ReadValue(RuntimeIUnionValue value)
                        => value.Value;
                }

                [ECMAScript]
                public readonly struct RuntimeIUnionValue : System.Runtime.CompilerServices.IUnion
                {
                    public string? AsString => default;

                    public object? Value => default;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "RuntimeIUnionProjectionModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "RuntimeIUnionProjectionModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "External member 'Demo.RuntimeIUnionValue.Value.get' is not supported");
        StringAssert.Contains(exception.Message, "property access");
    }

    [TestMethod]
    public async Task Convert_ClassUsingSystemUnionMarkerWithoutECMAScriptMarker_ThrowsUnsupportedExternalPropertyAccess()
    {
        var code = """
            using System;
            using ECMAScript;

            namespace System.Runtime.CompilerServices
            {
                [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
                public sealed class UnionAttribute : Attribute
                {
                }

                public interface IUnion
                {
                    object? Value { get; }
                }
            }

            namespace Demo
            {
                [ECMAScriptModule("webidl/plain-system-union-projection.mjs")]
                public static class PlainSystemUnionProjectionModule
                {
                    public static string? ReadString(PlainSystemUnion value)
                        => value.AsString;

                    public static object? ReadValue(PlainSystemUnion value)
                        => value.Value;
                }

                [System.Runtime.CompilerServices.Union]
                public readonly struct PlainSystemUnion : System.Runtime.CompilerServices.IUnion
                {
                    public PlainSystemUnion(string value)
                    {
                    }

                    public string? AsString => default;

                    public object? Value => default;

                    public static implicit operator PlainSystemUnion(string value) => default;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PlainSystemUnionProjectionModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PlainSystemUnionProjectionModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "External type 'Demo.PlainSystemUnion' is not supported");
        StringAssert.Contains(exception.Message, "property access");
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueNamesOrOptionsValueProjection_GeneratesNativeValue()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            [ECMAScriptModule("vue/union-value-projection.mjs")]
            public static class TestClass
            {
                public static object? Read(VueNamesOrOptions value)
                    => value.Value;
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var classSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "TestClass")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(classSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export function read(value) {
  return value;
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueValueUnionContractProjection_GeneratesNativeValue()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            [ECMAScriptModule("vue/value-union-projection.mjs")]
            public static class TestClass
            {
                public static object? ReadComputed(VueComputedValue<string> value)
                    => value.Value;

                public static object? ReadWatch(VueWatchDeclaration<string> value)
                    => value.Value;

                public static object? Read(VueStringNumberValue value)
                    => value.Value;

                public static object? ReadInjectFrom(VueInjectFrom<string> value)
                    => value.Value;

                public static object? ReadPropDeclaration(VuePropDeclaration<string> value)
                    => value.Value;
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var classSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "TestClass")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(classSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

AssertScriptEqual(
@"export function readComputed(value) {
  return value;
}
export function readWatch(value) {
  return value;
}
export function read(value) {
  return value;
}
export function readInjectFrom(value) {
  return value;
}
export function readPropDeclaration(value) {
  return value;
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueRouteValueUnionContractProjection_GeneratesNativeValue()
    {
        var code = """
            using ECMAScript;

            [ECMAScriptModule("vueroute/value-union-projection.mjs")]
            public static class TestClass
            {
                public static object? Read(RouteRecordName value)
                    => value.Value;
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var classSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "TestClass")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(classSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"export function read(value) {
  return value;
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueObjectInputConstraintConvenienceMembers_GeneratesFinalAttributeKeys()
    {
        var code = """
            using ECMAScript.Contract;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVNode RenderNumberInput()
                        => H("input", new VueObject
                        {
                            Type = "number",
                            Min = 0,
                            Max = 100,
                            Step = 5,
                            Value = "10"
                        });

                    public static IVNode RenderDateInput()
                        => H("input", new VueObject
                        {
                            Type = "date",
                            Min = "2026-01-01",
                            Max = "2026-12-31",
                            Step = "any"
                        });

                    public static IVNode RenderValidatedTextArea()
                        => H("textarea", new VueObject
                        {
                            Minlength = 5,
                            Maxlength = 120,
                            Pattern = "[A-Za-z ]+",
                            Wrap = "soft"
                        });

                    public static IVNode RenderFileInput()
                        => H("input", new VueObject
                        {
                            Type = "file",
                            Accept = ".png,.jpg"
                        });
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { h } from ""npm:vue@3"";
export function renderNumberInput() {
  return h(""input"", {
    type: ""number"",
    min: 0,
    max: 100,
    step: 5,
    value: ""10""
  });
}
export function renderDateInput() {
  return h(""input"", {
    type: ""date"",
    min: ""2026-01-01"",
    max: ""2026-12-31"",
    step: ""any""
  });
}
export function renderValidatedTextArea() {
  return h(""textarea"", {
    minlength: 5,
    maxlength: 120,
    pattern: ""[A-Za-z ]+"",
    wrap: ""soft""
  });
}
export function renderFileInput() {
  return h(""input"", { type: ""file"", accept: "".png,.jpg"" });
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueObjectIs_GeneratesSpecialAttribute()
    {
        var code = """
            using ECMAScript.Contract;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVNode RenderCustomizedBuiltIn()
                        => H("button", new VueObject
                        {
                            Is = "vue:primary-button"
                        }, "Save");

                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { h } from ""npm:vue@3"";
export function renderCustomizedBuiltIn() {
  return h(""button"", { is: ""vue:primary-button"" }, ""Save"");
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingWithModifiersParamsVariable_PreservesArrayArgument()
    {
        var code = """
            using ECMAScript.Contract;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVNode Render()
                    {
                        var modifiers = new[] { "stop", "prevent" };
                        return H("button", new VueObject
                        {
                            ["onClick"] = Vue3.WithModifiers(OnClick, modifiers)
                        }, "Save");
                    }

                    private static void OnClick()
                    {
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { h, withModifiers } from ""npm:vue@3"";
export function render() {
  let modifiers = [""stop"", ""prevent""];
  return h(""button"", { onClick: withModifiers(onClick, modifiers) }, ""Save"");
}
function onClick() { }
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingWithDirectivesParamsVariable_PreservesArrayArgument()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    private static VueDirective Focus = new VueDirective
                    {
                        Mounted = MountedDirective
                    };

                    public static IVNode Render()
                    {
                        var directives = new VueDirectiveArguments[]
                        {
                            new VueDirectiveArguments(Focus)
                        };
                        var button = H("button", "Save");
                        return Vue3.WithDirectives(button, directives);
                    }

                    private static void MountedDirective(Element element, VueDirectiveBinding binding, IVNode vnode)
                    {
                        element.SetAttribute("data-focus", "true");
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { h, withDirectives } from ""npm:vue@3"";
let focus = { mounted: mountedDirective };
export function render() {
  let directives = [new Array(focus)];
  let button = h(""button"", ""Save"");
  return withDirectives(button, directives);
}
function mountedDirective(element, binding, vnode) {
  element.setAttribute(""data-focus"", ""true"");
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComponentDefinitionMiscOptions_GeneratesInheritedOptionMembers()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "Panel",
                        InheritAttrs = false,
                        Expose = new[] { "focus", "reset" },
                        Render = Render
                    });

                    public static IVNode Render()
                        => H("section", "ready");
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""Panel"",
  inheritAttrs: false,
  expose: [""focus"", ""reset""],
  render: render
});
export function render() {
  return h(""section"", ""ready"");
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComponentObjectFormPropsAndEmits_GeneratesValidatorObjects()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record LabelProps : VueProps
                {
                    [Description("@#label")]
                    public string? Label { get; init; }

                    [Description("@#count")]
                    public int Count { get; init; }
                }

                public sealed record LabelPropOptions : VueProps
                {
                    [Description("@#label")]
                    public VuePropOptions<string>? Label { get; init; }

                    [Description("@#count")]
                    public VuePropOptions<int>? Count { get; init; }
                }

                [ECMAScriptModule("components/validated-label.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent<LabelProps> Component = Vue3.DefineComponent(new VueComponentOptions<LabelProps>
                    {
                        Name = "ValidatedLabel",
                        Props = new LabelPropOptions
                        {
                            Label = new VuePropOptions<string>
                            {
                                Type = VuePropType.String,
                                Required = true,
                                DefaultFactory = DefaultLabel,
                                Validator = ValidateLabel
                            },
                            Count = new VuePropOptions<int>
                            {
                                Types = new VuePropType?[] { VuePropType.Number, null },
                                Default = 1,
                                ValidatorWithProps = ValidateCount
                            }
                        },
                        Emits = new VueEmitRegistry<string>
                        {
                            { "save", ValidateSave }
                        },
                        Setup = Setup
                    });

                    private static VueRenderCallback Setup(LabelProps props, VueSetupContext context)
                    {
                        context.Emit("ignored", props.Label);
                        return Render;
                    }

                    private static IVNode Render()
                        => H("section", "ready");

                    private static string DefaultLabel()
                        => "Untitled";

                    private static bool ValidateLabel(string value)
                        => value.Length > 0;

                    private static bool ValidateCount(int value, VueProps rawProps)
                        => value >= 0;

                    private static bool ValidateSave(string value)
                        => value.Length > 0;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""ValidatedLabel"",
  props: { label: {
    type: String,
    required: true,
    default: defaultLabel,
    validator: validateLabel
  }, count: {
    type: [Number, null],
    default: 1,
    validator: validateCount
  } },
  emits: { save: validateSave },
  setup: setup
});
function setup(props, context) {
  context.emit(""ignored"", props.label);
  return render;
}
function render() {
  return h(""section"", ""ready"");
}
function defaultLabel() {
  return ""Untitled"";
}
function validateLabel(value) {
  return value.length > 0;
}
function validateCount(value, rawProps) {
  return value >= 0;
}
function validateSave(value) {
  return value.length > 0;
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComponentPropRegistry_GeneratesPropRegistryObjectLiteral()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/prop-registry.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "PropRegistryPanel",
                        Props = new VuePropRegistry
                        {
                            { "label", new VuePropOptions
                                {
                                    Type = VuePropType.String,
                                    Required = true
                                }
                            },
                            { "count", new VuePropType?[] { VuePropType.Number, null } }
                        },
                        Render = Render
                    });

                    public static IVNode Render()
                        => H("section", "ready");
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""PropRegistryPanel"",
  props: { label: { type: String, required: true }, count: [Number, null] },
  render: render
});
export function render() {
  return h(""section"", ""ready"");
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVuePropRegistryCollectionInitializerWithDynamicKey_ThrowsOperationTransformationException()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/prop-registry.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "PropRegistryPanel",
                        Props = BuildProps(),
                        Render = Render
                    });

                    private static VuePropRegistry BuildProps()
                    {
                        var key = "label";
                        return new VuePropRegistry
                        {
                            { key, VuePropType.String }
                        };
                    }

                    public static IVNode Render()
                        => H("section", "ready");
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "unsupported dynamic object key");
        StringAssert.Contains(exception.Message, "ECMAScript.Vue3.VuePropRegistry");
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueEmitRegistryCollectionInitializerWithDynamicKey_ThrowsOperationTransformationException()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/emit-registry.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "EmitRegistryPanel",
                        Emits = BuildEmits(),
                        Render = Render
                    });

                    private static VueEmitRegistry<string> BuildEmits()
                    {
                        var key = "save";
                        return new VueEmitRegistry<string>
                        {
                            { key, ValidateSave }
                        };
                    }

                    public static IVNode Render()
                        => H("section", "ready");

                    private static bool ValidateSave(string value)
                        => value.Length > 0;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "unsupported dynamic object key");
        StringAssert.Contains(exception.Message, "ECMAScript.Vue3.VueEmitRegistry");
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComponentOptionsLifecycle_GeneratesPlainHookOptions()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/lifecycle-panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "LifecyclePanel",
                        BeforeCreate = BeforeCreate,
                        Created = Created,
                        BeforeMount = BeforeMount,
                        Mounted = Mounted,
                        BeforeUpdate = BeforeUpdate,
                        Updated = Updated,
                        BeforeUnmount = BeforeUnmount,
                        Unmounted = Unmounted,
                        Activated = Activated,
                        Deactivated = Deactivated,
                        ErrorCaptured = CaptureError,
                        RenderTracked = OnDebug,
                        RenderTriggered = OnDebug,
                        ServerPrefetch = Prefetch,
                        Render = Render
                    });

                    public static IVNode Render()
                        => H("section", "ready");

                    private static void BeforeCreate()
                    {
                    }

                    private static void Created()
                    {
                    }

                    private static void BeforeMount()
                    {
                    }

                    private static void Mounted()
                    {
                    }

                    private static void BeforeUpdate()
                    {
                    }

                    private static void Updated()
                    {
                    }

                    private static void BeforeUnmount()
                    {
                    }

                    private static void Unmounted()
                    {
                    }

                    private static void Activated()
                    {
                    }

                    private static void Deactivated()
                    {
                    }

                    private static bool CaptureError(VueValue? error, VueComponentPublicInstance? instance, string info)
                        => false;

                    private static void OnDebug(VueDebuggerEvent @event)
                    {
                    }

                    private static IPromise Prefetch()
                        => Promise.Resolve();
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""LifecyclePanel"",
  beforeCreate: beforeCreate,
  created: created,
  beforeMount: beforeMount,
  mounted: mounted,
  beforeUpdate: beforeUpdate,
  updated: updated,
  beforeUnmount: beforeUnmount,
  unmounted: unmounted,
  activated: activated,
  deactivated: deactivated,
  errorCaptured: captureError,
  renderTracked: onDebug,
  renderTriggered: onDebug,
  serverPrefetch: prefetch,
  render: render
});
export function render() {
  return h(""section"", ""ready"");
}
function beforeCreate() { }
function created() { }
function beforeMount() { }
function mounted() { }
function beforeUpdate() { }
function updated() { }
function beforeUnmount() { }
function unmounted() { }
function activated() { }
function deactivated() { }
function captureError(error, instance, info) {
  return false;
}
function onDebug(event) { }
function prefetch() {
  return Promise.resolve();
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComponentOptionsProvideInject_GeneratesCompositionOptions()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record ProvidedValues : VueProps
                {
                    [Description("@#theme")]
                    public string? Theme { get; init; }
                }

                [ECMAScriptModule("components/provider-panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "ProviderPanel",
                        Provide = new ProvidedValues
                        {
                            Theme = "dark"
                        },
                        Inject = new[] { "feature" },
                        Render = Render
                    });

                    public static IVNode Render()
                        => H("section", "ready");
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""ProviderPanel"",
  provide: { theme: ""dark"" },
  inject: [""feature""],
  render: render
});
export function render() {
  return h(""section"", ""ready"");
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComponentOptionsInjectRegistry_GeneratesObjectFormInjectOptions()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/provider-panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "ProviderPanel",
                        Inject = new VueInjectRegistry<string>
                        {
                            ["theme"] = "theme",
                            ["label"] = new VueInjectOptions<string>
                            {
                                From = "message",
                                Default = "fallback"
                            }
                        },
                        Render = Render
                    });

                    public static IVNode Render()
                        => H("section", "ready");
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""ProviderPanel"",
  inject: { theme: ""theme"", label: { from: ""message"", default: ""fallback"" } },
  render: render
});
export function render() {
  return h(""section"", ""ready"");
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComponentOptionsTypedAssignments_GeneratesDirectAssignmentsWithoutHelperWrapping()
    {
        var code = """
            using System;
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record ThemeOptions : VueProps
                {
                    [Description("@#dark")]
                    public bool Dark { get; init; }
                }

                [ECMAScriptModule("components/typed-panel.mjs")]
                public static class PanelModule
                {
                    private static readonly VueInjectionKey<int> CountKey = Global.SymbolFn("count");

                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "TypedPanel",
                        Provide = BuildProvide(),
                        Inject = BuildInject(),
                        Computed = BuildComputed(),
                        Watch = BuildWatch(),
                        Components = BuildComponents(),
                        Render = Render
                    });

                    private static VueProps BuildProvide()
                    {
                        VueProps theme = new ThemeOptions
                        {
                            Dark = true
                        };

                        return theme;
                    }

                    private static VueInjectRegistry<int> BuildInject()
                    {
                        VueInjectOptions<int> optionalCount = new VueInjectOptions<int>
                        {
                            From = CountKey,
                            Default = 2
                        };

                        return new VueInjectRegistry<int>
                        {
                            ["count"] = CountKey,
                            ["optionalCount"] = optionalCount
                        };
                    }

                    private static VueComputedRegistry<int> BuildComputed()
                    {
                        Func<int> doubled = ReadDoubled;
                        VueWritableComputedOptions<int> plusOne = new VueWritableComputedOptions<int>
                        {
                            Get = ReadPlusOne,
                            Set = WritePlusOne
                        };

                        return new VueComputedRegistry<int>
                        {
                            ["doubled"] = doubled,
                            ["plusOne"] = plusOne
                        };
                    }

                    private static VueWatchRegistry<int> BuildWatch()
                    {
                        Action<int, int> countChanged = OnCountChanged;
                        VueWatchHandlerOptions<int> totalChanged = new VueWatchHandlerOptions<int>
                        {
                            Immediate = true,
                            Handler = OnTotalChanged
                        };

                        return new VueWatchRegistry<int>
                        {
                            ["count"] = countChanged,
                            ["total"] = totalChanged
                        };
                    }

                    private static VueComponentRegistry BuildComponents()
                    {
                        ECMAScript.Vue3.IVueComponent child = Vue3.DefineComponent(new VueComponentOptions
                        {
                            Name = "ChildView"
                        });

                        return new VueComponentRegistry
                        {
                            ["ChildView"] = child
                        };
                    }

                    public static IVNode Render()
                        => H("section", "ready");

                    private static int ReadDoubled()
                        => 2;

                    private static int ReadPlusOne()
                        => 3;

                    private static void WritePlusOne(int value)
                    {
                    }

                    private static void OnCountChanged(int value, int oldValue)
                    {
                    }

                    private static void OnTotalChanged(int value, int oldValue)
                    {
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
let countKey = Symbol(""count"");
export let component = defineComponent({
  name: ""TypedPanel"",
  provide: buildProvide(),
  inject: buildInject(),
  computed: buildComputed(),
  watch: buildWatch(),
  components: buildComponents(),
  render: render
});
function buildProvide() {
  let theme = { dark: true };
  return theme;
}
function buildInject() {
  let optionalCount = { from: countKey, default: 2 };
  return { count: countKey, optionalCount: optionalCount };
}
function buildComputed() {
  let doubled = readDoubled;
  let plusOne = { get: readPlusOne, set: writePlusOne };
  return { doubled: doubled, plusOne: plusOne };
}
function buildWatch() {
  let countChanged = onCountChanged;
  let totalChanged = { immediate: true, handler: onTotalChanged };
  return { count: countChanged, total: totalChanged };
}
function buildComponents() {
  let child = defineComponent({ name: ""ChildView"" });
  return { ChildView: child };
}
export function render() {
  return h(""section"", ""ready"");
}
function readDoubled() {
  return 2;
}
function readPlusOne() {
  return 3;
}
function writePlusOne(value) { }
function onCountChanged(value, oldValue) { }
function onTotalChanged(value, oldValue) { }
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComponentOptionsProvideFactory_GeneratesFunctionFormProvide()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record ProvidedValues : VueProps
                {
                    [Description("@#theme")]
                    public string? Theme { get; init; }
                }

                [ECMAScriptModule("components/provider-panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "ProviderPanel",
                        ProvideFactory = BuildProvide,
                        Render = Render
                    });

                    private static VueProps BuildProvide()
                        => new ProvidedValues
                        {
                            Theme = "dark"
                        };

                    public static IVNode Render()
                        => H("section", "ready");
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""ProviderPanel"",
  provide: buildProvide,
  render: render
});
function buildProvide() {
  return { theme: ""dark"" };
}
export function render() {
  return h(""section"", ""ready"");
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComponentOptionsProvideAndInjectSymbolKeys_GeneratesComputedProvideAndTypedInjectSource()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/provider-panel.mjs")]
                public static class PanelModule
                {
                    private static readonly Vue3.VueInjectionKey<int> CountKey = Global.SymbolFn("count");

                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "ProviderPanel",
                        Provide = new VueDictionary
                        {
                            [CountKey] = 1
                        },
                        Inject = new VueInjectRegistry<int>
                        {
                            ["count"] = CountKey,
                            ["optionalCount"] = new VueInjectOptions<int>
                            {
                                From = CountKey,
                                Default = 2
                            }
                        },
                        Render = Render
                    });

                    public static IVNode Render()
                        => H("section", "ready");
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
let countKey = Symbol(""count"");
export let component = defineComponent({
  name: ""ProviderPanel"",
  provide: { [countKey]: 1 },
  inject: { count: countKey, optionalCount: { from: countKey, default: 2 } },
  render: render
});
export function render() {
  return h(""section"", ""ready"");
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComponentOptionsInjectRegistryDefaultFactory_GeneratesFactoryObjectFormInject()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/provider-panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "ProviderPanel",
                        Inject = new VueInjectRegistry<string>
                        {
                            ["label"] = new VueInjectOptions<string>
                            {
                                From = "message",
                                DefaultFactory = BuildDefaultLabel
                            }
                        },
                        Render = Render
                    });

                    private static string BuildDefaultLabel()
                        => "fallback";

                    public static IVNode Render()
                        => H("section", "ready");
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""ProviderPanel"",
  inject: { label: { from: ""message"", default: buildDefaultLabel } },
  render: render
});
function buildDefaultLabel() {
  return ""fallback"";
}
export function render() {
  return h(""section"", ""ready"");
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComponentOptionsMixinsAndExtends_GeneratesCompositionOptions()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/mixed-panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "MixedPanel",
                        Extends = new VueComponentOptions
                        {
                            Name = "BasePanel",
                            Created = BaseCreated
                        },
                        Mixins = new VueComponentDefinition[]
                        {
                            new VueComponentOptions
                            {
                                Name = "FocusableMixin",
                                Mounted = FocusMounted
                            }
                        },
                        Render = Render
                    });

                    public static IVNode Render()
                        => H("section", "ready");

                    private static void BaseCreated()
                    {
                    }

                    private static void FocusMounted()
                    {
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""MixedPanel"",
  extends: { name: ""BasePanel"", created: baseCreated },
  mixins: [{ name: ""FocusableMixin"", mounted: focusMounted }],
  render: render
});
export function render() {
  return h(""section"", ""ready"");
}
function baseCreated() { }
function focusMounted() { }
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComponentOptionsData_GeneratesDataFactoryOption()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record LocalState : VueProps
                {
                    [Description("@#count")]
                    public int Count { get; init; }
                }

                [ECMAScriptModule("components/data-panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "DataPanel",
                        Data = CreateState,
                        Render = Render
                    });

                    public static IVNode Render()
                        => H("section", "ready");

                    private static LocalState CreateState()
                        => new LocalState
                        {
                            Count = 1
                        };
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""DataPanel"",
  data: createState,
  render: render
});
export function render() {
  return h(""section"", ""ready"");
}
function createState() {
  return { count: 1 };
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComponentOptionsComputed_GeneratesComputedOptions()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/computed-panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "ComputedPanel",
                        Computed = new VueComputedRegistry<int>
                        {
                            { "doubled", ReadDoubled },
                            { "plusOne", new VueWritableComputedOptions<int>
                            {
                                Get = ReadPlusOne,
                                Set = WritePlusOne
                            } }
                        },
                        Render = Render
                    });

                    public static IVNode Render()
                        => H("section", "ready");

                    private static int ReadDoubled()
                        => 2;

                    private static int ReadPlusOne()
                        => 3;

                    private static void WritePlusOne(int value)
                    {
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""ComputedPanel"",
  computed: { doubled: readDoubled, plusOne: { get: readPlusOne, set: writePlusOne } },
  render: render
});
export function render() {
  return h(""section"", ""ready"");
}
function readDoubled() {
  return 2;
}
function readPlusOne() {
  return 3;
}
function writePlusOne(value) { }
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComputedRegistryCollectionInitializerWithDynamicKey_ThrowsOperationTransformationException()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/computed-panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "ComputedPanel",
                        Computed = BuildComputed(),
                        Render = Render
                    });

                    private static VueComputedRegistry<int> BuildComputed()
                    {
                        var key = "doubled";
                        return new VueComputedRegistry<int>
                        {
                            { key, ReadDoubled }
                        };
                    }

                    public static IVNode Render()
                        => H("section", "ready");

                    private static int ReadDoubled()
                        => 2;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "unsupported dynamic object key");
        StringAssert.Contains(exception.Message, "ECMAScript.Vue3.VueComputedRegistry");
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComponentOptionsMethods_GeneratesMethodsOptions()
    {
        var code = """
            using System;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/methods-panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "MethodsPanel",
                        Methods = new VueMethodRegistry<Action>
                        {
                            { "reset", Reset },
                            { "focus", Focus }
                        },
                        Render = Render
                    });

                    public static IVNode Render()
                        => H("section", "ready");

                    private static void Reset()
                    {
                    }

                    private static void Focus()
                    {
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""MethodsPanel"",
  methods: { reset: reset, focus: focus },
  render: render
});
export function render() {
  return h(""section"", ""ready"");
}
function reset() { }
function focus() { }
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueMethodRegistryCollectionInitializerWithDynamicKey_ThrowsOperationTransformationException()
    {
        var code = """
            using System;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/methods-panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "MethodsPanel",
                        Methods = BuildMethods(),
                        Render = Render
                    });

                    private static VueMethodRegistry<Action> BuildMethods()
                    {
                        var key = "reset";
                        return new VueMethodRegistry<Action>
                        {
                            { key, Reset }
                        };
                    }

                    public static IVNode Render()
                        => H("section", "ready");

                    private static void Reset()
                    {
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "unsupported dynamic object key");
        StringAssert.Contains(exception.Message, "ECMAScript.Vue3.VueMethodRegistry");
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComponentOptionsWatch_GeneratesWatchOptions()
    {
        var code = """
            using System;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/watch-panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "WatchPanel",
                        Watch = new VueWatchRegistry<int>
                        {
                            { "count", OnCountChanged },
                            { "total", new VueWatchHandlerOptions<int>
                                {
                                    Immediate = true,
                                    Deep = 1,
                                    Handler = OnTotalChanged
                                }
                            },
                            { "legacy", "onLegacyChanged" },
                            { "legacyList", new[] { "onLegacyChanged", "onLegacyChangedAgain" } },
                            { "countList", new[] { OnCountChanged, OnCountChangedAgain } },
                            { "mixedList", new VueWatchEntry<int>[] { "onLegacyChanged", (Action<int, int>)OnCountChanged } }
                        },
                        Methods = new VueMethodRegistry<Action<int, int>>
                        {
                            { "onLegacyChanged", OnLegacyChanged },
                            { "onLegacyChangedAgain", OnLegacyChangedAgain }
                        },
                        Render = Render
                    });

                    public static IVNode Render()
                        => H("section", "ready");

                    private static void OnCountChanged(int value, int oldValue)
                    {
                    }

                    private static void OnTotalChanged(int value, int oldValue)
                    {
                    }

                    private static void OnLegacyChanged(int value, int oldValue)
                    {
                    }

                    private static void OnLegacyChangedAgain(int value, int oldValue)
                    {
                    }

                    private static void OnCountChangedAgain(int value, int oldValue)
                    {
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""WatchPanel"",
  watch: {
    count: onCountChanged,
    total: {
      immediate: true,
      deep: 1,
      handler: onTotalChanged
    },
    legacy: ""onLegacyChanged"",
    legacyList: [""onLegacyChanged"", ""onLegacyChangedAgain""],
    countList: [onCountChanged, onCountChangedAgain],
    mixedList: [""onLegacyChanged"", onCountChanged]
  },
  methods: { onLegacyChanged: onLegacyChanged, onLegacyChangedAgain: onLegacyChangedAgain },
  render: render
});
export function render() {
  return h(""section"", ""ready"");
}
function onCountChanged(value, oldValue) { }
function onTotalChanged(value, oldValue) { }
function onLegacyChanged(value, oldValue) { }
function onLegacyChangedAgain(value, oldValue) { }
function onCountChangedAgain(value, oldValue) { }
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComponentOptionsThisBoundCallbacks_GeneratesThisAwareOptionsWrappers()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public abstract class PanelThis
                {
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "ThisPanel",
                        Data = Vue3.BindThis<PanelThis>(BuildData),
                        Computed = new VueComputedRegistry<int>
                        {
                            { "double", Vue3.BindThis<PanelThis, int>(GetDouble) },
                            { "labelLength", new VueWritableComputedOptions<int>
                                {
                                    Get = Vue3.BindThis<PanelThis, int>(GetLabelLength),
                                    Set = Vue3.BindThis<PanelThis, int>(SetLabelLength)
                                }
                            }
                        },
                        Methods = new VueMethodRegistry<global::System.Action<int>>
                        {
                            { "add", Vue3.BindThis<PanelThis, int>(AddCount) }
                        },
                        Watch = new VueWatchRegistry<int>
                        {
                            { "count", Vue3.BindThis<PanelThis, int, int>(OnCountChanged) },
                            { "labelSize", new VueWatchCleanupHandlerOptions<int>
                                {
                                    Immediate = true,
                                    Handler = Vue3.BindThis<PanelThis, int>(OnLabelSizeChanged)
                                }
                            }
                        },
                        Render = Render
                    });

                    public static IVNode Render()
                        => H("section", "ready");

                    private static VueProps BuildData(PanelThis self)
                        => new VueDictionary
                        {
                            ["count"] = 1,
                            ["label"] = "seed"
                        };

                    private static int GetDouble(PanelThis self)
                        => 2;

                    private static int GetLabelLength(PanelThis self)
                        => 0;

                    private static void SetLabelLength(PanelThis self, int length)
                    {
                    }

                    private static void AddCount(PanelThis self, int step)
                    {
                    }

                    private static void OnCountChanged(PanelThis self, int value, int oldValue)
                    {
                    }

                    private static void OnLabelSizeChanged(PanelThis self, int value, int oldValue, VueWatchCleanupRegistration onCleanup)
                    {
                        onCleanup(() => {});
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        Assert.IsFalse(script.Contains("bindThis", StringComparison.Ordinal));
        StringAssert.Contains(script, "data: (__cb => function() {");
        StringAssert.Contains(script, "})(buildData)");
        StringAssert.Contains(script, "double: (__cb => function() {");
        StringAssert.Contains(script, "})(getDouble)");
        StringAssert.Contains(script, "get: (__cb => function() {");
        StringAssert.Contains(script, "})(getLabelLength)");
        StringAssert.Contains(script, "set: (__cb => function() {");
        StringAssert.Contains(script, "})(setLabelLength)");
        StringAssert.Contains(script, "add: (__cb => function() {");
        StringAssert.Contains(script, "})(addCount)");
        StringAssert.Contains(script, "count: (__cb => function() {");
        StringAssert.Contains(script, "})(onCountChanged)");
        StringAssert.Contains(script, "handler: (__cb => function() {");
        StringAssert.Contains(script, "})(onLabelSizeChanged)");
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueWatchRegistryCollectionInitializerWithDynamicKey_ThrowsOperationTransformationException()
    {
        var code = """
            using System;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/watch-panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "WatchPanel",
                        Watch = BuildWatch(),
                        Render = Render
                    });

                    private static VueWatchRegistry<int> BuildWatch()
                    {
                        var key = "count";
                        return new VueWatchRegistry<int>
                        {
                            { key, OnCountChanged }
                        };
                    }

                    public static IVNode Render()
                        => H("section", "ready");

                    private static void OnCountChanged(int value, int oldValue)
                    {
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "unsupported dynamic object key");
        StringAssert.Contains(exception.Message, "ECMAScript.Vue3.VueWatchRegistry");
    }
    [TestMethod]
    public async Task Convert_ClassUsingVueAppConfig_GeneratesPlainConfigurationAssignments()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static void Boot(IVueComponent component)
                    {
                        var app = Vue3.CreateApp(component);
                        app.Config.ErrorHandler = OnError;
                        app.Config.WarnHandler = OnWarn;
                        app.Config.Performance = true;
                        app.Config.CompilerOptions.IsCustomElement = IsCustomElement;
                        app.Config.CompilerOptions.Whitespace = Vue3.VueCompilerWhitespace.Preserve;
                        app.Config.CompilerOptions.Delimiters = new[] { "[[", "]]" };
                        app.Config.CompilerOptions.Comments = true;
                        app.Config.GlobalProperties["$name"] = "jazor";
                        app.Config.GlobalProperties["feature"] = true;
                        app.Config.OptionMergeStrategies["route"] = MergeRoute;
                        app.Config.IdPrefix = "app";
                        app.Config.ThrowUnhandledErrorInProduction = true;
                    }

                    private static void OnError(VueValue? error, VueComponentPublicInstance? instance, string info)
                    {
                    }

                    private static void OnWarn(string message, VueComponentPublicInstance? instance, string trace)
                    {
                    }

                    private static bool IsCustomElement(string tag)
                        => tag == "x-panel";

                    private static VueValue? MergeRoute(VueValue? parent, VueValue? child)
                        => child;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { createApp } from ""npm:vue@3"";
export function boot(component) {
  let app = createApp(component);
  app.config.errorHandler = onError;
  app.config.warnHandler = onWarn;
  app.config.performance = true;
  app.config.compilerOptions.isCustomElement = isCustomElement;
  app.config.compilerOptions.whitespace = ""preserve"";
  app.config.compilerOptions.delimiters = [""[["", ""]]""];
  app.config.compilerOptions.comments = true;
  app.config.globalProperties[""$name""] = ""jazor"";
  app.config.globalProperties[""feature""] = true;
  app.config.optionMergeStrategies[""route""] = mergeRoute;
  app.config.idPrefix = ""app"";
  app.config.throwUnhandledErrorInProduction = true;
}
function onError(error, instance, info) { }
function onWarn(message, instance, trace) { }
function isCustomElement(tag) {
  return tag === ""x-panel"";
}
function mergeRoute(parent, child) {
  return child;
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueObjectTypedProps_FlattensPropsAndOmitsNullExpansionMembers()
    {
        var code = """
            using ECMAScript.Contract;
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record ChildProps : VueProps
                {
                    [Description("@#title")]
                    public string? Title { get; init; }
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent<ChildProps> Child = Vue3.DefineComponent(new VueComponentOptions<ChildProps>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render()
                        => H(Child, new VueObject<ChildProps>
                        {
                            Props = new ChildProps { Title = "Welcome" },
                            Attrs = null,
                            Dataset = null,
                            Raw = null,
                            Title = null
                        });
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let child = defineComponent({ name: ""ChildView"" });
export function render() {
  return h(child, { title: ""Welcome"" });
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueComponentSetup_GeneratesSetupFunction()
    {
        var code = """
            using System;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/counter.mjs")]
                public static class CounterModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "CounterView",
                        Setup = Setup
                    });

                    private static VueRenderCallback Setup()
                    {
                        var count = Vue3.Ref(1);
                        return () => H("button", count.Value == 1);
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "CounterModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "CounterModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h, ref } from ""npm:vue@3"";
export let component = defineComponent({ name: ""CounterView"", setup: setup });
function setup() {
  let count = ref(1);
  return () => {
    return h(""button"", count.value === 1);
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingTypedVueComponentSetup_GeneratesPropsEmitsAndContextAwareSetup()
    {
        var code = """
            using ECMAScript;
            using System;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record CounterProps : VueProps;

                [ECMAScriptModule("components/counter.mjs")]
                public static class CounterModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions<CounterProps>
                    {
                        Name = "CounterView",
                        Props = ["message"],
                        Emits = ["ready"],
                        Setup = Setup
                    });

                    private static VueRenderCallback Setup(CounterProps props, VueSetupContext context)
                    {
                        context.Emit("ready", true);
                        return () => H("button", "ready");
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "CounterModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "CounterModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""CounterView"",
  props: [""message""],
  emits: [""ready""],
  setup: setup
});
function setup(props, context) {
  context.emit(""ready"", true);
  return () => {
    return h(""button"", ""ready"");
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueSetupContextEmit_GeneratesMultiPayloadEmitCalls()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record CounterProps : VueProps;

                [ECMAScriptModule("components/counter.mjs")]
                public static class CounterModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions<CounterProps>
                    {
                        Name = "CounterView",
                        Emits = ["batch"],
                        Setup = Setup
                    });

                    private static VueRenderCallback Setup(CounterProps props, VueSetupContext context)
                    {
                        context.Emit("batch", 1, "two", true, 4);
                        return () => H("button", "ready");
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "CounterModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "CounterModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""CounterView"",
  emits: [""batch""],
  setup: setup
});
function setup(props, context) {
  context.emit(""batch"", 1, ""two"", true, 4);
  return () => {
    return h(""button"", ""ready"");
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueUseModel_GeneratesTypedModelRef()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record CounterProps : VueProps
                {
                    [Description("@#modelValue")]
                    public int ModelValue { get; init; }
                }

                [ECMAScriptModule("components/counter.mjs")]
                public static class CounterModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions<CounterProps>
                    {
                        Name = "CounterView",
                        Props = ["modelValue"],
                        Emits = ["update:modelValue"],
                        Setup = Setup
                    });

                    private static VueRenderCallback Setup(CounterProps props, VueSetupContext context)
                    {
                        var model = Vue3.UseModel<int>(props, "modelValue", new VueModelOptions<int>
                        {
                            Get = Normalize,
                            Set = Normalize
                        });
                        model.Value = model.Value + 1;
                        return () => H("button", model.Value);
                    }

                    private static int Normalize(int value)
                        => value;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "CounterModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "CounterModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h, useModel } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""CounterView"",
  props: [""modelValue""],
  emits: [""update:modelValue""],
  setup: setup
});
function setup(props, context) {
  let model = useModel(props, ""modelValue"", { get: normalize, set: normalize });
  model.value = model.value + 1;
  return () => {
    return h(""button"", model.value);
  };
}
function normalize(value) {
  return value;
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueModelNameDefaultHelper_GeneratesTypedDefaultModelContract()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record CounterProps : VueProps
                {
                    [Description("@#modelValue")]
                    public int ModelValue { get; init; }
                }

                [ECMAScriptModule("components/counter-default-model.mjs")]
                public static class CounterModule
                {
                    private static readonly VueModelName<CounterProps, int> CounterModel = Vue3.ModelName<CounterProps, int>();
                    private static readonly string CounterUpdate = Vue3.ModelUpdateEventName(CounterModel);

                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions<CounterProps>
                    {
                        Name = "CounterView",
                        Props = [Vue3.ModelPropName(CounterModel)],
                        Emits = [CounterUpdate],
                        Setup = Setup
                    });

                    private static VueRenderCallback Setup(CounterProps props, VueSetupContext context)
                    {
                        var model = Vue3.UseModel(props, CounterModel);
                        return () => H("button", model.Value);
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "CounterModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "CounterModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h, useModel } from ""npm:vue@3"";
let counterModel = ""modelValue"";
let counterUpdate = `update:${counterModel}`;
export let component = defineComponent({
  name: ""CounterView"",
  props: [counterModel],
  emits: [counterUpdate],
  setup: setup
});
function setup(props, context) {
  let model = useModel(props, counterModel);
  return () => {
    return h(""button"", model.value);
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueModelNameNamedHelper_GeneratesTypedNamedModelContract()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record CounterProps : VueProps
                {
                    [Description("@#count")]
                    public int Count { get; init; }
                }

                [ECMAScriptModule("components/counter-named-model.mjs")]
                public static class CounterModule
                {
                    private static readonly VueModelName<CounterProps, int> CountModel = Vue3.ModelName<CounterProps, int>("count");
                    private static readonly string CountUpdate = Vue3.ModelUpdateEventName(CountModel);

                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions<CounterProps>
                    {
                        Name = "CounterView",
                        Props = [Vue3.ModelPropName(CountModel)],
                        Emits = [CountUpdate],
                        Setup = Setup
                    });

                    private static VueRenderCallback Setup(CounterProps props, VueSetupContext context)
                    {
                        var model = Vue3.UseModel(props, CountModel, new VueModelOptions<int>
                        {
                            Get = Normalize,
                            Set = Normalize
                        });
                        return () => H("button", model.Value);
                    }

                    private static int Normalize(int value)
                        => value;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "CounterModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "CounterModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h, useModel } from ""npm:vue@3"";
let countModel = ""count"";
let countUpdate = `update:${countModel}`;
export let component = defineComponent({
  name: ""CounterView"",
  props: [countModel],
  emits: [countUpdate],
  setup: setup
});
function setup(props, context) {
  let model = useModel(props, countModel, { get: normalize, set: normalize });
  return () => {
    return h(""button"", model.value);
  };
}
function normalize(value) {
  return value;
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueSetupContextEmitWithModelName_GeneratesTypedUpdateEventEmit()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record CounterProps : VueProps
                {
                    [Description("@#count")]
                    public int Count { get; init; }
                }

                [ECMAScriptModule("components/counter-model-emit.mjs")]
                public static class CounterModule
                {
                    private static readonly VueModelName<CounterProps, int> CountModel = Vue3.ModelName<CounterProps, int>("count");
                    private static readonly string CountUpdate = Vue3.ModelUpdateEventName(CountModel);

                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions<CounterProps>
                    {
                        Name = "CounterView",
                        Props = [Vue3.ModelPropName(CountModel)],
                        Emits = [CountUpdate],
                        Setup = Setup
                    });

                    private static VueRenderCallback Setup(CounterProps props, VueSetupContext context)
                    {
                        var model = Vue3.UseModel(props, CountModel);
                        context.Emit(CountModel, model.Value);
                        return () => H("button", model.Value);
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "CounterModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "CounterModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h, useModel } from ""npm:vue@3"";
let countModel = ""count"";
let countUpdate = `update:${countModel}`;
export let component = defineComponent({
  name: ""CounterView"",
  props: [countModel],
  emits: [countUpdate],
  setup: setup
});
function setup(props, context) {
  let model = useModel(props, countModel);
  context.emit(`update:${countModel}`, model.value);
  return () => {
    return h(""button"", model.value);
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueModelTypedModifierProjection_GeneratesTupleModifierAccess()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public abstract class CounterModifiers : VueModelModifiers
                {
                    [Description("@#trim")]
                    public abstract bool? Trimmed { get; }
                }

                public sealed record CounterProps : VueProps
                {
                    [Description("@#modelValue")]
                    public int? ModelValue { get; init; }
                }

                [ECMAScriptModule("components/counter-view.mjs")]
                public static class CounterModule
                {
                    public static IVueComponent<CounterProps> Component = Vue3.DefineComponent(new VueComponentOptions<CounterProps>
                    {
                        Name = "CounterView",
                        Props = ["modelValue"],
                        Emits = ["update:modelValue"],
                        Setup = Setup
                    });

                    private static VueRenderCallback Setup(CounterProps props, VueSetupContext context)
                    {
                        var model = Vue3.UseModel<int>(props, "modelValue");
                        var modifiers = model.GetModifiers<CounterModifiers>();
                        return () => H("button", modifiers.Trimmed == true ? model.Value : 0);
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "CounterModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "CounterModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h, useModel } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""CounterView"",
  props: [""modelValue""],
  emits: [""update:modelValue""],
  setup: setup
});
function setup(props, context) {
  let model = useModel(props, ""modelValue"");
  let modifiers = model[1];
  return () => {
    return h(""button"", modifiers.trim === true ? model.value : 0);
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingTypedUseAttrsAndUseSlots_GeneratesPlainHelpers()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public abstract record PanelAttrs : VueProps
                {
                    [Description("@#title")]
                    public abstract string? Title { get; }
                }

                public abstract record PanelSlots : VueSlots
                {
                    [Description("@#default")]
                    public abstract VueSlotCallback Default { get; }
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "PanelView",
                        Setup = Setup
                    });

                    private static VueRenderCallback Setup()
                    {
                        var attrs = Vue3.UseAttrs<PanelAttrs>();
                        var slots = Vue3.UseSlots<PanelSlots>();
                        return () => H("section", attrs.Title == "ready" ? slots.Default() : H("span", "empty"));
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h, useAttrs, useSlots } from ""npm:vue@3"";
export let component = defineComponent({ name: ""PanelView"", setup: setup });
function setup() {
  let attrs = useAttrs();
  let slots = useSlots();
  return () => {
    return h(""section"", attrs.title === ""ready"" ? slots.default() : h(""span"", ""empty""));
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueAttributeBagConvenienceMembers_GeneratesPlainMemberAccess()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "PanelView",
                        Setup = Setup
                    });

                    private static VueRenderCallback Setup()
                    {
                        var attrs = Vue3.UseAttrs();
                        return () => H("input", new VueObject
                        {
                            For = attrs.For,
                            Name = attrs.Name,
                            Type = attrs.Type,
                            Placeholder = attrs.Placeholder,
                            Disabled = attrs.Disabled,
                            Readonly = attrs.Readonly,
                            Required = attrs.Required,
                            Tabindex = attrs.Tabindex,
                            Role = attrs.Role
                        });
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h, useAttrs } from ""npm:vue@3"";
export let component = defineComponent({ name: ""PanelView"", setup: setup });
function setup() {
  let attrs = useAttrs();
  return () => {
    return h(""input"", {
      for: attrs.for,
      name: attrs.name,
      type: attrs.type,
      placeholder: attrs.placeholder,
      disabled: attrs.disabled,
      readonly: attrs.readonly,
      required: attrs.required,
      tabindex: attrs.tabindex,
      role: attrs.role
    });
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueReadSideListenerAndScopedSlotHelpers_GeneratesCallableAccess()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record RowScope : VueProps
                {
                    [Description("@#label")]
                    public string? Label { get; init; }
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "PanelView",
                        Setup = Setup
                    });

                    private static VueRenderCallback Setup()
                    {
                        var attrs = Vue3.UseAttrs<VueAttributeListeners>();
                        var slots = Vue3.UseSlots<VueScopedSlots<RowScope>>();
                        return () => H("section", new IVNode[]
                        {
                            H("button", new VueObject
                            {
                                Events = new VueEventHandlers
                                {
                                    ["on:update"] = attrs["on:update"]
                                }
                            }),
                            slots.Default!(new RowScope { Label = "default" }),
                            slots["row-item"]!(new RowScope { Label = "row" })
                        });
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h, useAttrs, useSlots } from ""npm:vue@3"";
export let component = defineComponent({ name: ""PanelView"", setup: setup });
function setup() {
  let attrs = useAttrs();
  let slots = useSlots();
  return () => {
    return h(""section"", [h(""button"", { ""on:update"": attrs[""on:update""] }), slots.default({ label: ""default"" }), slots[""row-item""]({ label: ""row"" })]);
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingUntypedVueComponentOptionsProps_GeneratesPropsArray()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "PanelView",
                        Props = ["title", "active"],
                        Emits = ["ready"],
                        Setup = Setup
                    });

                    private static VueRenderCallback Setup()
                    {
                        return () => H("section", "ready");
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""PanelView"",
  props: [""title"", ""active""],
  emits: [""ready""],
  setup: setup
});
function setup() {
  return () => {
    return h(""section"", ""ready"");
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingTypedVueComponentSetup_DoesNotInferRuntimePropsOrEmits()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public abstract record BaseCounterProps : VueProps
                {
                    [Description("@#id")]
                    public int Id { get; init; }
                }

                public sealed record CounterProps : BaseCounterProps
                {
                    [Description("@#message")]
                    public string? Message { get; init; }
                }

                [ECMAScriptModule("components/counter.mjs")]
                public static class CounterModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions<CounterProps>
                    {
                        Name = "CounterView",
                        Setup = Setup
                    });

                    private static VueRenderCallback Setup(CounterProps props, VueSetupContext context)
                    {
                        context.Emit("ready", props.Message);
                        return () => H("button", props.Id == 1 && props.Message == "ready");
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "CounterModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "CounterModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({ name: ""CounterView"", setup: setup });
function setup(props, context) {
  context.emit(""ready"", props.message);
  return () => {
    return h(""button"", props.id === 1 && props.message === ""ready"");
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingTypedVueComponentWithSlots_DoesNotInferRuntimePropsOrEmits()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record ChildProps : VueProps
                {
                    [Description("@#title")]
                    public string? Title { get; init; }
                }

                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback ChildContent { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent<ChildProps, ChildSlots> Child = Vue3.DefineComponent(new VueComponentOptions<ChildProps, ChildSlots>
                    {
                        Name = "ChildView",
                        Setup = SetupChild
                    });

                    public static IVNode Render()
                        => H(Child, new ChildProps { Title = "Welcome" }, new ChildSlots
                        {
                            ChildContent = RenderBody
                        });

                    private static IVNode RenderBody()
                        => H("span", "body");

                    private static VueRenderCallback SetupChild(ChildProps props, VueSetupContext<ChildSlots> context)
                    {
                        context.Emit("ready", props.Title);
                        return () => H("section", context.Slots.ChildContent());
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let child = defineComponent({ name: ""ChildView"", setup: setupChild });
export function render() {
  return h(child, { title: ""Welcome"" }, { default: renderBody });
}
function renderBody() {
  return h(""span"", ""body"");
}
function setupChild(props, context) {
  context.emit(""ready"", props.title);
  return () => {
    return h(""section"", context.slots.default());
  };
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingTypedVueComponentSetup_OmitsRuntimePropsAndEmitsWhenNotExplicit()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record EmptyProps : VueProps;

                [ECMAScriptModule("components/empty.mjs")]
                public static class EmptyModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new VueComponentOptions<EmptyProps>
                    {
                        Name = "EmptyView",
                        Setup = Setup
                    });

                    private static VueRenderCallback Setup(EmptyProps props, VueSetupContext context)
                    {
                        return () => H("div", "empty");
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "EmptyModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "EmptyModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({ name: ""EmptyView"", setup: setup });
function setup(props, context) {
  return () => {
    return h(""div"", ""empty"");
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingConfiguredPropsAndEmitsAttributes_InferPropsAndEmits()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using Jazor.ComplierTest;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record CounterProps : VueProps
                {
                    [Description("@#message")]
                    public string? Message { get; init; }
                }

                [ECMAScriptModule("components/counter.mjs")]
                public static class CounterModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new TestShiftedContractComponentOptions<int, CounterProps>
                    {
                        Name = "CounterView",
                        Bootstrap = Setup
                    });

                    private static VueRenderCallback Setup(CounterProps props, VueSetupContext context)
                    {
                        context.Emit("ready", props.Message);
                        return () => H("button", props.Message == "ready");
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "CounterModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(TestShiftedContractComponentOptions<,>).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "CounterModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  name: ""CounterView"",
  props: [""message""],
  emits: [""ready""],
  setup: setup
});
function setup(props, context) {
  context.emit(""ready"", props.message);
  return () => {
    return h(""button"", props.message === ""ready"");
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingInheritedPropsAndEmitsMembers_InferPropsAndEmits()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using Jazor.ComplierTest;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record CounterProps : VueProps
                {
                    [Description("@#message")]
                    public string? Message { get; init; }
                }

                [ECMAScriptModule("components/counter.mjs")]
                public static class CounterModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new TestInheritedContractComponentOptions<CounterProps>
                    {
                        Name = "CounterView",
                        Setup = Setup
                    });

                    private static VueRenderCallback Setup(CounterProps props, VueSetupContext context)
                    {
                        context.Emit("ready", props.Message);
                        return () => H("button", props.Message == "ready");
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "CounterModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(TestInheritedContractComponentOptions<>).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "CounterModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let component = defineComponent({
  props: [""message""],
  emits: [""ready""],
  name: ""CounterView"",
  setup: setup
});
function setup(props, context) {
  context.emit(""ready"", props.message);
  return () => {
    return h(""button"", props.message === ""ready"");
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueHComponentSlots_GeneratesSlotsObjectArgument()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback? Default { get; init; }
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Child = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render()
                        => H(Child, new ChildSlots { Default = RenderBody });

                    private static IVNode RenderBody()
                        => H("span", "body");
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let child = defineComponent({ name: ""ChildView"" });
export function render() {
  return h(child, { default: renderBody });
}
function renderBody() {
  return h(""span"", ""body"");
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueBaseSlotsWithStringKeys_GeneratesSlotsObjectArgument()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Child = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render()
                        => H(Child, new VueSlots
                        {
                            ["default"] = RenderBody
                        });

                    private static IVNode RenderBody()
                        => H("span", "body");
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let child = defineComponent({ name: ""ChildView"" });
export function render() {
  return h(child, { default: renderBody });
}
function renderBody() {
  return h(""span"", ""body"");
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueHComponentSlotsWithDefaultNamedSlot_GeneratesSlotsObjectArgument()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record ChildSlots : VueSlots
                {
                    public VueSlotCallback? Default { get; init; }
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Child = Vue3.DefineComponent(new VueComponentOptions
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render()
                        => H(Child, new ChildSlots { Default = RenderBody });

                    private static IVNode RenderBody()
                        => H("span", "body");
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let child = defineComponent({ name: ""ChildView"" });
export function render() {
  return h(child, { default: renderBody });
}
function renderBody() {
  return h(""span"", ""body"");
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueBaseSlotsWithPropsAndStringKeys_GeneratesPropsAndSlotsArguments()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record ChildProps : VueProps
                {
                    [Description("@#title")]
                    public string? Title { get; init; }
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Child = Vue3.DefineComponent(new VueComponentOptions<ChildProps>
                    {
                        Name = "ChildView",
                        Setup = SetupChild
                    });

                    public static IVNode Render()
                        => H(Child, new ChildProps { Title = "Welcome" }, new VueSlots { ["default"] = RenderBody });

                    private static IVNode RenderBody()
                        => H("span", "body");

                    private static VueRenderCallback SetupChild(ChildProps props, VueSetupContext context)
                        => () => H("section", "child");
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let child = defineComponent({ name: ""ChildView"", setup: setupChild });
export function render() {
  return h(child, { title: ""Welcome"" }, { default: renderBody });
}
function renderBody() {
  return h(""span"", ""body"");
}
function setupChild(props, context) {
  return () => {
    return h(""section"", ""child"");
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueHComponentPropsAndSlots_GeneratesPropsAndSlotsArguments()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record ChildProps : VueProps
                {
                    [Description("@#title")]
                    public string? Title { get; init; }
                }

                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#header")]
                    public VueSlotCallback<string>? Header { get; init; }
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Child = Vue3.DefineComponent(new VueComponentOptions<ChildProps>
                    {
                        Name = "ChildView",
                        Setup = SetupChild
                    });

                    public static IVNode Render()
                        => H(Child, new ChildProps { Title = "Welcome" }, new ChildSlots { Header = RenderHeader });

                    private static IVNode RenderHeader(string title)
                        => H("h1", title);

                    private static VueRenderCallback SetupChild(ChildProps props, VueSetupContext context)
                        => () => H("section", "child");
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let child = defineComponent({ name: ""ChildView"", setup: setupChild });
export function render() {
  return h(child, { title: ""Welcome"" }, { header: renderHeader });
}
function renderHeader(title) {
  return h(""h1"", title);
}
function setupChild(props, context) {
  return () => {
    return h(""section"", ""child"");
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }
    [TestMethod]
    public async Task Convert_ClassUsingTypedVueSlotOnlyComponent_GeneratesTypedSlotReadsAndExplicitEmits()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback ChildContent { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueSlotComponent<ChildSlots> Child = Vue3.DefineComponent(new VueSlotComponentOptions<ChildSlots>
                    {
                        Name = "ChildView",
                        Emits = ["ready"],
                        Setup = SetupChild
                    });

                    public static IVNode Render()
                        => H(Child, new ChildSlots
                        {
                            ChildContent = RenderBody
                        });

                    private static IVNode RenderBody()
                        => H("span", "body");

                    private static VueRenderCallback SetupChild(VueSetupContext<ChildSlots> context)
                    {
                        context.Emit("ready");
                        return () => H("section", context.Slots.ChildContent());
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let child = defineComponent({
  name: ""ChildView"",
  emits: [""ready""],
  setup: setupChild
});
export function render() {
  return h(child, { default: renderBody });
}
function renderBody() {
  return h(""span"", ""body"");
}
function setupChild(context) {
  context.emit(""ready"");
  return () => {
    return h(""section"", context.slots.default());
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingTypedVueSlotOnlyComponent_DoesNotInferRuntimeEmits()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback ChildContent { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueSlotComponent<ChildSlots> Child = Vue3.DefineComponent(new VueSlotComponentOptions<ChildSlots>
                    {
                        Name = "ChildView",
                        Setup = SetupChild
                    });

                    public static IVNode Render()
                        => H(Child, new ChildSlots
                        {
                            ChildContent = RenderBody
                        });

                    private static IVNode RenderBody()
                        => H("span", "body");

                    private static VueRenderCallback SetupChild(VueSetupContext<ChildSlots> context)
                    {
                        context.Emit("ready");
                        return () => H("section", context.Slots.ChildContent());
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let child = defineComponent({ name: ""ChildView"", setup: setupChild });
export function render() {
  return h(child, { default: renderBody });
}
function renderBody() {
  return h(""span"", ""body"");
}
function setupChild(context) {
  context.emit(""ready"");
  return () => {
    return h(""section"", context.slots.default());
  };
}
", script);
    }
    [TestMethod]
    public async Task Convert_ClassUsingTypedVueComponentSlots_GeneratesTypedSlotReadsAndWrites()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record ChildProps : VueProps
                {
                    [Description("@#title")]
                    public string? Title { get; init; }
                }

                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback ChildContent { get; init; } = default!;

                    [Description("@#header")]
                    public VueSlotCallback<string> Header { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent<ChildProps, ChildSlots> Child = Vue3.DefineComponent(new VueComponentOptions<ChildProps, ChildSlots>
                    {
                        Name = "ChildView",
                        Setup = SetupChild
                    });

                    public static IVNode Render()
                        => H(Child, new ChildProps { Title = "Welcome" }, new ChildSlots
                        {
                            ChildContent = RenderBody,
                            Header = RenderHeader
                        });

                    private static IVNode RenderBody()
                        => H("span", "body");

                    private static IVNode RenderHeader(string title)
                        => H("h1", title);

                    private static VueRenderCallback SetupChild(ChildProps props, VueSetupContext<ChildSlots> context)
                        => () => H("section", new IVNode[]
                        {
                            context.Slots.ChildContent(),
                            context.Slots.Header(props.Title ?? string.Empty)
                        });
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""npm:vue@3"";
export let child = defineComponent({ name: ""ChildView"", setup: setupChild });
export function render() {
  return h(child, { title: ""Welcome"" }, { default: renderBody, header: renderHeader });
}
function renderBody() {
  return h(""span"", ""body"");
}
function renderHeader(title) {
  return h(""h1"", title);
}
function setupChild(props, context) {
  return () => {
    return h(""section"", [context.slots.default(), context.slots.header(props.title ?? """")]);
  };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingConfiguredEmitsAttribute_WithNonLiteralEmitName_Throws()
    {
        var code = """
            using ECMAScript;
            using Jazor.ComplierTest;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record CounterProps : VueProps;

                [ECMAScriptModule("components/counter.mjs")]
                public static class CounterModule
                {
                    public static IVueComponent Component = Vue3.DefineComponent(new TestInheritedContractComponentOptions<CounterProps>
                    {
                        Name = "CounterView",
                        Setup = Setup
                    });

                    private static VueRenderCallback Setup(CounterProps props, VueSetupContext context)
                    {
                        var eventName = "ready";
                        context.Emit(eventName);
                        return () => H("button", "ready");
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "CounterModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(TestInheritedContractComponentOptions<>).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "CounterModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "[Emits]");
        StringAssert.Contains(exception.Message, "literal non-empty event names");
    }

    [TestMethod]
    public async Task Convert_ClassUsingEcmaScriptVueVuetifyProxy_GeneratesVuetifyImportsFromNameAttributes()
    {
        var code = """
            using ECMAScript;
            using ECMAScript.Vuetify;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static VueComponentPublicInstance Boot(IVueComponent component)
                    {
                        var app = Vue3.CreateApp(component);
                        app.Use(Vuetify.CreateVuetify(new VuetifyOptions
                        {
                            Components = new VuetifyComponentRegistry
                            {
                                VBtn = VuetifyComponents.VBtn
                            },
                            Directives = new VuetifyDirectiveRegistry
                            {
                                Ripple = VuetifyDirectives.Ripple
                            },
                            Theme = new VuetifyThemeOptions
                            {
                                DefaultTheme = "jazor"
                            }
                        }));
                        app.Component("v-btn", VuetifyComponents.VBtn);
                        app.Directive("ripple", VuetifyDirectives.Ripple);
                        return app.Mount("#app");
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vuetify.Vuetify).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { createApp } from ""npm:vue@3"";
import { createVuetify } from ""npm:vuetify"";
import { VBtn } from ""vuetify/components"";
import { Ripple } from ""vuetify/directives"";
export function boot(component) {
  let app = createApp(component);
  app.use(createVuetify({
    components: { VBtn: VBtn },
    directives: { Ripple: Ripple },
    theme: { defaultTheme: ""jazor"" }
  }));
  app.component(""v-btn"", VBtn);
  app.directive(""ripple"", Ripple);
  return app.mount(""#app"");
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueBaseRegistriesWithStringKeys_GeneratesRegistryObjectLiterals()
    {
        var code = """
            using ECMAScript;
            using ECMAScript.Vuetify;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static IVueComponent Create()
                        => Vue3.DefineComponent(new VueComponentOptions
                        {
                            Name = "ParentView",
                            Components = new VueComponentRegistry
                            {
                                ["ChildView"] = Vue3.DefineComponent(new VueComponentOptions
                                {
                                    Name = "ChildView"
                                })
                            },
                            Directives = new VueDirectiveRegistry
                            {
                                ["Ripple"] = VuetifyDirectives.Ripple
                            }
                        });
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vuetify.Vuetify).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent } from ""npm:vue@3"";
import { Ripple } from ""vuetify/directives"";
export function create() {
  return defineComponent({
    name: ""ParentView"",
    components: { ChildView: defineComponent({ name: ""ChildView"" }) },
    directives: { Ripple: Ripple }
  });
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueDirectiveObjectAuthoring_GeneratesDirectiveObjectLiteral()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static void Boot(IVueComponent component)
                    {
                        var app = Vue3.CreateApp(component);
                        app.Directive("focus", new VueDirective
                        {
                            Mounted = MountedDirective
                        });
                    }

                    private static void MountedDirective(Element element, VueDirectiveBinding binding, IVNode vnode)
                    {
                        element.SetAttribute("data-mounted", "true");
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { createApp } from ""npm:vue@3"";
export function boot(component) {
  let app = createApp(component);
  app.directive(""focus"", { mounted: mountedDirective });
}
function mountedDirective(element, binding, vnode) {
  element.setAttribute(""data-mounted"", ""true"");
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingTypedVueDirectiveInRegistry_GeneratesTypedDirectiveObjectLiteral()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Create()
                        => Vue3.DefineComponent(new VueComponentOptions
                        {
                            Name = "PanelView",
                            Directives = new VueDirectiveRegistry
                            {
                                ["Colorize"] = new VueDirective<string>
                                {
                                    Mounted = ApplyColor
                                }
                            }
                        });

                    private static void ApplyColor(Element element, VueDirectiveBinding<string> binding, IVNode vnode)
                    {
                        element.SetAttribute("data-color", binding.Value);
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent } from ""npm:vue@3"";
export function create() {
  return defineComponent({ name: ""PanelView"", directives: { Colorize: { mounted: applyColor } } });
}
function applyColor(element, binding, vnode) {
  element.setAttribute(""data-color"", binding.value);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueDirectiveFunctionShorthand_GeneratesDirectiveFunctionRegistration()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static void Boot(IVueComponent component)
                    {
                        var app = Vue3.CreateApp(component);
                        app.Directive("focus", ApplyFocus);
                    }

                    private static void ApplyFocus(Element element, VueDirectiveBinding binding)
                    {
                        element.SetAttribute("data-focus", "true");
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { createApp } from ""npm:vue@3"";
export function boot(component) {
  let app = createApp(component);
  app.directive(""focus"", applyFocus);
}
function applyFocus(element, binding) {
  element.setAttribute(""data-focus"", ""true"");
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingTypedVueDirectiveFunctionShorthand_GeneratesDirectiveFunctionRegistration()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static void Boot(IVueComponent component)
                    {
                        var app = Vue3.CreateApp(component);
                        app.Directive<string>("colorize", ApplyColor);
                    }

                    private static void ApplyColor(Element element, VueDirectiveBinding<string> binding)
                    {
                        element.SetAttribute("data-color", binding.Value);
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { createApp } from ""npm:vue@3"";
export function boot(component) {
  let app = createApp(component);
  app.directive(""colorize"", applyColor);
}
function applyColor(element, binding) {
  element.setAttribute(""data-color"", binding.value);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueDirectiveFunctionShorthandInRegistryCollectionInitializer_GeneratesDirectiveFunctionObjectLiteral()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Create()
                        => Vue3.DefineComponent(new VueComponentOptions
                        {
                            Name = "PanelView",
                            Directives = new VueDirectiveRegistry
                            {
                                { "Focus", ApplyFocus }
                            }
                        });

                    private static void ApplyFocus(Element element, VueDirectiveBinding binding)
                    {
                        element.SetAttribute("data-focus", "true");
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent } from ""npm:vue@3"";
export function create() {
  return defineComponent({ name: ""PanelView"", directives: { Focus: applyFocus } });
}
function applyFocus(element, binding) {
  element.setAttribute(""data-focus"", ""true"");
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueDirectiveRegistryCollectionInitializerWithDynamicKey_ThrowsOperationTransformationException()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Create()
                    {
                        var key = "Focus";
                        return Vue3.DefineComponent(new VueComponentOptions
                        {
                            Name = "PanelView",
                            Directives = new VueDirectiveRegistry
                            {
                                { key, ApplyFocus }
                            }
                        });
                    }

                    private static void ApplyFocus(Element element, VueDirectiveBinding binding)
                    {
                        element.SetAttribute("data-focus", "true");
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "unsupported dynamic object key");
        StringAssert.Contains(exception.Message, "ECMAScript.Vue3.VueDirectiveRegistry");
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueRenderDirectiveHelpers_GeneratesPlainVueHelperCalls()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    private static VueDirective Focus = new VueDirective
                    {
                        Mounted = MountedDirective
                    };

                    private static VueDirective<string> Colorize = new VueDirective<string>
                    {
                        Mounted = ApplyColor
                    };

                    public static IVNode Render()
                    {
                        var button = H("button", new VueObject
                        {
                            ["onClick"] = Vue3.WithModifiers(OnClick, "stop", "prevent")
                        }, "Save");
                        return Vue3.WithDirectives(
                            button,
                            new VueDirectiveArguments(Focus),
                            new VueDirectiveArguments<string>(Colorize, "red", "background", new VueDirectiveModifierBag
                            {
                                ["important"] = true
                            }));
                    }

                    private static void MountedDirective(Element element, VueDirectiveBinding binding, IVNode vnode)
                    {
                        element.SetAttribute("data-focus", "true");
                    }

                    private static void ApplyColor(Element element, VueDirectiveBinding<string> binding, IVNode vnode)
                    {
                        element.SetAttribute("data-color", binding.Value);
                    }

                    private static void OnClick()
                    {
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { h, withDirectives, withModifiers } from ""npm:vue@3"";
let focus = { mounted: mountedDirective };
let colorize = { mounted: applyColor };
export function render() {
  let button = h(""button"", { onClick: withModifiers(onClick, [""stop"", ""prevent""]) }, ""Save"");
  return withDirectives(button, [new Array(focus), new Array(colorize, ""red"", ""background"", { important: true })]);
}
function mountedDirective(element, binding, vnode) {
  element.setAttribute(""data-focus"", ""true"");
}
function applyColor(element, binding, vnode) {
  element.setAttribute(""data-color"", binding.value);
}
function onClick() { }
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingTypedVueDirectiveFunctionShorthandInRegistryCollectionInitializer_WithExplicitDelegateType_GeneratesDirectiveFunctionObjectLiteral()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent Create()
                        => Vue3.DefineComponent(new VueComponentOptions
                        {
                            Name = "PanelView",
                            Directives = new VueDirectiveRegistry
                            {
                                { "Colorize", (VueDirectiveFunction<string>)ApplyColor }
                            }
                        });

                    private static void ApplyColor(Element element, VueDirectiveBinding<string> binding)
                    {
                        element.SetAttribute("data-color", binding.Value);
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent } from ""npm:vue@3"";
export function create() {
  return defineComponent({ name: ""PanelView"", directives: { Colorize: applyColor } });
}
function applyColor(element, binding) {
  element.setAttribute(""data-color"", binding.value);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVuePluginOptionsStringKeys_GeneratesPluginOptionsObjectLiteral()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static void Boot(IVueComponent component, VuePlugin plugin)
                    {
                        var app = Vue3.CreateApp(component);
                        app.Use(plugin, new VuePluginOptions
                        {
                            ["feature"] = true,
                            ["theme"] = new VueDictionary
                            {
                                ["primary"] = "jazor"
                            }
                        });
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { createApp } from ""npm:vue@3"";
export function boot(component, plugin) {
  let app = createApp(component);
  app.use(plugin, { feature: true, theme: { primary: ""jazor"" } });
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVuePluginOptionsDynamicKey_ThrowsOperationTransformationException()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static void Boot(IVueComponent component, VuePlugin plugin)
                    {
                        var key = "feature";
                        var app = Vue3.CreateApp(component);
                        app.Use(plugin, new VuePluginOptions
                        {
                            [key] = true
                        });
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "unsupported dynamic object key");
        StringAssert.Contains(exception.Message, "ECMAScript.Vue3.VuePluginOptions");
    }

    [TestMethod]
    public async Task Convert_ClassUsingVuePluginObjectAuthoring_GeneratesPluginObjectLiteral()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                [ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static void Boot(IVueComponent component)
                    {
                        var app = Vue3.CreateApp(component);
                        app.Use(new VuePlugin
                        {
                            Install = InstallPlugin
                        });
                    }

                    private static void InstallPlugin(VueApp app)
                    {
                        app.Provide("featureEnabled", true);
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { createApp } from ""npm:vue@3"";
export function boot(component) {
  let app = createApp(component);
  app.use({ install: installPlugin });
}
function installPlugin(app) {
  app.provide(""featureEnabled"", true);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingTypedVuePluginCallbackOverload_GeneratesTypedPluginInstallCall()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record FeaturePluginOptions : VuePluginOptions
                {
                    public bool FeatureEnabled { get; init; }
                }

                [ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static void Boot(IVueComponent component)
                    {
                        var app = Vue3.CreateApp(component);
                        app.Use<FeaturePluginOptions>(InstallPlugin, new FeaturePluginOptions
                        {
                            FeatureEnabled = true
                        });
                    }

                    private static void InstallPlugin(VueApp app, FeaturePluginOptions options)
                    {
                        app.Provide("featureEnabled", options.FeatureEnabled);
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { createApp } from ""npm:vue@3"";
export function boot(component) {
  let app = createApp(component);
  app.use(installPlugin, { featureEnabled: true });
}
function installPlugin(app, options) {
  app.provide(""featureEnabled"", options.featureEnabled);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingTypedVuePluginObjectAuthoring_GeneratesTypedPluginObjectLiteral()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record FeaturePluginOptions : VuePluginOptions
                {
                    public bool FeatureEnabled { get; init; }
                }

                [ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static void Boot(IVueComponent component)
                    {
                        var app = Vue3.CreateApp(component);
                        app.Use(new VuePlugin<FeaturePluginOptions>
                        {
                            Install = InstallPlugin
                        }, new FeaturePluginOptions
                        {
                            FeatureEnabled = true
                        });
                    }

                    private static void InstallPlugin(VueApp app, FeaturePluginOptions options)
                    {
                        app.Provide("featureEnabled", options.FeatureEnabled);
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { createApp } from ""npm:vue@3"";
export function boot(component) {
  let app = createApp(component);
  app.use({ install: installPlugin }, { featureEnabled: true });
}
function installPlugin(app, options) {
  app.provide(""featureEnabled"", options.featureEnabled);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueP0CoverageBindings_GeneratesPlainVueImports()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record LocalState : VueProps
                {
                    [Description("@#value")]
                    public int Value { get; init; }
                }

                [ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static IVNode Boot(IVueComponent component, IVNode vnode, VueProps props, VueObject attrs, VueShallowRef<int> count)
                    {
                        var app = Vue3.CreateApp(component);
                        var runtimeVersion = Vue3.Version;
                        var appVersion = app.Version;
                        app.OnUnmount(Cleanup);
                        app.Mixin(new VueComponentOptions
                        {
                            Name = "SharedMixin"
                        });
                        var message = app.RunWithContext(GetMessage);
                        var merged = Vue3.MergeProps(props, attrs);
                        var cloned = Vue3.CloneVNode(vnode, merged);
                        var cloneOnly = Vue3.CloneVNode(cloned);
                        var isVNodeValue = Vue3.IsVNode(cloneOnly);
                        var isRefValue = Vue3.IsRef(count);
                        var current = Vue3.Unref(count);
                        Vue3.TriggerRef(count);
                        var state = Vue3.ShallowReactive(new LocalState { Value = current });
                        var readonlyState = Vue3.ShallowReadonly(state);
                        var raw = Vue3.ToRaw(readonlyState);
                        var stable = Vue3.MarkRaw(raw);
                        var proxy = Vue3.IsProxy(state);
                        var reactive = Vue3.IsReactive(state);
                        var readonlyFlag = Vue3.IsReadonly(readonlyState);
                        var hasContext = Vue3.HasInjectionContext();
                        var tick = Vue3.NextTick();
                        var tickWithCallback = Vue3.NextTick(Cleanup);
                        var setupAttrs = Vue3.UseAttrs();
                        var setupSlots = Vue3.UseSlots();
                        var panelRef = Vue3.UseTemplateRef<HTMLDivElement>("panel");
                        var generatedId = Vue3.UseId();
                        Vue3.WatchPostEffect(Cleanup);
                        Vue3.WatchSyncEffect(Cleanup);
                        Vue3.OnBeforeMount(Cleanup);
                        Vue3.OnBeforeUpdate(Cleanup);
                        Vue3.OnBeforeUnmount(Cleanup);
                        Vue3.OnErrorCaptured(CaptureError);
                        Vue3.OnActivated(Cleanup);
                        Vue3.OnDeactivated(Cleanup);
                        Vue3.OnRenderTracked(OnDebug);
                        Vue3.OnRenderTriggered(OnDebug);
                        Vue3.OnServerPrefetch(Prefetch);
                        var resolved = Vue3.ResolveComponent("ChildView");
                        var directive = Vue3.ResolveDirective("focus");
                        return cloneOnly;
                    }

                    private static string GetMessage() => "ready";

                    private static void Cleanup()
                    {
                    }

                    private static void OnDebug(VueDebuggerEvent @event)
                    {
                    }

                    private static bool CaptureError(VueValue? error, VueComponentPublicInstance? instance, string info)
                        => false;

                    private static IPromise Prefetch()
                        => Promise.Resolve();
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { cloneVNode, createApp, hasInjectionContext, isProxy, isReactive, isReadonly, isRef, isVNode, markRaw, mergeProps, nextTick, onActivated, onBeforeMount, onBeforeUnmount, onBeforeUpdate, onDeactivated, onErrorCaptured, onRenderTracked, onRenderTriggered, onServerPrefetch, resolveComponent, resolveDirective, shallowReactive, shallowReadonly, toRaw, triggerRef, unref, useAttrs, useId, useSlots, useTemplateRef, version, watchPostEffect, watchSyncEffect } from ""npm:vue@3"";
export function boot(component, vnode, props, attrs, count) {
  let app = createApp(component);
  let runtimeVersion = version;
  let appVersion = app.version;
  app.onUnmount(cleanup);
  app.mixin({ name: ""SharedMixin"" });
  let message = app.runWithContext(getMessage);
  let merged = mergeProps(props, attrs);
  let cloned = cloneVNode(vnode, merged);
  let cloneOnly = cloneVNode(cloned);
  let isVNodeValue = isVNode(cloneOnly);
  let isRefValue = isRef(count);
  let current = unref(count);
  triggerRef(count);
  let state = shallowReactive({ value: current });
  let readonlyState = shallowReadonly(state);
  let raw = toRaw(readonlyState);
  let stable = markRaw(raw);
  let proxy = isProxy(state);
  let reactive = isReactive(state);
  let readonlyFlag = isReadonly(readonlyState);
  let hasContext = hasInjectionContext();
  let tick = nextTick();
  let tickWithCallback = nextTick(cleanup);
  let setupAttrs = useAttrs();
  let setupSlots = useSlots();
  let panelRef = useTemplateRef(""panel"");
  let generatedId = useId();
  watchPostEffect(cleanup);
  watchSyncEffect(cleanup);
  onBeforeMount(cleanup);
  onBeforeUpdate(cleanup);
  onBeforeUnmount(cleanup);
  onErrorCaptured(captureError);
  onActivated(cleanup);
  onDeactivated(cleanup);
  onRenderTracked(onDebug);
  onRenderTriggered(onDebug);
  onServerPrefetch(prefetch);
  let resolved = resolveComponent(""ChildView"");
  let directive = resolveDirective(""focus"");
  return cloneOnly;
}
function getMessage() {
  return ""ready"";
}
function cleanup() { }
function onDebug(event) { }
function captureError(error, instance, info) {
  return false;
}
function prefetch() {
  return Promise.resolve();
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueCustomElementBindings_GeneratesPlainVueImportsAndOptions()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record BadgeProps : VueProps;

                [ECMAScriptModule("app/custom-elements.mjs")]
                public static class AppModule
                {
                    public static CustomElementConstructor Boot()
                    {
                        var elementCtor = Vue3.DefineCustomElement(new VueComponentOptions
                        {
                            Name = "UserBadge",
                            Render = Render
                        }, new VueCustomElementOptions
                        {
                            Styles = [":host { display: block; }"],
                            ConfigureApp = ConfigureElementApp,
                            ShadowRoot = false,
                            ShadowRootOptions = new ShadowRootInit
                            {
                                DelegatesFocus = true
                            },
                            Nonce = "nonce-1"
                        });
                        var typedCtor = Vue3.DefineCustomElement(new VueComponentOptions<BadgeProps>
                        {
                            Name = "TypedBadge",
                            Props = ["label"],
                            Emits = [],
                            Setup = Setup
                        });
                        var host = Vue3.UseHost();
                        var shadow = Vue3.UseShadowRoot();
                        Global.Window.CustomElements.Define("user-badge", elementCtor);
                        return elementCtor;
                    }

                    private static IVNode Render()
                        => H("span", "badge");

                    private static VueRenderCallback Setup(BadgeProps props, VueSetupContext context)
                        => Render;

                    private static void ConfigureElementApp(VueApp app)
                    {
                        app.Provide("configured", true);
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineCustomElement, h, useHost, useShadowRoot } from ""npm:vue@3"";
export function boot() {
  let elementCtor = defineCustomElement({ name: ""UserBadge"", render: render }, {
    styles: ["":host { display: block; }""],
    configureApp: configureElementApp,
    shadowRoot: false,
    shadowRootOptions: { delegatesFocus: true },
    nonce: ""nonce-1""
  });
  let typedCtor = defineCustomElement({
    name: ""TypedBadge"",
    props: [""label""],
    emits: [],
    setup: setup
  });
  let host = useHost();
  let shadow = useShadowRoot();
  window.customElements.define(""user-badge"", elementCtor);
  return elementCtor;
}
function render() {
  return h(""span"", ""badge"");
}
function setup(props, context) {
  return render;
}
function configureElementApp(app) {
  app.provide(""configured"", true);
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueCustomElementMergedOptions_GeneratesSingleArgumentDefineCustomElement()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record BadgeProps : VueProps
                {
                    [Description("@#label")]
                    public string? Label { get; init; }
                }

                [ECMAScriptModule("app/custom-elements.mjs")]
                public static class AppModule
                {
                    public static CustomElementConstructor Boot()
                    {
                        var elementCtor = Vue3.DefineCustomElement(new VueCustomElementComponentOptions<BadgeProps>
                        {
                            Name = "UserBadge",
                            Props = ["label"],
                            Emits = ["ready"],
                            Setup = Setup,
                            Styles = [":host { display: block; }"],
                            ConfigureApp = ConfigureElementApp,
                            ShadowRoot = false,
                            Nonce = "nonce-1"
                        });
                        var host = Vue3.UseHost<HTMLElement>();
                        Global.Window.CustomElements.Define("user-badge", elementCtor);
                        return elementCtor;
                    }

                    private static VueRenderCallback Setup(BadgeProps props, VueSetupContext context)
                    {
                        context.Emit("ready", props.Label ?? string.Empty);
                        return Render;
                    }

                    private static IVNode Render()
                        => H("span", "badge");

                    private static void ConfigureElementApp(VueApp app)
                    {
                        app.Provide("configured", true);
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { defineCustomElement, h, useHost } from ""npm:vue@3"";
export function boot() {
  let elementCtor = defineCustomElement({
    name: ""UserBadge"",
    props: [""label""],
    emits: [""ready""],
    setup: setup,
    styles: ["":host { display: block; }""],
    configureApp: configureElementApp,
    shadowRoot: false,
    nonce: ""nonce-1""
  });
  let host = useHost();
  window.customElements.define(""user-badge"", elementCtor);
  return elementCtor;
}
function setup(props, context) {
  context.emit(""ready"", props.label ?? """");
  return render;
}
function render() {
  return h(""span"", ""badge"");
}
function configureElementApp(app) {
  app.provide(""configured"", true);
}
", script);
    }

    [TestMethod]
    public async Task Convert_ClassUsingVueP1ReactivityBindings_GeneratesPlainVueImportsAndOptions()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using System;
            using static ECMAScript.Vue3;

            namespace Demo
            {
                public sealed record LocalState : VueProps
                {
                    [Description("@#value")]
                    public int Value { get; init; }
                }

                public abstract class LocalRefs : VueRefs<LocalState>
                {
                    [Description("@#value")]
                    public abstract IVueRef<int> Value { get; }
                }

                [ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static int Boot()
                    {
                        var count = Vue3.Ref(1);
                        var writable = Vue3.Computed(new Vue3.VueWritableComputedOptions<int>
                        {
                            Get = Read,
                            Set = Write
                        });
                        var readonlyCount = Vue3.Computed(Read);
                        var readonlyHandle = Vue3.Watch(readonlyCount, OnCountChanged);
                        var handle = Vue3.Watch(count, OnCountChanged, new Vue3.VueWatchOptions
                        {
                            Flush = Vue3.VueWatchFlush.Post,
                            Immediate = true,
                            Deep = 2,
                            Once = true,
                            OnTrack = OnDebug,
                            OnTrigger = OnDebug
                        });
                        handle.Pause();
                        handle.Resume();
                        handle.Stop();
                        var effect = Vue3.WatchEffect(RegisterCleanup, new Vue3.VueWatchEffectOptions
                        {
                            Flush = Vue3.VueWatchFlush.Sync,
                            OnTrack = OnDebug,
                            OnTrigger = OnDebug
                        });
                        var post = Vue3.WatchPostEffect(RegisterCleanup);
                        var sync = Vue3.WatchSyncEffect(RegisterCleanup);
                        Vue3.OnWatcherCleanup(Cleanup, true);
                        var custom = Vue3.CustomRef<int>(CreateCustomRef);
                        var normalizedRef = Vue3.ToValue(count);
                        var normalizedGetter = Vue3.ToValue<int>(Read);
                        var state = Vue3.Reactive(new LocalState { Value = normalizedRef });
                        var stateHandle = Vue3.Watch(state, OnStateChanged, new Vue3.VueWatchOptions
                        {
                            Deep = true
                        });
                        var multi = Vue3.Watch(new IVueRef<int>[] { count, count }, OnCountSourcesChanged, new Vue3.VueWatchOptions
                        {
                            Flush = Vue3.VueWatchFlush.Pre
                        });
                        var readonlyMulti = Vue3.Watch(new VueReadonlyRef<int>[] { readonlyCount }, OnCountSourcesChanged);
                        var getterMultiCleanup = Vue3.Watch(new Func<int>[] { Read, Read }, OnCountSourcesCleanup);
                        var linked = Vue3.ToRef<LocalState, int>(state, "value");
                        var linkedWithDefault = Vue3.ToRef<LocalState, int>(state, "missing", 10);
                        var normalizedPlainRef = Vue3.ToRef<int>(1);
                        var normalizedExistingRef = Vue3.ToRef<int>(count);
                        var normalizedGetterRef = Vue3.ToRef<int>(Read);
                        var refs = Vue3.ToRefs(state);
                        var typedRefs = Vue3.ToRefs<LocalRefs, LocalState>(state);
                        var propsRefs = Vue3.ToRefs<LocalRefs>(state);
                        Vue3.Provide("count", normalizedRef);
                        var injected = Vue3.Inject("count", 1);
                        Vue3.VueInjectionKey<int> countKey = Global.SymbolFn("count");
                        Vue3.Provide(countKey, normalizedRef);
                        var typedInjected = Vue3.Inject(countKey, Read, true);
                        var asyncChild = Vue3.DefineAsyncComponent(LoadChild);
                        var asyncWithOptions = Vue3.DefineAsyncComponent(new Vue3.VueAsyncComponentOptions
                        {
                            Loader = LoadChild,
                            Delay = 200,
                            Timeout = 3000,
                            Suspensible = false,
                            OnError = OnAsyncError
                        });
                        var scope = Vue3.EffectScope(true);
                        var scoped = scope.Run(Read);
                        scope.Stop();
                        var current = Vue3.GetCurrentScope();
                        Vue3.OnScopeDispose(Cleanup, true);
                        return writable.Value + custom.Value + injected + typedInjected + scoped + normalizedGetter;
                    }

                    private static int Read() => 1;

                    private static void Write(int value)
                    {
                    }

                    private static void OnCountChanged(int value, int oldValue)
                    {
                    }

                    private static void OnStateChanged(LocalState value, LocalState oldValue)
                    {
                    }

                    private static void OnCountSourcesChanged(int[] values, int[] oldValues)
                    {
                    }

                    private static void OnCountSourcesCleanup(int[] values, int[] oldValues, VueWatchCleanupRegistration onCleanup)
                    {
                        onCleanup(Cleanup);
                    }

                    private static void OnDebug(VueDebuggerEvent @event)
                    {
                    }

                    private static void RegisterCleanup(VueWatchCleanupRegistration onCleanup)
                    {
                        onCleanup(Cleanup);
                    }

                    private static Vue3.VueCustomRefHandlers<int> CreateCustomRef(Action track, Action trigger)
                    {
                        track();
                        trigger();
                        return new Vue3.VueCustomRefHandlers<int>
                        {
                            Get = Read,
                            Set = Write
                        };
                    }

                    private static IPromise<IVueComponent> LoadChild()
                    {
                        return default!;
                    }

                    private static void OnAsyncError(Error error, VueAsyncComponentRetryCallback retry, VueAsyncComponentRetryCallback fail, Number attempts)
                    {
                        retry();
                        fail();
                    }

                    private static void Cleanup()
                    {
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { computed, customRef, defineAsyncComponent, effectScope, getCurrentScope, inject, onScopeDispose, onWatcherCleanup, provide, reactive, ref, toRef, toRefs, toValue, watch, watchEffect, watchPostEffect, watchSyncEffect } from ""npm:vue@3"";
export function boot() {
  let count = ref(1);
  let writable = computed({ get: read, set: write });
  let readonlyCount = computed(read);
  let readonlyHandle = watch(readonlyCount, onCountChanged);
  let handle = watch(count, onCountChanged, {
    flush: ""post"",
    immediate: true,
    deep: 2,
    once: true,
    onTrack: onDebug,
    onTrigger: onDebug
  });
  handle.pause();
  handle.resume();
  handle.stop();
  let effect = watchEffect(registerCleanup, {
    flush: ""sync"",
    onTrack: onDebug,
    onTrigger: onDebug
  });
  let post = watchPostEffect(registerCleanup);
  let sync = watchSyncEffect(registerCleanup);
  onWatcherCleanup(cleanup, true);
  let custom = customRef(createCustomRef);
  let normalizedRef = toValue(count);
  let normalizedGetter = toValue(read);
  let state = reactive({ value: normalizedRef });
  let stateHandle = watch(state, onStateChanged, { deep: true });
  let multi = watch([count, count], onCountSourcesChanged, { flush: ""pre"" });
  let readonlyMulti = watch([readonlyCount], onCountSourcesChanged);
  let getterMultiCleanup = watch([read, read], onCountSourcesCleanup);
  let linked = toRef(state, ""value"");
  let linkedWithDefault = toRef(state, ""missing"", 10);
  let normalizedPlainRef = toRef(1);
  let normalizedExistingRef = toRef(count);
  let normalizedGetterRef = toRef(read);
  let refs = toRefs(state);
  let typedRefs = toRefs(state);
  let propsRefs = toRefs(state);
  provide(""count"", normalizedRef);
  let injected = inject(""count"", 1);
  let countKey = Symbol(""count"");
  provide(countKey, normalizedRef);
  let typedInjected = inject(countKey, read, true);
  let asyncChild = defineAsyncComponent(loadChild);
  let asyncWithOptions = defineAsyncComponent({
    loader: loadChild,
    delay: 200,
    timeout: 3000,
    suspensible: false,
    onError: onAsyncError
  });
  let scope = effectScope(true);
  let scoped = scope.run(read);
  scope.stop();
  let current = getCurrentScope();
  onScopeDispose(cleanup, true);
  return writable.value + custom.value + injected + typedInjected + scoped + normalizedGetter;
}
function read() {
  return 1;
}
function write(value) { }
function onCountChanged(value, oldValue) { }
function onStateChanged(value, oldValue) { }
function onCountSourcesChanged(values, oldValues) { }
function onCountSourcesCleanup(values, oldValues, onCleanup) {
  onCleanup(cleanup);
}
function onDebug(event) { }
function registerCleanup(onCleanup) {
  onCleanup(cleanup);
}
function createCustomRef(track, trigger) {
  track();
  trigger();
  return { get: read, set: write };
}
function loadChild() {
  return null;
}
function onAsyncError(error, retry, fail, attempts) {
  retry();
  fail();
}
function cleanup() { }
", script);
    }

    [TestMethod]
    public async Task Convert_ModuleStaticDomFieldInstancePropertyAccess_DoesNotDuplicateReceiver()
    {
        var code = """
            using ECMAScript;

            namespace Demo
            {
                [ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    private static HTMLTextAreaElement? _input;
                    private static HTMLElement? _output;

                    public static void RenderPreview()
                    {
                        if (_input is null || _output is null)
                            return;

                        var normalized = _input.Value;
                        _output.TextContent = normalized;
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "AppModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"let _input;
let _output;
export function renderPreview() {
  if (_input == null || _output == null)
    return;
  let normalized = _input.value;
  _output.textContent = normalized;
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleNamedComponentReference_GeneratesNamedImport()
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
                [ECMAScript.ECMAScriptModule("./components/wiki-home.mjs")]
                public static class WikiHomeModule
                {
                    public static object Component = null!;
                }

                [ECMAScript.ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static object Boot() => WikiHomeModule.Component;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code, "AppModule");
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
$@"import {{ component }} from ""./components/wiki-home.mjs"";
export function boot() {{
  return component;
}}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithDefaultExportField_Throws()
    {
        var code = """
            using System;
            using System.ComponentModel;

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
                [ECMAScript.ECMAScriptModule("./components/wiki-home.mjs")]
                public static class WikiHomeModule
                {
                    [Description("@#default")]
                    public static object Component = null;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code, "WikiHomeModule");
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "WikiHomeModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);
        StringAssert.Contains(exception.Message, "does not support default export");
        StringAssert.Contains(exception.Message, "Component");
    }

    [TestMethod]
    public async Task Convert_ClassWithMemberNamedDefault_Throws()
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
                [ECMAScript.ECMAScriptModule("./components/wiki-home.mjs")]
                public static class WikiHomeModule
                {
                    public static object Default = null;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code, "WikiHomeModule");
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "WikiHomeModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(moduleSymbol, semanticModel);
        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);
        StringAssert.Contains(exception.Message, "does not support default export");
        StringAssert.Contains(exception.Message, "Default");
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleDefaultExportReferenceFromProxyClass_Throws()
    {
        var code = """
            using System;
            using System.ComponentModel;

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
                [ECMAScript.ECMAScriptModule("./components/wiki-home.mjs")]
                public sealed class WikiHomeModule
                {
                    [Description("@#default")]
                    public static object Component = null!;
                }

                [ECMAScript.ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static object Boot() => WikiHomeModule.Component;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code, "AppModule");
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);
        StringAssert.Contains(exception.Message, "does not support default export");
        StringAssert.Contains(exception.Message, "Demo.WikiHomeModule");
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleDefaultAccessorReference_Throws()
    {
        var code = """
            using System;
            using System.ComponentModel;

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
                [ECMAScript.ECMAScriptModule("./components/wiki-home.mjs")]
                public static class WikiHomeModule
                {
                    public static object Component
                    {
                        [Description("@#default")]
                        get;
                    }
                }

                [ECMAScript.ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static object Boot() => WikiHomeModule.Component;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code, "AppModule");
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);
        StringAssert.Contains(exception.Message, "does not support default export");
        StringAssert.Contains(exception.Message, "Demo.WikiHomeModule");
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleDefaultConventionReference_Throws()
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
                [ECMAScript.ECMAScriptModule("./components/wiki-home.mjs")]
                public static class WikiHomeModule
                {
                    public static object Default = null!;
                }

                [ECMAScript.ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public static object Boot() => WikiHomeModule.Default;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code, "AppModule");
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var exception = await Assert.ThrowsAsync<NotSupportedException>(converter.Convert);
        StringAssert.Contains(exception.Message, "does not support default export");
        StringAssert.Contains(exception.Message, "Demo.WikiHomeModule");
    }

    [TestMethod]
    public async Task Convert_ClassWithCrossModuleStaticFieldReference_GeneratesLiveBindingImport()
    {
        // A module field is exported as a live ESM binding. Consumers must import the binding directly,
        // rather than manufacture an accessor that would change the source module's public contract.
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
                    public static int Value = 42;
                }

                [ECMAScript.ECMAScriptModule("System/ConsumerModule.js")]
                public static class ConsumerModule
                {
                    public static int Read() => RuntimeModule.Value;
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

        AssertScriptEqual(
            """
            import { value } from "System/RuntimeModule.js";
            export function read() {
              return value;
            }
            """ + "\n",
            script);
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
export function create() {
  return get_Value();
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithSameModuleStaticPropertyReferenceAndAssignment_UsesAccessorFunctions()
    {
        var code = """
            public static class TestClass
            {
                public static int Value { get; set; } = 42;

                public static int Read() => Value;

                public static void Write() => Value = 7;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code, "TestClass");
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"let _9d512e1fd4b4d93c = 42;
export function get_Value() {
  return _9d512e1fd4b4d93c;
}
export function set_Value(value) {
  _9d512e1fd4b4d93c = value;
}
export function read() {
  return get_Value();
}
export function write() {
  set_Value(7);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithNestedRecord_DoesNotEmitRuntimeDeclaration()
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
                [ECMAScript.ECMAScriptModule("app/main.mjs")]
                public static class AppModule
                {
                    public sealed record Point(int X, int Y);

                    public static object Create() => new Point(1, 2);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code, "AppModule");
        var appModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "AppModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = new AstConverter(appModule, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"export function create() {
  return { x: 1, y: 2 };
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
@"import { make } from ""System/RuntimeModule.js"";
export function create() {
  return make();
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
@"import { make } from ""System/RuntimeModule.js"";
export function create() {
  let factory = make;
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
export function create() {
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
export function create() {
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
export function set() {
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
export function create() {
  return Helpers.make();
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

        var makeId = ImportBindingName("System/RuntimeModule.js", "make");
        Assert.AreEqual(
$@"import {{ make as {makeId} }} from ""System/RuntimeModule.js"";
export function create() {{
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

        var rightMakeId = ImportBindingName("System/RightModule.js", "make");
        Assert.AreEqual(
$@"import {{ make }} from ""System/LeftModule.js"";
import {{ make as {rightMakeId} }} from ""System/RightModule.js"";
export function create() {{
  return make() + {rightMakeId}();
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
            if (specifier.StartsWith("make as ", StringComparison.Ordinal))
                return specifier.Substring("make as ".Length).Trim();

            return specifier;
        }

        var leftLocal = ParseLocalBinding(importLines[0]);
        var rightLocal = ParseLocalBinding(importLines[1]);
        StringAssert.Contains(script, $"return {rightLocal}() + {leftLocal}();");
    }

    [TestMethod]
    public async Task Convert_ClrRuntimeImportCatalog_EmitsCommentsOnlyForAuthoredImportMembers()
    {
        var code = """
            using System;

            namespace Demo
            {
                [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
                public sealed class JazorAttribute : Attribute
                {
                    public JazorAttribute() { }
                    public JazorAttribute(int op, string member) { }
                }

                [AttributeUsage(AttributeTargets.Method)]
                public sealed class Jazor : Attribute
                {
                    public Jazor(int op, string member) { }
                }

                public static class RuntimeModule
                {
                    [JazorAttribute(3, "static string.Compare(string, string)")]
                    public static int _e16eea9fe3891a62(string left, string right)
                    {
                        return left == right ? 0 : 1;
                    }

                    [JazorAttribute(3, "string.Length.get")]
                    public static int _26ea755ae0122f59 => 0;

                    [JazorAttribute]
                    public static int CompileOwned() => 0;

                    [JazorAttribute(2, "aliasOnly")]
                    public static int AliasOwned() => 0;

                    [JazorAttribute(3, "")]
                    public static int EmptyMember() => 0;

                    [Jazor(2, "legacyAliasOnly")]
                    public static int LegacyAliasOwned() => 0;

                    [Obsolete("documentation only")]
                    public static int Helper() => 0;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(code, "RuntimeModule");
        var clrConverter = new AstConverter(
            classSymbol,
            semanticModel,
            new AstConverterOptions(AstConverterProfile.ClrRuntime));

        var clrModule = await clrConverter.Convert();
        var clrScript = clrModule?.ToKnRECMAScript();

        Assert.IsNotNull(clrScript);
        var methodComment = "jazor:clr-member static string.Compare(string, string)";
        var methodCommentIndex = clrScript.IndexOf(methodComment, StringComparison.Ordinal);
        var methodDeclarationIndex = clrScript.IndexOf("export function _e16eea9fe3891a62", StringComparison.Ordinal);
        Assert.IsGreaterThanOrEqualTo(0, methodCommentIndex, clrScript);
        Assert.IsGreaterThan(methodCommentIndex, methodDeclarationIndex, clrScript);

        var propertyComment = "jazor:clr-member string.Length.get";
        var propertyCommentIndex = clrScript.IndexOf(propertyComment, StringComparison.Ordinal);
        var propertyDeclarationIndex = clrScript.IndexOf("export function get__26ea755ae0122f59", StringComparison.Ordinal);
        Assert.IsGreaterThanOrEqualTo(0, propertyCommentIndex, clrScript);
        Assert.IsGreaterThan(propertyCommentIndex, propertyDeclarationIndex, clrScript);

        Assert.AreEqual(2, clrScript.Split("jazor:clr-member", StringSplitOptions.None).Length - 1, clrScript);
        Assert.IsFalse(clrScript.Contains("jazor:clr-member aliasOnly", StringComparison.Ordinal), clrScript);

        var standardConverter = new AstConverter(classSymbol, semanticModel);
        var standardModule = await standardConverter.Convert();
        var standardScript = standardModule?.ToKnRECMAScript();

        Assert.IsNotNull(standardScript);
        Assert.IsFalse(standardScript.Contains("jazor:clr-member", StringComparison.Ordinal), standardScript);
    }

    [TestMethod]
    public async Task Convert_ClrRuntimeStyleNestedTypes_DoNotImportOwnModuleSymbols()
    {
        var code = """
            using System;
            using System.Collections.Generic;

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
                    private static T[] MaterializeArray<T>(IEnumerable<T> collection)
                    {
                        var result = new List<T>();
                        foreach (var item in collection)
                            result.Add(item);

                        return result.ToArray();
                    }

                    public sealed class JQueue<T>
                    {
                        public T[] Items { get; }

                        public JQueue()
                        {
                            Items = new T[0];
                        }

                        public JQueue(IEnumerable<T> collection)
                        {
                            Items = MaterializeArray(collection);
                        }

                        public static JQueue<T> WithCapacity(int capacity)
                        {
                            return new JQueue<T>();
                        }
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code);
        var runtimeModule = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "RuntimeModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();
        var converter = new AstConverter(runtimeModule, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        Assert.IsFalse(script.Contains("import {", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("export const RuntimeModule = {", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "get items() {");
        StringAssert.Contains(script, "= materializeArray(collection);");
        Assert.IsFalse(script.Contains("this.items =", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "return new JQueue(\"$ctor_");
    }

    [TestMethod]
    public async Task Convert_ClassWithDirectImportedRuntimeHelpers_PrunesUnusedHostTypeImport()
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
                    public static bool IsReadOnlyDictionaryCarrier(object instance) => instance is not null;

                    public static object MarkAsReadOnlyDictionaryCarrier(object instance) => instance;
                }

                [ECMAScript.ECMAScriptModule("System/ConsumerModule.js")]
                public static class ConsumerModule
                {
                    public static bool Check(object value) => RuntimeModule.IsReadOnlyDictionaryCarrier(value);

                    public static object Mark(object value) => RuntimeModule.MarkAsReadOnlyDictionaryCarrier(value);
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
        StringAssert.Contains(script, "import { isReadOnlyDictionaryCarrier, markAsReadOnlyDictionaryCarrier } from \"System/RuntimeModule.js\";");
        Assert.IsFalse(script.Contains("import { RuntimeModule", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("RuntimeModule,", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "return isReadOnlyDictionaryCarrier(value);");
        StringAssert.Contains(script, "return markAsReadOnlyDictionaryCarrier(value);");
    }

    [TestMethod]
    public async Task Convert_CrossModuleMemberClassSameArityConstructors_EmitBoundSelectors()
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
                    public sealed class JValue
                    {
                        public JValue(int value) { }
                        public JValue(string value) { }
                    }
                }

                [ECMAScript.ECMAScriptModule("System/ConsumerModule.js")]
                public static class ConsumerModule
                {
                    public static RuntimeModule.JValue CreateInt()
                        => new RuntimeModule.JValue(1);

                    public static RuntimeModule.JValue CreateString()
                        => new RuntimeModule.JValue("text");
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code, "ConsumerModule");
        var root = semanticModel.SyntaxTree.GetRoot();
        var consumer = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.Text == "ConsumerModule")
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();
        var valueType = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.Text == "JValue")
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();
        var intSelector = ConstructorHelperName(valueType, "int");
        var stringSelector = ConstructorHelperName(valueType, "string");
        var converter = new AstConverter(consumer, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "import { JValue } from \"System/RuntimeModule.js\";");
        StringAssert.Contains(script, $"return new JValue(\"{intSelector}\", 1);");
        StringAssert.Contains(script, $"return new JValue(\"{stringSelector}\", \"text\");");
    }

    [TestMethod]
    public async Task Convert_CrossModuleMemberClassTryCast_ImportsRuntimeTypeGuard()
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
                    public sealed class JDateTime
                    {
                    }
                }

                [ECMAScript.ECMAScriptModule("System/DateTimeModule.js")]
                public static class DateTimeModule
                {
                    public static RuntimeModule.JDateTime TryConvert(object value)
                        => value as RuntimeModule.JDateTime;
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(code, "DateTimeModule");
        var root = semanticModel.SyntaxTree.GetRoot();
        var moduleType = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.Text == "DateTimeModule")
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();
        var converter = new AstConverter(moduleType, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "import { JDateTime } from \"System/RuntimeModule.js\";");
        StringAssert.Contains(script, "return value instanceof JDateTime ? value : null;");
    }

    [TestMethod]
    public async Task Convert_ClrCarrierTypeChecks_EmitStableDedupedImportsIncludingSharedCarrier()
    {
        var code = """
            using System;
            using System.Globalization;

            public static class TestClass
            {
                public static bool IsDateTime(object value) => value is DateTime;

                public static bool IsDateTimeAgain(object value) => value is DateTime;

                public static bool IsCalendar(object value) => value is Calendar;

                public static bool IsGregorianCalendar(object value) => value is GregorianCalendar;
            }
            """;
        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(module);
        Assert.IsNotNull(script);
        var import = module.Body.OfType<ImportDeclaration>().Single();
        Assert.AreEqual("System/RuntimeModule.js", ((StringLiteral)import.Source).Value);
        var importedNames = import.Specifiers
            .OfType<ImportSpecifier>()
            .Select(static specifier => ((Identifier)specifier.Imported).Name)
            .ToArray();
        CollectionAssert.AreEqual(new[] { "JDateTime", "JGregorianCalendar" }, importedNames);
        AssertContainsCount(script, "value instanceof JDateTime", 2);
        AssertContainsCount(script, "value instanceof JGregorianCalendar", 2);
        Assert.IsFalse(script.Contains("value.date", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("value.kind", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("value.items", StringComparison.Ordinal), script);
        _ = new Acornima.Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_ClrCarrierTypeCheck_WhenCarrierNameIsShadowed_UsesStableImportAlias()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static bool IsDateTime(object value)
                {
                    var JDateTime = 1;
                    return JDateTime > 0 && value is DateTime;
                }
            }
            """;
        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        var carrierBinding = ImportBindingName("System/RuntimeModule.js", "JDateTime");
        StringAssert.Contains(script, $"import {{ JDateTime as {carrierBinding} }} from \"System/RuntimeModule.js\";");
        StringAssert.Contains(script, $"value instanceof {carrierBinding}");
        _ = new Acornima.Parser().ParseModule(script);
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
    public async Task Convert_EcmascriptInlineOperator_UsesInlineExpressionTemplate()
    {
        var code = """
            using ECMAScript;

            [ECMAScript]
            public sealed class Length
            {
                [ECMAScriptInline("`calc(${__arg1} - ${__arg2})`")]
                public static extern Length operator -(Length left, Length right);
            }

            [ECMAScriptModule("typed-values.mjs")]
            public static class TypedValueModule
            {
                public static Length Subtract(Length left, Length right) => left - right;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TypedValueModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptInlineAttribute).Assembly.Location));
        var module = await new AstConverter(classSymbol, semanticModel).Convert();

        AssertScriptEqual(
            """
            export function subtract(left, right) {
              return `calc(${left} - ${right})`;
            }

            """,
            module?.ToKnRECMAScript());
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
@"export function check() {
  return { value: 1 } === { value: 1 };
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task Convert_ClassWithRepeatedGuidCalls_EmitsOneGuidModuleImportWithUniqueSpecifiers()
    {
        var code = """
            using System;

            public static class TestClass
            {
                public static string ParseCompact(string input)
                    => Guid.Parse(input).ToString("N");

                public static Guid ParseAgain(string input)
                    => Guid.Parse(input);
            }
            """;
        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();

        Assert.IsNotNull(module);
        var import = module.Body.OfType<ImportDeclaration>().Single();
        Assert.AreEqual("System/GuidModule.js", ((StringLiteral)import.Source).Value);
        var importedNames = import.Specifiers
            .OfType<ImportSpecifier>()
            .Select(static specifier => ((Identifier)specifier.Imported).Name)
            .ToArray();
        Assert.HasCount(2, importedNames);
        Assert.HasCount(importedNames.Length, importedNames.Distinct(StringComparer.Ordinal));
        _ = new Acornima.Parser().ParseModule(module.ToKnRECMAScript());
    }

    [TestMethod]
    public async Task Convert_CurrentModuleImportTarget_ReusesDeclaredLocalBinding()
    {
        var code = """
            using ECMAScript;

            [ECMAScriptModule("System/StringModule.js")]
            public static class TestClass
            {
                public static string _5ad63706a889c294(string instance, int index)
                    => instance.Substring(index, 1);

                public static char ReadFirst(string value) => value[0];
            }
            """;
        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(module);
        Assert.IsEmpty(module.Body.OfType<ImportDeclaration>());
        StringAssert.Contains(script, "return _5ad63706a889c294(value, 0);");
        _ = new Acornima.Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_CurrentModuleImportWithoutLocalBinding_ThrowsActionableDiagnostic()
    {
        var code = """
            using ECMAScript;

            [ECMAScriptModule("System/StringModule.js")]
            public static class TestClass
            {
                public static char ReadFirst(string value) => value[0];
            }
            """;
        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var exception = await Assert.ThrowsAsync<NotSupportedException>(() => converter.Convert());

        StringAssert.Contains(exception.Message, "_5ad63706a889c294");
        StringAssert.Contains(exception.Message, "System/StringModule.js");
        StringAssert.Contains(exception.Message, "does not declare a matching local binding");
    }

    [TestMethod]
    public async Task Convert_ClassWithConditionalAccessSequenceFieldInitializer_EmitsParseableModule()
    {
        var code = """
            public static class TestClass
            {
                public static string? Value = GetValue()?.Trim()?.ToLower();

                private static string? GetValue() => null;
            }
            """;
        var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let value = (");
        _ = new Acornima.Parser().ParseModule(script);
    }

    #endregion
}
