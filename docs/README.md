# Jazor 文档中心

> Status: 活跃参考
> Positioning: 仓库级文档总入口，负责把目标、计划、状态快照和历史材料分流到正确目录。
> Note: 如果你要判断“现在是什么状态”，优先进入 `03-完成/*/status.md`；如果你要判断“为什么这样设计、应如何扩展”，优先进入 `01-目标/*`；历史目录默认不直接代表当前事实。

## 两条技术线路

| 线路 | 模式 | 说明 |
|------|------|------|
| **RazorVue** | 库模式 | Source Generator 驱动，不使用 .vue SFC，编译时转换 |
| **Jolt** | 全功能模式 | 类似 Vite，支持 .jazor + .vue SFC，LSP + HMR + Debug + Build |

## 导航

| 分类 | 说明 | 入口 |
|------|------|------|
| **目标** | 为什么做、解决什么问题、大致思路 | [01-目标/](./01-目标/README.md) |
| **计划** | WBS、里程碑、阶段拆分 | [02-计划/](./02-计划/README.md) |
| **完成** | 评审结果、状态快照 | [03-完成/](./03-完成/README.md) |
| **补充** | 继往开来、治理规则 | [04-补充/](./04-补充/README.md) |
| **遗弃** | 已废弃的历史材料 | [05-遗弃/](./05-遗弃/README.md) |

## 按项目结构对照

| 文档目录 | 对应源码 |
|---------|---------|
| **01-目标** | |
| `01-目标/ecmascript/` | `src/ECMAScript/` |
| `01-目标/compiler/` | `src/Jazor.Compiler/` 架构与规范 |
| `01-目标/compiler/semantic-walker/` | `src/Jazor.Compiler/` SemanticWalker |
| `01-目标/compiler/sourcemap/` | 跨模块 SourceMap |
| `01-目标/compiler/emit/` | `src/Jazor.Emit/` |
| `01-目标/clr/` | `src/Jazor.CLR/` + Generator |
| `01-目标/analyzer/` | `src/Jazor.Analyzer/` |
| `01-目标/webidl/` | `src/ECMAScript.WebIDL.Generator/` |
| `01-目标/razor/` | `src/Jazor.Razor/` |
| `01-目标/razorvue/` | `src/Jazor.RazorVue/`（库模式 + Vuetify） |
| `01-目标/jolt/` | `src/Jolt/`（全功能模式） |
| `01-目标/common/` | `src/Jazor.Common/` + `src/Jazor.Name/` |
| `01-目标/tools/` | `src/Jazor/` + VSCode + Test |
| **02-计划** | |
| `02-计划/jolt/` | `src/Jolt/`（Phase 1–7） |
| `02-计划/compiler/` | `src/Jazor.Compiler/` 实施清单 |
| **03-完成** | |
| `03-完成/jolt/` | `src/Jolt/` |
| `03-完成/razorvue/` | `src/Jazor.RazorVue/` |
| `03-完成/compiler/` | `src/Jazor.Compiler/` |
| `03-完成/emit/` | `src/Jazor.Emit/` |

## 快速入口

- 恢复工作 → [02-计划/workstream-dashboard.md](./02-计划/workstream-dashboard.md)
- Jolt 当前状态 → [03-完成/jolt/status.md](./03-完成/jolt/status.md)
- 编译器入口 → [01-目标/compiler/README.md](./01-目标/compiler/README.md)
- 编译器实现原则 → [src/Jazor.Compiler/ImplementationPrinciples.md](../src/Jazor.Compiler/ImplementationPrinciples.md)
- 编译器状态快照 → [03-完成/compiler/status.md](./03-完成/compiler/status.md)
- 白名单机制 → [01-目标/clr/](./01-目标/clr/README.md)
- RazorVue 库模式 → [01-目标/razorvue/](./01-目标/razorvue/README.md)
- Jolt 全功能模式 → [01-目标/jolt/](./01-目标/jolt/README.md)

## Compiler 主线

如果当前工作重点在 compiler，建议把入口分成三层来看：

1. 总路线与裁决原则 → [src/Jazor.Compiler/ImplementationPrinciples.md](../src/Jazor.Compiler/ImplementationPrinciples.md)
2. 架构与专题索引 → [01-目标/compiler/README.md](./01-目标/compiler/README.md)
3. 当前状态快照 → [03-完成/compiler/status.md](./03-完成/compiler/status.md)

当前已经固定下来的 compiler 主线摘要是：

- `tuple`：表达式组合 lowering
- `ref/out`：caller/callee 协议模拟
- `enum`：声明擦除 + 使用点常量化
- `interface`：纯契约，不发射 runtime artifact
- 成员类继承：支持 JS-compatible 子集
- 成员类构造函数重载：单 `constructor` + `$ctor_<hash>` helper + `arguments.length` dispatcher
