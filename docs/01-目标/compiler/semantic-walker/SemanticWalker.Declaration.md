# `SemanticWalker.cs.Declaration.cs`

## 定位

`SemanticWalker.cs.Declaration.cs` 负责声明和初始化器相关 lowering。

对应代码：

- `src/Jazor.Compiler/core/SemanticWalker.cs.Declaration.cs`

当前文件覆盖的不是“所有声明语义”，而是这几类非常具体的节点：

- 数组初始化器
- 字段 / 属性 / 变量初始化器
- 变量声明符 / 变量声明 / 声明组
- `out` / declaration expression

## 当前职责

### 1. 初始化值向目标类型对齐

当前文件里一个重要主线是：

- 初始化器不仅翻译值，还会按目标类型做 tuple 视图对齐

这体现在：

- `VisitArrayInitializer(...)`
- `VisitFieldInitializer(...)`
- `VisitPropertyInitializer(...)`
- `VisitVariableDeclarator(...)`

它们都会在适当位置调用 `TranslateTupleForTarget(...)`。

换言之，这份文件不只是“把右值塞给左值”，还承担了声明边界上的 tuple 视图/对象协议对齐。

### 2. 数组初始化器

`VisitArrayInitializer(...)` 会遍历元素列表，并根据父级数组类型推导元素目标类型。

典型结果：

```csharp
new int[] { 1, 2, 3 }
```

```js
[1, 2, 3]
```

如果元素本身是 tuple，当前还会按目标元素类型做 remap，而不是简单保留原投影视图。

### 3. 普通变量声明

`VisitVariableDeclarator(...)`、`VisitVariableDeclaration(...)`、`VisitVariableDeclarationGroup(...)` 共同负责把局部变量声明落成：

```js
let ...
```

典型结果：

```csharp
int x = 5, y = 10;
```

```js
let x = 5, y = 10;
```

当前实现统一使用 `let`，不在这里区分 `const`。

### 4. 字段 / 属性初始化器

`VisitFieldInitializer(...)` 和 `VisitPropertyInitializer(...)` 当前只返回初始化值表达式本身。

这说明这两个入口主要承担：

- 为上层 class / member 转换提供初始化表达式

而不是在这里直接生成完整类成员语法。

### 5. `out` 声明表达式

`VisitDeclarationExpression(...)` 当前服务于：

- `out var result`
- `out int value`

这条路径的关键点不是直接输出一条 `let` 语句，而是：

- 先返回标识符表达式
- 如果当前语义是 `Sense.OutParameter`，把对应 declarator 收集进 `SenseArgument`

之后由外层 block 在合适位置统一 flush 出声明。

## 当前关键规则

### 1. tuple remap 发生在声明边界

当前这些入口都可能成为 tuple 视图切换边界：

- 数组元素
- 字段初始化
- 属性初始化
- 变量声明初始化

所以 `Declaration` 文件和 `Tuple` 路径是直接耦合的，而不是完全独立。

### 2. 普通局部声明直接生成 `let`

`VisitVariableDeclaration(...)` 和 `VisitVariableDeclarationGroup(...)` 当前都会直接返回：

```js
let ...
```

这和旧文档里“声明位置分散仍待收集”的说法不一致。当前真实情况是：

- 普通局部声明直接按当前位置输出
- `out` 变量等需要预声明的场景，才通过上下文收集后再由外层统一输出

### 3. `out` 变量的预声明不在 `VisitDeclarationExpression(...)` 内直接写出

`DeclarationExpression` 当前只做两件事：

1. 返回表达式形式的变量名
2. 在 `Sense.OutParameter` 下登记 declarator

真正的：

```js
let result;
```

会在更外层 block flush 时出现。

### 4. 字段 / 属性初始化器不负责成员壳

这两个入口当前只关心“初始化值是什么”，不负责类成员结构本身。

## 现状与典型结果

### 数组初始化器

```csharp
var numbers = new int[] { 1, 2, 3 };
```

```js
let numbers = [1, 2, 3];
```

### 变量声明组

```csharp
int a = 1, b = 2, c;
string x = "hello", y = "world";
```

```js
let a = 1, b = 2, c;
let x = "hello", y = "world";
```

### `out var`

```csharp
if (int.TryParse(input, out var result))
{
    Console.WriteLine(result);
}
```

```js
let result, v$0;
if (v$0 = _16e2a901535b765e(input, result), result = v$0[1], v$0[0]) {
  console.log(result);
}
```

这里可以看到：

- `result` 的声明被提前到外层
- `DeclarationExpression` 本身只是整个 lowering 链上的一环

### 字段 / 属性初始化器的 tuple 对齐

```csharp
private (string first, int years) person = (name: "John", age: 30);
```

```js
{ first: "John", years: 30 }
```

## 当前边界

这部分当前已经解决的是：

- 数组初始化器
- 普通局部声明
- 字段 / 属性初始化器取值
- `out` 声明表达式收集
- 声明边界上的 tuple remap

它并未承担以下职责：

- 在这里处理完整类成员生成
- 在这里决定所有变量声明最终 flush 位置
- 区分 `const` / `let`
- 支持 `using` 声明

其中 `using` 相关语义当前明确属于不支持路径。

## 相关测试

主要测试在：

- `src/Jazor.CompilerTest/SemanticWalkerDeclarationTest.cs`

建议重点关注以下场景：

- `Visit_ArrayInitializer`
- `Visit_VariableDeclaration`
- `Visit_VariableDeclarationGroup`
- `Visit_DeclarationExpression_OutVar`
- `DirectVisit_FieldInitializer_TupleRemapByTargetType`
- `DirectVisit_PropertyInitializer_TupleRemapByTargetType`
- `DirectVisit_VariableDeclarator_TupleRemapByTargetType`

## 推荐阅读

建议按以下顺序阅读：

1. [SemanticWalker.md](./SemanticWalker.md)
2. [SemanticWalker.Declaration.md](./SemanticWalker.Declaration.md)
3. [SemanticWalker.Tuple.md](./SemanticWalker.Tuple.md)
4. [WalkerArgument.md](./WalkerArgument.md)

## 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [SemanticWalker.Tuple.md](./SemanticWalker.Tuple.md)
- [WalkerArgument.md](./WalkerArgument.md)
- [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)
