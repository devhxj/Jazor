using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode HFunctionAuthoringBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("layout-composition", "Layout composition",
            [
                H("p", "H functions are the production surface here because they keep the rendered structure explicit while staying inside the same typed ecosystem as the rest of the project."),
                CodeBlock("Section composition", """
private static IVNode PageSection(string id, string title, IVNode[] content)
    => H("section", new VueObject { Id = id, Class = "doc-section" },
    [
        H("div", new VueObject { Class = "section-anchor" }, id),
        H("h2", title),
        H("div", new VueObject { Class = "section-body" }, content)
    ]);
""")
            ]),
            PageSection("production-rules", "Production rules for H authoring",
            [
                H("ul",
                [
                    H("li", "Route and metadata shape come first; visual polish sits on top of a stable shell."),
                    H("li", "Prefer semantic HTML nodes and typed props over stringly-typed DOM manipulation."),
                    H("li", "Keep helper methods focused on one visual concept so the page source stays readable."),
                    H("li", "If a page needs richer interaction later, add it intentionally rather than hiding it inside layout helpers.")
                ])
            ]),
            PageSection("why-this-works", "Why this works for a real project",
            [
                H("p", "The shell is where H functions deliver the most value: route-aware layout, reusable structure, consistent page chrome, and type-checked authoring inside the same codebase as the rest of the product."),
                Callout("Service over purity", "The site optimizes for usability first: H owns the shell because that is the part users and maintainers need to stay consistent.")
            ])
        ]);
}
