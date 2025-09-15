using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Bloc4.Services
{
    /// <summary>
    /// Implémentation simple (mot de passe en clair). Utile pour tests locaux.
    /// </summary>
    public sealed class SimpleAuthService : IAuthService
    {
        private readonly string _adminPassword;
        public SimpleAuthService(string adminPassword = "admin") => _adminPassword = adminPassword;
        public Task<bool> AuthenticateAdminAsync(string password) => Task.FromResult(password == _adminPassword);
    }

    /// <summary>
    /// Utilitaires de hachage de mot de passe via PBKDF2 (SHA-256).
    /// Format de stockage : v1:iterations:saltBase64:hashBase64
    /// </summary>
    public static class PasswordHasher
    {
        public static string GenerateHash(string password, int iterations = 200_000, int saltSize = 16, int keySize = 32)
        {
            if (password is null) throw new ArgumentNullException(nameof(password));

            byte[] salt = RandomNumberGenerator.GetBytes(saltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, keySize);

            return $"v1:{iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrWhiteSpace(stored)) return false;

            var parts = stored.Split(':');
            if (parts.Length != 4 || parts[0] != "v1") return false;
            if (!int.TryParse(parts[1], out int iterations)) return false;

            byte[] salt     = Convert.FromBase64String(parts[2]);
            byte[] expected = Convert.FromBase64String(parts[3]);

            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expected.Length);

            return FixedTimeEquals(actual, expected);
        }

        private static bool FixedTimeEquals(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }

    /// <summary>
    /// Auth basée sur un hash PBKDF2 stocké (voir PasswordHasher.GenerateHash).
    /// </summary>
    public sealed class HashedAuthService : IAuthService
    {
        private readonly string _storedHash; // v1:iterations:salt:hash
        public HashedAuthService(string storedHash) => _storedHash = storedHash ?? throw new ArgumentNullException(nameof(storedHash));
        public Task<bool> AuthenticateAdminAsync(string password) => Task.FromResult(PasswordHasher.Verify(password, _storedHash));
    }
}