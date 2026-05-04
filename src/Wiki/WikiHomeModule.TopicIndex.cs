using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode TopicIndexBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("topic-clusters", "Topic clusters",
            [
                H("p", "The Wiki is easier to scan when pages are grouped by the problem you are solving, not just by source folder. Use this index when you know the theme but not the exact route name."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Orientation", "Overview, getting started, and project-line pages explain what exists and where to begin."),
                    CheckCard("Engineering contracts", "Compiler, host seam, import, runtime, and route-catalog pages explain the stable technical boundaries."),
                    CheckCard("Operational flow", "Governance, deployment, and verification pages explain how to keep the docs and emitted product stable.")
                ])
            ]),
            PageSection("core-runtime", "Core runtime and architecture",
            [
                H("p", "Start here if you need to understand how the major Jazor subsystems fit together."),
                RouteCardGrid([ProjectLinesPath, CompilerOverviewPath, RuntimeCatalogPath, JoltHostPath, RazorVueLibraryModePath])
            ]),
            PageSection("operating-and-writing", "Operating and writing",
            [
                H("p", "Start here if your immediate work is adding docs, validating output, or diagnosing broken local loops."),
                RouteCardGrid([GettingStartedPath, ContentModelPath, NavigationDiscoveryPath, ContentGovernancePath, TroubleshootingPath, TestingVerificationPath])
            ])
        ]);
}
