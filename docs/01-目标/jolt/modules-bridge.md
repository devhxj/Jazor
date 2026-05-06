# Modules Bridge

> Status: 活跃参考
> Positioning: 模块本地 operational docs 和 README 可用性的仓库级桥接。

## 目标

本文档负责桥接"模块本地 operational docs"，并非用于替代深度设计文档的。

它主要回答这些问题：

- 如果我是按项目模块在做事，第一眼该看哪份 README
- 哪些模块已经有深度局部文档，哪些主要还是靠 operational README
- 模块 README 和 repo-level 状态/计划文档如何衔接

## 模块入口

### Package / host entry

- [Jazor README](../../../src/Jazor/README.md)

### Compiler

- [Jazor.Compiler README](../../../src/Jazor.Compiler/README.md)
- [Jazor.Compiler 文档索引](../compiler/README.md)

### Compiler generator

- [Jazor.Compiler.Generator README](../../../src/Jazor.Compiler.Generator/README.md)

### Analyzer

- [Jazor.Analyzer README](../../../src/Jazor.Analyzer/README.md)

### CLR runtime

- [Jazor.CLR README](../../../src/Jazor.CLR/readme.md)

### Shared contracts

- [Jazor.Common README](../../../src/Jazor.Common/README.md)
- [Jazor.Name README](../../../src/Jazor.Name/README.md)

### CompilerTest

- [Jazor.CompilerTest README](../../../src/Jazor.CompilerTest/README.md)

### Emit / bundle pipeline

- [Jazor.Emit README](../../../src/Jazor.Emit/README.md)
- [Emit.Pipeline.Overview.md](../compiler/emit/Emit.Pipeline.Overview.md)
- [Jazor.EmitTest README](../../../src/Jazor.EmitTest/README.md)

说明：

- `Jazor.Emit` 是 materialisation / manifest / bundling 的入口
- `docs/01-目标/compiler/emit/` 是 emit lane 的 repo-level deep-doc 入口
- `Jazor.EmitTest` 保留 emit-side regression coverage 的入口

### RazorVue core

- [Jazor.RazorVue README](../../../src/Jazor.RazorVue/README.md)
- [RazorVue 文档入口](../razorvue/README.md)

### RazorVue analysis host

- [Jazor.Analyzer README](../../../src/Jazor.Analyzer/README.md)

### WebIDL generator

- [ECMAScript.WebIDL.Generator README](../../../src/ECMAScript.WebIDL.Generator/README.md)
- 遗留 [ECMAScript.WebIDL README](../../../src/ECMAScript.WebIDL/README.md) 已归档，仅供历史参考

## 当前状态

当前仓库主模块已经具备第一层 module-local operational README 覆盖。

维护规则：

- 新增模块时，优先在模块目录本地补充一份简明 README，再决定要不要扩 repo-level bridge
- 后续若再次出现 discoverability gap，优先继续在本文补，不单独膨胀成新的 repo-level backlog 文档

## 使用规则

### 1. 先分清自己现在看的是哪一层

- 看当前推进阶段：回到 [docs/02-计划/workstream-dashboard.md](../../02-计划/workstream-dashboard.md)
- 看长期设计：回到 [docs/01-目标/README.md](../README.md)
- 看模块内部操作说明：从本文跳进对应 README

### 2. 已有成熟局部文档集的模块，优先进入局部索引

目前最典型的是：

- `docs/01-目标/compiler/`

### 3. 只有 operational README 的模块，通过本文保持可发现性

这类模块无需为了纳入 repo-level 主阅读路径，就硬把文档搬家。
