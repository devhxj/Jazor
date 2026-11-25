namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 报名记录服务
/// </summary>
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 110)]
public class SaveLifeEnrollmentRecordService : IDynamicApiController, ITransient
{
    private const string EnrollmentPagePermission = "saveLifeEnrollmentRecord:page";
    private const string EnrollmentUpdatePermission = "saveLifeEnrollmentRecord:update";
    private const string EnrollmentAuditPermission = "saveLifeEnrollmentRecord:audit";
    private const string EnrollmentDeletePermission = "saveLifeEnrollmentRecord:delete";
    private const string EnrollmentContactPermission = "saveLifeEnrollmentRecord:contact";

    private readonly SqlSugarRepository<SaveLifeEnrollmentRecord> _recordRep;
    private readonly SqlSugarRepository<SaveLifeClinicalTrial> _trialRep;
    private readonly SqlSugarRepository<SaveLifeUserProfile> _profileRep;
    private readonly SqlSugarRepository<SaveLifePatientProfile> _patientProfileRep;
    private readonly SqlSugarRepository<SaveLifeEnrollmentMaterial> _materialRep;
    private readonly SqlSugarRepository<SaveLifeNotification> _notificationRep;
    private readonly ISaveLifePatientEnrollmentService _patientEnrollmentService;
    private readonly ISaveLifePrivacyConsentGuard _privacyConsentGuard;
    private readonly ISaveLifeFileAccessService _fileAccessService;
    private readonly UserManager _userManager;
    private readonly SysMenuService _sysMenuService;
    private readonly ISaveLifeEnrollmentAuditScopeAccessService? _auditScopeAccessService;

    public SaveLifeEnrollmentRecordService(
        SqlSugarRepository<SaveLifeEnrollmentRecord> recordRep,
        SqlSugarRepository<SaveLifeClinicalTrial> trialRep,
        SqlSugarRepository<SaveLifeUserProfile> profileRep,
        SqlSugarRepository<SaveLifePatientProfile> patientProfileRep,
        SqlSugarRepository<SaveLifeEnrollmentMaterial> materialRep,
        SqlSugarRepository<SaveLifeNotification> notificationRep,
        ISaveLifePatientEnrollmentService patientEnrollmentService,
        ISaveLifePrivacyConsentGuard privacyConsentGuard,
        ISaveLifeFileAccessService fileAccessService,
        UserManager userManager,
        SysMenuService sysMenuService,
        ISaveLifeEnrollmentAuditScopeAccessService? auditScopeAccessService = null)
    {
        _recordRep = recordRep;
        _trialRep = trialRep;
        _profileRep = profileRep;
        _patientProfileRep = patientProfileRep;
        _materialRep = materialRep;
        _notificationRep = notificationRep;
        _patientEnrollmentService = patientEnrollmentService;
        _privacyConsentGuard = privacyConsentGuard;
        _fileAccessService = fileAccessService;
        _userManager = userManager;
        _sysMenuService = sysMenuService;
        _auditScopeAccessService = auditScopeAccessService;
    }

    /// <summary>
    /// 分页查询报名记录
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<SaveLifeEnrollmentRecordOutput>> Page(SaveLifeEnrollmentRecordInput input)
    {
        await EnsureEnrollmentPermission(EnrollmentPagePermission, "无权查看报名记录");

        var keyword = input.Keyword?.Trim();
        var cdeKeyword = SaveLifeCdeRegistrationNumber.Normalize(keyword);
        var hasCdeKeyword = !string.IsNullOrWhiteSpace(cdeKeyword);
        var projectStatus = input.ProjectStatus?.Trim();
        var diseaseArea = input.DiseaseArea?.Trim();
        var clinicalPhase = input.ClinicalPhase?.Trim();
        var auditStatus = string.IsNullOrWhiteSpace(input.AuditStatus)
            ? input.Status?.Trim()
            : input.AuditStatus.Trim();
        var auditScope = await GetCurrentAuditScope();
        var scopedDiseaseAreas = auditScope.DiseaseAreas.ToList();
        var recordsQuery = _recordRep.AsQueryable();
        if (!auditScope.IsUnrestricted)
        {
            recordsQuery = scopedDiseaseAreas.Count == 0
                ? recordsQuery.Where(u => u.Id == 0)
                : recordsQuery.Where(u =>
                    SqlFunc.Subqueryable<SaveLifeClinicalTrial>()
                        .Where(trial =>
                            trial.Id == u.ProjectId &&
                            scopedDiseaseAreas.Contains(trial.DiseaseArea!))
                        .Any());
        }

        var page = await recordsQuery
            .WhereIF(input.ProjectId > 0, u => u.ProjectId == input.ProjectId)
            .WhereIF(!string.IsNullOrWhiteSpace(auditStatus), u => u.Status == auditStatus)
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), u =>
                SqlFunc.Subqueryable<SaveLifeClinicalTrial>()
                    .Where(trial => trial.Id == u.ProjectId)
                    .Where(trial =>
                        (trial.Title != null && trial.Title.Contains(keyword!)) ||
                        (trial.Sponsor != null && trial.Sponsor.Contains(keyword!)) ||
                        (hasCdeKeyword && trial.CdeRegistrationNumber.Contains(cdeKeyword)) ||
                        (trial.Location != null && trial.Location.Contains(keyword!)))
                    .Any())
            .WhereIF(!string.IsNullOrWhiteSpace(projectStatus), u =>
                SqlFunc.Subqueryable<SaveLifeClinicalTrial>()
                    .Where(trial => trial.Id == u.ProjectId && trial.Status == projectStatus)
                    .Any())
            .WhereIF(!string.IsNullOrWhiteSpace(diseaseArea), u =>
                SqlFunc.Subqueryable<SaveLifeClinicalTrial>()
                    .Where(trial => trial.Id == u.ProjectId && trial.DiseaseArea == diseaseArea)
                    .Any())
            .WhereIF(!string.IsNullOrWhiteSpace(clinicalPhase), u =>
                SqlFunc.Subqueryable<SaveLifeClinicalTrial>()
                    .Where(trial => trial.Id == u.ProjectId && trial.ClinicalPhase == clinicalPhase)
                    .Any())
            .OrderBy(u => u.SubmittedTime, OrderByType.Desc)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);

        var records = page.Items.ToList();
        return new SqlSugarPagedList<SaveLifeEnrollmentRecordOutput>
        {
            Page = page.Page,
            PageSize = page.PageSize,
            Total = page.Total,
            TotalPages = page.TotalPages,
            HasNextPage = page.HasNextPage,
            HasPrevPage = page.HasPrevPage,
            Items = await ToEnrollmentRecordOutputs(records)
        };
    }

    /// <summary>
    /// 新增报名记录
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task<long> Add(AddSaveLifeEnrollmentRecordInput input)
    {
        await EnsureCanAccessProject(input.ProjectId);
        var entity = input.Adapt<SaveLifeEnrollmentRecord>();
        entity.SubmittedTime ??= DateTime.Now;
        await _recordRep.InsertAsync(entity);
        return entity.Id;
    }

    /// <summary>
    /// 更新报名记录
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateSaveLifeEnrollmentRecordInput input)
    {
        await EnsureEnrollmentPermission(EnrollmentUpdatePermission, "无权编辑报名记录");
        var current = await GetEnrollmentRecord(input.Id);
        if (current.ProjectId != input.ProjectId)
            await EnsureCanAccessProject(input.ProjectId);

        await _recordRep.AsUpdateable(input.Adapt<SaveLifeEnrollmentRecord>()).IgnoreColumns(true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除报名记录
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteSaveLifeEnrollmentRecordInput input)
    {
        await EnsureEnrollmentPermission(EnrollmentDeletePermission, "无权删除报名记录");
        var entity = await GetEnrollmentRecord(input.Id);
        await _recordRep.FakeDeleteAsync(entity);
    }

    /// <summary>
    /// 审核报名记录
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Audit")]
    public async Task Audit(AuditSaveLifeEnrollmentRecordInput input)
    {
        await EnsureEnrollmentPermission(EnrollmentAuditPermission, "无权审核报名记录");
        var record = await GetEnrollmentRecord(input.Id);
        var nextStatus = SaveLifeEnrollmentStatusPolicy.Normalize(input.Status);
        if (!SaveLifeEnrollmentStatusPolicy.CanAudit(record.Status, nextStatus))
        {
            throw Oops.Oh($"当前状态“{record.Status}”不能审核为“{nextStatus}”").StatusCode(409);
        }

        record.Status = nextStatus;
        record.AuditRemark = SaveLifeEnrollmentMaterialHelper.NormalizeText(input.AuditRemark, 512, "审核备注");
        record.AuditTime = DateTime.Now;
        await _recordRep.AsUpdateable(record).IgnoreColumns(true).ExecuteCommandAsync();

        await AddEnrollmentNotification(record);
    }

    /// <summary>
    /// 标记已联系。
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Contact")]
    public async Task Contact(ContactSaveLifeEnrollmentRecordInput input)
    {
        await EnsureEnrollmentPermission(EnrollmentContactPermission, "无权处理报名联系记录");
        var record = await GetEnrollmentRecord(input.Id);
        if (!SaveLifeEnrollmentStatusPolicy.CanContact(record.Status))
        {
            throw Oops.Oh($"当前状态“{record.Status}”不能标记为“{SaveLifeEnrollmentStatusPolicy.Contacted}”")
                .StatusCode(409);
        }

        var contactRemark = SaveLifeEnrollmentMaterialHelper.NormalizeText(input.ContactRemark, 512, "联系备注");
        record.Status = SaveLifeEnrollmentStatusPolicy.Contacted;
        if (!string.IsNullOrWhiteSpace(contactRemark))
            record.AuditRemark = contactRemark;
        record.AuditTime = DateTime.Now;
        await _recordRep.AsUpdateable(record).IgnoreColumns(true).ExecuteCommandAsync();

        await AddEnrollmentNotification(record);
    }

    /// <summary>
    /// 后台发起补充资料通知
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "RequestSupplement")]
    public async Task RequestSupplement(RequestSupplementSaveLifeEnrollmentRecordInput input)
    {
        await EnsureEnrollmentPermission(EnrollmentAuditPermission, "无权发起补充资料请求");
        var record = await GetEnrollmentRecord(input.Id);
        var remark = SaveLifeEnrollmentMaterialHelper.NormalizeText(input.SupplementRequest, 512, "补充资料要求")
            ?? throw Oops.Oh("补充资料要求不能为空").StatusCode(400);

        await _recordRep.AsUpdateable()
            .SetColumns(u => new SaveLifeEnrollmentRecord
            {
                Status = "need_supplement",
                AuditRemark = remark,
                AuditTime = DateTime.Now
            })
            .Where(u => u.Id == input.Id)
            .ExecuteCommandAsync();

        record.Status = "need_supplement";
        record.AuditRemark = remark;
        record.AuditTime = DateTime.Now;
        await AddNotification(
            record.ExternalUserId,
            "material",
            "请补充报名材料",
            $"您报名的项目“{record.ProjectTitle ?? $"项目 {record.ProjectId}"}”需要补充材料：{remark}",
            "enrollment",
            record.Id);
    }

    /// <summary>
    /// 获取报名记录详情
    /// </summary>
    public async Task<SaveLifeEnrollmentRecordOutput> GetDetail([FromQuery] QueryByIdSaveLifeEnrollmentRecordInput input)
    {
        var record = await GetEnrollmentRecord(input.Id);
        return await ToEnrollmentRecordOutput(record);
    }

    /// <summary>
    /// 获取后台报名详情及患者上传资料
    /// </summary>
    [Authorize]
    [HttpGet("/api/saveLifeEnrollmentRecord/{id}/detailWithMaterials")]
    [ApiDescriptionSettings(Name = "DetailWithMaterials")]
    public async Task<SaveLifeEnrollmentRecordDetailOutput> GetDetailWithMaterials(long id)
    {
        await EnsureCanViewEnrollmentMaterials();

        var record = await GetEnrollmentRecord(id);
        var materials = await LoadEnrollmentMaterials(record);
        return new SaveLifeEnrollmentRecordDetailOutput
        {
            Record = await ToEnrollmentRecordOutput(record),
            Materials = materials
                .Select(material => SaveLifeEnrollmentMaterialService.ToOutput(material, canDelete: false))
                .ToList()
        };
    }

    /// <summary>
    /// 下载后台报名详情中的单个上传资料
    /// </summary>
    [Authorize]
    [HttpGet("/api/saveLifeEnrollmentRecord/{enrollmentId}/materials/{materialId}/download"), NonUnify]
    [ApiDescriptionSettings(Name = "DownloadMaterial")]
    [SuppressMonitor]
    public async Task<IActionResult> DownloadMaterial(long enrollmentId, long materialId)
    {
        await EnsureCanViewEnrollmentMaterials();

        var record = await GetEnrollmentRecord(enrollmentId);
        var material = await GetEnrollmentMaterial(record, materialId);
        return await _fileAccessService.DownloadAsync(material.FileId);
    }

    /// <summary>
    /// 打包下载后台报名详情中的上传资料
    /// </summary>
    [Authorize]
    [HttpGet("/api/saveLifeEnrollmentRecord/{enrollmentId}/materials/download"), NonUnify]
    [ApiDescriptionSettings(Name = "DownloadMaterialsArchive")]
    [SuppressMonitor]
    public async Task<IActionResult> DownloadMaterialsArchive(long enrollmentId)
    {
        await EnsureCanViewEnrollmentMaterials();

        var record = await GetEnrollmentRecord(enrollmentId);
        var materials = await LoadEnrollmentMaterials(record);
        if (materials.Count == 0)
            throw Oops.Oh("当前报名暂无可下载资料").StatusCode(404);

        return new SaveLifeEnrollmentMaterialArchiveResult(
            materials,
            _fileAccessService,
            $"save-life-enrollment-{record.Id}-materials.zip");
    }

    /// <summary>
    /// 小程序查询用户报名记录
    /// </summary>
    [Authorize]
    [HttpGet("/api/v1/save-life/enrollments"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppList")]
    public async Task<SqlSugarPagedList<SaveLifeMiniAppEnrollmentOutput>> MiniAppList([FromQuery] SaveLifeEnrollmentRecordInput input)
    {
        var externalUserId = GetCurrentMiniAppOpenId();
        input.PageSize = NormalizePageSize(input.PageSize);

        var page = await _recordRep.AsQueryable()
            .Where(u => u.ExternalUserId == externalUserId)
            .WhereIF(input.ProjectId > 0, u => u.ProjectId == input.ProjectId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Status), u => u.Status == input.Status)
            .OrderBy(u => u.SubmittedTime, OrderByType.Desc)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);

        var records = page.Items?.ToList() ?? new List<SaveLifeEnrollmentRecord>();
        var outputs = await ToMiniAppEnrollmentOutputs(records);
        return new SqlSugarPagedList<SaveLifeMiniAppEnrollmentOutput>
        {
            Page = page.Page,
            PageSize = page.PageSize,
            Total = page.Total,
            TotalPages = page.TotalPages,
            HasNextPage = page.HasNextPage,
            HasPrevPage = page.HasPrevPage,
            Items = outputs
        };
    }

    /// <summary>
    /// 小程序查询当前用户报名状态数量
    /// </summary>
    [Authorize]
    [HttpGet("/api/v1/save-life/enrollments/status-counts"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppStatusCounts")]
    public async Task<SaveLifeMiniAppEnrollmentStatusCountsOutput> MiniAppStatusCounts()
    {
        var externalUserId = GetCurrentMiniAppOpenId();
        return await SaveLifeEnrollmentStatusCounts.QueryAsync(
            _recordRep.AsQueryable()
                .Where(u => u.ExternalUserId == externalUserId));
    }

    /// <summary>
    /// 小程序获取报名详情
    /// </summary>
    [Authorize]
    [HttpGet("/api/v1/save-life/enrollments/{id}"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppDetail")]
    public async Task<SaveLifeMiniAppEnrollmentOutput> MiniAppDetail(long id)
    {
        var externalUserId = GetCurrentMiniAppOpenId();
        var record = await _recordRep.GetFirstAsync(u => u.Id == id && u.ExternalUserId == externalUserId)
            ?? throw Oops.Oh("报名记录不存在或不属于当前用户").StatusCode(404);

        return await ToMiniAppEnrollmentOutput(record, includeMaterials: true);
    }

    /// <summary>
    /// 小程序提交报名意向
    /// </summary>
    [Authorize]
    [HttpPost("/api/v1/save-life/enrollments"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppSubmit")]
    public async Task<SaveLifeMiniAppEnrollmentOutput> MiniAppSubmit(SubmitSaveLifeMiniAppEnrollmentInput input)
    {
        var externalUserId = GetCurrentMiniAppOpenId();
        await _privacyConsentGuard.EnsureConsentedAsync(externalUserId);
        await _privacyConsentGuard.EnsureSensitiveInformationConsentedAsync(externalUserId);
        var patientProfileId = await _patientEnrollmentService.ResolvePatientProfileIdAsync(
            externalUserId,
            input.PatientProfileId,
            allowSelfFallback: true);
        var result = await _patientEnrollmentService.SubmitConfirmedAsync(
            new SaveLifePatientEnrollmentSubmissionInput
            {
                ExternalUserId = externalUserId,
                PatientProfileId = patientProfileId,
                ProjectId = input.ProjectId,
                Fields = BuildMiniAppSubmissionFields(input)
            });
        if (SaveLifeEnrollmentValidation.IsBlockingExistingSubmission(result.Enrollment))
        {
            throw Oops.Oh("您已提交过该项目报名，请在我的申请中查看进度或补充材料")
                .StatusCode(409);
        }

        return await ToMiniAppEnrollmentOutput(result.Enrollment);
    }

    /// <summary>
    /// 小程序取消报名
    /// </summary>
    [Authorize]
    [HttpPost("/api/v1/save-life/enrollments/{id}/cancel"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppCancel")]
    public async Task<SaveLifeMiniAppEnrollmentOutput> MiniAppCancel(long id, [FromBody] CancelSaveLifeMiniAppEnrollmentInput? input = null)
    {
        var externalUserId = GetCurrentMiniAppOpenId();
        var record = await _recordRep.GetFirstAsync(u => u.Id == id && u.ExternalUserId == externalUserId)
            ?? throw Oops.Oh("报名记录不存在或不属于当前用户").StatusCode(404);

        record.Status = "cancelled";
        record.AuditRemark = "用户在小程序取消报名";
        record.AuditTime = DateTime.Now;
        await _recordRep.AsUpdateable(record).IgnoreColumns(true).ExecuteCommandAsync();

        return await ToMiniAppEnrollmentOutput(record);
    }

    private static SaveLifeEnrollmentSubmissionFields BuildMiniAppSubmissionFields(
        SubmitSaveLifeMiniAppEnrollmentInput input)
    {
        return new SaveLifeEnrollmentSubmissionFields
        {
            ContactName = input.ContactName,
            ContactPhone = input.ContactPhone,
            DiseaseDescription = input.DiseaseDescription,
            MedicalHistory = input.MedicalHistory,
            DiagnosisReport = input.DiagnosisReport,
            MedicationStatus = input.MedicationStatus,
            OtherInfo = input.OtherInfo,
            Notes = input.Notes
        };
    }

    private async Task<SaveLifeEnrollmentRecord> GetEnrollmentRecord(long id)
    {
        await EnsureEnrollmentPermission(EnrollmentPagePermission, "无权查看报名记录");
        var record = await _recordRep.GetByIdAsync(id)
            ?? throw Oops.Oh("报名记录不存在").StatusCode(404);
        await EnsureCanAccessProject(record.ProjectId);
        return record;
    }

    private async Task<SaveLifeEnrollmentAuditAccessScope> GetCurrentAuditScope()
    {
        return _auditScopeAccessService == null
            ? SaveLifeEnrollmentAuditAccessScope.Unrestricted
            : await _auditScopeAccessService.GetCurrentAccessScopeAsync();
    }

    private async Task EnsureCanAccessProject(long projectId)
    {
        var auditScope = await GetCurrentAuditScope();
        if (auditScope.IsUnrestricted)
            return;

        var project = await _trialRep.GetByIdAsync(projectId);
        if (project != null && auditScope.Includes(project.DiseaseArea))
            return;

        throw Oops.Oh("无权查看或处理该项目领域的报名记录").StatusCode(403);
    }

    private async Task EnsureEnrollmentPermission(string permission, string message)
    {
        // Direct service tests construct this class without the web authorization services.
        if (_auditScopeAccessService == null || _sysMenuService == null)
            return;

        var loginMode = App.User?.FindFirst(ClaimConst.LoginMode)?.Value;
        if (loginMode == ((int)LoginModeEnum.APP).ToString() ||
            loginMode?.Equals(nameof(LoginModeEnum.APP), StringComparison.OrdinalIgnoreCase) == true)
        {
            throw Oops.Oh("小程序账号无权访问后台报名记录").StatusCode(403);
        }

        if (_userManager.SuperAdmin || _userManager.SysAdmin)
            return;

        var permissions = await _sysMenuService.GetOwnBtnPermList();
        if (permissions.Any(current => current.Equals(permission, StringComparison.OrdinalIgnoreCase)))
            return;

        throw Oops.Oh(message).StatusCode(403);
    }

    private async Task<SaveLifeEnrollmentRecordOutput> ToEnrollmentRecordOutput(
        SaveLifeEnrollmentRecord record)
    {
        return (await ToEnrollmentRecordOutputs(new[] { record })).Single();
    }

    private async Task<List<SaveLifeEnrollmentRecordOutput>> ToEnrollmentRecordOutputs(
        IReadOnlyCollection<SaveLifeEnrollmentRecord> records)
    {
        if (records.Count == 0)
            return new List<SaveLifeEnrollmentRecordOutput>();

        var userProfileIds = records
            .Where(record => record.UserProfileId.HasValue)
            .Select(record => record.UserProfileId!.Value)
            .Distinct()
            .ToList();
        var externalUserIds = records
            .Select(record => record.ExternalUserId?.Trim())
            .Where(externalUserId => !string.IsNullOrWhiteSpace(externalUserId))
            .Select(externalUserId => externalUserId!)
            .Distinct()
            .ToList();
        var profiles = userProfileIds.Count == 0 && externalUserIds.Count == 0
            ? new List<SaveLifeUserProfile>()
            : await _profileRep.AsQueryable()
                .Where(profile =>
                    userProfileIds.Contains(profile.Id) ||
                    (profile.ExternalUserId != null && externalUserIds.Contains(profile.ExternalUserId)))
                .ToListAsync();

        var patientProfileIds = records
            .Where(record => record.PatientProfileId.HasValue)
            .Select(record => record.PatientProfileId!.Value)
            .Distinct()
            .ToList();
        var patientProfiles = patientProfileIds.Count == 0 && externalUserIds.Count == 0
            ? new List<SaveLifePatientProfile>()
            : await _patientProfileRep.AsQueryable()
                .Where(patient =>
                    patientProfileIds.Contains(patient.Id) ||
                    externalUserIds.Contains(patient.OwnerExternalUserId))
                .ToListAsync();
        var projectIds = records
            .Select(record => record.ProjectId)
            .Distinct()
            .ToList();
        var projects = projectIds.Count == 0
            ? new List<SaveLifeClinicalTrial>()
            : await _trialRep.AsQueryable()
                .Where(project => projectIds.Contains(project.Id))
                .Select(project => new SaveLifeClinicalTrial
                {
                    Id = project.Id,
                    CdeRegistrationNumber = project.CdeRegistrationNumber,
                    ClinicalPhase = project.ClinicalPhase,
                    DiseaseArea = project.DiseaseArea,
                    Title = project.Title
                })
                .ToListAsync();
        var projectsById = projects.ToDictionary(project => project.Id);

        return records
            .Select(record =>
            {
                var externalUserId = record.ExternalUserId?.Trim();
                var profile = profiles.FirstOrDefault(item =>
                    record.UserProfileId.HasValue && item.Id == record.UserProfileId.Value)
                    ?? profiles.FirstOrDefault(item =>
                        !string.IsNullOrWhiteSpace(externalUserId) &&
                        item.ExternalUserId == externalUserId);
                var patientProfile = patientProfiles.FirstOrDefault(item =>
                    record.PatientProfileId.HasValue && item.Id == record.PatientProfileId.Value &&
                    (string.IsNullOrWhiteSpace(externalUserId) ||
                     item.OwnerExternalUserId == externalUserId))
                    ?? (!record.PatientProfileId.HasValue
                        ? patientProfiles.FirstOrDefault(item =>
                            item.OwnerExternalUserId == externalUserId && item.IsDefault)
                        : null);
                var project = projectsById.TryGetValue(record.ProjectId, out var matchedProject)
                    ? matchedProject
                    : null;

                var output = record.Adapt<SaveLifeEnrollmentRecordOutput>();
                output.ProjectCdeRegistrationNumber = project?.CdeRegistrationNumber;
                output.ProjectDiseaseArea = project?.DiseaseArea;
                output.ProjectClinicalPhase = project?.ClinicalPhase;
                output.ProjectTitle = project?.Title ?? output.ProjectTitle;
                output.UserProfile = profile?.Adapt<SaveLifeEnrollmentUserProfileOutput>();
                output.PatientProfile = patientProfile?.Adapt<SaveLifeEnrollmentPatientProfileOutput>();
                return output;
            })
            .ToList();
    }

    private async Task<SaveLifeEnrollmentMaterial> GetEnrollmentMaterial(
        SaveLifeEnrollmentRecord record,
        long materialId)
    {
        var material = await _materialRep.AsQueryable()
            .Where(material => material.Id == materialId)
            .FirstAsync()
            ?? throw Oops.Oh("资料不存在或不属于当前报名").StatusCode(404);

        if (!IsMaterialInEnrollmentScope(material, record))
            throw Oops.Oh("资料不存在或不属于当前报名").StatusCode(404);

        return material;
    }

    private async Task<List<SaveLifeEnrollmentMaterial>> LoadEnrollmentMaterials(
        SaveLifeEnrollmentRecord record)
    {
        var externalUserId = record.ExternalUserId?.Trim();
        if (!string.IsNullOrWhiteSpace(externalUserId))
        {
            return await _materialRep.AsQueryable()
                .Where(material =>
                    material.ExternalUserId == externalUserId &&
                    material.PatientProfileId == record.PatientProfileId &&
                    (material.EnrollmentRecordId <= 0 || material.EnrollmentRecordId == record.Id))
                .OrderBy(material => material.CreateTime, OrderByType.Desc)
                .Take(SaveLifeEnrollmentMaterialHelper.MaxMaterialListCount)
                .ToListAsync();
        }

        return await _materialRep.AsQueryable()
            .Where(material => material.EnrollmentRecordId == record.Id)
            .OrderBy(material => material.CreateTime, OrderByType.Desc)
            .Take(SaveLifeEnrollmentMaterialHelper.MaxMaterialListCount)
            .ToListAsync();
    }

    private static bool IsMaterialInEnrollmentScope(
        SaveLifeEnrollmentMaterial material,
        SaveLifeEnrollmentRecord record)
    {
        var externalUserId = record.ExternalUserId?.Trim();
        if (!string.IsNullOrWhiteSpace(externalUserId))
        {
            return material.ExternalUserId == externalUserId &&
                material.PatientProfileId == record.PatientProfileId &&
                (material.EnrollmentRecordId <= 0 || material.EnrollmentRecordId == record.Id);
        }

        return material.EnrollmentRecordId == record.Id;
    }

    private async Task EnsureCanViewEnrollmentMaterials()
    {
        var loginMode = App.User?.FindFirst(ClaimConst.LoginMode)?.Value;
        if (loginMode == ((int)LoginModeEnum.APP).ToString() ||
            loginMode?.Equals(nameof(LoginModeEnum.APP), StringComparison.OrdinalIgnoreCase) == true)
        {
            throw Oops.Oh("小程序账号无权下载后台报名资料").StatusCode(403);
        }

        if (_userManager.SuperAdmin || _userManager.SysAdmin)
            return;

        if (_auditScopeAccessService == null || _sysMenuService == null)
            return;

        var permissions = await _sysMenuService.GetOwnBtnPermList();
        if (permissions.Any(permission => permission.Equals(EnrollmentPagePermission, StringComparison.OrdinalIgnoreCase)))
            return;

        throw Oops.Oh("无权查看报名资料").StatusCode(403);
    }

    private async Task<List<SaveLifeMiniAppEnrollmentOutput>> ToMiniAppEnrollmentOutputs(List<SaveLifeEnrollmentRecord> records)
    {
        if (records.Count == 0)
            return new List<SaveLifeMiniAppEnrollmentOutput>();

        var externalUserIds = records
            .Select(u => u.ExternalUserId)
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Distinct()
            .ToList();

        var materials = await _materialRep.AsQueryable()
            .Where(u => externalUserIds.Contains(u.ExternalUserId))
            .ToListAsync();
        var profiles = await _profileRep.AsQueryable()
            .Where(u => externalUserIds.Contains(u.ExternalUserId))
            .ToListAsync();
        var patientProfileIds = records
            .Where(u => u.PatientProfileId.HasValue)
            .Select(u => u.PatientProfileId!.Value)
            .Distinct()
            .ToList();
        var patientProfiles = patientProfileIds.Count == 0
            ? new List<SaveLifePatientProfile>()
            : await _patientProfileRep.AsQueryable()
                .Where(u => patientProfileIds.Contains(u.Id))
                .ToListAsync();
        var projectIds = records.Select(u => u.ProjectId).Distinct().ToList();
        var trials = await _trialRep.AsQueryable()
            .Where(u => projectIds.Contains(u.Id))
            .Select(u => new SaveLifeClinicalTrial { Id = u.Id, Sites = u.Sites })
            .ToListAsync();

        return records
            .Select(record => ToMiniAppEnrollmentOutput(
                record,
                profiles.FirstOrDefault(u => u.ExternalUserId == record.ExternalUserId),
                patientProfiles.FirstOrDefault(u =>
                    u.Id == record.PatientProfileId &&
                    u.OwnerExternalUserId == record.ExternalUserId),
                materials
                    .Where(u =>
                        u.PatientProfileId == record.PatientProfileId &&
                        SaveLifeEnrollmentMaterialHelper.IsEffectiveForEnrollment(
                            u,
                            record.ExternalUserId ?? string.Empty,
                            record.Id))
                    .ToList(),
                trials.FirstOrDefault(u => u.Id == record.ProjectId)?.Sites))
            .ToList();
    }

    private async Task<SaveLifeMiniAppEnrollmentOutput> ToMiniAppEnrollmentOutput(
        SaveLifeEnrollmentRecord record,
        bool includeMaterials = false)
    {
        var profile = !string.IsNullOrWhiteSpace(record.ExternalUserId)
            ? await _profileRep.GetFirstAsync(u => u.ExternalUserId == record.ExternalUserId)
            : null;
        var patientProfile = record.PatientProfileId.HasValue
            ? await _patientProfileRep.GetFirstAsync(u =>
                u.Id == record.PatientProfileId.Value &&
                u.OwnerExternalUserId == record.ExternalUserId)
            : null;
        var materials = !string.IsNullOrWhiteSpace(record.ExternalUserId)
            ? await _materialRep.AsQueryable()
                .Where(u =>
                    u.ExternalUserId == record.ExternalUserId &&
                    u.PatientProfileId == record.PatientProfileId &&
                    (u.EnrollmentRecordId <= 0 || u.EnrollmentRecordId == record.Id))
                .OrderBy(u => u.CreateTime, OrderByType.Desc)
                .Take(SaveLifeEnrollmentMaterialHelper.MaxMaterialListCount)
                .ToListAsync()
            : await _materialRep.AsQueryable()
                .Where(u => u.EnrollmentRecordId == record.Id)
                .OrderBy(u => u.CreateTime, OrderByType.Desc)
                .Take(SaveLifeEnrollmentMaterialHelper.MaxMaterialListCount)
                .ToListAsync();

        var trial = await _trialRep.GetFirstAsync(u => u.Id == record.ProjectId);
        var output = ToMiniAppEnrollmentOutput(record, profile, patientProfile, materials, trial?.Sites);
        if (includeMaterials)
        {
            output.Materials = materials
                .Select(material => SaveLifeEnrollmentMaterialService.ToOutput(
                    material,
                    CanDeleteMaterial(record, material)))
                .ToList();
        }

        return output;
    }

    private static bool CanDeleteMaterial(
        SaveLifeEnrollmentRecord record,
        SaveLifeEnrollmentMaterial material)
    {
        if (SaveLifeEnrollmentMaterialHelper.IsPersonalMaterial(material))
            return SaveLifeEnrollmentMaterialHelper.CanDeletePersonalMaterial(material);

        if (record.Status is "approved" or "cancelled" or "closed")
            return false;

        return material.Status is "submitted" or "need_supplement" or "rejected";
    }

    private static SaveLifeMiniAppEnrollmentOutput ToMiniAppEnrollmentOutput(
        SaveLifeEnrollmentRecord record,
        SaveLifeUserProfile? profile,
        SaveLifePatientProfile? patientProfile,
        List<SaveLifeEnrollmentMaterial> materials,
        string? sites = null)
    {
        var materialRequirements = BuildMaterialRequirements(materials);
        var profileValidation = SaveLifeEnrollmentValidation.ValidatePatientForSubmission(
            profile,
            patientProfile,
            BuildSubmissionFields(record));
        var missingFields = profileValidation.MissingFields;
        var completeness = CalculatePatientEnrollmentCompleteness(patientProfile, missingFields);
        var hasRequiredMaterials = materialRequirements
            .Where(u => u.Required)
            .All(u => u.Uploaded);

        return new SaveLifeMiniAppEnrollmentOutput
        {
            Id = record.Id.ToString(),
            ExternalUserId = record.ExternalUserId,
            PatientProfileId = record.PatientProfileId?.ToString(),
            ProjectId = record.ProjectId.ToString(),
            ProjectTitle = record.ProjectTitle,
            InstitutionName = SaveLifeTrialSiteFormatter.FormatInstitutionNames(sites),
            ContactName = record.ContactName,
            ContactPhone = record.ContactPhone,
            Status = record.Status,
            SubmittedTime = record.SubmittedTime,
            AuditTime = record.AuditTime,
            AuditRemark = record.AuditRemark,
            DiseaseDescription = record.DiseaseDescription,
            MedicalHistory = record.MedicalHistory,
            DiagnosisReport = record.DiagnosisReport,
            MedicationStatus = record.MedicationStatus,
            OtherInfo = record.OtherInfo,
            Notes = record.Notes,
            ProfileCompleteness = completeness,
            HasRequiredProfile = missingFields.Count == 0,
            MissingProfileFields = missingFields,
            MaterialCount = materials.Count,
            HasRequiredMaterials = hasRequiredMaterials,
            MaterialRequirements = materialRequirements,
            NextStepHint = BuildNextStepHint(record, missingFields, hasRequiredMaterials),
            CreateTime = record.CreateTime,
            UpdateTime = record.UpdateTime
        };
    }

    private static SaveLifeEnrollmentSubmissionFields BuildSubmissionFields(
        SaveLifeEnrollmentRecord record)
    {
        return new SaveLifeEnrollmentSubmissionFields
        {
            ContactName = record.ContactName,
            ContactPhone = record.ContactPhone,
            DiseaseDescription = record.DiseaseDescription,
            MedicalHistory = record.MedicalHistory,
            DiagnosisReport = record.DiagnosisReport,
            MedicationStatus = record.MedicationStatus,
            OtherInfo = record.OtherInfo,
            Notes = record.Notes
        };
    }

    private static int CalculatePatientEnrollmentCompleteness(
        SaveLifePatientProfile? patientProfile,
        IReadOnlyCollection<string> missingFields)
    {
        if (patientProfile == null)
            return 0;

        const int requiredFieldCount = 7;
        var completedFieldCount = Math.Max(0, requiredFieldCount - missingFields.Count);
        return (int)Math.Round(completedFieldCount * 100D / requiredFieldCount);
    }

    private static int NormalizePageSize(int pageSize)
    {
        if (pageSize <= 0)
            return 10;

        return Math.Min(pageSize, 50);
    }

    private static string GetCurrentMiniAppOpenId()
    {
        var openId = App.User?.FindFirst(ClaimConst.OpenId)?.Value?.Trim();
        if (!string.IsNullOrWhiteSpace(openId))
            return openId;

        throw Oops.Oh("小程序登录态无效，请重新登录").StatusCode(400);
    }

    private static List<SaveLifeMiniAppMaterialRequirementOutput> BuildMaterialRequirements(
        List<SaveLifeEnrollmentMaterial> materials)
    {
        var activeTypes = materials
            .Where(u => u.Status is "submitted" or "approved")
            .Select(u => u.MaterialType)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return new List<SaveLifeMiniAppMaterialRequirementOutput>
        {
            new()
            {
                MaterialType = "medical_record",
                MaterialTypeText = SaveLifeEnrollmentMaterialHelper.GetMaterialTypeText("medical_record"),
                Required = true,
                Uploaded = activeTypes.Contains("medical_record")
            },
            new()
            {
                MaterialType = "diagnosis_report",
                MaterialTypeText = SaveLifeEnrollmentMaterialHelper.GetMaterialTypeText("diagnosis_report"),
                Required = false,
                Uploaded = activeTypes.Contains("diagnosis_report")
            },
            new()
            {
                MaterialType = "lab_report",
                MaterialTypeText = SaveLifeEnrollmentMaterialHelper.GetMaterialTypeText("lab_report"),
                Required = false,
                Uploaded = activeTypes.Contains("lab_report")
            }
        };
    }

    private static string BuildNextStepHint(
        SaveLifeEnrollmentRecord record,
        List<string> missingFields,
        bool hasRequiredMaterials)
    {
        if (missingFields.Count > 0)
            return $"请补充资料：{string.Join("、", missingFields)}";

        if (record.Status == "need_supplement")
            return record.AuditRemark ?? "请按机构要求补充资料";

        if (!hasRequiredMaterials && record.Status is "pending" or "draft")
            return "建议上传病例资料，便于机构进行初步筛查";

        return record.Status switch
        {
            "pending" => "报名已提交，请等待机构审核",
            "approved" => "报名已通过，请留意机构联系",
            "contacted" => "机构已联系，请留意来电和后续安排",
            "rejected" => "报名未通过，可查看审核备注",
            "cancelled" => "报名已取消",
            "need_supplement" => record.AuditRemark ?? "请补充报名材料",
            _ => "可在申请详情查看后续进度"
        };
    }

    private async Task AddEnrollmentNotification(SaveLifeEnrollmentRecord record)
    {
        var statusText = record.Status switch
        {
            "approved" => "报名审核已通过",
            "rejected" => "报名审核未通过",
            "contacted" => "机构已联系",
            "need_supplement" => "请补充报名材料",
            _ => "报名状态已更新"
        };
        var content = record.Status switch
        {
            "approved" => $"您报名的项目“{record.ProjectTitle ?? $"项目 {record.ProjectId}"}”已通过审核，请保持电话畅通。",
            "contacted" => $"您报名的项目“{record.ProjectTitle ?? $"项目 {record.ProjectId}"}”已有机构联系，请留意来电和后续安排。",
            "rejected" => $"您报名的项目“{record.ProjectTitle ?? $"项目 {record.ProjectId}"}”未通过审核。{record.AuditRemark}",
            "need_supplement" => $"您报名的项目“{record.ProjectTitle ?? $"项目 {record.ProjectId}"}”需要补充材料。{record.AuditRemark}",
            _ => $"您报名的项目“{record.ProjectTitle ?? $"项目 {record.ProjectId}"}”状态已更新为 {record.Status}。"
        };

        await AddNotification(record.ExternalUserId, "enrollment", statusText, content, "enrollment", record.Id);
    }

    private async Task AddNotification(
        string? externalUserId,
        string type,
        string title,
        string? content,
        string businessType,
        long businessId)
    {
        if (string.IsNullOrWhiteSpace(externalUserId))
            return;

        await _notificationRep.InsertAsync(new SaveLifeNotification
        {
            ExternalUserId = externalUserId,
            NotificationType = type,
            Title = title.Length <= 128 ? title : title[..128],
            Content = string.IsNullOrWhiteSpace(content) ? null : content.Length <= 512 ? content : content[..512],
            BusinessType = businessType,
            BusinessId = businessId
        });
    }
}
