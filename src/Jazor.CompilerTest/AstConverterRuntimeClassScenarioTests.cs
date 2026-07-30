using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class AstConverterRuntimeClassScenarioTests
{
    [TestMethod]
    public void ConvertRuntimeClass_ModuleDeclaredBase_UsesExtendsAndSynthesizesSuper()
    {
        const string scenarioId = "ast-converter-runtime-class.module-base-implicit-super";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public class Base
                {
                    public Base()
                    {
                    }
                }

                public sealed class Derived : Base
                {
                }
            }
            """,
            scenarioId);
        var derived = fixture.GetType("Derived");
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var declaration = converter.ConvertRuntimeClass(derived);
        var script = declaration.ToKnRECMAScript();

        Assert.AreEqual("Derived", declaration.Id?.Name, scenarioId);
        StringAssert.Contains(script, "class Derived extends Base", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "super()", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void ConvertRuntimeClass_OverloadedModuleBase_PassesConstructorHelperSelector()
    {
        const string scenarioId = "ast-converter-runtime-class.overloaded-module-base";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public class Base
                {
                    public Base()
                    {
                    }

                    public Base(int mode)
                    {
                    }
                }

                public sealed class Derived : Base
                {
                }
            }
            """,
            scenarioId);
        var baseDefaultConstructor = fixture.GetType("Base").InstanceConstructors
            .Single(static constructor => !constructor.IsImplicitlyDeclared && constructor.Parameters.Length == 0);
        var derived = fixture.GetType("Derived");
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var declaration = converter.ConvertRuntimeClass(derived);
        var script = declaration.ToKnRECMAScript();
        var expectedHelper = Util.GetMemberConstructorHelperName(baseDefaultConstructor);

        Assert.AreEqual("Derived", declaration.Id?.Name, scenarioId);
        StringAssert.Contains(script, "class Derived extends Base", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, $"super(\"{expectedHelper}\")", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void ConvertRuntimeClass_ExternalBase_RejectsUnboundInheritance()
    {
        const string scenarioId = "ast-converter-runtime-class.external-base-rejected";
        var fixture = CompileModule(
            """
            public class ExternalBase
            {
            }

            public static class TestModule
            {
                public sealed class Derived : ExternalBase
                {
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var exception = Assert.ThrowsExactly<NotSupportedException>(
            () => converter.ConvertRuntimeClass(fixture.GetType("Derived")));

        StringAssert.Contains(exception.Message, "runtime class does not support inheritance", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(exception.Message, "Derived : ExternalBase", StringComparison.Ordinal, scenarioId);
    }

    [TestMethod]
    public async Task Convert_RazorVueRuntime_PredeclaredNestedHelperIsFlattenedToArtifactScope()
    {
        const string scenarioId = "ast-converter-runtime-class.razorvue-predeclared-nested-helper";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class Host
                {
                    public sealed class RenderHelper
                    {
                    }
                }
            }
            """,
            scenarioId);
        var helper = fixture.GetType("Host").GetTypeMembers("RenderHelper").Single();
        var declaredNames = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default)
        {
            [helper.OriginalDefinition] = "renderHelper"
        };
        var converter = new AstConverter(
            fixture.Module,
            fixture.SemanticModel,
            new AstConverterOptions(
                AstConverterProfile.RazorVueRuntime,
                DeclaredNames: declaredNames));

        var module = await converter.Convert();
        var helperDeclaration = converter.ConvertRuntimeClass(helper);

        Assert.IsNotNull(module, scenarioId);
        var exportedHost = module!.Body.OfType<ExportNamedDeclaration>().Single();
        Assert.IsInstanceOfType<ClassDeclaration>(exportedHost.Declaration, scenarioId);
        Assert.AreEqual("Host", ((ClassDeclaration)exportedHost.Declaration).Id?.Name, scenarioId);
        Assert.IsFalse(
            module.ToKnRECMAScript().Contains("RenderHelper", StringComparison.Ordinal),
            scenarioId);
        Assert.AreEqual("renderHelper", helperDeclaration.Id?.Name, scenarioId);
        _ = new Parser().ParseModule(module.ToKnRECMAScript());
        _ = new Parser().ParseScript(helperDeclaration.ToKnRECMAScript());
    }

    [TestMethod]
    public void ConvertRuntimeClass_CanceledToken_StopsBeforeLowering()
    {
        const string scenarioId = "ast-converter-runtime-class.canceled-token";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class Worker
                {
                }
            }
            """,
            scenarioId);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        _ = Assert.ThrowsExactly<OperationCanceledException>(
            () => converter.ConvertRuntimeClass(fixture.GetType("Worker"), cancellationSource.Token),
            scenarioId);
    }

    private static RuntimeClassFixture CompileModule(string source, string scenarioId)
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "AstConverterRuntimeClassScenario.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "AstConverterRuntimeClassScenarios_" + Guid.NewGuid().ToString("N"),
            syntaxTrees: [sourceTree],
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(
            0,
            errors,
            $"{scenarioId}:{Environment.NewLine}" +
            string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var semanticModel = compilation.GetSemanticModel(sourceTree);
        var module = sourceTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.ValueText == "TestModule")
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();
        return new RuntimeClassFixture(module, semanticModel);
    }

    private sealed record RuntimeClassFixture(
        INamedTypeSymbol Module,
        SemanticModel SemanticModel)
    {
        public INamedTypeSymbol GetType(string name)
            => Module.GetTypeMembers(name).Single();
    }
}
