using Furion.JsonSerialization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Data;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Jero.Web;

internal static class WebProtocols
{
    // MVC 与 SignalR 共用浏览器端的数据类型和日期格式。
    internal static void ConfigureJson(JsonSerializerOptions options)
    {
        options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // Int64 统一用十进制文本传输，避免超过 JavaScript 的安全整数范围，并让小值与可空字段使用同一传输类型。
        options.Converters.AddLongConverters();
        const string dateFormat = "yyyy-MM-dd HH:mm:ss";
        // UTC DateTime 转为宿主本地时间，DateTimeOffset 则保留值自身的时钟时间。
        options.Converters.Add(new DateTimeJsonConverter(dateFormat, outputToLocalDateTime: true));
        options.Converters.Add(new DateTimeOffsetJsonConverter(dateFormat));
        options.Converters.AddDataTableConverters();
        options.Converters.AddDataSetConverters();
    }

    // 存储过程结果的表名和列名随查询变化，OpenAPI 按实际 JSON 结构描述行数组和表名映射。
    internal static void ConfigureSchemas(SwaggerGenOptions options)
    {
        options.SchemaGeneratorOptions.CustomTypeMappings[typeof(DataTable)] = CreateDataTableSchema;
        options.SchemaGeneratorOptions.CustomTypeMappings[typeof(DataSet)] = CreateDataSetSchema;
    }

    private static OpenApiSchema CreateDataTableSchema()
    {
        return new OpenApiSchema
        {
            Type = JsonSchemaType.Array,
            Items = CreateDynamicRowSchema(),
            Description = "动态表格数据。数组中的每个对象代表一行数据，属性名为存储过程返回的列名。"
        };
    }

    private static OpenApiSchema CreateDataSetSchema()
    {
        return new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            AdditionalPropertiesAllowed = true,
            AdditionalProperties = CreateDataTableSchema(),
            Description = "动态数据集。属性名为表名，属性值为对应表的行对象数组。"
        };
    }

    private static OpenApiSchema CreateDynamicRowSchema()
    {
        return new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            AdditionalPropertiesAllowed = true,
            AdditionalProperties = new OpenApiSchema(),
            Description = "动态数据行。属性名为列名，属性值为单元格数据。"
        };
    }
}
