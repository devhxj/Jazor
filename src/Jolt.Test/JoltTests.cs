using Jolt.Analysis;
using ECMAScript.Internal.VueContracts.Protocol;
using Jolt.Rpc;
using Jolt.Services;
using Jolt.Workspace;
using SharedJoltRpcMethodNames = ECMAScript.Internal.VueContracts.Protocol.JoltRpcMethodNames;

namespace Jolt.Test;

[TestClass]
public sealed class JoltTests
{
    [TestMethod]
    public async Task Jolt_GetVolarContext_ReturnsTrackedFrontendDocuments()
    {
        var host = CreateHost();
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

        var response = await host.GetVolarContextAsync(
            new GetVolarContextRequest(
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
    public async Task Jolt_GetVolarContext_DerivesTrackedDocumentsFromRazorMarkupAndJsImports()
    {
        var host = CreateHost();
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Pages/Counter.jazor",
                DocumentKind.Jazor,
                """
                @module * as userCard from "../Scripts/user-card"

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

        var response = await host.GetVolarContextAsync(
            new GetVolarContextRequest(
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
    public async Task Jolt_GetVolarContext_DerivesTrackedDocumentsFromRazorMarkupAndCoLocatedAssets()
    {
        var host = CreateHost();
        await host.StartAsync(CancellationToken.None);

        using var scopedProject = CreateTemporaryScopedProject();
        var tempDirectory = scopedProject.ProjectRoot;
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

        var response = await host.GetVolarContextAsync(
            new GetVolarContextRequest(
                documentPath,
                Array.Empty<string>()),
            CancellationToken.None);

        Assert.AreEqual(2, response.SemanticContext.RelatedDocuments.Count);
        Assert.IsTrue(response.SemanticContext.RelatedDocuments.Any(document => document.DocumentPath.EndsWith("UserCard.vue", StringComparison.Ordinal)));
        Assert.IsTrue(response.SemanticContext.RelatedDocuments.Any(document => document.DocumentPath.EndsWith("Counter.ts", StringComparison.Ordinal)));
        Assert.AreEqual("2", response.SemanticContext.Properties["derivedDocumentCount"]);
    }

    [TestMethod]
    public async Task Jolt_GetVolarContext_DerivesTrackedWorkspaceVueDocumentOutsideNearbyDirectories()
    {
        var host = CreateHost();
        await host.StartAsync(CancellationToken.None);

        using var scopedProject = CreateTemporaryScopedProject();
        var tempDirectory = scopedProject.ProjectRoot;
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

        var response = await host.GetVolarContextAsync(
            new GetVolarContextRequest(jazorPath, Array.Empty<string>()),
            CancellationToken.None);

        Assert.AreEqual(1, response.SemanticContext.RelatedDocuments.Count);
        Assert.AreEqual(sharedVuePath.Replace('\\', '/'), response.SemanticContext.RelatedDocuments[0].DocumentPath.Replace('\\', '/'));
        Assert.AreEqual("1", response.SemanticContext.Properties["derivedDocumentCount"]);
    }

    [TestMethod]
    public async Task Jolt_GetVolarContext_DerivesDiskBackedWorkspaceVueDocumentOutsideNearbyDirectories()
    {
        var host = CreateHost();
        await host.StartAsync(CancellationToken.None);

        using var scopedProject = CreateTemporaryScopedProject();
        var tempDirectory = scopedProject.ProjectRoot;
        var jazorPath = Path.Combine(tempDirectory, "Pages", "Counter.jazor");
        var sharedVuePath = Path.Combine(tempDirectory, "Shared", "UserBadge.vue");
        Directory.CreateDirectory(Path.GetDirectoryName(jazorPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(sharedVuePath)!);
        await File.WriteAllTextAsync(jazorPath, "<UserBadge />");
        await File.WriteAllTextAsync(sharedVuePath, "<template><div>UserBadge</div></template>");

        var response = await host.GetVolarContextAsync(
            new GetVolarContextRequest(jazorPath, Array.Empty<string>()),
            CancellationToken.None);

        Assert.AreEqual(1, response.SemanticContext.RelatedDocuments.Count);
        Assert.AreEqual(sharedVuePath.Replace('\\', '/'), response.SemanticContext.RelatedDocuments[0].DocumentPath.Replace('\\', '/'));
        Assert.AreEqual("1", response.SemanticContext.Properties["derivedDocumentCount"]);
    }

    [TestMethod]
    public async Task Jolt_GetVolarContext_ProjectWideDiscoveryWithoutSlnx_ThrowsFriendlyEnglishRefusal()
    {
        var host = CreateHost();
        await host.StartAsync(CancellationToken.None);

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var jazorPath = Path.Combine(tempDirectory, "ProjectA", "Features", "Pages", "Home.jazor");
            var componentPath = Path.Combine(tempDirectory, "ProjectA", "Shared", "FancyButton.vue");
            Directory.CreateDirectory(Path.GetDirectoryName(jazorPath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(componentPath)!);
            await File.WriteAllTextAsync(jazorPath, "<FancyButton />");
            await File.WriteAllTextAsync(componentPath, "<template><button /></template>");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await host.GetVolarContextAsync(
                    new GetVolarContextRequest(jazorPath, Array.Empty<string>()),
                    CancellationToken.None));

            StringAssert.Contains(exception.Message, "No solution .slnx was found");
            StringAssert.Contains(exception.Message, "Open the project from a solution directory that contains a .slnx file.");
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
    public async Task Jolt_GetHotUpdatePlan_ReturnsDependentJazorDocumentsForFrontendChange()
    {
        var host = CreateHost();
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
    public async Task Jolt_GetHotUpdatePlan_ReturnsAffectedJazorDocumentsForTrackedWorkspaceVueOutsideNearbyDirectories()
    {
        var host = CreateHost();
        await host.StartAsync(CancellationToken.None);

        using var scopedProject = CreateTemporaryScopedProject();
        var tempDirectory = scopedProject.ProjectRoot;
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

    [TestMethod]
    public async Task Jolt_GetHotUpdatePlan_ReturnsAffectedJazorDocumentsForCssFrontendChange()
    {
        var host = CreateHost();
        await host.StartAsync(CancellationToken.None);

        using var scopedProject = CreateTemporaryScopedProject();
        var tempDirectory = scopedProject.ProjectRoot;
        var jazorPath = Path.Combine(tempDirectory, "Counter.jazor");
        var cssPath = Path.Combine(tempDirectory, "Counter.css");

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                jazorPath,
                DocumentKind.Jazor,
                "<template><div>Counter</div></template>",
                "1"),
            CancellationToken.None);
        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                cssPath,
                DocumentKind.Css,
                "body { color: red; }",
                "1"),
            CancellationToken.None);

        var response = await host.GetHotUpdatePlanAsync(
            new GetHotUpdatePlanRequest(cssPath, DocumentKind.Css, "2"),
            CancellationToken.None);

        Assert.IsFalse(response.RequiresFullReload);
        Assert.AreEqual("frontend-change", response.Reason);
        CollectionAssert.AreEquivalent(
            new[] { jazorPath.Replace('\\', '/') },
            response.AffectedDocumentPaths.Select(static path => path.Replace('\\', '/')).ToArray());
    }

    [TestMethod]
    public async Task Jolt_AnalyzeJazor_DelegatesToAnalysisClient()
    {
        var analysisClient = new RecordingVueAnalysisClient(
            new AnalyzeJazorResponse(
                diagnostics: Array.Empty<DiagnosticRecord>(),
                imports: Array.Empty<ImportDescriptor>(),
                artifacts: Array.Empty<ArtifactRecord>(),
                sourceMaps: Array.Empty<SourceMapDescriptor>()));
        var host = new JoltService(new InMemoryWorkspaceStore(), analysisClient);
        await host.StartAsync(CancellationToken.None);

        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Features/Counter.jazor",
                DocumentKind.Jazor,
                "<template><div /></template>",
                "1"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            volarContext: null);

        var response = await host.AnalyzeJazorAsync(request, CancellationToken.None);

        Assert.IsNotNull(analysisClient.LastRequest);
        Assert.AreEqual("Features/Counter.jazor", analysisClient.LastRequest.JazorDocument.DocumentPath);
        Assert.AreEqual(0, response.Diagnostics.Count);
        Assert.AreEqual(0, response.Imports.Count);
        Assert.AreEqual(0, response.Artifacts.Count);
        Assert.AreEqual(0, response.SourceMaps.Count);
    }

    [TestMethod]
    public async Task Jolt_AnalyzeJazor_PopulatesVolarContextFromTrackedImports()
    {
        var expectedResponse = new AnalyzeJazorResponse(
            diagnostics: Array.Empty<DiagnosticRecord>(),
            imports: Array.Empty<ImportDescriptor>(),
            artifacts: Array.Empty<ArtifactRecord>(),
            sourceMaps: Array.Empty<SourceMapDescriptor>());
        var analysisClient = new RecordingVueAnalysisClient(expectedResponse);
        var host = new JoltService(new InMemoryWorkspaceStore(), analysisClient);
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
                    @module UserCard from "../Components/UserCard.vue"
                    @module * as userCard from "../Scripts/user-card"

                    <template>
                      <UserCard />
                    </template>
                    """,
                    "5"),
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                volarContext: null),
            CancellationToken.None);

        Assert.IsNotNull(analysisClient.LastRequest);
        Assert.AreEqual(2, analysisClient.LastRequest.RelatedDocuments.Count);
        Assert.IsNotNull(analysisClient.LastRequest.VolarContext);
        Assert.AreEqual("frontend", analysisClient.LastRequest.VolarContext.ContextKind);
        Assert.AreEqual("2", analysisClient.LastRequest.VolarContext.Properties["derivedDocumentCount"]);
        Assert.IsTrue(analysisClient.LastRequest.RelatedDocuments.Any(static document => document.DocumentKind == DocumentKind.Vue));
        Assert.IsTrue(analysisClient.LastRequest.RelatedDocuments.Any(static document => document.DocumentKind == DocumentKind.TypeScript));
    }

    [TestMethod]
    public async Task Jolt_AnalyzeJazor_UsesInjectedAnalysisClient()
    {
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Features/Counter.jazor",
                DocumentKind.Jazor,
                "<template><div /></template>",
                "5"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            volarContext: null);
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
        var host = new JoltService(new InMemoryWorkspaceStore(), analysisClient);
        await host.StartAsync(CancellationToken.None);

        var response = await host.AnalyzeJazorAsync(request, CancellationToken.None);

        Assert.IsNotNull(analysisClient.LastRequest);
        Assert.AreEqual(request.JazorDocument.DocumentPath, analysisClient.LastRequest.JazorDocument.DocumentPath);
        Assert.AreEqual(expectedResponse.Diagnostics[0].Id, response.Diagnostics[0].Id);
    }

    [TestMethod]
    public async Task Jolt_AnalyzeJazor_DerivesVolarContextFromImportedTrackedDocuments()
    {
        var expectedResponse = new AnalyzeJazorResponse(
            diagnostics: Array.Empty<DiagnosticRecord>(),
            imports: Array.Empty<ImportDescriptor>(),
            artifacts: Array.Empty<ArtifactRecord>(),
            sourceMaps: Array.Empty<SourceMapDescriptor>());
        var analysisClient = new RecordingVueAnalysisClient(expectedResponse);
        var host = new JoltService(new InMemoryWorkspaceStore(), analysisClient);
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
                    @module UserCard from "./Components/UserCard.vue"
                    @module { count } from "./scripts/counter.ts"

                    <template>
                      <UserCard />
                      <div>{{ count }}</div>
                    </template>
                    """,
                    "1"),
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                volarContext: null),
            CancellationToken.None);

        Assert.IsNotNull(analysisClient.LastRequest);
        Assert.AreEqual(2, analysisClient.LastRequest.RelatedDocuments.Count);
        Assert.IsNotNull(analysisClient.LastRequest.VolarContext);
        Assert.AreEqual("frontend", analysisClient.LastRequest.VolarContext.ContextKind);
        Assert.IsTrue(analysisClient.LastRequest.RelatedDocuments.Any(static document => document.DocumentPath == "Features/Components/UserCard.vue"));
        Assert.IsTrue(analysisClient.LastRequest.RelatedDocuments.Any(static document => document.DocumentPath == "Features/scripts/counter.ts"));
    }

    [TestMethod]
    public async Task Jolt_GetVolarContext_DerivesRelatedDocumentsFromJazorImports()
    {
        var host = CreateHost();
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Counter.jazor",
                DocumentKind.Jazor,
                """
                @module UserCard from "./Components/UserCard.vue"

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

        var response = await host.GetVolarContextAsync(
            new GetVolarContextRequest(
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
    public async Task Jolt_GetVolarContext_EmitsRoslynProjectionArtifactsAndCodeBehindSummary()
    {
        var host = CreateHost();
        await host.StartAsync(CancellationToken.None);

        using var scopedProject = CreateTemporaryScopedProject();
        var tempDirectory = scopedProject.ProjectRoot;
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

        var response = await host.GetVolarContextAsync(
            new GetVolarContextRequest(jazorPath, Array.Empty<string>()),
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

    [TestMethod]
    public async Task Jolt_GetVolarContext_EmitsFrontendSummaryArtifacts()
    {
        var host = CreateHost();
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Counter.jazor",
                DocumentKind.Jazor,
                """
                @module UserCard from "./Components/UserCard.vue"
                @module { counterStore } from "./scripts/counter-store.ts"

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

        var response = await host.GetVolarContextAsync(
            new GetVolarContextRequest(
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
    public async Task Jolt_GetHotUpdatePlan_ReturnsAffectedJazorDocumentsForFrontendChange()
    {
        var host = CreateHost();
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Pages/Counter.jazor",
                DocumentKind.Jazor,
                """
                @module UserCard from "../Components/UserCard.vue"

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
    public async Task Jolt_GetVirtualArtifact_UsesTrackedWorkspaceDocument()
    {
        var host = CreateHost();
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Counter.jazor",
                DocumentKind.Jazor,
                """
                @module dayjs from "dayjs"

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
    public async Task Jolt_GetVirtualArtifact_InfersNearbyVueImportsFromRazorMarkup()
    {
        var host = CreateHost();
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
    public async Task Jolt_GetVirtualArtifact_FallbackCompilerLowersSimpleComputedAndMethodBodies()
    {
        var host = CreateHost();
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
    public async Task Jolt_GetVirtualArtifact_FallbackCompilerLowersCommonControlFlowSubset()
    {
        var host = CreateHost();
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
    public async Task Jolt_GetVirtualArtifact_FallbackCompilerLowersForeachCatchAndErrorThrowing()
    {
        var host = CreateHost();
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
    public async Task Jolt_GetVirtualArtifact_FallbackCompilerRestoresMemberRewritesAfterNestedScope()
    {
        var host = CreateHost();
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
    public async Task Jolt_GetVirtualArtifact_DelegatesResolvedFrontendDocumentsToAnalysisClient()
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
        var host = new JoltService(new InMemoryWorkspaceStore(), analysisClient);
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Features/Pages/Counter.jazor",
                DocumentKind.Jazor,
                """
                @module UserCard from "../Components/UserCard.vue"

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
        Assert.IsNotNull(analysisClient.LastRequest.VolarContext);
    }

    [TestMethod]
    public async Task Jolt_GetVirtualArtifact_PassesDerivedVolarContextToAnalysisClient()
    {
        var analysisClient = new RecordingVueAnalysisClient(
            new AnalyzeJazorResponse(
                diagnostics: Array.Empty<DiagnosticRecord>(),
                imports: Array.Empty<ImportDescriptor>(),
                artifacts: Array.Empty<ArtifactRecord>(),
                sourceMaps: Array.Empty<SourceMapDescriptor>()));
        var host = new JoltService(new InMemoryWorkspaceStore(), analysisClient);
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
                @module UserCard from "./Components/UserCard.vue"

                <template>
                  <UserCard />
                </template>
                """,
                version: "5"),
            CancellationToken.None);

        Assert.IsNotNull(analysisClient.LastRequest);
        Assert.AreEqual(1, analysisClient.LastRequest.RelatedDocuments.Count);
        Assert.IsNotNull(analysisClient.LastRequest.VolarContext);
        Assert.AreEqual("Features/Components/UserCard.vue", analysisClient.LastRequest.RelatedDocuments[0].DocumentPath);
        Assert.AreEqual("vue-sfc", response.Artifact.ArtifactKind);
        StringAssert.Contains(response.Artifact.Content, "<template>");
    }

    [TestMethod]
    public async Task Jolt_GetVirtualArtifact_FallsBackWhenAnalysisClientOmitsRequestedArtifact()
    {
        var analysisClient = new RecordingVueAnalysisClient(
            new AnalyzeJazorResponse(
                diagnostics:
                [
                    new DiagnosticRecord(
                        id: "TEST001",
                        severity: DiagnosticSeverityKind.Warning,
                        message: "Primary client returned diagnostics only.",
                        documentPath: "Features/Counter.jazor",
                        start: 0,
                        length: 0)
                ],
                imports: Array.Empty<ImportDescriptor>(),
                artifacts: Array.Empty<ArtifactRecord>(),
                sourceMaps: Array.Empty<SourceMapDescriptor>()));
        var host = new JoltService(new InMemoryWorkspaceStore(), analysisClient);
        await host.StartAsync(CancellationToken.None);

        var response = await host.GetVirtualArtifactAsync(
            new GetVirtualArtifactRequest(
                documentPath: "Features/Counter.jazor",
                artifactKind: "vue-sfc",
                text:
                """
                <template>
                  <div />
                </template>
                """,
                version: "fallback-artifact-missing"),
            CancellationToken.None);

        Assert.IsNotNull(analysisClient.LastRequest);
        Assert.AreEqual("vue-sfc", response.Artifact.ArtifactKind);
        StringAssert.Contains(response.Artifact.Content, "<template>");
    }

    [TestMethod]
    public async Task Jolt_AnalysisClientFactory_UsesInProcClientWhenTransportMissing()
    {
        var client = VueAnalysisClientFactory.CreateDefault();
        var response = await client.AnalyzeJazorAsync(
            new AnalyzeJazorRequest(
                new DocumentSnapshot(
                    "Features/Counter.jazor",
                    DocumentKind.Jazor,
                    """
                    @module dayjs from "dayjs"

                    <template>
                      <div />
                    </template>
                    """,
                    "factory-default"),
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                volarContext: null),
            CancellationToken.None);

        Assert.IsInstanceOfType<JazorVueAnalysisService>(client);
        Assert.AreEqual(1, response.Imports.Count);
        Assert.AreEqual(2, response.Artifacts.Count);
        Assert.AreEqual("vue-sfc", response.Artifacts[0].ArtifactKind);
    }

    [TestMethod]
    public async Task Jolt_AnalysisClientFactory_CreateUsesInProcClientByDefault()
    {
        var client = VueAnalysisClientFactory.Create(Array.Empty<string>());
        var response = await client.AnalyzeJazorAsync(
            new AnalyzeJazorRequest(
                new DocumentSnapshot(
                    "Features/Counter.jazor",
                    DocumentKind.Jazor,
                    """
                    <template>
                      <div />
                    </template>
                    """,
                    "factory-create-default"),
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                volarContext: null),
            CancellationToken.None);

        Assert.IsInstanceOfType<JazorVueAnalysisService>(client);
        Assert.AreEqual(2, response.Artifacts.Count);
    }

    [TestMethod]
    public async Task Jolt_AnalysisClientFactory_CreateTreatsNullModeAsInProcCompatibilityAlias()
    {
        var client = VueAnalysisClientFactory.Create(["--analysis-client=null"]);
        var response = await client.AnalyzeJazorAsync(
            new AnalyzeJazorRequest(
                new DocumentSnapshot(
                    "Features/Counter.jazor",
                    DocumentKind.Jazor,
                    """
                    <template>
                      <div />
                    </template>
                    """,
                    "factory-create-null-alias"),
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                volarContext: null),
            CancellationToken.None);

        Assert.IsInstanceOfType<JazorVueAnalysisService>(client);
        Assert.AreEqual(2, response.Artifacts.Count);
    }

    [TestMethod]
    public async Task Jolt_AnalysisClientFactory_IgnoresLegacyTransportModeWithoutCommand()
    {
        var client = VueAnalysisClientFactory.Create(["--analysis-client=transport"]);
        var response = await client.AnalyzeJazorAsync(
            new AnalyzeJazorRequest(
                new DocumentSnapshot(
                    "Features/Counter.jazor",
                    DocumentKind.Jazor,
                    """
                    <template>
                      <div />
                    </template>
                    """,
                    "factory-transport-missing-command"),
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                volarContext: null),
            CancellationToken.None);

        Assert.IsInstanceOfType<JazorVueAnalysisService>(client);
        Assert.AreEqual(2, response.Artifacts.Count);
    }

    [TestMethod]
    public void Jolt_AnalysisClientFactory_CreateUsesTransportWhenCommandProvided()
    {
        var client = VueAnalysisClientFactory.Create([
            "--analysis-command=dotnet",
            "--analysis-args=--info"]);

        Assert.IsInstanceOfType<RpcVueAnalysisClient>(client);
    }

    [TestMethod]
    public void Jolt_AnalysisClientFactory_UsesRpcClientWhenTransportProvided()
    {
        var client = VueAnalysisClientFactory.CreateFromTransport(
            new StubVueAnalysisRpcTransport(new RpcResponseEnvelope(
                id: "analysis-factory",
                success: true,
                payloadJson: JoltRpcSerializer.Serialize(new AnalyzeJazorResponse(
                    diagnostics: Array.Empty<DiagnosticRecord>(),
                    imports: Array.Empty<ImportDescriptor>(),
                    artifacts: Array.Empty<ArtifactRecord>(),
                    sourceMaps: Array.Empty<SourceMapDescriptor>())),
                error: null)));

        Assert.IsInstanceOfType<RpcVueAnalysisClient>(client);
    }

    [TestMethod]
    public async Task Jolt_RpcVueAnalysisClient_UsesSharedEnvelopeAndMethodName()
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
                payloadJson: JoltRpcSerializer.Serialize(response),
                error: null));
        var client = new RpcVueAnalysisClient(transport);
        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot("Features/Counter.jazor", DocumentKind.Jazor, "<template/>", "1"),
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            volarContext: null);

        var result = await client.AnalyzeJazorAsync(request, CancellationToken.None);

        Assert.IsNotNull(transport.LastRequest);
        Assert.AreEqual(VueAnalysisRpcMethodNames.AnalyzeJazor, transport.LastRequest.Method);
        Assert.IsFalse(string.IsNullOrWhiteSpace(transport.LastRequest.Id));
        Assert.AreEqual(0, result.Diagnostics.Count);
    }

    [TestMethod]
    public async Task Jolt_RpcVueAnalysisClient_ThrowsOnErrorEnvelope()
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
            volarContext: null);

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
    public async Task Jolt_RpcVueAnalysisClient_CanBridgeToVueAnalysisProcessor()
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
        var host = new JoltService(
            new InMemoryWorkspaceStore(),
            VueAnalysisClientFactory.CreateFromTransport(transport));
        await host.StartAsync(CancellationToken.None);

        var request = new AnalyzeJazorRequest(
            new DocumentSnapshot(
                "Features/Counter.jazor",
                DocumentKind.Jazor,
                """
                @module dayjs from "dayjs"

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
            volarContext: null);

        var response = await host.AnalyzeJazorAsync(request, CancellationToken.None);

        Assert.AreEqual(1, response.Imports.Count);
        Assert.AreEqual("dayjs", response.Imports[0].LocalName);
        Assert.AreEqual(2, response.Artifacts.Count);
        Assert.AreEqual("vue-sfc", response.Artifacts[0].ArtifactKind);
    }

    [TestMethod]
    public async Task Jolt_RpcVueAnalysisClient_InteropsWithVueAnalysisRpcProcessor()
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
        var host = new JoltService(new InMemoryWorkspaceStore(), client);
        await host.StartAsync(CancellationToken.None);

        var response = await host.AnalyzeJazorAsync(
            new AnalyzeJazorRequest(
                new DocumentSnapshot(
                    "Features/Counter.jazor",
                    DocumentKind.Jazor,
                    """
                    @module dayjs from "dayjs"

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
                volarContext: null),
            CancellationToken.None);

        Assert.AreEqual(1, response.Imports.Count);
        Assert.AreEqual("dayjs", response.Imports[0].LocalName);
        Assert.AreEqual(2, response.Artifacts.Count);
        Assert.AreEqual("vue-sfc", response.Artifacts[0].ArtifactKind);
    }

    [TestMethod]
    public async Task Jolt_ProcessAnalysisRpcTransport_InteropsWithJoltAnalysisProcess()
    {
        var analysisHostAssemblyPath = GetBuiltAssemblyPath("Jolt", "Jolt.dll");
        var transport = new ProcessAnalysisRpcTransport(
            "dotnet",
            $"\"{analysisHostAssemblyPath}\" --analysis-stdio");
        var client = VueAnalysisClientFactory.CreateFromTransport(transport);
        var host = new JoltService(new InMemoryWorkspaceStore(), client);
        await host.StartAsync(CancellationToken.None);

        var response = await host.AnalyzeJazorAsync(
            new AnalyzeJazorRequest(
                new DocumentSnapshot(
                    "Features/Counter.jazor",
                    DocumentKind.Jazor,
                    """
                    @module dayjs from "dayjs"

                    <template>
                      <div />
                    </template>
                    """,
                    "8"),
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                volarContext: null),
            CancellationToken.None);

        Assert.AreEqual(1, response.Imports.Count);
        Assert.AreEqual("dayjs", response.Imports[0].LocalName);
        Assert.AreEqual(2, response.Artifacts.Count);
    }

    [TestMethod]
    public async Task Jolt_ProcessAnalysisRpcTransport_InteropsWithJoltAnalysisProcessUsingExplicitCommand()
    {
        var analysisHostAssemblyPath = GetBuiltAssemblyPath("Jolt", "Jolt.dll");

        var transport = new ProcessAnalysisRpcTransport(
            command: "dotnet",
            arguments: $"\"{analysisHostAssemblyPath}\" --analysis-stdio");
        var client = VueAnalysisClientFactory.CreateFromTransport(transport);
        var host = new JoltService(new InMemoryWorkspaceStore(), client);
        await host.StartAsync(CancellationToken.None);

        var response = await host.AnalyzeJazorAsync(
            new AnalyzeJazorRequest(
                new DocumentSnapshot(
                    "Features/Counter.jazor",
                    DocumentKind.Jazor,
                    """
                    @module UserCard from "./UserCard.vue"

                    <template>
                      <UserCard />
                    </template>

                    @code {
                        [Prop] public string Title { get; set; } = "";
                    }
                    """,
                    "8"),
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                volarContext: null),
            CancellationToken.None);

        Assert.AreEqual(1, response.Imports.Count);
        Assert.AreEqual("UserCard", response.Imports[0].LocalName);
        Assert.AreEqual(2, response.Artifacts.Count);
    }

    [TestMethod]
    public async Task Jolt_RpcProcessor_GetOpenDocuments_ReturnsSerializedEnvelope()
    {
        var host = CreateHost();
        await host.StartAsync(CancellationToken.None);

        await host.OpenDocumentAsync(
            new DocumentSnapshot(
                "Components/UserCard.vue",
                DocumentKind.Vue,
                "<template><div /></template>",
                "7"),
            CancellationToken.None);

        var dispatcher = new JoltRpcDispatcher(host);
        var processor = new JoltRpcProcessor(dispatcher);
        var requestJson = JoltRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "req-1",
            method: SharedJoltRpcMethodNames.GetOpenDocuments,
            payloadJson: null));

        var responseJson = await processor.ProcessAsync(requestJson, CancellationToken.None);
        var response = JoltRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);
        var documents = response?.PayloadJson is null
            ? Array.Empty<DocumentSnapshot>()
            : JoltRpcSerializer.Deserialize<DocumentSnapshot[]>(response.PayloadJson);

        Assert.IsNotNull(response);
        Assert.AreEqual("req-1", response.Id);
        Assert.IsTrue(response.Success);
        Assert.IsNull(response.Error);
        Assert.IsNotNull(documents);
        Assert.AreEqual(1, documents.Length);
        Assert.AreEqual("Components/UserCard.vue", documents[0].DocumentPath);
    }

    [TestMethod]
    public async Task Jolt_RpcProcessor_GetVirtualArtifact_ReturnsArtifactPayload()
    {
        var host = CreateHost();
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

        var dispatcher = new JoltRpcDispatcher(host);
        var processor = new JoltRpcProcessor(dispatcher);
        var requestJson = JoltRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "req-artifact",
            method: SharedJoltRpcMethodNames.GetVirtualArtifact,
            payloadJson: JoltRpcSerializer.Serialize(new GetVirtualArtifactRequest(
                documentPath: "Features/Counter.jazor",
                artifactKind: "vue-sfc",
                text: null,
                version: null))));

        var responseJson = await processor.ProcessAsync(requestJson, CancellationToken.None);
        var response = JoltRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);
        var payload = response?.PayloadJson is null
            ? null
            : JoltRpcSerializer.Deserialize<GetVirtualArtifactResponse>(response.PayloadJson);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.IsNotNull(payload);
        Assert.AreEqual("vue-sfc", payload.Artifact.ArtifactKind);
        StringAssert.Contains(payload.Artifact.Content, "<template>");
    }

    [TestMethod]
    public async Task Jolt_RpcProcessor_UnknownMethod_ReturnsErrorEnvelope()
    {
        var host = CreateHost();
        await host.StartAsync(CancellationToken.None);

        var dispatcher = new JoltRpcDispatcher(host);
        var processor = new JoltRpcProcessor(dispatcher);
        var requestJson = JoltRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "req-unknown",
            method: "jolt/unknown",
            payloadJson: null));

        var responseJson = await processor.ProcessAsync(requestJson, CancellationToken.None);
        var response = JoltRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);

        Assert.IsNotNull(response);
        Assert.IsFalse(response.Success);
        Assert.IsNotNull(response.Error);
        Assert.AreEqual("unknown_method", response.Error.Code);
        Assert.AreEqual("req-unknown", response.Id);
        Assert.IsTrue(response.Error.Message.Contains("Unknown Jolt RPC method", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task Jolt_RpcProcessor_GetHostInfo_ReturnsCapabilities()
    {
        var host = CreateHost();
        await host.StartAsync(CancellationToken.None);

        var dispatcher = new JoltRpcDispatcher(host);
        var processor = new JoltRpcProcessor(dispatcher);
        var requestJson = JoltRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "req-host-info",
            method: SharedJoltRpcMethodNames.GetHostInfo,
            payloadJson: null));

        var responseJson = await processor.ProcessAsync(requestJson, CancellationToken.None);
        var response = JoltRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);
        var hostInfo = response?.PayloadJson is null
            ? null
            : JoltRpcSerializer.Deserialize<GetHostInfoResponse>(response.PayloadJson);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.IsNotNull(hostInfo);
        Assert.AreEqual("Jolt", hostInfo.HostName);
        Assert.AreEqual("0.1", hostInfo.ProtocolVersion);
        Assert.IsTrue(hostInfo.Capabilities.Any(static capability => capability.Name == SharedJoltRpcMethodNames.GetHostInfo));
    }

    [TestMethod]
    public async Task Jolt_StdioServer_ProcessesPingRequest()
    {
        var host = CreateHost();
        await host.StartAsync(CancellationToken.None);

        var dispatcher = new JoltRpcDispatcher(host);
        var processor = new JoltRpcProcessor(dispatcher);
        var requestJson = JoltRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "req-ping",
            method: SharedJoltRpcMethodNames.Ping,
            payloadJson: null));

        using var input = new StringReader(requestJson + Environment.NewLine);
        using var output = new StringWriter();
        var server = new StdioJoltRpcServer(processor);

        await server.RunAsync(input, output, CancellationToken.None);

        var responseJson = output.ToString().Trim();
        var response = JoltRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);
        var ping = response?.PayloadJson is null
            ? null
            : JoltRpcSerializer.Deserialize<PingResponse>(response.PayloadJson);

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

    private static JoltService CreateHost()
        => new(new InMemoryWorkspaceStore(), VueAnalysisClientFactory.CreateDefault());

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "JoltTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static JoltIntegrationProjectScope CreateTemporaryScopedProject(string projectName = "JoltTestProject")
        => JoltIntegrationProjectScope.CreateSingleProject(
            scenarioName: nameof(JoltTests),
            solutionName: projectName,
            projectName: projectName);

    private static void DeleteDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                Directory.Delete(path, recursive: true);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                Thread.Sleep(100 * attempt);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                Thread.Sleep(100 * attempt);
            }
        }

        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
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
