# 扩展生命周期管理

> Status: 活跃参考
> Positioning: Jolt 扩展系统的核心加载与注册机制

## 1. 文档定位

本文档描述 Jolt 扩展系统的生命周期管理，包括扩展的发现、加载、激活、停用和清理机制。核心实现在 `src/Jolt/Extensions/ExtensionLoader.cs`（约 1240 行）和 `src/Jolt/Extensions/ExtensionRegistry.cs`（约 809 行）。

## 2. 核心类型

### 2.1 IExtension 接口

**文件位置**: `src/Jolt/Extensions/IExtension.cs`

```csharp
internal interface IExtension
{
    ExtensionMetadata Metadata { get; }

    ValueTask InitializeAsync(ExtensionContext context, CancellationToken cancellationToken);

    ValueTask ActivateAsync(CancellationToken cancellationToken);

    ValueTask DeactivateAsync(CancellationToken cancellationToken);
}
```

**职责**:
- `Metadata`: 扩展元数据（ID、名称、版本、作者、依赖）
- `InitializeAsync`: 接收扩展上下文（根目录、扩展目录、注册表、设置、沙箱配置）
- `ActivateAsync`: 激活扩展（注册 provider 接口）
- `DeactivateAsync`: 停用扩展（清理资源）

### 2.2 ExtensionManifest 清单

**文件位置**: `src/Jolt/Extensions/ExtensionManifest.cs`

```csharp
internal sealed class ExtensionManifest
{
    public int? ManifestVersion { get; init; }
    public string? Id { get; init; }
    public string? Assembly { get; init; }
    public string? AssemblySha256 { get; init; }
    public string? Type { get; init; }
    public ExtensionPermissionManifest? Permissions { get; init; }
    public ExtensionSignatureManifest? Signature { get; init; }
    public Dictionary<string, JsonElement>? Settings { get; init; }
}
```

**清单版本**:
- `v0` (Legacy): 旧版清单格式，支持 `main`, `entryType`, `assemblyPath`, `capabilities` 字段
- `v1` (Current): 当前标准格式，使用 `assembly`, `type`, `permissions` 结构化声明

**权限声明** (`ExtensionPermissionManifest`):
- `Providers`: 允许的 LSP provider 能力列表（11 种）
- `Io`: IO 权限（level, readRoots, writeRoots）
- `Network`: 网络权限（level, allowedHosts）
- `ProcessIsolation`: 是否启用进程隔离

**签名** (`ExtensionSignatureManifest`):
- `KeyId`: 公钥 ID
- `Algorithm`: 签名算法（仅支持 RS256）
- `Value`: Base64/Base64URL 编码的签名值

### 2.3 ExtensionMetadata 元数据

**文件位置**: `src/Jolt/Extensions/ExtensionMetadata.cs`

```csharp
internal sealed record ExtensionMetadata(
    string Id,
    string Name,
    string Version,
    string? Description = null,
    string? Author = null,
    IReadOnlyList<string>? Dependencies = null);
```

## 3. 核心算法

### 3.1 内置扩展加载

**方法**: `ExtensionLoader.LoadBuiltinExtensionsAsync`

**流程**:
1. 遍历内置扩展实例（`IExtension` 对象）
2. 推断扩展目录（程序集所在目录）
3. 调用 `LoadExtensionCoreAsync` 完成加载
4. 失败时抛出异常（内置扩展加载失败被视为致命错误）

**特点**:
- 无需 `extension.json` 清单
- 无需安全验证（可信代码）
- 无需进程隔离（运行在主进程）
- 无法卸载（非可收集加载上下文）

### 3.2 用户扩展加载

**方法**: `ExtensionLoader.LoadUserExtensionsAsync`

**流程**:

#### 3.2.1 清单读取与验证

1. **读取清单**: `TryReadManifest`
   - 解析 `extension.json`
   - 检测清单版本（v0/v1）
   - v0 清单自动迁移到 v1 格式

2. **必需字段验证**:
   - `Id`: 扩展标识符
   - `Assembly`: 程序集路径（相对或绝对）
   - `Type`: 扩展类型全名（命名空间.类名）

3. **禁用/信任名单检查**:
   ```csharp
   if (options.DisabledExtensionIds.Contains(extensionId))
       return Rejected("extension id is disabled by host policy");

   if (options.TrustedExtensionIds.Count > 0
       && !options.TrustedExtensionIds.Contains(extensionId))
       return Rejected("extension id is not in trusted allow-list");
   ```

#### 3.2.2 安全验证链

1. **程序集路径限制** (`ResolveAssemblyPath`):
   - 必须在扩展目录内
   - 禁止路径遍历攻击（`../`）
   - 相对路径自动解析为绝对路径

2. **SHA256 哈希验证** (`ExtensionSecurityPolicy.IsAssemblyHashSatisfied`):
   ```csharp
   if (options.RequireAssemblyHash
       && !VerifyHash(assemblyPath, manifest.AssemblySha256))
       return Rejected("assembly sha256 verification failed");
   ```

3. **清单签名验证** (`ExtensionSecurityPolicy.IsManifestSignatureSatisfied`):
   ```csharp
   if (options.RequireManifestSignature || manifest.Signature is not null)
       if (!VerifySignature(manifest, options.TrustedPublicKeys))
           return Rejected("manifest signature verification failed");
   ```

4. **沙箱权限验证** (`ExtensionSecurityPolicy.IsSandboxPermissionSatisfied`):
   - 检查 IO 能力是否超出主机限制
   - 检查网络能力是否超出主机限制
   - 验证进程隔离要求

#### 3.2.3 加载路径选择

**进程隔离路径** (`Permissions.ProcessIsolation == true`):
1. 创建 `OutOfProcessExtensionProxy`（启动独立 worker 进程）
2. 通过 stdio JSON-RPC 与扩展通信
3. 应用运行时沙箱策略（IO/网络路径验证）

**进程内路径** (`Permissions.ProcessIsolation == false`):
1. 创建 `CollectibleExtensionLoadContext`（可卸载加载上下文）
2. 加载程序集并创建扩展实例
3. 验证扩展类型实现 `IExtension`
4. 验证元数据 ID 与清单 ID 匹配

#### 3.2.4 激活与注册

**方法**: `LoadExtensionCoreAsync`

```csharp
var context = new ExtensionContext(
    rootDirectory,
    extensionDirectory,
    registry,
    settings,
    sandboxProfile);

await extension.InitializeAsync(context, cancellationToken);
await extension.ActivateAsync(cancellationToken);
registry.RegisterExtension(extension);
TrackLoadedExtension(extension, ...);
```

### 3.3 清单迁移（v0 → v1）

**方法**: `ExtensionLoader.MigrateLegacyManifest`

**映射规则**:
| v0 字段 | v1 字段 |
|---------|---------|
| `id` | `id` |
| `assembly` / `main` / `assemblyPath` | `assembly` |
| `assemblySha256` / `assemblyHash` / `sha256` | `assemblySha256` |
| `type` / `entryType` / `typeName` | `type` |
| `capabilities` / `providers` | `permissions.providers` |
| `processIsolation` | `permissions.processIsolation` |
| `io` | `permissions.io` |
| `network` | `permissions.network` |

**检测启发式** (`LooksLikeLegacyManifest`):
```csharp
return root.Contains("main")
    || root.Contains("entryType")
    || root.Contains("assemblyPath")
    || root.Contains("capabilities")
    || root.Contains("processIsolation");
```

### 3.4 扩展清理

**方法**: `ExtensionLoader.DisposeAsync`

**流程**:
1. **逆序停用**: 按加载顺序的反向停用扩展
2. **卸载注册表**: 调用 `registry.UnregisterExtension`
3. **卸载加载上下文**: `CollectibleExtensionLoadContext.Unload()`
4. **强制 GC 终结**: 5 轮 GC + `WaitForPendingFinalizers()`
5. **警告未卸载上下文**: 检测内存泄漏

**示例**:
```csharp
foreach (var loaded in loadedExtensions.AsEnumerable().Reverse())
{
    await loaded.Extension.DeactivateAsync(CancellationToken.None);
    registry.UnregisterExtension(loaded.Extension);
}

var collectibleContexts = loadedExtensions
    .Select(x => x.LoadContext)
    .OfType<CollectibleExtensionLoadContext>()
    .Distinct()
    .ToArray();

foreach (var context in collectibleContexts)
    context.Unload();

for (var cycle = 0; cycle < 5; cycle++)
{
    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
    GC.WaitForPendingFinalizers();
}
```

## 4. 线程安全模型

### 4.1 ExtensionLoader

**锁策略**:
```csharp
private readonly Lock _stateGate = new();
private readonly List<LoadedExtensionState> _loadedExtensions = [];
```

**保护范围**:
- `_loadedExtensions` 列表（添加/清理）
- `_disposed` 状态标志

**无锁操作**:
- 扩展加载（每个扩展独立）
- 注册表操作（`ExtensionRegistry` 自身有锁）

### 4.2 ExtensionRegistry

**锁策略**:
```csharp
private readonly object _gate = new();
private readonly Dictionary<string, IExtension> _extensions = new(StringComparer.OrdinalIgnoreCase);
private readonly List<ILspDiagnosticProvider> _lspDiagnosticProviders = [];
// ... 11 个 provider 列表
```

**保护范围**:
- 扩展注册/注销
- provider 列表的添加/移除/排序
- 健康快照的添加/移除

**读取优化**:
- `GetLspDiagnosticProviders()` 返回快照副本（`ToArray()`）
- 外部持有 iterator 不会阻塞注册

## 5. 错误处理

### 5.1 加载失败分类

**Rejected（拒绝）**: 安全验证失败
- 清单缺失/无效 JSON
- 清单版本不支持
- 必需字段缺失
- 扩展 ID 被禁用
- 扩展 ID 不在信任名单
- 程序集路径验证失败
- SHA256 哈希不匹配
- 清单签名验证失败
- 沙箱权限验证失败

**Failed（失败）**: 运行时异常
- 程序集加载失败（`FileNotFoundException`, `BadImageFormatException`）
- 类型创建失败（`MissingMethodException`, `TargetInvocationException`）
- 初始化/激活异常
- 停用异常（清理时的非致命错误）

### 5.2 错误报告

**方法**: `ExtensionLoader.ReportLoad`

```csharp
var invocation = new ExtensionLoadInvocation(
    ExtensionId: extensionId,
    Source: source,  // "builtin" or "user"
    ExtensionDirectory: extensionDirectory,
    ManifestPath: manifestPath,
    AssemblyPath: assemblyPath,
    Status: status,  // "Loaded", "Rejected", "Failed"
    Reason: reason,
    Timestamp: DateTimeOffset.UtcNow);

_registry.ReportExtensionLoad(invocation);
_loadEventSink?.Invoke(invocation);  // 可选的日志持久化
```

### 5.3 异常处理原则

**静默失败场景**:
- 清理阶段的停用异常（`TryDeactivateSilentlyAsync`）
- 日志接收器异常（隔离扩展加载和可观察性）

**快速失败场景**:
- 内置扩展加载异常（抛出）
- 注册时的 provider 冲突（抛出并回滚）

## 6. 配置选项

### 6.1 ExtensionHostOptions

**文件位置**: `src/Jolt/Extensions/ExtensionHostOptions.cs`

| 配置项 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `Enabled` | bool | true | 是否启用扩展系统 |
| `ExtensionsDirectory` | string | ".jazor/extensions" | 扩展目录路径 |
| `AllowExternalDirectory` | bool | false | 是否允许扩展目录在根目录外 |
| `DisabledExtensionIds` | `IReadOnlySet<string>` | empty | 禁用的扩展 ID 集合 |
| `TrustedExtensionIds` | `IReadOnlySet<string>` | empty | 信任的扩展 ID 集合（白名单模式） |
| `TrustedPublicKeys` | `IReadOnlyDictionary<string, string>` | empty | 信任的公钥（KeyId → PEM/DER） |
| `RequireAssemblyHash` | bool | true | 是否要求程序集 SHA256 哈希验证 |
| `EnforceProviderPermissions` | bool | true | 是否强制执行 provider 权限验证 |
| `RequireManifestSignature` | bool | true | 是否要求清单签名验证 |
| `RequireProcessIsolation` | bool | false | 是否要求所有扩展进程隔离 |
| `MaxIoCapability` | string | "read" | 最大 IO 能力（none/read/readwrite） |
| `MaxNetworkCapability` | string | "loopback" | 最大网络能力（none/loopback/internet） |
| `LoadLogFilePath` | string? | null | 加载日志文件路径 |
| `LoadEventRetention` | int | 200 | 加载事件保留数量（0-10000） |
| `ProviderLogFilePath` | string? | null | Provider 调用日志文件路径 |
| `ProviderEventRetention` | int | 500 | Provider 事件保留数量（0-100000） |

### 6.2 命令行选项覆盖

**解析器**: `ExtensionHostOptionsResolver.Resolve`

**示例**:
```bash
jolt --extensions-enabled=true \
     --extensions-dir="./custom/extensions" \
     --extensions-disabled="bad-extension-1,bad-extension-2" \
     --extensions-trusted="trusted-extension" \
     --extensions-max-io-capability=readwrite \
     --extensions-require-signature=false
```

## 7. 与其他子系统的交互

### 7.1 与 LSP 系统的交互

**Provider 注册**:
- 11 种 LSP provider 接口（`ILspDiagnosticProvider`, `ILspCodeActionProvider`, 等）
- 自动发现扩展实现的 provider 接口
- 按优先级排序（`Priority` 降序，`Name` 升序）

**Provider 调用**:
- `LspSession` 从 `ExtensionRegistry` 获取 provider 列表
- 依次调用每个 provider 的 `ProvideXxxAsync` 方法
- 捕获异常并报告到 `ExtensionRegistry.ReportProviderInvocation`

### 7.2 与 DevServer 的交互

**热重载支持**:
- 进程内扩展可卸载（`CollectibleExtensionLoadContext`）
- 进程隔离扩展通过 worker 重启实现热重载
- 配置变更时重新加载扩展目录

### 7.3 与安全系统的交互

**沙箱执行**:
- 进程隔离扩展的 IO/网络请求在 `ExtensionWorkerServer` 中验证
- 每个能力调用前检查路径/主机权限
- 违反沙箱策略时抛出 `ExtensionWorkerProtocolException`

## 8. 设计权衡

### 8.1 进程隔离 vs 进程内加载

**进程隔离优势**:
- 崩溃隔离（扩展崩溃不影响主进程）
- 强制沙箱（无法绕过 IO/网络检查）
- 可独立卸载（worker 进程终止）

**进程隔离劣势**:
- IPC 开销（stdio JSON-RPC 序列化）
- 启动延迟（worker 进程启动 + 程序集加载）
- 调试复杂度（跨进程调试）

**默认策略**:
- 内置扩展：进程内（可信代码）
- 用户扩展：进程内（默认，性能优先）
- 敏感扩展：进程隔离（声明 `processIsolation: true` 或主机策略要求）

### 8.2 可收集加载上下文 vs 标准加载

**CollectibleAssemblyLoadContext 优势**:
- 支持卸载（释放文件句柄）
- 热重载友好

**可收集上下文限制**:
- 无法卸载可收集上下文外的引用
- GC 终结延迟（需要多轮 GC）
- 某些反射/序列化场景可能导致泄漏

**缓解措施**:
- 5 轮强制 GC + 紧凑
- 未卸载警告（`WriteUnloadWarning`）
- 进程隔离替代方案（进程终止保证释放）

### 8.3 清单迁移 vs 强制升级

**支持 v0 清单的原因**:
- 向后兼容（渐进式升级）
- 降低迁移门槛

**迁移风险**:
- 字段语义差异（如 `capabilities` vs `permissions.providers`）
- 启发式检测误判

**缓解措施**:
- 严格的字段映射规则
- 版本检测拒绝不支持的清单
- 迁移后的清单仍需通过 v1 验证

### 8.4 同步注册 vs 异步注册

**当前实现**: 同步注册（`RegisterExtension` 为同步方法）

**原因**:
- `ActivateAsync` 完成后再注册（已初始化状态）
- 注册表操作为内存字典更新（非 I/O）
- 简化错误处理（无需异步回滚）

**潜在改进**:
- 如果 `ActivateAsync` 变为长时间操作，可考虑异步注册
- 当前设计假设激活为轻量级（注册 provider 回调）
