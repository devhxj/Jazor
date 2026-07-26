# Jazor 文档中心

> Status: 活跃参考
> Positioning: 仓库级文档总入口，负责把目标、计划、状态快照和历史材料分流到正确目录。
> Note: 当前转型入口是 `02-计划/Jazor 架构转型开发计划.md`；`jolt/` 下的目标、计划和状态仅是历史资料。

## 技术线路

| 线路 | 模式 | 当前物理落点 | 说明 |
|------|------|-------------|------|
| **Razor-to-Vue 架构转型** | 当前唯一主线 | `src/Jazor.RazorVue/` + `src/Jazor.Analyzer/` + `src/Jazor.Compiler/` + `src/Jazor.Emit/` | 官方 Razor SG generated C# -> Roslyn `IOperation` -> Vue render-function `.mjs`；G0 已通过，Task 0.5 进行中 |
| **Jolt** | 历史（已退役） | Git 基线 `d68aecbb00b23aa35735c9a269b2e987c7815b05` | 退役提交 `3ee18679fbdf43c13e05d7bfac8857ddcebd19f9`；当前项目图无 Jolt |

## 导航

| 分类 | 说明 | 入口 |
|------|------|------|
| **目标** | 为什么做、解决什么问题、大致思路 | [01-目标/](./01-目标/README.md) |
| **计划** | WBS、里程碑、阶段拆分 | [02-计划/](./02-计划/README.md) |
| **完成** | 评审结果、状态快照 | [03-完成/](./03-完成/README.md) |
| **补充** | 治理规则、补充约束 | [04-补充/](./04-补充/README.md) |
| **遗弃** | 已废弃历史材料 | [05-遗弃/](./05-遗弃/README.md) |

## 按项目结构对照

| 文档目录 | 对应源码 |
|---------|---------|
| **01-目标** | |
| `01-目标/compiler/` | `src/Jazor.Compiler/` |
| `01-目标/compiler/emit/` | `src/Jazor.Emit/` |
| `01-目标/compiler/sourcemap/` | `src/Jazor.Common/SourceMaps/` + `src/Jazor.Emit/SourceMaps/` |
| `01-目标/clr/` | `src/Jazor.CLR/` + `src/Jazor.CLR.Generator/` + `src/Jazor.Compiler.Generator/` |
| `01-目标/analyzer/` | `src/Jazor.Analyzer/` |
| `01-目标/razor/` | historical Razor foundation boundary; current production Razor input flows through official Razor SG |
| `01-目标/razorvue/` | `src/Jazor.RazorVue/`（含 `RazorSdk/` 与 `Runtime/`） + `src/Jazor.Analyzer/` + `src/Jazor.Compiler/` + `src/Jazor.Emit/` |
| `01-目标/jolt/` | Jolt 历史目标材料；源码见 Git 基线 `d68aecbb00b23aa35735c9a269b2e987c7815b05` |
| `01-目标/csx/` | 历史探索材料；当前不作为转型主线 |
| `01-目标/common/` | `src/ECMAScript.Contract/` + `src/Jazor.Common/` |
| `01-目标/webidl/` | `src/ECMAScript.WebIDL.Generator/` |
| `01-目标/tools/` | `src/Jazor/`；Jolt VS Code 集成仅见历史基线 |
| `01-目标/ecmascript.vue3/` | `src/ECMAScript.Vue3/` |
| `01-目标/ecmascript.pinia/` | `src/ECMAScript.Pinia/` |
| **02-计划** | |
| `02-计划/ecmascript/` | `src/ECMAScript/` |
| `02-计划/ecmascript.vue3/` | `src/ECMAScript.Vue3/` |
| `02-计划/ecmascript.pinia/` | `src/ECMAScript.Pinia/` |
| `02-计划/wiki/` | `src/Wiki/` |
| `02-计划/jolt/` | Jolt 历史 WBS；源码见 Git 基线 |
| `02-计划/csx/` | 历史探索计划；当前不执行 |
| `02-计划/compiler/` | `src/Jazor.Compiler/` |
| `02-计划/jolt/razorvue-implementation/` | RazorVue/Jolt 历史交叉实施材料 |
| **03-完成** | |
| `03-完成/jolt/` | Jolt 历史完成与验证快照；源码见 Git 基线 |
| `03-完成/razorvue/` | RazorVue 线路的阶段性完成材料；物理源码已迁到 `Jazor.RazorVue`（含 `RazorSdk/`）/ `Jazor.Analyzer` / `ECMAScript.Vuetify` |
| `03-完成/compiler/` | `src/Jazor.Compiler/` |
| `03-完成/emit/` | `src/Jazor.Emit/` |
| `03-完成/ecmascript.vue3/` | `src/ECMAScript.Vue3/` |
| `03-完成/ecmascript.pinia/` | `src/ECMAScript.Pinia/` |
| `03-完成/wiki/` | `src/Wiki/` |

## 快速入口

- 当前架构转型计划 → [02-计划/Jazor 架构转型开发计划.md](./02-计划/Jazor%20架构转型开发计划.md)
- G0 决策记录 → [02-计划/RazorSgFinalDocument.G0.DecisionRecord.md](./02-计划/RazorSgFinalDocument.G0.DecisionRecord.md)
- 工作流总览 → [02-计划/workstream-dashboard.md](./02-计划/workstream-dashboard.md)
- ECMAScript Vue3 目标设计 → [01-目标/ecmascript.vue3/vue3-balanced-design.md](./01-目标/ecmascript.vue3/vue3-balanced-design.md)
- ECMAScript Vue3 模块映射规则 → [01-目标/ecmascript.vue3/vue3-module-mapping-rules.md](./01-目标/ecmascript.vue3/vue3-module-mapping-rules.md)
- ECMAScript Vue3 API 覆盖矩阵 → [01-目标/ecmascript.vue3/vue3-api-coverage-matrix.md](./01-目标/ecmascript.vue3/vue3-api-coverage-matrix.md)
- ECMAScript Vue3 映射细节 → [01-目标/ecmascript.vue3/vue3-mapping-details.md](./01-目标/ecmascript.vue3/vue3-mapping-details.md)
- ECMAScript Pinia 目标设计 → [01-目标/ecmascript.pinia/pinia-balanced-design.md](./01-目标/ecmascript.pinia/pinia-balanced-design.md)
- ECMAScript Pinia API 覆盖矩阵 → [01-目标/ecmascript.pinia/pinia-api-coverage-matrix.md](./01-目标/ecmascript.pinia/pinia-api-coverage-matrix.md)
- ECMAScript Pinia 剩余清单 → [02-计划/ecmascript.pinia/ECMAScript.Pinia.RemainingWorkChecklist.md](./02-计划/ecmascript.pinia/ECMAScript.Pinia.RemainingWorkChecklist.md)
- ECMAScript Pinia 当前状态 → [03-完成/ecmascript.pinia/status.md](./03-完成/ecmascript.pinia/status.md)
- ECMAScript Vue3 落地计划 → [02-计划/ecmascript.vue3/ECMAScript.Vue3.Authoring.ImplementationPlan.md](./02-计划/ecmascript.vue3/ECMAScript.Vue3.Authoring.ImplementationPlan.md)
- ECMAScript Vue3 当前状态 → [03-完成/ecmascript.vue3/status.md](./03-完成/ecmascript.vue3/status.md)
- Wiki 阶段计划 → [02-计划/wiki/Wiki.Phases.md](./02-计划/wiki/Wiki.Phases.md)
- Wiki 当前状态 → [03-完成/wiki/status.md](./03-完成/wiki/status.md)
- 编译器实现原则 → [../src/Jazor.Compiler/ImplementationPrinciples.md](../src/Jazor.Compiler/ImplementationPrinciples.md)
- 编译器状态快照 → [03-完成/compiler/status.md](./03-完成/compiler/status.md)
- RazorVue 设计入口 → [01-目标/razorvue/README.md](./01-目标/razorvue/README.md)
- Jolt 历史设计 → [01-目标/jolt/README.md](./01-目标/jolt/README.md)
- Jazor CSX Frontend 历史探索 → [01-目标/csx/README.md](./01-目标/csx/README.md)
- Jolt 历史状态快照 → [03-完成/jolt/status.md](./03-完成/jolt/status.md)
- Emit 当前状态 → [03-完成/emit/status.md](./03-完成/emit/status.md)
