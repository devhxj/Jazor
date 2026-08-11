// WikiHomeModule.TestingVerification.cs - 测试与验证 / Testing & Verification
using ECMAScript;
using static ECMAScript.Vue;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建测试与验证页面主体 / Build the testing & verification page body
    private static IVNode TestingVerificationBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("verification-layers", "验证层",
            [
                H("p", "生产契约由多个测试层保护，而非一个过大的套件。每层回答不同的失败模式。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("编译器回归", "`Jazor.CompilerTest` 锁定语义 Lowering、导入/头稳定性、命名和 Source Map 或目录确定性。"),
                    CheckCard("Emit 回归", "`Jazor.EmitTest` 检查 Bundle 和文件物化行为，而非仅信任编译器输出。"),
                    CheckCard("操作冒烟", "`scripts/csharp/wiki-verify-smoke.cs` 证明发射资源、路由回退、首次响应元数据、发现文档、响应头、浏览器入口连线和静态托管仍作为真实站点行为。"),
                    CheckCard("浏览器运行时", "`scripts/csharp/wiki-verify-browser.cs` 驱动无头 Edge 会话通过挂载、SPA 导航、搜索、未找到恢复、持久化外壳状态、Hash 路由、元数据同步和移动端抽屉行为。")
                ])
            ]),
            PageSection("focused-commands", "聚焦命令",
            [
                H("p", "验证应保持聚焦。运行能证明已变更契约的最小命令，仅在风险表面增长时扩展。"),
                CodeBlock("典型命令集", """
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter SemanticWalkerReferenceTest
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj
dotnet run --file ./scripts/csharp/test-dotnet.cs
dotnet run --file ./scripts/csharp/test-dotnet.cs -- --project wiki
dotnet run --file ./scripts/csharp/test-dotnet.cs -- --project wiki-publish
dotnet run --file ./scripts/csharp/test-dotnet.cs -- --project wiki-browser
dotnet run --file ./scripts/csharp/test-dotnet.cs -- --project wiki-browser-publish
dotnet run --file ./scripts/csharp/wiki-verify-smoke.cs -- --build-local
dotnet run --file ./scripts/csharp/wiki-verify-browser.cs -- --build-local
"""),
                H("ul",
                [
                    H("li", "在迭代一个 Lowering 路由或一个契约族时，使用聚焦的 `--filter` 运行。"),
                    H("li", "当变更跨越编译器、Emit、CLR 或宿主边界时，使用仓库测试脚本。"),
                    H("li", "当变更涉及生成的浏览器资源、路由注册或托管行为时，使用 Wiki 冒烟测试。"),
                    H("li", "当变更涉及历史路由、Hash 导航、剪贴板流、`localStorage`、焦点/实时区域行为或移动端抽屉时，使用 Wiki 浏览器验证。")
                ])
            ]),
            PageSection("coverage-and-determinism", "覆盖率与确定性",
            [
                H("p", "活跃的测试纪律不仅关于行覆盖率。它还保护下游工具依赖的确定性输出表面。"),
                H("ul",
                [
                    H("li", "行为契约优先：在断言文本形状之前锁定可观察的 Lowering 行为。"),
                    H("li", "如果变更影响源起源、Source Map、目录或输出文本，添加匹配的 `SourceOrigin`、`SourceMap` 或 `ESGenerator` 回归。"),
                    H("li", "如果变更添加辅助函数、重载分派器或合成临时变量，证明名称保持稳定且不会随遍历顺序漂移。"),
                    H("li", "覆盖率设置通过 `coverlet.runsettings` 保持为套件的一部分，但确定性输出被视为产品契约，不仅是测试便利。")
                ])
            ]),
            PageSection("wiki-release-gate", "Wiki 发布门槛",
            [
                H("p", "对于 `jazor.wiki`，发布就绪是操作性的。面向浏览器的外壳必须按照声明精确地构建、挂载、路由和提供预期资源。"),
                H("ul",
                [
                    H("li", "构建输出必须包含 `samples/Wiki/wwwroot/jazor/main.mjs`、`components/wiki-home.mjs` 和 `jazor-manifest.json`。"),
                    H("li", "已注册文档路由必须返回 HTTP 200 并携带外壳、`#app`、`/jazor/main.mjs` 和 `System/` import-map 前缀。"),
                    H("li", "浏览器资源如 `/jazor/System/StringModule.js`、`/site.css` 和 `/favicon.svg` 必须成功解析。"),
                    H("li", "每个路由的首次 HTML 响应在 SPA 水合之前必须已携带预期的标题、描述、robots 指令、规范 URL、社交元数据和基线安全头。"),
                    H("li", "发现文档必须保持一致：`robots.txt` 必须公告 sitemap，`sitemap.xml` 必须排除工具路由如 `/search`。"),
                    H("li", "无头浏览器验证必须证明真实挂载、SPA 路由转换、搜索/未找到恢复、持久化外壳状态、复制功能、Hash 路由和移动端抽屉行为无控制台或运行时错误。"),
                    H("li", "搜索路由和未知文档路由必须发射 `noindex, nofollow`，而非静默伪装为规范页面。"),
                    H("li", "发布验证必须证明 `wwwroot/jazor` 提供生产资源，且没有根目录阴影 `jazor/` 目录存活以覆盖该契约。")
                ]),
                Callout("实用规则", "如果单元测试通过但 Wiki 冒烟契约或无头浏览器契约回归，编译器或 Emit 变更尚未准备好用于生产。")
            ])
        ]);
}
