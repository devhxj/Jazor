using Acornima;
using Acornima.Ast;
using DenoHost.Core;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class AstConverterRuntimeClassScenarioTests
{
    [TestMethod]
    public async Task ConvertModule_NullForgivingFieldInitializer_LowersUnderlyingDefaultValue()
    {
        const string scenarioId = "ast-converter-module.null-forgiving-field-initializer";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public static string RequiredName = default!;
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "requiredName = null", StringComparison.Ordinal, scenarioId);
        Assert.IsFalse(script.Contains("default", StringComparison.Ordinal), scenarioId);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public void ConvertRuntimeClass_NullForgivingAutoPropertyInitializer_LowersUnderlyingDefaultValue()
    {
        const string scenarioId = "ast-converter-runtime-class.null-forgiving-auto-property-initializer";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class ViewModel
                {
                    public string RequiredName { get; set; } = default!;
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var declaration = converter.ConvertRuntimeClass(fixture.GetType("ViewModel"));
        var script = declaration.ToKnRECMAScript();

        StringAssert.Contains(script, "class ViewModel", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "= null", StringComparison.Ordinal, scenarioId);
        Assert.IsFalse(script.Contains("default", StringComparison.Ordinal), scenarioId);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public async Task ConvertRuntimeClass_FieldBackedProperty_DeclaresParseablePrivateStorage()
    {
        const string scenarioId = "ast-converter-runtime-class.field-backed-property-private-storage";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class Counter
                {
                    public int Value
                    {
                        get;
                        set => field = value;
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var declaration = converter.ConvertRuntimeClass(fixture.GetType("Counter"));
        var script = declaration.ToKnRECMAScript();

        StringAssert.Contains(script, "get value()", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "set value(value)", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "this.#", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseScript(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-field-property-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "counter.mjs");
            var testPath = Path.Combine(root, "counter.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                "export " + script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { Counter } from "./counter.mjs";

                Deno.test("field-backed accessor stores and reads its assigned value", () => {
                  const counter = new Counter();
                  counter.value = 42;
                  if (counter.value !== 42)
                    throw new Error("field-backed property did not preserve the assigned value");
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void ConvertRuntimeClass_ModuleDeclaredBase_UsesExtendsAndSynthesizesSuper()
    {
        const string scenarioId = "ast-converter-runtime-class.module-base-implicit-super";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public class Base
                {
                    public Base()
                    {
                    }
                }

                public sealed class Derived : Base
                {
                }
            }
            """,
            scenarioId);
        var derived = fixture.GetType("Derived");
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var declaration = converter.ConvertRuntimeClass(derived);
        var script = declaration.ToKnRECMAScript();

        Assert.AreEqual("Derived", declaration.Id?.Name, scenarioId);
        StringAssert.Contains(script, "class Derived extends Base", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "super()", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void ConvertRuntimeClass_OverloadedModuleBase_PassesConstructorHelperSelector()
    {
        const string scenarioId = "ast-converter-runtime-class.overloaded-module-base";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public class Base
                {
                    public Base()
                    {
                    }

                    public Base(int mode)
                    {
                    }
                }

                public sealed class Derived : Base
                {
                }
            }
            """,
            scenarioId);
        var baseDefaultConstructor = fixture.GetType("Base").InstanceConstructors
            .Single(static constructor => !constructor.IsImplicitlyDeclared && constructor.Parameters.Length == 0);
        var derived = fixture.GetType("Derived");
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var declaration = converter.ConvertRuntimeClass(derived);
        var script = declaration.ToKnRECMAScript();
        var expectedHelper = Util.GetMemberConstructorHelperName(baseDefaultConstructor);

        Assert.AreEqual("Derived", declaration.Id?.Name, scenarioId);
        StringAssert.Contains(script, "class Derived extends Base", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, $"super(\"{expectedHelper}\")", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public async Task ConvertModule_ExplicitBaseInitializerWithOverloadedBase_PreservesBoundSelectorAndArguments()
    {
        const string scenarioId = "ast-converter-runtime-class.explicit-base-initializer-overloaded-base";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public class Base
                {
                    public int Value;

                    public Base(int seed)
                    {
                        Value = seed;
                    }

                    public Base(string label)
                    {
                        Value = 100;
                    }
                }

                public sealed class Derived : Base
                {
                    public Derived(int seed) : base(seed + 1)
                    {
                        Value += 10;
                    }
                }
            }
            """,
            scenarioId);
        var baseIntConstructor = fixture.GetType("Base").InstanceConstructors
            .Single(static constructor =>
                !constructor.IsImplicitlyDeclared &&
                constructor.Parameters.Length == 1 &&
                constructor.Parameters[0].Type.SpecialType == SpecialType.System_Int32);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();
        var expectedHelper = Util.GetMemberConstructorHelperName(baseIntConstructor);

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, $"super(\"{expectedHelper}\", seed + 1)", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-explicit-base-initializer-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "classes.mjs");
            var testPath = Path.Combine(root, "classes.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { Derived } from "./classes.mjs";

                Deno.test("explicit base initializer selects the Roslyn-bound overload", () => {
                  const derived = new Derived(4);
                  if (derived.value !== 15)
                    throw new Error(`expected the int base constructor and derived body to produce 15, got ${derived.value}`);
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_CompoundFromEndArrayMutation_EvaluatesMethodReceiverOnce()
    {
        const string scenarioId = "ast-converter-runtime-class.compound-from-end-array-mutation";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class Measurement
                {
                    public int[] Values = [3, 4];
                    public int Reads;

                    public int[] GetValues()
                    {
                        Reads++;
                        return Values;
                    }

                    public int IncreaseLast(int amount)
                    {
                        GetValues()[^1] += amount;
                        return Reads * 100 + Values[^1];
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "getValues()", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, ".length - 1", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-compound-array-mutation-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "measurement.mjs");
            var testPath = Path.Combine(root, "measurement.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { Measurement } from "./measurement.mjs";

                Deno.test("compound from-end array mutation evaluates the receiver once", () => {
                  const measurement = new Measurement();
                  const result = measurement.increaseLast(5);
                  if (result !== 109 || measurement.reads !== 1 || measurement.values[1] !== 9)
                    throw new Error(`expected one receiver call and [3, 9], got result=${result}, reads=${measurement.reads}, values=${measurement.values}`);
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_UsingDeclaration_DisposesResourcesInReverseOrderOnDenoHost()
    {
        const string scenarioId = "ast-converter-runtime-class.using-declaration-reverse-disposal";
        var fixture = CompileModule(
            """
            using System;

            public static class TestModule
            {
                public sealed class Resource : IDisposable
                {
                    private readonly Recorder owner;
                    private readonly string label;

                    public Resource(Recorder owner, string label)
                    {
                        this.owner = owner;
                        this.label = label;
                    }

                    public void Dispose()
                    {
                        owner.Append(label);
                    }
                }

                public sealed class Recorder
                {
                    private string trace = "";

                    public string Trace => trace;

                    public void Append(string part)
                    {
                        trace += part;
                    }

                    public void Run()
                    {
                        using var first = new Resource(this, "first:");
                        using var second = new Resource(this, "second:");
                        Append("body:");
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "try", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "finally", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-using-declaration-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "using-declaration.mjs");
            var testPath = Path.Combine(root, "using-declaration.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { Recorder } from "./using-declaration.mjs";

                Deno.test("using declarations release resources in reverse acquisition order", () => {
                  const recorder = new Recorder();
                  recorder.run();
                  if (recorder.trace !== "body:second:first:")
                    throw new Error(`expected body then reverse disposal, got ${recorder.trace}`);
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_AwaitUsingDeclaration_AwaitsReverseDisposalOnDenoHost()
    {
        const string scenarioId = "ast-converter-runtime-class.await-using-declaration-reverse-disposal";
        var fixture = CompileModule(
            """
            using System;
            using System.Threading.Tasks;

            public static class TestModule
            {
                public sealed class Resource : IAsyncDisposable
                {
                    private readonly Recorder owner;
                    private readonly string label;

                    public Resource(Recorder owner, string label)
                    {
                        this.owner = owner;
                        this.label = label;
                    }

                    public async ValueTask DisposeAsync()
                    {
                        owner.Append(label + "start:");
                        await Task.Yield();
                        owner.Append(label + "done:");
                    }
                }

                public sealed class Recorder
                {
                    private string trace = "";

                    public string Trace => trace;

                    public void Append(string part)
                    {
                        trace += part;
                    }

                    public async Task RunAsync()
                    {
                        await using var first = new Resource(this, "first:");
                        await using var second = new Resource(this, "second:");
                        Append("body:");
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "await second.disposeAsync()", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "await first.disposeAsync()", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-await-using-declaration-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "await-using-declaration.mjs");
            var testPath = Path.Combine(root, "await-using-declaration.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { Recorder } from "./await-using-declaration.mjs";

                Deno.test("await using waits for reverse-order asynchronous disposal", async () => {
                  const recorder = new Recorder();
                  const run = recorder.runAsync();
                  if (recorder.trace !== "body:second:start:")
                    throw new Error(`expected the second disposer to suspend RunAsync, got ${recorder.trace}`);

                  await run;
                  if (recorder.trace !== "body:second:start:second:done:first:start:first:done:")
                    throw new Error(`expected awaited reverse disposal, got ${recorder.trace}`);
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_UsingExpression_DisposesSingleFactoryResultAcrossReturnAndThrowOnDenoHost()
    {
        const string scenarioId = "ast-converter-runtime-class.using-expression-return-throw";
        var fixture = CompileModule(
            """
            using System;

            public static class TestModule
            {
                public sealed class Recorder
                {
                    private string trace = "";
                    private int factoryCalls;

                    public string Trace => trace;
                    public int FactoryCalls => factoryCalls;

                    public string ReturnThroughUsing()
                    {
                        using (Open("return"))
                        {
                            Append("body:return|");
                            return Trace;
                        }
                    }

                    public void ThrowThroughUsing()
                    {
                        using (Open("throw"))
                        {
                            Append("body:throw|");
                            throw null;
                        }
                    }

                    private Resource Open(string label)
                    {
                        factoryCalls++;
                        Append("open:" + label + "|");
                        return new Resource(this, label);
                    }

                    public void Append(string value)
                    {
                        trace += value;
                    }

                }

                public sealed class Resource : IDisposable
                {
                    private readonly Recorder owner;
                    private readonly string label;

                    public Resource(Recorder owner, string label)
                    {
                        this.owner = owner;
                        this.label = label;
                    }

                    public void Dispose()
                    {
                        owner.Append("dispose:" + label + "|");
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "try", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "finally", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-using-expression-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "using-expression.mjs");
            var testPath = Path.Combine(root, "using-expression.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { Recorder } from "./using-expression.mjs";

                Deno.test("using expression caches its resource and disposes it across control-flow exits", () => {
                  const returning = new Recorder();
                  const returned = returning.returnThroughUsing();
                  if (returned !== "open:return|body:return|" ||
                      returning.trace !== "open:return|body:return|dispose:return|" ||
                      returning.factoryCalls !== 1) {
                    throw new Error(`unexpected return trace: returned=${returned}, trace=${returning.trace}, calls=${returning.factoryCalls}`);
                  }

                  const throwing = new Recorder();
                  let observedFailure = false;
                  try {
                    throwing.throwThroughUsing();
                  } catch {
                    observedFailure = true;
                  }
                  if (!observedFailure ||
                      throwing.trace !== "open:throw|body:throw|dispose:throw|" ||
                      throwing.factoryCalls !== 1) {
                    throw new Error(`unexpected throw trace: observed=${observedFailure}, trace=${throwing.trace}, calls=${throwing.factoryCalls}`);
                  }
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_ErasedInterfacePattern_UsesNullabilityAndProbesOnce()
    {
        const string scenarioId = "ast-converter-runtime-class.erased-interface-pattern";
        var fixture = CompileModule(
            """
            using System;

            public static class TestModule
            {
                public sealed class Matcher
                {
                    private IComparable? next;
                    private int probes;

                    public int Probes => probes;

                    public void SetNext(IComparable? value)
                    {
                        next = value;
                    }

                    private IComparable? Probe()
                    {
                        probes++;
                        return next;
                    }

                    public bool HasTag()
                    {
                        return Probe() is IComparable;
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "!= null", StringComparison.Ordinal, scenarioId);
        Assert.IsFalse(script.Contains("instanceof IComparable", StringComparison.Ordinal), scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-erased-interface-pattern-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "erased-interface-pattern.mjs");
            var testPath = Path.Combine(root, "erased-interface-pattern.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { Matcher } from "./erased-interface-pattern.mjs";

                Deno.test("erased interface pattern keeps C# null semantics and evaluates its source once", () => {
                  const matcher = new Matcher();
                  matcher.setNext(null);
                  if (matcher.hasTag() || matcher.probes !== 1)
                    throw new Error(`expected a null result after one probe, got result=${matcher.hasTag()}, probes=${matcher.probes}`);

                  matcher.setNext("tag");
                  if (!matcher.hasTag() || matcher.probes !== 2)
                    throw new Error(`expected a tagged result after two probes, got result=${matcher.hasTag()}, probes=${matcher.probes}`);
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_ListPatternSwitchExpression_CapturesSliceAndEvaluatesDiscriminantOnce()
    {
        const string scenarioId = "ast-converter-runtime-class.list-pattern-switch-expression";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class Matcher
                {
                    private int[]? next;
                    private int probes;

                    public int Probes => probes;

                    public void SetNext(int[]? value)
                    {
                        next = value;
                    }

                    private int[]? Probe()
                    {
                        probes++;
                        return next;
                    }

                    public int Describe()
                    {
                        return Probe() switch
                        {
                            [1, .. var tail] when tail.Length > 0 => tail[0],
                            [1] => 100,
                            null => -1,
                            _ => -2
                        };
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "Array.isArray", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-list-pattern-switch-expression-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "list-pattern-switch-expression.mjs");
            var testPath = Path.Combine(root, "list-pattern-switch-expression.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { Matcher } from "./list-pattern-switch-expression.mjs";

                Deno.test("list-pattern switch expression preserves arm ordering and single evaluation", () => {
                  const matcher = new Matcher();
                  const cases = [
                    [[1, 9], 9],
                    [[1], 100],
                    [null, -1],
                    [[2, 9], -2]
                  ];

                  for (const [value, expected] of cases) {
                    matcher.setNext(value);
                    const result = matcher.describe();
                    if (result !== expected)
                      throw new Error(`expected ${expected}, got ${result}`);
                  }

                  if (matcher.probes !== cases.length)
                    throw new Error(`expected ${cases.length} discriminant probes, got ${matcher.probes}`);
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_StructuralListPattern_UsesCustomLengthIndexerAndSliceContractsOnDenoHost()
    {
        const string scenarioId = "ast-converter-runtime-class.structural-list-pattern";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class Segment
                {
                    private readonly int[] values;

                    public Segment(int[] values)
                    {
                        this.values = values;
                    }

                    public int Length => values.Length;

                    public int this[int index]
                    {
                        get => values[index];
                        set => values[index] = value;
                    }

                    public Segment Slice(int start, int length)
                    {
                        var slice = new int[length];
                        for (var index = 0; index < length; index++)
                            slice[index] = values[start + index];
                        return new Segment(slice);
                    }
                }

                public sealed class Matcher
                {
                    private int probes;

                    public int Describe(Segment segment)
                    {
                        return segment switch
                        {
                            [1, .. var middle, 9] when middle.Length == 1 => middle[0],
                            [1, 9] => 90,
                            _ => -1
                        };
                    }

                    public int Mutate(Segment segment)
                    {
                        segment[1] = 4;
                        segment[1] += 2;
                        return segment[1];
                    }

                    public int MutateLast(Segment segment)
                    {
                        Read(segment)[^1] += 2;
                        return probes * 100 + segment[^1];
                    }

                    private Segment Read(Segment segment)
                    {
                        probes++;
                        return segment;
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();
        var indexer = fixture.GetType("Segment")
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Single(static property => property.IsIndexer);
        var getterHelper = Util.GetMemberIndexerAccessorHelperName(indexer.GetMethod!);
        var setterHelper = Util.GetMemberIndexerAccessorHelperName(indexer.SetMethod!);

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, ".length", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, ".slice(1, ", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, getterHelper + "(", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, setterHelper + "(", StringComparison.Ordinal, scenarioId);
        Assert.IsFalse(script.Contains("get this[]", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("Array.isArray(segment)", StringComparison.Ordinal), script);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-structural-list-pattern-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "structural-list-pattern.mjs");
            var testPath = Path.Combine(root, "structural-list-pattern.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { Matcher, Segment } from "./structural-list-pattern.mjs";

                Deno.test("structural list pattern preserves custom collection semantics", () => {
                  const matcher = new Matcher();
                  const cases = [
                    [[1, 5, 9], 5],
                    [[1, 9], 90],
                    [[1, 5, 6, 9], -1],
                    [[2, 5, 9], -1]
                  ];

                  for (const [values, expected] of cases) {
                    const actual = matcher.describe(new Segment(values));
                    if (actual !== expected)
                      throw new Error(`expected ${expected}, got ${actual} for ${values}`);
                  }

                  const writable = new Segment([1, 2, 3]);
                  const updated = matcher.mutate(writable);
                  if (updated !== 6)
                    throw new Error(`expected indexer mutation to return 6, got ${updated}`);

                  const lastUpdated = matcher.mutateLast(writable);
                  if (lastUpdated !== 105)
                    throw new Error(`expected one receiver read and a final value of 5, got result=${lastUpdated}`);
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_OverloadedRuntimeClassIndexers_UseDistinctSymbolBoundHelpersOnDenoHost()
    {
        const string scenarioId = "ast-converter-runtime-class.overloaded-indexers";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class KeyedValues
                {
                    private int numericValue;
                    private int namedValue;

                    public int this[int key]
                    {
                        get => numericValue + key;
                        set => numericValue = value - key;
                    }

                    public int this[string key]
                    {
                        get => namedValue + key.Length;
                        set => namedValue = value - key.Length;
                    }
                }

                public sealed class Consumer
                {
                    public int WriteAndRead(KeyedValues values)
                    {
                        values[2] = 8;
                        values["x"] = 10;
                        return values[2] + values["x"];
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();
        var indexers = fixture.GetType("KeyedValues")
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(static property => property.IsIndexer)
            .ToArray();
        var getterHelpers = indexers
            .Select(static property => Util.GetMemberIndexerAccessorHelperName(property.GetMethod!))
            .ToArray();
        var setterHelpers = indexers
            .Select(static property => Util.GetMemberIndexerAccessorHelperName(property.SetMethod!))
            .ToArray();

        Assert.AreEqual(2, indexers.Length, scenarioId);
        Assert.AreNotEqual(getterHelpers[0], getterHelpers[1], scenarioId);
        Assert.AreNotEqual(setterHelpers[0], setterHelpers[1], scenarioId);
        Assert.IsNotNull(script, scenarioId);
        foreach (var helper in getterHelpers.Concat(setterHelpers))
            StringAssert.Contains(script, helper + "(", StringComparison.Ordinal, scenarioId);

        Assert.IsFalse(script.Contains("get this[]", StringComparison.Ordinal), script);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-overloaded-indexers-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "overloaded-indexers.mjs");
            var testPath = Path.Combine(root, "overloaded-indexers.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { Consumer, KeyedValues } from "./overloaded-indexers.mjs";

                Deno.test("overloaded C# indexers remain symbol-bound", () => {
                  const consumer = new Consumer();
                  const actual = consumer.writeAndRead(new KeyedValues());
                  if (actual !== 18)
                    throw new Error(`expected 18, got ${actual}`);
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_Loops_PreserveContinueUpdatesBreakAndDoWhileOnDenoHost()
    {
        const string scenarioId = "ast-converter-runtime-class.loop-control-flow";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class BatchProcessor
                {
                    public int SumOddUntil(int upperBound)
                    {
                        var total = 0;
                        for (var value = 0; value < upperBound; value++)
                        {
                            if (value % 2 == 0)
                                continue;

                            total += value;
                            if (total >= 9)
                                break;
                        }

                        return total;
                    }

                    public int CountDown(int value)
                    {
                        var visits = 0;
                        do
                        {
                            visits++;
                            value--;
                        }
                        while (value > 0);

                        return visits;
                    }

                    public int SumUntil(int[] values, int limit)
                    {
                        var total = 0;
                        foreach (var value in values)
                        {
                            if (value < 0)
                                continue;

                            total += value;
                            if (total >= limit)
                                break;
                        }

                        return total;
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "for (let value", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "do {", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "for (let value of values)", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-loop-control-flow-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "loop-control-flow.mjs");
            var testPath = Path.Combine(root, "loop-control-flow.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { BatchProcessor } from "./loop-control-flow.mjs";

                Deno.test("loop lowering retains C# continue, break, foreach, and do-while behavior", () => {
                  const processor = new BatchProcessor();
                  if (processor.sumOddUntil(7) !== 9)
                    throw new Error(`expected odd total 9, got ${processor.sumOddUntil(7)}`);
                  if (processor.countDown(0) !== 1 || processor.countDown(3) !== 3)
                    throw new Error("do-while did not execute exactly once for zero and once per positive input");
                  if (processor.sumUntil([-1, 4, 7, 9], 10) !== 11)
                    throw new Error("foreach did not skip negative values and stop after reaching the limit");
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_ForCapturedControlVariable_PreservesSingleCSharpBindingOnDenoHost()
    {
        const string scenarioId = "ast-converter-runtime-class.for-captured-control-variable";
        var fixture = CompileModule(
            """
            using System;

            public static class TestModule
            {
                public sealed class CallbackReader
                {
                    public int ReadCapturedValues()
                    {
                        Func<int> first = null!;
                        Func<int> second = null!;
                        for (var i = 0; i < 2; i++)
                        {
                            if (i == 0)
                                first = () => i;
                            else
                                second = () => i;
                        }

                        return first() * 10 + second();
                    }

                    public int ReadCapturedLocalFunctionValues()
                    {
                        Func<int> first = null!;
                        Func<int> second = null!;
                        for (var i = 0; i < 2; i++)
                        {
                            int ReadCurrent() => i;
                            if (i == 0)
                                first = ReadCurrent;
                            else
                                second = ReadCurrent;
                        }

                        return first() * 10 + second();
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "let i = 0;", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "for (; i < 2; i++)", StringComparison.Ordinal, scenarioId);
        Assert.IsFalse(script.Contains("for (let i", StringComparison.Ordinal), scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-for-captured-control-variable-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "callback-reader.mjs");
            var testPath = Path.Combine(root, "callback-reader.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { CallbackReader } from "./callback-reader.mjs";

                Deno.test("closures over a C# for control variable observe its final value", () => {
                  const reader = new CallbackReader();
                  const lambda = reader.readCapturedValues();
                  const localFunction = reader.readCapturedLocalFunctionValues();
                  if (lambda !== 22 || localFunction !== 22)
                    throw new Error(`expected both callback forms to observe final i = 2, got lambda=${lambda}, localFunction=${localFunction}`);
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_ForUpdateSequence_AwaitsAfterIncrementOnDenoHost()
    {
        const string scenarioId = "ast-converter-runtime-class.for-update-await-sequence";
        var fixture = CompileModule(
            """
            using System.Threading.Tasks;

            public static class TestModule
            {
                public sealed class ProgressRecorder
                {
                    private string trace = "";

                    public string Trace => trace;

                    public async Task RunAsync()
                    {
                        for (var item = 0; item < 2; item++, await TickAsync(item))
                        {
                            trace += "body:" + item + ":";
                        }

                        trace += "done:";
                    }

                    private async Task TickAsync(int item)
                    {
                        trace += "tick:" + item + ":start:";
                        await Task.Yield();
                        trace += "tick:" + item + ":end:";
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "item++, await this.tickAsync(item)", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "await Promise.resolve()", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-for-update-await-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "for-update-await.mjs");
            var testPath = Path.Combine(root, "for-update-await.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { ProgressRecorder } from "./for-update-await.mjs";

                Deno.test("for update increments before the awaited progress tick", async () => {
                  const recorder = new ProgressRecorder();
                  const run = recorder.runAsync();
                  if (recorder.trace !== "body:0:tick:1:start:")
                    throw new Error(`expected body item 0 then suspended tick 1, got ${recorder.trace}`);

                  await run;
                  const expected = "body:0:tick:1:start:tick:1:end:body:1:tick:2:start:tick:2:end:done:";
                  if (recorder.trace !== expected)
                    throw new Error(`expected increment/await update order ${expected}, got ${recorder.trace}`);
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_ConditionalArrayIndexAndRange_CacheNullableReceiverOnceOnDenoHost()
    {
        const string scenarioId = "ast-converter-runtime-class.conditional-array-index-and-range";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class SnapshotReader
                {
                    private int[]? current;
                    private int probes;

                    public int Probes => probes;

                    public void SetCurrent(int[]? value)
                    {
                        current = value;
                    }

                    private int[]? Probe()
                    {
                        probes++;
                        return current;
                    }

                    public int? ReadLast()
                    {
                        return Probe()?[^1];
                    }

                    public int[]? ReadInterior()
                    {
                        return Probe()?[1..^1];
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "== null ? undefined", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, ".length - 1", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, ".slice(1,", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-conditional-array-index-and-range-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "conditional-array-index-and-range.mjs");
            var testPath = Path.Combine(root, "conditional-array-index-and-range.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { SnapshotReader } from "./conditional-array-index-and-range.mjs";

                Deno.test("conditional array index and range access short-circuit and evaluate their receiver once", () => {
                  const reader = new SnapshotReader();
                  reader.setCurrent(null);
                  const missingLast = reader.readLast();
                  const missingInterior = reader.readInterior();
                  if (missingLast != null || missingInterior != null)
                    throw new Error("conditional access did not preserve the nullable result");

                  reader.setCurrent([3, 5, 8]);
                  const last = reader.readLast();
                  const interior = reader.readInterior();
                  if (last !== 8 || interior.length !== 1 || interior[0] !== 5)
                    throw new Error(`expected last=8 and interior=[5], got last=${last}, interior=${interior}`);
                  if (reader.probes !== 4)
                    throw new Error(`expected four receiver probes, got ${reader.probes}`);
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_NullableGetValueOrDefault_UsesSingleProbeAndUnderlyingDefaultOnDenoHost()
    {
        const string scenarioId = "ast-converter-runtime-class.nullable-get-value-or-default";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class DefaultReader
                {
                    private int? next;
                    private int probes;

                    public int Probes => probes;

                    public void SetNext(int? value)
                    {
                        next = value;
                    }

                    private int? Probe()
                    {
                        probes++;
                        return next;
                    }

                    public int Read()
                    {
                        return Probe().GetValueOrDefault();
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "return this.probe() ?? 0;", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-nullable-get-value-or-default-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "nullable-get-value-or-default.mjs");
            var testPath = Path.Combine(root, "nullable-get-value-or-default.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { DefaultReader } from "./nullable-get-value-or-default.mjs";

                Deno.test("Nullable GetValueOrDefault preserves a single nullable receiver evaluation", () => {
                  const reader = new DefaultReader();

                  reader.setNext(null);
                  if (reader.read() !== 0 || reader.probes !== 1)
                    throw new Error(`expected null to produce 0 after one probe, got value=${reader.read()} probes=${reader.probes}`);

                  reader.setNext(undefined);
                  if (reader.read() !== 0 || reader.probes !== 2)
                    throw new Error(`expected undefined to produce 0 after one probe, got value=${reader.read()} probes=${reader.probes}`);

                  reader.setNext(23);
                  if (reader.read() !== 23 || reader.probes !== 3)
                    throw new Error(`expected 23 after one probe, got value=${reader.read()} probes=${reader.probes}`);
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_StringFormattingIntrinsics_PreserveComposedValue()
    {
        const string scenarioId = "ast-converter-runtime-class.string-formatting-intrinsics";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class LabelFormatter
                {
                    public string FormatLabels()
                    {
                        string[] labels = ["red", "blue"];
                        return string.Join("/", labels).ToUpperInvariant().PadLeft(10, '*');
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "from \"System/StringModule.js\"", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(
            script,
            "return _7894e0294f780eb5(_f269cd27a4bbd549(\"/\", labels).toUpperCase(), 10, \"*\");",
            StringComparison.Ordinal,
            scenarioId);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task ConvertModule_InstanceCapturingLocalFunctionDelegate_PreservesReceiverWhenDetached()
    {
        const string scenarioId = "ast-converter-runtime-class.local-function-delegate-bound-receiver";
        var fixture = CompileModule(
            """
            using System;

            public static class TestModule
            {
                public sealed class Counter
                {
                    public int Value;

                    public Func<int> CreateIncrementer()
                    {
                        int Increment()
                        {
                            Value++;
                            return Value;
                        }

                        return Increment;
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "Increment.bind(this)", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-local-function-delegate-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "counter.mjs");
            var testPath = Path.Combine(root, "counter.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { Counter } from "./counter.mjs";

                Deno.test("instance-capturing local function remains bound after callback detaches", () => {
                  const counter = new Counter();
                  const increment = counter.createIncrementer();
                  const first = increment();
                  const second = increment();
                  if (first !== 1 || second !== 2 || counter.value !== 2)
                    throw new Error(`expected detached callback results 1, 2 and counter value 2; got ${first}, ${second}, ${counter.value}`);
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_MethodGroupFromSideEffectingReceiver_EvaluatesAndBindsOnce()
    {
        const string scenarioId = "ast-converter-runtime-class.method-group-side-effecting-receiver";
        var fixture = CompileModule(
            """
            using System;

            public static class TestModule
            {
                public sealed class Counter
                {
                    public int Value;

                    public int Increment()
                    {
                        Value++;
                        return Value;
                    }
                }

                public sealed class CounterFactory
                {
                    private readonly Counter counter = new();

                    public int Reads;

                    public Counter GetCounter()
                    {
                        Reads++;
                        return counter;
                    }

                    public Func<int> CreateIncrementer() => GetCounter().Increment;

                    public int Value => counter.Value;
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, ".bind(", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-method-group-receiver-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "counter-factory.mjs");
            var testPath = Path.Combine(root, "counter-factory.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { CounterFactory } from "./counter-factory.mjs";

                Deno.test("method-group receiver is evaluated once and remains bound after callback detaches", () => {
                  const factory = new CounterFactory();
                  const increment = factory.createIncrementer();
                  const first = increment();
                  const second = increment();
                  if (factory.reads !== 1 || first !== 1 || second !== 2 || factory.value !== 2)
                    throw new Error(`expected one receiver read and detached results 1, 2; got reads=${factory.reads}, results=${first},${second}, value=${factory.value}`);
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_NestedRecordForEachDeconstruction_PreservesStructuralBindings()
    {
        const string scenarioId = "ast-converter-runtime-class.nested-record-foreach-deconstruction";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed record ReleaseMetric(string Id, int Pending);

                public sealed record ReleaseStage(string Name, ReleaseMetric Metric);

                public sealed class ReleaseQueue
                {
                    public int SumReadyPending()
                    {
                        ReleaseStage[] stages =
                        [
                            new ReleaseStage("ready", new ReleaseMetric("a", 2)),
                            new ReleaseStage("skip", new ReleaseMetric("b", 8)),
                            new ReleaseStage("ready", new ReleaseMetric("c", 3))
                        ];
                        int total = 0;
                        foreach (var (name, (_, pending)) in stages)
                        {
                            if (name == "ready")
                                total += pending;
                        }

                        return total;
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "for (let { name: name, metric: {", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-nested-record-foreach-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "release-queue.mjs");
            var testPath = Path.Combine(root, "release-queue.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { ReleaseQueue } from "./release-queue.mjs";

                Deno.test("nested record foreach binding retains the selected nested property", () => {
                  const queue = new ReleaseQueue();
                  const total = queue.sumReadyPending();
                  if (total !== 5)
                    throw new Error(`expected ready pending total 5, got ${total}`);
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_ImplicitMemberFieldDefaults_PreserveCSharpValueSemantics()
    {
        const string scenarioId = "ast-converter-runtime-class.implicit-member-field-defaults";
        var fixture = CompileModule(
            """
            using System;

            public static class TestModule
            {
                public sealed class Defaults
                {
                    public bool IsReady;
                    public char Marker;
                    public Half Fraction;
                    public long Total;
                    public Int128 Huge;
                    public string? Label;
                    public int? Retry;
                    public (int Count, bool Enabled) State;
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "isReady = false", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "marker = \"\\0\"", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "fraction = 0", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "total = 0n", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "huge = 0n", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "label = null", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "retry = null", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "state = { count: 0, enabled: false }", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-field-defaults-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "defaults.mjs");
            var testPath = Path.Combine(root, "defaults.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { Defaults } from "./defaults.mjs";

                Deno.test("implicit member fields use C# defaults instead of undefined", () => {
                  const value = new Defaults();
                  if (value.isReady !== false || value.marker !== "\0" || value.fraction !== 0 || value.total !== 0n || value.huge !== 0n || value.label !== null || value.retry !== null)
                    throw new Error("scalar C# member defaults were not preserved");
                  if (value.state.count !== 0 || value.state.enabled !== false)
                    throw new Error("tuple C# member defaults were not preserved");
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_ImplicitKnownClrValueFieldDefaults_ImportBoundConstructors()
    {
        const string scenarioId = "ast-converter-runtime-class.implicit-known-clr-value-field-defaults";
        var fixture = CompileModule(
            """
            using System;

            public static class TestModule
            {
                public sealed class Schedule
                {
                    public DateTime CreatedAt;
                    public DateTimeOffset PublishedAt;
                    public DateOnly DueDate;
                    public TimeOnly StartTime;
                    public TimeSpan Duration;
                    public Guid Id;
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "from \"System/DateTimeModule.js\"", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "from \"System/DateTimeOffsetModule.js\"", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "from \"System/DateOnlyModule.js\"", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "from \"System/TimeOnlyModule.js\"", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "from \"System/TimeSpanModule.js\"", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "from \"System/GuidModule.js\"", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "createdAt = _bfa8ee5dd46e2005()", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "publishedAt = _12b4f3f1dc14bea9()", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "dueDate = _5f8053a9657a0844()", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "startTime = _9f78f92d0753f4cf()", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "duration = _5af0f6ad850e6702()", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "id = _0e58e51018e846d2()", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task ConvertModule_ImplicitSourceReferenceFieldDefault_UsesNullWithoutExternalFallback()
    {
        const string scenarioId = "ast-converter-runtime-class.implicit-source-reference-field-default";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class Session
                {
                }

                public sealed class Dashboard
                {
                    public Session CurrentSession;
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "class Dashboard", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "currentSession = null", StringComparison.Ordinal, scenarioId);
        Assert.IsFalse(script.Contains("import ", StringComparison.Ordinal), scenarioId);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task ConvertModule_ImplicitErasedUnionFieldDefault_UsesNoBranchNull()
    {
        const string scenarioId = "ast-converter-runtime-class.implicit-erased-union-field-default";
        var fixture = CompileModule(
            """
            using ECMAScript;

            public static class TestModule
            {
                public sealed class NavigationTarget
                {
                    public RouteLocationRaw Route;
                }
            }
            """,
            scenarioId,
            MetadataReference.CreateFromFile(typeof(ECMAScript.RouteLocationRaw).Assembly.Location));
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "route = null", StringComparison.Ordinal, scenarioId);
        Assert.IsFalse(script.Contains("RouteLocationRaw", StringComparison.Ordinal), scenarioId);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public void ConvertRuntimeClass_ExternalBase_RejectsUnboundInheritance()
    {
        const string scenarioId = "ast-converter-runtime-class.external-base-rejected";
        var fixture = CompileModule(
            """
            public class ExternalBase
            {
            }

            public static class TestModule
            {
                public sealed class Derived : ExternalBase
                {
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var exception = Assert.ThrowsExactly<NotSupportedException>(
            () => converter.ConvertRuntimeClass(fixture.GetType("Derived")));

        StringAssert.Contains(exception.Message, "runtime class does not support inheritance", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(exception.Message, "Derived : ExternalBase", StringComparison.Ordinal, scenarioId);
    }

    [TestMethod]
    public async Task Convert_ModulePolicy_PredeclaredNestedHelperIsFlattenedToArtifactScope()
    {
        const string scenarioId = "ast-converter-runtime-class.razorvue-predeclared-nested-helper";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class Host
                {
                    public sealed class RenderHelper
                    {
                    }
                }
            }
            """,
            scenarioId);
        var helper = fixture.GetType("Host").GetTypeMembers("RenderHelper").Single();
        var declaredNames = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default)
        {
            [helper.OriginalDefinition] = "renderHelper"
        };
        var converter = new AstConverter(
            fixture.Module,
            fixture.SemanticModel,
            new AstConverterOptions(
                AstConverterProfile.Standard,
                DeclaredNames: declaredNames,
                ModulePolicy: FlattenNestedRuntimeClassModulePolicy.Instance));

        var module = await converter.Convert();
        var helperDeclaration = converter.ConvertRuntimeClass(helper);

        Assert.IsNotNull(module, scenarioId);
        var exportedHost = module!.Body.OfType<ExportNamedDeclaration>().Single();
        Assert.IsInstanceOfType<ClassDeclaration>(exportedHost.Declaration, scenarioId);
        Assert.AreEqual("Host", ((ClassDeclaration)exportedHost.Declaration).Id?.Name, scenarioId);
        Assert.IsFalse(
            module.ToKnRECMAScript().Contains("RenderHelper", StringComparison.Ordinal),
            scenarioId);
        Assert.AreEqual("renderHelper", helperDeclaration.Id?.Name, scenarioId);
        _ = new Parser().ParseModule(module.ToKnRECMAScript());
        _ = new Parser().ParseScript(helperDeclaration.ToKnRECMAScript());
    }

    [TestMethod]
    public async Task Convert_ModulePolicy_FlattenedNestedHelperKeepsCreationReferenceAndDeclarationAligned()
    {
        const string scenarioId = "ast-converter-runtime-class.flattened-nested-helper-creation-reference";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class Host
                {
                    public RenderHelper Create() => new RenderHelper();

                    public sealed class RenderHelper
                    {
                    }
                }
            }
            """,
            scenarioId);
        var helper = fixture.GetType("Host").GetTypeMembers("RenderHelper").Single();
        var declaredNames = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default)
        {
            [helper.OriginalDefinition] = "renderHelper"
        };
        var converter = new AstConverter(
            fixture.Module,
            fixture.SemanticModel,
            new AstConverterOptions(
                AstConverterProfile.Standard,
                DeclaredNames: declaredNames,
                ModulePolicy: FlattenNestedRuntimeClassModulePolicy.Instance));

        var module = await converter.Convert();
        var helperDeclaration = converter.ConvertRuntimeClass(helper);

        Assert.IsNotNull(module, scenarioId);
        var moduleScript = module!.ToKnRECMAScript();
        var helperScript = helperDeclaration.ToKnRECMAScript();
        StringAssert.Contains(moduleScript, "new renderHelper", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(helperScript, "class renderHelper", StringComparison.Ordinal, scenarioId);
        Assert.IsFalse(moduleScript.Contains("new RenderHelper", StringComparison.Ordinal), scenarioId);
        _ = new Parser().ParseModule(moduleScript);
        _ = new Parser().ParseScript(helperScript);
    }

    [TestMethod]
    public void ConvertRuntimeClass_CanceledToken_StopsBeforeLowering()
    {
        const string scenarioId = "ast-converter-runtime-class.canceled-token";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class Worker
                {
                }
            }
            """,
            scenarioId);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        _ = Assert.ThrowsExactly<OperationCanceledException>(
            () => converter.ConvertRuntimeClass(fixture.GetType("Worker"), cancellationSource.Token),
            scenarioId);
    }

    [TestMethod]
    public void ConvertRuntimeClass_ExpressionBodiedConstructor_PreservesBoundAssignment()
    {
        const string scenarioId = "ast-converter-runtime-class.expression-bodied-constructor";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class Widget
                {
                    public int Value;

                    public Widget(int value) => Value = value;
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var declaration = converter.ConvertRuntimeClass(fixture.GetType("Widget"));
        var script = declaration.ToKnRECMAScript();

        StringAssert.Contains(script, "constructor(value) {\n    this.value = value;\n  }", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void ConvertRuntimeClass_AbstractProperty_RejectsMissingRuntimeAccessor()
    {
        const string scenarioId = "ast-converter-runtime-class.abstract-property-rejected";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public abstract class Widget
                {
                    public abstract int Value { get; }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var exception = Assert.ThrowsExactly<NotSupportedException>(
            () => converter.ConvertRuntimeClass(fixture.GetType("Widget")),
            scenarioId);

        StringAssert.Contains(exception.Message, "does not support abstract property Value", StringComparison.Ordinal, scenarioId);
    }

    [TestMethod]
    public void ConvertRuntimeClass_Event_RejectsUnimplementedSubscriptionProtocol()
    {
        const string scenarioId = "ast-converter-runtime-class.event-rejected";
        var fixture = CompileModule(
            """
            using System;

            public static class TestModule
            {
                public sealed class Widget
                {
                    public event Action? Changed;

                    public void Raise() => Changed?.Invoke();
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var exception = Assert.ThrowsExactly<NotSupportedException>(
            () => converter.ConvertRuntimeClass(fixture.GetType("Widget")),
            scenarioId);

        StringAssert.Contains(exception.Message, "does not support Event:Changed", StringComparison.Ordinal, scenarioId);
    }

    [TestMethod]
    public void ConvertRuntimeClass_NestedDelegate_RejectsUnimplementedRuntimeDeclaration()
    {
        const string scenarioId = "ast-converter-runtime-class.nested-delegate-rejected";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class Widget
                {
                    public delegate void Changed();
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var exception = Assert.ThrowsExactly<NotSupportedException>(
            () => converter.ConvertRuntimeClass(fixture.GetType("Widget")),
            scenarioId);

        StringAssert.Contains(exception.Message, "does not support NamedType:Changed", StringComparison.Ordinal, scenarioId);
    }

    [TestMethod]
    public void ConvertRuntimeClass_MemberFilter_ExcludesFilteredMethod()
    {
        const string scenarioId = "ast-converter-runtime-class.member-filter";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class Widget
                {
                    public int Keep() => 1;

                    public int Skip() => 2;
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(
            fixture.Module,
            fixture.SemanticModel,
            new AstConverterOptions(
                AstConverterProfile.Standard,
                MemberFilter: static symbol => symbol.Name != "Skip"));

        var declaration = converter.ConvertRuntimeClass(fixture.GetType("Widget"));
        var script = declaration.ToKnRECMAScript();

        StringAssert.Contains(script, "keep()", StringComparison.Ordinal, scenarioId);
        Assert.IsFalse(script.Contains("skip()", StringComparison.Ordinal), scenarioId);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    [DataRow("ref int value", "ref")]
    [DataRow("params int[] values", "params")]
    public void ConvertRuntimeClass_OverloadedConstructorWithUnsupportedDispatchParameter_Rejects(
        string parameter,
        string expectedParameterKind)
    {
        var scenarioId = $"ast-converter-runtime-class.overload-{expectedParameterKind}-rejected";
        var fixture = CompileModule(
            $$"""
            public static class TestModule
            {
                public sealed class Widget
                {
                    public Widget()
                    {
                    }

                    public Widget({{parameter}})
                    {
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var exception = Assert.ThrowsExactly<NotSupportedException>(
            () => converter.ConvertRuntimeClass(fixture.GetType("Widget")),
            scenarioId);

        StringAssert.Contains(exception.Message, "constructor overload dispatch with ref/out/in/params parameters", StringComparison.Ordinal, scenarioId);
    }

    [TestMethod]
    public async Task ConvertModule_NullPatternTreatsUndefinedAsMissingValue()
    {
        const string scenarioId = "ast-converter-runtime-class.null-pattern-undefined";
        var fixture = CompileModule(
            """
            public static class TestModule
            {
                public sealed class NullMatcher
                {
                    public bool IsMissing(string? value) => value is null;

                    public bool HasValue(string? value) => value is not null;
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "value == null", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "!(value == null)", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-null-pattern-undefined-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "null-matcher.mjs");
            var testPath = Path.Combine(root, "null-matcher.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { NullMatcher } from "./null-matcher.mjs";

                Deno.test("null patterns treat an omitted host value as missing", () => {
                  const matcher = new NullMatcher();
                  const cases = [
                    [undefined, true, false],
                    [null, true, false],
                    ["present", false, true]
                  ];

                  for (const [value, missing, hasValue] of cases) {
                    if (matcher.isMissing(value) !== missing || matcher.hasValue(value) !== hasValue)
                      throw new Error(`unexpected null-pattern result for ${String(value)}`);
                  }
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ConvertModule_DictionaryForEachDeconstruction_UsesMapEntryProtocolOnDenoHost()
    {
        const string scenarioId = "ast-converter-runtime-class.dictionary-foreach-deconstruction";
        var fixture = CompileModule(
            """
            using System.Collections.Generic;

            public static class TestModule
            {
                public sealed class ReleaseQueue
                {
                    public int SumDeployable(Dictionary<string, int> stages)
                    {
                        var total = 0;
                        foreach (var (name, pending) in stages)
                        {
                            if (name != "queued" || pending <= 0)
                                continue;

                            total += pending;
                        }

                        return total;
                    }
                }
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        StringAssert.Contains(script, "for (let [name, pending] of stages)", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-runtime-class-dictionary-foreach-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var modulePath = Path.Combine(root, "release-queue.mjs");
            var testPath = Path.Combine(root, "release-queue.test.mjs");
            await File.WriteAllTextAsync(
                modulePath,
                script,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await File.WriteAllTextAsync(
                testPath,
                """
                import { ReleaseQueue } from "./release-queue.mjs";

                Deno.test("dictionary foreach deconstruction consumes JavaScript Map entries", () => {
                  const queue = new ReleaseQueue();
                  const stages = new Map([
                    ["queued", 2],
                    ["deployed", 9],
                    ["paused", 0]
                  ]);
                  const total = queue.sumDeployable(stages);
                  if (total !== 2)
                    throw new Error(`expected only the queued Map entry, got ${total}`);
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static RuntimeClassFixture CompileModule(
        string source,
        string scenarioId,
        params MetadataReference[] additionalReferences)
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "AstConverterRuntimeClassScenario.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "AstConverterRuntimeClassScenarios_" + Guid.NewGuid().ToString("N"),
            syntaxTrees: [sourceTree],
            references: [.. TestMetadataReferences.Net11, .. additionalReferences],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(
            0,
            errors,
            $"{scenarioId}:{Environment.NewLine}" +
            string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var semanticModel = compilation.GetSemanticModel(sourceTree);
        var module = sourceTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.ValueText == "TestModule")
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();
        return new RuntimeClassFixture(module, semanticModel);
    }

    private sealed record RuntimeClassFixture(
        INamedTypeSymbol Module,
        SemanticModel SemanticModel)
    {
        public INamedTypeSymbol GetType(string name)
            => Module.GetTypeMembers(name).Single();
    }
}
