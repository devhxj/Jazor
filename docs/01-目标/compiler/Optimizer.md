# `Optimizer`

## 目录

- [定位](#定位)
- [当前职责](#当前职责)
- [当前关键规则](#当前关键规则)
- [现状与典型结果](#现状与典型结果)
- [当前边界](#当前边界)
- [相关测试](#相关测试)
- [延伸阅读](#延伸阅读)

## 定位

`Optimizer` 是一个范围较小的 AST 优化器。

对应代码：

- `src/Jazor.Compiler/Optimizer.cs`

它目前仅公开一个入口：

- `OptimizeLogical(Expression expression)`

所以这份文档应理解为：

- “逻辑表达式去重优化说明”

而不是：

- “通用 JS AST 优化框架总文档”

## 当前职责

### 1. 同运算符逻辑树去重

当前优化目标只有一类：

- 同一层级逻辑表达式中重复的纯子表达式

例如：

```js
a && a
```

会被简化为：

```js
a
```

而：

```js
(a && b) && a
```

会被简化为：

```js
a && b
```

### 2. 递归优化子树

即使最终不做去重，`OptimizeLogical(...)` 也会先递归优化：

- `logical.Left`
- `logical.Right`

所以它既是“顶层去重器”，也是逻辑树的递归整理入口。

### 3. 副作用保护

去重前，当前实现会先检测左右子树是否包含副作用。

只要任一侧判定为有副作用：

- 不做跨操作数去重
- 但如果子节点已经被递归优化，仍会重建当前 `LogicalExpression`

这条规则是当前优化器最重要的保守边界。

## 当前关键规则

### 1. 只处理 `LogicalExpression`

如果输入不是 `LogicalExpression`，当前直接原样返回。

换言之，这里不负责：

- 常量折叠
- 算术表达式简化
- 条件表达式重写
- 死代码消除

### 2. 只扁平化“相同运算符”的逻辑链

`Flatten(...)` 只会继续展开：

- `&&` 链中的 `&&`
- `||` 链中的 `||`

不会跨运算符混合扁平化。

所以：

```js
a && (b || a)
```

不会因为外层已经有 `a` 就把内层 `a` 去掉。

### 3. 去重键当前基于脚本串

当前唯一性判断使用的是：

- `operand.ToKnRECMAScript()`

换言之，优化器当前不是做结构哈希，而是用规范化后的 JS 文本串去重。

这很实用，也足够支撑当前规模，但应被理解为当前实现策略，而不是抽象语义相等判定框架。

### 4. 副作用检测是保守的

`IsEffect(...)` 只把一部分明确无副作用的节点当成 pure：

- `Identifier`
- `Literal`
- `ThisExpression`
- `Super`
- 函数 / 类表达式定义本身
- 若干递归可判定节点

其他节点默认按“可能有副作用”处理。

例如：

- 调用表达式
- 赋值
- 更新表达式

都会阻止去重优化。

### 5. `MemberExpression` 的计算属性访问也会阻止优化

当前对 `MemberExpression` 的判定里，只要：

- `Computed == true`

就视为有副作用风险。

这说明优化器对索引访问、动态 key 访问是保守处理的。

## 现状与典型结果

### 简单重复

```js
a && a
```

```js
a
```

### 多操作数去重

```js
(a && b) && a
```

```js
a && b
```

### 不同运算符不混合

```js
a && (b || a)
```

当前不会被进一步改成别的结构。

### 含副作用时保持保守

```js
a && foo()
```

当前不会尝试把 `foo()` 与其他文本相同片段做跨树去重。

## 当前边界

这部分当前已经解决的是：

- `&&` / `||` 逻辑树递归去重
- 基于副作用保护的保守优化
- 左结合重建

它并未承担以下职责：

- 常量折叠
- 死代码消除
- 一元 / 二元表达式代数简化
- 通用插件化优化框架

## 相关测试

主要测试在：

- `src/Jazor.CompilerTest/OptimizerTest.cs`

建议重点关注以下场景：

- `OptimizeLogical_SimpleAndDuplicate_ReturnsSingleOperand`
- `OptimizeLogical_SimpleOrDuplicate_ReturnsSingleOperand`
- `OptimizeLogical_ThreeOperandsWithDuplicate_RemovesDuplicate`
- `OptimizeLogical_IdenticalSubExprWithDifferentOps_Deduplicated`
- `OptimizeLogical_NestedDifferentOperators_PreservesStructure`
- `OptimizeLogical_ComplexNestedExpression_OptimizesCorrectly`

## 延伸阅读

- [SemanticWalker.Pattern.md](./semantic-walker/SemanticWalker.Pattern.md)
- [SemanticWalker.Ordinary.md](./semantic-walker/SemanticWalker.Ordinary.md)
- [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
