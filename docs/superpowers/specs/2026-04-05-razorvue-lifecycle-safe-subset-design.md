# RazorVue 生命周期安全子集 lowering 设计

- 日期：2026-04-05
- 范围：在现有 RazorVue 主链路上补齐 lifecycle lowering，使 `OnInitialized*`、`OnParametersSet*`、`OnAfterRender*` 进入可执行的安全子集；超出边界时提供明确诊断。
- 目标：把 RazorVue 从“生命周期只记录存在性或空 hook 壳”推进到“安全子集可执行、超界即明确失败”的稳定主链路。

## 1. 背景与当前状态

当前 RazorVue 已打通以下主链路：

1. 组件候选发现与 descriptor/snapshot 提取。
2. `BuildRenderTree` 到 Vue render function 的 lowering。
3. 组件 props / emits / slots / 子组件引用的基础生成。
4. catalog 生成、identity/hash、基础 HMR boundary 分类。

当前缺口集中在 lifecycle / logic：

- `RazorVueCompilationContext` 与 `RazorVueSemanticSnapshot` 已能识别多种生命周期方法的存在性。
- `RazorVueArtifactFactory` 当前只对部分 lifecycle 生成空 hook 壳，且仅接受 no-op 形状。
- 真实生命周期方法体尚未进入可执行 lowering。
- `OnParametersSet*` 尚未接入稳定桥接。
- 生命周期超界时缺少清晰、专用的诊断面。

因此，这一轮不追求“完整 VueComponent 实例语义”，而是补齐 lifecycle 主链路的安全子集。

## 2. 设计目标

本轮目标：

1. 支持以下生命周期进入可执行 lowering：
   - `OnInitialized`
   - `OnInitializedAsync`
   - `OnParametersSet`
   - `OnParametersSetAsync`
   - `OnAfterRender`
   - `OnAfterRenderAsync`
2. 保持当前 `defineComponent({ setup(...) { ... return render; } })` 主架构不变。
3. 只支持可以安全投影到 Vue `setup()` closure / hook 的生命周期方法体子集。
4. 当方法体超出安全子集时，提供明确、稳定、可定位的专门诊断。
5. 补齐测试与文档，并在关键边界补足注释。

## 3. 非目标

本轮明确不做：

- 完整 class-instance -> `setup()` 运行时桥接。
- 组件字段、实例方法、`this` 语义的通用 lowering。
- `Dispose*`、`ShouldRender`、`SetParametersAsync` 的完整运行时桥接。
- host / hydration / HMR runtime 的大范围扩展。
- 为所有 C# 方法体提供通用 logic lowering。

## 4. 总体架构

### 4.1 复用现有主链路

继续沿用现有链路：

`RazorVueCompilationContext -> RazorVueSemanticSnapshot -> RazorVueArtifactFactory -> VueCompiledArtifact`

其中：

- `RazorVueCompilationContext` 负责发现 lifecycle 方法与必要符号信息。
- `RazorVueSemanticSnapshot` 承载 lowering 所需的生命周期语义载体。
- `RazorVueArtifactFactory` 负责把生命周期桥接写入 `setup()`。
- `RazorVueExpressionEmitter` 仅在现有可证明安全的表达式子集内复用，不扩张为完整实例逻辑 emitter。

### 4.2 关键架构约束

本轮不引入“组件实例对象”作为新的 lowering 中间层。原因是当前生成目标是 Vue Composition API 的 `setup()` closure，而非 class component runtime。若强行把 C# class lifecycle 直接映射为实例调用，容易引入错误的 `this`、字段状态、方法绑定、时序和闭包语义。

因此，本轮所有生命周期方法体都必须满足 closure-safe 约束，不能假设存在完整组件实例。

## 5. 生命周期 lowering 规则

### 5.1 `OnInitialized` / `OnInitializedAsync`

映射规则：

- `OnInitialized` -> `onMounted(() => { ... })`
- `OnInitializedAsync` -> `onMounted(async () => { ... })`

语义边界：

- 本轮将其视为“组件完成 setup 后的首次挂载初始化逻辑”。
- 不模拟 Blazor 中更完整的实例初始化时序。

### 5.2 `OnParametersSet` / `OnParametersSetAsync`

映射规则：

- 初次 setup 完成后执行一次。
- 当 props 变化时再次执行。
- 采用显式的 props watch / 同步桥接，而不是假装它与任意 Vue hook 天然等价。

实现方向：

- 生成用于追踪 props 变化的稳定桥接代码。
- 保证首次运行与后续变更运行都有明确定义。
- 不允许依赖 class 字段累积状态来描述参数变化前后差异。

### 5.3 `OnAfterRender` / `OnAfterRenderAsync`

映射规则：

- 首次渲染后：`onMounted(...)`
- 后续更新后：`onUpdated(...)`

`firstRender` 桥接：

- 首次调用传 `true`
- 后续更新调用传 `false`

实现要求：

- 明确在生成代码中维护首次/后续调用语义。
- 不做隐式推断，避免 future regression 时失去可读性。

## 6. 安全子集定义

### 6.1 允许的生命周期方法体形状

本轮允许进入 lowering 的方法体只覆盖“最小可证明安全”子集，目标是与现有 `setup()` 架构兼容，而不是追求表达力最大化。

允许的形状包括：

- 空方法体。
- `Task.CompletedTask` / `ValueTask.CompletedTask` / 等价 no-op 返回。
- 少量顺序语句。
- 局部变量声明。
- `return` / `expression statement`。
- `await` 形式的安全异步表达式。
- 当前 emitter 已明确支持的表达式节点。
- 仅依赖参数、局部变量、可直接投影到 closure 的数据访问。

### 6.2 明确不支持的形状

以下情况直接判定为超界：

- 访问组件字段。
- 调用组件实例普通方法。
- 使用 `this` 或依赖隐式实例绑定。
- 修改实例状态。
- 依赖完整 class runtime 的行为。
- 超出当前 emitter 稳定支持范围的复杂语句 / 表达式。
- 无法安全映射到 Vue props watch / mount / update 时序的写法。

## 7. 诊断策略

### 7.1 诊断目标

当生命周期方法无法进入安全 lowering 时，生成器应当给出清晰的生命周期专用诊断，而不是只暴露模糊的兜底异常。

诊断应明确指出：

- 组件名。
- 方法名。
- 不支持的原因。
- 当前支持的生命周期安全子集边界。

### 7.2 触发场景

专门诊断至少覆盖：

1. 生命周期方法体超出安全子集。
2. `OnParametersSet*` 出现无法安全桥接的语义。
3. `OnAfterRender*` 出现无法稳定处理 `firstRender` 的形状。
4. 生命周期依赖实例字段 / 实例方法 / `this`。

### 7.3 失败策略

- 对于超界的生命周期方法，采用“显式编译失败 + 明确诊断”。
- 不做静默空 hook。
- 不做看似成功但语义错误的 lowering。
- 除非该失败面尚未接入专门 issue 管道，否则不再依赖模糊 fallback 作为主要用户体验。

## 8. 关键代码改动方向

### 8.1 `RazorVueCompilationContext`

- 继续复用现有生命周期发现逻辑。
- 在必要时补充 `OnParametersSet*` 与 `OnAfterRender*` lowering 所需的符号/语法信息。
- 不新增独立的第二套生命周期发现入口。

### 8.2 `RazorVueSemanticSnapshot`

- 从“生命周期 capability flag”推进到“可用于 lowering 的最小 carrier”。
- 保留方法符号、必要参数信息，以及用于诊断的来源信息。
- 对 `OnAfterRender(bool firstRender)` 明确保留参数形状信息。

### 8.3 `RazorVueArtifactFactory`

- 作为本轮核心改动点。
- 在 `setup()` 中注入 lifecycle bridge。
- 为 `OnInitialized*`、`OnParametersSet*`、`OnAfterRender*` 分别生成清晰的 Vue hook / watch 代码。
- 对超界生命周期在 lowering 阶段抛出可转化为明确诊断的失败。
- 在关键边界添加注释，说明为何不允许直接使用 class-instance 语义。

### 8.4 `RazorVueExpressionEmitter`

- 仅在当前可证明安全的表达式能力内复用。
- 如果生命周期方法体需要的表达式超出当前稳定能力，优先诊断失败，而不是为本轮引入通用实例逻辑 lowering。

## 9. 测试策略

### 9.1 snapshot / descriptor 层

补充并锁定：

- `OnParametersSet*` 被识别并进入 snapshot。
- `OnAfterRender*` 的参数形状（含 `firstRender`）被正确记录。
- 生命周期相关 carrier 与来源信息完整。

### 9.2 pipeline / artifact lowering 层

重点补到 `RazorVuePipelineTests.cs`：

- `OnInitialized` -> `onMounted(...)`
- `OnInitializedAsync` -> `onMounted(async ... )`
- `OnParametersSet` / `OnParametersSetAsync` -> 初次执行 + props 变化桥接
- `OnAfterRender` / `OnAfterRenderAsync` -> `onMounted(...)` + `onUpdated(...)`
- `firstRender` 首次为 `true`、后续为 `false`
- 生命周期超界时，产生明确生命周期诊断

### 9.3 generator 层

重点补到 `ESGeneratorTests.cs`：

- lifecycle lowering 成功时 catalog 正常生成。
- lifecycle 超界时，生成器给出明确的生命周期诊断输出。
- 锁定失败面，避免未来回退到不透明的通用异常体验。

## 10. 文档更新

同步更新：

- `src/Jazor.Compiler/doc/RazorVue.Overview.md`
- `src/Jazor.Compiler/doc/RazorVue.Design.md`
- `src/Jazor.Compiler/doc/RazorVue.ImplementationChecklist.md`

更新原则：

- 明确写成“已支持 lifecycle 安全子集 lowering”。
- 明确列出已支持的 lifecycle：
  - `OnInitialized*`
  - `OnParametersSet*`
  - `OnAfterRender*`
- 说明 `OnParametersSet*` 的桥接模型。
- 说明 `OnAfterRender*` 的 `firstRender` 显式桥接。
- 说明哪些方法体仍不支持。
- 不把本轮描述成完整 logic lowering 或完整实例语义支持。

## 11. 注释策略

本轮遵循“关键边界必须有注释，机械性注释不要铺满”的原则，重点补在：

1. `setup()` 与 class-instance 语义边界。
2. lifecycle safe subset 的判定原因。
3. `OnParametersSet*` 为什么需要显式 props bridge。
4. `firstRender` 为什么需要明确桥接变量而不是隐式约定。
5. 生命周期超界为何必须给专门诊断。

## 12. 分阶段交付

### 阶段 1：诊断与 carrier 收口

- 补足 snapshot carrier。
- 接入或新增 lifecycle lowering 专门诊断。
- 锁定超界失败面。

### 阶段 2：安全子集 lowering

- 实现 `OnInitialized*`。
- 实现 `OnParametersSet*`。
- 实现 `OnAfterRender*` 与 `firstRender`。

### 阶段 3：测试与文档闭环

- 补 pipeline / generator / descriptor 测试。
- 同步更新设计与实现文档。
- 验证失败路径、回归路径与 identity/hash 不回归。

## 13. 预期结果

完成后，RazorVue 的 lifecycle 将从“存在性元数据 / 空 hook 壳”提升为：

- 常用生命周期可进入可执行 lowering。
- 安全子集边界清晰。
- 超界语义明确失败且可定位。
- 测试、文档、关键边界注释同步齐备。

一句话总结：

**把 RazorVue 的 lifecycle 从‘只记录或空壳’推进到‘安全子集可执行、超界即明确报错’的稳定主链路。**
