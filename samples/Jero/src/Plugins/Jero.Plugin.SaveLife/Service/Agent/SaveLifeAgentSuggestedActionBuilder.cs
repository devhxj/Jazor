namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeAgentSuggestedActionTypes
{
    public const string ViewTrial = "view_trial";
    public const string ConfirmIntention = "confirm_intention";
    public const string CompleteAccountProfile = "complete_account_profile";
    public const string CompleteHealthProfile = "complete_profile";
    public const string ViewEnrollments = "view_enrollments";
    public const string ViewEnrollment = "view_enrollment";
    public const string UploadMaterial = "upload_material";
    public const string StartScreening = "start_screening";
}

public static class SaveLifeAgentSuggestedActionBuilder
{
    public static List<SaveLifeAgentSuggestedActionOutput> BuildForIntention(
        string status,
        string? enrollmentRecordId,
        string? trialId,
        bool requiresProfileCompletion)
    {
        var actions = new List<SaveLifeAgentSuggestedActionOutput>();
        var hasEnrollment = !string.IsNullOrWhiteSpace(enrollmentRecordId);

        if (hasEnrollment)
            Add(actions, SaveLifeAgentSuggestedActionTypes.ViewEnrollment, "查看报名详情", "enrollment", enrollmentRecordId);
        if (requiresProfileCompletion)
            Add(actions, SaveLifeAgentSuggestedActionTypes.CompleteHealthProfile, "完善健康档案", "health_profile", null);
        if (hasEnrollment && status is "pending" or "draft")
            Add(actions, SaveLifeAgentSuggestedActionTypes.UploadMaterial, "上传病例资料", "material", null);
        if (!string.IsNullOrWhiteSpace(trialId))
            Add(actions, SaveLifeAgentSuggestedActionTypes.ViewTrial, "查看项目详情", "trial", trialId);

        return actions.Take(4).ToList();
    }

    private static void Add(
        ICollection<SaveLifeAgentSuggestedActionOutput> actions,
        string type,
        string label,
        string? targetType,
        string? targetId)
    {
        actions.Add(new SaveLifeAgentSuggestedActionOutput
        {
            Type = type,
            Label = label,
            TargetType = targetType,
            TargetId = targetId
        });
    }
}
