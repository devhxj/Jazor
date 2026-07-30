using System.Collections.Generic;

namespace Jazor.Common.SourceMaps;

/// <summary>
/// Source Map 的通用数据模型，供读写和链式合并使用。
/// </summary>
/// <remarks>
/// 坐标均为零基值；Segments 已表达生成文件到源文件的映射关系。
/// 该模型不携带编译器 AST 引用，便于 Emit 和其他工具独立消费。
/// </remarks>
public sealed record SourceMapDocument(
    string File,
    IReadOnlyList<SourceMapSource> Sources,
    IReadOnlyList<SourceMapSegment> Segments);

/// <summary>Source Map 中的源文件路径及可选内嵌内容。</summary>
public sealed record SourceMapSource(
    string Path,
    string? Content);

/// <summary>一个生成位置到源文件位置的 Source Map 映射段。</summary>
public sealed record SourceMapSegment(
    int GeneratedLine,
    int GeneratedColumn,
    int SourceIndex,
    int SourceLine,
    int SourceColumn);
