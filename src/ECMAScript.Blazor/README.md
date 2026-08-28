# ECMAScript.Blazor

> 定位：面向 Blazor authoring 场景的标准 ECMAScript 模拟/投影扩展库。

`ECMAScript.Blazor` 的写法应与 [ECMAScript 的 `Math` 投影](../ECMAScript/internal/Math.cs)一致：只用公开 ECMAScript 协议表达额外的 host shape、成员名与调用形状。它不是 Blazor CLR mapping 的事实源；RazorVue 消费的 framework type/member mapping 由 `Jazor.CLR.Generator` 生成并由 `Jazor.CLR` 持有。

## 编写边界

- 使用公共 ECMAScript 协议：`[ECMAScript]`、`[ECMAScript("specifier")]`、`[ECMAScriptInline]`，以及 `Description` 等标准名称/投影元数据。
- 仅在确有额外作者 API 时，用模拟/投影扩展成员表达所需的 host shape；C# surface 只描述可由编译器直接降低的标准 ECMAScript 语义。
- 不使用内部 `[Jazor]` 或 `Op.*`，不作为 whitelist 生成输入，也不维护第二份 CLR member mapping 表。
- 不声明 `[ECMAScriptModule]`，不包含 C# 编写的 JavaScript runtime module、helper 实现或手写 `.mjs`。
- 不拥有 Razor renderer、Vue listener/callback framing 或组件运行时实现。

需要实际运行时语义时，投影只声明其公开 host contract；实现归 `Jazor.CLR`。例如需要复杂控制流、事件值捕获或可复用 helper 时，应由 `Jazor.CLR` 的 C# module 提供，并沿既有 runtime catalog/Emit 管道物化。

## 职责划分

| Owner | 职责 |
| --- | --- |
| `ECMAScript.Blazor` | 可选的标准 ECMAScript 模拟/投影扩展与公开 host contract；不持有 framework member key |
| `Jazor.CLR` | C# 编写的 JavaScript runtime module、helper、Blazor CLR mapping 和运行时语义 |
| `Jazor.Artifacts.RuntimeProviderCatalog` | 标准 runtime provider 的发布与物化；本项目不拥有或替代该职责 |
| `Jazor.RazorVue` | Razor 生成 C# 的 render/lifecycle lowering，以及 Vue listener/component framing |

## 交付边界

该程序集作为 `Jazor.Vue` NuGet 的 `lib/net11.0` payload 交付；应用不需要单独引用或复制扩展源码。`Jazor` 核心包不包含该程序集，也不因此引入 Blazor framework reference。没有额外扩展需求时，不为维持程序集存在而复制 CLR mapping。

## 相关文档

- [Jazor.CLR](../Jazor.CLR/README.md)
- [Jazor.Vue](../Jazor.Vue/README.md)
- [Blazor CLR 类型支持计划](../../docs/04-roadmap/blazor-clr-support-plan.md)
