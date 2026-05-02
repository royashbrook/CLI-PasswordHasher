using Xunit;
using Microsoft.AspNetCore.Identity;

namespace h.Tests;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    // -- Format / shape ------------------------------------------------------

    [Theory]
    [InlineData("hi")]
    [InlineData("hello")]
    [InlineData("Password123")]
    [InlineData("alongertestpassword")]
    [InlineData("p@ssw0rd!#$%^&*()")]
    [InlineData("a")] // single char
    public void HashPassword_ProducesV3FormatString(string password)
    {
        var hash = _hasher.HashPassword(password);

        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        // v3 layout: [marker(1) + prf(4) + iter(4) + saltLen(4) + salt(16) + subkey(32)] = 61 bytes
        // base64: 4 * ceil(61/3) = 84 chars (no padding stripped here)
        Assert.Equal(84, hash.Length);
        // version 3 marker is 0x01 — base64 of bytes starting with 0x01 always begins with 'A'
        Assert.StartsWith("AQ", hash);
    }

    [Fact]
    public void HashPassword_SameInput_ProducesDifferentOutputs()
    {
        // Salts must be random, so consecutive hashes of the same password
        // must differ. If they're equal something is badly wrong.
        var hash1 = _hasher.HashPassword("test");
        var hash2 = _hasher.HashPassword("test");

        Assert.NotEqual(hash1, hash2);
    }

    // -- Roundtrip -----------------------------------------------------------

    [Theory]
    [InlineData("hi")]
    [InlineData("hello")]
    [InlineData("Password123")]
    [InlineData("alongertestpassword")]
    [InlineData("p@ssw0rd!#$%^&*()")]
    [InlineData("éèê")] // unicode (é è ê)
    public void HashThenVerify_Succeeds(string password)
    {
        var hash = _hasher.HashPassword(password);
        var result = _hasher.VerifyHashedPassword(hash, password);

        Assert.Equal(PasswordVerificationResult.Success, result);
    }

    [Fact]
    public void Verify_WrongPassword_Fails()
    {
        var hash = _hasher.HashPassword("correctpw");
        var result = _hasher.VerifyHashedPassword(hash, "wrongpw");

        Assert.Equal(PasswordVerificationResult.Failed, result);
    }

    [Fact]
    public void Verify_EmptyProvidedPassword_Fails()
    {
        var hash = _hasher.HashPassword("correctpw");
        var result = _hasher.VerifyHashedPassword(hash, "");

        Assert.Equal(PasswordVerificationResult.Failed, result);
    }

    // -- Known-good fixtures (regression) ------------------------------------
    //
    // These are concrete (password, hash) pairs that Verify must always
    // accept. If the format ever drifts (e.g. an attempted "modernize the
    // crypto defaults" PR that breaks backcompat), these break first.
    // Hashes were minted by `dotnet run --project ../h -- <password>`.

    [Theory]
    [InlineData("hello",
                "AQAAAAEAACcQAAAAEKiv0T9vTMrPweYZgdVnTHjvMs5Ud1yXsH+WNXS++HqwrUKrISOgS1DHct0D4irr4Q==")]
    [InlineData("Password123",
                "AQAAAAEAACcQAAAAEKnmf3ANd9NRuio6XM0APsaOvRfh/GC6xj3LPHeX7meBfFXwZ9bmfSjbZUSDxVWekQ==")]
    [InlineData("alongertestpassword",
                "AQAAAAEAACcQAAAAEBJOcxUBL47kXxH/YdFD5j1zfkxu3U9sYFpTVPITLxq8LdfcLT4NlGtpJnLRPL8m7A==")]
    public void Verify_KnownGoodHashes_Succeed(string password, string hash)
    {
        var result = _hasher.VerifyHashedPassword(hash, password);
        Assert.Equal(PasswordVerificationResult.Success, result);
    }

    [Theory]
    [InlineData("hello",
                "AQAAAAEAACcQAAAAEKiv0T9vTMrPweYZgdVnTHjvMs5Ud1yXsH+WNXS++HqwrUKrISOgS1DHcBADBADBAD==")]
    [InlineData("Password123",
                "AQAAAAEAACcQAAAAEKnmf3ANd9NRuio6XM0APsaOvRfh/GC6xj3LPHeX7meBfFXwZ9bmfSjbZBADBADBAD==")]
    public void Verify_TamperedHashes_Fail(string password, string tamperedHash)
    {
        var result = _hasher.VerifyHashedPassword(tamperedHash, password);
        Assert.Equal(PasswordVerificationResult.Failed, result);
    }

    // -- Cross-tool compat ---------------------------------------------------
    //
    // Hashes minted by Get-PasswordHash.ps1 (the pwsh script in
    // frtl/frtl-bc) MUST verify here. This test fixes the v3 byte format
    // as our public contract — anything generating an Identity v3 hash
    // (matching prf=HMACSHA256, iter=10000, salt=16, subkey=32) must
    // verify against the same `h` build.

    [Theory]
    [InlineData("test123",
                "AQAAAAEAACcQAAAAEAzkO99J3wHwgIJGX1bRz7Hd2vRMx/wiAttZEOGJHv0/k6qi/ZqVjHKx+tkWl4i4xg==")]
    public void Verify_PwshGeneratedHash_Succeeds(string password, string pwshHash)
    {
        var result = _hasher.VerifyHashedPassword(pwshHash, password);
        Assert.Equal(PasswordVerificationResult.Success, result);
    }
}
