// Provides the short-lived, one-time visual challenge used before an anonymous sign-in.
// 为匿名登录提供短时、一次性的图形验证码挑战。
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace JazorAdmin.Features.Identity;

public sealed class CaptchaService(IMemoryCache cache)
{
    private const string Alphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";
    private const int CodeLength = 4;
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(3);
    private readonly object sync = new();

    public CaptchaIssue Issue()
    {
        var id = RandomNumberGenerator.GetHexString(16);
        var answer = RandomNumberGenerator.GetString(Alphabet, CodeLength);
        var state = new CaptchaState(answer, RandomNumberGenerator.GetInt32(1, int.MaxValue), DateTimeOffset.UtcNow.Add(Lifetime));
        cache.Set(GetCacheKey(id), state, state.ExpiresAt);
        return new CaptchaIssue(id);
    }

    public bool TryValidate(string? id, string? answer)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(answer))
            return false;

        CaptchaState state;
        lock (sync)
        {
            if (!cache.TryGetValue<CaptchaState>(GetCacheKey(id), out var cached) ||
                cached is null ||
                cached.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                return false;
            }

            state = cached;

            // Consume before comparing so a successful or failed request cannot replay this challenge.
            // 比对前即消费挑战，成功或失败的请求都不能重放该验证码。
            cache.Remove(GetCacheKey(id));
        }

        var normalized = answer.Trim().ToUpperInvariant();
        return normalized.Length == state.Answer.Length &&
               CryptographicOperations.FixedTimeEquals(
                   Encoding.ASCII.GetBytes(normalized),
                   Encoding.ASCII.GetBytes(state.Answer));
    }

    public string? GetImage(string id)
    {
        if (string.IsNullOrWhiteSpace(id) ||
            !cache.TryGetValue<CaptchaState>(GetCacheKey(id), out var state) ||
            state is null ||
            state.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        return RenderSvg(state);
    }

    private static string GetCacheKey(string id) => "jazoradmin:captcha:" + id;

    private static string RenderSvg(CaptchaState state)
    {
        var builder = new StringBuilder("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 144 48\" role=\"img\" aria-label=\"Verification code\">");
        builder.Append("<rect width=\"144\" height=\"48\" rx=\"12\" fill=\"#eaf8f5\"/>");
        builder.Append("<path d=\"M8 14C32 ").Append(9 + state.Seed % 8).Append(" 56 ")
            .Append(20 + state.Seed % 7).Append(" 82 13S118 ").Append(7 + state.Seed % 9).Append(" 136 18\" fill=\"none\" stroke=\"#4fd4b5\" stroke-opacity=\".52\" stroke-width=\"1.5\"/>");
        builder.Append("<path d=\"M5 36C31 ").Append(29 + state.Seed % 9).Append(" 54 ")
            .Append(43 - state.Seed % 7).Append(" 78 35S114 ").Append(43 - state.Seed % 8).Append(" 140 32\" fill=\"none\" stroke=\"#ffb454\" stroke-opacity=\".58\" stroke-width=\"1.25\"/>");

        for (var index = 0; index < state.Answer.Length; index++)
        {
            var rotation = ((state.Seed >> (index * 3)) % 13) - 6;
            var baseline = 32 + ((state.Seed >> (index * 5)) % 5) - 2;
            var color = index is 1 or 3 ? "#1b786c" : "#17485a";
            builder.Append("<text x=\"").Append(17 + (index * 29)).Append("\" y=\"").Append(baseline)
                .Append("\" fill=\"").Append(color).Append("\" font-family=\"Segoe UI, Arial, sans-serif\" font-size=\"25\" font-weight=\"700\" transform=\"rotate(")
                .Append(rotation).Append(' ').Append(17 + (index * 29)).Append(' ').Append(baseline).Append(")\">")
                .Append(WebUtility.HtmlEncode(state.Answer[index].ToString())).Append("</text>");
        }

        return builder.Append("</svg>").ToString();
    }

    private sealed record CaptchaState(string Answer, int Seed, DateTimeOffset ExpiresAt);
}

public sealed record CaptchaIssue(string Id);
