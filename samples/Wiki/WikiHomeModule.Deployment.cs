// WikiHomeModule.Deployment.cs - 部署 / Deployment
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建部署页面主体 / Build the deployment page body
    private static IVNode DeploymentBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("build-output", "构建输出",
            [
                H("p", "Wiki 现在先将静态 ESM 模块发射到项目本地 `jazor/` 目录，然后在发布时将相同输出物化到 `wwwroot/jazor` 用于生产托管。"),
                CodeBlock("关键制品", """
samples/Wiki/wwwroot/jazor/main.mjs
samples/Wiki/wwwroot/jazor/components/wiki-home.mjs
samples/Wiki/wwwroot/jazor/jazor-manifest.json
""")
            ]),
            PageSection("route-fallback", "路由回退",
            [
                H("p", "宿主为真实文档路由和未知文档路径提供相同的前端外壳，但不会将它们坍缩为一个 HTTP 状态。已知路由保持 `200`，未知文档路径返回可恢复的 `404` 外壳并携带路由感知元数据。"),
                H("ul",
                [
                    H("li", "在开发环境中，`/jazor/*` 在查询 web 根目录之前从显式的项目本地发射挂载点解析。"),
                    H("li", "在发布输出中，`/jazor/*` 通过正常静态托管从 `wwwroot/jazor` 解析。"),
                    H("li", "未知文档路径仍然回退到前端入口页面，以便外壳可以建议恢复路由。"),
                    H("li", "首个 HTML 响应在客户端水合之前即携带路由正确的 `<title>`、描述、规范 URL、Open Graph 标签和 Twitter 标签。"),
                    H("li", "工具路由如 `/search` 刻意发射为 `noindex, nofollow`，而 `sitemap.xml` 仅列出规范内容页面。"),
                    H("li", "健康检查仍然是 `/health` 的真实后端端点。")
                ])
            ]),
            PageSection("operational-checks", "操作检查",
            [
                H("p", "Wiki 的最低发布纪律是构建、路由和入口验证。这是防止站点静默漂移回仅示例质量的关键。"),
                CodeBlock("推荐验证", """
dotnet run --file .\scripts\csharp\wiki-verify-smoke.cs -- --build-local
dotnet run --file .\scripts\csharp\wiki-verify-browser.cs -- --build-local
dotnet run --file .\scripts\csharp\wiki-verify-smoke.cs -- --publish
dotnet run --file .\scripts\csharp\wiki-verify-browser.cs -- --publish
dotnet run --file .\scripts\csharp\wiki-serve.cs -- --publish
"""),
                H("ul",
                [
                    H("li", "本地冒烟证明开发挂载从项目本地发射目录提供 `/jazor/*`。"),
                    H("li", "发布冒烟证明生产从 `wwwroot/jazor` 提供 `/jazor/*`，没有根目录阴影 `jazor/` 目录覆盖它，且首次响应元数据、robots 指令、sitemap 内容和安全头保持正确。"),
                    H("li", "浏览器验证证明挂载的外壳在 SPA 导航和有状态交互后仍匹配首次响应契约。"),
                    H("li", "发布预览启动实际的发布宿主，使手动浏览器检查可以使用与生产部署相同的目录形状。")
                ]),
                Callout("依赖说明", "Vue 3 在 `wwwroot/vendor/` 中本地供应。站点完全离线运行，无 CDN 依赖。")
            ])
        ]);
}
