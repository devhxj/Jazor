


using System.Text.Json;
using Jero.Plugin.SaveLife.Service;
using Xunit;
namespace Jero.Test.SaveLife;

public class SaveLifeMaterialEmbeddingResponseParserTests
{
    [Fact]
    public void GetFirstEmbedding_DataObject_ReturnsEmbedding()
    {
        // Arrange
        using var document = JsonDocument.Parse("""
            {
              "data": {
                "embedding": [0.1, "0.2", -0.3],
                "sparse_embedding": {
                  "values": [9, 8, 7]
                }
              }
            }
            """);

        // Act
        var embedding = SaveLifeMaterialEmbeddingResponseParser.GetFirstEmbedding(document.RootElement.GetProperty("data"));

        // Assert
        Assert.Equal([0.1f, 0.2f, -0.3f], embedding);
    }

    [Fact]
    public void GetFirstEmbedding_DataArray_ReturnsFirstEmbedding()
    {
        // Arrange
        using var document = JsonDocument.Parse("""
            {
              "data": [
                {
                  "embedding": [1, 2, 3]
                }
              ]
            }
            """);

        // Act
        var embedding = SaveLifeMaterialEmbeddingResponseParser.GetFirstEmbedding(document.RootElement.GetProperty("data"));

        // Assert
        Assert.Equal([1f, 2f, 3f], embedding);
    }

    [Fact]
    public void GetFirstEmbedding_NestedMultiEmbeddingObject_ReturnsDenseVector()
    {
        // Arrange
        using var document = JsonDocument.Parse("""
            {
              "data": {
                "multi_embedding": {
                  "text": [4, 5, 6]
                }
              }
            }
            """);

        // Act
        var embedding = SaveLifeMaterialEmbeddingResponseParser.GetFirstEmbedding(document.RootElement.GetProperty("data"));

        // Assert
        Assert.Equal([4f, 5f, 6f], embedding);
    }
}
