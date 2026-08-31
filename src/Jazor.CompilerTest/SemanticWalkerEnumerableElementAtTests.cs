using Acornima;
using DenoHost.Core;
using ECMAScript;
using Jazor.Common;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections;
using System.Reflection;
using System.Text.Json;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerEnumerableElementAtTests
{
    [TestMethod]
    public void Visit_EnumerableElementAt_UsesBoundImportAndTerminalInvocation()
    {
        var block = GetBlockOperation(
            """
            using System.Linq;

            public static class EnumerableElementAtScenarios
            {
                public static int Evaluate(int[] releaseIds, int index)
                {
                    return releaseIds.ElementAt(index);
                }
            }
            """);

        var elementAt = block.Descendants()
            .OfType<IInvocationOperation>()
            .Single(static invocation => invocation.TargetMethod.Name == "ElementAt");
        var staticKey = (elementAt.TargetMethod.ReducedFrom ?? elementAt.TargetMethod)
            .OriginalDefinition
            .ToDisplayString(Format.StaticExtensionNameFormat);
        Assert.AreEqual(
            "static System.Linq.Enumerable.ElementAt<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)",
            staticKey);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        Assert.HasCount(1, imports, body);
        Assert.AreEqual("System/Linq/EnumerableModule.js", imports[0].Key);
        var importNames = imports[0].Value.Select(static specifier => specifier.ToECMAScript()).ToArray();
        CollectionAssert.AreEqual(new[] { "elementAt" }, importNames);
        StringAssert.Contains(body, "return elementAt(releaseIds, index);", StringComparison.Ordinal);

        var moduleImports = "import { " + string.Join(", ", importNames) + " } from \"" + imports[0].Key + "\";";
        _ = new Parser().ParseModule(moduleImports + "\nfunction verify() " + body);
    }

    [TestMethod]
    public void Visit_EnumerableElementAtIndex_UsesBoundCarrierImportForDirectAndMethodGroupCalls()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            public static class EnumerableElementAtScenarios
            {
                public static int Evaluate(IEnumerable<int> releaseIds, Index index)
                {
                    Index stored = index;
                    var direct = releaseIds.ElementAt(stored);
                    Func<IEnumerable<int>, Index, int> selector = Enumerable.ElementAt;
                    return direct + selector(releaseIds, index);
                }
            }
            """);

        var invocation = block.Descendants()
            .OfType<IInvocationOperation>()
            .Single(static candidate => candidate.TargetMethod.Name == "ElementAt");
        var methodReference = block.Descendants()
            .OfType<IMethodReferenceOperation>()
            .Single(static reference => reference.Method.Name == "ElementAt");
        foreach (var method in new[]
        {
            invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod,
            methodReference.Method
        })
        {
            Assert.AreEqual(
                "static System.Linq.Enumerable.ElementAt<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Index)",
                method.OriginalDefinition.ToDisplayString(Format.StaticExtensionNameFormat));
        }

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        Assert.HasCount(1, imports, body);
        Assert.AreEqual("System/Linq/EnumerableModule.js", imports[0].Key);
        var importNames = imports[0].Value.Select(static specifier => specifier.ToECMAScript()).ToArray();
        CollectionAssert.AreEqual(new[] { "elementAtIndex" }, importNames);
        StringAssert.Contains(body, "let direct = elementAtIndex(releaseIds, stored);", StringComparison.Ordinal);
        StringAssert.Contains(body, ") => elementAtIndex(", StringComparison.Ordinal);
        StringAssert.Contains(body, "selector(releaseIds, index)", StringComparison.Ordinal);
        Assert.IsFalse(body.Contains(".fromEnd", StringComparison.Ordinal), body);
        Assert.IsFalse(body.Contains(".value", StringComparison.Ordinal), body);

        var moduleImports = "import { " + string.Join(", ", importNames) + " } from \"" + imports[0].Key + "\";";
        _ = new Parser().ParseModule(moduleImports + "\nfunction verify() " + body);
    }

    [TestMethod]
    public async Task Visit_EnumerableElementAtOrDefault_PreservesClosedDefaultAndMethodGroupBehaviorOnDenoHost()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            public static class EnumerableElementAtScenarios
            {
                public static int Evaluate(IEnumerable<int> releaseIds, int index)
                {
                    var direct = releaseIds.ElementAtOrDefault(index);
                    Func<IEnumerable<int>, int, int> selector = Enumerable.ElementAtOrDefault;
                    return direct + selector(releaseIds, index);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        Assert.IsEmpty(argument.FlushImportSpecifiers(), body);
        StringAssert.Contains(body, "__enumerableSource", StringComparison.Ordinal);
        StringAssert.Contains(body, "__enumerableIndex < 0", StringComparison.Ordinal);
        StringAssert.Contains(body, "for (let __enumerableItem of __enumerableSource)", StringComparison.Ordinal);
        StringAssert.Contains(body, "__enumerableCurrentIndex++", StringComparison.Ordinal);
        Assert.IsFalse(body.Contains("ElementAtOrDefault", StringComparison.Ordinal), body);

        var module = "export function evaluate(releaseIds, index) " + body;
        _ = new Parser().ParseModule(module);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-enumerable-element-at-or-default-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var modulePath = Path.Combine(root, "element-at-or-default.mjs");
            var testPath = Path.Combine(root, "element-at-or-default.test.mjs");
            await System.IO.File.WriteAllTextAsync(
                modulePath,
                module,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await System.IO.File.WriteAllTextAsync(
                testPath,
                """
                import { evaluate } from "./element-at-or-default.mjs";

                Deno.test("ElementAtOrDefault returns closed defaults and closes array iteration early", () => {
                  if (evaluate([7, 9], 0) !== 14)
                    throw new Error("the direct and method-group paths must both return the first item");
                  if (evaluate([], 0) !== 0 || evaluate([7], -1) !== 0 || evaluate([7], 2) !== 0)
                    throw new Error("empty, negative, and out-of-range indexes must use default(int)");
                  let threw = false;
                  try {
                    evaluate(null, 0);
                  } catch (error) {
                    threw = error instanceof TypeError && error.message === "source";
                  }
                  if (!threw)
                    throw new Error("a null source must preserve the compiler enumerable null guard");
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
    public void Visit_EnumerableElementAtOrDefault_UnconstrainedGenericReportsErasedDefaultBoundary()
    {
        var block = GetBlockOperation(
            """
            using System.Collections.Generic;
            using System.Linq;

            public static class EnumerableElementAtScenarios
            {
                public static T Evaluate<T>(IEnumerable<T> values, int index)
                {
                    return values.ElementAtOrDefault(index);
                }
            }
            """);

        var exception = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(
            exception.Message,
            "default(T) is not supported because the runtime type parameter may be a value type",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task Visit_EnumerableElementAtOrDefaultIndex_PreservesBoundIndexTraversalOnDenoHost()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            public static class EnumerableElementAtScenarios
            {
                public static int Evaluate(IEnumerable<int> releaseIds, Index index, bool includeLiteral)
                {
                    Index stored = index;
                    var direct = releaseIds.ElementAtOrDefault(stored);
                    Func<IEnumerable<int>, Index, int> selector = Enumerable.ElementAtOrDefault;
                    var result = direct + selector(releaseIds, index);
                    if (includeLiteral)
                        result += releaseIds.ElementAtOrDefault(^1);
                    return result;
                }
            }
            """);

        var indexInvocations = block.Descendants()
            .OfType<IInvocationOperation>()
            .Where(static invocation => invocation.TargetMethod.Name == "ElementAtOrDefault")
            .ToArray();
        Assert.HasCount(2, indexInvocations);
        var methodReference = block.Descendants()
            .OfType<IMethodReferenceOperation>()
            .Single(static reference => reference.Method.Name == "ElementAtOrDefault");
        var boundMethods = indexInvocations
            .Select(static invocation => invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod)
            .Append(methodReference.Method)
            .ToArray();
        foreach (var method in boundMethods)
        {
            var staticKey = method
                .OriginalDefinition
                .ToDisplayString(Format.StaticExtensionNameFormat);
            Assert.AreEqual(
                "static System.Linq.Enumerable.ElementAtOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Index)",
                staticKey);
        }

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToArray();
        Assert.HasCount(1, imports, body);
        Assert.AreEqual("System/IndexModule.js", imports[0].Key);
        var importNames = imports[0].Value.Select(static specifier => specifier.ToECMAScript()).ToArray();
        CollectionAssert.Contains(importNames, "_b141712b3756cf57");
        CollectionAssert.Contains(importNames, "_71953783d6b61ae1");
        CollectionAssert.Contains(importNames, "_ce8b9229a41c8545");
        StringAssert.Contains(body, "const __enumerableFromEnd = _b141712b3756cf57(__enumerableIndex);", StringComparison.Ordinal);
        StringAssert.Contains(body, "const __enumerableIndexValue = _71953783d6b61ae1(__enumerableIndex);", StringComparison.Ordinal);
        StringAssert.Contains(body, "if (!__enumerableFromEnd)", StringComparison.Ordinal);
        StringAssert.Contains(body, "__enumerableTail.push(__enumerableTailItem);", StringComparison.Ordinal);
        StringAssert.Contains(body, "__enumerableTail[__enumerableTailIndex] = __enumerableTailItem;", StringComparison.Ordinal);
        StringAssert.Contains(body, "% __enumerableIndexValue", StringComparison.Ordinal);
        Assert.IsFalse(body.Contains(".__jazor", StringComparison.Ordinal), body);
        Assert.IsFalse(body.Contains(".fromEnd", StringComparison.Ordinal), body);
        Assert.IsFalse(body.Contains(".value", StringComparison.Ordinal), body);

        var moduleImports = "import { " + string.Join(", ", importNames) + " } from \"" + imports[0].Key + "\";";
        var module = moduleImports + "\nexport function evaluate(releaseIds, index, includeLiteral) " + body;
        _ = new Parser().ParseModule(module);

        var root = Path.Combine(
            Path.GetTempPath(),
            "jazor-enumerable-element-at-or-default-index-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await MaterializeRuntimeCatalogAsync(root);
            var modulePath = Path.Combine(root, "element-at-or-default-index.mjs");
            var testPath = Path.Combine(root, "element-at-or-default-index.test.mjs");
            var configPath = Path.Combine(root, "deno.json");
            await System.IO.File.WriteAllTextAsync(
                modulePath,
                module,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await System.IO.File.WriteAllTextAsync(
                configPath,
                """
                {
                  "imports": {
                    "System/": "./System/"
                  }
                }
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await System.IO.File.WriteAllTextAsync(
                testPath,
                """
                import { evaluate } from "./element-at-or-default-index.mjs";
                import {
                  _1b0e1c2ab6c4cd39 as fromStart,
                  _ce8b9229a41c8545 as fromEnd
                } from "./System/IndexModule.js";

                function tracked(values, trace) {
                  return {
                    *[Symbol.iterator]() {
                      for (const value of values) {
                        trace.push(value);
                        yield value;
                      }
                    }
                  };
                }

                Deno.test("ElementAtOrDefault Index preserves from-start and from-end traversal", () => {
                  let trace = [];
                  if (evaluate(tracked([2, 7, 9], trace), fromStart(1), false) !== 14)
                    throw new Error("stored from-start Index or method-group dispatch returned the wrong item");
                  if (trace.join(",") !== "2,7,2,7")
                    throw new Error(`from-start Index did not stop at each target: ${trace.join(",")}`);

                  trace = [];
                  if (evaluate(tracked([2, 7, 9], trace), fromEnd(1), false) !== 18)
                    throw new Error("from-end Index did not return the requested tail item");
                  if (trace.join(",") !== "2,7,9,2,7,9")
                    throw new Error(`from-end Index did not observe each complete source: ${trace.join(",")}`);

                  trace = [];
                  if (evaluate(tracked([2, 7, 9], trace), fromEnd(2), false) !== 14)
                    throw new Error("the ring-buffer wrap did not preserve the second item from the end");
                  if (trace.join(",") !== "2,7,9,2,7,9")
                    throw new Error(`^2 traversal was ${trace.join(",")}`);

                  trace = [];
                  if (evaluate(tracked([2, 7, 9], trace), fromEnd(3), false) !== 4)
                    throw new Error("an Index equal to source length must return the first item");
                  if (trace.join(",") !== "2,7,9,2,7,9")
                    throw new Error(`^Count traversal was ${trace.join(",")}`);

                  trace = [];
                  if (evaluate(tracked([2, 7, 9], trace), fromStart(0), true) !== 13)
                    throw new Error("the ^1 literal did not retain its bound Index semantics");
                  if (trace.join(",") !== "2,2,2,7,9")
                    throw new Error(`the ^1 literal traversal was ${trace.join(",")}`);

                  trace = [];
                  if (evaluate(tracked([2, 7, 9], trace), fromEnd(0), false) !== 0 || trace.length !== 0)
                    throw new Error("Index.End must return default(int) without enumerating an unknown source");

                  trace = [];
                  if (evaluate(tracked([2, 7, 9], trace), fromEnd(4), false) !== 0)
                    throw new Error("an out-of-range from-end Index must return default(int)");
                  if (trace.join(",") !== "2,7,9,2,7,9")
                    throw new Error(`out-of-range from-end traversal was ${trace.join(",")}`);

                  trace = [];
                  if (evaluate(tracked([2, 7, 9], trace), fromStart(3), false) !== 0)
                    throw new Error("an out-of-range from-start Index must return default(int)");
                  if (trace.join(",") !== "2,7,9,2,7,9")
                    throw new Error(`out-of-range from-start traversal was ${trace.join(",")}`);

                  if (evaluate([], fromStart(0), false) !== 0 || evaluate([], fromEnd(1), false) !== 0)
                    throw new Error("empty Index paths must return default(int)");

                  let threw = false;
                  try {
                    evaluate(null, fromEnd(1), false);
                  } catch (error) {
                    threw = error instanceof TypeError && error.message === "source";
                  }
                  if (!threw)
                    throw new Error("a null source must fail before Index traversal");
                });
                """,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--config", configPath, "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static async Task MaterializeRuntimeCatalogAsync(string root)
    {
        // ECMAScript is the JS-resource carrier. Test hosts consume its manifest/dist package
        // directly; pure Jazor ModuleCatalog reflection is intentionally not involved here.
        var repositoryRoot = FindRepositoryRoot();
        var manifestPath = Path.Combine(repositoryRoot, "src", "ECMAScript", "manifest.json");
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(manifestPath));
        foreach (var entry in document.RootElement.GetProperty("imports").EnumerateObject())
        {
            var relativeFile = entry.Value.GetProperty("production").GetString()!;
            if (!relativeFile.StartsWith("dist/", StringComparison.Ordinal))
                continue;

            var relativePath = relativeFile["dist/".Length..];
            var sourcePath = Path.Combine(repositoryRoot, "src", "ECMAScript", relativeFile.Replace('/', Path.DirectorySeparatorChar));
            var content = await File.ReadAllTextAsync(sourcePath);
            var outputPath = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await System.IO.File.WriteAllTextAsync(
                outputPath,
                content,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;
        }

        throw new FileNotFoundException("Could not locate the Jazor repository root.");
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "EnumerableElementAtScenarios",
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
