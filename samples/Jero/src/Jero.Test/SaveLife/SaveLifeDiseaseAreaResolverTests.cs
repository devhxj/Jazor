using Jero.Plugin.SaveLife;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeDiseaseAreaResolverTests
{
    [Fact]
    public void Resolve_WithAmbiguousIndication_PreservesSourceInsteadOfFallingBackToTitle()
    {
        var resolution = SaveLifeDiseaseAreaResolver.Resolve(
            "其他",
            "冠心病联合治疗项目",
            "冠心病合并高胆固醇血症");

        Assert.Equal("其他", resolution.DiseaseArea);
        Assert.True(resolution.IsAmbiguous);
        Assert.False(resolution.IsHighConfidence);
        Assert.False(resolution.HasConflict);
    }

    [Fact]
    public void Resolve_WithDyslipidemiaOnlyListedAsCardiovascularRiskFactor_UsesCirculatoryEvidence()
    {
        var resolution = SaveLifeDiseaseAreaResolver.Resolve(
            "呼吸系统疾病",
            "阿司匹林肠溶片生物等效性试验",
            "降低急性心肌梗死风险。降低心血管危险因素者（冠心病家族史、糖尿病、血脂异常、高血压）心肌梗死发作的风险。");

        Assert.Equal("循环系统疾病", resolution.DiseaseArea);
        Assert.False(resolution.IsAmbiguous);
        Assert.True(resolution.IsHighConfidence);
        Assert.True(resolution.HasConflict);
        Assert.Equal("circulatory", resolution.RuleName);
    }

    [Fact]
    public void Resolve_WithDyslipidemiaAsPrimaryComorbidity_RemainsAmbiguous()
    {
        var resolution = SaveLifeDiseaseAreaResolver.Resolve(
            "循环系统疾病",
            "联合治疗项目",
            "冠心病合并血脂异常");

        Assert.Equal("循环系统疾病", resolution.DiseaseArea);
        Assert.True(resolution.IsAmbiguous);
        Assert.False(resolution.IsHighConfidence);
        Assert.False(resolution.HasConflict);
    }

    [Fact]
    public void Resolve_WithCirculatoryTermOnlyInNegativeEvidence_UsesMetabolicEvidence()
    {
        var resolution = SaveLifeDiseaseAreaResolver.Resolve(
            "循环系统疾病",
            "非诺贝特胶囊生物等效性预试验",
            "用于治疗高胆固醇血症。尚未证明非诺贝特能够降低2型糖尿病患者的冠心病发病率和死亡。");

        Assert.Equal("代谢系统与营养疾病", resolution.DiseaseArea);
        Assert.False(resolution.IsAmbiguous);
        Assert.True(resolution.IsHighConfidence);
        Assert.True(resolution.HasConflict);
        Assert.Equal("metabolic", resolution.RuleName);
    }
}
