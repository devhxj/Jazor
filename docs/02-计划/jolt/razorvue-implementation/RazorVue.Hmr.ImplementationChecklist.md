# RazorVue HMR 实现清单

> Status: 活跃计划
> Positioning: 未来 RazorVue HMR 工作的分阶段清单。
> Note: HMR 结构和元数据有意提前预留，但运行时行为和完整实现仍然延后于当前第一阶段通道之外。

本文档将 RazorVue HMR 设计转化为执行清单。

它是有意分阶段的。
它不假设运行时 HMR 应该立即实现。

相关文档：

- [RazorVue.Hmr.DecisionSummary.md](./RazorVue.Hmr.DecisionSummary.md)
- [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)
- [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)
- [RazorVue.DenoHostContract.md](../../../01-目标/razorvue/design/RazorVue.DenoHostContract.md)

## 1. 前置条件

在以下条件全部满足之前，不要开始真正的 HMR 工作：

1. RazorVue 入口拆分已稳定
2. 组件描述符提取已稳定
3. 渲染树提取已足够稳定，能产生确定性模板输出
4. 产物发射已存在
5. 面向宿主的清单物化已存在

如果这些不成立，HMR 工作将变成架构翻搅而非特性交付。

## 2. 阶段 1. 元数据预留

目标：

- 在没有运行时实现的情况下，使 HMR 在结构上成为可能

要求的输出：

- `ComponentId`
- `ModuleId`
- `DescriptorHash`
- `TemplateHash`
- `LogicHash`
- `HmrBoundaryKind`

清单：

- 向编译器产物模型添加身份字段
- 向面向宿主的清单模型添加 HMR 字段
- 使路径具有足够的确定性以获得稳定的 `ModuleId`
- 使描述符/模板/逻辑哈希具有确定性
- 预留可选的 origin-sidecar 关联用于诊断

验收：

- 等效构建产生稳定的身份
- 分离哈希通过物化被保留
- `DenoHost` 可以读取元数据

## 3. 阶段 2. 分类外壳

目标：

- 让编译器保守地分类变更类别

清单：

- 定义初始 `HmrBoundaryKind` 枚举
- 将描述符变更映射到重载分类
- 将仅模板变更映射到模板边界分类
- 将模糊的逻辑变更保持为完全重载
- 为每个分类路径添加可解释的原因

验收：

- 编译器可以比较新旧身份记录
- 分类是确定性的
- 不明确的情况默认为完全重载

## 4. 阶段 3. 宿主运行时骨架

目标：

- 让 `DenoHost` 消费 HMR 元数据并选择更新路径

清单：

- 为 HMR 元数据添加清单读取路径
- 在 `DenoHost` 中添加最小更新协调器
- 添加显式的仅模板更新尝试路径
- 添加显式的完全重载回退路径
- 添加开发者可见的重载回退原因

验收：

- 宿主可以接收编译器变更元数据
- 宿主可以在被允许时尝试最小热更新
- 宿主可以在不被允许时干净地回退

## 5. 阶段 4. 保守端到端验证

目标：

- 证明一条安全的端到端 HMR 通道

建议的首个验证：

- 一个稳定组件上的仅模板 Razor 变更

清单：

- 创建带有稳定描述符的特征化 fixture
- 验证模板哈希变更而描述符哈希保持稳定
- 验证宿主选择模板能力的更新路径
- 验证不支持的变更仍然触发完全重载

验收：

- 一个仅模板案例工作正常
- 一个描述符变更案例强制完全重载
- 没有不安全的热补丁案例被静默接受

## 6. 阶段 5. 后续完善

只有在保守循环全部通过之后，后续工作才应考虑：

- 更细粒度的逻辑安全分类
- 状态保留策略
- 特定于库的 HMR 提示
- 开发者覆盖层/调试工具

这些是完善项，不是入场要求。

## 7. 测试策略

建议的测试分层：

1. 身份稳定性测试
2. 分离哈希测试
3. 边界分类测试
4. 清单往返测试
5. 后续的宿主运行时行为测试

建议的早期测试名称：

- `RazorVue_HmrIdentity_EquivalentBuilds_AreStable`
- `RazorVue_HmrBoundary_TemplateOnlyChange_IsClassified`
- `RazorVue_HmrBoundary_DescriptorChange_ForcesFullReload`
- `RazorVue_HmrManifest_ContainsSplitHashes`

## 8. 早期 HMR 工作的非目标

不要将早期 HMR 工作扩展到：

- 通用运行时补丁基础设施
- 每个 Vue 库集成
- SSR/hydration 兼容性矩阵
- 组件实例状态保留保证
- 完美的源码覆盖层用户体验

## 9. 完成门

RazorVue HMR 仅在以下条件全部满足时才准备好进行真正的运行时扩展：

1. 产物身份是确定性的
2. 清单端到端携带 HMR 元数据
3. 保守分类已实现
4. 完全重载回退是显式的且经过测试
5. 至少一个仅模板流程已得到验证

## 10. 结论

启动 RazorVue HMR 的正确方式不是从一个实时演示开始。
而是从稳定身份、分离哈希、确定性分类和干净的 `DenoHost` 回退路径开始。
