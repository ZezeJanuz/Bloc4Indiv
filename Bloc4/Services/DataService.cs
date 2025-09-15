using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bloc4.Data;
using Bloc4.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Bloc4.Services
{
    public class EfDataService : IDataService
    {
        private readonly AppDbContext _db;
        public EfDataService(AppDbContext db) => _db = db;

        // --- Sites ---
        public Task<List<Site>> GetSitesAsync()
            => _db.Sites.OrderBy(s => s.Ville).ToListAsync();

        public async Task<Site> AddSiteAsync(Site site)
        {
            _db.Sites.Add(site);
            await _db.SaveChangesAsync();
            return site;
        }

        public async Task UpdateSiteAsync(Site site)
        {
            _db.Sites.Update(site);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteSiteAsync(int id)
        {
            var entity = await _db.Sites.FindAsync(id);
            if (entity != null)
            {
                _db.Sites.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        // --- Services ---
        public Task<List<Service>> GetServicesAsync()
            => _db.Services.OrderBy(s => s.Nom).ToListAsync();

        public async Task<Service> AddServiceAsync(Service service)
        {
            _db.Services.Add(service);
            await _db.SaveChangesAsync();
            return service;
        }

        public async Task UpdateServiceAsync(Service service)
        {
            _db.Services.Update(service);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteServiceAsync(int id)
        {
            var entity = await _db.Services.FindAsync(id);
            if (entity != null)
            {
                _db.Services.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        // --- Salariés ---
        public Task<List<Salarie>> GetSalariesAsync()
            => _db.Salaries.Include(s => s.Site).Include(s => s.Service)
                           .OrderBy(s => s.Nom).ThenBy(s => s.Prenom)
                           .ToListAsync();

        public Task<Salarie?> GetSalarieByIdAsync(int id, bool includeNavs = false)
        {
            var q = _db.Salaries.AsQueryable();
            if (includeNavs) q = q.Include(s => s.Site).Include(s => s.Service);
            return q.FirstOrDefaultAsync(s => s.Id == id);
        }

        public Task<List<Salarie>> SearchSalariesAsync(string? nomPartiel, int? siteId, int? serviceId)
        {
            var q = _db.Salaries.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nomPartiel))
                q = q.Where(s =>
                    EF.Functions.Like(s.Nom, $"%{nomPartiel}%") ||
                    EF.Functions.Like(s.Prenom, $"%{nomPartiel}%"));

            if (siteId.HasValue)    q = q.Where(s => s.SiteId == siteId);
            if (serviceId.HasValue) q = q.Where(s => s.ServiceId == serviceId);

            return q.Include(s => s.Site).Include(s => s.Service)
                    .OrderBy(s => s.Nom).ThenBy(s => s.Prenom)
                    .ToListAsync();
        }

        public async Task<Salarie> AddSalarieAsync(Salarie salarie)
        {
            _db.Salaries.Add(salarie);
            await _db.SaveChangesAsync();
            return salarie;
        }

        public async Task UpdateSalarieAsync(Salarie salarie)
        {
            _db.Salaries.Update(salarie);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteSalarieAsync(int id)
        {
            var entity = await _db.Salaries.FindAsync(id);
            if (entity != null)
            {
                _db.Salaries.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }
    }
}