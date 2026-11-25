namespace Jero.Core.Service;

public class WxOpenIdOutput
{
    public string OpenId { get; set; } = string.Empty;
}

public class WxPhoneOutput
{
    public string PhoneNumber { get; set; } = string.Empty;
}

public class GenerateQRImageOutput
{
    /// <summary>
    /// 生成状态
    /// </summary>
    public bool Success { get; set; } = false;

    /// <summary>
    /// 生成图片的绝对路径
    /// </summary>
    public string ImgPath { get; set; } = "";

    /// <summary>
    /// 生成图片的相对路径
    /// </summary>
    public string RelativeImgPath { get; set; } = "";

    /// <summary>
    /// 生成图片的错误信息
    /// </summary>
    public string Message { get; set; } = "";
}