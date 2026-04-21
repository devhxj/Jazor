# InProc Roslyn 代码服务

> 状态：已实现
> 定位：Jolt LSP 核心服务层，提供完整的 Roslyn 语义分析能力

## 1. 文档定位

本文档描述 `InProcRoslynCodeService` 的实现，这是 Jolt 项目中用于提供 C# 代码智能感知的核心服务。该服务通过进程内 Roslyn API 为 Jazor 文件和 C# 文件提供 Hover、Completion、Go To Definition、Find References、Rename、Diagnostics 等 LSP 功能。

**源文件位置**：
- `src/Jolt/Roslyn/InProc/InProcRoslynCodeService.cs`（主文件，约 989 行）
- `src/Jolt/Roslyn/InProc/InProcRoslynCodeService.ProjectionAndContext.cs`（投影与上下文）
- `src/Jolt/Roslyn/InProc/InProcRoslynCodeService.SymbolsAndSemantic.cs`（符号与语义分析）
- `src/Jolt/Roslyn/InProc/InProcRoslynCodeService.FallbackAndImplementation.cs`（回退与实现查找）

## 2. 核心类型

### 2.1 InProcRoslynCodeService

主服务类，协调文档投影、编译上下文创建和 LSP 功能实现。

**依赖项**：
- `JazorVueParser _parser`：解析 Jazor 文件
- `RazorDesignTimeCodeProjectionService _razorProjectionService`：Razor 设计时代码投影
- `Lock _compilationCacheGate`：编译缓存锁
- `Dictionary<string, CachedCompilationContext> _compilationCache`：编译缓存（最多 16 条）

### 2.2 RoslynCodeContext（record）

封装单次 LSP 请求的完整上下文：

```csharp
internal sealed record RoslynCodeContext(
    DocumentSnapshot Document,              // 原始文档
    string ProjectedText,                   // 投影后的 C# 代码
    ProjectionMap ProjectionMap,            // 位置映射
    SyntaxTree SyntaxTree,                  // Roslyn 语法树
    CSharpCompilation Compilation,          // Roslyn 编译
    SemanticModel SemanticModel,            // 语义模型
    IReadOnlyDictionary<SyntaxTree, ProjectedDocumentContext> ProjectedDocuments,
    int ProjectedOffset,                    // 投影代码中的偏移量
    LspPosition ProjectedPosition);         // 投影代码中的位置
```

### 2.3 ProjectedDocumentContext（record）

单个投影文档的上下文：

```csharp
internal sealed record ProjectedDocumentContext(
    DocumentSnapshot Document,
    string ProjectedText,
    ProjectionMap ProjectionMap,
    SyntaxTree SyntaxTree,
    SemanticModel SemanticModel);
```

### 2.4 CachedCompilationContext（class）

缓存的编译上下文，用于避免重复编译：

```csharp
private sealed class CachedCompilationContext
{
    public CSharpCompilation Compilation { get; }
    public IReadOnlyList<ProjectedDocumentContext> ProjectedDocuments { get; }
    public IReadOnlyDictionary<SyntaxTree, ProjectedDocumentContext> ContextsByTree { get; }
    public long LastUsedTick { get; set; }  // LRU 时钟
}
```

## 3. 核心算法

### 3.1 上下文创建（TryCreateContext）

**目的**：为 LSP 请求创建包含投影文档和 Roslyn 编译的完整上下文。

**流程**：

1. **构建投影文档列表**（`BuildProjectedDocuments`）：
   - 遍历所有 Roslyn 源文档（主文档 + 打开文档 + 工作区发现的文档）
   - 为每个文档创建投影：
     - **C# 文档**：使用全文档恒等投影（`WholeDocument`）
     - **Jazor 文档**：尝试 Razor 投影，失败则使用 JazorVueParser 回退
   - 返回 `ProjectedDocumentContext` 列表

2. **位置映射**（`TryMapToProjectedPositionWithBoundaryFallback`）：
   - 通过 `ProjectionMap.TryMapToProjectedPosition` 将原始位置映射到投影位置
   - **边界回退**：如果精确映射失败，向前搜索最近的投影偏移量
   - **C# 特殊处理**：C# 主文档使用恒等投影，EOF 边缘位置可直接映射

3. **创建编译**（`GetOrCreateCompilationContext`）：
   - 基于所有投影文档的 `SyntaxTree` 创建 `CSharpCompilation`
   - 为每个 `SyntaxTree` 获取 `SemanticModel`
   - 缓存编译结果（基于文档路径 + 内容 SHA256 的缓存键）
   - LRU 淘汰：超过 16 条缓存时删除最旧的条目

4. **返回 RoslynCodeContext**：
   - 封装主文档的投影信息、编译、语义模型
   - 提供投影偏移量和位置供后续 LSP 操作使用

**源代码引用**：
- 主流程：`InProcRoslynCodeService.cs:854-936`
- 投影构建：`InProcRoslynCodeService.ProjectionAndContext.cs:65-85`
- 位置映射回退：`InProcRoslynCodeService.ProjectionAndContext.cs:28-63`
- 编译缓存：`InProcRoslynCodeService.ProjectionAndContext.cs:302-354`

### 3.2 符号解析（TryResolveSymbol）

**目的**：在投影位置解析 Roslyn 符号（ISymbol）。

**策略**：

1. **候选偏移量枚举**（`EnumerateCandidateOffsets`）：
   ```csharp
   // 尝试多个偏移量以处理边界情况
   projectedOffset,
   projectedOffset - 1,
   projectedOffset + 1,
   projectedOffset - 2,
   projectedOffset + 2,
   ...
   ```

2. **Token 查找**：
   - 在投影 `SyntaxTree` 中查找每个候选偏移量的 `SyntaxToken`

3. **符号解析**（`TryResolveTokenSymbolAtCursor`）：
   - 遍历 Token 的祖先节点，查找以下语法构造：
     - `IdentifierNameSyntax`：标识符引用
     - `GenericNameSyntax`：泛型名称
     - `MemberAccessExpressionSyntax`：成员访问
     - `VariableDeclaratorSyntax`：变量声明
     - 各种声明节点（类型、方法、属性、参数等）
   - 优先使用 `GetSymbolInfo`，回退到 `GetDeclaredSymbol`

4. **返回主符号**（`GetPrimarySymbol`）：
   - 优先返回 `SymbolInfo.Symbol`
   - 回退到 `CandidateSymbols.FirstOrDefault()`

**源代码引用**：
- `InProcRoslynCodeService.SymbolsAndSemantic.cs:179-233`

### 3.3 位置映射（TryMapSpanToOriginalRange）

**目的**：将投影代码中的 `TextSpan` 映射回原始文档位置。

**流程**：

1. 使用 `LspProtocolHelpers.ToRange` 将投影 `TextSpan` 转换为 `LspRange`
2. 调用 `ProjectionMap.TryMapToOriginalRange` 进行逆向映射
3. 返回原始文档中的 `LspRange` 或 `null`（如果映射失败）

**关键点**：
- 所有返回给 LSP 客户端的位置都必须映射回原始文档
- 生成的 Razor 脚手架代码会被过滤掉（无法映射的位置不返回）

**源代码引用**：
- `InProcRoslynCodeService.SymbolsAndSemantic.cs:298-320`

### 3.4 Fallback Completion

**目的**：当 Roslyn 语义分析失败时，基于语法结构提供基础补全。

**触发条件**：
- `LookupVisibleSymbols` 返回空
- `LookupDeclaredTypeMemberSymbols` 返回空

**实现**（`LookupFallbackMemberCompletionItems`）：

1. **检测成员访问**（`TryGetFallbackMemberAccess`）：
   - 查找 `MemberAccessExpressionSyntax`（如 `obj.Property`）
   - 提取 `OwnerName`（如 `obj`）和 `MemberName`（如 `Property`）

2. **查找声明**（`FindFallbackMemberDeclarations`）：
   - 在投影文档中查找与 `OwnerName` 匹配的类型声明
   - 遍历该类型的成员（字段、属性、方法）
   - 匹配 `MemberName` 或前缀

3. **创建补全项**：
   - `Label`：成员名称
   - `Kind`：基于符号类型（Method=2, Property=10, Field=5）
   - `Detail`：完整签名（如 `int PropertyName`）
   - `Documentation`：固定为 `"Fallback"`

**限制**：
- 仅支持简单的成员访问表达式
- 不提供类型信息或文档注释
- 适用于快速失败场景

**源代码引用**：
- `InProcRoslynCodeService.FallbackAndImplementation.cs:346-471`

## 4. 线程安全模型

### 4.1 编译缓存（_compilationCache）

**保护机制**：
- 使用 `Lock _compilationCacheGate` 保护所有缓存访问
- 锁的粒度：整个缓存字典（读写操作）

**并发策略**：
- 多个线程可同时读取缓存（在锁内）
- 写入时独占锁，避免并发创建相同编译

### 4.2 容器名称缓存（ContainerNamesByPath）

**类型**：
```csharp
private static readonly ConcurrentDictionary<string, string> ContainerNamesByPath
```

**并发策略**：
- 使用 `ConcurrentDictionary` 提供无锁并发访问
- `GetOrAdd` 确保相同路径只计算一次哈希

### 4.3 只读状态

**不可变类型**：
- `RoslynCodeContext`（record）
- `ProjectedDocumentContext`（record）
- `CSharpCompilation`、`SyntaxTree`、`SemanticModel`（Roslyn 不可变类型）

**结论**：除编译缓存外，大部分操作无需锁，适合高并发 LSP 请求。

## 5. 错误处理

### 5.1 投影创建失败

**策略**：
- Razor 投影失败 → 回退到 JazorVueParser 投影
- 解析失败 → 返回空结果（不抛出异常）

### 5.2 符号解析失败

**策略**：
- 符号为 `null` → 尝试 Fallback 实现
- Fallback 也失败 → 返回空结果

### 5.3 位置映射失败

**策略**：
- 无法映射到原始文档 → 过滤掉该位置
- 边界情况 → 使用 `TryMapToProjectedPositionWithBoundaryFallback`

### 5.4 编译创建失败

**策略**：
- 元数据引用加载失败 → 记录警告，跳过该引用
- 编译诊断错误 → 通过 `GetDiagnosticsAsync` 返回给客户端

**源代码引用**：
- 元数据引用警告：`InProcRoslynCodeService.SymbolsAndSemantic.cs:1448-1458`

## 6. 配置选项

### 6.1 编译选项

```csharp
private static readonly CSharpParseOptions ParseOptions = new(
    languageVersion: LanguageVersion.CSharp14);
```

**说明**：使用 C# 14 预览版语言特性。

### 6.2 符号显示格式

```csharp
private static readonly SymbolDisplayFormat SymbolDisplayFormat = new(
    genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
    memberOptions: SymbolDisplayMemberOptions.IncludeContainingType |
                   SymbolDisplayMemberOptions.IncludeParameters |
                   SymbolDisplayMemberOptions.IncludeType,
    parameterOptions: SymbolDisplayParameterOptions.IncludeType |
                      SymbolDisplayParameterOptions.IncludeName,
    miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                          SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);
```

**用途**：生成 Hover、SignatureHelp 等功能的显示文本。

### 6.3 缓存大小

```csharp
private const int MaxCompilationCacheEntries = 16;
```

**说明**：最多缓存 16 个编译上下文，使用 LRU 策略淘汰。

## 7. 与其他子系统的交互

### 7.1 Razor 投影服务

**接口**：
```csharp
bool TryCreateProjection(
    DocumentSnapshot document,
    out RazorDesignTimeCodeProjection projection)
```

**用途**：将 Jazor 文件投影为 C# 代码，提供完整的 Razor 语法支持。

**回退策略**：Razor 投影失败时，使用 `JazorVueParser` 解析 `@code` 块。

### 7.2 虚拟文档映射

**类型**：`ProjectionMap`（来自 `Jolt.VirtualDocuments.Mapping`）

**用途**：
- 原始位置 → 投影位置映射
- 投影位置 → 原始位置映射
- 支持多段映射（如 Razor 模板 + @code 块）

### 7.3 工作区解析器

**类型**：`JoltWorkspaceResolver`（来自 `Jolt.Workspace`）

**用途**：
- 查找工作区中的相关文档（`*.cs`, `*.jazor`）
- 获取代码隐藏文件路径（`GetCoLocatedCodeBehindPaths`）
- 规范化路径比较

### 7.4 LSP 协议辅助

**类型**：`LspProtocolHelpers`（来自 `Jolt.Lsp`）

**用途**：
- 位置与偏移量转换（`GetOffset`, `GetPosition`）
- 文档路径转 URI（`ToDocumentUri`）
- Range 创建（`ToRange`）

## 8. 设计权衡

### 8.1 投影策略

**选择**：优先使用 Razor 官方投影，回退到 JazorVueParser。

**权衡**：
- **优点**：Razor 投影提供完整的语法支持和源映射
- **缺点**：依赖 Razor SDK，可能失败或性能开销大
- **回退**：JazorVueParser 仅解析 `@code` 块，轻量但功能有限

### 8.2 编译缓存

**选择**：缓存 16 个编译上下文，使用 LRU 淘汰。

**权衡**：
- **优点**：避免重复编译，提升响应速度
- **缺点**：占用内存，缓存键计算有开销
- **优化**：使用 SHA256 内容哈希确保缓存命中率

### 8.3 符号解析

**选择**：尝试多个候选偏移量，遍历祖先节点。

**权衡**：
- **优点**：处理边界情况（如 token 间隙），提高成功率
- **缺点**：多次查找可能影响性能
- **优化**：使用 `HashSet` 去重，避免重复解析

### 8.4 Fallback 机制

**选择**：提供语法级别的 Fallback Completion 和 Definition。

**权衡**：
- **优点**：在语义分析失败时仍提供基础功能
- **缺点**：功能有限，可能误导用户
- **定位**：作为降级方案，不作为主要路径

## 9. 支持的 LSP 能力

| LSP 能力 | 方法名 | 支持程度 |
|---------|--------|---------|
| Hover | `GetHoverAsync` | ✅ 完整支持 + Fallback |
| Completion | `GetCompletionItemsAsync` | ✅ 完整支持 + Fallback |
| Document Symbols | `GetDocumentSymbolsAsync` | ✅ 完整支持 |
| Semantic Tokens | `GetSemanticTokensAsync` | ✅ 完整支持 |
| Signature Help | `GetSignatureHelpAsync` | ✅ 完整支持 |
| Definition | `GetDefinitionAsync` | ✅ 完整支持 + Fallback |
| Type Definition | `GetTypeDefinitionAsync` | ✅ 完整支持 |
| Implementation | `GetImplementationAsync` | ✅ 完整支持（接口/继承） |
| References | `GetReferencesAsync` | ✅ 完整支持 + Fallback |
| Rename | `GetRenameAsync` | ✅ 完整支持 |
| Diagnostics | `GetDiagnosticsAsync` | ✅ 完整支持 |
| Document Highlights | `GetDocumentHighlightsAsync` | ✅ 完整支持 |
| Call Hierarchy | `PrepareCallHierarchyAsync`<br/>`GetIncomingCallsAsync`<br/>`GetOutgoingCallsAsync` | ✅ 完整支持 |
| Type Hierarchy | `PrepareTypeHierarchyAsync`<br/>`GetTypeHierarchySuperTypesAsync`<br/>`GetTypeHierarchySubTypesAsync` | ✅ 完整支持 |

## 10. 内部 Record 类型总结

| Record 名称 | 用途 | 关键字段 |
|------------|------|---------|
| `RoslynCodeContext` | 单次 LSP 请求的完整上下文 | Document, ProjectedText, SyntaxTree, SemanticModel, Compilation |
| `ProjectedDocumentContext` | 单个投影文档的上下文 | Document, ProjectedText, ProjectionMap, SyntaxTree, SemanticModel |
| `CallHierarchyRangeGroup` | 调用层次的范围分组 | Item, Ranges |
| `FallbackMemberAccess` | Fallback 成员访问 | OwnerName, MemberName |
| `FallbackMemberDeclaration` | Fallback 成员声明 | Document, IdentifierSpan, MemberName, Kind, Display |
| `FallbackMemberLocation` | Fallback 成员位置 | Document, IdentifierSpan, IsDeclaration |

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
