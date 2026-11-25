using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class SaveLifeAgentMaterialMemoryBudgetCollection
{
    public const string Name = "SaveLifeAgentMaterialMemoryBudget";
}

[Collection(SaveLifeAgentMaterialMemoryBudgetCollection.Name)]
public class SaveLifeAgentMaterialMemoryBudgetTests
{
    private const long Mebibyte = 1024 * 1024;
    private const long PerRequestAllocatedCeilingBytes = 256 * Mebibyte;
    private const long ConcurrentManagedCeilingBytes = 512 * Mebibyte;
    private const long ConcurrentProcessCeilingBytes = 512 * Mebibyte;

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task FullMaterialRequest_WithTwentyMegabyteMedia_StaysWithinWrittenMemoryCeiling(
        int concurrency)
    {
        var metrics = await MeasureAsync(concurrency, TestContext.Current.CancellationToken);

        TestContext.Current.TestOutputHelper?.WriteLine(
            $"T59 memory probe: concurrency={concurrency}, " +
            $"managed_peak_delta_mib={ToMebibytes(metrics.ManagedPeakDeltaBytes):F1}, " +
            $"working_set_peak_delta_mib={ToMebibytes(metrics.WorkingSetPeakDeltaBytes):F1}, " +
            $"private_peak_delta_mib={ToMebibytes(metrics.PrivatePeakDeltaBytes):F1}, " +
            $"allocated_per_request_mib={ToMebibytes(metrics.AllocatedPerRequestBytes):F1}, " +
            $"serialized_request_mib={ToMebibytes(metrics.MaximumSerializedRequestBytes):F1}.");

        Assert.InRange(metrics.AllocatedPerRequestBytes, 1, PerRequestAllocatedCeilingBytes);
        Assert.InRange(metrics.ManagedPeakDeltaBytes, 0, ConcurrentManagedCeilingBytes);
        Assert.InRange(metrics.WorkingSetPeakDeltaBytes, 0, ConcurrentProcessCeilingBytes);
        Assert.InRange(metrics.PrivatePeakDeltaBytes, 0, ConcurrentProcessCeilingBytes);
        Assert.InRange(
            metrics.MaximumSerializedRequestBytes,
            SaveLifeMaterialResourceBudget.MaterialReviewEncodedMediaBytes,
            SaveLifeMaterialResourceBudget.MaterialReviewSerializedRequestBytes);
    }

    private static async Task<MemoryMetrics> MeasureAsync(
        int concurrency,
        CancellationToken cancellationToken)
    {
        var databases = new List<SqlSugarClient>();
        var databasePaths = new List<string>();
        var preparedInputs = new List<SaveLifeAgentPreparedMaterialInput>();
        var gate = new SaveLifeAgentMaterialRequestGate();
        try
        {
            var operations = new List<MaterialOperation>();
            for (var index = 0; index < concurrency; index++)
            {
                var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
                databasePaths.Add(databasePath);
                var database = CreateDatabase(databasePath);
                databases.Add(database);

                var materialId = 10_000L + index;
                var patientProfileId = 20_000L + index;
                var externalUserId = $"synthetic-account-{index}";
                database.Insertable(CreateMaterial(
                    materialId,
                    patientProfileId,
                    externalUserId)).ExecuteCommand();
                operations.Add(new MaterialOperation(
                    new SaveLifeAgentMaterialModelInputService(
                        SqlSugarRepository<SaveLifeEnrollmentMaterial>.CreateForContext(database),
                        new GeneratedMaterialFileReader(
                            SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes),
                        gate),
                    new SaveLifeAgentMaterialAccessScope(
                        externalUserId,
                        patientProfileId,
                        null),
                    new SaveLifeAgentMaterialReference
                    {
                        Id = materialId.ToString(),
                        MaterialId = materialId.ToString()
                    }));
            }

            ForceFullCollection();
            using var process = Process.GetCurrentProcess();
            process.Refresh();
            var baseline = new MemorySnapshot(
                GC.GetTotalMemory(forceFullCollection: false),
                process.WorkingSet64,
                process.PrivateMemorySize64);
            var allocatedBefore = GC.GetTotalAllocatedBytes(precise: true);
            await using var sampler = new ProcessMemorySampler(process, baseline);

            var buildTasks = operations.Select(operation => operation.Service.BuildAsync(
                operation.Scope,
                [operation.Reference],
                CreateOptions(),
                cancellationToken));
            preparedInputs.AddRange(await Task.WhenAll(buildTasks));
            Assert.All(preparedInputs, prepared =>
            {
                Assert.Equal(
                    SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes,
                    prepared.RawMediaBytes);
                Assert.Single(prepared.Content);
            });

            var handler = new HoldingRequestHandler(concurrency);
            var client = new SaveLifeAgentMultimodalChatClient(
                new FakeHttpClientFactory(handler));
            var options = CreateOptions();
            var requestTasks = preparedInputs.Select(prepared => client.CompleteAsync(
                options,
                new SaveLifeAgentMultimodalChatRequest
                {
                    SystemInstructions = "仅解读当前患者的合成测试材料",
                    UserMessage = "请解读材料",
                    MaterialContent = prepared.Content,
                    AnswerSchema = BinaryData.FromString(
                        "{\"type\":\"object\",\"additionalProperties\":false}")
                },
                cancellationToken)).ToArray();
            var requestCompletion = Task.WhenAll(requestTasks);
            var requestArrival = handler.AllRequestsArrived.Task.WaitAsync(
                TimeSpan.FromSeconds(30),
                cancellationToken);

            try
            {
                var firstCompletion = await Task.WhenAny(requestArrival, requestCompletion);
                if (firstCompletion == requestCompletion)
                    await requestCompletion;
                await requestArrival;
                await Task.Delay(100, cancellationToken);
                sampler.SampleNow();
            }
            finally
            {
                handler.ReleaseRequests();
            }

            await requestCompletion;
            sampler.SampleNow();
            var allocatedBytes = GC.GetTotalAllocatedBytes(precise: true) - allocatedBefore;
            return sampler.CreateMetrics(
                allocatedBytes / concurrency,
                handler.MaximumContentLength);
        }
        finally
        {
            foreach (var prepared in preparedInputs)
                await prepared.DisposeAsync();
            foreach (var database in databases)
                database.Dispose();
            foreach (var databasePath in databasePaths)
            {
                if (File.Exists(databasePath))
                    File.Delete(databasePath);
            }
        }
    }

    private static SaveLifeAgentOptions CreateOptions() => new()
    {
        Enabled = true,
        ChatEndpoint = "https://ark.cn-beijing.volces.com/api/coding/v3",
        ChatApiKey = "synthetic-test-key",
        ChatModel = "doubao-seed-2.0-lite"
    };

    private static SaveLifeEnrollmentMaterial CreateMaterial(
        long materialId,
        long patientProfileId,
        string externalUserId) => new()
        {
            Id = materialId,
            ExternalUserId = externalUserId,
            PatientProfileId = patientProfileId,
            EnrollmentRecordId = 0,
            MaterialType = "diagnosis_report",
            FileId = 30_000L + materialId,
            FileName = "synthetic.png",
            FileSuffix = ".png",
            ContentType = "image/png",
            SizeBytes = SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes,
            SizeKb = SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes / 1024,
            Status = "submitted",
            ExtractStatus = "synced",
            IndexStatus = "synced",
            CreateTime = new DateTime(2026, 7, 16)
        };

    private static SqlSugarClient CreateDatabase(string databasePath)
    {
        var database = new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = $"Data Source={databasePath};Pooling=False",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
            ConfigureExternalServices = new ConfigureExternalServices
            {
                EntityService = (property, column) =>
                {
                    if (new NullabilityInfoContext().Create(property).WriteState is NullabilityState.Nullable)
                        column.IsNullable = true;
                }
            }
        });
        database.CodeFirst.InitTables(typeof(SaveLifeEnrollmentMaterial));
        return database;
    }

    private static void ForceFullCollection()
    {
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
    }

    private static double ToMebibytes(long bytes) => bytes / (double)Mebibyte;

    private sealed record MaterialOperation(
        SaveLifeAgentMaterialModelInputService Service,
        SaveLifeAgentMaterialAccessScope Scope,
        SaveLifeAgentMaterialReference Reference);

    private sealed record MemorySnapshot(
        long ManagedBytes,
        long WorkingSetBytes,
        long PrivateBytes);

    private sealed record MemoryMetrics(
        long ManagedPeakDeltaBytes,
        long WorkingSetPeakDeltaBytes,
        long PrivatePeakDeltaBytes,
        long AllocatedPerRequestBytes,
        long MaximumSerializedRequestBytes);

    private sealed class ProcessMemorySampler : IAsyncDisposable
    {
        private readonly Process _process;
        private readonly MemorySnapshot _baseline;
        private readonly CancellationTokenSource _cancellation = new();
        private readonly Task _samplingTask;
        private long _managedPeak;
        private long _workingSetPeak;
        private long _privatePeak;

        public ProcessMemorySampler(Process process, MemorySnapshot baseline)
        {
            _process = process;
            _baseline = baseline;
            _managedPeak = baseline.ManagedBytes;
            _workingSetPeak = baseline.WorkingSetBytes;
            _privatePeak = baseline.PrivateBytes;
            _samplingTask = SampleContinuouslyAsync(_cancellation.Token);
        }

        public void SampleNow()
        {
            _process.Refresh();
            SetMaximum(ref _managedPeak, GC.GetTotalMemory(forceFullCollection: false));
            SetMaximum(ref _workingSetPeak, _process.WorkingSet64);
            SetMaximum(ref _privatePeak, _process.PrivateMemorySize64);
        }

        public MemoryMetrics CreateMetrics(
            long allocatedPerRequestBytes,
            long maximumSerializedRequestBytes) => new(
            Math.Max(0, Interlocked.Read(ref _managedPeak) - _baseline.ManagedBytes),
            Math.Max(0, Interlocked.Read(ref _workingSetPeak) - _baseline.WorkingSetBytes),
            Math.Max(0, Interlocked.Read(ref _privatePeak) - _baseline.PrivateBytes),
            allocatedPerRequestBytes,
            maximumSerializedRequestBytes);

        public async ValueTask DisposeAsync()
        {
            _cancellation.Cancel();
            try
            {
                await _samplingTask;
            }
            catch (OperationCanceledException)
            {
            }
            _cancellation.Dispose();
        }

        private async Task SampleContinuouslyAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                SampleNow();
                await Task.Delay(5, cancellationToken);
            }
        }

        private static void SetMaximum(ref long target, long value)
        {
            var current = Interlocked.Read(ref target);
            while (value > current)
            {
                var observed = Interlocked.CompareExchange(ref target, value, current);
                if (observed == current)
                    return;
                current = observed;
            }
        }
    }

    private sealed class HoldingRequestHandler(int expectedRequests) : HttpMessageHandler
    {
        private readonly TaskCompletionSource _release = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        private int _requestCount;
        private long _maximumContentLength;

        public TaskCompletionSource AllRequestsArrived { get; } = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public long MaximumContentLength => Interlocked.Read(ref _maximumContentLength);

        public void ReleaseRequests() => _release.TrySetResult();

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SetMaximum(
                ref _maximumContentLength,
                request.Content?.Headers.ContentLength ?? 0);
            if (Interlocked.Increment(ref _requestCount) == expectedRequests)
                AllRequestsArrived.TrySetResult();

            await _release.Task.WaitAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"choices\":[{\"message\":{\"content\":\"{}\"},\"finish_reason\":\"stop\"}]}",
                    Encoding.UTF8,
                    "application/json")
            };
        }

        private static void SetMaximum(ref long target, long value)
        {
            var current = Interlocked.Read(ref target);
            while (value > current)
            {
                var observed = Interlocked.CompareExchange(ref target, value, current);
                if (observed == current)
                    return;
                current = observed;
            }
        }
    }

    private sealed class FakeHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            Assert.Equal(SaveLifeConst.AgentChatHttpClientName, name);
            return new HttpClient(handler, disposeHandler: false);
        }
    }

    private sealed class GeneratedMaterialFileReader(long length) : ISaveLifeAgentMaterialFileReader
    {
        public Task<Stream> OpenReadAsync(
            long fileId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<Stream>(new GeneratedStream(length));
        }
    }

    private sealed class GeneratedStream(long length) : Stream
    {
        private const int MaximumReadBytes = 64 * 1024;
        private long _position;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => length;

        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = (int)Math.Min(
                Math.Min(count, MaximumReadBytes),
                length - _position);
            if (read <= 0)
                return 0;
            buffer.AsSpan(offset, read).Fill(0xfb);
            _position += read;
            return read;
        }

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
            var read = (int)Math.Min(
                Math.Min(buffer.Length, MaximumReadBytes),
                length - _position);
            if (read <= 0)
                return 0;
            buffer.Span[..read].Fill(0xfb);
            _position += read;
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
    }
}
