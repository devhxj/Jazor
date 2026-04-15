using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Jazor.VueHost.DevServer;

internal sealed class StubFrontendModuleCompiler : IFrontendModuleCompiler
{
    private static readonly Regex StyleBlockPattern = new(
        @"<style\b[^>]*>(?<content>[\s\S]*?)</style>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public ValueTask<FrontendModuleCompilation?> CompileSfcAsync(
        string documentPath,
        string text,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return ValueTask.FromResult<FrontendModuleCompilation?>(
            new FrontendModuleCompilation
            {
                JavaScript = CreateStubSfcModule(documentPath, text),
                StyleContent = ExtractStyleContent(text),
                Dependencies = Array.Empty<string>(),
                SupportsHmr = true
            });
    }

    public ValueTask<FrontendModuleCompilation?> CompileTypeScriptAsync(
        string documentPath,
        string text,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return ValueTask.FromResult<FrontendModuleCompilation?>(
            new FrontendModuleCompilation
            {
                JavaScript = text,
                Dependencies = DenoFrontendModuleCompiler.ExtractJavaScriptDependencies(text),
                SupportsHmr = true
            });
    }

    private static string CreateStubSfcModule(string documentPath, string text)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
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

    private static string EscapeJavaScriptString(string value)
        => value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
}
