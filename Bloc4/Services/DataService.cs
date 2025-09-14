using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bloc4.Data;
using Bloc4.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Bloc4.Services
{
    public interface IDataService
    {
        // Sites
        Task<List<Site>> GetSitesAsync();
        Task<Site> AddSiteAsync(Site s);
        Task UpdateSiteAsync(Site s);
        Task DeleteSiteAsync(int id);

        // Services
        Task<List<Service>> GetServicesAsync();
        Task<Service> AddServiceAsync(Service s);
        Task UpdateServiceAsync(Service s);
        Task DeleteServiceAsync(int id);

        // Salaries & search
        Task<List<Salarie>> SearchSalariesAsync(string? nomPartiel, int? siteId, int? serviceId);
        Task<Salarie> AddSalarieAsync(Salarie s);
        Task UpdateSalarieAsync(Salarie s);
        Task DeleteSalarieAsync(int id);
    }

    public class EfDataService : IDataService
    {
        private readonly AppDbContext _db;
        public EfDataService(AppDbContext db) => _db = db;

        public Task<List<Site>> GetSitesAsync() 
            => _db.Sites.OrderBy(s => s.Ville).ToListAsync();

        public async Task<Site> AddSiteAsync(Site s) { _db.Sites.Add(s); await _db.SaveChangesAsync(); return s; }
        public async Task UpdateSiteAsync(Site s) { _db.Sites.Update(s); await _db.SaveChangesAsync(); }
        public async Task DeleteSiteAsync(int id)
        {
            var entity = await _db.Sites.FindAsync(id);
            if (entity != null) { _db.Sites.Remove(entity); await _db.SaveChangesAsync(); }
        }

        public Task<List<Service>> GetServicesAsync() 
            => _db.Services.OrderBy(s => s.Nom).ToListAsync();

        public async Task<Service> AddServiceAsync(Service s) { _db.Services.Add(s); await _db.SaveChangesAsync(); return s; }
        public async Task UpdateServiceAsync(Service s) { _db.Services.Update(s); await _db.SaveChangesAsync(); }
        public async Task DeleteServiceAsync(int id)
        {
            var entity = await _db.Services.FindAsync(id);
            if (entity != null) { _db.Services.Remove(entity); await _db.SaveChangesAsync(); }
        }

        public Task<List<Salarie>> SearchSalariesAsync(string? nomPartiel, int? siteId, int? serviceId)
        {
            var q = _db.Salaries.AsQueryable();
            if (!string.IsNullOrWhiteSpace(nomPartiel))
                q = q.Where(s => EF.Functions.Like(s.Nom, $"%{nomPartiel}%") || EF.Functions.Like(s.Prenom, $"%{nomPartiel}%"));
            if (siteId.HasValue) q = q.Where(s => s.SiteId == siteId);
            if (serviceId.HasValue) q = q.Where(s => s.ServiceId == serviceId);

            return q.Include(s => s.Site).Include(s => s.Service)
                    .OrderBy(s => s.Nom).ThenBy(s => s.Prenom)
                    .ToListAsync();
        }

        public async Task<Salarie> AddSalarieAsync(Salarie s) { _db.Salaries.Add(s); await _db.SaveChangesAsync(); return s; }
        public async Task UpdateSalarieAsync(Salarie s) { _db.Salaries.Update(s); await _db.SaveChangesAsync(); }
        public async Task DeleteSalarieAsync(int id)
        {
            var entity = await _db.Salaries.FindAsync(id);
            if (entity != null) { _db.Salaries.Remove(entity); await _db.SaveChangesAsync(); }
        }
    }
}
