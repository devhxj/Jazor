# Jazor.Compiler

`Jazor.Compiler` 是 Jazor 仓库中的核心 C# -> JavaScript 编译器模块。

它的主职责不是“把任意 .NET 代码直接翻译成 JS 文本”，而是：

- 以 Roslyn `IOperation` 为主要语义输入
- 以 Acornima ESTree 为主要中间表示
- 在受控输入域内追求使用点可观察行为等价
- 对宿主语义、命名、导入、source-origin 和输出确定性给出稳定契约

## 当前定位

`Jazor.Compiler` 当前应被理解为三层协作：

1. `AstConverter`：负责模块级和类型级展开，例如顶层 `function` / `class` / 字段 / 属性 / 导入声明组织。
2. `SemanticWalker`：负责语义级 lowering，把 `IOperation` 转成 ESTree。
3. WhiteList / `Op.Compile` / SourceOrigin：负责宿主映射事实、复杂宿主钩子和调试来源锚点。

产品集成通过通用、强类型的扩展契约接入，普通 compiler mainline 不携带产品 profile 或 ASP.NET Components lowering：

- `AstConverterModulePolicy`：定义模块层级、runtime class 位置、声明名与额外可见性的确定性投影策略。
- `SemanticWalkerHost`：定义窄 Roslyn operation rewrite seam；`RewriteInvocationIntrinsic` 在 compiler intrinsic/whitelist dispatch 前接收已 lower 的 operands 与 `SemanticInvocationLoweringContext`。
- `CompositeSemanticWalkerHost`：按“第一个 rewrite 声明者获胜、观察扇出、skip/claim OR”组合多个 product host，不暴露 `SemanticWalker` 继承面。
- `SourceOrigin`、`GeneratedNodePosition` 和 node-layout emitter：为需要组合更大 artifact 的产品提供 source-map 锚点与稳定节点坐标。

RazorVue 的 current-component、RenderTreeBuilder、children-to-slot 和 Components catalog 均由 `Jazor.RazorVue` 自己实现并测试；其最终 `defineComponent`/setup/render-function artifact 也不属于本项目。

真正的文件物化不在本项目内完成：

- `Jazor.Compiler` 负责 AST、模块文本、catalog / source map carriers
- `Jazor.Emit` 负责 `.mjs` / `.mjs.map` 与 manifest 物化

## 当前硬契约

下面这些路线已经不应再按“探索态”理解：

- `tuple`：走表达式组合 lowering，保投影、解构、比较与 remap 行为，不保 `System.ValueTuple` runtime identity。
- `ref/out`：走 caller/callee 协议模拟，优先保证求值顺序、回写顺序和最终结果。
- `enum`：走“声明擦除 + 使用点常量化”，运行时按底层标量处理。
- `interface`：只作为契约参与分析、宿主查找和投影，不发射 runtime declaration。
- `record`：固定走 structural lowering；创建、`with`、位置/属性模式、解构都按结构属性键处理，不保 nominal runtime identity；若需要普通 runtime class 语义，必须显式写 `class`。
- iterator：module method、runtime member method 和 local function 依据实际 Roslyn operation tree 声明 generator；`yield` 输出 `function*`，async iterator 输出 `async function*`，nested lambda/local function 的 yield 不影响外层函数。
- field-like event：当前模块 non-record runtime member class 的非静态、非 virtual/override 字段式事件使用私有调用列表与 add/remove/snapshot helper；直接实例方法组按 `(method, receiver)` 等价而非 JS `bind` 临时函数身份比较，raise 前固定快照以保留 C# 的多播顺序和订阅变更语义。模块静态事件、custom accessor、virtual/override、by-ref 参数或返回、delegate equality/combination 与 `IRaiseEventOperation` 保持显式失败。
- UTF-8 string literal：`IUtf8StringOperation` 使用 Roslyn 已解码的字符串值直接构造精确 byte `ArrayExpression`，沿 `ReadOnlySpan<byte>` 的既有 Array carrier 传递；不发射 JS 字符串、`TextEncoder`、BOM、隐式结束符或新的 typed-array identity。
- 成员类继承：当前支持同模块成员类的 JS-compatible 子集，真实输出 `extends` / `super(...)` / `super.member`。
- 普通方法重载：仅在确有同名 overload 时追加稳定签名 hash；ECMAScript runtime host API 默认跳过该后缀。
- 成员类构造函数重载：固定为单真实 `constructor` + `$ctor_<hash>` helper + 已绑定构造函数 selector dispatcher。
- `Op.Import`：已完成“发现 -> 收集 -> 合并 -> 模块头输出”的闭环，后续重点是稳定性而不是接线。
- `Op.Compile`：已接入主分发，但当前 contract 仍限于自包含表达式级钩子，不是完整 lowering 子系统。
- 模块导出：只支持 named export，不支持 default export；任何成员若解析到导出名 `default` 都应显式失败。

## 模块边界

新增能力时，优先按下面的边界落点判断：

- 输入是否合法：`Jazor.Analyzer`
- 外部 API 如何映射：`Jazor.CLR` / `ECMAScript` 标注 + WhiteList 生成
- 模块级结构如何展开：`AstConverter`
- 方法体 / 表达式如何 lowering：`SemanticWalker`
- 文件如何物化：`Jazor.Emit`

不要在 `ESGenerator`、`AstConverter`、`SemanticWalker` 里重复实现另一套宿主语义。

## 代码结构

```text
src/Jazor.Compiler/
├── AstConverter.cs
├── ESGenerator.cs
├── ESGenerator.SourceMap.cs
├── ESGenerator.SourceMapCatalog.cs
├── ESGenerator.SourceOrigin.cs
├── Optimizer.cs
├── Sense.cs
├── SenseArgument.cs
├── TypeMapper.cs
├── Util.cs
├── WhiteList.cs.*
├── SourceMap/
└── core/
    ├── SemanticWalker.cs
    ├── SemanticWalker.cs.Reference.cs
    ├── SemanticWalker.cs.Creation.cs
    ├── SemanticWalker.cs.Tuple.cs
    ├── SemanticWalker.cs.Pattern.cs
    ├── SemanticWalker.cs.Ordinary.cs
    ├── SemanticWalker.cs.TryCatch.cs
    └── ...
```

## 开发约束

修改本项目时，优先遵守这些约束：

1. 先保使用点可观察行为，再考虑 runtime 结构外观。
2. 对不支持的外部运行时语义，优先显式失败，不做 silent raw-JS 直通。
3. 稳定 temp 名、import alias、helper 名和 source-origin 锚点属于编译器契约，不是测试便利。
4. 能稳定用 `Alias` / `Inline` / `Import` 解决的宿主语义，不要冒进塞进 `Compile`。
5. 需要 temp / import / source-origin / 语句级协议的复杂宿主改写，不要硬塞进当前 `Compile(handler, args)` contract。
6. 生成文件不手改；若宿主映射变更，应回到 CLR/ECMAScript 标注源和 generator。

## 测试

主回归测试项目：

- `src/Jazor.CompilerTest`

常见入口：

```powershell
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
```

如需更细的测试分层、SourceMap / catalog 测试入口和编写约束，直接看：

- `src/Jazor.CompilerTest/README.md`

说明：

- 瞬时构建状态、测试总数和通过率不在本 README 中维护。
- 这类信息应以当前 CI、测试运行结果和仓库状态页为准，而不是写死在源码目录文档里。

## 推荐阅读顺序

如果要理解当前实现路线，建议按下面顺序阅读：

1. `src/Jazor.Compiler/ImplementationPrinciples.md`
2. `docs/01-目标/compiler/Compiler.HardRules.md`
3. `docs/01-目标/compiler/README.md`
4. `docs/01-目标/compiler/ArchitectureOverview.md`
5. `docs/01-目标/compiler/SyntaxTransformationPipeline.md`
6. `docs/01-目标/compiler/semantic-walker/SemanticWalker.md`
7. `docs/01-目标/compiler/ModuleConversionSpec.md`
8. `docs/01-目标/compiler/OpCompileSpec.md`

## 相关文档

- [ImplementationPrinciples.md](./ImplementationPrinciples.md)
- [Compiler Hard Rules](../../docs/01-目标/compiler/Compiler.HardRules.md)
- [Compiler 文档索引](../../docs/01-目标/compiler/README.md)
- [Compiler 架构桥接](../../docs/01-目标/compiler/architecture.md)
- [Jazor.CompilerTest README](../Jazor.CompilerTest/README.md)
