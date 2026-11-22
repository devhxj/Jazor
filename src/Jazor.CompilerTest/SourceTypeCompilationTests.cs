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
    public async Task Convert_UndefinedArgumentPreservesLaterArgument()
    {
        var script = await Convert("""
            using ECMAScript;

            [ECMAScript]
            public static class Host
            {
                [ECMAScriptUndefinedArgument(1)]
                [ECMAScriptInline("__arg2 === void 0 ? __arg1 + __arg3 : -1")]
                public static extern int Sum(int first, int last);
            }

            public static class TestModule
            {
                public static int Run() => Host.Sum(4, 7);
            }
            """);
        await AssertRun(script, 11);
    }

    [TestMethod]
    public async Task Convert_KeyedRefRejectsFieldsRequiringReadNormalization()
    {
        const string source = """
            using ECMAScript;

            [ECMAScript]
            public static class Host
            {
                [ECMAScriptInline("__arg1")]
                public static extern Vue.VueComputedRef<TValue> Lens<T, TValue>(
                    T source, [ECMAScriptPropertyPath(true, IndirectRead = true)] System.Func<T, TValue> key);

                [ECMAScriptInline("__arg1[__arg2] = __arg3")]
                public static extern void Set<T, TValue>(
                    T source, [ECMAScriptPropertyPath(true)] System.Func<T, TValue> key, TValue value);
            }

            public static class TestModule
            {
                public sealed record Data(
                    double Regular,
                    [property: ECMAScriptRequiredRead] double Guarded,
                    [property: ECMAScriptOptional] double? Optional);

                public static double Run(Data source)
                {
                    Host.Set(source, value => value.Guarded, 0d);
                    return Host.Lens(source, value => value.Regular).Value;
                }
            }
            """;
        var script = await Convert(source);
        StringAssert.Contains(script, "Guarded");
        foreach (var (field, diagnostic) in new[] {
            ("Guarded", "Read 'Guarded' directly"),
            ("Optional", "Read optional property 'Optional' directly"),
        })
        {
            var invalid = source.Replace("public static double Run", "public static double? Run")
                .Replace("value => value.Regular", "value => value." + field);
            var exception = await Assert.ThrowsAsync<OperationTransformationException>(() => Convert(invalid));
            StringAssert.Contains(exception.Message, diagnostic);
        }
    }

    [TestMethod]
    [DataRow("row => row.Nested.Alias", "\"nested.wire\"")]
    [DataRow("row => row.Alias", "\"wire\"")]
    [DataRow("row => row.Nested.Fallback", "\"nested.Fallback\"")]
    [DataRow("row => row.Dot", "\"[first.last]\"")]
    [DataRow("row => row.Brackets", "\"item[text]\"")]
    [DataRow("row => row.Index", "cannot be represented")]
    [DataRow("row => row.Bracketed", "cannot be represented")]
    [DataRow("row => row.DotBracket", "cannot be represented")]
    [DataRow("row => row.DotClosingBracket", "cannot be represented")]
    [DataRow("row => row.DotNewline", "cannot be represented")]
    [DataRow("row => row.Nested.Dot", "segments must be nonempty")]
    [DataRow("row => row.Nested.Brackets", "segments must be nonempty")]
    [DataRow("row => row.Nested.ClosingBracket", "segments must be nonempty")]
    [DataRow("row => row.Calculated", "requires a lambda")]
    [DataRow("row => other.Alias", "requires a lambda")]
    [DataRow("Selector", "requires a lambda")]
    [DataRow("row => { var selected = row.Alias; return selected; }", "requires a lambda")]
    public async Task Convert_FormFieldSelector_EncodesLiteralAndNestedPaths(string selector, string expected)
    {
        var source = $$"""
            using System;
            using System.ComponentModel;
            using ECMAScript;

            [ECMAScript]
            public static class Host
            {
                [ECMAScriptInline("__arg2")]
                public static extern string Path<T, TValue>(
                    T source, [ECMAScriptPropertyPath(VeeValidatePath = true)] Func<T, TValue> key);
            }

            public static class TestModule
            {
                public sealed record Data(
                    [property: Description("@#nested")] Data Nested,
                    [property: Description("@#wire")] string Alias,
                    [property: Description("@#first.last")] string Dot,
                    [property: Description("@#item[text]")] string Brackets,
                    [property: Description("@#item[0]")] string Index,
                    [property: Description("@#[item]")] string Bracketed,
                    [property: Description("@#a.b[x")] string DotBracket,
                    [property: Description("@#a.b]")] string DotClosingBracket,
                    [property: Description("@#a.b\nc")] string DotNewline,
                    [property: Description("@#item]")] string ClosingBracket,
                    [property: Description("@#")] string Fallback)
                {
                    public string Calculated => Alias;
                }

                private static string Selector(Data row) => row.Alias;
                public static string Run(Data row, Data other) => Host.Path(row, {{selector}});
            }
            """;
        if (expected.StartsWith('"'))
        {
            var script = await Convert(source);
            StringAssert.Contains(script, expected);
        }
        else
        {
            var exception = await Assert.ThrowsAsync<OperationTransformationException>(() => Convert(source));
            StringAssert.Contains(exception.Message, expected);
        }
    }

    [TestMethod]
    [DataRow("Plain", null)]
    [DataRow("WithField", "Value.Number")]
    [DataRow("WithComputed", "Value.Number")]
    [DataRow("WithRef", "Value.Number")]
    [DataRow("WithOptional", "Value.Number")]
    [DataRow("WithRequired", "Value.Number")]
    [DataRow("WithProxyKey", "Value.Number")]
    [DataRow("WithProto", "Value.Number")]
    [DataRow("WithRaw", "Value.Number")]
    [DataRow("WithShallow", "Value.Number")]
    [DataRow("WithReactive", "Value.Number")]
    [DataRow("WithReadonly", "Value.Number")]
    [DataRow("WithSkip", "Value.Number")]
    [DataRow("WithNullableNested", "Value.Nested")]
    [DataRow("Recursive", "Value.Next")]
    [DataRow("OpenRecord", "Value")]
    [DataRow("Generic<double>", "Value")]
    [DataRow("StructRecord", "Value")]
    [DataRow("Derived", "Value")]
    [DataRow("double", "Value")]
    public async Task Convert_PassiveModelSelector_RequiresPlainRecord(string payload, string? invalidMember)
    {
        var source = $$"""
            #nullable enable
            using System;
            using System.ComponentModel;
            using ECMAScript;

            [ECMAScript]
            public static class Host
            {
                [ECMAScriptInline("__arg2")]
                public static extern string Path<T, TValue>(
                    T source, [ECMAScriptPropertyPath(true, RequirePlainRecord = true)] Func<T, TValue> key);
            }

            public static class TestModule
            {
                public sealed record Leaf(string Text, double Number, bool Enabled);
                public sealed record Plain(Leaf First, Leaf Second)
                {
                    public const string Kind = "plain";
                }
                public sealed record WithField { public double Number; }
                public sealed record WithComputed(double Initial) { public double Number => Initial; }
                public sealed record WithRef(Vue.IVueRef<double> Number);
                public sealed record WithOptional([property: ECMAScriptOptional] double? Number);
                public sealed record WithRequired([property: ECMAScriptRequiredRead] double Number);
                public sealed record WithProxyKey([property: Description("@#__v_isRef")] bool Number);
                public sealed record WithProto([property: Description("@#__proto__")] bool Number);
                public sealed record WithRaw([property: Description("@#__v_raw")] bool Number);
                public sealed record WithShallow([property: Description("@#__v_isShallow")] bool Number);
                public sealed record WithReactive([property: Description("@#__v_isReactive")] bool Number);
                public sealed record WithReadonly([property: Description("@#__v_isReadonly")] bool Number);
                public sealed record WithSkip([property: Description("@#__v_skip")] bool Number);
                public sealed record WithNullableNested(Leaf? Nested);
                public sealed record Recursive { public Recursive Next { get; init; } = default!; }
                public record OpenRecord;
                public sealed record Derived : OpenRecord;
                public sealed record Generic<T>(T Item);
                public readonly record struct StructRecord(double Item);
                public sealed record Data({{payload}} Value);

                public static string Run(Data row) => Host.Path(row, item => item.Value);
            }
            """;
        if (invalidMember is null)
        {
            var script = await Convert(source);
            StringAssert.Contains(script, "\"Value\"");
        }
        else
        {
            var exception = await Assert.ThrowsAsync<OperationTransformationException>(() => Convert(source));
            StringAssert.Contains(exception.Message, $"Passive Vue model property '{invalidMember}'");
        }
    }

    [TestMethod]
    [DataRow("Host.Get(row, x => x.Child.Text)", "exactly one structural record data property")]
    [DataRow("Host.Get(row, x => x.Required)", "Read 'Required' directly")]
    [DataRow("Host.Get<Row, object>(row, x => x.Text)", "but the call expects")]
    [DataRow("Host.Set(row, x => x.Text, missing!)", null)]
    [DataRow("Host.Set<Row, string>(row, x => x.Text, missing)", "requires a non-null value")]
    [DataRow("Host.Set(row, x => x.Text, \"value\")", null)]
    [DataRow("Host.Set(row, x => x.Optional, missing)", null)]
    [DataRow("Host.Get(row, x => x.Optional)", null)]
    [DataRow("Host.Get(row, x => x.Text)", null)]
    [DataRow("Host.GetText(row, x => x.Text)", null)]
    [DataRow("Host.GetOther(row, x => x.Optional)", null)]
    [DataRow("Host.Get<Row, string>", "requires a direct call")]
    public async Task Convert_RecordKeySelector_PreservesSelectedTypeAndNullability(string call, string? error)
    {
        var source = $$"""
            #nullable enable
            using System;
            using ECMAScript;

            [ECMAScript]
            public static class Host
            {
                [ECMAScriptInline("__arg1[__arg2]")]
                public static extern TValue Get<T, TValue>(
                    T source, [ECMAScriptPropertyPath(true, IndirectRead = false)] Func<T, TValue> key);

                [ECMAScriptInline("__arg2")]
                public static extern string GetText(TestModule.Row source,
                    [ECMAScriptPropertyPath(true)] Func<TestModule.Row, string> key);

                [ECMAScriptInline("__arg2")]
                public static extern string GetOther<T, TValue>(
                    T source, [ECMAScriptPropertyPath(true)] Func<T, TValue> key);

                [ECMAScriptInline("__arg1[__arg2] = __arg3")]
                public static extern void Set<T, TValue>(
                    T source, [ECMAScriptPropertyPath(true)] Func<T, TValue> key, TValue value);
            }

            public static class TestModule
            {
                public sealed record Row(
                    string Text, Row Child,
                    [property: ECMAScriptOptional] string? Optional,
                    [property: ECMAScriptRequiredRead] string Required);

                public static void Run(Row row, string? missing)
                {
                    {{(call == "Host.Get<Row, string>" ? "Func<Row, Func<Row, string>, string> read = " : "")}}{{call}};
                }
            }
            """;
        if (error is null)
        {
            var script = await Convert(source);
            StringAssert.Contains(script, call.Contains("Optional") ? "Optional" : "Text");
            if (call == "Host.Get(row, x => x.Optional)")
                StringAssert.Contains(script, "?? null");
        }
        else
        {
            var exception = await Assert.ThrowsAsync<OperationTransformationException>(() => Convert(source));
            StringAssert.Contains(exception.Message, error);
        }
    }

    [TestMethod]
    [DataRow("Action<Row>", "row => { return; }")]
    [DataRow("Func<Row, Row, string>", "(left, right) => left.Text")]
    [DataRow("Func<string>", "() => \"key\"")]
    [DataRow("Func<Row, string>", "row => throw new Exception()")]
    public async Task Convert_RecordSelectorRejectsDelegatesWithoutOneFieldResult(string callback, string selector)
    {
        var source = $$"""
            using System;
            using ECMAScript;

            [ECMAScript]
            public static class Host
            {
                [ECMAScriptInline("__arg1")]
                public static extern string Path([ECMAScriptPropertyPath] {{callback}} key);
            }

            public sealed record Row(string Text);

            public static class TestModule
            {
                public static string Run() => Host.Path({{selector}});
            }
            """;
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(() => Convert(source));
        StringAssert.Contains(exception.Message, "requires a lambda selecting structural record data properties");
    }

    [TestMethod]
    public async Task Convert_FixedBooleanArgumentsPreserveTrueAndFalse()
    {
        var script = await Convert("""
            using ECMAScript;

            [ECMAScript]
            public static class Host
            {
                [ECMAScriptInline("__arg1 && !__arg2 ? __arg3 : -1")]
                [ECMAScriptFixedBooleanArgument(0, true)]
                [ECMAScriptFixedBooleanArgument(1, false)]
                public static extern int Select(int value);
            }

            public static class TestModule
            {
                public static int Run() => Host.Select(7);
            }
            """);
        await AssertRun(script, 7);
    }

    [TestMethod]
    public async Task Convert_FixedSlotKeepsAnOmittedOptionalArgumentAndAdaptsMethodGroup()
    {
        var script = await Convert("""
            using System;
            using ECMAScript;

            [ECMAScript]
            public class Host
            {
                [ECMAScriptFixedBooleanArgument(2, false)]
                public extern int Select(int value, [ECMAScriptUndefinedDefault] int? offset = null);
            }

            public static class TestModule
            {
                public static int Run(Host host)
                {
                    Func<int, int?, int> select = host.Select;
                    return host.Select(7) + select(8, 0);
                }
            }
            """);
        StringAssert.Contains(script, "host.Select(7, void 0, false)");
        StringAssert.Contains(script, "__jz_optionalCall(__jz_fixedArg0, __jz_fixedArg1, false)");
        StringAssert.Contains(script, ".bind(host)");
    }

    [TestMethod]
    public async Task Convert_SelectorConstructor_UsesBoundKeyWithoutAnInvocationResult()
    {
        var script = await Convert("""
            using System;
            using ECMAScript;

            [ECMAScript]
            public class Host
            {
                public extern Host([ECMAScriptPropertyPath(true, IndirectRead = false)] Func<Data, double> key);
            }

            public sealed record Data(double Count);

            public static class TestModule
            {
                public static Host Run() => new(value => value.Count);
            }
            """);
        StringAssert.Contains(script, """new Host("Count")""");
    }

    [TestMethod]
    public async Task Convert_RequiredStructuralReadChecksUndefinedAndKeepsWritesRaw()
    {
        var script = await Convert("""
            using ECMAScript;
            using System.ComponentModel;

            public static class TestModule
            {
                public static double Run()
                {
                    var box = new Box<double> { Value = 0 };
                    var reads = 0;
                    Box<double> Next() { reads++; return box; }
                    var first = Next().Value++;
                    Next().Value += 2;
                    var second = Next().Value;
                    var writes = new Box<double>();
                    writes.Value = 4;
                    Box<double>? absent = null;
                    var inherited = new Inherited<string>();
                    try { _ = inherited.Value; return -2; }
                    catch (System.Exception) { }
                    var presentNull = new Box<string?> { Value = null };
                    return (box.HasValue && !new Box<double>().HasValue && (absent?.Value ?? -1) == -1 &&
                        presentNull.HasValue && presentNull.Value == null)
                        ? first + second + writes.Value + reads : -1;
                }
            }

            [ECMAScript]
            public sealed record Box<T>
            {
                [Description("@#wire.key"), ECMAScriptRequiredRead, ECMAScriptExplicitNull]
                public T Value { get; set; }
                public extern bool HasValue { [ECMAScriptInline("Object.hasOwn(__arg1, \"wire.key\")")] get; }
            }

            [ECMAScript]
            public sealed record Inherited<T>
            {
                [Description("@#toString"), ECMAScriptRequiredRead]
                public T Value { get; set; }
            }
            """);
        StringAssert.Contains(script, "Required property 'wire.key' is undefined.");
        await AssertRun(script, 10);
    }

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

    [TestMethod]
    public async Task Convert_HostRecordOptionsKeepFixedBooleansAndExplicitNull()
    {
        var script = await Convert("""
            using ECMAScript;
            public record Options
            {
                [ECMAScriptFixedBoolean(true)]
                public bool Enabled => true;
                [ECMAScriptFixedBoolean(false)]
                public bool Disabled => false;
                [ECMAScriptExplicitNull]
                public string? Nullable { get; init; }
                public string? Omitted { get; init; }
            }
            public static class TestModule
            {
                public static Options Run() => new() { Nullable = null, Omitted = null };
            }
            """);

        StringAssert.Contains(script, "Enabled: true");
        StringAssert.Contains(script, "Disabled: false");
        StringAssert.Contains(script, "Nullable: null");
        Assert.DoesNotContain("Omitted", script, StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task Convert_OptionalHostMembersNormalizeReadsAndPreserveAssignmentTargets()
    {
        var script = await Convert("""
            using ECMAScript;
            using System;
            using System.ComponentModel;
            [ECMAScript]
            public class Host
            {
                [ECMAScriptOptional]
                public extern int? Value { get; set; }
                [ECMAScriptOptional]
                [Description("@#")]
                public extern string? this[string key] { get; set; }
                [ECMAScriptOptional]
                public extern string? Read(string key);
                public extern string Call([ECMAScriptUndefinedDefault] string? kind = null, string key = "");
                public extern string Ordered(string first, [ECMAScriptUndefinedDefault] string? middle = null, string last = "", string trailing = "unused");
            }
            public static class TestModule
            {
                public static bool Run(Host host, string key)
                {
                    var missing = host.Value == null && host[key] == null && host.Read(key) == null;
                    Func<string, string?> read = host.Read;
                    host.Value = 2;
                    host.Value++;
                    host.Value = host.Value;
                    host[key] = "present";
                    return missing && read(key) == null && host.Call(key: key) == ""
                        && host.Ordered(last: key, first: "first") == "";
                }
            }
            """);

        StringAssert.Contains(script, """host["Value"] ?? null""");
        StringAssert.Contains(script, "host[key] ?? null");
        StringAssert.Contains(script, "host.Read(key) ?? null");
        StringAssert.Contains(script, "host.Value = 2");
        StringAssert.Contains(script, """host.Value = host["Value"] ?? null""");
        StringAssert.Contains(script, "host[key] = \"present\"");
        StringAssert.Contains(script, "void 0");
        StringAssert.Contains(script, "__jz_optionalArgs");
        Assert.DoesNotContain("unused", script, StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task Convert_OptionalCallbackAdaptsMissingValueInDirectCallsAndMethodGroups()
    {
        var script = await Convert("""
            using ECMAScript;
            using System;
            [ECMAScript]
            public class Store
            {
                [ECMAScriptOptional]
                public extern double? Update(string key, [ECMAScriptOptionalCallback] Func<double?, double?> write);
            }
            public static class TestModule
            {
                public static double? Direct(Store store, Func<Func<double?, double?>> supply) =>
                    store.Update(write: supply(), key: "key");
                public static Func<string, Func<double?, double?>, double?> Capture(Store store) => store.Update;
            }
            """);

        Assert.AreEqual(2, script.Split("$callback($value ?? null) ?? void 0").Length - 1, script);
        Assert.AreEqual(1, script.Split("supply()").Length - 1, script);
        StringAssert.Contains(script, ".bind(store)");
    }

    [TestMethod]
    public async Task Convert_OptionalImportedPropertyNormalizesUndefined()
    {
        var script = await Convert("""
            using ECMAScript;
            [ECMAScript("host-values")]
            public static class Host
            {
                [ECMAScriptName("optionalValue")]
                [ECMAScriptOptional]
                public static extern string? Value { get; }
            }
            public static class TestModule
            {
                public static bool Run() => Host.Value == null;
            }
            """);

        StringAssert.Contains(script, "optionalValue");
        StringAssert.Contains(script, "?? null");
    }

    [TestMethod]
    public async Task Convert_HostArgumentRewriteStillWritesBackRefResult()
    {
        var script = await Convert("""
            using ECMAScript;
            [ECMAScript]
            public static class Host
            {
                public static extern bool TryRead(ref int value);
            }
            public static class TestModule
            {
                public static int Run()
                {
                    var value = 1;
                    Host.TryRead(ref value);
                    return value;
                }
            }
            """, new AstConverterOptions(AstConverterProfile.Standard, Host: new RefArgumentHost()));

        StringAssert.Contains(script, "Host.TryRead(value)");
        StringAssert.Contains(script, "[1]");
        StringAssert.Contains(script, "return value");
    }

    [TestMethod]
    public async Task Convert_NamedHostRefAndOutArgumentsKeepWritebackOrder()
    {
        var script = await Convert("""
            using ECMAScript;
            [ECMAScript]
            public static class Host
            {
                public static extern void Exchange(out int first, ref int second);
            }
            public static class TestModule
            {
                public static int Run()
                {
                    var second = 7;
                    Host.Exchange(second: ref second, first: out var first);
                    return first * 10 + second;
                }
            }
            """);

        StringAssert.Contains(script, "Host.Exchange(");
        StringAssert.Contains(script, "first = ");
        StringAssert.Contains(script, "second = ");
        StringAssert.Contains(script, "[0]");
        StringAssert.Contains(script, "[1]");
    }

    [TestMethod]
    public async Task Convert_InlineDeconstructUsesHostTupleWithoutCallingAMethod()
    {
        var script = await Convert("""
            using ECMAScript;
            [ECMAScript]
            public sealed class HostTuple
            {
                [ECMAScriptInline("__arg1")]
                public extern void Deconstruct(out int first, out int second);
            }
            public static class TestModule
            {
                public static int Run(HostTuple tuple)
                {
                    var (first, second) = tuple;
                    return first + second;
                }
            }
            """);

        Assert.DoesNotContain(".Deconstruct(", script, StringComparison.Ordinal);
        StringAssert.Contains(script, "[0]");
        StringAssert.Contains(script, "[1]");
    }

    [TestMethod]
    public async Task Convert_PartialRecordReadsSingleAndNestedPaths()
    {
        var script = await Convert("""
            using ECMAScript;
            using System;
            public record Detail([property: ECMAScriptName("item-count")] int Count);
            public record Shape(string Name, int? Score, Detail? Child) : Vue.VueProps;
            public static class TestModule
            {
                public static bool Run(Func<Vue.PartialRecord<Shape>> snapshot) =>
                    snapshot().Get(value => value.Name) == null &&
                    snapshot().GetValue(value => value.Score) == null &&
                    snapshot().GetValue(value => value.Child!.Count) == null;
            }
            """);

        StringAssert.Contains(script, """snapshot()["Name"] ?? null""");
        StringAssert.Contains(script, """snapshot()["Score"] ?? null""");
        StringAssert.Contains(script, """snapshot()["Child"]?.["item-count"] ?? null""");
    }

    [TestMethod]
    [DataRow("snapshot.GetValue(selector)", "selecting a record data property path")]
    [DataRow("snapshot.GetValue(value => 1)", "selecting a record data property path")]
    [DataRow("snapshot.GetValue(value => { var count = value.Score; return count; })", "selecting a record data property path")]
    [DataRow("snapshot.GetValue<long>(value => value.Score)", "selecting a record data property path")]
    [DataRow("snapshot.GetValue(value => value.Computed)", "selecting a record data property path")]
    [DataRow("snapshot.GetValue(value => other.Score)", "selecting a record data property path")]
    [DataRow("((Func<Func<Shape, int>, int?>)snapshot.GetValue<int>)(selector)", "direct call with a property selector")]
    public async Task Convert_PartialRecordRejectsSelectorsOutsideRecordPropertyContract(string expression, string message)
    {
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(() => Convert($$"""
            using ECMAScript;
            using System;
            public record Shape(int Score) : Vue.VueProps
            {
                public int Computed => Score + 1;
            }
            public static class TestModule
            {
                public static bool Run(Vue.PartialRecord<Shape> snapshot, Shape other, Func<Shape, int> selector)
                    => {{expression}} == null;
            }
            """));
        StringAssert.Contains(exception.Message, message);
    }

    private static async Task<string> Convert(string source, AstConverterOptions? options = null,
        Func<INamedTypeSymbol, AstConverterOptions>? createOptions = null, string moduleName = "TestModule")
    {
        var tree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions, "source-types.cs");
        var compilation = CSharpCompilation.Create("SourceTypes_" + Guid.NewGuid().ToString("N"), [tree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location)),
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

    private sealed class RefArgumentHost : SemanticWalkerHost
    {
        public override Expression? RewriteInvocationArgumentPreorder(
            IInvocationOperation operation, IArgumentOperation argumentOperation, int argumentIndex, SenseArgument argument)
            => operation.TargetMethod.Name == "TryRead" ? new Identifier("value") : null;
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
