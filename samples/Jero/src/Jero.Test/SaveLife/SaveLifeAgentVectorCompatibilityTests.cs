using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentVectorCompatibilityTests
{
    [Theory]
    [InlineData(null, "empty")]
    [InlineData("", "empty")]
    [InlineData("not-json", "invalid_json")]
    [InlineData("[1, 2]", "dimension_mismatch")]
    [InlineData("[1e999, 2, 3]", "non_finite")]
    public void ParseIndexVector_RejectsUnsafeOrIncompatiblePayloads(string? json, string expectedCode)
    {
        var error = Assert.Throws<SaveLifeAgentVectorCompatibilityException>(() =>
            SaveLifeAgentVectorCompatibilityPolicy.ParseIndexVector(
                json,
                expectedDimensions: 3,
                model: "doubao-embedding-vision"));

        Assert.Equal(expectedCode, error.ErrorCode);
        Assert.Equal(3, error.ExpectedDimensions);
        Assert.DoesNotContain("1e999", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseIndexVector_RejectsVectorsBeyondMaximumDimension()
    {
        var json = $"[{string.Join(',', Enumerable.Repeat("1", SaveLifeAgentVectorCompatibilityPolicy.MaximumDimensions + 1))}]";

        var error = Assert.Throws<SaveLifeAgentVectorCompatibilityException>(() =>
            SaveLifeAgentVectorCompatibilityPolicy.ParseIndexVector(
                json,
                SaveLifeAgentVectorCompatibilityPolicy.MaximumDimensions,
                "doubao-embedding-vision"));

        Assert.Equal("dimension_too_large", error.ErrorCode);
    }

    [Fact]
    public void ValidateQueryVector_RejectsDimensionConflict()
    {
        var error = Assert.Throws<SaveLifeAgentVectorCompatibilityException>(() =>
            SaveLifeAgentVectorCompatibilityPolicy.ValidateQueryVector(
                [1F, 2F],
                3,
                "doubao-embedding-vision"));

        Assert.Equal("dimension_mismatch", error.ErrorCode);
        Assert.Equal(2, error.ActualDimensions);
    }

    [Fact]
    public void CosineSimilarity_DimensionConflictIsNotSilentlyScoredAsZero()
    {
        var service = new SaveLifeAgentEmbeddingService(
            new SaveLifeAgentEmbeddingServiceTestsFactory());

        var error = Assert.Throws<SaveLifeAgentVectorCompatibilityException>(() =>
            service.CosineSimilarity([1F, 2F], [1F]));

        Assert.Equal("dimension_mismatch", error.ErrorCode);
    }

    private sealed class SaveLifeAgentEmbeddingServiceTestsFactory : ISaveLifeAgentClientFactory
    {
        public Task<SaveLifeAgentOptions> GetOptionsAsync() => Task.FromResult(new SaveLifeAgentOptions());

        public Task<bool> CanUseChatAsync() => Task.FromResult(false);

        public Task<bool> CanUseEmbeddingsAsync() => Task.FromResult(false);

        public Task<OpenAI.Chat.ChatClient> CreateChatClientAsync() => throw new NotSupportedException();

        public Task<OpenAI.Embeddings.EmbeddingClient> CreateEmbeddingClientAsync() => throw new NotSupportedException();
    }
}
