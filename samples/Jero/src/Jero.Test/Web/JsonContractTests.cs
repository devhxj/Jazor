using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;
using Furion.ViewEngine;
using Jero.Core.Service;
using Jero.Web;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Jero.Test.Web;

public class JsonContractTests
{
    [Fact]
    public void WebPayload_RoundTripsIdsDatesAndCustomFieldNames()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        WebProtocols.ConfigureJson(options);
        const long id = 9007199254740993;
        var date = new DateTime(2026, 11, 25, 13, 14, 15);
        var payload = new Payload(id, 1, null, date, new JsonObject { ["Tenant_ID"] = "demo" });

        var json = JsonSerializer.SerializeToElement(payload, options);

        Assert.Equal("9007199254740993", json.GetProperty("id").GetString());
        Assert.Equal("1", json.GetProperty("parentId").GetString());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("groupId").ValueKind);
        Assert.Equal("2026-11-25 13:14:15", json.GetProperty("createdAt").GetString());
        Assert.Equal("demo", json.GetProperty("fields").GetProperty("Tenant_ID").GetString());
        var restored = json.Deserialize<Payload>(options)!;
        Assert.Equal(id, restored.Id);
        Assert.Equal(date, restored.CreatedAt);
        Assert.Equal(id, JsonSerializer.Deserialize<long>("9007199254740993", options));
        var offsetDate = new DateTimeOffset(date, TimeSpan.FromHours(9));
        Assert.Equal("2026-11-25 13:14:15", JsonSerializer.SerializeToElement<DateTimeOffset?>(offsetDate, options).GetString());
    }

    [Fact]
    public void TabularResponse_PreservesRowsAndColumnNames()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        WebProtocols.ConfigureJson(options);
        using var table = new DataTable("Results");
        table.Columns.Add("Display_Name", typeof(string));
        table.Rows.Add("测试");
        using var data = new DataSet();
        data.Tables.Add(table);

        var rows = JsonSerializer.SerializeToElement(table, options);
        var tables = JsonSerializer.SerializeToElement(data, options);

        Assert.Equal("测试", rows[0].GetProperty("Display_Name").GetString());
        Assert.Equal("测试", tables.GetProperty("Results")[0].GetProperty("Display_Name").GetString());
    }

    private sealed record Payload(long Id, long? ParentId, long? GroupId, DateTime CreatedAt, JsonObject Fields);

    [Fact]
    public async Task TemplatePreview_ReadsFieldsFromHttpJson()
    {
        var model = JsonSerializer.SerializeToElement(new { User = new { Name = "Jero" } });
        var services = new ServiceCollection();
        services.AddViewEngine();
        using var provider = services.BuildServiceProvider();
        var templates = new SysTemplateService(null!, provider.GetRequiredService<IViewEngine>(), null!);

        Assert.Equal("Hello Jero", await templates.RenderAsync("Hello @(User.Name)", model));
    }
}
