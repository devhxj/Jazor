// WikiHomeModule.NavigationDiscovery.cs - 导航与发现 / Navigation & Discovery
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建导航与发现页面主体 / Build the navigation & discovery page body
    private static IVNode NavigationDiscoveryBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("left-rail", "左侧栏发现",
            [
                H("p", "左侧栏是主要的发现表面。它按关注点对页面分组，保持当前页面可见，并暴露客户端过滤功能而无需离开当前路由。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("分组入口", "基础、工程和运维作为稳定的产品分组保持可见，而非一个扁平的页面列表。"),
                    CheckCard("本地过滤", "搜索框在实时外壳中按路由片段、分组标签、标题、摘要和状态过滤。"),
                    CheckCard("当前页上下文", "活跃路由样式让读者在跨相关主题移动时保持方向感。")
                ])
            ]),
            PageSection("right-rail", "右侧栏导航",
            [
                H("p", "章节级导航是产品契约的一部分，而非事后补充。右侧栏从已注册的章节 id 和标题生成，因此每个文档都有直接的页内入口点。"),
                H("ul",
                [
                    H("li", "Hash 链接保持可分享且刷新安全。"),
                    H("li", "活跃章节状态现在同时跟随直接 hash 入口和实时阅读滚动，因此读者无需先点击即可看到当前位置。"),
                    H("li", "浏览器后退或前进在路由没有显式章节 hash 时恢复上次阅读滚动位置。"),
                    H("li", "永久链接操作暴露每个章节的直接链接，无需发明第二个路由系统。")
                ])
            ]),
            PageSection("related-pages", "相关页面与阅读流",
            [
                H("p", "文档外壳刻意帮助读者继续阅读，而非在某一页停下。相关页面和上一页/下一页流在中央目录中策划，使阅读路径保持有目的性。"),
                H("ul",
                [
                    H("li", "相关页面是显式的目录条目，而非关键词猜测。"),
                    H("li", "上一页和下一页流来自路由顺序，使长篇阅读保持可预测。"),
                    H("li", "概览路由卡片复用相同的元数据表面，使站点地图和页面装饰保持一致。")
                ])
            ]),
            PageSection("not-found-recovery", "未找到恢复",
            [
                H("p", "未知 URL 不会让读者落入死胡同外壳。宿主仍然提供应用，文档表面通过分组感知和片段感知的建议提供恢复。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("路由回退", "ASP.NET Core 仍然为未知文档路径返回 `index.html`，以便前端可以恢复。"),
                    CheckCard("请求路径上下文", "未找到文档显示请求的路由，以便维护者诊断页面是缺失还是拼写错误。"),
                    CheckCard("建议页面", "恢复链接从与正常导航相同的路由目录派生。")
                ])
            ]),
            PageSection("authoring-implications", "编写影响",
            [
                H("p", "导航质量取决于页面元数据质量。页面不是在 body 存在时就完成了；而是在标题、摘要、章节、相关链接和路由位置都支持发现时才算完成。"),
                CodeBlock("发现就绪页面检查清单", """
1. Register the route in the central catalog.
2. Add product-facing title, summary, and status.
3. Add section ids and TOC labels that read well as direct links.
4. Curate related pages that help the next reading step.
5. Rebuild and rerun smoke so emitted navigation markers stay valid.
"""),
                Callout("实用规则", "如果读者无法可靠地找到、浏览并从页面继续阅读，即使正文本身已完成，内容仍然是不完整的。")
            ])
        ]);
}
