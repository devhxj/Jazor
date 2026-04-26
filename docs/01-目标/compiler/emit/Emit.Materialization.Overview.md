# Emit Materialization 概述

> Status: 活跃参考
> Positioning: `Jazor.Emit` manifest 持久化与文件物化路径的模块级概述。
> Note: 本页聚焦非 bundling 的物化主线；如果要看 bundle 或 emit-side SourceMap 继续，请转到相邻专题。

## 1. 目的

本文档专注于 emit lane 中非 bundling 的那一半。

当你需要理解以下内容时使用它：

1. emit 如何读取 compiler-owned carriers
2. 输出文件和 manifests 如何写入
3. RazorVue 制品如何继续到具体的 emitted 文件

## 2. 主要物化路径

当前的物化路径是：

1. 通过 `EmitLoadContext` 加载根和引用程序集
2. 通过 `ModuleCollector` 收集 ECMAScript 和 RazorVue carriers
3. 通过 `ModuleWriter` 写入常规模块
4. 通过 `RazorVueModuleWriter` 写入 RazorVue 模块和 sidecar manifest 数据
5. 持久化 manifest 状态以实现增量跳过/清理行为

这是 emit 将编译器输出转换为稳定磁盘树的部分。

## 3. 核心组件

### 3.1 加载和收集

- `EmitLoadContext.cs`
- `ModuleCollector.cs`
- `CatalogReader.cs`
- `RazorVueCatalogReader.cs`

当前角色：

- 安全加载已构建的程序集
- 提取 compiler-generated catalog payloads
- 在写入时间之前检测内容和相对路径冲突

### 3.2 常规模块物化

- `ManifestModel.cs`
- `ModuleWriter.cs`

当前角色：

- 写入常规 ECMAScript 模块文件
- 跟踪基于 hash 的 manifest 状态
- 跳过未更改的文件
- 在请求时清理已移除的文件

### 3.3 RazorVue 物化

- `RazorVueManifestModel.cs`
- `RazorVueModuleWriter.cs`

当前角色：

- 物化 RazorVue 制品模块
- 持久化 RazorVue 特定的 manifest 数据
- 追加模块级 SourceMap sidecars
- 保持 RazorVue 输出演进与常规模块输出并行，而不是将其隐藏在一个合并的 manifest 中

## 4. 关键规则

- 收集时冲突检测在任何文件写入之前发生
- 输出写入必须保持在配置的输出目录内
- 常规模块和 RazorVue 制品并行演进，而不是作为一个模糊的 carrier
- manifest 状态是物化行为的一部分，而不是事后的补充思考

## 5. 边界

此路径拥有：

- 文件系统写入
- manifest 持久化
- 输出树形状
- RazorVue 制品继续到文件

此路径不拥有：

- 编译器 lowering 语义
- RazorVue 描述符含义
- 超越 handing off emitted files 的 bundle 编排策略

## 6. 接下来阅读

- [Emit.Pipeline.Overview.md](./Emit.Pipeline.Overview.md)
- [Emit.BundleAndSourceMap.Overview.md](./Emit.BundleAndSourceMap.Overview.md)
- [../README.md](../README.md)
