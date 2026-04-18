using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Workspace;
using System.Threading;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostWorkspaceResolverTests
{
    [TestMethod]
    public void JazorVueHost_WorkspaceResolver_RestrictsRootsToConfiguredWorkspaceFolders()
    {
        var baseDirectory = CreateTemporaryDirectory();
        try
        {
            var workspaceA = Path.Combine(baseDirectory, "workspace-a");
            var workspaceB = Path.Combine(baseDirectory, "workspace-b");
            var outside = Path.Combine(baseDirectory, "outside");
            Directory.CreateDirectory(workspaceA);
            Directory.CreateDirectory(workspaceB);
            Directory.CreateDirectory(outside);

            var outsideDocument = new DocumentSnapshot(
                Path.Combine(outside, "Counter.jazor"),
                DocumentKind.Jazor,
                "<Counter />",
                "1");

            using var _ = VueHostWorkspaceResolver.PushWorkspaceFolderRoots([workspaceA, workspaceB]);
            var roots = VueHostWorkspaceResolver
                .GetWorkspaceSearchRoots(
                    documentPath: outsideDocument.DocumentPath,
                    secondaryDocumentPath: null,
                    openDocuments: [outsideDocument])
                .Select(static root => Path.GetFullPath(root))
                .ToArray();

            CollectionAssert.Contains(roots, Path.GetFullPath(workspaceA));
            CollectionAssert.Contains(roots, Path.GetFullPath(workspaceB));
            Assert.IsFalse(
                roots.Any(root => IsSameOrDescendantPath(root, outside)),
                "Expected workspace search roots to stay inside configured workspace folders.");
        }
        finally
        {
            DeleteDirectory(baseDirectory);
        }
    }

    [TestMethod]
    public void JazorVueHost_WorkspaceResolver_DoesNotEscapeConfiguredWorkspaceFolderBoundary()
    {
        var baseDirectory = CreateTemporaryDirectory();
        try
        {
            var workspaceRoot = Path.Combine(baseDirectory, "workspace");
            var nestedDirectory = Path.Combine(workspaceRoot, "src", "features", "cards");
            var outsideDirectory = Path.Combine(baseDirectory, "outside");
            Directory.CreateDirectory(nestedDirectory);
            Directory.CreateDirectory(outsideDirectory);

            var rootDocumentPath = Path.Combine(nestedDirectory, "Counter.jazor");
            var outsideDocumentPath = Path.Combine(outsideDirectory, "Other.jazor");
            var openDocuments = new[]
            {
                new DocumentSnapshot(rootDocumentPath, DocumentKind.Jazor, "<Counter />", "1"),
                new DocumentSnapshot(outsideDocumentPath, DocumentKind.Jazor, "<Other />", "1")
            };

            using var _ = VueHostWorkspaceResolver.PushWorkspaceFolderRoots([workspaceRoot]);
            var roots = VueHostWorkspaceResolver
                .GetWorkspaceSearchRoots(rootDocumentPath, secondaryDocumentPath: outsideDocumentPath, openDocuments)
                .Select(static root => Path.GetFullPath(root))
                .ToArray();

            Assert.IsTrue(roots.Length > 0, "Expected at least one workspace search root.");
            Assert.IsTrue(
                roots.All(root => IsSameOrDescendantPath(root, workspaceRoot)),
                "Expected search roots to remain within the configured workspace root.");
        }
        finally
        {
            DeleteDirectory(baseDirectory);
        }
    }

    [TestMethod]
    public void JazorVueHost_WorkspaceResolver_MultiRootScope_OnlyEmitsCurrentWorkspaceBranch()
    {
        var baseDirectory = CreateTemporaryDirectory();
        try
        {
            var workspaceA = Path.Combine(baseDirectory, "workspace-a");
            var workspaceB = Path.Combine(baseDirectory, "workspace-b");
            var activeDirectory = Path.Combine(workspaceA, "src", "views");
            var unrelatedDirectory = Path.Combine(workspaceB, "src", "views");
            Directory.CreateDirectory(activeDirectory);
            Directory.CreateDirectory(unrelatedDirectory);

            var activeDocumentPath = Path.Combine(activeDirectory, "Dashboard.jazor");
            var activeDocument = new DocumentSnapshot(
                activeDocumentPath,
                DocumentKind.Jazor,
                "<Dashboard />",
                "1");

            using var _ = VueHostWorkspaceResolver.PushWorkspaceFolderRoots([workspaceA, workspaceB]);
            var roots = VueHostWorkspaceResolver
                .GetWorkspaceSearchRoots(activeDocumentPath, secondaryDocumentPath: null, [activeDocument])
                .Select(static root => Path.GetFullPath(root))
                .ToArray();

            Assert.IsTrue(roots.Length > 0, "Expected workspace search roots to be emitted.");
            Assert.IsTrue(
                roots.All(root => IsSameOrDescendantPath(root, workspaceA)),
                "Expected search roots to stay within the active workspace root.");
            Assert.IsFalse(
                roots.Any(root => IsSameOrDescendantPath(root, workspaceB)),
                "Expected unrelated workspace root to be excluded when active document is scoped to another root.");
        }
        finally
        {
            DeleteDirectory(baseDirectory);
        }
    }

    [TestMethod]
    public void JazorVueHost_WorkspaceResolver_MultiRootScope_WithCrossRootOpenDocuments_StillEmitsOnlyActiveBranch()
    {
        var baseDirectory = CreateTemporaryDirectory();
        try
        {
            var workspaceA = Path.Combine(baseDirectory, "workspace-a");
            var workspaceB = Path.Combine(baseDirectory, "workspace-b");
            var activeDirectory = Path.Combine(workspaceA, "src", "views");
            var foreignDirectory = Path.Combine(workspaceB, "src", "views");
            Directory.CreateDirectory(activeDirectory);
            Directory.CreateDirectory(foreignDirectory);

            var activeDocumentPath = Path.Combine(activeDirectory, "Dashboard.jazor");
            var foreignDocumentPath = Path.Combine(foreignDirectory, "Reports.jazor");
            var openDocuments = new[]
            {
                new DocumentSnapshot(
                    foreignDocumentPath,
                    DocumentKind.Jazor,
                    "<Reports />",
                    "1"),
                new DocumentSnapshot(
                    activeDocumentPath,
                    DocumentKind.Jazor,
                    "<Dashboard />",
                    "1")
            };

            using var _ = VueHostWorkspaceResolver.PushWorkspaceFolderRoots([workspaceA, workspaceB]);
            var roots = VueHostWorkspaceResolver
                .GetWorkspaceSearchRoots(activeDocumentPath, secondaryDocumentPath: null, openDocuments)
                .Select(static root => Path.GetFullPath(root))
                .ToArray();

            Assert.IsTrue(roots.Length > 0, "Expected workspace search roots to be emitted.");
            Assert.IsTrue(
                roots.All(root => IsSameOrDescendantPath(root, workspaceA)),
                "Expected active workspace branch to remain isolated.");
            Assert.IsFalse(
                roots.Any(root => IsSameOrDescendantPath(root, workspaceB)),
                "Expected unrelated workspace branch to be excluded.");
        }
        finally
        {
            DeleteDirectory(baseDirectory);
        }
    }

    [TestMethod]
    public void JazorVueHost_WorkspaceResolver_TryResolveTrackedVueComponent_PrefersActiveWorkspaceRoot()
    {
        var baseDirectory = CreateTemporaryDirectory();
        try
        {
            var workspaceA = Path.Combine(baseDirectory, "workspace-a");
            var workspaceB = Path.Combine(baseDirectory, "workspace-b");
            var activeDirectory = Path.Combine(workspaceA, "src", "pages");
            var componentDirectoryA = Path.Combine(workspaceA, "src", "components");
            var componentDirectoryB = Path.Combine(workspaceB, "src", "components");
            Directory.CreateDirectory(activeDirectory);
            Directory.CreateDirectory(componentDirectoryA);
            Directory.CreateDirectory(componentDirectoryB);

            var activeDocumentPath = Path.Combine(activeDirectory, "Home.jazor");
            var componentPathA = Path.Combine(componentDirectoryA, "FancyButton.vue");
            var componentPathB = Path.Combine(componentDirectoryB, "FancyButton.vue");
            var openDocuments = new[]
            {
                new DocumentSnapshot(componentPathB, DocumentKind.Vue, "<template>foreign</template>", "1"),
                new DocumentSnapshot(componentPathA, DocumentKind.Vue, "<template>active</template>", "1"),
                new DocumentSnapshot(activeDocumentPath, DocumentKind.Jazor, "<FancyButton />", "1")
            };

            using var _ = VueHostWorkspaceResolver.PushWorkspaceFolderRoots([workspaceA, workspaceB]);
            var resolved = VueHostWorkspaceResolver.TryResolveTrackedVueComponent(
                activeDocumentPath,
                "FancyButton",
                openDocuments,
                out var component);

            Assert.IsTrue(resolved, "Expected tracked component resolution to succeed.");
            Assert.AreEqual(
                VueHostWorkspaceResolver.NormalizePath(componentPathA),
                component.AbsolutePath);
            Assert.IsTrue(
                component.ImportPath.Contains("components/FancyButton.vue", StringComparison.OrdinalIgnoreCase),
                "Expected import path to target active workspace component.");
        }
        finally
        {
            DeleteDirectory(baseDirectory);
        }
    }

    [TestMethod]
    public void JazorVueHost_WorkspaceResolver_EnumerateTrackedVueComponents_RestrictsToActiveWorkspaceRoot()
    {
        var baseDirectory = CreateTemporaryDirectory();
        try
        {
            var workspaceA = Path.Combine(baseDirectory, "workspace-a");
            var workspaceB = Path.Combine(baseDirectory, "workspace-b");
            var activeDirectory = Path.Combine(workspaceA, "src", "pages");
            var componentDirectoryA = Path.Combine(workspaceA, "src", "components");
            var componentDirectoryB = Path.Combine(workspaceB, "src", "components");
            Directory.CreateDirectory(activeDirectory);
            Directory.CreateDirectory(componentDirectoryA);
            Directory.CreateDirectory(componentDirectoryB);

            var activeDocumentPath = Path.Combine(activeDirectory, "Home.jazor");
            var openDocuments = new[]
            {
                new DocumentSnapshot(Path.Combine(componentDirectoryA, "AlphaCard.vue"), DocumentKind.Vue, "<template />", "1"),
                new DocumentSnapshot(Path.Combine(componentDirectoryB, "BetaCard.vue"), DocumentKind.Vue, "<template />", "1"),
                new DocumentSnapshot(activeDocumentPath, DocumentKind.Jazor, "<Home />", "1")
            };

            using var _ = VueHostWorkspaceResolver.PushWorkspaceFolderRoots([workspaceA, workspaceB]);
            var components = VueHostWorkspaceResolver
                .EnumerateTrackedVueComponents(activeDocumentPath, openDocuments)
                .ToArray();

            Assert.AreEqual(1, components.Length);
            Assert.AreEqual("AlphaCard", components[0].ComponentName);
            Assert.IsTrue(
                IsSameOrDescendantPath(components[0].AbsolutePath, workspaceA),
                "Expected tracked component enumeration to stay within active workspace root.");
        }
        finally
        {
            DeleteDirectory(baseDirectory);
        }
    }

    private static bool IsSameOrDescendantPath(string path, string root)
    {
        var normalizedPath = path.Replace('\\', '/').TrimEnd('/');
        var normalizedRoot = root.Replace('\\', '/').TrimEnd('/');
        return string.Equals(normalizedPath, normalizedRoot, StringComparison.OrdinalIgnoreCase)
               || normalizedPath.StartsWith(normalizedRoot + "/", StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "JazorVueHostWorkspaceResolverTests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

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
}
