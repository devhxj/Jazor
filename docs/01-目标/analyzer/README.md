# 静态代码分析器

> 对应源码：`src/Jazor.Analyzer/`

## 为什么需要

Jazor 的 C# 到 JavaScript 转换有明确的边界——只支持白名单中的类型和成员。如果不在编译时拦截非法调用，错误会在运行时才暴露（生成的 JS 代码缺失或语义错误），调试成本极高。Analyzer 在编写代码的瞬间就给出诊断提示，把问题推到最早可修复的时间点。

## 解决什么问题

1. **白名单守门**：编译时验证用户代码只使用了 `[WhiteList]` 标注的类型和成员
2. **即时反馈**：在 IDE 中实时显示诊断（波浪线 + 错误列表），无需等到运行
3. **迁移引导**：对旧指令（如 `@import`）发出迁移诊断（JAZORVUE020），引导用户使用新语法

## 大致实现思路

Analyzer 基于 Roslyn DiagnosticAnalyzer 框架：

1. **注册语法/语义分析**：监听 `CompilationStartAnalysisContext`，在编译初始化阶段收集白名单
2. **类型检查**：对每个 `IOperation` 检查其类型符号是否在 `WhiteList.Types` 中
3. **成员检查**：对方法调用、属性访问等检查成员全名是否在 `WhiteList.Members` 中
4. **诊断输出**：不合规的用法生成 `DiagnosticDescriptor`，附带修复建议

### 诊断规则编号

- JAZORVUE020：检测 `@import` 旧指令，建议迁移到新语法

## 与白名单系统的关系

```
Jazor.CLR（标注 [WhiteList]）
       ↓ Generator 生成
WhiteList.cs（HashSet<string>）
       ↓ Analyzer 引用
编译时验证（用户代码合规检查）
```

Analyzer 是白名单机制的"第一道防线"，与 Jazor.CLR（声明层）和 Generator（生成层）构成三层协作。
