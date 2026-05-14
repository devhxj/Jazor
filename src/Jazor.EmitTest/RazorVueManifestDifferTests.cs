using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class RazorVueManifestDifferTests
{
    [TestMethod]
    public void RazorVueManifestDiffer_ClassifiesTemplatePatch_WhenOnlyTemplateHashChanges()
    {
        var previous = CreateManifest(CreateEntry(templateHash: "template-a", contentHash: "content-a", boundaryKind: RazorVueHmrBoundaryKind.TemplateOnly));
        var current = CreateManifest(CreateEntry(templateHash: "template-b", contentHash: "content-b", boundaryKind: RazorVueHmrBoundaryKind.TemplateOnly));

        var diff = RazorVueManifestDiffer.Diff(previous, current);

        Assert.AreEqual(RazorVueHotUpdateAction.TemplatePatch, diff.Action);
        Assert.IsFalse(diff.TopLevelMetadataChanged);
        Assert.AreEqual(RazorVueHotUpdateAction.TemplatePatch, diff.Modules[0].Action);
        Assert.IsTrue(diff.Modules[0].TemplateChanged);
        Assert.IsFalse(diff.Modules[0].LogicChanged);
    }

    [TestMethod]
    public void RazorVueManifestDiffer_ClassifiesLogicPatch_WhenLogicChangesInsideLogicSafeBoundary()
    {
        var previous = CreateManifest(CreateEntry(logicHash: "logic-a", contentHash: "content-a", boundaryKind: RazorVueHmrBoundaryKind.LogicSafe));
        var current = CreateManifest(CreateEntry(logicHash: "logic-b", contentHash: "content-b", boundaryKind: RazorVueHmrBoundaryKind.LogicSafe));

        var diff = RazorVueManifestDiffer.Diff(previous, current);

        Assert.AreEqual(RazorVueHotUpdateAction.LogicPatch, diff.Action);
        Assert.AreEqual(RazorVueHotUpdateAction.LogicPatch, diff.Modules[0].Action);
        Assert.IsTrue(diff.Modules[0].LogicChanged);
    }

    [TestMethod]
    public void RazorVueManifestDiffer_ClassifiesFullReload_WhenDescriptorChanges()
    {
        var previous = CreateManifest(CreateEntry(descriptorHash: "descriptor-a", boundaryKind: RazorVueHmrBoundaryKind.LogicSafe));
        var current = CreateManifest(CreateEntry(descriptorHash: "descriptor-b", boundaryKind: RazorVueHmrBoundaryKind.LogicSafe));

        var diff = RazorVueManifestDiffer.Diff(previous, current);

        Assert.AreEqual(RazorVueHotUpdateAction.FullReload, diff.Action);
        Assert.AreEqual(RazorVueHotUpdateAction.FullReload, diff.Modules[0].Action);
        Assert.IsTrue(diff.Modules[0].DescriptorChanged);
    }

    [TestMethod]
    public void RazorVueManifestDiffer_ClassifiesFullReload_WhenTopLevelPluginRequirementsChange()
    {
        var previous = CreateManifest(CreateEntry(), pluginRequirements: ["vuetify"]);
        var current = CreateManifest(CreateEntry(), pluginRequirements: ["feature-flags", "vuetify"]);

        var diff = RazorVueManifestDiffer.Diff(previous, current);

        Assert.AreEqual(RazorVueHotUpdateAction.FullReload, diff.Action);
        Assert.IsTrue(diff.TopLevelMetadataChanged);
        Assert.AreEqual(RazorVueHotUpdateAction.FullReload, diff.Modules[0].Action);
        StringAssert.Contains(diff.Reason, "plugin requirements");
    }

    [TestMethod]
    public void RazorVueManifestDiffer_ClassifiesFullReload_WhenComponentIsAdded()
    {
        var previous = CreateManifest(CreateEntry(componentId: "Demo.Host.CounterCard", componentName: "CounterCard", moduleId: "components/counter-card.mjs", relativeModulePath: "components/counter-card.mjs"));
        var current = CreateManifest(
            CreateEntry(componentId: "Demo.Host.CounterCard", componentName: "CounterCard", moduleId: "components/counter-card.mjs", relativeModulePath: "components/counter-card.mjs"),
            styles: null,
            pluginRequirements: null,
            CreateEntry(componentId: "Demo.Host.StatusBadge", componentName: "StatusBadge", moduleId: "components/status-badge.mjs", relativeModulePath: "components/status-badge.mjs"));

        var diff = RazorVueManifestDiffer.Diff(previous, current);

        Assert.AreEqual(RazorVueHotUpdateAction.FullReload, diff.Action);
        Assert.IsTrue(diff.Modules.Any(static module => module.ComponentName == "StatusBadge" && module.Action == RazorVueHotUpdateAction.FullReload));
    }

    [TestMethod]
    public void RazorVueManifestDiffer_ClassifiesFullReload_WhenContentChangesOutsideSplitHashes()
    {
        var previous = CreateManifest(CreateEntry(contentHash: "content-a", boundaryKind: RazorVueHmrBoundaryKind.TemplateOnly));
        var current = CreateManifest(CreateEntry(contentHash: "content-b", boundaryKind: RazorVueHmrBoundaryKind.TemplateOnly));

        var diff = RazorVueManifestDiffer.Diff(previous, current);

        Assert.AreEqual(RazorVueHotUpdateAction.FullReload, diff.Action);
        Assert.AreEqual(RazorVueHotUpdateAction.FullReload, diff.Modules[0].Action);
        Assert.IsTrue(diff.Modules[0].ContentChanged);
    }

    [TestMethod]
    public void RazorVueManifestDiffer_ClassifiesFullReload_WhenOnlyStyleHashChanges()
    {
        var previous = CreateManifest(CreateEntry(styleHash: "style-a", contentHash: "content-a", boundaryKind: RazorVueHmrBoundaryKind.LogicSafe));
        var current = CreateManifest(CreateEntry(styleHash: "style-b", contentHash: "content-b", boundaryKind: RazorVueHmrBoundaryKind.LogicSafe));

        var diff = RazorVueManifestDiffer.Diff(previous, current);

        Assert.AreEqual(RazorVueHotUpdateAction.FullReload, diff.Action);
        Assert.AreEqual(RazorVueHotUpdateAction.FullReload, diff.Modules[0].Action);
        Assert.IsTrue(diff.Modules[0].StyleChanged);
        StringAssert.Contains(diff.Modules[0].Reason, "Style block content changed");
    }

    [TestMethod]
    public void RazorVueManifestDiffer_ReportsUnifiedManifestProjectionReason_WhenPreviousProjectionIsMissing()
    {
        var current = CreateManifest(CreateEntry());

        var diff = RazorVueManifestDiffer.Diff(previous: null, current);

        Assert.AreEqual(RazorVueHotUpdateAction.FullReload, diff.Action);
        Assert.AreEqual("Previous Jazor manifest component projection is missing.", diff.Reason);
        Assert.IsTrue(diff.TopLevelMetadataChanged);
    }

    private static RazorVueManifestModel CreateManifest(
        RazorVueManifestEntry first,
        string[]? styles = null,
        string[]? pluginRequirements = null,
        params RazorVueManifestEntry[] rest)
        => new(
            "Demo.Host",
            new DateTime(2026, 4, 8, 0, 0, 0, DateTimeKind.Utc),
            [first, .. rest],
            (styles ?? ["vuetify/styles"]).ToList(),
            (pluginRequirements ?? ["vuetify"]).ToList());

    private static RazorVueManifestEntry CreateEntry(
        string componentId = "Demo.Host.ProfileForm",
        string moduleId = "components/profile-form.mjs",
        string componentName = "ProfileForm",
        string relativeModulePath = "components/profile-form.mjs",
        string sourceMapPath = "",
        string descriptorHash = "descriptor-hash",
        string templateHash = "template-hash",
        string logicHash = "logic-hash",
        string contentHash = "content-hash",
        string styleHash = "",
        RazorVueHmrBoundaryKind boundaryKind = RazorVueHmrBoundaryKind.LogicSafe)
        => new(
            AssemblyName: "Demo.Host",
            ComponentId: componentId,
            ModuleId: moduleId,
            ComponentName: componentName,
            RouteTemplates: [],
            RelativeModulePath: relativeModulePath,
            SourceMapPath: string.IsNullOrWhiteSpace(sourceMapPath) ? relativeModulePath + ".map" : sourceMapPath,
            OriginMapPath: relativeModulePath + ".origins.json",
            Imports: ["vue"],
            Styles: ["vuetify/styles"],
            PluginRequirements: ["vuetify"],
            DescriptorHash: descriptorHash,
            TemplateHash: templateHash,
            LogicHash: logicHash,
            ContentHash: contentHash,
            HmrBoundaryKind: boundaryKind,
            RequiresHydration: false,
            SupportsSsr: true,
            StyleHash: styleHash);
}
