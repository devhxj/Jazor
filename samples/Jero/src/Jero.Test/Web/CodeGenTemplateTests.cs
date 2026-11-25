using Furion.ViewEngine;
using Jero.Core;
using Jero.Core.Service;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Jero.Test.Web;

public class CodeGenTemplateTests
{
    [Fact]
    public async Task Generate_UsesJoltTemplatesForPreviewAndBothOutputModes()
    {
        var input = new SysCodeGen { TableName = "StockItem", PagePath = "inventory", NameSpace = "Jero.Application", GenerateType = "100" };
        var archiveFiles = SysCodeGenService.GetGeneratedFiles(input);
        input.GenerateType = "200";
        Assert.Equal(archiveFiles.ToArray(), SysCodeGenService.GetGeneratedFiles(input).ToArray());
        Assert.Equal("/Generated/inventory/StockItem/Index", SysCodeGenService.GeneratedComponent("inventory", "StockItem"));

        input.GenerateType = "210";
        var frontend = SysCodeGenService.GetGeneratedFiles(input);
        Assert.Equal(["Index.jazor.vm", "EditDialog.jazor.vm", "CrudContracts.cs.vm"], frontend.Select(file => file.Template));
        var optionalFrontend = SysCodeGenService.GetGeneratedFiles(input, hasFileTransfer: true);
        Assert.Equal("CrudRuntime.cs.vm", optionalFrontend.Last().Template);
        Assert.All(frontend, file => Assert.StartsWith(Path.Combine("Jero.Web", "App", "Generated", "inventory", "StockItem"), file.RelativePath));
        input.GenerateType = "220";
        Assert.All(SysCodeGenService.GetGeneratedFiles(input), file => Assert.StartsWith(Path.Combine("Jero.Application", "Service", "StockItem"), file.RelativePath));

        var id = new CodeGenConfig { PropertyName = "Id", NetType = "long", ColumnKey = "True" };
        var warehouse = new CodeGenConfig { PropertyName = "Warehouse", NetType = "string", ColumnKey = "True" };
        var name = new CodeGenConfig { PropertyName = "Name", NetType = "string", ColumnComment = "名称", WhetherRequired = "Y", WhetherTable = "Y" };
        var quantity = new CodeGenConfig { PropertyName = "Quantity", NetType = "decimal", ColumnComment = "数量", DefaultValue = "(-2.5)", WhetherTable = "Y", WhetherSortable = "Y" };
        var enabled = new CodeGenConfig { PropertyName = "Enabled", NetType = "bool", ColumnComment = "可用" };
        var arrival = new CodeGenConfig { PropertyName = "Arrival", NetType = "DateTime", ColumnComment = "到货日期", EffectType = "DatePicker", QueryType = "~" };
        var attachment = new CodeGenConfig { PropertyName = "Attachment", NetType = "string", ColumnComment = "附件", EffectType = "Upload_SingleFile" };
        var model = new CustomViewEngine
        {
            ClassName = "StockItem", LowerClassName = "stockItem", NameSpace = "Jero.Application", ProjectLastName = "Application",
            PagePath = "inventory", BusName = "库存",
            TableField = [id, warehouse, name, quantity, enabled, arrival, attachment], PrimaryKeyFieldList = [id, warehouse],
            QueryWhetherList = [name, quantity, enabled, arrival], AddUpdateFieldList = [name, quantity, enabled, arrival, attachment],
            HasLikeQuery = true, ImportFieldList = [name], UploadFieldList = [attachment]
        };
        var services = new ServiceCollection();
        services.AddViewEngine();
        using var provider = services.BuildServiceProvider();
        var engine = provider.GetRequiredService<IViewEngine>();
        var webRoot = FindWebRoot();
        var sources = new Dictionary<string, string>();
        foreach (var file in archiveFiles.Concat(optionalFrontend).DistinctBy(file => file.Template))
        {
            var source = await System.IO.File.ReadAllTextAsync(Path.Combine(webRoot, "template", file.Template), TestContext.Current.CancellationToken);
            sources.Add(file.Template, await SysCodeGenService.RenderSourceAsync(engine, source, model));
        }

        var index = sources["Index.jazor.vm"];
        Assert.Contains("@using Npm.ElementPlus", index);
        Assert.Contains("@bind:get='@(Query.Value.@quantity)'", index);
        Assert.Contains("keys.Set(\"warehouse\", Global.StringValue(row.@warehouse))", index);
        Assert.Contains("@quantity = -2.5", index);
        Assert.Contains("FileTransfer.Download", index);
        var edit = sources["EditDialog.jazor.vm"];
        Assert.Contains("string.IsNullOrWhiteSpace(row.@name)", edit);
        Assert.Contains("@onupdate:modelValue", edit);
        Assert.Contains("FileTransfer.Upload", edit);
        var contracts = sources["CrudContracts.cs.vm"];
        Assert.Contains("namespace Jero.Web.App.Generated.inventory.StockItem;", contracts);
        Assert.Contains("record RowKey(string? @id, string? @warehouse)", contracts);
        Assert.Contains("[ECMAScript(\"@jero/runtime/components/api-client.js\")]", contracts);
        Assert.Contains("[ECMAScript(\"@jero/runtime/generated/inventory/StockItem/file-transfer.js\")]", contracts);
        Assert.Contains("[ECMAScriptModule(\"generated/inventory/StockItem/file-transfer.js\")]", sources["CrudRuntime.cs.vm"]);
        Assert.Contains("u.Id == input.Id && u.Warehouse == input.Warehouse", sources["Service.cs.vm"]);
        Assert.Contains("class PageStockItemInput", sources["Input.cs.vm"]);
    }

    private static string FindWebRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var webRoot = Path.Combine(directory.FullName, "Jero.Web", "wwwroot");
            if (Directory.Exists(webRoot)) return webRoot;
            var workspaceWebRoot = Path.Combine(directory.FullName, "samples", "Jero", "src", "Jero.Web", "wwwroot");
            if (Directory.Exists(workspaceWebRoot)) return workspaceWebRoot;
        }
        throw new DirectoryNotFoundException("Jero.Web/wwwroot");
    }
}
