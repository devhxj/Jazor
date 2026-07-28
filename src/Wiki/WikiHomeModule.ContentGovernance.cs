// WikiHomeModule.ContentGovernance.cs - 内容治理 / Content Governance
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建内容治理页面主体 / Build the content governance page body
    private static IVNode ContentGovernanceBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("ownership-model", "所有权模型",
            [
                H("p", "Wiki 是代码优先的，但它仍然是产品表面。内容所有权是显式的：页面正文位于每页的 H 函数文件中，路由元数据位于中央目录中，宿主或外壳行为位于共享模块和元素辅助函数中。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("页面源码", "在该路由的专用页面文件中编辑页面文案、章节顺序和示例。"),
                    CheckCard("路由元数据", "在 `WikiHomeModule.RouteContract.cs` 中编辑路径、标题、摘要、状态、TOC 标签和相关页面连接。"),
                    CheckCard("外壳行为", "在共享外壳文件中编辑导航、翻页、TOC、永久链接和未找到行为，而非复制本地页面逻辑。")
                ])
            ]),
            PageSection("source-boundaries", "源码边界",
            [
                H("p", "编写边界必须对维护者保持明显。源文件直接编辑；生成的浏览器制品作为输出检查，不作为手工维护的源码。"),
                H("ul",
                [
                    H("li", "在 `WikiHomeModule.*.cs`、`Program.cs`、`AppModule.cs`、`host/index.template.html` 和 `site.css` 中编写内容。"),
                    H("li", "不要将 `src/Wiki/wwwroot/jazor/main.mjs`、`components/wiki-home.mjs` 或发射的 manifest 文件手工维护为行为的主要来源。"),
                    H("li", "如果发射输出因源码变更而改变，审查生成的差异并保持与导致变更的源码变更同步。")
                ])
            ]),
            PageSection("generated-assets", "生成资源",
            [
                H("p", "生成资源是交付产品的一部分，因此在操作上很重要。规则不是忽略它们。规则是从源码重新生成并作为下游产品制品审查。"),
                CodeBlock("编写与输出", """
Author here:
  src/Wiki/WikiHomeModule.*.cs
  src/Wiki/AppModule.cs
  src/Wiki/Program.cs
  src/Wiki/host/index.template.html
  src/Wiki/wwwroot/site.css

Review output here:
src/Wiki/wwwroot/jazor/main.mjs
src/Wiki/wwwroot/jazor/components/wiki-home.mjs
src/Wiki/wwwroot/jazor/jazor-manifest.json
"""),
                H("p", "这种拆分使维护者基于源码工作，同时仍强制审查浏览器将实际执行的内容。")
            ]),
            PageSection("change-flow", "安全变更流",
            [
                H("p", "内容变更只有在源码、目录和发射的产品输出都一致时才算完成。"),
                CodeBlock("操作流程", """
1. Edit the page body or shell source.
2. Update the central route catalog if the route contract changed.
3. Update preview URLs and smoke expectations for any new route.
4. Build the host to regenerate emitted browser assets.
5. Run `wiki-verify-smoke.cs` before treating the page as ready.
"""),
                H("ul",
                [
                    H("li", "如果添加了路由，README 和预览工具在同一切片中移动。"),
                    H("li", "如果外壳交互变更，验证发射的模块仍包含预期的浏览器标记。"),
                    H("li", "如果维护者无法判断哪个文件拥有某个变更，编写边界已经需要修正。")
                ])
            ]),
            PageSection("release-discipline", "发布纪律",
            [
                H("p", "生产就绪不仅关于页面是否读起来顺畅。文档变更只有在真实宿主和发射资产仍满足声明的产品契约时才可发布。"),
                H("ul",
                [
                    H("li", "保持页面标题和摘要面向产品；它们驱动导航、主视觉文案、建议和搜索过滤。"),
                    H("li", "不要仅因为页面主体本身编译通过就合并路由目录漂移、缺失章节锚点或过时的预览列表。"),
                    H("li", "将构建和冒烟验证视为每个新路由和每个影响外壳变更的最低发布门槛。")
                ]),
                Callout("实用规则", "文档站点是生产代码。内容变更不免除源码所有权、生成输出审查或操作验证。")
            ])
        ]);
}
