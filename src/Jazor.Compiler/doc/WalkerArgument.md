# WalkerArgument 分析文档

## 1. 文件概述

**文件路径**: `WalkerArgument.cs`

**职责**: 转换上下文参数，在遍历过程中传递上下文信息。

## 2. 核心设计思路

### 2.1 数据结构

```csharp
public sealed class WalkerArgument
{
    // 导入声明规范（按模块路径分组）
    private readonly Dictionary<string, List<ImportDeclarationSpecifier>> _specifiers;

    // 变量声明（使用深度+名称作为键）
    private readonly Dictionary<string, VariableDeclarator> _declarators;

    // 上下文表达式（用于模式匹配等场景）
    public (NodeType Type, Expression Target)? Context { get; }
}
```

### 2.2 变量声明机制

使用 `{depth}:{name}` 作为键，支持嵌套作用域：

```csharp
public void AddVarDeclarator(VariableDeclarator declarator, int depth)
{
    var name = declarator.Id is Identifier identifier
        ? identifier.Name
        : declarator.Id.ToECMAScript();
    var key = $"{depth}:{name}";
    if (!_declarators.ContainsKey(key))
        _declarators.Add(key, declarator);
}
```

### 2.3 导入管理

按模块路径分组存储导入声明：

```csharp
public void MergeImportSpecifier(string modulePath, ImportDeclarationSpecifier specifier)
{
    if (_specifiers.TryGetValue(modulePath, out var list))
        list.Add(specifier);
    else
        _specifiers.Add(modulePath, [specifier]);
}
```

### 2.4 不可变设计

`With` 方法创建新实例，复用变量声明列表：

```csharp
public WalkerArgument With(NodeType type, Expression target)
    => new((type, target), _specifiers, _declarators);
```

## 3. 使用场景

### 3.1 变量声明收集

```csharp
// 在 Visit 方法中添加变量声明
argument.AddVarDeclarator(declarator, _recursionDepth);

// 在块结束时刷新变量声明
var declarators = argument.FlushVarDeclarator();
var declaration = new VariableDeclaration(VariableDeclarationKind.Let, declarators);
```

### 3.2 导入声明收集

```csharp
// 添加导入声明
context.MergeImportSpecifier("System/BooleanModule.js", new ImportSpecifier(id));

// 生成导入声明
// TODO: 目前导入声明未实际使用
```

### 3.3 上下文表达式

```csharp
// 在模式匹配中传递输入表达式
var newContext = argument.With(NodeType.Identifier, inputExpr);
```

## 4. 已知缺陷

### 4.1 高优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **导入声明未实际生成** | `_specifiers` 收集但未使用 | 在 AstConverter 中生成 ImportDeclaration |
| **FlushVarDeclarator 清空后丢失信息** | 无法追踪已声明的变量 | 添加历史记录或声明检查方法 |

### 4.2 中优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **Context 类型不明确** | NodeType 和 Expression 混用 | 明确 Context 的用途和类型 |
| **缺少导入声明刷新方法** | 无法获取导入声明 | 添加 `FlushImportSpecifiers` 方法 |

### 4.3 低优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **ToECMAScript 依赖外部扩展** | 耦合度高 | 内联实现或提供工具方法 |
| **深度参数传递不一致** | 可能导致命名冲突 | 统一深度传递机制 |

## 5. 设计权衡

### 5.1 变量声明位置

**当前设计**: 变量声明分散在各个 statement 之间

**权衡**:
- 优点：实现简单，符合 C# 变量声明语义
- 缺点：生成代码可读性差，与 JavaScript 最佳实践不一致

**改进方向**: 收集声明并集中在块开头

### 5.2 不可变性

**当前设计**: With 方法创建新实例

**权衡**:
- 优点：线程安全，避免副作用
- 缺点：可能增加内存分配

**改进方向**: 保持当前设计，优化内存分配

## 6. 需完善内容

### 6.1 功能完善

- [ ] 实现导入声明生成
- [ ] 添加 `FlushImportSpecifiers` 方法
- [ ] 添加变量声明检查方法 (`HasVariable`)
- [ ] 明确 Context 属性的用途

### 6.2 代码质量

- [ ] 添加 XML 文档注释
- [ ] 添加单元测试
- [ ] 优化内存分配

### 6.3 API 改进

- [ ] 提供更友好的 API
- [ ] 添加 Builder 模式支持

## 7. 测试状态

**当前状态**: 缺少专门的 WalkerArgument 测试

**建议添加的测试**:
- 变量声明添加/刷新测试
- 导入声明添加测试
- With 方法测试
- 深度键生成测试

## 8. 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [AstConverter.md](./AstConverter.md)
- [rule.md](../rule.md)

---

**最后更新**: 2026-03-03
