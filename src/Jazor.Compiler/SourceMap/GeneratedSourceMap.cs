using System.Collections.Generic;

namespace Jazor.Compiler;

/// <summary>
/// Source Map v3 发射阶段使用的内部数据模型。
/// </summary>
/// <remarks>
/// Sources 和 Segments 已由 SourceMapEmitter 按确定性顺序组织；这些 record 不负责坐标转换、
/// VLQ 编码或 JSON 序列化，职责分别由 emitter 和 writer 承担。
/// </remarks>
internal sealed record GeneratedSourceMap(
    string File,
    IReadOnlyList<GeneratedSourceMapSource> Sources,
    IReadOnlyList<GeneratedSourceMapSegment> Segments);

/// <summary>
/// Source Map 中的一个源文件及可选源码内容。
/// </summary>
internal sealed record GeneratedSourceMapSource(
    string Path,
    string? Content);

/// <summary>
/// 一个生成列到源文件行列的映射段。
/// </summary>
internal sealed record GeneratedSourceMapSegment(
    int GeneratedLine,
    int GeneratedColumn,
    int SourceIndex,
    int SourceLine,
    int SourceColumn);
