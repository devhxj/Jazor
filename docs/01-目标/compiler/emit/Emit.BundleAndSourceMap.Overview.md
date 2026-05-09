# Emit Bundle And SourceMap 概述

## 1. 目的

本文档专注于 emit lane 中的 bundling 和 SourceMap 侧。

涵盖内容：

- emit 如何组装最终 bundle
- import 重写如何保持在 emit 内部
- 模块级 SourceMap 生成目前处于何处
- 狭窄的 active SourceMap lane 如何通过 emit 继续

## 2. Bundle 路径

bundle 路径：

1. 从 emitted output 加载 manifest 状态
2. 组装一个临时 bundle 工作空间
3. 将 emitted modules 复制到该工作空间
4. 在需要时重写 intra-graph import 路径
5. 生成一个临时 bundle 入口文件
6. 调用 `DenoHost` 进行最终 bundle 输出

bundling 保持在 host-facing lane，不会泄漏回编译器语义。

## 3. 核心组件

### 3.1 Bundle 组装

- `BundleOptions.cs`
- `ModuleBundler.cs`

角色：

- 准备临时工作空间
- 规范化 manifest 驱动的入口选择
- 为工作空间视图重写 imports
- 调用 `DenoHost` 进行最终 bundle 创建

关键规则：bundling 保持为 emit 关注点，绝不能强制编译器重新设计。

### 3.2 SourceMap 生成

- `SourceMaps/SourceMapBuilder.cs`
- `SourceMaps/SourceMapDocument.cs`
- `SourceMaps/SourceMapWriter.cs`
- `RazorVueModuleWriter.cs`

角色：

- 为 RazorVue-emitted modules 构建模块级 `.map` 文件
- 持久化 `sourcesContent`
- 将 `sourceMappingURL` 追加到 emitted module 文件

## 4. 当前程序位置

emit 中的 SourceMap 有意保持狭窄：

- 模块级 map 生成已位于此处
- 与 RazorVue 相关的 SourceMap 继续是活跃的
- 更广泛的 SourceMap 推广仍然比此切片更保守

emit 已经是 SourceMap 写入的操作主页，但不是整个广泛 SourceMap 程序的所有者。

## 5. 边界

此路径拥有：

- bundle 工作空间组装
- host-facing bundle 调用
- 模块级 SourceMap 写入
- 狭窄 SourceMap 推广的 emit 侧延续

此路径不拥有：

- 广泛 SourceMap 程序策略
- 编译器侧 origin 语义
- runtime HMR 策略

## 6. 接下来阅读

- [Emit.Pipeline.Overview.md](./Emit.Pipeline.Overview.md)
- [Emit.Materialization.Overview.md](./Emit.Materialization.Overview.md)
- [../sourcemap/SourceMap.Overview.md](../sourcemap/SourceMap.Overview.md)
- [../../../03-完成/emit/status.md](../../../03-完成/emit/status.md)
