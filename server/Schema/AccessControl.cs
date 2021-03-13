namespace atlantis
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public class AccessControl : IDisposable {
        readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        public string GetSalt() {
            byte[] salt = new byte[16];
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

        public void Dispose()
        {
            ((IDisposable)rng).Dispose();
        }
    }
}
