# Jazor 文档中心

本文档中心用于说明 Jazor 的产品边界、技术架构、实施计划和验证状态。文档按用途分为目标、计划、完成、补充和遗弃五类，读者应优先阅读当前主线文档，再参考专题设计和状态快照。

## 当前主线

Jazor 当前的 Razor-to-Vue 生产链路如下：

```text
官方 Razor Source Generator
        -> 最终 Roslyn Compilation
        -> Jazor.Vue Hook 与 RazorVue 绑定
        -> Jazor.Compiler / SemanticWalker
        -> Vue render-function .mjs
        -> Jazor.Emit 物化或打包
```

`Jazor.Vue` 是独立的显式 opt-in 包。仅引用 `Jazor` 不会安装 Razor Hook、扫描 Razor 组件或生成 Vue render catalog。

当前生产输入是官方 Razor Source Generator 完成后的最终 `Compilation`。系统不依赖 `EnableRazorHostOutputs`、`RazorCodeDocument`、`RazorCSharpDocument`，也不对生成 C# 进行二次解析。

## 文档分类

| 分类 | 适用范围 | 入口 |
| --- | --- | --- |
| 目标 | 产品定位、设计目标和技术边界 | [01-目标](./01-目标/README.md) |
| 计划 | 当前实施计划、里程碑和验收要求 | [02-计划](./02-计划/README.md) |
| 完成 | 状态快照、评审结论和验证记录 | [03-完成](./03-完成/README.md) |
| 补充 | 文档治理和非阻塞性补充说明 | [04-补充](./04-补充/README.md) |
| 遗弃 | 不再指导当前实现的历史材料 | [05-遗弃](./05-遗弃/README.md) |

## 推荐阅读

| 主题 | 文档 |
| --- | --- |
| Razor-to-Vue 设计 | [01-目标/razorvue/README.md](./01-目标/razorvue/README.md) |
| ECMAScript.Style 目标与边界 | [01-目标/ecmascript.style/README.md](./01-目标/ecmascript.style/README.md) |
| ECMAScript.Style 完成状态 | [03-完成/ecmascript.style/status.md](./03-完成/ecmascript.style/status.md) |
| Vue 3 绑定设计 | [01-目标/ecmascript.vue3/vue3-balanced-design.md](./01-目标/ecmascript.vue3/vue3-balanced-design.md) |
| Vue 3 实施计划 | [02-计划/ecmascript.vue3/ECMAScript.Vue3.Authoring.ImplementationPlan.md](./02-计划/ecmascript.vue3/ECMAScript.Vue3.Authoring.ImplementationPlan.md) |
| Vue 3 当前状态 | [03-完成/ecmascript.vue3/status.md](./03-完成/ecmascript.vue3/status.md) |
| 架构与实施计划 | [02-计划/Jazor 架构转型开发计划.md](./02-计划/Jazor%20架构转型开发计划.md) |
| 最终 Compilation 决策 | [02-计划/RazorSgFinalDocument.G0.DecisionRecord.md](./02-计划/RazorSgFinalDocument.G0.DecisionRecord.md) |
| 编译器实现原则 | [../src/Jazor.Compiler/ImplementationPrinciples.md](../src/Jazor.Compiler/ImplementationPrinciples.md) |
| 编译器状态 | [03-完成/compiler/status.md](./03-完成/compiler/status.md) |
| Emit 状态 | [03-完成/emit/status.md](./03-完成/emit/status.md) |
| 文档治理 | [04-补充/documentation-governance.md](./04-补充/documentation-governance.md) |

## 源码对照

| 领域 | 主要源码位置 |
| --- | --- |
| 编译器 | `src/Jazor.Compiler/` |
| CLR 映射与白名单 | `src/Jazor.CLR/`、`src/Jazor.CLR.Generator/`、`src/Jazor.Compiler.Generator/` |
| 分析器 | `src/Jazor.Analyzer/` |
| Razor-to-Vue 实现 | `src/Jazor.RazorVue/`、`src/Jazor.RazorVue.Generator/` |
| Razor-to-Vue 包 | `src/Jazor.Vue/` |
| 物化与打包 | `src/Jazor.Emit/` |
| CSS-in-JS | `src/ECMAScript.Style/` |
| 共享契约 | `src/Jazor.Common/`、`src/ECMAScript.Contract/` |
| Vue 绑定 | `src/ECMAScript.Vue3/`、`src/ECMAScript.VueContract/` |
| 其他生态绑定 | `src/ECMAScript.Pinia/`、`src/ECMAScript.VueRoute/`、`src/ECMAScript.Vuetify/` |

## 解释规则

- 当前语义以源码、当前测试和本目录中的当前目标文档为准。
- `03-完成` 中的测试审计和历史快照只代表其记录时点，不自动代表当前通过状态。
- `05-遗弃` 及其他明确标注为历史的材料仅用于背景追溯，不参与当前设计和验收。
- 当文档与实现不一致时，应先修正文档的归属或状态，再引用其结论。
