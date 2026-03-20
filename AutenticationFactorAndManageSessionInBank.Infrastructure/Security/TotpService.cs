using System.Security.Cryptography;
using System.Text;
using AutenticationFactorAndManageSessionInBank.Application.Abstractions.Security;

namespace AutenticationFactorAndManageSessionInBank.Infrastructure.Security;

public sealed class TotpService : ITotpService
{
    private const int TimeStepSeconds = 30;

    public string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(20);
        return Base32Encode(bytes);
    }

    public bool ValidateCode(string secret, string code, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 6)
        {
            return false;
        }

        var key = Base32Decode(secret);
        var timestep = nowUtc.ToUnixTimeSeconds() / TimeStepSeconds;

        for (var offset = -1; offset <= 1; offset++)
        {
            if (GenerateCode(key, timestep + offset) == code)
            {
                return true;
            }
        }

        return false;
    }

    public string BuildProvisioningUri(string issuer, string email, string secret) =>
        $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits=6&period=30";

    private static string GenerateCode(byte[] key, long timestep)
    {
        Span<byte> timestepBytes = stackalloc byte[8];
        for (var i = 7; i >= 0; i--)
        {
            timestepBytes[i] = (byte)(timestep & 0xFF);
            timestep >>= 8;
        }

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(timestepBytes.ToArray());
        var offset = hash[^1] & 0x0F;
        var binaryCode =
            ((hash[offset] & 0x7F) << 24) |
            ((hash[offset + 1] & 0xFF) << 16) |
            ((hash[offset + 2] & 0xFF) << 8) |
            (hash[offset + 3] & 0xFF);

        return (binaryCode % 1_000_000).ToString("D6");
    }

    private static string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var output = new StringBuilder();
        var bitBuffer = 0;
        var bitCount = 0;

        foreach (var b in data)
        {
            bitBuffer = (bitBuffer << 8) | b;
            bitCount += 8;

            while (bitCount >= 5)
            {
                output.Append(alphabet[(bitBuffer >> (bitCount - 5)) & 0x1F]);
                bitCount -= 5;
            }
        }

        if (bitCount > 0)
        {
            output.Append(alphabet[(bitBuffer << (5 - bitCount)) & 0x1F]);
        }

        return output.ToString();
    }

    private static byte[] Base32Decode(string input)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var cleaned = input.Trim().TrimEnd('=').ToUpperInvariant();
        var bytes = new List<byte>();
        var bitBuffer = 0;
        var bitCount = 0;

        foreach (var c in cleaned)
        {
            var value = alphabet.IndexOf(c);
            if (value < 0)
            {
                continue;
            }

            bitBuffer = (bitBuffer << 5) | value;
            bitCount += 5;

            if (bitCount >= 8)
            {
                bytes.Add((byte)((bitBuffer >> (bitCount - 8)) & 0xFF));
                bitCount -= 8;
            }
        }

        return bytes.ToArray();
    }
}
