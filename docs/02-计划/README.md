# 实施计划

本目录记录当前工作流的实施计划、里程碑、验收标准和阶段性任务。计划文档用于组织工作，不替代源码、测试或最终设计契约。

## 当前主线

当前 Razor-to-Vue 主线是：

```text
官方 Razor Source Generator 最终 Compilation
    -> Jazor.Vue Hook
    -> Roslyn Operation 绑定
    -> Jazor.Compiler 语义降低
    -> Vue render-function .mjs
    -> Jazor.Emit 物化或 bundle
```

`Jazor.Vue` 的包边界、Hook 行为和 catalog 生成方式属于当前产品契约；`JazorMode` 仅控制物化模式，不控制 Hook 是否启用。

## 当前计划文档

| 文档 | 用途 |
| --- | --- |
| [Jazor 发布路线图](./ReleaseRoadmap.md) | `0.3` 到 `1.0` 的版本门槛、HMR/调试/性能阶段与验收口径 |
| [Jazor 架构转型开发计划](./Jazor%20架构转型开发计划.md) | 主线计划、阶段目标和依赖关系 |
| [Razor SG Final-Document G0 决策记录](./RazorSgFinalDocument.G0.DecisionRecord.md) | 最终 Compilation 输入边界的决策与证据 |
| [ECMAScript 显式命名迁移计划](./compiler/ECMAScriptNamingMigrationPlan.md) | 取消隐式 PascalCase 到 lowerCamelCase fallback 的执行顺序、Gate 与验收 |
| [Jazor SSR 实施计划](./ssr/Jazor.Ssr.ImplementationPlan.md) | ASP.NET Core SSR、临时 Deno 后端与未来 Jint + Netpack 替换边界 |
| `razorvue-transition/` | Razor-to-Vue 路线、WBS、验收和状态分片 |
| `compiler/` | 编译器实施清单、转换闭包和源映射计划 |
| `ecmascript/` | ECMAScript 平台执行级计划 |
| `ecmascript.vue3/` | Vue 3 绑定与 authoring surface 计划 |
| `ecmascript.pinia/` | Pinia 绑定收口计划 |
| `ecmascript.vuetify/` | Vuetify 组件覆盖和收口计划 |
| [ecmascript.style/](./ecmascript.style/ECMAScript.Style.ImplementationPlan.md) | 强类型 C# CSS-in-JS 的公共 API、运行时、集成与发布实施计划 |
| `jazor.admin/` | 管理后台壳层实施计划 |
| `wiki/` | Wiki sample 的阶段计划 |
| `workstream-dashboard.md` | 工作流依赖和当前执行概览 |

## 计划使用规则

- 计划必须明确目标、前置条件、验收标准和对应测试入口。
- 已完成事项应转入 `03-完成` 的状态或评审文档，并保留可复核证据。
- 过时计划不得继续作为当前实现依据，应移入 `05-遗弃` 或在原文档中明确标注冻结状态。
- 任何计划结论都必须与当前源码和测试结果重新核对后才能用于发布说明。
