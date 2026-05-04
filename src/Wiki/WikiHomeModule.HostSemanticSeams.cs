using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode HostSemanticSeamsBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("why-seams-exist", "Why seams exist",
            [
                H("p", "Host semantics are not an escape hatch for arbitrary JavaScript. They are the declared seam between compiler lowering and supported external runtime behavior."),
                H("ul",
                [
                    H("li", "`WhiteList` declares which external types and members are supported."),
                    H("li", "Consumer dispatch stays ordered as `Allowed/Alias -> Inline -> Import -> Compile`."),
                    H("li", "Unsupported runtime-sensitive behavior should fail explicitly instead of degrading to raw JavaScript.")
                ])
            ]),
            PageSection("choose-the-right-seam", "Choose the right seam",
            [
                H("p", "The main engineering decision is not whether to add a mapping. It is which seam owns the behavior."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Alias", "Use for stable name remaps such as type/runtime name projection or obvious member rename cases like `ToString -> toString`."),
                    CheckCard("Inline", "Use for short, readable expression templates with stable local semantics and no complex control flow."),
                    CheckCard("Import", "Use for shared helper logic, repeated guards, non-trivial branching, or behavior that is clearer as an explicit runtime module."),
                    CheckCard("Compile", "Use when the host behavior needs AST-level construction, context-sensitive lowering, temps, imports, or protocol-aware structure.")
                ])
            ]),
            PageSection("whitelist-contract", "WhiteList contract",
            [
                H("p", "`WhiteList` is not just a string replacement table. It is the compiler's formal host capability surface, generated from source declarations in `Jazor.CLR` and related mappings."),
                CodeBlock("Current host-mapping sources", """
src/Jazor.CLR/module/*.cs
src/Jazor.Compiler/WhiteList.cs.Generate.cs
src/Jazor.Compiler.Generator/Program.cs
src/Jazor.Compiler/core/SemanticWalker.cs
"""),
                H("ul",
                [
                    H("li", "Change the declaration source first; do not hand-edit generated whitelist output."),
                    H("li", "Keep producer and consumer semantics aligned so the same API surface does not drift between CLR source and compiler dispatch."),
                    H("li", "Treat `Op.Discard` and explicit unsupported cases as product boundary markers, not as temporary embarrassment to hide.")
                ])
            ]),
            PageSection("inline-vs-compile", "Inline versus Compile",
            [
                H("p", "A common failure mode is leaving complex behavior in `Inline` for too long. The readability bar matters as much as the semantic bar."),
                H("ul",
                [
                    H("li", "Prefer `Inline` when one expression stays short, reviewable, and semantically obvious."),
                    H("li", "Upgrade to `Import` when behavior needs shared helper code or would become a long, branch-heavy template."),
                    H("li", "Upgrade to `Compile` when the host semantics need AST nodes, expression or statement restructuring, or contextual lowering decisions."),
                    H("li", "Do not push public authoring sugar into ad-hoc `[Jazor]` compile hooks if the behavior should really be an intrinsic compiler rule.")
                ]),
                Callout("Practical rule", "If reviewers have to mentally simulate placeholder substitution to trust the behavior, the seam is probably too weak.")
            ]),
            PageSection("verification-surface", "Verification surface",
            [
                H("p", "Every seam change should prove both mapping metadata and emitted behavior."),
                H("ul",
                [
                    H("li", "Add CLR whitelist tests when a type alias or member mapping changes."),
                    H("li", "Add compiler tests when dispatch order, inline emission, import binding, or compile-hook behavior changes."),
                    H("li", "Use Wiki smoke when the change affects browser-served modules, import-map assumptions, or emitted docs shell output."),
                    H("li", "Keep concrete and interface surfaces aligned when they represent one runtime contract family.")
                ])
            ])
        ]);
}
