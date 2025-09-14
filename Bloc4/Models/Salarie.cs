using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bloc4.Data.Models
{
    [Index(nameof(Nom))]
    [Index(nameof(Prenom))]
    [Index(nameof(Email), IsUnique = true)]
    public class Salarie
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Nom { get; set; } = string.Empty;

        [Required, MaxLength(120)]
        public string Prenom { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? TelephoneFixe { get; set; }

        [MaxLength(30)]
        public string? TelephonePortable { get; set; }

        [Required, MaxLength(200)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // --- Foreign Keys & Navigations ---
        [ForeignKey(nameof(Service))]
        public int ServiceId { get; set; }
        public Service? Service { get; set; }

        [ForeignKey(nameof(Site))]
        public int SiteId { get; set; }
        public Site? Site { get; set; }

        // --- Helpers non mappés ---
        [NotMapped]
        public string NomComplet => $"{Prenom} {Nom}";
    }
}