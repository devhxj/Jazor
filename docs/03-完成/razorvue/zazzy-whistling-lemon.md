# RazorVue SFC 生成：从 IOperation 迁移到 Razor IR 直接提取

## Context

当前 RazorVue 的 SFC 生成管线：`.razor` → Razor SDK → `BuildRenderTree`(C#) → IOperation 分析 → `RazorVueRenderFragment` → Canonical H-Model → SFC Semantic Model → `VueSfcArtifact` → 生成的 C# catalog class → Jazor.Emit 写 `.vue` 文件。

这条管线有根本性缺陷：Razor 的声明式模板（`@if`、`@foreach`、组件嵌套）在编译到 `BuildRenderTree` C# 代码后，已经退化为命令式的 `RenderTreeBuilder` 调用序列。要从这些调用中恢复原始模板结构，需要复杂的栈式解析器（`RazorVueRenderTreeExtractor`），且仍然无法完整还原控制流（`@if`/`@foreach` 在 IR 中是结构化的，在 `BuildRenderTree` 中是散开的 C# 代码块）。

**目标**：在 Razor SDK 编译管线中插入自定义 `IRazorEnginePhase`，直接从 Razor IR 节点提取模板结构并生成 SFC 文本，写入与现有格式相同的 catalog class，由 Jazor.Emit 写出 `.vue` 文件。这样完全绕过 `BuildRenderTree` → IOperation 路径，消除中间层损耗。

## 架构

```
当前管线:
  .razor → Razor SDK → BuildRenderTree(C#) → IOperation → RazorVueRenderTree → CanonicalHModel → SfcSemanticModel → VueSfcArtifact → catalog class → .vue

新管线:
  .razor → Razor SDK → IR → [RazorVueSfcPhase (自定义 IRazorEnginePhase)] → SFC 文本 → catalog class → .vue
```

新管线复用 Razor SDK 的编译管线，在 IR 阶段截取，不依赖 IOperation。

## 实施步骤

### Step 1: 项目基础设施

**添加 `ILAccess.Fody` 到 `Jazor.Common`**

因为 `Jazor.Common` 目标是 `netstandard2.0`，不能使用 `UnsafeAccessor`（需要 .NET 7+）。使用 `ILAccess.Fody` 在编译时重写 IL，实现对 Razor SDK internal API 的访问。

- 文件: `src/Jazor.Common/Jazor.Common.csproj`
  - 添加 `ILAccess.Fody` NuGet 包引用
  - 添加 `Fody.xUnit` 或直接配置 `FodyWeavers.xml`
- 文件: `src/Jazor.Common/FodyWeavers.xml`（新建）
  ```xml
  <Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <ILAccess />
  </Weavers>
  ```
- 参考 Jolt 中的 `UnsafeAccessor` 用法模式（`src/Jolt/Razor/InProc/RazorDesignTimeCodeProjectionService.cs`），用 `ILAccess.Fody` 的 `[ILAccess]` attribute 替代

### Step 2: Razor IR 节点到 SFC 的映射层

**新建 `RazorVueIrWalker`** — 核心类型，遍历 Razor IR 节点树，生成 SFC `<template>` 内容。

- 文件: `src/Jazor.RazorVue/IrExtraction/RazorVueIrWalker.cs`（新建）

  IR 节点 → SFC 映射规则：

  | Razor IR 节点 | SFC 输出 |
  |---------------|----------|
  | `MarkupElementIntermediateNode` | `<tag>` |
  | `HtmlContentIntermediateNode` | 纯文本 |
  | `CSharpExpressionIntermediateNode` | `{{ expression }}` |
  | `CSharpCodeIntermediateNode` | `@if` / `@foreach` 等（需要解析代码文本来识别控制流） |
  | `ComponentIntermediateNode` | `<ComponentName>` |
  | `ComponentAttributeIntermediateNode` | `:prop="value"` |
  | `TagHelperIntermediateNode` | 根据具体 tag helper 类型分派 |
  | `RazorDirectiveIntermediateNode` | 提取 `@code` 块内容 |

- 文件: `src/Jazor.RazorVue/IrExtraction/RazorVueIrSfcBuilder.cs`（新建）
  - 组装完整 `.vue` 文件：`<template>` + `<script setup>` + `<style scoped>`
  - 从 IR 中提取 `@code` 块内容生成 `<script setup>`
  - 收集组件 import 依赖

**关键限制**：`@if`/`@foreach` 在 Razor IR 中不是独立节点，它们以 `CSharpCodeIntermediateNode` 形式嵌入，需要解析 C# 代码文本来识别控制流模式。这意味着需要轻量的模式匹配来将 `if (...) {` 映射到 `v-if="..."`，`foreach (... in ...)` 映射到 `v-for="..."`。

### Step 3: 自定义 `IRazorEnginePhase`

**新建 `RazorVueSfcPhase`** — 注册到 Razor 编译管线，在 IR 生成后执行。

- 文件: `src/Jazor.RazorVue/IrExtraction/RazorVueSfcPhase.cs`（新建）

  ```csharp
  // 伪代码
  internal class RazorVueSfcPhase : IRazorEnginePhase
  {
      public RazorEngineEngine Engine { get; set; }

      public void Execute(RazorCodeDocument document)
      {
          var ir = document.GetDocumentIntermediateNode();
          var walker = new RazorVueIrWalker();
          var sfcContent = walker.Walk(ir);

          // 将 SFC 文本存入 document 特征，供 Source Generator 读取
          document.SetFeature(new RazorVueSfcFeature(sfcContent));
      }
  }
  ```

- 需要通过 `ILAccess.Fody` 访问的 internal API：
  - `RazorCodeDocument.GetDocumentIntermediateNode()` — 获取 IR 根节点
  - `DocumentIntermediateNode` 的子节点遍历
  - `IntermediateNodeWalker` 基类（如可访问）

### Step 4: Phase 注册与管线集成

**在 `RazorVueSfcPipeline` 中注册自定义 Phase**

- 文件: `src/Jazor.Analyzer/RazorVue/RazorVueSfcPipeline.cs`（修改）
  - 在 `RazorProjectEngineBuilder` 配置中注册 `RazorVueSfcPhase`
  - Phase 执行顺序：在 Razor SDK 完成代码生成之前（IR 已构建完成后）

- 文件: `src/Jazor.Analyzer/RazorVue/RazorVueGenerator.cs`（修改）
  - 从 `RazorCodeDocument` 的自定义特征中读取 SFC 文本
  - 写入与现有格式相同的 catalog class（复用 `BuildRazorVueSfcArtifactSource` 或等价逻辑）

### Step 5: Catalog Class 输出

**保持与现有 catalog class 格式一致**

- 输出格式不变：
  - 每个 `.vue` 生成一个 `.g.cs` 文件，包含 `Get<Name>Sfc()` 方法返回 SFC 文本
  - 一个 `RazorVueCatalog.g.cs` 包含 `GetArtifacts()` 聚合方法
- 这样 Jazor.Emit 的下游管线无需改动

### Step 6: 清理旧管线

**标记/移除 IOperation 路径**

- 文件: `src/Jazor.RazorVue/RenderTree/RazorVueRenderTreeExtractor.cs` — 标记 obsolete 或移除
- 文件: `src/Jazor.RazorVue/RenderTree/RazorVueRenderTree.cs` — 标记 obsolete 或移除
- 文件: `src/Jazor.RazorVue/Canonical/RazorVueCanonicalHModelFactory.cs` — 标记 obsolete 或移除
- 文件: `src/Jazor.RazorVue/Lowering/RazorVueSfcArtifactFactory.cs` — 标记 obsolete 或移除
- 注意：先确保新管线端到端可用后再移除旧代码

## 关键文件清单

| 文件 | 操作 | 说明 |
|------|------|------|
| `src/Jazor.Common/Jazor.Common.csproj` | 修改 | 添加 ILAccess.Fody 引用 |
| `src/Jazor.Common/FodyWeavers.xml` | 新建 | Fody 配置 |
| `src/Jazor.RazorVue/IrExtraction/RazorVueIrWalker.cs` | 新建 | IR 节点遍历 → SFC 生成 |
| `src/Jazor.RazorVue/IrExtraction/RazorVueIrSfcBuilder.cs` | 新建 | SFC 文件组装 |
| `src/Jazor.RazorVue/IrExtraction/RazorVueSfcPhase.cs` | 新建 | 自定义 IRazorEnginePhase |
| `src/Jazor.Analyzer/RazorVue/RazorVueSfcPipeline.cs` | 修改 | 注册自定义 Phase |
| `src/Jazor.Analyzer/RazorVue/RazorVueGenerator.cs` | 修改 | 读取 SFC 特征，写入 catalog class |

## 验证

1. **单元测试**：在 `src/Jazor.RazorVue.Test` 中添加 IR 遍历测试
   - 简单元素 → `<template>` 输出
   - 组件嵌套 → 正确的父子结构
   - 属性绑定 → `:prop="expr"`
   - `@if` / `@foreach` → `v-if` / `v-for` 映射
2. **集成测试**：现有 RazorVue 测试应通过（输出格式不变）
3. **端到端验证**：
   ```bash
   dotnet build src/Jazor.Analyzer/Jazor.Analyzer.csproj
   dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj
   ```

## 风险与缓解

- **Internal API 访问风险**：Razor SDK internal API 可能随版本变化。`ILAccess.Fody` 在编译时绑定，升级 SDK 时需验证。缓解：最小化 internal 访问点，优先使用 public API。
- **控制流识别局限**：`@if`/`@foreach` 在 IR 中以 C# 代码块存在，模式匹配可能不完整。缓解：首期支持常见模式（`if`/`else if`/`else`、`foreach`），复杂控制流可降级为原始 C# 代码块。
- **并行管线过渡期**：新旧管线可能共存一段时间。缓解：通过 feature flag 或配置切换，确保渐进式迁移。
