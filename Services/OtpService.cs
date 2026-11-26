using System.Collections.Concurrent;
using System.Security.Cryptography;
using MedicalAppointment.API.Services.Interface;

namespace MedicalAppointment.API.Services;

public class OtpService : IOtpService
{
    private const int MaxFailedAttempts = 5;

    private sealed class OtpEntry
    {
        public required string Code { get; init; }
        public required DateTime ExpiresAtUtc { get; init; }
        public int FailedAttempts { get; set; }
    }

    private readonly ConcurrentDictionary<string, OtpEntry> _store = new();
    private readonly object _syncLock = new();

    private static string BuildKey(string purpose, string identifier) =>
        $"{purpose.Trim().ToLowerInvariant()}:{identifier.Trim().ToLowerInvariant()}";

    public string GenerateOtp(string purpose, string identifier, TimeSpan ttl)
    {
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
        var key = BuildKey(purpose, identifier);

        lock (_syncLock)
        {
            _store[key] = new OtpEntry
            {
                Code = code,
                ExpiresAtUtc = DateTime.UtcNow.Add(ttl),
                FailedAttempts = 0
            };
        }

        return code;
    }

    public bool VerifyAndConsumeOtp(string purpose, string identifier, string otp)
    {
        var key = BuildKey(purpose, identifier);
        var candidate = otp.Trim();

        lock (_syncLock)
        {
            if (!_store.TryGetValue(key, out var entry))
            {
                return false;
            }

            if (DateTime.UtcNow > entry.ExpiresAtUtc)
            {
                _store.TryRemove(key, out _);
                return false;
            }

            if (!CryptographicOperations.FixedTimeEquals(
                    System.Text.Encoding.UTF8.GetBytes(entry.Code),
                    System.Text.Encoding.UTF8.GetBytes(candidate)))
            {
                entry.FailedAttempts++;
                if (entry.FailedAttempts >= MaxFailedAttempts)
                {
                    _store.TryRemove(key, out _);
                }
                return false;
            }

            _store.TryRemove(key, out _);
            return true;
        }
    }
}
