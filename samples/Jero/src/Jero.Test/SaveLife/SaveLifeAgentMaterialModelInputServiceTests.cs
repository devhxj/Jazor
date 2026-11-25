using System.Reflection;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentMaterialModelInputServiceTests
{
    [Fact]
    public void Constructor_RequiresProcessWideMaterialRequestGate()
    {
        var constructor = Assert.Single(typeof(SaveLifeAgentMaterialModelInputService).GetConstructors());
        var gateParameter = Assert.Single(
            constructor.GetParameters(),
            parameter => parameter.ParameterType == typeof(ISaveLifeAgentMaterialRequestGate));

        Assert.False(gateParameter.IsOptional);
        Assert.False(gateParameter.HasDefaultValue);
    }

    [Fact]
    public async Task BuildAsync_WithCurrentPatientImage_ReturnsImageDataUrl()
    {
        await WithDatabase(async db =>
        {
            var material = Material(1001, "openid-1", 501, ".png", "image/png");
            material.FileId = 7001;
            db.Insertable(material).ExecuteCommand();
            var reader = new FakeMaterialFileReader([1, 2, 3]);
            var service = CreateService(db, reader);

            var prepared = await service.BuildAsync(
                Scope(),
                [Reference(material.Id)],
                TestContext.Current.CancellationToken);

            var part = Assert.Single(prepared.Content);
            Assert.Equal(material.Id.ToString(), Assert.Single(prepared.References).MaterialId);
            Assert.Equal(SaveLifeAgentModelContentTypes.Image, part.Type);
            Assert.Equal("data:image/png;base64,AQID", part.Url);
            var evidence = Assert.Single(prepared.EvidenceSources);
            Assert.Equal(material.Id, evidence.MaterialId);
            Assert.Equal(SaveLifeAgentModelContentTypes.Image, evidence.Kind);
            Assert.Null(evidence.ExtractedText);
            Assert.Equal(1, reader.OpenCount);
            Assert.Equal(7001, reader.LastFileId);
        });
    }

    [Fact]
    public async Task BuildAsync_WithCurrentPatientVideo_ReturnsVideoDataUrl()
    {
        await WithDatabase(async db =>
        {
            var material = Material(1001, "openid-1", 501, ".mp4", "video/mp4");
            material.FileId = 7002;
            db.Insertable(material).ExecuteCommand();
            var reader = new FakeMaterialFileReader([4, 5, 6]);

            var prepared = await CreateService(db, reader).BuildAsync(
                Scope(),
                [Reference(material.Id)],
                TestContext.Current.CancellationToken);

            var part = Assert.Single(prepared.Content);
            Assert.Equal(SaveLifeAgentModelContentTypes.Video, part.Type);
            Assert.Equal("data:video/mp4;base64,BAUG", part.Url);
            Assert.Equal(1, reader.OpenCount);
        });
    }

    [Fact]
    public async Task BuildAsync_WithDocument_UsesExtractedTextWithoutOpeningBinary()
    {
        await WithDatabase(async db =>
        {
            var material = Material(1001, "openid-1", 501, ".pdf", "application/pdf");
            material.ExtractedText = "病理诊断：肺腺癌，ALK 融合阳性。";
            db.Insertable(material).ExecuteCommand();
            var reader = new FakeMaterialFileReader([1, 2, 3]);

            var prepared = await CreateService(db, reader).BuildAsync(
                Scope(),
                [Reference(material.Id)],
                TestContext.Current.CancellationToken);

            var part = Assert.Single(prepared.Content);
            Assert.Equal(SaveLifeAgentModelContentTypes.Text, part.Type);
            Assert.Contains("病理诊断：肺腺癌", part.Text, StringComparison.Ordinal);
            Assert.DoesNotContain("base64", part.Text, StringComparison.OrdinalIgnoreCase);
            var evidence = Assert.Single(prepared.EvidenceSources);
            Assert.Equal(material.Id, evidence.MaterialId);
            Assert.Equal(SaveLifeAgentModelContentTypes.Text, evidence.Kind);
            Assert.Equal(material.ExtractedText, evidence.ExtractedText);
            Assert.Equal(0, reader.OpenCount);
        });
    }

    [Fact]
    public async Task BuildAsync_WithOtherPatientMaterial_FailsBeforeOpeningFile()
    {
        await WithDatabase(async db =>
        {
            var material = Material(1001, "openid-1", 502, ".png", "image/png");
            material.FileId = 7001;
            db.Insertable(material).ExecuteCommand();
            var reader = new FakeMaterialFileReader([1, 2, 3]);

            var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
                CreateService(db, reader).BuildAsync(
                    Scope(),
                    [Reference(material.Id)],
                    TestContext.Current.CancellationToken));

            Assert.Equal(SaveLifeAgentFailureCodes.MaterialNotReady, error.Code);
            Assert.Equal(SaveLifeAgentFailureStages.Material, error.Stage);
            Assert.Equal(0, reader.OpenCount);
        });
    }

    [Fact]
    public async Task BuildAsync_WithProcessingMaterial_FailsBeforeOpeningFile()
    {
        await WithDatabase(async db =>
        {
            var material = Material(1001, "openid-1", 501, ".png", "image/png");
            material.FileId = 7001;
            material.ExtractStatus = "processing";
            material.IndexStatus = "pending";
            db.Insertable(material).ExecuteCommand();
            var reader = new FakeMaterialFileReader([1, 2, 3]);

            var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
                CreateService(db, reader).BuildAsync(
                    Scope(),
                    [Reference(material.Id)],
                    TestContext.Current.CancellationToken));

            Assert.Equal(SaveLifeAgentFailureCodes.MaterialNotReady, error.Code);
            Assert.Contains("处理中", error.Message, StringComparison.Ordinal);
            Assert.Equal(0, reader.OpenCount);
        });
    }

    [Fact]
    public async Task BuildAsync_WithUnsupportedMediaType_FailsBeforeOpeningFile()
    {
        await WithDatabase(async db =>
        {
            var material = Material(1001, "openid-1", 501, ".gif", "image/gif");
            material.FileId = 7001;
            db.Insertable(material).ExecuteCommand();
            var reader = new FakeMaterialFileReader([1, 2, 3]);

            var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
                CreateService(db, reader).BuildAsync(
                    Scope(),
                    [Reference(material.Id)],
                    TestContext.Current.CancellationToken));

            Assert.Equal(SaveLifeAgentFailureCodes.MaterialNotReady, error.Code);
            Assert.Equal(0, reader.OpenCount);
        });
    }

    [Fact]
    public async Task BuildAsync_WithDeclaredOversizedMedia_FailsBeforeOpeningFile()
    {
        await WithDatabase(async db =>
        {
            var material = Material(1001, "openid-1", 501, ".png", "image/png");
            material.FileId = 7001;
            material.SizeKb = SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes / 1024 + 1;
            db.Insertable(material).ExecuteCommand();
            var reader = new FakeMaterialFileReader([1, 2, 3]);

            var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
                CreateService(db, reader).BuildAsync(
                    Scope(),
                    [Reference(material.Id)],
                    TestContext.Current.CancellationToken));

            Assert.Equal(SaveLifeAgentFailureCodes.MaterialNotReady, error.Code);
            Assert.Equal(0, reader.OpenCount);
        });
    }

    [Fact]
    public async Task BuildAsync_WhenStreamExceedsLimit_FailsWithoutLeakingContent()
    {
        await WithDatabase(async db =>
        {
            var material = Material(1001, "openid-1", 501, ".png", "image/png");
            material.FileId = 7001;
            db.Insertable(material).ExecuteCommand();
            var bytes = new byte[SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes + 1];
            var reader = new FakeMaterialFileReader(bytes);

            var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
                CreateService(db, reader).BuildAsync(
                    Scope(),
                    [Reference(material.Id)],
                    TestContext.Current.CancellationToken));

            var summary = SaveLifeAgentDiagnostics.BuildErrorSummary(error);
            Assert.Equal(SaveLifeAgentFailureCodes.MaterialNotReady, error.Code);
            Assert.DoesNotContain("base64", summary, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(1, reader.OpenCount);
        });
    }

    [Fact]
    public async Task BuildAsync_WithNineRankedMedia_SelectsAllNineInRequestOrder()
    {
        await WithDatabase(async db =>
        {
            var materials = Enumerable.Range(1, 9)
                .Select(index =>
                {
                    var material = Material(1000 + index, "openid-1", 501, ".png", "image/png");
                    material.FileId = 7000 + index;
                    material.SizeBytes = 1;
                    material.SizeKb = 1;
                    return material;
                })
                .ToList();
            db.Insertable(materials).ExecuteCommand();
            var requestOrder = materials.AsEnumerable().Reverse().ToList();
            var reader = new FakeMaterialFileReader([1]);

            var prepared = await CreateService(db, reader).BuildAsync(
                Scope(),
                requestOrder.Select(material => Reference(material.Id)).ToList(),
                TestContext.Current.CancellationToken);

            Assert.Equal(9, prepared.Content.Count);
            Assert.Equal(requestOrder.Select(material => material.Id.ToString()).ToArray(),
                prepared.References.Select(reference => reference.MaterialId).ToArray());
            Assert.Equal(9, reader.OpenCount);
            Assert.Equal(requestOrder.Select(material => material.FileId).ToArray(), reader.OpenedFileIds);
            Assert.Equal(9, prepared.RawMediaBytes);
        });
    }

    [Fact]
    public async Task BuildAsync_WithTenDocumentsBeyondCharacterBudget_SelectsFirstNine()
    {
        await WithDatabase(async db =>
        {
            var materials = Enumerable.Range(1, 10)
                .Select(index =>
                {
                    var material = Material(1000 + index, "openid-1", 501, ".pdf", "application/pdf");
                    material.ExtractedText = new string('甲', 15_000);
                    return material;
                })
                .ToList();
            db.Insertable(materials).ExecuteCommand();
            var reader = new FakeMaterialFileReader([1]);

            var prepared = await CreateService(db, reader).BuildAsync(
                Scope(),
                materials.Select(material => Reference(material.Id)).ToList(),
                TestContext.Current.CancellationToken);

            Assert.Equal(9, prepared.Content.Count);
            Assert.Equal(materials.Take(9).Select(material => material.Id.ToString()).ToArray(),
                prepared.References.Select(reference => reference.MaterialId).ToArray());
            Assert.InRange(prepared.DocumentCharacters, 135_000, 144_000);
            Assert.Equal(0, reader.OpenCount);
        });
    }

    [Fact]
    public async Task BuildAsync_WithExactlyTwentyMegabytes_PreparesMediaWithinBudget()
    {
        await WithDatabase(async db =>
        {
            var material = Material(1001, "openid-1", 501, ".png", "image/png");
            material.FileId = 7001;
            material.SizeBytes = SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes;
            material.SizeKb = SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes / 1024;
            db.Insertable(material).ExecuteCommand();
            var reader = new GeneratedMaterialFileReader(
                SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes);

            await using var prepared = await CreateService(db, reader).BuildAsync(
                Scope(),
                [Reference(material.Id)],
                TestContext.Current.CancellationToken);

            Assert.Single(prepared.Content);
            Assert.Equal(SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes, prepared.RawMediaBytes);
            Assert.InRange(
                prepared.EstimatedSerializedBytes,
                1,
                SaveLifeMaterialResourceBudget.MaterialReviewSerializedRequestBytes);
            Assert.Equal(SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes, reader.BytesRead);
        });
    }

    [Fact]
    public async Task BuildAsync_WhenChunkedStreamExceedsTwentyMegabytes_StopsAfterBoundaryByte()
    {
        await WithDatabase(async db =>
        {
            var material = Material(1001, "openid-1", 501, ".png", "image/png");
            material.FileId = 7001;
            material.SizeBytes = SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes;
            material.SizeKb = SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes / 1024;
            db.Insertable(material).ExecuteCommand();
            var reader = new GeneratedMaterialFileReader(
                SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes + 1);

            await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
                CreateService(db, reader).BuildAsync(
                    Scope(),
                    [Reference(material.Id)],
                    TestContext.Current.CancellationToken));

            Assert.Equal(SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes + 1, reader.BytesRead);
        });
    }

    [Fact]
    public async Task BuildAsync_WhenMediaReadIsCanceled_ReleasesRequestGate()
    {
        await WithDatabase(async db =>
        {
            var material = Material(1001, "openid-1", 501, ".png", "image/png");
            material.FileId = 7001;
            material.SizeBytes = SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes;
            material.SizeKb = SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes / 1024;
            db.Insertable(material).ExecuteCommand();
            var reader = new CancelableMaterialFileReader();
            var gate = new SaveLifeAgentMaterialRequestGate();
            var service = new SaveLifeAgentMaterialModelInputService(
                SqlSugarRepository<SaveLifeEnrollmentMaterial>.CreateForContext(db),
                reader,
                gate);
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
                TestContext.Current.CancellationToken);
            var build = service.BuildAsync(
                Scope(),
                [Reference(material.Id)],
                cancellation.Token);
            await reader.ReadStarted.Task.WaitAsync(
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);

            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => build);
            await using var next = await gate.AcquireAsync(
                "openid-1",
                TestContext.Current.CancellationToken);
        });
    }

    [Fact]
    public async Task BuildAsync_WithFourTwentyMegabyteMedia_OpensOnlyHighestRankedMaterial()
    {
        await WithDatabase(async db =>
        {
            var materials = Enumerable.Range(1, 4)
                .Select(index =>
                {
                    var material = Material(1000 + index, "openid-1", 501, ".png", "image/png");
                    material.FileId = 7000 + index;
                    material.SizeBytes = SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes;
                    material.SizeKb = SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes / 1024;
                    return material;
                })
                .ToArray();
            db.Insertable(materials).ExecuteCommand();
            var reader = new GeneratedMaterialFileReader(
                SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes);

            await using var prepared = await CreateService(db, reader).BuildAsync(
                Scope(),
                materials.Select(material => Reference(material.Id)).ToArray(),
                TestContext.Current.CancellationToken);

            Assert.Single(prepared.Content);
            Assert.Equal(materials[0].Id.ToString(), Assert.Single(prepared.References).MaterialId);
            Assert.Equal(1, reader.OpenCount);
        });
    }

    private static SaveLifeAgentMaterialModelInputService CreateService(
        ISqlSugarClient db,
        ISaveLifeAgentMaterialFileReader reader)
    {
        return new SaveLifeAgentMaterialModelInputService(
            SqlSugarRepository<SaveLifeEnrollmentMaterial>.CreateForContext(db),
            reader,
            new SaveLifeAgentMaterialRequestGate());
    }

    private static SaveLifeAgentMaterialAccessScope Scope() =>
        new("openid-1", 501, null);

    private static SaveLifeAgentMaterialReference Reference(long id) => new()
    {
        Id = id.ToString(),
        MaterialId = id.ToString()
    };

    private static SaveLifeEnrollmentMaterial Material(
        long id,
        string owner,
        long patientProfileId,
        string suffix,
        string contentType)
    {
        return new SaveLifeEnrollmentMaterial
        {
            Id = id,
            ExternalUserId = owner,
            PatientProfileId = patientProfileId,
            EnrollmentRecordId = 0,
            MaterialType = "diagnosis_report",
            FileName = $"病例-{id}{suffix}",
            FileSuffix = suffix,
            ContentType = contentType,
            SizeBytes = 3,
            SizeKb = 1,
            Status = "submitted",
            ExtractStatus = "synced",
            IndexStatus = "synced",
            CreateTime = new DateTime(2026, 7, 15)
        };
    }

    private static async Task WithDatabase(Func<ISqlSugarClient, Task> test)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = $"Data Source={dbPath};Pooling=False",
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
            db.CodeFirst.InitTables(typeof(SaveLifeEnrollmentMaterial));
            await test(db);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    private sealed class FakeMaterialFileReader(byte[] bytes) : ISaveLifeAgentMaterialFileReader
    {
        public int OpenCount { get; private set; }

        public long? LastFileId { get; private set; }

        public List<long> OpenedFileIds { get; } = [];

        public Task<Stream> OpenReadAsync(long fileId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            OpenCount++;
            LastFileId = fileId;
            OpenedFileIds.Add(fileId);
            return Task.FromResult<Stream>(new MemoryStream(bytes, writable: false));
        }
    }

    private sealed class GeneratedMaterialFileReader(long length) : ISaveLifeAgentMaterialFileReader
    {
        public long BytesRead { get; private set; }

        public int OpenCount { get; private set; }

        public Task<Stream> OpenReadAsync(
            long fileId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            OpenCount++;
            return Task.FromResult<Stream>(new GeneratedStream(
                length,
                count => BytesRead += count));
        }
    }

    private sealed class CancelableMaterialFileReader : ISaveLifeAgentMaterialFileReader
    {
        public TaskCompletionSource ReadStarted { get; } = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<Stream> OpenReadAsync(
            long fileId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<Stream>(new CancelableStream(ReadStarted));
        }
    }

    private sealed class CancelableStream(TaskCompletionSource readStarted) : Stream
    {
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes;

        public override long Position
        {
            get => 0;
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            readStarted.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("Unreachable");
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
    }

    private sealed class GeneratedStream(long length, Action<int> onRead) : Stream
    {
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
            var read = (int)Math.Min(count, length - _position);
            if (read <= 0)
                return 0;
            Array.Clear(buffer, offset, read);
            _position += read;
            onRead(read);
            return read;
        }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = (int)Math.Min(buffer.Length, length - _position);
            if (read <= 0)
                return ValueTask.FromResult(0);
            buffer.Span[..read].Clear();
            _position += read;
            onRead(read);
            return ValueTask.FromResult(read);
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
