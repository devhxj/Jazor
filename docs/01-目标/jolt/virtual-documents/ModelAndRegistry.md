# Virtual Documents - Model and Registry

> Status: 活跃参考
> Positioning: Jolt LSP 服务器的核心抽象，用于处理源文档（如 .jazor）到投影文档（如 .cs、.vue）的多文档映射

## 1. 文档定位

Virtual Documents 子系统是 Jolt LSP 实现的基础设施，用于解决「单一源文档 → 多个投影文档」的映射问题。在 Jazor 场景中，一个 `.jazor` 文件需要同时投影为 C# 文档（用于 Razor 编译器）和 Vue 文档（用于 IDE 语言特性）。

该子系统提供：
- **类型安全的身份标识**：通过 `VirtualDocumentIdentity` record 区分源路径、投影路径和文档类型
- **内存中的虚拟文档存储**：`VirtualDocument` 封装了文档内容、版本和坐标映射关系
- **线程安全的注册表**：`InMemoryVirtualDocumentRegistry` 提供并发安全的增删改查操作

## 2. 核心类型

### 2.1 VirtualDocumentKind 枚举

**文件位置**：`src/Jolt/VirtualDocuments/Models/VirtualDocumentKind.cs`

```csharp
public enum VirtualDocumentKind
{
    Jazor,   // 源文档类型（.jazor）
    CSharp,  // 投影的 C# 文档
    Vue      // 投影的 Vue 文档
}
```

**设计说明**：
- 枚举值直接对应 Jazor 项目中的三种文档类型
- `Jazor` 表示用户编辑的源文档
- `CSharp` 和 `Vue` 表示由 Jolt 生成的投影文档

### 2.2 VirtualDocumentIdentity Record

**文件位置**：`src/Jolt/VirtualDocuments/Models/VirtualDocumentIdentity.cs`

```csharp
public sealed record VirtualDocumentIdentity(
    string SourceDocumentPath,      // 源文档路径（如 /path/to/file.jazor）
    string ProjectedDocumentPath,   // 投影文档路径（如 /path/to/file.jazor.cs）
    VirtualDocumentKind DocumentKind);
```

**设计说明**：
- 使用 C# record 类型，确保值语义相等性
- 三元组完整描述了一个虚拟文档的身份
- 支持模式匹配和解构

**使用示例**：
```csharp
var identity = new VirtualDocumentIdentity(
    "/src/App.jazor",
    "/src/App.jazor.cs",
    VirtualDocumentKind.CSharp
);
```

### 2.3 VirtualDocument 类

**文件位置**：`src/Jolt/VirtualDocuments/Models/VirtualDocument.cs`

```csharp
public sealed class VirtualDocument
{
    public VirtualDocument(
        VirtualDocumentIdentity identity,
        string text,
        ProjectionMap projectionMap,
        string? version)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        Text = text ?? throw new ArgumentNullException(nameof(text));
        ProjectionMap = projectionMap ?? throw new ArgumentNullException(nameof(projectionMap));
        Version = version;
    }

    public VirtualDocumentIdentity Identity { get; }
    public string Text { get; }
    public ProjectionMap ProjectionMap { get; }
    public string? Version { get; }
}
```

**设计说明**：
- **不可变性**：所有属性都是只读的，构造后不可修改
- **必填字段**：`Identity`、`Text`、`ProjectionMap` 为必填，`Version` 为可选
- **ProjectionMap**：存储源文档与投影文档之间的坐标映射关系（详见 `ProjectionMap.md`）

**职责划分**：
- `Identity`：文档的身份标识
- `Text`：投影文档的完整文本内容
- `ProjectionMap`：源文档坐标 → 投影文档坐标的双向映射
- `Version`：LSP 文档版本（用于并发编辑冲突检测）

## 3. IVirtualDocumentRegistry 接口

**文件位置**：`src/Jolt/VirtualDocuments/Registry/IVirtualDocumentRegistry.cs`

```csharp
public interface IVirtualDocumentRegistry
{
    // 获取源文档的所有投影文档
    ValueTask<IReadOnlyList<VirtualDocument>> GetBySourceDocumentAsync(
        string sourceDocumentPath,
        CancellationToken cancellationToken);

    // 根据投影文档路径获取单个虚拟文档
    ValueTask<VirtualDocument?> GetByProjectedDocumentAsync(
        string projectedDocumentPath,
        CancellationToken cancellationToken);

    // 插入或更新虚拟文档（批量操作）
    ValueTask UpsertAsync(
        IReadOnlyList<VirtualDocument> virtualDocuments,
        CancellationToken cancellationToken);

    // 删除源文档的所有投影文档
    ValueTask RemoveBySourceDocumentAsync(
        string sourceDocumentPath,
        CancellationToken cancellationToken);
}
```

**设计说明**：
- **异步 API**：使用 `ValueTask` 优化异步操作性能
- **批量操作**：`UpsertAsync` 接受文档列表，支持原子性更新同一源文档的多个投影
- **双向查询**：支持按源文档或投影文档路径查询
- **级联删除**：`RemoveBySourceDocumentAsync` 会删除源文档的所有投影

## 4. InMemoryVirtualDocumentRegistry 实现

**文件位置**：`src/Jolt/VirtualDocuments/Registry/InMemoryVirtualDocumentRegistry.cs`

### 4.1 数据结构

```csharp
private readonly Lock _gate = new();
private readonly Dictionary<string, VirtualDocument> _byProjectedPath =
    new(StringComparer.OrdinalIgnoreCase);
private readonly Dictionary<string, string[]> _projectedPathsBySource =
    new(StringComparer.OrdinalIgnoreCase);
```

**设计说明**：
- **双索引设计**：
  - `_byProjectedPath`：投影路径 → 虚拟文档（快速查询单个投影）
  - `_projectedPathsBySource`：源路径 → 投影路径数组（快速查询源的所有投影）
- **线程安全**：使用 .NET 9 的 `Lock` 类型（而非 `lock` 关键字）提供显式锁
- **路径规范化**：使用 `StringComparer.OrdinalIgnoreCase` 实现大小写不敏感比较

### 4.2 路径规范化

```csharp
private static string NormalizePath(string documentPath)
    => documentPath.Replace('\\', '/');
```

**设计说明**：
- 统一使用正斜杠 `/` 作为路径分隔符
- 处理 Windows 和 Unix 路径差异
- 在所有存储和查询操作前自动规范化

### 4.3 核心操作实现

#### 4.3.1 GetBySourceDocumentAsync

```csharp
public ValueTask<IReadOnlyList<VirtualDocument>> GetBySourceDocumentAsync(
    string sourceDocumentPath,
    CancellationToken cancellationToken)
{
    // 参数验证
    ArgumentNullException.ThrowIfNull(sourceDocumentPath);
    cancellationToken.ThrowIfCancellationRequested();

    // 查询源文档的所有投影路径
    string[]? projectedPaths;
    lock (_gate)
    {
        if (!_projectedPathsBySource.TryGetValue(NormalizePath(sourceDocumentPath), out projectedPaths))
        {
            return ValueTask.FromResult<IReadOnlyList<VirtualDocument>>(Array.Empty<VirtualDocument>());
        }
    }

    // 逐个加载投影文档
    var documents = new List<VirtualDocument>(projectedPaths.Length);
    lock (_gate)
    {
        foreach (var projectedPath in projectedPaths)
        {
            if (_byProjectedPath.TryGetValue(projectedPath, out var document))
            {
                documents.Add(document);
            }
        }
    }

    return ValueTask.FromResult<IReadOnlyList<VirtualDocument>>(documents);
}
```

**设计说明**：
- **分步加锁**：先查询索引，再逐个加载，减少锁持有时间
- **空集合优化**：直接返回 `Array.Empty<VirtualDocument>()` 而非空 `List`
- **防御性编程**：处理投影路径可能已被删除的情况

#### 4.3.2 UpsertAsync

```csharp
public ValueTask UpsertAsync(
    IReadOnlyList<VirtualDocument> virtualDocuments,
    CancellationToken cancellationToken)
{
    // 参数验证
    ArgumentNullException.ThrowIfNull(virtualDocuments);
    cancellationToken.ThrowIfCancellationRequested();

    if (virtualDocuments.Count == 0)
    {
        return ValueTask.CompletedTask;
    }

    // 验证所有文档属于同一源文档
    var sourceDocumentPath = NormalizePath(virtualDocuments[0].Identity.SourceDocumentPath);
    var projectedPaths = new string[virtualDocuments.Count];
    for (var index = 0; index < virtualDocuments.Count; index++)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var virtualDocument = virtualDocuments[index];
        var currentSourceDocumentPath = NormalizePath(virtualDocument.Identity.SourceDocumentPath);
        if (!string.Equals(sourceDocumentPath, currentSourceDocumentPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "All virtual documents in a single upsert must share the same source document path.",
                nameof(virtualDocuments));
        }

        projectedPaths[index] = NormalizePath(virtualDocument.Identity.ProjectedDocumentPath);
    }

    lock (_gate)
    {
        // 清理陈旧的投影文档
        if (_projectedPathsBySource.TryGetValue(sourceDocumentPath, out var previousProjectedPaths))
        {
            var currentProjectedPathSet = projectedPaths.ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var previousProjectedPath in previousProjectedPaths)
            {
                if (!currentProjectedPathSet.Contains(previousProjectedPath))
                {
                    _byProjectedPath.Remove(previousProjectedPath);
                }
            }
        }

        // 插入或更新当前投影文档
        for (var index = 0; index < virtualDocuments.Count; index++)
        {
            _byProjectedPath[projectedPaths[index]] = virtualDocuments[index];
        }

        // 更新源文档的投影路径索引
        _projectedPathsBySource[sourceDocumentPath] = projectedPaths;
    }

    return ValueTask.CompletedTask;
}
```

**设计说明**：
- **原子性保证**：同一源文档的所有投影在一次 `UpsertAsync` 中更新
- **陈旧投影清理**：自动删除源文档之前的投影（避免内存泄漏）
- **源文档一致性验证**：确保批量操作中的所有文档属于同一源文档
- **最后写入胜出**：同一投影路径的新文档会覆盖旧文档

**使用示例**：
```csharp
// 场景：更新 /src/App.jazor 的两个投影
var virtualDocuments = new[]
{
    new VirtualDocument(
        new VirtualDocumentIdentity("/src/App.jazor", "/src/App.jazor.cs", VirtualDocumentKind.CSharp),
        "/* C# code */",
        ProjectionMap.CreateWholeDocument("/src/App.jazor", "/src/App.jazor.cs", 100, 100),
        "1"
    ),
    new VirtualDocument(
        new VirtualDocumentIdentity("/src/App.jazor", "/src/App.jazor.vue", VirtualDocumentKind.Vue),
        "<!-- Vue template -->",
        ProjectionMap.CreateWholeDocument("/src/App.jazor", "/src/App.jazor.vue", 100, 100),
        "1"
    )
};

await registry.UpsertAsync(virtualDocuments, cancellationToken);
```

#### 4.3.3 RemoveBySourceDocumentAsync

```csharp
public ValueTask RemoveBySourceDocumentAsync(
    string sourceDocumentPath,
    CancellationToken cancellationToken)
{
    ArgumentNullException.ThrowIfNull(sourceDocumentPath);
    cancellationToken.ThrowIfCancellationRequested();

    lock (_gate)
    {
        if (_projectedPathsBySource.Remove(NormalizePath(sourceDocumentPath), out var projectedPaths))
        {
            foreach (var projectedPath in projectedPaths)
            {
                _byProjectedPath.Remove(projectedPath);
            }
        }
    }

    return ValueTask.CompletedTask;
}
```

**设计说明**：
- **级联删除**：删除源文档时，自动删除所有相关的投影文档
- **幂等性**：重复删除同一源文档不会抛出异常
- **双向清理**：同时清理两个索引字典

## 5. 线程安全模型

### 5.1 锁策略

- **单一锁**：使用 `Lock _gate` 保护所有共享状态
- **细粒度锁**：部分方法分步加锁（如 `GetBySourceDocumentAsync`），减少锁持有时间
- **避免死锁**：所有方法按固定顺序获取锁，不会嵌套锁

### 5.2 并发场景

| 场景 | 行为 |
|------|------|
| 多个读取操作 | 并发安全，通过 `Lock` 串行化 |
| 读写并发 | 安全，写操作会阻塞读操作 |
| 批量 Upsert + 查询 | 查询可能看到部分更新结果（因为分步加锁） |
| 陈旧投影清理 | 原子性操作，不会出现不一致状态 |

### 5.3 性能考虑

- **字典查找**：O(1) 平均时间复杂度
- **大小写不敏感比较**：使用 `StringComparer.OrdinalIgnoreCase`，避免频繁 `ToLower()`
- **内存占用**：双索引设计会增加内存占用，但换取查询性能

## 6. 错误处理

### 6.1 参数验证

所有方法都执行严格的参数验证：
```csharp
ArgumentNullException.ThrowIfNull(sourceDocumentPath);
cancellationToken.ThrowIfCancellationRequested();
```

### 6.2 业务逻辑验证

`UpsertAsync` 中的额外验证：
```csharp
if (!string.Equals(sourceDocumentPath, currentSourceDocumentPath, StringComparison.OrdinalIgnoreCase))
{
    throw new ArgumentException(
        "All virtual documents in a single upsert must share the same source document path.",
        nameof(virtualDocuments));
}
```

### 6.3 异常传播

所有方法都直接抛出异常，不捕获或转换：
- `ArgumentNullException`：参数为 null
- `OperationCanceledException`：操作被取消
- `ArgumentException`：业务逻辑验证失败

## 7. 配置选项

当前实现无配置选项，所有行为硬编码：
- 路径比较：大小写不敏感
- 路径分隔符：统一使用 `/`
- 线程安全：使用 `Lock` 串行化

## 8. 与其他子系统的交互

### 8.1 LSP 协议层

- **文档同步**：LSP 客户端的 `textDocument/didChange` 通知会触发 `UpsertAsync`
- **坐标转换**：LSP 的位置/范围请求使用 `VirtualDocument.ProjectionMap` 进行坐标映射
- **版本管理**：`VirtualDocument.Version` 对应 LSP 的文档版本号

### 8.2 投影映射引擎

- **依赖关系**：`VirtualDocument` 持有 `ProjectionMap` 实例
- **职责分离**：注册表只负责存储，不负责坐标转换逻辑

### 8.3 文档生成器

- **单向依赖**：文档生成器调用 `UpsertAsync` 更新虚拟文档
- **批量操作**：文档生成器一次性生成同一源文档的所有投影

## 9. 设计权衡

### 9.1 内存存储 vs. 持久化

**当前选择**：纯内存存储
- **优点**：实现简单，性能高，无 I/O 开销
- **缺点**：进程重启后数据丢失，无法跨进程共享
- **适用场景**：Jolt 是单进程 LSP 服务器，生命周期与 IDE 会话绑定

### 9.2 双索引 vs. 单索引扫描

**当前选择**：双索引（`_byProjectedPath` + `_projectedPathsBySource`）
- **优点**：查询性能 O(1)，避免遍历
- **缺点**：内存占用增加，更新时需要同步两个索引
- **适用场景**：读多写少的 LSP 场景

### 9.3 批量 Upsert vs. 单文档 Upsert

**当前选择**：批量 Upsert（接受 `IReadOnlyList<VirtualDocument>`）
- **优点**：保证同一源文档的投影原子性更新，减少锁竞争
- **缺点**：API 稍复杂，调用方需要构造列表
- **适用场景**：文档生成器通常会同时生成多个投影

### 9.4 分步加锁 vs. 持锁

**当前选择**：部分方法分步加锁（如 `GetBySourceDocumentAsync`）
- **优点**：减少锁持有时间，提高并发度
- **缺点**：可能出现不一致的中间状态
- **适用场景**：LSP 场景中，查询通常能容忍短暂的不一致

## 10. 未来扩展方向

### 10.1 可能的改进

1. **持久化缓存**：支持将虚拟文档缓存到磁盘，加速冷启动
2. **分区锁**：按源文档路径分片锁，提高并发度
3. **事件通知**：文档更新时触发事件，支持观察者模式
4. **性能指标**：添加命中率、延迟等监控指标

### 10.2 扩展点

- **路径规范化策略**：当前硬编码为 `Replace('\\', '/')`，可提取为策略
- **比较器选择**：当前固定大小写不敏感，可配置
- **存储后端**：接口抽象允许实现基于 Redis 或数据库的注册表
