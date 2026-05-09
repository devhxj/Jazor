# Deno 前端协议

Jolt 与 Deno Worker 之间的通信协议：编译协议（`DenoCompilationProtocol`）和前端智能感知协议（`DenoFrontendProtocol`）。定义 C# 和 TypeScript 之间 JSON-RPC 通信的请求和响应数据结构。

相关源文件：
- `src/Jolt/Frontend/Deno/Protocol/DenoCompilationProtocol.cs` - 编译协议定义
- `src/Jolt/Frontend/Deno/Protocol/DenoFrontendProtocol.cs` - 智能感知协议定义
- `src/Jolt/Frontend/IFrontendContextProvider.cs` - 前端上下文提供者接口
- `src/Jolt/Frontend/Deno/Hosting/DenoWorkerProcess.cs` - 协议传输层（独立文档）
- `src/Jolt/Frontend/Deno/Hosting/DenoVolarHost.cs` - 协议使用者（独立文档）

## 核心类型

### 编译协议（DenoCompilationProtocol）

编译协议定义了三种编译操作的请求和响应：SFC 编译、TypeScript 编译和 CSS 模块编译。

#### SFC 编译

**DenoSfcCompileRequest**：
```csharp
internal sealed class DenoSfcCompileRequest
{
    public required string DocumentPath { get; init; }  // 文档路径（绝对路径）
    public required string SfcText { get; init; }       // SFC 文本内容
    public required string Filename { get; init; }      // 文件名（用于错误消息）
    public bool IsProduction { get; init; }             // 是否生产模式（影响压缩等）
}
```

**DenoSfcCompileResult**：
```csharp
internal sealed class DenoSfcCompileResult
{
    public required string JsContent { get; init; }              // 编译后的 JavaScript 内容
    public string? JsSourceMap { get; init; }                   // JavaScript source map
    public string? CssContent { get; init; }                    // 编译后的 CSS 内容（<style> 块）
    public IReadOnlyList<DenoSfcStyleFragmentResult> StyleFragments { get; init; } = [];  // 样式片段
    public IReadOnlyList<string> Diagnostics { get; init; } = [];  // 编译诊断信息
    public bool SupportsHmr { get; init; }                      // 是否支持 HMR
}
```

**DenoSfcStyleFragmentResult**：
```csharp
internal sealed class DenoSfcStyleFragmentResult
{
    public required string CssContent { get; init; }   // CSS 内容
    public string? SourcePath { get; init; }          // 源文件路径（如果从外部文件导入）
    public int? SourceLineStart { get; init; }        // 源文件起始行号
    public int? SourceLineCount { get; init; }        // 源文件行数
}
```

**使用场景**：
- DevServer：实时编译 Vue SFC 文件
- 构建：生产模式编译（压缩、优化）
- HMR：检测 SFC 是否支持热更新

#### TypeScript 编译

**DenoTypeScriptCompileRequest**：
```csharp
internal sealed class DenoTypeScriptCompileRequest
{
    public required string DocumentPath { get; init; }  // 文档路径
    public required string Text { get; init; }          // TypeScript 文本内容
    public required string Filename { get; init; }      // 文件名
}
```

**DenoTypeScriptCompileResult**：
```csharp
internal sealed class DenoTypeScriptCompileResult
{
    public required string JsContent { get; init; }              // 编译后的 JavaScript 内容
    public string? JsSourceMap { get; init; }                   // JavaScript source map
    public IReadOnlyList<string> Diagnostics { get; init; } = [];  // 编译诊断信息
}
```

**使用场景**：
- 编译 `<script setup lang="ts">` 块
- 编译独立的 .ts 文件
- 支持类型检查（诊断信息）

#### CSS 模块编译

**DenoCssModuleCompileRequest**：
```csharp
internal sealed class DenoCssModuleCompileRequest
{
    public required string DocumentPath { get; init; }  // 文档路径
    public required string Text { get; init; }          // CSS 文本内容
    public required string Filename { get; init; }      // 文件名
    public bool IsProduction { get; init; }             // 是否生产模式
}
```

**DenoCssModuleCompileResult**：
```csharp
internal sealed class DenoCssModuleCompileResult
{
    public required string CssContent { get; init; }  // 编译后的 CSS 内容
    public IReadOnlyDictionary<string, string> Modules { get; init; } = new Dictionary<string, string>();  // CSS 模块映射（类名 -> 哈希）
    public IReadOnlyList<string> Diagnostics { get; init; } = [];  // 编译诊断信息
}
```

**使用场景**：
- 编译 CSS Modules（`<style module>`）
- 生成局部作用域的类名（哈希）
- 支持生产模式优化（压缩）

### 智能感知协议（DenoFrontendProtocol）

智能感知协议定义了前端智能感知请求的通用信封和各种具体的请求类型。

#### 请求/响应信封

**DenoFrontendRequestEnvelope**：
```csharp
internal sealed class DenoFrontendRequestEnvelope
{
    public string Id { get; set; } = string.Empty;      // 请求 ID（GUID，N 格式）
    public string Method { get; set; } = string.Empty;  // RPC 方法名
    public object? Payload { get; set; }                 // 请求载荷（具体请求对象）
}
```

**DenoFrontendResponseEnvelope**：
```csharp
internal sealed class DenoFrontendResponseEnvelope
{
    public string Id { get; set; } = string.Empty;       // 响应 ID（与请求 ID 匹配）
    public bool Success { get; set; }                    // 是否成功
    public JsonElement? Result { get; set; }             // 结果（成功时）
    public string? Error { get; set; }                   // 错误消息（失败时）
}
```

**设计意图**：
- 统一的 JSON-RPC 协议格式
- 请求 ID 用于匹配请求和响应
- `Success` 标志区分成功和失败
- `Result` 和 `Error` 互斥（成功时 `Result` 非空，失败时 `Error` 非空）

#### 通用请求类型

**DenoTemplateDocumentRequest**：
```csharp
internal class DenoTemplateDocumentRequest
{
    public required string DocumentPath { get; init; }           // 文档路径
    public required string Text { get; init; }                   // 文档文本内容
    public SemanticContext? FrontendContext { get; init; }       // 前端语义上下文（从 C# 编译获取）
    public IReadOnlyList<ArtifactRecord>? FrontendArtifacts { get; init; }  // 前端编译产物
}
```

**DenoTemplateRequest**（继承自 `DenoTemplateDocumentRequest`）：
```csharp
internal class DenoTemplateRequest : DenoTemplateDocumentRequest
{
    public required LspPosition Position { get; init; }  // 位置（行、列）
}
```

**DenoTemplateRangeRequest**（继承自 `DenoTemplateDocumentRequest`）：
```csharp
internal class DenoTemplateRangeRequest : DenoTemplateDocumentRequest
{
    public required LspRange Range { get; init; }  // 范围（起始位置、结束位置）
}
```

#### 具体请求类型

**DenoTemplateDiagnosticRequest**：
```csharp
internal sealed class DenoTemplateDiagnosticRequest : DenoTemplateDocumentRequest
{
    // 无额外字段
}
```

**DenoTemplateSemanticTokensRequest**：
```csharp
internal sealed class DenoTemplateSemanticTokensRequest : DenoTemplateDocumentRequest
{
    // 无额外字段
}
```

**DenoTemplateReferenceRequest**（继承自 `DenoTemplateRequest`）：
```csharp
internal sealed class DenoTemplateReferenceRequest : DenoTemplateRequest
{
    public bool IncludeDeclaration { get; init; }  // 是否包含声明位置
}
```

**DenoTemplateRenameRequest**（继承自 `DenoTemplateRequest`）：
```csharp
internal sealed class DenoTemplateRenameRequest : DenoTemplateRequest
{
    public required string NewName { get; init; }  // 新名称
}
```

#### RPC 方法映射

| 方法名 | 请求类型 | 响应类型 | 说明 |
|--------|---------|---------|------|
| `compile/sfc` | `DenoSfcCompileRequest` | `DenoSfcCompileResult` | 编译 Vue SFC |
| `compile/ts` | `DenoTypeScriptCompileRequest` | `DenoTypeScriptCompileResult` | 编译 TypeScript |
| `compile/css-module` | `DenoCssModuleCompileRequest` | `DenoCssModuleCompileResult` | 编译 CSS 模块 |
| `template/diagnostics` | `DenoTemplateDiagnosticRequest` | `LspDiagnostic[]` | 获取诊断信息 |
| `template/completion` | `DenoTemplateRequest` | `LspCompletionItem[]` | 获取补全项 |
| `template/documentSymbols` | `DenoTemplateDocumentRequest` | `LspDocumentSymbol[]` | 获取文档符号 |
| `template/semanticTokens` | `DenoTemplateSemanticTokensRequest` | `LspSemanticToken[]` | 获取语义标记 |
| `template/documentLinks` | `DenoTemplateDocumentRequest` | `LspDocumentLink[]` | 获取文档链接 |
| `template/inlayHints` | `DenoTemplateRangeRequest` | `LspInlayHint[]` | 获取内联提示 |
| `template/foldingRanges` | `DenoTemplateDocumentRequest` | `LspFoldingRange[]` | 获取折叠范围 |
| `template/hover` | `DenoTemplateRequest` | `LspHoverResult` | 获取悬停信息 |
| `template/definition` | `DenoTemplateRequest` | `LspLocation[]` | 获取定义位置 |
| `template/implementation` | `DenoTemplateRequest` | `LspLocation[]` | 获取实现位置 |
| `template/references` | `DenoTemplateReferenceRequest` | `LspLocation[]` | 获取引用位置 |
| `template/rename` | `DenoTemplateRenameRequest` | `LspWorkspaceEdit` | 获取重命名编辑 |

### 前端上下文提供者（IFrontendContextProvider）

**接口定义**（`src/Jolt/Frontend/IFrontendContextProvider.cs`）：
```csharp
public interface IFrontendContextProvider
{
    ValueTask<GetFrontendContextResponse> GetFrontendContextAsync(
        GetFrontendContextRequest request,
        CancellationToken cancellationToken);
}
```

**类型说明**：
- `GetFrontendContextRequest`：前端上下文请求（定义在 `Jazor.VueContracts.Protocol` 命名空间）
- `GetFrontendContextResponse`：前端上下文响应（包含 `SemanticContext` 和 `ArtifactRecord`）

**使用场景**：
- 从 C# 编译获取前端语义上下文
- 传递给 Deno Worker 用于智能感知增强
- 支持跨语言引用（C# 组件 → Vue 模板）

## 核心算法

### 请求序列化流程

**C# 端**：
```
1. 构造请求对象（如 DenoTemplateRequest）
2. 包装到 DenoFrontendRequestEnvelope：
   a. Id = Guid.NewGuid().ToString("N")
   b. Method = "template/completion"
   c. Payload = request
3. 使用 JsonSerializer.Serialize() 序列化
4. 通过 stdin 发送到 Deno worker
```

**TypeScript 端**：
```
1. 从 stdout 读取 JSON 字符串
2. 解析为请求信封
3. 根据 Method 路由到具体处理器
4. 反序列化 Payload 为具体请求类型
5. 处理请求
```

### 响应反序列化流程

**TypeScript 端**：
```
1. 处理请求，生成结果
2. 包装到 DenoFrontendResponseEnvelope：
   a. Id = request.Id（匹配请求 ID）
   b. Success = true/false
   c. Result = result（成功时）
   d. Error = errorMessage（失败时）
3. 使用 JSON.stringify() 序列化
4. 通过 stdout 发送到 C#
```

**C# 端**：
```
1. 从 stdout 读取 JSON 字符串
2. 反序列化为 DenoFrontendResponseEnvelope
3. 检查 Success 标志：
   a. 如果为 false，抛出异常（包含 Error）
   b. 如果为 true，继续
4. 反序列化 Result 为指定类型（如 LspCompletionItem[]）
5. 返回结果
```

### 前端上下文传递流程

**获取上下文**（C# 端）：
```
1. 编译 C# Razor/Vue 组件
2. 生成 SemanticContext（包含类型信息、符号引用等）
3. 生成 ArtifactRecord（编译产物记录）
4. 通过 IFrontendContextProvider.GetFrontendContextAsync() 获取
```

**传递上下文**（C# → TypeScript）：
```
1. 构造 DenoTemplateRequest
2. 设置 FrontendContext = context.SemanticContext
3. 设置 FrontendArtifacts = context.Artifacts
4. 序列化并发送到 Deno worker
```

**使用上下文**（TypeScript 端）：
```
1. 接收 DenoTemplateRequest
2. 提取 FrontendContext 和 FrontendArtifacts
3. 传递给 Volar 引擎
4. Volar 使用 C# 语义信息增强智能感知：
   - 跨语言引用解析（C# 组件 → Vue 模板）
   - 类型检查（C# 类型 → Vue 表达式）
   - 补全建议（C# 成员 → Vue 模板）
```

## 线程安全模型

### 协议层线程安全

**协议类型本身是无状态的**：
- 所有协议类型都是 `record` 或 `class`（只读属性）
- 没有可变字段或属性
- 线程安全（不可变）

**序列化/反序列化**：
- `JsonSerializer.Serialize/Deserialize` 是线程安全的（无状态）
- 但需要注意：不要共享 `JsonSerializerOptions` 实例（如果配置了自定义转换器）

### 传输层线程安全

**DenoWorkerProcess** 使用两个独立的锁：
- `_lifecycleGate`：保护生命周期操作（StartAsync, StopAsync）
- `_requestGate`：保护请求发送（SendRequestAsync）

**设计意图**：
- 协议层是无状态的，线程安全由传输层保证
- 请求发送是串行的（JSON-RPC over stdin/stdout 本质上是单线程的）

## 错误处理

### 协议层错误

**JSON 序列化错误**：
- **类型**：`JsonException`
- **原因**：请求对象包含无法序列化的数据（如循环引用）
- **处理**：传输层捕获并重试（最多 3 次）

**JSON 反序列化错误**：
- **类型**：`JsonException`
- **原因**：响应 JSON 格式错误或类型不匹配
- **处理**：传输层捕获并重试（最多 3 次）

### 业务层错误

**Worker 返回错误**：
- **标志**：`DenoFrontendResponseEnvelope.Success = false`
- **消息**：`DenoFrontendResponseEnvelope.Error`
- **处理**：传输层抛出 `InvalidOperationException`（包含错误消息和 stderr 摘要）

**诊断信息**：
- **位置**：`DenoSfcCompileResult.Diagnostics`, `DenoTypeScriptCompileResult.Diagnostics`, `DenoCssModuleCompileResult.Diagnostics`
- **类型**：`IReadOnlyList<string>` - 字符串列表
- **处理**：上层转换为 LSP 诊断信息（`LspDiagnostic`）

## 配置选项

### JSON 序列化配置

**DenoWorkerProcess 使用的配置**：
```csharp
private readonly JsonSerializerOptions _jsonOptions = new()
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,  // camelCase 命名（JavaScript 惯例）
    PropertyNameCaseInsensitive = true                   // 不区分大小写（容错）
};
```

**设计意图**：
- 与 JavaScript/TypeScript 互操作（camelCase 命名）
- 容错性（不区分大小写，避免字段名大小写不匹配问题）

### RPC 方法命名约定

**编译方法**：`compile/{type}`
- `compile/sfc` - SFC 编译
- `compile/ts` - TypeScript 编译
- `compile/css-module` - CSS 模块编译

**智能感知方法**：`template/{feature}`
- `template/diagnostics` - 诊断
- `template/completion` - 补全
- `template/documentSymbols` - 文档符号
- `template/semanticTokens` - 语义标记
- `template/hover` - 悬停
- `template/definition` - 定义
- `template/references` - 引用
- `template/rename` - 重命名

**设计意图**：
- 层次化命名（compile/template）
- 清晰的功能分类
- 易于扩展（添加新的编译类型或智能感知功能）

## 与其他子系统的交互

### 与 DenoVolarHost 的交互

**关系**：`DenoVolarHost` 使用协议类型与 `DenoWorkerProcess` 通信

**数据流**：
```
DenoVolarHost.GetTemplateCompletionItemsAsync()
    ↓
构造 DenoTemplateRequest
    ↓
调用 DenoWorkerProcess.SendRequestAsync<LspCompletionItem[]>("template/completion", request, ...)
    ↓
DenoWorkerProcess 序列化请求（使用协议类型）
    ↓
发送到 Deno worker
    ↓
Deno worker 处理并返回响应
    ↓
DenoWorkerProcess 反序列化响应（使用协议类型）
    ↓
返回 LspCompletionItem[] 给 DenoVolarHost
```

### 与 Volar 服务的交互

**TypeScript 端**（frontend-worker.ts）：
```
1. 接收 DenoFrontendRequestEnvelope
2. 根据 Method 路由到具体处理器
3. 提取 Payload 并转换为 Volar 请求格式
4. 调用 Volar API
5. 将 Volar 响应转换为 DenoFrontendResponseEnvelope
6. 发送回 C#
```

**协议转换**：
- C# 协议类型 → Volar 请求类型
- Volar 响应类型 → C# 协议类型

### 与 C# 编译系统的交互

**入口**：`IFrontendContextProvider`

**数据流**：
```
C# 编译（Razor/Vue）
    ↓
生成 SemanticContext + ArtifactRecord
    ↓
IFrontendContextProvider.GetFrontendContextAsync()
    ↓
GetFrontendContextResponse
    ↓
传入 DenoVolarHost 智能感知方法
    ↓
打包到 DenoTemplateRequest.FrontendContext/FrontendArtifacts
    ↓
发送到 Deno worker
    ↓
Volar 使用 C# 语义信息增强智能感知
```

## 设计权衡

### JSON-RPC vs 二进制协议

**权衡**：
- **JSON-RPC**：简单、跨语言、易于调试，但性能较低
- **二进制协议**（如 gRPC、MessagePack）：性能高，但复杂、调试困难

**选择**：JSON-RPC

**设计依据**：
- LSP 智能感知请求频率相对较低（用户键入触发），性能损失可接受（< 1ms）
- 与 Volar/LSP 协议一致（JSON-RPC）
- TypeScript/JavaScript 互操作简单（原生 JSON 支持）
- 易于调试（可读的 JSON 文本）

### 统一信封 vs 多个方法

**权衡**：
- **统一信封**：所有请求使用相同的信封类型（`DenoFrontendRequestEnvelope`），通过 `Method` 字段区分
- **多个方法**：每个请求类型有专门的 RPC 方法

**选择**：统一信封

**设计依据**：
- 简化传输层实现（只需要一个 `SendRequestAsync<TResult>` 方法）
- 易于扩展（添加新请求类型不需要修改传输层）
- 符合 JSON-RPC 标准（method + params）

### 继承 vs 扁平化

**权衡**：
- **继承**：`DenoTemplateRequest` 继承 `DenoTemplateDocumentRequest`，复用字段
- **扁平化**：每个请求类型独立定义所有字段

**选择**：继承

**设计依据**：
- 减少重复代码（`DocumentPath`, `Text`, `FrontendContext`, `FrontendArtifacts`）
- 清晰的类型层次（文档请求 → 位置请求 → 范围请求）
- C# record 类型支持继承（简洁语法）

### JsonElement vs 具体类型

**权衡**：
- **JsonElement**：`DenoFrontendResponseEnvelope.Result` 使用 `JsonElement?`，延迟反序列化
- **具体类型**：直接使用具体类型（如 `LspCompletionItem[]`）

**选择**：`JsonElement` + 泛型方法

**设计依据**：
- 传输层不需要知道响应类型（通用性）
- 调用方指定响应类型（`SendRequestAsync<TResult>`）
- 延迟反序列化（只在需要时反序列化）
- 支持 `null` 响应（某些请求没有返回值）

### 诊断信息格式

**权衡**：
- **字符串列表**：`IReadOnlyList<string>` Diagnostics
- **结构化诊断**：`LspDiagnostic[]` Diagnostics

**选择**：编译协议使用字符串列表，智能感知协议使用 `LspDiagnostic[]`

**设计依据**：
- **编译协议**：Deno worker 的诊断格式可能不同于 LSP，使用字符串保持灵活
- **智能感知协议**：直接使用 LSP 诊断类型，避免二次转换
- 上层负责统一格式（将字符串转换为 `LspDiagnostic`）
