# Emit Materialization 概述

## 1. 目的

本文档专注于 emit lane 中非 bundling 的那一半。

涵盖内容：

- emit 如何读取 compiler-owned carriers
- 输出文件和 manifests 如何写入
- runtime assets 和 source maps 如何随 module manifest 物化

## 2. 主要物化路径

物化路径：

1. 通过 `EmitLoadContext` 加载根和引用程序集
2. 通过 `ModuleCollector` 收集 ECMAScript module carriers
3. 通过 `ModuleWriter` 写入模块、source maps、runtime assets 和 manifest 数据
4. 持久化 manifest 状态以实现增量跳过/清理行为

这是 emit 将编译器输出转换为稳定磁盘树的部分。

## 3. 核心组件

### 3.1 加载和收集

- `EmitLoadContext.cs`
- `ModuleCollector.cs`
- `CatalogReader.cs`

角色：

- 安全加载已构建的程序集
- 提取 compiler-generated catalog payloads
- 在写入时间之前检测内容和相对路径冲突

### 3.2 常规模块物化

- `ManifestModel.cs`
- `ModuleWriter.cs`

角色：

- 写入常规 ECMAScript 模块文件
- 跟踪基于 hash 的 manifest 状态
- 跳过未更改的文件
- 在请求时清理已移除的文件

### 3.3 Runtime assets and maps

- `ModuleWriter.cs`
- `ManifestModel.cs`

角色：

- 物化 module-level source maps
- 复制 runtime assets
- 持久化 generic module manifest 数据
- 保持输出 path/hash/order 确定

## 4. 关键规则

- 收集时冲突检测在任何文件写入之前发生
- 输出写入必须保持在配置的输出目录内
- manifest 描述 generic module output，不再包含旧 RazorVue SFC/catalog shape
- manifest 状态是物化行为的一部分，不是事后补充

## 5. 边界

此路径拥有：

- 文件系统写入
- manifest 持久化
- 输出树形状
- runtime assets 和 source-map sidecar 物化

此路径不拥有：

- 编译器 lowering 语义
- RazorVue render-context lowering
- 超越 emitted files handoff 的 dev-server/HMR 编排策略

## 6. 接下来阅读

- [Emit.Pipeline.Overview.md](./Emit.Pipeline.Overview.md)
- [Emit.BundleAndSourceMap.Overview.md](./Emit.BundleAndSourceMap.Overview.md)
- [../README.md](../README.md)
