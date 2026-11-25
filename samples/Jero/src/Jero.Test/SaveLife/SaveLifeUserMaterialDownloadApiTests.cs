using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeUserMaterialDownloadApiTests
{
    [Fact]
    public void DownloadMaterial_UsesProtectedBinaryDownloadContract()
    {
        var method = typeof(SaveLifeUserProfileService).GetMethod(
            nameof(SaveLifeUserProfileService.DownloadMaterial),
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(method);
        Assert.NotNull(method!.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Equal(
            "/api/saveLifeUserProfile/materials/{userId}/{materialId}/download",
            method.GetCustomAttribute<HttpGetAttribute>()?.Template);
        Assert.NotNull(method.GetCustomAttribute<SuppressMonitorAttribute>());
        Assert.Contains(
            method.GetCustomAttributes(),
            attribute => attribute.GetType().Name == "NonUnifyAttribute");
        Assert.Equal(typeof(Task<IActionResult>), method.ReturnType);
        Assert.Equal(
            ["userId", "materialId"],
            method.GetParameters().Select(parameter => parameter.Name));
    }
}
