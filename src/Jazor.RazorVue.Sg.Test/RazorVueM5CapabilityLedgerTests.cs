namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueM5CapabilityLedgerTests
{
    [TestMethod]
    public void Ledger_CoversEveryPriorityWithoutUndeclaredStates()
    {
        var entries = RazorVueM5CapabilityLedger.All;

        Assert.IsTrue(entries.Count >= 20, "M5 needs an explicit decision for every major Blazor authoring family.");
        Assert.HasCount(entries.Count, entries.Select(static entry => entry.Id).Distinct(StringComparer.Ordinal));
        foreach (var priority in Enum.GetValues<RazorVueCapabilityPriority>())
            Assert.IsTrue(entries.Any(entry => entry.Priority == priority), priority.ToString());

        Assert.IsTrue(entries.All(static entry =>
            !string.IsNullOrWhiteSpace(entry.AuthoringShape) &&
            !string.IsNullOrWhiteSpace(entry.Owner) &&
            !string.IsNullOrWhiteSpace(entry.Fixture) &&
            !string.IsNullOrWhiteSpace(entry.Blocker)));
        Assert.IsTrue(entries
            .Where(static entry => entry.Status is RazorVueCapabilityStatus.Support or RazorVueCapabilityStatus.InProof)
            .All(static entry => entry.Evidence != RazorVueCapabilityEvidence.None));
        Assert.IsTrue(entries
            .Where(static entry => entry.Decision == RazorVueCapabilityDecision.Reject)
            .All(static entry => !string.IsNullOrWhiteSpace(entry.DiagnosticId)));
    }

    [TestMethod]
    public void Ledger_MapsCurrentSourceDiagnosticAndFinalOwnershipWithoutDuplication()
    {
        var serverOnlyInjection = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P1-server-only-injection");
        var parameterView = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P1-parameter-view");
        var parameterViewMembers = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P1-parameter-view-unsupported-members");

        Assert.AreEqual("JAZORVCA001", serverOnlyInjection.DiagnosticId);
        Assert.AreEqual(RazorVueCapabilityStatus.Reject, serverOnlyInjection.Status);
        var serverOnlyAspNetInjection = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P1-server-only-aspnet-injection");
        Assert.AreEqual("JAZORVCA002", serverOnlyAspNetInjection.DiagnosticId);
        Assert.AreEqual(RazorVueCapabilityStatus.Reject, serverOnlyAspNetInjection.Status);
        Assert.IsNull(parameterView.DiagnosticId);
        Assert.AreEqual(RazorVueCapabilityDecision.CompatibilityAdapter, parameterView.Decision);
        Assert.AreEqual(RazorVueCapabilityStatus.InProof, parameterView.Status);
        Assert.AreEqual("JAZORVCA003/JAZORVCA004/JAZORVCA005", parameterViewMembers.DiagnosticId);
        Assert.AreEqual(RazorVueCapabilityStatus.Guidance, parameterViewMembers.Status);

        var browserServiceInjection = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P1-browser-service-injection");
        Assert.AreEqual(RazorVueCapabilityStatus.InProof, browserServiceInjection.Status);
        Assert.AreEqual(RazorVueCapabilityDecision.CompatibilityAdapter, browserServiceInjection.Decision);
        Assert.IsTrue(browserServiceInjection.Evidence.HasFlag(RazorVueCapabilityEvidence.OfficialRazorSourceGenerator));
        Assert.IsTrue(browserServiceInjection.Evidence.HasFlag(RazorVueCapabilityEvidence.DenoRuntime));

        var propertyShape = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P1-injection-property-shape");
        Assert.AreEqual("JAZORVCA006", propertyShape.DiagnosticId);

        var hostServiceGap = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P1-known-host-service-adapter-gap");
        Assert.AreEqual("JAZORVCA007", hostServiceGap.DiagnosticId);
        Assert.AreEqual(RazorVueCapabilityStatus.Guidance, hostServiceGap.Status);

        var cascading = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P1-cascading-values");
        Assert.AreEqual("JAZORVCA008", cascading.DiagnosticId);
        Assert.AreEqual(RazorVueCapabilityDecision.CompatibilityAdapter, cascading.Decision);
        Assert.AreEqual(RazorVueCapabilityStatus.InProof, cascading.Status);
        Assert.IsTrue(cascading.Evidence.HasFlag(RazorVueCapabilityEvidence.AuthorSource));
        Assert.IsTrue(cascading.Evidence.HasFlag(RazorVueCapabilityEvidence.OfficialRazorSourceGenerator));
        Assert.IsTrue(cascading.Evidence.HasFlag(RazorVueCapabilityEvidence.ModuleArtifact));
        Assert.IsTrue(cascading.Evidence.HasFlag(RazorVueCapabilityEvidence.DenoRuntime));

        var route = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P0-route-layout-page-state");
        Assert.IsNull(route.DiagnosticId);
        Assert.AreEqual(RazorVueCapabilityStatus.InProof, route.Status);
        Assert.IsTrue(route.Evidence.HasFlag(RazorVueCapabilityEvidence.ModuleArtifact));

        var standardComponents = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P1-standard-blazor-component-adapters");
        Assert.IsNull(standardComponents.DiagnosticId);
        Assert.AreEqual(RazorVueCapabilityStatus.InProof, standardComponents.Status);
        Assert.IsTrue(standardComponents.Evidence.HasFlag(RazorVueCapabilityEvidence.DenoRuntime));
    }
}
