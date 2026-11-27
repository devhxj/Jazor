using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Jero.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Hosting;

namespace Jero.Web;

internal static class DevelopmentKeys
{
    private const string JwtKey = "JWTSettings:IssuerSigningKey";
    private const string PublicKey = "Cryptogram:PublicKey";
    private const string PrivateKey = "Cryptogram:PrivateKey";

    /// <summary>开发环境仅在未配置相应密钥时加载本机生成的对称 JWT 密钥或 SM2 密钥对。</summary>
    internal static void Apply(IHostEnvironment environment, IConfiguration configuration)
    {
        if (!environment.IsDevelopment())
            return;

        var jwtAlgorithm = configuration["JWTSettings:Algorithm"];
        bool jwtMissing = string.IsNullOrWhiteSpace(configuration[JwtKey])
            && string.IsNullOrWhiteSpace(configuration["JWTSettings:IssuerSigningPrivateKey"])
            && (string.IsNullOrWhiteSpace(jwtAlgorithm) || jwtAlgorithm is "HS256" or "HS384" or "HS512");
        bool sm2Missing = string.IsNullOrWhiteSpace(configuration[PublicKey])
            && string.IsNullOrWhiteSpace(configuration[PrivateKey])
            && configuration["Cryptogram:CryptoType"] == "SM2";
        if (!jwtMissing && !sm2Missing)
            return;

        var id = typeof(DevelopmentKeys).Assembly.GetCustomAttribute<UserSecretsIdAttribute>()!.UserSecretsId;
        // 生成值单独保存在 User Secrets 文件旁，宿主启动与用户编辑 secrets.json 使用不同文件。
        var directory = Path.GetDirectoryName(Microsoft.Extensions.Configuration.UserSecrets.PathHelper.GetSecretsPathFromSecretsId(id))!;
        var keys = ReadOrCreate(Path.Combine(directory, "development-keys.json"));
        var defaults = new Dictionary<string, string?>();
        if (jwtMissing)
            defaults.Add(JwtKey, keys.JwtSigningKey);
        if (sm2Missing)
        {
            defaults.Add(PublicKey, keys.Sm2PublicKey);
            defaults.Add(PrivateKey, keys.Sm2PrivateKey);
        }

        // Furion 读配置前会 Reload，种子数据初始化也走这条路径。
        // 内存配置源经过 Reload 后仍保留这些缺省值，并且不改动已有配置文件。
        ((IConfigurationBuilder)configuration).AddInMemoryCollection(defaults);
    }

    private static Keys ReadOrCreate(string path)
    {
        if (File.Exists(path))
            return Read();

        var (publicKey, privateKey) = GM.GenerateHexKeyPair();
        var keys = new Keys(Convert.ToHexString(RandomNumberGenerator.GetBytes(64)), publicKey, privateKey);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + "." + Guid.NewGuid().ToString("N");
        try
        {
            var options = new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.Write };
            if (!OperatingSystem.IsWindows())
                options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
            using (var stream = new FileStream(temporary, options))
                JsonSerializer.Serialize(stream, keys);
            try
            {
                // 写完整份文件后再原子移动。并发启动时，其余进程读取先成功写入的密钥。
                File.Move(temporary, path);
                return keys;
            }
            catch (IOException) when (File.Exists(path))
            {
                return Read();
            }
        }
        finally
        {
            File.Delete(temporary);
        }

        Keys Read() => JsonSerializer.Deserialize<Keys>(File.ReadAllBytes(path))
            ?? throw new InvalidOperationException("The local Jero development key file is empty.");
    }

    private sealed record Keys(string JwtSigningKey, string Sm2PublicKey, string Sm2PrivateKey);
}
