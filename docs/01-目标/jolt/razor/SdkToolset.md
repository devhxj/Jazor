# Razor SDK 工具集解析器

> 状态：已实现
> 定位：Jolt Razor SDK 发现与定位核心服务

## 1. 文档定位

本文档描述 Razor SDK 工具集解析器的实现，这是 Jolt 项目中用于发现和定位 .NET SDK 及 Razor SDK 的核心服务。该服务通过多路径搜索策略找到可用的 Razor SDK，提供版本管理和路径解析功能。

**源文件位置**：
- `src/Jolt/Razor/Toolset/RazorSdkToolset.cs`（record 定义，11 行）
- `src/Jolt/Razor/Toolset/RazorSdkToolsetResolver.cs`（核心解析器，385 行）
- `src/Jolt/Razor/Toolset/RazorSdkToolsetHost.cs`（包装器，38 行）

## 2. 核心类型

### 2.1 RazorSdkToolset（record）

封装 Razor SDK 的所有路径信息：

```csharp
internal sealed record RazorSdkToolset(
    string RootPath,                      // .NET SDK 根路径
    string SdkVersion,                    // SDK 版本（如 "9.0.101"）
    string SdkRootPath,                   // 特定版本 SDK 路径
    string RazorSdkRootPath,              // Razor SDK 根路径
    string RazorSourceGeneratorPath,      // Razor 源生成器 DLL 路径
    string RazorTasksPath,                // Razor 任务 DLL 路径
    string RazorDesignTimeTargetsPath,    // Razor 设计时目标文件路径
    string RazorComponentTargetsPath);    // Razor 组件目标文件路径
```

**路径示例**：
- `RootPath`: `C:\Program Files\dotnet`
- `SdkVersion`: `9.0.101`
- `SdkRootPath`: `C:\Program Files\dotnet\sdk\9.0.101`
- `RazorSdkRootPath`: `C:\Program Files\dotnet\sdk\9.0.101\Sdks\Microsoft.NET.Sdk.Razor`
- `RazorSourceGeneratorPath`: `...\source-generators\Microsoft.CodeAnalysis.Razor.Compiler.dll`
- `RazorTasksPath`: `...\tasks\net10.0\Microsoft.NET.Sdk.Razor.Tasks.dll`
- `RazorDesignTimeTargetsPath`: `...\targets\Microsoft.NET.Sdk.Razor.DesignTime.targets`
- `RazorComponentTargetsPath`: `...\targets\Microsoft.NET.Sdk.Razor.Component.targets`

### 2.2 RazorSdkToolsetResolver

核心解析器，负责发现和验证 Razor SDK。

**依赖项**：无（纯文件系统操作）

**关键常量**：
```csharp
private const string RazorSdkName = "Microsoft.NET.Sdk.Razor";
private const string BundledSdkEnvironmentVariable = "JOLT_DOTNET_ROOT";
private const string BundledSdkVersionEnvironmentVariable = "JOLT_DOTNET_SDK_VERSION";
```

### 2.3 RazorSdkToolsetHost

解析器的包装器，提供便捷的 API 和描述功能。

**依赖项**：
- `RazorSdkToolsetResolver _resolver`（可注入）

## 3. 核心算法

### 3.1 SDK 解析流程（Resolve）

**目的**：按优先级顺序查找可用的 Razor SDK。

**流程**：

1. **枚举候选根路径**（`EnumerateRoots`）：
   ```csharp
   foreach (var candidate in EnumerateRoots())
   {
       var toolset = TryResolveFromRoot(candidate.RootPath, candidate.Version);
       if (toolset is not null)
       {
           return toolset;
       }
   }
   return null;
   ```

2. **优先级顺序**：
   - ** bundled SDK（环境变量 `JOLT_DOTNET_ROOT`）**
   - **应用本地 SDK**（`AppContext.BaseDirectory/dotnet`）
   - **全局 SDK**（环境变量、默认路径、`dotnet --info`）

3. **版本选择**：
   - 如果指定了 `JOLT_DOTNET_SDK_VERSION`，优先匹配该版本
   - 否则使用 `SdkVersionComparer` 排序，选择最高版本

**源代码引用**：
- `RazorSdkToolsetResolver.cs:27-39`

### 3.2 候选根路径枚举（EnumerateRoots）

**目的**：按优先级枚举所有可能的 .NET SDK 根路径。

**路径来源**：

#### 3.2.1 Bundled SDK（最高优先级）

```csharp
var bundledRoot = Environment.GetEnvironmentVariable(BundledSdkEnvironmentVariable);
var bundledVersion = Environment.GetEnvironmentVariable(BundledSdkVersionEnvironmentVariable);
if (TryAddRoot(bundledRoot, bundledVersion, seen, out var bundledCandidate))
{
    yield return bundledCandidate;
}
```

**用途**：支持 Jolt 自带 .NET Runtime 的场景。

#### 3.2.2 应用本地 SDK

```csharp
var appBaseDirectory = AppContext.BaseDirectory;
if (TryAddRoot(Path.Combine(appBaseDirectory, "dotnet"), version: null, seen, out var localCandidate))
{
    yield return localCandidate;
}
```

**路径示例**：`D:\repository\own\jazor\Jazor\src\Jolt\bin\Debug\net10.0\dotnet`

#### 3.2.3 全局 SDK（多路径搜索）

**Windows 环境**：
```csharp
var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
yield return Path.Combine(programFiles, "dotnet");

var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
yield return Path.Combine(programFilesX86, "dotnet");
```

**非 Windows 环境**：
```csharp
private static readonly string[] NonWindowsDotNetRoots =
[
    "/usr/share/dotnet",
    "/usr/local/share/dotnet",
    "/usr/lib/dotnet",
    "/usr/lib64/dotnet",
    "/opt/dotnet"
];

var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
yield return Path.Combine(homeDirectory, ".dotnet");
```

**环境变量**：
```csharp
private static readonly string[] DotNetRootEnvironmentVariables =
[
    "JOLT_DOTNET_ROOT",
    "DOTNET_ROOT",
    "DOTNET_ROOT_X64",
    "DOTNET_ROOT_ARM64",
    "DOTNET_ROOT(x86)"
];
```

**动态发现**（`TryGetDotNetRootFromInfo`）：
```csharp
using var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = "--info",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    }
};
// 解析 "Base Path: C:\Program Files\dotnet\sdk\9.0.101\" 提取根路径
```

**源代码引用**：
- 主流程：`RazorSdkToolsetResolver.cs:41-65`
- 默认路径枚举：`RazorSdkToolsetResolver.cs:125-169`

### 3.3 根路径解析（TryResolveFromRoot）

**目的**：从给定的根路径解析 Razor SDK 工具集。

**流程**：

1. **验证 SDK 目录**：
   ```csharp
   var sdkRoot = Path.Combine(rootPath, "sdk");
   if (!Directory.Exists(sdkRoot))
   {
       return null;
   }
   ```

2. **枚举版本**：
   ```csharp
   var versions = Directory.GetDirectories(sdkRoot)
       .Select(Path.GetFileName)
       .Where(static version => !string.IsNullOrWhiteSpace(version))
       .Select(static version => version!)
       .OrderByDescending(static version => version, SdkVersionComparer.Instance)
       .ToArray();
   ```

3. **版本排序**：
   - 如果有 `preferredVersion`，优先匹配该版本
   - 否则使用 `SdkVersionComparer` 按版本号降序排序

4. **验证 Razor SDK**：
   ```csharp
   foreach (var version in versions)
   {
       var versionRoot = Path.Combine(sdkRoot, version);
       var razorSdkRoot = Path.Combine(versionRoot, "Sdks", RazorSdkName);
       var sourceGeneratorPath = Path.Combine(razorSdkRoot, "source-generators", "Microsoft.CodeAnalysis.Razor.Compiler.dll");
       var tasksPath = Path.Combine(razorSdkRoot, "tasks", "net10.0", "Microsoft.NET.Sdk.Razor.Tasks.dll");
       var designTimeTargetsPath = Path.Combine(razorSdkRoot, "targets", "Microsoft.NET.Sdk.Razor.DesignTime.targets");
       var componentTargetsPath = Path.Combine(razorSdkRoot, "targets", "Microsoft.NET.Sdk.Razor.Component.targets");

       if (!File.Exists(sourceGeneratorPath)
           || !File.Exists(tasksPath)
           || !File.Exists(designTimeTargetsPath)
           || !File.Exists(componentTargetsPath))
       {
           continue;
       }

       return new RazorSdkToolset(...);
   }
   ```

**验证逻辑**：
- 所有 4 个关键文件必须存在
- 否则跳过该版本，尝试下一个版本

**源代码引用**：`RazorSdkToolsetResolver.cs:67-123`

### 3.4 SDK 版本比较（SdkVersionComparer）

**目的**：比较 .NET SDK 版本号，支持语义化版本排序。

**比较规则**：

1. **解析版本**：
   ```csharp
   private static bool TryParse(string versionText, out Version? version, out string prereleaseLabel)
   {
       version = null;
       prereleaseLabel = string.Empty;

       var separatorIndex = versionText.IndexOf('-', StringComparison.Ordinal);
       var numericPart = separatorIndex >= 0 ? versionText[..separatorIndex] : versionText;
       prereleaseLabel = separatorIndex >= 0 ? versionText[(separatorIndex + 1)..] : string.Empty;

       return Version.TryParse(numericPart, out version);
   }
   ```

   **示例**：
   - `"9.0.101"` → version=`9.0.101`, prerelease=`""`
   - `"9.0.101-preview.5"` → version=`9.0.101`, prerelease=`"preview.5"`

2. **比较逻辑**：
   ```csharp
   var leftParsed = TryParse(left, out var leftVersion, out var leftPrerelease);
   var rightParsed = TryParse(right, out var rightVersion, out var rightPrerelease);

   if (leftParsed && rightParsed)
   {
       var versionComparison = leftVersion!.CompareTo(rightVersion);
       if (versionComparison != 0)
       {
           return versionComparison;  // 版本号大的优先
       }

       if (leftPrerelease.Length == 0 && rightPrerelease.Length > 0)
       {
           return 1;  // 正式版 > 预览版
       }

       if (leftPrerelease.Length > 0 && rightPrerelease.Length == 0)
       {
           return -1;  // 预览版 < 正式版
       }

       var prereleaseComparison = string.Compare(leftPrerelease, rightPrerelease, StringComparison.OrdinalIgnoreCase);
       if (prereleaseComparison != 0)
       {
           return prereleaseComparison;  // 预览标签按字母序
       }
   }
   ```

**排序示例**：
- `"9.0.101"` > `"9.0.101-preview.5"` > `"9.0.100"` > `"8.0.401"`

**源代码引用**：`RazorSdkToolsetResolver.cs:309-384`

### 3.5 dotnet --info 解析（TryParseDotNetRootFromInfoOutput）

**目的**：从 `dotnet --info` 输出中提取 .NET 根路径。

**输入示例**：
```
.NET SDK:
 Version:   9.0.101
 Commit:    12345678

运行时环境:
 OS Name:     Windows
 OS Version:  10.0.26200
 Base Path:   C:\Program Files\dotnet\sdk\9.0.101\

Host:
  Version:      9.0.1
  Architecture: x64
  Commit:       87654321
```

**解析逻辑**：
```csharp
foreach (var line in infoOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
{
    var trimmed = line.Trim();
    if (!trimmed.StartsWith("Base Path:", StringComparison.OrdinalIgnoreCase))
    {
        continue;
    }

    var basePath = trimmed["Base Path:".Length..].Trim().Trim('"');
    var normalizedBasePath = basePath.Replace('\\', '/').TrimEnd('/');
    var sdkSegmentIndex = normalizedBasePath.LastIndexOf("/sdk/", StringComparison.OrdinalIgnoreCase);

    var rootPath = normalizedBasePath[..sdkSegmentIndex];
    return basePath.Contains('\\')
        ? rootPath.Replace('/', '\\')
        : rootPath;
}
```

**输出**：`C:\Program Files\dotnet`

**源代码引用**：`RazorSdkToolsetResolver.cs:171-206`

## 4. 线程安全模型

### 4.1 无状态设计

**特点**：
- `RazorSdkToolsetResolver` 是无状态的
- 所有方法都是纯函数（输入 → 输出）
- 不维护缓存或可变状态

### 4.2 文件系统访问

**特点**：
- 使用 `Directory.Exists`, `File.Exists`, `Directory.GetDirectories` 等只读操作
- 不修改文件系统

**结论**：整个服务是线程安全的，适合并发调用。

## 5. 错误处理

### 5.1 进程启动失败

**策略**：
```csharp
try
{
    using var process = new Process { ... };
    if (!process.Start())
    {
        return null;
    }
    ...
}
catch (InvalidOperationException) { return null; }
catch (Win32Exception) { return null; }
catch (IOException) { return null; }
```

**原因**：`dotnet` 命令可能不存在或无权限执行。

### 5.2 超时处理

**策略**：
```csharp
if (!process.WaitForExit((int)TimeSpan.FromSeconds(3).TotalMilliseconds))
{
    TryTerminate(process);
    return null;
}
```

**超时时间**：3 秒

### 5.3 路径验证失败

**策略**：
- 使用 `TryAddRoot` 包装路径解析
- 捕获 `ArgumentException`, `IOException`, `NotSupportedException`
- 失败的路径被静默跳过

### 5.4 进程终止失败

**策略**：
```csharp
private static void TryTerminate(Process process)
{
    try
    {
        process.Kill(entireProcessTree: true);
    }
    catch (InvalidOperationException) { }
    catch (NotSupportedException) { }
}
```

## 6. 配置选项

### 6.1 环境变量

| 变量名 | 用途 | 示例 |
|-------|------|------|
| `JOLT_DOTNET_ROOT` | Bundled .NET SDK 根路径 | `C:\path\to\jolt\runtime\dotnet` |
| `JOLT_DOTNET_SDK_VERSION` | Bundled SDK 版本 | `9.0.101` |
| `DOTNET_ROOT` | 全局 .NET 根路径（x64） | `C:\Program Files\dotnet` |
| `DOTNET_ROOT_X64` | 全局 .NET 根路径（x64 显式） | `C:\Program Files\dotnet` |
| `DOTNET_ROOT_ARM64` | 全局 .NET 根路径（ARM64） | `C:\Program Files\dotnet` |
| `DOTNET_ROOT(x86)` | 全局 .NET 根路径（x86） | `C:\Program Files (x86)\dotnet` |

### 6.2 Razor SDK 名称

```csharp
private const string RazorSdkName = "Microsoft.NET.Sdk.Razor";
```

**说明**：.NET SDK 中 Razor SDK 的固定目录名。

### 6.3 目标框架版本

```csharp
var tasksPath = Path.Combine(razorSdkRoot, "tasks", "net10.0", "Microsoft.NET.Sdk.Razor.Tasks.dll");
```

**说明**：当前硬编码为 `net10.0`，未来可能需要动态解析。

## 7. 与其他子系统的交互

### 7.1 RazorDesignTimeCodeProjectionService

**依赖**：
```csharp
public RazorDesignTimeCodeProjectionService(RazorSdkToolsetHost? toolsetHost = null)
{
    _requireSdkAlignedProjection = toolsetHost is not null;
    _resolvedToolset = toolsetHost?.ResolveToolset();
}
```

**用途**：
- 如果提供 `toolsetHost`，则要求 SDK 对齐的投影
- 否则使用宽松的投影模式（允许 Fallback）

### 7.2 JoltWorkspaceResolver

**间接交互**：
- `RazorSdkToolsetResolver` 不直接依赖 `JoltWorkspaceResolver`
- 但两者都使用 `OperatingSystem.IsWindows()` 进行平台判断
- 路径比较器逻辑一致（Windows 不区分大小写）

## 8. 设计权衡

### 8.1 多路径搜索策略

**选择**：按优先级枚举多个候选路径，返回第一个可用的。

**权衡**：
- **优点**：适应各种部署场景（bundled、本地、全局）
- **缺点**：可能有性能开销（多次文件系统访问）
- **优化**：使用 `HashSet<string>` 去重，避免重复路径

### 8.2 版本比较算法

**选择**：自定义 `SdkVersionComparer`，支持语义化版本和预览标签。

**权衡**：
- **优点**：精确的版本排序，符合 .NET 生态约定
- **缺点**：实现复杂，需要手动解析版本字符串
- **替代方案**：使用 `Version.Parse()`，但不支持预览标签

### 8.3 dotnet --info 动态发现

**选择**：作为最后的回退手段，通过运行 `dotnet --info` 发现 SDK。

**权衡**：
- **优点**：适用于环境变量未配置的场景
- **缺点**：启动进程有开销，超时时间较长（3 秒）
- **优化**：仅在静态路径失败时才调用

### 8.4 硬编码 net10.0

**选择**：当前硬编码 Razor 任务路径为 `net10.0`。

**权衡**：
- **优点**：简单直接，适用于当前 .NET 10 目标
- **缺点**：未来升级到 .NET 11 需要修改代码
- **未来改进**：动态解析 `tasks` 目录下的子目录

### 8.5 严格验证策略

**选择**：要求所有 4 个关键文件存在才认为 SDK 可用。

**权衡**：
- **优点**：确保完整性，避免部分损坏的 SDK
- **缺点**：可能过于严格，某些边缘部署场景可能失败
- **考虑**：未来可提供宽松模式（仅要求核心文件）

## 9. 完整示例

### 9.1 Windows 环境

**环境变量**：
```
DOTNET_ROOT=C:\Program Files\dotnet
JOLT_DOTNET_ROOT=D:\jolt\runtime\dotnet
JOLT_DOTNET_SDK_VERSION=9.0.101
```

**解析流程**：
1. 尝试 `D:\jolt\runtime\dotnet`（bundled，版本 `9.0.101`）
2. 如果失败，尝试 `C:\Program Files\dotnet`（全局，最高版本）

**输出**：
```csharp
new RazorSdkToolset(
    RootPath: "D:\\jolt\\runtime\\dotnet",
    SdkVersion: "9.0.101",
    SdkRootPath: "D:\\jolt\\runtime\\dotnet\\sdk\\9.0.101",
    RazorSdkRootPath: "D:\\jolt\\runtime\\dotnet\\sdk\\9.0.101\\Sdks\\Microsoft.NET.Sdk.Razor",
    RazorSourceGeneratorPath: "...\\source-generators\\Microsoft.CodeAnalysis.Razor.Compiler.dll",
    RazorTasksPath: "...\\tasks\\net10.0\\Microsoft.NET.Sdk.Razor.Tasks.dll",
    RazorDesignTimeTargetsPath: "...\\targets\\Microsoft.NET.Sdk.Razor.DesignTime.targets",
    RazorComponentTargetsPath: "...\\targets\\Microsoft.NET.Sdk.Razor.Component.targets")
```

### 9.2 Linux 环境

**环境变量**：无

**解析流程**：
1. 尝试 `/usr/share/dotnet`
2. 尝试 `/usr/local/share/dotnet`
3. 尝试 `~/.dotnet`
4. 运行 `dotnet --info` 动态发现

**输出**：假设 `/usr/share/dotnet/sdk/9.0.101` 存在，返回对应的工具集。

### 9.3 Describe() 输出示例

**成功**：
```
Razor SDK toolset: available
  root:                D:\jolt\runtime\dotnet
  sdk version:         9.0.101
  sdk root:            D:\jolt\runtime\dotnet\sdk\9.0.101
  razor sdk root:      D:\jolt\runtime\dotnet\sdk\9.0.101\Sdks\Microsoft.NET.Sdk.Razor
  source generator:    D:\jolt\runtime\dotnet\sdk\9.0.101\Sdks\Microsoft.NET.Sdk.Razor\source-generators\Microsoft.CodeAnalysis.Razor.Compiler.dll
  tasks:               D:\jolt\runtime\dotnet\sdk\9.0.101\Sdks\Microsoft.NET.Sdk.Razor\tasks\net10.0\Microsoft.NET.Sdk.Razor.Tasks.dll
  design-time targets: D:\jolt\runtime\dotnet\sdk\9.0.101\Sdks\Microsoft.NET.Sdk.Razor\targets\Microsoft.NET.Sdk.Razor.DesignTime.targets
  component targets:   D:\jolt\runtime\dotnet\sdk\9.0.101\Sdks\Microsoft.NET.Sdk.Razor\targets\Microsoft.NET.Sdk.Razor.Component.targets
```

**失败**：
```
Razor SDK toolset: unavailable
```

## 10. 路径规范化

### 10.1 平台差异

**Windows**：
```csharp
private static StringComparer GetPathComparer()
    => OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;
```

**说明**：Windows 路径比较不区分大小写，Linux/Unix 区分大小写。

### 10.2 路径去重

**策略**：
```csharp
var seen = new HashSet<string>(GetPathComparer());

if (!seen.Add(fullPath))
{
    return false;  // 跳过重复路径
}
```

**用途**：避免多次解析相同的 SDK 根路径。

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
