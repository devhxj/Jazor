// WikiHomeModule.Faq.cs - 常见问题 / FAQ
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建常见问题页面主体 / Build the FAQ page body
    private static IVNode FaqBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("using-jazor", "使用 Jazor",
            [
                H("p", "问：新项目应该从 RazorVue 还是 Jolt 开始？"),
                H("p", "答：不要从 Jolt 开始。Jolt 已从转型分支退役；新工作进入 official Razor SG generated C# 到 Vue render-function `.mjs` 的主线，旧 `.jazor` host 只能从固定 baseline 维护。"),
                H("p", "问：Wiki 本身是否证明了 H-function 编写是生产安全的？"),
                H("p", "答：是的。当前的外壳、导航、路由回退和运行时模块导入都运行在与生产代码相同的 H-function 编写表面上。")
            ]),
            PageSection("compiler-boundaries", "编译器边界",
            [
                H("p", "问：为什么分析器有时比编译器更早报错？"),
                H("p", "答：这种不对称是有意设计。分析器允许在擦除位置更严格，使不受支持的具体外部类型更早暴露，而编译器仍在运行时敏感的 Lowering 点决定最终接受。"),
                H("p", "问：为什么不静默回退到原始 JavaScript？"),
                H("p", "答：因为不受支持的运行时敏感行为必须显式失败。静默的原始 JS 回退会侵蚀确定性，使支持边界无法推理。")
            ]),
            PageSection("runtime-and-host", "运行时与宿主行为",
            [
                H("p", "问：为什么 `System/*` 辅助函数是显式浏览器模块而非隐藏的运行时胶水？"),
                H("p", "答：因为生产输出必须是可检查、可导入和可冒烟验证的。显式模块保持浏览器契约可见。"),
                H("p", "问：为什么文档宿主在未知路由上仍然提供 HTML shell？"),
                H("p", "答：这样直接刷新和手动输入的 URL 仍然能启动 SPA 外壳，然后可以恢复到带有路由建议的未找到页面。")
            ]),
            PageSection("wiki-workflow", "Wiki 工作流",
            [
                H("p", "问：Wiki 是 CMS 吗？"),
                H("p", "答：不是。它是一个代码优先的文档产品，具有显式所有权、生成的浏览器制品和操作验证。"),
                H("p", "问：什么使文档变更完成？"),
                H("p", "答：源页面、中央路由目录、发射的浏览器输出和冒烟验证都需要一致，然后页面才被视为就绪。")
            ])
        ]);
}
