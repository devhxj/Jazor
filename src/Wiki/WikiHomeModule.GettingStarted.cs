// WikiHomeModule.GettingStarted.cs - 快速开始 / Getting Started
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建快速开始页面主体 / Build the getting started page body
    private static IVNode GettingStartedBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("boot-the-site", "本地启动站点",
            [
                H("p", "本地循环刻意保持简短。从仓库根目录构建、发射并运行静态宿主。"),
                CodeBlock("本地命令", """
dotnet build .\src\Wiki\Wiki.csproj
.\src\Wiki\serve.ps1 -Build
.\src\Wiki\verify-smoke.ps1 -BuildLocal
"""),
                H("p", "冒烟脚本现在是契约的一部分。只有当构建输出和路由回退都被验证通过时，一个真实路由才被视为有效。")
            ]),
            PageSection("route-model", "理解路由模型",
            [
                H("p", "Wiki 现在使用真实的 URL 路径配合服务器回退，因此路由可以直接刷新或在浏览器中直接打开。"),
                H("ul",
                [
                    H("li", "`/` 为概览页面"),
                    H("li", "`/search?q=compiler` 为可分享的查询驱动搜索入口"),
                    H("li", "`/guides/getting-started` 为本地工作流"),
                    H("li", "`/guides/project-lines` 为活跃产品线路"),
                    H("li", "`/guides/content-model` 为页面编写规则"),
                    H("li", "`/guides/navigation-discovery` 为分组导航、TOC 行为、相关页面和未找到恢复"),
                    H("li", "`/guides/information-architecture` 为路由族、命名规则和页面顺序规范"),
                    H("li", "`/guides/topic-index`、`/guides/glossary`、`/guides/faq` 和 `/guides/troubleshooting` 为发现与支持"),
                    H("li", "`/engineering/h-function-authoring` 为 H-function 契约"),
                    H("li", "`/engineering/compiler-overview` 为编译器管线入口"),
                    H("li", "`/engineering/compiler-support-boundary` 为活跃编译器语义和失败规则"),
                    H("li", "`/engineering/route-catalog-contract` 为单源路由注册契约"),
                    H("li", "`/engineering/host-semantic-seams` 为 Alias / Inline / Import / Compile 职责边界"),
                    H("li", "`/engineering/import-emit-contract` 为模块导入流和文件物化边界"),
                    H("li", "`/engineering/runtime-catalog` 为 CLR 运行时辅助函数生成和浏览器交付"),
                    H("li", "`/engineering/jolt-host` 和 `/engineering/razorvue-library-mode` 为两条活跃交付线路"),
                    H("li", "`/operations/content-governance` 为内容所有权、生成输出审查和发布规范"),
                    H("li", "`/operations/deployment` 为构建和托管细节"),
                    H("li", "`/operations/testing-verification` 为聚焦测试和冒烟工作流")
                ])
            ]),
            PageSection("add-a-page", "安全添加页面",
            [
                H("p", "新页面通过添加一个路由常量、一个目录条目、一个专用页面文件和一个 body 方法来引入。导航分组、TOC 连接、相关链接和翻页连续性都应从中央页面目录中自动流转。"),
                CodeBlock("最小页面形状", """
private const string NewPagePath = "/guides/new-page";

private static IVNode NewPageBody()
    => H("div", "...");
"""),
                H("p", "路由创建后，在目录中注册其摘要、状态、章节锚点和相关页面，然后重新运行冒烟脚本。")
            ]),
            PageSection("verify-the-result", "验证结果",
            [
                H("p", "对于 Wiki，验证刻意保持操作导向。构建输出、路由可用性和外壳稳定性比仅截图审查更重要。"),
                H("ul",
                [
                    H("li", "确认构建后 `main.mjs` 和 `components/wiki-home.mjs` 存在。"),
                    H("li", "确认所有已注册文档路由通过路由回退返回前端外壳。"),
                    H("li", "确认发射的模块文本仍包含预期的路由标识符、搜索外壳标记和页面标签。")
                ])
            ])
        ]);
}
