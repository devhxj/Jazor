# Razor 设计时代码投影服务

> 状态：已实现
> 定位：Jazor 文件的 Razor 语法投影与源映射生成核心服务

## 1. 文档定位

本文档描述 `RazorDesignTimeCodeProjectionService` 的实现，这是 Jolt 项目中用于将 Jazor 文件投影为 C# 代码的核心服务。该服务使用官方 Razor SDK 进行设计时代码生成，提取源映射，并提供多层回退策略确保鲁棒性。

**源文件位置**：
- `src/Jolt/Razor/InProc/RazorDesignTimeCodeProjectionService.cs`（约 437 行）

## 2. 核心类型

### 2.1 RazorDesignTimeCodeProjectionService

主服务类，负责创建 Jazor 文件的 Razor 投影。

**依赖项**：
- `bool _requireSdkAlignedProjection`：是否要求 SDK 对齐的投影
- `RazorSdkToolset? _resolvedToolset`：解析的 Razor SDK 工具集

**关键常量**：
```csharp
private const string ProjectionNamespace = "Jolt.RazorProjection";
```

### 2.2 RazorDesignTimeCodeProjection（readonly record struct）

投影输出结构：

```csharp
internal readonly record struct RazorDesignTimeCodeProjection(
    string ProjectedDocumentPath,      // 投影文档路径（virtual:*.razor.g.cs）
    string SourceText,                 // 生成的 C# 源代码
    ProjectionMap ProjectionMap);      // 原始位置到投影位置的映射
```

**路径示例**：
- 输入：`D:\src\Component.jazor`
- 输出：`virtual:D:\src\Component.jazor.razor.g.cs`

### 2.3 内部辅助类型

#### SourceMappingSegment（record）
```csharp
private sealed record SourceMappingSegment(
    int OriginalStart,      // 原始文档中的起始偏移
    int OriginalLength,     // 原始文档中的长度
    int ProjectedStart,     // 投影文档中的起始偏移
    int ProjectedLength,    // 投影文档中的长度
    string? FilePath);      // 关联的文件路径（用于多文件投影）
```

#### RazorCodeDocumentUnsafeAccessor（static class）
```csharp
private static class RazorCodeDocumentUnsafeAccessor
{
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetRequiredCSharpDocument")]
    internal static extern RazorCSharpDocument GetRequiredCSharpDocument(RazorCodeDocument codeDocument);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetCSharpDocument")]
    internal static extern RazorCSharpDocument? GetCSharpDocument(RazorCodeDocument codeDocument);
}
```

**用途**：使用 `UnsafeAccessor` 访问 Razor SDK 内部 API（非公开成员）。

## 3. 核心算法

### 3.1 投影创建流程（TryCreateProjection）

**目的**：将 Jazor 文件投影为 C# 代码并生成源映射。

**流程**：

1. **验证输入**：
   ```csharp
   if (document.DocumentKind != DocumentKind.Jazor || string.IsNullOrWhiteSpace(document.Text))
   {
       projection = default;
       return false;
   }
   ```

2. **检查 SDK 要求**：
   ```csharp
   if (_requireSdkAlignedProjection && _resolvedToolset is null)
   {
       projection = default;
       return false;
   }
   ```

3. **创建 Razor 文档**：
   ```csharp
   var sourceDocument = RazorSourceDocument.Create(document.Text, document.DocumentPath);
   ```

4. **创建项目引擎**（`CreateProjectEngine`）：
   ```csharp
   var projectEngine = RazorProjectEngine.Create(
       RazorConfiguration.Default,
       RazorProjectFileSystem.Create(rootPath),
       builder =>
       {
           builder.SetRootNamespace(ProjectionNamespace);
           builder.SetSupportLocalizedComponentNames();
           ComponentCodeDirective.Register(builder);
       });
   ```
   - 使用默认 Razor 配置
   - 设置根命名空间为 `Jolt.RazorProjection`
   - 注册 `@code` 指令支持（`ComponentCodeDirective`）

5. **处理设计时代码**：
   ```csharp
   var codeDocument = projectEngine.ProcessDesignTime(
       sourceDocument,
       RazorFileKind.Component,
       ImmutableArray<RazorSourceDocument>.Empty,
       tagHelpers: null);
   ```

6. **提取生成的代码和源映射**（`TryGetGeneratedCodeDocument`）：
   - 使用 `UnsafeAccessor` 调用 `GetRequiredCSharpDocument`
   - 提取 `SourceMappings`（通过反射访问内部属性）
   - 失败则回退到 `GetCSharpDocument`（可空版本）

7. **创建投影映射**（`CreateProjectionMap`）：
   - 将 `SourceMapping` 转换为 `ProjectionSegment`
   - 过滤掉零长度段和跨文件段
   - 按原始位置和投影位置排序

8. **Fallback 策略**：
   - 如果 `ProjectionMap.Segments.Count == 0`（无源映射）
   - 尝试 Code-Block Fallback（基于 `@code` 块位置）
   - 否则使用 Whole-Document 恒等投影

9. **异常处理**：
   ```csharp
   catch (TargetInvocationException)
   {
       return TryCreateFallbackProjection(document, out projection);
   }
   catch (SystemException)
   {
       return TryCreateFallbackProjection(document, out projection);
   }
   ```

**源代码引用**：`RazorDesignTimeCodeProjectionService.cs:27-98`

### 3.2 源映射提取（GetSourceMappings）

**目的**：从 Razor 生成的 C# 文档中提取源映射信息。

**策略 1：反射访问内部属性**
```csharp
private static IReadOnlyList<SourceMapping> GetSourceMappings(RazorCSharpDocument csharpDocument)
{
    try
    {
        var property = typeof(RazorCSharpDocument).GetProperty(
            "SourceMappings",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property?.GetValue(csharpDocument) is IEnumerable<SourceMapping> sourceMappings)
        {
            return sourceMappings.ToArray();
        }
    }
    catch (ArgumentException ex) { WriteSourceMappingFallbackWarning(ex); }
    catch (TargetException ex) { WriteSourceMappingFallbackWarning(ex); }
    catch (TargetInvocationException ex) { WriteSourceMappingFallbackWarning(ex); }
    catch (MemberAccessException ex) { WriteSourceMappingFallbackWarning(ex); }
    catch (NotSupportedException ex) { WriteSourceMappingFallbackWarning(ex); }
    catch (SystemException ex) { WriteSourceMappingFallbackWarning(ex); }

    return [];
}
```

**警告输出**：
```
[jolt][razor][warning] Falling back without Razor SourceMappings after ArgumentException: ...
```

**策略 2：UnsafeAccessor 回退**
```csharp
private static bool TryGetGeneratedCodeDocumentByUnsafeFallback(
    RazorCodeDocument codeDocument,
    out string generatedCode,
    out IReadOnlyList<SourceMapping> sourceMappings)
{
    try
    {
        var csharpDocument = RazorCodeDocumentUnsafeAccessor.GetCSharpDocument(codeDocument);
        if (csharpDocument is null)
        {
            generatedCode = string.Empty;
            sourceMappings = [];
            return false;
        }

        generatedCode = csharpDocument.Text.ToString();
        sourceMappings = GetSourceMappings(csharpDocument);
        return !string.IsNullOrWhiteSpace(generatedCode);
    }
    catch (TargetInvocationException) { ... }
    catch (SystemException) { ... }
}
```

**源代码引用**：
- 反射访问：`RazorDesignTimeCodeProjectionService.cs:365-403`
- UnsafeAccessor：`RazorDesignTimeCodeProjectionService.cs:322-353`

### 3.3 投影映射创建（CreateProjectionMap）

**目的**：将 Razor 的 `SourceMapping` 转换为 Jolt 的 `ProjectionSegment`。

**流程**：

1. **创建段**：
   ```csharp
   var segments = sourceMappings
       .Select(static mapping => TryCreateSegment(mapping))
       .Where(static segment => segment is not null)
       .Select(static segment => segment!)
       .Where(segment => segment.OriginalLength > 0 && segment.ProjectedLength > 0)
       .Where(segment => string.IsNullOrWhiteSpace(segment.FilePath)
           || PathComparer.Equals(
               NormalizeComparablePath(segment.FilePath),
               normalizedSourceDocumentPath))
       .Select(static segment => new ProjectionSegment(
           segment.OriginalStart,
           segment.OriginalLength,
           segment.ProjectedStart,
           segment.ProjectedLength))
       .OrderBy(static segment => segment.OriginalStart)
       .ThenBy(static segment => segment.ProjectedStart)
       .ToArray();
   ```

2. **过滤条件**：
   - 长度 > 0（排除零长度段）
   - 文件路径匹配（排除跨文件段，如 `_Imports.razor`）
   - 路径规范化（Windows 不区分大小写）

3. **创建 ProjectionMap**：
   ```csharp
   return new ProjectionMap(sourceDocumentPath, projectedDocumentPath, segments);
   ```

**源代码引用**：`RazorDesignTimeCodeProjectionService.cs:122-147`

### 3.4 Code-Block Fallback（TryCreateCodeBlockFallbackProjectionMap）

**目的**：当 Razor 源映射不可用时，基于 `@code` 块位置创建简化的投影映射。

**流程**：

1. **解析 Jazor 文档**：
   ```csharp
   var parsed = new JazorVueParser().Parse(document.DocumentPath, document.Text);
   ```

2. **验证 @code 块**：
   ```csharp
   if (parsed.CodeStartIndex < 0
       || parsed.CodeLength <= 0
       || string.IsNullOrWhiteSpace(parsed.Code))
   {
       projectionMap = default!;
       return false;
   }
   ```

3. **在生成的 C# 中查找 @code 内容**：
   ```csharp
   var projectedCodeStart = generatedCode.IndexOf(parsed.Code, StringComparison.Ordinal);
   if (projectedCodeStart < 0)
   {
       projectionMap = default!;
       return false;
   }
   ```

4. **创建单段映射**：
   ```csharp
   projectionMap = new ProjectionMap(
       document.DocumentPath,
       projectedDocumentPath,
       [
           new ProjectionSegment(
               parsed.CodeStartIndex,
               parsed.Code.Length,
               projectedCodeStart,
               parsed.Code.Length)
       ]);
   return true;
   ```

**限制**：
- 仅映射 `@code` 块，不包含模板部分
- 假设 `@code` 内容在生成的 C# 中是字面量存在（未修改）

**源代码引用**：`RazorDesignTimeCodeProjectionService.cs:181-214`

### 3.5 Fallback 投影（TryCreateFallbackProjection）

**目的**：当 Razor 投影完全失败时，使用 JazorVueParser 创建最小化投影。

**流程**：

1. **解析 Jazor 文档**：
   ```csharp
   var parsed = new JazorVueParser().Parse(document.DocumentPath, document.Text);
   ```

2. **验证 @code 块**：
   ```csharp
   if (parsed.CodeStartIndex < 0
       || parsed.CodeLength <= 0
       || string.IsNullOrWhiteSpace(parsed.Code))
   {
       projection = default;
       return false;
   }
   ```

3. **构建投影源代码**（`BuildFallbackProjectedSource`）：
   ```csharp
   var builder = new StringBuilder();
   builder.AppendLine("using System;");
   builder.AppendLine("using System.Collections.Generic;");
   builder.AppendLine("using System.Linq;");
   builder.AppendLine("using System.Threading.Tasks;");
   builder.AppendLine("#nullable enable");
   builder.Append("namespace ").Append(ProjectionNamespace).AppendLine(";");
   builder.Append("public partial class ")
       .Append(CreateFallbackContainerName(documentPath))
       .AppendLine();
   builder.AppendLine("{");
   builder.AppendLine(parsed.Code);
   builder.AppendLine("}");
   ```

4. **创建投影映射**：
   - 尝试 Code-Block Fallback
   - 失败则使用 Whole-Document 恒等投影

5. **返回投影**：
   ```csharp
   projection = new RazorDesignTimeCodeProjection(
       projectedDocumentPath,
       generatedCode,
       projectionMap);
   return true;
   ```

**特点**：
- 不依赖 Razor SDK
- 仅保留 C# using 指令和 `@code` 块
- 生成简单的 partial class 容器

**源代码引用**：
- 主流程：`RazorDesignTimeCodeProjectionService.cs:216-245`
- 源代码构建：`RazorDesignTimeCodeProjectionService.cs:247-265`

### 3.6 容器名称生成（CreateFallbackContainerName）

**目的**：为 Fallback 投影生成唯一的 C# 类名。

**算法**：

```csharp
private static string CreateFallbackContainerName(string documentPath)
{
    var fileName = Path.GetFileNameWithoutExtension(documentPath);
    var sanitized = string.Concat((fileName ?? "Document").Select(character =>
        char.IsLetterOrDigit(character) || character == '_' ? character : '_'));
    if (string.IsNullOrWhiteSpace(sanitized) || !char.IsLetter(sanitized[0]) && sanitized[0] != '_')
    {
        sanitized = "_" + sanitized;
    }

    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(documentPath));
    var hash = Convert.ToHexString(bytes.AsSpan(0, 4));
    return "__JazorDocument_" + sanitized + "_" + hash;
}
```

**示例**：
- `Counter.jazor` → `__JazorDocument_Counter_A1B2C3D4`
- `my-component.jazor` → `__JazorDocument_my__component_E5F6G7H8`

**哈希长度**：4 字节（8 个十六进制字符）

**源代码引用**：`RazorDesignTimeCodeProjectionService.cs:267-280`

### 3.7 路径规范化（NormalizeComparablePath）

**目的**：统一路径格式，支持跨平台比较。

**算法**：

```csharp
private static string NormalizeComparablePath(string path)
{
    if (string.IsNullOrWhiteSpace(path))
    {
        return string.Empty;
    }

    try
    {
        var fullPath = Path.IsPathRooted(path)
            ? Path.GetFullPath(path)
            : path;
        return fullPath.Replace('\\', '/');
    }
    catch (ArgumentException) { return path.Replace('\\', '/'); }
    catch (NotSupportedException) { return path.Replace('\\', '/'); }
    catch (PathTooLongException) { return path.Replace('\\', '/'); }
    catch (IOException) { return path.Replace('\\', '/'); }
}
```

**特点**：
- 统一使用 `/` 作为路径分隔符
- 绝对路径转换为完全限定路径
- 异常时回退到简单替换

**源代码引用**：`RazorDesignTimeCodeProjectionService.cs:149-179`

## 4. 线程安全模型

### 4.1 无状态设计

**特点**：
- `RazorDesignTimeCodeProjectionService` 本身是无状态的
- 所有方法都是纯函数（输入 → 输出）
- 不维护缓存或可变状态

### 4.2 RazorProjectEngine 线程安全

**假设**：
- `RazorProjectEngine.Create()` 返回的实例是线程安全的（根据 ASP.NET Core Razor 设计）
- 多线程调用 `ProcessDesignTime` 是安全的

**结论**：整个服务是线程安全的，适合并发调用。

## 5. 错误处理

### 5.1 Razor SDK 失败

**策略**：
- 捕获 `TargetInvocationException`（反射调用失败）
- 捕获 `SystemException`（通用异常）
- 回退到 `TryCreateFallbackProjection`

### 5.2 源映射提取失败

**策略**：
- 反射访问失败 → 记录警告，返回空列表
- 尝试 `UnsafeAccessor` 回退
- 最终回退到 Code-Block Fallback 或 Whole-Document 恒等投影

### 5.3 JazorVueParser 失败

**策略**：
- `CodeStartIndex < 0` 或 `CodeLength <= 0` → 返回 `false`
- 不抛出异常，静默失败

### 5.4 路径规范化失败

**策略**：
- 捕获各种路径异常（`ArgumentException`, `IOException` 等）
- 回退到简单的 `\` 替换为 `/`

## 6. 配置选项

### 6.1 投影命名空间

```csharp
private const string ProjectionNamespace = "Jolt.RazorProjection";
```

**用途**：所有生成的 C# 类都在此命名空间下。

### 6.2 SDK 对齐要求

```csharp
public RazorDesignTimeCodeProjectionService(RazorSdkToolsetHost? toolsetHost = null)
{
    _requireSdkAlignedProjection = toolsetHost is not null;
    _resolvedToolset = toolsetHost?.ResolveToolset();
}
```

**模式**：
- **严格模式**（`toolsetHost != null`）：要求 Razor SDK 可用
- **宽松模式**（`toolsetHost == null`）：允许 Fallback 投影

### 6.3 Razor 配置

```csharp
RazorConfiguration.Default
```

**说明**：使用默认 Razor 配置（非自定义）。

### 6.4 文件系统

```csharp
RazorProjectFileSystem.Create(rootPath)
```

**rootPath**：文档所在目录（用于解析 `_Imports.razor` 等依赖文件）。

## 7. 与其他子系统的交互

### 7.1 Razor SDK（Microsoft.AspNetCore.Razor.Language）

**依赖项**：
- `RazorProjectEngine`
- `RazorSourceDocument`
- `RazorCodeDocument`
- `RazorCSharpDocument`
- `SourceMapping`

**用途**：
- 提供 Razor 语法解析和 C# 代码生成
- 提供源映射信息

### 7.2 JazorVueParser

**来源**：`Jazor.Vue`

**用途**：
- Fallback 投影中解析 `@code` 块
- Code-Block Fallback 中定位代码块位置

### 7.3 ProjectionMap

**来源**：`Jolt.VirtualDocuments.Mapping`

**用途**：
- 封装原始位置到投影位置的映射
- 支持双向查找（原始 → 投影，投影 → 原始）

### 7.4 RazorSdkToolsetHost

**来源**：`Jolt.Razor.Toolset`

**用途**：
- 解析 Razor SDK 工具集
- 决定投影模式（严格 vs 宽松）

## 8. 设计权衡

### 8.1 多层回退策略

**选择**：Razor 投影 → UnsafeAccessor 回退 → Code-Block Fallback → Whole-Document 恒等投影。

**权衡**：
- **优点**：最大化鲁棒性，适应各种部署场景
- **缺点**：回退链复杂，调试困难
- **可观测性**：每个回退点都有警告日志

### 8.2 UnsafeAccessor 使用

**选择**：使用 .NET 7 的 `UnsafeAccessor` 访问 Razor SDK 内部 API。

**权衡**：
- **优点**：避免硬编码内部成员名称（编译时检查）
- **缺点**：依赖 Razor SDK 内部实现，可能随版本变化
- **风险**：Razor SDK 更新可能导致 `UnsafeAccessor` 失效
- **缓解**：提供反射回退（`GetSourceMappings`）

### 8.3 源映射反射访问

**选择**：通过反射访问 `RazorCSharpDocument.SourceMappings` 属性。

**权衡**：
- **优点**：不依赖公共 API（Razor SDK 未公开此属性）
- **缺点**：性能开销，可能被未来 Razor 版本移除
- **替代方案**：等待 Razor SDK 公开源映射 API

### 8.4 Code-Block Fallback 简化

**选择**：仅映射 `@code` 块，忽略模板部分。

**权衡**：
- **优点**：实现简单，覆盖主要使用场景（C# 代码智能感知）
- **缺点**：模板中的位置无法映射（如 `@onclick="Increment"`）
- **未来改进**：扩展到模板关键位置（指令、事件处理器等）

### 8.5 命名空间固定

**选择**：所有投影代码使用 `Jolt.RazorProjection` 命名空间。

**权衡**：
- **优点**：避免命名冲突，简化代码生成
- **缺点**：可能与用户代码冲突（如果用户使用相同命名空间）
- **风险**：较低（`Jolt.RazorProjection` 是项目专用前缀）

## 9. 完整示例

### 9.1 输入：`Counter.jazor`

```razor
@implements Jazor.Vue.IComponent

<int Count="0" />
<button @onclick="Increment">Increment</button>
<span>@Count</span>

@code {
    [Prop]
    public int Count { get; set; }

    public void Increment()
    {
        Count++;
    }
}
```

### 9.2 Razor 投影输出（简化）

```csharp
#pragma checksum "D:\src\Counter.jazor"{...}
namespace Jolt.RazorProjection
{
    [global::Microsoft.AspNetCore.Components.RouteAttribute("_")]
    public partial class Counter : Jazor.Vue.IComponent
    {
        #pragma warning disable 1998
        [global::Microsoft.AspNetCore.Components.InjectAttribute]
        private global::Jazor.Vue.IVueRuntime __vue_runtime { get; set; }

        protected override void BuildRenderTree(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            // ... 模板渲染逻辑 ...
        }

        [Prop]
        public int Count { get; set; }

        public void Increment()
        {
            Count++;
        }
    }
}
```

### 9.3 源映射示例

**原始位置** → **投影位置**：
- `@code` 块（行 7-18）→ `Counter` 类的成员定义（行 14-21）
- `[Prop]` 属性（行 8）→ `[Prop]` 属性（行 15）
- `Count` 属性（行 9）→ `Count` 属性（行 16）

### 9.4 Fallback 投影输出（简化）

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#nullable enable
namespace Jolt.RazorProjection;
public partial class __JazorDocument_Counter_A1B2C3D4
{
    [Prop]
    public int Count { get; set; }

    public void Increment()
    {
        Count++;
    }
}
```

**投影映射**：
- 单段映射：`@code` 块 → Fallback 类的成员定义

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
