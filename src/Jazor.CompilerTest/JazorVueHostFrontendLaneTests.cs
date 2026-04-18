using System.Text.Json;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.DevServer;
using Jazor.VueHost.Frontend.Deno.Hosting;
using Jazor.VueHost.Frontend.Deno.Protocol;
using Jazor.VueHost.Frontend;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Coordination;
using Jazor.VueHost.Lsp.Lanes;
using Jazor.VueHost.Lsp.Routing;
using Jazor.VueHost.Services;
using Jazor.VueHost.VirtualDocuments.Mapping;
using Jazor.VueHost.VirtualDocuments.Models;
using Jazor.VueHost.VirtualDocuments.Registry;
using Jazor.VueHost.Workspace;
using static Jazor.CompilerTest.SourceMapTestHelpers;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostFrontendLaneTests
{
    [TestMethod]
    public async Task JazorVueHost_DenoFrontendHost_CompileSfc_StartsWorkerAndReturnsTypedResult()
    {
        var workerProcess = new FakeDenoWorkerProcess();
        workerProcess.SetResult(
            "compile/sfc",
            new DenoSfcCompileResult
            {
                JsContent = "export default {};",
                JsSourceMap = """{"version":3}""",
                CssContent = ".counter{color:red;}",
                Diagnostics = [],
                SupportsHmr = true
            });
        var host = new DenoVolarHost(
            new DenoVolarHostOptions
            {
                Enabled = true,
                IgnoreStartupFailure = false
            },
            workerProcess);

        var result = await host.CompileSfcAsync(
            @"D:\temp\Counter.jazor",
            "<template><div /></template>",
            "Counter.jazor",
            CancellationToken.None);

        Assert.AreEqual(1, workerProcess.StartCallCount);
        Assert.IsNotNull(result);
        Assert.AreEqual("export default {};", result.JsContent);
        Assert.AreEqual("""{"version":3}""", result.JsSourceMap);
        Assert.AreEqual(".counter{color:red;}", result.CssContent);
        Assert.IsTrue(result.SupportsHmr);
        CollectionAssert.AreEqual(new[] { "compile/sfc" }, workerProcess.RequestMethods);
    }

    [TestMethod]
    public async Task JazorVueHost_DenoFrontendHost_CompileSfc_WhenWorkerCrashes_RetriesWithinSameRequestAndRecovers()
    {
        var workerProcess = new FakeDenoWorkerProcess();
        workerProcess.SetResult(
            "compile/sfc",
            new DenoSfcCompileResult
            {
                JsContent = "export default { ok: true };",
                JsSourceMap = """{"version":3}""",
                CssContent = ".ok{color:green;}",
                Diagnostics = [],
                SupportsHmr = true
            });
        workerProcess.SetFailure("compile/sfc", new InvalidOperationException("simulated worker crash"));

        var host = new DenoVolarHost(
            new DenoVolarHostOptions
            {
                Enabled = true,
                IgnoreStartupFailure = false
            },
            workerProcess);

        var recovered = await host.CompileSfcAsync(
            @"D:\temp\Counter.jazor",
            "<template><div /></template>",
            "Counter.jazor",
            CancellationToken.None);

        Assert.AreEqual(2, workerProcess.StartCallCount);
        Assert.AreEqual(1, workerProcess.StopCallCount);
        Assert.IsNotNull(recovered);
        Assert.AreEqual("export default { ok: true };", recovered.JsContent);
        CollectionAssert.AreEqual(
            new[] { "compile/sfc", "compile/sfc" },
            workerProcess.RequestMethods);
    }

    [TestMethod]
    public async Task JazorVueHost_DenoFrontendHost_CompileSfc_WithIgnoreStartupFailure_RetriesWithoutReturningFallback()
    {
        var workerProcess = new FakeDenoWorkerProcess();
        workerProcess.SetResult(
            "compile/sfc",
            new DenoSfcCompileResult
            {
                JsContent = "export default { recovered: true };",
                JsSourceMap = """{"version":3}""",
                CssContent = ".recovered{color:blue;}",
                Diagnostics = [],
                SupportsHmr = true
            });
        workerProcess.SetFailure("compile/sfc", new InvalidOperationException("simulated worker crash"));

        var host = new DenoVolarHost(
            new DenoVolarHostOptions
            {
                Enabled = true,
                IgnoreStartupFailure = true
            },
            workerProcess);

        var firstResult = await host.CompileSfcAsync(
            @"D:\temp\Counter.jazor",
            "<template><div /></template>",
            "Counter.jazor",
            CancellationToken.None);
        var secondResult = await host.CompileSfcAsync(
            @"D:\temp\Counter.jazor",
            "<template><div /></template>",
            "Counter.jazor",
            CancellationToken.None);

        Assert.IsNotNull(firstResult);
        Assert.IsNotNull(secondResult);
        Assert.AreEqual(2, workerProcess.StartCallCount);
        Assert.AreEqual(1, workerProcess.StopCallCount);
    }

    [TestMethod]
    public async Task JazorVueHost_DenoFrontendHost_TemplateCompletion_WhenWorkerCrashes_RetriesWithinSameRequestAndRecovers()
    {
        var workerProcess = new FakeDenoWorkerProcess();
        workerProcess.SetResult(
            "template/completion",
            new[]
            {
                new LspCompletionItem
                {
                    Label = "RecoveredTemplateItem",
                    Kind = 7,
                    Detail = "Recovered after worker restart."
                }
            });
        workerProcess.SetFailure("template/completion", new InvalidOperationException("simulated worker crash"));

        var host = new DenoVolarHost(
            new DenoVolarHostOptions
            {
                Enabled = true,
                IgnoreStartupFailure = false
            },
            workerProcess);

        var items = await host.GetTemplateCompletionItemsAsync(
            new DocumentSnapshot(
                @"D:\temp\App.vue",
                DocumentKind.Vue,
                "<template><App",
                "1"),
            new LspPosition { Line = 0, Character = 13 },
            context: null,
            CancellationToken.None);

        Assert.AreEqual(2, workerProcess.StartCallCount);
        Assert.AreEqual(1, workerProcess.StopCallCount);
        Assert.AreEqual(1, items.Count);
        Assert.AreEqual("RecoveredTemplateItem", items[0].Label);
        CollectionAssert.AreEqual(
            new[] { "template/completion", "template/completion" },
            workerProcess.RequestMethods);
    }

    [TestMethod]
    public async Task DenoFrontendModuleCompiler_CompileSfcAsync_PropagatesWorkerSourceMap()
    {
        var compiler = new DenoFrontendModuleCompiler(
            new FakeDenoFrontendHost
            {
                SfcCompileResult = new DenoSfcCompileResult
                {
                    JsContent = "export default {};",
                    JsSourceMap = """{"version":3}""",
                    CssContent = ".counter{color:red;}",
                    Diagnostics = [],
                    SupportsHmr = true
                }
            });

        var result = await compiler.CompileSfcAsync(
            @"D:\temp\Counter.vue",
            "<template><div /></template>",
            CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual("export default {};", result.JavaScript);
        Assert.AreEqual("""{"version":3}""", result.SourceMap);
        Assert.AreEqual(".counter{color:red;}", result.StyleContent);
        Assert.IsTrue(result.SupportsHmr);
    }

    [TestMethod]
    public async Task JazorVueHost_DenoFrontendHost_CompileSfcAsync_WithBundledWorker_ReturnsCompiledVueModuleAndColumnAwareSourceMap()
    {
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var documentPath = Path.Combine(tempDirectory, "App.vue");
            const string sfcText = """
                <template>
                  <div class="app">{{ count + 1 }}</div>
                </template>
                <script setup lang="ts">
                const count: number = 1;
                const typedLabel: string = `count:${count}`;
                enum MarkerState {
                  Active = 1
                }
                const buildVersion: number = 1;
                function runIteration(): number {
                  const marker = buildVersion + MarkerState.Active;
                  return marker;
                }
                </script>
                <style>
                .app {
                  color: red;
                }
                </style>
                """;
            await File.WriteAllTextAsync(documentPath, sfcText);

            await using var host = CreateBundledDenoFrontendHost();

            var result = await host.CompileSfcAsync(
                documentPath,
                sfcText,
                Path.GetFileName(documentPath),
                CancellationToken.None);

            Assert.IsNotNull(result);
            StringAssert.Contains(result.JsContent, "export default _sfc_main;");
            StringAssert.Contains(result.CssContent!, "color: red");
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.JsSourceMap));

            using var sourceMapDocument = JsonDocument.Parse(result.JsSourceMap!);
            var sourceMap = sourceMapDocument.RootElement;
            var sourceIndex = FindSourceIndexContaining(sourceMap, Path.GetFileName(documentPath));
            var segments = DecodeSegments(sourceMap)
                .Where(segment => segment.SourceIndex == sourceIndex)
                .ToArray();
            var typedLabelGeneratedPosition = GetLineColumnContaining(result.JsContent, "typedLabel");
            var typedLabelSourcePosition = GetLineColumnContaining(sfcText, "typedLabel");
            var templateExpressionSourceLine = GetLineIndexContaining(sfcText, "{{ count + 1 }}");
            var templateSegments = segments
                .Where(segment => segment.SourceLine == templateExpressionSourceLine)
                .ToArray();
            var markerExpressionSourceLine = GetLineIndexContaining(sfcText, "const marker = buildVersion + MarkerState.Active;");
            var markerStateSourcePosition = GetLineColumnContaining(sfcText, "MarkerState.Active");
            var markerExpressionSegments = segments
                .Where(segment => segment.SourceLine == markerExpressionSourceLine)
                .ToArray();

            Assert.AreEqual(Path.GetFileName(documentPath), sourceMap.GetProperty("sources")[sourceIndex].GetString());
            Assert.AreEqual(sfcText, sourceMap.GetProperty("sourcesContent")[sourceIndex].GetString());
            Assert.IsTrue(
                segments.Any(segment =>
                    segment.GeneratedLine == typedLabelGeneratedPosition.Line &&
                    segment.GeneratedColumn == typedLabelGeneratedPosition.Column &&
                    segment.SourceLine == typedLabelSourcePosition.Line &&
                    segment.SourceColumn == typedLabelSourcePosition.Column),
                "Expected script-setup sourcemap to preserve the typedLabel token column mapping back to the original Vue file.");
            Assert.IsTrue(
                templateSegments.Length >= 2,
                "Expected template render output to retain multiple source-map segments for the original template expression line.");
            Assert.IsTrue(
                templateSegments.Any(segment => segment.SourceColumn > 0),
                "Expected template source-map segments to preserve non-zero source columns.");
            Assert.IsTrue(
                markerExpressionSegments.Select(segment => segment.SourceColumn).Distinct().Count(static column => column > 0) >= 2,
                "Expected transpiled script sourcemap chaining to preserve multiple non-zero source columns for complex expression lines.");
            Assert.IsTrue(
                markerExpressionSegments.Any(segment => segment.SourceColumn >= markerStateSourcePosition.Column),
                "Expected sourcemap chaining to retain MarkerState.Active token column on the authored script line.");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetry(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_DenoFrontendHost_CompileTypeScript_StartsWorkerAndReturnsTypedResult()
    {
        var workerProcess = new FakeDenoWorkerProcess();
        workerProcess.SetResult(
            "compile/ts",
            new DenoTypeScriptCompileResult
            {
                JsContent = "export const count = 1;",
                JsSourceMap = """{"version":3}""",
                Diagnostics = []
            });
        var host = new DenoVolarHost(
            new DenoVolarHostOptions
            {
                Enabled = true,
                IgnoreStartupFailure = false
            },
            workerProcess);

        var result = await host.CompileTypeScriptAsync(
            @"D:\temp\counter.ts",
            "export const count: number = 1;",
            "counter.ts",
            CancellationToken.None);

        Assert.AreEqual(1, workerProcess.StartCallCount);
        Assert.IsNotNull(result);
        Assert.AreEqual("export const count = 1;", result.JsContent);
        Assert.AreEqual("""{"version":3}""", result.JsSourceMap);
        CollectionAssert.AreEqual(new[] { "compile/ts" }, workerProcess.RequestMethods);
    }

    [TestMethod]
    public async Task JazorVueHost_DenoFrontendHost_GetTemplateDiagnostics_StartsWorkerAndReturnsTypedDiagnostics()
    {
        var workerProcess = new FakeDenoWorkerProcess();
        workerProcess.SetResult(
            "template/diagnostics",
            new[]
            {
                new LspDiagnostic
                {
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 1 },
                        End = new LspPosition { Line = 0, Character = 12 }
                    },
                    Severity = 2,
                    Code = "JAZORVUEFRONTEND001",
                    Source = "Jazor.VueHost.Frontend",
                    Message = "Razor component 'MissingCard' could not be resolved to a nearby Vue file."
                }
            });
        var host = new DenoVolarHost(
            new DenoVolarHostOptions
            {
                Enabled = true,
                IgnoreStartupFailure = false
            },
            workerProcess);

        var diagnostics = await host.GetTemplateDiagnosticsAsync(CreateDocument("""
            <template>
              <MissingCard />
            </template>
            """), null, CancellationToken.None);

        Assert.AreEqual(1, workerProcess.StartCallCount);
        Assert.IsTrue(diagnostics.Any(static diagnostic => diagnostic.Code == "JAZORVUEFRONTEND001"));
        CollectionAssert.AreEqual(new[] { "template/diagnostics" }, workerProcess.RequestMethods);
    }

    [TestMethod]
    public async Task JazorVueHost_DenoFrontendHost_GetTemplateDocumentSymbols_StartsWorkerAndReturnsTypedSymbols()
    {
        var workerProcess = new FakeDenoWorkerProcess();
        workerProcess.SetResult(
            "template/documentSymbols",
            new[]
            {
                new LspDocumentSymbol
                {
                    Name = "Template",
                    Kind = 2,
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 0 },
                        End = new LspPosition { Line = 2, Character = 11 }
                    },
                    SelectionRange = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 0 },
                        End = new LspPosition { Line = 0, Character = 10 }
                    },
                    Children =
                    [
                        new LspDocumentSymbol
                        {
                            Name = "UserCard",
                            Kind = 5,
                            Range = new LspRange
                            {
                                Start = new LspPosition { Line = 1, Character = 3 },
                                End = new LspPosition { Line = 1, Character = 11 }
                            },
                            SelectionRange = new LspRange
                            {
                                Start = new LspPosition { Line = 1, Character = 3 },
                                End = new LspPosition { Line = 1, Character = 11 }
                            }
                        }
                    ]
                }
            });
        var host = new DenoVolarHost(
            new DenoVolarHostOptions
            {
                Enabled = true,
                IgnoreStartupFailure = false
            },
            workerProcess);

        var symbols = await host.GetTemplateDocumentSymbolsAsync(
            new DocumentSnapshot(
                @"D:\temp\Host.vue",
                DocumentKind.Vue,
                """
                <template>
                  <UserCard />
                </template>
                """,
                "1"),
            null,
            CancellationToken.None);

        Assert.AreEqual(1, workerProcess.StartCallCount);
        Assert.AreEqual(1, symbols.Count);
        Assert.AreEqual("Template", symbols[0].Name);
        Assert.IsNotNull(symbols[0].Children);
        Assert.AreEqual("UserCard", symbols[0].Children![0].Name);
        CollectionAssert.AreEqual(new[] { "template/documentSymbols" }, workerProcess.RequestMethods);
    }

    [TestMethod]
    public async Task JazorVueHost_DenoFrontendHost_GetTemplateSemanticTokens_StartsWorkerAndReturnsTypedTokens()
    {
        var workerProcess = new FakeDenoWorkerProcess();
        workerProcess.SetResult(
            "template/semanticTokens",
            new[]
            {
                new LspSemanticToken
                {
                    Line = 1,
                    Character = 3,
                    Length = 8,
                    TokenType = "class",
                    TokenModifiers = []
                }
            });
        var host = new DenoVolarHost(
            new DenoVolarHostOptions
            {
                Enabled = true,
                IgnoreStartupFailure = false
            },
            workerProcess);

        var tokens = await host.GetTemplateSemanticTokensAsync(
            new DocumentSnapshot(
                @"D:\temp\Host.vue",
                DocumentKind.Vue,
                """
                <template>
                  <UserCard />
                </template>
                """,
                "1"),
            null,
            CancellationToken.None);

        Assert.AreEqual(1, workerProcess.StartCallCount);
        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual("class", tokens[0].TokenType);
        CollectionAssert.AreEqual(new[] { "template/semanticTokens" }, workerProcess.RequestMethods);
    }

    [TestMethod]
    public async Task JazorVueHost_DenoFrontendHost_TypeScriptDocument_ReturnsSameFileScriptSymbolResults()
    {
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, "counter.ts"),
                DocumentKind.TypeScript,
                """
                const count = 1;

                function increment(step: number) {
                  return count + step;
                }

                const snapshot = count;

                inc
                """,
                "1");
            await File.WriteAllTextAsync(document.DocumentPath, document.Text);

            await using var host = CreateBundledDenoFrontendHost();

            var completionItems = await host.GetTemplateCompletionItemsAsync(
                document,
                GetLastPosition(document.Text, "inc", advance: "inc".Length),
                null,
                CancellationToken.None);
            CollectionAssert.Contains(completionItems.Select(static item => item.Label).ToArray(), "increment");

            var hoverPosition = GetPosition(document.Text, "return count + step;", advance: "return ".Length + 1);
            var hover = await host.GetTemplateHoverAsync(document, hoverPosition, null, CancellationToken.None);
            Assert.IsNotNull(hover);
            StringAssert.Contains(hover.Contents.Value, "count");
            StringAssert.Contains(hover.Contents.Value, "const");

            var definitions = await host.GetTemplateDefinitionAsync(document, hoverPosition, null, CancellationToken.None);
            Assert.AreEqual(1, definitions.Count);
            Assert.AreEqual(LspProtocolHelpers.ToDocumentUri(document.DocumentPath), definitions[0].Uri);
            Assert.AreEqual(0, definitions[0].Range.Start.Line);

            var references = await host.GetTemplateReferencesAsync(document, hoverPosition, includeDeclaration: true, null, CancellationToken.None);
            Assert.AreEqual(3, references.Count);
            Assert.IsTrue(references.All(static location => location.Uri.StartsWith("file:///", StringComparison.Ordinal)));
            Assert.IsTrue(references.All(location => location.Uri == LspProtocolHelpers.ToDocumentUri(document.DocumentPath)));

            var rename = await host.GetTemplateRenameAsync(document, hoverPosition, "total", null, CancellationToken.None);
            Assert.IsNotNull(rename);
            Assert.IsTrue(rename.Changes.ContainsKey(LspProtocolHelpers.ToDocumentUri(document.DocumentPath)));
            Assert.AreEqual(3, rename.Changes[LspProtocolHelpers.ToDocumentUri(document.DocumentPath)].Length);
            Assert.IsTrue(rename.Changes[LspProtocolHelpers.ToDocumentUri(document.DocumentPath)]
                .All(static edit => edit.NewText == "total"));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetry(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_DenoFrontendHost_VueScriptSetupDocument_ReturnsSameFileScriptSymbolResults()
    {
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Host.vue"),
                DocumentKind.Vue,
                """
                <template>
                  <div>{{ count }}</div>
                </template>
                <script setup lang="ts">
                const count = 1;

                function increment(step: number) {
                  return count + step;
                }

                const next = increment(count);

                inc
                </script>
                """,
                "1");
            await File.WriteAllTextAsync(document.DocumentPath, document.Text);

            await using var host = CreateBundledDenoFrontendHost();

            var completionItems = await host.GetTemplateCompletionItemsAsync(
                document,
                GetLastPosition(document.Text, "inc", advance: "inc".Length),
                null,
                CancellationToken.None);
            CollectionAssert.Contains(completionItems.Select(static item => item.Label).ToArray(), "increment");

            var usagePosition = GetPosition(document.Text, "increment(count)", advance: 1);
            var hover = await host.GetTemplateHoverAsync(document, usagePosition, null, CancellationToken.None);
            Assert.IsNotNull(hover);
            StringAssert.Contains(hover.Contents.Value, "increment");
            StringAssert.Contains(hover.Contents.Value, "function");

            var definitions = await host.GetTemplateDefinitionAsync(document, usagePosition, null, CancellationToken.None);
            Assert.AreEqual(1, definitions.Count);
            Assert.AreEqual(LspProtocolHelpers.ToDocumentUri(document.DocumentPath), definitions[0].Uri);
            Assert.AreEqual(6, definitions[0].Range.Start.Line);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetry(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_DenoFrontendHost_VueDocument_UsesVolarSyntaxDiagnostics()
    {
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Broken.vue"),
                DocumentKind.Vue,
                """
                <template><div></template>
                """,
                "1");
            await File.WriteAllTextAsync(document.DocumentPath, document.Text);

            await using var host = CreateBundledDenoFrontendHost();

            var diagnostics = await host.GetTemplateDiagnosticsAsync(document, null, CancellationToken.None);

            Assert.IsTrue(
                diagnostics.Any(static diagnostic =>
                    string.Equals(diagnostic.Source, "vue", StringComparison.OrdinalIgnoreCase)
                    || diagnostic.Message.Contains("missing end tag", StringComparison.OrdinalIgnoreCase)),
                "Expected Vue/Volar syntax diagnostics for malformed template markup.");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetry(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_DenoFrontendHost_JazorDocument_UsesFrontendMetadataForDiagnostics()
    {
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var componentPath = Path.Combine(tempDirectory, "Shared", "UserCard.vue");
            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Counter.jazor"),
                DocumentKind.Jazor,
                """
                <UserCard />
                """,
                "1");
            var context = CreateFrontendIntelliSenseContext(componentPath);

            await using var host = CreateBundledDenoFrontendHost();

            var diagnostics = await host.GetTemplateDiagnosticsAsync(document, context, CancellationToken.None);
            Assert.IsFalse(diagnostics.Any(static diagnostic => diagnostic.Code == "JAZORVUEFRONTEND001"));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetry(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_DenoFrontendHost_TypeScriptDocument_ResolvesRelativeImportSymbolsConservatively()
    {
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var importedPath = Path.Combine(tempDirectory, "label.ts");
            await File.WriteAllTextAsync(
                importedPath,
                """
                export function formatLabel(value: number) {
                  return value.toString();
                }
                """);

            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, "counter.ts"),
                DocumentKind.TypeScript,
                """
                import { formatLabel } from "./label";

                const label = formatLabel(1);
                """,
                "1");
            await File.WriteAllTextAsync(document.DocumentPath, document.Text);

            await using var host = CreateBundledDenoFrontendHost();

            var usagePosition = GetPosition(document.Text, "formatLabel(1)", advance: 1);
            var hover = await host.GetTemplateHoverAsync(document, usagePosition, null, CancellationToken.None);
            Assert.IsNotNull(hover);
            StringAssert.Contains(hover.Contents.Value, "function formatLabel(value: number)");
            StringAssert.Contains(hover.Contents.Value, "./label.ts");

            var definitions = await host.GetTemplateDefinitionAsync(document, usagePosition, null, CancellationToken.None);
            Assert.AreEqual(1, definitions.Count);
            Assert.AreEqual(LspProtocolHelpers.ToDocumentUri(importedPath), definitions[0].Uri);
            Assert.AreEqual(0, definitions[0].Range.Start.Line);

            var references = await host.GetTemplateReferencesAsync(document, usagePosition, includeDeclaration: true, null, CancellationToken.None);
            Assert.AreEqual(3, references.Count);
            Assert.IsTrue(references.Any(location => location.Uri == LspProtocolHelpers.ToDocumentUri(importedPath)));
            Assert.AreEqual(
                2,
                references.Count(location => location.Uri == LspProtocolHelpers.ToDocumentUri(document.DocumentPath)));

            var rename = await host.GetTemplateRenameAsync(document, usagePosition, "renderLabel", null, CancellationToken.None);
            Assert.IsNull(rename);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetry(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_DenoFrontendHost_TypeScriptDocument_ResolvesReExportedAliasAndDefaultImportSymbolsConservatively()
    {
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var importedPath = Path.Combine(tempDirectory, "format.ts");
            await File.WriteAllTextAsync(
                importedPath,
                """
                export function formatLabel(value: number) {
                  return value.toString();
                }
                """);

            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "label.ts"),
                """
                import { formatLabel } from "./format";

                export { formatLabel as renderLabel };
                export default formatLabel;
                """);

            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, "counter.ts"),
                DocumentKind.TypeScript,
                """
                import formatLabel, { renderLabel } from "./label";

                const direct = formatLabel(1);
                const aliased = renderLabel(2);
                """,
                "1");
            await File.WriteAllTextAsync(document.DocumentPath, document.Text);

            await using var host = CreateBundledDenoFrontendHost();

            var defaultUsagePosition = GetPosition(document.Text, "formatLabel(1)", advance: 1);
            var defaultHover = await host.GetTemplateHoverAsync(document, defaultUsagePosition, null, CancellationToken.None);
            Assert.IsNotNull(defaultHover);
            StringAssert.Contains(defaultHover.Contents.Value, "function formatLabel(value: number)");
            StringAssert.Contains(defaultHover.Contents.Value, "./format.ts");

            var defaultDefinitions = await host.GetTemplateDefinitionAsync(document, defaultUsagePosition, null, CancellationToken.None);
            Assert.AreEqual(1, defaultDefinitions.Count);
            Assert.AreEqual(LspProtocolHelpers.ToDocumentUri(importedPath), defaultDefinitions[0].Uri);
            Assert.AreEqual(0, defaultDefinitions[0].Range.Start.Line);

            var defaultRename = await host.GetTemplateRenameAsync(document, defaultUsagePosition, "formatMessage", null, CancellationToken.None);
            Assert.IsNotNull(defaultRename);
            Assert.AreEqual(
                2,
                defaultRename.Changes[LspProtocolHelpers.ToDocumentUri(document.DocumentPath)].Length);

            var aliasUsagePosition = GetPosition(document.Text, "renderLabel(2)", advance: 1);
            var aliasHover = await host.GetTemplateHoverAsync(document, aliasUsagePosition, null, CancellationToken.None);
            Assert.IsNotNull(aliasHover);
            StringAssert.Contains(aliasHover.Contents.Value, "function formatLabel(value: number)");
            StringAssert.Contains(aliasHover.Contents.Value, "./format.ts");

            var aliasDefinitions = await host.GetTemplateDefinitionAsync(document, aliasUsagePosition, null, CancellationToken.None);
            Assert.AreEqual(1, aliasDefinitions.Count);
            Assert.AreEqual(LspProtocolHelpers.ToDocumentUri(importedPath), aliasDefinitions[0].Uri);
            Assert.AreEqual(0, aliasDefinitions[0].Range.Start.Line);

            var aliasReferences = await host.GetTemplateReferencesAsync(document, aliasUsagePosition, includeDeclaration: true, null, CancellationToken.None);
            Assert.IsTrue(aliasReferences.Count >= 4);
            Assert.IsTrue(aliasReferences.Any(location => location.Uri == LspProtocolHelpers.ToDocumentUri(importedPath)));
            Assert.AreEqual(
                2,
                aliasReferences.Count(location => location.Uri == LspProtocolHelpers.ToDocumentUri(document.DocumentPath)));

            var aliasRename = await host.GetTemplateRenameAsync(document, aliasUsagePosition, "renderMessage", null, CancellationToken.None);
            Assert.IsNull(aliasRename);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetry(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_DenoFrontendHost_TypeScriptDocument_UsesBundledTypeScriptServiceForImportedMemberCompletionAndHover()
    {
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "palette.ts"),
                """
                export function createPalette() {
                  return {
                    primary: "#ffffff",
                    secondary: "#000000",
                  };
                }
                """);

            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, "counter.ts"),
                DocumentKind.TypeScript,
                """
                import { createPalette } from "./palette";

                const palette = createPalette();
                const swatch = palette.primary;

                palette.pr
                """,
                "1");
            await File.WriteAllTextAsync(document.DocumentPath, document.Text);

            await using var host = CreateBundledDenoFrontendHost();

            var completionItems = await host.GetTemplateCompletionItemsAsync(
                document,
                GetLastPosition(document.Text, "palette.pr", advance: "palette.pr".Length),
                null,
                CancellationToken.None);
            CollectionAssert.Contains(completionItems.Select(static item => item.Label).ToArray(), "primary");

            var hover = await host.GetTemplateHoverAsync(
                document,
                GetPosition(document.Text, "primary;", advance: 1),
                null,
                CancellationToken.None);
            Assert.IsNotNull(hover);
            StringAssert.Contains(hover.Contents.Value, "primary");
            StringAssert.Contains(hover.Contents.Value, "string");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetry(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetDiagnostics_DeduplicatesMatchingDenoDiagnostics()
    {
        var lane = CreateLane(new FakeDenoFrontendHost
        {
            Diagnostics = new[]
            {
                new LspDiagnostic
                {
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 1 },
                        End = new LspPosition { Line = 0, Character = 12 }
                    },
                    Severity = 2,
                    Code = "JAZORVUEFRONTEND001",
                    Source = "Jazor.VueHost.Frontend",
                    Message = "Razor component 'MissingCard' could not be resolved to a nearby Vue file."
                }
            }
        });

        var diagnostics = await lane.GetDiagnosticsAsync(CreateDocument("""
            <MissingCard />
            """), CancellationToken.None);

        Assert.IsTrue(diagnostics.Any(static diagnostic => diagnostic.Code == "JAZORVUEFRONTEND001"));
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetDiagnostics_DoesNotRequireLegacyVueImportWhenNearbyComponentExists()
    {
        var lane = CreateLane(new FakeDenoFrontendHost());
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var componentsDirectory = Path.Combine(tempDirectory, "Components");
            Directory.CreateDirectory(componentsDirectory);
            await File.WriteAllTextAsync(
                Path.Combine(componentsDirectory, "MissingCard.vue"),
                "<template><div /></template>");

            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Counter.jazor"),
                DocumentKind.Jazor,
                """
                <MissingCard />
                """,
                "1");

            var diagnostics = await lane.GetDiagnosticsAsync(document, CancellationToken.None);

            Assert.AreEqual(0, diagnostics.Count);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetry(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetCompletionItems_ReturnsEmptyWhenDenoReturnsEmpty()
    {
        var lane = CreateLane(new FakeDenoFrontendHost());
        var document = CreateDocument("""
            <
            """);

        var items = await lane.GetCompletionItemsAsync(
            document,
            new LspPosition { Line = 0, Character = 1 },
            CreateTemplateTarget(document),
            CancellationToken.None);

        Assert.AreEqual(0, items.Count);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetHover_ReturnsNullWhenDenoReturnsNull()
    {
        var lane = CreateLane(new FakeDenoFrontendHost());
        var document = CreateDocument("""
            <UserCard />
            """);

        var hover = await lane.GetHoverAsync(
            document,
            new LspPosition { Line = 0, Character = 2 },
            CreateTemplateTarget(document),
            CancellationToken.None);

        Assert.IsNull(hover);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetCompletionItems_PrefersDenoResults()
    {
        var lane = CreateLane(new FakeDenoFrontendHost
        {
            CompletionItems = new[]
            {
                new LspCompletionItem
                {
                    Label = "DenoOnlyCard",
                    Kind = 7,
                    Detail = "./DenoOnlyCard.vue",
                    Documentation = "Provided by the Deno worker."
                }
            }
        });
        var document = CreateDocument("""
            <
            """);

        var items = await lane.GetCompletionItemsAsync(
            document,
            new LspPosition { Line = 0, Character = 1 },
            CreateTemplateTarget(document),
            CancellationToken.None);

        Assert.AreEqual(1, items.Count);
        Assert.AreEqual("DenoOnlyCard", items[0].Label);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task JazorVueHost_FrontendLaneService_RecordsDenoFailureSnapshot_WhenCompletionThrows()
    {
        VolarLaneService.ResetDenoFailureSnapshotsForTests();
        try
        {
            var denoHost = new FakeDenoFrontendHost();
            denoHost.SetFailure("completion", new InvalidOperationException("simulated completion failure"));
            var lane = CreateLane(denoHost);
            var document = CreateDocument("<");

            var items = await lane.GetCompletionItemsAsync(
                document,
                new LspPosition { Line = 0, Character = 1 },
                CreateTemplateTarget(document),
                CancellationToken.None);

            Assert.AreEqual(0, items.Count);
            var snapshot = VolarLaneService
                .GetDenoFailureSnapshots()
                .SingleOrDefault(static entry => string.Equals(entry.Operation, "completion", StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(snapshot);
            Assert.AreEqual(1, snapshot.FailureCount);
            StringAssert.Contains(snapshot.LastErrorMessage, "simulated completion failure");
        }
        finally
        {
            VolarLaneService.ResetDenoFailureSnapshotsForTests();
        }
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task JazorVueHost_FrontendLaneService_Cancellation_DoesNotRecordDenoFailureSnapshot()
    {
        VolarLaneService.ResetDenoFailureSnapshotsForTests();
        try
        {
            var lane = CreateLane(new FakeDenoFrontendHost());
            var document = CreateDocument("<");
            using var cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();

            var canceled = false;
            try
            {
                await lane.GetCompletionItemsAsync(
                    document,
                    new LspPosition { Line = 0, Character = 1 },
                    CreateTemplateTarget(document),
                    cancellationSource.Token);
            }
            catch (OperationCanceledException)
            {
                canceled = true;
            }

            Assert.IsTrue(canceled, "Expected completion request to honor cancellation.");

            Assert.AreEqual(0, VolarLaneService.GetDenoFailureSnapshots().Count);
        }
        finally
        {
            VolarLaneService.ResetDenoFailureSnapshotsForTests();
        }
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetCompletionItems_UsesWorkspaceGraphWhenDenoEnabledButTemporarilyNotRunning()
    {
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var componentPath = Path.Combine(tempDirectory, "UserCard.vue");
            await File.WriteAllTextAsync(componentPath, "<template><div>UserCard</div></template>");

            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(documentPath, "<");
            var document = new DocumentSnapshot(
                documentPath,
                DocumentKind.Jazor,
                "<",
                "1");

            var lane = CreateLane(new FakeDenoFrontendHost
            {
                IsEnabled = true,
                IsRunning = false
            });

            var items = await lane.GetCompletionItemsAsync(
                document,
                new LspPosition { Line = 0, Character = 1 },
                CreateTemplateTarget(document),
                CancellationToken.None);
            var labels = items
                .Select(static item => item.Label)
                .ToArray();

            CollectionAssert.Contains(labels, "UserCard");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetry(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetDocumentSymbols_PrefersDenoResults()
    {
        var lane = CreateLane(new FakeDenoFrontendHost
        {
            DocumentSymbols =
            [
                new LspDocumentSymbol
                {
                    Name = "Template",
                    Kind = 2,
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 0 },
                        End = new LspPosition { Line = 2, Character = 11 }
                    },
                    SelectionRange = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 0 },
                        End = new LspPosition { Line = 0, Character = 10 }
                    }
                }
            ]
        });
        var document = new DocumentSnapshot(
            @"D:\temp\Host.vue",
            DocumentKind.Vue,
            """
            <template>
              <UserCard />
            </template>
            """,
            "1");

        var symbols = await lane.GetDocumentSymbolsAsync(document, CancellationToken.None);

        Assert.AreEqual(1, symbols.Count);
        Assert.AreEqual("Template", symbols[0].Name);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetSemanticTokens_PrefersDenoResults()
    {
        var lane = CreateLane(new FakeDenoFrontendHost
        {
            SemanticTokens =
            [
                new LspSemanticToken
                {
                    Line = 1,
                    Character = 3,
                    Length = 8,
                    TokenType = "class",
                    TokenModifiers = []
                }
            ]
        });
        var document = new DocumentSnapshot(
            @"D:\temp\Host.vue",
            DocumentKind.Vue,
            """
            <template>
              <UserCard />
            </template>
            """,
            "1");

        var tokens = await lane.GetSemanticTokensAsync(document, CancellationToken.None);

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual("class", tokens[0].TokenType);
        Assert.AreEqual(8, tokens[0].Length);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_UsesFrontendContextForJazorTemplateRequests()
    {
        var denoHost = new FakeDenoFrontendHost
        {
            HoverResult = new LspHoverResult
            {
                Contents = new LspMarkupContent
                {
                    Kind = "markdown",
                    Value = "metadata"
                },
                Range = new LspRange
                {
                    Start = new LspPosition { Line = 0, Character = 1 },
                    End = new LspPosition { Line = 0, Character = 9 }
                }
            }
        };
        var document = CreateDocument("""
            <UserCard />
            """);
        var frontendContextProvider = new FakeFrontendContextProvider(
            new GetFrontendContextResponse(
                new SemanticContext(
                    "frontend",
                    [
                        new DocumentSnapshot(
                            @"D:\temp\Components\UserCard.vue",
                            DocumentKind.Vue,
                            "<template><div /></template>",
                            "1")
                    ],
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["relatedDocumentCount"] = "1",
                        ["derivedDocumentCount"] = "1"
                    }),
                [
                    new ArtifactRecord(
                        artifactName: "virtual:D:/temp/Counter.jazor.frontend-summary.json",
                        artifactKind: "frontend-summary",
                        content: """{"documentPath":"D:/temp/Components/UserCard.vue","documentKind":"Vue","referencedComponents":["UserCard"]}""",
                        contentHash: null)
                ]));
        var lane = CreateLane(denoHost, frontendContextProvider);

        var hover = await lane.GetHoverAsync(
            document,
            new LspPosition { Line = 0, Character = 2 },
            new ProjectionTarget(
                LaneKind.Volar,
                DocumentRegionKind.Template,
                document.DocumentPath,
                document.DocumentPath,
                new LspPosition { Line = 0, Character = 2 },
                IsProjected: false),
            CancellationToken.None);

        Assert.IsNotNull(hover);
        Assert.IsNotNull(denoHost.LastDocument);
        Assert.AreEqual(document.DocumentPath, denoHost.LastDocument.DocumentPath);
        Assert.AreEqual(document.Text, denoHost.LastDocument.Text);
        Assert.IsNotNull(denoHost.LastContext);
        Assert.AreEqual("frontend", denoHost.LastContext.SemanticContext.ContextKind);
        Assert.AreEqual(1, denoHost.LastContext.SemanticContext.RelatedDocuments.Count);
        Assert.AreEqual("frontend-summary", denoHost.LastContext.Artifacts[0].ArtifactKind);
        Assert.AreEqual(0, hover.Range!.Start.Line);
        Assert.AreEqual(1, hover.Range.Start.Character);
        Assert.AreEqual(9, hover.Range.End.Character);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_UsesPrimaryProjectedVueDocument_ForJazorTemplateRequests()
    {
        var denoHost = new FakeDenoFrontendHost
        {
            HoverResult = new LspHoverResult
            {
                Contents = new LspMarkupContent
                {
                    Kind = "markdown",
                    Value = "projected"
                },
                Range = new LspRange
                {
                    Start = new LspPosition { Line = 0, Character = 1 },
                    End = new LspPosition { Line = 0, Character = 9 }
                }
            }
        };
        var document = CreateDocument("""
            <UserCard />
            """);
        var registry = new InMemoryVirtualDocumentRegistry();
        var projectedPath = "virtual:" + document.DocumentPath + ".g.vue";
        await registry.UpsertAsync(
        [
            new VirtualDocument(
                new VirtualDocumentIdentity(
                    document.DocumentPath,
                    projectedPath,
                    VirtualDocumentKind.Vue),
                document.Text,
                ProjectionMap.CreateWholeDocument(document.DocumentPath, projectedPath, document.Text.Length, document.Text.Length),
                "1"),
            new VirtualDocument(
                new VirtualDocumentIdentity(
                    document.DocumentPath,
                    "virtual:" + document.DocumentPath + ".template-only.vue",
                    VirtualDocumentKind.Vue),
                "<template><FallbackOnly /></template>",
                ProjectionMap.CreateWholeDocument(document.DocumentPath, "virtual:" + document.DocumentPath + ".template-only.vue", document.Text.Length, "<template><FallbackOnly /></template>".Length),
                "1")
        ],
            CancellationToken.None);

        var lane = CreateLane(denoHost, virtualDocumentRegistry: registry);

        var hover = await lane.GetHoverAsync(
            document,
            new LspPosition { Line = 0, Character = 2 },
            CreateTemplateTarget(document),
            CancellationToken.None);

        Assert.IsNotNull(hover);
        Assert.IsNotNull(denoHost.LastDocument);
        Assert.AreEqual(projectedPath, denoHost.LastDocument.DocumentPath);
        Assert.AreEqual(DocumentKind.Vue, denoHost.LastDocument.DocumentKind);
        Assert.AreEqual(document.Text, denoHost.LastDocument.Text);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_FallsBackToSource_WhenPrimaryProjectedVueDocumentIsMissing()
    {
        var denoHost = new FakeDenoFrontendHost
        {
            HoverResult = new LspHoverResult
            {
                Contents = new LspMarkupContent
                {
                    Kind = "markdown",
                    Value = "fallback"
                },
                Range = new LspRange
                {
                    Start = new LspPosition { Line = 0, Character = 1 },
                    End = new LspPosition { Line = 0, Character = 9 }
                }
            }
        };
        var document = CreateDocument("""
            <UserCard />
            """);
        var registry = new InMemoryVirtualDocumentRegistry();
        await registry.UpsertAsync(
        [
            new VirtualDocument(
                new VirtualDocumentIdentity(
                    document.DocumentPath,
                    "virtual:" + document.DocumentPath + ".template-only.vue",
                    VirtualDocumentKind.Vue),
                "<template><FallbackOnly /></template>",
                ProjectionMap.CreateWholeDocument(document.DocumentPath, "virtual:" + document.DocumentPath + ".template-only.vue", document.Text.Length, "<template><FallbackOnly /></template>".Length),
                "1")
        ],
            CancellationToken.None);

        var lane = CreateLane(denoHost, virtualDocumentRegistry: registry);

        var hover = await lane.GetHoverAsync(
            document,
            new LspPosition { Line = 0, Character = 2 },
            CreateTemplateTarget(document),
            CancellationToken.None);

        Assert.IsNotNull(hover);
        Assert.IsNotNull(denoHost.LastDocument);
        Assert.AreEqual(document.DocumentPath, denoHost.LastDocument.DocumentPath);
        Assert.AreEqual(DocumentKind.Jazor, denoHost.LastDocument.DocumentKind);
        Assert.AreEqual(document.Text, denoHost.LastDocument.Text);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_UsesPrimaryProjectedVueDocument_ForJazorTemplateRequestMatrix()
    {
        var document = CreateDocument("""
            <UserCard />
            """);
        var projectedPath = "virtual:" + document.DocumentPath + ".g.vue";
        var denoHost = CreateTemplateRequestMatrixHost(projectedPath);
        var registry = new InMemoryVirtualDocumentRegistry();
        await registry.UpsertAsync(
        [
            new VirtualDocument(
                new VirtualDocumentIdentity(
                    document.DocumentPath,
                    projectedPath,
                    VirtualDocumentKind.Vue),
                document.Text,
                ProjectionMap.CreateWholeDocument(document.DocumentPath, projectedPath, document.Text.Length, document.Text.Length),
                "1"),
            new VirtualDocument(
                new VirtualDocumentIdentity(
                    document.DocumentPath,
                    "virtual:" + document.DocumentPath + ".template-only.vue",
                    VirtualDocumentKind.Vue),
                "<template><FallbackOnly /></template>",
                ProjectionMap.CreateWholeDocument(document.DocumentPath, "virtual:" + document.DocumentPath + ".template-only.vue", document.Text.Length, "<template><FallbackOnly /></template>".Length),
                "1")
        ],
            CancellationToken.None);
        var lane = CreateLane(denoHost, virtualDocumentRegistry: registry);

        await AssertTemplateRequestDocumentSelectionAsync(
            lane,
            denoHost,
            document,
            projectedPath,
            DocumentKind.Vue);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_FallsBackToSource_ForJazorTemplateRequestMatrix_WhenPrimaryProjectedVueDocumentIsMissing()
    {
        var document = CreateDocument("""
            <UserCard />
            """);
        var denoHost = CreateTemplateRequestMatrixHost(document.DocumentPath);
        var registry = new InMemoryVirtualDocumentRegistry();
        await registry.UpsertAsync(
        [
            new VirtualDocument(
                new VirtualDocumentIdentity(
                    document.DocumentPath,
                    "virtual:" + document.DocumentPath + ".template-only.vue",
                    VirtualDocumentKind.Vue),
                "<template><FallbackOnly /></template>",
                ProjectionMap.CreateWholeDocument(document.DocumentPath, "virtual:" + document.DocumentPath + ".template-only.vue", document.Text.Length, "<template><FallbackOnly /></template>".Length),
                "1")
        ],
            CancellationToken.None);
        var lane = CreateLane(denoHost, virtualDocumentRegistry: registry);

        await AssertTemplateRequestDocumentSelectionAsync(
            lane,
            denoHost,
            document,
            document.DocumentPath,
            DocumentKind.Jazor);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetDefinition_ReturnsEmptyWhenDenoReturnsEmpty()
    {
        var lane = CreateLane(new FakeDenoFrontendHost());
        var document = CreateDocument("""
            <UserCard />
            """);

        var locations = await lane.GetDefinitionAsync(
            document,
            new LspPosition { Line = 0, Character = 2 },
            CreateTemplateTarget(document),
            CancellationToken.None);

        Assert.AreEqual(0, locations.Count);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetReferences_ReturnsEmptyWhenDenoReturnsEmpty()
    {
        var lane = CreateLane(new FakeDenoFrontendHost());
        var document = CreateDocument("""
            <UserCard />
            """);

        var locations = await lane.GetReferencesAsync(
            document,
            new LspPosition { Line = 0, Character = 2 },
            includeDeclaration: true,
            CreateTemplateTarget(document),
            CancellationToken.None);

        Assert.AreEqual(0, locations.Count);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetRename_ReturnsNullWhenDenoReturnsNull()
    {
        var lane = CreateLane(new FakeDenoFrontendHost());
        var document = CreateDocument("""
            <UserCard />
            """);

        var edit = await lane.GetRenameAsync(
            document,
            new LspPosition { Line = 0, Character = 2 },
            "AccountCard",
            CreateTemplateTarget(document),
            CancellationToken.None);

        Assert.IsNull(edit);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetReferences_ReturnsNativeScriptLocations_ForTypeScriptVueImport()
    {
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var declarationPath = Path.Combine(tempDirectory, "UserBadge.vue");
            await File.WriteAllTextAsync(declarationPath, "<template><div>UserBadge</div></template>");

            var scriptDocument = new DocumentSnapshot(
                Path.Combine(tempDirectory, "consumer.ts"),
                DocumentKind.TypeScript,
                """
                import UserBadge from "./UserBadge.vue";
                export const current = UserBadge;
                """,
                "1");
            await File.WriteAllTextAsync(scriptDocument.DocumentPath, scriptDocument.Text);

            var openJazorDocument = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Counter.jazor"),
                DocumentKind.Jazor,
                """
                <UserBadge />

                @code {
                    private string UserBadge => nameof(UserBadge);
                }
                """,
                "1");
            var diskJazorPath = Path.Combine(tempDirectory, "Dashboard.jazor");
            await File.WriteAllTextAsync(
                diskJazorPath,
                """
                <section>
                  <UserBadge />
                </section>

                @code {
                    private string UserBadge => nameof(UserBadge);
                }
                """);

            var workspaceStore = new InMemoryWorkspaceStore();
            await workspaceStore.UpsertDocumentAsync(scriptDocument, CancellationToken.None);
            await workspaceStore.UpsertDocumentAsync(openJazorDocument, CancellationToken.None);
            var lane = new VolarLaneService(
                workspaceStore,
                denoVolarHost: new FakeDenoFrontendHost
                {
                    References =
                    [
                        new LspLocation
                        {
                            Uri = LspProtocolHelpers.ToDocumentUri(scriptDocument.DocumentPath),
                            Range = new LspRange
                            {
                                Start = new LspPosition { Line = 0, Character = 7 },
                                End = new LspPosition { Line = 0, Character = 16 }
                            }
                        },
                        new LspLocation
                        {
                            Uri = LspProtocolHelpers.ToDocumentUri(scriptDocument.DocumentPath),
                            Range = new LspRange
                            {
                                Start = new LspPosition { Line = 1, Character = 23 },
                                End = new LspPosition { Line = 1, Character = 32 }
                            }
                        }
                    ],
                    Definitions =
                    [
                        new LspLocation
                        {
                            Uri = LspProtocolHelpers.ToDocumentUri(declarationPath),
                            Range = new LspRange
                            {
                                Start = new LspPosition { Line = 0, Character = 0 },
                                End = new LspPosition { Line = 0, Character = 0 }
                            }
                        }
                    ]
                },
                markupComponentBridge: new MarkupComponentBridgeService(workspaceStore));

            var locations = await lane.GetReferencesAsync(
                scriptDocument,
                new LspPosition { Line = 0, Character = 8 },
                includeDeclaration: true,
                CreateVolarTarget(scriptDocument),
                CancellationToken.None);

            var scriptUri = LspProtocolHelpers.ToDocumentUri(scriptDocument.DocumentPath);
            Assert.AreEqual(2, locations.Count);
            Assert.IsTrue(locations.All(location => location.Uri == scriptUri));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetry(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetRename_ReturnsNativeScriptEdits_ForTypeScriptVueImport()
    {
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var declarationPath = Path.Combine(tempDirectory, "UserBadge.vue");
            await File.WriteAllTextAsync(declarationPath, "<template><div>UserBadge</div></template>");

            var scriptDocument = new DocumentSnapshot(
                Path.Combine(tempDirectory, "consumer.ts"),
                DocumentKind.TypeScript,
                """
                import UserBadge from "./UserBadge.vue";
                export const current = UserBadge;
                """,
                "1");
            await File.WriteAllTextAsync(scriptDocument.DocumentPath, scriptDocument.Text);

            var openJazorDocument = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Counter.jazor"),
                DocumentKind.Jazor,
                """
                <UserBadge />

                @code {
                    private string UserBadge => nameof(UserBadge);
                }
                """,
                "1");
            var diskJazorPath = Path.Combine(tempDirectory, "Dashboard.jazor");
            await File.WriteAllTextAsync(
                diskJazorPath,
                """
                <section>
                  <UserBadge />
                </section>

                @code {
                    private string UserBadge => nameof(UserBadge);
                }
                """);

            var scriptUri = LspProtocolHelpers.ToDocumentUri(scriptDocument.DocumentPath);
            var workspaceStore = new InMemoryWorkspaceStore();
            await workspaceStore.UpsertDocumentAsync(scriptDocument, CancellationToken.None);
            await workspaceStore.UpsertDocumentAsync(openJazorDocument, CancellationToken.None);
            var lane = new VolarLaneService(
                workspaceStore,
                denoVolarHost: new FakeDenoFrontendHost
                {
                    Definitions =
                    [
                        new LspLocation
                        {
                            Uri = LspProtocolHelpers.ToDocumentUri(declarationPath),
                            Range = new LspRange
                            {
                                Start = new LspPosition { Line = 0, Character = 0 },
                                End = new LspPosition { Line = 0, Character = 0 }
                            }
                        }
                    ],
                    RenameResult = new LspWorkspaceEdit
                    {
                        Changes = new Dictionary<string, LspTextEdit[]>(StringComparer.Ordinal)
                        {
                            [scriptUri] =
                            [
                                new LspTextEdit
                                {
                                    Range = new LspRange
                                    {
                                        Start = new LspPosition { Line = 0, Character = 7 },
                                        End = new LspPosition { Line = 0, Character = 16 }
                                    },
                                    NewText = "ProfileBadge"
                                },
                                new LspTextEdit
                                {
                                    Range = new LspRange
                                    {
                                        Start = new LspPosition { Line = 1, Character = 23 },
                                        End = new LspPosition { Line = 1, Character = 32 }
                                    },
                                    NewText = "ProfileBadge"
                                }
                            ]
                        }
                    }
                },
                markupComponentBridge: new MarkupComponentBridgeService(workspaceStore));

            var edit = await lane.GetRenameAsync(
                scriptDocument,
                new LspPosition { Line = 0, Character = 8 },
                "ProfileBadge",
                CreateVolarTarget(scriptDocument),
                CancellationToken.None);

            Assert.IsNotNull(edit);
            Assert.IsTrue(edit.Changes.ContainsKey(scriptUri));
            Assert.AreEqual(1, edit.Changes.Count);
            Assert.AreEqual(2, edit.Changes[scriptUri].Length);
            Assert.IsTrue(edit.Changes[scriptUri].All(static change => change.NewText == "ProfileBadge"));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetry(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetCodeActions_DoesNotOfferLegacyVueImportQuickFixForMissingComponent()
    {
        var lane = CreateLane(new FakeDenoFrontendHost());
        var document = CreateDocument("""
            @jsimport dayjs from "dayjs"

            <MissingCard />
            """);
        var diagnostics =
            new[]
            {
                new LspDiagnostic
                {
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 3, Character = 3 },
                        End = new LspPosition { Line = 3, Character = 14 }
                    },
                    Severity = 2,
                    Code = "JAZORVUEFRONTEND001",
                    Source = "Jazor.VueHost.Frontend",
                    Message = "Razor component 'MissingCard' could not be resolved to a nearby Vue file."
                }
            };

        var actions = await lane.GetCodeActionsAsync(
            document,
            diagnostics[0].Range,
            diagnostics,
            CreateTemplateTarget(document),
            CancellationToken.None);

        Assert.AreEqual(0, actions.Count);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetCodeActions_DoesNotOfferLegacyVueImportQuickFixWhenNoImportsExist()
    {
        var lane = CreateLane(new FakeDenoFrontendHost());
        var document = CreateDocument("""
            <MissingCard />
            """);
        var diagnostics =
            new[]
            {
                new LspDiagnostic
                {
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 1, Character = 3 },
                        End = new LspPosition { Line = 1, Character = 14 }
                    },
                    Severity = 2,
                    Code = "JAZORVUEFRONTEND001",
                    Source = "Jazor.VueHost.Frontend",
                    Message = "Razor component 'MissingCard' could not be resolved to a nearby Vue file."
                }
            };

        var actions = await lane.GetCodeActionsAsync(
            document,
            diagnostics[0].Range,
            diagnostics,
            CreateTemplateTarget(document),
            CancellationToken.None);

        Assert.AreEqual(0, actions.Count);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetCodeActions_DoesNotOfferLegacyVueImportQuickFixWhenNearbyComponentExists()
    {
        var lane = CreateLane(new FakeDenoFrontendHost());
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var componentsDirectory = Path.Combine(tempDirectory, "Components");
            Directory.CreateDirectory(componentsDirectory);
            await File.WriteAllTextAsync(
                Path.Combine(componentsDirectory, "MissingCard.vue"),
                "<template><div /></template>");

            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Counter.jazor"),
                DocumentKind.Jazor,
                """
                <MissingCard />
                """,
                "1");
            var diagnostics =
                new[]
                {
                    new LspDiagnostic
                    {
                        Range = new LspRange
                        {
                            Start = new LspPosition { Line = 1, Character = 3 },
                            End = new LspPosition { Line = 1, Character = 14 }
                        },
                        Severity = 2,
                        Code = "JAZORVUEFRONTEND002",
                        Source = "Jazor.VueHost.Frontend",
                        Message = "Legacy @vueimport path './MissingCard.vue' is ignored for Razor-first IntelliSense."
                    }
                };

            var actions = await lane.GetCodeActionsAsync(
                document,
                diagnostics[0].Range,
                diagnostics,
                CreateTemplateTarget(document),
                CancellationToken.None);

            Assert.AreEqual(0, actions.Count);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetry(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetCodeActions_DoesNotOfferLegacyVueImportPathRewrite()
    {
        var lane = CreateLane(new FakeDenoFrontendHost());
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var componentsDirectory = Path.Combine(tempDirectory, "Components");
            Directory.CreateDirectory(componentsDirectory);
            await File.WriteAllTextAsync(
                Path.Combine(componentsDirectory, "MissingCard.vue"),
                "<template><div /></template>");

            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Counter.jazor"),
                DocumentKind.Jazor,
                """
                @vueimport MissingCard from "./MissingCard.vue"

                <MissingCard />
                """,
                "1");
            var diagnostics =
                new[]
                {
                    new LspDiagnostic
                    {
                        Range = new LspRange
                        {
                            Start = new LspPosition { Line = 0, Character = 30 },
                            End = new LspPosition { Line = 0, Character = 47 }
                        },
                        Severity = 2,
                        Code = "JAZORVUEFRONTEND001",
                        Source = "Jazor.VueHost.Frontend",
                        Message = "Razor component 'MissingCard' could not be resolved to a nearby Vue file."
                    }
                };

            var actions = await lane.GetCodeActionsAsync(
                document,
                diagnostics[0].Range,
                diagnostics,
                CreateTemplateTarget(document),
                CancellationToken.None);

            Assert.AreEqual(0, actions.Count);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetry(tempDirectory);
            }
        }
    }

    private static VolarLaneService CreateLane(
        IDenoVolarHost denoFrontendHost,
        IFrontendContextProvider? frontendContextProvider = null,
        IVirtualDocumentRegistry? virtualDocumentRegistry = null)
        => new(
            new InMemoryWorkspaceStore(),
            frontendContextProvider,
            virtualDocumentRegistry,
            denoFrontendHost);

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "JazorVueHostFrontendLaneTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteDirectoryWithRetry(string path)
    {
        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    return;
                }

                Directory.Delete(path, recursive: true);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                System.Threading.Thread.Sleep(100 * attempt);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                System.Threading.Thread.Sleep(100 * attempt);
            }
        }

        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private static DenoVolarHost CreateBundledDenoFrontendHost()
    {
        var baseDirectory = GetVueHostBuildBaseDirectory();
        var parsedOptions = DenoVolarHostOptionsParser.Parse(["--deno-worker"], baseDirectory);
        var options = new DenoVolarHostOptions
        {
            Enabled = true,
            ExecutablePath = parsedOptions.ExecutablePath,
            HasExplicitExecutableOverride = parsedOptions.HasExplicitExecutableOverride,
            WorkerScriptPath = parsedOptions.WorkerScriptPath,
            CacheDirectory = parsedOptions.CacheDirectory,
            Arguments = parsedOptions.Arguments,
            WorkingDirectory = parsedOptions.WorkingDirectory,
            IgnoreStartupFailure = false
        };

        Assert.IsTrue(File.Exists(options.ExecutablePath), $"Expected bundled Deno runtime '{options.ExecutablePath}' to exist.");
        Assert.IsTrue(File.Exists(options.WorkerScriptPath), $"Expected frontend worker script '{options.WorkerScriptPath}' to exist.");
        Assert.IsTrue(Directory.Exists(options.CacheDirectory), $"Expected frontend worker cache directory '{options.CacheDirectory}' to exist.");
        return new DenoVolarHost(options);
    }

    private static string GetVueHostBuildBaseDirectory()
    {
        var path = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "Jazor.VueHost",
            "bin",
            "Debug",
            "net10.0");
        Assert.IsTrue(Directory.Exists(path), $"Expected built VueHost output '{path}' to exist.");
        return path;
    }

    private static string GetRepositoryRoot()
        => Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            ".."));

    private static DocumentSnapshot CreateDocument(string text)
        => new(
            @"D:\temp\Counter.jazor",
            DocumentKind.Jazor,
            text,
            "1");

    private static LspPosition GetPosition(string text, string marker, int advance = 0)
    {
        var offset = text.IndexOf(marker, StringComparison.Ordinal);
        Assert.IsTrue(offset >= 0, $"Expected marker '{marker}' to exist.");
        return LspProtocolHelpers.GetPosition(text, offset + advance);
    }

    private static LspPosition GetLastPosition(string text, string marker, int advance = 0)
    {
        var offset = text.LastIndexOf(marker, StringComparison.Ordinal);
        Assert.IsTrue(offset >= 0, $"Expected marker '{marker}' to exist.");
        return LspProtocolHelpers.GetPosition(text, offset + advance);
    }

    private static ProjectionTarget CreateTemplateTarget(DocumentSnapshot document)
        => new(
            LaneKind.Volar,
            DocumentRegionKind.Template,
            document.DocumentPath,
            document.DocumentPath);

    private static ProjectionTarget CreateVolarTarget(DocumentSnapshot document)
        => new(
            LaneKind.Volar,
            DocumentRegionKind.Unknown,
            document.DocumentPath,
            document.DocumentPath);

    private static DenoVolarIntelliSenseContext CreateFrontendIntelliSenseContext(string componentPath)
        => new(
            new SemanticContext(
                "frontend",
                [
                    new DocumentSnapshot(
                        componentPath,
                        DocumentKind.Vue,
                        "<template><div>UserCard</div></template>",
                        "1")
                ],
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["relatedDocumentCount"] = "1",
                    ["provider"] = "Jazor.VueHost"
                }),
            [
                new ArtifactRecord(
                    artifactName: "virtual:" + componentPath + ".frontend-summary.json",
                    artifactKind: "frontend-summary",
                    content: ProtocolJsonSerializer.Serialize(new
                    {
                        documentPath = componentPath,
                        documentKind = "Vue",
                        referencedComponents = Array.Empty<string>()
                    }),
                    contentHash: null)
            ]);

    private static FakeDenoFrontendHost CreateTemplateRequestMatrixHost(string requestDocumentPath)
    {
        var uri = LspProtocolHelpers.ToDocumentUri(requestDocumentPath);
        return new FakeDenoFrontendHost
        {
            Diagnostics =
            [
                new LspDiagnostic
                {
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 1 },
                        End = new LspPosition { Line = 0, Character = 9 }
                    },
                    Severity = 2,
                    Code = "VUE001",
                    Source = "Volar",
                    Message = "matrix"
                }
            ],
            CompletionItems =
            [
                new LspCompletionItem
                {
                    Label = "UserCard",
                    Kind = 7,
                    Detail = "./UserCard.vue"
                }
            ],
            HoverResult = new LspHoverResult
            {
                Contents = new LspMarkupContent
                {
                    Kind = "markdown",
                    Value = "matrix"
                },
                Range = new LspRange
                {
                    Start = new LspPosition { Line = 0, Character = 1 },
                    End = new LspPosition { Line = 0, Character = 9 }
                }
            },
            DocumentSymbols =
            [
                new LspDocumentSymbol
                {
                    Name = "Template",
                    Kind = 2,
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 0 },
                        End = new LspPosition { Line = 0, Character = 11 }
                    },
                    SelectionRange = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 1 },
                        End = new LspPosition { Line = 0, Character = 9 }
                    }
                }
            ],
            SemanticTokens =
            [
                new LspSemanticToken
                {
                    Line = 0,
                    Character = 1,
                    Length = 8,
                    TokenType = "class",
                    TokenModifiers = []
                }
            ],
            Definitions =
            [
                new LspLocation
                {
                    Uri = uri,
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 1 },
                        End = new LspPosition { Line = 0, Character = 9 }
                    }
                }
            ],
            References =
            [
                new LspLocation
                {
                    Uri = uri,
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 1 },
                        End = new LspPosition { Line = 0, Character = 9 }
                    }
                }
            ],
            RenameResult = new LspWorkspaceEdit
            {
                Changes = new Dictionary<string, LspTextEdit[]>(StringComparer.Ordinal)
                {
                    [uri] =
                    [
                        new LspTextEdit
                        {
                            Range = new LspRange
                            {
                                Start = new LspPosition { Line = 0, Character = 1 },
                                End = new LspPosition { Line = 0, Character = 9 }
                            },
                            NewText = "AccountCard"
                        }
                    ]
                }
            }
        };
    }

    private static async Task AssertTemplateRequestDocumentSelectionAsync(
        VolarLaneService lane,
        FakeDenoFrontendHost denoHost,
        DocumentSnapshot document,
        string expectedDocumentPath,
        DocumentKind expectedDocumentKind)
    {
        var position = new LspPosition { Line = 0, Character = 2 };
        var target = CreateTemplateTarget(document);

        _ = await lane.GetDiagnosticsAsync(document, CancellationToken.None);
        AssertLastTemplateRequestDocument(denoHost, expectedDocumentPath, expectedDocumentKind);

        _ = await lane.GetCompletionItemsAsync(document, position, target, CancellationToken.None);
        AssertLastTemplateRequestDocument(denoHost, expectedDocumentPath, expectedDocumentKind);

        _ = await lane.GetHoverAsync(document, position, target, CancellationToken.None);
        AssertLastTemplateRequestDocument(denoHost, expectedDocumentPath, expectedDocumentKind);

        _ = await lane.GetDocumentSymbolsAsync(document, CancellationToken.None);
        AssertLastTemplateRequestDocument(denoHost, expectedDocumentPath, expectedDocumentKind);

        _ = await lane.GetSemanticTokensAsync(document, CancellationToken.None);
        AssertLastTemplateRequestDocument(denoHost, expectedDocumentPath, expectedDocumentKind);

        _ = await lane.GetDefinitionAsync(document, position, target, CancellationToken.None);
        AssertLastTemplateRequestDocument(denoHost, expectedDocumentPath, expectedDocumentKind);

        _ = await lane.GetReferencesAsync(document, position, includeDeclaration: true, target, CancellationToken.None);
        AssertLastTemplateRequestDocument(denoHost, expectedDocumentPath, expectedDocumentKind);

        _ = await lane.GetRenameAsync(document, position, "AccountCard", target, CancellationToken.None);
        AssertLastTemplateRequestDocument(denoHost, expectedDocumentPath, expectedDocumentKind);
    }

    private static void AssertLastTemplateRequestDocument(
        FakeDenoFrontendHost denoHost,
        string expectedDocumentPath,
        DocumentKind expectedDocumentKind)
    {
        Assert.IsNotNull(denoHost.LastDocument);
        Assert.AreEqual(expectedDocumentPath, denoHost.LastDocument.DocumentPath);
        Assert.AreEqual(expectedDocumentKind, denoHost.LastDocument.DocumentKind);
    }

    private sealed class FakeDenoWorkerProcess : IDenoWorkerProcess
    {
        private readonly Dictionary<string, object?> _results = new(StringComparer.Ordinal);
        private readonly Dictionary<string, Queue<Exception>> _failures = new(StringComparer.Ordinal);

        public bool IsRunning { get; private set; }

        public int StartCallCount { get; private set; }

        public int StopCallCount { get; private set; }

        public string[] RequestMethods => _requestMethods.ToArray();

        private readonly List<string> _requestMethods = [];

        public void SetResult(string method, object? result)
        {
            _results[method] = result;
        }

        public void SetFailure(string method, Exception exception)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(method);
            ArgumentNullException.ThrowIfNull(exception);

            if (!_failures.TryGetValue(method, out var queue))
            {
                queue = new Queue<Exception>();
                _failures[method] = queue;
            }

            queue.Enqueue(exception);
        }

        public ValueTask StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StartCallCount++;
            IsRunning = true;
            return ValueTask.CompletedTask;
        }

        public ValueTask<TResult?> SendRequestAsync<TResult>(
            string method,
            object payload,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _requestMethods.Add(method);

            if (_failures.TryGetValue(method, out var failures)
                && failures.Count > 0)
            {
                IsRunning = false;
                throw failures.Dequeue();
            }

            return ValueTask.FromResult(
                _results.TryGetValue(method, out var result)
                    ? (TResult?)result
                    : default);
        }

        public ValueTask StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StopCallCount++;
            IsRunning = false;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeDenoFrontendHost : IDenoVolarHost
    {
        private readonly Dictionary<string, Queue<Exception>> _failures = new(StringComparer.OrdinalIgnoreCase);

        public bool IsEnabled { get; init; } = true;

        public bool IsRunning { get; init; } = true;

        public DenoSfcCompileResult? SfcCompileResult { get; init; }

        public IReadOnlyList<LspDiagnostic> Diagnostics { get; init; } = [];

        public IReadOnlyList<LspCompletionItem> CompletionItems { get; init; } = [];

        public IReadOnlyList<LspDocumentSymbol> DocumentSymbols { get; init; } = [];

        public IReadOnlyList<LspSemanticToken> SemanticTokens { get; init; } = [];

        public LspHoverResult? HoverResult { get; init; }

        public IReadOnlyList<LspLocation> Definitions { get; init; } = [];

        public IReadOnlyList<LspLocation> References { get; init; } = [];

        public LspWorkspaceEdit? RenameResult { get; init; }

        public DocumentSnapshot? LastDocument { get; private set; }

        public LspPosition? LastPosition { get; private set; }

        public DenoVolarIntelliSenseContext? LastContext { get; private set; }

        public void SetFailure(string method, Exception exception)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(method);
            ArgumentNullException.ThrowIfNull(exception);

            if (!_failures.TryGetValue(method, out var queue))
            {
                queue = new Queue<Exception>();
                _failures[method] = queue;
            }

            queue.Enqueue(exception);
        }

        public ValueTask StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        }

        public ValueTask StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;

        public ValueTask<DenoSfcCompileResult?> CompileSfcAsync(
            string documentPath,
            string sfcText,
            string filename,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(SfcCompileResult);
        }

        public ValueTask<DenoTypeScriptCompileResult?> CompileTypeScriptAsync(
            string documentPath,
            string text,
            string filename,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult<DenoTypeScriptCompileResult?>(default);
        }

        public ValueTask<IReadOnlyList<LspDiagnostic>> GetTemplateDiagnosticsAsync(
            DocumentSnapshot document,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
        {
            ThrowIfConfigured("diagnostics", cancellationToken);
            LastDocument = document;
            LastContext = context;
            return ValueTask.FromResult(Diagnostics);
        }

        public ValueTask<IReadOnlyList<LspCompletionItem>> GetTemplateCompletionItemsAsync(
            DocumentSnapshot document,
            LspPosition position,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
        {
            ThrowIfConfigured("completion", cancellationToken);
            LastDocument = document;
            LastPosition = position;
            LastContext = context;
            return ValueTask.FromResult(CompletionItems);
        }

        public ValueTask<IReadOnlyList<LspDocumentSymbol>> GetTemplateDocumentSymbolsAsync(
            DocumentSnapshot document,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
        {
            ThrowIfConfigured("documentSymbol", cancellationToken);
            LastDocument = document;
            LastContext = context;
            return ValueTask.FromResult(DocumentSymbols);
        }

        public ValueTask<IReadOnlyList<LspSemanticToken>> GetTemplateSemanticTokensAsync(
            DocumentSnapshot document,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
        {
            ThrowIfConfigured("semanticTokens", cancellationToken);
            LastDocument = document;
            LastContext = context;
            return ValueTask.FromResult(SemanticTokens);
        }

        public ValueTask<LspHoverResult?> GetTemplateHoverAsync(
            DocumentSnapshot document,
            LspPosition position,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
        {
            ThrowIfConfigured("hover", cancellationToken);
            LastDocument = document;
            LastPosition = position;
            LastContext = context;
            return ValueTask.FromResult(HoverResult);
        }

        public ValueTask<IReadOnlyList<LspLocation>> GetTemplateDefinitionAsync(
            DocumentSnapshot document,
            LspPosition position,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
        {
            ThrowIfConfigured("definition", cancellationToken);
            LastDocument = document;
            LastPosition = position;
            LastContext = context;
            return ValueTask.FromResult(Definitions);
        }

        public ValueTask<IReadOnlyList<LspLocation>> GetTemplateReferencesAsync(
            DocumentSnapshot document,
            LspPosition position,
            bool includeDeclaration,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
        {
            ThrowIfConfigured("references", cancellationToken);
            LastDocument = document;
            LastPosition = position;
            LastContext = context;
            return ValueTask.FromResult(References);
        }

        public ValueTask<LspWorkspaceEdit?> GetTemplateRenameAsync(
            DocumentSnapshot document,
            LspPosition position,
            string newName,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
        {
            ThrowIfConfigured("rename", cancellationToken);
            LastDocument = document;
            LastPosition = position;
            LastContext = context;
            return ValueTask.FromResult(RenameResult);
        }

        private void ThrowIfConfigured(string method, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_failures.TryGetValue(method, out var queue)
                && queue.Count > 0)
            {
                throw queue.Dequeue();
            }
        }
    }

    private sealed class FakeFrontendContextProvider(GetFrontendContextResponse response) : IFrontendContextProvider
    {
        public ValueTask<GetFrontendContextResponse> GetFrontendContextAsync(
            GetFrontendContextRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(response);
        }
    }
}
