namespace Jazor.Emit;

internal enum BuildMode
{
    Production,
    Development
}

/// <summary>Stable result returned by the standard JavaScript project build.</summary>
internal sealed record ToolchainDiagnostic(string Code, string Message);

internal sealed record ToolchainResult(
    bool IsSuccess,
    int ExitCode,
    ToolchainDiagnostic? Diagnostic,
    int ModuleCount)
{
    public static ToolchainResult Success(int moduleCount)
        => new(true, 0, null, moduleCount);

    public static ToolchainResult Fail(int exitCode, string code, string message)
        => new(false, exitCode, new ToolchainDiagnostic(code, message), 0);
}
