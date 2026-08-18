using Acornima;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Reflection;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class CompilerLoweringCoverageRegressionTests
{
    [TestMethod]
    public void Visit_ExtensionInvocation_UsesTheBoundStaticExtensionHost()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Host
            {
            }

            [ECMAScript.ECMAScript]
            public static class Extensions
            {
                public static int Read(this Host host) => 1;
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    Host host = new();
                    var value = host.Read();
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "Read(host)");
        Assert.IsFalse(script.Contains("host.Read()", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Visit_PropertyAssignments_CoversBoundInstanceAndStaticTargets()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Host
            {
                public static int StaticValue { get; set; }
                public int Value { get; set; }
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    Host.StaticValue = 1;
                    var host = new Host();
                    host.Value = 2;
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "StaticValue");
        StringAssert.Contains(script, "host.Value = 2");
    }

    [TestMethod]
    public void Visit_DeconstructionAssignment_WritesInstanceAndStaticFieldsThroughTheirBoundTargets()
    {
        var block = CreateBlock(
            """
            public sealed class TestClass
            {
                private int left;
                private int right;
                private static int staticLeft;
                private static int staticRight;

                void TestMethod()
                {
                    (left, right) = (1, 2);
                    (staticLeft, staticRight) = (3, 4);
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "left");
        StringAssert.Contains(script, "staticLeft");
    }

    [TestMethod]
    public void PrimaryConstructorInitializerDiscovery_CollectsOnlyInstanceFieldAndAutoPropertyInitializers()
    {
        var compilation = CreateCompilation(
            """
            public static class ModuleHost
            {
                public sealed class Primary(int seed)
                {
                    public int Field = seed;
                    public int Uninitialized;
                    public static int StaticField = 1;
                    public int Auto { get; } = seed;
                    public int Plain { get; }
                    public static int StaticAuto { get; } = 1;
                }
            }
            """);
        var syntaxTree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(syntaxTree);
        var module = compilation.GetTypeByMetadataName("ModuleHost")!;
        var primary = module.GetTypeMembers("Primary").Single();
        var converter = new AstConverter(module, model);
        var method = typeof(AstConverter).GetMethod(
            "GetPrimaryConstructorInitializers",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        var result = ((System.Collections.IEnumerable)method.Invoke(converter, [primary])!)
            .Cast<object>()
            .ToArray();

        Assert.HasCount(2, result);
    }

    [TestMethod]
    public void Visit_ObjectAndCollectionInitializers_PreserveNestedPropertyIndexerAndAddSemantics()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Cell
            {
                public int Field;
                public int Value { get; set; }
            }

            [ECMAScript.ECMAScript]
            public sealed class Holder
            {
                private readonly Cell[] cells = [new Cell()];

                public Cell Child { get; } = new Cell();

                public Cell ChildField = new Cell();

                public Cell Replaceable { get; set; } = new Cell();

                public Cell this[int index] => cells[index];
            }

            [ECMAScript.ECMAScript]
            public sealed class Collector : System.Collections.IEnumerable
            {
                public void Add(int value)
                {
                }

                public System.Collections.IEnumerator GetEnumerator()
                    => System.Array.Empty<int>().GetEnumerator();
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var holder = new Holder
                    {
                        Child = { Field = 1, Value = 2 },
                        [0] = { Field = 3, Value = 4 },
                        ChildField = { Field = 5, Value = 6 },
                        Replaceable = new Cell { Field = 7, Value = 8 }
                    };
                    var collector = new Collector { 5, 6 };
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "holder", StringComparison.Ordinal);
        StringAssert.Contains(script, ".Add(5)", StringComparison.Ordinal);
        StringAssert.Contains(script, "[0]", StringComparison.Ordinal);
        StringAssert.Contains(script, "ChildField", StringComparison.Ordinal);
        StringAssert.Contains(script, "Replaceable", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_DeconstructionAssignment_PreservesCustomNestedAndStaticWriteTargets()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Pair
            {
                public void Deconstruct(out int left, out int right)
                {
                    left = 1;
                    right = 2;
                }
            }

            [ECMAScript.ECMAScript]
            public static class SharedState
            {
                public static int Value;
            }

            public sealed class TestClass
            {
                private int first;
                private static int second;

                void TestMethod()
                {
                    var pair = new Pair();
                    (first, SharedState.Value) = pair;

                    var left = 0;
                    var right = 0;
                    ((left, right), second) = ((3, 4), 5);
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "SharedState.Value", StringComparison.Ordinal);
        StringAssert.Contains(script, "first", StringComparison.Ordinal);
        StringAssert.Contains(script, "second", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_DeconstructionAssignment_CoversDiscardAndDeclarationTargets()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Pair
            {
                public void Deconstruct(out int left, out int right)
                {
                    left = 1;
                    right = 2;
                }
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var (_, declared) = new Pair();
                    var target = 0;
                    (_, target) = new Pair();
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "Deconstruct", StringComparison.Ordinal);
        StringAssert.Contains(script, "declared", StringComparison.Ordinal);
        StringAssert.Contains(script, "target", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_CustomListPatterns_PreserveLengthIndexerAndSliceProtocol()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Sliceable
            {
                public int Length => 3;

                public int this[int index] => index + 1;

                public Sliceable Slice(int start, int length) => this;
            }

            public sealed class TestClass
            {
                void TestMethod(Sliceable value)
                {
                    var matched = value is [1, .. var middle, 3];
                    var empty = value is [];
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "value.Length", StringComparison.Ordinal);
        StringAssert.Contains(script, "value.Slice", StringComparison.Ordinal);
        StringAssert.Contains(script, "value[0]", StringComparison.Ordinal);
    }

    [TestMethod]
    public void ConvertRuntimeClass_PrimaryConstructorBaseInvocation_ReplaysInitializersAndSuperArguments()
    {
        var compilation = CreateCompilation(
            """
            public static class ModuleHost
            {
                public class Base
                {
                    public Base(int seed)
                    {
                    }
                }

                public sealed class Derived(int seed) : Base(seed)
                {
                    public int Field = seed;
                    public int Auto { get; } = seed + 1;
                }
            }
            """);
        var syntaxTree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(syntaxTree);
        var module = compilation.GetTypeByMetadataName("ModuleHost");
        var derived = compilation.GetTypeByMetadataName("ModuleHost+Derived");
        Assert.IsNotNull(module);
        Assert.IsNotNull(derived);

        var declaration = new AstConverter(module!, model).ConvertRuntimeClass(derived!);
        var script = declaration.ToKnRECMAScript();

        StringAssert.Contains(script, "extends Base", StringComparison.Ordinal);
        StringAssert.Contains(script, "super(seed)", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void GetConfigOrSymbolName_PreservesTupleProjectionAndOverloadIdentity()
    {
        var compilation = CreateCompilation(
            """
            public sealed class Shape
            {
                public int Auto { get; set; }

                public void Render(int value)
                {
                }

                public void Render(string value)
                {
                }

                public void Accept((int first, int second) value)
                {
                }
            }
            """);
        var shape = compilation.GetTypeByMetadataName("Shape");
        Assert.IsNotNull(shape);

        var autoBackingField = shape!.GetMembers()
            .OfType<IFieldSymbol>()
            .Single(static field => field.AssociatedSymbol is IPropertySymbol);
        var overloadedMethods = shape.GetMembers("Render").OfType<IMethodSymbol>().ToArray();
        var tuple = Assert.IsInstanceOfType<INamedTypeSymbol>(
            shape.GetMembers("Accept").OfType<IMethodSymbol>().Single().Parameters[0].Type);

        Assert.AreNotEqual("Auto", Util.GetConfigOrSymbolName(autoBackingField));
        Assert.AreNotEqual(
            Util.GetConfigOrSymbolName(overloadedMethods[0]),
            Util.GetConfigOrSymbolName(overloadedMethods[1]));
        Assert.AreEqual("first", Util.GetConfigOrSymbolName(tuple.TupleElements[0]));
    }

    [TestMethod]
    public void SenseArgument_ImportAliasCollisions_AllocateStableDistinctBindings()
    {
        const string modulePath = "runtime/bridge.mjs";
        const string importedName = "render";
        var key = modulePath + "\0" + importedName;
        var prefix = "i$" + Jazor.Common.Format.HashName(key).TrimStart('_');

        var reservedArgument = new SenseArgument()
            .WithImportAliases()
            .WithImportContext([], [], [importedName, prefix], null, []);
        var reservedAlias = reservedArgument.BindImportSpecifier(modulePath, importedName);

        var occupiedArgument = new SenseArgument()
            .WithImportAliases()
            .WithImportContext(
                [],
                new Dictionary<string, string> { [prefix] = "other\0binding" },
                [importedName],
                null,
                []);
        var occupiedAlias = occupiedArgument.BindImportSpecifier(modulePath, importedName);

        var availableArgument = new SenseArgument()
            .WithImportAliases()
            .WithImportContext([], [], [importedName], null, []);
        var availableAlias = availableArgument.BindImportSpecifier(modulePath, importedName);

        Assert.AreEqual(prefix + "1", reservedAlias.Name);
        Assert.AreEqual(prefix + "1", occupiedAlias.Name);
        Assert.AreEqual(prefix, availableAlias.Name);
        Assert.AreNotEqual(importedName, reservedAlias.Name);
    }

    [TestMethod]
    public void SenseArgument_DefaultValue_AllocatesImportAliasWithoutOptionalCollisionMaps()
    {
        const string key = "runtime/bridge.mjs\0render";
        var method = typeof(SenseArgument).GetMethod(
            "AllocateImportAlias",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(method);

        var alias = Assert.IsInstanceOfType<string>(method!.Invoke(default(SenseArgument), [key]));

        Assert.AreEqual("i$" + Jazor.Common.Format.HashName(key).TrimStart('_'), alias);
    }

    [TestMethod]
    public void AstConverter_ImportedStringSpecifier_UsesTheAuthoredRuntimeKeyInDiagnostics()
    {
        var module = new Parser().ParseModule("import { \"runtime-key\" as local } from \"runtime.mjs\";");
        var declaration = Assert.IsInstanceOfType<Acornima.Ast.ImportDeclaration>(module.Body[0]);
        var specifier = Assert.IsInstanceOfType<Acornima.Ast.ImportSpecifier>(declaration.Specifiers[0]);
        var method = typeof(AstConverter).GetMethod(
            "GetImportedSpecifierName",
            BindingFlags.Static | BindingFlags.NonPublic)!;

        var name = Assert.IsInstanceOfType<string>(method.Invoke(null, [specifier]));

        Assert.AreEqual("runtime-key", name);
    }

    [TestMethod]
    public void Visit_ListPatternOnSupportedList_UsesMappedLengthAndIndexerProtocols()
    {
        var block = CreateBlock(
            """
            using System.Collections.Generic;

            public sealed class TestClass
            {
                void TestMethod(List<int> values)
                {
                    var matched = values is [1, .. var middle, 3];
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "values.length", StringComparison.Ordinal);
        StringAssert.Contains(script, "(values, 0)", StringComparison.Ordinal);
        Assert.DoesNotContain("values[0]", script, StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_StructuralRecordDeconstruction_ProjectsRecordMembersWithoutRuntimeDeconstructCall()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed record Pair(int Left, int Right);

            public sealed class TestClass
            {
                void TestMethod(Pair pair)
                {
                    var (left, right) = pair;
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "pair.Left", StringComparison.Ordinal);
        StringAssert.Contains(script, "pair.Right", StringComparison.Ordinal);
        Assert.DoesNotContain("Deconstruct", script, StringComparison.Ordinal);
    }

    [TestMethod]
    public void ConvertRuntimeClass_OverloadedConstructors_EmitSelectorAndBoundBaseHelpers()
    {
        var compilation = CreateCompilation(
            """
            public static class ModuleHost
            {
                public class Base
                {
                    public Base()
                    {
                    }

                    public Base(int seed)
                    {
                    }
                }

                public sealed class Derived : Base
                {
                    public Derived()
                        : base()
                    {
                    }

                    public Derived(int seed)
                        : base(seed)
                    {
                    }
                }
            }
            """);
        var syntaxTree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(syntaxTree);
        var module = compilation.GetTypeByMetadataName("ModuleHost");
        var derived = compilation.GetTypeByMetadataName("ModuleHost+Derived");
        Assert.IsNotNull(module);
        Assert.IsNotNull(derived);

        var declaration = new AstConverter(module!, model).ConvertRuntimeClass(derived!);
        var script = declaration.ToKnRECMAScript();

        StringAssert.Contains(script, "$ctor_", StringComparison.Ordinal);
        StringAssert.Contains(script, "super(\"$ctor_", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_ErasedUnionCreationWithInitializer_ReportsTheUnsupportedInitializerContract()
    {
        var block = CreateBlock(CreateErasedUnionCreationSource("new Choice(1) { State = 2 }"));

        var exception = Assert.Throws<OperationTransformationException>(
            () => new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "does not support object or collection initializers", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ErasedUnionCreationWithMultipleArguments_ReportsTheSingleValueContract()
    {
        var block = CreateBlock(CreateErasedUnionCreationSource("new Choice(1, 2)"));

        var exception = Assert.Throws<OperationTransformationException>(
            () => new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "requires exactly one constructor argument", StringComparison.Ordinal);
    }

    [TestMethod]
    public void InlineTemplate_MemberPlaceholders_RebuildObjectAndComputedPropertyNodes()
    {
        var method = typeof(SemanticWalker).GetMethod(
            "InstantiateInlineTemplate",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var rewritten = Assert.IsInstanceOfType<Acornima.Ast.Expression>(method.Invoke(
            null,
            [
                "coverage.member-placeholder",
                "__arg1[__arg2].name",
                new Acornima.Ast.Expression[]
                {
                    new Acornima.Ast.Identifier("target"),
                    new Acornima.Ast.Identifier("index")
                },
                null,
                null
            ]));

        Assert.AreEqual("target[index].name", rewritten.ToKnRECMAScript());

        var directMember = Assert.IsInstanceOfType<Acornima.Ast.Expression>(method.Invoke(
            null,
            [
                "coverage.member-placeholder.direct",
                "__arg1.name",
                new Acornima.Ast.Expression[] { new Acornima.Ast.Identifier("target") },
                null,
                null
            ]));
        var unchangedMember = Assert.IsInstanceOfType<Acornima.Ast.Expression>(method.Invoke(
            null,
            [
                "coverage.member-placeholder.unchanged",
                "stable.name",
                Array.Empty<Acornima.Ast.Expression>(),
                null,
                null
            ]));

        Assert.AreEqual("target.name", directMember.ToKnRECMAScript());
        Assert.AreEqual("stable.name", unchangedMember.ToKnRECMAScript());
    }

    [TestMethod]
    public void Visit_StringListPattern_UsesLengthAliasAndMappedCharacterAccess()
    {
        var block = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod(string value)
                {
                    var matched = value is ['a', .. var middle, 'z'];
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "value.length", StringComparison.Ordinal);
        StringAssert.Contains(script, "(value, 0)", StringComparison.Ordinal);
        Assert.DoesNotContain("value[0]", script, StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_PropertyPatternOnString_UsesTheRuntimeLengthAlias()
    {
        var block = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod(string value)
                {
                    var nonEmpty = value is { Length: > 0 };
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "value.length", StringComparison.Ordinal);
    }

    [TestMethod]
    public void ContainsAwaitOperation_RecognizesAwaitExpressionsAndAsyncUsingDeclarations()
    {
        var method = typeof(SemanticWalker).GetMethod(
            "ContainsAwaitOperation",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var normal = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod()
                {
                    var value = 1;
                }
            }
            """);
        var awaited = CreateBlock(
            """
            using System.Threading.Tasks;

            public sealed class TestClass
            {
                async Task TestMethod()
                {
                    await Task.CompletedTask;
                }
            }
            """);
        var asyncUsing = CreateBlock(
            """
            using System;
            using System.Threading.Tasks;

            public sealed class Resource : IAsyncDisposable
            {
                public ValueTask DisposeAsync() => default;
            }

            public sealed class TestClass
            {
                async Task TestMethod()
                {
                    await using var resource = new Resource();
                }
            }
            """)
            .DescendantsAndSelf()
            .OfType<IUsingDeclarationOperation>()
            .Single();
        var synchronousUsing = CreateBlock(
            """
            using System;

            public sealed class Resource : IDisposable
            {
                public void Dispose()
                {
                }
            }

            public sealed class TestClass
            {
                void TestMethod(Resource resource)
                {
                    using (resource)
                    {
                    }
                }
            }
            """)
            .DescendantsAndSelf()
            .OfType<IUsingOperation>()
            .Single();
        var asynchronousUsing = CreateBlock(
            """
            using System;
            using System.Threading.Tasks;

            public sealed class Resource : IAsyncDisposable
            {
                public ValueTask DisposeAsync() => default;
            }

            public sealed class TestClass
            {
                async Task TestMethod(Resource resource)
                {
                    await using (resource)
                    {
                    }
                }
            }
            """)
            .DescendantsAndSelf()
            .OfType<IUsingOperation>()
            .Single();

        Assert.IsFalse(InvokePrivateStatic<bool>(method, normal));
        Assert.IsTrue(InvokePrivateStatic<bool>(method, awaited));
        Assert.IsTrue(InvokePrivateStatic<bool>(method, asyncUsing));
        Assert.IsFalse(InvokePrivateStatic<bool>(method, synchronousUsing));
        Assert.IsTrue(InvokePrivateStatic<bool>(method, asynchronousUsing));
    }

    [TestMethod]
    public void PropertyMutationBridge_UsesImportBackedIndexersButNotAliasOnlyProperties()
    {
        var block = CreateBlock(
            """
            using System.Collections.Generic;

            public sealed class TestClass
            {
                void TestMethod(List<int> values)
                {
                    values[0]++;
                    var count = values.Count;
                }
            }
            """);
        var properties = block.DescendantsAndSelf().OfType<IPropertyReferenceOperation>().ToArray();
        var indexer = properties.Single(static property => property.Property.IsIndexer);
        var count = properties.Single(static property => property.Property.Name == "Count");
        var method = typeof(SemanticWalker).GetMethod(
            "RequiresPropertyMutationBridge",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var walker = new SemanticWalker(true);

        Assert.IsTrue(InvokePrivateInstance<bool>(method, walker, indexer));
        Assert.IsFalse(InvokePrivateInstance<bool>(method, walker, count));
    }

    [TestMethod]
    public void GetConfigOrSymbolName_ModuleOverloads_KeepTheOnlyRawRuntimeEntryPoint()
    {
        var compilation = CreateCompilation(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : global::System.Attribute
                {
                    public ECMAScriptModuleAttribute(string path)
                    {
                    }
                }

                [global::System.AttributeUsage(global::System.AttributeTargets.Method, Inherited = false)]
                public sealed class ECMAScriptNameAttribute : global::System.Attribute
                {
                    public ECMAScriptNameAttribute(string name)
                    {
                    }
                }
            }

            [ECMAScript.ECMAScriptModule("runtime/module.mjs")]
            public static class RuntimeModule
            {
                public static void Render()
                {
                }

                [ECMAScript.ECMAScriptName("renderWithValue")]
                public static void Render(int value)
                {
                }
            }
            """);
        var module = compilation.GetTypeByMetadataName("RuntimeModule")!;
        var methods = module.GetMembers("Render").OfType<IMethodSymbol>().ToArray();

        Assert.AreEqual("Render", Util.GetConfigOrSymbolName(methods.Single(static method => method.Parameters.Length == 0)));
        Assert.AreEqual("renderWithValue", Util.GetConfigOrSymbolName(methods.Single(static method => method.Parameters.Length == 1)));
    }

    [TestMethod]
    public void GetConfigOrSymbolName_TupleUnderlyingFields_KeepTheCanonicalAndRawNamesDistinct()
    {
        var compilation = CreateCompilation(
            """
            public static class TupleHost
            {
                public static (int Alias, string Label) Create() => (1, "label");
            }
            """);
        var tuple = Assert.IsInstanceOfType<INamedTypeSymbol>(
            compilation.GetTypeByMetadataName("TupleHost")!
                .GetMembers("Create")
                .OfType<IMethodSymbol>()
                .Single()
                .ReturnType);
        var named = tuple.TupleElements[0];
        Assert.IsNotNull(named.CorrespondingTupleField);
        var underlying = named.CorrespondingTupleField!;

        Assert.AreEqual("Alias", Util.GetConfigOrSymbolName(named));
        Assert.AreEqual("Item1", Util.GetConfigOrSymbolName(underlying));
    }

    [TestMethod]
    public void ListPatternPropertyAccess_PreservesFallbackAndAliasShapesForLengthAndIndexers()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Sliceable
            {
                public int Length => 3;

                public int this[int index] => index;

                public Sliceable Slice(int start, int length) => this;
            }

            public sealed class TestClass
            {
                void TestMethod(Sliceable value)
                {
                    var length = value.Length;
                    var item = value[0];
                    var slice = value.Slice(0, 1);
                }
            }
            """);
        var properties = block.DescendantsAndSelf().OfType<IPropertyReferenceOperation>().ToArray();
        var length = properties.Single(static property => property.Property.Name == "Length");
        var indexer = properties.Single(static property => property.Property.IsIndexer);
        var slice = block.DescendantsAndSelf().OfType<IInvocationOperation>()
            .Single(static invocation => invocation.TargetMethod.Name == "Slice");
        var method = typeof(SemanticWalker).GetMethod(
            "BuildListPatternPropertyAccess",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var boundAccess = typeof(SemanticWalker).GetMethod(
            "BuildListPatternBoundAccess",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var walker = new SemanticWalker(true);
        var receiver = new Acornima.Ast.Identifier("value");

        var fallbackLength = InvokePrivateInstance<Acornima.Ast.Expression>(
            method,
            walker,
            length,
            length.Property.GetMethod!,
            length.Property,
            receiver,
            new List<Acornima.Ast.Expression>(),
            null,
            "list pattern length access",
            length.Property.ContainingType);
        var aliasedLength = InvokePrivateInstance<Acornima.Ast.Expression>(
            method,
            walker,
            length,
            length.Property.GetMethod!,
            length.Property,
            receiver,
            new List<Acornima.Ast.Expression>(),
            "runtimeLength",
            "list pattern length access",
            length.Property.ContainingType);
        var aliasedIndexer = InvokePrivateInstance<Acornima.Ast.Expression>(
            method,
            walker,
            indexer,
            indexer.Property.GetMethod!,
            indexer.Property,
            receiver,
            new List<Acornima.Ast.Expression> { new Acornima.Ast.NumericLiteral(0, "0") },
            "runtimeIndex",
            "list pattern index access",
            indexer.Property.ContainingType);
        var fallbackIndexer = InvokePrivateInstance<Acornima.Ast.Expression>(
            method,
            walker,
            indexer,
            indexer.Property.GetMethod!,
            indexer.Property,
            receiver,
            new List<Acornima.Ast.Expression> { new Acornima.Ast.NumericLiteral(1, "1") },
            null,
            "list pattern index access",
            indexer.Property.ContainingType);
        var fallbackLengthWithoutHost = InvokePrivateInstance<Acornima.Ast.Expression>(
            method,
            walker,
            length,
            length.Property.GetMethod!,
            length.Property,
            receiver,
            new List<Acornima.Ast.Expression>(),
            null,
            "list pattern length access",
            null);
        var fallbackIndexerWithoutHost = InvokePrivateInstance<Acornima.Ast.Expression>(
            method,
            walker,
            indexer,
            indexer.Property.GetMethod!,
            indexer.Property,
            receiver,
            new List<Acornima.Ast.Expression> { new Acornima.Ast.NumericLiteral(1, "1") },
            null,
            "list pattern index access",
            null);
        var fallbackSliceWithoutHost = InvokePrivateInstance<Acornima.Ast.Expression>(
            boundAccess,
            walker,
            slice,
            slice.TargetMethod,
            receiver,
            new List<Acornima.Ast.Expression>
            {
                new Acornima.Ast.NumericLiteral(0, "0"),
                new Acornima.Ast.NumericLiteral(1, "1")
            },
            new SenseArgument(),
            "list pattern slice access",
            null);

        Assert.AreEqual("value.Length", fallbackLength.ToKnRECMAScript());
        Assert.AreEqual("value.runtimeLength", aliasedLength.ToKnRECMAScript());
        Assert.AreEqual("value[0]", aliasedIndexer.ToKnRECMAScript());
        Assert.AreEqual("value[1]", fallbackIndexer.ToKnRECMAScript());
        Assert.AreEqual("value.Length", fallbackLengthWithoutHost.ToKnRECMAScript());
        Assert.AreEqual("value[1]", fallbackIndexerWithoutHost.ToKnRECMAScript());
        Assert.AreEqual("value.Slice(0, 1)", fallbackSliceWithoutHost.ToKnRECMAScript());
    }

    [TestMethod]
    public void Visit_ObjectInitializerWithMultiParameterIndexer_ReportsUnrepresentableJavaScriptKey()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Cell
            {
                public int Value { get; set; }
            }

            [ECMAScript.ECMAScript]
            public sealed class Grid
            {
                public Cell this[int row, int column]
                {
                    get => new Cell();
                    set
                    {
                    }
                }
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var grid = new Grid { [1, 2] = new Cell { Value = 3 } };
                }
            }
            """);

        var exception = Assert.Throws<OperationTransformationException>(
            () => new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "Indexed initializer target", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_MemberInitializerWithMultiParameterIndexer_ReportsUnrepresentableJavaScriptKey()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Cell
            {
                public int Value { get; set; }
            }

            [ECMAScript.ECMAScript]
            public sealed class Grid
            {
                public Cell this[int row, int column] => new Cell();
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var grid = new Grid { [1, 2] = { Value = 3 } };
                }
            }
            """);

        var exception = Assert.Throws<OperationTransformationException>(
            () => new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "only supports a single translated index argument", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_StructuralRecordDeconstruction_CachesInvocationAndSkipsDiscard()
    {
        var block = CreateBlock(
            """
            public sealed record Pair(int First, int Second);

            public sealed class TestClass
            {
                private static Pair Create() => new Pair(1, 2);

                void TestMethod()
                {
                    var (first, _) = Create();
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "Create()", StringComparison.Ordinal);
        StringAssert.Contains(script, ".First", StringComparison.Ordinal);
        Assert.DoesNotContain(".Second", script, StringComparison.Ordinal);
        Assert.DoesNotContain("Deconstruct", script, StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ExtensionDeconstruction_ReportsTheUnsupportedReceiverProtocol()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Pair
            {
            }

            [ECMAScript.ECMAScript]
            public static class PairExtensions
            {
                public static void Deconstruct(this Pair pair, out int left, out int right)
                {
                    left = 1;
                    right = 2;
                }
            }

            public sealed class TestClass
            {
                void TestMethod(Pair pair)
                {
                    var (left, right) = pair;
                }
            }
            """);

        var exception = Assert.Throws<OperationTransformationException>(
            () => new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "Extension Deconstruct method", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_StructDeconstruction_ReportsTheNoRuntimeClassContract()
    {
        var block = CreateBlock(
            """
            public struct Pair
            {
                public void Deconstruct(out int left, out int right)
                {
                    left = 1;
                    right = 2;
                }
            }

            public sealed class TestClass
            {
                void TestMethod(Pair pair)
                {
                    var (left, right) = pair;
                }
            }
            """);

        var exception = Assert.Throws<OperationTransformationException>(
            () => new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "Custom Deconstruct on struct type", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ObjectInitializer_HostInstanceProjectionOwnsImplicitReceivers()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Cell
            {
                public int Field;
                public int Value { get; set; }
            }

            [ECMAScript.ECMAScript]
            public sealed class Holder
            {
                public Cell Field = new Cell();
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var holder = new Holder
                    {
                        Field = new Cell { Value = 1 }
                    };
                }
            }
            """);
        var walker = new SemanticWalker(true)
        {
            Host = new InitializerInstanceProjectionHost()
        };

        var script = walker.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "receiver.Field", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void TransformationExceptions_SourceLocationsAttachStableDiagnosticMetadata()
    {
        var tree = CSharpSyntaxTree.ParseText("public sealed class Contract { }", path: "contracts/Contract.cs");
        var location = tree.GetRoot().GetLocation();

        var operationException = new OperationTransformationException(OperationKind.None, "failure", location);
        var syntaxException = new SyntaxNodeTransformationException(SyntaxKind.ClassDeclaration, "failure", location);
        var none = new OperationTransformationException(OperationKind.None, "failure", Location.None);

        Assert.AreEqual("contracts/Contract.cs", operationException.Data["location.path"]);
        Assert.AreEqual("contracts/Contract.cs", syntaxException.Data["location.path"]);
        Assert.IsFalse(none.Data.Contains("location.path"));
    }

    [TestMethod]
    public void CompositeSemanticWalkerHost_NullHostArray_ReportsItsRequiredCompositionContract()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CompositeSemanticWalkerHost((SemanticWalkerHost[])null!));
    }

    [TestMethod]
    public void ConstructorRefOutSinkProtocol_ValueReturn_RejectsAnInvalidConstructorBody()
    {
        var body = new Acornima.Ast.FunctionBody(
            Acornima.Ast.NodeList.From<Acornima.Ast.Statement>(
                new Acornima.Ast.ReturnStatement(new Acornima.Ast.NumericLiteral(1, "1"))),
            strict: true);

        var exception = Assert.Throws<NotSupportedException>(() => ConstructorRefOutSinkProtocol.Apply(
            body,
            [new Acornima.Ast.Identifier("value")],
            new Acornima.Ast.Identifier("sink")));

        StringAssert.Contains(exception.Message, "value-return statement", StringComparison.Ordinal);
    }

    [TestMethod]
    public void EsGenerator_DirectorySeparatorNormalization_HandlesBothTerminalStates()
    {
        var method = typeof(ESGenerator).GetMethod(
            "EnsureDirectorySeparator",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var separator = Path.DirectorySeparatorChar.ToString();

        Assert.AreEqual("root" + separator, InvokePrivateStatic<string>(method, "root"));
        Assert.AreEqual("root" + separator, InvokePrivateStatic<string>(method, "root" + separator));
    }

    [TestMethod]
    public void LoweringSite_UnknownKind_UsesTheStableGenericTemporaryTag()
    {
        Assert.AreEqual("temp", new LoweringSite((LoweringSiteKind)(-1)).Tag);
    }

    [TestMethod]
    public void IsHostSkippedVariableDeclaration_DistinguishesDeclarationAndGroupOwnership()
    {
        var block = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod()
                {
                    int first = 1, second = 2;
                }
            }
            """);
        var group = block.Operations.OfType<IVariableDeclarationGroupOperation>().Single();
        var declaration = group.Declarations.Single();
        var method = typeof(SemanticWalker).GetMethod(
            "IsHostSkippedVariableDeclaration",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var ownedWalker = new SemanticWalker(true) { Host = new AllVariableDeclaratorsHost() };
        var ordinaryWalker = new SemanticWalker(true) { Host = new NoVariableDeclaratorsHost() };

        Assert.IsTrue(InvokePrivateInstance<bool>(method, ownedWalker, group, new SenseArgument()));
        Assert.IsTrue(InvokePrivateInstance<bool>(method, ownedWalker, declaration, new SenseArgument()));
        Assert.IsFalse(InvokePrivateInstance<bool>(method, ordinaryWalker, group, new SenseArgument()));
        Assert.IsFalse(InvokePrivateInstance<bool>(method, ordinaryWalker, declaration, new SenseArgument()));
    }

    [TestMethod]
    public void Visit_AsyncUsingThroughNestedGenericConstraint_ResolvesTheAsyncDisposalContract()
    {
        var block = CreateBlock(
            """
            using System;
            using System.Threading.Tasks;

            public sealed class TestClass
            {
                async Task TestMethod<TResource, TConstraint>(TResource resource)
                    where TResource : TConstraint
                    where TConstraint : IAsyncDisposable
                {
                    await using (resource)
                    {
                        await Task.Yield();
                    }
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "await", StringComparison.Ordinal);
        StringAssert.Contains(script, "finally", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ForEachStringAndStructuralRecord_DeconstructsTheirRuntimeShapes()
    {
        var stringBlock = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod(string value)
                {
                    foreach (var character in value)
                    {
                    }
                }
            }
            """);
        var recordBlock = CreateBlock(
            """
            public sealed record Pair(int Left, string Right);

            public sealed class TestClass
            {
                void TestMethod(Pair[] values)
                {
                    foreach (var (left, right) in values)
                    {
                    }
                }
            }
            """);

        var stringScript = VisitBlock(stringBlock);
        var recordScript = VisitBlock(recordBlock);

        StringAssert.Contains(stringScript, "for (let character of", StringComparison.Ordinal);
        StringAssert.Contains(recordScript, "{ Left: left, Right: right }", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_MemberInitializer_CachesAnIndexerReceiverWithSideEffects()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Cell
            {
                public int Value;
            }

            [ECMAScript.ECMAScript]
            public sealed class Holder
            {
                private readonly Cell[] cells = [new Cell()];

                public Cell this[int index] => cells[index];
            }

            public sealed class TestClass
            {
                private int next;

                private int NextIndex() => next++;

                void TestMethod()
                {
                    var holder = new Holder { [NextIndex()] = { Value = 1 } };
                }
            }
            """);

        var script = VisitBlock(block);

        Assert.AreEqual(1, script.Split("NextIndex()", StringSplitOptions.None).Length - 1);
        StringAssert.Contains(script, "Value = 1", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_MemberInitializer_HostCanProjectTheOuterImplicitReceiverOnly()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Cell
            {
                public int Value;
            }

            [ECMAScript.ECMAScript]
            public sealed class Holder
            {
                public Cell Child { get; } = new Cell();
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var holder = new Holder { Child = { Value = 1 } };
                }
            }
            """);
        var walker = new SemanticWalker(true)
        {
            Host = new OuterMemberInitializerProjectionHost()
        };

        var script = walker.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "owner.Child.Value = 1", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void GetConfigOrSymbolName_UsesStableNamesForAutoPropertyStorageAndOrdinaryOverloads()
    {
        var compilation = CreateCompilation(
            """
            public sealed class Shape
            {
                public int Value { get; set; }

                public void Render(int value)
                {
                }

                public void Render(string value)
                {
                }
            }
            """);
        var shape = compilation.GetTypeByMetadataName("Shape")!;
        var backingField = shape.GetMembers("<Value>k__BackingField").OfType<IFieldSymbol>().Single();
        var overloads = shape.GetMembers("Render").OfType<IMethodSymbol>().ToArray();
        var overloadNames = overloads.Select(Util.GetConfigOrSymbolName).ToArray();

        Assert.IsTrue(Util.GetConfigOrSymbolName(backingField).StartsWith("_", StringComparison.Ordinal));
        Assert.HasCount(2, overloadNames.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(overloadNames.All(static name => name.StartsWith("Render_", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void Visit_DeconstructionAssignment_CachesDependentTupleFieldsBeforeMutatingTargets()
    {
        var block = CreateBlock(
            """
            public sealed class TestClass
            {
                private int left;
                private int right;

                void TestMethod()
                {
                    (left, right) = (right, left);
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "left", StringComparison.Ordinal);
        StringAssert.Contains(script, "right", StringComparison.Ordinal);
        StringAssert.Contains(script, "let", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_DeconstructionAssignment_RejectsImportedStaticFieldWrites()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : global::System.Attribute
                {
                    public ECMAScriptModuleAttribute(string path)
                    {
                    }
                }
            }

            [ECMAScript.ECMAScriptModule("./imported-state")]
            public static class ImportedState
            {
                public static int Value;
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var local = 0;
                    (ImportedState.Value, local) = (1, 2);
                }
            }
            """);

        var exception = Assert.Throws<OperationTransformationException>(
            () => new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "imported bindings are read-only", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ObjectInitializer_RejectsMultiParameterIndexerFallback()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Grid
            {
                public int this[int row, int column]
                {
                    get => 0;
                    set
                    {
                    }
                }
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var grid = new Grid { [1, 2] = 3 };
                }
            }
            """);

        var exception = Assert.Throws<OperationTransformationException>(
            () => new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "Indexed initializer target", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ArrayListPatterns_LowersLeadingDiscardAndTrailingSlices()
    {
        var block = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod(int[] values)
                {
                    var leading = values is [.. var prefix, 3];
                    var discarded = values is [1, .., 3];
                    var empty = values is [];
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "Array.isArray(values)", StringComparison.Ordinal);
        StringAssert.Contains(script, "values.slice", StringComparison.Ordinal);
        StringAssert.Contains(script, "values.length", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_ArrayElementIncrementAndDecrement_CachesSideEffectingIndexes()
    {
        var block = CreateBlock(
            """
            public sealed class TestClass
            {
                private int next;

                private int NextIndex() => next++;

                void TestMethod(int[] values)
                {
                    values[NextIndex()]++;
                    --values[NextIndex()];
                }
            }
            """);

        var script = VisitBlock(block);

        Assert.AreEqual(2, script.Split("NextIndex()", StringSplitOptions.None).Length - 1);
        StringAssert.Contains(script, "++", StringComparison.Ordinal);
        StringAssert.Contains(script, "--", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TransformationExceptions_UseUnknownPathForSourceLocationsWithoutPathMetadata()
    {
        var location = Location.Create(
            string.Empty,
            new Microsoft.CodeAnalysis.Text.TextSpan(0, 1),
            new Microsoft.CodeAnalysis.Text.LinePositionSpan(
                new Microsoft.CodeAnalysis.Text.LinePosition(0, 0),
                new Microsoft.CodeAnalysis.Text.LinePosition(0, 1)));
        var operation = new OperationTransformationException(OperationKind.None, "failure", location);
        var syntax = new SyntaxNodeTransformationException(SyntaxKind.None, "failure", location);

        Assert.AreEqual("<unknown>", operation.Data["location.path"]);
        Assert.AreEqual("<unknown>", syntax.Data["location.path"]);
    }

    [TestMethod]
    public void GetConfigOrSymbolName_PreservesRuntimeAndModuleOverloadNamingRules()
    {
        var compilation = CreateCompilation(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }

                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : global::System.Attribute
                {
                }

                [global::System.AttributeUsage(global::System.AttributeTargets.Method, Inherited = false)]
                public sealed class ECMAScriptNameAttribute : global::System.Attribute
                {
                    public ECMAScriptNameAttribute(string value)
                    {
                    }
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class RuntimeHost
            {
                public void Call(int value)
                {
                }

                public void Call(string value)
                {
                }
            }

            [ECMAScript.ECMAScriptModule]
            public static class ModuleHost
            {
                public static void Render(int value)
                {
                }

                [ECMAScript.ECMAScriptName("renderText")]
                public static void Render(string value)
                {
                }
            }
            """);
        var runtimeHost = compilation.GetTypeByMetadataName("RuntimeHost")!;
        var moduleHost = compilation.GetTypeByMetadataName("ModuleHost")!;
        var runtimeCalls = runtimeHost.GetMembers("Call").OfType<IMethodSymbol>().ToArray();
        var moduleCalls = moduleHost.GetMembers("Render").OfType<IMethodSymbol>().ToArray();

        Assert.IsTrue(runtimeCalls.All(static method => Util.GetConfigOrSymbolName(method) == "Call"));
        Assert.AreEqual("Render", Util.GetConfigOrSymbolName(moduleCalls.Single(static method => method.Parameters[0].Type.SpecialType == SpecialType.System_Int32)));
        Assert.AreEqual("renderText", Util.GetConfigOrSymbolName(moduleCalls.Single(static method => method.Parameters[0].Type.SpecialType == SpecialType.System_String)));
    }

    [TestMethod]
    public void AstConverter_ImportAndHostHelpers_KeepAllLegalShapesStable()
    {
        var module = new Parser().ParseModule(
            """
            import defaultValue from "default.mjs";
            import * as namespaceValue from "namespace.mjs";
            import { read as localRead, "runtime-key" as localRuntime } from "named.mjs";
            """);
        var specifiers = module.Body
            .OfType<Acornima.Ast.ImportDeclaration>()
            .SelectMany(static declaration => declaration.Specifiers)
            .ToArray();
        var getImportedName = typeof(AstConverter).GetMethod(
            "GetImportedSpecifierName",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(getImportedName);

        CollectionAssert.AreEquivalent(
            new[] { "default", "*", "read", "runtime-key" },
            specifiers.Select(specifier => InvokePrivateStatic<string>(getImportedName!, specifier)).ToArray());

        var normalized = ImportDeclarationFactory.NormalizeSpecifiers(specifiers.Concat(specifiers));
        Assert.HasCount(4, normalized);

        var combineHosts = typeof(AstConverter).GetMethod(
            "CombineSemanticWalkerHosts",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(combineHosts);
        Assert.IsNull(combineHosts!.Invoke(null, [Array.Empty<SemanticWalkerHost?>()]));

        var first = new AllVariableDeclaratorsHost();
        var second = new NoVariableDeclaratorsHost();
        Assert.AreSame(first, combineHosts.Invoke(null, [new SemanticWalkerHost?[] { first }]));
        var composite = combineHosts.Invoke(null, [new SemanticWalkerHost?[] { first, null, second }]);
        Assert.IsNotNull(composite);
        Assert.AreNotSame(first, composite);
        Assert.AreNotSame(second, composite);
    }

    [TestMethod]
    public void RuntimeClassPrivateStorageNames_KeepAutoPropertyAndExplicitFieldProtocolsDistinct()
    {
        var compilation = CreateCompilation(
            """
            public sealed class RuntimeHost
            {
                public int Auto { get; set; }
                public int Explicit;
            }
            """);
        var type = compilation.GetTypeByMetadataName("RuntimeHost")!;
        var autoBackingField = type.GetMembers()
            .OfType<IFieldSymbol>()
            .Single(static field => field.AssociatedSymbol is IPropertySymbol);
        var explicitField = type.GetMembers("Explicit").OfType<IFieldSymbol>().Single();

        Assert.AreEqual(
            "fallback",
            RuntimeClassPrivateStorageNames.GetFieldStorageName(
                RuntimeClassPrivateStorage.JavaScriptPrivateFields,
                autoBackingField,
                "fallback"));
        Assert.AreEqual(
            "$jazor$private$" + Jazor.Common.Format.HashName(
                ((IPropertySymbol)autoBackingField.AssociatedSymbol!).OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)),
            RuntimeClassPrivateStorageNames.GetFieldStorageName(
                RuntimeClassPrivateStorage.ProxySafeMangledProperties,
                autoBackingField,
                "fallback"));
        Assert.AreEqual(
            "$jazor$private$explicitFallback",
            RuntimeClassPrivateStorageNames.GetFieldStorageName(
                RuntimeClassPrivateStorage.ProxySafeMangledProperties,
                explicitField,
                "explicitFallback"));
    }

    [TestMethod]
    public void RecordProxyClassification_UsesOnlyRuntimeVisibleMemberContracts()
    {
        var compilation = CreateCompilation(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.All)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }

                [global::System.AttributeUsage(global::System.AttributeTargets.All)]
                public sealed class ECMAScriptNameAttribute : global::System.Attribute
                {
                    public ECMAScriptNameAttribute(string value)
                    {
                    }
                }

                [global::System.AttributeUsage(global::System.AttributeTargets.All)]
                public sealed class ECMAScriptInlineAttribute : global::System.Attribute
                {
                    public ECMAScriptInlineAttribute(string value)
                    {
                    }
                }
            }

            [ECMAScript.ECMAScript]
            public record RuntimeRecord
            {
                public int Plain { get; set; }

                [ECMAScript.ECMAScriptName("renamed")]
                public int Renamed { get; set; }

                public int this[int index] => index;

                [ECMAScript.ECMAScriptInline("runtime(__arg1)")]
                public int Inline(int value) => value;

                public int Ordinary(int value) => value;
            }
            """);
        var record = compilation.GetTypeByMetadataName("RuntimeRecord")!;
        var plain = record.GetMembers("Plain").OfType<IPropertySymbol>().Single();
        var renamed = record.GetMembers("Renamed").OfType<IPropertySymbol>().Single();
        var indexer = record.GetMembers().OfType<IPropertySymbol>().Single(static property => property.IsIndexer);
        var inline = record.GetMembers("Inline").OfType<IMethodSymbol>().Single();
        var ordinary = record.GetMembers("Ordinary").OfType<IMethodSymbol>().Single();

        Assert.IsFalse(Util.IsECMAScriptRecordProxyMember(plain));
        Assert.IsTrue(Util.IsECMAScriptRecordProxyMember(renamed));
        Assert.IsTrue(Util.IsECMAScriptRecordProxyMember(indexer));
        Assert.IsTrue(Util.IsECMAScriptRecordProxyMember(inline));
        Assert.IsFalse(Util.IsECMAScriptRecordProxyMember(ordinary));
    }

    [TestMethod]
    public void ErasedInterfacePatternFolding_UsesDeterministicValuesAndNonNullStaticProofs()
    {
        var compilation = CreateCompilation(
            """
            public interface IContract
            {
            }

            public sealed class Runtime : IContract
            {
            }

            public readonly struct ValueRuntime : IContract
            {
            }

            public sealed class Other
            {
            }

            public sealed class TestClass
            {
                void TestMethod(Runtime runtime, ValueRuntime valueRuntime)
                {
                    var constructed = new Runtime() is IContract;
                    var rejected = new Other() is IContract;
                    var empty = default(Runtime) is IContract;
                    var guarded = runtime is IContract;
                    var value = valueRuntime is IContract;
                }
            }
            """);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var contract = compilation.GetTypeByMetadataName("IContract");
        Assert.IsNotNull(contract);
        var method = typeof(SemanticWalker).GetMethod(
            "TryEvaluateCompileTimeErasedInterfaceIsTypeCheck",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(method);
        var walker = new SemanticWalker(true);

        AssertFold("constructed", "AlwaysTrue");
        AssertFold("rejected", "AlwaysFalse");
        AssertFold("empty", "AlwaysFalse");
        AssertFold("guarded", "NonNullOnly");
        AssertFold("value", "AlwaysTrue");

        void AssertFold(string localName, string expected)
        {
            var declarator = tree.GetRoot()
                .DescendantNodes()
                .OfType<VariableDeclaratorSyntax>()
                .Single(candidate => candidate.Identifier.ValueText == localName);
            var operation = Assert.IsInstanceOfType<IIsTypeOperation>(
                model.GetOperation(declarator.Initializer!.Value));
            var arguments = new object?[] { operation, contract!, null };
            Assert.IsTrue((bool)method!.Invoke(walker, arguments)!);
            Assert.AreEqual(expected, arguments[2]!.ToString());
        }
    }

    [TestMethod]
    public void TupleHelpers_KeepReturnProjectionAndSingleEvaluationContractsExplicit()
    {
        var compilation = CreateCompilation(
            """
            public readonly record struct Shape(int Left, int Right);

            public sealed class TestClass
            {
                private static (int first, int second) GetPair() => (1, 2);

                private static (int left, int right) ReturnPair()
                {
                    return GetPair();
                }

                void TestMethod((int first, int second) pair)
                {
                    var local = pair;
                    var computed = GetPair();
                }
            }
            """);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var returnSyntax = tree.GetRoot().DescendantNodes().OfType<ReturnStatementSyntax>().Single();
        var returnOperation = Assert.IsInstanceOfType<IReturnOperation>(model.GetOperation(returnSyntax));
        var getReturnType = typeof(SemanticWalker).GetMethod(
            "GetTupleReturnTargetType",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(getReturnType);
        var returnType = Assert.IsInstanceOfType<INamedTypeSymbol>(getReturnType!.Invoke(null, [returnOperation]));
        Assert.IsTrue(returnType.IsTupleType);
        Assert.AreEqual("left", returnType.TupleElements[0].Name);

        var shape = compilation.GetTypeByMetadataName("Shape");
        Assert.IsNotNull(shape);
        var walker = new SemanticWalker(true);
        var structuralProperty = typeof(SemanticWalker).GetMethod(
            "TryGetStructuralRuntimeProperty",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(structuralProperty);
        var firstPropertyArguments = new object?[] { shape!, 0, null, null };
        Assert.IsTrue((bool)structuralProperty!.Invoke(walker, firstPropertyArguments)!);
        Assert.AreEqual("Left", firstPropertyArguments[2]);
        Assert.AreEqual(SpecialType.System_Int32, ((ITypeSymbol)firstPropertyArguments[3]!).SpecialType);
        var missingPropertyArguments = new object?[] { shape!, 2, null, null };
        Assert.IsFalse((bool)structuralProperty.Invoke(walker, missingPropertyArguments)!);

        var shouldCache = typeof(SemanticWalker).GetMethod(
            "ShouldCacheTupleSource",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(shouldCache);
        var declarators = tree.GetRoot().DescendantNodes().OfType<VariableDeclaratorSyntax>()
            .Where(static candidate => candidate.Identifier.ValueText is "local" or "computed")
            .ToDictionary(static candidate => candidate.Identifier.ValueText, StringComparer.Ordinal);
        var localValue = ((IVariableDeclaratorOperation)model.GetOperation(declarators["local"])!).Initializer!.Value;
        var computedValue = ((IVariableDeclaratorOperation)model.GetOperation(declarators["computed"])!).Initializer!.Value;
        Assert.IsFalse((bool)shouldCache!.Invoke(null, [localValue])!);
        Assert.IsTrue((bool)shouldCache.Invoke(null, [computedValue])!);
    }

    [TestMethod]
    public void Visit_RecursiveListAndRangePatterns_PreserveAuthoringEvaluationProtocols()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Point
            {
                public int X { get; set; }
                public int Y { get; set; }
            }

            public sealed class TestClass
            {
                void TestMethod(Point point, int[] values)
                {
                    if (point is { X: > 0, Y: var y })
                    {
                        var value = y;
                    }

                    var matched = values is [var first, .. var rest];
                    var last = values[^1];
                    var middle = values[1..^1];
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "point.X", StringComparison.Ordinal);
        StringAssert.Contains(script, "values.length", StringComparison.Ordinal);
        StringAssert.Contains(script, "slice", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_CompositeRecursivePatterns_PreservesMappedMembersAndSingleEvaluation()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }

                [global::System.AttributeUsage(global::System.AttributeTargets.All, Inherited = false)]
                public sealed class ECMAScriptNameAttribute : global::System.Attribute
                {
                    public ECMAScriptNameAttribute(string value)
                    {
                    }
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class PatternNode
            {
                [ECMAScript.ECMAScriptName("display-name")]
                public string Name { get; set; } = "";

                public PatternNode? Child { get; set; }

                public int[] Scores { get; set; } = [];
            }

            [ECMAScript.ECMAScript]
            public sealed record Position(int Row, int Column);

            public sealed class TestClass
            {
                private static PatternNode? CreateNode() => null;

                void TestMethod(object? candidate, PatternNode? node, int[] values)
                {
                    var recursive = CreateNode() is PatternNode
                    {
                        Name: "ready",
                        Child: { Scores: [> 0, .. var rest] }
                    };
                    var alternatives = node is { Name: "ready" } or { Scores: [] };
                    var declared = candidate is PatternNode typed;
                    var positional = new Position(1, 2) is Position(1, var column);
                    var inverted = values is not [];
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "display-name", StringComparison.Ordinal);
        StringAssert.Contains(script, "CreateNode()", StringComparison.Ordinal);
        StringAssert.Contains(script, "Scores", StringComparison.Ordinal);
        StringAssert.Contains(script, "Row", StringComparison.Ordinal);
        StringAssert.Contains(script, "values.length", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_PatternBranchMatrix_CoversDiscardGuardPropertyAndListProtocols()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class PatternNode
            {
                public int Value;
                public int[] Values { get; set; } = [];
            }

            public sealed class TestClass
            {
                void TestMethod(object? candidate, PatternNode? node, int[] values, string text)
                {
                    var discarded = candidate is PatternNode _;
                    var declared = candidate is PatternNode { Value: > 0 } matched;
                    var field = node is { Value: var current };
                    var property = node is { Values: [var first, .. var middle, var last] };
                    var array = values is [var head, .. var tail];
                    var empty = values is [];
                    var stringShape = text is ['a', .. var body, 'z'];

                    switch (candidate)
                    {
                        case PatternNode { Value: > 0 } guarded when node is not null:
                            break;
                        case PatternNode:
                            break;
                        default:
                            break;
                    }
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "Value", StringComparison.Ordinal);
        StringAssert.Contains(script, "Values", StringComparison.Ordinal);
        StringAssert.Contains(script, "length", StringComparison.Ordinal);
        StringAssert.Contains(script, "slice", StringComparison.Ordinal);
        StringAssert.Contains(script, "guarded", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_PatternFallbackAndStructuralTypeBoundaries_PreserveNullAndInterfaceContracts()
    {
        var block = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod(object? candidate)
                {
                    var nonNull = candidate is {};
                    var result = nonNull;
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "candidate != null", StringComparison.Ordinal);

        var structuralBlock = CreateBlock(
            """
            public sealed record StructuralBag(string Name);

            public sealed class TestClass
            {
                void TestMethod(StructuralBag value)
                {
                    var result = value is StructuralBag;
                }
            }
            """);

        var exception = Assert.Throws<OperationTransformationException>(
            () => new SemanticWalker(true).Visit(structuralBlock, new SenseArgument()));

        StringAssert.Contains(exception.Message, "uses structural lowering", StringComparison.Ordinal);
    }

    [TestMethod]
    public void ConvertRuntimeClass_ComplexPrimaryConstructorInheritance_PreservesFieldsPropertiesAndMethods()
    {
        var compilation = CreateCompilation(
            """
            public static class ModuleHost
            {
                public class Base(int seed)
                {
                    protected int BaseValue = seed;

                    protected int ReadBase() => BaseValue;
                }

                public sealed class Derived(int seed, string label) : Base(seed)
                {
                    private readonly string _label = label;

                    public int Value { get; private set; } = seed;

                    public string Describe()
                    {
                        Value++;
                        return _label + ":" + ReadBase() + ":" + Value;
                    }
                }
            }
            """);
        var syntaxTree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(syntaxTree);
        var module = compilation.GetTypeByMetadataName("ModuleHost");
        var derived = compilation.GetTypeByMetadataName("ModuleHost+Derived");
        Assert.IsNotNull(module);
        Assert.IsNotNull(derived);

        var declaration = new AstConverter(module!, model).ConvertRuntimeClass(derived!);
        var script = declaration.ToKnRECMAScript();

        StringAssert.Contains(script, "extends Base", StringComparison.Ordinal);
        StringAssert.Contains(script, "super(seed)", StringComparison.Ordinal);
        StringAssert.Contains(script, "Describe()", StringComparison.Ordinal);
        StringAssert.Contains(script, "this.Value++", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_PatternSwitchAndDeclarationForms_PreserveTypeGuardsAndCaseScopes()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class PatternEntry
            {
                public int Count { get; set; }

                public string? Name { get; set; }
            }

            public sealed class TestClass
            {
                void TestMethod(object? candidate)
                {
                    var typed = candidate is PatternEntry entry;
                    var typeOnly = candidate is PatternEntry;
                    var inferred = candidate is var preserved;

                    switch (candidate)
                    {
                        case PatternEntry { Name: "ready", Count: > 0 } matched:
                            var rendered = matched.Name;
                            break;
                        case null:
                            break;
                        default:
                            var fallback = "empty";
                            break;
                    }
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "PatternEntry", StringComparison.Ordinal);
        StringAssert.Contains(script, "matched", StringComparison.Ordinal);
        StringAssert.Contains(script, "ready", StringComparison.Ordinal);
        StringAssert.Contains(script, "empty", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_CustomListPatternSlices_CacheImpureLengthAndPreserveEdgeBindings()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Sliceable
            {
                public int Length => 3;

                public int this[int index] => index;

                public Sliceable Slice(int start, int length) => this;
            }

            public sealed class TestClass
            {
                void TestMethod(Sliceable value)
                {
                    var middleSlice = value is [var first, .. var middle, var last];
                    var leadingSlice = value is [.., var trailing];
                    var empty = value is [];
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "= value.Length", StringComparison.Ordinal);
        StringAssert.Contains(script, "value.Slice(1, v$0 - 2)", StringComparison.Ordinal);
        StringAssert.Contains(script, "value.Length", StringComparison.Ordinal);
    }

    [TestMethod]
    public void ErasedInterfacePatternFolding_UsesGenericConstraintChainsWithoutRuntimeHeuristics()
    {
        var compilation = CreateCompilation(
            """
            public interface IContract
            {
            }

            public interface IIntermediate : IContract
            {
            }

            public sealed class TestClass
            {
                void TestMethod<TDirect, TIndirect>(TDirect direct, TIndirect indirect, int? nullable)
                    where TDirect : IContract
                    where TIndirect : IIntermediate
                {
                    var directMatch = direct is IContract;
                    var indirectMatch = indirect is IContract;
                    var nullableMatch = nullable is System.IComparable;
                }
            }
            """);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var contract = compilation.GetTypeByMetadataName("IContract");
        var comparable = compilation.GetTypeByMetadataName("System.IComparable");
        Assert.IsNotNull(contract);
        Assert.IsNotNull(comparable);
        var fold = typeof(SemanticWalker).GetMethod(
            "TryEvaluateCompileTimeErasedInterfaceIsTypeCheck",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(fold);
        var walker = new SemanticWalker(true);

        foreach (var declarator in tree.GetRoot().DescendantNodes().OfType<VariableDeclaratorSyntax>())
        {
            var operation = Assert.IsInstanceOfType<IIsTypeOperation>(
                model.GetOperation(declarator.Initializer!.Value));
            var targetInterface = declarator.Identifier.ValueText == "nullableMatch" ? comparable! : contract!;
            var arguments = new object?[] { operation, targetInterface, null };

            Assert.IsTrue((bool)fold!.Invoke(walker, arguments)!);
            Assert.AreEqual("NonNullOnly", arguments[2]!.ToString());
        }

        var script = VisitBlock(
            CreateBlock(
                """
                public sealed class TestClass
                {
                    void TestMethod(int? nullable)
                    {
                        var nullableMatch = nullable is System.IComparable;
                    }
                }
                """));
        StringAssert.Contains(script, "nullable != null", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_CustomNestedDeconstruction_PreservesTheOutResultAndNestedTupleWrites()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Point
            {
                public void Deconstruct(out int x, out int y)
                {
                    x = 1;
                    y = 2;
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Coordinates
            {
                public void Deconstruct(out Point point, out int depth)
                {
                    point = new Point();
                    depth = 3;
                }
            }

            public sealed class TestClass
            {
                void TestMethod(Coordinates coordinates)
                {
                    var x = 0;
                    var y = 0;
                    var depth = 0;
                    ((x, y), depth) = coordinates;
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "coordinates.Deconstruct", StringComparison.Ordinal);
        StringAssert.Contains(script, "x", StringComparison.Ordinal);
        StringAssert.Contains(script, "y", StringComparison.Ordinal);
        StringAssert.Contains(script, "depth", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Visit_RuntimeParamsArguments_ExpandsLiteralCollectionAndBoundArrayForms()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class RuntimeSink
            {
                public void Send(params int[] values)
                {
                }
            }

            public sealed class TestClass
            {
                void TestMethod(RuntimeSink sink, int[] values)
                {
                    sink.Send(1, 2);
                    sink.Send([3, 4]);
                    sink.Send(values);
                    sink.Send(values: values);
                }
            }
            """);

        var script = VisitBlock(block);

        StringAssert.Contains(script, "sink.Send(1, 2)", StringComparison.Ordinal);
        StringAssert.Contains(script, "sink.Send(3, 4)", StringComparison.Ordinal);
        StringAssert.Contains(script, "sink.Send(...values)", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Util_RuntimeMarkerAndImportMetadataBoundaries_KeepSymbolContractsExplicit()
    {
        var compilation = CreateCompilation(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class | global::System.AttributeTargets.Struct)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }

                [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
                public sealed class ECMAScriptModuleAttribute : global::System.Attribute
                {
                    public ECMAScriptModuleAttribute(string path)
                    {
                    }
                }

                [global::System.AttributeUsage(global::System.AttributeTargets.Property | global::System.AttributeTargets.Method)]
                public sealed class ECMAScriptNameAttribute : global::System.Attribute
                {
                    public ECMAScriptNameAttribute(string value)
                    {
                    }
                }

                [global::System.AttributeUsage(global::System.AttributeTargets.Method)]
                public sealed class ECMAScriptInlineAttribute : global::System.Attribute
                {
                    public ECMAScriptInlineAttribute(string template)
                    {
                    }
                }
            }

            [global::System.AttributeUsage(global::System.AttributeTargets.All)]
            public sealed class EmptyMetadataAttribute : global::System.Attribute
            {
            }

            [global::System.AttributeUsage(global::System.AttributeTargets.All)]
            public sealed class DescriptionAttribute : global::System.Attribute
            {
                public DescriptionAttribute(string value)
                {
                }
            }

            [global::System.AttributeUsage(global::System.AttributeTargets.Property)]
            public sealed class JazorAttribute : global::System.Attribute
            {
                public JazorAttribute(int operation, string memberName, string runtimeName)
                {
                }
            }

            [ECMAScript.ECMAScript]
            [Description("@#runtime-boundary")]
            [EmptyMetadata]
            public record RuntimeRecord
            {
                public int Plain { get; set; }

                [ECMAScript.ECMAScriptName("named")]
                public int Named { get; set; }

                [Description("@#")]
                public int Boundary { get; set; }

                [Jazor(3, "mapped-member", "mapped-runtime")]
                public int Imported { get; set; }

                [Jazor(3, "mapped-without-runtime", null)]
                public int ImportedWithoutRuntime { get; set; }

                [Jazor(0, "", "")]
                public int InvalidImport { get; set; }

                public int this[int index] => index;

                [ECMAScript.ECMAScriptInline("__arg1")]
                public int Inline() => 0;

                [ECMAScript.ECMAScriptInline("   ")]
                public int BlankInline() => 0;

                [ECMAScript.ECMAScriptName("named-method")]
                public int NamedMethod() => 0;

                public extern int ExternMethod();

                [EmptyMetadata]
                public int Ordinary() => 0;
            }

            [ECMAScript.ECMAScriptModule("runtime/module.mjs")]
            public static class RuntimeModule
            {
                public static int Read(int value) => value;

                [ECMAScript.ECMAScriptName("readText")]
                public static int Read(string value) => value.Length;
            }

            public static class OverloadedHost
            {
                public static int Read(int value) => value;
                public static int Read(string value) => value.Length;
            }

            public record PlainRecord
            {
                [EmptyMetadata]
                public int Value { get; set; }

                public int Ordinary() => 0;
            }

            public sealed class PlainClass
            {
            }
            """);
        var record = compilation.GetTypeByMetadataName("RuntimeRecord");
        var module = compilation.GetTypeByMetadataName("RuntimeModule");
        var plain = compilation.GetTypeByMetadataName("PlainClass");
        Assert.IsNotNull(record);
        Assert.IsNotNull(module);
        Assert.IsNotNull(plain);

        var hasDirectMarker = typeof(Util).GetMethod(
            "HasDirectECMAScriptSupportMarker",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var isRuntimeMarker = typeof(Util).GetMethod(
            "IsRuntimeMarkerType",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var isModule = typeof(Util).GetMethod(
            "IsECMAScriptModuleType",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var isRecordProxyProperty = typeof(Util).GetMethod(
            "IsECMAScriptRecordProxyProperty",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var isRecordProxyMethod = typeof(Util).GetMethod(
            "IsECMAScriptRecordProxyMethod",
            BindingFlags.Static | BindingFlags.NonPublic)!;

        Assert.IsFalse(InvokePrivateStatic<bool>(hasDirectMarker, new object?[] { null }));
        Assert.IsTrue(InvokePrivateStatic<bool>(hasDirectMarker, record));
        Assert.IsFalse(InvokePrivateStatic<bool>(hasDirectMarker, plain));
        Assert.IsFalse(InvokePrivateStatic<bool>(isRuntimeMarker, new object?[] { null }));
        Assert.IsTrue(InvokePrivateStatic<bool>(isRuntimeMarker, record));
        Assert.IsFalse(InvokePrivateStatic<bool>(isRuntimeMarker, module));
        Assert.IsFalse(InvokePrivateStatic<bool>(isModule, new object?[] { null }));
        Assert.IsTrue(InvokePrivateStatic<bool>(isModule, module));
        Assert.IsFalse(InvokePrivateStatic<bool>(isModule, plain));

        var plainProperty = record!.GetMembers("Plain").OfType<IPropertySymbol>().Single();
        var namedProperty = record.GetMembers("Named").OfType<IPropertySymbol>().Single();
        var boundaryProperty = record.GetMembers("Boundary").OfType<IPropertySymbol>().Single();
        var indexer = record.GetMembers().OfType<IPropertySymbol>().Single(static property => property.IsIndexer);
        Assert.IsFalse(InvokePrivateStatic<bool>(isRecordProxyProperty, plainProperty));
        Assert.IsTrue(InvokePrivateStatic<bool>(isRecordProxyProperty, namedProperty));
        Assert.IsTrue(InvokePrivateStatic<bool>(isRecordProxyProperty, indexer));
        Assert.IsTrue(InvokePrivateStatic<bool>(isRecordProxyMethod, record.GetMembers("Inline").OfType<IMethodSymbol>().Single()));
        Assert.IsFalse(InvokePrivateStatic<bool>(isRecordProxyMethod, record.GetMembers("Ordinary").OfType<IMethodSymbol>().Single()));

        var metadata = Util.GetJavaScriptNameMetadata(record);
        Assert.IsFalse(metadata.HasECMAScriptNameAttribute);
        Assert.IsFalse(metadata.HasDescriptionBoundary);
        Assert.AreEqual("runtime-boundary", metadata.DescriptionName);

        var namedMetadata = Util.GetJavaScriptNameMetadata(namedProperty);
        Assert.IsTrue(namedMetadata.HasECMAScriptNameAttribute);
        Assert.AreEqual("named", namedMetadata.ECMAScriptName);

        var boundaryMetadata = Util.GetJavaScriptNameMetadata(boundaryProperty);
        Assert.IsTrue(boundaryMetadata.HasDescriptionBoundary);
        Assert.IsNull(boundaryMetadata.DescriptionName);

        var importedGetter = record.GetMembers("Imported").OfType<IPropertySymbol>().Single().GetMethod;
        Assert.IsNotNull(importedGetter);
        Assert.IsTrue(Util.TryGetJazorImportMapping(importedGetter!, out var memberName, out var runtimeName));
        Assert.AreEqual("mapped-member", memberName);
        Assert.AreEqual("mapped-runtime", runtimeName);
        var importedWithoutRuntimeGetter = record.GetMembers("ImportedWithoutRuntime")
            .OfType<IPropertySymbol>()
            .Single()
            .GetMethod;
        Assert.IsNotNull(importedWithoutRuntimeGetter);
        Assert.IsTrue(Util.TryGetJazorImportMapping(importedWithoutRuntimeGetter!, out memberName, out runtimeName));
        Assert.AreEqual("mapped-without-runtime", memberName);
        Assert.AreEqual(string.Empty, runtimeName);

        var invalidImportGetter = record.GetMembers("InvalidImport")
            .OfType<IPropertySymbol>()
            .Single()
            .GetMethod;
        Assert.IsNotNull(invalidImportGetter);
        Assert.IsFalse(Util.TryGetJazorImportMapping(invalidImportGetter!, out memberName, out runtimeName));
        Assert.AreEqual(string.Empty, memberName);
        Assert.AreEqual(string.Empty, runtimeName);

        Assert.IsTrue(InvokePrivateStatic<bool>(isRecordProxyMethod, record.GetMembers("NamedMethod").OfType<IMethodSymbol>().Single()));
        Assert.IsTrue(InvokePrivateStatic<bool>(isRecordProxyMethod, record.GetMembers("ExternMethod").OfType<IMethodSymbol>().Single()));
        Assert.IsFalse(InvokePrivateStatic<bool>(isRecordProxyMethod, record.GetMembers("BlankInline").OfType<IMethodSymbol>().Single()));
        Assert.IsFalse(Util.IsECMAScriptRecordProxyMember(null));
        Assert.IsTrue(Util.IsECMAScriptRecordProxyMember(
            record.GetMembers("NamedMethod").OfType<IMethodSymbol>().Single(),
            record));

        var runtimeModule = compilation.GetTypeByMetadataName("RuntimeModule")!;
        var moduleReadInt = runtimeModule.GetMembers("Read")
            .OfType<IMethodSymbol>()
            .Single(method => method.Parameters[0].Type.SpecialType == SpecialType.System_Int32);
        var moduleReadString = runtimeModule.GetMembers("Read")
            .OfType<IMethodSymbol>()
            .Single(method => method.Parameters[0].Type.SpecialType == SpecialType.System_String);
        var overloadedHost = compilation.GetTypeByMetadataName("OverloadedHost")!;
        var hostReadInt = overloadedHost.GetMembers("Read")
            .OfType<IMethodSymbol>()
            .Single(method => method.Parameters[0].Type.SpecialType == SpecialType.System_Int32);
        var plainRecord = compilation.GetTypeByMetadataName("PlainRecord")!;
        var plainRecordValue = plainRecord.GetMembers("Value").OfType<IPropertySymbol>().Single();
        var plainRecordOrdinary = plainRecord.GetMembers("Ordinary").OfType<IMethodSymbol>().Single();
        Assert.AreEqual("Read", Util.GetConfigOrSymbolName(moduleReadInt));
        Assert.AreEqual("readText", Util.GetConfigOrSymbolName(moduleReadString));
        Assert.AreNotEqual(
            "Read",
            Util.GetConfigOrSymbolName(hostReadInt),
            "Unconfigured ordinary overloads must keep a collision suffix.");

        Assert.AreEqual("Value", Util.GetConfigOrSymbolName(plainRecordValue));
        Assert.AreEqual("Ordinary", Util.GetConfigOrSymbolName(plainRecordOrdinary));
        Assert.IsFalse(Util.TryGetJazorImportMapping(plainRecordOrdinary, out memberName, out runtimeName));
        Assert.AreEqual(string.Empty, memberName);
        Assert.AreEqual(string.Empty, runtimeName);
        Assert.IsFalse(InvokePrivateStatic<bool>(isRecordProxyMethod, plainRecordOrdinary));
        Assert.IsFalse(InvokePrivateStatic<bool>(isRecordProxyMethod, plainRecordValue.GetMethod!));
        Assert.IsFalse(Util.IsECMAScriptRecordProxyMember(plainRecordOrdinary, plainRecord));
        var emptyMetadata = Util.GetJavaScriptNameMetadata(plainRecordValue);
        Assert.IsFalse(emptyMetadata.HasECMAScriptNameAttribute);
        Assert.IsNull(emptyMetadata.DescriptionName);
    }

    [TestMethod]
    public void ReferenceHelpers_CoverNonRuntimeAndNonNestedBoundaryPaths()
    {
        var compilation = CreateCompilation(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class | global::System.AttributeTargets.Method)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public static class RuntimeStatics
            {
                public static int Read() => 1;
            }

            public sealed class ParamsHost
            {
                public void Send(params int[] values)
                {
                }
            }

            public sealed class Holder
            {
                public int Property { get; set; }

                public int Read(int value)
                    => value;
            }
            """);
        var walker = new SemanticWalker(true);
        var intType = compilation.GetSpecialType(SpecialType.System_Int32);
        var arrayType = compilation.CreateArrayTypeSymbol(intType);
        var shouldFlatten = typeof(SemanticWalker).GetMethod(
            "ShouldFlattenRuntimeNestedType",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        Assert.IsFalse(InvokePrivateStatic<bool>(shouldFlatten, arrayType));

        var buildFullTypeName = typeof(SemanticWalker).GetMethod(
            "BuildFullTypeName",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        Assert.IsNull(buildFullTypeName.Invoke(walker, [arrayType, null]));

        var holder = compilation.GetTypeByMetadataName("Holder")!;
        var property = holder.GetMembers("Property").OfType<IPropertySymbol>().Single();
        var isCurrentModuleIndexer = typeof(SemanticWalker).GetMethod(
            "IsCurrentModuleRuntimeIndexer",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        Assert.IsFalse(InvokePrivateInstance<bool>(isCurrentModuleIndexer, walker, property));

        var normalizeReceiver = typeof(SemanticWalker).GetMethod(
            "NormalizeRuntimeReceiverHostCallee",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var readMethod = holder.GetMembers("Read").OfType<IMethodSymbol>().Single();
        var identifier = new Acornima.Ast.Identifier("value");
        Assert.AreSame(
            identifier,
            InvokePrivateInstance<Acornima.Ast.Expression>(normalizeReceiver, walker, identifier, readMethod));

        var paramsHost = compilation.GetTypeByMetadataName("ParamsHost")!;
        var sendMethod = paramsHost.GetMembers("Send").OfType<IMethodSymbol>().Single();
        var expandParams = typeof(SemanticWalker)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(method => method.Name == "TryExpandEcmascriptParamsArgument" &&
                method.GetParameters().Length == 4 &&
                method.GetParameters()[1].ParameterType == typeof(IParameterSymbol));
        var expanded = new List<Acornima.Ast.Expression>();
        Assert.IsFalse(expandParams.Invoke(null, [sendMethod, sendMethod.Parameters[0], identifier, expanded]) is true);
        Assert.IsEmpty(expanded);

        var toString = intType.GetMembers("ToString")
            .OfType<IMethodSymbol>()
            .Single(method => method.Parameters.Length == 1 &&
                method.Parameters[0].Type.SpecialType == SpecialType.System_String);
        var hexIntrinsic = typeof(SemanticWalker).GetMethod(
            "TryBuildIntegerHexToStringIntrinsic",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var hexArguments = new object?[]
        {
            toString,
            new Acornima.Ast.Identifier("value"),
            new List<Acornima.Ast.Expression> { JavaScriptAstFactory.CreateStringLiteral("D") },
            null
        };
        Assert.IsFalse((bool)hexIntrinsic.Invoke(null, hexArguments)!);
        Assert.IsNull(hexArguments[3]);

        var enumerableIntrinsic = typeof(SemanticWalker).GetMethod(
            "TryBuildEnumerableArrayLikeIntrinsic",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var enumerableArguments = new object?[]
        {
            sendMethod,
            new List<Acornima.Ast.Expression> { identifier },
            arrayType,
            new SenseArgument(),
            null
        };
        Assert.IsFalse((bool)enumerableIntrinsic.Invoke(walker, enumerableArguments)!);
        Assert.IsNull(enumerableArguments[4]);

        var preferredStatic = typeof(SemanticWalker).GetMethod(
            "TryBuildPreferredRuntimeStaticMemberAccess",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var syntax = readMethod.DeclaringSyntaxReferences.Single().GetSyntax();
        var semanticModel = compilation.GetSemanticModel(compilation.SyntaxTrees.Single());
        var preferredArguments = new object?[]
        {
            readMethod,
            syntax,
            semanticModel,
            "Read",
            null,
            null
        };
        Assert.IsFalse((bool)preferredStatic.Invoke(walker, preferredArguments)!);
        Assert.IsNull(preferredArguments[5]);

        var runtimeStatics = compilation.GetTypeByMetadataName("RuntimeStatics")!;
        var runtimeStaticMethod = runtimeStatics.GetMembers("Read").OfType<IMethodSymbol>().Single();
        var extensionHost = typeof(SemanticWalker).GetMethod(
            "TryBuildExtensionHostTarget",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        Assert.IsNotNull(extensionHost.Invoke(walker, [runtimeStaticMethod, null]));
    }

    [TestMethod]
    public void ConvertRuntimeClass_ImplicitDerivedConstructorAndAccessors_PreserveRuntimeClassProtocol()
    {
        var compilation = CreateCompilation(
            """
            public static class ModuleHost
            {
                public class Base
                {
                    protected int Read() => 7;
                }

                public sealed class Derived : Base
                {
                    private int _value = 3;

                    public int Value
                    {
                        get => _value + Read();
                        set => _value = value;
                    }

                    public int this[int index]
                    {
                        get => Value + index;
                        set => Value = value - index;
                    }
                }
            }
            """);
        var syntaxTree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(syntaxTree);
        var module = compilation.GetTypeByMetadataName("ModuleHost");
        var derived = compilation.GetTypeByMetadataName("ModuleHost+Derived");
        Assert.IsNotNull(module);
        Assert.IsNotNull(derived);

        var declaration = new AstConverter(module!, model).ConvertRuntimeClass(derived!);
        var script = declaration.ToKnRECMAScript();

        StringAssert.Contains(script, "extends Base", StringComparison.Ordinal);
        StringAssert.Contains(script, "constructor()", StringComparison.Ordinal);
        StringAssert.Contains(script, "super()", StringComparison.Ordinal);
        StringAssert.Contains(script, "$get_", StringComparison.Ordinal);
        StringAssert.Contains(script, "$set_", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    private static string VisitBlock(IBlockOperation block)
    {
        var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first);
        Assert.AreEqual(first, second);
        _ = new Parser().ParseScript(first);
        return first;
    }

    private static IBlockOperation CreateBlock(string source)
    {
        var compilation = CreateCompilation(source);
        var syntaxTree = compilation.SyntaxTrees.Single();
        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(
            compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }

    private static string CreateErasedUnionCreationSource(string creation)
        => $$"""
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class | global::System.AttributeTargets.Struct, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            namespace System.Runtime.CompilerServices
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Struct, Inherited = false)]
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
            public readonly struct Choice : System.Runtime.CompilerServices.IUnion
            {
                public Choice(int value)
                {
                }

                public Choice(int first, int second)
                {
                }

                public object? Value => null;

                public int State { get; init; }
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var value = {{creation}};
                }
            }
            """;

    private static T InvokePrivateStatic<T>(MethodInfo method, params object?[] arguments)
        => Assert.IsInstanceOfType<T>(method.Invoke(null, arguments));

    private static T InvokePrivateInstance<T>(MethodInfo method, object instance, params object?[] arguments)
        => Assert.IsInstanceOfType<T>(method.Invoke(instance, arguments));

    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "CompilerLoweringCoverage_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));
        return compilation;
    }

    private sealed class InitializerInstanceProjectionHost : SemanticWalkerHost
    {
        public override Acornima.Ast.Expression? RewriteInstanceReference(
            IInstanceReferenceOperation operation,
            SenseArgument argument)
            => new Acornima.Ast.Identifier("receiver");
    }

    private sealed class OuterMemberInitializerProjectionHost : SemanticWalkerHost
    {
        public override Acornima.Ast.Expression? RewriteInstanceReference(
            IInstanceReferenceOperation operation,
            SenseArgument argument)
            => operation.Parent is IPropertyReferenceOperation { Property.Name: "Child" }
                ? new Acornima.Ast.Identifier("owner")
                : null;
    }

    private sealed class AllVariableDeclaratorsHost : SemanticWalkerHost
    {
        public override bool ShouldSkipVariableDeclarator(
            IVariableDeclaratorOperation operation,
            SenseArgument argument)
            => true;
    }

    private sealed class NoVariableDeclaratorsHost : SemanticWalkerHost
    {
        public override bool ShouldSkipVariableDeclarator(
            IVariableDeclaratorOperation operation,
            SenseArgument argument)
            => false;
    }
}
