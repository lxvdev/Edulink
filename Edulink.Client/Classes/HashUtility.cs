using System;
using System.Linq;
using System.Security.Cryptography;

namespace Edulink.Classes
{
    public static class HashUtility
    {
        private const int _saltSize = 16; // 128-bit
        private const int _keySize = 32;  // 256-bit
        private const int _iterations = 25000; // Number of iterations

        private const string _hashAlgorithmName = "pbkdf2-sha512"; // Algorithm name for header
        private static readonly HashAlgorithmName _hashAlgorithm = HashAlgorithmName.SHA512; // Algorithm
        private static readonly string _hashFormat = "${0}${1}${2}${3}"; // Format for storing hash, salt, and iterations

        // I think this is the place where I commented the most

        /// <summary>
        /// Hashes a password using PBKDF2 with a randomly generated salt.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The hashed password in a string format.</returns>
        public static string HashPassword(string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password cannot be null or empty.", nameof(password));

                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    byte[] salt = new byte[_saltSize];
                    rng.GetBytes(salt);

                    using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, _iterations, _hashAlgorithm))
                    {
                        byte[] hash = pbkdf2.GetBytes(_keySize);

                        return string.Format(
                            _hashFormat,
                            _hashAlgorithmName,
                            _iterations,
                            Convert.ToBase64String(salt),
                            Convert.ToBase64String(hash)
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while hashing the password.", ex);
            }
        }

        /// <summary>
        /// Verifies a password against a previously hashed password.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <param name="hashedPassword">The previously hashed password.</param>
        /// <returns>True if the password matches; otherwise, false.</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password  cannot be null or empty.", nameof(password));
                if (string.IsNullOrEmpty(hashedPassword)) throw new ArgumentException("Hashed password cannot be null or empty.", nameof(hashedPassword));

                string[] parts = hashedPassword.Split('$');
                parts = parts.Where(part => !string.IsNullOrEmpty(part)).ToArray();

                if (parts.Length != 4 || parts[0] != _hashAlgorithmName) throw new FormatException("Invalid hashed password format.");

                if (!int.TryParse(parts[1], out int iterations)) throw new FormatException("Invalid iteration count in hashed password.");

                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] hash = Convert.FromBase64String(parts[3]);

                using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, _hashAlgorithm))
                {
                    byte[] testHash = pbkdf2.GetBytes(_keySize);
                    return CryptographicEquals(hash, testHash);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while verifying the password.", ex);
            }
        }

        /// <summary>
        /// Securely compares two byte arrays for equality.
        /// </summary>
        private static bool CryptographicEquals(byte[] a, byte[] b)
        {
            try
            {
                if (a == null) throw new ArgumentNullException("Byte array cannot be null.", nameof(a));
                if (b == null) throw new ArgumentNullException("Byte array cannot be null.", nameof(b));


                if (a.Length != b.Length)
                    return false;

                int result = 0;

                for (int i = 0; i < a.Length; i++)
                    result |= a[i] ^ b[i];

                return result == 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred during cryptographic comparison.", ex);
            }
        }
    }
}
