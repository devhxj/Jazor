# Jazor 语法转化逻辑总说明

## 目录

- [1. 文档定位](#1-文档定位)
- [2. 三类特性与各自职责](#2-三类特性与各自职责)
- [3. 总体转化流水线](#3-总体转化流水线)
- [4. 白名单生成与消费规则](#4-白名单生成与消费规则)
- [5. Analyzer 约束边界](#5-analyzer-约束边界)
- [6. `AstConverter` 的模块级转化规则](#6-astconverter-的模块级转化规则)
- [7. `SemanticWalker` 的语义级转化规则](#7-semanticwalker-的语义级转化规则)
- [8. AST 输出与序列化](#8-ast-输出与序列化)
- [9. 名称解析与符号稳定性](#9-名称解析与符号稳定性)
- [10. 错误处理策略](#10-错误处理策略)
- [11. 当前实现与设计稿不一致的地方](#11-当前实现与设计稿不一致的地方)
- [12. 面向未来语法的扩展协议](#12-面向未来语法的扩展协议)
- [13. 复核清单](#13-复核清单)

## 1. 文档定位

本文档用于给出 Jazor 当前代码库中一条可落地、可复核的语法转化链路说明，覆盖：

1. 特性标注与白名单生成
2. Analyzer 编译期约束
3. Source Generator 入口
4. `AstConverter` 模块级转换
5. `SemanticWalker` 语义级转换
6. 命名、白名单、错误处理与扩展规则

本文档以当前实现代码为准，并结合测试目录与既有文档交叉校验。凡是“设计上存在但实现尚未闭环”的能力，都会明确标注为“扩展点”，不作为既成事实描述。

相关阅读：

- [ArchitectureOverview.md](./ArchitectureOverview.md)：完整架构图与职责边界
- [ArchitectureOverview.Simplified.md](./ArchitectureOverview.Simplified.md)：一页版总览
- [README.md](./README.md)：文档总索引

## 2. 三类特性与各自职责

### 2.1 用户代码标记特性

- `[ECMAScriptModule]`：模块入口。`ESGenerator` 只从该特性出发寻找待转换类。
- `[ECMAScript]`：标记可在 ECMAScript 语义域中被 Analyzer 视为合法的类型。
- `[ECMAScriptName]`：指定编译期名称，优先级高于 `Description("@#...")`。
- `[ECMAScriptIgnore]`：语义上表示忽略成员，但当前编译器主链路未直接消费该特性。
- `[ECMAScriptInline]`：语义上表示直接提供 JavaScript 方法体，但当前编译器主链路未直接消费该特性。

### 2.2 白名单生成特性

- `[Jazor]` 定义在 `Jazor.Common`，服务于 `ECMAScript.dll` / `Jazor.CLR.dll` 里的运行库映射。
- 它不标记用户业务代码，而是标记“可被编译器识别的宿主类型和成员”。

`[Jazor]` 的三种常用入口：

- 无参：`Op.Compile`
- 单字符串：`Op.Inline`
- 三参数：显式指定 `Op`、成员签名、附加值

### 2.3 当前应如何理解这两套特性

- `[ECMAScriptModule]` / `[ECMAScript]` 决定“用户代码是否进入编译域”
- `[Jazor]` 决定“外部类型/成员是否可被调用，以及调用时如何改写”

两者职责不同，不应混用。

### 2.4 特性消费矩阵

| 特性 | 定位 | 当前主要消费者 | 当前状态 |
|------|------|----------------|----------|
| `[ECMAScriptModule]` | 模块入口 | `ESGenerator`、`Analyzer` | 已进入主链路 |
| `[ECMAScript]` | 允许类型 | `Analyzer` | 已进入主链路 |
| `[ECMAScriptName]` | 名称重写 | `Util.GetSymbolConfigName`、`AstConverter`、`SemanticWalker` | 已进入主链路 |
| `[ECMAScriptIgnore]` | 成员忽略 | 预留给编译器/运行库约定 | 未统一接入 |
| `[ECMAScriptInline]` | 用户自带 JS 代码 | 预留扩展点 | 未统一接入 |
| `[Jazor(Op.Alias)]` | 宿主名映射 | `WhiteList`、`SemanticWalker` | 已进入主链路 |
| `[Jazor(Op.Inline)]` | 简单模板表达式 | `WhiteList`、`SemanticWalker` | 已进入主链路，但有结构性风险 |
| `[Jazor(Op.Import)]` | 导入式宿主实现 | `SemanticWalker`、`SenseArgument`、`AstConverter` | 已进入主链路 |
| `[Jazor(Op.Compile)]` | 复杂宿主编译钩子 | `Jazor.Compiler.Generator`、`SemanticWalker` | 已接入主分发，当前 contract 仍限于表达式级钩子 |

## 3. 总体转化流水线

```text
ECMAScript / Jazor.CLR 运行库
    └─ [Jazor] 标注
          └─ Jazor.Compiler.Generator 扫描
                ├─ WhiteList.cs.Generate.cs
                ├─ WhiteList.cs.Compile.cs
                └─ SemanticWalker.cs.Generate.cs

用户 C# 代码
    └─ [ECMAScriptModule] / [ECMAScript] 标注
          └─ Jazor.Analyzer 校验类型与成员可用性
                └─ ESGenerator 找到模块类
                      └─ AstConverter 做模块级转换
                            └─ SemanticWalker 做 IOperation → ESTree 转换
                                  └─ Acornima AST
```

注意：当前 compiler 侧的稳定边界已经是“生成 AST / 文本 / SourceMap carriers 并写入 catalog”。真正 `.mjs` / `.mjs.map` 的文件物化由 `Jazor.Emit` 负责，不应再把“compiler 产 catalog”和“emit 写文件”混写成一个未定义阶段。

### 3.1 与 SourceMap 的关系

当前 sourcemap 基线已经落地，但它仍然依赖整条转化链路的稳定结构。

原因是 sourcemap 在本项目里不是单纯的“文本附属文件”，而是依赖整条转化链路的稳定结构：

```text
Roslyn IOperation
    -> SemanticWalker / AstConverter 给 AST 节点附着 SourceOrigin
    -> JavaScript writer 输出文本并构建 source map 内容
    -> ESGenerator 收集 catalog / module content / map carrier
    -> Jazor.Emit 写出 .mjs 与 .mjs.map
```

这意味着：

1. sourcemap 不能在 `SemanticWalker` 内直接拼 `mappings`
2. 也不能等到 emit 阶段再从最终 JS 文本反推
3. 它必须建立在稳定 lowering 结果之上

当前更准确的状态是：

- `SourceOrigin`、writer 侧 map 生成、emit 侧 `.map` 写出都已经进入主链路
- 后续重点不是“是否实现”，而是继续巩固 temp 名、import alias、synthetic 片段与主锚点的稳定契约

详见：

- [SourceMap.DecisionSummary.md](./sourcemap/SourceMap.DecisionSummary.md)
- [SourceMap.Design.md](./sourcemap/SourceMap.Design.md)
- [SourceMap.ImplementationChecklist.md](../../02-计划/compiler/SourceMap.ImplementationChecklist.md)

### 3.2 事实优先级

后续如出现“代码、测试、文档”不一致，建议固定采用以下优先级：

1. 当前源码实现
2. 当前测试断言
3. 新总说明文档
4. 旧版文档 / rule 文档 / README 中的历史表述

这样做的原因是：本仓库已有多份历史文档包含阶段性判断，不能直接视为现状。

## 4. 白名单生成与消费规则

### 4.1 生成阶段

`Jazor.Compiler.Generator` 会扫描 `ECMAScript.dll` 与 `Jazor.CLR.dll` 中的 `[Jazor]`：

- 类型级条目写入 `WhiteList.Types`
- 成员级条目写入 `WhiteList.Members`
- `Op.Compile` 额外生成：
  - `WhiteList.cs.Compile.cs` 接口声明
  - `SemanticWalker.cs.Generate.cs` 的字典装配代码

### 4.2 消费阶段

`SemanticWalker` 主要通过 `GetWhiteListExpression` 和 `GetMapperType` 消费白名单：

- `Alias`：改名
- `Inline`：按模板展开表达式
- `Import`：记录导入并生成调用
- `Compile`：`GetWhiteListExpressionCore(...)` 主入口会先尝试 `_whiteListCompiles`；只有返回 `null` 时才继续回落到 `Alias` / `Inline` / `Import`
- `Allowed`：允许原生转换
- `Discard`：通常由 Analyzer 或不支持分支兜底

### 4.3 现状与风险

- `Inline` 当前实现已经升级为“模板预解析 AST + 占位符重写 + 缓存复用”。
- 这解决了旧方案里“参数先转字符串再 parse”导致的结构不稳定问题。
- `Inline` 现在的主要风险不再是实现机制本身，而是边界误用：
  - 把需要控制求值顺序、临时变量或参数形状分支的语义继续塞进模板
  - 把应由 `Op.Compile` 接管的逻辑继续停留在声明式模板层
- `Op.Compile` 的分发基础设施已经接入主入口；当前真正的限制不在“是否接线”，而在 `Compile(handler, args)` contract 仍偏窄，天然更适合表达式级钩子，而不是完整 lowering 子系统。
- 已选定的演进策略是：
  1. 保留 `Inline` 的字符串声明方式
  2. 内部持续使用“模板 AST + 占位符替换”
  3. 复杂宿主语义继续落到 `Op.Compile`

具体方案见：

- [InlineAstTemplateSpec.md](./InlineAstTemplateSpec.md)
- [OpCompileSpec.md](./OpCompileSpec.md)

## 5. Analyzer 约束边界

`Jazor.Analyzer` 只对带 ES 特性的类及其上下文做检查，核心目标是“在进入转换器前尽量拒绝非法语义”。

当前主要规则：

- ES 特性只能落在最外层类型；嵌套 ES 类型直接报错
- 白名单中的类型和成员允许使用
- 其他类型必须是带 `[ECMAScript]` / `[ECMAScriptModule]` 的类型
- 事件、事件赋值、析构等会直接报错
- 数组、元组、泛型参数会递归检查元素/类型参数

Analyzer 的职责不是生成代码，而是收紧输入域。凡是 Analyzer 放行的代码，转换器仍可能因为“能力未实现”而失败，但不应出现大面积无约束输入。

## 6. `AstConverter` 的模块级转化规则

`AstConverter` 负责把一个模块类拆成 ES module 顶层声明。当前实现的事实规则如下：

### 6.1 输入前提

- 当前只接受 `public` 顶层类
- 嵌套模块类直接拒绝
- 实现上没有强制要求必须是 `static class`，但测试和设计都以“模块类 = public static class”为主

### 6.2 成员映射

- 静态字段 -> `let` / `const`
- 静态属性 -> `get_*/set_*` 函数
- 静态方法 -> `function`
- 成员类 -> `class`
- 成员 `record` -> 不发射 runtime declaration，使用点统一走 structural lowering
- 枚举 -> “声明擦除 + 使用点常量化”；若旧文档或历史实现仍出现 `Object.freeze({...})`，应视为已废弃的过渡态

当前 `AstConverter` 的处理粒度是“类成员声明”，而不是方法体内部语义；方法体、表达式体、字段初始化器一旦超出简单字面量，就会下沉给 `SemanticWalker`。

### 6.3 导出规则

- `public`、`internal` 视为模块公开成员
- 其他访问级别视为私有，不导出
- 只支持 named export；若成员最终导出名解析为 `default`，必须显式失败

### 6.4 当前边界与扩展点

- 顶层模块类型当前仍要求 `public` 且为顶层类型
- 方法统一生成为 `async: false`、`generator: false`，与 `SemanticWalker` 某些函数级推断还未对齐
- 单个 `SemanticModel` 被复用于所有 `DeclaringSyntaxReferences`，跨文件/partial 声明存在脆弱性

## 7. `SemanticWalker` 的语义级转化规则

`SemanticWalker` 是整个系统的核心。它继承 `OperationVisitor<SenseArgument, Node?>`，把 Roslyn `IOperation` 转成 Acornima ESTree。

### 7.1 设计原则

- 优先使用 `IOperation`，而不是直接基于语法节点拼接
- 通过 `SenseArgument` 显式传递上下文，不依赖 `operation.Parent`
- 尽量直接构造 AST；语法节点回退只作为局部补丁能力
- 不支持语义不等价的特性时，优先显式失败，而不是静默降级

### 7.2 `Sense` / `SenseArgument`

`Sense` 表示“当前转换场景”，例如：

- 左值 / 右值
- 函数体 / 嵌套块 / catch
- 模式匹配输入
- 属性读写
- 对象初始化器 / 集合初始化器
- out 参数 / 默认值 / 丢弃赋值

`SenseArgument` 还承担两类依赖收集：

- 变量声明提升：`AddVarDeclarator` / `FlushVarDeclarator`
- 导入规范收集：`MergeImportSpecifier`

其中变量声明提升已经在多个 block/try/catch/function 路径中实际使用；导入收集也已经通过 `AstConverter` 提升为模块级 `ImportDeclaration`。

### 7.3 语法域划分

测试目录已经把 `SemanticWalker` 的主要语法域拆成稳定模块：

- `Pattern`：模式匹配
- `Loop`：`for` / `foreach` / `while`
- `Switch`：传统 `switch` 与模式 `switch`
- `String`：插值字符串
- `TryCatch`：异常
- `Declaration`：变量、参数、局部函数
- `Ordinary`：普通表达式与赋值
- `Reference`：字段、属性、方法、索引
- `Creation`：对象、数组、匿名对象、初始化器
- `Tuple`：元组、比较、解构
- `NotSupport`：明确拒绝的 `IOperation`

文档、代码、测试三者在目录结构上是一一对应的，这也是未来扩展新语法时应保持的组织方式。

### 7.3.1 `Reference` 语法域里的运行时宿主修正

`Reference` 不只是“把字段/属性/方法访问翻译成成员表达式”。

它还承担一类很关键的运行时对齐工作：

- 当 C# 声明宿主和 JS 真实宿主不一致时，修正最终宿主
- 当静态成员声明在基类上、但运行时应挂到具体子类型上时，保留具体宿主
- 当调用点使用 `using` 类型别名时，避免把别名名词泄漏到最终 JS

例如：

- `Console.WriteLine` -> `console.log`
- `Bytes.Of(...)` -> `Uint8Array.of(...)`
- `Uint8Array.BYTES_PER_ELEMENT` 不应退回成 `TypedArray.BYTES_PER_ELEMENT`

这部分不是单纯的白名单改名，而是“运行时静态宿主解析”问题，当前由
`SemanticWalker.cs.Reference.cs` 内部一组 helper 统一处理。

详见：

- [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md)

### 7.4 语法域决策矩阵

新增一个 C# 语法点时，应先判断它属于哪一层问题：

| 问题类型 | 应落位置 | 典型例子 | 说明 |
|----------|----------|----------|------|
| 模块成员拆解 | `AstConverter` | 静态字段、模块级属性、嵌套类 | 影响 module 顶层结构 |
| 语义表达式/语句转换 | `SemanticWalker` | 模式匹配、解构、条件访问 | 影响 `IOperation -> ESTree` |
| 宿主 API 改写 | `WhiteList` + `SemanticWalker.Reference` | `Console.WriteLine -> console.log` | 白名单负责成员映射，引用域负责最终宿主修正 |
| 复杂宿主语义 | `Op.Compile` | 需要保留 AST 结构的内建 API | 不适合字符串模板 |
| 输入合法性收紧 | `Analyzer` | 事件、非法外部类型、嵌套 ES 类型 | 应尽早失败 |
| 后处理优化 | `Optimizer` | 常量折叠、死代码、表达式规范化 | 不能改变语义 |

判断原则：

- 只要问题依赖 `IOperation.Kind` 或上下文语义，优先归 `SemanticWalker`
- 只要问题依赖“成员是字段/方法/类/枚举”，优先归 `AstConverter`
- 只要问题依赖“某个外部 API 应如何映射”，优先归 `WhiteList`
- 只要问题依赖“模板字符串会损坏 AST 结构”，优先升级到 `Op.Compile`

## 8. AST 输出与序列化

当前仓库里实际存在三层“AST 之后”的路径：

- 测试路径：直接调用 `ToKnRECMAScript()` / `ToECMAScript()`，把 Acornima AST 序列化成 JavaScript 文本
- compiler/catalog 路径：`ESGenerator` 把 `AstConverter` 的真实结果、模块文本和 SourceMap carriers 收集到 `ModuleCatalog` 生成物中
- emit/materialization 路径：`Jazor.Emit` 读取 catalog，写出 `.mjs` / `.mjs.map` 与相关 manifest

另外，`Optimizer.cs` 已存在，但当前主链路没有发现统一接入点。因此应理解为：

- “C# -> ESTree” 是当前已经实现的主能力
- “Optimizer 统一接入主生成链路” 仍未被建立为稳定契约

### 8.1 输出层的持续稳定重点

后续更稳妥的重点应固定为：

1. `AstConverter` / `SemanticWalker` 继续稳定 AST 与 `SourceOrigin`
2. writer 继续保证文本输出与 source map 内容的确定性
3. `Jazor.Emit` 继续保持 catalog -> 文件物化的稳定契约
4. 若未来接入 `Optimizer`，仍只允许做语义保守优化

不建议跳过 AST 层，直接在 `ESGenerator` 或 emit 中重新拼字符串。

## 9. 名称解析与符号稳定性

符号到 JavaScript 名称的解析顺序由 `Util.GetConfigOrSymbolName` 决定：

1. `ECMAScriptNameAttribute`
2. `Description("@#name")`
3. 隐式 backing field 哈希名
4. 重载方法哈希后缀
5. 原始符号名

这里要单独区分两类重载：

- 普通方法重载仍然走 `GetConfigOrSymbolName(...)` 路线，只在确有同名方法重载时追加稳定签名 hash
- 成员类构造函数重载不走“多个 JS 名字”分裂，因为 JS class 只能有一个真实 `constructor`；它必须走 `$ctor_<hash>` helper + `arguments.length` dispatcher

换言之，“重载需要稳定区分”不等于“所有重载都靠最终输出名字区分”。

配套约束：

- 白名单匹配统一使用 `Format.NameFormat` 生成稳定签名
- 隐式字段不能直接使用 Roslyn 生成名，必须转为稳定哈希
- ECMAScript runtime host 上的方法默认跳过普通方法重载后缀，避免把宿主 API 意外拆成 CLR 风格 overload surface
- 任何新语法若引入新的“合成符号”，都必须先定义稳定命名规则，再扩展测试

## 10. 错误处理策略

当前系统有三层错误边界：

### 10.1 Analyzer 错误

非法类型或成员在编译期直接报 `JAZOR001`。

### 10.2 转换器错误

- `AstConverter` 对类级不满足约束的输入直接抛 `NotSupportedException`
- `SemanticWalker` 通过 `HandleTransformationFailure` 抛 `OperationTransformationException` 或 `SyntaxNodeTransformationException`

### 10.3 设计取舍

- 对“JS 无法保持语义等价”的语法，优先拒绝
- 对“暂未实现但未来可实现”的语法，保留独立 `Visit*` 或分文件扩展位
- 不鼓励“吞掉错误然后继续输出近似代码”

这套策略对“健壮”和“鲁棒”的要求，是“可预期失败”优先于“默默输出错误结果”。

## 11. 当前实现与设计稿不一致的地方

这部分必须单列，因为仓库中旧文档和当前实现存在偏差。

- `ECMAScriptInlineAttribute` 当前未进入主转换链路
- `ECMAScriptIgnoreAttribute` 当前未见统一消费入口
- 旧文档曾把 `Op.Compile` 写成“未接入主分发”；当前源码已经优先尝试 `_whiteListCompiles`，但 contract 仍偏窄
- 旧文档曾把 sourcemap 写成“明确延后”；当前 baseline 已落地，后续问题是稳定性而不是有没有
- `Optimizer` 已存在，但未见稳定接入主生成链路
- 一些文档仍宣称测试“全部通过”或某模块“完整”，与当前测试状态不一致

因此，后续所有设计评审应优先以源码和测试为准，再回写文档。

### 11.1 应如何处理这些不一致

- 若源码已实现、测试未更新：先补测试，再修正文档
- 若文档已定义、源码未实现：文档必须改成“扩展点”而不是“现状”
- 若测试通过但语义有争议：以源码实际契约和白名单设计为准，必要时重写断言
- 若三者都不一致：先确认 Analyzer 是否允许该输入进入转换域，再决定主责任位置

## 12. 面向未来语法的扩展协议

新增语法支持时，建议固定遵循下面的顺序，避免再次出现“文档先行、实现未闭环”的情况。

### 12.1 第一步：定义输入边界

- 是否允许 Analyzer 放行
- 是否需要新增白名单条目
- 是否需要新的 `Sense` 场景

### 12.2 第二步：选择实现层级

- 类成员级变化 -> `AstConverter`
- 方法体/表达式级变化 -> `SemanticWalker`
- 外部宿主 API 映射 -> `WhiteList`
- 复杂宿主映射 -> `Op.Compile`

### 12.3 第三步：先建测试矩阵

至少覆盖：

- happy path
- 嵌套/组合语法
- 命名与作用域
- 常量与默认值
- 错误路径
- 与已有白名单/导入/tuple/pattern 交互

建议同时覆盖三类测试：

- `AstConverterTests`：成员级拆解是否正确
- 对应 `SemanticWalker*Test`：方法体内部语义是否正确
- 失败路径测试：是否抛出预期异常，而非静默输出错误 AST

### 12.4 第四步：保持三件事同步

- 源码
- 对应测试文件
- 对应文档文件

新增一个大语法域时，推荐同步新增：

- `SemanticWalker.cs.<Feature>.cs`
- `SemanticWalker<Feature>Test.cs`
- `semantic-walker/SemanticWalker.<Feature>.md`

## 13. 复核清单

在接受任何语法转换改动前，至少做以下复核：

1. 代码复核：确认入口特性、Analyzer、Converter、Walker 四层是否一致
2. 测试复核：确认新增语法在 `AstConverterTests` 或对应 `SemanticWalker*Test` 中有断言
3. 文档复核：确认文档没有把“计划”写成“现状”
4. 鲁棒性复核：确认失败路径是显式失败，不是静默返回错误 AST
5. 未来兼容复核：确认扩展点优先使用 `SenseArgument`、`Op.Compile`、分文件拆分，而不是在单点堆积特判
6. 宿主映射复核：确认新语法若依赖外部 API，已经明确落在 `Alias`、`Inline`、`Import` 或 `Compile` 中的一种
7. 闭环复核：确认变更是否需要同步 `ESGenerator`、导入输出或 `Optimizer`，避免只修到 AST 层就宣称完成

---

**校核基线**

- 源码：`Jazor.Common`、`Jazor.Analyzer`、`Jazor.Compiler`、`Jazor.Compiler.Generator`
- 测试：`AstConverterTests.cs` 与全部 `SemanticWalker*Test.cs`
- 既有文档：`src/Jazor.CLR/doc/*`、`docs/01-目标/compiler/*`、`src/Jazor.Compiler/ImplementationPrinciples.md`

**结论**

Jazor 当前真正可靠的“语法转化主线”是：`[Jazor]` 生成白名单，`[ECMAScriptModule]` 进入编译域，Analyzer 缩小输入集合，`AstConverter` 做模块拆解，`SemanticWalker` 做语义下沉，writer / `ESGenerator` 产出 catalog 与 map carriers，`Jazor.Emit` 负责文件物化。未来要提升健壮性，重点不是继续堆 `Inline` 模板，而是稳住 `Op.Compile` contract、compiler/emit 边界，以及继续保持测试/文档同步。
