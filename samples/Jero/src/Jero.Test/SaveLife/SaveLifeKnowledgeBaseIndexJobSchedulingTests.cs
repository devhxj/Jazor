using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeKnowledgeBaseIndexJobSchedulingTests
{
    [Fact]
    public void PublicKnowledgeBaseIndexJob_UsesShortPeriodRunOnStartTrigger()
    {
        var trigger = GetTrigger<SaveLifePublicKnowledgeBaseIndexJob>("SaveLifePublicKnowledgeBaseIndexDailyTrigger");

        Assert.Equal("PeriodMinutesAttribute", trigger.GetType().Name);
        Assert.True((bool)trigger.GetType().GetProperty("RunOnStart")!.GetValue(trigger)!);
    }

    [Fact]
    public void MaterialPrivateKnowledgeBaseIndexJob_UsesShortPeriodRunOnStartTrigger()
    {
        var trigger = GetTrigger<SaveLifeMaterialPrivateKnowledgeBaseIndexJob>("SaveLifeMaterialPrivateKnowledgeBaseIndexDailyTrigger");

        Assert.Equal("PeriodMinutesAttribute", trigger.GetType().Name);
        Assert.True((bool)trigger.GetType().GetProperty("RunOnStart")!.GetValue(trigger)!);
    }

    private static Attribute GetTrigger<TJob>(string triggerId)
    {
        return (Attribute)typeof(TJob)
            .GetCustomAttributes(false)
            .Single(attribute => (string?)attribute.GetType().GetProperty("TriggerId")?.GetValue(attribute) == triggerId);
    }
}
