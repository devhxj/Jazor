# Workspace Store 子系统

> Status: 活跃参考
> Positioning: Jolt 工作区文档存储层，提供内存中的文档快照管理

## 1. 文档定位

Workspace Store 子系统是 Jolt 工作区管理的核心存储抽象，负责维护文档快照（`DocumentSnapshot`）的增删改查操作。该子系统位于 `src/Jolt/Workspace/` 目录下，为 LSP 服务、DevServer 和编译器提供统一的文档访问接口。

核心设计目标：
- 提供线程安全的文档存储
- 支持路径规范化（跨平台兼容）
- 提供文档变更通知机制
- 隔离存储实现与业务逻辑

## 2. 核心类型

### 2.1 `IJoltWorkspaceStore` 接口

**文件位置**：`src/Jolt/Workspace/IJoltWorkspaceStore.cs`

工作区存储的公共接口，定义了文档 CRUD 操作：

```csharp
public interface IJoltWorkspaceStore
{
    // 获取单个文档
    ValueTask<DocumentSnapshot?> GetDocumentAsync(
        string documentPath,
        CancellationToken cancellationToken);

    // 批量获取文档
    ValueTask<IReadOnlyList<DocumentSnapshot>> GetDocumentsAsync(
        IReadOnlyList<string> documentPaths,
        CancellationToken cancellationToken);

    // 获取所有打开的文档
    ValueTask<IReadOnlyList<DocumentSnapshot>> GetOpenDocumentsAsync(
        CancellationToken cancellationToken);

    // 插入或更新文档
    ValueTask UpsertDocumentAsync(
        DocumentSnapshot documentSnapshot,
        CancellationToken cancellationToken);

    // 移除文档
    ValueTask RemoveDocumentAsync(
        string documentPath,
        CancellationToken cancellationToken);
}
```

**设计特点**：
- 所有方法返回 `ValueTask` 以支持异步操作
- 所有方法接受 `CancellationToken` 支持取消
- 批量获取方法减少多次调用的开销
- `UpsertDocumentAsync` 提供幂等性保证

### 2.2 `InMemoryWorkspaceStore` 实现

**文件位置**：`src/Jolt/Workspace/InMemoryWorkspaceStore.cs`

默认的内存存储实现，使用 `ConcurrentDictionary<string, DocumentSnapshot>` 作为底层容器：

```csharp
public sealed class InMemoryWorkspaceStore : IJoltWorkspaceStore
{
    private readonly ConcurrentDictionary<string, DocumentSnapshot> _documents =
        new(StringComparer.OrdinalIgnoreCase);
}
```

**关键特性**：

1. **线程安全**：使用 `ConcurrentDictionary` 确保多线程访问安全
2. **路径规范化**：所有路径在存储前进行规范化处理
3. **大小写不敏感**：使用 `StringComparer.OrdinalIgnoreCase` 支持跨平台

**路径规范化算法**：

```csharp
private static string NormalizeDocumentPath(string documentPath)
{
    return Path.IsPathRooted(documentPath)
        ? Path.GetFullPath(documentPath).Replace('\\', '/')
        : documentPath.Replace('\\', '/');
}
```

- 绝对路径：展开为完整路径，然后统一使用 `/` 分隔符
- 相对路径：保持原样，仅统一分隔符

**文档快照规范化**：

在 `UpsertDocumentAsync` 中，如果路径被规范化，会创建新的 `DocumentSnapshot`：

```csharp
var normalizedSnapshot = string.Equals(normalizedPath, documentSnapshot.DocumentPath, StringComparison.Ordinal)
    ? documentSnapshot
    : new DocumentSnapshot(
        normalizedPath,
        documentSnapshot.DocumentKind,
        documentSnapshot.Text,
        documentSnapshot.Version);
```

### 2.3 `IWorkspaceDocumentChangeSink` 接口

**文件位置**：`src/Jolt/Workspace/IWorkspaceDocumentChangeSink.cs`

文档变更通知的观察者接口：

```csharp
internal interface IWorkspaceDocumentChangeSink
{
    ValueTask OnWorkspaceDocumentChangedAsync(
        DocumentSnapshot document,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        CancellationToken cancellationToken);
}
```

**设计意图**：
- 在文档变更时触发副作用（如重新分析、增量编译）
- 传入完整的打开文档列表，支持跨文档分析
- 异步接口避免阻塞存储操作

### 2.4 `NullWorkspaceDocumentChangeSink`

**文件位置**：`src/Jolt/Workspace/IWorkspaceDocumentChangeSink.cs`

空对象模式（Null Object Pattern）实现：

```csharp
internal sealed class NullWorkspaceDocumentChangeSink : IWorkspaceDocumentChangeSink
{
    public static NullWorkspaceDocumentChangeSink Instance { get; } = new();

    public ValueTask OnWorkspaceDocumentChangedAsync(
        DocumentSnapshot document,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}
```

**使用场景**：
- 当不需要变更通知时，避免空引用检查
- 单例模式减少内存分配

### 2.5 `DocumentSnapshot` 类型

**文件位置**：`src/Jazor.RazorVue/Protocol/` （来自共享契约）

文档快照的数据结构：

```csharp
public class DocumentSnapshot
{
    public string DocumentPath { get; }
    public DocumentKind DocumentKind { get; }
    public string Text { get; }
    public string? Version { get; }
}
```

- `DocumentPath`：规范化的文档路径
- `DocumentKind`：文档类型（Jazor、CSharp、Vue、JavaScript、TypeScript、Css、Unknown）
- `Text`：文档内容
- `Version`：文档版本字符串（可选，用于 LSP/宿主版本管理）

## 3. 核心算法

### 3.1 文档检索流程

**单个文档获取**：

```
GetDocumentAsync(documentPath)
    ↓
参数验证（ArgumentNullException、CancellationCheck）
    ↓
路径规范化（NormalizeDocumentPath）
    ↓
ConcurrentDictionary.TryGetValue
    ↓
返回 DocumentSnapshot?（可能为 null）
```

**批量文档获取**：

```
GetDocumentsAsync(documentPaths)
    ↓
参数验证
    ↓
遍历 documentPaths
    ↓
对每个路径：
    - 规范化路径
    - 检查取消标志
    - TryGetValue 查找
    - 如果找到，添加到结果列表
    ↓
返回 IReadOnlyList<DocumentSnapshot>
```

**性能优化**：
- 预分配列表容量：`new List<DocumentSnapshot>(documentPaths.Count)`
- 在循环中检查取消标志：`cancellationToken.ThrowIfCancellationRequested()`

### 3.2 文档更新流程

**Upsert 语义**：

```
UpsertDocumentAsync(documentSnapshot)
    ↓
参数验证
    ↓
路径规范化（NormalizeDocumentPath）
    ↓
如果路径被规范化，创建新的 DocumentSnapshot
    ↓
_documents[normalizedPath] = normalizedSnapshot
    ↓
返回 ValueTask.CompletedTask
```

**幂等性保证**：
- 相同路径的多次更新直接覆盖
- 路径规范化确保一致性

### 3.3 打开文档列表维护

**获取打开文档**：

```
GetOpenDocumentsAsync()
    ↓
检查取消标志
    ↓
_documents.Values.OrderBy(DocumentPath)
    ↓
转换为数组（IReadOnlyList）
    ↓
返回排序后的文档列表
```

**排序规则**：
- 使用 `StringComparer.OrdinalIgnoreCase` 排序
- 确保跨平台一致性

## 4. 线程安全模型

### 4.1 并发访问控制

**底层容器**：
- 使用 `ConcurrentDictionary<string, DocumentSnapshot>`
- 所有操作都是线程安全的
- 读操作无锁，写操作使用细粒度锁

**不变性保证**：
- `DocumentSnapshot` 对象本身是不可变的
- 更新操作创建新对象而非修改现有对象

### 4.2 并发场景

| 操作类型 | 并发行为 | 保证 |
|---------|---------|------|
| 多个读操作 | 完全并发 | 无锁，高性能 |
| 读 + 写操作 | 完全并发 | 读操作可能看到旧值或新值 |
| 多个写操作 | 串行化键级别 | 最后一次写入生效 |

### 4.3 取消支持

所有公共方法都支持取消：

```csharp
cancellationToken.ThrowIfCancellationRequested();
```

**取消检查点**：
- 方法入口处
- 批量操作的每次迭代

## 5. 错误处理

### 5.1 参数验证

**空值检查**：

```csharp
ArgumentNullException.ThrowIfNull(documentPath);
ArgumentNullException.ThrowIfNull(documentSnapshot);
ArgumentNullException.ThrowIfNull(documentPaths);
```

**取消检查**：

```csharp
cancellationToken.ThrowIfCancellationRequested();
```

### 5.2 路径处理

**路径规范化安全**：
- 使用 `Path.GetFullPath` 处理相对路径
- 替换 `\\` 为 `/` 确保跨平台兼容
- 不抛出路径相关异常（由 `Path` 类处理）

### 5.3 错误传播

**存储失败**：
- `ConcurrentDictionary` 操作不抛出异常（除内存不足）
- 返回 `null` 表示文档不存在

**版本冲突**：
- 不进行版本检查
- 最后一次写入覆盖所有先前版本

## 6. 配置选项

### 6.1 路径比较策略

**当前实现**：`StringComparer.OrdinalIgnoreCase`

**影响**：
- Windows 和 Linux 上路径匹配行为一致
- 大小写不敏感的路径查找

### 6.2 存储容量

**当前实现**：无限制内存增长

**潜在问题**：
- 长时间运行可能导致内存泄漏
- 大型工作区可能消耗大量内存

**未来改进方向**：
- 添加 LRU 缓存策略
- 实现文档数量上限
- 提供未使用文档的自动清理

## 7. 与其他子系统的交互

### 7.1 LSP 服务交互

**LspSession** 使用工作区存储：

```
LspSession.DidOpenTextDocument
    ↓
IJoltWorkspaceStore.UpsertDocumentAsync
    ↓
存储文档快照

LspSession.DidChangeTextDocument
    ↓
IJoltWorkspaceStore.UpsertDocumentAsync
    ↓
更新文档快照（版本递增）

LspSession.DidCloseTextDocument
    ↓
IJoltWorkspaceStore.RemoveDocumentAsync
    ↓
移除文档快照
```

### 7.2 DevServer 交互

**OnDemandCompiler** 使用工作区存储：

```
OnDemandCompiler.CompileRequest
    ↓
IJoltWorkspaceStore.GetDocumentAsync
    ↓
获取最新文档内容
    ↓
执行编译
```

### 7.3 编译器交互

**BuildOrchestrator** 使用工作区存储：

```
BuildOrchestrator.IncrementalBuild
    ↓
IJoltWorkspaceStore.GetOpenDocumentsAsync
    ↓
获取所有打开的文档
    ↓
检测变更并触发增量编译
```

### 7.4 工作区解析器交互

**JoltWorkspaceResolver** 使用工作区存储：

```
JoltWorkspaceResolver.ResolveDocumentAsync
    ↓
优先从 IJoltWorkspaceStore.GetDocumentsAsync 查找
    ↓
如果未找到，回退到文件系统
    ↓
返回文档快照或 null
```

## 8. 设计权衡

### 8.1 内存 vs 持久化

**当前选择**：纯内存存储

**优点**：
- 极快的访问速度（内存查找）
- 无 I/O 开销
- 简单的并发控制

**缺点**：
- 进程重启后数据丢失
- 内存消耗随工作区大小增长
- 无持久化历史记录

**适用场景**：
- 文档内容由外部系统（LSP 客户端、DevServer）维护
- 重启后可以从磁盘重新加载
- 不需要跨会话的文档历史

### 8.2 路径规范化时机

**当前选择**：存储时规范化，查询时也规范化

**优点**：
- 存储键始终一致
- 查询时无需多次规范化

**缺点**：
- 存储时需要创建新的 `DocumentSnapshot` 对象
- 增加少量内存分配

**替代方案**：
- 仅查询时规范化（需要多次规范化，但减少存储时分配）
- 延迟规范化（首次访问时规范化并缓存）

### 8.3 文档变更通知

**当前选择**：独立的 `IWorkspaceDocumentChangeSink` 接口

**优点**：
- 关注点分离（存储 vs 通知）
- 支持多个订阅者
- 空对象模式避免条件检查

**缺点**：
- 需要手动管理订阅关系
- 增加接口复杂度

**替代方案**：
- 事件模型（.NET event）
- 反应式扩展（Rx）
- 集成到存储接口中（返回变更事件）

### 8.4 批量操作支持

**当前选择**：提供 `GetDocumentsAsync` 批量获取

**优点**：
- 减少多次调用的开销
- 支持原子性读取（所有文档在同一时间点）

**缺点**：
- 不支持批量 Upsert（需要多次调用）
- 不支持事务性操作

**未来改进**：
- 添加 `UpsertDocumentsAsync` 批量更新
- 支持批量删除
- 提供事务性批量操作

### 8.5 版本管理

**当前选择**：`DocumentSnapshot.Version` 为可空字段，存储层不强制版本递增

**优点**：
- 灵活的版本控制策略
- 支持无版本的文档（如磁盘文件）

**缺点**：
- 无版本冲突检测
- 可能覆盖更新的版本

**适用场景**：
- LSP 客户端负责版本管理
- DevServer 使用文件系统时间戳
- 编译器不关心版本号

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**参考源文件**：
- `src/Jolt/Workspace/IJoltWorkspaceStore.cs`
- `src/Jolt/Workspace/InMemoryWorkspaceStore.cs`
- `src/Jolt/Workspace/IWorkspaceDocumentChangeSink.cs`
