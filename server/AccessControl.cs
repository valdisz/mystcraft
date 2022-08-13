namespace advisor
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public class AccessControl : IDisposable {
        readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        public string GetSalt() {
            byte[] salt = new byte[18];
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

        public bool VerifyPassword(string salt, string password, string digest) {
            return ComputeDigest(salt, password) == digest;
        }

        public void Dispose() {
            rng.Dispose();
        }
    }
}
