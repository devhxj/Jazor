# BCL 模块映射规则 - 待澄清问题

本文档记录 rule.md 中需要澄清的技术细节和设计决策。

> **Jazor.CLR 的双重作用**：
> 1. **白名单来源**：为 Jazor 编译器提供白名单配置
> 2. **CLR Module 库**：被 Jazor 编译成 CLR module，供编译时引用

## 特性体系说明

### 体系一：[ECMAScript] 系列（用户代码标记）

用于开发者标记自己的 C# 代码，告诉 Jazor 编译器如何转换为 JavaScript。

| 特性 | 命名空间 | 用途 |
|------|----------|------|
| `[ECMAScriptModule]` | `ECMAScript` | 标记类生成 ES module |
| `[ECMAScript]` | `ECMAScript` | 标记可被编译器识别的资源型类型 |
| `[ECMAScriptIgnore]` | `ECMAScript` | 标记被编译器忽略的成员 |
| `[ECMAScriptInline]` | `ECMAScript` | 标记方法直接使用内联代码 |

**使用场景**：开发者在自己的项目中使用

### 体系二：[Jazor]（Jazor.CLR 内部使用）

用于 `Jazor.CLR` 项目内部，实现双重功能：

1. **白名单来源**：`Jazor.Compiler.Generator` 读取 `[Jazor]` 特性生成白名单
2. **CLR Module 编译**：`Jazor.CLR` 项目本身被编译成 CLR module 库，供编译时引用

| 特性 | 命名空间 | 用途 |
|------|----------|------|
| `[Jazor]` | `Jazor.Common` | 标记 BCL 类型的映射规则 |

**工作流程**：

```
Jazor.CLR 项目
     │
     ├── [Jazor] 标记 ───→ 白名单生成器 ───→ Analyzer 白名单
     │
     └── 编译 ───────────→ CLR Module 库 ───→ Compiler 引用
```

## 1. 占位符替换机制

### 1.1 核心设计原则

**Jazor.CLR 将 BCL 类型扁平化为静态类静态方法**：

```
C# 实例方法 ──► 静态方法（实例作为第一个参数）
C# 静态方法 ──► 静态方法（保持原参数）
```

### 1.2 占位符替换规则

**`@#{n}` 替换的是参数表达式经过 AST 转换后的 JavaScript 代码**：

| 方法类型 | C# 签名 | 映射后 | @#{0} | @#{1} |
|---------|---------|--------|-------|-------|
| 实例方法 | `bool GetHashCode()` | `BooleanGetHashCode(bool instance)` | **实例表达式的 JS 代码** | - |
| 双参实例 | `bool Equals(object)` | `BooleanEquals(bool instance, object obj)` | 实例表达式 | obj 表达式 |
| 静态方法 | `static bool Parse(string)` | `BooleanParse(string value)` | value 表达式 | - |

**重要说明**：
- `@#{0}` 在实例方法中代表**实例表达式的完整 JS 代码**
- 实例可以是简单变量（如 `a`），也可以是复杂表达式（如 `GetValue().Property`）
- 占位符替换的是**已转换的 JavaScript 表达式**，不是原始标识符

**示例**：

```csharp
// C# 调用：a.GetHashCode()
[Jazor(Op.Inline, "override bool.GetHashCode()", "@#{0} ? 1 : 0")]
public extern static Number _xxx(bool instance);
// 生成的 JS：a ? 1 : 0

// C# 调用：GetValue().Property.GetHashCode()
// 生成的 JS：GetValue().Property ? 1 : 0
```

### 1.3 out/ref 参数处理规则

**out 和 ref 参数的占位符与普通参数一样处理**，特殊处理发生在**调用处**。

**定义处**：

```csharp
[Jazor(Op.Import, "static bool.TryParse(string, out bool)")]
public extern static Array _xxx(string value, bool result);
// 注意：返回 Array 包含 [bool success, bool result]
// 参数 result 的占位符 @#{1} 与普通参数一样处理
```

**调用处处理**：

```csharp
// C# 调用代码
if (bool.TryParse(input, out bool result))
{
    Console.WriteLine(result);
}
```

**生成的 JavaScript**：

```javascript
// 编译器生成逗号表达式，从返回数组中解构
let _temp;
if ((_temp = TryParse(input, false))[0]) {
    let result = _temp[1];
    console.log(result);
}
```

**规则总结**：

| 方面 | 处理方式 |
|------|----------|
| **定义处占位符** | `@#{0}`=value, `@#{1}`=result，与普通参数相同 |
| **调用处生成** | 逗号表达式 `(_temp = method(args))[0]` |
| **out/ref 赋值** | 从返回数组中取出对应索引值赋给变量 |
| **多个 out/ref** | 按签名中的顺序，依次从数组中取值 |

### 1.3 建议补充的文档内容

```markdown
### 占位符替换详细规则

| 场景 | @#{0} | @#{1} | @#{2} | 说明 |
|------|-------|-------|-------|------|
| 实例方法 | this | 第一个显式参数 | 第二个显式参数 | 包含隐式 this |
| 静态方法 | 第一个参数 | 第二个参数 | 第三个参数 | 无 this |
| 扩展方法 | 被扩展对象 | 第一个显式参数 | 第二个显式参数 | 待确认 |

**重要**：占位符替换的是**已转换的 JavaScript 表达式**，不是原始标识符。
```

---

## 2. 参数命名约定

### 2.1 当前现象

示例代码中实例方法的第一个参数常命名为 `instance`：

```csharp
public extern static string _xxx(bool instance, object? obj);
```

### 2.2 需要澄清的问题

- `instance` 这个命名是否有特殊含义？编译器是否会特殊处理？
- 还是纯粹为了代码可读性，实际只看参数位置？
- 是否可以使用其他命名（如 `self`, `thisValue`）？

### 2.3 建议

如果是纯粹的可读性约定，建议在文档中明确：

```markdown
**参数命名建议**（仅为可读性，不影响功能）：
- 实例方法的第一个参数建议命名为 `instance`
- 静态方法的参数使用有意义的名称
- 实际占位符替换基于参数位置，与名称无关
```

---

## 3. `extern` 关键字含义

### 3.1 定义

### 3.1 核心规则

**`extern` 表示该方法不需要实现或没有实现。**

**`extern + Op` 组合的具体含义**：

| 组合 | 含义 | 实际行为 |
|------|------|---------|
| `extern + Allowed` | JS 自有，无需处理 | JS 原生支持该语义，无需映射代码 |
| `extern + Replace` | JS 有同样语义的方法，但名称不同 | 调用 JS 原生方法，只需替换名称 |
| `extern + Inline` | 直接内联代码片段 | 替换为指定的内联表达式 |
| `extern + Discard` | 不支持 | 该成员在 JS 中无对应概念 |
| `extern + Import` | 从外部 JS 模块导入 | 实现存在于其他 JS 文件中 |

### 3.3 Op.Import 说明

**Import 只有一种形式：Import + extern**

```csharp
[Jazor(Op.Import, "static bool.Parse(string)")]
public extern static bool _xxx(string value);
```

- **含义**：该方法的实现存在于外部 JS 文件中
- **代码生成**：生成 `import { Parse } from '...'` 或全局调用
- **使用场景**：实现已在其他 JS 模块中提供

### 3.4 补充说明

- `extern` 是 C# 语法的一部分，表示**外部实现**
- 在 `Jazor.CLR` 中，它明确表示**该代码不会作为 C# 运行**
- **所有 `[Jazor]` 标记的方法都使用 `extern`**，不存在有实现的情况
- 在 `Jazor.CLR` 中，它明确表示**该代码不会作为 C# 运行**
- 没有 `extern` 的方法表示**该 C# 代码会被编译器处理**（提取/转换）

---

## 4. 字段和属性的映射

### 4.1 当前缺失

rule.md 主要关注方法映射，缺少字段和属性的明确规则。

### 4.2 需要澄清的问题

#### 问题 1：const/static readonly 字段的映射

```csharp
public static class BooleanModule
{
    // 如何映射 bool.TrueString？
    [Jazor(Op.Inline, "static readonly bool.TrueString", "\"true\"")]
    public extern static string _xxx(); // 这样正确吗？
}
```

#### 问题 2：属性的 get/set 如何区分？

```csharp
// 如何映射属性？
[Jazor(Op.Replace, "int.Length", "length")]  // 这样正确吗？
public extern static int _xxx(string instance);
```

#### 问题 3：自动属性的处理

- C# 自动属性在 JS 中如何表示？
- 是映射为字段还是 getter/setter 函数？

### 4.3 建议补充的文档内容

```markdown
### 字段映射规则

| 字段类型 | Op 选择 | 示例 |
|----------|---------|------|
| const | Inline | `[Jazor(Op.Inline, "const bool.TrueString", "\"true\"")]` |
| static readonly | Inline | 同上 |
| static 可写字段 | Import | 需要 JS 实现 setter |

### 属性映射规则

使用 `.get_` 和 `.set_` 前缀区分：

```csharp
// Getter
[Jazor(Op.Replace, "string.get_Length()", "length")]
public extern static int _xxx(string instance);

// Setter（如果可写）
[Jazor(Op.Import, "string.set_Chars(int, char)")]
public extern static void _yyy(string instance, int index, char value);
```
```

---

## 5. 可空类型的处理

### 5.1 当前缺失

rule.md 提到了 `string?` 映射为 `string | null`，但没有说明在映射方法中如何处理。

### 5.2 需要澄清的问题

#### 问题 1：可空参数的类型声明

```csharp
// C# 方法签名：static bool Parse(string? value)

[Jazor(Op.Import, "static bool.Parse(string?)")]
public static bool _xxx(string? value)  // 使用 string? 还是 string？
```

#### 问题 2：可空类型的默认值

```csharp
// C# 方法签名：static bool TryParse(string? value, out bool result)

[Jazor(Op.Import, "static bool.TryParse(string?, out bool)")]
public static (bool, bool) _xxx(string? value, bool result = false)  // 需要默认值吗？
```

### 5.3 建议补充的文档内容

```markdown
### 可空类型处理规则

1. **参数类型声明**：在模块方法中使用可空类型（`string?`）保持与 C# 一致
2. **实现中使用可选链**：`value?.Trim()?.ToLower()`
3. **签名中的可空标记**：在 `[Jazor]` 签名中使用 `?` 标记可空类型
```

---

## 6. 签名格式规范

### 6.1 需要澄清的问题

#### 问题 1：不同类型成员的签名格式

- 方法：`override bool.ToString()`、`static bool.Parse(string)`
- 构造函数：`bool.Boolean()`、`bool.Boolean(bool)`
- 属性 getter：`bool.get_Value()` 还是 `bool.Value { get }`？
- 属性 setter：`bool.set_Value(bool)` 还是 `bool.Value { set }`？
- 字段：`static readonly bool.TrueString` 还是其他格式？

#### 问题 2：泛型类型的签名格式

```csharp
// List<T>.Add(T) 的签名是什么？
// 是 "List<T>.Add(T)" 还是 "List`1.Add(T)"？
```

#### 问题 3：重载方法的区分

```csharp
// 两个 Parse 方法如何区分签名？
static bool.Parse(string)
static bool.Parse(ReadOnlySpan<char>)
```

### 6.2 建议补充的文档内容

```markdown
### 签名格式规范

#### 方法签名
```
[修饰符] 返回类型.方法名(参数类型列表)

示例：
- static bool.Parse(string)
- override bool.ToString()
- bool.CompareTo(object)
```

#### 构造函数签名
```
类型.类型名(参数类型列表)

示例：
- bool.Boolean()
- bool.Boolean(bool)
```

#### 属性签名
```
类型.get_属性名()
类型.set_属性名(参数类型)

示例：
- string.get_Length()
- string.set_Chars(int, char)
```

#### 字段签名
```
[修饰符] 类型.字段名

示例：
- static readonly bool.TrueString
- const int.MaxValue
```

#### 泛型签名
```
使用 ` 标记泛型参数数量：
- List`1.Add(T)
- Dictionary`2.get_Item(TKey)
```
```

---

## 7. 第 9 节 "参考方式" 的澄清

### 7.1 当前文本

> 项目的 module 目录表示需要转换的模块，doc 目录下存在着同名称的文档，在转换时不需要参考 module 目录中的模块，因为：
> - module 中的模块都是需要完善的，目前不是最终版本，不具备参考价值
> - 在转换时需要检查映射方法的入参和返回值是否映射合理
> - 在转换时需要考虑 Jazor 特性参数是否配置合理
> - 在转换时需要分析 op 类型选择是否合适

### 7.2 需要澄清的问题

- "需要转换的模块"指的是什么？是指 module 目录下的 C# 代码需要被转换成其他形式？
- 读者（使用者）应该如何使用这份文档？
- "转换时"指的是什么转换？C# 到 JS 的编译？还是 module 的迁移？

### 7.3 建议的重写

```markdown
## 9. 文档使用指南

### 目录结构说明

```
Jazor.CLR/
├── module/     # 模块实现代码（C# 编写，待完善）
├── doc/        # 参考文档（各类型映射的详细说明）
└── rule.md     # 本规则文档
```

### 使用建议

1. **实现新模块时**：
   - 优先参考本 rule.md 的通用规则
   - 参考 doc/ 目录下对应类型的详细说明
   - 不直接参考 module/ 目录（因为正在完善中）

2. **验证实现时**：
   - 检查入参和返回值类型映射是否正确
   - 检查 `[Jazor]` 特性参数配置是否合理
   - 检查 Op 类型选择是否合适
```

---

## 8. 命名格式约定

### 8.1 NameFormat 规范

成员名称统一使用 `Jazor.Name.Format.NameFormat` 进行约定，在 **Analyzer**、**Compiler** 和 **CLR** 中保持一致。

### 8.2 需要澄清的问题

- `NameFormat` 的具体格式是什么？
- 如何生成方法/属性的统一名称？
- 签名中的类型名是否使用全名（如 `System.String` 还是 `string`）？
- 泛型参数如何表示？

### 8.3 示例格式（待确认）

```
方法：Namespace.Type.MethodName(ParameterTypes)
属性：Namespace.Type.PropertyName
字段：Namespace.Type.FieldName
```

---

## 8. 成员命名格式规范（NameFormat）

### 8.1 概述

成员名称使用 `Jazor.Name.Format.NameFormat` 进行统一格式化，在 Analyzer、Compiler 和 CLR 中保持一致。

**NameFormat 就是具体的规范格式**，无需额外转换。

### 8.2 NameFormat 规范

#### 方法签名

```
[修饰符] 返回类型.方法名(参数类型列表)

示例：
- static bool.Parse(string)
- override bool.ToString()
- bool.CompareTo(object)
- static bool.TryParse(string, out bool)
```

#### 构造函数签名

```
类型.类型名(参数类型列表)

示例：
- bool.Boolean()
- bool.Boolean(bool)
```

#### 属性签名

```
类型.get_属性名()
类型.set_属性名(参数类型)

示例：
- string.get_Length()
- string.set_Chars(int, char)
```

**映射规则**：
- 属性映射为 **2 个静态方法**（get 和 set）
- 实例属性的第一个参数是 **实例本身**
- get 方法：`PropertyGet(实例)`
- set 方法：`PropertySet(实例, 值)`

```csharp
// C# 属性：instance.Length
[Jazor(Op.Replace, "string.get_Length()", "length")]
public extern static int _xxx(string instance); // @#{0} = instance

// 生成的 JS：instance.length
```

#### 字段签名

```
[修饰符] 类型.字段名

示例：
- static readonly bool.TrueString
- const int.MaxValue
```

**注意**：字段映射规则**待定**
- 原则上字段不映射
- 特殊字段（如 `int.MaxValue`）可能使用 `Op.Compile` 交给编译器处理

#### 泛型签名

```
使用 ` 标记泛型参数数量：
- List`1.Add(T)
- Dictionary`2.get_Item(TKey)
```

**注意**：泛型类型在 C# 中约束，在 JavaScript 中**类型擦除**：
- `List<T>` → JS 数组 `Array`
- `Dictionary<K,V>` → JS `Map`
- 泛型参数 `T`, `K`, `V` 在 JS 运行时不可见

#### 可空类型签名

```
可空类型在签名中使用 `?` 标记：
- static bool.Parse(string?)
- bool.TryParse(string?, out bool)
```

**注意**：可空类型在 JavaScript 中**保持可空性**，需要运行时检查：
- `string?` → JS `string | null | undefined`
- 实现中使用可选链 `?.` 处理可空值

### 8.3 规范要点

- **完整类型名**：包含命名空间（如 `System.Boolean.Parse`）
- **修饰符标记**：`static`, `override`, `virtual`, `abstract` 等
- **参数类型**：包含 `out`, `ref`, `in`, `params` 等修饰符
- **泛型表示**：使用 `` `n `` 表示泛型参数数量

---

## 附录：待确认问题清单

### 已确认 ✅

- [x] 实例方法的 `@#{0}` 是实例表达式经过 AST 转换后的 JS 代码
- [x] `extern` 表示不需要实现或没有实现
- [x] op.Import标注的方法不存在extern，必须有方法体，会被jazor编译成导出方法
- [x] out/ref 参数占位符与其他参数一样处理，调用处生成逗号表达式
- [x] 成员名称使用 `Jazor.Name.Format.NameFormat` 统一约定

### 待确认 ❓

- [x] 泛型类型在 JS 中擦除（`List<T>` 映射为数组）
- [x] 可空类型在签名中保持可空性（`string?`）
- [x] 属性映射为 2 个静态方法（get/set），实例属性第一个参数是自身
- [ ] 字段映射规则待定（原则上不映射，特殊字段可能使用 Op.Compile）

---

**文档版本**：v1.0
**创建日期**：2026-02-25
**状态**：待评审
