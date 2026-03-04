# AstConverter 分析文档

## 1. 文件概述

**文件路径**: `AstConverter.cs`

**职责**: 类级别转换器，将 C# 类转换为 ES6 Module。

## 2. 核心设计思路

### 2.1 转换流程

```
C# 类 (INamedTypeSymbol)
        │
        ▼
    遍历成员
        │
        ├── IFieldSymbol → VariableDeclaration
        ├── IPropertySymbol → 函数声明 (get/set)
        ├── IMethodSymbol → FunctionDeclaration
        ├── INamedTypeSymbol(Class) → ClassDeclaration
        └── INamedTypeSymbol(Enum) → VariableDeclaration
        │
        ▼
    ES6 Module
```

### 2.2 导出规则

- `public` 和 `internal` 成员 → `export`
- `private` 和 `protected` 成员 → 不导出

### 2.3 扁平化原则

静态类的成员直接转换为模块级变量/函数，而非静态类转换为 ES6 class。

## 3. 代码结构分析

### 3.1 核心方法

| 方法 | 职责 |
|------|------|
| `Convert()` | 主入口，遍历类成员并生成 Module |
| `ConvertModuleField()` | 静态字段 → 变量声明 |
| `ConvertModuleProperty()` | 静态属性 → get/set 函数 |
| `ConvertModuleMethod()` | 静态方法 → 函数声明 |
| `ConvertModuleClass()` | 嵌套类 → class 声明 |
| `ConvertModuleEnum()` | 枚举 → const 对象 |

### 3.2 依赖关系

```
AstConverter
    │
    ├── SemanticWalker (操作级别转换)
    │
    └── Acornima.Ast (ESTree 节点)
```

## 4. 已知缺陷

### 4.1 高优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **不支持嵌套类扁平化** | 嵌套类抛出 `NotSupportedException` | 实现递归扁平化，将嵌套类成员提升到模块级别 |
| **不支持泛型类** | 泛型类无法转换 | 添加泛型参数处理逻辑 |
| **不支持继承** | 继承的成员未处理 | 添加基类成员遍历逻辑 |

### 4.2 中优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **导入声明未实际使用** | `_imports` 列表未填充 | 在 SemanticWalker 中收集导入并传递给 AstConverter |
| **异步方法未标记 async** | 生成的函数缺少 async 关键字 | 检查 `IMethodSymbol.IsAsync` 并设置 `async: true` |
| **构造函数不支持** | 带构造函数的类转换失败 | 添加构造函数处理逻辑 |

### 4.3 低优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **索引器不支持** | 索引器成员被忽略 | 实现索引器到方法/属性的映射 |
| **运算符重载不支持** | 运算符成员被忽略 | 映射到 JavaScript 运算符或 Symbol 方法 |
| **事件不支持** | 事件成员被忽略 | 需要设计事件模型映射方案 |

## 5. 需完善内容

### 5.1 功能完善

- [ ] 实现嵌套类扁平化处理
- [ ] 添加泛型类支持
- [ ] 处理继承的成员
- [ ] 支持构造函数
- [ ] 支持异步方法标记

### 5.2 代码质量

- [ ] 添加更多 XML 文档注释
- [ ] 添加单元测试
- [ ] 优化错误消息格式

### 5.3 设计改进

- [ ] 将导入收集逻辑与 SemanticWalker 集成
- [ ] 支持自定义命名策略
- [ ] 添加源码位置映射（Source Map 支持）

## 6. 测试覆盖

**当前状态**: 缺少专门的 AstConverter 测试

**建议添加的测试**:
- 模块字段转换测试
- 模块属性转换测试
- 模块方法转换测试
- 嵌套类转换测试
- 枚举转换测试
- 可见性导出测试

## 7. 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [WalkerArgument.md](./WalkerArgument.md)
- [rule.md](../rule.md)

---

**最后更新**: 2026-03-03
