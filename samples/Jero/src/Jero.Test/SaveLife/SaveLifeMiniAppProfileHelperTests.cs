


using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;
namespace Jero.Test.SaveLife;

public class SaveLifeMiniAppProfileHelperTests
{
    [Fact]
    public void CalculateCompleteness_FullProfile_ReturnsOneHundred()
    {
        // Arrange
        var profile = new SaveLifeUserProfile
        {
            RealName = "张三",
            Gender = "male",
            BirthDate = new DateTime(1988, 1, 1),
            Phone = "13800138000",
            Address = "上海市",
            MedicalHistory = "肺癌术后",
            EmergencyContact = "李四",
            EmergencyPhone = "13900139000"
        };

        // Act
        var completeness = SaveLifeMiniAppProfileHelper.CalculateCompleteness(profile);

        // Assert
        Assert.Equal(100, completeness.Score);
        Assert.True(completeness.HasBasicProfile);
        Assert.True(completeness.HasContact);
        Assert.True(completeness.HasHealthRecord);
        Assert.True(completeness.HasEmergencyContact);
    }

    [Fact]
    public void NormalizeGender_ChineseValues_ReturnsStableEnumValue()
    {
        // Act
        var male = SaveLifeMiniAppProfileHelper.NormalizeGender("男");
        var female = SaveLifeMiniAppProfileHelper.NormalizeGender("女");
        var unknown = SaveLifeMiniAppProfileHelper.NormalizeGender("保密");

        // Assert
        Assert.Equal("male", male);
        Assert.Equal("female", female);
        Assert.Equal("unknown", unknown);
    }

    [Fact]
    public void BuildAgeRange_WithBirthDate_ReturnsOnlyAgeBand()
    {
        // Act
        var ageRange = SaveLifeMiniAppProfileHelper.BuildAgeRange(
            new DateTime(1968, 6, 1),
            new DateTime(2026, 6, 17));

        // Assert
        Assert.Equal("50-59岁", ageRange);
    }

    [Fact]
    public void EnrollmentMaterialHelper_NormalizeMaterialType_ReturnsKnownType()
    {
        // Act
        var materialType = SaveLifeEnrollmentMaterialHelper.NormalizeMaterialType("diagnosis_report");

        // Assert
        Assert.Equal("diagnosis_report", materialType);
        Assert.Equal("诊断报告", SaveLifeEnrollmentMaterialHelper.GetMaterialTypeText(materialType));
    }

    [Theory]
    [InlineData("report.PDF", ".pdf")]
    [InlineData("scan.jpeg", ".jpg")]
    [InlineData("record.docx", ".docx")]
    public void EnrollmentMaterialHelper_NormalizeExtension_ReturnsAllowedSuffix(string fileName, string expected)
    {
        // Act
        var suffix = SaveLifeEnrollmentMaterialHelper.NormalizeExtension(fileName);

        // Assert
        Assert.Equal(expected, suffix);
        Assert.True(SaveLifeEnrollmentMaterialHelper.IsAllowedExtension(suffix));
    }

    [Fact]
    public void EnrollmentMaterialHelper_CanDeletePersonalMaterial_OnlyAllowsUnboundEditableMaterial()
    {
        // Arrange
        var personalMaterial = new SaveLifeEnrollmentMaterial
        {
            EnrollmentRecordId = 0,
            Status = "submitted"
        };
        var enrollmentMaterial = new SaveLifeEnrollmentMaterial
        {
            EnrollmentRecordId = 1001,
            Status = "submitted"
        };
        var approvedPersonalMaterial = new SaveLifeEnrollmentMaterial
        {
            EnrollmentRecordId = 0,
            Status = "approved"
        };

        // Act & Assert
        Assert.True(SaveLifeEnrollmentMaterialHelper.CanDeletePersonalMaterial(personalMaterial));
        Assert.False(SaveLifeEnrollmentMaterialHelper.CanDeletePersonalMaterial(enrollmentMaterial));
        Assert.False(SaveLifeEnrollmentMaterialHelper.CanDeletePersonalMaterial(approvedPersonalMaterial));
    }

    [Fact]
    public void EnrollmentMaterialHelper_IsEffectiveForEnrollment_IncludesPersonalAndCurrentEnrollmentOnly()
    {
        var personal = new SaveLifeEnrollmentMaterial
        {
            ExternalUserId = "openid-1",
            EnrollmentRecordId = 0
        };
        var currentEnrollment = new SaveLifeEnrollmentMaterial
        {
            ExternalUserId = "openid-1",
            EnrollmentRecordId = 3001
        };
        var otherEnrollment = new SaveLifeEnrollmentMaterial
        {
            ExternalUserId = "openid-1",
            EnrollmentRecordId = 3002
        };
        var otherUser = new SaveLifeEnrollmentMaterial
        {
            ExternalUserId = "openid-2",
            EnrollmentRecordId = 0
        };

        Assert.True(SaveLifeEnrollmentMaterialHelper.IsEffectiveForEnrollment(personal, "openid-1", 3001));
        Assert.True(SaveLifeEnrollmentMaterialHelper.IsEffectiveForEnrollment(currentEnrollment, "openid-1", 3001));
        Assert.False(SaveLifeEnrollmentMaterialHelper.IsEffectiveForEnrollment(otherEnrollment, "openid-1", 3001));
        Assert.False(SaveLifeEnrollmentMaterialHelper.IsEffectiveForEnrollment(otherUser, "openid-1", 3001));
    }

    [Fact]
    public void KnowledgeContentSanitizer_Normalize_RemovesUnsafeHtmlAndKeepsMedia()
    {
        // Arrange
        const string content = """
            <p onclick="alert(1)" style="max-width: 100%; color: red;">正文</p>
            <custom><script>alert(2)</script><span onclick="alert(3)">嵌套文字</span></custom>
            <script>alert(1)</script>
            <img src="javascript:alert(1)" onerror="alert(1)" alt="bad" />
            <img src="java
            script:alert(1)" alt="bad2" />
            <img src="/upload/save-life/knowledge/a.png" alt="ok" style="max-width: 100%;" />
            <video src="https://example.com/a.mp4" controls autoplay onplay="alert(1)"></video>
            <a href="javascript:alert(1)" target="_blank">链接</a>
            """;

        // Act
        var normalized = SaveLifeKnowledgeContentSanitizer.Normalize(content);

        // Assert
        Assert.NotNull(normalized);
        Assert.DoesNotContain("<script", normalized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onclick", normalized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onerror", normalized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("javascript:", normalized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert(2)", normalized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert(3)", normalized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("color: red", normalized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("autoplay", normalized, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("嵌套文字", normalized);
        Assert.Contains("<img src=\"/upload/save-life/knowledge/a.png\" alt=\"ok\" style=\"max-width: 100%\">", normalized);
        Assert.Contains("<video src=\"https://example.com/a.mp4\" controls=\"\">", normalized);
    }

    [Fact]
    public void KnowledgeContentSanitizer_Normalize_PrefixesLocalUploadMediaWithRootSlash()
    {
        // Arrange
        const string content = """
            <p>正文图片</p>
            <img src="upload/save-life/knowledge/a.png" alt="图片" />
            <video src="upload/save-life/knowledge/v.mp4" poster="upload/save-life/knowledge/p.png" controls></video>
            """;

        // Act
        var normalized = SaveLifeKnowledgeContentSanitizer.Normalize(content);

        // Assert
        Assert.NotNull(normalized);
        Assert.Contains("src=\"/upload/save-life/knowledge/a.png\"", normalized);
        Assert.Contains("src=\"/upload/save-life/knowledge/v.mp4\"", normalized);
        Assert.Contains("poster=\"/upload/save-life/knowledge/p.png\"", normalized);
        Assert.DoesNotContain("src=\"upload/save-life/knowledge", normalized);
        Assert.DoesNotContain("poster=\"upload/save-life/knowledge", normalized);
    }

    [Fact]
    public void KnowledgeContentSanitizer_Normalize_WithMediaOrigin_PersistsAbsoluteMediaUrls()
    {
        const string content = "<img src=\"upload/save-life/knowledge/a.png\" /><video poster=\"/upload/save-life/knowledge/p.png\"></video>";

        var normalized = SaveLifeKnowledgeContentSanitizer.Normalize(
            content,
            "https://www.aiarecruit.com");

        Assert.NotNull(normalized);
        Assert.Contains("src=\"https://www.aiarecruit.com/upload/save-life/knowledge/a.png\"", normalized);
        Assert.Contains("poster=\"https://www.aiarecruit.com/upload/save-life/knowledge/p.png\"", normalized);
    }

    [Fact]
    public void KnowledgeContentSanitizer_Normalize_RemovesUnsafeMediaSourceAfterUrlNormalization()
    {
        // Arrange
        const string content = """
            <img src="java
            script:alert(1)" alt="bad" />
            <video src="data:video/mp4;base64,xxx" poster="vbscript:msgbox(1)" controls></video>
            """;

        // Act
        var normalized = SaveLifeKnowledgeContentSanitizer.Normalize(content);

        // Assert
        Assert.NotNull(normalized);
        Assert.DoesNotContain("javascript:", normalized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data:", normalized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("vbscript:", normalized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("src=", normalized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("poster=", normalized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void KnowledgeContentSanitizer_Normalize_RemovesWhitespaceFromAbsoluteMediaSource()
    {
        // Arrange
        const string content = """
            <img src="https://cdn.example.com/a
            .png" alt="ok" />
            """;

        // Act
        var normalized = SaveLifeKnowledgeContentSanitizer.Normalize(content);

        // Assert
        Assert.NotNull(normalized);
        Assert.Contains("src=\"https://cdn.example.com/a.png\"", normalized);
    }

    [Fact]
    public void KnowledgeContentSanitizer_Normalize_NormalizesRelativeMediaAndRejectsUnsafeProtocolRelativeSources()
    {
        // Arrange
        const string content = """
            <img src="images/a.png" alt="relative" />
            <img src="//cdn.example.com/a.png" alt="cdn" />
            <img src="//javascript:alert(1)" alt="bad" />
            <img src="../private/a.png" alt="parent" />
            """;

        // Act
        var normalized = SaveLifeKnowledgeContentSanitizer.Normalize(content);

        // Assert
        Assert.NotNull(normalized);
        Assert.Contains("src=\"/images/a.png\"", normalized);
        Assert.Contains("src=\"//cdn.example.com/a.png\"", normalized);
        Assert.DoesNotContain("javascript:", normalized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("../private", normalized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void KnowledgeIndexTextBuilder_Build_IncludesKnowledgeFieldsAndTextOnlyContent()
    {
        // Arrange
        var knowledge = new SaveLifeDiseaseKnowledge
        {
            Title = "肺癌术后复查",
            DiseaseName = "肺癌",
            Author = "肿瘤科",
            Summary = "复查频率和异常症状提醒",
            Tags = "肺癌,复查,CT",
            Content = """
                <h2>正文小标题</h2>
                <p>建议按医嘱定期复查胸部 CT。</p>
                <img src="/upload/save-life/knowledge/ct.png" alt="胸部CT" />
                <video src="https://cdn.example.com/review.mp4" controls></video>
                <p>如出现持续发热或胸痛，应及时就医。</p>
                """
        };

        // Act
        var indexText = SaveLifeKnowledgeIndexTextBuilder.Build(knowledge);

        // Assert
        Assert.Contains("肺癌术后复查", indexText);
        Assert.Contains("肺癌", indexText);
        Assert.Contains("肿瘤科", indexText);
        Assert.Contains("复查频率和异常症状提醒", indexText);
        Assert.Contains("肺癌,复查,CT", indexText);
        Assert.Contains("正文小标题", indexText);
        Assert.Contains("建议按医嘱定期复查胸部 CT。", indexText);
        Assert.Contains("如出现持续发热或胸痛，应及时就医。", indexText);
        Assert.DoesNotContain("upload/save-life/knowledge", indexText);
        Assert.DoesNotContain("cdn.example.com", indexText);
        Assert.DoesNotContain("胸部CT", indexText);
    }
}
