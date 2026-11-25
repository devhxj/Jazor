


using System.Security.Cryptography;
using System.Text.Json;
namespace Jero.Plugin.SaveLife.Service;

public interface ISaveLifeAgentEmbeddingService
{
    Task<float[]?> GenerateEmbeddingAsync(string text);

    string ComputeContentHash(string text);

    double CosineSimilarity(IReadOnlyList<float> left, IReadOnlyList<float> right);

    string SerializeVector(IReadOnlyList<float> vector);

    float[] DeserializeVector(string? json);
}

public sealed class SaveLifeAgentEmbeddingService : ISaveLifeAgentEmbeddingService, ITransient
{
    private readonly ISaveLifeAgentClientFactory _clientFactory;

    public SaveLifeAgentEmbeddingService(ISaveLifeAgentClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<float[]?> GenerateEmbeddingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var options = await _clientFactory.GetOptionsAsync();
        if (!options.HasEmbeddingConfiguration)
            return null;

        var client = await _clientFactory.CreateEmbeddingClientAsync();
        var input = LimitText(text, 6000);
        try
        {
            var result = await client.GenerateEmbeddingAsync(input);
            return result.Value.ToFloats().ToArray();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var message = SaveLifeAgentDiagnostics.BuildEmbeddingRequestErrorMessage(
                ex,
                options.EmbeddingModel,
                input.Length,
                BuildEndpoint(options.GetEmbeddingEndpoint()));
            throw new InvalidOperationException(message, ex);
        }
    }

    public string ComputeContentHash(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public double CosineSimilarity(IReadOnlyList<float> left, IReadOnlyList<float> right)
    {
        return SaveLifeAgentVectorCompatibilityPolicy.CosineSimilarity(left, right);
    }

    public string SerializeVector(IReadOnlyList<float> vector)
    {
        return JsonSerializer.Serialize(vector);
    }

    public float[] DeserializeVector(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            return JsonSerializer.Deserialize<float[]>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string LimitText(string text, int maxLength)
    {
        return text.Length <= maxLength ? text : text[..maxLength];
    }

    private static Uri? BuildEndpoint(string? endpointValue)
    {
        return Uri.TryCreate(endpointValue, UriKind.Absolute, out var endpoint)
            ? endpoint
            : null;
    }
}
