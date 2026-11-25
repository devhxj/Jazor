using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentRetrievalQueryTests
{
    [Fact]
    public void Build_UsesStructuredTermsAndKeepsTheCurrentToolQuery()
    {
        var query = SaveLifeAgentRetrievalQuery.Build(
            "肿瘤一般怎么治疗？另外现在有肿瘤招募项目吗？37岁",
            new SaveLifeAgentPatientSignal
            {
                SearchTerms = ["肿瘤"]
            });

        Assert.Null(query.DiseaseKeyword);
        Assert.Equal(["肿瘤"], query.SearchTerms);
        Assert.DoesNotContain("37", query.SearchTerms);
        Assert.True(query.HasProjectDiscoveryCriteria);
    }

    [Fact]
    public void Build_WithoutStructuredTerms_UsesTheCurrentToolQueryAsProjectCriterion()
    {
        var query = SaveLifeAgentRetrievalQuery.Build(
            "有没有白癜风项目，37岁",
            new SaveLifeAgentPatientSignal());

        Assert.Empty(query.SearchTerms);
        Assert.Equal("有没有白癜风项目，37岁", query.OriginalQuery);
        Assert.True(query.HasProjectDiscoveryCriteria);
    }

    [Fact]
    public void ScoreText_UsesAllStructuredTerms()
    {
        var query = SaveLifeAgentRetrievalQuery.Build(
            "肿瘤项目",
            new SaveLifeAgentPatientSignal
            {
                SearchTerms = ["肿瘤", "上海"]
            });

        var strong = query.ScoreText("上海肿瘤临床研究招募中");
        var weak = query.ScoreText("肿瘤科普知识");

        Assert.True(strong > weak);
    }
}
