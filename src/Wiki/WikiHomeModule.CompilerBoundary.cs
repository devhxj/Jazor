using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode CompilerBoundaryBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("controlled-domain", "Controlled input domain",
            [
                H("p", "`Jazor.Compiler` is not a generic CLR-to-JS compiler. The supported contract is a controlled input domain where Roslyn `IOperation` semantics, host mappings, and deterministic emission all matter more than pretending every .NET runtime shape can exist in JavaScript."),
                H("ul",
                [
                    H("li", "Primary goal: preserve usage-site observable behavior."),
                    H("li", "Second goal: keep host semantic boundaries explicit and reviewable."),
                    H("li", "Third goal: keep emitted imports, names, and source anchors deterministic.")
                ])
            ]),
            PageSection("behavior-priority", "Behavior priority",
            [
                H("p", "When Jazor cannot preserve full CLR runtime identity, it follows an explicit priority order instead of ad-hoc compromises."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("1. Evaluation order", "Do not duplicate or reorder side effects just to make the JS look cleaner."),
                    CheckCard("2. Side-effect count", "A lowering that changes how many times something executes is usually wrong even if the final value looks right."),
                    CheckCard("3. Final result", "Value, branch result, and visible state come before runtime structure fidelity."),
                    CheckCard("4. Usage-site semantics", "Tuple, record, and protocol-based features can erase runtime identity if behavior at the use site stays correct.")
                ])
            ]),
            PageSection("support-boundary", "Support boundary",
            [
                H("p", "Support is decided at the runtime-sensitive use site, not only by whether a type name appears somewhere in the source. That is why erased generic positions can be tolerated while concrete runtime materialization still fails."),
                H("ul",
                [
                    H("li", "Usually allowed: `List<Unsupported>`, `Task<Unsupported>`, `Dictionary<TKey, Unsupported>`, and similar erased positions."),
                    H("li", "Usually rejected: `new Unsupported()`, runtime-sensitive `default(Unsupported)`, and direct static or instance member access on unsupported external types."),
                    H("li", "Default policy for unsupported runtime-sensitive behavior: fail fast with an explicit diagnostic, not silent raw-JS fallback.")
                ])
            ]),
            PageSection("stabilized-routes", "Stabilized semantic routes",
            [
                H("p", "Several language routes are no longer exploratory. Contributors should treat them as active contracts."),
                H("ul",
                [
                    H("li", "Tuple: erased value-composition lowering; preserve projection, deconstruction, and comparison behavior, not `System.ValueTuple` runtime identity."),
                    H("li", "Ref/out: caller/callee protocol simulation with order and write-back semantics preserved."),
                    H("li", "Enum: declaration erasure plus usage-site constant lowering."),
                    H("li", "Interface: analysis and host-lookup contract only; no runtime declaration emission."),
                    H("li", "Import/emit chain: `SemanticWalker` collects imports, `AstConverter` emits stable module headers, `Jazor.Emit` materializes files.")
                ])
            ]),
            PageSection("practical-reading", "Practical reading order",
            [
                H("p", "When extending compiler support, start from the active rationale docs, not from historical pass-rate snapshots."),
                CodeBlock("Recommended sources", """
src/Jazor.Compiler/ImplementationPrinciples.md
docs/03-完成/compiler/status.md
src/Jazor.Compiler/README.md
src/Jazor.CompilerTest/README.md
"""),
                Callout("Working rule", "If a proposed compiler change weakens support boundaries or introduces output instability, it is probably the wrong extension route even if it makes one narrow case pass.")
            ])
        ]);
}
