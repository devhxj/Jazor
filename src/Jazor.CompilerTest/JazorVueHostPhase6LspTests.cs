using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Analysis;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Aggregation;
using Jazor.VueHost.Lsp.Coordination;
using Jazor.VueHost.Lsp.Lanes;
using Jazor.VueHost.Lsp.Routing;
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

        var jazorText = "@vueimport MyComponent from \"./MyComponent.vue\"\n@jsimport { ref } from \"vue\"\n\n<template>\n  <div />\n</template>";
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

    private sealed class StubVueAnalysisClient : IVueAnalysisClient
    {
        public ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
            AnalyzeJazorRequest request,
            CancellationToken cancellationToken)
            => ValueTask.FromResult(new AnalyzeJazorResponse(
                [], [], [], null));
    }

    #endregion
}
