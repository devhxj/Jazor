using System.Reflection;
using Microsoft.AspNetCore.Http;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeProjectImportContractTests
{
    [Fact]
    public void ProjectFileImportInput_ExposesSafeOverwriteOption()
    {
        var property = typeof(ImportSaveLifeProjectFileInput).GetProperty("OverwriteExisting");

        Assert.NotNull(property);
        Assert.Equal(typeof(bool), property.PropertyType);
        Assert.False(new ImportSaveLifeProjectFileInput().OverwriteExisting);
    }

    [Fact]
    public void ProjectFileImportValidation_AcceptsV23WorkbookSize()
    {
        const long v23WorkbookSize = 22_597_754;
        var file = new FormFile(Stream.Null, 0, v23WorkbookSize, "file", "clinical-trials-v23.xlsx");
        var validateFile = typeof(SaveLifeProjectImportRecordService)
            .GetMethod("ValidateProjectImportFile", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(validateFile);
        var exception = Record.Exception(() => validateFile.Invoke(null, new object[] { file }));

        Assert.Null(exception);
    }
}
