# ECMAScript 运行时静态宿主解析

## 目录

- [1. 这份文档说明什么](#1-这份文档说明什么)
- [2. 问题本质](#2-问题本质)
- [3. 设计约束](#3-设计约束)
- [4. 当前算法](#4-当前算法)
- [5. 继承问题是怎么全局处理的](#5-继承问题是怎么全局处理的)
- [6. 典型例子](#6-典型例子)
- [7. 为什么不是别的方案](#7-为什么不是别的方案)
- [8. 当前边界](#8-当前边界)
- [9. 相关测试](#9-相关测试)
- [10. 结论](#10-结论)

## 1. 这份文档说明什么

本文档说明 `SemanticWalker.cs.Reference.cs` 里“运行时静态成员最终挂到哪个 JS 宿主上”的决策逻辑。

对应实现主要在：

- `TryBuildPreferredRuntimeStaticMemberAccess(...)`
- `TryGetStaticSourceHostTypeFromSyntax(...)`
- `IsStaticHostOverrideCompatible(...)`
- `TryGetSpecializedRuntimeHostType(...)`
- `TryBuildRuntimeHostExpression(...)`

它解决的不是普通命名映射，而是下面这类更容易出错的场景：

- C# 声明宿主和 JS 真实宿主不一致
- 静态成员声明在基类上，但运行时应该挂到具体子类型上
- C# 使用了 `using` 类型别名，语法名字和真实类型不一致
- 运行时宿主来自 `extension(receiver)` 这种桥接类型，而不是普通 CLR 类型名

## 2. 问题本质

Jazor 的目标不是造一层新的 “JsObject / JsArray” 包装世界，而是尽量让 C# 写法和最终 JS host / member 形态对齐。

这会带来一个直接约束：

- 编译器输出时，应尽量恢复“真实 JS 宿主”
- 不能机械地把 Roslyn 看到的声明宿主原样发出去

典型反例：

```csharp
using Bytes = ECMAScript.Uint8Array;

var bytes = Bytes.Of(1, 2, 3);
Number size = Bytes.BYTES_PER_ELEMENT;
```

如果只信语法文本，结果会变成：

```js
Bytes.of(1, 2, 3)
Bytes.BYTES_PER_ELEMENT
```

这显然不对。最终应该是：

```js
Uint8Array.of(1, 2, 3)
Uint8Array.BYTES_PER_ELEMENT
```

再看另一个反例：

```csharp
TypedArray<byte, Uint8Array>.Of(...)
```

如果只信成员声明位置，可能会退回泛型基类宿主：

```js
TypedArray.of(...)
```

但 JS 运行时真正存在的是：

```js
Uint8Array.of(...)
```

所以这里的核心矛盾是：

1. Roslyn 的“声明宿主”适合做语义归属
2. JS 的“运行时宿主”才适合做最终输出
3. 两者不总是同一个东西

## 3. 设计约束

这套逻辑受下面这些约束限制。

### 3.1 不应通过名字硬编码

不能写这种判断：

```csharp
ContainingType?.Name == "Global"
Name == "Console"
```

原因很简单：

- 同名类型并不可靠
- 用户可以起别名
- 未来映射宿主会继续扩展
- 这会把规则绑死在个别类型上，无法全局推广

当前实现改为依赖：

- `[ECMAScript]`
- `[ECMAScriptModule]`
- 白名单别名
- 已有的类型映射和名称边界规则

换言之，规则建立在“运行时映射事实”上，而不是建立在“某几个名字恰好这么叫”上。

### 3.2 不能在每个具体宿主上重复声明静态成员

一种表面上简单的方案是把基类上的静态成员复制到每个具体子类型上。

这个方案被拒绝，原因是：

- 手写映射会越来越脆
- 生成层和手写层都要同步维护
- 它掩盖了真正的问题：编译器没有在输出阶段恢复真实宿主

所以当前方案把修复点放在编译器，而不是把运行时声明层污染成“到处重复一份”。

### 3.3 不能引入新的包装宿主类型

这和整体设计目标一致：

- 目标是尽量减少 C# / JS 的割裂
- 大小写差异和少量 C# 语法让步可以接受
- 引入 `JsObject` 之类的新宿主会放大认知割裂

所以宿主解析必须直接围绕现有 ECMAScript 映射类型完成。

### 3.4 C# 语法限制必须承认

有些映射不是“最像 JS 的写法”，而是“在 C# 约束下最接近 JS 的写法”。

例如：

- 可能需要 `_` 后缀来避开名字冲突
- 有些静态入口在 C# 中只能放到某个可声明的宿主下
- `extension(receiver)` 这样的桥接宿主需要额外恢复真实 receiver

这部分不是设计失误，而是对语言边界的显式处理。

## 4. 当前算法

当前静态宿主解析遵循下面的顺序。

### 4.1 先确认是不是 ECMAScript 运行时符号

只有 ECMAScript 运行时类型/成员才走这套特殊逻辑。

普通 C# 静态成员仍走普通路径。

目的明确：

- 避免把普通用户代码误改写
- 把宿主修正严格限制在 runtime mapping 领域

### 4.2 先从声明宿主构造一个“稳定运行时宿主”

这一层由 `TryBuildRuntimeHostExpression(...)` 完成。

它按优先级做几件事：

1. 尝试从 self-typed 泛型约束恢复更具体的宿主
2. 复用现有类型映射、白名单别名、导入规则、名称边界
3. 必要时再从 `extension(receiver)` 桥接宿主里恢复 receiver 对应的 JS host

例如：

- `System.Console` -> `console`
- `TypedArray<byte, Uint8Array>` -> `Uint8Array`

### 4.3 再从调用点语法恢复“用户实际使用的宿主类型”

这一层由 `TryGetStaticSourceHostTypeFromSyntax(...)` 完成。

关键点是：这里不能只读语法文本，必须结合 `SemanticModel`。

原因：

- `Bytes.Of(...)` 里的 `Bytes` 可能是类型别名
- `Outer.Inner.Member` 需要恢复最终绑定到的类型
- Roslyn 给属性访问、方法引用、调用表达式的语法颗粒度并不完全一致

所以这里先把 syntax 归一到可分析的宿主表达式，再取：

- alias 目标类型
- 直接绑定到的类型符号
- 或语义推断出来的类型信息

### 4.4 只有在“调用点宿主和声明宿主兼容”时才允许覆盖

这一层由 `IsStaticHostOverrideCompatible(...)` 判断。

兼容条件包括：

- 两者是同一个类型
- 两者 `OriginalDefinition` 相同
- 调用点宿主在声明宿主的基类链上
- 调用点宿主实现了声明宿主对应接口

这一步的目的不是“尽量改”，而是“只在确认它们属于同一套运行时 API 时才改”。

这样可以同时满足两点：

- 支持继承/泛型基类复用静态成员
- 避免把无关宿主错误并到一起

### 4.5 语义宿主恢复成功时，优先使用语义宿主

这是最稳妥的路径。

因为它能自动消掉纯语法层噪音，例如：

- `using Bytes = Uint8Array`
- 命名空间前缀
- 某些语法写法差异

一旦恢复出语义宿主，再经过运行时映射，最终输出会是稳定的 JS 宿主。

### 4.6 语义宿主恢复失败时，按“尽量不降级”的原则回退

如果声明宿主本身是泛型基类，且当前拿不到更具体的语义宿主，那么不能轻易退回声明宿主。

否则会把：

```js
Uint8Array.of
```

降成：

```js
TypedArray.of
```

这时的回退策略是：

- 能保留调用点语法宿主时，先保留它
- 只有在能从声明宿主自身恢复出更具体运行时宿主时，才强制输出那个恢复后的宿主

这个策略的核心不是“语法优先”，而是“不要把已经写成具体类型的调用点降成抽象宿主”。

## 5. 继承问题是怎么全局处理的

这次的关键决策是：继承问题在编译器里全局处理，而不是给某个宿主家族打补丁。

换言之，当前逻辑不是只为 `TypedArray` 服务。

它的推广方式是：

1. 先拿到声明宿主
2. 再拿到调用点宿主
3. 只要两者在继承/接口/原型定义上兼容，就允许调用点宿主覆盖声明宿主

这样带来的好处是：

- 不需要手写 `Uint8Array.BYTES_PER_ELEMENT` 这种重复声明
- 不需要为每个特例单独加 `if (Name == "...")`
- 同一套逻辑可以覆盖未来更多“基类声明，具体宿主运行”的映射

## 6. 典型例子

### 6.1 普通运行时宿主改名

```csharp
Console.WriteLine("x");
```

输出：

```js
console.log("x");
```

这里主要依赖运行时宿主映射本身。

### 6.2 类型别名不应泄漏到 JS

```csharp
using Bytes = ECMAScript.Uint8Array;

var bytes = Bytes.Of(1, 2, 3);
```

输出：

```js
let bytes = Uint8Array.of(1, 2, 3);
```

这里依赖“调用点语义宿主恢复”。

### 6.3 基类声明的静态成员仍应挂到具体运行时构造器

```csharp
Number size = Uint8Array.BYTES_PER_ELEMENT;
```

输出：

```js
let size = Uint8Array.BYTES_PER_ELEMENT;
```

这里依赖“继承兼容 + 具体宿主保留”。

### 6.4 self-typed 泛型约束恢复真实宿主

```csharp
Func<byte[], Uint8Array> factory = Uint8Array.Of;
```

输出：

```js
let factory = Uint8Array.of;
```

这里依赖 `TryGetSpecializedRuntimeHostType(...)` 从泛型类型参数恢复真实构造器。

## 7. 为什么不是别的方案

### 7.1 不是“全部只看声明宿主”

因为会把具体运行时宿主退化成抽象基类宿主。

### 7.2 不是“全部只看调用点文本”

因为会把 `using` 别名、局部命名差异、纯语法噪音直接泄漏到最终 JS。

### 7.3 不是“每个具体类型都重复声明一遍静态成员”

因为那是在 runtime host 层补洞，不是在编译器层解决问题。

### 7.4 不是“靠名字特判 Global / Console / Object”

因为那样不可扩展，也不稳健。

## 8. 当前边界

这套逻辑当前主要覆盖“运行时静态成员宿主选择”，不等于整个继承系统已经完全打通。

它明确解决的是：

- 静态属性引用
- 静态方法调用
- 静态方法组引用

它没有承诺解决所有”继承 + 最终 JS 宿主形态”问题，例如：

- 用户自定义继承体系的所有语义差异
- 非 ECMAScript 运行时类型的静态分派
- 需要专门运行时协议支持的更复杂继承场景

## 9. 相关测试

当前至少应关注这些测试：

- `Visit_MethodReference_TypedArrayStaticMethod_UsesConcreteRuntimeHost`
- `Visit_Reference_TypedArrayAliasHost_UsesConcreteRuntimeHost`
- `Visit_Reference_RuntimeStaticProperty_UsesImplicitEcmascriptMemberName`

位置：

- `src/Jazor.CompilerTest/SemanticWalkerReferenceTest.cs`

这些测试分别覆盖：

- 泛型基类静态方法的具体宿主输出
- `using` 类型别名不会泄漏到 JS
- 静态属性引用仍保持真实运行时宿主

## 10. 结论

当前方案的核心思想可以压缩成一句话：

> 静态成员属于谁，由声明宿主决定；最终发到哪个 JS host，由运行时映射后的“真实兼容宿主”决定。

这也是当前在“不引入额外包装类型、不靠名字硬编码、承认 C# 语法限制”的前提下，最贴近 JS 运行时事实的做法。
