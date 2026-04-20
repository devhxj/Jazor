# RazorVue — 库模式

> 对应源码：`src/Jazor.RazorVue/`、`src/Jazor.RazorVue.Analysis/`、`src/Jazor.RazorVue.Vuetify/`

## 为什么需要

不是所有场景都需要完整的开发服务器和 HMR。很多项目只需要在编译时把 Razor 组件转成 JavaScript，像使用普通 NuGet 库一样集成到现有构建流程中。RazorVue 就是这个"轻量版"——通过 Source Generator 在编译时完成一切，无需额外进程或开发服务器。

## 解决什么问题

1. **编译时转换**：Razor 组件在 `dotnet build` 时自动转换为 JavaScript，无需运行时或开发服务器
2. **零配置集成**：安装 NuGet 包即可，Source Generator 自动注册，不需要额外工具链
3. **Vuetify UI 库**：提供 35 个 Vuetify 3 组件的 C# 包装，用 Razor 语法编写 Material Design 界面
4. **库模式发布**：转换结果作为库的一部分输出，可被其他项目引用

## 大致实现思路

### 核心区别：不使用 .vue SFC

RazorVue 的核心设计选择是**不生成 .vue 单文件组件**。Razor 组件直接转换为纯 JavaScript/TypeScript 模块，跳过 Vue SFC 编译步骤：

```
Razor 组件 (.razor)
     ↓ Source Generator（编译时自动触发）
     ↓ Roslyn 分析 + 语义提取
     ↓ Jazor.RazorVue 核心语义转换
JavaScript 模块（纯 JS/TS，非 .vue SFC）
     ↓ 嵌入到程序集或输出到项目
作为 NuGet 库的一部分发布
```

### 三个子项目

**Jazor.RazorVue（核心语义）**
- 定义组件模型：属性映射、事件绑定、子内容插槽
- 处理 Razor 语法到 JavaScript 的语义转换
- 不依赖 .vue SFC 格式，直接输出 JS 模块

**Jazor.RazorVue.Analysis（编译时分析）**
- 薄 Roslyn 宿主，在编译时验证 RazorVue 组件的正确性
- 生成组件描述信息供 Source Generator 使用
- 输出诊断信息到 IDE

**Jazor.RazorVue.Vuetify（UI 组件库）**

Vuetify 3 组件的 C# 包装：

```csharp
[VueLibraryComponent("vuetify/components", "VBtn")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public partial class VBtn : VuetifyComponentBase
{
    [Parameter] public string? Text { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }
}
```

已包装 35 个组件：VBtn、VCard、VDialog、VDataTable、VTextField、VSelect、VTabs、VToolbar 等。

### 与 Jolt 的对比

| 维度 | RazorVue（库模式） | Jolt（全功能模式） |
|------|-------------------|---------------------|
| 触发方式 | Source Generator（编译时） | 独立进程（LSP + DevServer） |
| 输出格式 | 纯 JS/TS 模块 | .vue SFC + JS/CSS |
| 开发热更新 | 无（需要重新编译） | HMR（< 500ms） |
| 调试支持 | 无 | DAP + CDP 源码级调试 |
| LSP 智能提示 | 无（仅 Roslyn 分析） | 3-Lane 全语义（Jazor + Roslyn + Volar） |
| 适用场景 | 库开发、CI 构建 | 应用开发、实时预览 |

## 设计文档

`design/` 目录下包含 RazorVue 的设计决策、约束和实现规范。
