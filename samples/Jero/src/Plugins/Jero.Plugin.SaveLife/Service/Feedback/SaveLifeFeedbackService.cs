namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 小程序反馈服务
/// </summary>
[Authorize]
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 176)]
public class SaveLifeFeedbackService : IDynamicApiController, ITransient
{
    private const int MaxDiagnosticReportsPerTenMinutes = 20;

    private readonly SqlSugarRepository<SaveLifeFeedback> _feedbackRep;
    private readonly ISaveLifePrivacyConsentGuard _privacyConsentGuard;

    public SaveLifeFeedbackService(
        SqlSugarRepository<SaveLifeFeedback> feedbackRep,
        ISaveLifePrivacyConsentGuard privacyConsentGuard)
    {
        _feedbackRep = feedbackRep;
        _privacyConsentGuard = privacyConsentGuard;
    }

    [HttpPost("/api/v1/save-life/feedback"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppSubmit")]
    public async Task<SaveLifeFeedbackOutput> MiniAppSubmit(SubmitSaveLifeFeedbackInput input)
    {
        await _privacyConsentGuard.EnsureAccountActiveAsync();
        var externalUserId = GetCurrentMiniAppOpenId();
        var content = NormalizeRequired(input.Content, "反馈内容不能为空", 512);
        var feedbackType = NormalizeRequired(input.FeedbackType, "反馈类型不能为空", 32);
        var contact = NormalizeOptional(input.Contact, 64);

        var recentCount = await _feedbackRep.AsQueryable()
            .Where(u => u.ExternalUserId == externalUserId)
            .Where(u => u.CreateTime >= DateTime.Now.AddMinutes(-10))
            .CountAsync();

        if (recentCount >= 3)
            throw Oops.Oh("提交过于频繁，请稍后再试").StatusCode(429);

        var feedback = new SaveLifeFeedback
        {
            ExternalUserId = externalUserId,
            FeedbackType = feedbackType,
            Content = content,
            Contact = contact,
            Status = "pending"
        };

        await _feedbackRep.InsertAsync(feedback);
        return ToOutput(feedback);
    }

    [HttpPost("/api/v1/save-life/diagnostics"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppDiagnostic")]
    public async Task MiniAppDiagnostic(SubmitSaveLifeClientDiagnosticInput input)
    {
        await _privacyConsentGuard.EnsureAccountActiveAsync();
        var externalUserId = GetCurrentMiniAppOpenId();
        var recentCount = await _feedbackRep.AsQueryable()
            .Where(u => u.ExternalUserId == externalUserId && u.FeedbackType == "client_diagnostic")
            .Where(u => u.CreateTime >= DateTime.Now.AddMinutes(-10))
            .CountAsync();
        if (recentCount >= MaxDiagnosticReportsPerTenMinutes)
            return;

        var diagnostic = SaveLifeClientDiagnosticPolicy.Sanitize(input);
        await _feedbackRep.InsertAsync(new SaveLifeFeedback
        {
            ExternalUserId = externalUserId,
            FeedbackType = "client_diagnostic",
            Content = System.Text.Json.JsonSerializer.Serialize(diagnostic),
            Status = "diagnostic"
        });
    }

    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<SaveLifeFeedback>> Page(SaveLifeFeedbackInput input)
    {
        var externalUserId = input.ExternalUserId?.Trim();
        var feedbackType = input.FeedbackType?.Trim();
        var status = input.Status?.Trim();
        return await _feedbackRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(externalUserId), u => u.ExternalUserId == externalUserId)
            .WhereIF(!string.IsNullOrWhiteSpace(feedbackType), u => u.FeedbackType == feedbackType)
            .WhereIF(!string.IsNullOrWhiteSpace(status), u => u.Status == status)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    private static SaveLifeFeedbackOutput ToOutput(SaveLifeFeedback feedback)
    {
        return new SaveLifeFeedbackOutput
        {
            Id = feedback.Id.ToString(),
            FeedbackType = feedback.FeedbackType,
            Content = feedback.Content,
            Contact = feedback.Contact,
            Status = feedback.Status,
            CreatedAt = feedback.CreateTime
        };
    }

    private static string GetCurrentMiniAppOpenId()
    {
        var openId = App.User?.FindFirst(ClaimConst.OpenId)?.Value?.Trim();
        if (!string.IsNullOrWhiteSpace(openId))
            return openId;

        throw Oops.Oh("小程序登录态无效，请重新登录").StatusCode(400);
    }

    private static string NormalizeRequired(string? value, string errorMessage, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw Oops.Oh(errorMessage).StatusCode(400);

        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
