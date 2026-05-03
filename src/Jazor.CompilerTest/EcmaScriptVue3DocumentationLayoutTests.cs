namespace Jazor.ComplierTest;

[TestClass]
public sealed class EcmaScriptVue3DocumentationLayoutTests
{
    [TestMethod]
    public void Vue3_Documentation_IsSplitIntoDedicatedGoalPlanDoneDirectories()
    {
        var root = ResolveRepositoryRoot();

        var goalDir = Path.Combine(root, "docs", "01-目标", "ecmascript.vue3");
        var planDir = Path.Combine(root, "docs", "02-计划", "ecmascript.vue3");
        var doneDir = Path.Combine(root, "docs", "03-完成", "ecmascript.vue3");

        Assert.IsTrue(Directory.Exists(goalDir), $"Missing goal directory: {goalDir}");
        Assert.IsTrue(Directory.Exists(planDir), $"Missing plan directory: {planDir}");
        Assert.IsTrue(Directory.Exists(doneDir), $"Missing done directory: {doneDir}");

        Assert.IsTrue(File.Exists(Path.Combine(goalDir, "README.md")));
        Assert.IsTrue(File.Exists(Path.Combine(goalDir, "vue3-balanced-design.md")));
        Assert.IsTrue(File.Exists(Path.Combine(goalDir, "vue3-module-mapping-rules.md")));
        Assert.IsTrue(File.Exists(Path.Combine(goalDir, "vue3-api-coverage-matrix.md")));
        Assert.IsTrue(File.Exists(Path.Combine(goalDir, "vue3-mapping-details.md")));

        Assert.IsTrue(File.Exists(Path.Combine(planDir, "README.md")));
        Assert.IsTrue(File.Exists(Path.Combine(planDir, "ECMAScript.Vue3.Authoring.ImplementationPlan.md")));

        Assert.IsTrue(File.Exists(Path.Combine(doneDir, "README.md")));
        Assert.IsTrue(File.Exists(Path.Combine(doneDir, "status.md")));

        var legacyGoalFiles = Directory.GetFiles(
            Path.Combine(root, "docs", "01-目标", "ecmascript"),
            "vue3-*.md",
            SearchOption.TopDirectoryOnly);
        Assert.AreEqual(
            0,
            legacyGoalFiles.Length,
            $"Legacy Vue3 goal docs should be moved out of docs/01-目标/ecmascript/: {string.Join(", ", legacyGoalFiles.Select(Path.GetFileName))}");

        var legacyPlanFile = Path.Combine(
            root,
            "docs",
            "02-计划",
            "ecmascript",
            "ECMAScript.Vue3.Authoring.ImplementationPlan.md");
        Assert.IsFalse(File.Exists(legacyPlanFile), $"Legacy plan doc should be moved: {legacyPlanFile}");
    }

    [TestMethod]
    public void Vue3_Documentation_Entrypoints_UseNewSplitPaths()
    {
        var root = ResolveRepositoryRoot();
        var docsReadme = File.ReadAllText(Path.Combine(root, "docs", "README.md"));
        var dashboard = File.ReadAllText(Path.Combine(root, "docs", "02-计划", "workstream-dashboard.md"));

        StringAssert.Contains(docsReadme, "01-目标/ecmascript.vue3/vue3-balanced-design.md");
        StringAssert.Contains(docsReadme, "02-计划/ecmascript.vue3/ECMAScript.Vue3.Authoring.ImplementationPlan.md");
        StringAssert.Contains(docsReadme, "03-完成/ecmascript.vue3/status.md");

        Assert.IsFalse(
            docsReadme.Contains("01-目标/ecmascript/vue3-", StringComparison.Ordinal),
            "docs/README.md still contains old ecmascript/vue3 links.");
        Assert.IsFalse(
            docsReadme.Contains("02-计划/ecmascript/ECMAScript.Vue3.Authoring.ImplementationPlan.md", StringComparison.Ordinal),
            "docs/README.md still contains old ecmascript plan link.");

        StringAssert.Contains(dashboard, "../03-完成/ecmascript.vue3/status.md");
    }

    private static string ResolveRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Jazor.slnx")))
                return current.FullName;
            current = current.Parent;
        }

        throw new InvalidOperationException("Cannot locate repository root (Jazor.slnx).");
    }
}

