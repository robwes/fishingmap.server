using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

namespace FishingMap.Common.Utils
{
    public static class Cryptography
    {
        // OWASP-recommended work factor for PBKDF2-HMACSHA512 (2023 guidance: 210k).
        // Hashes don't record their iteration count, so old hashes are detected by
        // re-computing with LegacyIterations — see Validate's needsRehash out param.
        private const int Iterations = 210_000;
        private const int LegacyIterations = 10_000;

        public static string CreateHash(string value, string salt)
        {
            return CreateHash(value, salt, Iterations);
        }

        private static string CreateHash(string value, string salt, int iterations)
        {
            var valueBytes = KeyDerivation.Pbkdf2(
                    password: value,
                    salt: Encoding.UTF8.GetBytes(salt),
                    prf: KeyDerivationPrf.HMACSHA512,
                    iterationCount: iterations,
                    numBytesRequested: 256 / 8);

            return Convert.ToBase64String(valueBytes);
        }

        public static string CreateSalt()
        {
            byte[] randomBytes = new byte[128 / 8];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }

        public static bool Validate(string value, string salt, string hash)
        {
            return Validate(value, salt, hash, out _);
        }

        public static bool Validate(string value, string salt, string hash, out bool needsRehash)
        {
            if (FixedTimeEquals(CreateHash(value, salt, Iterations), hash))
            {
                needsRehash = false;
                return true;
            }

            if (FixedTimeEquals(CreateHash(value, salt, LegacyIterations), hash))
            {
                needsRehash = true;
                return true;
            }

            needsRehash = false;
            return false;
        }

        /// <summary>
        /// Fast one-way hash for high-entropy values (refresh tokens) — NOT for
        /// passwords, which must go through the PBKDF2 methods above.
        /// </summary>
        public static string Sha256(string value)
        {
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
        }

        private static bool FixedTimeEquals(string a, string b)
        {
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(a),
                Encoding.UTF8.GetBytes(b));
        }
    }
}
