namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeMiniAppProfileHelper
{
    public static SaveLifeProfileCompleteness CalculateCompleteness(SaveLifeUserProfile profile)
    {
        var hasBasicProfile = !string.IsNullOrWhiteSpace(profile.RealName) &&
            !string.IsNullOrWhiteSpace(profile.Gender) &&
            profile.BirthDate.HasValue;
        var hasContact = !string.IsNullOrWhiteSpace(profile.Phone) &&
            !string.IsNullOrWhiteSpace(profile.Address);
        var hasHealthRecord = !string.IsNullOrWhiteSpace(profile.MedicalHistory) ||
            !string.IsNullOrWhiteSpace(profile.Allergies);
        var hasEmergencyContact = !string.IsNullOrWhiteSpace(profile.EmergencyContact) &&
            !string.IsNullOrWhiteSpace(profile.EmergencyPhone);

        var score = 0;
        if (hasBasicProfile) score += 30;
        if (hasContact) score += 25;
        if (hasHealthRecord) score += 30;
        if (hasEmergencyContact) score += 15;

        return new SaveLifeProfileCompleteness(score, hasBasicProfile, hasContact, hasHealthRecord, hasEmergencyContact);
    }

    public static string? NormalizeText(string? value, int maxLength, string fieldName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        if (normalized.Length > maxLength)
            throw Oops.Oh($"{fieldName}不能超过{maxLength}个字符").StatusCode(400);

        return normalized;
    }

    public static string? NormalizeGender(string? value)
    {
        var normalized = NormalizeText(value, 16, "性别");
        if (normalized == null)
            return null;

        return normalized switch
        {
            "男" => "male",
            "女" => "female",
            "保密" or "其他" => "unknown",
            "male" or "female" or "unknown" => normalized,
            _ => throw Oops.Oh("性别取值不正确").StatusCode(400)
        };
    }

    public static DateTime? NormalizeBirthDate(DateTime? value)
    {
        if (value == null)
            return null;

        var date = value.Value.Date;
        if (date > DateTime.Today || date < new DateTime(1900, 1, 1))
            throw Oops.Oh("出生日期不正确").StatusCode(400);

        return date;
    }

    public static string? NormalizePhone(string? value, string fieldName)
    {
        var normalized = NormalizeText(value, 32, fieldName);
        if (normalized == null)
            return null;

        if (normalized.Any(u => !(char.IsDigit(u) || u is '+' or '-' or ' ')))
            throw Oops.Oh($"{fieldName}格式不正确").StatusCode(400);

        return normalized;
    }

    public static string? NormalizeEmail(string? value)
    {
        var normalized = NormalizeText(value, 128, "邮箱");
        if (normalized == null)
            return null;

        if (!new EmailAddressAttribute().IsValid(normalized))
            throw Oops.Oh("邮箱格式不正确").StatusCode(400);

        return normalized;
    }

    public static string? BuildAgeRange(DateTime? birthDate, DateTime? today = null)
    {
        if (!birthDate.HasValue)
            return null;

        var currentDate = (today ?? DateTime.Today).Date;
        var date = birthDate.Value.Date;
        var age = currentDate.Year - date.Year;
        if (date > currentDate.AddYears(-age))
            age--;

        if (age < 0)
            return null;

        if (age < 18)
            return "未成年";

        var start = age / 10 * 10;
        return $"{start}-{start + 9}岁";
    }
}

public sealed record SaveLifeProfileCompleteness(
    int Score,
    bool HasBasicProfile,
    bool HasContact,
    bool HasHealthRecord,
    bool HasEmergencyContact);
