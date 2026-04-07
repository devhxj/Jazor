# Modules Bridge

> Status: active reference
> Positioning: Repository-level bridge for module-local operational docs and README availability.

## 目标

本文档负责桥接“模块级 operational docs”，不是拿来替代深度设计文档的。

它主要回答这些问题：

- 如果我是按项目模块在做事，第一眼该看哪份 README
- 哪些模块已经有深度局部文档，哪些主要还是靠 operational README
- 模块 README 和 repo-level 状态/计划文档该咋个衔接

## 模块入口

### Package / host entry

- [Jazor README](../../../src/Jazor/README.md)

### Compiler

- [Jazor.Compiler README](../../../src/Jazor.Compiler/README.md)
- [Jazor.Compiler 文档索引](../../../src/Jazor.Compiler/doc/README.md)

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
- [Jazor.Emit Docs](../../../src/Jazor.Emit/doc/README.md)
- [Jazor.EmitTest README](../../../src/Jazor.EmitTest/README.md)

说明：

- `Jazor.Emit` 是 materialisation / manifest / bundling 的入口
- `src/Jazor.Emit/doc/` 是 emit lane 的局部 deep-doc 入口
- `Jazor.EmitTest` 保留 emit-side regression coverage 的入口

### Razor substrate

- [Jazor.Razor README](../../../src/Jazor.Razor/README.md)

### RazorVue core

- [Jazor.RazorVue README](../../../src/Jazor.RazorVue/README.md)

### RazorVue analysis host

- [Jazor.RazorVue.Analysis README](../../../src/Jazor.RazorVue.Analysis/README.md)

### WebIDL generator

- [ECMAScript.WebIDL.Generator README](../../../src/ECMAScript.WebIDL.Generator/README.md)

## 当前状态

当前仓库主模块已经具备第一层 module-local operational README 覆盖。

维护规则：

- 新增模块时，优先在模块目录本地补一份薄 README，再决定要不要扩 repo-level bridge
- 后头如果又出现 discoverability gap，优先继续在本文补，不单独膨胀成新的 repo-level backlog 文档

## 使用规则

### 1. 先分清自己现在看的是哪一层

- 看当前推进阶段：回到 [docs/status/README.md](../../status/README.md)
- 看当前执行入口：回到 [docs/plans/README.md](../../plans/README.md)
- 看长期设计：回到 [docs/architecture/README.md](../README.md)
- 看模块内部操作说明：从本文跳进对应 README

### 2. 已有成熟局部文档集的模块，优先进入局部索引

目前最典型的是：

- `src/Jazor.Compiler/doc/`

### 3. 只有 operational README 的模块，通过本文保持可发现性

这类模块没必要为了挤进 repo-level 主阅读路径，就硬把文档搬家。
