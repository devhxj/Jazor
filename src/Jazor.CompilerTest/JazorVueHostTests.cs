using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Analysis;
using Jazor.VueHost.Rpc;
using Jazor.VueHost.Services;
using Jazor.VueHost.Workspace;
using SharedVueHostRpcMethodNames = Jazor.VueContracts.Protocol.VueHostRpcMethodNames;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostTests
{
    [TestMethod]
    public async Task JazorVueHost_GetFrontendContext_ReturnsTrackedFrontendDocuments()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var vueDocument = new DocumentSnapshot(
            "Components/UserCard.vue",
            DocumentKind.Vue,
            "<template><div /></template>",
            "1");
        var tsDocument = new DocumentSnapshot(
            "Scripts/user-card.ts",
            DocumentKind.TypeScript,
            "export const x = 1;",
            "1");

        await host.OpenDocumentAsync(vueDocument, CancellationToken.None);
        await host.OpenDocumentAsync(tsDocument, CancellationToken.None);

        var response = await host.GetFrontendContextAsync(
            new GetFrontendContextRequest(
                "Features/Counter.jazor",
                ["Components/UserCard.vue", "Scripts/user-card.ts"]),
            CancellationToken.None);

        Assert.AreEqual("frontend", response.SemanticContext.ContextKind);
        Assert.AreEqual(2, response.SemanticContext.RelatedDocuments.Count);
        Assert.AreEqual("Components/UserCard.vue", response.SemanticContext.RelatedDocuments[0].DocumentPath);
        Assert.AreEqual("2", response.SemanticContext.Properties["relatedDocumentCount"]);
        Assert.IsTrue(response.Artifacts.Any(static artifact => artifact.ArtifactKind == "frontend-summary"));
    }

    [TestMethod]
    public async Task JazorVueHost_GetFrontendContext_DerivesTrackedDocumentsFromRazorMarkupAndJsImports()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Pages/Counter.jazor",
                DocumentKind.Jazor,
                """
                @jsimport * as userCard from "../Scripts/user-card"

                <template>
                  <UserCard />
                </template>
                """,
                "1"),
            CancellationToken.None);
        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Components/UserCard.vue",
                DocumentKind.Vue,
                "<template><div>UserCard</div></template>",
                "1"),
            CancellationToken.None);
        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Scripts/user-card.ts",
                DocumentKind.TypeScript,
                "export const userCard = 1;",
                "1"),
            CancellationToken.None);

        var response = await host.GetFrontendContextAsync(
            new GetFrontendContextRequest(
                "Features/Pages/Counter.jazor",
                Array.Empty<string>()),
            CancellationToken.None);

        Assert.AreEqual(2, response.SemanticContext.RelatedDocuments.Count);
        Assert.IsTrue(response.SemanticContext.RelatedDocuments.Any(static document => document.DocumentPath == "Features/Components/UserCard.vue"));
        Assert.IsTrue(response.SemanticContext.RelatedDocuments.Any(static document => document.DocumentPath == "Features/Scripts/user-card.ts"));
        Assert.AreEqual("0", response.SemanticContext.Properties["explicitDocumentCount"]);
        Assert.AreEqual("2", response.SemanticContext.Properties["derivedDocumentCount"]);
        Assert.AreEqual(5, response.Artifacts.Count);
        Assert.AreEqual("frontend-context", response.Artifacts[0].ArtifactKind);
        Assert.IsTrue(response.Artifacts.Any(static artifact => artifact.ArtifactKind == "razor-projection"));
        Assert.IsTrue(response.Artifacts.Any(static artifact => artifact.ArtifactKind == "razor-projected-csharp"));
        Assert.IsTrue(response.Artifacts.Any(static artifact => artifact.ArtifactKind == "frontend-summary"));
        Assert.IsTrue(response.Artifacts.Skip(1).Any(static artifact => artifact.Content.Contains("documentKind", StringComparison.Ordinal)));
        Assert.IsTrue(response.Artifacts.Skip(1).Any(static artifact => artifact.Content.Contains("userCard", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task JazorVueHost_GetFrontendContext_DerivesTrackedDocumentsFromRazorMarkupAndCoLocatedAssets()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var componentPath = Path.Combine(tempDirectory, "Components", "UserCard.vue");
            Directory.CreateDirectory(Path.GetDirectoryName(componentPath)!);

            await host.OpenDocumentAsync(
                new DocumentSnapshot(
                    documentPath,
                    DocumentKind.Jazor,
                    """
                    <UserCard />
                    """,
                    "1"),
                CancellationToken.None);
            await host.OpenDocumentAsync(
                new DocumentSnapshot(
                    componentPath,
                    DocumentKind.Vue,
                    "<template><div>UserCard</div></template>",
                    "1"),
                CancellationToken.None);
            await host.OpenDocumentAsync(
                new DocumentSnapshot(
                    Path.Combine(tempDirectory, "Counter.ts"),
                    DocumentKind.TypeScript,
                    "export const counterStore = 1;",
                    "1"),
                CancellationToken.None);

            var response = await host.GetFrontendContextAsync(
                new GetFrontendContextRequest(
                    documentPath,
                    Array.Empty<string>()),
                CancellationToken.None);

            Assert.AreEqual(2, response.SemanticContext.RelatedDocuments.Count);
            Assert.IsTrue(response.SemanticContext.RelatedDocuments.Any(document => document.DocumentPath.EndsWith("UserCard.vue", StringComparison.Ordinal)));
            Assert.IsTrue(response.SemanticContext.RelatedDocuments.Any(document => document.DocumentPath.EndsWith("Counter.ts", StringComparison.Ordinal)));
            Assert.AreEqual("2", response.SemanticContext.Properties["derivedDocumentCount"]);
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
    public async Task JazorVueHost_GetFrontendContext_DerivesTrackedWorkspaceVueDocumentOutsideNearbyDirectories()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var jazorPath = Path.Combine(tempDirectory, "Pages", "Counter.jazor");
            var sharedVuePath = Path.Combine(tempDirectory, "Shared", "UserBadge.vue");
            Directory.CreateDirectory(Path.GetDirectoryName(jazorPath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(sharedVuePath)!);

            await host.OpenDocumentAsync(
                new DocumentSnapshot(
                    jazorPath,
                    DocumentKind.Jazor,
                    "<UserBadge />",
                    "1"),
                CancellationToken.None);
            await host.OpenDocumentAsync(
                new DocumentSnapshot(
                    sharedVuePath,
                    DocumentKind.Vue,
                    "<template><div>UserBadge</div></template>",
                    "1"),
                CancellationToken.None);

            var response = await host.GetFrontendContextAsync(
                new GetFrontendContextRequest(jazorPath, Array.Empty<string>()),
                CancellationToken.None);

            Assert.AreEqual(1, response.SemanticContext.RelatedDocuments.Count);
            Assert.AreEqual(sharedVuePath.Replace('\\', '/'), response.SemanticContext.RelatedDocuments[0].DocumentPath.Replace('\\', '/'));
            Assert.AreEqual("1", response.SemanticContext.Properties["derivedDocumentCount"]);
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
    public async Task JazorVueHost_GetFrontendContext_DerivesDiskBackedWorkspaceVueDocumentOutsideNearbyDirectories()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var jazorPath = Path.Combine(tempDirectory, "Pages", "Counter.jazor");
            var sharedVuePath = Path.Combine(tempDirectory, "Shared", "UserBadge.vue");
            Directory.CreateDirectory(Path.GetDirectoryName(jazorPath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(sharedVuePath)!);
            await File.WriteAllTextAsync(jazorPath, "<UserBadge />");
            await File.WriteAllTextAsync(sharedVuePath, "<template><div>UserBadge</div></template>");

            var response = await host.GetFrontendContextAsync(
                new GetFrontendContextRequest(jazorPath, Array.Empty<string>()),
                CancellationToken.None);

            Assert.AreEqual(1, response.SemanticContext.RelatedDocuments.Count);
            Assert.AreEqual(sharedVuePath.Replace('\\', '/'), response.SemanticContext.RelatedDocuments[0].DocumentPath.Replace('\\', '/'));
            Assert.AreEqual("1", response.SemanticContext.Properties["derivedDocumentCount"]);
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
    public async Task JazorVueHost_GetHotUpdatePlan_ReturnsDependentJazorDocumentsForFrontendChange()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Pages/Counter.jazor",
                DocumentKind.Jazor,
                """
                <template>
                  <UserCard />
                </template>
                """,
                "1"),
            CancellationToken.None);
        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Components/UserCard.vue",
                DocumentKind.Vue,
                "<template><UserAvatar /></template><script setup>import UserAvatar from './UserAvatar.vue'</script>",
                "2"),
            CancellationToken.None);

        var response = await host.GetHotUpdatePlanAsync(
            new GetHotUpdatePlanRequest(
                "Features/Components/UserCard.vue",
                DocumentKind.Vue,
                "3"),
            CancellationToken.None);

        Assert.IsFalse(response.RequiresFullReload);
        CollectionAssert.AreEqual(
            new[] { "Features/Pages/Counter.jazor" },
            response.AffectedDocumentPaths.ToArray());
        Assert.AreEqual("frontend-change", response.Reason);
    }

    [TestMethod]
    public async Task JazorVueHost_GetHotUpdatePlan_ReturnsAffectedJazorDocumentsForTrackedWorkspaceVueOutsideNearbyDirectories()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var sharedVuePath = Path.Combine(tempDirectory, "Shared", "UserBadge.vue");
            var counterPath = Path.Combine(tempDirectory, "Pages", "Counter.jazor");
            var dashboardPath = Path.Combine(tempDirectory, "Dashboard", "Index.jazor");
            Directory.CreateDirectory(Path.GetDirectoryName(sharedVuePath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(counterPath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(dashboardPath)!);

            await host.OpenDocumentAsync(
                new DocumentSnapshot(
                    counterPath,
                    DocumentKind.Jazor,
                    "<UserBadge />",
                    "1"),
                CancellationToken.None);
            await host.OpenDocumentAsync(
                new DocumentSnapshot(
                    dashboardPath,
                    DocumentKind.Jazor,
                    "<UserBadge />",
                    "1"),
                CancellationToken.None);
            await host.OpenDocumentAsync(
                new DocumentSnapshot(
                    sharedVuePath,
                    DocumentKind.Vue,
                    "<template><div>UserBadge</div></template>",
                    "1"),
                CancellationToken.None);

            var response = await host.GetHotUpdatePlanAsync(
                new GetHotUpdatePlanRequest(sharedVuePath, DocumentKind.Vue, "2"),
                CancellationToken.None);

            Assert.IsFalse(response.RequiresFullReload);
            Assert.AreEqual("frontend-change", response.Reason);
            CollectionAssert.AreEquivalent(
                new[]
                {
                    counterPath.Replace('\\', '/'),
                    dashboardPath.Replace('\\', '/')
                },
                response.AffectedDocumentPaths.Select(static path => path.Replace('\\', '/')).ToArray());
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
    public async Task JazorVueHost_AnalyzeJazor_DelegatesToAnalysisClient()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Features/Counter.jazor",
                DocumentKind.Jazor,
                "<template><div /></template>",
                "1"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);

        var response = await host.AnalyzeJazorAsync(request, CancellationToken.None);

        Assert.AreEqual(0, response.Diagnostics.Count);
        Assert.AreEqual(0, response.Imports.Count);
        Assert.AreEqual(0, response.Artifacts.Count);
        Assert.AreEqual(0, response.SourceMaps.Count);
    }

    [TestMethod]
    public async Task JazorVueHost_AnalyzeJazor_PopulatesFrontendContextFromTrackedImports()
    {
        var expectedResponse = new AnalyzeJazorResponse(
            diagnostics: Array.Empty<DiagnosticRecord>(),
            imports: Array.Empty<ImportDescriptor>(),
            artifacts: Array.Empty<ArtifactRecord>(),
            sourceMaps: Array.Empty<SourceMapDescriptor>());
        var analysisClient = new RecordingVueAnalysisClient(expectedResponse);
        var host = new VueHostService(new InMemoryWorkspaceStore(), analysisClient);
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Components/UserCard.vue",
                DocumentKind.Vue,
                "<template><div>UserCard</div></template>",
                "1"),
            CancellationToken.None);
        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Scripts/user-card.ts",
                DocumentKind.TypeScript,
                "export const userCard = 1;",
                "1"),
            CancellationToken.None);

        await host.AnalyzeJazorAsync(
            new AnalyzeJazorRequest(
                new DocumentSnapshot(
                    "Features/Pages/Counter.jazor",
                    DocumentKind.Jazor,
                    """
                    @vueimport UserCard from "../Components/UserCard.vue"
                    @jsimport * as userCard from "../Scripts/user-card"

                    <template>
                      <UserCard />
                    </template>
                    """,
                    "5"),
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                frontendContext: null),
            CancellationToken.None);

        Assert.IsNotNull(analysisClient.LastRequest);
        Assert.AreEqual(2, analysisClient.LastRequest.RelatedDocuments.Count);
        Assert.IsNotNull(analysisClient.LastRequest.FrontendContext);
        Assert.AreEqual("frontend", analysisClient.LastRequest.FrontendContext.ContextKind);
        Assert.AreEqual("2", analysisClient.LastRequest.FrontendContext.Properties["derivedDocumentCount"]);
        Assert.IsTrue(analysisClient.LastRequest.RelatedDocuments.Any(static document => document.DocumentKind == DocumentKind.Vue));
        Assert.IsTrue(analysisClient.LastRequest.RelatedDocuments.Any(static document => document.DocumentKind == DocumentKind.TypeScript));
    }

    [TestMethod]
    public async Task JazorVueHost_AnalyzeJazor_UsesInjectedAnalysisClient()
    {
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Features/Counter.jazor",
                DocumentKind.Jazor,
                "<template><div /></template>",
                "5"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);
        var expectedResponse = new AnalyzeJazorResponse(
            diagnostics:
            [
                new DiagnosticRecord(
                    id: "JZ1001",
                    severity: DiagnosticSeverityKind.Warning,
                    message: "test",
                    documentPath: "Features/Counter.jazor",
                    start: 3,
                    length: 2)
            ],
            imports: Array.Empty<ImportDescriptor>(),
            artifacts: Array.Empty<ArtifactRecord>(),
            sourceMaps: Array.Empty<SourceMapDescriptor>());
        var analysisClient = new RecordingVueAnalysisClient(expectedResponse);
        var host = new VueHostService(new InMemoryWorkspaceStore(), analysisClient);
        await host.StartAsync(CancellationToken.None);

        var response = await host.AnalyzeJazorAsync(request, CancellationToken.None);

        Assert.IsNotNull(analysisClient.LastRequest);
        Assert.AreEqual(request.JazorDocument.DocumentPath, analysisClient.LastRequest.JazorDocument.DocumentPath);
        Assert.AreEqual(expectedResponse.Diagnostics[0].Id, response.Diagnostics[0].Id);
    }

    [TestMethod]
    public async Task JazorVueHost_AnalyzeJazor_DerivesFrontendContextFromImportedTrackedDocuments()
    {
        var expectedResponse = new AnalyzeJazorResponse(
            diagnostics: Array.Empty<DiagnosticRecord>(),
            imports: Array.Empty<ImportDescriptor>(),
            artifacts: Array.Empty<ArtifactRecord>(),
            sourceMaps: Array.Empty<SourceMapDescriptor>());
        var analysisClient = new RecordingVueAnalysisClient(expectedResponse);
        var host = new VueHostService(new InMemoryWorkspaceStore(), analysisClient);
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Components/UserCard.vue",
                DocumentKind.Vue,
                "<template><div>UserCard</div></template>",
                "2"),
            CancellationToken.None);
        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/scripts/counter.ts",
                DocumentKind.TypeScript,
                "export const count = 1;",
                "3"),
            CancellationToken.None);

        await host.AnalyzeJazorAsync(
            new AnalyzeJazorRequest(
                new DocumentSnapshot(
                    "Features/Counter.jazor",
                    DocumentKind.Jazor,
                    """
                    @vueimport UserCard from "./Components/UserCard.vue"
                    @jsimport { count } from "./scripts/counter.ts"

                    <template>
                      <UserCard />
                      <div>{{ count }}</div>
                    </template>
                    """,
                    "1"),
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                frontendContext: null),
            CancellationToken.None);

        Assert.IsNotNull(analysisClient.LastRequest);
        Assert.AreEqual(2, analysisClient.LastRequest.RelatedDocuments.Count);
        Assert.IsNotNull(analysisClient.LastRequest.FrontendContext);
        Assert.AreEqual("frontend", analysisClient.LastRequest.FrontendContext.ContextKind);
        Assert.IsTrue(analysisClient.LastRequest.RelatedDocuments.Any(static document => document.DocumentPath == "Features/Components/UserCard.vue"));
        Assert.IsTrue(analysisClient.LastRequest.RelatedDocuments.Any(static document => document.DocumentPath == "Features/scripts/counter.ts"));
    }

    [TestMethod]
    public async Task JazorVueHost_GetFrontendContext_DerivesRelatedDocumentsFromJazorImports()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Counter.jazor",
                DocumentKind.Jazor,
                """
                @vueimport UserCard from "./Components/UserCard.vue"

                <template>
                  <UserCard />
                </template>
                """,
                "1"),
            CancellationToken.None);
        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Components/UserCard.vue",
                DocumentKind.Vue,
                "<template><div>UserCard</div></template>",
                "2"),
            CancellationToken.None);

        var response = await host.GetFrontendContextAsync(
            new GetFrontendContextRequest(
                "Features/Counter.jazor",
                relatedDocumentPaths: Array.Empty<string>()),
            CancellationToken.None);

        Assert.AreEqual(1, response.SemanticContext.RelatedDocuments.Count);
        Assert.AreEqual("Features/Components/UserCard.vue", response.SemanticContext.RelatedDocuments[0].DocumentPath);
        Assert.AreEqual(4, response.Artifacts.Count);
        Assert.AreEqual("frontend-context", response.Artifacts[0].ArtifactKind);
        Assert.IsTrue(response.Artifacts.Any(static artifact => artifact.ArtifactKind == "razor-projection"));
        Assert.IsTrue(response.Artifacts.Any(static artifact => artifact.ArtifactKind == "razor-projected-csharp"));
        Assert.IsTrue(response.Artifacts.Any(static artifact => artifact.ArtifactKind == "frontend-summary"));
    }

    [TestMethod]
    public async Task JazorVueHost_GetFrontendContext_EmitsRoslynProjectionArtifactsAndCodeBehindSummary()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var jazorPath = Path.Combine(tempDirectory, "Counter.jazor");
            var codeBehindPath = Path.Combine(tempDirectory, "Counter.jazor.cs");

            await host.OpenDocumentAsync(
                new DocumentSnapshot(
                    jazorPath,
                    DocumentKind.Jazor,
                    """
                    <template>
                      <button>@Count</button>
                    </template>

                    @code {
                        [Prop] public int Count { get; set; }
                    }
                    """,
                    "1"),
                CancellationToken.None);
            await host.OpenDocumentAsync(
                new DocumentSnapshot(
                    codeBehindPath,
                    DocumentKind.CSharp,
                    """
                    public partial class Counter
                    {
                        [State] private int count = 1;
                    }
                    """,
                    "2"),
                CancellationToken.None);

            var response = await host.GetFrontendContextAsync(
                new GetFrontendContextRequest(jazorPath, Array.Empty<string>()),
                CancellationToken.None);

            CollectionAssert.Contains(
                new[] { "razor-design-time", "fallback" },
                response.SemanticContext.Properties["projectionKind"]);
            Assert.AreEqual("2", response.SemanticContext.Properties["roslynSourceDocumentCount"]);
            Assert.AreEqual("1", response.SemanticContext.Properties["codeBehindDocumentCount"]);

            var projectionArtifact = response.Artifacts.Single(static artifact => artifact.ArtifactKind == "razor-projection");
            StringAssert.Contains(projectionArtifact.Content, "Counter.jazor.cs");
            StringAssert.Contains(projectionArtifact.Content, "projectionKind");

            var projectedCSharpArtifact = response.Artifacts.Single(static artifact => artifact.ArtifactKind == "razor-projected-csharp");
            StringAssert.Contains(projectedCSharpArtifact.Content, "class");
            StringAssert.Contains(projectedCSharpArtifact.Content, "Counter");
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
    public async Task JazorVueHost_GetFrontendContext_EmitsFrontendSummaryArtifacts()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Counter.jazor",
                DocumentKind.Jazor,
                """
                @vueimport UserCard from "./Components/UserCard.vue"
                @jsimport { counterStore } from "./scripts/counter-store.ts"

                <template>
                  <UserCard />
                </template>
                """,
                "1"),
            CancellationToken.None);
        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Components/UserCard.vue",
                DocumentKind.Vue,
                """
                <template>
                  <CardShell />
                </template>
                <script setup>
                export const userCard = true;
                </script>
                """,
                "2"),
            CancellationToken.None);
        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/scripts/counter-store.ts",
                DocumentKind.TypeScript,
                """
                import { ref } from "vue";
                export const counterStore = ref(0);
                """,
                "3"),
            CancellationToken.None);

        var response = await host.GetFrontendContextAsync(
            new GetFrontendContextRequest(
                "Features/Counter.jazor",
                Array.Empty<string>()),
            CancellationToken.None);

        var summaries = response.Artifacts
            .Where(static artifact => artifact.ArtifactKind == "frontend-summary")
            .ToArray();

        Assert.AreEqual(2, summaries.Length);
        Assert.IsTrue(summaries.Any(static artifact => artifact.Content.Contains("counterStore", StringComparison.Ordinal)));
        Assert.IsTrue(summaries.Any(static artifact => artifact.Content.Contains("CardShell", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task JazorVueHost_GetHotUpdatePlan_ReturnsAffectedJazorDocumentsForFrontendChange()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Pages/Counter.jazor",
                DocumentKind.Jazor,
                """
                @vueimport UserCard from "../Components/UserCard.vue"

                <template>
                  <UserCard />
                </template>
                """,
                "1"),
            CancellationToken.None);
        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Components/UserCard.vue",
                DocumentKind.Vue,
                "<template><div>UserCard</div></template>",
                "2"),
            CancellationToken.None);

        var response = await host.GetHotUpdatePlanAsync(
            new GetHotUpdatePlanRequest(
                "Features/Components/UserCard.vue",
                DocumentKind.Vue,
                "3"),
            CancellationToken.None);

        Assert.IsFalse(response.RequiresFullReload);
        Assert.AreEqual("frontend-change", response.Reason);
        CollectionAssert.AreEquivalent(
            new[] { "Features/Pages/Counter.jazor" },
            response.AffectedDocumentPaths.ToArray());
    }

    [TestMethod]
    public async Task JazorVueHost_GetVirtualArtifact_UsesTrackedWorkspaceDocument()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Counter.jazor",
                DocumentKind.Jazor,
                """
                @jsimport dayjs from "dayjs"

                <template>
                  <div>{{ dayjs }}</div>
                </template>
                """,
                "12"),
            CancellationToken.None);

        var response = await host.GetVirtualArtifactAsync(
            new GetVirtualArtifactRequest(
                documentPath: "Features/Counter.jazor",
                artifactKind: "vue-sfc",
                text: null,
                version: null),
            CancellationToken.None);

        Assert.AreEqual("vue-sfc", response.Artifact.ArtifactKind);
        StringAssert.Contains(response.Artifact.Content, "<script setup>");
        StringAssert.Contains(response.Artifact.Content, "import dayjs from \"dayjs\";");
        Assert.AreEqual(2, response.SourceMaps.Count);
    }

    [TestMethod]
    public async Task JazorVueHost_GetVirtualArtifact_InfersNearbyVueImportsFromRazorMarkup()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var componentPath = Path.Combine(tempDirectory, "Components", "UserCard.vue");
            Directory.CreateDirectory(Path.GetDirectoryName(componentPath)!);
            await File.WriteAllTextAsync(
                componentPath,
                "<template><div>UserCard</div></template>");

            var response = await host.GetVirtualArtifactAsync(
                new GetVirtualArtifactRequest(
                    documentPath: documentPath,
                    artifactKind: "vue-sfc",
                    text:
                    """
                    <UserCard />

                    @code {
                        [Prop] public string Title { get; set; } = "";
                    }
                    """,
                    version: "1"),
                CancellationToken.None);

            Assert.AreEqual("vue-sfc", response.Artifact.ArtifactKind);
            StringAssert.Contains(response.Artifact.Content, "import UserCard from \"./Components/UserCard.vue\";");
            StringAssert.Contains(response.Artifact.Content, "<UserCard />");
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
    public async Task JazorVueHost_GetVirtualArtifact_FallbackCompilerLowersSimpleComputedAndMethodBodies()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var response = await host.GetVirtualArtifactAsync(
            new GetVirtualArtifactRequest(
                documentPath: "Features/Counter.jazor",
                artifactKind: "vue-sfc",
                text:
                """
                <template>
                  <button @click="increment(step)">{{ label }}</button>
                </template>

                @code {
                    [Prop] public int Step { get; set; } = 2;
                    [State] private int count = 1;
                    [Computed] public string Label => $"Count: {count + Step}";

                    public string Increment(int delta)
                    {
                        var next = count + delta + Step;
                        count = next;
                        return Label;
                    }
                }
                """,
                version: "13"),
            CancellationToken.None);

        StringAssert.Contains(response.Artifact.Content, "import { computed, ref, toRef } from \"vue\";");
        StringAssert.Contains(response.Artifact.Content, "const step = toRef(props, \"step\");");
        StringAssert.Contains(response.Artifact.Content, "const count = ref(1);");
        StringAssert.Contains(response.Artifact.Content, "const label = computed(() => `Count: ${count.value + step.value}`);");
        StringAssert.Contains(response.Artifact.Content, "function increment(delta) {");
        StringAssert.Contains(response.Artifact.Content, "let next = count.value + delta + step.value;");
        StringAssert.Contains(response.Artifact.Content, "count.value = next;");
        StringAssert.Contains(response.Artifact.Content, "return label.value;");
        Assert.IsFalse(response.Artifact.Content.Contains("Fallback compiler could not lower", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task JazorVueHost_GetVirtualArtifact_FallbackCompilerLowersCommonControlFlowSubset()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var response = await host.GetVirtualArtifactAsync(
            new GetVirtualArtifactRequest(
                documentPath: "Features/Counter.jazor",
                artifactKind: "vue-sfc",
                text:
                """
                <template>
                  <button @click="refreshAsync(card)">{{ title }}</button>
                </template>

                @code {
                    [Prop] public int Step { get; set; } = 3;
                    [Prop] public string? Title { get; set; }
                    [State] private int count = 0;

                    public async Task LogAsync(string value)
                    {
                        await Task.CompletedTask;
                    }

                    public async Task RefreshAsync(CardModel card)
                    {
                        for (int i = 0; i < Step; i++)
                        {
                            if (i == 1)
                            {
                                continue;
                            }

                            count += i;
                        }

                        while (count < Step)
                        {
                            count++;
                            if (count > 10)
                            {
                                break;
                            }
                        }

                        if (card.Title != null)
                        {
                            await LogAsync(card.Title);
                        }
                        else if (Title != null)
                        {
                            await LogAsync(Title);
                            return;
                        }
                    }
                }
                """,
                version: "14"),
            CancellationToken.None);

        StringAssert.Contains(response.Artifact.Content, "async function refreshAsync(card) {");
        StringAssert.Contains(response.Artifact.Content, "for (let i = 0; i < step.value; i++)");
        StringAssert.Contains(response.Artifact.Content, "continue;");
        StringAssert.Contains(response.Artifact.Content, "while (count.value < step.value)");
        StringAssert.Contains(response.Artifact.Content, "break;");
        StringAssert.Contains(response.Artifact.Content, "if (card.Title != null)");
        StringAssert.Contains(response.Artifact.Content, "await logAsync(card.Title);");
        StringAssert.Contains(response.Artifact.Content, "else if (title.value != null)");
        StringAssert.Contains(response.Artifact.Content, "await logAsync(title.value);");
        StringAssert.Contains(response.Artifact.Content, "return;");
        Assert.IsFalse(response.Artifact.Content.Contains("card.title.value", StringComparison.Ordinal));
        Assert.IsFalse(response.Artifact.Content.Contains("Fallback compiler could not lower", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task JazorVueHost_GetVirtualArtifact_FallbackCompilerLowersForeachCatchAndErrorThrowing()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var response = await host.GetVirtualArtifactAsync(
            new GetVirtualArtifactRequest(
                documentPath: "Features/Counter.jazor",
                artifactKind: "vue-sfc",
                text:
                """
                <template>
                  <button @click="refreshAsync()">{{ title }}</button>
                </template>

                @code {
                    [Prop] public IEnumerable<int> Numbers { get; set; } = Array.Empty<int>();
                    [Prop] public string? Title { get; set; }
                    [State] private int count = 0;

                    public async Task LogAsync(string value)
                    {
                        await Task.CompletedTask;
                    }

                    public void ThrowBoom()
                    {
                        throw new InvalidOperationException(Title ?? "boom");
                    }

                    public async Task RefreshAsync()
                    {
                        foreach (var number in Numbers)
                        {
                            count += number;
                        }

                        try
                        {
                            ThrowBoom();
                        }
                        catch (InvalidOperationException ex)
                        {
                            await LogAsync(ex.Message);
                        }
                        finally
                        {
                            count++;
                        }
                    }
                }
                """,
                version: "15"),
            CancellationToken.None);

        StringAssert.Contains(response.Artifact.Content, "const numbers = toRef(props, \"numbers\");");
        StringAssert.Contains(response.Artifact.Content, "function throwBoom() {");
        StringAssert.Contains(response.Artifact.Content, "throw new Error(title.value ?? \"boom\");");
        StringAssert.Contains(response.Artifact.Content, "async function refreshAsync() {");
        StringAssert.Contains(response.Artifact.Content, "for (const number of numbers.value)");
        StringAssert.Contains(response.Artifact.Content, "catch (ex)");
        StringAssert.Contains(response.Artifact.Content, "await logAsync(ex.Message);");
        StringAssert.Contains(response.Artifact.Content, "finally");
        Assert.IsFalse(response.Artifact.Content.Contains("Fallback compiler could not lower", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task JazorVueHost_GetVirtualArtifact_FallbackCompilerRestoresMemberRewritesAfterNestedScope()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var response = await host.GetVirtualArtifactAsync(
            new GetVirtualArtifactRequest(
                documentPath: "Features/Counter.jazor",
                artifactKind: "vue-sfc",
                text:
                """
                <template>
                  <button @click="refreshAsync()">{{ title }}</button>
                </template>

                @code {
                    [Prop] public string? Title { get; set; }

                    public async Task LogAsync(string value)
                    {
                        await Task.CompletedTask;
                    }

                    public async Task RefreshAsync()
                    {
                        if (Title != null)
                        {
                            string Title = "local";
                            await LogAsync(Title);
                        }

                        await LogAsync(Title ?? "fallback");
                    }
                }
                """,
                version: "16"),
            CancellationToken.None);

        StringAssert.Contains(response.Artifact.Content, "let Title = \"local\";");
        StringAssert.Contains(response.Artifact.Content, "await logAsync(Title);");
        StringAssert.Contains(response.Artifact.Content, "await logAsync(title.value ?? \"fallback\");");
        Assert.IsFalse(response.Artifact.Content.Contains("await logAsync(Title ?? \"fallback\")", StringComparison.Ordinal));
        Assert.IsFalse(response.Artifact.Content.Contains("Fallback compiler could not lower", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task JazorVueHost_GetVirtualArtifact_DelegatesResolvedFrontendDocumentsToAnalysisClient()
    {
        var expectedResponse = new AnalyzeJazorResponse(
            diagnostics: Array.Empty<DiagnosticRecord>(),
            imports: Array.Empty<ImportDescriptor>(),
            artifacts:
            [
                new ArtifactRecord(
                    artifactName: "virtual:Features/Pages/Counter.jazor.vue",
                    artifactKind: "vue-sfc",
                    content: "<template><UserCard /></template>",
                    contentHash: null)
            ],
            sourceMaps: Array.Empty<SourceMapDescriptor>());
        var analysisClient = new RecordingVueAnalysisClient(expectedResponse);
        var host = new VueHostService(new InMemoryWorkspaceStore(), analysisClient);
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Pages/Counter.jazor",
                DocumentKind.Jazor,
                """
                @vueimport UserCard from "../Components/UserCard.vue"

                <template>
                  <UserCard />
                </template>
                """,
                "12"),
            CancellationToken.None);
        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Components/UserCard.vue",
                DocumentKind.Vue,
                "<template><div>UserCard</div></template>",
                "1"),
            CancellationToken.None);

        var response = await host.GetVirtualArtifactAsync(
            new GetVirtualArtifactRequest(
                documentPath: "Features/Pages/Counter.jazor",
                artifactKind: "vue-sfc",
                text: null,
                version: null),
            CancellationToken.None);

        Assert.AreEqual("vue-sfc", response.Artifact.ArtifactKind);
        Assert.IsNotNull(analysisClient.LastRequest);
        Assert.AreEqual(1, analysisClient.LastRequest.RelatedDocuments.Count);
        Assert.AreEqual("Features/Components/UserCard.vue", analysisClient.LastRequest.RelatedDocuments[0].DocumentPath);
        Assert.IsNotNull(analysisClient.LastRequest.FrontendContext);
    }

    [TestMethod]
    public async Task JazorVueHost_GetVirtualArtifact_PassesDerivedFrontendContextToAnalysisClient()
    {
        var analysisClient = new RecordingVueAnalysisClient(
            new AnalyzeJazorResponse(
                diagnostics: Array.Empty<DiagnosticRecord>(),
                imports: Array.Empty<ImportDescriptor>(),
                artifacts: Array.Empty<ArtifactRecord>(),
                sourceMaps: Array.Empty<SourceMapDescriptor>()));
        var host = new VueHostService(new InMemoryWorkspaceStore(), analysisClient);
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Components/UserCard.vue",
                DocumentKind.Vue,
                "<template><div>UserCard</div></template>",
                "4"),
            CancellationToken.None);

        var response = await host.GetVirtualArtifactAsync(
            new GetVirtualArtifactRequest(
                documentPath: "Features/Counter.jazor",
                artifactKind: "vue-sfc",
                text:
                """
                @vueimport UserCard from "./Components/UserCard.vue"

                <template>
                  <UserCard />
                </template>
                """,
                version: "5"),
            CancellationToken.None);

        Assert.IsNotNull(analysisClient.LastRequest);
        Assert.AreEqual(1, analysisClient.LastRequest.RelatedDocuments.Count);
        Assert.IsNotNull(analysisClient.LastRequest.FrontendContext);
        Assert.AreEqual("Features/Components/UserCard.vue", analysisClient.LastRequest.RelatedDocuments[0].DocumentPath);
        Assert.AreEqual("vue-sfc", response.Artifact.ArtifactKind);
        StringAssert.Contains(response.Artifact.Content, "<template>");
    }

    [TestMethod]
    public void JazorVueHost_AnalysisClientFactory_UsesNullFallbackWhenTransportMissing()
    {
        var client = VueAnalysisClientFactory.CreateDefault();

        Assert.IsInstanceOfType<NullVueAnalysisClient>(client);
    }

    [TestMethod]
    public void JazorVueHost_AnalysisClientFactory_ParseRecognizesTransportMode()
    {
        var options = VueAnalysisClientFactory.Parse(
        [
            "--analysis-client=transport",
            "--analysis-command=dotnet",
            "--analysis-args=run --project src/Jazor.VueHost -- --analysis-stdio"
        ]);

        Assert.AreEqual(VueAnalysisClientMode.Transport, options.Mode);
        Assert.AreEqual("dotnet", options.Command);
        Assert.AreEqual("run --project src/Jazor.VueHost -- --analysis-stdio", options.Arguments);
    }

    [TestMethod]
    public void JazorVueHost_AnalysisClientFactory_FallsBackToNullWhenTransportModeHasNoCommand()
    {
        var client = VueAnalysisClientFactory.Create(
            new VueAnalysisClientOptions(
                VueAnalysisClientMode.Transport,
                command: null,
                arguments: null));

        Assert.IsInstanceOfType<NullVueAnalysisClient>(client);
    }

    [TestMethod]
    public void JazorVueHost_AnalysisClientFactory_UsesRpcClientWhenTransportProvided()
    {
        var client = VueAnalysisClientFactory.CreateFromTransport(
            new StubVueAnalysisRpcTransport(new RpcResponseEnvelope(
                id: "analysis-factory",
                success: true,
                payloadJson: VueHostRpcSerializer.Serialize(new AnalyzeJazorResponse(
                    diagnostics: Array.Empty<DiagnosticRecord>(),
                    imports: Array.Empty<ImportDescriptor>(),
                    artifacts: Array.Empty<ArtifactRecord>(),
                    sourceMaps: Array.Empty<SourceMapDescriptor>())),
                error: null)));

        Assert.IsInstanceOfType<RpcVueAnalysisClient>(client);
    }

    [TestMethod]
    public async Task JazorVueHost_RpcVueAnalysisClient_UsesSharedEnvelopeAndMethodName()
    {
        var response = new AnalyzeJazorResponse(
            diagnostics: Array.Empty<DiagnosticRecord>(),
            imports: Array.Empty<ImportDescriptor>(),
            artifacts: Array.Empty<ArtifactRecord>(),
            sourceMaps: Array.Empty<SourceMapDescriptor>());
        var transport = new StubVueAnalysisRpcTransport(
            new RpcResponseEnvelope(
                id: "analysis-1",
                success: true,
                payloadJson: VueHostRpcSerializer.Serialize(response),
                error: null));
        var client = new RpcVueAnalysisClient(transport);
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot("Features/Counter.jazor", DocumentKind.Jazor, "<template/>", "1"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);

        var result = await client.AnalyzeJazorAsync(request, CancellationToken.None);

        Assert.IsNotNull(transport.LastRequest);
        Assert.AreEqual(VueAnalysisRpcMethodNames.AnalyzeJazor, transport.LastRequest.Method);
        Assert.IsFalse(string.IsNullOrWhiteSpace(transport.LastRequest.Id));
        Assert.AreEqual(0, result.Diagnostics.Count);
    }

    [TestMethod]
    public async Task JazorVueHost_RpcVueAnalysisClient_ThrowsOnErrorEnvelope()
    {
        var transport = new StubVueAnalysisRpcTransport(
            new RpcResponseEnvelope(
                id: "analysis-err",
                success: false,
                payloadJson: null,
                error: new RpcErrorRecord("analysis_failure", "boom", null)));
        var client = new RpcVueAnalysisClient(transport);
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot("Features/Counter.jazor", DocumentKind.Jazor, "<template/>", "1"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);

        InvalidOperationException? exception = null;
        try
        {
            await client.AnalyzeJazorAsync(request, CancellationToken.None);
        }
        catch (InvalidOperationException caught)
        {
            exception = caught;
        }

        Assert.IsNotNull(exception);
        Assert.IsTrue(exception.Message.Contains("analysis_failure", StringComparison.Ordinal));
        Assert.IsTrue(exception.Message.Contains("boom", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task JazorVueHost_RpcVueAnalysisClient_CanBridgeToVueAnalysisProcessor()
    {
        var analysisProcessor = new VueAnalysisRpcProcessor(new JazorVueAnalysisService());
        var transport = new DelegateAnalysisRpcTransport(async (request, cancellationToken) =>
        {
            var responseJson = await analysisProcessor.ProcessAsync(
                VueAnalysisRpcSerializer.Serialize(request),
                cancellationToken);

            return VueAnalysisRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson)
                ?? throw new InvalidOperationException("Expected a valid analysis RPC response envelope.");
        });
        var host = new VueHostService(
            new InMemoryWorkspaceStore(),
            VueAnalysisClientFactory.CreateFromTransport(transport));
        await host.StartAsync(CancellationToken.None);

        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Features/Counter.jazor",
                DocumentKind.Jazor,
                """
                @jsimport dayjs from "dayjs"

                <template>
                  <div />
                </template>

                @code {
                    public void Tick()
                    {
                    }
                }
                """,
                "1"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null);

        var response = await host.AnalyzeJazorAsync(request, CancellationToken.None);

        Assert.AreEqual(1, response.Imports.Count);
        Assert.AreEqual("dayjs", response.Imports[0].LocalName);
        Assert.AreEqual(2, response.Artifacts.Count);
        Assert.AreEqual("vue-sfc", response.Artifacts[0].ArtifactKind);
    }

    [TestMethod]
    public async Task JazorVueHost_RpcVueAnalysisClient_InteropsWithVueAnalysisRpcProcessor()
    {
        var analysisProcessor = new VueAnalysisRpcProcessor(new JazorVueAnalysisService());
        var transport = new DelegateAnalysisRpcTransport(async (request, cancellationToken) =>
        {
            var requestJson = VueAnalysisRpcSerializer.Serialize(request);
            var responseJson = await analysisProcessor.ProcessAsync(requestJson, cancellationToken);
            return VueAnalysisRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson)
                ?? throw new InvalidOperationException("Failed to deserialize VueAnalysis RPC response.");
        });
        var client = VueAnalysisClientFactory.CreateFromTransport(transport);
        var host = new VueHostService(new InMemoryWorkspaceStore(), client);
        await host.StartAsync(CancellationToken.None);

        var response = await host.AnalyzeJazorAsync(
            new AnalyzeJazorRequest(
                new DocumentSnapshot(
                    "Features/Counter.jazor",
                    DocumentKind.Jazor,
                    """
                    @jsimport dayjs from "dayjs"

                    <template>
                      <div />
                    </template>

                    @code {
                        public void Tick()
                        {
                        }
                    }
                    """,
                    "6"),
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                frontendContext: null),
            CancellationToken.None);

        Assert.AreEqual(1, response.Imports.Count);
        Assert.AreEqual("dayjs", response.Imports[0].LocalName);
        Assert.AreEqual(2, response.Artifacts.Count);
        Assert.AreEqual("vue-sfc", response.Artifacts[0].ArtifactKind);
    }

    [TestMethod]
    public async Task JazorVueHost_ProcessAnalysisRpcTransport_InteropsWithVueHostAnalysisProcess()
    {
        var analysisHostAssemblyPath = GetBuiltAssemblyPath("Jazor.VueHost", "Jazor.VueHost.dll");
        var transport = new ProcessAnalysisRpcTransport(
            "dotnet",
            $"\"{analysisHostAssemblyPath}\" --analysis-stdio");
        var client = VueAnalysisClientFactory.CreateFromTransport(transport);
        var host = new VueHostService(new InMemoryWorkspaceStore(), client);
        await host.StartAsync(CancellationToken.None);

        var response = await host.AnalyzeJazorAsync(
            new AnalyzeJazorRequest(
                new DocumentSnapshot(
                    "Features/Counter.jazor",
                    DocumentKind.Jazor,
                    """
                    @jsimport dayjs from "dayjs"

                    <template>
                      <div />
                    </template>
                    """,
                    "8"),
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                frontendContext: null),
            CancellationToken.None);

        Assert.AreEqual(1, response.Imports.Count);
        Assert.AreEqual("dayjs", response.Imports[0].LocalName);
        Assert.AreEqual(2, response.Artifacts.Count);
    }

    [TestMethod]
    public async Task JazorVueHost_ProcessAnalysisRpcTransport_InteropsWithVueHostAnalysisProcessUsingExplicitCommand()
    {
        var analysisHostAssemblyPath = GetBuiltAssemblyPath("Jazor.VueHost", "Jazor.VueHost.dll");

        var transport = new ProcessAnalysisRpcTransport(
            command: "dotnet",
            arguments: $"\"{analysisHostAssemblyPath}\" --analysis-stdio");
        var client = VueAnalysisClientFactory.CreateFromTransport(transport);
        var host = new VueHostService(new InMemoryWorkspaceStore(), client);
        await host.StartAsync(CancellationToken.None);

        var response = await host.AnalyzeJazorAsync(
            new AnalyzeJazorRequest(
                new DocumentSnapshot(
                    "Features/Counter.jazor",
                    DocumentKind.Jazor,
                    """
                    @vueimport UserCard from "./UserCard.vue"

                    <template>
                      <UserCard />
                    </template>

                    @code {
                        [Prop] public string Title { get; set; } = "";
                    }
                    """,
                    "8"),
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                frontendContext: null),
            CancellationToken.None);

        Assert.AreEqual(1, response.Imports.Count);
        Assert.AreEqual("UserCard", response.Imports[0].LocalName);
        Assert.AreEqual(2, response.Artifacts.Count);
    }

    [TestMethod]
    public async Task JazorVueHost_RpcProcessor_GetOpenDocuments_ReturnsSerializedEnvelope()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Components/UserCard.vue",
                DocumentKind.Vue,
                "<template><div /></template>",
                "7"),
            CancellationToken.None);

        var dispatcher = new VueHostRpcDispatcher(host);
        var processor = new VueHostRpcProcessor(dispatcher);
        var requestJson = VueHostRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "req-1",
            method: SharedVueHostRpcMethodNames.GetOpenDocuments,
            payloadJson: null));

        var responseJson = await processor.ProcessAsync(requestJson, CancellationToken.None);
        var response = VueHostRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);
        var documents = response?.PayloadJson is null
            ? Array.Empty<DocumentSnapshot>()
            : VueHostRpcSerializer.Deserialize<DocumentSnapshot[]>(response.PayloadJson);

        Assert.IsNotNull(response);
        Assert.AreEqual("req-1", response.Id);
        Assert.IsTrue(response.Success);
        Assert.IsNull(response.Error);
        Assert.IsNotNull(documents);
        Assert.AreEqual(1, documents.Length);
        Assert.AreEqual("Components/UserCard.vue", documents[0].DocumentPath);
    }

    [TestMethod]
    public async Task JazorVueHost_RpcProcessor_GetVirtualArtifact_ReturnsArtifactPayload()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Counter.jazor",
                DocumentKind.Jazor,
                """
                <template>
                  <div />
                </template>
                """,
                "13"),
            CancellationToken.None);

        var dispatcher = new VueHostRpcDispatcher(host);
        var processor = new VueHostRpcProcessor(dispatcher);
        var requestJson = VueHostRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "req-artifact",
            method: SharedVueHostRpcMethodNames.GetVirtualArtifact,
            payloadJson: VueHostRpcSerializer.Serialize(new GetVirtualArtifactRequest(
                documentPath: "Features/Counter.jazor",
                artifactKind: "vue-sfc",
                text: null,
                version: null))));

        var responseJson = await processor.ProcessAsync(requestJson, CancellationToken.None);
        var response = VueHostRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);
        var payload = response?.PayloadJson is null
            ? null
            : VueHostRpcSerializer.Deserialize<GetVirtualArtifactResponse>(response.PayloadJson);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.IsNotNull(payload);
        Assert.AreEqual("vue-sfc", payload.Artifact.ArtifactKind);
        StringAssert.Contains(payload.Artifact.Content, "<template>");
    }

    [TestMethod]
    public async Task JazorVueHost_RpcProcessor_UnknownMethod_ReturnsErrorEnvelope()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var dispatcher = new VueHostRpcDispatcher(host);
        var processor = new VueHostRpcProcessor(dispatcher);
        var requestJson = VueHostRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "req-unknown",
            method: "vuehost/unknown",
            payloadJson: null));

        var responseJson = await processor.ProcessAsync(requestJson, CancellationToken.None);
        var response = VueHostRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);

        Assert.IsNotNull(response);
        Assert.IsFalse(response.Success);
        Assert.IsNotNull(response.Error);
        Assert.AreEqual("unknown_method", response.Error.Code);
        Assert.AreEqual("req-unknown", response.Id);
        Assert.IsTrue(response.Error.Message.Contains("Unknown Jazor.VueHost RPC method", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task JazorVueHost_RpcProcessor_GetHostInfo_ReturnsCapabilities()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var dispatcher = new VueHostRpcDispatcher(host);
        var processor = new VueHostRpcProcessor(dispatcher);
        var requestJson = VueHostRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "req-host-info",
            method: SharedVueHostRpcMethodNames.GetHostInfo,
            payloadJson: null));

        var responseJson = await processor.ProcessAsync(requestJson, CancellationToken.None);
        var response = VueHostRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);
        var hostInfo = response?.PayloadJson is null
            ? null
            : VueHostRpcSerializer.Deserialize<GetHostInfoResponse>(response.PayloadJson);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.IsNotNull(hostInfo);
        Assert.AreEqual("Jazor.VueHost", hostInfo.HostName);
        Assert.AreEqual("0.1", hostInfo.ProtocolVersion);
        Assert.IsTrue(hostInfo.Capabilities.Any(static capability => capability.Name == SharedVueHostRpcMethodNames.GetHostInfo));
    }

    [TestMethod]
    public async Task JazorVueHost_StdioServer_ProcessesPingRequest()
    {
        var host = new VueHostService(new InMemoryWorkspaceStore(), new NullVueAnalysisClient());
        await host.StartAsync(CancellationToken.None);

        var dispatcher = new VueHostRpcDispatcher(host);
        var processor = new VueHostRpcProcessor(dispatcher);
        var requestJson = VueHostRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "req-ping",
            method: SharedVueHostRpcMethodNames.Ping,
            payloadJson: null));

        using var input = new StringReader(requestJson + Environment.NewLine);
        using var output = new StringWriter();
        var server = new StdioVueHostRpcServer(processor);

        await server.RunAsync(input, output, CancellationToken.None);

        var responseJson = output.ToString().Trim();
        var response = VueHostRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);
        var ping = response?.PayloadJson is null
            ? null
            : VueHostRpcSerializer.Deserialize<PingResponse>(response.PayloadJson);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.AreEqual("req-ping", response.Id);
        Assert.IsNotNull(ping);
        Assert.AreEqual("pong", ping.Message);
        Assert.AreEqual("0.1", ping.ProtocolVersion);
    }

    private sealed class RecordingVueAnalysisClient : IVueAnalysisClient
    {
        private readonly AnalyzeJazorResponse _response;

        public RecordingVueAnalysisClient(AnalyzeJazorResponse response)
        {
            _response = response;
        }

        public AnalyzeJazorRequest? LastRequest { get; private set; }

        public ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(AnalyzeJazorRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return ValueTask.FromResult(_response);
        }
    }

    private sealed class StubVueAnalysisRpcTransport : IAnalysisRpcTransport
    {
        private readonly RpcResponseEnvelope _responseEnvelope;

        public StubVueAnalysisRpcTransport(RpcResponseEnvelope responseEnvelope)
        {
            _responseEnvelope = responseEnvelope;
        }

        public RpcRequestEnvelope? LastRequest { get; private set; }

        public ValueTask<RpcResponseEnvelope> SendAsync(
            RpcRequestEnvelope request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return ValueTask.FromResult(_responseEnvelope);
        }
    }

    private static string GetRepositoryRoot()
        => Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            ".."));

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "JazorVueHostTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string GetBuiltAssemblyPath(string projectDirectoryName, string assemblyFileName)
    {
        var assemblyPath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            projectDirectoryName,
            "bin",
            "Debug",
            "net10.0",
            assemblyFileName);
        Assert.IsTrue(File.Exists(assemblyPath), $"Expected built assembly '{assemblyPath}' to exist.");
        return assemblyPath;
    }
}
