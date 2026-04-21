# 扩展安全策略

> 状态：已实现
> 定位：Jolt 扩展系统的安全验证与沙箱执行机制

## 1. 文档定位

本文档描述 Jolt 扩展系统的安全策略，包括程序集路径限制、SHA256 哈希验证、清单签名验证、沙箱权限模型和运行时路径/主机验证。核心实现在 `src/Jolt/Extensions/ExtensionSecurityPolicy.cs`（约 857 行）和 `src/Jolt/Extensions/ExtensionSandboxProfile.cs`（约 146 行）。

## 2. 核心类型

### 2.1 ExtensionSandboxProfile 沙箱配置

**文件位置**: `src/Jolt/Extensions/ExtensionSandboxProfile.cs`

```csharp
internal sealed class ExtensionSandboxProfile
{
    public required string IoCapability { get; init; }  // "none", "read", "readwrite"
    public required string NetworkCapability { get; init; }  // "none", "loopback", "internet"
    public required string[] ReadRoots { get; init; }
    public required string[] WriteRoots { get; init; }
    public required string[] AllowedHosts { get; init; }

    public bool IsReadPathAllowed(string path) { ... }
    public bool IsWritePathAllowed(string path) { ... }
    public bool IsNetworkHostAllowed(string host) { ... }
}
```

**无限制配置** (`Unrestricted`):
```csharp
public static ExtensionSandboxProfile Unrestricted { get; } = new()
{
    IoCapability = ExtensionHostOptions.IoCapabilityReadWrite,
    NetworkCapability = ExtensionHostOptions.NetworkCapabilityInternet,
    ReadRoots = Array.Empty<string>(),
    WriteRoots = Array.Empty<string>(),
    AllowedHosts = Array.Empty<string>()
};
```

### 2.2 ExtensionSecurityPolicy 安全面具

**文件位置**: `src/Jolt/Extensions/ExtensionSecurityPolicy.cs`

**核心方法**:
- `IsAssemblyHashSatisfied`: SHA256 哈希验证
- `IsManifestSignatureSatisfied`: 清单签名验证
- `IsSandboxPermissionSatisfied`: 沙箱权限验证
- `IsProviderPermissionSatisfied`: Provider 能力验证
- `CreateRuntimeSandboxProfile`: 构建运行时沙箱配置

## 3. 核心算法

### 3.1 程序集路径限制

**方法**: `ExtensionLoader.ResolveAssemblyPath`

**规则**:
```csharp
var combined = Path.IsPathRooted(assemblyPath)
    ? Path.GetFullPath(assemblyPath)
    : Path.GetFullPath(Path.Combine(extensionDirectory, assemblyPath));

if (!IsPathInsideDirectory(normalizedExtensionDirectory, combined))
{
    throw new InvalidOperationException(
        $"Extension assembly path '{assemblyPath}' escapes extension directory.");
}
```

**路径检查** (`IsPathInsideDirectory`):
```csharp
var relativePath = Path.GetRelativePath(directoryPath, candidatePath);
return !string.IsNullOrWhiteSpace(relativePath)
    && !string.Equals(relativePath, "..", StringComparison.Ordinal)
    && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
    && !Path.IsPathRooted(relativePath);
```

**威胁模型**:
- 防止路径遍历攻击（`../../../system.dll`）
- 防止绝对路径逃逸（`C:\Windows\System32\malicious.dll`）
- 强制程序集在扩展目录边界内

### 3.2 SHA256 哈希验证

**方法**: `ExtensionSecurityPolicy.IsAssemblyHashSatisfied`

**流程**:
1. **规范化哈希值** (`NormalizeSha256`):
   ```csharp
   var normalized = value.Trim()
       .Replace(" ", string.Empty)
       .Replace("-", string.Empty)
       .ToUpperInvariant();

   if (normalized.StartsWith("SHA256:", StringComparison.OrdinalIgnoreCase))
       normalized = normalized["SHA256:".Length..];
   ```

2. **计算文件哈希** (`ComputeSha256Hex`):
   ```csharp
   using var stream = File.OpenRead(filePath);
   using var sha256 = SHA256.Create();
   return Convert.ToHexString(sha256.ComputeHash(stream));
   ```

3. **比较哈希值**:
   ```csharp
   var computed = ComputeSha256Hex(assemblyPath);
   return string.Equals(computed, normalizedExpected, StringComparison.OrdinalIgnoreCase);
   ```

**清单声明**:
```json
{
  "assembly": "./MyExtension.dll",
  "assemblySha256": "sha256:ABC123DEF456..."
}
```

**配置控制**:
```csharp
public bool RequireAssemblyHash { get; init; }  // 默认 true
```

### 3.3 清单签名验证

**方法**: `ExtensionSecurityPolicy.IsManifestSignatureSatisfied`

**签名算法**: 仅支持 RS256 (RSA-SHA256 with PKCS#1 v1.5)

**流程**:
1. **验证签名结构**:
   ```csharp
   if (signature is null)
       return false, "manifest signature is missing";

   if (string.IsNullOrWhiteSpace(signature.KeyId))
       return false, "manifest signature keyId is missing";

   if (!string.Equals(signature.Algorithm, "RS256", StringComparison.OrdinalIgnoreCase)
       && !string.Equals(signature.Algorithm, "RSA-SHA256", StringComparison.OrdinalIgnoreCase))
       return false, $"unsupported manifest signature algorithm '{signature.Algorithm}'";
   ```

2. **导入可信公钥** (`TryImportTrustedPublicKey`):
   ```csharp
   if (!trustedPublicKeys.TryGetValue(keyId, out var trustedPublicKeyValue))
       return false, $"trusted public key '{keyId}' is not configured";

   using var rsa = RSA.Create();
   if (trustedPublicKeyValue.Contains("BEGIN PUBLIC KEY"))
       rsa.ImportFromPem(trustedPublicKeyValue);
   else
       rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(trustedPublicKeyValue), out _);
   ```

3. **解码签名值** (`TryDecodeSignatureValue`):
   ```csharp
   try
   {
       signatureBytes = Convert.FromBase64String(signatureValue);
       return true;
   }
   catch (FormatException)
   {
       signatureBytes = Base64UrlDecode(signatureValue);
       return true;
   }
   ```

4. **构建签名载荷** (`BuildManifestSignaturePayload`):
   ```csharp
   var payload = new SortedDictionary<string, object?>(StringComparer.Ordinal)
   {
       ["id"] = manifest.Id?.Trim() ?? string.Empty,
       ["assembly"] = manifest.Assembly?.Trim() ?? string.Empty,
       ["assemblySha256"] = NormalizeSha256(manifest.AssemblySha256 ?? string.Empty),
       ["type"] = manifest.Type?.Trim() ?? string.Empty,
       ["providers"] = manifest.Permissions?.Providers.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
       ["processIsolation"] = manifest.Permissions?.ProcessIsolation ?? false,
       ["io"] = NormalizeIoPermissionForPayload(manifest.Permissions?.Io),
       ["network"] = NormalizeNetworkPermissionForPayload(manifest.Permissions?.Network),
       ["settings"] = NormalizeSettingsForPayload(manifest.Settings)
   };
   return JsonSerializer.Serialize(payload);
   ```

5. **验证签名**:
   ```csharp
   var payloadBytes = Encoding.UTF8.GetBytes(payload);
   var verified = rsa.VerifyData(
       payloadBytes,
       signatureBytes,
       HashAlgorithmName.SHA256,
       RSASignaturePadding.Pkcs1);
   return verified;
   ```

**签名载荷示例**:
```json
{
  "id": "my-extension",
  "assembly": "./MyExtension.dll",
  "assemblySha256": "ABC123...",
  "type": "MyExtension.MyExtensionClass",
  "providers": ["completion", "hover"],
  "processIsolation": false,
  "io": { "level": "read", "readRoots": ["/project"], "writeRoots": [] },
  "network": { "level": "none", "allowedHosts": [] },
  "settings": { "apiKey": "secret" }
}
```

**公钥配置**:
```json
{
  "extensions": {
    "trustedPublicKeys": {
      "key-1": "-----BEGIN PUBLIC KEY-----\nMIIBIjANBgkqhkiG9w...\n-----END PUBLIC KEY-----",
      "key-2": "MIIBCgKCQEA..."
    }
  }
}
```

**清单声明**:
```json
{
  "signature": {
    "keyId": "key-1",
    "algorithm": "RS256",
    "value": "Base64OrBase64UrlEncodedSignature..."
  }
}
```

### 3.4 沙箱权限模型

#### 3.4.1 IO 权限等级

**等级定义** (`ExtensionHostOptions`):
```csharp
public const string IoCapabilityNone = "none";
public const string IoCapabilityRead = "read";
public const string IoCapabilityReadWrite = "readwrite";
```

**权限矩阵**:

| Level | 读取 | 写入 | ReadRoots 默认 | WriteRoots 默认 |
|-------|------|------|----------------|-----------------|
| `none` | ❌ | ❌ | ❌ (禁止声明) | ❌ (禁止声明) |
| `read` | ✅ | ❌ | `/root`, `/extension` | ❌ (禁止声明) |
| `readwrite` | ✅ | ✅ | `/root`, `/extension` | `/extension` |

**验证规则**:
1. 等级不能超出主机限制（`MaxIoCapability`）
2. `none` 级别不能声明 `readRoots`/`writeRoots`
3. `read` 级别不能声明 `writeRoots`
4. 所有路径必须在扩展目录或根目录内

**运行时配置** (`CreateRuntimeSandboxProfile`):
```csharp
if (string.Equals(ioCapability, IoCapabilityNone))
{
    readRoots = Array.Empty<string>();
    writeRoots = Array.Empty<string>();
}
else if (string.Equals(ioCapability, IoCapabilityRead))
{
    if (writeRoots.Length > 0)
        throw new InvalidOperationException("io level 'read' cannot declare writeRoots.");
    if (readRoots.Length == 0)
        readRoots = [normalizedRootDirectory, normalizedExtensionDirectory];
}
else if (string.Equals(ioCapability, IoCapabilityReadWrite))
{
    if (readRoots.Length == 0)
        readRoots = [normalizedRootDirectory, normalizedExtensionDirectory];
    if (writeRoots.Length == 0)
        writeRoots = [normalizedExtensionDirectory];
}
```

#### 3.4.2 网络权限等级

**等级定义**:
```csharp
public const string NetworkCapabilityNone = "none";
public const string NetworkCapabilityLoopback = "loopback";
public const string NetworkCapabilityInternet = "internet";
```

**权限矩阵**:

| Level | HTTP/HTTPS | AllowedHosts 默认 | 主机验证规则 |
|-------|-----------|-------------------|-------------|
| `none` | ❌ | ❌ (禁止声明) | N/A |
| `loopback` | ✅ | `localhost`, `127.0.0.1`, `::1` | 仅允许回环地址，禁止 `*` |
| `internet` | ✅ | `*` (所有主机) | 允许任意主机（除非显式限制） |

**验证规则**:
1. 等级不能超出主机限制（`MaxNetworkCapability`）
2. `none` 级别不能声明 `allowedHosts`
3. `loopback` 级别禁止 `*` 通配符（防止绕过显式主机列表）
4. 主机名必须有效（`Uri.CheckHostName` != `Unknown`）

**运行时配置**:
```csharp
if (string.Equals(networkCapability, NetworkCapabilityNone))
{
    allowedHosts = Array.Empty<string>();
}
else if (string.Equals(networkCapability, NetworkCapabilityLoopback))
{
    if (allowedHosts.Length == 0)
        allowedHosts = ["localhost", "127.0.0.1", "::1"];
    // 禁止 * 通配符
    if (allowedHosts.Contains("*"))
        throw new InvalidOperationException("loopback mode forbids wildcard hosts.");
}
else if (string.Equals(networkCapability, NetworkCapabilityInternet))
{
    if (allowedHosts.Length == 0)
        allowedHosts = ["*"];
}
```

#### 3.4.3 Provider 能力权限

**能力列表** (11 种):
```csharp
public static class ExtensionCapabilityNames
{
    public const string Diagnostic = "diagnostic";
    public const string CodeAction = "codeAction";
    public const string Hover = "hover";
    public const string Completion = "completion";
    public const string DocumentSymbol = "documentSymbol";
    public const string SignatureHelp = "signatureHelp";
    public const string InlayHint = "inlayHint";
    public const string WorkspaceSymbol = "workspaceSymbol";
    public const string FoldingRange = "foldingRange";
    public const string References = "references";
    public const string Rename = "rename";
}
```

**接口到能力映射** (`ExtensionSecurityPolicy.ProviderCapabilityByInterface`):
```csharp
private static readonly IReadOnlyDictionary<Type, string> ProviderCapabilityByInterface = new()
{
    [typeof(ILspDiagnosticProvider)] = ExtensionCapabilityNames.Diagnostic,
    [typeof(ILspCodeActionProvider)] = ExtensionCapabilityNames.CodeAction,
    [typeof(ILspHoverProvider)] = ExtensionCapabilityNames.Hover,
    [typeof(ILspCompletionProvider)] = ExtensionCapabilityNames.Completion,
    [typeof(ILspDocumentSymbolProvider)] = ExtensionCapabilityNames.DocumentSymbol,
    [typeof(ILspSignatureHelpProvider)] = ExtensionCapabilityNames.SignatureHelp,
    [typeof(ILspInlayHintProvider)] = ExtensionCapabilityNames.InlayHint,
    [typeof(ILspWorkspaceSymbolProvider)] = ExtensionCapabilityNames.WorkspaceSymbol,
    [typeof(ILspFoldingRangeProvider)] = ExtensionCapabilityNames.FoldingRange,
    [typeof(ILspReferenceProvider)] = ExtensionCapabilityNames.References,
    [typeof(ILspRenameProvider)] = ExtensionCapabilityNames.Rename
};
```

**验证方法** (`IsProviderPermissionSatisfied`):
```csharp
var providedCapabilities = GetProvidedCapabilities(extensionType);
var allowedCapabilities = NormalizeAllowedCapabilities(manifest);

var deniedCapabilities = providedCapabilities
    .Where(capability => !allowedCapabilities.Contains(capability))
    .Order(StringComparer.OrdinalIgnoreCase)
    .ToArray();

if (deniedCapabilities.Length > 0)
{
    reason = $"provider capability denied: {string.Join(", ", deniedCapabilities)}";
    return false;
}

return true;
```

**清单声明**:
```json
{
  "permissions": {
    "providers": ["completion", "hover", "diagnostic"]
  }
}
```

### 3.5 运行时路径验证

**方法**: `ExtensionSandboxProfile.IsReadPathAllowed`

**流程**:
1. **检查 IO 能力**:
   ```csharp
   if (string.Equals(IoCapability, ExtensionHostOptions.IoCapabilityNone))
       return false;
   ```

2. **验证路径在允许列表内** (`IsPathAllowed`):
   ```csharp
   var normalizedPath = Path.GetFullPath(path);
   foreach (var root in roots)
   {
       var normalizedRoot = Path.GetFullPath(root);
       if (IsPathInsideDirectory(normalizedRoot, normalizedPath)
           || string.Equals(normalizedRoot, normalizedPath, StringComparison.OrdinalIgnoreCase))
       {
           return true;
       }
   }
   return false;
   ```

**目录包含检查** (`IsPathInsideDirectory`):
```csharp
var relativePath = Path.GetRelativePath(directoryPath, candidatePath);
return !string.IsNullOrWhiteSpace(relativePath)
    && !string.Equals(relativePath, "..", StringComparison.Ordinal)
    && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
    && !Path.IsPathRooted(relativePath);
```

**写入权限额外检查** (`IsWritePathAllowed`):
```csharp
if (!string.Equals(IoCapability, ExtensionHostOptions.IoCapabilityReadWrite))
    return false;
```

### 3.6 运行时主机验证

**方法**: `ExtensionSandboxProfile.IsNetworkHostAllowed`

**流程**:
1. **规范化主机名**:
   ```csharp
   var normalizedHost = host.Trim().ToLowerInvariant();
   ```

2. **检查网络能力**:
   ```csharp
   if (string.Equals(NetworkCapability, ExtensionHostOptions.NetworkCapabilityNone))
       return false;
   ```

3. **Loopback 模式特殊处理**:
   ```csharp
   if (string.Equals(NetworkCapability, ExtensionHostOptions.NetworkCapabilityLoopback))
   {
       if (!IsLoopbackHost(normalizedHost))
           return false;

       var effectiveHosts = AllowedHosts.Length == 0
           ? DefaultLoopbackHosts  // ["localhost", "127.0.0.1", "::1"]
           : AllowedHosts;

       if (effectiveHosts.Contains("*", StringComparer.Ordinal))
           return false;  // 禁止通配符绕过

       return effectiveHosts.Contains(normalizedHost, StringComparer.OrdinalIgnoreCase);
   }
   ```

4. **Internet 模式**:
   ```csharp
   if (AllowedHosts.Length == 0 || AllowedHosts.Contains("*"))
       return true;
   return AllowedHosts.Contains(normalizedHost, StringComparer.OrdinalIgnoreCase);
   ```

**回环主机检测** (`IsLoopbackHost`):
```csharp
return string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
    || string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
    || string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase);
```

### 3.7 进程隔离强制策略

**触发条件** (`ValidateProcessIsolationRequirement`):
1. **能力绑定沙箱** (`RequiresCapabilityBoundSandbox`):
   ```csharp
   var ioLevel = NormalizeIoCapability(permissions.Io?.Level) ?? IoCapabilityNone;
   var networkLevel = NormalizeNetworkCapability(permissions.Network?.Level) ?? NetworkCapabilityNone;

   if (!string.Equals(ioLevel, IoCapabilityNone)
       || !string.Equals(networkLevel, NetworkCapabilityNone))
       return true;

   return (readRoots.Length > 0 || writeRoots.Length > 0 || allowedHosts.Length > 0);
   ```

2. **主机策略强制**:
   ```csharp
   if (options.RequireProcessIsolation && manifest.Permissions?.ProcessIsolation != true)
       return false, "a separate worker process is required by host policy";
   ```

**拒绝消息**:
```
a separate worker process is required when io/network capabilities are declared
a separate worker process is required by host policy
```

## 4. 线程安全模型

### 4.1 ExtensionSandboxProfile

**无状态设计**:
- 所有属性为 `init` only（不可变）
- 方法为纯函数（无副作用）
- 无锁需求（只读数据结构）

**使用模式**:
```csharp
var profile = ExtensionSecurityPolicy.CreateRuntimeSandboxProfile(...);
if (profile.IsReadPathAllowed(path))
{
    // 安全访问
}
```

### 4.2 ExtensionSecurityPolicy

**静态方法设计**:
- 所有方法为 `static`
- 无共享状态
- 无锁需求

**线程安全性**: 完全线程安全（无状态）

## 5. 错误处理

### 5.1 验证失败错误

**程序集路径验证失败**:
```
Extension assembly path '../../../malicious.dll' escapes extension directory.
io permission path '/etc/passwd' escapes extension/root boundary.
```

**哈希验证失败**:
```
assembly sha256 verification failed
```

**签名验证失败**:
```
manifest signature is missing
manifest signature keyId is missing
unsupported manifest signature algorithm 'HS256'
trusted public key 'key-1' is not configured
manifest signature value is not valid base64/base64url
trusted public key 'key-1' cannot be parsed
manifest signature verification failed
```

**沙箱权限验证失败**:
```
unsupported io capability 'write'
io capability 'readwrite' exceeds host max 'read'
io level 'none' cannot declare readRoots/writeRoots
io level 'read' cannot declare writeRoots
unsupported network capability 'intranet'
network capability 'internet' exceeds host max 'loopback'
network level 'none' cannot declare allowedHosts
network level 'loopback' does not allow host 'example.com'
invalid network host 'host name'
a separate worker process is required when io/network capabilities are declared
```

**Provider 权限验证失败**:
```
provider capability denied: codeAction, rename
```

### 5.2 运行时沙箱违规

**ExtensionWorkerServer 抛出的异常**:
```csharp
throw new ExtensionWorkerProtocolException(
    ExtensionWorkerErrorCodes.SandboxViolation,
    $"sandbox io read denied for capability '{capability}' path '{documentPath}'.");
```

**错误码**: `SandboxViolation`

**触发场景**:
- Provider 尝试读取未授权路径
- Provider 尝试写入未授权路径
- Provider 返回包含未授权 URI 的结果
- Provider 返回包含未授权主机的网络 URI

## 6. 配置选项

### 6.1 主机策略配置

**示例 1: 默认配置**
```json
{
  "extensions": {
    "enabled": true,
    "requireAssemblyHash": true,
    "requireManifestSignature": true,
    "enforceProviderPermissions": true,
    "requireProcessIsolation": false,
    "maxIoCapability": "read",
    "maxNetworkCapability": "loopback"
  }
}
```

**示例 2: 严格模式**
```json
{
  "extensions": {
    "requireProcessIsolation": true,
    "maxIoCapability": "none",
    "maxNetworkCapability": "none",
    "trustedExtensionIds": ["trusted-extension-1"],
    "trustedPublicKeys": {
      "key-1": "-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----"
    }
  }
}
```

**示例 3: 宽松模式**
```json
{
  "extensions": {
    "requireAssemblyHash": false,
    "requireManifestSignature": false,
    "maxIoCapability": "readwrite",
    "maxNetworkCapability": "internet"
  }
}
```

### 6.2 扩展清单配置示例

**示例 1: 只读扩展**
```json
{
  "manifestVersion": 1,
  "id": "readonly-extension",
  "assembly": "./ReadOnlyExtension.dll",
  "assemblySha256": "sha256:ABC123...",
  "type": "ReadOnlyExtension.Extension",
  "permissions": {
    "providers": ["diagnostic", "hover"],
    "io": {
      "level": "read",
      "readRoots": ["./src", "./lib"]
    },
    "network": {
      "level": "none"
    }
  }
}
```

**示例 2: 网络扩展（Loopback）**
```json
{
  "manifestVersion": 1,
  "id": "lsp-proxy-extension",
  "assembly": "./LspProxyExtension.dll",
  "assemblySha256": "sha256:DEF456...",
  "type": "LspProxyExtension.Extension",
  "permissions": {
    "providers": ["completion", "diagnostic"],
    "io": {
      "level": "none"
    },
    "network": {
      "level": "loopback",
      "allowedHosts": ["localhost", "127.0.0.1"]
    },
    "processIsolation": true
  },
  "signature": {
    "keyId": "key-1",
    "algorithm": "RS256",
    "value": "Base64Signature..."
  }
}
```

## 7. 与其他子系统的交互

### 7.1 与 ExtensionWorkerServer 的交互

**沙箱检查点**:
- Provider 调用前验证输入路径/URI
- Provider 返回后验证输出路径/URI
- 违规时抛出 `ExtensionWorkerProtocolException`

**检查位置**:
```csharp
// 读取权限检查
EnsureReadPathAllowed(sandboxProfile, capability, typedContext.Document);

// 写入权限检查（Code Action 结果）
EnsureWritePathsAllowedForCodeActions(sandboxProfile, capability, actions);

// 网络权限检查（Workspace Symbol 结果）
EnsureNetworkUrisAllowedForWorkspaceSymbols(
    sandboxProfile, capability, symbols, payloadKind: "result");
```

### 7.2 与 ExtensionLoader 的交互

**验证顺序**:
1. 清单格式验证
2. 禁用/信任名单检查
3. 程序集路径验证
4. SHA256 哈希验证
5. 清单签名验证
6. 沙箱权限验证
7. Provider 权限验证

**失败时行为**:
- 记录 `ExtensionLoadInvocation`（状态: `Rejected`）
- 跳过扩展加载
- 继续处理下一个扩展

### 7.3 与 ExtensionRegistry 的交互

**无直接交互**:
- `ExtensionSecurityPolicy` 为静态工具类
- 无状态，无需注册表

## 8. 设计权衡

### 8.1 清单签名 vs 代码签名

**清单签名优势**:
- 无需特殊工具链（标准 JSON + RSA）
- 支持密钥轮换（多个 `KeyId`）
- 灵活（签名包含权限声明）

**代码签名优势**:
- 防篡改（程序集本身签名）
- 操作系统级信任（Windows 证书存储）
- 细粒度身份验证（作者证书）

**当前选择**: 清单签名
- 跨平台兼容性（无需 Windows 证书 API）
- 简化开发流程（无需 Authenticode 工具）
- 满足扩展场景需求（权限验证为主）

### 8.2 沙箱粒度：进程级 vs 方法级

**进程隔离沙箱**:
- 边界：worker 进程
- 检查点：JSON-RPC 请求/响应
- 限制：无法防止扩展内部的直接系统调用

**方法级沙箱**:
- 边界：Provider 方法调用
- 检查点：Provider 参数/返回值
- 限制：仅保护 Jolt-mediated 表面

**当前实现**: 进程隔离 + 方法级检查
- 进程隔离防止崩溃和资源泄漏
- 方法级检查防止 IO/网络违规
- **非完全沙箱**: 扩展仍可直接调用 `File.ReadAllText`（绕过检查）

**改进方向**:
- 使用 `System.AppDomain`（已过时）
- 使用 WebAssembly（未来考虑）
- 使用操作系统级沙箱（Linux namespaces, Windows job objects）

### 8.3 能力声明 vs 运行时发现

**能力声明** (`permissions.providers`):
- 扩展在清单中声明支持的 provider
- 主机验证声明与实际实现匹配
- 优势：显式权限控制
- 劣势：维护成本（清单与代码同步）

**运行时发现**:
- 主机通过反射检测扩展实现的 provider 接口
- 无需清单声明
- 优势：自动化
- 劣势：意外权限扩展

**当前策略**: 混合模式
- 清单声明允许的能力
- 运行时发现实际实现
- 验证实现是否在允许列表内
- `EnforceProviderPermissions = false` 时跳过验证

### 8.4 路径规范化时机

**加载时规范化** (`CreateRuntimeSandboxProfile`):
- 一次性解析相对路径
- 缓存绝对路径
- 优势：运行时性能高
- 劣势：不支持动态路径

**运行时规范化** (`IsReadPathAllowed`):
- 每次检查时规范化
- 优势：支持动态路径
- 劣势：性能开销

**当前实现**: 混合模式
- 加载时规范化 `readRoots`/`writeRoots`
- 运行时规范化待检查路径
- 平衡灵活性与性能
