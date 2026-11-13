using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Jazor.NodeHost;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class SourceTypeCompilationTests
{
    [TestMethod]
    public async Task Convert_FilteredSourceFieldInitializer_DeclaresHelperBeforeAllocation()
    {
        var script = await Convert("""
            public static class TestModule
            {
                private static readonly Model model = new(7);
                public static int Run() => model.Value;
            }
            internal sealed class Model
            {
                public Model(int value) { Value = value; }
                public int Value { get; }
            }
            """, new AstConverterOptions(AstConverterProfile.Standard, MemberFilter: _ => true));
        await AssertRun(script, 7);
    }

    [TestMethod]
    public async Task Convert_FilteredNamespacedSource_TraversesTheNamespaceBeforeFilteringMembers()
    {
        var script = await Convert("""
            namespace Demo;
            public static class TestModule { public static int Run() => Helpers.Read(); }
            internal static class Helpers { public static int Read() => 8; }
            """, new AstConverterOptions(AstConverterProfile.Standard, MemberFilter: _ => true), moduleName: "Demo.TestModule");
        await AssertRun(script, 8);
    }

    [TestMethod]
    public async Task Convert_HostConsumesMethodBody_SourceDependencyFilterStillDeclaresItsHelper()
    {
        var script = await Convert("""
            public static class TestModule
            {
                public static int Run() => 1;
                public static int Render() => Helpers.Read();
            }
            internal static class Helpers { public static int Read() => 5; }
            """, new AstConverterOptions(AstConverterProfile.Standard,
                MemberFilter: symbol => symbol.Name != "Render", SourceDependencyFilter: _ => true));
        StringAssert.Contains(script, "class Helpers", StringComparison.Ordinal);
        Assert.DoesNotContain("function Render", script, StringComparison.Ordinal);
        await AssertRun(script, 1);
    }

    [TestMethod]
    public async Task Convert_ConstructedGenericSourceProjection_KeepsOneModuleImplementation()
    {
        var script = await Convert("""
            public static class TestModule { public static int Run() => GenericHelper<int>.Read(); }
            public class GenericHelper<T> { public static int Read() => 4; }
            """, new AstConverterOptions(AstConverterProfile.Standard, ModulePolicy: new GenericProjectionPolicy()));
        Assert.DoesNotContain("class GenericHelper", script, StringComparison.Ordinal);
        await AssertRun(script, 4);
    }

    [TestMethod]
    public async Task Convert_FilteredRoot_StillIncludesTransitiveHelperDependencies()
    {
        var script = await Convert("""
            public static class TestModule
            {
                public static int Run() => Helpers.Read();
                public static bool Backend() => Server.Read();
            }
            internal static class Helpers { public static int Read() => new Derived().Value + new OtherDerived().Value; }
            internal class Base { public int Value = 3; }
            internal sealed class Derived : Base { }
            internal sealed class OtherDerived : Base { }
            internal static class Server { public static bool Read() => System.IO.File.Exists("server"); }
            """, new AstConverterOptions(AstConverterProfile.Standard, MemberFilter: symbol => symbol.Name != "Backend"));
        Assert.DoesNotContain("class Server", script, StringComparison.Ordinal);
        await AssertRun(script, 6);
    }

    [TestMethod]
    public async Task Convert_DeclaredNameOverrides_IncludeSourceHelpersWithoutLosingTheOverride()
    {
        var script = await Convert("""
            public static class TestModule { public static int Run() => new Model().Value; }
            internal sealed class Model { public int Value = 9; }
            """, createOptions: root => new AstConverterOptions(AstConverterProfile.Standard,
                DeclaredNames: new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default)
                {
                    [root.GetMembers("Run").Single()] = "ReadResult"
                }));
        StringAssert.Contains(script, "ReadResult", StringComparison.Ordinal);
        await AssertRun(script, 9);
    }

    [TestMethod]
    public void SourceDependencyAnalysis_InspectsConcreteTypeArgumentsWithoutRequiringRuntimeEmission()
    {
        var tree = CSharpSyntaxTree.ParseText("""
            using System.Collections.Generic;
            public static class Root { public static int Run() => new List<Payload>().Count; }
            public sealed class Payload { public string Name { get; set; } = ""; }
            """, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create("SourceAnnotations", [tree], TestMetadataReferences.Net11);
        var root = compilation.GetTypeByMetadataName("Root")!;
        Assert.IsFalse(SourceTypeClosure.Collect(compilation, [root]).Any(static type => type.Name == "Payload"));
        Assert.IsTrue(SourceTypeClosure.Collect(compilation, [root], includeTypeAnnotations: true)
            .Any(static type => type.Name == "Payload"));
    }

    [TestMethod]
    public async Task Convert_UnrelatedModuleHelpers_KeepSeparateOwnership()
    {
        var script = await Convert("""
            using ECMAScript;
            [ECMAScriptModule("./first")]
            public static class TestModule { public static int Run() => First.Value(); }
            [ECMAScriptModule("./second")]
            public static class OtherModule { public static int Run() => Second.Value(); }
            internal static class First { public static int Value() => 3; }
            internal static class Second { public static int Value() => 9; }
            """);
        Assert.DoesNotContain("class Second", script, StringComparison.Ordinal);
        await AssertRun(script, 3);
    }

    [TestMethod]
    public async Task Convert_HostOwnedProtocolType_IsConsumedWithoutCopyingItsClrBody()
    {
        var script = await Convert("""
            public static class TestModule { public static int Run() => Protocol.Read(); }
            internal static class Protocol { public static int Read() => System.IO.File.Exists("server") ? 0 : 0; }
            """, new AstConverterOptions(AstConverterProfile.Standard,
                ModulePolicy: new ProtocolPolicy(), Host: new ProtocolHost()));
        Assert.DoesNotContain("class Protocol", script, StringComparison.Ordinal);
        await AssertRun(script, 5);
    }

    [TestMethod]
    public async Task Convert_SourceHelpersWithNameCollisions_KeepDistinctStableBindings()
    {
        var script = await Convert("""
            public static class TestModule
            {
                public static int Run() => new A.Model().Read() + new B.Model().Read();
            }
            namespace A { internal class Model { public int Read() => 2; } }
            namespace B { internal class Model { public int Read() => 5; } }
            """);
        await AssertRun(script, 7);
    }

    [TestMethod]
    public async Task Convert_SourceHelperBaseInRoot_EmitsBaseBeforeDerived()
    {
        var script = await Convert("""
            public static class TestModule
            {
                public class Base { public int Value = 3; }
                public static int Run() => new Derived().Value;
            }
            internal sealed class Derived : TestModule.Base { }
            """);
        await AssertRun(script, 3);
    }

    [TestMethod]
    public async Task Convert_NameofSourceType_DoesNotEmitRuntimeImplementation()
    {
        var script = await Convert("""
            public static class TestModule { public static string Run() => nameof(Backend.Read); }
            public class Backend { public bool Read() => System.IO.File.Exists("backend"); }
            """);
        Assert.DoesNotContain("class Backend", script, StringComparison.Ordinal);
        StringAssert.Contains(script, "\"Read\"", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task Convert_SharedSourceClass_RequiresOneModuleOwnerInsteadOfDuplicatingState()
    {
        var error = await Assert.ThrowsAsync<NotSupportedException>(() => Convert("""
            using ECMAScript;
            [ECMAScriptModule("./first")]
            public static class TestModule { public static int Run() => Counter.Read(); }
            [ECMAScriptModule("./second")]
            public static class OtherModule { public static int Run() => Counter.Read(); }
            public static class Counter { private static int value; public static int Read() => ++value; }
            """));
        StringAssert.Contains(error.Message, "shared by generated modules", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task Convert_ErasedSourceTypeArgument_DoesNotMaterializeItsImplementation()
    {
        var script = await Convert("""
            using System.Collections.Generic;
            public static class TestModule { public static int Run() => new List<Backend>().Count; }
            public class Backend { public bool Read() => System.IO.File.Exists("backend"); }
            """);
        Assert.DoesNotContain("class Backend", script, StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task Convert_UnmarkedSourceHelpers_EmitsAndExecutesTheirImplementations()
    {
        var script = await Convert("""
            public static class TestModule
            {
                public static int Run()
                {
                    var counter = new Counter(3);
                    counter.Increment();
                    ICounter view = counter;
                    return Helpers.Read(counter) + new Options(2).Value + view.Zero();
                }
            }
            internal sealed class Counter : CounterBase
            {
                public Counter(int value) : base(value) { }
                public void Increment() { Value++; }
            }
            internal interface ICounter { int Zero(); }
            internal class CounterBase : ICounter
            {
                public int Value;
                public CounterBase(int value) { Value = value; }
                public int Zero() => 0;
            }
            internal static class Helpers
            {
                public static int Read(Counter counter) => counter.Value;
            }
            internal sealed record Options(int Value);
            internal class Unrelated
            {
                public bool Read() => System.IO.File.Exists("server-only");
            }
            """);
        StringAssert.Contains(script, "extends CounterBase", StringComparison.Ordinal);
        Assert.DoesNotContain("Unrelated", script, StringComparison.Ordinal);
        Assert.DoesNotContain("class Options", script, StringComparison.Ordinal);
        await AssertRun(script, 6);
    }

    [TestMethod]
    public async Task Convert_ClassWithEmptyName_PreservesConstructorAndInstanceSemantics()
    {
        var script = await Convert("""
            public static class TestModule
            {
                [System.ComponentModel.Description("@#")]
                public sealed class Counter
                {
                    public int Value = 2;
                    public Counter(int value) { Value += value; }
                }
                public static int Run() => new Counter(5).Value;
            }
            """);
        StringAssert.Contains(script, "new Counter(5)", StringComparison.Ordinal);
        await AssertRun(script, 7);
    }

    [TestMethod]
    public async Task Convert_UnmarkedSourceHelper_StillRejectsUnmappedClrCalls()
    {
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(() => Convert("""
            public static class TestModule
            {
                public static bool Run() => Helper.Read();
            }
            internal static class Helper
            {
                public static bool Read() => System.IO.File.Exists("missing");
            }
            """));
        StringAssert.Contains(exception.Message, "System.IO.File", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task Convert_SourceClosure_DoesNotCopyHostImplementations()
    {
        var script = await Convert("""
            using ECMAScript;
            [ECMAScript]
            public static class Browser
            {
                public static extern int Read();
            }
            public static class TestModule
            {
                public static int Run() => Browser.Read();
            }
            """);
        StringAssert.Contains(script, "Browser.Read()", StringComparison.Ordinal);
        Assert.DoesNotContain("class Browser", script, StringComparison.Ordinal);
    }

    private static async Task<string> Convert(string source, AstConverterOptions? options = null,
        Func<INamedTypeSymbol, AstConverterOptions>? createOptions = null, string moduleName = "TestModule")
    {
        var tree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions, "source-types.cs");
        var compilation = CSharpCompilation.Create("SourceTypes_" + Guid.NewGuid().ToString("N"), [tree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        options = createOptions?.Invoke(compilation.GetTypeByMetadataName(moduleName)!) ?? options;
        var converter = new AstConverter(compilation.GetTypeByMetadataName(moduleName)!, compilation.GetSemanticModel(tree), options);
        var first = (await converter.Convert())!.ToKnRECMAScript();
        var second = (await new AstConverter(compilation.GetTypeByMetadataName(moduleName)!, compilation.GetSemanticModel(tree), options).Convert())!.ToKnRECMAScript();
        Assert.AreEqual(first, second);
        _ = new Parser().ParseModule(first);
        return first;
    }

    private static async Task AssertRun(string script, int expected)
    {
        var root = Path.Combine(RepositoryTemp.Root, "source-types-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "app.mjs"), script);
            var testPath = Path.Combine(root, "test.mjs");
            await File.WriteAllTextAsync(testPath, $$"""
                import { Run } from './app.mjs';
                globalThis.__jazorTest('source type semantics', () => {
                  const actual = Run();
                  if (actual !== {{expected}}) throw new Error(`unexpected result: ${actual}`);
                });
                """);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await NodeModuleRunner.RunAsync(root, testPath, cancellationToken: timeout.Token);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private sealed class ProtocolPolicy : AstConverterModulePolicy
    {
        public override bool ShouldIncludeSourceDependency(INamedTypeSymbol type) => type.Name != "Protocol";
    }

    private sealed class GenericProjectionPolicy : AstConverterModulePolicy
    {
        public override IEnumerable<INamedTypeSymbol> EnumerateModuleTypes(INamedTypeSymbol moduleType)
        {
            yield return moduleType;
            var helper = moduleType.ContainingNamespace.GetTypeMembers("GenericHelper").Single();
            yield return helper.Construct(helper.BaseType!);
        }
    }

    private sealed class ProtocolHost : SemanticWalkerHost
    {
        public override Expression? RewriteInvocationPreorder(IInvocationOperation operation, SenseArgument argument)
            => operation.TargetMethod.ContainingType.Name == "Protocol" ? new NumericLiteral(5, "5") : null;
    }
}
