# 编译器硬规则

> Updated: 2026-04-30

编译器主线已经收口，这份文档把后续不应再反复摇摆的规则集中到一页。想快速判断"这条语义还能不能重新讨论"，看这里就行。

完整的架构设计和背景还是看这几份：

- [ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)
- [ArchitectureOverview.md](./ArchitectureOverview.md)
- [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)

## 模块输出

### 模块导出只支持 named export

公开成员统一走 named export。任何成员如果最终导出名解析为 `default`，直接报错。不允许生成 `export default`。

这条优先级高于"输出看起来像手写 JS"的风格偏好。

### `record` 不发射 runtime declaration

`record` 不是"语法更短的 class"。它不发射 runtime class declaration，也不参与 nominal runtime type identity 建模。如果需要普通 class 语义，必须显式写 `class`。

### `enum` / `interface` 保持声明擦除

`enum` 只在编译期做值域常量化，`interface` 只做契约，都不发射 runtime artifact。不要把它们悄悄拉回 runtime declaration 模式。

## 语义 lowering

### `record` 固定走 structural lowering

统一按结构化值对象处理：

- `new Record(...)` → 对象字面量
- `record with { ... }` → 对象 spread
- 位置模式 / 属性模式 → 按结构属性键匹配
- 解构赋值 → 按结构属性键展开

不承诺 `instanceof Record`、bare record nominal type check、或依赖实例 `Deconstruct()` 的 record runtime protocol。

### tuple 是编译期语法糖，不是 runtime 类型

保位置语义、投影、解构、比较与 remap 行为。不保 `System.ValueTuple` 的 runtime identity。

### `ref/out` 是 caller/callee 协议模拟

不是 CLR 地址模型复刻。后续扩展优先保证：求值顺序、回写顺序、最终结果形态。

## 宿主接缝

### import 主链已固定

`SemanticWalker` 收集 specifier → `SenseArgument` 上浮分组 → `AstConverter` 合并去重稳定排序 → 模块头输出 `ImportDeclaration`。后续重点是稳定性，不是重新发明导入链。

### 宿主分发顺序不能漂移

消费顺序：`Compile` → `Alias` → `Inline` → `Import` → 普通 lowering。`Compile_*` 接管后失败 = claimed-and-failed，不能静默回退到别的路径。

## 失败策略

### 不支持的语义优先显式失败

不支持的外部类型 materialization、外部成员访问/调用、不成立的模块导出/导入、无法自洽的继承/constructor protocol——都应给出可定位、可行动的错误，不要输出"看起来能跑"的不可靠 JS。

## 稳定性契约

### 稳定命名、import alias、source-origin 都是 contract

temp 名稳定、helper 名稳定、import alias 稳定、source-origin / sourcemap 锚点稳定——这些是编译器契约，不是测试便利。不能退化成遍历顺序的偶然产物。

## 冲突处理

源码实现、测试断言、`ImplementationPrinciples.md`、本文档、其他旧文档——按这个优先级处理冲突。发现不一致时优先修正文档漂移，不要放宽已收口的主线规则。
