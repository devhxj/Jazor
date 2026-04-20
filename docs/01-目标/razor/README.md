# Razor 基础层

> 对应源码：`src/Jazor.Razor/`、`src/Jazor.Compiler.Razor/`

## 为什么需要

RazorVue 和 Jolt 都需要在 Razor 组件模型上构建。但 ASP.NET Core 的 Razor 组件系统（Blazor）与 Jazor 的编译目标（JavaScript/Vue）有根本差异——Blazor 生成 .NET 运行时代码，Jazor 生成 JavaScript。需要一层薄薄的适配层把 Razor 语法树桥接到 Jazor 编译管线。

## 解决什么问题

1. **Razor 组件接入**：为 Razor 组件提供 Jazor 特有的基类（`JazorComponent`），替代 Blazor 的 `ComponentBase` 语义
2. **语义信息提取**：从 Razor 编译结果中提取类型信息和语义快照，供下游编译管线使用
3. **编译器扩展点**：定义 `IRazorSemanticFrontend` 接口，让 Jazor.Compiler 能处理 Razor 语法

## 大致实现思路

### Jazor.Razor

提供 Razor 语法到 Jazor 编译管线的薄基底：

- 定义 Razor 组件在 Jazor 生态系统中的基本抽象
- 为 RazorVue 提供 Razor 语法解析和处理的共享基础设施

### Jazor.Compiler.Razor

编译器侧的 Razor 集成，核心只有两个类：

1. **JazorComponent**：继承 `ComponentBase` 的抽象基类，作为 Razor 管线的入口点
2. **RazorComponentSemanticFrontend**：实现 `IRazorSemanticFrontend`，从 Razor 编译结果中：
   - 分类入口类型
   - 创建 `RazorVueSemanticSnapshot` 对象
   - 将 ASP.NET Core Razor 组件模型桥接到 Jazor 分析管线

```
Razor 源码 (.razor)
     ↓ ASP.NET Core Razor 编译器
Razor 编译结果（C# 语义）
     ↓ RazorComponentSemanticFrontend
RazorVueSemanticSnapshot
     ↓ Jazor.Compiler
JavaScript / Vue SFC 输出
```
