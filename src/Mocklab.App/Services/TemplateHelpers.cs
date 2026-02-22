using System.Security.Cryptography;

namespace Mocklab.App.Services;

/// <summary>
/// Instance helper methods for Scriban templates, exposed as <c>helpers</c> (e.g. helpers.guid(), helpers.rand_int(1, 100)).
/// Uses RandomNumberGenerator for randomness. Stateless and safe (no file/network access).
/// </summary>
public sealed class TemplateHelpers
{
    private static readonly char[] AlphaNum =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

    private static readonly string[] Adj = { "fast", "silent", "brave", "smart", "lazy", "eager" };
    private static readonly string[] Noun = { "tiger", "eagle", "fox", "panda", "otter", "wolf" };

    public string guid() => Guid.NewGuid().ToString();

    /// <summary>Random integer in [min, maxInclusive]. Uses RandomNumberGenerator.</summary>
    public int rand_int(int min, int maxInclusive)
    {
        if (maxInclusive < min)
            (min, maxInclusive) = (maxInclusive, min);
        return RandomNumberGenerator.GetInt32(min, maxInclusive + 1);
    }

    /// <summary>Cryptographically random alphanumeric string of given length.</summary>
    public string alphanum(int length = 12)
    {
        if (length <= 0) return "";
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);
        var chars = new char[length];
        for (var i = 0; i < length; i++)
            chars[i] = AlphaNum[bytes[i] % AlphaNum.Length];
        return new string(chars);
    }

    /// <summary>Random username: adj_noun + number (e.g. fast_tiger42).</summary>
    public string username()
    {
        var a = Adj[rand_int(0, Adj.Length - 1)];
        var n = Noun[rand_int(0, Noun.Length - 1)];
        var num = rand_int(10, 9999);
        return $"{a}_{n}{num}";
    }

    /// <summary>Random email using username() and optional domain (default example.com).</summary>
    public string email(string? domain = null)
    {
        domain ??= "example.com";
        return $"{username()}@{domain}";
    }
}
