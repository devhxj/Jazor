using Microsoft.Extensions.Logging;

namespace Jero.Plugin.SaveLife.Service;

public interface ISaveLifeMaterialFileStore
{
    Task<SysFile> UploadAsync(UploadFileInput input, string targetPath);

    Task DeleteAsync(long fileId);
}

public sealed class SaveLifeMaterialFileStore(SysFileService fileService)
    : ISaveLifeMaterialFileStore, ITransient
{
    public Task<SysFile> UploadAsync(UploadFileInput input, string targetPath)
    {
        return fileService.UploadFile(input, targetPath);
    }

    public Task DeleteAsync(long fileId)
    {
        return fileService.DeleteFile(new BaseIdInput { Id = fileId });
    }
}

public interface ISaveLifeReliableMaterialUploadCoordinator
{
    Task<SaveLifeEnrollmentMaterial> ExecuteAsync(
        SaveLifeReliableMaterialUploadOperation operation,
        CancellationToken cancellationToken = default);
}

public sealed class SaveLifeReliableMaterialUploadCoordinator
    : ISaveLifeReliableMaterialUploadCoordinator, ITransient
{
    private readonly SqlSugarRepository<SaveLifeEnrollmentMaterial> _materialRep;
    private readonly ISaveLifeMaterialFileStore _fileStore;
    private readonly ISaveLifeMaterialWorkOutboxProcessor _outboxProcessor;
    private readonly ILogger<SaveLifeReliableMaterialUploadCoordinator> _logger;
    private readonly Func<long> _idGenerator;

    public SaveLifeReliableMaterialUploadCoordinator(
        SqlSugarRepository<SaveLifeEnrollmentMaterial> materialRep,
        ISaveLifeMaterialFileStore fileStore,
        ISaveLifeMaterialWorkOutboxProcessor outboxProcessor,
        ILogger<SaveLifeReliableMaterialUploadCoordinator> logger)
        : this(
            materialRep,
            fileStore,
            outboxProcessor,
            logger,
            Yitter.IdGenerator.YitIdHelper.NextId)
    {
    }

    internal SaveLifeReliableMaterialUploadCoordinator(
        SqlSugarRepository<SaveLifeEnrollmentMaterial> materialRep,
        ISaveLifeMaterialFileStore fileStore,
        ISaveLifeMaterialWorkOutboxProcessor outboxProcessor,
        ILogger<SaveLifeReliableMaterialUploadCoordinator> logger,
        Func<long> idGenerator)
    {
        _materialRep = materialRep;
        _fileStore = fileStore;
        _outboxProcessor = outboxProcessor;
        _logger = logger;
        _idGenerator = idGenerator;
    }

    public async Task<SaveLifeEnrollmentMaterial> ExecuteAsync(
        SaveLifeReliableMaterialUploadOperation operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        var fingerprint = await SaveLifeMaterialUploadCommandPolicy.ComputeContentFingerprintAsync(
            operation.File,
            cancellationToken);
        var fileName = Path.GetFileNameWithoutExtension(operation.File.FileName);
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 128)
            throw Oops.Oh("材料文件名不能为空或超过128个字符").StatusCode(400);
        var fileSuffix = SaveLifeEnrollmentMaterialHelper.NormalizeExtension(operation.File.FileName);
        var payload = new SaveLifeMaterialUploadPayload(
            operation.PatientProfileId,
            operation.EnrollmentRecordId,
            fingerprint.ContentSha256,
            fingerprint.ActualSizeBytes,
            fileName,
            fileSuffix,
            operation.File.ContentType,
            operation.MaterialType,
            operation.Remark);
        var replay = await SaveLifeMaterialUploadPersistence.TryReplayAsync(
            _materialRep.Context,
            new SaveLifeMaterialUploadReplayRequest(
                operation.TenantId,
                operation.ExternalUserId,
                operation.ClientCommandId,
                payload),
            cancellationToken);
        if (replay.Exists)
            return replay.Material!;

        SysFile? uploadedFile = null;
        var committed = false;
        try
        {
            uploadedFile = await _fileStore.UploadAsync(new UploadFileInput
            {
                File = operation.File,
                AllowSuffix = SaveLifeEnrollmentMaterialHelper.AllowedSuffixes,
                FileType = operation.FileType,
                IsPublic = false,
                DataId = operation.DataId
            }, "upload/save-life/materials");
            var candidate = new SaveLifeEnrollmentMaterial
            {
                TenantId = operation.TenantId,
                PatientProfileId = operation.PatientProfileId,
                EnrollmentRecordId = operation.EnrollmentRecordId,
                UserProfileId = operation.UserProfileId,
                ExternalUserId = operation.ExternalUserId,
                ProjectId = operation.ProjectId,
                FileId = uploadedFile.Id,
                SizeBytes = fingerprint.ActualSizeBytes,
                ContentSha256 = fingerprint.ContentSha256,
                MaterialType = operation.MaterialType,
                FileName = uploadedFile.FileName ?? fileName,
                FileSuffix = uploadedFile.Suffix ?? fileSuffix,
                ContentType = operation.File.ContentType,
                SizeKb = uploadedFile.SizeKb,
                SizeInfo = uploadedFile.SizeInfo ?? SaveLifeEnrollmentMaterialHelper.FormatSize(uploadedFile.SizeKb),
                Status = "submitted",
                ExtractStatus = "pending",
                IndexStatus = "pending",
                Remark = operation.Remark
            };
            var result = await SaveLifeMaterialUploadPersistence.CommitAsync(
                _materialRep.Context,
                new SaveLifeMaterialUploadRequest(
                    operation.TenantId,
                    operation.ExternalUserId,
                    operation.ClientCommandId,
                    payload,
                    candidate),
                DateTime.Now,
                _idGenerator,
                cancellationToken);
            committed = true;
            if (result.Material.FileId != uploadedFile.Id)
                await CompensateUploadedFileAsync(operation.TenantId, uploadedFile.Id);
            if (!result.IsReplay)
                await TryDispatchOutboxAsync(result.OutboxId);

            return result.Material;
        }
        catch
        {
            if (uploadedFile != null && !committed)
                await CompensateUploadedFileAsync(operation.TenantId, uploadedFile.Id);
            throw;
        }
    }

    private async Task TryDispatchOutboxAsync(long outboxId)
    {
        try
        {
            await _outboxProcessor.ProcessOneAsync(outboxId);
        }
        catch (Exception error)
        {
            _logger.LogWarning(
                error,
                "SaveLife material outbox immediate dispatch failed; scheduled recovery will retry outbox {OutboxId}",
                outboxId);
        }
    }

    private async Task CompensateUploadedFileAsync(long? tenantId, long fileId)
    {
        var hasActiveReference = await _materialRep.AsQueryable()
            .Where(item => item.FileId == fileId)
            .AnyAsync();
        if (hasActiveReference)
            return;

        try
        {
            await _fileStore.DeleteAsync(fileId);
        }
        catch
        {
            try
            {
                await SaveLifeMaterialUploadPersistence.QueueFileCleanupAsync(
                    _materialRep.Context,
                    tenantId,
                    fileId,
                    DateTime.Now,
                    _idGenerator);
            }
            catch (Exception cleanupError)
            {
                _logger.LogError(
                    cleanupError,
                    "SaveLife material file compensation and cleanup tracking both failed");
            }
        }
    }
}

public sealed record SaveLifeReliableMaterialUploadOperation(
    long? TenantId,
    string ExternalUserId,
    string ClientCommandId,
    long PatientProfileId,
    long EnrollmentRecordId,
    long ProjectId,
    long? UserProfileId,
    IFormFile File,
    string MaterialType,
    string? Remark,
    string FileType,
    long DataId);
