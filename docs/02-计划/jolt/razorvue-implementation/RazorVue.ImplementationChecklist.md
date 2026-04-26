# RazorVue 实现清单

> Status: 活跃计划
> Positioning: RazorVue 第一阶段通道的主要执行清单。
> Note: 用作分阶段的实现指导；清单项可能混合已完成、部分完成和仍然开放的切片。

本文档将 RazorVue 设计分解为执行阶段。

它不重复设计推理。
其目的是将 RazorVue 设计转化为一系列具有明确验收门的可实施步骤。

相关文档：

- [RazorVue.DecisionSummary.md](../../../01-目标/razorvue/design/RazorVue.DecisionSummary.md)
- [RazorVue.Design.md](../../../01-目标/razorvue/design/RazorVue.Design.md)
- [RazorVue.HardRules.md](../../../01-目标/razorvue/design/RazorVue.HardRules.md)
- [RazorVue.Pitfalls.md](../../../01-目标/razorvue/design/RazorVue.Pitfalls.md)

## 1. 前置条件

在以下条件成立之前不要开始实现：

1. 纯 `[ECMAScriptModule] static class` 流程仍然正常工作
2. `JazorComponent` / `VueComponent` 层级已获接受
3. 基于分析器的提取已被接受为主要语义入口路径
4. `DenoHost` 已被接受为统一构建宿主
5. 第一阶段范围已明确限制

如果这些前提不稳定，实现将会漂移。

## 2. 第一阶段成功标准

第一阶段仅在项目能够做到以下所有事项时才算完成：

1. 发现 `[ECMAScriptModule]` Razor 组件
2. 强制执行 `JazorComponent` / `VueComponent` 约束
3. 提取 props / emits / slots / bind 元数据
4. 从 `BuildRenderTree` 恢复最小渲染树模型
5. 将该模型 lowering 为 Vue `defineComponent + render`
6. 发出 `DenoHost` 可消费的清单
7. 保留最小 source-origin 元数据
8. 保留最小 HMR 身份元数据

## 2.1 当前进度快照

仓库已经跨越了以下里程碑边界：

- P0 基础和迁移边界
- P1 发现和首批 Roslyn 诊断（`JAZORVUE001`、`JAZORVUE002`、`JAZORVUE004`、`JAZORVUE005`、`JAZORVUE006`）
- P2 组件契约提取
- 将 RazorVue 核心语义所有权移入 `Jazor.RazorVue` 并保持 `Jazor.RazorVue.Analysis` 为薄 Roslyn 宿主的分层重构
- 主要语义载体/编排路径：`RazorVueCompilationContext` -> `RazorVueSemanticSnapshot` -> `RazorVuePipeline` -> `RazorVueArtifactFactory` -> `RazorVueCatalog`
- P6 产物发射
- 发射侧物化和 `RazorVueCatalog` 的清单过渡

仓库已部分完成：

- P4 最小 `BuildRenderTree` 提取
- P5 Razor -> Vue lowering
- 结构化生成器诊断（超越回退表面）

当前已证明的 lowering 子集：

- HTML 元素
- 组件正常路径与组件节点 lowering
- props
- emit/listener 接线
- 文本节点
- 由参数属性支持的简单表达式
- 默认 slot 回退
- 命名 slot 接线
- 作用域 slot 接线
- 最小 `if` / `foreach` 结构化 lowering
- `OnInitialized*`、`OnParametersSet*` 和 `OnAfterRender*` 的生命周期安全子集 lowering
- `OnParametersSet*` 即时 watch 桥接
- `OnAfterRender*` 显式 `firstRender` 桥接
- 简单实例字段和辅助方法的最小 setup 侧逻辑 lowering（其参数可安全 lowering），包括固定深度的两级辅助组合

以下清单项即使存在一些脚手架仍处于实质开放状态：

- 当前生命周期/EventCallback/setup 字段/辅助安全子集之外的更广泛逻辑提取
- 完整的组件实例语义
- `Dispose*`、`ShouldRender` 和 `SetParametersAsync` 的运行时等效 lowering
- 更广泛的控制流覆盖验证
- 全面的 Razor 语法覆盖验证
- 最终的 `DenoHost` 端到端集成
- 最终的 HMR/sourcemap 输出

当前不支持的分析/lowering 形状的回退仍然是通常情况下来自 `RazorVueGenerator` 的 `JAZORVGA001`。当前薄的 `Jazor.RazorVue.Analysis` 宿主路径也为 `JAZORVGA002`（未找到组件）、`JAZORVGA003`（短组件名称歧义）、`JAZORVGA004`（保留的内部名称冲突）、`JAZORVGA005`（不支持的生命周期 lowering）和 `JAZORVGA006`（不支持的 setup 侧逻辑 lowering）投射结构化问题诊断。

## 3. P0. 基础和约束

### 3.1 定义语义载体和宿主迁移边界

任务：

- 定义编译器拥有的语义载体：
  - `RazorVueSemanticSnapshot`
  - `VueCompiledArtifact`
  - `RazorVueCatalog` 或等效结构
- 定义 RazorVue 输出如何与当前 `ModuleCatalog` / `Jazor.Emit` 路径共存
- 定义哪一层产生和消费每个载体

验收：

- 语义提取不依赖于隐藏的分析器状态
- 面向构建的发射有具体的载体
- 当前静态模块流程在过渡期间保持完整
- 阶段所有权在实现开始前是显式的

### 3.2 添加基础类

任务：

- 添加 `JazorComponent : ComponentBase`
- 添加 `VueComponent : JazorComponent`

验收：

- 层级可编译
- `JazorComponent` 保持精简
- `VueComponent` 成为 Vue 优先 API 的稳定宿主

### 3.3 添加分析器外壳

任务：

- 添加 RazorVue 分析器入口
- 启用生成代码分析
- 将纯静态模块规则与 RazorVue 规则拆分
- 缓存编译级别的符号：
  - `ECMAScriptModuleAttribute`
  - `JazorComponent`
  - `VueComponent`

验收：

- 分析器可以识别核心入口符号
- 有效的 RazorVue 组件符号不会被纯静态模块规则集拒绝

### 3.4 定义 source-origin 契约

任务：

- 定义 source-origin 条目结构
- 保留：
  - 已知时的原始 `.razor` 文件路径
  - 源跨度或稳定的段身份
  - 生成的回退跨度
  - 映射质量
- 定义来源层级：
  - Razor 源码映射
  - 生成的语法位置
  - 生成的回退

验收：

- source-origin 元数据不仅仅是类别标签
- 后续的 sourcemap/HMR 工作不需要重新设计 origin 存储
- 来源质量是显式的

### 3.5 定义组件解析规则

任务：

- 定义 `using` 驱动的可见性规则
- 定义内部名称保留
- 定义歧义诊断
- 定义第一阶段消歧语法

验收：

- 库采用保持基于 `using`
- 组件名称冲突是确定性的
- 完全限定的组件名称是第一阶段必需的歧义规避方式

### 3.6 添加文档索引条目

任务：

- 将 RazorVue 文档添加到 `doc/README.md`

验收：

- RazorVue 文档可从编译器文档索引中发现

## 4. P1. 发现和诊断

### 4.1 实现组件发现

任务：

- 检测 `[ECMAScriptModule] static class`
- 检测 `[ECMAScriptModule] JazorComponent` 后代
- 检测无效输入

验收：

- 入口拆分稳定

### 4.2 添加入口诊断

任务：

- 诊断不继承 `JazorComponent` 的 `[ECMAScriptModule]` Razor 组件
- 诊断 RazorVue 入口中的直接 `ComponentBase` 使用
- 诊断明显无效的入口形状

验收：

- 无效入口在 lowering 开始之前被捕获

### 4.3 添加误用诊断

任务：

- 诊断以下使用：
  - `StateHasChanged`
  - `ShouldRender`
  - `SetParametersAsync`
- 诊断明显的 bind/property 冲突

验收：

- 最高优先级的无效模式已被覆盖

### 4.4 测试

任务：

- 为以下场景添加分析器测试：
  - 有效 `VueComponent`
  - 有效静态模块
  - 无效的普通类
  - 无效的 `ComponentBase` 继承
  - 有效的 RazorVue 符号不被纯静态模块规则接受
  - 生成代码的 RazorVue 发现

验收：

- 发现/诊断路径有回归保护

## 5. P2. 组件契约提取

### 5.1 定义描述符结构

任务：

- 定义 `VueComponentDescriptor`
- 定义 prop / emit / slot / flag 描述符模型

验收：

- 契约模型足够稳定以支撑后续阶段

### 5.2 提取 props

任务：

- 提取普通的 `[Parameter]` 属性
- 支持 required/default/基础类型元数据

验收：

- prop 提取是确定性的

### 5.3 提取 emits

任务：

- 提取 `EventCallback`
- 提取 `EventCallback<T>`
- 定义 `OnXxx -> xxx` 映射

验收：

- emit 契约可在不进行模板 lowering 的情况下获取

### 5.4 提取 slots

任务：

- 将 `RenderFragment` 映射为默认/命名 slot
- 将 `RenderFragment<T>` 映射为作用域 slot

验收：

- slot 契约可在不进行模板 lowering 的情况下获取

### 5.5 提取 model/bind 元数据

任务：

- 检测 `Foo + FooChanged`
- 预留 Vue model/update 元数据

验收：

- 组件侧的 `@bind` 契约已知

### 5.6 从逻辑中提取显式 emits

任务：

- 在可行时检查 `Emit("...")` 使用
- 扩充 emit 契约

验收：

- 纯显式 emit 通道不会丢失

### 5.7 测试

任务：

- 添加契约提取测试
- 添加组件歧义测试
- 添加基于 `using` 的可见性测试

验收：

- props/emits/slots/model 元数据有回归保护

## 6. P3. 逻辑提取

### 6.1 提取字段和方法

任务：

- 提取普通字段
- 提取普通方法
- 识别成员中的 Vue 优先辅助使用

验收：

- setup 侧逻辑输入独立于渲染提取存在

### 6.2 提取状态类构造

任务：

- 识别：
  - `Ref`
  - `Reactive`
  - `Computed`
  - `TemplateRef`

验收：

- 最小状态设置信息可用

### 6.3 提取生命周期语法糖

任务：

- 识别：
  - `OnInitialized*`
  - `OnParametersSet*`
  - `OnAfterRender*`
  - `Dispose*`

验收：

- 生命周期 lowering 输入存在

### 6.4 提取 Vue 优先 API

任务：

- 识别：
  - `Emit`
  - `Provide`
  - `Inject`
  - `Expose`
  - `Watch`
  - `WatchEffect`
  - `NextTick`

验收：

- Vue 优先的编写 API 对 lowering 可见

### 6.5 保留逻辑的 source origin

任务：

- 将 `component-logic` source-origin 元数据附加到提取的逻辑绑定

验收：

- 逻辑提取不再产生匿名的不可见节点

### 6.6 测试

任务：

- 添加逻辑提取测试

验收：

- 生命周期/状态/API 提取有覆盖

## 7. P4. 最小 `BuildRenderTree` 提取

### 7.1 定义最小渲染树模型

任务：

- 定义以下节点：
  - element
  - component
  - text
  - expression
  - conditional
  - loop
  - attribute
  - slot content

验收：

- 渲染 lowering 不依赖于原始 operation 遍历

### 7.2 识别最小构建器模式

任务：

- 识别：
  - `OpenElement`
  - `CloseElement`
  - `OpenComponent`
  - `CloseComponent`
  - `AddAttribute`
  - `AddContent`

验收：

- 最小支持的 Razor 模板子集可被重建

### 7.3 支持最小模板结构

任务：

- 处理：
  - 纯 HTML 节点
  - 组件节点
  - 基本子内容
  - `if`
  - `foreach`

验收：

- 最小模板不再需要直接 operation 到字符串的发射

### 7.4 保留渲染树的 source origin

任务：

- 至少附加：
  - `razor-template`
  - `generated-render`
  元数据到渲染树节点
- 保留精确的源跨度或映射质量回退

验收：

- 后续的 sourcemap 支持有 source-origin 链可构建

### 7.5 测试

任务：

- 添加最小渲染树提取测试

验收：

- 渲染树提取有回归保护

## 8. P5. Razor -> Vue Lowering

### 8.1 定义 Vue 组件模型

任务：

- 定义包含以下内容的 Vue lowering 模型：
  - 描述符
  - setup 绑定
  - 生命周期绑定
  - 渲染节点
  - import/style 需求

验收：

- lowering 目标是稳定的模型，而非直接的字符串输出

### 8.2 Lowering HTML 元素

任务：

- 支持：
  - 原生属性
  - DOM 事件
  - DOM `@bind`
  - `@ref`
  - `@key`

验收：

- HTML 节点 lowering 为 Vue `h("tag", ...)`

### 8.3 Lowering 组件节点

任务：

- 支持：
  - prop 匹配
  - emit 监听器
  - 组件 `@bind-*`
  - slots
  - 作用域 slots
  - `@ref`
  - `@key`

验收：

- 组件节点 lowering 为 `h(Component, props, slots)`

### 8.4 Lowering 结构节点

任务：

- 支持：
  - `if`
  - `foreach`

验收：

- 最小控制流结构正确 lowering

### 8.5 Lowering 生命周期语法糖

任务：

- lowering：
  - `OnInitialized*`
  - `OnParametersSet*`
  - `OnAfterRender*`
  - `Dispose*`

验收：

- 生成 Vue 生命周期/watch 等效项

### 8.6 在 lowering 中保留 source origin

任务：

- 确保 source-origin 元数据在 Vue lowering 节点中存活

验收：

- origin 链不会在渲染树提取处终止

### 8.7 测试

任务：

- 添加 Vue lowering 测试

验收：

- lowering 形状受到回归测试或快照保护

## 9. P6. 产物发射

### 9.1 定义产物结构

任务：

- 定义以下输出字段：
  - 组件名称
  - 相对模块路径
  - 模块代码
  - imports
  - styles
  - 内容哈希
  - hints

验收：

- 产物模型足够稳定可供宿主消费
- 产物模型与最终文件写入步骤显式分离

### 9.2 发出标准 Vue ESM

任务：

- 发出：
  - `defineComponent`
  - `setup`
  - render function

验收：

- 输出可读且确定

### 9.3 发出描述符提供者或等效元数据

任务：

- 生成描述符提供者源码或等效的可发现载体

验收：

- 其他组件可以消费组件契约

### 9.4 预留 HMR 身份数据

任务：

- 添加：
  - `ComponentId`
  - `ModuleId`
  - `DescriptorHash`
  - `TemplateHash`
  - `LogicHash`
  - `HmrBoundaryKind`

验收：

- 产物身份是分离的，而非合并为一个通用哈希

### 9.5 在产物中保留 source-origin 交接

任务：

- 确保产物暴露后续 sourcemap 构建所需的最小钩子/结构
- 选择直接嵌入的 source origin 或附属 origin 映射输出

验收：

- source-origin 元数据在宿主交接前不被丢弃

### 9.6 测试

任务：

- 添加产物发射测试

验收：

- 模块输出、imports、哈希和元数据有回归保护

## 10. P7. `DenoHost` 集成

### 10.1 定义宿主清单契约

任务：

- 为以下内容定义清单字段：
  - 组件名称
  - 相对路径
  - imports
  - styles
  - 哈希
  - 运行时提示

验收：

- 宿主侧消费契约是显式的

### 10.2 集成构建交接

任务：

- 在面向构建的阶段物化已发射的产物/清单
- 将这些物化的输出交接给 `DenoHost`
- 更新宿主/发射流程，在过渡期间消费新的 RazorVue 载体与当前静态模块载体并行

验收：

- `DenoHost` 可以消费 RazorVue 输出
- 当前静态模块打包仍然正常工作

### 10.3 最小端到端验证

任务：

- 将一个最小 RazorVue 组件通过整个路径运行

验收：

- 编译器加宿主闭合了第一个可用循环

## 11. P8. 延后工作

以下内容不在第一阶段里程碑之内：

- 深度 Vuetify 集成
- router/pinia 集成
- 完整的内部支持矩阵
- SSR/hydration 策略完善
- 完整的 sourcemap 发射
- HMR 运行时行为
- `.vue` SFC 输出
- 通用多框架抽象

此阶段的管理规则：

- 在第一阶段完成门满足之前，不要开始生态深度的实现工作

## 12. 测试策略

第一阶段测试应分层：

1. 分析器发现和诊断
2. 契约提取
3. 逻辑提取
4. 渲染树提取
5. Vue lowering
6. 产物发射
7. 最小宿主集成

不要仅依赖端到端测试。

## 13. 第一阶段完成门

第一阶段仅在以下条件全部满足时才算完成：

1. `[ECMAScriptModule]` RazorVue 组件发现稳定。
2. `JazorComponent` / `VueComponent` 约束被强制执行。
3. props/emits/slots/model 元数据已被提取。
4. 最小渲染树恢复工作正常。
5. Vue `defineComponent + render` ESM 已被发出。
6. `DenoHost` 可以消费清单。
7. source-origin 元数据在主管线中存活。
8. 产物身份足够分离以支撑后续 HMR。
9. 主管线有回归测试覆盖。

## 14. 结论

这份清单不是关于快速实现所有内容。
它是关于确保 RazorVue 能够按照保留架构、诊断、元数据和未来可扩展性的顺序构建，而无需重新打开已确定的设计决策。
