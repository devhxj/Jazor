# Jazor Compiler 主线状态（2026-08-02）

> Status: 当前状态快照
> Positioning: 仓库级编译器主线状态快照

## 总结

`Jazor.Compiler` 仍然是当前仓库里最成熟的主干资产。

当前可复验基线：

- `Jazor.CompilerTest`：8158 / 8158 通过
- `Jazor.Compiler` 行覆盖：14003 / 14522（96.43%）
- `Jazor.Compiler` 分支覆盖：6051 / 6671（90.71%）
- 验收入口：`dotnet run --file scripts/csharp/verify-compiler-coverage.cs`

coverage gate 会直接运行完整 compiler suite、读取本次 TRX 与 Cobertura，并对 8,000 个通过测试、95% 行覆盖和 90% 分支覆盖执行非零退出码约束；`coverlet.runsettings` 本身不承担阈值判断。

更具体而言：

- 编译器主链路已经接近稳定主干
- 当前工作重点不是重做架构，而是维持主线闭环、控制边界扩张、给外围能力提供稳定依赖面
- 仓库级文档应该把 compiler 当成"稳定核心"，而不是"当前最混沌的探索区"

更具体地说，当前 compiler 主线已经不是“很多能力还没定路线”，而是：

- 关键 runtime 边界已经明确
- 若干以前容易摇摆的语义已经从“目标路线”收口为“当前契约”
- 后续增量工作应优先遵守这些契约，而不是重新打开基础语义形态

## 当前状态判断

### 1. 主链路成熟度高

`AstConverter`、`SemanticWalker`、白名单和 generator 主链路已经有稳定参考价值了。

因此 repo-level 文档应该优先把 compiler deep-dive 当成长期参考入口，而不是把它和阶段性实施材料混成一层。

### 2. 当前更像"稳定化与收口"，不是"大规模重构期"

当前 compiler 线最重要的几件事是：

- 保持语义主线稳定
- 控制新增能力对主链路的扰动
- 给 RazorVue、SourceMap、Emit 这些下游 lane 提供稳定上游

### 3. 当前已收口的关键路线

这一轮明确下来的，不只是“支持了更多测试”，而是几条长期容易反复的路线已经固定：

- `tuple`：走表达式组合 lowering，保使用点行为，不保 `System.ValueTuple` runtime identity
- `ref/out`：走 caller/callee 协议模拟，保求值顺序、回写顺序和结果形态
- `enum`：声明擦除，使用点常量化，运行时按底层标量处理
- `interface`：只作为契约参与分析、投影和宿主查找，不发射 runtime artifact；erased interface `is` 仅在 Roslyn 可证明时折叠，`T : IContract` 保留非空判断，`T : struct, IContract` 折叠为 `true`
- `record`：固定走 structural lowering；创建、`with`、位置/属性模式与解构都按结构属性键处理，不保 nominal runtime identity
- 模块导出：固定只支持 named export；任何成员若解析到导出名 `default` 都应显式失败
- 成员类继承：支持同模块成员类的 JS-compatible 子集，真实输出 `extends` / `super(...)` / `super.member`
- 成员类构造函数重载：单真实 `constructor` + `$ctor_<hash>` helper + 已绑定构造函数 selector dispatcher
- 产品扩展：核心以 `AstConverterModulePolicy`、`SemanticWalkerHost`、`CompositeSemanticWalkerHost` 和 `SemanticInvocationLoweringContext` 提供强类型组合契约；RazorVue product lowering 与 Components catalog 已迁出核心

这意味着 compiler 主线现在已经有一套更清晰的“什么必须保、什么可以擦除、什么必须显式失败”的规则，而不是继续在“尽量长得像手写 JS”上摇摆。

### 4. Import 与模块头链路已闭环

之前 import 还是“收集多、落盘少”的风险点。当前这部分已经进入稳定状态：

- `SemanticWalker` 收集 import specifier
- `SenseArgument` 上浮导入分组
- `AstConverter` 合并、去重并稳定排序
- 模块头生成 `ImportDeclaration`

所以 import 不再是当前 compiler 的核心缺口，后续重点更多是保持确定性，不是重新打通主链。

### 5. 这轮已经补上对称的 compiler 状态入口

之前 repo-level 主要是总项目状态和 RazorVue 状态，导致 compiler 虽然成熟，但在仓库级工作流图里不够显眼。本状态页的作用就是把这条断链补全。

## 下一步行动

### 1. Catalog / emit contract stability

**目标**：巩固 `compiler -> catalog -> emit` 边界与物化契约

**具体行动**：
- 已覆盖外部消费者的官方 Razor SG -> VueRenderCatalog -> `Jazor.Emit` -> 最终 `.mjs` -> bundled DenoHost 闭环；同一 Counter consumer 同时验证 DOM 与 descriptor 子组件显式 async `@bind:set` 的相对模块 import、`update:modelValue` 回写，以及 setter 内部先规范化 state 后完成持久化的顺序，另有异步点击后的条件内容切换，且产物不得回退到 render-context / `.vue` 协议。Razor SDK 禁止 `:set` 与 `:after` 组合，后续逻辑由显式 setter 承担。
- 避免文档把 compiler 产 catalog 和 emit 写文件混成一个未定义阶段
- 让 catalog、模块文本与最终物化产物的关系保持一致

**参考文档**：
- [TransformationRoadmap.md](../../02-计划/compiler/TransformationRoadmap.md)
- [TransformationClosureChecklist.md](../../02-计划/compiler/TransformationClosureChecklist.md)

### 2. Host semantics seam

**目标**：稳定 `Inline` / `Compile` 分工

**具体行动**：
- `Nullable<T>.Value` 作为 compiler-owned `Op.Compile` lowering：通过 AST 构造短路 nullish guard，保证 receiver 单次求值，并在 `null` / `undefined` carrier 上抛出稳定的 `InvalidOperationException` 语义。
- ECMAScript runtime `params` 默认映射为 JavaScript rest arguments；显式 `[PreserveParamsArray]` 则保留为单个数组实参，保护 Vue `withModifiers` 等 runtime array contract。
- C# 14 `field` 属性在成员运行时类中使用合法的 JavaScript private slot；回归同时校验 Acornima AST 可解析与 Deno.host 的 setter/getter 读写语义。
- 避免宿主语义扩张又跑回来破坏 compiler 主线边界
- 保持 `Inline` 和 `Compile` 的职责清晰分离

**参考文档**：
- [InlineAstTemplateSpec.md](../../01-目标/compiler/InlineAstTemplateSpec.md)
- [OpCompileSpec.md](../../01-目标/compiler/OpCompileSpec.md)
- [OpCompileImplementationChecklist.md](../../02-计划/compiler/OpCompileImplementationChecklist.md)

### 3. Source origin / sourcemap stability

**目标**：把“稳定 emission”从测试便利提升为持续契约

**具体行动**：
- 保持 temp 名、import alias、source-origin 锚点稳定
- 避免 traversal-order 影响输出
- 让 SourceMap 与真实输出链继续对齐，而不是只在测试链路里成立

**参考文档**：
- [SourceMap.Design.md](../../01-目标/compiler/sourcemap/SourceMap.Design.md)
- [SourceMap.Overview.md](../../01-目标/compiler/sourcemap/SourceMap.Overview.md)

## 深度文档

- [Compiler Architecture Bridge](../../01-目标/compiler/architecture.md)
- [Compiler 文档索引](../../01-目标/compiler/README.md)
- [ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)
- [Jazor.Compiler README](../../../src/Jazor.Compiler/README.md)

## 当前缺口

- output / emit / sourcemap 侧仍需继续扩展真实构建闭环的场景覆盖；当前外部 Razor Counter 已具备 DOM binder、子组件 explicit model setter、catalog import 与事件状态的 DenoHost 基线
- 宿主语义扩张仍然可能反向污染 compiler 边界，需要持续约束
- 还需要继续把 compiler 局部文档里的 active / historical 边界写清楚，避免旧阶段表述回流成“当前事实”
