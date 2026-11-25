using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Jero.Plugin.SaveLife.Service;

public interface ISaveLifeDocumentParserWorkerClient
{
    Task<SaveLifeMaterialTextExtractionResult> ExtractAsync(
        string suffix,
        Stream input,
        CancellationToken cancellationToken = default);
}

public sealed class SaveLifeDocumentParserWorkerClient :
    ISaveLifeDocumentParserWorkerClient,
    ISingleton
{
    internal const string WorkerAssemblyName = "Jero.Plugin.SaveLife.DocumentWorker.dll";
    internal const long MaximumWorkerWorkingSetBytes = 512L * 1024 * 1024;
    private const int MaximumWorkerResultBytes = 64 * 1024;
    private static readonly SemaphoreSlim ProcessGate = new(2, 2);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    private readonly Func<WorkerLaunchRequest, ProcessStartInfo> _startInfoFactory;

    public SaveLifeDocumentParserWorkerClient()
        : this(CreateDefaultStartInfo)
    {
    }

    internal SaveLifeDocumentParserWorkerClient(
        Func<WorkerLaunchRequest, ProcessStartInfo> startInfoFactory)
    {
        _startInfoFactory = startInfoFactory;
    }

    public async Task<SaveLifeMaterialTextExtractionResult> ExtractAsync(
        string suffix,
        Stream input,
        CancellationToken cancellationToken = default)
    {
        if (suffix is not ".pdf" and not ".doc")
            throw new ArgumentOutOfRangeException(nameof(suffix));
        ArgumentNullException.ThrowIfNull(input);

        await ProcessGate.WaitAsync(cancellationToken);
        string? workDirectory = null;
        try
        {
            workDirectory = Path.Combine(
                Path.GetTempPath(),
                $"savelife-document-worker-{Guid.NewGuid():N}");
            Directory.CreateDirectory(workDirectory);
            TryRestrictDirectoryPermissions(workDirectory);
            var inputPath = Path.Combine(workDirectory, "input.bin");
            var outputPath = Path.Combine(workDirectory, "result.json");
            await WriteBoundedInputAsync(input, inputPath, cancellationToken);
            var startInfo = _startInfoFactory(new WorkerLaunchRequest(
                suffix,
                inputPath,
                outputPath));
            using var process = Process.Start(startInfo) ??
                throw new InvalidOperationException("SaveLife document worker could not start.");
            var resourceExceeded = await MonitorProcessAsync(process, cancellationToken);

            if (resourceExceeded)
                return Failed("病例文档解析资源超限，无法安全处理", suffix);
            if (process.ExitCode != 0)
                return Failed("病例文档解析子进程异常退出，请稍后重试", suffix);

            return await ReadAndValidateResultAsync(
                outputPath,
                suffix,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return Failed("病例文档解析服务暂时不可用，请稍后重试", suffix);
        }
        finally
        {
            if (workDirectory != null)
                DeleteWorkDirectory(workDirectory);
            ProcessGate.Release();
        }
    }

    internal static string ResolveWorkerAssemblyPath()
    {
        var deployedPath = Path.Combine(AppContext.BaseDirectory, WorkerAssemblyName);
        if (File.Exists(deployedPath))
            return deployedPath;

        var baseDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        var targetFramework = baseDirectory.Name;
        var configuration = baseDirectory.Parent?.Name;
        var current = baseDirectory;
        while (current != null &&
               !File.Exists(Path.Combine(current.FullName, "Jero.slnx")))
        {
            current = current.Parent;
        }

        if (current != null && !string.IsNullOrWhiteSpace(configuration))
        {
            var developmentPath = Path.Combine(
                current.FullName,
                "Plugins",
                "Jero.Plugin.SaveLife.DocumentWorker",
                "bin",
                configuration,
                targetFramework,
                WorkerAssemblyName);
            if (File.Exists(developmentPath))
                return developmentPath;
        }

        throw new FileNotFoundException("SaveLife document worker is not deployed.");
    }

    private static ProcessStartInfo CreateDefaultStartInfo(WorkerLaunchRequest request)
    {
        var workerAssemblyPath = ResolveWorkerAssemblyPath();
        var info = new ProcessStartInfo
        {
            FileName = ResolveDotnetHost(),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(workerAssemblyPath)!
        };
        info.ArgumentList.Add(workerAssemblyPath);
        info.ArgumentList.Add("--format");
        info.ArgumentList.Add(request.Suffix);
        info.ArgumentList.Add("--input");
        info.ArgumentList.Add(request.InputPath);
        info.ArgumentList.Add("--output");
        info.ArgumentList.Add(request.OutputPath);
        info.Environment["DOTNET_GCHeapHardLimit"] = "0x18000000";
        info.Environment["DOTNET_EnableDiagnostics"] = "0";
        return info;
    }

    private static string ResolveDotnetHost()
    {
        var processPath = Environment.ProcessPath;
        if (!string.IsNullOrWhiteSpace(processPath) &&
            Path.GetFileNameWithoutExtension(processPath)
                .Equals("dotnet", StringComparison.OrdinalIgnoreCase))
        {
            return processPath;
        }

        return "dotnet";
    }

    private static async Task WriteBoundedInputAsync(
        Stream input,
        string inputPath,
        CancellationToken cancellationToken)
    {
        await using var output = new FileStream(
            inputPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 64 * 1024,
            FileOptions.Asynchronous | FileOptions.WriteThrough);
        TryRestrictFilePermissions(inputPath);
        var buffer = new byte[64 * 1024];
        long total = 0;
        while (true)
        {
            var remaining = SaveLifeMaterialTextExtractionLimits.MaximumInputBytes + 1 - total;
            var read = await input.ReadAsync(
                buffer.AsMemory(0, (int)Math.Min(buffer.Length, remaining)),
                cancellationToken);
            if (read == 0)
                break;
            total += read;
            if (total > SaveLifeMaterialTextExtractionLimits.MaximumInputBytes)
                throw new InvalidDataException("Bounded document input exceeded its validated size.");
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }
        await output.FlushAsync(cancellationToken);
    }

    private static async Task<SaveLifeMaterialTextExtractionResult> ReadAndValidateResultAsync(
        string outputPath,
        string suffix,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = new FileStream(
                outputPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 16 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            if (stream.Length <= 0 || stream.Length > MaximumWorkerResultBytes)
                return InvalidResult(suffix);
            var result = await JsonSerializer.DeserializeAsync<SaveLifeMaterialTextExtractionResult>(
                stream,
                JsonOptions,
                cancellationToken);
            if (result == null ||
                result.Status is not "synced" and not "skipped" and not "failed" ||
                result.ExtractedText?.Length >
                SaveLifeMaterialTextExtractionLimits.MaximumExtractedTextCharacters ||
                result.Error?.Length > 512 ||
                !string.Equals(result.ExtractModel, ResolveModel(suffix), StringComparison.Ordinal))
            {
                return InvalidResult(suffix);
            }
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return InvalidResult(suffix);
        }
    }

    private static SaveLifeMaterialTextExtractionResult InvalidResult(string suffix) =>
        Failed("病例文档解析子进程返回无效结果，请稍后重试", suffix);

    private static SaveLifeMaterialTextExtractionResult Failed(
        string message,
        string suffix) => new()
        {
            Status = "failed",
            Error = message,
            ExtractModel = ResolveModel(suffix)
        };

    internal static string ResolveModel(string suffix) => suffix == ".pdf"
        ? "pdf-pdfpig"
        : "doc-b2xtranslator";

    private static async Task<bool> MonitorProcessAsync(
        Process process,
        CancellationToken cancellationToken)
    {
        var stdoutDrain = process.StandardOutput.BaseStream.CopyToAsync(Stream.Null);
        var stderrDrain = process.StandardError.BaseStream.CopyToAsync(Stream.Null);
        try
        {
            var resourceExceeded = false;
            while (!process.HasExited)
            {
                await Task.Delay(10, cancellationToken);
                if (process.HasExited)
                    break;

                long workingSet;
                try
                {
                    process.Refresh();
                    if (process.HasExited)
                        break;
                    workingSet = process.WorkingSet64;
                }
                catch (InvalidOperationException) when (process.HasExited)
                {
                    break;
                }

                if (workingSet <= MaximumWorkerWorkingSetBytes)
                    continue;

                resourceExceeded = true;
                TryKill(process);
                break;
            }

            await process.WaitForExitAsync(cancellationToken);
            await Task.WhenAll(stdoutDrain, stderrDrain).WaitAsync(cancellationToken);
            return resourceExceeded;
        }
        catch
        {
            TryKill(process);
            await WaitForExitAndObserveOutputAsync(process, stdoutDrain, stderrDrain);
            throw;
        }
    }

    private static async Task WaitForExitAndObserveOutputAsync(
        Process process,
        Task stdoutDrain,
        Task stderrDrain)
    {
        try
        {
            await process.WaitForExitAsync(CancellationToken.None)
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch
        {
        }

        try
        {
            await Task.WhenAll(stdoutDrain, stderrDrain)
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch
        {
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch
        {
        }
    }

    private static void DeleteWorkDirectory(string path)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
                return;
            }
            catch
            {
                if (attempt < 2)
                    Thread.Sleep(10);
            }
        }
    }

    private static void TryRestrictDirectoryPermissions(string path)
    {
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
    }

    private static void TryRestrictFilePermissions(string path)
    {
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
    }

    internal sealed record WorkerLaunchRequest(
        string Suffix,
        string InputPath,
        string OutputPath);
}

internal sealed class SaveLifeInProcessDocumentParserWorkerClient :
    ISaveLifeDocumentParserWorkerClient
{
    public async Task<SaveLifeMaterialTextExtractionResult> ExtractAsync(
        string suffix,
        Stream input,
        CancellationToken cancellationToken = default) => suffix switch
        {
            ".pdf" => await SaveLifeMaterialTextExtractionService.ExtractPdfResultAsync(
                input,
                cancellationToken),
            ".doc" => await SaveLifeMaterialTextExtractionService.ExtractLegacyDocResultAsync(
                input,
                cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(suffix))
        };
}

public static class SaveLifeDocumentWorkerEntrypoint
{
    private const int MaximumResultBytes = 64 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<int> RunAsync(string[] args)
    {
        try
        {
            if (!TryParseArguments(args, out var suffix, out var inputPath, out var outputPath))
                return 2;
            var inputInfo = new FileInfo(inputPath);
            if (!inputInfo.Exists ||
                inputInfo.Length <= 0 ||
                inputInfo.Length > SaveLifeMaterialTextExtractionLimits.MaximumInputBytes ||
                File.Exists(outputPath) ||
                !string.Equals(
                    inputInfo.DirectoryName,
                    Path.GetDirectoryName(outputPath),
                    StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            SaveLifeMaterialTextExtractionResult result;
            if (suffix == ".doc")
            {
                result = SaveLifeMaterialTextExtractionService.ExtractLegacyDocFileResult(
                    inputPath,
                    CancellationToken.None);
            }
            else
            {
                await using var input = new FileStream(
                    inputPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 64 * 1024,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);
                result = await SaveLifeMaterialTextExtractionService.ExtractPdfResultAsync(
                    input,
                    CancellationToken.None);
            }
            result.ExtractModel = SaveLifeDocumentParserWorkerClient.ResolveModel(suffix);
            if (result.ExtractedText?.Length >
                SaveLifeMaterialTextExtractionLimits.MaximumExtractedTextCharacters)
            {
                return 3;
            }

            var bytes = JsonSerializer.SerializeToUtf8Bytes(result, JsonOptions);
            if (bytes.Length <= 0 || bytes.Length > MaximumResultBytes)
                return 3;
            await File.WriteAllBytesAsync(outputPath, bytes);
            if (!OperatingSystem.IsWindows())
                File.SetUnixFileMode(outputPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            return 0;
        }
        catch
        {
            return 3;
        }
    }

    private static bool TryParseArguments(
        IReadOnlyList<string> args,
        out string suffix,
        out string inputPath,
        out string outputPath)
    {
        suffix = string.Empty;
        inputPath = string.Empty;
        outputPath = string.Empty;
        if (args.Count != 6 ||
            args[0] != "--format" ||
            args[2] != "--input" ||
            args[4] != "--output")
        {
            return false;
        }

        suffix = args[1];
        inputPath = Path.GetFullPath(args[3]);
        outputPath = Path.GetFullPath(args[5]);
        return suffix is ".pdf" or ".doc" &&
            Path.IsPathFullyQualified(inputPath) &&
            Path.IsPathFullyQualified(outputPath);
    }
}
