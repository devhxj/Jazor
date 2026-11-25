namespace Jero.Core;

public class CryptogramUtil
{
    private static readonly Lazy<bool> StrongPasswordValue = new(
        () => App.GetConfig<bool>("Cryptogram:StrongPassword"));
    private static readonly Lazy<string> PasswordStrengthValidationValue = new(
        () => App.GetConfig<string>("Cryptogram:PasswordStrengthValidation"));
    private static readonly Lazy<string> PasswordStrengthValidationMsgValue = new(
        () => App.GetConfig<string>("Cryptogram:PasswordStrengthValidationMsg"));
    private static readonly Lazy<string> CryptoTypeValue = new(
        () => App.GetConfig<string>("Cryptogram:CryptoType"));
    private static readonly Lazy<string> PublicKeyValue = new(
        () => App.GetConfig<string>("Cryptogram:PublicKey"));
    private static readonly Lazy<string> PrivateKeyValue = new(
        () => App.GetConfig<string>("Cryptogram:PrivateKey"));

    public static bool StrongPassword => StrongPasswordValue.Value; // 是否开启密码强度验证
    public static string PasswordStrengthValidation => PasswordStrengthValidationValue.Value; // 密码强度验证正则表达式
    public static string PasswordStrengthValidationMsg => PasswordStrengthValidationMsgValue.Value; // 密码强度验证提示
    public static string CryptoType => CryptoTypeValue.Value; // 加密类型
    public static string PublicKey => PublicKeyValue.Value; // 公钥
    public static string PrivateKey => PrivateKeyValue.Value; // 私钥

    public static readonly string SM4_key = "0123456789abcdeffedcba9876543210";
    public static readonly string SM4_iv = "595298c7c6fd271f0402f804c33d3f66";

    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    public static string Encrypt(string plainText)
    {
        if (CryptoType == CryptogramEnum.MD5.ToString())
        {
            return MD5Encryption.Encrypt(plainText);
        }
        else if (CryptoType == CryptogramEnum.SM2.ToString())
        {
            return SM2Encrypt(plainText);
        }
        else if (CryptoType == CryptogramEnum.SM4.ToString())
        {
            return SM4EncryptECB(plainText);
        }
        return plainText;
    }

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="cipherText"></param>
    /// <returns></returns>
    public static string Decrypt(string cipherText)
    {
        if (CryptoType == CryptogramEnum.SM2.ToString())
        {
            return SM2Decrypt(cipherText);
        }
        else if (CryptoType == CryptogramEnum.SM4.ToString())
        {
            return SM4DecryptECB(cipherText);
        }
        return cipherText;
    }

    /// <summary>
    /// SM2加密
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    public static string SM2Encrypt(string plainText)
    {
        return GMUtil.SM2Encrypt(PublicKey, plainText);
    }

    /// <summary>
    /// SM2解密
    /// </summary>
    /// <param name="cipherText"></param>
    /// <returns></returns>
    public static string SM2Decrypt(string cipherText)
    {
        return GMUtil.SM2Decrypt(PrivateKey, cipherText);
    }

    /// <summary>
    /// SM4加密（ECB）
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    public static string SM4EncryptECB(string plainText)
    {
        return GMUtil.SM4EncryptECB(SM4_key, plainText);
    }

    /// <summary>
    /// SM4解密（ECB）
    /// </summary>
    /// <param name="cipherText"></param>
    /// <returns></returns>
    public static string SM4DecryptECB(string cipherText)
    {
        return GMUtil.SM4DecryptECB(SM4_key, cipherText);
    }

    /// <summary>
    /// SM4加密（CBC）
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    public static string SM4EncryptCBC(string plainText)
    {
        return GMUtil.SM4EncryptCBC(SM4_key, SM4_iv, plainText);
    }

    /// <summary>
    /// SM4解密（CBC）
    /// </summary>
    /// <param name="cipherText"></param>
    /// <returns></returns>
    public static string SM4DecryptCBC(string cipherText)
    {
        return GMUtil.SM4DecryptCBC(SM4_key, SM4_iv, cipherText);
    }
}
