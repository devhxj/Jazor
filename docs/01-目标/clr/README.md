# CLR 运行时支持与白名单机制

> 对应源码：`src/Jazor.CLR/`、`src/Jazor.CLR.Generator/`、`src/Jazor.Compiler.Generator/`

## 为什么需要

C# 到 JavaScript 的转换不是任意代码都能转的——只有**已明确映射过的 .NET 类型和方法**才能安全地转换。如果放任用户调用任何 .NET API，生成的 JavaScript 代码要么语义不等价，要么直接报错。白名单机制就是这道安全闸门。

## 解决什么问题

1. **编译时验证**：用户代码只能使用白名单中的类型和成员，否则 Analyzer 报错
2. **运行时映射**：白名单中的每个成员都有对应的 JavaScript 实现（在 Jazor.CLR 中用 C# 编写，语法贴合 JavaScript）
3. **自动同步**：白名单列表由 Generator 自动扫描 Jazor.CLR 中的 `[WhiteList]` 特性生成，避免手写遗漏

## 大致实现思路

### 三层协作

```
Jazor.CLR（声明层）
  用 [ECMAScriptModule] + [WhiteList("name")] 标注每个可转换的模块和成员
  用 [ECMAScriptLiteral("code")] 嵌入 JS 代码片段，或用 C# 实现复杂逻辑
       ↓ Generator 扫描
Jazor.Compiler.Generator / Jazor.CLR.Generator（生成层）
  扫描 Jazor.CLR 程序集中的 [WhiteList] 特性
  自动生成 WhiteList.cs（HashSet<string> 类型和成员全名）
  生成各模块的 JS 绑定代码
       ↓ 编译时使用
Jazor.Analyzer（验证层）
  在用户代码编译时检查：使用的类型/成员是否在 WhiteList.Types / WhiteList.Members 中
```

### Jazor.CLR 中的模块示例

```csharp
[ECMAScriptModule]
[WhiteList("bool")]
public static class BooleanModule
{
    [WhiteList("override bool.GetHashCode()")]
    [ECMAScriptLiteral("@#{0} ? 1 : 0")]
    public extern static Number BooleanGetHashCode(bool instance);

    [WhiteList("static bool.Parse(string)")]
    public static bool BooleanParse(string value) { /* C# 实现 */ }
}
```

### 已覆盖的模块

- 基础类型：Boolean、Char、String、Object
- 数值类型：SByte → UInt64、BigInteger、Decimal、Double、Single
- 日期时间：DateTime、DateTimeOffset、DateOnly、TimeOnly、TimeSpan
- 集合：List、Dictionary、HashSet、ReadOnlyCollection/Dictionary/Set
- 其他：StringBuilder、Nullable、ValueTuple、Console、Math、Exception

### Jazor.CLR.Generator 的职责

独立的代码生成工具，内建 .NET 类型到 JavaScript 类型的完整映射表（如 `Int32 → Number`、`DateTime → Date`、`BigInteger → BigInt`），并从 `System.Private.CoreLib.xml` 提取 XML 文档用于生成绑定代码。

## 参考文档

- 各模块详细文档：`src/Jazor.CLR/doc/*.md`（47 个模块文档保留在源码旁）
- 白名单规范：`docs/01-目标/compiler/WhiteList.md`
