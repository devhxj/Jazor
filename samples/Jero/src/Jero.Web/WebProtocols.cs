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
    // MVC and SignalR use the same field types and date formats for the browser.
    internal static void ConfigureJson(JsonSerializerOptions options)
    {
        options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // Preserve the wire type of all Int64 IDs, including small and nullable values.
        options.Converters.AddLongConverters();
        const string dateFormat = "yyyy-MM-dd HH:mm:ss";
        // The API localizes UTC DateTime values. A DateTimeOffset retains its own wall-clock time.
        options.Converters.Add(new DateTimeJsonConverter(dateFormat, outputToLocalDateTime: true));
        options.Converters.Add(new DateTimeOffsetJsonConverter(dateFormat));
        options.Converters.AddDataTableConverters();
        options.Converters.AddDataSetConverters();
    }

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
