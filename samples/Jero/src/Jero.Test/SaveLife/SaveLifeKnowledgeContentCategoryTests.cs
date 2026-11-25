using Jero.Plugin.SaveLife;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeKnowledgeContentCategoryTests
{
    [Theory]
    [InlineData(null, SaveLifeKnowledgeContentCategory.Science, "科普")]
    [InlineData("疾病知识", SaveLifeKnowledgeContentCategory.Science, "科普")]
    [InlineData("罕见病", SaveLifeKnowledgeContentCategory.RareDisease, "罕见病")]
    [InlineData("参与者故事", SaveLifeKnowledgeContentCategory.RareDisease, "罕见病")]
    [InlineData("患者故事", SaveLifeKnowledgeContentCategory.RareDisease, "罕见病")]
    [InlineData("new-drug", SaveLifeKnowledgeContentCategory.NewDrug, "新药资讯")]
    [InlineData("新药信息", SaveLifeKnowledgeContentCategory.NewDrug, "新药资讯")]
    public void NormalizeOrDefault_KnownAlias_ReturnsFixedMiniAppCategory(
        string? input,
        string expectedValue,
        string expectedLabel)
    {
        Assert.Equal(expectedValue, SaveLifeKnowledgeContentCategory.NormalizeOrDefault(input));
        Assert.Equal(expectedLabel, SaveLifeKnowledgeContentCategory.GetLabel(input));
    }

    [Fact]
    public void TryNormalize_UnknownCategory_RejectsDynamicValue()
    {
        Assert.False(SaveLifeKnowledgeContentCategory.TryNormalize("肿瘤资讯", out var normalized));
        Assert.Empty(normalized);
    }
}
