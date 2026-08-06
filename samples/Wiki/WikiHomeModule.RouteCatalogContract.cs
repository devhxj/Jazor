// WikiHomeModule.RouteCatalogContract.cs - 路由目录契约 / Route Catalog Contract
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建路由目录契约页面主体 / Build the route catalog contract page body
    private static IVNode RouteCatalogContractBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("single-source", "单一事实来源",
            [
                H("p", "Wiki 路由注册刻意集中化。`WikiHomeModule.RouteContract.cs` 是路由元数据、章节锚点、相关页面和 body 分派的唯一事实来源。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("目录数组", "`PagePaths`、分组、标题、摘要、状态、章节 id、章节标题和相关路径作为一个契约表面对齐。"),
                    CheckCard("外壳消费者", "导航栏、主视觉文案、右侧 TOC、相关页面面板、翻页流和未找到建议都从同一目录读取。"),
                    CheckCard("为何集中", "目标不是为抽象而抽象。目标是避免路由在多个文件和隐藏注册点之间漂移。")
                ])
            ]),
            PageSection("what-the-catalog-owns", "目录拥有什么",
            [
                H("p", "目录不仅负责路由存在性。它定义外壳向用户和维护者呈现的面向产品的元数据。"),
                H("ul",
                [
                    H("li", "真实路由路径和产品分组。"),
                    H("li", "页面标题、摘要和状态徽章。"),
                    H("li", "Body 分派函数。"),
                    H("li", "章节锚点和 TOC 标签。"),
                    H("li", "相关页面建议和上一页/下一页连续性。")
                ])
            ]),
            PageSection("safe-change-flow", "安全变更流",
            [
                H("p", "只有当目录、页面主体和操作检查一起移动时，页面变更才被视为安全。"),
                CodeBlock("最小路由添加工作流", """
1. Add one route constant.
2. Add one page body file and body method.
3. Register path, title, summary, status, sections, and related paths in the central catalog.
4. Update preview and smoke route expectations.
5. Rebuild and rerun verify-smoke.
"""),
                H("ul",
                [
                    H("li", "不要与中央目录并行添加隐藏路由注册或推断发现规则。"),
                    H("li", "不要让导航或 TOC 从可能与页面主体注册漂移的第二元数据源读取。"),
                    H("li", "将目录中的数组长度对齐视为正确性规则，而非仅仅是风格偏好。")
                ])
            ]),
            PageSection("failure-modes", "应避免的失败模式",
            [
                H("p", "大多数可维护性退化来自拆分元数据所有权或假设外壳可以后续推断结构。"),
                H("ul",
                [
                    H("li", "路由存在但页面标题或摘要未注册。"),
                    H("li", "页面主体存在但章节锚点未添加，导致 TOC 和直接链接漂移。"),
                    H("li", "相关页面或翻页顺序不再匹配预期的阅读流。"),
                    H("li", "目录变更后预览 URL、冒烟路由列表或发射标记检查未更新。")
                ]),
                Callout("实用规则", "如果维护者需要编辑两个不相关的元数据系统来添加一个页面，设计已经退化了。")
            ]),
            PageSection("verification-contract", "验证契约",
            [
                H("p", "路由目录受操作保护。文档外壳只有在目录反映在发射模块和服务路由中时才被视为有效。"),
                H("ul",
                [
                    H("li", "宿主启动现在在服务请求之前验证目录对齐、重复路径、重复章节 id 和相关页面完整性。"),
                    H("li", "冒烟检查发射的 `wiki-home.mjs` 中的路由标记、页面标题标记和章节锚点标记。"),
                    H("li", "冒烟通过真实宿主检查所有已注册文档路由，启用回退路由。"),
                    H("li", "未找到恢复通过建议页面和基于分组的回退依赖同一目录。"),
                    H("li", "概览页面通过 `RouteCardGrid(PagePaths)` 兼作实时目录表面。")
                ])
            ])
        ]);
}
