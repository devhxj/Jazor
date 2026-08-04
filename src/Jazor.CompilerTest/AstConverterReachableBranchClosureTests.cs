using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

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

    [TestMethod]
    public async Task Convert_RuntimeClassInitOnlyAutoPropertyAndImplicitFields_UsePrivateStorageDefaults()
    {
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class ReleaseSession
                {
                    private string _name;
                    private decimal _budget;
                    private (int Attempts, long Revision) _state;

                    public int RetryCount { get; init; }

                    public ReleaseSession()
                    {
                        RetryCount = 1;
                    }
                }
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();
        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();

        StringAssert.Contains(script, "class ReleaseSession", StringComparison.Ordinal);
        StringAssert.Contains(script, "constructor()", StringComparison.Ordinal);
        StringAssert.Contains(script, "= 1", StringComparison.Ordinal);
        StringAssert.Contains(script, "#_name = null", StringComparison.Ordinal);
        StringAssert.Contains(script, "= {", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public void Visit_ModuleContextInitOnlyAutoPropertyAssignment_UsesItsPrivateBackingField()
    {
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class ReleaseSession
                {
                    public int Revision { get; init; }

                    public ReleaseSession()
                    {
                        Revision = 2;
                    }
                }
            }
            """);
        var session = fixture.GetType("ReleaseSession");
        var constructor = session.DeclaringSyntaxReferences
            .Select(static reference => reference.GetSyntax())
            .OfType<ClassDeclarationSyntax>()
            .SelectMany(static declaration => declaration.Members.OfType<ConstructorDeclarationSyntax>())
            .Single();
        var operation = fixture.SemanticModel.GetOperation(constructor.Body!)!;
        var declaredNames = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default)
        {
            [session] = "ReleaseSession"
        };

        var script = new SemanticWalker(session, declaredNames)
            .Visit(operation, new SenseArgument())?
            .ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "this.#", StringComparison.Ordinal);
        StringAssert.Contains(script, "= 2", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task Convert_RuntimeClassPropertyWrites_DistinguishInitOnlyAndMutableInstanceContracts()
    {
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class ReleaseSession
                {
                    public int Revision { get; init; }
                    public int Retries { get; set; }
                    public ReleaseSession()
                    {
                        Revision = 2;
                        Retries = 3;
                    }
                }
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();
        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();

        StringAssert.Contains(script, "this.#", StringComparison.Ordinal);
        StringAssert.Contains(script, "= 2", StringComparison.Ordinal);
        StringAssert.Contains(script, "= 3", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_ModuleObjectInitializer_UsesThePublicInitContractOutsideTheOwningConstructor()
    {
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class ReleaseSession
                {
                    public int Revision { get; init; }
                }

                public static ReleaseSession Create()
                {
                    return new ReleaseSession { Revision = 3 };
                }
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();
        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();

        StringAssert.Contains(script, "revision", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(script, "= 3", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_RuntimeClassImplicitFieldDefaults_PreserveScalarAndNullableCarrierKinds()
    {
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class ReleaseState
                {
                    private bool _enabled;
                    private char _marker;
                    private long _revision;
                    private int? _retryAfter;
                }
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();
        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();

        StringAssert.Contains(script, "= false", StringComparison.Ordinal);
        StringAssert.Contains(script, "= \"\\0\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "= 0n", StringComparison.Ordinal);
        StringAssert.Contains(script, "= null", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_RuntimeInheritance_UsesBoundBaseConstructorSelectorsAndArguments()
    {
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public class ReleaseBase
                {
                    public ReleaseBase() { }

                    public ReleaseBase(int revision) { }
                }

                public sealed class DefaultRelease : ReleaseBase
                {
                    public DefaultRelease() : base() { }
                }

                public sealed class VersionedRelease : ReleaseBase
                {
                    public VersionedRelease() : base(7) { }
                }

                public sealed class ImplicitRelease : ReleaseBase
                {
                }
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();
        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();

        StringAssert.Contains(script, "class ReleaseBase", StringComparison.Ordinal);
        StringAssert.Contains(script, "extends ReleaseBase", StringComparison.Ordinal);
        StringAssert.Contains(script, "super(\"$ctor_", StringComparison.Ordinal);
        StringAssert.Contains(script, ", 7)", StringComparison.Ordinal);
        StringAssert.Contains(script, "class ImplicitRelease extends ReleaseBase", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_RuntimeClassStaticConstructor_RejectsTheUnsupportedInitializationProtocol()
    {
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class ReleaseSession
                {
                    public static int Current { get; set; }

                    static ReleaseSession()
                    {
                        Current = 1;
                    }
                }
            }
            """);

        var exception = await Assert.ThrowsExactlyAsync<NotSupportedException>(async () =>
            await new AstConverter(fixture.Module, fixture.SemanticModel).Convert());

        StringAssert.Contains(exception.Message, "static constructor", StringComparison.Ordinal);
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
