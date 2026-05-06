# Jazor 文档中心

> Status: 活跃参考
> Positioning: 仓库级文档总入口，负责把目标、计划、状态快照和历史材料分流到正确目录。
> Note: 判断“现在是什么状态”，优先读 `03-完成/*/status.md`；判断“为什么这样设计、应该如何扩展”，优先读 `01-目标/*`。

## 两条技术线路

| 线路 | 模式 | 当前物理落点 | 说明 |
|------|------|-------------|------|
| **RazorVue** | 库模式 | `src/Jazor.RazorVue/`（含 `RazorSdk/`） + `src/Jazor.Analyzer/RazorVue/` + `src/ECMAScript.Vuetify/` | 编译时 Razor-to-JS，Source Generator 驱动 |
| **Jolt** | 全功能模式 | `src/Jolt/` | `.jazor` 开发时宿主，承载编辑器、预览、构建和调试链路 |

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
| `01-目标/razor/` | `src/Jazor.Razor/` + `src/Jazor.Compiler.Razor/` |
| `01-目标/razorvue/` | `src/Jazor.RazorVue/`（含 `RazorSdk/`） + `src/Jazor.Analyzer/RazorVue/` + `src/ECMAScript.Vuetify/` |
| `01-目标/jolt/` | `src/Jolt/` |
| `01-目标/common/` | `src/ECMAScript.Contract/` + `src/Jazor.Common/` |
| `01-目标/webidl/` | `src/ECMAScript.WebIDL.Generator/` |
| `01-目标/tools/` | `src/Jazor/` + `src/Jolt.VSCodeExtension/` + 相关测试/工具项目 |
| `01-目标/ecmascript.vue3/` | `src/ECMAScript.Vue3/` |
| **02-计划** | |
| `02-计划/ecmascript/` | `src/ECMAScript/` |
| `02-计划/ecmascript.vue3/` | `src/ECMAScript.Vue3/` |
| `02-计划/wiki/` | `src/Wiki/` |
| `02-计划/jolt/` | `src/Jolt/` |
| `02-计划/compiler/` | `src/Jazor.Compiler/` |
| `02-计划/jolt/razorvue-implementation/` | RazorVue 线路 + `src/Jolt/` 的交叉实施材料 |
| **03-完成** | |
| `03-完成/jolt/` | `src/Jolt/` |
| `03-完成/razorvue/` | RazorVue 线路的阶段性完成材料；物理源码已迁到 `Jazor.RazorVue`（含 `RazorSdk/`）/ `Jazor.Analyzer` / `ECMAScript.Vuetify` |
| `03-完成/compiler/` | `src/Jazor.Compiler/` |
| `03-完成/emit/` | `src/Jazor.Emit/` |
| `03-完成/ecmascript.vue3/` | `src/ECMAScript.Vue3/` |

## 快速入口

- 恢复工作 → [02-计划/workstream-dashboard.md](./02-计划/workstream-dashboard.md)
- ECMAScript Vue3 目标设计 → [01-目标/ecmascript.vue3/vue3-balanced-design.md](./01-目标/ecmascript.vue3/vue3-balanced-design.md)
- ECMAScript Vue3 模块映射规则 → [01-目标/ecmascript.vue3/vue3-module-mapping-rules.md](./01-目标/ecmascript.vue3/vue3-module-mapping-rules.md)
- ECMAScript Vue3 API 覆盖矩阵 → [01-目标/ecmascript.vue3/vue3-api-coverage-matrix.md](./01-目标/ecmascript.vue3/vue3-api-coverage-matrix.md)
- ECMAScript Vue3 映射细节 → [01-目标/ecmascript.vue3/vue3-mapping-details.md](./01-目标/ecmascript.vue3/vue3-mapping-details.md)
- ECMAScript Vue3 落地计划 → [02-计划/ecmascript.vue3/ECMAScript.Vue3.Authoring.ImplementationPlan.md](./02-计划/ecmascript.vue3/ECMAScript.Vue3.Authoring.ImplementationPlan.md)
- ECMAScript Vue3 当前状态 → [03-完成/ecmascript.vue3/status.md](./03-完成/ecmascript.vue3/status.md)
- Wiki 阶段计划 → [02-计划/wiki/Wiki.Phases.md](./02-计划/wiki/Wiki.Phases.md)
- 编译器实现原则 → [../src/Jazor.Compiler/ImplementationPrinciples.md](../src/Jazor.Compiler/ImplementationPrinciples.md)
- 编译器状态快照 → [03-完成/compiler/status.md](./03-完成/compiler/status.md)
- RazorVue 设计入口 → [01-目标/razorvue/README.md](./01-目标/razorvue/README.md)
- Jolt 设计入口 → [01-目标/jolt/README.md](./01-目标/jolt/README.md)
- Jolt 当前状态 → [03-完成/jolt/status.md](./03-完成/jolt/status.md)
- Emit 当前状态 → [03-完成/emit/status.md](./03-完成/emit/status.md)
