// WikiHomeModule.TopicIndex.cs - 主题索引页正文 / Topic index page body
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode TopicIndexBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("topic-clusters", "主题集群",
            [
                H("p", "当你知道主题但不确定确切路由名称时，使用此索引按关注点浏览 Wiki。页面按解决的问题分组，而非按源文件夹排列，更容易浏览。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("导向", "概览、快速开始和项目线路页面解释存在什么以及从哪里开始。"),
                    CheckCard("工程契约", "编译器、宿主接缝、导入、运行时和路由目录页面解释稳定的技术边界。"),
                    CheckCard("运维流程", "治理、部署和验证页面解释如何保持文档和发射产品的稳定性。")
                ])
            ]),
            PageSection("core-runtime", "核心运行时与架构",
            [
                H("p", "如果你需要了解 Jazor 主要子系统如何协同工作，请从这里开始。"),
                RouteCardGrid([ProjectLinesPath, CompilerOverviewPath, RuntimeCatalogPath, JoltHostPath, RazorVueLibraryModePath, VueRouteBindingsPath])
            ]),
            PageSection("operating-and-writing", "运维与编写",
            [
                H("p", "如果你当前的工作是添加文档、验证输出或诊断损坏的本地循环，请从这里开始。"),
                RouteCardGrid([GettingStartedPath, ContentModelPath, NavigationDiscoveryPath, ContentGovernancePath, TroubleshootingPath, TestingVerificationPath])
            ])
        ]);
}
