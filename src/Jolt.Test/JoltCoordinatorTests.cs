using Jolt.Lsp;
using Jolt.Lsp.Aggregation;
using Jolt.Lsp.Coordination;
using Jolt.Lsp.Lanes;
using Jolt.Lsp.Routing;
using ECMAScript.Internal.VueContracts.Protocol;
using Jolt.Workspace;

namespace Jolt.Test;

[TestClass]
public sealed class JoltCoordinatorTests
{
    [TestMethod]
    public void LspResultAggregator_AggregateCodeActions_PreservesDistinctEditsWithSameTitleAndKind()
    {
        var aggregator = new LspResultAggregator();
        var first = new LspCodeAction
        {
            Title = "Import component",
            Kind = "quickfix",
            Edit = new LspWorkspaceEdit
            {
                Changes = new Dictionary<string, LspTextEdit[]>
                {
                    ["file:///component-a.jazor"] =
                    [
                        new LspTextEdit
                        {
                            Range = new LspRange
                            {
                                Start = new LspPosition { Line = 0, Character = 0 },
                                End = new LspPosition { Line = 0, Character = 0 }
                            },
                            NewText = "import A from './A.vue';\n"
                        }
                    ]
                }
            }
        };
        var second = new LspCodeAction
        {
            Title = "Import component",
            Kind = "quickfix",
            Edit = new LspWorkspaceEdit
            {
                Changes = new Dictionary<string, LspTextEdit[]>
                {
                    ["file:///component-b.jazor"] =
                    [
                        new LspTextEdit
                        {
                            Range = new LspRange
                            {
                                Start = new LspPosition { Line = 0, Character = 0 },
                                End = new LspPosition { Line = 0, Character = 0 }
                            },
                            NewText = "import B from './B.vue';\n"
                        }
                    ]
                }
            }
        };

        var aggregated = aggregator.AggregateCodeActions([first, second]);

        Assert.AreEqual(2, aggregated.Count);
    }

    [TestMethod]
    public void LspResultAggregator_AggregateDiagnostics_DeduplicatesByRangeAndMessage()
    {
        var aggregator = new LspResultAggregator();
        var first = new LspDiagnostic
        {
            Range = new LspRange
            {
                Start = new LspPosition { Line = 1, Character = 2 },
                End = new LspPosition { Line = 1, Character = 6 }
            },
            Code = "JAZOR001",
            Source = "jolt",
            Message = "duplicate"
        };
        var duplicate = new LspDiagnostic
        {
            Range = new LspRange
            {
                Start = new LspPosition { Line = 1, Character = 2 },
                End = new LspPosition { Line = 1, Character = 6 }
            },
            Code = "JAZOR001",
            Source = "jolt",
            Message = "duplicate"
        };
        var distinct = new LspDiagnostic
        {
            Range = new LspRange
            {
                Start = new LspPosition { Line = 1, Character = 2 },
                End = new LspPosition { Line = 1, Character = 7 }
            },
            Code = "JAZOR001",
            Source = "jolt",
            Message = "duplicate"
        };

        var aggregated = aggregator.AggregateDiagnostics([first, duplicate, distinct]);

        Assert.AreEqual(2, aggregated.Count);
        Assert.AreSame(first, aggregated[0]);
        Assert.AreSame(distinct, aggregated[1]);
    }

    [TestMethod]
    public async Task Jolt_MarkupBridgeFanoutCoordinator_Definition_FallsBackToVueFileWhenNativeDefinitionIsEmpty()
    {
        using var topology = CreateSingleProjectTopology(
            nameof(Jolt_MarkupBridgeFanoutCoordinator_Definition_FallsBackToVueFileWhenNativeDefinitionIsEmpty),
            out var project);
        var componentName = "UserBadge" + Guid.NewGuid().ToString("N", System.Globalization.CultureInfo.InvariantCulture);
        var vuePath = project.WriteFile(componentName + ".vue", $"<template><div>{componentName}</div></template>");

        var scriptDocument = CreateDocumentSnapshot(
            project,
            "consumer.ts",
            DocumentKind.TypeScript,
            $"""
            import {componentName} from "./{componentName}.vue";
            export const current = {componentName};
            """);

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

    [TestMethod]
    public async Task Jolt_ReferenceCoordinator_ForTypeScriptVueImport_FansOutIntoJazorWithoutDeclarationWhenExcluded()
    {
        using var topology = CreateSingleProjectTopology(
            nameof(Jolt_ReferenceCoordinator_ForTypeScriptVueImport_FansOutIntoJazorWithoutDeclarationWhenExcluded),
            out var project);
        var componentName = "UserBadge" + Guid.NewGuid().ToString("N", System.Globalization.CultureInfo.InvariantCulture);
        var vuePath = project.WriteFile(componentName + ".vue", $"<template><div>{componentName}</div></template>");

        var scriptDocument = CreateDocumentSnapshot(
            project,
            "consumer.ts",
            DocumentKind.TypeScript,
            $"""
            import {componentName} from "./{componentName}.vue";
            export const current = {componentName};
            """);

        var openJazorDocument = CreateDocumentSnapshot(
            project,
            "Counter.jazor",
            DocumentKind.Jazor,
            $"<{componentName} />");

        var diskJazorPath = project.WriteFile(
            "Dashboard.jazor",
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

    [TestMethod]
    public async Task Jolt_RenameCoordinator_ForTypeScriptVueImport_MergesNativeAndJazorEdits()
    {
        using var topology = CreateSingleProjectTopology(
            nameof(Jolt_RenameCoordinator_ForTypeScriptVueImport_MergesNativeAndJazorEdits),
            out var project);
        var componentName = "UserBadge" + Guid.NewGuid().ToString("N", System.Globalization.CultureInfo.InvariantCulture);
        project.WriteFile(componentName + ".vue", $"<template><div>{componentName}</div></template>");

        var scriptDocument = CreateDocumentSnapshot(
            project,
            "consumer.ts",
            DocumentKind.TypeScript,
            $"""
            import {componentName} from "./{componentName}.vue";
            export const current = {componentName};
            """);

        var openJazorDocument = CreateDocumentSnapshot(
            project,
            "Counter.jazor",
            DocumentKind.Jazor,
            $"<{componentName} />");

        var diskJazorPath = project.WriteFile(
            "Dashboard.jazor",
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

    [TestMethod]
    public async Task Jolt_ReferenceCoordinator_ForJazorMarkupTag_FansOutWithoutLaneLocations()
    {
        using var topology = CreateSingleProjectTopology(
            nameof(Jolt_ReferenceCoordinator_ForJazorMarkupTag_FansOutWithoutLaneLocations),
            out var project);
        var componentName = "UserBadge" + Guid.NewGuid().ToString("N", System.Globalization.CultureInfo.InvariantCulture);
        var vuePath = project.WriteFile(componentName + ".vue", $"<template><div>{componentName}</div></template>");

        var document = CreateDocumentSnapshot(
            project,
            "Counter.jazor",
            DocumentKind.Jazor,
            $"<{componentName} />");

        var secondDocumentPath = project.WriteFile(
            "Dashboard.jazor",
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

    [TestMethod]
    public async Task Jolt_RenameCoordinator_ForJazorMarkupTag_FansOutWithoutLaneEdits()
    {
        using var topology = CreateSingleProjectTopology(
            nameof(Jolt_RenameCoordinator_ForJazorMarkupTag_FansOutWithoutLaneEdits),
            out var project);
        var componentName = "UserBadge" + Guid.NewGuid().ToString("N", System.Globalization.CultureInfo.InvariantCulture);
        var vuePath = project.WriteFile(componentName + ".vue", $"<template><div>{componentName}</div></template>");

        var document = CreateDocumentSnapshot(
            project,
            "Counter.jazor",
            DocumentKind.Jazor,
            $"<{componentName} />");

        var secondDocumentPath = project.WriteFile(
            "Dashboard.jazor",
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

    [TestMethod]
    public async Task Jolt_MarkupBridgeFanoutCoordinator_Definition_WithFallbackDisabled_DeduplicatesNativeLocationsOnly()
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
    public async Task Jolt_ReferenceCoordinator_WithoutBridgeSymbol_DeduplicatesDuplicateLaneLocations()
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
    public async Task Jolt_RenameCoordinator_WithoutBridgeSymbol_DeduplicatesDuplicateNativeTextEdits()
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

    private static JoltIntegrationTestTopology CreateSingleProjectTopology(
        string scenarioName,
        out JoltIntegrationProject project)
    {
        var topology = JoltIntegrationTestTopology.Create(scenarioName);
        project = topology.CreateSingleProjectSolution("TestSolution", "TestProject");
        return topology;
    }

    private static DocumentSnapshot CreateDocumentSnapshot(
        JoltIntegrationProject project,
        string relativePath,
        DocumentKind documentKind,
        string text)
        => new(
            project.WriteFile(relativePath, text),
            documentKind,
            text,
            "1");

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

        public ValueTask<IReadOnlyList<LspDocumentHighlight>> GetDocumentHighlightsAsync(
            DocumentSnapshot document,
            LspPosition position,
            ProjectionTarget projectionTarget,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDocumentHighlight>>(Array.Empty<LspDocumentHighlight>());

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
