using System.Reflection;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeEnrollmentMaterialDownloadApiTests
{
    [Fact]
    public void EnrollmentMaterialEndpoints_UseProtectedBinaryDownloadContracts()
    {
        var detail = typeof(SaveLifeEnrollmentRecordService).GetMethod(
            nameof(SaveLifeEnrollmentRecordService.GetDetailWithMaterials),
            BindingFlags.Instance | BindingFlags.Public);
        var singleDownload = typeof(SaveLifeEnrollmentRecordService).GetMethod(
            nameof(SaveLifeEnrollmentRecordService.DownloadMaterial),
            BindingFlags.Instance | BindingFlags.Public);
        var archiveDownload = typeof(SaveLifeEnrollmentRecordService).GetMethod(
            nameof(SaveLifeEnrollmentRecordService.DownloadMaterialsArchive),
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(detail);
        Assert.NotNull(singleDownload);
        Assert.NotNull(archiveDownload);

        Assert.NotNull(detail!.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Equal(
            "/api/saveLifeEnrollmentRecord/{id}/detailWithMaterials",
            detail.GetCustomAttribute<HttpGetAttribute>()?.Template);
        Assert.Equal(typeof(Task<SaveLifeEnrollmentRecordDetailOutput>), detail.ReturnType);

        AssertDownloadEndpoint(
            singleDownload!,
            "/api/saveLifeEnrollmentRecord/{enrollmentId}/materials/{materialId}/download",
            ["enrollmentId", "materialId"]);
        AssertDownloadEndpoint(
            archiveDownload!,
            "/api/saveLifeEnrollmentRecord/{enrollmentId}/materials/download",
            ["enrollmentId"]);
    }

    [Fact]
    public void EnrollmentMaterialArchive_UsesPrivateStreamingArchiveResult()
    {
        var result = typeof(SaveLifeEnrollmentRecordService).Assembly.GetType(
            "Jero.Plugin.SaveLife.Service.SaveLifeEnrollmentMaterialArchiveResult");

        Assert.NotNull(result);
        Assert.True(typeof(ActionResult).IsAssignableFrom(result));
    }

    [Fact]
    public void EnrollmentContactEndpoint_UsesDedicatedContactContract()
    {
        var contact = typeof(SaveLifeEnrollmentRecordService).GetMethod(
            nameof(SaveLifeEnrollmentRecordService.Contact),
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(contact);
        Assert.Equal(typeof(Task), contact!.ReturnType);
        Assert.Equal("Contact", contact.GetCustomAttribute<ApiDescriptionSettingsAttribute>()?.Name);
        Assert.Equal(
            typeof(ContactSaveLifeEnrollmentRecordInput),
            contact.GetParameters().Single().ParameterType);
    }

    private static void AssertDownloadEndpoint(
        MethodInfo method,
        string route,
        string[] parameterNames)
    {
        Assert.NotNull(method.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Equal(route, method.GetCustomAttribute<HttpGetAttribute>()?.Template);
        Assert.NotNull(method.GetCustomAttribute<SuppressMonitorAttribute>());
        Assert.Contains(
            method.GetCustomAttributes(),
            attribute => attribute.GetType().Name == "NonUnifyAttribute");
        Assert.Equal(typeof(Task<IActionResult>), method.ReturnType);
        Assert.Equal(parameterNames, method.GetParameters().Select(parameter => parameter.Name));
    }
}
