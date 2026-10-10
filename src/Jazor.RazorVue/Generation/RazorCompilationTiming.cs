using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.RazorVue.Generation;

/// <summary>
/// Captures one final-compilation hook invocation without affecting its generated artifacts.
/// 所有阶段都是相邻的墙钟区间；并行 artifact 记录等待全部 worker 的时间，不累加 worker CPU 时间。
/// </summary>
internal sealed class RazorCompilationTiming
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly List<KeyValuePair<string, double>> _stages = new();
    private double _previousMilliseconds;

    internal int ComponentCount { get; set; }

    internal IReadOnlyList<KeyValuePair<string, double>> Stages => _stages;

    internal void CompleteStage(string name)
    {
        var elapsed = _stopwatch.Elapsed.TotalMilliseconds;
        _stages.Add(new KeyValuePair<string, double>(name, elapsed - _previousMilliseconds));
        _previousMilliseconds = elapsed;
    }

    internal string Format(string assemblyName, bool succeeded)
    {
        var parts = new List<string>
        {
            "[Jazor.RazorVue] compilation timing",
            "assembly=" + assemblyName,
            "components=" + ComponentCount.ToString(CultureInfo.InvariantCulture),
            "status=" + (succeeded ? "completed" : "failed"),
            "total=" + FormatMilliseconds(_stopwatch.Elapsed.TotalMilliseconds)
        };
        foreach (var stage in _stages)
            parts.Add(stage.Key + "=" + FormatMilliseconds(stage.Value));
        return string.Join("; ", parts);
    }

    internal void Write(string? path, string assemblyName, bool succeeded)
    {
        if (ComponentCount == 0 || string.IsNullOrEmpty(path))
            return;

        // This file belongs to the compilation's obj directory and is consumed by MSBuild,
        // never by Emit. Timing must neither change module bytes nor fail valid compilation.
        // 性能 sidecar 是可选诊断；只忽略其文件写入失败，不能捕获或吞掉 lowering 诊断。
        try
        {
#pragma warning disable RS1035 // The host hook writes only its explicitly configured timing sidecar.
            File.WriteAllText(path, Format(assemblyName, succeeded) + "\n", Encoding.UTF8);
#pragma warning restore RS1035
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    internal static string? GetOutputPath(ImmutableArray<SyntaxTree> generatedTrees, CancellationToken cancellationToken)
    {
        foreach (var tree in generatedTrees)
        {
            if (!RazorSourceTextRegistry.IsCarrierTree(tree))
                continue;

            var literal = tree.GetRoot(cancellationToken).DescendantNodes().OfType<VariableDeclaratorSyntax>()
                .FirstOrDefault(static variable => variable.Identifier.ValueText == "CompilationTimingPath")
                ?.Initializer?.Value as LiteralExpressionSyntax;
            if (literal is not null)
                return Encoding.UTF8.GetString(Convert.FromBase64String(literal.Token.ValueText));
        }
        return null;
    }

    private static string FormatMilliseconds(double value)
        => value.ToString("0.0", CultureInfo.InvariantCulture) + " ms";
}
