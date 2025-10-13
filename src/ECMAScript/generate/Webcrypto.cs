namespace ECMAScript;

/// <summary>
/// Algorithm
/// </summary>
[ECMAScript]
[Description("@#Algorithm")]
public record Algorithm(
    [property: Description("@#name")]string? Name = default);

/// <summary>
/// KeyAlgorithm
/// </summary>
[ECMAScript]
[Description("@#KeyAlgorithm")]
public record KeyAlgorithm(
    [property: Description("@#name")]string? Name = default);

/// <summary>
/// RsaOtherPrimesInfo
/// </summary>
[ECMAScript]
[Description("@#RsaOtherPrimesInfo")]
public record RsaOtherPrimesInfo(
    [property: Description("@#r")]string? R = default,
	[property: Description("@#d")]string? D = default,
	[property: Description("@#t")]string? T = default);

/// <summary>
/// JsonWebKey
/// </summary>
[ECMAScript]
[Description("@#JsonWebKey")]
public record JsonWebKey(
    [property: Description("@#kty")]string? Kty = default,
	[property: Description("@#use")]string? Use = default,
	[property: Description("@#key_ops")]string[]? KeyOps = default,
	[property: Description("@#alg")]string? Alg = default,
	[property: Description("@#ext")]bool Ext = default,
	[property: Description("@#crv")]string? Crv = default,
	[property: Description("@#x")]string? X = default,
	[property: Description("@#y")]string? Y = default,
	[property: Description("@#d")]string? D = default,
	[property: Description("@#n")]string? N = default,
	[property: Description("@#e")]string? E = default,
	[property: Description("@#p")]string? P = default,
	[property: Description("@#q")]string? Q = default,
	[property: Description("@#dp")]string? Dp = default,
	[property: Description("@#dq")]string? Dq = default,
	[property: Description("@#qi")]string? Qi = default,
	[property: Description("@#oth")]RsaOtherPrimesInfo[]? Oth = default,
	[property: Description("@#k")]string? K = default);

/// <summary>
/// CryptoKeyPair
/// </summary>
[ECMAScript]
[Description("@#CryptoKeyPair")]
public record CryptoKeyPair(
    [property: Description("@#publicKey")]CryptoKey? PublicKey = default,
	[property: Description("@#privateKey")]CryptoKey? PrivateKey = default);

/// <summary>
/// RsaKeyGenParams
/// </summary>
[ECMAScript]
[Description("@#RsaKeyGenParams")]
public record RsaKeyGenParams(
    [property: Description("@#modulusLength")]uint ModulusLength = default,
	[property: Description("@#publicExponent")]BigInteger? PublicExponent = default): Algorithm;

/// <summary>
/// RsaHashedKeyGenParams
/// </summary>
[ECMAScript]
[Description("@#RsaHashedKeyGenParams")]
public record RsaHashedKeyGenParams(
    [property: Description("@#hash")]HashAlgorithmIdentifier? Hash = default): RsaKeyGenParams;

/// <summary>
/// RsaKeyAlgorithm
/// </summary>
[ECMAScript]
[Description("@#RsaKeyAlgorithm")]
public record RsaKeyAlgorithm(
    [property: Description("@#modulusLength")]uint ModulusLength = default,
	[property: Description("@#publicExponent")]BigInteger? PublicExponent = default): KeyAlgorithm;

/// <summary>
/// RsaHashedKeyAlgorithm
/// </summary>
[ECMAScript]
[Description("@#RsaHashedKeyAlgorithm")]
public record RsaHashedKeyAlgorithm(
    [property: Description("@#hash")]KeyAlgorithm? Hash = default): RsaKeyAlgorithm;

/// <summary>
/// RsaHashedImportParams
/// </summary>
[ECMAScript]
[Description("@#RsaHashedImportParams")]
public record RsaHashedImportParams(
    [property: Description("@#hash")]HashAlgorithmIdentifier? Hash = default): Algorithm;

/// <summary>
/// RsaPssParams
/// </summary>
[ECMAScript]
[Description("@#RsaPssParams")]
public record RsaPssParams(
    [property: Description("@#saltLength")]uint SaltLength = default): Algorithm;

/// <summary>
/// RsaOaepParams
/// </summary>
[ECMAScript]
[Description("@#RsaOaepParams")]
public record RsaOaepParams(
    [property: Description("@#label")]IBufferSource? Label = default): Algorithm;

/// <summary>
/// EcdsaParams
/// </summary>
[ECMAScript]
[Description("@#EcdsaParams")]
public record EcdsaParams(
    [property: Description("@#hash")]HashAlgorithmIdentifier? Hash = default): Algorithm;

/// <summary>
/// EcKeyGenParams
/// </summary>
[ECMAScript]
[Description("@#EcKeyGenParams")]
public record EcKeyGenParams(
    [property: Description("@#namedCurve")]NamedCurve? NamedCurve = default): Algorithm;

/// <summary>
/// EcKeyAlgorithm
/// </summary>
[ECMAScript]
[Description("@#EcKeyAlgorithm")]
public record EcKeyAlgorithm(
    [property: Description("@#namedCurve")]NamedCurve? NamedCurve = default): KeyAlgorithm;

/// <summary>
/// EcKeyImportParams
/// </summary>
[ECMAScript]
[Description("@#EcKeyImportParams")]
public record EcKeyImportParams(
    [property: Description("@#namedCurve")]NamedCurve? NamedCurve = default): Algorithm;

/// <summary>
/// EcdhKeyDeriveParams
/// </summary>
[ECMAScript]
[Description("@#EcdhKeyDeriveParams")]
public record EcdhKeyDeriveParams(
    [property: Description("@#public")]CryptoKey? Public = default): Algorithm;

/// <summary>
/// AesCtrParams
/// </summary>
[ECMAScript]
[Description("@#AesCtrParams")]
public record AesCtrParams(
    [property: Description("@#counter")]IBufferSource? Counter = default,
	[property: Description("@#length")]byte Length = default): Algorithm;

/// <summary>
/// AesKeyAlgorithm
/// </summary>
[ECMAScript]
[Description("@#AesKeyAlgorithm")]
public record AesKeyAlgorithm(
    [property: Description("@#length")]ushort Length = default): KeyAlgorithm;

/// <summary>
/// AesKeyGenParams
/// </summary>
[ECMAScript]
[Description("@#AesKeyGenParams")]
public record AesKeyGenParams(
    [property: Description("@#length")]ushort Length = default): Algorithm;

/// <summary>
/// AesDerivedKeyParams
/// </summary>
[ECMAScript]
[Description("@#AesDerivedKeyParams")]
public record AesDerivedKeyParams(
    [property: Description("@#length")]ushort Length = default): Algorithm;

/// <summary>
/// AesCbcParams
/// </summary>
[ECMAScript]
[Description("@#AesCbcParams")]
public record AesCbcParams(
    [property: Description("@#iv")]IBufferSource? Iv = default): Algorithm;

/// <summary>
/// AesGcmParams
/// </summary>
[ECMAScript]
[Description("@#AesGcmParams")]
public record AesGcmParams(
    [property: Description("@#iv")]IBufferSource? Iv = default,
	[property: Description("@#additionalData")]IBufferSource? AdditionalData = default,
	[property: Description("@#tagLength")]byte TagLength = default): Algorithm;

/// <summary>
/// HmacImportParams
/// </summary>
[ECMAScript]
[Description("@#HmacImportParams")]
public record HmacImportParams(
    [property: Description("@#hash")]HashAlgorithmIdentifier? Hash = default,
	[property: Description("@#length")]uint Length = default): Algorithm;

/// <summary>
/// HmacKeyAlgorithm
/// </summary>
[ECMAScript]
[Description("@#HmacKeyAlgorithm")]
public record HmacKeyAlgorithm(
    [property: Description("@#hash")]KeyAlgorithm? Hash = default,
	[property: Description("@#length")]uint Length = default): KeyAlgorithm;

/// <summary>
/// HmacKeyGenParams
/// </summary>
[ECMAScript]
[Description("@#HmacKeyGenParams")]
public record HmacKeyGenParams(
    [property: Description("@#hash")]HashAlgorithmIdentifier? Hash = default,
	[property: Description("@#length")]uint Length = default): Algorithm;

/// <summary>
/// HkdfParams
/// </summary>
[ECMAScript]
[Description("@#HkdfParams")]
public record HkdfParams(
    [property: Description("@#hash")]HashAlgorithmIdentifier? Hash = default,
	[property: Description("@#salt")]IBufferSource? Salt = default,
	[property: Description("@#info")]IBufferSource? Info = default): Algorithm;

/// <summary>
/// Pbkdf2Params
/// </summary>
[ECMAScript]
[Description("@#Pbkdf2Params")]
public record Pbkdf2Params(
    [property: Description("@#salt")]IBufferSource? Salt = default,
	[property: Description("@#iterations")]uint Iterations = default,
	[property: Description("@#hash")]HashAlgorithmIdentifier? Hash = default): Algorithm;

/// <summary>
/// Crypto
/// </summary>
[ECMAScript]
[Description("@#Crypto")]
public class Crypto
{
    /// <summary>
    /// subtle
    /// </summary>
    [Description("@#subtle")]
    public extern SubtleCrypto Subtle { get; }

    /// <summary>
    /// getRandomValues
    /// </summary>
    /// <param name="array">array</param>
    [Description("@#getRandomValues")]
    public extern IArrayBufferView GetRandomValues(IArrayBufferView array);

    /// <summary>
    /// randomUUID
    /// </summary>
    [Description("@#randomUUID")]
    public extern string RandomUUID();
}

/// <summary>
/// CryptoKey
/// </summary>
[ECMAScript]
[Description("@#CryptoKey")]
public class CryptoKey
{
    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern KeyType Type { get; }

    /// <summary>
    /// extractable
    /// </summary>
    [Description("@#extractable")]
    public extern bool Extractable { get; }

    /// <summary>
    /// algorithm
    /// </summary>
    [Description("@#algorithm")]
    public extern object Algorithm { get; }

    /// <summary>
    /// usages
    /// </summary>
    [Description("@#usages")]
    public extern object Usages { get; }
}

/// <summary>
/// SubtleCrypto
/// </summary>
[ECMAScript]
[Description("@#SubtleCrypto")]
public class SubtleCrypto
{
    /// <summary>
    /// encrypt
    /// </summary>
    /// <param name="algorithm">algorithm</param>
    /// <param name="key">key</param>
    /// <param name="data">data</param>
    [Description("@#encrypt")]
    public extern PromiseResult<ArrayBuffer> Encrypt(AlgorithmIdentifier algorithm, CryptoKey key, IBufferSource data);

    /// <summary>
    /// decrypt
    /// </summary>
    /// <param name="algorithm">algorithm</param>
    /// <param name="key">key</param>
    /// <param name="data">data</param>
    [Description("@#decrypt")]
    public extern PromiseResult<ArrayBuffer> Decrypt(AlgorithmIdentifier algorithm, CryptoKey key, IBufferSource data);

    /// <summary>
    /// sign
    /// </summary>
    /// <param name="algorithm">algorithm</param>
    /// <param name="key">key</param>
    /// <param name="data">data</param>
    [Description("@#sign")]
    public extern PromiseResult<ArrayBuffer> Sign(AlgorithmIdentifier algorithm, CryptoKey key, IBufferSource data);

    /// <summary>
    /// verify
    /// </summary>
    /// <param name="algorithm">algorithm</param>
    /// <param name="key">key</param>
    /// <param name="signature">signature</param>
    /// <param name="data">data</param>
    [Description("@#verify")]
    public extern PromiseResult<bool> Verify(AlgorithmIdentifier algorithm, CryptoKey key, IBufferSource signature, IBufferSource data);

    /// <summary>
    /// digest
    /// </summary>
    /// <param name="algorithm">algorithm</param>
    /// <param name="data">data</param>
    [Description("@#digest")]
    public extern PromiseResult<ArrayBuffer> Digest(AlgorithmIdentifier algorithm, IBufferSource data);

    /// <summary>
    /// generateKey
    /// </summary>
    /// <param name="algorithm">algorithm</param>
    /// <param name="extractable">extractable</param>
    /// <param name="keyUsages">keyUsages</param>
    [Description("@#generateKey")]
    public extern PromiseResult<Either<CryptoKey, CryptoKeyPair>> GenerateKey(AlgorithmIdentifier algorithm, bool extractable, KeyUsage[] keyUsages);

    /// <summary>
    /// deriveKey
    /// </summary>
    /// <param name="algorithm">algorithm</param>
    /// <param name="baseKey">baseKey</param>
    /// <param name="derivedKeyType">derivedKeyType</param>
    /// <param name="extractable">extractable</param>
    /// <param name="keyUsages">keyUsages</param>
    [Description("@#deriveKey")]
    public extern PromiseResult<CryptoKey> DeriveKey(AlgorithmIdentifier algorithm, CryptoKey baseKey, AlgorithmIdentifier derivedKeyType, bool extractable, KeyUsage[] keyUsages);

    /// <summary>
    /// deriveBits
    /// </summary>
    /// <param name="algorithm">algorithm</param>
    /// <param name="baseKey">baseKey</param>
    /// <param name="length">length</param>
    [Description("@#deriveBits")]
    public extern PromiseResult<ArrayBuffer> DeriveBits(AlgorithmIdentifier algorithm, CryptoKey baseKey, uint? length = null);

    /// <summary>
    /// importKey
    /// </summary>
    /// <param name="format">format</param>
    /// <param name="keyData">keyData</param>
    /// <param name="algorithm">algorithm</param>
    /// <param name="extractable">extractable</param>
    /// <param name="keyUsages">keyUsages</param>
    [Description("@#importKey")]
    public extern PromiseResult<CryptoKey> ImportKey(KeyFormat format, Either<IBufferSource, JsonWebKey> keyData, AlgorithmIdentifier algorithm, bool extractable, KeyUsage[] keyUsages);

    /// <summary>
    /// exportKey
    /// </summary>
    /// <param name="format">format</param>
    /// <param name="key">key</param>
    [Description("@#exportKey")]
    public extern PromiseResult<Either<ArrayBuffer, JsonWebKey>> ExportKey(KeyFormat format, CryptoKey key);

    /// <summary>
    /// wrapKey
    /// </summary>
    /// <param name="format">format</param>
    /// <param name="key">key</param>
    /// <param name="wrappingKey">wrappingKey</param>
    /// <param name="wrapAlgorithm">wrapAlgorithm</param>
    [Description("@#wrapKey")]
    public extern PromiseResult<ArrayBuffer> WrapKey(KeyFormat format, CryptoKey key, CryptoKey wrappingKey, AlgorithmIdentifier wrapAlgorithm);

    /// <summary>
    /// unwrapKey
    /// </summary>
    /// <param name="format">format</param>
    /// <param name="wrappedKey">wrappedKey</param>
    /// <param name="unwrappingKey">unwrappingKey</param>
    /// <param name="unwrapAlgorithm">unwrapAlgorithm</param>
    /// <param name="unwrappedKeyAlgorithm">unwrappedKeyAlgorithm</param>
    /// <param name="extractable">extractable</param>
    /// <param name="keyUsages">keyUsages</param>
    [Description("@#unwrapKey")]
    public extern PromiseResult<CryptoKey> UnwrapKey(KeyFormat format, IBufferSource wrappedKey, CryptoKey unwrappingKey, AlgorithmIdentifier unwrapAlgorithm, AlgorithmIdentifier unwrappedKeyAlgorithm, bool extractable, KeyUsage[] keyUsages);
}