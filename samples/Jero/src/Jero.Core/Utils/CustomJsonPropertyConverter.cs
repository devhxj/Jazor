


using System.Text.Json;
using System.Text.Json.Serialization;
namespace Jero.Core;

/// <summary>
/// 自定义属性名称转换器
/// </summary>
public class CustomJsonPropertyConverter : JsonConverter<object>
{
    public static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new CustomJsonPropertyConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    // 缓存类型信息避免重复反射
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<PropertyMeta>> PropertyCache = new();

    // 日期时间格式化配置
    private readonly string _dateTimeFormat;

    public CustomJsonPropertyConverter(string dateTimeFormat = "yyyy-MM-dd HH:mm:ss")
    {
        _dateTimeFormat = dateTimeFormat;
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return PropertyCache.GetOrAdd(typeToConvert, type =>
            type.GetProperties()
                .Where(p => p.GetCustomAttribute<CustomJsonPropertyAttribute>() != null)
                .Select(p => new PropertyMeta(p))
                .ToList().AsReadOnly()
        ).Count > 0;
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonDoc = JsonDocument.ParseValue(ref reader);
        var instance = Activator.CreateInstance(typeToConvert)
            ?? throw new InvalidOperationException($"无法创建 {typeToConvert.FullName} 实例");
        var properties = PropertyCache.GetOrAdd(typeToConvert, BuildPropertyMeta);

        foreach (var prop in properties)
        {
            if (jsonDoc.RootElement.TryGetProperty(prop.JsonName, out var value))
            {
                object? propertyValue;

                // 特殊处理日期时间类型
                if (IsDateTimeType(prop.PropertyType))
                {
                    propertyValue = HandleDateTimeValue(value, prop.PropertyType);
                }
                else
                {
                    propertyValue = JsonSerializer.Deserialize(
                        value.GetRawText(),
                        prop.PropertyType,
                        options
                    );
                }

                prop.SetValue(instance, propertyValue);
            }
        }

        return instance;
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        var properties = PropertyCache.GetOrAdd(value.GetType(), BuildPropertyMeta);

        foreach (var prop in properties)
        {
            var propertyValue = prop.GetValue(value);

            writer.WritePropertyName(prop.JsonName);

            // 特殊处理日期时间类型
            if (propertyValue != null && IsDateTimeType(prop.PropertyType))
            {
                writer.WriteStringValue(FormatDateTime(propertyValue));
            }
            else
            {
                JsonSerializer.Serialize(writer, propertyValue, options);
            }
        }

        writer.WriteEndObject();
    }

    private static IReadOnlyList<PropertyMeta> BuildPropertyMeta(Type type)
    {
        return type.GetProperties()
            .Select(p => new PropertyMeta(p))
            .ToList().AsReadOnly();
    }

    private object? HandleDateTimeValue(JsonElement value, Type targetType)
    {
        var dateStr = value.GetString();
        if (string.IsNullOrEmpty(dateStr)) return null;

        var date = DateTime.Parse(dateStr);
        return targetType == typeof(DateTimeOffset)
            ? new DateTimeOffset(date)
            : (object)date;
    }

    private string FormatDateTime(object dateTime)
    {
        return dateTime switch
        {
            DateTime dt => dt.ToString(_dateTimeFormat),
            DateTimeOffset dto => dto.ToString(_dateTimeFormat),
            _ => dateTime?.ToString() ?? string.Empty
        };
    }

    private static bool IsDateTimeType(Type type)
    {
        var actualType = Nullable.GetUnderlyingType(type) ?? type;
        return actualType == typeof(DateTime) || actualType == typeof(DateTimeOffset);
    }

    private class PropertyMeta
    {
        private readonly PropertyInfo _property;
        private readonly Func<object, object?> _getter;
        private readonly Action<object, object?>? _setter;

        public string JsonName { get; }
        public Type PropertyType => _property.PropertyType;

        public PropertyMeta(PropertyInfo property)
        {
            _property = property;
            var declaringType = property.DeclaringType ?? throw new InvalidOperationException($"属性 {property.Name} 缺少声明类型");
            JsonName = property.GetCustomAttribute<CustomJsonPropertyAttribute>()?.Name ?? property.Name;

            // 编译表达式树优化属性访问
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            var valueParam = Expression.Parameter(typeof(object), "value");

            // Getter
            var getterExpr = Expression.Lambda<Func<object, object?>>(
                    Expression.Convert(
                        Expression.Property(
                            Expression.Convert(instanceParam, declaringType),
                            property),
                    typeof(object)),
                instanceParam);
            _getter = getterExpr.Compile();

            // Setter
            if (property.CanWrite)
            {
                var setterExpr = Expression.Lambda<Action<object, object?>>(
                    Expression.Assign(
                        Expression.Property(
                            Expression.Convert(instanceParam, declaringType),
                            property),
                        Expression.Convert(valueParam, property.PropertyType)),
                    instanceParam, valueParam);
                _setter = setterExpr.Compile();
            }
        }

        public object? GetValue(object instance) => _getter(instance);

        public void SetValue(object instance, object? value)
        {
            if (_setter != null)
            {
                _setter(instance, value);
            }
        }
    }
}
