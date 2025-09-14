using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Bloc4.Data.Models
{
    [Index(nameof(Ville), IsUnique = true)]
    public class Site
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Ville { get; set; } = string.Empty;

        // Navigation vers les salariés affectés à ce site
        public ICollection<Salarie> Salaries { get; set; } = new List<Salarie>();

        public override string ToString() => Ville;
    }
}