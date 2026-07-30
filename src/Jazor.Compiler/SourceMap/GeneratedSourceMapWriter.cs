using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jazor.Compiler;

/// <summary>
/// 以确定性顺序写出 Source Map v3 JSON 文本。
/// </summary>
/// <remarks>
/// 这里使用专用 writer 是为了控制字段顺序、字符串转义和 VLQ 编码，避免通用 JSON 序列化器
/// 引入不可预测格式差异。输出格式稳定性对缓存和增量构建同样重要。
/// </remarks>
internal sealed class GeneratedSourceMapWriter
{
    private const string Base64Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

    public string Write(GeneratedSourceMap sourceMap)
    {
        if (sourceMap is null)
            throw new ArgumentNullException(nameof(sourceMap));

        var builder = new StringBuilder();
        builder.Append('{');

        AppendPropertyName(builder, "version");
        builder.Append('3');

        builder.Append(',');
        AppendPropertyName(builder, "file");
        AppendString(builder, sourceMap.File);

        builder.Append(',');
        AppendPropertyName(builder, "sources");
        AppendSources(builder, sourceMap.Sources);

        if (sourceMap.Sources.Any(static source => source.Content is not null))
        {
            builder.Append(',');
            AppendPropertyName(builder, "sourcesContent");
            AppendSourcesContent(builder, sourceMap.Sources);
        }

        builder.Append(',');
        AppendPropertyName(builder, "names");
        builder.Append("[]");

        builder.Append(',');
        AppendPropertyName(builder, "mappings");
        AppendString(builder, BuildMappings(sourceMap.Segments));

        builder.Append('}');
        return builder.ToString();
    }

    private static void AppendPropertyName(StringBuilder builder, string name)
    {
        AppendString(builder, name);
        builder.Append(':');
    }

    private static void AppendSources(StringBuilder builder, IReadOnlyList<GeneratedSourceMapSource> sources)
    {
        builder.Append('[');
        for (var index = 0; index < sources.Count; index++)
        {
            if (index > 0)
                builder.Append(',');

            AppendString(builder, sources[index].Path);
        }

        builder.Append(']');
    }

    private static void AppendSourcesContent(StringBuilder builder, IReadOnlyList<GeneratedSourceMapSource> sources)
    {
        builder.Append('[');
        for (var index = 0; index < sources.Count; index++)
        {
            if (index > 0)
                builder.Append(',');

            if (sources[index].Content is null)
                builder.Append("null");
            else
                AppendString(builder, sources[index].Content!);
        }

        builder.Append(']');
    }

    private static void AppendString(StringBuilder builder, string? value)
    {
        if (value is null)
        {
            builder.Append("null");
            return;
        }

        builder.Append('"');
        foreach (var ch in value)
        {
            switch (ch)
            {
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '\b':
                    builder.Append("\\b");
                    break;
                case '\f':
                    builder.Append("\\f");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                default:
                    if (char.IsControl(ch))
                    {
                        builder.Append("\\u");
                        builder.Append(((int) ch).ToString("x4"));
                    }
                    else
                    {
                        builder.Append(ch);
                    }

                    break;
            }
        }

        builder.Append('"');
    }

    private static string BuildMappings(IReadOnlyList<GeneratedSourceMapSegment> segments)
    {
        if (segments.Count == 0)
            return string.Empty;

        var builder = new StringBuilder();
        var generatedLine = 0;
        var previousGeneratedColumn = 0;
        var previousSourceIndex = 0;
        var previousSourceLine = 0;
        var previousSourceColumn = 0;

        foreach (var group in segments
            .GroupBy(static segment => segment.GeneratedLine)
            .OrderBy(static group => group.Key))
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
        var vlq = value < 0 ? ((-value) << 1) + 1 : value << 1;
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
