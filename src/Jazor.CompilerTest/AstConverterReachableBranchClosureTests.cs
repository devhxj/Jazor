using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class AstConverterReachableBranchClosureTests
{
    [TestMethod]
    public void ConvertRuntimeClass_ExpressionBodiedConstructorAndEmptyMethod_EmitStableBodies()
    {
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class Worker
                {
                    private int _value;

                    public Worker() => _value = 1;

                    public void Reset() { }
                }
            }
            """);

        var declaration = new AstConverter(fixture.Module, fixture.SemanticModel)
            .ConvertRuntimeClass(fixture.GetType("Worker"));
        var script = declaration.ToKnRECMAScript();

        StringAssert.Contains(script, "constructor()", StringComparison.Ordinal);
        StringAssert.Contains(script, "this.#_value = 1", StringComparison.Ordinal);
        StringAssert.Contains(script, "reset()", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void ConvertRuntimeClass_ExternalBaseType_RejectsAnUnownedInheritanceProtocol()
    {
        var fixture = CompileModule(
            """
            using System.Collections.Generic;

            public static class TestModule
            {
                public sealed class ExternalDerived : List<int> { }
            }
            """);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var exception = Assert.ThrowsExactly<NotSupportedException>(() =>
            converter.ConvertRuntimeClass(fixture.GetType("ExternalDerived")));

        StringAssert.Contains(exception.Message, "does not support inheritance", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "List<int>", StringComparison.Ordinal);
    }

    [TestMethod]
    public void ConvertRuntimeClass_RefBaseConstructorInitializer_RejectsUnsupportedArgumentProtocol()
    {
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public class Base
                {
                    public Base(ref int value) => value++;
                }

                public sealed class Derived : Base
                {
                    public Derived(ref int value) : base(ref value) { }
                }
            }
            """);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var exception = Assert.ThrowsExactly<NotSupportedException>(() =>
            converter.ConvertRuntimeClass(fixture.GetType("Derived")));

        StringAssert.Contains(exception.Message, "ref/out constructor initializer arguments", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task Convert_ModuleEventHandlers_UnwrapStaticAndLocalMethodGroupSyntax()
    {
        var fixture = CompileModule(
            """
            using System;

            public static class TestModule
            {
                private static void Apply() { }

                public sealed class Emitter
                {
                    public event Action? Changed;

                    public void Wire()
                    {
                        static void Refresh() { }

                        Changed += (Action)Apply;
                        Changed -= (Apply);
                        Changed += Refresh;
                    }
                }
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();
        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();

        StringAssert.Contains(script, "$eventHandler", StringComparison.Ordinal);
        StringAssert.Contains(script, "apply", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "Refresh", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    private static ModuleFixture CompileModule(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "AstConverterReachableBranchClosure.cs");
        var compilation = CSharpCompilation.Create(
            "AstConverterReachableBranchClosure_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var module = syntaxTree.GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.ValueText == "TestModule")
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();
        return new ModuleFixture(module, semanticModel);
    }

    private sealed record ModuleFixture(INamedTypeSymbol Module, SemanticModel SemanticModel)
    {
        public INamedTypeSymbol GetType(string name)
            => Module.GetTypeMembers(name).Single();
    }
}
