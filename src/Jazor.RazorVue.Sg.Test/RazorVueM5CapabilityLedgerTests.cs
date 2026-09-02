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
        Assert.IsTrue(parameterView.Evidence.HasFlag(RazorVueCapabilityEvidence.BrowserSmoke));
        Assert.IsTrue(parameterView.Evidence.HasFlag(RazorVueCapabilityEvidence.PackageConsumer));
        Assert.AreEqual("JAZORVCA003/JAZORVCA004/JAZORVCA005", parameterViewMembers.DiagnosticId);
        Assert.AreEqual(RazorVueCapabilityStatus.Guidance, parameterViewMembers.Status);

        var complexLifecycle = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P1-complex-lifecycle");
        Assert.AreEqual(RazorVueCapabilityDecision.CompatibilityAdapter, complexLifecycle.Decision);
        Assert.AreEqual(RazorVueCapabilityStatus.InProof, complexLifecycle.Status);
        Assert.IsTrue(complexLifecycle.Evidence.HasFlag(RazorVueCapabilityEvidence.AuthorSource));
        Assert.IsTrue(complexLifecycle.Evidence.HasFlag(RazorVueCapabilityEvidence.OfficialRazorSourceGenerator));
        Assert.IsTrue(complexLifecycle.Evidence.HasFlag(RazorVueCapabilityEvidence.ModuleArtifact));
        Assert.IsTrue(complexLifecycle.Evidence.HasFlag(RazorVueCapabilityEvidence.DenoRuntime));
        Assert.IsTrue(complexLifecycle.Evidence.HasFlag(RazorVueCapabilityEvidence.BrowserSmoke));
        Assert.IsTrue(complexLifecycle.Evidence.HasFlag(RazorVueCapabilityEvidence.PackageConsumer));
        StringAssert.Contains(complexLifecycle.Fixture, "AsyncInitializationFailureReachesNextRender", StringComparison.Ordinal);
        StringAssert.Contains(complexLifecycle.Fixture, "CanceledParameterLifecycleAfterUnmountDoesNotInvalidate", StringComparison.Ordinal);
        StringAssert.Contains(complexLifecycle.Fixture, "QueuedParameterLifecycleDoesNotStartAfterUnmount", StringComparison.Ordinal);
        StringAssert.Contains(complexLifecycle.Fixture, "StaleParameterLifecycleFailureStillReachesNextRender", StringComparison.Ordinal);
        StringAssert.Contains(complexLifecycle.Fixture, "RepeatedRenderDoesNotRepeatAfterRenderAsyncHook", StringComparison.Ordinal);
        StringAssert.Contains(complexLifecycle.Fixture, "AsyncLifecycleCompletionAfterAsyncUnmountIsIgnored", StringComparison.Ordinal);
        StringAssert.Contains(complexLifecycle.Fixture, "ProvesAsyncRacesInRealBrowser", StringComparison.Ordinal);
        StringAssert.Contains(complexLifecycle.Blocker, "SSR/prerender", StringComparison.Ordinal);

        var browserServiceInjection = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P1-browser-service-injection");
        Assert.AreEqual(RazorVueCapabilityStatus.InProof, browserServiceInjection.Status);
        Assert.AreEqual(RazorVueCapabilityDecision.CompatibilityAdapter, browserServiceInjection.Decision);
        Assert.IsTrue(browserServiceInjection.Evidence.HasFlag(RazorVueCapabilityEvidence.OfficialRazorSourceGenerator));
        Assert.IsTrue(browserServiceInjection.Evidence.HasFlag(RazorVueCapabilityEvidence.DenoRuntime));
        Assert.IsTrue(browserServiceInjection.Evidence.HasFlag(RazorVueCapabilityEvidence.BrowserSmoke));
        Assert.IsTrue(browserServiceInjection.Evidence.HasFlag(RazorVueCapabilityEvidence.PackageConsumer));

        var propertyShape = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P1-injection-property-shape");
        Assert.AreEqual("JAZORVCA006", propertyShape.DiagnosticId);

        var hostServiceGap = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P1-known-host-service-adapter-gap");
        Assert.AreEqual("JAZORVCA007", hostServiceGap.DiagnosticId);
        Assert.AreEqual(RazorVueCapabilityStatus.Guidance, hostServiceGap.Status);

        var jsInterop = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P2-js-runtime");
        Assert.AreEqual("JAZORVGA022", jsInterop.DiagnosticId);
        Assert.AreEqual(RazorVueCapabilityDecision.Reject, jsInterop.Decision);
        Assert.AreEqual(RazorVueCapabilityStatus.Reject, jsInterop.Status);
        Assert.IsTrue(jsInterop.Evidence.HasFlag(RazorVueCapabilityEvidence.AuthorSource));
        Assert.IsTrue(jsInterop.Evidence.HasFlag(RazorVueCapabilityEvidence.OfficialRazorSourceGenerator));

        var authentication = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P2-authentication");
        Assert.AreEqual("JAZORVCA007", authentication.DiagnosticId);
        Assert.AreEqual(RazorVueCapabilityDecision.GuidedAdaptation, authentication.Decision);
        Assert.AreEqual(RazorVueCapabilityStatus.Guidance, authentication.Status);
        Assert.IsTrue(authentication.Evidence.HasFlag(RazorVueCapabilityEvidence.AuthorSource));
        StringAssert.Contains(authentication.Blocker, "versioned browser provider", StringComparison.Ordinal);

        var cascading = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P1-cascading-values");
        Assert.AreEqual("JAZORVCA008", cascading.DiagnosticId);
        Assert.AreEqual(RazorVueCapabilityDecision.CompatibilityAdapter, cascading.Decision);
        Assert.AreEqual(RazorVueCapabilityStatus.InProof, cascading.Status);
        Assert.IsTrue(cascading.Evidence.HasFlag(RazorVueCapabilityEvidence.AuthorSource));
        Assert.IsTrue(cascading.Evidence.HasFlag(RazorVueCapabilityEvidence.OfficialRazorSourceGenerator));
        Assert.IsTrue(cascading.Evidence.HasFlag(RazorVueCapabilityEvidence.ModuleArtifact));
        Assert.IsTrue(cascading.Evidence.HasFlag(RazorVueCapabilityEvidence.DenoRuntime));
        Assert.IsTrue(cascading.Evidence.HasFlag(RazorVueCapabilityEvidence.BrowserSmoke));
        Assert.IsTrue(cascading.Evidence.HasFlag(RazorVueCapabilityEvidence.PackageConsumer));

        var route = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P0-route-layout-page-state");
        Assert.IsNull(route.DiagnosticId);
        Assert.AreEqual(RazorVueCapabilityStatus.Support, route.Status);
        Assert.IsTrue(route.Evidence.HasFlag(RazorVueCapabilityEvidence.AuthorSource));
        Assert.IsTrue(route.Evidence.HasFlag(RazorVueCapabilityEvidence.OfficialRazorSourceGenerator));
        Assert.IsTrue(route.Evidence.HasFlag(RazorVueCapabilityEvidence.ModuleArtifact));
        Assert.IsTrue(route.Evidence.HasFlag(RazorVueCapabilityEvidence.DenoRuntime));
        Assert.IsTrue(route.Evidence.HasFlag(RazorVueCapabilityEvidence.BrowserSmoke));
        Assert.IsTrue(route.Evidence.HasFlag(RazorVueCapabilityEvidence.PackageConsumer));
        StringAssert.Contains(route.Fixture, "samples/RazorVue.Authoring/verify-smoke.cs", StringComparison.Ordinal);
        StringAssert.Contains(route.ExcludedSurface, "LocationChanging", StringComparison.Ordinal);

        var tdesign = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P0-tdesign-typed-authoring");
        Assert.AreEqual(RazorVueCapabilityDecision.DirectSupport, tdesign.Decision);
        Assert.AreEqual(RazorVueCapabilityStatus.Support, tdesign.Status);
        Assert.IsTrue(tdesign.Evidence.HasFlag(RazorVueCapabilityEvidence.AuthorSource));
        Assert.IsTrue(tdesign.Evidence.HasFlag(RazorVueCapabilityEvidence.OfficialRazorSourceGenerator));
        Assert.IsTrue(tdesign.Evidence.HasFlag(RazorVueCapabilityEvidence.ModuleArtifact));
        Assert.IsTrue(tdesign.Evidence.HasFlag(RazorVueCapabilityEvidence.DenoRuntime));
        Assert.IsTrue(tdesign.Evidence.HasFlag(RazorVueCapabilityEvidence.BrowserSmoke));
        Assert.IsTrue(tdesign.Evidence.HasFlag(RazorVueCapabilityEvidence.PackageConsumer));

        var coreDomEvents = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P1-blazor-clr-core-dom-events");
        Assert.AreEqual(RazorVueCapabilityDecision.CompatibilityAdapter, coreDomEvents.Decision);
        Assert.AreEqual(RazorVueCapabilityStatus.Support, coreDomEvents.Status);
        Assert.IsTrue(coreDomEvents.Evidence.HasFlag(RazorVueCapabilityEvidence.AuthorSource));
        Assert.IsTrue(coreDomEvents.Evidence.HasFlag(RazorVueCapabilityEvidence.OfficialRazorSourceGenerator));
        Assert.IsTrue(coreDomEvents.Evidence.HasFlag(RazorVueCapabilityEvidence.ModuleArtifact));
        Assert.IsTrue(coreDomEvents.Evidence.HasFlag(RazorVueCapabilityEvidence.DenoRuntime));
        Assert.IsTrue(coreDomEvents.Evidence.HasFlag(RazorVueCapabilityEvidence.BrowserSmoke));
        Assert.IsTrue(coreDomEvents.Evidence.HasFlag(RazorVueCapabilityEvidence.PackageConsumer));
        StringAssert.Contains(coreDomEvents.ImplementationPath, "typed onchange capture wrapper", StringComparison.Ordinal);

        var elementReference = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P1-blazor-clr-element-reference");
        Assert.AreEqual(RazorVueCapabilityDecision.DirectSupport, elementReference.Decision);
        Assert.AreEqual(RazorVueCapabilityStatus.Support, elementReference.Status);
        Assert.IsTrue(elementReference.Evidence.HasFlag(RazorVueCapabilityEvidence.AuthorSource));
        Assert.IsTrue(elementReference.Evidence.HasFlag(RazorVueCapabilityEvidence.OfficialRazorSourceGenerator));
        Assert.IsTrue(elementReference.Evidence.HasFlag(RazorVueCapabilityEvidence.ModuleArtifact));
        Assert.IsTrue(elementReference.Evidence.HasFlag(RazorVueCapabilityEvidence.DenoRuntime));
        Assert.IsTrue(elementReference.Evidence.HasFlag(RazorVueCapabilityEvidence.BrowserSmoke));
        Assert.IsTrue(elementReference.Evidence.HasFlag(RazorVueCapabilityEvidence.PackageConsumer));
        StringAssert.Contains(elementReference.ImplementationPath, "Import(ElementReferenceExtensions focus helper)", StringComparison.Ordinal);

        var standardComponents = RazorVueM5CapabilityLedger.All.Single(static entry =>
            entry.Id == "P1-standard-blazor-component-adapters");
        Assert.AreEqual("JAZORVGA021", standardComponents.DiagnosticId);
        Assert.AreEqual(RazorVueCapabilityDecision.Reject, standardComponents.Decision);
        Assert.AreEqual(RazorVueCapabilityStatus.Reject, standardComponents.Status);
        Assert.IsTrue(standardComponents.Evidence.HasFlag(RazorVueCapabilityEvidence.OfficialRazorSourceGenerator));

        foreach (var id in new[] { "P1-dynamic-component", "P1-error-boundary", "P1-standard-forms" })
        {
            var entry = RazorVueM5CapabilityLedger.All.Single(candidate => candidate.Id == id);
            Assert.AreEqual(RazorVueCapabilityDecision.Reject, entry.Decision, id);
            Assert.AreEqual(RazorVueCapabilityStatus.Reject, entry.Status, id);
            Assert.AreEqual("JAZORVGA021", entry.DiagnosticId, id);
        }
    }

    [TestMethod]
    public void Ledger_BlazorClrSlicesDeclareAuditableContractMetadata()
    {
        var entries = RazorVueM5CapabilityLedger.All
            .Where(static entry => entry.Id.Contains("blazor-clr", StringComparison.Ordinal))
            .ToArray();

        Assert.HasCount(11, entries);
        Assert.IsTrue(entries.All(static entry =>
            !string.IsNullOrWhiteSpace(entry.TargetProfiles) &&
            !string.IsNullOrWhiteSpace(entry.Carrier) &&
            !string.IsNullOrWhiteSpace(entry.ImplementationPath) &&
            !string.IsNullOrWhiteSpace(entry.ContributionContractVersion) &&
            !string.IsNullOrWhiteSpace(entry.Dependencies) &&
            !string.IsNullOrWhiteSpace(entry.ExcludedSurface)));
        Assert.IsTrue(entries.All(static entry =>
            entry.ImplementationPath.Contains("Jazor.CLR", StringComparison.Ordinal)));
        Assert.IsTrue(entries.All(static entry =>
            entry.ContributionContractVersion == "generated-clr-module/v1"));
        Assert.IsFalse(entries.Any(static entry =>
            entry.ImplementationPath.Contains("source-root", StringComparison.Ordinal)));

        var package = entries.Single(static entry => entry.Id == "P0-blazor-clr-mapping-package");
        Assert.AreEqual(RazorVueCapabilityStatus.InProof, package.Status);
        Assert.IsTrue(package.Evidence.HasFlag(RazorVueCapabilityEvidence.PackageConsumer));

        foreach (var id in new[]
        {
            "P2-blazor-clr-pointer-events",
            "P2-blazor-clr-wheel-events",
            "P2-blazor-clr-drag-events",
            "P2-blazor-clr-clipboard-events",
            "P2-blazor-clr-touch-events",
            "P2-blazor-clr-error-events",
            "P2-blazor-clr-progress-events"
        })
        {
            var eventSlice = entries.Single(entry => entry.Id == id);
            Assert.AreEqual(RazorVueCapabilityStatus.Support, eventSlice.Status, id);
            Assert.IsTrue(eventSlice.Evidence.HasFlag(RazorVueCapabilityEvidence.OfficialRazorSourceGenerator), id);
            Assert.IsTrue(eventSlice.Evidence.HasFlag(RazorVueCapabilityEvidence.DenoRuntime), id);
            Assert.IsTrue(eventSlice.Evidence.HasFlag(RazorVueCapabilityEvidence.BrowserSmoke), id);
            Assert.IsTrue(eventSlice.Evidence.HasFlag(RazorVueCapabilityEvidence.PackageConsumer), id);
        }

        Assert.IsFalse(entries.Any(static entry => entry.Id == "P2-blazor-clr-remaining-dom-events"));
    }
}
