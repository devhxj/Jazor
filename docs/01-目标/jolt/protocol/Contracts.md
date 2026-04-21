# Jolt RPC 协议契约

> 状态：已实现
> 定位：Jolt RPC 协议的核心数据契约定义，位于 `Jazor.VueContracts.Protocol` 共享命名空间

## 1. 文档定位

本文档描述 Jolt RPC 协议的契约类型，这些类型在编译器（Jolt）和前端分析器（RpcVueAnalysisClient）之间共享。所有契约类型定义在 `src/Jolt/Protocol/Contracts/` 目录下，通过 `Jazor.VueContracts.Protocol` 命名空间组织。

## 2. 核心类型

### 2.1 DocumentSnapshot (`DocumentSnapshot.cs`)

**职责**：表示文档的快照，包含文档路径、类型、内容和版本信息。

**关键成员**：
```csharp
public sealed class DocumentSnapshot
{
    public string DocumentPath { get; }      // 文档路径
    public DocumentKind DocumentKind { get; } // 文档类型
    public string Text { get; }              // 文档完整内容
    public string? Version { get; }          // 可选版本标识
}
```

**关联类型**：
```csharp
public enum DocumentKind
{
    Jazor,       // .jazor 文件
    CSharp,      // .cs 文件
    Vue,         // .vue 文件
    JavaScript,  // .js 文件
    TypeScript,  // .ts 文件
    Css,         // .css 文件
    Unknown      // 未知类型
}
```

**代码位置**：`src/Jolt/Protocol/Contracts/DocumentSnapshot.cs`

**设计决策**：
- 使用不可变设计，所有属性通过构造函数初始化
- `Version` 为可选字符串，支持不同版本管理策略（整数版本号、哈希值等）
- 构造函数进行空值检查，确保契约有效性

### 2.2 描述符类型 (`Descriptors.cs`)

#### 2.2.1 ImportDescriptor

**职责**：描述一个导入语句的完整信息，用于前端生成 import 声明。

**关键成员**：
```csharp
public sealed class ImportDescriptor
{
    public string LocalName { get; }         // 本地绑定名称
    public string Source { get; }            // 导入源路径
    public ImportKind ImportKind { get; }    // 导入类型（JSImport/VueImport）
    public ImportBindingKind BindingKind { get; } // 绑定类型（Default/Named/Namespace）
    public string? ImportedName { get; }     // 原始导出名称（Named 绑定时使用）
    public bool TemplateVisible { get; }     // 是否在 Vue 模板中可见
}
```

**关联枚举**：
```csharp
public enum ImportKind { JSImport, VueImport }
public enum ImportBindingKind { Default, Named, Namespace }
```

**代码位置**：`src/Jolt/Protocol/Contracts/Descriptors.cs:23-52`

**设计决策**：
- `TemplateVisible` 标志用于区分仅在脚本中使用的导入和在模板中也需要访问的导入（如 Vue 组件）
- `ImportedName` 支持重命名导入（`import { foo as bar } from 'mod'`）

#### 2.2.2 SourceMapDescriptor

**职责**：描述源代码到生成代码的映射关系，用于调试和错误定位。

**关键成员**：
```csharp
public sealed class SourceMapDescriptor
{
    public string SourcePath { get; }        // 源文件路径
    public string GeneratedPath { get; }     // 生成文件路径
    public int SourceStart { get; }          // 源代码起始位置
    public int SourceLength { get; }         // 源代码长度
    public int GeneratedStart { get; }       // 生成代码起始位置
    public int GeneratedLength { get; }      // 生成代码长度
}
```

**代码位置**：`src/Jolt/Protocol/Contracts/Descriptors.cs:54-83`

**设计决策**：
- 使用绝对偏移量而非行列号，简化计算
- 每个描述符表示一个连续的映射区间
- 支持跨文件映射（SourcePath 与 GeneratedPath 可不同）

#### 2.2.3 DiagnosticRecord

**职责**：描述编译器产生的诊断信息（错误、警告、提示）。

**关键成员**：
```csharp
public sealed class DiagnosticRecord
{
    public string Id { get; }                // 诊断标识符
    public DiagnosticSeverityKind Severity { get; } // 严重级别
    public string Message { get; }           // 诊断消息
    public string DocumentPath { get; }      // 所在文档路径
    public int Start { get; }                // 起始位置
    public int Length { get; }               // 长度
}
```

**关联枚举**：
```csharp
public enum DiagnosticSeverityKind
{
    Info,
    Warning,
    Error
}
```

**代码位置**：`src/Jolt/Protocol/Contracts/Descriptors.cs:85-114`

**设计决策**：
- `Id` 字段支持诊断去重和关联修复建议
- 使用绝对偏移量而非行列号，与 SourceMapDescriptor 保持一致

#### 2.2.4 ArtifactRecord

**职责**：描述编译生成的虚拟文件（如 JavaScript 输出、类型声明等）。

**关键成员**：
```csharp
public sealed class ArtifactRecord
{
    public string ArtifactName { get; }      // 工件名称（如文件名）
    public string ArtifactKind { get; }      // 工件类型（如 "js", "dts"）
    public string Content { get; }           // 工件内容
    public string? ContentHash { get; }      // 可选的内容哈希（用于缓存失效）
}
```

**代码位置**：`src/Jolt/Protocol/Contracts/Descriptors.cs:116-137`

**设计决策**：
- `ArtifactKind` 为开放字符串，支持扩展新类型
- `ContentHash` 为可选，仅在需要缓存失效时计算

### 2.3 RPC 请求/响应对 (`Requests.cs`)

#### 2.3.1 AnalyzeJazor

**职责**：请求分析 Jazor 文件，返回诊断信息、导入声明、生成工件和源映射。

**请求类型**：
```csharp
public sealed class AnalyzeJazorRequest
{
    public DocumentSnapshot JazorDocument { get; }           // 待分析的 Jazor 文档
    public IReadOnlyList<DocumentSnapshot> RelatedDocuments { get; } // 相关文档（C# 基类等）
    public SemanticContext? FrontendContext { get; }         // 可选的前端上下文
}
```

**响应类型**：
```csharp
public sealed class AnalyzeJazorResponse
{
    public IReadOnlyList<DiagnosticRecord> Diagnostics { get; }
    public IReadOnlyList<ImportDescriptor> Imports { get; }
    public IReadOnlyList<ArtifactRecord> Artifacts { get; }
    public IReadOnlyList<SourceMapDescriptor> SourceMaps { get; }
}
```

**代码位置**：`src/Jolt/Protocol/Contracts/Requests.cs:5-52`

**使用场景**：
- 编译器请求分析 Jazor 文件
- 返回所有必要的编译结果（诊断、导入、生成代码、映射）

#### 2.3.2 GetFrontendContext

**职责**：请求获取指定文档的前端语义上下文。

**请求类型**：
```csharp
public sealed class GetFrontendContextRequest
{
    public string DocumentPath { get; }
    public IReadOnlyList<string> RelatedDocumentPaths { get; }
}
```

**响应类型**：
```csharp
public sealed class GetFrontendContextResponse
{
    public SemanticContext SemanticContext { get; }
    public IReadOnlyList<ArtifactRecord> Artifacts { get; }
}
```

**代码位置**：`src/Jolt/Protocol/Contracts/Requests.cs:54-86`

**使用场景**：
- 编译器需要了解 Vue 组件的 props、emits、slots 等信息
- 从前端分析器获取类型定义和工件

#### 2.3.3 GetVirtualArtifact

**职责**：请求获取特定类型的虚拟文件内容（如转译后的 JS）。

**请求类型**：
```csharp
public sealed class GetVirtualArtifactRequest
{
    public string DocumentPath { get; }
    public string ArtifactKind { get; }       // 如 "js", "dts", "css"
    public string? Text { get; }              // 可选的文档内容（用于按需编译）
    public string? Version { get; }           // 可选的版本标识
}
```

**响应类型**：
```csharp
public sealed class GetVirtualArtifactResponse
{
    public ArtifactRecord Artifact { get; }
    public IReadOnlyList<DiagnosticRecord> Diagnostics { get; }
    public IReadOnlyList<SourceMapDescriptor> SourceMaps { get; }
}
```

**代码位置**：`src/Jolt/Protocol/Contracts/Requests.cs:88-135`

**使用场景**：
- 语言服务器请求虚拟文件内容（如转到定义、完成）
- 开发服务器请求按需编译输出

#### 2.3.4 GetHotUpdatePlan

**职责**：请求获取文档变更的热更新计划。

**请求类型**：
```csharp
public sealed class GetHotUpdatePlanRequest
{
    public string DocumentPath { get; }
    public DocumentKind DocumentKind { get; }
    public string? Version { get; }
}
```

**响应类型**：
```csharp
public sealed class GetHotUpdatePlanResponse
{
    public bool RequiresFullReload { get; }               // 是否需要完整重载
    public IReadOnlyList<string> AffectedDocumentPaths { get; } // 受影响的文档列表
    public string Reason { get; }                         // 原因说明
}
```

**代码位置**：`src/Jolt/Protocol/Contracts/Requests.cs:137-179`

**使用场景**：
- 开发服务器判断是否可以热更新
- 确定受影响的文档范围，实现精确的模块替换

### 2.4 主机信息 (`HostInfo.cs`)

#### 2.4.1 HostCapabilityDescriptor

**职责**：描述主机支持的一个能力特性。

**关键成员**：
```csharp
public sealed class HostCapabilityDescriptor
{
    public string Name { get; }            // 能力名称
    public string? Description { get; }    // 可选描述
}
```

**代码位置**：`src/Jolt/Protocol/Contracts/HostInfo.cs:3-16`

#### 2.4.2 GetHostInfoResponse

**职责**：响应主机的元信息。

**关键成员**：
```csharp
public sealed class GetHostInfoResponse
{
    public string HostName { get; }                     // 主机名称（如 "Jolt Language Server"）
    public string ProtocolVersion { get; }              // 协议版本
    public IReadOnlyList<HostCapabilityDescriptor> Capabilities { get; } // 能力列表
}
```

**代码位置**：`src/Jolt/Protocol/Contracts/HostInfo.cs:18-35`

#### 2.4.3 PingResponse

**职责**：响应 ping 请求，用于连接健康检查。

**关键成员**：
```csharp
public sealed class PingResponse
{
    public string Message { get; }           // 响应消息
    public string ProtocolVersion { get; }   // 协议版本
}
```

**代码位置**：`src/Jolt/Protocol/Contracts/HostInfo.cs:37-50`

### 2.5 SemanticContext (`SemanticContext.cs`)

**职责**：封装前端（Vue）的语义上下文信息。

**关键成员**：
```csharp
public sealed class SemanticContext
{
    public string ContextKind { get; }                       // 上下文类型（如 "VueComponent"）
    public IReadOnlyList<DocumentSnapshot> RelatedDocuments { get; } // 相关文档快照
    public IReadOnlyDictionary<string, string> Properties { get; }    // 附加属性
}
```

**代码位置**：`src/Jolt/Protocol/Contracts/SemanticContext.cs`

**设计决策**：
- 使用字典 `Properties` 支持扩展，避免频繁修改类型定义
- `RelatedDocuments` 包含完整的文档快照，无需额外请求

### 2.6 RPC 消息封装 (`RpcMessages.cs`)

#### 2.6.1 RpcRequestEnvelope

**职责**：封装 RPC 请求的通用信封格式。

**关键成员**：
```csharp
public sealed class RpcRequestEnvelope
{
    public string Id { get; }               // 请求标识符（用于关联响应）
    public string Method { get; }           // 方法名（如 "jolt/analyzeJazor"）
    public string? PayloadJson { get; }     // 可选的载荷 JSON
}
```

**代码位置**：`src/Jolt/Protocol/Contracts/RpcMessages.cs:3-22`

#### 2.6.2 RpcResponseEnvelope

**职责**：封装 RPC 响应的通用信封格式。

**关键成员**：
```csharp
public sealed class RpcResponseEnvelope
{
    public string? Id { get; }              // 对应的请求 ID
    public bool Success { get; }            // 是否成功
    public string? PayloadJson { get; }     // 成功时的载荷 JSON
    public RpcErrorRecord? Error { get; }   // 失败时的错误信息
}
```

**代码位置**：`src/Jolt/Protocol/Contracts/RpcMessages.cs:24-45`

#### 2.6.3 RpcErrorRecord

**职责**：描述 RPC 错误。

**关键成员**：
```csharp
public sealed class RpcErrorRecord
{
    public string Code { get; }             // 错误码（如 "unknown_method"）
    public string Message { get; }          // 错误消息
    public string? Details { get; }         // 可选的详细信息
}
```

**代码位置**：`src/Jolt/Protocol/Contracts/RpcMessages.cs:47-66`

### 2.7 JSON 序列化 (`ProtocolJsonSerializer.cs`)

**职责**：提供统一的 JSON 序列化配置。

**关键成员**：
```csharp
public static class ProtocolJsonSerializer
{
    public static readonly JsonSerializerOptions DefaultOptions; // Web 默认配置 + 枚举字符串转换

    public static string Serialize<T>(T value);
    public static T? Deserialize<T>(string json);
}
```

**代码位置**：`src/Jolt/Protocol/Contracts/ProtocolJsonSerializer.cs`

**设计决策**：
- 使用 `JsonSerializerDefaults.Web`（驼峰命名、宽松读取）
- 添加 `JsonStringEnumConverter` 将枚举序列化为字符串（而非数字）
- 禁用格式化（`WriteIndented = false`）减小传输大小

### 2.8 RPC 方法名常量

#### 2.8.1 JoltRpcMethodNames (`JoltRpcMethodNames.cs`)

**职责**：定义所有 Jolt RPC 方法的名称常量。

**常量列表**：
```csharp
public const string Ping = "jolt/ping";
public const string GetHostInfo = "jolt/getHostInfo";
public const string OpenDocument = "jolt/openDocument";
public const string UpdateDocument = "jolt/updateDocument";
public const string CloseDocument = "jolt/closeDocument";
public const string GetOpenDocuments = "jolt/getOpenDocuments";
public const string GetFrontendContext = "jolt/getFrontendContext";
public const string AnalyzeJazor = "jolt/analyzeJazor";
public const string GetVirtualArtifact = "jolt/getVirtualArtifact";
public const string GetHotUpdatePlan = "jolt/getHotUpdatePlan";
```

**代码位置**：`src/Jolt/Protocol/Contracts/JoltRpcMethodNames.cs`

**设计决策**：
- 使用 `jolt/` 前缀避免与其他 RPC 系统冲突
- 方法名采用小写 + 驼峰命名风格
- 常量避免字符串拼写错误

#### 2.8.2 VueAnalysisRpcMethodNames (`VueAnalysisRpcMethodNames.cs`)

**职责**：定义 Vue 分析器 RPC 方法名称。

**常量列表**：
```csharp
public const string AnalyzeJazor = "vueanalysis/analyzeJazor";
```

**代码位置**：`src/Jolt/Protocol/Contracts/VueAnalysisRpcMethodNames.cs`

**设计决策**：
- 使用 `vueanalysis/` 前缀区分 Vue 分析器方法
- 支持未来扩展更多 Vue 特定的 RPC 方法

## 3. 核心算法

### 3.1 契约验证策略

所有契约类型在构造时进行验证：

```csharp
// 构造函数中的参数验证
public DocumentSnapshot(
    string documentPath,
    DocumentKind documentKind,
    string text,
    string? version)
{
    DocumentPath = documentPath ?? throw new ArgumentNullException(nameof(documentPath));
    DocumentKind = documentKind;
    Text = text ?? throw new ArgumentNullException(nameof(text));
    Version = version;
}
```

**验证规则**：
- 非空字符串参数：`ArgumentNullException`
- 非空白字符串参数：`ArgumentException.ThrowIfNullOrWhiteSpace`
- 集合参数：非空检查

### 3.2 JSON 序列化配置

使用 `System.Text.Json` 的 Web 默认配置：

```csharp
private static JsonSerializerOptions CreateDefaultOptions()
{
    var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };
    options.Converters.Add(new JsonStringEnumConverter());
    return options;
}
```

**配置说明**：
- `JsonSerializerDefaults.Web`：驼峰命名、忽略大小写、允许尾部逗号
- `JsonStringEnumConverter`：枚举序列化为字符串（`"Jazor"` 而非 `0`）
- `WriteIndented = false`：紧凑输出，减少传输大小

### 3.3 请求/响应关联机制

使用 `Id` 字段关联请求和响应：

1. 客户端生成唯一请求 ID（如 GUID）
2. 服务器在响应中返回相同的 ID
3. 客户端通过 ID 匹配异步请求的响应

**示例流程**：
```
客户端 → { "id": "req-123", "method": "jolt/ping", "payloadJson": null }
服务器 → { "id": "req-123", "success": true, "payloadJson": "{...}", "error": null }
```

## 4. 线程安全模型

### 4.1 契约类型线程安全性

**所有契约类型均为不可变（immutable）**：
- 所有属性为只读（`{ get; }`）
- 构造后无法修改
- 天然线程安全，可自由共享

**示例**：
```csharp
// DocumentSnapshot 完全不可变
public sealed class DocumentSnapshot
{
    public string DocumentPath { get; }  // 只读属性
    public DocumentKind DocumentKind { get; }
    public string Text { get; }
    public string? Version { get; }
}
```

### 4.2 序列化器线程安全性

`ProtocolJsonSerializer.DefaultOptions` 为静态只读字段：
- 初始化后不可修改
- `JsonSerializerOptions` 本身线程安全（读取配置）
- `Serialize`/`Deserialize` 方法无状态，可并发调用

## 5. 错误处理

### 5.1 契约验证错误

**错误类型**：`ArgumentNullException`、`ArgumentException`

**触发场景**：
```csharp
// 空引用
new DocumentSnapshot(null, DocumentKind.Jazor, "text", null); // ArgumentNullException

// 空白字符串
new RpcRequestEnvelope("", "method", null); // ArgumentException
```

**处理策略**：快速失败（fail-fast），在构造时立即抛出异常

### 5.2 序列化错误

**错误类型**：`ArgumentException`

**触发场景**：
```csharp
// 空白 JSON
ProtocolJsonSerializer.Deserialize<DocumentSnapshot>("   "); // ArgumentException

// 无效 JSON 格式
ProtocolJsonSerializer.Deserialize<DocumentSnapshot>("{invalid}"); // JsonException
```

**处理策略**：
- 序列化前进行预检查（`string.IsNullOrWhiteSpace`）
- JSON 解析错误由 `System.Text.Json` 抛出 `JsonException`

### 5.3 RPC 层错误映射

契约层不直接处理 RPC 错误，错误映射在 `JoltRpcProcessor` 中完成（参见 `RpcTransport.md`）。

## 6. 配置选项

### 6.1 JSON 序列化配置

`ProtocolJsonSerializer.DefaultOptions` 可通过修改源代码调整：

```csharp
// 启用格式化（调试用）
options.WriteIndented = true;

// 添加自定义转换器
options.Converters.Add(new MyCustomConverter());

// 修改命名策略
options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
```

### 6.2 扩展契约类型

**添加新字段**：
```csharp
public sealed class ArtifactRecord
{
    // 现有字段...
    public string ArtifactName { get; }
    public string ArtifactKind { get; }
    public string Content { get; }
    public string? ContentHash { get; }

    // 新增字段（向后兼容）
    public string? MimeType { get; }  // 可选的 MIME 类型
}
```

**添加新方法名**：
```csharp
public static class JoltRpcMethodNames
{
    // 现有方法...
    public const string AnalyzeJazor = "jolt/analyzeJazor";

    // 新增方法
    public const string RenameSymbol = "jolt/renameSymbol";
}
```

## 7. 与其他子系统的交互

### 7.1 与 RPC 传输层交互

**契约层 → 传输层**：
- `RpcRequestEnvelope` → `JoltRpcProcessor.ProcessAsync`
- `RpcResponseEnvelope` ← `JoltRpcProcessor.ProcessAsync`
- `JoltRpcSerializer` 使用 `ProtocolJsonSerializer`

**数据流**：
```
IJoltRpcService (业务逻辑)
    ↓ (强类型请求/响应)
JoltRpcDispatcher (方法路由)
    ↓ (反序列化/序列化)
JoltRpcProcessor (RPC 处理)
    ↓ (RpcRequestEnvelope/RpcResponseEnvelope)
StdioJoltRpcServer (stdio 传输)
```

### 7.2 与编译器交互

**编译器 → 契约层**：
- 编译器创建 `AnalyzeJazorRequest`
- 契约层序列化为 JSON
- RPC 传输到前端分析器

**前端分析器 → 契约层**：
- 接收 `AnalyzeJazorRequest`
- 反序列化为强类型对象
- 返回 `AnalyzeJazorResponse`

### 7.3 与文档系统交互

**契约层使用文档类型**：
- `DocumentSnapshot`（完整文档快照）
- `DocumentKind`（文档类型枚举）
- `DocumentVersion`（文档版本标识，位于 `Jolt.Protocol.Documents` 命名空间）

**命名空间差异**：
- 契约类型：`Jazor.VueContracts.Protocol`（跨项目共享）
- 文档类型：`Jolt.Protocol.Documents`（Jolt 内部）

## 8. 设计权衡

### 8.1 不可变性 vs 可变性

**选择：完全不可变**

**优点**：
- 线程安全，无需锁
- 可预测的行为，无副作用
- 支持模式匹配和结构化比较（如 `record`）

**缺点**：
- 更新需要创建新实例
- 大对象频繁更新可能产生 GC 压力

**适用场景**：
- 契约类型通常表示快照，不需要更新
- RPC 请求/响应生命周期短，GC 压力可控

### 8.2 强类型 vs 弱类型载荷

**选择：强类型请求/响应 + JSON 载荷**

**方案 A**：完全强类型（每个方法定义独立请求/响应类）
```csharp
public sealed class AnalyzeJazorRequest { ... }
public sealed class GetFrontendContextRequest { ... }
```

**方案 B**：弱类型载荷（通用字典）
```csharp
public sealed class RpcRequest
{
    public string Method { get; }
    public Dictionary<string, object> Payload { get; }
}
```

**选择原因**：
- 类型安全：编译时检查，减少运行时错误
- IDE 支持：自动完成、重构、导航
- 文档化：类型定义即文档
- 性能：避免反射和字典查找

**权衡**：
- 需要为每个方法定义类型（代码量增加）
- 但类型复用度高（如 `DocumentSnapshot` 多处使用）

### 8.3 枚举字符串 vs 数字序列化

**选择：字符串序列化**

**配置**：
```csharp
options.Converters.Add(new JsonStringEnumConverter());
```

**优点**：
- 可读性强：`"Jazor"` vs `0`
- 向后兼容：新增枚举值不破坏旧版本（数字序号可能变化）
- 调试友好：JSON 日志直观

**缺点**：
- 传输大小略大：`"Jazor"` (7 bytes) vs `0` (1 byte)

**权衡**：可读性和兼容性优于传输大小

### 8.4 可选字段 vs 必需字段

**选择：明确区分可选/必需**

**模式**：
```csharp
// 必需字段：构造函数参数，无默认值
public string DocumentPath { get; }

// 可选字段：可空类型，默认为 null
public string? Version { get; }
```

**优点**：
- 强制明确：必需字段缺失时编译失败
- 向后兼容：新增字段可为可选
- 版本协商：`Version` 字段支持不同版本策略

### 8.5 单一命名空间 vs 多命名空间

**选择：单一共享命名空间 `Jazor.VueContracts.Protocol`**

**方案 A**：按功能分包
```
Jazor.VueContracts.Protocol.Requests
Jazor.VueContracts.Protocol.Responses
Jazor.VueContracts.Protocol.Descriptors
```

**方案 B**：单一命名空间
```
Jazor.VueContracts.Protocol
```

**选择原因**：
- 简化导入：`using Jazor.VueContracts.Protocol;`
- 避免循环依赖：请求/响应类型在同一命名空间
- 类型发现：IDE 自动完成时所有类型可见

**权衡**：
- 命名冲突风险增加（通过类型名称规避，如 `AnalyzeJazorRequest`）

### 8.6 同步 vs 异步 API

**选择：RPC 传输层异步，契约层无关**

契约类型本身不涉及异步操作，异步由 RPC 层处理：

```csharp
// IJoltRpcService 定义异步方法
Task<AnalyzeJazorResponse> AnalyzeJazorAsync(
    AnalyzeJazorRequest request,
    CancellationToken cancellationToken);

// 契约类型为纯数据结构
public sealed class AnalyzeJazorResponse { ... }
```

**优点**：
- 契约类型保持简单（POCO）
- 异步由传输层封装，契约类型可复用于不同传输（stdio、HTTP、WebSocket）

---

## 附录

### A. 完整类型清单

| 类型 | 文件 | 用途 |
|------|------|------|
| `DocumentKind` | `DocumentSnapshot.cs` | 文档类型枚举 |
| `DocumentSnapshot` | `DocumentSnapshot.cs` | 文档快照 |
| `ImportKind` | `Descriptors.cs` | 导入类型枚举 |
| `ImportBindingKind` | `Descriptors.cs` | 导入绑定类型枚举 |
| `ImportDescriptor` | `Descriptors.cs` | 导入描述符 |
| `SourceMapDescriptor` | `Descriptors.cs` | 源映射描述符 |
| `DiagnosticSeverityKind` | `Descriptors.cs` | 诊断严重级别枚举 |
| `DiagnosticRecord` | `Descriptors.cs` | 诊断记录 |
| `ArtifactRecord` | `Descriptors.cs` | 工件记录 |
| `AnalyzeJazorRequest` | `Requests.cs` | 分析请求 |
| `AnalyzeJazorResponse` | `Requests.cs` | 分析响应 |
| `GetFrontendContextRequest` | `Requests.cs` | 前端上下文请求 |
| `GetFrontendContextResponse` | `Requests.cs` | 前端上下文响应 |
| `GetVirtualArtifactRequest` | `Requests.cs` | 虚拟工件请求 |
| `GetVirtualArtifactResponse` | `Requests.cs` | 虚拟工件响应 |
| `GetHotUpdatePlanRequest` | `Requests.cs` | 热更新计划请求 |
| `GetHotUpdatePlanResponse` | `Requests.cs` | 热更新计划响应 |
| `HostCapabilityDescriptor` | `HostInfo.cs` | 主机能力描述符 |
| `GetHostInfoResponse` | `HostInfo.cs` | 主机信息响应 |
| `PingResponse` | `HostInfo.cs` | Ping 响应 |
| `SemanticContext` | `SemanticContext.cs` | 语义上下文 |
| `RpcRequestEnvelope` | `RpcMessages.cs` | RPC 请求信封 |
| `RpcResponseEnvelope` | `RpcMessages.cs` | RPC 响应信封 |
| `RpcErrorRecord` | `RpcMessages.cs` | RPC 错误记录 |
| `ProtocolJsonSerializer` | `ProtocolJsonSerializer.cs` | JSON 序列化器 |
| `JoltRpcMethodNames` | `JoltRpcMethodNames.cs` | Jolt RPC 方法名常量 |
| `VueAnalysisRpcMethodNames` | `VueAnalysisRpcMethodNames.cs` | Vue 分析器 RPC 方法名常量 |

### B. 命名约定

| 类别 | 约定 | 示例 |
|------|------|------|
| 请求类型 | `{动词}{名词}Request` | `AnalyzeJazorRequest` |
| 响应类型 | `{动词}{名词}Response` | `AnalyzeJazorResponse` |
| 描述符类型 | `{名词}Descriptor` | `ImportDescriptor` |
| 记录类型 | `{名词}Record` | `DiagnosticRecord` |
| 枚举类型 | `{名词}Kind` | `DocumentKind`, `ImportKind` |
| RPC 方法名 | `{service}/{action}` | `jolt/analyzeJazor` |
| 可选字段 | 可空类型 (`string?`) | `Version`, `ContentHash` |

### C. 相关文档

- `RpcTransport.md` - RPC 传输层实现（JoltRpcProcessor、JoltRpcDispatcher）
- `Documents.md` - 文档版本管理（DocumentVersion、TextSpan、TextChange）

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
