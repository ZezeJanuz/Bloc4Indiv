using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Bloc4.Data.Models
{
    [Index(nameof(Nom), IsUnique = true)]
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Nom { get; set; } = string.Empty;

        // Navigation vers les salariés rattachés à ce service
        public ICollection<Salarie> Salaries { get; set; } = new List<Salarie>();

        public override string ToString() => Nom;
    }
}