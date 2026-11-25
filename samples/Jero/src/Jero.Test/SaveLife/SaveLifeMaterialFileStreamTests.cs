using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Jero.Core;
using Jero.Core.Service;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeMaterialFileStreamTests
{
    [Fact]
    public void MaxMaterialFileBytes_MatchesTwentyMegabyteClientContract()
    {
        Assert.Equal(20L * 1024 * 1024, SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes);
    }

    [Fact]
    public void ShouldReloadFileRecord_ForIdOnlyMaterialFile_ReturnsTrue()
    {
        var method = typeof(SysFileService)
            .GetMethod("ShouldReloadFileRecord", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        var idOnlyFile = new SysFile { Id = 823225214713925 };
        var loadedFile = new SysFile
        {
            Id = 823225214713925,
            BucketName = "Local",
            FilePath = "upload/save-life/materials",
            FileName = "报名图片_1783147917055_1",
            Suffix = ".jpg"
        };

        Assert.True((bool)method!.Invoke(null, new object[] { idOnlyFile })!);
        Assert.False((bool)method.Invoke(null, new object[] { loadedFile })!);
    }

    [Fact]
    public void SaveLifeFileAccess_UsesNonActionLoadedFileContract()
    {
        var method = typeof(SysFileService).GetMethod(
            nameof(SysFileService.DownloadLoadedFile),
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(method);
        Assert.NotNull(method!.GetCustomAttribute<NonActionAttribute>());
        Assert.Equal(typeof(Task<IActionResult>), method.ReturnType);
        Assert.NotNull(typeof(ISaveLifeFileAccessService).GetMethod(
            nameof(ISaveLifeFileAccessService.DownloadAsync)));
        Assert.NotNull(typeof(ISaveLifeFileAccessService).GetMethod(
            nameof(ISaveLifeFileAccessService.OpenReadAsync)));
    }
}
