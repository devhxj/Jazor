using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Reflection;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class CompilerReferenceCoverageCompletionTests
{
    [TestMethod]
    public void Visit_RuntimeParamsAndIntegerFormats_PreserveCallShapeAndHexCase()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            static class RuntimeSink
            {
                public static void Expand(params int[] values) { }

                public static void Keep([Preserve] params int[] values) { }
            }

            public static class TestClass
            {
                static string TestMethod(int signed, uint unsigned, int[] values)
                {
                    RuntimeSink.Expand(new[] { 1, 2 });
                    RuntimeSink.Expand([3, 4]);
                    RuntimeSink.Expand(values);
                    RuntimeSink.Keep(values);
                    return signed.ToString("X") + signed.ToString("x") + unsigned.ToString("x");
                }
            }
            """);

        StringAssert.Contains(script, "RuntimeSink.Expand(1, 2)", StringComparison.Ordinal);
        StringAssert.Contains(script, "RuntimeSink.Expand(3, 4)", StringComparison.Ordinal);
        StringAssert.Contains(script, "RuntimeSink.Expand(...values)", StringComparison.Ordinal);
        StringAssert.Contains(script, "RuntimeSink.Keep(values)", StringComparison.Ordinal);
        StringAssert.Contains(script, "toUpperCase()", StringComparison.Ordinal);
        StringAssert.Contains(script, "toLowerCase()", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(signed, unsigned, values) " + script);
    }

    [TestMethod]
    public async Task Visit_ModuleInlineMember_BindsTheImportedInlineIdentifier()
    {
        var source =
            """
            using ECMAScript;

            [ECMAScriptModule("runtime/inline.mjs")]
            public static class InlineModule
            {
                [ECMAScriptInline("__arg1 + 1")]
                public static extern int Increment(int value);
            }

            public static class TestClass
            {
                static int TestMethod(int value)
                {
                    return InlineModule.Increment(value);
                }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "CompilerReferenceCoverageInlineModule_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11.Add(
                MetadataReference.CreateFromFile(typeof(ECMAScript.Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(diagnostics, string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));

        var model = compilation.GetSemanticModel(syntaxTree);
        var consumer = compilation.GetTypeByMetadataName("TestClass")!;
        var inlineModule = compilation.GetTypeByMetadataName("InlineModule")!;
        Assert.AreEqual("runtime/inline.mjs", Util.GetECMAScriptModuleImportPath(inlineModule));
        var script = (await new AstConverter(consumer, model).Convert())?.ToKnRECMAScript();
        Assert.IsNotNull(script);

        StringAssert.Contains(script, "value + 1", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script!);

        var invocationSyntax = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Single();
        var invocation = (IInvocationOperation)model.GetOperation(invocationSyntax)!;
        var intrinsic = typeof(SemanticWalker).GetMethod(
            "TryBuildIntrinsicMethodInvocation",
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types:
            [
                typeof(IInvocationOperation),
                typeof(IMethodSymbol),
                typeof(Expression),
                typeof(List<Expression>),
                typeof(SenseArgument),
                typeof(Expression).MakeByRefType()
            ],
            modifiers: null)!;
        var intrinsicArguments = new object?[]
        {
            invocation,
            invocation.TargetMethod,
            null,
            new List<Expression> { new Identifier("value") },
            new SenseArgument(),
            null
        };
        Assert.IsTrue((bool)intrinsic.Invoke(new SemanticWalker(true), intrinsicArguments)!);
    }

    [TestMethod]
    public void Visit_IntegerToStringWithDynamicFormat_RejectsUnsupportedRuntimeMember()
    {
        Assert.Throws<OperationTransformationException>(() => VisitBlock(
            """
            static class TestClass
            {
                static string TestMethod(int value, string format)
                {
                    return value.ToString(format);
                }
            }
            """));
    }

    [TestMethod]
    public void Visit_LongToStringHex_RejectsUnsupportedRuntimeMember()
    {
        Assert.Throws<OperationTransformationException>(() => VisitBlock(
            """
            static class TestClass
            {
                static string TestMethod(long value)
                {
                    return value.ToString("X");
                }
            }
            """));
    }

    [TestMethod]
    public void Visit_RuntimeParamsAndStoredRangeIndex_PreserveArrayMaterializationAndLengthProtocol()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            static class RuntimeSink
            {
                public static void Expand(params int[] values) { }
            }

            static class TestClass
            {
                static int[] TestMethod(int count, int[] values, System.Index start)
                {
                    RuntimeSink.Expand(new int[count]);
                    return values[start..];
                }
            }
            """);

        StringAssert.Contains(script, "RuntimeSink.Expand(...new Array(count))", StringComparison.Ordinal);
        StringAssert.Contains(script, ".slice(", StringComparison.Ordinal);
        StringAssert.Contains(script, ".length", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(count, values, start) " + script);
    }

    [TestMethod]
    public void Visit_TypeOfCurrentSourceClass_UsesTheStableRuntimeConstructorToken()
    {
        var script = VisitBlock(
            """
            using ECMAScript;

            [ECMAScript]
            sealed class Payload
            {
            }

            static class TestClass
            {
                static System.Type TestMethod()
                {
                    return typeof(Payload);
                }
            }
            """);

        StringAssert.Contains(script, "return Payload;", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void ErasedUnionProjectionAndAliasContracts_DistinguishSupportedRuntimeShapes()
    {
        var compilation = CreateCompilation(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Struct)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            namespace System.Runtime.CompilerServices
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Struct)]
                public sealed class UnionAttribute : global::System.Attribute
                {
                }

                public interface IUnion
                {
                    object? Value { get; }
                }
            }

            [ECMAScript.ECMAScript]
            [System.Runtime.CompilerServices.Union]
            public readonly struct RuntimeChoice : System.Runtime.CompilerServices.IUnion
            {
                public RuntimeChoice(object value) { }

                public object? Value => null;

                public string? AsText => null;
            }

            [System.Runtime.CompilerServices.Union]
            public readonly struct UnmappedChoice : System.Runtime.CompilerServices.IUnion
            {
                public UnmappedChoice(object value) { }

                public object? Value => null;
            }

            public sealed class PlainChoice
            {
                public object? Value => null;
            }
            """);
        var runtime = compilation.GetTypeByMetadataName("RuntimeChoice")!;
        var unmapped = compilation.GetTypeByMetadataName("UnmappedChoice")!;
        var plain = compilation.GetTypeByMetadataName("PlainChoice")!;
        var projection = GetPrivateStatic("IsErasedUnionProjectionProperty", typeof(IPropertySymbol));
        var unsupported = GetPrivateStatic("IsUnsupportedUnionProjectionProperty", typeof(IPropertySymbol));

        var runtimeValue = GetProperty(runtime, "Value");
        var runtimeAsText = GetProperty(runtime, "AsText");
        var unmappedValue = GetProperty(unmapped, "Value");
        var plainValue = GetProperty(plain, "Value");

        Assert.IsTrue((bool)projection.Invoke(null, [runtimeValue])!);
        Assert.IsTrue((bool)projection.Invoke(null, [runtimeAsText])!);
        Assert.IsFalse((bool)projection.Invoke(null, [plainValue])!);
        Assert.IsFalse((bool)unsupported.Invoke(null, [runtimeValue])!);
        Assert.IsTrue((bool)unsupported.Invoke(null, [unmappedValue])!);
        Assert.IsFalse((bool)unsupported.Invoke(null, [plainValue])!);

        var access = GetPrivateStatic(
            "BuildAliasedPropertyAccess",
            typeof(Expression),
            typeof(string),
            typeof(bool));
        Assert.AreEqual(
            "record['status-code']".Replace('\'', '\"'),
            ((Expression)access.Invoke(null, [new Identifier("record"), "['status-code']", false])!).ToKnRECMAScript());
        Assert.AreEqual(
            "record[7]",
            ((Expression)access.Invoke(null, [new Identifier("record"), "[7]", false])!).ToKnRECMAScript());
        Assert.AreEqual(
            "record[\"display-name\"]",
            ((Expression)access.Invoke(null, [new Identifier("record"), "display-name", false])!).ToKnRECMAScript());
        Assert.AreEqual(
            "record.plainName",
            ((Expression)access.Invoke(null, [new Identifier("record"), "plainName", false])!).ToKnRECMAScript());
    }

    [TestMethod]
    public void UtilSymbolContracts_CoverImportNamesRecordProxyAndOverloadBoundaries()
    {
        var compilation = CreateEcmaCompilation(
            """
            using System;
            using System.ComponentModel;
            using ECMAScript;

            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
            public sealed class JazorAttribute : Attribute
            {
                public JazorAttribute() { }

                public JazorAttribute(object? operation, string? member, string? value = null) { }
            }

            [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
            public sealed class OtherAttribute : Attribute
            {
            }

            [ECMAScript]
            [Description("@#")]
            public record RecordHost
            {
                [Other]
                [ECMAScriptName("named-value")]
                public int Named { get; init; }

                public int Plain { get; init; }

                public (int First, int Second) Pair;

                public extern int ExternGet { get; }

                public extern int ExternSet { set; }

                public int this[int index] => index;

                [ECMAScriptInline("__arg1")]
                public extern int Inline(int value);

                [ECMAScriptInline("   ")]
                public extern int Blank(int value);

                [ECMAScriptName("mapped-method")]
                public extern int Mapped(int value);

                public int Ordinary(int value) => value;
            }

            public record DerivedRecord : RecordHost
            {
            }

            [ECMAScriptModule("runtime/module.mjs")]
            public static class ModuleHost
            {
                [Jazor(3, "load", "loadRuntime")]
                public static void Load(int value) { }

                public static void Overload(int value) { }

                public static void Overload(string value) { }
            }
            """);
        var record = compilation.GetTypeByMetadataName("RecordHost")!;
        var derivedRecord = compilation.GetTypeByMetadataName("DerivedRecord")!;
        var module = compilation.GetTypeByMetadataName("ModuleHost")!;
        var named = record.GetMembers("Named").OfType<IPropertySymbol>().Single();
        var plain = record.GetMembers("Plain").OfType<IPropertySymbol>().Single();
        var tupleField = record.GetMembers("Pair").OfType<IFieldSymbol>().Single();
        var tupleType = (INamedTypeSymbol)tupleField.Type;
        var tupleItem = tupleType.GetMembers("First").OfType<IFieldSymbol>().Single();
        Assert.IsFalse(tupleItem.IsImplicitlyDeclared, tupleItem.ToDisplayString());
        Assert.IsNotNull(tupleItem.CorrespondingTupleField, tupleItem.ToDisplayString());
        Assert.AreEqual("First", Util.GetConfigOrSymbolName(tupleItem));
        var externGet = record.GetMembers("ExternGet").OfType<IPropertySymbol>().Single();
        var externSet = record.GetMembers("ExternSet").OfType<IPropertySymbol>().Single();
        var indexer = record.GetMembers().OfType<IPropertySymbol>().Single(static property => property.IsIndexer);
        var inline = record.GetMembers("Inline").OfType<IMethodSymbol>().Single();
        var blank = record.GetMembers("Blank").OfType<IMethodSymbol>().Single();
        var mapped = record.GetMembers("Mapped").OfType<IMethodSymbol>().Single();
        var ordinary = record.GetMembers("Ordinary").OfType<IMethodSymbol>().Single();
        var load = module.GetMembers("Load").OfType<IMethodSymbol>().Single();
        var overload = module.GetMembers("Overload").OfType<IMethodSymbol>().First();

        Assert.IsTrue(Util.TryGetJazorImportMapping(load, out var memberName, out var runtimeName));
        Assert.AreEqual("load", memberName);
        Assert.AreEqual("loadRuntime", runtimeName);
        Assert.IsFalse(Util.TryGetJazorImportMapping(ordinary, out _, out _));
        Assert.IsFalse(Util.TryGetJazorImportMapping(named, out _, out _));

        var metadata = Util.GetJavaScriptNameMetadata(named);
        Assert.IsTrue(metadata.HasECMAScriptNameAttribute);
        Assert.AreEqual("named-value", metadata.ECMAScriptName);
        Assert.IsFalse(Util.GetJavaScriptNameMetadata(plain).HasECMAScriptNameAttribute);

        Assert.AreEqual("named-value", Util.GetConfigOrSymbolName(named));
        Assert.AreEqual("Load", Util.GetConfigOrSymbolName(load));
        StringAssert.StartsWith(Util.GetConfigOrSymbolName(overload), "Overload", StringComparison.Ordinal);
        Assert.IsTrue(Util.IsECMAScriptRecordProxyMember(named));
        Assert.IsTrue(Util.IsECMAScriptRecordProxyMember(indexer));
        Assert.IsTrue(Util.IsECMAScriptRecordProxyMember(inline));
        Assert.IsTrue(Util.IsECMAScriptRecordProxyMember(mapped));
        Assert.IsTrue(Util.IsECMAScriptRecordProxyMember(externGet));
        Assert.IsTrue(Util.IsECMAScriptRecordProxyMember(externSet));
        Assert.IsFalse(Util.IsECMAScriptRecordProxyMember(ordinary));
        Assert.IsFalse(Util.IsECMAScriptRecordProxyMember(null));
        Assert.IsTrue(Util.IsECMAScriptRecordProxyMember(named, derivedRecord));

        var inlineTemplate = typeof(Util).GetMethod(
            "HasECMAScriptInlineTemplate",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        Assert.IsTrue((bool)inlineTemplate.Invoke(null, [inline])!);
        Assert.IsFalse((bool)inlineTemplate.Invoke(null, [blank])!);
        Assert.IsFalse((bool)inlineTemplate.Invoke(null, [ordinary])!);

        var runtimeMarker = typeof(Util).GetMethod(
            "IsRuntimeMarkerType",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var moduleMarker = typeof(Util).GetMethod(
            "IsECMAScriptModuleType",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        Assert.IsTrue((bool)runtimeMarker.Invoke(null, [record])!);
        Assert.IsFalse((bool)runtimeMarker.Invoke(null, [module])!);
        Assert.IsFalse((bool)runtimeMarker.Invoke(null, [null])!);
        Assert.IsTrue((bool)moduleMarker.Invoke(null, [module])!);
        Assert.IsFalse((bool)moduleMarker.Invoke(null, [record])!);
        Assert.IsFalse((bool)moduleMarker.Invoke(null, [null])!);
    }

    private static IPropertySymbol GetProperty(INamedTypeSymbol type, string name)
        => type.GetMembers(name).OfType<IPropertySymbol>().Single();

    private static MethodInfo GetPrivateStatic(string name, params Type[] parameterTypes)
        => typeof(SemanticWalker).GetMethod(
            name,
            BindingFlags.Static | BindingFlags.NonPublic,
            binder: null,
            types: parameterTypes,
            modifiers: null)!;

    private static string VisitBlock(string source)
    {
        var block = GetBlock(source);
        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        return script;
    }

    private static IBlockOperation GetBlock(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "CompilerReferenceCoverageCompletion.cs");
        var compilation = CSharpCompilation.Create(
            "CompilerReferenceCoverageCompletion_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        AssertCompilationSucceeded(compilation);

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(
            compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "CompilerReferenceCoverageCompletion_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        AssertCompilationSucceeded(compilation);
        return compilation;
    }

    private static CSharpCompilation CreateEcmaCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "CompilerReferenceCoverageEcma_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        AssertCompilationSucceeded(compilation);
        return compilation;
    }

    private static void AssertCompilationSucceeded(CSharpCompilation compilation)
    {
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));
    }
}
