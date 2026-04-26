# RazorVue DenoHost 契约

## 1. 目的

本文档定义 RazorVue 编译器输出与 `DenoHost` 之间的契约。

其目标是使构建所有权明确：

- RazorVue 编译器拥有语义提取和 Vue 工件生成
- `DenoHost` 拥有依赖解析、统一编译、打包和后续面向运行时的行为

本文档有意比 `RazorVue.Design.md` 更窄。
它仅涵盖面向主机的工件和清单期望。

> 延伸阅读：[RazorVue.Design.md](./RazorVue.Design.md) | [RazorVue.ImplementationChecklist.md](../../../02-计划/jolt/razorvue-implementation/RazorVue.ImplementationChecklist.md) | [RazorVue.ComponentDescriptorSpec.md](./RazorVue.ComponentDescriptorSpec.md)

## 目录

- [1-目的](#1-目的)
- [2-核心边界](#2-核心边界)
- [3-工件模型](#3-工件模型)
- [4-必需的工件字段](#4-必需的工件字段)
- [5-清单模型](#5-清单模型)
- [6-依赖解析规则](#6-依赖解析规则)
- [7-物理化规则](#7-物理化规则)
- [8-hmr-契约保留](#8-hmr-契约保留)
- [9-sourcemap-契约保留](#9-sourcemap-契约保留)
- [10-验证期望](#10-验证期望)
- [11-第一阶段最小契约](#11-第一阶段最小契约)
- [12-结论](#12-结论)

## 2. 核心边界

RazorVue 编译器负责：

- 发现有效的 RazorVue 组件
- 提取契约和逻辑
- 降低为 Vue 组件工件
- 声明导入/样式/运行时提示
- 发送清单就绪元数据

`DenoHost` 负责：

- 解析声明的依赖
- 编译和打包模块
- 编排最终输出
- 后续 HMR 和 sourcemap 主机行为

这种分离必须保持稳定。

## 3. 工件模型

编译器不应该只给 `DenoHost` 一个原始 JS 字符串。

它应该生成结构化工件模型，如：

```csharp
public sealed record VueCompiledArtifact(
    string ComponentName,
    string RelativeModulePath,
    string ModuleCode,
    ImmutableArray<VueImportRequirement> Imports,
    ImmutableArray<VueStyleRequirement> Styles,
    VueArtifactIdentity Identity,
    VueRuntimeHints Hints);
```

确切的类型形状可能会演变，但上述类别应保持稳定。

编译器内部推荐：

- `RazorVueSemanticSnapshot` 是降低前的语义载体
- `VueCompiledArtifact` 是降低结果
- `RazorVueCatalog` 是面向主机的物化载体

不要将这三个关注点折衷为一个临时字符串有效载荷。

## 4. 必需的工件字段

### 4.1 `ComponentName`

稳定的人类可读组件名称。

用于：

- 诊断
- 清单可读性
- 工具

### 4.2 `RelativeModulePath`

组件模块的编译器拥有的相对输出路径。

此路径必须：

- 确定性
- 在等效构建中稳定
- 独立于打包器特定的文件命名

### 4.3 `ModuleCode`

编译器发送的 Vue ESM 模块内容。

第一阶段目标：

- `defineComponent`
- `setup`
- 渲染函数
- 标准 ESM 导入/导出

### 4.4 `Imports`

声明的模块导入要求。

这些不由编译器解析。
它们为 `DenoHost` 声明。

示例：

- `vue`
- `vuetify/components`
- `vue-router`
- 项目本地生成的组件路径

### 4.5 `Styles`

声明的样式依赖或样式相关要求。

示例：

- CSS 包依赖
- 库样式要求
- 主题相关包要求

### 4.6 `Identity`

为未来 HMR 和稳定更改跟踪保留的标识元数据。

推荐字段：

```csharp
public sealed record VueArtifactIdentity(
    string ComponentId,
    string ModuleId,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    HmrBoundaryKind HmrBoundaryKind);
```

### 4.7 `Hints`

面向主机编排的运行时/构建提示。

推荐字段：

```csharp
public sealed record VueRuntimeHints(
    bool RequiresVueRuntime,
    bool RequiresHydration,
    bool SupportsSsr,
    bool UsesTeleport,
    bool UsesSuspense,
    bool UsesKeepAlive);
```

第一阶段可能保持提示较小，但类别应该存在。

### 4.8 `SourceOrigins` 或 `OriginMapPath`

第一阶段不需要最终的 sourcemap 发送，
但工件必须能够携带或引用编译器拥有的源始元数据。

推荐选项：

- `ImmutableArray<RazorVueSourceOrigin> SourceOrigins`
- `string? OriginMapPath`

第一阶段任一方法都可接受。
不可接受的是在主机移交之前丢失源链。

## 5. 清单模型

`DenoHost` 应该消费从工件派生的编译器拥有的清单。

推荐形状：

```csharp
public sealed record RazorVueManifest(
    string AssemblyName,
    ImmutableArray<RazorVueManifestEntry> Modules);

public sealed record RazorVueManifestEntry(
    string ComponentName,
    string RelativeModulePath,
    ImmutableArray<string> Imports,
    ImmutableArray<string> Styles,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    string ContentHash,
    HmrBoundaryKind HmrBoundaryKind,
    bool RequiresHydration,
    bool SupportsSsr);
```

确切的线格式可以是 JSON、生成源或其他主机可消费载体，
但逻辑契约应保持此显式。

## 6. 依赖解析规则

编译器声明依赖。
`DenoHost` 解析它们。

这意味着：

- 编译器不成为包解析器
- 编译器不决定最终打包拓扑
- 编译器不应静默重写主机依赖策略

`ImportSpecifier` 和样式要求是面向主机的声明，而非主机独立的保证。

## 7. 物理化规则

分析器是语义提取入口点，
但分析器不是最终工件写入器。

因此，完整链应解释为：

1. 分析器提取语义
2. 编译器拥有的降低构建结构化工件
3. 后续面向构建的发送阶段物化这些工件
4. `DenoHost` 消费物化输出

这种区别必须在实现和文档中保持显式。

推荐第一阶段链：

1. 分析器验证 RazorVue 入口和误用模式
2. 编译器拥有的提取构建 `RazorVueSemanticSnapshot`
3. 降低构建 `VueCompiledArtifact`
4. 发送物化 `RazorVueCatalog` 加上清单/侧车输出
5. `DenoHost` 消费这些输出

推荐按阶段的所有权：

- 分析器项目拥有诊断和发现规则
- 编译器拥有的提取驱动器拥有 `RazorVueSemanticSnapshot`
- Vue 降低/发送层拥有 `VueCompiledArtifact`
- 目录/物化层拥有 `RazorVueCatalog`、清单和源侧车
- `DenoHost` 仅拥有主机消费和构建编排

### 7.1 与当前 `ModuleCatalog` 的迁移兼容性

仓库已经发货了一个普通模块流，其中生成的源将模块元数据嵌入到目标程序集中。

第一阶段 RazorVue 应该定义兼容的迁移路径，而不是 wholesale 替换该流。

推荐迁移形状：

- 为普通静态模块保留 `ModuleCatalog`
- 为 Vue 组件工件添加 `RazorVueCatalog`，或添加版本化超集契约
- 更新主机/发送代码以在过渡期间消费两种形状

这降低了交付风险，并避免了在第一个 RazorVue 路径证明之前强制完全主机重写。

## 8. HMR 契约保留

第一阶段不需要完整的 HMR 行为，
但主机契约必须已经支持它。

这需要：

- 稳定的 `ComponentId`
- 稳定的 `ModuleId`
- 分离的描述符/模板/逻辑哈希
- 声明的 `HmrBoundaryKind`

推荐边界枚举：

```csharp
public enum HmrBoundaryKind
{
    Unknown,
    TemplateOnly,
    LogicSafe,
    FullReloadRequired
}
```

编译器不需要 yet 实现运行时 HMR，
但它必须交给 `DenoHost` 足够的信息以便以后这样做。

## 9. SourceMap 契约保留

第一阶段不需要完整的 sourcemap 发送，
但主机契约必须已经保留通向它的路径。

这意味着工件应该能够携带或引用：

- 源始元数据
- 后续 source-map 构建输出
- 用于映射关联的稳定模块标识

编译器不得强制 `DenoHost` 仅从最终 JS 文本推断源始。

推荐侧车模型：

```csharp
public sealed record RazorVueSourceOriginMap(
    string ComponentId,
    string ModuleId,
    ImmutableArray<RazorVueSourceOriginEntry> Entries);

public sealed record RazorVueSourceOriginEntry(
    RazorVueOriginKind OriginKind,
    string SourceFilePath,
    int SourceSpanStart,
    int SourceSpanLength,
    int StartLine,
    int StartColumn,
    string? GeneratedFilePath,
    int? GeneratedSpanStart,
    int? GeneratedSpanLength,
    RazorVueMappingQuality MappingQuality);
```

第一阶段不需要这种确切的线形状，
但它确实需要：

- 原始 `.razor` 文件的路径（已知时）
- 稳定的跨度或稳定的段标识
- 精确源不可用时的生成回退位置
- 显式映射质量

推荐出处字段：

```csharp
public enum RazorVueOriginProvenance
{
    RazorSourceMap,
    GeneratedSyntaxLocation,
    GeneratedFallback
}
```

该出处应保留，以便下游工具可以区分：

- 精确的 Razor 支持的映射
- 生成代码派生的映射
- 仅生成回退记录

## 10. 验证期望

`DenoHost` 应该能够从编译器输出假设以下内容：

- 组件/模块标识是确定性的
- 导入/样式是显式声明的
- 契约级验证已在上游发生
- 清单条目对应于有效的编译器拥有的工件

`DenoHost` 不应被要求：

- 重新发现 props/emits/slots
- 重新解释 Razor 模板语义
- 重建组件契约

## 11. 第一阶段最小契约

第一阶段只需要：

- 稳定的 Vue ESM 工件发送
- 显式导入/样式
- 确定性的相对模块路径
- 分离的标识/哈希数据
- 最小的运行时提示
- 主机可消费清单

第一阶段不需要：

- 完整的 HMR 实现
- 完整的 sourcemap 输出
- 高级主机侧优化元数据

## 12. 结论

RazorVue / `DenoHost` 契约应被视为一流设计边界。

如果编译器只发送临时字符串，或者如果 `DenoHost` 必须重新发现语义信息，
系统将立即在以下方面失去稳定性：

- 构建所有权
- 诊断
- HMR 演进
- sourcemap 演进

结构化工件和编译器拥有的清单是必需的契约表面。
