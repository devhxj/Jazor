using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Frontend.Deno.Hosting;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Lanes;
using Jazor.VueHost.Lsp.Routing;
using Jazor.VueHost.Services;
using Jazor.VueHost.Workspace;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostFrontendLaneTests
{
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
                        Start = new LspPosition { Line = 3, Character = 3 },
                        End = new LspPosition { Line = 3, Character = 12 }
                    },
                    Severity = 2,
                    Code = "JAZORVUEFRONTEND001",
                    Source = "Jazor.VueHost.Frontend",
                    Message = "Template component 'MissingCard' is not imported via @vueimport."
                }
            });
        var host = new DenoFrontendHost(
            new DenoFrontendHostOptions
            {
                Enabled = true,
                IgnoreStartupFailure = false
            },
            workerProcess);

        var diagnostics = await host.GetTemplateDiagnosticsAsync(CreateDocument("""
            <template>
              <MissingCard />
            </template>
            """), CancellationToken.None);

        Assert.AreEqual(1, workerProcess.StartCallCount);
        Assert.AreEqual(1, diagnostics.Count);
        Assert.AreEqual("JAZORVUEFRONTEND001", diagnostics[0].Code);
        CollectionAssert.AreEqual(new[] { "template/diagnostics" }, workerProcess.RequestMethods);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetDiagnostics_UsesDenoDiagnostics()
    {
        var lane = CreateLane(new FakeDenoFrontendHost
        {
            Diagnostics = new[]
            {
                new LspDiagnostic
                {
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 3, Character = 3 },
                        End = new LspPosition { Line = 3, Character = 12 }
                    },
                    Severity = 2,
                    Code = "JAZORVUEFRONTEND001",
                    Source = "Jazor.VueHost.Frontend",
                    Message = "Template component 'MissingCard' is not imported via @vueimport."
                }
            }
        });

        var diagnostics = await lane.GetDiagnosticsAsync(CreateDocument("""
            <template>
              <MissingCard />
            </template>
            """), CancellationToken.None);

        Assert.AreEqual(1, diagnostics.Count);
        Assert.AreEqual("JAZORVUEFRONTEND001", diagnostics[0].Code);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetDiagnostics_ReturnsUnresolvedVueImportDiagnostics()
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

                <template>
                  <MissingCard />
                </template>
                """,
                "1");

            var diagnostics = await lane.GetDiagnosticsAsync(document, CancellationToken.None);

            Assert.AreEqual(1, diagnostics.Count);
            Assert.AreEqual("JAZORVUEFRONTEND002", diagnostics[0].Code);
            StringAssert.Contains(diagnostics[0].Message, "MissingCard");
            StringAssert.Contains(diagnostics[0].Message, "./MissingCard.vue");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetCompletionItems_FallsBackWhenDenoReturnsEmpty()
    {
        var lane = CreateLane(new FakeDenoFrontendHost());
        var document = CreateDocument("""
            @vueimport UserCard from "./UserCard.vue"

            <template>
              <
            </template>
            """);

        var items = await lane.GetCompletionItemsAsync(
            document,
            new LspPosition { Line = 3, Character = 3 },
            CreateTemplateTarget(document),
            CancellationToken.None);

        CollectionAssert.Contains(items.Select(static item => item.Label).ToArray(), "UserCard");
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetCompletionItems_FiltersImportedComponentsByTypedPrefix()
    {
        var lane = CreateLane(new FakeDenoFrontendHost());
        var document = CreateDocument("""
            @vueimport UserCard from "./UserCard.vue"
            @vueimport ProfileCard from "./ProfileCard.vue"

            <template>
              <Use
            </template>
            """);

        var items = await lane.GetCompletionItemsAsync(
            document,
            new LspPosition { Line = 4, Character = 6 },
            CreateTemplateTarget(document),
            CancellationToken.None);

        var labels = items.Select(static item => item.Label).ToArray();
        CollectionAssert.Contains(labels, "UserCard");
        CollectionAssert.DoesNotContain(labels, "ProfileCard");
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
            @vueimport UserCard from "./UserCard.vue"

            <template>
              <
            </template>
            """);

        var items = await lane.GetCompletionItemsAsync(
            document,
            new LspPosition { Line = 3, Character = 3 },
            CreateTemplateTarget(document),
            CancellationToken.None);

        Assert.AreEqual(1, items.Count);
        Assert.AreEqual("DenoOnlyCard", items[0].Label);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetCompletionItems_SuggestsNearbyVueFilesWhenNotImported()
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
                <template>
                  <
                </template>
                """,
                "1");

            var items = await lane.GetCompletionItemsAsync(
                document,
                new LspPosition { Line = 1, Character = 3 },
                CreateTemplateTarget(document),
                CancellationToken.None);

            var missingCard = items.SingleOrDefault(static item => item.Label == "MissingCard");
            Assert.IsNotNull(missingCard);
            Assert.AreEqual("./Components/MissingCard.vue", missingCard.Detail);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetCompletionItems_FiltersNearbyVueFilesByTypedPrefix()
    {
        var lane = CreateLane(new FakeDenoFrontendHost());
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "UserCard.vue"),
                "<template><div /></template>");
            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "ProfileCard.vue"),
                "<template><div /></template>");

            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Counter.jazor"),
                DocumentKind.Jazor,
                """
                <template>
                  <Pro
                </template>
                """,
                "1");

            var items = await lane.GetCompletionItemsAsync(
                document,
                new LspPosition { Line = 1, Character = 6 },
                CreateTemplateTarget(document),
                CancellationToken.None);

            var labels = items.Select(static item => item.Label).ToArray();
            CollectionAssert.Contains(labels, "ProfileCard");
            CollectionAssert.DoesNotContain(labels, "UserCard");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetCompletionItems_DoesNotInjectNearbyVueFilesWhenFallbackAlreadyResolvedImports()
    {
        var lane = CreateLane(new FakeDenoFrontendHost());
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "ProfileCard.vue"),
                "<template><div /></template>");

            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Counter.jazor"),
                DocumentKind.Jazor,
                """
                @vueimport UserCard from "./UserCard.vue"

                <template>
                  <
                </template>
                """,
                "1");

            var items = await lane.GetCompletionItemsAsync(
                document,
                new LspPosition { Line = 3, Character = 3 },
                CreateTemplateTarget(document),
                CancellationToken.None);

            var labels = items.Select(static item => item.Label).ToArray();
            CollectionAssert.Contains(labels, "UserCard");
            CollectionAssert.DoesNotContain(labels, "ProfileCard");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetCodeActions_ReturnsQuickFixForMissingVueImport()
    {
        var lane = CreateLane(new FakeDenoFrontendHost());
        var document = CreateDocument("""
            @jsimport dayjs from "dayjs"

            <template>
              <MissingCard />
            </template>
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
                    Message = "Template component 'MissingCard' is not imported via @vueimport."
                }
            };

        var actions = await lane.GetCodeActionsAsync(
            document,
            diagnostics[0].Range,
            diagnostics,
            CreateTemplateTarget(document),
            CancellationToken.None);

        Assert.AreEqual(1, actions.Count);
        Assert.AreEqual("Import MissingCard via @vueimport", actions[0].Title);
        var edit = actions[0].Edit;
        Assert.IsNotNull(edit);
        var documentUri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath);
        Assert.IsTrue(edit.Changes.ContainsKey(documentUri));
        var textEdit = edit.Changes[documentUri].Single();
        Assert.AreEqual(1, textEdit.Range.Start.Line);
        Assert.AreEqual(0, textEdit.Range.Start.Character);
        Assert.AreEqual("@vueimport MissingCard from \"./MissingCard.vue\"" + Environment.NewLine, textEdit.NewText);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetCodeActions_InsertsQuickFixAtDocumentStartWhenNoImportsExist()
    {
        var lane = CreateLane(new FakeDenoFrontendHost());
        var document = CreateDocument("""
            <template>
              <MissingCard />
            </template>
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
                    Message = "Template component 'MissingCard' is not imported via @vueimport."
                }
            };

        var actions = await lane.GetCodeActionsAsync(
            document,
            diagnostics[0].Range,
            diagnostics,
            CreateTemplateTarget(document),
            CancellationToken.None);

        Assert.AreEqual(1, actions.Count);
        var edit = actions[0].Edit;
        Assert.IsNotNull(edit);
        var documentUri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath);
        var textEdit = edit.Changes[documentUri].Single();
        Assert.AreEqual(0, textEdit.Range.Start.Line);
        Assert.AreEqual(0, textEdit.Range.Start.Character);
        Assert.AreEqual("@vueimport MissingCard from \"./MissingCard.vue\"" + Environment.NewLine + Environment.NewLine, textEdit.NewText);
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetCodeActions_PrefersExistingComponentPathWhenAvailable()
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
                <template>
                  <MissingCard />
                </template>
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
                        Code = "JAZORVUEFRONTEND001",
                        Source = "Jazor.VueHost.Frontend",
                        Message = "Template component 'MissingCard' is not imported via @vueimport."
                    }
                };

            var actions = await lane.GetCodeActionsAsync(
                document,
                diagnostics[0].Range,
                diagnostics,
                CreateTemplateTarget(document),
                CancellationToken.None);

            Assert.AreEqual(1, actions.Count);
            var documentUri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath);
            var textEdit = actions[0].Edit!.Changes[documentUri].Single();
            Assert.AreEqual("@vueimport MissingCard from \"./Components/MissingCard.vue\"" + Environment.NewLine + Environment.NewLine, textEdit.NewText);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_FrontendLaneService_GetCodeActions_ReturnsQuickFixForUnresolvedVueImportPath()
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

                <template>
                  <MissingCard />
                </template>
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
                        Code = "JAZORVUEFRONTEND002",
                        Source = "Jazor.VueHost.Frontend",
                        Message = "Imported Vue component 'MissingCard' from './MissingCard.vue' could not be resolved."
                    }
                };

            var actions = await lane.GetCodeActionsAsync(
                document,
                diagnostics[0].Range,
                diagnostics,
                CreateTemplateTarget(document),
                CancellationToken.None);

            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual("Update MissingCard @vueimport path", actions[0].Title);
            var documentUri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath);
            var textEdit = actions[0].Edit!.Changes[documentUri].Single();
            Assert.AreEqual("./Components/MissingCard.vue", textEdit.NewText);
            Assert.AreEqual(0, textEdit.Range.Start.Line);
            Assert.AreEqual(29, textEdit.Range.Start.Character);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    private static FrontendLaneService CreateLane(IDenoFrontendHost denoFrontendHost)
        => new(
            new JazorLspDocumentService(
                new InMemoryWorkspaceStore(),
                new NullVueAnalysisClient()),
            denoFrontendHost);

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "JazorVueHostFrontendLaneTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static DocumentSnapshot CreateDocument(string text)
        => new(
            @"D:\temp\Counter.jazor",
            DocumentKind.Jazor,
            text,
            "1");

    private static ProjectionTarget CreateTemplateTarget(DocumentSnapshot document)
        => new(
            LaneKind.Frontend,
            DocumentRegionKind.Template,
            document.DocumentPath,
            document.DocumentPath);

    private sealed class FakeDenoWorkerProcess : IDenoWorkerProcess
    {
        private readonly Dictionary<string, object?> _results = new(StringComparer.Ordinal);

        public bool IsRunning { get; private set; }

        public int StartCallCount { get; private set; }

        public string[] RequestMethods => _requestMethods.ToArray();

        private readonly List<string> _requestMethods = [];

        public void SetResult(string method, object? result)
        {
            _results[method] = result;
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
            return ValueTask.FromResult(
                _results.TryGetValue(method, out var result)
                    ? (TResult?)result
                    : default);
        }

        public ValueTask StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IsRunning = false;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeDenoFrontendHost : IDenoFrontendHost
    {
        public bool IsRunning => true;

        public IReadOnlyList<LspDiagnostic> Diagnostics { get; init; } = [];

        public IReadOnlyList<LspCompletionItem> CompletionItems { get; init; } = [];

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

        public ValueTask<IReadOnlyList<LspDiagnostic>> GetTemplateDiagnosticsAsync(
            DocumentSnapshot document,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(Diagnostics);
        }

        public ValueTask<IReadOnlyList<LspCompletionItem>> GetTemplateCompletionItemsAsync(
            DocumentSnapshot document,
            LspPosition position,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(CompletionItems);
        }

        public ValueTask<LspHoverResult?> GetTemplateHoverAsync(
            DocumentSnapshot document,
            LspPosition position,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult<LspHoverResult?>(null);
        }

        public ValueTask<IReadOnlyList<LspLocation>> GetTemplateDefinitionAsync(
            DocumentSnapshot document,
            LspPosition position,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());
        }

        public ValueTask<IReadOnlyList<LspLocation>> GetTemplateReferencesAsync(
            DocumentSnapshot document,
            LspPosition position,
            bool includeDeclaration,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());
        }

        public ValueTask<LspWorkspaceEdit?> GetTemplateRenameAsync(
            DocumentSnapshot document,
            LspPosition position,
            string newName,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult<LspWorkspaceEdit?>(null);
        }
    }
}
