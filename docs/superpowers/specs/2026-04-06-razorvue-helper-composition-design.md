# RazorVue Helper Composition 两层闭环设计

- 日期：2026-04-06
- 范围：在现有 RazorVue `defineComponent + setup + render` 主链路上，为 setup-side logic 增加受限的 helper composition 能力。
- 目标：让 `render -> helperA -> helperB` 这类两层固定深度的组合在保守安全子集内稳定 lowering 到 `setup()`，超过边界时明确报 `JAZORVGA006`。

## 1. 当前状态

当前 RazorVue 已具备：

1. `RazorVueCompilationContext -> RazorVueSemanticSnapshot -> RazorVuePipeline -> RazorVueArtifactFactory -> RazorVueCatalog` 主链路。
2. `BuildRenderTree` 到 Vue render function 的 lowering。
3. lifecycle safe subset lowering（`OnInitialized*`、`OnParametersSet*`、`OnAfterRender*`）。
4. setup-side logic 的当前保守闭环：
   - 简单实例字段
   - helper 参数表达式的安全 lowering
   - helper 方法参数可在当前 safe subset 内投影到 `setup()`
5. `RazorVueExpressionEmitter` 已能登记 render / setup 中需要的字段与 helper，并在参数表达式层面发射 props / field / helper 调用。
6. `RazorVueArtifactFactory` 已能把字段与 helper materialize 到 `setup()` 中。

当前缺口不是“字段和 helper 能不能各自 lowering”，而是“helper 之间的组合边界仍未作为一个明确、可验证的阶段闭环收口”。文档中的 open item 仍是：`broader logic extraction beyond the current lifecycle/event-callback/setup-field/helper safe subset`。

## 2. 本轮目标

本轮只做受限的 helper composition 闭环，不改变 RazorVue 的 Vue-first 总体架构：

1. 支持 `render -> helperA(...) -> helperB(...)` 的两层固定深度组合。
2. 支持 helper body 内混合使用：
   - 已支持的 component field
   - `[Parameter]` props
   - 另一个已支持 helper
3. 保持现有 setup-side safe subset：
   - helper 仍必须是可安全发射的单表达式返回方法
   - async / statement-heavy / instance-runtime-dependent 形状继续视为 unsupported
4. 超过两层时，显式报 `JAZORVGA006`，而不是静默降级或尽量生成。

## 3. 非目标

本轮明确不做：

- 任意深度 helper 图。
- helper 递归 / 循环依赖检测与一般图算法。
- 完整组件实例对象桥接。
- `this` 语义扩张。
- 新的局部变量 lowering。
- 新的控制流 lowering（`if` / `switch` / 多语句 helper body）。
- `Dispose*` / `ShouldRender` / `SetParametersAsync` 新能力。
- 通用 statement lowering 框架。

## 4. 允许的安全子集

### 4.1 调用深度

- render 直接调用 helper，记为深度 1。
- helper 内再调用 helper，记为深度 2。
- helper 内再继续调用第三层 helper，直接视为越界。

允许的最大链：

- `render -> helperA`
- `render -> helperA -> helperB`

不允许的链：

- `render -> helperA -> helperB -> helperC`

### 4.2 helper body 内允许访问的内容

helper body 内仍只允许当前 setup-side safe subset：

- literal
- `[Parameter]` 属性引用
- 已支持字段引用
- unary / binary / conditional
- interpolated string
- 另一个已支持 helper 调用

### 4.3 参数与签名

- render / helper 调用点的实参数量必须与目标 helper 签名一致。
- 参数表达式本身必须可被当前 safe subset 发射。
- helper 定义仍发射为 `setup()` 内的局部函数，参数名沿用方法参数名。

## 5. 明确不支持的形态

以下一律视为超界并触发 setup-side logic unsupported：

- 三层及以上 helper 链。
- async helper。
- 多语句 helper body。
- 需要完整实例语义的 `this` 访问。
- helper 内复杂控制流。
- helper 递归 / 环状依赖。
- 不可发射的参数表达式。
- 调用点与目标 helper 签名不匹配。

## 6. 设计方案

### 6.1 emitter 负责“依赖收集 + 深度边界”

`RazorVueExpressionEmitter` 继续作为 setup-side logic 的依赖识别中心，但从“只收集 required helpers”升级为“收集 helper 依赖关系并带深度边界”。

需要最小化新增的语义状态：

- `requiredSetupMethods`：保留，表示最终需要 materialize 的 helper。
- `helperDepthBySymbol`：记录某个 helper 当前已知的最小深度。
- `currentSetupMethodStack`：表示当前正在展开哪个 helper，用于知道 `helperA -> helperB` 的依赖方向。
- 如有必要可额外保留 `helperEdges`，用于诊断或稳定排序；但本轮不要求暴露成通用图模型。

判定规则：

- render 直接引用 helper 时：登记为深度 1。
- 在 `EmitSetupExpression()` 中，如果当前 owner helper 为 `helperA`，其 body 内又遇到 `helperB(...)`：登记为深度 2。
- 如果在深度 2 的 helper 中继续遇到第三层 helper：立即抛 `UnsupportedSetupLogicLowering`。
- 同一 helper 若既被 render 直接用到，又被别的 helper 调用，取更小深度，不算越界。

### 6.2 artifact factory 继续负责 materialize

`RazorVueArtifactFactory` 不引入新的图算法，继续负责把 emitter 已登记的字段与 helper 写进 `setup()`：

1. render 先触发 root helper 登记。
2. factory 发射 root helper。
3. root helper body 的 setup expression 发射过程中，再登记二层 helper。
4. factory 下一轮发射二层 helper。
5. 如果二层 helper 再登记三层 helper，emitter 直接报错，factory 不再进入下一轮。

因此，本轮仍可复用现有“迭代直到稳定”的发射结构，而不必改成通用拓扑排序器。

### 6.3 诊断策略

继续使用 `RazorVueIssueCode.UnsupportedSetupLogicLowering` / `JAZORVGA006`，但对“深度越界”给出更明确的信息。

推荐错误文案：

- `RazorVue setup lowering only supports helper composition up to two levels in component 'X'.`
- 若能确定具体链路，则追加：
  - `Helper 'FormatOuter' reaches helper 'FormatLeaf' beyond the supported composition depth.`

仍保留对其他 unsupported shape 的专用说明，例如 async、多语句 body、不可发射参数表达式等。

## 7. 测试策略

### 7.1 成功测试

必须新增：

1. 两层 helper 链成功 lowering：
   - render 调 `FormatOuter(...)`
   - `FormatOuter(...)` 调 `FormatInner(...)`
   - 最终 `setup()` 中出现两个 helper 函数并正确连接
2. 两层链 + field/props/helper 混合：
   - helperA 读 field
   - helperB 读 props / 参数
   - 整体成功 lowering
3. 重复依赖稳定发射：
   - render 中两个 helper 共用同一个 inner helper
   - inner helper 不会重复 materialize，也不会错误越界

### 7.2 失败测试

必须新增：

4. 三层 helper 链失败：
   - `render -> A -> B -> C`
   - 抛 `RazorVueCompilationIssueException`
   - `Issue.Code == UnsupportedSetupLogicLowering`
5. 两层内但 helper shape 超界：
   - 例如 inner helper 为 async 或 body 非当前 safe subset
   - 仍走 `JAZORVGA006`

### 7.3 回归验证

保持全量 RazorVue 回归：

- `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "RazorVue"`

## 8. 文档同步要求

需要同步更新：

- `src/Jazor.Compiler/doc/RazorVue.Overview.md`
- `src/Jazor.Compiler/doc/RazorVue.ImplementationChecklist.md`

同步后的口径应明确：

- setup-side logic 已从“字段 + helper 单点 lowering”扩到“受限 helper composition”
- 支持两层固定深度
- 超过两层显式报 `JAZORVGA006`

## 9. 验收标准

完成后表示：

1. RazorVue setup-side logic 已具备受限组合能力，而不仅是字段/单 helper 的孤立 lowering。
2. 两层 helper 组合在 field/props/helper 混合读取场景下稳定工作。
3. 三层及以上 helper 链不会静默通过，而是明确报 `JAZORVGA006`。
4. RazorVue 全量测试保持通过。

## 10. 一句话结论

这一轮不是把 RazorVue 变成“任意深度的 helper runtime”，而是在现有 Vue-first `setup()` 主链路上，把 helper composition 收口到“两层固定深度、超界显式报错”的可验证阶段边界。