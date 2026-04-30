# Jazor Compiler 硬规则补充

> Status: 活跃参考
> Updated: 2026-04-30
> Positioning: 编译器主线已经收口、后续实现与文档不应再临场决定的硬约束摘要。

## 1. 文档定位

这份文档不是重新解释全部 compiler 设计，也不是替代：

- [src/Jazor.Compiler/ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)
- [ArchitectureOverview.md](./ArchitectureOverview.md)
- [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)

它只做一件事：

把当前已经落地、后续不应再反复摇摆的主线规则集中写成一页。

如果你只是想快速判断“这条语义还能不能重新讨论”，优先看这里。

## 2. 适用范围

本文档适用于当前 `Jazor.Compiler` 主线以及沿主线继续扩面的实现约束，重点覆盖：

1. 模块级输出边界
2. 语义级 lowering 边界
3. 宿主接缝与失败策略
4. 稳定性契约

本文档不展开：

1. 某个专题的完整背景
2. 每个语法域的逐节点细节
3. 历史实现为何演变成当前方案

这些内容分别回到专题文档和 `ImplementationPrinciples.md`。

## 3. 模块输出硬规则

### Rule 1. 模块导出只支持 named export

当前模块输出固定只支持 named export。

必须满足：

1. 公开成员统一走 named export
2. 任何成员若最终导出名解析为 `default`，必须显式失败
3. 不允许生成 `export default`

这条规则高于“输出看起来像手写 JS module”的风格偏好。

### Rule 2. `record` 不发射模块级或成员级 runtime declaration

当前 `record` 不是“语法更短的 class”。

模块层与成员层都必须满足：

1. `record` 不发射 runtime class declaration
2. `record` 不参与 nominal runtime type identity 建模
3. 若用户需要普通 runtime class 语义，必须显式写 `class`

### Rule 3. `enum` / `interface` 继续保持声明擦除路线

当前不得把它们悄悄拉回 runtime declaration 模式：

1. `enum` 只保编译期值域角色，使用点常量化
2. `interface` 只保契约角色，不发射 runtime artifact

## 4. 语义 lowering 硬规则

### Rule 4. `record` 固定走 structural lowering

`record` 当前必须统一按结构化值对象处理。

已固定的行为是：

1. `new Record(...)` -> 对象字面量
2. `record with { ... }` -> 对象 spread
3. 位置模式 / 属性模式 -> 按结构属性键匹配
4. 解构赋值 -> 按结构属性键展开

当前不承诺：

1. `instanceof Record`
2. bare record nominal type check
3. 依赖实例 `Deconstruct()` 的 record runtime protocol

### Rule 5. tuple 是编译期语法糖，不是 runtime 类型设计

tuple 相关 lowering 必须继续遵守：

1. 保位置语义、投影、解构、比较与 remap 行为
2. 不保 `System.ValueTuple` runtime identity
3. 运行时对象协议由当前静态视图决定

### Rule 6. `ref/out` 是 caller/callee 协议模拟

`ref/out` 不应被重新理解为 CLR 地址模型复刻。

后续扩展仍应优先保证：

1. 求值顺序
2. 回写顺序
3. 最终结果形态

## 5. 宿主接缝硬规则

### Rule 7. import 主链已经固定

当前 import 主链固定为：

1. `SemanticWalker` 发现 import specifier
2. `SenseArgument` 上浮导入分组
3. `AstConverter` 合并、去重、稳定排序
4. 模块头输出 `ImportDeclaration`

后续工作重点是稳定性，不是重新发明另一条导入链。

### Rule 8. 宿主分发顺序与失败语义不能漂移

当前消费顺序应继续保持：

1. `Compile`
2. `Alias`
3. `Inline`
4. `Import`
5. 普通 lowering

如果 `Compile_*` 明确接管后失败，应视为 claimed-and-failed，而不是静默回退成别的路径。

## 6. 失败策略硬规则

### Rule 9. 不支持的 runtime-sensitive 语义优先显式失败

当前 compiler 主线不接受 silent raw-JS fallback。

尤其是：

1. 不支持的外部类型 materialization
2. 不支持的外部成员访问 / 调用
3. 不成立的模块导出 / 导入结果
4. 无法自洽的 inheritance / constructor protocol

应优先给出可定位、可行动的失败，而不是输出“看起来能跑”的不可靠 JS。

## 7. 稳定性硬规则

### Rule 10. 稳定命名、import alias、source-origin 都是 contract

下面这些不是测试便利，而是编译器契约：

1. temp 名稳定
2. helper 名稳定
3. import alias 稳定
4. source-origin / sourcemap 锚点稳定

后续实现不得把它们退化成遍历顺序偶然产物。

## 8. 冲突处理

若出现“旧文档、局部文档、实现、测试”不一致，优先级按下面处理：

1. 当前源码实现
2. 当前测试断言
3. [src/Jazor.Compiler/ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)
4. 本文档
5. 其他旧文档与历史材料

发现冲突后，优先修正文档漂移，而不是先放宽已收口的主线规则。
