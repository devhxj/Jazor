using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Jazor.Npm.Editor;

internal static class NpmHostProcess
{
    internal static async Task<string?> RunAsync(string hostPath, string? netCoreRoot, string project, string operation, string request,
        CancellationToken cancellationToken)
    {
        var managedHost = Path.GetExtension(hostPath).Equals(".dll", StringComparison.OrdinalIgnoreCase);
        if (!File.Exists(hostPath) || managedHost && Path.DirectorySeparatorChar == '\\')
            return null;

        cancellationToken.ThrowIfCancellationRequested();
        var work = Path.Combine(Path.GetDirectoryName(project)!, ".jazor");
        var output = Path.Combine(work, "editor-" + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            Directory.CreateDirectory(work);
            var start = new ProcessStartInfo
            {
                FileName = managedHost ? Path.Combine(netCoreRoot ?? "", "dotnet") : hostPath,
                Arguments = (managedHost ? Quote(hostPath) + " " : "") + operation + " " + Quote(output),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            // VS ServiceHub can run from a private runtime older than the project SDK.
            // Keep the CET-compatible Windows apphost and resolve its framework from that SDK.
            if (!managedHost && !string.IsNullOrEmpty(netCoreRoot))
            {
                start.EnvironmentVariables.Remove("DOTNET_ROOT_X64");
                start.EnvironmentVariables.Remove("DOTNET_ROOT_ARM64");
                start.EnvironmentVariables["DOTNET_ROOT"] = netCoreRoot;
            }
            using var process = new Process { StartInfo = start, EnableRaisingEvents = true };
            var exited = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            process.Exited += (_, _) => exited.TrySetResult(true);
            process.Start();
            using var cancellation = cancellationToken.Register(() =>
            {
                try { process.Kill(); }
                catch (InvalidOperationException) { }
                catch (Win32Exception) { }
            });
            // Native Node tooling may write to either pipe. Read both while keeping
            // the response file reserved for the managed host protocol.
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            try
            {
                using var input = process.StandardInput.BaseStream;
                var payload = Encoding.UTF8.GetBytes(request);
                await input.WriteAsync(payload, 0, payload.Length, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await Task.WhenAll(stdout, stderr, exited.Task).ConfigureAwait(false);
            }
            cancellationToken.ThrowIfCancellationRequested();
            return process.ExitCode == 0 ? File.ReadAllText(output) : null;
        }
        catch (Exception ex) when (ex is IOException or Win32Exception or UnauthorizedAccessException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return null;
        }
        finally
        {
            if (File.Exists(output))
                File.Delete(output);
        }
    }

    // ProcessStartInfo.ArgumentList is unavailable on netstandard2.0.
    private static string Quote(string value) => "\"" +
        Regex.Replace(value, "(\\\\*)(\"|$)", match =>
            match.Groups[1].Value + (match.Groups[2].Length == 0 ? "" : "\\") + match.Value) + "\"";
}
