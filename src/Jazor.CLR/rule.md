# Jazor.CLR 开发规则文档

本文档整合了 Jazor 编译器的 BCL 模块映射规则和白名单规则。

> **Jazor.CLR 的作用**：被 Jazor 编译成 CLR module，供编译时引用。
> **白名单的作用**：为 Jazor 编译器提供允许使用的类型和成员清单，确保编译时类型安全。
> **白名单生成**：用 Jazor.Compiler.Generator 控制台读取 `ECMAScript.dll` 和 `Jazor.CLR.dll`，根据 `[Jazor]` 特性生成。

---

## 目录

1. [术语定义](#1-术语定义)
2. [特性体系说明](#2-特性体系说明)
3. [Op 枚举说明](#3-op-枚举说明)
4. [模块声明规范](#4-模块声明规范)
5. [白名单工作流程](#5-白名单工作流程)
6. [成员签名格式](#6-成员签名格式)
7. [方法哈希命名](#7-方法哈希命名)
8. [类型映射表](#8-类型映射表)
9. [out/ref 参数处理](#9-outref-参数处理)
10. [端到端示例](#10-端到端示例)
11. [错误处理与调试](#11-错误处理与调试)
12. [快速参考](#12-快速参考)
13. [附录](#13-附录)

---

## 1. 术语定义

| 术语 | 说明 |
|------|------|
| **Jazor.CLR** | CLR 模块库，包含 BCL 类型的 JavaScript 实现 |
| **ECMAScript** | 生产核心库，提供 JavaScript 基础类型映射定义 |
| **白名单** | 允许使用的类型和成员清单，用于编译时验证 |
| **CLR Module** | 由 Jazor.CLR 编译生成的 JavaScript 模块 |
| **Op** | 操作类型枚举，定义成员的处理方式 |
| **BCL** | Base Class Library，.NET 基础类库 |
| **ESTree** | ECMAScript AST 标准 |
| **TypeMapper** | Jazor 内部类型映射枚举 |

---

## 2. 特性体系说明

### 2.1 [Jazor] 特性

用于白名单生成和模块导入，标记类型和成员的映射关系。

**构造方法**：

```csharp
// 无参：Op = Op.Compile（编译器特殊处理）
[Jazor]

// 单字符串：Op = Op.Inline（内联代码）
[Jazor("内联代码模板")]

// 三参数：完整指定
[Jazor(Op op, string member, string? value = null)]
```

**参数说明**：

- `op`：操作类型（见 [Op 枚举说明](#3-op-枚举说明)）
- `member`：完整的 C# 成员签名，用于白名单生成和哈希计算
- `value`：
  - `Op.Alias`：JavaScript 方法名（如 `toString`）
  - `Op.Inline`：内联代码模板（如 `(__arg1 === __arg2)`）
  - 其他 Op 类型：通常不需要此参数

**使用说明**：

- `无参`和`单字符串`这 2 个构造函数主要给 `ECMAScript` 生产核心库使用
- `Jazor.CLR` 中默认应使用完整三参数形式，把 `member` 明确写出来
- `[Jazor]` 无参 = `Op.Compile`，不是“待定”或“以后补实现”的占位
- `[Jazor("...")]` = `Op.Inline`，字符串必须是稳定的表达式模板，不是任意 JS 代码片段
- generated 白名单必须通过 `Jazor.Compiler.Generator` 刷新；不要手改 generated 文件

### 2.2 [ECMAScript] 系列特性（用户代码标记）

| 特性 | 用途 |
|------|------|
| `[ECMAScriptModule]` | 标记类生成 ES module |
| `[ECMAScript]` | 标记可被编译器识别的资源型类型 |
| `[ECMAScriptIgnore]` | 标记被编译器忽略的成员 |
| `[ECMAScriptInline]` | 标记方法直接使用内联代码 |
| `[ECMAScriptName]` | 标记编译时别称（优先级 > [Description]） |

**编译器处理规则**：只处理 `[ECMAScriptModule]` 标记的静态类中有方法体的方法（非 `extern`），或带 `[ECMAScriptInline]` 的 extern 方法。

---

## 3. Op 枚举说明

### 3.1 Op 类型概览

| Op 类型 | extern? | 编译到 CLR module? | 白名单行为 | 说明 |
|---------|---------|-------------------|-----------|------|
| `Discard` | ✅ | ❌ | 记录但标记为不支持 | JavaScript 无对应概念 |
| `Allowed` | ✅ | ❌ | 记录为允许 | JavaScript 原生支持，无需处理 |
| `Alias` | ✅ | ❌ | 记录为允许 | JS 有类似方法但名称不同 |
| `Inline` | ✅ | ❌ | 记录为允许 | 简单表达式可直接内联 |
| `Import` | ❌ | ✅ | 记录为允许 | 需要完整 JavaScript 实现 |
| `Compile` | ✅ | ❌ | 记录为允许 | 编译器特殊处理 |

**核心原则**：
- `extern` = 方法不需要 C# 实现，只在白名单中标记
- **只有 `Op.Import` 方法会被编译到 CLR module**（因为有方法体）
- 其他 Op 类型的 `extern` 方法仅用于白名单注册，不生成 JavaScript 代码
- producer 侧默认选择顺序是：`Allowed/Alias -> Inline -> Import -> Compile`
- consumer 侧 `SemanticWalker` 分发顺序是：`Compile -> Alias -> Inline -> Import -> normal lowering`
- 两个顺序同时成立，不冲突；前者解决“声明端该怎么选”，后者解决“消费端命中后谁优先”

**最容易混淆的点**：

- `Compile` 在 consumer 侧排第一，不代表 producer 侧应该优先选 `Compile`
- `Import` 不是“Inline 写起来麻烦时的兜底”，而是“确实需要运行时实现”时才选
- `Inline` 不是“只要最后 JS 看起来是一行就行”，而是“结构稳定、不会把异常协议和求值顺序藏坏”
- `Allowed` 不是“JS 里差不多有类似概念”就能选，而是“默认 lowering 不需要再插手”才能选

**速记决策**：

- 只改名：`Alias`
- 只改成稳定单表达式：`Inline`
- 需要模块代码承接语义：`Import`
- 必须由编译器内部直接产 AST：`Compile`
- 当前明确不支持：`Discard`

### 3.1.1 五问决策法

新增成员时，按这 5 个问题从上往下问：

1. 默认 lowering 已经正确了吗？
   - 是：`Allowed`
2. 只是名字不对，结构和语义都对吗？
   - 是：`Alias`
3. 能稳定写成单个 expression，并且不会把异常协议、求值顺序、tuple 形状写坏吗？
   - 是：`Inline`
4. 这个语义更适合作为模块 helper 承接吗？
   - 是：`Import`
5. 前面都不合适，而且必须由编译器内部直接接管吗？
   - 是：`Compile`

如果 5 个问题都答不稳，先不要硬标；先把语义边界写清楚再定。

### 3.1.2 每个 Op 的入选条件

| Op | 什么时候选 | 什么时候不要选 |
|----|------------|----------------|
| `Allowed` | 默认 lowering 已经正确 | 还需要改名、改模板或补异常协议 |
| `Alias` | 只需要换宿主名/成员名 | 名字之外还要改参数、结构或返回形态 |
| `Inline` | 稳定单表达式、无 import、无 temp、无 throw 分支协议 | 需要临时变量、复杂副作用顺序、异常协议、tuple 形状 |
| `Import` | 需要模块代码、helper、循环、校验、异常、格式化、解析 | 其实只是稳定模板或简单改名 |
| `Compile` | 编译器内部特例，且当前 contract 足以直接产表达式 AST | 能更清楚地放进 Inline / Import，或需要 temp/import/source map |
| `Discard` | 当前明确不支持或 JS 无对等概念 | 只是暂时没写实现；这种情况不要用 Discard 糊掉 |

### 3.1.3 禁用清单

下面这些判断方式是错的：

- “先标 `Compile`，以后再补”
- “Inline 写着费劲，先改成 Import”
- “最终 JS 只有一行，所以一定是 Inline”
- “consumer 先试 Compile，所以 producer 也优先选 Compile”
- “暂时没时间实现，就先 Discard”

更稳的做法是：

- 不确定是否该 `Inline` 时，先看它是否带异常协议或重复求值风险
- 不确定是否该 `Compile` 时，先问它能不能更清楚地落到模块 helper
- 不确定是否该 `Discard` 时，先确认是不是“当前明确不支持”，而不是“还没做”

### 3.2 Op.Discard - 不支持

JavaScript 无对应概念（如 `GetHashCode`, `Console.ReadLine`）。**不编译到 CLR module**，仅白名单标记。

```csharp
// JavaScript 无哈希码机制
[Jazor(Op.Discard, "override object.GetHashCode()")]
public extern static Number _hash(object instance);

// 来自 doc/BooleanModule.md
// 成员：override bool.GetHashCode()
// 签名：_80b6c29cc0038969
[Jazor(Op.Discard, "override bool.GetHashCode()")]
public extern static Number _80b6c29cc0038969(bool instance);

// 带 IFormatProvider 的重载（通常不支持）
[Jazor(Op.Discard, "bool.ToString(System.IFormatProvider)")]
public extern static string _hash(bool instance, Intl.NumberFormat? provider);
```

### 3.3 Op.Allowed - 无操作

JavaScript 原生支持，默认行为正确（如默认构造函数、运算符）。**不编译到 CLR module**，编译器直接使用 JS 原生行为。

**入选条件**：

- 默认 lowering 已经正确
- 不需要改名
- 不需要补模板
- 不需要补运行时协议

**不要选 Allowed 的情况**：

- 名字不对但 JS 有原生成员
- 参数/返回形态还要调整
- 默认 lowering 会丢 C# 语义

```csharp
// 布尔默认构造（JS 布尔是原始类型）
[Jazor(Op.Allowed, "bool.Boolean()")]
public extern static bool _hash();

// 来自 doc/BooleanModule.md
// 成员：bool.Boolean()
// 签名：_2bd9618624257446
[Jazor(Op.Allowed, "bool.Boolean()")]
public extern static bool _2bd9618624257446();
```

### 3.4 Op.Alias - 方法名替换

JS 有原生方法但名称不同。**不编译到 CLR module**，编译器直接替换方法名。

**入选条件**：

- JS 原生能力已存在
- 只差宿主名或成员名
- 不需要额外模板和 helper

**不要选 Alias 的情况**：

- 需要改参数结构
- 需要异常/边界检查
- 需要从属性访问改成方法调用，或反过来，再加额外逻辑

```csharp
[Jazor(Op.Alias, "override bool.ToString()", "toString")]
public extern static string _hash(bool instance);
// 编译时直接生成：instance.toString()

// 来自 doc/BooleanModule.md
// 成员：override bool.ToString()
// 签名：_d48c2d39317daf8f
[Jazor(Op.Alias, "override bool.ToString()", "toString")]
public extern static string _d48c2d39317daf8f(bool instance);
```

### 3.5 Op.Inline - 内联代码

用占位符模板生成 JavaScript 表达式。**不编译到 CLR module**，编译器直接内联代码。

**占位符规则**：

| 方法类型 | __arg1 | __arg2 | __arg3 |
|----------|-------|-------|-------|
| 实例方法 | 实例 | 参数1 | 参数2 |
| 静态方法 | 参数1 | 参数2 | 参数3 |
| 扩展方法 | 被扩展对象 | 参数1 | 参数2 |

**选择边界**：

- 只要能稳定表达成单个 expression，就优先 Inline
- 但如果模板开始依赖 throw 分支、临时变量、重复求值规避、tuple 运行时对象形状，就不要继续硬塞 Inline
- Inline 解决的是“模板稳定”，不是“看起来代码短”

**典型适合 Inline**：

- 常量字面量
- 简单算术/比较模板
- 纯宿主调用改写，如 `substring(start, start + len)`

**典型不要继续 Inline**：

- `list[i]`、`dict[key]` 这类带异常语义的索引器
- 需要把参数缓存一次再比较的逻辑
- 需要手写 tuple 结果对象布局的逻辑

```csharp
// Equals → 严格相等
[Jazor(Op.Inline, "override bool.Equals(object)", "(__arg1 === __arg2)")]
public extern static bool _hash(bool instance, object? obj);

// 静态字段 → 字面量
[Jazor(Op.Inline, "static readonly bool.TrueString", "true")]
public extern static bool _hash();
// 编译时直接内联：true

// 来自 doc/BooleanModule.md
// 成员：static readonly bool.TrueString
// 签名：_49c57acefc093fcc
[Jazor(Op.Inline, "static readonly bool.TrueString", "true")]
public extern static bool _49c57acefc093fcc();
```

### 3.6 Op.Import - 模块导入

需要完整 JavaScript 实现。**会编译到 CLR module**，提供 C# 方法体。

但要先记住一条优先级规则：

> 能稳定用 `Inline` 表达的，就不要用 `Import`。

`Import` 不是“写模板麻烦时的兜底方案”，而是：

- 确实需要运行时实现
- 确实需要多步逻辑
- 确实需要模块级 helper

之后才使用。

**入选条件**：

- 需要真实方法体
- 需要循环、多步逻辑或 helper
- 需要异常消息、边界检查、解析/格式化协议
- 需要 out/ref 返回包

**不要选 Import 的情况**：

- 只是简单改名
- 只是稳定单表达式模板
- 只是想绕开 Inline/Compile 约束，不是真有运行时需求

当前仓库内已经做过一轮具体复审，建议直接参考 [Inline / Import 复审记录](InlineImportAudit.md) 里的迁移优先级，不要重复把适合 `Inline` 的成员继续实现成 `Import`。

```csharp
[Jazor(Op.Import, "static bool.Parse(string)")]
public static bool _hash(string? value)
{
    var str = value?.Trim()?.ToLower();
    if (str == "true") return true;
    if (str == "false") return false;
    throw new Error($"FormatException: String '{value}' was not recognized as a valid Boolean.");
}

// 来自 doc/BooleanModule.md
// 成员：static bool.Parse(string)
// 签名：_5dbf54319ebc8dfe
[Jazor(Op.Import, "static bool.Parse(string)")]
public static bool _5dbf54319ebc8dfe(string? value)
{
    var str = value?.Trim()?.ToLower();
    if (str == "true") return true;
    if (str == "false") return false;
    throw new Error($"FormatException: String '{value}' was not recognized as a valid Boolean.");
}
```

**注意**：
- `Op.Import` 必须使用 JavaScript 原生实现，尽量调用映射后的方法（避免调用 C#原生方法以防止编译时循环调用）
- `Op.Import` 实现的方法体必须健壮，不能简写
- C#没有`===`，使用`object.Equal`实现`===`，也可以用object.Is
- `ECMAScript.Global`映射的js的`GlobalThis`对象
- 避免调用 C#原生方法如`int.Parse`，强转如`(int)a`，而是使用js映射方法如`ParseInt`和`Number`类型
- 如果同样结果可以稳定写成 `Inline`，优先回到 `Inline`

### 3.7 Op.Compile - 编译器特殊处理

编译器有内置特殊逻辑，如常量内联、特殊类型转换。**不编译到 CLR module**，由编译器内部处理。

| 特性 | Op.Inline | Op.Compile |
|------|-----------|------------|
| 处理时机 | 代码生成阶段 | 编译器内部处理 |
| 占位符 | 使用 `__argN` | 由编译器决定 |
| 适用场景 | 简单表达式替换 | 复杂逻辑、编译器内置处理 |

当前 producer 侧要额外注意两点：

1. `Op.Compile` 虽然已经进入 `SemanticWalker` 主分发主线，但它仍然是编译器内部保留能力，不是常规 producer 选项。
2. 当前 `Compile` hook 形态本质上更接近“表达式级特殊钩子”，不要把需要临时变量、import 或语句级展开的语义直接挂到这里。

也就是说，现阶段更准确的选择规则是：

- 能稳定写成 `Inline`：优先 `Inline`
- 不能稳定 `Inline`，但更适合作为运行时 helper：优先 `Import`
- 只有既不适合 `Inline`，也不适合 `Import`，并且必须由编译器直接接管时，才考虑 `Compile`
- `Compile` 当前仍只适合自包含表达式级改写；凡是要临时变量、import、语句级 throw 协议、source map 来源跟踪的，先不要挂

**当前适合 Compile 的典型特征**：

- 不需要模块 helper
- 不需要声明提升
- 不需要 import
- 返回值仍然是单个 AST 表达式
- 语义属于编译器内部保留特例，而不是普通 BCL runtime 映射

**当前不要挂 Compile 的典型特征**：

- 需要 `throw` 作为表达式分支约定
- 需要稳定临时变量名
- 需要 source map / source-origin 追踪
- 需要 tuple 运行时形状拼装
- 其实作为模块 helper 更清晰

**一句话判断**：

- “必须由编译器直接产 AST 才合理”才考虑 `Compile`
- “模块 helper 也能清楚表达”就先不要上 `Compile`

```csharp
// 来自 doc/BooleanModule.md
// 成员：bool.GetTypeCode()
// 签名：_eb6a23c2a874fdf1
[Jazor(Op.Compile, "bool.GetTypeCode()")]
public extern static System.TypeCode _eb6a23c2a874fdf1(bool instance);
```

编译器消费约定与实施顺序，见：

- `src/Jazor.Compiler/doc/OpCompileSpec.md`
- `src/Jazor.Compiler/doc/OpCompileImplementationChecklist.md`

### 3.8 Op 类型选择决策

#### 决策流程图

```
JS 有原生对应？
├── 是，名称相同 → Allowed
├── 是，名称不同 → Alias
└── 否 → JS 有概念？
    ├── 否 → Discard
    ├── 能稳定写成单表达式 → Inline
    ├── 需要运行时实现/模块逻辑/校验/异常协议 → Import
    └── 既不适合 Inline，也不适合 Import，且必须由编译器直接接管 → Compile
```

#### 详细决策表

| 场景 | Op 类型 | 示例 | 判断依据 |
|------|---------|------|----------|
| 默认构造函数 | `Allowed` | `bool.Boolean()`, `int.Int32()` | JS 原始类型无需构造 |
| 类型转换运算符 | `Allowed` | 隐式/显式转换 | 编译器直接处理 |
| JS 原生方法，同名 | `Allowed` | 运算符重载 | 无需额外代码 |
| JS 原生方法，不同名 | `Alias` | `ToString()` → `toString()` | 只需改方法名 |
| JS 原生属性，不同名 | `Alias` | `Count` → `size`, `Length` → `length` | 属性 getter 替换 |
| 简单表达式（结构稳定） | `Inline` | `Equals` → `===`, `MaxValue` → 字面量 | 单表达式可内联，优先于 `Import` |
| 数组/集合直接映射 | `Alias` / `Inline` / `Import` | `list.Add` → `push`，`list[i]` 需看异常语义 | 不要按“集合操作”一刀切 |
| 解析/转换方法 | `Import` | `Parse`, `TryParse` | 需要验证和错误处理 |
| 需要 JS 特殊逻辑 | `Import` | `Dictionary.Add` 需检查重复键 | C# 语义与 JS 不同 |
| 编译器内置特例 | `Compile` | `TypeOf`, `GetTypeCode()` | 仅限编译器内部保留处理 |
| JS 无概念 | `Discard` | `GetHashCode`, `IFormatProvider` 重载 | 无法等价实现 |

#### 常见成员 Op 选择指南

**基础类型成员**：

| 成员类型 | 推荐 Op | 原因 |
|---------|---------|------|
| `ToString()` | `Alias` → `toString` | JS 原生方法 |
| `Equals(object)` | `Inline` → `===` | 简单比较 |
| `GetHashCode()` | `Discard` | JS 无哈希码概念 |
| `Parse(string)` | `Import` | 需验证和异常处理 |
| `TryParse(string, out T)` | `Import` | 需返回数组 |
| 静态常量（MaxValue等） | `Inline` | 字面量内联 |
| 带 IFormatProvider 重载 | `Discard` | JS 无格式化区域概念 |

补充原则：

- 只要成员还能稳定写成表达式模板，就不要因为“后续可能会复杂”而提前改成 `Import`

**集合类型成员**：

| 成员类型 | 推荐 Op | 原因 |
|---------|---------|------|
| 构造函数 `new List()` | `Inline` → `[]` 或 `new Map()` | 简单构造 |
| `Count` / `Length` | `Alias` → `size` / `length` | 属性名不同 |
| `Add(item)` | `Alias` → `push` (Array) 或 `Import` (Dictionary) | Array 直接替换，Dictionary 需检查重复 |
| `Contains(item)` | `Alias` → `includes` / `has` | 方法名不同 |
| `Remove(item)` | `Alias` → `delete` 或 `Import` | Map 直接删除，List 需查找 |
| 索引器 `this[i]` | `Inline` / `Import` / 未来 `Compile` | 是否需要越界 throw 才是关键，不要只看下标访问 |
| `GetEnumerator()` | `Discard` | JS 迭代协议不同 |
| `CopyTo()` | `Import` | 需要循环复制逻辑 |

**直接反例**：

- `string.this[int].get` 不是“下标访问看起来很直接”就能选 `Inline`
- `Dictionary.Add` 不是“最终只是 `set`”就能选 `Alias`
- `Parse/TryParse` 不是“能写成条件表达式”就该选 `Inline`
- `Compile` 不是“比 Import 更高级”的默认升级路线

#### 按类型簇看

**string 相关**：

| 成员形态 | 推荐 Op | 说明 |
|---------|---------|------|
| `Length` | `Alias` | 直接映射到 `length` |
| `ToUpper/ToLower/Trim/...` | `Alias` | JS 原生方法已存在，只需改名 |
| `IsNullOrEmpty`、`Substring(start, len)` | `Inline` | 稳定单表达式模板 |
| `Compare`、`Format`、复杂 `Split` | `Import` | 含 null 规则、循环或完整协议 |
| `this[int].get` | `Import`，未来可能 `Compile` | 关键不是下标访问，而是越界 `throw` 语义 |

**List<T> 相关**：

| 成员形态 | 推荐 Op | 说明 |
|---------|---------|------|
| 构造、`Count`、`Add`、`Contains` | `Inline` / `Alias` | 大多直接落到数组字面量或 Array 原生方法 |
| `Clear`、简单 slice/range | `Inline` | 仍是稳定单表达式 |
| `CopyTo`、`InsertRange`、`RemoveAll`、范围查找 | `Import` | 需要循环、多步逻辑或边界处理 |
| `this[int].get` | `Import`，未来可能 `Compile` | 越界异常协议不能被“数组下标直取”掩盖 |

**Dictionary<TKey, TValue> 相关**：

| 成员形态 | 推荐 Op | 说明 |
|---------|---------|------|
| `Count`、`ContainsKey` | `Alias` | 直接映射到 `size` / `has` |
| `Keys`、`Values` | `Inline` | `Array.from(...)` 这类稳定模板 |
| `TryGetValue`、`Remove(key, out value)` | `Import` | 有返回包协议 |
| `Add`、`this[key].get` | `Import`，未来可能 `Compile` | 重复键/缺失键异常是核心语义 |
| `this[key].set` | `Inline` | 只是稳定的 `set(...)` 调用 |

**时间与格式化类型**：

| 成员形态 | 推荐 Op | 说明 |
|---------|---------|------|
| carrier 直接 `toString` | `Alias` 或 `Import` | 取决于是否真能直接复用 carrier |
| 解析、格式化、culture/provider 相关重载 | `Import` | 协议完整，不能为了省事压成模板 |
| 简单属性读取 | `Alias` / `Import` | 看 carrier 是否已经稳定暴露对应结构 |
| `TryParse` / `ParseExact` / style 重载 | `Import` 或 `Discard` | 取决于是否真的支持完整协议 |

**tuple / 解构相关**：

| 成员形态 | 推荐 Op | 说明 |
|---------|---------|------|
| 普通 tuple 访问与解构 | 不在 CLR 模块层解决 | 这是编译器语法糖 lowering 问题 |
| 返回 tuple 的运行时 helper | 优先 `Import`，必要时未来 `Compile` | 不要在 `Inline` 模板里手写 tuple 运行时对象形状 |
| 依赖 tuple 结果等价的内建特例 | 未来扩 contract 后再评估 `Compile` | 当前 contract 还不适合承接 tuple 形状拼装 |

**一眼判断的经验规则**：

- 只要你在想“要不要把异常分支也塞进模板”，大概率已经不该继续 `Inline`
- 只要你在想“这里最好缓存一下参数避免重复求值”，大概率已经不该继续 `Inline`
- 只要你在想“其实运行时 helper 更直观”，就先选 `Import`
- 只要你在想“这个语义只有编译器自己最清楚”，再去评估 `Compile`

**特殊类型处理**：

| 类型 | 特殊处理 | 原因 |
|------|---------|------|
| `ReadOnlyDictionary` | 全部 `Discard` | 作为类型约束使用，不需要方法实现 |
| `ReadOnlyCollection` | 部分方法可用 | 底层是数组，部分方法可映射 |
| `IComparer<T>` | 作为参数传递 | 作为委托/回调处理 |
| `IEqualityComparer<T>` | 作为参数传递 | 作为委托/回调处理 |

---

## 4. 模块声明规范

### 4.1 扁平化原则

```
C# 实例方法 ──► 静态方法（实例作为第一个参数）
C# 静态方法 ──► 静态方法（保持原参数）
C# 属性 get ──► 静态方法（实例作为第一个参数）
C# 属性 set ──► 静态方法（实例 + 值作为参数）
```

> **重要**：
> - 成员签名和哈希值已在 `doc/` 目录下预先定义
> - 实现模块时，直接查阅对应的 doc 文档获取 member 和 hash
> - `doc/BooleanModule.md` 和 `module/BooleanModule.cs` 是标准参考样板

### 4.2 基本结构

```csharp
[ECMAScriptModule]
[Jazor(Op.Import, "C#类型名", "模块路径")]
public static class XxxModule { }
```

### 4.3 模块路径规范

| C# 类型 | 模块声明 | 说明 |
|---------|----------|------|
| `bool` | `[Jazor(Op.Import, "bool", "System/BooleanModule.js")]` | 导入外部模块 |
| `Console` | `[Jazor(Op.Alias, "System.Console", "console")]` | 替换为全局对象 |
| `Int32` | `[Jazor(Op.Import, "int", "System/Int32Module.js")]` | 基本类型别名 |
| `DateTime` | `[Jazor(Op.Import, "System.DateTime", "System/DateTimeModule.js")]` | 完整类型名 |

**路径命名规则**：
- 命名空间映射：`System.Collections.Generic` → `System/Collections/Generic/`
- 文件命名：`{类型名}Module.js`
- 泛型类型：`` `n `` 表示参数数量，如 `List`1Module.js`
- 嵌套类型：使用 `+` 连接，如 `Outer+InnerModule.js`

**类型名规范**：
- 基本类型使用关键字：`bool`, `int`, `string`, `object` 等
- 复杂类型使用完整名称：`System.DateTime`, `System.Guid`
- 泛型类型使用 `` `n `` 后缀：`List`1`, `Dictionary`2`

### 4.4 模块类结构

每个模块类必须遵循以下结构：

```csharp
namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "类型名", "模块路径")]
public static class XxxModule
{
    // 静态字段/属性 - extern 方法，不会被编译到 CLR module
    [Jazor(Op.Inline, "static readonly Type.FieldName", "value")]
    public extern static ReturnType _hash();

    // 实例方法 - extern 方法，不会被编译到 CLR module
    [Jazor(Op.Alias, "Type.MethodName(ParamType)", "jsMethod")]
    public extern static ReturnType _hash(Type instance, ParamType param);

    // 静态方法 - 有方法体，会被编译到 CLR module
    [Jazor(Op.Import, "static Type.MethodName(ParamType)")]
    public static ReturnType _hash(ParamType param)
    {
        // JavaScript 实现
    }
}
```

> **核心原则**：只有 `Op.Import` 方法（有方法体）会被编译到 CLR module。其他 Op 类型的 `extern` 方法只是白名单标记，**不会**在 CLR module 中生成代码。

---

## 5. 白名单工作流程

```
Jazor.CLR 项目
     │
     ├── [Jazor] 标记 ───→ 白名单生成器 ───→ Analyzer 白名单
     │                                              │
     │                                              ↓
     │                                    编译时类型/成员检查
     │
     └── 编译 ───────────→ CLR Module 库 ───→ Compiler 引用
```

**流程说明**：
1. 在 Jazor.CLR 中使用 `[Jazor]` 特性标记成员
2. 白名单生成器扫描特性，生成白名单数据
3. Analyzer 使用白名单验证用户代码
4. Compiler 根据白名单查找对应实现

---

## 6. 成员签名格式

成员签名用于白名单匹配，**由 doc 文档预先定义**，开发者无需自行构造。

> **重要**：所有成员签名已在 `doc/` 目录下的文档中定义，实现模块时直接查阅对应文档即可。

### 6.1 签名格式说明（仅供参考）

**方法签名**：`[修饰符] 类型.方法名(参数类型列表)`

```
static bool.Parse(string)
override bool.ToString()
bool.CompareTo(object)
static bool.TryParse(string, out bool)
```

**构造函数签名**：`类型.类型名(参数类型列表)`

```
bool.Boolean()
```

**字段签名**：`[修饰符] 类型.字段名`

```
static readonly bool.TrueString
```

**泛型表示**：使用泛型参数名（`T`, `TKey`, `TValue` 等）

```
List<T>.Add(T)
Dictionary<TKey, TValue>.get_Item(TKey)
System.Collections.Generic.List<T>.Contains(T)
```

> **注意**：成员签名中的泛型使用参数名（如 `<T>`, `<TKey, TValue>`），而非 `` `n `` 格式。
> 但**模块路径**中仍使用 `` `n `` 格式，如 `List`1Module.js`。

**可空表示**：使用 `?` 标记

```
static bool.Parse(string?)
```

**参数类型**：使用完整名称（如 `System.IFormatProvider`）

---

## 7. 方法哈希命名

模块内方法使用哈希值命名（`_` + 16位十六进制），**由 doc 文档预先定义**。

> **重要**：哈希值已在 `doc/` 目录下的文档中定义，开发者必须使用文档中指定的哈希值，**切勿自行计算**。

### 7.1 查阅 doc 文档

以 `doc/BooleanModule.md` 为例：

```markdown
**成员**：static bool.Parse(string)
**签名**：_5dbf54319ebc8dfe

**成员**：override bool.ToString()
**签名**：_d48c2d39317daf8f
```

### 7.2 哈希算法（仅供理解原理）

哈希值由白名单生成器计算：`SHA256(签名) → 取前8字节 → 16位十六进制`

```csharp
// 仅供理解，开发者无需使用
public static string GenerateHashName(string signature)
{
    using var sha256 = SHA256.Create();
    var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(signature));
    var sb = new StringBuilder("_");
    for (int i = 0; i < 8; i++)
        sb.Append(hash[i].ToString("x2"));
    return sb.ToString();
}
```

### 7.3 正确使用示例

```csharp
// ❌ 错误：使用错误的哈希名
[Jazor(Op.Discard, "override bool.GetHashCode()")]
public extern static Number _wronghash(bool instance);

// ✅ 正确：使用 doc/BooleanModule.md 中指定的哈希
[Jazor(Op.Discard, "override bool.GetHashCode()")]
public extern static Number _80b6c29cc0038969(bool instance);
```

---

## 8. 类型映射表

### 8.1 参数类型映射（C# → Jazor.CLR）

在模块方法签名中，C# 类型需要映射为 Jazor.CLR 定义的 JavaScript 类型：

| C# 类型 | Jazor.CLR 类型 | 示例 |
|---------|---------------|------|
| `bool` | `bool` | `public static bool _hash(bool instance)` |
| `int`, `uint`, `short`, `ushort`, `byte`, `sbyte`, `float`, `double`, `decimal` | `Number` | `public static Number _hash(Number instance)` |
| `long`, `ulong`, `Int128`, `UInt128`, `BigInteger` | `BigInt` | `public static BigInt _hash(BigInt instance)` |
| `char`, `string` | `string` | `public static string _hash(string instance)` |
| `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly` | `Date` | `public static Date _hash(Date instance)` |
| `List<T>`, `IList<T>`, `IEnumerable<T>`, `T[]` | `Array<T>` | `public static Array<T> _hash(Array<T> instance)` |
| `Dictionary<K,V>`, `IDictionary<K,V>` | `Map<TKey, TValue>` | `public static Map<TKey, TValue> _hash(Map<TKey, TValue> instance)` |
| `HashSet<T>`, `ISet<T>` | `Set<T>` | `public static Set<T> _hash(Set<T> instance)` |
| `object` | `object` | `public static object _hash(object instance)` |
| `void` | `void` | `public static void _hash(...)` |
| `IFormatProvider` | `Intl.NumberFormat?` | `public static string _hash(Number instance, Intl.NumberFormat? provider)` |
| `ReadOnlySpan<char>`, `Span<char>` | `string` | 无需特殊处理 |
| `System.Type` | `object` | JS 无类型系统 |

**泛型类型参数**：

```csharp
// 泛型模块类
public static class ListModule<T>
{
    // 参数使用泛型 T
    public extern static Array<T> _hash();
    public extern static void _hash(Array<T> instance, T item);
}

// 多泛型参数
public static class DictionaryModule<TKey, TValue> where TKey : notnull
{
    // 参数使用 Map<TKey, TValue>
    public extern static Map<TKey, TValue> _hash();
    public extern static bool _hash(Map<TKey, TValue> instance, TKey key);
}
```

**可空类型处理**：

```csharp
// C# 可空类型
string? value

// Jazor.CLR 签名保持可空标记
public static bool _hash(string? value)
```

### 8.2 基本类型映射（语义层面）

| C# 类型 | JavaScript 类型 | TypeMapper | 备注 |
|---------|-----------------|------------|------|
| `bool` | `boolean` | `Boolean` | 原始类型 |
| `char` | `string` | `String` | 单字符字符串 |
| `string` | `string` | `String` | 原始类型 |
| `object` | `object` | `Object` | 基类 |

### 8.3 数值类型映射

| C# 类型 | JS 类型 | TypeMapper |
|---------|---------|------------|
| `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `float`, `double`, `decimal` | `Number` | `Number` |
| `long`, `ulong`, `Int128`, `UInt128`, `BigInteger`, `TimeSpan` | `BigInt` | `BigInt` |
| `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly` | `Date` | `Date` |

### 8.4 集合类型映射

| C# 类型 | JS 类型 | TypeMapper |
|---------|---------|------------|
| `Array<T>`, `List<T>`, `IList<T>`, `IEnumerable<T>` | `Array` | `Array` |
| `Dictionary<K,V>`, `IDictionary<K,V>` | `Map` | `Map` |
| `HashSet<T>`, `ISet<T>` | `Set` | `Set` |
| `ReadOnlyCollection<T>`, `ReadOnlyDictionary<K,V>` | `Array`/`Map` | `Array`/`Map` |

### 8.5 特殊类型映射

| C# 类型 | JS 类型 | 说明 |
|---------|---------|------|
| `ReadOnlySpan<char>`, `Span<char>` | `string` | 无 |
| `System.Type` | `object` | JS 无类型系统 |
| `System.TypeCode` | - | 丢弃 |
| `void` | `void` | 无返回值 |
| `System.Guid` | `string` | UUID 字符串格式 |
| `System.Version` | `string` | 版本字符串 |

### 8.6 异常类型映射

| C# 异常 | JS 类型 | C# 异常 | JS 类型 |
|---------|---------|---------|---------|
| `Exception`, `SystemException`, `ArgumentException` | `Error` | `ArgumentNullException` | `TypeError` |
| `ArgumentOutOfRangeException`, `IndexOutOfRangeException` | `RangeError` | `DivideByZeroException` | `Error`/`Infinity` |
| `FormatException` | `SyntaxError` | `InvalidOperationException` | `Error` |
| `NotSupportedException` | `Error` | `NullReferenceException` | `TypeError` |

**异常处理注意**：
- JS 不支持异常链，`InnerException` 需手动拼接消息
- 部分 C# 异常在 JS 中用 `Error` 统一表示
- `try-catch-finally` 结构可直接映射

---

## 9. out/ref 参数处理

> **重要**：虽然 C# 中 `out` 和 `ref` 有语义区别，但 **Jazor 使用同种方式处理**：返回数组模拟，调用处解构。

### 9.1 返回数组模式

```csharp
// C# 签名
static bool TryParse(string, out bool result)
static void Modify(ref int value)

// JS 返回值格式（两者相同）
[returnValue, refOutValue]
```

**数组格式规则**：
- 索引 0：方法返回值（void 方法为 `null`）
- 索引 1+：按声明顺序的 out/ref 参数值

### 9.2 定义处示例

以 `doc/BooleanModule.md` 和 `module/BooleanModule.cs` 为例：

```csharp
// 成员：static bool.TryParse(string, out bool)
// 签名：_dada4bbdacd7aa19
[Jazor(Op.Import, "static bool.TryParse(string, out bool)")]
public static Array<object?> _dada4bbdacd7aa19(string? value, bool result)
{
    var str = value?.Trim()?.ToLower();
    if (str == "true")
        return [true, true];   // [返回值, out参数]
    else if (str == "false")
        return [true, false];  // [返回值, out参数]

    return [false, false];     // 解析失败
}
```

**重要注意事项**：
1. 返回类型必须是 `Array<object?>`，不能使用泛型数组
2. out/ref 参数在方法签名中只声明类型和名称，不使用 `out`/`ref` 关键字修饰
3. 数组长度 = 1（返回值）+ out/ref 参数数量

### 9.3 调用处生成

```csharp
// C# 代码
if (bool.TryParse(input, out result)) { }

// JS 生成
let $0;
if (($0 = _hash(input, result), result = $0[1], $0[0])) { }
```

**生成规则**：
1. 编译器自动生成临时变量（如 `$0`）
2. 先调用方法，结果存入临时变量
3. 从数组中提取 out/ref 参数值，赋回原变量
4. 使用返回值（索引 0）作为表达式的最终值

### 9.4 多个 out/ref 参数

```csharp
// C# 签名：static bool TryParse(string, out int value1, out int value2)
// JS 返回：[returnValue, value1, value2]

// 调用处生成
let $0;
if (($0 = _hash(input, value1, value2), value1 = $0[1], value2 = $0[2], $0[0])) { }
```

### 9.5 可空类型处理

在签名和实现中保持可空语义：

```csharp
[Jazor(Op.Import, "static bool.Parse(string?)")]
public static bool _hash(string? value)
{
    var str = value?.Trim()?.ToLower();  // 使用可选链
    // ...
}
```

---

## 10. 端到端示例

以下示例展示如何根据 `doc/BooleanModule.md` 实现 `module/BooleanModule.cs`。

### 10.1 步骤 1：查阅 doc 文档

```markdown
# doc/BooleanModule.md

**成员**：static bool.Parse(string)
**签名**：_5dbf54319ebc8dfe
**注释**：
<summary>Converts the specified string representation of a logical value to its <see cref="T:System.Boolean" /> equivalent.</summary>

**成员**：override bool.ToString()
**签名**：_d48c2d39317daf8f
**注释**：
<summary>Converts the value of this instance to its equivalent string representation.</summary>

**成员**：static bool.TryParse(string, out bool)
**签名**：_dada4bbdacd7aa19
**注释**：
<summary>Tries to convert the specified string representation of a logical value to its <see cref="T:System.Boolean" /> equivalent.</summary>
```

### 10.2 步骤 2：实现模块代码

```csharp
// module/BooleanModule.cs
namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "bool", "System/BooleanModule.js")]
public static class BooleanModule
{
    // 从 doc/BooleanModule.md 获取签名和注释

    ///<summary>Converts the value of this instance to its equivalent string representation.</summary>
    [Jazor(Op.Alias, "override bool.ToString()", "toString")]
    public extern static string _d48c2d39317daf8f(bool instance);

    ///<summary>Converts the specified string representation of a logical value to its Boolean equivalent.</summary>
    [Jazor(Op.Import, "static bool.Parse(string)")]
    public static bool _5dbf54319ebc8dfe(string? value)
    {
        var str = value?.Trim()?.ToLower();
        if (str == "true") return true;
        if (str == "false") return false;
        throw new Error($"FormatException: String '{value}' was not recognized as a valid Boolean.");
    }

    ///<summary>Tries to convert the specified string representation of a logical value to its Boolean equivalent.</summary>
    [Jazor(Op.Import, "static bool.TryParse(string, out bool)")]
    public static Array<object?> _dada4bbdacd7aa19(string? value, bool result)
    {
        var str = value?.Trim()?.ToLower();
        if (str == "true") return [true, true];
        if (str == "false") return [true, false];
        return [false, false];
    }
}
```

### 10.3 关键要点

1. **成员签名**：直接从 doc 文档复制，如 `static bool.Parse(string)`
2. **哈希命名**：使用 doc 文档中指定的签名，如 `_5dbf54319ebc8dfe`
3. **XML 注释**：从 doc 文档复制，保持一致性
4. **Op 类型**：根据方法特性选择（Alias/Inline/Import 等）

---

## 11. 错误处理与调试

### 11.1 常见错误场景

| 错误信息 | 原因 | 解决方案 |
|---------|------|----------|
| `Type 'X' is not in whitelist` | 类型未在白名单中 | 检查类型是否被 `[Jazor]` 标记 |
| `Member 'X' is not in whitelist` | 成员未在白名单中 | 检查成员签名是否正确标记 |
| `Hash mismatch for member 'X'` | 方法哈希名与签名不匹配 | 使用 doc 文档中指定的正确哈希值 |
| `Method not compiled` | `extern` 方法但 Op 不是 Import | Import 类型**不能**使用 `extern` |

### 11.2 调试步骤

1. **检查白名单内容**
   ```bash
   # 查看生成的白名单文件
   cat src/ECMAScript.Analyzer/WhiteList.cs
   ```

2. **验证签名格式**
   - 确保签名与 doc 文档中的 member 完全一致
   - 注意泛型使用 `` `n `` 格式
   - 注意可空类型使用 `?` 标记

3. **验证哈希值**
   - 查阅对应的 doc 文档
   - 确保方法名使用正确的哈希值

4. **检查特性标记**
   - 确认使用了 `[Jazor]` 特性
   - 确认 Op 类型正确
   - 确认 `extern` 使用正确

### 11.3 extern 使用规则

| Op 类型 | extern? | 原因 |
|---------|---------|------|
| `Discard` | ✅ | 不需要实现，标记为不支持 |
| `Allowed` | ✅ | JS 原生支持，无需额外代码 |
| `Alias` | ✅ | JS 原生方法，只需改名 |
| `Inline` | ✅ | 内联代码提供实现 |
| `Import` | ❌ | 需要完整的 C# 方法体实现 |
| `Compile` | ✅ | 编译器内部处理 |

### 11.4 常见陷阱与解决方案

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| 方法体未被编译到 CLR module | `extern` 方法只有白名单标记作用 | 只有 `Op.Import` 需要方法体，其他用 `extern` |
| 占位符未替换 | `__argN` 格式错误 | 使用正确格式 `__arg1`, `__arg2` |
| 返回值类型错误 | 使用泛型而非 `Array<object?>` | out/ref 方法必须返回 `Array<object?>` |
| 循环引用 | Import 方法互相调用 | 避免模块间循环依赖 |
| 类型映射错误 | 参数类型不正确 | 参考 GlobalUsings.cs 中的类型定义 |

---

## 12. 快速参考

### 12.1 Op 类型速查表

| 场景 | Op 类型 | extern? | 编译到 CLR module? |
|------|---------|---------|-------------------|
| JS 原生支持，无需处理 | `Allowed` | ✅ | ❌ |
| JS 有类似方法但名称不同 | `Alias` | ✅ | ❌ |
| 可用简单表达式实现 | `Inline` | ✅ | ❌ |
| 需要完整实现 | `Import` | ❌ | ✅ |
| 编译器特殊处理 | `Compile` | ✅ | ❌ |
| 不支持 | `Discard` | ✅ | ❌ |

### 12.2 占位符速查表

| 方法类型 | __arg1 | __arg2 | __arg3 |
|----------|-------|-------|-------|
| 实例方法 | 实例 | 参数1 | 参数2 |
| 静态方法 | 参数1 | 参数2 | 参数3 |
| 扩展方法 | 被扩展对象 | 参数1 | 参数2 |

### 12.3 类型映射速查表

| C# 类型 | JS 类型 | 类型检查方式 |
|---------|---------|-------------|
| `bool` | `boolean` | `typeof x === "boolean"` |
| `string` | `string` | `typeof x === "string"` |
| `int`, `double` | `number` | `typeof x === "number"` |
| `long`, `BigInteger` | `bigint` | `typeof x === "bigint"` |
| `DateTime` | `Date` | `x instanceof Date` |
| `Array`, `List<T>` | `Array` | `Array.isArray(x)` |
| `object` | `object` | `typeof x === "object"` |

### 12.4 特性使用模式

```csharp
// 实例方法
[Jazor(Op.Alias, "Type.MethodName(ParamType)", "jsMethodName")]
public extern static ReturnType _hash(Type instance, ParamType param);

// 静态方法
[Jazor(Op.Alias, "static Type.MethodName(ParamType)", "jsMethodName")]
public extern static ReturnType _hash(ParamType param);

// 带 out 参数
[Jazor(Op.Import, "static Type.TryParse(string, out Type)")]
public static Array<object?> _hash(string? value, Type result)
{
    return [true, parsed];  // [success, value]
}
```

---

## 13. 附录

### 13.1 类型模块示例

**Boolean 类型**：

| C# 成员 | Op | JavaScript 结果 |
|---------|-----|-----------------|
| `static bool.TrueString` | Inline | `"true"` |
| `bool.Boolean()` | Allowed | 无操作 |
| `override bool.ToString()` | Alias | `instance.toString()` |
| `override bool.Equals(object)` | Inline | `(a === b)` |
| `static bool.Parse(string)` | Import | 模块函数调用 |
| `static bool.TryParse(string, out bool)` | Import | 返回 `[success, value]` |

**Int32 类型**：

| C# 成员 | Op | JavaScript 结果 |
|---------|-----|-----------------|
| `static int.MaxValue` | Inline | `2147483647` |
| `static int.Parse(string)` | Import | 模块函数，带验证 |
| `static int.Max(int, int)` | Alias | `Math.max(a, b)` |

**String 类型**：

| C# 成员 | Op | JavaScript 结果 |
|---------|-----|-----------------|
| `string.get_Length()` | Alias | `str.length` |
| `string.Contains(string)` | Alias | `str.includes(value)` |
| `string.Trim()` | Alias | `str.trim()` |
| `static string.IsNullOrEmpty(string)` | Inline | `!value` |

### 13.2 设计原则

1. **GetHashCode 处理差异**：`bool/object.GetHashCode()` → `Discard`（JS 无哈希码机制）；`string.GetHashCode()` → `Import`（有实际用途）
2. **ToString 使用 Alias**：JS 原生支持 `toString()`，直接调用原生方法效率更高
3. **Equals 使用 Inline**：`===` 与 C# `Equals` 语义一致，内联避免函数调用开销
4. **Parse/TryParse 使用 Import**：解析逻辑复杂，需完整 JS 实现
5. **Import 是最后手段**：只要能稳定 `Inline`，就不要引入模块实现和导入成本

### 13.3 模块开发状态说明

`module/` 目录是开发目录，部分模块尚未完成开发。

**待开发模块示例**：

```csharp
// ReadOnlyDictionaryModule - 待开发状态
// 当前所有成员都是 Discard，需要根据实际需求改为正确的 Op 类型
[ECMAScriptModule]
[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>", "...")]
public static class ReadOnlyDictionaryModule<TKey, TValue> where TKey : notnull
{
    // TODO: 需要根据 JS Map 特性选择正确的 Op 类型
    // 例如：ContainsKey 可以改为 Alias("has")
    [Jazor(Op.Discard, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey)")]
    public extern static bool _08bd8c3015d3691e(Map<TKey, TValue> instance, object key);

    // 其他成员...
}
```

**开发优先级判断**：

| 优先级 | 判断依据 | 示例 |
|--------|---------|------|
| 高 | 常用 API，用户代码频繁调用 | `ContainsKey`, `TryGetValue`, `Count` |
| 中 | 较常用但可替代 | `Keys`, `Values`, `GetEnumerator` |
| 低 | 罕见使用场景 | 特殊构造函数、序列化方法 |

**完成模块开发的步骤**：

1. **查阅 doc 文档**：获取成员签名和哈希值
2. **选择 Op 类型**：参考 [3.8 Op 类型选择决策](#38-op-类型选择决策)
3. **实现方法**：
   - `Alias`: 指定 JS 方法名
   - `Inline`: 编写内联表达式
   - `Import`: 编写完整方法体
4. **移除 `extern`**：`Import` 类型必须有方法体
5. **测试验证**：确保生成的 JavaScript 代码正确

**参考已完成的模块**：

| 模块 | 完成度 | 可参考内容 |
|------|--------|-----------|
| `BooleanModule` | ✅ 完整 | 基础类型完整实现 |
| `Int32Module` | ✅ 完整 | 数值类型、Math 方法映射 |
| `StringModule` | ✅ 完整 | 字符串操作、Alias/Inline 混合使用 |
| `ListModule` | ✅ 完整 | 泛型集合、Array 方法映射 |
| `DictionaryModule` | ✅ 完整 | 泛型字典、Map 方法映射 |

### 13.4 注意事项

1. **成员签名和哈希**：已在 doc 文档中定义，直接查阅使用
2. **out/ref 统一处理**：返回数组 `[returnValue, outParam1, ...]`，调用处解构
3. **可空处理**：`string?` 使用可选链 `?.` 处理空值
4. **类型系统差异**：C# `GetType()` 返回 `Type`；JS `typeof` 返回字符串
5. **Console 差异**：C# `Write` 不换行，JS `console.log` 总是换行
6. **嵌套类型路径**：使用 `+` 连接，如 `Outer+InnerModule.js`
7. **泛型类型路径**：使用 `` `n `` 标记参数数量，如 `List`1Module.js`
8. **方法重载**：JS 不支持重载，不同重载需不同哈希名
9. **循环引用**：避免 `Op.Import` 方法互相调用
10. **XML 注释与 Jazor 特性的转义规则**：

**XML 注释与 Jazor 特性的转义规则**：

| 位置 | `<` | `>` | `&` | 说明 |
|------|-----|-----|-----|------|
| XML 注释 (`/// <summary>`) | `&lt;` | `&gt;` | `&amp;` | XML 格式要求，必须使用 HTML 实体 |
| Jazor 特性值 (`[Jazor(Op.Inline, ..., "code")]`) | `<` | `>` | `&&` 或 `&` | 直接使用 JavaScript 代码字符 |

**正确示例**：
```csharp
/// <summary>
/// C#: char.IsAsciiLetter(c)
/// JS: (c &gt;= 65 &amp;&amp; c &lt;= 90) || (c &gt;= 97 &amp;&amp; c &lt;= 122)
/// </summary>
[Jazor(Op.Inline, "static char.IsAsciiLetter(char)", "((__arg1 >= 65 && __arg1 <= 90) || (__arg1 >= 97 && __arg1 <= 122))")]
public extern static bool _hash(Number c);
```

**错误示例**（Jazor 特性值中错误使用 HTML 实体）：
```csharp
// ❌ 错误：Jazor 特性值中使用了 HTML 实体
[Jazor(Op.Inline, "static char.IsAsciiLetter(char)", "((__arg1 &gt;= 65 &amp;&amp; __arg1 &lt;= 90))")]

// ✅ 正确：Jazor 特性值中使用实际的 JavaScript 字符
[Jazor(Op.Inline, "static char.IsAsciiLetter(char)", "((__arg1 >= 65 && __arg1 <= 90))")]
```

**原因**：
- XML 注释是 XML 格式，`<` `>` `&` 是 XML 特殊字符，必须转义
- Jazor 特性值是 JavaScript 代码字符串，会被直接嵌入生成的 JavaScript 代码中，不需要转义

### 13.5 边界情况处理

**null 值处理**：
```csharp
// C# 可空参数
[Jazor(Op.Import, "static bool.Parse(string?)")]
public static bool _hash(string? value)
{
    // 使用可选链处理 null
    var str = value?.Trim()?.ToLower();
    if (str == null)
        throw new Error("Value cannot be null.");
    // ...
}
```

**类型转换边界**：
```csharp
// 数值溢出处理
[Jazor(Op.Import, "static int.Parse(string)")]
public static Number _hash(string? value)
{
    var num = Number(value);
    // JS Number 安全整数范围检查
    if (num > 2147483647 || num < -2147483648)
        throw new Error("OverflowException: Value was either too large or too small.");
    return num;
}
```

**空集合处理**：
```csharp
// 空数组返回
[Jazor(Op.Import, "static T[] Array.Empty<T>()")]
public static Array<object?> _hash()
{
    return [];  // 返回空数组
}
```

### 13.6 不支持的特性

| 特性 | 原因 |
|------|------|
| 事件 (`event`) | JS 使用回调/订阅模式，无多播事件 |
| 委托 (`delegate`) | JS 只有函数引用 |
| `unsafe` 代码 | JS 是安全语言，无指针操作 |
| `sizeof`/`stackalloc` | JS 无固定内存布局和栈分配 |

### 13.7 相关文档

- [CLAUDE.md](../../CLAUDE.md) - 项目整体架构和转换思想
- [Jazor.Name/rule.md](../Jazor.Name/rule.md) - 命名规范详细说明
- `doc/BooleanModule.md` / `module/BooleanModule.cs` - 标准参考样板

---

**文档维护者**：developerhan
**最后更新**：2026-02-28
**文档版本**：v3.1
