# Phase 2: 编译管道统一 + Source Map — 详细实施计划

## 目标

为 VueHost 编译管道添加标准 Source Map v3 支持，使浏览器 DevTools 可以将编译后的 JS 映射回 `.jazor` / `.vue` / `.ts` 源码，支持断点调试和调用栈还原。

**验收标准**: 浏览器 DevTools 的 Sources 面板中可见 `.jazor` 源码，断点可命中，调用栈显示 `.jazor` 文件名和行号。

## 当前实现状态（2026-04-17）

### 已完成

- `compile/ts` 已从 Deno worker 返回 `jsSourceMap`，并透传到 `DenoFrontendModuleCompiler`。
- `compile/sfc` 已从 Deno worker 返回 `.vue -> .js` 行级 `jsSourceMap`，并透传到 `DenoFrontendModuleCompiler`。
- `OnDemandCompiler` 在 TypeScript 模块存在 Source Map 时，会为输出 JS 追加 inline `sourceMappingURL=data:application/json;base64,...`。
- `OnDemandCompiler` 已处理带 `<style>` 的 SFC 模块样式注入前缀，对返回给浏览器的 sourcemap 进行生成行偏移。
- `JazorVueCompiler` 已产出 `.jazor -> .g.vue` 行级 Source Map，覆盖 import/helper、Prop/State/Computed、方法声明/方法体、保留诊断注释与 template 输出行。
- `.jazor` 编译路径已将 worker 的 `.js -> .g.vue` map 与 `JazorVueCompiler` 输出的 `.jazor -> .g.vue` map 通过 `Jazor.Emit.SourceMaps.SourceMapChainBuilder` 链回原始 `.jazor`，并在 inline/external Source Map 中返回原始 `.jazor` 的 `sourcesContent`。
- Dev Server 已支持对具备 `SourceMap` 的编译模块通过 `{module}.map` 返回 external Source Map JSON，覆盖 `.ts`、`.vue` 与 `.jazor`。
- 已补齐 TypeScript inline sourcemap 回归，覆盖 Deno host 协议透传、`OnDemandCompiler` 注入，以及 Dev Server HTTP 输出。
- 已补齐 Vue SFC sourcemap 回归，覆盖 Deno host 协议透传、`DenoFrontendModuleCompiler` 透传、inline 注入、`.vue.map`、未保存 workspace 文本，以及 style wrapper 偏移。
- 已补齐 `.jazor` 链式 sourcemap 回归，覆盖 `JazorVueCompiler` 生成中间 map、`OnDemandCompiler` 将 worker sourcemap 链回原始 `.jazor` 源、Dev Server `.jazor.map`、未保存 workspace 文本，以及实际 `mappings` 行号。
- 已补齐真实浏览器/CDP + HMR 长链路压测（环境变量门控），覆盖多轮热更新后断点重绑、调用栈回填与列号映射稳定性。
- `JazorVueCompiler` 已将 `.jazor -> .g.vue` 列映射从“单锚点”提升为“多锚点”策略：同一生成行可输出多个 token 级列段，并保留缩进偏移回退段，减少列 0 退化。
- Deno worker 已将 Vue 编译器原生 script/template source map 进行列级链式合并增强：按行排序段、早列回退到首段、列回推负偏移保护，复杂转换行不再轻易退化到 line-map。
- build 产物回归已补“模板 token 逆向列号 > 0”断言，验证最终 external source map 可链回原始 `.vue` 作者列信息。
- 真实浏览器/CDP + HMR 压测已补 `scopes/variables/evaluate/continue` 闭环断言，覆盖多轮热更新后调试会话的一致性。

### 尚未完成

- SourceMap 主链路（生成、链式、调试消费）已可用且具备列级增强；后续仍需继续扩展真实浏览器/CDP + HMR 的更高压力矩阵（并发断点、异常栈、长时运行）以及调试可视化/诊断端点。

---

## 一、Source Map 基础

### 1.1 Source Map v3 格式

```json
{
  "version": 3,
  "file": "App.jazor.js",
  "sourceRoot": "",
  "sources": ["../src/App.jazor"],
  "sourcesContent": ["@code { ... }\n<div>...</div>"],
  "names": [],
  "mappings": "AAAA;AACA;AAEA,SAAS..."
}
```

**mappings 编码规则**:
- 分号 `;` 分隔**生成文件的每一行**
- 逗号 `,` 分隔**同一行内的每个映射段**
- 每个段是 Base64 VLQ 编码的整数序列: `[genCol, sourceIdx, srcLine, srcCol]`
- 所有值都是**相对前一个值的 delta**（跨行重置 genCol）

### 1.2 VLQ 编码示例

```
源码第 3 行第 0 列 → 生成第 1 行第 0 列:
  第一个段: genCol=0, sourceIdx=0, srcLine=3, srcCol=0
  VLQ 编码: 0→A, 0→A, 3→D, 0→A  → "AADA"

源码第 4 行第 0 列 → 生成第 2 行第 0 列:
  与前段 delta: genCol=0(重置), sourceIdx=0, srcLine=+1, srcCol=0
  VLQ 编码: 0→A, 0→A, 1→C, 0→A  → "AACA"
```

### 1.3 两阶段编译的链式映射

```
.jazor 源码
    │  smap1: .jazor → Vue SFC
    ▼
Vue SFC 文本 (.g.vue)
    │  smap2: .vue → JS
    ▼
JS ESM 模块 (.js)
    │
    ▼  合并 smap1 + smap2
最终 Source Map: .jazor → .js (直接映射)
```

---

## 二、关键设计决策

### 2.1 精度目标

| 阶段 | 精度 | 说明 |
|------|------|------|
| **Phase 2 初始** | 行级 | 每个生成行映射到源码行 (genCol=0, srcCol=0) |
| **Phase 2 后续** | 列级 | 方法体 lowering 保留列偏移，模板精确到属性 |

行级映射已经能支持：断点、调用栈文件名/行号、源码查看。列级映射用于精确高亮和表达式级调试。

### 2.2 与 ProjectionMap 的关系

| 维度 | ProjectionMap (已有) | Source Map (本 Phase) |
|------|---------------------|----------------------|
| **用途** | 设计时 LSP 路由 | 构建时调试 |
| **格式** | `ProjectionSegment[]` | 标准 Source Map v3 JSON |
| **映射方向** | `.jazor` ↔ 虚拟文档 | `.jazor` → JS 产物 |
| **生命周期** | didChange 时重建 | 编译时生成，部署后可用 |
| **消费者** | LSP Session, Coordinators | 浏览器 DevTools, DAP 调试器 |
| **精度** | 段/字符级 | 行/列级 |

两者**独立维护**，不共享数据结构。ProjectionMap 服务于 LSP，Source Map 服务于调试。

### 2.3 Source Map 生成位置

| 编译阶段 | Source Map 来源 | 实现方式 |
|---------|----------------|---------|
| `.jazor` → Vue SFC | **.NET 进程内生成** (修改 JazorVueCompiler) | 逐行记录源码行号 |
| Vue SFC → JS | **Deno Worker 返回** (@vue/compiler-sfc 原生支持) | 透传 + 解码 |
| `.ts` → `.js` | **Deno Worker 返回** (TypeScript transpileModule 原生支持) | 透传 + 解码 |
| 链式合并 | **.NET 进程内** | 合并 smap1 + smap2 |

---

## 三、新增文件清单

```
src/Jazor.VueHost/
├── SourceMap/                              # [新建目录]
│   ├── SourceMapGenerator.cs               # Source Map v3 生成器
│   ├── SourceMapConsumer.cs                # Source Map v3 消费者 (逆向映射)
│   ├── SourceMapMerger.cs                  # 链式合并
│   ├── VlqCodec.cs                        # Base64 VLQ 编解码
│   ├── SourceMap.cs                        # Source Map 数据模型
│   ├── SourceMapping.cs                    # 单条映射记录
│   └── ISourceMapService.cs               # Source Map 服务接口
│
├── Jazor/Core/
│   ├── JazorVueCompiler.cs                # [修改] 添加 Source Map 跟踪
│   └── JazorVueCompilationResult.cs       # [修改] 或新建扩展类，携带 Source Map
│
├── DevServer/
│   ├── OnDemandCompiler.cs                # [修改] 集成 Source Map 链式合并
│   ├── DevHttpServer.cs                   # [修改] 服务 .map 文件 + inline Source Map
│   └── CompilationCache.cs               # [修改] 缓存 Source Map
```

---

## 四、接口与类型定义

### 4.1 SourceMap — 数据模型

```csharp
// SourceMap/SourceMap.cs
namespace Jazor.VueHost.SourceMap;

/// <summary>
/// 标准 Source Map v3 数据模型。
/// </summary>
public sealed class SourceMap
{
    /// <summary>Source Map 版本，固定为 3。</summary>
    public int Version => 3;

    /// <summary>生成文件名。</summary>
    public required string File { get; init; }

    /// <summary>源文件路径列表（相对于 sourceRoot）。</summary>
    public required IReadOnlyList<string> Sources { get; init; }

    /// <summary>源文件内容列表（可选，用于 DevTools 显示源码）。</summary>
    public IReadOnlyList<string?>? SourcesContent { get; init; }

    /// <summary>符号名称列表（可选）。</summary>
    public IReadOnlyList<string>? Names { get; init; }

    /// <summary>源文件根路径。</summary>
    public string SourceRoot { get; init; } = "";

    /// <summary>映射段列表，按生成行分组。</summary>
    public required IReadOnlyList<SourceMapLine> Lines { get; init; }

    /// <summary>序列化为标准 JSON。</summary>
    public string ToJson();

    /// <summary>生成 inline data URI。</summary>
    public string ToDataUri()
        => "data:application/json;base64," + Convert.ToBase64String(
            Encoding.UTF8.GetBytes(ToJson()));
}
```

### 4.2 SourceMapLine — 行映射

```csharp
// SourceMap/SourceMapLine.cs
namespace Jazor.VueHost.SourceMap;

/// <summary>
/// 生成文件中一行内的所有映射段。
/// </summary>
public sealed class SourceMapLine
{
    /// <summary>该行内的映射段列表。</summary>
    public required IReadOnlyList<SourceMapping> Mappings { get; init; }
}
```

### 4.3 SourceMapping — 单条映射

```csharp
// SourceMap/SourceMapping.cs
namespace Jazor.VueHost.SourceMap;

/// <summary>
/// 单条源码映射：生成文件位置 → 源文件位置。
/// 对应 Source Map v3 mappings 中的一个 VLQ 段。
/// </summary>
public sealed class SourceMapping
{
    /// <summary>生成文件列号 (0-based)。</summary>
    public required int GeneratedColumn { get; init; }

    /// <summary>源文件索引 (对应 Sources 数组)。</summary>
    public required int SourceIndex { get; init; }

    /// <summary>源文件行号 (0-based)。</summary>
    public required int SourceLine { get; init; }

    /// <summary>源文件列号 (0-based)。</summary>
    public int SourceColumn { get; init; }

    /// <summary>符号名称索引 (可选，对应 Names 数组)。</summary>
    public int? NameIndex { get; init; }
}
```

### 4.4 VlqCodec — VLQ 编解码

```csharp
// SourceMap/VlqCodec.cs
namespace Jazor.VueHost.SourceMap;

/// <summary>
/// Source Map v3 Base64 VLQ 编解码器。
/// </summary>
public static class VlqCodec
{
    private const string Base64Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

    /// <summary>将一组整数编码为 Base64 VLQ 字符串。</summary>
    public static string Encode(ReadOnlySpan<int> values);

    /// <summary>将 Base64 VLQ 字符串解码为一组整数。</summary>
    public static int[] Decode(ReadOnlySpan<char> encoded);

    /// <summary>编码单个 VLQ 值。</summary>
    public static string EncodeValue(int value);

    /// <summary>解码单个 VLQ 值，返回值和消耗的字符数。</summary>
    public static int DecodeValue(ReadOnlySpan<char> encoded, out int consumedChars);
}
```

**VLQ 编码算法**:
```csharp
public static string EncodeValue(int value)
{
    // 1. 如果是负数，标记符号位，取绝对值
    var isNegative = value < 0;
    var vlq = isNegative ? ((-value) << 1) + 1 : value << 1;

    // 2. 6位一组，每组最高位表示"还有后续"
    var sb = new StringBuilder();
    do
    {
        var digit = vlq & 0x1F;         // 低 5 位
        vlq >>= 5;
        if (vlq > 0) digit |= 0x20;     // 设置延续位
        sb.Append(Base64Chars[digit]);
    } while (vlq > 0);

    return sb.ToString();
}
```

### 4.5 SourceMapGenerator — 生成器

```csharp
// SourceMap/SourceMapGenerator.cs
namespace Jazor.VueHost.SourceMap;

/// <summary>
/// Source Map v3 生成器。支持逐行添加映射，最终输出标准 JSON。
/// </summary>
public sealed class SourceMapGenerator
{
    public SourceMapGenerator(string generatedFile);

    /// <summary>添加源文件。</summary>
    /// <returns>源文件索引。</returns>
    public int AddSource(string sourcePath, string? content = null);

    /// <summary>添加符号名称。</summary>
    /// <returns>名称索引。</returns>
    public int AddName(string name);

    /// <summary>
    /// 添加一条映射：当前生成行 + 指定列 → 源文件位置。
    /// 自动处理 delta 编码。
    /// </summary>
    public void AddMapping(int generatedColumn, int sourceIndex, int sourceLine, int sourceColumn, int? nameIndex = null);

    /// <summary>
    /// 添加一条行级映射（整行映射到源码行，列号=0）。
    /// Phase 2 初始精度：大多数映射使用此方法。
    /// </summary>
    public void AddLineMapping(int sourceIndex, int sourceLine);

    /// <summary>推进到下一生成行。</summary>
    public void NextLine();

    /// <summary>生成最终的 Source Map。</summary>
    public SourceMap ToSourceMap();

    /// <summary>直接输出 JSON 字符串。</summary>
    public string ToJson();
}
```

### 4.6 SourceMapConsumer — 消费者 (逆向映射)

```csharp
// SourceMap/SourceMapConsumer.cs
namespace Jazor.VueHost.SourceMap;

/// <summary>
/// Source Map 消费者：支持从生成文件位置反查源码位置。
/// </summary>
public sealed class SourceMapConsumer
{
    public SourceMapConsumer(SourceMap sourceMap);
    public SourceMapConsumer(string sourceMapJson);

    /// <summary>逆向映射：生成文件位置 → 源码位置。</summary>
    public OriginalPosition? OriginalPositionFor(int generatedLine, int generatedColumn);

    /// <summary>正向映射：源码位置 → 生成文件位置。</summary>
    public GeneratedPosition? GeneratedPositionFor(int sourceIndex, int sourceLine, int sourceColumn);

    /// <summary>获取源文件内容。</summary>
    public string? GetSourceContent(int sourceIndex);
}

public sealed class OriginalPosition
{
    public required string Source { get; init; }
    public required int Line { get; init; }      // 0-based
    public required int Column { get; init; }     // 0-based
    public string? Name { get; init; }
}

public sealed class GeneratedPosition
{
    public required int Line { get; init; }       // 0-based
    public required int Column { get; init; }     // 0-based
}
```

### 4.7 SourceMapMerger — 链式合并

```csharp
// SourceMap/SourceMapMerger.cs
namespace Jazor.VueHost.SourceMap;

/// <summary>
/// 链式 Source Map 合并器。
/// 合并 "A → B" (smap1) 和 "B → C" (smap2) 为 "A → C"。
/// </summary>
public static class SourceMapMerger
{
    /// <summary>
    /// 合并两个 Source Map。
    /// smap1: source(A) → generated(B)
    /// smap2: source(B) → generated(C)
    /// 返回: source(A) → generated(C)
    /// </summary>
    public static SourceMap Merge(SourceMap smap1, SourceMap smap2);
}
```

**合并算法**:
```
遍历 smap2 的每个映射段:
  mapping2: genLine:C, genCol:C → srcLine:B, srcCol:B (在中间文件 B 中)

  用 mapping2 的 srcLine:B, srcCol:B 查找 smap1 的逆向映射:
    smap1 逆向: srcLine:B, srcCol:B → genLine:A, genCol:A (在源文件 A 中)

  输出新映射: genLine:C, genCol:C → srcLine:A, srcCol:A (源文件 A)
```

### 4.8 ISourceMapService — 服务接口

```csharp
// SourceMap/ISourceMapService.cs
namespace Jazor.VueHost.SourceMap;

/// <summary>
/// Source Map 服务接口。为 Dev Server 和未来 DAP 调试器提供映射服务。
/// </summary>
public interface ISourceMapService
{
    /// <summary>注册一个编译产物的 Source Map。</summary>
    void Register(string generatedPath, SourceMap sourceMap);

    /// <summary>移除一个编译产物的 Source Map。</summary>
    void Unregister(string generatedPath);

    /// <summary>获取编译产物的 Source Map JSON。</summary>
    string? GetSourceMapJson(string generatedPath);

    /// <summary>逆向映射：产物位置 → 源码位置。</summary>
    OriginalPosition? OriginalPositionFor(string generatedPath, int line, int column);

    /// <summary>正向映射：源码位置 → 产物位置。</summary>
    GeneratedPosition? GeneratedPositionFor(string sourcePath, int line, int column);

    /// <summary>获取源文件内容（用于调试器显示）。</summary>
    string? GetSourceContent(string generatedPath, int sourceIndex);
}
```

---

## 五、核心实现细节

### 5.1 JazorVueCompiler Source Map 跟踪

**当前问题**: `JazorVueCompiler.Compile()` 使用 `StringBuilder` 逐行构建输出，但不记录哪一行输出来自哪一行源码。

**修改策略**: 引入 `SourceMapGenerator` 作为编译器的伴随输出，每次 `builder.AppendLine()` 时同步记录映射。

#### 5.1.1 编译器行号跟踪

```csharp
// JazorVueCompiler.cs — 修改 Compile() 方法内部
public JazorVueCompilationResult Compile(JazorVueDocument document)
{
    var diagnostics = new List<string>();
    // ... 现有初始化 ...

    var builder = new StringBuilder();
    var sourceMapGen = new SourceMapGenerator(
        generatedFile: Path.GetFileName(document.FilePath) + ".g.vue");

    // 源文件索引
    var sourceIndex = sourceMapGen.AddSource(
        sourcePath: Path.GetFileName(document.FilePath),
        content: document.SourceText);

    // 记录源码中各区域的起始行号
    var templateStartLine = GetLineFromOffset(document.SourceText, document.TemplateStartIndex);
    var codeStartLine = GetLineFromOffset(document.SourceText, document.CodeStartIndex);

    // === <script setup> ===
    builder.AppendLine("<script setup>");
    sourceMapGen.NextLine();
    // ↑ 这是生成的样板代码，不映射到源码

    // Vue helpers import
    if (vueHelpers.Count > 0)
    {
        builder.Append("import { ")
            .Append(string.Join(", ", vueHelpers))
            .AppendLine(" } from \"vue\";");
        sourceMapGen.NextLine();
        // ↑ 样板代码，不映射
    }

    // ... 用户 import 语句 ...
    foreach (var importStatement in imports)
    {
        builder.AppendLine(importStatement);
        sourceMapGen.NextLine();
        // 用户 import → 映射到 @module 指令行（@import/@vueimport/@jsimport 均为不支持输入）
        // Phase 2 初始: 不映射 import 语句（它们来自 JazorVueParser 解析，不是逐行对应）
    }

    // === Props ===
    if (props.Count > 0)
    {
        builder.AppendLine();
        sourceMapGen.NextLine();

        builder.AppendLine("const props = defineProps({");
        sourceMapGen.NextLine();
        // ↑ 样板代码

        foreach (var prop in props)
        {
            builder.Append("  ")
                .Append(prop.RuntimeName)
                .Append(": ")
                .Append(prop.VueTypeExpression)
                .AppendLine(",");
            // 映射到源码中 [Prop] 声明行
            sourceMapGen.AddLineMapping(sourceIndex, codeStartLine + prop.SourceLine);
            sourceMapGen.NextLine();
        }

        builder.AppendLine("});");
        sourceMapGen.NextLine();

        foreach (var prop in props)
        {
            builder.Append("const ")
                .Append(prop.RuntimeName)
                .Append(" = toRef(props, \"")
                .Append(prop.RuntimeName)
                .AppendLine("\");");
            sourceMapGen.AddLineMapping(sourceIndex, codeStartLine + prop.SourceLine);
            sourceMapGen.NextLine();
        }
    }

    // === States ===
    if (states.Count > 0)
    {
        builder.AppendLine();
        sourceMapGen.NextLine();

        foreach (var state in states)
        {
            builder.Append("const ")
                .Append(state.RuntimeName)
                .Append(" = ref(")
                .Append(LowerExpression(state.Initializer ?? "undefined", loweringContext, EmptyShadowedNames))
                .AppendLine(");");
            sourceMapGen.AddLineMapping(sourceIndex, codeStartLine + state.SourceLine);
            sourceMapGen.NextLine();
        }
    }

    // === Methods ===
    if (methods.Count > 0)
    {
        builder.AppendLine();
        sourceMapGen.NextLine();

        foreach (var method in methods)
        {
            builder.Append(method.IsAsync ? "async function " : "function ")
                .Append(method.RuntimeName)
                .Append("(")
                .Append(string.Join(", ", method.Parameters))
                .AppendLine(") {");
            sourceMapGen.AddLineMapping(sourceIndex, codeStartLine + method.SourceLine);
            sourceMapGen.NextLine();

            if (TryLowerMethodBody(method, loweringContext, out var loweredBody))
            {
                foreach (var line in loweredBody)
                {
                    builder.Append("  ").AppendLine(line);
                    // 方法体行 → 映射到源码中对应的行
                    // Phase 2 初始: 所有方法体行映射到方法声明行
                    sourceMapGen.AddLineMapping(sourceIndex, codeStartLine + method.SourceLine);
                    sourceMapGen.NextLine();
                }
            }

            builder.AppendLine("}");
            sourceMapGen.AddLineMapping(sourceIndex, codeStartLine + method.SourceLine);
            sourceMapGen.NextLine();
        }
    }

    // === @code 保留注释 ===
    if (!string.IsNullOrWhiteSpace(document.Code))
    {
        builder.AppendLine();
        sourceMapGen.NextLine();
        builder.AppendLine("/*");
        sourceMapGen.NextLine();
        builder.AppendLine(" Original @code block retained for bridge diagnostics:");
        sourceMapGen.NextLine();

        // 注释中的每行 @code → 映射到源码中对应的行
        var codeLines = document.Code.Replace("*/", "* /").Split('\n');
        foreach (var codeLine in codeLines)
        {
            builder.AppendLine(codeLine);
            sourceMapGen.AddLineMapping(sourceIndex, codeStartLine + /* lineOffset */ 0);
            sourceMapGen.NextLine();
        }

        builder.AppendLine("*/");
        sourceMapGen.NextLine();
    }

    // === </script> ===
    builder.AppendLine("</script>");
    sourceMapGen.NextLine();
    builder.AppendLine();
    sourceMapGen.NextLine();

    // === <template> ===
    builder.AppendLine("<template>");
    sourceMapGen.NextLine();

    var templateText = string.IsNullOrWhiteSpace(document.Template) ? "<div />" : document.Template;
    var templateLines = templateText.Split('\n');
    foreach (var templateLine in templateLines)
    {
        builder.AppendLine(templateLine);
        sourceMapGen.AddLineMapping(sourceIndex, templateStartLine + /* lineOffset */ 0);
        sourceMapGen.NextLine();
    }

    builder.AppendLine("</template>");
    sourceMapGen.NextLine();

    // === 产出 ===
    return new JazorVueCompilationResult(
        document,
        externalSymbols,
        builder.ToString(),
        generatedExternalDeclarationsText,
        diagnostics,
        sourceMapGen.ToSourceMap());  // 新增参数
}
```

#### 5.1.2 辅助方法

```csharp
/// <summary>
/// 从文本偏移量计算行号 (0-based)。
/// </summary>
private static int GetLineFromOffset(string text, int offset)
{
    var line = 0;
    for (var i = 0; i < offset && i < text.Length; i++)
    {
        if (text[i] == '\n') line++;
    }
    return line;
}
```

#### 5.1.3 JazorVueCompilationResult 扩展

```csharp
// 在 JazorVueCompilationResult 构造函数中添加可选参数:
public JazorVueCompilationResult(
    JazorVueDocument document,
    VirtualExternalSymbolTable externalSymbols,
    string generatedVueText,
    string generatedExternalDeclarationsText,
    IReadOnlyList<string> diagnostics,
    SourceMap? sourceMap = null)  // 新增
{
    // ... 现有赋值 ...
    SourceMap = sourceMap;
}

/// <summary>
/// 编译产物的 Source Map (.jazor → .g.vue)，可选。
/// Phase 2 新增。
/// </summary>
public SourceMap? SourceMap { get; }
```

### 5.2 Deno Worker Source Map 透传

#### 5.2.1 frontend-worker.ts — compileSfc 返回 Source Map

```typescript
// 修改 compileVueSfc 返回类型:
interface SfcCompileResult {
    jsContent: string;
    jsSourceMap: string | null;     // 新增: Vue SFC 编译产物的 Source Map
    cssContent: string | null;
    diagnostics: string[];
}

function compileVueSfc(sfcText: string, filename: string): SfcCompileResult {
    const descriptor = parse(sfcText, { filename });
    // ...

    // 编译 <script setup> 时请求 Source Map
    if (descriptor.scriptSetup) {
        const compiled = compileScript(descriptor, {
            id: filename,
            isProd: false,
            genDefaultAs: "_sfc_main",
        });
        jsContent += compiled.content + "\n";
        jsSourceMap = compiled.map
            ? JSON.stringify(compiled.map)
            : null;
    }

    // 编译 <template> 时请求 Source Map
    if (descriptor.template) {
        const templateCompiled = compileTemplate({
            source: descriptor.template.content,
            filename,
            id: filename,
            isProd: false,
        });
        // template render function 的 Source Map
        // (通常不需要合并到最终 JS 的 Source Map 中，
        //  因为 render function 是 Vue 运行时调用的)
    }

    return { jsContent, jsSourceMap, cssContent, diagnostics };
}
```

#### 5.2.2 DenoSfcCompileResult 扩展

```csharp
// Frontend/Deno/Protocol/DenoCompilationProtocol.cs — 扩展
internal sealed class DenoSfcCompileResult
{
    public required string JsContent { get; init; }
    public string? JsSourceMap { get; init; }        // 新增
    public string? CssContent { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}
```

### 5.3 OnDemandCompiler 集成链式 Source Map

```csharp
// DevServer/OnDemandCompiler.cs — 修改 CompileJazorAsync

private async ValueTask<CompilationResult> CompileJazorAsync(
    string path, string sourceText, CancellationToken ct)
{
    // Stage 1: .jazor → Vue SFC (带 smap1)
    var document = _parser.Parse(path, sourceText);
    var compilation = _compiler.Compile(document);

    // smap1: .jazor → .g.vue
    var smap1 = compilation.SourceMap;

    if (_denoVolarHost is not { IsRunning: true })
    {
        return new CompilationResult { /* ... 降级 ... */ };
    }

    // Stage 2: .g.vue → .js (带 smap2)
    var sfcResult = await _denoVolarHost.CompileSfcAsync(
        path, compilation.GeneratedVueText, Path.GetFileName(path), ct);

    // smap2: .g.vue → .js (从 Deno Worker 返回)
    SourceMap? smap2 = null;
    if (sfcResult?.JsSourceMap is string smap2Json)
    {
        smap2 = SourceMapConsumer.Parse(smap2Json);
    }

    // Stage 3: 合并 smap1 + smap2 → 最终 .jazor → .js
    SourceMap? finalSourceMap = null;
    string? sourceMapJson = null;
    if (smap1 is not null && smap2 is not null)
    {
        finalSourceMap = SourceMapMerger.Merge(smap1, smap2);
        sourceMapJson = finalSourceMap.ToJson();
    }
    else if (smap2 is not null)
    {
        // 没有 smap1 时，直接使用 smap2（.vue → .js 映射）
        finalSourceMap = smap2;
        sourceMapJson = smap2.ToJson();
    }

    var jsContent = sfcResult!.JsContent;
    if (sourceMapJson is not null)
    {
        // 追加 sourceMappingURL 注释
        var mapFileName = Path.GetFileName(path)
            .Replace(".jazor", ".jazor.map", StringComparison.OrdinalIgnoreCase);
        jsContent += $"\n//# sourceMappingURL={mapFileName}\n";
    }

    return new CompilationResult
    {
        ContentType = "text/javascript",
        Content = jsContent,
        SourceMap = sourceMapJson,             // Phase 2 新增
        Dependencies = ExtractImportPaths(document),
        Diagnostics = [..compilation.Diagnostics, ..sfcResult.Diagnostics]
    };
}
```

### 5.4 SourceMapMerger — 链式合并算法

```csharp
// SourceMap/SourceMapMerger.cs
public static SourceMap Merge(SourceMap smap1, SourceMap smap2)
{
    // smap1: .jazor (A) → .g.vue (B)
    // smap2: .g.vue (B) → .js (C)
    // 结果: .jazor (A) → .js (C)

    var consumer1 = new SourceMapConsumer(smap1);
    var gen = new SourceMapGenerator(smap2.File);

    // 添加 smap1 的源文件（最终的源是 A）
    for (var i = 0; i < smap1.Sources.Count; i++)
    {
        gen.AddSource(
            smap1.Sources[i],
            smap1.SourcesContent?[i]);
    }

    // 遍历 smap2 的每个映射段
    foreach (var smap2Line in smap2.Lines)
    {
        foreach (var mapping2 in smap2Line.Mappings)
        {
            // mapping2: genLine:C, genCol:C → srcLine:B, srcCol:B (在 B 中)

            // 用 smap1 的消费者反查 B → A
            var originalInA = consumer1.OriginalPositionFor(
                mapping2.SourceLine,
                mapping2.SourceColumn);

            if (originalInA is not null)
            {
                // 找到 A 中的位置 → 输出新映射
                var sourceIdx = gen.AddSource(originalInA.Source, content: null);
                gen.AddMapping(
                    mapping2.GeneratedColumn,
                    sourceIdx,
                    originalInA.Line,
                    originalInA.Column);
            }
            // 找不到映射时跳过（生成文件中有些行没有对应的源码）
        }
        gen.NextLine();
    }

    return gen.ToSourceMap();
}
```

### 5.5 DevHttpServer — 服务 Source Map

```csharp
// DevServer/DevHttpServer.cs — 在中间件管道中添加

// 在模块服务中间件中添加:
// GET /App.jazor.map → 返回 Source Map JSON
if (requestPath.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
{
    var sourcePath = requestPath[..^4]; // 去掉 .map
    var resolveResult = _moduleResolver.Resolve(sourcePath);
    if (resolveResult.Found)
    {
        var cacheEntry = _cache.Get(resolveResult.AbsolutePath);
        if (cacheEntry?.SourceMap is not null)
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(cacheEntry.SourceMap, ct);
            return;
        }
    }
    context.Response.StatusCode = 404;
    return;
}
```

### 5.6 Source Map 内联模式

开发模式默认使用 **inline Source Map**（避免额外的 HTTP 请求）：

```csharp
// OnDemandCompiler — 生成 inline Source Map
if (sourceMapJson is not null)
{
    var dataUri = "data:application/json;base64,"
        + Convert.ToBase64String(Encoding.UTF8.GetBytes(sourceMapJson));
    jsContent += $"\n//# sourceMappingURL={dataUri}\n";
}
```

或者通过配置选择 inline vs external：

```csharp
// jazor.config.json
{
    "build": {
        "sourcemap": "inline"   // "inline" | "external" | false
    }
}
```

---

## 六、实施步骤（严格顺序）

### Step 1: VLQ 编解码器 + Source Map 数据模型

**产出文件**:
- 新增 `SourceMap/VlqCodec.cs`
- 新增 `SourceMap/SourceMap.cs`
- 新增 `SourceMap/SourceMapping.cs`
- 新增 `SourceMap/SourceMapLine.cs`

**不依赖任何外部组件**，纯算法实现。

**测试**:
- VLQ 编码: `(0, 0, 3, 0)` → `"AADA"`
- VLQ 编码: 负数 → 正确编码
- VLQ 解码: `"AADA"` → `(0, 0, 3, 0)`
- 往返测试: encode(decode(x)) == x
- SourceMap.ToJson() 输出符合 v3 格式
- SourceMapConsumer 解析 JSON → 正确的逆向映射

**退出标准**: VLQ 编解码通过往返测试。Source Map JSON 生成和解析正确。

### Step 2: SourceMapGenerator

**产出文件**:
- 新增 `SourceMap/SourceMapGenerator.cs`

**测试**:
- 空生成器 → 空映射
- 逐行添加映射 → 正确的 delta VLQ 编码
- 多源文件映射 → Sources 数组正确
- sourcesContent 正确填充
- 跨行重置 genCol delta

**退出标准**: 生成的 Source Map JSON 通过 [source-map-validator](https://www.npmjs.com/package/source-map-validator) 验证。

### Step 3: SourceMapConsumer + SourceMapMerger

**产出文件**:
- 新增 `SourceMap/SourceMapConsumer.cs`
- 新增 `SourceMap/SourceMapMerger.cs`

**测试**:
- Consumer 逆向映射: 单行、多行、多源文件
- Consumer 正向映射
- Merger: 两个简单 Source Map 合并
- Merger: 有间隔行的 Source Map 合并
- Merger: smap2 某行在 smap1 中无映射 → 跳过
- Merger 往返: merge(smap1, smap2) → consumer 反查 → 回到 smap1 的源位置

**退出标准**: 合并后的 Source Map 反查能正确映射回原始源文件行号。

### Step 4: JazorVueCompiler Source Map 跟踪

**产出文件**:
- 修改 `Jazor/Core/JazorVueCompiler.cs`
- 修改 `Jazor/Core/JazorVueContracts.cs` (JazorVueCompilationResult 添加 SourceMap 属性)

**测试**:
- 简单 .jazor (只有 template) → smap 只有 template 行映射
- 复杂 .jazor (template + @code props/states/methods) → 各区域映射正确
- 映射的行号与源码实际行号一致（用测试用例验证）

**退出标准**: JazorVueCompiler.Compile() 返回的 SourceMap 能将 .g.vue 的每一行映射回 .jazor 的对应行。

### Step 5: Deno Worker Source Map 透传

**产出文件**:
- 修改 `Frontend/Deno/Worker/frontend-worker.ts`
- 修改 `Frontend/Deno/Protocol/DenoCompilationProtocol.cs`

**测试**:
- 手动测试: 通过 IPC 发送 compileSfc 请求 → 返回包含 jsSourceMap
- 验证: Deno 返回的 Source Map 是有效的 JSON

**退出标准**: Deno Worker 的 compileSfc 返回的 Source Map 可以被 SourceMapConsumer 解析。

### Step 6: OnDemandCompiler 集成

**产出文件**:
- 修改 `DevServer/OnDemandCompiler.cs`
- 修改 `DevServer/CompilationCache.cs`
- 修改 `DevServer/DevHttpServer.cs` — 服务 .map 文件
- 新增 `SourceMap/ISourceMapService.cs`
- 修改 `CompilationResult` — 添加 SourceMap 属性

**测试**:
- 集成测试: .jazor 编译 → 返回带 sourceMappingURL 的 JS
- 集成测试: .map 请求 → 返回有效 Source Map JSON
- 端到端测试: 浏览器 DevTools Sources 面板显示 .jazor 文件

**退出标准**: 浏览器 DevTools 中看到 .jazor 源码，断点可命中。

---

## 七、测试策略

### 7.1 单元测试清单

| 测试文件 | 覆盖内容 |
|---------|---------|
| `VlqCodecTests.cs` | VLQ 正数/负数/零/多值编码解码，往返一致性 |
| `SourceMapGeneratorTests.cs` | 逐行映射，多源文件，delta 计算正确性，JSON 输出格式 |
| `SourceMapConsumerTests.cs` | 逆向/正向映射，边界行，空行，多源文件 |
| `SourceMapMergerTests.cs` | 两级链式合并，间隔行，缺失映射 |
| `JazorVueCompilerSourceMapTests.cs` | 各种 .jazor 结构的行级映射正确性 |

### 7.2 集成测试清单

| 测试 | 说明 |
|------|------|
| `.jazor → JS + Source Map` | OnDemandCompiler 返回带 Source Map 的结果 |
| Source Map 链式合并 | .jazor → .g.vue → .js 三级映射合并正确 |
| Dev Server `.map` 文件服务 | HTTP 请求 .map 文件返回有效 JSON |
| 浏览器 DevTools | Sources 面板显示 .jazor 源码 |

### 7.3 验证工具

```bash
# 使用 Node.js source-map 库验证生成的 Source Map
node -e "
const { SourceMapConsumer } = require('source-map');
const fs = require('fs');
const smap = JSON.parse(fs.readFileSync('App.jazor.map'));
SourceMapConsumer.with(smap, null, consumer => {
    console.log(consumer.originalPositionFor({line: 5, column: 0}));
});
"
```

---

## 八、风险与降级

| 风险 | 影响 | 降级方案 |
|------|------|---------|
| JazorVueCompiler 的行号跟踪不够精确 | 断点偏移 1-2 行 | Phase 2 接受行级精度，后续迭代改进 |
| Deno Worker compileSfc 不返回 Source Map | 缺少 smap2 | 直接使用 smap1 (.jazor → .g.vue)，跳过合并 |
| VLQ 编码错误 | Source Map 无效 | 详尽的单元测试覆盖 |
| 链式合并在复杂场景下丢失映射 | 部分行无法映射 | 找不到映射时跳过，不崩溃 |
| sourcesContent 过大 | 内存占用 | 开发模式保留，生产模式可选去除 |

---

## 九、关键依赖关系

```
Step 1 (VLQ + 数据模型)
    ↓
Step 2 (SourceMapGenerator) ← 依赖 Step 1
    ↓
Step 3 (Consumer + Merger) ← 依赖 Step 1, 2
    ↓
Step 4 (JazorVueCompiler) ← 依赖 Step 2 (仅用 Generator)
    ↓
Step 5 (Deno Worker 透传) ← 独立，可与 Step 4 并行
    ↓
Step 6 (OnDemandCompiler 集成) ← 依赖 Step 3, 4, 5
```

Step 4 和 Step 5 可以并行开发。

---

## 十、与 Phase 1 的接口变更

### 10.1 CompilationResult 扩展

Phase 1 定义的 `CompilationResult` 添加：

```csharp
public sealed class CompilationResult
{
    // Phase 1 已有:
    public required string ContentType { get; init; }
    public required string Content { get; init; }
    public IReadOnlyList<string> Dependencies { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public bool IsError { get; init; }
    public string? ErrorMessage { get; init; }

    // Phase 2 新增:
    public string? SourceMap { get; init; }  // Source Map JSON (可序列化为 inline 或 external)
}
```

### 10.2 CompilationCache 扩展

缓存条目现在包含 Source Map：

```csharp
// 缓存键不变: (absolutePath, contentHash)
// 缓存值: CompilationResult (现在包含 SourceMap)
```

无接口变更，只是缓存值内容更丰富。

---

## 十一、不做的事情 (Phase 2 明确排除)

| 排除项 | 原因 |
|--------|------|
| 字符级精度 Source Map | 复杂度太高，行级已满足基本调试 |
| Source Map 服务独立进程 | DAP 需要到 Phase 4 |
| 生产模式 Source Map 压缩 | Phase 5 |
| 第三方库 Source Map (如 Sass) | 远期 |
| Source Map 持久化到磁盘 | Phase 2 只做内存 + inline |
