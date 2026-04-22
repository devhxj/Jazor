using Jazor.VueContracts.Protocol;
using Jolt.Lsp;
using Jolt.Lsp.Coordination;
using Jolt.Workspace;

namespace Jolt.Test;

[TestClass]
public sealed class JoltMarkupComponentBridgeTests
{
    [TestMethod]
    public async Task MarkupComponentBridgeService_FindJazorReferencesAsync_IncludesDeclarationAndMarkupOnlyUsages()
    {
        using var topology = JoltIntegrationTestTopology.Create(nameof(MarkupComponentBridgeService_FindJazorReferencesAsync_IncludesDeclarationAndMarkupOnlyUsages));
        var owningProject = topology.CreateSingleProjectSolution("MarkupComponentBridge", "MarkupComponentBridge");
        var declarationPath = owningProject.WriteFile("UserBadge.vue", "<template><div>UserBadge</div></template>");

        var hostDocument = new DocumentSnapshot(
            owningProject.GetPath("Host.vue"),
            DocumentKind.Vue,
            """
            <template>
              <UserBadge />
            </template>
            """,
            "1");
        var counterDocument = new DocumentSnapshot(
            owningProject.GetPath("Counter.jazor"),
            DocumentKind.Jazor,
            """
            <UserBadge />

            @code {
                private void UserBadge()
                {
                }
            }
            """,
            "1");
        var dashboardPath = owningProject.WriteFile(
            "Dashboard.jazor",
            """
            <section>
              <UserBadge />
            </section>

            @code {
                private string UserBadge => nameof(UserBadge);
            }
            """);

        var workspaceStore = new InMemoryWorkspaceStore();
        await workspaceStore.UpsertDocumentAsync(hostDocument, CancellationToken.None);
        await workspaceStore.UpsertDocumentAsync(counterDocument, CancellationToken.None);
        var service = new MarkupComponentBridgeService(workspaceStore);

        var references = await service.FindJazorReferencesAsync(
            hostDocument,
            "UserBadge",
            declarationPath,
            includeDeclaration: true,
            CancellationToken.None);

        var declarationUri = LspProtocolHelpers.ToDocumentUri(declarationPath);
        var hostUri = LspProtocolHelpers.ToDocumentUri(hostDocument.DocumentPath);
        var counterUri = LspProtocolHelpers.ToDocumentUri(counterDocument.DocumentPath);
        var dashboardUri = LspProtocolHelpers.ToDocumentUri(dashboardPath);

        Assert.IsTrue(references.Any(location => location.Uri == declarationUri));
        Assert.IsTrue(references.Any(location => location.Uri == hostUri));
        Assert.IsTrue(references.Any(location => location.Uri == counterUri));
        Assert.IsTrue(references.Any(location => location.Uri == dashboardUri));
        Assert.IsFalse(references.Any(location =>
            location.Uri == counterUri
            && location.Range.Start.Line >= 3));
        Assert.IsFalse(references.Any(location =>
            location.Uri == dashboardUri
            && location.Range.Start.Line >= 5));
    }

    [TestMethod]
    public async Task MarkupComponentBridgeService_FindJazorRenameChangesAsync_RenamesMarkupOnlyAcrossWorkspace()
    {
        using var topology = JoltIntegrationTestTopology.Create(nameof(MarkupComponentBridgeService_FindJazorRenameChangesAsync_RenamesMarkupOnlyAcrossWorkspace));
        var owningProject = topology.CreateSingleProjectSolution("MarkupComponentBridge", "MarkupComponentBridge");
        var declarationPath = owningProject.WriteFile("UserBadge.vue", "<template><div>UserBadge</div></template>");

        var hostDocument = new DocumentSnapshot(
            owningProject.GetPath("Host.vue"),
            DocumentKind.Vue,
            """
            <template>
              <UserBadge />
            </template>
            """,
            "1");
        var counterDocument = new DocumentSnapshot(
            owningProject.GetPath("Counter.jazor"),
            DocumentKind.Jazor,
            """
            <UserBadge />

            @code {
                private void UserBadge()
                {
                }
            }
            """,
            "1");
        var dashboardPath = owningProject.WriteFile(
            "Dashboard.jazor",
            """
            <section>
              <UserBadge />
            </section>

            @code {
                private string UserBadge => nameof(UserBadge);
            }
            """);

        var workspaceStore = new InMemoryWorkspaceStore();
        await workspaceStore.UpsertDocumentAsync(hostDocument, CancellationToken.None);
        await workspaceStore.UpsertDocumentAsync(counterDocument, CancellationToken.None);
        var service = new MarkupComponentBridgeService(workspaceStore);

        var changes = await service.FindJazorRenameChangesAsync(
            hostDocument,
            "UserBadge",
            declarationPath,
            "ProfileBadge",
            CancellationToken.None);

        var hostUri = LspProtocolHelpers.ToDocumentUri(hostDocument.DocumentPath);
        var counterUri = LspProtocolHelpers.ToDocumentUri(counterDocument.DocumentPath);
        var dashboardUri = LspProtocolHelpers.ToDocumentUri(dashboardPath);

        Assert.IsTrue(changes.ContainsKey(hostUri));
        Assert.IsTrue(changes.ContainsKey(counterUri));
        Assert.IsTrue(changes.ContainsKey(dashboardUri));
        Assert.IsTrue(changes.Values.SelectMany(static edits => edits).All(static edit => edit.NewText == "ProfileBadge"));
        Assert.IsFalse(changes[counterUri].Any(edit => edit.Range.Start.Line >= 3));
        Assert.IsFalse(changes[dashboardUri].Any(edit => edit.Range.Start.Line >= 5));
    }

    [TestMethod]
    public async Task MarkupComponentBridgeService_ResolveBridgeSymbolAsync_UsesNativeVueLocationForScriptImport()
    {
        using var topology = JoltIntegrationTestTopology.Create(nameof(MarkupComponentBridgeService_ResolveBridgeSymbolAsync_UsesNativeVueLocationForScriptImport));
        var owningProject = topology.CreateSingleProjectSolution("MarkupComponentBridge", "MarkupComponentBridge");
        var declarationPath = owningProject.WriteFile("UserBadge.vue", "<template><div>UserBadge</div></template>");

        var scriptDocument = new DocumentSnapshot(
            owningProject.GetPath("consumer.ts"),
            DocumentKind.TypeScript,
            """
            import UserBadge from "./UserBadge.vue";
            export const current = UserBadge;
            """,
            "1");
        var workspaceStore = new InMemoryWorkspaceStore();
        await workspaceStore.UpsertDocumentAsync(scriptDocument, CancellationToken.None);
        var service = new MarkupComponentBridgeService(workspaceStore);

        var resolved = await service.ResolveBridgeSymbolAsync(
            scriptDocument,
            new LspPosition { Line = 0, Character = 8 },
            [
                new LspLocation
                {
                    Uri = LspProtocolHelpers.ToDocumentUri(declarationPath),
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 0 },
                        End = new LspPosition { Line = 0, Character = 0 }
                    }
                }
            ],
            allowWorkspaceScan: true,
            CancellationToken.None);

        Assert.IsTrue(resolved.HasValue);
        Assert.AreEqual("UserBadge", resolved.Value.ComponentName);
        Assert.AreEqual(JoltWorkspaceResolver.NormalizePath(declarationPath), resolved.Value.AbsolutePath);
    }

    [TestMethod]
    public async Task MarkupComponentBridgeService_ResolveBridgeSymbolAsync_IgnoresFakeImportInLineComment()
    {
        using var topology = JoltIntegrationTestTopology.Create(nameof(MarkupComponentBridgeService_ResolveBridgeSymbolAsync_IgnoresFakeImportInLineComment));
        var owningProject = topology.CreateSingleProjectSolution("MarkupComponentBridge", "MarkupComponentBridge");
        owningProject.WriteFile("UserBadge.vue", "<template><div>UserBadge</div></template>");

        var scriptDocument = new DocumentSnapshot(
            owningProject.GetPath("consumer.ts"),
            DocumentKind.TypeScript,
            """
            // import UserBadge from "./UserBadge.vue";
            export const current = UserBadge;
            """,
            "1");
        var workspaceStore = new InMemoryWorkspaceStore();
        await workspaceStore.UpsertDocumentAsync(scriptDocument, CancellationToken.None);
        var service = new MarkupComponentBridgeService(workspaceStore);

        var resolved = await service.ResolveBridgeSymbolAsync(
            scriptDocument,
            FindPosition(scriptDocument.Text, "UserBadge", useLastOccurrence: true),
            locationHints: null,
            allowWorkspaceScan: true,
            CancellationToken.None);

        Assert.IsFalse(resolved.HasValue);
    }

    [TestMethod]
    public async Task MarkupComponentBridgeService_ResolveBridgeSymbolAsync_IgnoresFakeImportInBlockComment()
    {
        using var topology = JoltIntegrationTestTopology.Create(nameof(MarkupComponentBridgeService_ResolveBridgeSymbolAsync_IgnoresFakeImportInBlockComment));
        var owningProject = topology.CreateSingleProjectSolution("MarkupComponentBridge", "MarkupComponentBridge");
        owningProject.WriteFile("UserBadge.vue", "<template><div>UserBadge</div></template>");

        var scriptDocument = new DocumentSnapshot(
            owningProject.GetPath("consumer.ts"),
            DocumentKind.TypeScript,
            """
            /*
             import UserBadge from "./UserBadge.vue";
            */
            export const current = UserBadge;
            """,
            "1");
        var workspaceStore = new InMemoryWorkspaceStore();
        await workspaceStore.UpsertDocumentAsync(scriptDocument, CancellationToken.None);
        var service = new MarkupComponentBridgeService(workspaceStore);

        var resolved = await service.ResolveBridgeSymbolAsync(
            scriptDocument,
            FindPosition(scriptDocument.Text, "UserBadge", useLastOccurrence: true),
            locationHints: null,
            allowWorkspaceScan: true,
            CancellationToken.None);

        Assert.IsFalse(resolved.HasValue);
    }

    [TestMethod]
    public async Task MarkupComponentBridgeService_ResolveBridgeSymbolAsync_IgnoresFakeImportInTemplateString()
    {
        using var topology = JoltIntegrationTestTopology.Create(nameof(MarkupComponentBridgeService_ResolveBridgeSymbolAsync_IgnoresFakeImportInTemplateString));
        var owningProject = topology.CreateSingleProjectSolution("MarkupComponentBridge", "MarkupComponentBridge");
        owningProject.WriteFile("UserBadge.vue", "<template><div>UserBadge</div></template>");

        var scriptDocument = new DocumentSnapshot(
            owningProject.GetPath("consumer.ts"),
            DocumentKind.TypeScript,
            """
            const sample = `
            import UserBadge from "./UserBadge.vue";
            `;
            export const current = UserBadge;
            """,
            "1");
        var workspaceStore = new InMemoryWorkspaceStore();
        await workspaceStore.UpsertDocumentAsync(scriptDocument, CancellationToken.None);
        var service = new MarkupComponentBridgeService(workspaceStore);

        var resolved = await service.ResolveBridgeSymbolAsync(
            scriptDocument,
            FindPosition(scriptDocument.Text, "UserBadge", useLastOccurrence: true),
            locationHints: null,
            allowWorkspaceScan: true,
            CancellationToken.None);

        Assert.IsFalse(resolved.HasValue);
    }

    [TestMethod]
    public async Task MarkupComponentBridgeService_ResolveBridgeSymbolAsync_PrefersRealImportOverCommentText()
    {
        using var topology = JoltIntegrationTestTopology.Create(nameof(MarkupComponentBridgeService_ResolveBridgeSymbolAsync_PrefersRealImportOverCommentText));
        var owningProject = topology.CreateSingleProjectSolution("MarkupComponentBridge", "MarkupComponentBridge");
        var declarationPath = owningProject.WriteFile("UserBadge.vue", "<template><div>UserBadge</div></template>");

        var scriptDocument = new DocumentSnapshot(
            owningProject.GetPath("consumer.ts"),
            DocumentKind.TypeScript,
            """
            // import FakeBadge from "./FakeBadge.vue";
            import UserBadge from "./UserBadge.vue";
            export const current = UserBadge;
            """,
            "1");
        var workspaceStore = new InMemoryWorkspaceStore();
        await workspaceStore.UpsertDocumentAsync(scriptDocument, CancellationToken.None);
        var service = new MarkupComponentBridgeService(workspaceStore);

        var resolved = await service.ResolveBridgeSymbolAsync(
            scriptDocument,
            FindPosition(scriptDocument.Text, "UserBadge", useLastOccurrence: true),
            locationHints: null,
            allowWorkspaceScan: true,
            CancellationToken.None);

        Assert.IsTrue(resolved.HasValue);
        Assert.AreEqual("UserBadge", resolved.Value.ComponentName);
        Assert.AreEqual(JoltWorkspaceResolver.NormalizePath(declarationPath), resolved.Value.AbsolutePath);
    }

    [TestMethod]
    public async Task MarkupComponentBridgeService_FindJazorReferencesAndRenameChanges_IncludeOpenUnsavedAndDiskJazorForTypeScriptImport()
    {
        using var topology = JoltIntegrationTestTopology.Create(nameof(MarkupComponentBridgeService_FindJazorReferencesAndRenameChanges_IncludeOpenUnsavedAndDiskJazorForTypeScriptImport));
        var owningProject = topology.CreateSingleProjectSolution("MarkupComponentBridge", "MarkupComponentBridge");
        var declarationPath = owningProject.WriteFile("UserBadge.vue", "<template><div>UserBadge</div></template>");

        var scriptDocument = new DocumentSnapshot(
            owningProject.GetPath("consumer.ts"),
            DocumentKind.TypeScript,
            """
            import UserBadge from "./UserBadge.vue";
            export const current = UserBadge;
            """,
            "1");
        var counterDocument = new DocumentSnapshot(
            owningProject.GetPath("Counter.jazor"),
            DocumentKind.Jazor,
            """
            <UserBadge />

            @code {
                private void UserBadge()
                {
                }
            }
            """,
            "1");
        var dashboardPath = owningProject.WriteFile(
            "Dashboard.jazor",
            """
            <section>
              <UserBadge />
            </section>

            @code {
                private string UserBadge => nameof(UserBadge);
            }
            """);

        var workspaceStore = new InMemoryWorkspaceStore();
        await workspaceStore.UpsertDocumentAsync(scriptDocument, CancellationToken.None);
        await workspaceStore.UpsertDocumentAsync(counterDocument, CancellationToken.None);
        var service = new MarkupComponentBridgeService(workspaceStore);
        var resolved = await service.ResolveBridgeSymbolAsync(
            scriptDocument,
            new LspPosition { Line = 0, Character = 8 },
            locationHints: null,
            allowWorkspaceScan: true,
            CancellationToken.None);

        Assert.IsTrue(resolved.HasValue);

        var references = await service.FindJazorReferencesAsync(
            scriptDocument,
            resolved.Value.ComponentName,
            resolved.Value.AbsolutePath,
            includeDeclaration: true,
            CancellationToken.None);

        var changes = await service.FindJazorRenameChangesAsync(
            scriptDocument,
            resolved.Value.ComponentName,
            resolved.Value.AbsolutePath,
            "ProfileBadge",
            CancellationToken.None);

        var declarationUri = LspProtocolHelpers.ToDocumentUri(declarationPath);
        var counterUri = LspProtocolHelpers.ToDocumentUri(counterDocument.DocumentPath);
        var dashboardUri = LspProtocolHelpers.ToDocumentUri(dashboardPath);

        Assert.IsTrue(references.Any(location => location.Uri == declarationUri));
        Assert.IsTrue(references.Any(location => location.Uri == counterUri));
        Assert.IsTrue(references.Any(location => location.Uri == dashboardUri));
        Assert.IsFalse(references.Any(location =>
            location.Uri == counterUri
            && location.Range.Start.Line >= 3));
        Assert.IsFalse(references.Any(location =>
            location.Uri == dashboardUri
            && location.Range.Start.Line >= 5));

        Assert.IsTrue(changes.ContainsKey(counterUri));
        Assert.IsTrue(changes.ContainsKey(dashboardUri));
        Assert.IsFalse(changes[counterUri].Any(edit => edit.Range.Start.Line >= 3));
        Assert.IsFalse(changes[dashboardUri].Any(edit => edit.Range.Start.Line >= 5));
        Assert.IsTrue(changes[counterUri].All(static edit => edit.NewText == "ProfileBadge"));
        Assert.IsTrue(changes[dashboardUri].All(static edit => edit.NewText == "ProfileBadge"));
    }

    private static LspPosition FindPosition(string text, string marker, bool useLastOccurrence = false)
    {
        var offset = useLastOccurrence
            ? text.LastIndexOf(marker, StringComparison.Ordinal)
            : text.IndexOf(marker, StringComparison.Ordinal);
        Assert.IsTrue(offset >= 0, $"Marker '{marker}' was not found.");
        return LspProtocolHelpers.GetPosition(text, offset);
    }
}
