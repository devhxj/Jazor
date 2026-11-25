using System.Reflection;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentDiagnosticsTests
{
    [Fact]
    public void BuildErrorSummary_WithSecrets_RedactsSensitiveTokens()
    {
        var exception = new InvalidOperationException(
            "Authorization: Bearer abcdefghijklmnopqrstuvwxyz123456; api_key=sk-abcdefghijklmnopqrstuvwxyz123456");

        var summary = SaveLifeAgentDiagnostics.BuildErrorSummary(exception);

        Assert.NotNull(summary);
        Assert.Contains("InvalidOperationException", summary);
        Assert.DoesNotContain("abcdefghijklmnopqrstuvwxyz123456", summary);
        Assert.DoesNotContain("sk-abcdefghijklmnopqrstuvwxyz123456", summary);
        Assert.Contains("[redacted]", summary);
    }

    [Fact]
    public void BuildUserMessageLogText_WithSensitiveFields_PreservesDiagnosticMeaningAndRedactsValues()
    {
        const string message = "我想了解白癜风相关实验，电话 13800138000，邮箱 patient@example.com，"
            + "身份证 510107199001011234。\r\nAuthorization: Bearer abcdefghijklmnopqrstuvwxyz123456";

        var result = SaveLifeAgentDiagnostics.BuildUserMessageLogText(message);

        Assert.Contains("白癜风相关实验", result, StringComparison.Ordinal);
        Assert.Contains("138****8000", result, StringComparison.Ordinal);
        Assert.Contains("p***@example.com", result, StringComparison.Ordinal);
        Assert.Contains("510107********1234", result, StringComparison.Ordinal);
        Assert.Contains("Authorization=[redacted]", result, StringComparison.Ordinal);
        Assert.DoesNotContain("13800138000", result, StringComparison.Ordinal);
        Assert.DoesNotContain("patient@example.com", result, StringComparison.Ordinal);
        Assert.DoesNotContain("510107199001011234", result, StringComparison.Ordinal);
        Assert.DoesNotContain("abcdefghijklmnopqrstuvwxyz123456", result, StringComparison.Ordinal);
        Assert.DoesNotContain('\r', result);
        Assert.DoesNotContain('\n', result);
    }

    [Fact]
    public void BuildUserMessageLogText_WithLongMessage_LimitsLoggedContent()
    {
        var result = SaveLifeAgentDiagnostics.BuildUserMessageLogText(new string('问', 800));

        Assert.Equal(500, result.Length);
    }

    [Fact]
    public void BuildLogValueList_WithDuplicatesAndPhone_RedactsAndDeduplicatesValues()
    {
        var result = SaveLifeAgentDiagnostics.BuildLogValueList(
            ["白癜风", "白癜风", "13800138000"]);

        Assert.Equal("白癜风 | 138****8000", result);
    }

    [Fact]
    public void BuildErrorSummary_WithRetrievalFailure_IncludesSpecificReason()
    {
        var exception = new SaveLifeAgentUnavailableException(
            SaveLifeAgentFailureCodes.RetrievalUnavailable,
            SaveLifeAgentFailureStages.Retrieval,
            new InvalidOperationException("项目索引暂时不可用"));

        var result = SaveLifeAgentDiagnostics.BuildErrorSummary(exception);

        Assert.NotNull(result);
        Assert.Contains("InvalidOperationException", result, StringComparison.Ordinal);
        Assert.Contains("项目索引暂时不可用", result, StringComparison.Ordinal);
    }

    [Fact]
    public void ConfigErrorMessage_WithApiKey_RedactsBeforeReturningToUi()
    {
        var method = typeof(SaveLifeAgentConfigService).GetMethod(
            "NormalizeErrorMessage",
            BindingFlags.NonPublic | BindingFlags.Static);
        var secret = new string('x', 32);
        var exception = new InvalidOperationException(
            "Service request failed. Status: 400 (Bad Request); api_key=" + secret);

        var result = Assert.IsType<string>(method?.Invoke(null, [exception]));

        Assert.Contains("Service request failed", result);
        Assert.DoesNotContain(secret, result);
        Assert.Contains("[redacted]", result);
    }

    [Fact]
    public void ExtractServiceError_ReadsOpenAiCompatiblePayloadWithoutSecrets()
    {
        var secret = new string('y', 32);
        var payload = "{\"error\":{\"message\":\"model rejected api_key="
            + secret
            + "\",\"type\":\"invalid_request_error\",\"code\":\"model_not_found\"}}";

        var result = SaveLifeAgentDiagnostics.ExtractServiceError(payload);

        Assert.NotNull(result);
        Assert.Contains("model rejected", result);
        Assert.Contains("model_not_found", result);
        Assert.DoesNotContain(secret, result);
    }

    [Fact]
    public void EmbeddingRequestErrorMessage_ContainsModelAndInputDiagnostics()
    {
        var result = SaveLifeAgentDiagnostics.BuildEmbeddingRequestErrorMessage(
            new InvalidOperationException("Service request failed. Status: 400 (Bad Request)"),
            "BAAI/bge-large-zh-v1.5",
            450);

        Assert.Contains("BAAI/bge-large-zh-v1.5", result);
        Assert.Contains("inputChars=450", result);
        Assert.Contains("Status: 400", result);
    }
}
