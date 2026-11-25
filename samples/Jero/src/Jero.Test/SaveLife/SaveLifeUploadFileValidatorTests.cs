using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeUploadFileValidatorTests
{
    [Theory]
    [InlineData(nameof(SaveLifeEnrollmentMaterialService.MiniAppUpload))]
    [InlineData(nameof(SaveLifeEnrollmentMaterialService.MiniAppPersonalUpload))]
    public void MaterialUploadEndpoint_EnforcesTransportFormAndTimeoutBudgets(string methodName)
    {
        var method = typeof(SaveLifeEnrollmentMaterialService).GetMethod(methodName)!;
        var requestSize = Assert.IsAssignableFrom<IRequestSizeLimitMetadata>(
            method.GetCustomAttribute<RequestSizeLimitAttribute>());
        var form = method.GetCustomAttribute<RequestFormLimitsAttribute>();
        var timeout = method.GetCustomAttribute<RequestTimeoutAttribute>();

        Assert.Equal(20L * 1024 * 1024 + 256 * 1024, requestSize.MaxRequestBodySize);
        Assert.NotNull(form);
        Assert.Equal(20L * 1024 * 1024, form!.MultipartBodyLengthLimit);
        Assert.Equal(8, form.ValueCountLimit);
        Assert.Equal(128, form.KeyLengthLimit);
        Assert.Equal(2048, form.ValueLengthLimit);
        Assert.Equal(128, form.MultipartBoundaryLengthLimit);
        Assert.Equal(16, form.MultipartHeadersCountLimit);
        Assert.Equal(8192, form.MultipartHeadersLengthLimit);
        Assert.NotNull(timeout);
        Assert.Equal(TimeSpan.FromSeconds(120), timeout!.Timeout);
    }

    [Fact]
    public void ValidateAndResolveContentType_WithValidPngSignature_ReturnsStandardContentType()
    {
        var file = BuildFormFile(
            "image.png",
            "image/png",
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00]);

        var contentType = SaveLifeUploadFileValidator.ValidateAndResolveContentType(file, ".png");

        Assert.Equal("image/png", contentType);
    }

    [Fact]
    public void ValidateAndResolveContentType_WithSpoofedJpg_RejectsFile()
    {
        var file = BuildFormFile(
            "image.jpg",
            "image/jpeg",
            "<script>alert(1)</script>"u8.ToArray());

        Assert.ThrowsAny<Exception>(() =>
            SaveLifeUploadFileValidator.ValidateAndResolveContentType(file, ".jpg"));
    }

    [Fact]
    public void ValidateAndResolveContentType_WithMismatchedContentType_RejectsFile()
    {
        var file = BuildFormFile(
            "image.png",
            "application/pdf",
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

        Assert.ThrowsAny<Exception>(() =>
            SaveLifeUploadFileValidator.ValidateAndResolveContentType(file, ".png"));
    }

    private static FormFile BuildFormFile(string fileName, string contentType, byte[] bytes)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType,
        };
    }
}
