# 公共类型与工具

> 对应源码：`src/Jazor.Common/`、`src/Jazor.Name/`

## 为什么需要

编译器、分析器、代码生成器、CLR 映射等多个项目之间需要共享契约和工具函数。如果每个项目各自定义，会导致类型不兼容和重复代码。公共模块确保所有生产者使用统一的接口和命名规则。

## 解决什么问题

### Jazor.Common

跨生产者侧（Generator、Analyzer、Compiler、CLR 映射）的共享契约：

- 统一的特性定义（`[ECMAScriptModule]`、`[WhiteList]`、`[ECMAScriptLiteral]`）
- 跨项目共享的类型标记和常量
- 确保分析器、生成器、编译器使用同一套类型定义

### Jazor.Name

符号格式化和哈希命名工具：

- 提供统一的标识符格式化规则（C# 名称 ↔ JavaScript 名称转换）
- 基于内容哈希的唯一命名生成（用于 SourceMap、临时变量等场景）
- 确保生成的 JavaScript 标识符不会与用户代码冲突

## 与其他项目的关系

```
Jazor.Common（契约）
    ├── Jazor.Analyzer（引用特性定义）
    ├── Jazor.Compiler.Generator（引用特性定义）
    ├── Jazor.Compiler（引用特性定义）
    └── Jazor.CLR（标注特性）

Jazor.Name（命名工具）
    ├── Jazor.Compiler（变量命名、SourceMap 生成）
    ├── Jazor.CLR.Generator（模块命名）
    └── Jazor.Emit（输出文件命名）
```
