# VueCompiledArtifact - RazorVue 编译产物

## 1. 文档定位

`VueCompiledArtifact` 及相关记录类型，这是 RazorVue 编译链路的最终输出模型。编译产物由 Source Generator 生成并嵌入到程序集中，供 DevServer/HMR/Build 等下游阶段使用。

**核心文件**：
- `src/Jazor.RazorVue/Artifacts/VueCompiledArtifact.cs`

## 2. 核心类型

### 2.1 VueCompiledArtifact

编译产物的顶级容器，包含一个 RazorVue 组件的完整编译结果。

```csharp
public sealed record VueCompiledArtifact(
    string ComponentName,                    // 组件名称（PascalCase）
    string RelativeModulePath,               // 输出模块相对路径（如 "components/MyComponent.js"）
    string ModuleCode,                       // 生成的完整 ES6 module 源码
    ImmutableArray<string> Imports,          // 需要导入的外部依赖（如 "vue", "./utils"）
    ImmutableArray<string> Styles,           // 提取的 CSS/SCSS 内容
    ImmutableArray<string> PluginRequirements,// 需要的 Vue 插件（如 "router", "pinia"）
    VueArtifactIdentity Identity,            // 组件标识与哈希
    VueRuntimeHints Hints,                   // 运行时特性提示
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins); // 源码映射信息
```

**字段说明**：
- **ComponentName**：组件的 C# 类名，用作 Vue 组件注册名（需转换为 kebab-case）
- **RelativeModulePath**：输出路径相对于构建根目录，使用正斜杠，不以 `/` 开头
- **ModuleCode**：完整的 JavaScript/TypeScript 模块代码，可直接写入文件系统
- **Imports**：声明式依赖列表，由打包系统使用（非代码内的 import 语句）
- **Styles**：从 Razor 模板中提取的 `<style>` 块内容（去重后）
- **PluginRequirements**：组件使用的 Vue 插件标识符（影响 runtime 初始化）

### 2.2 VueArtifactIdentity

组件的唯一标识与版本控制信息，用于 HMR 边界检测和缓存失效。

```csharp
public sealed record VueArtifactIdentity(
    string ComponentId,       // 组件唯一 ID（通常为完整类型名）
    string ModuleId,          // 模块 ID（用于 import map 解析）
    string DescriptorHash,    // 组件描述符哈希（props/emits/slots）
    string TemplateHash,      // 模板 AST 哈希
    string LogicHash,         // setup() 函数体哈希
    HmrBoundaryKind HmrBoundaryKind); // HMR 边界类型
```

**哈希用途**：
- **DescriptorHash**：props/emits/slots 定义变化时触发组件重新注册
- **TemplateHash**：模板结构变化时触发模板热更新
- **LogicHash**：setup() 逻辑变化时触发组件实例热更新

### 2.3 VueRuntimeHints

运行时特性提示，优化 Vue runtime 初始化和 SSR 渲染。

```csharp
public sealed record VueRuntimeHints(
    bool RequiresVueRuntime,   // 是否需要 Vue runtime（false 表示纯静态组件）
    bool RequiresHydration,    // 是否需要水合（CSR 模式）
    bool SupportsSsr,          // 是否支持 SSR 服务端渲染
    bool UsesTeleport,         // 是否使用 <Teleport> 组件
    bool UsesSuspense,         // 是否使用 <Suspense> 组件
    bool UsesKeepAlive);       // 是否使用 <KeepAlive> 组件
```

**优化场景**：
- `RequiresVueRuntime = false`：纯静态组件，可跳过 Vue runtime 初始化
- `SupportsSsr = true`：生成时保留 SSR 兼容代码路径（避免 `window` 引用）
- `UsesTeleport/UsesSuspense/UsesKeepAlive`：影响 DevServer 的 HMR 策略

### 2.4 HmrBoundaryKind 枚举

HMR（热模块替换）边界类型，决定组件变更时的更新策略。

```csharp
public enum HmrBoundaryKind
{
    Unknown,             // 未知（默认值，保守策略为 FullReloadRequired）
    TemplateOnly,        // 仅模板变化，可安全热更新
    LogicSafe,           // 逻辑安全变更（如新增 computed），可热更新
    FullReloadRequired   // 需要完全重载（如 props 类型变化、生命周期钩子变化）
}
```

**边界检测规则**（在编译时确定）：
- **TemplateOnly**：仅模板 AST 变化，props/emits/slots 不变
- **LogicSafe**：setup() 逻辑变化，但未修改外部状态访问模式
- **FullReloadRequired**：组件签名变化（props 类型变化、emits 列表变化）

## 3. 核心算法

### 3.1 编译产物生成流程

```
RazorVue 组件类 (C#)
       ↓
SemanticWalker 转换
       ↓
VueComponentDescriptor 构建
       ↓
VueCodeEmitter 发射
       ↓
VueCompiledArtifact 生成
```

### 3.2 哈希计算策略

每个哈希字段使用不同的内容计算：

- **DescriptorHash**：基于 `VueComponentDescriptor` 的 props/emits/slots JSON 序列化
- **TemplateHash**：基于 Razor 模板的 C# AST 结构（忽略空白符和注释）
- **LogicHash**：基于 `setup()` 函数体的 JavaScript AST 结构

**哈希算法**：使用 SHA256 并截取前 16 字符（hex 编码）

### 3.3 HMR 边界推断

编译时根据变更类型推断 `HmrBoundaryKind`：

```csharp
// 伪代码示例
HmrBoundaryKind InferBoundary(VueArtifactIdentity oldIdentity, VueArtifactIdentity newIdentity)
{
    if (oldIdentity.DescriptorHash != newIdentity.DescriptorHash)
        return HmrBoundaryKind.FullReloadRequired;

    if (oldIdentity.TemplateHash != newIdentity.TemplateHash &&
        oldIdentity.LogicHash != newIdentity.LogicHash)
        return HmrBoundaryKind.LogicSafe;

    if (oldIdentity.TemplateHash != newIdentity.TemplateHash)
        return HmrBoundaryKind.TemplateOnly;

    return HmrBoundaryKind.Unknown;
}
```

## 4. 线程安全模型

`VueCompiledArtifact` 是不可变 record 类型，天然线程安全。

- **写入阶段**：Source Generator 在编译时单线程生成
- **读取阶段**：DevServer/HMR/Build 可并发读取（无状态访问）

## 5. 错误处理

### 5.1 编译时验证

Source Generator 生成阶段会验证：

- **RelativeModulePath 非空**：空路径抛出 `InvalidOperationException`
- **ModuleCode 非空**：空代码抛出 `InvalidOperationException`
- **哈希格式**：必须为 16 字符的 hex 字符串

### 5.2 运行时错误

- **Imports 解析失败**：DevServer 将回退到全量依赖扫描
- **Styles 注入失败**：记录警告但不阻塞模块加载

## 6. 配置选项

无直接配置选项。行为由 Source Generator 的分析阶段决定。

## 7. 与其他子系统的交互

### 7.1 与 RazorVueCatalog 的交互

- `RazorVueCatalog` 聚合多个 `VueCompiledArtifact`，提供程序集级别的索引
- 按 `RelativeModulePath` 和 `ComponentName` 排序，优化 DevServer 查找性能

### 7.2 与 RazorVueSemanticSnapshot 的交互

- `RazorVueSemanticSnapshot` 提供编译前的语义信息
- `VueCompiledArtifact` 是编译后的输出，两者通过 `ComponentId` 关联

### 7.3 与 DevServer/HMR 的交互

- DevServer 读取 `VueCompiledArtifact.ModuleCode` 写入磁盘
- HMR 使用 `VueArtifactIdentity` 检测变更边界
- `VueRuntimeHints` 影响 HMR 更新策略（如 `UsesSuspense` 需要 Suspense 边界处理）

### 7.4 与 Build 的交互

- Build 阶段将 `VueCompiledArtifact` 聚合为最终 bundle
- `Imports` 和 `PluginRequirements` 用于生成 import map 和插件初始化代码

## 8. 设计权衡

### 8.1 为什么使用 Record 类型

- **值语义**：产物是不可变快照，天然支持值比较（哈希相等性检查）
- **模式匹配**：方便解构和 HMR 边界推断
- **性能**：record 的 `Equals`/`GetHashCode` 自动基于所有字段生成

### 8.2 为什么分离 Identity 和 Hints

- **Identity**：用于版本控制和缓存失效（每次编译必变）
- **Hints**：用于运行时优化（跨编译复用）

如果合并为单一 record，会导致运行时优化信息变化时强制 HMR 全量重载。

### 8.3 为什么 SourceOrigins 不可变数组

- **性能**：编译时一次性生成，运行时只读访问
- **内存**：`ImmutableArray` 比原生数组更紧凑（无额外分配）
- **安全**：防止 DevServer 意外修改源映射信息

### 8.4 HMR 边界类型保守策略

当无法确定边界类型时（`Unknown`），默认使用 `FullReloadRequired` 而非 `TemplateOnly`：

- **理由**：错误的热更新可能导致状态不一致，全量重载更安全
- **优化方向**：后续可引入更精细的静态分析推断边界类型
