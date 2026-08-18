using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using Jazor.Compiler;
using Jazor.RazorVue.Generation;
using Jazor.RazorVue.RazorSdk;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

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
