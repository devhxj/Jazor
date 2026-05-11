// WikiHomeModule.InformationArchitecture.cs - 信息架构 / Information Architecture
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建信息架构页面主体 / Build the information architecture page body
    private static IVNode InformationArchitectureBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("concern-groups", "关注点分组",
            [
                H("p", "Wiki 路由结构首先按读者关注点分组。目标是即使在文档表面增长时也保持发现显而易见，而非将所有内容扁平化为一个长路由列表。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("基础 (Foundation)", "解释如何使用、导航和维护文档外壳本身的指南。"),
                    CheckCard("工程 (Engineering)", "贡献者需要理解的契约、接缝、编译器边界，以及运行时或 Emit 行为。"),
                    CheckCard("运维 (Operations)", "保持站点可发布的构建、治理、验证和面向发布的规则。")
                ])
            ]),
            PageSection("route-shape", "路由形状",
            [
                H("p", "URL 形状是产品契约的一部分。路由应在读者阅读正文之前就告诉他们正在打开什么类型的页面。"),
                CodeBlock("当前路由族", """
/                         overview
/guides/*                 reader and maintainer guides
/engineering/*            compiler and host contracts
/operations/*             build, governance, and verification
"""),
                H("ul",
                [
                    H("li", "使用小写路由段。"),
                    H("li", "使用连字符连接的英文单词，而非 camelCase 或不透明的缩写。"),
                    H("li", "保持分组前缀稳定，使恢复和建议逻辑保持可预测。")
                ])
            ]),
            PageSection("naming-rules", "命名规则",
            [
                H("p", "路由名称、标题、摘要和章节锚点应该相互配合阅读。外壳通过导航、过滤、TOC 链接、路由卡片和未找到建议直接暴露所有这些内容。"),
                H("ul",
                [
                    H("li", "标题应面向产品且可扫描，而非内部任务标签。"),
                    H("li", "摘要应在一句短句中说明页面结果。"),
                    H("li", "章节 id 应稳定且链接友好，因为它们会成为可分享的锚点。"),
                    H("li", "章节标题在正文和右侧 TOC 中都应自然阅读。")
                ])
            ]),
            PageSection("ordering-rules", "排序与阅读流",
            [
                H("p", "目录顺序不是装饰性的。它控制上一页/下一页流，影响相关概念的发现方式，并设定整个站点的阅读节奏。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("翻页连续性", "路由排序应使上一页和下一页导航感觉有意而非随意。"),
                    CheckCard("相关页面", "策划的相关链接应强化与路由顺序相同的心智模型，而非与之冲突。"),
                    CheckCard("概览目录", "概览页面兼作路由地图，因此路由顺序也是可见站点结构的一部分。")
                ])
            ]),
            PageSection("growth-without-drift", "增长而不漂移",
            [
                H("p", "添加页面应使站点更丰富，而不侵蚀路由模型。测试标准是新页面能否干净地放入一个关注点分组和一条阅读路径。"),
                CodeBlock("安全增长检查清单", """
1. Choose the correct concern group first.
2. Pick a route that matches the existing family shape.
3. Add product-facing title, summary, and status metadata.
4. Add related pages that help the next reading step.
5. Rebuild and rerun smoke so route and section markers stay protected.
"""),
                Callout("实用规则", "如果新页面在基础、工程或运维中没有明显的归属，信息架构可能需要在添加页面之前进行修订。")
            ])
        ]);
}
