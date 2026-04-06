# RazorVue Setup-side Logic 最小闭环设计

- 日期：2026-04-06
- 范围：在现有 RazorVue `defineComponent + setup + render` 主链路上，补齐最小可执行的 setup-side logic lane。
- 目标：让一小批可静态证明安全的实例字段、无参实例方法与保守表达式进入 `setup()`，超出边界时给出明确诊断，而不是静默降级。

## 1. 当前状态

当前 RazorVue 已具备：

1. `RazorVueCompilationContext -> RazorVueSemanticSnapshot -> RazorVuePipeline -> RazorVueArtifactFactory -> RazorVueCatalog` 主链路。
2. `BuildRenderTree` 到 Vue render function 的 lowering。
3. lifecycle safe subset lowering（`OnInitialized*`、`OnParametersSet*`、`OnAfterRender*`）。
4. `VueLogicDescriptor` 目前只记录 `Name / Arity / IsAsync`，没有真正的 setup-side binding/body carrier。
5. `RazorVueArtifactFactory` 当前 `setup()` 只注入 lifecycle lowering，不注入通用 logic lowering。
6. `RazorVueExpressionEmitter` 具备保守表达式发射能力，但 render lane 会拒绝组件字段和组件实例方法调用。

因此，当前“有 logic method 元数据”并不等于“这些 logic 能进入 `setup()` 运行”。

## 2. 本轮目标

本轮只做最小闭环，不改变 RazorVue 的总体架构：

1. 在 semantic snapshot 中保留 setup-side logic lowering 所需的最小语义载体。
2. 支持最保守的一组 setup-side authoring 形态：
   - 简单实例字段
   - 无参实例方法
   - 简单 helper 调用
   - 保守表达式（literal / parameter property / supported field / unary / binary / conditional / interpolated string）
3. 在 `setup()` 中生成最小字段声明与 helper 函数。
4. render / lifecycle 在访问这批安全字段与 helper 时，能够发射到同一份 `setup()` 作用域。
5. 超出边界时，给出 setup-side logic 专用结构化诊断。

## 3. 非目标

本轮明确不做：

- 完整组件实例对象桥接。
- 通用 `this` 语义。
- 带参实例方法 lowering。
- 任意赋值语义、状态写回、深层成员链。
- `Ref` / `Reactive` / `Computed` 等完整响应式 authoring。
- `Dispose*` / `ShouldRender` / `SetParametersAsync` 新能力扩张。
- 通用 statement lowering 框架。

## 4. 允许的安全子集

### 4.1 字段

仅支持满足以下条件的实例字段进入 setup-side lowering：

- 非 static。
- 有源码位置。
- 初始化表达式存在且可被保守表达式发射器处理。
- 不依赖组件实例写入语义。

首轮支持的字段初始化表达式：

- literal
- `[Parameter]` 属性引用
- 已支持字段引用
- unary / binary
- conditional
- interpolated string
- 同组件内已支持的无参 helper 调用

### 4.2 方法

仅支持无参实例方法，且方法体必须是以下最小形态之一：

- expression-bodied method
- 单个 `return <expr>;`

其中 `<expr>` 必须落在保守表达式子集内。

### 4.3 helper 调用

只允许调用同组件内、同样满足“无参 + 单表达式返回”的 helper。这样可以把 helper 直接发射为 `setup()` 里的局部函数，而不需要模拟 class instance。

## 5. 明确不支持的形态

以下一律视为超界：

- 字段赋值或自增自减
- 多语句方法体
- 带参实例方法
- `this` 访问
- 任意实例方法链
- 外部对象复杂方法调用
- 深层成员链
- 依赖完整组件实例生命周期的逻辑

## 6. 设计方案

### 6.1 semantic carrier

扩展 `VueLogicDescriptor`，从“只有方法签名”升级为“字段 + 方法”的最小 lowering carrier：

- `VueLogicFieldDescriptor`
  - `Name`
  - `InitializerShape`
  - `IFieldSymbol FieldSymbol`
- `VueLogicMethodDescriptor`
  - `Name`
  - `Arity`
  - `IsAsync`
  - `BodyShape`
  - `IMethodSymbol MethodSymbol`

`RazorVueSemanticSnapshot` 继续保留 `Logic`，但 `Logic` 需要携带这批 descriptor，而不再只是纯摘要。

### 6.2 lowering

在 `RazorVueArtifactFactory.BuildModuleCode(...)` 中，保留当前顺序：

1. Vue imports
2. `defineComponent`
3. `setup(props, { emit, slots, expose, attrs })`
4. lifecycle lowering
5. **新增 setup-side logic lowering**
6. `return () => ...`

setup-side logic lowering 的输出形态：

```js
setup(props, { emit, slots, expose, attrs }) {
  const titleText = `Count: ${props.value}`;
  function formatTitle() {
    return titleText;
  }
  return () => h("section", null, formatTitle());
}
```

### 6.3 表达式发射器

扩展 `RazorVueExpressionEmitter`，新增 setup-side 模式下的保守表达式发射：

- 当前组件 `[Parameter]` 属性 -> `props.xxx`
- 当前组件已支持字段 -> 直接引用局部变量名
- 当前组件已支持无参 helper -> 直接生成 `helperName()`

render lane 与 setup-side lane 共用同一套“支持表达式集合”，但错误消息需要区分：

- template expression unsupported
- setup-side logic unsupported

### 6.4 诊断

新增 `RazorVueIssueCode.UnsupportedSetupLogicLowering`，并由 `RazorVueGenerator` 映射到新诊断号（建议 `JAZORVGA006`）。

触发场景：

- 字段初始化表达式超界
- helper 方法体超界
- helper 调用引用了不支持的字段/方法
- render / lifecycle 访问到声明了但无法 lowering 的 setup-side symbol

## 7. 测试策略

### 7.1 descriptor / snapshot

新增或更新测试锁定：

- candidate 能发现最小字段/logic method
- snapshot.Logic 能保留字段与无参 helper
- 复杂方法/带参方法不会进入 supported lowering 集合

### 7.2 pipeline

先写失败测试，再实现：

1. `BuildRenderTree` 调用无参 helper，helper 读取 parameter property，生成 `setup()` 局部函数。
2. `BuildRenderTree` 读取安全字段，字段初始化依赖 parameter property，生成 `const fieldName = ...`。
3. helper 调用安全字段，render 调 helper，整个链路成功。
4. helper 访问带参实例方法或复杂实例状态，抛 `UnsupportedSetupLogicLowering`。

### 7.3 generator

锁定新的结构化诊断会被投影为具体诊断号，而不是退回 `JAZORVGA001`。

## 8. 文档同步要求

需要同步更新：

- `src/Jazor.Compiler/doc/RazorVue.Overview.md`
- `src/Jazor.Compiler/doc/RazorVue.Design.md`
- `src/Jazor.Compiler/doc/RazorVue.ImplementationChecklist.md`

重点统一口径：

- RazorVue 现在已有“setup-side logic 最小闭环”，但不是完整组件实例语义。
- 支持的是保守字段/无参 helper 子集。
- 超界继续走明确诊断。

## 9. 一句话结论

这一轮不是把 RazorVue 变成“类实例运行时”，而是在现有 Vue-first `setup()` 架构下，把最小可证明安全的 setup-side logic 变成真正可执行的主链路。