# Emit Pipeline 概述

## 1. 目的

本文档解释 `Jazor.Emit` 在 Jazor 程序中的角色，是本地 emit doc set 的顶层概述。

它回答四个问题：

1. `Jazor.Emit` 消费什么
2. 它写什么
3. RazorVue 和 SourceMap 如何通过 emit 继续
4. 哪些职责仍然属于其他地方

## 2. 模块位置

`Jazor.Emit` 是 host-facing 物化层，位于 compiler-side catalog 生成之后。

划分：

- compiler-side modules 生成 catalog 和 artifact 数据
- `Jazor.Emit` 从已构建的程序集读取这些编译后的 carriers
- `Jazor.Emit` 将文件和 manifests 写入输出树
- `Jazor.Emit` 可以通过 `DenoHost` 组装最终 bundle

`Jazor.Emit` 不是 compile-time 语义的所有者。它拥有：

- load/read/materialize/write flow
- manifest 持久化
- bundle 工作空间组装
- emit-side SourceMap 写入

## 3. 主流程

pipeline：

1. 在 `Program.cs` 中解析 CLI 选项
2. 通过 `EmitLoadContext` 加载根和引用程序集
3. 通过 `ModuleCollector` 收集常规模块 catalogs 和 RazorVue catalogs
4. 通过 `ModuleWriter` 写入常规模块
5. 通过 `RazorVueModuleWriter` 写入 RazorVue 模块、manifests 和模块级 `.map` 文件
6. 可选地通过 `ModuleBundler` bundle emitted 模块

emit lane 已经是以下内容的组合路径：

- 常规 ECMAScript 模块输出
- RazorVue 制品输出
- SourceMap sidecar 输出
- 最终 bundle 组装

## 4. 核心组件

更窄的后续文档：

- [Emit.Materialization.Overview.md](./Emit.Materialization.Overview.md)
- [Emit.BundleAndSourceMap.Overview.md](./Emit.BundleAndSourceMap.Overview.md)

### 4.1 加载和收集

- `EmitLoadContext.cs`
- `ModuleCollector.cs`
- `CatalogReader.cs`
- `RazorVueCatalogReader.cs`

此层加载已构建的程序集并提取 compiler-owned 生成的 carriers。

关键规则：路径冲突在收集时检测，在写入输出之前。

### 4.2 Manifest 和模块写入

- `ManifestModel.cs`
- `ModuleWriter.cs`
- `RazorVueManifestModel.cs`
- `RazorVueModuleWriter.cs`

此层将收集的记录转换为具体输出文件。

行为包括：

- 通过比较 manifest hash 状态跳过未更改的文件
- 在启用 `clean` 时清理已移除的输出
- 保持常规模块和 RazorVue manifest 演进并行，而不是将它们折叠成一个模糊的 carrier

### 4.3 Bundle 组装

- `BundleOptions.cs`
- `ModuleBundler.cs`

此层准备临时 bundle 工作空间，根据需要重写 intra-graph imports，并调用 `DenoHost` 进行最终 bundling。

边界：bundling 保持在 emit 中，绝不能泄漏回编译器语义所有权。

### 4.4 SourceMap 继续

- `SourceMaps/SourceMapBuilder.cs`
- `SourceMaps/SourceMapWriter.cs`

SourceMap 位置：

- 模块级 map 写入已经位于 emit 中
- RazorVue 的 SourceMap 继续是一个活跃的狭窄 lane
- 更广泛的 SourceMap 程序仍然比此活跃切片更保守

## 5. 边界

### 5.1 emit 拥有什么

- 文件系统物化
- manifest 持久化
- emitted 文件布局
- RazorVue 制品继续到文件
- emit-side map 生成
- bundle 编排

### 5.2 emit 不拥有什么

- 编译器 lowering 规则
- RazorVue 描述符语义
- Roslyn generator 入口逻辑
- repo-level 排序和工作流策略

这些仍在：

- `Jazor.Compiler`
- `Jazor.RazorVue`
- `Jazor.RazorVue.Analysis`
- repo-level `docs/03-完成/` 和 `docs/02-计划/`

## 6. 推荐阅读

如果你正在处理 emit 本身，按以下顺序阅读：

1. `src/Jazor.Emit/README.md`
2. 本文档
3. `Emit.Materialization.Overview.md` 或 `Emit.BundleAndSourceMap.Overview.md`
4. `src/Jazor.EmitTest/README.md`
5. `docs/03-完成/emit/status.md`

然后仅在需要时进入相邻 lanes：

- `docs/01-目标/razorvue/design/RazorVue.Overview.md`
- `docs/01-目标/compiler/sourcemap/SourceMap.Overview.md`
