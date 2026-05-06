# RazorVue 模板前端方向校准

## 结论

这条线的大方向已经明确，而且仓库当前实现已经走在正确方向上：

- `.razor` 组件主路应当优先使用 `RazorCodeDocument` / Razor IR。
- 只有源码中显式手写的 `BuildRenderTree` 组件，才允许走 `BuildRenderTree` 前端。
- 如果组件明显来自 Razor 生成，但当前宿主又拿不到绑定的 Razor 文档，那么应该显式失败，而不是静默回退。
- RazorVue 相关核心实现应继续收敛到 `Jazor.RazorVue`，不要再把复杂 RazorVue/Razor SDK 逻辑塞回 `Jazor.Common`。

当前仓库中，这个原则已经不只是“计划”，而是已有实现基础：

- `src/Jazor.RazorVue/RazorSdk/RazorVueRazorDocumentSemanticFrontend.cs`
- `src/Jazor.RazorVue/RazorSdk/RazorVuePreferredTemplateFrontend.cs`
- `src/Jazor.RazorVue/RazorSdk/RazorVueRazorIrTemplateFrontend.cs`

因此，后续工作不是重新发明一套并行管线，而是在现有 `Jazor.RazorVue/RazorSdk` 上继续收敛和增强。

## 当前真实结构

当前 RazorVue 的 SFC / artifact 管线，已经不是单纯的：

`.razor -> BuildRenderTree -> IOperation -> SFC`

而是：

1. `RazorVueRazorDocumentSemanticFrontend` 负责给语义快照补齐 Razor 文档路径与 `_Imports.razor` 路径。
2. `RazorVuePreferredTemplateFrontend` 决定模板前端分流。
3. 如果快照带有 `RazorDocumentPath`，优先走 `RazorVueRazorIrTemplateFrontend`。
4. 如果没有 Razor 文档，但 `BuildRenderTree` 被判定为源码手写，则走 `BuildRenderTreeTemplateFrontend`。
5. 其余情况直接报错。

也就是说，主原则已经在代码中落地，当前真正需要做的是把这个方向进一步“坐实”，而不是重新回到以 `BuildRenderTree` 为中心的设计。

## 为什么不能把目标定义成“继续从 BuildRenderTree 恢复模板”

`BuildRenderTree` 路径仍然有价值，但只适合一个明确边界：手写 `BuildRenderTree` authoring。

原因很直接：

- Razor 声明式结构在生成后的 `BuildRenderTree` C# 中已经退化。
- `@if`、`@foreach`、child content、slot forwarding、组件参数包装等信息在生成后会混入 builder 调用序列和辅助包装。
- 即便后续还能通过 `IOperation` 恢复相当一部分结构，这也不是 Razor 组件的最佳语义入口。

因此正确分层应该是：

- Razor 生成组件：先吃 `RazorCodeDocument` / IR。
- 手写 `BuildRenderTree` 组件：吃 `BuildRenderTree` / `IOperation`。

不能反过来把 `BuildRenderTree` 当成统一入口，再把 Razor IR 只是当成“可选优化”。

## 当前实现还不够彻底的地方

虽然大方向已经对了，但还有两个需要继续收敛的点。

### 1. IR 前端还依赖生成 C# / Roslyn operation 映射

`RazorVueRazorIrTemplateFrontend` 当前已经从 `DocumentIntermediateNode` 读取结构节点，但表达式和值解析仍通过 `RazorVueRazorIrOperationResolver` 回映射到 Razor 生成 C# 的 `IOperation`。

这比直接从 `BuildRenderTree` 逆向恢复模板已经好很多，但仍有一个中间依赖：

- IR 负责结构
- 生成 C# / `SourceMapping` 负责表达式 operation 绑定

这条路当前是可以接受的过渡形态，因为它保证了表达式 lowering 仍复用成熟的 Roslyn 语义。但长期目标应继续收敛到：

- 结构尽量直接从 IR 获取
- 表达式绑定尽量减少对生成 `BuildRenderTree` C# 的依赖

注意，这里的目标是“减少依赖”，不是为了抽象漂亮而强行抛弃 Roslyn 语义能力。

### 2. 文档和默认入口必须完全对齐这个原则

如果便捷入口、README、设计文档仍暗示“默认是 Roslyn-only / BuildRenderTree-first”，后续很容易又回到旧方向。

因此需要继续保证：

- `RazorVuePipeline` / `RazorVueSfcPipeline` 默认就是文档感知语义前端。
- 文档明确写清：
  - Razor 生成组件优先 IR
  - 只有手写 `BuildRenderTree` 才允许 fallback
  - 否则显式失败

## 边界约束

### 不要把复杂 Razor 接入放回 `Jazor.Common`

`Jazor.Common` 应保持真正的通用能力。

不应再放入这些内容：

- Razor SDK 内部反射接入
- RazorCodeDocument / IR 访问
- RazorVue 模板前端选择
- RazorVue catalog / SFC 生成策略

这些都属于 RazorVue 核心语义与 Razor SDK 桥接，应该留在 `Jazor.RazorVue`。

### 不要为了“直接接 Razor 官方内部实现”而引入额外复杂度

之前一种设想是：

- 给 `Jazor.Common` 加 Fody / ILAccess
- 自定义 `IRazorEnginePhase`
- 深入插入 Razor SDK 内部 phase 链

这条路不是完全不能走，但当前并不是最优先方向。原因：

- `Jazor.RazorVue` 已经能创建 `RazorProjectEngine`、拿到 `RazorCodeDocument`、读取 `DocumentIntermediateNode`。
- 现阶段的主要问题不是“完全拿不到 IR”，而是“如何让现有 IR 路线继续替代旧的 BuildRenderTree 主路”。
- 如果为了插 phase 而引入额外内部耦合、Fody、更多 SDK 版本风险，收益并不一定高于成本。

所以当前阶段的合理策略是：

- 优先把现有 `RazorSdk/*` 路线做稳。
- 只有当现有 `RazorCodeDocument -> IR -> lowering` 路线明确遇到无法绕开的 SDK 限制时，再评估更深的 phase 集成。

## 下一阶段建议

### 1. 把默认主路彻底固定成文档感知 + IR 优先

目标：

- 所有默认 pipeline 入口都优先使用 `RazorVueRazorDocumentSemanticFrontend`。
- 测试覆盖“默认入口不会悄悄退回 Roslyn-only 快照”。

这一步是护栏，不是新功能。

### 2. 继续增强 `RazorVueRazorIrTemplateFrontend`

重点不是新增一套新 pipeline，而是增强当前这条：

- 扩大支持的 IR 节点范围。
- 对当前明确 unsupported 的节点，给出更稳定、更可诊断的失败。
- 逐步减少对“生成 C# 结构必须完全对齐”的脆弱依赖。

### 3. 保持下游 catalog / emit 形状稳定

这是非常重要的约束：

- `Jazor.Analyzer` 继续只做宿主和 generator。
- `Jazor.RazorVue` 继续输出 `RazorVueCatalog` / `RazorVueSfcCatalog`。
- `Jazor.Emit` 继续负责 `.mjs` / `.vue` / manifest 物化。

也就是说，模板前端可以继续演进，但 catalog 和 emit 边界不要轻易打散。

### 4. 如果未来真要接更深的 Razor SDK phase，新增点也应落在 `Jazor.RazorVue`

即使后面证明需要：

- 自定义 `IRazorEnginePhase`
- 或更深的 `RazorProjectEngineBuilder` 扩展

这些实现也应该进入 `Jazor.RazorVue/RazorSdk` 或其相邻目录，而不是挪回 `Jazor.Common`。

## 最终判断

“用 `RazorCodeDocument` / IR 代替 `BuildRenderTree` 作为 Razor 组件主路”这个判断是对的，而且当前仓库已经部分实现。

真正应该推进的不是：

- 再设计一套独立的新 IR 管线
- 或把复杂接入转移到 `Jazor.Common`

而是：

- 继续以 `Jazor.RazorVue/RazorSdk` 为核心收敛现有实现
- 固化默认入口和测试护栏
- 逐步减少对生成 `BuildRenderTree` C# 的中间依赖
- 保持 catalog / emit 下游边界稳定

这才是和当前项目状态、以及 RazorVue 长期方向一致的路线。
