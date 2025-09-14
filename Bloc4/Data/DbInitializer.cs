using System.Linq;
using System.Threading.Tasks;
using Bloc4.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Bloc4.Data
{
    /// <summary>
    /// Initialise la base : applique les migrations si présentes, sinon crée le schéma,
    /// puis insère un jeu de données minimal.
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Crée / met à jour le schéma et seed des données de référence.
        /// </summary>
        /// <param name="db">Contexte EF Core</param>
        /// <param name="seedDemoData">Ajoute un salarié de démo si vrai et si la table est vide</param>
        public static async Task EnsureCreatedAsync(AppDbContext db, bool seedDemoData = true)
        {
            // Si des migrations existent => on les applique, sinon on crée le schéma.
            bool hasMigrations;
            try
            {
                hasMigrations = db.Database.GetMigrations().Any(); // sync: OK toutes versions
            }
            catch
            {
                hasMigrations = false;
            }

            if (hasMigrations)
                await db.Database.MigrateAsync();
            else
                await db.Database.EnsureCreatedAsync();

            // SQLite : mode WAL (meilleure concurrence si 2 instances)
            if (db.Database.IsSqlite())
            {
                try { await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;"); } catch { }
            }

            // --- Seed des tables de référence ---
            if (!await db.Sites.AnyAsync())
            {
                db.Sites.AddRange(
                    new Site { Ville = "Paris" },
                    new Site { Ville = "Lyon" },
                    new Site { Ville = "Lille" },
                    new Site { Ville = "Toulouse" },
                    new Site { Ville = "Nantes" }
                );
            }

            if (!await db.Services.AnyAsync())
            {
                db.Services.AddRange(
                    new Service { Nom = "Production" },
                    new Service { Nom = "Comptabilité" },
                    new Service { Nom = "Accueil" },
                    new Service { Nom = "Qualité" },
                    new Service { Nom = "Ressources Humaines" }
                );
            }

            await db.SaveChangesAsync();

            // --- Seed d'un salarié de démo ---
            if (seedDemoData && !await db.Salaries.AnyAsync())
            {
                var service = await db.Services.FirstAsync();
                var site = await db.Sites.FirstAsync();

                db.Salaries.Add(new Salarie
                {
                    Nom = "Durand",
                    Prenom = "Alice",
                    Email = "alice.durand@example.com",
                    TelephoneFixe = "+33 1 23 45 67 89",
                    TelephonePortable = "+33 6 12 34 56 78",
                    ServiceId = service.Id,
                    SiteId = site.Id
                });

                await db.SaveChangesAsync();
            }
        }
    }
}
