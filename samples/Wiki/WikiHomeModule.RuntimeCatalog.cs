// WikiHomeModule.RuntimeCatalog.cs - CLR 运行时目录 / CLR Runtime Catalog
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建运行时目录页面主体 / Build the runtime catalog page body
    private static IVNode RuntimeCatalogBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("why-catalog-exists", "为什么存在目录",
            [
                H("p", "Wiki 现在消费与生产 Jazor 输出在浏览器中需要的相同 CLR 支持运行时表面。目录的存在使 `Jazor.CLR` 导入辅助函数成为显式的 `System/*` ESM 模块，而非编译器中的隐藏假设。"),
                H("ul",
                [
                    H("li", "浏览器入口点可以只导入它们实际使用的运行时辅助函数。"),
                    H("li", "生成的 `System/*` 模块在本地发射目录 `samples/Wiki/wwwroot/jazor/System/` 下保持可检查。"),
                    H("li", "文档站点通过真实的浏览器提供资源证明这条路径，而非仅通过编译器单元测试。")
                ])
            ]),
            PageSection("generation-pipeline", "生成管线",
            [
                H("p", "当前管线扫描 CLR 白名单声明，发射编译器白名单制品，刷新进程内白名单视图，然后将浏览器就绪的运行时模块物化到 ECMAScript 目录中。"),
                CodeBlock("当前管线接触点", """
src/Jazor.Compiler.Generator/Program.cs
src/Jazor.Compiler.Generator/ClrRuntimeCatalogEmitter.cs
src/Jazor.Compiler.Generator/ClrRuntimeSelection.cs
src/Jazor.Compiler/WhiteList.cs.Generate.cs
src/ECMAScript/Catalog.g.cs
samples/Wiki/wwwroot/jazor/System/
"""),
                H("p", "单次运行刷新很重要。新的 CLR 映射应该在同一次生成器调用中对运行时目录发射可见，而非仅在第二次通过之后。")
            ]),
            PageSection("runtime-contract", "运行时契约",
            [
                H("p", "契约刻意保持显式：浏览器入口 HTML 声明 `System/` import-map 前缀，发射的 Jazor 模块导入命名的运行时辅助函数，目录输出为这些辅助函数提供稳定的 ESM 导出。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Import map", "浏览器通过宿主 HTML 模板中的 `/jazor/System/` 解析 `System/*`。"),
                    CheckCard("命名导出", "生成的运行时模块暴露可调用的辅助函数导出和模块命名空间对象以保持导入稳定性。"),
                    CheckCard("本地资源", "Wiki 在开发时从项目本地发射目录提供运行时辅助函数，发布后从 `wwwroot/jazor/System/` 提供。")
                ])
            ]),
            PageSection("operational-guardrails", "操作护栏",
            [
                H("p", "生产安全来自构建和冒烟纪律，而非假设目录在重构后仍然正确。"),
                H("ul",
                [
                    H("li", "运行 CLR 目录生成器，保持生成的白名单和目录与源映射同步。"),
                    H("li", "保持围绕运行时导出形状和包装器导入行为的聚焦 Emit 测试。"),
                    H("li", "变更后验证 Wiki 仍然提供 `/jazor/System/*` 模块和 import-map 契约。")
                ]),
                Callout("实用规则", "如果一个新的 CLR 导入辅助函数无法在一次生成器运行中发射到运行时目录，开发者体验尚未达到生产就绪。")
            ])
        ]);
}
