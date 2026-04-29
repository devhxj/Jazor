using Jolt.Analysis;
using Jolt.Extensions;
using Jolt.Jazor.Projection;
using Jolt.Lsp;
using Jolt.Lsp.Aggregation;
using Jolt.Lsp.Coordination;
using Jolt.Lsp.Lanes;
using Jolt.Lsp.Routing;
using ECMAScript.Internal.VueContracts.Protocol;
using Jolt.VirtualDocuments.Registry;
using Jolt.Workspace;

namespace Jolt.Test;

[TestClass]
public sealed class JoltLspSessionSliceTests
{
    [TestMethod]
    public void LspProtocolHelpers_GetOffset_RejectsOutOfRangePositions()
    {
        const string text = "first\nsecond";

        Assert.AreEqual(text.Length, LspProtocolHelpers.GetOffset(
            text,
            new LspPosition { Line = 1, Character = 6 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => LspProtocolHelpers.GetOffset(
            text,
            new LspPosition { Line = 2, Character = 0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => LspProtocolHelpers.GetOffset(
            text,
            new LspPosition { Line = 0, Character = 99 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => LspProtocolHelpers.GetOffset(
            text,
            new LspPosition { Line = -1, Character = 0 }));
    }

    [TestMethod]
    public async Task LspSession_Rename_RejectsInvalidNewNameBeforeLaneInvocation()
    {
        var documentPath = Path.Combine(Path.GetTempPath(), $"rename-invalid-{Guid.NewGuid():N}.jazor");
        var documentUri = LspProtocolHelpers.ToDocumentUri(documentPath);
        var workspaceStore = new InMemoryWorkspaceStore();
        await workspaceStore.UpsertDocumentAsync(
            new DocumentSnapshot(documentPath, DocumentKind.Jazor, "<UserCard />", "1"),
            CancellationToken.None);

        var lane = new PrepareRenameLane(LaneKind.Jazor, "UserCard");
        var session = CreateSession(workspaceStore, lane);
        var invalidNames = new[] { "", "   ", "1UserCard", "User-Card", "User Card" };

        foreach (var invalidName in invalidNames)
        {
            await Assert.ThrowsAsync<LspRequestException>(async () =>
            {
                await session.HandleRequestAsync(
                    new LspRequestMessage
                    {
                        Id = 10,
                        Method = "textDocument/rename",
                        Params = new LspRenameParams
                        {
                            TextDocument = new LspTextDocumentIdentifier
                            {
                                Uri = documentUri
                            },
                            Position = new LspPosition
                            {
                                Line = 0,
                                Character = 2
                            },
                            NewName = invalidName
                        }
                    },
                    CancellationToken.None);
            });
        }

        Assert.IsNull(lane.LastRenamePosition);
    }

    [TestMethod]
    public async Task LspSession_WorkspaceFoldersSnapshot_ReturnsReadOnlyCollection()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), $"workspace-folder-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootDirectory);
        try
        {
            var session = CreateSession(new InMemoryWorkspaceStore());
            var response = await session.HandleRequestAsync(
                new LspRequestMessage
                {
                    Id = 11,
                    Method = "initialize",
                    Params = new LspInitializeParams
                    {
                        WorkspaceFolders =
                        [
                            new LspWorkspaceFolder
                            {
                                Uri = new Uri(rootDirectory).AbsoluteUri,
                                Name = "workspace"
                            }
                        ]
                    }
                },
                CancellationToken.None);

            Assert.IsNotNull(response);
            Assert.IsNull(response!.Error);

            var method = typeof(LspSession).GetMethod(
                "GetWorkspaceFoldersSnapshot",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(method);

            var snapshot = (IReadOnlyList<LspWorkspaceFolder>)method!.Invoke(session, null)!;
            Assert.AreEqual(1, snapshot.Count);
            Assert.IsFalse(snapshot is LspWorkspaceFolder[]);
            var mutableListView = (IList<LspWorkspaceFolder>)snapshot;
            Assert.Throws<NotSupportedException>(() =>
            {
                mutableListView[0] = new LspWorkspaceFolder { Uri = "file:///changed", Name = "changed" };
            });
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task LspSession_DocumentFormatting_RespectsTrimTrailingWhitespaceOption()
    {
        const string text = "<div>  \n  value\t";
        var documentPath = Path.Combine(Path.GetTempPath(), $"formatting-trim-{Guid.NewGuid():N}.jazor");
        var documentUri = LspProtocolHelpers.ToDocumentUri(documentPath);
        var workspaceStore = new InMemoryWorkspaceStore();
        await workspaceStore.UpsertDocumentAsync(
            new DocumentSnapshot(documentPath, DocumentKind.Jazor, text, "1"),
            CancellationToken.None);

        var session = CreateSession(workspaceStore);
        var response = await session.HandleRequestAsync(
            new LspRequestMessage
            {
                Id = 12,
                Method = "textDocument/formatting",
                Params = new LspDocumentFormattingParams
                {
                    TextDocument = new LspTextDocumentIdentifier
                    {
                        Uri = documentUri
                    },
                    Options = new LspFormattingOptions
                    {
                        TrimTrailingWhitespace = false
                    }
                }
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var edits = response.Result as IReadOnlyList<LspTextEdit>;
        Assert.IsNotNull(edits);
        Assert.AreEqual(0, edits.Count);
    }

    [TestMethod]
    public async Task LspSession_DocumentFormatting_PreservesMixedNewlineStyle()
    {
        const string text = "<div>  \r\n<span>\t\n<em>  ";
        var documentPath = Path.Combine(Path.GetTempPath(), $"formatting-newlines-{Guid.NewGuid():N}.jazor");
        var documentUri = LspProtocolHelpers.ToDocumentUri(documentPath);
        var workspaceStore = new InMemoryWorkspaceStore();
        await workspaceStore.UpsertDocumentAsync(
            new DocumentSnapshot(documentPath, DocumentKind.Jazor, text, "1"),
            CancellationToken.None);

        var session = CreateSession(workspaceStore);
        var response = await session.HandleRequestAsync(
            new LspRequestMessage
            {
                Id = 13,
                Method = "textDocument/formatting",
                Params = new LspDocumentFormattingParams
                {
                    TextDocument = new LspTextDocumentIdentifier
                    {
                        Uri = documentUri
                    },
                    Options = new LspFormattingOptions()
                }
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var edits = response.Result as IReadOnlyList<LspTextEdit>;
        Assert.IsNotNull(edits);
        Assert.AreEqual(1, edits.Count);
        Assert.AreEqual("<div>\r\n<span>\n<em>", edits[0].NewText);
    }

    [TestMethod]
    public async Task LspSession_PrepareRename_ExpandsKebabCaseTagAtWordEnd()
    {
        const string text = "<my-tag />";
        var documentPath = Path.Combine(Path.GetTempPath(), $"prepare-rename-{Guid.NewGuid():N}.jazor");
        var documentUri = LspProtocolHelpers.ToDocumentUri(documentPath);
        var workspaceStore = new InMemoryWorkspaceStore();
        await workspaceStore.UpsertDocumentAsync(
            new DocumentSnapshot(documentPath, DocumentKind.Jazor, text, "1"),
            CancellationToken.None);

        var renameLane = new PrepareRenameLane(LaneKind.Volar, "my-tag");
        var session = CreateSession(workspaceStore, renameLane);

        var response = await session.HandleRequestAsync(
            new LspRequestMessage
            {
                Id = 1,
                Method = "textDocument/prepareRename",
                Params = new LspPrepareRenameParams
                {
                    TextDocument = new LspTextDocumentIdentifier
                    {
                        Uri = documentUri
                    },
                    Position = new LspPosition
                    {
                        Line = 0,
                        Character = 7
                    }
                }
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var result = response.Result as LspPrepareRenameResult;
        Assert.IsNotNull(result);
        Assert.AreEqual("my-tag", result.Placeholder);
        Assert.AreEqual(0, result.Range.Start.Line);
        Assert.AreEqual(1, result.Range.Start.Character);
        Assert.AreEqual(0, result.Range.End.Line);
        Assert.AreEqual(7, result.Range.End.Character);
        Assert.AreEqual(0, renameLane.LastRenamePosition?.Line);
        Assert.AreEqual(6, renameLane.LastRenamePosition?.Character);
    }

    [TestMethod]
    public async Task LspSession_TypeDefinition_ForNonRoslynTargets_DoesNotFallbackToDefinition()
    {
        const string text = "<template>\n  <MyCard />\n</template>";
        var documentPath = Path.Combine(Path.GetTempPath(), $"type-definition-{Guid.NewGuid():N}.jazor");
        var documentUri = LspProtocolHelpers.ToDocumentUri(documentPath);
        var workspaceStore = new InMemoryWorkspaceStore();
        await workspaceStore.UpsertDocumentAsync(
            new DocumentSnapshot(documentPath, DocumentKind.Jazor, text, "1"),
            CancellationToken.None);

        var lane = new DefinitionOnlyLane();
        var session = CreateSession(workspaceStore, lane);

        var response = await session.HandleRequestAsync(
            new LspRequestMessage
            {
                Id = 2,
                Method = "textDocument/typeDefinition",
                Params = new LspTypeDefinitionParams
                {
                    TextDocument = new LspTextDocumentIdentifier
                    {
                        Uri = documentUri
                    },
                    Position = new LspPosition
                    {
                        Line = 1,
                        Character = 4
                    }
                }
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        Assert.AreEqual(0, lane.DefinitionCallCount);
        var result = response.Result as IReadOnlyList<LspLocation>;
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task JazorLaneService_GetImplementationAsync_ReturnsEmptyForTemplateComponent()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "jazor-implementation-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var componentName = "UserCard" + Guid.NewGuid().ToString("N")[..8];
            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, componentName + ".vue"),
                $"<template><div>{componentName}</div></template>");

            var workspaceStore = new InMemoryWorkspaceStore();
            var documentService = new JazorLspDocumentService(workspaceStore, new StubVueAnalysisClient());
            var lane = new JazorLaneService(documentService);
            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Counter.jazor"),
                DocumentKind.Jazor,
                $"<template>\n  <{componentName} />\n</template>",
                "1");
            await workspaceStore.UpsertDocumentAsync(document, CancellationToken.None);

            var result = await lane.GetImplementationAsync(
                document,
                new LspPosition { Line = 1, Character = 3 },
                new ProjectionTarget(
                    LaneKind.Jazor,
                    DocumentRegionKind.Template,
                    document.DocumentPath,
                    document.DocumentPath),
                CancellationToken.None);

            Assert.AreEqual(0, result.Count);
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
    public async Task JazorLspDocumentService_GetCodeActions_TargetsPrivateMethodNearDiagnostic()
    {
        const string text =
            """
            <template>
              <div />
            </template>

            @code {
                private void First()
                {
                }

                private void Target()
                {
                }
            }
            """;
        var document = new DocumentSnapshot(
            Path.Combine(Path.GetTempPath(), $"quick-fix-target-{Guid.NewGuid():N}.jazor"),
            DocumentKind.Jazor,
            text,
            "1");
        var service = new JazorLspDocumentService(
            new InMemoryWorkspaceStore(),
            new StubVueAnalysisClient());
        var targetOffset = text.IndexOf("Target", StringComparison.Ordinal);
        var diagnostic = new LspDiagnostic
        {
            Code = "JAZORVUE001",
            Source = "Jolt",
            Message = "private method is not bridge-visible",
            Range = LspProtocolHelpers.ToRange(text, targetOffset, "Target".Length)
        };

        var actions = await service.GetCodeActionsAsync(
            document,
            [diagnostic],
            CancellationToken.None);

        Assert.AreEqual(1, actions.Count);
        var edit = actions[0].Edit!.Changes[LspProtocolHelpers.ToDocumentUri(document.DocumentPath)].Single();
        var editOffset = LspProtocolHelpers.GetOffset(text, edit.Range.Start);
        var expectedPrivateOffset = text.LastIndexOf("private", targetOffset, StringComparison.Ordinal);
        Assert.AreEqual(expectedPrivateOffset, editOffset);
        Assert.AreEqual("public", edit.NewText);
    }

    private static LspSession CreateSession(
        IJoltWorkspaceStore workspaceStore,
        params ILspLane[] lanes)
    {
        var laneRouter = new LspLaneRouter();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var projectionResolver = new DocumentProjectionResolver(
            new DocumentRegionClassifier(),
            virtualDocumentRegistry);
        var projectionService = new JazorProjectionService();
        var resultAggregator = new LspResultAggregator();
        var markupBridgeService = new MarkupComponentBridgeService(workspaceStore);
        var markupBridgeFanout = new MarkupBridgeFanoutCoordinator(markupBridgeService, resultAggregator);
        var lanesByKind = lanes.ToDictionary(static lane => lane.LaneKind);

        return new LspSession(
            workspaceStore,
            lanes,
            laneRouter,
            new LspMessageWriter(new MemoryStream()),
            projectionService,
            virtualDocumentRegistry,
            projectionResolver,
            resultAggregator,
            markupBridgeFanout,
            new ReferenceCoordinator(lanesByKind, laneRouter, markupBridgeFanout),
            new RenameCoordinator(lanesByKind, laneRouter, resultAggregator, markupBridgeFanout),
            new CodeActionCoordinator(lanesByKind, laneRouter, resultAggregator),
            extensionRegistry: new ExtensionRegistry());
    }

    private sealed class PrepareRenameLane : ILspLane
    {
        private readonly string _renamableToken;

        public PrepareRenameLane(LaneKind laneKind, string renamableToken)
        {
            LaneKind = laneKind;
            _renamableToken = renamableToken;
        }

        public LaneKind LaneKind { get; }

        public LspPosition? LastRenamePosition { get; private set; }

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
            => ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

        public ValueTask<LspWorkspaceEdit?> GetRenameAsync(
            DocumentSnapshot document,
            LspPosition position,
            string newName,
            ProjectionTarget projectionTarget,
            CancellationToken cancellationToken)
        {
            LastRenamePosition = position;
            var start = document.Text.IndexOf(_renamableToken, StringComparison.Ordinal);
            var expectedPosition = LspProtocolHelpers.GetPosition(document.Text, start + _renamableToken.Length - 1);
            if (position.Line != expectedPosition.Line || position.Character != expectedPosition.Character)
            {
                return ValueTask.FromResult<LspWorkspaceEdit?>(null);
            }

            return ValueTask.FromResult<LspWorkspaceEdit?>(
                new LspWorkspaceEdit
                {
                    Changes = new Dictionary<string, LspTextEdit[]>
                    {
                        [LspProtocolHelpers.ToDocumentUri(document.DocumentPath)] =
                        [
                            new LspTextEdit
                            {
                                Range = new LspRange
                                {
                                    Start = expectedPosition,
                                    End = expectedPosition
                                },
                                NewText = newName
                            }
                        ]
                    }
                });
        }

        public ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(
            DocumentSnapshot document,
            LspRange range,
            IReadOnlyList<LspDiagnostic> diagnostics,
            ProjectionTarget projectionTarget,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(Array.Empty<LspCodeAction>());
    }

    private sealed class DefinitionOnlyLane : ILspLane
    {
        public LaneKind LaneKind => LaneKind.Volar;

        public int DefinitionCallCount { get; private set; }

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
        {
            DefinitionCallCount++;
            return ValueTask.FromResult<IReadOnlyList<LspLocation>>(
            [
                new LspLocation
                {
                    Uri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath),
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 1, Character = 2 },
                        End = new LspPosition { Line = 1, Character = 8 }
                    }
                }
            ]);
        }

        public ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(
            DocumentSnapshot document,
            LspPosition position,
            bool includeDeclaration,
            ProjectionTarget projectionTarget,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

        public ValueTask<LspWorkspaceEdit?> GetRenameAsync(
            DocumentSnapshot document,
            LspPosition position,
            string newName,
            ProjectionTarget projectionTarget,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<LspWorkspaceEdit?>(null);

        public ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(
            DocumentSnapshot document,
            LspRange range,
            IReadOnlyList<LspDiagnostic> diagnostics,
            ProjectionTarget projectionTarget,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(Array.Empty<LspCodeAction>());
    }

    private sealed class StubVueAnalysisClient : IVueAnalysisClient
    {
        public ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
            AnalyzeJazorRequest request,
            CancellationToken cancellationToken)
            => ValueTask.FromResult(new AnalyzeJazorResponse([], [], [], []));
    }
}
