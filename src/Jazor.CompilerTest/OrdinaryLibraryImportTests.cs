using Acornima;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Jazor.NodeHost;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class OrdinaryLibraryImportTests
{
    [TestMethod]
    public void Import_NameofMetadata_DoesNotRecoverExternalComponentBaseProperties()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public abstract class Base { public string Value { get; set; } }
            public class Component : Base { }
            """);
        Assert.IsEmpty(fixture.Plan("public static string Run() => nameof(Ordinary.Component.Value);").BuildModules());
    }

    [TestMethod]
    public async Task Import_DelegateInvocation_UsesExistingErasedCallableProtocol()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public delegate int Transform(int value);
            public static class Entry
            {
                public static int Apply(Transform action, int value) => action(value);
            }
            """);
        await fixture.Compile("public static int Run() { Ordinary.Transform action = value => value + 3; return Ordinary.Entry.Apply(action, 4); }");
        await fixture.AssertRun(7);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Import_HostContractsKeepPrecedenceIncludingInheritedMarkers(bool inherited)
    {
        using var fixture = new Fixture("""
            using ECMAScript;
            namespace Ordinary;
            [ECMAScript] public class NativeBase { }
            """ + (inherited ? "public class Native : NativeBase" : "[ECMAScript] public class Native") +
            " { public int Read() => 3; }", dependencies: [new FileInfo(typeof(ECMAScript.ECMAScriptAttribute).Assembly.Location)]);
        Assert.IsEmpty(fixture.Plan("public static int Run() => new Ordinary.Native().Read();").BuildModules());
    }

    [TestMethod]
    public async Task Import_ExplicitStaticConstructor_RejectsUnsupportedInitializationProtocol()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public static class Entry { public static int Value; static Entry() { Value = 3; } public static int Read() => Value; }
            """);
        await Assert.ThrowsAsync<NotSupportedException>(() => fixture.Compile("public static int Run() => Ordinary.Entry.Read();"));
    }

    [TestMethod]
    public void Import_AutomaticAssetCandidates_KeepUnusedAssetsAndExistingMappingsOutOfRecovery()
    {
        using var fixture = new Fixture("namespace Ordinary; public static class Entry { public static int Read() => 3; }");
        var consumer = fixture.Consumer("public static int Run() => Ordinary.Entry.Read() + System.Math.Abs(-2);");
        var plan = OrdinaryLibraryImportPlan.Create(consumer,
            [new("Ordinary", fixture.LibraryPath), new("Unused", "missing-unused.dll"),
                new(typeof(object).Assembly.GetName().Name!, typeof(object).Assembly.Location)],
            [consumer.GetTypeByMetadataName("App")!]);
        Assert.HasCount(1, plan.BuildModules());
        Assert.IsFalse(plan.ContainsType(consumer.GetTypeByMetadataName("System.Math")!));
    }

    [TestMethod]
    public void Import_ProducerCatalog_TakesPrecedenceOverAutomaticImplementationRecovery()
    {
        using var fixture = new Fixture("""
            namespace Jazor.Generated { internal static class ModuleCatalog {} }
            namespace Ordinary { public static class Entry { public static int Read() => 3; } }
            """);
        var plan = fixture.Plan("public static int Run() => Ordinary.Entry.Read();");
        Assert.IsEmpty(plan.BuildModules());
    }

    [TestMethod]
    public void Import_WithConsumerNetModuleAndLocalAssemblyAttribute_KeepsAssemblyResolutionBounded()
    {
        using var fixture = new Fixture("""
            [assembly: LocalAttribute]
            public sealed class LocalAttribute : System.Attribute { }
            namespace Ordinary { public static class Entry { public static int Read() => 3; } }
            """);
        var netModule = CSharpCompilation.Create("UnusedModule", [CSharpSyntaxTree.ParseText("public class UnusedModuleType {}")],
            TestMetadataReferences.Net11, new CSharpCompilationOptions(OutputKind.NetModule));
        using var image = new MemoryStream();
        Assert.IsTrue(netModule.Emit(image).Success);
        var consumer = fixture.Consumer("public static int Run() => Ordinary.Entry.Read();")
            .AddReferences(MetadataReference.CreateFromImage(image.ToArray(), new MetadataReferenceProperties(MetadataImageKind.Module)));
        var plan = OrdinaryLibraryImportPlan.Create(consumer, [new("Ordinary", fixture.LibraryPath)],
            [consumer.GetTypeByMetadataName("App")!]);
        Assert.HasCount(1, plan.BuildModules());
    }

    [TestMethod]
    public async Task Import_ObjectInitializerAndIndexerAndOutResult_PreservesObservableState()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public sealed class Model
            {
                public int Value;
                public int this[int index] { get => Value + index; set => Value = value - index; }
                public int Read(out int value) { value = Value; return Value + 1; }
            }
            """);
        await fixture.Compile("""
            public static int Run()
            {
                var model = new Ordinary.Model { Value = 4 };
                model[2] = 9;
                var result = model.Read(out var value);
                return result + value + model[1];
            }
            """);
        await fixture.AssertRun(23);
    }

    [TestMethod]
    public void Import_HostPackageDependency_IsRecordedForExistingArtifactRestore()
    {
        using var fixture = new Fixture("public static class Entry { public static void Tick() { ECMAScript.Vue.NextTick(); } }",
            dependencies: [new FileInfo(typeof(ECMAScript.Vue).Assembly.Location),
                new FileInfo(typeof(ECMAScript.PromiseResult).Assembly.Location),
                new FileInfo(typeof(ECMAScript.ECMAScriptAttribute).Assembly.Location)]);
        var consumer = fixture.Consumer("public static void Run() => Entry.Tick();");
        var plan = OrdinaryLibraryImportPlan.Create(consumer, [new("Ordinary", fixture.LibraryPath)],
            [consumer.GetTypeByMetadataName("App")!]);
        Assert.IsTrue(plan.BuildModules().Single().PackageImports.Contains("vue"));
    }

    [TestMethod]
    public void SymbolIdentity_DistinguishesReferenceReturnsAndNestedGenericParameterOwners()
    {
        var compilation = CSharpCompilation.Create("Identity", [CSharpSyntaxTree.ParseText("""
            public unsafe class Outer<T>
            {
                public class Inner<U>
                {
                    public int Field;
                    public event System.Action Changed;
                    public ref int Reference() => ref Field;
                    public ref readonly int ReadOnlyReference() => ref Field;
                    public int Value() => Field;
                    public ref int Property => ref Field;
                    public void Pointer(int* value) { }
                    public System.Collections.Generic.List<T> Generic(U[] values) => null;
                }
            }
            """, TestMetadataReferences.PreviewParseOptions)], TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        var type = compilation.GetTypeByMetadataName("Outer`1+Inner`1")!;
        var keys = type.GetMembers().Select(LibrarySymbolIdentity.Member).ToArray();
        Assert.AreEqual(keys.Length, keys.Distinct(StringComparer.Ordinal).Count());
        StringAssert.Contains(LibrarySymbolIdentity.Member(type.GetMembers("Reference").Single()), "->1:");
        Assert.AreNotEqual(LibrarySymbolIdentity.Member(type.GetMembers("Reference").Single()),
            LibrarySymbolIdentity.Member(type.GetMembers("ReadOnlyReference").Single()));
        StringAssert.Contains(LibrarySymbolIdentity.Member(type.GetMembers("Generic").Single()), "!Outer`1");
        StringAssert.Contains(LibrarySymbolIdentity.Member(type.GetMembers("Pointer").Single()), "*");
    }

    [TestMethod]
    public async Task Import_StaticAutoPropertyInitializerAndSameTypeConstruction_PreservesDefaults()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public sealed class State
            {
                private static State instance = new State();
                private static int later = 9;
                public static int Initial { get; } = ReadLater();
                private readonly int observed;
                public State() { observed = later; }
                private static int ReadLater() => later;
                public static int Read() => instance.observed * 100 + Initial;
            }
            """);
        await fixture.Compile("public static int Run() => Ordinary.State.Read();");
        await fixture.AssertRun(9);
    }

    [TestMethod]
    public void Import_StaticInitializerAcrossSelectedAssemblies_RejectsMissingLazyInitialization()
    {
        using var dependency = new Fixture("namespace Dependency; public static class State { public static int Value; }", assemblyName: "Dependency");
        using var fixture = new Fixture("""
            namespace Ordinary;
            public static class State { private static int first = Dependency.State.Value; public static int Read() => first; }
            """, dependencies: [new FileInfo(dependency.LibraryPath)]);
        var compilation = fixture.Consumer("public static int Run() => Ordinary.State.Read();");
        var error = Assert.Throws<NotSupportedException>(() => OrdinaryLibraryImportPlan.Create(compilation,
            [new("Ordinary", fixture.LibraryPath), new("Dependency", dependency.LibraryPath)], [compilation.GetTypeByMetadataName("App")!]));
        StringAssert.Contains(error.Message, "Cross-type initialization");
    }

    [TestMethod]
    public void Generator_NamespacedModule_DiscoversLibraryDependencyRoots()
    {
        using var fixture = new Fixture("namespace Ordinary; public static class Entry { public static int Read() => 3; }");
        var modules = fixture.Generate("""
            using ECMAScript;
            namespace AppNamespace;
            [ECMAScriptModule("nested.mjs")]
            public static class App { public static int Run() => Ordinary.Entry.Read(); }
            """);
        Assert.HasCount(2, modules);
    }

    [TestMethod]
    public async Task Import_MultipleSelectedConstructors_UsesBoundSelectorForSameArity()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public sealed class Value
            {
                public int Number;
                public Value(int value) { Number = value; }
                public Value(string value) { Number = value.Length; }
            }
            """);
        await fixture.Compile("public static int Run() => new Ordinary.Value(4).Number + new Ordinary.Value(\"abc\").Number;");
        await fixture.AssertRun(7);
    }

    [TestMethod]
    public void Import_StaticInitializationAcrossClasses_RejectsMissingLazyInitializationProtocol()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public static class State
            {
                private static int first = Helper.Read();
                private static int second;
                public static int Later => second;
                public static int Read() => first + 8;
            }
            public static class Helper { public static int Read() => State.Later; }
            """);
        var error = Assert.Throws<NotSupportedException>(() => fixture.Plan("public static int Run() => Ordinary.State.Read();"));
        StringAssert.Contains(error.Message, "Cross-type initialization");
    }

    [TestMethod]
    public void ImportPlan_SupportsOnlySelectedRuntimeTypesAndExactMembers()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public sealed class Model { public int Read() => 3; public int Unused() => 4; }
            public sealed class Unused { public int Read() => 9; }
            """);
        var consumer = fixture.Consumer("public static int Run() => new Ordinary.Model().Read();");
        var app = consumer.GetTypeByMetadataName("App")!;
        var model = consumer.GetTypeByMetadataName("Ordinary.Model")!;
        var unused = consumer.GetTypeByMetadataName("Ordinary.Unused")!;
        var plan = OrdinaryLibraryImportPlan.Create(consumer, [new("Ordinary", fixture.LibraryPath)], [app]);
        Assert.IsTrue(plan.ContainsMember(model.GetMembers("Read").Single()));
        Assert.IsFalse(plan.ContainsMember(model.GetMembers("Unused").Single()));
        Assert.IsFalse(plan.ContainsMember(unused.GetMembers("Read").Single()));
        Assert.IsFalse(plan.ContainsType(consumer.CreateArrayTypeSymbol(model)));
        Assert.IsFalse(plan.TryGetTypeBinding(consumer.CreateArrayTypeSymbol(model), out _, out _));
        Assert.IsFalse(plan.TryGetTypeBinding(unused, out _, out _));
        Assert.IsFalse(plan.ContainsMember(consumer.Assembly));
        Assert.Throws<NotSupportedException>(() => plan.GetMemberName(app.GetMembers("Run").Single()));
        Assert.Throws<NotSupportedException>(() => plan.GetConstructorSelector(
            consumer.GetSpecialType(SpecialType.System_Object).InstanceConstructors[0]));
        Assert.IsFalse(plan.RequiresConstructorSelector(model.InstanceConstructors[0]));
        Assert.IsFalse(plan.RequiresConstructorSelector(consumer.GetSpecialType(SpecialType.System_Object).InstanceConstructors[0]));
        Assert.Throws<OperationCanceledException>(() => OrdinaryLibraryImportPlan.Create(consumer,
            [new("Ordinary", fixture.LibraryPath)], [app], new CancellationToken(true)));
    }

    [TestMethod]
    public void ImportManifest_AmbiguousImplementation_RejectsDuplicateAssetSelection()
    {
        Assert.Throws<NotSupportedException>(() => LibraryImportContext.ReadInputs([
            new ImportManifest("asset|Example|first.dll\nasset|Example|second.dll")], CancellationToken.None));
    }

    [TestMethod]
    public async Task Import_StaticInitializerReadingLaterField_ObservesZeroedStorage()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public static class State
            {
                private static int first = ReadLater();
                private static int later = 9;
                private static int ReadLater() => later;
                public static int Read() => first * 100 + later;
            }
            """);
        await fixture.Compile("public static int Run() => Ordinary.State.Read();");
        await fixture.AssertRun(9);
    }

    [TestMethod]
    public async Task Import_InstanceCallbackAndEnumConstant_CollectsMethodGroupWithoutRuntimeEnum()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public enum Choice { First = 5 }
            public sealed class Model
            {
                public int Read() => (int)Choice.First;
                public int Run() { System.Func<int> callback = Read; return callback(); }
            }
            """);
        await fixture.Compile("public static int Run() => new Ordinary.Model().Run();");
        await fixture.AssertRun(5);
    }

    [TestMethod]
    public async Task Import_RefParameter_UsesExistingCallerCalleeProtocol()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public static class Helpers { public static void Add(ref int value) { value += 4; } }
            """);
        await fixture.Compile("public static int Run() { var value = 3; Ordinary.Helpers.Add(ref value); return value; }");
        await fixture.AssertRun(7);
    }

    [TestMethod]
    public async Task Import_StaticFieldAndUnusedAutoPropertyInitializer_PreservesInitializationSideEffects()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public static class State
            {
                public static int Value;
                public static int Unused { get; } = Initialize();
                private static int Initialize() { Value = 9; return 1; }
                public static int Read() => Value;
            }
            """);
        await fixture.Compile("public static int Run() => Ordinary.State.Read();");
        await fixture.AssertRun(9);
    }

    [TestMethod]
    public void Import_MismatchedMemberSameAssemblyIdentity_RejectsWithoutNameOnlyFallback()
    {
        using var fixture = new Fixture("namespace Ordinary; public static class Entry { public static int Read(int value) => value; }");
        var path = Path.Combine(fixture.Root, "Mismatch.dll");
        var compilation = CSharpCompilation.Create("Ordinary", [CSharpSyntaxTree.ParseText("namespace Ordinary; public static class Entry { public static int Read(string value) => value.Length; }")],
            TestMetadataReferences.Net11, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.IsTrue(compilation.Emit(path).Success);
        var error = Assert.Throws<NotSupportedException>(() => fixture.Plan("public static int Run() => Ordinary.Entry.Read(3);", path));
        StringAssert.Contains(error.Message, "Cannot match library member");
    }

    [TestMethod]
    public void Import_RequiredMemberFromOmittedReference_ReportsRecoveredBindingFailure()
    {
        var root = RepositoryTemp.CreateDirectory("ordinary-missing-reference-");
        try
        {
            var dependency = CSharpCompilation.Create("PrivateDependency", [CSharpSyntaxTree.ParseText("public static class Dependency { public static int Read() => 4; }")],
                TestMetadataReferences.Net11, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var dependencyImage = new MemoryStream();
            Assert.IsTrue(dependency.Emit(dependencyImage).Success);
            var library = CSharpCompilation.Create("Ordinary", [CSharpSyntaxTree.ParseText("public static class Entry { public static int Read() => Dependency.Read(); }")],
                TestMetadataReferences.Net11.Add(MetadataReference.CreateFromImage(dependencyImage.ToArray())),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var path = Path.Combine(root, "Ordinary.dll");
            Assert.IsTrue(library.Emit(path).Success);
            using var fixture = new Fixture(new FileInfo(path));
            var error = Assert.Throws<NotSupportedException>(() => fixture.Plan("public static int Run() => Entry.Read();"));
            StringAssert.Contains(error.Message, "Cannot bind recovered");
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [TestMethod]
    public void ImportManifest_EmptyUnavailableAndUnrelatedFiles_DoNotSelectLibraries()
    {
        Assert.IsEmpty(LibraryImportContext.ReadInputs([new ImportManifest("\n\n")], CancellationToken.None));
        Assert.IsEmpty(LibraryImportContext.ReadInputs([new MissingManifest()], CancellationToken.None));
        Assert.IsEmpty(LibraryImportContext.ReadInputs([new UnrelatedAdditionalFile()], CancellationToken.None));
        var inputs = LibraryImportContext.ReadInputs([new ImportManifest("asset|Example|example.dll|ABCDEF")], CancellationToken.None);
        Assert.AreEqual("ABCDEF", inputs.Single().ContentHash);
    }

    [TestMethod]
    public void SymbolIdentity_DistinguishesOverloadsOnEnclosingAndNestedTypeArguments()
    {
        var tree = CSharpSyntaxTree.ParseText("""
            public class Outer<T> { public class Cell<U> { } }
            public static class Consumer
            {
                public static void Read(Outer<int>.Cell<string> cell) { }
                public static void Read(Outer<string>.Cell<string> cell) { }
                public static void Read(Outer<int>.Cell<int> cell) { }
            }
            """, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create("NestedOverloads", [tree], TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var overloads = compilation.GetTypeByMetadataName("Consumer")!.GetMembers("Read");
        Assert.HasCount(3, overloads);
        Assert.HasCount(3, overloads.Select(LibrarySymbolIdentity.Member).Distinct(StringComparer.Ordinal).ToArray());
    }

    [TestMethod]
    public void SymbolIdentity_DistinguishesEventsIndexersPointersDynamicAndNestedGenericOwners()
    {
        var tree = CSharpSyntaxTree.ParseText("""
            public unsafe class Shapes<T>
            {
                public event System.Action? Changed;
                public int this[int index] => index;
                public static int* Pointer(int* value) => value;
                public dynamic Dynamic(dynamic value) => value;
                public class Nested<U> { public T[] Read(T[,] values, U item) => new T[0]; }
            }
            """, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create("Shapes", [tree], TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        var type = compilation.GetTypeByMetadataName("Shapes`1")!;
        foreach (var member in type.GetMembers()) Assert.IsFalse(string.IsNullOrEmpty(LibrarySymbolIdentity.Member(member)));
        var nested = type.GetTypeMembers("Nested").Single();
        StringAssert.Contains(LibrarySymbolIdentity.Member(nested.GetMembers("Read").Single()), "Shapes`1");
        var constructed = nested.Construct(compilation.GetSpecialType(SpecialType.System_Int32));
        Assert.IsFalse(string.IsNullOrEmpty(LibrarySymbolIdentity.TypeName(constructed)));
    }

    [TestMethod]
    public async Task Import_NestedGenericType_UsesExactTypeParameterAndArraySignature()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public class Outer<T>
            {
                public sealed class Inner
                {
                    public T Read(T[] values) => values[0];
                }
            }
            """);
        await fixture.Compile("public static int Run() => new Ordinary.Outer<int>.Inner().Read(new[] { 7 });");
        await fixture.AssertRun(7);
    }

    [TestMethod]
    public async Task Import_VirtualPropertyAndInitializer_PreservesInstanceBehavior()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public class Base { public virtual int Value => 1; }
            public sealed class Derived : Base { public override int Value { get; } = 8; }
            public static class Factory { public static Base Create() => new Derived(); }
            """);
        await fixture.Compile("public static int Run() => Ordinary.Factory.Create().Value;");
        await fixture.AssertRun(8);
    }

    [TestMethod]
    public async Task Import_PropertySetterOnly_DoesNotLowerUnreachableGetter()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public sealed class State
            {
                public int Stored;
                public int Value { get => System.IO.File.Exists("server") ? 1 : 0; set => Stored = value; }
            }
            """);
        await fixture.Compile("public static int Run() { var state = new Ordinary.State(); state.Value = 6; return state.Stored; }");
        await fixture.AssertRun(6);
    }

    [TestMethod]
    public void Import_ReferenceOnlyAsset_RejectsMissingImplementation()
    {
        using var fixture = new Fixture("namespace Ordinary; public static class Entry { public static int Read() => 1; }", useReferenceAssembly: true);
        var error = Assert.Throws<NotSupportedException>(() => fixture.Plan("public static int Run() => Ordinary.Entry.Read();", fixture.ReferencePath));
        StringAssert.Contains(error.Message, "reference assembly");
    }

    [TestMethod]
    public void Import_ImplementationChangedAfterManifest_ReportsStaleInput()
    {
        using var fixture = new Fixture("namespace Ordinary; public static class Entry { public static int Read() => 1; }");
        var error = Assert.Throws<NotSupportedException>(() => fixture.Plan("public static int Run() => Ordinary.Entry.Read();", hash: "stale"));
        StringAssert.Contains(error.Message, "changed after");
    }

    [TestMethod]
    public void Import_ImplementationIdentityMismatch_RejectsDifferentAssembly()
    {
        using var fixture = new Fixture("namespace Ordinary; public static class Entry { public static int Read() => 1; }");
        var wrong = Path.Combine(fixture.Root, "Wrong.dll");
        var compilation = CSharpCompilation.Create("Wrong", [CSharpSyntaxTree.ParseText("public static class Entry { public static int Read() => 2; }")],
            TestMetadataReferences.Net11, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.IsTrue(compilation.Emit(wrong).Success);
        var error = Assert.Throws<NotSupportedException>(() => fixture.Plan("public static int Run() => Ordinary.Entry.Read();", wrong));
        StringAssert.Contains(error.Message, "expected 'Ordinary");
    }

    [TestMethod]
    [DataRow("public struct Value { public static int Read() => 1; }", "Value.Read()")]
    [DataRow("public class Value : System.Exception { public static int Read() => 1; }", "Value.Read()")]
    public void Import_UnsupportedRuntimeType_FailsExplicitly(string declaration, string expression)
    {
        using var fixture = new Fixture("namespace Ordinary; " + declaration);
        Assert.Throws<NotSupportedException>(() => fixture.Plan("public static int Run() => Ordinary." + expression + ";"));
    }

    [TestMethod]
    public async Task Import_RecordRuntime_UsesExistingStructuralBoundaryInsteadOfRecoveringRuntimeMembers()
    {
        using var fixture = new Fixture("namespace Ordinary; public record Value(int Number) { public int Read() => Number; }");
        Assert.IsEmpty(fixture.Plan("public static int Run() => new Ordinary.Value(1).Number;").BuildModules());
        await Assert.ThrowsAsync<OperationTransformationException>(() => fixture.Compile("public static int Run() => new Ordinary.Value(1).Read();"));
    }

    [TestMethod]
    public void ImportContext_TransfersInputsAndAcceptsIntegrationDeclaredRoots()
    {
        using var fixture = new Fixture("namespace Ordinary; public static class Entry { public static int Read() => 3; }");
        var compilation = fixture.Consumer("public static int Run() => Ordinary.Entry.Read();");
        LibraryImportContext.AddRoots(compilation, [compilation.GetTypeByMetadataName("App")!]);
        Assert.IsNull(LibraryImportContext.ForCompilation(compilation));
        LibraryImportContext.Configure(compilation, [new("Ordinary", fixture.LibraryPath)]);
        LibraryImportContext.AddRoots(compilation, [compilation.GetTypeByMetadataName("App")!]);
        Assert.HasCount(1, LibraryImportContext.ForCompilation(compilation)!.BuildModules());
        var destination = compilation.WithAssemblyName("DifferentConsumer");
        LibraryImportContext.Transfer(compilation, destination);
        LibraryImportContext.AddRoots(destination, [destination.GetTypeByMetadataName("App")!]);
        Assert.HasCount(1, LibraryImportContext.ForCompilation(destination)!.BuildModules());
        LibraryImportContext.Transfer(CSharpCompilation.Create("NoInputs"), CSharpCompilation.Create("NoDestinationInputs"));
    }

    [TestMethod]
    [DataRow("missing-delimiter")]
    [DataRow("|path")]
    [DataRow("asset|")]
    [DataRow("unknown|Ordinary")]
    [DataRow("asset|Ordinary")]
    [DataRow("asset|Missing|")]
    public void Generator_InvalidImplementationManifest_ReportsActionableDiagnostic(string manifest)
    {
        var compilation = CSharpCompilation.Create("Consumer", [CSharpSyntaxTree.ParseText("""
            using ECMAScript;
            [ECMAScriptModule("app.mjs")]
            public static class App { public static int Run() => 1; }
            """, TestMetadataReferences.PreviewParseOptions)], TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create([new ESGenerator().AsSourceGenerator()],
            additionalTexts: [new ImportManifest(manifest)], parseOptions: TestMetadataReferences.PreviewParseOptions);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);
        Assert.IsTrue(diagnostics.Any(static diagnostic => diagnostic.Id == "JAZORG001"));
        Assert.IsFalse(diagnostics.Any(static diagnostic => diagnostic.Id is "CS8785" or "CS8784"));
    }

    [TestMethod]
    public void ImportPlan_UnchangedInputsReuseArtifacts_ChangedImplementationInvalidatesSameCompilation()
    {
        var root = RepositoryTemp.CreateDirectory("ordinary-invalidation-");
        try
        {
            var path = Path.Combine(root, "Cached.dll");
            var original = Image(1);
            File.WriteAllBytes(path, original);
            var consumer = CSharpCompilation.Create("Consumer", [CSharpSyntaxTree.ParseText("""
                using ECMAScript;
                [ECMAScriptModule("app.mjs")]
                public static class App { public static int Run() => Cached.Read(); }
                """, TestMetadataReferences.PreviewParseOptions)], TestMetadataReferences.Net11
                    .Add(MetadataReference.CreateFromImage(original))
                    .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location)),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var input = new LibraryImplementationReference("Cached", path) { ContentHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(original)) };
            LibraryImportContext.Configure(consumer, [input]);
            var first = LibraryImportContext.ForCompilation(consumer)!;
            var firstModules = first.BuildModules();
            LibraryImportContext.Configure(consumer, [input]);
            Assert.AreSame(first, LibraryImportContext.ForCompilation(consumer));
            Assert.AreSame(firstModules, first.BuildModules());
            var changed = Image(2);
            File.WriteAllBytes(path, changed);
            LibraryImportContext.Configure(consumer, [input with { ContentHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(changed)) }]);
            var second = LibraryImportContext.ForCompilation(consumer)!;
            Assert.AreNotSame(first, second);
            var secondModules = second.BuildModules();
            Assert.AreNotEqual(firstModules.Single().RelativePath, secondModules.Single().RelativePath);
            Assert.AreNotEqual(firstModules.Single().Artifact.Content, secondModules.Single().Artifact.Content);
            LibraryImportContext.Configure(consumer, []);
            Assert.IsNull(LibraryImportContext.ForCompilation(consumer));
        }
        finally { Directory.Delete(root, recursive: true); }

        static byte[] Image(int value)
        {
            var library = CSharpCompilation.Create("Cached", [CSharpSyntaxTree.ParseText("public static class Cached { public static int Read() => " + value + "; }")],
                TestMetadataReferences.Net11, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var image = new MemoryStream();
            var emit = library.Emit(image);
            Assert.IsTrue(emit.Success, string.Join(Environment.NewLine, emit.Diagnostics));
            return image.ToArray();
        }
    }

    [TestMethod]
    public async Task Import_DirectCallAcrossAssemblies_UsesSharedDependencyModule()
    {
        var root = RepositoryTemp.CreateDirectory("ordinary-cross-assembly-");
        try
        {
            var otherPath = Path.Combine(root, "Other.dll");
            Emit("Other", "public static class Other { public static int Read(int value) => value * 3; }", otherPath);
            var ordinaryPath = Path.Combine(root, "Ordinary.dll");
            Emit("Ordinary", "public static class Entry { public static int Run() => Other.Read(4); }", ordinaryPath,
                MetadataReference.CreateFromFile(otherPath));
            using var fixture = new Fixture(new FileInfo(ordinaryPath), new FileInfo(otherPath));
            await fixture.Compile("public static int Run() => Entry.Run();");
            await fixture.AssertRun(12);
        }
        finally { Directory.Delete(root, recursive: true); }

        static void Emit(string name, string source, string path, params MetadataReference[] references)
        {
            var compilation = CSharpCompilation.Create(name, [CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions)],
                TestMetadataReferences.Net11.AddRange(references), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var result = compilation.Emit(path);
            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
        }
    }

    [TestMethod]
    public async Task Import_GenericMethodOverloads_PreservesConcreteVersusTypeParameterBinding()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public static class Generic
            {
                public static T Identity<T>(T value) => value;
                public static int Identity(int value) => value + 1;
            }
            """);
        await fixture.Compile("public static int Run() => Ordinary.Generic.Identity<int>(4) + Ordinary.Generic.Identity(4);");
        await fixture.AssertRun(9);
    }

    [TestMethod]
    public async Task Import_VirtualCallThroughBase_PreservesReachableOverride()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public class Base { public virtual int Read() => 1; }
            public sealed class Derived : Base { public override int Read() => 9; }
            public static class Factory { public static Base Create() => new Derived(); }
            """);
        await fixture.Compile("public static int Run() => Ordinary.Factory.Create().Read();");
        await fixture.AssertRun(9);
    }

    [TestMethod]
    public async Task Import_ReadOnlyUsage_DoesNotCompileUnsupportedUnusedSetter()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public sealed class Model
            {
                public int Value { get => 7; set { System.IO.File.Exists(value.ToString()); } }
            }
            """);
        await fixture.Compile("public static int Run() => new Ordinary.Model().Value;");
        await fixture.AssertRun(7);
    }

    [TestMethod]
    public async Task Import_ReferenceAssembly_PairsWithVersionedImplementationAndOverloads()
    {
        using var fixture = new Fixture("""
            [assembly: System.Reflection.AssemblyVersion("3.2.1.0")]
            namespace Ordinary;
            public static class Helpers
            {
                public static int Read(int value) => value + 4;
                public static int Read(string value) => value.Length;
            }
            """, useReferenceAssembly: true);
        await fixture.Compile("public static int Run() => Ordinary.Helpers.Read(3) + Ordinary.Helpers.Read(\"abc\");");
        await fixture.AssertRun(10);
    }

    [TestMethod]
    public async Task Import_StaticProperties_PreservesReadWriteAndBackingStorage()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public static class State { public static int Value { get; set; } = 4; }
            """);
        await fixture.Compile("public static int Run() { Ordinary.State.Value += 3; return Ordinary.State.Value; }");
        await fixture.AssertRun(7);
    }

    [TestMethod]
    public async Task Import_ConstructorSelection_PrunesUnusedUnsupportedOverload()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public sealed class Model
            {
                public int Value;
                public Model(int value) { Value = value; }
                public Model(string path) { Value = System.IO.File.Exists(path) ? 9 : 0; }
            }
            """);
        await fixture.Compile("public static int Run() => new Ordinary.Model(5).Value;");
        await fixture.AssertRun(5);
    }

    [TestMethod]
    public async Task Generator_TwoModules_EmitsOneSharedLibraryStateAndRecoveredSourceMap()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public static class State { private static int value; public static int Next() => ++value; }
            """);
        var modules = fixture.Generate("""
            using ECMAScript;
            [ECMAScriptModule("first.mjs")]
            public static class First { public static int Run() => Ordinary.State.Next(); }
            [ECMAScriptModule("second.mjs")]
            public static class Second { public static int Run() => Ordinary.State.Next(); }
            """);
        Assert.HasCount(3, modules);
        var library = modules.Single(module => module.RelativePath.StartsWith("_jazor/", StringComparison.Ordinal));
        StringAssert.Contains(library.SourceMapContent!, "jazor-recovered");
        Assert.DoesNotContain(fixture.Root, library.SourceMapContent!, StringComparison.Ordinal);
        await fixture.AssertSharedModules();
    }

    [TestMethod]
    public async Task Import_PublishedFastenshtein_PreservesResultsWithoutCompilingUnusedSpanOverload()
    {
        using var fixture = new Fixture(new FileInfo(typeof(Fastenshtein.Levenshtein).Assembly.Location));
        await fixture.Compile("public static int Run() => Fastenshtein.Levenshtein.Distance(\"kitten\", \"sitting\");");
        await fixture.AssertRun(Fastenshtein.Levenshtein.Distance("kitten", "sitting"));
    }

    [TestMethod]
    public async Task Import_UnannotatedDll_CompilesSelectedOverloadAndTransitivePrivateMethod()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public static class Algorithms
            {
                public static int Sum(int[] values) { int result = 0; foreach (int value in values) result += Twice(value); return result; }
                private static int Twice(int value) => value * 2;
                public static bool Sum(string serverOnly) => System.IO.File.Exists(serverOnly);
            }
            """);
        var output = await fixture.Compile("public static int Run() => Ordinary.Algorithms.Sum(new[] { 1, 3 });");
        Assert.DoesNotContain("File.Exists", output, StringComparison.Ordinal);
        await fixture.AssertRun(8);
    }

    [TestMethod]
    public async Task Import_ReachableUnsupportedApi_FailsAtRecoveredUsage()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public static class Backend { public static bool Read() => System.IO.File.Exists("server"); }
            """);
        var error = await Assert.ThrowsAsync<OperationTransformationException>(() => fixture.Compile("public static bool Run() => Ordinary.Backend.Read();"));
        StringAssert.Contains(error.Message, "System.IO.File");
    }

    [TestMethod]
    public async Task Import_InstanceClass_PreservesConstructorPrivateFieldAndProperty()
    {
        using var fixture = new Fixture("""
            namespace Ordinary;
            public sealed class Counter
            {
                private int value;
                public Counter(int initial) { value = initial; }
                public int Add(int step) { value += step; return value; }
                public int Value => value;
                public bool BackendOnly() => System.IO.File.Exists("server");
            }
            """);
        await fixture.Compile("public static int Run() { var counter = new Ordinary.Counter(4); counter.Add(3); return counter.Value; }");
        await fixture.AssertRun(7);
    }

    private sealed class Fixture : IDisposable
    {
        private readonly string _root = RepositoryTemp.CreateDirectory("ordinary-import-");
        private readonly string _libraryPath;
        private readonly string _assemblyName = "Ordinary";
        private readonly string? _referencePath;
        private readonly FileInfo[] _dependencies = [];
        internal string Root => _root;
        internal string LibraryPath => _libraryPath;
        internal string ReferencePath => _referencePath!;

        internal Fixture(FileInfo file, params FileInfo[] dependencies)
        {
            _libraryPath = file.FullName;
            _assemblyName = Path.GetFileNameWithoutExtension(file.FullName);
            _dependencies = dependencies;
        }

        internal Fixture(string source, bool useReferenceAssembly = false, string assemblyName = "Ordinary", params FileInfo[] dependencies)
        {
            _assemblyName = assemblyName;
            _dependencies = dependencies;
            _libraryPath = Path.Combine(_root, assemblyName + ".dll");
            var compilation = Create(assemblyName, source, dependencies.Select(file => MetadataReference.CreateFromFile(file.FullName)).ToArray());
            var result = compilation.Emit(_libraryPath);
            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
            if (useReferenceAssembly)
            {
                _referencePath = Path.Combine(_root, "Ordinary.ref.dll");
                using var stream = File.Create(_referencePath);
                var metadata = compilation.Emit(stream, options: new EmitOptions(metadataOnly: true, includePrivateMembers: false));
                Assert.IsTrue(metadata.Success, string.Join(Environment.NewLine, metadata.Diagnostics));
            }
        }

        internal async Task<string> Compile(string members)
        {
            var compilation = Consumer(members);
            var errors = compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
            Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
            var app = compilation.GetTypeByMetadataName("App")!;
            var plan = OrdinaryLibraryImportPlan.Create(compilation,
                new[] { new LibraryImplementationReference(_assemblyName, _libraryPath) }
                    .Concat(_dependencies.Select(file => new LibraryImplementationReference(Path.GetFileNameWithoutExtension(file.Name), file.FullName))), [app]);
            var modules = plan.BuildModules();
            foreach (var module in modules)
            {
                var path = Path.Combine(_root, module.RelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await File.WriteAllTextAsync(path, module.Artifact.Content);
            }
            var output = (await new AstConverter(app, compilation.GetSemanticModel(compilation.SyntaxTrees.Single()),
                new AstConverterOptions(AstConverterProfile.Standard, CurrentModuleOutputPath: "app.mjs") { LibraryImports = plan }).Convert())!.ToKnRECMAScript();
            _ = new Parser().ParseModule(output);
            await File.WriteAllTextAsync(Path.Combine(_root, "app.mjs"), output);
            return output + string.Join("\n", modules.Select(static module => module.Artifact.Content));
        }

        internal CSharpCompilation Consumer(string members)
            => Create("Consumer", "public static class App { " + members + " }",
                new[] { MetadataReference.CreateFromFile(_referencePath ?? _libraryPath) }
                    .Concat(_dependencies.Select(file => MetadataReference.CreateFromFile(file.FullName))).ToArray());

        internal OrdinaryLibraryImportPlan Plan(string members, string? implementation = null, string? hash = null)
        {
            var compilation = Consumer(members);
            return OrdinaryLibraryImportPlan.Create(compilation,
                [new LibraryImplementationReference(_assemblyName, implementation ?? _libraryPath) { ContentHash = hash }],
                [compilation.GetTypeByMetadataName("App")!]);
        }

        internal IReadOnlyList<GeneratedModule> Generate(string source)
        {
            var input = Create("Consumer", source, MetadataReference.CreateFromFile(_libraryPath),
                MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location));
            GeneratorDriver driver = CSharpGeneratorDriver.Create([new ESGenerator().AsSourceGenerator()],
                additionalTexts: [new ImportManifest("asset|" + _assemblyName + "|" + _libraryPath)],
                parseOptions: TestMetadataReferences.PreviewParseOptions);
            driver = driver.RunGeneratorsAndUpdateCompilation(input, out var output, out var diagnostics);
            Assert.IsEmpty(diagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), string.Join(Environment.NewLine, diagnostics));
            using var image = new MemoryStream();
            var result = output.Emit(image);
            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
            var assembly = System.Reflection.Assembly.Load(image.ToArray());
            var catalog = assembly.GetType("Jazor.Generated.ModuleCatalog")!;
            var rows = (System.Collections.IEnumerable)catalog.GetMethod("GetModules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.Invoke(null, null)!;
            var modules = new List<GeneratedModule>();
            foreach (var row in rows)
            {
                var type = row!.GetType();
                string Read(string name) => (string)type.GetProperty(name)!.GetValue(row)!;
                var module = new GeneratedModule(Read("RelativePath"), Read("Content"), (string?)type.GetProperty("SourceMapContent")!.GetValue(row));
                modules.Add(module);
                var path = Path.Combine(_root, module.RelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, module.Content);
            }
            return modules;
        }

        internal async Task AssertSharedModules()
        {
            var path = Path.Combine(_root, "test.mjs");
            await File.WriteAllTextAsync(path, """
                import { Run as first } from './first.mjs';
                import { Run as second } from './second.mjs';
                globalThis.__jazorTest('shared ordinary library state', () => {
                    const values = [first(), second(), first()];
                    if (JSON.stringify(values) !== '[1,2,3]') throw new Error(`Split state: ${values}`);
                });
                """);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await NodeModuleRunner.RunAsync(_root, path, cancellationToken: timeout.Token);
        }

        internal async Task AssertRun(int expected)
        {
            var repository = Directory.GetParent(RepositoryTemp.Root)!.Parent!.FullName;
            foreach (var source in Directory.EnumerateFiles(Path.Combine(repository, "src", "ECMAScript", "clr"), "*.js", SearchOption.AllDirectories))
            {
                var destination = Path.Combine(_root, "clr", Path.GetRelativePath(Path.Combine(repository, "src", "ECMAScript", "clr"), source));
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(source, destination, overwrite: true);
            }
            var path = Path.Combine(_root, "test.mjs");
            await File.WriteAllTextAsync(path, $$"""
                import { Run } from './app.mjs';
                globalThis.__jazorTest('ordinary DLL execution', () => {
                    const actual = Run();
                    if (actual !== {{expected}}) throw new Error(`Expected {{expected}}; got ${actual}`);
                });
                """);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await NodeModuleRunner.RunAsync(_root, path, cancellationToken: timeout.Token);
        }

        private static CSharpCompilation Create(string name, string source, params MetadataReference[] references)
            => CSharpCompilation.Create(name, [CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions, name + ".cs")],
                TestMetadataReferences.Net11.AddRange(references),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));

        public void Dispose() => Directory.Delete(_root, recursive: true);
    }

    private sealed record GeneratedModule(string RelativePath, string Content, string? SourceMapContent);

    private sealed class ImportManifest(string content) : AdditionalText
    {
        public override string Path => "consumer.jazorimports";
        public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(content);
    }

    private sealed class MissingManifest : AdditionalText
    {
        public override string Path => "missing.jazorimports";
        public override SourceText? GetText(CancellationToken cancellationToken = default) => null;
    }

    private sealed class UnrelatedAdditionalFile : AdditionalText
    {
        public override string Path => "unrelated.txt";
        public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From("ignored");
    }
}
