using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class AstConverterReachableBranchClosureTests
{
    [TestMethod]
    public async Task Convert_NonBindingExportNames_UseSourceBindingsAndPreserveConfiguredExports()
    {
        var fixture = CompileModule(
            """
            using ECMAScript;

            [ECMAScriptModule("release")]
            public static class TestModule
            {
                [ECMAScriptName("release-count")]
                public static int ReleaseCount = 1;

                [ECMAScriptName("release-work")]
                public static int ReleaseWork() => ReleaseCount + 1;

                [ECMAScriptName("release-worker")]
                public sealed class ReleaseWorker
                {
                }

                public static int Run() => ReleaseWork();
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();
        StringAssert.Contains(script, "export { ReleaseCount as \"release-count\" };", StringComparison.Ordinal);
        StringAssert.Contains(script, "export { ReleaseWork as \"release-work\" };", StringComparison.Ordinal);
        StringAssert.Contains(script, "export { ReleaseWorker as \"release-worker\" };", StringComparison.Ordinal);
        StringAssert.Contains(script, "return ReleaseWork();", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_NonBindingAccessorName_UsesThePropertySourceNameForItsLocalBinding()
    {
        var fixture = CompileModule(
            """
            using ECMAScript;

            [ECMAScriptModule("release")]
            public static class TestModule
            {
                public static int ReleaseValue
                {
                    [ECMAScriptName("release-value")]
                    get => 5;
                }

                public static int Read() => ReleaseValue;
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();
        StringAssert.Contains(script, "function ReleaseValue()", StringComparison.Ordinal);
        StringAssert.Contains(script, "return ReleaseValue();", StringComparison.Ordinal);
        Assert.DoesNotContain("function release-value", script, StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_AutoPropertyBackingFieldNameCollision_UsesAStablePrivateBinding()
    {
        var initialFixture = CompileModule(
            """
            using ECMAScript;

            [ECMAScriptModule("release")]
            public static class TestModule
            {
                public static int ReleaseValue { get; } = 5;
            }
            """);
        var property = initialFixture.Module.GetMembers("ReleaseValue").OfType<IPropertySymbol>().Single();
        var backingFieldName = Format.HashName(property.OriginalDefinition.ToDisplayString(Format.NameFormat));
        var fixture = CompileModule(
            $$"""
            using ECMAScript;

            [ECMAScriptModule("release")]
            public static class TestModule
            {
                [ECMAScriptName("{{backingFieldName}}")]
                public static int Collision() => 1;

                public static int ReleaseValue { get; } = 5;

                public static int Read() => Collision() + ReleaseValue;
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();
        StringAssert.Contains(script, $"function {backingFieldName}()", StringComparison.Ordinal);
        StringAssert.Contains(script, "function get_ReleaseValue()", StringComparison.Ordinal);
        StringAssert.Contains(script, $"return {backingFieldName}() + get_ReleaseValue();", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_ImportAliasCollidingWithConfiguredModuleBindings_AllocatesTheFirstAvailableStableSuffix()
    {
        var aliasBase = $"i${Format.HashName("runtime\0Make").TrimStart('_')}";
        var fixture = CompileModule(
            $$"""
            using ECMAScript;
            using Runtime = Demo.RuntimeModule;

            namespace Demo
            {
                [ECMAScriptModule("runtime")]
                public static class RuntimeModule
                {
                    public static int Make() => 1;
                }

                [ECMAScriptModule("consumer")]
                public static class TestModule
                {
                    [ECMAScriptName("Make")]
                    public static int LocalMake() => 2;

                    [ECMAScriptName("{{aliasBase}}")]
                    public static int ReservedGeneratedAlias() => 3;

                    [ECMAScriptName("{{aliasBase}}1")]
                    public static int ReservedGeneratedAliasSuffix() => 4;

                    public static int Combine() => LocalMake() + Runtime.Make();
                }
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module);
        var import = module.Body.OfType<ImportDeclaration>().Single();
        var specifier = import.Specifiers.OfType<ImportSpecifier>().Single();
        Assert.AreEqual("Make", ((Identifier)specifier.Imported).Name);
        Assert.AreEqual(aliasBase + "2", specifier.Local.Name);

        var script = module.ToKnRECMAScript();
        StringAssert.Contains(script, $"import {{ Make as {aliasBase}2 }} from \"runtime\";", StringComparison.Ordinal);
        StringAssert.Contains(script, $"return Make() + {aliasBase}2();", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_ReservedExternalExportName_UsesAnAliasedImportBinding()
    {
        var fixture = CompileModule(
            """
            using ECMAScript;
            using Runtime = Demo.RuntimeModule;

            namespace Demo
            {
                [ECMAScriptModule("runtime")]
                public static class RuntimeModule
                {
                    [ECMAScriptName("class")]
                    public static int Make() => 1;
                }

                [ECMAScriptModule("consumer")]
                public static class TestModule
                {
                    public static int Read() => Runtime.Make();
                }
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module);
        var import = module.Body.OfType<ImportDeclaration>().Single();
        var specifier = import.Specifiers.OfType<ImportSpecifier>().Single();
        Assert.AreEqual("class", ((Identifier)specifier.Imported).Name);
        Assert.AreNotEqual("class", specifier.Local.Name);

        var script = module.ToKnRECMAScript();
        StringAssert.Contains(script, $"import {{ class as {specifier.Local.Name} }} from \"runtime\";", StringComparison.Ordinal);
        StringAssert.Contains(script, $"return {specifier.Local.Name}();", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_ImportAliasOccupiedByAnotherExternalBinding_AllocatesAStableSuffix()
    {
        var aliasBase = $"i${Format.HashName("runtime\0Make").TrimStart('_')}";
        var fixture = CompileModule(
            $$"""
            using ECMAScript;
            using AliasOwner = Demo.AliasOwnerModule;
            using Left = Demo.LeftModule;
            using Runtime = Demo.RuntimeModule;

            namespace Demo
            {
                [ECMAScriptModule("alias-owner")]
                public static class AliasOwnerModule
                {
                    [ECMAScriptName("{{aliasBase}}")]
                    public static int Read() => 1;
                }

                [ECMAScriptModule("left")]
                public static class LeftModule
                {
                    public static int Make() => 2;
                }

                [ECMAScriptModule("runtime")]
                public static class RuntimeModule
                {
                    public static int Make() => 3;
                }

                [ECMAScriptModule("consumer")]
                public static class TestModule
                {
                    public static int Combine() => AliasOwner.Read() + Left.Make() + Runtime.Make();
                }
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module);
        var imports = module.Body.OfType<ImportDeclaration>().ToArray();
        var occupiedBinding = imports
            .SelectMany(static import => import.Specifiers.OfType<ImportSpecifier>())
            .Single(specifier => specifier.Imported is Identifier identifier && identifier.Name == aliasBase);
        var runtimeMake = imports
            .Single(import => ((StringLiteral)import.Source).Value == "runtime")
            .Specifiers
            .OfType<ImportSpecifier>()
            .Single();
        Assert.AreEqual(aliasBase, occupiedBinding.Local.Name);
        Assert.AreEqual(aliasBase + "1", runtimeMake.Local.Name);

        var script = module.ToKnRECMAScript();
        StringAssert.Contains(script, $"import {{ Make as {aliasBase}1 }} from \"runtime\";", StringComparison.Ordinal);
        StringAssert.Contains(script, $"return {aliasBase}() + Make() + {aliasBase}1();", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_StringExportImportAliasCollidingWithConfiguredBindings_UsesTheFirstFreeStableSuffix()
    {
        var aliasBase = $"i${Format.HashName("runtime\0release-work").TrimStart('_')}";
        var fixture = CompileModule(
            $$"""
            using ECMAScript;
            using Runtime = Demo.RuntimeModule;

            namespace Demo
            {
                [ECMAScriptModule("runtime")]
                public static class RuntimeModule
                {
                    [ECMAScriptName("release-work")]
                    public static int ReleaseWork() => 1;
                }

                [ECMAScriptModule("consumer")]
                public static class TestModule
                {
                    [ECMAScriptName("{{aliasBase}}")]
                    public static int ReservedGeneratedAlias() => 2;

                    [ECMAScriptName("{{aliasBase}}1")]
                    public static int ReservedGeneratedAliasSuffix() => 3;

                    public static int Read() => Runtime.ReleaseWork();
                }
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module);
        var specifier = module.Body
            .OfType<ImportDeclaration>()
            .Single()
            .Specifiers
            .OfType<ImportSpecifier>()
            .Single();
        Assert.IsInstanceOfType<StringLiteral>(specifier.Imported);
        Assert.AreEqual("release-work", ((StringLiteral)specifier.Imported).Value);
        Assert.AreEqual(aliasBase + "2", specifier.Local.Name);

        var script = module.ToKnRECMAScript();
        StringAssert.Contains(script, $"import {{ \"release-work\" as {aliasBase}2 }} from \"runtime\";", StringComparison.Ordinal);
        StringAssert.Contains(script, $"return {aliasBase}2();", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_ImportedMemberShadowedByLocalAndParameterBindings_UsesOneStableAliasAcrossMethods()
    {
        var alias = $"i${Format.HashName("runtime\0ReleaseWork").TrimStart('_')}";
        var fixture = CompileModule(
            """
            using ECMAScript;
            using Runtime = Demo.RuntimeModule;

            namespace Demo
            {
                [ECMAScriptModule("runtime")]
                public static class RuntimeModule
                {
                    public static int ReleaseWork() => 1;
                }

                [ECMAScriptModule("consumer")]
                public static class TestModule
                {
                    public static int FromLocal()
                    {
                        var ReleaseWork = 2;
                        return ReleaseWork + Runtime.ReleaseWork();
                    }

                    public static int FromParameter(int ReleaseWork)
                        => ReleaseWork + Runtime.ReleaseWork();
                }
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module);
        var specifier = module.Body
            .OfType<ImportDeclaration>()
            .Single()
            .Specifiers
            .OfType<ImportSpecifier>()
            .Single();
        Assert.AreEqual("ReleaseWork", ((Identifier)specifier.Imported).Name);
        Assert.AreEqual(alias, specifier.Local.Name);

        var script = module.ToKnRECMAScript();
        StringAssert.Contains(script, $"import {{ ReleaseWork as {alias} }} from \"runtime\";", StringComparison.Ordinal);
        Assert.AreEqual(2, CountOccurrences(script, $"+ {alias}()"), script);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_DefaultAndNamedMemberImports_DeduplicateAndFormValidCombinedImport()
    {
        var fixture = CompileModule(
            """
            using ECMAScript;
            using Runtime = Demo.RuntimeModule;

            namespace Demo
            {
                [ECMAScriptModule("runtime")]
                public static class RuntimeModule
                {
                    [ECMAScriptName("default")]
                    public static int DefaultExport() => 1;

                    public static int NamedExport() => 2;
                }

                [ECMAScriptModule("consumer")]
                public static class TestModule
                {
                    public static int Read()
                        => Runtime.DefaultExport() + Runtime.NamedExport() + Runtime.DefaultExport();
                }
            }
            """);

        var module = (await new AstConverter(fixture.Module, fixture.SemanticModel).Convert())!;
        var declaration = module.Body.OfType<ImportDeclaration>().Single();
        var specifier = declaration.Specifiers.OfType<ImportDefaultSpecifier>().Single();
        var namedSpecifier = declaration.Specifiers.OfType<ImportSpecifier>().Single();
        var script = module.ToKnRECMAScript();

        StringAssert.StartsWith(specifier.Local.Name, "i$", StringComparison.Ordinal);
        Assert.AreEqual("NamedExport", ((Identifier)namedSpecifier.Imported).Name);
        Assert.AreEqual("NamedExport", namedSpecifier.Local.Name);
        StringAssert.Contains(
            script,
            "import " + specifier.Local.Name + ", { NamedExport } from \"runtime\";",
            StringComparison.Ordinal);
        Assert.AreEqual(2, CountOccurrences(script, specifier.Local.Name + "()"), script);
        Assert.AreEqual(1, CountOccurrences(script, "NamedExport()"), script);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_CurrentModuleStringExportWithoutLocalBinding_RejectsTheInvalidSelfImport()
    {
        var fixture = CompileModule(
            """
            using ECMAScript;

            namespace Demo
            {
                [ECMAScriptModule("consumer")]
                public static class SharedModuleMember
                {
                    [ECMAScriptName("release-work")]
                    public static int Make() => 1;
                }

                [ECMAScriptModule("consumer")]
                public static class TestModule
                {
                    public static int Read() => SharedModuleMember.Make();
                }
            }
            """);

        var exception = await Assert.ThrowsExactlyAsync<NotSupportedException>(() =>
            new AstConverter(fixture.Module, fixture.SemanticModel).Convert());

        StringAssert.Contains(exception.Message, "Import 'release-work'", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "current module 'consumer'", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task Convert_ClrErrorConstructors_PreserveNativeTypeAndImportedCauseProtocol()
    {
        var fixture = CompileModule(
            """
            using System;

            public static class TestModule
            {
                public static Exception WithCause(string message, Exception cause)
                    => new Exception(message, cause);

                public static void Require(string? value)
                {
                    if (value == null)
                        throw new ArgumentNullException(nameof(value));
                }
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();
        StringAssert.Contains(script, "System/ExceptionModule.js", StringComparison.Ordinal);
        StringAssert.Contains(script, "new TypeError(\"value\")", StringComparison.Ordinal);
        StringAssert.Contains(script, "return", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_EcmascriptTypeErrorDerivedFromException_UsesTheNativeConstructorFallback()
    {
        var fixture = CompileModule(
            """
            using System;
            using ECMAScript;

            namespace Hosts
            {
                [ECMAScript]
                public sealed class TypeError : Exception
                {
                    public TypeError(string message) : base(message) { }
                }
            }

            public static class TestModule
            {
                public static Exception Create(string message) => new Hosts.TypeError(message);
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();
        StringAssert.Contains(script, "return new TypeError(message);", StringComparison.Ordinal);
        Assert.DoesNotContain("Hosts.TypeError", script, StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_EcmascriptErrorDerivedFromException_UsesTheNativeConstructorFallback()
    {
        var fixture = CompileModule(
            """
            using System;
            using ECMAScript;

            namespace Hosts
            {
                [ECMAScript]
                public sealed class Error : Exception
                {
                    public Error(string message) : base(message) { }
                }
            }

            public static class TestModule
            {
                public static Exception Create(string message) => new Hosts.Error(message);
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();
        StringAssert.Contains(script, "return new Error(message);", StringComparison.Ordinal);
        Assert.DoesNotContain("Hosts.Error", script, StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_EcmascriptErrorNameWithoutExceptionBase_UsesTheDeclaredHostConstructor()
    {
        var fixture = CompileModule(
            """
            using ECMAScript;

            namespace Hosts
            {
                [ECMAScript]
                public sealed class Error
                {
                    public Error(string message) { }
                }
            }

            public static class TestModule
            {
                public static Hosts.Error Create(string message) => new Hosts.Error(message);
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();
        StringAssert.Contains(script, "return new Error(message);", StringComparison.Ordinal);
        Assert.DoesNotContain("Hosts.Error", script, StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_GenericRuntimeHostWithNonSelfConstraint_DoesNotInventAConcreteRuntimeType()
    {
        var fixture = CompileModule(
            """
            using ECMAScript;

            namespace Hosts
            {
                [ECMAScript]
                public static class GenericHost<TBase, TValue>
                    where TValue : TBase
                {
                    public static int Count => 1;
                }
            }

            public static class TestModule
            {
                public static int Read() => Hosts.GenericHost<object, string>.Count;
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();
        StringAssert.Contains(script, "GenericHost.Count", StringComparison.Ordinal);
        Assert.DoesNotContain("String.count", script, StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

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
        StringAssert.Contains(script, "Reset()", StringComparison.Ordinal);
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
    public async Task Convert_TupleDeconstructionIntoMemberClassStaticFields_UsesTheDeclaredClassBinding()
    {
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class Counters
                {
                    public static int Left;
                    public static int Right;
                }

                public static void Update((int Left, int Right) value)
                {
                    (Counters.Left, Counters.Right) = value;
                }
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();
        StringAssert.Contains(script, "class Counters", StringComparison.Ordinal);
        StringAssert.Contains(script, "Counters.", StringComparison.Ordinal);
        Assert.DoesNotContain("let Counters", script, StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_MemberClassFieldLikeEvent_SkipsTheCompilerBackingFieldAndEmitsTheEventProtocol()
    {
        var fixture = CompileModule(
            """
            using System;

            public static class TestModule
            {
                public sealed class ReleaseNotice
                {
                    public event Action? Published;
                }
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();
        StringAssert.Contains(script, "class ReleaseNotice", StringComparison.Ordinal);
        StringAssert.Contains(script, "$event_store_", StringComparison.Ordinal);
        StringAssert.Contains(script, "$event_add_", StringComparison.Ordinal);
        StringAssert.Contains(script, "$event_remove_", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
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
        StringAssert.Contains(script, "Apply", StringComparison.OrdinalIgnoreCase);
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

        StringAssert.Contains(script, "Revision", StringComparison.OrdinalIgnoreCase);
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
    public async Task Convert_ProxySafeRuntimeClassStorage_CoversInheritanceAutoPropertiesPrimaryCaptureAndEvents()
    {
        var fixture = CompileModule(
            """
            using System;

            public static class TestModule
            {
                public class ReleaseBase(string prefix)
                {
                    private string Prefix { get; } = prefix;

                    public string ReadPrefix() => Prefix;
                }

                public sealed class ReleaseState : ReleaseBase
                {
                    public ReleaseState(string prefix) : base(prefix) { }

                    private string Value { get; set; } = "ready";

                    public string ReadValue() => Value;

                    public event Action? Changed;
                }
            }
            """);
        var options = new AstConverterOptions(
            AstConverterProfile.Standard,
            RuntimeClassPrivateStorage: RuntimeClassPrivateStorage.ProxySafeMangledProperties);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel, options).Convert();
        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();

        StringAssert.Contains(script, "class ReleaseBase", StringComparison.Ordinal);
        StringAssert.Contains(script, "class ReleaseState extends ReleaseBase", StringComparison.Ordinal);
        StringAssert.Contains(script, "$jazor$private$", StringComparison.Ordinal);
        StringAssert.Contains(script, "$jazor$private$$jazorPrimary_", StringComparison.Ordinal);
        StringAssert.Contains(script, "$jazor$private$$event_store_", StringComparison.Ordinal);
        StringAssert.Contains(script, "get Value()", StringComparison.Ordinal);
        StringAssert.Contains(script, "set Value(value)", StringComparison.Ordinal);
        Assert.DoesNotContain("#", script, StringComparison.Ordinal);
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

    [TestMethod]
    public async Task Convert_CurrentModuleIndexerCompoundAssignment_UsesSingleEvaluationGetterSetterProtocol()
    {
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class ReleaseSlots
                {
                    public int this[int index]
                    {
                        get => index;
                        set { }
                    }
                }

                public static ReleaseSlots GetSlots() => new();

                public static int GetIndex() => 1;

                public static int GetDelta() => 2;

                public static void Update()
                {
                    GetSlots()[GetIndex()] |= GetDelta();
                }
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();
        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();

        Assert.AreEqual(1, CountOccurrences(script, "= GetSlots()"), script);
        Assert.AreEqual(1, CountOccurrences(script, "= GetIndex()"), script);
        Assert.AreEqual(1, CountOccurrences(script, "| GetDelta()"), script);
        StringAssert.Contains(script, "|", StringComparison.Ordinal);
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
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptAttribute).Assembly.Location)),
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

    private static int CountOccurrences(string value, string fragment)
    {
        var count = 0;
        for (var offset = 0; (offset = value.IndexOf(fragment, offset, StringComparison.Ordinal)) >= 0; offset += fragment.Length)
            count++;
        return count;
    }

    private sealed record ModuleFixture(INamedTypeSymbol Module, SemanticModel SemanticModel)
    {
        public INamedTypeSymbol GetType(string name)
            => Module.GetTypeMembers(name).Single();
    }
}
