using System;
using System.Security.Cryptography;

namespace CliniqueVeterinaire.Helpers
{
    public static class PasswordHasher
    {
        private const string Format = "PBKDF2-SHA256";
        private const int Iterations = 600000;
        private const int SaltSize = 16;
        private const int KeySize = 32;

        public static string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Le mot de passe ne peut pas être vide.", "password");
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                return Format + "$" + Iterations + "$" + Convert.ToBase64String(salt)
                    + "$" + Convert.ToBase64String(pbkdf2.GetBytes(KeySize));
        }

        public static bool IsSupportedHash(string stored)
        {
            if (string.IsNullOrEmpty(stored) || stored.Length > 255) return false;
            string[] parts = stored.Split('$');
            if (parts.Length != 4 || parts[0] != Format || parts[1] != "600000") return false;
            try
            {
                return Convert.FromBase64String(parts[2]).Length == SaltSize
                    && Convert.FromBase64String(parts[3]).Length == KeySize;
            }
            catch (FormatException) { return false; }
        }

        public static bool Verify(string password, string stored)
        {
            if (password == null || !IsSupportedHash(stored)) return false;
            string[] parts = stored.Split('$');
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expected = Convert.FromBase64String(parts[3]);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] actual = pbkdf2.GetBytes(KeySize);
                int difference = 0;
                for (int i = 0; i < KeySize; i++) difference |= actual[i] ^ expected[i];
                return difference == 0;
            }
        }
    }
}
