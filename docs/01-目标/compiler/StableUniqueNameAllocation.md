# 稳定唯一名称分配设计

## 目录

- [目标边界](#目标边界)
- [当前方案](#当前方案)
- [设计分层](#设计分层)
- [关键原则](#关键原则)
- [当前稳定性模型](#当前稳定性模型)
- [为什么这样更合适](#为什么这样更合适)
- [明确不采用的方案](#明确不采用的方案)
- [实现落点](#实现落点)
- [验证重点](#验证重点)
- [当前接受的边界](#当前接受的边界)
- [最终结论](#最终结论)

## 目标边界

以下讨论 `Jazor.Compiler` 内部自动生成名称的分配规则，例如：

- tuple lowering 的中间缓存
- pattern / switch lowering 的输入缓存
- `try/catch` lowering 的合成异常参数
- 引用缓存、对象初始化 IIFE 内的临时变量

这里的“稳定”不是指“换机器、换 checkout 路径、换工作区后仍必须完全同名”。

当前接受的稳定边界是：

- 同一份源码
- 同一路径
- 同一编译上下文
- 无关空白、注释、无关语义改动尽量不影响名称

只要在这个边界内可复现、并且始终编译出正确代码，就满足当前要求。

## 当前方案

最终名称由下面几层信息组成：

1. `UniqueNameSession.OwnerKey`
2. `EmissionScopeContext.ScopeKey`
3. `LoweringSite`
4. `LoweringNameOwner.StableKey`
5. 固定回退命名空间 salt

最终格式固定为：

- `__<tag>$<hash>`

例如：

- `__swexpr$...`
- `__tfield$...`
- `__mcatch$...`

这里保留 `$` 不只是为了可读性，也是为了把自动生成名放进一个和普通 C# 用户标识符天然分离的命名空间。

这意味着：

- 普通 C# 局部变量、参数、局部函数名不会和这类内部名字直接冲突
- 命名系统不需要再扫描当前语法作用域里的用户声明名做额外防撞

## 设计分层

### 1. `UniqueNameSession`

`UniqueNameSession` 表示一次 emitted owner 内部的命名会话。

它负责：

- 生成 `OwnerKey`
- 建立根发射作用域
- 为当前 operation 树建立 session 内部 identity 索引

`OwnerKey` 当前允许使用规范化后的绝对路径：

- 路径变化会导致名字变化
- 这是允许的
- 它只承担“文档/编译单元隔离盐值”的职责

换言之，`filepath` 可以用，但只放在 session / document 这一层。

### 2. `EmissionScopeContext`

每个最终 JavaScript 发射作用域都对应一个 `EmissionScopeContext`。

它负责：

- 生成当前 scope 的 `ScopeKey`
- 跟踪当前 scope 已经分配过的内部名称
- 为某个 lowering owner 分配最终名称

`ScopeKey` 当前只表达词法层级，不承担 operation 级语义区分职责。

### 3. `LoweringSite`

`LoweringSite` 的主维度必须是固定枚举，不能退化成“调用点随便拼一个 site 名”。

它的职责是表达：

- 这是哪一类 lowering
- 在该 lowering 内，如果确实存在多个子槽位，就带上该槽位的精确 path

例如：

- `SwitchExpressionInput`
- `SwitchPatternInput`
- `TupleFieldCache("0.1.3")`
- `TupleNestedArgument("2.0")`
- `MultiCatchParameter`

这里的 slot path 不是为了开放成任意字符串协议，而是为了避免把多层 tuple 子槽位再压缩成有碰撞风险的整数。
换言之：

- site kind 仍然是固定枚举
- slot 只在“同 kind 下必须区分多个子槽位”时使用
- slot 必须能无损表达该 lowering 内部的位置路径

### 4. `LoweringNameOwner`

`LoweringNameOwner` 明确拆成两部分：

- `StableKey`
- `IdentityKey`

两者职责不同：

- `StableKey` 参与最终可见名称哈希，决定名称在“同路径、同语义 lowering”下是否稳定
- `IdentityKey` 只用于 session 内部缓存区分，不参与最终名称哈希

这是当前方案里最重要的分界线。

## 关键原则

### 1. 可见稳定性来自 `StableKey`，不是 `IOperation identity`

当前真正决定最终名称稳定性的，是显式构造出来的 `LoweringNameOwner.StableKey`。

它由 `SemanticWalker` 按 lowering 语义生成，例如：

- `switch expression` 输入缓存看 `switch.Value`
- pattern `switch` 输入缓存看 `switch.Value`
- method reference proxy 看目标方法和实例语义
- tuple / deconstruct / reference temp 看对应 operation 的语义 key

换言之，稳定性来自“lowering owner 语义”，不是来自“operation 在树里的编号”。

### 2. `OperationIdentityIndex` 只做 session 内部区分

`OperationIdentityIndex` 现在不再承担“最终稳定语义路径”职责。

它只负责：

- 给当前 operation 树中的节点分配 session 内部 identity
- 让同一 operation 在同一轮发射里重复申请名称时，能命中同一个缓存键
- 让不同 operation 即便 `StableKey` 相同，也能在分配表中作为不同 owner 记录

因此它可以保持轻量，不需要继续维护庞大的全语义指纹系统。

### 3. 冲突消解必须固定、不可依赖顺序编号

当主候选名与保留名冲突时，当前策略不是：

- `_1`
- `_2`
- 按访问顺序递增

而是固定命名空间回退：

- `p`
- `f1`
- `f2`
- ...

这样即便发生冲突，回退规则本身仍是纯函数。

## 当前稳定性模型

当前模型可以概括为：

> 路径只负责 owner 隔离，scope 只负责词法层级，真正的名称稳定性由 lowering owner 语义驱动。

具体来说：

- `filepath` 允许出现在 `OwnerKey`
- `filepath` 不进入 `ScopeKey`
- `filepath` 不直接参与具体 lowering owner 的语义构造
- `ScopeKey` 只表达 emitted lexical scope
- `StableKey` 才是最终名称的稳定主键
- `IdentityKey` 只是 session 内部辅助键

## 为什么这样更合适

这样分层以后，有几个直接收益：

1. 命名模型明显更简单
2. `filepath` 不再被误当成“全局稳定性目标”
3. 不需要让 `OperationIdentityIndex` 承担过重职责
4. 某个 lowering 拆成多个子 operation 时，只要 `LoweringSite` 和 `StableKey` 设计正确，就不会混淆
5. 代码审查时可以明确区分：
   - 这是 scope 问题
   - 这是 lowering owner 问题
   - 这是 session 隔离问题

## 明确不采用的方案

当前明确不采用：

- `FilePath + SourceSpan` 直接生成最终名称
- 任意调用点自由拼接 site kind
- 全局自增计数器
- “先申请者占主名，后来者 `_1/_2`”
- 继续把 `OperationIdentityIndex` 当成最终可见名称的稳定主键

## 实现落点

主要代码在：

- `src/Jazor.Compiler/UniqueNameSession.cs`
- `src/Jazor.Compiler/EmissionScopeContext.cs`
- `src/Jazor.Compiler/OperationIdentityIndex.cs`
- `src/Jazor.Compiler/LoweringNameOwner.cs`
- `src/Jazor.Compiler/LoweringSite.cs`
- `src/Jazor.Compiler/ScopeSite.cs`
- `src/Jazor.Compiler/SenseArgument.cs`
- `src/Jazor.Compiler/core/SemanticWalker.cs`

## 验证重点

当前回归主要验证：

- trivia-only 改动不会导致名称漂移
- 无关 sibling 插入不会破坏主要稳定性
- 多 lowering slot 能生成不同名称
- 同一 scope 下不同 owner 即便共享同一个 `StableKey`，也会通过固定 fallback 分流
- `catch` / tuple / switch / reference 等关键 lowering 仍保持稳定
- `Jazor.CompilerTest` 与 `Jazor.EmitTest` 保持通过

## 当前接受的边界

如果同一 emitted scope 下真的出现了多个 lowering owner：

- `LoweringSite` 相同
- `StableKey` 相同
- 但 `IdentityKey` 不同

那么它们会共享同一个主候选名，并通过固定 fallback 命名空间分流。

这类场景下，不承诺“插入一个新的完全同形 lowering owner 后，其它同组 owner 仍绝对不漂移”。

这是当前为了控制复杂度而接受的边界。

## 最终结论

`Jazor.Compiler` 当前不再把问题建模成：

> “给某个 `IOperation` 生成全局稳定名称”

而是建模成：

> “在某个 emitted scope 下，为某个明确的 lowering owner 生成稳定、冲突安全、可复现的内部名称”

这也是当前最符合生产要求、同时复杂度最低的落地方案。
