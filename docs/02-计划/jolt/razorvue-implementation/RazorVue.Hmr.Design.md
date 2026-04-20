# RazorVue HMR 设计

> 状态：活跃参考
> 定位：RazorVue HMR 的预留通道设计参考；不是活跃的实现计划。

本文档定义 RazorVue HMR 的设计。

这是一份实现前的设计文档。
当前仓库尚不包含完整的 RazorVue HMR 运行时。

本文档的存在目的是：

1. 定义 HMR 必须为 RazorVue 解决什么
2. 确定编译器/宿主的责任划分
3. 定义稳定身份和变更分类
4. 防止 HMR 扭曲 RazorVue 主管线

相关文档：

- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)
- [RazorVue.ComponentDescriptorSpec.md](./RazorVue.ComponentDescriptorSpec.md)
- [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)

## 1. 目标

RazorVue HMR 的存在是为了支持快速前端迭代，同时不削弱编译器和宿主的边界。

设计目标：

- 保持 HMR 与 Vue 优先的组件语义一致
- 将变更分类交由编译器负责
- 将运行时应用的所有权保留给 `DenoHost`
- 在不同构建之间保持确定性产物身份
- 在安全性不明确时允许保守回退

## 2. 非目标

第一阶段 HMR 明确不打算做以下事情：

1. 完全实现运行时热更新行为
2. 在每次更新中保留所有组件局部状态
3. 在 `Jazor.Compiler` 内部重建宿主拥有的前端/运行时内部机制
4. 仅从最终 JS 字符串推断 HMR 行为
5. 在第一天就让每个 Vue 生态库都具备热重载感知

## 3. 定位

HMR 是一个跨边界的能力。

编译器拥有：

- 身份
- 变更分类
- 更新安全元数据

`DenoHost` 拥有：

- 模块失效
- 运行时更新传输
- 浏览器/运行时重载策略
- 向完全页面重载的回退升级

这种分离必须保持稳定。

## 4. 为什么 HMR 必须提前设计

如果 HMR 被推迟为仅运行时关注点，
管线将迅速失去它所需的数据：

- 稳定的组件身份
- 变更类别边界
- 段所有权
- 用于更新诊断的 source-origin 链接

这将迫使后续重新设计：

- 产物
- 清单结构
- 描述符身份
- lowering 输出所有权

因此，第一阶段即使没有运行时实现也要在结构上预留 HMR。

## 5. 身份模型

RazorVue HMR 需要在等效构建之间保持稳定身份。

建议的最小身份结构：

```csharp
public sealed record VueArtifactIdentity(
    string ComponentId,
    string ModuleId,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    HmrBoundaryKind HmrBoundaryKind);
```

### 5.1 `ComponentId`

`ComponentId` 标识语义组件。

它应在等效重建中保持稳定，且不应依赖于：

- 临时输出文件名
- bundle chunk 重命名
- 运行时会话标识符

建议的输入源：

- 程序集身份
- 命名空间限定的组件身份
- 编译器路径规范化

### 5.2 `ModuleId`

`ModuleId` 标识已发出的 ESM 模块单元。

它应对宿主/运行时模块失效保持稳定。

当一个组件未来可能物化为多个模块时，它可能与 `ComponentId` 不同，
但第一阶段可以将它们紧密对齐。

### 5.3 分离哈希

编译器必须至少保留三个独立的变更哈希：

- `DescriptorHash`
- `TemplateHash`
- `LogicHash`

原因：

- 公共契约变更影响消费者
- 仅模板变更影响渲染输出
- 逻辑变更影响 setup/运行时行为

将这些合并为一个哈希会失去安全分类更新的能力。

## 6. 变更分类

RazorVue HMR 由编译器拥有的变更类别驱动。

建议的首个边界枚举：

```csharp
public enum HmrBoundaryKind
{
    Unknown,
    TemplateOnly,
    LogicSafe,
    FullReloadRequired
}
```

### 6.1 `TemplateOnly`

使用条件：

- 渲染结构发生变化
- 公共契约保持不变
- 逻辑表面保持不变

这是最有希望的 HMR 安全类别。

### 6.2 `LogicSafe`

仅在编译器能够保守地证明以下条件时使用：

- 描述符/公共契约保持稳定
- 逻辑变更在支持的热更新安全范围内
- 运行时可以安全地重新运行支持的更新钩子

这个类别应该保守地引入。
如果证明不够充分，应分类为完全重载。

### 6.3 `FullReloadRequired`

使用条件：

- props/emits/slots/model 契约发生变化
- 更新安全性不明确
- 库/运行时集成声明不兼容
- 编译器无法保留稳定的热边界

这不是失败。
这是安全的回退。

## 7. 与组件描述符的关系

描述符是 HMR 契约的一部分，而不仅仅是模板编译元数据。

描述符变更通常意味着：

- 公共契约漂移
- 调用方/被调用方兼容性风险
- 更大的失效范围

因此 `VueComponentDescriptor` 必须通过 `DescriptorHash` 参与身份。

通常应强制完全重载的描述符变更示例：

- prop 被添加/删除/重命名
- emit 名称契约变更
- slot 契约变更
- bind/model 对变更

## 8. 与模板 Lowering 的关系

模板 lowering 必须为组件的模板段暴露稳定的所有权。

这不意味着：

- 对最终 JS 字符串做差异比较
- 在渲染发射中编码 HMR 逻辑

它的意思是 lowering 管线必须保留足够的结构来说明：

- 这个模板发生了变更
- 这个模板属于这个组件/模块
- 这个模板仍然针对相同的描述符边界

## 9. 与逻辑提取的关系

逻辑提取也应暴露稳定的身份表面。

重要类别包括：

- 参与 setup 状态的字段
- 生命周期语法糖 lowering
- 显式的 `Emit`、`Provide`、`Inject`、`Expose`
- `VueComponent` 上的 Vue composable 风格编写 API

并非所有逻辑变更都是 HMR 安全的。

第一阶段设计规则：

- 保留 `LogicHash`
- 允许未来更细粒度的逻辑分段
- 不承诺对所有逻辑变更都进行安全的实时补丁

## 10. Source-origin 与 HMR

HMR 和 sourcemap 是不同的输出，
但它们共享相同的 source-origin 前置条件。

HMR 需要 source-origin 数据用于：

- 更新诊断
- 面向开发者的重载解释
- 未来的覆盖层/调试工具

因此 source-origin 元数据应至少保留到产物或附属文件级别。

HMR 在第一阶段不要求每个节点都有精确的 `.razor` 映射，
但它必须能够说明热更新决策来自：

- 精确的 Razor 支持的源码
- 生成代码衍生的映射
- 仅生成的回退

## 11. `DenoHost` 契约

`DenoHost` 应消费 HMR 相关元数据，而非重新发现它。

建议的面向宿主的字段包括：

- `ComponentId`
- `ModuleId`
- `DescriptorHash`
- `TemplateHash`
- `LogicHash`
- `HmrBoundaryKind`
- 可选的 source-origin 附属文件引用

建议的宿主职责：

1. 接收产物/清单更新
2. 比较新旧身份记录
3. 决定模板补丁、逻辑补丁或完全重载路径
4. 为保守回退提供开发者诊断

`DenoHost` 不应需要重新解释 Razor 语义来完成这些工作。

## 12. UI 库扩展

Vue UI 库可能需要额外的 HMR 元数据。

示例：

- 组件库可能需要样式依赖失效
- 插件可能将某些包装器标记为始终需要完全重载
- 描述符注册表可以声明某个组件对仅模板更新是否透明

但这些都是扩展。

核心 HMR 仍必须以基础编译器拥有的元数据来表达。

## 13. 首次实现形态

首个 HMR 实现通道应保持狭窄。

### 13.1 第一阶段预留

要求：

- 身份字段存在
- 分离哈希存在
- `HmrBoundaryKind` 存在
- 宿主清单可以携带它们

不要求：

- 实际的实时模块补丁
- 浏览器运行时协议
- 保持状态的组件替换

### 13.2 第二阶段运行时验证

只有在产物身份稳定之后，项目才应尝试：

- `DenoHost` 中的运行时失效接线
- 保守的仅模板更新路径
- 显式的完全重载回退路径

### 13.3 第三阶段生态完善

只有在核心路径稳定之后，项目才应尝试：

- 特定于库的 HMR 提示
- 更细粒度的逻辑安全更新
- 更好的开发者诊断和工具

## 14. 验证策略

HMR 设计应分层验证：

1. 身份稳定性测试
2. 哈希分离测试
3. 边界分类测试
4. 宿主清单兼容性测试
5. 后续的运行时行为测试

不要在身份模型被证明之前直接跳到运行时演示。

## 15. 设计结论

RazorVue HMR 应构建为保守的编译器与宿主之间的契约。

编译器负责分类变更并保留稳定身份。
`DenoHost` 负责应用更新。
当安全性不明确时，系统回退到完全重载，而非假装每个变更都是热安全的。
