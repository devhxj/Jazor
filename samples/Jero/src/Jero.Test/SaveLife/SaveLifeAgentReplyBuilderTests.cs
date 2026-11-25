using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentReplyBuilderTests
{
    [Fact]
    public void ExtractSignal_FreeText_ReturnsOnlyDeterministicPhoneSignal()
    {
        var signal = SaveLifeAgentReplyBuilder.ExtractSignal(
        [
            "患者58岁，男，在上海，确诊肺癌两年，联系电话13800138000。"
        ]);

        Assert.Null(signal.DiseaseKeyword);
        Assert.Null(signal.City);
        Assert.Equal("13800138000", signal.Phone);
    }

    [Fact]
    public void BuildEnrollmentNotes_UnorderedMessages_ReturnsChronologicalTranscript()
    {
        var notes = SaveLifeAgentReplyBuilder.BuildEnrollmentNotes(
            "明确有意向",
            [
                new SaveLifeAgentMessageOutput
                {
                    Role = "assistant",
                    Content = "请确认项目",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 2, 0)
                },
                new SaveLifeAgentMessageOutput
                {
                    Role = "user",
                    Content = "我想参加",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 1, 0)
                }
            ]);

        Assert.Contains("小程序助手意向：明确有意向", notes);
        Assert.True(
            notes.IndexOf("user: 我想参加", StringComparison.Ordinal) <
            notes.IndexOf("assistant: 请确认项目", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("明确有意向", "pending")]
    [InlineData("犹豫考虑", "draft")]
    [InlineData("暂不参与", "closed")]
    public void MapIntentionStatus_StructuredChoice_ReturnsExpectedStatus(
        string intention,
        string expectedStatus)
    {
        var status = SaveLifeAgentReplyBuilder.MapIntentionStatus(intention);

        Assert.Equal(expectedStatus, status);
    }
}
