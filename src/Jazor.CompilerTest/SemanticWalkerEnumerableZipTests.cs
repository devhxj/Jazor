using Acornima;
using DenoHost.Core;
using ECMAScript;
using Jazor.Common;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerEnumerableZipTests
{
    [TestMethod]
    public async Task Visit_EnumerableZip_UsesCompilerOwnedDualIteratorProtocolOnDenoHost()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class EnumerableZipScenarios
            {
                public static int[] Evaluate(
                    int[] ids,
                    string[] names,
                    bool useMethodGroup,
                    bool useSelector,
                    System.Func<int, string, int> selector)
                {
                    if (useSelector)
                        return ids.Zip(names, selector).ToArray();

                    if (useMethodGroup)
                    {
                        System.Func<
                            System.Collections.Generic.IEnumerable<int>,
                            System.Collections.Generic.IEnumerable<string>,
                            System.Collections.Generic.IEnumerable<(int First, string Second)>> zipper = Enumerable.Zip;
                        return zipper(ids, names).Select(pair => pair.First + pair.Second.Length).ToArray();
                    }

                    return ids.Zip(names).Select(pair => pair.First + pair.Second.Length).ToArray();
                }
            }
            """);

        var staticKeys = block.Descendants()
            .OfType<IInvocationOperation>()
            .Select(static invocation => (invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod)
                .OriginalDefinition
                .ToDisplayString(Format.StaticExtensionNameFormat))
            .Where(static key => key.Contains("Enumerable.Zip", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        CollectionAssert.AreEquivalent(
            new[]
            {
                "static System.Linq.Enumerable.Zip<TFirst, TSecond>(System.Collections.Generic.IEnumerable<TFirst>, System.Collections.Generic.IEnumerable<TSecond>)",
                "static System.Linq.Enumerable.Zip<TFirst, TSecond, TResult>(System.Collections.Generic.IEnumerable<TFirst>, System.Collections.Generic.IEnumerable<TSecond>, System.Func<TFirst, TSecond, TResult>)"
            },
            staticKeys,
            string.Join(Environment.NewLine, staticKeys));

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        Assert.IsEmpty(argument.FlushImportSpecifiers(), body);
        StringAssert.Contains(body, "Symbol.iterator", StringComparison.Ordinal);
        StringAssert.Contains(body, "__enumerableZipFirstIterator.next()", StringComparison.Ordinal);
        StringAssert.Contains(body, "__enumerableZipSecondIterator.next()", StringComparison.Ordinal);
        StringAssert.Contains(body, "__enumerableZipSecondIterator.return", StringComparison.Ordinal);
        StringAssert.Contains(body, "__enumerableZipFirstIterator.return", StringComparison.Ordinal);
        StringAssert.Contains(body, "try {", StringComparison.Ordinal);
        StringAssert.Contains(body, "finally", StringComparison.Ordinal);
        StringAssert.Contains(body, "first: __enumerableZipFirstStep.value", StringComparison.Ordinal);
        StringAssert.Contains(body, "second: __enumerableZipSecondStep.value", StringComparison.Ordinal);
        StringAssert.Contains(body, "__enumerableZipSelector(__enumerableZipFirstStep.value, __enumerableZipSecondStep.value)", StringComparison.Ordinal);
        StringAssert.Contains(body, "zipper = (v$0$0, v$0$1) =>", StringComparison.Ordinal);

        var module = "export function evaluate(ids, names, useMethodGroup, useSelector, selector) " + body;
        _ = new Parser().ParseModule(module);

        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-zip-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var modulePath = Path.Combine(root, "zip.mjs");
            var testPath = Path.Combine(root, "zip.test.mjs");
            await System.IO.File.WriteAllTextAsync(modulePath, module, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await System.IO.File.WriteAllTextAsync(
                testPath,
                """
                import { evaluate } from "./zip.mjs";

                function assertArray(actual, expected, name) {
                  if (actual.length !== expected.length || actual.some((value, index) => value !== expected[index]))
                    throw new Error(`${name}: ${JSON.stringify(actual)} did not match ${JSON.stringify(expected)}`);
                }

                function tracked(label, values, trace) {
                  return {
                    [Symbol.iterator]() {
                      trace.push(`${label}:iterator`);
                      let index = 0;
                      return {
                        next() {
                          if (index === values.length) {
                            trace.push(`${label}:done`);
                            return { done: true };
                          }
                          const value = values[index++];
                          trace.push(`${label}:next:${value}`);
                          return { value, done: false };
                        },
                        return() {
                          trace.push(`${label}:return`);
                          return { done: true };
                        }
                      };
                    }
                  };
                }

                Deno.test("Zip preserves iterator creation, advance, and close order", () => {
                  let trace = [];
                  assertArray(
                    evaluate(tracked("first", [2, 7, 9], trace), tracked("second", ["ab", "x"], trace), false, false, () => 0),
                    [4, 8],
                    "tuple Zip");
                  if (trace.join(",") !== "first:iterator,second:iterator,first:next:2,second:next:ab,first:next:7,second:next:x,first:next:9,second:done,second:return,first:return")
                    throw new Error(`tuple Zip traversal was ${trace.join(",")}`);

                  trace = [];
                  assertArray(
                    evaluate(tracked("first", [2, 7, 9], trace), tracked("second", ["ab", "x"], trace), true, false, () => 0),
                    [4, 8],
                    "method-group Zip");
                  if (trace.join(",") !== "first:iterator,second:iterator,first:next:2,second:next:ab,first:next:7,second:next:x,first:next:9,second:done,second:return,first:return")
                    throw new Error(`method-group Zip traversal was ${trace.join(",")}`);

                  trace = [];
                  assertArray(
                    evaluate(tracked("first", [2, 7, 9], trace), tracked("second", ["ab", "x"], trace), false, true, (id, name) => {
                      trace.push(`result:${id}:${name}`);
                      return id + name.length;
                    }),
                    [4, 8],
                    "selector Zip");
                  if (trace.join(",") !== "first:iterator,second:iterator,first:next:2,second:next:ab,result:2:ab,first:next:7,second:next:x,result:7:x,first:next:9,second:done,second:return,first:return")
                    throw new Error(`selector Zip traversal was ${trace.join(",")}`);

                  trace = [];
                  assertArray(
                    evaluate(tracked("first", [], trace), tracked("second", ["unused"], trace), false, false, () => 0),
                    [],
                    "empty-first Zip");
                  if (trace.join(",") !== "first:iterator,second:iterator,first:done,second:return,first:return")
                    throw new Error(`empty-first Zip traversal was ${trace.join(",")}`);

                  let threw = false;
                  try {
                    evaluate(null, [], false, false, () => 0);
                  } catch (error) {
                    threw = error instanceof TypeError && error.message === "first";
                  }
                  if (!threw)
                    throw new Error("Zip must reject a null first source before iterator creation");
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

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "EnumerableZipScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "Evaluate");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
