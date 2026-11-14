using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Jazor.Compiler;
using Jazor.NodeHost;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.ComplierTest;

internal static class NumericAuditOracle
{
    private sealed record Case(string Id, string Source);
    private static readonly Lazy<Task<IReadOnlyDictionary<string, string>>> Results = new(RunAsync);

    private static readonly string[] BoundarySources =
    [
        "int a=int.MaxValue, b=1; var result=unchecked(a+b);",
        "int a=int.MinValue, b=1; var result=unchecked(a-b);",
        "int a=int.MaxValue, b=int.MaxValue; var result=unchecked(a*b);",
        "uint a=uint.MaxValue, b=uint.MaxValue; var result=unchecked(a*b);",
        "uint a=uint.MaxValue, b=1; var result=unchecked(a+b);",
        "long a=long.MaxValue, b=1; var result=unchecked(a+b);",
        "ulong a=ulong.MaxValue, b=ulong.MaxValue; var result=unchecked(a*b);",
        "int a=int.MaxValue, b=1; var result=checked(a+b);",
        "uint a=uint.MaxValue, b=1; var result=checked(a+b);",
        "long a=long.MaxValue, b=2; var result=checked(a*b);",
        "int a=int.MaxValue, b=2; var result=checked(a*b);",
        "int a=-3, b=2; var result=a/b;",
        "int a=3, b=-2; var result=a/b;",
        "int a=int.MinValue, b=-1; var result=unchecked(a/b);",
        "long a=long.MinValue, b=-1; var result=unchecked(a/b);",
        "int a=int.MinValue, b=-1; var result=a%b;",
        "long a=long.MinValue, b=-1; var result=a%b;",
        "int a=1, b=0; var result=a/b;",
        "long a=1, b=0; var result=a%b;",
        "int a=-1, b=2; var result=1.0/(a/b);",
        "int a=-2, b=2; var result=1.0/(a%b);",
        "double a=-1.9; var result=(int)a;",
        "double a=2147483647.99; var result=checked((int)a);",
        "double a=2147483648.0; var result=checked((int)a);",
        "double a=double.NaN; var result=checked((int)a);",
        "double a=9223372036854775808.0; var result=checked((long)a);",
        "long a=long.MaxValue; var result=unchecked((int)a);",
        "ulong a=ulong.MaxValue; var result=unchecked((int)a);",
        "long a=-1; var result=unchecked((uint)a);",
        "long a=2147483648; var result=checked((int)a);",
        "int a=256; var result=unchecked((byte)a);",
        "int a=-129; var result=unchecked((sbyte)a);",
        "var result=unchecked((byte)300);",
        "short a=short.MaxValue; var result=unchecked(++a);",
        "byte a=byte.MaxValue; var result=checked(++a);",
        "int a=int.MaxValue; var old=unchecked(a++); var result=$\"{old}:{a}\";",
        "int calls=0; int Index(){calls++;return 0;} int[] items=[int.MaxValue]; var old=unchecked(items[Index()]++); var result=$\"{old}:{items[0]}:{calls}\";",
        "int calls=0; int Mark(int x){calls=calls*10+x;return x;} var quotient=Mark(3)/Mark(2); var result=$\"{quotient}:{calls}\";",
        "sbyte a=sbyte.MinValue; unchecked { a/=-1; } var result=a;",
        "short a=short.MinValue; unchecked { a/=-1; } var result=a;",
        "sbyte a=sbyte.MinValue; checked { a/=-1; } var result=a;",
        "long a=-1; var result=checked((uint)a);",
        "ulong a=uint.MaxValue; var result=checked((uint)a);",
        "int a=-1; var result=checked((ulong)a);",
        "int a=uint.MaxValue.GetHashCode(); var result=(ulong)a;",
    ];

    public static void AssertBoundaries()
    {
        var failures = Results.Value.GetAwaiter().GetResult();
        foreach (var failure in failures.Where(item => item.Key.StartsWith("numeric-boundary.", StringComparison.Ordinal)))
            Assert.Fail(failure.Key + ": " + failure.Value);
    }

    public static bool Covers(string id)
        => id.StartsWith("numeric-conversion.", StringComparison.Ordinal) ||
           id.StartsWith("binary-operator.arithmetic.", StringComparison.Ordinal) && IsIntegral(id.Split('.')[2]) ||
           id.StartsWith("mixed-numeric-binary.arithmetic.", StringComparison.Ordinal) && id.Split('.')[2] is "sbyte-short" or "byte-short" or "short-uint" or "int-uint" or "int-long" or "uint-long" or "uint-ulong" ||
           id.StartsWith("compound-assignment.arithmetic.", StringComparison.Ordinal) && IsIntegral(id.Split('.')[2]) ||
           id.StartsWith("unary-operator.mutation.", StringComparison.Ordinal) && IsIntegral(id.Split('.')[2]) ||
           id.StartsWith("unary-operator.numeric.", StringComparison.Ordinal) && id.Split('.')[3] == "minus" && IsIntegral(id.Split('.')[2]);

    private static bool IsIntegral(string type) => type is "sbyte" or "byte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong";

    public static void AssertEquivalent(string id)
    {
        var failures = Results.Value.GetAwaiter().GetResult();
        Assert.IsFalse(failures.TryGetValue(id, out var failure), id + ": " + failure);
    }

    private static async Task<IReadOnlyDictionary<string, string>> RunAsync()
    {
        // These scenario matrices used to assert raw JS operator tokens, which accepted integer
        // division, casts and updates with the wrong behavior. Compare each actual result to CLR.
        var cases = NumericConversionScenarioCatalog.All.Select(item => new Case(item.Id, item.Source))
            .Concat(BinaryOperatorScenarioCatalog.All.Select(item => new Case(item.Id, item.Source)))
            .Concat(MixedNumericBinaryOperatorScenarioCatalog.All.Select(item => new Case(item.Id, item.Source)))
            .Concat(CompoundAssignmentScenarioCatalog.All.Select(item => new Case(item.Id, item.Source)))
            .Concat(UnaryOperatorScenarioCatalog.All.Select(item => new Case(item.Id, item.Source)))
            .Where(item => Covers(item.Id))
            .Concat(BoundarySources.Select((source, index) => new Case("numeric-boundary." + index, source))).ToArray();
        var methods = cases.Select((item, index) => $"public static object Run{index}() {{ {item.Source} {ResultExpression(item)} }}");
        var source = "using System; public static class NumericOracle { private static object last; private static void Consume<T>(T value) { last=value; } " + string.Join("\n", methods) + "}";
        var tree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions, "numeric-oracle.cs");
        var compilation = CSharpCompilation.Create("NumericOracle_" + Guid.NewGuid().ToString("N"), [tree], TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var pe = new MemoryStream();
        var emitted = compilation.Emit(pe);
        Assert.IsTrue(emitted.Success, string.Join("\n", emitted.Diagnostics));
        pe.Position = 0;
        var context = new AssemblyLoadContext("numeric-oracle", isCollectible: true);
        var type = context.LoadFromStream(pe).GetType("NumericOracle")!;
        (string? Value, string? Error) Expected(int index)
        {
            try { return (Convert.ToString(type.GetMethod("Run" + index)!.Invoke(null, null), CultureInfo.InvariantCulture), null); }
            catch (TargetInvocationException exception) { return (null, exception.InnerException!.GetType().Name); }
        }
        var expectations = cases.Select((item, index) =>
        {
            var expected = Expected(index);
            return new { id = item.Id, method = "Run" + index, value = expected.Value, error = expected.Error };
        }).ToArray();
        context.Unload();
        var converter = new AstConverter(compilation.GetTypeByMetadataName("NumericOracle")!, compilation.GetSemanticModel(tree));
        var script = (await converter.Convert())!.ToKnRECMAScript();
        var root = Path.Combine(RepositoryTemp.Root, "numeric-oracle-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "app.mjs"), script);
            var repository = new DirectoryInfo(AppContext.BaseDirectory);
            while (!File.Exists(Path.Combine(repository.FullName, "Jazor.slnx"))) repository = repository.Parent!;
            var runtimeRoot = Path.Combine(repository.FullName, "src", "ECMAScript", "clr");
            foreach (var file in Directory.EnumerateFiles(runtimeRoot, "*.js", SearchOption.AllDirectories))
            {
                var target = Path.Combine(root, "clr", Path.GetRelativePath(runtimeRoot, file));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target);
            }
            var test = Path.Combine(root, "test.mjs");
            await File.WriteAllTextAsync(test, $$"""
                import * as module from './app.mjs';
                globalThis.__jazorTest('numeric scenarios match CLR', () => {
                  const failures = {};
                  for (const test of {{JsonSerializer.Serialize(expectations)}}) {
                    try {
                      const result = String(module[test.method]());
                      if (test.error || result !== test.value) failures[test.id] = `expected ${test.error ?? test.value}, actual ${result}`;
                    } catch (error) {
                      if (!test.error || !error.message.includes(test.error)) failures[test.id] = `expected ${test.error ?? test.value}, actual ${error.message}`;
                    }
                  }
                  console.log(JSON.stringify(failures));
                });
                """);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            var output = await NodeModuleRunner.RunAsync(root, test, cancellationToken: timeout.Token);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(output)!;
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    private static string ResultExpression(Case item)
    {
        var context = item.Id.Split('.')[^1];
        if (item.Id.StartsWith("compound-assignment.", StringComparison.Ordinal))
            return (context == "lambda" ? "apply();" : "") + "return " + (context == "array-element" ? "values[0]" : "left") + ";";
        return context switch
        {
            "expression-lambda" or "block-lambda" => "return " + (item.Id.StartsWith("numeric-conversion.", StringComparison.Ordinal) ? "convert()" : "calculate()") + ";",
            "lambda" => "return update();",
            "argument" => "return last;",
            "array-element" => "return results[0];",
            "array-statement" => "return values[0];",
            "anonymous-property" or "tuple-element" => "return result.Value;",
            "local-statement" => "return value;",
            _ => "return result;"
        };
    }
}
