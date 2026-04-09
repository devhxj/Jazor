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
                        Start = new LspPosition { Line = 0, Character = 1 },
                        End = new LspPosition { Line = 0, Character = 12 }
                    },
                    Severity = 2,
                    Code = "JAZORVUEFRONTEND001",
                    Source = "Jazor.VueHost.Frontend",
                    Message = "Razor component 'MissingCard' could not be resolved to a nearby Vue file."
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
        var host = new DenoFrontendHost(
            new DenoFrontendHostOptions
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
            CancellationToken.None);

        Assert.AreEqual(1, workerProcess.StartCallCount);
        Assert.AreEqual(1, symbols.Count);
        Assert.AreEqual("Template", symbols[0].Name);
        Assert.IsNotNull(symbols[0].Children);
        Assert.AreEqual("UserCard", symbols[0].Children![0].Name);
        CollectionAssert.AreEqual(new[] { "template/documentSymbols" }, workerProcess.RequestMethods);
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
                Directory.Delete(tempDirectory, recursive: true);
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
                Directory.Delete(tempDirectory, recursive: true);
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
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    private static FrontendLaneService CreateLane(IDenoFrontendHost denoFrontendHost)
        => new(
            new InMemoryWorkspaceStore(),
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

        public IReadOnlyList<LspDocumentSymbol> DocumentSymbols { get; init; } = [];

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

        public ValueTask<IReadOnlyList<LspDocumentSymbol>> GetTemplateDocumentSymbolsAsync(
            DocumentSnapshot document,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(DocumentSymbols);
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
