// WikiHomeModule.RouteContract.cs - Wiki 页面路由元数据目录 / Wiki page route metadata catalog
using System;
using ECMAScript;
using static ECMAScript.Vue;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 页面路由路径 / Page route paths
    internal static readonly string[] PagePaths =
    [
        OverviewPath,
        SearchPath,
        GettingStartedPath,
        ProjectLinesPath,
        ContentModelPath,
        NavigationDiscoveryPath,
        InformationArchitecturePath,
        TopicIndexPath,
        GlossaryPath,
        FaqPath,
        TroubleshootingPath,
        HFunctionAuthoringPath,
        CompilerOverviewPath,
        CompilerBoundaryPath,
        RouteCatalogContractPath,
        HostSemanticSeamsPath,
        ImportEmitContractPath,
        RuntimeCatalogPath,
        JoltHostPath,
        RazorVueLibraryModePath,
        VueRouteBindingsPath,
        ContentGovernancePath,
        DeploymentPath,
        TestingVerificationPath
    ];

    // 页面分组标识（Foundation/Engineering/Operations）/ Page group identifiers
    internal static readonly string[] PageGroups =
    [
        "Foundation",
        "Foundation",
        "Foundation",
        "Foundation",
        "Foundation",
        "Foundation",
        "Foundation",
        "Foundation",
        "Foundation",
        "Foundation",
        "Foundation",
        "Engineering",
        "Engineering",
        "Engineering",
        "Engineering",
        "Engineering",
        "Engineering",
        "Engineering",
        "Engineering",
        "Engineering",
        "Engineering",
        "Operations",
        "Operations",
        "Operations"
    ];

    // 页面标题 / Page titles
    internal static readonly string[] PageTitles =
    [
        "概览",
        "搜索",
        "快速开始",
        "项目线路",
        "内容模型",
        "导航与发现",
        "信息架构",
        "主题索引",
        "术语表",
        "常见问题",
        "故障排除",
        "H 函数编写",
        "编译器概览",
        "编译器支持边界",
        "路由目录契约",
        "宿主语义接缝",
        "导入与发射契约",
        "CLR 运行时目录",
        "Jolt 宿主（历史）",
        "RazorVue 库模式",
        "VueRoute 绑定",
        "内容治理",
        "部署",
        "测试与验证"
    ];

    // 页面摘要 / Page summaries
    internal static readonly string[] PageSummaries =
    [
        "面向生产的 Jazor 文档外壳，完全使用 ECMAScript.Vue H 函数编写。",
        "基于 URL 的全文搜索，覆盖路由元数据、标签、精选页面正文和章节标题。",
        "本地运行站点，理解路由模型，并端到端验证发射的 Wiki 宿主。",
        "了解当前 Razor-to-Vue 转型主线、共享编译器基础和已经退役的 Jolt 历史边界。",
        "代码优先的页面元数据、显式章节和保持可读性的 C# 导航契约。",
        "读者如何通过分组导航、章节目录、相关页面和 404 恢复在文档外壳中移动。",
        "路由、关注组、页面顺序和命名规则如何保持文档表面在增长时保持一致性。",
        "使用以路由为先的索引，按关注点跳转到 Jazor 主题，而无需记住确切 URL。",
        "编译器、运行时、宿主和文档术语在仓库中使用的共享词汇表。",
        "贡献者首次接触 Jazor 或 Wiki 时最常见问题的简短回答。",
        "从最常见的本地 Wiki、运行时模块和编译器边界故障中恢复。",
        "为什么 H 函数是此 Wiki 的生产编写表面，以及保持其可维护性的约定。",
        "编译器管线、活动契约和深入阅读方向的高级概览。",
        "受控输入、使用点验证、语义擦除和显式失败边界的活动编译器契约。",
        "为什么 `WikiHomeModule.RouteContract.cs` 是路由元数据、正文分发、目录锚点和相邻页面流的唯一注册面。",
        "WhiteList、Alias、Inline、Import 和 Compile 如何在支持的宿主语义面上划分职责。",
        "导入发现、模块 AST 组装、生成的目录和面向宿主的文件物化之间的稳定边界。",
        "CLR 导入 helper 如何变为浏览器可用的 `System/*` 运行时模块，以及哪些保障使该目录可安全发布。",
        "Jolt 已从转型分支退役；本页仅保留基线、能力范围和历史恢复入口。",
        "用于将 Razor 组件编译为 JavaScript 产物的构建时库模式，无需完整开发宿主。",
        "独立的 Vue Router 绑定库、其宿主表面范围，以及将测试排除在编译器套件之外的拆分验证路径。",
        "代码优先文档内容如何被拥有、编辑、审查和发布，而不偏离发射的产品外壳。",
        "构建输出、回退路由、冒烟验证和 Wiki 的静态交付契约。",
        "编译器、发射和运维冒烟检查如何协同保护生产文档表面。"
    ];

    // 页面状态标签 / Page status labels
    internal static readonly string[] PageStatuses =
    [
        "真实项目 MVP",
        "工具",
        "基础",
        "导向",
        "编写",
        "发现",
        "信息架构",
        "分类",
        "参考",
        "帮助",
        "支持",
        "工程",
        "核心",
        "边界",
        "目录",
        "宿主接缝",
        "管线",
        "运行时",
        "历史",
        "库",
        "绑定",
        "治理",
        "运维",
        "验证"
    ];

    // 页面负责人 / Page owners
    internal static readonly string[] PageOwners =
    [
        "文档",
        "文档",
        "文档",
        "文档",
        "文档",
        "文档",
        "文档",
        "文档",
        "文档",
        "文档",
        "文档",
        "文档",
        "编译器",
        "编译器",
        "文档",
        "编译器",
        "编译器",
        "CLR",
        "文档",
        "RazorVue",
        "运行时",
        "文档",
        "运维",
        "运维"
    ];

    // 页面目标读者 / Page audiences
    internal static readonly string[] PageAudiences =
    [
        "所有读者",
        "所有读者",
        "新读者",
        "新读者",
        "文档贡献者",
        "文档贡献者",
        "文档贡献者",
        "所有读者",
        "所有读者",
        "所有读者",
        "贡献者",
        "UI 编写者",
        "编译器贡献者",
        "编译器贡献者",
        "文档贡献者",
        "编译器贡献者",
        "编译器贡献者",
        "运行时贡献者",
        "维护者",
        "库编写者",
        "库编写者",
        "维护者",
        "维护者",
        "维护者"
    ];

    // 页面源文件路径（不翻译）/ Page source file paths
    internal static readonly string[] PageSourceFiles =
    [
        "samples/Wiki/WikiHomeModule.Overview.cs",
        "samples/Wiki/WikiHomeModule.Search.cs",
        "samples/Wiki/WikiHomeModule.GettingStarted.cs",
        "samples/Wiki/WikiHomeModule.ProjectLines.cs",
        "samples/Wiki/WikiHomeModule.ContentModel.cs",
        "samples/Wiki/WikiHomeModule.NavigationDiscovery.cs",
        "samples/Wiki/WikiHomeModule.InformationArchitecture.cs",
        "samples/Wiki/WikiHomeModule.TopicIndex.cs",
        "samples/Wiki/WikiHomeModule.Glossary.cs",
        "samples/Wiki/WikiHomeModule.Faq.cs",
        "samples/Wiki/WikiHomeModule.Troubleshooting.cs",
        "samples/Wiki/WikiHomeModule.HFunctionAuthoring.cs",
        "samples/Wiki/WikiHomeModule.CompilerOverview.cs",
        "samples/Wiki/WikiHomeModule.CompilerBoundary.cs",
        "samples/Wiki/WikiHomeModule.RouteCatalogContract.cs",
        "samples/Wiki/WikiHomeModule.HostSemanticSeams.cs",
        "samples/Wiki/WikiHomeModule.ImportEmitContract.cs",
        "samples/Wiki/WikiHomeModule.RuntimeCatalog.cs",
        "samples/Wiki/WikiHomeModule.JoltHost.cs",
        "samples/Wiki/WikiHomeModule.RazorVueLibraryMode.cs",
        "samples/Wiki/WikiHomeModule.VueRouteBindings.cs",
        "samples/Wiki/WikiHomeModule.ContentGovernance.cs",
        "samples/Wiki/WikiHomeModule.Deployment.cs",
        "samples/Wiki/WikiHomeModule.TestingVerification.cs"
    ];

    // 页面最后更新日期 / Page last-updated dates
    internal static readonly string[] PageLastUpdatedDates =
    [
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04",
        "2026-05-06",
        "2026-05-04",
        "2026-05-04",
        "2026-05-04"
    ];

    // 页面预计阅读时间（分钟）/ Page estimated reading minutes
    internal static readonly int[] PageReadingMinutes =
    [
        5,
        4,
        5,
        5,
        4,
        4,
        5,
        4,
        4,
        4,
        5,
        4,
        5,
        5,
        5,
        5,
        5,
        4,
        5,
        5,
        5,
        4,
        4,
        4
    ];

    // 页面搜索索引文本 / Page search index text
    internal static readonly string[] PageSearchBodies =
    [
        "介绍 jazor.wiki 作为使用 Vue 3 H 函数构建的生产文档外壳，包含路由回退、集中目录元数据和浏览器提供的运行时模块。",
        "提供可分享的全文搜索，覆盖页面元数据、标签、正文摘要和章节标题，支持结果高亮和章节级匹配。",
        "展示本地构建、服务和冒烟循环；解释路由结构、页面注册以及如何在每次更改后验证发射输出。",
        "解释当前 Razor-to-Vue 转型主线、共享编译器与 Emit 基础，以及 Jolt 的退役历史边界。",
        "描述显式页面契约、集中路由元数据、章节所有权和保持代码优先文档可读性的编辑规则。",
        "涵盖左侧导航发现、章节目录、相关页面、404 恢复以及可发现文档的编写含义。",
        "定义关注组、稳定的路由形态、命名规则、排序规则以及文档如何在不发生路由漂移的情况下增长。",
        "收集主要主题集群，让读者可以按关注点跳转到导向、工程契约或运维工作流。",
        "定义常用术语，如 SemanticWalker、AstConverter、WhiteList、import maps、CLR catalog、RazorVue、退役 Jolt 和冒烟验证。",
        "回答贡献者首次接触 Jazor 或 Wiki 时反复出现的问题，包括当前转型入口、退役 Jolt 边界、分析器与编译器边界、运行时 helper 模块和 Wiki 工作流。",
        "提供路由回退故障、缺失 System 模块、编译器诊断和损坏的本地验证循环的快速恢复步骤。",
        "解释为什么 H 函数是生产编写表面，布局 helper 如何保持显式，以及哪些规则保持 UI 组合可维护。",
        "概述编译器的目的、SemanticWalker 到 AstConverter 管线，以及围绕元组、接口、导入和显式失败的硬性契约。",
        "详述支持边界、行为优先顺序、稳定的语义路由，以及为什么不受支持的运行时敏感行为会显式失败。",
        "解释为什么路由注册、目录锚点、相关链接和页面顺序必须全部保留在一个路由契约中，而不是分散到多个文件。",
        "展示 WhiteList、Alias、Inline、Import 和 Compile 如何划分宿主职责，以及如何为映射选择正确的接缝。",
        "描述导入发现、模块头组装、输出分层，以及 emit 如何从编译器载体物化稳定的浏览器文件。",
        "解释 CLR 运行时目录存在的原因，helper 如何变为 System 模块，以及哪些护栏保持浏览器运行时导入可安全发布。",
        "记录 Jolt 从转型分支退役的结论、固定 baseline、历史能力范围和恢复入口。",
        "描述 RazorVue 作为构建时库模式，在共享语义、分析器宿主逻辑和库组件绑定之间划分所有权。",
        "解释 `ECMAScript.VueRoute` 作为独立的 Vue Router 绑定项目、其当前 API 切片，以及保持其独立于编译器套件增长的测试和打包边界。",
        "定义代码优先文档页面的所有权、源边界、生成资产审查和安全变更流程。",
        "描述本地构建输出、路由回退、运维检查和 Wiki 外壳的静态托管契约。",
        "解释验证层、聚焦命令、覆盖率期望，以及为什么冒烟验证是最低发布门槛。"
    ];

    // 页面标签集 / Page tag sets
    internal static readonly string[][] PageTagSets =
    [
        ["docs-shell", "routes", "overview"],
        ["search", "discovery", "query"],
        ["getting-started", "local-dev", "verification"],
        ["razor-sg", "razorvue", "architecture"],
        ["authoring", "metadata", "catalog"],
        ["navigation", "toc", "discovery"],
        ["information-architecture", "routes", "naming"],
        ["taxonomy", "index", "discovery"],
        ["glossary", "terms", "reference"],
        ["faq", "help", "workflow"],
        ["troubleshooting", "smoke", "runtime"],
        ["vue3", "h-function", "ui-authoring"],
        ["compiler", "estree", "semanticwalker"],
        ["compiler", "boundary", "lowering"],
        ["catalog", "routes", "metadata"],
        ["whitelist", "inline", "compile"],
        ["imports", "emit", "modules"],
        ["clr", "runtime", "system-modules"],
        ["jolt", "history", "retired"],
        ["razorvue", "library-mode", "build"],
        ["vueroute", "vue-router", "bindings"],
        ["ownership", "docs", "review"],
        ["hosting", "static-files", "fallback"],
        ["smoke", "tests", "verification"]
    ];

    // 页面正文分发函数 / Page body dispatch functions
    internal static readonly Func<IVNode>[] PageBodies =
    [
        OverviewBody,
        SearchBody,
        GettingStartedBody,
        ProjectLinesBody,
        ContentModelBody,
        NavigationDiscoveryBody,
        InformationArchitectureBody,
        TopicIndexBody,
        GlossaryBody,
        FaqBody,
        TroubleshootingBody,
        HFunctionAuthoringBody,
        CompilerOverviewBody,
        CompilerBoundaryBody,
        RouteCatalogContractBody,
        HostSemanticSeamsBody,
        ImportEmitContractBody,
        RuntimeCatalogBody,
        JoltHostBody,
        RazorVueLibraryModeBody,
        VueRouteBindingsBody,
        ContentGovernanceBody,
        DeploymentBody,
        TestingVerificationBody
    ];

    private static readonly string[] OverviewSectionIds =
    [
        "what-ships-now",
        "why-this-exists",
        "mvp-boundary",
        "site-structure",
        "registered-pages"
    ];

    // 概览页章节标题 / Overview page section titles
    private static readonly string[] OverviewSectionTitles =
    [
        "当前发布内容",
        "为什么存在",
        "MVP 边界",
        "站点结构",
        "已注册页面"
    ];

    private static readonly string[] SearchSectionIds =
    [
        "full-text",
        "section-hits",
        "topic-entry",
        "query-sharing"
    ];

    // 搜索页章节标题 / Search page section titles
    private static readonly string[] SearchSectionTitles =
    [
        "全文搜索",
        "章节匹配",
        "主题入口",
        "可分享的查询"
    ];

    private static readonly string[] GettingStartedSectionIds =
    [
        "boot-the-site",
        "route-model",
        "add-a-page",
        "verify-the-result"
    ];

    // 快速开始页章节标题 / Getting started page section titles
    private static readonly string[] GettingStartedSectionTitles =
    [
        "启动站点",
        "路由模型",
        "添加页面",
        "验证结果"
    ];

    private static readonly string[] ProjectLinesSectionIds =
    [
        "two-lines",
        "choose-a-path",
        "shared-core",
        "where-to-read-next"
    ];

    // 项目线路页章节标题 / Project lines page section titles
    private static readonly string[] ProjectLinesSectionTitles =
    [
        "两条活跃线路",
        "选择正确的路径",
        "共享核心",
        "下一步阅读"
    ];

    private static readonly string[] ContentModelSectionIds =
    [
        "page-contract",
        "navigation-contract",
        "editing-rules"
    ];

    // 内容模型页章节标题 / Content model page section titles
    private static readonly string[] ContentModelSectionTitles =
    [
        "页面契约",
        "导航契约",
        "编辑规则"
    ];

    private static readonly string[] HFunctionAuthoringSectionIds =
    [
        "layout-composition",
        "production-rules",
        "why-this-works"
    ];

    // H 函数编写页章节标题 / H-function authoring page section titles
    private static readonly string[] HFunctionAuthoringSectionTitles =
    [
        "布局组合",
        "生产规则",
        "为什么这样可行"
    ];

    private static readonly string[] NavigationDiscoverySectionIds =
    [
        "left-rail",
        "right-rail",
        "related-pages",
        "not-found-recovery",
        "authoring-implications"
    ];

    // 导航与发现页章节标题 / Navigation and discovery page section titles
    private static readonly string[] NavigationDiscoverySectionTitles =
    [
        "左侧导航发现",
        "右侧导航",
        "相关页面与阅读流",
        "404 恢复",
        "编写含义"
    ];

    private static readonly string[] InformationArchitectureSectionIds =
    [
        "concern-groups",
        "route-shape",
        "naming-rules",
        "ordering-rules",
        "growth-without-drift"
    ];

    // 信息架构页章节标题 / Information architecture page section titles
    private static readonly string[] InformationArchitectureSectionTitles =
    [
        "关注组",
        "路由形态",
        "命名规则",
        "排序与阅读流",
        "无漂移增长"
    ];

    private static readonly string[] TopicIndexSectionIds =
    [
        "topic-clusters",
        "core-runtime",
        "operating-and-writing"
    ];

    // 主题索引页章节标题 / Topic index page section titles
    private static readonly string[] TopicIndexSectionTitles =
    [
        "主题集群",
        "核心运行时与架构",
        "运维与编写"
    ];

    private static readonly string[] GlossarySectionIds =
    [
        "compiler-terms",
        "runtime-terms",
        "host-terms"
    ];

    // 术语表页章节标题 / Glossary page section titles
    private static readonly string[] GlossarySectionTitles =
    [
        "编译器术语",
        "运行时术语",
        "宿主与工作流术语"
    ];

    private static readonly string[] FaqSectionIds =
    [
        "using-jazor",
        "compiler-boundaries",
        "runtime-and-host",
        "wiki-workflow"
    ];

    // 常见问题页章节标题 / FAQ page section titles
    private static readonly string[] FaqSectionTitles =
    [
        "使用 Jazor",
        "编译器边界",
        "运行时与宿主行为",
        "Wiki 工作流"
    ];

    private static readonly string[] TroubleshootingSectionIds =
    [
        "route-and-host",
        "runtime-imports",
        "compiler-diagnostics",
        "workflow-fixes"
    ];

    // 故障排除页章节标题 / Troubleshooting page section titles
    private static readonly string[] TroubleshootingSectionTitles =
    [
        "路由与宿主问题",
        "运行时导入故障",
        "编译器与分析器诊断",
        "工作流修复"
    ];

    private static readonly string[] CompilerOverviewSectionIds =
    [
        "what-it-is",
        "core-pipeline",
        "hard-contracts",
        "read-this-next"
    ];

    // 编译器概览页章节标题 / Compiler overview page section titles
    private static readonly string[] CompilerOverviewSectionTitles =
    [
        "它是什么",
        "核心管线",
        "硬性契约",
        "下一步阅读"
    ];

    private static readonly string[] CompilerBoundarySectionIds =
    [
        "controlled-domain",
        "behavior-priority",
        "support-boundary",
        "stabilized-routes",
        "practical-reading"
    ];

    // 编译器支持边界页章节标题 / Compiler support boundary page section titles
    private static readonly string[] CompilerBoundarySectionTitles =
    [
        "受控输入域",
        "行为优先级",
        "支持边界",
        "稳定的语义路由",
        "实用阅读顺序"
    ];

    private static readonly string[] RouteCatalogContractSectionIds =
    [
        "single-source",
        "what-the-catalog-owns",
        "safe-change-flow",
        "failure-modes",
        "verification-contract"
    ];

    // 路由目录契约页章节标题 / Route catalog contract page section titles
    private static readonly string[] RouteCatalogContractSectionTitles =
    [
        "单一事实来源",
        "目录拥有什么",
        "安全变更流程",
        "应避免的失败模式",
        "验证契约"
    ];

    private static readonly string[] HostSemanticSeamsSectionIds =
    [
        "why-seams-exist",
        "choose-the-right-seam",
        "whitelist-contract",
        "inline-vs-compile",
        "verification-surface"
    ];

    // 宿主语义接缝页章节标题 / Host semantic seams page section titles
    private static readonly string[] HostSemanticSeamsSectionTitles =
    [
        "为什么存在接缝",
        "选择正确的接缝",
        "WhiteList 契约",
        "Inline 与 Compile 对比",
        "验证表面"
    ];

    private static readonly string[] ImportEmitContractSectionIds =
    [
        "boundary-split",
        "import-mainline",
        "layered-output",
        "host-materialization",
        "verification-signals"
    ];

    // 导入与发射契约页章节标题 / Import and emit contract page section titles
    private static readonly string[] ImportEmitContractSectionTitles =
    [
        "边界划分",
        "导入主线",
        "分层输出契约",
        "宿主物化",
        "验证信号"
    ];

    private static readonly string[] DeploymentSectionIds =
    [
        "build-output",
        "route-fallback",
        "operational-checks"
    ];

    // 部署页章节标题 / Deployment page section titles
    private static readonly string[] DeploymentSectionTitles =
    [
        "构建输出",
        "路由回退",
        "运维检查"
    ];

    private static readonly string[] ContentGovernanceSectionIds =
    [
        "ownership-model",
        "source-boundaries",
        "generated-assets",
        "change-flow",
        "release-discipline"
    ];

    // 内容治理页章节标题 / Content governance page section titles
    private static readonly string[] ContentGovernanceSectionTitles =
    [
        "所有权模型",
        "源边界",
        "生成资产",
        "安全变更流程",
        "发布纪律"
    ];

    private static readonly string[] TestingVerificationSectionIds =
    [
        "verification-layers",
        "focused-commands",
        "coverage-and-determinism",
        "wiki-release-gate"
    ];

    // 测试与验证页章节标题 / Testing and verification page section titles
    private static readonly string[] TestingVerificationSectionTitles =
    [
        "验证层",
        "聚焦命令",
        "覆盖率与确定性",
        "Wiki 发布门槛"
    ];

    private static readonly string[] RuntimeCatalogSectionIds =
    [
        "why-catalog-exists",
        "generation-pipeline",
        "runtime-contract",
        "operational-guardrails"
    ];

    // CLR 运行时目录页章节标题 / CLR runtime catalog page section titles
    private static readonly string[] RuntimeCatalogSectionTitles =
    [
        "目录存在的原因",
        "生成管线",
        "运行时契约",
        "运维护栏"
    ];

    private static readonly string[] JoltHostSectionIds =
    [
        "why-jolt",
        "subsystems",
        "run-modes",
        "when-to-choose-jolt"
    ];

    // Jolt 宿主页章节标题 / Jolt host page section titles
    private static readonly string[] JoltHostSectionTitles =
    [
        "退役结论",
        "历史能力范围",
        "历史恢复入口",
        "新项目如何选择"
    ];

    private static readonly string[] RazorVueLibraryModeSectionIds =
    [
        "why-razorvue",
        "physical-split",
        "build-time-flow",
        "when-to-choose-library-mode"
    ];

    // RazorVue 库模式页章节标题 / RazorVue library mode page section titles
    private static readonly string[] RazorVueLibraryModeSectionTitles =
    [
        "为什么存在 RazorVue",
        "物理拆分",
        "构建时流程",
        "何时选择库模式"
    ];

    private static readonly string[] VueRouteBindingsSectionIds =
    [
        "why-vueroute-exists",
        "current-surface",
        "authoring-boundary",
        "verification-path",
        "where-to-extend-next"
    ];

    // VueRoute 绑定页章节标题 / VueRoute bindings page section titles
    private static readonly string[] VueRouteBindingsSectionTitles =
    [
        "为什么存在 VueRoute 绑定",
        "当前表面",
        "编写边界",
        "验证路径",
        "下一步扩展方向"
    ];

    // 页面章节 ID 集合 / Page section ID sets
    internal static readonly string[][] PageSectionIdSets =
    [
        OverviewSectionIds,
        SearchSectionIds,
        GettingStartedSectionIds,
        ProjectLinesSectionIds,
        ContentModelSectionIds,
        NavigationDiscoverySectionIds,
        InformationArchitectureSectionIds,
        TopicIndexSectionIds,
        GlossarySectionIds,
        FaqSectionIds,
        TroubleshootingSectionIds,
        HFunctionAuthoringSectionIds,
        CompilerOverviewSectionIds,
        CompilerBoundarySectionIds,
        RouteCatalogContractSectionIds,
        HostSemanticSeamsSectionIds,
        ImportEmitContractSectionIds,
        RuntimeCatalogSectionIds,
        JoltHostSectionIds,
        RazorVueLibraryModeSectionIds,
        VueRouteBindingsSectionIds,
        ContentGovernanceSectionIds,
        DeploymentSectionIds,
        TestingVerificationSectionIds
    ];

    // 页面章节标题集合 / Page section title sets
    internal static readonly string[][] PageSectionTitleSets =
    [
        OverviewSectionTitles,
        SearchSectionTitles,
        GettingStartedSectionTitles,
        ProjectLinesSectionTitles,
        ContentModelSectionTitles,
        NavigationDiscoverySectionTitles,
        InformationArchitectureSectionTitles,
        TopicIndexSectionTitles,
        GlossarySectionTitles,
        FaqSectionTitles,
        TroubleshootingSectionTitles,
        HFunctionAuthoringSectionTitles,
        CompilerOverviewSectionTitles,
        CompilerBoundarySectionTitles,
        RouteCatalogContractSectionTitles,
        HostSemanticSeamsSectionTitles,
        ImportEmitContractSectionTitles,
        RuntimeCatalogSectionTitles,
        JoltHostSectionTitles,
        RazorVueLibraryModeSectionTitles,
        VueRouteBindingsSectionTitles,
        ContentGovernanceSectionTitles,
        DeploymentSectionTitles,
        TestingVerificationSectionTitles
    ];

    // 页面相关路径集合 / Page related path sets
    internal static readonly string[][] PageRelatedPathSets =
    [
        [SearchPath, GettingStartedPath, ProjectLinesPath],
        [TopicIndexPath, GlossaryPath, TroubleshootingPath],
        [ProjectLinesPath, NavigationDiscoveryPath, DeploymentPath],
        [RazorVueLibraryModePath, CompilerOverviewPath, GettingStartedPath],
        [NavigationDiscoveryPath, InformationArchitecturePath, ContentGovernancePath],
        [InformationArchitecturePath, TopicIndexPath, RouteCatalogContractPath],
        [TopicIndexPath, RouteCatalogContractPath, ContentModelPath],
        [GlossaryPath, CompilerOverviewPath, TroubleshootingPath],
        [TopicIndexPath, CompilerOverviewPath, RuntimeCatalogPath],
        [TroubleshootingPath, GettingStartedPath, ProjectLinesPath],
        [FaqPath, DeploymentPath, TestingVerificationPath],
        [ContentModelPath, CompilerOverviewPath, RouteCatalogContractPath],
        [CompilerBoundaryPath, HostSemanticSeamsPath, ImportEmitContractPath],
        [CompilerOverviewPath, HostSemanticSeamsPath, RuntimeCatalogPath],
        [NavigationDiscoveryPath, ContentModelPath, ContentGovernancePath],
        [ImportEmitContractPath, RuntimeCatalogPath, CompilerBoundaryPath],
        [RuntimeCatalogPath, DeploymentPath, TestingVerificationPath],
        [HostSemanticSeamsPath, ImportEmitContractPath, DeploymentPath],
        [ProjectLinesPath, RazorVueLibraryModePath, GettingStartedPath],
        [ProjectLinesPath, CompilerOverviewPath, HFunctionAuthoringPath],
        [ProjectLinesPath, RazorVueLibraryModePath, ImportEmitContractPath],
        [ContentModelPath, DeploymentPath, TestingVerificationPath],
        [TestingVerificationPath, TroubleshootingPath, RuntimeCatalogPath],
        [DeploymentPath, ContentGovernancePath, TroubleshootingPath]
    ];

    // 辅助方法：页面查询与索引 / Helper methods: page lookup and indexing
    private static int TotalPageCount => PagePaths.Length;

    private static bool IsKnownPage(string currentPath)
        => GetPageIndex(currentPath) >= 0;

    private static int GetPageIndex(string currentPath)
    {
        for (var pageIndex = 0; pageIndex < PagePaths.Length; pageIndex++)
        {
            if (PagePaths[pageIndex] == currentPath)
                return pageIndex;
        }

        return -1;
    }

    private static string GetPagePath(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < PagePaths.Length)
            return PagePaths[pageIndex];

        return "";
    }

    // 页面过滤与搜索 / Page filtering and search
    private static bool MatchesPageFilter(string currentPath, string filterText)
    {
        if (filterText.Length == 0)
            return true;

        if (currentPath.Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageGroup(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageTitle(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageSummary(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageStatus(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageOwner(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageAudience(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageSourceFile(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageLastUpdated(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageSearchBody(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var tags = GetPageTags(currentPath);
        for (var tagIndex = 0; tagIndex < tags.Length; tagIndex++)
        {
            if (tags[tagIndex].Contains(filterText, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    // 单字段 getter / Single-field getters
    private static string GetPageGroup(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageGroups.Length)
            return PageGroups[pageIndex];

        return "Unregistered";
    }

    private static string GetPageTitle(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageTitles.Length)
            return PageTitles[pageIndex];

        return "Unregistered page";
    }

    private static string GetPageSummary(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageSummaries.Length)
            return PageSummaries[pageIndex];

        return "The requested path is not part of the registered Wiki page catalog.";
    }

    private static string GetPageStatus(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageStatuses.Length)
            return PageStatuses[pageIndex];

        return "Not Found";
    }

    private static string GetPageOwner(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageOwners.Length)
            return PageOwners[pageIndex];

        return "Unknown";
    }

    private static string GetPageAudience(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageAudiences.Length)
            return PageAudiences[pageIndex];

        return "Unknown";
    }

    private static string GetPageSourceFile(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageSourceFiles.Length)
            return PageSourceFiles[pageIndex];

        return "";
    }

    private static string GetPageLastUpdated(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageLastUpdatedDates.Length)
            return PageLastUpdatedDates[pageIndex];

        return "";
    }

    private static int GetPageReadingMinutes(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageReadingMinutes.Length)
            return PageReadingMinutes[pageIndex];

        return 0;
    }

    private static string GetPageSearchBody(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageSearchBodies.Length)
            return PageSearchBodies[pageIndex];

        return "";
    }

    private static string[] GetPageTags(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageTagSets.Length)
            return PageTagSets[pageIndex];

        return [];
    }

    // 前后页面导航 / Previous/next page navigation
    private static string GetPreviousPath(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex < 0)
            return "";

        for (var previousIndex = pageIndex - 1; previousIndex >= 0; previousIndex--)
        {
            if (PagePaths[previousIndex] != SearchPath)
                return PagePaths[previousIndex];
        }

        return "";
    }

    private static string GetNextPath(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex < 0)
            return "";

        for (var nextIndex = pageIndex + 1; nextIndex < TotalPageCount; nextIndex++)
        {
            if (PagePaths[nextIndex] != SearchPath)
                return PagePaths[nextIndex];
        }

        return "";
    }

    // 页面章节与正文获取 / Page section and body retrieval
    private static string[] GetPageSectionIds(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < PageSectionIdSets.Length)
            return PageSectionIdSets[pageIndex];

        return [];
    }

    private static string[] GetPageSectionTitles(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < PageSectionTitleSets.Length)
            return PageSectionTitleSets[pageIndex];

        return [];
    }

    private static IVNode GetPageBody(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < PageBodies.Length)
            return PageBodies[pageIndex]();

        return H("div", new VueObject { Class = "doc-body" }, []);
    }

    private static string[] GetPageRelatedPaths(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < PageRelatedPathSets.Length)
            return PageRelatedPathSets[pageIndex];

        return [];
    }

    // 建议路径与 404 恢复 / Suggested paths and 404 recovery
    private static string[] GetSuggestedPaths(string currentPath)
    {
        var fragment = GetRouteFragment(currentPath);
        var suggestions = new List<string>();

        if (fragment.Length > 0)
        {
            for (var pageIndex = 0; pageIndex < PagePaths.Length; pageIndex++)
            {
                var pagePath = PagePaths[pageIndex];
                if (MatchesPageFilter(pagePath, fragment))
                    suggestions.Add(pagePath);
            }
        }

        if (suggestions.Count == 0)
        {
            var requestedGroup = GetRequestedGroup(currentPath);
            if (requestedGroup.Length > 0)
            {
                for (var pageIndex = 0; pageIndex < PagePaths.Length; pageIndex++)
                {
                    var pagePath = PagePaths[pageIndex];
                    if (GetPageGroup(pagePath) == requestedGroup)
                        suggestions.Add(pagePath);
                }
            }
        }

        if (suggestions.Count == 0)
            return [OverviewPath, SearchPath, TopicIndexPath];

        if (suggestions.Count > 3)
            suggestions.RemoveRange(3, suggestions.Count - 3);

        return suggestions.ToArray();
    }

    private static string GetRequestedGroup(string currentPath)
    {
        if (currentPath.StartsWith("/guides/", StringComparison.OrdinalIgnoreCase))
            return "Foundation";

        if (currentPath.StartsWith("/engineering/", StringComparison.OrdinalIgnoreCase))
            return "Engineering";

        if (currentPath.StartsWith("/operations/", StringComparison.OrdinalIgnoreCase))
            return "Operations";

        return "";
    }

    private static string GetRouteFragment(string currentPath)
    {
        var normalizedPath = currentPath.Trim('/');
        if (normalizedPath.Length == 0)
            return "";

        var lastSlashIndex = normalizedPath.LastIndexOf('/');
        if (lastSlashIndex >= 0 && lastSlashIndex < normalizedPath.Length - 1)
            return normalizedPath.Substring(lastSlashIndex + 1);

        return normalizedPath;
    }

    // 目录侧边栏渲染 / TOC rail rendering
    private static IVNode TocRail(string currentPath, string currentHash)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex < 0)
            return EmptyTocRail();

        var sectionIds = GetPageSectionIds(pageIndex);
        var sectionTitles = GetPageSectionTitles(pageIndex);
        var links = new List<IVNode>();
        for (var sectionIndex = 0; sectionIndex < sectionIds.Length && sectionIndex < sectionTitles.Length; sectionIndex++)
            links.Add(TocLink(currentPath, sectionIds[sectionIndex], sectionTitles[sectionIndex], currentHash));

        return TocRail("本页目录", links.ToArray());
    }
}
