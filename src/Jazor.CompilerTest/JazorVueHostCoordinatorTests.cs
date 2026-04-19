using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Aggregation;
using Jazor.VueHost.Lsp.Coordination;
using Jazor.VueHost.Lsp.Lanes;
using Jazor.VueHost.Lsp.Routing;
using Jazor.VueHost.Workspace;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostCoordinatorTests
{
    [TestMethod]
    public async Task JazorVueHost_MarkupBridgeFanoutCoordinator_Definition_FallsBackToVueFileWhenNativeDefinitionIsEmpty()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var componentName = "UserBadge" + Guid.NewGuid().ToString("N", System.Globalization.CultureInfo.InvariantCulture);

        try
        {
            var vuePath = Path.Combine(tempDirectory, componentName + ".vue");
            await File.WriteAllTextAsync(vuePath, $"<template><div>{componentName}</div></template>");

            var scriptDocument = new DocumentSnapshot(
                Path.Combine(tempDirectory, "consumer.ts"),
                DocumentKind.TypeScript,
                $"""
                import {componentName} from "./{componentName}.vue";
                export const current = {componentName};
                """,
                "1");
            await File.WriteAllTextAsync(scriptDocument.DocumentPath, scriptDocument.Text);

            var workspaceStore = new InMemoryWorkspaceStore();
            await workspaceStore.UpsertDocumentAsync(scriptDocument, CancellationToken.None);

            var coordinator = new MarkupBridgeFanoutCoordinator(
                new MarkupComponentBridgeService(workspaceStore),
                new LspResultAggregator());

            var locations = await coordinator.CoordinateDefinitionAsync(
                scriptDocument,
                new LspPosition { Line = 0, Character = 8 },
                Array.Empty<LspLocation>(),
                CancellationToken.None);

            Assert.AreEqual(1, locations.Count);
            Assert.AreEqual(LspProtocolHelpers.ToDocumentUri(vuePath), locations[0].Uri);
        }
        finally
        {
            DeleteDirectory(tempDirectory);
        }
    }

    [TestMethod]
    public async Task JazorVueHost_ReferenceCoordinator_ForTypeScriptVueImport_FansOutIntoJazorWithoutDeclarationWhenExcluded()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var componentName = "UserBadge" + Guid.NewGuid().ToString("N", System.Globalization.CultureInfo.InvariantCulture);

        try
        {
            var vuePath = Path.Combine(tempDirectory, componentName + ".vue");
            await File.WriteAllTextAsync(vuePath, $"<template><div>{componentName}</div></template>");

            var scriptDocument = new DocumentSnapshot(
                Path.Combine(tempDirectory, "consumer.ts"),
                DocumentKind.TypeScript,
                $"""
                import {componentName} from "./{componentName}.vue";
                export const current = {componentName};
                """,
                "1");
            await File.WriteAllTextAsync(scriptDocument.DocumentPath, scriptDocument.Text);

            var openJazorDocument = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Counter.jazor"),
                DocumentKind.Jazor,
                $"<{componentName} />",
                "1");
            await File.WriteAllTextAsync(openJazorDocument.DocumentPath, openJazorDocument.Text);

            var diskJazorPath = Path.Combine(tempDirectory, "Dashboard.jazor");
            await File.WriteAllTextAsync(
                diskJazorPath,
                $"""
                <section>
                  <{componentName} />
                </section>
                """);

            var workspaceStore = new InMemoryWorkspaceStore();
            await workspaceStore.UpsertDocumentAsync(scriptDocument, CancellationToken.None);
            await workspaceStore.UpsertDocumentAsync(openJazorDocument, CancellationToken.None);

            var scriptUri = LspProtocolHelpers.ToDocumentUri(scriptDocument.DocumentPath);
            var openJazorUri = LspProtocolHelpers.ToDocumentUri(openJazorDocument.DocumentPath);
            var diskJazorUri = LspProtocolHelpers.ToDocumentUri(diskJazorPath);
            var vueUri = LspProtocolHelpers.ToDocumentUri(vuePath);

            var importStart = "import ".Length;
            var usageStart = "export const current = ".Length;
            var coordinator = new ReferenceCoordinator(
                new Dictionary<LaneKind, ILspLane>
                {
                    [LaneKind.Volar] = new FakeLane
                    {
                        References =
                        [
                            new LspLocation
                            {
                                Uri = scriptUri,
                                Range = new LspRange
                                {
                                    Start = new LspPosition { Line = 0, Character = importStart },
                                    End = new LspPosition { Line = 0, Character = importStart + componentName.Length }
                                }
                            },
                            new LspLocation
                            {
                                Uri = scriptUri,
                                Range = new LspRange
                                {
                                    Start = new LspPosition { Line = 1, Character = usageStart },
                                    End = new LspPosition { Line = 1, Character = usageStart + componentName.Length }
                                }
                            }
                        ]
                    }
                },
                new LspLaneRouter(),
                new MarkupBridgeFanoutCoordinator(
                    new MarkupComponentBridgeService(workspaceStore),
                    new LspResultAggregator()));

            var locations = await coordinator.CoordinateAsync(
                scriptDocument,
                new LspPosition { Line = 0, Character = 8 },
                includeDeclaration: false,
                CreateVolarTarget(scriptDocument),
                CancellationToken.None);

            Assert.AreEqual(4, locations.Count);
            Assert.IsTrue(locations.Count(location => location.Uri == scriptUri) == 2);
            Assert.IsTrue(locations.Any(location => location.Uri == openJazorUri));
            Assert.IsTrue(locations.Any(location => location.Uri == diskJazorUri));
            Assert.IsFalse(locations.Any(location => location.Uri == vueUri));
        }
        finally
        {
            DeleteDirectory(tempDirectory);
        }
    }

    [TestMethod]
    public async Task JazorVueHost_RenameCoordinator_ForTypeScriptVueImport_MergesNativeAndJazorEdits()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var componentName = "UserBadge" + Guid.NewGuid().ToString("N", System.Globalization.CultureInfo.InvariantCulture);

        try
        {
            var vuePath = Path.Combine(tempDirectory, componentName + ".vue");
            await File.WriteAllTextAsync(vuePath, $"<template><div>{componentName}</div></template>");

            var scriptDocument = new DocumentSnapshot(
                Path.Combine(tempDirectory, "consumer.ts"),
                DocumentKind.TypeScript,
                $"""
                import {componentName} from "./{componentName}.vue";
                export const current = {componentName};
                """,
                "1");
            await File.WriteAllTextAsync(scriptDocument.DocumentPath, scriptDocument.Text);

            var openJazorDocument = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Counter.jazor"),
                DocumentKind.Jazor,
                $"<{componentName} />",
                "1");
            await File.WriteAllTextAsync(openJazorDocument.DocumentPath, openJazorDocument.Text);

            var diskJazorPath = Path.Combine(tempDirectory, "Dashboard.jazor");
            await File.WriteAllTextAsync(
                diskJazorPath,
                $"""
                <section>
                  <{componentName} />
                </section>
                """);

            var workspaceStore = new InMemoryWorkspaceStore();
            await workspaceStore.UpsertDocumentAsync(scriptDocument, CancellationToken.None);
            await workspaceStore.UpsertDocumentAsync(openJazorDocument, CancellationToken.None);

            var scriptUri = LspProtocolHelpers.ToDocumentUri(scriptDocument.DocumentPath);
            var openJazorUri = LspProtocolHelpers.ToDocumentUri(openJazorDocument.DocumentPath);
            var diskJazorUri = LspProtocolHelpers.ToDocumentUri(diskJazorPath);
            var importStart = "import ".Length;
            var usageStart = "export const current = ".Length;

            var coordinator = new RenameCoordinator(
                new Dictionary<LaneKind, ILspLane>
                {
                    [LaneKind.Volar] = new FakeLane
                    {
                        RenameEdit = new LspWorkspaceEdit
                        {
                            Changes = new Dictionary<string, LspTextEdit[]>(StringComparer.Ordinal)
                            {
                                [scriptUri] =
                                [
                                    new LspTextEdit
                                    {
                                        Range = new LspRange
                                        {
                                            Start = new LspPosition { Line = 0, Character = importStart },
                                            End = new LspPosition { Line = 0, Character = importStart + componentName.Length }
                                        },
                                        NewText = "ProfileBadge"
                                    },
                                    new LspTextEdit
                                    {
                                        Range = new LspRange
                                        {
                                            Start = new LspPosition { Line = 1, Character = usageStart },
                                            End = new LspPosition { Line = 1, Character = usageStart + componentName.Length }
                                        },
                                        NewText = "ProfileBadge"
                                    }
                                ]
                            }
                        }
                    }
                },
                new LspLaneRouter(),
                new LspResultAggregator(),
                new MarkupBridgeFanoutCoordinator(
                    new MarkupComponentBridgeService(workspaceStore),
                    new LspResultAggregator()));

            var edit = await coordinator.CoordinateAsync(
                scriptDocument,
                new LspPosition { Line = 0, Character = 8 },
                "ProfileBadge",
                CreateVolarTarget(scriptDocument),
                CancellationToken.None);

            Assert.IsNotNull(edit);
            Assert.AreEqual(3, edit.Changes.Count);
            Assert.IsTrue(edit.Changes.ContainsKey(scriptUri));
            Assert.IsTrue(edit.Changes.ContainsKey(openJazorUri));
            Assert.IsTrue(edit.Changes.ContainsKey(diskJazorUri));
            Assert.IsTrue(edit.Changes.Values.SelectMany(static changes => changes).All(static change => change.NewText == "ProfileBadge"));
        }
        finally
        {
            DeleteDirectory(tempDirectory);
        }
    }

    [TestMethod]
    public async Task JazorVueHost_ReferenceCoordinator_ForJazorMarkupTag_FansOutWithoutLaneLocations()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var componentName = "UserBadge" + Guid.NewGuid().ToString("N", System.Globalization.CultureInfo.InvariantCulture);

        try
        {
            var vuePath = Path.Combine(tempDirectory, componentName + ".vue");
            await File.WriteAllTextAsync(vuePath, $"<template><div>{componentName}</div></template>");

            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Counter.jazor"),
                DocumentKind.Jazor,
                $"<{componentName} />",
                "1");
            await File.WriteAllTextAsync(document.DocumentPath, document.Text);

            var secondDocumentPath = Path.Combine(tempDirectory, "Dashboard.jazor");
            await File.WriteAllTextAsync(
                secondDocumentPath,
                $"""
                <section>
                  <{componentName} />
                </section>
                """);

            var workspaceStore = new InMemoryWorkspaceStore();
            await workspaceStore.UpsertDocumentAsync(document, CancellationToken.None);

            var coordinator = new ReferenceCoordinator(
                new Dictionary<LaneKind, ILspLane>(),
                new LspLaneRouter(),
                new MarkupBridgeFanoutCoordinator(
                    new MarkupComponentBridgeService(workspaceStore),
                    new LspResultAggregator()));

            var locations = await coordinator.CoordinateAsync(
                document,
                new LspPosition { Line = 0, Character = 2 },
                includeDeclaration: true,
                CreateJazorTarget(document),
                CancellationToken.None);

            Assert.IsTrue(locations.Any(location => location.Uri == LspProtocolHelpers.ToDocumentUri(vuePath)));
            Assert.IsTrue(locations.Any(location => location.Uri == LspProtocolHelpers.ToDocumentUri(document.DocumentPath)));
            Assert.IsTrue(locations.Any(location => location.Uri == LspProtocolHelpers.ToDocumentUri(secondDocumentPath)));
        }
        finally
        {
            DeleteDirectory(tempDirectory);
        }
    }

    [TestMethod]
    public async Task JazorVueHost_RenameCoordinator_ForJazorMarkupTag_FansOutWithoutLaneEdits()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var componentName = "UserBadge" + Guid.NewGuid().ToString("N", System.Globalization.CultureInfo.InvariantCulture);

        try
        {
            var vuePath = Path.Combine(tempDirectory, componentName + ".vue");
            await File.WriteAllTextAsync(vuePath, $"<template><div>{componentName}</div></template>");

            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Counter.jazor"),
                DocumentKind.Jazor,
                $"<{componentName} />",
                "1");
            await File.WriteAllTextAsync(document.DocumentPath, document.Text);

            var secondDocumentPath = Path.Combine(tempDirectory, "Dashboard.jazor");
            await File.WriteAllTextAsync(
                secondDocumentPath,
                $"""
                <section>
                  <{componentName} />
                </section>
                """);

            var workspaceStore = new InMemoryWorkspaceStore();
            await workspaceStore.UpsertDocumentAsync(document, CancellationToken.None);

            var coordinator = new RenameCoordinator(
                new Dictionary<LaneKind, ILspLane>(),
                new LspLaneRouter(),
                new LspResultAggregator(),
                new MarkupBridgeFanoutCoordinator(
                    new MarkupComponentBridgeService(workspaceStore),
                    new LspResultAggregator()));

            var edit = await coordinator.CoordinateAsync(
                document,
                new LspPosition { Line = 0, Character = 2 },
                "ProfileBadge",
                CreateJazorTarget(document),
                CancellationToken.None);

            Assert.IsNotNull(edit);
            var currentUri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath);
            var secondUri = LspProtocolHelpers.ToDocumentUri(secondDocumentPath);
            Assert.IsTrue(edit.Changes.ContainsKey(currentUri));
            Assert.IsTrue(edit.Changes.ContainsKey(secondUri));
            Assert.IsFalse(edit.Changes.ContainsKey(LspProtocolHelpers.ToDocumentUri(vuePath)));
            Assert.IsTrue(edit.Changes.Values.SelectMany(static changes => changes).All(static change => change.NewText == "ProfileBadge"));
        }
        finally
        {
            DeleteDirectory(tempDirectory);
        }
    }

    [TestMethod]
    public async Task JazorVueHost_MarkupBridgeFanoutCoordinator_Definition_WithFallbackDisabled_DeduplicatesNativeLocationsOnly()
    {
        var document = new DocumentSnapshot(
            @"D:\temp\Counter.jazor",
            DocumentKind.Jazor,
            "<Counter />",
            "1");
        var duplicatedLocation = new LspLocation
        {
            Uri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath),
            Range = new LspRange
            {
                Start = new LspPosition { Line = 0, Character = 1 },
                End = new LspPosition { Line = 0, Character = 8 }
            }
        };
        var coordinator = new MarkupBridgeFanoutCoordinator(
            new MarkupComponentBridgeService(new InMemoryWorkspaceStore()),
            new LspResultAggregator());

        var locations = await coordinator.CoordinateDefinitionAsync(
            document,
            new LspPosition { Line = 0, Character = 2 },
            [duplicatedLocation, duplicatedLocation],
            allowMarkupFallback: false,
            CancellationToken.None);

        Assert.AreEqual(1, locations.Count);
        Assert.AreEqual(duplicatedLocation.Uri, locations[0].Uri);
        Assert.AreEqual(duplicatedLocation.Range.Start.Character, locations[0].Range.Start.Character);
    }

    [TestMethod]
    public async Task JazorVueHost_ReferenceCoordinator_WithoutBridgeSymbol_DeduplicatesDuplicateLaneLocations()
    {
        var document = new DocumentSnapshot(
            @"D:\temp\consumer.ts",
            DocumentKind.TypeScript,
            "const value = target;",
            "1");
        var duplicatedLocation = new LspLocation
        {
            Uri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath),
            Range = new LspRange
            {
                Start = new LspPosition { Line = 0, Character = 14 },
                End = new LspPosition { Line = 0, Character = 20 }
            }
        };
        var coordinator = new ReferenceCoordinator(
            new Dictionary<LaneKind, ILspLane>
            {
                [LaneKind.Volar] = new FakeLane
                {
                    References = [duplicatedLocation, duplicatedLocation]
                }
            },
            new LspLaneRouter(),
            new MarkupBridgeFanoutCoordinator(
                new MarkupComponentBridgeService(new InMemoryWorkspaceStore()),
                new LspResultAggregator()));

        var locations = await coordinator.CoordinateAsync(
            document,
            new LspPosition { Line = 0, Character = 14 },
            includeDeclaration: true,
            CreateVolarTarget(document),
            CancellationToken.None);

        Assert.AreEqual(1, locations.Count);
        Assert.AreEqual(duplicatedLocation.Uri, locations[0].Uri);
        Assert.AreEqual(duplicatedLocation.Range.Start.Character, locations[0].Range.Start.Character);
    }

    [TestMethod]
    public async Task JazorVueHost_RenameCoordinator_WithoutBridgeSymbol_DeduplicatesDuplicateNativeTextEdits()
    {
        var document = new DocumentSnapshot(
            @"D:\temp\consumer.ts",
            DocumentKind.TypeScript,
            "const value = target;",
            "1");
        var duplicateEdit = new LspTextEdit
        {
            Range = new LspRange
            {
                Start = new LspPosition { Line = 0, Character = 14 },
                End = new LspPosition { Line = 0, Character = 20 }
            },
            NewText = "renamed"
        };
        var scriptUri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath);
        var coordinator = new RenameCoordinator(
            new Dictionary<LaneKind, ILspLane>
            {
                [LaneKind.Volar] = new FakeLane
                {
                    RenameEdit = new LspWorkspaceEdit
                    {
                        Changes = new Dictionary<string, LspTextEdit[]>(StringComparer.Ordinal)
                        {
                            [scriptUri] = [duplicateEdit, duplicateEdit]
                        }
                    }
                }
            },
            new LspLaneRouter(),
            new LspResultAggregator(),
            new MarkupBridgeFanoutCoordinator(
                new MarkupComponentBridgeService(new InMemoryWorkspaceStore()),
                new LspResultAggregator()));

        var edit = await coordinator.CoordinateAsync(
            document,
            new LspPosition { Line = 0, Character = 14 },
            "renamed",
            CreateVolarTarget(document),
            CancellationToken.None);

        Assert.IsNotNull(edit);
        Assert.IsTrue(edit.Changes.ContainsKey(scriptUri));
        Assert.AreEqual(1, edit.Changes[scriptUri].Length);
        Assert.AreEqual("renamed", edit.Changes[scriptUri][0].NewText);
    }

    private static ProjectionTarget CreateVolarTarget(DocumentSnapshot document)
        => new(
            LaneKind.Volar,
            DocumentRegionKind.Code,
            document.DocumentPath,
            MappingId: "test-volar");

    private static ProjectionTarget CreateJazorTarget(DocumentSnapshot document)
        => new(
            LaneKind.Jazor,
            DocumentRegionKind.Template,
            document.DocumentPath,
            MappingId: "test-jazor");

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "jazor-vuehost-coordinator-" + Guid.NewGuid().ToString("N", System.Globalization.CultureInfo.InvariantCulture));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private sealed class FakeLane : ILspLane
    {
        public LaneKind LaneKind => LaneKind.Volar;

        public IReadOnlyList<LspLocation> References { get; init; } = Array.Empty<LspLocation>();

        public LspWorkspaceEdit? RenameEdit { get; init; }

        public ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(
            DocumentSnapshot document,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDiagnostic>>(Array.Empty<LspDiagnostic>());

        public ValueTask<LspHoverResult?> GetHoverAsync(
            DocumentSnapshot document,
            LspPosition position,
            ProjectionTarget projectionTarget,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<LspHoverResult?>(null);

        public ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
            DocumentSnapshot document,
            LspPosition position,
            ProjectionTarget projectionTarget,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(Array.Empty<LspCompletionItem>());

        public ValueTask<IReadOnlyList<LspDocumentSymbol>> GetDocumentSymbolsAsync(
            DocumentSnapshot document,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDocumentSymbol>>(Array.Empty<LspDocumentSymbol>());

        public ValueTask<IReadOnlyList<LspSemanticToken>> GetSemanticTokensAsync(
            DocumentSnapshot document,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspSemanticToken>>(Array.Empty<LspSemanticToken>());

        public ValueTask<LspSignatureHelp?> GetSignatureHelpAsync(
            DocumentSnapshot document,
            LspPosition position,
            ProjectionTarget projectionTarget,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<LspSignatureHelp?>(null);

        public ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
            DocumentSnapshot document,
            LspPosition position,
            ProjectionTarget projectionTarget,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

        public ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(
            DocumentSnapshot document,
            LspPosition position,
            bool includeDeclaration,
            ProjectionTarget projectionTarget,
            CancellationToken cancellationToken)
            => ValueTask.FromResult(References);

        public ValueTask<LspWorkspaceEdit?> GetRenameAsync(
            DocumentSnapshot document,
            LspPosition position,
            string newName,
            ProjectionTarget projectionTarget,
            CancellationToken cancellationToken)
            => ValueTask.FromResult(RenameEdit);

        public ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(
            DocumentSnapshot document,
            LspRange range,
            IReadOnlyList<LspDiagnostic> diagnostics,
            ProjectionTarget projectionTarget,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(Array.Empty<LspCodeAction>());
    }
}
