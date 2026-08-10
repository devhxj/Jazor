# RazorVue HMR 硬性规则

> Status: 活跃参考
> Positioning: RazorVue HMR 的当前保守运行时通道及后续扩展均必须遵守的约束参考。

本文档确定 HMR 实现规则中不可保持模糊的边界。

它不重复所有 HMR 设计讨论。
它的存在是为了锁定后续实现和评审不可再行商议的边界。

相关文档：

- [RazorVue.Hmr.DecisionSummary.md](./RazorVue.Hmr.DecisionSummary.md)
- [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)
- [RazorVue.Hmr.ImplementationChecklist.md](./RazorVue.Hmr.ImplementationChecklist.md)
- [RazorVue.Hmr.Pitfalls.md](./RazorVue.Hmr.Pitfalls.md)

## 1. 范围

这些规则适用于当前 RazorVue HMR 通道和后续扩展：

- 编译器拥有的产物身份
- 变更分类
- 面向宿主的 HMR 元数据
- `DenoHost` 运行时边界

## 2. 规则 1. HMR 是一等架构关注点

第一阶段先保留运行时 HMR 所需的数据；0.7 已据此交付受限的 `template-only` 运行时通道。

当前通道仅在身份、描述符和逻辑哈希不变而模板哈希变化时发送更新，且浏览器必须由 `JazorHmr.accept(moduleId, handler)` 显式接管。其他情况完整刷新。

HMR 不得被视为可选的后处理步骤。

## 3. 规则 2. 编译器拥有 HMR 身份

编译器必须拥有：

- `ComponentId`
- `ModuleId`
- `DescriptorHash`
- `TemplateHash`
- `LogicHash`
- `HmrBoundaryKind`

`DenoHost` 必须消费这些元数据，而非重新构建它们。

## 4. 规则 3. HMR 不得仅从已发出的 JS 差异推断

最终 JS 文本差异比较可以帮助诊断，
但它不得成为主要 HMR 安全模型。

主要分类必须保持编译器拥有和语义驱动的。

## 5. 规则 4. 分离哈希是强制性的

第一阶段不得将 HMR 变更类别合并为一个内容哈希。

至少，描述符、模板和逻辑必须保持分离。

## 6. 规则 5. 保守回退是强制性的

如果 HMR 安全性无法被证明，
编译器或宿主必须升级为完全重载。

不安全的乐观热补丁超出界限。

## 7. 规则 6. 公共契约漂移不是仅模板变更

对 props、emits、slots 或 bind/model 元数据的变更不得被分类为仅模板变更。

它们是契约级别的变更，通常应强制完全重载。

## 8. 规则 7. `DenoHost` 拥有运行时应用

编译器不拥有：

- 浏览器更新传输
- 模块失效运行时
- 组件实例替换运行时

这些属于 `DenoHost`。

## 9. 规则 8. HMR 不得扭曲主 lowering

HMR 元数据是 RazorVue 主管线的扩展。

它不得成为以下行为的理由：

- 围绕运行时补丁技巧重新设计模板 lowering
- 将渲染发射与宿主运行时细节耦合
- 将 bundler/运行时状态泄漏到编译器语义提取中

## 10. 规则 9. 描述符身份必须参与 HMR

`VueComponentDescriptor` 是 HMR 边界的一部分。

描述符变更必须影响：

- `DescriptorHash`
- 边界分类
- 宿主失效决策

## 11. 规则 10. Source-origin 元数据必须保持与 HMR 兼容

HMR 在第一阶段不要求完整的 sourcemap，
但它确实要求兼容的 source-origin 元数据。

至少，HMR 相关诊断必须能够追溯到：

- 原始源码（如果已知）
- 生成映射（当源码是间接的）
- 生成的回退（当仅有此可用时）

## 12. 规则 11. 库集成只能扩展基础 HMR 契约

库可以添加：

- 额外的失效提示
- 样式依赖提示
- 始终重载标记

库不得重新定义：

- `ComponentId`
- `ModuleId`
- 核心边界种类
- 编译器/宿主所有权划分

## 13. 规则 12. HMR 分类必须保持可解释

对于每个分类，
系统应该能够用编译器/宿主的术语解释为什么变更被归类为：

- 仅模板
- 逻辑安全
- 需要完全重载

没有可解释类别的隐藏启发式方法超出界限。

## 14. 规则 13. 初始运行时范围保持最小

0.7 的具备运行时能力的 HMR 里程碑仅证明：

- 宿主可以看到身份变更
- 宿主可以尝试带显式应用处理器的保守模板更新路径
- 宿主可以干净地回退到完全重载

它不得自动替换 Vue 实例、保留状态，或试图立即解决每个状态保留场景。

## 15. 规则 14. HMR 验证必须从身份稳定性开始

在任何运行时 HMR 演示被认为是有效的之前，
仓库必须已经证明：

1. 稳定的组件身份
2. 稳定的模块身份
3. 稳定的分离哈希
4. 确定性的边界分类

## 16. 结论

RazorVue HMR 只有在保持保守、元数据层由编译器拥有、运行时层由宿主拥有，且与更广泛的 RazorVue 产物模型兼容的情况下才是有效的。当前通道不得被误写为通用热替换或状态保留能力。
