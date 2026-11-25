using System.Text.Json;

namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeAgentVectorCompatibilityPolicy
{
    public const int MaximumDimensions = 4096;

    private const int MaximumJsonBytes = 256 * 1024;

    public static float[] ParseIndexVector(
        string? json,
        int? expectedDimensions,
        string model)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw Create("empty", expectedDimensions, null, model);
        if (json.Length > MaximumJsonBytes)
            throw Create("json_too_large", expectedDimensions, null, model);
        if (expectedDimensions is <= 0 or > MaximumDimensions)
            throw Create("expected_dimension_invalid", expectedDimensions, null, model);

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
                throw Create("invalid_json", expectedDimensions, null, model);

            var values = new List<float>();
            foreach (var element in document.RootElement.EnumerateArray())
            {
                if (values.Count >= MaximumDimensions)
                    throw Create("dimension_too_large", expectedDimensions, values.Count + 1, model);
                if (element.ValueKind != JsonValueKind.Number || !element.TryGetDouble(out var number))
                    throw Create("invalid_number", expectedDimensions, values.Count, model);
                if (!double.IsFinite(number) || number > float.MaxValue || number < -float.MaxValue)
                    throw Create("non_finite", expectedDimensions, values.Count + 1, model);
                values.Add((float)number);
            }

            if (values.Count == 0)
                throw Create("empty", expectedDimensions, 0, model);
            if (expectedDimensions.HasValue && values.Count != expectedDimensions.Value)
                throw Create("dimension_mismatch", expectedDimensions, values.Count, model);
            if (values.Any(value => !float.IsFinite(value)))
                throw Create("non_finite", expectedDimensions, values.Count, model);
            return values.ToArray();
        }
        catch (SaveLifeAgentVectorCompatibilityException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw Create("invalid_json", expectedDimensions, null, model);
        }
    }

    public static float[] ValidateQueryVector(
        float[]? vector,
        int? expectedDimensions,
        string model)
    {
        if (vector == null || vector.Length == 0)
            throw Create("empty", expectedDimensions, vector?.Length, model);
        if (vector.Length > MaximumDimensions)
            throw Create("dimension_too_large", expectedDimensions, vector.Length, model);
        if (expectedDimensions.HasValue && vector.Length != expectedDimensions.Value)
            throw Create("dimension_mismatch", expectedDimensions, vector.Length, model);
        if (vector.Any(value => !float.IsFinite(value)))
            throw Create("non_finite", expectedDimensions, vector.Length, model);
        return vector;
    }

    public static double CosineSimilarity(
        IReadOnlyList<float> left,
        IReadOnlyList<float> right,
        int? expectedDimensions = null,
        string model = "unknown")
    {
        if (left.Count == 0 || right.Count == 0)
            throw Create("empty", expectedDimensions, Math.Min(left.Count, right.Count), model);
        if (left.Count != right.Count)
            throw Create("dimension_mismatch", expectedDimensions ?? left.Count, right.Count, model);
        if (left.Any(value => !float.IsFinite(value)) || right.Any(value => !float.IsFinite(value)))
            throw Create("non_finite", expectedDimensions, right.Count, model);

        double dot = 0;
        double leftNorm = 0;
        double rightNorm = 0;
        for (var i = 0; i < left.Count; i++)
        {
            dot += left[i] * right[i];
            leftNorm += left[i] * left[i];
            rightNorm += right[i] * right[i];
        }

        if (leftNorm <= 0 || rightNorm <= 0 || !double.IsFinite(leftNorm) || !double.IsFinite(rightNorm))
            throw Create("zero_norm", expectedDimensions, right.Count, model);

        return dot / (Math.Sqrt(leftNorm) * Math.Sqrt(rightNorm));
    }

    private static SaveLifeAgentVectorCompatibilityException Create(
        string errorCode,
        int? expectedDimensions,
        int? actualDimensions,
        string model) => new(errorCode, expectedDimensions, actualDimensions, model);
}

public sealed class SaveLifeAgentVectorCompatibilityException : InvalidOperationException
{
    public SaveLifeAgentVectorCompatibilityException(
        string errorCode,
        int? expectedDimensions,
        int? actualDimensions,
        string model)
        : base($"向量兼容性校验失败：{errorCode}")
    {
        ErrorCode = errorCode;
        ExpectedDimensions = expectedDimensions;
        ActualDimensions = actualDimensions;
        Model = string.IsNullOrWhiteSpace(model) ? "unknown" : model.Trim();
    }

    public string ErrorCode { get; }

    public int? ExpectedDimensions { get; }

    public int? ActualDimensions { get; }

    public string Model { get; }
}
