using System.Collections.Generic;
using System.Threading.Tasks;
using Bloc4.Data.Models;

namespace Bloc4.Services
{
    /// <summary>Abstraction d'accès aux données.</summary>
    public interface IDataService
    {
        // --- Sites ---
        Task<List<Site>> GetSitesAsync();
        Task<Site> AddSiteAsync(Site site);
        Task UpdateSiteAsync(Site site);
        Task DeleteSiteAsync(int id);

        // --- Services ---
        Task<List<Service>> GetServicesAsync();
        Task<Service> AddServiceAsync(Service service);
        Task UpdateServiceAsync(Service service);
        Task DeleteServiceAsync(int id);

        // --- Salariés ---
        Task<List<Salarie>> GetSalariesAsync();
        Task<Salarie?> GetSalarieByIdAsync(int id, bool includeNavs = false);
        Task<List<Salarie>> SearchSalariesAsync(string? nomPartiel, int? siteId, int? serviceId);
        Task<Salarie> AddSalarieAsync(Salarie salarie);
        Task UpdateSalarieAsync(Salarie salarie);
        Task DeleteSalarieAsync(int id);
    }
}