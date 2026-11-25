using Microsoft.Extensions.Logging;

namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 小程序账号注销服务。
/// </summary>
[Authorize]
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 145)]
public class SaveLifeAccountDeletionService : IDynamicApiController, ITransient
{
    private const string AnonymizedEnrollmentStatus = "account_cancelled";

    private readonly SqlSugarRepository<SaveLifeUserProfile> _profileRep;
    private readonly SqlSugarRepository<SaveLifeEnrollmentMaterial> _materialRep;
    private readonly SysFileService _fileService;
    private readonly ILogger<SaveLifeAccountDeletionService> _logger;

    public SaveLifeAccountDeletionService(
        SqlSugarRepository<SaveLifeUserProfile> profileRep,
        SqlSugarRepository<SaveLifeEnrollmentMaterial> materialRep,
        SysFileService fileService,
        ILogger<SaveLifeAccountDeletionService> logger)
    {
        _profileRep = profileRep;
        _materialRep = materialRep;
        _fileService = fileService;
        _logger = logger;
    }

    /// <summary>
    /// 申请注销当前小程序账号。
    /// </summary>
    [HttpPost("/api/v1/save-life/users/miniapp/account-deletion"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppRequestAccountDeletion")]
    public async Task<SaveLifeMiniAppAccountDeletionOutput> MiniAppRequestAccountDeletion(
        RequestSaveLifeMiniAppAccountDeletionInput input)
    {
        if (!input.Confirmed)
            throw Oops.Oh("请确认已知悉注销影响后继续").StatusCode(400);

        var externalUserId = GetCurrentMiniAppOpenId();
        var profile = await _profileRep.AsQueryable()
            .ClearFilter()
            .Where(item => item.ExternalUserId == externalUserId && !item.IsDelete)
            .FirstAsync()
            ?? throw Oops.Oh("用户档案不存在，请重新登录后重试").StatusCode(404);

        var db = _profileRep.Context;
        var materialFileIds = await _materialRep.AsQueryable()
            .ClearFilter()
            .Where(item => item.ExternalUserId == externalUserId)
            .Select(item => item.FileId)
            .ToListAsync();
        var materialIds = await _materialRep.AsQueryable()
            .ClearFilter()
            .Where(item => item.ExternalUserId == externalUserId)
            .Select(item => item.Id)
            .ToListAsync();
        var avatarFileIds = await _profileRep.AsQueryable()
            .ClearFilter()
            .Where(item => item.ExternalUserId == externalUserId)
            .Select(item => item.AvatarFileId)
            .ToListAsync();
        var fileIds = materialFileIds
            .Concat(avatarFileIds.Where(item => item.HasValue).Select(item => item!.Value))
            .Where(item => item > 0)
            .Distinct()
            .ToList();
        var now = DateTime.Now;
        var anonymizedAccountId = $"deleted:{profile.Id}:{now.Ticks}";

        db.Ado.BeginTran();
        try
        {
            await DeleteAccountDataAsync(db, externalUserId, materialIds);
            await AnonymizeEnrollmentRecordsAsync(db, externalUserId, anonymizedAccountId);
            await db.Deleteable<SaveLifePatientProfile>()
                .Where(item => item.OwnerExternalUserId == externalUserId)
                .ExecuteCommandAsync();
            await db.Deleteable<SaveLifeUserProfile>()
                .Where(item => item.ExternalUserId == externalUserId)
                .ExecuteCommandAsync();
            await db.Insertable(new SaveLifeAccountDeletionRecord
            {
                DeletedProfileId = profile.Id,
                RequestedTime = now,
                ProcessedTime = now,
                Status = "completed"
            }).ExecuteCommandAsync();
            db.Ado.CommitTran();
        }
        catch
        {
            db.Ado.RollbackTran();
            throw;
        }

        await DeleteFilesAsync(fileIds);
        return new SaveLifeMiniAppAccountDeletionOutput
        {
            ProcessedTime = now,
            ReRegistrationAllowed = true
        };
    }

    private static async Task DeleteAccountDataAsync(
        ISqlSugarClient db,
        string externalUserId,
        IReadOnlyCollection<long> materialIds)
    {
        if (materialIds.Count > 0)
        {
            await db.Deleteable<SaveLifeMaterialWorkOutbox>()
                .Where(item => materialIds.Contains(item.AggregateId))
                .ExecuteCommandAsync();
        }

        await db.Deleteable<SaveLifeMaterialUploadCommand>()
            .Where(item => item.ExternalUserId == externalUserId)
            .ExecuteCommandAsync();
        await db.Deleteable<SaveLifeEnrollmentMaterial>()
            .Where(item => item.ExternalUserId == externalUserId)
            .ExecuteCommandAsync();
        await db.Deleteable<SaveLifeAgentVectorIndex>()
            .Where(item => item.ExternalUserId == externalUserId)
            .ExecuteCommandAsync();
        await db.Deleteable<SaveLifePatientProfileProposal>()
            .Where(item => item.OwnerExternalUserId == externalUserId)
            .ExecuteCommandAsync();
        await db.Deleteable<SaveLifeAgentBusinessCommand>()
            .Where(item => item.ExternalUserId == externalUserId)
            .ExecuteCommandAsync();
        await db.Deleteable<SaveLifeAgentMessage>()
            .Where(item => item.ExternalUserId == externalUserId)
            .ExecuteCommandAsync();
        await db.Deleteable<SaveLifeAgentConversation>()
            .Where(item => item.ExternalUserId == externalUserId)
            .ExecuteCommandAsync();
        await db.Deleteable<SaveLifeFavorite>()
            .Where(item => item.ExternalUserId == externalUserId)
            .ExecuteCommandAsync();
        await db.Deleteable<SaveLifeKnowledgeReadHistory>()
            .Where(item => item.ExternalUserId == externalUserId)
            .ExecuteCommandAsync();
        await db.Deleteable<SaveLifeKnowledgeLike>()
            .Where(item => item.ExternalUserId == externalUserId)
            .ExecuteCommandAsync();
        await db.Deleteable<SaveLifeNotification>()
            .Where(item => item.ExternalUserId == externalUserId)
            .ExecuteCommandAsync();
        await db.Deleteable<SaveLifeFeedback>()
            .Where(item => item.ExternalUserId == externalUserId)
            .ExecuteCommandAsync();
        await db.Deleteable<SaveLifeSetting>()
            .Where(item => item.ExternalUserId == externalUserId)
            .ExecuteCommandAsync();
    }

    private static async Task AnonymizeEnrollmentRecordsAsync(
        ISqlSugarClient db,
        string externalUserId,
        string anonymizedAccountId)
    {
        await db.Updateable<SaveLifeEnrollmentRecord>()
            .SetColumns(item => new SaveLifeEnrollmentRecord
            {
                PatientProfileId = null,
                UserProfileId = null,
                ExternalUserId = anonymizedAccountId,
                ContactName = null,
                ContactPhone = null,
                Status = AnonymizedEnrollmentStatus,
                AuditRemark = null,
                DiseaseDescription = null,
                MedicalHistory = null,
                DiagnosisReport = null,
                MedicationStatus = null,
                OtherInfo = null,
                Notes = null,
                UpdateTime = DateTime.Now
            })
            .Where(item => item.ExternalUserId == externalUserId)
            .ExecuteCommandAsync();
    }

    private async Task DeleteFilesAsync(IEnumerable<long> fileIds)
    {
        foreach (var fileId in fileIds)
        {
            try
            {
                var materialStillReferenced = await _materialRep.AsQueryable()
                    .ClearFilter()
                    .Where(item => item.FileId == fileId && !item.IsDelete)
                    .AnyAsync();
                var avatarStillReferenced = await _profileRep.AsQueryable()
                    .ClearFilter()
                    .Where(item => item.AvatarFileId == fileId && !item.IsDelete)
                    .AnyAsync();
                if (!materialStillReferenced && !avatarStillReferenced)
                    await _fileService.TryDeleteFile(fileId);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "SaveLife account deletion file cleanup failed. FileId={FileId}", fileId);
            }
        }
    }

    private static string GetCurrentMiniAppOpenId()
    {
        var openId = App.User?.FindFirst(ClaimConst.OpenId)?.Value?.Trim();
        if (!string.IsNullOrWhiteSpace(openId))
            return openId;

        throw Oops.Oh("小程序登录态无效，请重新登录").StatusCode(400);
    }
}
