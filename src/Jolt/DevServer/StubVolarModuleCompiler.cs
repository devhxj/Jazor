using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Jolt.Hosting;

namespace Jolt.DevServer;

internal sealed class StubVolarModuleCompiler : IVolarModuleCompiler
{
    private static readonly Regex StyleBlockPattern = new(
        @"(?<open><style\b[^>]*>)(?<content>[\s\S]*?)(?<close></style>)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public ValueTask<VolarModuleCompilation?> CompileSfcAsync(
        string documentPath,
        string text,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ReportStubCompilationActivated();

        return ValueTask.FromResult<VolarModuleCompilation?>(
            new VolarModuleCompilation
            {
                JavaScript = CreateStubSfcModule(documentPath, text),
                StyleContent = ExtractStyleContent(text),
                Dependencies = [],
                SupportsHmr = true
            });
    }

    public ValueTask<VolarModuleCompilation?> CompileTypeScriptAsync(
        string documentPath,
        string text,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ReportStubCompilationActivated();

        return ValueTask.FromResult<VolarModuleCompilation?>(
            new VolarModuleCompilation
            {
                JavaScript = text,
                Dependencies = DenoVolarModuleCompiler.ExtractJavaScriptDependencies(text),
                SupportsHmr = true
            });
    }

    private static string CreateStubSfcModule(string documentPath, string text)
    {
        var styleInsensitiveText = StripStyleContent(text);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(styleInsensitiveText)));
        var componentName = EscapeJavaScriptString(Path.GetFileNameWithoutExtension(documentPath));

        return
            $$"""
            const __jazorStubHash = "{{hash}}";
            export default { name: "{{componentName}}", __jazorStubHash };
            """;
    }

    private static string? ExtractStyleContent(string text)
    {
        var match = StyleBlockPattern.Match(text);
        return match.Success
            ? match.Groups["content"].Value.Trim()
            : null;
    }

    private static string StripStyleContent(string text)
        => StyleBlockPattern.Replace(
            text,
            static match => string.Concat(
                match.Groups["open"].Value,
                match.Groups["close"].Value));

    private static string EscapeJavaScriptString(string value)
        => value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);

    private static void ReportStubCompilationActivated()
        => FallbackTelemetry.ReportActivation(
            component: "frontendCompiler",
            mode: "stub",
            reason: "deno-frontend-unavailable");
}
