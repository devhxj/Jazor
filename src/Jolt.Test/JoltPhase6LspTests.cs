using Jazor.Vue;
using Jazor.VueContracts.Protocol;
using Jolt.Analysis;
using Jolt.Jazor.Projection;
using Jolt.Lsp;
using Jolt.Lsp.Aggregation;
using Jolt.Lsp.Coordination;
using Jolt.Lsp.Lanes;
using Jolt.Lsp.Routing;
using Jolt.Roslyn.InProc;
using Jolt.VirtualDocuments.Mapping;
using Jolt.VirtualDocuments.Models;
using Jolt.VirtualDocuments.Registry;
using Jolt.Workspace;

namespace Jolt.Test;

[TestClass]
public sealed class JoltPhase6LspTests
{
    #region Semantic Token Legend

    [TestMethod]
    public void LspSemanticTokenLegend_ContainsExpandedTokenTypes()
    {
        Assert.IsTrue(LspSemanticTokenLegend.TokenTypes.Contains("decorator"));
        Assert.IsTrue(LspSemanticTokenLegend.TokenTypes.Contains("type"));
        Assert.IsTrue(LspSemanticTokenLegend.TokenTypes.Contains("function"));
        Assert.IsTrue(LspSemanticTokenLegend.TokenTypes.Contains("enum"));
        Assert.IsTrue(LspSemanticTokenLegend.TokenTypes.Contains("interface"));
        Assert.IsTrue(LspSemanticTokenLegend.TokenTypes.Contains("namespace"));
    }

    [TestMethod]
    public void LspSemanticTokenLegend_ContainsExpandedModifiers()
    {
        Assert.IsTrue(LspSemanticTokenLegend.TokenModifiers.Contains("abstract"));
        Assert.IsTrue(LspSemanticTokenLegend.TokenModifiers.Contains("async"));
        Assert.IsTrue(LspSemanticTokenLegend.TokenModifiers.Contains("modification"));
    }

    [TestMethod]
    public void LspSemanticTokenLegend_StillContainsOriginalTypes()
    {
        Assert.IsTrue(LspSemanticTokenLegend.TokenTypes.Contains("class"));
        Assert.IsTrue(LspSemanticTokenLegend.TokenTypes.Contains("method"));
        Assert.IsTrue(LspSemanticTokenLegend.TokenTypes.Contains("property"));
        Assert.IsTrue(LspSemanticTokenLegend.TokenTypes.Contains("parameter"));
        Assert.IsTrue(LspSemanticTokenLegend.TokenTypes.Contains("variable"));
        Assert.IsTrue(LspSemanticTokenLegend.TokenTypes.Contains("keyword"));
        Assert.IsTrue(LspSemanticTokenLegend.TokenTypes.Contains("string"));
        Assert.IsTrue(LspSemanticTokenLegend.TokenTypes.Contains("number"));
    }

    [TestMethod]
    public void LspSemanticTokenLegend_EncodeDecode_Roundtrip_WithNewTypes()
    {
        var tokens = new List<LspSemanticToken>
        {
            new() { Line = 0, Character = 0, Length = 5, TokenType = "decorator" },
            new() { Line = 1, Character = 4, Length = 3, TokenType = "type" },
            new() { Line = 1, Character = 10, Length = 8, TokenType = "function", TokenModifiers = ["async"] },
            new() { Line = 2, Character = 0, Length = 6, TokenType = "enum", TokenModifiers = ["declaration", "readonly"] },
        };

        var encoded = LspSemanticTokenLegend.Encode(tokens);
        var decoded = LspSemanticTokenLegend.Decode(encoded.Data);

        Assert.AreEqual(tokens.Count, decoded.Count);
        for (var i = 0; i < tokens.Count; i++)
        {
            Assert.AreEqual(tokens[i].Line, decoded[i].Line, $"Token {i}: Line mismatch");
            Assert.AreEqual(tokens[i].Character, decoded[i].Character, $"Token {i}: Character mismatch");
            Assert.AreEqual(tokens[i].Length, decoded[i].Length, $"Token {i}: Length mismatch");
            Assert.AreEqual(tokens[i].TokenType, decoded[i].TokenType, $"Token {i}: TokenType mismatch");
        }
    }

    [TestMethod]
    public void LspSemanticTokenLegend_GetTokenTypeIndex_ReturnsCorrectIndex()
    {
        Assert.AreEqual(0, LspSemanticTokenLegend.GetTokenTypeIndex("class"));
        Assert.AreEqual(5, LspSemanticTokenLegend.GetTokenTypeIndex("keyword"));
        Assert.AreEqual(8, LspSemanticTokenLegend.GetTokenTypeIndex("decorator"));
    }

    [TestMethod]
    public void LspSemanticTokenLegend_CreateDescriptor_IncludesAllTypes()
    {
        var descriptor = LspSemanticTokenLegend.CreateDescriptor();
        Assert.AreEqual(LspSemanticTokenLegend.TokenTypes.Length, descriptor.TokenTypes.Length);
        Assert.AreEqual(LspSemanticTokenLegend.TokenModifiers.Length, descriptor.TokenModifiers.Length);
    }

    #endregion

    #region Template Projection Routing

    [TestMethod]
    public async Task DocumentProjectionResolver_ResolveAsync_UsesProjectedVueDocumentForJazorTemplateRequests()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"projection-{Guid.NewGuid():N}.jazor");
        var sourceText =
            """
            <template>
              <UserCard />
            </template>
            """;
        var projectedText =
            """
            <template>
              <UserCard />
            </template>
            """;

        var registry = new InMemoryVirtualDocumentRegistry();
        await registry.UpsertAsync(
        [
            new VirtualDocument(
                new VirtualDocumentIdentity(
                    sourcePath,
                    "virtual:" + sourcePath + ".g.vue",
                    VirtualDocumentKind.Vue),
                projectedText,
                ProjectionMap.CreateWholeDocument(sourcePath, "virtual:" + sourcePath + ".g.vue", sourceText.Length, projectedText.Length),
                version: "1")
        ],
            CancellationToken.None);

        var resolver = new DocumentProjectionResolver(new DocumentRegionClassifier(), registry);
        var target = await resolver.ResolveAsync(
            new DocumentSnapshot(sourcePath, DocumentKind.Jazor, sourceText, "1"),
            new LspPosition { Line = 1, Character = 3 },
            CancellationToken.None);

        Assert.AreEqual(LaneKind.Volar, target.LaneKind);
        Assert.AreEqual(DocumentRegionKind.Template, target.RegionKind);
        StringAssert.EndsWith(target.ProjectedDocumentPath, ".g.vue", StringComparison.Ordinal);
        Assert.IsTrue(target.IsProjected);
    }

    [TestMethod]
    public async Task DocumentProjectionResolver_ResolveAsync_FallsBackWhenPrimaryVueProjectionIsMissing()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"projection-{Guid.NewGuid():N}.jazor");
        var sourceText =
            """
            <template>
              <UserCard />
            </template>
            """;
        var registry = new InMemoryVirtualDocumentRegistry();
        await registry.UpsertAsync(
        [
            new VirtualDocument(
                new VirtualDocumentIdentity(
                    sourcePath,
                    "virtual:" + sourcePath + ".template-only.vue",
                    VirtualDocumentKind.Vue),
                sourceText,
                ProjectionMap.CreateWholeDocument(sourcePath, "virtual:" + sourcePath + ".template-only.vue", sourceText.Length, sourceText.Length),
                version: "1")
        ],
            CancellationToken.None);

        var resolver = new DocumentProjectionResolver(new DocumentRegionClassifier(), registry);
        var target = await resolver.ResolveAsync(
            new DocumentSnapshot(sourcePath, DocumentKind.Jazor, sourceText, "1"),
            new LspPosition { Line = 1, Character = 3 },
            CancellationToken.None);

        Assert.AreEqual(LaneKind.Volar, target.LaneKind);
        Assert.AreEqual(DocumentRegionKind.Template, target.RegionKind);
        Assert.IsFalse(target.IsProjected);
        Assert.AreEqual(sourcePath, target.ProjectedDocumentPath);
        Assert.AreEqual(sourcePath, target.MappingId);
    }

    [TestMethod]
    public async Task DocumentProjectionResolver_ResolveAsync_RoutesFunctionsBlockToRoslyn()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"projection-{Guid.NewGuid():N}.jazor");
        var sourceText =
            """
            @functions {
                private string Title => "Hello";
            }

            <template>
              <div>@Title</div>
            </template>
            """;

        var resolver = new DocumentProjectionResolver(
            new DocumentRegionClassifier(),
            new InMemoryVirtualDocumentRegistry());
        var target = await resolver.ResolveAsync(
            new DocumentSnapshot(sourcePath, DocumentKind.Jazor, sourceText, "1"),
            new LspPosition { Line = 1, Character = 12 },
            CancellationToken.None);

        Assert.AreEqual(LaneKind.Roslyn, target.LaneKind);
        Assert.AreEqual(DocumentRegionKind.Code, target.RegionKind);
        Assert.IsFalse(target.IsProjected);
    }

    [TestMethod]
    public async Task DocumentProjectionResolver_ResolveAsync_RoutesStandardRazorDirectiveToRoslynWhenProjectionMaps()
    {
        var document = new DocumentSnapshot(
            Path.Combine(Path.GetTempPath(), $"projection-{Guid.NewGuid():N}.jazor"),
            DocumentKind.Jazor,
            """
            @using Demo

            <template>
              <div>Hello</div>
            </template>
            """,
            "1");
        var registry = new InMemoryVirtualDocumentRegistry();
        var projectionService = new JazorProjectionService();
        await registry.UpsertAsync(
            await projectionService.ProjectAsync(document, CancellationToken.None),
            CancellationToken.None);

        var resolver = new DocumentProjectionResolver(new DocumentRegionClassifier(), registry);
        var target = await resolver.ResolveAsync(
            document,
            ToPosition(document.Text, "Demo", advance: 1),
            CancellationToken.None);

        Assert.AreEqual(LaneKind.Roslyn, target.LaneKind);
        Assert.AreEqual(DocumentRegionKind.Directive, target.RegionKind);
        Assert.IsTrue(target.IsProjected);
        Assert.IsNotNull(target.ProjectedPosition);
        Assert.IsTrue(
            target.ProjectedDocumentPath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase),
            $"Expected a projected C# document path, got '{target.ProjectedDocumentPath}'.");
    }

    [TestMethod]
    public async Task DocumentProjectionResolver_ResolveAsync_RoutesStandardRazorDirectiveToRoslynWithoutProjectionMap()
    {
        var document = new DocumentSnapshot(
            Path.Combine(Path.GetTempPath(), $"projection-{Guid.NewGuid():N}.jazor"),
            DocumentKind.Jazor,
            """
            @using Demo

            <template>
              <div>Hello</div>
            </template>
            """,
            "1");

        var resolver = new DocumentProjectionResolver(
            new DocumentRegionClassifier(),
            new InMemoryVirtualDocumentRegistry());
        var target = await resolver.ResolveAsync(
            document,
            ToPosition(document.Text, "Demo", advance: 1),
            CancellationToken.None);

        Assert.AreEqual(LaneKind.Roslyn, target.LaneKind);
        Assert.AreEqual(DocumentRegionKind.Directive, target.RegionKind);
        Assert.IsFalse(target.IsProjected);
        Assert.AreEqual(document.DocumentPath, target.ProjectedDocumentPath);
        Assert.AreEqual(document.DocumentPath, target.MappingId);
    }

    [TestMethod]
    public async Task DocumentProjectionResolver_ResolveAsync_LeavesModuleDirectiveOnJazorLane()
    {
        var document = new DocumentSnapshot(
            Path.Combine(Path.GetTempPath(), $"projection-{Guid.NewGuid():N}.jazor"),
            DocumentKind.Jazor,
            """
            @module CounterState from "./counter-state.ts"

            <template>
              <div>Hello</div>
            </template>
            """,
            "1");
        var registry = new InMemoryVirtualDocumentRegistry();
        var projectionService = new JazorProjectionService();
        await registry.UpsertAsync(
            await projectionService.ProjectAsync(document, CancellationToken.None),
            CancellationToken.None);

        var resolver = new DocumentProjectionResolver(new DocumentRegionClassifier(), registry);
        var target = await resolver.ResolveAsync(
            document,
            ToPosition(document.Text, "CounterState", advance: 1),
            CancellationToken.None);

        Assert.AreEqual(LaneKind.Jazor, target.LaneKind);
        Assert.AreEqual(DocumentRegionKind.Directive, target.RegionKind);
        Assert.IsFalse(target.IsProjected);
        Assert.AreEqual(document.DocumentPath, target.ProjectedDocumentPath);
        Assert.AreEqual(document.DocumentPath, target.MappingId);
    }

    [TestMethod]
    public async Task DocumentProjectionResolver_ResolveAsync_LeavesModuleDirectiveOnJazorLane_WhenNoBlockCodeDirectivePrecedesRealCodeBlock()
    {
        var document = new DocumentSnapshot(
            Path.Combine(Path.GetTempPath(), $"projection-{Guid.NewGuid():N}.jazor"),
            DocumentKind.Jazor,
            """
            @code
            @module CounterState from "./counter-state.ts"

            @code {
              private int Count = 1;
            }
            """,
            "1");

        var resolver = new DocumentProjectionResolver(
            new DocumentRegionClassifier(),
            new InMemoryVirtualDocumentRegistry());
        var target = await resolver.ResolveAsync(
            document,
            ToPosition(document.Text, "CounterState", advance: 1),
            CancellationToken.None);

        Assert.AreEqual(LaneKind.Jazor, target.LaneKind);
        Assert.AreEqual(DocumentRegionKind.Directive, target.RegionKind);
        Assert.IsFalse(target.IsProjected);
        Assert.AreEqual(document.DocumentPath, target.ProjectedDocumentPath);
        Assert.AreEqual(document.DocumentPath, target.MappingId);
    }

    [TestMethod]
    public void DocumentRegionClassifier_Classify_CrLfBlankLineAfterDirective_RemainsDirective()
    {
        const string sourceText = "@page \"/\"\r\n\r\n<div>Hello</div>";
        var classifier = new DocumentRegionClassifier();

        var directiveGapOffset = LspProtocolHelpers.GetOffset(
            sourceText,
            new LspPosition { Line = 1, Character = 0 });
        var templateOffset = LspProtocolHelpers.GetOffset(
            sourceText,
            new LspPosition { Line = 2, Character = 1 });

        Assert.AreEqual(DocumentRegionKind.Directive, classifier.Classify(sourceText, directiveGapOffset));
        Assert.AreEqual(DocumentRegionKind.Template, classifier.Classify(sourceText, templateOffset));
    }

    [TestMethod]
    public void DocumentRegionClassifier_Classify_DirectiveOnlyDocumentAtEndOfFile_RemainsDirective()
    {
        const string sourceText = "@m";
        var classifier = new DocumentRegionClassifier();

        var endOfFileOffset = LspProtocolHelpers.GetOffset(
            sourceText,
            new LspPosition { Line = 0, Character = sourceText.Length });

        Assert.AreEqual(DocumentRegionKind.Directive, classifier.Classify(sourceText, endOfFileOffset));
    }

    [TestMethod]
    public void DocumentRegionClassifier_Classify_CommentedCodeDirectiveMarker_DoesNotCaptureFollowingDirective()
    {
        const string sourceText =
            """
            // @code {
            @module CounterState from "./counter-state.ts"
            """;
        var classifier = new DocumentRegionClassifier();

        var directiveOffset = LspProtocolHelpers.GetOffset(
            sourceText,
            new LspPosition { Line = 1, Character = 2 });

        Assert.AreEqual(DocumentRegionKind.Directive, classifier.Classify(sourceText, directiveOffset));
    }

    [TestMethod]
    public void DocumentRegionClassifier_Classify_BlockCommentedCodeDirectiveMarker_DoesNotCaptureFollowingDirective()
    {
        const string sourceText =
            """
            /*
            @code {
            */
            @module CounterState from "./counter-state.ts"
            """;
        var classifier = new DocumentRegionClassifier();

        var directiveOffset = LspProtocolHelpers.GetOffset(
            sourceText,
            new LspPosition { Line = 3, Character = 2 });

        Assert.AreEqual(DocumentRegionKind.Directive, classifier.Classify(sourceText, directiveOffset));
    }

    [TestMethod]
    public void DocumentRegionClassifier_Classify_RazorCommentedCodeDirectiveMarker_DoesNotCaptureFollowingDirective()
    {
        const string sourceText =
            """
            @*
            @code {
            *@
            @module CounterState from "./counter-state.ts"
            """;
        var classifier = new DocumentRegionClassifier();

        var directiveOffset = LspProtocolHelpers.GetOffset(
            sourceText,
            new LspPosition { Line = 3, Character = 2 });

        Assert.AreEqual(DocumentRegionKind.Directive, classifier.Classify(sourceText, directiveOffset));
    }

    [TestMethod]
    public void DocumentRegionClassifier_Classify_NoBlockCodeDirectiveBeforeRealCodeBlock_DoesNotCaptureDirectiveGap()
    {
        const string sourceText =
            """
            @code
            @module CounterState from "./counter-state.ts"

            @code {
                private int Count = 1;
            }
            """;
        var classifier = new DocumentRegionClassifier();

        var danglingCodeDirectiveOffset = LspProtocolHelpers.GetOffset(
            sourceText,
            new LspPosition { Line = 0, Character = 2 });
        var moduleDirectiveOffset = LspProtocolHelpers.GetOffset(
            sourceText,
            new LspPosition { Line = 1, Character = 2 });
        var realCodeOffset = LspProtocolHelpers.GetOffset(
            sourceText,
            new LspPosition { Line = 4, Character = 20 });

        Assert.AreEqual(DocumentRegionKind.Directive, classifier.Classify(sourceText, danglingCodeDirectiveOffset));
        Assert.AreEqual(DocumentRegionKind.Directive, classifier.Classify(sourceText, moduleDirectiveOffset));
        Assert.AreEqual(DocumentRegionKind.Code, classifier.Classify(sourceText, realCodeOffset));
    }

    [TestMethod]
    public void DocumentRegionClassifier_Classify_FunctionsBlock_IgnoresBracesInsideStringsAndComments()
    {
        const string sourceText =
            """
            @functions {
                private string Json => "}";
                /* } */
                private int Count => 1;
            }

            <template>
              <div>@Count</div>
            </template>
            """;
        var classifier = new DocumentRegionClassifier();

        var stringLineOffset = LspProtocolHelpers.GetOffset(
            sourceText,
            new LspPosition { Line = 1, Character = 24 });
        var commentLineOffset = LspProtocolHelpers.GetOffset(
            sourceText,
            new LspPosition { Line = 2, Character = 7 });
        var laterCodeLineOffset = LspProtocolHelpers.GetOffset(
            sourceText,
            new LspPosition { Line = 3, Character = 18 });
        var templateOffset = LspProtocolHelpers.GetOffset(
            sourceText,
            new LspPosition { Line = 7, Character = 3 });

        Assert.AreEqual(DocumentRegionKind.Code, classifier.Classify(sourceText, stringLineOffset));
        Assert.AreEqual(DocumentRegionKind.Code, classifier.Classify(sourceText, commentLineOffset));
        Assert.AreEqual(DocumentRegionKind.Code, classifier.Classify(sourceText, laterCodeLineOffset));
        Assert.AreEqual(DocumentRegionKind.Template, classifier.Classify(sourceText, templateOffset));
    }

    [TestMethod]
    public async Task LspSession_HandleRequestAsync_TemplateCompletion_ForColdDiskBackedJazor_UsesPrimaryProjectedVuePath()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentText =
                """
                <template>
                  <UserCard />
                </template>
                """;
            await File.WriteAllTextAsync(documentPath, documentText);

            var workspaceStore = new InMemoryWorkspaceStore();
            var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
            var laneRouter = new LspLaneRouter();
            var capturingLane = new CapturingVolarLane();
            ILspLane[] lanes = [capturingLane];
            var lanesByKind = lanes.ToDictionary(static lane => lane.LaneKind);
            using var outputStream = new MemoryStream();
            var writer = new LspMessageWriter(outputStream);
            var projectionService = new JazorProjectionService();
            var projectionResolver = new DocumentProjectionResolver(
                new DocumentRegionClassifier(),
                virtualDocumentRegistry);
            var resultAggregator = new LspResultAggregator();
            var markupBridgeService = new MarkupComponentBridgeService(workspaceStore);
            var markupBridgeFanout = new MarkupBridgeFanoutCoordinator(markupBridgeService, resultAggregator);
            var referenceCoordinator = new ReferenceCoordinator(lanesByKind, laneRouter, markupBridgeFanout);
            var renameCoordinator = new RenameCoordinator(lanesByKind, laneRouter, resultAggregator, markupBridgeFanout);
            var codeActionCoordinator = new CodeActionCoordinator(lanesByKind, laneRouter, resultAggregator);

            var session = new LspSession(
                workspaceStore,
                lanes,
                laneRouter,
                writer,
                projectionService,
                virtualDocumentRegistry,
                projectionResolver,
                resultAggregator,
                markupBridgeFanout,
                referenceCoordinator,
                renameCoordinator,
                codeActionCoordinator);

            var response = await session.HandleRequestAsync(
                new LspRequestMessage
                {
                    Id = 7001,
                    Method = "textDocument/completion",
                    Params = new LspCompletionParams
                    {
                        TextDocument = new LspTextDocumentIdentifier
                        {
                            Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                        },
                        Position = new LspPosition
                        {
                            Line = 1,
                            Character = 3
                        }
                    }
                },
                CancellationToken.None);

            Assert.IsNotNull(response);
            Assert.IsNull(response!.Error);
            Assert.IsNotNull(capturingLane.LastProjectionTarget);
            StringAssert.EndsWith(
                capturingLane.LastProjectionTarget!.ProjectedDocumentPath,
                ".g.vue",
                StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(capturingLane.LastProjectionTarget.IsProjected);

            var virtualDocuments = await virtualDocumentRegistry.GetBySourceDocumentAsync(
                documentPath,
                CancellationToken.None);
            Assert.IsTrue(
                virtualDocuments.Any(static document =>
                    document.Identity.DocumentKind == VirtualDocumentKind.Vue
                    && document.Identity.ProjectedDocumentPath.EndsWith(".g.vue", StringComparison.OrdinalIgnoreCase)),
                "Expected cold disk-backed Jazor completion request to materialize the primary projected .g.vue document.");
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void LspLaneRouter_GetSemanticTokenLanes_ForJazor_UsesFrontendAndRoslynOnly()
    {
        var router = new LspLaneRouter();
        var lanes = router.GetSemanticTokenLanes(
            new DocumentSnapshot("Counter.jazor", DocumentKind.Jazor, "<UserCard />", "1"));

        CollectionAssert.AreEqual(new[] { LaneKind.Volar, LaneKind.Roslyn }, lanes.ToArray());
    }

    #endregion

    #region Roslyn Unopened Disk Docs

    [TestMethod]
    public async Task InProcRoslynCodeService_Definition_ResolvesUnopenedDiskBackedCSharpDeclaration()
    {
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var declarationPath = Path.Combine(tempDirectory, "CounterLogic.cs");
            await File.WriteAllTextAsync(
                declarationPath,
                """
                internal static class CounterLogic
                {
                    public static int Count = 1;
                }
                """);

            var consumerPath = Path.Combine(tempDirectory, "CounterLogicConsumer.cs");
            var consumerText =
                """
                internal static class CounterLogicConsumer
                {
                    public static int Read()
                    {
                        return CounterLogic.Count;
                    }
                }
                """;
            await File.WriteAllTextAsync(consumerPath, consumerText);

            var service = new InProcRoslynCodeService();
            var consumer = new DocumentSnapshot(
                consumerPath,
                DocumentKind.CSharp,
                consumerText,
                version: "1");

            var discoveredSourceDocuments = await service.GetSourceDocumentsAsync(
                consumer,
                openDocuments: null,
                cancellationToken: CancellationToken.None);
            Assert.IsTrue(discoveredSourceDocuments.Any(document =>
                string.Equals(
                    JoltWorkspaceResolver.NormalizePath(document.DocumentPath),
                    JoltWorkspaceResolver.NormalizePath(declarationPath),
                    StringComparison.OrdinalIgnoreCase)),
                "Expected source discovery to include unopened CounterLogic.cs.");

            var markerStartOffset = consumerText.IndexOf("CounterLogic.Count", StringComparison.Ordinal) + "CounterLogic.".Length;
            var probeResults = new List<string>();
            IReadOnlyList<LspLocation> definitions = Array.Empty<LspLocation>();
            for (var delta = -1; delta <= 6; delta++)
            {
                var probeOffset = markerStartOffset + delta;
                if (probeOffset < 0 || probeOffset > consumerText.Length)
                {
                    continue;
                }

                var probePosition = LspProtocolHelpers.GetPosition(consumerText, probeOffset);
                var probeDefinitions = await service.GetDefinitionAsync(
                    consumer,
                    probePosition,
                    cancellationToken: CancellationToken.None);
                probeResults.Add($"delta={delta},defs={probeDefinitions.Count}");
                if (probeDefinitions.Count > 0)
                {
                    definitions = probeDefinitions;
                    break;
                }
            }
            var diagnostics = await service.GetDiagnosticsAsync(
                consumer,
                cancellationToken: CancellationToken.None);

            Assert.IsTrue(
                definitions.Count > 0,
                "Expected at least one definition for CounterLogic.Count. Diagnostics: "
                + string.Join(" | ", diagnostics.Select(static diagnostic => diagnostic.Message))
                + " ; probes: "
                + string.Join(", ", probeResults));
            Assert.IsTrue(definitions.Any(location =>
                string.Equals(
                    JoltWorkspaceResolver.NormalizePath(LspProtocolHelpers.ToDocumentPath(location.Uri)),
                    JoltWorkspaceResolver.NormalizePath(declarationPath),
                    StringComparison.OrdinalIgnoreCase)));
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
    public async Task InProcRoslynCodeService_Definition_ResolvesUnopenedDiskBackedDeclaration_WithForwardSlashPathAndOpenDocuments()
    {
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var declarationPath = Path.Combine(tempDirectory, "SharedState.cs");
            await File.WriteAllTextAsync(
                declarationPath,
                """
                namespace Demo;

                internal static class SharedState
                {
                    internal static int Count = 1;
                }
                """);

            var consumerDiskPath = Path.Combine(tempDirectory, "CounterConsumer.cs");
            var consumerText =
                """
                namespace Demo;

                internal static class CounterConsumer
                {
                    internal static int Read()
                    {
                        return SharedState.Count;
                    }
                }
                """;

            var consumerPath = JoltWorkspaceResolver.NormalizePath(consumerDiskPath);
            var service = new InProcRoslynCodeService();
            var consumer = new DocumentSnapshot(
                consumerPath,
                DocumentKind.CSharp,
                consumerText,
                version: "1");
            var usagePosition = ToPosition(
                consumerText,
                "SharedState.Count",
                advance: "SharedState.".Length + 1);

            var definitions = await service.GetDefinitionAsync(
                consumer,
                usagePosition,
                openDocuments: [consumer],
                cancellationToken: CancellationToken.None);
            var resolvedUris = string.Join(
                " | ",
                definitions.Select(static location => location.Uri));
            Assert.IsTrue(
                definitions.Any(location =>
                    string.Equals(
                        JoltWorkspaceResolver.NormalizePath(LspProtocolHelpers.ToDocumentPath(location.Uri)),
                        JoltWorkspaceResolver.NormalizePath(declarationPath),
                        StringComparison.OrdinalIgnoreCase)),
                "Expected definition to resolve into unopened SharedState.cs using normalized path + openDocuments. Resolved: "
                + resolvedUris);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    #endregion

    #region JazorLane Semantic Tokens

    [TestMethod]
    public async Task JazorLaneService_GetDiagnostics_UsesDefaultInProcAnalysisClient()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var workspaceStore = new InMemoryWorkspaceStore();
            var documentService = new JazorLspDocumentService(workspaceStore, VueAnalysisClientFactory.CreateDefault());
            var lane = new JazorLaneService(documentService);
            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, $"test-default-analysis-{Guid.NewGuid():N}.jazor"),
                DocumentKind.Jazor,
                """
                @vueimport UserCard from "./UserCard.vue"

                <template>
                  <UserCard />
                </template>
                """,
                "1");

            var diagnostics = await lane.GetDiagnosticsAsync(document, CancellationToken.None);

            Assert.IsTrue(diagnostics.Any(static diagnostic =>
                string.Equals(diagnostic.Code, LegacyImportDirectiveCatalog.DiagnosticCode, StringComparison.Ordinal)));
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task JazorLaneService_GetDiagnostics_FallsBackWhenAnalysisClientReturnsEmptyResponse()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var workspaceStore = new InMemoryWorkspaceStore();
            var documentService = CreateDocumentService(workspaceStore);
            var lane = new JazorLaneService(documentService);
            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, $"test-empty-analysis-fallback-{Guid.NewGuid():N}.jazor"),
                DocumentKind.Jazor,
                """
                @vueimport UserCard from "./UserCard.vue"

                <template>
                  <UserCard />
                </template>
                """,
                "1");

            var diagnostics = await lane.GetDiagnosticsAsync(document, CancellationToken.None);

            Assert.IsTrue(diagnostics.Any(static diagnostic =>
                string.Equals(diagnostic.Code, LegacyImportDirectiveCatalog.DiagnosticCode, StringComparison.Ordinal)));
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task JazorLaneService_GetSemanticTokens_ReturnsTemplateWrapperTokens()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var documentService = CreateDocumentService(workspaceStore);
        var lane = new JazorLaneService(documentService);

        var jazorText = "<template>\n  <div>Hello</div>\n</template>\n\n@code {\n  public string Message { get; set; }\n}";
        var document = new DocumentSnapshot(
            Path.Combine(Path.GetTempPath(), "test-sem-tokens.jazor"),
            DocumentKind.Jazor,
            jazorText,
            "1");

        var tokens = await lane.GetSemanticTokensAsync(document, CancellationToken.None);

        // Should have template wrapper tokens and @code directive token
        Assert.IsTrue(tokens.Count >= 3, $"Expected at least 3 tokens, got {tokens.Count}");

        var templateTokens = tokens.Where(t => t.TokenType == "decorator").ToList();
        Assert.AreEqual(2, templateTokens.Count, "Should have 2 template decorator tokens");

        var keywordTokens = tokens.Where(t => t.TokenType == "keyword").ToList();
        Assert.IsTrue(keywordTokens.Any(k => k.Line > 0), "Should have @code keyword token");
    }

    [TestMethod]
    public async Task JazorLaneService_GetSemanticTokens_ReturnsCodeDirectiveToken()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var documentService = CreateDocumentService(workspaceStore);
        var lane = new JazorLaneService(documentService);

        var jazorText = "@code {\n  public string Name { get; set; }\n}";
        var document = new DocumentSnapshot(
            Path.Combine(Path.GetTempPath(), "test-code-directive.jazor"),
            DocumentKind.Jazor,
            jazorText,
            "1");

        var tokens = await lane.GetSemanticTokensAsync(document, CancellationToken.None);

        var codeKeyword = tokens.FirstOrDefault(t => t.TokenType == "keyword");
        Assert.IsNotNull(codeKeyword, "Should have @code keyword token");
        Assert.AreEqual("@code".Length, codeKeyword!.Length);
    }

    [TestMethod]
    public async Task JazorLaneService_GetSemanticTokens_IgnoresCommentedFakeCodeDirectiveMarkers()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var documentService = CreateDocumentService(workspaceStore);
        var lane = new JazorLaneService(documentService);

        foreach (var (name, jazorText) in new (string Name, string Text)[]
                 {
                     (
                         "line-comment",
                         """
                         // @code {

                         @code {
                           public string Name { get; set; }
                         }
                         """),
                     (
                         "block-comment",
                         """
                         /*
                         @code {
                         */

                         @code {
                           public string Name { get; set; }
                         }
                         """),
                     (
                         "razor-comment",
                         """
                         @*
                         @code {
                         *@

                         @code {
                           public string Name { get; set; }
                         }
                         """)
                 })
        {
            var document = new DocumentSnapshot(
                Path.Combine(Path.GetTempPath(), $"test-commented-code-token-{name}-{Guid.NewGuid():N}.jazor"),
                DocumentKind.Jazor,
                jazorText,
                "1");

            var tokens = await lane.GetSemanticTokensAsync(document, CancellationToken.None);
            var keywordTokens = tokens.Where(static token => token.TokenType == "keyword").ToList();
            var expectedDirectiveIndex = jazorText.LastIndexOf("@code", StringComparison.Ordinal);
            var expectedPosition = LspProtocolHelpers.GetPosition(jazorText, expectedDirectiveIndex);

            Assert.AreEqual(1, keywordTokens.Count, $"{name}: expected exactly one @code keyword token.");
            Assert.AreEqual(expectedPosition.Line, keywordTokens[0].Line, $"{name}: keyword token should map to the real @code line.");
            Assert.AreEqual(expectedPosition.Character, keywordTokens[0].Character, $"{name}: keyword token should map to the real @code column.");
            Assert.AreEqual("@code".Length, keywordTokens[0].Length, $"{name}: keyword token length should stay aligned with @code.");
        }
    }

    [TestMethod]
    public async Task JazorLaneService_GetSemanticTokens_ReturnsCodeDirectiveTokenForEachRealCodeBlock()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var documentService = CreateDocumentService(workspaceStore);
        var lane = new JazorLaneService(documentService);

        var jazorText =
            """
            @code {
              private int First = 1;
            }

            @code {
              private int Second = 2;
            }
            """;
        var document = new DocumentSnapshot(
            Path.Combine(Path.GetTempPath(), $"test-multi-code-tokens-{Guid.NewGuid():N}.jazor"),
            DocumentKind.Jazor,
            jazorText,
            "1");

        var tokens = await lane.GetSemanticTokensAsync(document, CancellationToken.None);
        var keywordTokens = tokens.Where(static token => token.TokenType == "keyword").ToArray();
        var directivePositions = JazorCodeDirectiveLocator.EnumerateCodeDirectives(jazorText)
            .Where(static match => match.HasBlockBody)
            .Select(match => LspProtocolHelpers.GetPosition(jazorText, match.DirectiveIndex))
            .ToArray();

        Assert.AreEqual(2, keywordTokens.Length, "Expected one semantic token per real @code block.");
        CollectionAssert.AreEquivalent(
            directivePositions.Select(static position => $"{position.Line}:{position.Character}").ToArray(),
            keywordTokens.Select(static token => $"{token.Line}:{token.Character}").ToArray());
    }

    [TestMethod]
    public async Task JazorLaneService_GetSemanticTokens_ReturnsComponentTagTokens()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var documentService = CreateDocumentService(workspaceStore);
        var lane = new JazorLaneService(documentService);

        var jazorText = "<template>\n  <MyButton />\n  <Counter />\n</template>";
        var document = new DocumentSnapshot(
            Path.Combine(Path.GetTempPath(), "test-comp-tags.jazor"),
            DocumentKind.Jazor,
            jazorText,
            "1");

        var tokens = await lane.GetSemanticTokensAsync(document, CancellationToken.None);

        var classTokens = tokens.Where(t => t.TokenType == "class").ToList();
        Assert.AreEqual(2, classTokens.Count, "Should have 2 component tag class tokens");

        var names = new HashSet<string>();
        foreach (var token in classTokens)
        {
            var name = jazorText.Split('\n')[token.Line]
                .Substring(token.Character, token.Length);
            names.Add(name);
        }

        Assert.IsTrue(names.Contains("MyButton"), "Should find MyButton component");
        Assert.IsTrue(names.Contains("Counter"), "Should find Counter component");
    }

    [TestMethod]
    public async Task JazorLaneService_GetSemanticTokens_ReturnsEmptyForNonJazor()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var documentService = CreateDocumentService(workspaceStore);
        var lane = new JazorLaneService(documentService);

        var document = new DocumentSnapshot(
            Path.Combine(Path.GetTempPath(), "test.ts"),
            DocumentKind.TypeScript,
            "const x = 1;",
            "1");

        var tokens = await lane.GetSemanticTokensAsync(document, CancellationToken.None);
        Assert.AreEqual(0, tokens.Count);
    }

    [TestMethod]
    public async Task JazorLaneService_GetSemanticTokens_ImportDirectives()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var documentService = CreateDocumentService(workspaceStore);
        var lane = new JazorLaneService(documentService);

        var jazorText = "@module MyComponent from \"./MyComponent.vue\"\n@module { ref } from \"vue\"\n\n<template>\n  <div />\n</template>";
        var document = new DocumentSnapshot(
            Path.Combine(Path.GetTempPath(), "test-imports.jazor"),
            DocumentKind.Jazor,
            jazorText,
            "1");

        var tokens = await lane.GetSemanticTokensAsync(document, CancellationToken.None);

        var importKeywords = tokens.Where(t => t.TokenType == "keyword"
            && (t.Character == 0 || t.Line == 1 && t.Character == 0))
            .ToList();

        Assert.IsTrue(importKeywords.Count >= 2, $"Expected at least 2 import keyword tokens, got {importKeywords.Count}");
    }

    [TestMethod]
    public async Task JazorLaneService_GetSemanticTokens_IgnoresFakeModuleDirectivesInsideCommentsAndCodeBlocks()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var documentService = CreateDocumentService(workspaceStore);
        var lane = new JazorLaneService(documentService);

        var jazorText =
            """
            @*
            @module FakeComment from "./fake-comment.vue"
            *@
            @module RealComponent from "./RealComponent.vue"

            @code {
                @module FakeCode from "./fake-code.ts"
            }
            """;
        var document = new DocumentSnapshot(
            Path.Combine(Path.GetTempPath(), $"test-import-tokens-{Guid.NewGuid():N}.jazor"),
            DocumentKind.Jazor,
            jazorText,
            "1");

        var tokens = await lane.GetSemanticTokensAsync(document, CancellationToken.None);
        var moduleTokens = tokens
            .Where(static token => token.TokenType == "keyword")
            .Where(token =>
            {
                var offset = LspProtocolHelpers.GetOffset(
                    jazorText,
                    new LspPosition { Line = token.Line, Character = token.Character });
                return offset + token.Length <= jazorText.Length
                    && string.Equals(jazorText.Substring(offset, token.Length), "@module", StringComparison.Ordinal);
            })
            .ToArray();
        var expectedPosition = ToPosition(jazorText, "RealComponent", advance: -8);

        Assert.AreEqual(1, moduleTokens.Length);
        Assert.AreEqual(expectedPosition.Line, moduleTokens[0].Line);
        Assert.AreEqual(expectedPosition.Character, moduleTokens[0].Character);
    }

    #endregion

    #region JazorLane Definition

    [TestMethod]
    public async Task JazorLaneService_GetDefinition_ResolvesComponentTag()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var componentName = "TestWidget" + Guid.NewGuid().ToString("N")[..8];
            var vuePath = Path.Combine(tempDirectory, componentName + ".vue");
            await File.WriteAllTextAsync(vuePath, $"<template><div>{componentName}</div></template>");

            var workspaceStore = new InMemoryWorkspaceStore();
            var documentService = CreateDocumentService(workspaceStore);
            var lane = new JazorLaneService(documentService);

            var jazorText = $"<template>\n  <{componentName} />\n</template>";
            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, "test-def.jazor"),
                DocumentKind.Jazor,
                jazorText,
                "1");
            await workspaceStore.UpsertDocumentAsync(document, CancellationToken.None);

            // Position at component tag name
            var position = new LspPosition { Line = 1, Character = 3 };
            var projectionTarget = new ProjectionTarget(
                LaneKind.Jazor,
                DocumentRegionKind.Template,
                document.DocumentPath,
                document.DocumentPath);

            var locations = await lane.GetDefinitionAsync(document, position, projectionTarget, CancellationToken.None);

            Assert.IsTrue(locations.Count > 0, "Should find at least one definition location");
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task JazorLaneService_GetDefinition_ReturnsEmptyForUnknownPosition()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var documentService = CreateDocumentService(workspaceStore);
        var lane = new JazorLaneService(documentService);

        var document = new DocumentSnapshot(
            Path.Combine(Path.GetTempPath(), "test-empty.jazor"),
            DocumentKind.Jazor,
            "<template>\n  <div>Hello</div>\n</template>",
            "1");

        var position = new LspPosition { Line = 0, Character = 0 };
        var projectionTarget = new ProjectionTarget(
            LaneKind.Jazor,
            DocumentRegionKind.Template,
            document.DocumentPath,
            document.DocumentPath);

        var locations = await lane.GetDefinitionAsync(document, position, projectionTarget, CancellationToken.None);
        // div is not a component — should return empty
        Assert.AreEqual(0, locations.Count);
    }

    #endregion

    #region JazorLane References

    [TestMethod]
    public async Task JazorLaneService_GetReferences_ReturnsEmptyForNonComponent()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var documentService = CreateDocumentService(workspaceStore);
        var lane = new JazorLaneService(documentService);

        var document = new DocumentSnapshot(
            Path.Combine(Path.GetTempPath(), "test-refs.jazor"),
            DocumentKind.Jazor,
            "<template>\n  <div>Hello</div>\n</template>",
            "1");

        var position = new LspPosition { Line = 0, Character = 0 };
        var projectionTarget = new ProjectionTarget(
            LaneKind.Jazor,
            DocumentRegionKind.Template,
            document.DocumentPath,
            document.DocumentPath);

        var locations = await lane.GetReferencesAsync(
            document, position, includeDeclaration: true,
            projectionTarget, CancellationToken.None);

        Assert.AreEqual(0, locations.Count);
    }

    #endregion

    #region JazorLane Rename

    [TestMethod]
    public async Task JazorLaneService_GetRename_ReturnsNullForNonComponent()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var documentService = CreateDocumentService(workspaceStore);
        var lane = new JazorLaneService(documentService);

        var document = new DocumentSnapshot(
            Path.Combine(Path.GetTempPath(), "test-rename.jazor"),
            DocumentKind.Jazor,
            "<template>\n  <div>Hello</div>\n</template>",
            "1");

        var position = new LspPosition { Line = 0, Character = 0 };
        var projectionTarget = new ProjectionTarget(
            LaneKind.Jazor,
            DocumentRegionKind.Template,
            document.DocumentPath,
            document.DocumentPath);

        var result = await lane.GetRenameAsync(
            document, position, "NewName",
            projectionTarget, CancellationToken.None);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task JazorLaneService_GetRename_ResolvesComponentTag()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var componentName = "RenameMe" + Guid.NewGuid().ToString("N")[..8];
            var vuePath = Path.Combine(tempDirectory, componentName + ".vue");
            await File.WriteAllTextAsync(vuePath, $"<template><div>{componentName}</div></template>");

            var workspaceStore = new InMemoryWorkspaceStore();
            var documentService = CreateDocumentService(workspaceStore);
            var lane = new JazorLaneService(documentService);

            var jazorText = $"<template>\n  <{componentName} />\n</template>";
            var document = new DocumentSnapshot(
                Path.Combine(tempDirectory, "test-rename-comp.jazor"),
                DocumentKind.Jazor,
                jazorText,
                "1");
            await workspaceStore.UpsertDocumentAsync(document, CancellationToken.None);

            var position = new LspPosition { Line = 1, Character = 3 };
            var projectionTarget = new ProjectionTarget(
                LaneKind.Jazor,
                DocumentRegionKind.Template,
                document.DocumentPath,
                document.DocumentPath);

            var result = await lane.GetRenameAsync(
                document, position, "NewComponent",
                projectionTarget, CancellationToken.None);

            Assert.IsNotNull(result, "Should return rename edit for component tag");
            Assert.IsTrue(result!.Changes.Count > 0, "Should have changes");
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    #endregion

    #region Helpers

    private static JazorLspDocumentService CreateDocumentService(IJoltWorkspaceStore workspaceStore)
    {
        var analysisClient = new StubVueAnalysisClient();
        return new JazorLspDocumentService(workspaceStore, analysisClient);
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "jazor-phase6-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(path);
        File.WriteAllText(
            Path.Combine(path, "TestProject.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);
        File.WriteAllText(
            Path.Combine(path, "TestProject.slnx"),
            """
            <Solution>
              <Project Path="TestProject.csproj" />
            </Solution>
            """);
        return path;
    }

    private static LspPosition ToPosition(string text, string marker, int advance = 0)
    {
        var index = text.IndexOf(marker, StringComparison.Ordinal);
        Assert.IsTrue(index >= 0, $"Expected marker '{marker}' to exist.");

        var target = index + advance;
        var line = 0;
        var character = 0;
        for (var offset = 0; offset < target; offset++)
        {
            if (text[offset] == '\n')
            {
                line++;
                character = 0;
                continue;
            }

            character++;
        }

        return new LspPosition
        {
            Line = line,
            Character = character
        };
    }

    private sealed class CapturingVolarLane : ILspLane
    {
        public LaneKind LaneKind => LaneKind.Volar;

        public ProjectionTarget? LastProjectionTarget { get; private set; }

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
        {
            LastProjectionTarget = projectionTarget;
            return ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(Array.Empty<LspCompletionItem>());
        }

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
            => ValueTask.FromResult(new AnalyzeJazorResponse(
                [], [], [], []));
    }

    #endregion
}
