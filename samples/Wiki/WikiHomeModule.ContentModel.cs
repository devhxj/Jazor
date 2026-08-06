// WikiHomeModule.ContentModel.cs - 内容模型 / Content Model
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建内容模型页面主体 / Build the content model page body
    private static IVNode ContentModelBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("page-contract", "页面契约",
            [
                H("p", "每个页面在一个中央目录中拥有显式的路由元数据：路径、分组、标题、摘要、状态、章节锚点、相关页面链接和 body 分派。这足以驱动外壳而无需引入隐藏的内容层。"),
                H("ul",
                [
                    H("li", "路径是真实的 URL，是托管契约的一部分。"),
                    H("li", "摘要是简短的产品面向说明，而非内部工程笔记。"),
                    H("li", "状态传达成熟度，而无需为每个页面发明版本系统。"),
                    H("li", "相邻页面推荐在同一目录中策划，而非运行时推断。")
                ])
            ]),
            PageSection("navigation-contract", "导航契约",
            [
                H("p", "导航设计上保持显式。左侧栏按产品关注点分组，右侧栏从章节级锚点生成，相关页面面板从同一页面目录策划。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("左侧栏", "按用户关注点分组的稳定页面入口。"),
                    CheckCard("文章主体", "直接在 H 函数中编写的可读章节。"),
                    CheckCard("右侧栏", "锚点级 TOC，便于快速浏览和直接链接。"),
                    CheckCard("相关页面", "策划的下一步链接，与路由目录保持同步。")
                ])
            ]),
            PageSection("editing-rules", "编辑规则",
            [
                H("p", "站点是代码优先的，但它不应该读起来像任意的应用代码。编辑规则使其对文档工作保持可读性。"),
                H("ul",
                [
                    H("li", "保持每个章节足够简短，无需打开生成输出即可浏览。"),
                    H("li", "优先使用显式辅助函数如 `PageSection`、`Callout`、`CodeBlock` 和路由卡片网格，而非通用 DSL 层。"),
                    H("li", "将页面目录条目、导航元数据和章节锚点视为产品契约的一部分。"),
                    H("li", "添加页面时，更新目录一次，让导航、TOC、相关链接和翻页从该源头自动流转。")
                ]),
                Callout("不要为巧妙性而优化", "如果文档页面在 C# 中变得难以编辑，答案通常是更清晰的 H 组合，而非新的元语言。")
            ])
        ]);
}
