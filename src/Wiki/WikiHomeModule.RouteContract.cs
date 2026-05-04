using System;
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    internal static readonly string[] PagePaths =
    [
        OverviewPath,
        GettingStartedPath,
        ContentModelPath,
        NavigationDiscoveryPath,
        InformationArchitecturePath,
        HFunctionAuthoringPath,
        CompilerBoundaryPath,
        RouteCatalogContractPath,
        HostSemanticSeamsPath,
        ImportEmitContractPath,
        RuntimeCatalogPath,
        ContentGovernancePath,
        DeploymentPath,
        TestingVerificationPath
    ];

    internal static readonly string[] PageGroups =
    [
        "Foundation",
        "Foundation",
        "Foundation",
        "Foundation",
        "Foundation",
        "Engineering",
        "Engineering",
        "Engineering",
        "Engineering",
        "Engineering",
        "Engineering",
        "Operations",
        "Operations",
        "Operations"
    ];

    internal static readonly string[] PageTitles =
    [
        "Overview",
        "Getting Started",
        "Content Model",
        "Navigation and Discovery",
        "Information Architecture",
        "H-Function Authoring",
        "Compiler Support Boundary",
        "Route Catalog Contract",
        "Host Semantic Seams",
        "Import and Emit Contract",
        "CLR Runtime Catalog",
        "Content Governance",
        "Deployment",
        "Testing and Verification"
    ];

    internal static readonly string[] PageSummaries =
    [
        "A production-oriented docs shell for Jazor, authored entirely with ECMAScript.Vue3 H functions.",
        "Run the site locally, understand the route model, and validate the emitted Wiki host end to end.",
        "Code-first page metadata, explicit sections, and a navigation contract that stays readable in C#.",
        "How readers move through the docs shell with grouped navigation, section TOCs, related pages, and not-found recovery.",
        "How routes, concern groups, page order, and naming rules keep the docs surface coherent as it grows.",
        "Why H functions are the production authoring surface for this Wiki, and the conventions that keep it maintainable.",
        "The active compiler contract for controlled input, usage-site validation, semantic erasure, and explicit failure boundaries.",
        "Why `WikiHomeModule.RouteContract.cs` is the single registration surface for route metadata, body dispatch, TOC anchors, and adjacent-page flow.",
        "How WhiteList, Alias, Inline, Import, and Compile divide responsibility across the supported host semantic surface.",
        "The stable boundary between import discovery, module AST assembly, generated catalogs, and host-facing file materialization.",
        "How CLR import helpers become browser-ready `System/*` runtime modules, and what guarantees keep that catalog safe to ship.",
        "How code-first docs content is owned, edited, reviewed, and released without drifting away from the emitted product shell.",
        "Build outputs, fallback routing, smoke verification, and the static delivery contract for Wiki.",
        "How compiler, emit, and operational smoke checks fit together to protect the production docs surface."
    ];

    internal static readonly string[] PageStatuses =
    [
        "Real Project MVP",
        "Foundation",
        "Authoring",
        "Discovery",
        "IA",
        "Engineering",
        "Boundary",
        "Catalog",
        "Host Seam",
        "Pipeline",
        "Runtime",
        "Governance",
        "Operations",
        "Verification"
    ];

    internal static readonly Func<IVNode>[] PageBodies =
    [
        OverviewBody,
        GettingStartedBody,
        ContentModelBody,
        NavigationDiscoveryBody,
        InformationArchitectureBody,
        HFunctionAuthoringBody,
        CompilerBoundaryBody,
        RouteCatalogContractBody,
        HostSemanticSeamsBody,
        ImportEmitContractBody,
        RuntimeCatalogBody,
        ContentGovernanceBody,
        DeploymentBody,
        TestingVerificationBody
    ];

    private static readonly string[] OverviewSectionIds =
    [
        "what-ships-now",
        "why-this-exists",
        "mvp-boundary",
        "site-structure",
        "registered-pages"
    ];

    private static readonly string[] OverviewSectionTitles =
    [
        "What ships now",
        "Why this exists",
        "MVP boundary",
        "Site structure",
        "Registered pages"
    ];

    private static readonly string[] GettingStartedSectionIds =
    [
        "boot-the-site",
        "route-model",
        "add-a-page",
        "verify-the-result"
    ];

    private static readonly string[] GettingStartedSectionTitles =
    [
        "Boot the site",
        "Route model",
        "Add a page",
        "Verify the result"
    ];

    private static readonly string[] ContentModelSectionIds =
    [
        "page-contract",
        "navigation-contract",
        "editing-rules"
    ];

    private static readonly string[] ContentModelSectionTitles =
    [
        "Page contract",
        "Navigation contract",
        "Editing rules"
    ];

    private static readonly string[] HFunctionAuthoringSectionIds =
    [
        "layout-composition",
        "production-rules",
        "why-this-works"
    ];

    private static readonly string[] HFunctionAuthoringSectionTitles =
    [
        "Layout composition",
        "Production rules",
        "Why this works"
    ];

    private static readonly string[] NavigationDiscoverySectionIds =
    [
        "left-rail",
        "right-rail",
        "related-pages",
        "not-found-recovery",
        "authoring-implications"
    ];

    private static readonly string[] NavigationDiscoverySectionTitles =
    [
        "Left rail discovery",
        "Right rail navigation",
        "Related pages and reading flow",
        "Not-found recovery",
        "Authoring implications"
    ];

    private static readonly string[] InformationArchitectureSectionIds =
    [
        "concern-groups",
        "route-shape",
        "naming-rules",
        "ordering-rules",
        "growth-without-drift"
    ];

    private static readonly string[] InformationArchitectureSectionTitles =
    [
        "Concern groups",
        "Route shape",
        "Naming rules",
        "Ordering and reading flow",
        "Growth without drift"
    ];

    private static readonly string[] CompilerBoundarySectionIds =
    [
        "controlled-domain",
        "behavior-priority",
        "support-boundary",
        "stabilized-routes",
        "practical-reading"
    ];

    private static readonly string[] CompilerBoundarySectionTitles =
    [
        "Controlled input domain",
        "Behavior priority",
        "Support boundary",
        "Stabilized semantic routes",
        "Practical reading order"
    ];

    private static readonly string[] RouteCatalogContractSectionIds =
    [
        "single-source",
        "what-the-catalog-owns",
        "safe-change-flow",
        "failure-modes",
        "verification-contract"
    ];

    private static readonly string[] RouteCatalogContractSectionTitles =
    [
        "Single source of truth",
        "What the catalog owns",
        "Safe change flow",
        "Failure modes to avoid",
        "Verification contract"
    ];

    private static readonly string[] HostSemanticSeamsSectionIds =
    [
        "why-seams-exist",
        "choose-the-right-seam",
        "whitelist-contract",
        "inline-vs-compile",
        "verification-surface"
    ];

    private static readonly string[] HostSemanticSeamsSectionTitles =
    [
        "Why seams exist",
        "Choose the right seam",
        "WhiteList contract",
        "Inline versus Compile",
        "Verification surface"
    ];

    private static readonly string[] ImportEmitContractSectionIds =
    [
        "boundary-split",
        "import-mainline",
        "layered-output",
        "host-materialization",
        "verification-signals"
    ];

    private static readonly string[] ImportEmitContractSectionTitles =
    [
        "Boundary split",
        "Import mainline",
        "Layered output contract",
        "Host materialization",
        "Verification signals"
    ];

    private static readonly string[] DeploymentSectionIds =
    [
        "build-output",
        "route-fallback",
        "operational-checks"
    ];

    private static readonly string[] DeploymentSectionTitles =
    [
        "Build output",
        "Route fallback",
        "Operational checks"
    ];

    private static readonly string[] ContentGovernanceSectionIds =
    [
        "ownership-model",
        "source-boundaries",
        "generated-assets",
        "change-flow",
        "release-discipline"
    ];

    private static readonly string[] ContentGovernanceSectionTitles =
    [
        "Ownership model",
        "Source boundaries",
        "Generated assets",
        "Safe change flow",
        "Release discipline"
    ];

    private static readonly string[] TestingVerificationSectionIds =
    [
        "verification-layers",
        "focused-commands",
        "coverage-and-determinism",
        "wiki-release-gate"
    ];

    private static readonly string[] TestingVerificationSectionTitles =
    [
        "Verification layers",
        "Focused commands",
        "Coverage and determinism",
        "Wiki release gate"
    ];

    private static readonly string[] RuntimeCatalogSectionIds =
    [
        "why-catalog-exists",
        "generation-pipeline",
        "runtime-contract",
        "operational-guardrails"
    ];

    private static readonly string[] RuntimeCatalogSectionTitles =
    [
        "Why the catalog exists",
        "Generation pipeline",
        "Runtime contract",
        "Operational guardrails"
    ];

    internal static readonly string[][] PageSectionIdSets =
    [
        OverviewSectionIds,
        GettingStartedSectionIds,
        ContentModelSectionIds,
        NavigationDiscoverySectionIds,
        InformationArchitectureSectionIds,
        HFunctionAuthoringSectionIds,
        CompilerBoundarySectionIds,
        RouteCatalogContractSectionIds,
        HostSemanticSeamsSectionIds,
        ImportEmitContractSectionIds,
        RuntimeCatalogSectionIds,
        ContentGovernanceSectionIds,
        DeploymentSectionIds,
        TestingVerificationSectionIds
    ];

    internal static readonly string[][] PageSectionTitleSets =
    [
        OverviewSectionTitles,
        GettingStartedSectionTitles,
        ContentModelSectionTitles,
        NavigationDiscoverySectionTitles,
        InformationArchitectureSectionTitles,
        HFunctionAuthoringSectionTitles,
        CompilerBoundarySectionTitles,
        RouteCatalogContractSectionTitles,
        HostSemanticSeamsSectionTitles,
        ImportEmitContractSectionTitles,
        RuntimeCatalogSectionTitles,
        ContentGovernanceSectionTitles,
        DeploymentSectionTitles,
        TestingVerificationSectionTitles
    ];

    internal static readonly string[][] PageRelatedPathSets =
    [
        [GettingStartedPath, NavigationDiscoveryPath, InformationArchitecturePath],
        [NavigationDiscoveryPath, ContentModelPath, DeploymentPath],
        [NavigationDiscoveryPath, InformationArchitecturePath, OverviewPath],
        [InformationArchitecturePath, ContentModelPath, RouteCatalogContractPath],
        [NavigationDiscoveryPath, RouteCatalogContractPath, ContentModelPath],
        [CompilerBoundaryPath, RouteCatalogContractPath, ContentModelPath],
        [RouteCatalogContractPath, HostSemanticSeamsPath, InformationArchitecturePath],
        [HostSemanticSeamsPath, ImportEmitContractPath, InformationArchitecturePath],
        [ImportEmitContractPath, ContentGovernancePath, RouteCatalogContractPath],
        [InformationArchitecturePath, ContentGovernancePath, DeploymentPath],
        [ImportEmitContractPath, ContentGovernancePath, DeploymentPath],
        [RuntimeCatalogPath, TestingVerificationPath, ImportEmitContractPath],
        [ContentGovernancePath, TestingVerificationPath, ImportEmitContractPath],
        [ContentGovernancePath, DeploymentPath, RuntimeCatalogPath]
    ];

    private static int TotalPageCount => PagePaths.Length;

    private static bool IsKnownPage(string currentPath)
        => GetPageIndex(currentPath) >= 0;

    private static int GetPageIndex(string currentPath)
    {
        for (var pageIndex = 0; pageIndex < PagePaths.Length; pageIndex++)
        {
            if (PagePaths[pageIndex] == currentPath)
                return pageIndex;
        }

        return -1;
    }

    private static string GetPagePath(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < PagePaths.Length)
            return PagePaths[pageIndex];

        return "";
    }

    private static bool MatchesPageFilter(string currentPath, string filterText)
    {
        if (filterText.Length == 0)
            return true;

        return currentPath.Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
               GetPageGroup(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
               GetPageTitle(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
               GetPageSummary(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
               GetPageStatus(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetPageGroup(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageGroups.Length)
            return PageGroups[pageIndex];

        return "Unregistered";
    }

    private static string GetPageTitle(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageTitles.Length)
            return PageTitles[pageIndex];

        return "Unregistered page";
    }

    private static string GetPageSummary(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageSummaries.Length)
            return PageSummaries[pageIndex];

        return "The requested path is not part of the registered Wiki page catalog.";
    }

    private static string GetPageStatus(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageStatuses.Length)
            return PageStatuses[pageIndex];

        return "Not Found";
    }

    private static string GetPreviousPath(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex <= 0)
            return "";

        return GetPagePath(pageIndex - 1);
    }

    private static string GetNextPath(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex < 0 || pageIndex >= TotalPageCount - 1)
            return "";

        return GetPagePath(pageIndex + 1);
    }

    private static string[] GetPageSectionIds(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < PageSectionIdSets.Length)
            return PageSectionIdSets[pageIndex];

        return [];
    }

    private static string[] GetPageSectionTitles(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < PageSectionTitleSets.Length)
            return PageSectionTitleSets[pageIndex];

        return [];
    }

    private static IVNode GetPageBody(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < PageBodies.Length)
            return PageBodies[pageIndex]();

        return H("div", new VueObject { Class = "doc-body" }, []);
    }

    private static string[] GetPageRelatedPaths(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < PageRelatedPathSets.Length)
            return PageRelatedPathSets[pageIndex];

        return [];
    }

    private static string[] GetSuggestedPaths(string currentPath)
    {
        var fragment = GetRouteFragment(currentPath);
        var suggestions = new List<string>();

        if (fragment.Length > 0)
        {
            for (var pageIndex = 0; pageIndex < PagePaths.Length; pageIndex++)
            {
                var pagePath = PagePaths[pageIndex];
                if (MatchesPageFilter(pagePath, fragment))
                    suggestions.Add(pagePath);
            }
        }

        if (suggestions.Count == 0)
        {
            var requestedGroup = GetRequestedGroup(currentPath);
            if (requestedGroup.Length > 0)
            {
                for (var pageIndex = 0; pageIndex < PagePaths.Length; pageIndex++)
                {
                    var pagePath = PagePaths[pageIndex];
                    if (GetPageGroup(pagePath) == requestedGroup)
                        suggestions.Add(pagePath);
                }
            }
        }

        if (suggestions.Count == 0)
            return [OverviewPath, GettingStartedPath, DeploymentPath];

        if (suggestions.Count > 3)
            suggestions.RemoveRange(3, suggestions.Count - 3);

        return suggestions.ToArray();
    }

    private static string GetRequestedGroup(string currentPath)
    {
        if (currentPath.StartsWith("/guides/", StringComparison.OrdinalIgnoreCase))
            return "Foundation";

        if (currentPath.StartsWith("/engineering/", StringComparison.OrdinalIgnoreCase))
            return "Engineering";

        if (currentPath.StartsWith("/operations/", StringComparison.OrdinalIgnoreCase))
            return "Operations";

        return "";
    }

    private static string GetRouteFragment(string currentPath)
    {
        var normalizedPath = currentPath.Trim('/');
        if (normalizedPath.Length == 0)
            return "";

        var lastSlashIndex = normalizedPath.LastIndexOf('/');
        if (lastSlashIndex >= 0 && lastSlashIndex < normalizedPath.Length - 1)
            return normalizedPath.Substring(lastSlashIndex + 1);

        return normalizedPath;
    }

    private static IVNode TocRail(string currentPath, string currentHash)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex < 0)
            return EmptyTocRail();

        var sectionIds = GetPageSectionIds(pageIndex);
        var sectionTitles = GetPageSectionTitles(pageIndex);
        var links = new List<IVNode>();
        for (var sectionIndex = 0; sectionIndex < sectionIds.Length && sectionIndex < sectionTitles.Length; sectionIndex++)
            links.Add(TocLink(currentPath, sectionIds[sectionIndex], sectionTitles[sectionIndex], currentHash));

        return TocRail("On this page", links.ToArray());
    }

}
