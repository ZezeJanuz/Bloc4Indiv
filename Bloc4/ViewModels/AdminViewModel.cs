using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Bloc4.Data.Models;
using Bloc4.Infrastructure;
using Bloc4.Services;

namespace Bloc4.ViewModels
{
    public class AdminViewModel : ObservableObject
    {
        private readonly IDataService _data;
        private readonly IPdfService _pdf;

        private bool _isAuthorized;
        public bool IsAuthorized
        {
            get => _isAuthorized;
            set => SetProperty(ref _isAuthorized, value);
        }

        // Collections pour les DataGrids
        public ObservableCollection<Site> Sites { get; } = new();
        public ObservableCollection<Service> Services { get; } = new();
        public ObservableCollection<Salarie> Salaries { get; } = new();

        // Sélections courantes
        private Site? _selectedSite;
        public Site? SelectedSite
        {
            get => _selectedSite;
            set => SetProperty(ref _selectedSite, value);
        }

        private Service? _selectedService;
        public Service? SelectedService
        {
            get => _selectedService;
            set => SetProperty(ref _selectedService, value);
        }

        private Salarie? _selectedSalarie;
        public Salarie? SelectedSalarie
        {
            get => _selectedSalarie;
            set => SetProperty(ref _selectedSalarie, value);
        }

        // Commandes
        public ICommand LoadAllCommand { get; }
        public ICommand AddSiteCommand { get; }
        public ICommand UpdateSiteCommand { get; }
        public ICommand DeleteSiteCommand { get; }
        public ICommand AddServiceCommand { get; }
        public ICommand UpdateServiceCommand { get; }
        public ICommand DeleteServiceCommand { get; }
        public ICommand AddSalarieCommand { get; }
        public ICommand UpdateSalarieCommand { get; }
        public ICommand DeleteSalarieCommand { get; }
        public ICommand ExportPdfCommand { get; }

        public AdminViewModel(IDataService data, IPdfService pdf)
        {
            _data = data;
            _pdf = pdf;

            LoadAllCommand        = new RelayCommand(async _ => await LoadAllAsync());
            AddSiteCommand        = new RelayCommand(async _ => await AddSiteAsync());
            UpdateSiteCommand     = new RelayCommand(async _ => await UpdateSiteAsync(), _ => SelectedSite != null);
            DeleteSiteCommand     = new RelayCommand(async _ => await DeleteSiteAsync(), _ => SelectedSite != null);

            AddServiceCommand     = new RelayCommand(async _ => await AddServiceAsync());
            UpdateServiceCommand  = new RelayCommand(async _ => await UpdateServiceAsync(), _ => SelectedService != null);
            DeleteServiceCommand  = new RelayCommand(async _ => await DeleteServiceAsync(), _ => SelectedService != null);

            AddSalarieCommand     = new RelayCommand(async _ => await AddSalarieAsync());
            UpdateSalarieCommand  = new RelayCommand(async _ => await UpdateSalarieAsync(), _ => SelectedSalarie != null);
            DeleteSalarieCommand  = new RelayCommand(async _ => await DeleteSalarieAsync(), _ => SelectedSalarie != null);

            ExportPdfCommand      = new RelayCommand(async _ =>
            {
                if (SelectedSalarie != null)
                    await _pdf.GenerateSalariePdfAsync(SelectedSalarie);
            }, _ => SelectedSalarie != null);
        }

        public async Task LoadAllAsync()
        {
            Sites.Clear();
            foreach (var s in await _data.GetSitesAsync()) Sites.Add(s);

            Services.Clear();
            foreach (var s in await _data.GetServicesAsync()) Services.Add(s);

            Salaries.Clear();
            foreach (var s in await _data.SearchSalariesAsync(null, null, null)) Salaries.Add(s);
        }

        private async Task AddSiteAsync()
        {
            var s = new Site { Ville = "Nouveau site" };
            s = await _data.AddSiteAsync(s);
            Sites.Add(s);
        }

        private async Task UpdateSiteAsync()
        {
            if (SelectedSite != null) await _data.UpdateSiteAsync(SelectedSite);
        }

        private async Task DeleteSiteAsync()
        {
            if (SelectedSite != null)
            {
                var toRemove = SelectedSite;
                await _data.DeleteSiteAsync(toRemove.Id);
                Sites.Remove(toRemove);
            }
        }

        private async Task AddServiceAsync()
        {
            var s = new Service { Nom = "Nouveau service" };
            s = await _data.AddServiceAsync(s);
            Services.Add(s);
        }

        private async Task UpdateServiceAsync()
        {
            if (SelectedService != null) await _data.UpdateServiceAsync(SelectedService);
        }

        private async Task DeleteServiceAsync()
        {
            if (SelectedService != null)
            {
                var toRemove = SelectedService;
                await _data.DeleteServiceAsync(toRemove.Id);
                Services.Remove(toRemove);
            }
        }

        private async Task AddSalarieAsync()
        {
            var s = new Salarie
            {
                Nom = "Nouveau",
                Prenom = "Salarié",
                Email = "nouveau.salarie@example.com",
                ServiceId = Services.Count > 0 ? Services[0].Id : 0,
                SiteId = Sites.Count > 0 ? Sites[0].Id : 0
            };
            s = await _data.AddSalarieAsync(s);
            Salaries.Add(s);
        }

        private async Task UpdateSalarieAsync()
        {
            if (SelectedSalarie != null) await _data.UpdateSalarieAsync(SelectedSalarie);
        }

        private async Task DeleteSalarieAsync()
        {
            if (SelectedSalarie != null)
            {
                var toRemove = SelectedSalarie;
                await _data.DeleteSalarieAsync(toRemove.Id);
                Salaries.Remove(toRemove);
            }
        }
    }
}
