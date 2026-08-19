using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using Acornima.Ast;
using Jazor.Compiler;
using Jazor.RazorVue.Generation;
using Jazor.RazorVue.RazorSdk;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueCoverageBoundaryCompletionTests
{
    [TestMethod]
    public void NativeHookHelpers_ResolveIndirectX64EntryAndValidateConstructorArguments()
    {
        var resolve = GetPrivateStatic(
            typeof(InitializeNativeHook),
            "ResolvePatchAddress",
            typeof(IntPtr),
            typeof(int));
        var memory = Marshal.AllocHGlobal(64);
        try
        {
            var bytes = new byte[64];
            Marshal.Copy(bytes, 0, memory, bytes.Length);
            Assert.AreEqual(memory, (IntPtr)resolve.Invoke(null, [memory, 12])!);

            // x64 ReadyToRun/JIT stubs may start with `jmp [rip+rel32]`. Resolve the
            // indirection and cover both a valid target cell and a null cell.
            bytes[0] = 0xFF;
            bytes[1] = 0x25;
            BitConverter.GetBytes(10).CopyTo(bytes, 2);
            var destination = IntPtr.Add(memory, 32);
            Marshal.Copy(bytes, 0, memory, bytes.Length);
            Marshal.Copy(BitConverter.GetBytes(destination.ToInt64()), 0, IntPtr.Add(memory, 16), IntPtr.Size);
            Assert.AreEqual(destination, (IntPtr)resolve.Invoke(null, [memory, 12])!);

            Marshal.Copy(new byte[IntPtr.Size], 0, IntPtr.Add(memory, 16), IntPtr.Size);
            Assert.AreEqual(memory, (IntPtr)resolve.Invoke(null, [memory, 12])!);
        }
        finally
        {
            Marshal.FreeHGlobal(memory);
        }

        var constructor = typeof(InitializeNativeHook).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            [typeof(MethodInfo), typeof(MethodInfo)],
            modifiers: null);
        Assert.IsNotNull(constructor);
        var target = typeof(InitializeNativeHook).GetMethod(
            "SelfTestTarget",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(target);

        var missingTarget = Assert.Throws<TargetInvocationException>(() =>
            constructor!.Invoke([null, target]));
        Assert.IsInstanceOfType<ArgumentNullException>(missingTarget.InnerException);

        var missingReplacement = Assert.Throws<TargetInvocationException>(() =>
            constructor!.Invoke([target, null]));
        Assert.IsInstanceOfType<ArgumentNullException>(missingReplacement.InnerException);
    }

    [TestMethod]
    public void NativeHookHelpers_EncodeEachSupportedJumpAndRejectUnknownArchitecture()
    {
        var buildJump = GetPrivateStatic(
            typeof(InitializeNativeHook),
            "BuildJump",
            typeof(IntPtr),
            typeof(Architecture));
        var destination = new IntPtr(unchecked((long)0x1020304050607080));

        var x64 = (byte[])buildJump.Invoke(null, [destination, Architecture.X64])!;
        CollectionAssert.AreEqual(
            new byte[] { 0x48, 0xB8, 0x80, 0x70, 0x60, 0x50, 0x40, 0x30, 0x20, 0x10, 0xFF, 0xE0 },
            x64);

        var arm64 = (byte[])buildJump.Invoke(null, [destination, Architecture.Arm64])!;
        Assert.AreEqual(16, arm64.Length);
        CollectionAssert.AreEqual(new byte[] { 0x50, 0x00, 0x00, 0x58, 0x00, 0x02, 0x1F, 0xD6 }, arm64[..8]);
        CollectionAssert.AreEqual(new byte[] { 0x80, 0x70, 0x60, 0x50, 0x40, 0x30, 0x20, 0x10 }, arm64[8..]);

        var unsupported = Assert.Throws<TargetInvocationException>(() =>
            buildJump.Invoke(null, [destination, Architecture.X86]));
        Assert.IsInstanceOfType<PlatformNotSupportedException>(unsupported.InnerException);
    }

    [TestMethod]
    public void NativeHookHelpers_ValidateSelfTestMethodAndResultContracts()
    {
        var validateMethods = GetPrivateStatic(
            typeof(InitializeNativeHook),
            "TryValidateSelfTestMethods",
            typeof(MethodInfo),
            typeof(MethodInfo),
            typeof(string).MakeByRefType());
        var target = typeof(InitializeNativeHook).GetMethod("SelfTestTarget", BindingFlags.Static | BindingFlags.NonPublic)!;
        var replacement = typeof(InitializeNativeHook).GetMethod("SelfTestReplacement", BindingFlags.Static | BindingFlags.NonPublic)!;

        var missing = new object?[] { null, replacement, null };
        Assert.IsFalse((bool)validateMethods.Invoke(null, missing)!);
        StringAssert.Contains((string)missing[2]!, "could not be resolved", StringComparison.Ordinal);
        var validMethods = new object?[] { target, replacement, null };
        Assert.IsTrue((bool)validateMethods.Invoke(null, validMethods)!);
        Assert.AreEqual(string.Empty, validMethods[2]);

        var validateResults = GetPrivateStatic(
            typeof(InitializeNativeHook),
            "TryValidateSelfTestResults",
            typeof(int),
            typeof(int),
            typeof(int),
            typeof(string).MakeByRefType());
        var patchFailure = new object?[] { 42, 0, 42, null };
        Assert.IsFalse((bool)validateResults.Invoke(null, patchFailure)!);
        StringAssert.Contains((string)patchFailure[3]!, "after patching", StringComparison.Ordinal);
        var restoreFailure = new object?[] { 43, 1, 41, null };
        Assert.IsFalse((bool)validateResults.Invoke(null, restoreFailure)!);
        StringAssert.Contains((string)restoreFailure[3]!, "after unpatching", StringComparison.Ordinal);
        var validResults = new object?[] { 43, 1, 42, null };
        Assert.IsTrue((bool)validateResults.Invoke(null, validResults)!);
        Assert.AreEqual(string.Empty, validResults[3]);
    }

    [TestMethod]
    public void NativeHookHelpers_ExerciseOwnedWindowsWriteAndEmptyWriteBoundaries()
    {
        var writeBytes = GetPrivateStatic(typeof(InitializeNativeHook), "WriteBytes", typeof(IntPtr), typeof(byte[]));
        var writeBytesWindows = GetPrivateStatic(
            typeof(InitializeNativeHook),
            "WriteBytesWindows",
            typeof(IntPtr),
            typeof(byte[]));
        var flushUnix = GetPrivateStatic(
            typeof(InitializeNativeHook),
            "FlushInstructionCacheUnix",
            typeof(IntPtr),
            typeof(int));

        // Empty patches are a valid no-op used by callers that computed an empty instruction
        // sequence; they must not enter either platform P/Invoke path.
        writeBytes.Invoke(null, [IntPtr.Zero, Array.Empty<byte>()]);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var memory = Marshal.AllocHGlobal(64);
            try
            {
                writeBytes.Invoke(null, [memory, new byte[] { 0xC3 }]);
                writeBytesWindows.Invoke(null, [memory, new byte[] { 0xC3 }]);
            }
            finally
            {
                Marshal.FreeHGlobal(memory);
            }

            // A non-writable address must surface the native protection failure instead of
            // being treated as a successful patch. This is the installer failure contract.
            var protectionFailure = Assert.Throws<TargetInvocationException>(() =>
                writeBytesWindows.Invoke(null, [IntPtr.Zero, new byte[] { 0xC3 }]));
            Assert.IsInstanceOfType<InvalidOperationException>(protectionFailure.InnerException);
            StringAssert.Contains(protectionFailure.InnerException!.Message, "VirtualProtect failed", StringComparison.Ordinal);
        }

        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            flushUnix.Invoke(null, [IntPtr.Zero, 0]);

        var target = typeof(InitializeNativeHook).GetMethod(
            "SelfTestTarget",
            BindingFlags.Static | BindingFlags.NonPublic);
        var replacement = typeof(InitializeNativeHook).GetMethod(
            "SelfTestReplacement",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(target);
        Assert.IsNotNull(replacement);
        var constructor = typeof(InitializeNativeHook).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            [typeof(MethodInfo), typeof(MethodInfo)],
            modifiers: null);
        Assert.IsNotNull(constructor);
        using var hook = (InitializeNativeHook)constructor!.Invoke([target, replacement]);
        Assert.IsFalse(hook.IsCurrentTargetPatched());
    }

    [TestMethod]
    public void DiagnosticAndBinderBoundaries_PreserveLocationsAndAggregateIndependentFailures()
    {
        var source = CSharpSyntaxTree.ParseText(
            "namespace Demo; public sealed class Component { public void Render() { } }",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/Component.razor");
        var compilation = CSharpCompilation.Create(
            "RazorVue.Coverage.Diagnostics",
            [source],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var component = compilation.GetTypeByMetadataName("Demo.Component");
        Assert.IsNotNull(component);
        var location = source.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .Single().Identifier.GetLocation();
        var external = Location.Create(
            "external",
            new TextSpan(0, 1),
            new LinePositionSpan(new LinePosition(0, 0), new LinePosition(0, 1)));

        var diagnostic = RazorVueDiagnosticFactory.Create(
            RazorVueDiagnosticCategory.DirectRender,
            "dedupe",
            primaryLocation: Location.None,
            component: component,
            additionalLocations: ImmutableArray.Create(location, location, external, Location.None));
        Assert.AreEqual("Pages/Component.razor", diagnostic.PrimaryLocation.GetLineSpan().Path);
        Assert.HasCount(1, diagnostic.AdditionalLocations);

        var comparerType = typeof(RazorVueDiagnosticFactory).GetNestedType("LocationComparer", BindingFlags.NonPublic);
        Assert.IsNotNull(comparerType);
        var comparer = comparerType!.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(null);
        var equals = comparerType.GetMethod("Equals", [typeof(Location), typeof(Location)])!;
        var hash = comparerType.GetMethod("GetHashCode", [typeof(Location)])!;
        Assert.IsFalse((bool)equals.Invoke(comparer, [location, external])!);
        Assert.IsFalse((bool)equals.Invoke(comparer, [external, location])!);
        Assert.IsFalse((bool)equals.Invoke(comparer, [null, location])!);
        Assert.IsFalse((bool)equals.Invoke(comparer, [location, null])!);
        Assert.IsTrue((bool)equals.Invoke(comparer, [location, location])!);
        var equivalentLocation = Location.Create(
            "Pages/Component.razor",
            location.SourceSpan,
            location.GetLineSpan().Span);
        Assert.IsTrue((bool)equals.Invoke(comparer, [location, equivalentLocation])!);
        // Keep the source span equal while changing the author path, then keep both the
        // path and source span equal while changing the mapped line span. These are distinct
        // comparer rejection paths and matter for deterministic diagnostic de-duplication.
        var differentPath = Location.Create(
            "Pages/Other.razor",
            location.SourceSpan,
            location.GetLineSpan().Span);
        Assert.IsFalse((bool)equals.Invoke(comparer, [location, differentPath])!);
        var differentLineSpan = Location.Create(
            "Pages/Component.razor",
            location.SourceSpan,
            new LinePositionSpan(
                new LinePosition(10, 0),
                new LinePosition(10, 1)));
        Assert.IsFalse((bool)equals.Invoke(comparer, [location, differentLineSpan])!);
        var zeroPath = Location.Create(
            "zero.razor",
            new TextSpan(0, 0),
            new LinePositionSpan(
                new LinePosition(0, 0),
                new LinePosition(0, 0)));
        // Location.None has no source tree/path. Pairing it with an otherwise equivalent source
        // location reaches each comparer fallback without fabricating an invalid Roslyn location.
        Assert.IsFalse((bool)equals.Invoke(comparer, [Location.None, zeroPath])!);
        Assert.IsFalse((bool)equals.Invoke(comparer, [zeroPath, Location.None])!);
        _ = hash.Invoke(comparer, [external]);
        _ = hash.Invoke(comparer, [Location.None]);

        Assert.AreEqual(Location.None, RazorVueDiagnosticFactory.GetSymbolLocation(null));
        Assert.AreEqual(
            Location.None,
            RazorVueDiagnosticFactory.GetSymbolLocation(compilation.GetSpecialType(SpecialType.System_String)));
        var syntaxDiagnostic = RazorVueDiagnosticFactory.FromException(
            new SyntaxNodeTransformationException(
                Microsoft.CodeAnalysis.CSharp.SyntaxKind.ClassDeclaration,
                "syntax",
                location),
            RazorVueDiagnosticCategory.DirectRender,
            component);
        var symbolDiagnostic = RazorVueDiagnosticFactory.FromException(
            new SymbolTransformationException(
                Microsoft.CodeAnalysis.SymbolKind.NamedType,
                "symbol",
                location),
            RazorVueDiagnosticCategory.DirectRender,
            component);
        Assert.AreEqual(RazorVueDiagnosticCategory.CompilerBridge, syntaxDiagnostic.Category);
        Assert.AreEqual(RazorVueDiagnosticCategory.CompilerBridge, symbolDiagnostic.Category);

        var binderSource = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components.Rendering;
            public sealed class Missing { }
            public sealed class ExpressionBody
            {
                public void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, "expression");
            }
            public sealed class Valid
            {
                public void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "valid");
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/Binding.razor.g.cs");
        var binderCompilation = CSharpCompilation.Create(
            "RazorVue.Coverage.Binder",
            [binderSource],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var symbols = new[] { "Missing", "ExpressionBody", "Valid" }
            .Select(name => binderCompilation.GetTypeByMetadataName(name)!)
            .ToImmutableArray();

        var bound = GeneratedCSharpBinder.TryBindFinalCompilationWithDiagnostics(
            binderCompilation,
            symbols,
            out var binding,
            out var diagnostics);
        Assert.IsFalse(bound);
        Assert.IsNull(binding);
        Assert.HasCount(2, diagnostics);
        StringAssert.Contains(diagnostics[0].Message, "did not", StringComparison.Ordinal);

        var valid = GeneratedCSharpBinder.TryBindFinalCompilationWithDiagnostics(
            binderCompilation,
            ImmutableArray.Create(symbols[2]),
            out var validBinding,
            out var validDiagnostics);
        Assert.IsTrue(valid);
        Assert.IsEmpty(validDiagnostics);
        Assert.IsNotNull(validBinding);
        Assert.AreEqual(1, validBinding!.Components.Length);

        var handwrittenSource = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components.Rendering;
            public sealed class Handwritten
            {
                public void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "handwritten");
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/Binding.razor.cs");
        var handwrittenCompilation = CSharpCompilation.Create(
            "RazorVue.Coverage.HandwrittenBinder",
            [handwrittenSource],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var handwrittenSymbol = handwrittenCompilation.GetTypeByMetadataName("Handwritten");
        Assert.IsNotNull(handwrittenSymbol);
        Assert.IsTrue(GeneratedCSharpBinder.TryBindHandwrittenWithDiagnostics(
            handwrittenCompilation,
            ImmutableArray.Create(handwrittenSymbol!),
            out var handwrittenBinding,
            out var handwrittenDiagnostics));
        Assert.IsNotNull(handwrittenBinding);
        Assert.IsEmpty(handwrittenDiagnostics);
    }

    [TestMethod]
    public void GeneratedSourceBindingAndDiagnostics_PreserveMappedRazorSourceIdentity()
    {
        var generatedTree = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components.Rendering;
            #line 12 "Pages/MappedComponent.razor"
            public sealed class MappedComponent
            {
                public void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "mapped");
                }
            }
            #line default
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "obj/MappedComponent.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.Coverage.MappedSource",
            [generatedTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var component = compilation.GetTypeByMetadataName("MappedComponent");
        Assert.IsNotNull(component);

        Assert.IsTrue(GeneratedCSharpBinder.TryBindFinalCompilationWithDiagnostics(
            compilation,
            ImmutableArray.Create(component!),
            out var binding,
            out var diagnostics));
        Assert.IsNotNull(binding);
        Assert.IsEmpty(diagnostics);
        Assert.AreEqual("Pages/MappedComponent.razor", binding!.Documents.Single().SourcePath);

        var generatedIdentifier = generatedTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .Single()
            .Identifier
            .GetLocation();
        var authorLocation = RazorVueDiagnosticFactory.ToAuthorLocation(generatedIdentifier);
        Assert.AreEqual("Pages/MappedComponent.razor", authorLocation.GetLineSpan().Path);
        Assert.AreEqual(11, authorLocation.GetLineSpan().StartLinePosition.Line);
        Assert.AreEqual(Location.None, RazorVueDiagnosticFactory.ToAuthorLocation(Location.None));

        var describeCurrentPlatform = GetPrivateStatic(typeof(InitializeNativeHook), "DescribeCurrentPlatform");
        var description = (string)describeCurrentPlatform.Invoke(null, null)!;
        StringAssert.Contains(description, "OS:", StringComparison.Ordinal);
        StringAssert.Contains(description, "Architecture:", StringComparison.Ordinal);
    }

    [TestMethod]
    public void ComponentConventionsAndInjectHelpers_ClassifyTypedParametersAndMalformedMetadata()
    {
        var tree = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components;
            namespace Fake { public sealed class EventCallback { } public sealed class RenderFragment { } }
            public sealed class ParameterHost
            {
                [Parameter] public string Value { get; set; } = "";
                public int Plain { get; set; }
                [Parameter] public RenderFragment? Content { get; set; }
                [Parameter] public EventCallback Changed { get; set; }
                [Parameter(CaptureUnmatchedValues = true)] public System.Collections.Generic.Dictionary<string, object> Extra { get; set; } = new();
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/ParameterHost.razor.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.Coverage.Parameters",
            [tree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var host = compilation.GetTypeByMetadataName("ParameterHost");
        Assert.IsNotNull(host);

        var names = LibraryComponentConventions.BuildParameterRuntimeNameMap(host!);
        Assert.IsEmpty(names);
        var conventions = typeof(LibraryComponentConventions);
        var isEvent = GetPrivateStatic(conventions, "IsEventCallback", typeof(ITypeSymbol));
        var isFragment = GetPrivateStatic(conventions, "IsRenderFragment", typeof(ITypeSymbol));
        Assert.IsFalse((bool)isEvent.Invoke(null, [null])!);
        Assert.IsFalse((bool)isFragment.Invoke(null, [null])!);
        var fakeEventCallback = compilation.GetTypeByMetadataName("Fake.EventCallback");
        var fakeRenderFragment = compilation.GetTypeByMetadataName("Fake.RenderFragment");
        Assert.IsNotNull(fakeEventCallback);
        Assert.IsNotNull(fakeRenderFragment);
        Assert.IsFalse((bool)isEvent.Invoke(null, [fakeEventCallback])!);
        Assert.IsFalse((bool)isFragment.Invoke(null, [fakeRenderFragment])!);
        Assert.IsTrue((bool)isEvent.Invoke(null, [host!.GetMembers("Changed").OfType<IPropertySymbol>().Single().Type])!);
        Assert.IsTrue((bool)isFragment.Invoke(null, [host.GetMembers("Content").OfType<IPropertySymbol>().Single().Type])!);
        var runtimeEventCallback = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.EventCallback");
        var runtimeRenderFragment = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.RenderFragment");
        Assert.IsNotNull(runtimeEventCallback);
        Assert.IsNotNull(runtimeRenderFragment);
        Assert.IsTrue((bool)isEvent.Invoke(null, [runtimeEventCallback])!);
        Assert.IsTrue((bool)isFragment.Invoke(null, [runtimeRenderFragment])!);

        var plain = host.GetMembers("Plain").OfType<IPropertySymbol>().Single();
        Assert.IsFalse(LibraryComponentConventions.IsParameterProperty(plain));
        var parameterValue = host.GetMembers("Value").OfType<IPropertySymbol>().Single();
        Assert.IsTrue(LibraryComponentConventions.IsParameterProperty(parameterValue));

        var extra = host.GetMembers("Extra").OfType<IPropertySymbol>().Single();
        var captures = GetPrivateStatic(typeof(VueInjectRegistry), "CapturesUnmatchedValues", typeof(IPropertySymbol));
        Assert.IsTrue((bool)captures.Invoke(null, [extra])!);
        var value = host.GetMembers("Value").OfType<IPropertySymbol>().Single();
        Assert.IsFalse((bool)captures.Invoke(null, [value])!);

        var hasAttribute = GetPrivateStatic(typeof(VueInjectRegistry), "HasAttribute", typeof(ISymbol), typeof(string));
        Assert.IsTrue((bool)hasAttribute.Invoke(null, [value, "Microsoft.AspNetCore.Components.ParameterAttribute"])!);
        Assert.IsFalse((bool)hasAttribute.Invoke(null, [value, "Missing.Attribute"])!);
        Assert.IsFalse((bool)hasAttribute.Invoke(
            null,
            [host.GetMembers("Plain").OfType<IPropertySymbol>().Single(), "Missing.Attribute"])!);

        var readComponentType = GetPrivateStatic(typeof(VueInjectRegistry), "ReadComponentType", typeof(AttributeData), typeof(int), typeof(string));
        var parameterAttribute = value.GetAttributes().Single();
        var failure = Assert.Throws<TargetInvocationException>(() =>
            readComponentType.Invoke(null, [parameterAttribute, 0, "contract"]));
        Assert.IsInstanceOfType<RazorVueDiagnosticException>(failure.InnerException);
    }

    [TestMethod]
    public void TailOutputAndBinderBoundaries_HandleEmptyInputsAndPublicCompatibilityOverloads()
    {
        var emptyCompilation = CSharpCompilation.Create(
            "RazorVue.Coverage.EmptyTail",
            [],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Assert.IsTrue(RazorTailOutput.TryBuildFinalCompilationCatalog(
            emptyCompilation,
            CancellationToken.None,
            out var catalog,
            out var tailDiagnostics));
        Assert.IsNull(catalog);
        Assert.IsEmpty(tailDiagnostics);

        var workerCount = GetPrivateStatic(
            typeof(RazorTailOutput),
            "GetArtifactBuildWorkerCount",
            typeof(int));
        Assert.AreEqual(0, workerCount.Invoke(null, [0]));
        Assert.AreEqual(0, workerCount.Invoke(null, [-1]));
        Assert.AreEqual(1, workerCount.Invoke(null, [1]));
        Assert.AreEqual(4, workerCount.Invoke(null, [4]));
        Assert.AreEqual(4, workerCount.Invoke(null, [9]));

        var finalFailure = Assert.Throws<ArgumentNullException>(() =>
            GeneratedCSharpBinder.TryBindFinalCompilation(
                null!,
                ImmutableArray<INamedTypeSymbol>.Empty,
                out _,
                out _));
        Assert.AreEqual("compilation", finalFailure.ParamName);

        var handwrittenFailure = Assert.Throws<ArgumentNullException>(() =>
            GeneratedCSharpBinder.TryBindHandwritten(
                null!,
                ImmutableArray<INamedTypeSymbol>.Empty,
                out _,
                out _));
        Assert.AreEqual("compilation", handwrittenFailure.ParamName);
    }

    [TestMethod]
    public void InitializeHookInstallerBoundaries_ReportUnpublishedReplacementAndRetainFailure()
    {
        var setFailure = GetPrivateStatic(typeof(InitializeHookInstaller), "SetFailure", typeof(string));
        setFailure.Invoke(null, ["contract failure"]);
        Assert.AreEqual("contract failure", InitializeHookInstaller.GetInstallFailure());

        var initializedField = typeof(InitializeHookInstaller).GetField("_initialized", BindingFlags.Static | BindingFlags.NonPublic)!;
        var previousInitialized = initializedField.GetValue(null);
        var installerHookField = typeof(InitializeHookInstaller).GetField("_hook", BindingFlags.Static | BindingFlags.NonPublic)!;
        var previousInstallerHook = installerHookField.GetValue(null);
        try
        {
            // Once completion is published, a missing hook is a hard failure and must not
            // silently re-enter native installation.
            initializedField.SetValue(null, 1);
            installerHookField.SetValue(null, null);
            Assert.IsFalse(InitializeHookInstaller.TryInstall());
        }
        finally
        {
            initializedField.SetValue(null, previousInitialized);
            installerHookField.SetValue(null, previousInstallerHook);
        }

        var initializeReplacement = GetPrivateStatic(
            typeof(InitializeHookInstaller),
            "InitializeReplacement",
            typeof(Microsoft.CodeAnalysis.GeneratorDriver),
            typeof(Compilation),
            typeof(Compilation).MakeByRefType(),
            typeof(ImmutableArray<Diagnostic>).MakeByRefType(),
            typeof(CancellationToken));
        var hookField = typeof(InitializeHookInstaller).GetField("_hook", BindingFlags.Static | BindingFlags.NonPublic)!;
        var previousHook = hookField.GetValue(null);
        try
        {
            hookField.SetValue(null, null);
            var arguments = new object?[] { null, CSharpCompilation.Create("Empty"), null, null, CancellationToken.None };
            var failure = Assert.Throws<TargetInvocationException>(() => initializeReplacement.Invoke(null, arguments));
            Assert.IsInstanceOfType<InvalidOperationException>(failure.InnerException);
            StringAssert.Contains(failure.InnerException!.Message, "before its hook handle was published", StringComparison.Ordinal);
        }
        finally
        {
            hookField.SetValue(null, previousHook);
        }

        // Do not leak a synthetic failure into later generator tests.
        setFailure.Invoke(null, [null]);
    }

    [TestMethod]
    public void MemberClosureAutoPropertyHelpers_DistinguishAutoComputedAndMetadataProperties()
    {
        var source = CSharpSyntaxTree.ParseText(
            "public sealed class Properties { public int Auto { get; set; } public int Computed => 1; public int this[int index] => index; }",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Properties.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.Coverage.Properties",
            [source],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var properties = compilation.GetTypeByMetadataName("Properties")!;
        var auto = properties.GetMembers("Auto").OfType<IPropertySymbol>().Single();
        var computed = properties.GetMembers("Computed").OfType<IPropertySymbol>().Single();
        var indexer = properties.GetMembers().OfType<IPropertySymbol>().Single(property => property.IsIndexer);

        var closureBuilder = GetPrivateStatic(typeof(MemberClosureBuilder), "IsAutoProperty", typeof(IPropertySymbol));
        Assert.IsTrue((bool)closureBuilder.Invoke(null, [auto])!);
        Assert.IsFalse((bool)closureBuilder.Invoke(null, [computed])!);
        Assert.IsFalse((bool)closureBuilder.Invoke(null, [indexer])!);

        var metadataString = compilation.GetSpecialType(SpecialType.System_String);
        var metadataLength = metadataString.GetMembers("Length").OfType<IPropertySymbol>().Single();
        Assert.IsFalse((bool)closureBuilder.Invoke(null, [metadataLength])!);
    }

    [TestMethod]
    public void MemberClosureLifecycleHelpers_CoverConstructorChainsAndInterfaceDisposeDispatch()
    {
        var compilation = CreateSimpleCompilation(
            """
            using System;
            using System.Threading.Tasks;
            using Microsoft.AspNetCore.Components;

            public class LifecycleBase : ComponentBase, IDisposable
            {
                public LifecycleBase() { }
                protected override void OnInitialized() { }
                protected override void OnParametersSet() { }
                public virtual void Dispose() { }
            }

            public sealed class LifecycleDerived : LifecycleBase
            {
                protected override void OnParametersSet() { }
                public override void Dispose() { }
                public void Regular() { }
            }

            public sealed class ExplicitDisposable : ComponentBase, IDisposable
            {
                void IDisposable.Dispose() { }
            }

            public sealed class AsyncDisposable : ComponentBase, IAsyncDisposable
            {
                public ValueTask DisposeAsync() => ValueTask.CompletedTask;
            }

            public class ConstructorBase
            {
                public ConstructorBase(int value) { }
            }

            public sealed class ConstructorShapes : ConstructorBase
            {
                public ConstructorShapes() : this(1) { }
                public ConstructorShapes(int value) : base(value) { }
                public ConstructorShapes(string value) { }
            }
            """);

        var closureBuilder = typeof(MemberClosureBuilder);
        var validateInitializer = GetPrivateStatic(
            closureBuilder,
            "TryValidateConstructorInitializer",
            typeof(IMethodSymbol),
            typeof(string).MakeByRefType());
        var constructorType = compilation.GetTypeByMetadataName("ConstructorShapes")!;
        var thisConstructor = constructorType.GetMembers()
            .OfType<IMethodSymbol>()
            .Single(method => method.MethodKind == MethodKind.Constructor && method.Parameters.Length == 0);
        var baseConstructor = constructorType.GetMembers()
            .OfType<IMethodSymbol>()
            .Single(method => method.MethodKind == MethodKind.Constructor && method.Parameters.Length == 1 && method.Parameters[0].Type.SpecialType == SpecialType.System_Int32);
        var bodyConstructor = constructorType.GetMembers()
            .OfType<IMethodSymbol>()
            .Single(method => method.MethodKind == MethodKind.Constructor && method.Parameters.Length == 1 && method.Parameters[0].Type.SpecialType == SpecialType.System_String);

        var thisArguments = new object?[] { thisConstructor, null };
        Assert.IsFalse((bool)validateInitializer.Invoke(null, thisArguments)!);
        StringAssert.Contains((string)thisArguments[1]!, "this(...)", StringComparison.Ordinal);
        var baseArguments = new object?[] { baseConstructor, null };
        Assert.IsFalse((bool)validateInitializer.Invoke(null, baseArguments)!);
        StringAssert.Contains((string)baseArguments[1]!, "base(...) arguments", StringComparison.Ordinal);
        var bodyArguments = new object?[] { bodyConstructor, null };
        Assert.IsTrue((bool)validateInitializer.Invoke(null, bodyArguments)!);
        Assert.IsNull(bodyArguments[1]);

        var lifecycleBase = compilation.GetTypeByMetadataName("LifecycleBase")!;
        var lifecycleDerived = compilation.GetTypeByMetadataName("LifecycleDerived")!;
        var componentBase = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase")!;
        var disposable = compilation.GetTypeByMetadataName("System.IDisposable")!;
        var asyncDisposable = compilation.GetTypeByMetadataName("System.IAsyncDisposable")!;
        var derivedParametersSet = lifecycleDerived.GetMembers("OnParametersSet").OfType<IMethodSymbol>().Single();
        var regular = lifecycleDerived.GetMembers("Regular").OfType<IMethodSymbol>().Single();

        var supportedLifecycle = GetPrivateStatic(
            closureBuilder,
            "IsSupportedLifecycleMethod",
            typeof(INamedTypeSymbol),
            typeof(IMethodSymbol),
            typeof(INamedTypeSymbol));
        Assert.IsTrue((bool)supportedLifecycle.Invoke(null, [lifecycleDerived, derivedParametersSet, lifecycleBase])!);
        Assert.IsFalse((bool)supportedLifecycle.Invoke(null, [lifecycleDerived, regular, lifecycleBase])!);

        var overridesBase = GetPrivateStatic(
            closureBuilder,
            "OverridesComponentBase",
            typeof(IMethodSymbol),
            typeof(INamedTypeSymbol));
        Assert.IsTrue((bool)overridesBase.Invoke(null, [derivedParametersSet, lifecycleBase])!);
        Assert.IsFalse((bool)overridesBase.Invoke(null, [derivedParametersSet, null])!);

        var lifecycleRoots = GetPrivateStatic(
            closureBuilder,
            "GetSupportedLifecycleRoots",
            typeof(Compilation),
            typeof(INamedTypeSymbol));
        Assert.IsNotNull(lifecycleRoots.Invoke(null, [compilation, lifecycleDerived]));
        Assert.IsNotNull(lifecycleRoots.Invoke(null, [compilation, lifecycleBase]));

        var dispose = lifecycleDerived.GetMembers("Dispose").OfType<IMethodSymbol>().Single();
        var baseDispose = lifecycleBase.GetMembers("Dispose").OfType<IMethodSymbol>().Single();
        var explicitDispose = compilation.GetTypeByMetadataName("ExplicitDisposable")!
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Single(method => method.MethodKind == MethodKind.ExplicitInterfaceImplementation);
        var disposeEntryPoint = GetPrivateStatic(
            closureBuilder,
            "IsDisposeEntryPoint",
            typeof(IMethodSymbol),
            typeof(INamedTypeSymbol),
            typeof(INamedTypeSymbol));
        Assert.IsTrue((bool)disposeEntryPoint.Invoke(null, [dispose, disposable, asyncDisposable])!);
        Assert.IsTrue((bool)disposeEntryPoint.Invoke(null, [explicitDispose, disposable, asyncDisposable])!);
        var asyncMethod = compilation.GetTypeByMetadataName("AsyncDisposable")!
            .GetMembers("DisposeAsync").OfType<IMethodSymbol>().Single();
        Assert.IsTrue((bool)disposeEntryPoint.Invoke(null, [asyncMethod, disposable, asyncDisposable])!);

        var disposeRoot = GetPrivateStatic(
            closureBuilder,
            "IsDisposeRoot",
            typeof(INamedTypeSymbol),
            typeof(IMethodSymbol),
            typeof(INamedTypeSymbol),
            typeof(INamedTypeSymbol));
        Assert.IsTrue((bool)disposeRoot.Invoke(null, [lifecycleDerived, dispose, disposable, asyncDisposable])!);
        Assert.IsFalse((bool)disposeRoot.Invoke(null, [lifecycleDerived, baseDispose, disposable, asyncDisposable])!);

        var findImplementation = GetPrivateStatic(
            closureBuilder,
            "FindEffectiveInterfaceImplementation",
            typeof(INamedTypeSymbol),
            typeof(IMethodSymbol));
        var interfaceDispose = disposable.GetMembers("Dispose").OfType<IMethodSymbol>().Single();
        Assert.IsNotNull(findImplementation.Invoke(null, [lifecycleDerived, interfaceDispose]));

        var effectiveImplementation = GetPrivateStatic(
            closureBuilder,
            "IsEffectiveInterfaceImplementation",
            typeof(IMethodSymbol),
            typeof(IMethodSymbol),
            typeof(IMethodSymbol));
        Assert.IsTrue((bool)effectiveImplementation.Invoke(null, [dispose, interfaceDispose, baseDispose])!);
        Assert.IsFalse((bool)effectiveImplementation.Invoke(null, [regular, interfaceDispose, baseDispose])!);

        _ = componentBase;
    }

    [TestMethod]
    public void CurrentComponentBoundaryHelpers_ClassifyDirectBindOperationsAndReceiverShapes()
    {
        var tree = CSharpSyntaxTree.ParseText(
            """
            using System;
            using Microsoft.AspNetCore.Components;
            public sealed class ReceiverComponent : ComponentBase
            {
                public string Value { get; set; } = string.Empty;
                public void Render()
                {
                    Action<string> callback = value => Value = value;
                    object converted = (object)(Action<string>)(value => Value = value);
                    var local = Value;
                    _ = callback;
                    _ = converted;
                    _ = local;
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "ReceiverComponent.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.Coverage.CurrentComponent",
            [tree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var model = compilation.GetSemanticModel(tree);
        var root = tree.GetRoot();
        var lambdas = root.DescendantNodes().OfType<LambdaExpressionSyntax>().ToArray();
        Assert.HasCount(2, lambdas);
        var anonymous = (IAnonymousFunctionOperation)model.GetOperation(lambdas[0])!;
        var cast = root.DescendantNodes()
            .OfType<CastExpressionSyntax>()
            .Single(candidate => candidate.Type.ToString() == "object");
        var converted = (IConversionOperation)model.GetOperation(cast)!;

        var valueKind = GetPrivateStatic(
            typeof(CurrentComponentSemanticWalkerHost),
            "TryGetDirectBinderValueKind",
            typeof(IOperation),
            typeof(DirectBinderValueKind).MakeByRefType());
        var convertedArguments = new object?[] { converted, null };
        Assert.IsTrue((bool)valueKind.Invoke(null, convertedArguments)!);
        Assert.AreEqual("String", convertedArguments[1]!.ToString());
        var anonymousArguments = new object?[] { anonymous, null };
        Assert.IsTrue((bool)valueKind.Invoke(null, anonymousArguments)!);
        Assert.AreEqual("String", anonymousArguments[1]!.ToString());

        var localValue = root.DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Single(declarator => declarator.Identifier.ValueText == "local")
            .Initializer!;
        var localOperation = model.GetOperation(localValue.Value)!;
        var rejectedArguments = new object?[] { localOperation, null };
        Assert.IsFalse((bool)valueKind.Invoke(null, rejectedArguments)!);
        Assert.AreEqual("None", rejectedArguments[1]!.ToString());

        var receiver = GetPrivateStatic(
            typeof(CurrentComponentMemberClosure).GetNestedType("Builder", BindingFlags.NonPublic)!,
            "IsCurrentComponentReceiver",
            typeof(IOperation));
        Assert.IsTrue((bool)receiver.Invoke(null, [null])!);
        Assert.IsFalse((bool)receiver.Invoke(null, [localOperation])!);
        Assert.IsFalse((bool)receiver.Invoke(null, [converted])!);

        var component = compilation.GetTypeByMetadataName("ReceiverComponent")!;
        var render = component.GetMembers("Render").OfType<IMethodSymbol>().Single();
        var closure = CurrentComponentMemberClosure.Create(
            component,
            compilation,
            [render, render],
            []);
        Assert.IsTrue(closure.Contains(render));
    }

    [TestMethod]
    public void ComponentSelectorBoundaries_RecognizeEquivalentMetadataAndMappedRazorPaths()
    {
        const string attributeSource =
            "namespace ECMAScript { [System.AttributeUsage(System.AttributeTargets.Class)] public sealed class ECMAScriptModuleAttribute : System.Attribute { public ECMAScriptModuleAttribute(string path) { } } }";
        var first = CreateSimpleCompilation(attributeSource + " [ECMAScript.ECMAScriptModule(\"one\")] public sealed class One { }");
        var second = CreateSimpleCompilation(attributeSource + " [ECMAScript.ECMAScriptModule(\"two\")] public sealed class Two { }");
        var one = first.GetTypeByMetadataName("One")!;
        var two = second.GetTypeByMetadataName("Two")!;
        var firstAttribute = first.GetTypeByMetadataName("ECMAScript.ECMAScriptModuleAttribute")!;
        var secondAttribute = second.GetTypeByMetadataName("ECMAScript.ECMAScriptModuleAttribute")!;
        var unrelated = CreateSimpleCompilation(
            attributeSource + " [System.Obsolete] public sealed class Unrelated { }")
            .GetTypeByMetadataName("Unrelated")!;
        var hasModule = GetPrivateStatic(
            typeof(ComponentSelector),
            "HasECMAScriptModuleAttribute",
            typeof(INamedTypeSymbol),
            typeof(INamedTypeSymbol));
        Assert.IsFalse((bool)hasModule.Invoke(null, [unrelated, firstAttribute])!);
        Assert.IsTrue((bool)hasModule.Invoke(null, [one, secondAttribute])!);
        Assert.IsTrue((bool)hasModule.Invoke(null, [two, firstAttribute])!);

        var plainTree = CSharpSyntaxTree.ParseText(
            "public sealed class Plain { public void Render() { } }",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Plain.cs");
        var razorTree = CSharpSyntaxTree.ParseText(
            "#line 1 \"Pages/Mapped.razor\"\npublic sealed class Mapped { public void Render() { } }\n#line default",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "obj/Mapped.razor.g.cs");
        var pathCompilation = CSharpCompilation.Create(
            "RazorVue.Coverage.Selector",
            [plainTree, razorTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var plain = pathCompilation.GetTypeByMetadataName("Plain")!;
        var mapped = pathCompilation.GetTypeByMetadataName("Mapped")!;
        Assert.IsFalse((bool)hasModule.Invoke(null, [plain, firstAttribute])!);
        var hasNamedIdentity = GetPrivateStatic(typeof(ComponentSelector), "HasRazorSourceIdentity", typeof(INamedTypeSymbol));
        var hasMethodIdentity = GetPrivateStatic(typeof(ComponentSelector), "HasRazorSourceIdentity", typeof(IMethodSymbol));
        Assert.IsFalse((bool)hasNamedIdentity.Invoke(null, [plain])!);
        Assert.IsTrue((bool)hasNamedIdentity.Invoke(null, [mapped])!);
        Assert.IsFalse((bool)hasMethodIdentity.Invoke(null, [plain.GetMembers("Render").OfType<IMethodSymbol>().Single()])!);
        Assert.IsTrue((bool)hasMethodIdentity.Invoke(null, [mapped.GetMembers("Render").OfType<IMethodSymbol>().Single()])!);
        var metadataString = pathCompilation.GetSpecialType(SpecialType.System_String);
        var metadataToString = metadataString.GetMembers("ToString")
            .OfType<IMethodSymbol>()
            .Single(method => method.Parameters.Length == 0);
        Assert.IsFalse((bool)hasNamedIdentity.Invoke(null, [metadataString])!);
        Assert.IsFalse((bool)hasMethodIdentity.Invoke(null, [metadataToString])!);
    }

    [TestMethod]
    public void ComponentInitializationLowererBoundaries_CoverBlockAndExpressionConstructors()
    {
        var compilation = CreateSimpleCompilation(
            """
            public sealed class ConstructorBoundaryComponent
            {
                private int _value;
                public ConstructorBoundaryComponent() { _value = 1; }
                public ConstructorBoundaryComponent(int value) => _value = value;
            }
            """);
        var component = compilation.GetTypeByMetadataName("ConstructorBoundaryComponent")!;
        var walker = new SemanticWalker(true);
        var getBody = GetPrivateStatic(
            typeof(ComponentInitializationLowerer),
            "GetConstructorFunctionBody",
            typeof(Compilation),
            typeof(IMethodSymbol),
            typeof(SemanticWalker),
            typeof(SenseArgument),
            typeof(CancellationToken));
        foreach (var constructor in component.InstanceConstructors.Where(static constructor => !constructor.IsImplicitlyDeclared))
        {
            var body = getBody.Invoke(null, [compilation, constructor, walker, new SenseArgument(), CancellationToken.None]);
            Assert.IsNotNull(body);
        }
    }

    [TestMethod]
    public void VueModuleBuilderSourceRootBoundaries_HandleRootedAndMissingPaths()
    {
        var compilation = CreateSimpleCompilation("public sealed class SourceRootHost { }");
        var getRoot = GetPrivateStatic(
            typeof(VueModuleBuilder),
            "TryGetCompilationSourceRoot",
            typeof(Compilation),
            typeof(GeneratedDocument));
        var empty = new GeneratedDocument(
            "empty.g.cs",
            string.Empty,
            SourceText.From(string.Empty),
            ImmutableArray<RazorSourceMap>.Empty);
        Assert.IsNull(getRoot.Invoke(null, [compilation, empty]));

        var rooted = empty with { SourcePath = Path.Combine(Environment.CurrentDirectory, "pages", "Home.razor") };
        var root = (string?)getRoot.Invoke(null, [compilation, rooted]);
        Assert.IsNotNull(root);

        var invalid = empty with { SourcePath = "::invalid::" };
        Assert.IsNull(getRoot.Invoke(null, [compilation, invalid]));

        var malformedRooted = empty with { SourcePath = "C:\\" + "\0bad" };
        Assert.IsNull(getRoot.Invoke(null, [compilation, malformedRooted]));

        var otherTree = CSharpSyntaxTree.ParseText(
            "public sealed class OtherSource { }",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "D:\\other\\OtherSource.cs");
        var splitCompilation = CSharpCompilation.Create(
            "RazorVue.Coverage.SourceRoot",
            [otherTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.IsNotNull(getRoot.Invoke(null, [splitCompilation, rooted]));
    }

    [TestMethod]
    public void DiagnosticFactoryBoundaries_DeduplicateMappedAndDistinctLocations()
    {
        var tree = CSharpSyntaxTree.ParseText("class DiagnosticSource { int Value; }");
        var first = Location.Create(
            tree,
            new TextSpan(4, 3));
        var same = Location.Create(
            tree,
            new TextSpan(4, 3));
        var different = Location.Create(
            tree,
            new TextSpan(5, 3));
        var diagnostic = RazorVueDiagnosticFactory.Create(
            RazorVueDiagnosticCategory.Internal,
            "duplicate locations",
            additionalLocations: ImmutableArray.Create<Location>(Location.None, first, same, different));
        Assert.HasCount(2, diagnostic.AdditionalLocations);
    }

    [TestMethod]
    public void RenderEmitterPureBoundaries_ClassifyTextLoopsBuilderOffsetsAndEventModifiers()
    {
        var tree = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components.Rendering;
            public sealed class RenderShapes
            {
                private int _value;
                private void Invoke() { }
                private static void Forward(RenderTreeBuilder builder, string value) { }
                private static void StringForward(string value) { }
                private static void DecimalValue(decimal value) { }
                public void Render(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "text");
                    builder.AddContent(1, (object?)null);
                    builder.AddContent(2, true);
                    builder.AddContent(3, 'c');
                    builder.AddContent(4, (sbyte)1);
                    builder.AddContent(5, (byte)2);
                    builder.AddContent(6, (short)3);
                    builder.AddContent(7, (ushort)4);
                    builder.AddContent(8, 5);
                    builder.AddContent(9, (uint)6);
                    builder.AddContent(10, (long)7);
                    builder.AddContent(11, (ulong)8);
                    builder.AddContent(12, (float)1.5);
                    builder.AddContent(13, 2.5d);
                    builder.AddContent(14, 3.5m);
                    Forward(builder, "forwarded");
                    StringForward("not a builder");
                    DecimalValue(3.5m);
                    _value++;
                    Invoke();
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/RenderShapes.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.Coverage.RenderPure",
            [tree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var model = compilation.GetSemanticModel(tree);
        var method = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Single(candidate => candidate.Identifier.ValueText == "Render");
        var body = Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(method.Body!));
        var emitter = typeof(RenderEmitter);
        var emitterImplementation = emitter.GetNestedType("Emitter", BindingFlags.NonPublic)!;

        var staticText = GetPrivateStatic(emitter, "IsStaticTextContent", typeof(IOperation));
        var guaranteedText = GetPrivateStatic(emitter, "IsGuaranteedStringTextContent", typeof(IOperation));
        var addContents = body.Descendants().OfType<IInvocationOperation>()
            .Where(invocation => invocation.TargetMethod.Name == "AddContent")
            .ToArray();
        var addContent = addContents[0];
        var textValue = addContent.Arguments[1].Value;
        Assert.IsTrue((bool)staticText.Invoke(null, [textValue])!);
        Assert.IsTrue((bool)guaranteedText.Invoke(null, [textValue])!);
        foreach (var invocation in addContents.Skip(1))
            Assert.IsTrue((bool)staticText.Invoke(null, [invocation.Arguments[^1].Value])!);
        var decimalValue = body.Descendants().OfType<IInvocationOperation>()
            .Single(invocation => invocation.TargetMethod.Name == "DecimalValue");
        Assert.IsTrue((bool)staticText.Invoke(null, [decimalValue.Arguments[0].Value])!);
        var increment = body.Descendants().OfType<IIncrementOrDecrementOperation>().Single();
        Assert.IsFalse((bool)staticText.Invoke(null, [increment])!);
        Assert.IsFalse((bool)guaranteedText.Invoke(null, [increment])!);

        var sideEffect = GetPrivateStatic(emitterImplementation, "IsLoopSideEffectOperation", typeof(IOperation));
        var statements = body.DescendantsAndSelf().OfType<IExpressionStatementOperation>().ToArray();
        Assert.IsFalse((bool)sideEffect.Invoke(null, [statements.First(statement => statement.Operation is IInvocationOperation)])!);
        Assert.IsTrue((bool)sideEffect.Invoke(null, [statements.Single(statement => statement.Operation is IIncrementOrDecrementOperation)])!);
        Assert.IsFalse((bool)sideEffect.Invoke(null, [body.Operations.First()])!);

        var constantString = GetPrivateStatic(
            emitter,
            "TryGetConstantString",
            typeof(IOperation),
            typeof(string).MakeByRefType());
        var output = new object?[] { textValue, null };
        Assert.IsTrue((bool)constantString.Invoke(null, output)!);
        Assert.AreEqual("text", output[1]);
        var outputFalse = new object?[] { increment, null };
        Assert.IsFalse((bool)constantString.Invoke(null, outputFalse)!);

        var eventName = GetPrivateStatic(emitter, "IsDirectEventAttributeName", typeof(string));
        Assert.IsTrue((bool)eventName.Invoke(null, ["onClick"])!);
        Assert.IsFalse((bool)eventName.Invoke(null, ["onclick"])!);
        Assert.IsFalse((bool)eventName.Invoke(null, ["on"])!);

        var elementFrame = emitter.GetNestedType("ElementFrame", BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(elementFrame);
        var merge = elementFrame!.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .SingleOrDefault(method => method.Name == "MergeDirectEventModifierCondition");
        Assert.IsNotNull(merge);
        var identifier = new Identifier("condition");
        var trueLiteral = new BooleanLiteral(true, "true");
        Assert.AreSame(identifier, merge!.Invoke(null, [null, identifier]));
        Assert.IsInstanceOfType<BooleanLiteral>(merge.Invoke(null, [trueLiteral, identifier]));
        Assert.IsInstanceOfType<BooleanLiteral>(merge.Invoke(null, [identifier, trueLiteral]));
        Assert.IsInstanceOfType<LogicalExpression>(merge.Invoke(null, [identifier, new Identifier("other")]));
        var falseLiteral = new BooleanLiteral(false, "false");
        Assert.IsInstanceOfType<LogicalExpression>(merge.Invoke(null, [falseLiteral, identifier]));
        Assert.IsInstanceOfType<LogicalExpression>(merge.Invoke(null, [identifier, falseLiteral]));

        var offset = GetPrivateStatic(emitterImplementation, "GetRenderTreeBuilderReceiverArgumentOffset", typeof(IInvocationOperation));
        Assert.AreEqual(0, offset.Invoke(null, [addContent]));
        var forward = body.Descendants().OfType<IInvocationOperation>()
            .Single(invocation => invocation.TargetMethod.Name == "Forward");
        Assert.AreEqual(1, offset.Invoke(null, [forward]));
        var invoke = body.Descendants().OfType<IInvocationOperation>()
            .Single(invocation => invocation.TargetMethod.Name == "Invoke");
        Assert.AreEqual(0, offset.Invoke(null, [invoke]));

        var addModifier = GetPrivateStatic(
            emitter,
            "AddDirectEventModifierStatement",
            typeof(List<Statement>),
            typeof(Expression),
            typeof(Expression),
            typeof(string));
        var eventExpression = new Identifier("event");
        var modifierStatements = new List<Statement>();
        addModifier.Invoke(null, [modifierStatements, eventExpression, null, "preventDefault"]);
        addModifier.Invoke(null, [modifierStatements, eventExpression, new BooleanLiteral(false, "false"), "preventDefault"]);
        addModifier.Invoke(null, [modifierStatements, eventExpression, new BooleanLiteral(true, "true"), "preventDefault"]);
        addModifier.Invoke(null, [modifierStatements, eventExpression, new Identifier("shouldPrevent"), "preventDefault"]);
        Assert.HasCount(2, modifierStatements);
    }

    [TestMethod]
    public void MemberClosureBoundaries_ClassifyLifecycleOverridesDisposalAndConstructorChains()
    {
        var tree = CSharpSyntaxTree.ParseText(
            """
            using System;
            using Microsoft.AspNetCore.Components;
            public class BaseComponent : ComponentBase, IDisposable
            {
                public virtual void Dispose() { }
                protected override void OnInitialized() { }
            }
            public sealed class DerivedComponent : BaseComponent
            {
                public override void Dispose() { }
                public static void StaticDispose() { }
                public DerivedComponent() { }
            }
            public sealed class ExplicitComponent : ComponentBase, IDisposable
            {
                void IDisposable.Dispose() { }
            }
            public sealed class ChainedComponent : ComponentBase
            {
                public ChainedComponent() : this(1) { }
                public ChainedComponent(int value) { }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/MemberClosureBoundaries.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.Coverage.MemberClosure",
            [tree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var baseComponent = compilation.GetTypeByMetadataName("BaseComponent")!;
        var derivedComponent = compilation.GetTypeByMetadataName("DerivedComponent")!;
        var explicitComponent = compilation.GetTypeByMetadataName("ExplicitComponent")!;
        var chainedComponent = compilation.GetTypeByMetadataName("ChainedComponent")!;
        var componentBase = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase")!;
        var disposable = compilation.GetTypeByMetadataName("System.IDisposable")!;
        var disposeInterfaceMethod = disposable.GetMembers("Dispose").OfType<IMethodSymbol>().Single();
        var baseDispose = baseComponent.GetMembers("Dispose").OfType<IMethodSymbol>().Single();
        var derivedDispose = derivedComponent.GetMembers("Dispose").OfType<IMethodSymbol>().Single();
        var staticDispose = derivedComponent.GetMembers("StaticDispose").OfType<IMethodSymbol>().Single();
        var explicitDispose = explicitComponent.GetMembers().OfType<IMethodSymbol>()
            .Single(method => method.MethodKind == MethodKind.ExplicitInterfaceImplementation);
        var onInitialized = baseComponent.GetMembers("OnInitialized").OfType<IMethodSymbol>().Single();

        var validateConstructor = GetPrivateStatic(
            typeof(MemberClosureBuilder),
            "TryValidateConstructorInitializer",
            typeof(IMethodSymbol),
            typeof(string).MakeByRefType());
        var chainedConstructor = chainedComponent.InstanceConstructors
            .Single(constructor => constructor.Parameters.Length == 0);
        var constructorArguments = new object?[] { chainedConstructor, null };
        Assert.IsFalse((bool)validateConstructor.Invoke(null, constructorArguments)!);
        StringAssert.Contains((string)constructorArguments[1]!, "this(...)", StringComparison.Ordinal);

        var effective = GetPrivateStatic(
            typeof(MemberClosureBuilder),
            "IsEffectiveInterfaceImplementation",
            typeof(IMethodSymbol),
            typeof(IMethodSymbol),
            typeof(IMethodSymbol));
        Assert.IsTrue((bool)effective.Invoke(null, [derivedDispose, baseDispose, baseDispose])!);
        Assert.IsFalse((bool)effective.Invoke(null, [staticDispose, baseDispose, baseDispose])!);
        Assert.IsFalse((bool)effective.Invoke(null, [explicitDispose, baseDispose, baseDispose])!);

        var overrides = GetPrivateStatic(
            typeof(MemberClosureBuilder),
            "OverridesComponentBase",
            typeof(IMethodSymbol),
            typeof(INamedTypeSymbol));
        Assert.IsTrue((bool)overrides.Invoke(null, [onInitialized, componentBase])!);
        Assert.IsFalse((bool)overrides.Invoke(null, [onInitialized, null])!);

        var lifecycleRoots = GetPrivateStatic(
            typeof(MemberClosureBuilder),
            "GetSupportedLifecycleRoots",
            typeof(Compilation),
            typeof(INamedTypeSymbol));
        Assert.IsNotNull(lifecycleRoots.Invoke(null, [compilation, derivedComponent]));

        var disposeEntry = GetPrivateStatic(
            typeof(MemberClosureBuilder),
            "IsDisposeEntryPoint",
            typeof(IMethodSymbol),
            typeof(INamedTypeSymbol),
            typeof(INamedTypeSymbol));
        Assert.IsTrue((bool)disposeEntry.Invoke(null, [baseDispose, disposable, null])!);
        Assert.IsFalse((bool)disposeEntry.Invoke(null, [staticDispose, disposable, null])!);
        Assert.IsTrue((bool)disposeEntry.Invoke(null, [explicitDispose, disposable, null])!);

        var isAutoProperty = GetPrivateStatic(typeof(MemberClosureBuilder), "IsAutoProperty", typeof(IPropertySymbol));
        var autoPropertyTree = CSharpSyntaxTree.ParseText("public sealed class Auto { public int Value { get; set; } public int Expr => 1; }");
        var autoCompilation = CSharpCompilation.Create(
            "RazorVue.Coverage.AutoProperty",
            [autoPropertyTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var autoType = autoCompilation.GetTypeByMetadataName("Auto")!;
        Assert.IsTrue((bool)isAutoProperty.Invoke(null, [autoType.GetMembers("Value").OfType<IPropertySymbol>().Single()])!);
        Assert.IsFalse((bool)isAutoProperty.Invoke(null, [autoType.GetMembers("Expr").OfType<IPropertySymbol>().Single()])!);
    }

    [TestMethod]
    public void MemberClosureLifecycleBoundaries_ValidateBaseArgumentsAsyncDisposalAndMetadataMembers()
    {
        var compilation = CreateSimpleCompilation(
            """
            using System;
            using System.Threading.Tasks;
            using Microsoft.AspNetCore.Components;

            public class SourceBase : ComponentBase
            {
                public SourceBase(int seed) { }
            }

            public sealed class BaseArgumentComponent : SourceBase
            {
                public BaseArgumentComponent() : base(1) { }
            }

            public sealed class AsyncComponent : ComponentBase, IAsyncDisposable
            {
                protected override void OnInitialized() { }
                public ValueTask DisposeAsync() => ValueTask.CompletedTask;
                public void NotDispose() { }
            }

            public sealed class PlainComponent : ComponentBase
            {
            }
            """);
        var baseArgument = compilation.GetTypeByMetadataName("BaseArgumentComponent")!;
        var asyncComponent = compilation.GetTypeByMetadataName("AsyncComponent")!;
        var plainComponent = compilation.GetTypeByMetadataName("PlainComponent")!;
        var sourceBase = compilation.GetTypeByMetadataName("SourceBase")!;
        var componentBase = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase")!;
        var disposable = compilation.GetTypeByMetadataName("System.IDisposable")!;
        var asyncDisposable = compilation.GetTypeByMetadataName("System.IAsyncDisposable")!;
        var dispose = disposable.GetMembers("Dispose").OfType<IMethodSymbol>().Single();
        var disposeAsync = asyncDisposable.GetMembers("DisposeAsync").OfType<IMethodSymbol>().Single();
        var asyncDispose = asyncComponent.GetMembers("DisposeAsync").OfType<IMethodSymbol>().Single();
        var notDispose = asyncComponent.GetMembers("NotDispose").OfType<IMethodSymbol>().Single();
        var onInitialized = asyncComponent.GetMembers("OnInitialized").OfType<IMethodSymbol>().Single();

        var validateConstructor = GetPrivateStatic(
            typeof(MemberClosureBuilder),
            "TryValidateConstructorInitializer",
            typeof(IMethodSymbol),
            typeof(string).MakeByRefType());
        var constructorArguments = new object?[] { baseArgument.InstanceConstructors.Single(static candidate => !candidate.IsImplicitlyDeclared), null };
        Assert.IsFalse((bool)validateConstructor.Invoke(null, constructorArguments)!);
        StringAssert.Contains((string)constructorArguments[1]!, "base(...) arguments", StringComparison.Ordinal);

        var findInterfaceImplementation = GetPrivateStatic(
            typeof(MemberClosureBuilder),
            "FindEffectiveInterfaceImplementation",
            typeof(INamedTypeSymbol),
            typeof(IMethodSymbol));
        Assert.IsNull(findInterfaceImplementation.Invoke(null, [plainComponent, dispose]));
        Assert.AreEqual(asyncDispose, findInterfaceImplementation.Invoke(null, [asyncComponent, disposeAsync]));

        var lifecycleRoots = GetPrivateStatic(
            typeof(MemberClosureBuilder),
            "GetSupportedLifecycleRoots",
            typeof(Compilation),
            typeof(INamedTypeSymbol));
        var roots = (System.Collections.IEnumerable)lifecycleRoots.Invoke(null, [compilation, asyncComponent])!;
        Assert.IsTrue(roots.Cast<IMethodSymbol>().Any(method => SymbolEqualityComparer.Default.Equals(method, asyncDispose)));

        var disposeEntry = GetPrivateStatic(
            typeof(MemberClosureBuilder),
            "IsDisposeEntryPoint",
            typeof(IMethodSymbol),
            typeof(INamedTypeSymbol),
            typeof(INamedTypeSymbol));
        Assert.IsTrue((bool)disposeEntry.Invoke(null, [asyncDispose, disposable, asyncDisposable])!);
        Assert.IsFalse((bool)disposeEntry.Invoke(null, [notDispose, disposable, asyncDisposable])!);

        var supportedLifecycle = GetPrivateStatic(
            typeof(MemberClosureBuilder),
            "IsSupportedLifecycleMethod",
            typeof(INamedTypeSymbol),
            typeof(IMethodSymbol),
            typeof(INamedTypeSymbol));
        Assert.IsTrue((bool)supportedLifecycle.Invoke(null, [asyncComponent, onInitialized, componentBase])!);
        Assert.IsFalse((bool)supportedLifecycle.Invoke(null, [plainComponent, onInitialized, componentBase])!);

        var declaredOnHierarchy = GetPrivateStatic(
            typeof(MemberClosureBuilder),
            "IsDeclaredOnSourceComponentHierarchy",
            typeof(INamedTypeSymbol),
            typeof(INamedTypeSymbol));
        Assert.IsTrue((bool)declaredOnHierarchy.Invoke(null, [baseArgument, sourceBase])!);
        Assert.IsFalse((bool)declaredOnHierarchy.Invoke(null, [baseArgument, componentBase])!);

        var isAutoProperty = GetPrivateStatic(typeof(MemberClosureBuilder), "IsAutoProperty", typeof(IPropertySymbol));
        Assert.IsFalse((bool)isAutoProperty.Invoke(null, [compilation.GetSpecialType(SpecialType.System_String)
            .GetMembers("Length").OfType<IPropertySymbol>().Single()])!);
    }

    private static CSharpCompilation CreateSimpleCompilation(string source)
        => CSharpCompilation.Create(
            "RazorVue.Coverage.Metadata." + Guid.NewGuid().ToString("N"),
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview))],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static MethodInfo GetPrivateStatic(Type owner, string name, params Type[] parameterTypes)
        => owner.GetMethod(
            name,
            BindingFlags.Static | BindingFlags.NonPublic,
            binder: null,
            types: parameterTypes,
            modifiers: null)!;
}
