using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jero.Core;

/// <summary>
/// JSON时间序列化 yyyy-MM-dd HH:mm:ss
/// </summary>
public class ChinaDateTimeConverter() : JsonConverterFactory
{
    protected virtual string DateTimeFormat => "yyyy-MM-dd HH:mm:ss";

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(DateTime) || typeToConvert == typeof(DateTimeOffset);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return typeToConvert == typeof(DateTime)
            ? new DateConverter(DateTimeFormat)
            : new OffsetConverter(DateTimeFormat);
    }

    private sealed class DateConverter(string format) : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Accept ISO timestamps as well as the custom format written by this converter.
            return reader.TryGetDateTime(out var date)
                ? date
                : DateTime.ParseExact(reader.GetString()!, format, CultureInfo.CurrentCulture, DateTimeStyles.RoundtripKind);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(format, CultureInfo.CurrentCulture));
        }
    }

    private sealed class OffsetConverter(string format) : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TryGetDateTimeOffset(out var date)
                ? date
                : DateTimeOffset.ParseExact(reader.GetString()!, format, CultureInfo.CurrentCulture, DateTimeStyles.None);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(format, CultureInfo.CurrentCulture));
        }
    }
}

/// <summary>
/// JSON时间序列化 yyyy-MM-dd HH:mm
/// </summary>
public class ChinaDateTimeConverterHH : ChinaDateTimeConverter
{
    protected override string DateTimeFormat => "yyyy-MM-dd HH:mm";
}

/// <summary>
/// JSON时间序列化 yyyy-MM-dd
/// </summary>
public class ChinaDateTimeConverterDate : ChinaDateTimeConverter
{
    protected override string DateTimeFormat => "yyyy-MM-dd";
}
