# Modules Bridge

## 目标

桥接模块本地 operational docs 和仓库级可发现性。回答三个问题：

- 按项目模块在做事，第一眼看哪份 README
- 哪些模块有深度局部文档，哪些靠 operational README
- 模块 README 和 repo-level 状态/计划文档怎么衔接

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

`Jazor.Emit` 是 materialisation / manifest / bundling 的入口；`docs/01-目标/compiler/emit/` 是 emit lane 的 repo-level deep-doc 入口；`Jazor.EmitTest` 保留 emit-side regression coverage 的入口。

### RazorVue core

- [Jazor.RazorVue README](../../../src/Jazor.RazorVue/README.md)
- [RazorVue 文档入口](../razorvue/README.md)

### RazorVue analysis host

- [Jazor.Analyzer README](../../../src/Jazor.Analyzer/README.md)

### WebIDL generator

- [ECMAScript.WebIDL.Generator README](../../../src/ECMAScript.WebIDL.Generator/README.md)
- 遗留 [ECMAScript.WebIDL README](../../../src/ECMAScript.WebIDL/README.md) 已归档，仅供历史参考

## 状态

仓库主模块已具备第一层 module-local operational README 覆盖。

维护规则：

- 新增模块时，优先在模块目录本地补充一份简明 README，再决定要不要扩 repo-level bridge
- 后续若出现 discoverability gap，优先继续在本文补，不单独膨胀成新的 repo-level backlog 文档

## 使用规则

看推进阶段：[docs/02-计划/workstream-dashboard.md](../../02-计划/workstream-dashboard.md)
看长期设计：[docs/01-目标/README.md](../README.md)
看模块内部操作说明：从上面的入口跳进对应 README

已有成熟局部文档集的模块（如 `docs/01-目标/compiler/`），优先进入局部索引。只有 operational README 的模块，通过本文保持可发现性，无需为了纳入 repo-level 主阅读路径就硬把文档搬家。
