# `SemanticWalker` 与白名单消费

## 定位

这份文档说明 `SemanticWalker` 如何消费白名单规则，而不是说明白名单数据本身如何生成。

对应代码主要分布在：

- `src/Jazor.Compiler/core/SemanticWalker.cs`
- `src/Jazor.Compiler/core/SemanticWalker.cs.InlineTemplate.cs`
- `src/Jazor.Compiler/core/SemanticWalker.cs.WhiteList.cs`

如果只看角色分工：

- `WhiteList` 提供“有哪些宿主规则”
- `SemanticWalker` 负责“这些规则在具体语义节点上怎么落地”

## 当前职责

`SemanticWalker` 消费白名单主要是为了处理三类问题：

### 1. 成员名与类型名映射

最常见的是 `Alias`。

例如：

- CLR 风格名称 -> 真实 JS API 名称
- 运行时宿主类型名 -> 最终 JS host 名称

这类映射最终会影响：

- 方法名
- 属性名
- 初始化器成员名
- 类型宿主名

### 2. 内联模板实例化

某些成员不是简单改名，而是要展开成一段表达式模板。

当前 `SemanticWalker` 会把这些 `Inline` 规则实例化为 AST 表达式，再继续参与整体 lowering。

这里的关键点是：

- 现在不是“参数转字符串再整体拼接”
- 当前实现已经升级为“模板先解析成 AST，再做占位符替换”

也就是说，`Inline` 仍然是声明式模板，但内部消费方式已经不是早期那种纯字符串替换模型。

### 3. 导入式成员调用

当成员规则是 `Import` 时，`SemanticWalker` 会：

1. 把 import specifier 合并进上下文
2. 生成对应的调用或引用表达式

这让“导入式宿主 API”能和普通 runtime host 一样，在引用和调用路径里被统一消费。

## 当前入口

白名单消费入口主要有三个。

### 1. `GetWhiteListSymbol(...)`

这个 helper 用来把“语义引用节点”转成真正要查询的符号。

最重要的特例是属性：

- 读属性时优先查询 getter
- 写属性时优先查询 setter

原因很直接：

- 对 JS 而言，属性读取和写入可能对应完全不同的宿主规则
- 初始化器、赋值、普通属性读取不能机械共用同一个成员签名

### 2. `GetWhiteListExpression(...)`

这是成员级白名单消费的核心入口。

当前处理顺序大致是：

1. 用成员签名查询 `WhiteList.Members`
2. `Alias` 时返回别名信息
3. `Inline` 时实例化 AST 模板
4. `Import` 时登记导入并生成对应调用表达式

它有两个重载：

- 一个只接收参数列表
- 一个额外接收实例表达式

实例重载的作用是把实例成员统一转成“`instance + args`”的参数布局，这样白名单模板和导入调用不需要为实例/静态再拆一套入口。

### 3. 类型名查询路径

类型级白名单不走 `GetWhiteListExpression(...)`，而是通过：

- `GetTypeConfigOrWhiteListName(...)`
- `BuildFullTypeName(...)`
- `TryBuildRuntimeHostExpression(...)`

这些路径消费 `WhiteList.Types`。

所以“成员白名单消费”和“类型宿主白名单消费”在实现上是两条不同通路，但目标一致：都在尽量恢复真实 JS runtime shape。

## `Op` 在当前消费侧的含义

从 `SemanticWalker` 视角看，常见 `Op` 的意义如下。

### `Op.Alias`

表示“这个符号不按默认名字输出，而是换成另一个运行时名字”。

它最常见，也最基础。

覆盖面包括：

- 方法名
- 属性 getter/setter 对应成员名
- 类型宿主名

### `Op.Inline`

表示“这个符号不能只靠改名表达，需要展开成一段表达式模板”。

当前实现通过 `InstantiateInlineTemplate(...)` 完成实例化。

注意两点：

- 模板语法使用 `__arg1`、`__arg2` 这类占位符
- 旧的 `@#{0}` 占位方式已经被视为 legacy，并会直接拒绝

### `Op.Import`

表示“这个符号对应一个导入式实现”。

当前消费侧会：

- 使用 `entry.Path` 作为 import 来源
- 使用 `entry.Value` 作为导入后的局部标识符
- 通过 `context.MergeImportSpecifier(...)` 汇总依赖

### `Op.Compile`

当前编译器已经有生成器基础设施和 `Compile_*` 挂载点，但主消费链路仍不是以它为主。

也就是说：

- 基础设施在
- 生成表在
- 挂载方法在
- 但当前主线行为仍主要依赖 `Alias` / `Inline` / `Import`

所以它现在更接近“保留的复杂宿主扩展位”，而不是白名单主路径。

另外，`Op.Compile` 当前并不经过 `WhiteList.Members` 表，而是走独立的 `Compile_*` 分发表。

它的目标分发顺序、`handler/args` 契约、`null` / `throw` 语义，统一见：

- [OpCompileSpec.md](./OpCompileSpec.md)

### `Op.Allowed` / `Op.Discard`

这两类更多影响“是否允许进入编译域”或“是否需要特殊处理”，而不是在 `SemanticWalker` 里直接产出复杂 AST。

从消费侧看：

- `Allowed` 往往意味着继续走普通 lowering
- `Discard` 更多由 Analyzer 或不支持分支兜底

## 内联模板当前实现

这是这块文档里最容易过时的部分，所以单独写清楚。

### 现状

当前 `Inline` 消费流程是：

1. 模板文本先被标准化
2. 占位符改写为内部保留名
3. 模板解析成 AST，并缓存
4. 每次实例化时，用参数 AST 重写占位符节点

对应代码在：

- `SemanticWalker.cs.InlineTemplate.cs`

### 这意味着什么

意味着当前实现已经具备这些性质：

- 模板 AST 只需要解析一次
- 参数不是文本替换，而是 AST 级替换
- 能拒绝 legacy 占位符语法
- 不会把参数先序列化再重新 parse 回 AST

所以现在再把 `Inline` 描述成“字符串替换 + Parser 解析结果”已经不准确了。

### 为什么仍然保留 Parser

即便改成 AST 模板模式，模板本身依然需要先 parse。

原因没变：

- 白名单模板本质上仍然是声明式 JS 表达式
- 手写每一种 AST 模板构造成本太高
- Parser 仍然是最稳定的模板入口

所以准确表述应该是：

> 当前不是“每次实例化都做字符串拼接再 parse”，而是“模板预解析一次，实例化时做 AST 重写”。

## 与运行时宿主解析的关系

白名单消费和运行时静态宿主解析是相互配合的，不是互相替代的。

举例：

```csharp
Console.WriteLine("x");
```

最终变成：

```js
console.log("x");
```

这里至少有两步：

1. `WriteLine` -> `log`
   这一步主要靠白名单 / 名称映射
2. `Console` -> `console`
   这一步主要靠 `Reference` 语法域里的运行时宿主归一化

所以不能把所有 runtime API 对齐问题都归结成“白名单改名”。

相关文档：

- [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)
- [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md)

## 当前边界

这套白名单消费逻辑当前没有承诺这些事情：

- `Op.Compile` 已全面接管复杂宿主
- import 已完整落盘成最终 `ImportDeclaration`
- 所有宿主问题都只靠白名单解决

当前更准确的说法是：

- 白名单是主要的宿主映射事实来源
- `SemanticWalker` 是最主要的消费层
- 复杂运行时宿主选择仍需要 `Reference` 等语义域配合

## 推荐阅读

建议按这个顺序一起看：

1. [WhiteList.md](./WhiteList.md)
2. [SemanticWalker.WhiteList.md](./SemanticWalker.WhiteList.md)
3. [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)
4. [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md)
5. [InlineAstTemplateSpec.md](./InlineAstTemplateSpec.md)

## 相关文档

- [WhiteList.md](./WhiteList.md)
- [SemanticWalker.md](./SemanticWalker.md)
- [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)
- [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md)
- [InlineAstTemplateSpec.md](./InlineAstTemplateSpec.md)
- [OpCompileSpec.md](./OpCompileSpec.md)
