# `WhiteList`

## 与实现原则的关系

阅读本文件前，建议先看：

- [src/Jazor.Compiler/ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)

那份文档定义的是 compiler 的总路线、失败策略和宿主语义边界；本文件只讨论 `WhiteList` 作为“宿主映射事实层”本身的模型与消费方式。

如果二者出现张力，应按下面方式理解：

- `ImplementationPrinciples.md` 负责回答“什么样的宿主语义应该存在边界，为什么不能静默 fallback”；
- 本文件负责回答“已有宿主映射规则现在以什么数据结构存在、由哪些 lowering 路径消费”。

## 定位

`WhiteList` 是 Jazor 编译器消费宿主映射规则的静态数据中心。

它不负责具体 lowering，也不负责最终语法树拼接；它负责回答两个最基础的问题：

1. 某个类型有没有运行时映射规则
2. 某个成员有没有运行时映射规则

对应数据主要在：

- `WhiteList.Types`
- `WhiteList.Members`

相关生成产物和消费代码分布在：

- `src/Jazor.Compiler/WhiteList.cs.Generate.cs`
- `src/Jazor.Compiler/core/SemanticWalker.cs`
- `src/Jazor.Compiler/core/SemanticWalker.cs.Reference.cs`

## 它解决什么问题

Jazor 不是直接把所有 CLR 类型和成员原样发成 JavaScript。

很多 API 都需要一个“宿主映射事实层”，例如：

- `Console.WriteLine` 为什么会变成 `console.log`
- `System.Math` 为什么会落到 `Math`
- 某些 API 为什么不是简单改名，而是要展开成模板
- 某些成员为什么需要从模块导入，而不是从全局宿主访问

`WhiteList` 就是这层事实来源。

## 当前数据模型

从使用方式看，`WhiteList` 现在可以理解成两张表。

### 1. `Types`

键是类型签名，值是该类型对应的宿主规则。

它主要用于：

- 类型名改写
- 运行时宿主名恢复
- 宿主构造器映射

### 2. `Members`

键是成员签名，值是该成员对应的宿主规则。

它主要用于：

- 方法名改写
- 属性 getter / setter 映射
- 运算符方法映射
- inline 模板映射
- import 成员映射

## 当前 `Op` 语义

### `Op.Alias`

最常见。

表示：

- 类型或成员在最终 JS 里应使用另一个名字

典型用途：

- `WriteLine` -> `log`
- `Console` -> `console`

### `Op.Inline`

表示：

- 这个成员不能只靠改名表达
- 需要展开成表达式模板

当前模板已经不是纯字符串替换模型，而是“预解析 AST + 参数占位符重写”模型。

### `Op.Import`

表示：

- 这个符号的实现来自模块导入
- 不是普通全局宿主或普通成员访问

### `Op.Compile`

表示：

- 为更复杂的宿主语义预留编译器挂载点

当前基础设施已经存在，并且 `SemanticWalker` 主分发会先尝试它；但它仍只适合少数表达式级复杂宿主钩子，不是白名单消费的常规主路径。

另外，`Op.Compile` 条目不经过 `WhiteList.Members` 常规表，而是由生成器额外产出 `Compile_*` 接口和分发表。

它的主分发顺序、fallback 语义和与 `Inline` 的边界，见：

- [OpCompileSpec.md](./OpCompileSpec.md)

### `Op.Allowed`

表示：

- 允许按普通 lowering 路径继续处理

### `Op.Discard`

表示：

- 该符号不应按普通 ECMAScript 映射继续使用

## 生成来源

`WhiteList` 不是手写维护的完整规则表，它主要来自运行时映射侧的声明式标注。

整体链路大致是：

```text
ECMAScript.dll / Jazor.CLR.dll
    -> [Jazor(Op.*)] 标注
    -> 编译器生成器扫描
    -> WhiteList.cs.Generate.cs
    -> SemanticWalker / Analyzer / 其他消费侧读取
```

所以 `WhiteList` 的核心价值不是“把规则硬编码在编译器里”，而是把运行时映射声明集中投影成编译器可查询的数据表。

## 当前消费方

`WhiteList` 最主要的消费方是 `SemanticWalker`。

常见消费位置包括：

- 类型名恢复
- 成员别名映射
- inline 模板实例化
- import 成员调用
- 运行时宿主表达式构造

但它不是只服务于一个语法域。

当前会在这些路径里间接或直接参与：

- `Reference`
- `Creation`
- `Ordinary`
- `Pattern`

## 与 `SemanticWalker` 的关系

两者关系可以概括成一句话：

> `WhiteList` 提供映射事实，`SemanticWalker` 负责把这些事实落到具体 AST。

这也意味着两点：

### 1. 白名单不等于最终结果

同一个白名单规则，最终输出还会受到这些因素影响：

- 当前语义节点类型
- 当前实例/静态调用形态
- 当前宿主是否需要运行时归一化
- 当前是否在初始化器、模式匹配、条件访问等特殊上下文里

### 2. 运行时宿主问题不可能只靠白名单解决

例如：

```csharp
Console.WriteLine("x");
```

最终结果里的：

- `WriteLine -> log` 更多是白名单问题
- `Console -> console` 更多是运行时宿主归一化问题

也就是说，白名单给出“名字怎么映射”，但“最终挂到哪个宿主上”还要靠语义层决定。

## 当前边界

`WhiteList` 当前不是这些东西：

- 完整的 CLR 语义数据库
- 自动保证所有映射都闭环的验证器
- 最终 JavaScript 生成器
- 模块头 `import` 声明生成器

更准确地说，它现在是：

- 编译器宿主映射规则的静态事实源
- 由生成器维护的查询表
- 供 `SemanticWalker` 等组件消费的基础设施

## 推荐阅读

建议按这个顺序看：

1. [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
2. [WhiteList.md](./WhiteList.md)
3. [SemanticWalker.WhiteList.md](./semantic-walker/SemanticWalker.WhiteList.md)
4. [SemanticWalker.Reference.md](./semantic-walker/SemanticWalker.Reference.md)
5. [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md)

## 相关文档

- [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
- [SemanticWalker.WhiteList.md](./semantic-walker/SemanticWalker.WhiteList.md)
- [OpCompileSpec.md](./OpCompileSpec.md)
- [SemanticWalker.Reference.md](./semantic-walker/SemanticWalker.Reference.md)
- [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md)
