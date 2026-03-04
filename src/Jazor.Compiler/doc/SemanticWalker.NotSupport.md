# SemanticWalker.cs.NotSupport.cs 分析文档

## 1. 文件概述

**文件路径**: `core/SemanticWalker.cs.NotSupport.cs`

**职责**: 定义所有不支持转换为 JavaScript 的 C# 特性，并抛出明确的异常。

**代码行数**: ~525 行

## 2. 不支持特性分类

### 2.1 事件系统

| 操作 | 原因 |
|------|------|
| `IRaiseEventOperation` | JavaScript 事件模型与 C# 多播事件模型根本不同 |
| `IEventReferenceOperation` | C# 事件支持多播委托、线程安全访问、弱引用 |
| `IEventAssignmentOperation` | JavaScript 事件是简单的回调函数模式 |

### 2.2 动态类型

| 操作 | 原因 |
|------|------|
| `IDynamicObjectCreationOperation` | C# 动态绑定语义与 JavaScript 静态分派模型不可通约 |
| `IDynamicMemberReferenceOperation` | 运行时解析、重载决策、动态分派 |
| `IDynamicInvocationOperation` | 无法保证语义等价 |
| `IDynamicIndexerAccessOperation` | 需要编译时确定类型信息 |

### 2.3 LINQ

| 操作 | 原因 |
|------|------|
| `ITranslatedQueryOperation` | LINQ 提供延迟执行、表达式树，JavaScript 没有对应构造 |

### 2.4 类型和内存操作

| 操作 | 原因 |
|------|------|
| `ITypeOfOperation` | C# typeof 获取类型信息 vs JavaScript typeof 获取值类型 |
| `ISizeOfOperation` | JavaScript 是安全语言，没有直接的内存大小概念 |
| `IAddressOfOperation` | JavaScript 不支持指针操作 |

### 2.5 资源管理

| 操作 | 原因 |
|------|------|
| `IUsingOperation` | JavaScript 没有内置的资源管理机制 |
| `IUsingDeclarationOperation` | 没有确定性析构 |

### 2.6 线程同步

| 操作 | 原因 |
|------|------|
| `ILockOperation` | JavaScript 是单线程语言，没有锁机制 |

### 2.7 编译器内部操作

| 操作 | 原因 |
|------|------|
| `IStopOperation` | 编译器内部标记 |
| `IEndOperation` | 编译器内部标记 |
| `IMethodBodyOperation` | 编译器内部操作 |
| `IConstructorBodyOperation` | 编译器内部操作 |
| `ICaughtExceptionOperation` | 编译器内部操作 |
| `IStaticLocalInitializationSemaphoreOperation` | 编译器内部操作 |
| `IFlowAnonymousFunctionOperation` | 编译器内部操作 |
| `IFlowCaptureOperation` | 编译器内部操作 |
| `IFlowCaptureReferenceOperation` | 编译器内部操作 |

### 2.8 VB.NET 特有功能

| 操作 | 原因 |
|------|------|
| `IForToLoopOperation` | VB.NET 特有 |
| `IRangeCaseClauseOperation` | VB.NET 特有 |
| `IRelationalCaseClauseOperation` | VB.NET 特有 |
| `IReDimOperation` | VB.NET 特有 |
| `IReDimClauseOperation` | VB.NET 特有 |

### 2.9 其他不支持

| 操作 | 原因 |
|------|------|
| `IRangeOperation` (独立) | C# Range 必须在索引器中消费 |
| `IInterpolatedStringHandlerCreationOperation` | 插值字符串处理器框架无法重现 |
| `IInterpolatedStringAppendOperation` | 依赖于处理器上下文 |
| `IInterpolatedStringHandlerArgumentPlaceholderOperation` | 编译器内部操作 |
| `IFunctionPointerInvocationOperation` | JavaScript 不支持函数指针 |
| `IUtf8StringOperation` | UTF-8 字节与 UTF-16 字符串不兼容 |
| `IInlineArrayAccessOperation` | JavaScript 没有内联数组概念 |

## 3. 异常消息设计

每个不支持操作都提供：
1. 明确的不支持声明
2. 具体的原因说明
3. 替代方案建议（如果有）

### 3.1 示例消息

```csharp
// 事件操作
"Event references are not supported in JavaScript conversion."

// 动态操作
"Dynamic object creation is not supported in JavaScript conversion."

// 资源管理
"Using statements are not supported in JavaScript conversion."

// 附带替代方案
// "Alternative: In JavaScript, use try-finally block to manage resources manually."
```

## 4. 替代方案指南

### 4.1 事件替代

```javascript
// C#: event += handler
// JavaScript: addEventListener('event', handler)
// 或自定义事件发射器模式
```

### 4.2 动态类型替代

```javascript
// C#: dynamic obj
// JavaScript: 使用普通对象 {} 或 Map
```

### 4.3 LINQ 替代

```javascript
// C#: collection.Where(x => x > 0).Select(x => x * 2)
// JavaScript: collection.filter(x => x > 0).map(x => x * 2)
// 或使用 lodash
```

### 4.4 资源管理替代

```javascript
// C#: using (var resource = ...)
// JavaScript:
try {
    // 使用资源
} finally {
    resource.close();
}
```

## 5. 设计原则

1. **快速失败**: 遇到不支持特性立即抛出异常
2. **明确原因**: 告知用户为什么不支持
3. **提供替代方案**: 帮助用户找到解决方法

## 6. 测试覆盖

**当前状态**: 有测试验证异常抛出

**测试场景**：
- ✅ 事件操作抛出异常
- ✅ 动态操作抛出异常
- ✅ LINQ 抛出异常
- ✅ 编译器内部操作抛出异常

## 7. 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [rule.md](../rule.md) - 详细的不支持特性列表

---

**最后更新**: 2026-03-03
