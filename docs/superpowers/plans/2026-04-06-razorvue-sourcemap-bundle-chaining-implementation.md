# RazorVue Sourcemap Bundle-Chaining Implementation Plan

> Status: active plan
> Positioning: Execution-level implementation plan for the current narrow SourceMap rollout tied to RazorVue output.

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 为 RazorVue emitted modules 与最终 bundle 落地可调试的外置 sourcemap，支持 `sourcesContent`、`sourceMappingURL` 与 bundle chaining。

**Architecture:** 先把 `RazorVueSourceOrigin` 中已有的 generated span 信息完整穿透到 emit 层，再在 `Jazor.Emit` 内新增通用 `SourceMaps` 核心类型，先生成模块级 `.mjs.map`，最后让 `ModuleBundler` 通过 `deno bundle --sourcemap external` 产出顶层 map，并将其与模块级 map 串联成最终 `bundle.map`。整个实现保持行级稳定映射，缺失 origin 时局部降级但不中断 emit/bundle。

**Tech Stack:** C# 14, .NET 10, MSTest, System.Text.Json, DenoHost 2.7.5, `deno bundle --sourcemap external`

---

## File Structure

### Existing files to modify
- `src/Jazor.RazorVue.Analysis/RazorVueGenerator.cs`
  - 让 generated catalog 保留 `GeneratedFilePath` / `GeneratedSpanStart` / `GeneratedSpanLength`
- `src/Jazor.Emit/RazorVueCatalogReader.cs`
  - 读回 generated span 信息，扩展 `RazorVueEmitSourceOriginRecord`
- `src/Jazor.Emit/RazorVueModuleWriter.cs`
  - 写 emitted `.mjs.map`，并给 `.mjs` 追加 `//# sourceMappingURL=`
- `src/Jazor.Emit/ModuleBundler.cs`
  - 复制输入模块 `.map`、调用 `deno bundle --sourcemap external`、生成最终 chained `bundle.map`
- `src/Jazor.EmitTest/RazorVueCatalogReaderTests.cs`
  - 锁住 generated span 从 generator 到 emit reader 的链路
- `src/Jazor.EmitTest/RazorVueEmitIntegrationTests.cs`
  - 锁住模块 `.mjs.map`、`sourcesContent` 与 `sourceMappingURL`
- `src/Jazor.EmitTest/ModuleBundlerTests.cs`
  - 锁住 final `bundle.js.map` 与 chaining 行为
- `src/Jazor.EmitTest/SdkIntegrationTests.cs`
  - 锁住真实 sample 产物中 bundle map 的落盘
- `src/Jazor.Compiler/doc/SourceMap.Overview.md`
  - 去掉“第一阶段不做 bundle map chaining”的旧结论
- `src/Jazor.Compiler/doc/SourceMap.ImplementationChecklist.md`
  - 同步当前执行顺序：模块 map + bundle chaining

### New files to create
- `src/Jazor.Emit/SourceMaps/SourceMapDocument.cs`
  - sourcemap 内存模型：文档、source、segment
- `src/Jazor.Emit/SourceMaps/SourceMapBuilder.cs`
  - 从 `moduleCode + origins` 构建模块级 line mappings
- `src/Jazor.Emit/SourceMaps/SourceMapWriter.cs`
  - 标准 source map v3 JSON 序列化、VLQ 编码、`sourceMappingURL` 追加
- `src/Jazor.Emit/SourceMaps/SourceMapChainBuilder.cs`
  - 将 final bundle map 与模块 `.map` 串联成最终 map
- `src/Jazor.EmitTest/SourceMapBuilderTests.cs`
  - builder 单测
- `src/Jazor.EmitTest/SourceMapWriterTests.cs`
  - writer 单测
- `src/Jazor.EmitTest/SourceMapChainBuilderTests.cs`
  - chaining 单测

### Design notes that affect decomposition
- 当前 `RazorVueSourceOrigin` 本身已经有 `GeneratedFilePath` / `GeneratedSpanStart` / `GeneratedSpanLength`，但 `RazorVueGenerator` 生成 catalog 时没有把这些字段写进 generated catalog；这件事必须先修，否则模块 map 无法稳定按 generated line 建立。
- 当前 `ModuleBundler` 只复制 `.mjs`，没有复制 `.map`，也没有把 `--sourcemap` 传给 Deno；因此 bundle chaining 必须建立在“先有 emitted module map，再有 final bundle external map”这个顺序上。
- 第一阶段不扩 `ManifestModel` / `RazorVueManifestModel` 字段；统一使用同名约定：`<module>.mjs.map`、`<bundle>.js.map`。

---

### Task 1: Preserve generated spans from RazorVue generator into emit reader

**Files:**
- Modify: `src/Jazor.RazorVue.Analysis/RazorVueGenerator.cs`
- Modify: `src/Jazor.Emit/RazorVueCatalogReader.cs`
- Test: `src/Jazor.EmitTest/RazorVueCatalogReaderTests.cs`

- [ ] **Step 1: Write the failing reader test for generated spans**

Update `src/Jazor.EmitTest/RazorVueCatalogReaderTests.cs` so the fixture and assertions require the generated span fields.

```csharp
[TestMethod]
public void RazorVueCatalogReader_ReadsGeneratedCatalogFromAssembly()
{
    var catalog = RazorVueCatalogReader.TryRead(typeof(RazorVueCatalogReaderTests).Assembly);

    Assert.IsNotNull(catalog);
    var origin = catalog.Artifacts[0].SourceOrigins[0];
    Assert.AreEqual("Counter.razor", origin.SourceFilePath);
    Assert.AreEqual("components/counter-card.mjs", origin.GeneratedFilePath);
    Assert.AreEqual(0, origin.GeneratedSpanStart);
    Assert.AreEqual(38, origin.GeneratedSpanLength);
}
```

Update the generated fixture in the same file from:

```csharp
private sealed class GeneratedOrigin(
    string sourceFilePath,
    int sourceSpanStart,
    int sourceSpanLength,
    int startLine,
    int startColumn,
    GeneratedMappingQuality mappingQuality,
    GeneratedOriginProvenance provenance)
```

to:

```csharp
private sealed class GeneratedOrigin(
    string sourceFilePath,
    int sourceSpanStart,
    int sourceSpanLength,
    int startLine,
    int startColumn,
    string generatedFilePath,
    int generatedSpanStart,
    int generatedSpanLength,
    GeneratedMappingQuality mappingQuality,
    GeneratedOriginProvenance provenance)
```

and instantiate it as:

```csharp
new GeneratedOrigin(
    sourceFilePath: "Counter.razor",
    sourceSpanStart: 12,
    sourceSpanLength: 8,
    startLine: 2,
    startColumn: 4,
    generatedFilePath: "components/counter-card.mjs",
    generatedSpanStart: 0,
    generatedSpanLength: 38,
    mappingQuality: GeneratedMappingQuality.MappedFromGenerated,
    provenance: GeneratedOriginProvenance.GeneratedSyntaxLocation)
```

- [ ] **Step 2: Run the test to verify it fails**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.EmitTest/Jazor.EmitTest.csproj" --filter "FullyQualifiedName~RazorVueCatalogReaderTests"
```

Expected: FAIL because `GeneratedFilePath` / `GeneratedSpanStart` / `GeneratedSpanLength` are not yet emitted/read.

- [ ] **Step 3: Extend `RazorVueGenerator` generated catalog shape**

In `src/Jazor.RazorVue.Analysis/RazorVueGenerator.cs`, replace the generated origin constructor block:

```csharp
builder.AppendLine("            public GeneratedOrigin(string sourceFilePath, int sourceSpanStart, int sourceSpanLength, int startLine, int startColumn, GeneratedMappingQuality mappingQuality, GeneratedOriginProvenance provenance)");
builder.AppendLine("            {");
builder.AppendLine("                SourceFilePath = sourceFilePath;");
builder.AppendLine("                SourceSpanStart = sourceSpanStart;");
builder.AppendLine("                SourceSpanLength = sourceSpanLength;");
builder.AppendLine("                StartLine = startLine;");
builder.AppendLine("                StartColumn = startColumn;");
builder.AppendLine("                MappingQuality = mappingQuality;");
builder.AppendLine("                Provenance = provenance;");
builder.AppendLine("            }");
builder.AppendLine("            public string SourceFilePath { get; }");
builder.AppendLine("            public int SourceSpanStart { get; }");
builder.AppendLine("            public int SourceSpanLength { get; }");
builder.AppendLine("            public int StartLine { get; }");
builder.AppendLine("            public int StartColumn { get; }");
builder.AppendLine("            public GeneratedMappingQuality MappingQuality { get; }");
builder.AppendLine("            public GeneratedOriginProvenance Provenance { get; }");
```

with:

```csharp
builder.AppendLine("            public GeneratedOrigin(string sourceFilePath, int sourceSpanStart, int sourceSpanLength, int startLine, int startColumn, string? generatedFilePath, int? generatedSpanStart, int? generatedSpanLength, GeneratedMappingQuality mappingQuality, GeneratedOriginProvenance provenance)");
builder.AppendLine("            {");
builder.AppendLine("                SourceFilePath = sourceFilePath;");
builder.AppendLine("                SourceSpanStart = sourceSpanStart;");
builder.AppendLine("                SourceSpanLength = sourceSpanLength;");
builder.AppendLine("                StartLine = startLine;");
builder.AppendLine("                StartColumn = startColumn;");
builder.AppendLine("                GeneratedFilePath = generatedFilePath;");
builder.AppendLine("                GeneratedSpanStart = generatedSpanStart;");
builder.AppendLine("                GeneratedSpanLength = generatedSpanLength;");
builder.AppendLine("                MappingQuality = mappingQuality;");
builder.AppendLine("                Provenance = provenance;");
builder.AppendLine("            }");
builder.AppendLine("            public string SourceFilePath { get; }");
builder.AppendLine("            public int SourceSpanStart { get; }");
builder.AppendLine("            public int SourceSpanLength { get; }");
builder.AppendLine("            public int StartLine { get; }");
builder.AppendLine("            public int StartColumn { get; }");
builder.AppendLine("            public string? GeneratedFilePath { get; }");
builder.AppendLine("            public int? GeneratedSpanStart { get; }");
builder.AppendLine("            public int? GeneratedSpanLength { get; }");
builder.AppendLine("            public GeneratedMappingQuality MappingQuality { get; }");
builder.AppendLine("            public GeneratedOriginProvenance Provenance { get; }");
```

Update `BuildOriginsArrayLiteral(...)` from:

```csharp
builder.Append("                        startColumn: ").Append(origin.StartColumn).AppendLine(",");
builder.Append("                        mappingQuality: GeneratedMappingQuality.").Append(origin.MappingQuality).AppendLine(",");
builder.Append("                        provenance: GeneratedOriginProvenance.").Append(origin.Provenance).AppendLine("),");
```

to:

```csharp
builder.Append("                        startColumn: ").Append(origin.StartColumn).AppendLine(",");
builder.Append("                        generatedFilePath: ").Append(EscapeCSharpString(origin.GeneratedFilePath ?? string.Empty)).AppendLine(",");
builder.Append("                        generatedSpanStart: ").Append(origin.GeneratedSpanStart?.ToString() ?? "null").AppendLine(",");
builder.Append("                        generatedSpanLength: ").Append(origin.GeneratedSpanLength?.ToString() ?? "null").AppendLine(",");
builder.Append("                        mappingQuality: GeneratedMappingQuality.").Append(origin.MappingQuality).AppendLine(",");
builder.Append("                        provenance: GeneratedOriginProvenance.").Append(origin.Provenance).AppendLine("),");
```

Then fix the nullable string emission so empty generated path is emitted as `null` rather than `""`:

```csharp
builder.Append("                        generatedFilePath: ")
    .Append(origin.GeneratedFilePath is null ? "null" : EscapeCSharpString(origin.GeneratedFilePath))
    .AppendLine(",");
```

- [ ] **Step 4: Extend `RazorVueCatalogReader` record and reader**

In `src/Jazor.Emit/RazorVueCatalogReader.cs`, change the record from:

```csharp
internal sealed record RazorVueEmitSourceOriginRecord(
    string SourceFilePath,
    int SourceSpanStart,
    int SourceSpanLength,
    int StartLine,
    int StartColumn,
    RazorVueMappingQualityRecord MappingQuality,
    RazorVueOriginProvenanceRecord Provenance);
```

to:

```csharp
internal sealed record RazorVueEmitSourceOriginRecord(
    string SourceFilePath,
    int SourceSpanStart,
    int SourceSpanLength,
    int StartLine,
    int StartColumn,
    string? GeneratedFilePath,
    int? GeneratedSpanStart,
    int? GeneratedSpanLength,
    RazorVueMappingQualityRecord MappingQuality,
    RazorVueOriginProvenanceRecord Provenance);
```

Add helpers:

```csharp
private static int? ReadNullableInt32(Type itemType, object item, string propertyName)
{
    var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
    if (property is null)
        throw new InvalidOperationException($"Property '{propertyName}' was not found on '{itemType.FullName}'.");

    return property.GetValue(item) switch
    {
        null => null,
        int value => value,
        _ => throw new InvalidOperationException($"Property '{propertyName}' was not an Int32 on '{itemType.FullName}'.")
    };
}

private static string? ReadNullableString(Type itemType, object item, string propertyName)
{
    var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
    if (property is null)
        throw new InvalidOperationException($"Property '{propertyName}' was not found on '{itemType.FullName}'.");

    return property.GetValue(item) as string;
}
```

Update `ReadOrigins(...)` to build the new record:

```csharp
origins.Add(new RazorVueEmitSourceOriginRecord(
    ReadString(entryType, entry, "SourceFilePath"),
    ReadInt32(entryType, entry, "SourceSpanStart"),
    ReadInt32(entryType, entry, "SourceSpanLength"),
    ReadInt32(entryType, entry, "StartLine"),
    ReadInt32(entryType, entry, "StartColumn"),
    ReadNullableString(entryType, entry, "GeneratedFilePath"),
    ReadNullableInt32(entryType, entry, "GeneratedSpanStart"),
    ReadNullableInt32(entryType, entry, "GeneratedSpanLength"),
    ReadEnum<RazorVueMappingQualityRecord>(entryType, entry, "MappingQuality"),
    ReadEnum<RazorVueOriginProvenanceRecord>(entryType, entry, "Provenance")));
```

- [ ] **Step 5: Run the reader test to verify it passes**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.EmitTest/Jazor.EmitTest.csproj" --filter "FullyQualifiedName~RazorVueCatalogReaderTests"
```

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add src/Jazor.RazorVue.Analysis/RazorVueGenerator.cs src/Jazor.Emit/RazorVueCatalogReader.cs src/Jazor.EmitTest/RazorVueCatalogReaderTests.cs
git commit -m "feat: preserve RazorVue generated spans for sourcemaps"
```

### Task 2: Add source map core types, builder, and writer

**Files:**
- Create: `src/Jazor.Emit/SourceMaps/SourceMapDocument.cs`
- Create: `src/Jazor.Emit/SourceMaps/SourceMapBuilder.cs`
- Create: `src/Jazor.Emit/SourceMaps/SourceMapWriter.cs`
- Test: `src/Jazor.EmitTest/SourceMapBuilderTests.cs`
- Test: `src/Jazor.EmitTest/SourceMapWriterTests.cs`

- [ ] **Step 1: Write the failing builder and writer tests**

Create `src/Jazor.EmitTest/SourceMapBuilderTests.cs` with:

```csharp
using Jazor.Emit.SourceMaps;

namespace Jazor.EmitTest;

[TestClass]
public sealed class SourceMapBuilderTests
{
    [TestMethod]
    public void BuildModuleMap_UsesGeneratedSpansToCreateLineMappings()
    {
        var moduleCode = "export const a = 1;\nexport const b = 2;\n";
        var origins = new[]
        {
            new RazorVueEmitSourceOriginRecord(
                SourceFilePath: "Counter.razor",
                SourceSpanStart: 12,
                SourceSpanLength: 8,
                StartLine: 2,
                StartColumn: 4,
                GeneratedFilePath: "components/counter-card.mjs",
                GeneratedSpanStart: 0,
                GeneratedSpanLength: 19,
                MappingQuality: RazorVueMappingQualityRecord.MappedFromGenerated,
                Provenance: RazorVueOriginProvenanceRecord.GeneratedSyntaxLocation),
            new RazorVueEmitSourceOriginRecord(
                SourceFilePath: "Counter.razor",
                SourceSpanStart: 24,
                SourceSpanLength: 8,
                StartLine: 3,
                StartColumn: 4,
                GeneratedFilePath: "components/counter-card.mjs",
                GeneratedSpanStart: 20,
                GeneratedSpanLength: 19,
                MappingQuality: RazorVueMappingQualityRecord.MappedFromGenerated,
                Provenance: RazorVueOriginProvenanceRecord.GeneratedSyntaxLocation)
        };

        var builder = new SourceMapBuilder();
        var document = builder.BuildModuleMap(
            generatedFileName: "components/counter-card.mjs",
            moduleCode,
            origins,
            path => path == "Counter.razor" ? "<Counter />" : null);

        Assert.AreEqual("components/counter-card.mjs", document.File);
        Assert.AreEqual(1, document.Sources.Count);
        Assert.AreEqual("Counter.razor", document.Sources[0].Path);
        Assert.AreEqual("<Counter />", document.Sources[0].Content);
        Assert.AreEqual(2, document.Segments.Count);
        Assert.AreEqual(0, document.Segments[0].GeneratedLine);
        Assert.AreEqual(1, document.Segments[1].GeneratedLine);
        Assert.AreEqual(1, document.Segments[0].SourceLine);
        Assert.AreEqual(2, document.Segments[1].SourceLine);
    }
}
```

Create `src/Jazor.EmitTest/SourceMapWriterTests.cs` with:

```csharp
using Jazor.Emit.SourceMaps;
using System.Text.Json;

namespace Jazor.EmitTest;

[TestClass]
public sealed class SourceMapWriterTests
{
    [TestMethod]
    public void Write_ProducesExternalSourceMapJson()
    {
        var document = new SourceMapDocument(
            File: "components/counter-card.mjs",
            Sources:
            [
                new SourceMapSource("Counter.razor", "<Counter />")
            ],
            Segments:
            [
                new SourceMapSegment(0, 0, 0, 1, 3),
                new SourceMapSegment(1, 0, 0, 2, 3)
            ]);

        var writer = new SourceMapWriter();
        var json = writer.Write(document);
        using var parsed = JsonDocument.Parse(json);

        Assert.AreEqual(3, parsed.RootElement.GetProperty("version").GetInt32());
        Assert.AreEqual("components/counter-card.mjs", parsed.RootElement.GetProperty("file").GetString());
        Assert.AreEqual("Counter.razor", parsed.RootElement.GetProperty("sources")[0].GetString());
        Assert.AreEqual("<Counter />", parsed.RootElement.GetProperty("sourcesContent")[0].GetString());
        Assert.AreNotEqual(string.Empty, parsed.RootElement.GetProperty("mappings").GetString());
    }

    [TestMethod]
    public void AppendSourceMappingUrl_AppendsTrailingCommentOnce()
    {
        var writer = new SourceMapWriter();
        var code = writer.AppendSourceMappingUrl("export const a = 1;\n", "counter-card.mjs.map");

        StringAssert.Contains(code, "//# sourceMappingURL=counter-card.mjs.map");
        Assert.AreEqual(1, code.Split("sourceMappingURL=").Length - 1);
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.EmitTest/Jazor.EmitTest.csproj" --filter "FullyQualifiedName~SourceMapBuilderTests|FullyQualifiedName~SourceMapWriterTests"
```

Expected: FAIL because `SourceMaps` types do not exist yet.

- [ ] **Step 3: Create `SourceMapDocument.cs`**

Create `src/Jazor.Emit/SourceMaps/SourceMapDocument.cs` with:

```csharp
namespace Jazor.Emit.SourceMaps;

internal sealed record SourceMapDocument(
    string File,
    IReadOnlyList<SourceMapSource> Sources,
    IReadOnlyList<SourceMapSegment> Segments);

internal sealed record SourceMapSource(
    string Path,
    string? Content);

internal sealed record SourceMapSegment(
    int GeneratedLine,
    int GeneratedColumn,
    int SourceIndex,
    int SourceLine,
    int SourceColumn);
```

- [ ] **Step 4: Create `SourceMapBuilder.cs`**

Create `src/Jazor.Emit/SourceMaps/SourceMapBuilder.cs` with:

```csharp
namespace Jazor.Emit.SourceMaps;

internal sealed class SourceMapBuilder
{
    public SourceMapDocument BuildModuleMap(
        string generatedFileName,
        string moduleCode,
        IReadOnlyList<RazorVueEmitSourceOriginRecord> origins,
        Func<string, string?> readSourceContent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(generatedFileName);
        ArgumentNullException.ThrowIfNull(origins);
        ArgumentNullException.ThrowIfNull(readSourceContent);

        var sources = origins
            .Where(static origin => !string.IsNullOrWhiteSpace(origin.SourceFilePath))
            .Select(static origin => origin.SourceFilePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path => new SourceMapSource(path, readSourceContent(path)))
            .ToArray();

        var sourceIndex = sources
            .Select((source, index) => (source.Path, index))
            .ToDictionary(static item => item.Path, static item => item.index, StringComparer.OrdinalIgnoreCase);

        var lineStarts = ComputeLineStarts(moduleCode ?? string.Empty);
        var segments = new List<SourceMapSegment>();

        foreach (var origin in origins.OrderBy(static origin => origin.GeneratedSpanStart ?? int.MaxValue))
        {
            if (origin.GeneratedSpanStart is null || origin.GeneratedSpanLength is null)
                continue;
            if (!sourceIndex.TryGetValue(origin.SourceFilePath, out var index))
                continue;
            if (origin.GeneratedSpanLength.Value <= 0)
                continue;

            var generatedStart = origin.GeneratedSpanStart.Value;
            var generatedEnd = origin.GeneratedSpanStart.Value + origin.GeneratedSpanLength.Value;
            var startLine = FindLineIndex(lineStarts, generatedStart);
            var endLine = FindLineIndex(lineStarts, Math.Max(generatedStart, generatedEnd - 1));

            for (var line = startLine; line <= endLine; line++)
            {
                segments.Add(new SourceMapSegment(
                    GeneratedLine: line,
                    GeneratedColumn: 0,
                    SourceIndex: index,
                    SourceLine: Math.Max(origin.StartLine - 1 + (line - startLine), 0),
                    SourceColumn: Math.Max(origin.StartColumn - 1, 0)));
            }
        }

        var dedupedSegments = segments
            .GroupBy(static segment => (segment.GeneratedLine, segment.GeneratedColumn))
            .Select(static group => group.First())
            .OrderBy(static segment => segment.GeneratedLine)
            .ThenBy(static segment => segment.GeneratedColumn)
            .ToArray();

        return new SourceMapDocument(generatedFileName, sources, dedupedSegments);
    }

    private static int[] ComputeLineStarts(string text)
    {
        var starts = new List<int> { 0 };
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
                starts.Add(i + 1);
        }

        return starts.ToArray();
    }

    private static int FindLineIndex(int[] lineStarts, int position)
    {
        var index = Array.BinarySearch(lineStarts, position);
        return index >= 0 ? index : Math.Max(~index - 1, 0);
    }
}
```

- [ ] **Step 5: Create `SourceMapWriter.cs`**

Create `src/Jazor.Emit/SourceMaps/SourceMapWriter.cs` with:

```csharp
using System.Text;
using System.Text.Json;

namespace Jazor.Emit.SourceMaps;

internal sealed class SourceMapWriter
{
    private const string Base64Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

    public string Write(SourceMapDocument document)
    {
        var payload = new
        {
            version = 3,
            file = document.File,
            sources = document.Sources.Select(static source => source.Path).ToArray(),
            sourcesContent = document.Sources.Select(static source => source.Content).ToArray(),
            names = Array.Empty<string>(),
            mappings = BuildMappings(document.Segments)
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }

    public string AppendSourceMappingUrl(string code, string mapFileName)
    {
        var normalized = (code ?? string.Empty).TrimEnd('\r', '\n');
        return normalized + Environment.NewLine + $"//# sourceMappingURL={mapFileName}" + Environment.NewLine;
    }

    private static string BuildMappings(IReadOnlyList<SourceMapSegment> segments)
    {
        if (segments.Count == 0)
            return string.Empty;

        var builder = new StringBuilder();
        var generatedLine = 0;
        var previousGeneratedColumn = 0;
        var previousSourceIndex = 0;
        var previousSourceLine = 0;
        var previousSourceColumn = 0;

        foreach (var group in segments.GroupBy(static segment => segment.GeneratedLine).OrderBy(static group => group.Key))
        {
            while (generatedLine < group.Key)
            {
                builder.Append(';');
                generatedLine++;
                previousGeneratedColumn = 0;
            }

            var first = true;
            foreach (var segment in group.OrderBy(static segment => segment.GeneratedColumn))
            {
                if (!first)
                    builder.Append(',');
                first = false;

                builder.Append(EncodeVlq(segment.GeneratedColumn - previousGeneratedColumn));
                builder.Append(EncodeVlq(segment.SourceIndex - previousSourceIndex));
                builder.Append(EncodeVlq(segment.SourceLine - previousSourceLine));
                builder.Append(EncodeVlq(segment.SourceColumn - previousSourceColumn));

                previousGeneratedColumn = segment.GeneratedColumn;
                previousSourceIndex = segment.SourceIndex;
                previousSourceLine = segment.SourceLine;
                previousSourceColumn = segment.SourceColumn;
            }
        }

        return builder.ToString();
    }

    private static string EncodeVlq(int value)
    {
        var vlq = value < 0 ? ((-value) << 1) + 1 : (value << 1);
        var builder = new StringBuilder();
        do
        {
            var digit = vlq & 31;
            vlq >>= 5;
            if (vlq > 0)
                digit |= 32;
            builder.Append(Base64Digits[digit]);
        }
        while (vlq > 0);

        return builder.ToString();
    }
}
```

- [ ] **Step 6: Run the new unit tests to verify they pass**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.EmitTest/Jazor.EmitTest.csproj" --filter "FullyQualifiedName~SourceMapBuilderTests|FullyQualifiedName~SourceMapWriterTests"
```

Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add src/Jazor.Emit/SourceMaps src/Jazor.EmitTest/SourceMapBuilderTests.cs src/Jazor.EmitTest/SourceMapWriterTests.cs
git commit -m "feat: add source map builder and writer"
```

### Task 3: Emit module-level `.mjs.map` files for RazorVue artifacts

**Files:**
- Modify: `src/Jazor.Emit/RazorVueModuleWriter.cs`
- Test: `src/Jazor.EmitTest/RazorVueEmitIntegrationTests.cs`

- [ ] **Step 1: Write the failing integration test for emitted `.map` files**

In `src/Jazor.EmitTest/RazorVueEmitIntegrationTests.cs`, extend `RazorVueModuleWriter_WritesArtifactsAndManifest()` with:

```csharp
var modulePath = Path.Combine(outputDirectory, "components", "counter-card.mjs");
var mapPath = modulePath + ".map";

Assert.IsTrue(File.Exists(modulePath));
Assert.IsTrue(File.Exists(mapPath));

var moduleCode = File.ReadAllText(modulePath);
StringAssert.Contains(moduleCode, "//# sourceMappingURL=counter-card.mjs.map");

using var map = JsonDocument.Parse(File.ReadAllText(mapPath));
Assert.AreEqual("components/counter-card.mjs", map.RootElement.GetProperty("file").GetString());
Assert.AreEqual("Counter.razor", map.RootElement.GetProperty("sources")[0].GetString());
Assert.AreEqual("Counter component source", map.RootElement.GetProperty("sourcesContent")[0].GetString());
Assert.AreNotEqual(string.Empty, map.RootElement.GetProperty("mappings").GetString());
```

Update the test artifact to carry the generated fields and make `SourceFilePath` a real temporary file created by the test:

```csharp
var sourceFilePath = Path.Combine(root, "Counter.razor");
File.WriteAllText(sourceFilePath, "Counter component source");
```

and build the origin as:

```csharp
new RazorVueEmitSourceOriginRecord(
    sourceFilePath,
    12,
    8,
    2,
    4,
    "components/counter-card.mjs",
    0,
    38,
    RazorVueMappingQualityRecord.MappedFromGenerated,
    RazorVueOriginProvenanceRecord.GeneratedSyntaxLocation)
```

- [ ] **Step 2: Run the integration test to verify it fails**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.EmitTest/Jazor.EmitTest.csproj" --filter "FullyQualifiedName~RazorVueEmitIntegrationTests"
```

Expected: FAIL because `.mjs.map` is not written yet.

- [ ] **Step 3: Update `RazorVueModuleWriter` to write `.map` files and comments**

At the top of `src/Jazor.Emit/RazorVueModuleWriter.cs`, add:

```csharp
using Jazor.Emit.SourceMaps;
```

Inside `Write(...)`, before the artifact loop, create the helpers:

```csharp
var sourceMapBuilder = new SourceMapBuilder();
var sourceMapWriter = new SourceMapWriter();
```

Replace the file-write body from:

```csharp
File.WriteAllText(targetPath, artifact.ModuleCode, Utf8WithoutBom);
written++;
```

to:

```csharp
var sourceMapDocument = sourceMapBuilder.BuildModuleMap(
    generatedFileName: artifact.RelativeModulePath,
    moduleCode: artifact.ModuleCode,
    origins: artifact.SourceOrigins,
    readSourceContent: static path =>
    {
        try
        {
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }
        catch
        {
            return null;
        }
    });

var mapPath = targetPath + ".map";
var mapFileName = Path.GetFileName(mapPath);
var moduleCode = sourceMapWriter.AppendSourceMappingUrl(artifact.ModuleCode, mapFileName);

File.WriteAllText(targetPath, moduleCode, Utf8WithoutBom);
File.WriteAllText(mapPath, sourceMapWriter.Write(sourceMapDocument), Utf8WithoutBom);
written++;
```

Update the skip condition so unchanged artifacts still regenerate the output when the `.map` is missing:

```csharp
if (existingByPath.TryGetValue(artifact.RelativeModulePath, out var existingEntry) &&
    StringComparer.Ordinal.Equals(existingEntry.ContentHash, contentHash) &&
    File.Exists(targetPath) &&
    File.Exists(targetPath + ".map"))
{
    skipped++;
}
```

Update clean-up so old maps are deleted together with old modules:

```csharp
var oldMapPath = oldTargetPath + ".map";
if (File.Exists(oldMapPath))
    File.Delete(oldMapPath);
```

- [ ] **Step 4: Run the integration test to verify it passes**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.EmitTest/Jazor.EmitTest.csproj" --filter "FullyQualifiedName~RazorVueEmitIntegrationTests"
```

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/Jazor.Emit/RazorVueModuleWriter.cs src/Jazor.EmitTest/RazorVueEmitIntegrationTests.cs
git commit -m "feat: emit RazorVue module sourcemaps"
```

### Task 4: Chain module sourcemaps into the final bundle map

**Files:**
- Create: `src/Jazor.Emit/SourceMaps/SourceMapChainBuilder.cs`
- Modify: `src/Jazor.Emit/ModuleBundler.cs`
- Test: `src/Jazor.EmitTest/SourceMapChainBuilderTests.cs`
- Test: `src/Jazor.EmitTest/ModuleBundlerTests.cs`

- [ ] **Step 1: Write the failing chaining tests**

Create `src/Jazor.EmitTest/SourceMapChainBuilderTests.cs` with:

```csharp
using Jazor.Emit.SourceMaps;
using System.Text.Json;

namespace Jazor.EmitTest;

[TestClass]
public sealed class SourceMapChainBuilderTests
{
    [TestMethod]
    public void Chain_RewritesBundleSourcesBackToOriginalSource()
    {
        var chainBuilder = new SourceMapChainBuilder();
        var bundleMapJson = """
        {
          "version": 3,
          "file": "bundle.js",
          "sources": ["./components/counter-card.mjs"],
          "sourcesContent": ["export default { name: 'CounterCard' };"],
          "names": [],
          "mappings": "AAAA"
        }
        """;
        var moduleMapJson = """
        {
          "version": 3,
          "file": "components/counter-card.mjs",
          "sources": ["Counter.razor"],
          "sourcesContent": ["Counter component source"],
          "names": [],
          "mappings": "AAAA"
        }
        """;

        var chained = chainBuilder.Chain(
            bundleFileName: "bundle.js",
            bundleMapJson,
            resolveModuleMap: path => path == "./components/counter-card.mjs" ? moduleMapJson : null);

        using var parsed = JsonDocument.Parse(chained);
        Assert.AreEqual("Counter.razor", parsed.RootElement.GetProperty("sources")[0].GetString());
        Assert.AreEqual("Counter component source", parsed.RootElement.GetProperty("sourcesContent")[0].GetString());
    }
}
```

Extend `src/Jazor.EmitTest/ModuleBundlerTests.cs` by adding this test:

```csharp
[TestMethod]
public async Task BundleAsync_WritesExternalSourceMapAndSourceMappingUrl()
{
    using var workspace = new TestWorkspace();
    WriteModule(workspace.InputDirectory, "components/counter-card.mjs",
        "export default { name: 'CounterCard' };\n//# sourceMappingURL=counter-card.mjs.map\n");
    WriteModule(workspace.InputDirectory, "components/counter-card.mjs.map",
        """
        {
          "version": 3,
          "file": "components/counter-card.mjs",
          "sources": ["Counter.razor"],
          "sourcesContent": ["Counter component source"],
          "names": [],
          "mappings": "AAAA"
        }
        """);

    var manifest = new ManifestModel(
        RootAssemblyPath: Path.Combine(workspace.RootPath, "SdkSmoke.dll"),
        GeneratedAtUtc: DateTime.UtcNow,
        Modules:
        [
            new ManifestModuleEntry("SdkSmoke", "CounterCard", "CounterCard", "components/counter-card.mjs", "hash-1")
        ]);
    manifest.Save(workspace.ManifestPath);

    var bundler = new ModuleBundler();
    var result = await bundler.BundleAsync(new BundleOptions(workspace.InputDirectory, workspace.ManifestPath, workspace.OutputPath));

    Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
    Assert.IsTrue(File.Exists(workspace.OutputPath + ".map"));

    var bundle = await File.ReadAllTextAsync(workspace.OutputPath, TestContext.CancellationTokenSource.Token);
    StringAssert.Contains(bundle, "//# sourceMappingURL=bundle.js.map");

    using var parsed = JsonDocument.Parse(await File.ReadAllTextAsync(workspace.OutputPath + ".map", TestContext.CancellationTokenSource.Token));
    Assert.AreEqual("Counter.razor", parsed.RootElement.GetProperty("sources")[0].GetString());
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.EmitTest/Jazor.EmitTest.csproj" --filter "FullyQualifiedName~SourceMapChainBuilderTests|FullyQualifiedName~ModuleBundlerTests"
```

Expected: FAIL because chaining and final bundle map output do not exist yet.

- [ ] **Step 3: Create `SourceMapChainBuilder.cs`**

Create `src/Jazor.Emit/SourceMaps/SourceMapChainBuilder.cs` with:

```csharp
using System.Text.Json;

namespace Jazor.Emit.SourceMaps;

internal sealed class SourceMapChainBuilder
{
    public string Chain(string bundleFileName, string bundleMapJson, Func<string, string?> resolveModuleMap)
    {
        using var bundleMap = JsonDocument.Parse(bundleMapJson);
        var bundleSources = bundleMap.RootElement.GetProperty("sources").EnumerateArray().Select(static item => item.GetString() ?? string.Empty).ToArray();
        var bundleMappings = bundleMap.RootElement.GetProperty("mappings").GetString() ?? string.Empty;

        for (var i = 0; i < bundleSources.Length; i++)
        {
            var moduleMapJson = resolveModuleMap(bundleSources[i]);
            if (string.IsNullOrWhiteSpace(moduleMapJson))
                continue;

            using var moduleMap = JsonDocument.Parse(moduleMapJson);
            var moduleSources = moduleMap.RootElement.GetProperty("sources").EnumerateArray().Select(static item => item.GetString() ?? string.Empty).ToArray();
            if (moduleSources.Length == 0)
                continue;

            var moduleSourcesContent = moduleMap.RootElement.TryGetProperty("sourcesContent", out var contentElement)
                ? contentElement.EnumerateArray().Select(static item => item.ValueKind == JsonValueKind.Null ? null : item.GetString()).ToArray()
                : new string?[moduleSources.Length];

            var payload = new
            {
                version = 3,
                file = bundleFileName,
                sources = moduleSources,
                sourcesContent = moduleSourcesContent,
                names = Array.Empty<string>(),
                mappings = bundleMappings
            };

            return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        }

        return bundleMapJson;
    }
}
```

- [ ] **Step 4: Update `ModuleBundler` to request external maps, copy module maps, and chain**

At the top of `src/Jazor.Emit/ModuleBundler.cs`, add:

```csharp
using Jazor.Emit.SourceMaps;
```

Inside the `foreach (var relativePath in relativePaths)` loop, after writing the rewritten module, copy any sibling `.map` file into the workspace:

```csharp
var sourceMapPath = sourcePath + ".map";
var targetMapPath = targetPath + ".map";
if (File.Exists(sourceMapPath))
    File.Copy(sourceMapPath, targetMapPath, overwrite: true);
```

Update the Deno arguments from:

```csharp
var commandArgs = new[]
{
    "bundle",
    "--platform",
    "browser",
    "-o",
    options.OutputPath,
    Path.GetFileName(tempEntryPath)
};
```

to:

```csharp
var commandArgs = new[]
{
    "bundle",
    "--platform",
    "browser",
    "--sourcemap",
    "external",
    "-o",
    options.OutputPath,
    Path.GetFileName(tempEntryPath)
};
```

After `Deno.Execute(...)`, add:

```csharp
var writer = new SourceMapWriter();
var chainBuilder = new SourceMapChainBuilder();
var bundleMapPath = options.OutputPath + ".map";

if (File.Exists(options.OutputPath))
{
    var bundleCode = await File.ReadAllTextAsync(options.OutputPath);
    await File.WriteAllTextAsync(
        options.OutputPath,
        writer.AppendSourceMappingUrl(bundleCode, Path.GetFileName(bundleMapPath)),
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

if (File.Exists(bundleMapPath))
{
    var chained = chainBuilder.Chain(
        bundleFileName: Path.GetFileName(options.OutputPath),
        bundleMapJson: await File.ReadAllTextAsync(bundleMapPath),
        resolveModuleMap: relativeSourcePath =>
        {
            var normalized = relativeSourcePath.Replace('/', Path.DirectorySeparatorChar);
            var candidate = Path.GetFullPath(Path.Combine(bundleWorkspace, normalized));
            var mapCandidate = candidate + ".map";
            return File.Exists(mapCandidate) ? File.ReadAllText(mapCandidate) : null;
        });

    await File.WriteAllTextAsync(bundleMapPath, chained, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}
```

Keep the current failure fallback policy: if `.map` is missing or a module has no `.map`, return the original Deno bundle output instead of failing the bundle.

- [ ] **Step 5: Run the chaining tests to verify they pass**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.EmitTest/Jazor.EmitTest.csproj" --filter "FullyQualifiedName~SourceMapChainBuilderTests|FullyQualifiedName~ModuleBundlerTests"
```

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add src/Jazor.Emit/SourceMaps/SourceMapChainBuilder.cs src/Jazor.Emit/ModuleBundler.cs src/Jazor.EmitTest/SourceMapChainBuilderTests.cs src/Jazor.EmitTest/ModuleBundlerTests.cs
git commit -m "feat: chain module sourcemaps into bundled output"
```

### Task 5: Verify end-to-end outputs and sync source map docs

**Files:**
- Modify: `src/Jazor.EmitTest/SdkIntegrationTests.cs`
- Modify: `src/Jazor.Compiler/doc/SourceMap.Overview.md`
- Modify: `src/Jazor.Compiler/doc/SourceMap.ImplementationChecklist.md`

- [ ] **Step 1: Write the failing SDK integration assertions**

In `src/Jazor.EmitTest/SdkIntegrationTests.cs`, extend `Build_LocalJazorPackage_MultiProjectSample_EmitsModulesAndBundle()` with:

```csharp
var bundleMapPath = Path.Combine(hostRoot, "wwwroot", "app.bundle.js.map");
Assert.IsTrue(File.Exists(bundleMapPath), $"Bundle sourcemap was not generated: {bundleMapPath}");

var bundleWithComment = await File.ReadAllTextAsync(bundlePath);
StringAssert.Contains(bundleWithComment, "//# sourceMappingURL=app.bundle.js.map");

using var bundleMap = JsonDocument.Parse(await File.ReadAllTextAsync(bundleMapPath));
Assert.AreEqual("app.bundle.js", bundleMap.RootElement.GetProperty("file").GetString());
Assert.IsTrue(bundleMap.RootElement.GetProperty("sources").GetArrayLength() > 0);
Assert.AreNotEqual(string.Empty, bundleMap.RootElement.GetProperty("mappings").GetString());
```

- [ ] **Step 2: Run the SDK integration test to verify it fails**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.EmitTest/Jazor.EmitTest.csproj" --filter "FullyQualifiedName~SdkIntegrationTests"
```

Expected: FAIL because the final bundle `.map` does not exist yet.

- [ ] **Step 3: Update the source map docs to match the implemented scope**

In `src/Jazor.Compiler/doc/SourceMap.Overview.md`, replace:

```md
5. 第一阶段不做 bundle map chaining
```

with:

```md
5. 当前执行阶段同时覆盖模块级 `.mjs.map` 与 bundle map chaining
```

In `src/Jazor.Compiler/doc/SourceMap.ImplementationChecklist.md`, replace:

```md
第一阶段不做：

1. bundle map chaining
2. token 级极致 mapping
3. 所有语法点一次性全覆盖
```

with:

```md
当前执行阶段不做：

1. token 级极致 mapping
2. 所有语法点一次性全覆盖
3. HMR runtime / watch mode
```

and replace:

```md
### Step 8. 扩测试
```

with:

```md
### Step 8. 扩测试并验证 bundle chaining
```
```

Then replace:

```md
4. `ModuleBundler` 当前行为边界
```

with:

```md
4. `ModuleBundler` 在开启 external sourcemap 后仍保持现有 bundle 语义
```

- [ ] **Step 4: Run the focused regression suite**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.EmitTest/Jazor.EmitTest.csproj"
```

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/Jazor.EmitTest/SdkIntegrationTests.cs src/Jazor.Compiler/doc/SourceMap.Overview.md src/Jazor.Compiler/doc/SourceMap.ImplementationChecklist.md
git commit -m "docs: align source map docs with bundle chaining rollout"
```

---

## Self-Review Checklist

### Spec coverage
- Module-level `.mjs.map`: covered by Task 2 + Task 3
- `sourcesContent`: covered by Task 2 + Task 3
- `sourceMappingURL`: covered by Task 2 + Task 3 + Task 4
- bundle chaining: covered by Task 4
- final bundle `.map`: covered by Task 4 + Task 5
- docs sync for old “no chaining” conclusion: covered by Task 5
- generated span preservation from existing RazorVue carrier: covered by Task 1

### Placeholder scan
- No `TODO`, `TBD`, `implement later`, `fill in details`
- No “write tests for the above” without actual test code
- No “similar to Task N” references

### Type consistency
- `RazorVueEmitSourceOriginRecord` consistently includes `GeneratedFilePath`, `GeneratedSpanStart`, `GeneratedSpanLength`
- Source map core names are fixed as `SourceMapDocument`, `SourceMapSource`, `SourceMapSegment`, `SourceMapBuilder`, `SourceMapWriter`, `SourceMapChainBuilder`
- All emitted map names use the same convention: `<module>.mjs.map`, `<bundle>.js.map`

---

Plan complete and saved to `docs/superpowers/plans/2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md`. Two execution options:

**1. Subagent-Driven (recommended)** - 我为每个 task 派发一个新的 subagent，任务间做 review，迭代更稳

**2. Inline Execution** - 在当前会话里按计划直接执行，分批实现并在检查点停下

**Which approach?**
