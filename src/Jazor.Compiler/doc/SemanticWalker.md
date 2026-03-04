# SemanticWalker 分析文档

## 1. 文件概述

**文件路径**: `core/SemanticWalker.cs` (主文件) 及其分文件

**职责**: 操作级别转换器，将 Roslyn IOperation 转换为 Acornima ESTree 节点。

## 2. 核心设计思路

### 2.1 访问者模式

SemanticWalker 继承自 `OperationVisitor<WalkerArgument, Node?>`，使用访问者模式遍历 IOperation 树。

```
IOperation 树
      │
      ▼
Visit(operation, argument)
      │
      ├── 递归深度控制
      ├── 栈溢出检查
      └── operation.Accept(this, argument)
            │
            ▼
      具体 Visit 方法
            │
            ▼
      ESTree Node
```

### 2.2 核心原则

1. **语义等价性**: 确保 C# 和 JavaScript 语义完全等价
2. **直接 AST 构造**: 对于已知的转换结构，直接构造 AST 节点
3. **空值安全处理**: 构造 AST 前检查 null
4. **编译时优化**: 利用编译时类型信息
5. **方法复用**: 优先复用现有 Visit 方法

**关于 Parser 的使用**：对于白名单中的内联代码模板（`Op.Inline`），使用 Parser 解析是**必要的设计选择**，因为模板可能包含任意复杂的 JavaScript 表达式。详见 [SemanticWalker.WhiteList.md](./SemanticWalker.WhiteList.md#43-使用-parser-解析内联代码的设计决策)。

### 2.3 分文件组织

| 文件 | 职责 | 行数 |
|------|------|------|
| `SemanticWalker.cs` | 主入口、类型映射、Translate 方法族 | ~470 |
| `SemanticWalker.cs.Pattern.cs` | 模式匹配 | ~800 |
| `SemanticWalker.cs.Reference.cs` | 字段/属性/方法引用 | ~585 |
| `SemanticWalker.cs.Loop.cs` | 循环语句 | ~200 |
| `SemanticWalker.cs.Switch.cs` | Switch 语句/表达式 | ~300 |
| `SemanticWalker.cs.String.cs` | 字符串插值 | ~100 |
| `SemanticWalker.cs.TryCatch.cs` | 异常处理 | ~150 |
| `SemanticWalker.cs.Creation.cs` | 对象/数组创建 | ~200 |
| `SemanticWalker.cs.Tuple.cs` | 元组和解构 | ~150 |
| `SemanticWalker.cs.Declaration.cs` | 变量声明 | ~100 |
| `SemanticWalker.cs.Ordinary.cs` | 二元/一元运算 | ~200 |
| `SemanticWalker.cs.Invalid.cs` | IInvalidOperation 处理 | ~200 |
| `SemanticWalker.cs.NotSupport.cs` | 不支持的操作 | ~100 |
| `SemanticWalker.cs.WhiteList.cs` | 白名单查询 | ~50 |
| `SemanticWalker.cs.Generate.cs` | 白名单生成 | 自动生成 |

## 3. 核心组件分析

### 3.1 Translate 方法族

提供类型安全的转换访问器：

| 方法 | 用途 | 失败行为 |
|------|------|---------|
| `Translate<T>(IOperation, WalkerArgument)` | 强制转换 | 抛出异常 |
| `Translate<T>(IOperation?, WalkerArgument, T?)` | 可选转换 | 返回默认值 |
| `Translate<T>(ICollection<T>, IOperation?, WalkerArgument)` | 集合转换 | 跳过失败项 |
| `TranslateExpression(IOperation, WalkerArgument)` | 表达式转换 | 抛出异常 |

### 3.2 GetMapperType 方法

类型映射的核心逻辑：

```
ITypeSymbol
    │
    ├── 元组/匿名类型 → Object
    ├── SpecialType 检查 → 基础类型映射
    ├── TypeKind 检查 → Array/Enum
    ├── 显示名称检查 → 特殊类型
    ├── 白名单别名检查 → 自定义映射
    └── Class/Struct → Class
```

### 3.3 GetUniqueName 方法

生成稳定的唯一变量名：

- 使用 SHA256 哈希确保稳定性
- 测试模式返回固定名称 `v$n`
- 用于临时变量、switch 表达式输入变量等

## 4. 已知缺陷

### 4.1 高优先级缺陷

| 缺陷 | 位置 | 影响 | 建议修复方案 |
|------|------|------|-------------|
| **模式匹配依赖向上遍历** | `Pattern.cs` | 可测试性差 | 通过 WalkerArgument 传入上下文表达式 |
| **变量声明位置分散** | `Declaration.cs` | 生成代码可读性差 | 收集声明并集中在块开头 |

### 4.2 中优先级缺陷

| 缺陷 | 位置 | 影响 | 建议修复方案 |
|------|------|------|-------------|
| **GetMapperType 类型检查不完整** | `SemanticWalker.cs` | 某些类型映射错误 | 添加更多类型检查分支 |
| **白名单查询性能** | `GetWhiteListExpression` | 每次调用创建 List | 使用缓存或优化参数传递 |
| **错误消息不够详细** | 多处 | 调试困难 | 添加更多上下文信息 |

### 4.3 低优先级缺陷

| 缺陷 | 位置 | 影响 | 建议修复方案 |
|------|------|------|-------------|
| **递归深度硬编码为 20** | `EnsureSufficientExecutionStack` | 可能不够灵活 | 配置化或动态调整 |
| **测试模式缓存使用 List** | `GetUniqueName` | 性能一般 | 改用 Dictionary |

## 5. 设计权衡

### 5.1 当前设计权衡

| 设计决策 | 权衡 | 优点 | 缺点 |
|---------|------|------|------|
| **功能完整优先** | 生成代码非最优 | 功能完整 | 性能可优化 |
| **模式匹配向上遍历** | 可测试性差 | 实现简单 | 依赖完整操作树 |
| **分文件组织** | 文件数量多 | 职责清晰 | 跨文件重构困难 |
| **内联模板使用 Parser** | 需解析开销 | 灵活处理复杂表达式 | 仅限白名单场景 |

### 5.2 潜在改进方向

| 改进方向 | 优先级 | 风险 | 说明 |
|---------|--------|------|------|
| 通过 Argument 传入上下文 | P1 | 中 | 减少向上遍历开销 |
| 变量声明集中化 | P2 | 低 | 改善生成代码可读性 |
| 缓存优化 | P3 | 中 | 缓存常用 AST 节点 |

## 6. 需完善内容

### 6.1 功能完善

- [ ] 优化模式匹配上下文传递
- [ ] 添加更多类型映射支持
- [ ] 完善错误处理和消息

### 6.2 代码质量

- [ ] 统一 XML 文档注释格式
- [ ] 添加更多单元测试
- [ ] 重构过长方法

### 6.3 性能优化

- [ ] 添加 AST 节点缓存
- [ ] 优化字符串操作
- [ ] 减少临时对象分配

## 7. 测试状态

**当前状态**: 533 个测试全部通过

**测试覆盖**:
- ✅ 模式匹配
- ✅ 循环语句
- ✅ Switch 语句/表达式
- ✅ 字符串插值
- ✅ 异常处理
- ✅ 元组
- ✅ 创建表达式
- ✅ 引用操作
- ✅ 变量声明
- ✅ 普通运算
- ✅ 无效操作处理

**建议添加的测试**:
- 边界条件测试
- 性能测试
- 错误场景测试

## 8. 相关文档

- [WalkerArgument.md](./WalkerArgument.md)
- [AstConverter.md](./AstConverter.md)
- [Optimizer.md](./Optimizer.md)
- [rule.md](../rule.md)

---

**最后更新**: 2026-03-04
