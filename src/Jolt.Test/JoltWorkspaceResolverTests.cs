using Jazor.VueContracts.Protocol;
using Jolt.Workspace;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace Jolt.Test;

[TestClass]
public sealed class JoltWorkspaceResolverTests
{
    [TestMethod]
    public void Jolt_WorkspaceResolver_RootedDocumentWithoutSlnx_DoesNotFallbackToConfiguredWorkspaceFolders()
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

            using var _ = JoltWorkspaceResolver.PushWorkspaceFolderRoots([workspaceA, workspaceB]);
            var exception = Assert.Throws<InvalidOperationException>(() => JoltWorkspaceResolver
                .GetWorkspaceSearchRoots(
                    documentPath: outsideDocument.DocumentPath,
                    secondaryDocumentPath: null,
                    openDocuments: [outsideDocument])
                .ToArray());

            StringAssert.Contains(exception.Message, "No solution .slnx was found");
            StringAssert.Contains(exception.Message, "Open the project from a solution directory that contains a .slnx file.");
        }
        finally
        {
            DeleteDirectory(baseDirectory);
        }
    }

    [TestMethod]
    public void Jolt_WorkspaceResolver_DoesNotEscapeConfiguredWorkspaceFolderBoundary()
    {
        var baseDirectory = CreateTemporaryDirectory();
        var solutionPath = string.Empty;
        try
        {
            var workspace = CreateScopedSolutionFixture(baseDirectory, "workspace", "WorkspaceApp");
            solutionPath = workspace.SolutionPath;

            var workspaceRoot = workspace.SolutionRoot;
            var nestedDirectory = Path.Combine(workspace.ProjectRoot, "src", "features", "cards");
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

            using var _ = JoltWorkspaceResolver.PushWorkspaceFolderRoots([workspaceRoot]);
            var roots = JoltWorkspaceResolver
                .GetWorkspaceSearchRoots(rootDocumentPath, secondaryDocumentPath: outsideDocumentPath, openDocuments)
                .Select(static root => Path.GetFullPath(root))
                .ToArray();

            CollectionAssert.AreEqual(new[] { Path.GetFullPath(workspace.ProjectRoot) }, roots);
            Assert.IsTrue(
                roots.All(root => IsSameOrDescendantPath(root, workspaceRoot)),
                "Expected search roots to remain within the configured workspace root.");
        }
        finally
        {
            JoltWorkspaceResolver.InvalidatePath(solutionPath);
            DeleteDirectory(baseDirectory);
        }
    }

    [TestMethod]
    public void Jolt_WorkspaceResolver_MultiRootScope_OnlyEmitsCurrentWorkspaceBranch()
    {
        var baseDirectory = CreateTemporaryDirectory();
        var solutionPathA = string.Empty;
        var solutionPathB = string.Empty;
        try
        {
            var workspaceA = CreateScopedSolutionFixture(baseDirectory, "workspace-a", "WorkspaceA");
            var workspaceB = CreateScopedSolutionFixture(baseDirectory, "workspace-b", "WorkspaceB");
            solutionPathA = workspaceA.SolutionPath;
            solutionPathB = workspaceB.SolutionPath;

            var activeDirectory = Path.Combine(workspaceA.ProjectRoot, "src", "views");
            var unrelatedDirectory = Path.Combine(workspaceB.ProjectRoot, "src", "views");
            Directory.CreateDirectory(activeDirectory);
            Directory.CreateDirectory(unrelatedDirectory);

            var activeDocumentPath = Path.Combine(activeDirectory, "Dashboard.jazor");
            var activeDocument = new DocumentSnapshot(
                activeDocumentPath,
                DocumentKind.Jazor,
                "<Dashboard />",
                "1");

            using var _ = JoltWorkspaceResolver.PushWorkspaceFolderRoots([workspaceA.SolutionRoot, workspaceB.SolutionRoot]);
            var roots = JoltWorkspaceResolver
                .GetWorkspaceSearchRoots(activeDocumentPath, secondaryDocumentPath: null, [activeDocument])
                .Select(static root => Path.GetFullPath(root))
                .ToArray();

            CollectionAssert.AreEqual(new[] { Path.GetFullPath(workspaceA.ProjectRoot) }, roots);
            Assert.IsFalse(
                roots.Any(root => IsSameOrDescendantPath(root, workspaceB.SolutionRoot)),
                "Expected unrelated workspace root to be excluded when active document is scoped to another root.");
        }
        finally
        {
            JoltWorkspaceResolver.InvalidatePath(solutionPathA);
            JoltWorkspaceResolver.InvalidatePath(solutionPathB);
            DeleteDirectory(baseDirectory);
        }
    }

    [TestMethod]
    public void Jolt_WorkspaceResolver_MultiRootScope_WithCrossRootOpenDocuments_StillEmitsOnlyActiveBranch()
    {
        var baseDirectory = CreateTemporaryDirectory();
        var solutionPathA = string.Empty;
        var solutionPathB = string.Empty;
        try
        {
            var workspaceA = CreateScopedSolutionFixture(baseDirectory, "workspace-a", "WorkspaceA");
            var workspaceB = CreateScopedSolutionFixture(baseDirectory, "workspace-b", "WorkspaceB");
            solutionPathA = workspaceA.SolutionPath;
            solutionPathB = workspaceB.SolutionPath;

            var activeDirectory = Path.Combine(workspaceA.ProjectRoot, "src", "views");
            var foreignDirectory = Path.Combine(workspaceB.ProjectRoot, "src", "views");
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

            using var _ = JoltWorkspaceResolver.PushWorkspaceFolderRoots([workspaceA.SolutionRoot, workspaceB.SolutionRoot]);
            var roots = JoltWorkspaceResolver
                .GetWorkspaceSearchRoots(activeDocumentPath, secondaryDocumentPath: null, openDocuments)
                .Select(static root => Path.GetFullPath(root))
                .ToArray();

            CollectionAssert.AreEqual(new[] { Path.GetFullPath(workspaceA.ProjectRoot) }, roots);
            Assert.IsFalse(
                roots.Any(root => IsSameOrDescendantPath(root, workspaceB.SolutionRoot)),
                "Expected unrelated workspace branch to be excluded.");
        }
        finally
        {
            JoltWorkspaceResolver.InvalidatePath(solutionPathA);
            JoltWorkspaceResolver.InvalidatePath(solutionPathB);
            DeleteDirectory(baseDirectory);
        }
    }

    [TestMethod]
    public void Jolt_WorkspaceResolver_TryResolveTrackedVueComponent_PrefersActiveWorkspaceRoot()
    {
        var baseDirectory = CreateTemporaryDirectory();
        var solutionPathA = string.Empty;
        var solutionPathB = string.Empty;
        try
        {
            var workspaceA = CreateScopedSolutionFixture(baseDirectory, "workspace-a", "WorkspaceA");
            var workspaceB = CreateScopedSolutionFixture(baseDirectory, "workspace-b", "WorkspaceB");
            solutionPathA = workspaceA.SolutionPath;
            solutionPathB = workspaceB.SolutionPath;

            var activeDirectory = Path.Combine(workspaceA.ProjectRoot, "src", "pages");
            var componentDirectoryA = Path.Combine(workspaceA.ProjectRoot, "src", "components");
            var componentDirectoryB = Path.Combine(workspaceB.ProjectRoot, "src", "components");
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

            using var _ = JoltWorkspaceResolver.PushWorkspaceFolderRoots([workspaceA.SolutionRoot, workspaceB.SolutionRoot]);
            var resolved = JoltWorkspaceResolver.TryResolveTrackedVueComponent(
                activeDocumentPath,
                "FancyButton",
                openDocuments,
                out var component);

            Assert.IsTrue(resolved, "Expected tracked component resolution to succeed.");
            Assert.AreEqual(
                JoltWorkspaceResolver.NormalizePath(componentPathA),
                component.AbsolutePath);
            Assert.IsTrue(
                component.ImportPath.Contains("components/FancyButton.vue", StringComparison.OrdinalIgnoreCase),
                "Expected import path to target active workspace component.");
        }
        finally
        {
            JoltWorkspaceResolver.InvalidatePath(solutionPathA);
            JoltWorkspaceResolver.InvalidatePath(solutionPathB);
            DeleteDirectory(baseDirectory);
        }
    }

    [TestMethod]
    public void Jolt_WorkspaceResolver_EnumerateTrackedVueComponents_RestrictsToActiveWorkspaceRoot()
    {
        var baseDirectory = CreateTemporaryDirectory();
        var solutionPathA = string.Empty;
        var solutionPathB = string.Empty;
        try
        {
            var workspaceA = CreateScopedSolutionFixture(baseDirectory, "workspace-a", "WorkspaceA");
            var workspaceB = CreateScopedSolutionFixture(baseDirectory, "workspace-b", "WorkspaceB");
            solutionPathA = workspaceA.SolutionPath;
            solutionPathB = workspaceB.SolutionPath;

            var activeDirectory = Path.Combine(workspaceA.ProjectRoot, "src", "pages");
            var componentDirectoryA = Path.Combine(workspaceA.ProjectRoot, "src", "components");
            var componentDirectoryB = Path.Combine(workspaceB.ProjectRoot, "src", "components");
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

            using var _ = JoltWorkspaceResolver.PushWorkspaceFolderRoots([workspaceA.SolutionRoot, workspaceB.SolutionRoot]);
            var components = JoltWorkspaceResolver
                .EnumerateTrackedVueComponents(activeDocumentPath, openDocuments)
                .ToArray();

            Assert.AreEqual(1, components.Length);
            Assert.AreEqual("AlphaCard", components[0].ComponentName);
            Assert.IsTrue(
                IsSameOrDescendantPath(components[0].AbsolutePath, workspaceA.SolutionRoot),
                "Expected tracked component enumeration to stay within active workspace root.");
        }
        finally
        {
            JoltWorkspaceResolver.InvalidatePath(solutionPathA);
            JoltWorkspaceResolver.InvalidatePath(solutionPathB);
            DeleteDirectory(baseDirectory);
        }
    }

    [TestMethod]
    public void Jolt_WorkspaceResolver_GetWorkspaceSearchRoots_SlnxScope_StaysInsideOwningProject()
    {
        var baseDirectory = CreateTemporaryDirectory();
        var solutionPath = string.Empty;
        try
        {
            var solutionRoot = Path.Combine(baseDirectory, "solution");
            Directory.CreateDirectory(solutionRoot);

            var projectA = CreateScopedSolutionProject(solutionRoot, "ProjectA");
            var projectB = CreateScopedSolutionProject(solutionRoot, "ProjectB");
            solutionPath = WriteScopedSolutionFile(solutionRoot, "scoped.slnx", "ProjectA/ProjectA.csproj", "ProjectB/ProjectB.csproj");

            var activeDirectory = Path.Combine(projectA, "Features", "Pages");
            var siblingDirectory = Path.Combine(projectB, "Features", "Pages");
            Directory.CreateDirectory(activeDirectory);
            Directory.CreateDirectory(siblingDirectory);

            var activeDocumentPath = Path.Combine(activeDirectory, "Home.jazor");
            var siblingDocumentPath = Path.Combine(siblingDirectory, "Reports.jazor");
            var openDocuments = new[]
            {
                new DocumentSnapshot(siblingDocumentPath, DocumentKind.Jazor, "<Reports />", "1"),
                new DocumentSnapshot(activeDocumentPath, DocumentKind.Jazor, "<Home />", "1")
            };

            var roots = JoltWorkspaceResolver
                .GetWorkspaceSearchRoots(activeDocumentPath, secondaryDocumentPath: siblingDocumentPath, openDocuments)
                .Select(Path.GetFullPath)
                .ToArray();

            CollectionAssert.AreEqual(new[] { Path.GetFullPath(projectA) }, roots);
            Assert.IsFalse(
                roots.Any(root => IsSameOrDescendantPath(root, projectB)),
                "Expected solution-scoped search roots to remain inside the owning project only.");
        }
        finally
        {
            JoltWorkspaceResolver.InvalidatePath(solutionPath);
            DeleteDirectory(baseDirectory);
        }
    }

    [TestMethod]
    public void Jolt_WorkspaceResolver_ComponentResolution_SlnxScope_DoesNotCrossSiblingProjects()
    {
        var baseDirectory = CreateTemporaryDirectory();
        var solutionPath = string.Empty;
        try
        {
            var solutionRoot = Path.Combine(baseDirectory, "solution");
            Directory.CreateDirectory(solutionRoot);

            var projectA = CreateScopedSolutionProject(solutionRoot, "ProjectA");
            var projectB = CreateScopedSolutionProject(solutionRoot, "ProjectB");
            solutionPath = WriteScopedSolutionFile(solutionRoot, "scoped.slnx", "ProjectA/ProjectA.csproj", "ProjectB/ProjectB.csproj");

            var activeDirectory = Path.Combine(projectA, "Features", "Pages");
            var siblingComponentDirectory = Path.Combine(projectB, "Shared", "Components");
            Directory.CreateDirectory(activeDirectory);
            Directory.CreateDirectory(siblingComponentDirectory);

            var activeDocumentPath = Path.Combine(activeDirectory, "Home.jazor");
            var siblingComponentPath = Path.Combine(siblingComponentDirectory, "FancyButton.vue");
            File.WriteAllText(siblingComponentPath, "<template><button>foreign</button></template>");

            var openDocuments = new[]
            {
                new DocumentSnapshot(siblingComponentPath, DocumentKind.Vue, "<template><button>foreign</button></template>", "1"),
                new DocumentSnapshot(activeDocumentPath, DocumentKind.Jazor, "<FancyButton />", "1")
            };

            var trackedResolved = JoltWorkspaceResolver.TryResolveTrackedVueComponent(
                activeDocumentPath,
                "FancyButton",
                openDocuments,
                out var trackedComponent);
            var workspaceResolved = JoltWorkspaceResolver.ResolveWorkspaceVueComponent(
                activeDocumentPath,
                "FancyButton",
                openDocuments,
                CancellationToken.None);

            Assert.IsFalse(trackedResolved, "Expected tracked component resolution to ignore sibling-project documents from the same .slnx.");
            Assert.AreEqual(default, trackedComponent);
            Assert.IsNull(workspaceResolved, "Expected workspace component resolution to stop at the owning project instead of crossing into a sibling project.");
            Assert.AreEqual(
                0,
                JoltWorkspaceResolver.EnumerateTrackedVueComponents(activeDocumentPath, openDocuments).Count(),
                "Expected tracked component enumeration to exclude sibling-project Vue documents.");
        }
        finally
        {
            JoltWorkspaceResolver.InvalidatePath(solutionPath);
            DeleteDirectory(baseDirectory);
        }
    }

    [TestMethod]
    public async Task Jolt_WorkspaceResolver_ResolveDocumentAsync_PrefersOpenDocumentSnapshotOverDiskContent()
    {
        var baseDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(baseDirectory, "Counter.vue");
            await File.WriteAllTextAsync(documentPath, "<template>disk</template>");
            var openDocument = new DocumentSnapshot(
                documentPath,
                DocumentKind.Vue,
                "<template>open</template>",
                "2");

            var resolved = await JoltWorkspaceResolver.ResolveDocumentAsync(
                documentPath,
                [openDocument],
                CancellationToken.None);

            Assert.IsNotNull(resolved);
            Assert.AreEqual("<template>open</template>", resolved.Text);
            Assert.AreEqual("2", resolved.Version);
        }
        finally
        {
            DeleteDirectory(baseDirectory);
        }
    }

    [TestMethod]
    public void Jolt_WorkspaceResolver_EnumerateWorkspaceFiles_ThrowsWhenCancellationRequested()
    {
        var baseDirectory = CreateTemporaryDirectory();
        try
        {
            Directory.CreateDirectory(Path.Combine(baseDirectory, "src"));
            File.WriteAllText(Path.Combine(baseDirectory, "src", "Counter.vue"), "<template />");

            using var cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();

            Assert.Throws<OperationCanceledException>(() =>
            {
                _ = JoltWorkspaceResolver
                    .EnumerateWorkspaceFiles([baseDirectory], "*.vue", cancellationSource.Token)
                    .ToArray();
            });
        }
        finally
        {
            DeleteDirectory(baseDirectory);
        }
    }

    [TestMethod]
    public void Jolt_WorkspaceResolver_ResolveWorkspaceVueComponent_ReachesProjectRootForDeepDocumentTrees()
    {
        var baseDirectory = CreateTemporaryDirectory();
        var solutionPath = string.Empty;
        try
        {
            var workspace = CreateScopedSolutionFixture(baseDirectory, "workspace", "WorkspaceApp", ".");
            solutionPath = workspace.SolutionPath;

            var projectRoot = workspace.ProjectRoot;
            var deepDocumentDirectory = Path.Combine(projectRoot, "src", "features", "alpha", "beta", "gamma", "pages");
            var sharedComponentDirectory = Path.Combine(projectRoot, "src", "components");
            Directory.CreateDirectory(deepDocumentDirectory);
            Directory.CreateDirectory(sharedComponentDirectory);
            File.WriteAllText(Path.Combine(projectRoot, "package.json"), """{ "name": "workspace-root" }""");

            var documentPath = Path.Combine(deepDocumentDirectory, "Home.jazor");
            File.WriteAllText(documentPath, "<FancyButton />");
            var componentPath = Path.Combine(sharedComponentDirectory, "FancyButton.vue");
            File.WriteAllText(componentPath, "<template><button /></template>");

            var openDocuments = new[]
            {
                new DocumentSnapshot(documentPath, DocumentKind.Jazor, "<FancyButton />", "1")
            };

            var resolved = JoltWorkspaceResolver.ResolveWorkspaceVueComponent(
                documentPath,
                "FancyButton",
                openDocuments,
                CancellationToken.None);

            Assert.IsNotNull(resolved, "Expected workspace resolution to reach the project root for deep document paths.");
            Assert.AreEqual(
                JoltWorkspaceResolver.NormalizePath(componentPath),
                resolved.Value.AbsolutePath);
            Assert.AreEqual(
                JoltWorkspaceResolver.ToImportPath(deepDocumentDirectory, componentPath),
                resolved.Value.ImportPath);
        }
        finally
        {
            JoltWorkspaceResolver.InvalidatePath(solutionPath);
            DeleteDirectory(baseDirectory);
        }
    }

    [TestMethod]
    public void Jolt_WorkspaceResolver_GetWorkspaceSearchRoots_PreservesImmediateTempWorkspaceDirectory()
    {
        var workspaceRoot = Path.Combine(Path.GetTempPath(), "jolt-temp-workspace-" + Guid.NewGuid().ToString("N"));
        var solutionPath = string.Empty;
        try
        {
            Directory.CreateDirectory(workspaceRoot);
            WriteScopedProjectFile(workspaceRoot, "TempWorkspace");
            solutionPath = WriteScopedSolutionFile(workspaceRoot, "TempWorkspace.slnx", "TempWorkspace.csproj");
            var documentPath = Path.Combine(workspaceRoot, "Counter.jazor");
            File.WriteAllText(documentPath, "<Counter />");

            var roots = JoltWorkspaceResolver
                .GetWorkspaceSearchRoots(
                    documentPath,
                    secondaryDocumentPath: null,
                    [new DocumentSnapshot(documentPath, DocumentKind.Jazor, "<Counter />", "1")])
                .Select(Path.GetFullPath)
                .ToArray();

            CollectionAssert.AreEqual(new[] { Path.GetFullPath(workspaceRoot) }, roots);
            Assert.IsFalse(
                roots.Any(root => string.Equals(
                    root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                    Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                    StringComparison.OrdinalIgnoreCase)),
                "Expected immediate temp workspace discovery to stop before the shared system temp root.");
        }
        finally
        {
            JoltWorkspaceResolver.InvalidatePath(solutionPath);
            DeleteDirectory(workspaceRoot);
        }
    }

    [TestMethod]
    public void Jolt_WorkspaceResolver_PushWorkspaceFolderRoots_NestedScopesRestorePreviousState()
    {
        var baseDirectory = CreateTemporaryDirectory();
        var solutionPathA = string.Empty;
        var solutionPathB = string.Empty;
        try
        {
            var workspaceA = CreateScopedSolutionFixture(baseDirectory, "workspace-a", "WorkspaceA");
            var workspaceB = CreateScopedSolutionFixture(baseDirectory, "workspace-b", "WorkspaceB");
            solutionPathA = workspaceA.SolutionPath;
            solutionPathB = workspaceB.SolutionPath;

            var documentA = new DocumentSnapshot(
                Path.Combine(workspaceA.ProjectRoot, "Counter.jazor"),
                DocumentKind.Jazor,
                "<Counter />",
                "1");
            var documentB = new DocumentSnapshot(
                Path.Combine(workspaceB.ProjectRoot, "Counter.jazor"),
                DocumentKind.Jazor,
                "<Counter />",
                "1");

            using var scopeA = JoltWorkspaceResolver.PushWorkspaceFolderRoots([workspaceA.SolutionRoot]);
            var rootsInScopeA = JoltWorkspaceResolver
                .GetWorkspaceSearchRoots(documentA.DocumentPath, secondaryDocumentPath: null, [documentA])
                .Select(Path.GetFullPath)
                .ToArray();
            CollectionAssert.AreEqual(new[] { Path.GetFullPath(workspaceA.ProjectRoot) }, rootsInScopeA);

            using (JoltWorkspaceResolver.PushWorkspaceFolderRoots([workspaceB.SolutionRoot]))
            {
                var rootsInScopeB = JoltWorkspaceResolver
                    .GetWorkspaceSearchRoots(documentB.DocumentPath, secondaryDocumentPath: null, [documentB])
                    .Select(Path.GetFullPath)
                    .ToArray();
                CollectionAssert.AreEqual(new[] { Path.GetFullPath(workspaceB.ProjectRoot) }, rootsInScopeB);
            }

            var rootsAfterNestedScope = JoltWorkspaceResolver
                .GetWorkspaceSearchRoots(documentA.DocumentPath, secondaryDocumentPath: null, [documentA])
                .Select(Path.GetFullPath)
                .ToArray();
            CollectionAssert.AreEqual(new[] { Path.GetFullPath(workspaceA.ProjectRoot) }, rootsAfterNestedScope);
        }
        finally
        {
            JoltWorkspaceResolver.InvalidatePath(solutionPathA);
            JoltWorkspaceResolver.InvalidatePath(solutionPathB);
            DeleteDirectory(baseDirectory);
        }
    }

    [TestMethod]
    public void Jolt_WorkspaceResolver_EnumerateWorkspaceFiles_BoundsCacheGrowth()
    {
        var baseDirectory = CreateTemporaryDirectory();
        try
        {
            JoltWorkspaceResolver.InvalidatePath(string.Empty);

            for (var index = 0; index < 1025; index++)
            {
                var workspaceRoot = Path.Combine(baseDirectory, "workspace-" + index.ToString("D4"));
                Directory.CreateDirectory(workspaceRoot);
                File.WriteAllText(Path.Combine(workspaceRoot, "Component.vue"), "<template />");

                _ = JoltWorkspaceResolver
                    .EnumerateWorkspaceFiles([workspaceRoot], "*.vue", CancellationToken.None)
                    .ToArray();
            }

            var cacheField = typeof(JoltWorkspaceResolver).GetField(
                "WorkspaceFileCache",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(cacheField);

            var cache = cacheField.GetValue(null) as ConcurrentDictionary<string, string[]>;
            Assert.IsNotNull(cache);
            Assert.IsTrue(
                cache.Count <= 1000,
                $"Expected workspace file cache to remain bounded, but found {cache.Count} entries.");
        }
        finally
        {
            JoltWorkspaceResolver.InvalidatePath(string.Empty);
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
            "JoltWorkspaceResolverTests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private readonly record struct ScopedSolutionFixture(
        string SolutionRoot,
        string ProjectRoot,
        string SolutionPath);

    private static ScopedSolutionFixture CreateScopedSolutionFixture(
        string baseDirectory,
        string solutionDirectoryName,
        string projectName,
        string projectDirectory = "")
    {
        var solutionRoot = Path.Combine(baseDirectory, solutionDirectoryName);
        Directory.CreateDirectory(solutionRoot);

        var normalizedProjectDirectory = string.IsNullOrWhiteSpace(projectDirectory)
            ? projectName
            : projectDirectory;
        var projectRoot = string.Equals(normalizedProjectDirectory, ".", StringComparison.Ordinal)
            ? solutionRoot
            : Path.Combine(solutionRoot, normalizedProjectDirectory);
        Directory.CreateDirectory(projectRoot);

        var projectPath = WriteScopedProjectFile(projectRoot, projectName);
        var projectRelativePath = Path.GetRelativePath(solutionRoot, projectPath).Replace('\\', '/');
        var solutionPath = WriteScopedSolutionFile(solutionRoot, solutionDirectoryName + ".slnx", projectRelativePath);
        return new ScopedSolutionFixture(solutionRoot, projectRoot, solutionPath);
    }

    private static string CreateScopedSolutionProject(string solutionRoot, string projectName)
    {
        var projectRoot = Path.Combine(solutionRoot, projectName);
        Directory.CreateDirectory(projectRoot);
        WriteScopedProjectFile(projectRoot, projectName);
        return projectRoot;
    }

    private static string WriteScopedProjectFile(string projectRoot, string projectName)
    {
        var projectPath = Path.Combine(projectRoot, projectName + ".csproj");
        File.WriteAllText(
            projectPath,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);
        return projectPath;
    }

    private static string WriteScopedSolutionFile(string solutionRoot, string fileName, params string[] projectPaths)
    {
        var solutionPath = Path.Combine(solutionRoot, fileName);
        var projectLines = string.Join(
            Environment.NewLine,
            projectPaths.Select(static projectPath => $"  <Project Path=\"{projectPath.Replace('\\', '/')}\" />"));
        File.WriteAllText(
            solutionPath,
            $$"""
            <Solution>
            {{projectLines}}
            </Solution>
            """);
        return solutionPath;
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
