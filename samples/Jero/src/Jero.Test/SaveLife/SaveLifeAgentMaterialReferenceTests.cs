using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentMaterialReferenceTests
{
    [Fact]
    public void BuildMaterialReference_WithPersonalMaterial_DoesNotExposeEnrollmentRecordIdZero()
    {
        var material = new SaveLifeEnrollmentMaterial
        {
            Id = 1001,
            EnrollmentRecordId = 0,
            MaterialType = "diagnosis_report",
            FileName = "诊断报告",
            FileSuffix = ".pdf",
            Status = "need_supplement"
        };
        var candidate = new SaveLifeAgentVectorIndex
        {
            ResourceId = material.Id,
            Title = "诊断报告 - 诊断报告.pdf",
            Content = "肺癌诊断报告"
        };

        var reference = SaveLifeAgentRetrievalService.BuildMaterialReference(material, candidate, 0.92);

        Assert.Equal("1001", reference.Id);
        Assert.Equal("1001", reference.MaterialId);
        Assert.True(reference.IsPersonalMaterial);
        Assert.Null(reference.EnrollmentRecordId);
        Assert.Equal("诊断报告.pdf", reference.FileName);
        Assert.Equal("need_supplement", reference.Status);
        Assert.Equal("需补充", reference.StatusText);
        Assert.False(reference.CanSatisfyRequiredMaterial);
        Assert.True(reference.RequiresSupplement);
    }

    [Fact]
    public void BuildMaterialReference_WithEnrollmentMaterial_ExposesEnrollmentRecordId()
    {
        var material = new SaveLifeEnrollmentMaterial
        {
            Id = 1002,
            EnrollmentRecordId = 2001,
            MaterialType = "medical_record",
            FileName = "病历",
            FileSuffix = ".jpg",
            Status = "approved"
        };
        var candidate = new SaveLifeAgentVectorIndex
        {
            ResourceId = material.Id,
            Title = "病例资料 - 病历.jpg",
            Content = "既往治疗病历"
        };

        var reference = SaveLifeAgentRetrievalService.BuildMaterialReference(material, candidate, 0.9);

        Assert.False(reference.IsPersonalMaterial);
        Assert.Equal("1002", reference.MaterialId);
        Assert.Equal("2001", reference.EnrollmentRecordId);
        Assert.True(reference.CanSatisfyRequiredMaterial);
        Assert.False(reference.RequiresSupplement);
    }
}
