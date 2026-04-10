using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Coordination;
using Jazor.VueHost.Workspace;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostMarkupComponentBridgeTests
{
    [TestMethod]
    public async Task MarkupComponentBridgeService_FindJazorReferencesAsync_IncludesDeclarationAndMarkupOnlyUsages()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var declarationPath = Path.Combine(tempDirectory, "UserBadge.vue");
            await File.WriteAllTextAsync(declarationPath, "<template><div>UserBadge</div></template>");

            var hostDocument = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Host.vue"),
                DocumentKind.Vue,
                """
                <template>
                  <UserBadge />
                </template>
                """,
                "1");
            var counterDocument = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Counter.jazor"),
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
            var dashboardPath = Path.Combine(tempDirectory, "Dashboard.jazor");
            await File.WriteAllTextAsync(
                dashboardPath,
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
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task MarkupComponentBridgeService_FindJazorRenameChangesAsync_RenamesMarkupOnlyAcrossWorkspace()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var declarationPath = Path.Combine(tempDirectory, "UserBadge.vue");
            await File.WriteAllTextAsync(declarationPath, "<template><div>UserBadge</div></template>");

            var hostDocument = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Host.vue"),
                DocumentKind.Vue,
                """
                <template>
                  <UserBadge />
                </template>
                """,
                "1");
            var counterDocument = new DocumentSnapshot(
                Path.Combine(tempDirectory, "Counter.jazor"),
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
            var dashboardPath = Path.Combine(tempDirectory, "Dashboard.jazor");
            await File.WriteAllTextAsync(
                dashboardPath,
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
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task MarkupComponentBridgeService_ResolveBridgeSymbolAsync_UsesNativeVueLocationForScriptImport()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var declarationPath = Path.Combine(tempDirectory, "UserBadge.vue");
            await File.WriteAllTextAsync(declarationPath, "<template><div>UserBadge</div></template>");

            var scriptDocument = new DocumentSnapshot(
                Path.Combine(tempDirectory, "consumer.ts"),
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
            Assert.AreEqual(VueHostWorkspaceResolver.NormalizePath(declarationPath), resolved.Value.AbsolutePath);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "JazorVueHostMarkupComponentBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
