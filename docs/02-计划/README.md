# 计划

WBS、里程碑、阶段拆分，以及各工作流的当前执行进度。

## 文件索引

| 文件 | 说明 |
|------|------|
| `Jazor 架构转型开发计划.md` | 当前唯一 Razor-to-Vue 主线计划：官方 Razor SG generated C# -> Roslyn `IOperation` -> Vue render-function `.mjs` |
| `RazorSgFinalDocument.G0.DecisionRecord.md` | G0 决策与证据记录；确认最终生成文档和 callback compilation 派生链 |
| `workstream-dashboard.md` | 全局工作流总览（依赖、并行策略、执行门槛） |
| `jazor-component-runtime-plan-2026-07-06.md` | 历史 Component Runtime 探索，不是当前执行主线 |

## 当前主线定位

当前转型分支的唯一 Razor-to-Vue 主线是：官方 Razor Source Generator 最终生成文档（generated C#） -> Roslyn `IOperation` -> Vue render-function `.mjs`。G0 已通过，Task 0.5 正在清理旧入口并锁定项目图。

以下旧路径已经退役，不是旁路或 fallback：

- 旧 Razor IR/SFC 管线仅保留历史材料，生产输入必须是官方 Razor SG generated C#。
- Jolt 已在 `3ee18679fbdf43c13e05d7bfac8857ddcebd19f9` 从当前分支退役；维护与比较使用基线 `d68aecbb00b23aa35735c9a269b2e987c7815b05`。

## 按项目结构索引

| 目录 | 对应源码 | 内容 |
|------|---------|------|
| `ecmascript/` | `src/ECMAScript/` | ECMAScript 平台内核相关执行级计划 |
| `ecmascript.vue3/` | `src/ECMAScript.Vue3/` | ECMAScript.Vue3 外部库 authoring surface 的执行级落地计划 |
| `ecmascript.vben/` | `src/ECMAScript.Vben/` | 后台壳层核心、首个 UI 适配闭环与后续扩展顺序的执行级计划 |
| `ecmascript.pinia/` | `src/ECMAScript.Pinia/` | ECMAScript.Pinia 外部库 authoring surface 的执行级收口清单 |
| `ecmascript.vuetify/` | `src/ECMAScript.Vuetify/` | ECMAScript.Vuetify 作为 Vuetify 代理层与后续 RazorVue authoring 消费面的执行级收口清单与组件覆盖矩阵 |
| `wiki/` | `src/Wiki/` | `jazor.wiki` sample 的阶段划分、收口计划与产品化分流边界 |
| `jolt/` | Git 基线 `d68aecbb00b23aa35735c9a269b2e987c7815b05` | 冻结的 Jolt Phase/WBS 历史，不是当前执行计划 |
| `csx/` | 无当前源码落点 | 历史 TSX-like `.jazor` 前端探索，不是当前路线 |
| `jolt/razorvue-implementation/` | Jolt/RazorVue 历史交叉材料 | 冻结的模板前端迁移、HMR/桥接探索，不代表当前生产代码所有权 |
| `compiler/` | `src/Jazor.Compiler/` | 编译管线实施清单、转换路线图、SourceMap 实施清单 |
