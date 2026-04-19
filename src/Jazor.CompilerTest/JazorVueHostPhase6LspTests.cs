using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Analysis;
using Jazor.VueHost.Jazor.Projection;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Aggregation;
using Jazor.VueHost.Lsp.Coordination;
using Jazor.VueHost.Lsp.Lanes;
using Jazor.VueHost.Lsp.Routing;
using Jazor.VueHost.Roslyn.InProc;
using Jazor.VueHost.VirtualDocuments.Mapping;
using Jazor.VueHost.VirtualDocuments.Models;
using Jazor.VueHost.VirtualDocuments.Registry;
using Jazor.VueHost.Workspace;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostPhase6LspTests
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
        var tempDirectory = Path.Combine(Path.GetTempPath(), "roslyn-unopened-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

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
                    VueHostWorkspaceResolver.NormalizePath(document.DocumentPath),
                    VueHostWorkspaceResolver.NormalizePath(declarationPath),
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
                    VueHostWorkspaceResolver.NormalizePath(LspProtocolHelpers.ToDocumentPath(location.Uri)),
                    VueHostWorkspaceResolver.NormalizePath(declarationPath),
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
        var tempDirectory = Path.Combine(Path.GetTempPath(), "roslyn-unopened-forward-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

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

            var consumerPath = VueHostWorkspaceResolver.NormalizePath(consumerDiskPath);
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
                        VueHostWorkspaceResolver.NormalizePath(LspProtocolHelpers.ToDocumentPath(location.Uri)),
                        VueHostWorkspaceResolver.NormalizePath(declarationPath),
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

    private static JazorLspDocumentService CreateDocumentService(IVueHostWorkspaceStore workspaceStore)
    {
        var analysisClient = new StubVueAnalysisClient();
        return new JazorLspDocumentService(workspaceStore, analysisClient);
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "jazor-phase6-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(path);
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
