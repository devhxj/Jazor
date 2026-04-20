# AstConverter.cs 问题增改工作清单

> 审查日期：2026-03-07
> 文件位置：`src/Jazor.Compiler/AstConverter.cs`

---

## 一、严重问题 (P0)

### 1.1 BackingField 重复声明

**问题描述**：
处理属性时，getter 和 setter 各自查找 BackingField，导致生成重复的变量声明。

**复现代码**：
```csharp
public static class TestClass
{
    public static int Prop { get; set; }
}
```

**当前输出**：
```javascript
let <Prop>k__BackingField;
let <Prop>k__BackingField;  // 重复！
export function get_Prop() { ... }
export function set_Prop(value) { ... }
```

**期望输出**：
```javascript
let <Prop>k__BackingField;
export function get_Prop() { ... }
export function set_Prop(value) { ... }
```

**修复位置**：`ConvertModuleProperty` 方法 (L348-383)

**修复方案**：
- [ ] 在处理属性前先检查 BackingField 是否已声明
- [ ] 使用 HashSet 或字典跟踪已声明的 BackingField
- [ ] 统一 get/set 共享同一个 BackingField 声明

---

### 1.2 字段初始化器缺失

**问题描述**：
只处理了常量值初始化（`HasConstantValue`），非 const 字段的动态初始化表达式被忽略。

**复现代码**：
```csharp
public static class TestClass
{
    public static int Field = ComputeValue();
    public static List<int> Items = [1, 2, 3];
}
```

**当前输出**：
```javascript
export let Field;        // 初始化表达式丢失！
export let Items;        // 初始化表达式丢失！
```

**期望输出**：
```javascript
export let Field = ComputeValue();
export let Items = [1, 2, 3];
```

**修复位置**：`ConvertVariableField` 方法 (L74-88)

**修复方案**：
- [ ] 添加对非 const 字段初始化表达式的处理
- [ ] 使用 SemanticWalker 转换初始化表达式 IOperation
- [ ] 处理字段初始化顺序依赖

---

### 1.3 BackingField 名称硬编码

**问题描述**：
BackingField 名称格式 `<Name>k__BackingField` 是 C# 编译器实现细节，不同编译器/版本可能不同。

**当前代码**：
```csharp
var backName = $"<{symbol.Name}>k__BackingField";
```

**修复位置**：L153, L263, L320, L352

**修复方案**：
- [ ] 使用 `IFieldSymbol.IsImplicitlyDeclared` 查找隐式声明的字段
- [ ] 通过 `ContainingType.GetMembers()` 查找关联的 BackingField
- [ ] 添加降级策略：找不到时生成约定名称

---

## 二、中等问题 (P1)

### 2.1 静态构造函数处理缺失

**问题描述**：
静态构造函数被跳过，没有生成对应的初始化代码。

**复现代码**：
```csharp
public static class TestClass
{
    public static int Value;
    static TestClass()
    {
        Value = 100;
    }
}
```

**期望行为**：
生成 IIFE 或模块初始化块执行静态构造函数逻辑。

**修复位置**：`Convert` 方法 (L35-72)

**修复方案**：
- [ ] 在 switch 中添加 `MethodKind.StaticConstructor` 处理
- [ ] 将静态构造函数代码合并到模块初始化
- [ ] 处理多个静态构造函数的执行顺序

---

### 2.2 属性初始化器未处理

**问题描述**：
属性初始化器（如 `int P { get; set; } = 42;`）的值没有设置到 BackingField。

**当前代码**：
```csharp
// 处理属性初始化器，如 int P { get; set; } = 42;
// 属性初始化器 是只有自动属性或field关键字实现的属性才有
// 会在BackingField的默认值上处理
```

**修复位置**：`ConvertModuleProperty` 方法 (L348-383)

**修复方案**：
- [ ] 从 `IPropertySymbol` 获取初始化表达式
- [ ] 将初始化值设置到 BackingField 声明中
- [ ] 处理复杂初始化表达式

---

### 2.3 导入声明未实现

**问题描述**：
`_imports` 列表从未被填充，白名单 `Op.Import` 机制无法工作。

**当前代码**：
```csharp
private readonly List<ImportDeclaration> _imports = [];
// ... 从未填充
var statements = NodeList.From(_imports.Concat(members));
```

**修复位置**：
- `AstConverter.cs` L29
- `SenseArgument.cs` 的导入收集机制

**修复方案**：
- [ ] 从 `SenseArgument` 获取收集的导入规范
- [ ] 生成 `ImportDeclaration` 并添加到 `_imports`
- [ ] 合并相同模块路径的导入

---

### 2.4 空值处理逻辑矛盾

**问题描述**：
`CreateEqualsValueClauseSyntaxLiteral` 方法中空值检查逻辑矛盾。

**当前代码**：
```csharp
if (value is null)
    throw new NotSupportedException($"Cannot convert null to literal.");
// ...
else if (type == SpecialType.None)
    return new NullLiteral("null");
```

**修复位置**：L491-527

**修复方案**：
- [ ] 明确区分 `null` 字面量和缺失值
- [ ] 允许 `null` 作为合法的字段初始值
- [ ] 统一空值处理策略

---

### 2.5 嵌套类成员处理不完整

**问题描述**：
`ConvertMemberClass` 只处理了字段、属性和普通方法，其他成员会抛出异常。

**当前代码**：
```csharp
case IMethodSymbol func when func.MethodKind == MethodKind.Ordinary:
    nodes.Add(ConvertMemberMethod(func));
    break;
default:
    throw new NotSupportedException();  // 信息不足
```

**修复位置**：`ConvertMemberClass` 方法 (L385-415)

**修复方案**：
- [ ] 添加嵌套枚举支持
- [ ] 添加有意义的错误信息
- [ ] 或静默跳过不支持的成员（如接口、委托）

---

## 三、改进建议 (P2)

### 3.1 异常信息规范化

**问题**：异常信息中英文混用，有拼写错误，信息不够详细。

**示例**：
```csharp
throw new NotSupportedException($"Jazor cannot suport {symbol.Name}.");  // "suport" 拼写错误
throw new NotSupportedException($"Jazor 不支持转换方法 {symbol.Name}...");
```

**修复方案**：
- [ ] 统一使用中文或英文错误信息
- [ ] 修正拼写错误
- [ ] 添加更多上下文（如操作类型、位置）

---

### 3.2 重复代码提取

**问题**：`ConvertMemberMethod` 和 `ConvertModuleMethod` 有大量重复代码。

**涉及代码**：
- 查找 IOperation（L125-139 vs L220-250）
- 生成自动属性 BackingField（L151-180 vs L260-281）
- 参数处理（L185-194 vs L286-295）

**修复方案**：
- [ ] 提取 `GetOperationFromMethod` 方法
- [ ] 提取 `GenerateAutoPropertyBody` 方法
- [ ] 提取 `ConvertParameters` 方法

---

### 3.3 枚举值精度问题

**问题**：`long` 类型枚举值转换可能丢失精度。

**当前代码**：
```csharp
var value = System.Convert.ToDouble(kv.Value);
```

**修复位置**：L444

**修复方案**：
- [ ] 检查枚举基础类型
- [ ] 对于 `long`/`ulong` 使用 BigInt 字面量
- [ ] 或在 JavaScript 中使用字符串表示

---

### 3.4 访问级别扩展

**问题**：`protected`、`protected internal` 等访问级别未明确处理。

**当前代码**：
```csharp
private bool ShouldBePrivate(Accessibility accessibility)
    => accessibility != Accessibility.Public && accessibility != Accessibility.Internal;
```

**修复方案**：
- [ ] 添加对 `protected` 成员的处理策略
- [ ] 考虑是否导出 `protected internal` 成员
- [ ] 文档化访问级别映射规则

---

## 四、测试补充

### 4.1 缺失的测试场景

| 测试场景 | 优先级 | 状态 |
|---------|-------|------|
| 动态字段初始化器 | P0 | ❌ 缺失 |
| 属性初始化器 | P0 | ❌ 缺失 |
| 静态构造函数效果验证 | P1 | ❌ 缺失 |
| 导入声明生成 | P1 | ❌ 缺失 |
| 复杂初始化表达式 | P1 | ❌ 缺失 |
| 错误路径测试 | P2 | ❌ 缺失 |
| 嵌套类边界情况 | P2 | ❌ 缺失 |

### 4.2 建议添加的测试用例

```csharp
// 动态字段初始化
[TestMethod]
public void Convert_ClassWithDynamicFieldInitializer_GeneratesCorrectly() { }

// 属性初始化器
[TestMethod]
public void Convert_ClassWithPropertyInitializer_SetsBackingFieldValue() { }

// 静态构造函数
[TestMethod]
public void Convert_ClassWithStaticConstructor_GeneratesInitCode() { }

// 导入声明
[TestMethod]
public void Convert_ClassWithWhiteListImport_GeneratesImportDeclaration() { }
```

---

## 五、执行计划

### 阶段一：修复严重问题 (P0)

| 任务 | 预计工作量 | 依赖 |
|-----|----------|------|
| 修复 BackingField 重复声明 | 2h | 无 |
| 实现字段初始化器 | 4h | SemanticWalker |
| 重构 BackingField 查找 | 2h | 无 |

### 阶段二：修复中等问题 (P1)

| 任务 | 预计工作量 | 依赖 |
|-----|----------|------|
| 实现静态构造函数处理 | 4h | 阶段一 |
| 实现属性初始化器 | 3h | 阶段一 |
| 实现导入声明生成 | 3h | SenseArgument |
| 修复空值处理逻辑 | 1h | 无 |
| 完善嵌套类成员处理 | 2h | 无 |

### 阶段三：改进与测试 (P2)

| 任务 | 预计工作量 | 依赖 |
|-----|----------|------|
| 规范化异常信息 | 1h | 无 |
| 提取重复代码 | 3h | 阶段二 |
| 修复枚举精度问题 | 1h | 无 |
| 补充测试用例 | 4h | 阶段二 |

---

## 六、变更记录

| 日期 | 版本 | 变更内容 |
|-----|-----|---------|
| 2026-03-07 | v1.0 | 初始审查报告 |

