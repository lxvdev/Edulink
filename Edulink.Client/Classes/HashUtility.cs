using System;
using System.Security.Cryptography;
using System.Text;

namespace Edulink.Classes
{
    public static class HashUtility
    {
        // I think this is the place where I commented the most
        // Hashes any string using SHA-256
        public static string Hash(string input)
        {
            // Checks if the input is null
            if (string.IsNullOrEmpty(input)) throw new ArgumentException("Input cannot be null or empty.", nameof(input));

            // Really fancy stuff
            using (var sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        // Checks plain text with a hash
        public static bool CompareHash(string input, string hash)
        {
            // Checks if inputs are null
            if (string.IsNullOrEmpty(input)) throw new ArgumentException("Input cannot be null or empty.", nameof(input));
            if (string.IsNullOrEmpty(input)) throw new ArgumentException("Hash cannot be null or empty.", nameof(hash));

            // Hash the input
            string inputHash = Hash(input);

            // Very important
            return SlowEquals(inputHash, hash);
        }

        // Compares two strings
        private static bool SlowEquals(string a, string b)
        {
            // Checks if inputs are null
            if (string.IsNullOrEmpty(a)) throw new ArgumentException("a cannot be null or empty.", nameof(a));
            if (string.IsNullOrEmpty(b)) throw new ArgumentException("b cannot be null or empty.", nameof(b));

            // Compares lengths
            if (a.Length != b.Length)
                return false;

            // Compares byte by byte
            var diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}
