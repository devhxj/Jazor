namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeMaterialEmbeddingResponseParser
{
    public static float[] GetFirstEmbedding(JsonElement data)
    {
        return data.ValueKind switch
        {
            JsonValueKind.Array => ReadFirstArrayItemEmbedding(data),
            JsonValueKind.Object => ReadEmbeddingObject(data),
            _ => []
        };
    }

    private static float[] ReadFirstArrayItemEmbedding(JsonElement data)
    {
        var direct = ReadFloatArray(data);
        if (direct.Length > 0)
            return direct;

        foreach (var item in data.EnumerateArray())
        {
            var embedding = item.ValueKind == JsonValueKind.Object
                ? ReadEmbeddingObject(item)
                : ReadFloatArray(item);
            if (embedding.Length > 0)
                return embedding;
        }

        return [];
    }

    private static float[] ReadEmbeddingObject(JsonElement element)
    {
        foreach (var propertyName in new[]
        {
            "embedding",
            "multi_embedding",
            "dense",
            "vector",
            "text",
            "image",
            "video"
        })
        {
            if (!element.TryGetProperty(propertyName, out var property))
                continue;

            var embedding = ReadEmbeddingValue(property);
            if (embedding.Length > 0)
                return embedding;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (property.NameEquals("sparse_embedding"))
                continue;

            var embedding = ReadEmbeddingValue(property.Value);
            if (embedding.Length > 0)
                return embedding;
        }

        return [];
    }

    private static float[] ReadEmbeddingValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Array => ReadFirstArrayItemEmbedding(element),
            JsonValueKind.Object => ReadEmbeddingObject(element),
            _ => []
        };
    }

    private static float[] ReadFloatArray(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
            return [];

        var values = new List<float>();
        foreach (var item in element.EnumerateArray())
        {
            var parsed = ReadFloat(item);
            if (!parsed.HasValue)
                return [];

            values.Add(parsed.Value);
        }

        return values.Count > 0 ? values.ToArray() : [];
    }

    private static float? ReadFloat(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Number => value.GetSingle(),
            JsonValueKind.String when float.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => null
        };
    }
}
