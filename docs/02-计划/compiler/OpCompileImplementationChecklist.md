# `Op.Compile` 实施清单

> Status: Active phase-one implementation artifact.
> Positioning: Focused checklist for first-stage `Op.Compile` wiring.
> Note: Defines the current contract boundary and sequencing for implementation; later contract expansion belongs to a later phase.

## 目标

这份清单不是重新定义 `Op.Compile` 语义，而是把：

- 哪些事现在就能做
- 哪些事必须等 contract 扩展后再做
- 接线时先补什么测试

落成可执行步骤。

语义定义见：

- [OpCompileSpec.md](./OpCompileSpec.md)

## 第一阶段边界

第一阶段只允许 `Op.Compile` 承担：

- 自包含表达式级改写
- 不新增声明
- 不新增 import
- 不依赖 `IOperation` 局部结构
- 不要求 source-origin 追踪

这不是保守偏好，而是当前 `Compile(handler, args)` contract 的直接结果。

同时保留两条 producer 侧优先级：

- 能稳定写成 `Inline` 的，先不要升级成 `Import`
- 能稳定写成 `Inline` 或作为运行时 helper 落到 `Import` 的，不要升级成 `Compile`

## 第一阶段实施步骤

### 1. 接主分发，但不扩 hook

在 `GetWhiteListExpression(...)` 里补上：

1. 根据成员签名查询 `_whiteListCompiles`
2. 命中后调用对应 `Compile_*`
3. 返回非 `null` 立即停止
4. 返回 `null` 继续 `Alias -> Inline -> Import`
5. 抛异常直接中止，不静默回退

### 2. 固定 `handler/args` 传参规则

必须先统一：

- `handler`
  - 实例成员传实例表达式
  - 静态成员传 `null`
- `args`
  - 只放显式参数
  - 不再把实例宿主重复塞进参数数组

### 3. 先只迁移“表达式安全型”条目

第一阶段只迁移这类候选：

- 返回常量表达式
- 返回简单宿主 helper 调用
- 需要少量 AST 判断但不引入 temp 的逻辑

不应在第一阶段迁移：

- 需要声明提升
- 需要导入绑定
- 需要多步 Sequence 之外的语句级改写
- 需要读 `SenseArgument` 的 lowering

也不应迁移这类条目：

- 其实已经能稳定用 `Inline` 表达，只是实现上暂时懒得写模板
- 更适合作为模块 helper 的运行时逻辑，只是为了少写一个 `Import` 实现而想塞进 `Compile`

### 4. 建立 `Compile_*` 模板样板

建议固定实现骨架：

```csharp
public Expression? Compile_xxx(Expression? handler, Expression?[] args)
{
    // 1. 参数数量和形状校验
    // 2. 不满足条件时 return null
    // 3. 命中后返回最终 Expression
}
```

这样可以把：

- “decline”
- “claim and succeed”
- “claim and fail”

三种路径写得很清楚。

## 第二阶段才考虑的事

如果后续希望 `Op.Compile` 承担真正的复杂宿主 lowering，需要先扩 hook contract，而不是直接往现有签名里塞逻辑。

### 可能需要新增的上下文

- `SenseArgument`
- 原始 `IOperation`
- 稳定临时变量名生成入口
- import 收集入口
- source-origin 写入入口

### 可能需要改变的返回模型

当前返回 `Expression?`。

如果以后要支持：

- 声明提升
- 语句级展开
- 复合返回值

就应考虑升级成更明确的结果对象，而不是继续复用单个 `Expression?`。

## 测试清单

### A. 分发优先级

至少覆盖：

- 命中 `Compile` 时不再进入 `Alias`
- 命中 `Compile` 时不再进入 `Inline`
- 命中 `Compile` 时不再进入 `Import`
- 未命中 `Compile` 时仍能正常落回后续链路

### B. 返回语义

至少覆盖：

- `Compile_*` 返回表达式
- `Compile_*` 返回 `null`
- `Compile_*` 抛异常

### C. 参数布局

至少覆盖：

- 实例方法：`handler != null`，`args` 只含真实参数
- 静态方法：`handler == null`
- getter：空 `args`
- setter：`args` 只有新值

### D. 已有白名单兼容性

至少覆盖：

- 现有 `Alias` 条目不回归
- 现有 `Inline` 条目不回归
- 现有 `Import` 条目不回归

## Producer 侧约束

`Jazor.CLR` / `ECMAScript` 侧在增加新的 `[Jazor(Op.Compile)]` 条目前，应先过一遍筛选：

### 总优先级

producer 侧优先按这个顺序判断：

1. `Allowed` / `Alias`
2. `Inline`
3. `Import`
4. `Compile`

重点是：

- `Alias` 只有在成员名和最终 JS 宿主都稳定时才保留；若宿主可能退化到错误 host，优先回到 `Inline`
- 只要 `Inline` 能稳定表达，就不要为了方便写模块函数而退到 `Import`
- `Import` 保留给真正需要运行时实现的场景
- `Compile` 只保留给编译器内部必须直接接管的少数特例

### 可以直接标 `Compile`

- `Inline` 结构上不稳定
- 同时也不适合作为 `Import` helper
- 仍然能落成单个表达式
- 不需要临时变量与导入
- 且语义属于编译器内部保留特例，不是普通运行时库映射

### 暂时不要标 `Compile`

- 需要多条语句
- 需要 import
- 需要声明或临时缓存
- 需要依赖 pattern / tuple / ref-out 上下文
- 只是返回稳定常量，例如 `bool.GetTypeCode()`

这类条目现在即便挂成 `Compile`，后面也只能在接线时卡住。

当前仓库里的保留样例是 `ECMAScript.Global.TypeOf(object)`。
像 `bool.GetTypeCode()` 这种稳定常量已经回落到 `Inline`，不再占用 compile 通道。

### 也不要直接标 `Import`

如果它满足：

- 单表达式可稳定表示
- 不需要运行时复用 helper
- 不需要模块状态

那就应先回到 `Inline` 评估，而不是直接进入 `Import`

## 推荐推进顺序

1. 先完成主分发接线
2. 先补返回语义和参数布局测试
3. 先迁移 1 到 3 个“表达式安全型”条目
4. 跑一轮白名单回归
5. 再把“其实应回到 `Inline/Import` 的条目”从 `Compile` 清走
6. 再评估是否需要 contract 扩展

## 完成标准

第一阶段完成时，应同时满足：

- `GetWhiteListExpression(...)` 已接入 `_whiteListCompiles`
- 返回语义被测试锁定
- `handler/args` 布局被文档和测试同时锁定
- 至少有一个真实 `Compile_*` 条目不再返回 `null`
- 没有把需要 temp/import 的逻辑硬塞进当前签名

## 相关文档

- [OpCompileSpec.md](./OpCompileSpec.md)
- [InlineAstTemplateSpec.md](./InlineAstTemplateSpec.md)
- [TransformationClosureChecklist.md](./TransformationClosureChecklist.md)
- [TransformationRoadmap.md](./TransformationRoadmap.md)
