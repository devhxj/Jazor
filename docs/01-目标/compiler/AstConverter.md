# `AstConverter`

## 定位

`AstConverter` 负责把一个带 `[ECMAScriptModule]` 约束的顶层类型转换成模块级 ESTree `Module`。

对应代码：

- `src/Jazor.Compiler/AstConverter.cs`

它位于整条链路中间：

```text
INamedTypeSymbol + SemanticModel
    -> AstConverter
    -> Acornima Module
    -> ESGenerator 收集为模块目录表
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

当前导出规则很直接：

- `public` / `internal` -> 生成 `ExportNamedDeclaration`
- 其他访问级别 -> 只生成本地声明，不导出

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

当前 `AstConverter` 支持把模块内部的非静态成员类和枚举转换出来：

- 成员类 -> `ClassDeclaration`
- 成员枚举 -> `const` + `Object.freeze(...)`

成员类内部又支持：

- 字段
- 属性（拆成 backing field + getter/setter）
- 构造函数
- 普通方法

这和旧文档里“不支持嵌套类扁平化”的说法也不同。当前实现不是扁平化，而是直接生成模块内类声明。

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

最终如何收集和暴露模块内容，由 `ESGenerator` 负责。

### 2. import 由 walker 收集、由 converter 提升

当前 import 处理路径是：

1. `SemanticWalker` 在方法 / 表达式转换中登记 import specifier
2. `SenseArgument.FlushImportSpecifiers()`
3. `AstConverter.MergeImports(...)`
4. `BuildImportDeclarations()` 生成模块级 `ImportDeclaration`

这说明 import 收集不是旧文档里说的“未实际使用”，当前已经接通。

### 3. 顶层属性不直接走 `IPropertySymbol`

在模块成员枚举里，`IPropertySymbol` 当前分支直接 `break`。

也就是说，顶层模块属性的可见行为依赖：

- backing field
- getter/setter method

而不是单独的“模块属性转换器”。

### 4. 成员类支持实例构造函数，但不支持静态构造函数

当前代码显式支持成员类实例构造函数：

- `ConvertMemberConstructor(...)`

但显式拒绝：

- 模块静态构造函数
- 成员类静态构造函数
- 带 constructor initializer 的成员类构造函数

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

### 枚举

当前模块内枚举会生成：

```js
const EnumName = Object.freeze({ ... });
```

必要时再包上 named export。

## 当前边界

这部分当前已经解决的是：

- 顶层模块类型转 `Module`
- 字段、方法、成员类、枚举
- import 提升
- 自动属性 getter/setter 生成
- `ref` / `out` 返回协议

它没有试图做这些事情：

- 支持非 `public` 顶层模块类型
- 支持顶层嵌套模块类型
- 支持模块静态构造函数
- 支持所有成员种类（例如事件、委托等）
- 直接产出最终 `.mjs` 文件

## 相关测试

主要测试在：

- `src/Jazor.CompilerTest/AstConverterTests.cs`

建议重点看这些场景：

- `Convert_SimplePublicClass_ReturnsModule`
- `Convert_NonPublicClass_ThrowsNotSupportedException`
- `Convert_ClassWithStaticField_GeneratesVariableDeclaration`
- `Convert_ClassWithMethod_GeneratesFunctionDeclaration`
- `Convert_ClassWithProperty_GeneratesPropertyMethods`
- `Convert_ClassWithNestedClass_GeneratesClassDeclaration`

## 推荐阅读

建议按这个顺序看：

1. [SemanticWalker.md](./SemanticWalker.md)
2. [AstConverter.md](./AstConverter.md)
3. [WalkerArgument.md](./WalkerArgument.md)
4. [ESGenerator.md](./ESGenerator.md)

## 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [WalkerArgument.md](./WalkerArgument.md)
- [ESGenerator.md](./ESGenerator.md)
- [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
