# Jazor 项目开发规则文档

## 1. 项目概述

Jazor 是一个 C# 到 JavaScript 的编译器项目，主要目标是将 C# 代码转换为等价的 JavaScript 代码，支持跨语言的语义保持和 AST 转换。

### 1.1 项目结构

```
Jazor/
├── ECMAScript/                 # 核心 ECMAScript 实现
├── ECMAScript.CLR/            # CLR 运行时支持
├── ECMAScript.Analyzer/       # 静态代码分析器
├── ECMAScript.Compiler/       # C# 到 JavaScript 编译器
├── ECMAScript.Server/         # 编译服务器
├── ECMAScript.Test/           # 单元测试
├── ECMAScript.ComplierTest/   # 编译器测试
└── ECMAScript.WebIDL/         # Web IDL 绑定
```

## 2. AST 转换器开发规范

### 2.1 核心原则

#### 2.1.1 语义等价性原则
- **强制要求**：确保 C# 和 JavaScript 之间的语义完全等价，禁止任何形式的简化处理
- **实施策略**：一旦无法保证 1:1 语义等价，必须抛出 `OperationTransformationException`
- **验证方法**：每个转换操作都必须经过语义等价性验证

#### 2.1.2 直接 AST 构造原则
- **强制要求**：必须直接构造目标 AST 节点，禁止使用 Parser 进行解析
- **实施策略**：使用 Acornima ESTree 节点类型直接构造 JavaScript AST
- **性能考虑**：避免字符串解析开销，提高编译性能

#### 2.1.3 空值安全处理原则
- **强制要求**：构造 AST 节点时必须先检查输入值是否为 null
- **实施策略**：避免 NullReferenceException，提供适当的默认值或抛出明确异常
- **代码示例**：
```csharp
var operand = Visit(operation.Operation, operation) as Expression;
if (operand == null) return null;
```

### 2.2 转换规则分类

#### 2.2.1 支持的转换类型
1. **基础语法转换**
   - 变量声明：`var/int` → `let`
   - 赋值操作：`= += -= *= /= %=` → JavaScript 相同运算符
   - 二元运算：`+ - * / % == != < > <= >= && ||` → JavaScript 相同运算符
   - 一元运算：`+ - ! ~` → JavaScript 相同运算符

2. **控制流转换**
   - if/else 语句：直接映射
   - switch 语句：仅支持常量模式
   - switch 表达式：使用 IIFE 包装
   - 循环语句：for、foreach、while 的直接映射

3. **模式匹配转换**
   - 常量模式：`value is 42` → `value === 42`
   - 类型模式：`obj is string` → `typeof obj === "string"`
   - 属性模式：`obj is { Name: "John" }` → `obj.Name === "John"`
   - 丢弃模式：`_` → `true`（使用统一占位符）

#### 2.2.2 不支持的特性
- 事件系统（EventReference、RaiseEvent）
- 动态类型（dynamic 关键字相关操作）
- LINQ 查询表达式
- goto 语句（除 break/continue）
- 多维数组访问
- VB.NET 特有功能

### 2.3 AST 节点构造规范

#### 2.3.1 节点类型选择
- 使用 `LogicalExpression` 而非 `BinaryExpression` 表示逻辑操作
- 使用 `UpdateExpression` 处理 typeof 等一元操作
- `NullLiteral` 必须提供 raw 参数
- `BooleanLiteral` 第一个参数为 bool 值，第二个参数为 string 原始值

#### 2.3.2 字符串处理规范
- 所有字符串字面量需要适当的转义处理
- 使用 `StringLiteral` 构造器时必须提供原始值参数
- 插值字符串转换为连接表达式

### 2.4 占位符使用规范

根据项目规范记忆，在 AST 转换器中处理模式匹配等需要占位符的场景时：

#### 2.4.1 统一占位符规则
- **强制使用**：下划线 `"_"` 作为通用占位符
- **禁止使用**：任何具有具体语义的硬编码名称（如 'value'、'target'、'input' 等）
- **目标**：确保语义中立性和上下文无关性

#### 2.4.2 例外情况
- **仅在必须保留绑定的上下文中**：才使用有意义的变量名
- **验证标准**：只有当变量名对后续逻辑有实际意义时才使用

#### 2.4.3 代码示例
```csharp
// ✅ 正确：使用统一占位符
var discardPattern = new Identifier("_");

// ❌ 错误：使用具体语义名称
var valuePattern = new Identifier("value");
```

## 3. 异常处理策略

### 3.1 异常类型定义

#### 3.1.1 `OperationTransformationException`
- **用途**：当 C# 操作无法转换为等价的 JavaScript AST 时抛出
- **构造参数**：
  - `IOperation operation`：导致异常的操作
  - `string message`：详细错误信息
  - `Exception innerException`：（可选）内部异常

#### 3.1.2 `SymbolTransformationException`
- **用途**：当符号转换失败时抛出
- **构造参数**：
  - `ISymbol symbol`：导致异常的符号
  - `string message`：详细错误信息
  - `Exception innerException`：（可选）内部异常

### 3.2 异常处理规则

#### 3.2.1 强制抛出异常的场景
1. **语义不等价**：无法保证 1:1 语义映射时
2. **不支持的操作**：遇到明确不支持的 C# 特性
3. **编译器内部操作**：处理编译器专用操作时
4. **动态语义退化**：动态类型相关操作

#### 3.2.2 异常信息规范
- **必须包含**：具体的操作类型和原因
- **必须提供**：替代方案建议（如果有）
- **格式标准**：`"{操作类型} operations are not supported in JavaScript conversion: {具体原因}"`

## 4. 性能优化策略

### 4.1 编译时优化
- **利用强类型信息**：使用 C# 编译时类型信息避免运行时检测
- **递归深度控制**：防止栈溢出，使用 `EnsureSufficientExecutionStack`
- **AST 节点复用**：优先复用现有的 Visit 方法

### 4.2 运行时优化
- **最简 AST 生成**：生成最简洁的 JavaScript 代码
- **避免不必要的包装**：除非必要，避免复杂的 IIFE 包装
- **类型转换优化**：依赖编译时类型安全进行优化

## 5. 代码注释规范

### 5.1 类级别注释要求

根据 AST 转换器注释规范记忆，需基于历史实现流程和代码分析，系统性地总结整体转换规则，并作为核心类的详细注释：

#### 5.1.1 必须包含的内容
1. **功能范围**：明确支持的转换类型和范围
2. **核心原则**：语义等价性、直接 AST 构造等核心原则
3. **分类规则**：详细的转换规则分类和示例
4. **技术规范**：AST 节点构造的技术要求
5. **限制说明**：明确不支持的特性和原因
6. **性能策略**：优化策略和性能考虑

#### 5.1.2 注释格式要求
- 使用 XML 文档注释格式
- 每个主要部分使用 `<para><b>标题</b></para>` 格式
- 提供具体的代码示例和转换结果
- 保持注释与实际实现的同步更新

### 5.2 方法级别注释要求

#### 5.2.1 标准格式
```csharp
/// <summary>
/// 处理 {操作类型} 操作
/// C# 示例：
/// {C# 代码示例}
/// 转换结果：{JavaScript 结果} / {异常情况}
/// </summary>
/// <param name="operation">当前访问的operation</param>
/// <param name="argument">当前访问的operation的父operation</param>
/// <returns>Acornima的ESTree的Node</returns>
```

## 6. 测试和验证规范

### 6.1 单元测试要求
- **覆盖率要求**：每个转换方法必须有对应的单元测试
- **测试场景**：包括正常转换和异常情况
- **验证内容**：AST 结构正确性和语义等价性

### 6.2 集成测试要求
- **端到端测试**：完整的 C# 到 JavaScript 转换流程
- **性能测试**：大型代码库的转换性能验证
- **兼容性测试**：不同版本 C# 语法的支持验证

## 7. 版本控制和发布规范

### 7.1 版本号管理
- 遵循语义化版本控制（Semantic Versioning）
- 主版本号：不兼容的 API 更改
- 次版本号：向后兼容的功能性新增
- 修订版本号：向后兼容的问题修正

### 7.2 发布流程
1. **代码审查**：所有变更必须经过代码审查
2. **测试验证**：完成所有单元测试和集成测试
3. **文档更新**：同步更新相关文档和注释
4. **性能基准**：验证性能指标未出现回归

## 8. 团队协作规范

### 8.1 开发流程
- 遵循标准化的 TODO 实现与缺陷修复流程
- 使用 Mermaid 流程图规划复杂功能实现
- 限制修复迭代次数，防止无限循环

### 8.2 知识管理
- 及时更新项目记忆和文档
- 分享最佳实践和经验教训
- 维护技术决策的可追溯性

## 9. 附录

### 9.1 相关文件清单
- `AstOperationWalker.cs`：核心 AST 转换器实现
- `AstTransformationException.cs`：异常类型定义
- `AcornimaOperationWalker.cs`：Acornima 特定实现

### 9.2 技术依赖
- Microsoft.CodeAnalysis（Roslyn）
- Acornima（JavaScript AST 库）
- .NET 运行时环境

### 9.3 更新历史
- 初始版本：基于当前代码库和记忆规则整合生成
- 后续更新：随项目演进持续维护

---

**文档维护者**：开发团队  
**最后更新**：2025-09-29  
**文档版本**：v1.0  