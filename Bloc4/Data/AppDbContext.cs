using Bloc4.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Bloc4.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Site> Sites => Set<Site>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<Salarie> Salaries => Set<Salarie>();

        // Allow DI options but also support parameterless for design-time tools
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public AppDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Default to local SQLite database file in app directory
                optionsBuilder.UseSqlite("Data Source=annuaire.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- SITE ---
            modelBuilder.Entity<Site>(entity =>
            {
                entity.Property(e => e.Ville)
                      .HasMaxLength(120)
                      .IsRequired();
                entity.HasIndex(e => e.Ville).IsUnique();
            });

            // --- SERVICE ---
            modelBuilder.Entity<Service>(entity =>
            {
                entity.Property(e => e.Nom)
                      .HasMaxLength(120)
                      .IsRequired();
                entity.HasIndex(e => e.Nom).IsUnique();
            });

            // --- SALARIE ---
            modelBuilder.Entity<Salarie>(entity =>
            {
                entity.Property(e => e.Nom).HasMaxLength(120).IsRequired();
                entity.Property(e => e.Prenom).HasMaxLength(120).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
                entity.Property(e => e.TelephoneFixe).HasMaxLength(30);
                entity.Property(e => e.TelephonePortable).HasMaxLength(30);

                entity.HasIndex(e => e.Nom);
                entity.HasIndex(e => e.Prenom);
                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasOne(e => e.Service)
                      .WithMany(s => s.Salaries)
                      .HasForeignKey(e => e.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict); // évite suppression en cascade accidentelle

                entity.HasOne(e => e.Site)
                      .WithMany(s => s.Salaries)
                      .HasForeignKey(e => e.SiteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
