using System.Linq;
using System.Threading.Tasks;
using Bloc4.Data.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bloc4.Data
{
    public static class DbInitializer
    {
        public static async Task EnsureCreatedAsync(AppDbContext db, bool seedDemoData = true)
        {
            var hasMigrations = false;
            try { hasMigrations = db.Database.GetMigrations().Any(); } catch { }

            if (hasMigrations)
            {
                // Si des tables du modèle existent mais PAS l'historique des migrations -> base issue de EnsureCreated
                if (db.Database.IsSqlite())
                {
                    var hasHistory = await SqliteTableExistsAsync(db, "__EFMigrationsHistory");
                    var hasModelTables =
                        await SqliteTableExistsAsync(db, "Sites") ||
                        await SqliteTableExistsAsync(db, "Services") ||
                        await SqliteTableExistsAsync(db, "Salaries");

                    if (!hasHistory && hasModelTables)
                    {
                        // Reset pour basculer proprement vers les migrations
                        await db.Database.EnsureDeletedAsync();
                    }
                }

                await db.Database.MigrateAsync();
            }
            else
            {
                // Pas de migrations (premier run) -> créer le schéma
                await db.Database.EnsureCreatedAsync();
            }

            // (Optionnel) WAL pour meilleure concurrence SQLite
            if (db.Database.IsSqlite())
            {
                try { await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;"); } catch { }
            }

            // --- Seed ---
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

        // Helpers
        private static async Task<bool> SqliteTableExistsAsync(AppDbContext db, string table)
        {
            if (!db.Database.IsSqlite()) return true; // pour autres providers, on suppose OK

            var cs = db.Database.GetConnectionString();
            // fallback si null (au cas où)
            if (string.IsNullOrWhiteSpace(cs)) cs = "Data Source=annuaire.db";

            using var conn = new SqliteConnection(cs);
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM sqlite_master WHERE type='table' AND name=$name;";
            cmd.Parameters.AddWithValue("$name", table);
            var result = await cmd.ExecuteScalarAsync();
            return result != null && result != DBNull.Value;
        }
    }
}