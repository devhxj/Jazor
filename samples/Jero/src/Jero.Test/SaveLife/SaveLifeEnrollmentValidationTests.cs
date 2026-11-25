using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeEnrollmentValidationTests
{
    [Fact]
    public void ValidateForSubmission_WithMissingProfile_ReturnsProfileAndDiseaseRequirements()
    {
        var result = SaveLifeEnrollmentValidation.ValidateForSubmission(null, null, null);

        Assert.False(result.CanSubmit);
        Assert.Contains("个人资料", result.MissingFields);
        Assert.Contains("本次报名病情描述或既往病史", result.MissingFields);
    }

    [Fact]
    public void ValidateForSubmission_WithCompleteProfileAndDiseaseDescription_AllowsSubmission()
    {
        var profile = new SaveLifeUserProfile
        {
            RealName = "张三",
            Gender = "男",
            BirthDate = new DateTime(1980, 1, 1),
            Phone = "13800138000",
            Address = "浙江杭州",
            MedicalHistory = "肺纤维化病史",
        };

        var result = SaveLifeEnrollmentValidation.ValidateForSubmission(profile, "肺纤维化", profile.MedicalHistory);

        Assert.True(result.CanSubmit);
        Assert.Empty(result.MissingFields);
    }

    [Fact]
    public void GetMissingProfileFields_WithPartialProfile_ReturnsConcreteFieldNames()
    {
        var profile = new SaveLifeUserProfile
        {
            RealName = "张三",
            Gender = "男",
        };

        var missing = SaveLifeEnrollmentValidation.GetMissingProfileFields(profile);

        Assert.Contains("出生日期", missing);
        Assert.Contains("手机号", missing);
        Assert.Contains("常住地址", missing);
        Assert.Contains("既往病史", missing);
    }

    [Fact]
    public void ValidateForSubmission_WithSubmissionPhone_AllowsProfilePhoneFallback()
    {
        var profile = new SaveLifeUserProfile
        {
            RealName = "张三",
            Gender = "男",
            BirthDate = new DateTime(1980, 1, 1),
            Address = "浙江杭州",
            MedicalHistory = "肺癌术后治疗史",
        };
        var fields = new SaveLifeEnrollmentSubmissionFields
        {
            ContactPhone = "13800138000",
            DiseaseDescription = "肺癌"
        };

        var result = SaveLifeEnrollmentValidation.ValidateForSubmission(profile, fields);

        Assert.True(result.CanSubmit);
        Assert.Empty(result.MissingFields);
    }

    [Fact]
    public void ResolveSubmissionStatus_WithPendingAndMissingData_DowngradesToDraft()
    {
        var validation = SaveLifeEnrollmentValidation.ValidateForSubmission(null, new SaveLifeEnrollmentSubmissionFields());

        var status = SaveLifeEnrollmentValidation.ResolveSubmissionStatus("pending", validation);

        Assert.Equal("draft", status);
    }

    [Theory]
    [InlineData("draft", true)]
    [InlineData("pending", true)]
    [InlineData("need_supplement", false)]
    [InlineData("approved", false)]
    [InlineData("cancelled", true)]
    [InlineData("rejected", true)]
    [InlineData("closed", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void CanReuseForSubmission_WithStatus_ReturnsExpectedResult(string? status, bool expected)
    {
        var record = new SaveLifeEnrollmentRecord { Status = status! };

        var result = SaveLifeEnrollmentValidation.CanReuseForSubmission(record);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("cancelled", true)]
    [InlineData("rejected", true)]
    [InlineData("closed", true)]
    [InlineData("draft", false)]
    [InlineData("pending", false)]
    [InlineData("approved", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void IsTerminalEnrollmentStatus_WithStatus_ReturnsExpectedResult(string? status, bool expected)
    {
        var result = SaveLifeEnrollmentValidation.IsTerminalEnrollmentStatus(status);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void PrepareForSubmission_WithTerminalRecord_ResetsReviewAndReusesRecord()
    {
        var submittedTime = new DateTime(2026, 7, 11, 16, 0, 0);
        var record = new SaveLifeEnrollmentRecord
        {
            Id = 1001,
            Status = "rejected",
            IsDelete = true,
            DeleteTime = submittedTime.AddDays(-1),
            AuditTime = submittedTime.AddDays(-2),
            AuditRemark = "不符合条件"
        };

        SaveLifeEnrollmentValidation.PrepareForSubmission(record, "pending", submittedTime);

        Assert.Equal(1001, record.Id);
        Assert.Equal("pending", record.Status);
        Assert.Equal(submittedTime, record.SubmittedTime);
        Assert.Null(record.AuditTime);
        Assert.Null(record.AuditRemark);
        Assert.False(record.IsDelete);
        Assert.Null(record.DeleteTime);
    }

    [Fact]
    public void ApplySubmissionFields_WithExistingValues_PreservesCurrentWhenInputMissing()
    {
        var profile = new SaveLifeUserProfile
        {
            RealName = "档案姓名",
            Phone = "13800138000",
            MedicalHistory = "档案病史"
        };
        var record = new SaveLifeEnrollmentRecord
        {
            ContactName = "报名姓名",
            ContactPhone = "13900139000",
            MedicalHistory = "报名病史"
        };
        var fields = new SaveLifeEnrollmentSubmissionFields
        {
            DiseaseDescription = "肺癌"
        };

        SaveLifeEnrollmentValidation.ApplySubmissionFields(record, fields, profile);

        Assert.Equal("报名姓名", record.ContactName);
        Assert.Equal("13900139000", record.ContactPhone);
        Assert.Equal("肺癌", record.DiseaseDescription);
        Assert.Equal("报名病史", record.MedicalHistory);
    }

    [Fact]
    public void ApplySubmissionFields_WithExplicitInput_UpdatesAndTruncatesFields()
    {
        var record = new SaveLifeEnrollmentRecord();
        var fields = new SaveLifeEnrollmentSubmissionFields
        {
            ContactName = "  李四  ",
            ContactPhone = "  13800138000  ",
            DiseaseDescription = new string('病', 600),
            Notes = new string('备', 5000)
        };

        SaveLifeEnrollmentValidation.ApplySubmissionFields(record, fields, null);

        Assert.Equal("李四", record.ContactName);
        Assert.Equal("13800138000", record.ContactPhone);
        Assert.Equal(512, record.DiseaseDescription?.Length);
        Assert.Equal(4000, record.Notes?.Length);
    }
}
