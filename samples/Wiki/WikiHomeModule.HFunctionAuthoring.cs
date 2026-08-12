// WikiHomeModule.HFunctionAuthoring.cs - H 函数编写 / H-Function Authoring
using ECMAScript;
using static ECMAScript.Vue;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建 H 函数编写页面主体 / Build the H-function authoring page body
    private static IVNode HFunctionAuthoringBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("layout-composition", "布局组合",
            [
                H("p", "H 函数是此处的生产表面，因为它们保持渲染结构显式，同时保持在与其余项目相同的类型化生态系统中。"),
                H("span", new VueObject
                {
                    Class = "h-function-style-badge " + WikiStyleSheet.HFunctionBadge
                }, "H() + ECMAScript.Style"),
                CodeBlock("章节组合", """
private static IVNode PageSection(string id, string title, IVNode[] content)
    => H("section", new VueObject { Id = id, Class = "doc-section" },
    [
        H("div", new VueObject { Class = "section-anchor" }, id),
        H("h2", title),
        H("div", new VueObject { Class = "section-body" }, content)
    ]);
""")
            ]),
            PageSection("production-rules", "H 编写生产规则",
            [
                H("ul",
                [
                    H("li", "路由和元数据形状优先；视觉润色建立在稳定外壳之上。"),
                    H("li", "优先使用语义化 HTML 节点和类型化 props，而非字符串类型的 DOM 操作。"),
                    H("li", "保持辅助方法聚焦于一个视觉概念，使页面源码保持可读。"),
                    H("li", "如果页面后续需要更丰富的交互，有意地添加它，而非隐藏在布局辅助函数中。")
                ])
            ]),
            PageSection("why-this-works", "为什么这对真实项目有效",
            [
                H("p", "外壳是 H 函数交付最大价值的地方：路由感知布局、可复用结构、一致的页面装饰，以及在与其余产品相同代码库中的类型检查编写。"),
                Callout("服务优先于纯粹性", "站点优先优化可用性：H 拥有外壳，因为这是用户和维护者需要保持一致的部分。")
            ])
        ]);
}
