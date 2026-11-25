using Xunit;
using Jero.Core;
using System.Text.Json;

namespace Jero.Test.Logging;

public class DatabaseLoggingWriterTests
{
    [Fact]
    public void LoggingJsonSerializer_PreservesMonitorPayloadTypes()
    {
        var monitor = LoggingJsonSerializer.Deserialize("""
            {
                "parameters": [{ "value": { "at": "2026-11-25T12:00:00Z", "items": [2, true, null] } }],
                "returnInformation": { "httpStatusCode": 204 },
                "exception": { "message": "failure" },
                "timeOperationElapsedMilliseconds": 12
            }
            """);

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var requestParam = LoggingJsonSerializer.Deserialize(LoggingJsonSerializer.RequestParam(monitor, options)!);
        Assert.Equal("2026-11-25T12:00:00Z", requestParam.Property("at").GetString());
        Assert.Equal(2, requestParam.Property("items")[0].GetInt32());
        Assert.True(requestParam.Property("items")[1].GetBoolean());
        Assert.Equal(JsonValueKind.Null, requestParam.Property("items")[2].ValueKind);
        Assert.Equal("204", monitor.Property("returnInformation").Property("httpStatusCode").Text());
        Assert.Equal(12L, monitor.Property("timeOperationElapsedMilliseconds").Integer());
        Assert.Equal("failure", monitor.Property("exception").Property("message").GetString());
    }

    [Theory]
    [InlineData("{}", null)]
    [InlineData("""{"parameters":[]}""", null)]
    [InlineData("""{"parameters":[{}]}""", "null")]
    [InlineData("""{"parameters":[{"value":null}]}""", "null")]
    public void LoggingJsonSerializer_DistinguishesMissingParameterFromNullValue(string json, string? expected)
    {
        var monitor = LoggingJsonSerializer.Deserialize(json);

        Assert.Equal(expected, LoggingJsonSerializer.RequestParam(monitor, new JsonSerializerOptions()));
    }

    [Fact]
    public void LoggingJsonSerializer_SerializesThrownExceptionWithInnerDetails()
    {
        static void ThrowWithInner()
        {
            throw new InvalidOperationException("outer", new ArgumentException("inner"));
        }
        var exception = Assert.Throws<InvalidOperationException>(ThrowWithInner);
        var json = LoggingJsonSerializer.SerializeException(exception, new JsonSerializerOptions());
        var result = LoggingJsonSerializer.Deserialize(json);

        Assert.Equal(typeof(InvalidOperationException).FullName, result.Property("ClassName").Text());
        Assert.Equal("outer", result.Property("Message").Text());
        Assert.Contains(nameof(LoggingJsonSerializer_SerializesThrownExceptionWithInnerDetails), result.Property("StackTraceString").Text());
        Assert.Equal("inner", result.Property("InnerException").Property("Message").Text());
    }

    [Fact]
    public async Task ProcessBatchAsync_OneMessageFails_ProcessesRemainingMessages()
    {
        var eventIds = new[] { 1, 2, 3 };
        var processedEventIds = new List<int>();
        var failedEventIds = new List<int>();

        await DatabaseLoggingWriter.ProcessBatchAsync(
            eventIds,
            eventId =>
            {
                processedEventIds.Add(eventId);
                return eventId == 2
                    ? Task.FromException(new InvalidOperationException("Invalid test log."))
                    : Task.CompletedTask;
            },
            (eventId, _) => failedEventIds.Add(eventId));

        Assert.Equal([1, 2, 3], processedEventIds);
        Assert.Equal([2], failedEventIds);
    }
}
