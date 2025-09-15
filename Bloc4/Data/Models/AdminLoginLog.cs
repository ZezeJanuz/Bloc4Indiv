using System;

namespace Bloc4.Data.Models
{
    public class AdminLoginLog
    {
        public int Id { get; set; }
        public DateTimeOffset AttemptedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool Success { get; set; }

        // Métadonnées utiles (pas de mot de passe !)
        public string? MachineName { get; set; }
        public string? UserName { get; set; }
        public string? Reason { get; set; } // ex: "Bad password"
    }
}