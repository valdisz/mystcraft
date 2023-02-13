namespace advisor;

using System;
using System.Security.Cryptography;
using System.Text;

public interface IAccessControl {
    string ComputeDigest(string salt, string password);
    string GetSalt();
    bool VerifyPassword(string salt, string password, string digest);
}

public class AccessControl : IDisposable, IAccessControl {
    readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

    public string GetSalt() {
        Span<byte> salt = stackalloc byte[18];
        rng.GetNonZeroBytes(salt);

        return Convert.ToBase64String(salt);
    }

    public string ComputeDigest(string salt, string password) {
        var input = salt + password;
        var buffer = Encoding.UTF8.GetBytes(input);

        using var alg = SHA256.Create();
        var digest = alg.ComputeHash(buffer);

        return Convert.ToBase64String(digest);
    }

    public bool VerifyPassword(string salt, string password, string digest)
        => ComputeDigest(salt, password) == digest;

    public void Dispose()
        => rng.Dispose();
}
