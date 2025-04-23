using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace backend.Utilities
{
    public static class EncryptUtility
    {
        public static bool IsBCryptHash(string hash)
        {
            return hash.StartsWith("$2a$") ||
                   hash.StartsWith("$2b$") ||
                   hash.StartsWith("$2x$") ||
                   hash.StartsWith("$2y$");
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (IsBCryptHash(storedHash))
            {
                return BCrypt.Net.BCrypt.Verify(password, storedHash);
            }
            else
            {
                string sha256Hash = HashWithSHA256(password);
                return sha256Hash.Equals(storedHash, StringComparison.OrdinalIgnoreCase);
            }
        }
        public static string MigrateFromSHA256(string password)
        {
            return HashPassword(password);
        }

        private static string HashWithSHA256(string input)
        {
            using var sha256 = SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }
}