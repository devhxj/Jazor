# RazorVue Block Code Execution Model Phase 1

> Status: In Progress
> Updated: 2026-05-19
> Parent: `RazorVue.BlockCode.ExecutionModel.md`

## 1. Phase-1 Goal

第一阶段不追求“完整 block code 全支持”，而是先完成一条生产可落地的正式主线：

- 复杂 block 不再被当成无限增长的前端特判问题
- 两条前端先统一到 body-level promotion
- 先给 H / render-function 路线建立正式承载能力

Phase 1 的关键词是：

- correctness
- single architecture
- frontends alignment

而不是：

- 局部最优 template lift
- 一次性吃掉所有 statement 形态

## 2. Current Completion

截至 2026-05-19，Phase 1 已完成的内容：

1. 命令式 block 已进入正式中间模型：`RazorVueImperativeBlockNode`
2. handwritten `BuildRenderTree` frontend 已按 body-level promotion 产生命令式 block
3. Razor IR frontend 已按同一 body-level promotion 规则产生命令式 block
4. `.mjs` / H artifact 已具备首段 imperative render bridge，可承载：
   - 提前 `return`
   - `while`
   - `switch`
   - `lock`
   - `try/catch/finally`
   - `using` / `using declaration`
   - 局部 mutation 后继续渲染
   - imperative body 内常量 `AddMarkupContent(...)`
   - imperative `OpenComponent(...)` / `AddComponentParameter(...)` 的 descriptor-aware prop/slot 分流
   - builder-style `RenderFragment` / `RenderFragment<T>` 组件参数的 imperative slot materialization
   - imperative body 对 injected/resolved component runtime-usage 的 descriptor identity/hash 收集
   - Razor IR root template `@{ ... }` imperative slot forwarding 与 handwritten `BuildRenderTree` 对齐
5. `.vue` / SFC artifact 已具备 render-function SFC 承载，上述 imperative slice 不再在 SFC lowering 阶段被显式拒绝
6. focused pipeline / SFC / Razor IR 回归已锁定上述场景

当前尚未完成的内容：

1. imperative body 的 canonical template path
2. 更复杂控制流与语句族的继续扩面，例如 `await using` / 更复杂异常与资源管理语义

## 3. Required Deliverables

### 2.1 Intermediate Model

引入正式命令式 block 节点，最小要求：

- 能挂在现有 `RazorVueRenderFragment.Children`
- 能表达“当前组件 render body 已提升为命令式执行”
- 能进入 artifact identity / diagnostics

### 2.2 Frontend Alignment

handwritten `BuildRenderTree` frontend 与 Razor IR frontend 在遇到 Phase-1 支持范围外的 block 时，必须：

- 得到相同 promotion 结论
- 给出同一类 issue / message 语义

### 2.3 Lowering

Phase 1 只要求：

- H artifact 能承载命令式 block
- `.vue` SFC artifact 能承载 render-function form 的命令式 block

## 4. Phase-1 Supported Imperative Slice

当前第一批已落地支持的复杂 block：

1. `while`
2. 局部变量赋值后继续参与渲染
3. 同一 render body 中“业务语句 + 渲染语句”的顺序保留
4. `return` 提前退出
5. `switch`
6. `try/catch/finally`
7. `lock`
8. `using` / `using declaration`
9. imperative body 内常量 `AddMarkupContent(...)`

这批场景已经覆盖“复杂业务不是纯 if/for/foreach 模板”的首段真实生产需求。

## 5. Phase-1 Explicit Non-Goals

Phase 1 不要求：

1. `await using`
2. 更细粒度的局部 block promotion
3. `lock` 的更强运行时语义模拟
## 6. Testing Requirement

每一项 Phase-1 支持都必须同时补：

1. handwritten frontend focused tests
2. Razor IR frontend focused tests
3. parity tests
4. pipeline/lowering tests
5. docs update

## 7. Exit Criteria

Phase 1 完成的标志不是“支持了多少 statement”，而是：

1. 命令式 block 已经成为正式中间语义
2. 两条前端不再各自扩 statement 特判
3. 至少一条真实复杂 block 路径已能稳定通过到 artifact
4. README / support-gap 文档已不再把复杂 block 简单描述成长期 unsupported
5. 至少一条 body-level imperative render 路径已能稳定编译并通过 focused tests
