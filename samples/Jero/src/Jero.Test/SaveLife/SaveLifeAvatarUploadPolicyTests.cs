using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAvatarUploadPolicyTests
{
    [Theory]
    [InlineData("avatar.jpg", "image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 })]
    [InlineData("avatar.jpeg", "image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 })]
    [InlineData("avatar.png", "image/png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })]
    public void Validate_WithSupportedImageSignature_AcceptsFile(
        string fileName,
        string contentType,
        byte[] content)
    {
        var file = CreateFile(fileName, contentType, content);

        SaveLifeAvatarUploadPolicy.Validate(file);
    }

    [Fact]
    public void Validate_WithSpoofedPngSignature_RejectsFile()
    {
        var file = CreateFile("avatar.png", "image/png", [0xFF, 0xD8, 0xFF, 0xE0]);

        Assert.ThrowsAny<Exception>(() => SaveLifeAvatarUploadPolicy.Validate(file));
    }

    [Fact]
    public void Validate_WithUnsupportedExtension_RejectsFile()
    {
        var file = CreateFile("avatar.gif", "image/gif", Encoding.ASCII.GetBytes("GIF89a"));

        Assert.ThrowsAny<Exception>(() => SaveLifeAvatarUploadPolicy.Validate(file));
    }

    [Fact]
    public void Validate_WithOversizedImage_RejectsFile()
    {
        var content = new byte[SaveLifeAvatarUploadPolicy.MaxFileBytes + 1];
        content[0] = 0xFF;
        content[1] = 0xD8;
        content[2] = 0xFF;
        var file = CreateFile("avatar.jpg", "image/jpeg", content);

        Assert.ThrowsAny<Exception>(() => SaveLifeAvatarUploadPolicy.Validate(file));
    }

    [Fact]
    public void MiniAppUploadAvatar_RequiresAuthorizationAndMultipartInput()
    {
        var method = typeof(SaveLifeUserProfileService)
            .GetMethod(nameof(SaveLifeUserProfileService.MiniAppUploadAvatar));

        Assert.NotNull(method);
        Assert.NotNull(method!.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Equal(
            "/api/v1/save-life/users/miniapp/avatar",
            method.GetCustomAttribute<HttpPostAttribute>()?.Template);
        Assert.Equal(typeof(Task<SaveLifeMiniAppAvatarOutput>), method.ReturnType);
        Assert.NotNull(method.GetParameters().Single().GetCustomAttribute<FromFormAttribute>());
    }

    private static FormFile CreateFile(string fileName, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType,
        };
    }
}
