# `AstConverter`

## 目录

- [定位](#定位)
- [当前职责](#当前职责)
- [当前关键规则](#当前关键规则)
- [现状与典型结果](#现状与典型结果)
- [当前边界](#当前边界)
- [相关测试](#相关测试)
- [延伸阅读](#延伸阅读)

## 定位

`AstConverter` 负责把一个带 `[ECMAScriptModule]` 约束的顶层类型转换成模块级 ESTree `Module`。

对应代码：

- `src/Jazor.Compiler/AstConverter.cs`

它位于整条链路中间：

```text
INamedTypeSymbol + SemanticModel
    -> AstConverter
    -> Acornima Module
    -> ESGenerator 收集为 module catalog / source map carriers
    -> Jazor.Emit 物化为文件
```

所以 `AstConverter` 不直接负责增量生成器生命周期，也不负责方法体内部细节 lowering；后者主要委托给 `SemanticWalker`。

## 当前职责

### 1. 顶层模块类型转换

`Convert()` 当前要求输入类型满足这些前提：

- 必须是 `public`
- 必须是顶层类型

如果不是：

- 非 `public` -> 直接抛 `NotSupportedException`
- 嵌套模块类型 -> 直接抛 `NotSupportedException`

这和旧文档里“`internal` 也导出”已经不一致，当前实现是更严格的。

### 2. 模块成员分发

当前 `Convert()` 会遍历 `_classSymbol.GetMembers()`，并按成员种类分发：

- `IFieldSymbol` -> `ConvertModuleField(...)`
- `IMethodSymbol` -> `ConvertModuleMethod(...)`
- 嵌套 `class` -> `ConvertModuleClass(...)`
- 嵌套 `enum` -> `ConvertModuleEnum(...)`

而这几类成员：

- `IPropertySymbol`

在顶层模块成员遍历中当前不会直接处理，它们依赖字段 / 访问器方法路径落地。这点也说明当前模块层的“属性”本质上是由 backing field 和 getter/setter method 组合出来的。

### 3. 模块级导出规则

当前导出规则直接：

- `public` / `internal` -> 生成 `ExportNamedDeclaration`
- 其他访问级别 -> 只生成本地声明，不导出
- 不支持 `default export`；如果配置名或推导名解析成 `default`，当前应显式失败

判断逻辑在：

- `ShouldBePrivate(...)`

所以顶层模块类型自身必须是 `public`，但其内部成员仍允许有私有实现成员。

### 4. 方法体与表达式体转换

`ConvertModuleMethod(...)` 会从声明语法里提取：

- block body
- expression body
- 自动属性访问器的隐式体

拿到对应 `IOperation` 后，再委托：

- `SemanticWalker`

生成函数体 AST。

如果 walker 在过程中收集了 import 依赖，`AstConverter` 会通过：

- `MergeImports(...)`

把依赖合并到模块级 import 表中。

### 5. 嵌套成员类 / 枚举

当前 `AstConverter` 支持把模块内部的非静态成员类转换出来；枚举和接口则按“只保编译期角色，不发射 runtime declaration”处理：

- 成员类 -> `ClassDeclaration`
- 成员 `record` -> 不发射 runtime declaration，使用点统一交给 structural lowering
- 成员枚举 -> 不发射模块级声明对象，使用点由 `SemanticWalker` 常量化
- 成员接口 -> 不发射 runtime declaration

成员类内部又支持：

- 字段
- 属性（拆成 backing field + getter/setter）
- 构造函数
- 普通方法
- 同模块成员类继承的受控子集：`extends`、显式 `: base(...)` 到 `super(...)`、无显式构造函数时合成 `super()`
- 构造函数重载的受控子集：单真实 `constructor` + `$ctor_<hash>` helper + 已绑定构造函数 selector 分派

这和旧文档里“不支持嵌套类扁平化”的说法也不同。当前实现不是扁平化，而是直接生成模块内类声明。

但这里必须额外强调两个方向约束：

- interface 一律不应发射 runtime artifact，它只是一种契约；
- record 一律不应被偷偷抬升回 nominal runtime class；若需要 `class` 语义，必须显式写 `class`；
- 继承如果要支持，必须真的输出 `extends` / `super` 相关语义，不能静默擦除。

### 6. `ref` / `out` 返回协议包装

模块方法或成员类方法只要参数里存在：

- `ref`
- `out`

当前就会在函数体上应用：

- `ApplyRefOutReturnProtocol(...)`

规则是：

- 原始返回值和 `ref` / `out` 参数一起包装成数组返回
- 显式 `return` 会被重写
- 无显式返回但有 `ref` / `out` 时，会在函数尾补一个返回数组

这让 `AstConverter` 在“函数壳层”完成 `ref` / `out` 协议，而不是把这部分逻辑塞进 `SemanticWalker` 的每个调用点。

## 当前关键规则

### 1. `Convert()` 产出的是 AST `Module`，不是最终文件

`AstConverter` 当前只生成：

- `Acornima.Ast.Module`

不是：

- `.mjs` 文件
- 嵌入资源
- 运行时模块对象

后续如何把 AST 序列化为文本、收集进 catalog，并最终物化为文件，分别由 writer / `ESGenerator` / `Jazor.Emit` 负责。

### 2. import 由 walker 收集、由 converter 提升

当前 import 处理路径是：

1. `SemanticWalker` 在方法 / 表达式转换中登记 import specifier
2. `SenseArgument.FlushImportSpecifiers()`
3. `AstConverter.MergeImports(...)`
4. `BuildImportDeclarations()` 生成模块头 `ImportDeclaration`

这说明 import 收集不是旧文档里说的“未实际使用”，当前主链已经接通；后续重点是稳定去重、排序和别名策略。

### 3. 顶层属性不直接走 `IPropertySymbol`

在模块成员枚举里，`IPropertySymbol` 当前分支直接 `break`。

换言之，顶层模块属性的可见行为依赖：

- backing field
- getter/setter method

而不是单独的“模块属性转换器”。

### 4. 成员类方法重载与构造函数重载走不同路线

普通成员方法仍然可以通过“不同 JS 成员名”展开：

- 默认名字统一走 `Util.GetConfigOrSymbolName(...)`
- 只有在确实存在同名方法重载时，才追加稳定签名 hash
- ECMAScript runtime host 上的方法默认跳过重载后缀，避免把宿主 API 人为拆裂

构造函数不能这样处理，因为 JS class 运行时只允许一个真实 `constructor`。

因此当前成员类构造函数重载固定为：

- 一个真实 `constructor`
- 零个或多个 `$ctor_<hash>` helper method
- Jazor 编译产物内部的 `new C(...)` / `super(...)` 在需要重载分派时传入 `$ctor_<hash>` selector
- `constructor` 内按 selector 分派，不保留 `arguments.length` fallback
- 命中分支后补齐该 overload 自身的 optional 默认值
- 派生类分支里先 `super(...)`，再调用 helper

### 5. 成员类支持实例构造函数，但不支持静态构造函数

当前代码显式支持成员类实例构造函数：

- `ConvertMemberConstructor(...)`
- `ConvertMemberConstructorDispatcher(...)`
- `ConvertMemberConstructorHelper(...)`

但显式拒绝：

- 模块静态构造函数
- 成员类静态构造函数
- `this(...)` 构造函数链
- `ref/out/in/params` 参与的构造函数分派
- 外部基类上的构造函数协议模拟

## 现状与典型结果

### 顶层静态字段和方法

```csharp
public static class TestClass
{
    public static int Field = 42;
    public static void Method() { }
}
```

```js
export let Field = 42;
export function Method() { }
```

### 自动属性

```csharp
public static class TestClass
{
    public static int Property { get; set; }
}
```

```js
let _38ee328c86b9b067;
export function get_Property() {
  return _38ee328c86b9b067;
}
export function set_Property(value) {
  _38ee328c86b9b067 = value;
}
```

### 模块内嵌套类

```csharp
public static class TestClass
{
    public class NestedClass
    {
        public int Field;
        public NestedClass(int value) { Field = value; }
    }
}
```

```js
export class NestedClass {
  Field;
  constructor(value) {
    this.Field = value;
  }
}
```

### 构造函数重载

```csharp
public static class TestClass
{
    public class NestedClass
    {
        public int Value;
        public NestedClass() { }
        public NestedClass(int value) { Value = value; }
    }
}
```

```js
export class NestedClass {
  Value;
  constructor() {
    let $args = arguments;
    let $ctor = $args[0];
    if ($ctor === "$ctor_<hash0>") {
      this.$ctor_<hash0>();
      return;
    }
    if ($ctor === "$ctor_<hash1>") {
      let value = $args[1];
      this.$ctor_<hash1>(value);
      return;
    }
    throw new Error("No matching constructor overload for NestedClass.");
  }
  $ctor_<hash0>() { }
  $ctor_<hash1>(value) {
    this.Value = value;
  }
}
```

这里的 `<hash0>` / `<hash1>` 表示基于构造函数签名生成的稳定后缀。  
重点不是 helper 的外观，而是：

- 真实 `constructor` 始终只有一个
- `new` 调用点按 Roslyn 已绑定构造函数传 selector
- helper 名稳定
- overload 选择结果稳定
- 默认值补齐与 `super(...)` 调用都留在明确分支里

### 成员类继承

```csharp
public static class TestClass
{
    public class BaseClass
    {
        public virtual int Value() => 1;
    }

    public class NestedClass : BaseClass
    {
        public override int Value() => base.Value() + 1;
    }
}
```

```js
export class BaseClass {
  Value() {
    return 1;
  }
}
export class NestedClass extends BaseClass {
  constructor() {
    super();
  }
  Value() {
    return super.Value() + 1;
  }
}
```

### 枚举

当前路线下，模块内 `enum` declaration 本身不再被当成一个独立 JS 输出对象。

换言之：

- `AstConverter` 不负责为它生成 runtime declaration
- `SemanticWalker` 负责把使用点改写成底层常量或标量表达式
- 任何依赖名字语义、反射语义或 `System.Enum` 家族 API 的能力都必须单独建模

因此 `enum` 的长期和当前实现方向现在是一致的：

- enum declaration = 编译期值域类型声明
- enum member usage = 底层常量
- enum typed runtime value = 标量值

这条路线的直接含义是：

- `AstConverter` 不承担 enum runtime declaration 发射；
- `SemanticWalker` 应承担 `E.A`、`default(E)`、比较、`switch`、`Flags` 位运算等使用点 lowering；
- 任何依赖枚举名字语义的能力，例如 `System.Enum` 家族 API、按名字格式化、反射式取值，都必须显式建模，否则默认失败。

## 当前边界

这部分当前已经解决的是：

- 顶层模块类型转 `Module`
- 字段、方法、成员类
- enum / interface declaration 擦除
- import 提升
- 自动属性 getter/setter 生成
- `ref` / `out` 返回协议
- 同模块成员类继承的 `extends` / `super(...)` / `super.member` 子集
- 成员类构造函数重载 dispatcher

它并未承担以下职责：

- 支持非 `public` 顶层模块类型
- 支持顶层嵌套模块类型
- 支持模块静态构造函数
- 支持所有成员种类（例如事件、委托等）
- 直接产出最终 `.mjs` 文件

文件物化边界仍然是：

- `AstConverter` 负责模块级 AST
- writer / `ESGenerator` 负责文本与 catalog carriers
- `Jazor.Emit` 负责 `.mjs` / `.mjs.map` 与 manifest 物化

它当前仍然显式拒绝这些路径：

- `this(...)` 构造函数链
- `base.Field`
- 外部基类
- `ref/out/in/params` 驱动的构造函数分派
- 任何仍依赖 CLR metadata identity 的 class runtime 语义

因此当前实现最需要避免的不是“支持少”，而是“把未打通语义呈现为已支持”。对不在支持子集内的成员类继承或构造函数协议，直接失败比生成看似可运行、实际语义错误的 class shape 更正确。

## 相关测试

主要测试在：

- `src/Jazor.CompilerTest/AstConverterTests.cs`

建议重点关注以下场景：

- `Convert_SimplePublicClass_ReturnsModule`
- `Convert_NonPublicClass_ThrowsNotSupportedException`
- `Convert_ClassWithStaticField_GeneratesVariableDeclaration`
- `Convert_ClassWithMethod_GeneratesFunctionDeclaration`
- `Convert_ClassWithProperty_GeneratesPropertyMethods`
- `Convert_ClassWithNestedClass_GeneratesClassDeclaration`
- `Convert_ClassWithNestedClassMultipleInstanceConstructors_GeneratesDispatcher`
- `Convert_ClassWithNestedClassConstructorOverloadsWithOptionalParameterOverlap_ThrowsNotSupportedException`
- `Convert_ClassWithNestedClassBaseConstructorInitializer_GeneratesSuperCall`
- `Convert_ClassWithNestedClassBaseMethodCall_GeneratesSuperInvocation`

## 延伸阅读

- [SemanticWalker.md](./semantic-walker/SemanticWalker.md)
- [WalkerArgument.md](./semantic-walker/WalkerArgument.md)
- [ESGenerator.md](./ESGenerator.md)
- [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
