# Razor-to-Vue 架构

> 适用范围：框架集成层中当前已实现的 Razor-to-Vue 方向。

## 生产输入与输出

Razor-to-Vue 是框架集成层中当前已实现的上层方向，建立在 Jazor 核心之上：生产输入来自官方 Razor Source Generator 完成后的最终 Roslyn `Compilation`，`Jazor.RazorVue` 在其中绑定生成的组件类型和 `BuildRenderTree` 操作，再调用 `Jazor.Compiler` 降低 C# 语义，产出 Vue render-function 模块。框架层的一般规则见 [框架集成层](./framework-integrations.md)。

```text
Razor 组件
  -> 官方 Razor Source Generator
  -> 最终 Compilation
  -> Jazor.RazorVue 组件绑定与 Vue framing
  -> Jazor.Compiler / SemanticWalker
  -> Vue render-function .mjs
  -> Jazor.Emit
```

生产路径以官方 Razor Source Generator 完成后的最终 `Compilation` 为输入，并直接生成 Vue render-function 模块。`EnableRazorHostOutputs`、`RazorCodeDocument`、`RazorCSharpDocument`、Razor IR、生成 SFC 和二次解析生成 C# 记录为其他表示层。

## 组件身份与导入契约

组件 lowering 只有一个组件身份边界。被 RazorVue 当作组件消费的类型必须满足以下全部条件：

1. 类型可赋值给 `Microsoft.AspNetCore.Components.ComponentBase`，直接或通过源码/库基类间接继承均可；
2. 类型实现 `ECMAScript.Vue.IVueComponent` 或其派生接口；
3. 类型声明组件导入描述：`[ECMAScriptModule("...")]` 或 `[ECMAScript("<specifier>")]`（导出名由 `[ECMAScriptName]` 给出）。

`IVueComponent<TProps>` / `IVueComponent<TProps, TSlots>` 是带类型化 props/slots 的可选增强契约，与非泛型 marker 共同构成组件入口。导入描述提供模块资格，组件 marker 提供组件身份；两种导入描述同时出现时 `[ECMAScriptModule]` 优先。direct render 和 library component import 仅消费同时满足全部入口条件的类型，Microsoft Blazor 内置 UI 组件通过显式 binding 接入。

## 包边界

| 包 | 责任 |
| --- | --- |
| `Jazor` | 编译器、运行时契约、分析器、Emit 与 MSBuild 基础能力 |
| `Jazor.Vue` | Razor 项目的显式 opt-in 包与 generator payload |
| `Jazor.RazorVue` | 最终 compilation 绑定、组件闭包、Vue artifact framing |
| `Jazor.Emit` | 物化 `.mjs`、source map、manifest、运行时资源和 bundle |

Razor-to-Vue 项目通过显式引用 `Jazor.Vue` 启用 Razor Hook 与组件扫描，详细配置见 [安装与配置](../03-guides/installation-and-configuration.md)。

## 降低原则

RazorVue 负责 Vue 特有边界：当前组件、`RenderTreeBuilder`、children-to-slot、组件 state、组件闭包和模块 framing。C# 表达式、成员访问、调用、临时变量、导入与 CLR 映射统一经过 `Jazor.Compiler` 翻译入口。

Razor 已负责校验未知参数、必需参数和参数类型不匹配；RazorVue 直接翻译已通过官方 Razor SG 绑定的生成 C#，不重复实现这些 Razor 编译器检查。

## 产物契约

- 每个组件生成确定性的 Vue render-function `.mjs` 模块。
- 组件模块、导入、组件标识、相对路径、内容哈希和 source map 锚点必须稳定。
- `.mjs`、`.mjs.map`、manifest 与 bundle 的写入属于 `Jazor.Emit`，RazorVue 只负责生成内容。
- `debug` 模式提供可调试模块和源映射；`release` 模式由 Netpack 产出浏览器包。
- 组件的开发期 HMR 与 SSR 是上层交付能力，官方 Razor SG 输入边界因此保持稳定。

实现级说明与聚焦测试位于 [Jazor.RazorVue README](../../src/Jazor.RazorVue/README.md)。
