# `SemanticWalker.cs.Reference.cs`

## 目录

- [定位](#定位)
- [职责](#职责)
- [名称与宿主选择顺序](#名称与宿主选择顺序)
- [运行时宿主归一化](#运行时宿主归一化)
- [导入式宿主](#导入式宿主)
- [特殊字段与边界值](#特殊字段与边界值)
- [边界](#边界)
- [相关测试](#相关测试)
- [延伸阅读](#延伸阅读)

## 定位

`SemanticWalker.cs.Reference.cs` 负责把“引用类” `IOperation` 转成对应的 JavaScript AST。

这里的“引用”不只是简单的 `obj.member`。当前文件同时处理：

- 局部变量、参数、`this`
- 字段、属性、索引器
- 方法组引用、普通调用
- 静态成员宿主修正
- ECMAScript 运行时宿主归一化
- 导入式宿主成员
- `ref` / `out` 调用回写

对应代码文件：

- `src/Jazor.Compiler/core/SemanticWalker.cs.Reference.cs`

## 职责

这部分逻辑可以分成五类。

### 1. 基础引用

最简单的局部变量、参数、实例引用会直接落成基础 AST：

- `VisitLocalReference` -> `Identifier`
- `VisitParameterReference` -> `Identifier`
- `VisitInstanceReference` -> `ThisExpression` / 其他实例表达式

这一层没有额外运行时修正，主要负责把 Roslyn 引用节点翻译成最直接的 JS 表达形式。

### 2. 成员访问

字段和属性访问都围绕 `MemberExpression` 构造，但会先经过若干映射步骤。

主要规则：

- 枚举值、特殊常量字段优先走专门映射
- 白名单别名优先于默认成员名
- 初始化器场景优先看 setter 映射
- 索引器和带参数属性会直接转成计算属性访问
- 静态属性需要额外做“最终宿主选择”

例如：

```csharp
array[0]
```

```js
array[0]
```

```csharp
Console.WriteLine
```

最终不会保留 `Console`，而是归一到真实运行时宿主。

### 3. 方法组与普通调用

`VisitMethodReference` 和 `VisitInvocation` 共用大部分宿主选择逻辑。

它们都会先决定：

1. 成员名是什么
2. 宿主是谁
3. 是否命中白名单 / intrinsic / import
4. 是否需要把 CLR 风格宿主改成真实 JS 宿主

之后再分别生成：

- 方法组引用
- `CallExpression`

这也是为什么“实例调用”和“方法组引用”都能共享同一套 `Console -> console`、`Bytes -> Uint8Array` 归一化逻辑。

但这里要特别澄清一条边界：

- `Reference` 负责的是“Roslyn 已经绑定好的成员，最终应落成哪个 JS 成员名、挂在哪个 JS 宿主上”
- 它不负责在运行时重新模拟一次 CLR overload dispatch

换言之，当前路线是：

- 普通调用场景下，Roslyn 先选中具体 `IMethodSymbol`
- `Reference` 再根据这个已绑定符号决定成员名和宿主
- 如果该方法在声明侧需要重载区分，就沿用 `Util.GetConfigOrSymbolName(...)` 产出的稳定签名 hash 后缀
- 如果它属于 ECMAScript runtime host API，则仍优先走白名单别名 / 宿主归一化，并可跳过普通方法重载后缀

方法组引用同样不是“构造一个 CLR 风格 overload object”。

当前它保的是：

- 指向已绑定方法对应的 JS 成员
- 必要时保留正确宿主
- 必要时通过局部 arrow / forwarder 保留后续调用语义

它不保的是：

- 基于实参类型的运行时二次重载判别
- 完整 CLR method-group conversion 面
- 一个能在 JS 侧继续参与 CLR 式 overload resolution 的运行时对象

### 4. 运行时静态宿主解析

这是当前 `Reference` 语法域里最重要的特殊逻辑之一。

问题本质：

- 成员的声明宿主不一定等于真实 JS 运行时宿主
- 调用点语法写下来的宿主也不一定就是最终应输出的宿主
- 既不能只信声明宿主，也不能只信语法文本

典型例子：

```csharp
using Bytes = ECMAScript.Uint8Array;

var bytes = Bytes.Of(1, 2, 3);
Number size = Bytes.BYTES_PER_ELEMENT;
Func<byte[], Uint8Array> factory = Bytes.Of;
```

应输出：

```js
let bytes = Uint8Array.of(1, 2, 3);
let size = Uint8Array.BYTES_PER_ELEMENT;
let factory = Uint8Array.of;
```

当前实现依赖这组 helper：

- `TryBuildPreferredRuntimeStaticMemberAccess(...)`
  统一决定静态属性、静态方法、方法组引用的最终宿主
- `TryGetStaticSourceHostTypeFromSyntax(...)`
  从调用点语法 + `SemanticModel` 恢复实际宿主类型
- `IsStaticHostOverrideCompatible(...)`
  用继承链、接口实现、泛型原型定义判断能否安全覆盖声明宿主
- `TryGetSpecializedRuntimeHostType(...)`
  从 self-typed 泛型约束恢复更具体的运行时宿主
- `TryBuildRuntimeHostExpression(...)`
  把宿主类型映射成最终 JS host 表达式

更完整说明见：

- [RuntimeStaticHostResolution.md](../RuntimeStaticHostResolution.md)

### 5. `ref` / `out` 回写

`VisitInvocation` 里对 `ref` / `out` 参数不会尝试模拟 CLR 引用语义对象。

当前策略是：

- 先把调用结果存到临时变量
- 再从返回结构中依次回写 `ref` / `out` 参数
- 用逗号表达式把“调用 + 回写 + 最终值”串起来

这和当前编译器其他 lowering 保持一致：优先保持结果等价和求值顺序正确，而不是引入新的运行时包装层。

## 名称与宿主选择顺序

当前 `Reference` 路径里，成员名和宿主大致按下面顺序确定。

### 1. 成员名

优先级：

1. 白名单别名
2. 显式名称配置
3. 默认符号名

这部分和 `Util.GetConfigOrSymbolName(...)`、`GetMethodConfigOrWhiteListName(...)`、`GetInitializerMemberName(...)` 配合完成。

对方法这里还要加一条：

- 若是普通用户方法，最终名可带稳定签名 hash，用来和其他同名 overload 区分
- 若是 ECMAScript runtime host API，优先保宿主 API 名称与宿主形态，不强行回退到 CLR overload surface

### 2. 宿主

普通情况下，宿主来源于：

- 实例表达式本身
- `BuildFullTypeName(...)`
- 导入式模块成员

ECMAScript 运行时映射场景下，还会额外经过：

- `NormalizeRuntimeReceiverHostInstance(...)`
- `NormalizeRuntimeReceiverHostCallee(...)`
- `TryBuildPreferredRuntimeStaticMemberAccess(...)`

换言之，`Reference` 最终产出的不是“按 C# 文本直接拼出来的宿主”，而是“在当前规则下最接近真实 JS host / member 形态的宿主”。

## 运行时宿主归一化

当前文件明确处理了一类较为常见的割裂来源：C# 为了可书写性必须保留 CLR 风格名称，但 JS 运行时实际是另一个宿主名。

典型例子：

```csharp
Console.WriteLine("x");
```

输出：

```js
console.log("x");
```

这里不是简单“成员重命名”。

完整过程通常包含两层：

1. `WriteLine` -> `log`
2. `Console` -> `console`

前者主要由白名单/名称映射负责，后者主要由 `Reference` 里的运行时宿主归一化负责。

## 导入式宿主

如果类型带有 `[ECMAScriptModule(...)]`，`Reference` 会优先把对应成员视为导入式宿主成员。

当前行为：

- 引用阶段会把 `ImportSpecifier` 合并到上下文
- 成员访问本身直接落成局部标识符引用
- 模块根类型不会重复把自己当成导入成员

这让“按模块导入的宿主类型”和“全局运行时宿主类型”能共用同一套引用转换入口。

## 特殊字段与边界值

`GetFieldName(...)` 还承担一部分运行时常量映射：

- `double.PositiveInfinity` -> `Infinity`
- `double.NaN` / `float.NaN` -> `NaN`
- `double.Epsilon` -> `Number.EPSILON`
- `double.MaxValue` -> `Number.MAX_VALUE`
- `long.MaxValue` / `long.MinValue` -> bigint 字面量

这部分属于“字段引用路径里的值语义修正”，不是单独的 creation / ordinary 逻辑。

## 边界

这份文件当前解决的是“引用与调用如何落成正确 JS 访问路径”。

它并未承担以下职责：

- 模拟完整 CLR overload dispatch
- 建立独立的运行时包装宿主层
- 为所有继承问题提供统一 CLR 级语义仿真
- 让 `ref` / `out` 变成真实引用对象
- 把成员类构造函数 dispatcher 逻辑搬进引用层

这里再精确一点：

- 普通方法的“重载区分”主要是声明/命名侧契约，`Reference` 只消费那个稳定结果
- 构造函数重载的“单 `constructor` + `$ctor_<hash>` helper + `arguments.length` 分派”属于 `AstConverter` 的类声明侧协议，不属于引用域
- `Reference` 真正要做的是：在调用点别把已经选定的方法挂错宿主、叫错名字，或者把 host API 重新污染成 CLR 风格外观

换句话说，这里的目标一直是：

- 保持当前编译器 lowering 结果稳定
- 尽量恢复真实 JS host / member 形态与可观察协议
- 不额外制造 C# / JS 的新割裂

## 相关测试

主要测试在：

- `src/Jazor.CompilerTest/SemanticWalkerReferenceTest.cs`

其中建议重点关注这些场景：

- 运行时宿主归一化
- 静态宿主选择
- `using` 类型别名不泄漏到 JS
- 方法组引用与普通调用共用宿主修正
- 索引器和数组元素访问
- `ref` / `out` 回写

与静态宿主解析直接相关的测试示例：

- `Visit_MethodReference_TypedArrayStaticMethod_UsesConcreteRuntimeHost`
- `Visit_Reference_TypedArrayAliasHost_UsesConcreteRuntimeHost`
- `Visit_Reference_RuntimeStaticProperty_UsesImplicitEcmascriptMemberName`

## 延伸阅读

- [SemanticWalker.md](./SemanticWalker.md)
- [SemanticWalker.WhiteList.md](./SemanticWalker.WhiteList.md)
- [RuntimeStaticHostResolution.md](../RuntimeStaticHostResolution.md)
- [SyntaxTransformationPipeline.md](../SyntaxTransformationPipeline.md)
