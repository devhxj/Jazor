# RazorVue HMR 决策摘要

> Status: 活跃参考
> Positioning: RazorVue HMR 的当前保守运行时通道及其不可越过的后续边界。

## 1. 本文档解决的问题

这是一份简短文档，仅保留 RazorVue HMR 方向的最终决策。

完整设计见：

- [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)
- [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)
- [RazorVue.Hmr.ImplementationChecklist.md](./RazorVue.Hmr.ImplementationChecklist.md)

## 2. 最终决策

### 2.1 HMR 从第一阶段起就被纳入架构

第一阶段先预留运行时 HMR 所需的数据，避免重新设计；0.7 已在此基础上交付一条保守的模板更新通道。

### 2.2 编译器拥有 HMR 身份和变更分类

编译器拥有的产物必须已经携带：

- `ComponentId`
- `ModuleId`
- `DescriptorHash`
- `TemplateHash`
- `LogicHash`
- `HmrBoundaryKind`

### 2.3 宿主拥有运行时 HMR 编排

编译器负责分类变更。
开发宿主决定如何在运行时应用更新。

这保持了构建/运行时的所有权稳定。

### 2.4 HMR 不基于最终 JS 文本差异比较

RazorVue HMR 必须基于编译器拥有的语义分类，
而不仅仅依赖已发出 JavaScript 的行级差异。

### 2.5 必须保守回退

如果编译器无法证明热更新是安全的，
它必须将变更分类为需要完全重载。

不安全的乐观补丁超出范围。

### 2.6 描述符、模板和逻辑变更是不同类别

RazorVue 至少必须区分：

- 公共契约变更
- 仅模板变更
- 逻辑变更

这些类别不得合并为一个无差别的单一内容哈希。

### 2.7 HMR 必须与 sourcemap/source-origin 工作保持兼容

HMR 和 sourcemap 是独立关注点，
但两者都依赖于：

- 稳定的产物身份
- 稳定的段所有权
- 保留的 source-origin 元数据

### 2.8 UI 库集成可以扩展 HMR 元数据，但不能重新定义它

Vuetify 或 MUI 风格集成等库可以提供额外的 HMR 提示，
但它们不能重新定义核心 HMR 契约。

## 3. 0.7 已交付范围

0.7 在身份和元数据基础上交付：

1. 产物和清单中的稳定身份字段
2. 分离的变更哈希
3. 显式的边界分类
4. 宿主可消费的元数据结构
5. 仅在描述符/逻辑/身份不变、模板哈希变化且边界为 `template-only` 时发送 `module-update`
6. 浏览器通过 `JazorHmr.accept(moduleId, handler)` 显式动态导入并交给应用处理
7. 未注册处理器、导入失败、处理器拒绝和其他任何分类均回退完整刷新

0.7 明确不提供：

- 组件实例状态保留
- 特定于库的热补丁引擎
- 模板级 DOM 补丁调试 UI

## 4. 验收摘要

当前保守通道的验收条件：

1. 产物拥有稳定的组件/模块身份
2. 描述符/模板/逻辑哈希是分离的
3. `HmrBoundaryKind` 被传递到面向宿主的输出
4. 宿主不需要自己重新发现 Razor 变更类别
5. HMR 元数据不需要后续重新设计主 lowering 路径
6. 真实浏览器可收到 WebSocket 更新、加载 cache-busted 模块并调用已注册处理器

## 5. 一句话结论

RazorVue HMR 以编译器拥有的变更身份和保守安全分类为基础，当前只交付显式处理器的模板更新；更广泛的运行时替换仍是后续工作。
