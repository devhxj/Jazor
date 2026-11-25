using System.Text.Json;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public sealed class SaveLifeAgentConfigurationDefaultsTests
{
    [Fact]
    public void Defaults_UseVerifiedDoubaoAliasesAndEndpoints()
    {
        var options = new SaveLifeAgentOptions();

        Assert.Equal("https://ark.cn-beijing.volces.com/api/coding/v3", options.GetChatEndpoint());
        Assert.Equal("doubao-seed-2.0-lite", options.ChatModel);
        Assert.Equal("https://ark.cn-beijing.volces.com/api/coding/v3", options.GetMaterialEmbeddingEndpoint());
        Assert.Equal("doubao-embedding-vision", options.MaterialEmbeddingModel);
    }

    [Fact]
    public void MaterialEmbeddingSecret_DoesNotFallBackToLegacyGenericSecret()
    {
        var options = new SaveLifeAgentOptions
        {
            ApiKey = "legacy-generic-secret",
            MaterialEmbeddingApiKey = null
        };

        Assert.Null(options.GetMaterialEmbeddingApiKey());
        Assert.False(options.HasMaterialEmbeddingConfiguration);
    }

    [Fact]
    public void PromptDefaults_ComposeRolePromptToolRulesAndContextInOrder()
    {
        var options = new SaveLifeAgentOptions();

        Assert.Equal(SaveLifeMafAgent.DefaultMainRole, options.MainRole);
        Assert.Equal(SaveLifeMafAgent.DefaultPrompt, options.Prompt);
        Assert.Equal(SaveLifeMafAgent.Instructions, options.Instructions);

        var instructions = SaveLifeMafAgent.BuildInstructions(
            "自定义主角色",
            "自定义业务提示词",
            "本轮可信上下文");

        Assert.True(instructions.IndexOf("自定义主角色", StringComparison.Ordinal) <
            instructions.IndexOf("自定义业务提示词", StringComparison.Ordinal));
        Assert.True(instructions.IndexOf("自定义业务提示词", StringComparison.Ordinal) <
            instructions.IndexOf(SaveLifeMafAgent.ToolInstructions, StringComparison.Ordinal));
        Assert.True(instructions.IndexOf(SaveLifeMafAgent.ToolInstructions, StringComparison.Ordinal) <
            instructions.IndexOf("本轮可信上下文", StringComparison.Ordinal));
        Assert.Contains(nameof(MedicalInformationTool), instructions, StringComparison.Ordinal);
        Assert.Contains(nameof(ClinicalTrialTool), instructions, StringComparison.Ordinal);
        Assert.Contains(nameof(HealthProfileProposalTool), instructions, StringComparison.Ordinal);
        Assert.Contains(nameof(RoutineGuidanceTool), instructions, StringComparison.Ordinal);
    }

    [Fact]
    public void JsonDefaults_MatchVerifiedDoubaoConfiguration()
    {
        var path = FindRepositoryFile("src", "Jero.Application", "Configuration", "SaveLife.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var agent = document.RootElement.GetProperty("SaveLife").GetProperty("Agent");

        Assert.Equal(
            "https://ark.cn-beijing.volces.com/api/coding/v3",
            agent.GetProperty("ChatEndpoint").GetString());
        Assert.Equal("doubao-seed-2.0-lite", agent.GetProperty("ChatModel").GetString());
        Assert.Equal(
            "https://ark.cn-beijing.volces.com/api/coding/v3",
            agent.GetProperty("MaterialEmbeddingEndpoint").GetString());
        Assert.Equal(
            "doubao-embedding-vision",
            agent.GetProperty("MaterialEmbeddingModel").GetString());
    }

    private static string FindRepositoryFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var solution = Path.Combine(directory.FullName, "src", "Jero.slnx");
            if (File.Exists(solution))
                return Path.Combine([directory.FullName, .. segments]);

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("无法定位仓库根目录。");
    }
}
