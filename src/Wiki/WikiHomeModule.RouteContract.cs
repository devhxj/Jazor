using System;
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    internal static readonly string[] PagePaths =
    [
        OverviewPath,
        SearchPath,
        GettingStartedPath,
        ProjectLinesPath,
        ContentModelPath,
        NavigationDiscoveryPath,
        InformationArchitecturePath,
        TopicIndexPath,
        GlossaryPath,
        FaqPath,
        TroubleshootingPath,
        HFunctionAuthoringPath,
        CompilerOverviewPath,
        CompilerBoundaryPath,
        RouteCatalogContractPath,
        HostSemanticSeamsPath,
        ImportEmitContractPath,
        RuntimeCatalogPath,
        JoltHostPath,
        RazorVueLibraryModePath,
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
        "Foundation",
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
        "Search",
        "Getting Started",
        "Project Lines",
        "Content Model",
        "Navigation and Discovery",
        "Information Architecture",
        "Topic Index",
        "Glossary",
        "FAQ",
        "Troubleshooting",
        "H-Function Authoring",
        "Compiler Overview",
        "Compiler Support Boundary",
        "Route Catalog Contract",
        "Host Semantic Seams",
        "Import and Emit Contract",
        "CLR Runtime Catalog",
        "Jolt Host",
        "RazorVue Library Mode",
        "Content Governance",
        "Deployment",
        "Testing and Verification"
    ];

    internal static readonly string[] PageSummaries =
    [
        "A production-oriented docs shell for Jazor, authored entirely with ECMAScript.Vue3 H functions.",
        "URL-driven full-text search across route metadata, tags, curated page body text, and section titles.",
        "Run the site locally, understand the route model, and validate the emitted Wiki host end to end.",
        "Understand the two active Jazor lines, when to choose them, and which shared compiler foundations they consume.",
        "Code-first page metadata, explicit sections, and a navigation contract that stays readable in C#.",
        "How readers move through the docs shell with grouped navigation, section TOCs, related pages, and not-found recovery.",
        "How routes, concern groups, page order, and naming rules keep the docs surface coherent as it grows.",
        "Use a route-first index to jump into Jazor topics by concern instead of memorizing exact URLs.",
        "Shared vocabulary for compiler, runtime, host, and documentation terms used across the repository.",
        "Short answers to the questions that recur most often when contributors first touch Jazor or Wiki.",
        "Recover from the most common local Wiki, runtime-module, and compiler-boundary failures.",
        "Why H functions are the production authoring surface for this Wiki, and the conventions that keep it maintainable.",
        "A high-level view of the compiler pipeline, active contracts, and where to read deeper.",
        "The active compiler contract for controlled input, usage-site validation, semantic erasure, and explicit failure boundaries.",
        "Why `WikiHomeModule.RouteContract.cs` is the single registration surface for route metadata, body dispatch, TOC anchors, and adjacent-page flow.",
        "How WhiteList, Alias, Inline, Import, and Compile divide responsibility across the supported host semantic surface.",
        "The stable boundary between import discovery, module AST assembly, generated catalogs, and host-facing file materialization.",
        "How CLR import helpers become browser-ready `System/*` runtime modules, and what guarantees keep that catalog safe to ship.",
        "The full-featured `.jazor` development host for editing, preview, build, and debug workflows.",
        "The build-time library mode for compiling Razor components into JavaScript artifacts without a full development host.",
        "How code-first docs content is owned, edited, reviewed, and released without drifting away from the emitted product shell.",
        "Build outputs, fallback routing, smoke verification, and the static delivery contract for Wiki.",
        "How compiler, emit, and operational smoke checks fit together to protect the production docs surface."
    ];

    internal static readonly string[] PageStatuses =
    [
        "Real Project MVP",
        "Utility",
        "Foundation",
        "Orientation",
        "Authoring",
        "Discovery",
        "IA",
        "Taxonomy",
        "Reference",
        "Help",
        "Support",
        "Engineering",
        "Core",
        "Boundary",
        "Catalog",
        "Host Seam",
        "Pipeline",
        "Runtime",
        "Host",
        "Library",
        "Governance",
        "Operations",
        "Verification"
    ];

    internal static readonly string[] PageOwners =
    [
        "Docs",
        "Docs",
        "Docs",
        "Docs",
        "Docs",
        "Docs",
        "Docs",
        "Docs",
        "Docs",
        "Docs",
        "Docs",
        "Docs",
        "Compiler",
        "Compiler",
        "Docs",
        "Compiler",
        "Compiler",
        "CLR",
        "Jolt",
        "RazorVue",
        "Docs",
        "Ops",
        "Ops"
    ];

    internal static readonly string[] PageAudiences =
    [
        "All readers",
        "All readers",
        "New readers",
        "New readers",
        "Docs contributors",
        "Docs contributors",
        "Docs contributors",
        "All readers",
        "All readers",
        "All readers",
        "Contributors",
        "UI authors",
        "Compiler contributors",
        "Compiler contributors",
        "Docs contributors",
        "Compiler contributors",
        "Compiler contributors",
        "Runtime contributors",
        "App authors",
        "Library authors",
        "Maintainers",
        "Maintainers",
        "Maintainers"
    ];

    internal static readonly string[] PageSourceFiles =
    [
        "src/Wiki/WikiHomeModule.Overview.cs",
        "src/Wiki/WikiHomeModule.Search.cs",
        "src/Wiki/WikiHomeModule.GettingStarted.cs",
        "src/Wiki/WikiHomeModule.ProjectLines.cs",
        "src/Wiki/WikiHomeModule.ContentModel.cs",
        "src/Wiki/WikiHomeModule.NavigationDiscovery.cs",
        "src/Wiki/WikiHomeModule.InformationArchitecture.cs",
        "src/Wiki/WikiHomeModule.TopicIndex.cs",
        "src/Wiki/WikiHomeModule.Glossary.cs",
        "src/Wiki/WikiHomeModule.Faq.cs",
        "src/Wiki/WikiHomeModule.Troubleshooting.cs",
        "src/Wiki/WikiHomeModule.HFunctionAuthoring.cs",
        "src/Wiki/WikiHomeModule.CompilerOverview.cs",
        "src/Wiki/WikiHomeModule.CompilerBoundary.cs",
        "src/Wiki/WikiHomeModule.RouteCatalogContract.cs",
        "src/Wiki/WikiHomeModule.HostSemanticSeams.cs",
        "src/Wiki/WikiHomeModule.ImportEmitContract.cs",
        "src/Wiki/WikiHomeModule.RuntimeCatalog.cs",
        "src/Wiki/WikiHomeModule.JoltHost.cs",
        "src/Wiki/WikiHomeModule.RazorVueLibraryMode.cs",
        "src/Wiki/WikiHomeModule.ContentGovernance.cs",
        "src/Wiki/WikiHomeModule.Deployment.cs",
        "src/Wiki/WikiHomeModule.TestingVerification.cs"
    ];

    internal static readonly string[] PageLastUpdatedDates =
    [
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04"
    ];

    internal static readonly int[] PageReadingMinutes =
    [
        5,
        4,
        5,
        5,
        4,
        4,
        5,
        4,
        4,
        4,
        5,
        4,
        5,
        5,
        5,
        5,
        5,
        4,
        5,
        5,
        4,
        4,
        4
    ];

    internal static readonly string[] PageSearchBodies =
    [
        "Introduces jazor.wiki as a production docs shell built with Vue 3 H functions, route fallback, central catalog metadata, and browser-served runtime modules.",
        "Provides shareable full-text search across page metadata, tags, body summaries, and section titles, with result highlighting and section-level matches.",
        "Shows the local build, serve, and smoke loop; explains route structure, page registration, and how to verify emitted output after each change.",
        "Explains the two active lines, RazorVue library mode and Jolt full host, and how both depend on the shared compiler and emit core.",
        "Describes the explicit page contract, central route metadata, section ownership, and editing rules that keep code-first docs readable.",
        "Covers left-rail discovery, section TOCs, related pages, not-found recovery, and the authoring implications of discoverable docs.",
        "Defines concern groups, stable route shape, naming rules, ordering rules, and how the docs can grow without route drift.",
        "Collects the major topic clusters so readers can jump by concern into orientation, engineering contracts, or operations workflows.",
        "Defines common terms such as SemanticWalker, AstConverter, WhiteList, import maps, CLR catalog, Jolt, RazorVue, and smoke verification.",
        "Answers recurring contributor questions about choosing Jolt or RazorVue, analyzer versus compiler boundaries, runtime helper modules, and Wiki workflow.",
        "Provides fast recovery steps for route fallback failures, missing System modules, compiler diagnostics, and broken local verification loops.",
        "Explains why H functions are the production authoring surface, how layout helpers stay explicit, and what rules keep UI composition maintainable.",
        "Summarizes the compiler's purpose, the SemanticWalker to AstConverter pipeline, and the hard contracts around tuples, interfaces, imports, and explicit failure.",
        "Details the support boundary, behavior-priority order, stabilized semantic routes, and why unsupported runtime-sensitive behavior fails explicitly.",
        "Explains why route registration, TOC anchors, related links, and page order must all stay in one route contract instead of drifting across files.",
        "Shows how WhiteList, Alias, Inline, Import, and Compile divide host responsibility and how to choose the right seam for a mapping.",
        "Describes import discovery, module-header assembly, output layering, and how emit materializes stable browser files from compiler carriers.",
        "Explains why the CLR runtime catalog exists, how helpers become System modules, and which guardrails keep browser runtime imports shippable.",
        "Describes Jolt as the full-featured `.jazor` development host with Jazor, Roslyn, and Volar lanes, preview, HMR, build, and debug support.",
        "Describes RazorVue as the build-time library mode with split ownership across shared semantics, analyzer host logic, and library component bindings.",
        "Defines ownership, source boundaries, generated asset review, and the safe change flow for code-first docs pages.",
        "Describes local build output, route fallback, operational checks, and the static hosting contract for the Wiki shell.",
        "Explains verification layers, focused commands, coverage expectations, and why smoke verification is the minimum release gate."
    ];

    internal static readonly string[][] PageTagSets =
    [
        ["docs-shell", "routes", "overview"],
        ["search", "discovery", "query"],
        ["getting-started", "local-dev", "verification"],
        ["jolt", "razorvue", "architecture"],
        ["authoring", "metadata", "catalog"],
        ["navigation", "toc", "discovery"],
        ["information-architecture", "routes", "naming"],
        ["taxonomy", "index", "discovery"],
        ["glossary", "terms", "reference"],
        ["faq", "help", "workflow"],
        ["troubleshooting", "smoke", "runtime"],
        ["vue3", "h-function", "ui-authoring"],
        ["compiler", "estree", "semanticwalker"],
        ["compiler", "boundary", "lowering"],
        ["catalog", "routes", "metadata"],
        ["whitelist", "inline", "compile"],
        ["imports", "emit", "modules"],
        ["clr", "runtime", "system-modules"],
        ["jolt", "dev-host", "lsp"],
        ["razorvue", "library-mode", "build"],
        ["ownership", "docs", "review"],
        ["hosting", "static-files", "fallback"],
        ["smoke", "tests", "verification"]
    ];

    internal static readonly Func<IVNode>[] PageBodies =
    [
        OverviewBody,
        SearchBody,
        GettingStartedBody,
        ProjectLinesBody,
        ContentModelBody,
        NavigationDiscoveryBody,
        InformationArchitectureBody,
        TopicIndexBody,
        GlossaryBody,
        FaqBody,
        TroubleshootingBody,
        HFunctionAuthoringBody,
        CompilerOverviewBody,
        CompilerBoundaryBody,
        RouteCatalogContractBody,
        HostSemanticSeamsBody,
        ImportEmitContractBody,
        RuntimeCatalogBody,
        JoltHostBody,
        RazorVueLibraryModeBody,
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

    private static readonly string[] SearchSectionIds =
    [
        "full-text",
        "section-hits",
        "topic-entry",
        "query-sharing"
    ];

    private static readonly string[] SearchSectionTitles =
    [
        "Full-text search",
        "Section matches",
        "Topic entry points",
        "Shareable queries"
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

    private static readonly string[] ProjectLinesSectionIds =
    [
        "two-lines",
        "choose-a-path",
        "shared-core",
        "where-to-read-next"
    ];

    private static readonly string[] ProjectLinesSectionTitles =
    [
        "Two active lines",
        "Choose the right path",
        "Shared core",
        "Where to read next"
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

    private static readonly string[] TopicIndexSectionIds =
    [
        "topic-clusters",
        "core-runtime",
        "operating-and-writing"
    ];

    private static readonly string[] TopicIndexSectionTitles =
    [
        "Topic clusters",
        "Core runtime and architecture",
        "Operating and writing"
    ];

    private static readonly string[] GlossarySectionIds =
    [
        "compiler-terms",
        "runtime-terms",
        "host-terms"
    ];

    private static readonly string[] GlossarySectionTitles =
    [
        "Compiler terms",
        "Runtime terms",
        "Host and workflow terms"
    ];

    private static readonly string[] FaqSectionIds =
    [
        "using-jazor",
        "compiler-boundaries",
        "runtime-and-host",
        "wiki-workflow"
    ];

    private static readonly string[] FaqSectionTitles =
    [
        "Using Jazor",
        "Compiler boundaries",
        "Runtime and host behavior",
        "Wiki workflow"
    ];

    private static readonly string[] TroubleshootingSectionIds =
    [
        "route-and-host",
        "runtime-imports",
        "compiler-diagnostics",
        "workflow-fixes"
    ];

    private static readonly string[] TroubleshootingSectionTitles =
    [
        "Route and host issues",
        "Runtime import failures",
        "Compiler and analyzer diagnostics",
        "Workflow fixes"
    ];

    private static readonly string[] CompilerOverviewSectionIds =
    [
        "what-it-is",
        "core-pipeline",
        "hard-contracts",
        "read-this-next"
    ];

    private static readonly string[] CompilerOverviewSectionTitles =
    [
        "What it is",
        "Core pipeline",
        "Hard contracts",
        "Read this next"
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

    private static readonly string[] JoltHostSectionIds =
    [
        "why-jolt",
        "subsystems",
        "run-modes",
        "when-to-choose-jolt"
    ];

    private static readonly string[] JoltHostSectionTitles =
    [
        "Why Jolt exists",
        "Subsystems",
        "Run modes",
        "When to choose Jolt"
    ];

    private static readonly string[] RazorVueLibraryModeSectionIds =
    [
        "why-razorvue",
        "physical-split",
        "build-time-flow",
        "when-to-choose-library-mode"
    ];

    private static readonly string[] RazorVueLibraryModeSectionTitles =
    [
        "Why RazorVue exists",
        "Physical split",
        "Build-time flow",
        "When to choose library mode"
    ];

    internal static readonly string[][] PageSectionIdSets =
    [
        OverviewSectionIds,
        SearchSectionIds,
        GettingStartedSectionIds,
        ProjectLinesSectionIds,
        ContentModelSectionIds,
        NavigationDiscoverySectionIds,
        InformationArchitectureSectionIds,
        TopicIndexSectionIds,
        GlossarySectionIds,
        FaqSectionIds,
        TroubleshootingSectionIds,
        HFunctionAuthoringSectionIds,
        CompilerOverviewSectionIds,
        CompilerBoundarySectionIds,
        RouteCatalogContractSectionIds,
        HostSemanticSeamsSectionIds,
        ImportEmitContractSectionIds,
        RuntimeCatalogSectionIds,
        JoltHostSectionIds,
        RazorVueLibraryModeSectionIds,
        ContentGovernanceSectionIds,
        DeploymentSectionIds,
        TestingVerificationSectionIds
    ];

    internal static readonly string[][] PageSectionTitleSets =
    [
        OverviewSectionTitles,
        SearchSectionTitles,
        GettingStartedSectionTitles,
        ProjectLinesSectionTitles,
        ContentModelSectionTitles,
        NavigationDiscoverySectionTitles,
        InformationArchitectureSectionTitles,
        TopicIndexSectionTitles,
        GlossarySectionTitles,
        FaqSectionTitles,
        TroubleshootingSectionTitles,
        HFunctionAuthoringSectionTitles,
        CompilerOverviewSectionTitles,
        CompilerBoundarySectionTitles,
        RouteCatalogContractSectionTitles,
        HostSemanticSeamsSectionTitles,
        ImportEmitContractSectionTitles,
        RuntimeCatalogSectionTitles,
        JoltHostSectionTitles,
        RazorVueLibraryModeSectionTitles,
        ContentGovernanceSectionTitles,
        DeploymentSectionTitles,
        TestingVerificationSectionTitles
    ];

    internal static readonly string[][] PageRelatedPathSets =
    [
        [SearchPath, GettingStartedPath, ProjectLinesPath],
        [TopicIndexPath, GlossaryPath, TroubleshootingPath],
        [ProjectLinesPath, NavigationDiscoveryPath, DeploymentPath],
        [JoltHostPath, RazorVueLibraryModePath, CompilerOverviewPath],
        [NavigationDiscoveryPath, InformationArchitecturePath, ContentGovernancePath],
        [InformationArchitecturePath, TopicIndexPath, RouteCatalogContractPath],
        [TopicIndexPath, RouteCatalogContractPath, ContentModelPath],
        [GlossaryPath, CompilerOverviewPath, TroubleshootingPath],
        [TopicIndexPath, CompilerOverviewPath, RuntimeCatalogPath],
        [TroubleshootingPath, GettingStartedPath, ProjectLinesPath],
        [FaqPath, DeploymentPath, TestingVerificationPath],
        [ContentModelPath, CompilerOverviewPath, RouteCatalogContractPath],
        [CompilerBoundaryPath, HostSemanticSeamsPath, ImportEmitContractPath],
        [CompilerOverviewPath, HostSemanticSeamsPath, RuntimeCatalogPath],
        [NavigationDiscoveryPath, ContentModelPath, ContentGovernancePath],
        [ImportEmitContractPath, RuntimeCatalogPath, CompilerBoundaryPath],
        [RuntimeCatalogPath, DeploymentPath, TestingVerificationPath],
        [HostSemanticSeamsPath, ImportEmitContractPath, DeploymentPath],
        [ProjectLinesPath, RazorVueLibraryModePath, GettingStartedPath],
        [ProjectLinesPath, JoltHostPath, HFunctionAuthoringPath],
        [ContentModelPath, DeploymentPath, TestingVerificationPath],
        [TestingVerificationPath, TroubleshootingPath, RuntimeCatalogPath],
        [DeploymentPath, ContentGovernancePath, TroubleshootingPath]
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

        if (currentPath.Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageGroup(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageTitle(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageSummary(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageStatus(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageOwner(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageAudience(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageSourceFile(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageLastUpdated(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageSearchBody(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var tags = GetPageTags(currentPath);
        for (var tagIndex = 0; tagIndex < tags.Length; tagIndex++)
        {
            if (tags[tagIndex].Contains(filterText, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
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

    private static string GetPageOwner(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageOwners.Length)
            return PageOwners[pageIndex];

        return "Unknown";
    }

    private static string GetPageAudience(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageAudiences.Length)
            return PageAudiences[pageIndex];

        return "Unknown";
    }

    private static string GetPageSourceFile(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageSourceFiles.Length)
            return PageSourceFiles[pageIndex];

        return "";
    }

    private static string GetPageLastUpdated(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageLastUpdatedDates.Length)
            return PageLastUpdatedDates[pageIndex];

        return "";
    }

    private static int GetPageReadingMinutes(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageReadingMinutes.Length)
            return PageReadingMinutes[pageIndex];

        return 0;
    }

    private static string GetPageSearchBody(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageSearchBodies.Length)
            return PageSearchBodies[pageIndex];

        return "";
    }

    private static string[] GetPageTags(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageTagSets.Length)
            return PageTagSets[pageIndex];

        return [];
    }

    private static string GetPreviousPath(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex < 0)
            return "";

        for (var previousIndex = pageIndex - 1; previousIndex >= 0; previousIndex--)
        {
            if (PagePaths[previousIndex] != SearchPath)
                return PagePaths[previousIndex];
        }

        return "";
    }

    private static string GetNextPath(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex < 0)
            return "";

        for (var nextIndex = pageIndex + 1; nextIndex < TotalPageCount; nextIndex++)
        {
            if (PagePaths[nextIndex] != SearchPath)
                return PagePaths[nextIndex];
        }

        return "";
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
            return [OverviewPath, SearchPath, TopicIndexPath];

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
