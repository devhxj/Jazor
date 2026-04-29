# Razor 基础层

> 对应源码：`src/Jazor.Razor/`、`src/Jazor.Compiler.Razor/`

## 为什么需要

Jazor 需要接入 Razor 语法，但又不能把 Razor 基础层和更高层的 RazorVue 语义、Jolt 宿主逻辑或编译器入口混在一起。`Razor` 这一层的职责，就是把“Razor authoring 入口”与“具体怎样提取语义、怎样生成产物”拆开。

## 解决什么问题

1. **最小 Razor 标记**：提供 Razor authoring 的最低层契约，而不是直接耦合到某个具体产品线基类。
2. **编译器侧前端桥接**：把 Razor 编译结果转成 `Jazor.Compiler` / RazorVue 能消费的语义快照。
3. **避免边界塌陷**：不把 Razor 基础层和 RazorVue/Jolt 的更高层实现揉在一起。

## 当前分工

### Jazor.Razor

- 只保留最薄的 Razor 标记层。
- 当前核心是 `IJazorComponent` 最小接口。

### Jazor.Compiler.Razor

- 提供编译器侧的 `JazorComponent` 基类。
- 提供 `RazorComponentSemanticFrontend`，把 Razor 编译结果桥接到 `IRazorSemanticFrontend`。

## 当前链路

```text
Razor 源码
     ↓ ASP.NET Core Razor 编译器
Razor 编译结果（C# 语义）
     ↓ RazorComponentSemanticFrontend
RazorVueCompilationContext / SemanticSnapshot
     ↓ 下游消费（Compiler / RazorVue / Jolt）
```

这里的输出边界不是固定的 `.vue SFC`。具体产物形态取决于下游消费者：可以是 RazorVue 的编译时 JS artifact，也可以是 Jolt 的投影和工具工件。
