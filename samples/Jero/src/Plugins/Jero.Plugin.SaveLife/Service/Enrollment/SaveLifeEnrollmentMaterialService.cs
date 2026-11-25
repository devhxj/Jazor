


using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 报名材料服务
/// </summary>
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 112)]
public class SaveLifeEnrollmentMaterialService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SaveLifeEnrollmentMaterial> _materialRep;
    private readonly SqlSugarRepository<SaveLifeEnrollmentRecord> _recordRep;
    private readonly SqlSugarRepository<SaveLifeUserProfile> _profileRep;
    private readonly SqlSugarRepository<SaveLifePatientProfile> _patientProfileRep;
    private readonly SqlSugarRepository<SaveLifeNotification> _notificationRep;
    private readonly ISaveLifeFileAccessService _fileAccessService;
    private readonly ISaveLifeMaterialIndexProcessor _materialIndexProcessor;
    private readonly ISaveLifeReliableMaterialUploadCoordinator _uploadCoordinator;
    private readonly ISaveLifePrivacyConsentGuard _privacyConsentGuard;

    public SaveLifeEnrollmentMaterialService(
        SqlSugarRepository<SaveLifeEnrollmentMaterial> materialRep,
        SqlSugarRepository<SaveLifeEnrollmentRecord> recordRep,
        SqlSugarRepository<SaveLifeUserProfile> profileRep,
        SqlSugarRepository<SaveLifePatientProfile> patientProfileRep,
        SqlSugarRepository<SaveLifeNotification> notificationRep,
        ISaveLifeFileAccessService fileAccessService,
        ISaveLifeMaterialIndexProcessor materialIndexProcessor,
        ISaveLifeReliableMaterialUploadCoordinator uploadCoordinator,
        ISaveLifePrivacyConsentGuard privacyConsentGuard)
    {
        _materialRep = materialRep;
        _recordRep = recordRep;
        _profileRep = profileRep;
        _patientProfileRep = patientProfileRep;
        _notificationRep = notificationRep;
        _fileAccessService = fileAccessService;
        _materialIndexProcessor = materialIndexProcessor;
        _uploadCoordinator = uploadCoordinator;
        _privacyConsentGuard = privacyConsentGuard;
    }

    /// <summary>
    /// 分页查询报名材料
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<SaveLifeEnrollmentMaterialOutput>> Page(SaveLifeEnrollmentMaterialInput input)
    {
        var externalUserId = input.ExternalUserId?.Trim();
        var page = await _materialRep.AsQueryable()
            .WhereIF(input.EnrollmentRecordId > 0, u => u.EnrollmentRecordId == input.EnrollmentRecordId)
            .WhereIF(!string.IsNullOrWhiteSpace(externalUserId), u => u.ExternalUserId == externalUserId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialType), u => u.MaterialType == input.MaterialType)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Status), u => u.Status == input.Status)
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExtractStatus), u => u.ExtractStatus == input.ExtractStatus)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);

        return new SqlSugarPagedList<SaveLifeEnrollmentMaterialOutput>
        {
            Page = page.Page,
            PageSize = page.PageSize,
            Total = page.Total,
            TotalPages = page.TotalPages,
            HasNextPage = page.HasNextPage,
            HasPrevPage = page.HasPrevPage,
            Items = page.Items.Select(u => ToOutput(u, true)).ToList()
        };
    }

    /// <summary>
    /// 审核报名材料
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Audit")]
    public async Task Audit(AuditSaveLifeEnrollmentMaterialInput input)
    {
        var status = SaveLifeEnrollmentMaterialHelper.NormalizeStatus(input.Status);
        var reviewRemark = SaveLifeEnrollmentMaterialHelper.NormalizeText(input.ReviewRemark, 512, "审核备注");
        var supplementRequest = SaveLifeEnrollmentMaterialHelper.NormalizeText(input.SupplementRequest, 512, "补充资料要求");

        var material = await _materialRep.GetByIdAsync(input.Id)
            ?? throw Oops.Oh("报名材料不存在").StatusCode(404);

        material.Status = status;
        material.ReviewRemark = reviewRemark;
        material.SupplementRequest = status == "need_supplement" ? supplementRequest : null;
        material.ReviewedTime = DateTime.Now;
        await _materialRep.AsUpdateable(material).IgnoreColumns(true).ExecuteCommandAsync();
        await AddMaterialNotification(material);
        await _materialIndexProcessor.QueueMaterialAsync(material.Id);
    }

    /// <summary>
    /// 小程序查询当前用户报名材料
    /// </summary>
    [Authorize]
    [HttpGet("/api/v1/save-life/enrollments/{enrollmentId}/materials"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppList")]
    public async Task<List<SaveLifeEnrollmentMaterialOutput>> MiniAppList(long enrollmentId)
    {
        var record = await GetCurrentEnrollment(enrollmentId);
        var patient = await ResolveEnrollmentPatient(record, null);
        var materials = await _materialRep.AsQueryable()
            .Where(u =>
                u.ExternalUserId == record.ExternalUserId &&
                u.PatientProfileId == patient.Id &&
                (u.EnrollmentRecordId <= 0 || u.EnrollmentRecordId == record.Id))
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .Take(SaveLifeEnrollmentMaterialHelper.MaxMaterialListCount)
            .ToListAsync();

        return materials
            .Select(u => ToOutput(
                u,
                SaveLifeEnrollmentMaterialHelper.IsPersonalMaterial(u)
                    ? SaveLifeEnrollmentMaterialHelper.CanDeletePersonalMaterial(u)
                    : CanModify(record, u)))
            .ToList();
    }

    /// <summary>
    /// 小程序查询当前用户个人病例资料库
    /// </summary>
    [Authorize]
    [HttpGet("/api/v1/save-life/materials"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppPersonalList")]
    public async Task<List<SaveLifeEnrollmentMaterialOutput>> MiniAppPersonalList()
    {
        var externalUserId = GetCurrentMiniAppOpenId();
        var patient = await ResolvePersonalPatient(externalUserId);
        var materials = await _materialRep.AsQueryable()
            .Where(u =>
                u.ExternalUserId == externalUserId &&
                u.PatientProfileId == patient.Id &&
                u.EnrollmentRecordId <= 0)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .Take(SaveLifeEnrollmentMaterialHelper.MaxMaterialListCount)
            .ToListAsync();

        return materials
            .Select(u => ToOutput(u, SaveLifeEnrollmentMaterialHelper.CanDeletePersonalMaterial(u)))
            .ToList();
    }

    /// <summary>
    /// 小程序上传当前用户报名材料
    /// </summary>
    [Authorize]
    [HttpPost("/api/v1/save-life/enrollments/{enrollmentId}/materials"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppUpload")]
    [RequestSizeLimit(SaveLifeMaterialResourceBudget.UploadRequestBytes)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes,
        ValueCountLimit = SaveLifeMaterialResourceBudget.FormValueCountLimit,
        KeyLengthLimit = SaveLifeMaterialResourceBudget.FormKeyLengthLimit,
        ValueLengthLimit = SaveLifeMaterialResourceBudget.FormValueLengthLimit,
        MultipartBoundaryLengthLimit = SaveLifeMaterialResourceBudget.MultipartBoundaryLengthLimit,
        MultipartHeadersCountLimit = SaveLifeMaterialResourceBudget.MultipartHeadersCountLimit,
        MultipartHeadersLengthLimit = SaveLifeMaterialResourceBudget.MultipartHeadersLengthLimit)]
    [RequestTimeout(SaveLifeMaterialResourceBudget.UploadTimeoutMilliseconds)]
    public async Task<SaveLifeEnrollmentMaterialOutput> MiniAppUpload(
        long enrollmentId,
        [FromForm] UploadSaveLifeMiniAppEnrollmentMaterialInput input,
        CancellationToken cancellationToken = default)
    {
        await _privacyConsentGuard.EnsureConsentedAsync();
        await _privacyConsentGuard.EnsureSensitiveInformationConsentedAsync();
        var record = await GetCurrentEnrollment(enrollmentId);
        var patient = await ResolveEnrollmentPatient(record, input.PatientProfileId);
        if (!CanModify(record, null))
            throw Oops.Oh("当前报名状态不允许上传材料").StatusCode(409);

        var profile = await _profileRep.GetFirstAsync(u => u.ExternalUserId == record.ExternalUserId);
        return await UploadReliableMaterialAsync(
            input,
            record.TenantId,
            record.ExternalUserId!,
            patient,
            record.Id,
            record.ProjectId,
            record.UserProfileId ?? profile?.Id,
            "save-life-enrollment-material",
            record.Id,
            material => CanModify(record, material),
            cancellationToken);
    }

    /// <summary>
    /// 小程序上传当前用户个人病例资料
    /// </summary>
    [Authorize]
    [HttpPost("/api/v1/save-life/materials"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppPersonalUpload")]
    [RequestSizeLimit(SaveLifeMaterialResourceBudget.UploadRequestBytes)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes,
        ValueCountLimit = SaveLifeMaterialResourceBudget.FormValueCountLimit,
        KeyLengthLimit = SaveLifeMaterialResourceBudget.FormKeyLengthLimit,
        ValueLengthLimit = SaveLifeMaterialResourceBudget.FormValueLengthLimit,
        MultipartBoundaryLengthLimit = SaveLifeMaterialResourceBudget.MultipartBoundaryLengthLimit,
        MultipartHeadersCountLimit = SaveLifeMaterialResourceBudget.MultipartHeadersCountLimit,
        MultipartHeadersLengthLimit = SaveLifeMaterialResourceBudget.MultipartHeadersLengthLimit)]
    [RequestTimeout(SaveLifeMaterialResourceBudget.UploadTimeoutMilliseconds)]
    public async Task<SaveLifeEnrollmentMaterialOutput> MiniAppPersonalUpload(
        [FromForm] UploadSaveLifeMiniAppEnrollmentMaterialInput input,
        CancellationToken cancellationToken = default)
    {
        var externalUserId = GetCurrentMiniAppOpenId();
        await _privacyConsentGuard.EnsureConsentedAsync(externalUserId);
        await _privacyConsentGuard.EnsureSensitiveInformationConsentedAsync(externalUserId);
        var patient = await ResolvePersonalPatient(externalUserId);
        var profile = await _profileRep.GetFirstAsync(u => u.ExternalUserId == externalUserId);
        return await UploadReliableMaterialAsync(
            input,
            patient.TenantId,
            externalUserId,
            patient,
            enrollmentRecordId: 0,
            projectId: 0,
            profile?.Id,
            "save-life-user-material",
            profile?.Id ?? 0,
            _ => true,
            cancellationToken);
    }

    /// <summary>
    /// 运营端重试病例材料后台解析和私有索引
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "RetryExtract")]
    public async Task RetryExtract(RetrySaveLifeEnrollmentMaterialExtractInput input)
    {
        await _materialIndexProcessor.QueueMaterialAsync(input.Id);
    }

    /// <summary>
    /// 小程序重试当前用户报名材料的后台解析和私有索引
    /// </summary>
    [Authorize]
    [HttpPost("/api/v1/save-life/enrollments/{enrollmentId}/materials/{materialId}/retry"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppRetry")]
    public async Task MiniAppRetry(
        long enrollmentId,
        long materialId,
        CancellationToken cancellationToken = default)
    {
        var record = await GetCurrentEnrollment(enrollmentId);
        var material = await GetCurrentMaterial(record, materialId);
        EnsureMaterialCanRetry(material);
        await _materialIndexProcessor.QueueMaterialAsync(material.Id, cancellationToken);
    }

    /// <summary>
    /// 小程序重试当前用户个人病例资料的后台解析和私有索引
    /// </summary>
    [Authorize]
    [HttpPost("/api/v1/save-life/materials/{materialId}/retry"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppPersonalRetry")]
    public async Task MiniAppPersonalRetry(
        long materialId,
        CancellationToken cancellationToken = default)
    {
        var material = await GetCurrentUserMaterial(materialId);
        EnsureMaterialCanRetry(material);
        await _materialIndexProcessor.QueueMaterialAsync(material.Id, cancellationToken);
    }

    /// <summary>
    /// 小程序删除当前用户报名材料
    /// </summary>
    [Authorize]
    [HttpDelete("/api/v1/save-life/enrollments/{enrollmentId}/materials/{materialId}"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppDelete")]
    public async Task MiniAppDelete(
        long enrollmentId,
        long materialId,
        CancellationToken cancellationToken = default)
    {
        var record = await GetCurrentEnrollment(enrollmentId);
        var material = await GetCurrentMaterial(record, materialId, includeDeleted: true);

        if (material.IsDelete)
            return;

        var canDelete = SaveLifeEnrollmentMaterialHelper.IsPersonalMaterial(material)
            ? SaveLifeEnrollmentMaterialHelper.CanDeletePersonalMaterial(material)
            : CanModify(record, material);
        if (!canDelete)
            throw Oops.Oh("当前报名材料不允许删除").StatusCode(409);

        await DeleteMaterialAndUnusedFile(material, cancellationToken);
    }

    /// <summary>
    /// 小程序删除当前用户个人病例资料
    /// </summary>
    [Authorize]
    [HttpDelete("/api/v1/save-life/materials/{materialId}"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppPersonalDelete")]
    public async Task MiniAppPersonalDelete(
        long materialId,
        CancellationToken cancellationToken = default)
    {
        var material = await GetCurrentUserMaterial(materialId, includeDeleted: true);

        if (material.IsDelete)
            return;

        if (!SaveLifeEnrollmentMaterialHelper.CanDeletePersonalMaterial(material))
            throw Oops.Oh("当前材料不允许删除").StatusCode(409);

        await DeleteMaterialAndUnusedFile(material, cancellationToken);
    }

    /// <summary>
    /// 小程序预览当前用户报名材料
    /// </summary>
    [Authorize]
    [HttpGet("/api/v1/save-life/enrollments/{enrollmentId}/materials/{materialId}/preview"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppPreview")]
    public async Task<IActionResult> MiniAppPreview(long enrollmentId, long materialId)
    {
        var record = await GetCurrentEnrollment(enrollmentId);
        var material = await GetCurrentMaterial(record, materialId);

        return await _fileAccessService.DownloadAsync(material.FileId);
    }

    /// <summary>
    /// 小程序预览当前用户个人病例资料
    /// </summary>
    [Authorize]
    [HttpGet("/api/v1/save-life/materials/{materialId}/preview"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppPersonalPreview")]
    public async Task<IActionResult> MiniAppPersonalPreview(
        long materialId)
    {
        var material = await GetCurrentUserMaterial(materialId);

        return await _fileAccessService.DownloadAsync(material.FileId);
    }

    private async Task<SaveLifeEnrollmentRecord> GetCurrentEnrollment(long enrollmentId)
    {
        var openId = GetCurrentMiniAppOpenId();
        var record = await _recordRep.GetFirstAsync(u => u.Id == enrollmentId && u.ExternalUserId == openId);

        return record ?? throw Oops.Oh("报名记录不存在或不属于当前用户").StatusCode(404);
    }

    private static string GetCurrentMiniAppOpenId()
    {
        return NormalizeRequired(App.User?.FindFirst(ClaimConst.OpenId)?.Value, "小程序登录态无效，请重新登录");
    }

    private async Task<SaveLifeEnrollmentMaterial> GetCurrentMaterial(
        SaveLifeEnrollmentRecord record,
        long materialId,
        bool includeDeleted = false)
    {
        var patient = await ResolveEnrollmentPatient(record, null);
        var query = includeDeleted
            ? _materialRep.AsQueryable().ClearFilter()
            : _materialRep.AsQueryable();
        var material = await query.Where(u =>
            u.Id == materialId &&
            (u.EnrollmentRecordId <= 0 || u.EnrollmentRecordId == record.Id) &&
            u.ExternalUserId == record.ExternalUserId &&
            u.PatientProfileId == patient.Id)
            .FirstAsync();

        return material ?? throw Oops.Oh("报名材料不存在或不属于当前用户").StatusCode(404);
    }

    private async Task<SaveLifeEnrollmentMaterial> GetCurrentUserMaterial(
        long materialId,
        bool includeDeleted = false)
    {
        var externalUserId = GetCurrentMiniAppOpenId();
        var patient = await ResolvePersonalPatient(externalUserId);
        var query = includeDeleted
            ? _materialRep.AsQueryable().ClearFilter()
            : _materialRep.AsQueryable();
        var material = await query.Where(u =>
            u.Id == materialId &&
            u.ExternalUserId == externalUserId &&
            u.PatientProfileId == patient.Id &&
            u.EnrollmentRecordId <= 0)
            .FirstAsync();

        return material ?? throw Oops.Oh("材料不存在或不属于当前用户").StatusCode(404);
    }

    private async Task<SaveLifePatientProfile> ResolvePersonalPatient(
        string externalUserId)
    {
        var patients = await LoadOwnedPatients(externalUserId);
        return SaveLifePatientMaterialScopePolicy.ResolveCurrentAccountPatient(patients, externalUserId);
    }

    private async Task<SaveLifePatientProfile> ResolveEnrollmentPatient(
        SaveLifeEnrollmentRecord record,
        string? requestedPatientProfileId)
    {
        var patients = await LoadOwnedPatients(record.ExternalUserId!);
        return SaveLifePatientMaterialScopePolicy.ResolveEnrollmentPatient(
            patients,
            record.ExternalUserId!,
            record.PatientProfileId,
            requestedPatientProfileId);
    }

    private async Task<List<SaveLifePatientProfile>> LoadOwnedPatients(string externalUserId)
    {
        return await _patientProfileRep.AsQueryable()
            .Where(patient =>
                patient.OwnerExternalUserId == externalUserId &&
                !patient.IsDelete)
            .ToListAsync();
    }

    private async Task<SaveLifeEnrollmentMaterialOutput> UploadReliableMaterialAsync(
        UploadSaveLifeMiniAppEnrollmentMaterialInput input,
        long? tenantId,
        string externalUserId,
        SaveLifePatientProfile patient,
        long enrollmentRecordId,
        long projectId,
        long? userProfileId,
        string fileType,
        long dataId,
        Func<SaveLifeEnrollmentMaterial, bool> canDelete,
        CancellationToken cancellationToken)
    {
        var clientCommandId = input.ClientCommandId?.Trim();
        if (string.IsNullOrWhiteSpace(clientCommandId) || clientCommandId.Length > 128)
            throw Oops.Oh("客户端命令Id不能为空或超过128个字符").StatusCode(400);

        var file = NormalizeUploadFile(input.File, input.OriginalFileName);
        var materialType = SaveLifeEnrollmentMaterialHelper.NormalizeMaterialType(input.MaterialType);
        var remark = SaveLifeEnrollmentMaterialHelper.NormalizeText(input.Remark, 512, "材料备注");
        var material = await _uploadCoordinator.ExecuteAsync(
            new SaveLifeReliableMaterialUploadOperation(
                tenantId,
                externalUserId,
                clientCommandId,
                patient.Id,
                enrollmentRecordId,
                projectId,
                userProfileId,
                file,
                materialType,
                remark,
                fileType,
                dataId),
            cancellationToken);
        return ToOutput(material, canDelete(material));
    }

    private async Task DeleteMaterialAndUnusedFile(
        SaveLifeEnrollmentMaterial material,
        CancellationToken cancellationToken)
    {
        await SaveLifeMaterialDeletionPersistence.DeleteAsync(
            _materialRep.Context,
            new SaveLifeMaterialDeletionRequest(
                material.TenantId,
                material.ExternalUserId,
                material.PatientProfileId ?? throw new InvalidOperationException("材料缺少患者归属"),
                material.EnrollmentRecordId,
                material.Id),
            DateTime.Now,
            Yitter.IdGenerator.YitIdHelper.NextId,
            cancellationToken);
    }

    private static IFormFile NormalizeUploadFile(IFormFile? file, string? originalFileName)
    {
        if (file == null || file.Length <= 0)
            throw Oops.Oh("材料文件不能为空").StatusCode(400);

        if (file.Length > SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes)
            throw Oops.Oh("材料文件不能超过20MB").StatusCode(413);

        var fileName = ResolveUploadFileName(file, originalFileName);
        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw Oops.Oh("材料文件名不正确").StatusCode(400);

        var extension = SaveLifeEnrollmentMaterialHelper.NormalizeExtension(fileName);
        if (!SaveLifeEnrollmentMaterialHelper.IsAllowedExtension(extension))
            throw Oops.Oh("仅支持 jpg、png、pdf、doc、docx、mp4、mov 材料").StatusCode(400);

        var contentType = SaveLifeUploadFileValidator.ValidateAndResolveContentType(file, extension);
        if (contentType == file.ContentType && file.FileName == fileName)
            return file;

        var normalizedFile = new FormFile(file.OpenReadStream(), 0, file.Length, file.Name, fileName)
        {
            Headers = file.Headers ?? new HeaderDictionary(),
            ContentType = contentType
        };
        return normalizedFile;
    }

    private static string ResolveUploadFileName(IFormFile file, string? originalFileName)
    {
        var normalizedOriginalName = Path.GetFileName(originalFileName?.Trim() ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(normalizedOriginalName))
            return normalizedOriginalName;

        var formFileName = Path.GetFileName(file.FileName?.Trim() ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(formFileName) && Path.HasExtension(formFileName))
            return formFileName;

        return $"病例材料{ResolveExtensionFromContentType(file.ContentType)}";
    }

    private static string ResolveExtensionFromContentType(string? contentType)
    {
        return contentType?.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/png" => ".png",
            "application/pdf" => ".pdf",
            "application/msword" => ".doc",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
            "video/mp4" => ".mp4",
            "video/quicktime" => ".mov",
            _ => string.Empty
        };
    }

    private static bool CanModify(SaveLifeEnrollmentRecord record, SaveLifeEnrollmentMaterial? material)
    {
        if (record.Status is "approved" or "cancelled" or "closed")
            return false;

        return material == null || material.Status is "submitted" or "need_supplement" or "rejected";
    }

    private static void EnsureMaterialCanRetry(SaveLifeEnrollmentMaterial material)
    {
        var readiness = SaveLifeAgentMaterialRetrievalPolicy.GetReadinessStatus(material);
        if (readiness == SaveLifeAgentMaterialReadinessStatuses.Processing)
            throw Oops.Oh("病例资料仍在处理中，无需重复提交").StatusCode(409);
        if (readiness == SaveLifeAgentMaterialReadinessStatuses.Ready)
            throw Oops.Oh("病例资料已处理完成，无需重试").StatusCode(409);
    }

    internal static SaveLifeEnrollmentMaterialOutput ToOutput(SaveLifeEnrollmentMaterial material, bool canDelete)
    {
        return new SaveLifeEnrollmentMaterialOutput
        {
            Id = material.Id.ToString(),
            PatientProfileId = material.PatientProfileId?.ToString(),
            EnrollmentRecordId = material.EnrollmentRecordId.ToString(),
            IsPersonalMaterial = SaveLifeEnrollmentMaterialHelper.IsPersonalMaterial(material),
            ExternalUserId = material.ExternalUserId,
            ProjectId = material.ProjectId.ToString(),
            MaterialType = material.MaterialType,
            MaterialTypeText = SaveLifeEnrollmentMaterialHelper.GetMaterialTypeText(material.MaterialType),
            FileName = BuildDisplayFileName(material),
            FileSuffix = material.FileSuffix,
            ContentType = material.ContentType,
            SizeKb = material.SizeKb,
            SizeInfo = material.SizeInfo ?? SaveLifeEnrollmentMaterialHelper.FormatSize(material.SizeKb),
            Status = material.Status,
            StatusText = SaveLifeEnrollmentMaterialHelper.GetStatusText(material.Status),
            Remark = material.Remark,
            ReviewRemark = material.ReviewRemark,
            SupplementRequest = material.SupplementRequest,
            ExtractStatus = material.ExtractStatus,
            ExtractStatusText = SaveLifeEnrollmentMaterialHelper.GetExtractStatusText(material.ExtractStatus),
            ExtractError = material.ExtractError,
            ExtractedTime = material.ExtractedTime,
            ExtractModel = material.ExtractModel,
            IndexStatus = material.IndexStatus,
            IndexStatusText = GetIndexStatusText(material.IndexStatus),
            IndexError = material.IndexError,
            IndexedTime = material.IndexedTime,
            ProcessRetryCount = material.ProcessRetryCount,
            NextProcessRetryTime = material.NextProcessRetryTime,
            ReviewedTime = material.ReviewedTime,
            CreateTime = material.CreateTime,
            UpdateTime = material.UpdateTime,
            CanDelete = canDelete
        };
    }

    private static string GetIndexStatusText(string? status)
    {
        return status switch
        {
            "synced" => "已进入私有知识库",
            "failed" => "索引失败",
            "skipped" => "已跳过自动入库",
            _ => "等待索引"
        };
    }

    private static string BuildDisplayFileName(SaveLifeEnrollmentMaterial material)
    {
        var fileName = material.FileName;
        if (string.IsNullOrWhiteSpace(fileName))
            return "病例材料";

        return !string.IsNullOrWhiteSpace(material.FileSuffix) && !fileName.EndsWith(material.FileSuffix, StringComparison.OrdinalIgnoreCase)
            ? $"{fileName}{material.FileSuffix}"
            : fileName;
    }

    private static string NormalizeRequired(string? value, string errorMessage)
    {
        var normalized = value?.Trim();
        if (!string.IsNullOrWhiteSpace(normalized))
            return normalized;

        throw Oops.Oh(errorMessage).StatusCode(400);
    }

    private static string? Limit(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private async Task AddMaterialNotification(SaveLifeEnrollmentMaterial material)
    {
        var title = material.Status switch
        {
            "approved" => "报名材料已通过",
            "need_supplement" => "报名材料需补充",
            "rejected" => "报名材料已退回",
            _ => "报名材料状态已更新"
        };
        var content = material.Status switch
        {
            "approved" => $"您上传的“{BuildDisplayFileName(material)}”已通过审核。",
            "need_supplement" => $"您上传的“{BuildDisplayFileName(material)}”需要补充说明：{material.SupplementRequest ?? material.ReviewRemark}",
            "rejected" => $"您上传的“{BuildDisplayFileName(material)}”已退回：{material.ReviewRemark}",
            _ => $"您上传的“{BuildDisplayFileName(material)}”状态已更新为 {material.Status}。"
        };

        await _notificationRep.InsertAsync(new SaveLifeNotification
        {
            ExternalUserId = material.ExternalUserId,
            NotificationType = "material",
            Title = title,
            Content = string.IsNullOrWhiteSpace(content) ? null : content.Length <= 512 ? content : content[..512],
            BusinessType = material.EnrollmentRecordId > 0 ? "enrollment" : "profile_material",
            BusinessId = material.EnrollmentRecordId > 0 ? material.EnrollmentRecordId : material.Id
        });
    }
}

public static class SaveLifePatientMaterialScopePolicy
{
    public static SaveLifePatientProfile ResolveCurrentAccountPatient(
        IReadOnlyList<SaveLifePatientProfile> patients,
        string ownerExternalUserId)
    {
        return ResolvePersonalPatient(patients, ownerExternalUserId, requestedPatientProfileId: null);
    }

    public static SaveLifePatientProfile ResolvePersonalPatient(
        IReadOnlyList<SaveLifePatientProfile> patients,
        string ownerExternalUserId,
        string? requestedPatientProfileId)
    {
        ArgumentNullException.ThrowIfNull(patients);
        var owner = NormalizeOwner(ownerExternalUserId);
        var ownedPatients = patients
            .Where(patient => SaveLifePatientOwnershipPolicy.IsOwnedBy(patient, owner))
            .ToList();

        if (string.IsNullOrWhiteSpace(requestedPatientProfileId))
        {
            var selfPatients = ownedPatients
                .Where(patient => patient.RelationshipToAccount == SaveLifePatientRelationships.Self)
                .ToList();
            if (selfPatients.Count != 1)
                throw Oops.Oh("本人患者档案不存在，请先完善患者档案").StatusCode(409);

            return selfPatients[0];
        }

        var patientProfileId = ParsePatientProfileId(requestedPatientProfileId);
        var patient = ownedPatients.SingleOrDefault(item => item.Id == patientProfileId);
        if (patient == null)
            throw Oops.Oh("患者档案不存在或无权访问").StatusCode(404);

        return patient;
    }

    public static SaveLifePatientProfile ResolveEnrollmentPatient(
        IReadOnlyList<SaveLifePatientProfile> patients,
        string ownerExternalUserId,
        long? enrollmentPatientProfileId,
        string? requestedPatientProfileId)
    {
        if (enrollmentPatientProfileId is > 0)
        {
            if (!string.IsNullOrWhiteSpace(requestedPatientProfileId) &&
                ParsePatientProfileId(requestedPatientProfileId) != enrollmentPatientProfileId.Value)
            {
                throw Oops.Oh("材料患者与报名患者不一致").StatusCode(409);
            }

            return ResolvePersonalPatient(
                patients,
                ownerExternalUserId,
                enrollmentPatientProfileId.Value.ToString());
        }

        var selfPatient = ResolvePersonalPatient(patients, ownerExternalUserId, null);
        if (!string.IsNullOrWhiteSpace(requestedPatientProfileId) &&
            ParsePatientProfileId(requestedPatientProfileId) != selfPatient.Id)
        {
            throw Oops.Oh("旧报名记录只能绑定本人患者材料").StatusCode(409);
        }

        return selfPatient;
    }

    private static long ParsePatientProfileId(string value)
    {
        if (!long.TryParse(value.Trim(), out var patientProfileId) || patientProfileId <= 0)
            throw Oops.Oh("患者档案 Id 格式不正确").StatusCode(400);

        return patientProfileId;
    }

    private static string NormalizeOwner(string ownerExternalUserId)
    {
        var owner = ownerExternalUserId?.Trim();
        if (string.IsNullOrWhiteSpace(owner))
            throw Oops.Oh("小程序登录态无效，请重新登录").StatusCode(400);

        return owner;
    }
}
