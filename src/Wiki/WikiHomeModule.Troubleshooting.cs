// WikiHomeModule.Troubleshooting.cs - 故障排除 / Troubleshooting
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建故障排除页面主体 / Build the troubleshooting page body
    private static IVNode TroubleshootingBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("route-and-host", "路由与宿主问题",
            [
                H("p", "如果直接路由刷新失败或文档外壳加载无内容，在修改页面代码之前先检查宿主契约。"),
                H("ul",
                [
                    H("li", "确认 `Program.cs` 仍然通过统一的 Jazor helper 提供静态文件并回退到 HTML shell。"),
                    H("li", "确认 `/jazor/main.mjs` 可以从开发环境的本地发射目录解析。"),
                    H("li", "确认请求的路由已在 `WikiHomeModule.RouteContract.cs` 中注册。")
                ])
            ]),
            PageSection("runtime-imports", "运行时导入失败",
            [
                H("p", "如果浏览器提供的 `System/*` 辅助函数加载失败，问题通常出在 Emit 输出或 import-map 连接，而非页面正文。"),
                CodeBlock("检查以下路径", """
src/Wiki/host/index.template.html
src/Wiki/wwwroot/jazor/main.mjs
src/Wiki/wwwroot/jazor/System/
src/Jazor.Compiler.Generator/Program.cs
src/ECMAScript/Catalog.g.cs
"""),
                H("p", "重新构建项目并验证 import-map 前缀仍然指向 `/jazor/System/`。")
            ]),
            PageSection("compiler-diagnostics", "编译器与分析器诊断",
            [
                H("p", "当诊断信息提到不受支持的类型或成员时，判断问题是编写时分析、WhiteList 映射还是运行时敏感 Lowering。"),
                H("ul",
                [
                    H("li", "使用编译器概览和支持边界页面判断使用点是否应该工作。"),
                    H("li", "当成员应该被宿主映射但没有时，检查 `Jazor.CLR` 声明。"),
                    H("li", "保持分析器和编译器预期与文档边界一致，而非弱化失败模式。")
                ])
            ]),
            PageSection("workflow-fixes", "工作流修复",
            [
                H("p", "Wiki 最快的修复循环仍然保持操作导向和显式。"),
                CodeBlock("聚焦修复循环", """
dotnet build .\src\Wiki\Wiki.csproj -v minimal
dotnet run --file .\scripts\csharp\wiki-verify-smoke.cs -- --build
dotnet run --file .\scripts\csharp\wiki-serve.cs -- --build
"""),
                Callout("实用规则", "如果页面渲染正常但冒烟测试失败，将宿主或发射资源契约视为已损坏。不要仅因为正文在某个标签页中看起来正确就将页面标记为完成。")
            ])
        ]);
}
