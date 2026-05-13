// WikiHomeModule.Overview.cs - 项目概览 / Project Overview
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建概览页面主体 / Build the overview page body
    private static IVNode OverviewBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("what-ships-now", "当前交付内容",
            [
                H("p", "Wiki 现在作为一个真实的文档外壳运行，而非单页演示。主要契约包括：稳定的静态宿主、明确的文档路由，以及基于 H-function 编写的布局。"),
                H("div", new VueObject { Class = "metric-grid" },
                [
                    MetricCard("" + TotalPageCount, "核心路由", "概览、指南、工程和运维页面作为一等入口交付。"),
                    MetricCard("1", "静态宿主", "ASP.NET Core 通过一个小型宿主提供静态资源、健康检查和回退路由服务。"),
                    MetricCard("100%", "H 函数外壳", "导航、主视觉区、文章章节、TOC 和翻页器全部基于 H-function 编写表面实现。")
                ])
            ]),
            PageSection("why-this-exists", "为什么存在",
            [
                H("p", "旧版 Wiki 证明了 Jazor 能够发射 Vue 模块。新版 Wiki 证明 H-function 路径可以承载面向生产的信息架构，而不仅仅是一个演示面板。"),
                H("ul",
                [
                    H("li", "站点本身现在是产品表面，而不仅仅是编译器示例。"),
                    H("li", "导航、页面发现、路由入口和部署指导被视为产品契约。"),
                    H("li", "内容模型保持显式，使维护者可以在没有隐藏管线的情况下演进。")
                ])
            ]),
            PageSection("mvp-boundary", "MVP 边界",
            [
                Callout("当前包含", "真实路由、多页文档、带本地过滤的左侧导航、右侧目录，以及上一页/下一页翻页流。"),
                H("ul",
                [
                    H("li", "已包含：面向生产的文档外壳、代码优先的页面，以及可冒烟验证的路由。"),
                    H("li", "已推迟：Markdown 导入、可编辑内容管理、评论和用户特定状态。"),
                    H("li", "已推迟：外部搜索服务和非 CDN 资源打包。")
                ])
            ]),
            PageSection("site-structure", "站点结构",
            [
                H("p", "站点刻意保持小型和显式。生产验证点不是一个抽象层，而是一个可维护的文档站点可以直接运行在 H-function 编写表面上，由一个中央页面目录驱动路由元数据和相邻页面导航。"),
                CodeBlock("当前生产表面", """
src/Wiki/
  Program.cs
  AppModule.cs
  WikiHomeModule.cs
  WikiHomeModule.RouteContract.cs
  WikiHomeModule.Elements.cs
  WikiCatalogGuard.cs
  WikiHomeModule.Overview.cs
  WikiHomeModule.Search.cs
  WikiHomeModule.GettingStarted.cs
  WikiHomeModule.ProjectLines.cs
  WikiHomeModule.ContentModel.cs
  WikiHomeModule.NavigationDiscovery.cs
  WikiHomeModule.InformationArchitecture.cs
  WikiHomeModule.TopicIndex.cs
  WikiHomeModule.Glossary.cs
  WikiHomeModule.Faq.cs
  WikiHomeModule.Troubleshooting.cs
  WikiHomeModule.HFunctionAuthoring.cs
  WikiHomeModule.CompilerOverview.cs
  WikiHomeModule.CompilerBoundary.cs
  WikiHomeModule.RouteCatalogContract.cs
  WikiHomeModule.HostSemanticSeams.cs
  WikiHomeModule.ImportEmitContract.cs
  WikiHomeModule.RuntimeCatalog.cs
  WikiHomeModule.JoltHost.cs
  WikiHomeModule.RazorVueLibraryMode.cs
  WikiHomeModule.VueRouteBindings.cs
  WikiHomeModule.ContentGovernance.cs
  WikiHomeModule.Deployment.cs
  WikiHomeModule.TestingVerification.cs
  host/index.template.html
  wwwroot/site.css
  wiki-verify-smoke.cs
""")
            ]),
            PageSection("registered-pages", "已注册页面",
            [
                H("p", "概览页面现在也是站点目录。下方每个已注册路由都由相同的页面元数据支撑，驱动导航分组、主视觉文案、相关页面建议、翻页连续性和右侧 TOC。"),
                RouteCardGrid(PagePaths)
            ])
        ]);
}
